Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading
Imports CaptureEngine.Diagnostics

Namespace CaptureEngine.Video.Backends.Native.Dxgi
    ''' <summary>
    ''' DxgiCaptureBackend — skeleton for DXGI Desktop Duplication capture.
    '''
    ''' ARCHITECTURE:
    '''   This backend captures the Windows desktop using DXGI Output Duplication
    '''   API (IDXGIOutputDuplication). Frames are acquired as D3D11 textures
    '''   (GPU-resident) and pushed to an IVideoFrameSink.
    '''
    '''   The D3D11 device is BORROWED from the engine (not owned by the backend).
    '''   This enables zero-copy NVENC registration on the same device.
    '''
    ''' D3D11 DEVICE OWNERSHIP:
    '''   - The backend receives a D3D11 device pointer via Initialize()
    '''   - The backend creates an IDXGIOutputDuplication on that device
    '''   - The backend does NOT dispose the D3D11 device — the engine owns it
    '''   - On Stop/Dispose, the backend releases IDXGIOutputDuplication + textures
    '''   - The D3D11 device remains alive after backend Dispose (engine owns it)
    '''
    ''' GPU RESOURCE LIFETIME:
    '''   - IDXGIOutputDuplication: Created in Initialize, released in Stop/Dispose
    '''   - Staging texture (D3D11Texture2D): Created in Start, released in Stop/Dispose
    '''   - Desktop texture (from AcquireNextFrame): Acquired per-frame, released per-frame
    '''   - All GPU resources are released before Dispose returns
    '''
    ''' FUTURE NVENC COMPATIBILITY:
    '''   - The D3D11 device handle is exposed via INativeCaptureBackend.D3D11DeviceHandle
    '''   - The adapter LUID is exposed via INativeCaptureBackend.AdapterLuid
    '''   - NVENC can verify same-GPU compatibility using LUID
    '''   - The staging texture can be registered with NvEncRegisterResource
    '''     for zero-copy encode (validated in V1 spike)
    '''
    ''' THREADING MODEL:
    '''   - Initialize, Start, Stop, Dispose: called from engine thread (single)
    '''   - Worker thread: internal, captures frames via AcquireNextFrame
    '''   - Diagnostics properties: thread-safe via SyncLock
    '''   - Dispose does NOT hold _sync across worker.Join() (P1-B.1 FIX pattern)
    '''
    ''' CURRENT STATUS: SKELETON ONLY
    '''   - All methods throw NotImplementedException or return placeholder values
    '''   - No actual DXGI calls are made
    '''   - No D3D11 device is created or borrowed
    '''   - Architecture is documented for future implementation
    ''' </summary>
    Public NotInheritable Class DxgiCaptureBackend
        Implements INativeCaptureBackend
        Implements IVideoBackendDiagnostics

        ' ---- backend state ----
        Private ReadOnly _sync As New Object()
        Private ReadOnly _logger As EngineLogger

        Private _state As NativeBackendState = NativeBackendState.Created
        Private _disposed As Boolean = False

        ' ---- D3D11 device (borrowed, NOT owned) ----
        Private _d3d11DeviceHandle As IntPtr = IntPtr.Zero
        Private _adapterLuid As Long? = Nothing

        ' ---- worker ----
        Private _workerThread As Thread
        Private _stopSignal As Boolean = False
        Private _sink As IVideoFrameSink

        ' ---- diagnostics ----
        Private _emittedFrames As Long = 0
        Private _droppedFrames As Long = 0
        Private _replacedFrames As Long = 0
        Private _noFrameCount As Long = 0
        Private _errorCount As Long = 0

        ' ---- config placeholders ----
        Private _outputIndex As Integer = 0
        Private _captureWidth As Integer = 0
        Private _captureHeight As Integer = 0

#Region "Construction"

        Public Sub New(Optional logger As EngineLogger = Nothing)
            _logger = If(logger, New EngineLogger("DxgiCaptureBackend"))
        End Sub

#End Region

#Region "INativeCaptureBackend (extends IVideoCaptureBackend)"

        ''' <summary>
        ''' The D3D11 device handle this backend uses for capture.
        ''' Returns IntPtr.Zero before Initialize and after Dispose.
        ''' The device is BORROWED from the engine — this backend does NOT own it.
        ''' </summary>
        Public ReadOnly Property D3D11DeviceHandle As IntPtr Implements INativeCaptureBackend.D3D11DeviceHandle
            Get
                SyncLock _sync
                    Return _d3d11DeviceHandle
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' The GPU adapter LUID. Used to verify capture+encode are on the same GPU.
        ''' Returns Nothing before Initialize.
        ''' </summary>
        Public ReadOnly Property AdapterLuid As Long? Implements INativeCaptureBackend.AdapterLuid
            Get
                SyncLock _sync
                    Return _adapterLuid
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Whether the backend currently has a live D3D11 capture session.
        ''' True between Start and Stop/Dispose.
        ''' </summary>
        Public ReadOnly Property IsCapturing As Boolean Implements INativeCaptureBackend.IsCapturing
            Get
                SyncLock _sync
                    Return _state = NativeBackendState.Running
                End SyncLock
            End Get
        End Property

#End Region

#Region "IVideoCaptureBackend"

        Public ReadOnly Property Diagnostics As IVideoBackendDiagnostics Implements IVideoCaptureBackend.Diagnostics
            Get
                Return Me
            End Get
        End Property

        Public Sub Initialize(context As IVideoBackendContext) Implements IVideoCaptureBackend.Initialize
            If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))

            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(DxgiCaptureBackend))
                End If
                If _state <> NativeBackendState.Created Then
                    Throw New InvalidOperationException(
                        "Initialize cannot be called from state '" & _state.ToString() & "'.")
                End If
                _state = NativeBackendState.Initialized
            End SyncLock

            ' SKELETON: No actual DXGI/D3D11 calls.
            ' Future implementation will:
            ' 1. Borrow D3D11 device from context (or create one)
            ' 2. Enumerate DXGI adapters, find NVIDIA adapter
            ' 3. Select output (monitor) by index
            ' 4. Create IDXGIOutputDuplication
            ' 5. Store device handle + adapter LUID
            ' 6. Query desktop dimensions

            _logger.Info("DxgiCaptureBackend: Initialize (skeleton — no DXGI calls)")
        End Sub

        Public Sub Start(sink As IVideoFrameSink) Implements IVideoCaptureBackend.Start
            If sink Is Nothing Then Throw New ArgumentNullException(NameOf(sink))

            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(DxgiCaptureBackend))
                End If
                Select Case _state
                    Case NativeBackendState.Starting, NativeBackendState.Running
                        Return ' idempotent
                    Case NativeBackendState.Initialized, NativeBackendState.Stopped
                        _state = NativeBackendState.Starting
                    Case Else
                        Throw New InvalidOperationException(
                            "Start cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            _sink = sink
            _stopSignal = False

            ' SKELETON: No actual worker thread.
            ' Future implementation will:
            ' 1. Create staging texture (D3D11Texture2D with D3D11_BIND_RENDER_TARGET)
            ' 2. Start worker thread that calls AcquireNextFrame in a loop
            ' 3. Copy desktop texture → staging texture (CopySubresourceRegion)
            ' 4. Push FrameAcquisitionResult to sink
            ' 5. Release desktop texture + ReleaseFrame per iteration

            SyncLock _sync
                _state = NativeBackendState.Running
            End SyncLock

            _logger.Info("DxgiCaptureBackend: Start (skeleton — no worker thread)")
        End Sub

        Public Sub [Stop]() Implements IVideoCaptureBackend.Stop
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(DxgiCaptureBackend))
                End If
                Select Case _state
                    Case NativeBackendState.Created, NativeBackendState.Initialized,
                         NativeBackendState.Stopped, NativeBackendState.Stopping
                        Return ' no-op
                    Case NativeBackendState.Faulted
                        Return ' no-op
                    Case NativeBackendState.Running
                        _state = NativeBackendState.Stopping
                    Case Else
                        Throw New InvalidOperationException(
                            "Stop cannot be called from state '" & _state.ToString() & "'.")
                End Select
            End SyncLock

            _stopSignal = True

            ' SKELETON: No actual worker to join.
            ' Future implementation will:
            ' 1. Set _stopSignal = True
            ' 2. Join worker thread (outside _sync lock — P1-B.1 FIX pattern)
            ' 3. Release IDXGIOutputDuplication
            ' 4. Release staging texture
            ' 5. Do NOT release D3D11 device (borrowed, not owned)

            SyncLock _sync
                _state = NativeBackendState.Stopped
            End SyncLock

            _logger.Info("DxgiCaptureBackend: Stop (skeleton)")
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dim needStop As Boolean = False

            SyncLock _sync
                If _disposed Then Return
                _disposed = True

                If _state = NativeBackendState.Running OrElse
                   _state = NativeBackendState.Starting Then
                    _state = NativeBackendState.Stopping
                    _stopSignal = True
                    needStop = True
                End If
            End SyncLock

            ' SKELETON: No worker to join.
            ' Future implementation will join worker OUTSIDE _sync (P1-B.1 FIX pattern).

            SyncLock _sync
                _d3d11DeviceHandle = IntPtr.Zero
                _adapterLuid = Nothing
                _state = NativeBackendState.Disposed
            End SyncLock

            _logger.Info("DxgiCaptureBackend: Dispose (skeleton)")
        End Sub

#End Region

#Region "IVideoBackendDiagnostics"

        Public ReadOnly Property EmittedFrames As Long Implements IVideoBackendDiagnostics.EmittedFrames
            Get
                SyncLock _sync
                    Return _emittedFrames
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property DroppedFrames As Long Implements IVideoBackendDiagnostics.DroppedFrames
            Get
                SyncLock _sync
                    Return _droppedFrames
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ReplacedFrames As Long Implements IVideoBackendDiagnostics.ReplacedFrames
            Get
                SyncLock _sync
                    Return _replacedFrames
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property NoFrameCount As Long Implements IVideoBackendDiagnostics.NoFrameCount
            Get
                SyncLock _sync
                    Return _noFrameCount
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ErrorCount As Long Implements IVideoBackendDiagnostics.ErrorCount
            Get
                SyncLock _sync
                    Return _errorCount
                End SyncLock
            End Get
        End Property

#End Region

#Region "Test inspection (Friend)"

        ''' <summary>Snapshot the current backend state (for tests).</summary>
        Friend ReadOnly Property CurrentState As NativeBackendState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

#End Region

    End Class
End Namespace
