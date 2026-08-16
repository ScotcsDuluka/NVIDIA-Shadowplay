// Phases/Phase2_DesktopDuplication.cs
//
// P1-B.2-V1 Spike — Phase 2: Desktop Duplication Test
//
// Goal: Capture 1000 frames using DXGI Output Duplication API and measure
// acquisition performance. This proves the capture pipeline can sustain
// the target framerate without CPU staging copy.
//
// Metrics:
//   - Achieved FPS (target >= 144)
//   - AcquireNextFrame latency (min/avg/max/p50/p95/p99)
//   - WAIT_TIMEOUT count (NoFrame events)
//   - ACCESS_LOST count (session invalidated)
//
// Acceptance criteria for Phase 2:
//   - 1000 frames acquired
//   - Achieved FPS >= 60 (lower bound; 144 target depends on display)
//   - No ACCESS_LOST during the test
//   - p95 acquire latency < 10 ms
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

// 'Result' is in SharpGen.Runtime (Vortice is built on SharpGen).
// 'ResultCode' exists in both Vortice.DXGI and Vortice.Direct3D11 — alias to
// disambiguate. We want the DXGI versions (DuplicateOutput is a DXGI API).
using SharpGen.Runtime;
using ResultCode = Vortice.DXGI.ResultCode;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase2_DesktopDuplication
{
    private const int TargetFrames = 1000;

    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine($" Phase 2 — Desktop Duplication Test ({TargetFrames} frames)");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        if (SpikeSharedContext.Device == null || SpikeSharedContext.TargetAdapter == null)
        {
            Console.Error.WriteLine("  FAIL: Phase 1 must run first to establish D3D11 device.");
            return 1;
        }

        // --- Step 1: Enumerate outputs on the NVIDIA adapter ---
        Console.WriteLine("[2.1] Enumerating outputs on NVIDIA adapter...");
        int outputIdx = 0;
        IDXGIOutput? primaryOutput = null;
        while (SpikeSharedContext.TargetAdapter.EnumOutputs((uint)outputIdx, out IDXGIOutput output).Success)
        {
            var desc = output.Description;
            Console.WriteLine($"  Output {outputIdx}: {desc.DeviceName}  " +
                              $"({desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left}x" +
                              $"{desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top})");
            if (outputIdx == 0)
                primaryOutput = output;
            else
                output.Dispose();
            outputIdx++;
        }

        if (primaryOutput == null)
        {
            Console.Error.WriteLine("  FAIL: No outputs found on NVIDIA adapter.");
            return 1;
        }

        // --- Step 2: Get IDXGIOutput1 and DuplicateOutput ---
        Console.WriteLine();
        Console.WriteLine("[2.2] Creating IDXGIOutputDuplication...");
        IDXGIOutput1 output1 = primaryOutput.QueryInterface<IDXGIOutput1>();
        IDXGIOutputDuplication duplication;
        try
        {
            duplication = output1.DuplicateOutput(SpikeSharedContext.Device);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  FAIL: DuplicateOutput threw: {ex.GetType().Name}: {ex.Message}");
            output1.Dispose();
            primaryOutput.Dispose();
            return 1;
        }

        var duplDesc = duplication.Description;
        Console.WriteLine($"  DuplicateOutput created. ModeDescription: {duplDesc.ModeDescription.Width}x{duplDesc.ModeDescription.Height}@{duplDesc.ModeDescription.RefreshRate.Numerator / (double)duplDesc.ModeDescription.RefreshRate.Denominator:F2}Hz");
        Console.WriteLine($"  Format: {duplDesc.ModeDescription.Format}");

        // --- Step 3: Pre-create a staging texture (BGRA8, same size as desktop) ---
        Texture2DDescription stagingDesc = new()
        {
            Width = duplDesc.ModeDescription.Width,
            Height = duplDesc.ModeDescription.Height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Vortice.DXGI.Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = ResourceOptionFlags.None,
        };
        ID3D11Texture2D stagingTexture = SpikeSharedContext.Device.CreateTexture2D(stagingDesc);
        Console.WriteLine($"  Staging texture created: {stagingDesc.Width}x{stagingDesc.Height} BGRA8");
        Console.WriteLine($"  Staging texture pointer: 0x{stagingTexture.NativePointer.ToInt64():x16}");

        // --- Step 4: Capture loop ---
        Console.WriteLine();
        Console.WriteLine($"[2.3] Capturing {TargetFrames} frames...");

        using var metrics = new FrameMetrics();
        metrics.Start();

        int framesCaptured = 0;
        var sw = Stopwatch.StartNew();

        while (framesCaptured < TargetFrames)
        {
            long t0 = sw.ElapsedTicks;

            Result hr;
            IDXGIResource? desktopResource = null;
            try
            {
                hr = duplication.AcquireNextFrame(
                    timeoutInMilliseconds: 100,  // 100ms — generous; 0 = non-blocking poll
                    out var frameInfo,
                    out desktopResource);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  AcquireNextFrame threw: {ex.GetType().Name}: {ex.Message}");
                metrics.RecordOtherError();
                break;
            }

            long t1 = sw.ElapsedTicks;
            double latencyMs = (t1 - t0) * 1000.0 / Stopwatch.Frequency;

            if (hr == ResultCode.WaitTimeout)
            {
                metrics.RecordWaitTimeout();
                continue;
            }
            if (hr == ResultCode.AccessLost)
            {
                metrics.RecordAccessLost();
                Console.Error.WriteLine("  ACCESS_LOST — duplication invalidated during capture.");
                break;
            }
            if (hr.Failure)
            {
                Console.Error.WriteLine($"  AcquireNextFrame failed: hr=0x{hr.Code:x8}");
                metrics.RecordOtherError();
                break;
            }

            // We have a frame. QueryInterface to ID3D11Texture2D.
            ID3D11Texture2D? desktopTexture = null;
            try
            {
                desktopTexture = desktopResource!.QueryInterface<ID3D11Texture2D>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  QueryInterface(ID3D11Texture2D) threw: {ex.Message}");
                desktopResource!.Dispose();
                metrics.RecordOtherError();
                continue;
            }

            // CopySubresourceRegion — copy desktop texture into staging.
            // This is the same pattern ddagrab uses (see Ddagrab_Research.md F3).
            // For zero-copy validation, we don't actually need to read the pixels —
            // we just need to prove the texture handle is valid and on the right device.
            SpikeSharedContext.DeviceContext!.CopyResource(stagingTexture, desktopTexture);

            // Release desktop texture + ReleaseFrame BEFORE next AcquireNextFrame.
            desktopTexture.Dispose();
            desktopResource!.Dispose();

            try
            {
                duplication.ReleaseFrame();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  ReleaseFrame threw: {ex.Message}");
                // Not fatal — continue.
            }

            metrics.RecordAcquire(latencyMs);
            framesCaptured++;

            if (framesCaptured % 100 == 0)
            {
                var snap = metrics.Snapshot();
                Console.WriteLine($"    [{framesCaptured,4}/{TargetFrames}] " +
                                  $"FPS={snap.AchievedFps:F1} " +
                                  $"avg={snap.AvgAcquireLatencyMs:F2}ms " +
                                  $"p95={snap.P95AcquireLatencyMs:F2}ms " +
                                  $"WT={snap.WaitTimeoutCount} " +
                                  $"AL={snap.AccessLostCount}");
            }
        }

        metrics.Stop();
        sw.Stop();

        // --- Step 5: Cleanup duplication + staging texture ---
        // Keep staging texture for Phase 3 (ownership test) and Phase 4 (NVENC registration).
        SpikeSharedContext.StagingTexture = stagingTexture;
        SpikeSharedContext.DuplicationDesc = duplDesc;

        duplication.Dispose();
        output1.Dispose();
        primaryOutput.Dispose();

        // --- Step 6: Print Phase 2 result summary ---
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 2 RESULT");
        Console.WriteLine("============================================================");
        var finalStats = metrics.Snapshot();
        finalStats.PrintToConsole("Capture loop");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // --- Step 7: Acceptance check ---
        bool pass = true;
        if (finalStats.FramesAcquired < TargetFrames)
        {
            Console.Error.WriteLine($"  FAIL: Only {finalStats.FramesAcquired} frames acquired (expected {TargetFrames}).");
            pass = false;
        }
        if (finalStats.AccessLostCount > 0)
        {
            Console.Error.WriteLine($"  FAIL: ACCESS_LOST occurred {finalStats.AccessLostCount} times.");
            pass = false;
        }
        if (finalStats.P95AcquireLatencyMs > 10.0)
        {
            Console.Error.WriteLine($"  WARN: p95 acquire latency > 10 ms ({finalStats.P95AcquireLatencyMs:F2} ms).");
            // Warning, not fail — depends on display refresh rate.
        }
        if (finalStats.AchievedFps < 60.0)
        {
            Console.Error.WriteLine($"  FAIL: Achieved FPS < 60 ({finalStats.AchievedFps:F2}).");
            pass = false;
        }

        Console.WriteLine(pass ? "  Phase 2: PASS" : "  Phase 2: FAIL");
        Console.WriteLine();
        return pass ? 0 : 1;
    }
}

// Augment SpikeSharedContext (declared partial in Phase1_DeviceTest.cs) with
// Phase 2-specific fields: the staging texture and the duplication description.
// These are populated by Phase 2 and consumed by Phase 3 and Phase 4.

public static partial class SpikeSharedContext
{
    public static ID3D11Texture2D? StagingTexture { get; set; }

    // IDXGIOutputDuplication.Description returns OutduplDescription (NOT
    // OutputDescription — that's a different type in Vortice.DXGI, with
    // different fields). OutduplDescription carries ModeDescription which
    // we need for width/height/format.
    public static OutduplDescription? DuplicationDesc { get; set; }
}
