' DulukaAuthFlow.vb — one Duluka GitHub OAuth flow (login or provider link).
' Division of trust: the SERVER builds the authorize URL and performs the code
' exchange (the client never sees a client_secret). The client's jobs are:
' host the localhost:8517 redirect listener, hand code+state back, and persist
' the session token it is shown EXACTLY ONCE straight into the DPAPI store.
' Log discipline (C/2): code/state/token presence is logged, never values.

Imports System.Diagnostics
Imports System.Net
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Friend Class DulukaAuthFlow

    Public Enum FlowKind
        Login
        LinkProvider
    End Enum

    Public Class Outcome
        Public Succeeded As Boolean
        Public Cancelled As Boolean
        Public ErrorCode As String = ""
        Public Message As String = ""
    End Class

    ' Single-flight: one OAuth flow at a time across the whole app.
    Private Shared _active As DulukaAuthFlow

    Private ReadOnly _kind As FlowKind
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _finished As Boolean

    Private Sub New(kind As FlowKind)
        _kind = kind
    End Sub

    ''' <summary>Begins a flow; Nothing if one is already running.</summary>
    Public Shared Function TryBegin(kind As FlowKind) As DulukaAuthFlow
        If _active IsNot Nothing Then Return Nothing
        Dim flow As New DulukaAuthFlow(kind)
        _active = flow
        Return flow
    End Function

    Public Function IsCancelled() As Boolean
        Return _cts.Token.IsCancellationRequested
    End Function

    Public Sub Cancel()
        If Not _finished Then
            _cts.Cancel()
        End If
    End Sub

    ''' <summary>
    ''' Runs the whole flow. report() may fire from ANY thread — callers must
    ''' marshal to the UI thread before touching controls.
    ''' </summary>
    Public Async Function RunAsync(report As Action(Of String)) As Task(Of Outcome)
        Dim outcome As New Outcome()
        Dim listener As HttpListener = Nothing
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        Try
            Dim deviceKey As String = store.EnsureDeviceKey()
            Dim deviceName As String = DulukaApi.DeviceName()

            ' 1) start — the server returns the authorize URL + flow state
            report("Contacting the Duluka server…")
            Dim startRes As DulukaApi.Result
            If _kind = FlowKind.Login Then
                Dim body As New JsonObject()
                body("deviceName") = deviceName
                body("deviceKey") = deviceKey
                startRes = Await DulukaApi.PostAsync("/v1/auth/github/start", Nothing, body.ToJsonString()).ConfigureAwait(False)
            Else
                Dim body As New JsonObject()
                body("provider") = "github"
                startRes = Await DulukaApi.PostAsync("/v1/account/providers", store.SessionToken, body.ToJsonString()).ConfigureAwait(False)
            End If
            If Not startRes.Ok Then
                FillError(outcome, startRes)
                Return outcome
            End If
            Dim authorizationUrl As String = Text(startRes.Resource, "authorizationUrl")
            Dim state As String = Text(startRes.Resource, "state")
            If authorizationUrl = "" OrElse state = "" Then
                outcome.ErrorCode = "error.client.protocol"
                outcome.Message = "Start response is missing authorizationUrl/state."
                Return outcome
            End If

            ' 2) host the local redirect listener BEFORE the browser opens
            report("Opening the browser…")
            listener = New HttpListener()
            listener.Prefixes.Add(DulukaApi.CallbackRedirectUri & "/")
            Try
                ' Exact path without the slash — GitHub appends ?code&state to the
                ' configured RedirectUri verbatim. If this stack rejects a
                ' non-slash prefix, the slash form above still matches.
                listener.Prefixes.Add(DulukaApi.CallbackRedirectUri)
            Catch
            End Try
            listener.Start()

            Dim opener As New ProcessStartInfo With {.FileName = authorizationUrl, .UseShellExecute = True}
            Process.Start(opener)
            report("Waiting for GitHub in your browser… (expires in ~10 min)")

            ' 3) wait for the single redirect (state TTL is 10 min server-side)
            Dim contextTask As Task(Of HttpListenerContext) = listener.GetContextAsync()
            Dim waitCts As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token)
            waitCts.CancelAfter(TimeSpan.FromMinutes(11))
            Dim finished As Task = Await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, waitCts.Token)).ConfigureAwait(False)
            If finished IsNot contextTask Then
                If _cts.Token.IsCancellationRequested Then
                    outcome.Cancelled = True
                    outcome.Message = "Sign-in cancelled."
                Else
                    outcome.ErrorCode = "error.client.timeout"
                    outcome.Message = "Timed out waiting for the browser."
                End If
                Return outcome
            End If

            ' HttpListenerContext is not IDisposable here — close the response
            ' explicitly so the browser tab is released on every path.
            Dim ctx As HttpListenerContext = Await contextTask.ConfigureAwait(False)
            Try
                Dim qCode As String = ctx.Request.QueryString("code")
                Dim qError As String = ctx.Request.QueryString("error")
                Dim qState As String = ctx.Request.QueryString("state")

                ' Browser-facing page — the C/2 XSS-hardened builder.
                Dim html As String = OAuthCallbackResponse.BuildCallbackHtml(qError, qCode)
                Dim htmlBytes As Byte() = Encoding.UTF8.GetBytes(html)
                ctx.Response.ContentType = "text/html"
                ctx.Response.StatusCode = 200
                ctx.Response.ContentLength64 = htmlBytes.Length
                Using stream = ctx.Response.OutputStream
                    stream.Write(htmlBytes, 0, htmlBytes.Length)
                End Using

                If qError <> "" Then
                    outcome.Cancelled = True
                    outcome.Message = "Provider reported: " & qError
                    Return outcome
                End If
                If qCode = "" Then
                    outcome.ErrorCode = "error.client.protocol"
                    outcome.Message = "Callback carried no authorization code."
                    Return outcome
                End If

                ' 4) hand code+state to the server; IT exchanges the code
                report("Creating your session…")
                If _kind = FlowKind.Login Then
                    Dim body As New JsonObject()
                    body("code") = qCode
                    body("state") = qState
                    body("deviceKey") = deviceKey
                    Dim done As DulukaApi.Result = Await DulukaApi.PostAsync(
                        "/v1/auth/github/callback", Nothing, body.ToJsonString()).ConfigureAwait(False)
                    If done.Ok AndAlso done.Resource IsNot Nothing Then
                        store.SetSession(Text(done.Resource, "sessionToken"),
                                         Text(done.Resource, "accountId"),
                                         Text(done.Resource, "deviceId"),
                                         Text(done.Resource, "sessionExpiresAt"),
                                         deviceName)
                        outcome.Succeeded = True
                        Return outcome
                    End If
                    ' 403 perm.device_removed = this device's key is dead; drop
                    ' the local key so the next attempt mints a fresh one.
                    If done.HttpStatus = 403 Then
                        store.RevokeDeviceKey()
                    End If
                    FillError(outcome, done)
                    Return outcome
                Else
                    Dim body As New JsonObject()
                    body("code") = qCode
                    body("state") = qState
                    Dim done As DulukaApi.Result = Await DulukaApi.PostAsync(
                        "/v1/account/providers/github/complete", store.SessionToken, body.ToJsonString()).ConfigureAwait(False)
                    If done.Ok Then
                        outcome.Succeeded = True
                        Return outcome
                    End If
                    FillError(outcome, done)
                    Return outcome
                End If
            Finally
                Try
                    ctx.Response.Close()
                Catch
                End Try
            End Try
        Catch ex As OperationCanceledException
            outcome.Cancelled = True
            outcome.Message = "Sign-in cancelled."
            Return outcome
        Catch ex As HttpListenerException
            ' Port 8517 already bound etc. — SAY SO instead of failing silently.
            outcome.ErrorCode = "error.client.env"
            outcome.Message = "Cannot listen on localhost:8517 (already in use?) — " & ex.Message
            Return outcome
        Catch ex As Exception
            outcome.ErrorCode = "error.client.env"
            outcome.Message = "Sign-in flow failed (" & ex.GetType().Name & ")."
            Return outcome
        Finally
            _finished = True
            If listener IsNot Nothing Then
                Try
                    If listener.IsListening Then listener.Stop()
                    listener.Close()
                Catch
                End Try
            End If
            If _active Is Me Then
                _active = Nothing
            End If
        End Try
    End Function

    Private Shared Sub FillError(outcome As Outcome, r As DulukaApi.Result)
        outcome.ErrorCode = r.ErrorCode
        outcome.Message = DulukaApi.HumanError(r)
    End Sub

    Private Shared Function Text(node As JsonNode, name As String) As String
        If node Is Nothing Then Return ""
        Dim value As JsonNode = node(name)
        If value Is Nothing Then Return ""
        Return value.GetValue(Of String)()
    End Function

End Class
