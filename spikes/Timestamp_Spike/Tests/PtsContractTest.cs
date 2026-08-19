// Tests/PtsContractTest.cs
//
// P1-B.2-V2 Test V2-4 — Engine PTS Contract
//
// Goal: Define the Engine's central timestamp format and validate the PTS
// (Presentation Time Stamp) contract for a 60 FPS frame sequence.
//
// Decision:
//   Engine Internal Timestamp: QPC ticks (Stopwatch ticks)
//   PTS = qpcTimestamp - T0   (T0 = first frame's QPC timestamp)
//   PTS is in QPC ticks (same units as Stopwatch.Frequency)
//
// Test approach:
//   1. Generate 1000 frames at 60 FPS (simulated, not real-time captured).
//   2. Each frame's QPC timestamp = T0 + i * (Stopwatch.Frequency / 60)
//      (one frame's worth of QPC ticks past the previous frame).
//   3. PTS[i] = qpcTimestamp[i] - T0 = i * (Stopwatch.Frequency / 60)
//   4. Verify:
//        - Frame 0 PTS == 0
//        - Frame 1 PTS ~= 166,666 (16.67ms at 10 MHz QPC)
//        - Frame 2 PTS ~= 333,333
//        - Average delta ~= Stopwatch.Frequency / 60
//        - Jitter < 1% (max-min delta / average delta < 0.01)
//
// Acceptance:
//   - Frame 0 has PTS == 0
//   - Average delta ~= Stopwatch.Frequency / FPS (within 1 tick)
//   - Jitter < 1%
//
// Forbidden:
//   - Do NOT use DateTime.UtcNow as clock — not monotonic, not high-resolution.
//   - Do NOT use TimeSpan.Ticks as Engine PTS — Engine PTS is QPC ticks.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;
using CaptureEngine.Timestamp.Spike.Utils;

namespace CaptureEngine.Timestamp.Spike.Tests;

public static class PtsContractTest
{
    private const int Fps = 60;
    private const int FrameCount = 1000;

    // Acceptance: jitter < 1% (max-min delta / average delta)
    private const double JitterThreshold = 0.01;

    public static TestResult Run()
    {
        var result = new TestResult
        {
            TestId = "V2-4",
            TestName = "Engine PTS Contract",
            Fps = Fps,
            FrameCount = FrameCount,
        };

        Console.WriteLine("============================================================");
        Console.WriteLine(" V2-4 — ENGINE PTS CONTRACT TEST");
        Console.WriteLine("============================================================");
        Console.WriteLine();
        Console.WriteLine($"Engine Internal Timestamp: QPC ticks (Stopwatch ticks)");
        Console.WriteLine($"PTS formula:               pts = qpcTimestamp - T0");
        Console.WriteLine($"FPS:                       {Fps}");
        Console.WriteLine($"Frame count:               {FrameCount}");
        Console.WriteLine($"Stopwatch.Frequency:       {Stopwatch.Frequency} Hz");
        Console.WriteLine();

        // Step 1: Simulate frame QPC timestamps
        // T0 = some arbitrary reference (we use a real GetTimestamp() for realism,
        // then generate synthetic frame timestamps after it).
        long t0 = Stopwatch.GetTimestamp();
        long expectedDelta = Stopwatch.Frequency / Fps;

        long[] qpcTimestamps = new long[FrameCount];
        long[] ptsValues = new long[FrameCount];

        for (int i = 0; i < FrameCount; i++)
        {
            // Synthetic QPC timestamp: T0 + i * (Freq / FPS)
            qpcTimestamps[i] = t0 + (long)i * expectedDelta;
            ptsValues[i] = TimestampConverter.ComputePts(qpcTimestamps[i], t0);
        }

        // Step 2: Verify PTS contract
        bool passFrame0PtsZero = ptsValues[0] == 0;
        bool passFrame1PtsApprox166k = Math.Abs(ptsValues[1] - expectedDelta) <= 1;
        bool passFrame2PtsApprox333k = Math.Abs(ptsValues[2] - 2 * expectedDelta) <= 1;

        // Compute deltas
        long[] deltas = new long[FrameCount - 1];
        for (int i = 1; i < FrameCount; i++)
        {
            deltas[i - 1] = ptsValues[i] - ptsValues[i - 1];
        }

        long minDelta = deltas.Min();
        long maxDelta = deltas.Max();
        double averageDelta = deltas.Average();
        double jitter = (maxDelta - minDelta) / averageDelta; // ratio

        bool passAverageDelta = Math.Abs(averageDelta - expectedDelta) <= 1.0;
        bool passJitter = jitter < JitterThreshold;

        result.T0 = t0;
        result.ExpectedDelta = expectedDelta;
        result.ActualAverageDelta = averageDelta;
        result.MinDelta = minDelta;
        result.MaxDelta = maxDelta;
        result.Jitter = jitter;
        result.Frame0Pts = ptsValues[0];
        result.Frame1Pts = ptsValues[1];
        result.Frame2Pts = ptsValues[2];
        result.LastFramePts = ptsValues[FrameCount - 1];

        result.PassFrame0PtsZero = passFrame0PtsZero;
        result.PassAverageDelta = passAverageDelta;
        result.PassJitter = passJitter;
        result.Pass = passFrame0PtsZero && passAverageDelta && passJitter;

        // Step 3: Print first few frames
        Console.WriteLine($"T0 (QPC):                  {t0}");
        Console.WriteLine($"Expected delta per frame:  {expectedDelta} ticks ({(double)expectedDelta / Stopwatch.Frequency * 1000.0:F4} ms)");
        Console.WriteLine();
        Console.WriteLine("First 5 frames:");
        Console.WriteLine($"  {"Frame",-6} {"QPC Timestamp",-20} {"PTS",-20} {"Delta",-15}");
        Console.WriteLine($"  {"------",-6} {"--------------------",-20} {"--------------------",-20} {"---------------",-15}");
        for (int i = 0; i < 5; i++)
        {
            long delta = i == 0 ? 0 : ptsValues[i] - ptsValues[i - 1];
            string deltaStr = i == 0 ? "FIRST" : delta.ToString();
            Console.WriteLine($"  {i,-6} {qpcTimestamps[i],-20} {ptsValues[i],-20} {deltaStr,-15}");
        }
        Console.WriteLine($"  ...");
        Console.WriteLine($"  {FrameCount - 1,-6} {qpcTimestamps[FrameCount - 1],-20} {ptsValues[FrameCount - 1],-20} {deltas[FrameCount - 2],-15}");
        Console.WriteLine();

        // Step 4: Summary stats
        Console.WriteLine($"Average delta:             {averageDelta:F2} ticks ({averageDelta / Stopwatch.Frequency * 1000.0:F4} ms)");
        Console.WriteLine($"Min delta:                  {minDelta} ticks");
        Console.WriteLine($"Max delta:                  {maxDelta} ticks");
        Console.WriteLine($"Jitter:                    {jitter:P4} ({jitter * 100:F4}%)");
        Console.WriteLine();
        Console.WriteLine($"Acceptance: Frame 0 PTS == 0              : {(passFrame0PtsZero ? "PASS" : "FAIL")} (actual: {ptsValues[0]})");
        Console.WriteLine($"Acceptance: Average delta ~= Freq/FPS      : {(passAverageDelta ? "PASS" : "FAIL")} (actual avg: {averageDelta:F2}, expected: {expectedDelta})");
        Console.WriteLine($"Acceptance: Jitter < 1%                    : {(passJitter ? "PASS" : "FAIL")} (actual: {jitter * 100:F4}%)");
        Console.WriteLine();
        Console.WriteLine($"V2-4 Result: {(result.Pass ? "PASS" : "FAIL")}");
        Console.WriteLine();

        if (!result.Pass)
        {
            Console.WriteLine("  WARNING: PTS contract violation.");
            Console.WriteLine("  This indicates either:");
            Console.WriteLine("    - Frame timestamps are not generated at uniform intervals");
            Console.WriteLine("    - T0 reference is not the first frame's timestamp");
            Console.WriteLine("    - PTS computation has a rounding or scaling bug");
            Console.WriteLine("  Action: Verify PTS formula and frame timestamp generation.");
            Console.WriteLine();
        }

        // Step 5: Print forbidden-pattern warning (reminder)
        Console.WriteLine("Forbidden patterns (must NOT appear in production code):");
        Console.WriteLine("  ❌ DateTime.UtcNow as clock source (not monotonic, not high-resolution)");
        Console.WriteLine("  ❌ TimeSpan.Ticks as Engine PTS unit (Engine PTS is QPC ticks)");
        Console.WriteLine("  ✅ Engine PTS = QPC timestamp - T0, in Stopwatch ticks");
        Console.WriteLine();

        return result;
    }

    public sealed class TestResult
    {
        public string TestId { get; set; } = "";
        public string TestName { get; set; } = "";
        public int Fps { get; set; }
        public int FrameCount { get; set; }
        public long T0 { get; set; }
        public long ExpectedDelta { get; set; }
        public double ActualAverageDelta { get; set; }
        public long MinDelta { get; set; }
        public long MaxDelta { get; set; }
        public double Jitter { get; set; }
        public long Frame0Pts { get; set; }
        public long Frame1Pts { get; set; }
        public long Frame2Pts { get; set; }
        public long LastFramePts { get; set; }
        public bool PassFrame0PtsZero { get; set; }
        public bool PassAverageDelta { get; set; }
        public bool PassJitter { get; set; }
        public bool Pass { get; set; }
    }
}
