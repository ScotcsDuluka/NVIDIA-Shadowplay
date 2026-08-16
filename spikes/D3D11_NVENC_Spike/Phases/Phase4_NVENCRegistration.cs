// Phases/Phase4_NVENCRegistration.cs
//
// P1-B.2-V1 Spike — Phase 4: NVENC Registration
//
// Goal: Prove that the D3D11 texture acquired in Phase 2 can be registered
// with NVENC as an input resource — this is the critical zero-copy path.
//
// Steps:
//   1. Load NVENC function table (NvEncodeAPICreateInstance)
//   2. Open encode session on Phase 1's D3D11 device (NvEncOpenEncodeSessionEx)
//   3. Verify H.264 codec is supported
//   4. Verify ARGB input format is supported (BGRA8 ↔ ARGB in NVENC)
//   5. (Optionally) initialize encoder with H.264 defaults
//   6. Register the staging texture (NvEncRegisterResource)
//   7. Unregister and tear down
//
// Acceptance criteria for Phase 4 (V1 BLOCKER):
//   - nvEncodeAPI.dll loads successfully
//   - NvEncodeAPICreateInstance returns NV_ENC_SUCCESS
//   - NvEncOpenEncodeSessionEx with NV_ENC_DEVICE_DIRECTX succeeds
//   - H.264 codec GUID is in the supported list
//   - ARGB (BGRA8) is in the supported input formats
//   - NvEncRegisterResource returns NV_ENC_SUCCESS
//
// SPDX-License-Identifier: MIT

using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase4_NVENCRegistration
{
    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 4 — NVENC Registration Spike");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        if (SpikeSharedContext.StagingTexture == null || SpikeSharedContext.Device == null)
        {
            Console.Error.WriteLine("  FAIL: Phase 1-3 must run first.");
            return 1;
        }

        // --- Step 1: Load NVENC function table ---
        Console.WriteLine("[4.1] Loading NVENC function table...");
        using var nvenc = new NvEncFunctionTable();
        if (!nvenc.TryLoad())
        {
            Console.Error.WriteLine("  FAIL: Could not load NVENC function table.");
            return 1;
        }
        Console.WriteLine("  PASS: NVENC function table loaded.");

        // --- Step 2: Open encode session on the D3D11 device ---
        Console.WriteLine();
        Console.WriteLine("[4.2] Opening NVENC encode session on D3D11 device...");

        if (nvenc.OpenEncodeSessionEx == null)
        {
            Console.Error.WriteLine("  FAIL: OpenEncodeSessionEx delegate is null.");
            return 1;
        }

        var sessionParams = new NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
        {
            version = NvEncodeAPI.MakeStructVersion<NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS>(),
            deviceType = NvEncodeAPI.NV_ENC_DEVICE_DIRECTX,
            device = SpikeSharedContext.Device.NativePointer,
            reserved = IntPtr.Zero,
            @event = IntPtr.Zero,        // 'event' is C# keyword — escape with @
            inputParams = IntPtr.Zero,
            apiVersion = NvEncodeAPI.NVENCAPI_VERSION,
        };

        int status = nvenc.OpenEncodeSessionEx(ref sessionParams, out IntPtr encoder);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncOpenEncodeSessionEx returned {status} " +
                                    $"({NvEncodeAPI.NvencStatusToString(status)}).");
            Console.Error.WriteLine("        Likely causes:");
            Console.Error.WriteLine("          - NVENC not supported on this GPU (needs NVIDIA GTX 600+ / Quadro K500+)");
            Console.Error.WriteLine("          - Driver too old (update to latest NVIDIA driver)");
            Console.Error.WriteLine("          - nvEncodeAPI.dll version mismatch");
            Console.Error.WriteLine("          - deviceType mismatch (NV_ENC_DEVICE_DIRECTX requires D3D11 device)");
            return 1;
        }
        Console.WriteLine($"  PASS: Encode session opened. Encoder handle: 0x{encoder.ToInt64():x16}");

        // --- Step 3: Verify H.264 codec is supported ---
        Console.WriteLine();
        Console.WriteLine("[4.3] Enumerating supported codecs...");

        if (nvenc.GetEncodeGUIDCount == null || nvenc.GetEncodeGUIDs == null)
        {
            Console.Error.WriteLine("  FAIL: GetEncodeGUIDCount/GetEncodeGUIDs delegate is null.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        status = nvenc.GetEncodeGUIDCount(encoder, out int codecCount);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncGetEncodeGUIDCount returned {status}.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }
        Console.WriteLine($"  NVENC reports {codecCount} supported codecs.");

        var codecs = new Guid[codecCount];
        status = nvenc.GetEncodeGUIDs(encoder, codecs, codecs.Length, out int actualCodecCount);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncGetEncodeGUIDs returned {status}.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        bool h264Supported = false;
        bool hevcSupported = false;
        bool av1Supported = false;
        Console.WriteLine("  Supported codecs:");
        foreach (var g in codecs.Take(actualCodecCount))
        {
            string name = "Unknown";
            if (g == NvEncodeAPI.NV_ENC_CODEC_H264_GUID) { name = "H.264"; h264Supported = true; }
            else if (g == NvEncodeAPI.NV_ENC_CODEC_HEVC_GUID) { name = "HEVC"; hevcSupported = true; }
            else if (g == NvEncodeAPI.NV_ENC_CODEC_AV1_GUID) { name = "AV1"; av1Supported = true; }
            Console.WriteLine($"    {name}: {g}");
        }

        if (!h264Supported)
        {
            Console.Error.WriteLine("  FAIL: H.264 codec is not supported by this NVENC.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }
        Console.WriteLine("  PASS: H.264 codec is supported.");

        // --- Step 4: Verify ARGB (BGRA8) input format is supported ---
        Console.WriteLine();
        Console.WriteLine("[4.4] Verifying ARGB (BGRA8) input format support for H.264...");

        if (nvenc.GetInputFormatCount == null || nvenc.GetInputFormats == null)
        {
            Console.Error.WriteLine("  FAIL: Input format delegates are null.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        status = nvenc.GetInputFormatCount(encoder, NvEncodeAPI.NV_ENC_CODEC_H264_GUID, out int formatCount);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncGetInputFormatCount returned {status}.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }
        Console.WriteLine($"  H.264 supports {formatCount} input formats.");

        var formats = new int[formatCount];
        status = nvenc.GetInputFormats(encoder, NvEncodeAPI.NV_ENC_CODEC_H264_GUID, formats, formats.Length, out int actualFormatCount);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncGetInputFormats returned {status}.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        bool argbSupported = false;
        Console.WriteLine("  H.264 input formats:");
        for (int i = 0; i < actualFormatCount; i++)
        {
            int fmt = formats[i];
            string name = fmt switch
            {
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_NV12 => "NV12",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_YV12 => "YV12",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_IYUV => "IYUV",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_YUV444 => "YUV444",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB => "ARGB (BGRA8)",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB10 => "ARGB10",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_AYUV => "AYUV",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ABGR => "ABGR",
                NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ABGR10 => "ABGR10",
                _ => $"Unknown(0x{fmt:x8})"
            };
            Console.WriteLine($"    [{i}] {name}");
            if (fmt == NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB)
                argbSupported = true;
        }

        if (!argbSupported)
        {
            Console.Error.WriteLine("  FAIL: ARGB (BGRA8) input format is not supported by H.264 on this NVENC.");
            Console.Error.WriteLine("        Phase 1-A v1.3.1 baseline requires BGRA8 — would need conversion (CPU/GPU cost).");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }
        Console.WriteLine("  PASS: ARGB (BGRA8) is supported — zero-copy path possible.");

        // --- Step 5: Register the staging texture ---
        // This is the CRITICAL V1 spike step. If NvEncRegisterResource succeeds,
        // zero-copy from D3D11 capture to NVENC is PROVEN.
        Console.WriteLine();
        Console.WriteLine("[4.5] Registering staging texture with NVENC...");

        if (nvenc.RegisterResource == null)
        {
            Console.Error.WriteLine("  FAIL: RegisterResource delegate is null.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        var registerParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
        {
            version = NvEncodeAPI.MakeStructVersion<NvEncodeAPI.NV_ENC_REGISTER_RESOURCE>(),
            resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
            // ModeDescription.Width/Height are uint in Vortice; our struct uses int.
            width = (int)SpikeSharedContext.DuplicationDesc!.Value.ModeDescription.Width,
            height = (int)SpikeSharedContext.DuplicationDesc!.Value.ModeDescription.Height,
            pitch = 0,  // 0 for D3D11 textures (NVENC queries the texture itself)
            resourceToRegister = SpikeSharedContext.StagingTexture!.NativePointer,
            registeredResource = IntPtr.Zero,  // OUT — populated by NVENC
            bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
            bufferUsage = 0,
            reserved2438 = new uint[243],      // initialize reserved padding to zeros
            p2PDeviceHandle = IntPtr.Zero,
        };

        status = nvenc.RegisterResource(encoder, ref registerParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncRegisterResource returned {status} " +
                                    $"({NvEncodeAPI.NvencStatusToString(status)}).");
            Console.Error.WriteLine("        Possible causes:");
            Console.Error.WriteLine("          - Texture dimensions out of NVENC's supported range");
            Console.Error.WriteLine("          - Texture bind flags incompatible (try adding RenderTarget)");
            Console.Error.WriteLine("          - Texture not on the same D3D11 device as the encode session");
            Console.Error.WriteLine("          - Driver version too old");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        IntPtr registeredHandle = registerParams.registeredResource;
        Console.WriteLine($"  PASS: Texture registered with NVENC.");
        Console.WriteLine($"         Registered handle: 0x{registeredHandle.ToInt64():x16}");
        Console.WriteLine($"         Width:  {registerParams.width}");
        Console.WriteLine($"         Height: {registerParams.height}");
        Console.WriteLine($"         Format: ARGB (BGRA8)");
        Console.WriteLine($"         Resource type: DirectX (D3D11Texture2D)");

        // --- Step 6: Unmap + cleanup ---
        // NVENC does NOT have an "UnregisterResource" function. To release a
        // registered resource, call nvEncUnmapInputResource with the
        // registeredResource handle returned by nvEncRegisterResource.
        Console.WriteLine();
        Console.WriteLine("[4.6] Unmapping resource and destroying encoder...");

        if (nvenc.UnmapInputResource != null)
        {
            int unmapStatus = nvenc.UnmapInputResource(encoder, registeredHandle);
            if (unmapStatus == NvEncodeAPI.NV_ENC_SUCCESS)
                Console.WriteLine("  PASS: Resource unmapped.");
            else
                Console.Error.WriteLine($"  WARN: UnmapInputResource returned {unmapStatus} — continuing.");
        }

        if (nvenc.DestroyEncoder != null)
        {
            int destroyStatus = nvenc.DestroyEncoder(encoder);
            if (destroyStatus == NvEncodeAPI.NV_ENC_SUCCESS)
                Console.WriteLine("  PASS: Encoder destroyed.");
            else
                Console.Error.WriteLine($"  WARN: DestroyEncoder returned {destroyStatus}.");
        }

        // --- Step 7: Phase 4 summary ---
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 4 RESULT");
        Console.WriteLine("============================================================");
        Console.WriteLine($"  NVENC loaded:          YES");
        Console.WriteLine($"  Encode session:        OPENED on D3D11 device");
        Console.WriteLine($"  H.264 codec:           SUPPORTED");
        Console.WriteLine($"  ARGB (BGRA8) format:   SUPPORTED");
        Console.WriteLine($"  Texture registered:    YES (NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX)");
        Console.WriteLine($"  GPU:                   {SpikeSharedContext.Gpu?.Description}");
        Console.WriteLine($"  Format:                ARGB (BGRA8)");
        Console.WriteLine("============================================================");
        Console.WriteLine();
        Console.WriteLine("  V1 BLOCKER STATUS: ✅ RESOLVED — zero-copy path PROVEN");
        Console.WriteLine();
        return 0;
    }
}
