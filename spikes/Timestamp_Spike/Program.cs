// Program.cs
//
// P1-B.2-V2 Timestamp Spike — entry point.
//
// Runs 4 tests in sequence:
//   V2-1: QPC Clock Validation           — Stopwatch.IsHighResolution + Frequency > 1MHz
//   V2-2: Monotonic Timestamp Test        — 100,000 samples, 0 backward jumps
//   V2-3: WGC Conversion Test             — formula qpc = wgcTicks * Freq / TicksPerSecond
//   V2-4: Engine PTS Contract              — 60 FPS, delta ~= Freq/60, jitter < 1%
//
// Output:
//   - Console output (OWNER captures to log file)
//   - Final verdict block at end of run
//
// Engine timestamp model decided by this spike:
//   - Internal unit: QPC ticks (Stopwatch ticks)
//   - WGC → QPC conversion: qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond
//   - PTS = qpcTimestamp - T0
//
// Forbidden (per spec):
//   ❌ DateTime.UtcNow as clock (not monotonic, not high-resolution)
//   ❌ TimeSpan.Ticks as Engine PTS unit (Engine PTS is QPC ticks)
//
// SPDX-License-Identifier: MIT
// Spike code — not production. Does NOT modify Engine-Rebuild.

using CaptureEngine.Timestamp.Spike.Tests;

namespace CaptureEngine.Timestamp.Spike;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" P1-B.2-V2 Timestamp Spike");
        Console.WriteLine(" Branch: Engine-Rebuild (spike — does NOT modify repo)");
        Console.WriteLine(" Runtime: .NET 8, Windows, x64");
        Console.WriteLine("============================================================");
        Console.WriteLine();
        Console.WriteLine("Engine timestamp model under validation:");
        Console.WriteLine("  Internal unit: QPC ticks (Stopwatch ticks)");
        Console.WriteLine("  WGC → QPC conversion: qpc = wgcTicks * Freq / TicksPerSecond");
        Console.WriteLine("  PTS formula:          pts = qpcTimestamp - T0");
        Console.WriteLine();

        // Allow optional test selection via args (--test V2-1)
        // Default: run all tests.
        string? selectedTest = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--test" && i + 1 < args.Length)
            {
                selectedTest = args[++i].ToUpperInvariant();
            }
            else if (args[i] == "--help" || args[i] == "-h")
            {
                PrintUsage();
                return 0;
            }
        }

        var results = new List<bool>();

        if (selectedTest == null || selectedTest == "V2-1")
        {
            var r = QpcClockTest.Run();
            results.Add(r.Pass);
        }

        if (selectedTest == null || selectedTest == "V2-2")
        {
            var r = MonotonicTest.Run();
            results.Add(r.Pass);
        }

        if (selectedTest == null || selectedTest == "V2-3")
        {
            var r = ConversionTest.Run();
            results.Add(r.Pass);
        }

        if (selectedTest == null || selectedTest == "V2-4")
        {
            var r = PtsContractTest.Run();
            results.Add(r.Pass);
        }

        // Final verdict
        Console.WriteLine("============================================================");
        Console.WriteLine(" FINAL VERDICT");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        int passed = results.Count(p => p);
        int total = results.Count;
        bool allPass = passed == total;

        Console.WriteLine($"Tests run:    {total}");
        Console.WriteLine($"Tests passed: {passed}");
        Console.WriteLine($"Tests failed: {total - passed}");
        Console.WriteLine();

        Console.WriteLine("P1-B.2-V2 RESULT");
        Console.WriteLine("----------------");
        Console.WriteLine($"Timestamp source:        QPC (Stopwatch)");
        Console.WriteLine($"Conversion (V2-3):       {(selectedTest == null || selectedTest == "V2-3" ? (allPass ? "PASS" : "FAIL") : "N/A")}");
        Console.WriteLine($"Monotonic (V2-2):         {(selectedTest == null || selectedTest == "V2-2" ? (allPass ? "PASS" : "FAIL") : "N/A")}");
        Console.WriteLine($"Engine timestamp unit:   Stopwatch ticks (QPC)");
        Console.WriteLine($"STATUS:                   {(allPass ? "RESOLVED" : "BLOCKED")}");
        Console.WriteLine();

        if (!allPass)
        {
            Console.WriteLine("One or more tests failed. See above for failure details.");
            Console.WriteLine("Do NOT proceed with production backend until all 4 tests pass.");
        }
        else
        {
            Console.WriteLine("All 4 tests passed. Engine timestamp model validated.");
            Console.WriteLine("Production backend may proceed using QPC ticks as Engine PTS unit.");
        }
        Console.WriteLine();

        return allPass ? 0 : 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: CaptureEngine.Timestamp.Spike.exe [--test V2-1|V2-2|V2-3|V2-4]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --test <ID>   Run only the specified test (V2-1, V2-2, V2-3, or V2-4)");
        Console.WriteLine("  --help, -h    Show this help message");
        Console.WriteLine();
        Console.WriteLine("Default (no args): run all 4 tests in sequence.");
        Console.WriteLine();
        Console.WriteLine("To capture output to a log file:");
        Console.WriteLine("  CaptureEngine.Timestamp.Spike.exe > timestamp_spike_output.log 2>&1");
    }
}
