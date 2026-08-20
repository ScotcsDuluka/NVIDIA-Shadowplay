// Phases/Phase6_MinimalEncode.cs
//
// P1-B.2-V1 Spike — Phase 6: Minimal H.264 Encode
//
// Goal: Prove that NVENC can encode a single D3D11 texture into a
// non-empty H.264 bitstream using the full encode pipeline:
//
//   Register → Map → Encode → LockBitstream → Copy → Unlock → Unmap → Unregister
//
// This phase depends on Phases 1-4 having already run (device creation,
// desktop duplication, texture ownership, NVENC registration).
// It creates its OWN encoder session (does not reuse Phase 4's).
//
// Acceptance criteria:
//   - NvEncEncodePicture returns NV_ENC_SUCCESS
//   - NvEncLockBitstream returns NV_ENC_SUCCESS
//   - bitstreamBufferPtr != IntPtr.Zero
//   - bitstreamSizeInBytes > 0
//   - First bytes of bitstream are plausible (Annex-B start code 00 00 00 01 or
//     at minimum non-zero)
//   - All cleanup operations succeed
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase6_MinimalEncode
{
    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 6 — Minimal H.264 Encode");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // Auto-run Phases 1-3 if not already done (Phase 6 depends on their shared context)
        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.WriteLine("  Phase 1-3 not yet run — auto-running them now...");
            Console.WriteLine();

            int p1 = Phase1_DeviceTest.Run();
            if (p1 != 0) { Console.Error.WriteLine("  FAIL: Phase 1 failed — cannot proceed."); return 1; }

            int p2 = Phase2_DesktopDuplication.Run();
            if (p2 != 0) { Console.Error.WriteLine("  FAIL: Phase 2 failed — cannot proceed."); return 1; }

            int p3 = Phase3_TextureOwnership.Run();
            if (p3 != 0) { Console.Error.WriteLine("  FAIL: Phase 3 failed — cannot proceed."); return 1; }

            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" Phase 6 — Minimal H.264 Encode (continuing after Phase 1-3)");
            Console.WriteLine("============================================================");
            Console.WriteLine();
        }

        // Now verify context is available
        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.Error.WriteLine("  FAIL: Phase 1-3 context still missing after auto-run.");
            return 1;
        }

        var device = SpikeSharedContext.Device;
        var duplDesc = SpikeSharedContext.DuplicationDesc.Value;
        // Vortice's ModeDescription.Width returns uint on OWNER's version
        uint texWidth = duplDesc.ModeDescription.Width;
        uint texHeight = duplDesc.ModeDescription.Height;

        Console.WriteLine($"  D3D11 device: 0x{device.NativePointer.ToInt64():x16}");
        Console.WriteLine($"  Texture size:  {texWidth}x{texHeight}");
        Console.WriteLine($"  Texture format: BGRA8 (ARGB in NVENC terms)");
        Console.WriteLine();

        // ─── Step 1: Load NVENC function table ───
        Console.WriteLine("[6.1] Loading NVENC function table...");
        using var nvenc = new NvEncFunctionTable();
        if (!nvenc.TryLoad())
        {
            Console.Error.WriteLine("  FAIL: Could not load NVENC function table.");
            return 1;
        }
        Console.WriteLine("  PASS: NVENC function table loaded.");
        PrintStructSizes();

        // ─── Step 2: Open encoder session ───
        Console.WriteLine();
        Console.WriteLine("[6.2] Opening NVENC encode session...");

        if (nvenc.OpenEncodeSessionEx == null)
        {
            Console.Error.WriteLine("  FAIL: OpenEncodeSessionEx delegate is null.");
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

        uint status = nvenc.OpenEncodeSessionEx(ref sessionParams, out IntPtr encoder);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS)
        {
            Console.Error.WriteLine($"  FAIL: NvEncOpenEncodeSessionEx returned {status} " +
                                    $"({NvEncodeAPI.NvencStatusToString(status)}).");
            return 1;
        }
        Console.WriteLine($"  PASS: Encode session opened. Encoder: 0x{encoder.ToInt64():x16}");

        // Track resources for cleanup
        IntPtr bitstreamBuffer = IntPtr.Zero;
        IntPtr registeredResource = IntPtr.Zero;
        IntPtr mappedInputResource = IntPtr.Zero;
        bool encoderInitialized = false;

        try
        {
            // ─── Step 3: Initialize encoder (H.264, default preset) ───
            Console.WriteLine();
            Console.WriteLine("[6.3] Initializing encoder (H.264, Default preset, sync mode)...");

            if (nvenc.InitializeEncoder == null)
            {
                Console.Error.WriteLine("  FAIL: InitializeEncoder delegate is null.");
                return 1;
            }

            var initParams = new NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS
            {
                version = NvEncodeAPI.MakeStructVersion(5) | (1u << 31),
                encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID,
                presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID,
                encodeWidth = (uint)texWidth,
                encodeHeight = (uint)texHeight,
                darWidth = (uint)texWidth,
                darHeight = (uint)texHeight,
                frameRateNum = 60,
                frameRateDen = 1,
                enableEncodeAsync = 0,   // synchronous mode
                enablePTD = 1,
                bitFields = 0,
                privDataSize = 0,
                privData = IntPtr.Zero,
                encodeConfig = IntPtr.Zero,  // NULL = use preset defaults
                maxEncodeWidth = (uint)texWidth,
                maxEncodeHeight = (uint)texHeight,
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
                return 1;
            }
            encoderInitialized = true;
            Console.WriteLine($"  PASS: Encoder initialized ({texWidth}x{texHeight} @ 60fps, H.264, Default preset).");

            // ─── Step 4: Create bitstream buffer ───
            Console.WriteLine();
            Console.WriteLine("[6.4] Creating bitstream buffer...");

            if (nvenc.CreateBitstreamBuffer == null)
            {
                Console.Error.WriteLine("  FAIL: CreateBitstreamBuffer delegate is null.");
                return 1;
            }

            var bstreamParams = new NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER
            {
                version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER,
                size = 0,  // 0 = default
                memoryHeap = 0,
                _padding = 0,
                bitstreamBuffer = IntPtr.Zero,
                reserved1 = IntPtr.Zero,
                reserved2 = IntPtr.Zero,
                reserved3 = new uint[226],
                reserved4 = new IntPtr[64],
            };

            uint bsStatus = nvenc.CreateBitstreamBuffer(encoder, ref bstreamParams);
            if (bsStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncCreateBitstreamBuffer returned {bsStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(bsStatus)}).");
                return 1;
            }
            bitstreamBuffer = bstreamParams.bitstreamBuffer;
            Console.WriteLine($"  PASS: Bitstream buffer created. Handle: 0x{bitstreamBuffer.ToInt64():x16}");

            // ─── Step 5: Create fresh texture + Register with NVENC ───
            Console.WriteLine();
            Console.WriteLine("[6.5] Creating fresh texture + registering with NVENC...");

            if (nvenc.RegisterResource == null)
            {
                Console.Error.WriteLine("  FAIL: RegisterResource delegate is null.");
                return 1;
            }

            // Create a fresh BGRA8 texture (same as Phase 4 Step 5)
            Texture2DDescription freshDesc = new()
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
            ID3D11Texture2D freshTexture = device.CreateTexture2D(freshDesc);
            Console.WriteLine($"  Fresh texture: {texWidth}x{texHeight} BGRA8, ptr=0x{freshTexture.NativePointer.ToInt64():x16}");

            // Fill texture with a simple pattern (solid blue) so NVENC has real data to encode
            var rtvDesc = device.CreateRenderTargetView(freshTexture);
            device.ImmediateContext.ClearRenderTargetView(rtvDesc, new Vortice.Mathematics.Color(0.0f, 0.0f, 1.0f, 1.0f));
            rtvDesc.Dispose();
            Console.WriteLine("  Texture filled with solid blue (0,0,255,255).");

            var registerParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
            {
                version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
                resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
                width = (uint)texWidth,
                height = (uint)texHeight,
                pitch = 0,  // 0 for textures
                subResourceIndex = 0,
                resourceToRegister = freshTexture.NativePointer,
                registeredResource = IntPtr.Zero,
                bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                reserved1 = new uint[248],
                reserved2 = new IntPtr[62],
            };

            // DIAGNOSTIC: print version field so we can verify the MakeStructVersion
            // macro is producing the correct value (NVIDIA expects 0x700D0003 for
            // NV_ENC_REGISTER_RESOURCE_VER with API version 13.0).
            Console.WriteLine($"  RegisterResource version field: 0x{registerParams.version:X8} " +
                              $"(expected 0x700D0003 for API 13.0)");

            // DIAGNOSTIC: dump full struct layout + raw bytes before native call.
            // VERIFIED expected layout from NVIDIA nvEncodeAPI.h (both SDK 11 and 13):
            //   offset 0:   version (4)
            //   offset 4:   resourceType (4)
            //   offset 8:   width (4)
            //   offset 12:  height (4)
            //   offset 16:  pitch (4)
            //   offset 20:  subResourceIndex (4)
            //   offset 24:  resourceToRegister (8)
            //   offset 32:  registeredResource (8, OUT)
            //   offset 40:  bufferFormat (4)
            //   offset 44:  bufferUsage (4)  ← WE DON'T HAVE THIS FIELD
            //   offset 48:  reserved1[247] (988)
            //   offset 1040: reserved2[62] (496)
            // Total: 1536 bytes (we have 1532)
            DumpStruct("NV_ENC_REGISTER_RESOURCE (before native call)", ref registerParams, 80);

            uint regStatus = nvenc.RegisterResource(encoder, ref registerParams);
            if (regStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncRegisterResource returned {regStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(regStatus)}).");
                freshTexture.Dispose();
                return 1;
            }
            registeredResource = registerParams.registeredResource;
            Console.WriteLine($"  PASS: Texture registered. Handle: 0x{registeredResource.ToInt64():x16}");

            // ─── Step 6: Map input resource ───
            Console.WriteLine();
            Console.WriteLine("[6.6] Mapping input resource...");

            if (nvenc.MapInputResource == null)
            {
                Console.Error.WriteLine("  FAIL: MapInputResource delegate is null.");
                return 1;
            }

            var mapParams = new NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE
            {
                version = NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER,
                subResourceIndex = 0,
                inputResource = registeredResource,
                mappedInputResource = IntPtr.Zero,
                reserved1 = new IntPtr[246],
                reserved2 = new IntPtr[63],
            };

            // DIAGNOSTIC: print version field + handle being passed, to verify
            // both that the macro is correct (expected 0x700D0004 for API 13.0)
            // and that the registered handle is exactly what RegisterResource returned.
            Console.WriteLine($"  MapInputResource version field:  0x{mapParams.version:X8} " +
                              $"(expected 0x700D0004 for API 13.0)");
            Console.WriteLine($"  MapInputResource inputResource:  0x{mapParams.inputResource.ToInt64():x16}");
            Console.WriteLine($"  (Should match registeredResource handle above.)");

            // DIAGNOSTIC: dump full struct layout + raw bytes before native call.
            // VERIFIED expected layout from NVIDIA nvEncodeAPI.h (BOTH SDK 11 and 13 are identical):
            //   offset 0:   version (4)
            //   offset 4:   subResourceIndex (4)
            //   offset 8:   inputResource (8)  ← NV_ENC_REGISTERED_PTR
            //   offset 16:  registeredResource (8)  ← WE DON'T HAVE THIS FIELD
            //   offset 24:  mappedResource (8, OUT)  ← WE HAVE THIS AT WRONG OFFSET (16)
            //   offset 32:  mappedBufferFmt (4)  ← WE DON'T HAVE THIS FIELD
            //   offset 36:  reserved1[251] (1004)
            //   offset 1040: reserved2[63] (504)
            // Total: 1544 bytes (we have 2496 — WAY too big!)
            DumpStruct("NV_ENC_MAP_INPUT_RESOURCE (before native call)", ref mapParams, 80);

            // Flush the D3D11 context to ensure the ClearRenderTargetView command
            // has completed before NVENC tries to map the texture. NVIDIA samples
            // don't always do this, but it's defensive — if the GPU command queue
            // has pending work, MapInputResource might fail.
            device.ImmediateContext.Flush();

            uint mapStatus = nvenc.MapInputResource(encoder, ref mapParams);
            if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncMapInputResource returned {mapStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(mapStatus)}).");
                Console.Error.WriteLine("  Likely cause (based on verified NVIDIA header):");
                Console.Error.WriteLine("    Our NV_ENC_MAP_INPUT_RESOURCE struct is 2496 bytes but NVIDIA expects 1544.");
                Console.Error.WriteLine("    We are missing the registeredResource field at offset 16,");
                Console.Error.WriteLine("    the mappedBufferFmt field at offset 32, and our mappedInputResource");
                Console.Error.WriteLine("    field is at the wrong offset (16 vs NVIDIA's 24).");
                Console.Error.WriteLine("    NVENC reads our mappedInputResource bytes (mostly zeros since we set it");
                Console.Error.WriteLine("    to IntPtr.Zero) as if it were the registeredResource field — finding");
                Console.Error.WriteLine("    NULL instead of the actual registered handle — and returns");
                Console.Error.WriteLine("    NV_ENC_ERR_RESOURCE_NOT_REGISTERED (status 20).");
                return 1;
            }
            mappedInputResource = mapParams.mappedInputResource;
            Console.WriteLine($"  PASS: Input resource mapped. Handle: 0x{mappedInputResource.ToInt64():x16}");

            // ─── Step 7: Encode picture ───
            Console.WriteLine();
            Console.WriteLine("[6.7] Encoding picture (synchronous)...");

            if (nvenc.EncodePicture == null)
            {
                Console.Error.WriteLine("  FAIL: EncodePicture delegate is null.");
                return 1;
            }

            var picParams = new NvEncodeAPI.NV_ENC_PIC_PARAMS
            {
                version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
                inputWidth = texWidth,
                inputHeight = texHeight,
                inputPitch = 0,
                encodePicFlags = 0,  // normal encode
                frameIdx = 0,
                inputTimeStamp = 0,  // no timestamp for spike
                inputDuration = 0,
                codecPicHints_bitfield = 0,
                _padding1 = 0,
                outputBitstream = bitstreamBuffer,
                inputBuffer = mappedInputResource,
                bufferFmt = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                picStruct = 1,  // NV_ENC_PIC_STRUCT_FRAME = 1
                picType = 0,  // OUT
                _padding2 = 0,
                reserved1 = new uint[244],
                reserved2 = new IntPtr[63],
            };

            uint encStatus = nvenc.EncodePicture(encoder, ref picParams);
            if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncEncodePicture returned {encStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(encStatus)}).");
                return 1;
            }
            Console.WriteLine("  PASS: NvEncEncodePicture returned NV_ENC_SUCCESS.");

            // ─── Step 8: Lock bitstream ───
            Console.WriteLine();
            Console.WriteLine("[6.8] Locking bitstream (retrieving encoded data)...");

            if (nvenc.LockBitstream == null)
            {
                Console.Error.WriteLine("  FAIL: LockBitstream delegate is null.");
                return 1;
            }

            var lockParams = new NvEncodeAPI.NV_ENC_LOCK_BITSTREAM
            {
                version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER,
                doNotWait = 0,  // blocking — wait for encode to complete
                ltrFrameIdx = 0,
                reserved1 = 0,
                outputBitstream = bitstreamBuffer,
                sliceType = 0,  // OUT
                picType = 0,    // OUT
                picIdx = 0,     // OUT
                _padding = 0,
                bitstreamBufferPtr = IntPtr.Zero,  // OUT
                bitstreamSizeInBytes = 0,           // OUT
                _padding2 = 0,
                frameIdx = 0,    // OUT
                _padding3 = 0,
                inputTimeStamp = 0,  // OUT
                inputDuration = 0,   // OUT
                reserved2 = new uint[221],
                _padding4 = 0,
                reserved3 = new IntPtr[63],
            };

            uint lockStatus = nvenc.LockBitstream(encoder, ref lockParams);
            if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncLockBitstream returned {lockStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(lockStatus)}).");
                return 1;
            }

            uint bsSize = lockParams.bitstreamSizeInBytes;
            IntPtr bsPtr = lockParams.bitstreamBufferPtr;

            Console.WriteLine($"  PASS: NvEncLockBitstream returned NV_ENC_SUCCESS.");
            Console.WriteLine($"  Bitstream pointer: 0x{bsPtr.ToInt64():x16}");
            Console.WriteLine($"  Bitstream size:    {bsSize} bytes");
            Console.WriteLine($"  Slice type:        {lockParams.sliceType}");
            Console.WriteLine($"  Pic type:          {lockParams.picType}");
            Console.WriteLine($"  Pic idx:           {lockParams.picIdx}");
            Console.WriteLine($"  Frame idx:         {lockParams.frameIdx}");

            // ─── Validation ───
            Console.WriteLine();
            Console.WriteLine("[6.9] Validating bitstream...");

            if (bsPtr == IntPtr.Zero)
            {
                Console.Error.WriteLine("  FAIL: Bitstream pointer is NULL.");
                return 1;
            }
            Console.WriteLine("  PASS: Bitstream pointer is non-zero.");

            if (bsSize == 0)
            {
                Console.Error.WriteLine("  FAIL: Bitstream size is 0.");
                return 1;
            }
            Console.WriteLine($"  PASS: Bitstream size > 0 ({bsSize} bytes).");

            // Copy first 32 bytes to managed memory for inspection
            int copyLen = (int)Math.Min(bsSize, 32);
            byte[] firstBytes = new byte[copyLen];
            Marshal.Copy(bsPtr, firstBytes, 0, copyLen);

            // Print as hex
            string hex = BitConverter.ToString(firstBytes).Replace("-", " ");
            Console.WriteLine($"  First {copyLen} bytes: {hex}");

            // Check for Annex-B start code (00 00 00 01 or 00 00 01)
            bool annexB = false;
            if (copyLen >= 4)
            {
                if (firstBytes[0] == 0x00 && firstBytes[1] == 0x00 &&
                    firstBytes[2] == 0x00 && firstBytes[3] == 0x01)
                {
                    annexB = true;
                }
                else if (copyLen >= 3 &&
                         firstBytes[0] == 0x00 && firstBytes[1] == 0x00 &&
                         firstBytes[2] == 0x01)
                {
                    annexB = true;
                }
            }

            if (annexB)
            {
                Console.WriteLine("  PASS: Annex-B start code detected (00 00 00 01 or 00 00 01).");
            }
            else
            {
                Console.WriteLine("  WARN: Annex-B start code NOT detected in first 4 bytes.");
                Console.WriteLine("        This may be normal — NVENC may output AVC (length-prefixed) format.");
                Console.WriteLine("        The bitstream is still valid if size > 0 and NV_ENC_SUCCESS was returned.");
            }

            // ─── Step 10: Unlock bitstream ───
            Console.WriteLine();
            Console.WriteLine("[6.10] Unlocking bitstream...");

            if (nvenc.UnlockBitstream == null)
            {
                Console.Error.WriteLine("  FAIL: UnlockBitstream delegate is null.");
                return 1;
            }

            uint unlockStatus = nvenc.UnlockBitstream(encoder, bitstreamBuffer);
            if (unlockStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncUnlockBitstream returned {unlockStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(unlockStatus)}).");
                return 1;
            }
            Console.WriteLine("  PASS: Bitstream unlocked.");

            // ─── Step 11: Unmap input resource ───
            Console.WriteLine();
            Console.WriteLine("[6.11] Unmapping input resource...");

            if (nvenc.UnmapInputResource == null)
            {
                Console.Error.WriteLine("  FAIL: UnmapInputResource delegate is null.");
                return 1;
            }

            uint unmapStatus = nvenc.UnmapInputResource(encoder, mappedInputResource);
            if (unmapStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncUnmapInputResource returned {unmapStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(unmapStatus)}).");
                return 1;
            }
            mappedInputResource = IntPtr.Zero;  // cleared
            Console.WriteLine("  PASS: Input resource unmapped.");

            // ─── Step 12: Unregister resource ───
            Console.WriteLine();
            Console.WriteLine("[6.12] Unregistering resource...");

            if (nvenc.UnregisterResource == null)
            {
                Console.Error.WriteLine("  FAIL: UnregisterResource delegate is null.");
                return 1;
            }

            uint unregStatus = nvenc.UnregisterResource(encoder, registeredResource);
            if (unregStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncUnregisterResource returned {unregStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(unregStatus)}).");
                return 1;
            }
            registeredResource = IntPtr.Zero;  // cleared
            Console.WriteLine("  PASS: Resource unregistered.");

            // ─── Step 13: Destroy bitstream buffer ───
            Console.WriteLine();
            Console.WriteLine("[6.13] Destroying bitstream buffer...");

            if (nvenc.DestroyBitstreamBuffer == null)
            {
                Console.Error.WriteLine("  FAIL: DestroyBitstreamBuffer delegate is null.");
                return 1;
            }

            uint destroyBsStatus = nvenc.DestroyBitstreamBuffer(encoder, bitstreamBuffer);
            if (destroyBsStatus != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine($"  FAIL: NvEncDestroyBitstreamBuffer returned {destroyBsStatus} " +
                                        $"({NvEncodeAPI.NvencStatusToString(destroyBsStatus)}).");
                return 1;
            }
            bitstreamBuffer = IntPtr.Zero;  // cleared
            Console.WriteLine("  PASS: Bitstream buffer destroyed.");

            // Dispose the fresh texture
            freshTexture.Dispose();
            Console.WriteLine("  PASS: Fresh texture disposed.");

            // ─── Phase 6 Result ───
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine(" Phase 6 RESULT");
            Console.WriteLine("============================================================");
            Console.WriteLine($"  EncodePicture:   NV_ENC_SUCCESS");
            Console.WriteLine($"  LockBitstream:   NV_ENC_SUCCESS");
            Console.WriteLine($"  Bitstream size:  {bsSize} bytes");
            Console.WriteLine($"  First {copyLen} bytes: {hex}");
            Console.WriteLine($"  Annex-B:          {(annexB ? "DETECTED" : "NOT detected (may be AVC length-prefixed)")}");
            Console.WriteLine($"  Cleanup:         ALL SUCCEEDED");
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.WriteLine("  Phase 6: PASS — non-empty H.264 bitstream produced.");
            Console.WriteLine();

            return 0;
        }
        finally
        {
            // Cleanup (best-effort, don't throw)
            Console.WriteLine();
            Console.WriteLine("[6.cleanup] Best-effort cleanup...");

            if (mappedInputResource != IntPtr.Zero && nvenc.UnmapInputResource != null)
            {
                try { nvenc.UnmapInputResource(encoder, mappedInputResource); }
                catch { }
                Console.WriteLine("  Mapped input resource unmapped (cleanup).");
            }

            if (registeredResource != IntPtr.Zero && nvenc.UnregisterResource != null)
            {
                try { nvenc.UnregisterResource(encoder, registeredResource); }
                catch { }
                Console.WriteLine("  Registered resource unregistered (cleanup).");
            }

            if (bitstreamBuffer != IntPtr.Zero && nvenc.DestroyBitstreamBuffer != null)
            {
                try { nvenc.DestroyBitstreamBuffer(encoder, bitstreamBuffer); }
                catch { }
                Console.WriteLine("  Bitstream buffer destroyed (cleanup).");
            }

            if (encoderInitialized && nvenc.DestroyEncoder != null)
            {
                try { nvenc.DestroyEncoder(encoder); }
                catch { }
                Console.WriteLine("  Encoder destroyed (cleanup).");
            }
        }
    }

    private static void PrintStructSizes()
    {
        Console.WriteLine("  Struct sizes (Marshal.SizeOf):");
        Console.WriteLine($"    NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS: {Marshal.SizeOf<NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS>()} bytes");
        Console.WriteLine($"    NV_ENC_INITIALIZE_PARAMS:              {Marshal.SizeOf<NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS>()} bytes");
        Console.WriteLine($"    NV_ENC_REGISTER_RESOURCE:               {Marshal.SizeOf<NvEncodeAPI.NV_ENC_REGISTER_RESOURCE>()} bytes");
        Console.WriteLine($"    NV_ENC_CREATE_BITSTREAM_BUFFER:         {Marshal.SizeOf<NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER>()} bytes");
        Console.WriteLine($"    NV_ENC_MAP_INPUT_RESOURCE:              {Marshal.SizeOf<NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE>()} bytes");
        Console.WriteLine($"    NV_ENC_PIC_PARAMS:                      {Marshal.SizeOf<NvEncodeAPI.NV_ENC_PIC_PARAMS>()} bytes");
        Console.WriteLine($"    NV_ENC_LOCK_BITSTREAM:                  {Marshal.SizeOf<NvEncodeAPI.NV_ENC_LOCK_BITSTREAM>()} bytes");
        Console.WriteLine("  Version field values (with OLD macro):");
        Console.WriteLine($"    NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER: 0x{NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER:X8}");
        Console.WriteLine($"    NV_ENC_INITIALIZE_PARAMS_VER:             0x{(NvEncodeAPI.MakeStructVersion(5) | (1u << 31)):X8}");
        Console.WriteLine($"    NV_ENC_REGISTER_RESOURCE_VER:             0x{NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER:X8}");
        Console.WriteLine($"    NV_ENC_CREATE_BITSTREAM_BUFFER_VER:       0x{NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER:X8}");
        Console.WriteLine($"    NV_ENC_MAP_INPUT_RESOURCE_VER:            0x{NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER:X8}");
        Console.WriteLine($"    NV_ENC_PIC_PARAMS_VER:                    0x{NvEncodeAPI.NV_ENC_PIC_PARAMS_VER:X8}");
        Console.WriteLine($"    NV_ENC_LOCK_BITSTREAM_VER:                0x{NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER:X8}");
    }

    /// <summary>
    /// Dumps a struct for diagnostic purposes:
    ///   - Marshal.SizeOf (actual managed struct size)
    ///   - Marshal.OffsetOf for every public field (with type)
    ///   - Field values (best-effort string representation)
    ///   - First N raw bytes (in hex) of the marshalled struct
    ///
    /// This is used before NvEncRegisterResource and NvEncMapInputResource
    /// so we can verify the struct layout and bytes being sent to NVENC.
    /// </summary>
    private static void DumpStruct<T>(string label, ref T structure, int dumpByteCount = 64) where T : struct
    {
        Console.WriteLine($"  --- DUMP: {label} ---");
        Type t = typeof(T);
        int managedSize = Marshal.SizeOf<T>();
        Console.WriteLine($"  Managed type:      {t.Name}");
        Console.WriteLine($"  Marshal.SizeOf:    {managedSize} bytes");

        // Use reflection to enumerate public fields in declaration order
        FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        Console.WriteLine($"  Field count:       {fields.Length}");
        Console.WriteLine($"  Field offsets:");
        foreach (FieldInfo f in fields)
        {
            // Marshal.OffsetOf throws for some marshalled array fields; catch and continue
            long offset;
            try
            {
                offset = (long)Marshal.OffsetOf<T>(f.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ???  : {f.Name,-25} (offset query failed: {ex.GetType().Name})");
                continue;
            }

            string typeName;
            if (f.FieldType.IsArray)
            {
                Type? elemType = f.FieldType.GetElementType();
                string elemName = elemType?.Name ?? "?";
                int arrLen = -1;
                try
                {
                    Array? arr = (Array?)f.GetValue(structure);
                    arrLen = arr?.Length ?? -1;
                }
                catch { }
                typeName = $"{elemName}[{arrLen}]";
            }
            else
            {
                typeName = f.FieldType.Name;
            }

            string valStr;
            try
            {
                object? val = f.GetValue(structure);
                valStr = val switch
                {
                    IntPtr p => $"0x{p.ToInt64():x16}",
                    uint u => $"0x{u:X8} ({u})",
                    int i => $"{i}",
                    ulong ul => $"0x{ul:X16}",
                    long l => $"{l}",
                    null => "<null>",
                    _ => val.ToString() ?? "<toString returned null>"
                };
            }
            catch (Exception ex)
            {
                valStr = $"<getValue failed: {ex.GetType().Name}>";
            }
            Console.WriteLine($"    offset {offset,4}: {f.Name,-25} ({typeName,-25}) = {valStr}");
        }

        // Dump raw bytes of the marshalled struct
        IntPtr buf = Marshal.AllocHGlobal(managedSize);
        try
        {
            Marshal.StructureToPtr(structure, buf, false);
            int dumpLen = Math.Min(dumpByteCount, managedSize);
            byte[] bytes = new byte[dumpLen];
            Marshal.Copy(buf, bytes, 0, dumpLen);

            Console.WriteLine($"  First {dumpLen} raw bytes (hex):");
            for (int i = 0; i < dumpLen; i += 16)
            {
                int lineLen = Math.Min(16, dumpLen - i);
                string hexPart = BitConverter.ToString(bytes, i, lineLen).Replace("-", " ");
                string asciiPart = new string(bytes.Skip(i).Take(lineLen)
                    .Select(b => (b >= 0x20 && b < 0x7F) ? (char)b : '.').ToArray());
                Console.WriteLine($"    {i,4:X4}: {hexPart,-48} | {asciiPart}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Raw byte dump failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            Marshal.FreeHGlobal(buf);
        }
        Console.WriteLine($"  --- END DUMP: {label} ---");
    }
}
