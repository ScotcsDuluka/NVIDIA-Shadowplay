Public Class Base

    Public Shared tcp As TcpClientHelper

    ' ── L1 (UI/Host Recovery): engine status rehydration ────────
    ' A restarted UI must learn the truth from the Engine, not from cached
    ' local state. The hub does NOT replay history to late joiners (Broadcast
    ' sends only to current clients), so engine_ready — which the Engine
    ' emits when IT connects/reconnects — is never seen by a UI that starts
    ' while the Engine is already connected. The fix is a bounded PULL:
    '   _engineStatusPulled = set once ANY engine_get_status answer arrives
    '                         (an answer proves the Engine is responsive —
    '                         process-alive alone is never treated as healthy)
    '   StartBoundedStatusPull = UI-thread timer, 2s × 10 attempts (20s cap):
    '                         bounded retry, no infinite reconnect loop; when
    '                         it exhausts, the UI stays honestly disconnected
    '                         and the user sees "Hub Offline" on the record
    '                         panel — no fake "Recording" is ever shown.
    ' It never launches or kills anything — the EngineProcessSupervisor owns
    ' process lifecycle and reuses an existing engine.
    Private _engineStatusPulled As Boolean = False
    Private _statusPullTimer As System.Windows.Forms.Timer
    Private Const StatusPullIntervalMs As Integer = 2000
    Private Const StatusPullMaxAttempts As Integer = 10

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        tcp = New TcpClientHelper("NVIDIA Overlay")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage

        ' L1: if THIS UI's socket lost the hub and came back (hub restart,
        ' sleep/resume), re-pull authoritative engine state — the Engine does
        ' not re-announce engine_ready for a UI-side reconnect.
        AddHandler tcp.OnReconnected, AddressOf OnTcpReconnected

        tcp.ConnectAsync()

        InitReplayHonesty()

        EngineProcessSupervisor.EnsureEngineRunning()

        StartBoundedStatusPull()
    End Sub

    ''' <summary>
    ''' L1: bounded engine status pull for (re)start rehydration. Runs on the
    ''' UI thread; each tick asks the hub for engine_get_status until the
    ''' Engine answers or the attempt budget is gone. Sending is a no-op while
    ''' the socket is down (Send checks IsConnected), so a hub that is still
    ''' offline simply consumes attempts — deterministic fallback, no
    ''' duplicate engine launch, no unbounded loop.
    ''' </summary>
    Private Sub StartBoundedStatusPull()
        Try
            If _statusPullTimer IsNot Nothing Then Return

            Dim pullTimer As New System.Windows.Forms.Timer With {.Interval = StatusPullIntervalMs}
            _statusPullTimer = pullTimer
            Dim attemptsLeft As Integer = StatusPullMaxAttempts

            AddHandler pullTimer.Tick, Sub(s, ev)
                                           Try
                                               attemptsLeft -= 1
                                               If _engineStatusPulled OrElse attemptsLeft <= 0 Then
                                                   pullTimer.Stop()
                                                   pullTimer.Dispose()
                                                   If _statusPullTimer Is pullTimer Then _statusPullTimer = Nothing
                                                   If Not _engineStatusPulled Then
                                                       Debug.WriteLine($"[Overlay] bounded status pull exhausted after {StatusPullMaxAttempts} attempts — engine state unknown (hub offline or engine initializing); engine broadcasts still reconcile on arrival")
                                                   End If
                                                   Return
                                               End If
                                               If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                                                   tcp.Send("engine_get_status")
                                                   Debug.WriteLine($"[Overlay] startup status pull attempt {StatusPullMaxAttempts - attemptsLeft}/{StatusPullMaxAttempts}")
                                               End If
                                           Catch ex As Exception
                                               Debug.WriteLine("[Overlay] status pull tick error: " & ex.Message)
                                           End Try
                                       End Sub

            pullTimer.Start()
            Debug.WriteLine($"[Overlay] bounded engine status pull started (max {StatusPullMaxAttempts} attempts)")
        Catch ex As Exception
            Debug.WriteLine("[Overlay] StartBoundedStatusPull error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' L1: this UI's TCP socket reconnected to the hub. Reset the pulled
    ''' latch and pull engine state immediately — the session may have
    ''' started, ended, or kept recording while we were blind.
    ''' </summary>
    Private Sub OnTcpReconnected()
        Try
            _engineStatusPulled = False
            If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                tcp.Send("engine_get_status")
                Debug.WriteLine("[Overlay] reconnect → pulled engine_get_status")
            End If
        Catch ex As Exception
            Debug.WriteLine("[Overlay] reconnect status pull failed: " & ex.Message)
        End Try
    End Sub

    Private Sub Base_TestFormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            ' L1: stop the bounded status pull — UI cleanup only. Nothing here
            ' touches NVIDIA Capture.exe: a recording session must survive
            ' this window closing (EngineProcessSupervisor.Shutdown below
            ' only stops the UI-side monitor thread).
            If _statusPullTimer IsNot Nothing Then
                _statusPullTimer.Stop()
                _statusPullTimer.Dispose()
                _statusPullTimer = Nothing
            End If

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

            ' Surface the failure text where the user pressed record — the
            ' toast says THAT it failed, this line says WHY (same channel
            ' as the "Hub Offline" message).
            Try
                If Record_Stats IsNot Nothing Then
                    Record_Stats.Text = message
                End If
            Catch
            End Try

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

                        Dim reason As String = If(parts.Length >= 3, parts(2).Trim(), "")
                        Debug.WriteLine($"[Overlay] Engine record_start FAILED: {reason}")
                        _isRecordingLocal = False
                        RecordValue = False

                        ' Map the engine's reject reason to the most specific toast we
                        ' have; everything else falls back to the generic error toast
                        ' (recording_error is registered in Notifier since the
                        ' silent-record-failure fix).
                        If reason.StartsWith("engine_not_ready", StringComparison.OrdinalIgnoreCase) OrElse
                           reason.StartsWith("engine_reconfiguring", StringComparison.OrdinalIgnoreCase) Then
                            ShowNotifier("notificationErrorEngineNotRunning")
                        Else
                            ShowNotifier("recording_error")
                        End If

                        ' Surface the exact reject reason where the user pressed record
                        ' (same channel as the "Hub Offline" message) — the toast says
                        ' THAT it failed, this line says WHY.
                        Try
                            If Record_Stats IsNot Nothing Then
                                Record_Stats.Text = If(reason.Length > 0, "Record failed: " & reason, "Record failed")
                            End If
                        Catch
                        End Try
                    End If

                Case "engine_record_stop"
                    If status = "ok" Then
                        Debug.WriteLine($"[Overlay] Engine confirmed record_stop OK")
                    Else

                        Debug.WriteLine($"[Overlay] Engine record_stop FAILED: {status}")
                        ShowNotifier("recording_error")
                    End If

                Case "engine_get_status"

                    ' L1: any answer proves the Engine is responsive — the
                    ' bounded startup/reconnect pull can retire.
                    _engineStatusPulled = True

                    If parts.Length >= 3 Then
                        ' L1 reconnect data contract:
                        '   <state>[|<elapsed_sec>[|<output_path>]]
                        ' The Engine appends elapsed + the active output path
                        ' only while a session is really alive ('|' is illegal
                        ' in Windows file names, so it cannot collide with the
                        ' path field). A state-only answer is still valid.
                        Dim statusFields As String() = parts(2).Trim().Split("|"c)
                        Dim engineState As String = statusFields(0).Trim()
                        Debug.WriteLine($"[Overlay] Engine status: {parts(2)}")

                        If engineState = "Recording" AndAlso Not _isRecordingLocal Then
                            _isRecordingLocal = True
                            RecordValue = True
                        ElseIf engineState <> "Recording" AndAlso _isRecordingLocal Then
                            _isRecordingLocal = False
                            RecordValue = False
                        End If

                        ' L1 rehydration: seed the record panel with ENGINE
                        ' truth (elapsed of the session that outlived the
                        ' previous UI process). The periodic
                        ' engine_recording_progress broadcast takes over from
                        ' here — nothing is extrapolated locally.
                        If engineState = "Recording" AndAlso statusFields.Length >= 2 Then
                            Dim restoredSec As Integer
                            If Integer.TryParse(statusFields(1).Trim(), restoredSec) AndAlso restoredSec >= 0 Then
                                If Record_Stats IsNot Nothing Then
                                    Record_Stats.Text = TimeSpan.FromSeconds(restoredSec).ToString("hh\:mm\:ss")
                                End If
                                If statusFields.Length >= 3 AndAlso statusFields(2).Trim().Length > 0 Then
                                    Debug.WriteLine($"[Overlay] rehydrated active output: {statusFields(2).Trim()}")
                                End If
                            End If
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
