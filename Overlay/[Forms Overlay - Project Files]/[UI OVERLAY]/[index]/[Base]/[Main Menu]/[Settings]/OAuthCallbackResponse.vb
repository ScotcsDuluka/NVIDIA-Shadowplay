Option Strict On
Option Explicit On

Imports System.Net
Imports System.Text.Json

Friend NotInheritable Class OAuthCallbackResponse

    Private Sub New()
    End Sub

    Friend Shared Function BuildCallbackHtml(errorParam As String, code As String) As String
        If Not String.IsNullOrEmpty(errorParam) Then
            Dim encoded As String = WebUtility.HtmlEncode(errorParam)
            Return $"<html><body><h2>Login Cancelled</h2><p>Error: {encoded}</p><script>setTimeout(function(){{window.close();}}, 2000);</script></body></html>"
        ElseIf String.IsNullOrEmpty(code) Then
            Return "<html><body><h2>Error</h2><p>No authorization code received.</p><script>setTimeout(function(){window.close();}, 2000);</script></body></html>"
        Else
            Return "<html><body><h2>Login Successful!</h2><p>You can close this window now.</p><script>setTimeout(function(){window.close();}, 1500);</script></body></html>"
        End If
    End Function

    Friend Shared Function BuildTokenRequestLogLine(useClientSecret As Boolean, verifierLength As Integer) As String
        If useClientSecret Then
            Return "Token request: Client Secret flow (secret redacted)"
        End If
        Return $"Token request: PKCE flow (code_verifier length={verifierLength}, redacted)"
    End Function

    Friend Shared Function BuildTokenResponseLogLine(json As String) As String
        Try
            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim errEl As JsonElement
                If doc.RootElement.TryGetProperty("error", errEl) Then
                    Dim desc As String = ""
                    Dim descEl As JsonElement
                    If doc.RootElement.TryGetProperty("error_description", descEl) Then
                        desc = descEl.GetString()
                    End If
                    Return $"Token error: {errEl.GetString()} - {desc}"
                End If
                Dim tokEl As JsonElement
                If doc.RootElement.TryGetProperty("access_token", tokEl) Then
                    Return "Token response: access_token received (redacted)"
                End If
            End Using
        Catch
        End Try
        Return "Token response: unrecognized body (redacted)"
    End Function

End Class
