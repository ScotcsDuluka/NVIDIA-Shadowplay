#If DEBUG Then
Public Class Debug_UI



    Private _lastX As Integer = Integer.MinValue
    Private _lastY As Integer = Integer.MinValue
    Private _lastW As Integer = Integer.MinValue
    Private _lastH As Integer = Integer.MinValue

    ' ★ Messages
    Private Const WM_MOVING As Integer = &H216
    Private Const WM_MOVE As Integer = &H3
    Private Const WM_SIZE As Integer = &H5

    ' ★ Timer
    Private WithEvents tmr As New Timer With {.Interval = 1}

    Public Sub New()
        InitializeComponent()
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.DoubleBuffered = True

        AddHandler Me.LocationChanged, AddressOf OnChanged
        AddHandler Me.ResizeEnd, AddressOf OnChanged

        tmr.Start()
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case WM_MOVING, WM_MOVE, WM_SIZE
                MyBase.WndProc(m)
                DoSync() ' ★ Sync!
                Return
        End Select
        MyBase.WndProc(m)
    End Sub

    Private Sub OnChanged(sender As Object, e As EventArgs)
        DoSync() ' ★ Sync!
    End Sub

    Private Sub tmr_Tick(sender As Object, e As EventArgs) Handles tmr.Tick
        DoSync() ' ★ Sync every tick!
    End Sub

    Public Sub DoSync()

        Dim pt As Point = Me.PointToScreen(Point.Empty)
        Dim x As Integer = pt.X : Dim y As Integer = pt.Y
        Dim w As Integer = Me.ClientSize.Width : Dim h As Integer = Me.ClientSize.Height


        For Each frm In Application.OpenForms
            If frm.Name <> Me.Name AndAlso Not frm.IsDisposed AndAlso frm.Visible Then

                If frm.Name = "sha1" OrElse frm.Name = "sha2" OrElse frm.Name = "sha3" OrElse frm.Name = "sha4" Then
                    Continue For
                End If

                If frm.WindowState <> FormWindowState.Normal Then
                    frm.WindowState = FormWindowState.Normal
                End If

                frm.SetBounds(x, y, w, h)

            End If
        Next

    End Sub

    Private Sub Debug_UI_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Application.Exit()
    End Sub
End Class
#End If