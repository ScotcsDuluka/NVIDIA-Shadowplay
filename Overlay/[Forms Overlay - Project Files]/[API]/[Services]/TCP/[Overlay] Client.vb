Public Class Base

    ''' <summary>Shared TCP helper — accessible from all forms (Sub_Record, Video Capture, etc.)</summary>
    Public Shared tcp As TcpClientHelper

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tcp = New TcpClientHelper("NVIDIA Overlay")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage
    End Sub

    ' Dispose TCP on form close
    Private Sub Base_TestFormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If tcp IsNot Nothing Then
                tcp.Disconnect()
                tcp.Dispose()
            End If
        Catch
        End Try

    End Sub

    Public Sub OnMessage(msg As String)
        If InvokeRequired Then
            Invoke(Sub() OnMessage(msg))
            Return
        End If

        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        If parts.Length < 2 Then Exit Sub

        Dim data = parts(1)

        Dim colonIndex = data.IndexOf(":"c)
        Dim cmd, value As String
        If colonIndex >= 0 Then
            cmd = data.Substring(0, colonIndex)
            value = data.Substring(colonIndex + 1)
        Else
            cmd = data
            value = ""
        End If

        Select Case cmd

            Case "open_overlay"
                tcp.SendLog(cmd)
                If Settings_List.Visible Then Return
                If Base_Gallery.Visible Then Return

                isFunctionActive_f3 = False

                If shadowplay.Visible = True Then
                    HideAllControls()
                    shadowplay.Visible = False
                Else
                    ShowMainPanel()
                    shadowplay.Visible = True
                    Base_Game_Filter_Sub.Opacity = 0
                    Base_Game_Filter.Opacity = 0
                    Base_Game_Filter.Hide()
                    Base_Game_Filter_Sub.Hide()
                End If

            Case "engine_ready"
                ' ✅ P1.6: Engine just connected to the Hub (or reconnected).
                ' Re-send PREWARM_FFMPEG so Engine picks up our ffmpeg.exe path.
                ' Problem: Overlay sends PREWARM on its Load event, but Engine
                ' often connects LATER → the original PREWARM is lost. Engine
                ' then can't find ffmpeg → RECORD_START fails. By re-sending
                ' PREWARM here, we ensure Engine always gets the path.
                Debug.WriteLine("[Overlay] received engine_ready, re-sending PREWARM_FFMPEG")
                Try
                    Dim ffmpegPath As String = AppSettings.Instance.Paths.FFmpegPath
                    If Not String.IsNullOrEmpty(ffmpegPath) Then
                        Dim encoderName As String = AppSettings.Instance.Recording.Encoder
                        tcp.Send("PREWARM_FFMPEG", ffmpegPath & "|" & encoderName)
                    End If
                Catch ex As Exception
                    Debug.WriteLine("[Overlay] engine_ready re-send failed: " & ex.Message)
                End Try

            Case "engine_response"
                ' ✅ P2.6: parse engine_response and reconcile UI state.
                ' Format: engine_response:<command>,<status>[,<data>][,req=<reqId>]
                Debug.WriteLine($"[Overlay] engine_response: {value}")
                HandleEngineResponse(value)

            Case Else
                Debug.WriteLine("Unknown: " & cmd)

        End Select
    End Sub

    ''' <summary>
    ''' ✅ P2.6: parse engine_response and reconcile Overlay's optimistic UI state.
    ''' Format: &lt;command&gt;,&lt;status&gt;[,&lt;data&gt;][,req=&lt;reqId&gt;]
    ''' If Engine reports an error for record_start, clear _isRecordingLocal so
    ''' the Overlay doesn't show "Recording" when nothing is actually recording.
    ''' </summary>
    Private Sub HandleEngineResponse(value As String)
        Try
            If String.IsNullOrEmpty(value) Then Return
            Dim parts As String() = value.Split(","c)
            If parts.Length < 2 Then Return

            Dim cmd As String = parts(0).Trim()
            Dim status As String = parts(1).Trim()

            Select Case cmd
                Case "engine_record_start"
                    If status = "ok" Then
                        Debug.WriteLine($"[Overlay] Engine confirmed record_start OK")
                    Else
                        ' Engine failed to start recording — revert optimistic UI.
                        Debug.WriteLine($"[Overlay] Engine record_start FAILED: {status} {If(parts.Length >= 3, parts(2), "")}")
                        _isRecordingLocal = False
                        RecordValue = False
                        ShowNotifier("recording_error")
                    End If

                Case "engine_record_stop"
                    If status = "ok" Then
                        Debug.WriteLine($"[Overlay] Engine confirmed record_stop OK")
                    Else
                        Debug.WriteLine($"[Overlay] Engine record_stop FAILED: {status}")
                    End If

                Case "engine_get_status"
                    ' parts(2) = "Recording" or "Idle"
                    If parts.Length >= 3 Then
                        Dim engineState As String = parts(2).Trim()
                        Debug.WriteLine($"[Overlay] Engine status: {engineState}")
                        ' Reconcile local state with Engine's actual state.
                        If engineState = "Recording" AndAlso Not _isRecordingLocal Then
                            _isRecordingLocal = True
                            RecordValue = True
                        ElseIf engineState <> "Recording" AndAlso _isRecordingLocal Then
                            _isRecordingLocal = False
                            RecordValue = False
                        End If
                    End If
            End Select
        Catch ex As Exception
            Debug.WriteLine($"[Overlay] HandleEngineResponse error: {ex.Message}")
        End Try
    End Sub
End Class
