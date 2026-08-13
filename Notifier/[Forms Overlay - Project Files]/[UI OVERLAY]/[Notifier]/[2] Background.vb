Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms


Public Class Notifier
    Inherits Form

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

    Private Delegate Sub MMTimerProc(uID As UInteger, uMsg As UInteger,
                                      dwUser As UIntPtr, dw1 As UInteger, dw2 As UInteger)
    Private Const TIME_PERIODIC As Integer = 1
    Private Const TIME_KILL_SYNCHRONOUS As Integer = &H100

    <DllImport("winmm.dll")>
    Private Shared Function timeSetEvent(uDelay As UInteger, uResolution As UInteger,
                                         fptc As MMTimerProc, dwUser As UIntPtr,
                                         fuEvent As UInteger) As UInteger
    End Function

    <DllImport("winmm.dll")>
    Private Shared Function timeKillEvent(uTimerID As UInteger) As Integer
    End Function

    <DllImport("winmm.dll")>
    Private Shared Function timeBeginPeriod(uPeriod As UInteger) As UInteger
    End Function

    <DllImport("winmm.dll")>
    Private Shared Function timeEndPeriod(uPeriod As UInteger) As UInteger
    End Function

    Private mmTimerId As UInteger = 0
    Private mmCallback As MMTimerProc
    Private sw As Stopwatch

    ' ★★★ ใหม่: ป้องกันบักกดรัวๆ — คิว BeginInvoke ไม่ให้ต่อซ้อน ★★★
    Private _invokePending As Boolean = False

    Private Sub Animation_Engine_Start()
        timeBeginPeriod(1)
        mmCallback = New MMTimerProc(AddressOf OnMMTick)
        mmTimerId = timeSetEvent(1, 1, mmCallback, UIntPtr.Zero,
                                  TIME_PERIODIC Or TIME_KILL_SYNCHRONOUS)
        Debug.WriteLine("[Notifier.MM] Timer started, ID=" & mmTimerId)
    End Sub

    Private Sub Animation_Engine_Stop()
        If mmTimerId <> 0 Then
            timeKillEvent(mmTimerId)
            mmTimerId = 0
            ' ✅ Bug 1 FIX: only call timeEndPeriod if we actually called
            ' timeBeginPeriod. Old code called timeEndPeriod(1) unconditionally
            ' — if Animation_Engine_Stop ran twice (once in Tick completion,
            ' once in FormClosing), the second call was unbalanced → corrupts
            ' system timer resolution reference count.
            timeEndPeriod(1)
        End If
        _invokePending = False
        Debug.WriteLine("[Notifier.MM] Timer stopped")
    End Sub

    ' ★★★ แก้: throttle — ถ้า tick ก่อนหน้ายังรออยู่ในคิว ข้ามไป ★★★
    Private Sub OnMMTick(uID As UInteger, uMsg As UInteger,
                          dwUser As UIntPtr, dw1 As UInteger, dw2 As UInteger)
        If Me.IsDisposed OrElse Me.Disposing Then Return

        If _invokePending Then Return
        _invokePending = True

        Try
            Me.BeginInvoke(Sub()
                               _invokePending = False
                               Animation_Engine_Tick()
                           End Sub)
        Catch ex As ObjectDisposedException
            _invokePending = False
            Debug.WriteLine("[Notifier.MM] BeginInvoke failed — disposed")
        End Try
    End Sub

    Public Sub StartSlide(panel As Control,
                           fromX As Integer,
                           toX As Integer,
                           duration As Double,
                           Optional completed As Action = Nothing)


        Debug.WriteLine("[Notifier] StartSlide: " & panel.Name &
                        " X " & fromX & "→" & toX &
                        " dur=" & duration & "ms" &
                        " wasRunning=" & animationRunning)
        TopMost = True
        If animationRunning Then
            Animation_Engine_Stop()
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
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub StartSlideY(panel As Control,
                           fromY As Integer,
                           toY As Integer,
                           duration As Double,
                           Optional completed As Action = Nothing)

        Debug.WriteLine("[Notifier] StartSlideY: " & panel.Name &
                        " Y " & fromY & "→" & toY &
                        " dur=" & duration & "ms" &
                        " wasRunning=" & animationRunning)
        TopMost = True
        If animationRunning Then
            Animation_Engine_Stop()
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
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Private Sub Animation_Engine_Tick()
        If Not animationRunning Then Return

        Dim elapsed = sw.Elapsed.TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        If t >= 1 Then
            t = 1
            animationRunning = False
            Animation_Engine_Stop()

            If isSlideX Then
                currentPanel.Left = targetX
            Else
                currentPanel.Top = targetY
            End If

            Debug.WriteLine("[Notifier] Animation COMPLETE " &
                            If(isSlideX, "X=" & targetX, "Y=" & targetY))

            Dim callback As Action = onComplete
            onComplete = Nothing

            If callback IsNot Nothing Then
                Try
                    callback.Invoke()
                Catch ex As Exception
                    Debug.WriteLine("[Notifier] onComplete ERROR: " & ex.Message)
                End Try
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
    Private _delayTimers As Timer
    Private _closeTimer As Timer

    Private Sub StopDelayTimer()
        If _delayTimer IsNot Nothing Then
            _delayTimer.Stop()
            _delayTimer.Dispose()
            _delayTimer = Nothing
        End If
        ' ✅ M3 FIX: also dispose _delayTimers (plural) — was never disposed.
        ' Each toast show cycle created a new one without disposing the old.
        If _delayTimers IsNot Nothing Then
            _delayTimers.Stop()
            _delayTimers.Dispose()
            _delayTimers = Nothing
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

        HideFromAltTab()

        ' Dim w As Integer = Screen.PrimaryScreen.WorkingArea.Width


        Dim w As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            Me.Location = New Point(w - Me.Width, 205)
            Debug.WriteLine("[Notifier] Position Y=205")
        Else
            Me.Location = New Point(w - Me.Width, 105)
            Debug.WriteLine("[Notifier] Position Y=105")
        End If

        Notifier_black.Location = New Point(Me.Width, 0)
        Notifier_green.Location = New Point(Me.Width, 0)
        Notifier_green.Size = New Size(300, 90)
        Notifier_black.Size = New Size(300, 90)



        _delayTimers = New System.Windows.Forms.Timer()
        _delayTimers.Interval = 300
        AddHandler _delayTimers.Tick, Sub()
                                          _delayTimers.Stop()

                                          Shadow.Show()
                                          Opacity = 1
                                          StartSlide(Notifier_green, Me.Width, Me.Width - 300, 200)
                                          StopDelayTimer()









                                          _delayTimer = New Timer()
                                          _delayTimer.Interval = 250
                                          AddHandler _delayTimer.Tick, Sub()
                                                                           _delayTimer.Stop()

                                                                           StartSlide(Notifier_black, Me.Width, Me.Width - 300, 300,
                                                                               Sub()
                                                                                   Notifier_Sub.Show()
                                                                                   Notifier_green_stop.Visible = True
                                                                               End Sub)
                                                                       End Sub
                                          _delayTimer.Start()

                                          autoClose.Interval = 6000
                                          RemoveHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          AddHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          autoClose.Start()

                                          TopMost = True
                                          Debug.WriteLine("[Notifier] ===== Form Load Done =====")







                                      End Sub
        _delayTimers.Start()

    End Sub

    Private Sub AutoClose_Tick(sender As Object, e As EventArgs)
        Debug.WriteLine("[Notifier] AutoClose triggered")
        autoClose.Stop()
        SlideOutAll()
    End Sub

    ' ★★★ ใหม่: cleanup ตอนปิด ★★★
    Private Sub Notifier_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Debug.WriteLine("[Notifier] FormClosing — cleanup")
        animationRunning = False
        Animation_Engine_Stop()
        autoClose.Stop()
        StopDelayTimer()
        StopCloseTimer()
    End Sub

#End Region

#Region "Slide Out"

    Private Sub SlideOutAll()
        Debug.WriteLine("[Notifier] SlideOutAll")

        ' ✅ Bug 2 FIX: wrap Close() in try/catch — if Shadow or Notifier_Sub
        ' are already closed/disposed (e.g., double-trigger of SlideOutAll),
        ' Close() throws ObjectDisposedException which crashes the Notifier.
        Try : If Shadow IsNot Nothing AndAlso Not Shadow.IsDisposed Then Shadow.Close()
        Catch ex As Exception : Debug.WriteLine("[Notifier] Shadow.Close error: " & ex.Message)
        End Try
        Try : If Notifier_Sub IsNot Nothing AndAlso Not Notifier_Sub.IsDisposed Then Notifier_Sub.Close()
        Catch ex As Exception : Debug.WriteLine("[Notifier] Notifier_Sub.Close error: " & ex.Message)
        End Try

        ' ✅ Bug 4 FIX: delay setting Visible=False until AFTER slide-out completes.
        ' Old code set it immediately, so UpdateNotifier saw Visible=False during
        ' slide-out and tried Notifier.Show() on an already-visible form → no-op →
        ' new notification was lost when Me.Close() fired.
        ' Now we set it in the _closeTimer callback (after all animations are done).
        StartSlide(Notifier_black, Notifier_black.Left, Me.Width + 300, 600)


        StopDelayTimer()
        _delayTimer = New Timer()
        _delayTimer.Interval = 200
        AddHandler _delayTimer.Tick, Sub()
                                         _delayTimer.Stop()
                                         StartSlide(Notifier_green, Notifier_green.Left, Me.Width + 300, 600)

                                         StopCloseTimer()
                                         _closeTimer = New Timer()
                                         _closeTimer.Interval = 200
                                         AddHandler _closeTimer.Tick, Sub()
                                                                          _closeTimer.Stop()
                                                                          ' ✅ Bug 4 FIX: set Visible=False
                                                                          ' HERE, after all animations are done.
                                                                          ' UpdateNotifier can now correctly
                                                                          ' detect that the toast is fully closed.
                                                                          Notifier_green_stop.Visible = False
                                                                          Me.Close()
                                                                      End Sub
                                         _closeTimer.Start()
                                     End Sub
        _delayTimer.Start()
    End Sub

#End Region

#Region "Click Events"

    Public Sub DoCloseClick()
        Debug.WriteLine("[Notifier] DoCloseClick → SlideOutAll")
        ' ✅ Bug 3 FIX: was Application.Restart() — killed the entire process
        ' and restarted. Heavy-handed for just dismissing a toast. Now just
        ' slide out and close the form. The Loader (main form) stays alive
        ' and can show the next notification.
        autoClose.Stop()
        SlideOutAll()
    End Sub

    Private Sub Notifier_green_Click(sender As Object, e As EventArgs) Handles Notifier_green.Click, text_n.Click, icon_n.Click, Notifier_black.Click, Notifier_green_stop.Click
        Debug.WriteLine("[Notifier] Click → SlideOutAll")
        ' ✅ Bug 3 FIX: same as DoCloseClick — was Application.Restart().
        autoClose.Stop()
        SlideOutAll()
    End Sub

    Private Sub IF_N_Tick(sender As Object, e As EventArgs) Handles IF_N.Tick
        If Me.IsDisposed Then
            IF_N.Stop()
            Return
        End If

        Debug.WriteLine("[Notifier] IF_N tick → StartSlideY")
        StartSlideY(Me, Me.Top, 105, 200)
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If Not My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            IF_N.Stop()
            Debug.WriteLine("[Notifier] IF_N stopped")
        End If
    End Sub

#End Region

End Class