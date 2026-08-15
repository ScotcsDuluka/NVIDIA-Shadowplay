Imports System.Net.WebSockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports Newtonsoft.Json.Linq

Public Class ObsWebSocketClient
    Implements IDisposable

    Private _ws As ClientWebSocket
    Private _cts As CancellationTokenSource
    Private _isConnected As Boolean
    Private _autoReconnect As Boolean
    Private _currentReconnectDelayMs As Integer = 1000
    Private Const MaxReconnectDelayMs As Integer = 30000

    Private ReadOnly _host As String
    Private ReadOnly _port As Integer
    Private ReadOnly _password As String

    Private _receiveBuffer(8 * 1024 - 1) As Byte
    Private _receiveStream As New StringBuilder()

    Public Event OnEvent(eventType As String, eventData As JObject, raw As JObject)
    Public Event OnConnected()
    Public Event OnDisconnected()
    Public Event OnReconnecting()
    Public Event OnLog(level As String, message As String)

    Public Sub New(host As String, port As Integer, Optional password As String = "", Optional autoReconnect As Boolean = True)
        _host = host
        _port = port
        _password = password
        _autoReconnect = autoReconnect
    End Sub

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected AndAlso _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open
        End Get
    End Property

    Public Sub Connect()
        Try
            Disconnect()
            _cts = New CancellationTokenSource()
            _ws = New ClientWebSocket()
            Dim uri = New Uri($"ws://{_host}:{_port}/")
            Log("info", $"Connecting to OBS WebSocket at {uri}…")
            _ws.ConnectAsync(uri, _cts.Token).Wait(5000)
            _isConnected = True
            _currentReconnectDelayMs = 1000
            Task.Run(AddressOf ReceiveLoop)
        Catch ex As Exception
            Log("error", $"Connect failed: {ex.Message}")
            _isConnected = False
            If _autoReconnect Then Task.Run(AddressOf ReconnectLoop)
        End Try
    End Sub

    Public Sub Disconnect()
        _isConnected = False
        If _cts IsNot Nothing Then
            Try : _cts.Cancel() : Catch : End Try
        End If
        If _ws IsNot Nothing Then
            Try
                If _ws.State = WebSocketState.Open Then
                    _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None).Wait(1000)
                End If
            Catch : End Try
            Try : _ws.Dispose() : Catch : End Try
        End If
        _ws = Nothing
    End Sub

    Private Async Sub ReceiveLoop()
        Try
            While _isConnected AndAlso Not _cts.IsCancellationRequested
                Dim result As WebSocketReceiveResult
                _receiveStream.Clear()

                Do
                    result = Await _ws.ReceiveAsync(New ArraySegment(Of Byte)(_receiveBuffer), _cts.Token)
                    If result.MessageType = WebSocketMessageType.Close Then
                        Log("info", "OBS WebSocket closed by server")
                        Exit While
                    End If
                    If result.Count > 0 Then
                        _receiveStream.Append(Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count))
                    End If
                Loop While Not result.EndOfMessage

                If _receiveStream.Length = 0 Then Continue While

                Dim rawStr = _receiveStream.ToString()
                Try
                    Dim msg = JObject.Parse(rawStr)
                    HandleMessage(msg)
                Catch ex As Exception
                    Log("warn", $"Could not parse OBS message: {rawStr.Substring(0, Math.Min(rawStr.Length, 200))}")
                End Try
            End While
        Catch ex As Exception
            Log("error", $"ReceiveLoop error: {ex.Message}")
        End Try

        _isConnected = False
        RaiseEvent OnDisconnected()
        If _autoReconnect Then Task.Run(AddressOf ReconnectLoop)
    End Sub

    Private Sub HandleMessage(msg As JObject)
        Dim op = msg("op")
        If op Is Nothing Then Return

        Select Case op.Value(Of Integer)()
            Case 0
                HandleHello(msg)
            Case 2
                Log("info", "OBS WebSocket identified — listening for events")
                RaiseEvent OnConnected()
            Case 5
                Dim d As JObject = TryCast(msg("d"), JObject)
                If d Is Nothing Then Return
                Dim eventTypeTok As JToken = d("eventType")
                If eventTypeTok Is Nothing Then Return
                Dim eventType As String = eventTypeTok.Value(Of String)()
                If eventType Is Nothing Then Return
                Dim eventData As JObject = TryCast(d("eventData"), JObject)
                Log("info", $"Event received: {eventType}  data={If(eventData?.ToString(Newtonsoft.Json.Formatting.None), "{}")}")
                RaiseEvent OnEvent(eventType, eventData, msg)
            Case Else
        End Select
    End Sub

    Private Sub HandleHello(helloMsg As JObject)
        Dim d = helloMsg("d")
        If d Is Nothing Then Return

        Dim identify As New JObject()
        identify("rpcVersion") = 1

        Dim auth = d("authentication")
        If auth IsNot Nothing AndAlso Not String.IsNullOrEmpty(_password) Then
            Dim salt = auth("salt").Value(Of String)()
            Dim challenge = auth("challenge").Value(Of String)()
            identify("authentication") = BuildAuth(_password, salt, challenge)
        End If

        Dim payload As New JObject()
        payload("op") = 1
        payload("d") = identify

        SendJson(payload)
        Log("info", "Sent Identify to OBS")
    End Sub

    Private Function BuildAuth(password As String, salt As String, challenge As String) As String
        Dim salted = Encoding.UTF8.GetBytes(password & salt)
        Dim secretBytes As Byte()
        Using sha As SHA256 = SHA256.Create()
            secretBytes = sha.ComputeHash(salted)
        End Using
        Dim secret = Convert.ToBase64String(secretBytes)
        Dim challengeBytes = Encoding.UTF8.GetBytes(secret & challenge)
        Dim authBytes As Byte()
        Using sha As SHA256 = SHA256.Create()
            authBytes = sha.ComputeHash(challengeBytes)
        End Using
        Return Convert.ToBase64String(authBytes)
    End Function

    Private Sub SendJson(payload As JObject)
        If Not IsConnected Then Exit Sub
        Try
            Dim json = payload.ToString(Newtonsoft.Json.Formatting.None)
            Dim bytes = Encoding.UTF8.GetBytes(json)
            _ws.SendAsync(New ArraySegment(Of Byte)(bytes), WebSocketMessageType.Text, True, _cts.Token).Wait(2000)
        Catch ex As Exception
            Log("error", $"SendJson failed: {ex.Message}")
            _isConnected = False
        End Try
    End Sub

    Private Sub ReconnectLoop()
        RaiseEvent OnReconnecting()
        While Not IsConnected AndAlso _autoReconnect
            Try
                Thread.Sleep(_currentReconnectDelayMs)
                _currentReconnectDelayMs = Math.Min(_currentReconnectDelayMs * 2, MaxReconnectDelayMs)

                If _cts IsNot Nothing Then
                    Try : _cts.Cancel() : Catch : End Try
                    Try : _cts.Dispose() : Catch : End Try
                End If
                If _ws IsNot Nothing Then
                    Try : _ws.Dispose() : Catch : End Try
                End If

                _cts = New CancellationTokenSource()
                _ws = New ClientWebSocket()
                Dim uri = New Uri($"ws://{_host}:{_port}/")
                Log("info", $"Reconnecting to OBS WebSocket ({_currentReconnectDelayMs}ms backoff)…")
                _ws.ConnectAsync(uri, _cts.Token).Wait(5000)
                _isConnected = True
                _currentReconnectDelayMs = 1000
                Task.Run(AddressOf ReceiveLoop)
                Return
            Catch ex As Exception
                Log("warn", $"Reconnect failed: {ex.Message}")
                _isConnected = False
            End Try
        End While
    End Sub

    Private Sub Log(level As String, message As String)
        Dim line As String = $"[ObsWS:{level}] {DateTime.Now:HH:mm:ss.fff} {message}"
        Debug.WriteLine(line)
        WriteLogFile(line)
        RaiseEvent OnLog(level, message)
    End Sub

    Private Shared Sub WriteLogFile(line As String)
        Try
            Dim logPath As String = System.IO.Path.Combine(Application.StartupPath, "notifier_obs.log")
            Using fs As New FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
                Using sw As New StreamWriter(fs)
                    sw.WriteLine(line)
                End Using
            End Using
        Catch
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _autoReconnect = False
        Disconnect()
    End Sub

End Class
