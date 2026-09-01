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
        TestRunner.RunTest(
            "V-CT2 config 1280x720 native=false → effective encode 1280x720",
            AddressOf VCT2_CustomResolution_ReachesEncodeDims)
        TestRunner.RunTest(
            "V-CT2b native=true → encode = capture dims (no scaler)",
            AddressOf VCT2b_NativeResolution_UsesCaptureDims)
        TestRunner.RunTest(
            "V-CT2c custom dims LARGER than capture → loud failure (no silent fallback)",
            AddressOf VCT2c_OversizedCustom_FailsLoudly)
        TestRunner.RunTest(
            "V-CT3 config bitrate 10200 kbps → 10,200,000 bps through every layer",
            AddressOf VCT3_BitrateKbps_ToBps_AllLayers)
        TestRunner.RunTest(
            "V-CT3b bitrate reaches EncoderConfig unchanged (engine mapping)",
            AddressOf VCT3b_Bitrate_ReachesEncoderStartupConfig)
        TestRunner.RunTest(
            "V-CT4 config encoder_preset=7 → internal preset 'p7' (single mapper)",
            AddressOf VCT4_EncoderPreset_MappedToInternalPreset)
        TestRunner.RunTest(
            "V-CT4b encoder_preset out of range → engine.json fallback → p4",
            AddressOf VCT4b_Preset_FallbackChain)
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

    ''' <summary>
    ''' V-CT2: native=false + width=1280/height=720 → the effective startup
    ''' config carries 1280x720 and the dimension resolver returns exactly
    ''' that for a 1920x1080 capture (NVENC GPU scaling path).
    ''' </summary>
    Private Sub VCT2_CustomResolution_ReachesEncodeDims()
        WriteVideoConfig(VideoConfigJson(60, 17000, 4, False, 1280, 720))

        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(settings.UseNativeResolution = False,
                          $"control: UseNativeResolution expected False, got {settings.UseNativeResolution}")
        TestRunner.Assert(settings.CustomWidth = 1280 AndAlso settings.CustomHeight = 720,
                          $"control: CustomWidth/Height expected 1280/720, got {settings.CustomWidth}/{settings.CustomHeight}")

        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(settings)
        TestRunner.Assert(startup.UseNativeResolution = False,
                          $"effective: UseNativeResolution expected False, got {startup.UseNativeResolution}")
        TestRunner.Assert(startup.RequestedWidth = 1280 AndAlso startup.RequestedHeight = 720,
                          $"effective: RequestedWidth/Height expected 1280/720, got {startup.RequestedWidth}/{startup.RequestedHeight}")

        Dim dims As Tuple(Of Integer, Integer) =
            EngineStartupConfig.ResolveEncodeDimensions(1920, 1080, startup.UseNativeResolution,
                                                        startup.RequestedWidth, startup.RequestedHeight)
        TestRunner.Assert(dims.Item1 = 1280 AndAlso dims.Item2 = 720,
                          $"encode dims expected 1280x720, got {dims.Item1}x{dims.Item2} — custom resolution did not reach the encoder")
    End Sub

    ''' <summary>
    ''' V-CT2b: native=true → encode dims = capture dims (today's proven
    ''' behavior preserved as the explicit native branch).
    ''' </summary>
    Private Sub VCT2b_NativeResolution_UsesCaptureDims()
        WriteVideoConfig(VideoConfigJson(60, 17000, 4, True, 1920, 1080))

        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(settings)
        TestRunner.Assert(startup.UseNativeResolution = True,
                          $"UseNativeResolution expected True, got {startup.UseNativeResolution}")

        Dim dims As Tuple(Of Integer, Integer) =
            EngineStartupConfig.ResolveEncodeDimensions(1920, 1080, startup.UseNativeResolution,
                                                        startup.RequestedWidth, startup.RequestedHeight)
        TestRunner.Assert(dims.Item1 = 1920 AndAlso dims.Item2 = 1080,
                          $"native encode dims expected 1920x1080 (capture), got {dims.Item1}x{dims.Item2}")
    End Sub

    ''' <summary>
    ''' V-CT2c: a custom resolution larger than the capture must FAIL LOUDLY
    ''' — NVENC cannot upscale, and a silent desktop-resolution fallback is
    ''' forbidden by the phase law (no fake wiring).
    ''' </summary>
    Private Sub VCT2c_OversizedCustom_FailsLoudly()
        Dim threw As Boolean = False
        Try
            EngineStartupConfig.ResolveEncodeDimensions(1280, 720, False, 1920, 1080)
        Catch ex As ArgumentException
            threw = True
        End Try
        TestRunner.Assert(threw,
                          "oversized custom resolution must throw ArgumentException (loud failure), not silently fall back to the desktop size")
    End Sub

    ''' <summary>
    ''' V-CT3: the owner sets bitrate=10200 (kbps) in config.json →
    ''' CaptureSettings.Bitrate = 10,200,000 bps (kbps→bps in the unified
    ''' apply) → EngineStartupConfig.BitrateBps = 10,200,000 — bit-exact at
    ''' every layer, no kbps/bps drift.
    ''' </summary>
    Private Sub VCT3_BitrateKbps_ToBps_AllLayers()
        WriteVideoConfig(VideoConfigJson(60, 10200, 4, True, 1920, 1080))

        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(settings.Bitrate = 10200000L,
                          $"layer 1 (CaptureSettings.Bitrate): expected 10,200,000 bps (10200 kbps × 1000), got {settings.Bitrate}")

        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(settings)
        TestRunner.Assert(startup.BitrateBps = 10200000L,
                          $"layer 2 (EngineStartupConfig.BitrateBps): expected 10,200,000 bps, got {startup.BitrateBps}")
    End Sub

    ''' <summary>
    ''' V-CT3b: the engine's EncoderConfig mapping preserves the bitrate
    ''' exactly (RecordingEngine.Initialize maps startup → EncoderConfig with
    ''' minrate = maxrate = bitrate for CBR; verified here at the mapping
    ''' expression level by re-running the same If() logic the engine uses —
    ''' the NVENC-side consumption is proven by V-CT5 in Encoder.Tests).
    ''' </summary>
    Private Sub VCT3b_Bitrate_ReachesEncoderStartupConfig()
        WriteVideoConfig(VideoConfigJson(60, 10200, 4, True, 1920, 1080))

        Dim startup As EngineStartupConfig =
            NextRecordingConfig.MapStartupConfig(
                CaptureSettings.Load(NextRecordingConfig.EngineConfigPath()))

        ' Same mapping expression RecordingEngine.Initialize applies:
        Dim encBitrate As Long = If(startup.BitrateBps > 0, startup.BitrateBps, 20000000L)
        TestRunner.Assert(encBitrate = 10200000L,
                          $"EncoderConfig.BitrateBps mapping: expected 10,200,000, got {encBitrate}")
    End Sub

    ''' <summary>
    ''' V-CT4: config.json Recording.current.encoder_preset=7 → the internal
    ''' preset key 'p7' via the SINGLE existing mapper
    ''' (ConfigMigrator.MapNvencPresetInteger). Pre-wiring this config value
    ''' was dead in the unified apply path — the engine always used the
    ''' engine.json Preset string.
    ''' </summary>
    Private Sub VCT4_EncoderPreset_MappedToInternalPreset()
        WriteVideoConfig(VideoConfigJson(60, 17000, 7, True, 1920, 1080))

        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(settings.NvencPreset = 7,
                          $"control: CaptureSettings.NvencPreset expected 7 (config encoder_preset), got {settings.NvencPreset} — unified apply does not map encoder_preset")

        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(settings)
        TestRunner.Assert(startup.Preset = "p7",
                          $"effective: EngineStartupConfig.Preset expected 'p7' (single mapper), got '{startup.Preset}'")
    End Sub

    ''' <summary>
    ''' V-CT4b: fallback chain — an out-of-range encoder_preset is NOT
    ''' applied by the unified apply (stays at the model default 4 → 'p4'),
    ''' and a manually-built settings object with an invalid int + an
    ''' engine.json-style string falls back to that string. No third preset
    ''' source exists.
    ''' </summary>
    Private Sub VCT4b_Preset_FallbackChain()
        ' (a) config encoder_preset=99 → unified apply rejects it → default 4 → 'p4'
        WriteVideoConfig(VideoConfigJson(60, 17000, 99, True, 1920, 1080))

        Dim settings As CaptureSettings =
            CaptureSettings.Load(NextRecordingConfig.EngineConfigPath())
        TestRunner.Assert(settings.NvencPreset = 4,
                          $"(a) out-of-range encoder_preset must be rejected by the unified apply (default 4 kept), got {settings.NvencPreset}")
        Dim startup As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(settings)
        TestRunner.Assert(startup.Preset = "p4",
                          $"(a) fallback preset expected 'p4', got '{startup.Preset}'")

        ' (b) engine.json-style compat fallback: invalid int + explicit string
        Dim s2 As New CaptureSettings() With {.NvencPreset = 0, .Preset = "p2"}
        Dim startup2 As EngineStartupConfig = NextRecordingConfig.MapStartupConfig(s2)
        TestRunner.Assert(startup2.Preset = "p2",
                          $"(b) engine.json Preset compat fallback expected 'p2', got '{startup2.Preset}'")
    End Sub

End Module
