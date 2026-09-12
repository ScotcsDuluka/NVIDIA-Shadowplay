Option Strict On
Option Explicit On
Option Infer On

' L2Tests.vb — REAL capture+encode lanes, hardware-conditional.
'
' QSV lane (this class of machine): legacy engine, ddagrab → h264_qsv, the
' exact production argument mapping from FFmpegArgumentBuilder. Per mode the
' QsvGate probe decides the EXPECTATION:
'   ModeSupported(fps)=True  → GRID assertions (W3-A1..A7 via PtsProbe)
'   ModeSupported(fps)=False → HONEST_FAILURE assertions (the mode must fail
'                              loudly: error surfaced, no false success,
'                              engine reusable)
' Native lane: RecordingEngine (Ddagrab+NVENC) — BLOCKED without NVIDIA.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports EngineCapture = NVIDIA_Capture.CaptureEngine
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Recording

Namespace TimingGate.Tests

    Friend Module L2Tests

        Friend Sub RunAll()
            Console.WriteLine("── W3-L2: real capture lanes (hardware-conditional) ──")
            TestRunner.RunTest("W3-L2-Q1: real grid gate 30fps (ddagrab+qsv, A1–A7)", AddressOf Test_QsvGrid30)
            TestRunner.RunTest("W3-L2-Q2: real grid gate 60fps (ddagrab+qsv, A1–A7)", AddressOf Test_QsvGrid60)
            TestRunner.RunTest("W3-L2-Q3: real grid gate 120fps (ddagrab+qsv, A1–A7)", AddressOf Test_QsvGrid120)
            TestRunner.RunTest("W3-L2-Q4: real grid gate 144fps (ddagrab+qsv, A1–A7)", AddressOf Test_QsvGrid144)
            TestRunner.RunTest("W3-L2-Q5a: 240fps — failure propagation + engine reusable (D1/D3)", AddressOf Test_Qsv240Honest)
            TestRunner.RunTest("W3-L2-Q5b: 240fps — no false success for a failed session (D2)",
                               AddressOf Test_Qsv240FalseSuccess)
            TestRunner.RunTest("W3-L2-C1: real fps-switch restarts 30→60→30 on ONE engine", AddressOf Test_QsvFpsSwitch)
            TestRunner.RunTest("W3-L2-N1: native engine grid 30fps [NVIDIA-gated]", AddressOf Test_NativeGrid30)
            TestRunner.RunTest("W3-L2-N2: native engine grid 60fps [NVIDIA-gated]", AddressOf Test_NativeGrid60)
            TestRunner.RunTest("W3-L2-N3: native engine grid 120fps [NVIDIA-gated]", AddressOf Test_NativeGrid120)
            TestRunner.RunTest("W3-L2-N4: native engine grid 144fps [NVIDIA-gated]", AddressOf Test_NativeGrid144)
            TestRunner.RunTest("W3-L2-N5: native engine grid 240fps [NVIDIA-gated]", AddressOf Test_NativeGrid240)
            TestRunner.RunTest("W3-L2-N6: native per-session FPS rebuild authority 60→30→120 [NVIDIA-gated]", AddressOf Test_NativeFpsRebuild)
        End Sub

        ' ─────────────── QSV lane ───────────────

        Private Sub Test_QsvGrid30()
            Test_QsvGrid(30, 5.0)
        End Sub
        Private Sub Test_QsvGrid60()
            Test_QsvGrid(60, 5.0)
        End Sub
        Private Sub Test_QsvGrid120()
            Test_QsvGrid(120, 5.0)
        End Sub
        Private Sub Test_QsvGrid144()
            Test_QsvGrid(144, 5.0)
        End Sub

        Private Sub Test_QsvGrid(fps As Integer, sec As Double)
            TestRig.SetEnv("LMHLP_SRC", Nothing)   ' keep the seam env clean for L1 reruns
            If Not QsvGate.ModeSupported(TestRig._ffmpeg, fps) Then
                If Not QsvGate.QsvAvailable Then
                    Throw New SkipException($"QSV lane unavailable: {QsvGate.Reason}")
                End If
                Throw New SkipException($"fps={fps}: QSV runtime rejects this frame rate — run as HONEST_FAILURE cell, not GRID")
            End If
            RunRealQsvRecording(fps, sec, $"W3-L2-Q{fps}")
        End Sub

        ''' <summary>One real recording + full grid gate. Returns the report.</summary>
        Private Function RunRealQsvRecording(fps As Integer, sec As Double, label As String) As PtsReport
            Dim baselineFfmpeg As Integer = TestRig.FfmpegCount()
            Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_{label}_{fps}fps_{DateTime.Now:HHmmssfff}.mp4")
            Dim rec As New L1Tests.Rec()
            Dim engine As New EngineCapture(TestRig.QsvSettings(fps))
            rec.Wire(engine)
            Try
                Dim sw As Stopwatch = Stopwatch.StartNew()
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, $"{label}: real recording failed to start (check encoder)")
                While sw.Elapsed.TotalSeconds < sec
                    Thread.Sleep(100)
                End While
                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, $"{label}: stop returned False")
                TestRunner.Assert(WaitEngineIdle(engine, 8000), $"{label}: state={engine.State} after stop")
                TestRunner.Assert(rec.Count(rec.Errors) = 0,
                                  $"{label}: unexpected errors on the green path: [{Join(rec.Errors)}]")

                Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, outPath, fps, sec, label)
                TestRunner.Report(rep)
                TestRunner.Assert(rep.IsPass, $"{label}: grid verdict FAIL — {rep}")
                TestRunner.Assert(rep.DupCount = 0 AndAlso rep.NegCount = 0,
                                  $"{label}: monotonicity breach — {rep}")
                TestRunner.Assert(WaitForFfmpegAtMost(baselineFfmpeg, 8000),
                                  $"{label}: ffmpeg process did not return to baseline")
                Return rep
            Finally
                engine.Dispose()
            End Try
        End Function

        ''' <summary>240 fps: the QsvGate probe decides the cell semantics.
        ''' Supported → full grid gate. Unsupported → HONEST_FAILURE:
        '''   Q5a (this test): the failure must surface with the encoder's own
        '''   evidence, the engine must settle Idle and stay reusable.
        '''   Q5b (was KNOWN-RED 2026-09-11; fixed by the IsOutputFileValid gate,
        '''   M2/W1 2026-09-12): a failed session with invalid output must not
        '''   report success — proven live by M2-W3 on this exact machine
        '''   (stop returned True over a 0-byte moov-less file).</summary>
        Private Sub Test_Qsv240Honest()
            If QsvGate.ModeSupported(TestRig._ffmpeg, 240) Then
                RunRealQsvRecording(240, 5.0, "W3-L2-Q5")
                Return
            End If
            If Not QsvGate.QsvAvailable Then
                Throw New SkipException($"QSV lane unavailable: {QsvGate.Reason} — 240fps cell BLOCKED")
            End If

            Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_q5a_240_{DateTime.Now:HHmmssfff}.mp4")
            Dim rec As New L1Tests.Rec()
            Dim engine As New EngineCapture(TestRig.QsvSettings(240))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                If Not started Then
                    ' Start-time rejection is an honest failure — acceptable.
                    TestRunner.Assert(rec.Count(rec.Errors) >= 1, "start rejected without any error event")
                    AssertReusableQsv()
                    Return
                End If

                ' D1: the failure must surface with the encoder's own evidence.
                Dim surfaced As Boolean = False
                Dim sw As Stopwatch = Stopwatch.StartNew()
                While sw.Elapsed.TotalSeconds < 8
                    If rec.Count(rec.Errors) > 0 Then surfaced = True : Exit While
                    Thread.Sleep(100)
                End While
                TestRunner.Assert(surfaced, "encoder rejection never surfaced as an error event")
                TestRunner.Assert(Join(rec.Errors).Length > 0, "error event had empty text")

                engine.StopRecordingAsync().GetAwaiter().GetResult()

                ' D3: deterministic terminal + reusable engine.
                TestRunner.Assert(WaitEngineIdle(engine, 8000), $"state after 240 failure = {engine.State}")
                AssertReusableQsv()
            Finally
                engine.Dispose()
            End Try
        End Sub

        Private Sub Test_Qsv240FalseSuccess()
            If QsvGate.ModeSupported(TestRig._ffmpeg, 240) Then
                ' Mode became supported on this machine — the false-success cell
                ' is not exercisable here anymore; the GRID cell covers it.
                TestRunner.Report(New PtsReport() With {.PrimaryClass = PtsClass.GRID_OK,
                    .TargetFps = 240, .Frames = 0, .Notes = New List(Of String) From {"240fps supported — D2 cell N/A (GRID gate owns it)"}})
                Return
            End If
            If Not QsvGate.QsvAvailable Then
                Throw New SkipException($"QSV lane unavailable: {QsvGate.Reason}")
            End If

            Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_q5b_240_{DateTime.Now:HHmmssfff}.mp4")
            Dim rec As New L1Tests.Rec()
            Dim engine As New EngineCapture(TestRig.QsvSettings(240))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start rejected — cell shape changed (see Q5a)")

                Dim sw As Stopwatch = Stopwatch.StartNew()
                While sw.Elapsed.TotalSeconds < 8 AndAlso rec.Count(rec.Errors) = 0
                    Thread.Sleep(100)
                End While
                TestRunner.Assert(rec.Count(rec.Errors) >= 1, "session failure never surfaced")

                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()

                ' The KNOWN-RED contract: a failed session with invalid output
                ' must not report success.
                Dim fi As New FileInfo(outPath)
                Dim validOutput As Boolean = False
                If fi.Exists AndAlso fi.Length > 0 Then
                    Try
                        Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, outPath, 240, 0, "W3-L2-Q5b-out")
                        validOutput = rep.IsPass
                    Catch
                        validOutput = False
                    End Try
                End If
                If Not validOutput Then
                    TestRunner.Assert(Not stopOk,
                                      "FALSE SUCCESS: stop returned True for a failed 240fps session with invalid output")
                    TestRunner.Assert(rec.Count(rec.Stopped) = 0,
                                      $"RecordingStopped fired {rec.Count(rec.Stopped)}× for invalid 240fps output")
                End If
            Finally
                engine.Dispose()
            End Try
        End Sub

        Private Sub AssertReusableQsv()
            ' D3: the engine must still work for a supported mode right after.
            Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_q5_reuse_{DateTime.Now:HHmmssfff}.mp4")
            Dim engine As New EngineCapture(TestRig.QsvSettings(60))
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "engine not reusable after 240 failure (60fps start rejected)")
                Thread.Sleep(1500)
                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "recovery stop returned False")
                Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, outPath, 60, 1.5, "W3-L2-Q5-reuse")
                TestRunner.Report(rep)
                TestRunner.Assert(rep.IsPass, $"post-failure recovery grid FAIL: {rep}")
            Finally
                engine.Dispose()
            End Try
        End Sub

        ' ─────────────── QSV restart across fps ───────────────

        Private Sub Test_QsvFpsSwitch()
            If Not QsvGate.QsvAvailable Then
                Throw New SkipException($"QSV lane unavailable: {QsvGate.Reason}")
            End If
            Dim engine As New EngineCapture(TestRig.QsvSettings(30))
            Try
                Dim cycle As Integer = 0
                For Each fps As Integer In New Integer() {30, 60, 30}
                    cycle += 1
                    Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_c1_cycle{cycle}_{fps}fps.mp4")
                    Dim cfgEngine As New EngineCapture(TestRig.QsvSettings(fps))
                    Try
                        Dim started As Boolean = cfgEngine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                        TestRunner.Assert(started, $"cycle {cycle} ({fps}fps): start rejected")
                        Thread.Sleep(1500)
                        Dim stopOk As Boolean = cfgEngine.StopRecordingAsync().GetAwaiter().GetResult()
                        TestRunner.Assert(stopOk, $"cycle {cycle}: stop returned False")
                        TestRunner.Assert(WaitEngineIdle(cfgEngine, 8000), $"cycle {cycle}: state={cfgEngine.State}")
                        Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, outPath, fps, 1.5, $"W3-L2-C1#{cycle}")
                        TestRunner.Report(rep)
                        TestRunner.Assert(rep.IsPass, $"cycle {cycle} ({fps}fps) grid FAIL: {rep}")
                    Finally
                        cfgEngine.Dispose()
                    End Try
                Next
            Finally
                engine.Dispose()
            End Try
        End Sub

        ' ─────────────── Native lane (NVIDIA-gated) ───────────────

        Private Sub Test_NativeGrid30()
            Test_NativeGrid(30)
        End Sub
        Private Sub Test_NativeGrid60()
            Test_NativeGrid(60)
        End Sub
        Private Sub Test_NativeGrid120()
            Test_NativeGrid(120)
        End Sub
        Private Sub Test_NativeGrid144()
            Test_NativeGrid(144)
        End Sub
        Private Sub Test_NativeGrid240()
            Test_NativeGrid(240)
        End Sub

        Private Sub RequireNative()
            If Not NativeGate.NativeAvailable Then
                Throw New SkipException($"native lane unavailable: {NativeGate.Reason}")
            End If
        End Sub

        Private Sub Test_NativeGrid(fps As Integer)
            RequireNative()
            Dim logger As New EngineLogger("w3-native", EngineLogger.LogLevel.Warning)
            Dim eng As New RecordingEngine(logger)
            Try
                Dim startup As New EngineStartupConfig()
                startup.Fps = 60
                eng.Initialize(startup)

                Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_n_{fps}_{DateTime.Now:HHmmssfff}.mp4")
                Dim cfg As New SessionConfig() With {
                    .OutputPath = outPath,
                    .DurationSeconds = 3,
                    .FFmpegPath = TestRig._ffmpeg,
                    .TargetFps = fps,
                    .AudioEnabled = False
                }
                Dim r As SessionResult = eng.StartSession(cfg)
                TestRunner.Assert(r.Pass, $"native {fps}fps session failed: {r.ErrorMessage}")
                TestRunner.Assert(r.FramesEncoded > 1, $"native {fps}fps: frames_encoded={r.FramesEncoded}")

                Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, outPath, fps, 3.0, $"W3-L2-N({fps})")
                TestRunner.Report(rep)
                TestRunner.Assert(rep.IsPass, $"native {fps}fps grid FAIL: {rep}")
            Finally
                Try : eng.Dispose() : Catch : End Try
            End Try
        End Sub

        ''' <summary>The NVENC FPS authority must follow the SESSION fps
        ''' (per-session rebuild): 60-init engine, sessions at 30 then 120,
        ''' each file on ITS OWN grid — the "120 frames interpreted as 60"
        ''' bug class.</summary>
        Private Sub Test_NativeFpsRebuild()
            RequireNative()
            Dim logger As New EngineLogger("w3-native", EngineLogger.LogLevel.Warning)
            Dim eng As New RecordingEngine(logger)
            Try
                Dim startup As New EngineStartupConfig()
                startup.Fps = 60
                eng.Initialize(startup)
                For Each fps As Integer In New Integer() {30, 120}
                    Dim outPath As String = Path.Combine(TestRig._sandbox, $"l2_n6_{fps}_{DateTime.Now:HHmmssfff}.mp4")
                    Dim cfg As New SessionConfig() With {
                        .OutputPath = outPath,
                        .DurationSeconds = 3,
                        .FFmpegPath = TestRig._ffmpeg,
                        .TargetFps = fps,
                        .AudioEnabled = False
                    }
                    Dim r As SessionResult = eng.StartSession(cfg)
                    TestRunner.Assert(r.Pass, $"rebuild session {fps}fps failed: {r.ErrorMessage}")
                    Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, outPath, fps, 3.0, $"W3-L2-N6({fps})")
                    TestRunner.Report(rep)
                    TestRunner.Assert(rep.IsPass, $"rebuild session {fps}fps grid FAIL: {rep}")
                Next
            Finally
                Try : eng.Dispose() : Catch : End Try
            End Try
        End Sub

        ' ─────────────── shared helpers ───────────────

        Private Function WaitEngineIdle(engine As EngineCapture, timeoutMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < timeoutMs
                If engine.State = EngineCapture.CaptureState.Idle Then Return True
                Thread.Sleep(100)
            End While
            Return engine.State = EngineCapture.CaptureState.Idle
        End Function

        Private Function WaitForFfmpegAtMost(maxCount As Integer, budgetMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < budgetMs
                If TestRig.FfmpegCount() <= maxCount Then Return True
                Thread.Sleep(100)
            End While
            Return TestRig.FfmpegCount() <= maxCount
        End Function

        Private Function Join(list As List(Of String)) As String
            SyncLock list : Return String.Join(" | ", list) : End SyncLock
        End Function

    End Module

End Namespace
