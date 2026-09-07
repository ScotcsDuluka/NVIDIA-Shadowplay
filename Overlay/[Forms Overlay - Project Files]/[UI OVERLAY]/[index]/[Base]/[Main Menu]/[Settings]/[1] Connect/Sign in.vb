







Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Signin
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

    Private _signInBusy As Boolean

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    
    
    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        If DulukaAccountStore.Instance.HasSession Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        Username_BOX.Focus()
    End Sub

    

    Private Sub BT_SignIn_Click(sender As Object, e As EventArgs) Handles BT_SignIn.Click
        NativeSignIn()
    End Sub

    Private Sub Password_BOX_KeyDown(sender As Object, e As KeyEventArgs) Handles Password_BOX.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            NativeSignIn()
        End If
    End Sub

    Private Async Sub NativeSignIn()
        If _signInBusy Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance

        
        
        Dim username As String = Username_BOX.Text.Trim()
        Dim password As String = Password_BOX.Text
        If username = "" OrElse password = "" Then
            Status_TEXT.Text = "Enter your username and password."
            Return
        End If

        _signInBusy = True
        BT_SignIn.Enabled = False
        BT_Connect.Enabled = False
        BT_CreateAccount.Enabled = False
        Status_TEXT.Text = "Signing in…"
        Try
            Dim body As New JsonObject()
            body("username") = username
            body("password") = password
            body("deviceKey") = store.EnsureDeviceKey()
            body("deviceName") = DulukaApi.DeviceName()
            Dim r As DulukaApi.Result = Await DulukaApi.PostAsync("/v1/auth/login", Nothing, body.ToJsonString()).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                store.SetSession(ResourceText(r.Resource, "sessionToken"),
                                 ResourceText(r.Resource, "accountId"),
                                 ResourceText(r.Resource, "deviceId"),
                                 ResourceText(r.Resource, "sessionExpiresAt"),
                                 DulukaApi.DeviceName())
                
                
                
                store.SetProfile(store.DisplayName,
                                 ResourceText(r.Resource, "username"))
                Password_BOX.Clear()
                Status_TEXT.Text = ""
                Me.Hide()
                Base_Connect.ReturnFromSubPage()
            ElseIf r.HttpStatus = 401 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = "Incorrect username or password."
            ElseIf r.HttpStatus = 403 AndAlso r.ErrorCode = "perm.device_removed" Then
                
                
                store.RevokeDeviceKey()
                Status_TEXT.Text = "This device was revoked by your account. Try signing in again."
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            Debug.WriteLine($"NativeSignIn error: {ex.GetType().Name}")
            If Not IsDisposed Then Status_TEXT.Text = "Cannot reach Duluka."
        Finally
            _signInBusy = False
            If Not IsDisposed Then
                BT_SignIn.Enabled = True
                BT_Connect.Enabled = True
                BT_CreateAccount.Enabled = True
            End If
        End Try
    End Sub

    

    Private Sub BT_Connect_Click(sender As Object, e As EventArgs) Handles BT_Connect.Click
        Me.Hide()
        Base_Connect_Login.Show()
    End Sub

    Private Sub BT_CreateAccount_Click(sender As Object, e As EventArgs) Handles BT_CreateAccount.Click
        Me.Hide()
        Base_Connect_Create.Show()
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub

    
    
    Friend Sub Note(message As String)
        Status_TEXT.Text = message
    End Sub

    Private Function ResourceText(resource As JsonNode, name As String) As String
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function

End Class
