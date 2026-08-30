Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms


Public Class Notifier2
    Inherits Form

    ' ===== T27: dynamic slot geometry (OWNER spec) =====
    ' Same model as the sibling Notifier unit — a slot is a SCREEN ROW.
    ' This unit used to be pinned to "slot 2" (+100px); now the router
    ' assigns CurrentRow per show, so it can end up on row 0 after a reflow.
    Private Const SlotGapPx As Integer = 10

    Public CurrentRow As Integer = 0
    Public UnitId As Integer = 0

    Private ReadOnly Property RowOffsetY As Integer
        Get
            Return CurrentRow * (Me.Height + SlotGapPx)
        End Get
    End Property

    ''' <summary>Base Y of row 0 — 205 when the in-game "main" notifier block
    ''' is up (notifier_main flag), 105 otherwise. Shared: the Loader's router
    ''' reflow uses it as the glide target, too.</summary>
    Public Shared Function BaseRowY() As Integer
        If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            Return 205
        End If
        Return 105
    End Function

    ' ===== T27: slot lifecycle signals =====
    ' Shared on purpose — toast forms are VB default instances, recreated
    ' after every close, so per-instance subscriptions would die with them.
    Public Shared Event SlideOutStarted(sender As Form)
    Public Shared Event DanceCompleted(sender As Form)
    Public Shared Event UnitClosed(sender As Form)

    ' Set at SlideOutAll start; a fresh default instance resets it naturally.
    Private _isClosing As Boolean = False

    ' ===== T27.1: Y-riders — one animation clock for the whole toast =====
    ' A toast is 3 stacked windows (card / content overlay / shadow) and they
    ' used to be animated by 3 independent clocks (MM timer / WM_TIMER /
    ' 16ms sync poll). On a loaded UI thread the overlay's WM_TIMER starves
    ' and misses the reflow glide — OWNER saw "ICO + Text vanish while
    ' sliding up" — and the shadow lagged the card ("ตามไม่ทัน เห็นนิดๆ").
    ' Now every Y move of THIS form drags both riders in the SAME engine
    ' tick: same clock, same easing, atomic alignment.
    ' Strong refs only — never touch the VB default instances here, so a
    ' closed rider is detected via IsDisposed instead of silently
    ' auto-recreated as a hidden ghost form.
    Private ContentOverlay As Form = Nothing
    Private ShadowRider As Form = Nothing

    Private Sub MoveRidersToY(y As Integer)
        If ContentOverlay IsNot Nothing AndAlso Not ContentOverlay.IsDisposed Then
            ContentOverlay.Top = y
        End If
        If ShadowRider IsNot Nothing AndAlso Not ShadowRider.IsDisposed Then
            ShadowRider.Top = y
        End If
    End Sub

    ''' <summary>T27.1: runs when a Y animation of this card completes.
    ''' Pins the riders to the final Y; if the content overlay died mid-show
    ''' (the OWNER-visible vanish) it is resurrected — re-anchored to the
    ''' card and faded back in by its own Form_Load.</summary>
    Public Sub SettleRiders()
        If _isClosing Then Return
        If ContentOverlay IsNot Nothing AndAlso Not ContentOverlay.IsDisposed Then
            ContentOverlay.Top = Me.Top
        Else
            Try
                Notifier_Sub2.Show()
                ContentOverlay = Notifier_Sub2
            Catch
                ' overlay default instance unavailable — leave the bare card
            End Try
        End If
        If ShadowRider IsNot Nothing AndAlso Not ShadowRider.IsDisposed Then
            ShadowRider.Top = Me.Top
        End If
    End Sub

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

    Private Class AnimState
        Public Sw As Stopwatch
        Public Duration As Double
        Public StartX As Integer
        Public TargetX As Integer
        Public StartY As Integer
        Public TargetY As Integer
        Public IsSlideX As Boolean
        Public OnComplete As Action
    End Class

    Private ReadOnly _activeAnims As New Dictionary(Of Control, AnimState)()

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

    ' Guards against BeginInvoke calls stacking up if a UI-thread tick is still
    ' processing when the next 1ms MM tick fires.
    Private _invokePending As Boolean = False

    Private Sub Animation_Engine_Start()
        If mmTimerId <> 0 Then Return ' already running — nothing to do
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
        End If
        timeEndPeriod(1)
        _invokePending = False
        Debug.WriteLine("[Notifier.MM] Timer stopped")
    End Sub

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
        Catch ex As InvalidOperationException
            ' T27.2: handle already destroyed (mid-close race) — an unhandled
            ' exception on the winmm callback thread would kill the process.
            _invokePending = False
            Debug.WriteLine("[Notifier.MM] BeginInvoke failed — no handle")
        End Try
    End Sub

    ''' <summary>
    ''' Slides a control's Left between two X positions. If the same control
    ''' already has an animation running, that one is replaced — but any OTHER
    ''' control's in-flight animation is left untouched.
    ''' </summary>
    Public Sub StartSlide(panel As Control,
                           fromX As Integer,
                           toX As Integer,
                           duration As Double,
                           Optional completed As Action = Nothing)

        Debug.WriteLine("[Notifier] StartSlide: " & panel.Name &
                        " X " & fromX & "→" & toX &
                        " dur=" & duration & "ms")
        TopMost = True

        Dim state As New AnimState With {
            .Sw = Stopwatch.StartNew(),
            .Duration = duration,
            .StartX = fromX,
            .TargetX = toX,
            .IsSlideX = True,
            .OnComplete = completed
        }

        panel.Left = fromX
        _activeAnims(panel) = state ' replaces only THIS control's animation, if any

        Animation_Engine_Start()
    End Sub

    ''' <summary>Same as <see cref="StartSlide"/> but for a control's Top (Y axis).</summary>
    Public Sub StartSlideY(panel As Control,
                           fromY As Integer,
                           toY As Integer,
                           duration As Double,
                           Optional completed As Action = Nothing)

        Debug.WriteLine("[Notifier] StartSlideY: " & panel.Name &
                        " Y " & fromY & "→" & toY &
                        " dur=" & duration & "ms")
        TopMost = True

        Dim state As New AnimState With {
            .Sw = Stopwatch.StartNew(),
            .Duration = duration,
            .StartY = fromY,
            .TargetY = toY,
            .IsSlideX = False,
            .OnComplete = completed
        }

        panel.Top = fromY
        _activeAnims(panel) = state
        If panel Is Me Then MoveRidersToY(fromY) ' T27.1: riders glued from frame one

        Animation_Engine_Start()
    End Sub

    ''' <summary>Cancels every in-flight animation without invoking their completion callbacks. Used on form close/hide.</summary>
    Private Sub Animation_Engine_CancelAll()
        _activeAnims.Clear()
        Animation_Engine_Stop()
    End Sub

    Private Sub Animation_Engine_Tick()
        If _activeAnims.Count = 0 Then
            Animation_Engine_Stop()
            Return
        End If

        ' Snapshot the keys — callbacks fired below may add/replace entries in _activeAnims.
        Dim controls As New List(Of Control)(_activeAnims.Keys)
        Dim finishedCallbacks As New List(Of Action)()

        For Each panel In controls
            Dim state As AnimState = Nothing
            If Not _activeAnims.TryGetValue(panel, state) Then Continue For ' removed mid-loop

            Dim elapsed = state.Sw.Elapsed.TotalMilliseconds
            Dim t As Double = elapsed / state.Duration

            If t >= 1 Then
                t = 1
                If state.IsSlideX Then
                    panel.Left = state.TargetX
                Else
                    panel.Top = state.TargetY
                    If panel Is Me Then MoveRidersToY(state.TargetY) ' T27.1
                End If

                Debug.WriteLine("[Notifier] Animation COMPLETE " & panel.Name & " " &
                                If(state.IsSlideX, "X=" & state.TargetX, "Y=" & state.TargetY))

                _activeAnims.Remove(panel)
                If state.OnComplete IsNot Nothing Then finishedCallbacks.Add(state.OnComplete)
            Else
                Dim eased As Double = 1 - Math.Pow(1 - t, 3)

                If state.IsSlideX Then
                    panel.Left = CInt(state.StartX + (state.TargetX - state.StartX) * eased)
                Else
                    Dim newY As Integer = CInt(state.StartY + (state.TargetY - state.StartY) * eased)
                    panel.Top = newY
                    If panel Is Me Then MoveRidersToY(newY) ' T27.1: same tick, same easing
                End If
            End If
        Next

        If _activeAnims.Count = 0 Then
            Animation_Engine_Stop()
        End If

        ' Fire completion callbacks after bookkeeping is consistent, so a callback
        ' that immediately starts another animation sees clean state.
        For Each cb In finishedCallbacks
            Try
                cb.Invoke()
            Catch ex As Exception
                Debug.WriteLine("[Notifier] onComplete ERROR: " & ex.Message)
            End Try
        Next
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

        Dim w As Integer = Screen.PrimaryScreen.WorkingArea.Width
        ' T27: row-aware — the router may place this unit on row 0 or row 1
        Me.Location = New Point(w - Me.Width, BaseRowY() + RowOffsetY)
        Debug.WriteLine($"[Notifier2] Position Y={Me.Top} (row {CurrentRow})")
        ContentOverlay = Notifier_Sub2   ' T27.1: strong ref to this cycle's overlay

        Notifier_black.Location = New Point(Me.Width, 0)
        Notifier_green.Location = New Point(Me.Width, 0)
        Notifier_green.Size = New Size(300, 90)
        Notifier_black.Size = New Size(300, 90)

        _delayTimers = New System.Windows.Forms.Timer()
        _delayTimers.Interval = 300
        AddHandler _delayTimers.Tick, Sub()
                                          _delayTimers.Stop()

                                          Shadow2.Show()
                                          ShadowRider = Shadow2   ' T27.1
                                          Opacity = 1
                                          StartSlide(Notifier_green, Me.Width, Me.Width - 300, 200)
                                          StopDelayTimer()

                                          _delayTimer = New Timer()
                                          _delayTimer.Interval = 250
                                          AddHandler _delayTimer.Tick, Sub()
                                                                           _delayTimer.Stop()

                                                                           StartSlide(Notifier_black, Me.Width, Me.Width - 300, 300,
                                                                               Sub()
                                                                                   Notifier_Sub2.Show()
                                                                                   Notifier_green_stop.Visible = True
                                                                                   RaiseEvent DanceCompleted(Me)
                                                                               End Sub)
                                                                       End Sub
                                          _delayTimer.Start()

                                          autoClose.Interval = 6000
                                          RemoveHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          AddHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          autoClose.Start()

                                          TopMost = True
                                          Debug.WriteLine("[Notifier2] ===== Form Load Done =====")

                                      End Sub
        _delayTimers.Start()

    End Sub

    Private Sub AutoClose_Tick(sender As Object, e As EventArgs)
        Debug.WriteLine("[Notifier2] AutoClose triggered")
        autoClose.Stop()
        SlideOutAll()
    End Sub

    Private Sub Notifier_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Debug.WriteLine("[Notifier2] FormClosing — cleanup")
        Animation_Engine_CancelAll()
        autoClose.Stop()
        StopDelayTimer()
        StopCloseTimer()
        ContentOverlay = Nothing   ' T27.1
        ShadowRider = Nothing      ' T27.1
    End Sub

    ' T27: the router waits for this to free the slot / serve the queue
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        RaiseEvent UnitClosed(Me)
    End Sub

#End Region

#Region "Slide Out"

    Private Sub SlideOutAll()
        ' T27: idempotent — a click racing the auto-close must not re-enter
        If _isClosing Then Return
        _isClosing = True
        Debug.WriteLine("[Notifier2] SlideOutAll")
        RaiseEvent SlideOutStarted(Me)

        Shadow2.Close()
        Notifier_Sub2.Close()
        Notifier_green_stop.Visible = False

        ' Both panels now animate independently — Notifier_green's slide (started
        ' 200ms later, below) no longer interrupts Notifier_black's in-flight one.
        StartSlide(Notifier_black, Notifier_black.Left, Me.Width + 300, 600)

        StopDelayTimer()
        _delayTimer = New Timer()
        _delayTimer.Interval = 200
        AddHandler _delayTimer.Tick, Sub()
                                         _delayTimer.Stop()
                                         ' T27.2: close the card when the green panel is
                                         ' FULLY out — the old +200ms close timer fired while
                                         ' the exit slide was mid-flight (green only ~1/3 out),
                                         ' so CancelAll froze it and the toast popped out of
                                         ' existence. Completion-callback close = smooth exit,
                                         ' and UnitClosed lands after the visible exit ends.
                                         StartSlide(Notifier_green, Notifier_green.Left, Me.Width + 300, 600,
                                                    Sub()
                                                        Me.Close()
                                                    End Sub)
                                     End Sub
        _delayTimer.Start()
    End Sub

#End Region

#Region "Click Events"

    ' T27: clicking a toast dismisses ONLY that toast (clean slide-out).
    ' The old Application.Restart() nuked both slots and the pending queue.
    Public Sub DoCloseClick()
        Debug.WriteLine("[Notifier2] DoCloseClick → dismiss this toast")
        SlideOutAll()
    End Sub

    Private Sub Notifier_green_Click(sender As Object, e As EventArgs) Handles Notifier_green.Click, text_n.Click, icon_n.Click, Notifier_black.Click, Notifier_green_stop.Click
        Debug.WriteLine("[Notifier2] Click → dismiss this toast")
        SlideOutAll()
    End Sub

    Private Sub IF_N_Tick(sender As Object, e As EventArgs) Handles IF_N.Tick
        If Me.IsDisposed Then
            IF_N.Stop()
            Return
        End If

        Debug.WriteLine("[Notifier2] IF_N tick → StartSlideY")
        ' T27: back to this unit's own row Y (base 105 — main block is gone)
        StartSlideY(Me, Me.Top, 105 + RowOffsetY, 200)
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If Not My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            IF_N.Stop()
            Debug.WriteLine("[Notifier2] IF_N stopped")
        End If
    End Sub

#End Region

End Class
