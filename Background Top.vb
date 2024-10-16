Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Diagnostics
Imports System.Windows.Forms
Imports System.Management
Public Class bg_top

    Private tpsTimer As Timer
    ' ตัวอย่างฟังก์ชันในการอัปเดตค่า TPS
    Private Sub MainForm_TPS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' ตั้งค่า Timer
        tpsTimer = New Timer()
        tpsTimer.Interval = 2000  ' ทำงานทุก 1 วินาที
        AddHandler tpsTimer.Tick, AddressOf OnTimedEvent
        tpsTimer.Start()
    End Sub

    ' ฟังก์ชันที่ทำงานเมื่อ Timer ทำงาน
    Private Sub OnTimedEvent(source As Object, e As EventArgs)
        ' อัปเดตค่า TPS ในที่นี้คือการจำลองค่า
        Dim randomTPS As Integer = New Random().Next(0, 500) ' จำลองค่า TPS ระหว่าง 30-60
        UpdateTPS(randomTPS)  ' อัปเดตค่า TPS ที่แสดงใน Label
    End Sub
    Private Sub UpdateTPS(ByVal newTPS As Integer)
        If newTPS = 0 Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("TPS is Low. Decreased playing experience.")
        Else
        End If

        tpsLabel.Text = "TPS : " & newTPS.ToString()
    End Sub
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
    Private Sub bg_top_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        ac1.Size = New Size(600, 300) ' ขนาดที่ต้องการAlignPanelToTop()
    End Sub

    Private Sub Logo_Click(sender As Object, e As EventArgs) Handles Logo.Click
        Application.Exit()
    End Sub

    Private Sub Logo_text_Click(sender As Object, e As EventArgs) Handles Logo_text.Click
        Application.Restart()
    End Sub

    Private Sub bg_top_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Me.DoubleBuffered = True
    End Sub

    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load.Tick

    End Sub

End Class