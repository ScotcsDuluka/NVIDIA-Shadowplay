Option Strict On
Option Explicit On

Imports System.Net
Imports System.Text.Json

Friend NotInheritable Class OAuthCallbackResponse

    Private Sub New()
    End Sub

    ''' <summary>The shared page shell for every callback state. Brand DNA:
    ''' dark stage, #161616 card, #76B900 accent, the ring-and-slit mark from
    ''' the installer icons, NVIDIA Sans (installed system-wide by the
    ''' installer) with a Segoe UI fallback. Zero external resources — inline
    ''' CSS/SVG only, safe offline and under strict browsers.
    ''' Placeholders: @@ACCENT@@ @@STATE@@ @@HEADLINE@@ @@SUBLINE@@ @@CLOSE_MS@@.</summary>
    Private Shared ReadOnly PageShell As String = String.Join(
        ControlChars.Lf,
        New String() {
            "<!DOCTYPE html>",
            "<html lang=""en"">",
            "<head>",
            "<meta charset=""utf-8"">",
            "<meta name=""viewport"" content=""width=device-width, initial-scale=1"">",
            "<title>Duluka Shadow — Sign-in</title>",
            "<style>",
            "  * { margin:0; padding:0; box-sizing:border-box; }",
            "  html, body { height:100%; }",
            "  body {",
            "    font-family: 'NVIDIA Sans', 'Segoe UI', system-ui, -apple-system, sans-serif;",
            "    background:#0b0b0b;",
            "    background-image: radial-gradient(1100px 560px at 50% -12%, #1c2812 0%, #101010 52%, #0b0b0b 100%);",
            "    color:#eaeaea;",
            "    display:flex; align-items:center; justify-content:center;",
            "    padding:24px;",
            "  }",
            "  .card {",
            "    width:min(430px, 100%);",
            "    background:#161616;",
            "    border:1px solid #2a2a2a;",
            "    border-radius:20px;",
            "    padding:46px 36px 32px;",
            "    text-align:center;",
            "    box-shadow:0 24px 64px rgba(0,0,0,.55);",
            "    position:relative; overflow:hidden;",
            "    animation:rise .35s ease-out both;",
            "  }",
            "  @keyframes rise { from { opacity:0; transform:translateY(10px); } to { opacity:1; transform:none; } }",
            "  .hair { position:absolute; top:0; left:50%; transform:translateX(-50%);",
            "          width:76px; height:3px; border-radius:3px; background:var(--accent); }",
            "  .mark { width:104px; height:104px; margin:2px auto 20px; display:block; }",
            "  .mark .base { stroke:#2e2e2e; }",
            "  .mark .arc  { stroke:var(--accent); transform-origin:52px 52px; }",
            "  .mark .check{ stroke:var(--accent); stroke-dasharray:64; stroke-dashoffset:64; }",
            "  .mark .cross{ stroke:#e5484d; opacity:0; }",
            "  .mark.busy .arc  { animation:spin .9s linear infinite; }",
            "  .mark.busy .check, .mark.busy .cross { display:none; }",
            "  .mark.done .arc, .mark.done .cross { display:none; }",
            "  .mark.done .check{ display:block; animation:draw .45s ease-out .05s forwards; }",
            "  .mark.done .base { stroke:var(--accent); transition:stroke .3s ease; }",
            "  .mark.fail .arc, .mark.fail .check { display:none; }",
            "  .mark.fail .cross{ display:block; animation:pop .3s ease-out both; }",
            "  @keyframes spin { to { transform:rotate(360deg); } }",
            "  @keyframes draw { to { stroke-dashoffset:0; } }",
            "  @keyframes pop { from { opacity:0; transform:scale(.7); } to { opacity:1; transform:scale(1); } }",
            "  h1 { font-size:21px; font-weight:600; letter-spacing:.2px; margin-bottom:8px; }",
            "  p  { font-size:13.5px; line-height:1.55; color:#9a9a9a; }",
            "  .sub { margin-top:2px; word-wrap:break-word; }",
            "  .count { margin-top:18px; font-size:12px; color:#6f6f6f; }",
            "  button {",
            "    margin-top:16px; padding:9px 26px; border-radius:10px; cursor:pointer;",
            "    font:inherit; font-size:13px; font-weight:600; color:#101010;",
            "    background:var(--accent); border:0;",
            "  }",
            "  button:hover { filter:brightness(1.08); }",
            "  .hint { display:none; margin-top:10px; font-size:12px; color:#6f6f6f; }",
            "</style>",
            "</head>",
            "<body>",
            "  <div class=""card"" style=""--accent:@@ACCENT@@"">",
            "    <div class=""hair""></div>",
            "    <svg class=""mark busy"" id=""mark"" viewBox=""0 0 104 104"" aria-hidden=""true"">",
            "      <circle class=""base"" cx=""52"" cy=""52"" r=""41"" fill=""none"" stroke-width=""6""/>",
            "      <rect x=""0"" y=""50"" width=""104"" height=""4"" fill=""#161616""/>",
            "      <circle class=""arc"" cx=""52"" cy=""52"" r=""41"" fill=""none"" stroke-width=""6"" stroke-linecap=""round"" stroke-dasharray=""78 180""/>",
            "      <path class=""check"" d=""M35 54 L48 67 L70 40"" fill=""none"" stroke-width=""6"" stroke-linecap=""round"" stroke-linejoin=""round""/>",
            "      <g class=""cross"" stroke-width=""6"" stroke-linecap=""round"">",
            "        <line x1=""40"" y1=""40"" x2=""64"" y2=""64""/>",
            "        <line x1=""64"" y1=""40"" x2=""40"" y2=""64""/>",
            "      </g>",
            "    </svg>",
            "    <div id=""block-busy"">",
            "      <h1>Completing sign-in…</h1>",
            "      <p>Finishing your Duluka session — this only takes a moment.</p>",
            "    </div>",
            "    <div id=""block-done"" style=""display:none"">",
            "      <h1>You’re signed in!</h1>",
            "      <p>All set — you can return to Duluka Shadow now.</p>",
            "    </div>",
            "    <div id=""block-fail"" style=""display:none"">",
            "      <h1>@@HEADLINE@@</h1>",
            "      <p class=""sub"">@@SUBLINE@@</p>",
            "    </div>",
            "    <p class=""count"" id=""count""></p>",
            "    <button id=""close"" type=""button"">Close window</button>",
            "    <p class=""hint"" id=""hint"">Browser refused the auto-close — just close this tab.</p>",
            "  </div>",
            "<script>",
            "  (function () {",
            "    var state = '@@STATE@@';",
            "    var closeMs = @@CLOSE_MS@@;",
            "    var mark = document.getElementById('mark');",
            "    var countdown = document.getElementById('count');",
            "    function show(id) {",
            "      ['block-busy', 'block-done', 'block-fail'].forEach(function (b) {",
            "        document.getElementById(b).style.display = (b === id) ? 'block' : 'none';",
            "      });",
            "    }",
            "    function startCountdown() {",
            "      var deadline = Date.now() + closeMs;",
            "      setInterval(function () {",
            "        var left = Math.max(0, Math.ceil((deadline - Date.now()) / 1000));",
            "        countdown.textContent = 'This window closes automatically in ' + left + '…';",
            "      }, 250);",
            "      setTimeout(function () {",
            "        window.close();",
            "        document.getElementById('hint').style.display = 'block';",
            "      }, closeMs);",
            "    }",
            "    document.getElementById('close').addEventListener('click', function () {",
            "      window.close();",
            "      document.getElementById('hint').style.display = 'block';",
            "    });",
            "    if (state === 'busy') {",
            "      show('block-busy');",
            "      setTimeout(function () {",
            "        mark.setAttribute('class', 'mark done');",
            "        show('block-done');",
            "        startCountdown();",
            "      }, 1100);",
            "    } else {",
            "      mark.setAttribute('class', 'mark fail');",
            "      show('block-fail');",
            "      startCountdown();",
            "    }",
            "  })();",
            "</script>",
            "</body>",
            "</html>"
        })

    ''' <summary>Builds the browser page GitHub lands on after the OAuth
    ''' redirect. Three states: a provider error (red cross + the provider's
    ''' message), a missing code (red cross), and the happy path (spinner
    ''' that resolves to the brand check, then auto-closes).
    ''' Everything is HTML-escaped where user/provider data is embedded.</summary>
    Friend Shared Function BuildCallbackHtml(errorParam As String, code As String) As String
        Dim state As String, accent As String, headline As String, subline As String, closeMs As String

        If Not String.IsNullOrEmpty(errorParam) Then
            state = "fail"
            accent = "#e5484d"
            headline = "Login cancelled"
            subline = "GitHub reported: <b>" & WebUtility.HtmlEncode(errorParam) &
                      "</b>.<br>You can safely close this window and try again from the app."
            closeMs = "8000"
        ElseIf String.IsNullOrEmpty(code) Then
            state = "fail"
            accent = "#e5484d"
            headline = "Sign-in incomplete"
            subline = "No authorization code was received.<br>Close this window and try again from the app."
            closeMs = "8000"
        Else
            state = "busy"
            accent = "#76B900"
            headline = "Completing sign-in…"
            subline = "Finishing your Duluka session — this only takes a moment."
            closeMs = "3200"
        End If

        Return PageShell.
            Replace("@@ACCENT@@", accent).
            Replace("@@STATE@@", state).
            Replace("@@HEADLINE@@", headline).
            Replace("@@SUBLINE@@", subline).
            Replace("@@CLOSE_MS@@", closeMs)
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
