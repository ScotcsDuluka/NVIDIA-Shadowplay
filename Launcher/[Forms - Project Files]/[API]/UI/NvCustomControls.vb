' ═══════════════════════════════════════════════════════════════════
'  NVIDIA ShadowPlay — ALL Custom UI Controls (VB.NET WinForms)
'  รวมทุก Controls ไฟล์เดียว — ขอบโค้ง + Hover Effect + Glow
'
'  Controls ทั้งหมด (12 ตัว):
'   1.  NvButton         — ปุ่มหลัก มี 7 variants, 3 sizes, glow + shimmer
'   2.  NvIconButton     — ปุ่มไอคอน สี่เหลี่ยม/กลม, hover ขยาย
'   3.  NvRecordButton   — ปุ่ม Record วงกลม→สี่เหลี่ยม, pulse animation
'   4.  NvTitleBarButton — ปุ่ม Min/Max/Close, Close hover แดง
'   5.  NvToggleButton   — สวิตช์เปิด/ปิด, gradient + glow, 3 ขนาด
'   6.  NvCheckbox       — เช็คบ็อกซ์, gradient + glow, hover ขยาย
'   7.  NvPillButton     — ปุ่มแคปซูล (tab/filter), active gradient
'   8.  NvActionCard     — การ์ดคลิกได้, hover ยกขึ้น, shortcut badge
'   9.  NvStatusDot      — จุดบ่งบอกสถานะ, glow + pulse
'  10. NvBlurPanel      — พาเนลเบลอๆ (Glassmorphism)
'  11. NvCircleButton   — ปุ่มกลม, glow + pulse ring
'  12. NvGlowLabel      — ข้อความมี glow
'
'  วิธีใช้: Copy ไฟล์นี้ไปวางในโปรเจกต์ แล้ว Build → จะปรากฏใน Toolbox
' ═══════════════════════════════════════════════════════════════════

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Runtime.InteropServices

' ╔═══════════════════════════════════════════════════════════════════╗
' ║  SHARED HELPERS — NvExtensions                                   ║
' ╚═══════════════════════════════════════════════════════════════════╝
Public Module NvExtensions

    ''' <summary>สร้าง GraphicsPath สี่เหลี่ยมขอบโค้ง</summary>
    Public Function RoundedRect(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim r As Integer = Math.Min(radius, Math.Min(rect.Width \ 2, rect.Height \ 2))
        path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90)
        path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90)
        path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90)
        path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    ''' <summary>ผสมสี 2 สี ด้วย progress (0.0 - 1.0) — ปลอดภัยจาก NaN/Infinity</summary>
    Public Function LerpColor(c1 As Color, c2 As Color, t As Single) As Color
        ' Guard: ป้องกัน CInt พังเมื่อ t เป็น NaN หรือ Infinity
        If Single.IsNaN(t) OrElse Single.IsInfinity(t) Then t = 0.0F
        Dim s As Single = CSng(t)
        Return Color.FromArgb(
            ClampByte(CInt(Math.Round(CSng(c1.A) + (CSng(c2.A) - CSng(c1.A)) * s))),
            ClampByte(CInt(Math.Round(CSng(c1.R) + (CSng(c2.R) - CSng(c1.R)) * s))),
            ClampByte(CInt(Math.Round(CSng(c1.G) + (CSng(c2.G) - CSng(c1.G)) * s))),
            ClampByte(CInt(Math.Round(CSng(c1.B) + (CSng(c2.B) - CSng(c1.B)) * s)))
        )
    End Function

    ''' <summary>จำกัดค่าให้อยู่ในช่วง 0-255 ป้องกัน overflow</summary>
    Private Function ClampByte(v As Integer) As Integer
        If v < 0 Then Return 0
        If v > 255 Then Return 255
        Return v
    End Function

    ''' <summary>CInt ปลอดภัย — ป้องกัน OverflowException เมื่อค่าเป็น NaN/Infinity</summary>
    Public Function SafeCInt(v As Single) As Integer
        If Single.IsNaN(v) OrElse Single.IsInfinity(v) Then Return 0
        Dim i As Long = CLng(Math.Round(v))
        If i < Integer.MinValue Then Return Integer.MinValue
        If i > Integer.MaxValue Then Return Integer.MaxValue
        Return CInt(i)
    End Function

    ''' <summary>ทำสีให้สว่างขึ้น</summary>
    Public Function Lighten(c As Color, amount As Integer) As Color
        Return Color.FromArgb(
            ClampByte(c.R + amount),
            ClampByte(c.G + amount),
            ClampByte(c.B + amount)
        )
    End Function

    ''' <summary>ทำสีให้เข้มขึ้น</summary>
    Public Function Darken(c As Color, amount As Integer) As Color
        Return Color.FromArgb(
            ClampByte(c.R - amount),
            ClampByte(c.G - amount),
            ClampByte(c.B - amount)
        )
    End Function

    ''' <summary>คำนวณ Easing — Ease Out Cubic</summary>
    Public Function EaseOutCubic(t As Double) As Double
        Return 1.0 - Math.Pow(1.0 - t, 3)
    End Function

End Module


' ╔═══════════════════════════════════════════════════════════════════╗
' ║  NATIVE API — Windows DWM                                        ║
' ╚═══════════════════════════════════════════════════════════════════╝
Public Module NvNativeAPI
    <DllImport("winmm.dll")>
    Public Function timeBeginPeriod(uPeriod As UInteger) As UInteger
    End Function

    <DllImport("dwmapi.dll")>
    Public Function DwmExtendFrameIntoClientArea(hwnd As IntPtr, ByRef pMarInset As MARGINS) As Integer
    End Function

    <DllImport("dwmapi.dll", PreserveSig:=True)>
    Public Function DwmSetWindowAttribute(hwnd As IntPtr, attr As Integer, ByRef attrValue As Integer, attrSize As Integer) As Integer
    End Function

    Public Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20
    Public Const DWMWA_SYSTEMBACKDROP_TYPE As Integer = 38

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MARGINS
        Public leftWidth As Integer
        Public rightWidth As Integer
        Public topHeight As Integer
        Public bottomHeight As Integer
    End Structure

    Public Sub Initialize()
        timeBeginPeriod(1)
    End Sub
End Module


' ╔══════════════════════════════════════════════════════════════════════════════════════╗
' ║  1. NvButton — ปุ่มหลัก ขอบโค้ง มี 7 variants, 3 sizes, glow + shimmer on hover   ║
' ╚══════════════════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvButton
    Inherits Control

    Public Enum NvButtonVariant
        Green
        Red
        Orange
        Blue
        Surface
        Ghost
        DangerGhost
    End Enum

    Public Enum NvBtnSize
        Small
        Medium
        Large
    End Enum

    ' ── Fields ──
    Private _variant As NvButtonVariant = NvButtonVariant.Green
    Private _btnSize As NvBtnSize = NvBtnSize.Medium
    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _cornerRadius As Integer = 10
    Private _glowAlpha As Single = 0F
    Private _targetGlowAlpha As Single = 0F
    Private _shimmerPhase As Single = -1.0F
    Private _isDisabled As Boolean = False
    Private _btnImage As Image = Nothing
    Private _imageSize As Size = New Size(16, 16)
    Private _animTimer As Timer

    ' ── Color lookup ──
    Private Function GetColors() As Color()
        Select Case _variant
            Case NvButtonVariant.Green
                Return New Color() {
                    Color.FromArgb(118, 185, 0),
                    Color.FromArgb(142, 214, 0),
                    Color.FromArgb(90, 143, 0),
                    Color.FromArgb(0, 0, 0),
                    Color.FromArgb(77, 118, 185, 0)
                }
            Case NvButtonVariant.Red
                Return New Color() {
                    Color.FromArgb(255, 59, 59),
                    Color.FromArgb(255, 85, 85),
                    Color.FromArgb(204, 32, 32),
                    Color.FromArgb(255, 255, 255),
                    Color.FromArgb(77, 255, 59, 59)
                }
            Case NvButtonVariant.Orange
                Return New Color() {
                    Color.FromArgb(255, 149, 0),
                    Color.FromArgb(255, 170, 51),
                    Color.FromArgb(204, 119, 0),
                    Color.FromArgb(0, 0, 0),
                    Color.FromArgb(77, 255, 149, 0)
                }
            Case NvButtonVariant.Blue
                Return New Color() {
                    Color.FromArgb(74, 158, 255),
                    Color.FromArgb(106, 180, 255),
                    Color.FromArgb(51, 128, 221),
                    Color.FromArgb(255, 255, 255),
                    Color.FromArgb(77, 74, 158, 255)
                }
            Case NvButtonVariant.Surface
                Return New Color() {
                    Color.FromArgb(42, 42, 42),
                    Color.FromArgb(51, 51, 51),
                    Color.FromArgb(30, 30, 30),
                    Color.FromArgb(232, 232, 232),
                    Color.FromArgb(0, 0, 0, 0)
                }
            Case NvButtonVariant.Ghost
                Return New Color() {
                    Color.Transparent,
                    Color.FromArgb(255, 255, 255, 15),
                    Color.FromArgb(255, 255, 255, 8),
                    Color.FromArgb(160, 160, 160),
                    Color.FromArgb(0, 0, 0, 0)
                }
            Case NvButtonVariant.DangerGhost
                Return New Color() {
                    Color.Transparent,
                    Color.FromArgb(255, 59, 59, 20),
                    Color.FromArgb(255, 59, 59, 10),
                    Color.FromArgb(255, 59, 59),
                    Color.FromArgb(0, 0, 0, 0)
                }
            Case Else
                Return New Color() {Color.Gray, Color.LightGray, Color.DarkGray, Color.White, Color.Transparent}
        End Select
    End Function

    Private Function IsColored() As Boolean
        Return _variant <= NvButtonVariant.Blue
    End Function

    ' ── Properties ──
    <Category("NVIDIA")>
    Public Property ButtonVariant As NvButtonVariant
        Get
            Return _variant
        End Get
        Set(value As NvButtonVariant)
            _variant = value
            UpdateSize()
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property ButtonSizeSetting As NvBtnSize
        Get
            Return _btnSize
        End Get
        Set(value As NvBtnSize)
            _btnSize = value
            UpdateSize()
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property ButtonImage As Image
        Get
            Return _btnImage
        End Get
        Set(value As Image)
            _btnImage = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property ButtonImageSize As Size
        Get
            Return _imageSize
        End Get
        Set(value As Size)
            _imageSize = value
            Invalidate()
        End Set
    End Property

    ' ── Constructor ──
    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Font = New Font("Segoe UI", 13.0F, FontStyle.Regular)
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
        UpdateSize()
    End Sub

    Private Sub UpdateSize()
        Select Case _btnSize
            Case NvBtnSize.Small : Height = 32
            Case NvBtnSize.Medium : Height = 38
            Case NvBtnSize.Large : Height = 46
        End Select
        AdjustWidth()
    End Sub

    ' ── Animation ──
    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim changed As Boolean = False

        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.2F
            changed = True
        ElseIf _scale <> _targetScale Then
            _scale = _targetScale
            changed = True
        End If

        Dim gd As Single = _targetGlowAlpha - _glowAlpha
        If Math.Abs(gd) > 0.01F Then
            _glowAlpha += gd * 0.15F
            changed = True
        ElseIf _glowAlpha <> _targetGlowAlpha Then
            _glowAlpha = _targetGlowAlpha
            changed = True
        End If

        If _shimmerPhase >= 0 Then
            _shimmerPhase += 0.025F
            changed = True
        End If

        If changed Then Invalidate()
    End Sub

    ' ── Paint ──
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim c As Color() = GetColors()
        Dim radius As Integer
        If _cornerRadius > 0 Then
            radius = _cornerRadius
        Else
            Select Case _btnSize
                Case NvBtnSize.Small : radius = 8
                Case NvBtnSize.Medium : radius = 10
                Case Else : radius = 12
            End Select
        End If

        If _isDisabled Then
            DrawBody(g, Color.FromArgb(74, 74, 74), Color.FromArgb(102, 102, 102), radius, False, False)
            DrawContent(g, Color.FromArgb(102, 102, 102))
            Return
        End If

        Dim bgColor As Color
        If _isPressed Then
            bgColor = c(2)
        ElseIf _isHovered Then
            bgColor = c(1)
        Else
            bgColor = c(0)
        End If

        Dim textColor As Color = c(3)
        Dim colored As Boolean = IsColored()
        Dim hasGlow As Boolean = colored AndAlso _glowAlpha > 0.01F
        Dim hasBorder As Boolean = (Not colored)
        Dim hasShimmer As Boolean = colored AndAlso _isHovered AndAlso Not _isPressed

        DrawBody(g, bgColor, textColor, radius, hasGlow, hasBorder)
        DrawContent(g, textColor)
    End Sub

    Private Sub DrawBody(g As Graphics, bgColor As Color, textColor As Color, radius As Integer, hasGlow As Boolean, hasBorder As Boolean)
        Dim rect As Rectangle = ClientRectangle

        Using path As GraphicsPath = RoundedRect(rect, radius)
            If hasGlow Then
                Dim glowC As Color = GetColors()(4)
                Using glowPath As GraphicsPath = RoundedRect(
                    New Rectangle(rect.X - 4, rect.Y - 4, rect.Width + 8, rect.Height + 8), radius + 4)
                    Using pgb As New PathGradientBrush(glowPath)
                        pgb.CenterColor = Color.FromArgb(SafeCInt(_glowAlpha * 77), glowC)
                        pgb.SurroundColors = New Color() {Color.FromArgb(0, glowC)}
                        g.FillPath(pgb, glowPath)
                    End Using
                End Using
            End If

            If bgColor <> Color.Transparent Then
                Using brush As New SolidBrush(bgColor)
                    g.FillPath(brush, path)
                End Using
            End If

            If hasBorder Then
                Dim borderAlpha As Integer
                If _isHovered Then borderAlpha = 36 Else borderAlpha = 18
                Using pen As New Pen(Color.FromArgb(borderAlpha, 255, 255, 255), 1.0F)
                    g.DrawPath(pen, path)
                End Using
            End If

            If IsColored() Then
                Using hlPath As GraphicsPath = RoundedRect(
                    New Rectangle(rect.X, rect.Y, rect.Width, rect.Height \ 2), radius)
                    Using hlBrush As New LinearGradientBrush(
                        New PointF(0, rect.Y), New PointF(0, rect.Y + rect.Height \ 2),
                        Color.FromArgb(30, 255, 255, 255), Color.Transparent)
                        g.SetClip(path)
                        g.FillPath(hlBrush, hlPath)
                        g.ResetClip()
                    End Using
                End Using
            End If
        End Using
    End Sub

    Private Sub DrawContent(g As Graphics, textColor As Color)
        Dim rect As Rectangle = ClientRectangle
        Dim hasImage As Boolean = (_btnImage IsNot Nothing)
        Dim hasText As Boolean = Not String.IsNullOrEmpty(Text)
        If Not hasImage AndAlso Not hasText Then Return

        Dim textW As Single = 0
        If hasText Then
            textW = g.MeasureString(Text, Font).Width
        End If
        Dim imgW As Single = 0
        If hasImage Then imgW = _imageSize.Width + 8
        Dim totalW As Single = imgW + textW
        Dim startX As Single = (rect.Width - totalW) / 2
        Dim centerY As Single = rect.Height / 2

        Using txtBrush As New SolidBrush(textColor)
            Dim x As Single = startX
            If hasImage Then
                g.DrawImage(_btnImage, x, centerY - _imageSize.Height / 2.0F, CSng(_imageSize.Width), CSng(_imageSize.Height))
                x += _imageSize.Width + 8
            End If
            If hasText Then
                Dim sf As New StringFormat()
                sf.Alignment = StringAlignment.Near
                sf.LineAlignment = StringAlignment.Center
                g.DrawString(Text, Font, txtBrush, New RectangleF(x, 0, textW + 4, rect.Height), sf)
            End If
        End Using
    End Sub

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        AdjustWidth()
    End Sub

    Private Sub AdjustWidth()
        If Not String.IsNullOrEmpty(Text) Then
            Using g As Graphics = CreateGraphics()
                Dim textW As Single = g.MeasureString(Text, Font).Width
                Dim imgW As Single = 0
                If _btnImage IsNot Nothing Then imgW = _imageSize.Width + 8
                Dim pad As Integer
                Select Case _btnSize
                    Case NvBtnSize.Small : pad = 14
                    Case NvBtnSize.Medium : pad = 20
                    Case Else : pad = 28
                End Select
                Width = CInt(textW + imgW + pad * 2)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        If _isDisabled Then Return
        _isHovered = True
        _targetScale = 1.0F
        If IsColored() Then
            _targetGlowAlpha = 1.0F
            _shimmerPhase = 0F
        End If
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        _targetScale = 1.0F
        _targetGlowAlpha = 0F
        _shimmerPhase = -1.0F
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If _isDisabled OrElse e.Button <> MouseButtons.Left Then Return
        _isPressed = True
        _targetScale = 0.97F
        _shimmerPhase = -1.0F
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If _isDisabled OrElse e.Button <> MouseButtons.Left Then Return
        _isPressed = False
        _targetScale = 1.0F
        If _isHovered AndAlso IsColored() Then _shimmerPhase = 0F
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  2. NvIconButton — ปุ่มไอคอน สี่เหลี่ยมขอบโค้ง / กลม                   ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvIconButton
    Inherits Control

    Public Enum IconVariant
        Surface
        Green
        Red
        Ghost
        DangerGhost
    End Enum

    Private _variant As IconVariant = IconVariant.Surface
    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _active As Boolean = False
    Private _cornerRadius As Integer = 10
    Private _glowAlpha As Single = 0F
    Private _targetGlowAlpha As Single = 0F
    Private _btnImage As Image = Nothing
    Private _imagePadding As Single = 0.45F
    Private _animTimer As Timer

    <Category("NVIDIA")>
    Public Property ButtonVariant As IconVariant
        Get
            Return _variant
        End Get
        Set(value As IconVariant)
            _variant = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property IsActive As Boolean
        Get
            Return _active
        End Get
        Set(value As Boolean)
            _active = value
            _targetGlowAlpha = If(value, 1.0F, 0F)
            _animTimer.Start()
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA")>
    Public Property IconImage As Image
        Get
            Return _btnImage
        End Get
        Set(value As Image)
            _btnImage = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Size = New Size(36, 36)
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim changed As Boolean = False
        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.2F
            changed = True
        ElseIf _scale <> _targetScale Then
            _scale = _targetScale
            changed = True
        End If
        Dim gd As Single = _targetGlowAlpha - _glowAlpha
        If Math.Abs(gd) > 0.01F Then
            _glowAlpha += gd * 0.15F
            changed = True
        ElseIf _glowAlpha <> _targetGlowAlpha Then
            _glowAlpha = _targetGlowAlpha
            changed = True
        End If
        If changed Then Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim rect As Rectangle = ClientRectangle
        Dim bgColor As Color
        Dim textColor As Color
        Dim glowColor As Color
        Dim hasBorder As Boolean = True
        Dim hasGlow As Boolean = False

        Select Case _variant
            Case IconVariant.Surface
                If _isPressed Then
                    bgColor = Color.FromArgb(30, 30, 30)
                ElseIf _isHovered Then
                    bgColor = Color.FromArgb(51, 51, 51)
                Else
                    bgColor = Color.FromArgb(42, 42, 42)
                End If
                If _active Then
                    textColor = Color.FromArgb(118, 185, 0)
                Else
                    textColor = Color.FromArgb(160, 160, 160)
                End If
                glowColor = Color.FromArgb(118, 185, 0)
                hasGlow = _active AndAlso _glowAlpha > 0.01F

            Case IconVariant.Green
                If _isPressed Then
                    bgColor = Color.FromArgb(90, 143, 0)
                ElseIf _isHovered Then
                    bgColor = Color.FromArgb(142, 214, 0)
                Else
                    bgColor = Color.FromArgb(118, 185, 0)
                End If
                textColor = Color.FromArgb(0, 0, 0)
                glowColor = Color.FromArgb(118, 185, 0)
                hasGlow = _isHovered
                hasBorder = False

            Case IconVariant.Red
                If _isPressed Then
                    bgColor = Color.FromArgb(204, 32, 32)
                ElseIf _isHovered Then
                    bgColor = Color.FromArgb(255, 85, 85)
                Else
                    bgColor = Color.FromArgb(255, 59, 59)
                End If
                textColor = Color.FromArgb(255, 255, 255)
                glowColor = Color.FromArgb(255, 59, 59)
                hasGlow = _isHovered
                hasBorder = False

            Case IconVariant.Ghost
                If _isHovered Then
                    bgColor = Color.FromArgb(255, 255, 255, 15)
                Else
                    bgColor = Color.Transparent
                End If
                textColor = Color.FromArgb(160, 160, 160)
                hasBorder = _isHovered

            Case IconVariant.DangerGhost
                If _isHovered Then
                    bgColor = Color.FromArgb(255, 59, 59, 20)
                Else
                    bgColor = Color.Transparent
                End If
                textColor = Color.FromArgb(255, 59, 59)
                hasBorder = _isHovered

            Case Else
                bgColor = Color.FromArgb(42, 42, 42)
                textColor = Color.FromArgb(160, 160, 160)
        End Select

        Using path As GraphicsPath = RoundedRect(rect, _cornerRadius)
            If hasGlow Then
                Using gp As GraphicsPath = RoundedRect(
                    New Rectangle(rect.X - 6, rect.Y - 6, rect.Width + 12, rect.Height + 12), _cornerRadius + 6)
                    Using pgb As New PathGradientBrush(gp)
                        pgb.CenterColor = Color.FromArgb(SafeCInt(_glowAlpha * 50), glowColor)
                        pgb.SurroundColors = New Color() {Color.FromArgb(0, glowColor)}
                        g.FillPath(pgb, gp)
                    End Using
                End Using
            End If

            If bgColor <> Color.Transparent Then
                Using brush As New SolidBrush(bgColor)
                    g.FillPath(brush, path)
                End Using
            End If

            If hasBorder Then
                Dim ba As Integer
                If _isHovered Then ba = 36 Else ba = 18
                Using pen As New Pen(Color.FromArgb(ba, 255, 255, 255), 1.0F)
                    g.DrawPath(pen, path)
                End Using
            End If
        End Using

        If _btnImage IsNot Nothing Then
            Dim imgSz As Single = Math.Min(Width, Height) * _imagePadding
            g.DrawImage(_btnImage, (Width - imgSz) / 2.0F, (Height - imgSz) / 2.0F, imgSz, imgSz)
        End If
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetScale = 1.05F
        If _variant = IconVariant.Green OrElse _variant = IconVariant.Red Then
            _targetGlowAlpha = 1.0F
        End If
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        _targetScale = 1.0F
        _targetGlowAlpha = If(_active, 1.0F, 0F)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = True
            _targetScale = 0.93F
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = False
            If _isHovered Then
                _targetScale = 1.05F
            Else
                _targetScale = 1.0F
            End If
        End If
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  3. NvRecordButton — ปุ่ม Record พิเศษ                                  ║
' ║     วงกลม (idle) → สี่เหลี่ยมขอบโค้ง (recording), pulse animation  ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvRecordButton
    Inherits Control

    Private _isRecording As Boolean = False
    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _morphProgress As Single = 0F
    Private _targetMorph As Single = 0F
    Private _pulsePhase As Single = 0F
    Private _glowAlpha As Single = 0F
    Private _targetGlowAlpha As Single = 0F
    Private _animTimer As Timer

    <Category("NVIDIA Behavior")>
    Public Property IsRecording As Boolean
        Get
            Return _isRecording
        End Get
        Set(value As Boolean)
            _isRecording = value
            _targetMorph = If(value, 1.0F, 0F)
            _animTimer.Start()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Size = New Size(52, 52)
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim changed As Boolean = False

        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.2F
            changed = True
        ElseIf _scale <> _targetScale Then
            _scale = _targetScale
            changed = True
        End If

        Dim md As Single = _targetMorph - _morphProgress
        If Math.Abs(md) > 0.005F Then
            _morphProgress += md * 0.1F
            changed = True
        ElseIf _morphProgress <> _targetMorph Then
            _morphProgress = _targetMorph
            changed = True
        End If

        Dim gd As Single = _targetGlowAlpha - _glowAlpha
        If Math.Abs(gd) > 0.01F Then
            _glowAlpha += gd * 0.15F
            changed = True
        End If

        If _isRecording Then
            _pulsePhase += 0.03F
            If _pulsePhase > 1.0F Then _pulsePhase -= 1.0F
            changed = True
        End If

        If changed Then Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim cx As Single = Width / 2.0F
        Dim cy As Single = Height / 2.0F
        Dim baseR As Single = Math.Min(Width, Height) / 2.0F - 4
        Dim r As Single = baseR * _scale

        Dim recordColor As Color = Color.FromArgb(118, 185, 0)
        Dim stopColor As Color = Color.FromArgb(255, 59, 59)
        Dim currentColor As Color = LerpColor(recordColor, stopColor, _morphProgress)
        Dim hoverColor As Color = LerpColor(Color.FromArgb(142, 214, 0), Color.FromArgb(255, 85, 85), _morphProgress)
        Dim pressColor As Color = LerpColor(Color.FromArgb(90, 143, 0), Color.FromArgb(204, 32, 32), _morphProgress)

        Dim btnColor As Color
        If _isPressed Then
            btnColor = pressColor
        ElseIf _isHovered Then
            btnColor = hoverColor
        Else
            btnColor = currentColor
        End If

        ' Pulse ring
        If _isRecording AndAlso Not _isHovered Then
            For i As Integer = 0 To 1
                Dim phase As Single = (_pulsePhase + i * 0.5F) Mod 1.0F
                Dim alpha As Integer = CInt((1.0F - phase) * If(i = 0, 80, 50))
                Dim pr As Single = r + (phase * 20)
                Dim pw As Single = 2.0F * (1.0F - phase)
                If pw > 0.2F Then
                    Using pp As New Pen(Color.FromArgb(alpha, stopColor), pw)
                        g.DrawEllipse(pp, cx - pr, cy - pr, pr * 2, pr * 2)
                    End Using
                End If
            Next
        End If

        ' Glow
        If _glowAlpha > 0.01F Then
            Using gp As New GraphicsPath()
                gp.AddEllipse(cx - r - 8, cy - r - 8, (r + 8) * 2, (r + 8) * 2)
                Using pgb As New PathGradientBrush(gp)
                    pgb.CenterColor = Color.FromArgb(SafeCInt(_glowAlpha * 60), btnColor)
                    pgb.SurroundColors = New Color() {Color.FromArgb(0, btnColor)}
                    g.FillPath(pgb, gp)
                End Using
            End Using
        End If

        ' Outer ring
        Dim ringAlpha As Integer
        If _isRecording Then
            ringAlpha = CInt(40 + 15 * Math.Sin(_pulsePhase * Math.PI * 2))
        Else
            ringAlpha = 40
        End If
        Using ringPen As New Pen(Color.FromArgb(ringAlpha, currentColor), 2.0F)
            g.DrawEllipse(ringPen, cx - r - 3, cy - r - 3, (r + 3) * 2, (r + 3) * 2)
        End Using

        ' Main shape (morph circle → rounded rect)
        Dim innerR As Single = r * 0.72F
        Dim mainPath As GraphicsPath

        If _morphProgress < 0.01F Then
            mainPath = New GraphicsPath()
            mainPath.AddEllipse(cx - innerR, cy - innerR, innerR * 2, innerR * 2)
        Else
            Dim sz As Single = innerR * (0.5F + 0.5F * _morphProgress)
            Dim cR As Single = (r * (1.0F - _morphProgress)) * 0.3F
            mainPath = RoundedRect(New Rectangle(CInt(cx - sz), CInt(cy - sz), CInt(sz * 2), CInt(sz * 2)), CInt(cR))
        End If

        Using mainPath
            Using gradBrush As New PathGradientBrush(mainPath)
                gradBrush.CenterPoint = New PointF(cx - innerR * 0.2F, cy - innerR * 0.2F)
                gradBrush.CenterColor = Lighten(btnColor, 25)
                gradBrush.SurroundColors = New Color() {Darken(btnColor, 15)}
                g.FillPath(gradBrush, mainPath)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        IsRecording = Not IsRecording
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetScale = 1.08F
        _targetGlowAlpha = 1.0F
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        _targetScale = 1.0F
        If _isRecording Then
            _targetGlowAlpha = 0.5F
        Else
            _targetGlowAlpha = 0F
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = True
            _targetScale = 0.92F
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = False
            If _isHovered Then
                _targetScale = 1.08F
            Else
                _targetScale = 1.0F
            End If
        End If
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  4. NvTitleBarButton — ปุ่ม Min/Max/Close สำหรับ Title Bar              ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvTitleBarButton
    Inherits Control

    Public Enum TitleBtnType
        Minimize
        Maximize
        Close
    End Enum

    Private _btnType As TitleBtnType = TitleBtnType.Close
    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _animTimer As Timer

    <Category("NVIDIA")>
    Public Property ButtonType As TitleBtnType
        Get
            Return _btnType
        End Get
        Set(value As TitleBtnType)
            _btnType = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Size = New Size(32, 32)
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.25F
            Invalidate()
        ElseIf _scale <> _targetScale Then
            _scale = _targetScale
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect As New Rectangle(0, 0, Width, Height)
        Dim iconColor As Color
        Dim bgHoverColor As Color = Color.FromArgb(255, 255, 255, 20)

        If _btnType = TitleBtnType.Close Then
            If _isHovered Then
                bgHoverColor = Color.FromArgb(232, 17, 35)
                iconColor = Color.White
            Else
                iconColor = Color.FromArgb(102, 102, 102)
            End If
        Else
            If _isHovered Then
                iconColor = Color.White
            Else
                iconColor = Color.FromArgb(102, 102, 102)
            End If
        End If

        If _isHovered Then
            Using path As GraphicsPath = RoundedRect(rect, 6)
                Using brush As New SolidBrush(bgHoverColor)
                    g.FillPath(brush, path)
                End Using
                If _btnType = TitleBtnType.Close Then
                    Using gp As GraphicsPath = RoundedRect(
                        New Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6), 9)
                        Using pgb As New PathGradientBrush(gp)
                            pgb.CenterColor = Color.FromArgb(30, 232, 17, 35)
                            pgb.SurroundColors = New Color() {Color.FromArgb(0, 232, 17, 35)}
                            g.FillPath(pgb, gp)
                        End Using
                    End Using
                End If
            End Using
        End If

        Dim cx As Single = Width / 2.0F
        Dim cy As Single = Height / 2.0F
        Dim iconSz As Single
        If _btnType = TitleBtnType.Close Then
            iconSz = 8.0F
        Else
            iconSz = 7.0F
        End If

        Using pen As New Pen(iconColor, 1.5F)
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round

            Select Case _btnType
                Case TitleBtnType.Minimize
                    g.DrawLine(pen, cx - iconSz, cy, cx + iconSz, cy)
                Case TitleBtnType.Maximize
                    g.DrawRectangle(pen, cx - iconSz, cy - iconSz, iconSz * 2, iconSz * 2)
                Case TitleBtnType.Close
                    g.DrawLine(pen, cx - iconSz, cy - iconSz, cx + iconSz, cy + iconSz)
                    g.DrawLine(pen, cx + iconSz, cy - iconSz, cx - iconSz, cy + iconSz)
            End Select
        End Using
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetScale = 1.0F
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        _targetScale = 1.0F
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = True
            _targetScale = 0.9F
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = False
            _targetScale = 1.0F
        End If
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  5. NvToggleButton — สวิตช์ NVIDIA (gradient + glow, 3 ขนาด)           ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("ValueChanged")>
Public Class NvToggleButton
    Inherits Control

    Public Enum ToggleSizeMode
        Small
        Medium
        Large
    End Enum

    Private _isOn As Boolean = False
    Private _togglePos As Single = 0F
    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _onColor As Color = Color.FromArgb(118, 185, 0)
    Private _offColor As Color = Color.FromArgb(74, 74, 74)
    Private _sizeMode As ToggleSizeMode = ToggleSizeMode.Medium
    Private _showGlow As Boolean = True
    Private _animTimer As Timer
    Private _sw As System.Diagnostics.Stopwatch
    Private _animDuration As Integer = 300

    <Category("NVIDIA Behavior")>
    Public Property IsOn As Boolean
        Get
            Return _isOn
        End Get
        Set(value As Boolean)
            If _isOn <> value Then
                _isOn = value
                _sw.Restart()
                _animTimer.Start()
                RaiseEvent ValueChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property OnColor As Color
        Get
            Return _onColor
        End Get
        Set(value As Color)
            _onColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property OffColor As Color
        Get
            Return _offColor
        End Get
        Set(value As Color)
            _offColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property SizeMode As ToggleSizeMode
        Get
            Return _sizeMode
        End Get
        Set(value As ToggleSizeMode)
            _sizeMode = value
            Select Case value
                Case ToggleSizeMode.Small : Size = New Size(36, 20)
                Case ToggleSizeMode.Medium : Size = New Size(44, 24)
                Case ToggleSizeMode.Large : Size = New Size(52, 28)
            End Select
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property ShowGlow As Boolean
        Get
            Return _showGlow
        End Get
        Set(value As Boolean)
            _showGlow = value
            Invalidate()
        End Set
    End Property

    Public Event ValueChanged As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Size = New Size(44, 24)
        _animTimer = New Timer With {.Interval = 1}
        _sw = New System.Diagnostics.Stopwatch()
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim elapsed As Double = _sw.Elapsed.TotalMilliseconds / _animDuration
        If elapsed >= 1.0 Then
            elapsed = 1.0
            _animTimer.Stop()
            _sw.Stop()
        End If
        Dim eased As Double = EaseOutCubic(elapsed)
        If _isOn Then
            _togglePos = CSng(eased)
        Else
            _togglePos = CSng(1.0 - eased)
        End If
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim w As Single = Width
        Dim h As Single = Height
        Dim thumbDiam As Single = h - 6
        Dim margin As Single = 3
        Dim trackR As Single = h / 2.0F

        Dim trackColor As Color = LerpColor(_offColor, _onColor, _togglePos)
        Dim thumbX As Single = margin + (w - thumbDiam - margin * 2) * _togglePos
        Dim thumbY As Single = (h - thumbDiam) / 2.0F

        ' Glow
        If _showGlow AndAlso _togglePos > 0.05F Then
            Dim glowAlpha As Integer = SafeCInt(_togglePos * 50)
            Using gp As New GraphicsPath()
                gp.AddEllipse(-4, -4, w + 8, h + 8)
                Using pgb As New PathGradientBrush(gp)
                    pgb.CenterColor = Color.FromArgb(glowAlpha, _onColor)
                    pgb.SurroundColors = New Color() {Color.FromArgb(0, _onColor)}
                    g.FillPath(pgb, gp)
                End Using
            End Using
        End If

        ' Track
        Using trackPath As GraphicsPath = RoundedRect(New Rectangle(0, 0, CInt(w), CInt(h)), CInt(trackR))
            If _togglePos > 0.01F Then
                Using gb As New LinearGradientBrush(
                    New PointF(0, 0), New PointF(w, h),
                    Lighten(_onColor, 15), _onColor)
                    g.FillPath(gb, trackPath)
                End Using
                If _togglePos < 0.99F Then
                    Using ob As New SolidBrush(Color.FromArgb(CInt((1.0F - _togglePos) * 255), _offColor))
                        g.SetClip(trackPath)
                        g.FillRectangle(ob, 0, 0, w, h)
                        g.ResetClip()
                    End Using
                End If
            Else
                Using ob As New SolidBrush(_offColor)
                    g.FillPath(ob, trackPath)
                End Using
            End If

            Using hl As New LinearGradientBrush(
                New PointF(0, 0), New PointF(0, h),
                Color.FromArgb(25, 255, 255, 255), Color.Transparent)
                g.SetClip(trackPath)
                g.FillRectangle(hl, 0, 0, w, h / 2.0F)
                g.ResetClip()
            End Using
        End Using

        ' Thumb
        Using thumbPath As New GraphicsPath()
            thumbPath.AddEllipse(thumbX, thumbY, thumbDiam, thumbDiam)
            Dim thumbColor As Color
            If _togglePos > 0.5F Then
                thumbColor = Color.White
            Else
                thumbColor = Color.FromArgb(136, 136, 136)
            End If
            Using tb As New SolidBrush(thumbColor)
                g.FillPath(tb, thumbPath)
            End Using
            g.DrawPath(New Pen(Color.FromArgb(60, 0, 0, 0), 1.0F), thumbPath)
            If _togglePos > 0.5F Then
                Using tg As New Pen(Color.FromArgb(SafeCInt(_togglePos * 80), _onColor), 1.5F)
                    g.DrawPath(tg, thumbPath)
                End Using
            End If
        End Using
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        IsOn = Not IsOn
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  6. NvCheckbox — เช็คบ็อกซ์ ขอบโค้ง มี gradient + glow + hover ขยาย  ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("CheckedChanged")>
Public Class NvCheckbox
    Inherits Control

    Private _checked As Boolean = False
    Private _isHovered As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _glowAlpha As Single = 0F
    Private _targetGlowAlpha As Single = 0F
    Private _checkProgress As Single = 0F
    Private _targetCheckProgress As Single = 0F
    Private _checkColor As Color = Color.FromArgb(118, 185, 0)
    Private _boxSize As Integer = 18
    Private _cornerRadius As Integer = 5
    Private _animTimer As Timer

    <Category("NVIDIA Behavior")>
    Public Property Checked As Boolean
        Get
            Return _checked
        End Get
        Set(value As Boolean)
            If _checked <> value Then
                _checked = value
                _targetCheckProgress = If(value, 1.0F, 0F)
                _targetGlowAlpha = If(value, 1.0F, 0F)
                _animTimer.Start()
                RaiseEvent CheckedChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property CheckColor As Color
        Get
            Return _checkColor
        End Get
        Set(value As Color)
            _checkColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property BoxSize As Integer
        Get
            Return _boxSize
        End Get
        Set(value As Integer)
            _boxSize = value
            UpdateSize()
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property BoxCornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = value
            Invalidate()
        End Set
    End Property

    Public Event CheckedChanged As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Font = New Font("Segoe UI", 13.0F)
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
        UpdateSize()
    End Sub

    Private Function GetTextWidth() As Integer
        If String.IsNullOrEmpty(Text) Then Return 0
        Using g As Graphics = CreateGraphics()
            Return CInt(g.MeasureString(Text, Font).Width) + 10
        End Using
    End Function

    Private Sub UpdateSize()
        Height = _boxSize
        Width = _boxSize + 4 + GetTextWidth()
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim changed As Boolean = False
        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.2F
            changed = True
        ElseIf _scale <> _targetScale Then
            _scale = _targetScale
            changed = True
        End If
        Dim gd As Single = _targetGlowAlpha - _glowAlpha
        If Math.Abs(gd) > 0.01F Then
            _glowAlpha += gd * 0.12F
            changed = True
        End If
        Dim cd As Single = _targetCheckProgress - _checkProgress
        If Math.Abs(cd) > 0.01F Then
            _checkProgress += cd * 0.12F
            changed = True
        End If
        If changed Then Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim bs As Integer = SafeCInt(_boxSize * _scale)
        Dim ox As Integer = (_boxSize - bs) \ 2
        Dim oy As Integer = (_boxSize - bs) \ 2
        Dim boxRect As New Rectangle(ox, oy, bs, bs)

        Dim bgColor As Color
        If _checkProgress > 0.5F Then
            bgColor = LerpColor(
                If(_isHovered, Color.FromArgb(51, 51, 51), Color.FromArgb(42, 42, 42)),
                _checkColor, (_checkProgress - 0.5F) * 2.0F)
        Else
            If _isHovered Then
                bgColor = Color.FromArgb(51, 51, 51)
            Else
                bgColor = Color.FromArgb(42, 42, 42)
            End If
        End If

        ' Glow
        If _glowAlpha > 0.01F Then
            Using gp As GraphicsPath = RoundedRect(
                New Rectangle(ox - 4, oy - 4, bs + 8, bs + 8), _cornerRadius + 4)
                Using pgb As New PathGradientBrush(gp)
                    pgb.CenterColor = Color.FromArgb(SafeCInt(_glowAlpha * 50), _checkColor)
                    pgb.SurroundColors = New Color() {Color.FromArgb(0, _checkColor)}
                    g.FillPath(pgb, gp)
                End Using
            End Using
        End If

        ' Box
        Using path As GraphicsPath = RoundedRect(boxRect, _cornerRadius)
            Using brush As New SolidBrush(bgColor)
                g.FillPath(brush, path)
            End Using
            Dim borderAlpha As Integer
            If _checkProgress > 0.5F Then
                borderAlpha = SafeCInt(_glowAlpha * 64)
            ElseIf _isHovered Then
                borderAlpha = 36
            Else
                borderAlpha = 18
            End If
            Using pen As New Pen(Color.FromArgb(borderAlpha, _checkColor), 1.0F)
                g.DrawPath(pen, path)
            End Using
        End Using

        ' Checkmark
        If _checkProgress > 0.01F Then
            Dim checkAlpha As Integer = SafeCInt(_checkProgress * 255)
            Using p As New Pen(Color.FromArgb(checkAlpha, 0, 0, 0), 2.5F)
                p.StartCap = LineCap.Round
                p.EndCap = LineCap.Round
                Dim cx As Single = ox + bs * 0.28F
                Dim cy As Single = oy + bs * 0.52F
                Dim mx As Single = ox + bs * 0.45F
                Dim my As Single = oy + bs * 0.72F
                Dim ex As Single = ox + bs * 0.75F
                Dim ey As Single = oy + bs * 0.3F

                If _checkProgress < 0.5F Then
                    Dim t As Single = _checkProgress * 2.0F
                    g.DrawLine(p, cx, cy, cx + (mx - cx) * t, cy + (my - cy) * t)
                Else
                    Dim t As Single = (_checkProgress - 0.5F) * 2.0F
                    g.DrawLine(p, cx, cy, mx, my)
                    g.DrawLine(p, mx, my, mx + (ex - mx) * t, my + (ey - my) * t)
                End If
            End Using
        End If

        ' Text
        If Not String.IsNullOrEmpty(Text) Then
            Using brush As New SolidBrush(Color.FromArgb(232, 232, 232))
                Dim sf As New StringFormat()
                sf.LineAlignment = StringAlignment.Center
                g.DrawString(Text, Font, brush, _boxSize + 10, _boxSize / 2.0F, sf)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        UpdateSize()
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetScale = 1.0F
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _targetScale = 1.0F
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        Checked = Not Checked
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  7. NvPillButton — ปุ่มแคปซูล (Tab/Filter)                             ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvPillButton
    Inherits Control

    Private _active As Boolean = False
    Private _isHovered As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _glowAlpha As Single = 0F
    Private _targetGlowAlpha As Single = 0F
    Private _activeColor As Color = Color.FromArgb(118, 185, 0)
    Private _pillImage As Image = Nothing
    Private _animTimer As Timer

    <Category("NVIDIA Behavior")>
    Public Property IsActive As Boolean
        Get
            Return _active
        End Get
        Set(value As Boolean)
            _active = value
            _targetGlowAlpha = If(value, 1.0F, 0F)
            _animTimer.Start()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property ActiveColor As Color
        Get
            Return _activeColor
        End Get
        Set(value As Color)
            _activeColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property PillImage As Image
        Get
            Return _pillImage
        End Get
        Set(value As Image)
            _pillImage = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        Height = 34
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim changed As Boolean = False
        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.2F
            changed = True
        End If
        Dim gd As Single = _targetGlowAlpha - _glowAlpha
        If Math.Abs(gd) > 0.01F Then
            _glowAlpha += gd * 0.12F
            changed = True
        End If
        If changed Then Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim rect As Rectangle = ClientRectangle
        Dim pillR As Integer = Height \ 2

        Using path As GraphicsPath = RoundedRect(rect, pillR)
            If _glowAlpha > 0.01F Then
                Using gp As GraphicsPath = RoundedRect(
                    New Rectangle(rect.X - 4, rect.Y - 4, rect.Width + 8, rect.Height + 8), pillR + 4)
                    Using pgb As New PathGradientBrush(gp)
                        pgb.CenterColor = Color.FromArgb(SafeCInt(_glowAlpha * 60), _activeColor)
                        pgb.SurroundColors = New Color() {Color.FromArgb(0, _activeColor)}
                        g.FillPath(pgb, gp)
                    End Using
                End Using
            End If

            If _active Then
                Using gb As New LinearGradientBrush(
                    New PointF(0, 0), New PointF(Width, Height),
                    Color.FromArgb(142, 214, 0), Color.FromArgb(118, 185, 0))
                    g.FillPath(gb, path)
                End Using
                Using hl As New LinearGradientBrush(
                    New PointF(0, 0), New PointF(0, Height),
                    Color.FromArgb(30, 255, 255, 255), Color.Transparent)
                    g.SetClip(path)
                    g.FillRectangle(hl, 0, 0, Width, Height \ 2)
                    g.ResetClip()
                End Using
            Else
                Dim sbColor As Color
                If _isHovered Then
                    sbColor = Color.FromArgb(51, 51, 51)
                Else
                    sbColor = Color.FromArgb(42, 42, 42)
                End If
                Using sb As New SolidBrush(sbColor)
                    g.FillPath(sb, path)
                End Using
            End If

            Dim ba As Integer
            If _active Then
                ba = CInt(25 + _glowAlpha * 39)
            ElseIf _isHovered Then
                ba = 36
            Else
                ba = 18
            End If
            Dim bc As Color
            If _active Then
                bc = _activeColor
            Else
                bc = Color.White
            End If
            Using pen As New Pen(Color.FromArgb(ba, bc), 1.0F)
                g.DrawPath(pen, path)
            End Using
        End Using

        Dim hasImage As Boolean = (_pillImage IsNot Nothing)
        Dim hasText As Boolean = Not String.IsNullOrEmpty(Text)
        If hasImage OrElse hasText Then
            Dim textW As Single = 0
            If hasText Then textW = g.MeasureString(Text, Font).Width
            Dim imgW As Single = 0
            If hasImage Then imgW = 20
            Dim totalW As Single = imgW + textW
            Dim startX As Single = (Width - totalW) / 2
            Dim textColor As Color
            If _active Then
                textColor = Color.FromArgb(0, 0, 0)
            Else
                textColor = Color.FromArgb(160, 160, 160)
            End If
            Using tb As New SolidBrush(textColor)
                Dim x As Single = startX
                If hasImage Then
                    g.DrawImage(_pillImage, x, (Height - 14) / 2.0F, 14, 14)
                    x += 20
                End If
                If hasText Then
                    Dim sf As New StringFormat()
                    sf.Alignment = StringAlignment.Near
                    sf.LineAlignment = StringAlignment.Center
                    g.DrawString(Text, Font, tb, x, Height / 2.0F, sf)
                End If
            End Using
        End If
    End Sub

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        Using g As Graphics = CreateGraphics()
            Dim tw As Single = g.MeasureString(Text, Font).Width
            Dim iw As Single = 0
            If _pillImage IsNot Nothing Then iw = 20
            Width = CInt(tw + iw + 36)
        End Using
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetScale = 1.03F
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _targetScale = 1.0F
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then _targetScale = 0.97F
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            If _isHovered Then
                _targetScale = 1.03F
            Else
                _targetScale = 1.0F
            End If
        End If
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  8. NvActionCard — การ์ดคลิกได้ (hover ยกขึ้น, shortcut badge)       ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvActionCard
    Inherits Control

    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _liftY As Single = 0F
    Private _targetLiftY As Single = 0F
    Private _active As Boolean = False
    Private _cardColor As Color = Color.FromArgb(118, 185, 0)
    Private _cardImage As Image = Nothing
    Private _shortcutText As String = ""
    Private _descText As String = ""
    Private _animTimer As Timer

    <Category("NVIDIA Behavior")>
    Public Property IsActive As Boolean
        Get
            Return _active
        End Get
        Set(value As Boolean)
            _active = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property AccentColor As Color
        Get
            Return _cardColor
        End Get
        Set(value As Color)
            _cardColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property CardImage As Image
        Get
            Return _cardImage
        End Get
        Set(value As Image)
            _cardImage = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property ShortcutText As String
        Get
            Return _shortcutText
        End Get
        Set(value As String)
            _shortcutText = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property DescriptionText As String
        Get
            Return _descText
        End Get
        Set(value As String)
            _descText = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        Size = New Size(280, 56)
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim d As Single = _targetLiftY - _liftY
        If Math.Abs(d) > 0.1F Then
            _liftY += d * 0.2F
            Invalidate()
        ElseIf _liftY <> _targetLiftY Then
            _liftY = _targetLiftY
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim rect As New Rectangle(0, SafeCInt(_liftY), Width, Height)
        Dim radius As Integer = 12

        Using path As GraphicsPath = RoundedRect(rect, radius)
            Dim bgColor As Color
            If _active Then
                bgColor = Color.FromArgb(118, 185, 0, 20)
            ElseIf _isHovered Then
                bgColor = Color.FromArgb(51, 51, 51)
            Else
                bgColor = Color.FromArgb(42, 42, 42)
            End If
            Using brush As New SolidBrush(bgColor)
                g.FillPath(brush, path)
            End Using

            Dim ba As Integer
            If _active Then
                ba = 64
            ElseIf _isHovered Then
                ba = 36
            Else
                ba = 18
            End If
            Dim bc As Color
            If _active Then
                bc = _cardColor
            Else
                bc = Color.White
            End If
            Using pen As New Pen(Color.FromArgb(ba, bc), 1.0F)
                g.DrawPath(pen, path)
            End Using
        End Using

        ' Active glow
        If _active Then
            Using gp As GraphicsPath = RoundedRect(
                New Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6), radius + 3)
                Using pgb As New PathGradientBrush(gp)
                    pgb.CenterColor = Color.FromArgb(20, _cardColor)
                    pgb.SurroundColors = New Color() {Color.FromArgb(0, _cardColor)}
                    g.FillPath(pgb, gp)
                End Using
            End Using
        End If

        ' Icon box
        Dim iconBox As New Rectangle(14, rect.Y + (Height - 40) \ 2, 40, 40)
        Using iconPath As GraphicsPath = RoundedRect(iconBox, 10)
            If _active Then
                Using gb As New LinearGradientBrush(
                    iconBox.Location, New Point(iconBox.Right, iconBox.Bottom),
                    Color.FromArgb(142, 214, 0), Color.FromArgb(118, 185, 0))
                    g.FillPath(gb, iconPath)
                End Using
            Else
                Dim ibColor As Color
                If _isHovered Then
                    ibColor = Color.FromArgb(255, 255, 255, 15)
                Else
                    ibColor = Color.FromArgb(255, 255, 255, 8)
                End If
                Using ib As New SolidBrush(ibColor)
                    g.FillPath(ib, iconPath)
                End Using
            End If
        End Using

        ' Title text
        Dim textX As Integer = 64
        Dim titleColor As Color
        If _active Then
            titleColor = _cardColor
        Else
            titleColor = Color.FromArgb(232, 232, 232)
        End If
        Using titleBrush As New SolidBrush(titleColor)
            Dim sf As New StringFormat()
            sf.LineAlignment = StringAlignment.Center
            sf.Trimming = StringTrimming.EllipsisCharacter
            g.DrawString(Text, Font, titleBrush, textX, rect.Y + (Height \ 2) - 8, sf)
        End Using

        ' Description
        If Not String.IsNullOrEmpty(_descText) Then
            Using descBrush As New SolidBrush(Color.FromArgb(102, 102, 102))
                Using descFont As New Font("Segoe UI", 11.0F)
                    Dim sf As New StringFormat()
                    sf.LineAlignment = StringAlignment.Center
                    sf.Trimming = StringTrimming.EllipsisCharacter
                    g.DrawString(_descText, descFont, descBrush, textX, rect.Y + (Height \ 2) + 8, sf)
                End Using
            End Using
        End If

        ' Shortcut badge
        If Not String.IsNullOrEmpty(_shortcutText) Then
            Using badgeFont As New Font("Segoe UI", 10.0F, FontStyle.Bold)
                Dim badgeW As Single = g.MeasureString(_shortcutText, badgeFont).Width + 16
                Dim badgeH As Single = 22
                Dim badgeX As Single = Width - badgeW - 12
                Dim badgeY As Single = rect.Y + (Height - badgeH) / 2.0F
                Dim badgeRect As New Rectangle(CInt(badgeX), CInt(badgeY), CInt(badgeW), CInt(badgeH))
                Using badgePath As GraphicsPath = RoundedRect(badgeRect, 5)
                    Using bb As New SolidBrush(Color.FromArgb(255, 255, 255, 12))
                        g.FillPath(bb, badgePath)
                    End Using
                    Using bp As New Pen(Color.FromArgb(18, 255, 255, 255), 1.0F)
                        g.DrawPath(bp, badgePath)
                    End Using
                End Using
                Using tb As New SolidBrush(Color.FromArgb(102, 102, 102))
                    Dim sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Center
                    g.DrawString(_shortcutText, badgeFont, tb, badgeRect, sf)
                End Using
            End Using
        End If
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetLiftY = -1
        _animTimer.Start()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        _targetLiftY = 0
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = True
            _targetLiftY = 1
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = False
            If _isHovered Then
                _targetLiftY = -1
            Else
                _targetLiftY = 0
            End If
        End If
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  9. NvStatusDot — จุดบ่งบอกสถานะ                                       ║
' ╚══════════════════════════════════════════════════════════════════════════╝
Public Class NvStatusDot
    Inherits Control

    Public Enum DotStatus
        Running
        Stopped
        Loading
    End Enum

    Private _status As DotStatus = DotStatus.Running
    Private _dotSize As Integer = 8
    Private _pulsePhase As Single = 0F
    Private _animTimer As Timer

    <Category("NVIDIA Behavior")>
    Public Property Status As DotStatus
        Get
            Return _status
        End Get
        Set(value As DotStatus)
            _status = value
            _animTimer.Start()
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property DotSize As Integer
        Get
            Return _dotSize
        End Get
        Set(value As Integer)
            _dotSize = value
            Size = New Size(value + 16, value + 16)
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Size = New Size(24, 24)
        _animTimer = New Timer With {.Interval = 16}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        If _status = DotStatus.Loading Then
            _pulsePhase += 0.06F
            If _pulsePhase > 1.0F Then _pulsePhase -= 1.0F
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim cx As Single = Width / 2.0F
        Dim cy As Single = Height / 2.0F
        Dim r As Single = _dotSize / 2.0F

        Dim dotColor As Color
        Select Case _status
            Case DotStatus.Running : dotColor = Color.FromArgb(118, 185, 0)
            Case DotStatus.Stopped : dotColor = Color.FromArgb(74, 74, 74)
            Case DotStatus.Loading : dotColor = Color.FromArgb(255, 149, 0)
            Case Else : dotColor = Color.FromArgb(74, 74, 74)
        End Select

        If _status <> DotStatus.Stopped Then
            Dim glowAlpha As Integer
            If _status = DotStatus.Loading Then
                glowAlpha = CInt(30 + 20 * Math.Sin(_pulsePhase * Math.PI * 2))
            Else
                glowAlpha = 50
            End If
            Using gp As New GraphicsPath()
                gp.AddEllipse(cx - r - 4, cy - r - 4, (r + 4) * 2, (r + 4) * 2)
                Using pgb As New PathGradientBrush(gp)
                    pgb.CenterColor = Color.FromArgb(glowAlpha, dotColor)
                    pgb.SurroundColors = New Color() {Color.FromArgb(0, dotColor)}
                    g.FillPath(pgb, gp)
                End Using
            End Using
        End If

        Using dotPath As New GraphicsPath()
            dotPath.AddEllipse(cx - r, cy - r, r * 2, r * 2)
            Using brush As New SolidBrush(dotColor)
                g.FillPath(brush, dotPath)
            End Using
        End Using
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  10. NvBlurPanel — พาเนลเบลอๆ (Glassmorphism)                          ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultProperty("BlurColor")>
Public Class NvBlurPanel
    Inherits Panel

    Private _blurColor As Color = Color.FromArgb(3, 255, 255, 255)
    Private _borderColor As Color = Color.FromArgb(6, 255, 255, 255)
    Private _cornerRadius As Integer = 12
    Private _glowColor As Color = Color.Transparent
    Private _glowSize As Integer = 0

    <Category("NVIDIA Appearance")>
    Public Property BlurColor As Color
        Get
            Return _blurColor
        End Get
        Set(value As Color)
            _blurColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property GlowColor As Color
        Get
            Return _glowColor
        End Get
        Set(value As Color)
            _glowColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property GlowSize As Integer
        Get
            Return _glowSize
        End Get
        Set(value As Integer)
            _glowSize = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim rect As New Rectangle(0, 0, Width, Height)

        If _glowSize > 0 AndAlso _glowColor <> Color.Transparent Then
            Using glowPath As GraphicsPath = RoundedRect(
                New Rectangle(-_glowSize, -_glowSize, Width + _glowSize * 2, Height + _glowSize * 2),
                _cornerRadius + _glowSize)
                Using gb As New SolidBrush(Color.FromArgb(40, _glowColor))
                    e.Graphics.FillPath(gb, glowPath)
                End Using
            End Using
        End If

        Using path As GraphicsPath = RoundedRect(rect, _cornerRadius)
            Using bg As New SolidBrush(_blurColor)
                e.Graphics.FillPath(bg, path)
            End Using
            Using border As New Pen(_borderColor, 1.0F)
                e.Graphics.DrawPath(border, path)
            End Using
        End Using
    End Sub
End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  11. NvCircleButton — ปุ่มกลม พร้อม glow + pulse ring                   ║
' ╚══════════════════════════════════════════════════════════════════════════╝
<DefaultEvent("Click")>
Public Class NvCircleButton
    Inherits Control

    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False
    Private _scale As Single = 1.0F
    Private _targetScale As Single = 1.0F
    Private _buttonColor As Color = Color.FromArgb(118, 185, 0)
    Private _hoverColor As Color = Color.FromArgb(142, 214, 0)
    Private _glowEnabled As Boolean = True
    Private _glowSize As Integer = 8
    Private _icon As String = ""
    Private _iconFont As Font = New Font("Segoe MDL2 Assets", 14.0F)
    Private _buttonSz As Integer = 48
    Private _pulseEnabled As Boolean = False
    Private _pulsePhase As Single = 0F
    Private _animTimer As Timer

    <Category("NVIDIA Appearance")>
    Public Property ButtonColor As Color
        Get
            Return _buttonColor
        End Get
        Set(value As Color)
            _buttonColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property HoverColor As Color
        Get
            Return _hoverColor
        End Get
        Set(value As Color)
            _hoverColor = value
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property GlowEnabled As Boolean
        Get
            Return _glowEnabled
        End Get
        Set(value As Boolean)
            _glowEnabled = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property PulseEnabled As Boolean
        Get
            Return _pulseEnabled
        End Get
        Set(value As Boolean)
            _pulseEnabled = value
            _animTimer.Start()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property IconText As String
        Get
            Return _icon
        End Get
        Set(value As String)
            _icon = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Layout")>
    Public Property CircleButtonSize As Integer
        Get
            Return _buttonSz
        End Get
        Set(value As Integer)
            _buttonSz = value
            Width = value
            Height = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.StandardDoubleClick, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Width = _buttonSz
        Height = _buttonSz
        _animTimer = New Timer With {.Interval = 1}
        AddHandler _animTimer.Tick, AddressOf AnimTick
    End Sub

    Private Sub AnimTick(sender As Object, e As EventArgs)
        Dim changed As Boolean = False
        Dim sd As Single = _targetScale - _scale
        If Math.Abs(sd) > 0.005F Then
            _scale += sd * 0.15F
            changed = True
        ElseIf _scale <> _targetScale Then
            _scale = _targetScale
            changed = True
        End If
        If _pulseEnabled Then
            _pulsePhase += 0.03F
            If _pulsePhase > 1.0F Then _pulsePhase -= 1.0F
            changed = True
        End If
        If changed Then Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim cx As Single = Width / 2.0F
        Dim cy As Single = Height / 2.0F
        Dim baseR As Single = Math.Min(Width, Height) / 2.0F - 2
        Dim r As Single = baseR * _scale
        Dim curColor As Color
        If _isPressed Then
            curColor = _hoverColor
        ElseIf _isHovered Then
            curColor = _hoverColor
        Else
            curColor = _buttonColor
        End If

        ' Pulse rings
        If _pulseEnabled AndAlso Not _isHovered Then
            For i As Integer = 0 To 1
                Dim phase As Single = (_pulsePhase + i * 0.5F) Mod 1.0F
                Dim alpha As Integer = CInt((1.0F - phase) * If(i = 0, 80, 50))
                Dim pr As Single = r + (phase * 20)
                Dim pw As Single = (2.0F - i * 0.5F) * (1.0F - phase)
                If pw > 0.2F Then
                    Using pp As New Pen(Color.FromArgb(alpha, _buttonColor), pw)
                        g.DrawEllipse(pp, cx - pr, cy - pr, pr * 2, pr * 2)
                    End Using
                End If
            Next
        End If

        ' Glow
        If _glowEnabled Then
            Dim gr As Single = r + _glowSize
            Using gp As New GraphicsPath()
                gp.AddEllipse(cx - gr, cy - gr, gr * 2, gr * 2)
                Using pgb As New PathGradientBrush(gp)
                    pgb.CenterColor = Color.FromArgb(50, _buttonColor)
                    pgb.SurroundColors = New Color() {Color.FromArgb(0, _buttonColor)}
                    g.FillPath(pgb, gp)
                End Using
            End Using
        End If

        ' Main circle
        Using mp As New GraphicsPath()
            mp.AddEllipse(cx - r, cy - r, r * 2, r * 2)
            Using gb As New PathGradientBrush(mp)
                gb.CenterPoint = New PointF(cx - r * 0.2F, cy - r * 0.2F)
                gb.CenterColor = Lighten(curColor, 30)
                gb.SurroundColors = New Color() {Darken(curColor, 20)}
                g.FillPath(gb, mp)
            End Using
            Using bp As New Pen(Color.FromArgb(40, 255, 255, 255), 1.0F)
                g.DrawPath(bp, mp)
            End Using
        End Using

        ' Highlight
        Using hp As New GraphicsPath()
            hp.AddEllipse(cx - r * 0.6F, cy - r * 0.7F, r * 1.2F, r * 0.5F)
            Using hb As New LinearGradientBrush(
                New PointF(cx, cy - r * 0.7F), New PointF(cx, cy - r * 0.2F),
                Color.FromArgb(60, 255, 255, 255), Color.Transparent)
                g.FillPath(hb, hp)
            End Using
        End Using

        ' Icon
        If Not String.IsNullOrEmpty(_icon) Then
            Dim ts As SizeF = g.MeasureString(_icon, _iconFont)
            g.DrawString(_icon, _iconFont, New SolidBrush(Color.FromArgb(240, 255, 255, 255)),
                cx - ts.Width / 2.0F, cy - ts.Height / 2.0F)
        End If
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _isHovered = True
        _targetScale = 1.08F
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        _targetScale = 1.0F
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = True
            _targetScale = 0.95F
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = False
            If _isHovered Then
                _targetScale = 1.08F
            Else
                _targetScale = 1.0F
            End If
        End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Dim s As Integer = Math.Min(Width, Height)
        If Width <> s OrElse Height <> s Then
            ClientSize = New Size(s, s)
        End If
    End Sub

End Class


' ╔══════════════════════════════════════════════════════════════════════════╗
' ║  12. NvGlowLabel — ข้อความที่มี glow effect                             ║
' ╚══════════════════════════════════════════════════════════════════════════╝
Public Class NvGlowLabel
    Inherits Control

    Private _glowColor As Color = Color.FromArgb(118, 185, 0)
    Private _glowSize As Integer = 3
    Private _glowAlpha As Integer = 40

    <Category("NVIDIA Appearance")>
    Public Property GlowColor As Color
        Get
            Return _glowColor
        End Get
        Set(value As Color)
            _glowColor = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property GlowSize As Integer
        Get
            Return _glowSize
        End Get
        Set(value As Integer)
            _glowSize = value
            Invalidate()
        End Set
    End Property

    <Category("NVIDIA Appearance")>
    Public Property GlowAlpha As Integer
        Get
            Return _glowAlpha
        End Get
        Set(value As Integer)
            _glowAlpha = value
            Invalidate()
        End Set
    End Property

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Font = New Font("Segoe UI", 12.0F)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        Dim sf As New StringFormat()
        sf.LineAlignment = StringAlignment.Center

        For i As Integer = _glowSize To 1 Step -1
            Using glowBrush As New SolidBrush(Color.FromArgb(_glowAlpha \ i, _glowColor))
                e.Graphics.DrawString(Text, Font, glowBrush, New RectangleF(i, i, Width, Height), sf)
            End Using
        Next

        Using textBrush As New SolidBrush(ForeColor)
            e.Graphics.DrawString(Text, Font, textBrush, New RectangleF(0, 0, Width, Height), sf)
        End Using
    End Sub

End Class