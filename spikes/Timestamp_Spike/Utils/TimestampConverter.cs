// Utils/TimestampConverter.cs
//
// P1-B.2-V2 Timestamp Spike — Timestamp conversion utilities.
//
// Provides conversion between WGC SystemRelativeTime ticks (100-ns units,
// TimeSpan ticks) and Engine QPC ticks (Stopwatch ticks).
//
// CONVERSION FORMULA (per V2-3 spec):
//   qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond
//
// Rationale:
//   WGC SystemRelativeTime is a Windows.Foundation.TimeSpan whose .Ticks
//   property is normalized to 100-ns units (frequency = 10^7 Hz).
//   Stopwatch ticks are at QPC frequency (typically 10 MHz on Windows,
//   but can vary).
//
//   To convert a WGC timestamp to the Engine's QPC clock domain:
//     qpc = wgcTicks * (Stopwatch.Frequency / TimeSpan.TicksPerSecond)
//
//   The ratio (Stopwatch.Frequency / TimeSpan.TicksPerSecond) is the
//   "ticks per tick" conversion factor. On a typical Windows machine:
//     Stopwatch.Frequency     = 10,000,000 (10 MHz QPC)
//     TimeSpan.TicksPerSecond = 10,000,000 (100-ns ticks, 10 MHz)
//     ratio                   = 1.0 (1:1 on this hardware)
//
//   On hardware where QPC frequency differs from 10 MHz, the ratio
//   differs from 1.0 and conversion is required for cross-clock comparison.
//
// CRITICAL:
//   - DO NOT use DateTime.UtcNow as a clock — not monotonic, not high-resolution.
//   - DO NOT use TimeSpan.Ticks directly as Engine PTS — Engine PTS is QPC ticks.
//   - The PTS = QPC timestamp - T0, where T0 is the QPC timestamp of the first frame.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;

namespace CaptureEngine.Timestamp.Spike.Utils;

/// <summary>
/// Timestamp conversion utilities for cross-clock-domain translation.
///
/// The Engine uses QPC (Stopwatch) ticks as its internal timestamp unit.
/// WGC (Windows Graphics Capture) provides SystemRelativeTime as a
/// Windows.Foundation.TimeSpan with 100-ns ticks.
///
/// This class provides the bridge: convert WGC ticks to QPC ticks so
/// they can be compared against Stopwatch.GetTimestamp() values.
/// </summary>
public static class TimestampConverter
{
    /// <summary>
    /// Converts WGC SystemRelativeTime ticks (100-ns units) to QPC ticks
    /// (Stopwatch.Frequency units).
    ///
    /// Formula: qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond
    ///
    /// This is the formula validated by V2-3.
    /// </summary>
    /// <param name="wgcTicks">WGC SystemRelativeTime.Ticks value (100-ns units).</param>
    /// <returns>Equivalent value in QPC ticks (Stopwatch ticks).</returns>
    public static long ConvertWgcToQpc(long wgcTicks)
    {
        // Use long arithmetic — both operands are integers.
        // Order matters for precision: multiply first, then divide.
        // This avoids floating-point rounding for large timestamps.
        //
        // On typical Windows hardware (Stopwatch.Frequency == 10,000,000 and
        // TimeSpan.TicksPerSecond == 10,000,000), the ratio is 1:1 and conversion
        // is identity. On hardware with different QPC frequency, this scales
        // correctly.
        return wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Converts QPC ticks (Stopwatch ticks) to WGC SystemRelativeTime ticks
    /// (100-ns units).
    ///
    /// Inverse of ConvertWgcToQpc. Useful for cases where Engine computes
    /// a PTS in QPC ticks and needs to compare against a WGC timestamp.
    /// </summary>
    /// <param name="qpcTicks">QPC timestamp (Stopwatch ticks).</param>
    /// <returns>Equivalent value in WGC ticks (100-ns units).</returns>
    public static long ConvertQpcToWgc(long qpcTicks)
    {
        return qpcTicks * TimeSpan.TicksPerSecond / Stopwatch.Frequency;
    }

    /// <summary>
    /// Converts QPC ticks to seconds (double-precision floating point).
    /// </summary>
    public static double QpcToSeconds(long qpcTicks)
    {
        return (double)qpcTicks / Stopwatch.Frequency;
    }

    /// <summary>
    /// Converts WGC ticks (100-ns units) to seconds (double-precision floating point).
    /// </summary>
    public static double WgcToSeconds(long wgcTicks)
    {
        return (double)wgcTicks / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Computes the PTS (Presentation Time Stamp) for a frame given its raw
    /// QPC timestamp and the T0 reference (first frame's QPC timestamp).
    ///
    /// PTS = qpcTimestamp - t0
    ///
    /// PTS is in QPC ticks (the Engine's internal timestamp unit).
    /// Frame 0 has PTS = 0 by construction.
    /// </summary>
    public static long ComputePts(long qpcTimestamp, long t0)
    {
        return qpcTimestamp - t0;
    }

    /// <summary>
    /// Computes the expected PTS delta between consecutive frames at a given
    /// target FPS. Used by V2-4 to verify PTS contract.
    ///
    /// Expected delta = Stopwatch.Frequency / fps
    ///
    /// At 60 FPS with Freq=10MHz: delta = 166,666 ticks (16.67 ms).
    /// </summary>
    public static long ExpectedPtsDelta(int fps)
    {
        if (fps <= 0)
            throw new ArgumentOutOfRangeException(nameof(fps), "FPS must be positive.");
        return Stopwatch.Frequency / fps;
    }
}
