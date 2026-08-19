Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic

Namespace CaptureEngine.Backends
    ''' <summary>
    ''' Contract for video capture backends. Replaces direct FFmpeg invocation
    ''' so the Engine can support both FFmpeg-based backends (ddagrab/gdigrab/
    ''' gfxcapture) and future native backends (DXGI Desktop Duplication,
    ''' Windows Graphics Capture, NvFBC).
    '''
    ''' This is the new architectural slot defined in docs/ARCHITECTURE.md:
    '''
    '''   CaptureEngine
    '''       │
    '''       ▼
    '''   VideoLayer (Pipeline resolver)
    '''       │
    '''       ▼
    '''   Backend (implements IVideoBackend)
    '''       │
    '''       ▼
    '''   Encoder
    '''
    ''' Compatibility:
    '''   - CaptureEngine.Video.IVideoCaptureBackend is the Foundation contract
    '''     (FROZEN @ 82d792ab). IVideoBackend is a HIGHER-LEVEL interface that
    '''     lives in the CaptureEngine assembly (NOT CaptureEngine.Video) and
    '''     is intended for use by the Engine's Pipeline layer.
    '''   - When a CaptureEngine.Video backend (e.g. DdagrabBackend skeleton)
    '''     is wired up to IVideoBackend, an adapter class will wrap it.
    '''   - IVideoBackend intentionally does NOT expose FrameCount /
    '''     FrameAvailable status (those are Foundation concerns). IVideoBackend
    '''     is about start/stop/get-frame lifecycle + diagnostics.
    '''
    ''' Lifecycle:
    '''   Create → Start → [GetFrame() repeatedly] → Stop → Dispose
    '''
    '''   Start is idempotent — calling it twice must not throw.
    '''   Stop is idempotent — calling it twice must not throw.
    '''   GetFrame is only valid between Start and Stop.
    '''   Dispose is idempotent — calling it twice must not throw.
    '''
    ''' Threading:
    '''   - Start, Stop, Dispose are thread-safe (may be called from any thread).
    '''   - GetFrame is intended to be called from a dedicated capture thread
    '''     (not the UI thread). It may block briefly waiting for the next
    '''     frame from the source.
    ''' </summary>
    Public Interface IVideoBackend
        Inherits IDisposable

        ''' <summary>Backend lifecycle state.</summary>
        ReadOnly Property CurrentState As VideoBackendState

        ''' <summary>
        ''' Start capturing. Idempotent — calling twice does nothing.
        '''
        ''' Throws InvalidOperationException if called after Dispose.
        ''' Throws backend-specific exceptions on capture init failure
        ''' (e.g. DXGI output not found, FFmpeg binary missing).
        ''' </summary>
        Sub Start()

        ''' <summary>
        ''' Stop capturing. Idempotent — calling twice does nothing.
        '''
        ''' Stop signals the producer to halt, drains any in-flight frames
        ''' (sink-owned), and finalizes the backend's capture session.
        '''
        ''' Per P1-A v1.3.1 §4 + P1-B.1 FIX change #2: Stop stops the producer;
        ''' queued frames remain sink-owned; backend does NOT require unbounded
        ''' drain during Stop.
        ''' </summary>
        Sub Stop()

        ''' <summary>
        ''' Try to get the next captured frame.
        '''
        ''' Returns:
        '''   - VideoFrame with valid data if a frame is available
        '''   - Nothing if no frame is available (caller should retry or yield)
        '''
        ''' Throws InvalidOperationException if called before Start or after Stop.
        ''' Throws ObjectDisposedException if called after Dispose.
        '''
        ''' Implementation note: GetFrame should NOT block indefinitely.
        ''' Callers polling in a tight loop should add their own sleep/yield
        ''' between calls (the NoFrame case is the backend's signal that
        ''' no work is ready yet, per P1-A v1.3.1 §6.4).
        ''' </summary>
        Function GetFrame() As VideoFrame

        ''' <summary>
        ''' Get backend-specific diagnostics counters (frames emitted,
        ''' frames dropped, errors, etc.). Read-only snapshot.
        '''
        ''' Returns a snapshot dictionary. Never throws.
        ''' </summary>
        Function GetDiagnostics() As IReadOnlyDictionary(Of String, Long)
    End Interface

    ''' <summary>Backend lifecycle state.</summary>
    Public Enum VideoBackendState
        ''' <summary>Created but not started.</summary>
        Created

        ''' <summary>Start() in progress (capture init).</summary>
        Starting

        ''' <summary>Capturing frames.</summary>
        Running

        ''' <summary>Stop() in progress (drain + finalize).</summary>
        Stopping

        ''' <summary>Stopped — can be started again.</summary>
        Stopped

        ''' <summary>Permanently disposed — cannot be restarted.</summary>
        Disposed

        ''' <summary>Backend failed (capture init error, device lost, etc.).</summary>
        Faulted
    End Enum

    ''' <summary>
    ''' Captured video frame — opaque handle to a single frame's worth of data.
    '''
    ''' Lightweight value type. Concrete backends (FFmpegBackend, D3D11Backend)
    ''' will provide their own concrete implementations; this interface allows
    ''' the pipeline layer to consume frames without knowing the underlying
    ''' format (BGRA8 texture, NV12 surface, raw byte buffer, etc.).
    '''
    ''' Frame ownership:
    '''   - Caller receives a VideoFrame reference
    '''   - Caller MUST call Dispose() when done (frames may hold GPU resources)
    '''   - Caller MUST NOT hold frame references across Stop() boundaries
    ''' </summary>
    Public Interface VideoFrame
        Inherits IDisposable

        ''' <summary>Frame width in pixels.</summary>
        ReadOnly Property Width As Integer

        ''' <summary>Frame height in pixels.</summary>
        ReadOnly Property Height As Integer

        ''' <summary>Frame timestamp in 100-nanosecond QPC-derived ticks.</summary>
        ReadOnly Property TimestampTicks As Long

        ''' <summary>Origin: "ddagrab" | "gdigrab" | "gfxcapture" | "dxgi" | "wgc".</summary>
        ReadOnly Property Origin As String

        ''' <summary>Pixel format: "bgra8" | "nv12" | "yuv420p" | etc.</summary>
        ReadOnly Property PixelFormat As String
    End Interface
End Namespace
