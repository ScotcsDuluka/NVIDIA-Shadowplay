Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Diagnostics

<DefaultEvent("ValueChanged")>
Public Class ToggleSwitch
    Inherits Control

#Region "============================================================================ NATIVE API (สำหรับบังคับ 60 FPS)"

    ' บังคับให้ Windows Timer ทำงานทุก 1ms แทนที่จะเป็น 15.6ms
    <DllImport("winmm.dll")>
    Private Shared Function timeBeginPeriod(uPeriod As UInteger) As UInteger
    End Function

    ' โหลดครั้งเดียวตอนเปิดโปรแกรม
    Shared Sub New()
        timeBeginPeriod(1)
    End Sub

#End Region

#Region "============================================================================ FIELDS"

    Private _isOn As Boolean = False
    Private _togglePosition As Single = 0F
    Private _isHovered As Boolean = False
    Private _isPressed As Boolean = False

    Private _onColor As Color = Color.FromArgb(118, 185, 0)
    Private _offColor As Color = Color.FromArgb(60, 63, 67)
    Private _thumbColor As Color = Color.FromArgb(245, 245, 245)

    Private _cornerRadius As Integer = 40
    Private _thumbMargin As Integer = 3
    Private _showGlow As Boolean = False

    ' Animation Tools
    Private WithEvents _animTimer As New Timer With {.Interval = 1}
    Private _stopwatch As New Stopwatch()
    Private _animDuration As Integer = 180

#End Region

#Region "============================================================================ PROPERTIES"

    <Category("Behavior")>
    Public Property IsOn As Boolean
        Get
            Return _isOn
        End Get
        Set(value As Boolean)
            If _isOn <> value Then
                _isOn = value
                _stopwatch.Restart()
                _animTimer.Start()
                RaiseEvent ValueChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    <Category("Appearance")>
    Public Property OnColor As Color
        Get
            Return _onColor
        End Get
        Set(value As Color)
            _onColor = value
            Invalidate()
        End Set
    End Property

    <Category("Appearance")>
    Public Property OffColor As Color
        Get
            Return _offColor
        End Get
        Set(value As Color)
            _offColor = value
            Invalidate()
        End Set
    End Property

    <Category("Appearance")>
    Public Property ShowGlow As Boolean
        Get
            Return _showGlow
        End Get
        Set(value As Boolean)
            _showGlow = value
            Invalidate()
        End Set
    End Property

#End Region

#Region "============================================================================ EVENTS"

    Public Event ValueChanged As EventHandler

#End Region

#Region "============================================================================ CONSTRUCTOR"

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        Size = New Size(50, 28)
    End Sub

#End Region

#Region "============================================================================ ANIMATION (Time-based)"

    Private Sub _animTimer_Tick(sender As Object, e As EventArgs) Handles _animTimer.Tick
        ' คำนวณว่าหลุดจากเวลาเริ่มต้นมากี่มิลลิวินาทีแล้ว (0.0 - 1.0)
        Dim elapsed As Double = _stopwatch.Elapsed.TotalMilliseconds / _animDuration

        If elapsed >= 1.0 Then
            elapsed = 1.0
            _animTimer.Stop()
            _stopwatch.Stop()
        End If

        ' สูตร Ease Out Cubic: ทำให้ตัวเลขเริ่มจากเร็วแล้วค่อยๆ ช้าลงอย่างลื่นไหลมาก
        Dim easedProgress As Double = 1.0 - Math.Pow(1.0 - elapsed, 3)

        ' ถ้าเปิด ให้เพิ่มจาก 0 -> 1, ถ้าปิด ให้ลดจาก 1 -> 0
        If _isOn Then
            _togglePosition = CSng(easedProgress)
        Else
            _togglePosition = CSng(1.0 - easedProgress)
        End If

        Invalidate()
    End Sub

#End Region

#Region "============================================================================ DRAWING"

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality

        DrawGlow(e.Graphics)
        DrawTrack(e.Graphics)
        DrawThumb(e.Graphics)
    End Sub

    Private Sub DrawGlow(g As Graphics)
        If Not _showGlow OrElse _togglePosition < 0.05F Then Return

        Dim alpha As Integer = CInt(_togglePosition * 40)
        Dim glowColor As Color = Color.FromArgb(alpha, _onColor)
        Dim glowRect As New Rectangle(-4, -4, Width + 8, Height + 8)

        Using path As New GraphicsPath()
            GraphicsExtensions.AddRoundedRectangle(path, glowRect, _cornerRadius + 4)
            Using brush As New SolidBrush(glowColor)
                g.FillPath(brush, path)
            End Using
        End Using
    End Sub

    Private Sub DrawTrack(g As Graphics)
        Dim trackRect As New Rectangle(0, 0, Width, Height)

        Using path As New GraphicsPath()
            GraphicsExtensions.AddRoundedRectangle(path, trackRect, _cornerRadius)

            Dim baseColor As Color = _offColor
            If _isHovered OrElse _isPressed Then baseColor = LightenColor(baseColor, 15)

            Using bgBrush As New SolidBrush(baseColor)
                g.FillPath(bgBrush, path)
            End Using

            If _togglePosition > 0.01F Then
                Dim alpha As Integer = CInt(_togglePosition * 255)
                Dim overlayColor As Color = _onColor
                If _isHovered OrElse _isPressed Then overlayColor = LightenColor(overlayColor, 15)

                Using onBrush As New SolidBrush(Color.FromArgb(alpha, overlayColor))
                    g.FillPath(onBrush, path)
                End Using
            End If

            Using pen As New Pen(Color.FromArgb(30, 255, 255, 255), 1.0F)
                g.DrawPath(pen, path)
            End Using
        End Using
    End Sub

    Private Function LightenColor(baseColor As Color, amount As Integer) As Color
        Dim r As Integer = baseColor.R + amount
        Dim g As Integer = baseColor.G + amount
        Dim b As Integer = baseColor.B + amount

        If r > 255 Then r = 255
        If g > 255 Then g = 255
        If b > 255 Then b = 255

        Return Color.FromArgb(r, g, b)
    End Function

    Private Sub DrawThumb(g As Graphics)
        Dim thumbSize As Integer = Height - (_thumbMargin * 2)
        Dim maxSlide As Single = Width - thumbSize - (_thumbMargin * 2)

        If maxSlide < 0 Then maxSlide = 0

        Dim x As Single = _thumbMargin + (_togglePosition * maxSlide)
        Dim thumbRect As New RectangleF(x, _thumbMargin, thumbSize, thumbSize)

        Using shadowPath As New GraphicsPath()
            shadowPath.AddEllipse(New RectangleF(x + 0.5F, _thumbMargin + 1.5F, thumbSize, thumbSize))
            Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
                g.FillPath(shadowBrush, shadowPath)
            End Using
        End Using

        Using thumbPath As New GraphicsPath()
            thumbPath.AddEllipse(thumbRect)

            Dim pt1 As New PointF(x, _thumbMargin)
            Dim pt2 As New PointF(x, _thumbMargin + thumbSize)
            Using gradBrush As New LinearGradientBrush(pt1, pt2, Color.FromArgb(250, 250, 250), Color.FromArgb(215, 215, 215))
                g.FillPath(gradBrush, thumbPath)
            End Using

            Using highlightPath As New GraphicsPath()
                highlightPath.AddEllipse(New RectangleF(x + 3, _thumbMargin + 2, thumbSize - 6, thumbSize * 0.35F))
                Dim hp1 As New PointF(x, _thumbMargin)
                Dim hp2 As New PointF(x, _thumbMargin + thumbSize * 0.45F)
                Using highlightBrush As New LinearGradientBrush(hp1, hp2, Color.FromArgb(100, 255, 255, 255), Color.Transparent)
                    g.FillPath(highlightBrush, highlightPath)
                End Using
            End Using

            Using borderPen As New Pen(Color.FromArgb(50, 50, 50), 0.5F)
                'g.DrawPath(borderPen, thumbPath)
            End Using
        End Using
    End Sub

#End Region

#Region "============================================================================ INTERACTION"

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If Not _isHovered Then
            _isHovered = True
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _isHovered = False
        _isPressed = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = True
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            _isPressed = False
            IsOn = Not IsOn
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
    End Sub

#End Region

End Class

' ================================================================================
' EXTENSION MODULE
' ================================================================================
Public Module GraphicsExtensions

    <System.Runtime.CompilerServices.Extension>
    Public Sub AddRoundedRectangle(path As GraphicsPath, rect As Rectangle, radius As Integer)
        Dim maxRadius As Integer = Math.Min(rect.Width \ 2, rect.Height \ 2)
        radius = Math.Min(radius, maxRadius)

        If radius < 1 Then
            path.AddRectangle(rect)
            Return
        End If

        Dim d As Single = radius * 2
        path.AddArc(rect.X, rect.Y, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
    End Sub

End Module