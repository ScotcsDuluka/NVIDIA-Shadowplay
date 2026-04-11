Imports System.Runtime.InteropServices

Partial Public Class Base

#Region "Animation Engine - MM Timer"

    ' ============================================
    ' ★ Variables
    ' ============================================
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

    ' ============================================
    ' ★ MM Timer Declarations (High Precision)
    ' ============================================
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

    ' ============================================
    ' ★ State Variables
    ' ============================================
    Private mmTimerId As UInteger = 0
    Private mmCallback As MMTimerProc       ' Keep reference alive → prevent GC
    Private sw As Stopwatch
    Private _invokePending As Boolean = False

    ' ============================================
    ' ★ Timer Start
    ' ============================================
    Private Sub Animation_Engine_Start()
        ' Kill old timer if running
        If mmTimerId <> 0 Then
            timeKillEvent(mmTimerId)
            mmTimerId = 0
        End If

        ' Set high resolution
        timeBeginPeriod(1)

        ' Create callback delegate (keep in class variable!)
        mmCallback = New MMTimerProc(AddressOf OnMMTick)

        ' Start timer: 16ms interval (~60 FPS), 1ms resolution
        mmTimerId = timeSetEvent(16, 1, mmCallback, UIntPtr.Zero,
                                  TIME_PERIODIC Or TIME_KILL_SYNCHRONOUS)
    End Sub

    ' ============================================
    ' ★ Timer Stop
    ' ============================================
    Private Sub Animation_Engine_Stop()
        If mmTimerId <> 0 Then
            timeKillEvent(mmTimerId)
            mmTimerId = 0
        End If

        timeEndPeriod(1)
        _invokePending = False

        If sw IsNot Nothing Then sw.Stop()
    End Sub

    ' ============================================
    ' ★ MM Timer Callback (runs on separate thread)
    ' ============================================
    Private Sub OnMMTick(uID As UInteger, uMsg As UInteger,
                          dwUser As UIntPtr, dw1 As UInteger, dw2 As UInteger)

        ' Safety: skip if disposed
        If Me.IsDisposed OrElse Me.Disposing Then Return

        ' Throttle: skip if previous tick still waiting
        If _invokePending Then Return
        _invokePending = True

        Try
            ' Marshal to UI Thread
            Me.BeginInvoke(Sub()
                               Try
                                   _invokePending = False
                                   Animation_Engine_Tick()
                               Catch ex As Exception
                                   _invokePending = False
                               End Try
                           End Sub)
        Catch ex As ObjectDisposedException
            _invokePending = False
        Catch ex As InvalidOperationException
            _invokePending = False
        End Try
    End Sub

    ' ============================================
    ' ★ PUBLIC: Animate X (Left/Right)
    ' ============================================
    Public Sub ANX(panel As Control,
                   fromX As Integer,
                   toX As Integer,
                   durationMs As Double,
                   Optional completed As Action = Nothing)

        ' Null check
        If panel Is Nothing Then Return

        ' Stop existing animation
        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        ' Set values
        currentPanel = panel
        startX = fromX
        targetX = toX
        animDuration = durationMs
        onComplete = completed
        isSlideX = True

        ' Initialize position & start
        panel.Left = fromX
        animStart = DateTime.Now
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    ' ============================================
    ' ★ PUBLIC: Animate Y (Top/Bottom)
    ' ============================================
    Public Sub AMY(panel As Control,
                   fromY As Integer,
                   toY As Integer,
                   durationMs As Double,
                   Optional completed As Action = Nothing)

        ' Null check
        If panel Is Nothing Then Return

        ' Stop existing animation
        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        ' Set values
        currentPanel = panel
        startY = fromY
        targetY = toY
        animDuration = durationMs
        onComplete = completed
        isSlideX = False

        ' Initialize position & start
        panel.Top = fromY
        animStart = DateTime.Now
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    ' ============================================
    ' ★ Animation Tick (runs on UI Thread)
    ' ============================================
    Private Sub Animation_Engine_Tick()
        ' Quick exit if not running
        If Not animationRunning Then Return

        ' Safety check
        If currentPanel Is Nothing OrElse currentPanel.IsDisposed Then
            animationRunning = False
            Animation_Engine_Stop()
            Return
        End If

        ' Calculate progress
        Dim elapsed As Double = sw.Elapsed.TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        ' Check complete
        If t >= 1.0 Then
            t = 1.0
            animationRunning = False
            Animation_Engine_Stop()

            ' Force final position (exact value)
            If isSlideX Then
                currentPanel.Left = targetX
            Else
                currentPanel.Top = targetY
            End If

            ' Fire callback safely
            Dim callback As Action = onComplete
            onComplete = Nothing

            If callback IsNot Nothing Then
                Try
                    callback.Invoke()
                Catch ex As Exception
                    Debug.WriteLine("[Anim] Callback Error: " & ex.Message)
                End Try
            End If

            Return
        End If

        ' Ease Out Cubic (smooth deceleration)
        Dim eased As Double = 1.0 - Math.Pow(1.0 - t, 3)

        ' Apply position
        If isSlideX Then
            currentPanel.Left = CInt(startX + (targetX - startX) * eased)
        Else
            currentPanel.Top = CInt(startY + (targetY - startY) * eased)
        End If
    End Sub

#End Region

End Class