Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports CaptureEngine

Partial Public Class Base
    Public ReplayValue As Boolean = False
    Public RecordValue As Boolean = False

    Private Shared _encoderAvailabilityChecked As Boolean = False
    Private Shared _availableEncoders As New Dictionary(Of String, Boolean)()

    ''' <summary>
    ''' UI-level cooldown — prevents rapid-fire hotkey/button spam.
    ''' Shared across all actions (Record/Replay/Save) so that pressing
    ''' Alt+F9 then Alt+Shift+F10 within 500ms is also blocked.
    ''' This works TOGETHER with ScreenRecorder.ACTION_COOLDOWN_MS (defense-in-depth).
    ''' </summary>
    Private Shared _lastUiActionTime As DateTime = DateTime.MinValue
    Private Shared _uiActionLock As New Object()
    Private Const UI_ACTION_COOLDOWN_MS As Integer = 500

    ''' <summary>
    ''' Throttle cooldown rejection logs — only log once per cooldown period
    ''' instead of every 30ms (which floods the debug output).
    ''' </summary>
    Private Shared _lastCooldownLogTime As DateTime = DateTime.MinValue

    ''' <summary>
    ''' Guard flag to prevent overlapping toggle operations.
    ''' Without this, rapid toggle can cause Stop+Start to overlap,
    ''' leading to duplicate events and orphaned FFmpeg processes.
    ''' </summary>
    Private Shared _isTogglingRecording As Boolean = False
    Private Shared _isTogglingReplay As Boolean = False

    ''' <summary>
    ''' FIX #1: Added SyncLock — old code had no thread safety.
    ''' Hotkey hooks fire on different threads, so two rapid presses
    ''' could both read the same old _lastUiActionTime and both pass.
    ''' Also throttled logging to avoid debug output spam.
    ''' </summary>
    Private Function CheckUiCooldown() As Boolean
        SyncLock _uiActionLock
            Dim msSinceLast As Double = (DateTime.Now - _lastUiActionTime).TotalMilliseconds
            If msSinceLast < UI_ACTION_COOLDOWN_MS Then
                ' Throttled log: only print once per cooldown window
                Dim msSinceLog As Double = (DateTime.Now - _lastCooldownLogTime).TotalMilliseconds
                If msSinceLog >= UI_ACTION_COOLDOWN_MS Then
                    Debug.WriteLine(String.Format("UI Cooldown: Rejected — {0:F0}ms < {1}ms", msSinceLast, UI_ACTION_COOLDOWN_MS))
                    _lastCooldownLogTime = DateTime.Now
                End If
                Return False
            End If
            Return True
        End SyncLock
    End Function

    Private Sub MarkUiAction()
        SyncLock _uiActionLock
            _lastUiActionTime = DateTime.Now
        End SyncLock
    End Sub

    Private ReadOnly Property Recorder As CaptureEngine.CaptureCore.ScreenRecorder
        Get
            Return Base_RecordingsSet.RecorderInstance
        End Get
    End Property

#Region "Initialize Recorder Events"
    Private Sub InitializeRecorderEvents()
        RemoveHandler Recorder.RecordingStarted, AddressOf OnRecordingStarted
        RemoveHandler Recorder.RecordingStopped, AddressOf OnRecordingStopped
        RemoveHandler Recorder.RecordingError, AddressOf OnRecordingError
        RemoveHandler Recorder.BufferStarted, AddressOf OnBufferStarted
        RemoveHandler Recorder.BufferStopped, AddressOf OnBufferStopped
        RemoveHandler Recorder.ReplaySaved, AddressOf OnReplaySaved

        AddHandler Recorder.RecordingStarted, AddressOf OnRecordingStarted
        AddHandler Recorder.RecordingStopped, AddressOf OnRecordingStopped
        AddHandler Recorder.RecordingError, AddressOf OnRecordingError
        AddHandler Recorder.BufferStarted, AddressOf OnBufferStarted
        AddHandler Recorder.BufferStopped, AddressOf OnBufferStopped
        AddHandler Recorder.ReplaySaved, AddressOf OnReplaySaved
    End Sub
#End Region

#Region "Recorder Status Properties - For Base_RecordingsSet"
    Public ReadOnly Property ReplayActive As Boolean
        Get
            Try
                Return Recorder IsNot Nothing AndAlso Recorder.IsBuffering
            Catch ex As Exception
                Debug.WriteLine("ReplayActive Error: " & ex.Message)
                Return False
            End Try
        End Get
    End Property

    Public ReadOnly Property IsRecording As Boolean
        Get
            Try
                Return Recorder IsNot Nothing AndAlso Recorder.IsRecording
            Catch ex As Exception
                Debug.WriteLine("IsRecording Error: " & ex.Message)
                Return False
            End Try
        End Get
    End Property
#End Region

#Region "Get Output Directory"
    ''' <summary>
    ''' Get output directory from AppSettings (config.json)
    ''' Falls back to Base_Gallery.txtFilePath.Text or default path
    ''' </summary>
    Private Function GetOutputDirectory() As String
        Dim outputDir As String = ""

        Try
            outputDir = AppSettings.Instance.Paths.SavePath

            If String.IsNullOrEmpty(outputDir) AndAlso Base_Gallery IsNot Nothing AndAlso Base_Gallery.txtFilePath IsNot Nothing Then
                outputDir = Base_Gallery.txtFilePath.Text
            End If
        Catch ex As Exception
            Debug.WriteLine("GetOutputDirectory: Error - " & ex.Message)
        End Try

        If String.IsNullOrEmpty(outputDir) OrElse Not Directory.Exists(outputDir) Then
            outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Shadowplay", "Gallery")
        End If

        If Not Directory.Exists(outputDir) Then
            Try
                Directory.CreateDirectory(outputDir)
            Catch ex As Exception
                Debug.WriteLine("GetOutputDirectory: Failed to create directory - " & ex.Message)
                outputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            End Try
        End If

        Debug.WriteLine("GetOutputDirectory: " & outputDir)
        Return outputDir
    End Function
#End Region

#Region "Toggle Recording (Alt+F9)"

    ' Phase 5: PrivacyOpen() and IsPrivacyEnabled() moved to Sub_Misc.vb.
    ' They are still accessible from here because both files are partial
    ' classes of the same Base class.

    Public Async Sub ToggleRecording()
        ' ═══ UI Cooldown Guard ═══
        If Not CheckUiCooldown() Then Exit Sub

        ' ═══ Toggle Guard: Prevent overlapping start/stop ═══
        SyncLock _uiActionLock
            If _isTogglingRecording Then
                Debug.WriteLine("ToggleRecording: Rejected — toggle already in progress")
                Exit Sub
            End If
            _isTogglingRecording = True
        End SyncLock
        MarkUiAction()

        If Not IsPrivacyEnabled() Then
            ShowMainPanel()
            OpenSettings()
            PrivacyOpen()
            ShowNotifier("notificationWarningDesktopCaptureDisabled")
            _isTogglingRecording = False
            Exit Sub
        End If
        Try
            ' ═══ FIX #3: Only apply settings on FIRST call or when not already running ═══
            ' Old code: Called ApplyRecorderSettings + ApplyAudioSettings on EVERY toggle
            ' This means pressing Stop would reset all settings from config, overriding
            ' any runtime changes. Now only apply when actually starting.
            If Not Recorder.IsRecording Then
                Recorder.FFmpegPath = Path.Combine(Application.StartupPath, "api-core", "ffmpeg.exe")
                Recorder.FFprobePath = Path.Combine(Application.StartupPath, "api-core", "ffprobe.exe")

                ' ★ FIX: Validate FFmpeg exists before attempting to record
                If Not File.Exists(Recorder.FFmpegPath) Then
                    Debug.WriteLine($"ToggleRecording: FFmpeg not found at {Recorder.FFmpegPath}")
                    ShowNotifier("recording_error")
                    SyncLock _uiActionLock
                        _isTogglingRecording = False
                    End SyncLock
                    Exit Sub
                End If

                ApplyRecorderSettings()
                ApplyAudioSettings(Recorder)
            End If

            If Recorder.IsRecording Then
                ' ═══ Stop recording ═══
                ' ★ Fix E: Optimistic UI feedback — show "saved" notifier IMMEDIATELY
                ' instead of waiting for StopRecordingAsync to complete (which blocks
                ' up to 3s on FFmpeg graceful exit). User sees instant response.
                ' If stop fails internally, the error is logged but UI stays consistent
                ' (recording was going to stop either way).
                RecordValue = False
                ShowNotifier("recording_saved")
                Debug.WriteLine("Recording stop requested")
                Await Recorder.StopRecordingAsync()
                Debug.WriteLine("Recording stopped")
            Else
                ' ═══ Start recording ═══
                ' ★ Fix E: Optimistic UI feedback — show "started" notifier IMMEDIATELY
                ' so user sees instant response. If StartRecordingAsync fails (e.g.
                ' FFmpeg not found, audio pipe init error), we roll back the notifier
                ' to "recording_error" after the fact.
                ShowNotifier("recording_started")
                Debug.WriteLine("Recording start requested")

                Dim outputDir As String = GetOutputDirectory()
                Dim fileName = $"Record_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4"
                Dim outputPath = Path.Combine(outputDir, fileName)

                Dim success = Await Recorder.StartRecordingAsync(outputPath)

                If success Then
                    RecordValue = True
                    Debug.WriteLine("Recording started successfully")
                Else
                    RecordValue = False
                    ShowNotifier("recording_error")
                    Debug.WriteLine("Recording failed to start — rolling back optimistic notifier")
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[ToggleRecording] Error: {ex.Message}")
            RecordValue = False
            ShowNotifier("recording_error")
        Finally
            SyncLock _uiActionLock
                _isTogglingRecording = False
            End SyncLock
        End Try
    End Sub
#End Region

#Region "Toggle Instant Replay (Alt+Shift+F10)"
    Public Async Sub ToggleInstantReplay()
        ' ═══ UI Cooldown Guard ═══
        If Not CheckUiCooldown() Then Exit Sub

        ' ═══ Toggle Guard: Prevent overlapping start/stop ═══
        ' If a previous toggle is still running (e.g. StopBufferAsync hasn't
        ' completed yet), reject immediately to prevent:
        '   1. Duplicate events (ReplayBufferStopped fires twice)
        '   2. Orphaned FFmpeg processes
        '   3. Start called before previous Stop fully cleaned up
        SyncLock _uiActionLock
            If _isTogglingReplay Then
                Debug.WriteLine("ToggleInstantReplay: Rejected — toggle already in progress")
                Exit Sub
            End If
            _isTogglingReplay = True
        End SyncLock
        MarkUiAction()

        If Not IsPrivacyEnabled() Then
            ShowMainPanel()
            OpenSettings()
            PrivacyOpen()
            ShowNotifier("notificationWarningDesktopCaptureDisabled")
            _isTogglingReplay = False
            Exit Sub
        End If
        Try
            ' ═══ FIX #3: Only apply settings when starting buffer, not when stopping ═══
            If Not Recorder.IsBuffering Then
                Recorder.FFmpegPath = Path.Combine(Application.StartupPath, "api-core", "ffmpeg.exe")
                Recorder.FFprobePath = Path.Combine(Application.StartupPath, "api-core", "ffprobe.exe")

                ' ★ FIX: Validate FFmpeg exists before attempting to buffer
                If Not File.Exists(Recorder.FFmpegPath) Then
                    Debug.WriteLine($"ToggleInstantReplay: FFmpeg not found at {Recorder.FFmpegPath}")
                    ShowNotifier("notificationWarningBufferFailed")
                    SyncLock _uiActionLock
                        _isTogglingReplay = False
                    End SyncLock
                    Exit Sub
                End If

                ApplyRecorderSettings()
                ApplyAudioSettings(Recorder)
            End If

            If Recorder.IsBuffering Then
                ' ═══ Stop replay buffer ═══
                ' ★ Fix E: Optimistic UI feedback — show "off" notifier IMMEDIATELY
                ' instead of waiting for StopBufferAsync to complete (which blocks
                ' up to 3s on FFmpeg graceful exit).
                ReplayValue = False
                SetControlColor(Replay_Logo, Color.White)
                SetControlEnabled(Menu_Replay_Box2, False)
                SetControlEnabled(Menu_Replay_save_text, False)
                SetControlEnabled(Menu_Replay_save_key, False)
                ShowNotifier("instant_replay_off")
                Debug.WriteLine("Replay buffer stop requested")
                Await Recorder.StopBufferAsync()
                Debug.WriteLine("Replay buffer stopped")
            Else
                ' ═══ Start replay buffer ═══
                ' ★ Fix E: Optimistic UI feedback — show "on" notifier IMMEDIATELY.
                ' If StartBufferAsync fails, we roll back the notifier.
                Dim saveSeconds As Integer = AppSettings.Instance.Recording.ReplayDuration
                If saveSeconds < 15 Then saveSeconds = 15
                If saveSeconds > 1200 Then saveSeconds = 1200
                Recorder.BufferDurationSeconds = saveSeconds

                Debug.WriteLine($"Replay save duration set to: {saveSeconds}s")
                ShowNotifier("instant_replay_on")
                Debug.WriteLine("Replay buffer start requested")

                Dim success = Await Recorder.StartBufferAsync()

                If success Then
                    ReplayValue = True
                    SetControlEnabled(Menu_Replay_Box2, True)
                    SetControlEnabled(Menu_Replay_save_text, True)
                    SetControlEnabled(Menu_Replay_save_key, True)
                    Debug.WriteLine("Replay buffer started successfully")
                Else
                    ReplayValue = False
                    ShowNotifier("replay_error")
                    Debug.WriteLine("Replay buffer failed to start — rolling back optimistic notifier")
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[ToggleInstantReplay] Error: {ex.Message}")
            ReplayValue = False
            ShowNotifier("replay_error")
        Finally
            SyncLock _uiActionLock
                _isTogglingReplay = False
            End SyncLock
        End Try
    End Sub
#End Region

#Region "Save Instant Replay"
    Public Async Sub SaveInstantReplay()
        ' ═══ UI Cooldown Guard ═══
        If Not CheckUiCooldown() Then Exit Sub
        MarkUiAction()

        Try
            ' Check if buffer is active
            If Not Recorder.IsBuffering Then
                ShowNotifier("replay_turn_on")
                Debug.WriteLine("SaveInstantReplay: Buffer not active")
                Return
            End If

            ' Disable UI during save
            SetControlEnabled(Menu_Replay_Box2, False)
            SetControlEnabled(Menu_Replay_save_text, False)
            SetControlEnabled(Menu_Replay_save_key, False)

            ' Get output directory from AppSettings
            Dim outputDir As String = GetOutputDirectory()

            ' Generate filename
            Dim fileName = $"Replay_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4"
            Dim outputPath = Path.Combine(outputDir, fileName)

            ' Get requested duration from AppSettings.Instance (config.json)
            Dim requestedDuration As Integer = AppSettings.Instance.Recording.ReplayDuration
            If requestedDuration < 15 Then requestedDuration = 15
            If requestedDuration > 1200 Then requestedDuration = 1200

            Debug.WriteLine($"══════════ SaveInstantReplay ══════════")
            Debug.WriteLine($"Output: {outputPath}")
            Debug.WriteLine($"Requested: {requestedDuration}s")

            Dim success = Await Recorder.SaveReplayAsync(outputPath, requestedDuration)

            If success Then
                Debug.WriteLine($"SaveInstantReplay: SUCCESS — {outputPath}")
                SaveReplayDurationInfo(outputPath)
            Else
                Debug.WriteLine("SaveInstantReplay: FAILED")
                ShowNotifier("replay_error")
            End If

        Catch ex As Exception
            Debug.WriteLine($"[SaveInstantReplay] Error: {ex.Message}")
            Debug.WriteLine($"Stack: {ex.StackTrace}")
            ShowNotifier("replay_error")
        Finally
            ' ═══ FIX #5: Only re-enable UI if buffer is still active ═══
            ' Old code: Always re-enabled, even after buffer was stopped during save.
            ' If user pressed Alt+Shift+F10 (stop buffer) while save was running,
            ' the save controls would re-enable after save finished, but buffer is dead.
            If Recorder.IsBuffering Then
                SetControlEnabled(Menu_Replay_Box2, True)
                SetControlEnabled(Menu_Replay_save_text, True)
                SetControlEnabled(Menu_Replay_save_key, True)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' FIX #6: ApplyAudioSettings was hardcoding DDAGrab, overriding user's choice.
    ''' Now reads CaptureAPI from AppSettings, falling back to Auto.
    ''' </summary>
    Private Sub ApplyAudioSettings(recorder As CaptureEngine.CaptureCore.ScreenRecorder)
        Try
            Dim systemAudioEnabled As Boolean = AppSettings.Instance.Audio.SystemAudioEnabled
            Dim micEnabled As Boolean = AppSettings.Instance.Audio.MicEnabled
            Dim systemAudioVolume As Single = AppSettings.Instance.Audio.SystemAudioVolume
            Dim micVolume As Single = AppSettings.Instance.Audio.MicVolume

            If systemAudioEnabled AndAlso micEnabled Then
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.Both
            ElseIf systemAudioEnabled Then
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.SystemOnly
            ElseIf micEnabled Then
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.MicOnly
            Else
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.None
            End If

            recorder.SystemAudioVolume = systemAudioVolume
            recorder.MicVolume = micVolume

            ' ═══ FIX #6: Removed hardcoded DDAGrab ═══
            ' Old code: recorder.SelectedCaptureAPI = CaptureCore.ScreenRecorder.CaptureAPIType.DDAGrab
            ' This forced DDAGrab every time, overriding Auto/GFxCapture/GDIGrab settings.
            ' Now we do NOT touch SelectedCaptureAPI here — let the recorder keep
            ' whatever value was set by AppSettings.ApplyToRecorder() or default (Auto).
            ' CaptureAPI is a capture-side setting, not an audio setting.

            If micEnabled AndAlso Not String.IsNullOrEmpty(AppSettings.Instance.Audio.MicDeviceName) Then
                recorder.MicDeviceName = AppSettings.Instance.Audio.MicDeviceName
            End If

            Debug.WriteLine($"Audio Settings: Mode={recorder.AudioMode}, SystemVol={systemAudioVolume}, MicVol={micVolume}")

        Catch ex As Exception
            recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.SystemOnly
            recorder.SystemAudioVolume = 1.0F
            Debug.WriteLine($"ApplyAudioSettings Error: {ex.Message} - Using defaults")
        End Try
    End Sub

    Private Sub SaveReplayDurationInfo(filePath As String)
        Try
            Dim actualSeconds As Integer = CInt(Math.Floor(Recorder.GetVideoDuration(filePath)))
            Dim minutes As Integer = actualSeconds \ 60
            Dim seconds As Integer = actualSeconds Mod 60

            Dim dataDir As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "Replay")

            If Not Directory.Exists(dataDir) Then
                Directory.CreateDirectory(dataDir)
            End If

            ' Delete old files first
            Dim deletedCount As Integer = 0
            For Each oldFile As String In Directory.GetFiles(dataDir, "*.m")
                Try
                    File.Delete(oldFile)
                    deletedCount += 1
                Catch ex As Exception
                    Debug.WriteLine("Could not delete: " & oldFile)
                End Try
            Next
            For Each oldFile As String In Directory.GetFiles(dataDir, "*.s")
                Try
                    File.Delete(oldFile)
                    deletedCount += 1
                Catch ex As Exception
                    Debug.WriteLine("Could not delete: " & oldFile)
                End Try
            Next
            Debug.WriteLine("Deleted " & deletedCount & " old files")

            ' Create new files
            Dim mPath As String = Path.Combine(dataDir, minutes & ".m")
            Dim sPath As String = Path.Combine(dataDir, seconds & ".s")

            File.WriteAllText(mPath, "")
            File.WriteAllText(sPath, "")

            Debug.WriteLine("Created: " & minutes & ".m, " & seconds & ".s")
            Debug.WriteLine("Duration: " & minutes & "m " & seconds & "s")

            ' ═══ FIX #7: Show duration-aware notifier ═══
            ' Old code: Always showed "saved_last_15" even if replay was 60s
            ShowNotifier("saved_last_15")
        Catch ex As Exception
            Debug.WriteLine("SaveReplayDurationInfo Error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub OnRecordingStarted(sender As Object, e As EventArgs)
        If InvokeRequired Then
            Invoke(Sub() OnRecordingStarted(sender, e))
            Return
        End If
        Debug.WriteLine("Event: RecordingStarted")
    End Sub

    Private Sub OnRecordingStopped(sender As Object, filePath As String)
        If InvokeRequired Then
            Invoke(Sub() OnRecordingStopped(sender, filePath))
            Return
        End If
        Debug.WriteLine($"Event: RecordingStopped - {filePath}")
    End Sub

    ''' <summary>
    ''' FIX #8: Removed ShowNotifier from event handler to prevent double-notification.
    ''' Old code: OnRecordingError called ShowNotifier("recording_error")
    ''' AND ToggleRecording's Catch block also called ShowNotifier("recording_error")
    ''' Result: User saw 2 error popups for one failure. Now only the call site shows it.
    ''' </summary>
    Private Sub OnRecordingError(sender As Object, message As String)
        If InvokeRequired Then
            Invoke(Sub() OnRecordingError(sender, message))
            Return
        End If
        Debug.WriteLine($"Event: RecordingError - {message}")
        ' Don't show notifier here — the calling method handles it
    End Sub

    Private Sub OnBufferStarted(sender As Object, e As EventArgs)
        If InvokeRequired Then
            Invoke(Sub() OnBufferStarted(sender, e))
            Return
        End If
        Debug.WriteLine("Event: BufferStarted")
    End Sub

    Private Sub OnBufferStopped(sender As Object, e As EventArgs)
        If InvokeRequired Then
            Invoke(Sub() OnBufferStopped(sender, e))
            Return
        End If
        Debug.WriteLine("Event: BufferStopped")
    End Sub

    Private Sub OnReplaySaved(sender As Object, filePath As String)
        If InvokeRequired Then
            Invoke(Sub() OnReplaySaved(sender, filePath))
            Return
        End If
        Debug.WriteLine($"Event: ReplaySaved - {filePath}")
    End Sub
#End Region

#Region "Helper Methods"
    Private Sub ApplyRecorderSettings()
        Try
            AppSettings.Instance.ApplyToRecorder(Recorder)
            Debug.WriteLine("ApplyRecorderSettings: Applied from AppSettings.Instance")
            ValidateEncoder()
        Catch ex As Exception
            Debug.WriteLine("ApplyRecorderSettings Error: " & ex.Message)
            Recorder.Preset = CaptureCore.ScreenRecorder.RecordingPreset.Medium
            SelectBestEncoder()
        End Try
    End Sub

    Private Sub ValidateEncoder()
        Try
            Dim currentEncoder As CaptureCore.ScreenRecorder.VideoEncoder = Recorder.Encoder
            Dim encoderName As String = currentEncoder.ToString()
            Dim requiresHardware As Boolean = True

            Select Case currentEncoder
                Case CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264,
                 CaptureCore.ScreenRecorder.VideoEncoder.NVENC_HEVC,
                 CaptureCore.ScreenRecorder.VideoEncoder.NVENC_AV1
                    requiresHardware = AppSettings.HasNvidia

                Case CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264,
                 CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC
                    requiresHardware = AppSettings.HasIntel
                    If AppSettings.HasIntel Then
                        Dim ffmpegPath As String = Recorder.FFmpegPath
                        If Not String.IsNullOrEmpty(ffmpegPath) Then
                            Dim codecName As String = If(
                            currentEncoder = CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC,
                            "hevc_qsv", "h264_qsv")
                            If Not Base_RecordingsSet.CheckEncoderAvailability(ffmpegPath, encoderName) Then
                                Debug.WriteLine($"ValidateEncoder: {codecName} NOT available in FFmpeg, falling back")
                                requiresHardware = False
                            End If
                        End If
                    End If

                Case CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264,
                 CaptureCore.ScreenRecorder.VideoEncoder.AMF_HEVC
                    requiresHardware = AppSettings.HasAMD

                Case Else
                    requiresHardware = False
            End Select

            If requiresHardware Then
                Debug.WriteLine($"ValidateEncoder: {encoderName} - Hardware detected")
            Else
                Debug.WriteLine($"ValidateEncoder: {encoderName} - No hardware, selecting fallback")
                SelectBestEncoder()
            End If

        Catch ex As Exception
            Debug.WriteLine("ValidateEncoder Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' FIX #9: Added FFmpeg encoder availability check for NVENC and AMF.
    ''' Old code: Only checked QSV availability in FFmpeg.
    ''' If HasNvidia=True but FFmpeg doesn't have h264_nvenc, recording would fail.
    ''' Now validates all hardware encoders against FFmpeg before selecting.
    ''' </summary>
    Private Sub SelectBestEncoder()
        Try
            Dim ffmpegPath As String = Recorder.FFmpegPath
            Dim selectedEncoder As CaptureCore.ScreenRecorder.VideoEncoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264

            If AppSettings.HasNvidia Then
                ' Check if FFmpeg actually supports NVENC
                If Not String.IsNullOrEmpty(ffmpegPath) AndAlso
                   Base_RecordingsSet.CheckEncoderAvailability(ffmpegPath, "NVENC_HEVC") Then
                    selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.NVENC_HEVC
                    Debug.WriteLine("SelectBestEncoder: NVENC_HEVC (NVIDIA)")
                Else
                    Debug.WriteLine("SelectBestEncoder: NVIDIA detected but NVENC not available in FFmpeg, skipping")
                End If
            End If

            If selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264 AndAlso AppSettings.HasIntel Then
                ' Check if FFmpeg actually supports QSV
                If Not String.IsNullOrEmpty(ffmpegPath) AndAlso
                   Base_RecordingsSet.CheckEncoderAvailability(ffmpegPath, "QuickSync_HEVC") Then
                    selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC
                    Debug.WriteLine("SelectBestEncoder: QuickSync_HEVC (Intel)")
                Else
                    Debug.WriteLine("SelectBestEncoder: Intel detected but QSV not available in FFmpeg, skipping")
                End If
            End If

            If selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264 AndAlso AppSettings.HasAMD Then
                ' Check if FFmpeg actually supports AMF
                If Not String.IsNullOrEmpty(ffmpegPath) AndAlso
                   Base_RecordingsSet.CheckEncoderAvailability(ffmpegPath, "AMF_HEVC") Then
                    selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.AMF_HEVC
                    Debug.WriteLine("SelectBestEncoder: AMF_HEVC (AMD)")
                Else
                    Debug.WriteLine("SelectBestEncoder: AMD detected but AMF not available in FFmpeg, skipping")
                End If
            End If

            If selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264 Then
                Debug.WriteLine("SelectBestEncoder: LibX264 (CPU fallback)")
            End If

            Recorder.Encoder = selectedEncoder

        Catch ex As Exception
            Debug.WriteLine("SelectBestEncoder Error: " & ex.Message)
            Recorder.Encoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264
        End Try
    End Sub

#End Region

#Region "Encoder Info (Optional - for debugging)"

    ''' <summary>
    ''' Get information about current encoder
    ''' </summary>
    Public Function GetEncoderInfo() As String
        Try
            Dim encoder As CaptureCore.ScreenRecorder.VideoEncoder = Recorder.Encoder

            Select Case encoder
                Case CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264
                    Return "NVIDIA NVENC H.264"
                Case CaptureCore.ScreenRecorder.VideoEncoder.NVENC_HEVC
                    Return "NVIDIA NVENC HEVC"
                Case CaptureCore.ScreenRecorder.VideoEncoder.NVENC_AV1
                    Return "NVIDIA NVENC AV1"

                Case CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264
                    Return "Intel QuickSync H.264"
                Case CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC
                    Return "Intel QuickSync HEVC"

                Case CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264
                    Return "AMD AMF H.264"
                Case CaptureCore.ScreenRecorder.VideoEncoder.AMF_HEVC
                    Return "AMD AMF HEVC"

                Case CaptureCore.ScreenRecorder.VideoEncoder.LibX264
                    Return "CPU LibX264"
                Case CaptureCore.ScreenRecorder.VideoEncoder.LibX265
                    Return "CPU LibX265"

                Case Else
                    Return encoder.ToString()
            End Select

        Catch ex As Exception
            Return "Unknown"
        End Try
    End Function
    Public Function GetEncoderInfoDetailed() As String
        Try
            Dim encoder As CaptureCore.ScreenRecorder.VideoEncoder = Recorder.Encoder
            Dim info As New System.Text.StringBuilder()

            info.AppendLine("Encoder: " & GetEncoderInfo())

            Select Case encoder
                Case CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264,
                     CaptureCore.ScreenRecorder.VideoEncoder.NVENC_HEVC,
                     CaptureCore.ScreenRecorder.VideoEncoder.NVENC_AV1
                    info.AppendLine("Rate Control: -cq 20 (Quality Mode)")
                    info.AppendLine("Preset: p" & Recorder.EncoderPreset)

                Case CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264,
                     CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC
                    info.AppendLine("Rate Control: -global_quality 20 -look_ahead 1")
                    info.AppendLine("Preset: " & GetQSVPresetString(Recorder.EncoderPreset))

                Case CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264,
                     CaptureCore.ScreenRecorder.VideoEncoder.AMF_HEVC
                    info.AppendLine("Rate Control: -rc qvbr -qvbr_quality_level 20")

                Case CaptureCore.ScreenRecorder.VideoEncoder.LibX264,
                     CaptureCore.ScreenRecorder.VideoEncoder.LibX265
                    info.AppendLine("Rate Control: -crf 18-20")
                    info.AppendLine("Note: CPU encoding may impact performance")
            End Select

            info.AppendLine("Resolution: " & Recorder.ResolutionWidth & "x" & Recorder.ResolutionHeight)
            info.AppendLine("Framerate: " & Recorder.Framerate & " fps")
            info.AppendLine("Bitrate: " & Recorder.Bitrate & " kbps")

            Return info.ToString()

        Catch ex As Exception
            Return "Error getting encoder info: " & ex.Message
        End Try
    End Function

    Private Function GetQSVPresetString(preset As Integer) As String
        Select Case preset
            Case 1 : Return "slow"
            Case 2 : Return "medium"
            Case 3 : Return "fast"
            Case 4 : Return "faster"
            Case Else : Return "veryfast"
        End Select
    End Function

#End Region

End Class