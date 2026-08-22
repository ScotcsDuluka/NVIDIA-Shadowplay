Option Strict On
Option Explicit On
Option Infer On

' RecordingEngine.vb
'
' Process-lifetime orchestrator. Composes IVideoCaptureBackend (DdagrabBackend) +
' IEncoderBackend (NvencEncoderBackend) whose Initialize() happens ONCE per process.
' Per-session: creates CaptureSession (audio + FFmpeg + mux).
'
' RecordingEngine itself OWNS NO GPU resources — the backends do.
' It just constructs them, dispatches sessions, and disposes them.

Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.Video.Handoff

Namespace CaptureEngine.Recording

    Public NotInheritable Class RecordingEngine
        Implements IDisposable

        Private ReadOnly _sync As New Object()
        Private ReadOnly _logger As EngineLogger

        ' ─── Persistent backends (created in Initialize, disposed in Dispose) ─
        Private _capture As DdagrabBackend
        Private _encoder As NvencEncoderBackend
        Private _state As RecordingEngineState = RecordingEngineState.Created
        Private _disposed As Boolean = False

        ' ─── Current session ─────────────────────────────────────────────
        Private _currentSession As CaptureSession

        Public Sub New(logger As EngineLogger)
            _logger = logger
        End Sub

        ''' <summary>
        ''' Initialize persistent GPU resources (D3D11 + DXGI + NVENC).
        ''' Called ONCE at process start. Subsequent calls throw.
        ''' </summary>
        Public Sub Initialize()
            SyncLock _sync
                If _disposed Then Throw New ObjectDisposedException(NameOf(RecordingEngine))
                If _state <> RecordingEngineState.Created Then
                    Throw New InvalidOperationException($"Initialize() called from state {_state}")
                End If
                _state = RecordingEngineState.Initializing
            End SyncLock

            Try
                ' ─── Initialize DdagrabBackend (creates D3D11 + DXGI duplication) ─
                _capture = New DdagrabBackend(_logger)
                Dim ctx As New BackendContext(_logger)
                _capture.Initialize(ctx)
                _logger.Info($"RecordingEngine: DdagrabBackend initialized — {_capture.OutputWidth}x{_capture.OutputHeight}")

                ' ─── Initialize NvencEncoderBackend (creates D3D11 + NVENC session) ─
                _encoder = New NvencEncoderBackend(_logger)
                Dim encConfig As New EncoderConfig() With {
                    .CodecKey = "NVENC_H264",
                    .BitrateBps = 20_000_000L,
                    .MinrateBps = 20_000_000L,
                    .MaxrateBps = 20_000_000L,
                    .BufsizeBps = 40_000_000L,
                    .GopSize = 60,
                    .RateControl = "cbr",
                    .Preset = "p4",
                    .ExpectedWidth = _capture.OutputWidth,
                    .ExpectedHeight = _capture.OutputHeight
                }
                _encoder.Initialize(encConfig)
                _logger.Info("RecordingEngine: NvencEncoderBackend initialized")

                _state = RecordingEngineState.Idle
                _logger.Info("RecordingEngine: ready (Idle)")
            Catch ex As Exception
                _state = RecordingEngineState.Faulted
                _logger.Error($"RecordingEngine: Initialize failed: {ex.Message}", ex)
                ' Cleanup partially-created backends
                Try : _encoder?.Dispose() : Catch : End Try
                Try : _capture?.Dispose() : Catch : End Try
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Start a new recording session. Blocks until session completes
        ''' (duration elapsed or Stop() called). Returns SessionResult.
        ''' Only ONE active session at a time.
        ''' </summary>
        Public Function StartSession(config As SessionConfig) As SessionResult
            If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))

            SyncLock _sync
                If _disposed Then Throw New ObjectDisposedException(NameOf(RecordingEngine))
                If _state <> RecordingEngineState.Idle Then
                    Throw New InvalidOperationException($"StartSession() called from state {_state}")
                End If
                _state = RecordingEngineState.Recording
            End SyncLock

            Dim result As SessionResult = Nothing
            Try
                _currentSession = New CaptureSession(
                    _capture, _encoder, config, _logger)
                result = _currentSession.Run()
                _currentSession = Nothing
            Catch ex As Exception
                _logger.Error($"RecordingEngine: session failed: {ex.Message}", ex)
                result = New SessionResult() With {
                    .OutputPath = config.OutputPath,
                    .RequestedDurationSec = config.DurationSeconds,
                    .ErrorMessage = ex.Message
                }
            Finally
                SyncLock _sync
                    _state = RecordingEngineState.Idle
                End SyncLock
            End Try

            Return result
        End Function

        ''' <summary>Signal early stop. The running session will return shortly.</summary>
        Public Sub [Stop]()
            SyncLock _sync
                If _disposed Then Return
            End SyncLock
            _currentSession?.[Stop]()
        End Sub

        Public Function GetStatus() As EngineStatus
            SyncLock _sync
                Return New EngineStatus() With {
                    .State = _state,
                    .CurrentSessionId = If(_currentSession IsNot Nothing, "active", Nothing)
                }
            End SyncLock
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _sync
                If _disposed Then Return
                _disposed = True
                _state = RecordingEngineState.Disposed
            End SyncLock

            ' Stop any active session first
            _currentSession?.[Stop]()
            Try : _currentSession?.Dispose() : Catch : End Try

            ' Dispose persistent backends (reverse order)
            Try : _encoder?.Dispose() : Catch ex As Exception : _logger.Warning($"encoder.Dispose: {ex.Message}") : End Try
            Try : _capture?.Dispose() : Catch ex As Exception : _logger.Warning($"capture.Dispose: {ex.Message}") : End Try

            _logger.Info("RecordingEngine: disposed")
        End Sub

        ' ─── Minimal IVideoBackendContext ──────────────────────────────────
        Private NotInheritable Class BackendContext
            Implements IVideoBackendContext

            Private ReadOnly _logger As EngineLogger

            Public Sub New(logger As EngineLogger)
                _logger = logger
            End Sub

            Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
                Get
                    Return _logger
                End Get
            End Property

            Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
                Get
                    Return VideoBackendKind.Ddagrab
                End Get
            End Property
        End Class

    End Class

End Namespace
