# P1-F NVENC Contract Gap Analysis

> Comparing actual NVENC requirements against the current IVideoFrame architecture.
> All claims are SOURCE-PROVEN unless marked RUNTIME-UNKNOWN.

## The Problem

The Foundation `IVideoFrame` interface (frozen @ 82d792ab) defines:

```vbnet
Public Interface IVideoFrame
    Inherits IDisposable
    ReadOnly Property Origin As VideoFrameOrigin        ' CpuMemory | GpuD3D11Texture
    ReadOnly Property PixelFormat As VideoPixelFormat   ' Bgra8 | Rgba8 | Nv12 | Unknown
    ReadOnly Property Dimensions As VideoFrameDimensions ' Width + Height
    ReadOnly Property Diagnostics As FrameDiagnostics    ' Sequence + CaptureTimeTicks + PTS
End Interface
```

**What NVENC actually needs from a video frame:**

| NVENC Requirement | IVideoFrame Has It? | Gap |
|---|---|---|
| D3D11 texture pointer (`ID3D11Texture2D*`) | ❌ NO | **CRITICAL** — NVENC's `NvEncRegisterResource` requires `resourceToRegister` = raw `ID3D11Texture2D*` pointer. `IVideoFrame` does not expose any native pointer. |
| Texture width/height | ✅ YES (via `Dimensions`) | — |
| Texture pixel format (BGRA8) | ✅ YES (via `PixelFormat = Bgra8`) | — |
| Frame timestamp | ✅ YES (via `Diagnostics.PresentationTimestampTicks`) | — |
| Texture ownership (can NVENC hold it?) | ❌ NO | `IVideoFrame.Dispose()` releases the frame, but NVENC may still be using it (async encode). Need a way to defer Dispose until NVENC releases the resource. |
| Adapter LUID (which GPU?) | ❌ NO | NVENC requires the D3D11 texture to be on the same D3D11 device as the NVENC encoder session. `IVideoFrame` does not carry device/adapter identity. |
| D3D11 device pointer (for NVENC session) | ❌ NO | NVENC's `NvEncOpenEncodeSessionEx` needs the `ID3D11Device*`. The frame does not carry it — and shouldn't (it's a frame, not a device). But the encoder needs it from somewhere. |

## Gap #1: No Native Texture Pointer (CRITICAL)

**Why NVENC needs it:**
`NvEncRegisterResource` takes `resourceToRegister` as `void*` — this must be the `ID3D11Texture2D*` native pointer. Without it, NVENC cannot register the texture.

**Current state:**
- `DdagrabFrame.vb` (placeholder) has NO texture field — `Dispose()` just increments a counter.
- `FakeVideoFrame` (test) has NO GPU resources.
- `IVideoFrame.Origin = GpuD3D11Texture` tells us the data IS on GPU, but provides no way to access it.

**Possible solutions:**

| Solution | Description | Pros | Cons | Modifies Foundation? |
|---|---|---|---|---|
| A. `IGpuVideoFrame` extends `IVideoFrame` | New interface: `IGpuVideoFrame : IVideoFrame` + `ReadOnly Property NativeTexturePointer As IntPtr` | Foundation untouched. Clean separation. Type-safe. | Encoder must cast `IVideoFrame → IGpuVideoFrame`. May fail if frame is CPU. | ❌ NO |
| B. Add pointer to `FrameDiagnostics` struct | Add `NativeResourcePointer As IntPtr` to `FrameDiagnostics` | Single struct carries everything. | `FrameDiagnostics` is a `Structure` in Foundation — **FROZEN, cannot modify**. | ✅ YES (BLOCKED) |
| C. Cast `IVideoFrame` to concrete `DdagrabFrame` | Encoder knows the concrete type and casts. | Simple. | Couples encoder to capture backend — violates ARCHITECTURE.md Rule 2. | ❌ NO |
| D. Resource provider / registry | Backend provides a `IResourceProvider` that the encoder calls to get the pointer. | Fully decoupled. | Over-engineered for a single pointer. | ❌ NO |

**Recommended: Solution A — `IGpuVideoFrame`**

```vbnet
' NEW file: CaptureEngine.Video/Contract/IGpuVideoFrame.vb
' Does NOT modify IVideoFrame. Does NOT modify Foundation structs.
' New interface that extends IVideoFrame for GPU-resident frames.

Namespace CaptureEngine.Video
    Public Interface IGpuVideoFrame
        Inherits IVideoFrame

        ''' <summary>Raw ID3D11Texture2D* pointer. Valid until Dispose().</summary>
        ReadOnly Property NativeTexturePointer As IntPtr

        ''' <summary>The D3D11 device that owns this texture.</summary>
        ReadOnly Property DevicePointer As IntPtr
    End Interface
End Namespace
```

**Rationale:**
- Foundation stays frozen — `IGpuVideoFrame` is a NEW file, not a modification.
- `IVideoFrame` stays unchanged — CPU frames still implement it directly.
- GPU frames (DdagrabFrame) implement `IGpuVideoFrame` (which includes `IVideoFrame`).
- Encoder accepts `IGpuVideoFrame` — if the frame doesn't implement it, encoder rejects with a clear error.
- `DevicePointer` allows the encoder to verify the texture is on the correct D3D11 device (same as NVENC session).

## Gap #2: Texture Lifetime vs NVENC Resource Registration

**The problem:**
```
Timeline:
  t0: Capture backend creates D3D11 texture (CopyResource from DXGI)
  t1: Backend pushes IVideoFrame to sink
  t2: Encoder dequeues frame, calls NvEncRegisterResource
  t3: Encoder calls NvEncMapInputResource
  t4: Encoder calls NvEncEncodePicture (async — NVENC may still be using the texture)
  t5: Frame's Dispose() is called by the sink (bounded queue eviction)
  t6: NVENC tries to read the texture — CRASH (texture was released at t5)
```

**NVENC semantics (from API Map):**
- After `NvEncRegisterResource`: NVENC holds a reference to the texture.
- After `NvEncUnregisterResource`: NVENC releases the reference — texture can be freed.
- `NvEncEncodePicture` is potentially ASYNCHRONOUS — NVENC may not have finished encoding when the call returns.

**Required lifecycle:**
```
Capture creates texture
  ↓
Frame wraps texture pointer
  ↓
Encoder registers texture (NvEncRegisterResource)
  ↓
Encoder maps (NvEncMapInputResource)
  ↓
Encoder encodes (NvEncEncodePicture)
  ↓
Encoder waits for completion (NvEncLockBitstream — blocks until data ready in sync mode)
  ↓
Encoder unlocks bitstream (NvEncUnlockBitstream)
  ↓
Encoder unmaps (NvEncUnmapInputResource)
  ↓
Encoder unregisters (NvEncUnregisterResource)
  ↓
Frame.Dispose() releases texture (ID3D11Texture2D.Release())
```

**Key insight:** In synchronous mode (`enableEncodeAsync=0`), `NvEncLockBitstream` blocks until encoding is complete. After `UnlockBitstream`, the texture is safe to release. The full register→map→encode→lock→unlock→unmap→unregister sequence must complete BEFORE `IVideoFrame.Dispose()` is called.

**Impact on BoundedVideoFrameSink:**
The bounded queue must NOT dispose frames while they are being encoded. Currently, the sink disposes evicted frames immediately. If the encoder hasn't finished with a frame yet, this will crash NVENC.

**Solutions:**

| Solution | Description | Impact |
|---|---|---|
| A. Encoder holds a reference to the frame | Encoder calls `frame.AddRef()` before registering, `frame.Release()` after unregistering. Frame is not disposed until refcount = 0. | Requires `IVideoFrame` to support refcounting — **modifies Foundation (BLOCKED)**. |
| B. Encoder copies the frame before encoding | Encoder calls `CopyResource` to its own staging texture, then disposes the input frame. NVENC registers the encoder's texture, not the capture backend's. | Adds one GPU copy per frame (same as current D3D11 spike pattern). Safe — no lifetime coupling. |
| C. Bounded queue capacity ≥ max encoder latency | If the queue never evicts a frame while the encoder is using it, no crash. Capacity must be ≥ (encoder pipeline depth + 1). | Fragile — depends on encoder latency being bounded. |
| D. Encoder-owned frame pool | Encoder pre-allocates a pool of textures. For each input frame, it copies to a pool texture and registers that. Input frame is disposed immediately after copy. | Best long-term solution — decouples frame lifetime from NVENC entirely. Pool size = encoder pipeline depth. |

**Recommended: Solution D (encoder-owned texture pool)** for production. **Solution B (copy)** for initial spike.

## Gap #3: No Adapter LUID / Device Identity

**Why NVENC needs it:**
NVENC's `NvEncOpenEncodeSessionEx` takes a D3D11 device pointer. The texture registered via `NvEncRegisterResource` MUST be on the same D3D11 device. If the capture backend creates a D3D11 device on Adapter A, and the encoder opens a session on Adapter B, `NvEncRegisterResource` will fail with `NV_ENC_ERR_RESOURCE_REGISTER_FAILED`.

**Current state:**
- `IVideoFrame` carries no device identity.
- `IVideoBackendContext` (Foundation) has `BackendKind` (enum: Ddagrab/GfxCapture) but no LUID or device pointer.

**Solutions:**

| Solution | Description | Modifies Foundation? |
|---|---|---|
| A. `IGpuVideoFrame.DevicePointer` | Frame carries the D3D11 device pointer it was created with. Encoder verifies match before registering. | ❌ NO (new interface) |
| B. Encoder creates its own D3D11 device on the same adapter | Encoder enumerates NVIDIA adapters (same as Phase 1), creates its own device, opens NVENC session. Textures are shared via DXGI keyed mutex or shared handles. | Complex — cross-device texture sharing. |
| C. Encoder receives D3D11 device from the pipeline | The pipeline (or backend) provides the D3D11 device to the encoder via a context object. | Clean but requires a new context type. |

**Recommended: Solution A (frame carries device pointer via `IGpuVideoFrame`).**

## Gap #4: Missing NVENC Structs in Spike

The spike defines structs for:
- `NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS` ✅
- `NV_ENC_REGISTER_RESOURCE` ✅
- `NV_ENC_INITIALIZE_PARAMS` ✅
- `NV_ENCODE_API_FUNCTION_LIST` ✅

The spike does NOT define structs for:
- `NV_ENC_MAP_INPUT_RESOURCE` ❌
- `NV_ENC_PIC_PARAMS` ❌ (most complex struct — ~300+ fields)
- `NV_ENC_LOCK_BITSTREAM` ❌
- `NV_ENC_CREATE_INPUT_BUFFER` ❌ (not needed for D3D11 path — RegisterResource replaces this)
- `NV_ENC_CONFIG` ❌ (codec-specific configuration — H.264 params, rate control, etc.)

**Impact:** The spike cannot complete the encode path beyond Phase 4 (registration). Phases 5 (benchmark) would need these structs to actually encode frames.

**Estimate:** Defining `NV_ENC_PIC_PARAMS` + `NV_ENC_CONFIG` + `NV_ENC_LOCK_BITSTREAM` is ~500-800 lines of P/Invoke declarations. This is the bulk of the remaining spike work.

## Summary: Minimum Interface a Future NVENC Backend Needs From a Video Frame

| # | Information | Why NVENC Needs It | Current IVideoFrame Has It? | Recommended Solution |
|---|---|---|---|---|
| 1 | **Native texture pointer** (`ID3D11Texture2D*`) | `NvEncRegisterResource.resourceToRegister` | ❌ NO | `IGpuVideoFrame.NativeTexturePointer As IntPtr` |
| 2 | **D3D11 device pointer** (for adapter match verification) | Verify texture is on the same device as NVENC encoder session | ❌ NO | `IGpuVideoFrame.DevicePointer As IntPtr` |
| 3 | **Width + Height** | `NvEncRegisterResource.width/height` | ✅ YES (`Dimensions`) | — |
| 4 | **Pixel format** (BGRA8 = ARGB in NVENC terms) | `NvEncRegisterResource.bufferFormat` | ✅ YES (`PixelFormat = Bgra8`) | — |
| 5 | **Presentation timestamp** | `NvEncEncodePicture.inputTimeStamp` (optional — NVENC can auto-generate) | ✅ YES (`Diagnostics.PresentationTimestampTicks`) | — |
| 6 | **Texture lifetime guarantee** (texture must not be released until NVENC unregisters) | NVENC holds reference to texture between Register and Unregister | ❌ NO (frame can be disposed by sink at any time) | Encoder-owned texture pool (copy pattern) OR extended frame lifetime contract |
