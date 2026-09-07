' [Main] Account home — the Duluka Account landing page (SIGNED-IN ONLY).
' Identity model (product rule): the DULUKA ACCOUNT owns the identity;
' username/password is its NATIVE authentication method and a ProviderLink
' (GitHub in v0) is an EXTERNAL authentication method into the SAME account.
' Anyone without a Duluka session who lands here is forwarded to the
' Sign in page. Identity comes from GET /v1/account/me — never from GitHub.
' 401 / dead session is terminal: wipe the local session, forward to
' Sign in, exactly once per occurrence (contract §5.4).

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

    Private Sub Base_Connect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        LayoutRow()
    End Sub

    ''' <summary>The action row divides the panel content width (62px margins)
    ' into FOUR equal buttons (Devices / Security / Providers / Edit Profile)
    ' with 16px gaps — recomputed on every resize so the row tracks the panel
    ' width exactly (anchors cannot make quarters). At the design width (panel
    ' 1760) this computes the Designer values.</summary>
    Private Sub LayoutRow()
        Dim contentW As Integer = Settings_Panel.Width - 124
        If contentW <= 0 Then Return
        Dim gap As Integer = 16
        Dim bw As Integer = (contentW - 3 * gap) \ 4
        If bw < 180 Then bw = 180 ' below this the captions clip
        BT_Devices.Width = bw
        BT_Security.Location = New Point(62 + bw + gap, BT_Security.Top)
        BT_Security.Width = bw
        BT_Providers.Location = New Point(62 + 2 * (bw + gap), BT_Providers.Top)
        BT_Providers.Width = bw
        BT_EditProfile.Location = New Point(62 + 3 * (bw + gap), BT_EditProfile.Top)
        BT_EditProfile.Width = bw
    End Sub

    Private Sub Settings_Panel_Resize(sender As Object, e As EventArgs) Handles Settings_Panel.Resize
        LayoutRow()
    End Sub

    ''' <summary>GATE: this page exists ONLY for signed-in users — anyone
    ' without a Duluka session is forwarded to the Sign in page, and a
    ' signed-in account whose native credential is STILL MISSING (GitHub
    ' bootstrap, no username/password yet) is forwarded to the first-time
    ' Account Setup page. Product rule: setup is one-time; once the username
    ' exists this page renders normally forever after.</summary>
    Private Sub Base_Connect_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        If Not DulukaAccountStore.Instance.HasSession Then
            ForwardToSignIn()
            Return
        End If
        RenderState()
        If DulukaAccountStore.Instance.Username = "" Then
            ForwardToSetup()
            Return
        End If
        RefreshAccountAsync()
    End Sub

    ''' <summary>Paints the identity card from the cached store. The page is
    ' signed-in only (the VisibleChanged gate forwards everyone else), but the
    ' visibility of every block is still enforced here so the card can never
    ' render as a blank page again.</summary>
    Private Sub RenderState()
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then Return

        Dim name As String = store.DisplayName
        USERSNAME_TEXT.Text = If(name <> "", name, "Duluka Account")
        Avatar_BOX.Text = If(name <> "", name.Substring(0, 1).ToUpperInvariant(), "D")
        RenderAvatar(store.ProfileImage)

        Dim meta As String = ""
        If store.Username <> "" Then
            meta &= "Username  " & store.Username & Environment.NewLine
        Else
            meta &= "Username  (not set up yet)" & Environment.NewLine
        End If
        meta &= "Account  " & If(store.AccountId <> "", store.AccountId, "—") & Environment.NewLine &
                "This device  " & If(store.DeviceName <> "", store.DeviceName, "—")
        Account_META.Text = meta

        Dim expires As String = store.SessionExpiresAtText
        Session_META.Text = "Device  " & If(store.DeviceName <> "", store.DeviceName, "—") &
                            "        Expires  " & If(expires <> "", expires, "unknown")

        Card_PANEL.Visible = True
        Avatar_BOX.Visible = True
        USERSNAME_TEXT.Visible = True
        Account_META.Visible = True
        BT_Devices.Visible = True
        BT_Security.Visible = True
        BT_Providers.Visible = True
        BT_Logout.Visible = True
        Session_PANEL.Visible = True
        ' First-time setup nudge: a GitHub-bootstrapped account has no native
        ' username/password yet — keep the setup offer on screen until done.
        ' The Setup page itself re-gates (it never renders for an account that
        ' already has a username), so the nudge cannot overstay its welcome.
        Nudge_PANEL.Visible = (store.Username = "")
    End Sub

    ''' <summary>Server-truth refresh of the identity card. Keeps the cached
    ' profile on transient errors; a 401 is terminal and signs out locally.
    ' When the FORCED-SETUP gate is armed (right after a provider login) and
    ' /me reports no native username, the Setup page replaces this one —
    ' exactly once per login, so Back can return without a bounce loop.</summary>
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

    ' ── forced first-time setup gate ────────────────────────────────────────

    ''' <summary>Armed ONLY by the provider-login path (Login flow). A GitHub
    ' bootstrap lands on Account home first; when /me confirms the account is
    ' provider-only, the Setup page is force-opened (once per login — the
    ' user may defer with Back, the nudge stays until the username exists).</summary>
    Private _setupGateArmed As Boolean

    Friend Sub ArmForcedSetupGate()
        _setupGateArmed = True
    End Sub

    ' ── avatar rendering ──────────────────────────────────────────────────

    ''' <summary>Show the profile image when it decodes; the letter avatar is
    ' the fallback for "no image" AND for a corrupt value — a bad avatar can
    ' never blank the identity card. The previous bitmap is disposed so rapid
    ' re-renders do not accumulate GDI+ handles.</summary>
    Private Sub RenderAvatar(dataUrl As String)
        DulukaAvatar.SetPreview(Avatar_PICTURE, dataUrl, Avatar_BOX)
    End Sub

    ' ── navigation ──────────────────────────────────────────────────────────

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

    ''' <summary>Sub-pages return here through this — the gate either shows the
    ' identity card (session alive) or forwards to Sign in (no session).</summary>
    Friend Sub ReturnFromSubPage()
        Me.Show()
    End Sub

    ''' <summary>Sub-pages surface one-line outcomes (terminal sign-outs etc.);
    ' routed to whichever page ends up visible.</summary>
    Friend Sub NotifyFromSubPage(message As String)
        Status_TEXT.Text = message
    End Sub

    ''' <summary>The one forward path to the Sign in page — used by the gate,
    ' by 401 handling and after sign out.</summary>
    Friend Sub ForwardToSignIn()
        Me.Hide()
        Base_Connect_Signin.Settings_Panel.Location = New Point(80, 160)
        Base_Connect_Signin.Show()
        Base_Connect_Signin.Opacity = 1
    End Sub

    ''' <summary>The one forward path to the first-time Account Setup page —
    ' a signed-in account whose native credential is missing must choose its
    ' immutable username and password before Account Home is usable. The
    ' Setup page re-checks server truth itself, so an already-initialized
    ' account is never asked to set up twice.</summary>
    Friend Sub ForwardToSetup()
        Me.Hide()
        Base_Connect_Setup.Settings_Panel.Location = New Point(80, 160)
        Base_Connect_Setup.Show()
        Base_Connect_Setup.Opacity = 1
    End Sub

    ' ── sign out ────────────────────────────────────────────────────────────

    Private Async Sub BT_Logout_Click(sender As Object, e As EventArgs) Handles BT_Logout.Click
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then
            ForwardToSignIn()
            Return
        End If
        BT_Logout.Enabled = False
        Status_TEXT.Text = "Signing out…"
        Dim token As String = store.SessionToken
        If token <> "" Then
            ' Best effort — sign out must never strand the user on a dead session.
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
