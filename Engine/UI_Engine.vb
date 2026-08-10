
' UI_Engine.vb
' ShadowPlay Engine - GFE-Style Main Form Logic
' Quality presets, bitrate slider, FFmpeg preview, encoder detection,
' record/stop, hotkeys, config save/load.

Imports System.IO
Imports System.Text
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
    Private _qualityButtons As List(Of Button)
    Private _suppressSettingChange As Boolean = False

    ' ── Form Load / Close ──────────────────────────────────────

    Private Sub UI_Engine_Load(sender As Object, e As EventArgs) Handles Me.Load
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shadowplay-config.json")

        ' Build quality button list
        _qualityButtons = New List(Of Button) From {
            btnQualityLow,
            btnQualityMedium,
            btnQualityHigh,
            btnQualityCustom
        }

        ' Wire up all event handlers
        AddHandler btnQualityLow.Click, AddressOf OnQualityButtonClick
        AddHandler btnQualityMedium.Click, AddressOf OnQualityButtonClick
        AddHandler btnQualityHigh.Click, AddressOf OnQualityButtonClick
        AddHandler btnQualityCustom.Click, AddressOf OnQualityButtonClick
        AddHandler cboFPS.SelectedIndexChanged, AddressOf OnFPSChanged
        AddHandler cboPreset.SelectedIndexChanged, AddressOf OnPresetChanged
        AddHandler cboResolution.SelectedIndexChanged, AddressOf OnResolutionChanged
        AddHandler trkBitrate.Scroll, AddressOf OnBitrateScroll
        AddHandler btnPreviewRefresh.Click, AddressOf OnPreviewRefresh
        AddHandler btnPreviewCopy.Click, AddressOf OnPreviewCopy
        AddHandler cboCodecEncoder.SelectedIndexChanged, AddressOf OnCodecEncoderChanged
        AddHandler cboCaptureMethod.SelectedIndexChanged, AddressOf OnCaptureMethodChanged
        AddHandler btnRecord.Click, AddressOf OnRecordClick
        AddHandler btnStop.Click, AddressOf OnStopClick
        AddHandler btnBrowse.Click, AddressOf OnBrowseOutput
        AddHandler btnFFmpegBrowse.Click, AddressOf OnBrowseFFmpeg
        AddHandler tmrRecording.Tick, AddressOf OnTimerTick

        ' Load persisted settings into the model, then sync UI
        LoadSettings()
        SyncUIFromSettings()

        ' Initialize engine and hotkeys
        InitializeEngine()
        RegisterHotkeys()

        ' Async encoder detection
        DetectEncoders()

        _isLoaded = True

        ' Initial display updates
        UpdateBitrateDisplay()
        UpdateRecommendedRange()
        UpdateStorageEstimate()
        UpdateFFmpegPreview()
    End Sub

    Private Sub UI_Engine_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        SaveSettings()
        If _captureEngine IsNot Nothing AndAlso _captureEngine.IsRecording Then
            _captureEngine.ForceStop()
        End If
        If _hotkeyManager IsNot Nothing Then _hotkeyManager.Dispose()
        If _captureEngine IsNot Nothing Then _captureEngine.Dispose()
    End Sub

    ' ── Load / Save Settings ───────────────────────────────────

    Private Sub LoadSettings()
        _settings = CaptureSettings.Load(_configPath)
        txtOutputDir.Text = _settings.OutputDirectory
        txtFFmpegPath.Text = _settings.FFmpegPath
        ValidateFFmpegPath()
    End Sub

    Private Sub SaveSettings()
        If _settings Is Nothing Then Return

        ' Sync UI -> model
        _settings.FPS = GetSelectedFPS()
        _settings.Bitrate = CLng(trkBitrate.Value) * 1000L
        _settings.NVENCPreset = GetSelectedNVENCPreset()
        _settings.OutputDirectory = txtOutputDir.Text
        _settings.FFmpegPath = txtFFmpegPath.Text

        ' Resolution
        ApplyResolutionToSettings()

        ' Capture method
        Select Case cboCaptureMethod.SelectedIndex
            Case 0 : _settings.CaptureMethod = "ddagrab"
            Case 1 : _settings.CaptureMethod = "gdigrab"
            Case 2 : _settings.CaptureMethod = "gfxcapture"
        End Select

        ' Encoder
        Dim encText As String = GetSelectedEncoderID()
        If Not String.IsNullOrEmpty(encText) Then
            _settings.Encoder = encText
        End If

        _settings.Save(_configPath)
    End Sub

    ' ── Sync UI from Settings ──────────────────────────────────

    Private Sub SyncUIFromSettings()
        _suppressSettingChange = True
        Try
            ' Quality preset button
            Select Case _settings.QualityPreset.ToLower()
                Case "low"    : SetActiveQualityButton(btnQualityLow)
                Case "medium" : SetActiveQualityButton(btnQualityMedium)
                Case "high"   : SetActiveQualityButton(btnQualityHigh)
                Case "custom" : SetActiveQualityButton(btnQualityCustom)
                Case Else     : SetActiveQualityButton(btnQualityHigh)
            End Select

            ' FPS
            SelectFPSSelection(_settings.FPS)

            ' NVENC Preset combo
            SelectNVENCPresetSelection(_settings.NVENCPreset)

            ' Resolution combo
            SelectResolutionCombo()

            ' Bitrate trackbar (settings.Bitrate is in bps; trackbar is kbps)
            Dim bitrateKbps As Integer = CInt(_settings.Bitrate \ 1000L)
            bitrateKbps = Math.Max(trkBitrate.Minimum, Math.Min(trkBitrate.Maximum, bitrateKbps))
            trkBitrate.Value = bitrateKbps

            ' Capture method
            Select Case _settings.CaptureMethod.ToLower()
                Case "ddagrab"    : cboCaptureMethod.SelectedIndex = 0
                Case "gdigrab"    : cboCaptureMethod.SelectedIndex = 1
                Case "gfxcapture" : cboCaptureMethod.SelectedIndex = 2
                Case Else          : cboCaptureMethod.SelectedIndex = 0
            End Select

        Finally
            _suppressSettingChange = False
        End Try
    End Sub

    ' ── Quality Preset Buttons ─────────────────────────────────

    Private Sub OnQualityButtonClick(sender As Object, e As EventArgs)
        If Not _isLoaded Then Return

        Dim btn As Button = CType(sender, Button)
        Dim preset As String = CStr(btn.Tag)
        If String.IsNullOrEmpty(preset) Then Return

        If preset <> "custom" Then
            _settings.ApplyQualityPreset(preset)
            SyncUIFromSettings()
        End If

        _settings.QualityPreset = preset
        SetActiveQualityButton(btn)

        UpdateBitrateDisplay()
        UpdateRecommendedRange()
        UpdateStorageEstimate()
        UpdateFFmpegPreview()
    End Sub

    Private Sub SetActiveQualityButton(activeBtn As Button)
        For Each btn As Button In _qualityButtons
            If btn Is activeBtn Then
                btn.BackColor = _bgButtonQualActive
                btn.ForeColor = Drawing.Color.White
            Else
                btn.BackColor = _bgButtonQual
                btn.ForeColor = _fgText
            End If
        Next
    End Sub

    Private Sub SwitchToCustom()
        If _settings.QualityPreset <> "custom" Then
            _settings.QualityPreset = "custom"
            SetActiveQualityButton(btnQualityCustom)
        End If
    End Sub

    ' ── Settings Row 1: FPS / Preset / Resolution ─────────────

    Private Sub OnFPSChanged(sender As Object, e As EventArgs)
        If Not _isLoaded OrElse _suppressSettingChange Then Return
        _settings.FPS = GetSelectedFPS()
        SwitchToCustom()
        UpdateRecommendedRange()
        UpdateStorageEstimate()
        UpdateFFmpegPreview()
    End Sub

    Private Sub OnPresetChanged(sender As Object, e As EventArgs)
        If Not _isLoaded OrElse _suppressSettingChange Then Return
        _settings.NVENCPreset = GetSelectedNVENCPreset()
        SwitchToCustom()
        UpdateFFmpegPreview()
    End Sub

    Private Sub OnResolutionChanged(sender As Object, e As EventArgs)
        If Not _isLoaded OrElse _suppressSettingChange Then Return
        ApplyResolutionToSettings()
        SwitchToCustom()
        UpdateRecommendedRange()
        UpdateFFmpegPreview()
    End Sub

    ' ── Settings Row 2: Bitrate Slider ─────────────────────────

    Private Sub OnBitrateScroll(sender As Object, e As EventArgs)
        If Not _isLoaded Then Return
        _settings.Bitrate = CLng(trkBitrate.Value) * 1000L
        SwitchToCustom()
        UpdateBitrateDisplay()
        UpdateStorageEstimate()
        UpdateFFmpegPreview()
    End Sub

    Private Sub UpdateBitrateDisplay()
        Dim mbps As Double = trkBitrate.Value / 1000.0
        lblBitrateValue.Text = mbps.ToString("F1") & " Mbps"
    End Sub

    Private Sub UpdateStorageEstimate()
        Dim bitrateKbps As Long = CLng(trkBitrate.Value)
        Dim bitrateBps As Long = bitrateKbps * 1000L
        Dim bytesPerHour As Double = (bitrateBps / 8.0) * 3600.0
        Dim gbPerHour As Double = bytesPerHour / 1073741824.0
        lblStorageEstimate.Text = "~ " & gbPerHour.ToString("F1") & " GB/hour"
    End Sub

    Private Sub UpdateRecommendedRange()
        Dim fps As Integer = GetSelectedFPS()
        Dim resKey As String = GetResolutionKey()
        Dim rec As (RangeMin As Integer, RangeMax As Integer, RecMin As Integer, RecMax As Integer)
        rec = GetRecommendedBitrateRange(fps, resKey)
        lblBitrateRange.Text = "Range: 5 - 150 Mbps (Recommended: " &
            rec.RecMin.ToString() & " - " & rec.RecMax.ToString() & " Mbps)"
    End Sub

    ''' <summary>
    ''' Returns recommended bitrate range (in Mbps) for given FPS and resolution.
    ''' </summary>
    Private Function GetRecommendedBitrateRange(fps As Integer, resolutionKey As String) _
        As (RangeMin As Integer, RangeMax As Integer, RecMin As Integer, RecMax As Integer)

        Dim isHighFps As Boolean = (fps >= 60)

        Select Case resolutionKey
            Case "1920x1080", "Native"
                If isHighFps Then
                    Return (35, 100, 50, 80)     ' 1080p60
                Else
                    Return (10, 50, 20, 35)      ' 1080p30
                End If
            Case "1280x720"
                If isHighFps Then
                    Return (15, 50, 25, 40)      ' 720p60
                Else
                    Return (5, 25, 10, 20)       ' 720p30
                End If
            Case "854x480"
                If isHighFps Then
                    Return (5, 20, 8, 15)        ' 480p60
                Else
                    Return (2, 10, 3, 6)         ' 480p30
                End If
            Case Else
                If isHighFps Then
                    Return (15, 50, 25, 40)
                Else
                    Return (5, 25, 10, 20)
                End If
        End Select
    End Function

    ' ── FFmpeg Preview ──────────────────────────────────────────

    Private Sub UpdateFFmpegPreview()
        txtFFmpegPreview.Text = BuildFFmpegPreview()
    End Sub

    Private Function BuildFFmpegPreview() As String
        Dim encoder As String = _settings.Encoder
        If String.IsNullOrEmpty(encoder) Then
            encoder = GetSelectedEncoderID()
        End If
        If String.IsNullOrEmpty(encoder) Then
            encoder = "h264_nvenc"
        End If

        Dim fps As Integer = GetSelectedFPS()
        Dim bitrateKbps As Long = CLng(trkBitrate.Value)

        Dim resW As Integer
        Dim resH As Integer
        If cboResolution.SelectedIndex <= 0 Then
            ' Native
            resW = Screen.PrimaryScreen.Bounds.Width
            resH = Screen.PrimaryScreen.Bounds.Height
        Else
            Dim parts As String() = CStr(cboResolution.SelectedItem).Split("x"c)
            If parts.Length = 2 Then
                resW = CInt(parts(0))
                resH = CInt(parts(1))
            Else
                resW = 1920
                resH = 1080
            End If
        End If

        Dim captureMethod As String = _settings.CaptureMethod
        If String.IsNullOrEmpty(captureMethod) Then
            captureMethod = "ddagrab"
        End If

        Dim nvecPreset As String = _settings.NVENCPreset
        If String.IsNullOrEmpty(nvecPreset) Then
            nvecPreset = "p4"
        End If

        Dim tune As String = _settings.Tune
        If String.IsNullOrEmpty(tune) Then
            tune = "ll"
        End If

        Dim sb As New StringBuilder()
        sb.Append("ffmpeg.exe")
        sb.Append(" -f " & captureMethod)
        sb.Append(" -framerate " & fps.ToString())
        sb.Append(" -video_size " & resW.ToString() & "x" & resH.ToString())
        sb.Append(" -i desktop")
        sb.AppendLine()
        sb.Append("  -c:v " & encoder)
        sb.Append(" -preset " & nvecPreset)
        sb.Append(" -tune " & tune)

        If _settings.Zerolatency Then
            sb.Append(" -zerolatency 1")
        End If

        If _settings.SpatialAQ Then
            sb.Append(" -spatial_aq 1")
        End If

        If _settings.TemporalAQ Then
            sb.Append(" -temporal_aq 1")
        End If

        If _settings.Lookahead > 0 Then
            sb.Append(" -lookahead " & _settings.Lookahead.ToString())
        End If

        sb.Append(" -rc " & _settings.RateControl)
        sb.Append(" -b:v " & bitrateKbps.ToString() & "k")
        sb.Append(" -pix_fmt " & _settings.PixelFormat)
        sb.AppendLine()
        sb.Append("  -f " & _settings.FileFormat)
        sb.Append(" """ & _settings.GenerateOutputFilename() & """)

        Return sb.ToString()
    End Function

    Private Sub OnPreviewRefresh(sender As Object, e As EventArgs)
        UpdateFFmpegPreview()
    End Sub

    Private Sub OnPreviewCopy(sender As Object, e As EventArgs)
        If Not String.IsNullOrEmpty(txtFFmpegPreview.Text) Then
            Try
                Clipboard.SetText(txtFFmpegPreview.Text)
            Catch
                ' Clipboard may be locked by another process
            End Try
        End If
    End Sub

    ' ── Codec Encoder Combo ────────────────────────────────────

    Private Sub OnCodecEncoderChanged(sender As Object, e As EventArgs)
        If Not _isLoaded Then Return
        Dim encId As String = GetSelectedEncoderID()
        If Not String.IsNullOrEmpty(encId) Then
            _settings.Encoder = encId
        End If
        UpdateFFmpegPreview()
    End Sub

    Private Function GetSelectedEncoderID() As String
        If cboCodecEncoder.SelectedIndex < 0 Then Return ""
        Dim text As String = cboCodecEncoder.Text.Trim()
        ' Format: "  h264_nvenc - H.264 [fb]" -> extract first token
        If text.StartsWith("--") Then Return "" ' Group header
        Dim parts As String() = text.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length > 0 Then
            Return parts(0).Trim()
        End If
        Return ""
    End Function

    ' ── Capture Method ─────────────────────────────────────────

    Private Sub OnCaptureMethodChanged(sender As Object, e As EventArgs)
        If Not _isLoaded Then Return
        Select Case cboCaptureMethod.SelectedIndex
            Case 0 : _settings.CaptureMethod = "ddagrab"
            Case 1 : _settings.CaptureMethod = "gdigrab"
            Case 2 : _settings.CaptureMethod = "gfxcapture"
        End Select
        UpdateFFmpegPreview()
    End Sub

    ' ── Encoder Detection ──────────────────────────────────────

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
                                 _settings.AudioCapture = False
                                 _settings.Save(_configPath)
                             End If
                         End If

                         Dim detectedAudioDevices As List(Of String) = audioDevices

                         Me.Invoke(Sub()
                                       PopulateEncoderDropdown()

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

                                       ' Try to auto-select the previously saved encoder
                                       AutoSelectSavedEncoder()

                                       ' Update FFmpeg preview now that we have encoders
                                       UpdateFFmpegPreview()

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

    Private Sub PopulateEncoderDropdown()
        cboCodecEncoder.Items.Clear()

        Dim nvenc As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetNVENCEncoders()
        Dim qsv As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetQSVEncoders()
        Dim amf As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetAMFEncoders()
        Dim cpu As List(Of EncoderDetector.EncoderInfo) = _encoderDetector.GetCPUEncoders()
        Dim fallbackTag As String = ""
        If _encoderDetector.UsedFallback Then fallbackTag = " [fb]"

        If nvenc.Count > 0 Then
            cboCodecEncoder.Items.Add("-- NVIDIA NVENC --")
            For Each enc In nvenc
                cboCodecEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If
        If qsv.Count > 0 Then
            cboCodecEncoder.Items.Add("-- Intel QuickSync --")
            For Each enc In qsv
                cboCodecEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If
        If amf.Count > 0 Then
            cboCodecEncoder.Items.Add("-- AMD AMF --")
            For Each enc In amf
                cboCodecEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If
        If cpu.Count > 0 Then
            cboCodecEncoder.Items.Add("-- CPU Software --")
            For Each enc In cpu
                cboCodecEncoder.Items.Add("  " & enc.ID & " - " & enc.CodecFamily & fallbackTag)
            Next
        End If

        If cboCodecEncoder.Items.Count = 0 Then
            cboCodecEncoder.Items.Add("No encoders available")
            cboCodecEncoder.SelectedIndex = 0
        End If
    End Sub

    Private Sub AutoSelectSavedEncoder()
        ' Try to match the saved encoder first
        If Not String.IsNullOrEmpty(_settings.Encoder) Then
            For i As Integer = 0 To cboCodecEncoder.Items.Count - 1
                Dim itemText As String = CStr(cboCodecEncoder.Items(i))
                If itemText.IndexOf(_settings.Encoder, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    cboCodecEncoder.SelectedIndex = i
                    Return
                End If
            Next
        End If

        ' Fall back to the recommended encoder
        If _encoderDetector IsNot Nothing Then
            Dim recommended As String = _encoderDetector.GetRecommendedEncoder()
            For i As Integer = 0 To cboCodecEncoder.Items.Count - 1
                Dim itemText As String = CStr(cboCodecEncoder.Items(i))
                If itemText.IndexOf(recommended, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    cboCodecEncoder.SelectedIndex = i
                    Exit For
                End If
            Next
        End If
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
                         AddHandler engine.ErrorOccurred, AddressOf OnEngineErrorOccurred

                         _captureEngine?.Dispose()
                         _captureEngine = engine

                         Dim ok As Task(Of Boolean) = _captureEngine.StartRecordingAsync()
                         ok.Wait()
                     Catch ex As Exception
                         Me.Invoke(Sub()
                                       lblStatus.Text = "Failed: " & ex.Message
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

    Private Sub OnEngineErrorOccurred(message As String)
        Me.Invoke(Sub()
                      lblStatus.Text = "Error: " & message
                      lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                      btnRecord.Enabled = True
                      btnStop.Enabled = False
                  End Sub)
    End Sub

    ' ── Hotkeys ────────────────────────────────────────────────

    Private Sub RegisterHotkeys()
        _hotkeyManager = New HotkeyManager()
        _hotkeyStartId = _hotkeyManager.RegisterFromString(_settings.HotkeyStart)
        _hotkeyStopId = _hotkeyManager.RegisterFromString(_settings.HotkeyStop)
        If _hotkeyStartId >= 0 Then
            AddHandler _hotkeyManager.HotkeyPressed, AddressOf OnHotkeyPressed
        End If
    End Sub

    Private Sub OnHotkeyPressed(id As Integer)
        If _captureEngine Is Nothing Then Return
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

    ' ── Browse Dialogs ─────────────────────────────────────────

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
            ElseIf appDir.Contains("bin" & Path.DirectorySeparatorChar) Then
                Dim parentDir As String = appDir
                For depth As Integer = 1 To 5
                    Try
                        parentDir = Directory.GetParent(parentDir)?.FullName
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

    ' ── Helpers ────────────────────────────────────────────────

    Private Sub ValidateFFmpegPath()
        If File.Exists(txtFFmpegPath.Text) Then
            lblFFmpegStatus.Text = "FFmpeg found"
            lblFFmpegStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
        Else
            lblFFmpegStatus.Text = "FFmpeg not found - click ... to browse"
            lblFFmpegStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
        End If
    End Sub

    Private Sub InitializeEngine()
        _captureEngine = New CaptureEngine(_settings)
        AddHandler _captureEngine.StateChanged, AddressOf OnEngineStateChanged
        AddHandler _captureEngine.RecordingStarted, AddressOf OnRecordingStarted
        AddHandler _captureEngine.RecordingStopped, AddressOf OnRecordingStopped
        AddHandler _captureEngine.ErrorOccurred, AddressOf OnEngineErrorOccurred
    End Sub

    Private Function GetSelectedFPS() As Integer
        If cboFPS.SelectedIndex >= 0 Then
            Dim val As Integer = 0
            If Integer.TryParse(CStr(cboFPS.SelectedItem), val) Then
                Return val
            End If
        End If
        Return _settings.FPS
    End Function

    Private Function GetSelectedNVENCPreset() As String
        If cboPreset.SelectedIndex >= 0 Then
            Return CStr(cboPreset.SelectedItem).ToLower()
        End If
        Return _settings.NVENCPreset
    End Function

    Private Function GetResolutionKey() As String
        If cboResolution.SelectedIndex > 0 Then
            Return CStr(cboResolution.SelectedItem)
        End If
        Return "Native"
    End Function

    Private Sub ApplyResolutionToSettings()
        If cboResolution.SelectedIndex <= 0 Then
            _settings.UseNativeResolution = True
            _settings.CustomWidth = 0
            _settings.CustomHeight = 0
        Else
            Dim text As String = CStr(cboResolution.SelectedItem)
            Dim parts As String() = text.Split("x"c)
            If parts.Length = 2 Then
                _settings.UseNativeResolution = False
                _settings.CustomWidth = CInt(parts(0))
                _settings.CustomHeight = CInt(parts(1))
            End If
        End If
    End Sub

    Private Sub SelectFPSSelection(targetFps As Integer)
        For i As Integer = 0 To cboFPS.Items.Count - 1
            If CStr(cboFPS.Items(i)) = targetFps.ToString() Then
                cboFPS.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Private Sub SelectNVENCPresetSelection(targetPreset As String)
        If String.IsNullOrEmpty(targetPreset) Then targetPreset = "p4"
        Dim target As String = targetPreset.ToLower()
        For i As Integer = 0 To cboPreset.Items.Count - 1
            If CStr(cboPreset.Items(i)).ToLower() = target Then
                cboPreset.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Private Sub SelectResolutionCombo()
        If _settings.UseNativeResolution Then
            cboResolution.SelectedIndex = 0
        ElseIf _settings.CustomWidth = 1920 AndAlso _settings.CustomHeight = 1080 Then
            cboResolution.SelectedIndex = 1
        ElseIf _settings.CustomWidth = 1280 AndAlso _settings.CustomHeight = 720 Then
            cboResolution.SelectedIndex = 2
        ElseIf _settings.CustomWidth = 854 AndAlso _settings.CustomHeight = 480 Then
            cboResolution.SelectedIndex = 3
        Else
            cboResolution.SelectedIndex = 0
        End If
    End Sub

    Private Sub DebugLog(message As String)
        Try
            Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            If Not Directory.Exists(logDir) Then Directory.CreateDirectory(logDir)
            Dim logPath As String = Path.Combine(logDir, "ui-engine.log")
            Dim logLine As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] " & message & Environment.NewLine
            File.AppendAllText(logPath, logLine)
        Catch
        End Try
    End Sub

End Class
