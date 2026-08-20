# P1-F NVENC Phase 6 — Minimal H.264 Encode Result

> **STATUS: AWAITING OWNER RUN**
>
> This document will be filled in with actual runtime output after OWNER
> runs Phase 6 on Windows + NVIDIA hardware.
>
> All struct definitions are SOURCE-PROVEN. No runtime claims are made yet.

## Spike Code

File: `spikes/D3D11_NVENC_Spike/Phases/Phase6_MinimalEncode.cs`

### What was added:
- 4 new NVENC structs in `NvEncodeAPI.cs`:
  - `NV_ENC_CREATE_BITSTREAM_BUFFER` — bitstream buffer creation
  - `NV_ENC_MAP_INPUT_RESOURCE` — input resource mapping
  - `NV_ENC_PIC_PARAMS` — per-frame encode parameters
  - `NV_ENC_LOCK_BITSTREAM` — bitstream retrieval
- 7 new delegates in `NvEncodeAPI.cs`:
  - `NvEncCreateBitstreamBufferDelegate`
  - `NvEncDestroyBitstreamBufferDelegate`
  - `NvEncMapInputResourceDelegate`
  - `NvEncUnmapInputResourceDelegate`
  - `NvEncEncodePictureDelegate`
  - `NvEncLockBitstreamDelegate`
  - `NvEncUnlockBitstreamDelegate`
- 7 new properties in `NvEncFunctionTable` (marshalled in `TryLoad()`)
- `Phase6_MinimalEncode.cs` — full encode path implementation
- `Program.cs` updated to support `phase6` argument

### SDK Version Assumptions:
- SDK 11 struct layouts (OWNER's nvEncodeAPI64.dll is SDK 11)
- Struct versions: BitstreamBuffer ver=1, MapInputResource ver=4, PicParams ver=1, LockBitstream ver=1
- LayoutKind.Sequential with Pack=1
- Explicit padding fields for 8-byte alignment of pointer fields

### Struct Layout Notes:
- `NV_ENC_PIC_PARAMS` uses `codecPicHints_bitfield` (single uint32) instead of C bitfields
- Same approach as `NV_ENC_INITIALIZE_PARAMS.bitFields`
- Reserved array sizes are derived from SDK 11 header — MUST be verified at runtime
- If any NVENC call returns `NV_ENC_ERR_INVALID_PARAM`, the struct size is wrong

## How to Run

```cmd
cd spikes\D3D11_NVENC_Spike
dotnet build -c Debug -p:Platform=x64
:: Copy nvEncodeAPI64.dll to output directory if not already there
dotnet run -c Debug -p:Platform=x64 -- phase6
:: OR run all phases:
:: dotnet run -c Debug -p:Platform=x64
```

## Evidence Classification (Before Run)

| Step | Classification | Notes |
|---|---|---|
| Struct definitions | SOURCE-PROVEN | Defined in NvEncodeAPI.cs, layout matches SDK 11 comments |
| Delegate marshalling | SOURCE-PROVEN | GetDelegateForFunctionPointer called in TryLoad() |
| Phase 6 call sequence | SOURCE-PROVEN | Code follows the documented call sequence in P1-F-NVENC-CALL-SEQUENCE.md |
| NvEncCreateBitstreamBuffer | RUNTIME-UNKNOWN | Never called at runtime |
| NvEncMapInputResource | RUNTIME-UNKNOWN | Never called at runtime |
| NvEncEncodePicture | RUNTIME-UNKNOWN | Never called at runtime |
| NvEncLockBitstream | RUNTIME-UNKNOWN | Never called at runtime |
| NvEncUnlockBitstream | RUNTIME-UNKNOWN | Never called at runtime |
| NvEncUnmapInputResource | RUNTIME-UNKNOWN | Never called at runtime |
| NvEncDestroyBitstreamBuffer | RUNTIME-UNKNOWN | Never called at runtime |
| Bitstream non-empty | RUNTIME-UNKNOWN | Depends on all above succeeding |
| Annex-B start code | RUNTIME-UNKNOWN | Depends on bitstream being produced |
| Cleanup | RUNTIME-UNKNOWN | Depends on all above |

## Runtime Result (To Be Filled)

```
Hardware:          <<OWNER fills — e.g. NVIDIA GeForce GTX 1080 Ti>>
Driver version:    <<OWNER fills — e.g. 560.xx>>
NVENC API version: <<OWNER fills — e.g. 13.0>>
Input dimensions:  <<OWNER fills — e.g. 1680x1050>>
Input format:      <<OWNER fills — e.g. ARGB (BGRA8)>>
Encoder config:    <<OWNER fills — e.g. H.264, Default preset, sync mode, 60fps>>

NVENC operations:
  NvEncOpenEncodeSessionEx:    <<NV_ENC_SUCCESS / FAIL code>>
  NvEncInitializeEncoder:      <<NV_ENC_SUCCESS / FAIL code>>
  NvEncCreateBitstreamBuffer:  <<NV_ENC_SUCCESS / FAIL code>>
  NvEncRegisterResource:       <<NV_ENC_SUCCESS / FAIL code>>
  NvEncMapInputResource:       <<NV_ENC_SUCCESS / FAIL code>>
  NvEncEncodePicture:           <<NV_ENC_SUCCESS / FAIL code>>
  NvEncLockBitstream:           <<NV_ENC_SUCCESS / FAIL code>>

Bitstream bytes:    <<OWNER fills — e.g. 12345>>
First 32 bytes:     <<OWNER fills — e.g. 00 00 00 01 67 42 ...>>
Annex-B:            <<DETECTED / NOT detected>>
Cleanup:            <<ALL SUCCEEDED / FAILED at step X>>

Phase 6 verdict:    <<PASS / FAIL>>
Evidence:           <<RUNTIME-PROVEN / RUNTIME-UNKNOWN>>
```

## Struct Size Verification (To Be Filled)

```
NV_ENC_CREATE_BITSTREAM_BUFFER:  <<Marshal.SizeOf>> bytes (expected ~1456)
NV_ENC_MAP_INPUT_RESOURCE:       <<Marshal.SizeOf>> bytes (expected ~1488)
NV_ENC_PIC_PARAMS:               <<Marshal.SizeOf>> bytes (expected ~1560)
NV_ENC_LOCK_BITSTREAM:           <<Marshal.SizeOf>> bytes (expected ~1476)
```

If any struct size causes `NV_ENC_ERR_INVALID_PARAM`, the reserved array sizes
must be adjusted to match the actual SDK 11 header.

## Unresolved Blockers

1. **Struct layout verification** — the 4 new structs use estimated reserved
   array sizes. If any is wrong, the corresponding NVENC call will return
   `NV_ENC_ERR_INVALID_PARAM`. OWNER must verify sizes against `nvEncodeAPI.h`
   SDK 11 if this happens.

2. **`NV_ENC_PIC_PARAMS` complexity** — this struct contains C bitfields and
   codec-specific hints. We use a single `uint32` placeholder. If NVENC
   rejects it, the bitfield layout must be decoded from the SDK 11 header.

3. **Bitstream format** — NVENC may output AVC (length-prefixed) instead of
   Annex-B. The spike checks for Annex-B start code but does not fail if it's
   not found (just warns).

## Success Criterion

PASS means ONLY:

"One real D3D11 texture was successfully encoded by NVENC into a non-empty
H.264 bitstream on OWNER's NVIDIA hardware."
