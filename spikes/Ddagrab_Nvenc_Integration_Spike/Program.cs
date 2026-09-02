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
        Console.WriteLine(" Restart + bitrate characterization");
        Console.WriteLine("============================================================");

        const int sessionCount = 3;
        const int durationSeconds = 5;
        var logger = new EngineLogger("IntegrationSpike", EngineLogger.LogLevel.Warning);
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

        long totalEncoded = 0;
        long totalBytes = 0;
        long totalConsumed = 0;
        long totalFailures = 0;
        bool allPass = true;

        for (int session = 1; session <= sessionCount; session++)
        {
            var sink = new BoundedVideoFrameSink(4, BoundedHandoffPolicy.DropOldest, logger);
            encoder.Start();
            capture.Start(sink);

            var sw = Stopwatch.StartNew();
            long encoded = 0;
            long bytes = 0;
            long consumed = 0;
            long failures = 0;
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

                    long pts = frame.Diagnostics.PresentationTimestampTicks;
                    if (firstPts < 0) firstPts = pts;
                    lastPts = pts;

                    try
                    {
                        EncodedPacket packet = null!;
                        if (encoder.Encode(frame, ref packet) && packet != null)
                        {
                            encoded++;
                            bytes += packet.PayloadLength;
                        }
                        else
                        {
                            failures++;
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
            double encodeFps = encoded / elapsed;
            double mbps = bytes * 8.0 / elapsed / 1_000_000.0;
            bool sessionPass = encoded > 0 && failures == 0 &&
                               capture.Diagnostics.ErrorCount == 0 &&
                               capture.TexturesCreated == capture.TexturesDisposed;
            allPass &= sessionPass;
            totalEncoded += encoded;
            totalBytes += bytes;
            totalConsumed += consumed;
            totalFailures += failures;

            Console.WriteLine();
            Console.WriteLine($"--- SESSION {session}/{sessionCount} ---");
            Console.WriteLine($"Elapsed:              {elapsed:F3}s");
            Console.WriteLine($"Frames consumed:      {consumed}");
            Console.WriteLine($"Encoded packets:      {encoded}");
            Console.WriteLine($"Encode failures:      {failures}");
            Console.WriteLine($"Encoded FPS:          {encodeFps:F2}");
            Console.WriteLine($"Encoded bytes:        {bytes}");
            Console.WriteLine($"Measured bitrate:     {mbps:F2} Mbps");
            Console.WriteLine($"PTS span:             {(lastPts >= firstPts && firstPts >= 0 ? (lastPts - firstPts) / 10_000_000.0 : 0):F3}s");
            Console.WriteLine($"Capture errors:       {capture.Diagnostics.ErrorCount}");
            Console.WriteLine($"AccessLost:           {capture.AccessLostCount}");
            Console.WriteLine($"Textures:             {capture.TexturesCreated}/{capture.TexturesDisposed}");
            Console.WriteLine($"SESSION VERDICT:      {(sessionPass ? "PASS" : "FAIL")}");
        }

        double aggregateMbps = totalBytes * 8.0 /
                               (durationSeconds * sessionCount) /
                               1_000_000.0;
        Console.WriteLine();
        Console.WriteLine("=== AGGREGATE ===");
        Console.WriteLine($"Sessions:             {sessionCount}");
        Console.WriteLine($"Frames consumed:      {totalConsumed}");
        Console.WriteLine($"Encoded packets:      {totalEncoded}");
        Console.WriteLine($"Encode failures:      {totalFailures}");
        Console.WriteLine($"Aggregate bitrate:    {aggregateMbps:F2} Mbps");
        Console.WriteLine($"Final capture errors: {capture.Diagnostics.ErrorCount}");
        Console.WriteLine($"Final AccessLost:     {capture.AccessLostCount}");
        Console.WriteLine($"Final textures:       {capture.TexturesCreated}/{capture.TexturesDisposed}");
        Console.WriteLine($"VERDICT:              {(allPass ? "PASS" : "FAIL")}");
        return allPass ? 0 : 1;
    }
}


