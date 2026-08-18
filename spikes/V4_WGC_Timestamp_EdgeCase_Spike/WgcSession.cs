// WgcSession.cs — single-session capture wrapper
// SPDX-License-Identifier: MIT
// Standalone class — does NOT import or share state with V1/V2/V3.

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

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal sealed class WgcSession : IDisposable
{
    private static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    private ID3D11Device? _d3dDevice;
    private IDirect3DDevice? _winrtDevice;
    private GraphicsCaptureItem? _captureItem;
    private Direct3D11CaptureFramePool? _framePool;
    private GraphicsCaptureSession? _session;
    private IDXGIFactory1? _factory;
    private IDXGIAdapter1? _adapter;
    private IDXGIOutput? _output;

    public string DisplayConfig { get; private set; } = "";
    public int Width { get; private set; }
    public int Height { get; private set; }

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        IntPtr CreateForWindow([In] IntPtr window, in Guid iid);
        IntPtr CreateForMonitor([In] IntPtr monitor, in Guid iid);
    }

    /// <summary>
    /// Sets up WGC interop. Creates D3D11 device + WinRT IDirect3DDevice + capture item + frame pool.
    /// Does NOT start capture — call StartCapture() to begin.
    /// </summary>
    public void Setup()
    {
        DXGI.CreateDXGIFactory1(out _factory).CheckError();
        _factory!.EnumAdapters1(0, out _adapter).CheckError();

        FeatureLevel[] levels = { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 };
        // Explicit types on all out params to disambiguate overload resolution.
        // Vortice has multiple overloads; without explicit types the compiler
        // cannot determine which to call (CS0121 ambiguous overload).
        ID3D11Device device;
        ID3D11DeviceContext context;
        Vortice.Direct3D11.D3D11.D3D11CreateDevice(
            _adapter!, DriverType.Unknown, DeviceCreationFlags.BgraSupport,
            levels, out device, out context).CheckError();
        _d3dDevice = device;

        _adapter!.EnumOutputs(0, out _output).CheckError();
        var desc = _output!.Description;
        Width = (int)(desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left);
        Height = (int)(desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top);
        DisplayConfig = $"{Width}x{Height}@{desc.DeviceName}";

        var rect = new RECT
        {
            Left = (int)desc.DesktopCoordinates.Left,
            Top = (int)desc.DesktopCoordinates.Top,
            Right = (int)desc.DesktopCoordinates.Right,
            Bottom = (int)desc.DesktopCoordinates.Bottom,
        };
        IntPtr hmon = MonitorFromRect(ref rect, 2);
        if (hmon == IntPtr.Zero)
            throw new InvalidOperationException("MonitorFromRect failed.");

        IDXGIDevice dxgiDevice = _d3dDevice!.QueryInterface<IDXGIDevice>();
        _winrtDevice = Direct3D11Helper.CreateDirect3DDeviceFromDXGIDevice(dxgiDevice.NativePointer);
        dxgiDevice.Dispose();

        _captureItem = CreateCaptureItemFromHmonitor(hmon);
        var size = _captureItem.Size;

        _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            _winrtDevice, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, size);
        _session = _framePool.CreateCaptureSession(_captureItem);
    }

    /// <summary>
    /// Starts capture and collects frames for the specified duration.
    /// Returns all captured frames.
    /// </summary>
    public List<FrameRecord> Capture(int durationSec, string loadCondition)
    {
        if (_framePool == null || _session == null)
            throw new InvalidOperationException("WgcSession not set up. Call Setup() first.");

        var frames = new List<FrameRecord>();
        var sw = Stopwatch.StartNew();
        long durationMs = durationSec * 1000L;
        var frameReady = new ManualResetEventSlim(false);
        Direct3D11CaptureFrame? latestFrame = null;
        object frameLock = new();

        _framePool.FrameArrived += (sender, _) =>
        {
            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = sender.TryGetNextFrame();
                frameReady.Set();
            }
        };

        Console.WriteLine($"  Starting capture ({loadCondition}, {durationSec}s)...");
        _session.StartCapture();

        long t0 = 0;
        bool t0Set = false;
        long prevSrt = 0;
        long prevPts = 0;

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

            long srt = frame.SystemRelativeTime.Ticks;

            if (!t0Set)
            {
                t0 = srt;
                t0Set = true;
                Console.WriteLine($"  T0 = {t0} ticks ({t0 / 10_000_000.0:F6} s)");
            }

            long pts = srt - t0;
            long deltaSrt = frames.Count > 0 ? srt - prevSrt : long.MinValue;
            long deltaPts = frames.Count > 0 ? pts - prevPts : long.MinValue;

            frames.Add(new FrameRecord
            {
                FrameIndex = frames.Count,
                SystemRelativeTimeTicks = srt,
                DeltaFromPreviousSrtTicks = deltaSrt,
                Pts = pts,
                DeltaFromPreviousPtsTicks = deltaPts,
                WallClockUtcCaptured = DateTime.UtcNow,
            });

            prevSrt = srt;
            prevPts = pts;

            if (frames.Count % 500 == 0)
            {
                Console.WriteLine($"    f{frames.Count,6} | SRT={srt,16} | PTS={pts,12} | d={deltaSrt,8} | {sw.Elapsed.TotalSeconds:F1}s");
            }

            frame.Dispose();
        }

        Console.WriteLine($"  Capture ended: {frames.Count} frames in {sw.Elapsed.TotalSeconds:F2}s");
        _session.Dispose();
        _framePool.Dispose();
        return frames;
    }

    private static GraphicsCaptureItem CreateCaptureItemFromHmonitor(IntPtr hmon)
    {
        var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
        IntPtr itemPointer = interop.CreateForMonitor(hmon, GraphicsCaptureItemGuid);
        GraphicsCaptureItem item = GraphicsCaptureItem.FromAbi(itemPointer);
        Marshal.Release(itemPointer);
        return item;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

    public void Dispose()
    {
        _session?.Dispose();
        _framePool?.Dispose();
        _captureItem = null;
        _winrtDevice = null;
        _d3dDevice?.Dispose();
        _output?.Dispose();
        _adapter?.Dispose();
        _factory?.Dispose();
    }
}
