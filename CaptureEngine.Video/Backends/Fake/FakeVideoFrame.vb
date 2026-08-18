Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading

Namespace CaptureEngine.Video.Backends.Fake
    ''' <summary>
    ''' Minimal IVideoFrame implementation for the fake backend. (P1-A v1.3.1 §3)
    '''
    ''' Tracks Dispose() calls so ownership/lifetime tests can assert that
    ''' every FrameAvailable result's frame is disposed exactly once by
    ''' whoever currently owns it (the sink under DropOldest, the backend
    ''' under DropNewest-with-Dropped outcome, or the consumer downstream).
    ''' </summary>
    Public NotInheritable Class FakeVideoFrame
        Implements IVideoFrame

        Private ReadOnly _origin As VideoFrameOrigin
        Private ReadOnly _pixelFormat As VideoPixelFormat
        Private ReadOnly _dimensions As VideoFrameDimensions
        Private ReadOnly _diagnostics As FrameDiagnostics

        Private _disposed As Boolean = False
        Private _disposeCount As Integer = 0

        Public Sub New(origin As VideoFrameOrigin,
                       pixelFormat As VideoPixelFormat,
                       dimensions As VideoFrameDimensions,
                       diagnostics As FrameDiagnostics)
            _origin = origin
            _pixelFormat = pixelFormat
            _dimensions = dimensions
            _diagnostics = diagnostics
        End Sub

        Public ReadOnly Property Origin As VideoFrameOrigin Implements IVideoFrame.Origin
            Get
                Return _origin
            End Get
        End Property

        Public ReadOnly Property PixelFormat As VideoPixelFormat Implements IVideoFrame.PixelFormat
            Get
                Return _pixelFormat
            End Get
        End Property

        Public ReadOnly Property Dimensions As VideoFrameDimensions Implements IVideoFrame.Dimensions
            Get
                Return _dimensions
            End Get
        End Property

        Public ReadOnly Property Diagnostics As FrameDiagnostics Implements IVideoFrame.Diagnostics
            Get
                Return _diagnostics
            End Get
        End Property

        ''' <summary>
        ''' Number of times Dispose() has been called. MUST be exactly 1 over
        ''' the frame's lifetime; >1 indicates double-dispose, 0 indicates
        ''' a leak.
        ''' </summary>
        Public ReadOnly Property DisposeCount As Integer
            Get
                Return Thread.VolatileRead(_disposeCount)
            End Get
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return Thread.VolatileRead(_disposeCount) > 0
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            Thread.VolatileWrite(_disposeCount, Thread.VolatileRead(_disposeCount) + 1)
            _disposed = True
        End Sub
    End Class
End Namespace
