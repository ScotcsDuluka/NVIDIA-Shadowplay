Option Strict On
Option Explicit On
Option Infer On

' SessionEndContractTests.vb — C/3 regression coverage for the Duluka
' session-end broadcast contract (Engine → Hub → Overlay).
'
' A RecordingEngine session can end three ways:
'   1. manual stop    → HandleRecordingStop owns the saved/error broadcast
'   2. natural expiry → DurationSeconds elapsed, nobody pressed stop
'   3. async failure  → StartSession faulted (mux abort, NVENC fault, ...)
'
' Endings 2 and 3 previously produced NO event at all — the Overlay kept
' showing "Recording" until the next user action (audit P1 × 2, 2026-09-06).
' The fix: a completion watcher (RecordingEngineHost.WatchSessionEnd)
' broadcasts for every UNOWNED ending; the stop path claims ownership
' first (ClaimSessionEndBroadcast) under one lock, so every ending is
' broadcast EXACTLY ONCE.
'
' Two layers (same pattern as P3UIContractTests / W2OverlayHonestyTests):
'   BEHAVIORAL — SessionEndBroadcastPolicy.Decide/DescribeFailure run for
'                real (the policy file is LINKED into this assembly).
'   SOURCE     — the wiring vectors in RecordingEngineHost.vb are pinned
'                (watcher attach, claim, gated broadcast, error broadcast).

Imports System
Imports System.IO
Imports Engine.ConfigTruth.Tests
Imports CaptureEngine.Recording

Friend Module SessionEndContractTests

    ' ── repo source locator (same walk as W2OverlayHonestyTests) ──

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
        Throw New Exception("repo root not found above " & AppContext.BaseDirectory)
    End Function

    Private Function Source(relPath As String) As String
        Dim p As String = Path.Combine(RepoRoot(), relPath.Replace("/"c, Path.DirectorySeparatorChar))
        If Not File.Exists(p) Then
            Throw New Exception("source file missing: " & p)
        End If
        Return File.ReadAllText(p)
    End Function

    Private Const HostVb As String = "Engine/Engine/[API]/RecordingEngineHost.vb"

    Private Sub ExpectContains(haystack As String, needle As String, what As String)
        Assert(haystack.Contains(needle), what & " — expected to contain: " & needle)
    End Sub

    ' ── SessionResult builders (Pass semantics per RecordingDTOs.vb) ──

    Private Function GoodResult() As SessionResult
        ' FramesEncoded>0, NvencErrors=0, FileExists, FileSize>0,
        ' VideoStreamFound, audio-expected-and-clean → Pass=True
        Return New SessionResult() With {
            .OutputPath = "C:\videos\out.mp4",
            .FramesEncoded = 120,
            .NvencErrors = 0,
            .FileExists = True,
            .FileSize = 123456L,
            .VideoStreamFound = True,
            .AudioRequested = True,
            .AudioStreamFound = True,
            .AudioDroppedBytes = 0L,
            .MuxDroppedBytes = 0L,
            .MicDroppedBytes = 0L
        }
    End Function

    Private Function BrokenResult() As SessionResult
        ' File missing → Pass=False (broken/unwritten recording)
        Return New SessionResult() With {
            .OutputPath = "C:\videos\out.mp4",
            .FramesEncoded = 3,
            .FileExists = False,
            .FileSize = 0L,
            .VideoStreamFound = False,
            .AudioRequested = True,
            .AudioStreamFound = False,
            .ErrorMessage = "LiveMux failed to start (ffmpeg) — session aborted"
        }
    End Function

    Public Sub RunAll()
        Console.WriteLine()
        Console.WriteLine("── Session-end broadcast contract (C/3) ──")

        ' Behavioral: the decision matrix (real policy code, linked source)
        TestRunner.RunTest("SESSEND: stop path owns the ending → watcher silent (no double toast)",
                           AddressOf T_StopOwns_IsNone)
        TestRunner.RunTest("SESSEND: natural expiry + pass → engine_recording_saved",
                           AddressOf T_Expiry_Pass_IsSaved)
        TestRunner.RunTest("SESSEND: natural expiry + fail → engine_recording_error",
                           AddressOf T_Expiry_Fail_IsError)
        TestRunner.RunTest("SESSEND: faulted task (no result) → engine_recording_error",
                           AddressOf T_Faulted_IsError)
        TestRunner.RunTest("SESSEND: failure detail mirrors stop-path evidence line",
                           AddressOf T_DescribeFailure)

        ' Source contract: the host wiring exists and cannot regress
        TestRunner.RunTest("SESSEND: host arms the completion watcher per session",
                           AddressOf T_Source_WatcherWired)
        TestRunner.RunTest("SESSEND: host claims broadcast ownership on stop + gates saved",
                           AddressOf T_Source_ClaimAndGate)
    End Sub

    ' ── behavioral ──

    Private Sub T_StopOwns_IsNone()
        ' Ending 1 (manual stop): stop path responds + broadcasts itself.
        Assert(SessionEndBroadcastPolicy.Decide(GoodResult(), True) = SessionEndAction.None,
               "pass + stop-owns must be None")
        Assert(SessionEndBroadcastPolicy.Decide(BrokenResult(), True) = SessionEndAction.None,
               "fail + stop-owns must be None")
        Assert(SessionEndBroadcastPolicy.Decide(Nothing, True) = SessionEndAction.None,
               "faulted + stop-owns must be None (stop path's Await Catch responds)")
    End Sub

    Private Sub T_Expiry_Pass_IsSaved()
        ' Ending 2 (natural expiry), healthy session: the file WAS written —
        ' the Overlay must get engine_recording_saved (was: silence).
        Assert(SessionEndBroadcastPolicy.Decide(GoodResult(), False) = SessionEndAction.Saved,
               "unowned pass must broadcast Saved")
    End Sub

    Private Sub T_Expiry_Fail_IsError()
        ' Ending 2/3 with a broken result: the Overlay must get
        ' engine_recording_error (was: silence while UI showed "Recording").
        Assert(SessionEndBroadcastPolicy.Decide(BrokenResult(), False) = SessionEndAction.Error,
               "unowned fail must broadcast Error")
    End Sub

    Private Sub T_Faulted_IsError()
        ' Ending 3 (async failure): StartSession threw — no SessionResult
        ' exists; the watcher must still broadcast Error (was: silence).
        Assert(SessionEndBroadcastPolicy.Decide(Nothing, False) = SessionEndAction.Error,
               "unowned faulted task must broadcast Error")
    End Sub

    Private Sub T_DescribeFailure()
        Dim fromResult As String = SessionEndBroadcastPolicy.DescribeFailure(BrokenResult(), "")
        Assert(fromResult.Contains("pass=False"), "result failure line starts with pass=False")
        Assert(fromResult.Contains("LiveMux failed to start"), "result failure carries ErrorMessage")

        Dim fromFault As String = SessionEndBroadcastPolicy.DescribeFailure(Nothing, "boom")
        Assert(fromFault = "session faulted: boom", "faulted task carries the fault message")

        Dim fromNothing As String = SessionEndBroadcastPolicy.DescribeFailure(Nothing, "")
        Assert(fromNothing = "session ended without a result", "no-result fallback exists")
    End Sub

    ' ── source contract (pins the wiring vectors) ──

    Private Sub T_Source_WatcherWired()
        Dim host As String = Source(HostVb)

        ' The watcher is armed for EVERY session, before any await.
        ExpectContains(host, "WatchSessionEnd(_recordingTask, sessionId)",
                       "RecordingEngineHost.vb watcher attach in HandleRecordingStart")
        ExpectContains(host, "Private Sub WatchSessionEnd(task As Task(Of SessionResult), sessionId As Long)",
                       "RecordingEngineHost.vb watcher method")
        ExpectContains(host, "SessionEndBroadcastPolicy.Decide(result, False)",
                       "RecordingEngineHost.vb decision through the linked policy")

        ' Stale-watcher + double-broadcast guards.
        ExpectContains(host, "If _currentSessionId <> sessionId Then Return",
                       "RecordingEngineHost.vb stale watcher guard")
    End Sub

    Private Sub T_Source_ClaimAndGate()
        Dim host As String = Source(HostVb)

        ' Stop path claims BEFORE signalling Stop.
        ExpectContains(host, "Dim stopOwnsBroadcast As Boolean = ClaimSessionEndBroadcast()",
                       "RecordingEngineHost.vb claim before Stop()")
        ' Search from the claim line onward (the file header comment also
        ' contains the string "_recordingEngine.Stop()").
        Dim claimIdx As Integer = host.IndexOf("Dim stopOwnsBroadcast As Boolean = ClaimSessionEndBroadcast()", StringComparison.Ordinal)
        Dim stopIdx As Integer = If(claimIdx >= 0, host.IndexOf("_recordingEngine.Stop()", claimIdx, StringComparison.Ordinal), -1)
        Assert(claimIdx >= 0 AndAlso stopIdx > claimIdx,
               "claim must precede _recordingEngine.Stop()")

        ' The saved broadcast is gated on ownership (no double toast when
        ' the watcher already reported an expiry ending).
        ExpectContains(host, "If stopOwnsBroadcast AndAlso tcp IsNot Nothing AndAlso tcp.IsConnected Then",
                       "RecordingEngineHost.vb saved broadcast gated on ownership")

        ' Exactly-once machinery lives under one lock.
        ExpectContains(host, "Private ReadOnly _sessionEndLock As New Object()",
                       "RecordingEngineHost.vb session-end lock")
        ExpectContains(host, "_sessionEndClaimed = True",
                       "RecordingEngineHost.vb claim mark")
    End Sub

End Module
