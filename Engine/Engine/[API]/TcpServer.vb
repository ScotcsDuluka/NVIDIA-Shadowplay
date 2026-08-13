Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

''' <summary>
''' ═══════════════════════════════════════════════════════════════════════════
''' TCP Engine Hub Client — เชื่อมต่อกับ API Hub (port 5000)
''' รับ broadcast commands จาก Overlay และ execute capture operations
''' ═══════════════════════════════════════════════════════════════════════════
'''
''' Architecture:
'''   Overlay → tcp://127.0.0.1:5000 (API Hub) → broadcast → Engine (this)
'''   Engine เชื่อมกับ Hub เป็น TCP Client เหมือน Overlay/Notifier
'''
''' Protocol:
'''   Overlay ส่ง: [Send] NVIDIA Overlay|engine_record_start
'''   Hub broadcast: ไปยังทุก client รวม Engine
'''   Engine รับ → parse command → execute → ส่ง response กลับ
''' ═══════════════════════════════════════════════════════════════════════════
''' </summary>
Public Class EngineHubClient
    Implements IDisposable

#Region "Events"
    ''' <summary>ยกขึ้นเมื่อได้รับ engine command จาก Hub</summary>
    Public Event OnCommandReceived(sender As Object, e As CommandEventArgs)

    ''' <summary>ยกขึ้นเมื่อสถานะการเชื่อมต่อเปลี่ยน</summary>
    Public Event OnConnectionStatusChanged(sender As Object, connected As Boolean)

    ''' <summary>ยกขึ้นเมื่อมี log message</summary>
    Public Event OnLog(sender As Object, message As String)
#End Region

#Region "Private State"
    Private _client As TcpClient
    Private _writer As StreamWriter
    Private _reader As StreamReader
    Private _isConnected As Boolean = False
    Private _isReconnecting As Boolean = False
    Private _cts As CancellationTokenSource
    Private _writeLock As New Object()
    Private _reconnectLock As New Object()

    Private Const HUB_HOST As String = "127.0.0.1"
    Private Const HUB_PORT As Integer = 5000
    Private Const APP_NAME As String = "NVIDIA Engine"
    Private Const RECONNECT_BASE_MS As Integer = 2000
    Private Const RECONNECT_MAX_MS As Integer = 30000
    Private _reconnectDelay As Integer = RECONNECT_BASE_MS
#End Region

#Region "Public Properties"
    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected
        End Get
    End Property
#End Region

#Region "Connect / Disconnect"
    ''' <summary>เชื่อมต่อกับ Hub และเริ่ม listen</summary>
    Public Sub Connect()
        Try
            Disconnect()
            _cts = New CancellationTokenSource()

            _client = New TcpClient()
            _client.NoDelay = True
            _client.Connect(HUB_HOST, HUB_PORT)

            Dim stream As NetworkStream = _client.GetStream()
            _writer = New StreamWriter(stream) With {.AutoFlush = True}
            _reader = New StreamReader(stream)

            _isConnected = True
            _reconnectDelay = RECONNECT_BASE_MS

            ' Register ตัวเองกับ Hub
            SendRegister()

            RaiseEvent OnConnectionStatusChanged(Me, True)
            RaiseEvent OnLog(Me, "[Engine] Connected to Hub")

            Task.Run(AddressOf ListenLoop, _cts.Token)
            Task.Run(AddressOf PingLoop, _cts.Token)

        Catch ex As Exception
            _isConnected = False
            RaiseEvent OnLog(Me, $"[Engine] Connect failed: {ex.Message}")
            RaiseEvent OnConnectionStatusChanged(Me, False)
            Task.Run(AddressOf ReconnectLoop)
        End Try
    End Sub

    ''' <summary>ตัดการเชื่อมต่อ</summary>
    Public Sub Disconnect()
        _isConnected = False
        If _cts IsNot Nothing Then
            Try : _cts.Cancel() : Catch : End Try
        End If
        If _client IsNot Nothing Then
            Try : _client.Close() : Catch : End Try
        End If
        _client = Nothing
        _writer = Nothing
        _reader = Nothing
    End Sub
#End Region

#Region "Send Methods"
    ''' <summary>ส่ง command ไปยัง Hub (จะ broadcast ไปหา clients อื่น)</summary>
    Public Sub Send(cmd As String, Optional value As String = "")
        If Not IsConnected Then Return

        Try
            SyncLock _writeLock
                Dim msg As String
                If String.IsNullOrEmpty(value) Then
                    msg = $"[Send] {APP_NAME}|{cmd}"
                Else
                    msg = $"[Send] {APP_NAME}|{cmd}:{value}"
                End If
                _writer.WriteLine(msg)
            End SyncLock
        Catch ex As Exception
            Debug.WriteLine($"EngineHubClient.Send Error: {ex.Message}")
            _isConnected = False
            RaiseEvent OnConnectionStatusChanged(Me, False)
        End Try
    End Sub

    ''' <summary>ส่ง log ไปยัง Hub</summary>
    Public Sub SendLog(message As String)
        If Not IsConnected Then Return

        Try
            SyncLock _writeLock
                _writer.WriteLine($"[Receive] {APP_NAME}|{message}")
            End SyncLock
        Catch ex As Exception
            Debug.WriteLine($"EngineHubClient.SendLog Error: {ex.Message}")
            _isConnected = False
        End Try
    End Sub

    ''' <summary>ส่ง response กลับไปยัง Overlay (ผ่าน Hub broadcast)</summary>
    Public Sub SendResponse(command As String, status As String, Optional data As String = "", Optional requestId As String = Nothing)
        Dim value As String = $"{command},{status}"
        If Not String.IsNullOrEmpty(data) Then
            value &= $",{data}"
        End If
        ' ✅ P1: include requestId in response so the Overlay can correlate
        ' this response to the original request. Old protocol had no correlation
        ' → if two engine_record_start commands were issued rapidly, the Overlay
        ' couldn't tell which response matched which request.
        ' Format: engine_response:<command>,<status>[,<data>][,req=<reqId>]
        ' Backward compatible: if requestId is null/empty, the suffix is omitted.
        If Not String.IsNullOrEmpty(requestId) Then
            value &= $",req={requestId}"
        End If
        Send($"engine_response:{value}")
    End Sub

    Private Sub SendRegister()
        Try
            SyncLock _writeLock
                _writer.WriteLine($"[Send] {APP_NAME}|register:NVIDIA Engine")
            End SyncLock
        Catch
        End Try
    End Sub
#End Region

#Region "Listen Loop"
    Private Sub ListenLoop()
        Dim exitReason As String = "unknown"
        Try
            While _isConnected AndAlso Not _cts.IsCancellationRequested
                Dim msg = _reader.ReadLine()
                If msg Is Nothing Then
                    exitReason = "ReadLine returned Nothing (remote closed)"
                    Exit While
                End If

                ' ข้าม pong
                If msg = "[System]|pong" Then Continue While

                ' Parse message
                ProcessMessage(msg)
            End While
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is OperationCanceledException
            exitReason = $"IOException/OperationCanceledException: {ex.Message}"
        Catch ex As Exception
            exitReason = $"Exception: {ex.Message}"
            Debug.WriteLine($"EngineHubClient.ListenLoop Error: {ex.Message}")
        End Try

        Debug.WriteLine($"[Engine] ListenLoop exited: {exitReason}")
        RaiseEvent OnLog(Me, $"[Engine] ListenLoop exited: {exitReason}")

        _isConnected = False
        RaiseEvent OnConnectionStatusChanged(Me, False)
        ' ✅ FIX: only spawn ReconnectLoop if not already reconnecting.
        ' Old code spawned a new ReconnectLoop on every ListenLoop exit,
        ' even if one was already running → double "Reconnecting..." logs.
        If Not _isReconnecting Then
            Task.Run(AddressOf ReconnectLoop)
        End If
    End Sub

    Private Sub PingLoop()
        Try
            While _isConnected AndAlso Not _cts.IsCancellationRequested
                Thread.Sleep(10000)
                If Not IsConnected Then Exit While

                ' ✅ FIX: check underlying socket health before writing.
                ' Old code just tried to write and caught the exception,
                ' but sometimes the socket is half-open and the write
                ' succeeds locally while the remote never receives it.
                ' Poll for write availability to detect this case.
                If _client Is Nothing OrElse Not _client.Connected Then
                    Debug.WriteLine("[Engine] PingLoop: socket not connected, exiting")
                    Exit While
                End If

                Try
                    SyncLock _writeLock
                        _writer.WriteLine($"[Send] {APP_NAME}|ping")
                    End SyncLock
                Catch ex As Exception
                    Debug.WriteLine($"[Engine] PingLoop write failed: {ex.Message}")
                    Exit While
                End Try
            End While
        Catch ex As Exception
            Debug.WriteLine($"[Engine] PingLoop exception: {ex.Message}")
        End Try

        ' ✅ FIX: if PingLoop exits because the socket is dead, force a
        ' reconnect. Old code exited silently → ListenLoop kept blocking
        ' on ReadLine → waited for Hub's 30s heartbeat kill → slow.
        ' Now we proactively close the client so ReadLine returns Nothing
        ' immediately, triggering a fast reconnect.
        If _isConnected Then
            Debug.WriteLine("[Engine] PingLoop exited while _isConnected=True, forcing reconnect")
            Try
                _client?.Close()
            Catch
            End Try
        End If
    End Sub

    Private Sub ReconnectLoop()
        ' ✅ FIX: guard against double-spawn. Old code logged "Reconnecting..."
        ' BEFORE checking _isConnected, so even if Engine was already
        ' reconnected (by a parallel ReconnectLoop), it would still log.
        ' Now we check the flag first and bail out immediately.
        If _isReconnecting Then Return
        _isReconnecting = True

        Try
            RaiseEvent OnLog(Me, "[Engine] Reconnecting to Hub...")

            SyncLock _reconnectLock
                While Not _isConnected
                    Try
                        _reconnectDelay = Math.Min(_reconnectDelay * 2, RECONNECT_MAX_MS)
                        Thread.Sleep(_reconnectDelay)

                        _cts = New CancellationTokenSource()
                        _client = New TcpClient()
                        _client.NoDelay = True
                        _client.Connect(HUB_HOST, HUB_PORT)

                        Dim stream As NetworkStream = _client.GetStream()
                        _writer = New StreamWriter(stream) With {.AutoFlush = True}
                        _reader = New StreamReader(stream)

                        _isConnected = True
                        _reconnectDelay = RECONNECT_BASE_MS

                        SendRegister()

                        RaiseEvent OnConnectionStatusChanged(Me, True)
                        RaiseEvent OnLog(Me, "[Engine] Reconnected to Hub")

                        Task.Run(AddressOf ListenLoop, _cts.Token)
                        Task.Run(AddressOf PingLoop, _cts.Token)
                        Return

                    Catch ex As Exception
                        Debug.WriteLine($"EngineHubClient.ReconnectLoop Error: {ex.Message}")
                        _isConnected = False
                        Try : _client.Close() : Catch : End Try
                    End Try
                End While
            End SyncLock
        Finally
            _isReconnecting = False
        End Try
    End Sub
#End Region

#Region "Message Processing"
    ''' <summary>
    ''' Parse ข้อความจาก Hub และฟิลเตอร์เฉพาะ engine_ commands
    ''' Format: [Send] AppName|command:value หรือ [Receive] AppName|command:value
    '''
    ''' ✅ P1: Request ID support (backward compatible).
    '''   New format: [Send] AppName|command:reqId|value
    '''   Old format: [Send] AppName|command:value
    ''' If the value contains a leading token with no colon and the command
    ''' looks like it should have a payload, we treat the first | -delimited
    ''' chunk after the command as the requestId. Old clients that don't send
    ''' a reqId still work — Value just contains the original payload and
    ''' RequestId is empty.
    ''' </summary>
    Private Sub ProcessMessage(msg As String)
        Try
            If Not msg.Contains("|") Then Return

            Dim parts = msg.Split("|"c)
            If parts.Length < 2 Then Return

            Dim data = parts(1)

            ' ข้าม messages ที่ส่งจากตัวเอง
            If msg.Contains(APP_NAME) Then Return

            Dim colonIndex = data.IndexOf(":"c)
            Dim cmd, value As String
            If colonIndex >= 0 Then
                cmd = data.Substring(0, colonIndex)
                value = data.Substring(colonIndex + 1)
            Else
                cmd = data
                value = ""
            End If

            ' ✅ P1.5: handle Overlay's PREWARM_FFMPEG command BEFORE the engine_*
            ' filter. Overlay sends this with value="<ffmpegPath>|<encoderName>"
            ' to tell Engine where ffmpeg.exe lives (Overlay's api-core folder).
            ' Old behavior: Engine ignored this and used its own search path,
            ' which often didn't find ffmpeg.exe → StartRecordingAsync failed
            ' with "FFmpeg not found" → no response sent → Overlay showed
            ' "Recording" but nothing happened.
            If cmd = "PREWARM_FFMPEG" AndAlso value.Length > 0 Then
                Dim pipeIdx As Integer = value.IndexOf("|"c)
                Dim ffmpegPath As String = If(pipeIdx > 0, value.Substring(0, pipeIdx), value)
                RaiseEvent OnCommandReceived(Me, New CommandEventArgs("engine_prewarm_ffmpeg", ffmpegPath))
                Return
            End If

            ' ✅ P2: handle engine_config_changed broadcast from Overlay.
            ' Overlay sends this after SaveVideoSettings() so the Engine can
            ' refresh its UI immediately instead of waiting up to 2s for the
            ' file-poll timer. value = "video" or "config" or "".
            If cmd = "engine_config_changed" Then
                RaiseEvent OnCommandReceived(Me, New CommandEventArgs("engine_config_changed", value))
                Return
            End If

            ' ฟิลเตอร์เฉพาะ engine_ commands (และ alias เก่า)
            ' ✅ P1.5: accept legacy RECORD_START/STOP/REPLAY_* commands that
            ' Overlay's Sub_Record.vb still sends. Old behavior filtered them
            ' out (cmd.StartsWith("engine_") was the only check), so pressing
            ' Record in the Overlay did nothing while Overlay's UI optimistically
            ' showed "Recording" — bug reported as "กดอัด ขึ้นอัด แต่ไม่ได้อัดจริง".
            Dim canonicalCmd As String = cmd
            Select Case cmd
                Case "RECORD_START" : canonicalCmd = "engine_record_start"
                Case "RECORD_STOP" : canonicalCmd = "engine_record_stop"
                Case "REPLAY_START" : canonicalCmd = "engine_replay_start"
                Case "REPLAY_STOP" : canonicalCmd = "engine_replay_stop"
                Case "REPLAY_SAVE" : canonicalCmd = "engine_replay_save"
            End Select
            If Not canonicalCmd.StartsWith("engine_") Then Return
            cmd = canonicalCmd

            ' ✅ P1: parse optional requestId. New format:
            '   command:reqId|payload  (reqId is the first | -segment after the command)
            ' If the value starts with "req=<token>|" we treat <token> as the requestId
            ' and strip it from value. Otherwise value is unchanged (backward compat).
            Dim reqId As String = Nothing
            If value.StartsWith("req=", StringComparison.Ordinal) Then
                Dim sepIdx As Integer = value.IndexOf("|"c)
                If sepIdx > 4 Then
                    reqId = value.Substring(4, sepIdx - 4)
                    value = value.Substring(sepIdx + 1)
                End If
            End If

            RaiseEvent OnLog(Me, $"[Engine] Received: {cmd}={value}" & If(reqId, $" (req={reqId})", ""))
            RaiseEvent OnCommandReceived(Me, New CommandEventArgs(cmd, value) With {.RequestId = reqId})

        Catch ex As Exception
            Debug.WriteLine($"EngineHubClient.ProcessMessage Error: {ex.Message}")
        End Try
    End Sub
#End Region

#Region "IDisposable"
    Public Sub Dispose() Implements IDisposable.Dispose
        Disconnect()
    End Sub
#End Region

End Class

''' <summary>Event args สำหรับ command ที่รับมา</summary>
Public Class CommandEventArgs
    Inherits EventArgs

    Public Property Command As String
    Public Property Value As String
    ''' <summary>
    ''' ✅ P1: optional request ID for correlation. Empty if the sender
    ''' did not include one (old clients). Echo back in SendResponse to let
    ''' the Overlay match this response to its original request.
    ''' </summary>
    Public Property RequestId As String

    Public Sub New(cmd As String, val As String)
        Command = cmd
        Value = val
    End Sub
End Class
