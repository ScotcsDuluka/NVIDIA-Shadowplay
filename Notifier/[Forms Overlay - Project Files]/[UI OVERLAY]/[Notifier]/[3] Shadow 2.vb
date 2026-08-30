Imports System.IO
Imports System.Runtime.InteropServices

Public Class Shadow2
    Inherits Form

    ' ===== Toast slot geometry (OWNER spec) =====
    ' Must match the sibling Notifier2 unit: slot 2 = one pitch below slot 1.
    Private Const SlotIndex As Integer = 2
    Private Const SlotGapPx As Integer = 10

    Private ReadOnly Property SlotOffsetY As Integer
        Get
            Return (SlotIndex - 1) * (Me.Height + SlotGapPx)
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED Or WS_EX_NOACTIVATE
            Return cp
        End Get
    End Property

    ' T30.1: never take focus - not on Show, not on click, not ever.
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000

    ' T30.1: WinForms-level half of the no-focus guarantee (covers Show()).
    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

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
        Debug.WriteLine("[Shadow2] ===== Form Load =====")

        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        Dim baseY As Integer
        If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            baseY = 205
            Debug.WriteLine("[Shadow2] Base Y=205 (notifier_main present)")
        Else
            baseY = 105
            Debug.WriteLine("[Shadow2] Base Y=105")
        End If
        Me.Location = New Point(screenWidth - Me.Width, baseY + SlotOffsetY)
        Debug.WriteLine("[Shadow2] Position Y=" & Me.Top & " (slot " & SlotIndex & ")")
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        HideFromAltTab()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        HideFromAltTab()
        Try
            If Me.IsDisposed OrElse Notifier2.IsDisposed Then
                Timer1.Stop()
                Return
            End If

            ' ✅ M4 FIX: removed HideFromAltTab() and Notifier_Sub.TopMost = True.
            ' Both were redundant — HideFromAltTab is called once in Shadow_Load,
            ' and TopMost is set in the Designer. Calling them 1000x/second was
            ' pure CPU waste (Win32 P/Invoke per tick).
            ' Only sync position — that's the only thing that needs updating.
            Me.Left = Notifier2.Left
            Me.Top = Notifier2.Top
        Catch ex As Exception
            Debug.WriteLine("[Shadow2] Timer1 ERROR: " & ex.Message)
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
        Debug.WriteLine("[Shadow2] Handle created → DWM setup")

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
        Debug.WriteLine("[Shadow2] FormClosing — cleanup")
        Timer1.Stop()
    End Sub

    Private Sub HideShadow_Tick(sender As Object, e As EventArgs) Handles HideShadow.Tick
        HideFromAltTab()
    End Sub
End Class
