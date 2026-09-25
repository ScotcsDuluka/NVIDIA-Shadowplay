Imports System.Runtime.InteropServices

Partial Public Class Base

#Region "Animation Engine - MM Timer"

    Private Enum AnimMode
        SlideX
        SlideY
        ResizeW
        ResizeH
    End Enum

    Private Class AnimItem
        Public Panel As Control
        Public StartVal As Integer
        Public TargetVal As Integer
        Public Mode As AnimMode
    End Class

    Private animStart As DateTime
    Private animDuration As Double
    Private startX As Integer
    Private targetX As Integer
    Private startY As Integer
    Private targetY As Integer
    Private startW As Integer
    Private targetW As Integer
    Private startH As Integer
    Private targetH As Integer
    Private currentPanel As Control
    Private animationRunning As Boolean = False
    Private onComplete As Action
    Private animType As AnimMode
    Private anims As New List(Of AnimItem)
    Private isMulti As Boolean = False
    Private animGeneration As Integer = 0
    Private timePeriodCount As Integer = 0

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
    Private sw As Stopwatch
    Private _invokePending As Boolean = False

    Private Sub Animation_Engine_Start()
        If mmTimerId <> 0 Then
            timeKillEvent(mmTimerId)
            mmTimerId = 0
        End If

        If timePeriodCount = 0 Then
            timeBeginPeriod(1)
        End If
        timePeriodCount += 1

        mmCallback = New MMTimerProc(AddressOf OnMMTick)
        mmTimerId = timeSetEvent(16, 1, mmCallback, UIntPtr.Zero,
                                  TIME_PERIODIC Or TIME_KILL_SYNCHRONOUS)
    End Sub

    Private Sub Animation_Engine_Stop()
        animGeneration += 1

        If mmTimerId <> 0 Then
            timeKillEvent(mmTimerId)
            mmTimerId = 0
        End If

        timePeriodCount -= 1
        If timePeriodCount < 0 Then timePeriodCount = 0
        If timePeriodCount = 0 Then
            timeEndPeriod(1)
        End If

        _invokePending = False

        If sw IsNot Nothing Then sw.Stop()
    End Sub

    Private Sub OnMMTick(uID As UInteger, uMsg As UInteger,
                          dwUser As UIntPtr, dw1 As UInteger, dw2 As UInteger)

        If Me.IsDisposed OrElse Me.Disposing Then Return
        If _invokePending Then Return
        _invokePending = True

        Dim gen As Integer = animGeneration

        Try
            Me.BeginInvoke(Sub()
                               If gen <> animGeneration Then
                                   _invokePending = False
                                   Return
                               End If
                               Try
                                   _invokePending = False
                                   Animation_Engine_Tick()
                               Catch ex As Exception
                                   _invokePending = False
                               End Try
                           End Sub)
        Catch ex As ObjectDisposedException
            _invokePending = False
        Catch ex As InvalidOperationException
            _invokePending = False
        End Try
    End Sub

    Public Sub ANH_Group(panels As Control(),
                     fromVals As Integer(),
                     toVals As Integer(),
                     durationMs As Double,
                     Optional completed As Action = Nothing)

        If panels Is Nothing OrElse panels.Length = 0 Then Return
        If fromVals Is Nothing OrElse toVals Is Nothing Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        anims.Clear()
        For i As Integer = 0 To panels.Length - 1
            If i < fromVals.Length AndAlso i < toVals.Length Then
                If panels(i) IsNot Nothing Then
                    anims.Add(New AnimItem With {.Panel = panels(i), .StartVal = fromVals(i), .TargetVal = toVals(i), .Mode = AnimMode.ResizeH})
                    panels(i).Height = fromVals(i)
                End If
            End If
        Next

        animDuration = durationMs
        onComplete = completed
        isMulti = True
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub
    Public Sub ANX(panel As Control,
                   fromX As Integer,
                   toX As Integer,
                   durationMs As Double,
                   Optional completed As Action = Nothing)

        If panel Is Nothing Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        currentPanel = panel
        startX = fromX
        targetX = toX
        animDuration = durationMs
        onComplete = completed
        animType = AnimMode.SlideX
        isMulti = False
        anims.Clear()

        panel.Left = fromX
        animStart = DateTime.Now
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub AMY(panel As Control,
                   fromY As Integer,
                   toY As Integer,
                   durationMs As Double,
                   Optional completed As Action = Nothing)

        If panel Is Nothing Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        currentPanel = panel
        startY = fromY
        targetY = toY
        animDuration = durationMs
        onComplete = completed
        animType = AnimMode.SlideY
        isMulti = False
        anims.Clear()

        panel.Top = fromY
        animStart = DateTime.Now
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub ANW(panel As Control,
                   fromW As Integer,
                   toW As Integer,
                   durationMs As Double,
                   Optional completed As Action = Nothing)

        If panel Is Nothing Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        currentPanel = panel
        startW = fromW
        targetW = toW
        animDuration = durationMs
        onComplete = completed
        animType = AnimMode.ResizeW
        isMulti = False
        anims.Clear()

        panel.Width = fromW
        animStart = DateTime.Now
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub ANH(panel As Control,
                   fromH As Integer,
                   toH As Integer,
                   durationMs As Double,
                   Optional completed As Action = Nothing)

        If panel Is Nothing Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        currentPanel = panel
        startH = fromH
        targetH = toH
        animDuration = durationMs
        onComplete = completed
        animType = AnimMode.ResizeH
        isMulti = False
        anims.Clear()

        panel.Height = fromH
        animStart = DateTime.Now
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub ANX_Multi(panels As Control(),
                         fromX As Integer,
                         toX As Integer,
                         durationMs As Double,
                         Optional completed As Action = Nothing)
        If panels Is Nothing OrElse panels.Length = 0 Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        anims.Clear()
        For Each p As Control In panels
            If p IsNot Nothing Then
                anims.Add(New AnimItem With {.Panel = p, .StartVal = fromX, .TargetVal = toX, .Mode = AnimMode.SlideX})
                p.Left = fromX
            End If
        Next

        animDuration = durationMs
        onComplete = completed
        isMulti = True
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub AMY_Multi(panels As Control(),
                         fromY As Integer,
                         toY As Integer,
                         durationMs As Double,
                         Optional completed As Action = Nothing)
        If panels Is Nothing OrElse panels.Length = 0 Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        anims.Clear()
        For Each p As Control In panels
            If p IsNot Nothing Then
                anims.Add(New AnimItem With {.Panel = p, .StartVal = fromY, .TargetVal = toY, .Mode = AnimMode.SlideY})
                p.Top = fromY
            End If
        Next

        animDuration = durationMs
        onComplete = completed
        isMulti = True
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub ANW_Multi(panels As Control(),
                         fromW As Integer,
                         toW As Integer,
                         durationMs As Double,
                         Optional completed As Action = Nothing)
        If panels Is Nothing OrElse panels.Length = 0 Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        anims.Clear()
        For Each p As Control In panels
            If p IsNot Nothing Then
                anims.Add(New AnimItem With {.Panel = p, .StartVal = fromW, .TargetVal = toW, .Mode = AnimMode.ResizeW})
                p.Width = fromW
            End If
        Next

        animDuration = durationMs
        onComplete = completed
        isMulti = True
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Public Sub ANH_Multi(panels As Control(),
                         fromH As Integer,
                         toH As Integer,
                         durationMs As Double,
                         Optional completed As Action = Nothing)
        If panels Is Nothing OrElse panels.Length = 0 Then Return

        If animationRunning Then
            Animation_Engine_Stop()
            animationRunning = False
        End If

        anims.Clear()
        For Each p As Control In panels
            If p IsNot Nothing Then
                anims.Add(New AnimItem With {.Panel = p, .StartVal = fromH, .TargetVal = toH, .Mode = AnimMode.ResizeH})
                p.Height = fromH
            End If
        Next

        animDuration = durationMs
        onComplete = completed
        isMulti = True
        sw = Stopwatch.StartNew()
        animationRunning = True

        Animation_Engine_Start()
    End Sub

    Private Sub Animation_Engine_Tick()
        If Not animationRunning Then Return

        If isMulti Then
            Animation_Engine_Tick_Multi()
        Else
            Animation_Engine_Tick_Single()
        End If
    End Sub

    Private Sub Animation_Engine_Tick_Single()
        If currentPanel Is Nothing OrElse currentPanel.IsDisposed Then
            animationRunning = False
            Animation_Engine_Stop()
            Return
        End If

        Dim elapsed As Double = sw.Elapsed.TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        If t >= 1.0 Then
            t = 1.0
            animationRunning = False
            Animation_Engine_Stop()

            Select Case animType
                Case AnimMode.SlideX : currentPanel.Left = targetX
                Case AnimMode.SlideY : currentPanel.Top = targetY
                Case AnimMode.ResizeW : currentPanel.Width = targetW
                Case AnimMode.ResizeH : currentPanel.Height = targetH
            End Select

            Dim callback As Action = onComplete
            onComplete = Nothing

            If callback IsNot Nothing Then
                Try
                    callback.Invoke()
                Catch ex As Exception
                    Debug.WriteLine("[Anim] Callback Error: " & ex.Message)
                End Try
            End If

            Return
        End If

        Dim eased As Double = 1.0 - Math.Pow(1.0 - t, 3)

        Select Case animType
            Case AnimMode.SlideX
                currentPanel.Left = CInt(startX + (targetX - startX) * eased)
            Case AnimMode.SlideY
                currentPanel.Top = CInt(startY + (targetY - startY) * eased)
            Case AnimMode.ResizeW
                currentPanel.Width = CInt(startW + (targetW - startW) * eased)
            Case AnimMode.ResizeH
                currentPanel.Height = CInt(startH + (targetH - startH) * eased)
        End Select
    End Sub

    Private Sub Animation_Engine_Tick_Multi()
        If anims.Count = 0 Then
            animationRunning = False
            Animation_Engine_Stop()
            Return
        End If

        Dim elapsed As Double = sw.Elapsed.TotalMilliseconds
        Dim t As Double = elapsed / animDuration
        If t >= 1.0 Then t = 1.0

        Dim eased As Double = 1.0 - Math.Pow(1.0 - t, 3)
        Dim done As New List(Of AnimItem)

        For Each anim As AnimItem In anims
            If anim.Panel Is Nothing OrElse anim.Panel.IsDisposed Then
                done.Add(anim)
                Continue For
            End If

            Dim value As Integer = CInt(anim.StartVal + (anim.TargetVal - anim.StartVal) * eased)

            Select Case anim.Mode
                Case AnimMode.SlideX : anim.Panel.Left = value
                Case AnimMode.SlideY : anim.Panel.Top = value
                Case AnimMode.ResizeW : anim.Panel.Width = value
                Case AnimMode.ResizeH : anim.Panel.Height = value
            End Select

            If t >= 1.0 Then done.Add(anim)
        Next

        For Each c As AnimItem In done
            anims.Remove(c)
        Next

        If anims.Count = 0 Then
            animationRunning = False
            Animation_Engine_Stop()

            Dim callback As Action = onComplete
            onComplete = Nothing
            If callback IsNot Nothing Then
                Try
                    callback.Invoke()
                Catch ex As Exception
                    Debug.WriteLine("[Anim] Callback Error: " & ex.Message)
                End Try
            End If
        End If
    End Sub

#End Region

End Class