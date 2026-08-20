# P1-F NVENC API Map

> Source-level analysis of the NVIDIA NVENC encode path.
> All claims are SOURCE-PROVEN unless marked RUNTIME-UNKNOWN.
> D3D11 spike has NEVER been executed on Windows + NVIDIA hardware.

## Complete Encode Path

```
D3D11 ID3D11Texture2D (capture output)
        ↓
NvEncRegisterResource         ← registers texture as NVENC input
        ↓
NV_ENC_REGISTERED_PTR (opaque handle)
        ↓
NvEncMapInputResource         ← maps registered resource to NVENC input buffer
        ↓
NV_ENC_MAP_INPUT_RESOURCE (mapped handle)
        ↓
NvEncEncodePicture            ← submits frame for encoding
        ↓
NvEncLockBitstream            ← retrieves encoded bitstream
        ↓
Bitstream data (H.264 NAL units)
        ↓
NvEncUnlockBitstream          ← releases bitstream buffer
        ↓
NvEncUnmapInputResource       ← unmaps the input resource
        ↓
NvEncUnregisterResource       ← unregisters the texture
        ↓
Texture can now be released by the capture backend
```

## Operation Details

### 1. NvEncodeAPICreateInstance

| Property | Value |
|---|---|
| **Input** | `ref NV_ENCODE_API_FUNCTION_LIST` (version + reserved fields pre-filled) |
| **Output** | Function table populated with 38 function pointers (SDK 11) |
| **Ownership** | Caller owns the function list struct (stack or heap) |
| **Lifetime** | Valid until nvEncodeAPI64.dll is unloaded |
| **Threading** | Thread-safe — can be called once at startup |
| **Failure cases** | `NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY` (wrong API version), `NV_ENC_ERR_GENERIC` (struct size mismatch) |
| **Evidence** | SOURCE-PROVEN: `NvEncodeAPI.cs` line 508. Spike loads via P/Invoke. |
| **Runtime** | RUNTIME-UNKNOWN — spike never executed |

### 2. NvEncOpenEncodeSessionEx

| Property | Value |
|---|---|
| **Input** | `NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS` (version, deviceType=DIRECTX, device=D3D11 device pointer, apiVersion) |
| **Output** | `out IntPtr encoder` (opaque encoder handle) |
| **Ownership** | Caller owns the encoder handle — MUST call `NvEncDestroyEncoder` |
| **Lifetime** | Valid from open until `NvEncDestroyEncoder` |
| **Threading** | Encoder session is thread-safe per NVENC docs. Multiple sessions allowed. |
| **Failure cases** | `NV_ENC_ERR_NO_ENCODE_DEVICE` (no NVIDIA GPU), `NV_ENC_ERR_UNSUPPORTED_DEVICE` (deviceType mismatch), `NV_ENC_ERR_INVALID_DEVICE` (wrong D3D11 device flags) |
| **Evidence** | SOURCE-PROVEN: `Phase4_NVENCRegistration.cs` lines 67-89. deviceType=0x00 (DIRECTX), device=SpikeSharedContext.Device.NativePointer. |
| **Runtime** | RUNTIME-UNKNOWN |

### 3. NvEncInitializeEncoder

| Property | Value |
|---|---|
| **Input** | `ref NV_ENC_INITIALIZE_PARAMS` (version, encodeGUID=H264, presetGUID=DEFAULT, encodeWidth/Height, frameRateNum/Den, enablePTD=1) |
| **Output** | Encoder configured — no output value (status code only) |
| **Ownership** | Modifies the encoder session. No new resources owned. |
| **Lifetime** | Configuration persists until `NvEncDestroyEncoder` or `NvEncReconfigureEncoder` |
| **Threading** | Must be called before any `NvEncRegisterResource` or `NvEncEncodePicture` calls |
| **Failure cases** | `NV_ENC_ERR_INVALID_PARAM` (bad dimensions/framerate), `NV_ENC_ERR_UNSUPPORTED_PARAM` (unsupported preset) |
| **Evidence** | SOURCE-PROVEN: `Phase4_NVENCRegistration.cs` lines 217-268. encodeGUID=H264_GUID, presetGUID=DEFAULT_GUID, 60fps, encodeConfig=NULL (use preset defaults). |
| **Runtime** | RUNTIME-UNKNOWN |
| **CRITICAL** | Spike comment (line 218-221): "NVIDIA's NvEncoder sample calls NvEncInitializeEncoder BEFORE NvEncRegisterResource. Without this call, NvEncRegisterResource returns NV_ENC_ERR_DEVICE_NOT_EXIST." |

### 4. NvEncRegisterResource

| Property | Value |
|---|---|
| **Input** | `ref NV_ENC_REGISTER_RESOURCE` (version, resourceType=DIRECTX, width, height, resourceToRegister=ID3D11Texture2D*, bufferFormat=ARGB) |
| **Output** | `registeredResource` field populated with `NV_ENC_REGISTERED_PTR` (opaque handle) |
| **Ownership** | NVENC holds an internal reference to the D3D11 texture. Caller MUST NOT release the texture until `NvEncUnregisterResource` is called. |
| **Lifetime** | Registered resource is valid from registration until `NvEncUnregisterResource`. Can be mapped/unmapped multiple times. |
| **Threading** | Must be called on the same thread that opened the encoder session (or with proper synchronization). |
| **Failure cases** | `NV_ENC_ERR_RESOURCE_REGISTER_FAILED` (texture format mismatch, device mismatch, texture already registered), `NV_ENC_ERR_INVALID_PARAM` (bad width/height) |
| **Evidence** | SOURCE-PROVEN: `Phase4_NVENCRegistration.cs` lines 286-380. resourceType=0x00 (DIRECTX), bufferFormat=0x01000000 (ARGB/BGRA8). |
| **Runtime** | RUNTIME-UNKNOWN — **THIS IS THE V1 BLOCKER. If this fails, zero-copy D3D11→NVENC path is NOT possible.** |

### 5. NvEncMapInputResource

| Property | Value |
|---|---|
| **Input** | encoder handle + `NV_ENC_REGISTERED_PTR` (from RegisterResource) |
| **Output** | `NV_ENC_MAP_INPUT_RESOURCE` struct with mapped resource pointer |
| **Ownership** | NVENC holds a temporary mapping. Caller MUST call `NvEncUnmapInputResource` before unmapping. |
| **Lifetime** | Valid from map until unmap. Must unmap before encoding next frame (or use multiple registered resources). |
| **Threading** | Same thread as encoder session. |
| **Failure cases** | `NV_ENC_ERR_RESOURCE_NOT_REGISTERED`, `NV_ENC_ERR_INVALID_PARAM` |
| **Evidence** | NOT IMPLEMENTED in spike — function pointer exists in function table (offset 208) but spike does NOT call it. Delegate not defined. |
| **Runtime** | RUNTIME-UNKNOWN |

### 6. NvEncEncodePicture

| Property | Value |
|---|---|
| **Input** | `NV_ENC_PIC_PARAMS` (version, inputBuffer=mapped resource, outputBitstream=bitstream buffer handle, encodePicFlags, frameIdx, inputTimeStamp, inputDuration) |
| **Output** | Status code only — encoded data retrieved via `NvEncLockBitstream` |
| **Ownership** | NVENC takes ownership of the input buffer until `NvEncUnmapInputResource`. Bitstream buffer must be pre-created via `NvEncCreateBitstreamBuffer`. |
| **Lifetime** | Asynchronous if `enableEncodeAsync=1` — completion signaled via event. Synchronous if `enableEncodeAsync=0` — `NvEncLockBitstream` blocks until data ready. |
| **Threading** | Same thread as encoder session. Async mode allows concurrent encode + bitstream retrieval. |
| **Failure cases** | `NV_ENC_ERR_ENCODER_BUSY`, `NV_ENC_ERR_NEED_MORE_INPUT` (B-frames need lookahead), `NV_ENC_ERR_OUT_OF_MEMORY` |
| **Evidence** | NOT IMPLEMENTED in spike — function pointer exists (offset 136) but spike does NOT call it. `NV_ENC_PIC_PARAMS` struct NOT defined. |
| **Runtime** | RUNTIME-UNKNOWN |

### 7. NvEncLockBitstream

| Property | Value |
|---|---|
| **Input** | `NV_ENC_LOCK_BITSTREAM` (version, outputBitstream=bitstream buffer, doNotWait=0) |
| **Output** | `bitstreamBufferPtr` (pointer to encoded H.264 NAL data), `bitstreamSizeInBytes` |
| **Ownership** | NVENC owns the bitstream buffer. Caller MUST copy data before calling `NvEncUnlockBitstream`. |
| **Lifetime** | Locked from lock until unlock. Data invalidated after unlock. |
| **Threading** | Same thread as encoder session. |
| **Failure cases** | `NV_ENC_ERR_LOCK_BUSY` (data not ready in sync mode — shouldn't happen) |
| **Evidence** | NOT IMPLEMENTED in spike — function pointer exists (offset 144) but spike does NOT call it. `NV_ENC_LOCK_BITSTREAM` struct NOT defined. |
| **Runtime** | RUNTIME-UNKNOWN |

### 8. NvEncUnlockBitstream

| Property | Value |
|---|---|
| **Input** | encoder handle + bitstream buffer handle |
| **Output** | Status code — releases the bitstream buffer for reuse |
| **Ownership** | NVENC reclaims the bitstream buffer. |
| **Lifetime** | Buffer available for next `NvEncEncodePicture` after unlock. |
| **Threading** | Same thread. |
| **Evidence** | NOT IMPLEMENTED in spike — function pointer at offset 152. |

### 9. NvEncUnmapInputResource

| Property | Value |
|---|---|
| **Input** | encoder handle + `NV_ENC_MAP_INPUT_RESOURCE` handle |
| **Output** | Status code — releases the mapped resource |
| **Ownership** | NVENC releases its temporary mapping. The registered resource is still valid. |
| **Threading** | Same thread. |
| **Evidence** | NOT IMPLEMENTED in spike — function pointer at offset 216. |

### 10. NvEncUnregisterResource

| Property | Value |
|---|---|
| **Input** | encoder handle + `NV_ENC_REGISTERED_PTR` |
| **Output** | Status code — releases the registration |
| **Ownership** | NVENC releases its internal reference to the D3D11 texture. Texture can now be released by the caller. |
| **Threading** | Same thread. |
| **Evidence** | SOURCE-PROVEN: `NvEncodeAPI.cs` lines 484-486. Delegate defined. Spike does NOT call it yet (no cleanup path in Phase 4). |
| **Runtime** | RUNTIME-UNKNOWN |

### 11. NvEncDestroyEncoder

| Property | Value |
|---|---|
| **Input** | encoder handle |
| **Output** | Status code — destroys encoder session |
| **Ownership** | Releases ALL resources associated with the encoder session (registered resources, bitstream buffers, input buffers). |
| **Threading** | Must be the last call on the encoder handle. |
| **Evidence** | SOURCE-PROVEN: `NvEncodeAPI.cs` lines 488-489. Delegate defined. Spike calls it in error paths (e.g. Phase4 line 99). |
| **Runtime** | RUNTIME-UNKNOWN |

## API Coverage Summary

| API Function | Defined | Called in Spike | Struct Defined |
|---|---|---|---|
| NvEncodeAPICreateInstance | ✅ | ✅ Phase 4 | ✅ NV_ENCODE_API_FUNCTION_LIST |
| NvEncodeAPIGetMaxSupportedVersion | ✅ | ✅ Phase 4 | — |
| NvEncOpenEncodeSessionEx | ✅ | ✅ Phase 4 | ✅ NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS |
| NvEncGetEncodeGUIDCount | ✅ | ✅ Phase 4 | — |
| NvEncGetEncodeGUIDs | ✅ | ✅ Phase 4 | — |
| NvEncGetInputFormatCount | ✅ | ✅ Phase 4 | — |
| NvEncGetInputFormats | ✅ | ✅ Phase 4 | — |
| NvEncInitializeEncoder | ✅ | ✅ Phase 4 | ✅ NV_ENC_INITIALIZE_PARAMS |
| NvEncRegisterResource | ✅ | ✅ Phase 4 | ✅ NV_ENC_REGISTER_RESOURCE |
| NvEncUnregisterResource | ✅ | ❌ NOT CALLED | — |
| NvEncMapInputResource | ✅ (ptr at offset 208) | ❌ NOT CALLED | ❌ NV_ENC_MAP_INPUT_RESOURCE NOT DEFINED |
| NvEncUnmapInputResource | ✅ (ptr at offset 216) | ❌ NOT CALLED | — |
| NvEncEncodePicture | ✅ (ptr at offset 136) | ❌ NOT CALLED | ❌ NV_ENC_PIC_PARAMS NOT DEFINED |
| NvEncLockBitstream | ✅ (ptr at offset 144) | ❌ NOT CALLED | ❌ NV_ENC_LOCK_BITSTREAM NOT DEFINED |
| NvEncUnlockBitstream | ✅ (ptr at offset 152) | ❌ NOT CALLED | — |
| NvEncDestroyEncoder | ✅ | ✅ Phase 4 (error paths) | — |
| NvEncCreateBitstreamBuffer | ✅ (ptr at offset 120) | ❌ NOT CALLED | ❌ NOT DEFINED |
| NvEncDestroyBitstreamBuffer | ✅ (ptr at offset 128) | ❌ NOT CALLED | — |

### Coverage: 8 of 17 critical API functions are actually CALLED in the spike.

The remaining 9 (Map/Unmap/Encode/Lock/Unlock/Bitstream create+destroy) are defined as function pointers in the table but their delegates, structs, and call sites do NOT exist. **The spike proves device+codec+format+registration capability but does NOT prove the full encode pipeline.**
