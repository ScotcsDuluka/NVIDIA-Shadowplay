Partial Public Class Base

#Region "Animation Engine"

    Private animStart As DateTime
    Private animDuration As Double
    Private startX As Integer
    Private targetX As Integer
    Private startY As Integer
    Private targetY As Integer
    Private currentPanel As Control
    Private animationRunning As Boolean = False
    Private onComplete As Action
    Private isSlideX As Boolean

    Public Sub ANX(panel As Control,
                   fromX As Integer,
                   toX As Integer,
                   duration As Double,
                   Optional completed As Action = Nothing)

        currentPanel = panel
        startX = fromX
        targetX = toX
        animDuration = duration
        onComplete = completed
        isSlideX = True

        panel.Left = fromX
        animStart = DateTime.Now
        animationRunning = True

        Animation_Engine.Start()
    End Sub

    Public Sub AMY(panel As Control,
                   fromY As Integer,
                   toY As Integer,
                   duration As Double,
                   Optional completed As Action = Nothing)

        currentPanel = panel
        startY = fromY
        targetY = toY
        animDuration = duration
        onComplete = completed
        isSlideX = False

        panel.Top = fromY
        animStart = DateTime.Now
        animationRunning = True

        Animation_Engine.Start()
    End Sub

    Private Sub Animation_Engine_Tick(sender As Object, e As EventArgs) Handles Animation_Engine.Tick

        If Not animationRunning Then Return

        Dim elapsed = (DateTime.Now - animStart).TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        If t >= 1 Then
            t = 1
            animationRunning = False
            Animation_Engine.Stop()

            If onComplete IsNot Nothing Then
                onComplete.Invoke()
                onComplete = Nothing
            End If
        End If

        Dim eased As Double = 1 - Math.Pow(1 - t, 3)

        If isSlideX Then
            Dim newX As Integer = startX + (targetX - startX) * eased
            currentPanel.Left = newX
        Else
            Dim newY As Integer = startY + (targetY - startY) * eased
            currentPanel.Top = newY
        End If

    End Sub

#End Region

End Class