Imports System.Drawing.Drawing2D
Imports System.ComponentModel

Public Class ShadowPanel
    Inherits Panel

    Private _shadowSize As Integer = 5
    Private _shadowColor As Color = Color.FromArgb(100, 0, 0, 0) 'สีดำ โปร่งแสง 100

    <Category("Appearance")>
    Public Property ShadowSize As Integer
        Get
            Return _shadowSize
        End Get
        Set(value As Integer)
            _shadowSize = value
            Me.Invalidate() 'สั่งให้วาดใหม่
        End Set
    End Property

    <Category("Appearance")>
    Public Property ShadowColor As Color
        Get
            Return _shadowColor
        End Get
        Set(value As Color)
            _shadowColor = value
            Me.Invalidate()
        End Set
    End Property

    Public Sub New()
        ' ตั้งค่าให้ Panel วาดรูปด้วยตัวเองเพื่อป้องกันหน้าจอกระพริบ
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or
                    ControlStyles.UserPaint Or
                    ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        ' 1. วาดเงา (เลื่อนตำแหน่งลงมาด้านล่างขวาตามค่า ShadowSize)
        Using shadowBrush As New SolidBrush(_shadowColor)
            e.Graphics.FillRectangle(shadowBrush, _shadowSize, _shadowSize, Me.Width - _shadowSize, Me.Height - _shadowSize)
        End Using

        ' 2. วาดพื้นหลังหลักของ Panel ทับลงไป (ให้ขนาดเล็กกว่าเงา)
        Using bgBrush As New SolidBrush(Me.BackColor)
            e.Graphics.FillRectangle(bgBrush, 0, 0, Me.Width - _shadowSize, Me.Height - _shadowSize)
        End Using

        ' 3. ถ้ามี Border ให้วาด Border ด้วย (จะได้ไม่ถูกเงาทับ)
        If Me.BorderStyle <> BorderStyle.None Then
            ControlPaint.DrawBorder(e.Graphics, New Rectangle(0, 0, Me.Width - _shadowSize, Me.Height - _shadowSize), Me.ForeColor, ButtonBorderStyle.Solid)
        End If

        MyBase.OnPaint(e)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Me.Invalidate()
    End Sub
End Class