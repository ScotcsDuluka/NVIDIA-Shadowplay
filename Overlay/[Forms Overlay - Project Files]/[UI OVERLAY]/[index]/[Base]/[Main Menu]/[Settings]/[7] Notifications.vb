Imports System.Diagnostics
Imports System.Drawing
Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports System.Net.Http.Headers
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks
Imports System.Security.Cryptography
Imports System.IO
Public Class Base_Notifications
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
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

    ' ====================================================================
    ' Settings → Notifications page
    '
    ' Each toggle writes one key of the config.json "Notifications"
    ' section (AppSettings.Notifications). The Notifier re-reads those
    ' keys at display time — it is the single choke point every toast
    ' passes through (TCP path + OBS bridge), so a disabled category
    ' stops showing from the very next notification on.
    ' ====================================================================
    Private _notiLoading As Boolean

    Private Sub LoadNotificationToggles()
        _notiLoading = True
        ' Guards: the instances live in the Designer.vb and can be stripped
        ' by hand edits — never let that NRE the whole page.
        If ToggleRecording IsNot Nothing Then ToggleRecording.IsOn = AppSettings.Instance.Notifications.Recording
        If ToggleInstantReplay IsNot Nothing Then ToggleInstantReplay.IsOn = AppSettings.Instance.Notifications.InstantReplay
        If ToggleScreenshots IsNot Nothing Then ToggleScreenshots.IsOn = AppSettings.Instance.Notifications.Screenshots
        If ToggleShareOverlay IsNot Nothing Then ToggleShareOverlay.IsOn = AppSettings.Instance.Notifications.ShareOverlay
        If ToggleSystemMonitor IsNot Nothing Then ToggleSystemMonitor.IsOn = AppSettings.Instance.Notifications.SystemMonitor
        If ToggleUpdates IsNot Nothing Then ToggleUpdates.IsOn = AppSettings.Instance.Notifications.Updates
        If ToggleErrors IsNot Nothing Then ToggleErrors.IsOn = AppSettings.Instance.Notifications.Errors
        _notiLoading = False
    End Sub

    Private Sub ToggleRecording_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRecording.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.Recording = ToggleRecording.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleInstantReplay_ValueChanged(sender As Object, e As EventArgs) Handles ToggleInstantReplay.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.InstantReplay = ToggleInstantReplay.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleScreenshots_ValueChanged(sender As Object, e As EventArgs) Handles ToggleScreenshots.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.Screenshots = ToggleScreenshots.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleShareOverlay_ValueChanged(sender As Object, e As EventArgs) Handles ToggleShareOverlay.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ShareOverlay = ToggleShareOverlay.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleSystemMonitor_ValueChanged(sender As Object, e As EventArgs) Handles ToggleSystemMonitor.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.SystemMonitor = ToggleSystemMonitor.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleUpdates_ValueChanged(sender As Object, e As EventArgs) Handles ToggleUpdates.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.Updates = ToggleUpdates.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleErrors_ValueChanged(sender As Object, e As EventArgs) Handles ToggleErrors.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.Errors = ToggleErrors.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub Base_Connect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        LoadNotificationToggles()
    End Sub

    Private Sub Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub
End Class
