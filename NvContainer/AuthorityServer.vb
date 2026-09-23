Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Json
Imports System.Threading

' TCP authority: line-delimited JSON request/response.
' Request:  {"cookie": "...", "cmd": "status|list|start|stop|restart|ping", "worker": "name"}
' Response: {"ok": true|false, ...} single line, then close.
' This endpoint is the CONTROL PLANE — it never touches pixels and never
' talks to the overlay/WebView family (that is NVIDIA Web Helper.exe's job).
Friend Class AuthorityServer

    Private ReadOnly _config As ContainerConfig
    Private ReadOnly _supervisor As Supervisor
    Private _listener As TcpListener
    Private _thread As Thread
    Private _running As Boolean = False
    Private ReadOnly _bootUtc As DateTime = DateTime.UtcNow

    Public Sub New(config As ContainerConfig, supervisor As Supervisor)
        _config = config
        _supervisor = supervisor
    End Sub

    Public Sub Start()
        _listener = New TcpListener(Net.IPAddress.Any, _config.Port)
        Try
            _listener.Start()
        Catch ex As Exception
            ContainerLog.Log("authority bind FAILED on port " & _config.Port.ToString() & ": " & ex.Message)
            Throw
        End Try
        _running = True
        _thread = New Thread(AddressOf AcceptLoop)
        _thread.IsBackground = True
        _thread.Start()
    End Sub

    Public Sub StopListening()
        _running = False
        Try
            If _listener IsNot Nothing Then _listener.Stop()
        Catch
        End Try
    End Sub

    Private Sub AcceptLoop()
        While _running
            Dim client As TcpClient = Nothing
            Try
                client = _listener.AcceptTcpClient()
            Catch ex As Exception
                If Not _running Then
                    Exit While
                End If
                ContainerLog.Log("authority accept error: " & ex.Message)
                Thread.Sleep(500)
                Continue While
            End Try
            Dim c As TcpClient = client
            Dim t As New Thread(Sub() HandleClient(c))
            t.IsBackground = True
            t.Start()
        End While
    End Sub

    Private Sub HandleClient(client As TcpClient)
        Try
            client.ReceiveTimeout = 5000
            client.SendTimeout = 5000
            Using client
                Dim stream As NetworkStream = client.GetStream()
                Dim req As String = ReadLine(stream, 8192)
                If req Is Nothing OrElse req.Trim() = "" Then
                    WriteLine(stream, "{""ok"": false, ""error"": ""empty request""}")
                    Return
                End If

                Dim cookie As String = ""
                Dim cmd As String = ""
                Dim worker As String = ""
                Try
                    Using doc As JsonDocument = JsonDocument.Parse(req)
                        Dim root As JsonElement = doc.RootElement
                        Dim prop As JsonElement
                        If root.TryGetProperty("cookie", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            cookie = prop.GetString()
                        End If
                        If root.TryGetProperty("cmd", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            cmd = prop.GetString().ToLowerInvariant()
                        End If
                        If root.TryGetProperty("worker", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            worker = prop.GetString()
                        End If
                    End Using
                Catch ex As Exception
                    WriteLine(stream, "{""ok"": false, ""error"": ""bad json: " & JsonEncode(ex.Message) & """}")
                    Return
                End Try

                If cookie <> _config.SecurityCookie Then
                    ContainerLog.Log("authority: REJECT (bad cookie) cmd='" & cmd & "' from " &
                                     client.Client.RemoteEndPoint?.ToString())
                    WriteLine(stream, "{""ok"": false, ""error"": ""auth""}")
                    Return
                End If

                WriteLine(stream, Dispatch(cmd, worker))
            End Using
        Catch ex As Exception
            ContainerLog.Log("authority client error: " & ex.Message)
        End Try
    End Sub

    Private Function Dispatch(cmd As String, worker As String) As String
        Select Case cmd
            Case "ping"
                Return "{""ok"": true, ""pong"": true}"

            Case "status"
                Dim d As New Dictionary(Of String, Object)
                d("ok") = True
                Dim c As New Dictionary(Of String, Object)
                c("port") = _config.Port
                c("uptimeSec") = CInt((DateTime.UtcNow - _bootUtc).TotalSeconds)
                c("workers") = _supervisor.StatusList()
                d("container") = c
                Return JsonSerializer.Serialize(d)

            Case "list"
                Dim d As New Dictionary(Of String, Object)
                d("ok") = True
                d("workers") = _supervisor.StatusList()
                Return JsonSerializer.Serialize(d)

            Case "start", "stop", "restart"
                If worker = "" Then
                    Return "{""ok"": false, ""error"": ""worker name required""}"
                End If
                If Not _supervisor.HasWorker(worker) Then
                    Return "{""ok"": false, ""error"": ""unknown worker: " & JsonEncode(worker) & """}"
                End If
                Dim done As Boolean = False
                If cmd = "start" Then
                    done = _supervisor.Start(worker)
                ElseIf cmd = "stop" Then
                    done = _supervisor.StopWorker(worker)
                Else
                    done = _supervisor.Restart(worker)
                End If
                If done Then
                    ContainerLog.Log("authority: " & cmd & " '" & worker & "' OK")
                    Return "{""ok"": true, ""cmd"": """ & cmd & """, ""worker"": """ & JsonEncode(worker) & """}"
                End If
                Return "{""ok"": false, ""error"": """ & cmd & " failed""}"

            Case ""
                Return "{""ok"": false, ""error"": ""missing cmd""}"

            Case Else
                Return "{""ok"": false, ""error"": ""unknown cmd: " & JsonEncode(cmd) & """}"
        End Select
    End Function

    Private Shared Function JsonEncode(s As String) As String
        Return JsonSerializer.Serialize(s).Trim(""""c)
    End Function

    Private Shared Function ReadLine(stream As NetworkStream, maxBytes As Integer) As String
        Dim buf As New List(Of Byte)
        Dim one(0) As Byte
        Try
            While buf.Count < maxBytes
                Dim n As Integer = stream.Read(one, 0, 1)
                If n <= 0 Then
                    Exit While
                End If
                If one(0) = 10 Then
                    Exit While
                End If
                If one(0) <> 13 Then
                    buf.Add(one(0))
                End If
            End While
        Catch
        End Try
        If buf.Count = 0 Then
            Return Nothing
        End If
        Return Encoding.UTF8.GetString(buf.ToArray())
    End Function

    Private Shared Sub WriteLine(stream As NetworkStream, s As String)
        Dim b As Byte() = Encoding.UTF8.GetBytes(s & vbCrLf)
        stream.Write(b, 0, b.Length)
        stream.Flush()
    End Sub

End Class
