Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class Notifier
    Inherits BlockClose

#Region "WinAPI"

    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
            Return cp
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

#End Region

#Region "Animation Engine"

    Private animStart As DateTime
    Private animDuration As Double
    Private startX As Integer
    Private targetX As Integer
    Private startY As Integer
    Private targetY As Integer
    Private currentPanel As Control
    Private animationRunning As Boolean = False
    Private onComplete As Action
    Private isSlideX As Boolean

    Public Sub StartSlide(panel As Control,
                           fromX As Integer,
                           toX As Integer,
                           duration As Double,
                           Optional completed As Action = Nothing)

        ' Stop previous animation
        If animationRunning Then
            Animation_Engine.Stop()
            animationRunning = False
        End If

        currentPanel = panel
        startX = fromX
        targetX = toX
        animDuration = duration
        onComplete = completed
        isSlideX = True

        panel.Left = fromX
        animStart = DateTime.Now
        animationRunning = True

        Animation_Engine.Start()
    End Sub

    Public Sub StartSlideY(panel As Control,
                           fromY As Integer,
                           toY As Integer,
                           duration As Double,
                           Optional completed As Action = Nothing)

        ' Stop previous animation
        If animationRunning Then
            Animation_Engine.Stop()
            animationRunning = False
        End If

        currentPanel = panel
        startY = fromY
        targetY = toY
        animDuration = duration
        onComplete = completed
        isSlideX = False

        panel.Top = fromY
        animStart = DateTime.Now
        animationRunning = True

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

            ' Final position
            If isSlideX Then
                currentPanel.Left = targetX
            Else
                currentPanel.Top = targetY
            End If

            ' Callback
            Dim callback As Action = onComplete
            onComplete = Nothing

            If callback IsNot Nothing Then
                ' Use BeginInvoke to prevent blocking
                Me.BeginInvoke(Sub() callback.Invoke())
            End If
        Else
            Dim eased As Double = 1 - Math.Pow(1 - t, 3)

            If isSlideX Then
                currentPanel.Left = CInt(startX + (targetX - startX) * eased)
            Else
                currentPanel.Top = CInt(startY + (targetY - startY) * eased)
            End If
        End If
    End Sub

#End Region

#Region "Timers"

    Public autoClose As New Timer()
    Private _delayTimer As Timer
    Private _closeTimer As Timer

    Private Sub StopDelayTimer()
        If _delayTimer IsNot Nothing Then
            _delayTimer.Stop()
            _delayTimer.Dispose()
            _delayTimer = Nothing
        End If
    End Sub

    Private Sub StopCloseTimer()
        If _closeTimer IsNot Nothing Then
            _closeTimer.Stop()
            _closeTimer.Dispose()
            _closeTimer = Nothing
        End If
    End Sub

#End Region

#Region "Form Load"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BlockClose.AllowClose = False
        Shadow.Show()
        HideFromAltTab()

        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            Me.Location = New Point(screenWidth - Me.Width, 205)
        Else
            Me.Location = New Point(screenWidth - Me.Width, 105)
        End If

        Notifier_black.Location = New Point(Me.Width, 0)
        Notifier_green.Location = New Point(Me.Width, 0)
        Notifier_green.Size = New Size(300, 90)
        Notifier_black.Size = New Size(300, 90)

        ' Start initial animation
        StartSlide(Notifier_green, Me.Width, Me.Width - 300, 200)

        ' Delay for black panel
        StopDelayTimer()
        _delayTimer = New Timer()
        _delayTimer.Interval = 200
        AddHandler _delayTimer.Tick, Sub()
                                         _delayTimer.Stop()
                                         StartSlide(Notifier_black, Me.Width, Me.Width - 300, 300,
                                             Sub()
                                                 Notifier_Sub.Show()
                                                 Notifier_green_stop.Visible = True
                                             End Sub)
                                     End Sub
        _delayTimer.Start()

        ' Auto close timer
        autoClose.Interval = 6000
        RemoveHandler autoClose.Tick, AddressOf AutoClose_Tick
        AddHandler autoClose.Tick, AddressOf AutoClose_Tick
        autoClose.Start()

        TopMost = True
    End Sub

    Private Sub AutoClose_Tick(sender As Object, e As EventArgs)
        autoClose.Stop()
        SlideOutAll()
    End Sub

#End Region

#Region "Slide Out"

    Private Sub SlideOutAll()
        BlockClose.AllowClose = True
        Shadow.Close()
        Notifier_Sub.Close()
        Notifier_green_stop.Visible = False

        StartSlide(Notifier_black, Notifier_black.Left, Me.Width + 300, 600)

        ' Delay for green panel
        StopDelayTimer()
        _delayTimer = New Timer()
        _delayTimer.Interval = 200
        AddHandler _delayTimer.Tick, Sub()
                                         _delayTimer.Stop()
                                         StartSlide(Notifier_green, Notifier_green.Left, Me.Width + 300, 600)

                                         ' Close timer
                                         StopCloseTimer()
                                         _closeTimer = New Timer()
                                         _closeTimer.Interval = 200
                                         AddHandler _closeTimer.Tick, Sub()
                                                                          _closeTimer.Stop()
                                                                          Me.Close()
                                                                      End Sub
                                         _closeTimer.Start()
                                     End Sub
        _delayTimer.Start()
    End Sub

#End Region

#Region "Click Events"

    Public Sub DoCloseClick()
        Application.Restart()
    End Sub

    Private Sub Notifier_green_Click(sender As Object, e As EventArgs) Handles Notifier_green.Click, text_n.Click, icon_n.Click, Notifier_black.Click, Notifier_green_stop.Click
        Application.Restart()
    End Sub

    Private Sub IF_N_Tick(sender As Object, e As EventArgs) Handles IF_N.Tick
        StartSlideY(Me, Me.Top, 105, 200)
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If Not My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            IF_N.Stop()
        End If
    End Sub

#End Region

End Class