Option Strict On
Option Explicit On

Namespace CaptureEngine.Video.Backends.Native
    ''' <summary>
    ''' Native Capture Backend Architecture — P1-E
    '''
    ''' ARCHITECTURE DECISION (P1-C LOCKED):
    '''
    '''   FFmpeg = Full-Pipeline Execution Backend
    '''     (process-based: capture + filter + encode + output in one ffmpeg process)
    '''
    '''   Native = Frame Backend
    '''     (produces IVideoFrame, feeds Frame Pipeline: sink → encoder → output)
    '''
    ''' This document describes the Native Frame Backend architecture.
    '''
    ''' ┌─────────────────────────────────────────────────────────┐
    ''' │                   Engine                                 │
    ''' │                                                           │
    ''' │  ┌──────────────────┐    ┌──────────────────────────┐    │
    ''' │  │ FFmpegPipeline   │    │ NativeCaptureBackend       │    │
    ''' │  │ Backend          │    │ (INativeCaptureBackend)    │    │
    ''' │  │                  │    │                            │    │
    ''' │  │ ffmpeg.exe       │    │ DXGI Desktop Duplication    │    │
    ''' │  │  ├ ddagrab       │    │ WGC (future)              │    │
    ''' │  │  ├ filter        │    │ NvFBC (future)             │    │
    ''' │  │  ├ nvenc         │    │                            │    │
    ''' │  │  └ output        │    │  D3D11 Device (borrowed)   │    │
    ''' │  │                  │    │  ↓                         │    │
    ''' │  │ No IVideoFrame   │    │  IVideoFrame (GPU texture) │    │
    ''' │  │ No IVideoSink    │    │  ↓                         │    │
    ''' │  │ No FramePipeline │    │  IVideoFrameSink           │    │
    ''' │  │                  │    │  ↓                         │    │
    ''' │  │ Process IS the   │    │  Frame Pipeline             │    │
    ''' │  │ entire pipeline  │    │  ↓                         │    │
    ''' │  │                  │    │  Encoder Backend            │    │
    ''' │  │                  │    │  ↓                         │    │
    ''' │  │                  │    │  Output                     │    │
    ''' │  └──────────────────┘    └──────────────────────────┘    │
    ''' │                                                           │
    ''' │  Two independent paths. No coupling.                      │
    ''' └─────────────────────────────────────────────────────────┘
    '''
    '''
    ''' D3D11 DEVICE OWNERSHIP:
    '''
    '''   The D3D11 device is BORROWED by the native backend — not owned.
    '''
    '''   Engine creates D3D11 device with:
    '''     - D3D11_CREATE_DEVICE_VIDEO_SUPPORT (for NVENC)
    '''     - D3D11_CREATE_DEVICE_BGRA_SUPPORT (for Desktop Duplication)
    '''     - ID3D11Multithread::SetMultithreadProtected(TRUE)
    '''
    '''   The device is passed to the native backend via Initialize(context).
    '''   The backend uses it to create IDXGIOutputDuplication.
    '''   The backend does NOT dispose the device — the engine owns it.
    '''
    '''   The same device is also passed to the NVENC encoder.
    '''   This enables zero-copy: the staging texture from capture can be
    '''   registered with NvEncRegisterResource on the same device.
    '''
    '''   V1 SPIKE PROVEN:
    '''     - D3D11_CREATE_DEVICE_VIDEO_SUPPORT flag is required
    '''     - Multithread protection is required (without it, Desktop Duplication
    '''       performance collapses from ~100 FPS to ~3 FPS)
    '''     - NvEncRegisterResource succeeds when InitializeEncoder is called first
    '''     - Texture must have D3D11_BIND_RENDER_TARGET flag
    '''
    '''
    ''' GPU RESOURCE LIFETIME:
    '''
    '''   Resource            Created              Released              Owner
    '''   ─────────────────   ──────────────────   ──────────────────   ──────────
    '''   D3D11 Device        Engine init          Engine shutdown       Engine
    '''   IDXGIOutputDupl     Backend.Initialize   Backend.Stop/Dispose  Backend
    '''   Staging Texture     Backend.Start        Backend.Stop/Dispose  Backend
    '''   Desktop Texture     AcquireNextFrame     Per-frame (Release)   DXGI
    '''   NVENC Encoder       Encoder init        Encoder shutdown     Encoder
    '''   NVENC Registered    RegisterResource    UnregisterResource   Encoder
    '''
    '''   Key rule: The backend releases all resources it creates, but NEVER
    '''   disposes the D3D11 device (it's borrowed).
    '''
    '''
    ''' FUTURE NVENC COMPATIBILITY:
    '''
    '''   INativeCaptureBackend exposes:
    '''     - D3D11DeviceHandle (IntPtr) — for NvEncOpenEncodeSessionEx
    '''     - AdapterLuid (Long) — for same-GPU verification
    '''
    '''   The encoder can verify zero-copy feasibility:
    '''     If backend.AdapterLuid = encoder.AdapterLuid Then
    '''         ' Same GPU — zero-copy possible
    '''         NvEncRegisterResource(texture)
    '''     Else
    '''         ' Different GPU — fallback to CPU copy
    '''     End If
    '''
    '''   V1 spike proved this path works with GTX 1080 Ti + driver 582.66.
    '''
    '''
    ''' THREADING MODEL:
    '''
    '''   Thread              Role                         Lock usage
    '''   ─────────────────   ──────────────────────────  ──────────────────────────
    '''   Engine thread       Initialize, Start, Stop      Acquires _sync briefly
    '''   Worker thread       AcquireNextFrame loop        Acquires _sync for state read
    '''   Consumer thread     TryTake from sink            No backend lock
    '''   Any thread          Diagnostics properties       Acquires _sync briefly
    '''
    '''   Dispose safety (P1-B.1 FIX pattern):
    '''     1. Capture state under lock
    '''     2. Set _stopSignal + _disposed under lock
    '''     3. Release lock
    '''     4. Join worker thread OUTSIDE lock
    '''     5. Reacquire lock to finalize state = Disposed
    '''
    '''   This prevents deadlock: worker needs _sync to read _stopSignal,
    '''   so Dispose must NOT hold _sync across worker.Join().
    '''
    '''
    ''' BACKEND HIERARCHY:
    '''
    '''   INativeCaptureBackend (contract)
    '''     ├── DxgiCaptureBackend (DXGI Desktop Duplication)
    '''     │   ├── D3D11 device (borrowed)
    '''     │   ├── IDXGIOutputDuplication
    '''     │   ├── IDXGIOutput1 (DuplicateOutput1 with format list)
    '''     │   ├── AcquireNextFrame → ID3D11Texture2D
    '''     │   └── CopySubresourceRegion → staging texture
    '''     │
    '''     ├── WgcCaptureBackend (future — Windows.Graphics.Capture)
    '''     │   ├── IDirect3DDevice (WinRT)
    '''     │   ├── GraphicsCaptureItem (from HMONITOR)
    '''     │   ├── Direct3D11CaptureFramePool
    '''     │   └── FrameArrived event → Direct3D11CaptureFrame
    '''     │
    '''     └── NvFbcCaptureBackend (future — NVIDIA Frame Buffer Capture)
    '''       (requires NVIDIA SDK, not yet available)
    '''
    '''   All implement IVideoCaptureBackend (for lifecycle + sink) and
    '''   INativeCaptureBackend (for D3D11 device + LUID + IsCapturing).
    ''' </summary>
    Public Class NativeBackendArchitecture_DocumentationOnly
        ' This class exists solely for XML documentation.
        ' No runtime behavior.
    End Class
End Namespace
