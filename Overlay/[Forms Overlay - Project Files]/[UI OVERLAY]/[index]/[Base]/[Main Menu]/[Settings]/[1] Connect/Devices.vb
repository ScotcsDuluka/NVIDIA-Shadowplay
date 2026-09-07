





Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks

Public Class Base_Connect_Devices
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
    Private _nextRowY As Integer = 8

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then
            LoadDevicesAsync()
        End If
    End Sub

    Private Async Sub LoadDevicesAsync()
        If _loading Then Return
        If Not DulukaAccountStore.Instance.HasSession Then
            Me.Hide()
            Base_Connect.ReturnFromSubPage()
            Return
        End If
        _loading = True
        Try
            Status_TEXT.Text = "Loading devices…"
            ClearRows()
            Dim token As String = DulukaAccountStore.Instance.SessionToken
            Dim r As DulukaApi.Result = Await DulukaApi.GetAsync("/v1/account/devices", token).ConfigureAwait(True)
            If IsDisposed OrElse Not IsHandleCreated Then Return

            If r.Ok AndAlso r.Resource IsNot Nothing Then
                Dim list As JsonNode = r.Resource("devices")
                Dim current As JsonNode = Nothing
                Dim others As New List(Of JsonNode)()

                If list IsNot Nothing Then
                    For Each d As JsonNode In list.AsArray()
                        If d Is Nothing Then Continue For
                        If NodeText(d, "deviceId") = DulukaAccountStore.Instance.DeviceId Then
                            current = d
                        Else
                            others.Add(d)
                        End If
                    Next
                End If

                If current IsNot Nothing Then
                    AddSectionHeader("This device")
                    AddDeviceRow(NodeText(current, "deviceId"),
                                 NodeText(current, "deviceName"),
                                 NodeText(current, "lastSeenAt"),
                                 NodeText(current, "revokedAt"), True)
                End If
                If others.Count > 0 Then
                    AddSectionHeader("Other devices")
                    For Each d As JsonNode In others
                        AddDeviceRow(NodeText(d, "deviceId"),
                                     NodeText(d, "deviceName"),
                                     NodeText(d, "lastSeenAt"),
                                     NodeText(d, "revokedAt"), False)
                    Next
                End If

                If current Is Nothing AndAlso others.Count = 0 Then
                    Status_TEXT.Text = "No devices registered yet."
                Else
                    Status_TEXT.Text = ""
                End If
            ElseIf r.AuthDead Then
                TerminalSignOut("Your session has expired. Please sign in again.")
            Else
                Status_TEXT.Text = DulukaApi.HumanError(r)
            End If
        Finally
            _loading = False
        End Try
    End Sub

    Private Sub AddSectionHeader(title As String)
        Dim header As New Label With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0)),
            .Location = New Point(0, _nextRowY + 6)
        }
        header.Text = title.ToUpperInvariant()
        List_PANEL.Controls.Add(header)
        _nextRowY += 36
    End Sub

    Private Sub AddDeviceRow(id As String, name As String, lastSeen As String,
                             revokedAt As String, isCurrent As Boolean)
        
        
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
        title.Text = If(name <> "", name, "Unnamed device")

        Dim meta As New Label With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.Gainsboro,
            .Location = New Point(16, 40)
        }
        If revokedAt <> "" Then
            meta.Text = "Revoked"
            meta.ForeColor = Color.IndianRed
        Else
            meta.Text = "Active  ·  last seen " & FormatIso(lastSeen)
        End If

        row.Controls.Add(title)
        row.Controls.Add(meta)

        If revokedAt = "" Then
            Dim revoke As New Label With {
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
                .BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40)),
                .Cursor = Cursors.Hand,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .ForeColor = Color.White,
                .Location = New Point(rowW - 180, 14),
                .Size = New Size(150, 44),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Text = "Revoke"
            }
            
            Dim rowId As String = id
            Dim rowIsCurrent As Boolean = isCurrent
            AddHandler revoke.Click, Sub(s, e) RevokeDevice(rowId, rowIsCurrent)
            row.Controls.Add(revoke)
        End If

        List_PANEL.Controls.Add(row)
        StretchRows()
    End Sub

    Private Sub ClearRows()
        List_PANEL.Controls.Clear()
        _nextRowY = 8
    End Sub

    Private Async Sub RevokeDevice(deviceId As String, isCurrent As Boolean)
        Dim confirmText As String = If(isCurrent,
            "Revoke THIS device? This Duluka Account Session on it will be signed out.",
            "Revoke this device? All its Duluka Account Sessions will be signed out.")
        If MessageBox.Show(confirmText, "Revoke device", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

        Status_TEXT.Text = "Revoking…"
        Dim token As String = DulukaAccountStore.Instance.SessionToken
        Dim r As DulukaApi.Result = Await DulukaApi.PostAsync(
            "/v1/account/devices/" & deviceId & "/revoke", token, "{}").ConfigureAwait(True)
        If IsDisposed OrElse Not IsHandleCreated Then Return

        If r.Ok Then
            If isCurrent Then
                TerminalSignOut("This device was revoked — signed out.")
            Else
                LoadDevicesAsync()
            End If
        ElseIf r.AuthDead Then
            TerminalSignOut("Your session has expired. Please sign in again.")
        ElseIf r.HttpStatus = 403 Then
            
            
            Status_TEXT.Text = DulukaApi.HumanError(r)
        ElseIf r.HttpStatus = 404 Then
            Status_TEXT.Text = "That device is already gone — refreshing."
            LoadDevicesAsync()
        Else
            Status_TEXT.Text = DulukaApi.HumanError(r)
        End If
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
        If iso = "" Then Return "never"
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

    Private Sub BT_RefreshDevices_Click(sender As Object, e As EventArgs) Handles BT_RefreshDevices.Click
        LoadDevicesAsync()
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub
End Class
