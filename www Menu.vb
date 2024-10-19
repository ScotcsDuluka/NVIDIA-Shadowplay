Imports System.Runtime.InteropServices
Imports System.Drawing

Public Class www_en
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
    Private Sub www_en_Load(sender As Object, e As EventArgs) Handles Me.Load
        HideFromAltTab()
    End Sub
    Private copyCount As Integer = 0
    Private maxCopies As Integer = 3
    Private Sub O_1_Click(sender As Object, e As EventArgs) Handles O_1.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ""
        Notifier.text_n.Text = "Extension not found"
    End Sub

    Private Sub www_en_Click(sender As Object, e As EventArgs) Handles MyBase.Click
        www.TopMost = True
    End Sub
End Class