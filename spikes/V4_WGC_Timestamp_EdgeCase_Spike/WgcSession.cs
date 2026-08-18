// WgcSession.cs — single-session capture wrapper
// SPDX-License-Identifier: MIT
// Standalone class — does NOT import or share state with V1/V2/V3.
//
// ACQUISITION DESIGN:
//   FrameArrived callback → TryGetNextFrame → enqueue to bounded BlockingCollection
//   Consumer thread → dequeue → extract timestamp → dispose frame → add to List
//
//   Counters:
//     FrameArrivedCount     — incremented every time FrameArrived fires
//     TryGetNextFrameCount  — incremented every time TryGetNextFrame is called
//     AcquiredFrameCount     — incremented when TryGetNextFrame returns non-null
//     ConsumedFrameCount     — incremented when consumer dequeues + processes a frame
//     DroppedByHarnessCount  — incremented when queue is full and frame must be dropped
//     SupersededCount        — incremented when TryGetNextFrame returns null (WGC had no new frame)
//
//   Invariant: AcquiredFrameCount = ConsumedFrameCount + DroppedByHarnessCount
//   (every acquired frame is either consumed or dropped by the harness — never silently lost)
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
    // WGC typically delivers at display refresh rate; a queue of 16
    // provides ample buffer without unbounded growth.
    private const int QueueCapacity = 16;

    public string DisplayConfig { get; private set; } = "";
    public int Width { get; private set; }
    public int Height { get; private set; }

    // === Acquisition counters (per session) ===
    public long FrameArrivedCount { get; private set; }
    public long TryGetNextFrameCount { get; private set; }
    public long AcquiredFrameCount { get; private set; }
    public long ConsumedFrameCount { get; private set; }
    public long DroppedByHarnessCount { get; private set; }
    public long SupersededCount { get; private set; }

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
    ///
    /// Uses a bounded BlockingCollection as the producer-consumer queue.
    /// No ManualResetEventSlim, no lost-signal race.
    /// </summary>
    public List<FrameRecord> Capture(int durationSec, string loadCondition)
    {
        if (_framePool == null || _session == null)
            throw new InvalidOperationException("WgcSession not set up. Call Setup() first.");

        // Reset counters
        FrameArrivedCount = 0;
        TryGetNextFrameCount = 0;
        AcquiredFrameCount = 0;
        ConsumedFrameCount = 0;
        DroppedByHarnessCount = 0;
        SupersededCount = 0;

        var frames = new List<FrameRecord>();
        var sw = Stopwatch.StartNew();
        long durationMs = durationSec * 1000L;

        // Bounded queue — producer adds, consumer takes.
        // Using BlockingCollection with CancellationToken for clean shutdown.
        using var cts = new CancellationTokenSource();
        using var queue = new BlockingCollection<QueuedFrame>(QueueCapacity);

        // === PRODUCER: FrameArrived callback ===
        // Runs on WGC's thread-pool thread (free-threaded frame pool).
        // Tries to acquire frame, then enqueues. If queue is full, drops + counts.
        _framePool.FrameArrived += (sender, _) =>
        {
            Interlocked.Increment(ref _frameArrivedRaw);
            // We use a field for the raw count because properties can't be Interlocked'd.
            // The property returns the field value at the end.

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
                Interlocked.Increment(ref _supersededRaw);
                return;
            }

            Interlocked.Increment(ref _acquiredRaw);

            var item = new QueuedFrame(frame, arrivalUtc);
            try
            {
                // Try to add with zero timeout — if queue is full, drop immediately.
                if (!queue.TryAdd(item, 0))
                {
                    // Queue full — harness must drop the frame.
                    Interlocked.Increment(ref _droppedByHarnessRaw);
                    item.Dispose();
                }
            }
            catch (InvalidOperationException)
            {
                // Queue has been marked as CompleteAdding — we're shutting down.
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
        // Dequeue from BlockingCollection, extract timestamp, dispose frame.
        while (sw.ElapsedMilliseconds < durationMs)
        {
            // Blocking take with timeout — no lost-signal race.
            // If no frame arrives within 500ms, we just loop and check duration.
            if (!queue.TryTake(out QueuedFrame? item, 500, cts.Token))
            {
                // Timeout — no frame available. Check if we should stop.
                if (sw.ElapsedMilliseconds >= durationMs)
                    break;
                continue;
            }

            Interlocked.Increment(ref _consumedRaw);

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
                WallClockUtcCaptured = consumeUtc,  // when the harness consumed the frame
            });

            prevSrt = srt;
            prevPts = pts;

            if (frames.Count % 500 == 0)
            {
                long dropped = Interlocked.Read(ref _droppedByHarnessRaw);
                Console.WriteLine($"    f{frames.Count,6} | SRT={srt,16} | PTS={pts,12} | d={deltaSrt,8} | " +
                                  $"dropped={dropped,4} | {sw.Elapsed.TotalSeconds:F1}s");
            }

            item.Dispose();
        }

        // === Shutdown ===
        // Signal producers to stop, then drain remaining frames in the queue.
        cts.Cancel();
        queue.CompleteAdding();

        // Drain remaining items (bounded — not unbounded)
        while (queue.TryTake(out QueuedFrame? remaining, 0))
        {
            Interlocked.Increment(ref _consumedRaw);
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
                WallClockUtcCaptured = DateTime.UtcNow,
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
        SupersededCount = Interlocked.Read(ref _supersededRaw);

        Console.WriteLine($"  Capture ended: {frames.Count} consumed frames in {sw.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Arrived={FrameArrivedCount} TryGet={TryGetNextFrameCount} Acquired={AcquiredFrameCount} " +
                          $"Consumed={ConsumedFrameCount} Dropped={DroppedByHarnessCount} Superseded={SupersededCount}");

        // Verify invariant: Acquired = Consumed + Dropped
        long expectedConsumedOrDropped = AcquiredFrameCount - DroppedByHarnessCount;
        if (ConsumedFrameCount != expectedConsumedOrDropped)
        {
            Console.WriteLine($"  WARNING: Invariant check — Consumed({ConsumedFrameCount}) != " +
                            $"Acquired({AcquiredFrameCount}) - Dropped({DroppedByHarnessCount}) = {expectedConsumedOrDropped}");
            Console.WriteLine("    (May occur if frames are still in queue at shutdown — check drain logic)");
        }

        _session.Dispose();
        _framePool.Dispose();
        return frames;
    }

    // Raw fields for Interlocked operations (properties can't be used with Interlocked)
    private long _frameArrivedRaw;
    private long _tryGetNextFrameRaw;
    private long _acquiredRaw;
    private long _consumedRaw;
    private long _droppedByHarnessRaw;
    private long _supersededRaw;

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
