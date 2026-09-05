Imports System.IO
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

    Private _host As String
    Private _port As Integer
    Private _password As String

    Private _receiveBuffer(8 * 1024 - 1) As Byte
    Private _receiveStream As New StringBuilder()

    Private _requestCounter As Integer = 0
    Private _pendingResponses As New Dictionary(Of String, TaskCompletionSource(Of JObject))
    Private _pendingLock As New Object()

    Private _isReconnecting As Boolean = False
    Private _reconnectLock As New Object()

    ' ★ Connection-epoch fix: every socket lifetime gets a generation. Old
    ' receive/reconnect loops must not mutate shared state or dispose sockets
    ' that belong to a NEWER epoch. Without this, UpdateEndpoint (called by
    ' the 2s notifier_obs.json watcher) + an old ReceiveLoop waking from its
    ' cancelled ReceiveAsync would: (1) clobber _isConnected of the new
    ' connection, (2) spawn a competing ReconnectLoop that DISPOSES the new
    ' live socket, (3) run two ReceiveLoops over the shared _receiveBuffer.
    Private _generation As Integer = 0

    Private Function CurrentGeneration() As Integer
        Return Volatile.Read(_generation)
    End Function

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

    Public ReadOnly Property CurrentHost As String
        Get
            Return _host
        End Get
    End Property

    Public ReadOnly Property CurrentPort As Integer
        Get
            Return _port
        End Get
    End Property

    Public ReadOnly Property CurrentPassword As String
        Get
            Return _password
        End Get
    End Property

    Public Sub UpdateEndpoint(host As String, port As Integer, Optional password As String = "")
        If host = _host AndAlso port = _port AndAlso password = _password Then Return

        Dim pwdStatus As String = If(password <> _password, "changed", "unchanged")
        Log("info", $"Endpoint changed: {_host}:{_port} → {host}:{port} (password {pwdStatus})")

        _host = host
        _port = port
        _password = password

        ' Bump the epoch FIRST: any old loop waking up sees a stale generation
        ' and silently retires instead of fighting the new connection.
        Interlocked.Increment(_generation)

        _isConnected = False
        _isReconnecting = False
        _currentReconnectDelayMs = 1000

        Disconnect()
        Connect()
    End Sub

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected AndAlso _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open
        End Get
    End Property

    Public Sub Connect()
        Try
            ' New epoch: old ReceiveLoops/ReconnectLoops become stale no-ops.
            Dim myGen As Integer = Interlocked.Increment(_generation)

            Disconnect()
            _isReconnecting = False
            _cts = New CancellationTokenSource()
            _ws = New ClientWebSocket()
            Dim uri = New Uri($"ws://{_host}:{_port}/")
            Log("info", $"Connecting to OBS WebSocket at {uri}…")

            ' ★ Fix: Wait(5000) on a timed-out task is a false positive — the
            ' old code set _isConnected=True and started ReceiveLoop against a
            ' socket still in Connecting. Cancel the pending attempt on timeout.
            Dim connectTask = _ws.ConnectAsync(uri, _cts.Token)
            If Not connectTask.Wait(5000) Then
                Try : _cts.Cancel() : Catch : End Try
                Log("error", "Connect timed out after 5s")
                _isConnected = False
                TryStartReconnect()
                Return
            End If
            _isConnected = True
            _currentReconnectDelayMs = 1000
            Task.Run(Sub() ReceiveLoop(myGen))
        Catch ex As Exception
            Log("error", $"Connect failed: {ex.Message}")
            _isConnected = False
            TryStartReconnect()
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

    ''' <summary>Epoch-scoped disconnect: only touches the current socket when
    ''' the caller's generation is still the live one — a stale reconnect loop
    ''' must never dispose a newer epoch's connection.</summary>
    Private Sub DisconnectIfCurrent(myGen As Integer)
        If myGen <> CurrentGeneration() Then Return
        Disconnect()
    End Sub

    Private Async Sub ReceiveLoop(myGen As Integer)
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

        ' ★ Epoch guard: a superseded loop (endpoint change / reconnect) must
        ' not touch the connection state of the CURRENT epoch or spawn a
        ' competing reconnect against the new socket.
        If myGen <> CurrentGeneration() Then
            Log("info", "ReceiveLoop retired (superseded by a newer connection)")
            Return
        End If
        _isConnected = False
        RaiseEvent OnDisconnected()
        TryStartReconnect()
    End Sub

    Private Sub TryStartReconnect()
        If Not _autoReconnect Then Return
        SyncLock _reconnectLock
            If _isReconnecting Then Return
            _isReconnecting = True
        End SyncLock
        Task.Run(AddressOf ReconnectLoop)
    End Sub

    Private Sub HandleMessage(msg As JObject)
        Dim op = msg("op")
        If op Is Nothing Then Return

        Dim opInt As Integer = op.Value(Of Integer)()

        Log("info", $"<< RECV op={opInt}: {msg.ToString(Newtonsoft.Json.Formatting.None)}")

        Select Case opInt
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
            Case 7
                Dim d As JObject = TryCast(msg("d"), JObject)
                If d Is Nothing Then Return
                Dim reqIdTok As JToken = d("requestId")
                If reqIdTok Is Nothing Then Return
                Dim reqId As String = reqIdTok.Value(Of String)()
                If reqId Is Nothing Then Return
                Log("info", $"  → matching requestId={reqId} (pending={_pendingResponses.Count})")
                SyncLock _pendingLock
                    If _pendingResponses.ContainsKey(reqId) Then
                        _pendingResponses(reqId).TrySetResult(d)
                        _pendingResponses.Remove(reqId)
                    End If
                End SyncLock
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
        If Not IsConnected Then
            Log("warn", $"SendJson skipped — not connected (payload={payload.ToString(Newtonsoft.Json.Formatting.None)})")
            Exit Sub
        End If
        Try
            Dim json = payload.ToString(Newtonsoft.Json.Formatting.None)
            Dim bytes = Encoding.UTF8.GetBytes(json)
            Log("info", $">> SEND {json}")
            _ws.SendAsync(New ArraySegment(Of Byte)(bytes), WebSocketMessageType.Text, True, _cts.Token).Wait(2000)
        Catch ex As Exception
            Log("error", $"SendJson failed: {ex.Message}")
            _isConnected = False
        End Try
    End Sub

    Public Function SendRequest(requestType As String, Optional requestData As JObject = Nothing, Optional timeoutMs As Integer = 5000) As JObject
        If Not IsConnected Then Return Nothing

        Dim id As String
        Dim tcs As New TaskCompletionSource(Of JObject)()
        SyncLock _pendingLock
            _requestCounter += 1
            id = _requestCounter.ToString()
            _pendingResponses(id) = tcs
        End SyncLock

        Dim payload As New JObject()
        payload("op") = 6
        payload("d") = New JObject()
        payload("d")("requestType") = requestType
        payload("d")("requestId") = id
        If requestData IsNot Nothing Then
            payload("d")("requestData") = requestData
        End If

        SendJson(payload)
        Log("info", $"Sent request: {requestType} (id={id})  payload={payload.ToString(Newtonsoft.Json.Formatting.None)}")

        If tcs.Task.Wait(timeoutMs) Then
            Return tcs.Task.Result
        Else
            SyncLock _pendingLock
                If _pendingResponses.ContainsKey(id) Then
                    _pendingResponses.Remove(id)
                End If
            End SyncLock
            Log("warn", $"Request timed out: {requestType} (id={id})")
            Return Nothing
        End If
    End Function

    Private Sub ReconnectLoop()
        ' ★ Epoch fix: each attempt owns its OWN generation. The loop used to
        ' dispose the SHARED _cts/_ws fields — killing whatever newer
        ' connection was live by the time the backoff elapsed.
        Dim myGen As Integer = Interlocked.Increment(_generation)
        RaiseEvent OnReconnecting()
        While Not IsConnected AndAlso _autoReconnect AndAlso myGen = CurrentGeneration()
            Try
                Thread.Sleep(_currentReconnectDelayMs)
                If myGen <> CurrentGeneration() Then Exit While
                _currentReconnectDelayMs = Math.Min(_currentReconnectDelayMs * 2, MaxReconnectDelayMs)

                DisconnectIfCurrent(myGen)

                _cts = New CancellationTokenSource()
                _ws = New ClientWebSocket()
                Dim uri = New Uri($"ws://{_host}:{_port}/")
                Log("info", $"Reconnecting to OBS WebSocket ({_currentReconnectDelayMs}ms backoff)…")
                Dim connectTask = _ws.ConnectAsync(uri, _cts.Token)
                If Not connectTask.Wait(5000) Then
                    Try : _cts.Cancel() : Catch : End Try
                    Log("warn", "Reconnect timed out after 5s")
                    Continue While
                End If
                ' Adopt the connection only if no newer epoch superseded us.
                SyncLock _reconnectLock
                    If myGen <> CurrentGeneration() Then
                        Try : _ws.Dispose() : Catch : End Try
                        Exit While
                    End If
                    _isConnected = True
                    _currentReconnectDelayMs = 1000
                    _isReconnecting = False
                End SyncLock
                Task.Run(Sub() ReceiveLoop(myGen))
                Return
            Catch ex As Exception
                Log("warn", $"Reconnect failed: {ex.Message}")
                _isConnected = False
            End Try
        End While

        SyncLock _reconnectLock
            If myGen = CurrentGeneration() Then
                _isReconnecting = False
            End If
        End SyncLock
    End Sub

    Private Sub Log(level As String, message As String)
        Dim line As String = $"[ObsWS:{level}] {DateTime.Now:HH:mm:ss.fff} {message}"
        Debug.WriteLine(line)
        WriteLogFile(line)
        RaiseEvent OnLog(level, message)
    End Sub

    Private Shared Sub WriteLogFile(line As String)
        Try
            Dim logPath As String = AppLayout.P("Logs", "notifier_obs.log")
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
        ' Retire every loop of every epoch before tearing the socket down.
        Interlocked.Increment(_generation)
        Disconnect()
    End Sub

End Class
