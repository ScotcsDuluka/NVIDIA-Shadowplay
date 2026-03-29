Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Public Class Base_Privacy_Control
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
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub
    Private Sub py_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
    End Sub

    Private Sub py_2_Click(sender As Object, e As EventArgs) Handles py_2.Click
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\privacy") Then
            File.Delete(Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data\privacy"))
            py_2.Text = LangHelper.GetText("l10n.instantReplayStart")
        Else
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data\privacy").Dispose()
            py_2.Text = LangHelper.GetText("l10n.instantReplayStop")
        End If
    End Sub

End Class