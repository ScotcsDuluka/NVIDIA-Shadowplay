Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.AxHost
Imports Microsoft.VisualBasic.Logging

Public Class Notifier_Sub2
    Inherits Form

    ' T27: slot geometry lives in the background forms (CurrentRow) — this
    ' content form just follows its parent's Y, wherever the router puts it.

    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
            Return cp
        End Get
    End Property

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ShadowForm As Shadow2

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ParentNotifier As Notifier2

    Private fadeTimer As New Timer()
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

    Private Sub Notifier_Sub_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Debug.WriteLine("[Notifier_Sub2] ===== Form Load =====")

        HideFromAltTab()
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        Me.Location = New Point(screenWidth - Me.Width, Notifier2.Location.Y)

        Me.Opacity = 0
        Me.Show()

        fadeTimer.Interval = 10
        AddHandler fadeTimer.Tick, AddressOf FadeIn
        fadeTimer.Start()

        Debug.WriteLine("[Notifier_Sub2] Fade in started")
    End Sub

    Private Sub FadeIn(sender As Object, e As EventArgs)
        If Me.Opacity < 1 Then
            Me.Opacity += 0.05
            Exit Sub
        End If

        Me.Opacity = 1

        If Shadow2.Opacity < 1 Then
            Shadow2.Opacity += 0.05
        Else
            Shadow2.Opacity = 1
            fadeTimer.Stop()
            Debug.WriteLine("[Notifier_Sub2] Fade complete")
        End If
    End Sub

    Private Sub text_n_Click_1(sender As Object, e As EventArgs) Handles MyBase.Click, text_n.Click, Notifier_black.Click, PictureBox1.Click, icon_n.Click
        Debug.WriteLine("[Notifier_Sub2] Click → DoCloseClick")
        Notifier2.DoCloseClick()
    End Sub

    Private animStart As DateTime
    Private animDuration As Double
    Private startX As Integer
    Private targetX As Integer
    Private startY As Integer
    Private targetY As Integer
    Private currentPanel As Control
    Private animationRunning As Boolean = False
    Private onComplete As Action
    Private isSlideX As Boolean = False

    Private Sub StartSlideY(LOS As Control,
                       fromY As Integer,
                       toY As Integer,
                       duration As Double,
                       Optional completed As Action = Nothing)

        Debug.WriteLine("[Notifier_Sub2] StartSlideY: " & LOS.Name &
                        " Y " & fromY & "→" & toY &
                        " dur=" & duration & "ms" &
                        " wasRunning=" & animationRunning)

        currentPanel = LOS
        startY = fromY
        targetY = toY
        animDuration = duration
        onComplete = completed

        LOS.Top = fromY
        animStart = DateTime.Now
        animationRunning = True

        Animation_Engine.Interval = 15
        Animation_Engine.Start()
    End Sub

    Private Sub Animation_Engine_Tick(sender As Object, e As EventArgs) Handles Animation_Engine.Tick
        If Not animationRunning Then Return

        Dim elapsed = (DateTime.Now - animStart).TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        If t >= 1 Then
            t = 1
            animationRunning = False
            Animation_Engine.Stop()

            Debug.WriteLine("[Notifier_Sub2] Animation COMPLETE → Y=" & targetY)

            If onComplete IsNot Nothing Then
                onComplete.Invoke()
                onComplete = Nothing
            End If
        End If

        Dim eased As Double = 1 - Math.Pow(1 - t, 3)

        Dim newY As Integer = startY + (targetY - startY) * eased
        currentPanel.Top = newY
    End Sub

    ''' <summary>T27: glides this content form vertically. The Loader's router
    ''' calls it in lockstep with the background form when the bottom toast
    ''' moves up into the top slot (reflow) — the shadow follows the bg form
    ''' on its own.</summary>
    Public Sub GlideTo(targetY As Integer, durationMs As Integer)
        StartSlideY(Me, Me.Top, targetY, durationMs)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Me.IsDisposed Then
            Timer1.Stop()
            Return
        End If
        Debug.WriteLine("[Notifier_Sub2] Timer1 tick → StartSlideY")
        ' T27: legacy "main off" recovery — follow the background form's
        ' CURRENT position (row-aware) instead of the old hardcoded Y=205.
        Try
            StartSlideY(Me, Me.Top, Notifier2.Top, 200)
        Catch
            ' background default instance disposed — nothing to sync to
        End Try
    End Sub

    Private Sub Notifier_Sub_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Debug.WriteLine("[Notifier_Sub2] FormClosing — cleanup")
        animationRunning = False
        Animation_Engine.Stop()
        fadeTimer.Stop()
        fadeTimer.Dispose()
        Timer1.Stop()
    End Sub
End Class
