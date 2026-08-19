// Tests/MonotonicTest.cs
//
// P1-B.2-V2 Test V2-2 — Monotonic Timestamp Test
//
// Goal: Prove timestamps never go backward.
//
// Run: 100,000 samples of Stopwatch.GetTimestamp()
// Track: backward jumps, minimum delta, maximum delta
//
// Acceptance: Backward jumps == 0
//
// Notes:
//   - Stopwatch.GetTimestamp() is QPC-backed; on Windows QPC is monotonic
//     per the Windows API contract (QueryPerformanceCounter never goes
//     backward on a single core, and on multi-core systems Windows
//     synchronizes QPC across cores).
//   - If this test FAILs with backward jumps > 0, it indicates either:
//       (a) Hardware/driver bug (rare — has happened on some AMD systems)
//       (b) Windows version with QPC synchronization issue (extremely rare)
//       (c) The test process was migrated across NUMA nodes with
//           unsynchronized TSC (very rare on modern hardware)
//   - Forward jumps are expected (time passes between samples).
//   - Zero delta is also possible if the timer resolution is coarser than
//     the loop iteration time. We track these separately.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;

namespace CaptureEngine.Timestamp.Spike.Tests;

public static class MonotonicTest
{
    private const int SampleCount = 100_000;

    public static TestResult Run()
    {
        var result = new TestResult
        {
            TestId = "V2-2",
            TestName = "Monotonic Timestamp Test",
            SampleCount = SampleCount,
        };

        Console.WriteLine("============================================================");
        Console.WriteLine(" V2-2 — MONOTONIC TIMESTAMP TEST");
        Console.WriteLine("============================================================");
        Console.WriteLine();
        Console.WriteLine($"Samples: {SampleCount}");
        Console.WriteLine();

        long[] timestamps = new long[SampleCount];

        // Step 1: Collect 100,000 samples in a tight loop.
        // We use a tight loop (no Thread.Sleep) to maximize the chance of
        // catching backward jumps if any exist.
        for (int i = 0; i < SampleCount; i++)
        {
            timestamps[i] = Stopwatch.GetTimestamp();
        }

        // Step 2: Analyze
        int backwardJumps = 0;
        int zeroDeltas = 0;
        long minDelta = long.MaxValue;
        long maxDelta = long.MinValue;
        long sumDeltas = 0;
        int positiveDeltaCount = 0;

        for (int i = 1; i < SampleCount; i++)
        {
            long current = timestamps[i];
            long previous = timestamps[i - 1];
            long delta = current - previous;

            if (delta < 0)
            {
                backwardJumps++;
            }
            else if (delta == 0)
            {
                zeroDeltas++;
            }
            else
            {
                if (delta < minDelta) minDelta = delta;
                if (delta > maxDelta) maxDelta = delta;
                sumDeltas += delta;
                positiveDeltaCount++;
            }
        }

        // Edge case: if all deltas were zero or negative, min/max are still sentinel
        if (positiveDeltaCount == 0)
        {
            minDelta = 0;
            maxDelta = 0;
        }

        double averageDelta = positiveDeltaCount > 0
            ? (double)sumDeltas / positiveDeltaCount
            : 0.0;

        result.BackwardJumps = backwardJumps;
        result.ZeroDeltas = zeroDeltas;
        result.MinDelta = minDelta;
        result.MaxDelta = maxDelta;
        result.AverageDelta = averageDelta;
        result.Pass = backwardJumps == 0;

        // Step 3: Report
        Console.WriteLine($"Backward jumps: {backwardJumps}");
        Console.WriteLine($"Zero deltas:    {zeroDeltas} (timer resolution coarser than loop iteration)");
        Console.WriteLine($"Minimum delta:  {minDelta} ticks ({(double)minDelta / Stopwatch.Frequency * 1000.0:F6} ms)");
        Console.WriteLine($"Maximum delta:  {maxDelta} ticks ({(double)maxDelta / Stopwatch.Frequency * 1000.0:F6} ms)");
        Console.WriteLine($"Average delta:  {averageDelta:F2} ticks ({averageDelta / Stopwatch.Frequency * 1000.0:F6} ms)");
        Console.WriteLine();
        Console.WriteLine($"Acceptance: Backward jumps == 0 : {(result.Pass ? "PASS" : "FAIL")} ({backwardJumps} observed)");
        Console.WriteLine();
        Console.WriteLine($"V2-2 Result: {(result.Pass ? "PASS" : "FAIL")}");
        Console.WriteLine();

        if (backwardJumps > 0)
        {
            Console.WriteLine("  WARNING: Backward jumps detected. This is a hardware/driver anomaly.");
            Console.WriteLine("  Possible causes:");
            Console.WriteLine("    - AMD processor with unsynchronized TSC across cores");
            Console.WriteLine("    - NUMA node migration (process moved between nodes)");
            Console.WriteLine("    - Windows version with QPC sync issue (extremely rare)");
            Console.WriteLine("  Action: Re-run on a different machine to isolate.");
            Console.WriteLine();
        }

        return result;
    }

    public sealed class TestResult
    {
        public string TestId { get; set; } = "";
        public string TestName { get; set; } = "";
        public int SampleCount { get; set; }
        public int BackwardJumps { get; set; }
        public int ZeroDeltas { get; set; }
        public long MinDelta { get; set; }
        public long MaxDelta { get; set; }
        public double AverageDelta { get; set; }
        public bool Pass { get; set; }
    }
}
