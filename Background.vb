Imports System.Drawing
Imports System.Runtime.InteropServices

Public Class Bg
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
    Private Sub Bg_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        HideFromAltTab()
    End Sub

    Private Sub Bg_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        www.TopMost = True
        Opacity -= 0.1 ' ลดความโปร่งใสลงทีละ 0.05
        If Opacity = 0 Then
            Me.Hide()
            Timer1.Stop()
        End If
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If Opacity = 0.5 Then
            Timer2.Stop()
        Else
            Me.Opacity += 0.1 ' เพิ่มความโปร่งใสทีละ 0.05
            Me.Show()
        End If
        www.TopMost = True
    End Sub

    Private Sub Bg_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
        www.TopMost = True
    End Sub

    Private Sub load_Tick(sender As Object, e As EventArgs) Handles load.Tick
        HideFromAltTab()
    End Sub
End Class
