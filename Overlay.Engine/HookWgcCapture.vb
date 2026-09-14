' HookWgcCapture.vb — 60fps overlay frame source via Windows.Graphics
' .Capture. Captures the WebView2 Chromium child window straight off the
' GPU (DWM surface — works while the window is occluded by the game, no
' PNG/base64 round-trip like the CDP path). Pixels land in the SAME
' shared-memory frame the in-game DLL already reads; the CDP capture
' stays as fallback when WGC is unavailable.
'
' NOTE WGC delivers PREMULTIPLIED alpha BGRA — the DLL blends with
' ONE/INV_SRC_ALPHA (vs straight-alpha PNG from CDP).

Imports System
Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Windows.Graphics
Imports Windows.Graphics.Capture
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.DirectX.Direct3D11

Public Class HookWgcCapture

    ' ── COM interop (kept to the members actually used) ────────
    <ComImport, Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IGraphicsCaptureItemInterop
        <PreserveSig>
        Function CreateForWindow(window As IntPtr, ByRef iid As Guid, ByRef item As IntPtr) As Integer
    End Interface

    <StructLayout(LayoutKind.Sequential)>
    Private Structure D3D11_TEXTURE2D_DESC
        Public Width As UInteger
        Public Height As UInteger
        Public MipLevels As UInteger
        Public ArraySize As UInteger
        Public Format As Integer
        Public SampleDescCount As UInteger
        Public SampleDescQuality As UInteger
        Public Usage As Integer
        Public BindFlags As UInteger
        Public CPUAccessFlags As UInteger
        Public MiscFlags As UInteger
    End Structure

    <ComImport, Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface ID3D11Texture2DCom
        <PreserveSig> Sub GetDesc(ByRef desc As D3D11_TEXTURE2D_DESC)
    End Interface

    <ComImport, Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface ID3D11DeviceContextCom
        Sub CopyResource(dst As IntPtr, src As IntPtr)
        Sub Map(resource As IntPtr, subresource As UInteger, mapType As UInteger, flags As UInteger, ByRef mappedRow As IntPtr) As <MarshalAs(UnmanagedType.Error)> Integer
        Sub Unmap(resource As IntPtr, subresource As UInteger)
    End Interface

    <ComImport, Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140")>
    Private Class D3DDeviceContextRCW
    End Class

    <ComImport, Guid("dc8bee63-6f76-4740-b274-22db79b6b96d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface ID3D11DeviceCom
        <PreserveSig> Function CreateTexture2D(ByRef desc As D3D11_TEXTURE2D_DESC, initialData As IntPtr, ByRef texOut As IntPtr) As Integer
        Sub GetImmediateContext(ByRef ctx As ID3D11DeviceContextCom)
    End Interface

    <ComImport, Guid("dbcbbce4-1a02-4c72-8a14-0b1b1e0e0e0e")>
    Private Class Dummy
    End Class

    <DllImport("d3d11.dll")>
    Private Shared Function D3D11CreateDevice(
        adapter As IntPtr, driverType As Integer, software As IntPtr, flags As UInteger,
        featureLevels As IntPtr, featureLevelCount As UInteger, sdkVersion As UInteger,
        ByRef deviceOut As IntPtr, ByRef featureLevelOut As Integer, ByRef contextOut As IntPtr) As Integer
    End Function

    <DllImport("d3d11.dll", EntryPoint:="CreateDirect3D11DeviceFromDXGIDevice")>
    Private Shared Function CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice As IntPtr, ByRef graphicsDeviceOut As IntPtr) As Integer
    End Function

    <DllImport("dxgi.dll")>
    Private Shared Function DXGIGetDebugInterface(riid As Guid, ByRef ppv As IntPtr) As Integer
    End Function

    <DllImport("d3d11.dll")>
    Private Shared Function D3D11DXGIDeviceQI() As IntPtr
    End Function

    ' simpler path to the DXGI device: IUnknown::QueryInterface on the
    ' D3D11 device (both interfaces live on the same object)
    <DllImport("ole32.dll")>
    Private Shared Function CoTaskMemFree(p As IntPtr)
    End Function

    <ComImport, Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IDXGIDeviceCom
    End Interface

    ' IUnknown QI via Marshal
    <DllImport("kernel32.dll")>
    Private Shared Function lstrlenW(p As IntPtr) As Integer
    End Function

    Private Const D3D11_SDK_VERSION As UInteger = 7
    Private Const D3D11_CREATE_DEVICE_BGRA_SUPPORT As UInteger = &H20
    Private Const DXGI_FORMAT_B8G8R8A8_UNORM As Integer = 87
    Private Const D3D11_USAGE_STAGING As Integer = 3
    Private Const D3D11_CPU_ACCESS_READ As UInteger = &H20000
    Private Const D3D11_MAP_READ As UInteger = 1

    ' ── state ─────────────────────────────────────────────────
    Private _webViewHwnd As IntPtr
    Private _thread As Thread
    Private _stopFlag As Boolean
    Private Shared _running As Boolean
    Private Shared _frames As Long

    ''' <summary>True when WGC is actively producing frames (CDP stays off
    '     then; it is only the fallback).</summary>
    Public Shared ReadOnly Property Running As Boolean
        Get
            Return _running
        End Get
    End Property

    Public Sub New(webViewHwnd As IntPtr)
        _webViewHwnd = webViewHwnd
    End Sub

    Public Sub Start()
        If _thread IsNot Nothing Then Return
        _stopFlag = False
        _thread = New Thread(AddressOf WgcLoop) With {.IsBackground = True, .Name = "HookWgcCapture"}
        _thread.Start()
    End Sub

    Public Sub [Stop]()
        _stopFlag = True
        Try : _thread?.Join(1000) : Catch : End Try
    End Sub

    Private Shared Sub L(m As String)
        Try
            Dim p As String = AppLayout.P("Logs", "wgc.log")
            Dim d As String = IO.Path.GetDirectoryName(p)
            If Not IO.Directory.Exists(d) Then IO.Directory.CreateDirectory(d)
            IO.File.AppendAllText(p, DateTime.Now.ToString("HH:mm:ss.fff") & " " & m & Environment.NewLine)
        Catch
        End Try
    End Sub

    Private Sub WgcLoop()
        Try
            RunCapture()
        Catch ex As Exception
            L("wgc failed: " & ex.Message & " — CDP fallback will own capture")
            _running = False
        End Try
    End Sub

    Private Sub RunCapture()
        ' 1) find the Chromium child of the WebView host window
        Dim child As IntPtr = FindChromeChild(_webViewHwnd)
        If child = IntPtr.Zero Then Throw New Exception("no chrome child window")
        L("target child window " & child.ToString())

        ' 2) D3D11 device (BGRA support) → DXGI device → WinRT IDirect3DDevice
        Dim d3dDev As IntPtr = IntPtr.Zero, ctxPtr As IntPtr = IntPtr.Zero
        Dim hr As Integer = D3D11CreateDevice(IntPtr.Zero, 1, IntPtr.Zero,
            D3D11_CREATE_DEVICE_BGRA_SUPPORT, IntPtr.Zero, 0, D3D11_SDK_VERSION,
            d3dDev, Nothing, ctxPtr)
        If hr < 0 OrElse d3dDev = IntPtr.Zero Then Throw New Exception("D3D11CreateDevice hr=0x" & hr.ToString("X8"))
        Dim dxgiDevGuid As New Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c")
        Dim dxgiDev As IntPtr = IntPtr.Zero
        hr = Marshal.QueryInterface(d3dDev, dxgiDevGuid, dxgiDev)
        If hr < 0 Then Throw New Exception("QI dxgi device hr=0x" & hr.ToString("X8"))
        Dim winrtDevIid As New Guid("a37624ab-8d5f-4650-9d3e-965ae599ab9c") ' IDirect3DDevice
        Dim winrtDevPtr As IntPtr = IntPtr.Zero
        hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDev, winrtDevPtr)
        If hr < 0 OrElse winrtDevPtr = IntPtr.Zero Then Throw New Exception("CreateDirect3D11DeviceFromDXGIDevice hr=0x" & hr.ToString("X8"))
        Dim winrtDev = CType(Marshal.GetObjectForIUnknown(winrtDevPtr), IDirect3DDevice)

        ' 3) capture item for the window (IGraphicsCaptureItemInterop)
        Dim interopGuid As New Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760")
        Dim factoryPtr As IntPtr = IntPtr.Zero
        Dim itemIid As New Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760") ' wrong on purpose? no — GraphicsCaptureItem iid:
        itemIid = New Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760")
        Dim activationName As IntPtr = Marshal.StringToHGlobalUni("Windows.Graphics.Capture.GraphicsCaptureItem")
        hr = RoGetActivationFactory(activationName, interopGuid, factoryPtr)
        Marshal.FreeHGlobal(activationName)
        If hr < 0 OrElse factoryPtr = IntPtr.Zero Then Throw New Exception("RoGetActivationFactory hr=0x" & hr.ToString("X8"))
        Dim interop = CType(Marshal.GetObjectForIUnknown(factoryPtr), IGraphicsCaptureItemInterop)
        Dim itemGuid As New Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760")
        Dim itemPtr As IntPtr = IntPtr.Zero
        ' the item interface id (IGraphicsCaptureItem, not the interop one)
        Dim realItemGuid As New Guid("362EA9FE-4EBA-4C79-BF55-47CE0A87A6F5")
        hr = interop.CreateForWindow(child, realItemGuid, itemPtr)
        If hr < 0 OrElse itemPtr = IntPtr.Zero Then Throw New Exception("CreateForWindow hr=0x" & hr.ToString("X8"))
        Dim item = GraphicsCaptureItem.FromAbi(itemPtr)

        ' 4) frame pool + session
        Dim size As SizeInt32 = item.Size
        Dim pool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            winrtDev, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, size)
        Dim session = pool.CreateCaptureSession(item)
        Try
            session.IsCursorCaptureEnabled = False
        Catch
        End Try
        Try
            session.IsBorderRequired = False
        Catch
        End Try

        Dim frameCount As Long = 0
        Dim pubView As MemoryMappedViewAccessor = Nothing
        Dim pubMmf As MemoryMappedFile = Nothing
        Dim stageTex As IntPtr = IntPtr.Zero
        Dim d3dCtx As ID3D11DeviceContextCom = CType(Marshal.GetObjectForIUnknown(ctxPtr), ID3D11DeviceContextCom)
        Dim devCom = CType(Marshal.GetObjectForIUnknown(d3dDev), ID3D11DeviceCom)
        Dim lastW As Integer = 0, lastH As Integer = 0

        AddHandler pool.FrameArrived, Sub(sender, e)
                                           Try
                                               Using frame As Direct3D11CaptureFrame = sender.TryGetNextFrame()
                                                   If frame Is Nothing Then Return
                                                   If HookCdpCapture.CaptureEnabled = 0 Then Return
                                                   Dim s As SizeInt32 = frame.ContentSize
                                                   If s.Width <= 0 OrElse s.Height <= 0 Then Return
                                                   If s.Width <> lastW OrElse s.Height <> lastH Then
                                                       If stageTex <> IntPtr.Zero Then Marshal.Release(stageTex) : stageTex = IntPtr.Zero
                                                       lastW = s.Width : lastH = s.Height
                                                   End If

                                                   ' surface → ID3D11Texture2D
                                                   Dim surfObj As IDirect3DSurface = frame.Surface
                                                   Dim dxgiIid As New Guid("a37624ab-8d5f-4650-9d3e-965ae599ab9c")
                                                   Dim surfPtr As IntPtr = IntPtr.Zero
                                                   surfPtr = surfObj.As<IDirect3DDxgiInterfaceAccess>().
                                                       GetInterface(New Guid("9c03f410-0000-4b1f-83f0-a2de85b1a2e1"))
                                                   Dim frameTex As IntPtr = surfPtr

                                                   ' staging texture for CPU readback
                                                   If stageTex = IntPtr.Zero Then
                                                       Dim d As New D3D11_TEXTURE2D_DESC()
                                                       d.Width = CUInt(lastW) : d.Height = CUInt(lastH)
                                                       d.MipLevels = 1 : d.ArraySize = 1
                                                       d.Format = DXGI_FORMAT_B8G8R8A8_UNORM
                                                       d.SampleDescCount = 1
                                                       d.Usage = D3D11_USAGE_STAGING
                                                       d.CPUAccessFlags = D3D11_CPU_ACCESS_READ
                                                       Dim hr2 As Integer = devCom.CreateTexture2D(d, IntPtr.Zero, stageTex)
                                                       If hr2 < 0 Then L("CreateTexture2D staging hr=0x" & hr2.ToString("X8")) : Return
                                                   End If

                                                   d3dCtx.CopyResource(stageTex, frameTex)
                                                   Dim mapped As IntPtr = IntPtr.Zero
                                                   d3dCtx.Map(stageTex, 0, D3D11_MAP_READ, 0, mapped)
                                                   If mapped <> IntPtr.Zero Then
                                                       HookCdpCapture.PublishPixels(lastW, lastH, mapped, lastW * 4)
                                                       d3dCtx.Unmap(stageTex, 0)
                                                   End If
                                                   Marshal.Release(frameTex)
                                                   frameCount += 1
                                                   If frameCount = 1 OrElse frameCount Mod 300 = 0 Then
                                                       L("frame #" & frameCount & " " & lastW & "x" & lastH)
                                                   End If
                                               End Using
                                           Catch ex As Exception
                                               Static errLog As Integer
                                               errLog += 1
                                               If errLog <= 5 Then L("frame error: " & ex.Message)
                                           End Try
                                       End Sub

        session.StartCapture()
        _running = True
        L("capture started " & size.Width & "x" & size.Height)

        While Not _stopFlag
            Thread.Sleep(500)
        End While

        Try : session.Dispose() : Catch : End Try
        Try : pool.Dispose() : Catch : End Try
        _running = False
        L("capture stopped after " & frameCount & " frames")
    End Sub

    <DllImport("combase.dll")>
    Private Shared Function RoGetActivationFactory(name As IntPtr, ByRef iid As Guid, ByRef factory As IntPtr) As Integer
    End Function

    ''' <summary>Finds the Chromium child window of the WebView2 control
    '''     (Chrome_WidgetWin_*), else the first descendant.</summary>
    Private Shared Function FindChromeChild(root As IntPtr) As IntPtr
        If root = IntPtr.Zero Then Return IntPtr.Zero
        Dim found As IntPtr = FindWindowEx(root, IntPtr.Zero, "Chrome_WidgetWin_0", Nothing)
        If found = IntPtr.Zero Then found = FindWindowEx(root, IntPtr.Zero, Nothing, Nothing)
        Return found
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function FindWindowEx(parent As IntPtr, after As IntPtr, className As String, windowName As String) As IntPtr
    End Function

End Class
