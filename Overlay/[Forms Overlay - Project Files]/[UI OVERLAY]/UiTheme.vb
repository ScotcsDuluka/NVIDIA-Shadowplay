' =====================================================================
' UiTheme — central skin + motion engine for the Overlay UI.
' Style: "glass dark" refresh — cooler layered darks, NVIDIA green DNA,
'        rounded cards, animated hover/press, soft form fade-ins.
'
' Design rules:
'  - Runtime sweep: designer files stay untouched (VS designer keeps working).
'  - Old palette colors are REMAPPED live (keyed by RGB) to the new stack.
'  - TransparencyKey colors (Red / Blue / Magenta) are never touched —
'    they are color-key transparency keys, not visible surfaces.
'  - Hero green #76B900 stays; hover tones are new.
'  - Motion: single 15 ms engine timer, ease-out cubic, always tweened
'    from the CURRENT color so reversals never jump.
' =====================================================================

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Public Module UiTheme

#Region "PALETTE"

    ' Accent (hero NVIDIA green + new hover tone)
     Public ReadOnly Accent As Color = Color.FromArgb(118, 185, 0)      ' #76B900
    Public ReadOnly AccentHover As Color = Color.FromArgb(146, 220, 20)

    ' Glass dark surface stack (cool graphite, deep → top)
    Public ReadOnly SurfaceDeepest As Color = Color.FromArgb(18, 22, 26)
    Public ReadOnly SurfaceDeep As Color = Color.FromArgb(25, 30, 35)
    Public ReadOnly SurfaceBase As Color = Color.FromArgb(31, 38, 45)
    Public ReadOnly SurfaceRaised As Color = Color.FromArgb(41, 49, 58)
    Public ReadOnly SurfaceHover As Color = Color.FromArgb(54, 64, 75)
    Public ReadOnly SurfaceTop As Color = Color.FromArgb(64, 75, 87)

    ' Text
    Public ReadOnly TextBright As Color = Color.FromArgb(233, 239, 244)
    Public ReadOnly TextMuted As Color = Color.FromArgb(146, 161, 173)

#End Region

#Region "REMAP (old palette → new palette)"

    Private ReadOnly _remap As New Dictionary(Of Integer, Color)
    Private _mapReady As Boolean = False

    Private Function RgbKey(r As Integer, g As Integer, b As Integer) As Integer
        Return (r << 16) Or (g << 8) Or b
    End Function

    Private Function RgbKey(c As Color) As Integer
        Return (c.R << 16) Or (c.G << 8) Or c.B
    End Function

    Private Sub R(r As Integer, g As Integer, b As Integer, target As Color)
        _remap(RgbKey(r, g, b)) = target
    End Sub

    Private Sub EnsureMap()
        If _mapReady Then Return
        ' Old dark stack (values gathered from every Designer file)
        R(33, 35, 38, SurfaceDeep)        ' old deepest panels
        R(30, 34, 38, SurfaceDeepest)
        R(37, 40, 44, SurfaceDeep)
        R(28, 32, 36, SurfaceDeepest)
        R(30, 33, 36, SurfaceDeepest)
        R(38, 43, 47, SurfaceBase)        ' old main surface
        R(46, 52, 57, SurfaceRaised)
        R(46, 53, 59, SurfaceRaised)
        R(52, 58, 64, SurfaceTop)
        R(55, 60, 65, SurfaceTop)
        R(56, 56, 56, SurfaceHover)
        R(60, 63, 67, SurfaceTop)
        R(64, 64, 64, SurfaceHover)       ' old hover gray
        R(53, 55, 58, SurfaceHover)       ' old group-hover gray
        ' Old text tones
        R(150, 160, 165, TextMuted)
        R(160, 165, 170, TextMuted)
        R(208, 214, 220, TextBright)
        ' Old hover green (UiTheme.AccentHover = 0,128,0)
        R(0, 128, 0, AccentHover)
        _mapReady = True
    End Sub

    Private Function MapColor(c As Color) As Color
        If c.A < 255 Then Return c           ' transparent / keyed: never touch
        Dim out As Color = Nothing
        If _remap.TryGetValue(RgbKey(c), out) Then Return out
        Return c
    End Function

    Private Function IsSurfaceColor(c As Color) As Boolean
        If c.A < 255 Then Return False
        If _remap.ContainsKey(RgbKey(c)) Then Return True
        ' New palette values too (idempotent second pass / dynamic children)
        Return RgbKey(c) = RgbKey(SurfaceDeepest) OrElse
               RgbKey(c) = RgbKey(SurfaceDeep) OrElse
               RgbKey(c) = RgbKey(SurfaceBase) OrElse
               RgbKey(c) = RgbKey(SurfaceRaised) OrElse
               RgbKey(c) = RgbKey(SurfaceHover) OrElse
               RgbKey(c) = RgbKey(SurfaceTop) OrElse
               RgbKey(c) = RgbKey(Accent)
    End Function

    Private Function IsAccent(c As Color) As Boolean
        Return c.A = 255 AndAlso RgbKey(c) = RgbKey(Accent)
    End Function

#End Region

#Region "COLOR MATH"

    Public Function Lerp(a As Color, b As Color, t As Double) As Color
        t = Math.Max(0.0, Math.Min(1.0, t))
        Return Color.FromArgb(
            CInt(a.A + (b.A - a.A) * t),
            CInt(a.R + (b.R - a.R) * t),
            CInt(a.G + (b.G - a.G) * t),
            CInt(a.B + (b.B - a.B) * t))
    End Function

    Public Function Shade(c As Color, factor As Single) As Color
        Return Color.FromArgb(c.A,
                              Math.Max(0, CInt(c.R * factor)),
                              Math.Max(0, CInt(c.G * factor)),
                              Math.Max(0, CInt(c.B * factor)))
    End Function

#End Region

#Region "TWEEN ENGINE (BackColor)"

    Private Class Job
        Public Ctrl As Control
        Public From As Color
        Public Target As Color
        Public Ms As Integer
        Public T As Integer
    End Class

    Private ReadOnly _jobs As New List(Of Job)
    Private _engine As Timer

    Private Sub Enqueue(ctrl As Control, target As Color, ms As Integer)
        If ctrl Is Nothing OrElse ctrl.IsDisposed Then Return
        Dim startCol As Color = ctrl.BackColor
        For i As Integer = _jobs.Count - 1 To 0 Step -1
            If _jobs(i).Ctrl Is ctrl Then _jobs.RemoveAt(i)
        Next
        If startCol.ToArgb() = target.ToArgb() Then Return
        Dim j As New Job With {.Ctrl = ctrl, .From = startCol, .Target = target, .Ms = Math.Max(30, ms), .T = 0}
        _jobs.Add(j)
        EnsureEngine()
    End Sub

    Private Sub EnsureEngine()
        If _engine IsNot Nothing Then
            If Not _engine.Enabled Then _engine.Start()
            Return
        End If
        _engine = New Timer With {.Interval = 15}
        AddHandler _engine.Tick,
            Sub()
                If _jobs.Count = 0 Then
                    _engine.Stop()
                    Return
                End If
                Dim done As New List(Of Job)
                For Each j As Job In _jobs
                    j.T += _engine.Interval
                    Dim t As Double = j.T / CDbl(j.Ms)
                    Dim eased As Double = 1.0 - Math.Pow(1.0 - Math.Min(1.0, t), 3)
                    If j.Ctrl.IsDisposed OrElse j.Ctrl.Disposing Then
                        done.Add(j)
                        Continue For
                    End If
                    j.Ctrl.BackColor = Lerp(j.From, j.Target, eased)
                    If t >= 1.0 Then
                        j.Ctrl.BackColor = j.Target
                        done.Add(j)
                    End If
                Next
                For Each j As Job In done
                    _jobs.Remove(j)
                Next
                If _jobs.Count = 0 Then _engine.Stop()
            End Sub
        _engine.Start()
    End Sub

#End Region

#Region "HOVER / PRESS"

    Private Class HoverSpec
        Public Hover As Color
        Public Rest As Color
        Public Press As Color
    End Class

    ' Controls wired by the app code itself (SetHoverEffect / SetGroupHoverEffect)
    Private ReadOnly _hover As New Dictionary(Of Control, HoverSpec)
    Private ReadOnly _hoverWired As New HashSet(Of Control)

    Public Sub RegisterManualHover(ctrl As Control, hover As Color, rest As Color)
        If ctrl Is Nothing Then Return
        EnsureMap()
        Dim spec As New HoverSpec With {
            .Hover = hover, .Rest = rest, .Press = Shade(hover, 0.85F)}
        Dim fresh As Boolean = Not _hover.ContainsKey(ctrl)
        _hover(ctrl) = spec
        ctrl.BackColor = rest
        If fresh Then WireHoverHandlers(ctrl)
    End Sub

    Public Sub RegisterManualGroupHover(ctrls As Control(), hover As Color, rest As Color)
        If ctrls Is Nothing Then Return
        EnsureMap()
        For Each c As Control In ctrls
            If c Is Nothing Then Continue For
            _hover(c) = New HoverSpec With {.Hover = hover, .Rest = rest, .Press = Shade(hover, 0.85F)}
        Next
        For Each c As Control In ctrls
            If c Is Nothing Then Continue For
            Dim captured = ctrls
            Dim fresh As Boolean = Not _hoverWired.Contains(c)
            If fresh Then
                _hoverWired.Add(c)
                AddHandler c.MouseEnter,
                    Sub()
                        For Each g As Control In captured
                            If g IsNot Nothing Then Enqueue(g, hover, 130)
                        Next
                    End Sub
                AddHandler c.MouseLeave,
                    Sub()
                        Dim mp As Point = Cursor.Position
                        Dim still As Boolean = False
                        For Each g As Control In captured
                            If g IsNot Nothing AndAlso
                               g.RectangleToScreen(g.ClientRectangle).Contains(mp) Then
                                still = True
                                Exit For
                            End If
                        Next
                        If Not still Then
                            For Each g As Control In captured
                                If g IsNot Nothing Then Enqueue(g, rest, 180)
                            Next
                        End If
                    End Sub
            End If
        Next
    End Sub

    Private Sub WireHoverHandlers(ctrl As Control)
        If _hoverWired.Contains(ctrl) Then Return
        _hoverWired.Add(ctrl)
        AddHandler ctrl.MouseEnter,
            Sub()
                Dim spec As HoverSpec = Nothing
                If _hover.TryGetValue(ctrl, spec) Then Enqueue(ctrl, spec.Hover, 130)
            End Sub
        AddHandler ctrl.MouseLeave,
            Sub()
                Dim spec As HoverSpec = Nothing
                If _hover.TryGetValue(ctrl, spec) Then Enqueue(ctrl, spec.Rest, 180)
            End Sub
        AddHandler ctrl.MouseDown,
            Sub()
                Dim spec As HoverSpec = Nothing
                If _hover.TryGetValue(ctrl, spec) Then Enqueue(ctrl, spec.Press, 60)
            End Sub
        AddHandler ctrl.MouseUp,
            Sub()
                Dim spec As HoverSpec = Nothing
                If _hover.TryGetValue(ctrl, spec) Then
                    Dim over As Boolean = ctrl.ClientRectangle.Contains(ctrl.PointToClient(Cursor.Position))
                    Enqueue(ctrl, If(over, spec.Hover, spec.Rest), 90)
                End If
            End Sub
    End Sub

    Private Sub AttachAutoHover(ctrl As Control, rest As Color)
        If _hover.ContainsKey(ctrl) OrElse _hoverWired.Contains(ctrl) Then Return
        Dim hover As Color = If(IsAccent(rest), AccentHover, SurfaceHover)
        Dim spec As New HoverSpec With {.Hover = hover, .Rest = rest, .Press = Shade(rest, 0.86F)}
        _hover(ctrl) = spec
        WireHoverHandlers(ctrl)
    End Sub

#End Region

#Region "ROUNDED CARDS + GLASS BORDER"

    Private ReadOnly _glassPainted As New HashSet(Of Control)

    Private Function RadiusFor(w As Integer, h As Integer) As Integer
        Dim m As Integer = Math.Min(w, h)
        Dim r As Integer
        If m >= 220 Then
            r = 16
        ElseIf m >= 120 Then
            r = 14
        Else
            r = 10
        End If
        Return Math.Max(6, Math.Min(r, m \ 4))
    End Function

    Private Function RoundPath(rc As Rectangle, r As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = Math.Max(2, r * 2)
        If d > rc.Width Then d = rc.Width
        If d > rc.Height Then d = rc.Height
        path.AddArc(rc.Left, rc.Top, d, d, 180, 90)
        path.AddArc(rc.Right - d, rc.Top, d, d, 270, 90)
        path.AddArc(rc.Right - d, rc.Bottom - d, d, d, 0, 90)
        path.AddArc(rc.Left, rc.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Private Sub RoundCard(c As Control)
        Dim r As Integer = RadiusFor(c.ClientSize.Width, c.ClientSize.Height)
        ApplyRegion(c, r)
        ' Keep the region correct when the card animates/resizes (dropdowns etc.)
        AddHandler c.Resize,
            Sub()
                If c.IsDisposed Then Return
                ApplyRegion(c, RadiusFor(c.ClientSize.Width, c.ClientSize.Height))
            End Sub
        If Not _glassPainted.Contains(c) Then
            _glassPainted.Add(c)
            AddHandler c.Paint,
                Sub(s As Object, e As PaintEventArgs)
                    Try
                        Dim w As Integer = c.ClientSize.Width
                        Dim h As Integer = c.ClientSize.Height
                        If w < 12 OrElse h < 12 Then Return
                        Dim rr As Integer = RadiusFor(w, h)
                        Using path As GraphicsPath = RoundPath(New Rectangle(0, 0, w - 1, h - 1), rr)
                            ' soft glass edge
                            Using penEdge As New Pen(Color.FromArgb(28, 255, 255, 255))
                                e.Graphics.DrawPath(penEdge, path)
                            End Using
                            ' 1px top highlight — the "glass" read
                            Using penTop As New Pen(Color.FromArgb(46, 255, 255, 255))
                                e.Graphics.DrawLine(penTop, rr, 1, w - 1 - rr, 1)
                            End Using
                        End Using
                    Catch
                        ' never let decoration kill a form
                    End Try
                End Sub
            c.Invalidate()
        End If
    End Sub

    Private Sub ApplyRegion(c As Control, r As Integer)
        Dim path As GraphicsPath = RoundPath(New Rectangle(0, 0, c.ClientSize.Width, c.ClientSize.Height), r)
        Dim old As Region = c.Region
        c.Region = New Region(path)
        If old IsNot Nothing Then old.Dispose()
        path.Dispose()
    End Sub

    Private Function ShouldRound(c As Control) As Boolean
        If c.Width < 64 OrElse c.Height < 64 Then Return False
        If c.Parent Is Nothing Then Return False
        ' full-bleed backdrops stay square (they align with shadow windows)
        If c.Width >= CInt(c.Parent.ClientSize.Width * 0.95) AndAlso
           c.Height >= CInt(c.Parent.ClientSize.Height * 0.95) Then Return False
        Select Case c.Name
            Case "bg_action", "shadowplay", "ME_CLOSE_BG", "ME_CLOSE_BG_GRE"
                Return False
        End Select
        If TypeOf c Is PictureBox Then Return True
        If TypeOf c Is Panel OrElse TypeOf c Is Label Then
            Return IsSurfaceColor(c.BackColor) AndAlso c.BackgroundImage Is Nothing
        End If
        Return False
    End Function

#End Region

#Region "FORM FADE-IN"

    Private Sub AttachFade(f As Form, target As Double)
        If target <= 0 Then target = 1.0
        If f.Opacity > 0.001 Then f.Opacity = 0.0001
        Dim tmr As Timer = Nothing
        tmr = New Timer With {.Interval = 10}
        AddHandler tmr.Tick,
            Sub()
                If f.IsDisposed OrElse f.Disposing Then
                    tmr.Stop()
                    tmr.Dispose()
                    Return
                End If
                Dim op As Double = f.Opacity
                op += (target - op) * 0.24
                If Math.Abs(target - op) < 0.02 Then
                    f.Opacity = target
                    tmr.Stop()
                    tmr.Dispose()
                Else
                    f.Opacity = op
                End If
            End Sub
        AddHandler f.Shown,
            Sub()
                If f.IsDisposed Then Return
                f.Opacity = 0.0001
                tmr.Stop()
                tmr.Start()
            End Sub
    End Sub

#End Region

#Region "SKIN SWEEP"

    Private ReadOnly _skinned As New HashSet(Of Form)

    ''' <summary>Skin a whole form. Call once from the form's OnLoad.</summary>
    ''' <param name="fadeIn">True = soft fade on every Show (static pages).</param>
    Public Sub SkinForm(f As Form, Optional fadeIn As Boolean = True)
        If f Is Nothing OrElse f.IsDisposed Then Return
        If Not _skinned.Add(f) Then Return
        EnsureMap()
        SkinTree(f)
        ' controls created at runtime get skinned too
        AddHandler f.ControlAdded,
            Sub(s As Object, e As ControlEventArgs)
                Try
                    SkinTree(e.Control)
                Catch
                End Try
            End Sub
        If fadeIn Then AttachFade(f, If(f.Opacity >= 1.0, 1.0, f.Opacity))
    End Sub

    ''' <summary>Walk a control subtree: remap colors, round cards, wire hover.</summary>
    Public Sub SkinTree(root As Control)
        If root Is Nothing Then Return
        EnsureMap()
        Walk(root)
    End Sub

    Private Sub Walk(c As Control)
        Try
            c.BackColor = MapColor(c.BackColor)
            Dim fc As Color = MapColor(c.ForeColor)
            If fc.ToArgb() <> c.ForeColor.ToArgb() Then c.ForeColor = fc

            If ShouldRound(c) Then RoundCard(c)

            If TypeOf c Is Button Then
                AttachAutoHover(c, c.BackColor)
            ElseIf TypeOf c Is Label AndAlso c.Cursor = Cursors.Hand Then
                AttachAutoHover(c, c.BackColor)
            End If
        Catch
            ' single control failure must never break the form
        End Try
        For Each child As Control In c.Controls
            Walk(child)
        Next
    End Sub

#End Region

End Module
