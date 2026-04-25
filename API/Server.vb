Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Partial Public Class API_RUN

    Private listener As TcpListener
    Private clients As New List(Of ClientInfo)
    Private clientsLock As New Object()
    Private startTime As DateTime

    Private Class ClientInfo
        Public Client As TcpClient
        Public Writer As StreamWriter
        Public AppName As String = "Unknown"
        Public LastActivity As DateTime = DateTime.Now
        Public ConnectedAt As DateTime = DateTime.Now
    End Class

    Private Sub Server_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        startTime = DateTime.Now

        Dim uiTimer As New System.Windows.Forms.Timer()
        uiTimer.Interval = 1000
        AddHandler uiTimer.Tick, AddressOf UpdateUI
        uiTimer.Start()

        Task.Run(AddressOf StartServer)
        Log("[Log] NVIDIA API", "server_started")
    End Sub

    ' ═══════════════════════════════════════════
    ' เพิ่ม: อัปเดต UI ทุก 1 วินาที
    ' ═══════════════════════════════════════════
    Private Sub UpdateUI()
        Me.Text = "API Server - tcp://127.0.0.1:5000"
        If InvokeRequired Then
            Invoke(Sub() UpdateUI())
            Return
        End If

        ' Uptime
        Dim uptime = DateTime.Now - startTime
        lblUptime.Text = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}"

        ' Client count
        SyncLock clientsLock
            lblClientsOnline.Text = clients.Count.ToString()
        End SyncLock

        ' Server status
        lblStatus.Text = "Online"
        lblStatus.ForeColor = Color.FromArgb(76, 175, 80)

        ' Total messages
        lblMessages.Text = totalMessages.ToString()

        ' Client list
        UpdateClientList()
    End Sub

    Private totalMessages As Integer = 0

    Private Sub UpdateClientList()
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
        listener = New TcpListener(IPAddress.Any, 5000)
        listener.Start()
#Disable Warning BC42358
        Task.Run(AddressOf HeartbeatMonitor)
#Enable Warning BC42358
        While True
            Try
                Dim client = Await listener.AcceptTcpClientAsync()
                Dim info As New ClientInfo With {
                    .Client = client,
                    .Writer = New StreamWriter(client.GetStream()) With {.AutoFlush = True},
                    .ConnectedAt = DateTime.Now
                }

                SyncLock clientsLock
                    clients.Add(info)
                End SyncLock

                Log("[Log] NVIDIA API", $"client_connected_{clients.Count}")
#Disable Warning BC42358
                Task.Run(Function() HandleClientAsync(info))
#Enable Warning BC42358
            Catch ex As Exception
                Log("[Error] NVIDIA API", $"accept_failed_{ex.Message}")
            End Try
        End While
    End Sub

    Private Async Function HandleClientAsync(info As ClientInfo) As Task
        Dim reader As New StreamReader(info.Client.GetStream())

        Dim id As String = "unknown"
        If info.Client.Client IsNot Nothing AndAlso info.Client.Client.RemoteEndPoint IsNot Nothing Then
            id = info.Client.Client.RemoteEndPoint.ToString()
        End If

        Try
            While True
                Dim msg = Await reader.ReadLineAsync()
                If msg Is Nothing Then Exit While

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

        totalMessages += 1
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
                Log("[Heartbeat] NVIDIA API", $"removed_dead_client_{d.AppName}")
            Next
        End SyncLock
    End Sub

    Private Sub HeartbeatMonitor()
        While True
            Thread.Sleep(10000)

            Dim dead As New List(Of ClientInfo)
            SyncLock clientsLock
                For Each c In clients
                    If (DateTime.Now - c.LastActivity).TotalSeconds > 30 Then
                        dead.Add(c)
                    End If
                Next

                For Each d In dead
                    clients.Remove(d)
                    Try : d.Client.Close() : Catch : End Try
                    Log("[Heartbeat] NVIDIA API", $"killed_inactive_{d.AppName}")
                Next
            End SyncLock
        End While
    End Sub

    Private Sub Log(app As String, msg As String)
        Dim line = $"[{DateTime.Now:HH:mm:ss}] {app} ""{msg}"""

        Dim action = Sub()
                         If lstLog.Items.Count > 1000 Then
                             lstLog.Items.RemoveAt(0)
                         End If
                         lstLog.Items.Add(line)
                         lstLog.TopIndex = lstLog.Items.Count - 1
                     End Sub

        If lstLog.InvokeRequired Then
            lstLog.Invoke(action)
        Else
            action()
        End If
    End Sub

End Class