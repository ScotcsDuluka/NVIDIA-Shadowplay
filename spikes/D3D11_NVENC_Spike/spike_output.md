============================================================
 Phase 1 — D3D11 Device Test
============================================================

[1.1] Enumerating DXGI adapters...
  Adapter [0] NVIDIA GeForce GTX 1080 Ti (NVIDIA:0x10de:0x1b06) LUID=(0000c820,00000000) Video=11107MB Sys=0MB Shared=8109MB
  Adapter [1] NVIDIA GeForce GTX 1080 Ti (NVIDIA:0x10de:0x1b06) LUID=(00017965,00000000) Video=11107MB Sys=0MB Shared=8109MB
  Adapter [2] Microsoft Basic Render Driver (Microsoft (WARP):0x1414:0x008c) LUID=(0000e1d8,00000000) Video=0MB Sys=0MB Shared=8109MB

[1.2] Selected NVIDIA adapter #0: NVIDIA GeForce GTX 1080 Ti
       VendorId:  0x10de (NVIDIA)
       DeviceId:  0x1b06
       LUID:      (0000c820,00000000)
       Memory:    Video=11107MB Sys=0MB Shared=8109MB

[1.3] Creating D3D11 device on NVIDIA adapter...
  Device created. Feature level: Level_11_1
  Device pointer: 0x0000017b02654bd0
  Multithread protection: ENABLED (required for VideoSupport + Desktop Duplication)

[1.4] Verifying D3D11 device adapter LUID matches selected NVIDIA adapter...
  Device's parent adapter: NVIDIA GeForce GTX 1080 Ti
    VendorId:  0x10de
    DeviceId:  0x1b06
    LUID:      (0000c820,00000000)
  PASS: LUID matches.

============================================================
 Phase 1 RESULT
============================================================
  D3D11_DEVICE_OK
  Adapter=LUID(0000c820,00000000)
  GPU=NVIDIA GeForce GTX 1080 Ti
  VendorId=0x10de
  DeviceId=0x1b06
  FeatureLevel=Level_11_1
  DedicatedVideoMemory=11107MB
  DevicePointer=0x0000017b02654bd0
============================================================

============================================================
 Phase 2 — Desktop Duplication Test (1000 frames)
============================================================

[2.1] Enumerating outputs on NVIDIA adapter...
  Output 0: \\.\DISPLAY1  (1680x1050)

[2.2] Creating IDXGIOutputDuplication...
  DuplicateOutput created. ModeDescription: 1680x1050@75.02Hz
  Format: B8G8R8A8_UNorm
  Staging texture created: 1680x1050 BGRA8
  Staging texture pointer: 0x0000017b028f6020

[2.3] Capturing 1000 frames...
    [ 100/1000] FPS=76.4 avg=0.04ms p95=0.05ms WT=450419 AL=0
    [ 200/1000] FPS=75.4 avg=0.03ms p95=0.05ms WT=933943 AL=0
    [ 300/1000] FPS=75.2 avg=0.03ms p95=0.04ms WT=1395864 AL=0
    [ 400/1000] FPS=74.6 avg=0.03ms p95=0.04ms WT=1870872 AL=0
    [ 500/1000] FPS=74.6 avg=0.03ms p95=0.04ms WT=2340388 AL=0
    [ 600/1000] FPS=74.3 avg=0.03ms p95=0.04ms WT=2782482 AL=0
    [ 700/1000] FPS=74.4 avg=0.03ms p95=0.04ms WT=3269960 AL=0
    [ 800/1000] FPS=74.2 avg=0.03ms p95=0.04ms WT=3775868 AL=0
    [ 900/1000] FPS=74.3 avg=0.03ms p95=0.04ms WT=4220007 AL=0
    [1000/1000] FPS=74.2 avg=0.03ms p95=0.04ms WT=4717057 AL=0

============================================================
 Phase 2 RESULT
============================================================
--- Capture loop ---
  Total time:           13478.3 ms
  Frames acquired:      1000
  Frames dropped:       0
  Achieved FPS:         74.19
  WaitTimeout count:    4717057
  AccessLost count:     0
  Other errors:         0
  Acquire latency:
    min/avg/max:        0.011 / 0.026 / 1.306 ms
    p50/p95/p99:        0.022 / 0.040 / 0.059 ms
============================================================

  Phase 2: PASS

============================================================
 Phase 3 — Texture Ownership Test
============================================================

[3.1] Querying texture description...
  Texture pointer:   0x0000017b028f6020
  Texture type:      ID3D11Texture2D
  Width:             1680
  Height:            1050
  MipLevels:         1
  ArraySize:         1
  Format:            B8G8R8A8_UNorm
  SampleDescription: count=1, quality=0
  Usage:             Default
  BindFlags:         ShaderResource
  CPUAccessFlags:    None
  MiscFlags:         None

[3.2] Verifying format...
  PASS: Format is BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM).

[3.3] Verifying dimensions match desktop...
  Desktop:  1680x1050
  Texture:  1680x1050
  PASS: Dimensions match.

[3.4] Verifying resource usage...
  Usage = Default:           True
  Usage = Staging:           False
  BindFlags.ShaderResource:  True
  BindFlags.RenderTarget:    False
  CPUAccessFlags (any):      False
  PASS: Texture is GPU-resident (Default usage, no CPU access).

[3.5] Verifying texture's parent device matches Phase 1 device...
  Phase 1 device pointer: 0x0000017b02654bd0
  Texture's parent device: 0x0000017b02654bd0
  PASS: Texture lives on the same D3D11 device as Phase 1.

[3.6] Verifying ArraySize...
  ArraySize = 1 (1 = single texture, >1 = texture array)
  PASS: Single texture (ArraySize=1) — suitable for direct NVENC registration.

============================================================
 Phase 3 RESULT
============================================================
  Texture: ID3D11Texture2D @ 0x0000017b028f6020
  Format:  BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM)
  Dims:    1680x1050
  Device:  Same (0x0000017b02654bd0)
  Usage:   Default
  BindFlags: ShaderResource
  CPUAccess: None (None = GPU-resident)
  ArraySize: 1
============================================================

  Phase 3: PASS — texture is GPU-resident BGRA8 on the same device as Phase 1.

============================================================
 Phase 4 — NVENC Registration Spike
============================================================

[4.1] Loading NVENC function table...
  NVENC max supported API: major=13, minor=0 (packed=0x000000D0)
  Spike requests API:     major=13, minor=0 (NVENCAPI_VERSION=0x0000000D)
  Function table version: 0x7001000D (struct size=2552 bytes)
  PASS: NVENC function table loaded.

[4.2] Opening NVENC encode session on D3D11 device...
  PASS: Encode session opened. Encoder handle: 0x0000017b06d6efd0

[4.3] Enumerating supported codecs...
  NVENC reports 2 supported codecs.
  Supported codecs:
    H.264: 6bc82762-4e63-4ca4-aa85-1e50f321f6bf
    HEVC: 790cdc88-4522-4d7b-9425-bda9975f7603
  PASS: H.264 codec is supported.

[4.4] Verifying ARGB (BGRA8) input format support for H.264...
  H.264 supports 9 input formats.
  H.264 input formats:
    [0] NV12
    [1] YV12
    [2] IYUV
    [3] YUV444
    [4] ARGB (BGRA8)
    [5] ABGR
    [6] AYUV
    [7] ARGB10
    [8] ABGR10
  PASS: ARGB (BGRA8) is supported — zero-copy path possible.

[4.4a] CLR struct layout diagnostic:
  NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS:
    Marshal.SizeOf = 1552 bytes
    version                                  offset    0  (UInt32)
    deviceType                               offset    4  (Int32)
    device                                   offset    8  (IntPtr)
    reserved                                 offset   16  (IntPtr)
    apiVersion                               offset   24  (UInt32)
    reserved1                                offset   28  (UInt32[])
    reserved2                                offset 1040  (IntPtr[])
  NV_ENC_REGISTER_RESOURCE:
    Marshal.SizeOf = 1532 bytes
    version                                  offset    0  (UInt32)
    resourceType                             offset    4  (Int32)
    width                                    offset    8  (UInt32)
    height                                   offset   12  (UInt32)
    pitch                                    offset   16  (UInt32)
    subResourceIndex                         offset   20  (UInt32)
    resourceToRegister                       offset   24  (IntPtr)
    registeredResource                       offset   32  (IntPtr)
    bufferFormat                             offset   40  (Int32)
    reserved1                                offset   44  (UInt32[])
    reserved2                                offset 1036  (IntPtr[])
  NV_ENC_INITIALIZE_PARAMS:
    Marshal.SizeOf = 1784 bytes
    version                                  offset    0  (UInt32)
    encodeGUID                               offset    4  (Guid)
    presetGUID                               offset   20  (Guid)
    encodeWidth                              offset   36  (UInt32)
    encodeHeight                             offset   40  (UInt32)
    darWidth                                 offset   44  (UInt32)
    darHeight                                offset   48  (UInt32)
    frameRateNum                             offset   52  (UInt32)
    frameRateDen                             offset   56  (UInt32)
    enableEncodeAsync                        offset   60  (UInt32)
    enablePTD                                offset   64  (UInt32)
    bitFields                                offset   68  (UInt32)
    privDataSize                             offset   72  (UInt32)
    _padding1                                offset   76  (UInt32)
    privData                                 offset   80  (IntPtr)
    encodeConfig                             offset   88  (IntPtr)
    maxEncodeWidth                           offset   96  (UInt32)
    maxEncodeHeight                          offset  100  (UInt32)
    maxMEHintCountsPerBlockL0                offset  104  (UInt32)
    maxMEHintCountsPerBlockL1                offset  108  (UInt32)
    reserved                                 offset  112  (UInt32[])
    _padding2                                offset 1268  (UInt32)
    reserved2                                offset 1272  (IntPtr[])
  NV_ENCODE_API_FUNCTION_LIST:
    Marshal.SizeOf = 2552 bytes
    version                                  offset    0  (UInt32)
    reserved                                 offset    4  (UInt32)
    nvEncOpenEncodeSession                   offset    8  (IntPtr)
    nvEncGetEncodeGUIDCount                  offset   16  (IntPtr)
    nvEncGetEncodeProfileGUIDCount           offset   24  (IntPtr)
    nvEncGetEncodeProfileGUIDs               offset   32  (IntPtr)
    nvEncGetEncodeGUIDs                      offset   40  (IntPtr)
    nvEncGetInputFormatCount                 offset   48  (IntPtr)
    nvEncGetInputFormats                     offset   56  (IntPtr)
    nvEncGetEncodeCaps                       offset   64  (IntPtr)
    nvEncGetEncodePresetCount                offset   72  (IntPtr)
    nvEncGetEncodePresetGUIDs                offset   80  (IntPtr)
    nvEncGetEncodePresetConfig               offset   88  (IntPtr)
    nvEncInitializeEncoder                   offset   96  (IntPtr)
    nvEncCreateInputBuffer                   offset  104  (IntPtr)
    nvEncDestroyInputBuffer                  offset  112  (IntPtr)
    nvEncCreateBitstreamBuffer               offset  120  (IntPtr)
    nvEncDestroyBitstreamBuffer              offset  128  (IntPtr)
    nvEncEncodePicture                       offset  136  (IntPtr)
    nvEncLockBitstream                       offset  144  (IntPtr)
    nvEncUnlockBitstream                     offset  152  (IntPtr)
    nvEncLockInputBuffer                     offset  160  (IntPtr)
    nvEncUnlockInputBuffer                   offset  168  (IntPtr)
    nvEncGetEncodeStats                      offset  176  (IntPtr)
    nvEncGetSequenceParams                   offset  184  (IntPtr)
    nvEncRegisterAsyncEvent                  offset  192  (IntPtr)
    nvEncUnregisterAsyncEvent                offset  200  (IntPtr)
    nvEncMapInputResource                    offset  208  (IntPtr)
    nvEncUnmapInputResource                  offset  216  (IntPtr)
    nvEncDestroyEncoder                      offset  224  (IntPtr)
    nvEncInvalidateRefFrames                 offset  232  (IntPtr)
    nvEncOpenEncodeSessionEx                 offset  240  (IntPtr)
    nvEncRegisterResource                    offset  248  (IntPtr)
    nvEncUnregisterResource                  offset  256  (IntPtr)
    nvEncReconfigureEncoder                  offset  264  (IntPtr)
    reserved1                                offset  272  (IntPtr)
    nvEncCreateMVBuffer                      offset  280  (IntPtr)
    nvEncDestroyMVBuffer                     offset  288  (IntPtr)
    nvEncRunMotionEstimationOnly             offset  296  (IntPtr)
    reserved2                                offset  304  (IntPtr[])
  NV_ENC_INITIALIZE_PARAMS_VER: 0xF005000D
  NV_ENC_REGISTER_RESOURCE_VER: 0x7003000D
  NV_ENCODE_API_FUNCTION_LIST_VER: 0x7001000D

[4.4b] Initializing encoder (required before RegisterResource)...
  PASS: Encoder initialized (1680x1050 @ 60fps, H.264, Default preset).

[4.5] Registering a fresh staging texture with NVENC...
  Creating fresh texture: 1680x1050 BGRA8 on device 0x0000017b02654bd0
  Fresh texture pointer: 0x0000017b028917a0
  Fresh texture's parent device: 0x0000017b02654bd0
  Phase 1 device pointer:        0x0000017b02654bd0
  PASS: Fresh texture is on the same D3D11 device.
  PASS: Texture registered with NVENC.
         Registered handle: 0x0000017b06ba73e0
         Width:  1680
         Height: 1050
         Format: ARGB (BGRA8)
         Resource type: DirectX (D3D11Texture2D)

[4.6] Unregistering resource and destroying encoder...
  PASS: Resource unregistered.
  PASS: Fresh texture disposed.
  PASS: Encoder destroyed.

============================================================
 Phase 4 RESULT
============================================================
  NVENC loaded:          YES
  Encode session:        OPENED on D3D11 device
  H.264 codec:           SUPPORTED
  ARGB (BGRA8) format:   SUPPORTED
  Texture registered:    YES (NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX)
  GPU:                   NVIDIA GeForce GTX 1080 Ti
  Format:                ARGB (BGRA8)
============================================================

  V1 BLOCKER STATUS: ✅ RESOLVED — zero-copy path PROVEN

============================================================
 Phase 5 — Performance Benchmark
============================================================

[5.1] Determining current desktop resolution...
  Desktop resolution: 1680x1050
  NOTE: Spike cannot force desktop resolution change.
        Will run benchmarks matching current desktop resolution only.
  WARN: Current desktop (1680x1050) is not in target list.
        Running benchmark at current resolution with all 3 FPS targets.

[5.2] Benchmark: 1680x1050 @ 60 FPS, 10s duration
  Target:    1680x1050 @ 60 FPS
  Achieved:  74.53 FPS (745 frames in 1342 iterations)
  Latency:   avg=7.849 ms, p50=8.325 ms, p95=13.630 ms, p99=14.358 ms
  Errors:    WT=597, AL=0, Other=0, Dropped=0
  CPU usage: 0.0%
  GPU usage: 0.0%
  NVENC:     Not benchmarked in Phase 5 (separate concern)

[5.2] Benchmark: 1680x1050 @ 120 FPS, 10s duration
  Target:    1680x1050 @ 120 FPS
  Achieved:  74.76 FPS (748 frames in 1342 iterations)
  Latency:   avg=8.154 ms, p50=8.539 ms, p95=13.636 ms, p99=14.894 ms
  Errors:    WT=594, AL=0, Other=0, Dropped=0
  CPU usage: 0.0%
  GPU usage: 0.0%
  NVENC:     Not benchmarked in Phase 5 (separate concern)

[5.2] Benchmark: 1680x1050 @ 144 FPS, 10s duration
  Target:    1680x1050 @ 144 FPS
  Achieved:  74.77 FPS (748 frames in 1340 iterations)
  Latency:   avg=7.946 ms, p50=8.157 ms, p95=13.510 ms, p99=14.995 ms
  Errors:    WT=592, AL=0, Other=0, Dropped=0
  CPU usage: 0.0%
  GPU usage: 0.0%
  NVENC:     Not benchmarked in Phase 5 (separate concern)

============================================================
 Phase 5 SUMMARY
============================================================
  Resolution   TargetFPS  AchievedFPS  p95Lat(ms)   p99Lat(ms)   Dropped  Status  
  ------------ ---------- ------------ ----------   ----------   -------  --------
  1680x1050    60         74.53        13.63        14.36        0        PASS    
  1680x1050    120        74.76        13.64        14.89        0        FAIL    
  1680x1050    144        74.77        13.51        14.99        0        FAIL    

  Benchmark results: 1/3 PASS

  Phase 5: FAIL — see results above

*** Phase 5 FAILED — stopping.

============================================================
 SPIKE RESULT: AT LEAST ONE PHASE FAILED
============================================================
