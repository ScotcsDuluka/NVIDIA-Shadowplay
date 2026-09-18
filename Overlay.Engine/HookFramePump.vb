' HookFramePump.vb — streams the osc page render to hooked games.
'
' The WebView2 child window (Chromium surface showing the live osc page —
' dim+menu when open, transparent/nothing when closed) is BitBlt'ed into a
' shared-memory frame at ~30fps. The in-game mod (MelonMod) reads the frame
' and draws it INSIDE the game's own render, so the overlay works even in
' exclusive fullscreen. Format: BGRA32 (Unity TextureFormat.BGRA32 reads it
' directly — no swizzle). Frame header: int32 magic, width, height,
' frameId (writer increments after the pixels are complete).

Imports System
Imports System.IO.MemoryMappedFiles
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class HookFramePump

    Public Const MmfName As String = "NVIDIA_Share_Overlay_Frame_v1"
    Private Const HeaderBytes As Integer = 64
    Private Const Magic As Integer = &H3250534E          ' "NSP2"
    Private Const RendererCapabilityOffset As Long = 61
    ''' <summary>Written into the frame header (+16) so the in-game DLL
    '     knows whether the overlay should draw. Set by OscHostForm.</summary>
    Public Shared OverlayVisible As Integer = 0

    ''' <summary>True while the injected in-game DLL is alive (it increments
    '     header[+20] on every Present). When live, Alt+Z drives the overlay
    '     purely through the header — the external window hides, the game
    '     keeps focus.</summary>
    Public Shared Function HookLive() As Boolean
        Try
            If _pubMmf Is Nothing Then
                _pubMmf = MemoryMappedFile.OpenExisting(MmfName, MemoryMappedFileRights.Read)
            End If
            If _viewPub Is Nothing Then
                _viewPub = _pubMmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read)
            End If
            Dim count As Integer = _viewPub.ReadInt32(20)
            Dim nowTick As Integer = Environment.TickCount
            ' frozen counter = the game (or the hook) is gone
            Dim alive As Boolean = (count <> _lastLiveCount) OrElse
                                   (count > 0 AndAlso nowTick - _lastLiveTick < 3000)
            If count <> _lastLiveCount Then _lastLiveTick = nowTick
            _lastLiveCount = count
            ' Native mode is selected only when the renderer has a
            ' compatible compositor. Other APIs remain observable but use
            ' the desktop fallback until their resource path is complete.
            Return alive AndAlso count > 0 AndAlso
                   _viewPub.ReadByte(RendererCapabilityOffset) = 1
        Catch
            _viewPub = Nothing
            _lastLiveCount = -1
            Return False
        End Try
    End Function
    ''' <summary>Current overlayVisible flag from the frame header — the
    '     ground truth the in-game DLL draws by.</summary>
    Public Shared Function OverlayHeaderVisible() As Boolean
        Try
            If _pubMmf Is Nothing Then
                _pubMmf = MemoryMappedFile.OpenExisting(MmfName, MemoryMappedFileRights.Read)
            End If
            If _viewPub Is Nothing Then
                _viewPub = _pubMmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read)
            End If
            Return _viewPub.ReadInt32(16) = 1
        Catch
            Return False
        End Try
    End Function

    Public Shared Function ExclusiveFullscreen() As Boolean
        Try
            If _pubMmf Is Nothing Then
                _pubMmf = MemoryMappedFile.OpenExisting(MmfName, MemoryMappedFileRights.Read)
            End If
            If _viewPub Is Nothing Then
                _viewPub = _pubMmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read)
            End If
            Return _viewPub.ReadInt32(60) = 1
        Catch
            Return False
        End Try
    End Function

    Private Shared _pubMmf As MemoryMappedFile
    Private Shared _viewPub As MemoryMappedViewAccessor
    Private Shared _lastLiveCount As Integer = -1
    Private Shared _lastLiveTick As Integer = 0

    Private ReadOnly _lock As New Object()
    Private _thread As Thread
    Private _stopFlag As Boolean
    Private _webViewHwnd As IntPtr
    Private _mmf As MemoryMappedFile

    ' GDI state (recreated on size change)
    Private _srcHdc As IntPtr
    Private _childHwnd As IntPtr
    Private _memDc As IntPtr
    Private _dib As IntPtr
    Private _bitsPtr As IntPtr
    Private _capW As Integer
    Private _capH As Integer

    Public Sub Start(webViewHwnd As IntPtr)
        _webViewHwnd = webViewHwnd
        If _thread IsNot Nothing Then Return
        _stopFlag = False
        _thread = New Thread(AddressOf PumpLoop) With {.IsBackground = True, .Name = "HookFramePump"}
        _thread.Start()
    End Sub

    Public Sub [Stop]()
        _stopFlag = True
        Try : _thread?.Join(500) : Catch : End Try
        ReleaseGdi()
        _thread = Nothing
    End Sub

    Private Sub PumpLoop()
        Dim frameId As Integer = 0
        While Not _stopFlag
            Try
                Dim child As IntPtr = FindChromeChild(_webViewHwnd)
                If child = IntPtr.Zero Then Thread.Sleep(500) : Continue While
                Dim r As RECT
                GetWindowRect(child, r)
                Dim w As Integer = r.Right - r.Left
                Dim h As Integer = r.Bottom - r.Top
                If w <= 0 OrElse h <= 0 OrElse w > 4096 OrElse h > 4096 Then Thread.Sleep(500) : Continue While

                If w <> _capW OrElse h <> _capH Then
                    ReleaseGdi()
                    If Not CreateDib(w, h) Then Thread.Sleep(500) : Continue While
                End If

                ' PW_RENDERFULLCONTENT (0x2) on the MAIN window: DWM grabs
                ' the whole window tree incl. the GPU-composited Chromium
                ' child (child-only PrintWindow returns black — measured).
                PrintWindow(_webViewHwnd, _memDc, 2)  ' main-window variant

                ' publish into the MMF: header first-written-last via frameId
                Dim mapSize As Long = HeaderBytes + CLng(w) * h * 4
                If _mmf Is Nothing Then
                    _mmf = MemoryMappedFile.CreateOrOpen(MmfName, mapSize, MemoryMappedFileAccess.ReadWrite)
                End If
                Dim view = _mmf.CreateViewAccessor(0, mapSize)
                view.Write(0, Magic)
                view.Write(4, w)
                view.Write(8, h)
                view.Write(12, frameId)
                view.Write(16, OverlayVisible)
                view.Write(24, Process.GetCurrentProcess().Id)
                view.Write(56, Environment.TickCount)
                Dim bytes((CLng(w) * h * 4) - 1) As Byte
                Marshal.Copy(_bitsPtr, bytes, 0, bytes.Length)
                view.WriteArray(HeaderBytes, bytes, 0, bytes.Length)
                view.Write(12, frameId)          ' frameId last = frame complete
                view.Flush()
                view.Dispose()
                frameId += 1
                Thread.Sleep(33)                 ' ~30fps
            Catch
                ReleaseGdi()
                Thread.Sleep(1000)
            End Try
        End While
    End Sub

    Private Function CreateDib(w As Integer, h As Integer) As Boolean
        Dim bmi As New BITMAPINFO()
        bmi.bmiHeader.biSize = Marshal.SizeOf(GetType(BITMAPINFOHEADER))
        bmi.bmiHeader.biWidth = w
        bmi.bmiHeader.biHeight = -h            ' top-down
        bmi.bmiHeader.biPlanes = 1
        bmi.bmiHeader.biBitCount = 32
        bmi.bmiHeader.biCompression = 0        ' BI_RGB
        Dim screenDc As IntPtr = GetDC(IntPtr.Zero)
        _dib = CreateDIBSection(screenDc, bmi, DIB_RGB_COLORS, _bitsPtr, IntPtr.Zero, 0)
        ReleaseDC(IntPtr.Zero, screenDc)
        If _dib = IntPtr.Zero Then Return False
        _memDc = CreateCompatibleDC(screenDc)
        Dim old As IntPtr = SelectObject(_memDc, _dib)
        DeleteObject(old)
        _capW = w
        _capH = h
        Return True
    End Function

    Private Sub ReleaseGdi()
        If _memDc <> IntPtr.Zero Then DeleteDC(_memDc) : _memDc = IntPtr.Zero
        If _dib <> IntPtr.Zero Then DeleteObject(_dib) : _dib = IntPtr.Zero
        If _srcHdc <> IntPtr.Zero Then ReleaseDC(_childHwnd, _srcHdc) : _srcHdc = IntPtr.Zero
        _mmf = Nothing
    End Sub

    ''' <summary>Finds the Chromium child window of the WebView2 control
    '     (Chrome_WidgetWin_*), else the first descendant.</summary>
    Private Shared Function FindChromeChild(root As IntPtr) As IntPtr
        If root = IntPtr.Zero Then Return IntPtr.Zero
        Dim found As IntPtr = FindWindowEx(root, IntPtr.Zero, "Chrome_WidgetWin_0", Nothing)
        If found = IntPtr.Zero Then found = FindWindowEx(root, IntPtr.Zero, Nothing, Nothing)
        Return found
    End Function

    ' ── GDI interop ────────────────────────────────────────────
    Private Const SRCCOPY As Integer = &HCC0020
    Private Const DIB_RGB_COLORS As Integer = 0

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BITMAPINFOHEADER
        Public biSize As UInteger
        Public biWidth As Integer
        Public biHeight As Integer
        Public biPlanes As UShort
        Public biBitCount As UShort
        Public biCompression As UInteger
        Public biSizeImage As UInteger
        Public biXPelsPerMeter As Integer
        Public biYPelsPerMeter As Integer
        Public biClrUsed As UInteger
        Public biClrImportant As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BITMAPINFO
        Public bmiHeader As BITMAPINFOHEADER
        Public bmiColors As UInteger
    End Structure

    <DllImport("user32.dll")>
    Private Shared Function GetWindowDC(hWnd As IntPtr) As IntPtr
    End Function
    <DllImport("user32.dll")>
    Private Shared Function ReleaseDC(hWnd As IntPtr, hDc As IntPtr) As Integer
    End Function
    <DllImport("user32.dll")>
    Private Shared Function GetWindowRect(hWnd As IntPtr, ByRef rect As RECT) As Boolean
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function FindWindowEx(parent As IntPtr, after As IntPtr, className As String, windowName As String) As IntPtr
    End Function
    <DllImport("gdi32.dll")>
    Private Shared Function BitBlt(hdcDest As IntPtr, x As Integer, y As Integer, w As Integer, h As Integer, hdcSrc As IntPtr, x1 As Integer, y1 As Integer, rop As Integer) As Boolean
    End Function
    <DllImport("gdi32.dll")>
    Private Shared Function CreateCompatibleDC(hdc As IntPtr) As IntPtr
    End Function
    <DllImport("gdi32.dll")>
    Private Shared Function CreateDIBSection(hdc As IntPtr, bmi As BITMAPINFO, usage As Integer, ByRef bits As IntPtr, hSection As IntPtr, offset As Integer) As IntPtr
    End Function
    <DllImport("gdi32.dll")>
    Private Shared Function SelectObject(hdc As IntPtr, obj As IntPtr) As IntPtr
    End Function
    <DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(obj As IntPtr) As Boolean
    End Function
    <DllImport("gdi32.dll")>
    Private Shared Function DeleteDC(hdc As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")>
    Private Shared Function GetDC(hWnd As IntPtr) As IntPtr
    End Function
    <DllImport("user32.dll")>
    Private Shared Function PrintWindow(hWnd As IntPtr, hdcBlt As IntPtr, nFlags As UInteger) As Boolean
    End Function

End Class
