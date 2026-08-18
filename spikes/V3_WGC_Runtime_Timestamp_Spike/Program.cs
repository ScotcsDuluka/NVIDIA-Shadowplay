// Program.cs — V3 WGC Runtime Timestamp Capture Spike
//
// P1-B.2 V3 — Runtime validation of the approved WGC timestamp contract.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using WinRT;
using Windows.Graphics;
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
        Console.WriteLine("============================================================");
        Console.WriteLine();

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

            FeatureLevel[] featureLevels = { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 };

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

            // === Get primary output ===
            adapter.EnumOutputs(0, out IDXGIOutput output).CheckError();
            var outputDesc = output.Description;
            Console.WriteLine($"  Output: {outputDesc.DeviceName}");
            int desktopW = (int)(outputDesc.DesktopCoordinates.Right - outputDesc.DesktopCoordinates.Left);
            int desktopH = (int)(outputDesc.DesktopCoordinates.Bottom - outputDesc.DesktopCoordinates.Top);
            Console.WriteLine($"  Desktop: {desktopW}x{desktopH}");
            Console.WriteLine();

            // === Get HMONITOR ===
            var rect = new RECT
            {
                Left = (int)outputDesc.DesktopCoordinates.Left,
                Top = (int)outputDesc.DesktopCoordinates.Top,
                Right = (int)outputDesc.DesktopCoordinates.Right,
                Bottom = (int)outputDesc.DesktopCoordinates.Bottom,
            };
            IntPtr hmon = MonitorFromRect(ref rect, 2 /* MONITOR_DEFAULTTONEAREST */);
            if (hmon == IntPtr.Zero)
                throw new InvalidOperationException("Could not get HMONITOR.");
            Console.WriteLine($"  HMONITOR: 0x{hmon.ToInt64():X16}");
            Console.WriteLine();

            // === Create IDirect3DDevice from D3D11 device ===
            IDXGIDevice dxgiDevice = device.QueryInterface<IDXGIDevice>();
            IDirect3DDevice d3dDevice = Direct3D11Helper.CreateDirect3DDeviceFromDXGIDevice(dxgiDevice.NativePointer);
            dxgiDevice.Dispose();
            Console.WriteLine("  IDirect3DDevice created.");
            Console.WriteLine();

            // === Session A ===
            Console.WriteLine("=== SESSION A ===");
            var sessionA = CaptureSession(captureDurationSec, d3dDevice, hmon, desktopW, desktopH, "A");

            // === Session B (restart) ===
            SessionResult? sessionB = null;
            if (doSessionRestart)
            {
                Console.WriteLine();
                Console.WriteLine("=== SESSION B (restart) ===");
                Console.WriteLine("  Disposing session A...");
                Thread.Sleep(1000);
                Console.WriteLine("  Creating session B...");
                sessionB = CaptureSession(captureDurationSec, d3dDevice, hmon, desktopW, desktopH, "B");
            }

            // === Report ===
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" FINAL REPORT");
            Console.WriteLine("============================================================");
            ReportSession("Session A", sessionA);
            if (sessionB.HasValue)
                ReportSession("Session B", sessionB.Value);

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
    // Data structures
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
    }

    // ================================================================
    // Capture Session
    // ================================================================

    private static SessionResult CaptureSession(
        int durationSec,
        IDirect3DDevice d3dDevice,
        IntPtr hmon,
        int width,
        int height,
        string label)
    {
        var frames = new List<FrameRecord>();
        var sw = Stopwatch.StartNew();
        long durationMs = durationSec * 1000L;

        // Create GraphicsCaptureItem via HMONITOR interop
        Console.WriteLine($"[{label}] Creating GraphicsCaptureItem...");
        GraphicsCaptureItem captureItem = CreateCaptureItemFromHmonitor(hmon);
        Console.WriteLine($"[{label}] CaptureItem: {captureItem.Size.Width}x{captureItem.Size.Height}");

        SizeInt32 size = captureItem.Size;

        // Create frame pool (free-threaded so FrameArrived fires on a thread pool thread)
        var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            d3dDevice,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            2,
            size);
        Console.WriteLine($"[{label}] FramePool created: {size.Width}x{size.Height}, 2 buffers");

        var frameReady = new ManualResetEventSlim(false);
        Direct3D11CaptureFrame? latestFrame = null;
        object frameLock = new();

        framePool.FrameArrived += (sender, _) =>
        {
            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = sender.TryGetNextFrame();
                frameReady.Set();
            }
        };

        var session = framePool.CreateCaptureSession(captureItem);
        Console.WriteLine($"[{label}] Starting capture...");
        session.StartCapture();

        long t0 = 0;
        bool t0Set = false;
        long prevTimestamp = 0;

        Console.WriteLine($"[{label}] Capturing for {durationSec} seconds...");

        while (sw.ElapsedMilliseconds < durationMs)
        {
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
            long srt = frame.SystemRelativeTime.Ticks;

            if (!t0Set)
            {
                t0 = srt;
                t0Set = true;
                Console.WriteLine($"[{label}] T0 = {t0} ticks ({t0 / 10_000_000.0:F6} s)");
            }

            long pts = srt - t0;
            long delta = frames.Count > 0 ? srt - prevTimestamp : 0;

            frames.Add(new FrameRecord
            {
                Index = frames.Count,
                SystemRelativeTimeTicks = srt,
                PTS = pts,
                Delta = delta,
            });

            prevTimestamp = srt;

            if (frames.Count % 300 == 0)
            {
                Console.WriteLine($"  [{label}] f{frames.Count,5} | " +
                                  $"SRT={srt,15} | PTS={pts,12} | d={delta,8} | " +
                                  $"{sw.Elapsed.TotalSeconds:F1}s");
            }

            frame.Dispose();
        }

        Console.WriteLine($"[{label}] Stopping...");
        session.Dispose();
        framePool.Dispose();

        Thread.Sleep(500);
        return ComputeResult(label, frames, sw.Elapsed.TotalSeconds);
    }

    // ================================================================
    // Statistics
    // ================================================================

    private static SessionResult ComputeResult(string label, List<FrameRecord> frames, double durationSec)
    {
        var r = new SessionResult { TotalFrames = frames.Count, DurationSec = durationSec };

        if (frames.Count == 0)
        {
            Console.WriteLine($"[{label}] WARNING: No frames captured!");
            return r;
        }

        r.FirstTimestamp = frames[0].SystemRelativeTimeTicks;
        r.LastTimestamp = frames[^1].SystemRelativeTimeTicks;
        r.FirstPTS = frames[0].PTS;
        r.LastPTS = frames[^1].PTS;

        var deltas = frames.Skip(1).Select(f => f.Delta).ToList();
        if (deltas.Count > 0)
        {
            r.MinDelta = deltas.Min();
            r.MaxDelta = deltas.Max();
            r.AvgDelta = deltas.Average();
            var sorted = deltas.OrderBy(d => d).ToList();
            r.MedianDelta = sorted[sorted.Count / 2];
        }

        r.EqualDeltaCount = deltas.Count(d => d == 0);
        r.NegativeDeltaCount = deltas.Count(d => d < 0);
        r.NegativePTSCount = frames.Count(f => f.PTS < 0);

        r.TimestampMonotonic = true;
        r.PTSMonotonic = true;
        for (int i = 1; i < frames.Count; i++)
        {
            if (frames[i].SystemRelativeTimeTicks < frames[i - 1].SystemRelativeTimeTicks)
                r.TimestampMonotonic = false;
            if (frames[i].PTS < frames[i - 1].PTS)
                r.PTSMonotonic = false;
        }

        return r;
    }

    private static void ReportSession(string label, SessionResult r)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {label} ---");
        Console.WriteLine($"  Total frames:             {r.TotalFrames}");
        Console.WriteLine($"  Capture duration:         {r.DurationSec:F2} s");
        Console.WriteLine($"  First timestamp (SRT):    {r.FirstTimestamp}");
        Console.WriteLine($"  Last timestamp (SRT):     {r.LastTimestamp}");
        Console.WriteLine($"  First PTS:                {r.FirstPTS}");
        Console.WriteLine($"  Last PTS:                 {r.LastPTS}");
        Console.WriteLine($"  Last PTS (seconds):       {r.LastPTS / 10_000_000.0:F6}");
        Console.WriteLine($"  Min delta:                {r.MinDelta} ({r.MinDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Max delta:                {r.MaxDelta} ({r.MaxDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Average delta:            {r.AvgDelta:F1} ({r.AvgDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Median delta:             {r.MedianDelta:F1} ({r.MedianDelta / 10_000.0:F3} ms)");
        Console.WriteLine($"  Equal delta count (=0):   {r.EqualDeltaCount}");
        Console.WriteLine($"  Negative delta count:     {r.NegativeDeltaCount}");
        Console.WriteLine($"  Negative PTS count:       {r.NegativePTSCount}");
        Console.WriteLine($"  Timestamp monotonic:      {r.TimestampMonotonic}");
        Console.WriteLine($"  PTS monotonic:            {r.PTSMonotonic}");
        if (r.TotalFrames > 0)
        {
            Console.WriteLine($"  Achieved FPS:             {r.TotalFrames / r.DurationSec:F2}");
            if (r.EqualDeltaCount > 0)
                Console.WriteLine($"  Equal timestamps: YES ({r.EqualDeltaCount}x — ALLOWED, no dedup)");
            else
                Console.WriteLine($"  Equal timestamps: NO (all deltas > 0)");
            if (r.NegativeDeltaCount > 0)
                Console.WriteLine($"  REGRESSION detected: {r.NegativeDeltaCount}x");
        }
    }

    // ================================================================
    // WGC Interop
    // ================================================================

    /// <summary>
    /// Creates a GraphicsCaptureItem from an HMONITOR via the IGraphicsCaptureItemInterop
    /// COM interface. Uses CsWinRT's .As&lt;T&gt;() extension method.
    ///
    /// Based on Microsoft's official CaptureHelper from Windows.UI.Composition-Win32-Samples.
    /// </summary>
    private static GraphicsCaptureItem CreateCaptureItemFromHmonitor(IntPtr hmon)
    {
        var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
        IntPtr itemPointer = interop.CreateForMonitor(hmon, GraphicsCaptureItemGuid);
        GraphicsCaptureItem item = GraphicsCaptureItem.FromAbi(itemPointer);
        Marshal.Release(itemPointer);
        return item;
    }

    // Microsoft official GUID for GraphicsCaptureItem ABI interface.
    // NOTE: This is 632EF5D30760, NOT 632F5FA72F1B (which was wrong).
    private static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        IntPtr CreateForWindow([In] IntPtr window, in Guid iid);
        IntPtr CreateForMonitor([In] IntPtr monitor, in Guid iid);
    }

    // ================================================================
    // Win32 Helpers
    // ================================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);
}
