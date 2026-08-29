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
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
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
        ' RECORDING
        If ToggleRecordingStarted IsNot Nothing Then ToggleRecordingStarted.IsOn = AppSettings.Instance.Notifications.RecordingStarted
        If ToggleRecordingSaved IsNot Nothing Then ToggleRecordingSaved.IsOn = AppSettings.Instance.Notifications.RecordingSaved
        If ToggleRecordingError IsNot Nothing Then ToggleRecordingError.IsOn = AppSettings.Instance.Notifications.RecordingError
        ' INSTANT REPLAY
        If ToggleReplaySaved IsNot Nothing Then ToggleReplaySaved.IsOn = AppSettings.Instance.Notifications.ReplaySaved
        If ToggleInstantReplayOn IsNot Nothing Then ToggleInstantReplayOn.IsOn = AppSettings.Instance.Notifications.InstantReplayOn
        If ToggleInstantReplayOff IsNot Nothing Then ToggleInstantReplayOff.IsOn = AppSettings.Instance.Notifications.InstantReplayOff
        If ToggleReplayTurnOn IsNot Nothing Then ToggleReplayTurnOn.IsOn = AppSettings.Instance.Notifications.ReplayTurnOn
        If ToggleReplayError IsNot Nothing Then ToggleReplayError.IsOn = AppSettings.Instance.Notifications.ReplayError
        ' SCREENSHOTS
        If ToggleScreenshotSaved IsNot Nothing Then ToggleScreenshotSaved.IsOn = AppSettings.Instance.Notifications.ScreenshotSaved
        If ToggleValidSavePath IsNot Nothing Then ToggleValidSavePath.IsOn = AppSettings.Instance.Notifications.ValidSavePath
        ' SHARE OVERLAY
        If ToggleOpenShare IsNot Nothing Then ToggleOpenShare.IsOn = AppSettings.Instance.Notifications.OpenShare
        ' SYSTEM MONITOR
        If ToggleRamWarning IsNot Nothing Then ToggleRamWarning.IsOn = AppSettings.Instance.Notifications.RamWarning
        If ToggleRamWarning95 IsNot Nothing Then ToggleRamWarning95.IsOn = AppSettings.Instance.Notifications.RamWarning95
        If ToggleRamCritical IsNot Nothing Then ToggleRamCritical.IsOn = AppSettings.Instance.Notifications.RamCritical
        If ToggleCpuWarning IsNot Nothing Then ToggleCpuWarning.IsOn = AppSettings.Instance.Notifications.CpuWarning
        If ToggleDiskSpaceLow IsNot Nothing Then ToggleDiskSpaceLow.IsOn = AppSettings.Instance.Notifications.DiskSpaceLow
        ' UPDATES
        If ToggleUpdateAvailable IsNot Nothing Then ToggleUpdateAvailable.IsOn = AppSettings.Instance.Notifications.UpdateAvailable
        If ToggleVersionLatest IsNot Nothing Then ToggleVersionLatest.IsOn = AppSettings.Instance.Notifications.VersionLatest
        If ToggleUpdateError IsNot Nothing Then ToggleUpdateError.IsOn = AppSettings.Instance.Notifications.UpdateError
        ' ERRORS & FEEDBACK
        If ToggleAccountConfirmError IsNot Nothing Then ToggleAccountConfirmError.IsOn = AppSettings.Instance.Notifications.AccountConfirmError
        If ToggleExtensionNotFound IsNot Nothing Then ToggleExtensionNotFound.IsOn = AppSettings.Instance.Notifications.ExtensionNotFound
        If ToggleFeatureNotReady IsNot Nothing Then ToggleFeatureNotReady.IsOn = AppSettings.Instance.Notifications.FeatureNotReady
        If ToggleGpuRequired IsNot Nothing Then ToggleGpuRequired.IsOn = AppSettings.Instance.Notifications.GpuRequired
        If ToggleEngineNotRunning IsNot Nothing Then ToggleEngineNotRunning.IsOn = AppSettings.Instance.Notifications.EngineNotRunning
        If ToggleEngineUIInUse IsNot Nothing Then ToggleEngineUIInUse.IsOn = AppSettings.Instance.Notifications.EngineUIInUse
        If ToggleErrorResolution IsNot Nothing Then ToggleErrorResolution.IsOn = AppSettings.Instance.Notifications.ErrorResolution
        If ToggleDesktopCaptureDisabled IsNot Nothing Then ToggleDesktopCaptureDisabled.IsOn = AppSettings.Instance.Notifications.DesktopCaptureDisabled
        _notiLoading = False
    End Sub

    ' Bulk switch (Enable all / Disable all buttons): flips every switch
    ' under the same _notiLoading gate so no per-toggle save fires mid
    ' loop, then writes the config section directly and saves once.
    Private Sub SetAllNotifications(value As Boolean)
        _notiLoading = True
        ' RECORDING
        If ToggleRecordingStarted IsNot Nothing Then ToggleRecordingStarted.IsOn = value
        If ToggleRecordingSaved IsNot Nothing Then ToggleRecordingSaved.IsOn = value
        If ToggleRecordingError IsNot Nothing Then ToggleRecordingError.IsOn = value
        ' INSTANT REPLAY
        If ToggleReplaySaved IsNot Nothing Then ToggleReplaySaved.IsOn = value
        If ToggleInstantReplayOn IsNot Nothing Then ToggleInstantReplayOn.IsOn = value
        If ToggleInstantReplayOff IsNot Nothing Then ToggleInstantReplayOff.IsOn = value
        If ToggleReplayTurnOn IsNot Nothing Then ToggleReplayTurnOn.IsOn = value
        If ToggleReplayError IsNot Nothing Then ToggleReplayError.IsOn = value
        ' SCREENSHOTS
        If ToggleScreenshotSaved IsNot Nothing Then ToggleScreenshotSaved.IsOn = value
        If ToggleValidSavePath IsNot Nothing Then ToggleValidSavePath.IsOn = value
        ' SHARE OVERLAY
        If ToggleOpenShare IsNot Nothing Then ToggleOpenShare.IsOn = value
        ' SYSTEM MONITOR
        If ToggleRamWarning IsNot Nothing Then ToggleRamWarning.IsOn = value
        If ToggleRamWarning95 IsNot Nothing Then ToggleRamWarning95.IsOn = value
        If ToggleRamCritical IsNot Nothing Then ToggleRamCritical.IsOn = value
        If ToggleCpuWarning IsNot Nothing Then ToggleCpuWarning.IsOn = value
        If ToggleDiskSpaceLow IsNot Nothing Then ToggleDiskSpaceLow.IsOn = value
        ' UPDATES
        If ToggleUpdateAvailable IsNot Nothing Then ToggleUpdateAvailable.IsOn = value
        If ToggleVersionLatest IsNot Nothing Then ToggleVersionLatest.IsOn = value
        If ToggleUpdateError IsNot Nothing Then ToggleUpdateError.IsOn = value
        ' ERRORS & FEEDBACK
        If ToggleAccountConfirmError IsNot Nothing Then ToggleAccountConfirmError.IsOn = value
        If ToggleExtensionNotFound IsNot Nothing Then ToggleExtensionNotFound.IsOn = value
        If ToggleFeatureNotReady IsNot Nothing Then ToggleFeatureNotReady.IsOn = value
        If ToggleGpuRequired IsNot Nothing Then ToggleGpuRequired.IsOn = value
        If ToggleEngineNotRunning IsNot Nothing Then ToggleEngineNotRunning.IsOn = value
        If ToggleEngineUIInUse IsNot Nothing Then ToggleEngineUIInUse.IsOn = value
        If ToggleErrorResolution IsNot Nothing Then ToggleErrorResolution.IsOn = value
        If ToggleDesktopCaptureDisabled IsNot Nothing Then ToggleDesktopCaptureDisabled.IsOn = value
        _notiLoading = False
        With AppSettings.Instance.Notifications
            .RecordingStarted = value
            .RecordingSaved = value
            .RecordingError = value
            .ReplaySaved = value
            .InstantReplayOn = value
            .InstantReplayOff = value
            .ReplayTurnOn = value
            .ReplayError = value
            .ScreenshotSaved = value
            .ValidSavePath = value
            .OpenShare = value
            .RamWarning = value
            .RamWarning95 = value
            .RamCritical = value
            .CpuWarning = value
            .DiskSpaceLow = value
            .UpdateAvailable = value
            .VersionLatest = value
            .UpdateError = value
            .AccountConfirmError = value
            .ExtensionNotFound = value
            .FeatureNotReady = value
            .GpuRequired = value
            .EngineNotRunning = value
            .EngineUIInUse = value
            .ErrorResolution = value
            .DesktopCaptureDisabled = value
        End With
        AppSettings.Instance.Save()
    End Sub

    Private Sub BT_EnableAll_Click(sender As Object, e As EventArgs) Handles BT_EnableAll.Click
        SetAllNotifications(True)
    End Sub

    Private Sub BT_DisableAll_Click(sender As Object, e As EventArgs) Handles BT_DisableAll.Click
        SetAllNotifications(False)
    End Sub

    ' RECORDING
    Private Sub ToggleRecordingStarted_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRecordingStarted.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.RecordingStarted = ToggleRecordingStarted.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleRecordingSaved_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRecordingSaved.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.RecordingSaved = ToggleRecordingSaved.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleRecordingError_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRecordingError.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.RecordingError = ToggleRecordingError.IsOn
        AppSettings.Instance.Save()
    End Sub

    ' INSTANT REPLAY
    Private Sub ToggleReplaySaved_ValueChanged(sender As Object, e As EventArgs) Handles ToggleReplaySaved.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ReplaySaved = ToggleReplaySaved.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleInstantReplayOn_ValueChanged(sender As Object, e As EventArgs) Handles ToggleInstantReplayOn.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.InstantReplayOn = ToggleInstantReplayOn.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleInstantReplayOff_ValueChanged(sender As Object, e As EventArgs) Handles ToggleInstantReplayOff.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.InstantReplayOff = ToggleInstantReplayOff.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleReplayTurnOn_ValueChanged(sender As Object, e As EventArgs) Handles ToggleReplayTurnOn.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ReplayTurnOn = ToggleReplayTurnOn.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleReplayError_ValueChanged(sender As Object, e As EventArgs) Handles ToggleReplayError.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ReplayError = ToggleReplayError.IsOn
        AppSettings.Instance.Save()
    End Sub

    ' SCREENSHOTS
    Private Sub ToggleScreenshotSaved_ValueChanged(sender As Object, e As EventArgs) Handles ToggleScreenshotSaved.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ScreenshotSaved = ToggleScreenshotSaved.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleValidSavePath_ValueChanged(sender As Object, e As EventArgs) Handles ToggleValidSavePath.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ValidSavePath = ToggleValidSavePath.IsOn
        AppSettings.Instance.Save()
    End Sub

    ' SHARE OVERLAY
    Private Sub ToggleOpenShare_ValueChanged(sender As Object, e As EventArgs) Handles ToggleOpenShare.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.OpenShare = ToggleOpenShare.IsOn
        AppSettings.Instance.Save()
    End Sub

    ' SYSTEM MONITOR
    Private Sub ToggleRamWarning_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRamWarning.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.RamWarning = ToggleRamWarning.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleRamWarning95_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRamWarning95.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.RamWarning95 = ToggleRamWarning95.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleRamCritical_ValueChanged(sender As Object, e As EventArgs) Handles ToggleRamCritical.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.RamCritical = ToggleRamCritical.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleCpuWarning_ValueChanged(sender As Object, e As EventArgs) Handles ToggleCpuWarning.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.CpuWarning = ToggleCpuWarning.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleDiskSpaceLow_ValueChanged(sender As Object, e As EventArgs) Handles ToggleDiskSpaceLow.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.DiskSpaceLow = ToggleDiskSpaceLow.IsOn
        AppSettings.Instance.Save()
    End Sub

    ' UPDATES
    Private Sub ToggleUpdateAvailable_ValueChanged(sender As Object, e As EventArgs) Handles ToggleUpdateAvailable.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.UpdateAvailable = ToggleUpdateAvailable.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleVersionLatest_ValueChanged(sender As Object, e As EventArgs) Handles ToggleVersionLatest.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.VersionLatest = ToggleVersionLatest.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleUpdateError_ValueChanged(sender As Object, e As EventArgs) Handles ToggleUpdateError.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.UpdateError = ToggleUpdateError.IsOn
        AppSettings.Instance.Save()
    End Sub

    ' ERRORS & FEEDBACK
    Private Sub ToggleAccountConfirmError_ValueChanged(sender As Object, e As EventArgs) Handles ToggleAccountConfirmError.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.AccountConfirmError = ToggleAccountConfirmError.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleExtensionNotFound_ValueChanged(sender As Object, e As EventArgs) Handles ToggleExtensionNotFound.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ExtensionNotFound = ToggleExtensionNotFound.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleFeatureNotReady_ValueChanged(sender As Object, e As EventArgs) Handles ToggleFeatureNotReady.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.FeatureNotReady = ToggleFeatureNotReady.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleGpuRequired_ValueChanged(sender As Object, e As EventArgs) Handles ToggleGpuRequired.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.GpuRequired = ToggleGpuRequired.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleEngineNotRunning_ValueChanged(sender As Object, e As EventArgs) Handles ToggleEngineNotRunning.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.EngineNotRunning = ToggleEngineNotRunning.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleEngineUIInUse_ValueChanged(sender As Object, e As EventArgs) Handles ToggleEngineUIInUse.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.EngineUIInUse = ToggleEngineUIInUse.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleErrorResolution_ValueChanged(sender As Object, e As EventArgs) Handles ToggleErrorResolution.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.ErrorResolution = ToggleErrorResolution.IsOn
        AppSettings.Instance.Save()
    End Sub

    Private Sub ToggleDesktopCaptureDisabled_ValueChanged(sender As Object, e As EventArgs) Handles ToggleDesktopCaptureDisabled.ValueChanged
        If _notiLoading Then Return
        AppSettings.Instance.Notifications.DesktopCaptureDisabled = ToggleDesktopCaptureDisabled.IsOn
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
