// Program.cs — V2 WGC Timestamp Validation Spike — Pure Math Tests
//
// P1-B.2 V2 — WGC Timestamp Validation
//
// APPROVED CONTRACT:
//   GraphicsCaptureFrame.SystemRelativeTime:
//   - QPC-based timestamp
//   - Windows.Foundation.TimeSpan
//   - TimeSpan.Ticks = 100 ns
//   - .Ticks is already normalized to 100-ns units
//   - It is NOT raw QPC/Stopwatch ticks
//
//   PTS = SystemRelativeTime.Ticks - T0
//
//   DO NOT multiply by 100.
//   DO NOT perform QPF conversion on SystemRelativeTime.Ticks.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

namespace V2_WGC_Timestamp_Spike;

internal static class Program
{
    private static int _passed = 0;
    private static int _failed = 0;
    private static readonly List<string> _failures = new();

    private static int Main(string[] args)
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" V2 WGC Timestamp Validation Spike — Pure Math Tests");
        Console.WriteLine(" Branch: Engine-Rebuild (spike — does NOT modify repo)");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Run all test groups
        Tests.TimestampMathTests.RunAll(RunTest);
        Tests.TimestampBoundaryTests.RunAll(RunTest);
        Tests.TimestampMonotonicityTests.RunAll(RunTest);
        Tests.LongDurationTests.RunAll(RunTest);
        Tests.ForbiddenMultiplyTests.RunAll(RunTest);
        Tests.QpcSeparationTests.RunAll(RunTest);

        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine($" Result: {_passed} passed, {_failed} failed, {_passed + _failed} total");
        Console.WriteLine("------------------------------------------------------------");
        if (_failed > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failures:");
            foreach (var f in _failures)
                Console.WriteLine($"  - {f}");
        }
        return _failed > 0 ? 1 : 0;
    }

    internal static void RunTest(string name, Action test)
    {
        var paddedName = name.Length < 70 ? name + new string(' ', 70 - name.Length) : name;
        Console.Write($"[{paddedName}] ");
        try
        {
            test();
            _passed++;
            Console.WriteLine("PASS");
        }
        catch (Exception ex)
        {
            _failed++;
            _failures.Add($"{name} -> {ex.Message}");
            Console.WriteLine("FAIL");
            Console.WriteLine($"    {ex.Message}");
        }
    }
}
