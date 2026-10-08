

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

    Private Shared Function L(key As String, ParamArray args() As String) As String
        Return LangHelper.GetText(key, args)
    End Function

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then
            _confirmAll = False
            BT_RevokeAll.Text = L("l10n.acctSecuritySignOutAll")
            _confirmDelete = False
            BT_DeleteAccount.Text = L("l10n.acctSecurityDeleteAccount")
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

        Dim lines As String = L("l10n.acctSecurityCurrentSession") & Environment.NewLine &
                              L("l10n.acctMetaDevice", If(store.DeviceName <> "", store.DeviceName, "—")) & Environment.NewLine &
                              L("l10n.acctMetaExpires", If(store.SessionExpiresAtText <> "", store.SessionExpiresAtText, "—"))
        Info_META.Text = lines
        Status_TEXT.Text = ""
    End Sub

    Private Async Sub BT_RefreshSession_Click(sender As Object, e As EventArgs) Handles BT_RefreshSession.Click
        If _busy Then Return
        _busy = True
        Try
            Status_TEXT.Text = L("l10n.acctSecurityExtending")
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/auth/session/refresh", token, "{}").ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                Dim expires As JsonNode = r.Resource("sessionExpiresAt")
                If expires IsNot Nothing Then
                    DulukaAccountStore.Instance.SetSessionExpiry(expires.GetValue(Of String)())
                End If
                RenderSessionInfo()
                Status_TEXT.Text = L("l10n.acctSecurityExtended")
            ElseIf r.AuthDead Then
                TerminalSignOut(L("l10n.acctSecuritySessionExpired"))
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Finally
            _busy = False
        End Try
    End Sub

    Private Async Sub BT_RevokeAll_Click(sender As Object, e As EventArgs) Handles BT_RevokeAll.Click
        If _busy Then Return

        If Not _confirmAll Then
            _confirmAll = True
            BT_RevokeAll.Text = L("l10n.acctSecurityReallySignOutAll")
            Status_TEXT.Text = L("l10n.acctSecuritySignOutAllWarn")
            Return
        End If

        _busy = True
        Try
            Status_TEXT.Text = L("l10n.acctSecuritySigningOutAll")
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/auth/sessions/revoke-all", token, "{}").ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                Dim countText As String = L("l10n.acctSecurityAllSignedOut")
                Dim countNode As JsonNode = If(r.Resource IsNot Nothing, r.Resource("revokedSessions"), Nothing)
                If countNode IsNot Nothing Then
                    countText = L("l10n.acctSecuritySignedOutCount", countNode.GetValue(Of Integer)().ToString())
                End If
                TerminalSignOut(countText)
            ElseIf r.AuthDead Then
                
                TerminalSignOut(L("l10n.acctSecuritySessionGone"))
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
                _confirmAll = False
                BT_RevokeAll.Text = L("l10n.acctSecuritySignOutAll")
            End If
        Finally
            _busy = False
        End Try
    End Sub

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
                Status_TEXT.Text = L("l10n.acctSetupChooseAndFill")
                Return
            End If
            If username.Length < 3 OrElse username.Length > 32 OrElse
               username.Any(Function(c) Not (Char.IsLetterOrDigit(c) OrElse c = "."c OrElse c = "_"c OrElse c = "-"c)) Then
                Status_TEXT.Text = L("l10n.acctUsernameRule")
                Return
            End If
        Else
            If current = "" OrElse newPw = "" OrElse confirm = "" Then
                Status_TEXT.Text = L("l10n.acctSecurityFillPasswords")
                Return
            End If
        End If
        If newPw.Length < 8 OrElse newPw.Length > 128 Then
            Status_TEXT.Text = L("l10n.acctSecurityNewPasswordRule")
            Return
        End If
        If newPw <> confirm Then
            Status_TEXT.Text = L("l10n.acctSecurityNewNoMatch")
            Return
        End If

        _busy = True
        Try
            Status_TEXT.Text = If(providerOnly, L("l10n.acctSecurityAddingPassword"), L("l10n.acctSecurityUpdatingPassword"))
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

                If providerOnly Then DulukaAccountStore.Instance.SetProfile(
                    DulukaAccountStore.Instance.DisplayName, username)
                SetupPasswordForm()
                Status_TEXT.Text = If(providerOnly,
                    L("l10n.acctSecurityAccountSetUp"),
                    L("l10n.acctSecurityPasswordUpdated"))
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = L("l10n.acctSecurityCurrentIncorrect")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_username" Then
                Status_TEXT.Text = L("l10n.acctUsernameRule")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_password" Then
                Status_TEXT.Text = L("l10n.acctPasswordRule")
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "conflict.username_taken" Then

                Dim meR As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/me", token).ConfigureAwait(True)
                If Not IsDisposed AndAlso meR.Ok AndAlso meR.Resource IsNot Nothing Then
                    Dim meUser As String = ResourceText(meR.Resource, "username")
                    If meUser <> "" Then
                        DulukaAccountStore.Instance.SetProfile(ResourceText(meR.Resource, "displayName"), meUser)
                        SetupPasswordForm()
                        Status_TEXT.Text = L("l10n.acctSecurityAlreadyHasPassword")
                        Return
                    End If
                End If
                If Not IsDisposed Then Status_TEXT.Text = L("l10n.acctUsernameTaken")
            ElseIf r.AuthDead Then
                TerminalSignOut(L("l10n.acctSessionExpired"))
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception

            Debug.WriteLine($"BT_ChangePassword error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = L("l10n.acctCannotReachRetry")
        Finally
            _busy = False
        End Try
    End Sub

    Private Sub SetupPasswordForm()
        Dim providerOnly As Boolean = ProviderOnlyAccount
        PwUsername_LBL.Visible = providerOnly
        PwUsername_BOX.Visible = providerOnly
        PwCurrent_LBL.Text = If(providerOnly, L("l10n.acctSecurityCurrentNotSet"), L("l10n.acctSecurityCurrentPassword"))
        PwCurrent_LBL.Enabled = Not providerOnly
        PwCurrent_BOX.Enabled = Not providerOnly
        PwHeader_LBL.Text = If(providerOnly, L("l10n.acctSecurityAddPasswordHeader"), L("l10n.acctSecurityChangePassword"))

        BT_ChangePassword.Text = If(providerOnly, L("l10n.acctSetUpAccount"), L("l10n.acctSecurityChangePassword"))

        Dim hasPassword As Boolean = Not providerOnly
        DzPassword_LBL.Visible = hasPassword
        DzPassword_BOX.Visible = hasPassword
        '  LayoutPasswordRows()
    End Sub

    Private Sub LayoutPasswordRows()

    End Sub

    Private Async Sub BT_DeleteAccount_Click(sender As Object, e As EventArgs) Handles BT_DeleteAccount.Click
        If _busy Then Return

        If Not _confirmDelete Then
            _confirmDelete = True
            BT_DeleteAccount.Text = L("l10n.acctSecurityReallyDelete")
            Status_TEXT.Text = L("l10n.acctSecurityDeleteMeta")
            Return
        End If

        Dim hasPassword As Boolean = Not ProviderOnlyAccount
        Dim password As String = DzPassword_BOX.Text
        If hasPassword AndAlso password = "" Then
            Status_TEXT.Text = L("l10n.acctSecurityTypeToDelete")
            Return
        End If

        _busy = True
        Try
            Status_TEXT.Text = L("l10n.acctSecurityDeleting")
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

                DulukaAccountStore.Instance.ClearSession()
                Me.Hide()
                Base_Connect.ReturnFromSubPage()   
                Base_Connect_Signin.Note(L("l10n.acctAccountDeleted"))
            ElseIf r.AuthDead Then
                TerminalSignOut(L("l10n.acctSessionExpired"))
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = If(password = "",
                    L("l10n.acctSecurityPasswordRequiredToDelete"),
                    L("l10n.acctSecurityCurrentIncorrect"))
                DisarmDelete()
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
                DisarmDelete()
            End If
        Catch ex As Exception

            Debug.WriteLine($"BT_DeleteAccount error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = L("l10n.acctCannotReachRetry")
            DisarmDelete()
        Finally
            _busy = False
        End Try
    End Sub

    Private Sub DisarmDelete()
        _confirmDelete = False
        BT_DeleteAccount.Text = L("l10n.acctSecurityDeleteAccount")
    End Sub

    Private Sub TerminalSignOut(message As String)
        DulukaAccountStore.Instance.ClearSession()
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
        Base_Connect.NotifyFromSubPage(message)
    End Sub

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
