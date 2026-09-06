' [Main] Account home — the Duluka Account landing page.
' Identity model (product rule): the DULUKA ACCOUNT owns the identity; a
' ProviderLink (GitHub in v0) is only an authentication method into it.
'   unauthenticated -> sign-in entry ("Continue with GitHub")
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

    Private Sub Base_Connect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        RenderState()
    End Sub

    ''' <summary>Re-renders on every show — the state may have changed while a
    ' sub-page (sign-in/devices/security/providers) was on top.</summary>
    Private Sub Base_Connect_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Not Visible Then Return
        RenderState()
        If DulukaAccountStore.Instance.HasSession Then
            RefreshAccountAsync()
        End If
    End Sub

    Private Sub RenderState()
        Dim authed As Boolean = DulukaAccountStore.Instance.HasSession

        ' Unauthenticated — sign-in entry, provider-aware wording. GitHub is
        ' presented as an authentication method, not as the account itself.
        BT_Connect.Visible = Not authed
        Auth_PROMPT.Visible = Not authed
        Provider_NOTE.Visible = Not authed

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
            Account_META.Text = "Account  " & If(DulukaAccountStore.Instance.AccountId <> "", DulukaAccountStore.Instance.AccountId, "—") &
                                Environment.NewLine &
                                "This device:  " & If(DulukaAccountStore.Instance.DeviceName <> "", DulukaAccountStore.Instance.DeviceName, "—")
        Else
            USERSNAME_TEXT.Text = ""
            Account_META.Text = ""
            Status_TEXT.Text = ""
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
                Dim accountId As String = ResourceText(r.Resource, "accountId")
                Dim deviceName As String = ""
                Dim deviceNode As JsonNode = r.Resource("currentDevice")
                If deviceNode IsNot Nothing Then
                    deviceName = NodeText(deviceNode, "deviceName")
                End If
                DulukaAccountStore.Instance.SetProfile(displayName)

                USERSNAME_TEXT.Text = If(displayName <> "", displayName, "Duluka Account")
                Account_META.Text = "Account  " & If(accountId <> "", accountId, "—") &
                                    Environment.NewLine &
                                    "This device:  " & If(deviceName <> "", deviceName, "—")
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
    ' the correct state (authenticated or the sign-in entry).</summary>
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

End Class
