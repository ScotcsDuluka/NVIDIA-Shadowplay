' UI_Engine.vb
' ShadowPlay Engine - Main Form Logic
' TCP Hub Client Mode — เชื่อม API Hub (port 5000) รับ engine_* commands
'
' ✅ P2: Engine UI ตอนนี้อ่านค่าจาก config.json + video.json ของ Overlay
' (source of truth) แสดงผลแบบ read-only และ refresh ทุก 2 วินาที
' ถ้า Overlay เปลี่ยนค่า → Engine UI จะอัปเดทตาม

Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Xml

Partial Public Class UI_Engine
    Const WS_EX_TRANSPARENT As Integer = &H20



    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Protected Overrides Sub WndProc(ByRef m As Message)

        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If m.Msg = WM_NCHITTEST Then
            Dim pos As Point = Me.PointToClient(Cursor.Position)


            If Me.GetChildAtPoint(pos) Is Nothing Then
                m.Result = CType(HTTRANSPARENT, IntPtr)
                Return
            End If
        End If

        MyBase.WndProc(m)

    End Sub

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000
    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub
    Private Sub hub_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub
    ' ── Fields ─────────────────────────────────────────────────

    Private _settings As CaptureSettings
    Private _captureEngine As CaptureEngine
    Private _encoderDetector As EncoderDetector
    Private _hotkeyStartId As Integer = -1
    Private _hotkeyStopId As Integer = -1
    Private _configPath As String
    Private _isLoaded As Boolean = False

    ' ✅ P2: cache last-loaded Overlay config so we can detect changes
    Private _overlayConfig As OverlayConfig.AppConfig
    Private _overlayVideo As OverlayConfig.VideoConfig
    Private _lastConfigWrite As DateTime = DateTime.MinValue
    Private _lastVideoWrite As DateTime = DateTime.MinValue

    ' ── Form Load / Close ──────────────────────────────────────

    Private Sub UI_Engine_Load(sender As Object, e As EventArgs) Handles Me.Load
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shadowplay-config.json")

        ' ✅ P2: locate Overlay's config.json + video.json first.
        ' lblConfigSource will show where they were found.
        RefreshOverlayConfigUI()
        lblConfigSource.Text = "Config: " & If(OverlayConfig.IsAvailable,
                                               OverlayConfig.ConfigPath,
                                               "(not found — using Engine defaults)")

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

        ' ✅ P2: refresh Overlay config every 2s to detect changes
        AddHandler tmrRefresh.Tick, AddressOf OnRefreshTick
        tmrRefresh.Start()

        _isLoaded = True
    End Sub

    Private Sub UI_Engine_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If _captureEngine IsNot Nothing AndAlso _captureEngine.IsRecording Then
            _captureEngine.ForceStop()
        End If
        SaveSettings()
        If tcp IsNot Nothing Then
            tcp.Disconnect()
            tcp.Dispose()
        End If
        If _captureEngine IsNot Nothing Then _captureEngine.Dispose()
        ' ✅ P1: flush all queued log writers so no log lines are lost on exit.
        BackgroundLogger.ShutdownAll()
    End Sub

    ' ── Hub Client (uses shared TcpClientHelper — see [Overlay] Client.vb) ──

    Private Sub StartHubClient()
        StartTcpClient()
        ' Give the TCP helper a moment to connect, then broadcast engine_ready.
        ' If not connected yet, the TCP helper will fire OnReconnecting; we'll
        ' broadcast engine_ready on the next successful register attempt via
        ' a one-shot timer.
        Dim t As New System.Windows.Forms.Timer With {.Interval = 500}
        AddHandler t.Tick, Sub(s, ev)
                               t.Stop()
                               t.Dispose()
                               BroadcastEngineReady()
                               ' Update UI
                               If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                               End If
                           End Sub
        t.Start()
    End Sub

    Private Sub OnHubLog(sender As Object, message As String)
        DebugLog(message)
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════
    ' ✅ P2: Overlay config synchronization
    ' ═══════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Refresh the UI from Overlay's config.json + video.json.
    ''' Called on Form Load and every 2s by tmrRefresh.
    ''' </summary>
    Private Sub RefreshOverlayConfigUI()
        Try
            ' Re-resolve path in case Overlay just started after Engine
            If Not OverlayConfig.IsAvailable Then
                OverlayConfig.ResetResolvedPath()
            End If

            _overlayConfig = OverlayConfig.LoadConfig()
            _overlayVideo = OverlayConfig.LoadVideoConfig()

            ' ─── video.json → UI ───
            If _overlayVideo IsNot Nothing Then
                ' Encoder (display label)
                lblPresetValue.Text = If(_overlayVideo.active_preset, "(none)")
                lblNvencPreset.Text = "NVENC: " & OverlayConfig.MapNvencPreset(_overlayVideo.current.encoder_preset)

                ' FPS / Bitrate (read-only mirror)
                If _overlayVideo.current.fps > 0 AndAlso _overlayVideo.current.fps <= 240 Then
                    nudFPS.Value = _overlayVideo.current.fps
                End If
                ' ✅ P2.6: clamp to nudBitrate's min/max (1-200 Mbps) to avoid
                ' ArgumentOutOfRangeException if Overlay's bitrate is out of range.
                If _overlayVideo.current.bitrate > 0 Then
                    Dim mbps As Integer = CInt(_overlayVideo.current.bitrate / 1000)
                    mbps = Math.Max(1, Math.Min(200, mbps))
                    nudBitrate.Value = mbps
                End If

                ' Resolution
                chkNativeRes.Checked = _overlayVideo.current.use_native_resolution
                cboResolution.Enabled = Not _overlayVideo.current.use_native_resolution
                If Not _overlayVideo.current.use_native_resolution AndAlso _overlayVideo.current.width > 0 Then
                    lblNvencPreset.Text &= $" | {_overlayVideo.current.width}x{_overlayVideo.current.height}"
                End If

                ' Replay duration
                If _overlayVideo.replay_duration >= 15 AndAlso _overlayVideo.replay_duration <= 1200 Then
                    nudReplayDuration.Value = _overlayVideo.replay_duration
                End If

                ' Audio
                chkSysAudio.Checked = _overlayVideo.audio.system_enabled
                chkMic.Checked = _overlayVideo.audio.mic_enabled
                trkSysVol.Value = CInt(Math.Max(0, Math.Min(100, _overlayVideo.audio.system_volume * 100)))
                trkMicVol.Value = CInt(Math.Max(0, Math.Min(100, _overlayVideo.audio.mic_volume * 100)))
                lblSysVol.Text = "Sys Vol: " & trkSysVol.Value & "%"
                lblMicVol.Text = "Mic Vol: " & trkMicVol.Value & "%"
                If Not String.IsNullOrEmpty(_overlayVideo.audio.mic_device) Then
                    lblMicDevice.Text = "Mic: " & _overlayVideo.audio.mic_device
                Else
                    lblMicDevice.Text = "Mic: (default)"
                End If

                ' Encoder dropdown
                UpdateEncoderDropdownFromOverlay(_overlayVideo.encoder)
            End If

            ' ─── config.json → UI ───
            If _overlayConfig IsNot Nothing Then
                ' Output directory
                Dim galleryPath As String = _overlayConfig.Paths.GalleryPath
                If String.IsNullOrEmpty(galleryPath) Then galleryPath = _overlayConfig.Paths.SavePath
                txtOutputDir.Text = If(String.IsNullOrEmpty(galleryPath), "(not set)", galleryPath)

                ' FFmpeg path
                txtFFmpegPath.Text = If(_overlayConfig.Paths.FFmpegPath, "(not set)")
                ValidateFFmpegPath()

                ' GitHub user
                If _overlayConfig.GitHubUser.IsLoggedIn AndAlso Not String.IsNullOrEmpty(_overlayConfig.GitHubUser.Username) Then
                    lblGitHubUser.Text = _overlayConfig.GitHubUser.Username
                    lblGitHubStatus.Text = "Status: logged in"
                    lblGitHubStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                Else
                    lblGitHubUser.Text = "(not logged in)"
                    lblGitHubStatus.Text = "Status: not logged in"
                    lblGitHubStatus.ForeColor = Drawing.Color.FromArgb(160, 160, 160)
                End If
            End If

            ' Config source label
            lblConfigSource.Text = "Config: " & If(OverlayConfig.IsAvailable,
                                                   OverlayConfig.ConfigPath,
                                                   "(not found)")

        Catch ex As Exception
            DebugLog("RefreshOverlayConfigUI error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Update the encoder dropdown to reflect the Overlay-selected encoder.
    ''' Called from RefreshOverlayConfigUI.
    ''' </summary>
    Private Sub UpdateEncoderDropdownFromOverlay(overlayEncoder As String)
        If String.IsNullOrEmpty(overlayEncoder) Then Return
        Dim ffmpegEncoder As String = OverlayConfig.MapEncoderToFfmpeg(overlayEncoder)
        ' Find the dropdown item that starts with this encoder id
        For i As Integer = 0 To cboEncoder.Items.Count - 1
            Dim item As String = cboEncoder.Items(i).ToString()
            If item.IndexOf(ffmpegEncoder, StringComparison.OrdinalIgnoreCase) >= 0 Then
                If cboEncoder.SelectedIndex <> i Then
                    cboEncoder.SelectedIndex = i
                End If
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' Timer tick: check if Overlay's config files changed on disk.
    ''' Uses LastWriteTime to avoid re-reading + re-parsing on every tick.
    ''' </summary>
    Private Sub OnRefreshTick(sender As Object, e As EventArgs)
        Try
            Dim configChanged As Boolean = False
            Dim videoChanged As Boolean = False

            Dim configPath As String = OverlayConfig.ConfigPath
            Dim videoPath As String = OverlayConfig.VideoConfigPath

            If configPath.Length > 0 AndAlso File.Exists(configPath) Then
                Dim wt As DateTime = File.GetLastWriteTime(configPath)
                If wt <> _lastConfigWrite Then
                    _lastConfigWrite = wt
                    configChanged = True
                End If
            End If

            If videoPath.Length > 0 AndAlso File.Exists(videoPath) Then
                Dim wt As DateTime = File.GetLastWriteTime(videoPath)
                If wt <> _lastVideoWrite Then
                    _lastVideoWrite = wt
                    videoChanged = True
                End If
            End If

            ' If config was not found before, retry path resolution every tick
            If Not OverlayConfig.IsAvailable Then
                OverlayConfig.ResetResolvedPath()
                If OverlayConfig.IsAvailable Then
                    configChanged = True
                    videoChanged = True
                End If
            End If

            If configChanged OrElse videoChanged Then
                DebugLog($"[Engine] Overlay config changed (config={configChanged}, video={videoChanged}) → refreshing UI")
                RefreshOverlayConfigUI()
            End If

            ' Update Hub status panel
            UpdateHubStatusUI()
        Catch ex As Exception
            ' Don't let timer exceptions crash the form
            DebugLog("OnRefreshTick error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Update the Hub status panel from the TcpClientHelper.
    ''' </summary>
    Private Sub UpdateHubStatusUI()
        If tcp Is Nothing Then
            lblHubStatus.Text = "not started"
            lblHubClients.Text = "Engine: offline"
            Return
        End If

        If tcp.IsConnected Then
            lblHubStatus.Text = "connected (port 5000)"
            lblHubStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
            lblHubClients.Text = "Engine: online"
        Else
            lblHubStatus.Text = "disconnected — reconnecting..."
            lblHubStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
            lblHubClients.Text = "Engine: offline"
        End If
    End Sub

    ' ═══════════════════════════════════════════════════════════════════════
    ' ✅ P2.3: command dispatch moved to [Overlay] Client.vb (DispatchEngineCommand)
    ' — uses the shared TcpClientHelper, same as Overlay.
    ' ═══════════════════════════════════════════════════════════════════════

    ' ── Engine Command Handlers (TCP แค่ on/off) ──────────

    Private Async Function HandleEngineRecordStart(value As String, reqId As String) As Task
        Try
            DebugLog($"[Engine] HandleEngineRecordStart: path={value}, reqId={If(String.IsNullOrEmpty(reqId), "(none)", reqId)}")

            ' ✅ P2.6: load settings from Overlay's config.json + video.json
            ' (source of truth). Old code loaded from Engine's shadowplay-config.json
            ' which drifted out of sync with Overlay → wrong encoder/fps/bitrate.
            ' Now we merge Overlay's video.json into CaptureSettings so the
            ' recording uses exactly what the user picked in Overlay's UI.
            Dim s As CaptureSettings = CaptureSettings.Load(_configPath)
            SyncWithOverlayConfig(s)
            _settings = s
            DebugLog($"[Engine] config loaded (synced with Overlay): FFmpegPath={If(_settings.FFmpegPath, "(empty)")}, Encoder={If(_settings.Encoder, "(empty)")}, CaptureMethod={_settings.CaptureMethod}, FPS={_settings.FPS}, Bitrate={_settings.Bitrate}")

            ' ✅ P1.6: if FFmpegPath is empty or doesn't exist, respond with
            ' a clear error immediately. Old code let StartRecordingAsync run
            ' and fail inside Validate() — but the error path was unreliable
            ' (Async Sub + event-based ErrorOccurred). Now we fail fast and
            ' send the response ourselves.
            If String.IsNullOrEmpty(_settings.FFmpegPath) OrElse Not IO.File.Exists(_settings.FFmpegPath) Then
                DebugLog($"[Engine] RECORD_START rejected: FFmpegPath invalid ('{_settings.FFmpegPath}')")
                SendResponse("engine_record_start", "error", "ffmpeg_not_found", reqId)
                Return
            End If

            If String.IsNullOrEmpty(_settings.Encoder) Then
                DebugLog("[Engine] RECORD_START rejected: Encoder not set")
                SendResponse("engine_record_start", "error", "no_encoder_selected", reqId)
                Return
            End If

            If Not IO.Directory.Exists(_settings.OutputDirectory) Then
                IO.Directory.CreateDirectory(_settings.OutputDirectory)
            End If

            _captureEngine?.Dispose()
            _captureEngine = New CaptureEngine(_settings)
            AddHandler _captureEngine.StateChanged, AddressOf OnEngineStateChanged
            AddHandler _captureEngine.RecordingStarted, AddressOf OnRecordingStarted
            AddHandler _captureEngine.RecordingStopped, AddressOf OnRecordingStopped
            AddHandler _captureEngine.ErrorOccurred, AddressOf OnEngineError
            AddHandler _captureEngine.ProgressUpdated, AddressOf OnEngineProgress

            DebugLog($"[Engine] starting CaptureEngine with override path: {value}")
            Dim ok As Boolean = Await _captureEngine.StartRecordingAsync(value)
            DebugLog($"[Engine] StartRecordingAsync returned: {ok}")

            If ok Then
                ' ✅ P2.6: caller already marshals via BeginUiInvoke, so we're
                ' on the UI thread. Direct UI access is safe.
                lblStatus.Text = "Recording (Hub)..."
                lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                tmrRecording.Start()
                btnRecord.Enabled = False
                btnStop.Enabled = True
                SendResponse("engine_record_start", "ok", _captureEngine.OutputFile, reqId)
            Else
                SendResponse("engine_record_start", "error", "start_failed", reqId)
            End If
        Catch ex As Exception
            DebugLog("RecordStart error: " & ex.Message)
            SendResponse("engine_record_start", "error", ex.Message, reqId)
        End Try
    End Function

    ''' <summary>
    ''' ✅ P2.6: copy values from Overlay's video.json + config.json into
    ''' Engine's CaptureSettings. This is what makes Engine actually use the
    ''' encoder/fps/bitrate the user picked in Overlay's UI.
    ''' </summary>
    Private Sub SyncWithOverlayConfig(s As CaptureSettings)
        Try
            Dim video As OverlayConfig.VideoConfig = OverlayConfig.LoadVideoConfig()
            Dim appCfg As OverlayConfig.AppConfig = OverlayConfig.LoadConfig()

            If video IsNot Nothing Then
                ' Encoder: Overlay uses 'NVENC_H264' etc., FFmpeg uses 'h264_nvenc'.
                s.Encoder = OverlayConfig.MapEncoderToFfmpeg(video.encoder)

                ' FPS / Bitrate from current preset values.
                If video.current.fps > 0 AndAlso video.current.fps <= 240 Then
                    s.FPS = video.current.fps
                End If
                If video.current.bitrate > 0 Then
                    ' Overlay stores bitrate in kbps, CaptureSettings expects bps.
                    s.Bitrate = video.current.bitrate * 1000L
                End If

                ' Resolution.
                s.UseNativeResolution = video.current.use_native_resolution
                If Not video.current.use_native_resolution Then
                    s.CustomWidth = video.current.width
                    s.CustomHeight = video.current.height
                End If

                ' Capture method from api_capture (Overlay's name).
                If Not String.IsNullOrEmpty(video.api_capture) Then
                    s.CaptureMethod = video.api_capture.ToLowerInvariant()
                End If

                ' ✅ Audio Capture — sync from Overlay's video.json.audio
                s.SystemAudioCapture = video.audio.system_enabled
                s.MicCapture = video.audio.mic_enabled
                s.SystemAudioVolume = video.audio.system_volume
                s.MicVolume = video.audio.mic_volume
                s.MicDeviceName = video.audio.mic_device
                s.AudioCapture = s.SystemAudioCapture OrElse s.MicCapture
            End If

            If appCfg IsNot Nothing Then
                ' FFmpegPath: prefer Overlay's path (it's the bundler).
                If Not String.IsNullOrEmpty(appCfg.Paths.FFmpegPath) AndAlso IO.File.Exists(appCfg.Paths.FFmpegPath) Then
                    s.FFmpegPath = appCfg.Paths.FFmpegPath
                End If

                ' OutputDirectory: prefer GalleryPath, fallback to SavePath.
                Dim outDir As String = appCfg.Paths.GalleryPath
                If String.IsNullOrEmpty(outDir) Then outDir = appCfg.Paths.SavePath
                If Not String.IsNullOrEmpty(outDir) Then
                    s.OutputDirectory = outDir
                End If
            End If

            ' Persist back to shadowplay-config.json so old code paths still work.
            s.Save(_configPath)
        Catch ex As Exception
            DebugLog($"SyncWithOverlayConfig error: {ex.Message}")
        End Try
    End Sub

    Private Async Function HandleEngineRecordStop(reqId As String) As Task
        Try
            If _captureEngine Is Nothing OrElse Not _captureEngine.IsRecording Then
                SendResponse("engine_record_stop", "error", "not_recording", reqId)
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

            SendResponse("engine_record_stop", "ok", _captureEngine.OutputFile, reqId)
        Catch ex As Exception
            SendResponse("engine_record_stop", "error", ex.Message, reqId)
        End Try
    End Function

    Private Sub HandleEngineReplayStart(value As String, reqId As String)
        SendResponse("engine_replay_start", "error", "not_implemented", reqId)
    End Sub

    Private Sub HandleEngineReplayStop(reqId As String)
        SendResponse("engine_replay_stop", "error", "not_implemented", reqId)
    End Sub

    Private Sub HandleEngineReplaySave(value As String, reqId As String)
        SendResponse("engine_replay_save", "error", "not_implemented", reqId)
    End Sub

    Private Sub HandleEngineGetStatus(reqId As String)
        Dim state As String = "Idle"
        If _captureEngine IsNot Nothing AndAlso _captureEngine.IsRecording Then
            state = "Recording"
        End If
        SendResponse("engine_get_status", "ok", state, reqId)
    End Sub

    Private Sub HandleEngineLoadConfig(reqId As String)
        Try
            _settings = CaptureSettings.Load(_configPath)
            SendResponse("engine_load_config", "ok", Nothing, reqId)
        Catch ex As Exception
            SendResponse("engine_load_config", "error", ex.Message, reqId)
        End Try
    End Sub

    Private Sub HandleEngineSetEncoder(value As String, reqId As String)
        SendResponse("engine_set_encoder", "error", "use_config_json", reqId)
    End Sub

    ''' <summary>
    ''' ✅ P1.5: Overlay sends PREWARM_FFMPEG with the path to ffmpeg.exe in
    ''' Overlay's api-core folder. Engine should use this path if it doesn't
    ''' have its own ffmpeg.exe. Without this, Engine's CaptureSettings might
    ''' point to a non-existent ffmpeg.exe → StartRecordingAsync fails →
    ''' "กดอัด ขึ้นอัด แต่ไม่ได้อัดจริง".
    '''
    ''' ✅ P2.6: simplified — caller (OnTcpMessage) now marshals via BeginUiInvoke
    ''' so this method always runs on the UI thread. Removed the inner Me.Invoke.
    ''' </summary>
    Private Sub HandleEnginePrewarmFFmpeg(ffmpegPath As String)
        Try
            DebugLog($"[Engine] PREWARM_FFMPEG received: {ffmpegPath}")

            If String.IsNullOrEmpty(ffmpegPath) OrElse Not IO.File.Exists(ffmpegPath) Then
                DebugLog($"[Engine] PREWARM_FFMPEG: path does not exist: {ffmpegPath}")
                Return
            End If

            ' ✅ P2.6: caller marshals via BeginUiInvoke, so we're on the UI thread.
            ' Load current settings, update FFmpegPath, save back.
            Dim s As CaptureSettings = CaptureSettings.Load(_configPath)
            If String.IsNullOrEmpty(s.FFmpegPath) OrElse Not IO.File.Exists(s.FFmpegPath) Then
                s.FFmpegPath = ffmpegPath
                s.Save(_configPath)
                _settings = s
                txtFFmpegPath.Text = ffmpegPath
                ValidateFFmpegPath()
                DebugLog($"[Engine] PREWARM_FFMPEG: updated FFmpegPath → {ffmpegPath}")
                ' Re-detect encoders with the new ffmpeg path.
                DetectEncoders()
            Else
                DebugLog($"[Engine] PREWARM_FFMPEG: already have valid FFmpegPath: {s.FFmpegPath}")
            End If
        Catch ex As Exception
            DebugLog($"[Engine] PREWARM_FFMPEG error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' ✅ P2: Overlay broadcast engine_config_changed after saving video.json
    ''' or config.json. Engine reloads its UI from the files immediately.
    ''' value = "video" | "config" | "" (empty = reload both)
    ''' ✅ P2.6: caller marshals via BeginUiInvoke, so we're already on UI thread.
    ''' </summary>
    Private Sub HandleEngineConfigChanged(scope As String)
        Try
            DebugLog($"[Engine] engine_config_changed received (scope={scope})")
            ' Force path re-resolution in case Overlay moved files
            OverlayConfig.ResetResolvedPath()
            ' Refresh UI (we're on the UI thread already).
            RefreshOverlayConfigUI()
        Catch ex As Exception
            DebugLog($"[Engine] engine_config_changed error: {ex.Message}")
        End Try
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
        AddHandler _captureEngine.ProgressUpdated, AddressOf OnEngineProgress
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

    ' ✅ C4 FIX: convert OnRecordClick/OnStopClick to Async Sub so we can
    ' Await StartRecordingAsync/StopRecordingAsync instead of .Wait().
    ' .Wait() is sync-over-async: burns a threadpool thread for the duration
    ' and wraps exceptions in AggregateException (misleading error messages).

    Private Async Sub OnRecordClick(sender As Object, e As EventArgs)
        SaveSettings()
        btnRecord.Enabled = False
        btnStop.Enabled = True

        Try
            ' ✅ P2.6: sync with Overlay config before recording
            ' (same as HandleEngineRecordStart does for TCP-triggered recordings).
            Dim s As CaptureSettings = CaptureSettings.Load(_configPath)
            SyncWithOverlayConfig(s)
            _settings = s

            Dim engine As New CaptureEngine(_settings)
            AddHandler engine.StateChanged, AddressOf OnEngineStateChanged
            AddHandler engine.RecordingStarted, AddressOf OnRecordingStarted
            AddHandler engine.RecordingStopped, AddressOf OnRecordingStopped
            AddHandler engine.ErrorOccurred, AddressOf OnEngineError
            ' ✅ C2 FIX: wire ProgressUpdated so button-triggered recordings also
            ' update the status panel + broadcast to Overlay. Was missing — only
            ' TCP-triggered recordings had it.
            AddHandler engine.ProgressUpdated, AddressOf OnEngineProgress

            _captureEngine?.Dispose()
            _captureEngine = engine

            Await _captureEngine.StartRecordingAsync()
        Catch ex As Exception
            lblStatus.Text = "Error: " & ex.Message
            lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
            btnRecord.Enabled = True
            btnStop.Enabled = False
        End Try
    End Sub

    Private Async Sub OnStopClick(sender As Object, e As EventArgs)
        btnStop.Enabled = False

        Try
            Await _captureEngine.StopRecordingAsync()
            btnRecord.Enabled = True
        Catch
            btnRecord.Enabled = True
        End Try
    End Sub

    ' ── Engine Events ────────────────────────────────────────────

    Private Sub OnEngineStateChanged(state As CaptureEngine.CaptureState)
        ' ✅ P2.9: broadcast state change to Overlay so its UI stays in sync.
        ' Format: engine_state_changed:<state_name>
        ' Overlay uses this to reconcile its _isRecordingLocal flag.
        Dim stateName As String = state.ToString()
        Try
            If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                tcp.Send("engine_state_changed", stateName)
            End If
        Catch
        End Try

        ' ✅ C6 FIX: guard Me.Invoke with try/catch. This handler is called from
        ' FFmpeg's Exited threadpool thread; if the form is closing (IsDisposed=True
        ' or handle being destroyed), Me.Invoke throws InvalidOperationException
        ' which would propagate out and crash the app.
        SafeInvoke(Sub()
                       Select Case state
                           Case CaptureEngine.CaptureState.Idle
                               lblStatus.Text = "Idle - Hub Client"
                               lblStatus.ForeColor = Drawing.Color.FromArgb(160, 160, 160)
                               tmrRecording.Stop()
                               ' ✅ P2.10: update status panel
                               lblRecState.Text = "● Idle"
                               lblRecState.ForeColor = Drawing.Color.FromArgb(160, 160, 160)

                           Case CaptureEngine.CaptureState.Recording
                               lblStatus.Text = "Recording..."
                               lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                               lblTimer.ForeColor = Drawing.Color.Red
                               tmrRecording.Start()
                               ' ✅ P2.10: update status panel
                               lblRecState.Text = "● Recording"
                               lblRecState.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                               ' Show target bitrate so user can compare with actual.
                               If _settings IsNot Nothing AndAlso _settings.Bitrate > 0 Then
                                   lblRecBitrate.Text = $"Target: {(_settings.Bitrate / 1000000.0):F1} Mbps · {_settings.FPS} FPS · p{_settings.NvencPreset}"
                               End If

                           Case CaptureEngine.CaptureState.Stopping
                               lblStatus.Text = "Stopping..."
                               lblStatus.ForeColor = Drawing.Color.FromArgb(255, 200, 50)
                               tmrRecording.Stop()
                               ' ✅ P2.10: update status panel
                               lblRecState.Text = "● Stopping..."
                               lblRecState.ForeColor = Drawing.Color.FromArgb(255, 200, 50)

                           Case CaptureEngine.CaptureState.HasError
                               lblStatus.Text = "Error"
                               lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                               tmrRecording.Stop()
                               btnRecord.Enabled = True
                               btnStop.Enabled = False
                               ' ✅ P2.10: update status panel
                               lblRecState.Text = "● Error"
                               lblRecState.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                       End Select
                   End Sub)
    End Sub

    Private Sub OnRecordingStarted(filename As String)
        SafeInvoke(Sub()
                       lblStatus.Text = "Recording: " & Path.GetFileName(filename)
                   End Sub)
    End Sub

    Private Sub OnRecordingStopped(filename As String)
        ' ✅ P2.9: broadcast saved file path to Overlay.
        Try
            If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                tcp.Send("engine_recording_saved", filename)
            End If
        Catch
        End Try

        SafeInvoke(Sub()
                       tmrRecording.Stop()
                       lblTimer.Text = "00:00:00"
                       lblTimer.ForeColor = _accentGreen
                       btnRecord.Enabled = True
                       lblStatus.Text = "Saved: " & Path.GetFileName(filename)
                       lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                   End Sub)
    End Sub

    Private Sub OnEngineError(message As String)
        ' ✅ P2.9: broadcast error to Overlay so it can show a toast.
        Try
            If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                tcp.Send("engine_recording_error", message)
            End If
        Catch
        End Try

        SafeInvoke(Sub()
                       lblStatus.Text = "Error: " & message
                       lblStatus.ForeColor = Drawing.Color.FromArgb(200, 50, 50)
                       btnRecord.Enabled = True
                       btnStop.Enabled = False
                   End Sub)
    End Sub

    ''' <summary>
    ' ✅ C6 FIX: safe Me.Invoke wrapper. Uses BeginInvoke (fire-and-forget) so
    ' the calling thread (FFmpeg Exited, etc.) doesn't block. Catches
    ' InvalidOperationException (form handle not created) and ObjectDisposedException
    ' (form already disposed) silently — these happen during shutdown and are
    ' expected, not errors.
    ''' </summary>
    Private Sub SafeInvoke(action As Action)
        Try
            If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
            Me.BeginInvoke(action)
        Catch ex As InvalidOperationException
            ' Form handle not created yet — drop the update.
        Catch ex As ObjectDisposedException
            ' Form already disposed — drop the update.
        Catch
        End Try
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

    ' ✅ P2.9: progress event from CaptureEngine — fired every FFmpeg stderr
    ' progress line (~1/sec at 60fps). Broadcast to Overlay so it can show
    ' real-time timer + file size in its UI.
    ' Format: engine_recording_progress:<duration_sec>|<frames>|<size_bytes>
    Private Sub OnEngineProgress(frames As Long, duration As TimeSpan, sizeBytes As Long)
        Try
            ' ✅ P2.10: update local UI status panel.
            If Me.IsHandleCreated AndAlso Not Me.IsDisposed Then
                Me.BeginInvoke(Sub()
                                   ' Timer
                                   lblTimer.Text = duration.ToString("hh\:mm\:ss")
                                   ' File size
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
                                   lblRecSize.Text = sizeStr
                                   ' Frame count
                                   lblRecFrames.Text = $"{frames:N0} frames"
                                   ' Actual bitrate (size / duration)
                                   If duration.TotalSeconds > 0 Then
                                       Dim actualMbps As Double = (sizeBytes * 8.0) / (duration.TotalSeconds * 1000000.0)
                                       Dim targetStr As String = ""
                                       If _settings IsNot Nothing AndAlso _settings.Bitrate > 0 Then
                                           targetStr = $" / target {(_settings.Bitrate / 1000000.0):F1}"
                                       End If
                                       lblRecBitrate.Text = $"Actual: {actualMbps:F1} Mbps{targetStr}"
                                   End If
                               End Sub)
            End If

            ' Broadcast to Overlay.
            If tcp IsNot Nothing AndAlso tcp.IsConnected Then
                Dim sec As Integer = CInt(Math.Floor(duration.TotalSeconds))
                Dim value As String = $"{sec}|{frames}|{sizeBytes}"
                tcp.Send("engine_recording_progress", value)
            End If
        Catch
        End Try
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
        Else
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
    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Dim uiFile = Path.Combine(Application.StartupPath, "Engine.UI")

        Try
            If File.Exists(uiFile) Then
                File.Delete(uiFile)
            End If
        Catch ex As IOException
        End Try
    End Sub

    Private Sub OPEN_UI_Tick(sender As Object, e As EventArgs) Handles OPEN_UI.Tick
        HideFromAltTab()

        Dim uiFile = Path.Combine(Application.StartupPath, "Engine.UI")

        If File.Exists(uiFile) Then
            Me.Opacity = 1
            Me.WindowState = FormWindowState.Maximized
        Else
            Me.Opacity = 0
            Me.WindowState = FormWindowState.Minimized
        End If
    End Sub

    Private Sub BT_Back_MouseMove(sender As Object, e As MouseEventArgs) Handles BT_Back.MouseMove
        BT_Back.BackColor = Color.Green
    End Sub

    Private Sub BT_Back_MouseLeave(sender As Object, e As EventArgs) Handles BT_Back.MouseLeave
        BT_Back.BackColor = Color.FromArgb(118, 185, 0)
    End Sub
End Class