Option Strict On
Option Explicit On
Option Infer On

' Phase4bContracts.vb — Phase 4B experiment-only contracts (NOT production).
'
' Production CaptureSession takes concrete DdagrabBackend/NvencEncoderBackend.
' The Phase 4B fork needs interface-typed seams so the SAME timing code can
' run A/B on:
'   - this Intel machine  : Phase4bSyntheticCapture + Phase4bCannedEncoder
'   - the NVIDIA machine  : real DdagrabBackend + real NvencEncoderBackend
'                           (wrapped by Phase4bDdagrabAdapter — zero changes
'                            to the real backends)
'
' NOTHING in this file modifies production behavior.

Imports System
Imports System.Collections.Concurrent
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab

Namespace Phase4b

    ''' <summary>IVideoCaptureBackend + the concrete members the production
    ''' CaptureSession.Run() reads for logging/diagnostics.</summary>
    Public Interface IPhase4bVideoBackend
        Inherits IVideoCaptureBackend

        ReadOnly Property OutputWidth As Integer
        ReadOnly Property OutputHeight As Integer
        ReadOnly Property OutputRefreshRate As Integer
        ReadOnly Property FramesPushed As Long
        ReadOnly Property AccessLostCount As Long
        ReadOnly Property TexturesCreated As Long
        ReadOnly Property TexturesDisposed As Long
    End Interface

    ''' <summary>Minimal IVideoBackendContext (RecordingEngine's is private).</summary>
    Public NotInheritable Class Phase4bVideoBackendContext
        Implements IVideoBackendContext

        Private ReadOnly _logger As EngineLogger
        Public Sub New(logger As EngineLogger)
            _logger = logger
        End Sub

        Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
            Get
                Return _logger
            End Get
        End Property

        Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
            Get
                Return VideoBackendKind.Ddagrab
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Adapter exposing a REAL production DdagrabBackend through the
    ''' experiment seam — for the NVIDIA-machine runbook run. Pure delegation.
    ''' </summary>
    Public NotInheritable Class Phase4bDdagrabAdapter
        Implements IPhase4bVideoBackend

        Private ReadOnly _inner As DdagrabBackend

        Public Sub New(inner As DdagrabBackend)
            If inner Is Nothing Then Throw New ArgumentNullException(NameOf(inner))
            _inner = inner
        End Sub

        Public ReadOnly Property Inner As DdagrabBackend
            Get
                Return _inner
            End Get
        End Property

        Public ReadOnly Property Diagnostics As IVideoBackendDiagnostics Implements IVideoCaptureBackend.Diagnostics
            Get
                Return _inner.Diagnostics
            End Get
        End Property
        Public Sub Initialize(context As IVideoBackendContext) Implements IVideoCaptureBackend.Initialize
            _inner.Initialize(context)
        End Sub
        Public Sub Start(sink As IVideoFrameSink) Implements IVideoCaptureBackend.Start
            _inner.Start(sink)
        End Sub
        Public Sub [Stop]() Implements IVideoCaptureBackend.Stop
            _inner.Stop()
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            _inner.Dispose()
        End Sub

        Public ReadOnly Property OutputWidth As Integer Implements IPhase4bVideoBackend.OutputWidth
            Get
                Return _inner.OutputWidth
            End Get
        End Property
        Public ReadOnly Property OutputHeight As Integer Implements IPhase4bVideoBackend.OutputHeight
            Get
                Return _inner.OutputHeight
            End Get
        End Property
        Public ReadOnly Property OutputRefreshRate As Integer Implements IPhase4bVideoBackend.OutputRefreshRate
            Get
                Return _inner.OutputRefreshRate
            End Get
        End Property
        Public ReadOnly Property FramesPushed As Long Implements IPhase4bVideoBackend.FramesPushed
            Get
                Return _inner.FramesPushed
            End Get
        End Property
        Public ReadOnly Property AccessLostCount As Long Implements IPhase4bVideoBackend.AccessLostCount
            Get
                Return _inner.AccessLostCount
            End Get
        End Property
        Public ReadOnly Property TexturesCreated As Long Implements IPhase4bVideoBackend.TexturesCreated
            Get
                Return _inner.TexturesCreated
            End Get
        End Property
        Public ReadOnly Property TexturesDisposed As Long Implements IPhase4bVideoBackend.TexturesDisposed
            Get
                Return _inner.TexturesDisposed
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Experiment-local replacement for the assembly-internal
    ''' DeferredVideoFrameDisposer (Friend in CaptureEngine.Recording):
    ''' background-thread frame disposer with the same contract —
    ''' Enqueue never blocks the CFR loop, CompleteAndWait drains.
    ''' </summary>
    Public NotInheritable Class Phase4bDeferredFrameDisposer
        Implements IDisposable

        Private ReadOnly _queue As New ConcurrentQueue(Of IVideoFrame)()
        Private ReadOnly _signal As New AutoResetEvent(False)
        Private ReadOnly _thread As Thread
        Private _completeRequested As Boolean = False
        Private _disposed As Boolean = False

        Public Sub New()
            _thread = New Thread(AddressOf LoopBody) With {.IsBackground = True, .Name = "Phase4bDeferredFrameDisposer"}
            _thread.Start()
        End Sub

        Public Sub Enqueue(frame As IVideoFrame)
            If frame Is Nothing Then Return
            If _disposed Then
                Try : frame.Dispose() : Catch : End Try
                Return
            End If
            _queue.Enqueue(frame)
            _signal.Set()
        End Sub

        ''' <summary>Drain everything already queued, then stop the thread.</summary>
        Public Sub CompleteAndWait()
            SyncLock _signal
                _completeRequested = True
            End SyncLock
            _signal.Set()
            If Not _thread.Join(10000) Then
                ' Same 10s cap as the production disposer contract.
            End If
        End Sub

        Private Sub LoopBody()
            Do
                Dim drained As Boolean = False
                Dim frame As IVideoFrame = Nothing
                Do While _queue.TryDequeue(frame)
                    drained = True
                    Try : frame.Dispose() : Catch : End Try
                Loop
                Dim shouldExit As Boolean = False
                SyncLock _signal
                    shouldExit = (_completeRequested AndAlso _queue.IsEmpty)
                End SyncLock
                If shouldExit Then Exit Do
                If Not drained Then _signal.WaitOne(50)
            Loop
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            SyncLock _signal
                _completeRequested = True
            End SyncLock
            _signal.Set()
            Dim frame As IVideoFrame = Nothing
            Do While _queue.TryDequeue(frame)
                Try : frame.Dispose() : Catch : End Try
            Loop
            Try : _signal.Dispose() : Catch : End Try
        End Sub
    End Class

End Namespace
