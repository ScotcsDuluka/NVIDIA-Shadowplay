

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

    Private Shared _active As DulukaAuthFlow

    Private ReadOnly _kind As FlowKind
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _finished As Boolean

    Private Sub New(kind As FlowKind)
        _kind = kind
    End Sub

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

    Public Async Function RunAsync(report As Action(Of String)) As Task(Of Outcome)
        Dim outcome As New Outcome()
        Dim listener As HttpListener = Nothing
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        Try
            Dim deviceKey As String = store.EnsureDeviceKey()
            Dim deviceName As String = DulukaApi.DeviceName()

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

            report("Opening the browser…")
            listener = New HttpListener()
            listener.Prefixes.Add(DulukaApi.CallbackRedirectUri & "/")
            Try

                listener.Prefixes.Add(DulukaApi.CallbackRedirectUri)
            Catch
            End Try
            listener.Start()

            Dim opener As New ProcessStartInfo With {.FileName = authorizationUrl, .UseShellExecute = True}
            Process.Start(opener)
            report("Waiting for GitHub in your browser… (expires in ~10 min)")

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

            Dim ctx As HttpListenerContext = Await contextTask.ConfigureAwait(False)
            Try
                Dim qCode As String = ctx.Request.QueryString("code")
                Dim qError As String = ctx.Request.QueryString("error")
                Dim qState As String = ctx.Request.QueryString("state")

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
                        ' Seed the local profile straight from the callback
                        ' when the server provides it: a set-up account must
                        ' not look "needs setup" just because GitHub login
                        ' used to return no username (that blind guess was
                        ' the endless Setup-screen loop). Absent fields =
                        ' old server — the /me fetch on the account home
                        ' remains the fallback source of truth.
                        Dim seededUsername As String = Text(done.Resource, "username")
                        If seededUsername <> "" Then
                            Dim seededName As String = Text(done.Resource, "displayName")
                            store.SetProfile(If(seededName <> "", seededName, store.DisplayName),
                                             seededUsername)
                        End If
                        outcome.Succeeded = True
                        Return outcome
                    End If

                    If done.HttpStatus = 403 OrElse
                       (done.HttpStatus = 409 AndAlso done.ErrorCode = "conflict.link_conflict") Then
                        ' Dead end with THIS device key: revoked server-side
                        ' (403) or bound to a different account (409). Drop it
                        ' so the next attempt mints a fresh key instead of
                        ' failing identically forever — without this the 409
                        ' is a permanent lockout of the sign-in button.
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
