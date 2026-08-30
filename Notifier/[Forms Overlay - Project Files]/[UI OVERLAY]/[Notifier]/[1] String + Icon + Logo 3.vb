Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.AxHost
Imports Microsoft.VisualBasic.Logging

Public Class Notifier_Sub3
    Inherits Form

    ' ===== Toast slot geometry (OWNER spec) =====
    ' Must match the sibling Notifier3 unit: slot 3 = two pitches below slot 1.
    Private Const SlotIndex As Integer = 3
    Private Const SlotGapPx As Integer = 10

    Private ReadOnly Property SlotOffsetY As Integer
        Get
            Return (SlotIndex - 1) * (Me.Height + SlotGapPx)
        End Get
    End Property

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

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ShadowForm As Shadow3

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ParentNotifier As Notifier3

    Private fadeTimer As New Timer()
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

    Private Sub Notifier_Sub_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Debug.WriteLine("[Notifier_Sub3] ===== Form Load =====")

        HideFromAltTab()
        ' T30.3 OWNER: born ANCHORED on the parked card - this form is only
        ' ever shown AFTER the card's slide-in completed (never mid-slide).
        ' It is a still window for its whole life: the BG engine carries it
        ' in Y (ApplyUnitY, same tick as the card + shadow) and NOTHING
        ' ever animates it in X.
        Me.Location = New Point(Notifier3.Left + Notifier3.Notifier_black.Left, Notifier3.Location.Y)

        Me.Opacity = 0
        Me.Show()

        fadeTimer.Interval = 10
        AddHandler fadeTimer.Tick, AddressOf FadeIn
        fadeTimer.Start()

        Debug.WriteLine("[Notifier_Sub3] Fade in started")
    End Sub

    Private Sub FadeIn(sender As Object, e As EventArgs)
        If Me.Opacity < 1 Then
            Me.Opacity += 0.05
            Exit Sub
        End If

        Me.Opacity = 1

        ' T30.1: the shadow is NOT the rider's business anymore - the unit's
        ' Background form fades it in when the slide-in completes.
        fadeTimer.Stop()
        Debug.WriteLine("[Notifier_Sub3] Fade complete")
    End Sub

    Private Sub text_n_Click_1(sender As Object, e As EventArgs) Handles MyBase.Click, text_n.Click, Notifier_black.Click, PictureBox1.Click, icon_n.Click
        Debug.WriteLine("[Notifier_Sub3] Click → DoCloseClick")
        Notifier3.DoCloseClick()
    End Sub

    ' T30.3 OWNER: the rider NEVER moves itself. On the Y axis the BG form
    ' is the single clock - its engine (ApplyUnitY) carries the card, this
    ' rider and the shadow in the SAME tick, and nothing ever animates the
    ' rider on X. The old self-slide engine is deleted; this handler only
    ' drains a stray external .Start() so it can never fight the clock.
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Stop()
    End Sub

    Private Sub Notifier_Sub_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Debug.WriteLine("[Notifier_Sub3] FormClosing — cleanup")
        fadeTimer.Stop()
        fadeTimer.Dispose()
        Timer1.Stop()
    End Sub
End Class
