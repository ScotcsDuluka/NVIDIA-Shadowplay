Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms


Public Class Notifier2
    Inherits Form

    ' ===== Toast slot geometry (OWNER spec) =====
    ' Slot 1 = the main toast ([2] Background.vb) — position unchanged.
    ' Slots 2/3 are copies stacking BELOW slot 1 with a 10px vertical gap.
    ' Pitch = unit design height (300x90, all three forms of a unit share it)
    ' + 10px gap, so slot 2 sits +100px and slot 3 +200px below slot 1.
    Private Const SlotIndex As Integer = 2
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

    ' ===== T30.6 FPS FIX: the frame pump lives ON the UI thread =====
    ' History: T30.2 marshalled every 1 ms MM tick into a BeginInvoke post
    ' (pump flood, ~20 fps on real hardware). T30.5 gated the post on the
    ' MM thread (6 ms) - better, but the cross-thread hop itself remained:
    ' under game load each round trip (MM thread -> posted message -> UI
    ' dispatch) still stretched into tens of ms, so positions landed at
    ' only ~30 Hz. The T29-era engine applied INLINE on the UI thread and
    ' was smooth on the same machine - so the hop is what had to go.
    ' Now one UI-thread pump timer (Interval=1; UnitClockRes holds 1 ms
    ' system resolution while the engine runs) checks the frame clock and
    ' applies every animation INLINE:
    '   - no cross-thread hop left that can stretch under game load
    '   - WM_TIMER never backlogs (it only fires when the queue is empty),
    '     so a stalled pump self-heals on the next tick with time-correct
    '     easing - no burst, no backlog, no invoke-pending bookkeeping
    '   - easing stays stopwatch-based; the 6 ms gate = ~167 writes/s,
    '     above every common display refresh
    Private ReadOnly _engineSw As New Stopwatch()
    Private _lastFrameMs As Integer = -1000000
    Private Const FrameIntervalMs As Integer = 6
    Private _uiPump As System.Windows.Forms.Timer

    Private Sub Animation_Engine_Start()
        If _uiPump IsNot Nothing Then Return ' already running — nothing to do
        _engineSw.Restart()     ' new frame-clock epoch for this run
        _lastFrameMs = -1000000 ' first frame applies immediately
        UnitClockRes.Acquire() ' T30.2: refcounted - timeBeginPeriod is process-global
        _uiPump = New System.Windows.Forms.Timer()
        _uiPump.Interval = 1
        AddHandler _uiPump.Tick, AddressOf UiPump_Tick
        _uiPump.Start()
        Debug.WriteLine("[Notifier.MM] UI-thread frame pump started")
    End Sub

    Private Sub Animation_Engine_Stop()
        If _uiPump IsNot Nothing Then
            _uiPump.Stop()
            _uiPump.Dispose()
            _uiPump = Nothing
        End If
        UnitClockRes.Release() ' T30.2: only the LAST idle engine drops 1 ms
        Debug.WriteLine("[Notifier.MM] UI-thread frame pump stopped")
    End Sub

    ' Runs on the UI thread. A not-due tick costs one stopwatch compare; a
    ' due tick applies every active animation inline (zero marshalling).
    Private Sub UiPump_Tick(sender As Object, e As EventArgs)
        If Me.IsDisposed OrElse Me.Disposing Then Return
        If CInt(_engineSw.Elapsed.TotalMilliseconds) - _lastFrameMs < FrameIntervalMs Then Return
        Animation_Engine_Tick()
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

        ' T30.5: engine-wide frame gate (see the frame-clock note above).
        Dim nowMs As Integer = CInt(_engineSw.Elapsed.TotalMilliseconds)
        Dim frameDue As Boolean = nowMs - _lastFrameMs >= FrameIntervalMs
        Dim wroteAny As Boolean = False

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
                    ApplySlideX(panel, state.TargetX)
                ElseIf panel Is Me Then
                    ApplyUnitY(state.TargetY)
                Else
                    panel.Top = state.TargetY
                End If

                Debug.WriteLine("[Notifier] Animation COMPLETE " & panel.Name & " " &
                                If(state.IsSlideX, "X=" & state.TargetX, "Y=" & state.TargetY))

                _activeAnims.Remove(panel)
                If state.OnComplete IsNot Nothing Then finishedCallbacks.Add(state.OnComplete)
            Else
                ' T30.5: ONE engine-wide frame cadence (was per-anim ~8 ms
                ' checked at 1000 Hz). Easing is still computed from each
                ' anim's stopwatch, so motion speed stays time-correct even
                ' if a frame is dropped under load.
                If Not frameDue Then Continue For

                Dim eased As Double = 1 - Math.Pow(1 - t, 3)

                If state.IsSlideX Then
                    ApplySlideX(panel, CInt(state.StartX + (state.TargetX - state.StartX) * eased))
                ElseIf panel Is Me Then
                    ApplyUnitY(CInt(state.StartY + (state.TargetY - state.StartY) * eased))
                Else
                    panel.Top = CInt(state.StartY + (state.TargetY - state.StartY) * eased)
                End If

                wroteAny = True
            End If
        Next

        If wroteAny Then _lastFrameMs = nowMs ' T30.5: frame applied

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
            Debug.WriteLine("[Notifier2] Position Y=" & showY & " (stack-assigned)")
        Else
            Dim baseY As Integer
            If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
                baseY = 205
                Debug.WriteLine("[Notifier2] Base Y=205 (notifier_main present)")
            Else
                baseY = 105
                Debug.WriteLine("[Notifier2] Base Y=105")
            End If
            ' Slot 2 = one pitch below the main toast position
            showY = baseY + SlotOffsetY
            Debug.WriteLine("[Notifier2] Position Y=" & showY & " (slot " & SlotIndex & ")")
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

                                                                           ' T30.3 OWNER: the rider (Text+ICO) is a STILL
                                                                           ' window - it never rides. It is revealed only
                                                                           ' when the card is PARKED, reborn anchored on it
                                                                           ' (its Load) and stays still for life.
                                                                           StartSlide(Notifier_black, Me.Width, Me.Width - 300, 300,
                                                                               Sub()
                                                                                   Notifier_green_stop.Visible = True
                                                                                   Notifier_Sub2.Reveal(Me.Left, Me.Top, Me.Notifier_black.Left)
                                                                                   FadeInShadow()
                                                                               End Sub)
                                                                       End Sub
                                          _delayTimer.Start()

                                          autoClose.Interval = 6000
                                          RemoveHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          AddHandler autoClose.Tick, AddressOf AutoClose_Tick
                                          autoClose.Start()

                                          ' T30.2: no Form.TopMost here. It is SetWindowPos(HWND_TOPMOST)
                                          ' without SWP_NOACTIVATE - it can activate the toast and it
                                          ' lifts the BG above the rider/shadow until the heartbeat
                                          ' re-sorts the band (the z-order flap seen as flicker).
                                          ' RaiseUnit()/FadeInShadow own the z-order, activation-free.
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

    ' T30.4 OWNER rule "key ซ้ำ ให้ใช้ Updater UI > Slot นั้นๆ": True for the
    ' whole replace-dance window (BeginDance..EndDance). Routing uses it to
    ' tell a DANCE (unit survives - a same-key repeat coalesces onto the
    ' dance already in flight and surfaces on THIS slot) apart from a
    ' mid-EXIT (unit is dying - queue instead of painting a corpse).
    Public ReadOnly Property IsDancing As Boolean
        Get
            Return _isDancing
        End Get
    End Property
    Private _isDancing As Boolean = False

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
    ' T30.3 OWNER: the still rider never rides the dance either - it is
    ' closed the instant the card leaves; the reveal Show()s it fresh,
    ' reborn anchored on the parked card and fading in in place.
    Public Function BeginDance() As Boolean
        If _inTransition Then Return False
        _inTransition = True
        _isDancing = True
        _activeReflowTarget = Integer.MinValue ' T30.6: the dance owns the Y axis now
        CloseShadowNow()
        ' T30.7: Hide, never Close. Form.Close() DISPOSES the form - the
        ' reveal's Show() then threw ObjectDisposedException inside the
        ' engine callback, silently skipping EndDance/FadeInShadow: zombie
        ' unit, shadow gone until the whole unit got reborn (the blinking
        ' Shadow form). A hidden rider keeps its handle and is reborn by
        ' Reveal() instead.
        Notifier_Sub2.Hide()
        Return True
    End Function

    ' The router calls this when the slide-in back completed.
    Public Sub EndDance()
        _inTransition = False
        _isDancing = False
    End Sub

#End Region

#Region "T30 RaiseStack - Unit Move / Z-Order"

    ' ===== T30.1/T30.3: single-clock unit (OWNER) =====
    ' The MM engine is the ONLY clock, and each axis has ONE leader:
    '   X (entrance/dance/exit slides): the CARD leads - it is the only
    '       thing that moves. The rider (Text+ICO) is a still window that
    '       exists only while the card is parked; the shadow is only alive
    '       while steady - neither is ever written during an X slide.
    '   Y (stack reflow): THIS FORM leads and the rider + shadow are
    '       carried in the SAME tick (ApplyUnitY) - one rigid body.
    ' The shadow fades in AFTER a slide-in completes and closes the moment
    ' a slide-out starts, so it never needs its own position-sync timer.
    Private _shadowFade As System.Windows.Forms.Timer

    ' ===== T30.2: one atomic move per engine apply =====
    ' BeginDeferWindowPos/EndDeferWindowPos applies every window position
    ' update in a SINGLE compositor pass - DWM can never snapshot the card
    ' with the rider (Text+ICO) or the shadow still a frame behind (the
    ' "swimming text" tearing). Everything still lands inside the same
    ' 1 ms engine tick and now inside the same displayed frame too.
    Private Structure PendingMove
        Public Hwnd As IntPtr
        Public X As Integer
        Public Y As Integer
    End Structure

    Private Const SWP_NOZORDER_FLAG As Integer = &H4

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function BeginDeferWindowPos(nCount As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function DeferWindowPos(hWinPosInfo As IntPtr, hWnd As IntPtr,
                                           hWndInsertAfter As IntPtr,
                                           x As Integer, y As Integer,
                                           cx As Integer, cy As Integer,
                                           uFlags As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function EndDeferWindowPos(hWinPosInfo As IntPtr) As Boolean
    End Function

    Private Sub FlushMoves(moves As List(Of PendingMove))
        If moves.Count = 0 Then Return
        Try
            Dim h As IntPtr = BeginDeferWindowPos(moves.Count)
            If h = IntPtr.Zero Then Throw New InvalidOperationException("BeginDeferWindowPos failed")
            For Each m As PendingMove In moves
                h = DeferWindowPos(h, m.Hwnd, IntPtr.Zero, m.X, m.Y, 0, 0,
                                   SWP_NOSIZE_FLAG Or SWP_NOZORDER_FLAG Or SWP_NOACTIVATE_FLAG)
                If h = IntPtr.Zero Then Throw New InvalidOperationException("DeferWindowPos failed")
            Next
            EndDeferWindowPos(h)
        Catch ex As Exception
            ' Defensive fallback: plain per-window moves - identical end state.
            For Each m As PendingMove In moves
                SetWindowPos(m.Hwnd, IntPtr.Zero, m.X, m.Y, 0, 0,
                             SWP_NOSIZE_FLAG Or SWP_NOZORDER_FLAG Or SWP_NOACTIVATE_FLAG)
            Next
        End Try
    End Sub

    ' X slide (entrance/dance): the card PANEL is the driver; the rider
    ' follows at the panel's absolute X; the shadow only realigns with the
    ' (stationary) form when it needs to.
    Private Sub ApplySlideX(panel As Control, newX As Integer)
        Dim moves As New List(Of PendingMove)()
        If panel.Left <> newX Then
            moves.Add(New PendingMove With {.Hwnd = panel.Handle, .X = newX, .Y = panel.Top})
        End If
        If Notifier_Sub2 IsNot Nothing AndAlso Not Notifier_Sub2.IsDisposed AndAlso Notifier_Sub2.Visible Then
            Dim riderX As Integer = Me.Left + newX
            If Notifier_Sub2.Left <> riderX Then
                moves.Add(New PendingMove With {.Hwnd = Notifier_Sub2.Handle, .X = riderX, .Y = Notifier_Sub2.Top})
            End If
        End If
        If Shadow2 IsNot Nothing AndAlso Not Shadow2.IsDisposed AndAlso Shadow2.Visible Then
            If Shadow2.Left <> Me.Left OrElse Shadow2.Top <> Me.Top Then
                moves.Add(New PendingMove With {.Hwnd = Shadow2.Handle, .X = Me.Left, .Y = Me.Top})
            End If
        End If
        FlushMoves(moves)
    End Sub

    ' Y reflow (stack compact): this FORM is the driver; the rider and the
    ' shadow ride to the same new top inside the same atomic batch.
    Private Sub ApplyUnitY(newTop As Integer)
        Dim moves As New List(Of PendingMove)()
        If Me.Top <> newTop Then
            moves.Add(New PendingMove With {.Hwnd = Me.Handle, .X = Me.Left, .Y = newTop})
        End If
        If Notifier_Sub2 IsNot Nothing AndAlso Not Notifier_Sub2.IsDisposed AndAlso Notifier_Sub2.Visible AndAlso Notifier_Sub2.Top <> newTop Then
            moves.Add(New PendingMove With {.Hwnd = Notifier_Sub2.Handle, .X = Notifier_Sub2.Left, .Y = newTop})
        End If
        If Shadow2 IsNot Nothing AndAlso Not Shadow2.IsDisposed AndAlso Shadow2.Visible AndAlso
           (Shadow2.Top <> newTop OrElse Shadow2.Left <> Me.Left) Then
            moves.Add(New PendingMove With {.Hwnd = Shadow2.Handle, .X = Me.Left, .Y = newTop})
        End If
        FlushMoves(moves)
    End Sub

    ' T30.1 OWNER rule: the slot's opening slide finishes ("done") -> the
    ' shadow fades in. Positioned under THIS unit's card (stack-aware Y),
    ' shown without activation, z-order fixed to Shadow < BG < Rider.
    ' T30.7: the shadow is HIDDEN (never Closed) while the unit lives, so
    ' this works after every dance - the old IsDisposed early-out silently
    ' killed it forever after the first dance.
    Public Sub FadeInShadow()
        Try
            If Shadow2 Is Nothing OrElse Shadow2.IsDisposed OrElse Shadow2.Disposing Then Return
            ' Already steady at full opacity and no fade running -> nothing to do.
            If Shadow2.Visible AndAlso Shadow2.Opacity >= 1R AndAlso _shadowFade Is Nothing Then Return
            If _shadowFade IsNot Nothing Then
                _shadowFade.Stop()
                _shadowFade.Dispose()
                _shadowFade = Nothing
            End If
            Shadow2.Opacity = 0R
            ' T30.7: position BEFORE Show - the first frame that can ever be
            ' seen is already on the card, never at the Load-time default spot.
            Shadow2.Location = New Point(Me.Left, Me.Top)
            Shadow2.Show()
            ' T30.7: atomic z-sort - the shadow must sit UNDER the card
            ' before its first visible fade frame (see RaiseUnit).
            RaiseUnit()
            _shadowFade = New System.Windows.Forms.Timer() With {.Interval = 15}
            AddHandler _shadowFade.Tick, Sub()
                                             If Shadow2.IsDisposed OrElse Shadow2.Disposing Then
                                                 _shadowFade.Stop()
                                                 Return
                                             End If
                                             If Shadow2.Opacity >= 1 Then
                                                 Shadow2.Opacity = 1R
                                                 _shadowFade.Stop()
                                                 _shadowFade.Dispose()
                                                 _shadowFade = Nothing
                                                 RaiseUnit()
                                             Else
                                                 Shadow2.Opacity = Math.Min(1R, Shadow2.Opacity + 0.1R)
                                             End If
                                         End Sub
            _shadowFade.Start()
        Catch ex As Exception
            Debug.WriteLine("[Notifier2] FadeInShadow error: " & ex.Message)
        End Try
    End Sub

    ' T30.1 OWNER rule: the moment a closing slide starts - in every case -
    ' the shadow goes. No fade-out, no sync, just gone.
    ' T30.7: HIDE, never Close(). Close() disposes the instance and the
    ' next FadeInShadow() silently died on IsDisposed - the shadow only
    ' came back after the whole unit was reborn (the "Shadow แว้บๆ" blink).
    Public Sub CloseShadowNow()
        Try
            If _shadowFade IsNot Nothing Then
                _shadowFade.Stop()
                _shadowFade.Dispose()
                _shadowFade = Nothing
            End If
            If Shadow2 IsNot Nothing AndAlso Not Shadow2.IsDisposed AndAlso Not Shadow2.Disposing Then
                Shadow2.Opacity = 0R
                Shadow2.Hide()
            End If
        Catch ex As Exception
            Debug.WriteLine("[Notifier2] CloseShadowNow error: " & ex.Message)
        End Try
    End Sub

    ' T30.6: re-entry guard. CompactStack runs every 100 ms and used to
    ' RESTART the in-flight glide (StartSlideY replaces animations), which
    ' reset the ease-out velocity to full on every heartbeat - the glide
    ' pulsed and crawled instead of easing into place. Same-target reflows
    ' are ignored while one is already in flight; BeginDance/SlideOutAll/
    ' IF_N clear the memory when they take the Y axis.
    Private _activeReflowTarget As Integer = Integer.MinValue

    ''' <summary>
    ''' T30/T30.1: glide the unit to a new Y so the stack can compact when a
    ''' toast above closes. Single clock: this form animates; the rider and
    ''' the shadow are carried along by the same 1 ms engine tick. Safe on
    ''' any state - per-control animation replace means the last call wins.
    ''' </summary>
    Public Sub ReflowTo(targetY As Integer, Optional durationMs As Integer = 250)
        If Me.IsDisposed OrElse Me.Disposing Then Return
        If Me.Top = targetY Then Return
        If _activeReflowTarget = targetY AndAlso _activeAnims.ContainsKey(Me) Then Return

        ' T30.1: animate THIS form only - the engine tick carries the rider
        ' and the shadow along in the same tick (single clock, no drift).
        _activeReflowTarget = targetY
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
    ''' without activation.
    ''' T30.7: ONE atomic commit. Three separate SetWindowPos calls each
    ''' move a window to the top of the topmost band and DWM can composite
    ''' BETWEEN them - one frame with the shadow above the card = the
    ''' "Shadow แว้บๆ" blink. BeginDeferWindowPos applies the whole chain
    ''' (Shadow &lt; BG &lt; Rider) in a single compositor update.</summary>
    Public Sub RaiseUnit()
        Try
            Dim hasShadow As Boolean = Shadow2 IsNot Nothing AndAlso Not Shadow2.IsDisposed AndAlso
                                       Shadow2.Visible AndAlso Shadow2.IsHandleCreated
            Dim hasRider As Boolean = Notifier_Sub2 IsNot Nothing AndAlso Not Notifier_Sub2.IsDisposed AndAlso
                                      Notifier_Sub2.Visible AndAlso Notifier_Sub2.IsHandleCreated
            Dim count As Integer = 1
            If hasShadow Then count += 1
            If hasRider Then count += 1

            Dim h As IntPtr = BeginDeferWindowPos(count)
            If h <> IntPtr.Zero Then
                ' Chain bottom -> top (OWNER spec 1.Shadow 2.BG 3.Text+ico):
                ' rider first (top of band), BG below it, shadow below BG.
                Dim prev As IntPtr = HwndTopmost
                If hasRider Then
                    h = DeferWindowPos(h, Notifier_Sub2.Handle, prev, 0, 0, 0, 0,
                                       SWP_NOSIZE_FLAG Or SWP_NOMOVE_FLAG Or SWP_NOACTIVATE_FLAG)
                    If h <> IntPtr.Zero Then prev = Notifier_Sub2.Handle
                End If
                If h <> IntPtr.Zero Then
                    h = DeferWindowPos(h, Me.Handle, prev, 0, 0, 0, 0,
                                       SWP_NOSIZE_FLAG Or SWP_NOMOVE_FLAG Or SWP_NOACTIVATE_FLAG)
                    If h <> IntPtr.Zero Then prev = Me.Handle
                End If
                If h <> IntPtr.Zero AndAlso hasShadow Then
                    h = DeferWindowPos(h, Shadow2.Handle, prev, 0, 0, 0, 0,
                                       SWP_NOSIZE_FLAG Or SWP_NOMOVE_FLAG Or SWP_NOACTIVATE_FLAG)
                End If
                If h <> IntPtr.Zero AndAlso EndDeferWindowPos(h) Then Return
            End If
        Catch ex As Exception
            Debug.WriteLine("[Notifier2] RaiseUnit defer error: " & ex.Message)
        End Try
        ' Fallback: plain per-window raises - identical final order.
        Try
            If Shadow2 IsNot Nothing AndAlso Not Shadow2.IsDisposed AndAlso Shadow2.IsHandleCreated Then
                RaiseWindow(Shadow2.Handle)
            End If
            RaiseWindow(Me.Handle)
            If Notifier_Sub2 IsNot Nothing AndAlso Not Notifier_Sub2.IsDisposed AndAlso Notifier_Sub2.IsHandleCreated Then
                RaiseWindow(Notifier_Sub2.Handle)
            End If
        Catch ex As Exception
            Debug.WriteLine("[Notifier2] RaiseUnit error: " & ex.Message)
        End Try
    End Sub

    Private Sub RaiseWindow(h As IntPtr)
        SetWindowPos(h, HwndTopmost, 0, 0, 0, 0,
                     SWP_NOSIZE_FLAG Or SWP_NOMOVE_FLAG Or SWP_NOACTIVATE_FLAG)
    End Sub

#End Region

#Region "Slide Out"

    Private Sub SlideOutAll()
        Debug.WriteLine("[Notifier2] SlideOutAll")
        _inTransition = True
        _isDancing = False ' an exit always wins - routing must queue, not coalesce
        _activeReflowTarget = Integer.MinValue ' T30.6: the exit owns the Y axis now

        ' T30.1/T30.3 OWNER rules: the closing slide STARTS first - the
        ' shadow goes this instant, in every case. The rider (Text+ICO)
        ' is still too: it never rides out, it goes the same instant and
        ' the card slides out alone.
        ' T30.7: Hide, never Close - the unit is still alive during the
        ' exit slide; disposal happens when Me.Close() lands.
        CloseShadowNow()
        Notifier_Sub2.Hide()
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
                                                                          Notifier_Sub2.Close()
                                                                          Me.Close()
                                                                      End Sub
                                         _closeTimer.Start()
                                     End Sub
        _delayTimer.Start()
    End Sub

#End Region

#Region "Click Events"

    Public Sub DoCloseClick()
        Debug.WriteLine("[Notifier2] DoCloseClick → Restart")
        Application.Restart()
    End Sub

    Private Sub Notifier_green_Click(sender As Object, e As EventArgs) Handles Notifier_green.Click, text_n.Click, icon_n.Click, Notifier_black.Click, Notifier_green_stop.Click
        Debug.WriteLine("[Notifier2] Click → Restart")
        Application.Restart()
    End Sub

    Private Sub IF_N_Tick(sender As Object, e As EventArgs) Handles IF_N.Tick
        If Me.IsDisposed Then
            IF_N.Stop()
            Return
        End If

        Debug.WriteLine("[Notifier2] IF_N tick → StartSlideY")
        _activeReflowTarget = Integer.MinValue ' T30.6: IF_N owns the Y axis now
        StartSlideY(Me, Me.Top, 105 + SlotOffsetY, 200)
        Dim screenWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
        If Not My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            IF_N.Stop()
            Debug.WriteLine("[Notifier2] IF_N stopped")
        End If
    End Sub

#End Region

End Class
