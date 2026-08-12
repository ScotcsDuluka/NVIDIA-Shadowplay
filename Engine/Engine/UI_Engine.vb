' UI_Engine.vb
' ShadowPlay Engine - Main Form Logic
' TCP Hub Client Mode — เชื่อม API Hub (port 5000) รับ engine_* commands

Imports System.IO
Imports System.Windows.Forms

Public Class UI_Engine

    ' ── Fields ─────────────────────────────────────────────────

    Private _settings As CaptureSettings
    Private _captureEngine As CaptureEngine
    Private _encoderDetector As EncoderDetector
    Private _hotkeyStartId As Integer = -1
    Private _hotkeyStopId As Integer = -1
    Private _configPath As String
    Private _isLoaded As Boolean = False

    ' ── TCP Hub Client (เชื่อม Hub port 5000) ──────────────────

    Private _hubClient As EngineHubClient

    ' ── Form Load / Close ──────────────────────────────────────

    Private Sub UI_Engine_Load(sender As Object, e As EventArgs) Handles Me.Load
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shadowplay-config.json")

        LoadSettings()
        InitializeEngine()
        DetectEncoders()

        ' เชื่อมกับ API Hub
        StartHubClient()

        AddHandler chkNativeRes.CheckedChanged, AddressOf OnNativeResChanged
        AddHandler btnRecord.Click, AddressOf OnRecordClick
        AddHandler btnStop.Click, AddressOf OnStopClick
        AddHandler btnBrowse.Click, AddressOf OnBrowseOutput
        AddHandler btnFFmpegBrowse.Click, AddressOf OnBrowseFFmpeg
        AddHandler btnDetect.Click, AddressOf OnDetectClick
        AddHandler tmrRecording.Tick, AddressOf OnTimerTick
        AddHandler cboEncoder.SelectedIndexChanged, AddressOf OnEncoderChanged
        AddHandler cboCaptureMethod.SelectedIndexChanged, AddressOf OnCaptureMethodChanged

        _isLoaded = True
    End Sub

    Private Sub UI_Engine_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If _captureEngine IsNot Nothing AndAlso _captureEngine.IsRecording Then
            _captureEngine.ForceStop()
        End If
        SaveSettings()
        If _hubClient IsNot Nothing Then _hubClient.Dispose()
        If _captureEngine IsNot Nothing Then _captureEngine.Dispose()
        ' ✅ P1: flush all queued log writers so no log lines are lost on exit.
        BackgroundLogger.ShutdownAll()
    End Sub

    ' ── Hub Client ────────────────────────────────────────────

    Private Sub StartHubClient()
        Try
            _hubClient = New EngineHubClient()
            AddHandler _hubClient.OnCommandReceived, AddressOf OnHubCommand
            AddHandler _hubClient.OnConnectionStatusChanged, AddressOf OnHubConnectionChanged
            AddHandler _hubClient.OnLog, AddressOf OnHubLog

            _hubClient.Connect()
            lblHotkeys.Text = "Connecting to Hub (port 5000)..."
            DebugLog("Hub client starting...")
        Catch ex As Exception
            lblHotkeys.Text = "Hub connect failed: " & ex.Message
            DebugLog("Hub client error: " & ex.Message)
        End Try
    End Sub

    Private Sub OnHubConnectionChanged(sender As Object, connected As Boolean)
        Me.Invoke(Sub()
                      If connected Then
                          lblHotkeys.Text = "Hub Connected (port 5000) | engine_* ready"
                          lblHotkeys.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                      Else
                          lblHotkeys.Text = "Hub Disconnected — reconnecting..."
                          lblHotkeys.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
                      End If
                  End Sub)
    End Sub

    Private Sub OnHubLog(sender As Object, message As String)
        DebugLog(message)
    End Sub

    ''' <summary>
    ''' รับ engine_* commands จาก Hub แล้ว execute
    ''' ✅ P1: made Async so a slow StopRecordingAsync (up to 10s) doesn't block
    ''' the listener thread. Old code did task.Wait() inside OnHubCommand which
    ''' queued every subsequent TCP command behind it.
    ''' </summary>
    Private Async Sub OnHubCommand(sender As Object, e As CommandEventArgs)
        DebugLog($"[Engine] Executing: {e.Command}={e.Value}" & If(e.RequestId, $" (req={e.RequestId})", ""))

        Try
            Select Case e.Command
                Case "engine_record_start"
                    Await HandleEngineRecordStart(e.Value, e.RequestId)
                Case "engine_record_stop"
                    Await HandleEngineRecordStop(e.RequestId)
                Case "engine_replay_start"
                    HandleEngineReplayStart(e.Value, e.RequestId)
                Case "engine_replay_stop"
                    HandleEngineReplayStop(e.RequestId)
                Case "engine_replay_save"
                    HandleEngineReplaySave(e.Value, e.RequestId)
                Case "engine_get_status"
                    HandleEngineGetStatus(e.RequestId)
                Case "engine_load_config"
                    HandleEngineLoadConfig(e.RequestId)
                Case "engine_set_encoder"
                    HandleEngineSetEncoder(e.Value, e.RequestId)
                Case Else
                    _hubClient.SendResponse(e.Command, "error", "unknown_command", e.RequestId)
            End Select
        Catch ex As Exception
            DebugLog($"OnHubCommand unhandled exception: {ex.Message}")
            Try
                _hubClient.SendResponse(e.Command, "error", ex.Message, e.RequestId)
            Catch
            End Try
        End Try
    End Sub

    ' ── Engine Command Handlers (TCP แค่ on/off) ──────────

    Private Async Function HandleEngineRecordStart(value As String, reqId As String) As Task
        Try
            ' โหลด config ใหม่จาก json ทุกครั้งก่อน record
            _settings = CaptureSettings.Load(_configPath)

            If Not IO.Directory.Exists(_settings.OutputDirectory) Then
                IO.Directory.CreateDirectory(_settings.OutputDirectory)
            End If

            _captureEngine?.Dispose()
            _captureEngine = New CaptureEngine(_settings)
            AddHandler _captureEngine.StateChanged, AddressOf OnEngineStateChanged
            AddHandler _captureEngine.RecordingStarted, AddressOf OnRecordingStarted
            AddHandler _captureEngine.RecordingStopped, AddressOf OnRecordingStopped
            AddHandler _captureEngine.ErrorOccurred, AddressOf OnEngineError

            ' ✅ P1.5: pass the Overlay-supplied output path through to CaptureEngine.
            ' value is the path the Overlay wants the file saved to (e.g.
            ' "C:\Users\...\Videos\Shadowplay\Gallery\Record_2024-01-01_12-00-00.mp4").
            ' If empty, CaptureEngine falls back to settings.GenerateOutputFilename().
            ' Old behavior ignored value entirely → file landed in Engine's preferred
            ' folder instead of where the Overlay told the user it would be.
            Dim ok As Boolean = Await _captureEngine.StartRecordingAsync(value)

            If ok Then
                Me.Invoke(Sub()
                              lblStatus.Text = "Recording (Hub)..."
                              lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                              tmrRecording.Start()
                              btnRecord.Enabled = False
                              btnStop.Enabled = True
                          End Sub)
                _hubClient.SendResponse("engine_record_start", "ok", _captureEngine.OutputFile, reqId)
            Else
                _hubClient.SendResponse("engine_record_start", "error", "start_failed", reqId)
            End If
        Catch ex As Exception
            DebugLog("RecordStart error: " & ex.Message)
            _hubClient.SendResponse("engine_record_start", "error", ex.Message, reqId)
        End Try
    End Function

    Private Async Function HandleEngineRecordStop(reqId As String) As Task
        Try
            If _captureEngine Is Nothing OrElse Not _captureEngine.IsRecording Then
                _hubClient.SendResponse("engine_record_stop", "error", "not_recording", reqId)
                Return
            End If

            ' ✅ P1: Await instead of task.Wait().
            Await _captureEngine.StopRecordingAsync()

            Me.Invoke(Sub()
                          tmrRecording.Stop()
                          lblTimer.Text = "00:00:00"
                          lblStatus.Text = "Saved: " & Path.GetFileName(_captureEngine.OutputFile)
                          lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                          btnRecord.Enabled = True
                          btnStop.Enabled = False
                      End Sub)

            _hubClient.SendResponse("engine_record_stop", "ok", _captureEngine.OutputFile, reqId)
        Catch ex As Exception
            _hubClient.SendResponse("engine_record_stop", "error", ex.Message, reqId)
        End Try
    End Function

    Private Sub HandleEngineReplayStart(value As String, reqId As String)
        _hubClient.SendResponse("engine_replay_start", "error", "not_implemented", reqId)
    End Sub

    Private Sub HandleEngineReplayStop(reqId As String)
        _hubClient.SendResponse("engine_replay_stop", "error", "not_implemented", reqId)
    End Sub

    Private Sub HandleEngineReplaySave(value As String, reqId As String)
        _hubClient.SendResponse("engine_replay_save", "error", "not_implemented", reqId)
    End Sub

    Private Sub HandleEngineGetStatus(reqId As String)
        Dim state As String = "Idle"
        If _captureEngine IsNot Nothing AndAlso _captureEngine.IsRecording Then
            state = "Recording"
        End If
        _hubClient.SendResponse("engine_get_status", "ok", state, reqId)
    End Sub

    Private Sub HandleEngineLoadConfig(reqId As String)
        Try
            _settings = CaptureSettings.Load(_configPath)
            _hubClient.SendResponse("engine_load_config", "ok", Nothing, reqId)
        Catch ex As Exception
            _hubClient.SendResponse("engine_load_config", "error", ex.Message, reqId)
        End Try
    End Sub

    Private Sub HandleEngineSetEncoder(value As String, reqId As String)
        _hubClient.SendResponse("engine_set_encoder", "error", "use_config_json", reqId)
    End Sub

    ' ── Init ──────────────────────────────────────────────────

    Private Sub LoadSettings()
        _settings = CaptureSettings.Load(_configPath)

        chkNativeRes.Checked = _settings.UseNativeResolution
        nudFPS.Value = Math.Max(1, Math.Min(240, _settings.FPS))
        nudBitrate.Value = Math.Max(1, Math.Min(200, CLng(_settings.Bitrate / 1000000)))
        txtOutputDir.Text = _settings.OutputDirectory
        txtFFmpegPath.Text = _settings.FFmpegPath

        Select Case _settings.CaptureMethod.ToLower()
            Case "ddagrab" : cboCaptureMethod.SelectedIndex = 0
            Case "gdigrab" : cboCaptureMethod.SelectedIndex = 1
            Case "gfxcapture" : cboCaptureMethod.SelectedIndex = 2
            Case Else : cboCaptureMethod.SelectedIndex = 0
        End Select

        ValidateFFmpegPath()
    End Sub

    Private Sub SaveSettings()
        If _settings Is Nothing Then Return
        _settings.UseNativeResolution = chkNativeRes.Checked
        _settings.FPS = CInt(nudFPS.Value)
        _settings.Bitrate = CLng(nudBitrate.Value) * 1000000
        _settings.OutputDirectory = txtOutputDir.Text
        _settings.FFmpegPath = txtFFmpegPath.Text

        Select Case cboCaptureMethod.SelectedIndex
            Case 0 : _settings.CaptureMethod = "ddagrab"
            Case 1 : _settings.CaptureMethod = "gdigrab"
            Case 2 : _settings.CaptureMethod = "gfxcapture"
        End Select

        _settings.Save(_configPath)
    End Sub

    Private Sub InitializeEngine()
        _captureEngine = New CaptureEngine(_settings)
        AddHandler _captureEngine.StateChanged, AddressOf OnEngineStateChanged
        AddHandler _captureEngine.RecordingStarted, AddressOf OnRecordingStarted
        AddHandler _captureEngine.RecordingStopped, AddressOf OnRecordingStopped
        AddHandler _captureEngine.ErrorOccurred, AddressOf OnEngineError
    End Sub

    ' ── Detect Encoders ────────────────────────────────────────

    Private Sub DetectEncoders()
        lblStatus.Text = "Detecting encoders..."
        lblStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)

        _encoderDetector = New EncoderDetector(_settings.FFmpegPath)

        Task.Run(Sub()
                     Try
                         Dim encOk As Boolean = _encoderDetector.DetectEncoders()
                         _encoderDetector.DetectCaptureDevices()

                         Me.Invoke(Sub()
                                       UpdateEncoderDropdown()

                                       If encOk Then
                                           lblStatus.Text = "Idle - Hub Client (" & _encoderDetector.VideoEncoders.Count.ToString() & " encoders)"
                                           lblStatus.ForeColor = Drawing.Color.FromArgb(160, 160, 160)
                                       Else
                                           lblStatus.Text = "Detection: " & If(String.IsNullOrEmpty(_encoderDetector.LastDetectionError), "No encoders", _encoderDetector.LastDetectionError)
                                           lblStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
                                       End If
                                   End Sub)
                     Catch ex As Exception
                         Me.Invoke(Sub()
                                       lblStatus.Text = "Detection failed: " & ex.Message
                                       lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                                   End Sub)
                     End Try
                 End Sub)
    End Sub

    Private Sub UpdateEncoderDropdown()
        cboEncoder.Items.Clear()

        Dim nvenc As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetNVENCEncoders()
        Dim qsv As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetQSVEncoders()
        Dim amf As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetAMFEncoders()
        Dim cpu As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetCPUEncoders()
        Dim fallbackTag As String = ""
        If _encoderDetector.UsedFallback Then fallbackTag = " [fb]"

        If nvenc.Count > 0 Then
            cboEncoder.Items.Add("-- NVIDIA NVENC --")
            For Each enc In nvenc
                cboEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If
        If qsv.Count > 0 Then
            cboEncoder.Items.Add("-- Intel QuickSync --")
            For Each enc In qsv
                cboEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If
        If amf.Count > 0 Then
            cboEncoder.Items.Add("-- AMD AMF --")
            For Each enc In amf
                cboEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If
        If cpu.Count > 0 Then
            cboEncoder.Items.Add("-- CPU Software --")
            For Each enc In cpu
                cboEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If

        If cboEncoder.Items.Count = 0 Then
            cboEncoder.Items.Add("No encoders available")
            cboEncoder.SelectedIndex = 0
            Return
        End If

        Dim recommended As String = _encoderDetector.GetRecommendedEncoder()
        For i As Integer = 0 To cboEncoder.Items.Count - 1
            If cboEncoder.Items(i).ToString().IndexOf(recommended, StringComparison.OrdinalIgnoreCase) >= 0 Then
                cboEncoder.SelectedIndex = i
                ' ✅ FIX: set _settings.Encoder directly here. Previously this fired OnEncoderChanged
                ' which early-returned when _isLoaded=False (race: detection completes before line 46
                ' sets _isLoaded=True), so first Record click errored with "No encoder selected".
                If _settings IsNot Nothing Then
                    Dim parts As String() = cboEncoder.Items(i).ToString().Trim().Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
                    If parts.Length > 0 Then _settings.Encoder = parts(0).Trim()
                End If
                Exit For
            End If
        Next
    End Sub

    ' ── Record / Stop ──────────────────────────────────────────

    Private Sub OnRecordClick(sender As Object, e As EventArgs)
        SaveSettings()
        btnRecord.Enabled = False
        btnStop.Enabled = True

        Task.Run(Sub()
                     Try
                         Dim engine As New CaptureEngine(_settings)
                         AddHandler engine.StateChanged, AddressOf OnEngineStateChanged
                         AddHandler engine.RecordingStarted, AddressOf OnRecordingStarted
                         AddHandler engine.RecordingStopped, AddressOf OnRecordingStopped
                         AddHandler engine.ErrorOccurred, AddressOf OnEngineError

                         _captureEngine?.Dispose()
                         _captureEngine = engine

                         Dim ok As Task(Of Boolean) = _captureEngine.StartRecordingAsync()
                         ok.Wait()
                     Catch ex As Exception
                         Me.Invoke(Sub()
                                       lblStatus.Text = "Error: " & ex.Message
                                       lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                                       btnRecord.Enabled = True
                                       btnStop.Enabled = False
                                   End Sub)
                     End Try
                 End Sub)
    End Sub

    Private Sub OnStopClick(sender As Object, e As EventArgs)
        btnStop.Enabled = False

        Task.Run(Sub()
                     Try
                         Dim task As Task(Of Boolean) = _captureEngine.StopRecordingAsync()
                         task.Wait()
                         Me.Invoke(Sub() btnRecord.Enabled = True)
                     Catch
                         Me.Invoke(Sub() btnRecord.Enabled = True)
                     End Try
                 End Sub)
    End Sub

    ' ── Engine Events ────────────────────────────────────────────

    Private Sub OnEngineStateChanged(state As CaptureEngine.CaptureState)
        Me.Invoke(Sub()
                      Select Case state
                          Case CaptureEngine.CaptureState.Idle
                              lblStatus.Text = "Idle - Hub Client"
                              lblStatus.ForeColor = Drawing.Color.FromArgb(160, 160, 160)
                              tmrRecording.Stop()

                          Case CaptureEngine.CaptureState.Recording
                              lblStatus.Text = "Recording..."
                              lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                              lblTimer.ForeColor = Drawing.Color.Red
                              tmrRecording.Start()

                          Case CaptureEngine.CaptureState.Stopping
                              lblStatus.Text = "Stopping..."
                              lblStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
                              tmrRecording.Stop()

                          Case CaptureEngine.CaptureState.HasError
                              lblStatus.Text = "Error"
                              lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                              tmrRecording.Stop()
                              btnRecord.Enabled = True
                              btnStop.Enabled = False
                      End Select
                  End Sub)
    End Sub

    Private Sub OnRecordingStarted(filename As String)
        Me.Invoke(Sub()
                      lblStatus.Text = "Recording: " & Path.GetFileName(filename)
                  End Sub)
    End Sub

    Private Sub OnRecordingStopped(filename As String)
        Me.Invoke(Sub()
                      tmrRecording.Stop()
                      lblTimer.Text = "00:00:00"
                      lblTimer.ForeColor = _accentGreen
                      btnRecord.Enabled = True
                      lblStatus.Text = "Saved: " & Path.GetFileName(filename)
                      lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                  End Sub)
    End Sub

    Private Sub OnEngineError(message As String)
        Me.Invoke(Sub()
                      lblStatus.Text = "Error: " & message
                      lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                      btnRecord.Enabled = True
                      btnStop.Enabled = False
                  End Sub)
    End Sub

    Private Sub OnHotkeyPressed(id As Integer)
        If id = _hotkeyStartId AndAlso _captureEngine.State = CaptureEngine.CaptureState.Idle Then
            OnRecordClick(Nothing, Nothing)
        ElseIf id = _hotkeyStopId AndAlso _captureEngine.IsRecording Then
            OnStopClick(Nothing, Nothing)
        End If
    End Sub

    Private Sub OnTimerTick(sender As Object, e As EventArgs)
        If _captureEngine IsNot Nothing Then
            lblTimer.Text = _captureEngine.RecordingDuration.ToString("hh\:mm\:ss")
        End If
    End Sub

    ' ── UI Change Handlers ─────────────────────────────────────

    Private Sub OnNativeResChanged(sender As Object, e As EventArgs)
        cboResolution.Enabled = Not chkNativeRes.Checked
    End Sub

    Private Sub OnEncoderChanged(sender As Object, e As EventArgs)
        If Not _isLoaded Then Return
        Dim selected As String = cboEncoder.Text.Trim()
        Dim parts As String() = selected.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length > 0 Then
            _settings.Encoder = parts(0).Trim()
        End If
    End Sub

    Private Sub OnCaptureMethodChanged(sender As Object, e As EventArgs)
        If Not _isLoaded Then Return
        Select Case cboCaptureMethod.SelectedIndex
            Case 0 : _settings.CaptureMethod = "ddagrab"
            Case 1 : _settings.CaptureMethod = "gdigrab"
            Case 2 : _settings.CaptureMethod = "gfxcapture"
        End Select
    End Sub

    Private Sub OnBrowseOutput(sender As Object, e As EventArgs)
        Using dlg As New FolderBrowserDialog()
            dlg.Description = "Select recording output folder"
            dlg.SelectedPath = txtOutputDir.Text
            If dlg.ShowDialog() = DialogResult.OK Then
                txtOutputDir.Text = dlg.SelectedPath
                _settings.OutputDirectory = dlg.SelectedPath
            End If
        End Using
    End Sub

    Private Sub OnBrowseFFmpeg(sender As Object, e As EventArgs)
        Using dlg As New OpenFileDialog()
            dlg.Title = "Select ffmpeg.exe"
            dlg.Filter = "FFmpeg (ffmpeg.exe)|ffmpeg.exe"
            dlg.FileName = "ffmpeg.exe"

            Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
            If File.Exists(txtFFmpegPath.Text) Then
                dlg.InitialDirectory = Path.GetDirectoryName(txtFFmpegPath.Text)
            ElseIf Directory.Exists(Path.Combine(appDir, "API-Core")) Then
                dlg.InitialDirectory = Path.Combine(appDir, "API-Core")
            ElseIf appDir.Contains("bin" & IO.Path.DirectorySeparatorChar) Then
                Dim parentDir As String = appDir
                For depth As Integer = 1 To 5
                    Try
                        parentDir = System.IO.Directory.GetParent(parentDir)?.FullName
                        If String.IsNullOrWhiteSpace(parentDir) Then Exit For
                        Dim apiCoreDir As String = Path.Combine(parentDir, "API-Core")
                        If Directory.Exists(apiCoreDir) Then
                            dlg.InitialDirectory = apiCoreDir
                            Exit For
                        End If
                    Catch
                        Exit For
                    End Try
                Next
            End If

            If dlg.ShowDialog() = DialogResult.OK Then
                txtFFmpegPath.Text = dlg.FileName
                _settings.FFmpegPath = dlg.FileName
                ValidateFFmpegPath()
                DetectEncoders()
            End If
        End Using
    End Sub

    Private Sub OnDetectClick(sender As Object, e As EventArgs)
        SaveSettings()
        DetectEncoders()
    End Sub

    Private Sub ValidateFFmpegPath()
        If File.Exists(txtFFmpegPath.Text) Then
            lblFFmpegStatus.Text = "FFmpeg found"
            lblFFmpegStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
        Else
            lblFFmpegStatus.Text = "FFmpeg not found - click ... to browse"
            lblFFmpegStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
        End If
    End Sub

    Private Sub DebugLog(message As String)
        Try
            Dim logDir As String = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            Dim logPath As String = IO.Path.Combine(logDir, "ui-engine.log")
            Dim logLine As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] " & message
            ' ✅ P1: route through BackgroundLogger instead of File.AppendAllText per line.
            BackgroundLogger.Log(logPath, logLine)
        Catch
        End Try
    End Sub

End Class