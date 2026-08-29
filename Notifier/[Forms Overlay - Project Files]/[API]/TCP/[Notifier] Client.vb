Imports System.IO
Partial Public Class Loader

    Private tcp As TcpClientHelper
    Private obs As ObsWebSocketClient
    Private obsCfg As ObsConfig
    Private obsConfigWatcher As System.Windows.Forms.Timer
    Private Const ObsConfigPollMs As Integer = 2000

    Public Class NotificationData
        Public Property Key As String
        Public Property LocalizationKey As String = ""
        Public Property Ico As String = ""
        Public Property Png As Boolean = False
        Public Property Color As Color = Color.White
        Public Property Args As String() = {}

        Public Sub New(key As String, Optional ico As String = "", Optional png As Boolean = False,
                       Optional color As Color = Nothing, Optional args As String() = Nothing,
                       Optional localizationKey As String = "")
            Me.Key = key
            Me.LocalizationKey = localizationKey
            Me.Ico = ico
            Me.Png = png
            Me.Color = If(color = Nothing, Color.White, color)
            Me.Args = If(args, {})
        End Sub
    End Class

    Private ReadOnly notifications As New List(Of NotificationData)()

    Private Sub InitNotifications()
        ' =================== ไม่มี ARGS ===================
        notifications.Add(New NotificationData("l10n.test"))
        notifications.Add(New NotificationData("l10n.notificationWarningDesktopCaptureDisabled", ""))
        notifications.Add(New NotificationData("l10n.notificationScreenshotSavedToGallery", ""))
        notifications.Add(New NotificationData("l10n.ramwram", ""))
        notifications.Add(New NotificationData("l10n.ramwram95", ""))
        notifications.Add(New NotificationData("l10n.ramwramcritical", ""))
        notifications.Add(New NotificationData("l10n.cpuwram", ""))
        notifications.Add(New NotificationData("l10n.diskspacelow", ""))
        notifications.Add(New NotificationData("l10n.irOn", ""))

        ' checks เก่า (key ตรงกับ localization)
        notifications.Add(New NotificationData("l10n.notificationWarningGameRequired", ""))
        notifications.Add(New NotificationData("l10n.notificationWarningNvidiaGpuRequired", ""))
        notifications.Add(New NotificationData("l10n.validsavepath", ""))
        notifications.Add(New NotificationData("l10n.notificationManualRecordStarted", "", False, greenColor))
        notifications.Add(New NotificationData("l10n.notificationManualRecordStopped", ""))
        notifications.Add(New NotificationData("l10n.notificationReplaySaveError", ""))
        notifications.Add(New NotificationData("l10n.notificationTurnOnInstantReplay", ""))
        notifications.Add(New NotificationData("l10n.notificationInstantReplayStopped", ""))
        notifications.Add(New NotificationData("l10n.notificationInstantReplayStarted", "", False, greenColor))
        notifications.Add(New NotificationData("l10n.notificationWarningPhotographyNotAllowed", ""))
        notifications.Add(New NotificationData("l10n.notificationFeatureNotReady", ""))
        notifications.Add(New NotificationData("l10n.notificationCustomOverlayFileNotFound", ""))
        notifications.Add(New NotificationData("l10n.notificationaccountconfirmerror", ""))
        notifications.Add(New NotificationData("l10n.notifierOpen", ""))
        notifications.Add(New NotificationData("l10n.notifierNotUsing", ""))
        notifications.Add(New NotificationData("l10n.notificationAppClosed", ""))
        notifications.Add(New NotificationData("l10n.notificationSharedClose", ""))
        notifications.Add(New NotificationData("l10n.notificationUpdateAvailable", ""))
        notifications.Add(New NotificationData("l10n.notificationVersionLatest", ""))
        notifications.Add(New NotificationData("l10n.notificationErrorResolution", ""))
        notifications.Add(New NotificationData("l10n.notificationErrorGeneral", ""))
        notifications.Add(New NotificationData("l10n.foldererror", ""))
        notifications.Add(New NotificationData("l10n.Capture_notuse", ""))
        notifications.Add(New NotificationData("l10n.openLocation", ""))
        notifications.Add(New NotificationData("l10n.privacy", ""))

        ' =================== เพิ่ม key จริงจาก log ===================
        notifications.Add(New NotificationData("l10n.feature_not_ready", "", localizationKey:="l10n.notificationFeatureNotReady"))
        notifications.Add(New NotificationData("l10n.replay_turn_on", "", localizationKey:="l10n.notificationTurnOnInstantReplay"))
        notifications.Add(New NotificationData("l10n.instant_replay_on", "", False, greenColor, localizationKey:="l10n.notificationInstantReplayStarted"))
        notifications.Add(New NotificationData("l10n.instant_replay_off", "", localizationKey:="l10n.notificationInstantReplayStopped"))
        notifications.Add(New NotificationData("l10n.saved_last_15", "", localizationKey:="l10n.notificationInstantReplaySaved"))
        notifications.Add(New NotificationData("l10n.account_confirm_error", "", localizationKey:="l10n.notificationaccountconfirmerror"))
        notifications.Add(New NotificationData("l10n.replay_error", "", localizationKey:="l10n.notificationReplaySaveError"))
        notifications.Add(New NotificationData("l10n.recording_started", "", False, greenColor, localizationKey:="l10n.notificationManualRecordStarted"))
        notifications.Add(New NotificationData("l10n.recording_saved", "", localizationKey:="l10n.notificationManualRecordStopped"))
        notifications.Add(New NotificationData("l10n.update_available", "", localizationKey:="l10n.notificationUpdateAvailable"))
        notifications.Add(New NotificationData("l10n.version_latest", "", localizationKey:="l10n.notificationVersionLatest"))
        notifications.Add(New NotificationData("l10n.notificationErrorEngineNotRunning", "", localizationKey:="l10n.notificationErrorEngineNotRunning"))

        ' =================== มี ARGS ===================
        notifications.Add(New NotificationData("l10n.testarg", "", False, Color.White, {"1", "2"}))
        notifications.Add(New NotificationData("l10n.notificationInstantReplaySaved", ""))
        notifications.Add(New NotificationData("l10n.notificationOpenShare", "", True, Color.White, {"Alt + Z"}))
    End Sub

    Public Sub OnMessage(msg As String)


        ' เช็ค InvokeRequired
        If InvokeRequired Then
            Invoke(Sub() OnMessage(msg))
            Return
        End If
        LoadLanguage()
        ' ตัวอย่างข้อความ: [NVIDIA Overlay]|l10n.notificationOpenShare
        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        Dim key As String = parts(1).Trim().Trim(""""c)

        If String.IsNullOrEmpty(key) Then Exit Sub

        ' ค้นหา NotificationData
        Dim nd As NotificationData = notifications.FirstOrDefault(
            Function(n) n.Key.Equals(key, StringComparison.OrdinalIgnoreCase))

        If nd Is Nothing Then
            Debug.WriteLine($"[Notifier] Unknown key: {key}")
            Exit Sub
        End If

        ' เลือก localization key (ถ้ามี)
        Dim locKey As String = If(String.IsNullOrEmpty(nd.LocalizationKey), nd.Key, nd.LocalizationKey)
        Dim message As String

        ' กรณีพิเศษ InstantReplaySaved / saved_last_15 → ต้องอ่าน duration + ลบไฟล์
        If nd.Key = "l10n.notificationInstantReplaySaved" OrElse nd.Key = "l10n.saved_last_15" Then
            Dim dur = GetSavedReplayDuration()
            DeleteReplayFiles()
            message = LangHelper.GetText(locKey, dur.minutes.ToString(), dur.seconds.ToString())
        ElseIf nd.Args.Length > 0 Then
            ' มี args อื่น ๆ
            message = LangHelper.GetText(locKey, nd.Args)
        Else
            message = LangHelper.GetText(locKey)
        End If

        ' ลบไฟล์ trigger (ถ้ามี)
        SafeDelete(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", nd.Key))

        ' Settings → Notifications: per-category switch (config.json).
        ' A suppressed toast still cleans up its trigger file above,
        ' but never touches the notifier window.
        If Not NotificationAllowed(key) Then Exit Sub

        ' จัดการสถานะ Notifier
        ManageNotifierState()

        ' แสดงผล — T27: identity = canonical l10n key (aliases collapse),
        ' ดังนั้นการยิง key เดิมซ้ำจะกลายเป็น toggle-update ที่ toast เดิม
        RouteToast(locKey, message, nd.Png, nd.Ico, nd.Color)

        tcp.SendLog(message)
    End Sub

    Public Sub OnMessageWithArgs(msg As String, args As String())
        If InvokeRequired Then
            Invoke(Sub() OnMessageWithArgs(msg, args))
            Return
        End If
        LoadLanguage()
        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        Dim key As String = parts(1).Trim().Trim(""""c)
        If String.IsNullOrEmpty(key) Then Exit Sub

        Dim nd As NotificationData = notifications.FirstOrDefault(
            Function(n) n.Key.Equals(key, StringComparison.OrdinalIgnoreCase))

        If nd Is Nothing Then
            Debug.WriteLine($"[Notifier] Unknown key: {key}")
            Exit Sub
        End If

        Dim locKey As String = If(String.IsNullOrEmpty(nd.LocalizationKey), nd.Key, nd.LocalizationKey)
        Dim message As String
        If args IsNot Nothing AndAlso args.Length > 0 Then
            message = LangHelper.GetText(locKey, args)
        Else
            message = LangHelper.GetText(locKey)
        End If

        SafeDelete(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", nd.Key))

        ' Same per-category gate as OnMessage (see NotificationAllowed).
        If Not NotificationAllowed(key) Then Exit Sub

        ManageNotifierState()
        RouteToast(locKey, message, nd.Png, nd.Ico, nd.Color)
        tcp.SendLog(message)
    End Sub

    ' ====================================================================
    ' Settings → Notifications gates
    '
    ' Every toast funnels through OnMessage / OnMessageWithArgs, so the
    ' per-category switches from the Overlay's Notifications page
    ' (config.json section "Notifications", written by AppSettings) are
    ' checked here — once — at the display choke point. Keys are mapped
    ' explicitly; unknown/future keys always show (fail-open) so a new
    ' notification can never be silently swallowed by a stale mapping.
    ' ====================================================================
    Private Function NotificationAllowed(key As String) As Boolean
        Dim category As String = NotificationCategory(key)
        If category Is Nothing Then Return True
        Return AppConfigShared.ReadBool("Notifications", category, True)
    End Function

    ''' <summary>Returns the config key (Settings→Notifications) for a toast key, or Nothing when the key has no toggle.</summary>
    Private Shared Function NotificationCategory(key As String) As String
        If key.StartsWith("l10n.", StringComparison.OrdinalIgnoreCase) Then key = key.Substring(5)
        Dim k As String = key.ToLowerInvariant()
        Select Case k
            ' RECORDING
            Case "recording_started"
                Return "RecordingStarted"
            Case "recording_saved"
                Return "RecordingSaved"
            Case "recording_error"
                Return "RecordingError"
            ' INSTANT REPLAY
            Case "instant_replay_on"
                Return "InstantReplayOn"
            Case "instant_replay_off"
                Return "InstantReplayOff"
            Case "replay_turn_on"
                Return "ReplayTurnOn"
            Case "replay_error"
                Return "ReplayError"
            Case "notificationinstantreplaysaved", "saved_last_15"
                Return "ReplaySaved"
            ' SCREENSHOTS
            Case "notificationscreenshotsavedtogallery"
                Return "ScreenshotSaved"
            Case "validsavepath"
                Return "ValidSavePath"
            ' SHARE OVERLAY
            Case "notificationopenshare"
                Return "OpenShare"
            ' SYSTEM MONITOR
            Case "ramwram"
                Return "RamWarning"
            Case "ramwram95"
                Return "RamWarning95"
            Case "ramwramcritical"
                Return "RamCritical"
            Case "cpuwram"
                Return "CpuWarning"
            Case "diskspacelow"
                Return "DiskSpaceLow"
            ' UPDATES
            Case "update_available"
                Return "UpdateAvailable"
            Case "version_latest"
                Return "VersionLatest"
            Case "notificationerrorgeneral"
                Return "UpdateError"
            ' ERRORS & FEEDBACK
            Case "account_confirm_error"
                Return "AccountConfirmError"
            Case "extension_not_found"
                Return "ExtensionNotFound"
            Case "feature_not_ready"
                Return "FeatureNotReady"
            Case "notificationwarningnvidiagpurequired"
                Return "GpuRequired"
            Case "notificationerrorenginenotrunning"
                Return "EngineNotRunning"
            Case "notificationerrorengineuiinuse"
                Return "EngineUIInUse"
            Case "notificationerrorresolution"
                Return "ErrorResolution"
            Case "notificationwarningdesktopcapturedisabled"
                Return "DesktopCaptureDisabled"
            Case Else
                Return Nothing
        End Select
    End Function

End Class