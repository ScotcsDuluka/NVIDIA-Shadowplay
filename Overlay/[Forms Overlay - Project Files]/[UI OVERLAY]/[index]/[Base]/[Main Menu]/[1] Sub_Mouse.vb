Imports System.Drawing
Imports System.IO

Partial Public Class Base

#Region "============================================================================ MOUSE EVENT HANDLERS - SCREENSHOT"

    Private Sub bg_sh_MouseMove(sender As Object, e As MouseEventArgs) Handles Bg_Mode1.MouseMove, Logo_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub bg_sh_MouseLeave(sender As Object, e As EventArgs) Handles Bg_Mode1.MouseLeave, Logo_Mode1.MouseLeave
        ResetScreenshotColors()
        SetScreenshotBorder(False)
        Base_Background_Top.Bg_Mode1.Visible = False
    End Sub

    Private Sub sh_MouseMove(sender As Object, e As MouseEventArgs) Handles Text_Mode1.MouseMove, Key_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub sh_MouseLeave(sender As Object, e As EventArgs) Handles Text_Mode1.MouseLeave, Key_Mode1.MouseLeave
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
        CaptureScreen()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - SETTINGS"
    Public Sub OpenSettings()
        Me.Opacity = 0
        Base_Settings.Opacity = 0
        Base_Settings.Show()

        Base_Background_Top.Bg_SET3.Visible = False
        ME_CLOSE_BG.Visible = False
        clickThrough = True
        a_1.Visible = False : a_2.Visible = False : a_3.Visible = False
        Settings_List.Visible = True
        shadowplay.Visible = False
        Menu_Replay.Visible = False
        Menu_Record.Visible = False

        Dim t As New Timer With {.Interval = 20}
        AddHandler t.Tick, Sub(s, e)
                               t.Stop()
                               Base_Settings.Opacity = 1
                               Me.Opacity = 1

                               AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
                           End Sub
        t.Start()
    End Sub
    Private Sub set_to_Click(sender As Object, e As EventArgs) Handles Settings_Logo.Click, Settings_Box.Click, Settings_Text.Click
        OpenSettings()
        IF_OpenShare = False
    End Sub
    Private Sub set_to_MouseMove(sender As Object, e As MouseEventArgs) Handles Settings_Logo.MouseMove
        SetSettingsBorder(True)
    End Sub

    Private Sub set_to_MouseLeave(sender As Object, e As EventArgs) Handles Settings_Logo.MouseLeave
        SetSettingsBorder(False)
    End Sub

    Private Sub SetSettingsBorder(isVisible As Boolean)
        s1.Visible = isVisible
        s1r.Visible = isVisible
        s1l.Visible = isVisible
        s1b.Visible = isVisible
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - PRIVACY/CONNECT"

    Private Sub Privacy_MouseMove(sender As Object, e As MouseEventArgs) Handles Connect_Text.MouseMove, Connect_ICO.MouseMove
        Connect_Box_Sub.BackColor = greenColor
    End Sub

    Private Sub Privacy_MouseLeave(sender As Object, e As EventArgs) Handles Connect_Text.MouseLeave, Connect_ICO.MouseLeave
        Connect_Box_Sub.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Privacy_Click(sender As Object, e As EventArgs) Handles Connect_Text.Click, Connect_ICO.Click
        ShowNotifier("account_confirm_error")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - REPLAY"

    Private Sub replay_on_MouseMove(sender As Object, e As MouseEventArgs) Handles Replay_Logo.MouseMove, Replay_Text.MouseMove, Replay_Stats.MouseMove
        SetReplayBorder(Not Menu_Replay.Visible)
        Base_Background_Top.b1.Visible = True
    End Sub

    Private Sub replay_on_MouseLeave(sender As Object, e As EventArgs) Handles Replay_Logo.MouseLeave, Replay_Text.MouseLeave, Replay_Stats.MouseLeave
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

    Private Sub replay_on_Click(sender As Object, e As EventArgs) Handles Replay_Logo.Click, Replay_Text.Click, Replay_Stats.Click
        'AMY(Menu_Replay, -200, 3, 150)
        ShadowLoad()
        Menu_Replay.Visible = Not Menu_Replay.Visible
        Menu_Record.Visible = False
        a_1.Visible = Not a_1.Visible
        a_2.Visible = False
        a_3.Visible = False
        SetReplayControlBorder(True)
    End Sub
    Private Sub logo_replay_MouseHover(sender As Object, e As EventArgs) Handles Replay_Logo.MouseHover, Replay_Text.MouseHover, Replay_Stats.MouseHover
        If Base_Background_Top.b2_all.Visible = True Then
            'AMY(Menu_Replay, -200, 3, 150)
            ShadowLoad()
            Menu_Replay.Visible = Not Menu_Replay.Visible
            Menu_Record.Visible = False
            a_1.Visible = Not a_1.Visible
            a_2.Visible = False
            a_3.Visible = False
            SetReplayControlBorder(True)
        End If
    End Sub



#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - RECORD"

    Private Sub logo_record_MouseMove(sender As Object, e As MouseEventArgs) Handles Record_Logo.MouseMove, Record_Text.MouseMove, Record_Stats.MouseMove
        SetRecordBorder(Not Menu_Record.Visible)
        Base_Background_Top.b2.Visible = True
    End Sub

    Private Sub logo_record_MouseLeave(sender As Object, e As EventArgs) Handles Record_Logo.MouseLeave, Record_Text.MouseLeave, Record_Stats.MouseLeave
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

    Private Sub logo_record_Click(sender As Object, e As EventArgs) Handles Record_Logo.Click, Record_Text.Click, Record_Stats.Click
        ' AMY(Menu_Record, -200, 3, 150)
        ShadowLoad()
        Menu_Record.Visible = Not Menu_Record.Visible
        Menu_Replay.Visible = False
        a_2.Visible = True
        a_1.Visible = False
        a_3.Visible = False
        SetReplayControlBorder(True)
    End Sub
    Private Sub logo_record_MouseHover(sender As Object, e As EventArgs) Handles Record_Logo.MouseHover, Record_Text.MouseHover, Record_Stats.MouseHover
        If Base_Background_Top.b1_all.Visible = True Then
            'AMY(Menu_Record, -200, 3, 150)
            ShadowLoad()
            Menu_Record.Visible = Not Menu_Record.Visible
            Menu_Replay.Visible = False
            a_2.Visible = True
            a_1.Visible = False
            a_3.Visible = False
            SetRecordControlBorder(True)
        End If
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - LIVE STREAM"

    Private Sub logo_live_MouseMove(sender As Object, e As MouseEventArgs) Handles Live_Logo.MouseMove, Live_Text.MouseMove, Live_Stats.MouseMove
        a_3.Visible = True
        a_3r.Visible = True
        a_3l.Visible = True
        a_3b.Visible = True
        Base_Background_Top.b3.Visible = True
    End Sub

    Private Sub logo_live_MouseLeave(sender As Object, e As EventArgs) Handles Live_Logo.MouseLeave, Live_Text.MouseLeave, Live_Stats.MouseLeave
        a_3.Visible = False
        a_3r.Visible = False
        a_3l.Visible = False
        a_3b.Visible = False
        Base_Background_Top.b3.Visible = False
        Live_Logo.ForeColor = System.Drawing.Color.White
    End Sub

    Private Sub logo_live_Click(sender As Object, e As EventArgs) Handles Live_Logo.Click, Live_Text.Click, Live_Stats.Click
        ShowNotifier("feature_not_ready")
        Menu_Replay.Visible = False
        a_1.Visible = False
        Menu_Record.Visible = False
        a_2.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - MICROPHONE & VIDEO"

    Private Sub mic_MouseMove(sender As Object, e As MouseEventArgs) Handles MIC_ICO.MouseMove
        MIC_ICO.ForeColor = System.Drawing.Color.Gray
    End Sub

    Private Sub mic_MouseLeave(sender As Object, e As EventArgs) Handles MIC_ICO.MouseLeave
        MIC_ICO.ForeColor = System.Drawing.Color.White
    End Sub
    Public Sub LoadMicState()
        If AppSettings.Instance.Audio.MicEnabled = True Then
            MIC_ICO.Text = ""
        Else
            MIC_ICO.Text = ""
        End If
    End Sub
    Private Sub mic_Click(sender As Object, e As EventArgs) Handles MIC_ICO.Click
        MIC_ICO.Text = If(MIC_ICO.Text = "", "", "")
        If MIC_ICO.Text = "" Then
            AppSettings.Instance.Audio.MicEnabled = True
        Else
            AppSettings.Instance.Audio.MicEnabled = False
        End If
        AppSettings.Instance.Save()

        Debug.WriteLine("Mic Enabled: " & AppSettings.Instance.Audio.MicEnabled)
    End Sub


    Private Sub vdo_MouseMove(sender As Object, e As MouseEventArgs) Handles vdo.MouseMove
        vdo.ForeColor = System.Drawing.Color.Gray
    End Sub

    Private Sub vdo_MouseLeave(sender As Object, e As EventArgs) Handles vdo.MouseLeave
        vdo.ForeColor = System.Drawing.Color.White
    End Sub

    Private Sub vdo_Click(sender As Object, e As EventArgs) Handles vdo.Click
        ShowNotifier("extension_not_found")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - GALLERY"

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
        shadowplay.Visible = False
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Menu_Replay.Visible = False
        Menu_Record.Visible = False
        Base_Gallery.Opacity = 0
        Base_Gallery.Show()


        Dim TIME As New Timer With {.Interval = 20}
        AddHandler TIME.Tick, Sub(s, MIEXXXXXXX)
                                  TIME.Stop()

                                  AMY(Base_Gallery.Base_Submenu, -200, 5, 300)
                                  Base_Gallery.Opacity = 1
                              End Sub
        TIME.Start()

    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - REPLAY CONTROLS"

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
        'a_1.Visible = False
        ToggleInstantReplay()
        'Menu_Replay.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - REPLAY SAVE CONTROLS"

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

#Region "============================================================================ MOUSE EVENT HANDLERS - RECORD CONTROLS"

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

#Region "============================================================================ MOUSE EVENT HANDLERS - PHOTO MODE"

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

#Region "============================================================================ MOUSE EVENT HANDLERS - GAME FILTER"

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

#Region "============================================================================ MOUSE EVENT HANDLERS - UPLOAD/SHARE"

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

#Region "==================== MOUSE EVENT HANDLERS"

    ' ========== SHARED COLORS ==========
    Private ReadOnly grayColor As Color = Color.Gray

    ' ========== ALL FORMS LIST ==========
    Private ReadOnly allForms As Form() = {
        Base_Settings,
        Base_Connect,
        Base_Privacy_Control,
        Base_Overlay_Hub,
        Base_KeySet,
        Base_RecordingsSet
    }

    ' ========== HELPER METHOD ==========
    Private Sub OpenPanel(showForm As Form, settingsCtrl As Control)
        For Each f In allForms
            If f IsNot showForm Then f?.Hide()
        Next
        Settings_List.Visible = False
        showForm.Opacity = 0
        showForm.Show()
        settingsCtrl.Location = New Point(80, 160)

        Dim t As New Timer With {.Interval = 1}

        AddHandler t.Tick, Sub(s, e)
                               t.Stop()

                               showForm.Opacity = 1

                           End Sub

        t.Start()
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
    Private Sub Connect_MouseMove(sender As Object, e As MouseEventArgs) Handles Connect_Text.MouseMove, Connect_ICO.MouseMove
        Connect_Box_Sub.BackColor = greenColor
    End Sub

    Private Sub Connect_MouseLeave(sender As Object, e As EventArgs) Handles Connect_Text.MouseLeave, Connect_ICO.MouseLeave
        Connect_Box_Sub.BackColor = grayColor
    End Sub

    Private Sub Connect_Click(sender As Object, e As EventArgs) Handles Connect_Text.Click, Connect_ICO.Click
        OpenPanel(Base_Connect, Base_Connect.settings_1)
    End Sub

    ' ========== PRIVACY SETTINGS ==========
    Private Sub Saved_MouseMove(sender As Object, e As MouseEventArgs) Handles saved_e.MouseMove, Label4.MouseMove, Label5.MouseMove
        saved_e1.BackColor = greenColor
    End Sub

    Private Sub Saved_MouseLeave(sender As Object, e As EventArgs) Handles saved_e.MouseLeave, Label4.MouseLeave, Label5.MouseLeave
        saved_e1.BackColor = grayColor
    End Sub

    Private Sub Saved_Click(sender As Object, e As EventArgs) Handles saved_e.Click, Label4.Click, Label5.Click
        OpenPanel(Base_Privacy_Control, Base_Privacy_Control.settings_1)
    End Sub

    ' ========== OVERLAY HUB ==========
    Private Sub Hub_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox10.MouseMove, Label12.MouseMove, Label15.MouseMove
        hub.BackColor = greenColor
    End Sub

    Private Sub Hub_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox10.MouseLeave, Label12.MouseLeave, Label15.MouseLeave
        hub.BackColor = grayColor
    End Sub

    Private Sub Hub_Click(sender As Object, e As EventArgs) Handles PictureBox10.Click, Label12.Click, Label15.Click
        OpenPanel(Base_Overlay_Hub, Base_Overlay_Hub.settings_1)
    End Sub

    ' ========== KEYBOARD SHORTCUTS ==========
    Private Sub K1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox11.MouseMove, Label17.MouseMove, Label18.MouseMove
        k1.BackColor = greenColor
    End Sub

    Private Sub K1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox11.MouseLeave, Label17.MouseLeave, Label18.MouseLeave
        k1.BackColor = grayColor
    End Sub

    Private Sub K1_Click(sender As Object, e As EventArgs) Handles PictureBox11.Click, Label17.Click, Label18.Click
        OpenPanel(Base_KeySet, Base_KeySet.keyset)
    End Sub

    ' ========== HIGHLIGHTS ==========
    Private Sub Highlights_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox16.MouseMove, Label21.MouseMove, Label22.MouseMove
        hg2.BackColor = greenColor
    End Sub

    Private Sub Highlights_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox16.MouseLeave, Label21.MouseLeave, Label22.MouseLeave
        hg2.BackColor = grayColor
    End Sub

    Private Sub Highlights_Click(sender As Object, e As EventArgs) Handles PictureBox16.Click, Label21.Click, Label22.Click
        ShowNotifier("feature_not_ready")
    End Sub

    ' ========== VIDEO CAPTURE SETTINGS ==========
    Private Sub vd1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox13.MouseMove, vdo_setme.MouseMove, videoCapture_Text.MouseMove, videoCapture_ICO.MouseMove
        vd1.BackColor = greenColor
    End Sub

    Private Sub vd1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox13.MouseLeave, vdo_setme.MouseLeave, videoCapture_Text.MouseLeave, videoCapture_ICO.MouseLeave
        vd1.BackColor = grayColor
    End Sub



    Public Sub OpenRecordings()
        Menu_Replay.Visible = False
        Menu_Record.Visible = False
        sha1.Hide()
        sha2.Hide()
        sha3.Hide()
        sha4.Hide()

        Dim t As New Timer With {.Interval = 10}
        AddHandler t.Tick, Sub(s, e)
                               t.Stop()

                               OpenSettings()
                           End Sub
        t.Start()

        Dim td As New Timer With {.Interval = 20}
        AddHandler td.Tick, Sub(s, e)
                                td.Stop()

                                OpenPanel(Base_RecordingsSet, Base_RecordingsSet.setret)
                           End Sub
        td.Start()
    End Sub

    Private Sub vd1_Click(sender As Object, e As EventArgs) Handles PictureBox13.Click, vdo_setme.Click, videoCapture_Text.Click, videoCapture_ICO.Click
        OpenPanel(Base_RecordingsSet, Base_RecordingsSet.setret)

    End Sub

    ' ========== NOTIFICATIONS ==========
    Private Sub Noti_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox17.MouseMove, notifications_Text.MouseMove, notifications_ICO.MouseMove
        noy.BackColor = greenColor
    End Sub

    Private Sub Noti_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox17.MouseLeave, notifications_Text.MouseLeave, notifications_ICO.MouseLeave
        noy.BackColor = grayColor
    End Sub

    Private Sub Noti_Click(sender As Object, e As EventArgs) Handles PictureBox17.Click, notifications_Text.Click, notifications_ICO.Click
        If isNotiOn = True Then
            notifications_ICO.ForeColor = Color.White
            notifications_Text.ForeColor = Color.White
            isNotiOn = False
        Else
            notifications_ICO.ForeColor = Color.Gray
            notifications_Text.ForeColor = Color.Gray
            isNotiOn = True
        End If
    End Sub

    ' ========== ABOUT ==========
    Private Sub About_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove, About_Text.MouseMove, Label9.MouseMove
        ab_bg.BackColor = greenColor
    End Sub

    Private Sub About_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave, About_Text.MouseLeave, Label9.MouseLeave
        ab_bg.BackColor = grayColor
    End Sub

#End Region
    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click, About_Text.Click, Label9.Click
        ShowNotifier("feature_not_ready")
    End Sub

End Class