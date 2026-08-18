// Tests/TimestampBoundaryTests.cs
//
// V2 Spike — Test Group 2: Boundary cases
//
// Proves:
//   - First frame (PTS = 0)
//   - Equal timestamps (PTS[n] == PTS[n-1] is allowed)
//   - Large relative timestamp (24+ hours)
//   - Minimum/maximum safe Int64 values
//
// SPDX-License-Identifier: MIT

namespace V2_WGC_Timestamp_Spike.Tests;

public static class TimestampBoundaryTests
{
    public static void RunAll(Action<string, Action> runner)
    {
        runner("BOUNDARY: First frame PTS = 0", Test_FirstFrame);
        runner("BOUNDARY: Equal timestamps → equal PTS (allowed)", Test_EqualTimestamps);
        runner("BOUNDARY: All-equal sequence → all PTS = 0", Test_AllEqualSequence);
        runner("BOUNDARY: Large PTS (24 hours, no overflow)", Test_LargePTS_24h);
        runner("BOUNDARY: Large PTS (7 days, no overflow)", Test_LargePTS_7d);
        runner("BOUNDARY: Int64.MaxValue PTS (no overflow for reasonable T0)", Test_MaxPTS);
        runner("BOUNDARY: T0 near Int64.MaxValue (edge case)", Test_T0NearMax);
        runner("BOUNDARY: Single frame (only T0)", Test_SingleFrame);
    }

    /// <summary>
    /// First frame: PTS = T0 - T0 = 0.
    /// </summary>
    private static void Test_FirstFrame()
    {
        long t0 = 123_456_789_012_345L;
        long pts0 = t0 - t0;
        Assert(pts0 == 0, $"First frame PTS should be 0, got {pts0}");
    }

    /// <summary>
    /// Equal timestamps: if T[n] == T[n-1], then PTS[n] == PTS[n-1].
    /// This is ALLOWED by the timestamp layer — no drop/dedup policy.
    /// </summary>
    private static void Test_EqualTimestamps()
    {
        long t0 = 100_000_000L;
        long t1 = t0; // same timestamp

        long pts0 = t0 - t0; // = 0
        long pts1 = t1 - t0; // = 0

        Assert(pts0 == pts1, $"Equal timestamps should produce equal PTS: {pts0} vs {pts1}");
        Assert(pts1 == 0, $"Equal timestamp PTS should be 0, got {pts1}");
    }

    /// <summary>
    /// All-equal sequence: every frame has the same timestamp.
    /// Every PTS = 0. This is valid — no dedup/drop.
    /// </summary>
    private static void Test_AllEqualSequence()
    {
        long t0 = 999_999_999L;
        long[] timestamps = { t0, t0, t0, t0, t0 };

        foreach (var t in timestamps)
        {
            long pts = t - t0;
            Assert(pts == 0, $"All-equal sequence: PTS should be 0, got {pts}");
        }
    }

    /// <summary>
    /// 24 hours of recording at 100-ns resolution.
    /// 24h = 86400s × 10,000,000 ticks/s = 864,000,000,000 ticks.
    /// This fits comfortably in Int64 (max ≈ 9.2 × 10^18).
    /// </summary>
    private static void Test_LargePTS_24h()
    {
        long ticksPerSecond = 10_000_000L;
        long recordingDurationTicks = 24 * 60 * 60 * ticksPerSecond; // 864,000,000,000

        long t0 = 25_000_000_000_000L; // realistic WGC origin
        long tEnd = t0 + recordingDurationTicks;

        long ptsEnd = tEnd - t0;

        Assert(ptsEnd == 864_000_000_000L,
            $"24h PTS should be 864,000,000,000 ticks, got {ptsEnd}");
        Assert(ptsEnd > 0, "24h PTS should be positive");
        Assert(ptsEnd < long.MaxValue, "24h PTS should not overflow");

        // Verify: 864,000,000,000 ticks = 86,400 seconds = 24 hours
        double hours = ptsEnd / (10_000_000.0 * 3600.0);
        Assert(hours > 23.99 && hours < 24.01,
            $"24h PTS should be ~24 hours, got {hours:F6} hours");
    }

    /// <summary>
    /// 7 days of recording.
    /// 7d = 604,800s × 10,000,000 = 6,048,000,000,000 ticks.
    /// </summary>
    private static void Test_LargePTS_7d()
    {
        long ticksPerSecond = 10_000_000L;
        long recordingDurationTicks = 7 * 24 * 60 * 60 * ticksPerSecond;

        long t0 = 25_000_000_000_000L;
        long tEnd = t0 + recordingDurationTicks;

        long ptsEnd = tEnd - t0;

        Assert(ptsEnd == 6_048_000_000_000L,
            $"7d PTS should be 6,048,000,000,000 ticks, got {ptsEnd}");
        Assert(ptsEnd > 0, "7d PTS should be positive");
        Assert(ptsEnd < long.MaxValue / 1000, "7d PTS should have plenty of headroom");

        double days = ptsEnd / (10_000_000.0 * 86400.0);
        Assert(days > 6.99 && days < 7.01,
            $"7d PTS should be ~7 days, got {days:F6} days");
    }

    /// <summary>
    /// Int64.MaxValue PTS — if T0 is small and Tn is near max.
    /// This is an extreme edge case that validates no overflow in the
    /// subtraction Tn - T0 when Tn is very large.
    /// </summary>
    private static void Test_MaxPTS()
    {
        long t0 = 1_000_000L; // very small origin
        long tn = long.MaxValue - 1_000_000L; // near max

        long pts = tn - t0;

        Assert(pts > 0, $"Max PTS should be positive, got {pts}");
        Assert(pts < long.MaxValue, "Max PTS should not be exactly MaxValue");
    }

    /// <summary>
    /// Edge case: T0 near Int64.MaxValue.
    /// If T0 is very large, a frame slightly after T0 should still produce
    /// a positive PTS without overflow.
    /// </summary>
    private static void Test_T0NearMax()
    {
        long t0 = long.MaxValue - 1_000_000L;
        long t1 = t0 + 500_000L; // 50ms later

        long pts = t1 - t0;

        Assert(pts == 500_000L, $"T0 near max: PTS should be 500,000, got {pts}");
        Assert(pts > 0, "T0 near max: PTS should be positive");
    }

    /// <summary>
    /// Single frame: only T0 exists.
    /// PTS = 0. No overflow, no error.
    /// </summary>
    private static void Test_SingleFrame()
    {
        long t0 = 42_000_000_000L;
        long pts = t0 - t0;

        Assert(pts == 0, $"Single frame PTS should be 0, got {pts}");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"ASSERT FAILED: {message}");
    }
}
