// Tests/QpcClockTest.cs
//
// P1-B.2-V2 Test V2-1 — QPC Clock Validation
//
// Goal: Prove that Stopwatch is backed by the High Resolution Counter (QPC).
//
// Outputs:
//   Stopwatch.IsHighResolution : TRUE
//   Stopwatch.Frequency        : XXXXXXXX Hz
//   Timestamp (GetTimestamp)    : XXXXXXXX
//   Seconds                     : X.XXXXXXXX
//
// Acceptance:
//   IsHighResolution == true
//   Frequency > 1 MHz (1,000,000 Hz)
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;

namespace CaptureEngine.Timestamp.Spike.Tests;

public static class QpcClockTest
{
    public static TestResult Run()
    {
        var result = new TestResult
        {
            TestId = "V2-1",
            TestName = "QPC Clock Validation",
        };

        Console.WriteLine("============================================================");
        Console.WriteLine(" V2-1 — QPC CLOCK TEST");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Step 1: Query Stopwatch.IsHighResolution
        bool isHighRes = Stopwatch.IsHighResolution;
        Console.WriteLine($"Stopwatch.IsHighResolution: {isHighRes}");

        // Step 2: Query Stopwatch.Frequency
        long frequency = Stopwatch.Frequency;
        Console.WriteLine($"Stopwatch.Frequency:        {frequency} Hz");

        // Step 3: Capture a timestamp
        long timestamp = Stopwatch.GetTimestamp();
        Console.WriteLine($"Timestamp:                  {timestamp}");

        // Step 4: Convert to seconds
        double seconds = (double)timestamp / Stopwatch.Frequency;
        Console.WriteLine($"Seconds:                    {seconds:F8}");

        // Step 5: Acceptance check
        bool passIsHighRes = isHighRes;
        bool passFreq = frequency > 1_000_000; // > 1 MHz

        result.IsHighResolution = isHighRes;
        result.Frequency = frequency;
        result.Timestamp = timestamp;
        result.Seconds = seconds;
        result.PassIsHighResolution = passIsHighRes;
        result.PassFrequency = passFreq;

        Console.WriteLine();
        Console.WriteLine($"Acceptance: IsHighResolution==true : {(passIsHighRes ? "PASS" : "FAIL")}");
        Console.WriteLine($"Acceptance: Frequency > 1MHz       : {(passFreq ? "PASS" : "FAIL")} ({frequency:N0} Hz)");

        result.Pass = passIsHighRes && passFreq;
        Console.WriteLine();
        Console.WriteLine($"V2-1 Result: {(result.Pass ? "PASS" : "FAIL")}");
        Console.WriteLine();

        return result;
    }

    public sealed class TestResult
    {
        public string TestId { get; set; } = "";
        public string TestName { get; set; } = "";
        public bool IsHighResolution { get; set; }
        public long Frequency { get; set; }
        public long Timestamp { get; set; }
        public double Seconds { get; set; }
        public bool PassIsHighResolution { get; set; }
        public bool PassFrequency { get; set; }
        public bool Pass { get; set; }
    }
}
