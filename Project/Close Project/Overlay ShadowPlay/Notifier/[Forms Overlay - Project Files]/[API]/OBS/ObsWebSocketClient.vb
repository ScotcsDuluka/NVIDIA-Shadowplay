Imports System.IO
Imports System.Net.WebSockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports Newtonsoft.Json.Linq

' ObsWebSocketClient — OBS Studio WebSocket v5 bridge (Notifier side).
'
' ═══ RESILIENCE MODEL (2026-09-05 reconnect-hardening pass) ═══
'
' Connection epochs: every connection lifetime (Connect, ReconnectLoop
' attempt) owns a generation token from Interlocked.Increment(_generation).
' A loop/request that did not win the CURRENT generation is retired: it
' may not write connection state, raise lifecycle events, complete
' pending requests, or touch a socket it does not own. This closes the
' races where an old receive loop woke after an endpoint change and
' received on (or killed) the NEW connection.
'
' Bounded everywhere: ConnectAsync capped 5s, sends capped 2s, request
' waits capped per-call, reconnect sleeps are interruptible by Dispose,
' socket disconnects abort (dispose) instead of a graceful-close wait.
' Nothing on the caller side can block forever, and Dispose never leaves
' a live ownership path behind.
'
' State model (derived from the fields below — there is no enum):
'   Disposed      terminal, after Dispose(); no loop may reconnect
'   Reconnecting  _isReconnecting=True (guard: _reconnectLock)
'   Connecting    ConnectCore/ReconnectLoop between bump and adopt
'   Connected     _isConnected=True and a ReceiveLoop of the CURRENT
'                 generation is receiving on the current socket
'   (OBS-protocol "identified/Ready" is event-level only: op0→op1→op2
'    raises OnConnected; no dedicated state flag exists.)
' There is deliberately NO device/source state here: this client owns a
' WebSocket, not OBS sources. Track-level device recovery lives in the
' capture engine (WASAPI path), not in this bridge.

Public Class ObsWebSocketClient
    Implements IDisposable

    Private _ws As ClientWebSocket
    Private _cts As CancellationTokenSource
    Private _isConnected As Boolean
    Private _autoReconnect As Boolean
    Private _currentReconnectDelayMs As Integer = 1000
    Private Const MaxReconnectDelayMs As Integer = 30000

    ' ★ Reconnect backoff base. Production default 1000ms; the resilience
    ' test harness overrides it (same assembly — Friend) to make ×100
    ' cycles fast without changing production behavior.
    Friend _reconnectBaseDelayMs As Integer = 1000

    Private _host As String
    Private _port As Integer
    Private _password As String

    ' Per-epoch receive buffers. (Previously shared instance fields — a
    ' superseded loop could interleave into the new loop's message stream.)
    ' Allocated per loop now; messages are small, loops are one per epoch.

    Private _requestCounter As Integer = 0
    Private _pendingResponses As New Dictionary(Of String, TaskCompletionSource(Of JObject))()
    Private _pendingGenerations As New Dictionary(Of String, Integer)()
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

    ' Shutdown signal for reconnect sleeps — Dispose() wakes a sleeping
    ' ReconnectLoop immediately instead of letting the task linger through
    ' up to a 30s backoff. One handle per client instance (instances are
    ' rare — bridge start); deliberately not disposed so an in-flight
    ' WaitOne can never hit ObjectDisposedException.
    Private ReadOnly _shutdownEvent As New ManualResetEvent(False)

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
        Dim newGen As Integer = Interlocked.Increment(_generation)
        FailStalePendingRequests(newGen, "endpoint changed")

        _isConnected = False
        _isReconnecting = False
        _currentReconnectDelayMs = _reconnectBaseDelayMs

        Disconnect()
        Connect()
    End Sub

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected AndAlso _ws IsNot Nothing AndAlso _ws.State = WebSocketState.Open
        End Get
    End Property

    ''' <summary>
    ''' Begin connecting. Fully off the caller's thread: the previous inline
    ''' ConnectAsync.Wait(5000) blocked the Notifier UI thread (config watcher
    ''' tick) for up to 5s per attempt while OBS was down. Public contract
    ''' unchanged — fire-and-forget, no return value consumed by any caller.
    ''' </summary>
    Public Sub Connect()
        Task.Run(Sub() ConnectCore())
    End Sub

    Private Sub ConnectCore()
        ' New epoch: old ReceiveLoops/ReconnectLoops become stale no-ops.
        Dim myGen As Integer = Interlocked.Increment(_generation)
        FailStalePendingRequests(myGen, "superseded by a newer connection")
        Try
            Disconnect()
            _isReconnecting = False
            _cts = New CancellationTokenSource()
            _ws = New ClientWebSocket()
            Dim uri = New Uri($"ws://{_host}:{_port}/")
            Log("info", $"Connecting to OBS WebSocket at {uri}…")

            ' Fix: Wait(5000) on a timed-out task is a false positive — the
            ' old code set _isConnected=True and started ReceiveLoop against a
            ' socket still in Connecting. Cancel the pending attempt on timeout.
            Dim connectTask = _ws.ConnectAsync(uri, _cts.Token)
            If Not connectTask.Wait(5000) Then
                Try : _cts.Cancel() : Catch : End Try
                Log("error", "Connect timed out after 5s")
                _isConnected = False
                DisconnectIfCurrent(myGen)   ' own socket: dispose (leak fix)
                TryStartReconnect()
                Return
            End If

            ' ★ Adopt only if no newer epoch superseded us while connecting
            ' (ReconnectLoop / UpdateEndpoint / another Connect). The old code
            ' adopted unconditionally — its socket became an orphan with no
            ' receive loop, and the field was overwritten by the newer socket.
            SyncLock _reconnectLock
                If myGen <> CurrentGeneration() Then
                    Try : _ws.Dispose() : Catch : End Try
                    Log("info", "Connect superseded while connecting — discarded")
                    Return
                End If
                _isConnected = True
                _currentReconnectDelayMs = _reconnectBaseDelayMs
            End SyncLock
            Task.Run(Sub() ReceiveLoop(myGen))
        Catch ex As Exception
            Log("error", $"Connect failed: {ex.Message}")
            _isConnected = False
            DisconnectIfCurrent(myGen)
            TryStartReconnect()
        End Try
    End Sub

    Public Sub Disconnect()
        _isConnected = False
        If _cts IsNot Nothing Then
            Try : _cts.Cancel() : Catch : End Try
        End If
        If _ws IsNot Nothing Then
            ' ★ Bounded teardown: Abort (dispose) instead of the previous
            ' graceful CloseAsync(...).Wait(1000) — the graceful wait ran on
            ' the Notifier UI thread (config watcher) and a disposed/half-dead
            ' socket made every reconnect cycle stall the UI for up to 1s.
            ' Auto-reconnect owns recovery; the close handshake is expendable.
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
        ' ★ Epoch-scoped locals: the loop receives ONLY on the socket/CTS it
        ' was started for. The previous code read the shared _ws/_cts fields
        ' every iteration — after an endpoint change or reconnect the old loop
        ' could pass its while-check against the NEW connection's state and
        ' call ReceiveAsync on the NEW socket: two competing receivers, the
        ' loser throws, and if the loser was the NEW loop it fired
        ' OnDisconnected + spawned a reconnect against a healthy socket.
        Dim myWs As ClientWebSocket = _ws
        Dim myCts As CancellationTokenSource = _cts
        Dim receiveBuffer(8 * 1024 - 1) As Byte

        Try
            While myGen = CurrentGeneration() AndAlso
                  myWs IsNot Nothing AndAlso myWs Is _ws AndAlso
                  _isConnected AndAlso
                  myCts IsNot Nothing AndAlso Not myCts.IsCancellationRequested
                Dim result As WebSocketReceiveResult
                Dim receiveStream As New StringBuilder()

                Do
                    result = Await myWs.ReceiveAsync(New ArraySegment(Of Byte)(receiveBuffer), myCts.Token)
                    If result.MessageType = WebSocketMessageType.Close Then
                        Log("info", "OBS WebSocket closed by server")
                        Exit Do
                    End If
                    If result.Count > 0 Then
                        receiveStream.Append(Encoding.UTF8.GetString(receiveBuffer, 0, result.Count))
                    End If
                Loop While Not result.EndOfMessage

                If receiveStream.Length = 0 Then Continue While

                Dim rawStr = receiveStream.ToString()
                Try
                    Dim msg = JObject.Parse(rawStr)
                    HandleMessage(msg, myWs, myCts)
                Catch ex As Exception
                    Log("warn", $"Could not parse OBS message: {rawStr.Substring(0, Math.Min(rawStr.Length, 200))}")
                End Try
            End While
        Catch ex As OperationCanceledException
            ' Cancellation is a normal teardown path (Dispose/Disconnect).
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
        FailPendingRequestsOfGen(myGen, "connection lost")
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

    Private Sub HandleMessage(msg As JObject, myWs As ClientWebSocket, myCts As CancellationTokenSource)
        Dim op = msg("op")
        If op Is Nothing Then Return

        Dim opInt As Integer = op.Value(Of Integer)()

        Log("info", $"<< RECV op={opInt}: {msg.ToString(Newtonsoft.Json.Formatting.None)}")

        Select Case opInt
            Case 0
                HandleHello(msg, myWs, myCts)
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
                    ' ★ Epoch match: a response may only complete a request that
                    ' was issued on the SAME connection generation. Stale-era
                    ' requests were already failed when their epoch ended.
                    If _pendingResponses.ContainsKey(reqId) AndAlso
                       _pendingGenerations.ContainsKey(reqId) AndAlso
                       _pendingGenerations(reqId) = CurrentGeneration() Then
                        _pendingResponses(reqId).TrySetResult(d)
                        _pendingResponses.Remove(reqId)
                        _pendingGenerations.Remove(reqId)
                    End If
                End SyncLock
            Case Else
        End Select
    End Sub

    Private Sub HandleHello(helloMsg As JObject, myWs As ClientWebSocket, myCts As CancellationTokenSource)
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

        SendJson(payload, myWs, myCts)
        Log("info", "Sent Identify to OBS")
    End Sub

    Private Function BuildAuth(password As String, salt As String, challenge As String) As String
        Dim salted = Encoding.UTF8.GetBytes(password & salt)
        Dim secretBytes As Byte()
        Using sha As SHA256 = SHA256.Create()
            secretBytes = sha.ComputeHash(salted)
        End Using
        Dim secret = Convert.ToBase64String(secretBytes)
        Dim challengeBytes As Byte() = Encoding.UTF8.GetBytes(secret & challenge)
        Dim authBytes As Byte()
        Using sha As SHA256 = SHA256.Create()
            authBytes = sha.ComputeHash(challengeBytes)
        End Using
        Return Convert.ToBase64String(authBytes)
    End Function

    Private Sub SendJson(payload As JObject)
        SendJson(payload, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Epoch-scoped send: the ReceiveLoop path passes its OWN socket/CTS so a
    ''' message received on connection N can never be answered on connection
    ''' N+1. The parameterless overload (public-era contract) targets the
    ''' current fields as before.
    ''' </summary>
    Private Sub SendJson(payload As JObject, myWs As ClientWebSocket, myCts As CancellationTokenSource)
        Dim ws As ClientWebSocket = If(myWs, _ws)
        Dim cts As CancellationTokenSource = If(myCts, _cts)
        If Not IsConnected OrElse ws Is Nothing OrElse ws.State <> WebSocketState.Open Then
            Log("warn", $"SendJson skipped — not connected (payload={payload.ToString(Newtonsoft.Json.Formatting.None)})")
            Exit Sub
        End If
        Try
            Dim json = payload.ToString(Newtonsoft.Json.Formatting.None)
            Dim bytes = Encoding.UTF8.GetBytes(json)
            Log("info", $">> SEND {json}")
            ws.SendAsync(New ArraySegment(Of Byte)(bytes), WebSocketMessageType.Text, True, cts.Token).Wait(2000)
        Catch ex As Exception
            Log("error", $"SendJson failed: {ex.Message}")
            _isConnected = False
        End Try
    End Sub

    Public Function SendRequest(requestType As String, Optional requestData As JObject = Nothing, Optional timeoutMs As Integer = 5000) As JObject
        If Not IsConnected Then Return Nothing

        Dim id As String
        Dim myGen As Integer = CurrentGeneration()
        Dim tcs As New TaskCompletionSource(Of JObject)()
        SyncLock _pendingLock
            _requestCounter += 1
            id = _requestCounter.ToString()
            _pendingResponses(id) = tcs
            _pendingGenerations(id) = myGen
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
        Log("info", $"Sent request: {requestType} (id={id} gen={myGen})  payload={payload.ToString(Newtonsoft.Json.Formatting.None)}")

        ' Bounded by timeoutMs; additionally the wait ends early when the
        ' connection epoch ends (FailPendingRequests) — a dead socket can
        ' never hold a caller longer than the connection it belongs to.
        If tcs.Task.Wait(timeoutMs) Then
            Return tcs.Task.Result
        Else
            SyncLock _pendingLock
                If _pendingResponses.ContainsKey(id) Then
                    _pendingResponses.Remove(id)
                    _pendingGenerations.Remove(id)
                End If
            End SyncLock
            Log("warn", $"Request timed out: {requestType} (id={id})")
            Return Nothing
        End If
    End Function

    ''' <summary>Fail every pending request that belongs to the given epoch —
    ''' called when that epoch's connection ends, so no caller stays blocked
    ''' against a socket that can no longer answer. (Result = Nothing, same
    ''' contract as a timeout.)</summary>
    Private Sub FailPendingRequestsOfGen(gen As Integer, reason As String)
        SyncLock _pendingLock
            Dim stale As New List(Of String)()
            For Each kv In _pendingGenerations
                If kv.Value = gen Then stale.Add(kv.Key)
            Next
            RemoveAndFail(stale, reason)
        End SyncLock
    End Sub

    ''' <summary>Fail every pending request issued on an epoch OLDER than the
    ''' newly-bumped generation — called on every epoch transition (endpoint
    ''' change, reconnect, dispose). This is what keeps a dead connection from
    ''' holding a caller for the full request timeout.</summary>
    Private Sub FailStalePendingRequests(newGen As Integer, reason As String)
        SyncLock _pendingLock
            Dim stale As New List(Of String)()
            For Each kv In _pendingGenerations
                If kv.Value <> newGen Then stale.Add(kv.Key)
            Next
            RemoveAndFail(stale, reason)
        End SyncLock
    End Sub

    Private Sub RemoveAndFail(ids As List(Of String), reason As String)
        For Each id In ids
            Dim tcs As TaskCompletionSource(Of JObject) = Nothing
            If _pendingResponses.TryGetValue(id, tcs) Then
                _pendingResponses.Remove(id)
                _pendingGenerations.Remove(id)
                Log("info", $"request id={id} failed (connection epoch ended: {reason})")
                tcs.TrySetResult(Nothing)
            End If
        Next
    End Sub

    Private Sub ReconnectLoop()
        ' ★ Epoch fix: each attempt owns its OWN generation. The loop used to
        ' dispose the SHARED _cts/_ws fields — killing whatever newer
        ' connection was live by the time the backoff elapsed.
        Dim myGen As Integer = Interlocked.Increment(_generation)
        FailStalePendingRequests(myGen, "superseded by reconnect")
        RaiseEvent OnReconnecting()
        While Not IsConnected AndAlso _autoReconnect AndAlso myGen = CurrentGeneration()
            Try
                ' Interruptible backoff: Dispose() sets _shutdownEvent so the
                ' task exits promptly instead of sleeping through up to 30s
                ' after disposal (bounded-shutdown contract).
                If _shutdownEvent.WaitOne(_currentReconnectDelayMs) Then Exit While
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
                    DisconnectIfCurrent(myGen)
                    Continue While
                End If
                ' Adopt the connection only if no newer epoch superseded us.
                SyncLock _reconnectLock
                    If myGen <> CurrentGeneration() Then
                        Try : _ws.Dispose() : Catch : End Try
                        Exit While
                    End If
                    _isConnected = True
                    _currentReconnectDelayMs = _reconnectBaseDelayMs
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
        ' Retire every loop of every epoch before tearing the socket down,
        ' then wake any reconnect backoff sleep so the task exits promptly.
        Dim newGen As Integer = Interlocked.Increment(_generation)
        FailStalePendingRequests(newGen, "client disposed")
        Try : _shutdownEvent.Set() : Catch : End Try
        Disconnect()
    End Sub

End Class
