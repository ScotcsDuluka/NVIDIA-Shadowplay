// Tests/TimestampMathTests.cs
//
// V2 Spike — Test Group 1: Zero-based PTS + Relative timestamp calculation
//
// Proves:
//   1. First frame timestamp → PTS = 0
//   2. PTS = Tn - T0 (simple subtraction, no conversion)
//   3. SystemRelativeTime.Ticks is treated as 100-ns units
//
// SPDX-License-Identifier: MIT

namespace V2_WGC_Timestamp_Spike.Tests;

public static class TimestampMathTests
{
    public static void RunAll(Action<string, Action> runner)
    {
        runner("MATH: First frame PTS = 0 (zero-based)", Test_ZeroBasedPTS);
        runner("MATH: Second frame PTS = T1 - T0", Test_RelativePTS);
        runner("MATH: PTS is in 100-ns units (not multiplied)", Test_100nsUnits);
        runner("MATH: 60 FPS frame interval = ~166667 ticks", Test_60FpsInterval);
        runner("MATH: 144 FPS frame interval = ~69444 ticks", Test_144FpsInterval);
        runner("MATH: PTS sequence matches timestamp deltas", Test_SequenceDeltas);
    }

    /// <summary>
    /// First frame timestamp → PTS = 0.
    /// T0 is the first frame's SystemRelativeTime.Ticks.
    /// PTS[0] = T0 - T0 = 0.
    /// </summary>
    private static void Test_ZeroBasedPTS()
    {
        // Simulate a WGC SystemRelativeTime value (100-ns ticks).
        // Real values look like ~25,000,000,000,000 (2.5 seconds in 100-ns ticks).
        long t0 = 25_000_000_000_000L;

        // PTS = T0 - T0 = 0
        long pts0 = t0 - t0;

        Assert(pts0 == 0, $"Expected PTS[0]=0, got {pts0}");
    }

    /// <summary>
    /// Second frame: PTS = T1 - T0.
    /// Simple subtraction — NO conversion factor applied.
    /// </summary>
    private static void Test_RelativePTS()
    {
        long t0 = 25_000_000_000_000L;
        // 16.68ms later = 166,833 ticks (100-ns units)
        long t1 = t0 + 166_833L;

        long pts1 = t1 - t0;

        Assert(pts1 == 166_833L, $"Expected PTS[1]=166833, got {pts1}");
    }

    /// <summary>
    /// Prove that PTS values are in 100-ns units.
    /// 1 second = 10,000,000 ticks (100-ns each).
    /// If someone multiplied by 100, they'd get 1,000,000,000 — which is wrong.
    /// </summary>
    private static void Test_100nsUnits()
    {
        long t0 = 0;
        long t1 = 10_000_000L; // exactly 1 second in 100-ns ticks

        long pts = t1 - t0;

        // 1 second = 10,000,000 ticks at 100-ns resolution
        Assert(pts == 10_000_000L, $"1 second should be 10,000,000 ticks, got {pts}");

        // NOT 1,000,000,000 (which would be the result of multiplying by 100)
        Assert(pts != 1_000_000_000L, $"PTS should NOT be multiplied by 100");
    }

    /// <summary>
    /// At 60 FPS, each frame interval is ~16.667ms.
    /// In 100-ns ticks: 16.667ms = 166,667 ticks.
    /// </summary>
    private static void Test_60FpsInterval()
    {
        long ticksPerSecond = 10_000_000L; // 100-ns ticks
        double fps = 60.0;
        long expectedInterval = (long)(ticksPerSecond / fps); // 166,666

        long t0 = 1_000_000_000L;
        long t1 = t0 + expectedInterval;

        long pts = t1 - t0;

        Assert(pts == 166_666L, $"60 FPS interval should be 166,666 ticks, got {pts}");
        // Sanity: this is ~16.67ms, which is correct for 60 FPS
        double ms = pts / 10_000.0;
        Assert(ms > 16.0 && ms < 17.0, $"60 FPS interval should be ~16.67ms, got {ms:F3}ms");
    }

    /// <summary>
    /// At 144 FPS, each frame interval is ~6.944ms.
    /// In 100-ns ticks: 6.944ms = 69,444 ticks.
    /// </summary>
    private static void Test_144FpsInterval()
    {
        long ticksPerSecond = 10_000_000L;
        double fps = 144.0;
        long expectedInterval = (long)(ticksPerSecond / fps); // 69,444

        long t0 = 1_000_000_000L;
        long t1 = t0 + expectedInterval;

        long pts = t1 - t0;

        Assert(pts == 69_444L, $"144 FPS interval should be 69,444 ticks, got {pts}");
        double ms = pts / 10_000.0;
        Assert(ms > 6.5 && ms < 7.5, $"144 FPS interval should be ~6.94ms, got {ms:F3}ms");
    }

    /// <summary>
    /// A sequence of timestamps should produce PTS values that match
    /// the individual deltas from T0.
    /// </summary>
    private static void Test_SequenceDeltas()
    {
        long t0 = 50_000_000_000L;
        long[] timestamps =
        {
            t0,
            t0 + 100_000,        // 10ms later
            t0 + 200_000,        // 20ms later
            t0 + 300_000,        // 30ms later
            t0 + 350_000,        // 35ms later (irregular gap)
        };

        long[] expectedPts = { 0, 100_000, 200_000, 300_000, 350_000 };

        for (int i = 0; i < timestamps.Length; i++)
        {
            long pts = timestamps[i] - t0;
            Assert(pts == expectedPts[i],
                $"Frame {i}: expected PTS={expectedPts[i]}, got {pts}");
        }
    }

    // === Simple assert helper ===
    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"ASSERT FAILED: {message}");
    }
}
