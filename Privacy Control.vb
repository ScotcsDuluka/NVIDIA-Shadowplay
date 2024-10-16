Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Public Class py
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
    Private Sub py_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Base.alt_z.Start()
        Base.settings_1.Visible = True
        Me.Hide()
    End Sub

    Private Sub bg_fn_Click(sender As Object, e As EventArgs) Handles bg_fn.Click
        Base.alt_z.Start()
        Base.settings_1.Visible = True
        Me.Hide()
    End Sub

    Private Sub py_2_Click(sender As Object, e As EventArgs) Handles py_2.Click
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/py")
            py_2.Text = ("Turn on")
        Else
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/py").Dispose()
            py_2.Text = ("Turn off")
        End If
    End Sub

    Private Sub py_1_Click(sender As Object, e As EventArgs) Handles py_1.Click
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/py")
            py_2.Text = ("Turn on")
        Else
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/py").Dispose()
            py_2.Text = ("Turn off")
        End If

    End Sub
End Class