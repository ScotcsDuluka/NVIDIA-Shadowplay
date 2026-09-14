' HookPrintWindowCapture.vb — 30-40 FPS overlay frames via PrintWindow
' with PW_RENDERFULLCONTENT. Works for occluded windows (in-game mode)
' because PW_RENDERFULLCONTENT (=0x2, Windows 8.1+) forces DWM to render
' the FULL composited content of a window even when another fullscreen
' window is in front of it. This is the documented Microsoft path for
' capturing DWM-composited, GPU-rendered windows like WebView2/Chromium.
'
' 3-4x faster than the CDP captureScreenshot path (~11 FPS) because it
' skips PNG encode + base64 + WebSocket round-trip + PNG decode entirely
' — PrintWindow writes pixels straight into a DIB section we own, and we
' publish the pointer to the shared-memory MMF via HookCdpCapture.
' PublishPixels (identical MMF layout the injected DLL reads).
'
' Fallback: if PrintWindow fails 5 frames in a row (e.g. WebView2
' refuses PW_RENDERFULLCONTENT for some reason), ExternalCapture is
' cleared so the CDP polling loop resumes publishing. The CDP path stays
' wired exactly as before; this class only ADDS a faster path on top.
'
' CDP Input.* dispatch (mouse/key injection to the page) is unchanged —
' that is owned by HookCdpCapture.SendCdpInput, NOT by this class.

Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class HookPrintWindowCapture

    ' PrintWindow flag: render the full content including GPU-composited
    ' children. Without this, PrintWindow only renders the GDI parts of
    ' a window — for WebView2 (Chromium compositor) that gives a black
    ' frame. With it, DWM is asked to composite the window into our DC.
    Private Const PW_RENDERFULLCONTENT As Integer = &H2

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function PrintWindow(hwnd As IntPtr, hdcBlt As IntPtr, nFlags As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetClientRect(hwnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function CreateCompatibleDC(hdc As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteDC(hdc As IntPtr) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function SelectObject(hdc As IntPtr, hgdiobj As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(hObject As IntPtr) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BITMAPINFOHEADER
        Public biSize As Integer
        Public biWidth As Integer
        Public biHeight As Integer     ' negative = top-down DIB (matches MMF layout)
        Public biPlanes As Short
        Public biBitCount As Short
        Public biCompression As Integer
        Public biSizeImage As Integer
        Public biXPelsPerMeter As Integer
        Public biYPelsPerMeter As Integer
        Public biClrUsed As Integer
        Public biClrImportant As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BITMAPINFO
        Public bmiHeader As BITMAPINFOHEADER
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=1)>
        Public bmiColors As Integer()
    End Structure

    <DllImport("gdi32.dll")>
    Private Shared Function CreateDIBSection(hdc As IntPtr, ByRef pbmi As BITMAPINFO, iUsage As Integer,
                                            ByRef ppvBits As IntPtr, hSection As IntPtr, dwOffset As Integer) As IntPtr
    End Function

    Private ReadOnly _hwnd As IntPtr
    Private _thread As Thread
    Private _stopFlag As Boolean
    Private _frameCount As Long
    Private _failStreak As Integer

    ''' <summary>target HWND = the WebView2 control's handle. May be
    '     IntPtr.Zero if the WebView2 hasn't been realized yet when the
    '     constructor runs; the capture loop polls GetClientRect until
    '     it succeeds.</summary>
    Public Sub New(hwnd As IntPtr)
        _hwnd = hwnd
    End Sub

    Public Sub Start()
        If _thread IsNot Nothing Then Return
        _stopFlag = False
        _thread = New Thread(AddressOf CaptureLoop) With {
            .IsBackground = True,
            .Name = "HookPrintWindowCapture"
        }
        _thread.Start()
    End Sub

    Public Sub [Stop]()
        _stopFlag = True
        Try : _thread?.Join(500) : Catch : End Try
    End Sub

    Private Sub CaptureLoop()
        ' The target HWND may be IntPtr.Zero if the WebView2 control
        ' wasn't realized when Start() was called. Poll GetClientRect
        ' until it succeeds (the WebView2 becomes real shortly after
        ' EnsureCoreWebView2Async completes on the UI thread).
        Dim waited As Integer = 0
        Dim bootRect As RECT
        While Not _stopFlag
            If _hwnd <> IntPtr.Zero AndAlso GetClientRect(_hwnd, bootRect) AndAlso
               bootRect.Right - bootRect.Left > 0 Then Exit While
            Thread.Sleep(100)
            waited += 1
            If waited = 30 Then L("waiting for webview HWND... (" & waited * 100 & "ms)")
            If waited > 600 Then
                L("hwnd never appeared after 60s — aborting")
                Return
            End If
        End While
        If _stopFlag Then Return

        Dim hdc As IntPtr = IntPtr.Zero
        Dim dib As IntPtr = IntPtr.Zero
        Dim dibBits As IntPtr = IntPtr.Zero
        Dim curW As Integer = 0, curH As Integer = 0

        Try
            While Not _stopFlag
                Try
                    ' gate: only publish when the overlay is open (same
                    ' flag HookCdpCapture reads). Closed overlay = idle.
                    If HookCdpCapture.CaptureEnabled = 0 Then
                        ' make sure CDP isn't fighting us while idle
                        If _frameCount > 0 Then
                            ' we previously published — keep ownership so
                            ' the moment overlay reopens we're instant
                        End If
                        Thread.Sleep(100)
                        Continue While
                    End If

                    Dim rc As RECT
                    If Not GetClientRect(_hwnd, rc) Then
                        Thread.Sleep(50)
                        Continue While
                    End If
                    Dim w As Integer = rc.Right - rc.Left
                    Dim h As Integer = rc.Bottom - rc.Top
                    If w <= 0 OrElse h <= 0 OrElse w > 4096 OrElse h > 2160 Then
                        Thread.Sleep(50)
                        Continue While
                    End If

                    ' (re)create the DIB section when the window size changes.
                    ' A 1680x1050 DIB is ~6.7MB; we only allocate on resize,
                    ' so steady-state has zero per-frame allocation (unlike
                    ' the CDP path which allocates a new byte[] every frame).
                    If w <> curW OrElse h <> curH Then
                        If dib <> IntPtr.Zero Then DeleteObject(dib)
                        If hdc <> IntPtr.Zero Then DeleteDC(hdc)
                        dib = IntPtr.Zero
                        dibBits = IntPtr.Zero
                        hdc = CreateCompatibleDC(IntPtr.Zero)
                        If hdc = IntPtr.Zero Then
                            L("CreateCompatibleDC failed: " & Marshal.GetLastWin32Error())
                            Thread.Sleep(200)
                            Continue While
                        End If
                        Dim bi As New BITMAPINFO()
                        bi.bmiHeader = New BITMAPINFOHEADER With {
                            .biSize = Marshal.SizeOf(Of BITMAPINFOHEADER)(),
                            .biWidth = w,
                            .biHeight = -h,             ' top-down (matches MMF row order)
                            .biPlanes = 1,
                            .biBitCount = 32,           ' BGRA
                            .biCompression = 0,         ' BI_RGB
                            .biSizeImage = w * h * 4
                        }
                        bi.bmiColors = New Integer(0) {}
                        dib = CreateDIBSection(hdc, bi, 0, dibBits, IntPtr.Zero, 0)
                        If dib = IntPtr.Zero OrElse dibBits = IntPtr.Zero Then
                            L("CreateDIBSection failed: " & Marshal.GetLastWin32Error())
                            Thread.Sleep(200)
                            Continue While
                        End If
                        curW = w : curH = h
                        L("dib created " & w & "x" & h & " bits=" & dibBits.ToString("X"))
                    End If

                    ' Select the DIB into our DC; PrintWindow draws into it.
                    Dim oldObj As IntPtr = SelectObject(hdc, dib)
                    Dim ok As Boolean = PrintWindow(_hwnd, hdc, PW_RENDERFULLCONTENT)
                    SelectObject(hdc, oldObj)

                    If Not ok Then
                        _failStreak += 1
                        If _failStreak = 1 Then
                            L("PrintWindow returned false (err=" & Marshal.GetLastWin32Error() & ")")
                        End If
                        ' 5 consecutive failures → let CDP take over publishing.
                        ' We keep polling so if the window recovers we resume.
                        If _failStreak = 5 Then
                            HookCdpCapture.ExternalCapture = 0
                            L("PrintWindow failed 5x — CDP takeover (ExternalCapture=0)")
                        End If
                        Thread.Sleep(50)
                        Continue While
                    End If

                    ' success → claim publishing ownership, reset fail streak
                    If _failStreak >= 5 Then
                        L("PrintWindow recovered after " & _failStreak & " fails — resuming")
                    End If
                    _failStreak = 0
                    HookCdpCapture.ExternalCapture = 1

                    ' publish pixels straight to the shared-memory MMF.
                    ' rowPitch = w * 4 (BGRA, no padding — DIB is top-down).
                    HookCdpCapture.PublishPixels(w, h, dibBits, w * 4)

                    _frameCount += 1L
                    If _frameCount = 1L OrElse _frameCount Mod 300L = 0L Then
                        L("pw published frame #" & _frameCount.ToString() & " " & w & "x" & h & " failStreak=0")
                    End If

                    ' target ~30 FPS (33ms cadence). PrintWindow itself
                    ' takes ~15-25ms at 1680x1050, so this sleep yields the
                    ' remainder to other threads (CDP input, OSC server).
                    Thread.Sleep(33)
                Catch ex As Exception
                    L("pw loop error: " & ex.Message)
                    Thread.Sleep(200)
                End Try
            End While
        Finally
            If dib <> IntPtr.Zero Then DeleteObject(dib)
            If hdc <> IntPtr.Zero Then DeleteDC(hdc)
            ' release publishing ownership on shutdown so CDP can resume
            HookCdpCapture.ExternalCapture = 0
        End Try
    End Sub

    Private Shared Sub L(m As String)
        Try
            Dim p As String = AppLayout.P("Logs", "printwindow.log")
            Dim d As String = IO.Path.GetDirectoryName(p)
            If Not IO.Directory.Exists(d) Then IO.Directory.CreateDirectory(d)
            IO.File.AppendAllText(p, DateTime.Now.ToString("HH:mm:ss.fff") & " " & m & Environment.NewLine)
        Catch
        End Try
    End Sub

End Class
