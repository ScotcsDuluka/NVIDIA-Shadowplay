

Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Providers
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

    Private _loading As Boolean
    Private _linking As Boolean
    Private _flow As DulukaAuthFlow
    Private _nextRowY As Integer = 8

    Private Shared Function L(key As String, ParamArray args() As String) As String
        Return LangHelper.GetText(key, args)
    End Function

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible AndAlso Not _linking Then
            LoadProvidersAsync()
        End If
    End Sub

    Private Async Sub LoadProvidersAsync()
        If _loading Then Return
        If Not DulukaAccountStore.Instance.HasSession Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        _loading = True
        Try
            Status_TEXT.Text = L("l10n.acctProvidersLoading")
            ClearRows()
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/providers", token).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then

                Dim githubLink As JsonNode = Nothing

                Dim list As JsonNode = r.Resource("providers")
                If list IsNot Nothing Then
                    For Each p As JsonNode In list.AsArray()
                        If p Is Nothing Then Continue For
                        If NodeText(p, "providerKey") = "github" AndAlso NodeText(p, "status") = "Active" Then
                            githubLink = p
                        End If
                    Next
                End If

                If githubLink IsNot Nothing Then
                    AddProviderRow(NodeText(githubLink, "linkId"), "github",
                                   NodeText(githubLink, "providerEmail"),
                                   NodeText(githubLink, "linkedAt"), True)
                Else

                    AddProviderRow("", "github", "", "", False)
                End If
            ElseIf r.AuthDead Then
                TerminalSignOut(L("l10n.acctSessionExpired"))
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Finally
            _loading = False
        End Try
    End Sub

    Private Sub AddProviderRow(linkId As String, providerKey As String, email As String,
                               linkedAt As String, connected As Boolean)

        Dim rowW As Integer = Math.Max(List_PANEL.ClientSize.Width, 480)
        Dim row As New Panel With {
            .BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57)),
            .Location = New Point(0, _nextRowY),
            .Size = New Size(rowW, 72)
        }
        _nextRowY += 80

        Dim title As New Label With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 11.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(16, 10)
        }
        Dim shownEmail As String = If(email <> "", email, L("l10n.acctProvidersNoEmail"))
        title.Text = L("l10n.acctProvidersGithubTitle", If(connected, shownEmail, L("l10n.acctProvidersNotConnected")))

        Dim meta As New Label With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.Gainsboro,
            .Location = New Point(16, 40)
        }
        If connected Then
            meta.Text = L("l10n.acctProvidersConnectedAt", FormatIso(linkedAt))
        Else
            meta.Text = L("l10n.acctProvidersNotLinkedYet")
        End If

        row.Controls.Add(title)
        row.Controls.Add(meta)

        If connected Then
            Dim unlink As New Label With {
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
                .BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40)),
                .Cursor = Cursors.Hand,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .ForeColor = Color.White,
                .Location = New Point(rowW - 180, 14),
                .Size = New Size(150, 44),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Text = L("l10n.acctProvidersUnlink")
            }
            
            Dim rowLinkId As String = linkId
            AddHandler unlink.Click, Sub(s, e) UnlinkProvider(rowLinkId)
            row.Controls.Add(unlink)
        Else
            Dim link As New Label With {
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
                .BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0)),
                .Cursor = Cursors.Hand,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .ForeColor = Color.White,
                .Location = New Point(rowW - 180, 14),
                .Size = New Size(150, 44),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Text = L("l10n.acctProvidersLinkGithub")
            }
            AddHandler link.Click, Sub(s, e) BT_LinkNew_Click(s, e)
            row.Controls.Add(link)
        End If

        List_PANEL.Controls.Add(row)
        StretchRows()
    End Sub

    Private Sub ClearRows()
        List_PANEL.Controls.Clear()
        _nextRowY = 8
    End Sub

    Private Async Sub UnlinkProvider(linkId As String)
        If MessageBox.Show(L("l10n.acctProvidersUnlinkConfirm") & Environment.NewLine &
                           L("l10n.acctProvidersUnlinkConfirmNote"),
                           L("l10n.acctProvidersUnlinkCaption"), MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

        Status_TEXT.Text = L("l10n.acctProvidersUnlinking")
        Dim token As String = DulukaAccountStore.Instance.SessionToken
        Dim r As DulukaApi.Result = Await DulukaApi.DeleteAsync(
            "/v1/account/providers/" & linkId, token).ConfigureAwait(True)
        If IsDisposed OrElse Not IsHandleCreated Then Return

        If r.Ok Then
            Dim revoked As Integer = 0
            If r.Resource IsNot Nothing Then
                Dim countNode As JsonNode = r.Resource("revokedSessions")
                If countNode IsNot Nothing Then revoked = countNode.GetValue(Of Integer)()
            End If
            Status_TEXT.Text = If(revoked > 0,
                L("l10n.acctProvidersUnlinkedRevoked", revoked.ToString()),
                L("l10n.acctProvidersUnlinked"))
            LoadProvidersAsync()
        ElseIf r.AuthDead Then
            TerminalSignOut(L("l10n.acctSessionExpired"))
        ElseIf r.HttpStatus = 409 Then

            Status_TEXT.Text = L("l10n.acctProvidersCannotUnlink", r.Message)
        ElseIf r.HttpStatus = 404 Then
            Status_TEXT.Text = L("l10n.acctProvidersLinkAlreadyGone")
            LoadProvidersAsync()
        Else
            Status_TEXT.Text = DulukaApi.HumanError(r)
        End If
    End Sub

    Private Async Sub BT_LinkNew_Click(sender As Object, e As EventArgs) Handles BT_LinkNew.Click
        If _linking Then Return
        If Not DulukaAccountStore.Instance.HasSession Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If

        Dim flow As DulukaAuthFlow = DulukaAuthFlow.TryBegin(DulukaAuthFlow.FlowKind.LinkProvider)
        If flow Is Nothing Then
            Status_TEXT.Text = L("l10n.acctAnotherFlow")
            Return
        End If
        _flow = flow

        _linking = True
        BT_LinkNew.Enabled = False
        Try
            ' Same presentation shell as the Login flow
            ' (Base_Connect_Login.BT_StartLogin_Click): hand the overlay over
            ' to the authentication presentation while GitHub is in progress.
            Base_Connect.Hide()
            Base_Settings.Hide()
            Base.ME_CLOSE_BG.Visible = True
            Base.Opacity = 0
            Base.Settings_List.Visible = False
            Base.shadowplay.Visible = True

            Base_Background_Top.d.Visible = True
            Base_Background_Top.ME_CLOSE_BG_GRE.Visible = True
            Base_Background_Top.ME_CLOSE_BG.Visible = True

            Base.ShowMainPanel()
            Base.Opacity = 0.85
            Base.IF_OpenShare = True
            Base.HideAllControls()
            Me.Hide()

            Dim outcome As DulukaAuthFlow.Outcome = Await flow.RunAsync(AddressOf Report).ConfigureAwait(True)
            _flow = Nothing
            If IsDisposed OrElse Not IsHandleCreated Then Return

            ' Restore the presentation the same way the Login flow does,
            ' then come back HERE — to Linked providers — never to Login.
            ' Provider linking is not sign-in: no setup gate, no identity
            ' change, just the provider link on the existing account.
            Base.ShowMainPanel()
            Base.OpenSettings()
            Base.IF_OpenShare = False

            Base_Settings.Hide()
            Base_Connect.Hide()
            Base.Settings_List.Visible = False
            Me.Show()

            If outcome.Succeeded Then
                LoadProvidersAsync()
                Status_TEXT.Text = L("l10n.acctProvidersLinkSuccess")
            Else
                Status_TEXT.Text = outcome.Message
            End If
        Finally
            _flow = Nothing
            _linking = False
            If Not IsDisposed Then BT_LinkNew.Enabled = True
        End Try
    End Sub

    Private Sub Report(message As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        Try
            Invoke(New Action(Sub() Status_TEXT.Text = message))
        Catch ex As ObjectDisposedException
        Catch ex As InvalidOperationException
        End Try
    End Sub

    Private Sub TerminalSignOut(message As String)
        DulukaAccountStore.Instance.ClearSession()
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
        Base_Connect.NotifyFromSubPage(message)
    End Sub

    Private Function NodeText(node As JsonNode, name As String) As String
        Dim value As JsonNode = node(name)
        If value Is Nothing Then Return ""
        Return value.GetValue(Of String)()
    End Function

    Private Function FormatIso(iso As String) As String
        If iso = "" Then Return "—"
        Dim parsed As DateTimeOffset
        If DateTimeOffset.TryParse(iso, parsed) Then
            Return parsed.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        End If
        Return iso
    End Function

    Private Sub StretchRows()
        Dim w As Integer = List_PANEL.ClientSize.Width
        If w <= 0 Then Return
        For Each c As Control In List_PANEL.Controls
            If TypeOf c Is Panel Then c.Width = w
        Next
    End Sub

    Private Sub List_PANEL_Resize(sender As Object, e As EventArgs) Handles List_PANEL.Resize
        StretchRows()
    End Sub

    Private Sub BT_RefreshProviders_Click(sender As Object, e As EventArgs) Handles BT_RefreshProviders.Click
        LoadProvidersAsync()
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        If _flow IsNot Nothing Then
            _flow.Cancel()
        End If
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub
End Class
