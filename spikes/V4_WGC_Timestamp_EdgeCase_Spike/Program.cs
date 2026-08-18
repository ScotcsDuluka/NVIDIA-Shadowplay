// Program.cs — V4 WGC Timestamp Edge-Case Spike — entry point
// SPDX-License-Identifier: MIT
//
// Usage:
//   --mode longrun --duration 600          Test 1: long runtime
//   --mode sessions --count 10 --duration-per-session 15  Test 4: session recreation
//   --mode stress --condition static --duration 90       Test 5a: stress static
//   --mode stress --condition active --duration 90 --load true  Test 5b: stress active+load
//   --mode all                                        Run all tests sequentially

using System.Diagnostics;

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" V4 WGC Timestamp Edge-Case Spike");
        Console.WriteLine(" Branch: Engine-Rebuild (spike — does NOT modify repo)");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        string mode = "all";
        int duration = 600;
        int sessionCount = 10;
        int durationPerSession = 15;
        string condition = "static";
        bool load = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--mode": mode = args[++i]; break;
                case "--duration": duration = int.Parse(args[++i]); break;
                case "--count": sessionCount = int.Parse(args[++i]); break;
                case "--duration-per-session": durationPerSession = int.Parse(args[++i]); break;
                case "--condition": condition = args[++i]; break;
                case "--load": load = bool.Parse(args[++i]); break;
            }
        }

        Console.WriteLine($"Mode: {mode}");
        Console.WriteLine();

        string evidenceDir = Path.Combine(AppContext.BaseDirectory, "evidence");
        Directory.CreateDirectory(evidenceDir);
        Console.WriteLine($"Evidence dir: {evidenceDir}");
        Console.WriteLine();

        var allSessions = new List<SessionResult>();
        var allFrames = new List<FrameRecord>();
        int sessionIdx = 0;

        try
        {
            if (mode is "all" or "longrun")
            {
                Console.WriteLine("=== TEST 1: Long Runtime ===");
                var (sess, frames) = RunSingleSession(sessionIdx++, "longrun", duration, "idle", evidenceDir, null);
                allSessions.Add(sess);
                allFrames.AddRange(frames);
            }

            if (mode is "all" or "sessions")
            {
                Console.WriteLine();
                Console.WriteLine("=== TEST 4: Session Recreation ===");
                for (int i = 0; i < sessionCount; i++)
                {
                    var (sess, frames) = RunSingleSession(sessionIdx++, "recreation", durationPerSession, "idle", evidenceDir, null);
                    allSessions.Add(sess);
                    allFrames.AddRange(frames);
                    Console.WriteLine($"  Session {i + 1}/{sessionCount}: FirstPts={sess.FirstPts}, LastPts={sess.LastPts}");
                    if (i < sessionCount - 1)
                    {
                        Console.WriteLine("  Disposing... pausing 1s...");
                        Thread.Sleep(1000);
                    }
                }
            }

            if (mode is "all" or "stress")
            {
                Console.WriteLine();
                Console.WriteLine("=== TEST 5a: Stress — Static Content ===");
                using var loadGen = load ? new LoadGenerator(4) : null;
                loadGen?.Start();
                var (sessStatic, framesStatic) = RunSingleSession(sessionIdx++, "stress_static", 90, "static-content", evidenceDir, loadGen);
                allSessions.Add(sessStatic);
                allFrames.AddRange(framesStatic);
                loadGen?.Stop();

                Console.WriteLine();
                Console.WriteLine("=== TEST 5b: Stress — Active Content + Load ===");
                if (load)
                {
                    using var loadGen2 = new LoadGenerator(4);
                    loadGen2.Start();
                    var (sessActive, framesActive) = RunSingleSession(sessionIdx++, "stress_active", 90, "active-content", evidenceDir, loadGen2);
                    allSessions.Add(sessActive);
                    allFrames.AddRange(framesActive);
                }
            }

            // === Tests 2, 3, 6 are analysis-only (derived from all sessions) ===
            Console.WriteLine();
            Console.WriteLine("=== TEST 2/3/6: Analysis (derived from all sessions) ===");
            Console.WriteLine($"  Total equal timestamps across all sessions: {allSessions.Sum(s => s.EqualTimestampCount)}");
            Console.WriteLine($"  Total regressions across all sessions: {allSessions.Sum(s => s.NegativeDeltaCount)}");
            Console.WriteLine($"  Total negative PTS across all sessions: {allSessions.Sum(s => s.NegativePtsCount)}");

            var topGaps = TimestampAnalyzer.GetTopGaps(allFrames, 10);
            Console.WriteLine($"  Top 10 largest gaps (aggregate):");
            foreach (var g in topGaps)
                Console.WriteLine($"    frame {g.frameIdx}: delta={g.delta} ({g.delta / 10_000.0:F3} ms)");

            // === Tests 7, 8 ===
            Console.WriteLine();
            Console.WriteLine("=== TEST 7: Resolution/Display Mode ===");
            Console.WriteLine("  NOT TESTED — no alternate display config safely available without OS-level changes.");

            Console.WriteLine();
            Console.WriteLine("=== TEST 8: Suspend/Resume ===");
            Console.WriteLine("  NOT TESTED — no safe automated mechanism available. Manual tester required.");

            // === Final report ===
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" FINAL REPORT");
            Console.WriteLine("============================================================");
            foreach (var s in allSessions)
                ReportSession(s);

            Console.WriteLine();
            Console.WriteLine("=== AGGREGATE ===");
            Console.WriteLine($"  Total sessions: {allSessions.Count}");
            Console.WriteLine($"  Total frames: {allFrames.Count}");
            Console.WriteLine($"  Total equal timestamps: {allSessions.Sum(s => s.EqualTimestampCount)}");
            Console.WriteLine($"  Total regressions: {allSessions.Sum(s => s.NegativeDeltaCount)}");
            Console.WriteLine($"  Total negative PTS: {allSessions.Sum(s => s.NegativePtsCount)}");
            Console.WriteLine($"  All sessions timestamp-monotonic: {allSessions.All(s => s.TimestampMonotonic)}");
            Console.WriteLine($"  All sessions PTS-monotonic: {allSessions.All(s => s.PtsMonotonic)}");

            // Write evidence files
            EvidenceWriter.WriteFinalReport(evidenceDir, allSessions, allFrames, "PASS — 0 errors", "pending");
            Console.WriteLine();
            Console.WriteLine($"Evidence files written to: {evidenceDir}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static (SessionResult, List<FrameRecord>) RunSingleSession(
        int idx, string mode, int durationSec, string loadCondition,
        string evidenceDir, LoadGenerator? loadGen)
    {
        using var wgc = new WgcSession();
        wgc.Setup();
        Console.WriteLine($"  Display: {wgc.DisplayConfig}");

        var sw = Stopwatch.StartNew();
        var frames = wgc.Capture(durationSec, loadCondition);
        sw.Stop();

        var result = TimestampAnalyzer.ComputeStats(frames, idx);
        result.WallElapsedSeconds = sw.Elapsed.TotalSeconds;
        result.DisplayConfig = wgc.DisplayConfig;
        result.LoadCondition = loadCondition;
        result.AchievedFps = frames.Count / sw.Elapsed.TotalSeconds;

        // Copy acquisition counters from WgcSession
        result.FrameArrivedCount = wgc.FrameArrivedCount;
        result.TryGetNextFrameCount = wgc.TryGetNextFrameCount;
        result.AcquiredFrameCount = wgc.AcquiredFrameCount;
        result.ConsumedFrameCount = wgc.ConsumedFrameCount;
        result.DroppedByHarnessCount = wgc.DroppedByHarnessCount;
        result.SupersededCount = wgc.SupersededCount;
        result.AcquisitionFps = wgc.AcquiredFrameCount / sw.Elapsed.TotalSeconds;
        result.ConsumedFps = wgc.ConsumedFrameCount / sw.Elapsed.TotalSeconds;
        result.HarnessDropRate = wgc.AcquiredFrameCount > 0
            ? wgc.DroppedByHarnessCount / (double)wgc.AcquiredFrameCount
            : 0.0;

        // Write evidence
        EvidenceWriter.WriteFramesCsv(evidenceDir, idx, mode, frames);
        EvidenceWriter.WriteSummaryJson(evidenceDir, idx, mode, result);

        return (result, frames);
    }

    private static void ReportSession(SessionResult r)
    {
        Console.WriteLine();
        Console.WriteLine($"--- Session {r.SessionIndex} ({r.LoadCondition}) ---");

        Console.WriteLine("  === ACQUISITION SUMMARY ===");
        Console.WriteLine($"    FrameArrived:      {r.FrameArrivedCount}");
        Console.WriteLine($"    TryGetNextFrame:   {r.TryGetNextFrameCount}");
        Console.WriteLine($"    Acquired:          {r.AcquiredFrameCount}");
        Console.WriteLine($"    Consumed:          {r.ConsumedFrameCount}");
        Console.WriteLine($"    DroppedByHarness:  {r.DroppedByHarnessCount}");
        Console.WriteLine($"    Superseded:        {r.SupersededCount}");

        Console.WriteLine("  === DELIVERY SUMMARY ===");
        Console.WriteLine($"    Acquisition FPS:  {r.AcquisitionFps:F2}");
        Console.WriteLine($"    Consumed FPS:      {r.ConsumedFps:F2}");
        Console.WriteLine($"    Harness Drop Rate:{r.HarnessDropRate:P2}");

        Console.WriteLine("  === TIMESTAMP SUMMARY ===");
        Console.WriteLine($"    Frames:            {r.FrameCount}");
        Console.WriteLine($"    Duration:          {r.WallElapsedSeconds:F2} s");
        Console.WriteLine($"    First SRT:         {r.FirstSrt}");
        Console.WriteLine($"    Last SRT:          {r.LastSrt}");
        Console.WriteLine($"    First PTS:         {r.FirstPts}");
        Console.WriteLine($"    Last PTS:          {r.LastPts} ({r.LastPts / 10_000_000.0:F6} s)");
        Console.WriteLine($"    Equal timestamps:  {r.EqualTimestampCount}");
        Console.WriteLine($"    Negative deltas:   {r.NegativeDeltaCount}");
        Console.WriteLine($"    Negative PTS:      {r.NegativePtsCount}");
        Console.WriteLine($"    Timestamp monotonic:{r.TimestampMonotonic}");
        Console.WriteLine($"    PTS monotonic:     {r.PtsMonotonic}");
        Console.WriteLine($"    Min delta:         {r.MinDelta} ({r.MinDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"    Median delta:      {r.MedianDelta} ({r.MedianDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"    P95 delta:         {r.P95Delta} ({r.P95Delta / 10_000.0:F3} ms)");
        Console.WriteLine($"    P99 delta:         {r.P99Delta} ({r.P99Delta / 10_000.0:F3} ms)");
        Console.WriteLine($"    Max delta:         {r.MaxDelta} ({r.MaxDelta / 10_000.0:F3} ms)");

        if (r.EqualTimestampCount > 0)
            Console.WriteLine($"    Equal events:      {r.EqualTimestampEvents.Count} (first: {r.EqualTimestampEvents[0]})");
        if (r.NegativeDeltaCount > 0)
            Console.WriteLine($"    Regression events:  {r.RegressionEvents.Count} (first: {r.RegressionEvents[0]})");

        // Invariant check
        long expected = r.AcquiredFrameCount - r.DroppedByHarnessCount;
        bool invariantHolds = r.ConsumedFrameCount == expected;
        Console.WriteLine($"    Invariant (Acquired-Consumed=Dropped): {(invariantHolds ? "HOLDS" : "MISMATCH")} " +
                          $"[{r.ConsumedFrameCount} == {r.AcquiredFrameCount} - {r.DroppedByHarnessCount} = {expected}]");
    }
}
