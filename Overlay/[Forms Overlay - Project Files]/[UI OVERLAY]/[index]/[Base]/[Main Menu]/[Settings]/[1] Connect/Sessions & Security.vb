' Sessions & Security — the current session at a glance, plus the three
' destructive actions: extend the session (refresh), sign out on ALL devices
' (revoke-all, two-step confirm — it signs out every signed-in device,
' including this one) and DELETE THE ACCOUNT ITSELF (irreversible cascade —
' account, username/password, linked providers, devices and sessions; two-step
' confirm + current-password proof when the account has one). 401 anywhere is
' the terminal path.

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
    Private _confirmDelete As Boolean
    Private _busy As Boolean

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then
            _confirmAll = False
            BT_RevokeAll.Text = "Sign out on ALL devices"
            _confirmDelete = False
            BT_DeleteAccount.Text = "Delete this account permanently"
            DzPassword_BOX.Clear()
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
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_username" Then
                Status_TEXT.Text = "Username: 3-32 characters — letters, digits, dot, underscore, hyphen."
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_password" Then
                Status_TEXT.Text = "Password must be 8-128 characters."
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "conflict.username_taken" Then
                ' The code covers BOTH "username taken" AND "this account
                ' already has a password" (credential_exists maps to it — e.g.
                ' the account was initialized on another device). Server truth
                ' decides: a /me username means this account is DONE — adopt
                ' it and leave the setup state instead of dead-ending.
                Dim meR As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/me", token).ConfigureAwait(True)
                If Not IsDisposed AndAlso meR.Ok AndAlso meR.Resource IsNot Nothing Then
                    Dim meUser As String = ResourceText(meR.Resource, "username")
                    If meUser <> "" Then
                        DulukaAccountStore.Instance.SetProfile(ResourceText(meR.Resource, "displayName"), meUser)
                        SetupPasswordForm()
                        Status_TEXT.Text = "This account already has a username and password."
                        Return
                    End If
                End If
                If Not IsDisposed Then Status_TEXT.Text = "That username is already taken — pick another."
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            ' NEVER silent: a transport or parsing failure must land in the
            ' status line, not vanish (and never escape an Async Sub).
            Debug.WriteLine($"BT_ChangePassword error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = "Cannot reach Duluka — try again."
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
        ' Honest button label per mode — first-time adoption SETS UP the
        ' native credential; it does not change an existing one.
        BT_ChangePassword.Text = If(providerOnly, "Set Up Account", "Change password")
        ' Danger zone: the deletion password row exists ONLY for accounts
        ' that actually have a password to verify against.
        Dim hasPassword As Boolean = Not providerOnly
        DzPassword_LBL.Visible = hasPassword
        DzPassword_BOX.Visible = hasPassword
        LayoutPasswordRows()
    End Sub

    ''' <summary>Stacks the password rows top-to-bottom (header → [username]
    ' → current → new → confirm → button) and the DANGER ZONE below them
    ' (header → note → [current password] → delete button), with the status
    ' line last. Rows hidden by the mode are collapsed instead of leaving a
    ' fixed hole, so every layout stays tight and nothing can overlap — the
    ' static Designer slots are the fallback.</summary>
    Private Sub LayoutPasswordRows()
        Const RowPitch As Integer = 36
        Const ButtonGap As Integer = 40
        Dim y As Integer = PwHeader_LBL.Top
        If PwUsername_LBL.Visible Then
            PwUsername_LBL.Top = y + 4 : PwUsername_BOX.Top = y
            y += RowPitch
        End If
        PwCurrent_LBL.Top = y + 4 : PwCurrent_BOX.Top = y : y += RowPitch
        PwNew_LBL.Top = y + 4 : PwNew_BOX.Top = y : y += RowPitch
        PwConfirm_LBL.Top = y + 4 : PwConfirm_BOX.Top = y : y += RowPitch
        BT_ChangePassword.Top = y + ButtonGap - RowPitch + 4

        ' Danger zone stacks below the password form in BOTH modes — the
        ' optional deletion-password row collapses cleanly like the rest.
        y = BT_ChangePassword.Top + BT_ChangePassword.Height + 28
        DzHeader_LBL.Top = y
        y += 34
        DzNote_META.Top = y
        y += DzNote_META.Height + 10
        If DzPassword_LBL.Visible Then
            DzPassword_LBL.Top = y + 4 : DzPassword_BOX.Top = y
            y += RowPitch
        End If
        BT_DeleteAccount.Top = y + 14
        Status_TEXT.Top = BT_DeleteAccount.Top + BT_DeleteAccount.Height + 18
    End Sub

    ' ── delete account (irreversible) ────────────────────────────────────────

    ''' <summary>DELETE /v1/account — the irreversible one. Two-step confirm
    ' (same discipline as revoke-all): the first click only ARMS the button,
    ' nothing is sent. An account WITH a password must type it (server
    ' enforces the same bar as the password change); a provider-only account
    ' has nothing to type. Success means the account, its username/password,
    ' every linked provider, all devices and sessions are GONE server-side —
    ' the local wipe lands the user on Sign in. NEVER silent: every outcome
    ' lands in the status line, and the armed state resets on any refusal.</summary>
    Private Async Sub BT_DeleteAccount_Click(sender As Object, e As EventArgs) Handles BT_DeleteAccount.Click
        If _busy Then Return

        If Not _confirmDelete Then
            _confirmDelete = True
            BT_DeleteAccount.Text = "Really delete EVERYTHING? Click again"
            Status_TEXT.Text = "This removes the account, its username and password, linked providers, devices and sessions — permanently."
            Return
        End If

        Dim hasPassword As Boolean = Not ProviderOnlyAccount
        Dim password As String = DzPassword_BOX.Text
        If hasPassword AndAlso password = "" Then
            Status_TEXT.Text = "Type your current password to confirm the deletion."
            Return
        End If

        _busy = True
        Try
            Status_TEXT.Text = "Deleting account…"
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim body As String = "{}"
            If hasPassword Then
                Dim obj As New System.Text.Json.Nodes.JsonObject()
                obj("currentPassword") = password
                body = obj.ToJsonString()
            End If
            Dim r As DulukaApi.Result = Await DulukaApi.DeleteAsync("/v1/account", token, body).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                ' The account is GONE server-side — the local wipe is the
                ' whole point. Land on Sign in with the outcome in its line.
                DulukaAccountStore.Instance.ClearSession()
                Me.Hide()
                Base_Connect.ReturnFromSubPage()   ' gate forwards to Sign in
                Base_Connect_Signin.Note("Duluka Account deleted.")
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = If(password = "",
                    "Current password is required to delete this account.",
                    "Current password is incorrect.")
                DisarmDelete()
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
                DisarmDelete()
            End If
        Catch ex As Exception
            ' NEVER silent: a transport or parsing failure must land in the
            ' status line, not vanish (and never escape an Async Sub).
            Debug.WriteLine($"BT_DeleteAccount error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = "Cannot reach Duluka — try again."
            DisarmDelete()
        Finally
            _busy = False
        End Try
    End Sub

    Private Sub DisarmDelete()
        _confirmDelete = False
        BT_DeleteAccount.Text = "Delete this account permanently"
    End Sub

    Private Sub TerminalSignOut(message As String)
        DulukaAccountStore.Instance.ClearSession()
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
        Base_Connect.NotifyFromSubPage(message)
    End Sub

    ''' <summary>String field of a §7.1 success `resource` object ("" if the
    ' field is absent or null — e.g. username on a provider-only account).</summary>
    Private Function ResourceText(resource As JsonNode, name As String) As String
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub
End Class
