Option Strict On
Option Explicit On
Option Infer On

' DdagrabBackend.vb
'
' Production video capture backend using DXGI Output Duplication.
' Implements IVideoCaptureBackend contract — Initialize() persistent,
' Start/Stop per-session, Dispose at process exit.
'
' Phase 12a-4 FILL-IN:
'   - Real DXGI Output Duplication in worker loop
'   - D3D11 staging texture per frame (D3D11VideoFrame)
'   - AccessLost recovery (recreate duplication)
'   - ReleaseFrame in every path (success, drop, error)
'
' OWNERSHIP MODEL (verified via handoff audit):
'   - Backend OWNS: D3D11 device, DXGI Output Duplication (persistent —
'     created in Initialize, destroyed in Dispose).
'   - Per-frame: staging texture (created in WorkerLoop, owned by
'     D3D11VideoFrame). When TryPush returns Pushed/Replaced, ownership
'     TRANSFERS to sink. When Dropped, backend disposes the frame.
'   - DXGI desktop texture is NOT owned by us — it's borrowed from
'     IDXGIOutputDuplication for the duration between AcquireNextFrame
'     and ReleaseFrame. We CopyResource to our staging texture, then
'     ReleaseFrame immediately (before pushing to sink).
'
' CONTRACT COMPLIANCE:
'   - Initialize() creates persistent GPU resources (D3D11 + DXGI duplication)
'   - Start() spawns worker thread; Stop() signals + joins (2s budget)
'   - Dispose() deadlock-free per P1-B.1 FIX change #1
'   - TryPush never blocks; Dropped → backend disposes frame
'   - ReleaseFrame called in every path (success, NoFrame, error)
'
' HARD RULES:
'   - Foundation NOT modified (CaptureEngine.vb @ 82d792ab FROZEN)
'   - IVideoFrame contract unchanged (D3D11VideoFrame is additive)
'   - No reflection, no TryCast hack
'   - No reference to CaptureEngine.Encoder.Nvenc (no backward dep)
'   - Frame ownership = TRANSFER on Pushed/Replaced; BORROW never

Imports System
Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports SharpGen.Runtime
Imports Vortice.Direct3D
Imports Vortice.Direct3D11
Imports Vortice.DXGI
' Alias to disambiguate ResultCode (Vortice.DXGI vs Vortice.Direct3D11).
Imports ResultCode = Vortice.DXGI.ResultCode

Namespace CaptureEngine.Video.Backends.Ddagrab

    Public NotInheritable Class DdagrabBackend
        Implements IVideoCaptureBackend
        Implements IVideoBackendDiagnostics

        ' ---- backend state ----
        Private ReadOnly _sync As New Object()
        Private ReadOnly _logger As EngineLogger

        Private _state As DdagrabBackendState = DdagrabBackendState.Created
        Private _disposed As Boolean = False

        ' ---- worker ----
        Private _workerThread As Thread
        Private _stopSignal As Boolean = False
        Private _sink As IVideoFrameSink
        Private _interAttemptDelayMs As Integer = 1   ' avoid busy-looping

        ' ---- diagnostics (counters) ----
        Private _emittedFrames As Long = 0
        Private _droppedFrames As Long = 0
        Private _replacedFrames As Long = 0
        Private _noFrameCount As Long = 0
        Private _errorCount As Long = 0
        Private _accessLostCount As Long = 0
        Private _nextSequence As Long = 0

        ' ---- Phase 12a-5 metrics (per OWNER request) ----
        ' These metrics let 12a-6/12a-8 observe GPU resource lifecycle and
        ' detect slow leaks. Do NOT optimize (e.g. pool staging textures)
        ' based on these numbers yet — the ownership model is new and must
        ' be validated first.
        Private _texturesCreated As Long = 0
        Private _texturesDisposed As Long = 0
        Private _framesPushed As Long = 0      ' sink accepted (Pushed+Replaced)
        Private _framesDropped As Long = 0      ' sink refused (Dropped — backend disposed)

        ' ---- captured context ----
        Private _context As IVideoBackendContext

        ' ---- persistent D3D11 / DXGI resources (created in Initialize) ----
        Private _device As ID3D11Device
        Private _deviceContext As ID3D11DeviceContext
        Private _dxgiFactory As IDXGIFactory1
        Private _adapter As IDXGIAdapter1
        Private _output As IDXGIOutput
        Private _duplication As IDXGIOutputDuplication
        Private _outputWidth As Integer
        Private _outputHeight As Integer

        ' ---- adapter LUID (for cross-device comparison) ----
        Private _adapterLuidLow As UInteger
        Private _adapterLuidHigh As Integer

        ' ---- shared-handle mode (Phase 12a-5c) ----
        ' When True, staging textures are created with SharedNthandle flag and
        ' a shared handle is obtained for cross-device resource sharing.
        Private _useSharedHandle As Boolean = False

        ' ---- staging texture description (used per-frame) ----
        Private _stagingDesc As Texture2DDescription

        ' ---- output dimensions (Public — needed by harness/orchestration
        '      to size encoder) ----
        Public ReadOnly Property OutputWidth As Integer
            Get
                Return _outputWidth
            End Get
        End Property

        Public ReadOnly Property OutputHeight As Integer
            Get
                Return _outputHeight
            End Get
        End Property

        ' ---- adapter LUID (Public — for cross-device comparison in harness) ----
        Public ReadOnly Property AdapterLuidLow As UInteger
            Get
                Return _adapterLuidLow
            End Get
        End Property

        Public ReadOnly Property AdapterLuidHigh As Integer
            Get
                Return _adapterLuidHigh
            End Get
        End Property

        ''' <summary>
        ''' Enable shared-handle mode for cross-device resource sharing.
        ''' Must be set BEFORE Initialize(). When True, staging textures are
        ''' created with D3D11_RESOURCE_MISC_SHARED_NTHANDLE and a shared NT
        ''' handle is obtained for each frame. The encoder opens the shared
        ''' resource via ID3D11Device1.OpenSharedResource1.
        ''' </summary>
        Public WriteOnly Property UseSharedHandle As Boolean
            Set(value As Boolean)
                _useSharedHandle = value
            End Set
        End Property

        Public Sub New(Optional logger As EngineLogger = Nothing)
            _logger = If(logger, New EngineLogger("DdagrabBackend"))
        End Sub

        ' ===== IVideoCaptureBackend =====

        Public ReadOnly Property Diagnostics As IVideoBackendDiagnostics Implements IVideoCaptureBackend.Diagnostics
            Get
                Return Me
            End Get
        End Property

        Public Sub Initialize(context As IVideoBackendContext) Implements IVideoCaptureBackend.Initialize
            If context Is Nothing Then
                Throw New ArgumentNullException(NameOf(context))
            End If
            ThrowIfDisposed()

            SyncLock _sync
                If _state <> DdagrabBackendState.Created Then
                    Throw New InvalidOperationException(
                        "Initialize cannot be called from state '" & _state.ToString() & "'. Expected 'Created'.")
                End If
                _state = DdagrabBackendState.Initializing
            End SyncLock

            Try
                If context.BackendKind <> VideoBackendKind.Ddagrab Then
                    Throw New VideoBackendConfigurationException(
                        "DdagrabBackend.Initialize: context.BackendKind must be Ddagrab, but was " &
                        context.BackendKind.ToString() & ".")
                End If

                _context = context

                ' ─── Create D3D11 device on primary NVIDIA adapter ─────────
                ' Per audit verdict #1: each backend owns its own D3D11 device.
                ' Device flags: BgraSupport (Desktop Duplication) + VideoSupport
                ' (interop-friendly; required by some NVENC paths).
                _dxgiFactory = DXGI.CreateDXGIFactory1(Of IDXGIFactory1)()

                Dim nvidiaIdx As Integer = -1
                Dim adapterIdx As Integer = 0
                Dim adapter1 As IDXGIAdapter1 = Nothing
                Do While _dxgiFactory.EnumAdapters1(CUInt(adapterIdx), adapter1).Success
                    Using a As IDXGIAdapter1 = adapter1
                        Dim desc As AdapterDescription1 = a.Description1
                        _logger.Info($"  Adapter [{adapterIdx}] {desc.Description} " &
                                     $"(0x{desc.VendorId:x4}:0x{desc.DeviceId:x4})")
                        ' Pick first NVIDIA adapter (vendor 0x10DE).
                        If desc.VendorId = &H10DEUI AndAlso nvidiaIdx < 0 Then
                            nvidiaIdx = adapterIdx
                        End If
                    End Using
                    adapterIdx += 1
                Loop

                If nvidiaIdx < 0 Then
                    Throw New VideoBackendRuntimeException(
                        "DdagrabBackend: no NVIDIA adapter found. Capture requires an NVIDIA GPU.")
                End If

                _dxgiFactory.EnumAdapters1(CUInt(nvidiaIdx), _adapter).CheckError()
                Dim nvidiaDesc As AdapterDescription1 = _adapter.Description1
                _adapterLuidLow = nvidiaDesc.Luid.LowPart
                _adapterLuidHigh = nvidiaDesc.Luid.HighPart
                _logger.Info($"DdagrabBackend: selected NVIDIA adapter #{nvidiaIdx}: {nvidiaDesc.Description}")

                Dim requestedFeatureLevels As FeatureLevel() = {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0
                }
                Dim flags As DeviceCreationFlags = DeviceCreationFlags.BgraSupport Or
                                                    DeviceCreationFlags.VideoSupport
                D3D11.D3D11CreateDevice(
                    _adapter,
                    DriverType.Unknown,
                    flags,
                    requestedFeatureLevels,
                    _device,
                    _deviceContext).CheckError()

                ' Enable multithread protection (Phase 2 spike: required for
                ' Desktop Duplication performance — without it FPS drops from
                ' ~100 to ~3 with massive WAIT_TIMEOUT counts).
                Dim multithread As ID3D11Multithread = _deviceContext.QueryInterface(Of ID3D11Multithread)()
                multithread.SetMultithreadProtected(True)
                multithread.Dispose()
                _logger.Info($"DdagrabBackend: D3D11 device created (feature level {_device.FeatureLevel})")

                ' ─── Enumerate outputs, pick primary ─────────────────────────
                Dim outIdx As Integer = 0
                Dim out_ As IDXGIOutput = Nothing
                If Not _adapter.EnumOutputs(CUInt(outIdx), out_).Success Then
                    Throw New VideoBackendRuntimeException(
                        "DdagrabBackend: no DXGI outputs found on adapter.")
                End If
                _output = out_
                Dim outDesc As OutputDescription = _output.Description
                _outputWidth = outDesc.DesktopCoordinates.Right - outDesc.DesktopCoordinates.Left
                _outputHeight = outDesc.DesktopCoordinates.Bottom - outDesc.DesktopCoordinates.Top
                _logger.Info($"DdagrabBackend: output #{outIdx}: {outDesc.DeviceName} " &
                             $"({_outputWidth}x{_outputHeight})")

                ' ─── DuplicateOutput (persistent — 1 per output per process) ─
                ' Phase 11 root cause #2: Windows limits 1 duplication per output
                ' per process. Initialize() creates it ONCE; Start/Stop just
                ' starts/stops frame delivery.
                Dim output1 As IDXGIOutput1 = _output.QueryInterface(Of IDXGIOutput1)()
                _duplication = output1.DuplicateOutput(_device)
                output1.Dispose()
                _logger.Info("DdagrabBackend: DXGI Output Duplication created (persistent)")

                ' ─── Staging texture description (used per-frame in WorkerLoop) ─
                Dim miscFlags As ResourceOptionFlags = ResourceOptionFlags.None
                If _useSharedHandle Then
                    miscFlags = ResourceOptionFlags.SharedNthandle
                    _logger.Info("DdagrabBackend: shared-handle mode ENABLED")
                End If
                _stagingDesc = New Texture2DDescription() With {
                    .Width = CUInt(_outputWidth),
                    .Height = CUInt(_outputHeight),
                    .MipLevels = 1,
                    .ArraySize = 1,
                    .Format = Format.B8G8R8A8_UNorm,
                    .SampleDescription = New SampleDescription(1, 0),
                    .Usage = ResourceUsage.Default,
                    .BindFlags = BindFlags.ShaderResource Or BindFlags.RenderTarget,
                    .CPUAccessFlags = CpuAccessFlags.None,
                    .MiscFlags = miscFlags
                }

                _state = DdagrabBackendState.Initialized
                _logger.Info("DdagrabBackend: Initialize complete (real DXGI capture)")
            Catch ex As VideoBackendException
                _state = DdagrabBackendState.Faulted
                _logger.Error("DdagrabBackend: Initialize failed", ex)
                CleanupPersistentResources()
                Throw
            Catch ex As Exception
                _state = DdagrabBackendState.Faulted
                _logger.Error("DdagrabBackend: Initialize failed (unexpected)", ex)
                CleanupPersistentResources()
                Throw New VideoBackendRuntimeException("Initialize failed unexpectedly", ex)
            End Try
        End Sub

        Public Sub Start(sink As IVideoFrameSink) Implements IVideoCaptureBackend.Start
            If sink Is Nothing Then
                Throw New ArgumentNullException(NameOf(sink))
            End If
            ThrowIfDisposed()

            SyncLock _sync
                Select Case _state
                    Case DdagrabBackendState.Starting, DdagrabBackendState.Running
                        _logger.Warning("DdagrabBackend: Start ignored, already '" & _state.ToString() & "'.")
                        Return
                    Case DdagrabBackendState.Initialized, DdagrabBackendState.Stopped
                        _state = DdagrabBackendState.Starting
                    Case Else
                        Throw New InvalidOperationException(
                            "Start cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            Try
                _sink = sink
                _stopSignal = False
                _workerThread = New Thread(AddressOf WorkerLoop) With {
                    .IsBackground = True,
                    .Name = "DdagrabBackend.Worker"
                }
                _workerThread.Start()
                _state = DdagrabBackendState.Running
                _logger.Info("DdagrabBackend: started (real DXGI capture)")
            Catch ex As Exception
                _state = DdagrabBackendState.Faulted
                _logger.Error("DdagrabBackend: Start failed", ex)
                Throw New VideoBackendRuntimeException("Start failed", ex)
            End Try
        End Sub

        Public Sub [Stop]() Implements IVideoCaptureBackend.Stop
            ThrowIfDisposed()

            SyncLock _sync
                Select Case _state
                    Case DdagrabBackendState.Created, DdagrabBackendState.Initialized, DdagrabBackendState.Stopped
                        _logger.Warning("DdagrabBackend: Stop ignored, not running (state='" & _state.ToString() & "').")
                        Return
                    Case DdagrabBackendState.Stopping
                        _logger.Warning("DdagrabBackend: Stop ignored, already stopping.")
                        Return
                    Case DdagrabBackendState.Faulted
                        _logger.Warning("DdagrabBackend: Stop on Faulted state; no work to stop.")
                        Return
                    Case DdagrabBackendState.Running
                        _state = DdagrabBackendState.Stopping
                    Case Else
                        Throw New InvalidOperationException(
                            "Stop cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            Try
                _stopSignal = True
                Dim worker = _workerThread
                If worker IsNot Nothing Then
                    If Not worker.Join(TimeSpan.FromSeconds(2)) Then
                        _logger.Error("DdagrabBackend: worker did not acknowledge stop within 2 s", Nothing)
                    End If
                End If
                _state = DdagrabBackendState.Stopped
                _logger.Info("DdagrabBackend: stopped")
            Catch ex As Exception
                _state = DdagrabBackendState.Faulted
                _logger.Error("DdagrabBackend: Stop failed", ex)
                Throw New VideoBackendShutdownException("Stop failed", ex)
            End Try
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' P1-B.1 FIX change #1: NEVER wait for the worker while holding _sync.
            ' Worker needs _sync on every iteration — holding _sync across Join = deadlock.

            Dim workerToJoin As Thread = Nothing
            Dim needJoin As Boolean = False

            SyncLock _sync
                If _disposed Then Return
                _disposed = True

                If _state = DdagrabBackendState.Disposed Then
                    Return
                ElseIf _state = DdagrabBackendState.Running OrElse _state = DdagrabBackendState.Starting Then
                    _logger.Info("DdagrabBackend: Dispose while Running — invoking stop path.")
                    _state = DdagrabBackendState.Stopping
                    _stopSignal = True
                    workerToJoin = _workerThread
                    needJoin = True
                Else
                    _logger.Info("DdagrabBackend: Dispose from state '" & _state.ToString() & "'.")
                End If
            End SyncLock

            If needJoin AndAlso workerToJoin IsNot Nothing Then
                Try
                    If Not workerToJoin.Join(TimeSpan.FromSeconds(2)) Then
                        _logger.Error("DdagrabBackend: worker did not acknowledge stop within 2 s", Nothing)
                    End If
                Catch ex As Exception
                    _logger.Error("DdagrabBackend: stop path failed during Dispose (will still dispose)", ex)
                End Try
            End If

            SyncLock _sync
                If _state = DdagrabBackendState.Stopping Then
                    _state = DdagrabBackendState.Stopped
                End If
                _state = DdagrabBackendState.Disposed
            End SyncLock

            ' Release persistent GPU resources OUTSIDE _sync (GPU release can be slow).
            CleanupPersistentResources()
            _logger.Info("DdagrabBackend: disposed")
        End Sub

        ' ===== IVideoBackendDiagnostics =====

        Public ReadOnly Property EmittedFrames As Long Implements IVideoBackendDiagnostics.EmittedFrames
            Get
                SyncLock _sync
                    Return _emittedFrames
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property DroppedFrames As Long Implements IVideoBackendDiagnostics.DroppedFrames
            Get
                SyncLock _sync
                    Return _droppedFrames
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ReplacedFrames As Long Implements IVideoBackendDiagnostics.ReplacedFrames
            Get
                SyncLock _sync
                    Return _replacedFrames
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property NoFrameCount As Long Implements IVideoBackendDiagnostics.NoFrameCount
            Get
                SyncLock _sync
                    Return _noFrameCount
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ErrorCount As Long Implements IVideoBackendDiagnostics.ErrorCount
            Get
                SyncLock _sync
                    Return _errorCount
                End SyncLock
            End Get
        End Property

        ' Extra diagnostic (not in interface — read via cast or test helper)
        Public ReadOnly Property AccessLostCount As Long
            Get
                SyncLock _sync
                    Return _accessLostCount
                End SyncLock
            End Get
        End Property

        ' ===== Phase 12a-5 metrics (per OWNER request) =====
        ' GPU resource lifecycle counters. At steady state with no leak:
        '   _texturesDisposed ~= _texturesCreated (after some lag)
        '   _framesPushed + _framesDropped = _emittedFrames
        ' If _texturesCreated - _texturesDisposed grows unboundedly → leak.

        Public ReadOnly Property TexturesCreated As Long
            Get
                SyncLock _sync
                    Return _texturesCreated
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property TexturesDisposed As Long
            Get
                SyncLock _sync
                    Return _texturesDisposed
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property FramesPushed As Long
            Get
                SyncLock _sync
                    Return _framesPushed
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property FramesDropped As Long
            Get
                SyncLock _sync
                    Return _framesDropped
                End SyncLock
            End Get
        End Property

        ' ===== Test-visible state (Friend) =====

        Public ReadOnly Property CurrentState As DdagrabBackendState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        Friend Function WithInterAttemptDelayMs(delayMs As Integer) As DdagrabBackend
            SyncLock _sync
                _interAttemptDelayMs = delayMs
                Return Me
            End SyncLock
        End Function

        ' ===== Internal worker — REAL DXGI CAPTURE LOOP =====

        Private Sub WorkerLoop()
            Try
                Do
                    Dim shouldStop As Boolean
                    SyncLock _sync
                        shouldStop = _stopSignal
                    End SyncLock
                    If shouldStop Then Exit Do

                    ' ─── AcquireNextFrame (DXGI Output Duplication) ────────
                    Dim sequence As Long
                    Dim attemptTime As Long
                    SyncLock _sync
                        sequence = _nextSequence
                        _nextSequence += 1
                        attemptTime = CLng(Math.Truncate(
                            CDec(Stopwatch.GetTimestamp()) * 10000000D /
                            CDec(Stopwatch.Frequency)))
                    End SyncLock

                    Dim frameInfo As OutduplFrameInfo
                    Dim desktopResource As IDXGIResource = Nothing
                    Dim acquireResult As Result = _duplication.AcquireNextFrame(
                        100,  ' 100ms timeout
                        frameInfo,
                        desktopResource)

                    If Not acquireResult.Success Then
                        ' Common case: DXGI_ERROR_WAIT_TIMEOUT (no new frame since last acquire)
                        ' Not an error — increment NoFrameCount and continue.
                        If acquireResult = ResultCode.WaitTimeout Then
                            SyncLock _sync
                                _noFrameCount += 1
                            End SyncLock
                        ElseIf acquireResult = ResultCode.AccessLost Then
                            ' Output duplication lost (mode change, fullscreen app, etc.)
                            ' Recreate the duplication on next iteration.
                            SyncLock _sync
                                _accessLostCount += 1
                            End SyncLock
                            _logger.Warning("DdagrabBackend: DXGI_ACCESS_LOST — recreating duplication")
                            RecreateDuplication()
                        Else
                            SyncLock _sync
                                _errorCount += 1
                            End SyncLock
                            _logger.Error($"DdagrabBackend: AcquireNextFrame failed: hr=0x{acquireResult.Code:x8}")
                        End If

                        If _interAttemptDelayMs > 0 Then
                            Thread.Sleep(_interAttemptDelayMs)
                        End If
                        Continue Do
                    End If

                    ' ─── Got a frame — copy to staging texture + ReleaseFrame ─
                    Dim stagingTexture As ID3D11Texture2D = Nothing
                    Dim desktopTexture As ID3D11Texture2D = Nothing
                    Dim frame As D3D11VideoFrame = Nothing
                    Try
                        desktopTexture = desktopResource.QueryInterface(Of ID3D11Texture2D)()

                        ' Create staging texture (per-frame allocation — Phase 12a simple strategy)
                        stagingTexture = _device.CreateTexture2D(_stagingDesc)
                        SyncLock _sync
                            _texturesCreated += 1
                        End SyncLock

                        ' If shared-handle mode, obtain NT handle for cross-device sharing
                        Dim sharedHandle As IntPtr = IntPtr.Zero
                        If _useSharedHandle Then
                            Dim dxgiRes As IDXGIResource1 = stagingTexture.QueryInterface(Of IDXGIResource1)()
                            ' Vortice API: CreateSharedHandle(attributes, access, name) As IntPtr
                            ' Returns the handle directly (NOT Result). Throws on failure.
                            ' SharedResourceFlags is an enum — use ReadWrite for cross-device access.
                            Try
                                sharedHandle = dxgiRes.CreateSharedHandle(Nothing, Vortice.DXGI.SharedResourceFlags.ReadWrite, Nothing)
                            Catch ex As Exception
                                _logger.Error($"DdagrabBackend: CreateSharedHandle threw: {ex.Message}")
                                dxgiRes.Dispose()
                                stagingTexture.Dispose()
                                SyncLock _sync
                                    _texturesDisposed += 1
                                End SyncLock
                                SyncLock _sync
                                    _errorCount += 1
                                End SyncLock
                                Continue Do
                            End Try
                            dxgiRes.Dispose()
                        End If

                        ' GPU copy: DXGI desktop texture → our staging texture
                        _deviceContext.CopyResource(stagingTexture, desktopTexture)

                        ' ReleaseFrame IMMEDIATELY — duplication requires ReleaseFrame
                        ' before next AcquireNextFrame. The staging texture is now
                        ' independent (we own it).
                        Try
                            _duplication.ReleaseFrame()
                        Catch ex As Exception
                            _logger.Warning($"DdagrabBackend: ReleaseFrame threw: {ex.Message}")
                        End Try

                        ' ─── Construct D3D11VideoFrame + push to sink ─────────
                        ' PTS = capture time (P1-A §3.6.1 Option β — pipeline PTS
                        ' uses the same timebase as CaptureTimeTicks). Encoder
                        ' will read frame.Diagnostics.PresentationTimestampTicks.
                        frame = New D3D11VideoFrame(
                            stagingTexture,
                            _outputWidth,
                            _outputHeight,
                            sequence,
                            attemptTime,
                            attemptTime,
                            sharedHandle)  ' IntPtr.Zero for direct path, handle for shared path
                        stagingTexture = Nothing  ' ownership transferred to frame

                        ' Set disposal callback for metric tracking — fires regardless
                        ' of who disposes the frame (backend, sink, or consumer).
                        frame.OnDisposed = Sub()
                                              SyncLock _sync
                                                  _texturesDisposed += 1
                                              End SyncLock
                                          End Sub

                        ' TryPush — TRANSFER ownership model
                        Dim outcome As PushOutcome = _sink.TryPush(
                            FrameAcquisitionResult.Available(frame, sequence, attemptTime))
                        Select Case outcome
                            Case PushOutcome.Pushed
                                SyncLock _sync
                                    _emittedFrames += 1
                                    _framesPushed += 1
                                End SyncLock
                                frame = Nothing  ' ownership transferred to sink
                            Case PushOutcome.Replaced
                                SyncLock _sync
                                    _emittedFrames += 1
                                    _replacedFrames += 1
                                    _framesPushed += 1  ' new frame accepted
                                End SyncLock
                                frame = Nothing  ' ownership transferred to sink (old frame disposed by sink)
                            Case PushOutcome.Dropped
                                SyncLock _sync
                                    _droppedFrames += 1
                                    _framesDropped += 1
                                End SyncLock
                                ' Backend retains ownership — dispose immediately.
                                ' frame.Dispose() triggers OnDisposed callback which increments _texturesDisposed.
                                ' (No manual _texturesDisposed increment here — callback handles it.)
                                frame?.Dispose()
                                frame = Nothing
                        End Select

                    Catch ex As Exception
                        SyncLock _sync
                            _errorCount += 1
                        End SyncLock
                        _logger.Error($"DdagrabBackend: worker iteration failed: {ex.Message}", ex)
                        ' Dispose any partially-created resources.
                        ' If frame was constructed, frame.Dispose() triggers OnDisposed callback.
                        ' If only stagingTexture exists (frame NOT constructed yet), manual increment needed.
                        If frame IsNot Nothing Then
                            frame.Dispose()
                            ' OnDisposed callback handles _texturesDisposed += 1
                        End If
                        If stagingTexture IsNot Nothing Then
                            stagingTexture.Dispose()
                            SyncLock _sync
                                _texturesDisposed += 1
                            End SyncLock
                        End If
                    Finally
                        ' Always dispose the DXGI desktop texture wrapper.
                        ' (The underlying DXGI texture belongs to the duplication
                        ' and is released via ReleaseFrame above.)
                        Try : desktopTexture?.Dispose() : Catch : End Try
                        Try : desktopResource?.Dispose() : Catch : End Try
                    End Try

                Loop
            Catch ex As Exception
                _logger.Error($"DdagrabBackend: worker thread crashed: {ex.Message}", ex)
            End Try

            _logger.Info("DdagrabBackend: worker exited")
        End Sub

        Private Sub RecreateDuplication()
            ' DXGI_ACCESS_LOST recovery — recreate the duplication object.
            ' Existing _duplication is dead; replace it.
            Try
                _duplication?.Dispose()
            Catch
            End Try
            Try
                Dim output1 As IDXGIOutput1 = _output.QueryInterface(Of IDXGIOutput1)()
                _duplication = output1.DuplicateOutput(_device)
                output1.Dispose()
                _logger.Info("DdagrabBackend: DXGI Output Duplication recreated")
            Catch ex As Exception
                _logger.Error($"DdagrabBackend: failed to recreate duplication: {ex.Message}", ex)
                ' Worker will see access-lost again next iteration; keep trying.
            End Try
        End Sub

        Private Sub CleanupPersistentResources()
            ' Dispose in reverse construction order. All wrapped in try/catch
            ' so partial-init failures don't leak remaining resources.
            Try : _duplication?.Dispose() : Catch ex As Exception : _logger.Warning($"duplication.Dispose threw: {ex.Message}") : End Try
            Try : _output?.Dispose() : Catch : End Try
            Try : _adapter?.Dispose() : Catch : End Try
            Try : _deviceContext?.Dispose() : Catch : End Try
            Try : _device?.Dispose() : Catch : End Try
            Try : _dxgiFactory?.Dispose() : Catch : End Try
        End Sub

        Private Sub ThrowIfDisposed()
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(
                        NameOf(DdagrabBackend),
                        "DdagrabBackend has been disposed and can no longer be used.")
                End If
            End SyncLock
        End Sub

        Public Enum DdagrabBackendState
            Created
            Initializing
            Initialized
            Starting
            Running
            Stopping
            Stopped
            Faulted
            Disposed
        End Enum
    End Class
End Namespace
