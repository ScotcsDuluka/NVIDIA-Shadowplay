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
        If logo_record.ForeColor = greenColor Then
            Await Task.Delay(1200)
            logo_record.ForeColor = System.Drawing.Color.White
        End If

        If logo_replay.ForeColor = greenColor Then
            Await Task.Delay(1200)
        End If
    End Sub

    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load_App.Tick
        If sub_record.Visible = True Then
            Base_Background_Top.b2_all.Visible = True
        Else
            Base_Background_Top.b2_all.Visible = False
        End If
        If sub_replay.Visible = True Then
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
            s_record.Text = LangHelper.GetText("l10n.recording")
            s_record.ForeColor = greenColor
            logo_record.ForeColor = greenColor
            s_record.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        Else
            Label13.Text = LangHelper.GetText("l10n.start")
            logo_record.ForeColor = System.Drawing.Color.White
            s_record.Text = LangHelper.GetText("l10n.notRecording")
            s_record.ForeColor = System.Drawing.Color.Gray
            s_record.Font = New Font("Segoe UI", 12, FontStyle.Regular)
        End If
    End Sub

    Private Sub UpdateReplayStatus()
        If ReplayValue = True Then
            s_replay.Text = LangHelper.GetText("l10n.on")
            s_replay.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            s_replay.ForeColor = greenColor
            logo_replay.ForeColor = greenColor
            Label8.ForeColor = System.Drawing.Color.White
        Else
            s_replay.Text = LangHelper.GetText("l10n.off")
            s_replay.Font = New Font("Segoe UI", 12, FontStyle.Regular)
            s_replay.ForeColor = System.Drawing.Color.Gray
            logo_replay.ForeColor = System.Drawing.Color.White
            Label8.ForeColor = System.Drawing.Color.Gray
        End If
    End Sub

    Private Sub UpdateMicStatus()
        If mic.Text = "" Then
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

    Private Sub PictureBox19_MouseMove(sender As Object, e As MouseEventArgs) Handles menu_record_sub.MouseMove, menu_record_subkey.MouseMove
        menu_record_subbg.BackColor = greenColor
    End Sub

    Private Sub PictureBox19_MouseLeave(sender As Object, e As EventArgs) Handles menu_record_sub.MouseLeave, menu_record_subkey.MouseLeave
        menu_record_subbg.BackColor = System.Drawing.Color.Black
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

    Private Sub PictureBox24_Click(sender As Object, e As EventArgs) Handles sub_replay_setodv.Click, Label3.Click
        OpenRecordings()
    End Sub

    Private Sub logo_replay_MouseHover(sender As Object, e As EventArgs) Handles logo_replay.MouseHover, replay.MouseHover, s_replay.MouseHover
        If Base_Background_Top.b2_all.Visible = True Then
            AMY(sub_replay, -200, 3, 150)
            sub_replay.Visible = Not sub_replay.Visible
            sub_record.Visible = False
            a_1.Visible = Not a_1.Visible
            a_2.Visible = False
            a_3.Visible = False
            SetReplayControlBorder(True)
        End If
    End Sub

    Private Sub logo_record_MouseHover(sender As Object, e As EventArgs) Handles logo_record.MouseHover, record.MouseHover, s_record.MouseHover
        If Base_Background_Top.b1_all.Visible = True Then
            AMY(sub_record, -200, 3, 150)
            sub_record.Visible = Not sub_record.Visible
            sub_replay.Visible = False
            a_2.Visible = True
            a_1.Visible = False
            a_3.Visible = False
            SetRecordControlBorder(True)
        End If
    End Sub

#End Region

End Class