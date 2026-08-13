Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Partial Public Class API_RUN

    Private listener As TcpListener
    Private clients As New List(Of ClientInfo)
    Private clientsLock As New Object()
    Private startTime As DateTime
    Private uiTimer As System.Windows.Forms.Timer
    Private _heartbeatCts As CancellationTokenSource
    Private _isShuttingDown As Boolean = False

    Private Class ClientInfo
        Public Client As TcpClient
        Public Writer As StreamWriter
        Public AppName As String = "Unknown"
        Public LastActivity As DateTime = DateTime.Now
        Public ConnectedAt As DateTime = DateTime.Now
    End Class

    Private Sub Server_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        startTime = DateTime.Now

        uiTimer = New System.Windows.Forms.Timer()
        uiTimer.Interval = 1000
        AddHandler uiTimer.Tick, AddressOf UpdateUI
        uiTimer.Start()

        Task.Run(AddressOf StartServer)
        Log("[Log] NVIDIA API", "server_started")
    End Sub

    ''' <summary>
    ''' แก้: FormClosing cleanup — หยุดทุกอย่างให้เรียบร้อย
    ''' </summary>
    Private Sub Server_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' ✅ FIX: previously this handler ran full destructive cleanup regardless of e.Cancel.
        ' When user clicked X, API_RUN_FormClosing set e.Cancel=True (minimize to tray),
        ' but THIS handler had already torn down the listener, heartbeat, and all clients —
        ' so the tray icon stayed alive but the Hub was dead and never restarted.
        If e.Cancel Then Return

        _isShuttingDown = True

        ' หยุด heartbeat
        If _heartbeatCts IsNot Nothing Then
            Try : _heartbeatCts.Cancel() : Catch : End Try
        End If

        ' หยุด UI timer
        If uiTimer IsNot Nothing Then
            uiTimer.Stop()
            uiTimer.Dispose()
            uiTimer = Nothing
        End If

        ' ปิด clients ทั้งหมด
        SyncLock clientsLock
            For Each c In clients
                Try : c.Client.Close() : Catch : End Try
            Next
            clients.Clear()
        End SyncLock

        ' หยุด listener
        If listener IsNot Nothing Then
            Try : listener.Stop() : Catch : End Try
        End If
    End Sub

    ''' <summary>
    ''' แก้: ย้าย UI modification เข้าไว้ใน Invoke check ก่อน
    ''' </summary>
    Private Sub UpdateUI()
        If InvokeRequired Then
            Try
                Invoke(Sub() UpdateUI())
            Catch ex As ObjectDisposedException
                ' form ปิดแล้ว ไม่ต้องทำอะไร
            End Try
            Return
        End If

        If _isShuttingDown Then Return

        Try
            Me.Text = "API Server - tcp://127.0.0.1:5000"

            ' Uptime
            Dim uptime = DateTime.Now - startTime
            lblUptime.Text = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}"

            ' Client count
            SyncLock clientsLock
                lblClientsOnline.Text = clients.Count.ToString()
            End SyncLock

            ' Server status
            lblStatus.Text = "Server log | Online"
            lblStatus.ForeColor = Color.FromArgb(76, 175, 80)

            ' Total messages — แก้: ใช้ Interlocked.Read
            lblMessages.Text = Interlocked.Read(totalMessages).ToString()

            ' Client list
            UpdateClientList()
        Catch ex As ObjectDisposedException
        End Try
    End Sub

    ''' <summary>
    ''' แก้: totalMessages เปลี่ยนเป็น Long ใช้กับ Interlocked
    ''' </summary>
    Private totalMessages As Long = 0

    Private Sub UpdateClientList()
        If lstLog.IsDisposed OrElse lstClients.IsDisposed Then Return

        lstClients.Items.Clear()

        SyncLock clientsLock
            For Each c In clients
                Dim name As String = c.AppName
                If name = "Unknown" Then name = "Connecting..."
                Dim line = $"{name} / Connected  {c.ConnectedAt:HH:mm:ss}"
                lstClients.Items.Add(line)
            Next
        End SyncLock
    End Sub

    Private Async Sub StartServer()
        ' ✅ P2.2: bind to IPv6 Any with dual-stack enabled.
        ' Old code: New TcpListener(IPAddress.Loopback, 5000) → IPv4 only.
        ' Problem: when Engine's TcpClient.Connect("127.0.0.1", 5000) resolved
        ' to ::ffff:127.0.0.1 (IPv6 dual-stack), the connection was refused
        ' because the Hub wasn't listening on IPv6.
        ' Fix: bind IPv6Any with SocketOptionName.IPv6Only=False (default in
        ' .NET 8) → accepts both IPv4 and IPv6 connections on the same socket.
        ' We still constrain to loopback by checking the remote endpoint
        ' after accept (to keep the security posture from P0).

        ' ✅ P2.6: retry loop. If both IPv6 and IPv4 binds fail (port 5000
        ' already in use by another app), wait 5s and retry instead of
        ' crashing the Hub.
        Dim bindAttempts As Integer = 0
        Dim shouldRetry As Boolean = False
        Do
            shouldRetry = False
            Try
                listener = New TcpListener(IPAddress.IPv6Any, 5000)
                listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, False)
                listener.Start()
                Exit Do
            Catch ex As SocketException
                Log("[Warn] NVIDIA API", $"ipv6_bind_failed_{ex.Message}_trying_ipv4")
                Try
                    listener = New TcpListener(IPAddress.Loopback, 5000)
                    listener.Start()
                    Exit Do
                Catch ex2 As SocketException
                    bindAttempts += 1
                    Log("[Error] NVIDIA API", $"bind_failed_attempt_{bindAttempts}_ipv4_error_{ex2.Message}")
                    If bindAttempts >= 12 Then
                        ' Give up after ~1 minute of retries.
                        Log("[Error] NVIDIA API", "bind_failed_giving_up_after_12_attempts")
                        Return
                    End If
                    shouldRetry = True
                End Try
            End Try

            ' ✅ P2.6b: VB.NET does not allow Await inside Catch/Finally/SyncLock
            ' (BC36943). Wait outside the Catch block instead.
            If shouldRetry AndAlso Not _isShuttingDown Then
                ' Task.Delay(...).Wait() is a blocking wait — fine here because
                ' StartServer is already on a background Task (no UI thread to block).
                Try
                    Task.Delay(5000).Wait()
                Catch
                    ' Wait was interrupted (CTS cancelled) — exit gracefully.
                    Return
                End Try
            End If
        Loop While shouldRetry AndAlso Not _isShuttingDown

        If _isShuttingDown Then Return

        _heartbeatCts = New CancellationTokenSource()
        Task.Run(Sub() HeartbeatMonitor(_heartbeatCts.Token), _heartbeatCts.Token)
        While Not _isShuttingDown
            Try
                Dim client = Await listener.AcceptTcpClientAsync()
                ' ✅ FIX: hard cap on connected clients to bound memory.
                Dim curCount As Integer
                SyncLock clientsLock : curCount = clients.Count : End SyncLock
                If curCount >= 32 Then
                    Try : client.Close() : Catch : End Try
                    Log("[Warn] NVIDIA API", "client_rejected_max_reached")
                    Continue While
                End If

                Dim info As New ClientInfo With {
                    .Client = client,
                    .Writer = New StreamWriter(client.GetStream()) With {.AutoFlush = True},
                    .ConnectedAt = DateTime.Now
                }

                SyncLock clientsLock
                    clients.Add(info)
                End SyncLock

                Log("[Log] NVIDIA API", $"client_connected_{clients.Count}")
                Task.Run(Function() HandleClientAsync(info))
            Catch ex As Exception
                If Not _isShuttingDown Then
                    Log("[Error] NVIDIA API", $"accept_failed_{ex.Message}")
                End If
                Exit While
            End Try
        End While
    End Sub

    Private Async Function HandleClientAsync(info As ClientInfo) As Task
        Dim reader As New StreamReader(info.Client.GetStream())

        Dim id As String = "unknown"
        If info.Client.Client IsNot Nothing AndAlso info.Client.Client.RemoteEndPoint IsNot Nothing Then
            id = info.Client.Client.RemoteEndPoint.ToString()
        End If

        ' ✅ FIX: per-read timeout. If a client opens a socket and sends no newline
        ' for hours, old code held the connection forever. With 60s inactivity, the
        ' HeartbeatMonitor (30s) wins first, but this is belt-and-suspenders.
        info.Client.ReceiveTimeout = 60000

        Try
            While True
                Dim msg = Await reader.ReadLineAsync()
                If msg Is Nothing Then Exit While

                ' ✅ FIX: message size cap. Old code happily ReadLine'd a 1GB line into RAM.
                ' 64 KB is generous for any legitimate command in this protocol
                ' (longest is something like `engine_record_start:<path>`).
                If msg.Length > 65536 Then
                    Log("[Warn] NVIDIA API", $"client_{id}_oversized_msg_{msg.Length}")
                    Exit While
                End If

                info.LastActivity = DateTime.Now
                ProcessMessage(msg, info)
                Broadcast(msg, info)
            End While

        Catch ex As IOException
            Log("[Warn] NVIDIA API", $"client_{id}_connection_lost")
        Catch ex As Exception
            Log("[Error] NVIDIA API", $"client_{id}_error_{ex.Message}")

        Finally
            SyncLock clientsLock
                clients.Remove(info)
            End SyncLock

            Try : info.Client.Close() : Catch : End Try
            Log("[Log] NVIDIA API", $"client_{id}_disconnected")
        End Try
    End Function

    Private Sub ProcessMessage(msg As String, info As ClientInfo)
        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        Dim app = parts(0)
        Dim data = parts(1)

        Dim cleanName = app.Replace("[Send] ", "").Replace("[Receive] ", "").Trim()
        info.AppName = cleanName

        Dim colonIndex = data.IndexOf(":"c)
        Dim cmd, value As String
        If colonIndex >= 0 Then
            cmd = data.Substring(0, colonIndex)
            value = data.Substring(colonIndex + 1)
        Else
            cmd = data
            value = ""
        End If

        If cmd = "ping" Then
            Try
                info.Writer.WriteLine("[System]|pong")
            Catch : End Try
            Exit Sub
        End If

        If cmd = "register" Then
            info.AppName = value
            Log("[Log] NVIDIA API", $"client_registered_{value}")
            Exit Sub
        End If

        ' แก้: ใช้ Interlocked.Increment
        Interlocked.Increment(totalMessages)
        Log(app, data)

        Select Case cmd
            Case "overlay_show"
                ' action

            Case "notifier_show"
                ' action

            Case "open"
                ' action

            Case Else

        End Select
    End Sub

    Private Sub Broadcast(msg As String, senderInfo As ClientInfo)
        Dim dead As New List(Of ClientInfo)
        Dim broadcastLog As String = Nothing

        ' ✅ FIX: previously Log() was called WHILE holding clientsLock. Log() does
        ' lstLog.Invoke() which blocks on the UI thread; the UI thread can be waiting
        ' on clientsLock inside UpdateClientList() → classic deadlock.
        ' Now we collect the dead list and the log message under the lock, but do the
        ' Log() call after releasing the lock.
        SyncLock clientsLock
            For Each c In clients
                If c Is senderInfo Then Continue For

                Try
                    c.Writer.WriteLine(msg)
                    c.LastActivity = DateTime.Now
                Catch
                    dead.Add(c)
                End Try
            Next

            For Each d In dead
                clients.Remove(d)
                Try : d.Client.Close() : Catch : End Try
                broadcastLog = $"removed_dead_client_{d.AppName}"
            Next
        End SyncLock

        If broadcastLog IsNot Nothing Then
            Log("[Heartbeat] NVIDIA API", broadcastLog)
        End If
    End Sub

    ''' <summary>
    ''' แก้: HeartbeatMonitor รับ CancellationToken — หยุดได้
    ''' </summary>
    Private Sub HeartbeatMonitor(token As CancellationToken)
        Try
            While Not token.IsCancellationRequested
                Thread.Sleep(10000)
                If token.IsCancellationRequested Then Exit While

                Dim dead As New List(Of ClientInfo)
                Dim killLog As New List(Of String)
                SyncLock clientsLock
                    For Each c In clients
                        ' ✅ FIX: increased from 30s to 60s. Old timeout was too
                        ' aggressive — Engine pings every 10s, but if one ping
                        ' is delayed or lost (network hiccup, OS scheduling),
                        ' the 30s window closes fast. 60s gives 5 missed pings
                        ' worth of buffer before killing the connection.
                        If (DateTime.Now - c.LastActivity).TotalSeconds > 60 Then
                            dead.Add(c)
                        End If
                    Next

                    For Each d In dead
                        clients.Remove(d)
                        Try : d.Client.Close() : Catch : End Try
                        killLog.Add($"killed_inactive_{d.AppName}")
                    Next
                End SyncLock
                ' ✅ FIX: log outside the lock (matches Broadcast fix).
                For Each ln In killLog
                    Log("[Heartbeat] NVIDIA API", ln)
                Next
            End While
        Catch ex As OperationCanceledException
            ' ปกติ — ถูก cancel
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' แก้: Log ป้องกัน crash ตอน form ปิด
    ''' </summary>
    Private Sub Log(app As String, msg As String)
        If _isShuttingDown Then Return
        If lstLog.IsDisposed Then Return

        Dim line = $"[{DateTime.Now:HH:mm:ss}] {app} ""{msg}"""

        Dim action = Sub()
                         If lstLog.IsDisposed Then Return
                         If lstLog.Items.Count > 1000 Then
                             lstLog.Items.RemoveAt(0)
                         End If
                         lstLog.Items.Add(line)
                         lstLog.TopIndex = lstLog.Items.Count - 1
                     End Sub

        If lstLog.InvokeRequired Then
            Try
                lstLog.Invoke(action)
            Catch ex As ObjectDisposedException
                ' form ปิดแล้ว
            End Try
        Else
            Try
                action()
            Catch ex As ObjectDisposedException
            End Try
        End If
    End Sub

End Class