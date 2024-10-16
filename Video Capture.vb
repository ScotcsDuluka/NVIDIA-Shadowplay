Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar

Public Class set_vdo
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
    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
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
        Base.settings_1.Visible = True
        Base.alt_z.Start()
        Base.alt_shift_f10.Start()
        Base.record_1.Start()
        Me.Hide()
        Label2.ForeColor = Color.White
        Label2.Cursor = Cursors.Hand
        bg_re.Cursor = Cursors.Hand
    End Sub

    Private Sub set_vdo_Load(sender As Object, e As EventArgs) Handles Me.Load
        HideFromAltTab()
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
End Class