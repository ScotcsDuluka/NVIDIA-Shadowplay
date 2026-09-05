Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Partial Public Class API_RUN

    ' F-04: loopback-only listeners (127.0.0.1 + ::1). The old single
    ' `listener` field (wildcard-bound) is gone — see StartServer.
    Private loopbackListeners As TcpListener() = New TcpListener() {}
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

        ' หยุด listener ทุกตัว (F-04: loopback listeners อาจมีมากกว่าหนึ่ง —
        ' 127.0.0.1 และ ::1 แยก socket กัน)
        For Each l As TcpListener In loopbackListeners
            Try : l.Stop() : Catch : End Try
        Next
        loopbackListeners = New TcpListener() {}
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
        ' ✅ F-04 FIX (C/2): bind ONLY loopback — was: IPv6Any dual-stack wildcard.
        '
        ' History: P2.2 widened the bind from IPAddress.Loopback to IPv6Any
        ' (IPv6Only=False) so the Engine's dual-stack "127.0.0.1" connect could
        ' never be refused, and this comment claimed "We still constrain to
        ' loopback by checking the remote endpoint after accept" — but NO such
        ' check existed (C/6 F-04). Measured on this machine before the fix:
        ' the wildcard hub accepted and served 192.168.1.254 and 192.168.137.1
        ' clients end-to-end (connect + ping → pong).
        '
        ' Now the boundary is kernel-level: loopback-only listeners created by
        ' LoopbackGate (127.0.0.1 + ::1 with IPv6Only=True, so dual-stack
        ' mapping can never widen the ::1 socket to a wildcard). The accept
        ' path re-checks the peer with LoopbackGate.IsLoopback as defense in
        ' depth (see HandleClientAsync).
        '
        ' ✅ P2.6: retry loop (kept). If EVERY loopback bind fails (port 5000
        ' already in use by another app), wait 5s and retry instead of
        ' crashing the Hub. A single-family bind failure only logs and
        ' continues on the other family.
        Dim bindAttempts As Integer = 0
        Dim shouldRetry As Boolean = False
        Do
            shouldRetry = False
            Dim bound As New List(Of TcpListener)
            For Each l As TcpListener In LoopbackGate.CreateLoopbackListeners(5000)
                Try
                    l.Start()
                    bound.Add(l)
                Catch ex As SocketException
                    Try : l.Stop() : Catch : End Try
                    Log("[Warn] NVIDIA API", $"loopback_bind_failed_{l.LocalEndpoint}_{ex.Message}_continuing")
                End Try
            Next
            loopbackListeners = bound.ToArray()
            If loopbackListeners.Length > 0 Then
                Exit Do
            End If

            ' Every loopback bind failed — keep the P2.6/M12 retry UX.
            bindAttempts += 1
            Log("[Error] NVIDIA API", $"loopback_bind_failed_attempt_{bindAttempts}_all_families")
            If bindAttempts >= 12 Then
                ' Give up after ~1 minute of retries.
                Log("[Error] NVIDIA API", "bind_failed_giving_up_after_12_attempts")
                ' ✅ M12 FIX: tell the user the hub is dead. Old code just
                ' returned silently — lblStatus still said "Online" because
                ' UpdateUI hardcodes "Online". Now show an error status and
                ' a tray balloon so the user knows nothing can connect.
                Try
                    Me.Invoke(Sub()
                                  lblStatus.Text = "Server log | OFFLINE — bind failed"
                                  lblStatus.ForeColor = Color.FromArgb(200, 50, 50)
                                  notifyIcon.BalloonTipTitle = "NVIDIA API"
                                  notifyIcon.BalloonTipText = "Hub failed to bind port 5000 after 12 attempts. Check if another app is using the port."
                                  notifyIcon.BalloonTipIcon = ToolTipIcon.Error
                                  notifyIcon.ShowBalloonTip(5000)
                              End Sub)
                Catch
                End Try
                Return
            End If
            shouldRetry = True

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
        ' Deliberate fire-and-forget: HeartbeatMonitor exits via CTS cancellation
        ' and logs its own failures; awaiting it here would stall the accept loop.
#Disable Warning BC42358 ' Fire-and-forget by design (see comment above)
        Task.Run(Sub() HeartbeatMonitor(_heartbeatCts.Token), _heartbeatCts.Token)
#Enable Warning BC42358

        ' ✅ F-04: one accept task per bound loopback listener. WhenAny picks
        ' the first completed accept; that slot is re-armed against the SAME
        ' listener so both families keep serving concurrently. A faulted
        ' accept (listener stopped) drops only its own slot — the remaining
        ' listener keeps serving until shutdown.
        Dim accepts As New List(Of Task(Of TcpClient))
        Dim owners As New List(Of TcpListener)
        For Each l As TcpListener In loopbackListeners
            accepts.Add(l.AcceptTcpClientAsync())
            owners.Add(l)
        Next

        While Not _isShuttingDown AndAlso accepts.Count > 0
            Dim completed As Task(Of TcpClient) = Await Task.WhenAny(accepts)
            Dim idx As Integer = accepts.IndexOf(completed)
            Dim client As TcpClient = Nothing
            Try
                client = Await completed
            Catch ex As Exception
                If Not _isShuttingDown Then
                    Log("[Error] NVIDIA API", $"accept_failed_{ex.Message}")
                End If
                ' Drop this listener from rotation; keep the others alive.
                accepts.RemoveAt(idx)
                owners.RemoveAt(idx)
                Continue While
            End Try

            ' Re-arm the slot for its listener before any client work.
            accepts(idx) = owners(idx).AcceptTcpClientAsync()

            ' ✅ FIX (kept): hard cap on connected clients to bound memory.
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
            ' Deliberate fire-and-forget: HandleClientAsync owns its own
            ' try/catch and per-client lifetime; the accept loop must not
            ' wait on it or one slow client would block all others.
#Disable Warning BC42358 ' Fire-and-forget by design (see comment above)
            Task.Run(Function() HandleClientAsync(info))
#Enable Warning BC42358
        End While
    End Sub

    Private Async Function HandleClientAsync(info As ClientInfo) As Task
        ' ✅ F-04 defense in depth: the kernel-level loopback bind is THE
        ' boundary; this re-check guarantees that a future bind change (or a
        ' platform dual-stack surprise) cannot silently reopen the Hub to
        ' non-loopback peers. Rejected immediately, before any stream work.
        Dim peer As IPEndPoint = TryCast(info.Client.Client.RemoteEndPoint, IPEndPoint)
        If Not LoopbackGate.IsLoopback(peer) Then
            Log("[Warn] NVIDIA API", $"client_rejected_non_loopback_{If(peer IsNot Nothing, peer.ToString(), "<unknown>")}")
            Try : info.Client.Close() : Catch : End Try
            Return
        End If

        Dim reader As New StreamReader(info.Client.GetStream())

        Dim id As String = "unknown"
        If info.Client.Client IsNot Nothing AndAlso info.Client.Client.RemoteEndPoint IsNot Nothing Then
            id = info.Client.Client.RemoteEndPoint.ToString()
        End If

        ' ✅ FIX: per-read timeout. If a client opens a socket and sends no newline
        ' for hours, old code held the connection forever. With 60s inactivity, the
        ' HeartbeatMonitor (30s) wins first, but this is belt-and-suspenders.
        info.Client.ReceiveTimeout = 60000

        ' ✅ F-04 follow-up (message layer): per-WRITE timeout. Broadcast()
        ' writes under clientsLock; without SendTimeout a local client that
        ' never reads blocks that write once its buffers fill — deadlocking
        ' the WHOLE hub (broadcasts, pongs, HeartbeatMonitor) for as long as
        ' the attacker holds the socket. With the timeout, the stuck write
        ' throws on the writer thread and the dead-client path releases the
        ' lock; the hub survives. Legit loopback clients never block 60s.
        info.Client.SendTimeout = 60000

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
            ' ✅ C2 FIX: lock clientsLock while writing pong. StreamWriter is not
            ' thread-safe — Broadcast() could be writing to the same info.Writer
            ' on another thread at the same time, interleaving bytes and
            ' producing garbled TCP frames like "[Sys|pongotem|ping".
            Try
                SyncLock clientsLock
                    info.Writer.WriteLine("[System]|pong")
                End SyncLock
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
        ' ✅ M8 FIX: collect ALL dead client names, not just the last one.
        ' Old code reassigned broadcastLog inside the loop, so if 3 clients
        ' died in one Broadcast cycle, only the third's name was logged.
        Dim deadLogs As New List(Of String)

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
                deadLogs.Add($"removed_dead_client_{d.AppName}")
            Next
        End SyncLock

        ' ✅ M8 FIX: log every dead client, not just the last.
        For Each ln In deadLogs
            Log("[Heartbeat] NVIDIA API", ln)
        Next
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