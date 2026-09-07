' Create account — native Duluka Account registration.
' Client validation is a courtesy; the server stays the authority. A
' successful registration creates the account + credential + device and
' lands the user straight into a Duluka Account Session (same shape as
' login). The password never leaves this form except inside the TLS-free
' local-loop request body — it is never stored or logged anywhere.

Imports System.Diagnostics
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Create
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

    Private _creating As Boolean

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then
            Username_BOX.Clear()
            Password_BOX.Clear()
            Confirm_BOX.Clear()
            Status_TEXT.Text = ""
            If DulukaAccountStore.Instance.HasSession Then
                ' Already signed in — there is nothing to create here.
                Me.Hide()
                Base_Connect.ReturnFromSubPage()
            End If
        End If
    End Sub

    Private Async Sub BT_Create_Click(sender As Object, e As EventArgs) Handles BT_Create.Click
        If _creating Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance

        ' Courtesy validation (server is the authority):
        '   username 3-32 chars, letters/digits/dot/underscore/hyphen
        '   password ≥ 8, confirm must match
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

        _creating = True
        BT_Create.Enabled = False
        Status_TEXT.Text = "Creating your account…"
        Try
            Dim body As New JsonObject()
            body("username") = username
            body("password") = password
            body("deviceKey") = store.EnsureDeviceKey()
            body("deviceName") = DulukaApi.DeviceName()
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/auth/register", Nothing, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                store.SetSession(ResourceText(r.Resource, "sessionToken"),
                                 ResourceText(r.Resource, "accountId"),
                                 ResourceText(r.Resource, "deviceId"),
                                 ResourceText(r.Resource, "sessionExpiresAt"),
                                 DulukaApi.DeviceName())
                ' The native credential exists NOW — cache the username
                ' immediately so the Account Home first-time-setup gate can
                ' never misfire for a native account before /me returns.
                store.SetProfile(ResourceText(r.Resource, "username"),
                                 ResourceText(r.Resource, "username"))
                Status_TEXT.Text = ""
                Me.Hide()
                Base_Connect.ReturnFromSubPage()
            ElseIf r.HttpStatus = 409 AndAlso r.ErrorCode = "conflict.username_taken" Then
                Status_TEXT.Text = "That username is already taken — pick another."
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_username" Then
                Status_TEXT.Text = "Username: 3-32 characters — letters, digits, dot, underscore, hyphen."
            ElseIf r.HttpStatus = 400 AndAlso r.ErrorCode = "invalid_password" Then
                Status_TEXT.Text = "Password must be 8-128 characters."
            ElseIf r.HttpStatus = 403 AndAlso r.ErrorCode = "perm.device_removed" Then
                store.RevokeDeviceKey()
                Status_TEXT.Text = "This device was revoked. Try again to mint a fresh device key."
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            Debug.WriteLine($"CreateAccount error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = "Cannot reach Duluka."
        Finally
            _creating = False
            If Not IsDisposed Then BT_Create.Enabled = True
        End Try
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub

    Private Function ResourceText(resource As JsonNode, name As String) As String
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function
End Class
