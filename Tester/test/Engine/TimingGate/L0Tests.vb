Option Strict On
Option Explicit On
Option Infer On

' L0Tests.vb — deterministic offline gate tests: PtsAnalyzer math and the
' config contract the FPS matrix relies on. No ffmpeg, no hardware, no I/O.

Imports System
Imports NVIDIA_Capture

Namespace TimingGate.Tests

    Friend Module L0Tests

        Friend Sub RunAll()
            Console.WriteLine("── W3-L0: offline analyzer + config contract (deterministic) ──")
            TestRunner.RunTest("W3-L0-A1: perfect grids 30/60/120/144/240 → GRID_OK/SAWTOOTH, exact stats", AddressOf Test_PerfectGrids)
            TestRunner.RunTest("W3-L0-A2: PTS monotonicity — duplicate/negative/non-increasing classified", AddressOf Test_Monotonicity)
            TestRunner.RunTest("W3-L0-A3: first-frame classes — FIRST_GAP(38ms shape)/START_OFFSET/SAWTOOTH", AddressOf Test_FirstFrameClasses)
            TestRunner.RunTest("W3-L0-A4: percentile + effFPS math on known vectors", AddressOf Test_PercentileMath)
            TestRunner.RunTest("W3-L0-A5: CaptureSettings.Validate fps bounds — 240 accepted, 241 rejected", AddressOf Test_FpsBounds)
            TestRunner.RunTest("W3-L0-V1: VFR verdict NEVER from avg_frame_rate — broken Δ with perfect avg must FAIL", AddressOf Test_AvgRateNotSufficient)
        End Sub

        Private Function Grid(fps As Integer, frames As Integer) As Double()
            Dim pts(frames - 1) As Double
            For i As Integer = 0 To frames - 1
                pts(i) = i / CDbl(fps)
            Next
            Return pts
        End Function

        Private Sub Test_PerfectGrids()
            For Each fps As Integer In New Integer() {30, 60, 120, 144, 240}
                Dim frames As Integer = fps * 2          ' 2 s of CFR
                Dim rep As PtsReport = PtsAnalyzer.Analyze(Grid(fps, frames), fps, 2.0,
                                                           frames / CDbl(fps), $"{fps}/1", frames)
                TestRunner.Assert(rep.IsPass, $"fps={fps}: expected PASS, got {rep.PrimaryClass}")
                TestRunner.Assert(rep.DupCount = 0, $"fps={fps}: dup={rep.DupCount}")
                TestRunner.Assert(rep.NegCount = 0, $"fps={fps}: neg={rep.NegCount}")
                TestRunner.Assert(rep.EffFps > fps - 0.001 AndAlso rep.EffFps < fps + 0.001,
                                  $"fps={fps}: effFPS={rep.EffFps:0.000}")
                Dim tMs As Double = 1000.0 / fps
                TestRunner.Assert(Math.Abs(rep.MedianDeltaMs - tMs) < 0.0001,
                                  $"fps={fps}: median={rep.MedianDeltaMs:0.0000} expected {tMs:0.0000}")
                TestRunner.Assert(Math.Abs(rep.FirstDeltaMs - tMs) < 0.0001,
                                  $"fps={fps}: firstΔ={rep.FirstDeltaMs:0.0000}")
                TestRunner.Assert(rep.Packets = frames, $"fps={fps}: packets={rep.Packets} ≠ frames={frames}")
            Next
        End Sub

        Private Sub Test_Monotonicity()
            ' one duplicated PTS slot → DUPLICATE_PTS (FAIL)
            Dim dupPts As Double() = {0.0, 0.033333, 0.066667, 0.066667, 0.1, 0.133333}
            Dim repDup As PtsReport = PtsAnalyzer.Analyze(dupPts, 30, 0, 0.0, "30/1", -1)
            TestRunner.Assert(Not repDup.IsPass, "duplicate-PTS stream passed")
            TestRunner.Assert(repDup.PrimaryClass = PtsClass.DUPLICATE_PTS AndAlso repDup.DupCount = 1,
                              $"dup class={repDup.PrimaryClass} count={repDup.DupCount}")

            ' one reversed PTS step → NEGATIVE_DELTA (FAIL)
            Dim negPts As Double() = {0.0, 0.033333, 0.066667, 0.050000, 0.1}
            Dim repNeg As PtsReport = PtsAnalyzer.Analyze(negPts, 30, 0, 0.0, "30/1", -1)
            TestRunner.Assert(Not repNeg.IsPass, "negative-PTS stream passed")
            TestRunner.Assert(repNeg.PrimaryClass = PtsClass.NEGATIVE_DELTA,
                              $"neg class={repNeg.PrimaryClass}")

            ' a fully decreasing tail → still NEGATIVE_DELTA
            Dim tailPts As Double() = {0.0, 0.033, 0.066, 0.065, 0.064}
            Dim repTail As PtsReport = PtsAnalyzer.Analyze(tailPts, 30, 0, 0.0, "30/1", -1)
            TestRunner.Assert(repTail.PrimaryClass = PtsClass.NEGATIVE_DELTA,
                              $"tail class={repTail.PrimaryClass}")

            ' rate drift — uniform 25fps mislabeled as 30 → RATE_DRIFT (FAIL)
            Dim driftPts As Double() = Grid(25, 50)
            Dim repDrift As PtsReport = PtsAnalyzer.Analyze(driftPts, 30, 2.0, 2.0, "25/1", 50)
            TestRunner.Assert(Not repDrift.IsPass, "25fps stream passed a 30fps gate")
            TestRunner.Assert(repDrift.PrimaryClass = PtsClass.RATE_DRIFT,
                              $"drift class={repDrift.PrimaryClass}")
        End Sub

        Private Sub Test_FirstFrameClasses()
            ' 38ms-class first gap at 30fps (grid resumes normally after) —
            ' must classify FIRST_GAP (38ms > T + max(3 ticks, 12%T) = 37.33ms)
            Dim gapPts(89) As Double
            For i As Integer = 0 To 89
                gapPts(i) = 0.038 + (i - 1) / 30.0
                If i = 0 Then gapPts(i) = 0.0
            Next
            Dim repGap As PtsReport = PtsAnalyzer.Analyze(gapPts, 30, 3.0, 0.0, "30/1", -1)
            TestRunner.Assert(repGap.PrimaryClass = PtsClass.FIRST_GAP,
                              $"38ms first gap classified {repGap.PrimaryClass}")

            ' timeline not starting at zero → START_OFFSET
            Dim offsetPts As Double() = Grid(30, 60)
            For i As Integer = 0 To offsetPts.Length - 1
                offsetPts(i) += 0.01
            Next
            Dim repOff As PtsReport = PtsAnalyzer.Analyze(offsetPts, 30, 2.0, 2.0, "30/1", 60)
            TestRunner.Assert(repOff.PrimaryClass = PtsClass.START_OFFSET,
                              $"start-offset classified {repOff.PrimaryClass}")

            ' 144fps container sawtooth (Δ alternating 106/107 ticks) — PASS class
            Dim saw(287) As Double
            Dim ticks As Long = 0
            For i As Integer = 0 To 287
                saw(i) = ticks / PtsConstants.TimebaseTicksPerSec
                ticks += If((i Mod 2) = 0, 106L, 107L)
            Next
            Dim repSaw As PtsReport = PtsAnalyzer.Analyze(saw, 144, 2.0, 288 / 144.0, "144/1", 288)
            TestRunner.Assert(repSaw.IsPass, $"144 tick-sawtooth must PASS, got {repSaw.PrimaryClass}")
            TestRunner.Assert(repSaw.PrimaryClass = PtsClass.TIMEBASE_SAWTOOTH,
                              $"sawtooth classified {repSaw.PrimaryClass}")

            ' clean 30fps grid must NOT be classified as sawtooth
            Dim clean As PtsReport = PtsAnalyzer.Analyze(Grid(30, 90), 30, 3.0, 3.0, "30/1", 90)
            TestRunner.Assert(clean.PrimaryClass = PtsClass.GRID_OK,
                              $"clean grid classified {clean.PrimaryClass}")
        End Sub

        Private Sub Test_PercentileMath()
            ' pts spaced 10/30/50/70/90 ms (5 deltas) → median 50, mean 50,
            ' P95 = 86 (interp: idx 3.8 → 70+(90-70)*0.8), P99 = 88.8
            Dim pts(5) As Double
            Dim acc As Double = 0.0
            Dim gaps As Double() = {0.01, 0.03, 0.05, 0.07, 0.09}
            For i As Integer = 0 To 5
                pts(i) = acc
                If i < 5 Then acc += gaps(i)
            Next
            Dim rep As PtsReport = PtsAnalyzer.Analyze(pts, 30, 0, 0.0, "?", -1)
            TestRunner.Assert(Math.Abs(rep.MedianDeltaMs - 50.0) < 0.001,
                              $"median={rep.MedianDeltaMs}")
            TestRunner.Assert(Math.Abs(rep.P95Ms - 86.0) < 0.001, $"P95={rep.P95Ms}")
            TestRunner.Assert(Math.Abs(rep.P99Ms - 89.2) < 0.001, $"P99={rep.P99Ms}")
            TestRunner.Assert(Math.Abs(rep.MeanDeltaMs - 50.0) < 0.001, $"mean={rep.MeanDeltaMs}")
        End Sub

        Private Sub Test_FpsBounds()
            Dim s As New CaptureSettings()
            s.FFmpegPath = TestRig._ffmpeg        ' isolate the FPS dimension
            For Each good As Integer In New Integer() {1, 30, 60, 120, 144, 240}
                s.FPS = good
                TestRunner.Assert(s.Validate().Valid, $"Validate rejected fps={good}")
            Next
            For Each bad As Integer In New Integer() {0, -1, 241}
                s.FPS = bad
                TestRunner.Assert(Not s.Validate().Valid, $"Validate accepted fps={bad}")
            Next
        End Sub

        ''' <summary>Mission rule: VFR verdicts must come from the ΔPTS
        ''' distribution — a container whose avg_frame_rate tag reads exactly
        ''' the target while the timeline carries duplicate+gap steps must
        ''' FAIL. Stream A (clean) and stream B (broken, same frames/span/avg)
        ''' differ only in Δ distribution; the analyzer must disagree.</summary>
        Private Sub Test_AvgRateNotSufficient()
            ' Stream A: perfect 30fps, 90 frames, avg tag "30/1" → PASS
            Dim repA As PtsReport = PtsAnalyzer.Analyze(Grid(30, 90), 30, 3.0, 3.0, "30/1", 90)
            TestRunner.Assert(repA.IsPass, "clean grid failed (sanity)")

            ' Stream B: same 90 slots, same total span (so the container's
            ' avg_frame_rate tag would still read exactly "30/1"), but ten
            ' duplicated PTS steps — each compensated by a double step right
            ' after, leaving the SUM of intervals (and hence the average
            ' rate) unchanged while the timeline is visibly non-CFR.
            Dim broken(89) As Double
            For i As Integer = 0 To 89
                broken(i) = i / 30.0
            Next
            For Each k As Integer In New Integer() {3, 9, 15, 21, 27, 33, 39, 45, 51, 57}
                broken(k) = broken(k - 1)          ' duplicated PTS slot
                ' broken(k+1) stays at (k+1)/30 = (k-1)/30 + 2/30 → double step
            Next
            Dim repB As PtsReport = PtsAnalyzer.Analyze(broken, 30, 3.0, 3.0, "30/1", 90)
            TestRunner.Assert(Not repB.IsPass,
                              "broken ΔPTS with a perfect avg tag PASSED — avg_frame_rate leaked into the verdict")
            TestRunner.Assert(repB.PrimaryClass = PtsClass.DUPLICATE_PTS,
                              $"broken stream classified {repB.PrimaryClass}")
            TestRunner.Assert(repB.DupCount = 10, $"expected 10 dup steps, got {repB.DupCount}")
            TestRunner.Assert(repA.ContainerAvgRate = repB.ContainerAvgRate,
                              "both streams must carry the identical avg tag (rule under test)")
        End Sub

    End Module

End Namespace
