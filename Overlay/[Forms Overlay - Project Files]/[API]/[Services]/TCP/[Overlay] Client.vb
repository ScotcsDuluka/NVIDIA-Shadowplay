Public Class Base

    Public Shared tcp As TcpClientHelper

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        tcp = New TcpClientHelper("NVIDIA Overlay")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage

        tcp.ConnectAsync()

        InitReplayHonesty()

        EngineProcessSupervisor.EnsureEngineRunning()
    End Sub

    Private Sub Base_TestFormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            
            EngineProcessSupervisor.Shutdown()

            If tcp IsNot Nothing Then
                tcp.Disconnect()
                tcp.Dispose()
            End If
        Catch
        End Try

    End Sub

    Public Sub OnMessage(msg As String)
        If InvokeRequired Then

            If Not IsHandleCreated Then Return
            Try
                BeginInvoke(Sub() OnMessage(msg))
            Catch
                
            End Try
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

                _isRecordingLocal = False
                RecordValue = False
                Try
                    tcp.Send("engine_get_status")
                    Debug.WriteLine("[Overlay] engine_ready → pulled engine_get_status")
                Catch ex As Exception
                    Debug.WriteLine("[Overlay] engine_get_status pull failed: " & ex.Message)
                End Try

            Case "engine_response"

                Debug.WriteLine($"[Overlay] engine_response: {value}")
                HandleEngineResponse(value)

            Case "engine_state_changed"

                Debug.WriteLine($"[Overlay] engine_state_changed: {value}")
                HandleEngineStateChanged(value)

            Case "engine_recording_progress"

                Debug.WriteLine($"[Overlay] engine_recording_progress: {value}")
                HandleEngineProgress(value)

            Case "engine_recording_saved"

                Debug.WriteLine($"[Overlay] engine_recording_saved: {value}")
                HandleEngineRecordingSaved(value)

            Case "engine_recording_error"
                
                Debug.WriteLine($"[Overlay] engine_recording_error: {value}")
                HandleEngineRecordingError(value)

            Case Else
                Debug.WriteLine("Unknown: " & cmd)

        End Select
    End Sub

    Private Sub HandleEngineStateChanged(stateName As String)
        Try
            Select Case stateName
                Case "Recording"
                    If Not _isRecordingLocal Then
                        _isRecordingLocal = True
                        RecordValue = True
                        Debug.WriteLine("[Overlay] reconcile: state=Recording → _isRecordingLocal=True")
                    End If
                Case "Idle", "Stopping", "HasError"
                    If _isRecordingLocal AndAlso stateName <> "Stopping" Then
                        _isRecordingLocal = False
                        RecordValue = False
                        Debug.WriteLine($"[Overlay] reconcile: state={stateName} → _isRecordingLocal=False")
                    End If
            End Select
        Catch ex As Exception
            Debug.WriteLine($"[Overlay] HandleEngineStateChanged error: {ex.Message}")
        End Try
    End Sub

    Private Sub HandleEngineProgress(value As String)
        Try
            Dim parts As String() = value.Split("|"c)
            If parts.Length < 3 Then Return

            Dim sec As Integer
            Dim frames As Long
            Dim sizeBytes As Long
            If Not Integer.TryParse(parts(0), sec) Then Return
            If Not Long.TryParse(parts(1), frames) Then Return
            If Not Long.TryParse(parts(2), sizeBytes) Then Return

            Dim sizeStr As String
            If sizeBytes >= 1024 * 1024 * 1024 Then
                sizeStr = (sizeBytes / (1024.0 * 1024 * 1024)).ToString("F2") & " GB"
            ElseIf sizeBytes >= 1024 * 1024 Then
                sizeStr = (sizeBytes / (1024.0 * 1024)).ToString("F1") & " MB"
            ElseIf sizeBytes >= 1024 Then
                sizeStr = (sizeBytes / 1024.0).ToString("F0") & " KB"
            Else
                sizeStr = sizeBytes & " B"
            End If

            If RecordValue AndAlso Record_Stats IsNot Nothing Then
                Record_Stats.Text = TimeSpan.FromSeconds(sec).ToString("hh\:mm\:ss") & " - " & sizeStr
            End If

            Debug.WriteLine($"[Overlay] progress: {sec}s, {frames} frames, {sizeStr}")
        Catch ex As Exception
            Debug.WriteLine($"[Overlay] HandleEngineProgress error: {ex.Message}")
        End Try
    End Sub

    Private Sub HandleEngineRecordingSaved(filePath As String)
        Try
            _isRecordingLocal = False
            RecordValue = False

            ShowNotifier("recording_saved")
            Debug.WriteLine($"[Overlay] recording saved: {filePath}")
        Catch ex As Exception
            Debug.WriteLine($"[Overlay] HandleEngineRecordingSaved error: {ex.Message}")
        End Try
    End Sub

    Private Sub HandleEngineRecordingError(message As String)
        Try
            _isRecordingLocal = False
            RecordValue = False
            ShowNotifier("recording_error")
            Debug.WriteLine($"[Overlay] recording error: {message}")
        Catch ex As Exception
            Debug.WriteLine($"[Overlay] HandleEngineRecordingError error: {ex.Message}")
        End Try
    End Sub

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

                        ShowNotifier("recording_started")
                        Debug.WriteLine($"[Overlay] Engine confirmed record_start OK")
                    Else
                        
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
                        ShowNotifier("recording_error")
                    End If

                Case "engine_get_status"
                    
                    If parts.Length >= 3 Then
                        Dim engineState As String = parts(2).Trim()
                        Debug.WriteLine($"[Overlay] Engine status: {engineState}")
                        
                        If engineState = "Recording" AndAlso Not _isRecordingLocal Then
                            _isRecordingLocal = True
                            RecordValue = True
                        ElseIf engineState <> "Recording" AndAlso _isRecordingLocal Then
                            _isRecordingLocal = False
                            RecordValue = False
                        End If
                    End If

                Case "engine_replay_start"

                    If status = "ok" Then
                        _isBufferingLocal = True
                        ReplayValue = True
                        SetControlEnabled(Menu_Replay_Box2, True)
                        SetControlEnabled(Menu_Replay_save_text, True)
                        SetControlEnabled(Menu_Replay_save_key, True)
                        ShowNotifier("instant_replay_on")
                    Else
                        Debug.WriteLine($"[Overlay] Engine replay_start FAILED: {status}")
                        ShowNotifier("replay_error")
                    End If

                Case "engine_replay_stop"
                    If status = "ok" Then
                        ShowNotifier("instant_replay_off")
                    Else

                        Debug.WriteLine($"[Overlay] Engine replay_stop FAILED: {status}")
                    End If

                Case "engine_replay_save"
                    If status = "ok" Then

                        ShowNotifier("saved_last_15")
                    Else
                        Debug.WriteLine($"[Overlay] Engine replay_save FAILED: {status}")
                        ShowNotifier("replay_error")
                    End If

            End Select
        Catch ex As Exception
            Debug.WriteLine($"[Overlay] HandleEngineResponse error: {ex.Message}")
        End Try
    End Sub
End Class
