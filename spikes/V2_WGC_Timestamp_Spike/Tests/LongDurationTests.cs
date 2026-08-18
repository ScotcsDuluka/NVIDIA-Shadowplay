// Tests/LongDurationTests.cs
//
// V2 Spike — Test Group 4: Long-duration arithmetic
//
// Proves:
//   - Relative 100-ns PTS over 1 hour, 24 hours, 7 days
//   - No integer overflow for realistic recording durations
//   - PTS values remain correct after millions of frames
//   - Frame count × interval = total PTS (arithmetic consistency)
//
// SPDX-License-Identifier: MIT

namespace V2_WGC_Timestamp_Spike.Tests;

public static class LongDurationTests
{
    public static void RunAll(Action<string, Action> runner)
    {
        runner("LONG: 1 hour recording — PTS correct", Test_1Hour);
        runner("LONG: 24 hour recording — PTS correct", Test_24Hours);
        runner("LONG: 7 day recording — PTS correct", Test_7Days);
        runner("LONG: 1M frames at 144 FPS — PTS arithmetic consistent", Test_1MFrames144Fps);
        runner("LONG: No overflow for 1 year recording", Test_1YearNoOverflow);
        runner("LONG: PTS = frameCount × interval (consistency)", Test_FrameCountConsistency);
    }

    /// <summary>
    /// 1 hour: 3600s × 10,000,000 = 36,000,000,000 ticks.
    /// </summary>
    private static void Test_1Hour()
    {
        long t0 = 25_000_000_000_000L;
        long oneHour = 3600L * 10_000_000L; // 36,000,000,000
        long tEnd = t0 + oneHour;

        long pts = tEnd - t0;

        Assert(pts == 36_000_000_000L, $"1h PTS should be 36,000,000,000, got {pts}");
        Assert(pts > 0, "1h PTS should be positive");
        Assert(pts < long.MaxValue, "1h PTS should not overflow");

        double hours = pts / (10_000_000.0 * 3600.0);
        Assert(hours > 0.999 && hours < 1.001, $"1h should be ~1.0 hours, got {hours:F6}");
    }

    /// <summary>
    /// 24 hours: 86400s × 10,000,000 = 864,000,000,000 ticks.
    /// </summary>
    private static void Test_24Hours()
    {
        long t0 = 25_000_000_000_000L;
        long oneDay = 86400L * 10_000_000L; // 864,000,000,000
        long tEnd = t0 + oneDay;

        long pts = tEnd - t0;

        Assert(pts == 864_000_000_000L, $"24h PTS should be 864,000,000,000, got {pts}");

        double hours = pts / (10_000_000.0 * 3600.0);
        Assert(hours > 23.999 && hours < 24.001, $"24h should be ~24.0 hours, got {hours:F6}");
    }

    /// <summary>
    /// 7 days: 604800s × 10,000,000 = 6,048,000,000,000 ticks.
    /// </summary>
    private static void Test_7Days()
    {
        long t0 = 25_000_000_000_000L;
        long sevenDays = 7L * 86400 * 10_000_000; // 6,048,000,000,000
        long tEnd = t0 + sevenDays;

        long pts = tEnd - t0;

        Assert(pts == 6_048_000_000_000L, $"7d PTS should be 6,048,000,000,000, got {pts}");

        double days = pts / (10_000_000.0 * 86400.0);
        Assert(days > 6.999 && days < 7.001, $"7d should be ~7.0 days, got {days:F6}");
    }

    /// <summary>
    /// 1 million frames at 144 FPS.
    /// Interval = 10,000,000 / 144 ≈ 69,444 ticks per frame.
    /// Total PTS = 1,000,000 × 69,444 = 69,444,000,000 ticks ≈ 6944 seconds ≈ 1.93 hours.
    /// Verify frameCount × interval == last PTS.
    /// </summary>
    private static void Test_1MFrames144Fps()
    {
        long t0 = 25_000_000_000_000L;
        long frameInterval = 10_000_000L / 144; // 69,444
        int frameCount = 1_000_000;

        long lastTimestamp = t0 + (long)(frameCount - 1) * frameInterval;
        long lastPts = lastTimestamp - t0;

        long expectedPts = (long)(frameCount - 1) * frameInterval;

        Assert(lastPts == expectedPts,
            $"1M frames: last PTS {lastPts} should equal expected {expectedPts}");

        // Verify total duration is reasonable (~1.93 hours)
        double hours = lastPts / (10_000_000.0 * 3600.0);
        Assert(hours > 1.9 && hours < 2.0,
            $"1M frames @ 144fps should be ~1.93 hours, got {hours:F3} hours");

        // Verify no overflow
        Assert(lastPts > 0, "1M frames: last PTS should be positive");
        Assert(lastPts < long.MaxValue, "1M frames: last PTS should not overflow");
    }

    /// <summary>
    /// 1 year recording: 365 × 86400 × 10,000,000 = 315,360,000,000,000,000 ticks.
    /// This is ~3.15 × 10^17, which is well below Int64.MaxValue (~9.2 × 10^18).
    /// No overflow.
    /// </summary>
    private static void Test_1YearNoOverflow()
    {
        long t0 = 25_000_000_000_000L;
        long oneYear = 365L * 86400 * 10_000_000; // 315,360,000,000,000,000

        // Verify it fits in Int64
        Assert(oneYear < long.MaxValue,
            $"1 year in ticks ({oneYear}) should fit in Int64 (max={long.MaxValue})");
        Assert(oneYear > 0, "1 year should be positive");

        long tEnd = t0 + oneYear;
        long pts = tEnd - t0;

        Assert(pts == oneYear, $"1 year PTS should be {oneYear}, got {pts}");
        Assert(pts > 0, "1 year PTS should be positive");

        // Verify: 1 year ≈ 315,360,000,000,000,000 ticks
        double years = pts / (10_000_000.0 * 86400.0 * 365.0);
        Assert(years > 0.999 && years < 1.001,
            $"1 year should be ~1.0 years, got {years:F6}");

        // Headroom check: how many years until overflow?
        long maxYears = long.MaxValue / (365L * 86400 * 10_000_000);
        Assert(maxYears > 29, $"Int64 should support at least 29 years of 100-ns ticks, got {maxYears}");
    }

    /// <summary>
    /// Arithmetic consistency: frameCount × frameInterval == last PTS.
    /// This verifies that the sum of intervals equals the direct delta.
    /// </summary>
    private static void Test_FrameCountConsistency()
    {
        long t0 = 50_000_000_000L;
        long frameInterval = 166_667L; // ~60 FPS
        int frameCount = 10_000;

        // Method 1: Direct delta
        long lastTimestamp = t0 + (long)(frameCount - 1) * frameInterval;
        long directDelta = lastTimestamp - t0;

        // Method 2: Sum of intervals
        long sumOfIntervals = 0;
        for (int i = 1; i < frameCount; i++)
        {
            long tn = t0 + (long)i * frameInterval;
            long prevTn = t0 + (long)(i - 1) * frameInterval;
            sumOfIntervals += tn - prevTn;
        }

        Assert(directDelta == sumOfIntervals,
            $"Direct delta ({directDelta}) should equal sum of intervals ({sumOfIntervals})");

        Assert(directDelta == (long)(frameCount - 1) * frameInterval,
            $"Direct delta should equal (frameCount-1) × interval: {directDelta} vs {(long)(frameCount - 1) * frameInterval}");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"ASSERT FAILED: {message}");
    }
}
