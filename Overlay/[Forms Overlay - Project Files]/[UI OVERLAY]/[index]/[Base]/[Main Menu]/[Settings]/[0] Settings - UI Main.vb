Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices

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
        Base.d.Visible = True
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

        ' อ่านค่าปัจจุบัน
        Dim currentLang = "en-US"
        If File.Exists(currentFile) Then currentLang = File.ReadAllText(currentFile).Trim

        ' สลับภาษา
        Dim newLang As String
        Select Case currentLang
            Case "en-US"
                newLang = "th-TH"
            Case "th-TH"
                newLang = "zh-CHS"
            Case Else
                newLang = "en-US"
        End Select

        ' บันทึก
        File.WriteAllText(currentFile, newLang)

        ' โหลดภาษาใหม่
        Dim langFile = Path.Combine(langFolder, newLang & ".json")
        LangHelper.LoadLang(langFile)

        ' อัปเดต UI
        Base.UpdateLocalizedTexts()

        ' ตั้งชื่อปุ่มจาก JSON
        SW_lang.Text = LangHelper.GetText("meta.languageName")
        AppSettings.Instance.Save()


        Me.Hide()
        Base.ME_CLOSE_BG.Visible = True
        Base.d.Visible = True
        Base.Opacity = 0.85
        Base.Settings_List.Visible = False
        Base.shadowplay.Visible = True
        Base.ShowMainPanel()
        AppSettings.Instance.Save()
        Base.HideAllControls()

        ' ✅ Refresh ทุกอย่าง
        RefreshAllControls(Base)
        RefreshAllControls(Me)
        Application.DoEvents()

        ' Delay
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