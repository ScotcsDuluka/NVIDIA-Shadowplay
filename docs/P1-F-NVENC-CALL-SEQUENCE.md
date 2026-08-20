# P1-F NVENC Call Sequence

> Exact call sequence for minimum synchronous H.264 encode using NVENC.
>
> Every step is classified as:
> - **SOURCE-PROVEN** — struct + delegate defined in spike source code
> - **RUNTIME-PROVEN** — actually executed on Windows + NVIDIA GPU (GTX 1080 Ti)
> - **SPIKE-DEFINED** — function pointer exists in NVENC function table but struct/delegate NOT defined
> - **RUNTIME-UNKNOWN** — not yet executed at runtime

## Phase A: One-Time Setup (per encoder session)

```
┌─────────────────────────────────────────────────────────────────┐
│ Step 1: Load NVENC function table                               │
│ Classification: RUNTIME-PROVEN                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncodeAPIGetMaxSupportedVersion(out uint version)            │
│    → returns packed (major<<4)|minor = 0xD0 (13.0)             │
│    → status: NV_ENC_SUCCESS                                     │
│    → RUNTIME-PROVEN: Phase 4 output confirmed                  │
│                                                                 │
│  NvEncodeAPICreateInstance(ref NV_ENCODE_API_FUNCTION_LIST)    │
│    → version = NVENCAPI_STRUCT_VERSION(1) | 0x7<<28            │
│    → 38 function pointers populated (SDK 11 layout)             │
│    → status: NV_ENC_SUCCESS                                     │
│    → RUNTIME-PROVEN: Phase 4 output confirmed                  │
│                                                                 │
│  Lifetime: valid until DLL unloaded                             │
│  Ownership: caller owns function list struct                    │
│  Threading: thread-safe (call once at startup)                 │
│  Failure: NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY (wrong version)  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 2: Open encoder session on D3D11 device                    │
│ Classification: RUNTIME-PROVEN                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncOpenEncodeSessionEx(                                      │
│      ref NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS,                  │
│      out IntPtr encoder)                                        │
│    → params.version = NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER  │
│    → params.deviceType = NV_ENC_DEVICE_DIRECTX (0x00)           │
│    → params.device = D3D11Device.NativePointer                  │
│    → params.apiVersion = NVENCAPI_VERSION (0x0000000D = 13.0)  │
│    → status: NV_ENC_SUCCESS                                     │
│    → encoder handle: 0x0000019204eb5610                         │
│    → RUNTIME-PROVEN: Phase 4 output confirmed                  │
│                                                                 │
│  Lifetime: valid until NvEncDestroyEncoder                      │
│  Ownership: caller owns encoder handle — MUST destroy           │
│  Threading: encoder session is thread-safe                      │
│  Failure: NV_ENC_ERR_NO_ENCODE_DEVICE (no NVIDIA GPU),         │
│           NV_ENC_ERR_UNSUPPORTED_DEVICE (deviceType mismatch)    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 3: Verify H.264 codec support                              │
│ Classification: RUNTIME-PROVEN                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncGetEncodeGUIDCount(encoder, out int count)               │
│    → count: N codecs supported                                  │
│    → RUNTIME-PROVEN                                             │
│                                                                 │
│  NvEncGetEncodeGUIDs(encoder, Guid[], arraySize, out actual)   │
│    → returns array of codec GUIDs                               │
│    → verify NV_ENC_CODEC_H264_GUID is in the list              │
│    → RUNTIME-PROVEN: H.264 confirmed supported                 │
│                                                                 │
│  Optional: verify HEVC, AV1                                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 4: Verify ARGB (BGRA8) input format support                │
│ Classification: RUNTIME-PROVEN                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncGetInputFormatCount(encoder, H264_GUID, out int count)   │
│    → count: N input formats supported for H.264                 │
│    → RUNTIME-PROVEN                                             │
│                                                                 │
│  NvEncGetInputFormats(encoder, H264_GUID, int[], size, out)   │
│    → returns array of NV_ENC_BUFFER_FORMAT values              │
│    → verify NV_ENC_BUFFER_FORMAT_ARGB (0x01000000) is in list  │
│    → RUNTIME-PROVEN: ARGB confirmed supported                  │
│    → "zero-copy path possible"                                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 5: Initialize encoder                                      │
│ Classification: RUNTIME-PROVEN                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncInitializeEncoder(encoder, ref NV_ENC_INITIALIZE_PARAMS)  │
│    → params.version = MakeStructVersion(5) | (1u << 31)        │
│    → params.encodeGUID = NV_ENC_CODEC_H264_GUID                │
│    → params.presetGUID = NV_ENC_PRESET_DEFAULT_GUID            │
│    → params.encodeWidth = textureWidth (e.g. 1680)             │
│    → params.encodeHeight = textureHeight (e.g. 1050)           │
│    → params.darWidth = encodeWidth (square pixels)              │
│    → params.darHeight = encodeHeight                            │
│    → params.frameRateNum = 60 (or target FPS)                  │
│    → params.frameRateDen = 1                                    │
│    → params.enableEncodeAsync = 0 (synchronous)                 │
│    → params.enablePTD = 1 (presentation-time decision)         │
│    → params.encodeConfig = IntPtr.Zero (use preset defaults)   │
│    → params.maxEncodeWidth = encodeWidth                        │
│    → params.maxEncodeHeight = encodeHeight                      │
│    → status: NV_ENC_SUCCESS                                     │
│    → RUNTIME-PROVEN: "Encoder initialized (1680x1050 @ 60fps)"│
│                                                                 │
│  CRITICAL: NvEncInitializeEncoder MUST be called BEFORE        │
│  NvEncRegisterResource. Without it, RegisterResource fails     │
│  with NV_ENC_ERR_DEVICE_NOT_EXIST.                              │
│                                                                 │
│  CRITICAL: version field uses MakeStructVersion(5) | (1<<31).  │
│  The bit-31 flag is REQUIRED for SDK 11 compat. Without it,    │
│  NvEncInitializeEncoder fails with NV_ENC_ERR_GENERIC.         │
│                                                                 │
│  Lifetime: configuration persists until DestroyEncoder or      │
│  ReconfigureEncoder                                             │
│  Ownership: no new resources owned                              │
│  Threading: must be called before any Register/Encode calls    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 6: Create bitstream buffer                                 │
│ Classification: SPIKE-DEFINED (struct NOT defined)             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncCreateBitstreamBuffer(encoder, ref NV_ENC_CREATE_BSTREAM)│
│    → struct NOT defined in spike                                │
│    → function pointer at offset 120 in function table           │
│    → output: bitstreamBuffer handle                             │
│                                                                 │
│  Required struct fields:                                        │
│    version, size (0=default), bitstreamBuffer (OUT)             │
│                                                                 │
│  Struct size: UNKNOWN — verify against nvEncodeAPI.h SDK 11    │
│  Estimated: ~50-100 bytes                                      │
│                                                                 │
│  Lifetime: valid until NvEncDestroyBitstreamBuffer              │
│  Ownership: caller owns buffer — MUST destroy                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Phase B: Per-Frame Encode Loop (repeat for each frame)

```
┌─────────────────────────────────────────────────────────────────┐
│ Step 7: Register D3D11 texture with NVENC                      │
│ Classification: RUNTIME-PROVEN                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncRegisterResource(encoder, ref NV_ENC_REGISTER_RESOURCE)  │
│    → params.version = NV_ENC_REGISTER_RESOURCE_VER              │
│    → params.resourceType = NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX   │
│    → params.width = textureWidth                               │
│    → params.height = textureHeight                             │
│    → params.pitch = 0 (textures have implicit pitch)           │
│    → params.subResourceIndex = 0 (non-array texture)            │
│    → params.resourceToRegister = texture.NativePointer         │
│    → params.bufferFormat = NV_ENC_BUFFER_FORMAT_ARGB (0x0100..) │
│    → output: registeredResource (opaque handle)                 │
│    → status: NV_ENC_SUCCESS                                     │
│    → RUNTIME-PROVEN: Phase 4 confirmed registration succeeds   │
│                                                                 │
│  CRITICAL: texture must be on the SAME D3D11 device as the     │
│  encoder session. Cross-device registration will fail with      │
│  NV_ENC_ERR_RESOURCE_REGISTER_FAILED.                          │
│                                                                 │
│  CRITICAL: texture must NOT be released until after             │
│  NvEncUnregisterResource. NVENC holds an internal reference.    │
│                                                                 │
│  Lifetime: registered until NvEncUnregisterResource             │
│  Ownership: NVENC holds reference to texture — caller MUST NOT  │
│  release texture until after unregister.                        │
│  Threading: same thread as encoder session                      │
│  Failure: NV_ENC_ERR_RESOURCE_REGISTER_FAILED (format/device   │
│           mismatch), NV_ENC_ERR_INVALID_PARAM                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 8: Map input resource                                      │
│ Classification: SPIKE-DEFINED (struct NOT defined)             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncMapInputResource(encoder, ref NV_ENC_MAP_INPUT_RESOURCE)  │
│    → struct NOT defined in spike                                │
│    → function pointer at offset 208                              │
│    → input: registeredResource (from Step 7)                    │
│    → output: mappedInputResource (handle for Step 9)            │
│    → status: NV_ENC_SUCCESS (expected)                          │
│                                                                 │
│  Required struct fields:                                        │
│    version, subResourceIndex (0), inputResource (registered     │
│    handle), mappedInputResource (OUT), reserved1[], reserved2[]│
│                                                                 │
│  Struct size: UNKNOWN — verify against nvEncodeAPI.h SDK 11    │
│  Estimated: ~1500 bytes (similar to REGISTER_RESOURCE)          │
│                                                                 │
│  Lifetime: valid until NvEncUnmapInputResource                  │
│  Ownership: NVENC holds temporary mapping                        │
│  Threading: same thread                                         │
│  Failure: NV_ENC_ERR_RESOURCE_NOT_REGISTERED,                   │
│           NV_ENC_ERR_INVALID_PARAM                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 9: Encode picture                                          │
│ Classification: SPIKE-DEFINED (struct NOT defined)               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncEncodePicture(encoder, ref NV_ENC_PIC_PARAMS)             │
│    → struct NOT defined in spike                                │
│    → function pointer at offset 136                              │
│    → input:                                                     │
│      version                                                    │
│      inputWidth = textureWidth                                  │
│      inputHeight = textureHeight                                │
│      inputPitch = 0                                             │
│      encodePicFlags = 0 (normal encode)                         │
│      frameIdx = frameIndex (0, 1, 2, ...)                      │
│      inputTimeStamp = frame.Metadata.PresentationTimestamp      │
│      inputDuration = 0                                          │
│      outputBitstream = bitstreamBuffer (from Step 6)             │
│      inputBuffer = mappedInputResource (from Step 8)            │
│      bufferFmt = NV_ENC_BUFFER_FORMAT_ARGB (0x01000000)        │
│    → status: NV_ENC_SUCCESS (expected)                          │
│                                                                 │
│  In synchronous mode (enableEncodeAsync=0):                     │
│    NvEncEncodePicture returns after the frame is submitted.     │
│    The encoded data is NOT yet ready — must call                │
│    NvEncLockBitstream to retrieve it (Step 10).                 │
│                                                                 │
│  Struct size: UNKNOWN — HIGH RISK (most complex struct)         │
│  Contains union/anonymous structs in C header.                 │
│  Must verify against nvEncodeAPI.h SDK 11.                      │
│  Estimated: ~200-400 bytes.                                     │
│                                                                 │
│  Lifetime: NVENC owns input buffer until Unmap.                 │
│  Ownership: bitstream buffer receives output.                   │
│  Threading: same thread                                         │
│  Failure: NV_ENC_ERR_ENCODER_BUSY,                             │
│           NV_ENC_ERR_NEED_MORE_INPUT (B-frame lookahead),      │
│           NV_ENC_ERR_OUT_OF_MEMORY                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 10: Lock bitstream (retrieve encoded data)                 │
│ Classification: SPIKE-DEFINED (struct NOT defined)              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncLockBitstream(encoder, ref NV_ENC_LOCK_BITSTREAM)         │
│    → struct NOT defined in spike                                │
│    → function pointer at offset 144                              │
│    → input:                                                     │
│      version                                                    │
│      doNotWait = 0 (blocking — wait for encode to complete)     │
│      outputBitstream = bitstreamBuffer (from Step 6)             │
│    → output:                                                    │
│      bitstreamBufferPtr (pointer to encoded H.264 NAL data)    │
│      bitstreamSizeInBytes (size of encoded data)                │
│      picType (frame type: I/P/B)                                │
│      picIdx (frame index)                                       │
│      inputTimeStamp (echoes the timestamp from Step 9)           │
│    → status: NV_ENC_SUCCESS (expected)                          │
│                                                                 │
│  In synchronous mode: this call BLOCKS until encoding is        │
│  complete. The thread will wait for the NVENC hardware to       │
│  finish.                                                        │
│                                                                 │
│  Struct size: UNKNOWN — verify against nvEncodeAPI.h SDK 11    │
│  Estimated: ~100-200 bytes.                                     │
│                                                                 │
│  Lifetime: data valid until NvEncUnlockBitstream                │
│  Ownership: NVENC owns the buffer — caller MUST copy data       │
│  before calling Unlock.                                         │
│  Threading: same thread                                         │
│  Failure: NV_ENC_ERR_LOCK_BUSY (shouldn't happen in sync mode)│
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 11: Copy bitstream data to output                         │
│ Classification: NOT APPLICABLE (caller code, not NVENC API)     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  // Copy encoded H.264 NAL data from NVENC's buffer            │
│  byte[] encodedData = new byte[bitstreamSizeInBytes];           │
│  Marshal.Copy(bitstreamBufferPtr, encodedData, 0, size);        │
│                                                                 │
│  // Verify non-empty                                            │
│  if (encodedData.Length == 0)                                   │
│      throw new Exception("NVENC returned empty bitstream");     │
│                                                                 │
│  // encodedData now contains H.264 NAL units ready for          │
│  // muxing into an MP4 container.                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 12: Unlock bitstream                                       │
│ Classification: SPIKE-DEFINED (function pointer exists)         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncUnlockBitstream(encoder, IntPtr bitstreamBuffer)          │
│    → function pointer at offset 152                              │
│    → input: bitstreamBuffer handle (from Step 6)                │
│    → status: NV_ENC_SUCCESS (expected)                          │
│    → no struct needed — just pass the handle                    │
│                                                                 │
│  After unlock: bitstream buffer available for next encode.      │
│  Data pointer from Step 10 is INVALIDATED.                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 13: Unmap input resource                                   │
│ Classification: SPIKE-DEFINED (function pointer exists)         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncUnmapInputResource(encoder, IntPtr mappedInputResource)  │
│    → function pointer at offset 216                              │
│    → input: mappedInputResource handle (from Step 8)             │
│    → status: NV_ENC_SUCCESS (expected)                          │
│    → no struct needed — just pass the handle                    │
│                                                                 │
│  After unmap: registered resource is still valid.               │
│  Can re-map + encode again (for texture pool reuse).            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 14: Unregister resource                                    │
│ Classification: SOURCE-PROVEN (delegate defined)                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncUnregisterResource(encoder, IntPtr registeredResource)    │
│    → delegate defined at NvEncodeAPI.cs line 485                 │
│    → function pointer at offset 256                              │
│    → input: registeredResource handle (from Step 7)             │
│    → status: NV_ENC_SUCCESS (expected)                          │
│    → RUNTIME-PROVEN: Phase 4 confirmed "Resource unregistered" │
│                                                                 │
│  After unregister: NVENC releases reference to D3D11 texture.  │
│  Texture can now be released by the caller.                     │
│                                                                 │
│  For texture pool: skip this step if reusing the same texture   │
│  for the next frame. Keep the resource registered.              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Phase C: Cleanup (at encoder shutdown)

```
┌─────────────────────────────────────────────────────────────────┐
│ Step 15: Destroy bitstream buffer                               │
│ Classification: SPIKE-DEFINED (function pointer exists)         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncDestroyBitstreamBuffer(encoder, IntPtr bitstreamBuffer)   │
│    → function pointer at offset 128                              │
│    → input: bitstreamBuffer handle (from Step 6)                │
│    → status: NV_ENC_SUCCESS (expected)                          │
│    → no struct needed — just pass the handle                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ Step 16: Destroy encoder                                        │
│ Classification: SOURCE-PROVEN + RUNTIME-PROVEN                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NvEncDestroyEncoder(encoder)                                   │
│    → delegate defined at NvEncodeAPI.cs line 489                 │
│    → function pointer at offset 224                              │
│    → input: encoder handle (from Step 2)                        │
│    → status: NV_ENC_SUCCESS (expected)                          │
│    → RUNTIME-PROVEN: Phase 4 confirmed "Encoder destroyed"     │
│                                                                 │
│  After destroy: ALL resources associated with the encoder       │
│  session are released (registered resources, bitstream          │
│  buffers, etc.).                                                │
│                                                                 │
│  This MUST be the last call on the encoder handle.              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Classification Summary

| Step | API Function | Classification |
|---|---|---|
| 1 | NvEncodeAPICreateInstance | ✅ RUNTIME-PROVEN |
| 1 | NvEncodeAPIGetMaxSupportedVersion | ✅ RUNTIME-PROVEN |
| 2 | NvEncOpenEncodeSessionEx | ✅ RUNTIME-PROVEN |
| 3 | NvEncGetEncodeGUIDCount | ✅ RUNTIME-PROVEN |
| 3 | NvEncGetEncodeGUIDs | ✅ RUNTIME-PROVEN |
| 4 | NvEncGetInputFormatCount | ✅ RUNTIME-PROVEN |
| 4 | NvEncGetInputFormats | ✅ RUNTIME-PROVEN |
| 5 | NvEncInitializeEncoder | ✅ RUNTIME-PROVEN |
| 6 | NvEncCreateBitstreamBuffer | ❌ SPIKE-DEFINED (struct missing) |
| 7 | NvEncRegisterResource | ✅ RUNTIME-PROVEN |
| 8 | NvEncMapInputResource | ❌ SPIKE-DEFINED (struct missing) |
| 9 | NvEncEncodePicture | ❌ SPIKE-DEFINED (struct missing) |
| 10 | NvEncLockBitstream | ❌ SPIKE-DEFINED (struct missing) |
| 11 | Copy bitstream | N/A (caller code) |
| 12 | NvEncUnlockBitstream | ❌ SPIKE-DEFINED (delegate missing) |
| 13 | NvEncUnmapInputResource | ❌ SPIKE-DEFINED (delegate missing) |
| 14 | NvEncUnregisterResource | ✅ SOURCE-PROVEN + RUNTIME-PROVEN |
| 15 | NvEncDestroyBitstreamBuffer | ❌ SPIKE-DEFINED (delegate missing) |
| 16 | NvEncDestroyEncoder | ✅ SOURCE-PROVEN + RUNTIME-PROVEN |

### Totals:
- **RUNTIME-PROVEN**: 10 of 17 functions (59%)
- **SOURCE-PROVEN** (defined but not runtime-validated): 2 (12%)
- **SPIKE-DEFINED** (function pointer exists but struct/delegate missing): 5 (29%)
- **NOT APPLICABLE**: 1 (copy step)

### Remaining work to complete the encode pipeline:
1. Define 4 missing structs: `NV_ENC_CREATE_BITSTREAM_BUFFER`, `NV_ENC_MAP_INPUT_RESOURCE`, `NV_ENC_PIC_PARAMS`, `NV_ENC_LOCK_BITSTREAM`
2. Add 3 missing delegates: `NvEncUnlockBitstream`, `NvEncUnmapInputResource`, `NvEncDestroyBitstreamBuffer`
3. Write Phase 6 minimal encode test
4. Run Phase 6 on OWNER's Windows + NVIDIA
