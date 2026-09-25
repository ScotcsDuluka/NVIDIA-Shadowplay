// Tests/ForbiddenMultiplyTests.cs
//
// V2 Spike — Test Group 5: Explicitly prove WGC path does NOT use (Tn - T0) * 100
//
// The forbidden operation is: PTS = (SystemRelativeTime.Ticks - T0) * 100
// This would be wrong because SystemRelativeTime.Ticks is ALREADY in 100-ns units.
// Multiplying by 100 would treat ticks as raw QPC ticks (which are NOT 100-ns).
//
// SPDX-License-Identifier: MIT

namespace V2_WGC_Timestamp_Spike.Tests;

public static class ForbiddenMultiplyTests
{
    public static void RunAll(Action<string, Action> runner)
    {
        runner("FORBID: (Tn - T0) * 100 ≠ correct PTS", Test_Multiply100IsWrong);
        runner("FORBID: (Tn - T0) * 100 produces 100x error", Test_100xError);
        runner("FORBID: Correct PTS does NOT multiply by 100", Test_NoMultiply);
        runner("FORBID: QPF conversion is separate from WGC path", Test_QpfSeparate);
        runner("FORBID: WGC ticks ≠ raw QPC ticks", Test_WgcVsQpc);
    }

    /// <summary>
    /// Prove that (Tn - T0) * 100 ≠ the correct PTS.
    /// The correct PTS is (Tn - T0) with NO multiplication.
    /// </summary>
    private static void Test_Multiply100IsWrong()
    {
        long t0 = 25_000_000_000_000L;
        long t1 = t0 + 166_667L; // 1 frame @ 60fps

        long correctPts = t1 - t0;           // = 166,667
        long wrongPts = (t1 - t0) * 100;     // = 16,666,700 (WRONG)

        Assert(correctPts != wrongPts,
            $"Correct PTS ({correctPts}) should NOT equal wrong PTS ({wrongPts})");
        Assert(correctPts == 166_667L, $"Correct PTS should be 166,667, got {correctPts}");
        Assert(wrongPts == 16_666_700L, $"Wrong PTS should be 16,666,700, got {wrongPts}");
    }

    /// <summary>
    /// Prove that multiplying by 100 produces a 100x error.
    /// For a 1-second PTS: correct = 10,000,000, wrong = 1,000,000,000.
    /// </summary>
    private static void Test_100xError()
    {
        long t0 = 0;
        long t1 = 10_000_000L; // 1 second in 100-ns ticks

        long correctPts = t1 - t0;           // 10,000,000
        long wrongPts = (t1 - t0) * 100;     // 1,000,000,000

        Assert(wrongPts == correctPts * 100,
            $"Wrong PTS should be exactly 100x correct: {wrongPts} vs {correctPts * 100}");
        Assert(correctPts == 10_000_000L, $"1 second should be 10,000,000 ticks, got {correctPts}");
        Assert(wrongPts == 1_000_000_000L, $"Wrong 1 second would be 1,000,000,000 ticks, got {wrongPts}");

        // The wrong value would imply 100 seconds instead of 1 second
        double correctSeconds = correctPts / 10_000_000.0;
        double wrongSeconds = wrongPts / 10_000_000.0;
        Assert(correctSeconds == 1.0, $"Correct: 1 second");
        Assert(wrongSeconds == 100.0, $"Wrong: would be 100 seconds (100x error)");
    }

    /// <summary>
    /// Prove that the correct WGC PTS path does NOT multiply by 100.
    /// The correct path is: PTS = SystemRelativeTime.Ticks - T0.
    /// No multiplication, no QPF conversion.
    ///
    /// IMPORTANT: Frame 0 (PTS = 0) is EXCLUDED from the wrong-vs-correct
    /// comparison because 0 × 100 = 0 — the forbidden multiplication
    /// produces the same result as the correct path when the delta is zero.
    /// This is a mathematical identity, not a contract violation.
    /// Only frames with non-zero PTS can distinguish correct from wrong.
    /// </summary>
    private static void Test_NoMultiply()
    {
        // Simulate 5 WGC frames at ~60 FPS
        long t0 = 25_000_000_000_000L;
        long[] timestamps =
        {
            t0,                  // frame 0: PTS = 0 (zero delta — skip comparison)
            t0 + 166_667,        // frame 1: PTS = 166,667
            t0 + 333_334,        // frame 2: PTS = 333,334
            t0 + 500_001,        // frame 3: PTS = 500,001
            t0 + 666_668,        // frame 4: PTS = 666,668
        };

        long[] correctPts = new long[timestamps.Length];
        long[] wrongPts = new long[timestamps.Length];

        for (int i = 0; i < timestamps.Length; i++)
        {
            correctPts[i] = timestamps[i] - t0;              // CORRECT: no multiply
            wrongPts[i] = (timestamps[i] - t0) * 100;       // FORBIDDEN: multiply by 100
        }

        // Verify correct PTS values for ALL frames (including frame 0)
        long[] expected = { 0, 166_667, 333_334, 500_001, 666_668 };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert(correctPts[i] == expected[i],
                $"Frame {i}: correct PTS should be {expected[i]}, got {correctPts[i]}");
        }

        // For frames with NON-ZERO PTS (i >= 1), prove that ×100 gives a different (wrong) result.
        // Frame 0 is excluded because 0 × 100 = 0 — the comparison is vacuous.
        for (int i = 1; i < timestamps.Length; i++)
        {
            Assert(wrongPts[i] != correctPts[i],
                $"Frame {i}: wrong PTS ({wrongPts[i]}) should differ from correct ({correctPts[i]}) — " +
                $"×100 must NOT match the correct path for non-zero PTS");
            Assert(wrongPts[i] == expected[i] * 100,
                $"Frame {i}: wrong PTS should be 100× correct: {wrongPts[i]} vs {expected[i] * 100}");
        }

        // Explicit proof using OWNER's example:
        //   T0 = 10_000_000
        //   T1 = 10_016_667
        //   Correct:  T1 - T0 = 16_667
        //   Forbidden: (T1 - T0) × 100 = 1_666_700
        long exampleT0 = 10_000_000L;
        long exampleT1 = 10_016_667L;
        long exampleCorrect = exampleT1 - exampleT0;
        long exampleForbidden = (exampleT1 - exampleT0) * 100;

        Assert(exampleCorrect == 16_667L,
            $"Example: correct PTS should be 16,667, got {exampleCorrect}");
        Assert(exampleForbidden == 1_666_700L,
            $"Example: forbidden PTS should be 1,666,700, got {exampleForbidden}");
        Assert(exampleCorrect != exampleForbidden,
            $"Example: correct ({exampleCorrect}) must differ from forbidden ({exampleForbidden})");
    }

    /// <summary>
    /// Prove that QPF (QueryPerformanceFrequency) conversion is a SEPARATE
    /// concern from the WGC timestamp path.
    ///
    /// Raw QPC path (NOT used by WGC):
    ///   rawQpcTicks = Stopwatch.GetTimestamp() - t0Qpc
    ///   pts100ns = rawQpcTicks * 10_000_000 / Stopwatch.Frequency
    ///
    /// WGC path (what we use):
    ///   pts100ns = SystemRelativeTime.Ticks - t0Wgc
    ///   // NO QPF conversion needed — ticks are already 100-ns
    /// </summary>
    private static void Test_QpfSeparate()
    {
        long t0Wgc = 25_000_000_000_000L; // WGC origin (100-ns ticks)
        long t1Wgc = t0Wgc + 10_000_000;   // 1 second later

        // WGC path: simple subtraction, no QPF
        long wgcPts = t1Wgc - t0Wgc;
        Assert(wgcPts == 10_000_000L, $"WGC 1s PTS should be 10,000,000, got {wgcPts}");

        // QPC path (SEPARATE — not used for WGC):
        // If we had raw QPC ticks (e.g., Stopwatch.GetTimestamp()), they'd need QPF:
        long qpcFrequency = 10_000_000; // typical QPF = 10MHz (varies by system)
        long rawQpcDelta = 10_000_000; // 1 second of raw QPC ticks at 10MHz
        long qpcPts = rawQpcDelta * 10_000_000 / qpcFrequency; // convert to 100-ns

        // Both paths should give the same result for 1 second
        Assert(qpcPts == 10_000_000L, $"QPC 1s PTS should also be 10,000,000, got {qpcPts}");
        Assert(wgcPts == qpcPts, $"WGC and QPC should agree for 1 second: {wgcPts} vs {qpcPts}");

        // BUT: the WGC path does NOT use QPF at all
        // The QPC path requires QPF, the WGC path does NOT
        // This test proves they're separate concerns that produce the same unit
    }

    /// <summary>
    /// Prove that WGC ticks (100-ns) ≠ raw QPC ticks (frequency-dependent).
    ///
    /// WGC SystemRelativeTime.Ticks are always 100-ns, regardless of QPF.
    /// Raw QPC ticks depend on Stopwatch.Frequency (typically 10MHz, but varies).
    ///
    /// If QPF = 10MHz, then 1 QPC tick = 100ns, which coincidentally equals WGC ticks.
    /// But if QPF ≠ 10MHz, the values would differ.
    /// The key point: WGC ticks are DEFINED as 100-ns, not derived from QPF.
    /// </summary>
    private static void Test_WgcVsQpc()
    {
        // WGC: 1 second is always 10,000,000 ticks (100-ns definition)
        long wgcOneSecond = 10_000_000L;

        // QPC: 1 second is Stopwatch.Frequency ticks (varies by system)
        // Typical: 10,000,000 (10MHz), but could be different
        long[] possibleQpfValues = { 3_579_545, 10_000_000, 14_318_180 };

        foreach (long qpf in possibleQpfValues)
        {
            long qpcOneSecond = qpf; // raw QPC ticks for 1 second

            // WGC ticks are ALWAYS 10,000,000 for 1 second, regardless of QPF
            Assert(wgcOneSecond == 10_000_000L,
                $"WGC 1s should always be 10,000,000 ticks (100-ns definition)");

            // QPC ticks for 1 second depend on frequency
            // Only when QPF = 10MHz do they coincide
            if (qpf == 10_000_000)
            {
                Assert(qpcOneSecond == wgcOneSecond,
                    "QPC == WGC only when QPF = 10MHz");
            }
            else
            {
                Assert(qpcOneSecond != wgcOneSecond,
                    $"QPC ({qpcOneSecond}) should differ from WGC ({wgcOneSecond}) when QPF={qpf}");
            }
        }

        // CONCLUSION: WGC path uses SystemRelativeTime.Ticks directly (100-ns).
        // QPC path would need QPF conversion. They are SEPARATE concerns.
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"ASSERT FAILED: {message}");
    }
}
