// Program.cs
//
// P1-B.2-V1 Spike — D3D11/NVENC Interop Validation
//
// Entry point. Runs phases 1-5 in sequence, with the option to run a single phase.
//
// Usage:
//   CaptureEngine.Video.Spike.D3D11.exe                # run all phases 1-5
//   CaptureEngine.Video.Spike.D3D11.exe phase1         # run only Phase 1
//   CaptureEngine.Video.Spike.D3D11.exe phase4         # run only Phase 4 (NVENC)
//   CaptureEngine.Video.Spike.D3D11.exe --log report.md  # tee output to file
//
// Exit codes:
//   0 = all requested phases passed
//   1 = at least one phase failed (see console output for details)
//   2 = invalid arguments
//
// SPDX-License-Identifier: MIT

using CaptureEngine.Video.Spike.D3D11.Phases;

namespace CaptureEngine.Video.Spike.D3D11;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        PrintBanner();

        // Parse args
        string? logFile = null;
        var phasesToRun = new List<int>();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i].ToLowerInvariant();
            if (a == "--log" && i + 1 < args.Length)
            {
                logFile = args[++i];
            }
            else if (a.StartsWith("phase", StringComparison.Ordinal))
            {
                if (int.TryParse(a.AsSpan("phase".Length), out int n) && n >= 1 && n <= 5)
                    phasesToRun.Add(n);
                else
                {
                    Console.Error.WriteLine($"ERROR: Invalid phase: {a}");
                    return 2;
                }
            }
            else if (a == "--help" || a == "-h")
            {
                PrintHelp();
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: Unknown argument: {a}");
                PrintHelp();
                return 2;
            }
        }

        if (phasesToRun.Count == 0)
            phasesToRun = new() { 1, 2, 3, 4, 5 };

        // Optionally tee output to a log file
        TextWriter? logWriter = null;
        if (logFile != null)
        {
            try
            {
                logWriter = new StreamWriter(logFile, append: false) { AutoFlush = true };
                Console.SetOut(new TeeWriter(Console.Out, logWriter));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WARNING: Could not open log file '{logFile}': {ex.Message}");
            }
        }

        // Run phases
        int overallResult = 0;
        try
        {
            foreach (int phaseNum in phasesToRun)
            {
                int result = phaseNum switch
                {
                    1 => Phase1_DeviceTest.Run(),
                    2 => Phase2_DesktopDuplication.Run(),
                    3 => Phase3_TextureOwnership.Run(),
                    4 => Phase4_NVENCRegistration.Run(),
                    5 => Phase5_PerformanceBenchmark.Run(),
                    _ => 1,
                };
                if (result != 0)
                {
                    overallResult = result;
                    Console.WriteLine($"*** Phase {phaseNum} FAILED — stopping.");
                    break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(overallResult == 0
                ? " SPIKE RESULT: ALL REQUESTED PHASES PASSED"
                : " SPIKE RESULT: AT LEAST ONE PHASE FAILED");
            Console.WriteLine("============================================================");
        }
        finally
        {
            SpikeSharedContext.Cleanup();
            logWriter?.Dispose();
        }

        return overallResult;
    }

    private static void PrintBanner()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" CaptureEngine.Video.Spike.D3D11");
        Console.WriteLine(" P1-B.2-V1 — D3D11/NVENC Interop Spike");
        Console.WriteLine(" Branch: Engine-Rebuild (spike — does NOT modify repo)");
        Console.WriteLine(" Foundation baseline: 82d792ab (untouched)");
        Console.WriteLine(" Target commit: 39da0640 (untouched)");
        Console.WriteLine("============================================================");
        Console.WriteLine();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  CaptureEngine.Video.Spike.D3D11.exe [phaseN...] [--log FILE]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  phase1     Run Phase 1 (D3D11 Device Test)");
        Console.WriteLine("  phase2     Run Phase 2 (Desktop Duplication Test)");
        Console.WriteLine("  phase3     Run Phase 3 (Texture Ownership Test)");
        Console.WriteLine("  phase4     Run Phase 4 (NVENC Registration Test)");
        Console.WriteLine("  phase5     Run Phase 5 (Performance Benchmark)");
        Console.WriteLine("  --log F    Tee output to file F (in addition to console)");
        Console.WriteLine("  --help     Show this help");
        Console.WriteLine();
        Console.WriteLine("If no phase is specified, all 5 phases run in order.");
        Console.WriteLine("Phases must be run in order — running Phase 4 without Phase 1-3 will fail.");
    }

    /// <summary>
    /// Writer that writes to both console and a log file simultaneously.
    /// </summary>
    private sealed class TeeWriter : TextWriter
    {
        private readonly TextWriter _a;
        private readonly TextWriter _b;
        public TeeWriter(TextWriter a, TextWriter b) { _a = a; _b = b; }
        public override System.Text.Encoding Encoding => _a.Encoding;
        public override void Write(char value) { _a.Write(value); _b.Write(value); }
        public override void Write(string? value) { _a.Write(value); _b.Write(value); }
        public override void WriteLine(string? value) { _a.WriteLine(value); _b.WriteLine(value); }
        public override void WriteLine() { _a.WriteLine(); _b.WriteLine(); }
        public override void Flush() { _a.Flush(); _b.Flush(); }
    }
}
