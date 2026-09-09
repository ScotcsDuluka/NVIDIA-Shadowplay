' ══════════════════════════════════════════════════════════════════════════════
' L1ReconnectTests.vb — LEVEL 1 UI/HOST RECOVERY source contracts
' ══════════════════════════════════════════════════════════════════════════════
' Pins the Level-1 reconnect contract to the exact code vectors (P3-UICT7 /
' W2-H style: read the REAL repo source files, assert on them — deterministic,
' no hardware, no recording).
'
' L1-1  Restarted UI pulls engine state at startup (bounded, no fake state).
' L1-2  UI-side TCP reconnect re-pulls state (engine never re-announces for it).
' L1-3  engine_get_status carries engine truth: state|elapsed|output.
' L1-4  New-engine sessions broadcast real 1s progress (time + file size).
' L1-5  Session truth retires on every ending kind (status goes state-only).
' L1-6  UI shutdown NEVER stops the engine (UI cleanup ≠ engine lifecycle).
' L1-7  Supervisor reuses an existing engine — never spawns a duplicate.
'
' NOTE on needles: assertions target CODE vectors exactly like W2-H, so
' comments inside the tested files cannot satisfy or trip them.
' ══════════════════════════════════════════════════════════════════════════════

Imports System
Imports System.IO

Imports Engine.ConfigTruth.Tests

Friend Module L1ReconnectTests

    ' ── source file paths (literal repo paths) ──

    Private Const OverlayClientVb As String =
        "Overlay/[Forms Overlay - Project Files]/[API]/[Services]/TCP/[Overlay] Client.vb"
    Private Const OverlayTcpHelperVb As String =
        "Overlay/[Forms Overlay - Project Files]/[API]/[Services]/TCP/TcpClientHelper.vb"
    Private Const SupervisorVb As String =
        "Overlay/[Forms Overlay - Project Files]/[API]/[Services]/EngineProcessSupervisor.vb"
    Private Const EngineHostVb As String =
        "Engine/Engine/[API]/RecordingEngineHost.vb"
    Private Const EngineUiVb As String =
        "Engine/Engine/[UI]/UI_Engine.vb"

    Public Sub RunAll()
        TestRunner.RunTest(
            "L1-1 restarted UI pulls engine_get_status at startup (bounded retry, honest fallback)",
            AddressOf L1_1_StartupBoundedStatusPull)
        TestRunner.RunTest(
            "L1-2 UI-side TCP reconnect re-pulls engine state",
            AddressOf L1_2_ReconnectStatusPull)
        TestRunner.RunTest(
            "L1-3 engine_get_status answers engine truth: state|elapsed|output",
            AddressOf L1_3_StatusDataContract)
        TestRunner.RunTest(
            "L1-4 new-engine sessions broadcast real 1s progress (elapsed + file size)",
            AddressOf L1_4_NewEngineProgressBroadcast)
        TestRunner.RunTest(
            "L1-5 session truth retires on every ending kind",
            AddressOf L1_5_SessionTruthRetired)
        TestRunner.RunTest(
            "L1-6 UI shutdown never stops the engine",
            AddressOf L1_6_UIShutdown_NeverStopsEngine)
        TestRunner.RunTest(
            "L1-7 supervisor reuses an existing engine — never spawns a duplicate",
            AddressOf L1_7_Supervisor_ReuseOnly)
    End Sub

    ' ── L1-1 ──
    Private Sub L1_1_StartupBoundedStatusPull()
        Dim cli As String = Source(OverlayClientVb)

        ' The bounded pull exists and is wired at Base_Load, AFTER the
        ' supervisor (spawn-or-reuse) — state query never launches anything.
        Assert(cli.IndexOf("Private Sub StartBoundedStatusPull()", StringComparison.Ordinal) >= 0,
               "StartBoundedStatusPull exists")
        Dim ensureIdx As Integer = cli.IndexOf("EngineProcessSupervisor.EnsureEngineRunning()", StringComparison.Ordinal)
        Dim pullCallIdx As Integer = cli.IndexOf("StartBoundedStatusPull()", StringComparison.Ordinal)
        Assert(ensureIdx >= 0 AndAlso pullCallIdx > ensureIdx,
               "Base_Load pulls status AFTER EnsureEngineRunning (query must never launch)")

        ' Bounded: interval + attempt cap are constants, not an infinite loop.
        ExpectContains(cli, "Private Const StatusPullIntervalMs As Integer = 2000", "status pull interval")
        ExpectContains(cli, "Private Const StatusPullMaxAttempts As Integer = 10", "status pull attempt cap")
        ExpectContains(cli, "_engineStatusPulled As Boolean = False", "status pulled latch")

        ' The pull sends the query and honesty on exhaustion is logged.
        Dim pullBody As String = ExtractMethod(cli, "Private Sub StartBoundedStatusPull()")
        ExpectContains(pullBody, "tcp.Send(""engine_get_status"")", "startup status pull sends engine_get_status")
        ExpectContains(pullBody, "attemptsLeft <= 0", "bounded attempts guard")
        ExpectContains(pullBody, "engine state unknown", "honest exhausted message")

        ' An answer retires the pull (engine responsive = liveness proven).
        Dim respBody As String = ExtractCase(cli, "Case ""engine_get_status""", "Case ""engine_replay_start""")
        ExpectContains(respBody, "_engineStatusPulled = True", "answer retires the pull")
    End Sub

    ' ── L1-2 ──
    Private Sub L1_2_ReconnectStatusPull()
        Dim helper As String = Source(OverlayTcpHelperVb)
        Dim cli As String = Source(OverlayClientVb)

        ' Overlay's TcpClientHelper raises OnReconnected exactly like the
        ' Engine-side v9 fix (initial connect does NOT raise it).
        ExpectContains(helper, "Public Event OnReconnected()", "Overlay helper OnReconnected event")
        ExpectContains(helper, "RaiseEvent OnReconnected()", "Overlay helper raises OnReconnected after reconnect")

        ' The UI re-pulls state on its own socket reconnect.
        ExpectContains(cli, "AddHandler tcp.OnReconnected, AddressOf OnTcpReconnected", "UI wires OnReconnected")
        Dim reconnectBody As String = ExtractMethod(cli, "Private Sub OnTcpReconnected()")
        ExpectContains(reconnectBody, "_engineStatusPulled = False", "reconnect resets the pulled latch")
        ExpectContains(reconnectBody, "tcp.Send(""engine_get_status"")", "reconnect re-pulls engine state")
    End Sub

    ' ── L1-3 ──
    Private Sub L1_3_StatusDataContract()
        Dim host As String = Source(EngineHostVb)
        Dim engineUi As String = Source(EngineUiVb)

        ' New engine: Recording + a live host clock → state|elapsed|output.
        Dim hostStatus As String = ExtractMethod(host, "Private Sub HandleRecordingGetStatus")
        ExpectContains(hostStatus, "RecordingEngineState.Recording AndAlso", "new engine gates on real Recording state")
        ExpectContains(hostStatus, "_newEngineSessionClock.IsRunning", "new engine gates on live session clock")
        ExpectContains(hostStatus, "{stateName}|{elapsedSec}|{_newEngineSessionOutputPath}", "new engine data contract state|elapsed|output")

        ' Legacy engine: same contract from its own recording clock/output.
        Dim legacyStatus As String = ExtractMethod(engineUi, "Private Sub HandleEngineGetStatus")
        ExpectContains(legacyStatus, "_captureEngine.RecordingDuration.TotalSeconds", "legacy elapsed from engine clock")
        ExpectContains(legacyStatus, "_captureEngine.OutputFile", "legacy output from engine")
        ExpectContains(legacyStatus, "Recording|{elapsedSec}|{outputFile}", "legacy data contract state|elapsed|output")

        ' The clock starts only after every start guard passed (host truth).
        Dim startBody As String = ExtractMethod(host, "Private Async Function HandleRecordingStart")
        ExpectContains(startBody, "_newEngineSessionOutputPath = If(value, """")", "host records accepted output path")
        ExpectContains(startBody, "_newEngineSessionClock.Restart()", "host clock restarts per session")
    End Sub

    ' ── L1-4 ──
    Private Sub L1_4_NewEngineProgressBroadcast()
        Dim host As String = Source(EngineHostVb)

        ' Persistent 1s host broadcaster for new-engine sessions.
        ExpectContains(host, "Private Sub OnNewEngineProgressTick", "host progress tick exists")
        Dim tickBody As String = ExtractMethod(host, "Private Sub OnNewEngineProgressTick")
        ExpectContains(tickBody, "taskRef.IsCompleted", "tick no-ops without an active session")
        ExpectContains(tickBody, "_newEngineSessionClock.Elapsed.TotalSeconds", "tick elapsed from host session clock")
        ExpectContains(tickBody, "fi.Length", "tick size measured from real file")
        ExpectContains(tickBody, "tcp.Send(""engine_recording_progress""", "tick broadcasts progress")

        ' The UI seeds the panel from a status answer (time restore) and the
        ' periodic progress broadcast keeps it ticking (existing W2-H5 path).
        Dim cli As String = Source(OverlayClientVb)
        Dim respBody As String = ExtractCase(cli, "Case ""engine_get_status""", "Case ""engine_replay_start""")
        ExpectContains(respBody, "TimeSpan.FromSeconds(restoredSec)", "status answer restores elapsed into the panel")
        ExpectContains(respBody, "rehydrated active output", "status answer logs the active output")
    End Sub

    ' ── L1-5 ──
    Private Sub L1_5_SessionTruthRetired()
        Dim host As String = Source(EngineHostVb)

        ' The session-end watcher (every ending kind: manual stop, expiry,
        ' fault) retires the host clock + output path.
        Dim watcherBody As String = ExtractMethod(host, "Private Sub WatchSessionEnd")
        ExpectContains(watcherBody, "_newEngineSessionClock.Reset()", "watcher retires the session clock")
        ExpectContains(watcherBody, "_newEngineSessionOutputPath = """"", "watcher retires the output path")

        ' Dispose retires both too (host runtime gone → no session truth).
        Dim disposeBody As String = ExtractMethod(host, "Private Sub DisposeRecordingEngine")
        ExpectContains(disposeBody, "_newEngineProgressTimer.Stop()", "dispose stops the progress broadcaster")
        ExpectContains(disposeBody, "_newEngineSessionClock.Reset()", "dispose retires the session clock")
    End Sub

    ' ── L1-6 ──
    Private Sub L1_6_UIShutdown_NeverStopsEngine()
        Dim cli As String = Source(OverlayClientVb)

        ' UI FormClosing = UI cleanup ONLY: no stop command, no process kill,
        ' no window-message kill — recording must survive the UI dying.
        Dim closingBody As String = ExtractMethod(cli, "Private Sub Base_TestFormClosing")
        ExpectNotContains(closingBody, "RECORD_STOP", "UI close must not send RECORD_STOP")
        ExpectNotContains(closingBody, "engine_record_stop", "UI close must not send engine_record_stop")
        ExpectNotContains(closingBody, ".Kill", "UI close must not kill any process")
        ExpectNotContains(closingBody, "CloseMainWindow", "UI close must not close the engine window")
        ExpectContains(closingBody, "EngineProcessSupervisor.Shutdown()", "UI close only stops the UI-side monitor")

        ' And the supervisor Shutdown contract: a flag, not a kill.
        Dim sup As String = Source(SupervisorVb)
        Dim shutdownBody As String = ExtractMethod(sup, "Public Shared Sub Shutdown()")
        ExpectNotContains(shutdownBody, "Kill", "supervisor Shutdown must not kill")
        ExpectContains(shutdownBody, "_shuttingDown = True", "supervisor Shutdown only flags the monitor loop")

        ' The whole UI TCP surface never sends a record stop — the ONLY
        ' RECORD_STOP send lives in the user-action path (Sub_Record toggle).
        ExpectNotContains(cli, "RECORD_STOP", "Overlay TCP client never sends RECORD_STOP")
    End Sub

    ' ── L1-7 ──
    Private Sub L1_7_Supervisor_ReuseOnly()
        Dim sup As String = Source(SupervisorVb)

        ' Spawn path reuses an existing engine process (duplicate invariant).
        Dim spawnBody As String = ExtractMethod(sup, "Private Shared Sub SpawnIfNotRunning")
        ExpectContains(spawnBody, "FindEngineProcess()", "spawn checks for an existing engine")
        ExpectContains(spawnBody, "engine already running (pid", "spawn logs reuse of the existing engine")
        ExpectNotContains(spawnBody, ".Kill", "spawn path never kills")

        ' The supervisor never carries a kill/stop path at all.
        ExpectNotContains(sup, ".Kill(", "supervisor has no kill path")
    End Sub

    ' ── helpers (same walk as W2) ──

    Private Function Source(relPath As String) As String
        Dim p As String = Path.Combine(RepoRoot(), relPath.Replace("/"c, Path.DirectorySeparatorChar))
        If Not File.Exists(p) Then
            Throw New Exception("source file missing: " & p)
        End If
        Return File.ReadAllText(p)
    End Function

    Private Function RepoRoot() As String
        Dim dir As New DirectoryInfo(AppContext.BaseDirectory)
        For depth As Integer = 0 To 12
            Dim probe As String = Path.Combine(dir.FullName, "Tester", "test", "Engine", "ConfigTruth")
            If Directory.Exists(probe) AndAlso
               Directory.Exists(Path.Combine(dir.FullName, "docs")) AndAlso
               Directory.Exists(Path.Combine(dir.FullName, ".git")) Then
                Return dir.FullName
            End If
            If dir.Parent Is Nothing Then Exit For
            dir = dir.Parent
        Next
        Throw New Exception("repo root not found above " & AppContext.BaseDirectory &
                            " — L1 source contracts cannot run without the source tree")
    End Function

    ''' <summary>Extract a Sub/Function body from its declaration to the next
    ''' top-level member declaration (heuristic: "    End Sub/Function" at the
    ''' same nesting is handled by scanning for the next "    Private/Public/
    ''' Friend" member line). Good enough for source pins.</summary>
    Private Function ExtractMethod(source As String, declaration As String) As String
        Dim startIdx As Integer = source.IndexOf(declaration, StringComparison.Ordinal)
        Assert(startIdx >= 0, "declaration not found: " & declaration)
        Dim nextMember As Integer = source.Length
        For Each prefix As String In New String() {"    Private ", "    Public ", "    Friend ", "    Protected "}
            Dim scanFrom As Integer = startIdx + declaration.Length
            Dim idx As Integer = source.IndexOf(prefix, scanFrom, StringComparison.Ordinal)
            If idx >= 0 AndAlso idx < nextMember Then nextMember = idx
        Next
        ' End-of-file or next member — either way the full body is inside.
        If nextMember <= startIdx Then nextMember = source.Length
        Return source.Substring(startIdx, nextMember - startIdx)
    End Function

    ''' <summary>Extract a Select Case branch body: from the case label to the
    ''' next case label (or End Select).</summary>
    Private Function ExtractCase(source As String, caseLabel As String, nextCaseLabel As String) As String
        Dim startIdx As Integer = source.IndexOf(caseLabel, StringComparison.Ordinal)
        Assert(startIdx >= 0, "case not found: " & caseLabel)
        Dim endIdx As Integer = source.IndexOf(nextCaseLabel, startIdx + caseLabel.Length, StringComparison.Ordinal)
        If endIdx < 0 Then
            endIdx = source.IndexOf("End Select", startIdx, StringComparison.Ordinal)
        End If
        Assert(endIdx > startIdx, "case body end not found after: " & caseLabel)
        Return source.Substring(startIdx, endIdx - startIdx)
    End Function

    Private Sub ExpectContains(haystack As String, needle As String, what As String)
        Assert(haystack.Contains(needle), what & " — expected to contain: " & needle)
    End Sub

    Private Sub ExpectNotContains(haystack As String, needle As String, what As String)
        Assert(Not haystack.Contains(needle), what & " — must NOT contain: " & needle)
    End Sub

End Module
