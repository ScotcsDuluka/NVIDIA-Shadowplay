' [Main] Account home — the Duluka Account landing page.
' Identity model (product rule): the DULUKA ACCOUNT owns the identity;
' username/password is its NATIVE authentication method and a ProviderLink
' (GitHub in v0) is an EXTERNAL authentication method into the SAME account.
'   unauthenticated -> native sign-in form + "Continue with GitHub" +
'                      "Create Duluka Account"
'   authenticated   -> Duluka Account identity from GET /v1/account/me
'                      (never from GitHub UI state) + navigation + sign out.
' 401 / dead session is terminal: wipe the local session, re-render,
' exactly once per occurrence (contract §5.4).

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
    Private _signInBusy As Boolean

    Private Sub Base_Connect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        RenderState()
    End Sub

    ''' <summary>Re-renders on every show — the state may have changed while a
    ' sub-page (sign-in/devices/security/providers/create) was on top.</summary>
    Private Sub Base_Connect_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        RenderState()
        If DulukaAccountStore.Instance.HasSession Then
            RefreshAccountAsync()
        End If
    End Sub

    Private Sub RenderState()
        Dim authed As Boolean = DulukaAccountStore.Instance.HasSession

        ' Unauthenticated — the primary account experience: native sign-in,
        ' the provider alternative, and account creation. GitHub is presented
        ' as an authentication method, never as the account itself.
        Auth_PROMPT.Visible = Not authed
        Username_LBL.Visible = Not authed
        Username_BOX.Visible = Not authed
        Password_LBL.Visible = Not authed
        Password_BOX.Visible = Not authed
        BT_SignIn.Visible = Not authed
        Or_LBL.Visible = Not authed
        BT_Connect.Visible = Not authed
        Provider_NOTE.Visible = Not authed
        BT_CreateAccount.Visible = Not authed

        ' Authenticated — the Duluka Account identity card + navigation.
        Box_PNG.Visible = authed
        USERSNAME_TEXT.Visible = authed
        Account_META.Visible = authed
        BT_Devices.Visible = authed
        BT_Security.Visible = authed
        BT_Providers.Visible = authed
        BT_Logout.Visible = authed

        If authed Then
            Dim name As String = DulukaAccountStore.Instance.DisplayName
            USERSNAME_TEXT.Text = If(name <> "", name, "Duluka Account")
            Dim meta As String = ""
            If DulukaAccountStore.Instance.Username <> "" Then
                meta &= "Username  " & DulukaAccountStore.Instance.Username & Environment.NewLine
            End If
            meta &= "Account  " & If(DulukaAccountStore.Instance.AccountId <> "", DulukaAccountStore.Instance.AccountId, "—") &
                    Environment.NewLine &
                    "This device:  " & If(DulukaAccountStore.Instance.DeviceName <> "", DulukaAccountStore.Instance.DeviceName, "—")
            Account_META.Text = meta
        Else
            USERSNAME_TEXT.Text = ""
            Account_META.Text = ""
        End If
    End Sub

    ''' <summary>Server-truth refresh of the identity card. Keeps the cached
    ' profile on transient errors; a 401 is terminal and signs out locally.</summary>
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
                Dim accountId As String = ResourceText(r.Resource, "accountId")
                Dim deviceName As String = ""
                Dim deviceNode As JsonNode = r.Resource("currentDevice")
                If deviceNode IsNot Nothing Then
                    deviceName = NodeText(deviceNode, "deviceName")
                End If
                DulukaAccountStore.Instance.SetProfile(displayName, username)

                USERSNAME_TEXT.Text = If(displayName <> "", displayName, "Duluka Account")
                Dim meta As String = ""
                If username <> "" Then meta &= "Username  " & username & Environment.NewLine
                meta &= "Account  " & If(accountId <> "", accountId, "—") & Environment.NewLine &
                        "This device:  " & If(deviceName <> "", deviceName, "—")
                Account_META.Text = meta
                Status_TEXT.Text = ""
            ElseIf r.AuthDead Then
                DulukaAccountStore.Instance.ClearSession()
                RenderState()
                Status_TEXT.Text = "Your session has expired. Please sign in again."
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Catch ex As Exception
            Debug.WriteLine($"RefreshAccountAsync error: {ex.GetType().Name}")
        Finally
            _meInFlight = False
        End Try
    End Sub

    ' ── native sign-in ──────────────────────────────────────────────────────

    Private Async Sub BT_SignIn_Click(sender As Object, e As EventArgs) Handles BT_SignIn.Click
        NativeSignIn()
    End Sub

    Private Sub Password_Box_Enter(sender As Object, e As KeyEventArgs) Handles Password_BOX.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            NativeSignIn()
        End If
    End Sub

    Private Async Sub NativeSignIn()
        If _signInBusy Then Return
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance

        ' Client-side validation is a courtesy only — the server stays the
        ' authority. Both fields required before a network round trip.
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
                Password_BOX.Clear()
                RenderState()
                Status_TEXT.Text = ""
                RefreshAccountAsync()
            ElseIf r.HttpStatus = 401 AndAlso r.ErrorCode = "invalid_credentials" Then
                Status_TEXT.Text = "Incorrect username or password."
            ElseIf r.HttpStatus = 403 AndAlso r.ErrorCode = "perm.device_removed" Then
                ' This device's key is dead — drop it so the next attempt mints
                ' a fresh one, and tell the user honestly what happened.
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

    ' ── navigation ──────────────────────────────────────────────────────────

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub

    Private Sub BT_Connect_Click(sender As Object, e As EventArgs) Handles BT_Connect.Click
        OpenSubPage(Base_Connect_Login)
    End Sub

    Private Sub BT_CreateAccount_Click(sender As Object, e As EventArgs) Handles BT_CreateAccount.Click
        OpenSubPage(Base_Connect_Create)
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

    Friend Sub OpenSubPage(target As Form)
        Me.Hide()
        target.Show()
    End Sub

    ''' <summary>Sub-pages return here through this — VisibleChanged re-renders
    ' the correct state (sign-in entry or the identity card).</summary>
    Friend Sub ReturnFromSubPage()
        Me.Show()
    End Sub

    ''' <summary>Sub-pages surface one-line outcomes (terminal sign-outs etc.)
    ' on the home status line.</summary>
    Friend Sub NotifyFromSubPage(message As String)
        Status_TEXT.Text = message
    End Sub

    ' ── sign out ────────────────────────────────────────────────────────────

    Private Async Sub BT_Logout_Click(sender As Object, e As EventArgs) Handles BT_Logout.Click
        Dim store As DulukaAccountStore = DulukaAccountStore.Instance
        If Not store.HasSession Then
            RenderState()
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
        RenderState()
        Status_TEXT.Text = "Signed out."
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
