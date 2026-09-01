Imports System.Drawing
Imports System.IO

Partial Public Class Base
    Private ReadOnly _statusFontRegular As New Font("Segoe UI", 12, FontStyle.Regular)
    Private ReadOnly _statusFontBold As New Font("Segoe UI", 12, FontStyle.Bold)
    Private ReadOnly _scanTargetColor As Color = ColorTranslator.FromHtml("#ACB22E")
    Private _lastRecordValue As Boolean? = Nothing
    Private _lastReplayValue As Boolean? = Nothing
    Private _lastMicEnabled As Boolean? = Nothing
    Private _lastPixelScanUtc As DateTime = DateTime.MinValue
    Private Const PixelScanCooldownMs As Integer = 250
    Private Const PixelSampleStride As Integer = 6

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

    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load_App.Tick


        UpdateReplayStatus()
        UpdateRecordStatus()
        UpdateMicStatus()
        Dim filePaths As String = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")

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

    Public Sub RefreshRuntimeStatusTexts()
        UpdateReplayStatus(True)
        UpdateRecordStatus(True)
        UpdateMicStatus(True)
    End Sub

#End Region

#Region "============================================================================ STATUS UPDATE METHODS"
    Private Function ContainsTargetColorSampled(image As Bitmap, targetColor As Color, stride As Integer) As Boolean
        Dim safeStride As Integer = Math.Max(1, stride)
        For x As Integer = 0 To image.Width - 1 Step safeStride
            For y As Integer = 0 To image.Height - 1 Step safeStride
                If image.GetPixel(x, y) = targetColor Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function

    Private Function LStatus(key As String, ParamArray args() As String) As String
        Return LangHelper.GetText(key, args)
    End Function

    Private Sub ApplyCaptureStatus(
        isActive As Boolean,
        statusControl As Control,
        logoControl As Control,
        actionTextControl As Control,
        actionIconControl As Control,
        activeStatusKey As String,
        inactiveStatusKey As String,
        activeActionKey As String,
        inactiveActionKey As String)

        statusControl.Text = If(isActive, LStatus(activeStatusKey), LStatus(inactiveStatusKey))
        actionTextControl.Text = If(isActive, LStatus(activeActionKey), LStatus(inactiveActionKey))

        statusControl.Font = If(isActive, _statusFontBold, _statusFontRegular)
        statusControl.ForeColor = If(isActive, greenColor, Color.Gray)
        logoControl.ForeColor = If(isActive, greenColor, Color.White)
        actionIconControl.Text = If(isActive, "", "")
    End Sub

    Private Sub UpdateRecordStatus(Optional force As Boolean = False)
        If Not force AndAlso _lastRecordValue.HasValue AndAlso _lastRecordValue.Value = RecordValue Then
            Return
        End If

        ApplyCaptureStatus(
            RecordValue,
            Record_Stats,
            Record_Logo,
            Menu_Record_text,
            Menu_Record_ico,
            "l10n.recording",
            "l10n.notRecording",
            "l10n.stopAndSave",
            "l10n.start")

        _lastRecordValue = RecordValue
    End Sub

    Private Sub UpdateReplayStatus(Optional force As Boolean = False)
        If Not force AndAlso _lastReplayValue.HasValue AndAlso _lastReplayValue.Value = ReplayValue Then
            Return
        End If

        ApplyCaptureStatus(
            ReplayValue,
            Replay_Stats,
            Replay_Logo,
            Menu_Replay_text,
            Menu_Replay_ico,
            "l10n.on",
            "l10n.off",
            "l10n.instantReplayStop",
            "l10n.instantReplayStart")

        Menu_Replay_save_ico.ForeColor = If(ReplayValue, Color.White, Color.Gray)
        _lastReplayValue = ReplayValue
    End Sub

    ' PHASE 3 UI CONTRACT (spec section 10 violation #3): config.json
    ' Audio.MicEnabled is the ONLY state source. The old body derived the
    ' config value FROM the icon glyph text and wrote it back — classic
    ' UI-guessed-state (manufactured state). Now this method is display-sync
    ' only: the glyph is re-derived from the canonical bool via LoadMicState
    ' (Sub_Mouse.vb) and config is NEVER written here.
    Private Sub UpdateMicStatus(Optional force As Boolean = False)
        Dim micEnabledNow As Boolean = AppSettings.Instance.Audio.MicEnabled

        If Not force AndAlso _lastMicEnabled.HasValue AndAlso _lastMicEnabled.Value = micEnabledNow Then
            Return
        End If

        LoadMicState()
        _lastMicEnabled = micEnabledNow
    End Sub

#End Region

#Region "============================================================================ GAME DETECTION"

    Private Sub GAMES_IN_Tick(sender As Object, e As EventArgs) Handles GAMES_IN.Tick
        Static targetGames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "minecraft", "javaw", "robloxplayerbeta", "robloxcrashhandler", "java",
            "crashhandler", "gta5", "hd-player", "a dance of fire and ice", "aot",
            "aot2_as", "iw5mp", "iw5sp", "obscure", "genshinimpact", "gta5_enhanced",
            "dwrg", "dungeons", "minecraftlegends.windows", "secret neighbour",
            "smash_legends", "asphalt9_steam_x64_rtl", "furmark_gui", "misidefull", "miside zero", "HSHO-Win64-Shipping" ,"re9","re4"
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

    ' ✅ PHASE 3 UI CONTRACT (UI spec §14.3 — Paths unify, one picker per path):
    ' the dead save_sc_Click that used to live here was REMOVED. It had NO
    ' Handles clause (never invoked — the live handler is
    ' [Gallery]/[1] Main.vb save_sc_Click) and duplicated the GalleryPath
    ' writer. Paths.GalleryPath now has exactly ONE UI writer:
    ' Base_Gallery.save_sc_Click ([Gallery]/[1] Main.vb:54-65).
    Private Sub ME_CLOSE_BG_MouseMove(sender As Object, e As MouseEventArgs) Handles ME_CLOSE_BG.MouseMove
        Base_Background_Top.ME_CLOSE_BG_GRE.BackColor = greenColor
    End Sub

    Private Sub ME_CLOSE_BG_MouseLeave(sender As Object, e As EventArgs) Handles ME_CLOSE_BG.MouseLeave
        Base_Background_Top.ME_CLOSE_BG_GRE.BackColor = System.Drawing.Color.Black
    End Sub

    Private Sub ME_CLOSE_BG_Click(sender As Object, e As EventArgs) Handles ME_CLOSE_BG.Click
        HideAllControls()
    End Sub



#End Region


#Region "Privacy State (Phase 5: moved from Sub_Record.vb)"

    ''' <summary>
    ''' Phase 5: Relocated from Sub_Record.vb.
    ''' Opens the Privacy Control settings panel. Belongs here (UI navigation
    ''' for the Privacy toggle) rather than inside the Record toggle module.
    ''' </summary>
    Private Sub PrivacyOpen()
        OpenPanel(Base_Privacy_Control, Base_Privacy_Control.settings_1)
    End Sub

    ''' <summary>
    ''' Phase 5: Relocated from Sub_Record.vb.
    ''' Returns True when the user has explicitly opted in to desktop capture
    ''' (config.json Privacy.DesktopCaptureEnabled; previously the
    ''' Data/NVIDIA_Shadowplay_Data/privacy marker file — see
    ''' AppSettings.MigrateLegacyMarkerFiles for the one-time import).
    ''' </summary>
    Private Function IsPrivacyEnabled() As Boolean
        Return AppSettings.Instance.Privacy.DesktopCaptureEnabled
    End Function

#End Region

End Class