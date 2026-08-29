Imports System.IO
Imports System.Runtime.InteropServices

Public Class Shadow
    Inherits Form

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
            Return cp
        End Get
    End Property

    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000

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
        Dim newStyle As Integer = (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
        SetWindowLong(Me.Handle, GWL_EXSTYLE, newStyle)
    End Sub

    Private Sub Shadow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Debug.WriteLine("[Shadow] ===== Form Load =====")

        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        ' T27: spawn at the background form's CURRENT row position — the
        ' router may have placed it on row 0 or row 1. The sync timer below
        ' keeps it glued to the bg form afterwards.
        Dim y As Integer
        Try
            y = Notifier.Top
        Catch
            y = Notifier.BaseRowY()
        End Try
        Me.Location = New Point(screenWidth - Me.Width, y)
        Debug.WriteLine($"[Shadow] Position Y={Me.Top}")
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        HideFromAltTab()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        HideFromAltTab()
        Try
            If Me.IsDisposed OrElse Notifier.IsDisposed Then
                Timer1.Stop()
                Return
            End If

            ' ✅ M4 FIX: removed HideFromAltTab() and Notifier_Sub.TopMost = True.
            ' Both were redundant — HideFromAltTab is called once in Shadow_Load,
            ' and TopMost is set in the Designer. Calling them 1000x/second was
            ' pure CPU waste (Win32 P/Invoke per tick).
            ' Only sync position — that's the only thing that needs updating.
            Me.Left = Notifier.Left
            Me.Top = Notifier.Top
        Catch ex As Exception
            Debug.WriteLine("[Shadow] Timer1 ERROR: " & ex.Message)
        End Try
    End Sub

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(
        hwnd As IntPtr,
        dwAttribute As Integer,
        ByRef pvAttribute As Integer,
        cbAttribute As Integer
    ) As Integer
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmExtendFrameIntoClientArea(
        hwnd As IntPtr,
        ByRef pMarInset As MARGINS
    ) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MARGINS
        Public leftWidth As Integer
        Public rightWidth As Integer
        Public topHeight As Integer
        Public bottomHeight As Integer
    End Structure

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Debug.WriteLine("[Shadow] Handle created → DWM setup")

        Dim attrValue As Integer = 2
        DwmSetWindowAttribute(Me.Handle, 2, attrValue, 4)

        Dim margins As New MARGINS With {
            .leftWidth = 1,
            .rightWidth = 1,
            .topHeight = 1,
            .bottomHeight = 1
        }

        DwmExtendFrameIntoClientArea(Me.Handle, margins)
    End Sub
    Private Sub Shadow_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Debug.WriteLine("[Shadow] FormClosing — cleanup")
        Timer1.Stop()
    End Sub

    Private Sub HideShadow_Tick(sender As Object, e As EventArgs) Handles HideShadow.Tick
        HideFromAltTab()
    End Sub
End Class