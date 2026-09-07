' Account setup — FIRST-TIME Duluka Account setup for provider-bootstrapped
' accounts. GitHub OAuth creates the Duluka Account (the account is the
' PRIMARY identity; GitHub is only a linked provider / authentication
' bootstrap), but such an account has NO native credential yet: no username,
' no password. This page forces that one-time choice:
'
'   OAuth success → load Duluka Account → check native credential state
'     → not initialized → THIS page (username + password + confirm)
'     → initialized    → straight to Account Home
'
' The username becomes IMMUTABLE on success (server: NativeCredential is
' 1:1 per account, uniqueness DB-enforced, no rename endpoint exists) —
' this page therefore never offers a rename path either. The Display Name
' stays an independent, provider-owned field.
' The password lives only in this form and the local-loop request body —
' never stored or logged anywhere.

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
    Private _truthCheckInFlight As Boolean

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    ''' <summary>GATE: this page exists ONLY for a signed-in account whose
    ' native credential is still missing. Anyone initialized goes straight to
    ' Account Home; anyone without a session goes to Sign in. A server-truth
    ' check catches a stale local store so an ALREADY-initialized account is
    ' never asked to set up twice.</summary>
    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then
            Me.Hide()
            Base_Connect.ForwardToSignIn()
            Return
        End If
        If store.Username <> "" Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        Username_BOX.Clear()
        Password_BOX.Clear()
        Confirm_BOX.Clear()
        Status_TEXT.Text = ""
        Username_BOX.Focus()
        ServerTruthCheckAsync()
    End Sub

    ''' <summary>Server-truth pre-check (GET /v1/account/me): a non-empty
    ' username means the native credential ALREADY exists (e.g. setup was
    ' completed from another device, or the local store was wiped) — dismiss
    ' this page to Account Home instead of asking for setup again.</summary>
    Private Async Sub ServerTruthCheckAsync()
        If _truthCheckInFlight Then Return
        _truthCheckInFlight = True
        Try
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/me", token).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                Dim username As String = ResourceText(r.Resource, "username")
                If username <> "" AndAlso Visible Then
                    DulukaAccountStore.Instance.SetProfile(ResourceText(r.Resource, "displayName"), username)
                    Status_TEXT.Text = ""
                    Me.Hide()
                    Base_Connect.ReturnFromSubPage()
                End If
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            End If
            ' Transient failures keep the page up: the submit path reports
            ' every error honestly, nothing here may fail silently.
        Catch ex As Exception
            Debug.WriteLine($"Setup truth check error: {ex.GetType().Name}")
        Finally
            _truthCheckInFlight = False
        End Try
    End Sub

    Private Async Sub BT_Setup_Click(sender As Object, e As EventArgs) Handles BT_Setup.Click
        If _busy Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance

        ' Courtesy validation (server is the authority):
        '   username 3-32 chars, letters/digits/dot/underscore/hyphen
        '   password 8-128, confirm must match
        Dim username As String = Username_BOX.Text.Trim()
        Dim password As String = Password_BOX.Text
        Dim confirm As String = Confirm_BOX.Text
        If username = "" OrElse password = "" OrElse confirm = "" Then
            Status_TEXT.Text = "Fill in every field."
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
        BT_Setup.Enabled = False
        Status_TEXT.Text = "Setting up your account…"
        Try
            ' The EXISTING first-time credential mechanism is REUSED —
            ' POST /v1/account/password with username+newPassword on a
            ' provider-only account. No second credential API anywhere.
            Dim body As New JsonObject()
            body("username") = username
            body("newPassword") = password
            Dim token As String = store.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/account/password", token, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok Then
                ' Username is now SET (immutable). Session stays valid; no
                ' duplicate account is created — same accountId, same store.
                store.SetProfile(store.DisplayName, username)
                Username_BOX.Clear()
                Password_BOX.Clear()
                Confirm_BOX.Clear()
                Status_TEXT.Text = "Account ready."
                Me.Hide()
                Base_Connect.ReturnFromSubPage()
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "conflict.username_taken" Then
                ' Same wire code covers "username taken" AND "this account
                ' already has a password" (initialized elsewhere). Server
                ' truth decides which one this was.
                Dim meR As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/me", token).ConfigureAwait(True)
                If IsDisposed OrElse Not IsHandleCreated Then Return
                If meR.Ok AndAlso meR.Resource IsNot Nothing Then
                    Dim meUser As String = ResourceText(meR.Resource, "username")
                    If meUser <> "" Then
                        DulukaAccountStore.Instance.SetProfile(ResourceText(meR.Resource, "displayName"), meUser)
                        Status_TEXT.Text = "This account is already set up."
                        Me.Hide()
                        Base_Connect.ReturnFromSubPage()
                        Return
                    End If
                End If
                If Not IsDisposed Then Status_TEXT.Text = "That username is already taken — pick another."
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
            ' NEVER silent: every failure lands in the status line and the
            ' button comes back (no unhandled exception, no dead UI).
            Debug.WriteLine($"AccountSetup error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = "Cannot reach Duluka."
        Finally
            _busy = False
            If Not IsDisposed Then BT_Setup.Enabled = True
        End Try
    End Sub

    ''' <summary>Setup is REQUIRED while the native credential is missing —
    ' Back cannot bounce the user into the Account Home gate (that would
    ' loop back here). The hint says so honestly; after setup the page
    ' never reappears anyway.</summary>
    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        If DulukaAccountStore.Instance.Username = "" Then
            Status_TEXT.Text = "Choose your username and password to finish setting up your account."
            Return
        End If
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub

    Private Sub TerminalSignOut(message As String)
        DulukaAccountStore.Instance.ClearSession()
        Me.Hide()
        Base_Connect.ForwardToSignIn()
        Base_Connect_Signin.Note(message)
    End Sub

    Private Function ResourceText(resource As JsonNode, name As String) As String
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function
End Class
