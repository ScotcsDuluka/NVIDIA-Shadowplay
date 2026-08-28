Option Strict On
Option Explicit On
Option Infer On

' RecordingEngineHost.vb
'
' Partial class of UI_Engine that adds Phase 12b RecordingEngine integration.
' Replaces the legacy CaptureEngine (FFmpeg subprocess) with the new
' RecordingEngine (D3D11 + NVENC + NAudio sidecar + FFmpeg mux).
'
' Architecture:
'   RecordingEngine (process-lifetime, persistent GPU resources)
'       ↓ StartSession(config) — BLOCKS until duration or Stop()
'       ↓ Run on background thread (Task.Run) → UI thread not blocked
'   CaptureSession (per-session: audio sidecar + H.264 + wrap + mux → MP4)
'
' Phase 12b hardening (this file):
'   ✔ Initialize runs OFF the UI thread (D3D11 + DXGI + NVENC init can take
'     hundreds of ms — must never block form load / message pump)
'   ✔ FFmpeg resolution order per Phase-11 postmortem lesson #1:
'     settings path → deployment root (exe dir + API-Core) → exe dir → PATH
'   ✔ Full Config → SessionConfig/EngineStartupConfig mapping from
'     CaptureSettings (codec, bitrate, GOP=fps, audio enable, volume)
'   ✔ JobObjectGuard (KILL_ON_JOB_CLOSE) owns every spawned ffmpeg/ffprobe
'     via the OnProcessStarted hook → no orphan FFmpeg, even on host crash
'
' Integration:
'   HandleEngineRecordStart → start background task → respond "ok" immediately
'   HandleEngineRecordStop  → _recordingEngine.Stop() → await task → respond
'   HandleEngineGetStatus   → _recordingEngine.GetStatus() → respond

Imports System.IO
Imports System.Threading.Tasks
Imports CaptureEngine.Recording
Imports CaptureEngine.Diagnostics

Partial Public Class UI_Engine

    ' ─── RecordingEngine (Phase 12b) ─────────────────────────────────
    Private _recordingEngine As RecordingEngine
    Private _recordingTask As Task(Of SessionResult)
    Private _useNewEngine As Boolean = True  ' True = use RecordingEngine, False = legacy
    Private _engineReady As Boolean = False  ' set once off-thread Initialize succeeds

    ' Phase 12b: one job object owns every child ffmpeg this host spawns
    ' (recording pipeline + legacy path guards). KILL_ON_JOB_CLOSE means
    ' a host crash can never leave an orphan ffmpeg behind.
    Private _engineJobGuard As JobObjectGuard

    ''' <summary>
    ''' Initialize RecordingEngine on a BACKGROUND thread (Phase 12b fix —
    ''' was synchronous on the UI thread). Called from UI_Engine_Load.
    ''' Creates persistent D3D11 + DXGI + NVENC resources (process-lifetime)
    ''' and the shared JobObjectGuard.
    ''' </summary>
    Private Sub InitializeRecordingEngine()
        Try
            _engineJobGuard = New JobObjectGuard()
        Catch ex As Exception
            ' Best-effort orphan protection — engine still works without it.
            DebugLog($"[RecordingEngine] JobObjectGuard unavailable: {ex.Message}")
        End Try

        Dim settingsSnapshot As CaptureSettings = _settings
        Dim baseDir As String = AppLayout.Dir

        Task.Run(Sub()
                     Try
                         Dim logger As New EngineLogger("RecordingEngine", EngineLogger.LogLevel.Info, AddressOf DebugLog)
                         Dim engine As New RecordingEngine(logger)

                         ' Startup config from Overlay settings (persistent
                         ' encoder session — codec/bitrate/GOP are one-shot).
                         Dim startup As New EngineStartupConfig()
                         If settingsSnapshot IsNot Nothing Then
                             If Not String.IsNullOrEmpty(settingsSnapshot.Encoder) Then
                                 ' Fallback-fix: CaptureSettings.Encoder may hold a
                                 ' FFMPEG name ('h264_nvenc') — the encoder contract
                                 ' wants the internal key ('NVENC_H264'). Normalize
                                 ' BOTH directions (owner log 20:01:46 showed the
                                 ' engine silently falling back to legacy for a
                                 ' whole day because of exactly this).
                                 Dim internalKey As String = OverlayConfig.MapEncoderToInternal(settingsSnapshot.Encoder)
                                 If Not String.Equals(internalKey, settingsSnapshot.Encoder, StringComparison.OrdinalIgnoreCase) Then
                                     DebugLog($"[RecordingEngine] encoder name normalized: '{settingsSnapshot.Encoder}' → '{internalKey}'")
                                 End If
                                 startup.CodecKey = internalKey
                             End If
                             If settingsSnapshot.Bitrate > 0 Then
                                 startup.BitrateBps = settingsSnapshot.Bitrate
                             End If
                             If settingsSnapshot.FPS > 0 Then
                                 startup.GopSize = settingsSnapshot.FPS
                             End If
                             If Not String.IsNullOrEmpty(settingsSnapshot.RateControl) Then
                                 startup.RateControl = settingsSnapshot.RateControl
                             End If
                             If Not String.IsNullOrEmpty(settingsSnapshot.Preset) Then
                                 startup.Preset = settingsSnapshot.Preset
                             End If
                         End If

                         engine.Initialize(startup)

                         _recordingEngine = engine
                         _engineReady = True
                         DebugLog("[RecordingEngine] initialized — Idle (background thread)")
                     Catch ex As Exception
                         DebugLog($"[RecordingEngine] Initialize FAILED: {ex.Message}")
                         _recordingEngine = Nothing
                         _engineReady = False
                         _useNewEngine = False
                         DebugLog("[RecordingEngine] falling back to legacy CaptureEngine")
                     End Try
                 End Sub)
    End Sub

    ''' <summary>
    ''' Dispose RecordingEngine + job guard. Called from UI_Engine_FormClosing.
    ''' Order matters: engine first (stops any active session → mux finishes),
    ''' THEN the job guard (its handle close would kill ffmpeg children).
    ''' </summary>
    Private Sub DisposeRecordingEngine()
        Try
            _recordingEngine?.Dispose()
            DebugLog("[RecordingEngine] disposed")
        Catch ex As Exception
            DebugLog($"[RecordingEngine] dispose error: {ex.Message}")
        End Try

        Try
            _engineJobGuard?.Dispose()
            DebugLog("[RecordingEngine] job guard disposed")
        Catch ex As Exception
            DebugLog($"[RecordingEngine] job guard dispose error: {ex.Message}")
        End Try
    End Sub

    ' ─── FFmpeg path resolution (Phase-11 postmortem lesson #1) ──────

    ''' <summary>
    ''' Resolve the real ffmpeg.exe path. The Overlay\API-Core directory is
    ''' part of the deployment contract — NEVER rely on PATH alone.
    ''' Resolution order:
    '''   1. CaptureSettings.FFmpegPath (if the file exists)
    '''   2. {exe dir}\API-Core\ffmpeg.exe   (deployment root)
    '''   3. {exe dir}\ffmpeg.exe
    '''   4. bare "ffmpeg" (PATH — last resort, logged loudly)
    ''' </summary>
    Private Function ResolveFFmpegPath() As String
        Dim candidates As New List(Of String)

        Dim settingsPath As String = _settings?.FFmpegPath
        If Not String.IsNullOrEmpty(settingsPath) Then candidates.Add(settingsPath)

        Dim baseDir As String = AppLayout.Dir
        candidates.Add(AppLayout.P("FFmpeg", "ffmpeg.exe"))
        candidates.Add(Path.Combine(baseDir, "API-Core", "ffmpeg.exe"))
        candidates.Add(Path.Combine(baseDir, "ffmpeg.exe"))

        For Each c As String In candidates
            Try
                If File.Exists(c) Then
                    DebugLog($"[RecordingEngine] ffmpeg resolved: {c}")
                    Return c
                End If
            Catch
            End Try
        Next

        DebugLog("[RecordingEngine] WARNING — ffmpeg NOT found in settings/deployment; falling back to PATH lookup")
        Return "ffmpeg"
    End Function

    ' ─── Recording command handlers (Phase 12b) ──────────────────────

    ''' <summary>
    ''' Start recording using RecordingEngine. Non-blocking — starts a
    ''' background task that runs StartSession() and responds immediately.
    ''' </summary>
    Private Async Function HandleRecordingStart(value As String, reqId As String) As Task
        Try
            If Not _engineReady OrElse _recordingEngine Is Nothing Then
                SendResponse("engine_record_start", "error", "engine_not_ready", reqId)
                Return
            End If

            If _recordingTask IsNot Nothing AndAlso Not _recordingTask.IsCompleted Then
                SendResponse("engine_record_start", "error", "already_recording", reqId)
                Return
            End If

            ' Resolve FFmpeg path (deployment contract — Phase 11 lesson #1)
            Dim ffmpegPath As String = ResolveFFmpegPath()
            If ffmpegPath = "ffmpeg" AndAlso _settings?.FFmpegPath IsNot Nothing AndAlso _settings.FFmpegPath.Length > 0 Then
                ' Settings named a path that does not exist — surface it.
                SendResponse("engine_record_start", "error", "ffmpeg_not_found: " & _settings.FFmpegPath, reqId)
                Return
            End If

            ' Create session config (full Phase 12b mapping + M1 mic mapping from audio.json)
            ' P13.4: AudioClockMode plumbing — audio.json "AudioClockMode":
            ' "Device" = hardware-stamped timeline (WasapiPositionCapture +
            ' AudioTapDeviceClock), "Legacy" = proven v2 path.
            Dim config As New SessionConfig() With {
                .OutputPath = value,
                .DurationSeconds = 3600,          ' no fixed duration — stop via command
                .FFmpegPath = ffmpegPath,
                .AudioEnabled = (_settings Is Nothing) OrElse _settings.SystemAudioCapture,
                .SystemVolume = If(_settings IsNot Nothing, _settings.SystemAudioVolume, 1.0F),
                .MicEnabled = (_settings IsNot Nothing) AndAlso _settings.MicCapture,
                .MicVolume = If(_settings IsNot Nothing, _settings.MicVolume, 1.0F),
                .MicDeviceId = If(_settings IsNot Nothing, _settings.MicDeviceId, ""),
                .MicDeviceName = If(_settings IsNot Nothing, _settings.MicDeviceName, ""),
                .MicSeparateTracks = (_settings IsNot Nothing) AndAlso
                                     _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack,
                .AudioClockMode = If(_settings IsNot Nothing, _settings.AudioClockMode, "Legacy"),
                .OnProcessStarted = AddressOf AssignChildToJob
            }

            DebugLog($"[RecordingEngine] starting session: path={value}, audio={config.AudioEnabled}, mic={config.MicEnabled}")

            ' Start on background thread — StartSession blocks until done
            _recordingTask = Task.Run(Function() _recordingEngine.StartSession(config))

            ' Respond immediately — recording has started
            ' UI update on UI thread
            Me.Invoke(Sub()
                          lblStatus.Text = "Recording (Hub)..."
                          lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                          tmrRecording.Start()
                          btnRecord.Enabled = False
                          btnStop.Enabled = True
                      End Sub)

            SendResponse("engine_record_start", "ok", value, reqId)
        Catch ex As Exception
            DebugLog($"[RecordingEngine] start error: {ex.Message}")
            SendResponse("engine_record_start", "error", ex.Message, reqId)
        End Try
    End Function

    ''' <summary>
    ''' Stop recording. Signals RecordingEngine.Stop() and waits for the
    ''' background task to complete. Returns the session result.
    ''' </summary>
    Private Async Function HandleRecordingStop(reqId As String) As Task
        Try
            If _recordingEngine Is Nothing OrElse _recordingTask Is Nothing Then
                SendResponse("engine_record_stop", "error", "not_recording", reqId)
                Return
            End If

            DebugLog("[RecordingEngine] stopping...")
            _recordingEngine.Stop()

            ' Wait for the session to complete (should be quick after Stop)
            Dim result As SessionResult = Await _recordingTask
            _recordingTask = Nothing

            DebugLog($"[RecordingEngine] stopped: pass={result.Pass}, file={result.OutputPath}")
            DebugLog($"[RecordingEngine] evidence: frames={result.FramesEncoded}, audioBytes={result.AudioBytes}, " &
                     $"dropped={result.AudioDroppedBytes}, accountingOk={result.AudioAccountingOk}, " &
                     $"offset={result.SystemOffsetSec:0.000}s, muxDur={result.MuxVideoDurationSec:0.000}s")

            ' UI update
            Me.Invoke(Sub()
                          tmrRecording.Stop()
                          lblTimer.Text = "00:00:00"
                          lblStatus.Text = "Saved: " & Path.GetFileName(result.OutputPath)
                          lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                          btnRecord.Enabled = True
                          btnStop.Enabled = False
                      End Sub)

            If result.Pass Then
                SendResponse("engine_record_stop", "ok", result.OutputPath, reqId)
            Else
                Dim why As String = $"pass=False: frames={result.FramesEncoded}, file={result.FileExists}, " &
                                    $"video={result.VideoStreamFound}, audio={result.AudioStreamFound}" &
                                    If(String.IsNullOrEmpty(result.ErrorMessage), "", $", err={result.ErrorMessage}")
                SendResponse("engine_record_stop", "error", why, reqId)
            End If
        Catch ex As Exception
            DebugLog($"[RecordingEngine] stop error: {ex.Message}")
            SendResponse("engine_record_stop", "error", ex.Message, reqId)
        End Try
    End Function

    ''' <summary>
    ''' Get recording status.
    ''' </summary>
    Private Sub HandleRecordingGetStatus(reqId As String)
        Try
            If _recordingEngine Is Nothing Then
                SendResponse("engine_get_status", "ok", If(_engineReady, "Idle", "Initializing"), reqId)
                Return
            End If

            Dim status As EngineStatus = _recordingEngine.GetStatus()
            SendResponse("engine_get_status", "ok", status.State.ToString(), reqId)
        Catch ex As Exception
            SendResponse("engine_get_status", "error", ex.Message, reqId)
        End Try
    End Sub

    ' ─── Job-object assignment (no-orphan-FFmpeg criterion) ──────────

    ''' <summary>
    ''' Assign a spawned child (ffmpeg/ffprobe) to the host's job object.
    ''' Passed into SessionConfig.OnProcessStarted and MuxCoordinator.
    ''' </summary>
    Private Sub AssignChildToJob(proc As System.Diagnostics.Process)
        If _engineJobGuard IsNot Nothing AndAlso proc IsNot Nothing Then
            Try
                _engineJobGuard.Assign(proc)
                DebugLog($"[RecordingEngine] child → job object: {proc.ProcessName} (pid {proc.Id})")
            Catch ex As Exception
                DebugLog($"[RecordingEngine] job assign failed for pid {proc.Id}: {ex.Message}")
            End Try
        End If
    End Sub

End Class
