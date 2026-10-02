Option Strict On
Option Explicit On
Option Infer On

' VideoRenderSink.vb — the render-target contract + a headless sink.
'
' WHY THIS CONTRACT EXISTS (design doc §3.3/§3.8):
'   The render thread owns ALL GPU work. The sink is what the render thread
'   presents INTO. Two implementations:
'     - NullVideoSink   : headless counter sink — proves the whole pipeline
'                         (queue → clock → present decisions → dispose) on
'                         any OS, with zero display. Used by tests + by the
'                         session when no window handle is configured.
'     - D3D11VideoRenderer : the real present path (Vortice swapchain),
'                         created only for a real HWND on Windows.
'
' OWNERSHIP RULE (single-owner discipline, §3.3):
'   Present(frame) is SYNCHRONOUS. The sink copies/uploads whatever it needs
'   and returns; the frame buffer stays owned by the CALLER, which disposes
'   it right after Present returns. The sink NEVER retains a PlaybackFrame
'   beyond the call, so every frame keeps flowing through exactly one
'   dispose point — the leak invariant stays testable end to end.

Imports System
Imports System.Threading

Namespace Gallery.Video

    ''' <summary>A render target for presented frames. Implementations must be
    ''' thread-safe for ONE presenting thread (the session's render thread);
    ''' Present must not throw on device loss — it reports via IsAvailable.</summary>
    Public Interface IVideoRenderSink
        Inherits IDisposable

        ''' <summary>False after an unrecoverable device loss (session maps this
        ''' to RendererUnavailable per §7).</summary>
        ReadOnly Property IsAvailable As Boolean

        ''' <summary>Synchronous present of one frame (copy/upload inside the
        ''' call; caller keeps ownership and disposes after return).</summary>
        Sub Present(frame As PlaybackFrame)

        ''' <summary>Successful Present calls (leak-invariant evidence).</summary>
        ReadOnly Property PresentsCount As Long

        ''' <summary>Device-loss recreations attempted (D3D11 path; Null = 0).</summary>
        ReadOnly Property DeviceLostCount As Long
    End Interface

    ''' <summary>
    ''' Headless sink: records metrics only. This is the Linux-testable proof
    ''' instrument for the present pipeline — same decision path, zero GPU.
    ''' </summary>
    Public NotInheritable Class NullVideoSink
        Implements IVideoRenderSink

        Private _availableState As Integer = 1   ' 1 available, 0 not
        Private _presents As Long
        Private _lastPts As Long = -1
        Private _lastSequence As Long = -1

        Public ReadOnly Property IsAvailable As Boolean Implements IVideoRenderSink.IsAvailable
            Get
                Return Volatile.Read(_availableState) <> 0
            End Get
        End Property

        Public Sub Present(frame As PlaybackFrame) Implements IVideoRenderSink.Present
            If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))
            Interlocked.Increment(_presents)
            Volatile.Write(_lastPts, frame.PtsTicks)
            Volatile.Write(_lastSequence, frame.Sequence)
        End Sub

        Public ReadOnly Property PresentsCount As Long Implements IVideoRenderSink.PresentsCount
            Get
                Return Volatile.Read(_presents)
            End Get
        End Property

        Public ReadOnly Property DeviceLostCount As Long Implements IVideoRenderSink.DeviceLostCount
            Get
                Return 0L
            End Get
        End Property

        ''' <summary>PTS of the most recently presented frame (diagnostics/tests).</summary>
        Public ReadOnly Property LastPresentedPtsTicks As Long
            Get
                Return Volatile.Read(_lastPts)
            End Get
        End Property

        ''' <summary>Sequence of the most recently presented frame (tests pin
        ''' the pause-seek "show the target frame" behavior with this).</summary>
        Public ReadOnly Property LastPresentedSequence As Long
            Get
                Return Volatile.Read(_lastSequence)
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Nothing to release — kept for Using symmetry with the real sink.
        End Sub

    End Class

End Namespace
