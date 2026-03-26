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

    Private Sub Screenshot_Click(sender As Object, e As EventArgs) Handles Logo_Mode1.Click, Bg_Mode1.Click, Text_Mode1.Click
        CaptureScreen()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - SETTINGS"

    Private Sub set_to_Click(sender As Object, e As EventArgs) Handles Settings_Logo.Click
        OpenSettings()
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

    Private Sub Privacy_MouseMove(sender As Object, e As MouseEventArgs) Handles box_py.MouseMove, text_py.MouseMove, logo_py.MouseMove
        bg_py.BackColor = greenColor
    End Sub

    Private Sub Privacy_MouseLeave(sender As Object, e As EventArgs) Handles box_py.MouseLeave, text_py.MouseLeave, logo_py.MouseLeave
        bg_py.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Privacy_Click(sender As Object, e As EventArgs) Handles box_py.Click, text_py.Click, logo_py.Click
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
        Replay_Logo.ForeColor = If(ReplayActive, greenColor, System.Drawing.Color.White)
        Base_Background_Top.b1.Visible = False
    End Sub

    Private Sub SetReplayBorder(isVisible As Boolean)
        a_1.Visible = Menu_Replay.Visible OrElse isVisible
        a_1r.Visible = isVisible
        a_1l.Visible = isVisible
        a_1b.Visible = isVisible
    End Sub

    Private Sub replay_on_Click(sender As Object, e As EventArgs) Handles Replay_Logo.Click, Replay_Text.Click, Replay_Stats.Click
        AMY(Menu_Replay, -200, 3, 150)
        Menu_Replay.Visible = Not Menu_Replay.Visible
        sub_record.Visible = False
        a_1.Visible = Not a_1.Visible
        a_2.Visible = False
        a_3.Visible = False
        SetReplayControlBorder(True)
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - RECORD"

    Private Sub logo_record_MouseMove(sender As Object, e As MouseEventArgs) Handles Record_Logo.MouseMove, Record_Text.MouseMove, Record_Stats.MouseMove
        SetRecordBorder(Not sub_record.Visible)
        Base_Background_Top.b2.Visible = True
    End Sub

    Private Sub logo_record_MouseLeave(sender As Object, e As EventArgs) Handles Record_Logo.MouseLeave, Record_Text.MouseLeave, Record_Stats.MouseLeave
        SetRecordBorder(False)
        Base_Background_Top.b2.Visible = False
        If Record_Logo.ForeColor = greenColor OrElse Record_Logo.ForeColor = ColorTranslator.FromHtml("#426800") Then
            Record_Logo.ForeColor = greenColor
        Else
            Record_Logo.ForeColor = System.Drawing.Color.White
        End If
    End Sub

    Private Sub SetRecordBorder(isVisible As Boolean)
        a_2.Visible = sub_record.Visible OrElse isVisible
        a_2r.Visible = isVisible
        a_2l.Visible = isVisible
        a_2b.Visible = isVisible
    End Sub

    Private Sub logo_record_Click(sender As Object, e As EventArgs) Handles Record_Logo.Click, Record_Text.Click, Record_Stats.Click
        AMY(sub_record, -200, 3, 150)
        sub_record.Visible = Not sub_record.Visible
        Menu_Replay.Visible = False
        a_2.Visible = True
        a_1.Visible = False
        a_3.Visible = False
        SetReplayControlBorder(True)
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
        sub_record.Visible = False
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
        sub_record.Visible = False
        AMY(Base_Gallery.Base_Submenu, -200, 5, 300)
        Base_Gallery.Show()
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
        a_1.Visible = False
        ToggleInstantReplay()
        Menu_Replay.Visible = False
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
        a_1.Visible = False
        Menu_Replay.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - RECORD CONTROLS"

    Private Sub RecordControl_MouseMove(sender As Object, e As MouseEventArgs) Handles sh_record.MouseMove, PictureBox5.MouseMove, Label13.MouseMove
        SetRecordControlBorder(True)
    End Sub

    Private Sub RecordControl_MouseLeave(sender As Object, e As EventArgs) Handles sh_record.MouseLeave, PictureBox5.MouseLeave, Label13.MouseLeave
        SetRecordControlBorder(False)
    End Sub

    Private Sub SetRecordControlBorder(isVisible As Boolean)
        st1.Visible = isVisible
        str.Visible = isVisible
        stl.Visible = isVisible
        stb.Visible = isVisible
    End Sub

    Private Sub RecordControl_Click(sender As Object, e As EventArgs) Handles sh_record.Click, PictureBox5.Click, Label13.Click
        a_2.Visible = False
        ToggleRecording()
        sub_record.Visible = False
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

    Private Sub Photo_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode2.MouseMove, Text_Mode2.MouseMove, Bg_Mode2.MouseMove
        SetPhotoBorder(True)
        Base_Background_Top.Bg_Mode2.Visible = True
    End Sub

    Private Sub Photo_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode2.MouseLeave, Text_Mode2.MouseLeave, Bg_Mode2.MouseLeave
        SetPhotoColors(System.Drawing.Color.White)
        SetPhotoBorder(False)
        Base_Background_Top.Bg_Mode2.Visible = False
    End Sub

    Private Sub Photo_Click(sender As Object, e As EventArgs) Handles Bg_Mode2.Click, Text_Mode2.Click, Logo_Mode2.Click
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

    Private Sub GameFilter_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode3.MouseMove, Text_Mode3.MouseMove, Bg_Mode3.MouseMove
        SetGameBorder(True)
    End Sub

    Private Sub GameFilter_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode3.MouseLeave, Text_Mode3.MouseLeave, Bg_Mode3.MouseLeave
        SetGameColors(System.Drawing.Color.White)
        SetGameBorder(False)
    End Sub

    Private Sub GameFilter_Click(sender As Object, e As EventArgs) Handles Logo_Mode3.Click, Text_Mode3.Click, Bg_Mode3.Click
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

#Region "============================================================================ MOUSE EVENT HANDLERS - SETTINGS PANEL"

    Private Sub VS_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox6.MouseMove, Label10.MouseMove
        'SetVSBorder(True)
    End Sub

    Private Sub VS_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox6.MouseLeave, Label10.MouseLeave
        'SetVSBorder(False)
    End Sub

    Private Sub VS_Click(sender As Object, e As EventArgs) Handles PictureBox6.Click, Label10.Click
        OpenRecordings()
    End Sub

    Private Sub SetS1Border(isVisible As Boolean)
        s1.Visible = isVisible
        s1r.Visible = isVisible
        s1l.Visible = isVisible
        s1b.Visible = isVisible
    End Sub

    Private Sub Settings_MouseMove(sender As Object, e As MouseEventArgs) Handles Settings_Logo.MouseMove, Settings_Box.MouseMove, Settings_Text.MouseMove
        Base_Background_Top.Bg_SET3.Visible = True
        SetS1Border(True)
    End Sub

    Private Sub Settings_MouseLeave(sender As Object, e As EventArgs) Handles Settings_Logo.MouseLeave, Settings_Box.MouseLeave, Settings_Text.MouseLeave
        Base_Background_Top.Bg_SET3.Visible = False
        SetS1Border(False)
    End Sub

    Private Sub Settings_Click(sender As Object, e As EventArgs) Handles Settings_Box.Click, Settings_Text.Click
        OpenSettings()
    End Sub

    Private Sub OpenSettings()
        Base_Settings.Show()
        AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base_Background_Top.Bg_SET3.Visible = False
        ME_CLOSE_BG.Visible = False
        d.Visible = False
        clickThrough = True
        Opacity = 1
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        settings_1.Visible = True
        SET_Back.Visible = False
        shadowplay.Visible = False
        Menu_Replay.Visible = False
        sub_record.Visible = False
    End Sub

    Private Sub OpenRecordings()
        OpenSettings()
        Base_Settings.Hide()
        Base_Privacy_Control.Hide()
        Base_Overlay_Hub.Hide()
        Base_KeySet.Hide()
        Base_RecordingsSet.setre.Location = New Point(695, -2000)
        Base_RecordingsSet.Show()
        AMY(Base_RecordingsSet.setre, -2000, 160, 300)
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - PRIVACY SETTINGS"

    Private Sub Saved_MouseMove(sender As Object, e As MouseEventArgs) Handles saved_e.MouseMove, Label4.MouseMove, Label5.MouseMove
        saved_e1.BackColor = greenColor
    End Sub

    Private Sub Saved_MouseLeave(sender As Object, e As EventArgs) Handles saved_e.MouseLeave, Label4.MouseLeave, Label5.MouseLeave
        saved_e1.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Saved_Click(sender As Object, e As EventArgs) Handles saved_e.Click, Label4.Click, Label5.Click
        Base_Settings.Hide()
        Base_Overlay_Hub.Hide()
        Base_KeySet.Hide()
        Base_RecordingsSet.Hide()
        Base_Privacy_Control.settings_1.Location = New Point(695, -2000)
        Base_Privacy_Control.Show()
        AMY(Base_Privacy_Control.settings_1, -2000, 160, 300)
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - OVERLAY HUB"

    Private Sub Hub_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox10.MouseMove, Label12.MouseMove, Label15.MouseMove
        hub.BackColor = greenColor
    End Sub

    Private Sub Hub_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox10.MouseLeave, Label12.MouseLeave, Label15.MouseLeave
        hub.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Hub_Click(sender As Object, e As EventArgs) Handles PictureBox10.Click, Label12.Click, Label15.Click
        Base_Settings.Hide()
        Base_KeySet.Hide()
        Base_Privacy_Control.Hide()
        Base_RecordingsSet.Hide()
        Base_Overlay_Hub.settings_1.Location = New Point(695, -2000)
        Base_Overlay_Hub.Show()
        AMY(Base_Overlay_Hub.settings_1, -2000, 160, 300)
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - KEYBOARD SHORTCUTS"

    Private Sub K1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox11.MouseMove, Label17.MouseMove, Label18.MouseMove
        k1.BackColor = greenColor
    End Sub

    Private Sub K1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox11.MouseLeave, Label17.MouseLeave, Label18.MouseLeave
        k1.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub K1_Click(sender As Object, e As EventArgs) Handles PictureBox11.Click, Label17.Click, Label18.Click
        Base_Settings.Hide()
        Base_RecordingsSet.Hide()
        Base_Privacy_Control.Hide()
        Base_Overlay_Hub.Hide()
        Base_KeySet.keyset.Location = New Point(695, -2000)
        Base_KeySet.Show()
        AMY(Base_KeySet.keyset, -2000, 160, 300)
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - HIGHLIGHTS"

    Private Sub Highlights_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox16.MouseMove, Label21.MouseMove, Label22.MouseMove
        hg2.BackColor = greenColor
    End Sub

    Private Sub Highlights_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox16.MouseLeave, Label21.MouseLeave, Label22.MouseLeave
        hg2.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Highlights_Click(sender As Object, e As EventArgs) Handles PictureBox16.Click, Label21.Click, Label22.Click
        ShowNotifier("feature_not_ready")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - VIDEO CAPTURE SETTINGS"

    Private Sub vd1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox13.MouseMove, vdo_setme.MouseMove, Label19.MouseMove, Label20.MouseMove
        vd1.BackColor = greenColor
    End Sub

    Private Sub vd1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox13.MouseLeave, vdo_setme.MouseLeave, Label19.MouseLeave
        vd1.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub vd1_Click(sender As Object, e As EventArgs) Handles PictureBox13.Click, vdo_setme.Click, Label19.Click, Label20.Click
        Base_Settings.Hide()
        Base_Privacy_Control.Hide()
        Base_Overlay_Hub.Hide()
        Base_KeySet.Hide()
        Base_RecordingsSet.setre.Location = New Point(695, -2000)
        Base_RecordingsSet.Show()
        AMY(Base_RecordingsSet.setre, -2000, 160, 300)
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - NOTIFICATIONS"

    Private Sub Noti_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox17.MouseMove, nott.MouseMove, noty.MouseMove
        noy.BackColor = greenColor
    End Sub

    Private Sub Noti_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox17.MouseLeave, nott.MouseLeave
        noy.BackColor = System.Drawing.Color.Gray
    End Sub

    Private Sub Noti_Click(sender As Object, e As EventArgs) Handles PictureBox17.Click, nott.Click, noty.Click
        isNotiOn = Not isNotiOn
        Dim targetColor As System.Drawing.Color = If(isNotiOn, System.Drawing.Color.White, System.Drawing.Color.Gray)
        noty.ForeColor = targetColor
        nott.ForeColor = targetColor
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - ABOUT"

    Private Sub About_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove, Label6.MouseMove, Label9.MouseMove
        ab_bg.BackColor = greenColor
    End Sub

    Private Sub About_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave, Label6.MouseLeave, Label9.MouseLeave
        ab_bg.BackColor = System.Drawing.Color.Gray
    End Sub

#End Region

End Class