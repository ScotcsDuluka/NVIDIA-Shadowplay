Option Strict On
Option Explicit On
Option Infer On

' TestObsServer.vb — an in-process OBS-protocol WebSocket server over a raw
' TcpListener (no HttpListener URL-ACL requirements). Implements just enough
' of the OBS WebSocket v5 protocol for the resilience harness:
'
'   op 0 (Hello)          → sent on accept (when AutoIdentify)
'   op 1 (Identify)       → answered with op 2 (Identified) + OnIdentified
'   op 5 (Event)          → PushEvent() sends an event message
'   op 6 (Request)        → AutoRespond answers op 7 echoing the requestId
'
' KillAll() disposes every live connection server-side — the "OBS dies"
' simulation. Everything is loopback + ephemeral ports.

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Friend NotInheritable Class TestObsServer

    Friend NotInheritable Class ObsConnection
        Public ReadOnly Property Id As Guid = Guid.NewGuid()
        Public ReadOnly Property Tag As String
        Public ReadOnly Property Client As TcpClient
        Public Property Alive As Boolean = True

        Public Sub New(tag As String, client As TcpClient)
            _tag = tag
            _client = client
        End Sub

        Friend Sub Kill()
            Alive = False
            Try : _client.Close() : Catch : End Try
        End Sub
    End Class

    Private _listener As TcpListener
    Private ReadOnly _clients As New ConcurrentDictionary(Of Guid, ObsConnection)()
    Private _accepting As Boolean = False
    Private _acceptTask As Task = Nothing
    Private _connCounter As Integer = 0

    Public Property AutoIdentify As Boolean = True
    Public Property AutoRespond As Boolean = True
    Public Property ServerTag As String = "srv"

    Public ReadOnly Property Port As Integer
    Public ReadOnly Property TotalAccepted As Integer
        Get
            Return Interlocked.Read(_acceptedCounter)
        End Get
    End Property
    Private _acceptedCounter As Long = 0

    Public Event OnIdentified(conn As ObsConnection)
    Public Event OnRequest(conn As ObsConnection, requestType As String, requestId As String)

    Public ReadOnly Property AliveCount As Integer
        Get
            Dim n As Integer = 0
            For Each kv In _clients
                If kv.Value.Alive Then n += 1
            Next
            Return n
        End Get
    End Property

    Public Sub Start()
        _listener = New TcpListener(IPAddress.Loopback, 0)
        _listener.Start()
        _port = CType(_listener.LocalEndpoint, IPEndPoint).Port
        _accepting = True
        _acceptTask = Task.Run(AddressOf AcceptLoopAsync)
    End Sub

    Public Sub [Stop]()
        _accepting = False
        Try : _listener.Stop() : Catch : End Try
        KillAll()
    End Sub

    ''' <summary>Dispose every live connection — simulates OBS dying.</summary>
    Public Sub KillAll()
        For Each kv In _clients
            kv.Value.Kill()
        Next
    End Sub

    Public Sub PushEvent(eventType As String, Optional eventData As String = "{}")
        Dim msg As String = $"{{""op"":5,""d"":{{""eventType"":""{eventType}"",""eventData"":{eventData}}}}}"
        For Each kv In _clients
            If kv.Value.Alive Then
                Try
                    SendServerText(kv.Value, msg)
                Catch
                End Try
            End If
        Next
    End Sub

    Private Async Function AcceptLoopAsync() As Task
        While _accepting
            Try
                Dim client As TcpClient = Await _listener.AcceptTcpClientAsync()
                Interlocked.Increment(_acceptedCounter)
                Dim conn As New ObsConnection($"{ServerTag}#{Interlocked.Increment(_connCounter)}", client)
                _clients(conn.Id) = conn
                Task.Run(Sub() HandleClientAsync(conn))
            Catch ex As Exception When Not _accepting
                Exit While
            Catch ex As Exception
                ' accept failure while still accepting — brief retry
                Thread.Sleep(10)
            End Try
        End While
    End Function

    Private Async Sub HandleClientAsync(conn As ObsConnection)
        Try
            Dim stream As NetworkStream = conn.Client.GetStream()
            Dim key As String = ReadHandshakeKey(stream)
            SendHandshakeAccept(stream, key)

            If AutoIdentify Then
                SendServerText(conn, "{\""op\"":0,""d\"":{}}")
            End If

            While conn.Alive AndAlso _accepting
                Dim text As String = Await ReadClientFrameAsync(stream)
                If text Is Nothing Then Exit While

                Dim msg As Newtonsoft.Json.Linq.JObject = Nothing
                Try
                    msg = Newtonsoft.Json.Linq.JObject.Parse(text)
                Catch
                    Continue While
                End Try

                Dim opTok = msg("op")
                If opTok Is Nothing Then Continue While
                Dim op As Integer = opTok.Value(Of Integer)()

                If op = 1 Then
                    SendServerText(conn, "{\""op\"":2,\""d\"":{}}")
                    RaiseEvent OnIdentified(conn)
                ElseIf op = 6 Then
                    Dim d = msg("d")
                    Dim reqType As String = If(d?("requestType")?.Value(Of String)(), "")
                    Dim reqId As String = If(d?("requestId")?.Value(Of String)(), "")
                    RaiseEvent OnRequest(conn, reqType, reqId)
                    If AutoRespond Then
                        Dim resp As String = "{{""op"":7,""d"":{{""requestType"":""{0}"",""requestId"":""{1}"",""responseData"":{{""ok"":true,""from"":""{2}""}}}}}}"
                        SendServerText(conn, String.Format(resp, reqType, reqId, ServerTag))
                    End If
                End If
            End While
        Catch
            ' socket death (KillAll / client abort) — normal for this harness
        Finally
            conn.Alive = False
            Dim removed As ObsConnection = Nothing
            _clients.TryRemove(conn.Id, removed)
            Try : conn.Client.Close() : Catch : End Try
        End Try
    End Sub

    ' ── WebSocket framing (RFC 6455, minimal) ──────────────────────────

    Private Shared Function ReadHandshakeKey(stream As NetworkStream) As String
        Dim buffer(4095) As Byte
        Dim sb As New StringBuilder()
        While Not sb.ToString().EndsWith(vbCr & vbLf & vbCr & vbLf, StringComparison.Ordinal)
            Dim n As Integer = stream.Read(buffer, 0, buffer.Length)
            If n <= 0 Then Throw New IOException("handshake stream closed")
            sb.Append(Encoding.ASCII.GetString(buffer, 0, n))
        End While
        For Each line As String In sb.ToString().Split({vbCr & vbLf}, StringSplitOptions.RemoveEmptyEntries)
            Dim idx As Integer = line.IndexOf(":", StringComparison.Ordinal)
            If idx > 0 AndAlso line.Substring(0, idx).Trim().Equals("Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase) Then
                Return line.Substring(idx + 1).Trim()
            End If
        Next
        Throw New IOException("no Sec-WebSocket-Key in handshake")
    End Function

    Private Shared Sub SendHandshakeAccept(stream As NetworkStream, key As String)
        Dim sha As SHA256 = Nothing
        Using sha1 As System.Security.Cryptography.SHA1 = System.Security.Cryptography.SHA1.Create()
            Dim acceptBytes As Byte() = sha1.ComputeHash(Encoding.ASCII.GetBytes(key & "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))
            Dim accept As String = Convert.ToBase64String(acceptBytes)
            Dim resp As String = "HTTP/1.1 101 Switching Protocols" & vbCr & vbLf &
                                 "Upgrade: websocket" & vbCr & vbLf &
                                 "Connection: Upgrade" & vbCr & vbLf &
                                 $"Sec-WebSocket-Accept: {accept}" & vbCr & vbLf & vbCr & vbLf
            Dim bytes As Byte() = Encoding.ASCII.GetBytes(resp)
            stream.Write(bytes, 0, bytes.Length)
        End Using
    End Sub

    ''' <summary>Read one client frame (masked), return its text payload.
    ''' Nothing = connection closed / fatal framing error.</summary>
    Private Shared Async Function ReadClientFrameAsync(stream As NetworkStream) As Task(Of String)
        Dim header(1) As Byte
        If Not Await ReadExactAsync(stream, header, 2) Then Return Nothing
        Dim opcode As Integer = header(0) And &HF
        Dim masked As Boolean = (header(1) And &H80) <> 0
        Dim len As Long = header(1) And &H7F

        If len = 126 Then
            Dim ext(1) As Byte
            If Not Await ReadExactAsync(stream, ext, 2) Then Return Nothing
            len = (CLng(ext(0)) << 8) Or ext(1)
        ElseIf len = 127 Then
            Dim ext(7) As Byte
            If Not Await ReadExactAsync(stream, ext, 8) Then Return Nothing
            len = 0
            For i As Integer = 0 To 7
                len = (len << 8) Or ext(i)
            Next
        End If

        Dim mask(3) As Byte
        If masked AndAlso Not Await ReadExactAsync(stream, mask, 4) Then Return Nothing

        If opcode = 8 Then Return Nothing   ' close

        Dim payload(CInt(len - 1)) As Byte
        If len > 0 AndAlso Not Await ReadExactAsync(stream, payload, CInt(len)) Then Return Nothing

        If masked Then
            For i As Integer = 0 To payload.Length - 1
                payload(i) = payload(i) Xor mask(i Mod 4)
            Next
        End If
        Return Encoding.UTF8.GetString(payload)
    End Function

    Private Shared Async Function ReadExactAsync(stream As NetworkStream, buffer As Byte(), count As Integer) As Task(Of Boolean)
        Dim offset As Integer = 0
        While offset < count
            Dim n As Integer = Await stream.ReadAsync(buffer, offset, count - offset)
            If n <= 0 Then Return False
            offset += n
        End While
        Return True
    End Function

    Private Shared Sub SendServerText(conn As ObsConnection, text As String)
        Dim payload As Byte() = Encoding.UTF8.GetBytes(text)
        Dim frame As New List(Of Byte)()
        frame.Add(&H81)   ' FIN + text
        If payload.Length <= 125 Then
            frame.Add(CByte(payload.Length))
        ElseIf payload.Length <= &HFFFF Then
            frame.Add(CByte(126))
            frame.Add(CByte((payload.Length >> 8) And &HFF))
            frame.Add(CByte(payload.Length And &HFF))
        Else
            frame.Add(CByte(127))
            Dim l As Long = payload.Length
            For shift As Integer = 56 To 0 Step -8
                frame.Add(CByte((l >> shift) And &HFF))
            Next
        End If
        frame.AddRange(payload)
        Dim bytes As Byte() = frame.ToArray()
        conn.Client.GetStream().Write(bytes, 0, bytes.Length)
    End Sub

End Class
