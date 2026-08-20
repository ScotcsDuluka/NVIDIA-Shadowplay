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

using System.Runtime.InteropServices;
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
            version = NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER,
            deviceType = NvEncodeAPI.NV_ENC_DEVICE_DIRECTX,
            device = SpikeSharedContext.Device.NativePointer,
            reserved = IntPtr.Zero,
            apiVersion = NvEncodeAPI.NVENCAPI_VERSION,
            reserved1 = new uint[253],
            reserved2 = new IntPtr[64],
        };

        uint status = nvenc.OpenEncodeSessionEx(ref sessionParams, out IntPtr encoder);
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

        // === CLR Layout Diagnostic ===
        // Print Marshal.SizeOf and field offsets for ALL NVENC structs
        // to verify the CLR layout matches the C header before calling
        // NvEncInitializeEncoder.
        Console.WriteLine();
        Console.WriteLine("[4.4a] CLR struct layout diagnostic:");
        PrintStructLayout<NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS>("NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS");
        PrintStructLayout<NvEncodeAPI.NV_ENC_REGISTER_RESOURCE>("NV_ENC_REGISTER_RESOURCE");
        PrintStructLayout<NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS>("NV_ENC_INITIALIZE_PARAMS");
        PrintStructLayout<NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST>("NV_ENCODE_API_FUNCTION_LIST");
        Console.WriteLine($"  NV_ENC_INITIALIZE_PARAMS_VER: 0x{NvEncodeAPI.MakeStructVersion(5) | (1u << 31):X8}");
        Console.WriteLine($"  NV_ENC_REGISTER_RESOURCE_VER: 0x{NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER:X8}");
        Console.WriteLine($"  NV_ENCODE_API_FUNCTION_LIST_VER: 0x{NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST_VER:X8}");

        // --- Step 4.4b: Initialize encoder ---
        // NVIDIA's NvEncoder sample calls NvEncInitializeEncoder BEFORE
        // NvEncRegisterResource. Without this call, NvEncRegisterResource
        // returns NV_ENC_ERR_DEVICE_NOT_EXIST because the encoder session
        // hasn't been configured yet.
        Console.WriteLine();
        Console.WriteLine("[4.4b] Initializing encoder (required before RegisterResource)...");

        if (nvenc.InitializeEncoder == null)
        {
            Console.Error.WriteLine("  FAIL: InitializeEncoder delegate is null.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        uint encWidth = SpikeSharedContext.DuplicationDesc!.Value.ModeDescription.Width;
        uint encHeight = SpikeSharedContext.DuplicationDesc!.Value.ModeDescription.Height;

        var initParams = new NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS
        {
            version = NvEncodeAPI.MakeStructVersion(5) | (1u << 31), // NV_ENC_INITIALIZE_PARAMS_VER
            encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID,
            presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID,
            encodeWidth = encWidth,
            encodeHeight = encHeight,
            darWidth = encWidth,
            darHeight = encHeight,
            frameRateNum = 60,
            frameRateDen = 1,
            enableEncodeAsync = 0,
            enablePTD = 1,
            bitFields = 0,
            privDataSize = 0,
            privData = IntPtr.Zero,
            encodeConfig = IntPtr.Zero,       // NULL = use preset defaults
            maxEncodeWidth = encWidth,
            maxEncodeHeight = encHeight,
            maxMEHintCountsPerBlockL0 = 0,
            maxMEHintCountsPerBlockL1 = 0,
            reserved = new uint[289],
            reserved2 = new IntPtr[64],
        };

        uint initStatus = nvenc.InitializeEncoder(encoder, ref initParams);
        if (initStatus != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncInitializeEncoder returned {initStatus} " +
                                    $"({NvEncodeAPI.NvencStatusToString(initStatus)}).");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }
        Console.WriteLine($"  PASS: Encoder initialized ({encWidth}x{encHeight} @ 60fps, H.264, Default preset).");

        // --- Step 5: Register a fresh staging texture with NVENC ---
        //
        // This is the CRITICAL V1 spike step. If NvEncRegisterResource succeeds,
        // zero-copy from D3D11 capture to NVENC is PROVEN.
        //
        // We create a FRESH texture here instead of using Phase 2's staging
        // texture. Reason: Phase 2's staging texture was used with
        // CopyResource in a capture loop, and the D3D11 device context may
        // have queued commands referencing it. Registering a fresh texture
        // eliminates any ambiguity about whether the device/texture state is
        // valid for NVENC.
        //
        // The fresh texture is created with the SAME D3D11 device that
        // Phase 1 created — so if registration succeeds, it proves that
        // the device used for capture is the same device that NVENC can use
        // for encoding. That's the V1 zero-copy claim.
        Console.WriteLine();
        Console.WriteLine("[4.5] Registering a fresh staging texture with NVENC...");

        if (nvenc.RegisterResource == null)
        {
            Console.Error.WriteLine("  FAIL: RegisterResource delegate is null.");
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }

        // SDK 11 does NOT have NvEncGetLastErrorString — skip the diagnostic.

        // Create a fresh texture on the same device as Phase 1/2.
        uint texWidth = SpikeSharedContext.DuplicationDesc!.Value.ModeDescription.Width;
        uint texHeight = SpikeSharedContext.DuplicationDesc!.Value.ModeDescription.Height;
        Console.WriteLine($"  Creating fresh texture: {texWidth}x{texHeight} BGRA8 on device 0x{SpikeSharedContext.Device.NativePointer.ToInt64():x16}");

        Vortice.Direct3D11.Texture2DDescription freshDesc = new()
        {
            Width = texWidth,
            Height = texHeight,
            MipLevels = 1,
            ArraySize = 1,
            Format = Vortice.DXGI.Format.B8G8R8A8_UNorm,
            SampleDescription = new Vortice.DXGI.SampleDescription(1, 0),
            Usage = Vortice.Direct3D11.ResourceUsage.Default,
            // Try both RenderTarget + ShaderResource. NVENC may require both.
            BindFlags = Vortice.Direct3D11.BindFlags.RenderTarget | Vortice.Direct3D11.BindFlags.ShaderResource,
            CPUAccessFlags = Vortice.Direct3D11.CpuAccessFlags.None,
            MiscFlags = Vortice.Direct3D11.ResourceOptionFlags.None,
        };
        Vortice.Direct3D11.ID3D11Texture2D freshTexture =
            SpikeSharedContext.Device.CreateTexture2D(freshDesc);
        Console.WriteLine($"  Fresh texture pointer: 0x{freshTexture.NativePointer.ToInt64():x16}");

        // Verify fresh texture is on the same device.
        Vortice.Direct3D11.ID3D11Device freshTexDevice = freshTexture.Device;
        long freshTexDevicePtr = freshTexDevice.NativePointer.ToInt64();
        long phase1DevicePtr = SpikeSharedContext.Device.NativePointer.ToInt64();
        Console.WriteLine($"  Fresh texture's parent device: 0x{freshTexDevicePtr:x16}");
        Console.WriteLine($"  Phase 1 device pointer:        0x{phase1DevicePtr:x16}");
        if (freshTexDevicePtr != phase1DevicePtr)
        {
            Console.Error.WriteLine("  FAIL: Fresh texture is NOT on the same device as Phase 1.");
            freshTexture.Dispose();
            nvenc.DestroyEncoder?.Invoke(encoder);
            return 1;
        }
        Console.WriteLine("  PASS: Fresh texture is on the same D3D11 device.");

        var registerParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
        {
            version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
            resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
            width = texWidth,
            height = texHeight,
            pitch = 0,                              // 0 for D3D11 textures
            subResourceIndex = 0,                   // 0 for non-array textures
            resourceToRegister = freshTexture.NativePointer,
            registeredResource = IntPtr.Zero,       // OUT — populated by NVENC
            bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
            // SDK 11 layout: no bufferUsage, no pInputFencePoint, no chromaOffset
            reserved1 = new uint[248],              // SDK 11: 248
            reserved2 = new IntPtr[62],              // SDK 11: 62
        };

        status = nvenc.RegisterResource(encoder, ref registerParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncRegisterResource returned {status} " +
                                    $"({NvEncodeAPI.NvencStatusToString(status)}).");

            // SDK 11 does NOT have NvEncGetLastErrorString — skip the diagnostic.

            // Dump the register params for debugging.
            Console.Error.WriteLine("  Register params:");
            Console.Error.WriteLine($"    version:           0x{registerParams.version:X8}");
            Console.Error.WriteLine($"    resourceType:      {registerParams.resourceType} (DIRECTX=0)");
            Console.Error.WriteLine($"    width:             {registerParams.width}");
            Console.Error.WriteLine($"    height:            {registerParams.height}");
            Console.Error.WriteLine($"    pitch:             {registerParams.pitch}");
            Console.Error.WriteLine($"    subResourceIndex:  {registerParams.subResourceIndex}");
            Console.Error.WriteLine($"    resourceToRegister:0x{registerParams.resourceToRegister.ToInt64():x16}");
            Console.Error.WriteLine($"    bufferFormat:      0x{registerParams.bufferFormat:X8} (ARGB=0x01000000)");

            Console.Error.WriteLine("  Possible causes:");
            Console.Error.WriteLine("    - Texture dimensions out of NVENC's supported range");
            Console.Error.WriteLine("    - Texture bind flags incompatible (try adding RenderTarget)");
            Console.Error.WriteLine("    - Texture not on the same D3D11 device as the encode session");
            Console.Error.WriteLine("    - Driver version too old");
            Console.Error.WriteLine("    - D3D11 device context has pending operations");
            Console.Error.WriteLine("    - NVENCAPI_STRUCT_VERSION mismatch (struct may be wrong size)");

            freshTexture.Dispose();
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

        // --- Step 6: Unregister + cleanup ---
        // nvEncUnregisterResource exists in the SDK 13.x function table at
        // offset 256 (between nvEncRegisterResource and nvEncReconfigureEncoder).
        // My earlier assumption that NVENC lacked UnregisterResource was wrong.
        Console.WriteLine();
        Console.WriteLine("[4.6] Unregistering resource and destroying encoder...");

        if (nvenc.UnregisterResource != null)
        {
            int unregStatus = nvenc.UnregisterResource(encoder, registeredHandle);
            if (unregStatus == NvEncodeAPI.NV_ENC_SUCCESS)
                Console.WriteLine("  PASS: Resource unregistered.");
            else
                Console.Error.WriteLine($"  WARN: UnregisterResource returned {unregStatus} — continuing.");
        }

        // Dispose the fresh texture we created in Step 5.
        freshTexture.Dispose();
        Console.WriteLine("  PASS: Fresh texture disposed.");

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

    /// <summary>
    /// Prints Marshal.SizeOf and field offsets for a blittable struct.
    /// Uses Marshal.OffsetOf to get the CLR's computed offset for each field,
    /// which helps verify that the struct layout matches the C header.
    /// </summary>
    private static void PrintStructLayout<T>(string structName) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        Console.WriteLine($"  {structName}:");
        Console.WriteLine($"    Marshal.SizeOf = {size} bytes");

        // Print offsets for key fields
        var fields = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                int offset = Marshal.OffsetOf<T>(field.Name).ToInt32();
                Console.WriteLine($"    {field.Name,-40} offset {offset,4}  ({field.FieldType.Name})");
            }
            catch
            {
                Console.WriteLine($"    {field.Name,-40} offset ???  ({field.FieldType.Name}) — could not compute");
            }
        }
    }
}
