Option Strict On
Option Explicit On
Option Infer On

' LoopbackGate.vb — F-04: the Hub's loopback trust boundary, as REAL
' production code shared by the Hub (API Server.vb) and the boundary
' regression tests (same source, zero copy drift).
'
' Contract (C/2 F-04):
'   ALLOWED peers : 127.0.0.1, ::1 (IPv4-mapped loopback forms included)
'   REJECTED      : every other endpoint — LAN IPv4, LAN IPv6, anything
'
' Two enforcement layers, strongest first:
'   1. BIND-LEVEL (kernel): the Hub only ever binds loopback addresses —
'      127.0.0.1 (IPv4) and ::1 (IPv6, IPv6Only=True so the socket can never
'      be widened to a dual-stack wildcard). A non-loopback peer cannot even
'      complete the TCP handshake; nothing application-side can regress that.
'   2. ACCEPT-LEVEL (defense in depth): the accepted socket's RemoteEndPoint
'      is re-checked with IsLoopback() so a future bind change (or a
'      platform dual-stack surprise) cannot silently reopen the Hub.

Imports System.Net
Imports System.Net.Sockets

Public NotInheritable Class LoopbackGate

    Private Sub New()
    End Sub

    ''' <summary>
    ''' True only when the endpoint is one of the machine's loopback
    ''' addresses (127.0.0.0/8 loopback forms and ::1). IPv4-mapped IPv6
    ''' representations (e.g. ::ffff:127.0.0.1 — what a dual-stack socket
    ''' reports for IPv4 peers) are unwrapped before the check; mapped
    ''' NON-loopback addresses (e.g. ::ffff:192.168.1.254) stay rejected.
    ''' </summary>
    Public Shared Function IsLoopback(ep As IPEndPoint) As Boolean
        If ep Is Nothing Then Return False
        Dim ip As IPAddress = ep.Address
        If ip.IsIPv4MappedToIPv6 Then ip = ip.MapToIPv4()
        Return IPAddress.IsLoopback(ip)
    End Function

    ''' <summary>
    ''' Create the Hub's listeners: IPv4 127.0.0.1 always; IPv6 ::1 when the
    ''' OS supports it (both are bound explicitly and independently — never
    ''' a wildcard — so the loopback boundary holds even when one family is
    ''' unavailable). Callers Start() the returned listeners themselves.
    ''' </summary>
    Public Shared Function CreateLoopbackListeners(port As Integer) As TcpListener()
        Dim listeners As New List(Of TcpListener)()

        Try
            Dim v6 As New TcpListener(IPAddress.IPv6Loopback, port)
            ' IPv6Only=True pins this socket to ::1 — it can never grow into
            ' a dual-stack wildcard that would accept LAN IPv4 as mapped
            ' traffic (the F-04 regression this gate exists to prevent).
            v6.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, True)
            listeners.Add(v6)
        Catch
            ' No usable IPv6 loopback (rare) — the IPv4 listener still
            ' carries the full IPC surface.
        End Try

        listeners.Add(New TcpListener(IPAddress.Loopback, port))
        Return listeners.ToArray()
    End Function

End Class
