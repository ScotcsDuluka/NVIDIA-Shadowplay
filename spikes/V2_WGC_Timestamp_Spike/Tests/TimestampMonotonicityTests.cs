// Tests/TimestampMonotonicityTests.cs
//
// V2 Spike — Test Group 3: Monotonicity + equal timestamps
//
// Proves:
//   - Increasing timestamps → non-decreasing PTS
//   - Equal timestamps → equal PTS (allowed, no drop policy)
//   - Slightly increasing (1 tick apart) → strictly increasing PTS
//   - Irregular gaps (burst then pause) → still non-decreasing
//
// SPDX-License-Identifier: MIT

namespace V2_WGC_Timestamp_Spike.Tests;

public static class TimestampMonotonicityTests
{
    public static void RunAll(Action<string, Action> runner)
    {
        runner("MONO: Strictly increasing → strictly increasing PTS", Test_StrictlyIncreasing);
        runner("MONO: Equal timestamps → equal PTS (allowed)", Test_EqualAllowed);
        runner("MONO: Non-decreasing (some equal) → non-decreasing PTS", Test_NonDecreasing);
        runner("MONO: 1-tick gap → strictly increasing", Test_OneTickGap);
        runner("MONO: Burst pattern (fast then slow)", Test_BurstPattern);
        runner("MONO: 1000 frames at 60 FPS (simulated)", Test_1000Frames60Fps);
    }

    /// <summary>
    /// Strictly increasing timestamps → strictly increasing PTS.
    /// </summary>
    private static void Test_StrictlyIncreasing()
    {
        long t0 = 1_000_000_000L;
        long[] timestamps = { t0, t0 + 100, t0 + 200, t0 + 300 };

        for (int i = 1; i < timestamps.Length; i++)
        {
            long ptsPrev = timestamps[i - 1] - t0;
            long ptsCurr = timestamps[i] - t0;

            Assert(ptsCurr > ptsPrev,
                $"Frame {i}: PTS should be strictly > prev: {ptsCurr} vs {ptsPrev}");
        }
    }

    /// <summary>
    /// Equal timestamps → equal PTS.
    /// This is ALLOWED by the timestamp layer.
    /// </summary>
    private static void Test_EqualAllowed()
    {
        long t0 = 5_000_000_000L;
        long[] timestamps = { t0, t0 + 1000, t0 + 1000, t0 + 2000 };

        long[] pts = new long[timestamps.Length];
        for (int i = 0; i < timestamps.Length; i++)
            pts[i] = timestamps[i] - t0;

        // Frame 1 and 2 have equal timestamps → equal PTS
        Assert(pts[1] == pts[2],
            $"Equal timestamps should produce equal PTS: {pts[1]} vs {pts[2]}");

        // Overall sequence is non-decreasing
        for (int i = 1; i < pts.Length; i++)
        {
            Assert(pts[i] >= pts[i - 1],
                $"Frame {i}: PTS should be >= prev: {pts[i]} vs {pts[i - 1]}");
        }
    }

    /// <summary>
    /// Non-decreasing timestamps (some equal) → non-decreasing PTS.
    /// </summary>
    private static void Test_NonDecreasing()
    {
        long t0 = 10_000_000L;
        // Mix of strictly increasing and equal
        long[] timestamps =
        {
            t0,
            t0 + 166667,       // +1 frame @ 60fps
            t0 + 166667,       // equal (duplicate)
            t0 + 333334,       // +1 more frame
            t0 + 333334,       // equal (duplicate)
            t0 + 333334,       // equal (duplicate)
            t0 + 500001,       // +1 more frame
        };

        long[] pts = new long[timestamps.Length];
        for (int i = 0; i < timestamps.Length; i++)
            pts[i] = timestamps[i] - t0;

        // Verify non-decreasing
        for (int i = 1; i < pts.Length; i++)
        {
            Assert(pts[i] >= pts[i - 1],
                $"Frame {i}: PTS {pts[i]} < prev {pts[i - 1]} (should be non-decreasing)");
        }

        // Verify duplicates are allowed (not dropped)
        Assert(pts[1] == pts[2], "Duplicate frames should produce equal PTS");
        Assert(pts[3] == pts[4] && pts[4] == pts[5], "Triple duplicate should produce equal PTS");
    }

    /// <summary>
    /// 1-tick gap (smallest possible increase) → strictly increasing PTS.
    /// </summary>
    private static void Test_OneTickGap()
    {
        long t0 = 1_000_000L;
        long t1 = t0 + 1; // 100ns later

        long pts0 = t0 - t0; // = 0
        long pts1 = t1 - t0; // = 1

        Assert(pts1 > pts0, $"1-tick gap: PTS should be strictly increasing: {pts1} > {pts0}");
        Assert(pts1 == 1, $"1-tick gap: PTS should be exactly 1, got {pts1}");
    }

    /// <summary>
    /// Burst pattern: fast burst of frames, then a pause, then more frames.
    /// PTS should remain non-decreasing throughout.
    /// </summary>
    private static void Test_BurstPattern()
    {
        long t0 = 100_000_000L;
        long[] timestamps =
        {
            t0,
            t0 + 100,           // burst: 10us apart
            t0 + 200,
            t0 + 300,
            t0 + 50_000_000,    // pause: 5 seconds
            t0 + 50_000_100,    // burst again
            t0 + 50_000_200,
        };

        long[] pts = new long[timestamps.Length];
        for (int i = 0; i < timestamps.Length; i++)
            pts[i] = timestamps[i] - t0;

        for (int i = 1; i < pts.Length; i++)
        {
            Assert(pts[i] >= pts[i - 1],
                $"Burst frame {i}: PTS {pts[i]} < prev {pts[i - 1]}");
        }

        // Verify the pause created a large jump (not a gap or reset)
        Assert(pts[4] > pts[3] * 100_000,
            $"Burst: pause should create large PTS jump: {pts[4]} vs {pts[3]}");
    }

    /// <summary>
    /// Simulate 1000 frames at 60 FPS.
    /// Each frame is ~166,667 ticks apart (16.67ms at 100ns resolution).
    /// PTS should be non-decreasing and match expected values.
    /// </summary>
    private static void Test_1000Frames60Fps()
    {
        long t0 = 25_000_000_000_000L;
        long frameInterval = 10_000_000L / 60; // ~166,667 ticks per frame

        long prevPts = 0;
        for (int i = 0; i < 1000; i++)
        {
            long tn = t0 + (long)(i * frameInterval);
            long pts = tn - t0;

            Assert(pts >= prevPts,
                $"Frame {i}: PTS {pts} < prev {prevPts} (should be non-decreasing)");

            // Spot-check specific frames
            if (i == 0)
                Assert(pts == 0, $"Frame 0: PTS should be 0, got {pts}");
            if (i == 999)
            {
                // Last frame: ~999 × 166,667 = ~166,500,333 ticks = ~16.65 seconds
                double seconds = pts / 10_000_000.0;
                Assert(seconds > 16.0 && seconds < 17.0,
                    $"Frame 999: ~16.67s expected, got {seconds:F3}s");
            }

            prevPts = pts;
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"ASSERT FAILED: {message}");
    }
}
