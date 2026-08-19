Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading
Imports CaptureEngine.Video

Namespace CaptureEngine.Video.Tests.Fakes
    ''' <summary>
    ''' A test double that simulates an FFmpeg-subprocess-backed capture backend.
    '''
    ''' This backend mimics the lifecycle behavior of a real FFmpegBackend:
    ''' - Initialize spawns a "process" (simulated)
    ''' - Start begins a worker thread that "reads frames" from the process
    ''' - Stop stops the worker and kills the process
    ''' - Dispose cleans up everything
    '''
    ''' The fake supports injecting failures:
    ''' - FFmpegMissing: Initialize throws VideoBackendConfigurationException
    ''' - ProcessExit: worker thread simulates unexpected process exit
    ''' - SlowStop: Stop takes longer than normal (to test timeout)
    '''
    ''' This does NOT implement a real FFmpegBackend. It validates the
    ''' IVideoCaptureBackend contract for process-based backends.
    ''' </summary>
    Public NotInheritable Class FakeFFmpegBackend
        Implements IVideoCaptureBackend
        Implements IVideoBackendDiagnostics

        ' ---- simulation config ----
        Private _ffmpegMissing As Boolean = False
        Private _processExitAfterFrames As Integer = -1  ' -1 = never exit
        Private _stopDelayMs As Integer = 0

        ' ---- backend state ----
        Private ReadOnly _sync As New Object()
        Private _state As FFmpegBackendState = FFmpegBackendState.Created
        Private _disposed As Boolean = False

        ' ---- worker ----
        Private _workerThread As Thread
        Private _stopSignal As Boolean = False
        Private _sink As IVideoFrameSink

        ' ---- process simulation ----
        Private _processAlive As Boolean = False
        Private _processExitedUnexpectedly As Boolean = False

        ' ---- diagnostics ----
        Private _emittedFrames As Long = 0
        Private _droppedFrames As Long = 0
        Private _replacedFrames As Long = 0
        Private _noFrameCount As Long = 0
        Private _errorCount As Long = 0

        ' ---- frame simulation ----
        Private _nextSequence As Long = 0
        Private _interAttemptDelayMs As Integer = 10

        Public Enum FFmpegBackendState
            Created
            Initialized
            Starting
            Running
            Stopping
            Stopped
            Faulted
            Disposed
        End Enum

#Region "Test configuration (fluent API)"

        Public Function WithFFmpegMissing() As FakeFFmpegBackend
            _ffmpegMissing = True
            Return Me
        End Function

        Public Function WithProcessExitAfterFrames(count As Integer) As FakeFFmpegBackend
            _processExitAfterFrames = count
            Return Me
        End Function

        Public Function WithStopDelayMs(delayMs As Integer) As FakeFFmpegBackend
            _stopDelayMs = delayMs
            Return Me
        End Function

        Public Function WithInterAttemptDelayMs(delayMs As Integer) As FakeFFmpegBackend
            _interAttemptDelayMs = delayMs
            Return Me
        End Function

#End Region

#Region "IVideoCaptureBackend"

        Public ReadOnly Property Diagnostics As IVideoBackendDiagnostics Implements IVideoCaptureBackend.Diagnostics
            Get
                Return Me
            End Get
        End Property

        Public Sub Initialize(context As IVideoBackendContext) Implements IVideoCaptureBackend.Initialize
            If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))

            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(FakeFFmpegBackend),
                        "FakeFFmpegBackend has been disposed.")
                End If
                If _state <> FFmpegBackendState.Created Then
                    Throw New InvalidOperationException(
                        "Initialize cannot be called from state '" & _state.ToString() & "'. Expected 'Created'.")
                End If
                _state = FFmpegBackendState.Initializing
            End SyncLock

            ' Simulate FFmpeg binary not found
            If _ffmpegMissing Then
                SyncLock _sync
                    _state = FFmpegBackendState.Faulted
                End SyncLock
                Throw New VideoBackendConfigurationException(
                    "FFmpeg binary not found at the configured path.")
            End If

            ' Simulate process spawn
            _processAlive = True

            SyncLock _sync
                _state = FFmpegBackendState.Initialized
            End SyncLock
        End Sub

        Public Sub Start(sink As IVideoFrameSink) Implements IVideoCaptureBackend.Start
            If sink Is Nothing Then Throw New ArgumentNullException(NameOf(sink))

            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(FakeFFmpegBackend))
                End If
                Select Case _state
                    Case FFmpegBackendState.Starting, FFmpegBackendState.Running
                        Return ' idempotent
                    Case FFmpegBackendState.Initialized, FFmpegBackendState.Stopped
                        _state = FFmpegBackendState.Starting
                    Case Else
                        Throw New InvalidOperationException(
                            "Start cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            _sink = sink
            _stopSignal = False
            _nextSequence = 0
            _workerThread = New Thread(AddressOf WorkerLoop) With {
                .IsBackground = True,
                .Name = "FakeFFmpegBackend.Worker"
            }
            _workerThread.Start()

            SyncLock _sync
                _state = FFmpegBackendState.Running
            End SyncLock
        End Sub

        Public Sub [Stop]() Implements IVideoCaptureBackend.Stop
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(FakeFFmpegBackend))
                End If
                Select Case _state
                    Case FFmpegBackendState.Created, FFmpegBackendState.Initialized, FFmpegBackendState.Stopped
                        Return ' no-op
                    Case FFmpegBackendState.Stopping
                        Return ' already stopping
                    Case FFmpegBackendState.Faulted
                        Return ' no work to stop
                    Case FFmpegBackendState.Running
                        _state = FFmpegBackendState.Stopping
                    Case Else
                        Throw New InvalidOperationException(
                            "Stop cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            _stopSignal = True

            ' Simulate stop delay
            If _stopDelayMs > 0 Then
                Thread.Sleep(_stopDelayMs)
            End If

            ' Kill simulated process
            _processAlive = False

            Dim worker = _workerThread
            If worker IsNot Nothing Then
                If Not worker.Join(TimeSpan.FromSeconds(2)) Then
                    ' Worker didn't acknowledge — log and proceed
                End If
            End If

            SyncLock _sync
                _state = FFmpegBackendState.Stopped
            End SyncLock
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dim workerToJoin As Thread = Nothing
            Dim needJoin As Boolean = False

            SyncLock _sync
                If _disposed Then Return
                _disposed = True

                If _state = FFmpegBackendState.Running OrElse _state = FFmpegBackendState.Starting Then
                    _state = FFmpegBackendState.Stopping
                    _stopSignal = True
                    workerToJoin = _workerThread
                    needJoin = True
                End If
            End SyncLock

            If needJoin AndAlso workerToJoin IsNot Nothing Then
                Try
                    If Not workerToJoin.Join(TimeSpan.FromSeconds(2)) Then
                        ' Timeout — proceed anyway
                    End If
                Catch
                End Try
            End If

            _processAlive = False

            SyncLock _sync
                If _state = FFmpegBackendState.Stopping Then
                    _state = FFmpegBackendState.Stopped
                End If
                _state = FFmpegBackendState.Disposed
            End SyncLock
        End Sub

#End Region

#Region "IVideoBackendDiagnostics"

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

#End Region

#Region "Test inspection properties"

        Public ReadOnly Property CurrentState As FFmpegBackendState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ProcessAlive As Boolean
            Get
                Return _processAlive
            End Get
        End Property

        Public ReadOnly Property ProcessExitedUnexpectedly As Boolean
            Get
                Return _processExitedUnexpectedly
            End Get
        End Property

#End Region

#Region "Worker loop"

        Private Sub WorkerLoop()
            Dim framesBeforeExit As Integer = 0

            Do
                Dim shouldStop As Boolean
                SyncLock _sync
                    shouldStop = _stopSignal
                End SyncLock
                If shouldStop Then Exit Do

                ' Simulate process unexpected exit
                If _processExitAfterFrames >= 0 AndAlso framesBeforeExit >= _processExitAfterFrames Then
                    _processAlive = False
                    _processExitedUnexpectedly = True
                    SyncLock _sync
                        _errorCount += 1
                    End SyncLock
                    Exit Do
                End If

                ' Simulate frame delivery
                Dim sink = _sink
                If sink IsNot Nothing Then
                    ' Create a fake frame result
                    Dim sequence As Long
                    SyncLock _sync
                        sequence = _nextSequence
                        _nextSequence += 1
                    End SyncLock

                    ' For simplicity, we push a NoFrame result (no actual frame object)
                    ' Real FFmpeg backend would push FrameAvailable with pixel data
                    Dim result = FrameAcquisitionResult.NoFrame(sequence, 1_000_000 + sequence * 10_000)
                    Dim outcome = sink.TryPush(result)

                    SyncLock _sync
                        Select Case outcome
                            Case PushOutcome.Pushed
                                _emittedFrames += 1
                            Case PushOutcome.Replaced
                                _emittedFrames += 1
                                _replacedFrames += 1
                            Case PushOutcome.Dropped
                                _droppedFrames += 1
                        End Select
                    End SyncLock
                End If

                framesBeforeExit += 1

                If _interAttemptDelayMs > 0 Then
                    Thread.Sleep(_interAttemptDelayMs)
                End If
            Loop
        End Sub

#End Region
    End Class
End Namespace
