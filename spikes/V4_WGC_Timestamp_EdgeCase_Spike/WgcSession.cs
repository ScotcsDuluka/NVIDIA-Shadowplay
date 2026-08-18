// WgcSession.cs — single-session capture wrapper
// SPDX-License-Identifier: MIT
// Standalone class — does NOT import or share state with V1/V2/V3.
//
// ACQUISITION DESIGN:
//   FrameArrived callback → TryGetNextFrame → enqueue to bounded BlockingCollection
//   Consumer thread → dequeue → extract timestamp → dispose frame → add to List
//
//   Counters:
//     FrameArrivedCount       — incremented every time FrameArrived fires
//     TryGetNextFrameCount    — incremented every time TryGetNextFrame is called
//     AcquiredFrameCount      — incremented when TryGetNextFrame returns non-null
//     ConsumedFrameCount      — incremented when consumer dequeues + processes a frame
//     DroppedByHarnessCount   — incremented when queue is full and frame must be dropped
//     NoFrameReturnedCount    — incremented when TryGetNextFrame returns null
//                               (WGC had no new frame to give; does NOT imply supersession)
//     ShutdownDiscardedCount  — incremented when a frame is acquired but the queue
//                               has been closed (CompleteAdding) during shutdown
//
//   Invariant: AcquiredFrameCount = ConsumedFrameCount + DroppedByHarnessCount + ShutdownDiscardedCount
//   (every acquired frame is either consumed, dropped by harness, or discarded during shutdown)
//
// SYNCHRONIZATION DESIGN:
//   Uses BlockingCollection (bounded) — no ManualResetEventSlim, no lost-signal race.
//   BlockingCollection is thread-safe, bounded, and supports cancellation.
//   Producer (FrameArrived callback) adds to collection; consumer (main thread) takes.
//   If collection is full, producer drops the frame and increments DroppedByHarnessCount.

using System.Collections.Concurrent;
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

    // Bounded queue capacity — keeps memory usage bounded.
    private const int QueueCapacity = 16;

    public string DisplayConfig { get; private set; } = "";
    public double RefreshRate { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    // === Acquisition counters (per session) ===
    // Properties are read-only to external callers; raw fields are used
    // internally for Interlocked operations.
    public long FrameArrivedCount { get; private set; }
    public long TryGetNextFrameCount { get; private set; }
    public long AcquiredFrameCount { get; private set; }
    public long ConsumedFrameCount { get; private set; }
    public long DroppedByHarnessCount { get; private set; }
    public long NoFrameReturnedCount { get; private set; }
    public long ShutdownDiscardedCount { get; private set; }

    // Raw fields for Interlocked — reset at the start of each Capture() call.
    private long _frameArrivedRaw;
    private long _tryGetNextFrameRaw;
    private long _acquiredRaw;
    private long _consumedRaw;
    private long _droppedByHarnessRaw;
    private long _noFrameReturnedRaw;
    private long _shutdownDiscardedRaw;

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        IntPtr CreateForWindow([In] IntPtr window, in Guid iid);
        IntPtr CreateForMonitor([In] IntPtr monitor, in Guid iid);
    }

    /// <summary>
    /// Internal item placed in the bounded queue by the FrameArrived callback.
    /// Carries the Direct3D11CaptureFrame (COM resource) + arrival wall clock.
    /// </summary>
    private sealed class QueuedFrame : IDisposable
    {
        public Direct3D11CaptureFrame Frame;
        public DateTime ArrivalWallClockUtc;

        public QueuedFrame(Direct3D11CaptureFrame frame, DateTime arrivalUtc)
        {
            Frame = frame;
            ArrivalWallClockUtc = arrivalUtc;
        }

        public void Dispose() => Frame.Dispose();
    }

    public void Setup()
    {
        DXGI.CreateDXGIFactory1(out _factory).CheckError();
        _factory!.EnumAdapters1(0, out _adapter).CheckError();

        FeatureLevel[] levels = { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 };
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

        // Query refresh rate from DXGIOutput1
        IDXGIOutput1 output1 = _output.QueryInterface<IDXGIOutput1>();
        var modeDesc = output1.FindClosestMatchingMode(new ModeDescription(Width, Height, new Rational(0, 0), Format.B8G8R8A8_UNorm), _d3dDevice);
        double refreshRate = (double)modeDesc.RefreshRate.Numerator / modeDesc.RefreshRate.Denominator;
        output1.Dispose();

        DisplayConfig = $"{Width}x{Height}@{refreshRate:F2}Hz@{desc.DeviceName}";
        RefreshRate = refreshRate;

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
    ///
    /// Uses a bounded BlockingCollection as the producer-consumer queue.
    /// No ManualResetEventSlim, no lost-signal race.
    /// </summary>
    public List<FrameRecord> Capture(int durationSec, string loadCondition)
    {
        if (_framePool == null || _session == null)
            throw new InvalidOperationException("WgcSession not set up. Call Setup() first.");

        // === FIX 1: Reset ALL raw counter fields at start of each session ===
        // Interlocked.Exchange ensures atomic reset even if a stale callback
        // is still running from a previous session.
        Interlocked.Exchange(ref _frameArrivedRaw, 0);
        Interlocked.Exchange(ref _tryGetNextFrameRaw, 0);
        Interlocked.Exchange(ref _acquiredRaw, 0);
        Interlocked.Exchange(ref _consumedRaw, 0);
        Interlocked.Exchange(ref _droppedByHarnessRaw, 0);
        Interlocked.Exchange(ref _noFrameReturnedRaw, 0);
        Interlocked.Exchange(ref _shutdownDiscardedRaw, 0);

        var frames = new List<FrameRecord>();
        var sw = Stopwatch.StartNew();
        long durationMs = durationSec * 1000L;

        using var cts = new CancellationTokenSource();
        using var queue = new BlockingCollection<QueuedFrame>(QueueCapacity);

        // === PRODUCER: FrameArrived callback ===
        _framePool.FrameArrived += (sender, _) =>
        {
            Interlocked.Increment(ref _frameArrivedRaw);

            DateTime arrivalUtc = DateTime.UtcNow;
            Interlocked.Increment(ref _tryGetNextFrameRaw);

            Direct3D11CaptureFrame? frame;
            try
            {
                frame = sender.TryGetNextFrame();
            }
            catch
            {
                // WGC may throw during shutdown — ignore.
                return;
            }

            if (frame == null)
            {
                // FIX 4: Renamed from "SupersededCount" to "NoFrameReturnedCount"
                // — does NOT imply supersession; WGC simply returned null.
                Interlocked.Increment(ref _noFrameReturnedRaw);
                return;
            }

            Interlocked.Increment(ref _acquiredRaw);

            var item = new QueuedFrame(frame, arrivalUtc);
            try
            {
                if (!queue.TryAdd(item, 0))
                {
                    // Queue full — harness must drop the frame.
                    Interlocked.Increment(ref _droppedByHarnessRaw);
                    item.Dispose();
                }
            }
            catch (InvalidOperationException)
            {
                // FIX 3: Queue has been marked as CompleteAdding during shutdown.
                // This frame was acquired (AcquiredFrameCount was incremented) but
                // cannot be enqueued. Track it separately so the invariant holds.
                Interlocked.Increment(ref _shutdownDiscardedRaw);
                item.Dispose();
            }
        };

        // Start capture
        Console.WriteLine($"  Starting capture ({loadCondition}, {durationSec}s)...");
        Console.WriteLine($"  Queue capacity: {QueueCapacity}");
        _session.StartCapture();

        long t0 = 0;
        bool t0Set = false;
        long prevSrt = 0;
        long prevPts = 0;

        // === CONSUMER: main thread ===
        while (sw.ElapsedMilliseconds < durationMs)
        {
            if (!queue.TryTake(out QueuedFrame? item, 500, cts.Token))
            {
                if (sw.ElapsedMilliseconds >= durationMs)
                    break;
                continue;
            }

            Interlocked.Increment(ref _consumedRaw);

            // FIX 2: Separate ArrivalWallClockUtc (from QueuedFrame) and
            // ConsumeWallClockUtc (captured here when consumer processes the frame).
            DateTime consumeUtc = DateTime.UtcNow;
            long srt = item.Frame.SystemRelativeTime.Ticks;

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
                ArrivalWallClockUtc = item.ArrivalWallClockUtc,  // FIX 2: when FrameArrived fired
                ConsumeWallClockUtc = consumeUtc,                 // FIX 2: when consumer processed
            });

            prevSrt = srt;
            prevPts = pts;

            if (frames.Count % 500 == 0)
            {
                long dropped = Interlocked.Read(ref _droppedByHarnessRaw);
                long noFrame = Interlocked.Read(ref _noFrameReturnedRaw);
                Console.WriteLine($"    f{frames.Count,6} | SRT={srt,16} | PTS={pts,12} | d={deltaSrt,8} | " +
                                  $"dropped={dropped,4} | noFrame={noFrame,4} | {sw.Elapsed.TotalSeconds:F1}s");
            }

            item.Dispose();
        }

        // === Shutdown ===
        cts.Cancel();
        queue.CompleteAdding();

        // Drain remaining items (bounded — max QueueCapacity items)
        while (queue.TryTake(out QueuedFrame? remaining, 0))
        {
            Interlocked.Increment(ref _consumedRaw);

            DateTime consumeUtc = DateTime.UtcNow;
            long srt = remaining.Frame.SystemRelativeTime.Ticks;
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
                ArrivalWallClockUtc = remaining.ArrivalWallClockUtc,
                ConsumeWallClockUtc = consumeUtc,
            });
            prevSrt = srt;
            prevPts = pts;
            remaining.Dispose();
        }

        // Update property counters from raw fields
        FrameArrivedCount = Interlocked.Read(ref _frameArrivedRaw);
        TryGetNextFrameCount = Interlocked.Read(ref _tryGetNextFrameRaw);
        AcquiredFrameCount = Interlocked.Read(ref _acquiredRaw);
        ConsumedFrameCount = Interlocked.Read(ref _consumedRaw);
        DroppedByHarnessCount = Interlocked.Read(ref _droppedByHarnessRaw);
        NoFrameReturnedCount = Interlocked.Read(ref _noFrameReturnedRaw);
        ShutdownDiscardedCount = Interlocked.Read(ref _shutdownDiscardedRaw);

        Console.WriteLine($"  Capture ended: {frames.Count} consumed frames in {sw.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Arrived={FrameArrivedCount} TryGet={TryGetNextFrameCount} Acquired={AcquiredFrameCount} " +
                          $"Consumed={ConsumedFrameCount} Dropped={DroppedByHarnessCount} " +
                          $"NoFrameReturned={NoFrameReturnedCount} ShutdownDiscarded={ShutdownDiscardedCount}");

        // Verify invariant: Acquired = Consumed + Dropped + ShutdownDiscarded
        long accounted = ConsumedFrameCount + DroppedByHarnessCount + ShutdownDiscardedCount;
        bool invariantHolds = AcquiredFrameCount == accounted;
        if (!invariantHolds)
        {
            Console.WriteLine($"  WARNING: Invariant — Acquired({AcquiredFrameCount}) != " +
                            $"Consumed({ConsumedFrameCount}) + Dropped({DroppedByHarnessCount}) + " +
                            $"ShutdownDiscarded({ShutdownDiscardedCount}) = {accounted}");
        }

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
