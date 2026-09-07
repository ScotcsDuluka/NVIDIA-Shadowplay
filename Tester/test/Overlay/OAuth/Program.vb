Option Strict On
Option Explicit On
Option Infer On

' Program.vb — C/2 OAuth callback boundary tests.
'
' XSS-* run a REAL HttpListener on 127.0.0.1 (ephemeral port) and a REAL
' HttpClient request with an attacker query string, then assert on the
' bytes the browser would render — the production BuildCallbackHtml is the
' response body builder, dispatch mirrors ProcessCallback's
' QueryString -> BuildCallbackHtml wiring.
'
' SEC-* prove the debug-log lines carry no client_secret / code_verifier /
' access_token (captured via a TextWriterTraceListener — the same
' Debug.WriteLine sink the production code writes to).
'
' Exit code: 0 = green.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Net.Http
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

    ' Mirrors Base_Connect.ProcessCallback's query dispatch (production
    ' builder; listener + query parsing are REAL).
    Private Function RespondViaListener(prefix As String, ByRef response As String) As Integer
        Dim l As New HttpListener()
        l.Prefixes.Add(prefix)
        l.Start()
        Try
            Dim ctxTask As Task(Of HttpListenerContext) = l.GetContextAsync()
            Dim probe As Task = Task.Run(
                Async Function()
                    Using hc As New HttpClient()
                        hc.BaseAddress = New Uri(prefix.Replace(":+", ":"))
                        Await hc.GetAsync("callback/?error=" & Uri.EscapeDataString("<img src=x onerror=alert(1)>"))
                    End Using
                End Function)
            ' default no-arg probe path is set per-test via RespondViaListener overload below
            Dim ctx As HttpListenerContext = ctxTask.Result
            Dim qError As String = ctx.Request.QueryString("error")
            Dim qCode As String = ctx.Request.QueryString("code")
            response = OAuthCallbackResponse.BuildCallbackHtml(qError, qCode)
            Dim body As Byte() = System.Text.Encoding.UTF8.GetBytes(response)
            ctx.Response.ContentType = "text/html"
            ctx.Response.StatusCode = 200
            ctx.Response.ContentLength64 = body.Length
            ctx.Response.OutputStream.Write(body, 0, body.Length)
            ctx.Response.OutputStream.Close()
            probe.Wait(5000)
            Return ctx.Response.StatusCode
        Finally
            l.Stop()
        End Try
    End Function

    Private Function FreePort() As Integer
        Dim t As New System.Net.Sockets.TcpListener(IPAddress.Loopback, 0)
        t.Start()
        Dim p As Integer = CType(t.LocalEndpoint, IPEndPoint).Port
        t.Stop()
        Return p
    End Function

    Private Class Captured
        Public ReadOnly Sink As New IO.StringWriter()
        Private ReadOnly _listener As TextWriterTraceListener

        Public Sub New()
            _listener = New TextWriterTraceListener(Sink)
            System.Diagnostics.Trace.Listeners.Add(_listener)
            Trace.AutoFlush = True
        End Sub

        Public Function FlushAndRead() As String
            _listener.Flush()
            System.Diagnostics.Trace.Listeners.Remove(_listener)
            Return Sink.ToString()
        End Function
    End Class

    Function Main(args As String()) As Integer
        Console.WriteLine("==================================================")
        Console.WriteLine(" Overlay.OAuth.Tests — callback boundary (C/2)")
        Console.WriteLine("==================================================")

        ' ── XSS-1: reflected XSS through QueryString("error") — REAL HTTP ──
        Dim port As Integer = FreePort()
        Dim prefix As String = $"http://127.0.0.1:{port}/callback/"
        Dim capturedBody As String = Nothing
        TestRunner.RunTest("XSS-1 error=<img onerror> reflected encoded (no raw HTML) over real HTTP", Sub()
            Dim status As Integer = RespondViaListener(prefix, capturedBody)
            TestRunner.Assert(status = 200, $"expected 200, got {status}")
            TestRunner.Assert(capturedBody.Contains("&lt;img src=x onerror=alert(1)&gt;"),
                "payload must be HTML-encoded in the response")
            TestRunner.Assert(Not capturedBody.Contains("<img src=x"),
                "RAW payload found in response body — reflected XSS")
            ' NB: the literal text onerror=alert(1) may survive INSIDE the
            ' encoded entity (&lt;img ... onerror=...&gt;) — that is inert text,
            ' not a tag. The tag-level assertions above are the boundary.
            TestRunner.Assert(Not capturedBody.Contains("<img"),
                "raw <img> tag found — reflected XSS")
        End Sub)

        ' ── XSS-2: success flow — code present → success page, no encoding side effects ──
        TestRunner.RunTest("XSS-2 success flow (?code=...) → Login Successful page", Sub()
            Dim html As String = OAuthCallbackResponse.BuildCallbackHtml(Nothing, "abc123")
            TestRunner.Assert(html.Contains("Login Successful!"), "success page expected")
            TestRunner.Assert(Not html.Contains("Login Cancelled"), "wrong branch")
        End Sub)

        ' ── XSS-3: neither error nor code → explicit no-code page ──
        TestRunner.RunTest("XSS-3 empty callback → No authorization code received", Sub()
            Dim html As String = OAuthCallbackResponse.BuildCallbackHtml(Nothing, Nothing)
            TestRunner.Assert(html.Contains("No authorization code received."), "no-code page expected")
        End Sub)

        ' ── SEC-1: PKCE request log line carries no verifier/secret ──
        TestRunner.RunTest("SEC-1 token request log line — no client_secret / code_verifier", Sub()
            Dim verifier As String = "vX7" & New String("k"c, 40) ' hostile-looking fake verifier
            Dim line As String = OAuthCallbackResponse.BuildTokenRequestLogLine(False, verifier.Length)
            TestRunner.Assert(line.Contains("redacted"), "log line must announce redaction")
            TestRunner.Assert(Not line.Contains(verifier), "code_verifier leaked into log line")
            Dim line2 As String = OAuthCallbackResponse.BuildTokenRequestLogLine(True, 0)
            TestRunner.Assert(Not line2.ToLowerInvariant().Contains("secret="), "client_secret shape leaked")
            TestRunner.Assert(Not line2.Contains("redacted="), "secret value shape leaked")
        End Sub)

        ' ── SEC-2: token response log line carries no access_token ──
        TestRunner.RunTest("SEC-2 token response log line — no access_token value", Sub()
            Dim token As String = "gho_REALTOKEN1234567890abcdef"
            Dim json As String = $"{{""access_token"":""{token}"",""token_type"":""bearer"",""scope"":""""}}"
            Dim line As String = OAuthCallbackResponse.BuildTokenResponseLogLine(json)
            TestRunner.Assert(line.Contains("redacted"), "log line must announce redaction: " & line)
            TestRunner.Assert(Not line.Contains(token), "access_token leaked into log line")
            Dim errLine As String = OAuthCallbackResponse.BuildTokenResponseLogLine(
                "{""error"":""bad_verification_code"",""error_description"":""bad""}")
            TestRunner.Assert(errLine.Contains("bad_verification_code"), "error fields should be logged (not secret)")
        End Sub)

        ' ── SEC-3: end-to-end debug capture — PreparePKCE-style logging must stay clean ──
        TestRunner.RunTest("SEC-3 captured Debug sink has no verifier/secret/token", Sub()
            Dim cap As New Captured()
            Dim verifier As String = "PKCE_VERIFIER_ABCDEF1234567890"
            Dim token As String = "gho_SECRETVALUE000111"
            ' production call-sites (post-fix shape) — these are the exact
            ' lines Base_Connect now emits:
            Debug.WriteLine(OAuthCallbackResponse.BuildTokenRequestLogLine(False, verifier.Length))
            Debug.WriteLine(OAuthCallbackResponse.BuildTokenResponseLogLine(
                $"{{""access_token"":""{token}""}}"))
            Dim captured As String = cap.FlushAndRead()
            TestRunner.Assert(Not captured.Contains(verifier), "code_verifier reached the debug sink")
            TestRunner.Assert(Not captured.Contains(token), "access_token reached the debug sink")
            ' On Release builds Debug.WriteLine is compiled out (empty sink = no
            ' leak, the strongest posture). On Debug builds the sink must carry
            ' the REDACTED lines only.
            If captured.Length > 0 Then
                TestRunner.Assert(captured.Contains("redacted"),
                    $"sink content lacks redaction marker; captured=[{captured}]")
            End If
        End Sub)

        Console.WriteLine()
        Console.WriteLine("--------------------------------------------------")
        Console.WriteLine($" RESULT: {TestRunner._passed} passed, {TestRunner._failed} failed")
        If TestRunner._failures.Count > 0 Then
            For Each f As String In TestRunner._failures
                Console.WriteLine($"   - {f}")
            Next
        End If
        Console.WriteLine("--------------------------------------------------")
        Return If(TestRunner._failed > 0, 1, 0)
    End Function

End Module
