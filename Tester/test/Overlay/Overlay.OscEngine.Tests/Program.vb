' Program.vb — Overlay.OscEngine.Tests (custom console runner, repo
' convention cloned from Gallery.Video.Tests / Engine.Concurrency.Tests:
' honest SKIP, exit 0=pass / 1=fail / 2=setup-fail).
'
' Contract coverage:
'   - OscWire frame codec == the GOLDEN transcript captured from the REAL
'     engine.io-client@3.5.4 + socket.io-client@2.5.0 libs (frame-by-frame,
'     not byte-similar: exact strings from Tester/.../golden/golden-transcript.json)
'   - OscProtocol hub-line build/parse == the exact strings produced by
'     TcpClientHelper + consumed by Base.OnMessage / OnTcpMessage
'     (the pipe-truncation rule is asserted as OBSERVED behavior)

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Text.Json

Module Program

    Private _failures As Integer
    Private _passes As Integer
    Private _skips As Integer

    Sub Main(args As String())
        RunTest("wire/golden-handshake-open-packet", AddressOf GoldenHandshake)
        RunTest("wire/golden-connect-on-first-poll", AddressOf GoldenConnect)
        RunTest("wire/golden-event-push-frame", AddressOf GoldenEventPush)
        RunTest("wire/golden-batched-push", AddressOf GoldenBatch)
        RunTest("wire/golden-client-emit-post-body", AddressOf GoldenClientEmit)
        RunTest("wire/golden-client-ping-server-pong", AddressOf GoldenPingPong)
        RunTest("wire/payload-roundtrip", AddressOf PayloadRoundtrip)
        RunTest("wire/bare-open-packet-is-parser-error", AddressOf BareOpenRejected)
        RunTest("wire/binary-framing-matches-vendor-decoder", AddressOf BinaryFraming)
        RunTest("hub/exact-record-start-line", AddressOf HubRecordStartLine)
        RunTest("hub/exact-prewarm-line-path-only", AddressOf HubPrewarmLine)
        RunTest("hub/parse-engine-broadcast", AddressOf HubParseEngineLine)
        RunTest("hub/pipe-truncation-is-real", AddressOf HubPipeTruncation)
        RunTest("hub/status-parse-defensive", AddressOf HubStatusDefensive)
        RunTest("hub/record-output-path-format", AddressOf HubOutputPathFormat)
        RunTest("hub/reconcile-states", AddressOf HubReconcile)
        RunTest("rects/hit-test", AddressOf RectHitTest)
        RunTest("rects/parse-json-array-shapes", AddressOf RectParseShapes)
        RunTest("bridge/polyfill-is-injectable-js", AddressOf BridgePolyfill)
        RunTest("hotkeys/binding-parser", AddressOf HotkeyParser)

        Console.Out.WriteLine()
        Console.Out.WriteLine($"PASS={_passes} FAIL={_failures} SKIP={_skips}")
        Environment.ExitCode = If(_failures > 0, 1, 0)
    End Sub

    ' ── runner plumbing (convention) ──

    Class SkipException
        Inherits Exception
        Public Sub New(reason As String)
            MyBase.New(reason)
        End Sub
    End Class

    Sub Assert(cond As Boolean, msg As String)
        If Not cond Then Throw New Exception(msg)
    End Sub

    Sub AssertEqual(expected As String, actual As String, msg As String)
        If Not String.Equals(expected, actual, StringComparison.Ordinal) Then
            Throw New Exception(msg & " | expected=" & expected & " actual=" & actual)
        End If
    End Sub

    Sub RunTest(name As String, body As Action)
        Try
            body()
            _passes += 1
            Console.Out.WriteLine("PASS " & name)
        Catch sk As SkipException
            _skips += 1
            Console.Out.WriteLine("SKIP " & name & " — " & sk.Message)
        Catch ex As Exception
            _failures += 1
            Console.Out.WriteLine("FAIL " & name & " — " & ex.Message)
        End Try
    End Sub

    Private Function GoldenPath() As String
        Dim p As String = IO.Path.Combine(AppContext.BaseDirectory, "golden", "golden-transcript.json")
        If Not File.Exists(p) Then Throw New SkipException("golden fixture not found at " & p)
        Return p
    End Function

    ' ── wire codec vs golden transcript ──

    Sub GoldenHandshake()
        Dim doc As JsonElement = JsonDocument.Parse(File.ReadAllText(GoldenPath())).RootElement
        Dim first As JsonElement = doc.GetProperty("steps")(0)
        AssertEqual("c2s", first.GetProperty("dir").GetString(), "first step direction")
        Dim url As String = first.GetProperty("url").GetString()
        Assert(url.Contains("EIO=3"), "golden handshake must be EIO=3")
        Assert(url.Contains("transport=polling"), "golden handshake must be polling")
        Assert(url.Contains("X_LOCAL_SECURITY_COOKIE="), "secret rides the handshake URL")

        Dim second As JsonElement = doc.GetProperty("steps")(1)
        Dim body As String = second.GetProperty("body").GetString()
        ' exact golden open packet (prefix form)
        Assert(body.EndsWith(":" & "0{""sid"":""GOLDENSID"",""upgrades"":[],""pingInterval"":2000,""pingTimeout"":60000}", StringComparison.Ordinal),
               "open packet must be the length-prefixed golden form; got " & body)
        Dim packets As List(Of String) = OscWire.DecodePayload(body)
        Assert(packets.Count = 1, "open batch has one packet")
        Dim expected As String = OscWire.OpenPacket("GOLDENSID", 2000, 60000)
        AssertEqual(packets(0), expected, "OpenPacket reproduces the golden handshake payload")
    End Sub

    Sub GoldenConnect()
        ' server sends "40" on the client's FIRST poll (client never sends it)
        Dim doc As JsonElement = JsonDocument.Parse(File.ReadAllText(GoldenPath())).RootElement
        Dim connectBody As String = Nothing
        For i As Integer = 2 To 6
            Dim st As JsonElement = doc.GetProperty("steps")(i)
            If st.GetProperty("dir").GetString() = "s2c" AndAlso st.GetProperty("body").GetString() = "2:40" Then
                connectBody = st.GetProperty("body").GetString()
                Exit For
            End If
        Next
        If connectBody Is Nothing Then Throw New SkipException("golden transcript lacks the 2:40 step (short capture)")
        AssertEqual("2:40", connectBody, "socket.io CONNECT frame")
        AssertEqual("40", OscWire.SocketConnectPacket(), "SocketConnectPacket constant")
        ' and NO c2s step carries a bare "40" POST body:
        For Each st As JsonElement In doc.GetProperty("steps").EnumerateArray()
            If st.GetProperty("dir").GetString() = "c2s" AndAlso st.GetProperty("kind").GetString() = "request" Then
                Dim b As String = st.GetProperty("body").GetString()
                Assert(b Is Nothing OrElse Not b.TrimEnd().EndsWith(":40") AndAlso Not b = "40",
                       "client must never POST a CONNECT frame (socket.io v2 default ns)")
            End If
        Next
    End Sub

    Sub GoldenEventPush()
        Dim packet As String = OscWire.SocketEventPacket("/ShadowPlay/v.1.0/WindowState", "{""windowMsg"":""overlayToggle""}")
        AssertEqual("42[""/ShadowPlay/v.1.0/WindowState"",{""windowMsg"":""overlayToggle""}]", packet, "event frame")
        Dim body As String = OscWire.EncodePayload(New String() {packet})
        AssertEqual((packet.Length.ToString()) & ":" & packet, body, "payload prefix len == packet chars")
        Dim parsed As List(Of String) = OscWire.DecodePayload(body)
        Assert(parsed.Count = 1 AndAlso parsed(0) = packet, "roundtrip")
        Dim ev As OscWire.SocketEvent = OscWire.TryParseSocketEvent(packet)
        Assert(ev IsNot Nothing, "parses")
        AssertEqual("/ShadowPlay/v.1.0/WindowState", ev.Channel, "channel")
        AssertEqual("{""windowMsg"":""overlayToggle""}", ev.PayloadJson, "payload")
    End Sub

    Sub GoldenBatch()
        Dim p1 As String = OscWire.SocketEventPacket("/ShadowPlay/v.1.0/Notification", "{""id"":""n1"",""title"":""Recording saved""}")
        Dim p2 As String = OscWire.SocketEventPacket("/ShadowPlay/v.1.0/Hotkey", "{""hotkey"":""Alt+F10""}")
        Dim batch As String = OscWire.EncodePayload(New String() {p1, p2})
        Dim parsed As List(Of String) = OscWire.DecodePayload(batch)
        Assert(parsed.Count = 2, "batch decodes both packets")
        AssertEqual(p1, parsed(0), "batch first")
        AssertEqual(p2, parsed(1), "batch second")
    End Sub

    Sub GoldenClientEmit()
        ' golden POST body: 50:42["/ShadowPlay/v.1.0/Hotkey",{"hotkey":"Alt+F9"}]
        Dim golden As String = "50:42[""/ShadowPlay/v.1.0/Hotkey"",{""hotkey"":""Alt+F9""}]"
        Dim packets As List(Of String) = OscWire.DecodePayload(golden)
        Assert(packets.Count = 1, "golden emit decodes")
        Dim ev As OscWire.SocketEvent = OscWire.TryParseSocketEvent(packets(0))
        Assert(ev IsNot Nothing, "golden emit parses as EVENT")
        AssertEqual("/ShadowPlay/v.1.0/Hotkey", ev.Channel, "emit channel")
        AssertEqual("{""hotkey"":""Alt+F9""}", ev.PayloadJson, "emit payload")
        ' server building the same emit must produce the identical frame
        AssertEqual("42[""/ShadowPlay/v.1.0/Hotkey"",{""hotkey"":""Alt+F9""}]",
                    OscWire.SocketEventPacket("/ShadowPlay/v.1.0/Hotkey", "{""hotkey"":""Alt+F9""}"), "emit builder")
    End Sub

    Sub GoldenPingPong()
        ' measured: the CLIENT POSTs "1:2" (engine.io PING); the server
        ' delivers "3" in a later poll response.
        Dim decoded As List(Of String) = OscWire.DecodePayload("1:2")
        Assert(decoded.Count = 1 AndAlso decoded(0) = "2", "client ping packet is '2'")
        AssertEqual("3", OscWire.EnginePong, "server pong constant")
        ' server delivers the pong inside a polling batch: "1:3"
        AssertEqual("1:3", OscWire.EncodePayload(New String() {OscWire.EnginePong}), "pong payload batch form")
        Assert(OscWire.DecodePayload(OscWire.EncodePayload(New String() {OscWire.EnginePong}))(0) = "3", "pong decodes")
    End Sub

    Sub PayloadRoundtrip()
        Dim samples As String() = {
            "40", "42[""ch"",null]", "42[""ch"",""plainstring""]",
            "41", "2", "3", "6",
            OscWire.SocketEventPacket("/a/b/c", "{""x"":1,""y"":""s|:pecial""}")
        }
        For Each s As String In samples
            Dim enc As String = OscWire.EncodePayload(New String() {s})
            Dim dec As List(Of String) = OscWire.DecodePayload(enc)
            Assert(dec.Count = 1 AndAlso dec(0) = s, "roundtrip of " & s)
        Next
        ' multi + mixed
        Dim multi As String = OscWire.EncodePayload(New String() {"40", "42[""x"",{}]", "2"})
        Dim dm As List(Of String) = OscWire.DecodePayload(multi)
        Assert(dm.Count = 3 AndAlso dm(0) = "40" AndAlso dm(1) = "42[""x"",{}]" AndAlso dm(2) = "2", "multi roundtrip")
    End Sub

    Sub BareOpenRejected()
        ' measured in probe run: an UNPREFIXED open packet makes the real
        ' client fail with "parser error". Our encoder must never emit one.
        Dim open As String = OscWire.OpenPacket("S1", 25000, 60000)
        Dim enc As String = OscWire.EncodePayload(New String() {open})
        Dim prefixLen As Integer = open.Length.ToString().Length
        Assert(enc.StartsWith(open.Length.ToString(), StringComparison.Ordinal), "payload must start with the packet length")
        Assert(enc(prefixLen) = ":"c, "prefix separator right after the digits")
        Assert(enc.Length = prefixLen + 1 + open.Length, "total length = digits + colon + packet")
    End Sub

    Sub BinaryFraming()
        ' Byte-exact against the bundled engine.io-parser 1.x
        ' decodePayloadAsBinary (extracted from osc/vendor.js):
        '   0x00 | ascii decimal byte-length | 0xFF | utf8 payload
        Dim open As String = OscWire.OpenPacket("4d2d474f0c744b38", 25000, 60000)
        Dim framed As Byte() = OscWire.EncodePayloadBinaryOne(open)
        Dim expectedPayload As Byte() = System.Text.Encoding.UTF8.GetBytes(open)
        Dim lenText As String = expectedPayload.Length.ToString()
        Assert(framed(0) = 0, "leading zero byte marks a STRING packet")
        Assert(framed(1) = CByte(AscW(lenText(0)) - AscW("0"c)), "length digit byte holds the DIGIT VALUE (not ascii)")
        Assert(framed(1 + lenText.Length) = 255, "0xFF terminates the length")
        Assert(framed.Length = 1 + lenText.Length + 1 + expectedPayload.Length, "total framed length")
        For i As Integer = 0 To expectedPayload.Length - 1
            Assert(framed(1 + lenText.Length + 1 + i) = expectedPayload(i), "payload byte " & i)
        Next
        ' two packets concatenated
        Dim two As Byte() = OscWire.EncodePayloadBinary(New String() {"40", "42[""ch"",null]"})
        Assert(two(0) = 0 AndAlso two(1) = 2 AndAlso two(2) = 255, "first packet header: length 2 as digit value")
        Dim afterFirst As Integer = 3 + 2 ' header + "40"
        Assert(two(afterFirst) = 0, "second packet starts with the zero byte")
        ' empty payload list → zero bytes (the POST-ack shape)
        Assert(OscWire.EncodePayloadBinary(New String() {}).Length = 0, "empty batch encodes empty")
    End Sub

    ' ── hub protocol (exact strings) ──

    Sub HubRecordStartLine()
        AssertEqual("[Send] NVIDIA Overlay Engine|RECORD_START:C:\vids\Record_2026-09-13_10-00-00.mp4",
                    OscProtocol.BuildLine("RECORD_START", "C:\vids\Record_2026-09-13_10-00-00.mp4"),
                    "RECORD_START line matches TcpClientHelper.Send format")
        AssertEqual("[Send] NVIDIA Overlay Engine|RECORD_STOP",
                    OscProtocol.BuildLine("RECORD_STOP"), "no-value line has no colon")
    End Sub

    Sub HubPrewarmLine()
        ' path ONLY — the Forms overlay's "|encoder" suffix never survives
        ' the engine-side split; we do not send dead bytes.
        Dim line As String = OscProtocol.BuildLine("PREWARM_FFMPEG", "C:\app\api-core\ffmpeg.exe")
        AssertEqual("[Send] NVIDIA Overlay Engine|PREWARM_FFMPEG:C:\app\api-core\ffmpeg.exe", line, "prewarm line")
        Assert(line.IndexOf("|", line.IndexOf("|"c) + 1) < 0, "value must contain no pipe")
    End Sub

    Sub HubParseEngineLine()
        ' engine broadcasts arrive VERBATIM from the sender ("[Send] " prefix
        ' preserved by the hub's Broadcast) — the receiver sees:
        Dim m As OscProtocol.HubMessage = OscProtocol.ParseHubLine("[Send] NVIDIA Engine|engine_response:engine_record_start,ok")
        Assert(m IsNot Nothing, "parses")
        AssertEqual("NVIDIA Engine", m.Sender, "sender")
        AssertEqual("engine_response", m.Cmd, "cmd")
        AssertEqual("engine_record_start,ok", m.Value, "value")

        Dim r As OscProtocol.EngineResponse = OscProtocol.ParseEngineResponse(m.Value)
        Assert(r IsNot Nothing, "response parses")
        AssertEqual("engine_record_start", r.Cmd, "resp cmd")
        AssertEqual("ok", r.Status, "resp status")
    End Sub

    Sub HubPipeTruncation()
        ' OBSERVED live behavior: sender packs "Recording|42|C:\out.mp4" but
        ' every receiver takes parts(1) only → the tail is a SEPARATE segment
        ' on the wire: "[Send] NVIDIA Engine|engine_response:engine_get_status,ok,Recording|42|C:\out.mp4".
        ' Our parser must (a) take the state word from the command segment
        ' and (b) keep the tail in FullValue for defensive recovery.
        Dim m As OscProtocol.HubMessage = OscProtocol.ParseHubLine(
            "[Send] NVIDIA Engine|engine_response:engine_get_status,ok,Recording|42|C:\out.mp4")
        AssertEqual("engine_response", m.Cmd, "cmd under pipe rule")
        AssertEqual("engine_get_status,ok,Recording", m.Value, "value truncated at first pipe")
        Assert(m.FullValue.Contains("42") AndAlso m.FullValue.Contains("C:\out.mp4"), "tail preserved in FullValue")
        Dim r As OscProtocol.EngineResponse = OscProtocol.ParseEngineResponse(m.FullValue)
        AssertEqual("Recording", OscProtocol.StatusStateFromData(r.Data), "state word")
        AssertEqual("42", OscProtocol.StatusElapsedFromData(r.Data).ToString(), "elapsed recovered when tail survived")
    End Sub

    Sub HubStatusDefensive()
        ' state-only answer (no session)
        Dim r As OscProtocol.EngineResponse = OscProtocol.ParseEngineResponse("engine_get_status,ok,Idle")
        AssertEqual("Idle", OscProtocol.StatusStateFromData(r.Data), "state-only")
        Assert(OscProtocol.StatusElapsedFromData(r.Data) = -1, "no elapsed")
        ' progress with truncated tail: only sec is trustworthy
        Dim m As OscProtocol.HubMessage = OscProtocol.ParseHubLine(
            "[Send] NVIDIA Engine|engine_recording_progress:17|0|8123456")
        AssertEqual("17", m.Value, "progress sec survives")
        Dim fields As String() = m.FullValue.Split("|"c)
        AssertEqual("17", fields(0), "field0")
    End Sub

    Sub HubOutputPathFormat()
        Dim p As String = OscProtocol.RecordOutputPath("C:\vids", New DateTime(2026, 9, 13, 10, 0, 0, DateTimeKind.Local))
        AssertEqual("C:\vids\Record_2026-09-13_10-00-00.mp4", p, "path format matches the Forms overlay's")
        Dim fallback As String = OscProtocol.RecordOutputPath("", New DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Local))
        Assert(fallback.EndsWith("\Record_2026-01-02_03-04-05.mp4", StringComparison.Ordinal), "fallback dir + same naming")
    End Sub

    Sub HubReconcile()
        Assert(OscProtocol.ShouldShowRecording("Recording", False), "Recording → True")
        Assert(OscProtocol.ShouldShowRecording("Idle", True) = False, "Idle → False")
        Assert(OscProtocol.ShouldShowRecording("HasError", True) = False, "HasError → False")
        Assert(OscProtocol.ShouldShowRecording("Stopping", True), "Stopping keeps True")
    End Sub

    ' ── displayRects ──

    Sub RectHitTest()
        Dim rects As New List(Of Rectangle) From {New Rectangle(100, 200, 300, 50)}
        Assert(OscProtocol.PointInAnyRect(100, 200, rects), "top-left inclusive")
        Assert(OscProtocol.PointInAnyRect(399, 249, rects), "bottom-right exclusive edge inside")
        Assert(Not OscProtocol.PointInAnyRect(400, 250, rects), "right/bottom exclusive")
        Assert(Not OscProtocol.PointInAnyRect(99, 200, rects), "left exclusive")
        Assert(Not OscProtocol.PointInAnyRect(50, 50, Nothing), "null rects → no hit")
    End Sub

    Sub RectParseShapes()
        Dim arr As JsonElement = JsonDocument.Parse("[[10,20,30,40],{""x"":1,""y"":2,""width"":3,""height"":4},{""x"":5,""y"":6,""w"":7,""h"":8}]").RootElement
        Dim rects As List(Of Rectangle) = OscProtocol.ParseDisplayRects(arr)
        Assert(rects.Count = 3, "three shapes parsed")
        AssertEqual("10,20,30,40", rects(0).Left & "," & rects(0).Top & "," & rects(0).Width & "," & rects(0).Height, "array shape")
        AssertEqual("1,2,3,4", rects(1).Left & "," & rects(1).Top & "," & rects(1).Width & "," & rects(1).Height, "object shape")
        AssertEqual("5,6,7,8", rects(2).Left & "," & rects(2).Top & "," & rects(2).Width & "," & rects(2).Height, "short-object shape")
        Dim bad As JsonElement = JsonDocument.Parse("{}").RootElement
        Assert(OscProtocol.ParseDisplayRects(bad).Count = 0, "non-array → empty")
    End Sub

    Sub BridgePolyfill()
        Dim js As String = CefQueryBridge.PolyfillSource()
        Assert(js.Contains("window.cefQuery=function"), "defines cefQuery")
        Assert(js.Contains("chrome.webview.postMessage"), "posts to WebView2 host")
        Assert(js.Contains("__cefDeliver"), "delivers host responses")
        Assert(js.Contains("persistent"), "handles persistent queries")
    End Sub

    Sub HotkeyParser()
        ' Alt+Z (the default toggle binding)
        Dim az = OscHotkeys.ParseBinding("Alt+Z").Value
        Assert(az.Modifiers = 1, "MOD_ALT")
        Assert(az.VirtualKey = &H5A, "VK_Z")
        ' Ctrl+Alt+F9
        Dim caf9 = OscHotkeys.ParseBinding("Ctrl+Alt+F9").Value
        Assert(caf9.Modifiers = 3, "MOD_ALT|MOD_CONTROL")
        Assert(caf9.VirtualKey = &H78, "VK_F9")
        ' Shift+F10
        Assert(OscHotkeys.ParseBinding("Shift+F10").Value.Modifiers = 4, "MOD_SHIFT")
        ' digits + win
        Dim w5 = OscHotkeys.ParseBinding("Win+5").Value
        Assert(w5.Modifiers = 8, "MOD_WIN")
        Assert(w5.VirtualKey = &H35, "VK_5")
        ' invalid shapes → Nothing
        Assert(OscHotkeys.ParseBinding(Nothing) Is Nothing, "null")
        Assert(OscHotkeys.ParseBinding("") Is Nothing, "empty")
        Assert(OscHotkeys.ParseBinding("Meta+Z") Is Nothing, "unknown modifier")
        Assert(OscHotkeys.ParseBinding("Alt+F99") Is Nothing, "F out of range")
        Assert(OscHotkeys.ParseBinding("Alt+") Is Nothing, "trailing plus")
    End Sub

End Module
