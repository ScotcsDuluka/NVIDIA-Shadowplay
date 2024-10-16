Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports System.Drawing.Drawing2D
Public Class Notifier_Sub

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

    Private Sub overlay_sub_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        Me.Location = New Point(screenWidth - Me.Width, 90)
        Me.Opacity = 1
        Timer1.Interval = 10 ' ตั้งค่าเวลาที่ Form จะจางหาย
        Timer1.Start() ' เริ่ม Timer เมื่อโหลด Form
        If Base.noty.ForeColor = Color.Gray Then
            Opacity = 0
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        ' ลดค่า Opacity ของ Form ทีละน้อย
        If Me.Opacity > 0 Then
            Me.Opacity -= 0.07 ' ค่อยๆลด Opacity
            TopMost = True
        Else
            Timer1.Stop() ' หยุด Timer เมื่อ Opacity เป็น 0
            Me.Close() ' ปิด Form
        End If
    End Sub
End Class