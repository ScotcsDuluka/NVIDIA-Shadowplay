Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports Newtonsoft.Json.Linq
Public Class Base_Settings
    Inherits System.Windows.Forms.Form
    Private Const LanguageFolderName As String = "Languages"
    Private Const CurrentLanguageFileName As String = "current.txt"
    Private Const DefaultLanguageCode As String = "en-US"

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If m.Msg = WM_NCHITTEST Then
            m.Result = CType(HTTRANSPARENT, IntPtr)
            Return
        End If

        MyBase.WndProc(m)
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub

    Private Async Sub ch_Click(sender As Object, e As EventArgs) Handles ch.Click
        ch.Enabled = False
        Await CheckForUpdateAsync()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click

        Me.Hide()
        Base.ME_CLOSE_BG.Visible = True
        Base.Opacity = 0
        Base.Settings_List.Visible = False
        Base.shadowplay.Visible = True


        Base_Background_Top.d.Visible = True
        Base_Background_Top.ME_CLOSE_BG_GRE.Visible = True
        Base_Background_Top.ME_CLOSE_BG.Visible = True


        AppSettings.Instance.Save()

        Dim TIME As New Timer With {.Interval = 20}
        AddHandler TIME.Tick, Sub(s, MIEXXXXXXX)
                                  TIME.Stop()
                                  Base.ShowMainPanel()
                                  Base.Opacity = 0.85
                                  Base.IF_OpenShare = True
                              End Sub
        TIME.Start()

    End Sub

    Private Sub Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        ToggleUseWindowsSnip.IsOn = AppSettings.Instance.UI.UseWindowsSnip
    End Sub
    Private Sub ToggleUseWindowsSnip_ValueChanged(sender As Object, e As EventArgs) Handles ToggleUseWindowsSnip.ValueChanged
        AppSettings.Instance.UI.UseWindowsSnip = ToggleUseWindowsSnip.IsOn
        AppSettings.Instance.Save()
    End Sub
    Private Sub Back_btn_Click(sender As Object, e As EventArgs)
        Hide()
    End Sub
    Private _delayTimer As Timer
    Private Sub RefreshAllControls(parent As Control)
        parent.Refresh()
        For Each ctrl As Control In parent.Controls
            ctrl.Refresh()
            If ctrl.HasChildren Then
                RefreshAllControls(ctrl)
            End If
        Next
    End Sub
    Private Sub SW_lang_Click(sender As Object, e As EventArgs) Handles SW_lang.Click
        Dim langFolder As String = GetLanguageFolderPath()
        Dim currentLang As String = GetCurrentLanguageCode(langFolder)
        Dim files = Directory.GetFiles(langFolder, "*.json")
        If files.Length = 0 Then Return

        Dim cms As ContextMenuStrip = CreateLanguageMenu()

        For Each f As String In files
            cms.Items.Add(CreateLanguageMenuItem(f, currentLang, cms))
        Next

        cms.Show(SW_lang, 0, SW_lang.Height)
    End Sub

    Private Sub SelectLang(langCode As String)
        Dim langFolder As String = GetLanguageFolderPath()
        Dim currentFile As String = Path.Combine(langFolder, CurrentLanguageFileName)
        File.WriteAllText(currentFile, langCode)

        Dim langFile = Path.Combine(langFolder, langCode & ".json")
        LangHelper.LoadLang(langFile)

        Base.UpdateLocalizedTexts()
        SW_lang.Text = LangHelper.GetText("meta.languageName")
        AppSettings.Instance.Save()

        Me.Hide()
        Base.ME_CLOSE_BG.Visible = True
        Base.Opacity = 0.85
        Base.Settings_List.Visible = False
        Base.shadowplay.Visible = True
        Base.ShowMainPanel()
        AppSettings.Instance.Save()
        Base.HideAllControls()

        RefreshAllControls(Base)
        RefreshAllControls(Me)
        Application.DoEvents()

        _delayTimer = New Timer()
        _delayTimer.Interval = 100
        AddHandler _delayTimer.Tick, Sub()
                                         _delayTimer.Stop()
                                         _delayTimer.Dispose()
                                         Base.ShowMainPanel()
                                         Base.OpenSettings()
                                         Base.ShowNotifier("notificationOpenShare")
                                     End Sub
        _delayTimer.Start()
    End Sub

    Private Function GetLanguageFolderPath() As String
        Return AppLayout.P(LanguageFolderName)
    End Function

    Private Function GetCurrentLanguageCode(langFolder As String) As String
        Dim currentFile As String = Path.Combine(langFolder, CurrentLanguageFileName)
        If File.Exists(currentFile) Then
            Return File.ReadAllText(currentFile).Trim()
        End If

        Return DefaultLanguageCode
    End Function

    Private Function CreateLanguageMenu() As ContextMenuStrip
        Dim menu As New ContextMenuStrip()
        menu.BackColor = Color.FromArgb(30, 30, 34)
        menu.ForeColor = Color.FromArgb(220, 220, 220)
        menu.Font = New Font("Segoe UI", 10)
        menu.ShowImageMargin = False
        Return menu
    End Function

    Private Function CreateLanguageMenuItem(languageFile As String, currentLang As String, menu As ContextMenuStrip) As ToolStripMenuItem
        Dim langCode As String = Path.GetFileNameWithoutExtension(languageFile)
        Dim item As New ToolStripMenuItem(GetLanguageDisplayName(languageFile, langCode)) With {
            .Tag = langCode,
            .Width = 160
        }

        If langCode = currentLang Then
            item.ForeColor = Color.FromArgb(100, 149, 237)
        End If

        AddHandler item.Click, Sub(sender, e)
                                   Dim code = CStr(CType(sender, ToolStripMenuItem).Tag)
                                   SelectLang(code)
                                   menu.Close()
                               End Sub
        Return item
    End Function

    Private Function GetLanguageDisplayName(languageFile As String, fallbackCode As String) As String
        Try
            Dim raw = File.ReadAllText(languageFile, System.Text.Encoding.UTF8)
            Dim jObject As JObject = JObject.Parse(raw)
            If jObject("meta.languageName") IsNot Nothing Then
                Return jObject("meta.languageName").ToString()
            End If
        Catch
        End Try

        Return fallbackCode
    End Function

    ' Export
    Private Sub btnExportSettings_Click(sender As Object, e As EventArgs) Handles btnExportSettings.Click
        Dim path As String = SettingsExportImport.ExportWithDialog(Me)
        If path IsNot Nothing Then
            MessageBox.Show(LangHelper.GetText("l10n.exportSuccess"), "Settings - Export                                                        ", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show(LangHelper.GetText("l10n.exportFailed"), "Settings - Export                                                        ", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    ' Import
    Private Sub btnImportSettings_Click(sender As Object, e As EventArgs) Handles btnImportSettings.Click
        If SettingsExportImport.ImportWithDialog(Me) Then
            MessageBox.Show(LangHelper.GetText("l10n.importSuccess"), "Settings - Import                                                        ", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Base_RecordingsSet.LoadAPIRECORD()
        Else
            MessageBox.Show(LangHelper.GetText("l10n.importFailed"), "Settings - Import                                                        ", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub
End Class