// Tests/ConversionTest.cs
//
// P1-B.2-V2 Test V2-3 — WGC SystemRelativeTime Conversion
//
// Goal: Prove the conversion formula between WGC TimeSpan ticks and QPC ticks.
//
// Conversion:
//   qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond
//
// Test approach:
//   For each input duration (0s, 1s, 10s, 60s, 3600s):
//     1. Compute wgcTicks = duration_seconds * TimeSpan.TicksPerSecond
//     2. Compute actualQpc = TimestampConverter.ConvertWgcToQpc(wgcTicks)
//     3. Compute expectedQpc = duration_seconds * Stopwatch.Frequency (the
//        "ground truth" QPC count for that duration)
//     4. Compute error = |actualQpc - expectedQpc| in milliseconds
//
// Acceptance: error < 1ms for all durations
//
// Notes:
//   - On typical Windows hardware, Stopwatch.Frequency == TimeSpan.TicksPerSecond
//     (both 10 MHz), so the ratio is 1:1 and conversion is identity.
//     In that case error is exactly 0 ms for all durations.
//   - On hardware where Stopwatch.Frequency != 10 MHz (some older systems,
//     or systems with TSC-based QPC at a different rate), the conversion
//     formula scales correctly and error remains small (limited by
//     integer-division rounding).
//   - The test validates that the FORMULA is correct, not that the hardware
//     has a specific frequency.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;
using CaptureEngine.Timestamp.Spike.Utils;

namespace CaptureEngine.Timestamp.Spike.Tests;

public static class ConversionTest
{
    // Test inputs: durations in seconds
    private static readonly double[] TestDurationsSeconds = { 0, 1, 10, 60, 3600 };

    // Acceptance threshold: error < 1 ms (in seconds)
    private const double ErrorThresholdSeconds = 0.001;

    public static TestResult Run()
    {
        var result = new TestResult
        {
            TestId = "V2-3",
            TestName = "WGC SystemRelativeTime Conversion",
        };

        Console.WriteLine("============================================================");
        Console.WriteLine(" V2-3 — WGC CONVERSION TEST");
        Console.WriteLine("============================================================");
        Console.WriteLine();
        Console.WriteLine($"Formula: qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond");
        Console.WriteLine($"Stopwatch.Frequency:     {Stopwatch.Frequency} Hz");
        Console.WriteLine($"TimeSpan.TicksPerSecond: {TimeSpan.TicksPerSecond} Hz");
        Console.WriteLine($"Ratio (Freq / TicksPerSec): {(double)Stopwatch.Frequency / TimeSpan.TicksPerSecond:F6}");
        Console.WriteLine();

        // Step 1: Display a sample conversion for "1 second" (per spec output format)
        long oneSecondWgcTicks = 1L * TimeSpan.TicksPerSecond;
        long oneSecondExpectedQpc = Stopwatch.Frequency; // 1 second of QPC ticks
        long oneSecondActualQpc = TimestampConverter.ConvertWgcToQpc(oneSecondWgcTicks);
        double oneSecondErrorMs = Math.Abs(oneSecondActualQpc - oneSecondExpectedQpc)
                                  / (double)Stopwatch.Frequency * 1000.0;

        Console.WriteLine($"Input: 1 second");
        Console.WriteLine($"  WGC ticks (input):    {oneSecondWgcTicks}");
        Console.WriteLine($"  Expected QPC:         {oneSecondExpectedQpc}");
        Console.WriteLine($"  Actual QPC:           {oneSecondActualQpc}");
        Console.WriteLine($"  Error:                {oneSecondErrorMs:F4} ms");
        Console.WriteLine();

        // Step 2: Run all test cases
        Console.WriteLine("Test cases:");
        Console.WriteLine($"  {"Input",-10} {"WGC Ticks",-20} {"Expected QPC",-20} {"Actual QPC",-20} {"Error (ms)",-12} {"Result",-8}");
        Console.WriteLine($"  {"----------",-10} {"--------------------",-20} {"--------------------",-20} {"--------------------",-20} {"----------",-12} {"--------",-8}");

        bool allPass = true;
        var rows = new List<ConversionRow>();

        foreach (double durationSeconds in TestDurationsSeconds)
        {
            long wgcTicks = (long)(durationSeconds * TimeSpan.TicksPerSecond);
            long expectedQpc = (long)(durationSeconds * Stopwatch.Frequency);
            long actualQpc = TimestampConverter.ConvertWgcToQpc(wgcTicks);
            double errorMs = Math.Abs(actualQpc - expectedQpc)
                             / (double)Stopwatch.Frequency * 1000.0;

            bool pass = errorMs < ErrorThresholdSeconds * 1000.0; // < 1 ms
            if (!pass) allPass = false;

            string inputLabel = durationSeconds switch
            {
                0 => "0s",
                1 => "1s",
                10 => "10s",
                60 => "60s",
                3600 => "3600s",
                _ => $"{durationSeconds}s"
            };

            Console.WriteLine($"  {inputLabel,-10} {wgcTicks,-20} {expectedQpc,-20} {actualQpc,-20} {errorMs,-12:F4} {(pass ? "PASS" : "FAIL"),-8}");

            rows.Add(new ConversionRow
            {
                Input = inputLabel,
                WgcTicks = wgcTicks,
                ExpectedQpc = expectedQpc,
                ActualQpc = actualQpc,
                ErrorMs = errorMs,
                Pass = pass,
            });
        }

        result.Rows = rows;
        result.Pass = allPass;

        Console.WriteLine();
        Console.WriteLine($"Acceptance: error < 1ms for all : {(allPass ? "PASS" : "FAIL")}");
        Console.WriteLine();
        Console.WriteLine($"V2-3 Result: {(result.Pass ? "PASS" : "FAIL")}");
        Console.WriteLine();

        if (!allPass)
        {
            Console.WriteLine("  WARNING: Conversion error exceeded 1ms threshold.");
            Console.WriteLine("  This indicates either:");
            Console.WriteLine("    - Stopwatch.Frequency is significantly different from 10 MHz");
            Console.WriteLine("    - Integer division is losing precision for large durations");
            Console.WriteLine("  Action: Verify Stopwatch.Frequency and re-check the formula.");
            Console.WriteLine();
        }

        return result;
    }

    public sealed class TestResult
    {
        public string TestId { get; set; } = "";
        public string TestName { get; set; } = "";
        public List<ConversionRow> Rows { get; set; } = new();
        public bool Pass { get; set; }
    }

    public sealed class ConversionRow
    {
        public string Input { get; set; } = "";
        public long WgcTicks { get; set; }
        public long ExpectedQpc { get; set; }
        public long ActualQpc { get; set; }
        public double ErrorMs { get; set; }
        public bool Pass { get; set; }
    }
}
