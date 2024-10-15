Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms
Imports System.Drawing.Drawing2D
Imports Windows.Foundation.Metadata
Public Class Notifier

    <DllImport("user32.dll", SetLastError:=True)>
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

    Dim slideInPanel1 As Boolean = False
    Dim slideInPanel2 As Boolean = False
    Dim slideOutPanel1 As Boolean = False
    Dim slideOutPanel2 As Boolean = False
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Base.noty.ForeColor = Color.Gray Then
            Opacity = 0
        End If
        Me.DoubleBuffered = True
        HideFromAltTab()

        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        Me.Location = New Point(screenWidth - Me.Width, 90)

        Notifier_green.Size = New Size(300, 90) ' ตั้งขนาดของ Notifier_green
        Notifier_green.Location = New Point(Me.Width, 0) ' เริ่มต้น Notifier_green นอกหน้าจอด้านขวา
        Notifier_green.Visible = True ' ทำให้ Notifier_green มองเห็นได้

        ' ตั้งค่าขนาดและตำแหน่งของ Notifier_black
        Notifier_black.Size = New Size(300, 90) ' ตั้งขนาดของ Notifier_black
        Notifier_black.Location = New Point(Me.Width, 0) ' เริ่มต้น Notifier_black นอกหน้าจอด้านขวา
        Notifier_black.Visible = True ' ทำให้ Notifier_black มองเห็นได้

        ' เริ่มการเลื่อน Notifier_green เข้ามาเมื่อเปิดฟอร์ม
        slideInPanel1 = True
        Timer1.Interval = 1 ' ตั้งค่า Timer5 ให้ทำงานทุก 1 มิลลิวินาที
        Timer5.Interval = 1 ' ตั้งค่า Timer5 ให้ทำงานทุก 1 มิลลิวินาที
        Timer5.Start() ' เริ่ม Timer


        Timer2.Interval = 6500 ' ตั้งค่า Timer2 ให้หน่วงเวลา 6 วินาที (6000 มิลลิวินาที)
        Timer2.Start() ' เริ่ม Timer2
        load.Start()

        De.Interval = 75
        TopMost = True
    End Sub


    ' Timer สำหรับควบคุมการเคลื่อนที่ของ Notifier_green และ Notifier_black
    Private Async Sub Timer5_Tick(sender As Object, e As EventArgs) Handles Timer5.Tick
        ' เลื่อน Notifier_green เข้ามา
        If slideInPanel1 Then
            If Notifier_green.Location.X > Me.Width - Notifier_green.Width Then
                Notifier_green.Location = New Point(Notifier_green.Location.X - 25, Notifier_green.Location.Y) ' เลื่อน Notifier_green ทีละ 10 พิกเซล
            Else
                slideInPanel1 = False ' หยุดการเลื่อน Notifier_green เมื่อมั' นเข้ามาในฟอร์ม
                Timer5.Stop() ' หยุด Timer
                slideInPanel2 = True ' เริ่มให้ Panel2 เลื่อนเข้ามา
                Timer5.Start() ' เริ่ม Timer อีกครั้งสำหรับ Panel2
            End If
        ElseIf slideInPanel2 Then
            ' เลื่อน Panel2 เข้ามาแต่หยุดก่อนขอบฟอร์ม (เช่น หยุดห่างจากขอบขวา 50 พิกเซล)
            If Notifier_black.Location.X > Me.Width - Notifier_black.Width Then
                Notifier_black.Location = New Point(Notifier_black.Location.X - 25, Notifier_black.Location.Y) ' เลื่อน Panel2 ทีละ 10 พิกเซล
            Else

                slideInPanel2 = False ' หยุดการเลื่อน Panel2 เมื่อถึงตำแหน่งที่ต้องการ
                Timer5.Stop() ' หยุด Timer
                De.Start()
                Notifier_Sub.Show()
                If Base.noty.ForeColor = Color.Gray Then
                    Me.Close()
                End If
            End If
        End If
    End Sub
    ' หลังจากครบ 6 วินาที เริ่มเลื่อน Panel1 ออก
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick

        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
        slideOutPanel2 = True
        Timer2.Stop() ' หยุด Timer2 หลังจากครบ 6 วินาที
    End Sub

    ' เลื่อน Panel1 ออก
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        icon_n.Visible = False
        text_n.Visible = False
        Logo.Visible = False
        If slideOutPanel2 Then
            If Notifier_black.Location.X < Me.Width + Notifier_black.Width Then
                Notifier_black.Location = New Point(Notifier_black.Location.X + 25, Notifier_black.Location.Y) ' เลื่อน Panel1 ทีละ 25 พิกเซล
            Else
                slideOutPanel2 = False
                slideOutPanel1 = True
            End If
        ElseIf slideOutPanel1 Then
            If Notifier_green.Location.X < Me.Width + Notifier_green.Width Then
                Notifier_green.Location = New Point(Notifier_green.Location.X + 25, Notifier_green.Location.Y) ' เลื่อน Panel1 ทีละ 25 พิกเซล
            Else
                If text_n.Text = ("NVIDIA Shadowplay™ app is closed.") Then
                    Application.Exit()
                End If
                Close()
            End If
        End If
    End Sub
    Private Sub Panel1_Click(sender As Object, e As EventArgs) Handles Notifier_green.Click
        slideOutPanel2 = True
        Timer5.Stop()
        Timer1.Interval = 1 ' ตั้งค่า Timer1 ให้ทำงานทุก 1 มิลลิวินาที
        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
    End Sub

    Private Sub Panel2_Click(sender As Object, e As EventArgs) Handles Notifier_black.Click
        slideOutPanel2 = True
        Timer5.Stop()

        Timer1.Interval = 1 ' ตั้งค่า Timer1 ให้ทำงานทุก 1 มิลลิวินาที
        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
    End Sub

    Private Sub text_n_Click(sender As Object, e As EventArgs) Handles text_n.Click
        slideOutPanel2 = True
        Timer5.Stop()
        Timer1.Interval = 1 ' ตั้งค่า Timer1 ให้ทำงานทุก 1 มิลลิวินาที
        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
    End Sub

    Private Sub icon_n_Click(sender As Object, e As EventArgs) Handles icon_n.Click
        slideOutPanel2 = True
        Timer5.Stop()
        Timer1.Interval = 1 ' ตั้งค่า Timer1 ให้ทำงานทุก 1 มิลลิวินาที
        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
    End Sub

    Private Sub top_Click(sender As Object, e As EventArgs) Handles Notifier_green_stop.Click
        slideOutPanel2 = True
        Timer5.Stop()
        Timer1.Interval = 1 ' ตั้งค่า Timer1 ให้ทำงานทุก 1 มิลลิวินาที
        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
    End Sub

    Private Sub load_Tick(sender As Object, e As EventArgs) Handles load.Tick

        If Notifier_Sub.Timer1.Enabled = False Then
            TopMost = True
        Else
            Notifier_Sub.TopMost = True
        End If
    End Sub
    Private Sub De_Tick(sender As Object, e As EventArgs) Handles De.Tick

        If Timer5.Enabled Then
            If icon_n.Text = ("") Then
                Logo.Visible = True
                icon_n.Visible = False
                text_n.Visible = True
                If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
                    py.Stop()
                Else
                    If icon_n.Text = ("") Then
                        Return
                    Else
                        py.Start()
                        icon_n.Font = New Font(icon_n.Font.FontFamily, 40)
                        icon_n.ForeColor = Color.White
                        icon_n.Text = ("")
                        text_n.Text = ("Privacy control capture has off. Turn on to use")
                    End If
                End If
                De.Stop()
            Else
                icon_n.Visible = True
                text_n.Visible = True
                If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
                    py.Stop()
                Else
                    If icon_n.Text = ("") Then
                        Return
                    Else
                        py.Start()
                        icon_n.Font = New Font(icon_n.Font.FontFamily, 40)
                        icon_n.ForeColor = Color.White
                        icon_n.Text = ("")
                        text_n.Text = ("Privacy control capture has off. Turn on to use")
                    End If
                End If
                De.Stop()
            End If
        Else
            If icon_n.Text = ("") Then
                Logo.Visible = True
                icon_n.Visible = False
                text_n.Visible = True
                If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
                    py.Stop()
                Else
                    If icon_n.Text = ("") Then
                        Return
                    Else
                        py.Start()
                        icon_n.Font = New Font(icon_n.Font.FontFamily, 40)
                        icon_n.ForeColor = Color.White
                        icon_n.Text = ("")
                        text_n.Text = ("Privacy control capture has off. Turn on to use")
                    End If
                End If
                De.Stop()
            Else
                icon_n.Visible = True
                text_n.Visible = True
                If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
                    py.Stop()
                Else
                    If icon_n.Text = ("") Then
                        Return
                    Else
                        py.Start()
                        icon_n.Font = New Font(icon_n.Font.FontFamily, 40)
                        icon_n.ForeColor = Color.White
                        icon_n.Text = ("")
                        text_n.Text = ("Privacy control capture has off. Turn on to use")
                    End If
                End If
                De.Stop()
            End If
        End If
        De.Stop()
    End Sub

    Private Sub py_Tick(sender As Object, e As EventArgs) Handles py.Tick
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
            py.Stop()
        Else
            If icon_n.Text = ("") Then
                Return
            Else
                icon_n.Font = New Font(icon_n.Font.FontFamily, 40)
                icon_n.ForeColor = Color.White
                icon_n.Text = ("")
                text_n.Text = ("Privacy control capture has off. Turn on to use")
            End If
        End If
    End Sub

    Private Sub Logo_Click(sender As Object, e As EventArgs) Handles Logo.Click
        slideOutPanel2 = True
        Timer5.Stop()
        Timer1.Interval = 1 ' ตั้งค่า Timer1 ให้ทำงานทุก 1 มิลลิวินาที
        Timer1.Start() ' เริ่ม Timer1 เพื่อเลื่อน Panel1
    End Sub
End Class
