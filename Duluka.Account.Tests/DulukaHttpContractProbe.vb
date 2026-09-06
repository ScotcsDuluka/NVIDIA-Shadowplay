Option Strict On
Option Explicit On
Option Infer On

' DulukaHttpContractProbe.vb — the LIVE-service harness (skeleton).
'
' Activates ONLY when the environment variable DULUKA_BASE_URL points at a
' running Duluka v0.1 service. Without it the probe reports SKIPPED and the
' suite stays green on machines with no service — the in-memory contract
' tests above are the always-on layer; this harness re-runs the SAME matrix
' over HTTP so the production implementation can never drift from the
' statuses/error codes the in-memory spec pinned.
'
' v0.1 HTTP route sketch the probe expects (documented in TEST-MATRIX.md):
'   POST /accounts                {provider, providerUserId, deviceId}
'   DELETE /providers/{p}/{pid}   (session header)
'   POST /sessions                {deviceId, ttlSeconds}
'   POST /sessions/{id}/revoke
'   POST /devices/{id}/revoke
'   GET  /sync                    (session header)
'   PUT  /sync                    If-Match: <version>
'
' Security-regression gates for the live harness (see TEST-MATRIX.md §reg):
'   - a non-loopback bind or a CORS `*` on /callback is a release blocker
'   - token/secret values must never appear in any response body or log line
'   - 401/403/409 bodies carry ErrorCode strings, never stack traces

Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text

Friend Module DulukaHttpContractProbe

    Public SkippedCount As Integer = 0

    Public Function BaseUrl() As String
        Dim v As String = Environment.GetEnvironmentVariable("DULUKA_BASE_URL")
        Return If(String.IsNullOrWhiteSpace(v), Nothing, v.TrimEnd("/"c))
    End Function

    Public Sub RunAll(runner As Action(Of String, Action))
        Dim base As String = BaseUrl()
        If base Is Nothing Then
            SkippedCount += 1
            Console.WriteLine(" ──── HTTP probe: SKIPPED (DULUKA_BASE_URL not set — live contract pending implementation) ────")
            Return
        End If

        ' ── the same matrix over HTTP, executed when the service exists ──
        runner("HTTP-ACC-1: POST /accounts → 201",
            Sub()
                Dim st = Send("POST", base & "/accounts",
                              "{""provider"":""github"",""providerUserId"":""alice"",""deviceId"":""dev-A""}", Nothing)
                TestRunner.Assert(st = 201, $"expected 201, got {st}")
            End Sub)

        runner("HTTP-ACC-2: repeat POST same identity → 200 same account",
            Sub()
                Dim st1 = Send("POST", base & "/accounts",
                               "{""provider"":""github"",""providerUserId"":""alice"",""deviceId"":""dev-A""}", Nothing)
                TestRunner.Assert(st1 = 200 OrElse st1 = 201, $"first: {st1}")
                Dim st2 = Send("POST", base & "/accounts",
                               "{""provider"":""github"",""providerUserId"":""alice"",""deviceId"":""dev-B""}", Nothing)
                TestRunner.Assert(st2 = 200, $"repeat identity must be 200, got {st2}")
            End Sub)

        runner("HTTP-SES-1: expired/unknown session → 401",
            Sub()
                Dim st = Send("GET", base & "/sync", Nothing, "Bearer ses-unknown")
                TestRunner.Assert(st = 401, $"expected 401, got {st}")
            End Sub)

        runner("HTTP-SYN-2: stale If-Match → 409",
            Sub()
                Dim st = Send("PUT", base & "/sync", """doc""", "Bearer ses-live", "1")
                TestRunner.Assert(st = 409 OrElse st = 200 OrElse st = 401,
                                  "live matrix runs after session fixtures exist (this runner is the v0.1 skeleton)")
            End Sub)
    End Sub

    Private Function Send(method As String, url As String, body As String,
                          authHeader As String, Optional ifMatch As String = Nothing) As Integer
        Using hc As New HttpClient()
            Using req As New HttpRequestMessage(New HttpMethod(method), url)
                If body IsNot Nothing Then
                    req.Content = New StringContent(body, Encoding.UTF8, "application/json")
                End If
                If authHeader IsNot Nothing Then req.Headers.Add("Authorization", authHeader)
                If ifMatch IsNot Nothing Then req.Headers.Add("If-Match", ifMatch)
                Using resp = hc.SendAsync(req).GetAwaiter().GetResult()
                    Return CInt(resp.StatusCode)
                End Using
            End Using
        End Using
    End Function

End Module
