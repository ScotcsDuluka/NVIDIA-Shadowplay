' ══════════════════════════════════════════════════════════════════════════════
' W2OverlayHonestyTests.vb — WAVE 2 UI HONESTY source contracts
' ══════════════════════════════════════════════════════════════════════════════
' Pins the Wave-2 "Overlay must not lie" fixes to the exact code vectors.
' Same pattern as P3UIContractTests: read the REAL repo source files and
' assert on them (deterministic, no hardware).
'
' W2-H1  Record toasts are Engine-confirmed (no optimistic saved/started).
' W2-H2  Replay panel honestly disabled + marked until Engine has a buffer.
' W2-H3  Replay engine responses reconcile Overlay state (start/stop/save).
' W2-H4  engine_ready resets stale state + pulls engine_get_status.
' W2-H5  Real recording progress reaches the record panel (Record_Stats).
' W2-H6  Dead commands stay dead: BUFFER_DURATION + hotkey broadcast.
' W2-H7  Mic toggle + settings import notify the Engine (engine_config_changed).
' W2-H8  Behavioral: LoadVideoConfig — config.json beats stale video.json.
'
' NOTE on needles: assertions target CODE vectors (e.g. `ShowNotifier("…")`
' call shapes, `.Send("…")` shapes) exactly like P3-UICT7, so explanatory
' comments inside the tested files cannot satisfy or trip them.
' ══════════════════════════════════════════════════════════════════════════════

Imports System
Imports System.IO
Imports Engine.ConfigTruth.Tests

Friend Module W2OverlayHonestyTests

    ' ── Repo source locator (same walk as P3UIContractTests) ──

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
                            " — W2-H source contracts cannot run without the source tree")
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

    ' ── source file paths (literal repo paths) ──

    Private Const OverlayClientVb As String =
        "Overlay/[Forms Overlay - Project Files]/[API]/[Services]/TCP/[Overlay] Client.vb"
    Private Const SubRecordVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[1] Sub_Record.vb"
    Private Const MainMenuVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[1] Main Menu.vb"
    Private Const SubMiscVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[1] Sub_Misc.vb"
    Private Const SubMouseVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[1] Sub_Mouse.vb"
    Private Const VideoPageVb As String =
        "Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[Settings]/[5] Video Capture.vb"
    Private Const ExportImportVb As String =
        "Overlay/[Forms Overlay - Project Files]/[API]/[Services]/SettingsExportImport.vb"
    Private Const OverlayConfigVb As String =
        "Engine/Engine/[Integration]/OverlayConfig.vb"
    Private Const EnUsJson As String =
        "Overlay/Languages/en-US.json"
    Private Const ThThJson As String =
        "Overlay/Languages/th-TH.json"

    Public Sub RunAll()
        TestRunner.RunTest(
            "W2-H1 record toasts fire on Engine confirmation only (no optimistic saved/started)",
            AddressOf W2H1_RecordToasts_EngineConfirmed)
        TestRunner.RunTest(
            "W2-H2 replay panel disabled + marked until Engine has a real buffer",
            AddressOf W2H2_ReplayPanel_HonestlyDisabled)
        TestRunner.RunTest(
            "W2-H3 replay responses reconcile Overlay state (start/stop/save cases)",
            AddressOf W2H3_ReplayResponses_Reconciled)
        TestRunner.RunTest(
            "W2-H4 engine_ready resets stale state and pulls engine_get_status",
            AddressOf W2H4_EngineReady_StatusPull)
        TestRunner.RunTest(
            "W2-H5 real recording progress reaches the record panel",
            AddressOf W2H5_Progress_ReachesPanel)
        TestRunner.RunTest(
            "W2-H6 dead commands stay dead (BUFFER_DURATION / hotkey broadcast)",
            AddressOf W2H6_DeadCommands_Gone)
        TestRunner.RunTest(
            "W2-H7 mic toggle + settings import notify the Engine",
            AddressOf W2H7_ConfigChange_Notifications)
        TestRunner.RunTest(
            "W2-H8 LoadVideoConfig: config.json beats stale video.json (behavioral)",
            AddressOf W2H8_ConfigJson_Wins)
    End Sub

    ' ── W2-H1 ──
    Private Sub W2H1_RecordToasts_EngineConfirmed()
        Dim rec As String = Source(SubRecordVb)
        Dim cli As String = Source(OverlayClientVb)

        ' Optimistic toasts are GONE from the toggle paths in Sub_Record.
        ExpectNotContains(rec, "ShowNotifier(""recording_saved"")", "Sub_Record.vb stop path")
        ExpectNotContains(rec, "ShowNotifier(""recording_started"")", "Sub_Record.vb start path")

        ' The Engine confirmation paths own the toasts now.
        ExpectContains(cli, "HandleEngineRecordingSaved(filePath As String)", "Client.vb saved handler")
        ExpectContains(cli, "ShowNotifier(""recording_saved"")", "Client.vb saved handler toast")
        ExpectContains(cli, "ShowNotifier(""recording_started"")", "Client.vb started toast on ok")

        ' Started toast lives in the record_start ok branch, error keeps failing loud.
        ExpectContains(cli, "ShowNotifier(""recording_error"")", "Client.vb failure toast")
        ExpectContains(cli, "Case ""engine_record_stop""", "Client.vb stop case")

        ' Record state stays optimistic-visible but Engine-reconciled:
        ' Sub_Record still flips local state for immediate UI feedback,
        ' and Client.vb still reverts it on failure (P2.6 contract intact).
        ExpectContains(cli, "_isRecordingLocal = False", "Client.vb revert on failure")
    End Sub

    ' ── W2-H2 ──
    Private Sub W2H2_ReplayPanel_HonestlyDisabled()
        Dim rec As String = Source(SubRecordVb)
        Dim cli As String = Source(OverlayClientVb)
        Dim misc As String = Source(SubMiscVb)
        Dim menu As String = Source(MainMenuVb)

        ' The disable routine exists and hits every replay control.
        ExpectContains(rec, "Sub InitReplayHonesty()", "Sub_Record.vb honesty init")
        For Each ctrl As String In New String() {"Menu_Replay_key", "Menu_Replay_Box1",
                                                 "Menu_Replay_text", "Menu_Replay_Box2",
                                                 "Menu_Replay_save_text", "Menu_Replay_save_key"}
            ExpectContains(rec, ctrl & ".Enabled = False", "Sub_Record.vb disables " & ctrl)
        Next

        ' It is called at startup (Base_Load), not dead code.
        ExpectContains(cli, "InitReplayHonesty()", "Client.vb Base_Load calls the init")

        ' Optimistic replay turn-on is gone from Sub_Record (no fake state,
        ' no fake toast — the UI lights up only via engine_response ok).
        ExpectNotContains(rec, "ShowNotifier(""instant_replay_on"")", "Sub_Record.vb start path")
        ExpectNotContains(rec, "ShowNotifier(""saved_last_15"")", "Sub_Record.vb save path")
        ExpectNotContains(rec, "_isBufferingLocal = True", "Sub_Record.vb optimistic state")

        ' The panel LABELS say "not available yet" instead of "Turn on":
        ' static label (Main Menu l10n apply) + ticker label (Sub_Misc
        ' UpdateReplayStatus inactive action key).
        ExpectContains(menu, "Menu_Replay_text.Text = L(""l10n.replayNotImplemented"")", "Main Menu.vb label")
        ExpectContains(misc, """l10n.replayNotImplemented"")", "Sub_Misc.vb inactive label")
        ExpectNotContains(misc, """l10n.instantReplayStart"")", "Sub_Misc.vb old label gone")

        ' The l10n key exists in the language stores (en-US + th-TH sampled;
        ' the wave added it to every Overlay/Languages/*.json).
        ExpectContains(Source(EnUsJson), """l10n.replayNotImplemented""", "en-US.json key")
        ExpectContains(Source(ThThJson), """l10n.replayNotImplemented""", "th-TH.json key")
    End Sub

    ' ── W2-H3 ──
    Private Sub W2H3_ReplayResponses_Reconciled()
        Dim cli As String = Source(OverlayClientVb)

        ' All three replay commands have response cases now (they were
        ' silently ignored — P2.6 handler had no replay branches).
        ExpectContains(cli, "Case ""engine_replay_start""", "Client.vb replay_start case")
        ExpectContains(cli, "Case ""engine_replay_stop""", "Client.vb replay_stop case")
        ExpectContains(cli, "Case ""engine_replay_save""", "Client.vb replay_save case")

        ' The ok path owns the state flip + toast + button re-enable;
        ' the error path owns the honest failure toast.
        ExpectContains(cli, "_isBufferingLocal = True", "Client.vb ok-confirmed buffer state")
        ExpectContains(cli, "ShowNotifier(""instant_replay_on"")", "Client.vb on-toast on ok")
        ExpectContains(cli, "ShowNotifier(""instant_replay_off"")", "Client.vb off-toast on ok")
        ExpectContains(cli, "ShowNotifier(""saved_last_15"")", "Client.vb saved-toast on ok")
        ExpectContains(cli, "ShowNotifier(""replay_error"")", "Client.vb honest replay failure")
    End Sub

    ' ── W2-H4 ──
    Private Sub W2H4_EngineReady_StatusPull()
        Dim cli As String = Source(OverlayClientVb)

        ' engine_ready clears the stale "Recording" state...
        Dim readyIdx As Integer = cli.IndexOf("Case ""engine_ready""", StringComparison.Ordinal)
        Assert(readyIdx >= 0, "Client.vb engine_ready case exists")
        Dim respIdx As Integer = cli.IndexOf("Case ""engine_response""", StringComparison.Ordinal)
        Assert(respIdx > readyIdx, "engine_response case follows engine_ready")
        Dim readyBody As String = cli.Substring(readyIdx, respIdx - readyIdx)
        ExpectContains(readyBody, "_isRecordingLocal = False", "engine_ready stale-state reset")
        ExpectContains(readyBody, "_isRecordingLocal = False", "engine_ready resets before pull")

        ' ...and PULLS the authoritative state (the previously orphaned
        ' engine_get_status response handler finally gets traffic).
        ExpectContains(readyBody, "tcp.Send(""engine_get_status"")", "engine_ready status pull")
        ExpectContains(cli, "Case ""engine_get_status""", "Client.vb status response handler")
        ExpectContains(cli, "RecordValue = True", "status response reconciles to Recording")
    End Sub

    ' ── W2-H5 ──
    Private Sub W2H5_Progress_ReachesPanel()
        Dim cli As String = Source(OverlayClientVb)

        ' The Debug-only TODO is gone; real timer + size reach Record_Stats.
        ExpectNotContains(cli, "TODO: UI designers can wire this", "Client.vb progress TODO removed")
        ExpectContains(cli, "Record_Stats.Text =", "Client.vb progress writes the panel")
        ExpectContains(cli, "TimeSpan.FromSeconds(sec)", "Client.vb progress formats timer")
    End Sub

    ' ── W2-H6 ──
    Private Sub W2H6_DeadCommands_Gone()
        ' Both sends had ZERO engine handlers ([Engine] Client.vb dispatches
        ' engine_*/legacy record commands only). Pin the send vectors shut
        ' (P3-UICT7 style: code shapes, not comments).
        Dim video As String = Source(VideoPageVb)
        ExpectNotContains(video, ".Send(""BUFFER_DURATION""", "[5] Video Capture.vb send")
        ExpectNotContains(video, "Case ""BUFFER_DURATION""", "[5] Video Capture.vb dispatch")

        Dim menu As String = Source(MainMenuVb)
        ExpectNotContains(menu, "tcp.Send(""Hotkeys registered!"")", "Main Menu.vb hotkey broadcast")

        ' And no engine handler may ever appear for BUFFER_DURATION.
        Dim engineClient As String = Source("Engine/Engine/[API]/[Engine] Client.vb")
        ExpectNotContains(engineClient, "BUFFER_DURATION", "[Engine] Client.vb dispatch")
    End Sub

    ' ── W2-H7 ──
    Private Sub W2H7_ConfigChange_Notifications()
        Dim imp As String = Source(ExportImportVb)
        Dim mouse As String = Source(SubMouseVb)

        ' Import broadcasts with an EMPTY scope (reload all + reinit).
        ExpectContains(imp, "Base.tcp.Send(""engine_config_changed"", """")", "SettingsExportImport.vb import broadcast")

        ' Mic toggle broadcasts with scope "audio" (reload + mirror refresh,
        ' no NVENC rebuild). Sub_Mouse.vb is a PUA-glyph file — the wave
        ' edited it via byte-exact splice; the pin guards the Send vector.
        ExpectContains(mouse, "tcp.Send(""engine_config_changed"", ""audio"")", "Sub_Mouse.vb Mic_Click broadcast")

        ' Engine side keeps accepting both scopes.
        Dim engineUi As String = Source("Engine/Engine/[UI]/UI_Engine.vb")
        ExpectContains(engineUi, "Sub HandleEngineConfigChanged", "UI_Engine.vb handler")
    End Sub

    ' ── W2-H8 (behavioral — runs the REAL LoadVideoConfig) ──
    Private Sub W2H8_ConfigJson_Wins()
        Dim cfgDir As String = Path.Combine(AppLayout.Dir, "Config")
        Directory.CreateDirectory(cfgDir)
        Dim cfgJson As String = Path.Combine(cfgDir, "config.json")
        Dim vidJson As String = Path.Combine(cfgDir, "video.json")

        ' Stale legacy video.json says fps 60 …
        File.WriteAllText(vidJson, "{""current"": {""fps"": 60, ""bitrate"": 17000}}")
        ' … while the authoritative config.json says fps 75.
        File.WriteAllText(cfgJson,
            "{""Recording"": {""current"": {""fps"": 75, ""bitrate"": 10200}}, " &
            """Audio"": {""MicEnabled"": true}}")

        OverlayConfig.ResetResolvedPath()
        Dim vc As OverlayConfig.VideoConfig = OverlayConfig.LoadVideoConfig()
        Assert(vc IsNot Nothing, "LoadVideoConfig returned Nothing")
        Assert(vc.current.fps = 75,
               "config.json must win over stale video.json (expected fps 75, got " &
               vc.current.fps.ToString(Globalization.CultureInfo.InvariantCulture) & ")")

        ' Legacy fallback still exists for OLD installs: config.json without
        ' a usable Recording section → video.json is honored again.
        File.WriteAllText(cfgJson, "{""Audio"": {""MicEnabled"": true}}")
        OverlayConfig.ResetResolvedPath()
        Dim vc2 As OverlayConfig.VideoConfig = OverlayConfig.LoadVideoConfig()
        Assert(vc2 IsNot Nothing AndAlso vc2.current.fps = 60,
               "legacy video.json fallback must survive when config.json has no Recording section")

        ' Cleanup so other suites start from a clean config dir.
        File.Delete(cfgJson)
        File.Delete(vidJson)
        OverlayConfig.ResetResolvedPath()
    End Sub
End Module
