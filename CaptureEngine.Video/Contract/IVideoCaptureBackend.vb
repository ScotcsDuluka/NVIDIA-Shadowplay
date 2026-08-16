Option Strict On
Option Explicit On

Namespace CaptureEngine.Video
    ''' <summary>
    ''' A video capture backend. (P1-A v1.3.1 §4.1, §11)
    '''
    ''' Implemented by DdagrabBackend, GfxCaptureBackend (future), and
    ''' FakeVideoCaptureBackend (P1-B.1).
    ''' </summary>
    Public Interface IVideoCaptureBackend
        Inherits IDisposable

        ''' <summary>
        ''' Back-end initialization. See §5 for device-ownership behaviour.
        ''' </summary>
        ''' <exception cref="VideoBackendConfigurationException">Thrown when config values are invalid for this backend.</exception>
        ''' <exception cref="VideoBackendRuntimeException">Thrown when backend-specific native resources cannot be created.</exception>
        Sub Initialize(context As IVideoBackendContext)

        ''' <summary>
        ''' Begin emitting FrameAcquisitionResult values into the sink. Returns
        ''' when the backend has started its capture thread/poll loop.
        ''' </summary>
        Sub Start(sink As IVideoFrameSink)

        ''' <summary>
        ''' Stop emitting. Must drain in-flight results per the configured
        ''' BoundedHandoffPolicy before returning.
        ''' </summary>
        Sub [Stop]()

        ' IDisposable.Dispose inherited — idempotent, robust, never throws.

        ''' <summary>
        ''' Read-only diagnostics surface (§3.2). Exposed for the entire
        ''' lifetime of the backend; counters may be polled from any thread.
        ''' </summary>
        ReadOnly Property Diagnostics As IVideoBackendDiagnostics
    End Interface
End Namespace
