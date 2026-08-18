// EvidenceWriter.cs — writes raw + aggregated evidence to disk
// SPDX-License-Identifier: MIT
// Append-only — never overwrite a prior session's file mid-run.
// No frame is ever dropped from the CSV, even if flagged as anomalous.

using System.Globalization;
using System.Text.Json;

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal static class EvidenceWriter
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    /// <summary>
    /// Writes every frame, raw, unfiltered, to a per-session CSV.
    /// </summary>
    public static void WriteFramesCsv(string dir, int sessionIndex, string mode, List<FrameRecord> frames)
    {
        string path = Path.Combine(dir, $"v4_session_{mode}_{sessionIndex}_frames.csv");
        using var sw = new StreamWriter(path, append: false);
        sw.WriteLine("frameIndex,systemRelativeTimeTicks,deltaFromPreviousSrtTicks,pts,deltaFromPreviousPtsTicks,wallClockUtcCaptured");
        foreach (var f in frames)
        {
            string deltaSrt = f.DeltaFromPreviousSrtTicks == long.MinValue ? "FIRST" : f.DeltaFromPreviousSrtTicks.ToString(CultureInfo.InvariantCulture);
            string deltaPts = f.DeltaFromPreviousPtsTicks == long.MinValue ? "FIRST" : f.DeltaFromPreviousPtsTicks.ToString(CultureInfo.InvariantCulture);
            sw.WriteLine($"{f.FrameIndex},{f.SystemRelativeTimeTicks},{deltaSrt},{f.Pts},{deltaPts},{f.WallClockUtcCaptured:O}");
        }
    }

    /// <summary>
    /// Writes per-session summary to JSON.
    /// </summary>
    public static void WriteSummaryJson(string dir, int sessionIndex, string mode, SessionResult r)
    {
        string path = Path.Combine(dir, $"v4_session_{mode}_{sessionIndex}_summary.json");
        var json = new
        {
            sessionIndex = r.SessionIndex,
            mode,
            frameCount = r.FrameCount,
            wallElapsedSeconds = r.WallElapsedSeconds,
            firstSrt = r.FirstSrt,
            lastSrt = r.LastSrt,
            firstPts = r.FirstPts,
            lastPts = r.LastPts,
            lastPtsSeconds = r.LastPts / 10_000_000.0,
            minDelta = r.MinDelta,
            maxDelta = r.MaxDelta,
            averageDelta = r.AverageDelta,
            medianDelta = r.MedianDelta,
            p95Delta = r.P95Delta,
            p99Delta = r.P99Delta,
            equalTimestampCount = r.EqualTimestampCount,
            equalTimestampEvents = r.EqualTimestampEvents.Select(e => new { prevIdx = e.prevIdx, currIdx = e.currIdx, srt = e.srt }),
            negativeDeltaCount = r.NegativeDeltaCount,
            regressionEvents = r.RegressionEvents.Select(e => new { prevIdx = e.prevIdx, currIdx = e.currIdx, prevSrt = e.prevSrt, currSrt = e.currSrt, delta = e.delta }),
            negativePtsCount = r.NegativePtsCount,
            timestampMonotonic = r.TimestampMonotonic,
            ptsMonotonic = r.PtsMonotonic,
            displayConfig = r.DisplayConfig,
            loadCondition = r.LoadCondition,
            achievedFps = r.AchievedFps,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(json, JsonOpts));
    }

    /// <summary>
    /// Writes a final rollup JSON matching the required output schema.
    /// </summary>
    public static void WriteFinalReport(string dir, List<SessionResult> allSessions, List<FrameRecord> allFrames, string buildResult, string commitSha)
    {
        string path = Path.Combine(dir, "v4_final_report.json");

        var topGapsAll = TimestampAnalyzer.GetTopGaps(allFrames, 10);

        var report = new
        {
            build = buildResult,
            commit = commitSha,
            sessions = allSessions.Select(r => new
            {
                sessionIndex = r.SessionIndex,
                mode = r.LoadCondition,
                frameCount = r.FrameCount,
                wallElapsedSeconds = r.WallElapsedSeconds,
                firstSrt = r.FirstSrt,
                lastSrt = r.LastSrt,
                firstPts = r.FirstPts,
                lastPts = r.LastPts,
                minDelta = r.MinDelta,
                maxDelta = r.MaxDelta,
                averageDelta = r.AverageDelta,
                medianDelta = r.MedianDelta,
                p95Delta = r.P95Delta,
                p99Delta = r.P99Delta,
                equalTimestampCount = r.EqualTimestampCount,
                negativeDeltaCount = r.NegativeDeltaCount,
                negativePtsCount = r.NegativePtsCount,
                timestampMonotonic = r.TimestampMonotonic,
                ptsMonotonic = r.PtsMonotonic,
                achievedFps = r.AchievedFps,
            }),
            aggregateTopGaps = topGapsAll.Select(g => new { frameIdx = g.frameIdx, delta = g.delta, deltaMs = g.delta / 10_000.0 }),
            totalEqualTimestamps = allSessions.Sum(s => s.EqualTimestampCount),
            totalRegressions = allSessions.Sum(s => s.NegativeDeltaCount),
            totalNegativePts = allSessions.Sum(s => s.NegativePtsCount),
            remainingUnknowns = new[]
            {
                "exact cause of duplicate timestamps",
                "long-term behavior beyond spike duration",
                "suspend/resume behavior if not tested",
                "multi-monitor/multi-adapter behavior if not tested",
                "behavior on GPU vendors other than NVIDIA",
                "behavior on GPU generations other than Pascal",
                "statistical confidence on rare events (absence of observation is not proof of absence)",
            },
        };
        File.WriteAllText(path, JsonSerializer.Serialize(report, JsonOpts));
    }
}
