Option Strict On
Option Explicit On
Option Infer On

' Program.vb — OBS runtime resilience + reconnect hardening harness.
'
' Every test drives the REAL production ObsWebSocketClient (linked source)
' against real loopback WebSocket conversations with the in-process
' TestObsServer. No mocks of the client itself.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks

Friend Module TestRunner
    Friend _passed As Integer = 0
    Friend _failed As Integer = 0
    Friend ReadOnly _failures As New List(Of String)()

    Friend Sub RunTest(name As String, test As Action)
        Console.Write($"  {name} ... ")
        Try
            test()
            Console.WriteLine("PASS")
            _passed += 1
        Catch ex As Exception
            Console.WriteLine("FAIL")
            Console.WriteLine($"      → {ex.Message}")
            _failures.Add(name & ": " & ex.Message)
            _failed += 1
        End Try
    End Sub

    Friend Sub Assert(cond As Boolean, message As String)
        If Not cond Then Throw New Exception(message)
    End Sub
End Module

Friend Module Program

    Function Main(args As String()) As Integer
        Console.WriteLine("==================================================")
        Console.WriteLine(" Notifier.Obs.Resilience.Tests")
        Console.WriteLine(" OBS runtime resilience + reconnect hardening")
        Console.WriteLine("==================================================")
        Console.WriteLine()

        ObsResilienceTests.RunAll()

        Console.WriteLine()
        Console.WriteLine("--------------------------------------------------")
        Console.WriteLine($" RESULT: {TestRunner._passed} passed, {TestRunner._failed} failed")
        If TestRunner._failures.Count > 0 Then
            Console.WriteLine(" Failures:")
            For Each f As String In TestRunner._failures
                Console.WriteLine($"   - {f}")
            Next
        End If
        Console.WriteLine("--------------------------------------------------")
        Return If(TestRunner._failed > 0, 1, 0)
    End Function

End Module

Friend Module ObsResilienceTests

    ' ── lifecycle observation (shared, reset per test) ───────────────
    Private ReadOnly _lock As New Object()
    Private _lifecycle As New List(Of String)()
    Private _connectedTcs As TaskCompletionSource(Of Boolean)
    Private _eventTcs As TaskCompletionSource(Of String)
    Private _connCount As Integer
    Private _disconnectedCount As Integer
    Private _reconnectingCount As Integer
    Private ReadOnly _eventCounts As New Dictionary(Of String, Integer)()

    Public Sub RunAll()
        TestRunner.RunTest("OBS-A: old connection callback after new connection — new connection stays authoritative", AddressOf Test_A_OldLoopAfterNewConnection)
        TestRunner.RunTest("OBS-B: dispose during reconnect — no reconnect after dispose, no leaks", AddressOf Test_B_DisposeDuringReconnect)
        TestRunner.RunTest("OBS-C: dead socket during SendRequest — bounded completion", AddressOf Test_C_DeadSocketSendRequest)
        TestRunner.RunTest("OBS-D: OBS dies mid-session — recovery attempted, events resume", AddressOf Test_D_DeviceDeathRecovery)
        TestRunner.RunTest("OBS-E: reconnect + SendRequest simultaneously — request belongs to its generation", AddressOf Test_E_ReconnectPlusSendRequest)
        TestRunner.RunTest("OBS-F: disconnect/reconnect x100 — one live socket, no stale callbacks", AddressOf Test_F_Flap100)
    End Sub

    Private Sub ResetState()
        SyncLock _lock
            _lifecycle = New List(Of String)()
            _eventCounts.Clear()
            _connCount = 0
            _disconnectedCount = 0
            _reconnectingCount = 0
            _connectedTcs = NewTcs()
            _eventTcs = NewTcsString()
        End SyncLock
    End Sub

    Private Function NewTcs() As TaskCompletionSource(Of Boolean)
        Return New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
    End Function

    Private Function NewTcsString() As TaskCompletionSource(Of String)
        Return New TaskCompletionSource(Of String)(TaskCreationOptions.RunContinuationsAsynchronously)
    End Function

    Private Sub Log(kind As String, Optional detail As String = "")
        SyncLock _lock
            _lifecycle.Add($"{DateTime.Now:HH:mm:ss.fff} {kind} {detail}")
        End SyncLock
    End Sub

    Private Sub Wire(client As ObsWebSocketClient)
        AddHandler client.OnConnected,
            Sub()
                Log("connected")
                Interlocked.Increment(_connCount)
                Dim tcs = _connectedTcs
                If tcs IsNot Nothing Then tcs.TrySetResult(True)
            End Sub
        AddHandler client.OnDisconnected,
            Sub()
                Log("disconnected")
                Interlocked.Increment(_disconnectedCount)
            End Sub
        AddHandler client.OnReconnecting,
            Sub()
                Log("reconnecting")
                Interlocked.Increment(_reconnectingCount)
            End Sub
        AddHandler client.OnEvent,
            Sub(eventType As String, eventData As JObject3, raw As JObject3)
                Log("event", eventType)
                SyncLock _lock
                    If _eventCounts.ContainsKey(eventType) Then
                        _eventCounts(eventType) += 1
                    Else
                        _eventCounts(eventType) = 1
                    End If
                End SyncLock
                Dim tcs = _eventTcs
                If tcs IsNot Nothing Then tcs.TrySetResult(eventType)
            End Sub
    End Sub

    ' Alias so the Wire lambda signature matches the client's event type.
    Private UsingJobjectAlias As Boolean = True
    Private UsingJobjectAlias2 As Boolean = UsingJobjectAlias

    Private Function WaitConnected(timeoutMs As Integer) As Boolean
        Dim tcs = NewTcs()
        _connectedTcs = tcs
        Return tcs.Task.Wait(timeoutMs)
    End Function

    Private Function WaitEvent(timeoutMs As Integer) As Boolean
        Dim tcs = NewTcsString()
        _eventTcs = tcs
        Return tcs.Task.Wait(timeoutMs)
    End Function

    Private Function WaitReconnecting(timeoutMs As Integer) As Boolean
        Dim deadline As Long = DateTime.UtcNow.Ticks + timeoutMs * 10000L
        While DateTime.UtcNow.Ticks < deadline
            If Interlocked.Read(_reconnectingCount) > 0 Then Return True
            Thread.Sleep(20)
        End While
        Return False
    End Function

    Private Function EventCount(eventType As String) As Integer
        SyncLock _lock
            Dim n As Integer = 0
            If _eventCounts.TryGetValue(eventType, n) Then Return n
            Return 0
        End SyncLock
    End Function

    ''' <summary>Lifecycle churn (disconnect/reconnecting) recorded AFTER the
    ''' last "connected" entry — the stale-loop fingerprint.</summary>
    Private Function ChurnAfterLastConnected() As Integer
        SyncLock _lock
            Dim lastConnected As Integer = -1
            For i As Integer = 0 To _lifecycle.Count - 1
                If _lifecycle(i).Contains(" connected") Then lastConnected = i
            Next
            Dim churn As Integer = 0
            For i As Integer = lastConnected + 1 To _lifecycle.Count - 1
                If _lifecycle(i).Contains(" disconnected") OrElse _lifecycle(i).Contains(" reconnecting") Then churn += 1
            Next
            Return churn
        End SyncLock
    End Function

    Private Function GetFreePort() As Integer
        Dim l As New TcpListener(IPAddress.Loopback, 0)
        l.Start()
        Dim port As Integer = CType(l.LocalEndpoint, IPEndPoint).Port
        l.Stop()
        Return port
    End Function

    ' ── Test A ─────────────────────────────────────────────────────────
    ' T0 old socket connected (loop asleep on it) → T1 reconnect/endpoint
    ' change begins → T2 new socket connected → T3/T4 old loop wakes on the
    ' disposed old socket AFTER the new connection is live.
    ' Expected: the new connection remains authoritative — no churn after
    ' the new OnConnected, exactly one server-side live connection, events
    ' delivered exactly once.

    Private Sub Test_A_OldLoopAfterNewConnection()
        ResetState()
        Dim s1 As New TestObsServer() With {.ServerTag = "A-old"}
        s1.Start()
        Dim s2 As New TestObsServer() With {.ServerTag = "A-new"}
        s2.Start()
        Try
            Using client As New ObsWebSocketClient("127.0.0.1", s1.Port, "", True)
                Wire(client)
                client._reconnectBaseDelayMs = 50
                client.Connect()
                TestRunner.Assert(WaitConnected(8000), "A: connected to old endpoint")
                TestRunner.Assert(s1.AliveCount = 1, "A: one live connection on old endpoint")

                ' T1: endpoint change retires epoch 1 and connects to s2.
                client.UpdateEndpoint("127.0.0.1", s2.Port, "")
                TestRunner.Assert(WaitConnected(8000), "A: connected to new endpoint")

                ' T3/T4: the old loop wakes on the disposed socket NOW — after
                ' the new connection is authoritative. Settle and check.
                Thread.Sleep(1500)
                TestRunner.Assert(ChurnAfterLastConnected() = 0,
                                  $"A: stale loop churned the new connection ({ChurnAfterLastConnected()} lifecycle events after last connect)")
                TestRunner.Assert(s2.AliveCount = 1, $"A: exactly one live connection on new endpoint (got {s2.AliveCount})")
                TestRunner.Assert(client.IsConnected, "A: client connected")

                s2.PushEvent("RecordStateChanged")
                TestRunner.Assert(WaitEvent(3000), "A: event from new connection delivered")
                Thread.Sleep(500)
                TestRunner.Assert(EventCount("RecordStateChanged") = 1,
                                  $"A: event delivered exactly once (got {EventCount("RecordStateChanged")})")
            End Using
        Finally
            s1.Stop()
            s2.Stop()
        End Try
    End Sub

    ' ── Test B ─────────────────────────────────────────────────────────

    Private Sub Test_B_DisposeDuringReconnect()
        ResetState()
        Dim deadPort As Integer = GetFreePort()   ' nothing listens here
        Dim client As New ObsWebSocketClient("127.0.0.1", deadPort, "", True)
        Wire(client)
        client._reconnectBaseDelayMs = 50
        client.Connect()
        TestRunner.Assert(WaitReconnecting(8000), "B: reconnect loop engaged against dead endpoint")

        client.Dispose()
        Thread.Sleep(300)

        Dim connBefore As Integer = Interlocked.Read(_connCount)
        Dim recBefore As Integer = Interlocked.Read(_reconnectingCount)

        ' A still-alive reconnect loop would hit this listener on its next
        ' attempt (backoff is 50ms — dozens of attempts fit in the window).
        Dim probe As New TcpListener(IPAddress.Loopback, deadPort)
        probe.Start()
        Dim acceptTask As Task(Of TcpClient) = probe.AcceptTcpClientAsync()
        Dim connected As Boolean = False
        Try
            connected = acceptTask.Wait(2000)
        Catch
            connected = False
        End Try
        probe.Stop()

        TestRunner.Assert(Not connected, "B: reconnect attempted after dispose (socket/task leak)")
        TestRunner.Assert(Interlocked.Read(_connCount) = connBefore, "B: no connection formed after dispose")
        TestRunner.Assert(Interlocked.Read(_reconnectingCount) = recBefore, "B: no new reconnect cycle after dispose")
        TestRunner.Assert(Not client.IsConnected, "B: client reports disconnected")
    End Sub

    ' ── Test C ─────────────────────────────────────────────────────────

    Private Sub Test_C_DeadSocketSendRequest()
        ResetState()
        Dim srv As New TestObsServer() With {.ServerTag = "C", .AutoRespond = False}
        srv.Start()
        Try
            Using client As New ObsWebSocketClient("127.0.0.1", srv.Port, "", True)
                Wire(client)
                client._reconnectBaseDelayMs = 50
                client.Connect()
                TestRunner.Assert(WaitConnected(8000), "C: connected")

                ' In-flight request; server holds it (AutoRespond=False).
                Dim reqTask = Task.Run(Function() client.SendRequest("GetStats", Nothing, 5000))
                Thread.Sleep(400)
                TestRunner.Assert(srv.SawRequest("GetStats"), "C: request reached the server")

                ' OBS dies mid-request.
                srv.KillAll()
                Dim result = reqTask.Result   ' must complete bounded — never hang
                TestRunner.Assert(result Is Nothing, "C: request failed cleanly (Nothing)")

                ' Fast-path: request while disconnected returns immediately.
                Dim sw As Stopwatch = Stopwatch.StartNew()
                Dim r2 = client.SendRequest("GetStats", Nothing, 5000)
                sw.Stop()
                TestRunner.Assert(r2 Is Nothing, "C: disconnected request returns Nothing")
                TestRunner.Assert(sw.ElapsedMilliseconds < 1000,
                                  $"C: fast fail when disconnected ({sw.ElapsedMilliseconds}ms)")
            End Using
        Finally
            srv.Stop()
        End Try
    End Sub

    ' ── Test D ─────────────────────────────────────────────────────────
    ' OBS-level device invalidation = the OBS process/endpoint dies. Expected:
    ' recovery IS attempted (disconnect → bounded reconnect → identify) and
    ' event delivery resumes. (Track-level fabricated silence is a WASAPI
    ' concern inside the capture engine — not observable from this bridge.)

    Private Sub Test_D_DeviceDeathRecovery()
        ResetState()
        Dim srv As New TestObsServer() With {.ServerTag = "D"}
        srv.Start()
        Try
            Using client As New ObsWebSocketClient("127.0.0.1", srv.Port, "", True)
                Wire(client)
                client._reconnectBaseDelayMs = 50
                client.Connect()
                TestRunner.Assert(WaitConnected(8000), "D: connected")

                srv.KillAll()   ' OBS dies
                TestRunner.Assert(WaitConnected(10000), "D: recovery attempted and reconnected")
                TestRunner.Assert(client.IsConnected, "D: client connected again")

                srv.PushEvent("ReplayBufferSaved")
                TestRunner.Assert(WaitEvent(3000), "D: event delivery resumed after recovery")
                Thread.Sleep(400)
                TestRunner.Assert(EventCount("ReplayBufferSaved") = 1, "D: resumed event delivered exactly once")
            End Using
        Finally
            srv.Stop()
        End Try
    End Sub

    ' ── Test E ─────────────────────────────────────────────────────────

    Private Sub Test_E_ReconnectPlusSendRequest()
        ResetState()
        Dim s1 As New TestObsServer() With {.ServerTag = "E-old", .AutoRespond = False}
        s1.Start()
        Dim s2 As New TestObsServer() With {.ServerTag = "E-new", .AutoRespond = True}
        s2.Start()
        Try
            Using client As New ObsWebSocketClient("127.0.0.1", s1.Port, "", True)
                Wire(client)
                client._reconnectBaseDelayMs = 50
                client.Connect()
                TestRunner.Assert(WaitConnected(8000), "E: connected (old generation)")

                ' Request in flight on the OLD generation; server holds it.
                Dim sw As Stopwatch = Stopwatch.StartNew()
                Dim reqTask = Task.Run(Function() client.SendRequest("GetStats", Nothing, 5000))
                Thread.Sleep(400)
                TestRunner.Assert(s1.SawRequest("GetStats"), "E: request issued on old generation")

                ' Reconnect (endpoint change) while the request is pending.
                client.UpdateEndpoint("127.0.0.1", s2.Port, "")
                Dim result = reqTask.Result
                sw.Stop()
                TestRunner.Assert(result Is Nothing, "E: old-generation request not answered by anything")
                TestRunner.Assert(sw.ElapsedMilliseconds < 4500,
                                  $"E: old-generation request unblocked promptly ({sw.ElapsedMilliseconds}ms < 5000ms timeout)")

                TestRunner.Assert(WaitConnected(8000), "E: new generation connected")
                TestRunner.Assert(Not s2.SawRequest("GetStats"), "E: new generation never received the old request")

                ' A request on the NEW generation works end-to-end.
                Dim r2 = client.SendRequest("GetVersion", Nothing, 5000)
                TestRunner.Assert(r2 IsNot Nothing, "E: new-generation request answered")
                TestRunner.Assert(r2("d")("responseData")("from").Value(Of String)() = "E-new",
                                  "E: response came from the new generation's server")
            End Using
        Finally
            s1.Stop()
            s2.Stop()
        End Try
    End Sub

    ' ── Test F ─────────────────────────────────────────────────────────

    Private Sub Test_F_Flap100()
        ResetState()
        Dim srv As New TestObsServer() With {.ServerTag = "F"}
        srv.Start()
        Try
            Using client As New ObsWebSocketClient("127.0.0.1", srv.Port, "", True)
                Wire(client)
                client._reconnectBaseDelayMs = 10
                client.Connect()
                TestRunner.Assert(WaitConnected(8000), "F: initial connection")

                For i As Integer = 1 To 100
                    srv.KillAll()
                    If Not WaitConnected(10000) Then
                        TestRunner.Assert(False, $"F: cycle {i} failed to reconnect within 10s")
                        Return
                    End If
                Next

                Thread.Sleep(1500)   ' settle: any stale loop would churn here

                TestRunner.Assert(srv.AliveCount = 1, $"F: exactly one live server-side connection (got {srv.AliveCount})")
                TestRunner.Assert(Interlocked.Read(_connCount) = 101,
                                  $"F: exactly 101 OnConnected (initial + 100 reconnects, got {_connCount})")
                TestRunner.Assert(Interlocked.Read(_disconnectedCount) = 100,
                                  $"F: exactly 100 OnDisconnected (one per OBS death, got {_disconnectedCount})")
                TestRunner.Assert(srv.TotalAccepted >= 101 AndAlso srv.TotalAccepted <= 103,
                                  $"F: server accepted 101-103 connections (got {srv.TotalAccepted})")

                srv.PushEvent("RecordStateChanged")
                TestRunner.Assert(WaitEvent(3000), "F: event delivered after stress")
                Thread.Sleep(500)
                TestRunner.Assert(EventCount("RecordStateChanged") = 1, "F: no duplicate/stale event delivery")
                TestRunner.Assert(client.IsConnected, "F: client still connected")
            End Using
        Finally
            srv.Stop()
        End Try
    End Sub

End Module
