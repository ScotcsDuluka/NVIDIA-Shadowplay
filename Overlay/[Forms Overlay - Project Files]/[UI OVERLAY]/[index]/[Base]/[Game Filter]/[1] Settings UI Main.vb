Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class Base_Game_Filter
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

    Private Sub Game_Filter_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.Manual

        Dim screenArea = Screen.PrimaryScreen.Bounds

        Me.Size = New Size(268, screenArea.Height)
        Me.Location = New Point(0, 0)

        Main_Filter.Location = New Point(-500, 0)
        Me.Opacity = 0
        ANIME.Start()
        ANIME.Interval = 1
    End Sub

    Private Sub ANIME_Tick(sender As Object, e As EventArgs) Handles ANIME.Tick
        HideFromAltTab()

        If Opacity >= 0.78 AndAlso Not hasAnimated Then
            hasAnimated = True
            StartSlideX(Main_Filter, -500, 0, 250)
        End If

        If Opacity <= 0 AndAlso hasAnimated Then
            hasAnimated = False
            Main_Filter.Location = New Point(-500, 0)
        End If

    End Sub

    Private Sub d_Click(sender As Object, e As EventArgs) Handles d.Click, ME_CLOSE_BG.Click, ME_CLOSE_BG_GRE.Click
        Base_Game_Filter_Sub.Opacity = 0
        Me.Opacity = 0
        Me.Hide()
        Base_Game_Filter_Sub.Hide()
        Base.isFunctionActive_f3 = True
    End Sub
    Private Sub ME_CLOSE_BG_GRE_MouseMove(sender As Object, e As MouseEventArgs) Handles d.MouseMove, ME_CLOSE_BG_GRE.MouseMove, ME_CLOSE_BG.MouseMove
        ME_CLOSE_BG_GRE.BackColor = ColorTranslator.FromHtml("#76B900")
        Base_Game_Filter_Sub.ME_CLOSE_BG_GRE.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub ME_CLOSE_BG_GRE_MouseLeave(sender As Object, e As EventArgs) Handles d.MouseLeave, ME_CLOSE_BG_GRE.MouseLeave, ME_CLOSE_BG.MouseLeave
        Base_Game_Filter_Sub.ME_CLOSE_BG_GRE.BackColor = System.Drawing.Color.Black
        ME_CLOSE_BG_GRE.BackColor = Color.FromArgb(1, 0, 1)
    End Sub
End Class