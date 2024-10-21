Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar

Public Class set_vdo
    ' Struct สำหรับใช้เก็บข้อมูลของการตั้งค่าหน้าจอ
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DEVMODE
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)>
        Public dmDeviceName As String
        Public dmSpecVersion As Short
        Public dmDriverVersion As Short
        Public dmSize As Short
        Public dmDriverExtra As Short
        Public dmFields As Integer
        Public dmPositionX As Integer
        Public dmPositionY As Integer
        Public dmDisplayOrientation As Integer
        Public dmDisplayFixedOutput As Integer
        Public dmColor As Short
        Public dmDuplex As Short
        Public dmYResolution As Short
        Public dmTTOption As Short
        Public dmCollate As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)>
        Public dmFormName As String
        Public dmLogPixels As Short
        Public dmBitsPerPel As Integer
        Public dmPelsWidth As Integer
        Public dmPelsHeight As Integer
        Public dmDisplayFlags As Integer
        Public dmDisplayFrequency As Integer
        Public dmICMMethod As Integer
        Public dmICMIntent As Integer
        Public dmMediaType As Integer
        Public dmDitherType As Integer
        Public dmReserved1 As Integer
        Public dmReserved2 As Integer
        Public dmPanningWidth As Integer
        Public dmPanningHeight As Integer
    End Structure

    ' Import EnumDisplaySettings จาก user32.dll
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80 ' สถานะสำหรับ ToolWindow (ไม่แสดงใน Alt+Tab)
    Private Const WS_EX_APPWINDOW As Integer = &H40000 ' สถานะสำหรับการแสดงใน Task Switcher
    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub
    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        If res.Text = "" Or ghz.Text = "" Or bit.Text = "" Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Erorr to Saved.")
            Return
        End If
        My.Settings.res = res.Text
        My.Settings.fps = ghz.Text
        My.Settings.bit = bit.Text
        My.Settings.Save()
        res.ForeColor = Color.Gray
        ghz.ForeColor = Color.Gray
        bit.ForeColor = Color.Gray
        Base.settings_1.Visible = True
        Base.alt_z.Start()
        Base.alt_shift_f10.Start()
        Base.record_1.Start()
        Me.Hide()
        Label2.ForeColor = Color.White
        Label2.Cursor = Cursors.Hand
        bg_re.Cursor = Cursors.Hand
    End Sub

    Private Sub bg_fn_Click(sender As Object, e As EventArgs) Handles bg_fn.Click
        If res.Text = "" Or ghz.Text = "" Or bit.Text = "" Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Erorr to Saved.")
            Return
        End If
        My.Settings.res = res.Text
        My.Settings.fps = ghz.Text
        My.Settings.bit = bit.Text
        My.Settings.Save()
        res.ForeColor = Color.Gray
        ghz.ForeColor = Color.Gray
        bit.ForeColor = Color.Gray
        Base.settings_1.Visible = True
        Base.alt_z.Start()
        Base.alt_shift_f10.Start()
        Base.record_1.Start()
        Me.Hide()
        Label2.ForeColor = Color.White
        Label2.Cursor = Cursors.Hand
        bg_re.Cursor = Cursors.Hand
    End Sub

    ' Import EnumDisplaySettings จาก user32.dll
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function EnumDisplaySettings(ByVal lpszDeviceName As String, ByVal iModeNum As Integer, ByRef lpDevMode As DEVMODE) As Boolean
    End Function


    Private Const VREFRESH As Integer = 116 ' Refresh Rate Index
    Private Sub set_vdo_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim currentScreen As Screen = Screen.PrimaryScreen
        Dim devMode As New DEVMODE()

        Dim currentWidth As Integer = currentScreen.Bounds.Width
        Dim currentHeight As Integer = currentScreen.Bounds.Height

        res.Text = currentWidth & " x " & currentHeight
        ghz.Text = 60
        bit.Text = 20

        ' ซ่อน Form จาก Alt+Tab
        HideFromAltTab()
    End Sub


    Private Sub b_KeyPress(sender As Object, e As KeyPressEventArgs) Handles bit.KeyPress
        ' ตรวจสอบว่าเป็นตัวเลขหรือไม่
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True ' ถ้าไม่ใช่ตัวเลขให้ไม่อนุญาต
        End If
    End Sub

    Private Sub bz_TextChanged(sender As Object, e As EventArgs) Handles bit.TextChanged
        Dim inputValue As Integer

        ' ตรวจสอบว่าป้อนข้อมูลเป็นตัวเลขและอยู่ในช่วง 1-60
        If Integer.TryParse(bit.Text, inputValue) Then
            If inputValue < 1 OrElse inputValue > 100 Then
                Notifier.Show()
                Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                Notifier.icon_n.ForeColor = Color.White
                Notifier.icon_n.Text = ("")
                Notifier.text_n.Text = ("Bitrate Max 100. Your key " & bit.Text & " Bitrate")
                bit.Text = "20" ' เคลียร์ค่าใน TextBox
            End If
        End If
    End Sub

    Private Sub ghz_KeyPress(sender As Object, e As KeyPressEventArgs) Handles ghz.KeyPress
        ' ตรวจสอบว่าเป็นตัวเลขหรือไม่
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True ' ถ้าไม่ใช่ตัวเลขให้ไม่อนุญาต
        End If
    End Sub

    Private Sub ghz_TextChanged(sender As Object, e As EventArgs) Handles ghz.TextChanged
        Dim inputValue As Integer

        ' ตรวจสอบว่าป้อนข้อมูลเป็นตัวเลขและอยู่ในช่วง 1-60
        If Integer.TryParse(ghz.Text, inputValue) Then
            If inputValue < 1 OrElse inputValue > 120 Then
                Notifier.Show()
                Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                Notifier.icon_n.ForeColor = Color.White
                Notifier.icon_n.Text = ("")
                Notifier.text_n.Text = ("Framerate Max 120. Your key " & ghz.Text & " Framerate")

                ghz.Text = "60" ' เคลียร์ค่าใน TextBox

            End If
        End If
    End Sub
    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        Label2.ForeColor = Color.Gray
        Label2.Cursor = Cursors.Default
        bg_re.Cursor = Cursors.Default
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles bg_re.Click
        Label2.ForeColor = Color.Gray
        Label2.Cursor = Cursors.Default
        bg_re.Cursor = Cursors.Default
    End Sub

    Private Sub ALTZ_Tick(sender As Object, e As EventArgs) Handles ALTZ.Tick

    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
        res.ForeColor = Color.White
        ghz.ForeColor = Color.White
        bit.ForeColor = Color.White
    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles PictureBox3.Click
        res.ForeColor = Color.White
        ghz.ForeColor = Color.White
        bit.ForeColor = Color.White
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        res.ForeColor = Color.White
        ghz.ForeColor = Color.White
        bit.ForeColor = Color.White
    End Sub
End Class