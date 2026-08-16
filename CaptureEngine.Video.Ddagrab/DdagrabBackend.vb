Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading
Imports CaptureEngine.Diagnostics

Namespace CaptureEngine.Video.Backends.Ddagrab
    ''' <summary>
    ''' DdagrabBackend — production video capture backend using DXGI Output
    ''' Duplication (ddagrab). (P1-A v1.3.1 §1.3, §7.1, §11)
    '''
    ''' GLM-1 SKELETON STATUS:
    ''' This is a SKELETON implementation. It satisfies the full
    ''' IVideoCaptureBackend lifecycle contract (Initialize / Start / Stop /
    ''' Dispose + Diagnostics + idempotency + post-Dispose guards +
    ''' deadlock-free Dispose-while-Running) but does NOT yet perform real
    ''' DXGI Output Duplication capture.
    '''
    ''' What is implemented:
    '''   - Full lifecycle state machine (Created → Initializing →
    '''     Initialized → Starting → Running → Stopping → Stopped →
    '''     Disposed; Faulted on failure).
    '''   - Worker thread that polls for frames. Currently always returns
    '''     NoFrame (real DXGI capture is a TODO below).
    '''   - Per P1-A v1.3.1 §6.4, NoFrame results are NOT pushed to the
    '''     sink — they are an internal backend signal. The
    '''     NoFrameCount diagnostic counter increments.
    '''   - Stop contract per P1-A v1.3.1 §4 + P1-B.1 FIX change #2:
    '''     Stop stops the producer; queued frames remain sink-owned;
    '''     backend does NOT require unbounded drain during Stop.
    '''   - Dispose deadlock-free per P1-B.1 FIX change #1: capture state
    '''     under lock, set stop signal under lock, RELEASE lock, Join
    '''     worker OUTSIDE lock, re-acquire lock ONLY to finalize state.
    '''
    ''' What is NOT yet implemented (TODO for a future task):
    '''   - Real DXGI Output Duplication: create D3D11 device, enumerate
    '''     DXGI adapters/outputs, duplicate output, call
    '''     AcquireNextFrame in the worker loop.
    '''   - Real D3D11 device creation per §5 (Option A working assumption
    '''     from P1-B.2 §16.4 — each backend owns its own device on the
    '''     same physical GPU).
    '''   - Real DdagrabFrame construction (the placeholder DdagrabFrame
    '''     class exists but is not yet emitted).
    '''   - BGRA8 baseline enforcement on real captured textures (§3.4,
    '''     §15.1) — the OS texture's DXGI_FORMAT must be
    '''     DXGI_FORMAT_B8G8R8A8_UNORM.
    '''   - Timestamp conversion per P1-A v1.3.1 §3.6.1 Option β (chosen
    '''     in P1-B.2 §16.3): CaptureTimeTicks =
    '''     Stopwatch.GetTimestamp() * 10_000_000L \ Stopwatch.Frequency.
    '''
    ''' Constraints honored:
    '''   - Foundation NOT modified.
    '''   - No FFmpeg / NVENC / NAudio / Audio / UI / Output integration.
    '''   - No contract (interface) modifications.
    ''' </summary>
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
        Private _interAttemptDelayMs As Integer = 1   ' avoid busy-looping in skeleton mode

        ' ---- diagnostics (counters) ----
        Private _emittedFrames As Long = 0
        Private _droppedFrames As Long = 0
        Private _replacedFrames As Long = 0
        Private _noFrameCount As Long = 0
        Private _errorCount As Long = 0

        ' ---- monotonic sequence ----
        Private _nextSequence As Long = 0

        ' ---- captured context (for real-implementation TODO) ----
        Private _context As IVideoBackendContext

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
                ' Validate backend kind.
                If context.BackendKind <> VideoBackendKind.Ddagrab Then
                    Throw New VideoBackendConfigurationException(
                        "DdagrabBackend.Initialize: context.BackendKind must be Ddagrab, but was " &
                        context.BackendKind.ToString() & ".")
                End If

                _context = context

                ' TODO (future task): create D3D11 device on the NVIDIA adapter,
                ' enumerate DXGI outputs, duplicate the primary output. Until
                ' then, this skeleton accepts Initialize and proceeds to
                ' Initialized. Real capture will be added in a later task.

                _state = DdagrabBackendState.Initialized
                _logger.Info("DdagrabBackend: Initialize complete (SKELETON — real DXGI capture not yet implemented)")
            Catch ex As VideoBackendException
                _state = DdagrabBackendState.Faulted
                _logger.Error("DdagrabBackend: Initialize failed", ex)
                Throw
            Catch ex As Exception
                _state = DdagrabBackendState.Faulted
                _logger.Error("DdagrabBackend: Initialize failed (unexpected)", ex)
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
                _logger.Info("DdagrabBackend: started (SKELETON — worker emits NoFrame until real capture is implemented)")
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
            ' P0 fix (P1-B.1 FIX change #1): NEVER wait for the worker thread
            ' while holding _sync. The worker itself needs _sync on every
            ' iteration (to read state and update diagnostics counters), so
            ' holding _sync across worker.Join() is a guaranteed deadlock.
            '
            ' Pattern:
            '   1. Capture state under lock; set the stop signal under lock;
            '      mark _disposed to make Dispose idempotent.
            '   2. Release the lock.
            '   3. Join the worker thread OUTSIDE the lock.
            '   4. Re-acquire the lock ONLY to finalize state to Disposed.

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

            ' JOIN OUTSIDE THE LOCK — the worker needs _sync on every iteration.
            If needJoin AndAlso workerToJoin IsNot Nothing Then
                Try
                    If Not workerToJoin.Join(TimeSpan.FromSeconds(2)) Then
                        If _logger IsNot Nothing Then
                            _logger.Error("DdagrabBackend: worker did not acknowledge stop within 2 s", Nothing)
                        End If
                    End If
                Catch ex As Exception
                    If _logger IsNot Nothing Then
                        _logger.Error("DdagrabBackend: stop path failed during Dispose (will still dispose)", ex)
                    End If
                End Try
            End If

            ' RE-ACQUIRE LOCK ONLY TO FINALIZE STATE.
            SyncLock _sync
                If _state = DdagrabBackendState.Stopping Then
                    _state = DdagrabBackendState.Stopped
                End If
                _state = DdagrabBackendState.Disposed
                _logger.Info("DdagrabBackend: disposed")
            End SyncLock
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

        ' ===== Test-visible state (Friend) =====

        ''' <summary>Snapshot the current backend state (for tests).</summary>
        Friend ReadOnly Property CurrentState As DdagrabBackendState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        ''' <summary>Inter-attempt delay (ms). Default 1 ms (skeleton); real capture will use AcquireNextFrame's poll interval.</summary>
        Friend Function WithInterAttemptDelayMs(delayMs As Integer) As DdagrabBackend
            SyncLock _sync
                _interAttemptDelayMs = delayMs
                Return Me
            End SyncLock
        End Function

        ' ===== Internal worker =====

        Private Sub WorkerLoop()
            Do
                Dim shouldStop As Boolean
                SyncLock _sync
                    shouldStop = _stopSignal
                End SyncLock
                If shouldStop Then Exit Do

                ' TODO (future task): replace this placeholder with real
                ' DXGI Output Duplication:
                '   1. IDXGIOutputDuplication.AcquireNextFrame(timeout, ...)
                '   2. If returned DXGI_ERROR_WAIT_TIMEOUT → NoFrame (current skeleton path).
                '   3. If returned S_OK with a new frame:
                '      a. Construct a DdagrabFrame wrapping the ID3D11Texture2D.
                '      b. Convert timestamp per §3.6.1 Option β:
                '         CaptureTimeTicks = Stopwatch.GetTimestamp() * 10_000_000L \ Stopwatch.Frequency
                '      c. Push FrameAcquisitionResult.Available(frame, sequence, attemptTime) to sink.
                '      d. ReleaseFrame() to return the texture to the duplication session.
                '
                ' For now, skeleton emits NoFrame forever so the lifecycle
                ' contract is exercised without depending on a real D3D11
                ' device or DXGI Output Duplication.

                Dim sequence As Long
                Dim attemptTime As Long
                SyncLock _sync
                    sequence = _nextSequence
                    _nextSequence += 1
                    ' Per §3.6.1 Option β (P1-B.2 §16.3):
                    '   CaptureTimeTicks = Stopwatch.GetTimestamp() * 10_000_000L \ Stopwatch.Frequency
                    '
                    ' Overflow-safe implementation: the naive multiplication
                    ' Stopwatch.GetTimestamp() * 10_000_000L overflows Int64 on
                    ' systems where Stopwatch.Frequency is large (e.g. Linux:
                    ' Frequency = 1_000_000_000; GetTimestamp() returns
                    ' nanoseconds since boot which can exceed 10^13 — multiplied
                    ' by 10^7 overflows Int64's ~9.2 × 10^18 max).
                    '
                    ' Use Decimal for the intermediate multiplication, then
                    ' truncate back to Long. The conversion is exact for any
                    ' realistic QPC tick value.
                    attemptTime = CLng(Math.Truncate(
                        CDec(Stopwatch.GetTimestamp()) * 10000000D /
                        CDec(Stopwatch.Frequency)))
                    _noFrameCount += 1
                End SyncLock

                ' NoFrame results are NOT pushed to the sink (§6.4).
                ' The sink only sees FrameAvailable (and optionally Error).

                If _interAttemptDelayMs > 0 Then
                    Thread.Sleep(_interAttemptDelayMs)
                End If
            Loop

            _logger.Info("DdagrabBackend: worker exited")
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

        ''' <summary>
        ''' Internal Ddagrab-backend state. (P1-A v1.3.1 §4.4)
        ''' Friend = assembly-internal; not part of the public API.
        ''' </summary>
        Friend Enum DdagrabBackendState
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
