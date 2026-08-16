Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading
Imports CaptureEngine.Configuration
Imports CaptureEngine.Diagnostics

Namespace CaptureEngine.Engine
    ''' <summary>
    ''' Core engine lifecycle controller — Phase 0 (Foundation).
    '''
    ''' Lifecycle:
    '''
    '''   Create  -&gt; Initialize(config) -&gt; Start -&gt; Running -&gt; Stop -&gt; Stopped -&gt; Dispose -&gt; Disposed
    '''
    ''' Foundation scope: state machine + structured logging only.
    ''' No capture, no encoding, no audio, no video, no FFmpeg, no NAudio,
    ''' no named pipes, no I/O pipelines. Those belong to later phases.
    ''' </summary>
    Public NotInheritable Class CaptureEngine
        Implements IDisposable

        Private ReadOnly _sync As New Object()
        Private ReadOnly _logger As EngineLogger

        Private _state As EngineState = EngineState.Created
        Private _disposed As Boolean = False

        ''' <summary>
        ''' Construct a CaptureEngine.
        ''' </summary>
        ''' <param name="logger">
        ''' Optional pre-built logger. When null, a default
        ''' <c>New EngineLogger("CaptureEngine")</c> is used.
        ''' </param>
        Public Sub New(Optional logger As EngineLogger = Nothing)
            _logger = If(logger, New EngineLogger("CaptureEngine"))
            _logger.Debug("constructed (state=Created)")
        End Sub

        ''' <summary>Current lifecycle state. Safe to read from any thread.</summary>
        Public ReadOnly Property CurrentState As EngineState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Initialize the engine with the supplied configuration.
        ''' Only valid in the <see cref="EngineState.Created"/> state.
        ''' </summary>
        ''' <exception cref="ArgumentNullException">
        ''' Thrown when <paramref name="config"/> is null.
        ''' </exception>
        ''' <exception cref="InvalidOperationException">
        ''' Thrown when called from any state other than Created.
        ''' </exception>
        Public Sub Initialize(config As EngineConfig)
            If config Is Nothing Then
                Throw New ArgumentNullException(NameOf(config))
            End If
            ThrowIfDisposed()

            SyncLock _sync
                If _state <> EngineState.Created Then
                    Throw New InvalidOperationException(
                        "Initialize cannot be called from state '" & _state.ToString() &
                        "'. Expected 'Created'.")
                End If
                _state = EngineState.Initializing
            End SyncLock

            Try
                _logger.Info("Initialize: begin")
                ApplyConfig(config)
                TransitionTo(EngineState.Stopped, EngineState.Initializing)
                _logger.Info("Initialize: complete (state=Stopped)")
            Catch ex As Exception
                _logger.Error("Initialize: failed", ex)
                ForceTransitionTo(EngineState.Faulted)
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Start the engine. Only valid in the <see cref="EngineState.Stopped"/> state.
        '''
        ''' Idempotent contract:
        '''   - Calling Start() while already Starting or Running is a no-op
        '''     that emits a Warning and preserves the current state.
        '''   - Calling Start() from any other state throws
        '''     <see cref="InvalidOperationException"/>.
        ''' </summary>
        Public Sub Start()
            ThrowIfDisposed()

            SyncLock _sync
                Select Case _state
                    Case EngineState.Starting, EngineState.Running
                        _logger.Warning("Start: ignored, engine is already '" & _state.ToString() & "'.")
                        Return
                    Case EngineState.Stopped
                        _state = EngineState.Starting
                    Case Else
                        Throw New InvalidOperationException(
                            "Start cannot be called from state '" & _state.ToString() &
                            "'. Expected 'Stopped'.")
                End Select
            End SyncLock

            Try
                _logger.Info("Start: begin")
                ' Foundation: no real capture/encode work to start yet.
                TransitionTo(EngineState.Running, EngineState.Starting)
                _logger.Info("started")
            Catch ex As Exception
                _logger.Error("Start: failed", ex)
                ForceTransitionTo(EngineState.Faulted)
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Stop the engine. Valid from Running, Stopping, Stopped, Created, or Faulted.
        '''
        ''' Idempotent contract:
        '''   - Calling Stop() when the engine is not Running (Stopped / Created /
        '''     Stopping / Faulted) is a no-op that emits a Warning.
        ''' </summary>
        Public Sub [Stop]()
            ThrowIfDisposed()

            SyncLock _sync
                Select Case _state
                    Case EngineState.Stopped, EngineState.Created
                        _logger.Warning("Stop: ignored, engine is not running (state='" & _state.ToString() & "').")
                        Return
                    Case EngineState.Stopping
                        _logger.Warning("Stop: ignored, engine is already stopping.")
                        Return
                    Case EngineState.Faulted
                        _logger.Warning("Stop: engine is Faulted; no running work to stop.")
                        Return
                    Case EngineState.Running
                        _state = EngineState.Stopping
                    Case Else
                        Throw New InvalidOperationException(
                            "Stop cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            Try
                _logger.Info("Stop: begin")
                ' Foundation: no real capture/encode work to stop yet.
                TransitionTo(EngineState.Stopped, EngineState.Stopping)
                _logger.Info("stopped")
            Catch ex As Exception
                _logger.Error("Stop: failed", ex)
                ForceTransitionTo(EngineState.Faulted)
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Release all resources held by the engine. Idempotent — safe to call
        ''' multiple times. After Dispose, the engine enters the Disposed state
        ''' and any subsequent Initialize/Start/Stop call throws
        ''' <see cref="ObjectDisposedException"/>.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        Private Sub Dispose(disposing As Boolean)
            SyncLock _sync
                If _disposed Then Return
                _disposed = True

                If disposing Then
                    If _state = EngineState.Disposed Then
                        ' Defensive: should not happen, but stay silent.
                    ElseIf _state = EngineState.Running OrElse _state = EngineState.Starting Then
                        _logger.Info("Dispose: stopping engine in state '" & _state.ToString() & "'.")
                    Else
                        _logger.Info("Dispose: engine in state '" & _state.ToString() & "'.")
                    End If
                    _state = EngineState.Disposed
                    _logger.Info("disposed")
                End If
            End SyncLock
        End Sub

        ' ---- internal helpers ----

        ''' <summary>
        ''' Apply Foundation-level config. Phase 0 only acknowledges LogLevel;
        ''' wiring it to the runtime logger is deferred to a later phase
        ''' (see Known Limitations in the task report).
        ''' </summary>
        Private Sub ApplyConfig(config As EngineConfig)
            _logger.Debug("ApplyConfig: LogLevel=" & config.LogLevel.ToString())
        End Sub

        ''' <summary>
        ''' Transition to <paramref name="newState"/> only if the current state
        ''' matches <paramref name="expectedOld"/>. Throws on mismatch.
        ''' </summary>
        Private Sub TransitionTo(newState As EngineState, expectedOld As EngineState)
            SyncLock _sync
                If _state <> expectedOld Then
                    Throw New InvalidOperationException(
                        "State transition to '" & newState.ToString() &
                        "' failed: expected current state '" & expectedOld.ToString() &
                        "' but was '" & _state.ToString() & "'.")
                End If
                _state = newState
            End SyncLock
        End Sub

        ''' <summary>
        ''' Force a transition regardless of current state (except Disposed).
        ''' Used for fault recovery paths.
        ''' </summary>
        Private Sub ForceTransitionTo(newState As EngineState)
            SyncLock _sync
                If _state = EngineState.Disposed Then Return
                _state = newState
            End SyncLock
        End Sub

        Private Sub ThrowIfDisposed()
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(
                        NameOf(CaptureEngine),
                        "CaptureEngine has been disposed and can no longer be used.")
                End If
            End SyncLock
        End Sub
    End Class
End Namespace
