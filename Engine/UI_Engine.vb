' UI_Engine.vb
' ShadowPlay Engine - Main Form Logic
' Connects UI to CaptureEngine, EncoderDetector, CaptureSettings
' Handles: Record/Stop, Detect, Hotkeys, Browse, Config load/save

Imports System.IO
Imports System.Windows.Forms

Public Class UI_Engine

    ' ── Fields ─────────────────────────────────────────────────

    Private _settings As CaptureSettings
    Private _captureEngine As CaptureEngine
    Private _encoderDetector As EncoderDetector
    Private _hotkeyManager As HotkeyManager
    Private _hotkeyStartId As Integer = -1
    Private _hotkeyStopId As Integer = -1
    Private _configPath As String
    Private _isLoaded As Boolean = False

    ' ── Form Load / Close ──────────────────────────────────────

    Private Sub UI_Engine_Load(sender As Object, e As EventArgs) Handles Me.Load
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shadowplay-config.json")

        LoadSettings()
        InitializeEngine()
        RegisterHotkeys()
        DetectEncoders()

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
        If _hotkeyManager IsNot Nothing Then _hotkeyManager.Dispose()
        If _captureEngine IsNot Nothing Then _captureEngine.Dispose()
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

    Private Sub RegisterHotkeys()
        _hotkeyManager = New HotkeyManager()
        _hotkeyStartId = _hotkeyManager.RegisterFromString(_settings.HotkeyStart)
        _hotkeyStopId = _hotkeyManager.RegisterFromString(_settings.HotkeyStop)
        If _hotkeyStartId >= 0 Then
            AddHandler _hotkeyManager.HotkeyPressed, AddressOf OnHotkeyPressed
        End If
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

                         ' Auto-detect audio devices
                         Dim audioDevices As List(Of String) = _encoderDetector.DetectAudioDevices()
                         Dim audioOk As Boolean = False
                         If _settings.AudioCapture AndAlso Not String.IsNullOrWhiteSpace(_settings.AudioDevice) Then
                             audioOk = _encoderDetector.IsAudioDeviceAvailable(_settings.AudioDevice)
                             If Not audioOk Then
                                 ' Audio device not found - disable audio capture
                                 _settings.AudioCapture = False
                                 _settings.Save(_configPath)
                             End If
                         End If

                         Dim detectedAudioDevices As List(Of String) = audioDevices

                         Me.Invoke(Sub()
                                       UpdateEncoderDropdown()

                                       If encOk Then
                                           Dim msg As String = "Idle - Ready (" & _encoderDetector.VideoEncoders.Count.ToString() & " encoders"
                                           If Not audioOk AndAlso _settings.AudioCapture = False Then
                                               msg = msg & ", no audio device"
                                           End If
                                           msg = msg & ")"
                                           lblStatus.Text = msg
                                           lblStatus.ForeColor = Drawing.Color.FromArgb(160, 160, 160)
                                       Else
                                           Dim errDetail As String = _encoderDetector.LastDetectionError
                                           If _encoderDetector.UsedFallback Then
                                               lblStatus.Text = "FFmpeg detect failed - using fallback encoders. Check: " & errDetail
                                               lblStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
                                           Else
                                           If String.IsNullOrEmpty(errDetail) Then errDetail = "No encoders found"
                                           lblStatus.Text = "Detection: " & errDetail
                                           lblStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
                                           End If
                                       End If

                                       ' Log detected audio devices
                                       If detectedAudioDevices.Count > 0 Then
                                           DebugLog("Audio devices found: " & String.Join(", ", detectedAudioDevices))
                                       Else
                                           DebugLog("No audio devices found on this system")
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

        ' Select recommended
        Dim recommended As String = _encoderDetector.GetRecommendedEncoder()
        For i As Integer = 0 To cboEncoder.Items.Count - 1
            If cboEncoder.Items(i).ToString().IndexOf(recommended, StringComparison.OrdinalIgnoreCase) >= 0 Then
                cboEncoder.SelectedIndex = i
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

    ' ── Engine Events ─────────────────────────────────────────

    Private Sub OnEngineStateChanged(state As CaptureEngine.CaptureState)
        Me.Invoke(Sub()
                      Select Case state
                          Case CaptureEngine.CaptureState.Idle
                              lblStatus.Text = "Idle - Ready"
                              lblStatus.ForeColor = Drawing.Color.FromArgb(160, 160, 160)
                              lblTimer.ForeColor = _accentGreen
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

            ' Auto-navigate to likely locations
            Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
            If File.Exists(txtFFmpegPath.Text) Then
                dlg.InitialDirectory = Path.GetDirectoryName(txtFFmpegPath.Text)
            ElseIf Directory.Exists(Path.Combine(appDir, "API-Core")) Then
                dlg.InitialDirectory = Path.Combine(appDir, "API-Core")
            ElseIf appDir.Contains("bin" & IO.Path.DirectorySeparatorChar) Then
                ' Walk up from bin\Release\... to solution root
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
                ' Re-detect encoders with new FFmpeg path
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
            If Not IO.Directory.Exists(logDir) Then IO.Directory.CreateDirectory(logDir)
            Dim logPath As String = IO.Path.Combine(logDir, "ui-engine.log")
            Dim logLine As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] " & message & Environment.NewLine
            IO.File.AppendAllText(logPath, logLine)
        Catch
        End Try
    End Sub

End Class
