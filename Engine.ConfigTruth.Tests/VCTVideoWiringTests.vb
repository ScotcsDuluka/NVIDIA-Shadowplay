Option Strict On
Option Explicit On
Option Infer On

' VCTVideoWiringTests.vb — PHASE 1 VIDEO RUNTIME WIRING (deterministic, no
' hardware, no ffmpeg, no recording, no threads).
'
' OWNER acceptance tests V-CT1..V-CT5 mapped onto the Linux-runnable seams:
'   V-CT1  FPS        : config.json Recording.current.fps=75 → effective 75
'                       (SessionConfig.TargetFps — pre-wiring this value never
'                        reached the runtime; CaptureSession read the display
'                        refresh rate instead, CaptureSession.vb:489/:541)
'   V-CT1b FPS≠GOP    : config fps=75 must NOT become GopSize=75 (the
'                       pre-wiring host mapped FPS→GOP,
'                       RecordingEngineHost.vb:96-98 — accidental coupling)
'   V-CT2  RESOLUTION : native=false 1280x720 → effective 1280x720
'                       (commit 2 — ResolveEncodeDimensions)
'   V-CT3  BITRATE    : config 10200 kbps → 10,200,000 bps through every
'                       mapping layer (commit 3)
'   V-CT4  PRESET     : encoder_preset=X → internal preset=X (commit 3)
'   V-CT5  NVENC      : init params non-default/zero (commit 3 —
'                       NvEncParamBuilder contract tests, Encoder.Tests)
'
' What runs here is REAL production code, linked by the vbproj:
'   CaptureSettings.Load / OverlayConfig.* / NextRecordingConfig.* /
'   CaptureSettings (Engine\[Capture]) — same files the Engine compiles.

Imports System
Imports System.IO
Imports CaptureEngine.Recording
Imports Engine.ConfigTruth.Tests

Friend Module VCTVideoWiringTests

    ' ── Fixture: unified Overlay config.json with explicit video values ──

    Private Function VideoConfigJson(fps As Integer,
                                     bitrateKbps As Integer,
                                     encoderPreset As Integer,
                                     useNative As Boolean,
                                     width As Integer,
                                     height As Integer) As String
        Dim inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("{")
        sb.AppendLine("  ""Recording"": {")
        sb.AppendLine("    ""encoder"": ""NVENC_H264"",")
        sb.AppendLine("    ""active_preset"": ""Medium"",")
        sb.AppendLine("    ""replay_duration"": 60,")
        sb.AppendLine("    ""current"": { " &
                      $"""fps"": {fps.ToString(inv)}, " &
                      $"""bitrate"": {bitrateKbps.ToString(inv)}, " &
                      $"""encoder_preset"": {encoderPreset.ToString(inv)}, " &
                      $"""use_native_resolution"": {useNative.ToString(inv).ToLowerInvariant()}, " &
                      $"""width"": {width.ToString(inv)}, " &
                      $"""height"": {height.ToString(inv)} }}")
        sb.AppendLine("  },")
        sb.AppendLine("  ""Audio"": {")
        sb.AppendLine("    ""SystemAudioEnabled"": true,")
        sb.AppendLine("    ""MicEnabled"": false,")
        sb.AppendLine("    ""SystemAudioVolume"": 1.0,")
        sb.AppendLine("    ""MicVolume"": 1.0,")
        sb.AppendLine("    ""MicDeviceName"": """",")
        sb.AppendLine("    ""MicDeviceId"": """",")
        sb.AppendLine("    ""TrackMode"": 0,")
        sb.AppendLine("    ""AudioClockMode"": ""Legacy""")
        sb.AppendLine("  },")
        sb.AppendLine("  ""Paths"": { ""GalleryPath"": """", ""SavePath"": """", ""FFmpegPath"": """" }")
        sb.AppendLine("}")
        Return sb.ToString()
    End Function

    Private Sub WriteVideoConfig(json As String)
        Dim cfgDir As String = Path.Combine(AppLayout.Dir, "Config")
        Directory.CreateDirectory(cfgDir)
        File.WriteAllText(Path.Combine(cfgDir, "config.json"), json)
        OverlayConfig.ResetResolvedPath()
    End Sub

    Public Sub RunAll()
        TestRunner.RunTest(
            "V-CT1 config FPS=75 → effective SessionConfig.TargetFps=75 (FPS wiring)",
            AddressOf VCT1_ConfigFps_ReachesSessionTargetFps)
        TestRunner.RunTest(
            "V-CT1b config FPS=75 does NOT leak into EngineStartupConfig.GopSize",
            AddressOf VCT1b_Fps_DoesNotBecomeGop)
        TestRunner.RunTest(
            "V-CT1c MapSessionConfig Nothing-settings → TargetFps=0 (explicit unset, not display)",
            AddressOf VCT1c_NothingSettings_TargetFpsUnset)
    End Sub

    ''' <summary>
    ''' V-CT1: the owner sets FPS=75 in config.json → presses Record → the
    ''' effective per-session config MUST carry 75. Exercises the exact
    ''' production chain: CaptureSettings.Load (unified config.json WINS) →
    ''' NextRecordingConfig.MapSessionConfig.
    ''' </summary>
    Private Sub VCT1_ConfigFps_ReachesSessionTargetFps()
        WriteVideoConfig(VideoConfigJson(75, 17000, 4, True, 1920, 1080))

        ' Control: the loader must reflect the persisted FPS.
        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(settings.FPS = 75,
                          $"control: CaptureSettings.FPS expected 75 (persisted), got {settings.FPS}")

        ' The composition HandleRecordingStart runs (FIX-1 fresh reload → map).
        Dim runtime As SessionConfig =
            NextRecordingConfig.MapSessionConfig(settings, "vct1-out.mp4", "ffmpeg", Nothing)

        TestRunner.Assert(runtime.TargetFps = 75,
                          $"effective: SessionConfig.TargetFps expected 75 (config fps), got {runtime.TargetFps} — config FPS did not reach the session runtime")
    End Sub

    ''' <summary>
    ''' V-CT1b: FPS and GOP are SEPARATE settings. The pre-wiring host code
    ''' mapped settingsSnapshot.FPS → startup.GopSize
    ''' (RecordingEngineHost.vb:96-98) — an accidental coupling this phase
    ''' removes. GOP stays at the engine default (60) regardless of FPS.
    ''' </summary>
    Private Sub VCT1b_Fps_DoesNotBecomeGop()
        WriteVideoConfig(VideoConfigJson(75, 17000, 4, True, 1920, 1080))

        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(settings.FPS = 75, "control: settings.FPS expected 75")

        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(settings)

        TestRunner.Assert(startup.Fps = 75,
                          $"startup.Fps expected 75 (config fps), got {startup.Fps}")
        TestRunner.Assert(startup.GopSize = 60,
                          $"GopSize must stay at the engine default 60 when config carries no GOP — got {startup.GopSize} (FPS→GOP accidental mapping is back)")
    End Sub

    ''' <summary>
    ''' V-CT1c: with NO settings the mapping yields TargetFps=0 — an EXPLICIT
    ''' unset that CaptureSession reports loudly (fallback 60 + warning).
    ''' The display refresh rate is never substituted silently.
    ''' </summary>
    Private Sub VCT1c_NothingSettings_TargetFpsUnset()
        Dim runtime As SessionConfig =
            NextRecordingConfig.MapSessionConfig(Nothing, "vct1c-out.mp4", "ffmpeg", Nothing)

        TestRunner.Assert(runtime.TargetFps = 0,
                          $"TargetFps expected 0 (unset → session warns + falls back 60), got {runtime.TargetFps} — nothing may invent an FPS source")

        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(Nothing)
        TestRunner.Assert(startup.Fps = 0,
                          $"startup.Fps expected 0 (unset), got {startup.Fps}")
        TestRunner.Assert(startup.GopSize = 60,
                          $"GopSize default expected 60, got {startup.GopSize}")
    End Sub

End Module
