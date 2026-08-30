Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms


Public Class Notifier3
    Inherits Form

    ' ===== Toast slot geometry (OWNER spec) =====
    ' Slot 1 = the main toast ([2] Background.vb) — position unchanged.
    ' Slots 2/3 are copies stacking BELOW slot 1 with a 10px vertical gap.
    ' Pitch = unit design height (300x90, all three forms of a unit share it)
    ' + 10px gap, so slot 2 sits +100px and slot 3 +200px below slot 1.
    Private Const SlotIndex As Integer = 3
    Private Const SlotGapPx As Integer = 10

    Private ReadOnly Property SlotOffsetY As Integer
        Get
            Return (SlotIndex - 1) * (Me.Height + SlotGapPx)
        End Get
    End Property

    ' ===== T30 RaiseStack =====
    ' Explicit Y for the NEXT show, assigned by the Loader stack manager
    ' (the unit's rank in the active toast stack; newest = bottom).
    ' -1 = legacy behaviour: rest at this slot's classic offset below slot 1.
    ' Consumed once by Form_Load.
    Public Property UnitTargetY As Integer = -1

#Region "WinAPI"

    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000
    ' T30.1: never take focus - not on Show, not on click, not ever.
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED Or WS_EX_NOACTIVATE
            Return cp
        End Get
    End Property

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

        If panel Is Nothing OrElse panel.IsDisposed OrElse panel.Disposing Then Return ' T30: never animate a corpse

        Debug.WriteLine("[Notifier] StartSlide: " & panel.Name &
                        " X " & fromX & "→" & toX &
                        " dur=" & duration & "ms")
        ' T30.1: no TopMost setter here - it can activate the form. The
        ' no-activate z-order guarantee is RaiseUnit()'s job.

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

        If panel Is Nothing OrElse panel.IsDisposed OrElse panel.Disposing Then Return ' T30: never animate a corpse

        Debug.WriteLine("[Notifier] StartSlideY: " & panel.Name &
                        " Y " & fromY & "→" & toY &
                        " dur=" & duration & "ms")
        ' T30.1: no TopMost setter here (can activate) - see StartSlide.

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

            ' T30: a rider form (content/shadow) can be closed mid-flight by
            ' a dance or slide-out - drop its animation instead of writing
            ' .Top/.Left into a disposed form (would throw on the UI thread).
            If panel.IsDisposed OrElse panel.Disposing Then
                _activeAnims.Remove(panel)
                Continue For
            End If

            Dim elapsed = state.Sw.Elapsed.TotalMilliseconds
            Dim t As Double = elapsed / state.Duration

            If t >= 1 Then
                t = 1
                If state.IsSlideX Then
                    panel.Left = state.TargetX
                    SyncFollowersX(panel)
                Else
                    panel.Top = state.TargetY
                    SyncFollowersY(panel.Top)
                End If

                Debug.WriteLine("[Notifier] Animation COMPLETE " & panel.Name & " " &
                                If(state.IsSlideX, "X=" & state.TargetX, "Y=" & state.TargetY))

                _activeAnims.Remove(panel)
                If state.OnComplete IsNot Nothing Then finishedCallbacks.Add(state.OnComplete)
            Else
                Dim eased As Double = 1 - Math.Pow(1 - t, 3)

                If state.IsSlideX Then
                    panel.Left = CInt(state.StartX + (state.TargetX - state.StartX) * eased)
                    SyncFollowersX(panel)
                Else
                    panel.Top = CInt(state.StartY + (state.TargetY - state.StartY) * eased)
                    SyncFollowersY(panel.Top)
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
        Dim showY As Integer
        If UnitTargetY >= 0 Then
            ' T30: the Loader stack manager picked this unit's rank in the
            ' active stack - show exactly there, then consume the value.
            showY = UnitTargetY
            UnitTargetY = -1
            Debug.WriteLine("[Notifier3] Position Y=" & showY & " (stack-assigned)")
        Else
            Dim baseY As Integer
            If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
                baseY = 205
                Debug.WriteLine("[Notifier3] Base Y=205 (notifier_main present)")
            Else
                baseY = 105
                Debug.WriteLine("[Notifier3] Base Y=105")
            End If
            ' Slot 3 = two pitches below the main toast position
            showY = baseY + SlotOffsetY
            Debug.WriteLine("[Notifier3] Position Y=" & showY & " (slot " & SlotIndex & ")")
        End If
        Me.Location = New Point(w - Me.Width, showY)

        Notifier_black.Location = New Point(Me.Width, 0)
        Notifier_green.Location = New Point(Me.Width, 0)
        Notifier_green.Size = New Size(300, 90)
        Notifier_black.Size = New Size(300, 90)

        _delayTimers = New System.Windows.Forms.Timer()
        _delayTimers.Interval = 300
        AddHandler _delayTimers.Tick, Sub()
                                          _delayTimers.Stop()

                                          ' T30.1: no shadow during the slide - it fades in when
                                          ' the entrance COMPLETES (OWNER spec).
                                          Opacity = 1
                                          StartSlide(Notifier_green, Me.Width, Me.Width - 300, 200)
                                          StopDelayTimer()

                                          _delayTimer = New Timer()
                                          _delayTimer.Interval = 250
                                          AddHandler _delayTimer.Tick, Sub()
                                                                           _delayTimer.Stop()

                                                                           ' T30.1: the rider (Text+ICO) is visible from the
                                                                           ' first frame of the card slide and rides it -
                                                                           ' the engine carries it in the same 1 ms tick.
                                                                           Notifier_Sub3.Show()
                                                                           StartSlide(Notifier_black, Me.Width, Me.Width - 300, 300,
                                                                               Sub()
                                                                                   Notifier_green_stop.Visible = True
                                                                                   FadeInShadow()
                                                                               End Sub)
                                                                       End Sub
                                          _delayTimer.Start()

                                          autoClose.Interval = 6000
                                          RemoveHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          AddHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          autoClose.Start()

                                          TopMost = True
                                          Debug.WriteLine("[Notifier3] ===== Form Load Done =====")

                                      End Sub
        _delayTimers.Start()

    End Sub

    Private Sub AutoClose_Tick(sender As Object, e As EventArgs)
        Debug.WriteLine("[Notifier3] AutoClose triggered")
        autoClose.Stop()
        SlideOutAll()
    End Sub

    Private Sub Notifier_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Debug.WriteLine("[Notifier3] FormClosing — cleanup")
        Animation_Engine_CancelAll()
        autoClose.Stop()
        StopDelayTimer()
        StopCloseTimer()
        If _shadowFade IsNot Nothing Then
            _shadowFade.Stop()
            _shadowFade.Dispose()
            _shadowFade = Nothing
        End If
    End Sub

#End Region

#Region "Transition Guard"

    ' True while a replace dance or a slide-out exit is in flight. The
    ' router checks this so a 20ms notification burst can never start a
    ' second overlapping sequence on this unit - it coalesces the content
    ' onto the hidden rider instead, and whatever animation is already
    ' running surfaces it (latest wins, zero overlap).
    Private _inTransition As Boolean = False

    Public ReadOnly Property InTransition As Boolean
        Get
            Return _inTransition
        End Get
    End Property

    ' The router starts a replace dance through here. False = a dance is
    ' already running or the unit is sliding out - coalesce instead.
    ' T30.1: a dance slides the card away, so the shadow dies the moment
    ' the dance starts (OWNER rule) and is faded back in when the
    ' slide-in completes (Loader calls FadeInShadow in that callback).
    Public Function BeginDance() As Boolean
        If _inTransition Then Return False
        _inTransition = True
        CloseShadowNow()
        Return True
    End Function

    ' The router calls this when the slide-in back completed.
    Public Sub EndDance()
        _inTransition = False
    End Sub

#End Region

#Region "T30 RaiseStack - Unit Move / Z-Order"

    ' ===== T30.1: single-clock followers (OWNER: play the animation once,
    ' everything else syncs at 1 ms - simultaneously) =====
    ' The MM engine is the ONLY clock. Every tick that writes the card's
    ' position writes the rider (Text+ICO) and the shadow in the SAME tick,
    ' so all windows of a unit move as one rigid body - no follower can lag
    ' a frame behind. The shadow only exists while the unit is steady: it
    ' fades in AFTER a slide-in completes and closes the moment a slide-out
    ' starts, so it never needs its own position-sync timer.
    Private _shadowFade As System.Windows.Forms.Timer

    Private Sub SyncFollowersX(panel As Control)
        If Notifier_Sub3 IsNot Nothing AndAlso Not Notifier_Sub3.IsDisposed AndAlso Notifier_Sub3.Visible Then
            Notifier_Sub3.Left = Me.Left + panel.Left
        End If
        If Shadow3 IsNot Nothing AndAlso Not Shadow3.IsDisposed AndAlso Shadow3.Visible Then
            Shadow3.Left = Me.Left
            Shadow3.Top = Me.Top
        End If
    End Sub

    Private Sub SyncFollowersY(newTop As Integer)
        If Notifier_Sub3 IsNot Nothing AndAlso Not Notifier_Sub3.IsDisposed AndAlso Notifier_Sub3.Visible Then
            Notifier_Sub3.Top = newTop
        End If
        If Shadow3 IsNot Nothing AndAlso Not Shadow3.IsDisposed AndAlso Shadow3.Visible Then
            Shadow3.Top = newTop
            Shadow3.Left = Me.Left
        End If
    End Sub

    ' T30.1 OWNER rule: the slot's opening slide finishes ("done") -> the
    ' shadow fades in. Positioned under THIS unit's card (stack-aware Y),
    ' shown without activation, z-order fixed to Shadow < BG < Rider.
    Public Sub FadeInShadow()
        Try
            If Shadow3.IsDisposed OrElse Shadow3.Disposing Then Return
            Shadow3.Opacity = 0R
            Shadow3.Show()
            Shadow3.Location = New Point(Me.Left, Me.Top)
            RaiseUnit()
            If _shadowFade IsNot Nothing Then
                _shadowFade.Stop()
                _shadowFade.Dispose()
            End If
            _shadowFade = New System.Windows.Forms.Timer() With {.Interval = 15}
            AddHandler _shadowFade.Tick, Sub()
                                             If Shadow3.IsDisposed OrElse Shadow3.Disposing Then
                                                 _shadowFade.Stop()
                                                 Return
                                             End If
                                             If Shadow3.Opacity >= 1 Then
                                                 Shadow3.Opacity = 1R
                                                 _shadowFade.Stop()
                                                 RaiseUnit()
                                             Else
                                                 Shadow3.Opacity = Math.Min(1R, Shadow3.Opacity + 0.1R)
                                             End If
                                         End Sub
            _shadowFade.Start()
        Catch ex As Exception
            Debug.WriteLine("[Notifier3] FadeInShadow error: " & ex.Message)
        End Try
    End Sub

    ' T30.1 OWNER rule: the moment a closing slide starts - in every case -
    ' the shadow closes. No fade-out, no sync, just gone.
    Public Sub CloseShadowNow()
        Try
            If Shadow3 IsNot Nothing AndAlso Not Shadow3.IsDisposed AndAlso Not Shadow3.Disposing Then
                Shadow3.Close()
            End If
        Catch ex As Exception
            Debug.WriteLine("[Notifier3] CloseShadowNow error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' T30/T30.1: glide the unit to a new Y so the stack can compact when a
    ''' toast above closes. Single clock: this form animates; the rider and
    ''' the shadow are carried along by the same 1 ms engine tick. Safe on
    ''' any state - per-control animation replace means the last call wins.
    ''' </summary>
    Public Sub ReflowTo(targetY As Integer, Optional durationMs As Integer = 250)
        If Me.IsDisposed OrElse Me.Disposing Then Return
        If Me.Top = targetY Then Return

        ' T30.1: animate THIS form only - the engine tick carries the rider
        ' and the shadow along in the same tick (single clock, no drift).
        StartSlideY(Me, Me.Top, targetY, durationMs)
    End Sub

    ' T30: re-assert HWND_TOPMOST for every window of the unit WITHOUT
    ' stealing focus (SetWindowPos + SWP_NOACTIVATE, never Form.TopMost).
    Private Const SWP_NOSIZE_FLAG As Integer = &H1
    Private Const SWP_NOMOVE_FLAG As Integer = &H2
    Private Const SWP_NOACTIVATE_FLAG As Integer = &H10
    Private Shared ReadOnly HwndTopmost As New IntPtr(-1)

    <DllImport("user32.dll")>
    Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
                                         x As Integer, y As Integer,
                                         cx As Integer, cy As Integer,
                                         uFlags As Integer) As Boolean
    End Function

    ''' <summary>T30: raise the whole unit (card + content + shadow) above
    ''' other topmost windows - fullscreen borderless games included -
    ''' without activation.</summary>
    Public Sub RaiseUnit()
        Try
            ' T30.1 OWNER z-order spec, bottom -> top:
            '   1. Shadow   2. BG (this form)   3. Text+ICO (rider on top)
            ' Each HWND_TOPMOST call moves the window to the TOP of the
            ' topmost band, so the LAST call ends up on top of the unit.
            If Shadow3 IsNot Nothing AndAlso Not Shadow3.IsDisposed AndAlso Shadow3.IsHandleCreated Then
                RaiseWindow(Shadow3.Handle)
            End If
            RaiseWindow(Me.Handle)
            If Notifier_Sub3 IsNot Nothing AndAlso Not Notifier_Sub3.IsDisposed AndAlso Notifier_Sub3.IsHandleCreated Then
                RaiseWindow(Notifier_Sub3.Handle)
            End If
        Catch ex As Exception
            Debug.WriteLine("[Notifier3] RaiseUnit error: " & ex.Message)
        End Try
    End Sub

    Private Sub RaiseWindow(h As IntPtr)
        SetWindowPos(h, HwndTopmost, 0, 0, 0, 0,
                     SWP_NOSIZE_FLAG Or SWP_NOMOVE_FLAG Or SWP_NOACTIVATE_FLAG)
    End Sub

#End Region

#Region "Slide Out"

    Private Sub SlideOutAll()
        Debug.WriteLine("[Notifier3] SlideOutAll")
        _inTransition = True

        ' T30.1 OWNER rule: the closing slide STARTS first - the shadow is
        ' closed this instant, in every case. The rider (Text+ICO) is NOT
        ' closed here anymore: it rides the slide-out (the engine carries
        ' it) and is closed together with the unit below.
        Shadow3.Close()
        Notifier_green_stop.Visible = False

        ' Both panels now animate independently — Notifier_green's slide (started
        ' 200ms later, below) no longer interrupts Notifier_black's in-flight one.
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
                                                                          Notifier_Sub3.Close()
                                                                          Me.Close()
                                                                      End Sub
                                         _closeTimer.Start()
                                     End Sub
        _delayTimer.Start()
    End Sub

#End Region

#Region "Click Events"

    Public Sub DoCloseClick()
        Debug.WriteLine("[Notifier3] DoCloseClick → Restart")
        Application.Restart()
    End Sub

    Private Sub Notifier_green_Click(sender As Object, e As EventArgs) Handles Notifier_green.Click, text_n.Click, icon_n.Click, Notifier_black.Click, Notifier_green_stop.Click
        Debug.WriteLine("[Notifier3] Click → Restart")
        Application.Restart()
    End Sub

    Private Sub IF_N_Tick(sender As Object, e As EventArgs) Handles IF_N.Tick
        If Me.IsDisposed Then
            IF_N.Stop()
            Return
        End If

        Debug.WriteLine("[Notifier3] IF_N tick → StartSlideY")
        StartSlideY(Me, Me.Top, 105 + SlotOffsetY, 200)
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If Not My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            IF_N.Stop()
            Debug.WriteLine("[Notifier3] IF_N stopped")
        End If
    End Sub

#End Region

End Class
