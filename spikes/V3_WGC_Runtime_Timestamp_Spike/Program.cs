// Program.cs — V3 WGC Runtime Timestamp Capture Spike
//
// P1-B.2 V3 — Runtime validation of the approved WGC timestamp contract.
//
// APPROVED CONTRACT:
//   GraphicsCaptureFrame.SystemRelativeTime:
//   - QPC-based timestamp
//   - Windows.Foundation.TimeSpan
//   - TimeSpan.Ticks = 100 ns
//
//   PTS = SystemRelativeTime.Ticks - T0
//   T0 = SystemRelativeTime.Ticks of the first accepted WGC frame.
//
// DO NOT multiply by 100.
// DO NOT use Stopwatch.Frequency/QPF on SystemRelativeTime.Ticks.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;

// WinRT types for Windows.Graphics.Capture
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

namespace V3_WGC_Runtime_Timestamp_Spike;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" V3 WGC Runtime Timestamp Capture Spike");
        Console.WriteLine(" Branch: Engine-Rebuild (spike — does NOT modify repo)");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Parse args
        int captureDurationSec = 30;
        bool doSessionRestart = true;
        for (int i = 0; i < args.Length; i++)
        {
            if (int.TryParse(args[i], out int dur) && dur > 0)
                captureDurationSec = dur;
            if (args[i].Equals("--no-restart", StringComparison.OrdinalIgnoreCase))
                doSessionRestart = false;
        }
        Console.WriteLine($"Capture duration per session: {captureDurationSec} seconds");
        Console.WriteLine($"Session restart test: {(doSessionRestart ? "YES" : "NO")}");
        Console.WriteLine();

        try
        {
            // === Setup D3D11 device ===
            Console.WriteLine("[setup] Creating D3D11 device for WGC interop...");
            DXGI.CreateDXGIFactory1(out IDXGIFactory1 factory).CheckError();
            factory.EnumAdapters1(0, out IDXGIAdapter1 adapter).CheckError();

            FeatureLevel[] featureLevels =
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
            };

            ID3D11Device device;
            ID3D11DeviceContext context;
            Vortice.Direct3D11.D3D11.D3D11CreateDevice(
                adapter,
                DriverType.Unknown,
                DeviceCreationFlags.BgraSupport,
                featureLevels,
                out device,
                out context).CheckError();

            Console.WriteLine($"  Device: {adapter.Description1.Description}");
            Console.WriteLine($"  Feature level: {device.FeatureLevel}");
            Console.WriteLine();

            // === Get primary monitor for capture ===
            adapter.EnumOutputs(0, out IDXGIOutput output).CheckError();
            var outputDesc = output.Description;
            Console.WriteLine($"  Output: {outputDesc.DeviceName}");
            int desktopW = (int)(outputDesc.DesktopCoordinates.Right - outputDesc.DesktopCoordinates.Left);
            int desktopH = (int)(outputDesc.DesktopCoordinates.Bottom - outputDesc.DesktopCoordinates.Top);
            Console.WriteLine($"  Desktop: {desktopW}x{desktopH}");
            Console.WriteLine();

            // === Session A: Capture frames ===
            Console.WriteLine("=== SESSION A ===");
            var sessionA = CaptureSession(captureDurationSec, device, output, "A");

            // === Session B: Restart test (optional) ===
            SessionResult? sessionB = null;
            if (doSessionRestart)
            {
                Console.WriteLine();
                Console.WriteLine("=== SESSION B (restart) ===");
                Console.WriteLine("  Disposing session A...");
                // Session A resources are cleaned up in CaptureSession.
                // Small delay to let WGC fully release.
                Thread.Sleep(1000);

                Console.WriteLine("  Creating session B...");
                sessionB = CaptureSession(captureDurationSec, device, output, "B");
            }

            // === Report ===
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" FINAL REPORT");
            Console.WriteLine("============================================================");
            ReportSession("Session A", sessionA);
            if (sessionB.HasValue)
                ReportSession("Session B", sessionB.Value);

            // Cleanup
            context.Dispose();
            device.Dispose();
            output.Dispose();
            adapter.Dispose();
            factory.Dispose();

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    // ================================================================
    // Capture Session
    // ================================================================

    private struct FrameRecord
    {
        public int Index;
        public long SystemRelativeTimeTicks;
        public long PTS;
        public long Delta;
    }

    private struct SessionResult
    {
        public int TotalFrames;
        public double DurationSec;
        public long FirstTimestamp;
        public long LastTimestamp;
        public long FirstPTS;
        public long LastPTS;
        public long MinDelta;
        public long MaxDelta;
        public double AvgDelta;
        public double MedianDelta;
        public int EqualDeltaCount;
        public int NegativeDeltaCount;
        public int NegativePTSCount;
        public bool TimestampMonotonic;
        public bool PTSMonotonic;
        public List<FrameRecord> Frames;
    }

    /// <summary>
    /// Captures WGC frames for the specified duration, recording
    /// SystemRelativeTime.Ticks for each frame.
    /// </summary>
    private static SessionResult CaptureSession(
        int durationSec,
        ID3D11Device device,
        IDXGIOutput output,
        string label)
    {
        var frames = new List<FrameRecord>();
        var sw = Stopwatch.StartNew();
        long durationMs = durationSec * 1000L;

        // Get the GraphicsCaptureItem for this output
        var interopFactory = (IGraphicsCaptureItemInterop)WinRT.CastConversion.As<object>(
            WindowsRuntimeMarshal.GetActivationFactory(typeof(GraphicsCaptureItem)));
        // Alternative: use HMONITOR interop
        IntPtr hmon = output.NativePointer; // Not exactly right — need MonitorFromWindow or similar

        // Actually, let's use the HMONITOR from the output's description
        // WGC requires an HMONITOR, not an IDXGIOutput.
        // We'll use the Win32 API to get the HMONITOR for the output's coordinates.
        var outputDesc = output.Description;
        // Use MonitorFromRect or similar — but we don't have user32.dll import.
        // Simpler: use the DXGI output's GetDesc to find the monitor handle.

        // For .NET 8 with Windows.Graphics.Capture, we can use the
        // GraphicsCaptureItem.CreateFromMonitor approach if available, or
        // the interop approach. Let's use the simplest method.

        // Actually, in .NET 8 with net8.0-windows10.0.19041.0 target,
        // we might have direct access to GraphicsCaptureItem.CreateFromMonitor.
        // If not, we'll use the HMONITOR interop.

        Console.WriteLine($"[{label}] Setting up GraphicsCaptureItem...");

        // Get HMONITOR from IDXGIOutput via GetDesc
        // The IDXGIOutput::GetDesc gives us the desktop coordinates,
        // but we need the HMONITOR. We can use MonitorFromRect from user32.
        // However, there's a simpler way: cast IDXGIOutput to IDXGIOutput6 and
        // use GetDesc1 which includes the monitor handle... but that's not standard.
        //
        // The simplest approach for WGC: use the interop method.

        // For now, let's create the capture item via the interop interface
        GraphicsCaptureItem captureItem = CreateCaptureItemFromOutput(output);

        Console.WriteLine($"[{label}] CaptureItem: {captureItem.Size.Width}x{captureItem.Size.Height}");

        // Create Direct3D11 device for WGC
        // WGC needs IDirect3DDevice (WinRT), not ID3D11Device (COM).
        // We need to create an IDirect3DDevice from our ID3D11Device.
        IDirect3DDevice d3dDevice = CreateDirect3DDeviceFromD3D11Device(device);

        Console.WriteLine($"[{label}] IDirect3DDevice created.");

        // Create a frame pool
        // Direct3D11CaptureFramePool.Create() gives us real GPU frames
        SizeInt32 size = captureItem.Size;
        var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            d3dDevice,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            2,  // number of buffers
            size);

        Console.WriteLine($"[{label}] FramePool created: {size.Width}x{size.Height}, 2 buffers");

        // === Capture loop ===
        // Use a manual event to wait for frames
        var frameReady = new ManualResetEventSlim(false);
        Direct3D11CaptureFrame? latestFrame = null;
        object frameLock = new();

        framePool.FrameArrived += (sender, e) =>
        {
            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = sender.TryGetNextFrame();
                frameReady.Set();
            }
        };

        var session = framePool.CreateCaptureSession(captureItem);
        Console.WriteLine($"[{label}] Starting capture session...");
        session.StartCapture();

        long t0 = 0;
        bool t0Set = false;
        long prevTimestamp = 0;

        Console.WriteLine($"[{label}] Capturing for {durationSec} seconds...");

        while (sw.ElapsedMilliseconds < durationMs)
        {
            // Wait for a frame (non-blocking, short timeout for responsiveness)
            if (!frameReady.Wait(50))
                continue;

            frameReady.Reset();

            Direct3D11CaptureFrame? frame;
            lock (frameLock)
            {
                frame = latestFrame;
                latestFrame = null;
            }

            if (frame == null)
                continue;

            // === THE CRITICAL MEASUREMENT ===
            // SystemRelativeTime is a Windows.Foundation.TimeSpan
            // TimeSpan.Ticks = 100-ns units
            long systemRelativeTimeTicks = frame.SystemRelativeTime.Ticks;

            // PTS = Tn - T0
            if (!t0Set)
            {
                t0 = systemRelativeTimeTicks;
                t0Set = true;
                Console.WriteLine($"[{label}] T0 set: {t0} ticks ({t0 / 10_000_000.0:F6} seconds)");
            }

            long pts = systemRelativeTimeTicks - t0;
            long delta = t0Set && frames.Count > 0
                ? systemRelativeTimeTicks - prevTimestamp
                : 0;

            frames.Add(new FrameRecord
            {
                Index = frames.Count,
                SystemRelativeTimeTicks = systemRelativeTimeTicks,
                PTS = pts,
                Delta = delta,
            });

            prevTimestamp = systemRelativeTimeTicks;

            // Progress output every 5 seconds
            if (frames.Count % (300) == 0 || sw.ElapsedMilliseconds % 5000 < 100)
            {
                Console.WriteLine($"  [{label}] frame {frames.Count,5} | " +
                                  $"SRT={systemRelativeTimeTicks,15} | " +
                                  $"PTS={pts,12} | " +
                                  $"delta={delta,8} | " +
                                  $"elapsed={sw.Elapsed.TotalSeconds:F1}s");
            }

            frame.Dispose();
        }

        // === Cleanup session ===
        Console.WriteLine($"[{label}] Stopping capture...");
        session.Dispose();
        framePool.Dispose();
        captureItem.Dispose();

        // Wait a moment for cleanup
        Thread.Sleep(500);

        // === Compute statistics ===
        return ComputeSessionResult(label, frames, sw.Elapsed.TotalSeconds, t0Set, t0);
    }

    /// <summary>
    /// Computes all required statistics from the captured frames.
    /// </summary>
    private static SessionResult ComputeSessionResult(
        string label,
        List<FrameRecord> frames,
        double durationSec,
        bool t0Set,
        long t0)
    {
        var result = new SessionResult
        {
            Frames = frames,
            TotalFrames = frames.Count,
            DurationSec = durationSec,
        };

        if (frames.Count == 0)
        {
            Console.WriteLine($"[{label}] WARNING: No frames captured!");
            return result;
        }

        result.FirstTimestamp = frames[0].SystemRelativeTimeTicks;
        result.LastTimestamp = frames[frames.Count - 1].SystemRelativeTimeTicks;
        result.FirstPTS = frames[0].PTS;
        result.LastPTS = frames[frames.Count - 1].PTS;

        // Delta statistics (skip frame 0 which has delta=0)
        var deltas = frames.Skip(1).Select(f => f.Delta).ToList();
        if (deltas.Count > 0)
        {
            result.MinDelta = deltas.Min();
            result.MaxDelta = deltas.Max();
            result.AvgDelta = deltas.Average();
            var sorted = deltas.OrderBy(d => d).ToList();
            result.MedianDelta = sorted[sorted.Count / 2];
        }

        // Count equal/negative deltas
        result.EqualDeltaCount = deltas.Count(d => d == 0);
        result.NegativeDeltaCount = deltas.Count(d => d < 0);

        // Count negative PTS
        result.NegativePTSCount = frames.Count(f => f.PTS < 0);

        // Monotonicity checks
        result.TimestampMonotonic = true;
        result.PTSMonotonic = true;
        for (int i = 1; i < frames.Count; i++)
        {
            if (frames[i].SystemRelativeTimeTicks < frames[i - 1].SystemRelativeTimeTicks)
                result.TimestampMonotonic = false;
            if (frames[i].PTS < frames[i - 1].PTS)
                result.PTSMonotonic = false;
        }

        return result;
    }

    /// <summary>
    /// Prints the session report with all required metrics.
    /// </summary>
    private static void ReportSession(string label, SessionResult r)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {label} ---");
        Console.WriteLine($"  Total frames:             {r.TotalFrames}");
        Console.WriteLine($"  Capture duration:         {r.DurationSec:F2} seconds");
        Console.WriteLine($"  First timestamp (SRT):    {r.FirstTimestamp}");
        Console.WriteLine($"  Last timestamp (SRT):     {r.LastTimestamp}");
        Console.WriteLine($"  First PTS:                {r.FirstPTS}");
        Console.WriteLine($"  Last PTS:                 {r.LastPTS}");
        Console.WriteLine($"  Last PTS (seconds):       {r.LastPTS / 10_000_000.0:F6}");
        Console.WriteLine($"  Min delta:                {r.MinDelta} ticks ({r.MinDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Max delta:                {r.MaxDelta} ticks ({r.MaxDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Average delta:            {r.AvgDelta:F1} ticks ({r.AvgDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Median delta:             {r.MedianDelta:F1} ticks ({r.MedianDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Equal delta count (=0):   {r.EqualDeltaCount}");
        Console.WriteLine($"  Negative delta count:     {r.NegativeDeltaCount}");
        Console.WriteLine($"  Negative PTS count:        {r.NegativePTSCount}");
        Console.WriteLine($"  Timestamp monotonic:      {r.TimestampMonotonic}");
        Console.WriteLine($"  PTS monotonic:            {r.PTSMonotonic}");

        if (r.TotalFrames > 0)
        {
            Console.WriteLine($"  Achieved FPS:             {r.TotalFrames / r.DurationSec:F2}");
            Console.WriteLine($"  First frame SRT (seconds):{r.FirstTimestamp / 10_000_000.0:F6}");

            // Report equal timestamps if any
            if (r.EqualDeltaCount > 0)
            {
                Console.WriteLine($"  Equal timestamps occurred: YES ({r.EqualDeltaCount} times)");
                Console.WriteLine("    (This is ALLOWED — no dedup/drop policy applied)");
            }
            else
            {
                Console.WriteLine($"  Equal timestamps occurred: NO (all deltas > 0)");
            }

            // Report regressions if any
            if (r.NegativeDeltaCount > 0)
            {
                Console.WriteLine($"  TIMESTAMP REGRESSION detected: {r.NegativeDeltaCount} times");
                Console.WriteLine("    (Timestamp went backward — evidence of regression)");
            }
        }
    }

    // ================================================================
    // WGC Interop Helpers
    // ================================================================

    /// <summary>
    /// Creates a GraphicsCaptureItem from an IDXGIOutput.
    /// Uses the HMONITOR interop interface.
    /// </summary>
    private static GraphicsCaptureItem CreateCaptureItemFromOutput(IDXGIOutput output)
    {
        // Get the HMONITOR from the IDXGIOutput
        // We need to find the monitor that matches the output's coordinates.
        // The simplest way: use MonitorFromRect from user32.dll.

        var desc = output.Description;
        // Create a RECT from the output's desktop coordinates
        var left = (int)desc.DesktopCoordinates.Left;
        var top = (int)desc.DesktopCoordinates.Top;
        var right = (int)desc.DesktopCoordinates.Right;
        var bottom = (int)desc.DesktopCoordinates.Bottom;

        IntPtr hmon = MonitorFromRect(left, top, right, bottom);
        if (hmon == IntPtr.Zero)
            throw new InvalidOperationException("Could not get HMONITOR from IDXGIOutput coordinates.");

        return GraphicsCaptureItem.CreateFromMonitor(hmon);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private static IntPtr MonitorFromRect(int left, int top, int right, int bottom)
    {
        var rect = new RECT { Left = left, Top = top, Right = right, Bottom = bottom };
        return MonitorFromRect(ref rect, 2 /* MONITOR_DEFAULTTONEAREST */);
    }

    /// <summary>
    /// Creates an IDirect3DDevice (WinRT) from an ID3D11Device (COM).
    /// WGC requires the WinRT IDirect3DDevice interface.
    /// </summary>
    private static IDirect3DDevice CreateDirect3DDeviceFromD3D11Device(ID3D11Device d3d11Device)
    {
        // We need to use the IDXGIDevice to create the IDirect3DDevice.
        // The WinRT CreateDirect3D11DeviceFromDXGIDevice API does this.
        IDXGIDevice dxgiDevice = d3d11Device.QueryInterface<IDXGIDevice>();

        // Use the WinRT interop to create IDirect3DDevice from IDXGIDevice
        var dxgiDevicePtr = dxgiDevice.NativePointer;
        var iid = typeof(IDirect3DDevice).GUID;

        // Use Direct3D11Helper.CreateDirect3DDeviceFromDXGIDevice
        // This is available via the Windows.Graphics.DirectX.Direct3D11 interop
        var d3dDevice = Direct3D11Helper.CreateDirect3DDeviceFromDXGIDevice(dxgiDevicePtr);

        dxgiDevice.Dispose();
        return d3dDevice;
    }
}
