

Imports System.Diagnostics
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Friend Module DulukaApi

    Public ReadOnly Property ApiBase As String
        Get
            Dim fromEnv As String = Environment.GetEnvironmentVariable("DULUKA_API_BASE")
            If Not String.IsNullOrWhiteSpace(fromEnv) Then
                Return fromEnv.TrimEnd("/"c)
            End If
            Return "http://127.0.0.1:5115"
        End Get
    End Property

    Public Const CallbackRedirectUri As String = "http://localhost:8517/v1/auth/github/callback"

    Public Class Result
        Public Ok As Boolean
        Public HttpStatus As Integer
        Public ErrorCode As String = ""
        Public Message As String = ""
        Public Retryable As Boolean
        Public Resource As JsonNode

        Public ReadOnly Property AuthDead As Boolean
            Get
                Return (Not Ok) AndAlso HttpStatus = 401
            End Get
        End Property
    End Class

    Private ReadOnly _http As New HttpClient With {.Timeout = TimeSpan.FromSeconds(15)}

    Public Function DeviceName() As String
        Dim name As String = Environment.MachineName
        If String.IsNullOrWhiteSpace(name) Then name = "ShadowPlay Desktop"
        If name.Length > 64 Then name = name.Substring(0, 64)
        Return name
    End Function

    Public Function GetAsync(path As String, sessionToken As String) As Task(Of Result)
        Return SendAsync(HttpMethod.Get, path, sessionToken, Nothing)
    End Function

    Public Function PostAsync(path As String, sessionToken As String, jsonBody As String) As Task(Of Result)
        Return SendAsync(HttpMethod.Post, path, sessionToken, jsonBody)
    End Function

    Public Function PutAsync(path As String, sessionToken As String, jsonBody As String) As Task(Of Result)
        Return SendAsync(HttpMethod.Put, path, sessionToken, jsonBody)
    End Function

    Public Function DeleteAsync(path As String, sessionToken As String) As Task(Of Result)
        Return SendAsync(HttpMethod.Delete, path, sessionToken, Nothing)
    End Function

    Public Function DeleteAsync(path As String, sessionToken As String, jsonBody As String) As Task(Of Result)
        Return SendAsync(HttpMethod.Delete, path, sessionToken, jsonBody)
    End Function

    Public Async Function SendAsync(method As HttpMethod, path As String, sessionToken As String,
                                    jsonBody As String) As Task(Of Result)
        Dim result As New Result()
        Dim request As HttpRequestMessage = New HttpRequestMessage(method, ApiBase & path)
        Try
            request.Headers.Add("X-ReqId", "ui-" & Guid.NewGuid().ToString("N"))
            If Not String.IsNullOrEmpty(sessionToken) Then
                request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", sessionToken)
            End If
            If jsonBody IsNot Nothing Then
                request.Content = New StringContent(jsonBody, Encoding.UTF8, "application/json")
            End If

            Using response As HttpResponseMessage = Await _http.SendAsync(request).ConfigureAwait(False)
                result.HttpStatus = CInt(response.StatusCode)
                Dim body As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                ParseEnvelope(body, result)
            End Using
        Catch ex As Exception

            Debug.WriteLine($"DulukaApi transport failure: {ex.GetType().Name} — {ex.Message}")
            result.Ok = False
            result.HttpStatus = 0
            result.ErrorCode = "error.client.env"
            result.Retryable = True
            result.Message = "Cannot reach Duluka."
        Finally
            request.Dispose()
        End Try
        Return result
    End Function

    Private Sub ParseEnvelope(body As String, result As Result)
        If String.IsNullOrWhiteSpace(body) Then

            If result.HttpStatus = 404 Then
                result.Ok = False
                result.HttpStatus = 404
                result.ErrorCode = "error.404"
                result.Retryable = False
                result.Message = "This server build does not provide this API (HTTP 404) — update Duluka.Server and restart it."
            Else
                MarkLocal(result, "Empty response from server.")
            End If
            Return
        End If
        Dim root As JsonNode
        Try
            root = JsonNode.Parse(body)
        Catch ex As JsonException
            root = Nothing
        End Try
        If root Is Nothing Then
            MarkLocal(result, "Unrecognized response from server.")
            Return
        End If

        Dim okNode As JsonNode = root("ok")
        result.Ok = okNode IsNot Nothing AndAlso okNode.GetValue(Of Boolean)()

        If result.Ok Then
            result.Resource = root("resource")
        Else
            result.ErrorCode = Text(root("errorCode"))
            result.Message = Text(root("message"))
            Dim retryNode As JsonNode = root("retryable")
            result.Retryable = retryNode IsNot Nothing AndAlso retryNode.GetValue(Of Boolean)()
            Dim statusNode As JsonNode = root("httpStatus")
            If result.HttpStatus = 0 AndAlso statusNode IsNot Nothing Then
                result.HttpStatus = statusNode.GetValue(Of Integer)()
            End If
        End If
    End Sub

    Private Sub MarkLocal(result As Result, message As String)
        result.Ok = False
        result.ErrorCode = "error.client.env"
        result.Retryable = True
        result.Message = message
    End Sub

    Private Function Text(node As JsonNode) As String
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function

    Public Function HumanError(r As Result) As String
        If r.ErrorCode = "error.client.env" Then Return r.Message
        If r.ErrorCode = "github_not_configured" Then
            Return "GitHub sign-in is not configured on the Duluka server — the client secret is missing."
        End If
        If r.HttpStatus = 429 Then Return "Too many requests. Please wait and try again."
        Dim code As String = If(String.IsNullOrEmpty(r.ErrorCode), "error." & r.HttpStatus.ToString(), r.ErrorCode)
        Dim message As String = If(String.IsNullOrEmpty(r.Message), "", " — " & r.Message)
        Return "[" & code & "]" & message
    End Function

End Module
