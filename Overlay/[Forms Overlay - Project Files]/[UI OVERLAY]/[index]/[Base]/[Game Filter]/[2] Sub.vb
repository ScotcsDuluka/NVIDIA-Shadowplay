Imports System.Drawing
Imports System.Runtime.InteropServices

Public Class Base_Game_Filter_Sub


    Inherits NoCloseForm
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

#Region "Animation Engine"

    Private animStart As DateTime
    Private animDuration As Double
    Private startValue As Integer
    Private targetValue As Integer
    Private animationRunning As Boolean = False
    Private currentControl As Control

    Private WithEvents AnimationTimer As New Timer With {.Interval = 15}
    Private WithEvents ANIME As New Timer With {.Interval = 15}

    Private Sub StartSlideX(ctrl As Control, fromX As Integer, toX As Integer, duration As Double)

        If animationRunning Then Return

        currentControl = ctrl
        startValue = fromX
        targetValue = toX
        animDuration = duration

        ctrl.Left = fromX
        animationRunning = True
        animStart = DateTime.Now

        AnimationTimer.Start()

    End Sub

    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs) Handles AnimationTimer.Tick

        If Not animationRunning Then Return

        Dim elapsed = (DateTime.Now - animStart).TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        If t >= 1 Then
            t = 1
            animationRunning = False
            AnimationTimer.Stop()
        End If

        Dim eased As Double = 1 - Math.Pow(1 - t, 3)
        Dim newX As Integer = startValue + (targetValue - startValue) * eased

        currentControl.Left = newX

    End Sub

#End Region

    Private hasAnimated As Boolean = False

    Private Sub Game_Filter_Sub_Load(sender As Object, e As EventArgs) Handles Me.Load

        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.Manual

        Dim screenArea = Screen.PrimaryScreen.Bounds

        Me.Size = New Size(268, screenArea.Height)
        Me.Location = New Point(0, 0)

        ' เริ่มซ่อนทางซ้าย
        BG.Location = New Point(-500, 0)

        ANIME.Start()

    End Sub

    Private Sub ANIME_Tick(sender As Object, e As EventArgs) Handles ANIME.Tick
        HideFromAltTab()
        If Me.Opacity >= 0.78 AndAlso Not hasAnimated Then
            hasAnimated = True
            StartSlideX(BG, -500, 0, 250)
        End If

        If Me.Opacity <= 0 AndAlso hasAnimated Then
            hasAnimated = False
            BG.Location = New Point(-500, 0)
        End If

    End Sub

    Private Sub BG_Click(sender As Object, e As EventArgs) Handles BG.Click
        Base_Game_Filter.TopMost = True
    End Sub

    Private Sub BG_MouseDown(sender As Object, e As MouseEventArgs) Handles BG.MouseDown
        Base_Game_Filter.TopMost = True
    End Sub

    Private Sub BG_MouseHover(sender As Object, e As EventArgs) Handles BG.MouseHover
        Base_Game_Filter.TopMost = True
    End Sub
End Class