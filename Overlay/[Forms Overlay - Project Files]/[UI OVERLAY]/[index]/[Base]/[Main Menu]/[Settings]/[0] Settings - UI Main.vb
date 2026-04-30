Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports Newtonsoft.Json.Linq
Public Class Base_Settings
    Inherits NoCloseForm

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

        AppSettings.Instance.Save()

        Dim TIME As New Timer With {.Interval = 20}
        AddHandler TIME.Tick, Sub(s, MIEXXXXXXX)
                                  TIME.Stop()
                                  Base.ShowMainPanel()
                                  Base.Opacity = 0.85
                              End Sub
        TIME.Start()

    End Sub

    Private Sub Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Back_btn_Click(sender As Object, e As EventArgs) Handles Back_btn.Click
        Me.Hide()
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
        Dim langFolder = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile = Path.Combine(langFolder, "current.txt")

        Dim currentLang = "en-US"
        If File.Exists(currentFile) Then currentLang = File.ReadAllText(currentFile).Trim

        Dim files = Directory.GetFiles(langFolder, "*.json")
        If files.Length = 0 Then Return

        Dim cms As New ContextMenuStrip()
        cms.BackColor = Color.FromArgb(30, 30, 34)
        cms.ForeColor = Color.FromArgb(220, 220, 220)
        cms.Font = New Font("Segoe UI", 10)
        cms.ShowImageMargin = False

        For Each f As String In files
            Dim langCode = Path.GetFileNameWithoutExtension(f)
            Dim langName As String = langCode

            Try
                Dim raw = File.ReadAllText(f, System.Text.Encoding.UTF8)
                Dim jObject As JObject = JObject.Parse(raw)
                If jObject("meta.languageName") IsNot Nothing Then
                    langName = jObject("meta.languageName").ToString()
                End If
            Catch
            End Try

            Dim tsi As New ToolStripMenuItem(CStr(langName))
            tsi.Tag = langCode
            tsi.Width = 160

            If langCode = currentLang Then
                tsi.ForeColor = Color.FromArgb(100, 149, 237)
            End If

            AddHandler tsi.Click, Sub(s, a)
                                      Dim code = CStr(CType(s, ToolStripMenuItem).Tag.ToString())
                                      SelectLang(code)
                                      cms.Close()
                                  End Sub

            cms.Items.Add(tsi)
        Next

        cms.Show(SW_lang, 0, SW_lang.Height)
    End Sub

    Private Sub SelectLang(langCode As String)
        Dim langFolder = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile = Path.Combine(langFolder, "current.txt")
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

End Class