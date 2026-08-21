// Phases/Phase9_SynchronizationIsolation.cs
//
// P1-B.2 Spike — Phase 9: Pipeline Synchronization Isolation
//
// Goal: Determine whether the throughput ceiling of the D3D11 → NVENC pipeline
// is bounded by DXGI capture, NVENC encode, GPU synchronization, CPU
// orchestration, or queue depth.
//
// 5 experiments:
//   A: Capture-only baseline (Acquire → Copy → Release, no NVENC)
//   B: NVENC-only baseline (Map → Encode → Lock → Unlock → Unmap, no DXGI)
//   C: Full pipeline (DXGI → Copy → NVENC) — same as Phase 7/8
//   D: Full pipeline WITHOUT explicit Flush (test Flush necessity)
//   E: Full pipeline with queue depth 1/2/4/8 (test in-flight pipelining)
//
// Each experiment reports per-stage timing: min/avg/P50/P95/P99/max (microseconds)
//
// Final verdict: DXGI_BOUND / NVENC_BOUND / GPU_SYNC_BOUND / CPU_BOUND / QUEUE_DEPTH_BOUND / MIXED / INCONCLUSIVE
//
// SPDX-License-Identifier: MIT
// Spike code — not production. Phase 7 untouched.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase9_SynchronizationIsolation
{
    private const int DurationSeconds = 60;
    private const int AcquireTimeoutMs = 100;
    private const int WarmupFrames = 30;

    // Stage indices (same as Phase 8)
    private const int S_Acquire = 0;
    private const int S_Copy = 1;
    private const int S_Release = 2;
    private const int S_Map = 3;
    private const int S_Encode = 4;
    private const int S_Lock = 5;
    private const int S_Unlock = 6;
    private const int S_Unmap = 7;
    private const int S_Total = 8;
    private const int NUM_STAGES = 9;

    private static readonly string[] StageNames =
    {
        "AcquireNextFrame", "CopyResource", "ReleaseFrame",
        "MapInputResource", "EncodePicture", "LockBitstream",
        "UnlockBitstream", "UnmapInputResource", "Total",
    };

    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 9 — Pipeline Synchronization Isolation");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Auto-run Phases 1-3 if not already done
        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.WriteLine("  Phase 1-3 not yet run — auto-running...");
            int p1 = Phase1_DeviceTest.Run(); if (p1 != 0) { Console.Error.WriteLine("  FAIL: Phase 1."); return 1; }
            int p2 = Phase2_DesktopDuplication.Run(); if (p2 != 0) { Console.Error.WriteLine("  FAIL: Phase 2."); return 1; }
            int p3 = Phase3_TextureOwnership.Run(); if (p3 != 0) { Console.Error.WriteLine("  FAIL: Phase 3."); return 1; }
            Console.WriteLine();
        }

        var device = SpikeSharedContext.Device!;
        var duplDesc = SpikeSharedContext.DuplicationDesc!.Value;
        uint texWidth = duplDesc.ModeDescription.Width;
        uint texHeight = duplDesc.ModeDescription.Height;

        Console.WriteLine($"  Texture: {texWidth}x{texHeight}");
        Console.WriteLine($"  Duration per experiment: {DurationSeconds}s");
        Console.WriteLine();

        // ─── Setup: create duplication + NVENC encoder + encoder texture + bitstream buffer ───
        // Shared across all experiments
        Console.WriteLine("[9.0] Setup...");
        var setup = SetupPipeline(device, texWidth, texHeight);
        if (setup == null) return 1;
        var dup = setup.Value.duplication;
        var encoder = setup.Value.encoder;
        var nvenc = setup.Value.nvenc;
        var encoderTexture = setup.Value.encoderTexture;
        var registeredResource = setup.Value.registeredResource;
        var bitstreamBuffer = setup.Value.bitstreamBuffer;

        try
        {
            // ═══ Experiment A: Capture-only baseline ═══
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" Experiment A: Capture-Only Baseline (Acquire → Copy → Release)");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine("  No NVENC. Measures DXGI + GPU copy cost only.");
            Console.WriteLine();
            var expA = RunExperimentA(device, dup, encoderTexture, texWidth, texHeight);
            PrintExperimentResults("A", expA);

            // ═══ Experiment B: NVENC-only baseline ═══
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" Experiment B: NVENC-Only Baseline (Map → Encode → Lock → Unlock → Unmap)");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine("  No DXGI capture. Encoder texture already has data from Experiment A.");
            Console.WriteLine("  Measures pure NVENC encode cost.");
            Console.WriteLine();
            var expB = RunExperimentB(device, encoder, nvenc, registeredResource, bitstreamBuffer, texWidth, texHeight);
            PrintExperimentResults("B", expB);

            // ═══ Experiment C: Full pipeline baseline ═══
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" Experiment C: Full Pipeline (DXGI → Copy → NVENC, with Flush)");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine("  Same as Phase 7/8. Includes deviceCtx.Flush() before MapInputResource.");
            Console.WriteLine();
            var expC = RunExperimentC(device, dup, encoder, nvenc, encoderTexture,
                registeredResource, bitstreamBuffer, texWidth, texHeight, useFlush: true);
            PrintExperimentResults("C", expC);

            // ═══ Experiment D: Full pipeline WITHOUT Flush ═══
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" Experiment D: Full Pipeline WITHOUT Flush");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine("  Same as C but no deviceCtx.Flush() before MapInputResource.");
            Console.WriteLine("  Tests whether Flush is necessary for correctness / performance.");
            Console.WriteLine();
            var expD = RunExperimentC(device, dup, encoder, nvenc, encoderTexture,
                registeredResource, bitstreamBuffer, texWidth, texHeight, useFlush: false);
            PrintExperimentResults("D", expD);

            // ═══ Experiment E: Queue depth 1/2/4/8 ═══
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" Experiment E: Queue Depth Analysis (1/2/4/8)");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine("  Tests whether multiple in-flight encode requests improve throughput.");
            Console.WriteLine("  NOTE: Current spike uses synchronous NVENC (enableEncodeAsync=0).");
            Console.WriteLine("  Queue depth > 1 requires multiple registered resources + bitstream");
            Console.WriteLine("  buffers. This experiment creates N of each and rotates.");
            Console.WriteLine();

            var expE1 = RunExperimentE(device, dup, encoder, nvenc, texWidth, texHeight, queueDepth: 1);
            PrintExperimentResults("E (queue=1)", expE1);

            var expE2 = RunExperimentE(device, dup, encoder, nvenc, texWidth, texHeight, queueDepth: 2);
            PrintExperimentResults("E (queue=2)", expE2);

            var expE4 = RunExperimentE(device, dup, encoder, nvenc, texWidth, texHeight, queueDepth: 4);
            PrintExperimentResults("E (queue=4)", expE4);

            var expE8 = RunExperimentE(device, dup, encoder, nvenc, texWidth, texHeight, queueDepth: 8);
            PrintExperimentResults("E (queue=8)", expE8);

            // ═══ Summary + Verdict ═══
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" Phase 9 — Summary + Verdict");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine();

            // Compare experiments
            Console.WriteLine("  Experiment comparison:");
            Console.WriteLine($"    {'Exp':<15} {'FPS':>8} {'Frames':>8} {'Drops':>6} {'Errors':>7} {'LockAvg':>10} {'EncAvg':>10} {'AcqAvg':>10}");
            PrintExpRow("A (cap-only)", expA);
            PrintExpRow("B (nvenc-only)", expB);
            PrintExpRow("C (full+flush)", expC);
            PrintExpRow("D (full no-flush)", expD);
            PrintExpRow("E q=1", expE1);
            PrintExpRow("E q=2", expE2);
            PrintExpRow("E q=4", expE4);
            PrintExpRow("E q=8", expE8);

            Console.WriteLine();
            Console.WriteLine("  Queue depth comparison:");
            Console.WriteLine($"    {'Depth':>6} {'FPS':>8} {'LockAvg':>10} {'LockP95':>10} {'LockP99':>10} {'EncAvg':>10} {'AcqAvg':>10} {'Drops':>6}");
            PrintQueueRow(1, expE1);
            PrintQueueRow(2, expE2);
            PrintQueueRow(4, expE4);
            PrintQueueRow(8, expE8);

            Console.WriteLine();
            Console.WriteLine("  Phase 8 baseline comparison:");
            Console.WriteLine("    Phase 8 Run A: 16.37 FPS / 14M errors (ANOMALOUS — likely driver/system issue)");
            Console.WriteLine("    Phase 8 Run B: 80.18 FPS / 0 errors (HEALTHY baseline)");
            Console.WriteLine("    Phase 9 Experiment C is the closest equivalent to Phase 8.");
            Console.WriteLine("    If Phase 9 C shows similar anomaly (high errors / low FPS), it confirms");
            Console.WriteLine("    a reproducible failure mode. If not, Phase 8 Run A was transient.");
            Console.WriteLine();

            // Verdict logic based on measured evidence
            Console.WriteLine("  Bottleneck classification:");
            double capFps = expA.fps;
            double nvencFps = expB.fps;
            double fullFps = expC.fps;
            double lockAvgC = GetStageAvg(expC, S_Lock);
            double encAvgC = GetStageAvg(expC, S_Encode);
            double acqAvgC = GetStageAvg(expC, S_Acquire);
            double copyAvgC = GetStageAvg(expC, S_Copy);

            Console.WriteLine($"    Capture-only FPS (A):     {capFps:F2}");
            Console.WriteLine($"    NVENC-only FPS (B):       {nvencFps:F2}");
            Console.WriteLine($"    Full pipeline FPS (C):    {fullFps:F2}");
            Console.WriteLine($"    LockBitstream avg (C):    {lockAvgC:F1} us");
            Console.WriteLine($"    EncodePicture avg (C):   {encAvgC:F1} us");
            Console.WriteLine($"    AcquireNextFrame avg (C): {acqAvgC:F1} us");
            Console.WriteLine($"    CopyResource avg (C):     {copyAvgC:F1} us");
            Console.WriteLine();

            // Determine verdict from evidence
            string verdict;
            string reason;

            if (fullFps < 1.0 || expC.nvencErrors > 0)
            {
                verdict = "INCONCLUSIVE";
                reason = $"Full pipeline had {expC.nvencErrors} NVENC errors or FPS < 1.0. Cannot classify bottleneck with faulty pipeline.";
            }
            else if (capFps < fullFps * 0.95)
            {
                verdict = "DXGI_BOUND";
                reason = $"Capture-only FPS ({capFps:F2}) < full pipeline FPS ({fullFps:F2}) * 0.95. DXGI acquisition is the ceiling.";
            }
            else if (nvencFps < fullFps * 0.95)
            {
                verdict = "NVENC_BOUND";
                reason = $"NVENC-only FPS ({nvencFps:F2}) < full pipeline FPS ({fullFps:F2}) * 0.95. NVENC encode is the ceiling.";
            }
            else if (lockAvgC > encAvgC * 2 && lockAvgC > 1000)
            {
                verdict = "GPU_SYNC_BOUND";
                reason = $"LockBitstream avg ({lockAvgC:F1} us) > 2x EncodePicture avg ({encAvgC:F1} us) and > 1000 us. GPU synchronization is the dominant cost.";
            }
            else if (expE8.fps > expE1.fps * 1.15)
            {
                verdict = "QUEUE_DEPTH_BOUND";
                reason = $"Queue=8 FPS ({expE8.fps:F2}) > queue=1 FPS ({expE1.fps:F2}) * 1.15. Pipeline depth limits throughput.";
            }
            else if (acqAvgC + copyAvgC > lockAvgC + encAvgC)
            {
                verdict = "MIXED";
                reason = $"DXGI+Copy ({acqAvgC + copyAvgC:F1} us) > NVENC+Lock ({lockAvgC + encAvgC:F1} us). Both capture and encode contribute significantly.";
            }
            else
            {
                verdict = "CPU_BOUND";
                reason = $"No single stage dominates. Total overhead is CPU orchestration. Acquire avg={acqAvgC:F1}, Copy avg={copyAvgC:F1}, Encode avg={encAvgC:F1}, Lock avg={lockAvgC:F1}.";
            }

            Console.WriteLine();
            Console.WriteLine($"  VERDICT: {verdict}");
            Console.WriteLine($"  REASON:  {reason}");
            Console.WriteLine();
            Console.WriteLine("  Measurement limitations:");
            Console.WriteLine("    1. AcquireNextFrame cannot distinguish CPU vs compositor wait (no DXGI profiler).");
            Console.WriteLine("    2. CopyResource measures CPU command-submit only (GPU may not be done).");
            Console.WriteLine("    3. EncodePicture + LockBitstream may overlap on GPU (not separable without GPU timestamps).");
            Console.WriteLine("    4. Queue depth experiment uses synchronous NVENC (enableEncodeAsync=0).");
            Console.WriteLine("       True async pipelining would require NVENC events + threading (beyond spike scope).");
            Console.WriteLine("       Queue depth > 1 here means N registered textures + N bitstream buffers,");
            Console.WriteLine("       submitted back-to-back before any LockBitstream (batches the GPU sync).");
            Console.WriteLine("============================================================");
            Console.WriteLine();
            return 0;
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("[9.cleanup] Cleanup...");
            try { if (registeredResource != IntPtr.Zero && nvenc.UnregisterResource != null) nvenc.UnregisterResource(encoder, registeredResource); } catch { }
            try { if (bitstreamBuffer != IntPtr.Zero && nvenc.DestroyBitstreamBuffer != null) nvenc.DestroyBitstreamBuffer(encoder, bitstreamBuffer); } catch { }
            try { encoderTexture?.Dispose(); } catch { }
            try { nvenc.DestroyEncoder?.Invoke(encoder); } catch { }
            try { dup.Dispose(); } catch { }
            Console.WriteLine("  Cleanup done.");
        }
    }

    // ═══ Result struct ═══
    private struct ExpResult
    {
        public double durationSec;
        public long frames;
        public long drops;
        public long nvencErrors;
        public long waitTimeouts;
        public double fps;
        public long totalBytes;
        public List<long>[] stageLatencies;
    }

    // ═══ Experiment A: Capture-only ═══
    private static ExpResult RunExperimentA(
        ID3D11Device device, IDXGIOutputDuplication dup,
        ID3D11Texture2D encoderTexture, uint texW, uint texH)
    {
        var r = new ExpResult { stageLatencies = NewStageLists() };
        var deviceCtx = device.ImmediateContext;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < TimeSpan.FromSeconds(DurationSeconds))
        {
            long fStart = Stopwatch.GetTimestamp();
            long t0, t1;

            // AcquireNextFrame
            t0 = Stopwatch.GetTimestamp();
            var acq = dup.AcquireNextFrame(AcquireTimeoutMs, out var fi, out var dr);
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Acquire].Add(TicksToUs(t0, t1));

            if (acq.Failure)
            {
                if (acq.Code == Vortice.DXGI.ResultCode.WaitTimeout) r.waitTimeouts++;
                else r.drops++;
                if (dr != null) dr.Dispose();
                r.stageLatencies[S_Total].Add(TicksToUs(fStart, Stopwatch.GetTimestamp()));
                continue;
            }

            var dt = dr.QueryInterface<ID3D11Texture2D>();
            dr.Dispose();

            // CopyResource
            t0 = Stopwatch.GetTimestamp();
            deviceCtx.CopyResource(encoderTexture, dt);
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Copy].Add(TicksToUs(t0, t1));

            // ReleaseFrame
            t0 = Stopwatch.GetTimestamp();
            dup.ReleaseFrame();
            dt.Dispose();
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Release].Add(TicksToUs(t0, t1));

            // No NVENC stages
            r.stageLatencies[S_Map].Add(0);
            r.stageLatencies[S_Encode].Add(0);
            r.stageLatencies[S_Lock].Add(0);
            r.stageLatencies[S_Unlock].Add(0);
            r.stageLatencies[S_Unmap].Add(0);

            r.frames++;
            r.stageLatencies[S_Total].Add(TicksToUs(fStart, Stopwatch.GetTimestamp()));
        }
        sw.Stop();
        r.durationSec = sw.Elapsed.TotalSeconds;
        r.fps = r.frames / r.durationSec;
        return r;
    }

    // ═══ Experiment B: NVENC-only (no DXGI) ═══
    private static ExpResult RunExperimentB(
        ID3D11Device device, IntPtr encoder, NvEncFunctionTable nvenc,
        IntPtr registeredResource, IntPtr bitstreamBuffer, uint texW, uint texH)
    {
        var r = new ExpResult { stageLatencies = NewStageLists() };
        var deviceCtx = device.ImmediateContext;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < TimeSpan.FromSeconds(DurationSeconds))
        {
            long fStart = Stopwatch.GetTimestamp();
            long t0, t1;

            r.stageLatencies[S_Acquire].Add(0);
            r.stageLatencies[S_Copy].Add(0);
            r.stageLatencies[S_Release].Add(0);

            // MapInputResource
            t0 = Stopwatch.GetTimestamp();
            deviceCtx.Flush();
            var mapParams = MakeMapParams(registeredResource);
            uint mapStatus = nvenc.MapInputResource!(encoder, ref mapParams);
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Map].Add(TicksToUs(t0, t1));

            if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }
            IntPtr mappedInput = mapParams.mappedResource;

            try
            {
                // EncodePicture
                var picParams = MakePicParams(mappedInput, bitstreamBuffer, texW, texH);
                t0 = Stopwatch.GetTimestamp();
                uint encStatus = nvenc.EncodePicture!(encoder, ref picParams);
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Encode].Add(TicksToUs(t0, t1));

                if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }

                // LockBitstream
                var lockParams = MakeLockParams(bitstreamBuffer);
                t0 = Stopwatch.GetTimestamp();
                uint lockStatus = nvenc.LockBitstream!(encoder, ref lockParams);
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Lock].Add(TicksToUs(t0, t1));

                if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }
                r.totalBytes += (int)lockParams.bitstreamSizeInBytes;

                // UnlockBitstream
                t0 = Stopwatch.GetTimestamp();
                nvenc.UnlockBitstream!(encoder, bitstreamBuffer);
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Unlock].Add(TicksToUs(t0, t1));
            }
            finally
            {
                // UnmapInputResource
                t0 = Stopwatch.GetTimestamp();
                if (mappedInput != IntPtr.Zero)
                    try { nvenc.UnmapInputResource!(encoder, mappedInput); } catch { }
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Unmap].Add(TicksToUs(t0, t1));
            }

            r.frames++;
            r.stageLatencies[S_Total].Add(TicksToUs(fStart, Stopwatch.GetTimestamp()));
        }
        sw.Stop();
        r.durationSec = sw.Elapsed.TotalSeconds;
        r.fps = r.frames / r.durationSec;
        return r;
    }

    // ═══ Experiment C/D: Full pipeline (with/without Flush) ═══
    private static ExpResult RunExperimentC(
        ID3D11Device device, IDXGIOutputDuplication dup,
        IntPtr encoder, NvEncFunctionTable nvenc,
        ID3D11Texture2D encoderTexture, IntPtr registeredResource,
        IntPtr bitstreamBuffer, uint texW, uint texH, bool useFlush)
    {
        var r = new ExpResult { stageLatencies = NewStageLists() };
        var deviceCtx = device.ImmediateContext;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < TimeSpan.FromSeconds(DurationSeconds))
        {
            long fStart = Stopwatch.GetTimestamp();
            long t0, t1;

            // AcquireNextFrame
            t0 = Stopwatch.GetTimestamp();
            var acq = dup.AcquireNextFrame(AcquireTimeoutMs, out var fi, out var dr);
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Acquire].Add(TicksToUs(t0, t1));

            if (acq.Failure)
            {
                if (acq.Code == Vortice.DXGI.ResultCode.WaitTimeout) r.waitTimeouts++;
                else r.drops++;
                if (dr != null) dr.Dispose();
                r.stageLatencies[S_Total].Add(TicksToUs(fStart, Stopwatch.GetTimestamp()));
                continue;
            }

            var dt = dr.QueryInterface<ID3D11Texture2D>();
            dr.Dispose();

            // CopyResource
            t0 = Stopwatch.GetTimestamp();
            deviceCtx.CopyResource(encoderTexture, dt);
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Copy].Add(TicksToUs(t0, t1));

            // ReleaseFrame
            t0 = Stopwatch.GetTimestamp();
            dup.ReleaseFrame();
            dt.Dispose();
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Release].Add(TicksToUs(t0, t1));

            // MapInputResource (with/without Flush)
            t0 = Stopwatch.GetTimestamp();
            if (useFlush) deviceCtx.Flush();
            var mapParams = MakeMapParams(registeredResource);
            uint mapStatus = nvenc.MapInputResource!(encoder, ref mapParams);
            t1 = Stopwatch.GetTimestamp();
            r.stageLatencies[S_Map].Add(TicksToUs(t0, t1));

            if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }
            IntPtr mappedInput = mapParams.mappedResource;

            try
            {
                // EncodePicture
                var picParams = MakePicParams(mappedInput, bitstreamBuffer, texW, texH);
                t0 = Stopwatch.GetTimestamp();
                uint encStatus = nvenc.EncodePicture!(encoder, ref picParams);
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Encode].Add(TicksToUs(t0, t1));

                if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }

                // LockBitstream
                var lockParams = MakeLockParams(bitstreamBuffer);
                t0 = Stopwatch.GetTimestamp();
                uint lockStatus = nvenc.LockBitstream!(encoder, ref lockParams);
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Lock].Add(TicksToUs(t0, t1));

                if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }
                r.totalBytes += (int)lockParams.bitstreamSizeInBytes;

                // UnlockBitstream
                t0 = Stopwatch.GetTimestamp();
                nvenc.UnlockBitstream!(encoder, bitstreamBuffer);
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Unlock].Add(TicksToUs(t0, t1));
            }
            finally
            {
                t0 = Stopwatch.GetTimestamp();
                if (mappedInput != IntPtr.Zero)
                    try { nvenc.UnmapInputResource!(encoder, mappedInput); } catch { }
                t1 = Stopwatch.GetTimestamp();
                r.stageLatencies[S_Unmap].Add(TicksToUs(t0, t1));
            }

            r.frames++;
            r.stageLatencies[S_Total].Add(TicksToUs(fStart, Stopwatch.GetTimestamp()));
        }
        sw.Stop();
        r.durationSec = sw.Elapsed.TotalSeconds;
        r.fps = r.frames / r.durationSec;
        return r;
    }

    // ═══ Experiment E: Queue depth N ═══
    // Creates N registered textures + N bitstream buffers.
    // Submits N frames back-to-back (Acquire→Copy→Map→Encode), then
    // Lock→Unlock→Unmap all N. This batches the GPU sync.
    private static ExpResult RunExperimentE(
        ID3D11Device device, IDXGIOutputDuplication dup,
        IntPtr encoder, NvEncFunctionTable nvenc,
        uint texW, uint texH, int queueDepth)
    {
        var r = new ExpResult { stageLatencies = NewStageLists() };
        var deviceCtx = device.ImmediateContext;

        // Create N encoder textures + N registered resources + N bitstream buffers
        var textures = new ID3D11Texture2D[queueDepth];
        var regResources = new IntPtr[queueDepth];
        var bitstreamBuffers = new IntPtr[queueDepth];

        for (int i = 0; i < queueDepth; i++)
        {
            var texDesc = new Texture2DDescription
            {
                Width = texW, Height = texH, MipLevels = 1, ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm, SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default, BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None, MiscFlags = ResourceOptionFlags.None,
            };
            textures[i] = device.CreateTexture2D(texDesc);

            var regParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
            {
                version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
                resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
                width = texW, height = texH, pitch = 0, subResourceIndex = 0,
                resourceToRegister = textures[i].NativePointer, registeredResource = IntPtr.Zero,
                bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                reserved1 = new uint[248], reserved2 = new IntPtr[62],
            };
            nvenc.RegisterResource!(encoder, ref regParams);
            regResources[i] = regParams.registeredResource;

            var bsParams = new NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER
            {
                version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER,
                size = 0, memoryHeap = 0, _padding = 0, bitstreamBuffer = IntPtr.Zero,
                reserved1 = IntPtr.Zero, reserved2 = IntPtr.Zero,
                reserved3 = new uint[226], reserved4 = new IntPtr[64],
            };
            nvenc.CreateBitstreamBuffer!(encoder, ref bsParams);
            bitstreamBuffers[i] = bsParams.bitstreamBuffer;
        }

        var sw = Stopwatch.StartNew();

        try
        {
            while (sw.Elapsed < TimeSpan.FromSeconds(DurationSeconds))
            {
                long fStart = Stopwatch.GetTimestamp();
                long t0, t1;

                // Submit N frames: Acquire → Copy → Release → Map → Encode (no Lock yet)
                var mappedInputs = new IntPtr[queueDepth];
                int submitted = 0;

                for (int q = 0; q < queueDepth; q++)
                {
                    // Acquire
                    t0 = Stopwatch.GetTimestamp();
                    var acq = dup.AcquireNextFrame(AcquireTimeoutMs, out var fi, out var dr);
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Acquire].Add(TicksToUs(t0, t1));

                    if (acq.Failure)
                    {
                        if (acq.Code == Vortice.DXGI.ResultCode.WaitTimeout) r.waitTimeouts++;
                        else r.drops++;
                        if (dr != null) dr.Dispose();
                        continue;
                    }

                    var dt = dr.QueryInterface<ID3D11Texture2D>();
                    dr.Dispose();

                    // Copy
                    t0 = Stopwatch.GetTimestamp();
                    deviceCtx.CopyResource(textures[q], dt);
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Copy].Add(TicksToUs(t0, t1));

                    // Release
                    t0 = Stopwatch.GetTimestamp();
                    dup.ReleaseFrame();
                    dt.Dispose();
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Release].Add(TicksToUs(t0, t1));

                    // Map
                    t0 = Stopwatch.GetTimestamp();
                    deviceCtx.Flush();
                    var mapParams = MakeMapParams(regResources[q]);
                    uint mapStatus = nvenc.MapInputResource!(encoder, ref mapParams);
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Map].Add(TicksToUs(t0, t1));

                    if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }
                    mappedInputs[q] = mapParams.mappedResource;

                    // Encode (no Lock yet — submit to GPU pipeline)
                    var picParams = MakePicParams(mappedInputs[q], bitstreamBuffers[q], texW, texH);
                    t0 = Stopwatch.GetTimestamp();
                    uint encStatus = nvenc.EncodePicture!(encoder, ref picParams);
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Encode].Add(TicksToUs(t0, t1));

                    if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; mappedInputs[q] = IntPtr.Zero; continue; }
                    submitted++;
                }

                // Lock → Unlock → Unmap all submitted frames (batched GPU sync)
                for (int q = 0; q < queueDepth; q++)
                {
                    if (mappedInputs[q] == IntPtr.Zero) continue;

                    // Lock
                    var lockParams = MakeLockParams(bitstreamBuffers[q]);
                    t0 = Stopwatch.GetTimestamp();
                    uint lockStatus = nvenc.LockBitstream!(encoder, ref lockParams);
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Lock].Add(TicksToUs(t0, t1));

                    if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS) { r.nvencErrors++; r.drops++; continue; }
                    r.totalBytes += (int)lockParams.bitstreamSizeInBytes;
                    r.frames++;

                    // Unlock
                    t0 = Stopwatch.GetTimestamp();
                    nvenc.UnlockBitstream!(encoder, bitstreamBuffers[q]);
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Unlock].Add(TicksToUs(t0, t1));

                    // Unmap
                    t0 = Stopwatch.GetTimestamp();
                    try { nvenc.UnmapInputResource!(encoder, mappedInputs[q]); } catch { }
                    t1 = Stopwatch.GetTimestamp();
                    r.stageLatencies[S_Unmap].Add(TicksToUs(t0, t1));
                }

                r.stageLatencies[S_Total].Add(TicksToUs(fStart, Stopwatch.GetTimestamp()));
            }
            sw.Stop();
            r.durationSec = sw.Elapsed.TotalSeconds;
            r.fps = r.frames / r.durationSec;
        }
        finally
        {
            // Cleanup queue resources
            for (int i = 0; i < queueDepth; i++)
            {
                if (regResources[i] != IntPtr.Zero) try { nvenc.UnregisterResource!(encoder, regResources[i]); } catch { }
                if (bitstreamBuffers[i] != IntPtr.Zero) try { nvenc.DestroyBitstreamBuffer!(encoder, bitstreamBuffers[i]); } catch { }
                textures[i]?.Dispose();
            }
        }
        return r;
    }

    // ═══ Helpers ═══

    private static (IDXGIOutputDuplication duplication, IntPtr encoder, NvEncFunctionTable nvenc,
                     ID3D11Texture2D encoderTexture, IntPtr registeredResource, IntPtr bitstreamBuffer)? SetupPipeline(
        ID3D11Device device, uint texW, uint texH)
    {
        // Create duplication
        IDXGIOutput? primaryOutput = null;
        int outIdx = 0;
        while (SpikeSharedContext.TargetAdapter!.EnumOutputs((uint)outIdx, out IDXGIOutput out_).Success)
        {
            if (outIdx == 0) primaryOutput = out_; else out_.Dispose();
            outIdx++;
        }
        if (primaryOutput == null) { Console.Error.WriteLine("  FAIL: No outputs."); return null; }
        var output1 = primaryOutput.QueryInterface<IDXGIOutput1>();
        IDXGIOutputDuplication dup;
        try { dup = output1.DuplicateOutput(device); }
        catch (Exception ex) { Console.Error.WriteLine($"  FAIL: DuplicateOutput: {ex.Message}"); output1.Dispose(); primaryOutput.Dispose(); return null; }
        output1.Dispose(); primaryOutput.Dispose();

        // Load NVENC
        var nvenc = new NvEncFunctionTable();
        if (!nvenc.TryLoad()) { Console.Error.WriteLine("  FAIL: NVENC load."); dup.Dispose(); return null; }

        var sessionParams = new NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
        {
            version = NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER,
            deviceType = NvEncodeAPI.NV_ENC_DEVICE_DIRECTX, device = device.NativePointer,
            reserved = IntPtr.Zero, apiVersion = NvEncodeAPI.NVENCAPI_VERSION,
            reserved1 = new uint[253], reserved2 = new IntPtr[64],
        };
        uint status = nvenc.OpenEncodeSessionEx!(ref sessionParams, out IntPtr encoder);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: OpenSession: {status}"); return null; }

        // Init encoder
        var initParams = new NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS
        {
            version = NvEncodeAPI.MakeStructVersion(5) | (1u << 31),
            encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID,
            presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID,
            encodeWidth = texW, encodeHeight = texH, darWidth = texW, darHeight = texH,
            frameRateNum = 60, frameRateDen = 1, enableEncodeAsync = 0, enablePTD = 1,
            bitFields = 0, privDataSize = 0, privData = IntPtr.Zero, encodeConfig = IntPtr.Zero,
            maxEncodeWidth = texW, maxEncodeHeight = texH,
            maxMEHintCountsPerBlockL0 = 0, maxMEHintCountsPerBlockL1 = 0,
            reserved = new uint[289], reserved2 = new IntPtr[64],
        };
        status = nvenc.InitializeEncoder!(encoder, ref initParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: Init: {status}"); return null; }

        // Create bitstream buffer
        var bsParams = new NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER
        {
            version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER, size = 0, memoryHeap = 0, _padding = 0,
            bitstreamBuffer = IntPtr.Zero, reserved1 = IntPtr.Zero, reserved2 = IntPtr.Zero,
            reserved3 = new uint[226], reserved4 = new IntPtr[64],
        };
        status = nvenc.CreateBitstreamBuffer!(encoder, ref bsParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: BS buffer: {status}"); return null; }

        // Create encoder texture
        var texDesc = new Texture2DDescription
        {
            Width = texW, Height = texH, MipLevels = 1, ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm, SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default, BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CPUAccessFlags = CpuAccessFlags.None, MiscFlags = ResourceOptionFlags.None,
        };
        var encoderTexture = device.CreateTexture2D(texDesc);

        // Register texture
        var regParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
        {
            version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
            resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
            width = texW, height = texH, pitch = 0, subResourceIndex = 0,
            resourceToRegister = encoderTexture.NativePointer, registeredResource = IntPtr.Zero,
            bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
            reserved1 = new uint[248], reserved2 = new IntPtr[62],
        };
        status = nvenc.RegisterResource!(encoder, ref regParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: Register: {status}"); return null; }

        Console.WriteLine("  PASS: Setup complete.");
        return (dup, encoder, nvenc, encoderTexture, regParams.registeredResource, bsParams.bitstreamBuffer);
    }

    private static NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE MakeMapParams(IntPtr registeredResource) => new()
    {
        version = NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER, subResourceIndex = 0,
        inputResource = registeredResource, registeredResource = registeredResource,
        mappedResource = IntPtr.Zero, mappedBufferFmt = 0,
        reserved1 = new uint[251], reserved2 = new IntPtr[63],
    };

    private static NvEncodeAPI.NV_ENC_PIC_PARAMS MakePicParams(IntPtr mappedInput, IntPtr bitstreamBuffer, uint texW, uint texH) => new()
    {
        version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER, inputWidth = texW, inputHeight = texH, inputPitch = 0,
        encodePicFlags = 0, frameIdx = 0, inputTimeStamp = 0, inputDuration = 0,
        inputBuffer = mappedInput, outputBitstream = bitstreamBuffer, completionEvent = IntPtr.Zero,
        bufferFmt = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB, pictureStruct = 1, pictureType = 0, _padding1 = 0,
        codecPicParams = new byte[1536], meHintCountsPerBlock = new byte[32], meExternalHints = IntPtr.Zero,
        reserved1 = new uint[6], reserved2 = new IntPtr[2], qpDeltaMap = IntPtr.Zero, qpDeltaMapSize = 0,
        reservedBitFields = 0, meHintRefPicDist = new ushort[2], _padding2 = 0, alphaBuffer = IntPtr.Zero,
        reserved3 = new uint[286], reserved4 = new IntPtr[59],
    };

    private static NvEncodeAPI.NV_ENC_LOCK_BITSTREAM MakeLockParams(IntPtr bitstreamBuffer) => new()
    {
        version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER, bitfields = 0, outputBitstream = bitstreamBuffer,
        sliceOffsets = IntPtr.Zero, frameIdx = 0, hwEncodeStatus = 0, numSlices = 0, bitstreamSizeInBytes = 0,
        outputTimeStamp = 0, outputDuration = 0, bitstreamBufferPtr = IntPtr.Zero, pictureType = 0,
        pictureStruct = 0, frameAvgQP = 0, frameSatd = 0, ltrFrameIdx = 0, ltrFrameBitmap = 0,
        reserved = new uint[13], intraMBCount = 0, interMBCount = 0, averageMVX = 0, averageMVY = 0,
        alphaLayerSizeInBytes = 0, reserved1 = new uint[218], reserved2 = new IntPtr[64],
    };

    private static List<long>[] NewStageLists()
    {
        var arr = new List<long>[NUM_STAGES];
        for (int i = 0; i < NUM_STAGES; i++) arr[i] = new List<long>(8 * 1024);
        return arr;
    }

    private static long TicksToUs(long t0, long t1) => (long)((t1 - t0) * 1_000_000.0 / Stopwatch.Frequency);

    private static double Percentile(List<long> data, double p)
    {
        if (data.Count == 0) return 0;
        var s = new List<long>(data); s.Sort();
        if (s.Count == 1) return s[0];
        double rank = p * (s.Count - 1);
        int lo = (int)Math.Floor(rank); int hi = (int)Math.Ceiling(rank);
        if (lo == hi) return s[lo];
        return s[lo] + (rank - lo) * (s[hi] - s[lo]);
    }

    private static double GetStageAvg(ExpResult r, int stage) =>
        r.stageLatencies[stage].Count > 0 ? r.stageLatencies[stage].Average() : 0;

    private static void PrintExperimentResults(string label, ExpResult r)
    {
        Console.WriteLine($"  Duration:    {r.durationSec:F3}s");
        Console.WriteLine($"  Frames:      {r.frames}");
        Console.WriteLine($"  FPS:         {r.fps:F2}");
        Console.WriteLine($"  Drops:       {r.drops}");
        Console.WriteLine($"  NVENC errors: {r.nvencErrors}");
        Console.WriteLine($"  Wait timeouts: {r.waitTimeouts}");
        Console.WriteLine($"  Total bytes: {r.totalBytes}");
        Console.WriteLine();
        Console.WriteLine($"  {'Stage':<25} {'min':>8} {'avg':>8} {'P50':>8} {'P95':>8} {'P99':>8} {'max':>8}");
        Console.WriteLine($"  {new string('-', 25)} {new string('-', 8)} {new string('-', 8)} {new string('-', 8)} {new string('-', 8)} {new string('-', 8)} {new string('-', 8)}");
        for (int s = 0; s < NUM_STAGES; s++)
        {
            var lat = r.stageLatencies[s];
            if (lat.Count == 0) continue;
            Console.WriteLine($"  {StageNames[s],-25} {lat.Min(),8} {lat.Average(),8:F1} {Percentile(lat, 0.50),8:F1} {Percentile(lat, 0.95),8:F1} {Percentile(lat, 0.99),8:F1} {lat.Max(),8}");
        }
        Console.WriteLine();
    }

    private static void PrintExpRow(string label, ExpResult r)
    {
        Console.WriteLine($"    {label,-20} {r.fps,8:F2} {r.frames,8} {r.drops,6} {r.nvencErrors,7} {GetStageAvg(r, S_Lock),10:F1} {GetStageAvg(r, S_Encode),10:F1} {GetStageAvg(r, S_Acquire),10:F1}");
    }

    private static void PrintQueueRow(int depth, ExpResult r)
    {
        var lat = r.stageLatencies[S_Lock];
        Console.WriteLine($"    {depth,6} {r.fps,8:F2} {lat.Average(),10:F1} {Percentile(lat, 0.95),10:F1} {Percentile(lat, 0.99),10:F1} {GetStageAvg(r, S_Encode),10:F1} {GetStageAvg(r, S_Acquire),10:F1} {r.drops,6}");
    }
}
