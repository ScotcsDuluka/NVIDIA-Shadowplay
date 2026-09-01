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

        ' ★ PHASE 1 VIDEO RUNTIME WIRING (V-CT2): the effective startup values
        ' this engine was initialized with — the immutable per-session echo
        ' source for requested-vs-actual evidence.
        Private _startupEcho As EngineStartupConfig

        ' ─── Current session ─────────────────────────────────────────────
        Private _currentSession As CaptureSession

        Public Sub New(logger As EngineLogger)
            _logger = logger
        End Sub

        ''' <summary>
        ''' Initialize persistent GPU resources (D3D11 + DXGI + NVENC).
        ''' Called ONCE at process start. Subsequent calls throw.
        ''' Uses the proven default startup config (NVENC_H264 / 20 Mbps / GOP 60 / p4).
        ''' </summary>
        Public Sub Initialize()
            Initialize(New EngineStartupConfig())
        End Sub

        ''' <summary>
        ''' Phase 12b: initialize with host-provided startup options
        ''' (codec / bitrate / GOP / preset come from Overlay settings).
        ''' The encoder session is process-lifetime — these values cannot
        ''' change per session without an engine rebuild.
        ''' </summary>
        Public Sub Initialize(startup As EngineStartupConfig)
            If startup Is Nothing Then startup = New EngineStartupConfig()

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
                _logger.Info($"RecordingEngine: DdagrabBackend initialized — {_capture.OutputWidth}x{_capture.OutputHeight} @ {_capture.OutputRefreshRate}Hz")

                ' ─── Initialize NvencEncoderBackend (creates D3D11 + NVENC session) ─
                _encoder = New NvencEncoderBackend(_logger)
                Dim gop As Integer = If(startup.GopSize > 0, startup.GopSize, 60)

                ' ★ PHASE 1 VIDEO RUNTIME WIRING (V-CT2): resolution group.
                ' The capture backend grabs the DESKTOP at its native size;
                ' the encode size follows config.json:
                '   use_native_resolution=true            → desktop size
                '   use_native_resolution=false + w/h     → NVENC scales (GPU)
                ' Oversized requests fail LOUDLY (no silent desktop fallback).
                Dim encodeDims As Tuple(Of Integer, Integer) =
                    EngineStartupConfig.ResolveEncodeDimensions(
                        _capture.OutputWidth, _capture.OutputHeight,
                        startup.UseNativeResolution, startup.RequestedWidth, startup.RequestedHeight)
                If encodeDims.Item1 <> _capture.OutputWidth OrElse encodeDims.Item2 <> _capture.OutputHeight Then
                    _logger.Info($"[RecordingEngine] resolution: requested {startup.RequestedWidth}x{startup.RequestedHeight} (use_native_resolution=False) → NVENC encode {encodeDims.Item1}x{encodeDims.Item2} from capture {_capture.OutputWidth}x{_capture.OutputHeight}")
                Else
                    _logger.Info($"[RecordingEngine] resolution: native (use_native_resolution=True or unset) → encode {_capture.OutputWidth}x{_capture.OutputHeight}")
                End If

                Dim encConfig As New EncoderConfig() With {
                    .CodecKey = If(String.IsNullOrEmpty(startup.CodecKey), "NVENC_H264", startup.CodecKey),
                    .BitrateBps = If(startup.BitrateBps > 0, startup.BitrateBps, 20_000_000L),
                    .MinrateBps = If(startup.BitrateBps > 0, startup.BitrateBps, 20_000_000L),
                    .MaxrateBps = If(startup.BitrateBps > 0, startup.BitrateBps, 20_000_000L),
                    .BufsizeBps = If(startup.BitrateBps > 0, startup.BitrateBps * 2, 40_000_000L),
                    .GopSize = gop,
                    .RateControl = If(String.IsNullOrEmpty(startup.RateControl), "cbr", startup.RateControl),
                    .Preset = If(String.IsNullOrEmpty(startup.Preset), "p4", startup.Preset),
                    .FrameRateFps = startup.Fps,
                    .ExpectedWidth = _capture.OutputWidth,
                    .ExpectedHeight = _capture.OutputHeight,
                    .EncodeWidth = encodeDims.Item1,
                    .EncodeHeight = encodeDims.Item2
                }
                _encoder.Initialize(encConfig)
                _startupEcho = startup

                ' ★ PHASE 1 (task §9): requested → selected → actual capture
                ' method. The New Engine has exactly ONE production backend;
                ' anything else requested is a recorded GAP, never a silent
                ' substitute and never a fake selector.
                Dim requestedMethod As String = If(startup.RequestedCaptureMethod, "").Trim()
                If requestedMethod.Length = 0 OrElse String.Equals(requestedMethod, "ddagrab", StringComparison.OrdinalIgnoreCase) Then
                    _logger.Info($"[RecordingEngine] capture method: requested='{If(requestedMethod.Length = 0, "(default)", requestedMethod)}' → selected=DdagrabBackend → actual=DdagrabBackend (DXGI Desktop Duplication)")
                Else
                    _logger.Warning($"[RecordingEngine] capture method: requested='{requestedMethod}' → GAP: '{requestedMethod}' is not implemented in the New Engine (only production backend = ddagrab) — running DdagrabBackend. Gap recorded, NOT silently accepted.")
                End If

                ' ★ PHASE 1 (task §8): pixel format truth line. The pipeline is
                ' BGRA (D3D11) → NVENC ARGB. A config nv12 would need a GPU
                ' conversion layer between capture and NVENC — NOT implemented
                ' (BLOCKER P1-PIXFMT); the config value is reported honestly,
                ' no fake pass.
                _logger.Info($"[RecordingEngine] pixel format: config='{If(String.IsNullOrEmpty(startup.RequestedPixelFormat), "(default nv12)", startup.RequestedPixelFormat)}' → runtime=BGRA8 (D3D11 capture) → NVENC input=ARGB — nv12 conversion NOT implemented (BLOCKER P1-PIXFMT); recording continues BGRA/ARGB")

                Dim fpsEcho As String = If(startup.Fps > 0, startup.Fps.ToString(), "unset (60 default)")
                _logger.Info($"RecordingEngine: NvencEncoderBackend initialized ({encConfig.CodecKey}, {encConfig.BitrateBps} bps, GOP {encConfig.GopSize}, preset {encConfig.Preset})")
                _logger.Info($"[RecordingEngine] effective video config (startup): codec={encConfig.CodecKey}, fps={fpsEcho}, bitrate={encConfig.BitrateBps} bps, rc={encConfig.RateControl}, preset={encConfig.Preset}, gop={encConfig.GopSize} (GOP independent of FPS — PHASE 1)")

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
        ''' PHASE 3 UI diagnostics: the startup config this engine was
        ''' initialized with (init-time immutable). Nothing before
        ''' Initialize succeeds — callers must handle Nothing.
        ''' Read-only echo for the Effective Runtime panel (UI spec §12);
        ''' no runtime semantics.
        ''' </summary>
        Public ReadOnly Property StartupEcho As EngineStartupConfig
            Get
                Return _startupEcho
            End Get
        End Property

        ''' <summary>
        ''' PHASE 3 UI diagnostics: actual capture backend geometry
        ''' ("WxH @ NNNHz"), init-time immutable. Empty string before the
        ''' backend exists. Read-only echo, no runtime semantics.
        ''' </summary>
        Public ReadOnly Property CaptureGeometry As String
            Get
                If _capture Is Nothing Then Return ""
                Return $"{_capture.OutputWidth}x{_capture.OutputHeight} @ {_capture.OutputRefreshRate}Hz"
            End Get
        End Property

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
                ' ★ PHASE 1 VIDEO RUNTIME WIRING (V-CT2): stamp the ACTUAL
                ' encode dimensions the persistent encoder runs with, so the
                ' session evidence always shows requested vs actual.
                ' (The encoder dims are init-time; the per-session request is
                ' preserved in the echo fields.)
                Dim echo As EngineStartupConfig = If(_startupEcho, New EngineStartupConfig())
                config.UseNativeResolution = echo.UseNativeResolution
                config.RequestedWidth = echo.RequestedWidth
                config.RequestedHeight = echo.RequestedHeight
                config.EncodeWidth = _encoder.EncodeWidthOutput
                config.EncodeHeight = _encoder.EncodeHeightOutput

                _currentSession = New CaptureSession(
                    _capture, _encoder, config, _logger)
                result = _currentSession.Run()
                _currentSession = Nothing
                _lastSessionResult = result
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
                    .CurrentSessionId = If(_currentSession IsNot Nothing, "active", Nothing),
                    .LastSessionResult = _lastSessionResult
                }
            End SyncLock
        End Function

        Private _lastSessionResult As SessionResult = Nothing

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
