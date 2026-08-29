Imports System.Drawing
Imports System.IO

' Base (mouse) — click / hover wiring for the main menu: screenshot, settings,
' replay / record menus, gallery, and every settings-panel entry point.
' Pure UI event handlers — recording logic lives in the other Base partials.

Partial Public Class Base

#Region "Screenshot (Mode 1)"

    Private Sub ScreenshotBg_MouseMove(sender As Object, e As MouseEventArgs) Handles Bg_Mode1.MouseMove, Logo_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub ScreenshotBg_MouseLeave(sender As Object, e As EventArgs) Handles Bg_Mode1.MouseLeave, Logo_Mode1.MouseLeave
        ResetScreenshotColors()
        SetScreenshotBorder(False)
        Base_Background_Top.Bg_Mode1.Visible = False
    End Sub

    Private Sub ScreenshotText_MouseMove(sender As Object, e As MouseEventArgs) Handles Text_Mode1.MouseMove, Key_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub ScreenshotText_MouseLeave(sender As Object, e As EventArgs) Handles Text_Mode1.MouseLeave, Key_Mode1.MouseLeave
        SetScreenshotBorder(False)
        Base_Background_Top.Bg_Mode1.Visible = False
    End Sub

    Private Sub SetScreenshotBorder(isVisible As Boolean)
        s_1.Visible = isVisible
        s_1r.Visible = isVisible
        s_1l.Visible = isVisible
        s_1b.Visible = isVisible
    End Sub

    Private Sub ResetScreenshotColors()
        Logo_Mode1.ForeColor = System.Drawing.Color.White
        Text_Mode1.ForeColor = System.Drawing.Color.White
    End Sub

    Private Sub Screenshot_Click(sender As Object, e As EventArgs) Handles Logo_Mode1.Click, Bg_Mode1.Click, Text_Mode1.Click, Key_Mode1.Click
        If AppSettings.Instance.UI.UseWindowsSnip Then
            SnipWithWindows()
        Else
            CaptureScreen()
        End If
    End Sub

#End Region

#Region "Settings page"
    Public Sub OpenSettings()
        Me.Opacity = 0
        Base_Settings.Opacity = 0
        Base_Settings.Show()

        Base_Background_Top.Bg_SET3.Visible = False
        ME_CLOSE_BG.Visible = False
        clickThrough = True
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Settings_List.Visible = True
        shadowplay.Visible = False
        Menu_Replay.Visible = False
        Menu_Record.Visible = False

        ' Let the settings form finish showing, then fade in and animate the menu.
        RunAfterDelay(20,
            Sub()
                Base_Settings.Opacity = 1
                Me.Opacity = 1
                AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
            End Sub)
    End Sub
    Private Sub SettingsLogo_Click(sender As Object, e As EventArgs) Handles Settings_Logo.Click, Settings_Box.Click, Settings_Text.Click
        OpenSettings()
        IF_OpenShare = False
    End Sub
    Private Sub SettingsLogo_MouseMove(sender As Object, e As MouseEventArgs) Handles Settings_Logo.MouseMove
        SetSettingsBorder(True)
    End Sub

    Private Sub SettingsLogo_MouseLeave(sender As Object, e As EventArgs) Handles Settings_Logo.MouseLeave
        SetSettingsBorder(False)
    End Sub

    Private Sub SetSettingsBorder(isVisible As Boolean)
        s1.Visible = isVisible
        s1r.Visible = isVisible
        s1l.Visible = isVisible
        s1b.Visible = isVisible
    End Sub

#End Region

#Region "Connect (double-registered with the panel handlers below — kept, see note)"

    Private Sub Privacy_MouseMove(sender As Object, e As MouseEventArgs) Handles Connect_TEXT.MouseMove, Connect_ICO.MouseMove
        Connect_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub Privacy_MouseLeave(sender As Object, e As EventArgs) Handles Connect_TEXT.MouseLeave, Connect_ICO.MouseLeave
        Connect_BOX_SUB.BackColor = System.Drawing.Color.Gray
    End Sub

    ' NOTE (kept as-is): this handler is double-registered with Connect_Click
    ' below — both fire on Connect_TEXT.Click / Connect_ICO.Click, so a click
    ' currently shows the error notifier AND opens the Connect panel.
    ' Removing either side changes user-visible behavior — OWNER's call.
    Private Sub Privacy_Click(sender As Object, e As EventArgs) Handles Connect_TEXT.Click, Connect_ICO.Click
        ShowNotifier("account_confirm_error")
    End Sub

#End Region

#Region "Replay menu"

    Private Sub ReplayMenu_MouseMove(sender As Object, e As MouseEventArgs) Handles Replay_Logo.MouseMove, Replay_Text.MouseMove, Replay_Stats.MouseMove
        SetReplayBorder(Not Menu_Replay.Visible)
        Base_Background_Top.b1.Visible = True
    End Sub

    Private Sub ReplayMenu_MouseLeave(sender As Object, e As EventArgs) Handles Replay_Logo.MouseLeave, Replay_Text.MouseLeave, Replay_Stats.MouseLeave
        SetReplayBorder(False)
        Base_Background_Top.b1.Visible = False
        SetReplayControlBorder(False)
    End Sub

    Private Sub SetReplayBorder(isVisible As Boolean)
        a_1.Visible = Menu_Replay.Visible OrElse isVisible
        a_1r.Visible = isVisible
        a_1l.Visible = isVisible
        a_1b.Visible = isVisible
    End Sub

    ' Shared by Click + MouseHover: flip the replay menu open/closed.
    Private Sub ToggleReplayMenu()
        ShadowLoad()
        Menu_Replay.Visible = Not Menu_Replay.Visible
        Menu_Record.Visible = False
        a_1.Visible = Not a_1.Visible
        a_2.Visible = False
        a_3.Visible = False
        SetReplayControlBorder(True)
    End Sub

    Private Sub ReplayMenu_Click(sender As Object, e As EventArgs) Handles Replay_Logo.Click, Replay_Text.Click, Replay_Stats.Click
        ToggleReplayMenu()
    End Sub

    Private Sub ReplayMenu_MouseHover(sender As Object, e As EventArgs) Handles Replay_Logo.MouseHover, Replay_Text.MouseHover, Replay_Stats.MouseHover
        If Base_Background_Top.b2_all.Visible = True Then
            ToggleReplayMenu()
        End If
    End Sub



#End Region

#Region "Record menu"

    Private Sub RecordMenu_MouseMove(sender As Object, e As MouseEventArgs) Handles Record_Logo.MouseMove, Record_Text.MouseMove, Record_Stats.MouseMove
        SetRecordBorder(Not Menu_Record.Visible)
        Base_Background_Top.b2.Visible = True
    End Sub

    Private Sub RecordMenu_MouseLeave(sender As Object, e As EventArgs) Handles Record_Logo.MouseLeave, Record_Text.MouseLeave, Record_Stats.MouseLeave
        SetRecordBorder(False)
        SetRecordControlBorder(False)
        Base_Background_Top.b2.Visible = False
    End Sub

    Private Sub SetRecordBorder(isVisible As Boolean)
        a_2.Visible = Menu_Record.Visible OrElse isVisible
        a_2r.Visible = isVisible
        a_2l.Visible = isVisible
        a_2b.Visible = isVisible
    End Sub

    ' Shared by Click + MouseHover: flip the record menu open/closed.
    Private Sub ToggleRecordMenu()
        ShadowLoad()
        Menu_Record.Visible = Not Menu_Record.Visible
        Menu_Replay.Visible = False
        a_2.Visible = True
        a_1.Visible = False
        a_3.Visible = False
        SetRecordControlBorder(True)
    End Sub

    Private Sub RecordMenu_Click(sender As Object, e As EventArgs) Handles Record_Logo.Click, Record_Text.Click, Record_Stats.Click
        ToggleRecordMenu()
    End Sub

    Private Sub RecordMenu_MouseHover(sender As Object, e As EventArgs) Handles Record_Logo.MouseHover, Record_Text.MouseHover, Record_Stats.MouseHover
        If Base_Background_Top.b1_all.Visible = True Then
            ToggleRecordMenu()
        End If
    End Sub

#End Region

#Region "Live stream"

    Private Sub LiveMenu_MouseMove(sender As Object, e As MouseEventArgs) Handles Live_Logo.MouseMove, Live_Text.MouseMove, Live_Stats.MouseMove
        a_3.Visible = True
        a_3r.Visible = True
        a_3l.Visible = True
        a_3b.Visible = True
        Base_Background_Top.b3.Visible = True
    End Sub

    Private Sub LiveMenu_MouseLeave(sender As Object, e As EventArgs) Handles Live_Logo.MouseLeave, Live_Text.MouseLeave, Live_Stats.MouseLeave
        a_3.Visible = False
        a_3r.Visible = False
        a_3l.Visible = False
        a_3b.Visible = False
        Base_Background_Top.b3.Visible = False
        Live_Logo.ForeColor = System.Drawing.Color.White
    End Sub

    Private Sub LiveMenu_Click(sender As Object, e As EventArgs) Handles Live_Logo.Click, Live_Text.Click, Live_Stats.Click
        ShowNotifier("feature_not_ready")
        Menu_Replay.Visible = False
        a_1.Visible = False
        Menu_Record.Visible = False
        a_2.Visible = False
    End Sub

#End Region

#Region "Microphone & video"

    Private Sub Mic_MouseMove(sender As Object, e As MouseEventArgs) Handles MIC_ICO.MouseMove
        MIC_ICO.ForeColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Mic_MouseLeave(sender As Object, e As EventArgs) Handles MIC_ICO.MouseLeave
        MIC_ICO.ForeColor = System.Drawing.Color.White
    End Sub
    ' MIC_ICO.Text holds PUA glyphs from the bundled icon font — they look
    ' invisible in source but are meaningful; never retype or trim them.
    Public Sub LoadMicState()
        If AppSettings.Instance.Audio.MicEnabled = True Then
            MIC_ICO.Text = ""
        Else
            MIC_ICO.Text = ""
        End If
    End Sub
    Private Sub Mic_Click(sender As Object, e As EventArgs) Handles MIC_ICO.Click
        MIC_ICO.Text = If(MIC_ICO.Text = "", "", "")
        If MIC_ICO.Text = "" Then
            AppSettings.Instance.Audio.MicEnabled = True
        Else
            AppSettings.Instance.Audio.MicEnabled = False
        End If
        AppSettings.Instance.Save()

        Debug.WriteLine("Mic Enabled: " & AppSettings.Instance.Audio.MicEnabled)
    End Sub


    Private Sub VideoInfo_MouseMove(sender As Object, e As MouseEventArgs) Handles vdo.MouseMove
        vdo.ForeColor = System.Drawing.Color.Gray
    End Sub

    Private Sub VideoInfo_MouseLeave(sender As Object, e As EventArgs) Handles vdo.MouseLeave
        vdo.ForeColor = System.Drawing.Color.White
    End Sub

    Private Sub VideoInfo_Click(sender As Object, e As EventArgs) Handles vdo.Click
        ShowNotifier("extension_not_found")
    End Sub

#End Region

#Region "Gallery"

    Private Sub SetGalleryColors(color As System.Drawing.Color)
        Gallery_Logo.ForeColor = color
        Gallery_Text.ForeColor = color
        Gallery_Box.ForeColor = color
    End Sub

    Private Sub SetGalleryBorder(isVisible As Boolean)
        g1.Visible = isVisible
        g1r.Visible = isVisible
        g1l.Visible = isVisible
        g1b.Visible = isVisible
    End Sub

    Private Sub Gallery_MouseMove(sender As Object, e As MouseEventArgs) Handles Gallery_Logo.MouseMove, Gallery_Text.MouseMove, Gallery_Box.MouseMove
        Base_Background_Top.Bg_SET2.Visible = True
        SetGalleryBorder(True)
    End Sub

    Private Sub Gallery_MouseLeave(sender As Object, e As EventArgs) Handles Gallery_Logo.MouseLeave, Gallery_Text.MouseLeave, Gallery_Box.MouseLeave
        Base_Background_Top.Bg_SET2.Visible = False
        SetGalleryBorder(False)
    End Sub

    Private Sub Gallery_Click(sender As Object, e As EventArgs) Handles Gallery_Box.Click, Gallery_Text.Click, Gallery_Logo.Click
        IF_OpenShare = False
        shadowplay.Visible = False
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Menu_Replay.Visible = False
        Menu_Record.Visible = False
        Base_Gallery.Opacity = 0
        Base_Gallery.Show()


        ' Fade the gallery in after it has finished showing.
        RunAfterDelay(20,
            Sub()
                AMY(Base_Gallery.Base_Submenu, -200, 5, 300)
                Base_Gallery.Opacity = 1
            End Sub)

    End Sub

#End Region

#Region "Replay controls (instant replay)"

    Private Sub ReplayControl_MouseMove(sender As Object, e As MouseEventArgs) Handles Menu_Replay_key.MouseMove, Menu_Replay_Box1.MouseMove, Menu_Replay_text.MouseMove
        SetReplayControlBorder(True)
    End Sub

    Private Sub ReplayControl_MouseLeave(sender As Object, e As EventArgs) Handles Menu_Replay_key.MouseLeave, Menu_Replay_Box1.MouseLeave, Menu_Replay_text.MouseLeave
        SetReplayControlBorder(False)
    End Sub

    Private Sub SetReplayControlBorder(isVisible As Boolean)
        r_1.Visible = isVisible
        r_1r.Visible = isVisible
        r_1l.Visible = isVisible
        r_1b.Visible = isVisible
    End Sub

    Private Sub ReplayControl_Click(sender As Object, e As EventArgs) Handles Menu_Replay_key.Click, Menu_Replay_Box1.Click, Menu_Replay_text.Click
        ToggleInstantReplay()
    End Sub

#End Region

#Region "Replay save controls"

    Private Sub ReplaySave_MouseMove(sender As Object, e As MouseEventArgs) Handles Menu_Replay_Box2.MouseMove, Menu_Replay_save_text.MouseMove, Menu_Replay_save_key.MouseMove
        SetReplaySaveBorder(True)
    End Sub

    Private Sub ReplaySave_MouseLeave(sender As Object, e As EventArgs) Handles Menu_Replay_Box2.MouseLeave, Menu_Replay_save_text.MouseLeave, Menu_Replay_save_key.MouseLeave
        SetReplaySaveBorder(False)
    End Sub

    Private Sub SetReplaySaveBorder(isVisible As Boolean)
        rs1.Visible = isVisible
        rsl.Visible = isVisible
        rsr.Visible = isVisible
        rsb.Visible = isVisible
    End Sub

    Private Sub ReplaySave_Click(sender As Object, e As EventArgs) Handles Menu_Replay_Box2.Click, Menu_Replay_save_text.Click, Menu_Replay_save_key.Click
        SaveInstantReplay()
    End Sub

#End Region

#Region "Record controls"

    Private Sub RecordControl_MouseMove(sender As Object, e As MouseEventArgs) Handles Menu_Record_key.MouseMove, Menu_Record_Box1.MouseMove, Menu_Record_text.MouseMove
        SetRecordControlBorder(True)
    End Sub

    Private Sub RecordControl_MouseLeave(sender As Object, e As EventArgs) Handles Menu_Record_key.MouseLeave, Menu_Record_Box1.MouseLeave, Menu_Record_text.MouseLeave
        SetRecordControlBorder(False)
    End Sub

    Private Sub SetRecordControlBorder(isVisible As Boolean)
        st1.Visible = isVisible
        str.Visible = isVisible
        stl.Visible = isVisible
        stb.Visible = isVisible
    End Sub

    Private Sub RecordControl_Click(sender As Object, e As EventArgs) Handles Menu_Record_key.Click, Menu_Record_Box1.Click, Menu_Record_text.Click
        a_2.Visible = False
        ToggleRecording()
        Menu_Record.Visible = False
    End Sub

#End Region

#Region "Photo mode (Mode 2)"

    Private Sub SetPhotoColors(color As System.Drawing.Color)
        Logo_Mode2.ForeColor = color
        Text_Mode2.ForeColor = color
        Bg_Mode2.ForeColor = color
    End Sub

    Private Sub SetPhotoBorder(isVisible As Boolean)
        s_2.Visible = isVisible
        s_2r.Visible = isVisible
        s_2l.Visible = isVisible
        s_2b.Visible = isVisible
    End Sub

    Private Sub Photo_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode2.MouseMove, Text_Mode2.MouseMove, Bg_Mode2.MouseMove, Key_Mode2.MouseMove
        SetPhotoBorder(True)
        Base_Background_Top.Bg_Mode2.Visible = True
    End Sub

    Private Sub Photo_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode2.MouseLeave, Text_Mode2.MouseLeave, Bg_Mode2.MouseLeave, Key_Mode2.MouseLeave
        SetPhotoColors(System.Drawing.Color.White)
        SetPhotoBorder(False)
        Base_Background_Top.Bg_Mode2.Visible = False
    End Sub

    Private Sub Photo_Click(sender As Object, e As EventArgs) Handles Bg_Mode2.Click, Text_Mode2.Click, Logo_Mode2.Click, Key_Mode2.Click
        ShowNotifier("notificationWarningNvidiaGpuRequired")
    End Sub

#End Region

#Region "Game filter (Mode 3)"

    Private Sub SetGameColors(color As System.Drawing.Color)
        Logo_Mode3.ForeColor = color
        Text_Mode3.ForeColor = color
        Bg_Mode3.ForeColor = color
    End Sub

    Private Sub SetGameBorder(isVisible As Boolean)
        s_3.Visible = isVisible
        s_3r.Visible = isVisible
        s_3l.Visible = isVisible
        s_3b.Visible = isVisible
        Base_Background_Top.Bg_Mode3.Visible = isVisible
    End Sub

    Private Sub GameFilter_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode3.MouseMove, Text_Mode3.MouseMove, Bg_Mode3.MouseMove, Key_Mode3.MouseMove
        SetGameBorder(True)
    End Sub

    Private Sub GameFilter_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode3.MouseLeave, Text_Mode3.MouseLeave, Bg_Mode3.MouseLeave, Key_Mode3.MouseLeave
        SetGameColors(System.Drawing.Color.White)
        SetGameBorder(False)
    End Sub

    Private Sub GameFilter_Click(sender As Object, e As EventArgs) Handles Logo_Mode3.Click, Text_Mode3.Click, Bg_Mode3.Click, Key_Mode3.Click
        ToggleGameFilter()
        HideAllControls()
        isFunctionActive = False
    End Sub

#End Region

#Region "Upload / share"

    Private Sub SetUploadBorder(isVisible As Boolean)
        h1.Visible = isVisible
        h1r.Visible = isVisible
        h1l.Visible = isVisible
        h1b.Visible = isVisible
    End Sub

    Private Sub Upload_MouseMove(sender As Object, e As MouseEventArgs) Handles Share_Box.MouseMove, Share_Text.MouseMove, Share_Logo.MouseMove
        Base_Background_Top.Bg_SET1.Visible = True
        SetUploadBorder(True)
    End Sub

    Private Sub Upload_MouseLeave(sender As Object, e As EventArgs) Handles Share_Box.MouseLeave, Share_Text.MouseLeave, Share_Logo.MouseLeave
        Base_Background_Top.Bg_SET1.Visible = False
        SetUploadBorder(False)
    End Sub

    Private Sub Upload_Click(sender As Object, e As EventArgs) Handles Share_Text.Click, Share_Box.Click, Share_Logo.Click
        ShowNotifier("feature_not_ready")
    End Sub

#End Region

#Region "Menu panel — settings pages & system entries"

    ' ========== SHARED COLORS ==========
    Private ReadOnly grayColor As Color = Color.Gray

    ' ========== ALL FORMS LIST ==========
    Private ReadOnly allForms As Form() = {
        Base_Settings,
        Base_Connect,
        Base_Privacy_Control,
        Base_Overlay_Hub,
        Base_KeySet,
        Base_RecordingsSet,
        Base_AudioSet,
        Base_Notifications
    }

    ' One-shot delay helper: runs <action> on the UI thread after <intervalMs>.
    ' The timer disposes itself inside Tick — without that Dispose the GCHandle
    ' plus Tick delegate chain stay alive until GC (this used to leak per click).
    Private Sub RunAfterDelay(intervalMs As Integer, action As Action)
        Dim t As New Timer With {.Interval = intervalMs}
        AddHandler t.Tick,
            Sub(s, e)
                t.Stop()
                t.Dispose()
                action()
            End Sub
        t.Start()
    End Sub

    ' ========== HELPER METHOD ==========
    Private Sub OpenPanel(showForm As Form, settingsCtrl As Control)
        IF_OpenShare = False
        For Each f In allForms
            If f IsNot showForm Then f.Hide()
        Next
        Settings_List.Visible = False
        showForm.Opacity = 0
        showForm.Show()
        settingsCtrl.Location = New Point(80, 160)

        ' Show the panel only after it is fully on screen (avoids a flash).
        RunAfterDelay(1, Sub() showForm.Opacity = 1)
    End Sub

    ' ========== SETTINGS PANEL ==========
    Private Sub Settings_MouseMove(sender As Object, e As MouseEventArgs) Handles Settings_Logo.MouseMove, Settings_Box.MouseMove, Settings_Text.MouseMove
        Base_Background_Top.Bg_SET3.Visible = True
        s1.Visible = True : s1r.Visible = True : s1l.Visible = True : s1b.Visible = True
    End Sub

    Private Sub Settings_MouseLeave(sender As Object, e As EventArgs) Handles Settings_Logo.MouseLeave, Settings_Box.MouseLeave, Settings_Text.MouseLeave
        Base_Background_Top.Bg_SET3.Visible = False
        s1.Visible = False : s1r.Visible = False : s1l.Visible = False : s1b.Visible = False
    End Sub

    ' ========== CONNECT ==========
    Private Sub Connect_MouseMove(sender As Object, e As MouseEventArgs) Handles Connect_TEXT.MouseMove, Connect_ICO.MouseMove
        Connect_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub Connect_MouseLeave(sender As Object, e As EventArgs) Handles Connect_TEXT.MouseLeave, Connect_ICO.MouseLeave
        Connect_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub Connect_Click(sender As Object, e As EventArgs) Handles Connect_TEXT.Click, Connect_ICO.Click
        OpenPanel(Base_Connect, Base_Connect.settings_1)
    End Sub

    ' ========== PRIVACY SETTINGS ==========
    Private Sub PrivacyControl_MouseMove(sender As Object, e As MouseEventArgs) Handles PrivacyControl_TEXT.MouseMove, PrivacyControl_ICO.MouseMove
        PrivacyControl_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub PrivacyControl_MouseLeave(sender As Object, e As EventArgs) Handles PrivacyControl_TEXT.MouseLeave, PrivacyControl_ICO.MouseLeave
        PrivacyControl_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub PrivacyControl_Click(sender As Object, e As EventArgs) Handles PrivacyControl_TEXT.Click, PrivacyControl_ICO.Click
        OpenPanel(Base_Privacy_Control, Base_Privacy_Control.settings_1)
    End Sub

    ' ========== OVERLAY HUB ==========
    Private Sub Hub_MouseMove(sender As Object, e As MouseEventArgs) Handles HUDLayout_TEXT.MouseMove, HUDLayout_ICO.MouseMove
        HUDLayout_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub Hub_MouseLeave(sender As Object, e As EventArgs) Handles HUDLayout_TEXT.MouseLeave, HUDLayout_ICO.MouseLeave
        HUDLayout_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub Hub_Click(sender As Object, e As EventArgs) Handles HUDLayout_TEXT.Click, HUDLayout_ICO.Click
        OpenPanel(Base_Overlay_Hub, Base_Overlay_Hub.settings_1)
    End Sub

    ' ========== KEYBOARD SHORTCUTS ==========
    Private Sub KeyboardShortcuts_MouseMove(sender As Object, e As MouseEventArgs) Handles KeyboardShortcuts_TEXT.MouseMove, KeyboardShortcuts_ICO.MouseMove
        KeyboardShortcuts_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub KeyboardShortcuts_MouseLeave(sender As Object, e As EventArgs) Handles KeyboardShortcuts_TEXT.MouseLeave, KeyboardShortcuts_ICO.MouseLeave
        KeyboardShortcuts_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub KeyboardShortcuts_Click(sender As Object, e As EventArgs) Handles KeyboardShortcuts_TEXT.Click, KeyboardShortcuts_ICO.Click
        OpenPanel(Base_KeySet, Base_KeySet.keyset)
    End Sub

    ' ========== HIGHLIGHTS ==========
    Private Sub Highlights_MouseMove(sender As Object, e As MouseEventArgs) Handles Highlights_TEXT.MouseMove, Highlights_ICO.MouseMove
        Highlights_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub Highlights_MouseLeave(sender As Object, e As EventArgs) Handles Highlights_TEXT.MouseLeave, Highlights_ICO.MouseLeave
        Highlights_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub Highlights_Click(sender As Object, e As EventArgs) Handles Highlights_TEXT.Click, Highlights_ICO.Click
        ShowNotifier("feature_not_ready")
    End Sub

    ' ========== VIDEO CAPTURE SETTINGS ==========
    Private Sub VideoCapture_MouseMove(sender As Object, e As MouseEventArgs) Handles VideoCapture_TEXT_SUB.MouseMove, VideoCapture_TEXT.MouseMove, VideoCapture_ICO.MouseMove
        VideoCapture_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub VideoCapture_MouseLeave(sender As Object, e As EventArgs) Handles VideoCapture_TEXT_SUB.MouseLeave, VideoCapture_TEXT.MouseLeave, VideoCapture_ICO.MouseLeave
        VideoCapture_BOX_SUB.BackColor = grayColor
    End Sub



    Public Sub OpenRecordings()
        IF_OpenShare = False
        Menu_Replay.Visible = False
        Menu_Record.Visible = False
        sha1.Hide()
        sha2.Hide()
        sha3.Hide()
        sha4.Hide()

        ' Sequence: OpenSettings() rebuilds the settings shell first; the
        ' recordings panel opens on top of it 10 ms later.
        RunAfterDelay(10, Sub() OpenSettings())
        RunAfterDelay(20, Sub() OpenPanel(Base_RecordingsSet, Base_RecordingsSet.setret))
    End Sub

    Private Sub VideoCapture_Click(sender As Object, e As EventArgs) Handles VideoCapture_TEXT_SUB.Click, VideoCapture_TEXT.Click, VideoCapture_ICO.Click
        OpenPanel(Base_RecordingsSet, Base_RecordingsSet.setret)

    End Sub

    ' ========== Engine ==========
    Private Sub EngineUI_MouseMove(sender As Object, e As MouseEventArgs) Handles Engine_TEXT.MouseMove, Engine_ICO.MouseMove
        Engine_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub EngineUI_MouseLeave(sender As Object, e As EventArgs) Handles Engine_TEXT.MouseLeave, Engine_ICO.MouseLeave
        Engine_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub EngineUI_Click(sender As Object, e As EventArgs) Handles Engine_TEXT.Click, Engine_ICO.Click
        Dim uiFile = AppLayout.P("Flags", "Engine.UI")

        Dim captureProcess = Process.GetProcessesByName("NVIDIA Capture").FirstOrDefault()
        If captureProcess Is Nothing Then
            Engine_UI.Stop()
            ShowNotifier("notificationErrorEngineNotRunning")
            Exit Sub
        End If
        Try
            Using fs As FileStream = File.Create(uiFile)
            End Using

            Engine_UI.Start()

        Catch ex As IOException
            ShowNotifier("notificationErrorEngineUIInUse")
        End Try
    End Sub

    ' ========== AUDIO ==========
    Private Sub AudioUI_MouseMove(sender As Object, e As MouseEventArgs) Handles Audio_TEXT.MouseMove, Audio_ICO.MouseMove
        Audio_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub AudioUI_MouseLeave(sender As Object, e As EventArgs) Handles Audio_TEXT.MouseLeave, Audio_ICO.MouseLeave
        Audio_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub AudioUI_Click(sender As Object, e As EventArgs) Handles Audio_TEXT.Click, Audio_ICO.Click
        ' ✅ GLM/6: Audio UI now lives in the Overlay ([6] Audio Capture page) —
        ' no more "Audio.UI" marker file round-trip to the Engine process.
        ' The old Engine AudioSettingsForm is dormant (nothing creates the
        ' marker) and can be removed in a follow-up commit.
        OpenPanel(Base_AudioSet, Base_AudioSet.setret)
    End Sub

    ' ========== NOTIFICATIONS ==========
    Private Sub Notifications_MouseMove(sender As Object, e As MouseEventArgs) Handles Notifications_TEXT.MouseMove, notifications_ICO.MouseMove
        Notifications_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub Notifications_MouseLeave(sender As Object, e As EventArgs) Handles Notifications_TEXT.MouseLeave, notifications_ICO.MouseLeave
        Notifications_BOX_SUB.BackColor = grayColor
    End Sub

    Private Sub Notifications_Click(sender As Object, e As EventArgs) Handles Notifications_TEXT.Click, notifications_ICO.Click
        OpenPanel(Base_Notifications, Base_Notifications.Menu_Settings)
    End Sub

    ' ========== ABOUT ==========
    Private Sub About_MouseMove(sender As Object, e As MouseEventArgs) Handles About_TEXT.MouseMove, About_ICO.MouseMove
        About_BOX_SUB.BackColor = greenColor
    End Sub

    Private Sub About_MouseLeave(sender As Object, e As EventArgs) Handles About_TEXT.MouseLeave, About_ICO.MouseLeave
        About_BOX_SUB.BackColor = grayColor
    End Sub

#End Region
    Private Sub About_Click(sender As Object, e As EventArgs) Handles About_TEXT.Click, About_ICO.Click
        ShowNotifier("feature_not_ready")
    End Sub

End Class