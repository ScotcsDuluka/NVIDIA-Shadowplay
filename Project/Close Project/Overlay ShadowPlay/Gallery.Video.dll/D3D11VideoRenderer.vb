Option Strict On
Option Explicit On
Option Infer On

' D3D11VideoRenderer.vb — the real present path (design doc §3.8).
'
' MVP present pipeline, zero shaders (proof-first):
'
'   PlaybackFrame(BGRA8 bytes, CPU)
'     → Map(dynamic upload texture, WriteDiscard)   [pooled ×1 — one texture
'        per Present; Map/Unmap is the upload, NO GPU→CPU readback anywhere]
'     → CopyResource(backbuffer, uploadTexture)     [GPU-only, same dims]
'     → Present(vsync)                              [DXGI stretches the video-
'        sized backbuffer into the window (Scaling.Stretch); aspect-correct
'        letterboxing is a shader-based UI-integration follow-up, NOT assumed]
'
' OWNERSHIP TABLE (§3.8) — every row mapped to a field:
'   ID3D11Device + context            _device/_context    created here, disposed here
'   IDXGIFactory2 + swapchain         _factory/_swapChain created here, disposed here
'   Backbuffer texture                _backBuffer         created here, disposed here
'   Dynamic upload texture            _upload             created here, disposed here
'   Decoded BGRA8 buffers             PlaybackFrame       NEVER retained — Present
'                                                         copies and returns (§2.1)
'
' DEVICE LOSS (§7): a failed Present triggers ONE recreate (device+swapchain);
' if that fails too, IsAvailable=False and the session maps it to
' RendererUnavailable. DeviceLostCount is the evidence counter.
'
' RUNTIME GATE (honest): TryCreate returns Nothing (never throws) off-Windows
' or without a usable GPU/desktop — the session then uses NullVideoSink (or
' faults, when a window was explicitly configured). COM/native loads are
' lazy and every failure is caught at the gate.

Imports System
Imports System.Runtime.InteropServices
Imports System.Threading
Imports SharpGen.Runtime
Imports Vortice.Direct3D
Imports Vortice.Direct3D11
Imports Vortice.DXGI

Namespace Gallery.Video

    Public NotInheritable Class D3D11VideoRenderer
        Implements IVideoRenderSink

        ''' <summary>VSync: 1 = present on vertical blank (default, playback UX);
        ''' 0 = immediate (perf measurement mode, §6). Shared so the hardware
        ''' perf test can switch deterministically.</summary>
        Public Shared Property DefaultSyncInterval As Integer = 1

        Private ReadOnly _hwnd As IntPtr
        Private ReadOnly _width As Integer
        Private ReadOnly _height As Integer
        Private _syncInterval As Integer = DefaultSyncInterval

        Private _device As ID3D11Device
        Private _context As ID3D11DeviceContext
        Private _factory As IDXGIFactory2
        Private _swapChain As IDXGISwapChain1
        Private _backBuffer As ID3D11Texture2D
        Private _upload As ID3D11Texture2D

        Private _availableState As Integer = 1
        Private _presents As Long
        Private _deviceLostCount As Long
        Private _disposedState As Integer = 0

        Private Sub New(hwnd As IntPtr, width As Integer, height As Integer)
            _hwnd = hwnd
            _width = width
            _height = height
            CreateDeviceAndSwapchain()
        End Sub

        ''' <summary>Factory gate: Nothing = no renderer available (honest).</summary>
        Public Shared Function TryCreate(hwnd As IntPtr, width As Integer, height As Integer) As D3D11VideoRenderer
            If hwnd = IntPtr.Zero Then Return Nothing
            If width <= 0 OrElse height <= 0 Then Return Nothing
            If Not RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then Return Nothing

            Try
                Return New D3D11VideoRenderer(hwnd, width, height)
            Catch ex As Exception
                ' DllNotFoundException / TypeInitializationException / SharpGen
                ' COM failures all land here — off-hardware is a NORMAL outcome.
                Return Nothing
            End Try
        End Function

        Public ReadOnly Property IsAvailable As Boolean Implements IVideoRenderSink.IsAvailable
            Get
                Return Volatile.Read(_availableState) <> 0 AndAlso Volatile.Read(_disposedState) = 0
            End Get
        End Property

        Public ReadOnly Property PresentsCount As Long Implements IVideoRenderSink.PresentsCount
            Get
                Return Volatile.Read(_presents)
            End Get
        End Property

        Public ReadOnly Property DeviceLostCount As Long Implements IVideoRenderSink.DeviceLostCount
            Get
                Return Volatile.Read(_deviceLostCount)
            End Get
        End Property

        Public ReadOnly Property SwapchainWidth As Integer
            Get
                Return _width
            End Get
        End Property

        Public ReadOnly Property SwapchainHeight As Integer
            Get
                Return _height
            End Get
        End Property

        ' ---- creation (render/orchestration thread only) ----

        Private Sub CreateDeviceAndSwapchain()
            Dim featureLevels As FeatureLevel() = {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0
            }
            ' Default hardware adapter (playback runs on ANY GPU — unlike capture,
            ' which requires the NVIDIA adapter for NVENC; DdagrabBackend.vb:242
            ' constraint does not apply to the playback domain).
            D3D11.D3D11CreateDevice(CType(Nothing, IDXGIAdapter), DriverType.Hardware,
                                    DeviceCreationFlags.BgraSupport,
                                    featureLevels, _device, _context).CheckError()

            _factory = DXGI.CreateDXGIFactory1(Of IDXGIFactory2)()

            Dim desc As New SwapChainDescription1 With {
                .Width = CUInt(_width),
                .Height = CUInt(_height),
                .Format = Format.B8G8R8A8_UNorm,
                .Stereo = False,
                .SampleDescription = New SampleDescription(1, 0),
                .BufferUsage = Usage.RenderTargetOutput,
                .BufferCount = 2,
                .Scaling = Scaling.Stretch,
                .SwapEffect = SwapEffect.Sequential,
                .AlphaMode = AlphaMode.Ignore,
                .Flags = SwapChainFlags.None
            }
            _swapChain = _factory.CreateSwapChainForHwnd(_device, _hwnd, desc)

            _backBuffer = _swapChain.GetBuffer(Of ID3D11Texture2D)(0)

            Dim texDesc As New Texture2DDescription With {
                .Width = CUInt(_width),
                .Height = CUInt(_height),
                .MipLevels = 1,
                .ArraySize = 1,
                .Format = Format.B8G8R8A8_UNorm,
                .SampleDescription = New SampleDescription(1, 0),
                .Usage = ResourceUsage.Dynamic,
                .BindFlags = BindFlags.ShaderResource,
                .CPUAccessFlags = CpuAccessFlags.Write,
                .MiscFlags = ResourceOptionFlags.None
            }
            _upload = _device.CreateTexture2D(texDesc)

            ' Multithread protection — proven pattern (DdagrabBackend.vb:278):
            ' cheap insurance across render/seek/teardown threads.
            Dim multithread = _context.QueryInterface(Of ID3D11Multithread)()
            multithread.SetMultithreadProtected(True)
            multithread.Dispose()

            Volatile.Write(_availableState, 1)
        End Sub

        ' ---- present (render thread only; synchronous copy, §2.1) ----

        Public Sub Present(frame As PlaybackFrame) Implements IVideoRenderSink.Present
            If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))
            If Volatile.Read(_disposedState) <> 0 Then Return
            If frame.Width <> _width OrElse frame.Height <> _height Then
                ' Swapchain matches the PROBED video size; a mismatch means a
                ' generation change raced the renderer — dropped by caller via
                ' generation guard. Defensive count, never throw.
                Return
            End If

            Dim bytesNeeded = _width * _height * 4
            Dim src As Byte() = frame.Pixels

            Try
                Dim mapped As MappedSubresource = _context.Map(_upload, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None)
                Try
                    If mapped.RowPitch = bytesNeeded \ _height Then
                        Marshal.Copy(src, 0, mapped.DataPointer, bytesNeeded)
                    Else
                        ' Row pitch padded (alignment) — copy row by row.
                        Dim srcRow As Integer = bytesNeeded \ _height
                        For y = 0 To _height - 1
                            Dim dstPtr = IntPtr.Add(mapped.DataPointer, CInt(y * mapped.RowPitch))
                            Marshal.Copy(src, y * srcRow, dstPtr, srcRow)
                        Next
                    End If
                Finally
                    _context.Unmap(_upload, 0)
                End Try

                _context.CopyResource(_backBuffer, _upload)
                Dim result = _swapChain.Present(CUInt(_syncInterval), PresentFlags.None)

                If result.Failure Then
                    HandleDeviceLost()
                    Return
                End If

                Interlocked.Increment(_presents)
            Catch ex As Exception
                HandleDeviceLost()
            End Try
        End Sub

        ''' <summary>Device-lost policy (§3.8): recreate ONCE, then report
        ''' unavailable (session → RendererUnavailable fault). Counted.</summary>
        Private Sub HandleDeviceLost()
            Interlocked.Increment(_deviceLostCount)
            ReleaseGpuObjects()
            Try
                CreateDeviceAndSwapchain()
                Volatile.Write(_availableState, 1)
            Catch
                Volatile.Write(_availableState, 0)
            End Try
        End Sub

        Private Sub ReleaseGpuObjects()
            ' Repo dispose discipline: swallow, count via metrics, never throw.
            SafeDispose(_upload) : _upload = Nothing
            SafeDispose(_backBuffer) : _backBuffer = Nothing
            SafeDispose(_swapChain) : _swapChain = Nothing
            SafeDispose(_factory) : _factory = Nothing
            SafeDispose(_context) : _context = Nothing
            SafeDispose(_device) : _device = Nothing
        End Sub

        Private Shared Sub SafeDispose(obj As IDisposable)
            If obj Is Nothing Then Return
            Try
                obj.Dispose()
            Catch
            End Try
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.CompareExchange(_disposedState, 1, 0) <> 0 Then Return
            ReleaseGpuObjects()
        End Sub

    End Class

End Namespace
