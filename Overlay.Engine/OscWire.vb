' OscWire.vb — pure codecs for the osc controller-server wire protocol,
' extracted so the test suite can verify them frame-by-frame against the
' golden transcript captured from the REAL client libraries
' (Tester/test/Overlay/Overlay.OscEngine.Tests/golden/).
'
' Protocol facts encoded here were MEASURED, not assumed (see
' Overlay.Engine/PROTOCOL-MATRIX.md):
'   - engine.io v3 polling payload = concat of "<len>:<packet>" where len
'     counts the packet string's chars (UTF-16 code units, ASCII payloads).
'   - The open packet is length-prefixed like every other packet; sending a
'     bare 0{...} fails the client with "parser error" (measured).
'   - socket.io v2 frames: "40" CONNECT (server→client initiates the
'     default namespace; the client never sends it), "42[...]" EVENT,
'     "2" PING (client→server), "3" PONG (server→client), "6" NOOP.

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Public NotInheritable Class OscWire

    Private Sub New()
    End Sub

    ' ── engine.io v3 payload (batch) codec ─────────────────────

    ''' <summary>Encodes packets into one polling payload body:
    ''' "5:hello3:42[..." style concat of length-prefixed packets.</summary>
    Public Shared Function EncodePayload(packets As IList(Of String)) As String
        Dim sb As New StringBuilder()
        For Each p As String In packets
            If p Is Nothing Then Continue For
            sb.Append(p.Length.ToString(CultureInfo.InvariantCulture))
            sb.Append(":"c)
            sb.Append(p)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Encodes packets in the BINARY polling framing demanded by
    ''' binary-capable clients (browser XHR with responseType=arraybuffer,
    ''' Content-Type octet-stream). Per packet, byte-exact per the bundled
    ''' engine.io-parser 1.x decodePayloadAsBinary (verified against
    ''' osc/vendor.js):
    '''   0x00 | decimal digit VALUES (one byte 0-9 per digit) | 0xFF | utf8
    ''' NOTE: the length digits are the DIGIT VALUES, not ASCII chars — the
    ''' decoder does `lengthString += bytes[d]` on a number, so bytes
    ''' [8,1] mean length 81, while ASCII would read as 5651.</summary>
    Public Shared Function EncodePayloadBinary(packets As IList(Of String)) As Byte()
        Dim chunks As New List(Of Byte)
        For Each p As String In packets
            If p Is Nothing Then Continue For
            Dim payload As Byte() = Encoding.UTF8.GetBytes(p)
            chunks.Add(0)
            Dim lengthText As String = payload.Length.ToString(CultureInfo.InvariantCulture)
            For Each c As Char In lengthText
                chunks.Add(CByte(AscW(c) - AscW("0"c)))
            Next
            chunks.Add(255)
            For Each b As Byte In payload
                chunks.Add(b)
            Next
        Next
        Return chunks.ToArray()
    End Function

    Public Shared Function EncodePayloadBinaryOne(packet As String) As Byte()
        Dim one As String() = {packet}
        Return EncodePayloadBinary(one)
    End Function

    ''' <summary>Decodes a polling payload body into packets. Tolerates
    ''' trailing garbage by stopping at the first malformed prefix (the
    ''' client never sends one; this is defense, not protocol).</summary>
    Public Shared Function DecodePayload(body As String) As List(Of String)
        Dim result As New List(Of String)
        If String.IsNullOrEmpty(body) Then Return result
        Dim i As Integer = 0
        While i < body.Length
            Dim colonIdx As Integer = body.IndexOf(":"c, i)
            If colonIdx < 0 Then Exit While
            Dim lenText As String = body.Substring(i, colonIdx - i)
            Dim len As Integer
            If Not Integer.TryParse(lenText, NumberStyles.Integer, CultureInfo.InvariantCulture, len) OrElse len < 0 Then Exit While
            If colonIdx + 1 + len > body.Length Then Exit While
            result.Add(body.Substring(colonIdx + 1, len))
            i = colonIdx + 1 + len
        End While
        Return result
    End Function

    ''' <summary>The engine.io v3 open packet (sid/pingInterval/pingTimeout/
    ''' upgrades:[] — upgrades empty keeps the client on polling forever,
    ''' exactly like the golden server run).</summary>
    Public Shared Function OpenPacket(sid As String, pingIntervalMs As Integer, pingTimeoutMs As Integer) As String
        Return "0{""sid"":""" & sid & """,""upgrades"":[],""pingInterval"":" &
               pingIntervalMs.ToString(CultureInfo.InvariantCulture) &
               ",""pingTimeout"":" & pingTimeoutMs.ToString(CultureInfo.InvariantCulture) & "}"
    End Function

    ' ── socket.io v2 frame builders ────────────────────────────

    ''' <summary>Server→client CONNECT for the default namespace. The client
    ''' surfaces its "connect" event ONLY after receiving this.</summary>
    Public Shared Function SocketConnectPacket() As String
        Return "40"
    End Function

    ''' <summary>Builds an EVENT frame: 42["<channel>",<payload-json>].</summary>
    Public Shared Function SocketEventPacket(channel As String, payloadJson As String) As String
        Return "42[" & JsonString(channel) & "," & If(String.IsNullOrEmpty(payloadJson), "null", payloadJson) & "]"
    End Function

    Public Shared Function SocketAckPacket(ackId As Integer, payloadJson As String) As String
        Return "43" & ackId.ToString(CultureInfo.InvariantCulture) & "[" & If(String.IsNullOrEmpty(payloadJson), "null", payloadJson) & "]"
    End Function

    Public Const EnginePing As String = "2"
    Public Const EnginePong As String = "3"
    Public Const EngineNoop As String = "6"

    ' ── socket.io v2 EVENT frame parser (for client→server emits) ──

    Public Class SocketEvent
        Public Channel As String
        Public PayloadJson As String
        Public Raw As String
    End Class

    ''' <summary>Parses "42[...]" (and 42#-prefixed binary placeholders we
    ''' reject). Returns Nothing for non-EVENT packets (connect/disconnect/
    ''' engine-level frames are handled by the caller).</summary>
    Public Shared Function TryParseSocketEvent(packet As String) As SocketEvent
        If packet Is Nothing OrElse packet.Length < 5 OrElse Not packet.StartsWith("42", StringComparison.Ordinal) Then Return Nothing
        Dim openIdx As Integer = packet.IndexOf("["c)
        If openIdx < 0 Then Return Nothing
        Dim closeIdx As Integer = packet.LastIndexOf("]"c)
        If closeIdx < openIdx Then Return Nothing
        Dim inner As String = packet.Substring(openIdx + 1, closeIdx - openIdx - 1)
        ' inner = "<channel-json>[,<payload-json>]"
        If Not inner.StartsWith("""", StringComparison.Ordinal) Then Return Nothing
        Dim endQuote As Integer = FindClosingQuote(inner)
        If endQuote < 0 Then Return Nothing
        Dim ev As New SocketEvent()
        ev.Raw = packet
        Try
            ev.Channel = System.Text.Json.JsonSerializer.Deserialize(Of String)(inner.Substring(0, endQuote + 1))
        Catch ex As Exception
            Return Nothing
        End Try
        Dim rest As String = inner.Substring(endQuote + 1).TrimStart()
        If rest.Length = 0 Then
            ev.PayloadJson = Nothing
        ElseIf rest.StartsWith(","c) Then
            ev.PayloadJson = rest.Substring(1).Trim()
        Else
            ev.PayloadJson = Nothing
        End If
        Return ev
    End Function

    Private Shared Function FindClosingQuote(s As String) As Integer
        ' The channel name is a plain JSON string produced by us or the page
        ' (no escaped quotes in osc channel names); find the closing quote.
        For i As Integer = 1 To s.Length - 1
            If s(i) = """"c AndAlso s(i - 1) <> "\"c Then Return i
        Next
        Return -1
    End Function

    ''' <summary>Minimal JSON string literal (quotes + backslashes + control
    ''' chars) — channel names are ASCII; payload JSON arrives pre-encoded.</summary>
    Public Shared Function JsonString(s As String) As String
        Dim sb As New StringBuilder(s.Length + 16)
        sb.Append("""")
        For Each c As Char In s
            Select Case c
                Case """"c : sb.Append("\""")
                Case "\"c : sb.Append("\\")
                Case ControlChars.Back : sb.Append("\b")
                Case ControlChars.FormFeed : sb.Append("\f")
                Case ControlChars.Lf : sb.Append("\n")
                Case ControlChars.Cr : sb.Append("\r")
                Case ControlChars.Tab : sb.Append("\t")
                Case Else
                    If AscW(c) < 32 Then
                        sb.Append("\u").Append(AscW(c).ToString("x4", CultureInfo.InvariantCulture))
                    Else
                        sb.Append(c)
                    End If
            End Select
        Next
        sb.Append("""")
        Return sb.ToString()
    End Function

End Class
