Option Strict On
Option Explicit On
Option Infer On

' PlaybackFrame.vb — one decoded BGRA8 frame + PTS (100-ns ticks).
'
' Ownership discipline cloned from CaptureEngine.Video.Ddagrab/D3D11VideoFrame.vb
' (audited pattern, design doc §2.1):
'   - single-owner at all times; ownership transfers via FrameQueue;
'   - Dispose() is one-shot via Interlocked.CompareExchange;
'   - never throws from Dispose;
'   - property reads remain safe after Dispose;
'   - a dispose callback feeds leak-invariant metrics (created vs disposed).

Imports System
Imports System.Threading

Namespace Gallery.Video

    Public NotInheritable Class PlaybackFrame
        Implements IDisposable

        ''' <summary>BGRA8 pixel data, Width*Height*4 bytes (row-major, top-down —
        ''' the exact layout rawvideo bgra emits; uploaded once on the render thread).</summary>
        Public ReadOnly Property Pixels As Byte()

        ''' <summary>Frame width in pixels.</summary>
        Public ReadOnly Property Width As Integer

        ''' <summary>Frame height in pixels.</summary>
        Public ReadOnly Property Height As Integer

        ''' <summary>Frame presentation timestamp in 100-ns ticks (QPC domain —
        ''' same units as FrameDiagnostics/SyncMath conventions).</summary>
        Public ReadOnly Property PtsTicks As Long

        ''' <summary>Generation token the frame was produced under. Frames from an
        ''' older generation than the current one are stale and must be dropped
        ''' on dequeue (seek/flush contract — no stale frame is ever presented).</summary>
        Public ReadOnly Property Generation As Long

        ''' <summary>Diagnostic counter: monotonic index within its generation.</summary>
        Public ReadOnly Property Sequence As Long

        Private _disposedState As Integer = 0
        Private _onDisposed As Action

        ''' <summary>Leak-invariant metric hook: invoked exactly once on Dispose
        ''' regardless of who disposes (queue flush, render consumer, session stop).</summary>
        Public WriteOnly Property OnDisposed As Action
            Set(value As Action)
                _onDisposed = value
            End Set
        End Property

        Public Sub New(pixels As Byte(), width As Integer, height As Integer,
                       ptsTicks As Long, generation As Long, sequence As Long)
            If pixels Is Nothing Then Throw New ArgumentNullException(NameOf(pixels))
            Dim expected = width * height * 4
            If width <= 0 OrElse height <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(width), "dimensions must be positive")
            If pixels.Length <> expected Then
                Throw New ArgumentException($"pixel buffer {pixels.Length}B != {width}x{height}x4 = {expected}B")
            End If

            _pixels = pixels
            _width = width
            _height = height
            _ptsTicks = ptsTicks
            _generation = generation
            _sequence = sequence
        End Sub

        Public ReadOnly Property BytesRequired As Integer
            Get
                Return Width * Height * 4
            End Get
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return Volatile.Read(_disposedState) <> 0
            End Get
        End Property

        ''' <summary>One-shot dispose (P1-B.1 FIX pattern — concurrent Dispose safe).</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.CompareExchange(_disposedState, 1, 0) <> 0 Then Return

            ' Drop the pixel buffer reference first (GC pressure relief), then
            ' fire the metric callback. Never throw from Dispose (lesson #3).
            _pixels = Nothing
            Try
                _onDisposed?.Invoke()
            Catch
            End Try
        End Sub

    End Class

End Namespace
