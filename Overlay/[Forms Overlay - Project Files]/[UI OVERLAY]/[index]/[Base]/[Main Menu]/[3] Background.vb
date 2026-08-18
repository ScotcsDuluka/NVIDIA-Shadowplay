Imports System.Drawing
Imports System.Runtime.InteropServices

Public Class Base_Background
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
    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(
        hwnd As IntPtr,
        dwAttribute As Integer,
        ByRef pvAttribute As Integer,
        cbAttribute As Integer) As Integer
    End Function

    Private Sub EnableMica()

        Dim backdropType As Integer = 2 ' 2 = Mica
        DwmSetWindowAttribute(Me.Handle, 38, backdropType, 4)

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        EnableMica()
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
        ' FIX: VB.NET And/Or precedence — And binds tighter than Or, so the original
        '     `style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW` evaluates as
        '     `style Or (WS_EX_TOOLWINDOW And (Not WS_EX_APPWINDOW))` which never
        '     clears the APPWINDOW bit if it was already set. Explicit parens fix it.
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub
    Private Sub Bg_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        ' HideFromAltTab() — removed: WS_EX_TOOLWINDOW is sticky once set in Bg_Load.
        '                  Calling it on every MouseMove was ~dozens of redundant
        '                  GetWindowLong+SetWindowLong P/Invoke pairs per second.
    End Sub

    Private Sub Bg_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        Opacity -= 0.1
        If Opacity = 0 Then
            Me.Hide()
            Timer1.Stop()
        End If
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If Opacity = 0.5 Then
            Timer2.Stop()
        Else
            Me.Opacity += 0.1
            Me.Show()
        End If

    End Sub

    Private Sub Bg_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick

    End Sub

End Class
