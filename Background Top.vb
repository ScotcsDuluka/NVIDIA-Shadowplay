Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Diagnostics
Imports System.Windows.Forms
Imports System.Management
Public Class Base_Background_Top
#Region "Animation Engine"

    Private animStart As DateTime
    Private animDuration As Double
    Private startValue As Integer
    Private targetValue As Integer
    Private animationRunning As Boolean = False
    Private currentControl As Control

    Private WithEvents AnimationTimer As New Timer With {.Interval = 15}

    Private Sub StartSlideY(ctrl As Control,
                        fromY As Integer,
                        toY As Integer,
                        duration As Double)

        If animationRunning Then Return

        currentControl = ctrl
        startValue = fromY
        targetValue = toY
        animDuration = duration

        ctrl.Top = fromY
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
        Dim newY As Integer = startValue + (targetValue - startValue) * eased

        currentControl.Top = newY

    End Sub

#End Region

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

    End Sub

    Private Sub Logo_Click(sender As Object, e As EventArgs)
        Application.Exit()
    End Sub

    Private Sub Main_Top_Click(sender As Object, e As EventArgs) Handles Main_Top.Click
        Application.Restart()
    End Sub

    Private Sub bg_top_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()

        ' ตั้งตำแหน่งเริ่มต้น

        ANIME.Start()
        Main_Top.Location = New Point(0, -100)
    End Sub

    Private hasAnimated As Boolean = False

    Private Sub ANIME_Tick(sender As Object, e As EventArgs) Handles ANIME.Tick

        ' เริ่ม Animation ตอน Opacity ≥ 0.5
        If Me.Opacity >= 0.5 AndAlso Not hasAnimated Then
            hasAnimated = True
            StartSlideY(Main_Top, -100, 0, 100)
        End If

        ' รีเซ็ต ถ้า Opacity หายไป
        If Me.Opacity <= 0 Then
            hasAnimated = False
            Main_Top.Location = New Point(0, -100)
        End If

        If Main_Top.Location = New Point(0, 0) Then
        End If
    End Sub
End Class