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

    Private Sub ch_Click(sender As Object, e As EventArgs) Handles ch.Click
        ch.Enabled = False
        CheckForUpdateAsync()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Me.Hide()
        Base.ME_CLOSE_BG.Visible = True
        Base.d.Visible = True
        Base.SET_Back.Visible = True
        Base.Opacity = 0.85
        Base.settings_1.Visible = False
        Base.Main_menu_list.Visible = True
        Base.ShowMainPanel()   ' ใช้ ShowMainPanel แทน ALT_Z.Start()
    End Sub

    Private Sub Settings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub Back_btn_Click(sender As Object, e As EventArgs) Handles Back_btn.Click
        Me.Hide()
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
    End Sub

    Private Sub text_settings_Click(sender As Object, e As EventArgs) Handles text_settings.Click
        ' ไม่ต้องทำอะไร
    End Sub
End Class