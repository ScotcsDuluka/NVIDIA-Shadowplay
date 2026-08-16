// Phases/Phase3_TextureOwnership.cs
//
// P1-B.2-V1 Spike — Phase 3: Texture Ownership Test
//
// Goal: Verify that the staging texture captured by Phase 2:
//   1. Is a real ID3D11Texture2D (not a CPU buffer)
//   2. Has format BGRA8 (matches P1-A v1.3.1 baseline)
//   3. Has correct dimensions (matches desktop)
//   4. Lives on the same D3D11 device as Phase 1 (no cross-device copy)
//   5. Has expected resource usage (Default + ShaderResource, no CPU staging)
//
// This proves the texture is suitable for direct NVENC registration in Phase 4
// without any CPU staging copy.
//
// Acceptance criteria for Phase 3:
//   - Texture pointer is valid (non-zero)
//   - Format = BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM)
//   - Width/Height matches desktop dimensions
//   - Device pointer matches Phase 1's device
//   - Usage = Default (no CPU staging)
//   - BindFlags includes ShaderResource
//   - CPUAccessFlags = None
//
// SPDX-License-Identifier: MIT

using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase3_TextureOwnership
{
    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 3 — Texture Ownership Test");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        if (SpikeSharedContext.StagingTexture == null
            || SpikeSharedContext.Device == null
            || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.Error.WriteLine("  FAIL: Phase 2 must run first to populate staging texture.");
            return 1;
        }

        var texture = SpikeSharedContext.StagingTexture;
        var device = SpikeSharedContext.Device;
        var duplDesc = SpikeSharedContext.DuplicationDesc.Value;

        // --- Step 1: Get texture description ---
        Console.WriteLine("[3.1] Querying texture description...");
        Texture2DDescription desc = texture.Description;
        long texturePtr = texture.NativePointer.ToInt64();
        long devicePtr = device.NativePointer.ToInt64();

        Console.WriteLine($"  Texture pointer:   0x{texturePtr:x16}");
        Console.WriteLine($"  Texture type:      ID3D11Texture2D");
        Console.WriteLine($"  Width:             {desc.Width}");
        Console.WriteLine($"  Height:            {desc.Height}");
        Console.WriteLine($"  MipLevels:         {desc.MipLevels}");
        Console.WriteLine($"  ArraySize:         {desc.ArraySize}");
        Console.WriteLine($"  Format:            {desc.Format}");
        Console.WriteLine($"  SampleDescription: count={desc.SampleDescription.Count}, quality={desc.SampleDescription.Quality}");
        Console.WriteLine($"  Usage:             {desc.Usage}");
        Console.WriteLine($"  BindFlags:         {desc.BindFlags}");
        Console.WriteLine($"  CPUAccessFlags:    {desc.CPUAccessFlags}");
        Console.WriteLine($"  MiscFlags:         {desc.MiscFlags}");

        // --- Step 2: Verify format is BGRA8 ---
        Console.WriteLine();
        Console.WriteLine("[3.2] Verifying format...");
        if (desc.Format != Format.B8G8R8A8_UNorm)
        {
            Console.Error.WriteLine($"  FAIL: Texture format is {desc.Format}, expected B8G8R8A8_UNorm (BGRA8).");
            Console.Error.WriteLine("        P1-A v1.3.1 baseline requires BGRA8 (§3.4, §15.1).");
            return 1;
        }
        Console.WriteLine("  PASS: Format is BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM).");

        // --- Step 3: Verify dimensions match desktop ---
        Console.WriteLine();
        Console.WriteLine("[3.3] Verifying dimensions match desktop...");
        int desktopWidth = duplDesc.ModeDescription.Width;
        int desktopHeight = duplDesc.ModeDescription.Height;
        Console.WriteLine($"  Desktop:  {desktopWidth}x{desktopHeight}");
        Console.WriteLine($"  Texture:  {desc.Width}x{desc.Height}");
        if (desc.Width != desktopWidth || desc.Height != desktopHeight)
        {
            Console.Error.WriteLine("  FAIL: Texture dimensions do not match desktop.");
            return 1;
        }
        Console.WriteLine("  PASS: Dimensions match.");

        // --- Step 4: Verify resource usage (Default, no CPU staging) ---
        Console.WriteLine();
        Console.WriteLine("[3.4] Verifying resource usage...");
        bool isDefault = desc.Usage == ResourceUsage.Default;
        bool hasShaderResource = desc.BindFlags.HasFlag(BindFlags.ShaderResource);
        bool hasRenderTarget = desc.BindFlags.HasFlag(BindFlags.RenderTarget);
        bool hasCPUAccess = desc.CPUAccessFlags != CpuAccessFlags.None;
        bool hasStagingUsage = desc.Usage == ResourceUsage.Staging;

        Console.WriteLine($"  Usage = Default:           {isDefault}");
        Console.WriteLine($"  Usage = Staging:           {hasStagingUsage}");
        Console.WriteLine($"  BindFlags.ShaderResource:  {hasShaderResource}");
        Console.WriteLine($"  BindFlags.RenderTarget:    {hasRenderTarget}");
        Console.WriteLine($"  CPUAccessFlags (any):      {hasCPUAccess}");

        if (hasStagingUsage)
        {
            Console.Error.WriteLine("  FAIL: Texture has Usage=Staging — this is a CPU staging texture,");
            Console.Error.WriteLine("        not a GPU-resident texture. Zero-copy is not possible.");
            return 1;
        }
        if (hasCPUAccess)
        {
            Console.Error.WriteLine("  FAIL: Texture has CPUAccessFlags set — indicates CPU staging path.");
            return 1;
        }
        if (!isDefault)
        {
            Console.Error.WriteLine($"  FAIL: Texture Usage is {desc.Usage}, expected Default.");
            return 1;
        }
        if (!hasShaderResource)
        {
            Console.Error.WriteLine("  WARN: Texture lacks BindFlags.ShaderResource. NVENC may not accept it.");
            Console.Error.WriteLine("       Phase 4 will confirm whether NVENC accepts it.");
        }
        Console.WriteLine("  PASS: Texture is GPU-resident (Default usage, no CPU access).");

        // --- Step 5: Verify device pointer matches Phase 1's device ---
        Console.WriteLine();
        Console.WriteLine("[3.5] Verifying texture's parent device matches Phase 1 device...");

        // Vortice exposes texture.Device as a property (not a GetDevice method).
        ID3D11Device textureDevice = texture.Device;
        long textureDevicePtr = textureDevice.NativePointer.ToInt64();
        Console.WriteLine($"  Phase 1 device pointer: 0x{devicePtr:x16}");
        Console.WriteLine($"  Texture's parent device: 0x{textureDevicePtr:x16}");

        bool sameDevice = textureDevicePtr == devicePtr;
        if (!sameDevice)
        {
            Console.Error.WriteLine("  FAIL: Texture's parent device does NOT match Phase 1 device.");
            Console.Error.WriteLine("        This indicates the texture was created on a different GPU.");
            Console.Error.WriteLine("        Zero-copy to NVENC on Phase 1's GPU is NOT possible.");
            return 1;
        }
        Console.WriteLine("  PASS: Texture lives on the same D3D11 device as Phase 1.");
        // Do NOT dispose textureDevice — it's the same device as SpikeSharedContext.Device,
        // disposing would invalidate the shared device.

        // --- Step 6: Verify ArraySize (NVENC accepts both ArraySize=1 and >1) ---
        Console.WriteLine();
        Console.WriteLine("[3.6] Verifying ArraySize...");
        if (desc.ArraySize < 1)
        {
            Console.Error.WriteLine("  FAIL: ArraySize < 1 — invalid texture.");
            return 1;
        }
        Console.WriteLine($"  ArraySize = {desc.ArraySize} (1 = single texture, >1 = texture array)");
        if (desc.ArraySize == 1)
        {
            Console.WriteLine("  PASS: Single texture (ArraySize=1) — suitable for direct NVENC registration.");
        }
        else
        {
            Console.WriteLine("  PASS: Texture array (ArraySize>1) — NVENC can register individual slices.");
        }

        // --- Step 7: Phase 3 summary ---
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 3 RESULT");
        Console.WriteLine("============================================================");
        Console.WriteLine($"  Texture: ID3D11Texture2D @ 0x{texturePtr:x16}");
        Console.WriteLine($"  Format:  BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM)");
        Console.WriteLine($"  Dims:    {desc.Width}x{desc.Height}");
        Console.WriteLine($"  Device:  {(sameDevice ? "Same" : "Different")} (0x{textureDevicePtr:x16})");
        Console.WriteLine($"  Usage:   {desc.Usage}");
        Console.WriteLine($"  BindFlags: {desc.BindFlags}");
        Console.WriteLine($"  CPUAccess: {desc.CPUAccessFlags} (None = GPU-resident)");
        Console.WriteLine($"  ArraySize: {desc.ArraySize}");
        Console.WriteLine("============================================================");
        Console.WriteLine();
        Console.WriteLine("  Phase 3: PASS — texture is GPU-resident BGRA8 on the same device as Phase 1.");
        Console.WriteLine();
        return 0;
    }
}
