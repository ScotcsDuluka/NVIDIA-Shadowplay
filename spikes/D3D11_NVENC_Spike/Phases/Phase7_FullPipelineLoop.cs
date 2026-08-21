// Phases/Phase7_FullPipelineLoop.cs
//
// P1-B.2-V1 Spike — Phase 7: Full Pipeline 60-Second Loop with Metrics
//
// Goal: Run the complete capture → encode pipeline for 60 seconds and
// collect performance metrics. This is the most realistic end-to-end test.
//
// Pipeline per iteration:
//   AcquireNextFrame (DXGI Desktop Duplication)
//     ↓
//   CopyResource into encoder-owned GPU texture (D3D11_USAGE_DEFAULT)
//     ↓
//   ReleaseFrame (return OS-owned texture)
//     ↓
//   MapInputResource (NVENC)
//     ↓
//   EncodePicture (NVENC)
//     ↓
//   LockBitstream (read encoded bytes, accumulate bitrate)
//     ↓
//   UnlockBitstream
//     ↓
//   UnmapInputResource
//     ↓ (next frame)
//
// After 60 seconds:
//   UnregisterResource
//   DestroyBitstreamBuffer
//   DestroyEncoder
//   Release encoder texture
//
// Resources created ONCE (outside the loop):
//   - ID3D11Texture2D encoderTexture (GPU-resident, D3D11_USAGE_DEFAULT)
//   - NVENC encoder session
//   - NVENC registered resource (encoderTexture)
//   - NVENC bitstream buffer
//
// Metrics collected:
//   - frames_captured, frames_encoded, achieved_fps
//   - dropped_count, nvenc_errors, capture_wait_timeouts
//   - encode_latency_us: min/avg/max + P50/P95/P99
//   - total_encoded_bytes + bitrate_estimate (bps/kbps/Mbps)
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase7_FullPipelineLoop
{
    private const int DurationSeconds = 60;
    private const int AcquireTimeoutMs = 100;
    private const int WarmupFrames = 30;

    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 7 — Full Pipeline 60-Second Loop with Metrics");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Auto-run Phases 1-3 if not already done (Phase 7 depends on their shared context)
        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.WriteLine("  Phase 1-3 not yet run — auto-running them now...");
            Console.WriteLine();
            int p1 = Phase1_DeviceTest.Run();
            if (p1 != 0) { Console.Error.WriteLine("  FAIL: Phase 1 failed."); return 1; }
            int p2 = Phase2_DesktopDuplication.Run();
            if (p2 != 0) { Console.Error.WriteLine("  FAIL: Phase 2 failed."); return 1; }
            int p3 = Phase3_TextureOwnership.Run();
            if (p3 != 0) { Console.Error.WriteLine("  FAIL: Phase 3 failed."); return 1; }
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" Phase 7 — Full Pipeline (continuing after Phase 1-3)");
            Console.WriteLine("============================================================");
            Console.WriteLine();
        }

        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.Error.WriteLine("  FAIL: Phase 1-3 context missing.");
            return 1;
        }

        var device = SpikeSharedContext.Device;
        var duplDesc = SpikeSharedContext.DuplicationDesc.Value;
        uint texWidth = duplDesc.ModeDescription.Width;
        uint texHeight = duplDesc.ModeDescription.Height;

        // Create our OWN IDXGIOutputDuplication (Phase 2 disposes its own).
        Console.WriteLine("[7.0] Creating IDXGIOutputDuplication...");
        IDXGIOutput? primaryOutput = null;
        int outIdx = 0;
        while (SpikeSharedContext.TargetAdapter!.EnumOutputs((uint)outIdx, out IDXGIOutput out_).Success)
        {
            if (outIdx == 0) primaryOutput = out_;
            else out_.Dispose();
            outIdx++;
        }
        if (primaryOutput == null)
        {
            Console.Error.WriteLine("  FAIL: No outputs found.");
            return 1;
        }
        IDXGIOutput1 output1 = primaryOutput.QueryInterface<IDXGIOutput1>();
        IDXGIOutputDuplication duplication;
        try { duplication = output1.DuplicateOutput(device); }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  FAIL: DuplicateOutput: {ex.Message}");
            output1.Dispose(); primaryOutput.Dispose();
            return 1;
        }
        output1.Dispose();
        primaryOutput.Dispose();
        Console.WriteLine($"  PASS: Duplication created ({texWidth}x{texHeight}).");

        Console.WriteLine($"  D3D11 device:    0x{device.NativePointer.ToInt64():x16}");
        Console.WriteLine($"  Texture size:    {texWidth}x{texHeight}");
        Console.WriteLine($"  Duration:        {DurationSeconds}s");
        Console.WriteLine($"  AcquireTimeout:  {AcquireTimeoutMs}ms");
        Console.WriteLine($"  Warmup frames:   {WarmupFrames} (not counted)");
        Console.WriteLine();

        // ─── Step 1: Load NVENC + open session + init encoder ───
        Console.WriteLine("[7.1] Loading NVENC + opening encoder session...");
        using var nvenc = new NvEncFunctionTable();
        if (!nvenc.TryLoad())
        {
            Console.Error.WriteLine("  FAIL: Could not load NVENC.");
            return 1;
        }

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
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            if (encoder != IntPtr.Zero && nvenc.DestroyEncoder != null)
                try { nvenc.DestroyEncoder(encoder); } catch { }
            Console.Error.WriteLine($"  FAIL: OpenEncodeSessionEx: {status} ({NvEncodeAPI.NvencStatusToString(status)})");
            return 1;
        }
        Console.WriteLine($"  PASS: Encoder session opened. Handle: 0x{encoder.ToInt64():x16}");

        // Track for cleanup
        ID3D11Texture2D? encoderTexture = null;
        IntPtr bitstreamBuffer = IntPtr.Zero;
        IntPtr registeredResource = IntPtr.Zero;
        bool encoderOpened = true;

        try
        {
            // ─── Step 2: Initialize encoder ───
            Console.WriteLine("[7.2] Initializing encoder (H.264, Default preset)...");
            var initParams = new NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS
            {
                version = NvEncodeAPI.MakeStructVersion(5) | (1u << 31),
                encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID,
                presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID,
                encodeWidth = texWidth,
                encodeHeight = texHeight,
                darWidth = texWidth,
                darHeight = texHeight,
                frameRateNum = 60,
                frameRateDen = 1,
                enableEncodeAsync = 0,
                enablePTD = 1,
                bitFields = 0,
                privDataSize = 0,
                privData = IntPtr.Zero,
                encodeConfig = IntPtr.Zero,
                maxEncodeWidth = texWidth,
                maxEncodeHeight = texHeight,
                maxMEHintCountsPerBlockL0 = 0,
                maxMEHintCountsPerBlockL1 = 0,
                reserved = new uint[289],
                reserved2 = new IntPtr[64],
            };
            status = nvenc.InitializeEncoder!(encoder, ref initParams);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: InitializeEncoder: {status} ({NvEncodeAPI.NvencStatusToString(status)})");
                return 1;
            }
            Console.WriteLine("  PASS: Encoder initialized.");

            // ─── Step 3: Create bitstream buffer ───
            Console.WriteLine("[7.3] Creating bitstream buffer...");
            var bsParams = new NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER
            {
                version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER,
                size = 0,
                memoryHeap = 0,
                _padding = 0,
                bitstreamBuffer = IntPtr.Zero,
                reserved1 = IntPtr.Zero,
                reserved2 = IntPtr.Zero,
                reserved3 = new uint[226],
                reserved4 = new IntPtr[64],
            };
            status = nvenc.CreateBitstreamBuffer!(encoder, ref bsParams);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: CreateBitstreamBuffer: {status}");
                return 1;
            }
            bitstreamBuffer = bsParams.bitstreamBuffer;
            Console.WriteLine($"  PASS: Bitstream buffer: 0x{bitstreamBuffer.ToInt64():x16}");

            // ─── Step 4: Create encoder-owned GPU texture (USAGE_DEFAULT, NOT STAGING) ───
            Console.WriteLine("[7.4] Creating encoder-owned GPU texture (D3D11_USAGE_DEFAULT)...");
            var texDesc = new Texture2DDescription
            {
                Width = texWidth,
                Height = texHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };
            encoderTexture = device.CreateTexture2D(texDesc);
            Console.WriteLine($"  PASS: Encoder texture: 0x{encoderTexture.NativePointer.ToInt64():x16}");

            // ─── Step 5: Register encoder texture ONCE with NVENC ───
            Console.WriteLine("[7.5] Registering encoder texture with NVENC...");
            var regParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
            {
                version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
                resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
                width = texWidth,
                height = texHeight,
                pitch = 0,
                subResourceIndex = 0,
                resourceToRegister = encoderTexture.NativePointer,
                registeredResource = IntPtr.Zero,
                bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                reserved1 = new uint[248],
                reserved2 = new IntPtr[62],
            };
            status = nvenc.RegisterResource!(encoder, ref regParams);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: RegisterResource: {status}");
                return 1;
            }
            registeredResource = regParams.registeredResource;
            Console.WriteLine($"  PASS: Registered: 0x{registeredResource.ToInt64():x16}");
            Console.WriteLine();

            // ─── Step 6: Warmup (30 frames, not counted) ───
            Console.WriteLine($"[7.6] Warmup ({WarmupFrames} frames)...");
            int warmupEncoded = 0;
            for (int i = 0; i < WarmupFrames; i++)
            {
                if (!EncodeOneFrame(device, duplication, encoder, nvenc, encoderTexture,
                    registeredResource, bitstreamBuffer, texWidth, texHeight, out long _))
                    continue;
                warmupEncoded++;
            }
            Console.WriteLine($"  Warmup: {warmupEncoded}/{WarmupFrames} frames encoded.");
            Console.WriteLine();

            // ─── Step 7: Timed 60-second loop ───
            Console.WriteLine($"[7.7] Timed loop ({DurationSeconds}s)...");
            var sw = Stopwatch.StartNew();
            var duration = TimeSpan.FromSeconds(DurationSeconds);

            long framesCaptured = 0, framesEncoded = 0, droppedCount = 0, nvencErrors = 0, waitTimeouts = 0;
            long totalEncodedBytes = 0;
            long minEncUs = long.MaxValue, maxEncUs = 0;
            double sumEncUs = 0;
            var latencies = new List<long>(8 * 1024);

            while (sw.Elapsed < duration)
            {
                long t0 = Stopwatch.GetTimestamp();

                bool ok = EncodeOneFrame(device, duplication, encoder, nvenc, encoderTexture,
                    registeredResource, bitstreamBuffer, texWidth, texHeight, out int bsLen);

                long t1 = Stopwatch.GetTimestamp();
                long encUs = (long)((t1 - t0) * 1_000_000.0 / Stopwatch.Frequency);

                if (!ok)
                {
                    if (bsLen == -1) waitTimeouts++;
                    else { droppedCount++; nvencErrors++; }
                    continue;
                }

                framesCaptured++;
                framesEncoded++;
                totalEncodedBytes += bsLen;
                latencies.Add(encUs);
                if (encUs < minEncUs) minEncUs = encUs;
                if (encUs > maxEncUs) maxEncUs = encUs;
                sumEncUs += encUs;
            }
            sw.Stop();

            double elapsedSec = sw.Elapsed.TotalSeconds;
            double avgEncUs = framesEncoded > 0 ? sumEncUs / framesEncoded : 0;
            double achievedFps = framesEncoded / elapsedSec;
            double bitrateBps = elapsedSec > 0 ? (totalEncodedBytes * 8.0) / elapsedSec : 0;
            double p50 = Percentile(latencies, 0.50);
            double p95 = Percentile(latencies, 0.95);
            double p99 = Percentile(latencies, 0.99);

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" Phase 7 — METRICS");
            Console.WriteLine("============================================================");
            Console.WriteLine($"  duration_seconds:          {elapsedSec:F3}");
            Console.WriteLine($"  frames_captured:           {framesCaptured}");
            Console.WriteLine($"  frames_encoded:            {framesEncoded}");
            Console.WriteLine($"  dropped_count:             {droppedCount}");
            Console.WriteLine($"  nvenc_errors:              {nvencErrors}");
            Console.WriteLine($"  capture_wait_timeouts:     {waitTimeouts}");
            Console.WriteLine($"  achieved_fps:              {achievedFps:F2}");
            Console.WriteLine($"  total_encoded_bytes:       {totalEncodedBytes}");
            Console.WriteLine($"  bitrate_bps:               {bitrateBps:F0}");
            Console.WriteLine($"  bitrate_kbps:              {bitrateBps / 1000:F2}");
            Console.WriteLine($"  bitrate_mbps:              {bitrateBps / 1_000_000:F3}");
            Console.WriteLine($"  encode_latency_us (min):   {minEncUs}");
            Console.WriteLine($"  encode_latency_us (avg):   {avgEncUs:F1}");
            Console.WriteLine($"  encode_latency_us (max):   {maxEncUs}");
            Console.WriteLine($"  encode_latency_us P50:     {p50:F1}");
            Console.WriteLine($"  encode_latency_us P95:     {p95:F1}");
            Console.WriteLine($"  encode_latency_us P99:     {p99:F1}");
            Console.WriteLine("============================================================");
            Console.WriteLine();

            if (framesEncoded > 0 && nvencErrors == 0)
                Console.WriteLine("  Phase 7: PASS");
            else if (framesEncoded > 0)
                Console.WriteLine($"  Phase 7: PARTIAL ({nvencErrors} NVENC errors)");
            else
                Console.WriteLine("  Phase 7: FAIL (0 frames encoded)");

            return framesEncoded > 0 ? 0 : 1;
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("[7.cleanup] Best-effort cleanup...");
            if (registeredResource != IntPtr.Zero && nvenc.UnregisterResource != null)
                try { nvenc.UnregisterResource(encoder, registeredResource); } catch { }
            if (bitstreamBuffer != IntPtr.Zero && nvenc.DestroyBitstreamBuffer != null)
                try { nvenc.DestroyBitstreamBuffer(encoder, bitstreamBuffer); } catch { }
            if (encoderTexture != null)
                try { encoderTexture.Dispose(); } catch { }
            try { duplication.Dispose(); } catch { }
            if (encoderOpened && nvenc.DestroyEncoder != null)
                try { nvenc.DestroyEncoder(encoder); } catch { }
            Console.WriteLine("  Cleanup done.");
        }
    }

    /// <summary>
    /// Encode one frame: AcquireNextFrame → CopyResource → ReleaseFrame →
    /// Map → Encode → Lock → Unlock → Unmap.
    /// Returns true on success; bsLen = encoded byte count.
    /// Returns false on failure; bsLen = -1 for WAIT_TIMEOUT, -2 for NVENC error.
    /// </summary>
    private static bool EncodeOneFrame(
        ID3D11Device device, IDXGIOutputDuplication duplication,
        IntPtr encoder, NvEncFunctionTable nvenc,
        ID3D11Texture2D encoderTexture, IntPtr registeredResource,
        IntPtr bitstreamBuffer, uint texWidth, uint texHeight,
        out int bsLen)
    {
        bsLen = 0;
        var deviceCtx = device.ImmediateContext;

        // AcquireNextFrame
        var acquireResult = duplication.AcquireNextFrame(AcquireTimeoutMs, out var frameInfo, out var desktopResource);
        if (acquireResult.Failure)
        {
            if (acquireResult.Code == ResultCode.WaitTimeout)
            {
                bsLen = -1;
                return false;
            }
            bsLen = -2;
            if (desktopResource != null) desktopResource.Dispose();
            return false;
        }

        var desktopTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
        desktopResource.Dispose();

        // CopyResource into encoder texture
        deviceCtx.CopyResource(encoderTexture, desktopTexture);

        // ReleaseFrame (return OS-owned texture)
        duplication.ReleaseFrame();
        desktopTexture.Dispose();

        // MapInputResource
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
        deviceCtx.Flush();
        uint mapStatus = nvenc.MapInputResource!(encoder, ref mapParams);
        if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            bsLen = -2;
            return false;
        }
        IntPtr mappedInput = mapParams.mappedResource;

        try
        {
            // EncodePicture
            var picParams = new NvEncodeAPI.NV_ENC_PIC_PARAMS
            {
                version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
                inputWidth = texWidth,
                inputHeight = texHeight,
                inputPitch = 0,
                encodePicFlags = 0,
                frameIdx = 0,
                inputTimeStamp = 0,
                inputDuration = 0,
                inputBuffer = mappedInput,
                outputBitstream = bitstreamBuffer,
                completionEvent = IntPtr.Zero,
                bufferFmt = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                pictureStruct = 1,
                pictureType = 0,
                _padding1 = 0,
                codecPicParams = new byte[1536],
                meHintCountsPerBlock = new byte[32],
                meExternalHints = IntPtr.Zero,
                reserved1 = new uint[6],
                reserved2 = new IntPtr[2],
                qpDeltaMap = IntPtr.Zero,
                qpDeltaMapSize = 0,
                reservedBitFields = 0,
                meHintRefPicDist = new ushort[2],
                _padding2 = 0,
                alphaBuffer = IntPtr.Zero,
                reserved3 = new uint[286],
                reserved4 = new IntPtr[59],
            };
            uint encStatus = nvenc.EncodePicture!(encoder, ref picParams);
            if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                bsLen = -2;
                return false;
            }

            // LockBitstream
            var lockParams = new NvEncodeAPI.NV_ENC_LOCK_BITSTREAM
            {
                version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER,
                bitfields = 0,
                outputBitstream = bitstreamBuffer,
                sliceOffsets = IntPtr.Zero,
                frameIdx = 0,
                hwEncodeStatus = 0,
                numSlices = 0,
                bitstreamSizeInBytes = 0,
                outputTimeStamp = 0,
                outputDuration = 0,
                bitstreamBufferPtr = IntPtr.Zero,
                pictureType = 0,
                pictureStruct = 0,
                frameAvgQP = 0,
                frameSatd = 0,
                ltrFrameIdx = 0,
                ltrFrameBitmap = 0,
                reserved = new uint[13],
                intraMBCount = 0,
                interMBCount = 0,
                averageMVX = 0,
                averageMVY = 0,
                alphaLayerSizeInBytes = 0,
                reserved1 = new uint[218],
                reserved2 = new IntPtr[64],
            };
            uint lockStatus = nvenc.LockBitstream!(encoder, ref lockParams);
            if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                bsLen = -2;
                return false;
            }
            bsLen = (int)lockParams.bitstreamSizeInBytes;

            // UnlockBitstream
            nvenc.UnlockBitstream!(encoder, bitstreamBuffer);
        }
        finally
        {
            // UnmapInputResource (always, even on failure)
            if (mappedInput != IntPtr.Zero)
                try { nvenc.UnmapInputResource!(encoder, mappedInput); } catch { }
        }

        return bsLen > 0;
    }

    private static double Percentile(List<long> sorted, double p)
    {
        if (sorted.Count == 0) return 0;
        var s = new List<long>(sorted);
        s.Sort();
        if (s.Count == 1) return s[0];
        double rank = p * (s.Count - 1);
        int lo = (int)Math.Floor(rank);
        int hi = (int)Math.Ceiling(rank);
        if (lo == hi) return s[lo];
        return s[lo] + (rank - lo) * (s[hi] - s[lo]);
    }
}
