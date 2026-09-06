' Sessions & Security — the current session at a glance, plus the two
' destructive actions: extend the session (refresh) and sign out on ALL
' devices (revoke-all, two-step confirm — it signs out every signed-in
' device, including this one). 401 anywhere is the terminal path.

Imports System.Diagnostics
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

    Private Async Sub BT_ChangePassword_Click(sender As Object, e As EventArgs) Handles BT_ChangePassword.Click
        If _busy Then Return
        Dim current As String = PwCurrent_BOX.Text
        Dim newPw As String = PwNew_BOX.Text
        Dim confirm As String = PwConfirm_BOX.Text

        If current = "" OrElse newPw = "" OrElse confirm = "" Then
            Status_TEXT.Text = "Fill in every password field."
            Return
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
            Status_TEXT.Text = "Updating password…"
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim body As New System.Text.Json.Nodes.JsonObject()
            body("currentPassword") = current
            body("newPassword") = newPw
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync(
                "/v1/account/password", token, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                PwCurrent_BOX.Clear()
                PwNew_BOX.Clear()
                PwConfirm_BOX.Clear()
                Status_TEXT.Text = "Password updated."
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = "Current password is incorrect."
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "native_credential_absent" Then
                Status_TEXT.Text = "This account signs in with a linked provider only."
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Finally
            _busy = False
        End Try
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
