Option Strict On
Option Explicit On

Namespace CaptureEngine.Video.Backends.Native
    ''' <summary>
    ''' State machine for native capture backends.
    '''
    ''' State transitions:
    '''   Created → Initialized (via Initialize)
    '''   Initialized → Running (via Start)
    '''   Running → Stopping → Stopped (via Stop)
    '''   Running → Disposed (via Dispose while Running)
    '''   Stopped → Running (via Start — restart)
    '''   Any → Faulted (on unexpected error)
    '''   Any → Disposed (via Dispose)
    '''
    ''' Faulted is terminal — cannot restart from Faulted without
    ''' creating a new backend instance.
    ''' </summary>
    Public Enum NativeBackendState
        ''' <summary>Backend created but not initialized.</summary>
        Created

        ''' <summary>Initialize succeeded. D3D11 device borrowed, adapter selected.</summary>
        Initialized

        ''' <summary>Start called. Worker thread beginning.</summary>
        Starting

        ''' <summary>Worker thread running, frames being delivered to sink.</summary>
        Running

        ''' <summary>Stop called. Worker being signaled to stop.</summary>
        Stopping

        ''' <summary>Worker stopped. D3D11 capture session released. Device may be borrowed again on restart.</summary>
        Stopped

        ''' <summary>Unexpected error. D3D11 resources may be in an inconsistent state. Backend cannot restart.</summary>
        Faulted

        ''' <summary>All resources released. Backend cannot be used.</summary>
        Disposed
    End Enum
End Namespace
