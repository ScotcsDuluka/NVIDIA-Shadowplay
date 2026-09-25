Imports System.Diagnostics
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Setup
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

    Private _busy As Boolean

    Private Shared Function L(key As String, ParamArray args() As String) As String
        Return LangHelper.GetText(key, args)
    End Function

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession OrElse store.Username <> "" Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        Auth_PROMPT.Text = L("l10n.acctSetupPrompt") & Environment.NewLine &
            L("l10n.acctSetupPromptDetail")
        Username_BOX.Clear()
        Password_BOX.Clear()
        Confirm_BOX.Clear()
        Status_TEXT.Text = ""
        Username_BOX.Focus()
    End Sub

    Private Async Sub BT_SetupAccount_Click(sender As Object, e As EventArgs) Handles BT_SetupAccount.Click
        If _busy Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance

        Dim username As String = Username_BOX.Text.Trim()
        Dim password As String = Password_BOX.Text
        Dim confirm As String = Confirm_BOX.Text
        If username = "" OrElse password = "" OrElse confirm = "" Then
            Status_TEXT.Text = L("l10n.acctSetupChooseAndFill")
            Return
        End If
        If username.Length < 3 OrElse username.Length > 32 OrElse
           username.Any(Function(c) Not (Char.IsLetterOrDigit(c) OrElse c = "."c OrElse c = "_"c OrElse c = "-"c)) Then
            Status_TEXT.Text = L("l10n.acctUsernameRule")
            Return
        End If
        If password.Length < 8 OrElse password.Length > 128 Then
            Status_TEXT.Text = L("l10n.acctPasswordRule")
            Return
        End If
        If password <> confirm Then
            Status_TEXT.Text = L("l10n.acctPasswordsNoMatch")
            Return
        End If

        _busy = True
        BT_SetupAccount.Enabled = False
        Status_TEXT.Text = L("l10n.acctSetupBusy")
        Try
            Dim body As New JsonObject()
            body("username") = username
            body("newPassword") = password
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync(
                "/v1/account/password", store.SessionToken, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                Dim chosen As String = ResourceText(r.Resource, "username")
                If chosen = "" Then chosen = username
                store.SetProfile(store.DisplayName, chosen)
                Username_BOX.Clear()
                Password_BOX.Clear()
                Confirm_BOX.Clear()
                Status_TEXT.Text = ""
                Me.Hide()
                Base_Connect.ReturnFromSubPage()
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "conflict.username_taken" Then
                Status_TEXT.Text = L("l10n.acctUsernameTaken")
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "credential_exists" Then
                ' The account ALREADY has a password (set up earlier on this
                ' or another device). Never silently return home here — the
                ' home screen would bounce straight back to Setup and the
                ' Back button would look broken. Tell the user instead.
                Status_TEXT.Text = L("l10n.acctSetupAlreadyPassword")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_credentials" Then
                ' The server's change-password branch answering "current
                ' password is required" means the same thing: setup is done.
                Status_TEXT.Text = L("l10n.acctSetupAlreadyPassword")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_username" Then
                Status_TEXT.Text = L("l10n.acctUsernameRule")
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_password" Then
                Status_TEXT.Text = L("l10n.acctPasswordRule")
            ElseIf r.AuthDead Then
                TerminalSignOut(L("l10n.acctSessionExpired"))
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            Debug.WriteLine($"SetupAccount error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = L("l10n.acctCannotReach")
        Finally
            _busy = False
            If Not IsDisposed Then BT_SetupAccount.Enabled = True
        End Try
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub

    Private Sub TerminalSignOut(message As String)
        DulukaAccountStore.Instance.ClearSession()
        Me.Hide()
        Base_Connect.ForwardToSignIn()
        Base_Connect.NotifyFromSubPage(message)
    End Sub

    Private Function ResourceText(resource As JsonNode, name As String) As String
        If resource Is Nothing Then Return ""
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function

End Class
