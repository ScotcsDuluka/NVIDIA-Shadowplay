Imports System.Drawing
Imports System.IO

Partial Public Class Base

    Private Sub SetControlColor(ctrl As Control, color As Color)
        If ctrl IsNot Nothing Then
            If ctrl.InvokeRequired Then
                ctrl.Invoke(Sub() ctrl.ForeColor = color)
            Else
                ctrl.ForeColor = color
            End If
        End If
    End Sub

    Private Sub SetControlEnabled(ctrl As Control, enabled As Boolean)
        If ctrl IsNot Nothing Then
            If ctrl.InvokeRequired Then
                ctrl.Invoke(Sub() ctrl.Enabled = enabled)
            Else
                ctrl.Enabled = enabled
            End If
        End If
    End Sub

    Private Sub SetControlText(ctrl As Control, text As String)
        If ctrl IsNot Nothing Then
            If ctrl.InvokeRequired Then
                ctrl.Invoke(Sub() ctrl.Text = text)
            Else
                ctrl.Text = text
            End If
        End If
    End Sub

#Region "============================================================================ TIMER EVENT HANDLERS"

    Private Async Sub CheckStatus()
        If Record_Logo.ForeColor = greenColor Then
            Await Task.Delay(1200)
            Record_Logo.ForeColor = System.Drawing.Color.White
        End If

        If Replay_Logo.ForeColor = greenColor Then
            Await Task.Delay(1200)
        End If
    End Sub

    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load_App.Tick
        If sub_record.Visible = True Then
            Base_Background_Top.b2_all.Visible = True
        Else
            Base_Background_Top.b2_all.Visible = False
        End If
        If Menu_Replay.Visible = True Then
            Base_Background_Top.b1_all.Visible = True
        Else
            Base_Background_Top.b1_all.Visible = False
        End If

        UpdateReplayStatus()
        UpdateRecordStatus()
        UpdateMicStatus()
        Dim filePaths As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier_main")

        Try
            ' If 'Notifier.Visible Then
            '   If Not File.Exists(filePaths) Then
            'File.Create(filePaths).Dispose()
            '  End If
            '  Else
            '     If File.Exists(filePaths) Then
            'File.Delete(filePaths)
            ' End If
            '    End If
        Catch ex As Exception
        End Try
    End Sub
    Public PrivacyValue As Boolean = False

    Private Sub hg1_Tick(sender As Object, e As EventArgs) Handles hg1.Tick
        If My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, DataDirectoryName, "save")) Then
            hg1.Stop()
            Return
        End If

        Using bmpScreenshot As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
            Using g As Graphics = Graphics.FromImage(bmpScreenshot)
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)
            End Using

            Dim targetColor As System.Drawing.Color = ColorTranslator.FromHtml("#ACB22E")

            For x As Integer = 0 To bmpScreenshot.Width - 1
                For y As Integer = 0 To bmpScreenshot.Height - 1
                    If bmpScreenshot.GetPixel(x, y) = targetColor Then
                        If Not My.Computer.FileSystem.FileExists(Application.StartupPath & DataDirectoryName & "/save") Then
                            ShowNotifier("saved_last_15")
                            hg1.Stop()
                        End If
                        Exit Sub
                    End If
                Next
            Next
        End Using
    End Sub

    Private Sub not_save_Tick(sender As Object, e As EventArgs) Handles not_save.Tick
        File.Delete(Path.Combine(Application.StartupPath, DataDirectoryName, "save"))
        hg1.Start()
    End Sub

#End Region

#Region "============================================================================ STATUS UPDATE METHODS"

    Private Sub UpdateRecordStatus()
        If RecordValue = True Then
            Label13.Text = LangHelper.GetText("l10n.stopAndSave")
            Record_Stats.Text = LangHelper.GetText("l10n.recording")
            Record_Stats.ForeColor = greenColor
            Record_Logo.ForeColor = greenColor
            Record_Stats.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            icon_record.Text = ""
        Else
            Label13.Text = LangHelper.GetText("l10n.start")
            Record_Logo.ForeColor = System.Drawing.Color.White
            Record_Stats.Text = LangHelper.GetText("l10n.notRecording")
            Record_Stats.ForeColor = System.Drawing.Color.Gray
            Record_Stats.Font = New Font("Segoe UI", 12, FontStyle.Regular)
            icon_record.Text = ""
        End If
    End Sub

    Private Sub UpdateReplayStatus()
        If ReplayValue = True Then
            Replay_Stats.Text = LangHelper.GetText("l10n.on")
            Menu_Replay_text.Text = LangHelper.GetText("l10n.instantReplayStop")
            Replay_Stats.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            Replay_Stats.ForeColor = greenColor
            Replay_Logo.ForeColor = greenColor
            Menu_Replay_save_ico.ForeColor = System.Drawing.Color.White
            Menu_Replay_ico.Text = ""
        Else
            Replay_Stats.Text = LangHelper.GetText("l10n.off")
            Menu_Replay_text.Text = LangHelper.GetText("l10n.instantReplayStart")
            Replay_Stats.Font = New Font("Segoe UI", 12, FontStyle.Regular)
            Replay_Stats.ForeColor = System.Drawing.Color.Gray
            Replay_Logo.ForeColor = System.Drawing.Color.White
            Menu_Replay_save_ico.ForeColor = System.Drawing.Color.Gray
            Menu_Replay_ico.Text = ""
        End If
    End Sub

    Private Sub UpdateMicStatus()
        If MIC_ICO.Text = "" Then
            AppSettings.Instance.Audio.MicEnabled = True
        Else
            AppSettings.Instance.Audio.MicEnabled = False
        End If
    End Sub

#End Region

#Region "============================================================================ GAME DETECTION"

    Private Sub GAMES_IN_Tick(sender As Object, e As EventArgs) Handles GAMES_IN.Tick
        Static targetGames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "minecraft", "javaw", "robloxplayerbeta", "robloxcrashhandler", "java",
            "crashhandler", "gta5", "hd-player", "a dance of fire and ice", "aot",
            "aot2_as", "iw5mp", "iw5sp", "obscure", "genshinimpact", "gta5_enhanced",
            "dwrg", "dungeons", "minecraftlegends.windows", "secret neighbour",
            "smash_legends", "asphalt9_steam_x64_rtl", "furmark_gui", "misidefull", "miside zero", "HSHO-Win64-Shipping"
        }

        Dim isGameRunning As Boolean = Process.GetProcesses().Any(Function(p) targetGames.Contains(p.ProcessName))

        If isGameRunning AndAlso Not notifierShown Then
            ShowNotifier("notificationOpenShare")
            notifierShown = True
        ElseIf Not isGameRunning Then
            notifierShown = False
        End If
    End Sub

#End Region

#Region "============================================================================ MISC EVENT HANDLERS"

    Private Sub Logo_Click(sender As Object, e As EventArgs) Handles Logo.Click
        ShowNotifier("notificationOpenShare")
    End Sub

    Private Sub Logo_text_DoubleClick(sender As Object, e As EventArgs)
        Application.Restart()
    End Sub

    Private Sub Base_Click(sender As Object, e As EventArgs) Handles MyBase.Click
        HandleGalleryDisplay()
    End Sub

    Private Sub HandleGalleryDisplay()
        If Base_Gallery.settings_1.Visible = True Then
            Base_Gallery.Show()
            Base_Gallery.TopMost = True
        End If
    End Sub

    Private Sub save_sc_Click(sender As Object, e As EventArgs)
        Using folderDlg As New FolderBrowserDialog
            folderDlg.Description = "Select the folder to save the capture."
            If folderDlg.ShowDialog = DialogResult.OK Then
                Base_Gallery.txtFilePath.Text = folderDlg.SelectedPath
                AppSettings.Instance.Paths.GalleryPath = folderDlg.SelectedPath
                AppSettings.Instance.Save()
            End If
        End Using
    End Sub
    Private Sub ME_CLOSE_BG_MouseMove(sender As Object, e As MouseEventArgs) Handles ME_CLOSE_BG.MouseMove, d.MouseMove
        Base_Background_Top.ME_CLOSE_BG_GRE.BackColor = greenColor
    End Sub

    Private Sub ME_CLOSE_BG_MouseLeave(sender As Object, e As EventArgs) Handles ME_CLOSE_BG.MouseLeave, d.MouseLeave
        Base_Background_Top.ME_CLOSE_BG_GRE.BackColor = System.Drawing.Color.Black
    End Sub

    Private Sub ME_CLOSE_BG_Click(sender As Object, e As EventArgs) Handles ME_CLOSE_BG.Click, d.Click
        HideAllControls()
    End Sub

    Private Sub PictureBox24_Click(sender As Object, e As EventArgs) Handles Menu_Replay_Box3.Click, Menu_Replay_Sttings_text.Click
        OpenRecordings()
    End Sub

    Private Sub logo_replay_MouseHover(sender As Object, e As EventArgs) Handles Replay_Logo.MouseHover, Replay_Text.MouseHover, Replay_Stats.MouseHover
        If Base_Background_Top.b2_all.Visible = True Then
            AMY(Menu_Replay, -200, 3, 150)
            Menu_Replay.Visible = Not Menu_Replay.Visible
            sub_record.Visible = False
            a_1.Visible = Not a_1.Visible
            a_2.Visible = False
            a_3.Visible = False
            SetReplayControlBorder(True)
        End If
    End Sub

    Private Sub logo_record_MouseHover(sender As Object, e As EventArgs) Handles Record_Logo.MouseHover, Record_Text.MouseHover, Record_Stats.MouseHover
        If Base_Background_Top.b1_all.Visible = True Then
            AMY(sub_record, -200, 3, 150)
            sub_record.Visible = Not sub_record.Visible
            Menu_Replay.Visible = False
            a_2.Visible = True
            a_1.Visible = False
            a_3.Visible = False
            SetRecordControlBorder(True)
        End If
    End Sub

#End Region

End Class