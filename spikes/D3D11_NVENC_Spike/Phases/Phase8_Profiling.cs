// Phases/Phase8_Profiling.cs
//
// P1-B.2 Performance Profiling Spike — Phase 8: Per-Stage Timing
//
// Goal: Instrument the Phase 7 pipeline with per-stage timing to identify
// the bottleneck. Same pipeline semantics as Phase 7 — only adds timing.
//
// Stages instrumented (per frame, in microseconds):
//   1. AcquireNextFrame — DXGI Desktop Duplication acquisition
//   2. CopyResource — GPU-side texture copy (encoderTexture ← desktopTexture)
//   3. ReleaseFrame — return OS-owned texture to duplication session
//   4. MapInputResource — NVENC resource mapping
//   5. EncodePicture — NVENC H.264 encode (synchronous)
//   6. LockBitstream — retrieve encoded bytes (blocking — waits for GPU)
//   7. UnlockBitstream — release bitstream buffer
//   8. UnmapInputResource — NVENC resource unmapping
//
// Each stage reports: min / avg / P50 / P95 / P99 / max
//
// The "total encode latency" (Phase 7 metric) = stages 4+5+6+7+8.
// The "total frame latency" = stages 1+2+3+4+5+6+7+8.
//
// Measurement limitations (stated honestly):
//   - AcquireNextFrame may block on GPU compositor; CPU time vs GPU wait
//     cannot be distinguished without DXGI profiler API (not available in spike).
//   - CopyResource is a GPU command submit — CPU returns immediately but the
//     GPU may not have completed. The measured time is CPU submit time only.
//   - EncodePicture is synchronous — CPU blocks until NVENC accepts the frame.
//     If NVENC is busy (pipelining), this includes GPU wait time.
//   - LockBitstream is the primary GPU synchronization point — it blocks until
//     the NVENC hardware has finished encoding. This is where GPU wait time
//     is concentrated.
//   - deviceCtx.Flush() before MapInputResource ensures CopyResource's GPU
//     command is submitted. This is NOT a GPU sync — it only flushes the
//     command queue. The actual sync happens at LockBitstream.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase8_Profiling
{
    private const int DurationSeconds = 60;
    private const int AcquireTimeoutMs = 100;
    private const int WarmupFrames = 30;

    // Stage indices
    private const int S_Acquire = 0;
    private const int S_Copy = 1;
    private const int S_Release = 2;
    private const int S_Map = 3;
    private const int S_Encode = 4;
    private const int S_Lock = 5;
    private const int S_Unlock = 6;
    private const int S_Unmap = 7;
    private const int S_Total = 8;  // total per-frame
    private const int NUM_STAGES = 9;

    private static readonly string[] StageNames =
    {
        "AcquireNextFrame",
        "CopyResource",
        "ReleaseFrame",
        "MapInputResource",
        "EncodePicture",
        "LockBitstream",
        "UnlockBitstream",
        "UnmapInputResource",
        "Total",
    };

    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 8 — Per-Stage Performance Profiling");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Auto-run Phases 1-3 if not already done
        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.WriteLine("  Phase 1-3 not yet run — auto-running...");
            int p1 = Phase1_DeviceTest.Run();
            if (p1 != 0) { Console.Error.WriteLine("  FAIL: Phase 1."); return 1; }
            int p2 = Phase2_DesktopDuplication.Run();
            if (p2 != 0) { Console.Error.WriteLine("  FAIL: Phase 2."); return 1; }
            int p3 = Phase3_TextureOwnership.Run();
            if (p3 != 0) { Console.Error.WriteLine("  FAIL: Phase 3."); return 1; }
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" Phase 8 — Profiling (continuing after Phase 1-3)");
            Console.WriteLine("============================================================");
            Console.WriteLine();
        }

        var device = SpikeSharedContext.Device!;
        var duplDesc = SpikeSharedContext.DuplicationDesc!.Value;
        uint texWidth = duplDesc.ModeDescription.Width;
        uint texHeight = duplDesc.ModeDescription.Height;

        // Create our own duplication
        Console.WriteLine("[8.0] Creating IDXGIOutputDuplication...");
        IDXGIOutput? primaryOutput = null;
        int outIdx = 0;
        while (SpikeSharedContext.TargetAdapter!.EnumOutputs((uint)outIdx, out IDXGIOutput out_).Success)
        {
            if (outIdx == 0) primaryOutput = out_;
            else out_.Dispose();
            outIdx++;
        }
        if (primaryOutput == null) { Console.Error.WriteLine("  FAIL: No outputs."); return 1; }
        IDXGIOutput1 output1 = primaryOutput.QueryInterface<IDXGIOutput1>();
        IDXGIOutputDuplication duplication;
        try { duplication = output1.DuplicateOutput(device); }
        catch (Exception ex) { Console.Error.WriteLine($"  FAIL: DuplicateOutput: {ex.Message}"); output1.Dispose(); primaryOutput.Dispose(); return 1; }
        output1.Dispose(); primaryOutput.Dispose();
        Console.WriteLine($"  PASS: Duplication ({texWidth}x{texHeight}).");

        // Load NVENC
        Console.WriteLine("[8.1] Loading NVENC + opening encoder...");
        using var nvenc = new NvEncFunctionTable();
        if (!nvenc.TryLoad()) { Console.Error.WriteLine("  FAIL: NVENC load."); return 1; }

        var sessionParams = new NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
        {
            version = NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER,
            deviceType = NvEncodeAPI.NV_ENC_DEVICE_DIRECTX,
            device = device.NativePointer,
            reserved = IntPtr.Zero,
            apiVersion = NvEncodeAPI.NVENCAPI_VERSION,
            reserved1 = new uint[253],
            reserved2 = new IntPtr[64],
        };
        uint status = nvenc.OpenEncodeSessionEx!(ref sessionParams, out IntPtr encoder);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: OpenEncodeSessionEx: {status}"); return 1; }
        Console.WriteLine($"  PASS: Encoder: 0x{encoder.ToInt64():x16}");

        ID3D11Texture2D? encoderTexture = null;
        IntPtr bitstreamBuffer = IntPtr.Zero;
        IntPtr registeredResource = IntPtr.Zero;
        bool encoderOpened = true;

        try
        {
            // Init encoder
            var initParams = new NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS
            {
                version = NvEncodeAPI.MakeStructVersion(5) | (1u << 31),
                encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID,
                presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID,
                encodeWidth = texWidth, encodeHeight = texHeight,
                darWidth = texWidth, darHeight = texHeight,
                frameRateNum = 60, frameRateDen = 1,
                enableEncodeAsync = 0, enablePTD = 1, bitFields = 0,
                privDataSize = 0, privData = IntPtr.Zero, encodeConfig = IntPtr.Zero,
                maxEncodeWidth = texWidth, maxEncodeHeight = texHeight,
                maxMEHintCountsPerBlockL0 = 0, maxMEHintCountsPerBlockL1 = 0,
                reserved = new uint[289], reserved2 = new IntPtr[64],
            };
            status = nvenc.InitializeEncoder!(encoder, ref initParams);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: Init: {status}"); return 1; }

            // Create bitstream buffer
            var bsParams = new NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER
            {
                version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER,
                size = 0, memoryHeap = 0, _padding = 0,
                bitstreamBuffer = IntPtr.Zero, reserved1 = IntPtr.Zero, reserved2 = IntPtr.Zero,
                reserved3 = new uint[226], reserved4 = new IntPtr[64],
            };
            status = nvenc.CreateBitstreamBuffer!(encoder, ref bsParams);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: BS buffer: {status}"); return 1; }
            bitstreamBuffer = bsParams.bitstreamBuffer;

            // Create encoder texture (GPU-resident, USAGE_DEFAULT)
            var texDesc = new Texture2DDescription
            {
                Width = texWidth, Height = texHeight, MipLevels = 1, ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm, SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default, BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None, MiscFlags = ResourceOptionFlags.None,
            };
            encoderTexture = device.CreateTexture2D(texDesc);

            // Register texture ONCE
            var regParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
            {
                version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
                resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
                width = texWidth, height = texHeight, pitch = 0, subResourceIndex = 0,
                resourceToRegister = encoderTexture.NativePointer, registeredResource = IntPtr.Zero,
                bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                reserved1 = new uint[248], reserved2 = new IntPtr[62],
            };
            status = nvenc.RegisterResource!(encoder, ref regParams);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: Register: {status}"); return 1; }
            registeredResource = regParams.registeredResource;
            Console.WriteLine("  PASS: Setup complete.");
            Console.WriteLine();

            // Warmup (30 frames, not counted)
            Console.WriteLine($"[8.2] Warmup ({WarmupFrames} frames)...");
            for (int i = 0; i < WarmupFrames; i++)
            {
                ProfiledEncodeOneFrame(device, duplication, encoder, nvenc,
                    encoderTexture, registeredResource, bitstreamBuffer,
                    texWidth, texHeight, out int _, out var _timings);
            }
            Console.WriteLine("  Warmup done.");
            Console.WriteLine();

            // Timed loop
            Console.WriteLine($"[8.3] Timed loop ({DurationSeconds}s)...");
            var sw = Stopwatch.StartNew();
            var duration = TimeSpan.FromSeconds(DurationSeconds);

            long framesCaptured = 0, framesEncoded = 0, droppedCount = 0, nvencErrors = 0, waitTimeouts = 0;
            long totalEncodedBytes = 0;

            // Per-stage timing accumulators
            var stageLatencies = new List<long>[NUM_STAGES];
            for (int i = 0; i < NUM_STAGES; i++)
                stageLatencies[i] = new List<long>(8 * 1024);

            while (sw.Elapsed < duration)
            {
                bool ok = ProfiledEncodeOneFrame(device, duplication, encoder, nvenc,
                    encoderTexture, registeredResource, bitstreamBuffer,
                    texWidth, texHeight, out int bsLen, out long[] timings);

                if (!ok)
                {
                    if (bsLen == -1) waitTimeouts++;
                    else { droppedCount++; nvencErrors++; }
                    continue;
                }

                framesCaptured++;
                framesEncoded++;
                totalEncodedBytes += bsLen;

                for (int s = 0; s < NUM_STAGES; s++)
                    stageLatencies[s].Add(timings[s]);
            }
            sw.Stop();

            double elapsedSec = sw.Elapsed.TotalSeconds;
            double achievedFps = framesEncoded / elapsedSec;
            double bitrateBps = elapsedSec > 0 ? (totalEncodedBytes * 8.0) / elapsedSec : 0;

            // Report
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" Phase 8 — PROFILING RESULTS");
            Console.WriteLine("============================================================");
            Console.WriteLine($"  duration_seconds:        {elapsedSec:F3}");
            Console.WriteLine($"  frames_captured:         {framesCaptured}");
            Console.WriteLine($"  frames_encoded:          {framesEncoded}");
            Console.WriteLine($"  dropped_count:           {droppedCount}");
            Console.WriteLine($"  nvenc_errors:            {nvencErrors}");
            Console.WriteLine($"  capture_wait_timeouts:   {waitTimeouts}");
            Console.WriteLine($"  achieved_fps:           {achievedFps:F2}");
            Console.WriteLine($"  total_encoded_bytes:    {totalEncodedBytes}");
            Console.WriteLine($"  bitrate_mbps:           {bitrateBps / 1_000_000:F3}");
            Console.WriteLine();
            Console.WriteLine("─── Per-Stage Timing (microseconds) ───");
            Console.WriteLine($"  {'Stage':<25} {'min':>8} {'avg':>8} {'P50':>8} {'P95':>8} {'P99':>8} {'max':>8}");
            Console.WriteLine($"  {'─'*25} {'─'*8} {'─'*8} {'─'*8} {'─'*8} {'─'*8} {'─'*8}");

            for (int s = 0; s < NUM_STAGES; s++)
            {
                var lat = stageLatencies[s];
                if (lat.Count == 0) continue;
                long minV = lat.Min();
                double avgV = lat.Average();
                double p50 = Percentile(lat, 0.50);
                double p95 = Percentile(lat, 0.95);
                double p99 = Percentile(lat, 0.99);
                long maxV = lat.Max();
                Console.WriteLine($"  {StageNames[s],-25} {minV,8} {avgV,8:F1} {p50,8:F1} {p95,8:F1} {p99,8:F1} {maxV,8}");
            }

            Console.WriteLine();
            Console.WriteLine("─── Bottleneck Analysis ───");

            // Identify the stage with highest avg (excluding Total)
            double maxAvg = 0;
            string bottleneck = "";
            for (int s = 0; s < NUM_STAGES - 1; s++)
            {
                double avg = stageLatencies[s].Count > 0 ? stageLatencies[s].Average() : 0;
                if (avg > maxAvg) { maxAvg = avg; bottleneck = StageNames[s]; }
            }
            Console.WriteLine($"  Highest avg stage: {bottleneck} ({maxAvg:F1} µs)");

            // LockBitstream is the GPU sync point
            double lockAvg = stageLatencies[S_Lock].Count > 0 ? stageLatencies[S_Lock].Average() : 0;
            double encodeAvg = stageLatencies[S_Encode].Count > 0 ? stageLatencies[S_Encode].Average() : 0;
            double acquireAvg = stageLatencies[S_Acquire].Count > 0 ? stageLatencies[S_Acquire].Average() : 0;
            double copyAvg = stageLatencies[S_Copy].Count > 0 ? stageLatencies[S_Copy].Average() : 0;

            Console.WriteLine($"  LockBitstream avg: {lockAvg:F1} µs (GPU sync — encode completion wait)");
            Console.WriteLine($"  EncodePicture avg: {encodeAvg:F1} µs (NVENC submit — may include GPU queue)");
            Console.WriteLine($"  AcquireNextFrame avg: {acquireAvg:F1} µs (DXGI — may include compositor wait)");
            Console.WriteLine($"  CopyResource avg: {copyAvg:F1} µs (GPU command submit — CPU-side only)");

            double totalAvg = stageLatencies[S_Total].Count > 0 ? stageLatencies[S_Total].Average() : 0;
            Console.WriteLine($"  Total per-frame avg: {totalAvg:F1} µs ({1_000_000.0 / totalAvg:F1} FPS theoretical max)");

            Console.WriteLine();
            Console.WriteLine("─── Measurement Limitations ───");
            Console.WriteLine("  1. AcquireNextFrame: cannot distinguish CPU time vs compositor wait");
            Console.WriteLine("     without DXGI profiler API (unavailable in spike).");
            Console.WriteLine("  2. CopyResource: measured time is CPU command-submit only.");
            Console.WriteLine("     GPU may not have completed the copy when the timer stops.");
            Console.WriteLine("     Actual GPU copy completion is deferred to LockBitstream sync.");
            Console.WriteLine("  3. EncodePicture: synchronous NVENC call. CPU blocks until NVENC");
            Console.WriteLine("     accepts the frame. If NVENC is still encoding the previous frame,");
            Console.WriteLine("     this includes GPU queue wait time (not separable in spike).");
            Console.WriteLine("  4. LockBitstream: the primary GPU synchronization point. This is");
            Console.WriteLine("     where GPU-side encode completion wait is concentrated. High P99");
            Console.WriteLine("     here indicates NVENC pipeline stalls or GPU contention.");
            Console.WriteLine("  5. deviceCtx.Flush() before MapInputResource is NOT timed separately");
            Console.WriteLine("     — it is included in the CopyResource→Map gap (not a separate stage).");
            Console.WriteLine("  6. No GPU timestamp queries used (would require D3D11 query API");
            Console.WriteLine("     integration beyond spike scope).");
            Console.WriteLine("============================================================");

            if (framesEncoded > 0 && nvencErrors == 0)
                Console.WriteLine("  Phase 8: PASS — profiling complete, 0 errors.");
            else
                Console.WriteLine("  Phase 8: FAIL");
            Console.WriteLine();
            return framesEncoded > 0 ? 0 : 1;
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("[8.cleanup] Best-effort cleanup...");
            if (registeredResource != IntPtr.Zero && nvenc.UnregisterResource != null)
                try { nvenc.UnregisterResource(encoder, registeredResource); } catch { }
            if (bitstreamBuffer != IntPtr.Zero && nvenc.DestroyBitstreamBuffer != null)
                try { nvenc.DestroyBitstreamBuffer(encoder, bitstreamBuffer); } catch { }
            if (encoderTexture != null)
                try { encoderTexture.Dispose(); } catch { }
            if (encoderOpened && nvenc.DestroyEncoder != null)
                try { nvenc.DestroyEncoder(encoder); } catch { }
            try { duplication.Dispose(); } catch { }
            Console.WriteLine("  Cleanup done.");
        }
    }

    /// <summary>
    /// Same pipeline as Phase 7's EncodeOneFrame, but with per-stage timing.
    /// Returns timings[0..7] for each stage, timings[8] for total.
    /// </summary>
    private static bool ProfiledEncodeOneFrame(
        ID3D11Device device, IDXGIOutputDuplication duplication,
        IntPtr encoder, NvEncFunctionTable nvenc,
        ID3D11Texture2D encoderTexture, IntPtr registeredResource,
        IntPtr bitstreamBuffer, uint texWidth, uint texHeight,
        out int bsLen, out long[] timings)
    {
        bsLen = 0;
        timings = new long[NUM_STAGES];
        var deviceCtx = device.ImmediateContext;
        long fStart = Stopwatch.GetTimestamp();

        // Stage 0: AcquireNextFrame
        long t0 = Stopwatch.GetTimestamp();
        var acquireResult = duplication.AcquireNextFrame(AcquireTimeoutMs, out var frameInfo, out var desktopResource);
        long t1 = Stopwatch.GetTimestamp();
        timings[S_Acquire] = TicksToUs(t0, t1);

        if (acquireResult.Failure)
        {
            if (acquireResult.Code == Vortice.DXGI.ResultCode.WaitTimeout)
            {
                bsLen = -1;
                timings[S_Total] = TicksToUs(fStart, Stopwatch.GetTimestamp());
                return false;
            }
            bsLen = -2;
            if (desktopResource != null) desktopResource.Dispose();
            timings[S_Total] = TicksToUs(fStart, Stopwatch.GetTimestamp());
            return false;
        }

        var desktopTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
        desktopResource.Dispose();

        // Stage 1: CopyResource
        t0 = Stopwatch.GetTimestamp();
        deviceCtx.CopyResource(encoderTexture, desktopTexture);
        t1 = Stopwatch.GetTimestamp();
        timings[S_Copy] = TicksToUs(t0, t1);

        // Stage 2: ReleaseFrame + dispose desktop texture
        t0 = Stopwatch.GetTimestamp();
        duplication.ReleaseFrame();
        desktopTexture.Dispose();
        t1 = Stopwatch.GetTimestamp();
        timings[S_Release] = TicksToUs(t0, t1);

        // Stage 3: MapInputResource (includes Flush before map)
        t0 = Stopwatch.GetTimestamp();
        deviceCtx.Flush();
        var mapParams = new NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE
        {
            version = NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER,
            subResourceIndex = 0,
            inputResource = registeredResource,
            registeredResource = registeredResource,
            mappedResource = IntPtr.Zero,
            mappedBufferFmt = 0,
            reserved1 = new uint[251],
            reserved2 = new IntPtr[63],
        };
        uint mapStatus = nvenc.MapInputResource!(encoder, ref mapParams);
        t1 = Stopwatch.GetTimestamp();
        timings[S_Map] = TicksToUs(t0, t1);

        if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            bsLen = -2;
            timings[S_Total] = TicksToUs(fStart, Stopwatch.GetTimestamp());
            return false;
        }
        IntPtr mappedInput = mapParams.mappedResource;

        try
        {
            // Stage 4: EncodePicture
            var picParams = new NvEncodeAPI.NV_ENC_PIC_PARAMS
            {
                version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
                inputWidth = texWidth, inputHeight = texHeight, inputPitch = 0,
                encodePicFlags = 0, frameIdx = 0, inputTimeStamp = 0, inputDuration = 0,
                inputBuffer = mappedInput, outputBitstream = bitstreamBuffer,
                completionEvent = IntPtr.Zero,
                bufferFmt = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                pictureStruct = 1, pictureType = 0, _padding1 = 0,
                codecPicParams = new byte[1536],
                meHintCountsPerBlock = new byte[32],
                meExternalHints = IntPtr.Zero,
                reserved1 = new uint[6], reserved2 = new IntPtr[2],
                qpDeltaMap = IntPtr.Zero, qpDeltaMapSize = 0, reservedBitFields = 0,
                meHintRefPicDist = new ushort[2], _padding2 = 0,
                alphaBuffer = IntPtr.Zero,
                reserved3 = new uint[286], reserved4 = new IntPtr[59],
            };
            t0 = Stopwatch.GetTimestamp();
            uint encStatus = nvenc.EncodePicture!(encoder, ref picParams);
            t1 = Stopwatch.GetTimestamp();
            timings[S_Encode] = TicksToUs(t0, t1);

            if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                bsLen = -2;
                timings[S_Total] = TicksToUs(fStart, Stopwatch.GetTimestamp());
                return false;
            }

            // Stage 5: LockBitstream (GPU sync point — blocks until encode completes)
            var lockParams = new NvEncodeAPI.NV_ENC_LOCK_BITSTREAM
            {
                version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER,
                bitfields = 0, outputBitstream = bitstreamBuffer,
                sliceOffsets = IntPtr.Zero, frameIdx = 0, hwEncodeStatus = 0,
                numSlices = 0, bitstreamSizeInBytes = 0,
                outputTimeStamp = 0, outputDuration = 0,
                bitstreamBufferPtr = IntPtr.Zero,
                pictureType = 0, pictureStruct = 0, frameAvgQP = 0, frameSatd = 0,
                ltrFrameIdx = 0, ltrFrameBitmap = 0,
                reserved = new uint[13],
                intraMBCount = 0, interMBCount = 0,
                averageMVX = 0, averageMVY = 0,
                alphaLayerSizeInBytes = 0,
                reserved1 = new uint[218], reserved2 = new IntPtr[64],
            };
            t0 = Stopwatch.GetTimestamp();
            uint lockStatus = nvenc.LockBitstream!(encoder, ref lockParams);
            t1 = Stopwatch.GetTimestamp();
            timings[S_Lock] = TicksToUs(t0, t1);

            if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                bsLen = -2;
                timings[S_Total] = TicksToUs(fStart, Stopwatch.GetTimestamp());
                return false;
            }
            bsLen = (int)lockParams.bitstreamSizeInBytes;

            // Stage 6: UnlockBitstream
            t0 = Stopwatch.GetTimestamp();
            nvenc.UnlockBitstream!(encoder, bitstreamBuffer);
            t1 = Stopwatch.GetTimestamp();
            timings[S_Unlock] = TicksToUs(t0, t1);
        }
        finally
        {
            // Stage 7: UnmapInputResource
            t0 = Stopwatch.GetTimestamp();
            if (mappedInput != IntPtr.Zero)
                try { nvenc.UnmapInputResource!(encoder, mappedInput); } catch { }
            t1 = Stopwatch.GetTimestamp();
            timings[S_Unmap] = TicksToUs(t0, t1);
        }

        timings[S_Total] = TicksToUs(fStart, Stopwatch.GetTimestamp());
        return bsLen > 0;
    }

    private static long TicksToUs(long t0, long t1)
    {
        return (long)((t1 - t0) * 1_000_000.0 / Stopwatch.Frequency);
    }

    private static double Percentile(List<long> data, double p)
    {
        if (data.Count == 0) return 0;
        var s = new List<long>(data);
        s.Sort();
        if (s.Count == 1) return s[0];
        double rank = p * (s.Count - 1);
        int lo = (int)Math.Floor(rank);
        int hi = (int)Math.Ceiling(rank);
        if (lo == hi) return s[lo];
        return s[lo] + (rank - lo) * (s[hi] - s[lo]);
    }
}
