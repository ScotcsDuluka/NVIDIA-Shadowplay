Imports System.Diagnostics
Imports System.Drawing
Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports System.Net.Http.Headers
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks
Imports System.Security.Cryptography
Imports System.IO
Imports System.Text
Imports System.Threading

Public Class Base_Connect
    Inherits NoCloseForm

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ GitHub App Configuration
    ' ═══════════════════════════════════════════════════════════════════════════════
    Private Const CLIENT_ID As String = "Iv23liJGjq9Pbp2XhAOM"

    ' ⚠️ ใส่ Client Secret ที่นี่ (สร้างจาก GitHub App settings → Client secrets → Generate)
    Private Const CLIENT_SECRET As String = ""  ' <-- ใส่ secret ที่นี่ถ้า PKCE ไม่ทำงาน

    ' ✅ OAuth callback port — MUST NOT be 5000 (that's the TCP Hub port used by
    ' TcpClientHelper to reach Engine/API/Notifier). Old code used 5000, which made
    ' HttpListener fail to bind whenever the Hub was already running (i.e. always).
    Private Const OAUTH_CALLBACK_PORT As Integer = 8765
    Private Const OAUTH_CALLBACK_URL As String = "http://localhost:8765/callback/"

    ' ✅ PKCE variables
    Private _codeVerifier As String = ""
    Private _codeChallenge As String = ""

    ' ✅ HttpListener with proper cleanup
    Private _listener As HttpListener
    Private _listenerCts As Threading.CancellationTokenSource
    Private _isListening As Boolean = False

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' Window Style Constants
    ' ═══════════════════════════════════════════════════════════════════════════════
    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' Window Message Handling
    ' ═══════════════════════════════════════════════════════════════════════════════
    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1
        If m.Msg = WM_NCHITTEST Then
            Dim pos As Point = Me.PointToClient(Cursor.Position)
            If Me.GetChildAtPoint(pos) Is Nothing Then
                m.Result = CType(HTTRANSPARENT, IntPtr)
                Return
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' Form Load
    ' ═══════════════════════════════════════════════════════════════════════════════
    Private Sub Base_Connect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        LoadUserInfo()
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' Form Closing - Cleanup
    ' ═══════════════════════════════════════════════════════════════════════════════
    Private Sub Base_Connect_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        StopLoginListener()
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ PKCE Helper Methods
    ' ═══════════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' สร้าง cryptographically random string สำหรับ code_verifier
    ''' </summary>
    Private Function GenerateCodeVerifier() As String
        ' PKCE requires 43-128 characters
        Dim bytes(31) As Byte ' 32 bytes = 43 base64url chars
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(bytes)
        End Using
        Return Base64UrlEncode(bytes)
    End Function

    ''' <summary>
    ''' สร้าง SHA256 hash และ encode เป็น base64url
    ''' </summary>
    Private Function GenerateCodeChallenge(verifier As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = Encoding.ASCII.GetBytes(verifier)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Return Base64UrlEncode(hash)
        End Using
    End Function

    ''' <summary>
    ''' Base64Url encode (ไม่มี padding, ใช้ - แทน +, _ แทน /)
    ''' </summary>
    Private Function Base64UrlEncode(bytes As Byte()) As String
        Dim base64 As String = Convert.ToBase64String(bytes)
        Return base64.Replace("+", "-").Replace("/", "_").Replace("=", "")
    End Function

    ''' <summary>
    ''' เตรียม PKCE parameters สำหรับ OAuth flow
    ''' </summary>
    Private Sub PreparePKCE()
        _codeVerifier = GenerateCodeVerifier()
        _codeChallenge = GenerateCodeChallenge(_codeVerifier)
        Debug.WriteLine($"PKCE prepared:")
        Debug.WriteLine($"  verifier: {_codeVerifier}")
        Debug.WriteLine($"  verifier length: {_codeVerifier.Length}")
        Debug.WriteLine($"  challenge: {_codeChallenge}")
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ Login Flow - GitHub App
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Sub Github_Text_Click(sender As Object, e As EventArgs) Handles Github_Text.Click
        Try
            ' ✅ Stop existing listener ก่อนเริ่มใหม่
            StopLoginListener()

            ' ✅ เตรียม PKCE parameters
            PreparePKCE()

            ' ✅ Start listener ก่อนเปิด browser
            StartLoginListener()

            ' ✅ สร้าง authorize URL
            ' ถ้ามี CLIENT_SECRET → ใช้ PKCE flow
            ' ถ้าไม่มี → ใช้ PKCE only
            Dim url As String

            If String.IsNullOrEmpty(CLIENT_SECRET) Then
                ' PKCE only flow
                url = "https://github.com/login/oauth/authorize?" &
                      "client_id=" & CLIENT_ID &
                      "&redirect_uri=" & OAUTH_CALLBACK_URL.TrimEnd("/"c) &
                      "&response_type=code" &
                      "&code_challenge=" & _codeChallenge &
                      "&code_challenge_method=S256"
            Else
                ' Standard OAuth flow with client_secret
                url = "https://github.com/login/oauth/authorize?" &
                      "client_id=" & CLIENT_ID &
                      "&redirect_uri=" & OAUTH_CALLBACK_URL.TrimEnd("/"c) &
                      "&response_type=code"
            End If

            Debug.WriteLine($"Opening browser...")
            Debug.WriteLine($"URL: {url}")

            Dim psi As New ProcessStartInfo With {
                .FileName = url,
                .UseShellExecute = True
            }

            Process.Start(psi)

        Catch ex As Exception
            Debug.WriteLine($"Github_Text_Click Error: {ex.Message}")
            MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ HttpListener with proper cleanup
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Sub StartLoginListener()
        Try
            ' Stop existing listener if any
            If _isListening Then
                StopLoginListener()
            End If

            _listener = New HttpListener()
            _listener.Prefixes.Add(OAUTH_CALLBACK_URL)
            _listener.Start()

            _listenerCts = New Threading.CancellationTokenSource()
            _isListening = True

            Debug.WriteLine($"HttpListener started on {OAUTH_CALLBACK_URL}")

            ' ✅ Listen asynchronously with cancellation support
            Task.Run(Async Function()
                         Try
                             While _isListening AndAlso Not _listenerCts.Token.IsCancellationRequested
                                 Dim getContextTask As Task(Of HttpListenerContext) = _listener.GetContextAsync()

                                 ' Wait for request or cancellation
                                 Dim completedTask As Task = Await Task.WhenAny(getContextTask, Task.Delay(Threading.Timeout.Infinite, _listenerCts.Token))

                                 If completedTask Is getContextTask Then
                                     Dim context As HttpListenerContext = Await getContextTask
                                     ProcessCallback(context)
                                 End If
                             End While

                         Catch ex As HttpListenerException
                             Debug.WriteLine($"HttpListener stopped: {ex.Message}")
                         Catch ex As OperationCanceledException
                             Debug.WriteLine("HttpListener cancelled")
                         Catch ex As Exception
                             Debug.WriteLine($"HttpListener error: {ex.Message}")
                         End Try
                     End Function, _listenerCts.Token)

        Catch ex As Exception
            Debug.WriteLine($"StartLoginListener Error: {ex.Message}")
            _isListening = False
        End Try
    End Sub

    Private Sub StopLoginListener()
        Try
            _isListening = False

            If _listenerCts IsNot Nothing Then
                _listenerCts.Cancel()
                _listenerCts.Dispose()
                _listenerCts = Nothing
            End If

            If _listener IsNot Nothing Then
                If _listener.IsListening Then
                    _listener.Stop()
                End If
                _listener.Close()
                _listener = Nothing
            End If

            Debug.WriteLine("HttpListener stopped and disposed")

        Catch ex As Exception
            Debug.WriteLine($"StopLoginListener Error: {ex.Message}")
        End Try
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ Process OAuth Callback
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Async Sub ProcessCallback(context As HttpListenerContext)
        Try
            Dim request As HttpListenerRequest = context.Request
            Dim response As HttpListenerResponse = context.Response

            ' ✅ รับ code จาก query string
            Dim code As String = request.QueryString("code")
            Dim [error] As String = request.QueryString("error")

            ' ✅ ส่ง response กลับไปยัง browser
            Dim responseHtml As String

            If Not String.IsNullOrEmpty([error]) Then
                responseHtml = $"<html><body><h2>Login Cancelled</h2><p>Error: {[error]}</p><script>setTimeout(function(){{window.close();}}, 2000);</script></body></html>"
                Debug.WriteLine($"OAuth error: {[error]}")
            ElseIf String.IsNullOrEmpty(code) Then
                responseHtml = "<html><body><h2>Error</h2><p>No authorization code received.</p><script>setTimeout(function(){window.close();}, 2000);</script></body></html>"
                Debug.WriteLine("No authorization code received")
            Else
                responseHtml = "<html><body><h2>Login Successful!</h2><p>You can close this window now.</p><script>setTimeout(function(){window.close();}, 1500);</script></body></html>"

                ' ✅ แลก code เป็น token
                Dim token As String = Await GetAccessToken(code)

                If Not String.IsNullOrEmpty(token) Then
                    ' ✅ ดึงข้อมูล user
                    Await GetUser(token)

                    ' ✅ แสดง notification บน UI thread
                    Invoke(Sub()
                               MessageBox.Show("Login สำเร็จ 😏🔥", "GitHub Login", MessageBoxButtons.OK, MessageBoxIcon.Information)
                           End Sub)
                End If
            End If

            ' Send response
            Dim buffer As Byte() = Encoding.UTF8.GetBytes(responseHtml)
            response.ContentLength64 = buffer.Length
            response.ContentType = "text/html"
            response.StatusCode = 200

            Using output As Stream = response.OutputStream
                output.Write(buffer, 0, buffer.Length)
            End Using

        Catch ex As Exception
            Debug.WriteLine($"ProcessCallback Error: {ex.Message}")
        End Try
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ Get Access Token (รองรับทั้ง PKCE และ Client Secret)
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Async Function GetAccessToken(code As String) As Task(Of String)
        Try
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Add("Accept", "application/json")

                Dim values As New Dictionary(Of String, String) From {
                    {"client_id", CLIENT_ID},
                    {"code", code},
                    {"redirect_uri", OAUTH_CALLBACK_URL.TrimEnd("/"c)}
                }

                ' ✅ ถ้ามี CLIENT_SECRET → ใช้ client_secret
                ' ✅ ถ้าไม่มี → ใช้ PKCE (code_verifier)
                If Not String.IsNullOrEmpty(CLIENT_SECRET) Then
                    values.Add("client_secret", CLIENT_SECRET)
                    Debug.WriteLine("Using: Client Secret flow")
                Else
                    values.Add("code_verifier", _codeVerifier)
                    Debug.WriteLine("Using: PKCE flow")
                End If

                Dim content As New FormUrlEncodedContent(values)

                ' ✅ Debug
                Dim requestBody As String = Await content.ReadAsStringAsync()
                Debug.WriteLine($"Token request: {requestBody}")

                Dim response As HttpResponseMessage = Await client.PostAsync("https://github.com/login/oauth/access_token", content)
                Dim json As String = Await response.Content.ReadAsStringAsync()

                Debug.WriteLine($"Token response: {json}")

                Dim doc As JsonDocument = JsonDocument.Parse(json)

                ' ✅ Check for error
                Dim errorElement As JsonElement
                If doc.RootElement.TryGetProperty("error", errorElement) Then
                    Dim errorMsg As String = errorElement.GetString()
                    Dim errorDesc As String = ""
                    Dim descElement As JsonElement
                    If doc.RootElement.TryGetProperty("error_description", descElement) Then
                        errorDesc = descElement.GetString()
                    End If

                    Debug.WriteLine($"Token error: {errorMsg} - {errorDesc}")

                    Invoke(Sub()
                               MessageBox.Show($"Login failed: {errorMsg}" & vbCrLf & errorDesc, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                           End Sub)

                    Return Nothing
                End If

                ' ✅ Get access token
                Dim tokenElement As JsonElement
                If doc.RootElement.TryGetProperty("access_token", tokenElement) Then
                    Return tokenElement.GetString()
                End If

                Return Nothing
            End Using

        Catch ex As Exception
            Debug.WriteLine($"GetAccessToken Error: {ex.Message}")
            Return Nothing
        End Try
    End Function

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ Get User Info and Save
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Async Function GetUser(token As String) As Task
        Try
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", token)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DulukaShadow")

                ' Get user info
                Dim json As String = Await client.GetStringAsync("https://api.github.com/user")
                Dim doc As JsonDocument = JsonDocument.Parse(json)

                Dim username As String = doc.RootElement.GetProperty("login").GetString()
                Dim avatarUrl As String = doc.RootElement.GetProperty("avatar_url").GetString()

                Debug.WriteLine($"GitHub user: {username}, avatar: {avatarUrl}")

                ' ✅ Get avatar bytes
                Dim avatarBytes As Byte() = Await client.GetByteArrayAsync(avatarUrl)

                ' ✅ สร้าง avatar image อย่างถูกต้อง
                Dim avatarImg As Bitmap
                Using ms As New MemoryStream(avatarBytes)
                    avatarImg = New Bitmap(ms)
                End Using

                ' ✅ Update UI on UI thread
                Invoke(Sub()
                           USERSNAME_TEXT.Text = username
                           Box_PNG.BackgroundImage = avatarImg
                       End Sub)

                ' ✅ Save to AppSettings (main storage)
                AppSettings.Instance.SaveGitHubUser(username, avatarUrl, token)
                AppSettings.Instance.SaveAvatarFromBytes(avatarBytes)

                Debug.WriteLine($"User saved: {username}")

            End Using

        Catch ex As Exception
            Debug.WriteLine($"GetUser Error: {ex.Message}")
        End Try
    End Function

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' ✅ Load User Info from AppSettings
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Sub LoadUserInfo()
        Try
            ' ✅ โหลดจาก AppSettings
            If AppSettings.Instance.IsGitHubLoggedIn Then
                USERSNAME_TEXT.Text = AppSettings.Instance.GitHubUser.Username

                ' ✅ โหลด avatar
                Dim avatarPath As String = Path.Combine(Application.StartupPath, "avatar.png")
                If File.Exists(avatarPath) Then
                    Try
                        ' ✅ อ่าน bytes และสร้าง image ใหม่เพื่อหลีกเลี่ยง file lock
                        Dim bytes As Byte() = File.ReadAllBytes(avatarPath)
                        Using ms As New MemoryStream(bytes)
                            Box_PNG.BackgroundImage = New Bitmap(ms)
                        End Using
                    Catch ex As Exception
                        Debug.WriteLine($"LoadUserInfo avatar error: {ex.Message}")
                    End Try
                End If

                Debug.WriteLine($"Loaded user: {AppSettings.Instance.GitHubUser.Username}")
            Else
                USERSNAME_TEXT.Text = ""
                Debug.WriteLine("No user logged in")
            End If

        Catch ex As Exception
            Debug.WriteLine($"LoadUserInfo Error: {ex.Message}")
        End Try
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' Logout
    ' ═══════════════════════════════════════════════════════════════════════════════

    Public Sub Logout()
        Try
            ' ✅ Clear from AppSettings
            AppSettings.Instance.ClearGitHubUser()

            ' ✅ Delete avatar cache
            Dim avatarPath As String = Path.Combine(Application.StartupPath, "avatar.png")
            If File.Exists(avatarPath) Then
                File.Delete(avatarPath)
            End If

            ' ✅ Clear UI
            USERSNAME_TEXT.Text = ""
            Box_PNG.BackgroundImage = Nothing

            Debug.WriteLine("User logged out")

        Catch ex As Exception
            Debug.WriteLine($"Logout Error: {ex.Message}")
        End Try
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════════════
    ' Navigation
    ' ═══════════════════════════════════════════════════════════════════════════════

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub
End Class
