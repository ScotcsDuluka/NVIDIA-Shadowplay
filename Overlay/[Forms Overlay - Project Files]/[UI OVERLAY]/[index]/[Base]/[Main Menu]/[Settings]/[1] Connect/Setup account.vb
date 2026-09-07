' Setup account — FIRST-TIME setup for a GitHub-bootstrapped Duluka Account.
' Product rule: a Duluka Account created through a Linked Provider has NO
' native credential yet. This page picks the account's PERMANENT username and
' its first password. It reuses the server's provider-only branch of
' /v1/account/password (SetInitialPassword) — the ONLY credential mechanism;
' no second hashing or registration path exists client- or server-side.
' USERNAME IMMUTABILITY: once the username is set the server refuses any
' second adoption (credential_exists) and no UI can edit it — this page
' refuses to render for an account that already has a username, the Security
' page flips to change-password mode, and the username field never returns.
' Display Name and profile image remain independently editable elsewhere.
' Log discipline (C/2): username/password values are NEVER logged; only
' exception TYPE names reach the debug sink on failure paths.

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

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    ''' <summary>GATE: this page exists ONLY for a signed-in account that has
    ' no native username yet (the GitHub-bootstrap case). Anyone signed-in
    ' WITH a username (native Case A / already-set-up Case C) is sent straight
    ' back — the setup UI must never be reachable twice for one account.
    ' Anyone without a session goes back through Account home's gate to Sign
    ' in.</summary>
    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession OrElse store.Username <> "" Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        Auth_PROMPT.Text = "Set up your Duluka Account" & Environment.NewLine &
            "GitHub stays a linked sign-in method. Choose your permanent username and a password."
        Username_BOX.Clear()
        Password_BOX.Clear()
        Confirm_BOX.Clear()
        Status_TEXT.Text = ""
        Username_BOX.Focus()
    End Sub

    Private Async Sub BT_SetupAccount_Click(sender As Object, e As EventArgs) Handles BT_SetupAccount.Click
        If _busy Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance

        ' Courtesy validation (the server is the authority — same rules):
        '   username 3-32 chars, letters/digits/dot/underscore/hyphen
        '   password 8-128, confirm must match
        Dim username As String = Username_BOX.Text.Trim()
        Dim password As String = Password_BOX.Text
        Dim confirm As String = Confirm_BOX.Text
        If username = "" OrElse password = "" OrElse confirm = "" Then
            Status_TEXT.Text = "Choose a username and fill both password fields."
            Return
        End If
        If username.Length < 3 OrElse username.Length > 32 OrElse
           username.Any(Function(c) Not (Char.IsLetterOrDigit(c) OrElse c = "."c OrElse c = "_"c OrElse c = "-"c)) Then
            Status_TEXT.Text = "Username: 3-32 characters — letters, digits, dot, underscore, hyphen."
            Return
        End If
        If password.Length < 8 OrElse password.Length > 128 Then
            Status_TEXT.Text = "Password must be 8-128 characters."
            Return
        End If
        If password <> confirm Then
            Status_TEXT.Text = "Passwords do not match."
            Return
        End If

        _busy = True
        BT_SetupAccount.Enabled = False
        Status_TEXT.Text = "Setting up your account…"
        Try
            ' Provider-only branch of /v1/account/password: username + new
            ' password, no currentPassword (none exists yet). The server
            ' hashes once, enforces username uniqueness and immutability.
            Dim body As New JsonObject()
            body("username") = username
            body("newPassword") = password
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync(
                "/v1/account/password", store.SessionToken, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                ' Cache the permanent username — the server echoes it; fall
                ' back to what was typed (same value the server canonicalized).
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
                Status_TEXT.Text = "That username is already taken — pick another."
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "credential_exists" Then
                ' The account gained a credential behind our back (race or
                ' another device): setup is DONE — never a second adoption.
                Status_TEXT.Text = ""
                Me.Hide()
                Base_Connect.ReturnFromSubPage()
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_username" Then
                Status_TEXT.Text = "Username: 3-32 characters — letters, digits, dot, underscore, hyphen."
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_password" Then
                Status_TEXT.Text = "Password must be 8-128 characters."
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            Debug.WriteLine($"SetupAccount error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = "Cannot reach Duluka."
        Finally
            _busy = False
            If Not IsDisposed Then BT_SetupAccount.Enabled = True
        End Try
    End Sub

    ''' <summary>Back is allowed — the session stays valid and the Security
    ' page keeps offering the same setup; Account home re-raises the nudge
    ' until the username exists.</summary>
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
