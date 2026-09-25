// TimestampAnalyzer.cs — pure stats functions, no I/O
// SPDX-License-Identifier: MIT
//
// Percentile method: nearest-rank
//   p95 = sorted[(int)Math.Ceiling(0.95 * count) - 1]  (clamped to [0, count-1])
//   p99 = sorted[(int)Math.Ceiling(0.99 * count) - 1]  (clamped to [0, count-1])
//   median = sorted[count / 2]
//
// This is the "nearest-rank" method — NOT linear interpolation.
// Documented here for reproducibility.

using System.Globalization;

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal static class TimestampAnalyzer
{
    /// <summary>
    /// Computes all stats from raw frame records.
    /// The result is a partial SessionResult — caller must set SessionIndex, DisplayConfig, LoadCondition, WallElapsedSeconds, AchievedFps.
    /// </summary>
    public static SessionResult ComputeStats(List<FrameRecord> frames, int sessionIndex)
    {
        var r = new SessionResult
        {
            SessionIndex = sessionIndex,
            FrameCount = frames.Count,
            EqualTimestampEvents = new(),
            RegressionEvents = new(),
            FirstSrt = frames.Count > 0 ? frames[0].SystemRelativeTimeTicks : 0,
            LastSrt = frames.Count > 0 ? frames[^1].SystemRelativeTimeTicks : 0,
            FirstPts = frames.Count > 0 ? frames[0].Pts : 0,
            LastPts = frames.Count > 0 ? frames[^1].Pts : 0,
            MinDelta = long.MaxValue,
            MaxDelta = long.MinValue,
            AverageDelta = 0,
            MedianDelta = 0,
            P95Delta = 0,
            P99Delta = 0,
            EqualTimestampCount = 0,
            NegativeDeltaCount = 0,
            NegativePtsCount = 0,
            TimestampMonotonic = true,
            PtsMonotonic = true,
        };

        if (frames.Count == 0) return r;

        // Collect deltas (skip first frame which has sentinel)
        var deltas = new List<long>(frames.Count - 1);
        for (int i = 1; i < frames.Count; i++)
        {
            long d = frames[i].DeltaFromPreviousSrtTicks;
            if (d == long.MinValue) continue;
            deltas.Add(d);
        }

        if (deltas.Count == 0) return r;

        // Basic stats
        r.MinDelta = deltas.Min();
        r.MaxDelta = deltas.Max();
        r.AverageDelta = deltas.Average();

        // Sorted copy for percentiles (nearest-rank)
        var sorted = deltas.OrderBy(d => d).ToList();
        r.MedianDelta = sorted[sorted.Count / 2];
        r.P95Delta = sorted[Math.Clamp((int)Math.Ceiling(0.95 * sorted.Count) - 1, 0, sorted.Count - 1)];
        r.P99Delta = sorted[Math.Clamp((int)Math.Ceiling(0.99 * sorted.Count) - 1, 0, sorted.Count - 1)];

        // Equal timestamps + regressions
        for (int i = 1; i < frames.Count; i++)
        {
            long prev = frames[i - 1].SystemRelativeTimeTicks;
            long curr = frames[i].SystemRelativeTimeTicks;
            long delta = frames[i].DeltaFromPreviousSrtTicks;

            if (delta == long.MinValue) continue;

            if (curr == prev)
            {
                r.EqualTimestampCount++;
                r.EqualTimestampEvents.Add((i - 1, i, curr));
            }
            if (curr < prev)
            {
                r.NegativeDeltaCount++;
                r.RegressionEvents.Add((i - 1, i, prev, curr, delta));
                r.TimestampMonotonic = false;
            }
            if (frames[i].Pts < frames[i - 1].Pts)
            {
                r.PtsMonotonic = false;
            }
            if (frames[i].Pts < 0)
            {
                r.NegativePtsCount++;
            }
        }

        return r;
    }

    /// <summary>
    /// Returns the 10 largest individual deltas with their frame indices.
    /// </summary>
    public static List<(long frameIdx, long delta)> GetTopGaps(List<FrameRecord> frames, int topN = 10)
    {
        var gaps = new List<(long frameIdx, long delta)>();
        for (int i = 1; i < frames.Count; i++)
        {
            long d = frames[i].DeltaFromPreviousSrtTicks;
            if (d == long.MinValue) continue;
            gaps.Add((i, d));
        }
        return gaps.OrderByDescending(g => g.delta).Take(topN).ToList();
    }
}
