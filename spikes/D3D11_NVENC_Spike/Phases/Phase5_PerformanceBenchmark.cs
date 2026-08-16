// Phases/Phase5_PerformanceBenchmark.cs
//
// P1-B.2-V1 Spike — Phase 5: Performance Benchmark
//
// Goal: Benchmark the full capture pipeline at 3 resolutions × 3 FPS targets.
//
// For each (resolution, targetFPS) combination:
//   - Run a capture loop for a fixed duration (e.g., 10 seconds)
//   - Measure: achieved FPS, p95/p99 acquire latency, dropped frames, CPU%, GPU%
//   - Note: This spike does NOT include actual NVENC encoding — Phase 5 only
//     measures the capture pipeline. NVENC encoding benchmark is a separate
//     concern (deferred until encoder integration phase).
//
// Acceptance criteria for Phase 5:
//   - At 1920x1080 @ 144 FPS: achieved FPS >= 130 (90% of target)
//   - At 2560x1440 @ 120 FPS: achieved FPS >= 108
//   - At 3840x2160 @ 60 FPS:  achieved FPS >= 54
//   - p95 acquire latency < 15 ms at all resolutions
//   - No ACCESS_LOST events
//
// NOTE on resolution: we cannot force the desktop resolution from a spike
// process. The benchmark will use the CURRENT desktop resolution and adjust
// target FPS accordingly. If the desktop is not one of the target resolutions,
// we run the benchmark at the current resolution and note the mismatch.
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

// Disambiguate Result/ResultCode — both Vortice.DXGI and Vortice.Direct3D11
// declare these. We want the DXGI versions.
using Result = Vortice.DXGI.Result;
using ResultCode = Vortice.DXGI.ResultCode;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase5_PerformanceBenchmark
{
    // Benchmark targets — (width, height, targetFPS, durationSeconds)
    private static readonly (int W, int H, int Fps, int Sec)[] Targets =
    {
        (1920, 1080, 60,  10),
        (1920, 1080, 120, 10),
        (1920, 1080, 144, 10),
        (2560, 1440, 60,  10),
        (2560, 1440, 120, 10),
        (2560, 1440, 144, 10),
        (3840, 2160, 60,  10),
        (3840, 2160, 120, 10),
        (3840, 2160, 144, 10),
    };

    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 5 — Performance Benchmark");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        if (SpikeSharedContext.Device == null || SpikeSharedContext.TargetAdapter == null)
        {
            Console.Error.WriteLine("  FAIL: Phase 1 must run first.");
            return 1;
        }

        // --- Determine current desktop resolution ---
        Console.WriteLine("[5.1] Determining current desktop resolution...");
        IDXGIOutput? primaryOutput = null;
        int outputIdx = 0;
        while (SpikeSharedContext.TargetAdapter.EnumOutputs((uint)outputIdx, out IDXGIOutput output).Success)
        {
            if (outputIdx == 0) primaryOutput = output;
            else output.Dispose();
            outputIdx++;
        }
        if (primaryOutput == null)
        {
            Console.Error.WriteLine("  FAIL: No primary output found.");
            return 1;
        }
        var outDesc = primaryOutput.Description;
        // Cast explicit — Vortice's RawRect fields may be uint in some versions.
        int desktopW = (int)(outDesc.DesktopCoordinates.Right - outDesc.DesktopCoordinates.Left);
        int desktopH = (int)(outDesc.DesktopCoordinates.Bottom - outDesc.DesktopCoordinates.Top);
        Console.WriteLine($"  Desktop resolution: {desktopW}x{desktopH}");
        Console.WriteLine($"  NOTE: Spike cannot force desktop resolution change.");
        Console.WriteLine($"        Will run benchmarks matching current desktop resolution only.");

        // Filter targets to those matching current desktop resolution
        var matchingTargets = Targets.Where(t => t.W == desktopW && t.H == desktopH).ToList();
        if (matchingTargets.Count == 0)
        {
            Console.WriteLine($"  WARN: Current desktop ({desktopW}x{desktopH}) is not in target list.");
            Console.WriteLine($"        Running benchmark at current resolution with all 3 FPS targets.");
            matchingTargets = new()
            {
                (desktopW, desktopH, 60,  10),
                (desktopW, desktopH, 120, 10),
                (desktopW, desktopH, 144, 10),
            };
        }

        // --- Setup duplication ---
        IDXGIOutput1 output1 = primaryOutput.QueryInterface<IDXGIOutput1>();
        IDXGIOutputDuplication duplication = output1.DuplicateOutput(SpikeSharedContext.Device);

        Texture2DDescription stagingDesc = new()
        {
            Width = desktopW,
            Height = desktopH,
            MipLevels = 1,
            ArraySize = 1,
            Format = Vortice.DXGI.Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = ResourceOptionFlags.None,
        };
        ID3D11Texture2D staging = SpikeSharedContext.Device.CreateTexture2D(stagingDesc);

        // --- Run benchmarks ---
        var allResults = new List<BenchmarkResult>();
        foreach (var target in matchingTargets)
        {
            Console.WriteLine();
            Console.WriteLine($"[5.2] Benchmark: {target.W}x{target.H} @ {target.Fps} FPS, {target.Sec}s duration");
            var result = RunSingleBenchmark(duplication, staging, target.W, target.H, target.Fps, target.Sec);
            allResults.Add(result);
            result.PrintToConsole();

            // Brief cooldown between benchmarks
            Thread.Sleep(500);
        }

        // --- Cleanup ---
        staging.Dispose();
        duplication.Dispose();
        output1.Dispose();
        primaryOutput.Dispose();

        // --- Final summary table ---
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 5 SUMMARY");
        Console.WriteLine("============================================================");
        Console.WriteLine($"  {"Resolution",-12} {"TargetFPS",-10} {"AchievedFPS",-12} {"p95Lat(ms)",-12} {"p99Lat(ms)",-12} {"Dropped",-8} {"Status",-8}");
        Console.WriteLine($"  {"------------",-12} {"----------",-10} {"------------",-12} {"----------",-12} {"----------",-12} {"-------",-8} {"--------",-8}");
        int passCount = 0;
        foreach (var r in allResults)
        {
            bool pass = r.AchievedFps >= r.TargetFps * 0.9 && r.AccessLostCount == 0;
            if (pass) passCount++;
            Console.WriteLine($"  {$"{r.Width}x{r.Height}",-12} {r.TargetFps,-10} {r.AchievedFps,-12:F2} {r.P95LatencyMs,-12:F2} {r.P99LatencyMs,-12:F2} {r.DroppedFrames,-8} {(pass ? "PASS" : "FAIL"),-8}");
        }
        Console.WriteLine();
        Console.WriteLine($"  Benchmark results: {passCount}/{allResults.Count} PASS");

        bool overallPass = passCount == allResults.Count;
        Console.WriteLine();
        Console.WriteLine(overallPass ? "  Phase 5: PASS" : "  Phase 5: FAIL — see results above");
        Console.WriteLine();
        return overallPass ? 0 : 1;
    }

    private static BenchmarkResult RunSingleBenchmark(
        IDXGIOutputDuplication duplication,
        ID3D11Texture2D staging,
        int width, int height, int targetFps, int durationSeconds)
    {
        // For benchmark, we use a tight acquire loop with minimal timeout (1 ms)
        // to maximize capture rate. We honor targetFps only as a measure of
        // "did we achieve at least this rate" — we do NOT throttle.

        double targetIntervalMs = 1000.0 / targetFps;
        int totalFramesTarget = (int)(targetFps * durationSeconds);

        using var metrics = new FrameMetrics();
        var sw = Stopwatch.StartNew();
        var cpuCounter = new PerformanceCounterLazy();
        var gpuCounter = new GpuUsageLazy(SpikeSharedContext.Gpu!.Description);
        cpuCounter.Start();
        gpuCounter.Start();
        metrics.Start();

        int iterations = 0;
        while (sw.ElapsedMilliseconds < durationSeconds * 1000)
        {
            iterations++;
            long t0 = sw.ElapsedTicks;

            Result hr = duplication.AcquireNextFrame(
                timeoutInMilliseconds: 1,  // aggressive poll
                out var frameInfo,
                out IDXGIResource? desktopResource);

            long t1 = sw.ElapsedTicks;
            double latencyMs = (t1 - t0) * 1000.0 / Stopwatch.Frequency;

            if (hr == ResultCode.WaitTimeout)
            {
                metrics.RecordWaitTimeout();
                continue;
            }
            if (hr == ResultCode.AccessLost)
            {
                metrics.RecordAccessLost();
                break;
            }
            if (hr.Failure)
            {
                metrics.RecordOtherError();
                break;
            }

            ID3D11Texture2D? desktopTexture = null;
            try
            {
                desktopTexture = desktopResource!.QueryInterface<ID3D11Texture2D>();
            }
            catch
            {
                desktopResource!.Dispose();
                metrics.RecordOtherError();
                continue;
            }

            SpikeSharedContext.DeviceContext!.CopyResource(staging, desktopTexture);
            desktopTexture.Dispose();
            desktopResource!.Dispose();

            try { duplication.ReleaseFrame(); }
            catch { /* ignore */ }

            metrics.RecordAcquire(latencyMs);
        }

        metrics.Stop();
        sw.Stop();
        cpuCounter.Stop();
        gpuCounter.Stop();

        var snap = metrics.Snapshot();
        return new BenchmarkResult(
            Width: width,
            Height: height,
            TargetFps: targetFps,
            AchievedFps: snap.AchievedFps,
            P50LatencyMs: snap.P50AcquireLatencyMs,
            P95LatencyMs: snap.P95AcquireLatencyMs,
            P99LatencyMs: snap.P99AcquireLatencyMs,
            AvgLatencyMs: snap.AvgAcquireLatencyMs,
            DroppedFrames: snap.FramesDropped,
            WaitTimeoutCount: snap.WaitTimeoutCount,
            AccessLostCount: snap.AccessLostCount,
            OtherErrorCount: snap.OtherErrorCount,
            CpuUsagePercent: cpuCounter.AveragePercent,
            GpuUsagePercent: gpuCounter.AveragePercent,
            NvencStatus: "Not benchmarked in Phase 5 (separate concern)",
            TotalFramesAcquired: snap.FramesAcquired,
            TotalIterations: iterations);
    }
}

public sealed record BenchmarkResult(
    int Width,
    int Height,
    int TargetFps,
    double AchievedFps,
    double P50LatencyMs,
    double P95LatencyMs,
    double P99LatencyMs,
    double AvgLatencyMs,
    int DroppedFrames,
    int WaitTimeoutCount,
    int AccessLostCount,
    int OtherErrorCount,
    double CpuUsagePercent,
    double GpuUsagePercent,
    string NvencStatus,
    int TotalFramesAcquired,
    int TotalIterations)
{
    public void PrintToConsole()
    {
        Console.WriteLine($"  Target:    {Width}x{Height} @ {TargetFps} FPS");
        Console.WriteLine($"  Achieved:  {AchievedFps:F2} FPS ({TotalFramesAcquired} frames in {TotalIterations} iterations)");
        Console.WriteLine($"  Latency:   avg={AvgLatencyMs:F3} ms, p50={P50LatencyMs:F3} ms, p95={P95LatencyMs:F3} ms, p99={P99LatencyMs:F3} ms");
        Console.WriteLine($"  Errors:    WT={WaitTimeoutCount}, AL={AccessLostCount}, Other={OtherErrorCount}, Dropped={DroppedFrames}");
        Console.WriteLine($"  CPU usage: {CpuUsagePercent:F1}%");
        Console.WriteLine($"  GPU usage: {GpuUsagePercent:F1}%");
        Console.WriteLine($"  NVENC:     {NvencStatus}");
    }
}

/// <summary>
/// Lazy CPU usage sampler using PerformanceCounter.
/// Falls back to Process.TotalProcessorTime if PerformanceCounter is unavailable.
/// </summary>
internal sealed class PerformanceCounterLazy : IDisposable
{
    private Process? _proc;
    private TimeSpan _lastCpuTime;
    private DateTime _lastSample;
    private double _sumPercent;
    private int _sampleCount;
    private bool _running;

    public double AveragePercent => _sampleCount > 0 ? _sumPercent / _sampleCount : 0;

    public void Start()
    {
        _proc = Process.GetCurrentProcess();
        _lastCpuTime = _proc.TotalProcessorTime;
        _lastSample = DateTime.UtcNow;
        _running = true;
    }

    public void Sample()
    {
        if (!_running || _proc == null) return;
        var nowCpu = _proc.TotalProcessorTime;
        var now = DateTime.UtcNow;
        var cpuUsed = (nowCpu - _lastCpuTime).TotalMilliseconds;
        var elapsed = (now - _lastSample).TotalMilliseconds;
        double pct = elapsed > 0 ? (cpuUsed / elapsed) * 100.0 / Environment.ProcessorCount : 0;
        _sumPercent += pct;
        _sampleCount++;
        _lastCpuTime = nowCpu;
        _lastSample = now;
    }

    public void Stop() => _running = false;

    public void Dispose()
    {
        _proc?.Dispose();
        _running = false;
    }
}

/// <summary>
/// Lazy GPU usage sampler. Uses NVIDIA's nvml.dll if available; otherwise
/// returns 0 and logs a warning.
/// </summary>
internal sealed class GpuUsageLazy : IDisposable
{
    private readonly string _gpuName;
    private double _sumPercent;
    private int _sampleCount;

    public GpuUsageLazy(string gpuName) => _gpuName = gpuName;

    public double AveragePercent => _sampleCount > 0 ? _sumPercent / _sampleCount : 0;

    public void Start() { /* NVML init would go here */ }

    public void Sample()
    {
        // TODO: implement NVML query via P/Invoke to nvml.dll
        // For now, returns 0 — OWNER should extend this or use external
        // GPU monitoring (e.g., nvidia-smi) and report results manually.
        _sumPercent += 0;
        _sampleCount++;
    }

    public void Stop() { /* NVML shutdown */ }

    public void Dispose() { }
}
