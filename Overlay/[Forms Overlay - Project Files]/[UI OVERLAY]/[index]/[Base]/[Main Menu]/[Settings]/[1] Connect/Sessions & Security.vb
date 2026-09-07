' Sessions & Security — the current session at a glance, plus the two
' destructive actions: extend the session (refresh) and sign out on ALL
' devices (revoke-all, two-step confirm — it signs out every signed-in
' device, including this one). 401 anywhere is the terminal path.

Imports System.Diagnostics
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Security
    Inherits System.Windows.Forms.Form

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

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
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub

    Private _confirmAll As Boolean
    Private _busy As Boolean

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then
            _confirmAll = False
            BT_RevokeAll.Text = "Sign out on ALL devices"
            SetupPasswordForm()
            RenderSessionInfo()
        End If
    End Sub

    Private Sub RenderSessionInfo()
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        ' This is a DULUKA ACCOUNT SESSION (issued by the Duluka server after
        ' provider authentication) — never a "GitHub session". The server does
        ' not expose a session-creation timestamp on any v0 endpoint, so only
        ' device + expiry are shown (nothing is faked).
        Dim lines As String = "Current Duluka Account Session" & Environment.NewLine &
                              "Device  " & If(store.DeviceName <> "", store.DeviceName, "—") & Environment.NewLine &
                              "Expires  " & If(store.SessionExpiresAtText <> "", store.SessionExpiresAtText, "—")
        Info_META.Text = lines
        Status_TEXT.Text = ""
    End Sub

    Private Async Sub BT_RefreshSession_Click(sender As Object, e As EventArgs) Handles BT_RefreshSession.Click
        If _busy Then Return
        _busy = True
        Try
            Status_TEXT.Text = "Extending session…"
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/auth/session/refresh", token, "{}").ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                Dim expires As JsonNode = r.Resource("sessionExpiresAt")
                If expires IsNot Nothing Then
                    DulukaAccountStore.Instance.SetSessionExpiry(expires.GetValue(Of String)())
                End If
                RenderSessionInfo()
                Status_TEXT.Text = "Session extended."
            ElseIf r.AuthDead Then
                TerminalSignOut("Session expired — signed out.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Finally
            _busy = False
        End Try
    End Sub

    Private Async Sub BT_RevokeAll_Click(sender As Object, e As EventArgs) Handles BT_RevokeAll.Click
        If _busy Then Return

        ' Two-step confirm: the first click only arms the button.
        If Not _confirmAll Then
            _confirmAll = True
            BT_RevokeAll.Text = "Really sign out EVERYWHERE? Click again"
            Status_TEXT.Text = "This ends your Duluka Account Session on every device, including this one."
            Return
        End If

        _busy = True
        Try
            Status_TEXT.Text = "Signing out everywhere…"
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/auth/sessions/revoke-all", token, "{}").ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                Dim countText As String = "All devices signed out."
                Dim countNode As JsonNode = If(r.Resource IsNot Nothing, r.Resource("revokedSessions"), Nothing)
                If countNode IsNot Nothing Then
                    countText = "Signed out on " & countNode.GetValue(Of Integer)().ToString() & " device(s)."
                End If
                TerminalSignOut(countText)
            ElseIf r.AuthDead Then
                ' Already gone server-side — the local wipe is still correct.
                TerminalSignOut("Session was already gone — signed out.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
                _confirmAll = False
                BT_RevokeAll.Text = "Sign out on ALL devices"
            End If
        Finally
            _busy = False
        End Try
    End Sub

    ''' <summary>Provider-only (bootstrapped via GitHub) accounts have no
    ' username/password yet — the password form switches to FIRST-TIME SETUP:
    ' the user picks a username, no current password exists to verify. This
    ' also releases the last-provider trap so GitHub can be unlinked later.</summary>
    Private ReadOnly Property ProviderOnlyAccount As Boolean
        Get
            Return DulukaAccountStore.Instance.Username = ""
        End Get
    End Property

    Private Async Sub BT_ChangePassword_Click(sender As Object, e As EventArgs) Handles BT_ChangePassword.Click
        If _busy Then Return
        Dim providerOnly As Boolean = ProviderOnlyAccount
        Dim username As String = PwUsername_BOX.Text.Trim()
        Dim current As String = PwCurrent_BOX.Text
        Dim newPw As String = PwNew_BOX.Text
        Dim confirm As String = PwConfirm_BOX.Text

        If providerOnly Then
            If username = "" OrElse newPw = "" OrElse confirm = "" Then
                Status_TEXT.Text = "Choose a username and fill both password fields."
                Return
            End If
            If username.Length < 3 OrElse username.Length > 32 OrElse
               username.Any(Function(c) Not (Char.IsLetterOrDigit(c) OrElse c = "."c OrElse c = "_"c OrElse c = "-"c)) Then
                Status_TEXT.Text = "Username: 3-32 characters — letters, digits, dot, underscore, hyphen."
                Return
            End If
        Else
            If current = "" OrElse newPw = "" OrElse confirm = "" Then
                Status_TEXT.Text = "Fill in every password field."
                Return
            End If
        End If
        If newPw.Length < 8 OrElse newPw.Length > 128 Then
            Status_TEXT.Text = "New password must be 8-128 characters."
            Return
        End If
        If newPw <> confirm Then
            Status_TEXT.Text = "New passwords do not match."
            Return
        End If

        _busy = True
        Try
            Status_TEXT.Text = If(providerOnly, "Adding password sign-in…", "Updating password…")
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim body As New System.Text.Json.Nodes.JsonObject()
            body("newPassword") = newPw
            If providerOnly Then
                body("username") = username
            Else
                body("currentPassword") = current
            End If
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync(
                "/v1/account/password", token, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                PwUsername_BOX.Clear()
                PwCurrent_BOX.Clear()
                PwNew_BOX.Clear()
                PwConfirm_BOX.Clear()
                ' The account now has a username — cache it so the form flips
                ' to the change-password mode without a full /me round trip.
                If providerOnly Then DulukaAccountStore.Instance.SetProfile(
                    DulukaAccountStore.Instance.DisplayName, username)
                SetupPasswordForm()
                Status_TEXT.Text = If(providerOnly,
                    "Account set up — you can now sign in with your username and password.",
                    "Password updated — you can now sign in with it.")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = "Current password is incorrect."
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "conflict.username_taken" Then
                Status_TEXT.Text = "That username is already taken — pick another."
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Finally
            _busy = False
        End Try
    End Sub

    ''' <summary>Arranges the password form for the account kind: change mode
    ' (current password required) vs first-time adoption mode (username
    ' picker, no current password). The username is PERMANENT once set —
    ' adoption mode is therefore only reachable while it is still unset.</summary>
    Private Sub SetupPasswordForm()
        Dim providerOnly As Boolean = ProviderOnlyAccount
        PwUsername_LBL.Visible = providerOnly
        PwUsername_BOX.Visible = providerOnly
        PwCurrent_LBL.Text = If(providerOnly, "Current password (not set yet)", "Current password")
        PwCurrent_LBL.Enabled = Not providerOnly
        PwCurrent_BOX.Enabled = Not providerOnly
        PwHeader_LBL.Text = If(providerOnly, "Add password sign-in", "Change password")
        BT_ChangePassword.Text = If(providerOnly, "Set Up Account", "Change password")
    End Sub

    Private Sub TerminalSignOut(message As String)
        DulukaAccountStore.Instance.ClearSession()
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
        Base_Connect.NotifyFromSubPage(message)
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub
End Class
