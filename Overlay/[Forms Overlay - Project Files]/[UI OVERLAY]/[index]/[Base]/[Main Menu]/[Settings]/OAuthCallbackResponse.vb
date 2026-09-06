Option Strict On
Option Explicit On

' OAuthCallbackResponse.vb — C/2: the GitHub OAuth callback surface,
' extracted from Base_Connect.ProcessCallback / GetAccessToken so the
' boundary tests exercise the SAME code the Overlay runs (zero copy drift).
'
' Two facts pinned here by tests:
'   1. CALLBACK HTML: QueryString("error") is ATTACKER-CONTROLLED text — any
'      web page can navigate a browser to http://localhost:8765/callback/
'      with an arbitrary query string. It MUST be HTML-encoded before being
'      embedded in the response, or the callback page is a reflected-XSS
'      gadget in the localhost origin.
'   2. SECRET HYGIENE: Debug.WriteLine must never carry client_secret,
'      code_verifier, or access_token. Logs are summaries only.

Imports System.Net
Imports System.Text.Json

Friend NotInheritable Class OAuthCallbackResponse

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Build the browser-facing HTML for one OAuth callback request.
    ''' errorParam wins over code (matches GitHub's contract).
    '''
    ''' errorParam is ATTACKER-CONTROLLED (any web page can navigate a user to
    ''' http://localhost:8765/callback/?error=<payload>) — it is HTML-encoded
    ''' here so the callback page can never execute reflected script.
    ''' </summary>
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

    ''' <summary>
    ''' Log line for the token request — flow + verifier length ONLY.
    ''' The raw FormUrlEncoded body carries client_secret or code_verifier.
    ''' </summary>
    Friend Shared Function BuildTokenRequestLogLine(useClientSecret As Boolean, verifierLength As Integer) As String
        If useClientSecret Then
            Return "Token request: Client Secret flow (secret redacted)"
        End If
        Return $"Token request: PKCE flow (code_verifier length={verifierLength}, redacted)"
    End Function

    ''' <summary>
    ''' Log line for the token response — error fields only; any access_token
    ''' is announced by PRESENCE, never by value.
    ''' </summary>
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
