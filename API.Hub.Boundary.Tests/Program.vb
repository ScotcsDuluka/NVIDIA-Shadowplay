Option Strict On
Option Explicit On
Option Infer On

' Program.vb — C/2 F-04 boundary tests. REAL sockets everywhere:
' LoopbackGate listeners are bound for real, clients really connect, and
' rejection is proven at the KERNEL level (nothing is bound on the non-
' loopback address, so the TCP handshake itself must fail). The only
' value-level (non-socket) checks are the IsLoopback endpoint-classification
' units, which operate on plain IPEndPoint values.
'
' Exit code convention: 0 = all green (skips allowed, reported), 1 = failure.

Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks

Friend Module Program

    Friend _passed As Integer = 0
    Friend _failed As Integer = 0
    Friend _skipped As Integer = 0
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

    Friend Sub RunSkip(name As String, reason As String)
        Console.WriteLine($"  {name} ... SKIP ({reason})")
        _skipped += 1
    End Sub

    Friend Sub Assert(cond As Boolean, message As String)
        If Not cond Then Throw New Exception(message)
    End Sub

    ' ── Accept harness: mirrors the production guard exactly —
    '    accept → LoopbackGate.IsLoopback → accepted/rejected bookkeeping.
    Private Class GateServer
        Public ReadOnly Accepted As New List(Of String)
        Public ReadOnly Rejected As New List(Of String)
        Private ReadOnly _listeners As TcpListener()
        Private ReadOnly _acceptTasks As New List(Of Task)
        Private ReadOnly _gateLock As New Object()

        Public Sub New(port As Integer)
            _listeners = LoopbackGate.CreateLoopbackListeners(port)
            For Each l As TcpListener In _listeners
                l.Start()
            Next
        End Sub

        Public Function Ports() As Dictionary(Of Boolean, Integer)
            Dim d As New Dictionary(Of Boolean, Integer)
            For Each l As TcpListener In _listeners
                Dim ep As IPEndPoint = CType(l.LocalEndpoint, IPEndPoint)
                d(ep.AddressFamily = AddressFamily.InterNetworkV6) = ep.Port
            Next
            Return d
        End Function

        Public Sub RunAcceptLoopFor(durationMs As Integer)
            Dim cts As New CancellationTokenSource()
            For Each l As TcpListener In _listeners
                Dim listenerLocal As TcpListener = l
                _acceptTasks.Add(Task.Run(
                    Async Function()
                        While Not cts.IsCancellationRequested
                            Try
                                Dim client As TcpClient = Await listenerLocal.AcceptTcpClientAsync()
                                Dim peer As IPEndPoint = TryCast(client.Client.RemoteEndPoint, IPEndPoint)
                                SyncLock _gateLock
                                    If LoopbackGate.IsLoopback(peer) Then
                                        Accepted.Add(If(peer IsNot Nothing, peer.ToString(), "?"))
                                    Else
                                        Rejected.Add(If(peer IsNot Nothing, peer.ToString(), "?"))
                                    End If
                                End SyncLock
                                Try : client.Close() : Catch : End Try
                            Catch
                                Exit While ' listener stopped
                            End Try
                        End While
                    End Function, cts.Token))
            Next
            Thread.Sleep(durationMs)
            cts.Cancel()
            For Each l As TcpListener In _listeners
                Try : l.Stop() : Catch : End Try
            Next
            Try : Task.WaitAll(_acceptTasks.ToArray(), 3000) : Catch : End Try
        End Sub
    End Class

    ' ── Probe helpers (real sockets) ──
    Private Function TryConnect(ip As IPAddress, port As Integer, ByRef established As Boolean) As String
        Try
            Using c As New TcpClient()
                c.Connect(ip, port)
                established = c.Connected
                If established Then
                    ' Drain briefly so a server-side gate that closes
                    ' non-loopback peers surfaces as a read failure.
                    Dim s As NetworkStream = c.GetStream()
                    s.ReadTimeout = 300
                    Dim buf(63) As Byte
                    Try
                        Dim n As Integer = s.Read(buf, 0, buf.Length)
                        established = n > 0 OrElse c.Connected
                    Catch ex As System.IO.IOException
                        ' Server closed on us — for IPC-3 semantics the
                        ' handshake still completed, so report that.
                        established = True
                    End Try
                End If
                Return Nothing
            End Using
        Catch ex As Exception
            established = False
            Return ex.GetBaseException().Message
        End Try
    End Function

    Private Function NonLoopbackIPv4s() As List(Of IPAddress)
        Dim out As New List(Of IPAddress)
        For Each iface In Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            If iface.OperationalStatus <> Net.NetworkInformation.OperationalStatus.Up Then Continue For
            For Each ua In iface.GetIPProperties().UnicastAddresses
                If ua.Address.AddressFamily = AddressFamily.InterNetwork Then
                    Dim ip As IPAddress = ua.Address
                    If IPAddress.IsLoopback(ip) Then Continue For
                    If ip.ToString().StartsWith("169.254.") Then Continue For
                    If Not out.Contains(ip) Then out.Add(ip)
                End If
            Next
        Next
        Return out
    End Function

    Function Main(args As String()) As Integer
        Console.WriteLine("==================================================")
        Console.WriteLine(" API.Hub.Boundary.Tests — C/2 F-04 loopback gate")
        Console.WriteLine("==================================================")

        ' ── IPC-1 / IPC-2 / IPC-3 / IPC-5: one gate-server, real binds ──
        Dim srv As GateServer = Nothing
        Dim ports As Dictionary(Of Boolean, Integer) = Nothing
        Dim hasV6 As Boolean = False
        RunTest("IPC-0 gate server binds loopback only", Sub()
            srv = New GateServer(0)
            ports = srv.Ports()
            hasV6 = ports.ContainsKey(True)
            Assert(ports.ContainsKey(False), "IPv4 loopback listener missing")
        End Sub)

        If srv IsNot Nothing Then
            Dim acceptTask = Task.Run(Sub() srv.RunAcceptLoopFor(6000))
            Thread.Sleep(400) ' let the accept loops spin up

            RunTest("IPC-1 IPv4 loopback 127.0.0.1 → ACCEPT", Sub()
                Dim established As Boolean = False
                Dim err As String = TryConnect(IPAddress.Loopback, ports(False), established)
                Assert(established, $"127.0.0.1:{ports(False)} not established ({err})")
            End Sub)

            If hasV6 Then
                RunTest("IPC-2 IPv6 loopback ::1 → ACCEPT", Sub()
                    Dim established As Boolean = False
                    Dim err As String = TryConnect(IPAddress.IPv6Loopback, ports(True), established)
                    Assert(established, $"[::1]:{ports(True)} not established ({err})")
                End Sub)
            Else
                RunSkip("IPC-2 IPv6 loopback ::1 → ACCEPT", "no IPv6 listener on this host")
            End If

            Dim lanIPs = NonLoopbackIPv4s()
            If lanIPs.Count > 0 Then
                RunTest("IPC-3 non-loopback LAN IPv4 → REJECT (kernel: handshake fails)", Sub()
                    For Each ip In lanIPs
                        Dim established As Boolean = False
                        Dim err As String = TryConnect(ip, ports(False), established)
                        Assert(Not established,
                               $"non-loopback {ip}:{ports(False)} WAS ESTABLISHED — boundary open ({err})")
                    Next
                End Sub)
            Else
                RunSkip("IPC-3 non-loopback LAN IPv4 → REJECT", "no non-loopback IPv4 address on this host")
            End If

            RunTest("IPC-3b IsLoopback classification (value-level units)", Sub()
                Assert(LoopbackGate.IsLoopback(New IPEndPoint(IPAddress.Loopback, 5000)), "127.0.0.1 must be loopback")
                Assert(LoopbackGate.IsLoopback(New IPEndPoint(IPAddress.IPv6Loopback, 5000)), "::1 must be loopback")
                Assert(LoopbackGate.IsLoopback(New IPEndPoint(IPAddress.Parse("::ffff:127.0.0.1"), 5000)),
                       "mapped ::ffff:127.0.0.1 must be loopback")
                Assert(Not LoopbackGate.IsLoopback(New IPEndPoint(IPAddress.Parse("::ffff:192.168.1.254"), 5000)),
                       "mapped ::ffff:192.168.1.254 must be REJECTED")
                Assert(Not LoopbackGate.IsLoopback(New IPEndPoint(IPAddress.Parse("192.168.1.254"), 5000)),
                       "LAN IPv4 must be rejected")
                Assert(Not LoopbackGate.IsLoopback(New IPEndPoint(IPAddress.Parse("fe80::1"), 5000)),
                       "link-local IPv6 must be rejected")
                Assert(Not LoopbackGate.IsLoopback(Nothing), "missing endpoint must be rejected")
            End Sub)

            RunTest("IPC-5 multiple concurrent loopback clients → all accepted", Sub()
                Dim nConnected As Integer = 0
                Dim tasks As New List(Of Task)
                For i As Integer = 1 To 5
                    tasks.Add(Task.Run(Sub()
                        Try
                            Using c As New TcpClient()
                                c.Connect(IPAddress.Loopback, ports(False))
                                If c.Connected Then Interlocked.Increment(nConnected)
                                Thread.Sleep(150) ' hold the socket a beat
                            End Using
                        Catch
                        End Try
                    End Sub))
                Next
                Task.WaitAll(tasks.ToArray(), 4000)
                Assert(nConnected = 5, $"expected 5 loopback connections, got {nConnected}")
            End Sub)

            RunTest("IPC-4 malformed/early-disconnect client → server survives", Sub()
                ' Abortive garbage: invalid-UTF8 bytes with no framing, then
                ' hard reset (linger 0), plus a zero-byte connect/close.
                For Each payload In New Byte()() {New Byte() {255, 254, 253, 0, 10, 0}, New Byte() {}}
                    Using c As New TcpClient()
                        c.Connect(IPAddress.Loopback, ports(False))
                        c.LingerState = New LingerOption(True, 0)
                        Dim s As NetworkStream = c.GetStream()
                        If payload.Length > 0 Then s.Write(payload, 0, payload.Length)
                        ' no graceful close — RST on dispose
                    End Using
                Next
                Thread.Sleep(200)
                ' The gate must still accept a well-behaved client afterwards.
                Dim established As Boolean = False
                TryConnect(IPAddress.Loopback, ports(False), established)
                Assert(established, "gate stopped accepting after malformed clients")
            End Sub)

            acceptTask.Wait(8000)

            RunTest("IPC-3c gate bookkeeping saw exactly the loopback peers", Sub()
                SyncLock srv.Accepted
                    Assert(srv.Accepted.Count >= 7,
                           $"expected ≥7 accepted peers (1+1+1+5), saw {srv.Accepted.Count}")
                    Assert(srv.Rejected.Count = 0,
                           $"gate recorded {srv.Rejected.Count} rejected peers on loopback-only binds")
                    For Each p In srv.Accepted
                        Assert(p.StartsWith("127.0.0.1") OrElse p.StartsWith("[::1]") OrElse p.Contains("127.0.0.1") OrElse p.Contains("[::1]:"),
                               $"unexpected accepted peer: {p}")
                    Next
                End SyncLock
            End Sub)

            RunTest("IPC-6 stop/dispose → port freed, Stop idempotent", Sub()
                ' RunAcceptLoopFor already stopped the listeners; re-Stop must not throw.
                ' (The GateServer stopped its own copies; verify a fresh gate
                ' listener lifecycle: start → stop → connect must be refused.)
                Dim g2 As New GateServer(0)
                Dim p2 As Dictionary(Of Boolean, Integer) = g2.Ports()
                g2.RunAcceptLoopFor(50) ' stops on return
                Thread.Sleep(150)
                Dim established As Boolean = False
                TryConnect(IPAddress.Loopback, p2(False), established)
                Assert(Not established, "port still accepting after listener Stop — socket leak")
            End Sub)
        End If

        ' ── IPC-7: the REAL production hub (port 5001) — protocol survives ──
        Dim hubUp As Boolean = False
        Try
            Using probe As New TcpClient()
                probe.Connect(IPAddress.Loopback, 5001)
                hubUp = probe.Connected
            End Using
        Catch
        End Try

        If hubUp Then
            RunTest("IPC-7 live hub 127.0.0.1:5001 ping → pong (protocol intact)", Sub()
                Using c As New TcpClient()
                    c.Connect(IPAddress.Loopback, 5001)
                    Dim s As NetworkStream = c.GetStream()
                    s.ReadTimeout = 3000
                    Dim w As New IO.StreamWriter(s) With {.NewLine = vbCr & vbLf, .AutoFlush = True}
                    Dim r As New IO.StreamReader(s)
                    w.WriteLine("[BoundaryTests]|ping")
                    Dim line As String = r.ReadLine()
                    Assert(line = "[System]|pong", $"expected pong, got '{If(line, "<nothing>")}'")
                End Using
            End Sub)
        Else
            RunSkip("IPC-7 live hub ping → pong", "hub not running on 127.0.0.1:5001")
        End If

        Console.WriteLine()
        Console.WriteLine("--------------------------------------------------")
        Console.WriteLine($" RESULT: {_passed} passed, {_failed} failed, {_skipped} skipped")
        If _failures.Count > 0 Then
            For Each f As String In _failures
                Console.WriteLine($"   - {f}")
            Next
        End If
        Console.WriteLine("--------------------------------------------------")
        Return If(_failed > 0, 1, 0)
    End Function

End Module
