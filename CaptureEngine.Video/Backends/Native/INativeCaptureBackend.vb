Option Strict On
Option Explicit On

Namespace CaptureEngine.Video.Backends.Native
    ''' <summary>
    ''' Native capture backend contract for GPU-resident frame capture.
    '''
    ''' ARCHITECTURE DECISION (P1-C LOCKED):
    '''   FFmpeg = Full-Pipeline Execution Backend (process-based, no IVideoFrame)
    '''   Native = Frame Backend (produces IVideoFrame, feeds Frame Pipeline)
    '''
    ''' This contract is for backends that capture desktop frames into GPU-resident
    ''' D3D11 textures (DXGI Desktop Duplication, WGC, NvFBC). The frames are then
    ''' consumed by a Frame Pipeline (IVideoFrameSink → Encoder → Output).
    '''
    ''' INativeCaptureBackend extends IVideoCaptureBackend — it adds:
    '''   - GPU device ownership reporting
    '''   - Native resource lifetime management
    '''   - Future NVENC zero-copy compatibility surface
    '''
    ''' Lifecycle:
    '''   Created → Initialized → Running → Stopped → Disposed
    '''                      ↘ Faulted ↗
    '''
    ''' Threading model:
    '''   - Initialize, Start, Stop, Dispose may be called from any thread
    '''   - Frame acquisition (worker thread) is internal — consumers never
    '''     call AcquireFrame directly; frames arrive via IVideoFrameSink
    '''   - Diagnostics properties are thread-safe (Interlocked or lock)
    '''
    ''' Dispose safety:
    '''   - Dispose MUST be idempotent (callable multiple times, no exception)
    '''   - Dispose while Running MUST stop the worker + release GPU resources
    '''   - Dispose MUST NOT hold a lock across worker.Join()
    '''   - After Dispose, all methods throw ObjectDisposedException
    ''' </summary>
    Public Interface INativeCaptureBackend
        Inherits IVideoCaptureBackend

        ''' <summary>
        ''' The D3D11 device this backend uses for capture.
        ''' Returns Nothing before Initialize, and after Dispose.
        '''
        ''' This device is SHARED with the encoder (NVENC) to enable
        ''' zero-copy texture registration. The backend does NOT own
        ''' the device exclusively — it borrows it from the engine.
        ''' </summary>
        ReadOnly Property D3D11DeviceHandle As IntPtr

        ''' <summary>
        ''' The GPU adapter LUID this backend is bound to.
        ''' Returns Nothing before Initialize.
        ''' Used to verify that capture and encode are on the same
        ''' physical GPU (V1 zero-copy requirement).
        ''' </summary>
        ReadOnly Property AdapterLuid As Long?

        ''' <summary>
        ''' Whether the backend currently owns a live D3D11 capture session.
        ''' True between Start and Stop/Dispose.
        ''' False after Dispose.
        ''' </summary>
        ReadOnly Property IsCapturing As Boolean
    End Interface
End Namespace
