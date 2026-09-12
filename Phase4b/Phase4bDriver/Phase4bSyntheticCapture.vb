Option Strict On
Option Explicit On
Option Infer On

' Phase4bSyntheticCapture.vb — Phase 4B experiment-only capture backend.
'
' Emits IVideoFrame at a REAL wall-clock cadence with REAL QPC (Stopwatch)
' timestamps, using the exact timestamp semantics of production
' DdagrabBackend: CaptureTimeTicks == PresentationTimestampTicks ==
' QpcNow100ns sampled at emission, monotonic, in the same 100ns domain the
' CFR loop compares against (`CaptureTimeTicks <= targetQpc100ns`).
'
' Purpose: make the CAPTURE-stage warm-up a CONTROLLED VARIABLE so the A/B
' isolates the timeline-delay causality. `--warmup-ms` reproduces the
' Phase-4-measured first-frame acquisition residual (28ms); on the NVIDIA
' machine the real DdagrabBackend replaces this backend and provides the
' real DXGI warm-up. Instrumentation mirrors DdagrabBackend's
' "FRAME n CAPTURE TIMING" format so one analyzer parses both.

Imports System
Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video

Namespace Phase4b

    Public NotInheritable Class Phase4bSyntheticCapture
        Implements IPhase4bVideoBackend
        Implements IVideoBackendDiagnostics

        Private ReadOnly _sync As New Object()
        Private ReadOnly _logger As EngineLogger

        Private _state As BackendState = BackendState.Created
        Private _disposed As Boolean = False

        Private _sink As IVideoFrameSink
        Private _worker As Thread
        Private _stopSignal As Boolean = False

        ' Config
        Private ReadOnly _firstFrameDelayMs As Integer
        Private ReadOnly _frameIntervalMs As Double
        Private ReadOnly _width As Integer
        Private ReadOnly _height As Integer

        ' Diagnostics counters
        Private _emittedFrames As Long = 0
        Private _framesPushed As Long = 0
        Private _droppedFrames As Long = 0
        Private _replacedFrames As Long = 0
        Private _noFrameCount As Long = 0
        Private _errorCount As Long = 0

        Private _nextSequence As Long = 0
        Private _lastCaptureQpc100ns As Long = 0

        Public Sub New(logger As EngineLogger,
                       firstFrameDelayMs As Integer,
                       frameIntervalMs As Double,
                       width As Integer,
                       height As Integer)
            _logger = If(logger, New EngineLogger("Phase4bSyntheticCapture"))
            _firstFrameDelayMs = Math.Max(0, firstFrameDelayMs)
            _frameIntervalMs = Math.Max(1.0, frameIntervalMs)
            _width = width
            _height = height
        End Sub

        ' ===== IVideoCaptureBackend =====

        Public ReadOnly Property Diagnostics As IVideoBackendDiagnostics Implements IVideoCaptureBackend.Diagnostics
            Get
                Return Me
            End Get
        End Property

        Public Sub Initialize(context As IVideoBackendContext) Implements IVideoCaptureBackend.Initialize
            SyncLock _sync
                If _disposed Then Throw New ObjectDisposedException(NameOf(Phase4bSyntheticCapture))
                If _state <> BackendState.Created Then
                    Throw New InvalidOperationException($"Initialize from state {_state}")
                End If
                _state = BackendState.Initialized
            End SyncLock
            _logger.Info($"Phase4bSyntheticCapture: Initialize complete ({_width}x{_height}, warmup {_firstFrameDelayMs}ms, interval {_frameIntervalMs:0.###}ms) — SYNTHETIC backend (experiment)")
        End Sub

        Public Sub Start(sink As IVideoFrameSink) Implements IVideoCaptureBackend.Start
            If sink Is Nothing Then Throw New ArgumentNullException(NameOf(sink))
            SyncLock _sync
                If _disposed Then Throw New ObjectDisposedException(NameOf(Phase4bSyntheticCapture))
                Select Case _state
                    Case BackendState.Starting, BackendState.Running
                        _logger.Warning("Phase4bSyntheticCapture: Start ignored, already running")
                        Return
                    Case BackendState.Initialized, BackendState.Stopped
                        _state = BackendState.Starting
                    Case Else
                        Throw New InvalidOperationException($"Start from state {_state}")
                End Select
                _sink = sink
                _stopSignal = False
                ' Per-session sequence reset so the first-4 instrumentation
                ' gates apply per run (log gating only — no timing semantics).
                _nextSequence = 0
                _lastCaptureQpc100ns = 0
                _worker = New Thread(AddressOf WorkerLoop) With {.IsBackground = True, .Name = "Phase4bSyntheticCapture.Worker"}
                _worker.Start()
                _state = BackendState.Running
            End SyncLock
            _logger.Info("Phase4bSyntheticCapture: started")
        End Sub

        Public Sub [Stop]() Implements IVideoCaptureBackend.Stop
            SyncLock _sync
                If _state <> BackendState.Running Then
                    _logger.Warning($"Phase4bSyntheticCapture: Stop ignored (state={_state})")
                    Return
                End If
                _state = BackendState.Stopping
                _stopSignal = True
            End SyncLock
            Dim w = _worker
            If w IsNot Nothing AndAlso Not w.Join(2000) Then
                _logger.Error("Phase4bSyntheticCapture: worker did not stop within 2s", Nothing)
            End If
            SyncLock _sync
                _state = BackendState.Stopped
            End SyncLock
            _logger.Info("Phase4bSyntheticCapture: stopped")
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _sync
                If _disposed Then Return
                _disposed = True
                _stopSignal = True
            End SyncLock
            Dim w = _worker
            If w IsNot Nothing AndAlso w.IsAlive Then w.Join(2000)
            SyncLock _sync
                _state = BackendState.Disposed
            End SyncLock
            _logger.Info("Phase4bSyntheticCapture: disposed")
        End Sub

        ' ===== IVideoBackendDiagnostics =====

        Public ReadOnly Property EmittedFrames As Long Implements IVideoBackendDiagnostics.EmittedFrames
            Get
                SyncLock _sync : Return _emittedFrames : End SyncLock
            End Get
        End Property
        Public ReadOnly Property DroppedFrames As Long Implements IVideoBackendDiagnostics.DroppedFrames
            Get
                SyncLock _sync : Return _droppedFrames : End SyncLock
            End Get
        End Property
        Public ReadOnly Property ReplacedFrames As Long Implements IVideoBackendDiagnostics.ReplacedFrames
            Get
                SyncLock _sync : Return _replacedFrames : End SyncLock
            End Get
        End Property
        Public ReadOnly Property NoFrameCount As Long Implements IVideoBackendDiagnostics.NoFrameCount
            Get
                SyncLock _sync : Return _noFrameCount : End SyncLock
            End Get
        End Property
        Public ReadOnly Property ErrorCount As Long Implements IVideoBackendDiagnostics.ErrorCount
            Get
                SyncLock _sync : Return _errorCount : End SyncLock
            End Get
        End Property

        ' ===== IPhase4bVideoBackend extras =====

        Public ReadOnly Property OutputWidth As Integer Implements IPhase4bVideoBackend.OutputWidth
            Get
                Return _width
            End Get
        End Property
        Public ReadOnly Property OutputHeight As Integer Implements IPhase4bVideoBackend.OutputHeight
            Get
                Return _height
            End Get
        End Property
        Public ReadOnly Property OutputRefreshRate As Integer Implements IPhase4bVideoBackend.OutputRefreshRate
            Get
                Return 60
            End Get
        End Property
        Public ReadOnly Property FramesPushed As Long Implements IPhase4bVideoBackend.FramesPushed
            Get
                SyncLock _sync : Return _framesPushed : End SyncLock
            End Get
        End Property
        Public ReadOnly Property AccessLostCount As Long Implements IPhase4bVideoBackend.AccessLostCount
            Get
                Return 0
            End Get
        End Property
        Public ReadOnly Property TexturesCreated As Long Implements IPhase4bVideoBackend.TexturesCreated
            Get
                SyncLock _sync : Return _emittedFrames : End SyncLock
            End Get
        End Property
        Public ReadOnly Property TexturesDisposed As Long Implements IPhase4bVideoBackend.TexturesDisposed
            Get
                SyncLock _sync : Return _emittedFrames : End SyncLock
            End Get
        End Property

        ' ===== Worker =====

        Private Sub WorkerLoop()
            Try
                ' First-frame acquisition delay (the warm-up residual under test).
                If _firstFrameDelayMs > 0 Then Thread.Sleep(_firstFrameDelayMs)

                Dim intervalTicks As Long = CLng(_frameIntervalMs * Stopwatch.Frequency / 1000.0)
                Dim nextEmit As Long = Stopwatch.GetTimestamp()

                Do
                    Dim stopNow As Boolean = False
                    SyncLock _sync
                        stopNow = _stopSignal
                    End SyncLock
                    If stopNow Then Exit Do

                    EmitFrame()

                    nextEmit += intervalTicks
                    Dim nowTicks As Long = Stopwatch.GetTimestamp()
                    If nextEmit > nowTicks Then
                        Dim waitMs As Double = (nextEmit - nowTicks) * 1000.0 / Stopwatch.Frequency
                        If waitMs > 1.0 Then
                            Thread.Sleep(CInt(Math.Min(15, waitMs)))
                        ElseIf waitMs > 0.05 Then
                            Thread.SpinWait(200)
                        End If
                    Else
                        nextEmit = nowTicks  ' never accumulate a backlog of synthetic frames
                    End If
                Loop
            Catch ex As Exception
                _logger.Error($"Phase4bSyntheticCapture: worker crashed: {ex.Message}", ex)
                SyncLock _sync
                    _errorCount += 1
                End SyncLock
            End Try
            _logger.Info("Phase4bSyntheticCapture: worker exited")
        End Sub

        Private Sub EmitFrame()
            Dim seq As Long
            SyncLock _sync
                seq = _nextSequence
                _nextSequence += 1
            End SyncLock

            ' DdagrabBackend semantics: QPC sampled at acquisition, monotonic,
            ' capture == presentation (100ns QPC domain, overflow-safe).
            Dim qpc100ns As Long = TicksTo100ns(Stopwatch.GetTimestamp())
            If qpc100ns < _lastCaptureQpc100ns Then qpc100ns = _lastCaptureQpc100ns
            _lastCaptureQpc100ns = qpc100ns

            If seq <= 4 Then
                Dim captureTick As Long = Stopwatch.GetTimestamp()
                _logger.Info($"DdagrabBackend: FRAME {seq} CAPTURE TIMING:")
                _logger.Info($"  acquireQpc100ns={qpc100ns}")
                _logger.Info($"  frameQpc100ns={qpc100ns}")
                _logger.Info($"  captureTick={captureTick}")
                _logger.Info($"  captureQpc={captureTick}")
            End If

            Dim diag As New FrameDiagnostics(seq, qpc100ns, qpc100ns)
            Dim frame As New Phase4bCpuFrame(_width, _height, diag)
            Dim sink = _sink
            If sink Is Nothing Then Return

            Dim outcome As PushOutcome = sink.TryPush(FrameAcquisitionResult.Available(frame, seq, qpc100ns))
            SyncLock _sync
                Select Case outcome
                    Case PushOutcome.Pushed
                        _emittedFrames += 1
                        _framesPushed += 1
                    Case PushOutcome.Replaced
                        _emittedFrames += 1
                        _framesPushed += 1
                        _replacedFrames += 1
                    Case PushOutcome.Dropped
                        _droppedFrames += 1
                        Try : frame.Dispose() : Catch : End Try
                End Select
            End SyncLock
        End Sub

        ''' <summary>Production WasapiPositionCapture.StopwatchTicksTo100ns —
        ''' overflow-safe: (ticks/freq)×1e7 + (ticks%freq)×1e7/freq.
        ''' NOTE VB precedence: `\` binds LOWER than `*` — parentheses are
        ''' load-bearing here (without them the result wraps every second).</summary>
        Friend Shared Function TicksTo100ns(qpcTicks As Long) As Long
            Dim f As Long = Stopwatch.Frequency
            Return (qpcTicks \ f) * 10000000L + ((qpcTicks Mod f) * 10000000L) \ f
        End Function

        Private Enum BackendState
            Created
            Initialized
            Starting
            Running
            Stopping
            Stopped
            Disposed
        End Enum
    End Class

    ''' <summary>Minimal CPU-memory IVideoFrame (no pixel payload — Phase 4B
    ''' measures TIMING, not content; the canned encoder never reads pixels).</summary>
    Public NotInheritable Class Phase4bCpuFrame
        Implements IVideoFrame

        Private ReadOnly _dims As VideoFrameDimensions
        Private ReadOnly _diag As FrameDiagnostics
        Private _disposed As Boolean = False

        Public Sub New(width As Integer, height As Integer, diag As FrameDiagnostics)
            _dims = New VideoFrameDimensions(width, height)
            _diag = diag
        End Sub

        Public ReadOnly Property Origin As VideoFrameOrigin Implements IVideoFrame.Origin
            Get
                Return VideoFrameOrigin.CpuMemory
            End Get
        End Property
        Public ReadOnly Property PixelFormat As VideoPixelFormat Implements IVideoFrame.PixelFormat
            Get
                Return VideoPixelFormat.Bgra8
            End Get
        End Property
        Public ReadOnly Property Dimensions As VideoFrameDimensions Implements IVideoFrame.Dimensions
            Get
                Return _dims
            End Get
        End Property
        Public ReadOnly Property Diagnostics As FrameDiagnostics Implements IVideoFrame.Diagnostics
            Get
                Return _diag
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            _disposed = True
        End Sub
    End Class

End Namespace
