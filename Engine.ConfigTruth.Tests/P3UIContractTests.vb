Option Strict On
Option Explicit On
Option Infer On

' P3UIContractTests.vb — PHASE 3 UI CONFIG CONTRACT (deterministic, no
' hardware, no ffmpeg, no recording, no WinForms instantiation).
'
' OWNER scope (PHASE 3 implementation wave 1, branch Engine-Rebuild-Stabilization):
'   Engine WinForms = DIAGNOSTIC/OPERATOR only (no engine.json user writes)
'   Overlay         = user-facing config surface (config.json single store)
'   AudioSettingsForm = single legacy audio.json write (triple-write killed)
'
' The UI forms are WinForms (net10.0-windows) and cannot execute on Linux —
' so the contract they must satisfy is pinned the way the repo pins other
' non-runnable truths: LITERAL SOURCE CONTRACTS, executed against the same
' files the Engine/Overlay projects compile (FAIL-first, matrix §9).
' Each assertion cites the contract section it enforces. Contract references:
'   - CONFIG_RUNTIME_CONTRACT v1.0 (committed 5b2a8a0): §1 stores law,
'     §4 locked regime table, Q1-Q6 open OWNER decisions, §7 amendment protocol
'   - docs/UI_CONFIG_ARCHITECTURE.md v1.1: §9 REMOVE list, §10 violations,
'     §12 panel spec, §14 order, §15.6 volume decision
'
' P3-UICT1  Engine UI must NOT persist engine.json from UI controls
'           (second-writer divergence resolved; contract v1.0 §1, UI spec §9).
' P3-UICT2  Engine UI mirror controls are read-only + diagnostics panel exists
'           (UI spec §9 KEEP/REMOVE, §12 panel).
' P3-UICT3  AudioSettingsForm writes ONLY legacy audio.json — no engine.json,
'           no video.json (UI spec §14.1; contract v1.0 §1 store roles).
' P3-UICT4  Volume UI implements the model contract 0.0-1.0
'           (AppSettings.vb:86-94; UI spec §15.6 "one decision, documented" —
'           the 0-100% option; range debate remains open per contract v1.0).
' P3-UICT5  Mic state: canonical bool is the source; glyph never decides,
'           config never written from glyph (UI spec §10 violation #3).
' P3-UICT6  Paths.GalleryPath has exactly ONE UI writer (UI spec §14.3).
' P3-UICT7  Dead GET_FFMPEG_ARGS command is gone (UI spec §12/§14.4 —
'           no engine handler existed: [Engine] Client.vb Case Else).
' P3-UICT8  Behavioral sanity: the canonical seam the UI relies on
'           (unified apply → CaptureSettings → SessionConfig) still maps
'           regime-A audio + api_capture + preset after the wave
'           (contract v1.0 §2 registry + §4 regime table).

Imports System
Imports System.IO
Imports System.Text
Imports CaptureEngine.Recording
Imports Engine.ConfigTruth.Tests

Friend Module P3UIContractTests

    ' ── Repo source locator ──
    ' Tests run from bin/...; walk up to the repo root (has .git + docs +
    ' Engine.ConfigTruth.Tests). Fails loudly if the source tree is absent —
    ' a missing source tree must FAIL, not silently skip (FAIL-first).

    Private Function RepoRoot() As String
        Dim dir As New DirectoryInfo(AppContext.BaseDirectory)
        For depth As Integer = 0 To 12
            Dim probe As String = Path.Combine(dir.FullName, "Engine.ConfigTruth.Tests")
            If Directory.Exists(probe) AndAlso
               Directory.Exists(Path.Combine(dir.FullName, "docs")) AndAlso
               Directory.Exists(Path.Combine(dir.FullName, ".git")) Then
                Return dir.FullName
            End If
            If dir.Parent Is Nothing Then Exit For
            dir = dir.Parent
        Next
        Throw New Exception("repo root not found above " & AppContext.BaseDirectory &
                            " — P3-UICT source contracts cannot run without the source tree")
    End Function

    Private Function Source(relPath As String) As String
        Dim p As String = Path.Combine(RepoRoot(), relPath.Replace("/"c, Path.DirectorySeparatorChar))
        If Not File.Exists(p) Then
            Throw New Exception("source file missing: " & p)
        End If
        Return File.ReadAllText(p)
    End Function

    Private Sub ExpectContains(haystack As String, needle As String, what As String)
        Assert(haystack.Contains(needle), what & " — expected to contain: " & needle)
    End Sub

    Private Sub ExpectNotContains(haystack As String, needle As String, what As String)
        Assert(Not haystack.Contains(needle), what & " — must NOT contain: " & needle)
    End Sub

    Public Sub RunAll()
        TestRunner.RunTest(
            "P3-UICT1 Engine UI no longer persists engine.json from UI controls",
            AddressOf P3UICT1_EngineUI_NoEngineJsonUserWrites)
        TestRunner.RunTest(
            "P3-UICT2 Engine UI mirrors read-only + Effective Runtime panel present",
            AddressOf P3UICT2_EngineUI_ReadOnlyMirrors_AndDiagPanel)
        TestRunner.RunTest(
            "P3-UICT3 AudioSettingsForm writes ONLY legacy audio.json (no engine.json/video.json)",
            AddressOf P3UICT3_AudioForm_SingleWrite)
        TestRunner.RunTest(
            "P3-UICT4 Volume UI capped at 100% (model contract 0.0-1.0, UI spec §15.6)",
            AddressOf P3UICT4_VolumeCap_100)
        TestRunner.RunTest(
            "P3-UICT5 Mic toggle reads the canonical bool, never the icon glyph",
            AddressOf P3UICT5_MicState_BoolFirst)
        TestRunner.RunTest(
            "P3-UICT6 Paths.GalleryPath has exactly ONE UI writer",
            AddressOf P3UICT6_GalleryPath_SingleWriter)
        TestRunner.RunTest(
            "P3-UICT7 dead GET_FFMPEG_ARGS command removed from Overlay video page",
            AddressOf P3UICT7_DeadCommand_Removed)
        TestRunner.RunTest(
            "P3-UICT8 canonical seam intact: unified apply → CaptureSettings → SessionConfig",
            AddressOf P3UICT8_CanonicalSeam_Intact)
    End Sub

    ' ── source file helpers (literal repo paths) ──

    Private Const EngineUiVb As String = "Engine/Engine/[UI]/UI_Engine.vb"
    Private Const EngineUiDesignerVb As String = "Engine/Engine/[UI]/UI_Engine.Designer.vb"
    Private Const EngineAudioFormVb As String = "Engine/Engine/[UI]/AudioSettingsForm.vb"
    Private Const EngineAudioDesignerVb As String = "Engine/Engine/[UI]/AudioSettingsForm.Designer.vb"

    Private Const VideoPageVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[Settings]/[5] Video Capture.vb"
    Private Const AudioPageVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[Settings]/[6] Audio Capture.vb"
    Private Const SubMouseVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[1] Sub_Mouse.vb"
    Private Const SubMiscVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[1] Sub_Misc.vb"
    Private Const GalleryMainVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[Gallery]/[1] Main.vb"

    ' ── P3-UICT1 ──
    Private Sub P3UICT1_EngineUI_NoEngineJsonUserWrites()
        Dim src As String = Source(EngineUiVb)

        ' The second-writer Sub is GONE (was SaveSettings:652-667 — copied
        ' control values into _settings and called _settings.Save).
        ExpectNotContains(src, "Sub SaveSettings", "UI_Engine.vb")
        ExpectNotContains(src, "_settings.Save(", "UI_Engine.vb")

        ' The removal is deliberate and documented, not accidental.
        ExpectContains(src, "SaveSettings() REMOVED", "UI_Engine.vb removal note")

        ' The write-handlers that fed the controls are gone too.
        ExpectNotContains(src, "Private Sub OnEncoderChanged", "UI_Engine.vb")
        ExpectNotContains(src, "Private Sub OnCaptureMethodChanged", "UI_Engine.vb")
        ExpectNotContains(src, "Private Sub OnBrowseOutput", "UI_Engine.vb")
        ExpectNotContains(src, "Private Sub OnBrowseFFmpeg", "UI_Engine.vb")

        ' Declared engine.json writers REMAIN (compat writers per contract v1.0 §1):
        ' SyncWithOverlayConfig legacy-branch fallback + PREWARM handler.
        ExpectContains(src, "s.Save(_configPath)", "UI_Engine.vb legacy-branch compat writer")
        ExpectContains(src, "HandleEnginePrewarmFFmpeg", "UI_Engine.vb PREWARM handler")
    End Sub

    ' ── P3-UICT2 ──
    Private Sub P3UICT2_EngineUI_ReadOnlyMirrors_AndDiagPanel()
        Dim designer As String = Source(EngineUiDesignerVb)

        ' Read-only mirrors (UI spec §9 REMOVE list → read-only conversion).
        ExpectContains(designer, "nudFPS.Enabled = False", "UI_Engine.Designer.vb")
        ExpectContains(designer, "nudBitrate.Enabled = False", "UI_Engine.Designer.vb")
        ExpectContains(designer, "chkNativeRes.Enabled = False", "UI_Engine.Designer.vb")
        ExpectContains(designer, "cboEncoder.Enabled = False", "UI_Engine.Designer.vb")
        ExpectContains(designer, "cboCaptureMethod.Enabled = False", "UI_Engine.Designer.vb")
        ExpectContains(designer, "nudReplayDuration.Enabled = False", "UI_Engine.Designer.vb")
        ExpectContains(designer, "txtOutputDir.ReadOnly = True", "UI_Engine.Designer.vb")
        ExpectContains(designer, "txtFFmpegPath.ReadOnly = True", "UI_Engine.Designer.vb")

        ' Effective Runtime panel (UI spec §12) exists and is read-only.
        ExpectContains(designer, "pnlDiag", "UI_Engine.Designer.vb")
        ExpectContains(designer, "txtDiagnostics.ReadOnly = True", "UI_Engine.Designer.vb")

        Dim src As String = Source(EngineUiVb)
        ' Panel composes the truth layers (requested → effective → actual → output).
        ExpectContains(src, "BuildDiagnosticsText", "UI_Engine.vb")
        ExpectContains(src, "== CAPTURE API (contract v1.0 §4", "UI_Engine.vb")
        ExpectContains(src, "BLOCKER P1-PIXFMT", "UI_Engine.vb honesty line")
        ExpectContains(src, "aspirational until NVENC RC lands", "UI_Engine.vb bitrate honesty")
    End Sub

    ' ── P3-UICT3 ──
    Private Sub P3UICT3_AudioForm_SingleWrite()
        Dim src As String = Source(EngineAudioFormVb)

        ' The single remaining write: legacy audio.json fallback tier.
        ExpectContains(src, "_settings.SaveAudio(", "AudioSettingsForm.vb")

        ' engine.json write REMOVED (was _settings.Save(engineJsonPath)).
        ExpectNotContains(src, "_settings.Save(", "AudioSettingsForm.vb")

        ' video.json shadow write REMOVED (was SaveToOverlayVideoJson + JObject).
        ExpectNotContains(src, "SaveToOverlayVideoJson", "AudioSettingsForm.vb")
        ExpectNotContains(src, "JObject", "AudioSettingsForm.vb")

        ' Honest presentation (contract v1.0 §1; UI spec §10.3.2 — no lying controls).
        ExpectContains(src, "Operator view", "AudioSettingsForm.vb")
    End Sub

    ' ── P3-UICT4 ──
    Private Sub P3UICT4_VolumeCap_100()
        Dim engineDesigner As String = Source(EngineAudioDesignerVb)
        Dim overlayDesigner As String = Source(AudioPageVb.Replace("[6] Audio Capture.vb", "[6] Audio Capture.Designer.vb"))

        ExpectContains(engineDesigner, "trkSystemVol.Maximum = 100", "Engine audio designer")
        ExpectContains(engineDesigner, "trkMicVol.Maximum = 100", "Engine audio designer")
        ExpectNotContains(engineDesigner, "trkSystemVol.Maximum = 150", "Engine audio designer")
        ExpectNotContains(engineDesigner, "trkMicVol.Maximum = 150", "Engine audio designer")

        ExpectContains(overlayDesigner, "trkSystemVol.Maximum = 100", "Overlay [6] designer")
        ExpectContains(overlayDesigner, "trkMicVol.Maximum = 100", "Overlay [6] designer")
        ExpectNotContains(overlayDesigner, "trkSystemVol.Maximum = 150", "Overlay [6] designer")
        ExpectNotContains(overlayDesigner, "trkMicVol.Maximum = 150", "Overlay [6] designer")

        ' Load clamps match the slider cap (no 150 anywhere in the clamps).
        ExpectNotContains(Source(AudioPageVb), "Math.Min(150,", "Overlay [6] Audio Capture.vb")
        ExpectNotContains(Source(EngineAudioFormVb), "Math.Min(150,", "AudioSettingsForm.vb")
    End Sub

    ' ── P3-UICT5 ──
    Private Sub P3UICT5_MicState_BoolFirst()
        Dim mouse As String = Source(SubMouseVb)

        ' The toggle decision reads the canonical bool (writes !bool, then
        ' re-derives the glyph via LoadMicState — display only).
        ExpectContains(mouse, "AppSettings.Instance.Audio.MicEnabled = Not AppSettings.Instance.Audio.MicEnabled",
                       "Sub_Mouse.vb Mic_Click bool-first")

        Dim misc As String = Source(SubMiscVb)

        ' The old glyph→config write-back is gone (UI spec §10 violation #3):
        ' config must never be ASSIGNED from the glyph-derived local.
        ExpectNotContains(misc, "AppSettings.Instance.Audio.MicEnabled = micEnabledNow", "Sub_Misc.vb")

        ' UpdateMicStatus now syncs the DISPLAY from the bool.
        ExpectContains(misc, "LoadMicState()", "Sub_Misc.vb display sync")
    End Sub

    ' ── P3-UICT6 ──
    Private Sub P3UICT6_GalleryPath_SingleWriter()
        ' The dead, never-wired duplicate writer in Sub_Misc is gone.
        ExpectNotContains(Source(SubMiscVb), "AppSettings.Instance.Paths.GalleryPath = ", "Sub_Misc.vb")

        ' The one live writer remains: Base_Gallery.save_sc_Click.
        ExpectContains(Source(GalleryMainVb), "AppSettings.Instance.Paths.GalleryPath = txtFilePath.Text",
                       "Gallery/[1] Main.vb single writer")
    End Sub

    ' ── P3-UICT7 ──
    Private Sub P3UICT7_DeadCommand_Removed()
        ' GET_FFMPEG_ARGS had no engine handler ([Engine] Client.vb:245-283
        ' — engine_* only) and the box always showed "engine not connected".
        ' The two reintroduction vectors are pinned shut: the Overlay send
        ' and any engine-side dispatch case. (The literal command name may
        ' still appear in PHASE 3 documentation comments explaining the
        ' removal — that is why the assertions target the CODE vectors.)
        Dim video As String = Source(VideoPageVb)
        ExpectNotContains(video, ".Send(""GET_FFMPEG_ARGS""", "[5] Video Capture.vb send")
        ExpectNotContains(video, "Case ""GET_FFMPEG_ARGS""", "[5] Video Capture.vb dispatch")
        ExpectContains(video, "Requested (config.json)", "[5] Video Capture.vb truthful preview")

        ' And no engine handler may ever appear for it (it must stay dead).
        ExpectNotContains(Source("Engine/Engine/[API]/[Engine] Client.vb"),
                          """GET_FFMPEG_ARGS""", "[Engine] Client.vb dispatch")
    End Sub

    ' ── P3-UICT8 (behavioral — the seam the UI contract stands on) ──
    Private Sub P3UICT8_CanonicalSeam_Intact()
        ' Unified config.json (regime-A audio + api_capture + preset) must
        ' still flow through the ONE mapper into CaptureSettings, and the
        ' per-record seam into SessionConfig — unchanged by the UI wave
        ' (contract v1.0 §2 registry, §4 regime table: audio = A).
        Dim cfgDir As String = Path.Combine(AppLayout.Dir, "Config")
        Directory.CreateDirectory(cfgDir)
        Dim inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        Dim sb As New StringBuilder()
        sb.AppendLine("{")
        sb.AppendLine("  ""Recording"": {")
        sb.AppendLine("    ""encoder"": ""NVENC_H264"",")
        sb.AppendLine("    ""active_preset"": ""Medium"",")
        sb.AppendLine("    ""replay_duration"": 60,")
        sb.AppendLine("    ""api_capture"": ""ddagrab"",")
        sb.AppendLine("    ""current"": { ""fps"": 75, ""bitrate"": 10200, ""encoder_preset"": 7," &
                      " ""use_native_resolution"": false, ""width"": 1280, ""height"": 720 }")
        sb.AppendLine("  },")
        sb.AppendLine("  ""Audio"": {")
        sb.AppendLine("    ""SystemAudioEnabled"": true,")
        sb.AppendLine("    ""MicEnabled"": true,")
        sb.AppendLine("    ""SystemAudioVolume"": 1.0,")
        sb.AppendLine("    ""MicVolume"": 0.5,")
        sb.AppendLine("    ""MicDeviceName"": ""TestMic"",")
        sb.AppendLine("    ""MicDeviceId"": ""test-id"",")
        sb.AppendLine("    ""TrackMode"": 1,")
        sb.AppendLine("    ""AudioClockMode"": ""Legacy""")
        sb.AppendLine("  },")
        sb.AppendLine("  ""Paths"": { ""GalleryPath"": """", ""SavePath"": """", ""FFmpegPath"": """" }")
        sb.AppendLine("}")
        File.WriteAllText(Path.Combine(cfgDir, "config.json"), sb.ToString())
        OverlayConfig.ResetResolvedPath()

        Dim eff As CaptureSettings = NextRecordingConfig.LoadEffectiveSettings()
        Assert(eff.CaptureMethod = "ddagrab", "api_capture → CaptureMethod (unified apply)")
        Assert(eff.NvencPreset = 7, "encoder_preset → NvencPreset (V-CT4 single mapper)")
        Assert(eff.FPS = 75, "current.fps → FPS (V-CT1)")
        Assert(eff.SystemAudioCapture, "SystemAudioEnabled → SystemAudioCapture (regime A)")
        Assert(eff.MicCapture, "MicEnabled → MicCapture (regime A)")
        Assert(Math.Abs(eff.MicVolume - 0.5F) < 0.001F, "MicVolume passthrough (regime A)")
        Assert(eff.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack, "TrackMode=1 → SeparateTrack")

        Dim session As SessionConfig =
            NextRecordingConfig.MapSessionConfig(eff, "C:\out\test.mp4", "ffmpeg", Nothing)
        Assert(session.TargetFps = 75, "SessionConfig.TargetFps = config fps (per-record, regime A)")
        Assert(session.MicEnabled, "SessionConfig.MicEnabled (regime A)")
        Assert(session.MicSeparateTracks, "SessionConfig.MicSeparateTracks (regime A)")
    End Sub

End Module
