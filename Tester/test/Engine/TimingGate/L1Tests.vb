Option Strict On
Option Explicit On
Option Infer On

' L1Tests.vb — legacy engine (REAL NVIDIA_Capture.CaptureEngine) driven
' through the helper-exe seam: deterministic failure injection, restart,
' and honesty contracts on the SINGLE-PROCESS video-only path. No hardware.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace TimingGate.Tests

    Friend Module L1Tests

        ''' <summary>Thread-safe event recorder (F03 pattern).</summary>
        Friend NotInheritable Class Rec
            Public ReadOnly Started As New List(Of String)()
            Public ReadOnly Stopped As New List(Of String)()
            Public ReadOnly Errors As New List(Of String)()

            Public Sub Wire(engine As EngineCapture)
                AddHandler engine.RecordingStarted, Sub(f) LockAdd(Started, f)
                AddHandler engine.RecordingStopped, Sub(f) LockAdd(Stopped, f)
                AddHandler engine.ErrorOccurred, Sub(m) LockAdd(Errors, m)
            End Sub

            Private Shared Sub LockAdd(list As List(Of String), item As String)
                SyncLock list : list.Add(item) : End SyncLock
            End Sub

            Public Function Count(list As List(Of String)) As Integer
                SyncLock list : Return list.Count : End SyncLock
            End Function
        End Class

        Friend Sub RunAll()
            Console.WriteLine("── W3-L1: legacy engine + helper seam (failure/restart/honesty, machine-independent) ──")
            TestRunner.RunTest("W3-L1-B1: unexpected ffmpeg death — error exactly once, no saved event, terminal deterministic, no orphan",
                               AddressOf Test_UnexpectedDeath)
            TestRunner.RunTest("W3-L1-B2: stop-after-fault idempotency — no double terminal event", AddressOf Test_StopAfterFaultIdempotent)
            TestRunner.RunTest("W3-L1-B3: FALSE-SUCCESS — failed session with garbage output must not report success",
                               AddressOf Test_FalseSuccessGarbage)
            TestRunner.RunTest("W3-L1-B4: valid helper stream — saved exactly once + full PTS/packet integrity", AddressOf Test_ValidStream)
            TestRunner.RunTest("W3-L1-B5: restart-after-fault — next recording valid", AddressOf Test_RestartAfterFault)
            TestRunner.RunTest("W3-L1-C1: fps-switch restarts 15→30→15 on ONE engine — each file on its own grid", AddressOf Test_FpsSwitchRestarts)
        End Sub

        Private Sub ConfigureHelper(src As String, createEmpty As Boolean, sleepS As Integer, exitCode As Integer)
            TestRig.ClearHelperEnv()
            If createEmpty Then
                TestRig.SetEnv("LMHLP_CREATE_EMPTY", "1")
                TestRig.SetEnv("LMHLP_COPYTO", _currentOutput)
            ElseIf src IsNot Nothing Then
                TestRig.SetEnv("LMHLP_SRC", src)
                TestRig.SetEnv("LMHLP_COPYTO", _currentOutput)
            End If
            TestRig.SetEnv("LMHLP_SLEEP", sleepS.ToString())
            TestRig.SetEnv("LMHLP_EXIT", exitCode.ToString())
        End Sub

        Private _currentOutput As String = ""

        Private Function WaitStateNot(engine As EngineCapture, state As EngineCapture.CaptureState, timeoutMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < timeoutMs
                If engine.State <> state Then Return True
                Thread.Sleep(50)
            End While
            Return engine.State <> state
        End Function

        Private Function HelperProcesses() As Integer
            Dim name As String = IO.Path.GetFileNameWithoutExtension(Environment.ProcessPath)
            Dim n As Integer = 0
            For Each p As Process In Process.GetProcessesByName(name)
                Try
                    If Not p.HasExited Then n += 1
                Catch
                End Try
                Try : p.Dispose() : Catch : End Try
            Next
            Return n
        End Function

        Private Function WaitFor(cond As Func(Of Boolean), timeoutMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < timeoutMs
                If cond() Then Return True
                Thread.Sleep(50)
            End While
            Return cond()
        End Function

        ' ── B1 ──
        Private Sub Test_UnexpectedDeath()
            Dim baselineHelpers As Integer = HelperProcesses()
            Dim baselineFfmpeg As Integer = TestRig.FfmpegCount()
            _currentOutput = Path.Combine(TestRig._sandbox, "l1_b1.mp4")
            If File.Exists(_currentOutput) Then File.Delete(_currentOutput)
            ConfigureHelper(Nothing, False, 2, -40)    ' dies ~2s in, before writing anything

            Dim rec As New Rec()
            Dim engine As New EngineCapture(TestRig.HelperSettings(15))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False (helper seam broken)")
                TestRunner.Assert(WaitStateNot(engine, EngineCapture.CaptureState.Recording, 10000),
                                  "engine stayed Recording after the helper died")

                ' Failure propagation: surfaced exactly once, with evidence.
                TestRunner.Assert(rec.Count(rec.Errors) = 1,
                                  $"expected exactly 1 error event, got {rec.Count(rec.Errors)}: [{Join(rec.Errors)}]")

                ' The stop flow must settle the terminal contract without a save.
                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                ' ★ M2/W1 regression lock (absent-output leg): the stop flow ran,
                ' but nothing usable landed on disk — the boolean must say so.
                TestRunner.Assert(Not stopOk,
                                  "FALSE SUCCESS: stop returned True for a session whose output file is absent")
                TestRunner.Assert(rec.Count(rec.Stopped) = 0,
                                  $"RecordingStopped fired for a nonexistent output ({rec.Count(rec.Stopped)}×)")
                TestRunner.Assert(WaitFor(Function() engine.State = EngineCapture.CaptureState.Idle, 5000),
                                  $"engine did not settle Idle after stop (state={engine.State})")

                TestRunner.Assert(Not File.Exists(_currentOutput), "output file appeared despite dead helper")
                TestRunner.Assert(WaitFor(Function() HelperProcesses() = baselineHelpers, 8000),
                                  "helper process leaked")
                TestRunner.Assert(WaitFor(Function() TestRig.FfmpegCount() <= baselineFfmpeg, 8000),
                                  "ffmpeg-level process leaked")
            Finally
                TestRig.ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        ' ── B2 ──
        Private Sub Test_StopAfterFaultIdempotent()
            Dim rec As New Rec()
            Dim engine As New EngineCapture(TestRig.HelperSettings(15))
            rec.Wire(engine)
            Try
                _currentOutput = Path.Combine(TestRig._sandbox, "l1_b2.mp4")
                ConfigureHelper(Nothing, False, 2, -40)
                Dim started As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(WaitStateNot(engine, EngineCapture.CaptureState.Recording, 10000),
                                  "fault never surfaced")
                Dim errorsAtFault As Integer = rec.Count(rec.Errors)
                TestRunner.Assert(errorsAtFault >= 1, "unexpected exit never surfaced as an error")

                Dim stop1 As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                Dim stopped1 As Integer = rec.Count(rec.Stopped)
                Dim errors1 As Integer = rec.Count(rec.Errors)
                TestRunner.Assert(WaitFor(Function() engine.State = EngineCapture.CaptureState.Idle, 5000),
                                  $"first stop left state={engine.State}")

                ' Idempotency: a second stop must change NOTHING observable.
                Dim stop2 As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(rec.Count(rec.Stopped) = stopped1, "second stop fired a new Stopped event")
                TestRunner.Assert(rec.Count(rec.Errors) = errors1, "second stop fired a new error")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"second stop left state={engine.State}")
            Finally
                TestRig.ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        ' ── B3 — FALSE-SUCCESS contract (was KNOWN-RED, baseline 2026-09-11) ──
        ' Contract under test: when the session FAILED (error surfaced) and the
        ' output is invalid (0-byte garbage), StopRecordingAsync must report
        ' failure (False) and no Stopped event may fire. Live evidence that this
        ' was violated: the real QSV-240 run (M2-W3) returned
        ' stopped=True with a 0-byte moov-less file. Root shape: Step-4
        ' (CaptureEngine.vb:560-586) keyed honesty on File.Exists alone on the
        ' single-process path, and the stop returned True unconditionally.
        ' FIXED by the IsOutputFileValid gate (M2/W1 2026-09-12) — this test is
        ' now the regression lock for the garbage leg. B1 covers the absent leg.
        Private Sub Test_FalseSuccessGarbage()
            Dim rec As New Rec()
            Dim engine As New EngineCapture(TestRig.HelperSettings(15))
            rec.Wire(engine)
            Try
                _currentOutput = Path.Combine(TestRig._sandbox, "l1_b3.mp4")
                ConfigureHelper(Nothing, True, 2, -40)    ' 0-byte garbage then die
                Dim started As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(WaitStateNot(engine, EngineCapture.CaptureState.Recording, 10000),
                                  "fault never surfaced")
                TestRunner.Assert(rec.Count(rec.Errors) >= 1, "session failure was not surfaced as an error")

                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()

                ' The contract (this is the assertion that is RED on current code):
                TestRunner.Assert(Not stopOk,
                                  "FALSE SUCCESS: StopRecordingAsync returned True for a FAILED session whose output is invalid")
                TestRunner.Assert(rec.Count(rec.Stopped) = 0,
                                  $"RecordingStopped fired {rec.Count(rec.Stopped)}× for garbage output")
                Dim fi As New FileInfo(_currentOutput)
                Dim garbage As Boolean = (Not fi.Exists) OrElse fi.Length = 0
                TestRunner.Assert(garbage, "fixture precondition lost: output should be 0-byte/absent")
                TestRunner.Assert(WaitFor(Function() engine.State = EngineCapture.CaptureState.Idle, 5000),
                                  $"engine did not settle Idle (state={engine.State})")
            Finally
                TestRig.ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        ' ── B4 ──
        Private Sub Test_ValidStream()
            Dim fixture As String = TestRig.GenerateTestSrc(30, 2.0)
            Dim rec As New Rec()
            Dim engine As New EngineCapture(TestRig.HelperSettings(30))
            rec.Wire(engine)
            Try
                _currentOutput = Path.Combine(TestRig._sandbox, "l1_b4.mp4")
                ConfigureHelper(fixture, False, 30, 0)
                Dim started As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                Thread.Sleep(1500)
                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "graceful stop returned False")

                TestRunner.Assert(rec.Count(rec.Stopped) = 1,
                                  $"expected RecordingStopped exactly once, got {rec.Count(rec.Stopped)}")
                Dim saved As String = ""
                SyncLock rec.Stopped
                    If rec.Stopped.Count > 0 Then saved = rec.Stopped(0)
                End SyncLock
                TestRunner.Assert(saved = _currentOutput, $"saved path {saved} ≠ {_currentOutput}")

                ' encode→packet + full PTS gate on the delivered container
                Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, _currentOutput, 30, 2.0, "W3-L1-B4")
                TestRunner.Report(rep)
                TestRunner.Assert(rep.IsPass, $"L1-B4 grid verdict FAIL: {rep}")
                TestRunner.Assert(WaitFor(Function() engine.State = EngineCapture.CaptureState.Idle, 5000),
                                  $"state after stop = {engine.State}")
            Finally
                TestRig.ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        ' ── B5 ──
        Private Sub Test_RestartAfterFault()
            Dim rec As New Rec()
            Dim engine As New EngineCapture(TestRig.HelperSettings(15))
            rec.Wire(engine)
            Try
                ' 1) fault
                _currentOutput = Path.Combine(TestRig._sandbox, "l1_b5_fault.mp4")
                ConfigureHelper(Nothing, False, 2, -40)
                Dim started As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(WaitStateNot(engine, EngineCapture.CaptureState.Recording, 10000), "fault never surfaced")
                engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(WaitFor(Function() engine.State = EngineCapture.CaptureState.Idle, 5000),
                                  $"engine not Idle after fault stop (state={engine.State})")

                ' 2) immediate restart must succeed and deliver a valid grid
                Dim fixture As String = TestRig.GenerateTestSrc(15, 2.0)
                _currentOutput = Path.Combine(TestRig._sandbox, "l1_b5_recover.mp4")
                ConfigureHelper(fixture, False, 30, 0)
                Dim restarted As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                TestRunner.Assert(restarted, "engine was NOT reusable after a fault (restart rejected)")
                Thread.Sleep(1200)
                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "recovered stop returned False")
                TestRunner.Assert(rec.Count(rec.Stopped) = 1,
                                  $"expected exactly one save after recovery, got {rec.Count(rec.Stopped)}")

                Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, _currentOutput, 15, 2.0, "W3-L1-B5")
                TestRunner.Report(rep)
                TestRunner.Assert(rep.IsPass, $"L1-B5 recovered grid FAIL: {rep}")
            Finally
                TestRig.ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        ' ── C1 ──
        Private Sub Test_FpsSwitchRestarts()
            Dim rec As New Rec()
            Dim engine As New EngineCapture(TestRig.HelperSettings(15))
            rec.Wire(engine)
            Try
                Dim fixtures As New Dictionary(Of Integer, String)()
                For Each fps As Integer In New Integer() {15, 30, 15}
                    If Not fixtures.ContainsKey(fps) Then fixtures(fps) = TestRig.GenerateTestSrc(fps, 2.0)
                Next

                Dim cycle As Integer = 0
                For Each fps As Integer In New Integer() {15, 30, 15}
                    cycle += 1
                    _currentOutput = Path.Combine(TestRig._sandbox, $"l1_c1_cycle{cycle}_{fps}fps.mp4")
                    ConfigureHelper(fixtures(fps), False, 30, 0)
                    Dim started As Boolean = engine.StartRecordingAsync(_currentOutput).GetAwaiter().GetResult()
                    TestRunner.Assert(started, $"cycle {cycle}: start rejected")
                    Thread.Sleep(1200)
                    Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                    TestRunner.Assert(stopOk, $"cycle {cycle}: stop returned False")
                    TestRunner.Assert(WaitFor(Function() engine.State = EngineCapture.CaptureState.Idle, 5000),
                                      $"cycle {cycle}: state={engine.State} after stop")

                    Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, _currentOutput, fps, 2.0, $"W3-L1-C1#{cycle}")
                    TestRunner.Report(rep)
                    TestRunner.Assert(rep.IsPass, $"cycle {cycle} ({fps}fps) grid FAIL: {rep}")
                Next
                TestRunner.Assert(rec.Count(rec.Stopped) = 3,
                                  $"expected 3 saves total, got {rec.Count(rec.Stopped)}")
            Finally
                TestRig.ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        Private Function Join(list As List(Of String)) As String
            SyncLock list : Return String.Join(" | ", list) : End SyncLock
        End Function

    End Module

End Namespace
