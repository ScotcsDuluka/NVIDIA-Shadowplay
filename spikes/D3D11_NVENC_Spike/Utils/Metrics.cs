// Utils/Metrics.cs
//
// P1-B.2-V1 Spike — D3D11/NVENC Interop
// Performance metrics collection for Phase 2 and Phase 5.
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace CaptureEngine.Video.Spike.D3D11.Utils;

/// <summary>
/// Tracks per-frame acquisition latency and overall FPS for a capture loop.
/// Uses Stopwatch (QPC-based) — same source as WGC SystemRelativeTime per
// P1-A v1.3.1 §3.6.1 timestamp model.
/// </summary>
public sealed class FrameMetrics : IDisposable
{
    private readonly Stopwatch _totalSw = new();
    private readonly List<double> _frameLatenciesMs = new();
    private int _waitTimeoutCount;
    private int _accessLostCount;
    private int _otherErrorCount;
    private int _framesAcquired;
    private int _framesDropped;
    private bool _disposed;

    public void Start() => _totalSw.Start();

    public void Stop() => _totalSw.Stop();

    public void RecordAcquire(double latencyMs)
    {
        _frameLatenciesMs.Add(latencyMs);
        _framesAcquired++;
    }

    public void RecordWaitTimeout() => _waitTimeoutCount++;
    public void RecordAccessLost() => _accessLostCount++;
    public void RecordOtherError() => _otherErrorCount++;
    public void RecordDropped() => _framesDropped++;

    public CaptureStatsSnapshot Snapshot()
    {
        var totalMs = _totalSw.Elapsed.TotalMilliseconds;
        var fps = totalMs > 0 ? _framesAcquired / (totalMs / 1000.0) : 0;

        double avgLatency = 0, p50 = 0, p95 = 0, p99 = 0, maxLatency = 0, minLatency = 0;
        if (_frameLatenciesMs.Count > 0)
        {
            avgLatency = _frameLatenciesMs.Average();
            minLatency = _frameLatenciesMs.Min();
            maxLatency = _frameLatenciesMs.Max();
            var sorted = _frameLatenciesMs.OrderBy(x => x).ToList();
            p50 = sorted[sorted.Count / 2];
            p95 = sorted[(int)(sorted.Count * 0.95)];
            p99 = sorted[(int)(sorted.Count * 0.99)];
        }

        return new CaptureStatsSnapshot(
            TotalMs: totalMs,
            FramesAcquired: _framesAcquired,
            FramesDropped: _framesDropped,
            WaitTimeoutCount: _waitTimeoutCount,
            AccessLostCount: _accessLostCount,
            OtherErrorCount: _otherErrorCount,
            AchievedFps: fps,
            AvgAcquireLatencyMs: avgLatency,
            MinAcquireLatencyMs: minLatency,
            MaxAcquireLatencyMs: maxLatency,
            P50AcquireLatencyMs: p50,
            P95AcquireLatencyMs: p95,
            P99AcquireLatencyMs: p99);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _frameLatenciesMs.Clear();
    }
}

public sealed record CaptureStatsSnapshot(
    double TotalMs,
    int FramesAcquired,
    int FramesDropped,
    int WaitTimeoutCount,
    int AccessLostCount,
    int OtherErrorCount,
    double AchievedFps,
    double AvgAcquireLatencyMs,
    double MinAcquireLatencyMs,
    double MaxAcquireLatencyMs,
    double P50AcquireLatencyMs,
    double P95AcquireLatencyMs,
    double P99AcquireLatencyMs)
{
    public void PrintToConsole(string label = "")
    {
        if (!string.IsNullOrEmpty(label))
            Console.WriteLine($"--- {label} ---");

        Console.WriteLine($"  Total time:           {TotalMs:F1} ms");
        Console.WriteLine($"  Frames acquired:      {FramesAcquired}");
        Console.WriteLine($"  Frames dropped:       {FramesDropped}");
        Console.WriteLine($"  Achieved FPS:         {AchievedFps:F2}");
        Console.WriteLine($"  WaitTimeout count:    {WaitTimeoutCount}");
        Console.WriteLine($"  AccessLost count:     {AccessLostCount}");
        Console.WriteLine($"  Other errors:         {OtherErrorCount}");
        Console.WriteLine($"  Acquire latency:");
        Console.WriteLine($"    min/avg/max:        {MinAcquireLatencyMs:F3} / {AvgAcquireLatencyMs:F3} / {MaxAcquireLatencyMs:F3} ms");
        Console.WriteLine($"    p50/p95/p99:        {P50AcquireLatencyMs:F3} / {P95AcquireLatencyMs:F3} / {P99AcquireLatencyMs:F3} ms");
    }
}
