Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.IO
Imports System.Runtime.InteropServices

' ====================================================================
' T28: ToastUnit — ONE window per toast (replaces the 3-window stack)
'
' The old toast was 3 stacked topmost windows (card / ICO+Text overlay /
' shadow) driven by VB default instances and 3 independent clocks. That
' architecture is a bug factory: riders desync (OWNER: "ICO + Text หาย
' ไปไหนไม่รู้"), overlays vanish, default-instance ghosts wedge the
' router (OWNER: "toast จบแล้ว ค้างเลย"), the shadow lagged or never
' showed at all.
'
' T28 renders EVERYTHING — soft shadow, green accent edge, black card,
' icon glyph, text, logo — into ONE per-pixel-alpha layered window
' (UpdateLayeredWindow). One window moving = nothing to desync. Every
' toast is a fresh explicit instance (no default-instance ghosts).
' The 2-slot router (toggle update / queue / slide-up reflow) stays.
'
' Visual spec (matches the old stack):
'   card 300x90, black, 5px NVIDIA-green left edge, rounded corners,
'   icon glyph "nvgcshare" 35pt centered in the left 93px zone,
'   text "Segoe UI Semibold" 11pt Bold white middle-left,
'   optional 48x48 logo (zoom-fit) over the icon zone,
'   soft shadow bleeding 8px around the card.
' ====================================================================
Public Class ToastUnit
    Inherits Form

    ' ----- geometry (constants — never Me.Height, the window carries shadow bleed) -----
    Public Const CardW As Integer = 300
    Public Const CardH As Integer = 90
    Public Const SlotGapPx As Integer = 10
    Private Const Bleed As Integer = 8          ' shadow margin around the card
    Private Const CornerR As Integer = 10       ' card corner radius

    Public ReadOnly UnitId As Integer
    Public CurrentRow As Integer = 0

    ''' <summary>True between entrance completion and slide-out start —
    ''' the router's toggle-update restarts the 6s window only in this phase.</summary>
    Public ReadOnly Property IsShowing As Boolean
        Get
            Return _isShowing
        End Get
    End Property
    Private _isShowing As Boolean = False
    Private _isClosing As Boolean = False

    ' ----- slot lifecycle signals (instance events — the router subscribes per unit) -----
    Public Event DanceCompleted(u As ToastUnit)
    Public Event SlideOutStarted(u As ToastUnit)
    Public Event UnitClosed(u As ToastUnit)

    ' ----- shared content resources -----
    Private Shared _logo As Image          ' lazy — from embedded resource
    Private Shared ReadOnly SyncLockObj As New Object()

    Private Shared Function GetLogo() As Image
        If _logo IsNot Nothing Then Return _logo
        SyncLock SyncLockObj
            If _logo IsNot Nothing Then Return _logo
            Try
                Dim asm As Reflection.Assembly = Reflection.Assembly.GetExecutingAssembly()
                Using src As Stream = asm.GetManifestResourceStream("Notifier.logo.png")
                    If src IsNot Nothing Then
                        Dim ms As New MemoryStream()
                        src.CopyTo(ms)
                        ms.Position = 0
                        _logo = Image.FromStream(ms)   ' ms kept alive by the Image
                    End If
                End Using
            Catch
            End Try
        End SyncLock
        Return _logo
    End Function

    ' ----- content state (re-rendered on toggle update) -----
    Private _message As String = ""
    Private _showImage As Boolean = False
    Private _icon As String = ""
    Private _iconColor As Color = Color.White

    Private _bitmap As Bitmap              ' current layered surface

    ' ===== geometry helpers =====

    Public Shared Function BaseRowY() As Integer
        If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then
            Return 205
        End If
        Return 105
    End Function

    Private ReadOnly Property CardRowOffsetY As Integer
        Get
            Return CurrentRow * (CardH + SlotGapPx)
        End Get
    End Property

    Private ReadOnly Property CardTargetY As Integer
        Get
            Return BaseRowY() + CardRowOffsetY
        End Get
    End Property

    Private ReadOnly Property ScreenRight As Integer
        Get
            Return Screen.PrimaryScreen.WorkingArea.Width
        End Get
    End Property

    ' ===== creation =====

    Public Sub New(id As Integer)
        UnitId = id
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        TopMost = True
        AutoScaleMode = AutoScaleMode.None
        DoubleBuffered = True
        BackColor = Color.Black   ' pre-layered frames stay dark, never white-flash
        ClientSize = New Size(CardW + 2 * Bleed, CardH + 2 * Bleed)
        Text = "Toast" & id

        autoClose.Interval = 6000
        AddHandler autoClose.Tick, Sub()
                                       autoClose.Stop()
                                       SlideOut()
                                   End Sub
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_LAYERED Or WS_EX_TOOLWINDOW
            cp.ExStyle = cp.ExStyle And Not WS_EX_APPWINDOW
            Return cp
        End Get
    End Property

    ' ===== show / update / close =====

    ''' <summary>Entrance: position off-screen right at this unit's row,
    ''' render content, show, slide the whole window in. One window moves —
    ''' nothing can desync.</summary>
    Public Sub ShowToast(row As Integer, message As String, showImage As Boolean, icon As String, iconColor As Color)
        CurrentRow = row
        _message = message
        _showImage = showImage
        _icon = icon
        _iconColor = iconColor
        _isShowing = False
        _isClosing = False

        RenderContent()
        Location = New Point(ScreenRight, CardTargetY - Bleed)
        Debug.WriteLine($"[Toast{UnitId}] show row={row} Y={CardTargetY}")

        Show()
        ApplyBitmap()
        AnimateTo(ScreenRight - (CardW + 2 * Bleed), Nothing, 350,
                  Sub()
                      _isShowing = True
                      TopMost = True
                      autoClose.Start()
                      RaiseEvent DanceCompleted(Me)
                      Debug.WriteLine($"[Toast{UnitId}] entrance complete")
                  End Sub)
    End Sub

    ''' <summary>Toggle update: re-render the surface in place. If the toast
    ''' is in its showing phase the 6s window restarts (during the entrance
    ''' dance the entrance owns the timer — same rule as T27).</summary>
    Public Sub UpdateContent(message As String, showImage As Boolean, icon As String, iconColor As Color)
        _message = message
        _showImage = showImage
        _icon = icon
        _iconColor = iconColor
        RenderContent()
        If IsHandleCreated Then ApplyBitmap()
        If _isShowing Then
            autoClose.Stop()
            autoClose.Start()
        End If
        Debug.WriteLine($"[Toast{UnitId}] content updated (showing={_isShowing})")
    End Sub

    Public Sub SlideOut()
        If _isClosing Then Return   ' click racing auto-close must not re-enter
        _isClosing = True
        _isShowing = False
        autoClose.Stop()
        StopMainWatch()
        Debug.WriteLine($"[Toast{UnitId}] slide out")
        RaiseEvent SlideOutStarted(Me)
        AnimateTo(ScreenRight + Bleed, Nothing, 500, Sub() Close())
    End Sub

    ''' <summary>Reflow glide: the router moved this unit to row 0 — glide up.</summary>
    Public Sub GlideToRow(row As Integer)
        CurrentRow = row
        AnimateTo(Nothing, CardTargetY - Bleed, 420, Nothing)
        Debug.WriteLine($"[Toast{UnitId}] glide to row {row} (Y={CardTargetY})")
    End Sub

    ' ===== legacy "main off" recovery (was IF_N + Timer1) =====

    Private _mainWatch As Timer

    Public Sub StartMainOffRecovery()
        If _mainWatch Is Nothing Then
            _mainWatch = New Timer With {.Interval = 100}
            AddHandler _mainWatch.Tick,
                Sub()
                    If Me.IsDisposed OrElse Me.Disposing Then
                        StopMainWatch()
                        Return
                    End If
                    If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then Return
                    StopMainWatch()
                    AnimateTo(Nothing, CardTargetY - Bleed, 200, Nothing)
                    Debug.WriteLine($"[Toast{UnitId}] main-off recovery glide")
                End Sub
        End If
        _mainWatch.Start()
    End Sub

    Private Sub StopMainWatch()
        If _mainWatch IsNot Nothing Then
            _mainWatch.Stop()
            _mainWatch.Dispose()
            _mainWatch = Nothing
        End If
    End Sub

    ' ===== input =====

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Debug.WriteLine($"[Toast{UnitId}] click → dismiss")
        SlideOut()
    End Sub

    ' ===== teardown =====

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        AnimationEngineCancelAll()
        autoClose.Stop()
        StopMainWatch()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        If _bitmap IsNot Nothing Then
            _bitmap.Dispose()
            _bitmap = Nothing
        End If
        RaiseEvent UnitClosed(Me)
    End Sub

#Region "Timers"

    Private ReadOnly autoClose As New Timer()

#End Region

#Region "Rendering — shadow + card + accent + glyph + text in one bitmap"

    Private Shared Function RoundedPath(x As Integer, y As Integer, w As Integer, h As Integer, r As Integer) As GraphicsPath
        Dim p As New GraphicsPath()
        Dim d As Integer = 2 * r
        p.AddArc(x, y, d, d, 180, 90)
        p.AddArc(x + w - d, y, d, d, 270, 90)
        p.AddArc(x + w - d, y + h - d, d, d, 0, 90)
        p.AddArc(x, y + h - d, d, d, 90, 90)
        p.CloseFigure()
        Return p
    End Function

    ''' <summary>Path with rounded RIGHT corners only (square left) — the
    ''' black card body: its left edge is a straight cut over the 5px green
    ''' accent, right edge follows the card's rounded silhouette.</summary>
    Private Shared Function RoundedRightPath(x As Integer, y As Integer, w As Integer, h As Integer, r As Integer) As GraphicsPath
        Dim p As New GraphicsPath()
        Dim d As Integer = 2 * r
        p.AddLine(x, y, x + w - d, y)
        p.AddArc(x + w - d, y, d, d, 270, 90)
        p.AddLine(x + w, y + r, x + w, y + h - r)
        p.AddArc(x + w - d, y + h - d, d, d, 0, 90)
        p.AddLine(x + w - d, y + h, x, y + h)
        p.CloseFigure()
        Return p
    End Function

    Private Sub RenderContent()
        Dim w As Integer = CardW + 2 * Bleed
        Dim h As Integer = CardH + 2 * Bleed
        Dim cx As Integer = Bleed          ' card origin inside the window
        Dim cy As Integer = Bleed

        Dim old As Bitmap = _bitmap
        Dim bmp As New Bitmap(w, h, Imaging.PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

            ' 1) soft shadow — concentric rounded rects, alpha fades outward
            For i As Integer = 6 To 1 Step -1
                Using shadowPath As GraphicsPath = RoundedPath(cx - i * 2, cy - i * 2 + 1, CardW + i * 4, CardH + i * 4, CornerR + i * 2)
                    Using b As New SolidBrush(Color.FromArgb(15 - i * 2, 0, 0, 0))
                        g.FillPath(b, shadowPath)
                    End Using
                End Using
            Next

            ' 2) green base card (its rounded left edge shows as the 5px accent)
            Using greenPath As GraphicsPath = RoundedPath(cx, cy, CardW, CardH, CornerR)
                Using b As New SolidBrush(Color.FromArgb(118, 185, 0))
                    g.FillPath(b, greenPath)
                End Using
            End Using

            ' 3) black card body from x+5 — square left edge over the green,
            '    rounded right corners
            Using blackPath As GraphicsPath = RoundedRightPath(cx + 5, cy, CardW - 5, CardH, CornerR)
                Using b As New SolidBrush(Color.Black)
                    g.FillPath(b, blackPath)
                End Using
            End Using

            ' 4) icon glyph — nvgcshare 35pt, centered in the left 93px zone
            If Not String.IsNullOrEmpty(_icon) Then
                Try
                    Using f As New Font("nvgcshare", 35.0F, FontStyle.Regular, GraphicsUnit.Point)
                        Using sf As New StringFormat With {
                            .Alignment = StringAlignment.Center,
                            .LineAlignment = StringAlignment.Center
                        }
                            Using b As New SolidBrush(_iconColor)
                                g.DrawString(_icon, f, b, New RectangleF(cx, cy, 93, CardH), sf)
                            End Using
                        End Using
                    End Using
                Catch
                End Try
            End If

            ' 5) optional logo — zoom-fit 48x48 into the old PictureBox rect
            If _showImage Then
                Dim logo As Image = GetLogo()
                If logo IsNot Nothing Then
                    Dim box As New Rectangle(cx + 21, cy + 13, 48, 64)
                    Dim scale As Double = Math.Min(box.Width / logo.Width, box.Height / logo.Height)
                    Dim dw As Integer = CInt(logo.Width * scale)
                    Dim dh As Integer = CInt(logo.Height * scale)
                    Dim dx As Integer = box.X + (box.Width - dw) \ 2
                    Dim dy As Integer = box.Y + (box.Height - dh) \ 2
                    g.DrawImage(logo, dx, dy, dw, dh)
                End If
            End If

            ' 6) text — Segoe UI Semibold 11pt Bold, white, middle-left
            Try
                Using f As New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold, GraphicsUnit.Point)
                    Using sf As New StringFormat With {
                        .Alignment = StringAlignment.Near,
                        .LineAlignment = StringAlignment.Center,
                        .FormatFlags = StringFormatFlags.NoWrap,
                        .Trimming = StringTrimming.EllipsisCharacter
                    }
                        Using b As New SolidBrush(Color.White)
                            g.DrawString(_message, f, b, New RectangleF(cx + 96, cy, CardW - 106, CardH), sf)
                        End Using
                    End Using
                End Using
            Catch
            End Try
        End Using

        _bitmap = bmp
        If old IsNot Nothing Then old.Dispose()
    End Sub

#End Region

#Region "UpdateLayeredWindow — per-pixel alpha surface"

    Private Const WS_EX_LAYERED As Integer = &H80000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000
    Private Const ULW_ALPHA As Integer = 2

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NLSIZE
        Public cx As Integer
        Public cy As Integer
        Public Sub New(cx_ As Integer, cy_ As Integer)
            cx = cx_
            cy = cy_
        End Sub
    End Structure

    <DllImport("user32.dll")>
    Private Shared Function UpdateLayeredWindow(hwnd As IntPtr, hdcDst As IntPtr,
                                                pptDst As IntPtr, ByRef psize As NLSIZE,
                                                hdcSrc As IntPtr, pptSrc As IntPtr,
                                                crKey As Integer, pblend As IntPtr,
                                                dwFlags As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetDC(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ReleaseDC(hWnd As IntPtr, hdc As IntPtr) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function CreateCompatibleDC(hdc As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteDC(hdc As IntPtr) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function SelectObject(hdc As IntPtr, obj As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(obj As IntPtr) As Integer
    End Function

    ''' <summary>Push the rendered bitmap into the window. Called once per
    ''' content change — position moves are plain SetWindowPos, the layered
    ''' surface survives moves untouched.</summary>
    Private Sub ApplyBitmap()
        If _bitmap Is Nothing OrElse Not IsHandleCreated Then Return
        Dim screenDc As IntPtr = GetDC(IntPtr.Zero)
        Dim memDc As IntPtr = CreateCompatibleDC(screenDc)
        Dim hBitmap As IntPtr = IntPtr.Zero
        Dim old As IntPtr = IntPtr.Zero
        Try
            hBitmap = _bitmap.GetHbitmap(Color.FromArgb(0))
            old = SelectObject(memDc, hBitmap)
            Dim size As New NLSIZE(_bitmap.Width, _bitmap.Height)
            UpdateLayeredWindow(Handle, screenDc, IntPtr.Zero, size, memDc, IntPtr.Zero, 0, IntPtr.Zero, ULW_ALPHA)
        Catch ex As Exception
            Debug.WriteLine($"[Toast{UnitId}] ULW error: {ex.Message}")
        Finally
            If old <> IntPtr.Zero Then SelectObject(memDc, old)
            If hBitmap <> IntPtr.Zero Then DeleteObject(hBitmap)
            DeleteDC(memDc)
            ReleaseDC(IntPtr.Zero, screenDc)
        End Try
    End Sub

#End Region

#Region "Animation Engine — MM timer, one window, nothing to desync"

    Private Class AnimState
        Public Sw As Stopwatch
        Public Duration As Double
        Public FromX As Integer
        Public ToX As Integer
        Public FromY As Integer
        Public ToY As Integer
        Public AnimateX As Boolean
        Public OnComplete As Action
    End Class

    Private _anim As AnimState

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
    Private _invokePending As Boolean = False

    Private Sub AnimationEngineStart()
        If mmTimerId <> 0 Then Return
        timeBeginPeriod(1)
        mmCallback = New MMTimerProc(AddressOf OnMMTick)
        mmTimerId = timeSetEvent(1, 1, mmCallback, UIntPtr.Zero,
                                  TIME_PERIODIC Or TIME_KILL_SYNCHRONOUS)
    End Sub

    Private Sub AnimationEngineStop()
        If mmTimerId <> 0 Then
            timeKillEvent(mmTimerId)
            mmTimerId = 0
        End If
        timeEndPeriod(1)
        _invokePending = False
    End Sub

    Private Sub AnimationEngineCancelAll()
        _anim = Nothing
        AnimationEngineStop()
    End Sub

    Private Sub OnMMTick(uID As UInteger, uMsg As UInteger,
                          dwUser As UIntPtr, dw1 As UInteger, dw2 As UInteger)
        If Me.IsDisposed OrElse Me.Disposing Then Return

        If _invokePending Then Return
        _invokePending = True

        Try
            Me.BeginInvoke(Sub()
                               _invokePending = False
                               AnimationEngineTick()
                           End Sub)
        Catch ex As ObjectDisposedException
            _invokePending = False
        Catch ex As InvalidOperationException
            _invokePending = False   ' handle destroyed mid-close — not fatal
        End Try
    End Sub

    ''' <summary>Animate this ONE window on X and/or Y. Replaces any in-flight
    ''' animation (the entrance dance → exit race resolves naturally).</summary>
    Private Sub AnimateTo(toX As Integer?, toY As Integer?, duration As Double, onComplete As Action)
        Dim st As New AnimState With {
            .Sw = Stopwatch.StartNew(),
            .Duration = duration,
            .AnimateX = toX.HasValue,
            .FromX = Me.Left,
            .ToX = If(toX.HasValue, toX.Value, Me.Left),
            .FromY = Me.Top,
            .ToY = If(toY.HasValue, toY.Value, Me.Top),
            .OnComplete = onComplete
        }
        _anim = st
        AnimationEngineStart()
    End Sub

    Private Sub AnimationEngineTick()
        If _anim Is Nothing Then
            AnimationEngineStop()
            Return
        End If

        Dim t As Double = _anim.Sw.Elapsed.TotalMilliseconds / _anim.Duration
        If t >= 1 Then
            If _anim.AnimateX Then Me.Left = _anim.ToX Else Me.Top = _anim.ToY
            Dim cb As Action = _anim.OnComplete
            _anim = Nothing
            AnimationEngineStop()
            Debug.WriteLine($"[Toast{UnitId}] animation complete")
            If cb IsNot Nothing Then
                Try
                    cb.Invoke()
                Catch ex As Exception
                    Debug.WriteLine($"[Toast{UnitId}] onComplete ERROR: {ex.Message}")
                End Try
            End If
            Return
        End If

        Dim eased As Double = 1 - Math.Pow(1 - t, 3)
        If _anim.AnimateX Then
            Me.Left = CInt(_anim.FromX + (_anim.ToX - _anim.FromX) * eased)
        Else
            Me.Top = CInt(_anim.FromY + (_anim.ToY - _anim.FromY) * eased)
        End If
    End Sub

#End Region

End Class
