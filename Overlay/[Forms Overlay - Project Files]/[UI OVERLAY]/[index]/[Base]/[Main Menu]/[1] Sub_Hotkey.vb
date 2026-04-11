Imports System.Drawing
Partial Public Class Base

#Region "Hotkey Event Handlers"

    Private Sub OnAltZ() Handles _hotkeyService.AltZPressed
        If Settings_List.Visible Then Return
        If Base_Gallery.Visible Then Return


        isFunctionActive_f3 = False

        If shadowplay.Visible = True Then
            HideAllControls()
            shadowplay.Visible = False
        Else
            ShowMainPanel()
            shadowplay.Visible = True
            Base_Game_Filter_Sub.Opacity = 0
            Base_Game_Filter.Opacity = 0
            Base_Game_Filter.Hide()
            Base_Game_Filter_Sub.Hide()
        End If
    End Sub

    Private Sub OnAltF1() Handles _hotkeyService.AltF1Pressed
        If Settings_List.Visible Then Return
        CaptureScreen()
    End Sub

    Private Sub OnAltF9() Handles _hotkeyService.AltF9Pressed
        If Settings_List.Visible Then Return
        ToggleRecording()
    End Sub

    Private Sub OnAltShiftF10() Handles _hotkeyService.AltShiftF10Pressed
        If Settings_List.Visible Then Return
        ToggleInstantReplay()
    End Sub

    Private Sub OnAltF10() Handles _hotkeyService.AltF10Pressed
        If Settings_List.Visible Then Return
        SaveInstantReplay()
    End Sub

    Private Sub OnAltF3() Handles _hotkeyService.AltF3Pressed
        If Settings_List.Visible Then Return
        ToggleGameFilter()
    End Sub

    Private Sub OnAltF2() Handles _hotkeyService.AltF2Pressed
        If Settings_List.Visible Then Return
        ShowNotifier("notificationWarningNvidiaGpuRequired")
    End Sub

    Private Sub OnAltF8() Handles _hotkeyService.AltF8Pressed
        If Settings_List.Visible Then Return
        ShowNotifier("feature_not_ready")
    End Sub

    Private Sub OnAltF12() Handles _hotkeyService.AltF12Pressed
        If Settings_List.Visible Then Return
        ShowNotifier("feature_not_ready")
    End Sub

    Private Sub OnAltT() Handles _hotkeyService.AltTPressed
        If Settings_List.Visible Then Return
        ShowNotifier("notificationOpenShare")
    End Sub

#End Region

#Region "============================================================================ (เดิมคือ KEYBOARD DETECTION)"

    Public Sub HideAllControls()
        Me.SuspendLayout()

        isFunctionActive = False
        Me.Opacity = 0
        Base_Background.Opacity = 0
        Base_Background_Top.Opacity = 0
        Menu_Replay.Visible = False
        Menu_Record.Visible = False
        Settings_List.Visible = False
        shadowplay.Visible = True
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Base_Background.Hide()
        Base_Background_Top.Hide()
        Me.Hide()

        Me.ResumeLayout(True)
    End Sub

    Public Sub ShowMainPanel()
        Me.SuspendLayout()

        isFunctionActive = True
        Base_Background.Show()
        Base_Background_Top.Show()
        Base_Background_Top.TopMost = True
        Me.Show()
        Me.TopMost = True
        Base_Background.Opacity = 0.5
        Base_Background_Top.Opacity = 1
        Me.Opacity = 0.85

        Me.ResumeLayout(True)
    End Sub

    Private Sub ToggleGameFilter()
        Base_Game_Filter.Main_Filter.Location = New Point(-500, 0)
        Base_Game_Filter_Sub.BG.Location = New Point(-500, 0)
        If isFunctionActive_f3 = False Then
            isFunctionActive_f3 = True
            isFunctionActive = False
            ShowNotifier("notificationWarningNvidiaGpuRequired")
            HideAllControls()
            Base_Game_Filter_Sub.Show()
            Base_Game_Filter_Sub.Opacity = 0.78
            Base_Game_Filter.Show()
            Base_Game_Filter.Opacity = 1
        Else
            isFunctionActive_f3 = False
            Base_Game_Filter_Sub.Opacity = 0
            Base_Game_Filter.Opacity = 0
            Base_Game_Filter.Hide()
            Base_Game_Filter_Sub.Hide()
        End If
    End Sub

#End Region

End Class