Option Strict On
Option Explicit On
Option Infer On

' CT4ConfigTruthTests.vb — PHASE 0 CONFIG TRUTH, deterministic (no
' hardware, no ffmpeg, no recording, no threads, no timing).
'
' OWNER flow (STEP 1 of the fix wave):
'   config.json → Load → change value in config → trigger next-recording
'   mapping → assert runtime got the NEW values.
'
' What runs here is REAL production code, linked by the vbproj:
'   CaptureSettings.Load   — Engine\[Capture]\CaptureSettings.vb
'                            (unified Overlay config.json WINS, :101-116)
'   OverlayConfig.*        — Engine\[Integration]\OverlayConfig.vb
'                            (the Overlay config.json parser the engine uses)
'   NextRecordingConfig.*  — Engine\[API]\NextRecordingConfig.vb (FIX-1 seam)
'   AppLayout.*            — Common\AppLayout.vb (linked into every app)
'
' Why the handler composition is expressed at the seam:
'   HandleRecordingStart is a WinForms form method (Me.Invoke, lblStatus,
'   tmrRecording) — not executable on Linux. After FIX-1 the handler
'   delegates to exactly:
'       LoadEffectiveSettings() + MapSessionConfig(...)   (≡ BuildSessionConfig)
'   and CT-4 executes that same production composition. The PRE-FIX
'   behavior (which CT-4 reproduced to prove the bug — see the BEFORE
'   line inside the main test) was:
'       MapSessionConfig(process-start snapshot, ...)     (RecordingEngineHost.vb:214-228
'                                                          mapping UI_Engine._settings,
'                                                          loaded once — UI_Engine.vb:616, :85)

Imports System
Imports System.IO
Imports CaptureEngine.Recording
Imports Engine.ConfigTruth.Tests

Friend Module CT4ConfigTruthTests

    ' ── Fixture: unified Overlay config.json (the ONE user-facing file) ──
    ' Shape = what Overlay's AppSettings.Save writes and what the engine's
    ' OverlayConfig mirror parses: nested Recording.current (video.json-
    ' shaped) + flat top-level Audio section (PascalCase).

    Private Function ConfigJson(systemEnabled As Boolean,
                                micEnabled As Boolean,
                                sysVol As Single,
                                micVol As Single,
                                micName As String,
                                micId As String,
                                trackMode As Integer,
                                clockMode As String) As String
        Dim inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("{")
        sb.AppendLine("  ""Recording"": {")
        sb.AppendLine("    ""encoder"": ""NVENC_H264"",")
        sb.AppendLine("    ""active_preset"": ""Medium"",")
        sb.AppendLine("    ""replay_duration"": 60,")
        sb.AppendLine("    ""current"": { ""fps"": 60, ""bitrate"": 17000, ""encoder_preset"": 4, ""use_native_resolution"": true, ""width"": 1920, ""height"": 1080 }")
        sb.AppendLine("  },")
        sb.AppendLine("  ""Audio"": {")
        sb.AppendLine("    ""SystemAudioEnabled"": " & systemEnabled.ToString(inv).ToLowerInvariant() & ",")
        sb.AppendLine("    ""MicEnabled"": " & micEnabled.ToString(inv).ToLowerInvariant() & ",")
        sb.AppendLine("    ""SystemAudioVolume"": " & sysVol.ToString(inv) & ",")
        sb.AppendLine("    ""MicVolume"": " & micVol.ToString(inv) & ",")
        sb.AppendLine("    ""MicDeviceName"": """ & micName & """,")
        sb.AppendLine("    ""MicDeviceId"": """ & micId & """,")
        sb.AppendLine("    ""TrackMode"": " & trackMode.ToString(inv) & ",")
        sb.AppendLine("    ""AudioClockMode"": """ & clockMode & """")
        sb.AppendLine("  },")
        sb.AppendLine("  ""Paths"": { ""GalleryPath"": """", ""SavePath"": """", ""FFmpegPath"": """" }")
        sb.AppendLine("}")
        Return sb.ToString()
    End Function

    Private Function WriteConfig(json As String) As String
        ' AppLayout.Dir = the test exe dir (no Application\ / Overlay\ leaf)
        ' → OverlayConfig.ResolveConfigDir step 2 lands on <exeDir>\Config.
        Dim cfgDir As String = Path.Combine(AppLayout.Dir, "Config")
        Directory.CreateDirectory(cfgDir)
        Dim cfgPath As String = Path.Combine(cfgDir, "config.json")
        File.WriteAllText(cfgPath, json)
        ' The unified config dir is cached process-wide — invalidate so
        ' every phase re-resolves deterministically.
        OverlayConfig.ResetResolvedPath()
        Return cfgPath
    End Function

    Public Sub RunAll()
        TestRunner.RunTest(
            "CT-4 next-recording uses FRESH config after persisted change",
            AddressOf CT4_NextRecording_UsesFreshConfigAfterPersistedChange)
        TestRunner.RunTest(
            "CT-4 MapSessionConfig preserves handler defaults (verbatim move)",
            AddressOf CT4_MapSessionConfig_PreservesHandlerDefaults)
    End Sub

    ''' <summary>
    ''' THE CT-4 contract: UI setting = persisted config = effective runtime
    ''' config. Exercises the exact production chain the next recording uses.
    ''' </summary>
    Private Sub CT4_NextRecording_UsesFreshConfigAfterPersistedChange()

        ' ── 1) OWNER's settings as persisted at process start ──
        '    (system ON, mic OFF, volumes 1.0, single track, Legacy clock)
        WriteConfig(ConfigJson(True, False, 1.0F, 1.0F, "", "", 0, "Legacy"))

        ' Process-start snapshot — the SAME object state UI_Engine._settings
        ' held pre-fix (LoadSettings UI_Engine.vb:616, called once at :85).
        Dim processStart As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())

        ' Control: the loader must reflect the persisted phase-A file.
        TestRunner.Assert(processStart.SystemAudioCapture = True,
                          "control phase-A: SystemAudioCapture expected True")
        TestRunner.Assert(processStart.MicCapture = False,
                          "control phase-A: MicCapture expected False")
        TestRunner.Assert(processStart.AudioClockMode = "Legacy",
                          $"control phase-A: AudioClockMode expected 'Legacy', got '{processStart.AudioClockMode}'")

        ' ── 2) OWNER changes settings in the Overlay UI → Overlay saves ──
        '    (system OFF, mic ON, volumes 0.25/0.7, separate track, Device clock)
        WriteConfig(ConfigJson(False, True, 0.25F, 0.7F, "CT4 Test Mic", "ct4-device-id", 1, "Device"))

        ' Control: a fresh load MUST see the change (this is what the fix
        ' relies on — it also isolates any loader failure from the mapping).
        Dim reloaded As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(reloaded.SystemAudioCapture = False,
                          "control reload: SystemAudioCapture expected False after persist")
        TestRunner.Assert(reloaded.MicCapture = True,
                          "control reload: MicCapture expected True after persist")

        ' ── 3) next-recording mapping — the production composition ──
        '
        ' BEFORE-FIX evidence run (reproduces HandleRecordingStart pre-fix):
        '   the handler mapped UI_Engine._settings — the process-start
        '   snapshot — so the phase-B change never reached runtime:
        '
        '   Dim runtime As SessionConfig =
        '       NextRecordingConfig.MapSessionConfig(processStart, "ct4-out.mp4", "ffmpeg", Nothing)
        '
        '   ↑ this line FAILED CT-4 exactly as designed (stale snapshot →
        '   phase-A values), proving the bug at HEAD fae0e6a.
        '
        ' AFTER FIX-1 (the permanent line): the handler runs
        '   LoadEffectiveSettings + SyncWithOverlayConfig + MapSessionConfig
        '   (RecordingEngineHost.vb, FIX-1 block) ≡ BuildSessionConfig:
        Dim runtime As SessionConfig =
            NextRecordingConfig.BuildSessionConfig("ct4-out.mp4", "ffmpeg", Nothing)

        ' [BEFORE-EVIDENCE RUN] the pre-fix composition that FAILED this
        ' test — kept as documentation of the exact wiring change:
        ' Dim runtime As SessionConfig =
        '     NextRecordingConfig.MapSessionConfig(processStart, "ct4-out.mp4", "ffmpeg", Nothing)

        ' ── 4) runtime must use the NEW (phase-B) values ──
        TestRunner.Assert(runtime.AudioEnabled = False,
                          $"AudioEnabled: expected False (persisted), got {runtime.AudioEnabled} — stale config reached runtime")
        TestRunner.Assert(runtime.MicEnabled = True,
                          $"MicEnabled: expected True (persisted), got {runtime.MicEnabled} — stale config reached runtime")
        TestRunner.Assert(Math.Abs(runtime.SystemVolume - 0.25F) < 0.0001F,
                          $"SystemVolume: expected 0.25 (persisted), got {runtime.SystemVolume} — stale config reached runtime")
        TestRunner.Assert(Math.Abs(runtime.MicVolume - 0.7F) < 0.0001F,
                          $"MicVolume: expected 0.7 (persisted), got {runtime.MicVolume} — stale config reached runtime")
        TestRunner.Assert(runtime.MicDeviceId = "ct4-device-id",
                          $"MicDeviceId: expected 'ct4-device-id' (persisted), got '{runtime.MicDeviceId}' — stale config reached runtime")
        TestRunner.Assert(runtime.MicDeviceName = "CT4 Test Mic",
                          $"MicDeviceName: expected 'CT4 Test Mic' (persisted), got '{runtime.MicDeviceName}' — stale config reached runtime")
        TestRunner.Assert(runtime.MicSeparateTracks = True,
                          $"MicSeparateTracks: expected True (persisted TrackMode=1), got {runtime.MicSeparateTracks} — stale config reached runtime")
        TestRunner.Assert(runtime.AudioClockMode = "Device",
                          $"AudioClockMode: expected 'Device' (persisted), got '{runtime.AudioClockMode}' — stale config reached runtime")
    End Sub

    ''' <summary>
    ''' Pins the VERBATIM-move property: MapSessionConfig with Nothing
    ''' settings must yield exactly the fallbacks the pre-fix inline
    ''' mapping produced (RecordingEngineHost.vb:218-226 — every
    ''' If(_settings IsNot Nothing, ..., default) branch). Guards the
    ''' extraction against accidental semantic drift.
    ''' </summary>
    Private Sub CT4_MapSessionConfig_PreservesHandlerDefaults()
        Dim runtime As SessionConfig =
            NextRecordingConfig.MapSessionConfig(Nothing, "ct4-out.mp4", "ffmpeg", Nothing)

        ' .AudioEnabled = (_settings Is Nothing) OrElse _settings.SystemAudioCapture
        '   → Nothing settings ⇒ True (pre-fix semantics, preserved).
        TestRunner.Assert(runtime.AudioEnabled = True,
                          $"AudioEnabled default: expected True (Nothing-settings fallback), got {runtime.AudioEnabled}")
        TestRunner.Assert(runtime.MicEnabled = False,
                          $"MicEnabled default: expected False, got {runtime.MicEnabled}")
        TestRunner.Assert(Math.Abs(runtime.SystemVolume - 1.0F) < 0.0001F,
                          $"SystemVolume default: expected 1.0, got {runtime.SystemVolume}")
        TestRunner.Assert(Math.Abs(runtime.MicVolume - 1.0F) < 0.0001F,
                          $"MicVolume default: expected 1.0, got {runtime.MicVolume}")
        TestRunner.Assert(runtime.MicDeviceId = "",
                          $"MicDeviceId default: expected empty, got '{runtime.MicDeviceId}'")
        TestRunner.Assert(runtime.MicDeviceName = "",
                          $"MicDeviceName default: expected empty, got '{runtime.MicDeviceName}'")
        TestRunner.Assert(runtime.MicSeparateTracks = False,
                          $"MicSeparateTracks default: expected False, got {runtime.MicSeparateTracks}")
        TestRunner.Assert(runtime.AudioClockMode = "Legacy",
                          $"AudioClockMode default: expected 'Legacy', got '{runtime.AudioClockMode}'")
        TestRunner.Assert(runtime.DurationSeconds = 3600,
                          $"DurationSeconds: expected 3600 (no fixed duration), got {runtime.DurationSeconds}")
        TestRunner.Assert(runtime.OutputPath = "ct4-out.mp4",
                          $"OutputPath: expected 'ct4-out.mp4', got '{runtime.OutputPath}'")
        TestRunner.Assert(runtime.FFmpegPath = "ffmpeg",
                          $"FFmpegPath: expected 'ffmpeg', got '{runtime.FFmpegPath}'")
        TestRunner.Assert(runtime.OnProcessStarted Is Nothing,
                          "OnProcessStarted: expected Nothing when not provided")
    End Sub

End Module
