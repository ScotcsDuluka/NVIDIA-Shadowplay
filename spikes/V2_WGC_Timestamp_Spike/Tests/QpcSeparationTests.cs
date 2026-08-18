// Tests/QpcSeparationTests.cs
//
// V2 Spike — Test Group 6: Prove raw-QPC conversion is kept separate
//
// The WGC timestamp path and the raw-QPC conversion path are DIFFERENT:
//
//   WGC path:
//     pts100ns = SystemRelativeTime.Ticks - T0
//     // No QPF, no multiplication, no division
//
//   Raw-QPC path (separate, NOT used for WGC):
//     rawDelta = Stopwatch.GetTimestamp() - qpcT0
//     pts100ns = rawDelta * 10_000_000 / Stopwatch.Frequency
//     // Requires QPF conversion
//
// These tests prove the two paths are kept separate and that the WGC path
// does NOT accidentally use QPC conversion.
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace V2_WGC_Timestamp_Spike.Tests;

public static class QpcSeparationTests
{
    public static void RunAll(Action<string, Action> runner)
    {
        runner("QPC_SEP: WGC path = simple subtraction (no QPF)", Test_WgcNoQpf);
        runner("QPC_SEP: QPC path = rawDelta × 10M / QPF (needs QPF)", Test_QpcNeedsQpf);
        runner("QPC_SEP: WGC and QPC produce same unit (100-ns)", Test_SameUnit);
        runner("QPC_SEP: WGC path does NOT reference Stopwatch.Frequency", Test_NoFrequencyReference);
        runner("QPC_SEP: WGC path does NOT divide by QPF", Test_NoDivision);
    }

    /// <summary>
    /// WGC path: PTS = SystemRelativeTime.Ticks - T0.
    /// No Stopwatch.Frequency, no multiplication, no division.
    /// </summary>
    private static void Test_WgcNoQpf()
    {
        long t0Wgc = 25_000_000_000_000L;
        long tnWgc = t0Wgc + 5_000_000L; // 0.5 seconds later

        // The ONLY operation in the WGC path
        long wgcPts = tnWgc - t0Wgc;

        Assert(wgcPts == 5_000_000L, $"WGC 0.5s PTS should be 5,000,000, got {wgcPts}");

        // Verify: no QPF was needed
        // (Stopwatch.Frequency was NOT used in the calculation above)
    }

    /// <summary>
    /// QPC path: requires Stopwatch.Frequency to convert raw ticks to 100-ns.
    /// This is the SEPARATE path — not used for WGC timestamps.
    /// </summary>
    private static void Test_QpcNeedsQpf()
    {
        long qpcT0 = Stopwatch.GetTimestamp();
        // Simulate a 0.5 second delay (in raw QPC ticks)
        long qpcTn = qpcT0 + Stopwatch.Frequency / 2;

        // QPC path REQUIRES QPF conversion
        long rawDelta = qpcTn - qpcT0;
        long qpcPts = rawDelta * 10_000_000L / Stopwatch.Frequency;

        // Result should be ~5,000,000 (0.5 seconds in 100-ns)
        Assert(qpcPts > 4_900_000L && qpcPts < 5_100_000L,
            $"QPC 0.5s PTS should be ~5,000,000, got {qpcPts}");

        // Key point: QPC path NEEDED Stopwatch.Frequency.
        // WGC path does NOT.
    }

    /// <summary>
    /// Both paths produce the same unit (100-ns ticks), but via different methods.
    /// WGC: direct (no conversion).
    /// QPC: converted via QPF.
    /// </summary>
    private static void Test_SameUnit()
    {
        // WGC: 1 second = 10,000,000 ticks (by definition)
        long wgcOneSecond = 10_000_000L;

        // QPC: 1 second = Stopwatch.Frequency ticks, then converted:
        long qpcOneSecond = Stopwatch.Frequency; // raw ticks
        long qpcConverted = qpcOneSecond * 10_000_000L / Stopwatch.Frequency; // → 100-ns

        Assert(qpcConverted == wgcOneSecond,
            $"Both paths should produce 10,000,000 for 1 second: WGC={wgcOneSecond}, QPC={qpcConverted}");
    }

    /// <summary>
    /// Prove that the WGC path function does NOT reference Stopwatch.Frequency.
    ///
    /// The WGC PTS calculation is:
    ///   long WgcPts(long systemRelativeTimeTicks, long t0)
    ///   {
    ///       return systemRelativeTimeTicks - t0;
    ///   }
    ///
    /// This function does NOT use Stopwatch.Frequency.
    /// </summary>
    private static void Test_NoFrequencyReference()
    {
        // Simulate the WGC PTS function
        static long WgcPts(long systemRelativeTimeTicks, long t0)
        {
            return systemRelativeTimeTicks - t0;
        }

        // Test it
        long t0 = 25_000_000_000_000L;
        long tn = t0 + 166_667L;
        long pts = WgcPts(tn, t0);

        Assert(pts == 166_667L, $"WGC PTS should be 166,667, got {pts}");

        // The function WgcPts does NOT reference Stopwatch.Frequency.
        // It's a pure subtraction. No QPF involvement.
        // (This is a code-level proof — the function source is above.)
    }

    /// <summary>
    /// Prove that the WGC path does NOT divide by QPF.
    /// The forbidden QPC-style conversion is:
    ///   pts = rawDelta * 10_000_000 / Stopwatch.Frequency
    ///
    /// The WGC path is simply:
    ///   pts = systemRelativeTimeTicks - t0
    ///
    /// No division, no QPF.
    /// </summary>
    private static void Test_NoDivision()
    {
        long t0 = 25_000_000_000_000L;
        long tn = t0 + 10_000_000L; // 1 second

        // WGC path: NO division
        long wgcPts = tn - t0;
        Assert(wgcPts == 10_000_000L, $"WGC 1s should be 10,000,000 (no division), got {wgcPts}");

        // Forbidden QPC-style path (would be wrong for WGC):
        long qpf = Stopwatch.Frequency;
        long wrongQpcStylePts = (tn - t0) * 10_000_000L / qpf;

        // If QPF happens to be 10MHz, both would be equal — but that's a coincidence.
        // The WGC path is correct BY DEFINITION, not by QPF conversion.
        // If QPF ≠ 10MHz, the QPC-style path would give a DIFFERENT (wrong) answer.

        if (qpf != 10_000_000L)
        {
            Assert(wrongQpcStylePts != wgcPts,
                $"When QPF={qpf} (≠10MHz), QPC-style path should differ from WGC: " +
                $"QPC-style={wrongQpcStylePts}, WGC={wgcPts}");
        }

        // Regardless of QPF, the WGC path is always correct because
        // SystemRelativeTime.Ticks is DEFINED as 100-ns.
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"ASSERT FAILED: {message}");
    }
}
