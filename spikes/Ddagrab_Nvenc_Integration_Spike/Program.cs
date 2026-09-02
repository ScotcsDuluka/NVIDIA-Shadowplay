using System.Diagnostics;
using CaptureEngine.Diagnostics;
using CaptureEngine.Encoder;
using CaptureEngine.Encoder.Nvenc;
using CaptureEngine.Video;
using CaptureEngine.Video.Backends.Ddagrab;
using CaptureEngine.Video.Handoff;

internal sealed class IntegrationContext : IVideoBackendContext
{
    public IntegrationContext(VideoBackendKind kind)
    {
        BackendKind = kind;
        Logger = new EngineLogger("DdagrabNvencIntegration", EngineLogger.LogLevel.Info);
    }

    public EngineLogger Logger { get; }
    public VideoBackendKind BackendKind { get; }
}

internal static class Program
{
    private static int Main()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Ddagrab -> NVENC Integration Spike");
        Console.WriteLine(" Real DXGI frame -> D3D11VideoFrame -> native NVENC");
        Console.WriteLine("============================================================");

        const int durationSeconds = 8;
        var logger = new EngineLogger("IntegrationSpike", EngineLogger.LogLevel.Info);
        var context = new IntegrationContext(VideoBackendKind.Ddagrab);

        using var capture = new DdagrabBackend(logger);
        capture.UseSharedHandle = true;
        capture.Initialize(context);

        int width = capture.OutputWidth;
        int height = capture.OutputHeight;
        int refresh = capture.OutputRefreshRate;
        Console.WriteLine($"Capture: {width}x{height} @ {refresh}Hz");

        var config = new EncoderConfig
        {
            CodecKey = "NVENC_H264",
            BitrateBps = 20_000_000L,
            MinrateBps = 20_000_000L,
            MaxrateBps = 20_000_000L,
            BufsizeBps = 40_000_000L,
            RateControl = "cbr",
            GopSize = refresh > 0 ? refresh : 60,
            Preset = "p4",
            FrameRateFps = refresh > 0 ? refresh : 60,
            ExpectedWidth = width,
            ExpectedHeight = height,
            EncodeWidth = width,
            EncodeHeight = height
        };

        using var encoder = new NvencEncoderBackend(logger);
        encoder.Initialize(config);
        encoder.Start();

        var sink = new BoundedVideoFrameSink(
            4, BoundedHandoffPolicy.DropOldest, logger);
        capture.Start(sink);

        var sw = Stopwatch.StartNew();
        long encoded = 0;
        long packetsBytes = 0;
        long consumed = 0;
        long encodeFailures = 0;
        long framesSeen = 0;
        long firstPts = -1;
        long lastPts = -1;

        try
        {
            while (sw.Elapsed < TimeSpan.FromSeconds(durationSeconds))
            {
                FrameAcquisitionResult result = default;
                if (!sink.TryTake(ref result))
                {
                    Thread.Sleep(1);
                    continue;
                }

                consumed++;
                var frame = result.Frame;
                if (frame == null)
                    continue;

                framesSeen++;
                long pts = frame.Diagnostics.PresentationTimestampTicks;
                if (firstPts < 0) firstPts = pts;
                lastPts = pts;

                try
                {
                    EncodedPacket packet = null!;
                    if (encoder.Encode(frame, ref packet) && packet != null)
                    {
                        encoded++;
                        packetsBytes += packet.PayloadLength;
                    }
                    else
                    {
                        encodeFailures++;
                    }
                }
                finally
                {
                    frame.Dispose();
                }
            }
        }
        finally
        {
            capture.Stop();
            encoder.Stop();
            sink.Dispose();
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double fps = framesSeen / elapsed;
        double encodeFps = encoded / elapsed;
        double mbps = packetsBytes * 8.0 / elapsed / 1_000_000.0;

        Console.WriteLine();
        Console.WriteLine("=== RESULT ===");
        Console.WriteLine($"Elapsed:              {elapsed:F3}s");
        Console.WriteLine($"Frames consumed:      {consumed}");
        Console.WriteLine($"Frames seen:          {framesSeen}");
        Console.WriteLine($"Capture emitted:      {capture.Diagnostics.EmittedFrames}");
        Console.WriteLine($"Capture dropped:      {capture.Diagnostics.DroppedFrames}");
        Console.WriteLine($"Capture no-frame:     {capture.Diagnostics.NoFrameCount}");
        Console.WriteLine($"Capture errors:       {capture.Diagnostics.ErrorCount}");
        Console.WriteLine($"AccessLost:           {capture.AccessLostCount}");
        Console.WriteLine($"Textures created:     {capture.TexturesCreated}");
        Console.WriteLine($"Textures disposed:    {capture.TexturesDisposed}");
        Console.WriteLine($"Encoded packets:      {encoded}");
        Console.WriteLine($"Encode failures:      {encodeFailures}");
        Console.WriteLine($"Encoded FPS:          {encodeFps:F2}");
        Console.WriteLine($"Input FPS:            {fps:F2}");
        Console.WriteLine($"Encoded bytes:        {packetsBytes}");
        Console.WriteLine($"Measured bitrate:     {mbps:F2} Mbps");
        Console.WriteLine($"First PTS:            {firstPts}");
        Console.WriteLine($"Last PTS:             {lastPts}");
        Console.WriteLine($"PTS span:             {(lastPts >= firstPts && firstPts >= 0 ? (lastPts - firstPts) / 10_000_000.0 : 0):F3}s");

        bool pass = encoded > 0 && encodeFailures == 0 && capture.Diagnostics.ErrorCount == 0;
        pass &= capture.TexturesCreated == capture.TexturesDisposed + sink.Count;
        Console.WriteLine($"VERDICT: {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }
}


