

Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Profile
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

    Private _pendingImage As String
    Private _saveInFlight As Boolean

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        If Not DulukaAccountStore.Instance.HasSession Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        LoadCurrent()
    End Sub

    Private Sub LoadCurrent()
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        Name_BOX.Text = store.DisplayName
        If store.Username <> "" Then
            Username_VALUE.Text = store.Username
        Else
            Username_VALUE.Text = "— (this account signs in with a provider)"
        End If
        _pendingImage = store.ProfileImage
        DulukaAvatar.SetPreview(Avatar_PICTURE, _pendingImage, AvatarLetter_LABEL)
        Status_TEXT.Text = ""
    End Sub

    Private Sub BT_ChangeImage_Click(sender As Object, e As EventArgs) Handles BT_ChangeImage.Click
        Using picker As New OpenFileDialog
            picker.Title = "Choose a profile image"

            picker.Filter = "PNG or JPEG images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            picker.CheckFileExists = True
            If picker.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim reason = ""
            Dim dataUrl = EncodeFromFile(picker.FileName, reason)
            If dataUrl Is Nothing Then
                Status_TEXT.Text = reason
                Return
            End If
            _pendingImage = dataUrl
            SetPreview(Avatar_PICTURE, _pendingImage, AvatarLetter_LABEL)
            Status_TEXT.Text = ""
        End Using
    End Sub

    Private Sub BT_RemoveImage_Click(sender As Object, e As EventArgs) Handles BT_RemoveImage.Click
        _pendingImage = ""
        SetPreview(Avatar_PICTURE, "", AvatarLetter_LABEL)
        Status_TEXT.Text = ""
    End Sub

    Private Async Sub BT_Save_Click(sender As Object, e As EventArgs) Handles BT_Save.Click
        If _saveInFlight Then Return
        Dim store = DulukaAccountStore.Instance
        If Not store.HasSession Then
            Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If

        Dim displayName = Name_BOX.Text.Trim
        If displayName.Length > 64 Then
            Status_TEXT.Text = "Display name is too long (max 64 characters)."
            Return
        End If

        _saveInFlight = True
        BT_Save.Enabled = False
        Try
            Status_TEXT.Text = "Saving…"
            Dim body As New JsonObject
            body("displayName") = If(displayName <> "", displayName, Nothing)
            body("profileImage") = If(_pendingImage <> "", _pendingImage, Nothing)

            Dim token = store.SessionToken
            Dim r = Await PutAsync(
                "/v1/account/profile", token, body.ToJsonString).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                store.SetProfileWithImage(ResourceText(r.Resource, "displayName"),
                                          ResourceText(r.Resource, "username"),
                                          ResourceText(r.Resource, "profileImage"))
                Hide()
                Base_Connect.ReturnFromSubPage()
                Base_Connect.NotifyFromSubPage("Profile updated.")
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            Else
                Status_TEXT.Text = HumanError(r)
            End If
        Finally
            _saveInFlight = False
            If Not IsDisposed Then BT_Save.Enabled = True
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

    Private Function ResourceText(resource As JsonNode, name As String) As String
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function

End Class
