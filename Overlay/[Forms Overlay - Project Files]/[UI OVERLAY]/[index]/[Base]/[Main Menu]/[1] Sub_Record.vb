Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports Captrue_Core
Imports System.Windows.Forms

Partial Public Class Base

    ' Status flags for UI
    Public ReplayValue As Boolean = False
    Public RecordValue As Boolean = False

    ' ✅ Cache for encoder availability
    Private Shared _encoderAvailabilityChecked As Boolean = False
    Private Shared _availableEncoders As New Dictionary(Of String, Boolean)()

    ' Get the shared recorder instance
    Private ReadOnly Property Recorder As CaptureCore.ScreenRecorder
        Get
            Return Base_RecordingsSet.RecorderInstance
        End Get
    End Property

#Region "Initialize Recorder Events"
    Private Sub InitializeRecorderEvents()
        ' Remove existing handlers first
        RemoveHandler Recorder.RecordingStarted, AddressOf OnRecordingStarted
        RemoveHandler Recorder.RecordingStopped, AddressOf OnRecordingStopped
        RemoveHandler Recorder.RecordingError, AddressOf OnRecordingError
        RemoveHandler Recorder.BufferStarted, AddressOf OnBufferStarted
        RemoveHandler Recorder.BufferStopped, AddressOf OnBufferStopped
        RemoveHandler Recorder.ReplaySaved, AddressOf OnReplaySaved

        ' Add handlers
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
            ' ✅ Primary: Use AppSettings.Instance.Paths.SavePath
            outputDir = AppSettings.Instance.Paths.SavePath

            ' Fallback: Use Base_Gallery.txtFilePath.Text
            If String.IsNullOrEmpty(outputDir) AndAlso Base_Gallery IsNot Nothing AndAlso Base_Gallery.txtFilePath IsNot Nothing Then
                outputDir = Base_Gallery.txtFilePath.Text
            End If
        Catch ex As Exception
            Debug.WriteLine("GetOutputDirectory: Error - " & ex.Message)
        End Try

        ' Fallback: Default path
        If String.IsNullOrEmpty(outputDir) OrElse Not Directory.Exists(outputDir) Then
            outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Shadowplay", "Gallery")
        End If

        ' Ensure directory exists
        If Not Directory.Exists(outputDir) Then
            Try
                Directory.CreateDirectory(outputDir)
            Catch ex As Exception
                Debug.WriteLine("GetOutputDirectory: Failed to create directory - " & ex.Message)
                ' Ultimate fallback
                outputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            End Try
        End If

        Debug.WriteLine("GetOutputDirectory: " & outputDir)
        Return outputDir
    End Function
#End Region

#Region "Toggle Recording (Alt+F9)"
    Public Async Sub ToggleRecording()
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\privacy") Then
        Else
            ShowNotifier("recording_error")
            Exit Sub
        End If
        Try
            ' Set FFmpeg paths
            Recorder.FFmpegPath = Path.Combine(Application.StartupPath, "api-core", "ffmpeg.exe")
            Recorder.FFprobePath = Path.Combine(Application.StartupPath, "api-core", "ffprobe.exe")
            ApplyRecorderSettings()

            If Recorder.IsRecording Then
                ' Stop recording
                Await Recorder.StopRecordingAsync()
                RecordValue = False
                ShowNotifier("recording_saved")
                Debug.WriteLine($"Recording stopped")
            Else
                ' Start recording
                RecordValue = True
                ShowNotifier("recording_started")
                Debug.WriteLine("Recording started")

                ' ✅ Use output directory from AppSettings
                Dim outputDir As String = GetOutputDirectory()
                Dim fileName = $"Record_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4"
                Dim outputPath = Path.Combine(outputDir, fileName)

                Dim success = Await Recorder.StartRecordingAsync(outputPath)
                If Not success Then
                    RecordValue = False
                    ShowNotifier("recording_error")
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[ToggleRecording] Error: {ex.Message}")
            RecordValue = False
            ShowNotifier("recording_error")
        End Try
    End Sub
#End Region

#Region "Toggle Instant Replay (Alt+Shift+F10)"
    Public Async Sub ToggleInstantReplay()
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\privacy") Then
        Else
            ShowNotifier("recording_error")
            Exit Sub
        End If
        Try
            ' Set FFmpeg paths
            Recorder.FFmpegPath = Path.Combine(Application.StartupPath, "api-core", "ffmpeg.exe")
            Recorder.FFprobePath = Path.Combine(Application.StartupPath, "api-core", "ffprobe.exe")
            ApplyRecorderSettings()

            If Recorder.IsBuffering Then
                ' Stop replay buffer
                Await Recorder.StopBufferAsync()
                ReplayValue = False
                SetControlColor(logo_replay, Color.White)
                ShowNotifier("instant_replay_off")
                Debug.WriteLine("Replay buffer stopped")
            Else
                ' Start replay buffer
                ReplayValue = True
                ShowNotifier("instant_replay_on")
                SetControlEnabled(replay_sc1, True)
                SetControlEnabled(Label7, True)
                SetControlEnabled(Label16, True)
                Debug.WriteLine("Replay buffer starting...")

                ' ✅ Set replay save duration from AppSettings.Instance (config.json)
                ' Note: Buffer is ALWAYS 20 minutes fixed, this is just how much to SAVE
                Dim saveSeconds As Integer = AppSettings.Instance.Recording.ReplayDuration
                If saveSeconds < 15 Then saveSeconds = 15
                If saveSeconds > 1200 Then saveSeconds = 1200
                Recorder.BufferDurationSeconds = saveSeconds

                Debug.WriteLine($"Replay save duration set to: {saveSeconds}s")
                Debug.WriteLine($"Buffer capacity: FIXED at 1200s (20 minutes)")

                Dim success = Await Recorder.StartBufferAsync()
                If Not success Then
                    ReplayValue = False
                    ShowNotifier("replay_error")
                    Debug.WriteLine("Replay buffer failed to start")
                Else
                    Debug.WriteLine($"Replay buffer started successfully")
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[ToggleInstantReplay] Error: {ex.Message}")
            ReplayValue = False
            ShowNotifier("replay_error")
        End Try
    End Sub
#End Region

#Region "Save Instant Replay (Alt+F10) - Shadowplay Style v2.0"
    ''' <summary>
    ''' Save instant replay with Shadowplay behavior:
    ''' - Uses output path from AppSettings (config.json)
    ''' - Saves what's available if buffer < requested
    ''' - Waits for next full second before saving
    ''' - Buffer is always 20 minutes, user setting controls save duration
    ''' </summary>
    Public Async Sub SaveInstantReplay()
        Try
            ' Check if buffer is active
            If Not Recorder.IsBuffering Then
                ShowNotifier("replay_turn_on")
                Debug.WriteLine("SaveInstantReplay: Buffer not active")
                Return
            End If

            ' Disable UI during save
            SetControlEnabled(replay_sc1, False)
            SetControlEnabled(Label7, False)
            SetControlEnabled(Label16, False)

            ' ✅ Get output directory from AppSettings
            Dim outputDir As String = GetOutputDirectory()

            ' Generate filename
            Dim fileName = $"Replay_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4"
            Dim outputPath = Path.Combine(outputDir, fileName)

            ' ✅ Get requested duration from AppSettings.Instance (config.json)
            Dim requestedDuration As Integer = AppSettings.Instance.Recording.ReplayDuration
            If requestedDuration < 15 Then requestedDuration = 15
            If requestedDuration > 1200 Then requestedDuration = 1200

            Debug.WriteLine($"══════════ SaveInstantReplay ══════════")
            Debug.WriteLine($"Output: {outputPath}")
            Debug.WriteLine($"Requested: {requestedDuration}s")
            Debug.WriteLine($"Buffer capacity: 1200s (fixed)")

            ' Save replay - Shadowplay style:
            ' - If buffer has 12s but requested 15s, saves 12s
            ' - If pressed at 0.30s, waits until 1.00s then saves
            ' - Uses timestamp-based extraction (not index-based)
            Dim success = Await Recorder.SaveReplayAsync(outputPath, requestedDuration)

            If success Then
                Debug.WriteLine($"SaveInstantReplay: SUCCESS")
                Debug.WriteLine($"  Saved to {outputPath}")

                ShowNotifier("saved_last_15")

                ' ✅ Save duration info for UI display - gets duration from actual file
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
            ' Re-enable UI
            SetControlEnabled(replay_sc1, True)
            SetControlEnabled(Label7, True)
            SetControlEnabled(Label16, True)
        End Try
    End Sub

    Private Sub SaveReplayDurationInfo(filePath As String)
        Try
            ' ✅ Get duration from file
            Dim actualSeconds As Integer = CInt(Math.Floor(Recorder.GetVideoDuration(filePath)))
            Dim minutes As Integer = actualSeconds \ 60
            Dim seconds As Integer = actualSeconds Mod 60

            Dim dataDir As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "Replay")

            ' ✅ Create directory if not exists
            If Not Directory.Exists(dataDir) Then
                Directory.CreateDirectory(dataDir)
            End If

            ' ✅ 1. DELETE OLD FILES FIRST
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

            ' ✅ 2. CREATE NEW FILES
            Dim mPath As String = Path.Combine(dataDir, minutes & ".m")
            Dim sPath As String = Path.Combine(dataDir, seconds & ".s")

            File.WriteAllText(mPath, "")
            File.WriteAllText(sPath, "")

            Debug.WriteLine("Created: " & minutes & ".m, " & seconds & ".s")
            Debug.WriteLine("Duration: " & minutes & "m " & seconds & "s")

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

    Private Sub OnRecordingError(sender As Object, message As String)
        If InvokeRequired Then
            Invoke(Sub() OnRecordingError(sender, message))
            Return
        End If
        Debug.WriteLine($"Event: RecordingError - {message}")
        ShowNotifier("recording_error")
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

    ''' <summary>
    ''' ✅ Apply settings from AppSettings.Instance to Recorder
    ''' With fallback to best available encoder
    ''' </summary>
    Private Sub ApplyRecorderSettings()
        Try
            ' ✅ Apply settings from AppSettings.Instance to Recorder
            AppSettings.Instance.ApplyToRecorder(Recorder)
            Debug.WriteLine("ApplyRecorderSettings: Applied from AppSettings.Instance")

            ' ✅ Verify encoder is valid for this system
            ValidateEncoder()

        Catch ex As Exception
            Debug.WriteLine("ApplyRecorderSettings Error: " & ex.Message)

            ' Fallback settings
            Recorder.Preset = CaptureCore.ScreenRecorder.RecordingPreset.Medium

            ' ✅ FIXED: Select best available encoder (NVENC > QuickSync > AMF > Software)
            SelectBestEncoder()
        End Try
    End Sub

    ''' <summary>
    ''' ✅ Validate that current encoder is available, fallback if not
    ''' </summary>
    Private Sub ValidateEncoder()
        Try
            Dim currentEncoder As CaptureCore.ScreenRecorder.VideoEncoder = Recorder.Encoder
            Dim encoderName As String = currentEncoder.ToString()
            Dim requiresHardware As Boolean = True

            ' Check if this is a hardware encoder
            Select Case currentEncoder
                Case CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264,
                     CaptureCore.ScreenRecorder.VideoEncoder.NVENC_HEVC,
                     CaptureCore.ScreenRecorder.VideoEncoder.NVENC_AV1
                    requiresHardware = AppSettings.HasNvidia

                Case CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264,
                     CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC
                    requiresHardware = AppSettings.HasIntel

                Case CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264,
                     CaptureCore.ScreenRecorder.VideoEncoder.AMF_HEVC
                    requiresHardware = AppSettings.HasAMD

                Case Else
                    ' Software encoders are always available
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
    ''' ✅ FIXED: Select best available encoder
    ''' Priority: NVENC > QuickSync > AMF > Software
    ''' </summary>
    Private Sub SelectBestEncoder()
        Try
            Dim selectedEncoder As CaptureCore.ScreenRecorder.VideoEncoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264

            ' ═══════════════════════════════════════════════════════════════════════
            ' ✅ Priority: NVENC > QuickSync > AMF > LibX264
            ' This matches Base_RecordingsSet_Fixed.vb priority
            ' ═══════════════════════════════════════════════════════════════════════

            If AppSettings.HasNvidia Then
                ' NVIDIA NVENC - Best performance
                selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264
                Debug.WriteLine("SelectBestEncoder: NVENC_H264 (NVIDIA)")

            ElseIf AppSettings.HasIntel Then
                ' ✅ Intel QuickSync - Good performance (priority over AMD)
                selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264
                Debug.WriteLine("SelectBestEncoder: QuickSync_H264 (Intel)")

            ElseIf AppSettings.HasAMD Then
                ' AMD AMF
                selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264
                Debug.WriteLine("SelectBestEncoder: AMF_H264 (AMD)")

            Else
                ' Fallback to CPU
                selectedEncoder = CaptureCore.ScreenRecorder.VideoEncoder.LibX264
                Debug.WriteLine("SelectBestEncoder: LibX264 (CPU fallback)")
            End If

            Recorder.Encoder = selectedEncoder

        Catch ex As Exception
            Debug.WriteLine("SelectBestEncoder Error: " & ex.Message)
            ' Ultimate fallback
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

#End Region

End Class
