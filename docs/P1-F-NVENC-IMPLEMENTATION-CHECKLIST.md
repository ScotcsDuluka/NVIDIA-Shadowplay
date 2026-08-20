# P1-F NVENC Implementation Checklist

> Complete checklist for implementing a synchronous H.264 NVENC encoder backend.
>
> **UPDATE (2026-08-20):** D3D11 spike has been RUNTIME-VALIDATED by OWNER on
> GTX 1080 Ti. Phase 1-4 ALL PASS. V1 BLOCKER RESOLVED — NvEncRegisterResource
> returns NV_ENC_SUCCESS. Zero-copy path PROVEN at runtime.
>
> **UPDATE (2026-08-20):** GLM-3 has committed new frame contract foundation
> (commit 0136031) with `IVideoFrame.ResourceHandle As IntPtr` — this resolves
> Contract Gap #1 from P1-F-NVENC-CONTRACT-GAP.md. The frame contract now
> carries the native pointer NVENC needs.

## Runtime Evidence Summary

| Phase | Status | Key Result | Evidence |
|---|---|---|---|
| Phase 1 — D3D11 Device | ✅ RUNTIME-PROVEN | NVIDIA GTX 1080 Ti adapter found, device created with BgraSupport + VideoSupport | `D3D11_NVENC_Spike_Report.md` commit 50c5d24 |
| Phase 2 — Desktop Duplication | ✅ RUNTIME-PROVEN | 1000 frames captured, FPS=109.83, p95 latency=0.05ms, 0 ACCESS_LOST | Same report |
| Phase 3 — Texture Ownership | ✅ RUNTIME-PROVEN | BGRA8, GPU-resident, same device, ArraySize=1 | Same report |
| Phase 4 — NVENC Registration | ✅ RUNTIME-PROVEN | NvEncRegisterResource = NV_ENC_SUCCESS. V1 BLOCKER RESOLVED. | Same report |
| Phase 5 — Performance Benchmark | ✅ RUNTIME-PROVEN | 1680×1050: 60fps target → 101.52 FPS achieved, avg latency=6.03ms | Same report |
| Encode pipeline (Map→Encode→Lock) | ❌ NOT IMPLEMENTED | 9 of 17 API functions still missing structs + delegates | `P1-F-NVENC-API-MAP.md` |

## Contract Status

| Gap (from P1-F-NVENC-CONTRACT-GAP.md) | Status | Resolution |
|---|---|---|
| #1: No native texture pointer in IVideoFrame | ✅ RESOLVED | GLM-3 commit 0136031: new `IVideoFrame.ResourceHandle As IntPtr` |
| #2: Texture lifetime vs NVENC registration | ⚠️ PARTIALLY RESOLVED | New `VideoFrame` has cleanup callback — encoder can defer Dispose. But encoder-owned texture pool still recommended. |
| #3: No adapter/device identity on frames | ⚠️ PARTIALLY RESOLVED | `FrameMetadata.Source` string identifies backend. But raw `ID3D11Device*` not on frame — encoder must verify via device pointer match. |
| #4: Missing NVENC structs | ❌ STILL MISSING | See checklist below |

---

## Native Structures Required for Minimum Synchronous H.264 Encode

### Already Defined in Spike (SOURCE-PROVEN):

#### 1. NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS

| Property | Value |
|---|---|
| **File** | `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` lines 228-240 |
| **Native size** | 1552 bytes (x64) |
| **SDK version** | SDK 11 (struct ver = 1) |
| **Layout** | `LayoutKind.Sequential, Pack = 1` |
| **Required fields** | version, deviceType (0x00=DIRECTX), device (D3D11Device.NativePointer), apiVersion (NVENCAPI_VERSION) |
| **Runtime validated** | ✅ Phase 4 PASS — NvEncOpenEncodeSessionEx returned NV_ENC_SUCCESS |
| **Fields verified at runtime** | deviceType=0x00, apiVersion=0x0000000D (NVENC 13.0) |

#### 2. NV_ENC_INITIALIZE_PARAMS

| Property | Value |
|---|---|
| **File** | `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` lines 324-352 |
| **Native size** | 1784 bytes (x64) |
| **SDK version** | SDK 11 (struct ver = 5, with 0x1<<31 flag) |
| **Layout** | `LayoutKind.Sequential, Pack = 1` |
| **Required fields** | version, encodeGUID (H264_GUID), presetGUID (DEFAULT_GUID), encodeWidth, encodeHeight, darWidth, darHeight, frameRateNum, frameRateDen, enableEncodeAsync (0=sync), enablePTD (1), encodeConfig (NULL=preset defaults), maxEncodeWidth, maxEncodeHeight |
| **Runtime validated** | ✅ Phase 4 PASS — NvEncInitializeEncoder returned NV_ENC_SUCCESS |
| **Fields verified at runtime** | 1680×1050 @ 60fps, H.264, Default preset, enableEncodeAsync=0, enablePTD=1 |
| **IMPORTANT** | version field uses `MakeStructVersion(5) | (1u << 31)` — the bit 31 flag is REQUIRED for SDK 11 compat. Without it, NvEncInitializeEncoder fails. |

#### 3. NV_ENC_REGISTER_RESOURCE

| Property | Value |
|---|---|
| **File** | `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` lines 267-283 |
| **Native size** | 1532 bytes (x64) |
| **SDK version** | SDK 11 (struct ver = 3) |
| **Layout** | `LayoutKind.Sequential, Pack = 1` |
| **Required fields** | version, resourceType (0x00=DIRECTX), width, height, pitch (0 for textures), subResourceIndex (0), resourceToRegister (ID3D11Texture2D.NativePointer), bufferFormat (0x01000000=ARGB) |
| **Output** | registeredResource (IntPtr — NVENC's opaque handle) |
| **Runtime validated** | ✅ Phase 4 PASS — NvEncRegisterResource returned NV_ENC_SUCCESS |
| **Fields verified at runtime** | width=1680, height=1050, bufferFormat=ARGB (BGRA8), resourceToRegister=fresh texture pointer |
| **SDK 11 vs SDK 13 difference** | SDK 11 has reserved1[248] + reserved2[62]. SDK 13 has bufferUsage + chromaOffset fields + reserved1[244] + reserved2[61]. Using SDK 11 layout because OWNER's DLL is SDK 11. |

#### 4. NV_ENCODE_API_FUNCTION_LIST

| Property | Value |
|---|---|
| **File** | `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` lines 408-453 |
| **Native size** | 2552 bytes (x64) |
| **SDK version** | SDK 11 (struct ver = 1, 38 function pointers + reserved2[281]) |
| **Layout** | `LayoutKind.Sequential, Pack = 1` |
| **Runtime validated** | ✅ Phase 4 PASS — NvEncodeAPICreateInstance returned NV_ENC_SUCCESS |
| **Function pointers used** | nvEncOpenEncodeSessionEx (offset 240), nvEncGetEncodeGUIDCount (16), nvEncGetEncodeGUIDs (40), nvEncGetInputFormatCount (48), nvEncGetInputFormats (56), nvEncInitializeEncoder (96), nvEncRegisterResource (248), nvEncUnregisterResource (256), nvEncDestroyEncoder (224) |
| **Function pointers NOT YET marshalled** | nvEncMapInputResource (208), nvEncUnmapInputResource (216), nvEncEncodePicture (136), nvEncLockBitstream (144), nvEncUnlockBitstream (152), nvEncCreateBitstreamBuffer (120), nvEncDestroyBitstreamBuffer (128) |

---

### NOT Yet Defined (SPIKE-DEFINED but structs missing):

#### 5. NV_ENC_CONFIG

| Property | Value |
|---|---|
| **Status** | ❌ NOT DEFINED — no struct, no delegate |
| **Purpose** | Codec-specific configuration (H.264 profile, level, rate control, GOP, B-frames, etc.) |
| **Where used** | `NV_ENC_INITIALIZE_PARAMS.encodeConfig` (pointer to this struct). If NULL, NVENC uses preset defaults. |
| **Minimum approach** | Pass NULL initially — use preset defaults. Struct needed only when custom rate control / GOP / B-frames are required. |
| **Native size** | UNKNOWN — must verify against `nvEncodeAPI.h` SDK 11. Estimate: ~3000+ bytes (contains nested `NV_ENC_CONFIG_H264`). |
| **SDK version** | SDK 11 |
| **Fields needed** | `profileGUID`, `frameIntervalP` (1=I-only, 2=IB, 3=IBP), `rcParams` (rate control: CBR/VBR/CQ), `encodeCodecConfig` (nested H.264 config: `NV_ENC_CONFIG_H264`) |
| **Risk** | HIGH — this is the most complex NVENC struct. Layout must match SDK 11 exactly or NvEncInitializeEncoder will fail. |
| **Verification source** | NVIDIA Video Codec SDK 11 header `nvEncodeAPI.h` — OWNER must provide or confirm SDK version. |

#### 6. NV_ENC_CONFIG_H264

| Property | Value |
|---|---|
| **Status** | ❌ NOT DEFINED |
| **Purpose** | H.264-specific encoder configuration (profile, level, entropy coding, B-frames, reference frames, etc.) |
| **Where used** | Nested inside `NV_ENC_CONFIG.encodeCodecConfig` |
| **Native size** | UNKNOWN — must verify against `nvEncodeAPI.h` SDK 11 |
| **Fields needed** | `profile`, `level`, `idrPeriod`, `numRefL0`, `numRefL1`, `bframeCount`, `bAdaptiveBFramePattern`, `entropyCodingMode` (0=CABAC, 1=CAVLC) |
| **Risk** | HIGH — nested struct, complex layout. |

#### 7. NV_ENC_MAP_INPUT_RESOURCE

| Property | Value |
|---|---|
| **Status** | ❌ NOT DEFINED — function pointer exists at offset 208 but no struct, no delegate |
| **Purpose** | Maps a registered resource to an NVENC input buffer for encoding |
| **Input** | version, registeredResource (from NvEncRegisterResource output) |
| **Output** | `mappedInputResource` — handle for NvEncEncodePicture |
| **Native size** | UNKNOWN — must verify against `nvEncodeAPI.h` SDK 11. |
| **Fields needed** | version, subResourceIndex (0), inputResource (registered resource handle), mappedInputResource (OUT), reserved1[247], reserved2[63] |
| **Risk** | MEDIUM — similar size to NV_ENC_REGISTER_RESOURCE but different reserved array sizes. |
| **Verification source** | `nvEncodeAPI.h` SDK 11. |

#### 8. NV_ENC_PIC_PARAMS

| Property | Value |
|---|---|
| **Status** | ❌ NOT DEFINED — function pointer exists at offset 136 but no struct, no delegate |
| **Purpose** | Parameters for a single NvEncEncodePicture call |
| **Input** | version, inputWidth, inputHeight, outputBitstream (bitstream buffer handle), inputBuffer (mapped input resource), inputTimeStamp, encodePicFlags, frameIdx, bufferFmt |
| **Native size** | UNKNOWN — must verify. Estimate: ~200-400 bytes. |
| **Fields needed** | version, inputWidth, inputHeight, inputPitch (0 for textures), encodePicFlags (0 for normal), frameIdx, inputTimeStamp (frame PTS), inputDuration (0), outputBitstream (from NvEncCreateBitstreamBuffer), inputBuffer (from NvEncMapInputResource), bufferFmt (0x01000000=ARGB) |
| **Risk** | HIGH — most complex per-frame struct. Contains union/anonymous structs in C header. |

#### 9. NV_ENC_LOCK_BITSTREAM

| Property | Value |
|---|---|
| **Status** | ❌ NOT DEFINED — function pointer exists at offset 144 but no struct, no delegate |
| **Purpose** | Locks the bitstream buffer to retrieve encoded H.264 data |
| **Input** | version, outputBitstream (handle), doNotWait (0=blocking in sync mode) |
| **Output** | `bitstreamBufferPtr` (pointer to encoded data), `bitstreamSizeInBytes`, `picType`, `picIdx` |
| **Native size** | UNKNOWN — must verify. Estimate: ~100-200 bytes. |
| **Fields needed** | version, doNotWait (0), outputBitstream, sliceType (OUT), picType (OUT), picIdx (OUT), bitstreamBufferPtr (OUT), bitstreamSizeInBytes (OUT), frameIdx (OUT), inputTimeStamp (OUT) |
| **Risk** | MEDIUM — straightforward struct but must verify field order. |

#### 10. NV_ENC_CREATE_BITSTREAM_BUFFER

| Property | Value |
|---|---|
| **Status** | ❌ NOT DEFINED — function pointer exists at offset 120 but no struct, no delegate |
| **Purpose** | Creates a bitstream buffer for NVENC to write encoded output |
| **Input** | version, size (0=default), |
| **Output** | `bitstreamBuffer` (handle for NvEncEncodePicture.outputBitstream) |
| **Native size** | UNKNOWN — must verify. |
| **Fields needed** | version, size (0), bitstreamBuffer (OUT) |
| **Risk** | LOW — simple struct. |

---

## Function Order (Synchronous Mode)

The exact call sequence for minimum synchronous H.264 encode:

```
1. NvEncodeAPICreateInstance          ← load function table
2. NvEncOpenEncodeSessionEx           ← open encoder on D3D11 device
3. NvEncGetEncodeGUIDCount + GUIDs    ← verify H.264 is supported
4. NvEncGetInputFormatCount + Formats ← verify ARGB (BGRA8) is supported
5. NvEncInitializeEncoder             ← configure encoder (codec, resolution, FPS)
6. NvEncCreateBitstreamBuffer         ← create output buffer
7. NvEncRegisterResource              ← register D3D11 texture
8. NvEncMapInputResource              ← map texture to NVENC input
9. NvEncEncodePicture                 ← encode one frame (blocks in sync mode)
10. NvEncLockBitstream                ← retrieve encoded data (blocks until ready)
11. Copy bitstream data to output    ← caller owns the copy
12. NvEncUnlockBitstream              ← release bitstream buffer
13. NvEncUnmapInputResource           ← unmap input
14. NvEncUnregisterResource           ← unregister texture
15. NvEncDestroyBitstreamBuffer        ← destroy output buffer
16. NvEncDestroyEncoder                ← destroy encoder session
```

### Steps Already Runtime-Proven (1-5, 7):

| Step | Status |
|---|---|
| 1. NvEncodeAPICreateInstance | ✅ RUNTIME-PROVEN (Phase 4) |
| 2. NvEncOpenEncodeSessionEx | ✅ RUNTIME-PROVEN (Phase 4) |
| 3. NvEncGetEncodeGUIDCount + GUIDs | ✅ RUNTIME-PROVEN (Phase 4) |
| 4. NvEncGetInputFormatCount + Formats | ✅ RUNTIME-PROVEN (Phase 4) |
| 5. NvEncInitializeEncoder | ✅ RUNTIME-PROVEN (Phase 4) |
| 6. NvEncCreateBitstreamBuffer | ❌ SPIKE-DEFINED (function pointer exists, struct NOT defined) |
| 7. NvEncRegisterResource | ✅ RUNTIME-PROVEN (Phase 4) |
| 8. NvEncMapInputResource | ❌ SPIKE-DEFINED (function pointer exists, struct NOT defined) |
| 9. NvEncEncodePicture | ❌ SPIKE-DEFINED (function pointer exists, struct NOT defined) |
| 10. NvEncLockBitstream | ❌ SPIKE-DEFINED (function pointer exists, struct NOT defined) |
| 11. Copy bitstream | ❌ NOT APPLICABLE (depends on step 10) |
| 12. NvEncUnlockBitstream | ❌ SPIKE-DEFINED (function pointer exists, no struct needed — just pass handle) |
| 13. NvEncUnmapInputResource | ❌ SPIKE-DEFINED (function pointer exists, no struct needed — just pass handle) |
| 14. NvEncUnregisterResource | ✅ SOURCE-PROVEN (delegate defined, called in Phase 4 cleanup) |
| 15. NvEncDestroyBitstreamBuffer | ❌ SPIKE-DEFINED (function pointer exists, no struct needed — just pass handle) |
| 16. NvEncDestroyEncoder | ✅ SOURCE-PROVEN (delegate defined, called in Phase 4 error paths) |

---

## Implementation Work Items

| # | Work Item | Estimate | Depends On | Risk |
|---|---|---|---|---|
| 1 | Define `NV_ENC_CREATE_BITSTREAM_BUFFER` struct + delegate | ~30 lines | — | LOW |
| 2 | Define `NV_ENC_MAP_INPUT_RESOURCE` struct + delegate | ~50 lines | — | MEDIUM |
| 3 | Define `NV_ENC_PIC_PARAMS` struct + delegate | ~200 lines | — | HIGH |
| 4 | Define `NV_ENC_LOCK_BITSTREAM` struct + delegate | ~80 lines | — | MEDIUM |
| 5 | Add delegates for UnlockBitstream, UnmapInputResource, DestroyBitstreamBuffer | ~30 lines | — | LOW |
| 6 | Define `NV_ENC_CONFIG` + `NV_ENC_CONFIG_H264` (optional — can pass NULL initially) | ~500 lines | — | HIGH |
| 7 | Write Phase 6 minimal encode test (register → map → encode → lock → copy → verify non-empty) | ~200 lines | Items 1-5 | MEDIUM |
| 8 | Run Phase 6 on OWNER's Windows + NVIDIA | OWNER action | Item 7 | — |
| **TOTAL** | **~1090 lines** | | | |

---

## SDK Version Verification

| Component | Expected SDK | Evidence |
|---|---|---|
| nvEncodeAPI64.dll | SDK 11 (FileDescription: "NVIDIA Video Encoder API, Version 11.0") | OWNER confirmed via Phase 4 runtime output |
| nvEncodeAPI.h (for struct definitions) | SDK 11 (or 13.1 — header has both layouts) | OWNER has SDK 13.1.15 zip in repo (`Video_Codec_Interface_13.1.15.zip`) |
| Struct versions | SDK 11 (FUNCTION_LIST ver=1, REGISTER_RESOURCE ver=3, INITIALIZE_PARAMS ver=5+bit31) | Runtime validated — all PASS with SDK 11 versions |
| NVENC driver API | 13.0 (NvEncodeAPIGetMaxSupportedVersion returned 0xD0 = 13.0) | Runtime confirmed |
| IMPORTANT | DLL is SDK 11 but driver supports API 13.0. Use SDK 11 struct layouts — they are backward-compatible. | SOURCE-PROVEN + RUNTIME-PROVEN |

---

## Unresolved Blockers

| # | Blocker | Severity | Required Action |
|---|---|---|---|
| B1 | Missing 5 NVENC structs (BitstreamBuffer, MapInputResource, PicParams, LockBitstream, Config) | HIGH | Define structs from nvEncodeAPI.h SDK 11 header. OWNER has SDK 13.1.15 — must verify SDK 11 vs 13 layout differences for these structs. |
| B2 | NV_ENC_PIC_PARAMS is the most complex struct — contains unions in C header | HIGH | C unions map to `StructLayout.Explicit` in C# — but `[MarshalAs]` is NOT supported in Explicit layout. Need workaround (manual field offset or byte array). |
| B3 | NV_ENC_CONFIG + NV_ENC_CONFIG_H264 are ~500 lines each in C header | MEDIUM | Can defer by passing NULL (use preset defaults). Needed only when custom rate control / GOP / B-frames are required. |
| B4 | No `IEncoderBackend` contract exists yet (GLM-3 is building it) | MEDIUM | Wait for GLM-3 to commit the encoder contract. Then implement NVENC as a concrete `IEncoderBackend`. |
