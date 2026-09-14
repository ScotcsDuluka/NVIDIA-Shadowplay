' HookCdpCapture.vb — captures the osc page via the Chrome DevTools
' Protocol (Page.captureScreenshot). Unlike PrintWindow/GDI (black for
' GPU-composited WebView2 content), CDP renders the PAGE itself — works
' even when the window is off-screen or occluded. ~5fps MVP via polling.
'
' Frame publish: shared memory "NVIDIA_Share_Overlay_Frame_v1"
'   header: [0]=magic "NSPL", [4]=w, [8]=h, [12]=frameId (TickCount when
'           the CDP pump is the writer), [16]=overlayVisible, [20]=live
'           counter (injected DLL writes), [24]=engine PID,
'           [28]=controller port, [32..50]=controller secret (ASCII, NUL-
'           terminated) — the in-game DLL reads these to POST /Hook/Input.
'   pixels: BGRA32 from offset 64

Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Net.Http
Imports System.Text
Imports System.Threading

Public Class HookCdpCapture

    Private Const MmfName As String = "NVIDIA_Share_Overlay_Frame_v1"
    Private Const HeaderBytes As Integer = 64
    Private Const Magic As Integer = &H4C50534E          ' "NSPL"
    Private Const PubMaxBytes As Long = HeaderBytes + 2560L * 1440L * 4L

    ''' <summary>1 = capture + publish frames (overlay open); 0 = publish
    '     visible=0 and idle. Set by OscHostForm.SetOverlayOpen.</summary>
    Public Shared CaptureEnabled As Integer = 0

    ''' <summary>Controller REST endpoint the in-game DLL posts hook input
    '     to. Published in the frame header so the DLL needs no config.</summary>
    Public Shared ControllerPort As Integer = 0
    Public Shared ControllerSecret As String = ""

    ''' <summary>Shared log sink for hook subsystems (input reader etc.).</summary>
    Public Shared Sub LogLine(m As String)
        L(m)
    End Sub

    ''' <summary>1 = the WGC GPU source owns frame publishing — the CDP loop
    '     must not publish (two writers would fight over the header).</summary>
    Public Shared ExternalCapture As Integer = 0

    Private Shared _pubMmf As MemoryMappedFile
    Private Shared _pubView As MemoryMappedViewAccessor
    Private Shared ReadOnly PubLock As New Object()
    Private Shared _pubFrames As Long
    Private Shared _lastEnabled As Integer = -1

    Private _thread As Thread
    Private _stopFlag As Boolean
    Private _debugPort As String

    Public Sub New(debugPort As String)
        _debugPort = debugPort
    End Sub

    Public Sub Start()
        If _thread IsNot Nothing Then Return
        _stopFlag = False
        _thread = New Thread(AddressOf CaptureLoop) With {.IsBackground = True, .Name = "HookCdpCapture"}
        _thread.Start()
    End Sub

    Public Sub [Stop]()
        _stopFlag = True
        Try : _thread?.Join(500) : Catch : End Try
    End Sub

    Private Shared Sub L(m As String)
        Try
            Dim p As String = AppLayout.P("Logs", "hookcdp.log")
            Dim d As String = IO.Path.GetDirectoryName(p)
            If Not IO.Directory.Exists(d) Then IO.Directory.CreateDirectory(d)
            IO.File.AppendAllText(p, DateTime.Now.ToString("HH:mm:ss.fff") & " " & m & Environment.NewLine)
        Catch
        End Try
    End Sub

    ''' <summary>Port (+28) and secret (+32) live next to the header fields
    '     the DLL already knows — one mapping, no extra handshake.</summary>
    Private Shared Sub WriteEndpointHeader()
        Try
            _pubView.Write(28, ControllerPort)
            Dim b(30) As Byte
            If ControllerSecret IsNot Nothing Then
                Dim n As Integer = Math.Min(Encoding.ASCII.GetByteCount(ControllerSecret), 31)
                Encoding.ASCII.GetBytes(ControllerSecret, 0, n, b, 0)
            End If
            _pubView.WriteArray(32, b, 0, b.Length)
        Catch
        End Try
    End Sub

    Private Shared Sub EnsurePub()
        If _pubMmf IsNot Nothing Then Return
        _pubMmf = MemoryMappedFile.CreateOrOpen(MmfName, PubMaxBytes, MemoryMappedFileAccess.ReadWrite)
        _pubView = _pubMmf.CreateViewAccessor(0, PubMaxBytes)
        _pubView.Write(0, Magic)
        _pubView.Write(16, 0)
        ' epoch @+56: bumped every time a NEW engine process creates the
        ' section — the injected DLL polls this to drop its handle on the
        ' ORPHANED section of a dead engine and re-open the live one.
        _pubView.Write(56, Environment.TickCount)
        WriteEndpointHeader()
        L("mmf created, endpoint published (port=" & ControllerPort & ")")
    End Sub

    Private Shared Sub PublishClosed()
        EnsurePub()
        SyncLock PubLock
            _pubView.Write(16, 0)
            _pubView.Write(24, Process.GetCurrentProcess().Id)
            WriteEndpointHeader()
        End SyncLock
    End Sub

    Private Shared Sub PublishFrame(png As Byte())
        Using ms As New MemoryStream(png)
            Using bmp As New Bitmap(ms)
                Dim w As Integer = bmp.Width
                Dim h As Integer = bmp.Height
                EnsurePub()
                Dim bmpData As BitmapData = bmp.LockBits(
                    New Rectangle(0, 0, w, h), ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb)
                Dim total As Integer = CInt(bmpData.Stride) * h
                Dim raw(total - 1) As Byte
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, raw, 0, total)
                bmp.UnlockBits(bmpData)
                SyncLock PubLock
                    _pubView.Write(4, w)
                    _pubView.Write(8, h)
                    _pubView.Write(12, Environment.TickCount)
                    _pubView.Write(16, 1)
                    _pubView.Write(24, Process.GetCurrentProcess().Id)
                    WriteEndpointHeader()
                    For y As Integer = 0 To h - 1
                        _pubView.WriteArray(HeaderBytes + CLng(y) * w * 4, raw,
                            y * bmpData.Stride, Math.Min(w * 4, bmpData.Stride))
                    Next
                End SyncLock
                _pubFrames += 1L
                If _pubFrames = 1L OrElse _pubFrames Mod 100L = 0L Then
                    L("published frame #" & _pubFrames.ToString() & " " & w & "x" & h)
                End If
            End Using
        End Using
    End Sub

    ' ── CDP input injection (real Chromium input pipeline) ─────
    ' Synthetic DOM events CANNOT trigger CSS :hover (the browser only
    ' updates :hover from real input). Input.dispatchMouseEvent/Key go
    ' through Chromium's own input pipeline → hover, click, focus, text
    ' all behave natively. Own connection: independent of the capture WS.
    Private Shared ReadOnly InputWsLock As New Object()
    Private Shared _inputWs As System.Net.WebSockets.ClientWebSocket
    Private Shared _inputMsgId As Integer = 50000

    ''' <summary>Sends a CDP Input.* command; connects lazily. Returns False
    '     when the connection failed (caller falls back to DOM events).</summary>
    Public Shared Function SendCdpInput(cdpJson As String) As Boolean
        SyncLock InputWsLock
            Try
                If _inputWs Is Nothing OrElse _inputWs.State <> System.Net.WebSockets.WebSocketState.Open Then
                    If _inputWs IsNot Nothing Then _inputWs.Dispose()
                    _inputWs = New System.Net.WebSockets.ClientWebSocket()
                    Dim listJson As String = New HttpClient().GetStringAsync(
                        "http://127.0.0.1:9224/json/list").Result
                    Dim arr As System.Text.Json.JsonElement = System.Text.Json.JsonDocument.Parse(
                        listJson.Substring(listJson.IndexOf("["c))).RootElement
                    Dim wsUrl As String = Nothing
                    For Each t As System.Text.Json.JsonElement In arr.EnumerateArray()
                        Dim u As System.Text.Json.JsonElement
                        If t.TryGetProperty("url", u) AndAlso u.GetString().Contains("index.html") Then
                            Dim wN As System.Text.Json.JsonElement
                            If t.TryGetProperty("webSocketDebuggerUrl", wN) Then wsUrl = wN.GetString()
                            Exit For
                        End If
                    Next
                    If String.IsNullOrEmpty(wsUrl) Then Return False
                    _inputWs.ConnectAsync(New Uri(wsUrl), Nothing).Wait(3000)
                    If _inputWs.State <> System.Net.WebSockets.WebSocketState.Open Then Return False
                End If
                _inputMsgId += 1
                ' wrap: caller passes the "method/params" body
                Dim msg As String = "{""id"":" & _inputMsgId & "," & cdpJson & "}"
                Dim b As Byte() = Encoding.UTF8.GetBytes(msg)
                _inputWs.SendAsync(New ArraySegment(Of Byte)(b),
                    System.Net.WebSockets.WebSocketMessageType.Text, True, Nothing).Wait(3000)
                Return True
            Catch
                Try : _inputWs?.Dispose() : Catch : End Try
                _inputWs = Nothing
                Return False
            End Try
        End SyncLock
    End Function

    ''' <summary>Publishes raw BGRA pixels (WGC GPU path — no PNG). The
    '     pixels arrive on the capture thread; the header layout is
    '     identical to the PNG path.</summary>
    Public Shared Sub PublishPixels(w As Integer, h As Integer, pixels As IntPtr, rowPitch As Integer)
        If w <= 0 OrElse h <= 0 OrElse pixels = IntPtr.Zero Then Return
        If h > 2160 OrElse w > 2560 Then Return
        EnsurePub()
        Dim total As Integer = rowPitch * h
        Dim raw(total - 1) As Byte
        System.Runtime.InteropServices.Marshal.Copy(pixels, raw, 0, total)
        SyncLock PubLock
            _pubView.Write(4, w)
            _pubView.Write(8, h)
            _pubView.Write(12, Environment.TickCount)
            _pubView.Write(16, 1)
            _pubView.Write(24, Process.GetCurrentProcess().Id)
            WriteEndpointHeader()
            For y As Integer = 0 To h - 1
                _pubView.WriteArray(HeaderBytes + CLng(y) * w * 4, raw, y * rowPitch, w * 4)
            Next
        End SyncLock
        _pubFrames += 1L
        If _pubFrames = 1L OrElse _pubFrames Mod 300L = 0L Then
            L("wgc published frame #" & _pubFrames.ToString() & " " & w & "x" & h)
        End If
    End Sub

    Private Sub CaptureLoop()
        Dim wsClient As New System.Net.WebSockets.ClientWebSocket()
        Dim wsOpen As Boolean = False
        Dim msgId As Integer = 1

        While Not _stopFlag
            Try
                If Not wsOpen Then
                    Using http As New HttpClient()
                        Dim listJson As String = http.GetStringAsync(
                            "http://127.0.0.1:" & _debugPort & "/json/list").Result
                        Dim arrStart As Integer = listJson.IndexOf("["c)
                        If arrStart < 0 Then Throw New Exception("no json array in /json/list")
                        Dim arr As System.Text.Json.JsonElement = System.Text.Json.JsonDocument.Parse(
                            listJson.Substring(arrStart)).RootElement
                        Dim wsUrl As String = Nothing
                        For Each t As System.Text.Json.JsonElement In arr.EnumerateArray()
                            Dim u As System.Text.Json.JsonElement
                            If t.TryGetProperty("url", u) AndAlso
                               u.GetString().Contains("index.html") Then
                                Dim wN As System.Text.Json.JsonElement
                                If t.TryGetProperty("webSocketDebuggerUrl", wN) Then
                                    wsUrl = wN.GetString()
                                End If
                                Exit For
                            End If
                        Next
                        If String.IsNullOrEmpty(wsUrl) Then Throw New Exception("no page target")
                        If wsClient.State <> System.Net.WebSockets.WebSocketState.Open Then
                            wsClient.Dispose()
                            wsClient = New System.Net.WebSockets.ClientWebSocket()
                        End If
                        wsClient.ConnectAsync(New Uri(wsUrl), Nothing).Wait(5000)
                        If wsClient.State <> System.Net.WebSockets.WebSocketState.Open Then
                            Throw New Exception("ws connect failed: " & wsClient.State.ToString())
                        End If
                        wsOpen = True
                        L("ws connected")
                    End Using
                End If

                If CaptureEnabled <> _lastEnabled Then
                    _lastEnabled = CaptureEnabled
                    L("capture " & If(CaptureEnabled = 1, "ENABLED", "disabled"))
                End If

                If CaptureEnabled = 0 Then
                    PublishClosed()
                    Thread.Sleep(400)
                    Continue While
                End If

                ' WGC (GPU source) owns publishing while active — idle here
                If ExternalCapture = 1 Then
                    Thread.Sleep(100)
                    Continue While
                End If

                ' captureScreenshot polling — FORCED render works while the
                ' window is occluded by the game. (Page.startScreencast was
                ' tried and DOES NOT: it needs compositor frames, which an
                ' occluded window never produces — frozen at 1 frame.)
                ' ~65ms PNG round-trip = ~14fps ceiling; true 60fps needs
                ' Windows.Graphics.Capture (GPU, no PNG) — next milestone.
                Dim req As String = "{""id"":" & msgId & ",""method"":""Page.captureScreenshot""," &
                                    """params"":{""format"":""png"",""optimizeForSpeed"":true," &
                                    """clip"":{""x"":0,""y"":0,""width"":1920,""height"":1200,""scale"":0.875}}}"
                Dim sent = Encoding.UTF8.GetBytes(req)
                Dim sendOk As Boolean = wsClient.SendAsync(New ArraySegment(Of Byte)(sent),
                    System.Net.WebSockets.WebSocketMessageType.Text, True, Nothing).Wait(5000)
                If Not sendOk OrElse wsClient.State <> System.Net.WebSockets.WebSocketState.Open Then
                    Throw New Exception("send captureScreenshot failed (state=" & wsClient.State.ToString() & ")")
                End If

                Dim pngBytes As Byte() = Nothing
                Dim deadline As Integer = Environment.TickCount + 6000
                While Environment.TickCount < deadline AndAlso pngBytes Is Nothing
                    If wsClient.State <> System.Net.WebSockets.WebSocketState.Open Then Exit While
                    Dim buf(65536) As Byte
                    Dim ms As New MemoryStream()
                    Dim got As Boolean = False
                    Try
                        While Not got
                            If wsClient.State <> System.Net.WebSockets.WebSocketState.Open Then Exit While
                            Dim seg = New ArraySegment(Of Byte)(buf)
                            Dim cts As New CancellationTokenSource(3000)
                            Dim res = wsClient.ReceiveAsync(seg, cts.Token).Result
                            ms.Write(seg.Array, seg.Offset, res.Count)
                            If res.EndOfMessage Then got = True
                        End While
                    Catch ex As AggregateException
                        Dim allCancel As Boolean = ex.InnerExceptions.Count > 0
                        For Each ie As Exception In ex.InnerExceptions
                            If Not (TypeOf ie Is OperationCanceledException) Then allCancel = False
                        Next
                        If Not allCancel Then Throw
                    End Try
                    If Not got Then Continue While
                    Dim txt As String = Encoding.UTF8.GetString(ms.ToArray())
                    Dim idNeedle As String = """id"":" & msgId
                    If txt.Contains(idNeedle) AndAlso txt.Contains("""data""") Then
                        Dim m2 As System.Text.RegularExpressions.Match =
                            System.Text.RegularExpressions.Regex.Match(txt, """data""\s*:\s*""([^""]+)""")
                        If m2.Success Then
                            pngBytes = Convert.FromBase64String(m2.Groups(1).Value)
                        End If
                    End If
                End While

                If pngBytes IsNot Nothing Then
                    PublishFrame(pngBytes)
                End If
                msgId += 1
                Thread.Sleep(20)
            Catch ex As Exception
                L("cdp error: " & ex.Message)
                wsOpen = False
                Try : wsClient.Abort() : Catch : End Try
                wsClient = New System.Net.WebSockets.ClientWebSocket()
                Thread.Sleep(2000)
            End Try
        End While
    End Sub

    Private Shared Sub SendCdp(ws As System.Net.WebSockets.ClientWebSocket, req As String)
        Dim b As Byte() = Encoding.UTF8.GetBytes(req)
        ws.SendAsync(New ArraySegment(Of Byte)(b),
            System.Net.WebSockets.WebSocketMessageType.Text, True, Nothing).Wait(3000)
    End Sub

End Class
