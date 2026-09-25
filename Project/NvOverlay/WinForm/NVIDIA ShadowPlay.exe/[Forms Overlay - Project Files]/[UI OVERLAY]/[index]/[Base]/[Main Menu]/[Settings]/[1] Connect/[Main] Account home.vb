

Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect
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

    Private _meInFlight As Boolean

    Private Shared Function L(key As String, ParamArray args() As String) As String
        Return LangHelper.GetText(key, args)
    End Function

    Private Sub Base_Connect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        LayoutRow()
    End Sub

    Private Sub LayoutRow()
        Dim contentW As Integer = Settings_Panel.Width - 124
        If contentW <= 0 Then Return
        Dim gap As Integer = 16
        Dim bw As Integer = (contentW - 3 * gap) \ 4
        If bw < 180 Then bw = 180 
        BT_Devices.Width = bw
        BT_Security.Location = New Point(62 + bw + gap, BT_Security.Top)
        BT_Security.Width = bw
        BT_Providers.Location = New Point(62 + 2 * (bw + gap), BT_Providers.Top)
        BT_Providers.Width = bw
        BT_EditProfile.Location = New Point(62 + 3 * (bw + gap), BT_EditProfile.Top)
        BT_EditProfile.Width = bw
        LayoutNudge()
    End Sub

    ' nudge row re-derived in code like the action row — a designer
    ' re-serialise at another DPI once stranded BT_SetupNow at x=2823 on a
    ' 1636px panel (the FINISH SETTING UP CTA went invisible)
    Private Sub LayoutNudge()
        Dim m As Integer = 24
        BT_SetupNow.Location = New Point(Nudge_PANEL.Width - BT_SetupNow.Width - m, BT_SetupNow.Top)
        Nudge_META.Width = Math.Max(200, BT_SetupNow.Left - Nudge_META.Left - m)
    End Sub

    Private Sub Settings_Panel_Resize(sender As Object, e As EventArgs) Handles Settings_Panel.Resize
        LayoutRow()
    End Sub

    Private Sub Base_Connect_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        If Not DulukaAccountStore.Instance.HasSession Then
            ForwardToSignIn()
            Return
        End If
        RenderState()
        ' Setup is decided by SERVER truth, never by the local cache alone.
        ' After a GitHub (re)login the callback response carries no username,
        ' so the local value is "" even for a fully set-up account — forcing
        ' Setup here used to trap users in an endless loop (Back bounced
        ' straight back to Setup). RefreshAccountAsync fetches
        ' /v1/account/me, restores the real profile, and only forwards to
        ' Setup when the SERVER confirms the account has no username and the
        ' login flow's one-shot gate is armed. Until then the home card +
        ' the FINISH SETTING UP nudge stay visible and Back keeps working.
        RefreshAccountAsync()
    End Sub

    Private Sub RenderState()
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then Return

        Dim name As String = store.DisplayName
        USERSNAME_TEXT.Text = If(name <> "", name, L("l10n.acctTitle"))
        Avatar_BOX.Text = If(name <> "", name.Substring(0, 1).ToUpperInvariant(), "D")
        RenderAvatar(store.ProfileImage)

        Dim meta As String = ""
        If store.Username <> "" Then
            meta &= L("l10n.acctMetaUsername", store.Username) & Environment.NewLine
        Else
            meta &= L("l10n.acctMetaUsernameNotSet") & Environment.NewLine
        End If
        meta &= L("l10n.acctMetaAccount", If(store.AccountId <> "", store.AccountId, "—")) & Environment.NewLine &
                L("l10n.acctMetaThisDevice", If(store.DeviceName <> "", store.DeviceName, "—"))
        Account_META.Text = meta

        Dim expires As String = store.SessionExpiresAtText
        Session_META.Text = L("l10n.acctMetaDevice", If(store.DeviceName <> "", store.DeviceName, "—")) &
                            "        " & L("l10n.acctMetaExpires", If(expires <> "", expires, L("l10n.acctMetaExpiresUnknown")))

        Card_PANEL.Visible = True
        ' Avatar_BOX / Avatar_PICTURE visibility is owned EXCLUSIVELY by
        ' DulukaAvatar.SetPreview (via RenderAvatar above): image → picture
        ' visible + letter hidden, no image → the reverse. Forcing the letter
        ' label visible here re-showed it ON TOP of the loaded profile image
        ' (same 96×96 rect, label first in the panel's z-order), so the home
        ' card always looked like a letter avatar. Do not touch them here.
        USERSNAME_TEXT.Visible = True
        Account_META.Visible = True
        BT_Devices.Visible = True
        BT_Security.Visible = True
        BT_Providers.Visible = True
        BT_EditProfile.Visible = True
        BT_Logout.Visible = True
        Session_PANEL.Visible = True

        Nudge_PANEL.Visible = (store.Username = "")
    End Sub

    Private Async Sub RefreshAccountAsync()
        If _meInFlight Then Return
        If Not DulukaAccountStore.Instance.HasSession Then Return
        _meInFlight = True
        Try
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/me", token).ConfigureAwait(True)
            _meInFlight = False
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                Dim displayName As String = ResourceText(r.Resource, "displayName")
                Dim username As String = ResourceText(r.Resource, "username")
                Dim profileImage As String = ResourceText(r.Resource, "profileImage")
                DulukaAccountStore.Instance.SetProfileWithImage(displayName, username, profileImage)
                RenderState()
                Status_TEXT.Text = ""

                Dim gateArmed As Boolean = _setupGateArmed
                _setupGateArmed = False
                If gateArmed AndAlso username = "" Then
                    Me.Hide()
                    Base_Connect_Setup.Show()
                    Return
                End If
            ElseIf r.AuthDead Then
                DulukaAccountStore.Instance.ClearSession()
                ForwardToSignIn()
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            Debug.WriteLine($"RefreshAccountAsync error: {ex.GetType().Name}")
        Finally
            _meInFlight = False
        End Try
    End Sub

    Private _setupGateArmed As Boolean

    Friend Sub ArmForcedSetupGate()
        _setupGateArmed = True
    End Sub

    Private Sub RenderAvatar(dataUrl As String)
        DulukaAvatar.SetPreview(Avatar_PICTURE, dataUrl, Avatar_BOX)
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub

    Private Sub BT_Devices_Click(sender As Object, e As EventArgs) Handles BT_Devices.Click
        OpenSubPage(Base_Connect_Devices)
    End Sub

    Private Sub BT_Security_Click(sender As Object, e As EventArgs) Handles BT_Security.Click
        OpenSubPage(Base_Connect_Security)
    End Sub

    Private Sub BT_Providers_Click(sender As Object, e As EventArgs) Handles BT_Providers.Click
        OpenSubPage(Base_Connect_Providers)
    End Sub

    Private Sub BT_SetupNow_Click(sender As Object, e As EventArgs) Handles BT_SetupNow.Click
        OpenSubPage(Base_Connect_Setup)
    End Sub

    Private Sub BT_EditProfile_Click(sender As Object, e As EventArgs) Handles BT_EditProfile.Click
        OpenSubPage(Base_Connect_Profile)
    End Sub

    Friend Sub OpenSubPage(target As Form)
        Me.Hide()
        target.Show()
    End Sub

    Friend Sub ReturnFromSubPage()
        Me.Show()
    End Sub

    Friend Sub NotifyFromSubPage(message As String)
        Status_TEXT.Text = message
    End Sub

    Friend Sub ForwardToSignIn()
        Me.Hide()
        Base_Connect_Signin.Settings_Panel.Location = New Point(80, 160)
        Base_Connect_Signin.Show()
        Base_Connect_Signin.Opacity = 1
    End Sub

    Friend Sub ForwardToSetup()
        Me.Hide()
        Base_Connect_Setup.Settings_Panel.Location = New Point(80, 160)
        Base_Connect_Setup.Show()
        Base_Connect_Setup.Opacity = 1
    End Sub

    Private Async Sub BT_Logout_Click(sender As Object, e As EventArgs) Handles BT_Logout.Click
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then
            ForwardToSignIn()
            Return
        End If
        BT_Logout.Enabled = False
        Status_TEXT.Text = L("l10n.acctHomeSigningOut")
        Dim token As String = store.SessionToken
        If token <> "" Then
            
            Await DulukaApi.PostAsync("/v1/auth/session/revoke", token, "{}").ConfigureAwait(True)
        End If
        store.ClearSession()
        BT_Logout.Enabled = True
        ForwardToSignIn()
    End Sub

    Private Function ResourceText(resource As JsonNode, name As String) As String
        Dim node As JsonNode = resource(name)
        If node Is Nothing Then Return ""
        Return node.GetValue(Of String)()
    End Function

    Private Function NodeText(node As JsonNode, name As String) As String
        Dim value As JsonNode = node(name)
        If value Is Nothing Then Return ""
        Return value.GetValue(Of String)()
    End Function

End Class
