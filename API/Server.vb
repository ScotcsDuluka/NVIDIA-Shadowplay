Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class API_RUN

    ' ══════════════════════════════════════
    ' 🌐 SERVER VARIABLES
    ' ══════════════════════════════════════

    Dim listener As TcpListener
    Dim clients As New List(Of TcpClient)
    Private clientsLock As New Object()   ' ✅ FIX: Thread Safety


    ' ══════════════════════════════════════
    ' ▶️ START SERVER
    ' ══════════════════════════════════════

    Private Sub API_Server_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Task.Run(AddressOf StartServer)
        Log("SYSTEM", "Server started on port 5000")
    End Sub

    Sub StartServer()
        listener = New TcpListener(IPAddress.Any, 5000)
        listener.Start()

        While True
            Dim client = listener.AcceptTcpClient()

            ' ✅ FIX: Thread-safe add
            SyncLock clientsLock
                clients.Add(client)
            End SyncLock

            Log("SYSTEM", $"Client connected (Total: {clients.Count})")

            Dim t As New Thread(Sub() HandleClient(client))
            t.IsBackground = True
            t.Start()
        End While
    End Sub


    ' ══════════════════════════════════════
    ' 📥 HANDLE CLIENT
    ' ══════════════════════════════════════

    Sub HandleClient(client As TcpClient)
        Dim clientId As String = ""

        Try
            clientId = client.Client.RemoteEndPoint.ToString()

            Using reader As New StreamReader(client.GetStream())

                While True
                    Try
                        Dim msg = reader.ReadLine()
                        If msg Is Nothing Then Exit While

                        ProcessMessage(msg)
                        Broadcast(msg, client)

                    Catch ex As Exception
                        Log($"ERROR-{clientId}", ex.Message)
                        Exit While
                    End Try
                End While

            End Using

        Finally
            ' ✅ FIX: Cleanup when disconnect
            SyncLock clientsLock
                clients.Remove(client)
            End SyncLock

            Try : client.Close() : Catch : End Try

            Log("SYSTEM", $"Client {clientId} disconnected (Remaining: {clients.Count})")
        End Try
    End Sub


    ' ══════════════════════════════════════
    ' 📨 PROCESS MESSAGE
    ' ══════════════════════════════════════

    Sub ProcessMessage(msg As String)
        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        Dim app = parts(0).Trim()
        Dim data = parts(1).Trim()

        Log(app, data)

        Select Case data
            Case "record_start"
              '  Log("ACTION", "▶ Start Recording")

            Case "record_stop"
                'Log("ACTION", "⏹ Stop Recording")

            Case Else
                'Log("RECV", data)
        End Select
    End Sub


    ' ══════════════════════════════════════
    ' 📤 BROADCAST TO ALL CLIENTS
    ' ══════════════════════════════════════

    Sub Broadcast(msg As String, senderClient As TcpClient)
        Dim deadClients As New List(Of TcpClient)

        SyncLock clientsLock
            For Each c In clients
                If c Is senderClient Then Continue For

                Try
                    Using writer As New StreamWriter(c.GetStream())
                        writer.WriteLine(msg)
                        writer.Flush()
                    End Using
                Catch
                    deadClients.Add(c)   ' Mark dead client
                End Try
            Next

            ' ✅ FIX: Remove dead clients
            For Each dead In deadClients
                clients.Remove(dead)
                Try : dead.Close() : Catch : End Try
            Next
        End SyncLock

        If deadClients.Count > 0 Then
            Log("CLEANUP", $"Removed {deadClients.Count} dead client(s)")
        End If
    End Sub


    ' ══════════════════════════════════════
    ' 📝 LOGGING
    ' ══════════════════════════════════════

    Sub Log(app As String, data As String)
        Dim line = $"[{DateTime.Now:HH:mm:ss}] {app} ""{data}"""

        If lstLog.InvokeRequired Then
            Try
                lstLog.Invoke(Sub()
                                  lstLog.Items.Add(line)
                                  lstLog.TopIndex = lstLog.Items.Count - 1
                              End Sub)
            Catch
            End Try
        Else
            lstLog.Items.Add(line)
            lstLog.TopIndex = lstLog.Items.Count - 1
        End If

        Try
            IO.File.AppendAllText("server.log", line & Environment.NewLine)
        Catch
        End Try
    End Sub

End Class