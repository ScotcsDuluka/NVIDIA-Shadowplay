Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports Captrue_Core.CaptureCore

Public Class Base_RecordingsSet

#Region "Constants"
    Public Const MIN_BITRATE_GLOBAL As Integer = 500
    Public Const MAX_BITRATE_GLOBAL As Integer = 100000
    Public Const DEFAULT_BITRATE As Integer = 8000

    Public Const MIN_FPS_GLOBAL As Integer = 1
    Public Const MAX_FPS_GLOBAL As Integer = 240
    Public Const DEFAULT_FPS As Integer = 60

    Public Const MIN_RESOLUTION_WIDTH As Integer = 320
    Public Const MAX_RESOLUTION_WIDTH As Integer = 7680
    Public Const MIN_RESOLUTION_HEIGHT As Integer = 240
    Public Const MAX_RESOLUTION_HEIGHT As Integer = 4320

    Private Structure BitrateLimit
        Public MinBitrate As Integer
        Public MaxBitrate As Integer
        Public RecommendedMin As Integer
        Public RecommendedMax As Integer

        Public Sub New(min As Integer, max As Integer, recMin As Integer, recMax As Integer)
            MinBitrate = min
            MaxBitrate = max
            RecommendedMin = recMin
            RecommendedMax = recMax
        End Sub
    End Structure

    Private Structure FPSLimit
        Public MinFPS As Integer
        Public MaxFPS As Integer
        Public RecommendedFPS As Integer

        Public Sub New(min As Integer, max As Integer, recommended As Integer)
            MinFPS = min
            MaxFPS = max
            RecommendedFPS = recommended
        End Sub
    End Structure

    Private Shared ReadOnly DEFAULT_BITRATE_LIMIT As New BitrateLimit(1500, 20000, 3000, 12000)
    Private Shared ReadOnly DEFAULT_FPS_LIMIT As New FPSLimit(1, 60, 30)

    Private Shared ReadOnly COLOR_ACTIVE As Color = Color.FromArgb(118, 185, 0)
    Private Shared ReadOnly COLOR_INACTIVE As Color = Color.FromArgb(33, 35, 38)
#End Region

#Region "Window Management"
    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If m.Msg = WM_NCHITTEST Then
            m.Result = New IntPtr(HTTRANSPARENT)
            Return
        End If

        MyBase.WndProc(m)
    End Sub

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    <DllImport("user32.dll", SetLastError:=True, EntryPoint:="SetWindowLongPtr")>
    Private Shared Function SetWindowLongPtr(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, EntryPoint:="GetWindowLongPtr")>
    Private Shared Function GetWindowLongPtr(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    Private Sub HideFromAltTab()
        Try
            If IntPtr.Size = 8 Then
                Dim style As IntPtr = GetWindowLongPtr(Me.Handle, GWL_EXSTYLE)
                Dim newStyle As Long = (style.ToInt64() Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
                SetWindowLongPtr(Me.Handle, GWL_EXSTYLE, New IntPtr(newStyle))
            End If
        Catch ex As Exception
            Debug.WriteLine("HideFromAltTab Error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Dictionaries with Default Fallbacks"
    Private Shared ReadOnly FPS_LIMITS As New Dictionary(Of String, FPSLimit) From {
        {"1280x720", New FPSLimit(0, 240, 60)},
        {"1366x768", New FPSLimit(0, 240, 60)},
        {"1600x900", New FPSLimit(0, 165, 60)},
        {"1920x1080", New FPSLimit(0, 144, 60)},
        {"2560x1080", New FPSLimit(0, 144, 60)},
        {"2560x1440", New FPSLimit(0, 120, 60)},
        {"3440x1440", New FPSLimit(0, 100, 60)},
        {"3840x2160", New FPSLimit(0, 60, 30)}
    }

    Private Shared ReadOnly _bitrateLimits As New Dictionary(Of String, BitrateLimit) From {
        {"1280x720", New BitrateLimit(1500, 8000, 2500, 5000)},
        {"1366x768", New BitrateLimit(1500, 10000, 3000, 6000)},
        {"1600x900", New BitrateLimit(2000, 12000, 3500, 8000)},
        {"1920x1080", New BitrateLimit(3000, 100000, 8000, 25000)},
        {"2560x1080", New BitrateLimit(4000, 25000, 6000, 15000)},
        {"2560x1440", New BitrateLimit(6000, 40000, 10000, 25000)},
        {"3440x1440", New BitrateLimit(8000, 50000, 14000, 35000)},
        {"3840x2160", New BitrateLimit(12000, 80000, 20000, 60000)},
        {"7680x4320", New BitrateLimit(24000, 150000, 40000, 100000)}
    }

    Private _currentBitrateMax As Integer = DEFAULT_BITRATE_LIMIT.MaxBitrate
    Private _currentBitrateMin As Integer = DEFAULT_BITRATE_LIMIT.MinBitrate
    Private _currentFPSMax As Integer = DEFAULT_FPS_LIMIT.MaxFPS
    Private _currentFPSMin As Integer = DEFAULT_FPS_LIMIT.MinFPS
    Private _currentResolution As String = "1920x1080"

    ' ✅ Instance-level ComboBox reference
    Private _encoderComboBox As ComboBox = Nothing

    ' ✅ NEW: Cache encoder availability
    Private Shared _encoderAvailabilityCache As New Dictionary(Of String, Boolean)()
    Private Shared _availabilityCacheLock As New Object()
#End Region

#Region "Shared ScreenRecorder Instance"
    Private Shared ReadOnly _recorder As New Lazy(Of ScreenRecorder)(Function() New ScreenRecorder())
    Private Shared ReadOnly _encoderDict As New Dictionary(Of String, ScreenRecorder.VideoEncoder)()
    Private Shared ReadOnly _lockObj As New Object()

    Public Shared ReadOnly Property RecorderInstance As ScreenRecorder
        Get
            Return _recorder.Value
        End Get
    End Property

    Public Shared Function GetConfiguredRecorder() As ScreenRecorder
        ApplySettingsToRecorder()
        Return _recorder.Value
    End Function

    ''' <summary>
    ''' ✅ Apply AppSettings.Instance ไปยัง ScreenRecorder
    ''' </summary>
    Public Shared Sub ApplySettingsToRecorder()
        Try
            AppSettings.Instance.ApplyToRecorder(_recorder.Value)
            Debug.WriteLine("ApplySettingsToRecorder: Applied from AppSettings.Instance")
        Catch ex As Exception
            Debug.WriteLine("ApplySettingsToRecorder Error: " & ex.Message)
        End Try
    End Sub

    Public Shared Function IsNvidiaAvailable() As Boolean
        Return AppSettings.HasNvidia
    End Function

    Public Shared Function IsIntelAvailable() As Boolean
        Return AppSettings.HasIntel
    End Function

    Public Shared Function IsAMDAvailable() As Boolean
        Return AppSettings.HasAMD
    End Function
#End Region

#Region "✅ NEW: Encoder Availability Check"

    ''' <summary>
    ''' ✅ NEW: Check if FFmpeg supports a specific encoder
    ''' </summary>
    Private Shared Function CheckEncoderAvailability(ffmpegPath As String, encoderName As String) As Boolean
        ' Check cache first
        SyncLock _availabilityCacheLock
            If _encoderAvailabilityCache.ContainsKey(encoderName) Then
                Return _encoderAvailabilityCache(encoderName)
            End If
        End SyncLock

        If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
            Return False
        End If

        Try
            ' Map encoder name to FFmpeg codec
            Dim codecName As String = GetFFmpegCodecName(encoderName)
            If String.IsNullOrEmpty(codecName) Then
                Return False
            End If

            ' Run FFmpeg -encoders to check
            Using proc As New Process()
                proc.StartInfo = New ProcessStartInfo() With {
                    .FileName = ffmpegPath,
                    .Arguments = "-hide_banner -encoders",
                    .UseShellExecute = False,
                    .CreateNoWindow = True,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .StandardOutputEncoding = System.Text.Encoding.UTF8
                }

                proc.Start()
                Dim output As String = proc.StandardOutput.ReadToEnd()
                proc.WaitForExit(5000)

                ' Check if encoder is in the list
                Dim isAvailable As Boolean = output.Contains(codecName)

                ' Cache the result
                SyncLock _availabilityCacheLock
                    _encoderAvailabilityCache(encoderName) = isAvailable
                End SyncLock

                Debug.WriteLine(String.Format("CheckEncoderAvailability: {0} ({1}) = {2}", encoderName, codecName, isAvailable))
                Return isAvailable
            End Using
        Catch ex As Exception
            Debug.WriteLine("CheckEncoderAvailability Error: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' ✅ Map encoder name to FFmpeg codec name
    ''' </summary>
    Private Shared Function GetFFmpegCodecName(encoderName As String) As String
        Select Case encoderName
            Case "NVENC_H264" : Return "h264_nvenc"
            Case "NVENC_HEVC" : Return "hevc_nvenc"
            Case "NVENC_AV1" : Return "av1_nvenc"
            Case "QuickSync_H264" : Return "h264_qsv"
            Case "QuickSync_HEVC" : Return "hevc_qsv"
            Case "AMF_H264" : Return "h264_amf"
            Case "AMF_HEVC" : Return "hevc_amf"
            Case "LibX264" : Return "libx264"
            Case "LibX265" : Return "libx265"
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' ✅ NEW: Test if an encoder actually works with a quick encoding test
    ''' </summary>
    Private Shared Function TestEncoderWorks(ffmpegPath As String, encoderName As String) As Boolean
        If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
            Return False
        End If

        Dim codecName As String = GetFFmpegCodecName(encoderName)
        If String.IsNullOrEmpty(codecName) Then
            Return False
        End If

        Try
            ' Quick test: encode 1 frame of null source
            Dim testArgs As String = String.Format(
                "-f lavfi -i ""nullsrc=s=320x240:d=0.1"" -c:v {0} -frames:v 1 -f null - -hide_banner -loglevel error",
                codecName
            )

            Using proc As New Process()
                proc.StartInfo = New ProcessStartInfo() With {
                    .FileName = ffmpegPath,
                    .Arguments = testArgs,
                    .UseShellExecute = False,
                    .CreateNoWindow = True,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True
                }

                proc.Start()
                proc.WaitForExit(10000) ' 10 second timeout

                Dim success As Boolean = (proc.ExitCode = 0)
                Debug.WriteLine(String.Format("TestEncoderWorks: {0} = {1}", encoderName, success))
                Return success
            End Using
        Catch ex As Exception
            Debug.WriteLine("TestEncoderWorks Error for " & encoderName & ": " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' ✅ Clear the encoder availability cache (call when FFmpeg path changes)
    ''' </summary>
    Public Shared Sub ClearEncoderAvailabilityCache()
        SyncLock _availabilityCacheLock
            _encoderAvailabilityCache.Clear()
        End SyncLock
    End Sub
#End Region

#Region "Replay TrackBar"
    Private Sub TrackBar_Replaylast_Scroll(sender As Object, e As EventArgs) Handles TrackBar_Replaylast.Scroll
        Dim rawValue As Integer = TrackBar_Replaylast.Value
        Dim snappedValue As Integer = CInt(Math.Round(rawValue / 15.0) * 15)
        snappedValue = Math.Max(15, Math.Min(1200, snappedValue))

        TrackBar_Replaylast.Value = snappedValue
        UpdateBufferLabel(snappedValue)
    End Sub

    Private Sub TrackBar_Replaylast_MouseUp(sender As Object, e As MouseEventArgs) Handles TrackBar_Replaylast.MouseUp
        Dim seconds As Integer = TrackBar_Replaylast.Value

        ' ✅ Save to AppSettings.Instance
        AppSettings.Instance.Recording.ReplayDuration = seconds
        AppSettings.Instance.Save()

        Try
            RecorderInstance.BufferDurationSeconds = seconds
            Debug.WriteLine("TrackBar: Buffer=" & seconds & "s")
        Catch ex As Exception
            Debug.WriteLine("TrackBar Error: " & ex.Message)
        End Try
    End Sub

    Public Sub UpdateBufferLabel(seconds As Integer)
        If seconds >= 60 Then
            Dim minutes As Integer = seconds \ 60
            Dim remainingSeconds As Integer = seconds Mod 60
            If remainingSeconds = 0 Then
                lbl_BufferDuration.Text = LangHelper.GetText("l10n.replayLength") & " " & minutes & " " & LangHelper.GetText("l10n.m")
            Else
                lbl_BufferDuration.Text = LangHelper.GetText("l10n.replayLength") & " " & minutes & " " & LangHelper.GetText("l10n.m") & " " & remainingSeconds & " " & LangHelper.GetText("l10n.s")
            End If
        Else
            lbl_BufferDuration.Text = LangHelper.GetText("l10n.replayLength") & " " & seconds & " " & LangHelper.GetText("l10n.s")
        End If
    End Sub
#End Region

#Region "Helper Methods - Get Limits"
    Private Function GetBitrateLimits(resolution As String) As BitrateLimit
        If _bitrateLimits.ContainsKey(resolution) Then
            Return _bitrateLimits(resolution)
        End If

        Dim parts() As String = resolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length = 2 Then
            Dim w As Integer, h As Integer
            If Integer.TryParse(parts(0).Trim(), w) AndAlso Integer.TryParse(parts(1).Trim(), h) Then
                Dim pixels As Long = CLng(w) * CLng(h)
                Return CalculateBitrateLimitsFromPixels(pixels)
            End If
        End If

        Return DEFAULT_BITRATE_LIMIT
    End Function

    Private Function CalculateBitrateLimitsFromPixels(pixels As Long) As BitrateLimit
        Dim basePixels As Long = 1920 * 1080
        Dim ratio As Double = pixels / CDbl(basePixels)

        Dim minBitrate As Integer = CInt(Math.Max(MIN_BITRATE_GLOBAL, 3000 * ratio))
        Dim maxBitrate As Integer = CInt(Math.Min(MAX_BITRATE_GLOBAL, 20000 * ratio))
        Dim recMin As Integer = CInt(Math.Max(MIN_BITRATE_GLOBAL, 5000 * ratio))
        Dim recMax As Integer = CInt(Math.Min(MAX_BITRATE_GLOBAL, 12000 * ratio))

        Return New BitrateLimit(minBitrate, maxBitrate, recMin, recMax)
    End Function

    Private Function GetFPSLimits(resolution As String) As FPSLimit
        If FPS_LIMITS.ContainsKey(resolution) Then
            Return FPS_LIMITS(resolution)
        End If

        Dim parts() As String = resolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length = 2 Then
            Dim w As Integer, h As Integer
            If Integer.TryParse(parts(0).Trim(), w) AndAlso Integer.TryParse(parts(1).Trim(), h) Then
                Dim pixels As Long = CLng(w) * CLng(h)
                Return CalculateFPSLimitsFromPixels(pixels)
            End If
        End If

        Return DEFAULT_FPS_LIMIT
    End Function

    Private Function CalculateFPSLimitsFromPixels(pixels As Long) As FPSLimit
        Dim maxFPS As Integer

        If pixels >= 7680 * 4320 Then
            maxFPS = 60
        ElseIf pixels >= 3840 * 2160 Then
            maxFPS = 60
        ElseIf pixels >= 3440 * 1440 Then
            maxFPS = 100
        ElseIf pixels >= 2560 * 1440 Then
            maxFPS = 120
        ElseIf pixels >= 1920 * 1080 Then
            maxFPS = 144
        Else
            maxFPS = 240
        End If

        Return New FPSLimit(1, maxFPS, Math.Min(60, maxFPS))
    End Function

    Private Function IsValidResolution(resolution As String) As Boolean
        If String.IsNullOrWhiteSpace(resolution) Then Return False

        Dim parts() As String = resolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length <> 2 Then Return False

        Dim w As Integer, h As Integer
        If Not Integer.TryParse(parts(0).Trim(), w) OrElse Not Integer.TryParse(parts(1).Trim(), h) Then
            Return False
        End If

        Return w >= MIN_RESOLUTION_WIDTH AndAlso w <= MAX_RESOLUTION_WIDTH AndAlso
               h >= MIN_RESOLUTION_HEIGHT AndAlso h <= MAX_RESOLUTION_HEIGHT
    End Function

    Private Function ValidateBitrate(bitrate As Integer, resolution As String) As Integer
        Dim limits As BitrateLimit = GetBitrateLimits(resolution)
        Return Math.Max(limits.MinBitrate, Math.Min(limits.MaxBitrate, bitrate))
    End Function

    Private Function ValidateFPS(fps As Integer, resolution As String) As Integer
        Dim limits As FPSLimit = GetFPSLimits(resolution)
        Return Math.Max(limits.MinFPS, Math.Min(limits.MaxFPS, fps))
    End Function
#End Region

#Region "Form Events"

    Public Sub LoadAPIRECORD()
        Debug.WriteLine("═════════════════════════════════════════════════════════════════")
        Debug.WriteLine("LoadAPIRECORD START")

        Try
            Quality.Enabled = False
            HideFromAltTab()

            ' ✅ 1. Set ComboBox reference
            If cmbEncoder IsNot Nothing Then
                _encoderComboBox = cmbEncoder
                Debug.WriteLine("_encoderComboBox set OK")
            End If

            ' ✅ 2. Setup TrackBar
            TrackBar_Replaylast.Minimum = 15
            TrackBar_Replaylast.Maximum = 1200
            TrackBar_Replaylast.SmallChange = 15
            TrackBar_Replaylast.LargeChange = 15
            TrackBar_Replaylast.TickFrequency = 10

            ' ✅ Load from AppSettings.Instance
            Dim savedSeconds As Integer = AppSettings.Instance.Recording.ReplayDuration
            savedSeconds = Math.Max(15, Math.Min(1200, savedSeconds))
            savedSeconds = CInt(Math.Round(savedSeconds / 15.0) * 15)
            TrackBar_Replaylast.Value = savedSeconds
            UpdateBufferLabel(savedSeconds)

            SetupTrackBarDefaults()

            ' ✅ 3. Hardware detection
            AppSettings.DetectHardware()
            Debug.WriteLine($"Hardware: NVIDIA={AppSettings.HasNvidia}, Intel={AppSettings.HasIntel}, AMD={AppSettings.HasAMD}")

            ' ✅ 4. Find FFmpeg
            Dim ffmpegPath As String = FindFFmpegPath()
            If Not String.IsNullOrEmpty(ffmpegPath) Then
                RecorderInstance.SetFFmpegPath(ffmpegPath)
                AppSettings.Instance.Paths.FFmpegPath = ffmpegPath

                ' ✅ Clear cache when FFmpeg path changes
                ClearEncoderAvailabilityCache()

                ' ✅ Check ddagrab availability
                ScreenRecorder.CheckDDAGrabAvailability(ffmpegPath)

                ' Pre-warm
                Task.Run(Sub()
                             Try
                                 ScreenRecorder.PreWarmFFmpeg(ffmpegPath, RecorderInstance.Encoder)
                             Catch ex As Exception
                                 Debug.WriteLine("Pre-warm error: " & ex.Message)
                             End Try
                         End Sub)
            End If

            ' ✅ 5. Populate encoders (with validation)
            PopulateEncoderDictionary(ffmpegPath)

            ' ✅ 6. Select encoder
            SelectSavedOrBestEncoder()

            ' ✅ 7. Load other settings
            LoadResolutionBox()
            LoadSettings()
            UpdateUIFromPreset()

            ' ✅ 8. Update preview
            UpdateCommandPreview()

            Quality.Enabled = True
            Debug.WriteLine("LoadAPIRECORD END (Success)")

        Catch ex As Exception
            Debug.WriteLine("LoadAPIRECORD Error: " & ex.Message)
            Quality.Enabled = True
        End Try

        Debug.WriteLine("═════════════════════════════════════════════════════════════════")
    End Sub

    Private Function FindFFmpegPath() As String
        Dim possiblePaths As New List(Of String)()

        possiblePaths.Add(Path.Combine(Application.StartupPath, "api-core", "ffmpeg.exe"))
        possiblePaths.Add(Path.Combine(Application.StartupPath, "ffmpeg.exe"))
        possiblePaths.Add(Path.Combine(Application.StartupPath, "bin", "ffmpeg.exe"))

        For Each testPath As String In possiblePaths
            If File.Exists(testPath) Then
                Debug.WriteLine("FFmpeg found at: " & testPath)
                Return testPath
            End If
        Next

        Debug.WriteLine("FFmpeg NOT found!")
        Return Nothing
    End Function

    Public Sub Base_RecordingsSet_Load(sender As Object, e As EventArgs) Handles Me.Load
        LoadAPIRECORD()
    End Sub

    Private Sub Base_RecordingsSet_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            SaveCustomSettings()
            AppSettings.Instance.Save()
        Catch ex As Exception
            Debug.WriteLine("FormClosing Save Error: " & ex.Message)
        End Try
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Try
            If FPS_BOX.Text = "" Then
                FPS_BOX.Text = DEFAULT_FPS.ToString()
            End If
            If FPS_BOX.Text = "0" Then
                FPS_BOX.Text = DEFAULT_FPS.ToString()
            End If
            Me.Hide()
            vdo_resetall.ForeColor = Color.White
            vdo_resetall.Cursor = Cursors.Hand
            Base_Settings.Show()
            Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
            SaveCustomSettings()
        Catch ex As Exception
            Debug.WriteLine("action_fn_Click Error: " & ex.Message)
        End Try
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles vdo_resetall.Click
        vdo_resetall.ForeColor = Color.Gray
        vdo_resetall.Cursor = Cursors.Default
        If sender Is Nothing Then Return
        AppSettings.Instance.Recording.Preset = "Medium"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.Medium)
    End Sub
#End Region

#Region "Populate Encoders"

    ''' <summary>
    ''' ✅ FIXED: Populate encoders with availability validation
    ''' </summary>
    Public Sub PopulateEncoderDictionary(Optional ffmpegPath As String = Nothing)
        Debug.WriteLine("=== PopulateEncoderDictionary START ===")

        ' Clear and populate dictionary
        SyncLock _lockObj
            _encoderDict.Clear()
        End SyncLock

        ' Clear ComboBox
        If _encoderComboBox IsNot Nothing Then
            Try
                If _encoderComboBox.InvokeRequired Then
                    _encoderComboBox.Invoke(Sub() _encoderComboBox.Items.Clear())
                Else
                    _encoderComboBox.Items.Clear()
                End If
            Catch ex As Exception
                Debug.WriteLine("Error clearing ComboBox: " & ex.Message)
            End Try
        End If

        Dim addedCount As Integer = 0
        Dim availableEncoders As New List(Of String)()

        ' ══════════════════════════════════════════════════════════════════════════
        ' ✅ NVIDIA NVENC
        ' ══════════════════════════════════════════════════════════════════════════
        If AppSettings.HasNvidia Then
            ' NVENC H264
            If String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "NVENC_H264") Then
                AddEncoderSafe("NVENC_H264", ScreenRecorder.VideoEncoder.NVENC_H264, addedCount)
                availableEncoders.Add("NVENC_H264")
            Else
                Debug.WriteLine("NVENC_H264: NOT available in FFmpeg")
            End If

            ' NVENC HEVC
            If String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "NVENC_HEVC") Then
                AddEncoderSafe("NVENC_HEVC", ScreenRecorder.VideoEncoder.NVENC_HEVC, addedCount)
                availableEncoders.Add("NVENC_HEVC")
            Else
                Debug.WriteLine("NVENC_HEVC: NOT available in FFmpeg")
            End If

            ' NVENC AV1 (only on RTX 40 series+)
            If AppSettings.SupportsNVENCAV1 Then
                If String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "NVENC_AV1") Then
                    AddEncoderSafe("NVENC_AV1", ScreenRecorder.VideoEncoder.NVENC_AV1, addedCount)
                    availableEncoders.Add("NVENC_AV1")
                Else
                    Debug.WriteLine("NVENC_AV1: NOT available in FFmpeg")
                End If
            Else
                Debug.WriteLine("NVENC_AV1: Skipped (GPU does not support)")
            End If
        End If

        ' ══════════════════════════════════════════════════════════════════════════
        ' ✅ INTEL QUICKSYNC - FIXED
        ' ══════════════════════════════════════════════════════════════════════════
        If AppSettings.HasIntel Then
            ' QuickSync H264
            Dim qsvH264Available As Boolean = String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "QuickSync_H264")

            If qsvH264Available Then
                AddEncoderSafe("QuickSync_H264", ScreenRecorder.VideoEncoder.QuickSync_H264, addedCount)
                availableEncoders.Add("QuickSync_H264")
                Debug.WriteLine("QuickSync_H264: ✅ Available")
            Else
                Debug.WriteLine("QuickSync_H264: ❌ NOT available in FFmpeg - need FFmpeg with QSV support!")
            End If

            ' QuickSync HEVC
            Dim qsvHEVCAvailable As Boolean = String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "QuickSync_HEVC")

            If qsvHEVCAvailable Then
                AddEncoderSafe("QuickSync_HEVC", ScreenRecorder.VideoEncoder.QuickSync_HEVC, addedCount)
                availableEncoders.Add("QuickSync_HEVC")
                Debug.WriteLine("QuickSync_HEVC: ✅ Available")
            Else
                Debug.WriteLine("QuickSync_HEVC: ❌ NOT available in FFmpeg - need FFmpeg with QSV support!")
            End If
        Else
            Debug.WriteLine("Intel QuickSync: Skipped (No Intel GPU detected)")
        End If

        ' ══════════════════════════════════════════════════════════════════════════
        ' ✅ AMD AMF
        ' ══════════════════════════════════════════════════════════════════════════
        If AppSettings.HasAMD Then
            ' AMF H264
            If String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "AMF_H264") Then
                AddEncoderSafe("AMF_H264", ScreenRecorder.VideoEncoder.AMF_H264, addedCount)
                availableEncoders.Add("AMF_H264")
                Debug.WriteLine("AMF_H264: ✅ Available")
            Else
                Debug.WriteLine("AMF_H264: ❌ NOT available in FFmpeg")
            End If

            ' AMF HEVC
            If String.IsNullOrEmpty(ffmpegPath) OrElse CheckEncoderAvailability(ffmpegPath, "AMF_HEVC") Then
                AddEncoderSafe("AMF_HEVC", ScreenRecorder.VideoEncoder.AMF_HEVC, addedCount)
                availableEncoders.Add("AMF_HEVC")
                Debug.WriteLine("AMF_HEVC: ✅ Available")
            Else
                Debug.WriteLine("AMF_HEVC: ❌ NOT available in FFmpeg")
            End If
        Else
            Debug.WriteLine("AMD AMF: Skipped (No AMD GPU detected)")
        End If

        ' ══════════════════════════════════════════════════════════════════════════
        ' ✅ Software Encoders (always available)
        ' ══════════════════════════════════════════════════════════════════════════
        AddEncoderSafe("LibX264", ScreenRecorder.VideoEncoder.LibX264, addedCount)
        availableEncoders.Add("LibX264")

        AddEncoderSafe("LibX265", ScreenRecorder.VideoEncoder.LibX265, addedCount)
        availableEncoders.Add("LibX265")

        ' ══════════════════════════════════════════════════════════════════════════
        Debug.WriteLine("=== PopulateEncoderDictionary END: Added " & addedCount & " encoders ===")
        Debug.WriteLine("Available encoders: " & String.Join(", ", availableEncoders))

        ' ✅ Show warning if QuickSync was expected but not found
        If AppSettings.HasIntel AndAlso Not availableEncoders.Contains("QuickSync_H264") Then
            ShowEncoderWarning("Intel QuickSync", ffmpegPath)
        End If
    End Sub

    ''' <summary>
    ''' ✅ Show warning when an expected encoder is not available
    ''' </summary>
    Private Sub ShowEncoderWarning(encoderName As String, ffmpegPath As String)
        Dim message As String = String.Format(
            "{0} was detected but is NOT available in FFmpeg!{1}{1}" &
            "Solution: Use a FFmpeg build with {0} support.{1}{1}" &
            "Recommended: Download from github.com/BtbN/FFmpeg-Builds{1}" &
            "(Look for builds with 'qsv' or 'amf' in the name)",
            encoderName, vbCrLf
        )

        Debug.WriteLine("⚠️ " & message)

        ' Optional: Show tooltip or status message
        If lblEncoderInfo IsNot Nothing Then
            lblEncoderInfo.Text = encoderName & " not available in FFmpeg!"
            lblEncoderInfo.ForeColor = Color.Red
        End If
    End Sub

    Private Sub AddEncoderSafe(name As String, encoder As ScreenRecorder.VideoEncoder, ByRef count As Integer)
        SyncLock _lockObj
            If Not _encoderDict.ContainsKey(name) Then
                _encoderDict.Add(name, encoder)
            End If
        End SyncLock

        count += 1

        If _encoderComboBox IsNot Nothing Then
            Try
                If _encoderComboBox.InvokeRequired Then
                    _encoderComboBox.Invoke(Sub() _encoderComboBox.Items.Add(name))
                Else
                    _encoderComboBox.Items.Add(name)
                End If
            Catch ex As Exception
                Debug.WriteLine("Error adding '" & name & "' to ComboBox: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub SelectSavedOrBestEncoder()
        Debug.WriteLine("=== SelectSavedOrBestEncoder START ===")

        If _encoderComboBox Is Nothing Then
            Debug.WriteLine("ERROR: _encoderComboBox is Nothing!")
            Exit Sub
        End If

        Dim itemCount As Integer = 0
        If _encoderComboBox.InvokeRequired Then
            _encoderComboBox.Invoke(Sub() itemCount = _encoderComboBox.Items.Count)
        Else
            itemCount = _encoderComboBox.Items.Count
        End If

        If itemCount = 0 Then
            Debug.WriteLine("ERROR: ComboBox has no items!")
            Exit Sub
        End If

        ' ✅ Try saved encoder from AppSettings.Instance
        Dim savedEncoder As String = AppSettings.Instance.Recording.EncoderNow
        If Not String.IsNullOrEmpty(savedEncoder) Then
            Dim index As Integer = -1
            If _encoderComboBox.InvokeRequired Then
                _encoderComboBox.Invoke(Sub() index = _encoderComboBox.Items.IndexOf(savedEncoder))
            Else
                index = _encoderComboBox.Items.IndexOf(savedEncoder)
            End If

            If index >= 0 Then
                If _encoderComboBox.InvokeRequired Then
                    _encoderComboBox.Invoke(Sub() _encoderComboBox.SelectedIndex = index)
                Else
                    _encoderComboBox.SelectedIndex = index
                End If
                Debug.WriteLine("Selected saved encoder: " & savedEncoder)
                Exit Sub
            Else
                Debug.WriteLine("Saved encoder '" & savedEncoder & "' not found in available encoders")
            End If
        End If

        ' ✅ Priority order: NVENC > QuickSync > AMF > LibX264
        Dim priorityOrder As String() = {
            "NVENC_HEVC", "NVENC_H264",
            "QuickSync_HEVC", "QuickSync_H264",
            "AMF_HEVC", "AMF_H264",
            "LibX264", "LibX265"
        }

        If _encoderComboBox.InvokeRequired Then
            _encoderComboBox.Invoke(Sub()
                                        For Each encoderName As String In priorityOrder
                                            Dim idx As Integer = _encoderComboBox.Items.IndexOf(encoderName)
                                            If idx >= 0 Then
                                                _encoderComboBox.SelectedIndex = idx
                                                Debug.WriteLine("Selected best available encoder: " & encoderName)
                                                Return
                                            End If
                                        Next

                                        ' Fallback to first item
                                        If _encoderComboBox.Items.Count > 0 Then
                                            _encoderComboBox.SelectedIndex = 0
                                            Debug.WriteLine("Fallback to first encoder: " & _encoderComboBox.Items(0).ToString())
                                        End If
                                    End Sub)
        Else
            For Each encoderName As String In priorityOrder
                Dim idx As Integer = _encoderComboBox.Items.IndexOf(encoderName)
                If idx >= 0 Then
                    _encoderComboBox.SelectedIndex = idx
                    Debug.WriteLine("Selected best available encoder: " & encoderName)
                    Exit Sub
                End If
            Next

            ' Fallback to first item
            If _encoderComboBox.Items.Count > 0 Then
                _encoderComboBox.SelectedIndex = 0
                Debug.WriteLine("Fallback to first encoder: " & _encoderComboBox.Items(0).ToString())
            End If
        End If
    End Sub
#End Region

#Region "TrackBar & Bitrate Management"
    Private Sub SetupTrackBarDefaults()
        If TrackBar_BITRATE Is Nothing Then Exit Sub

        Dim limits As BitrateLimit = GetBitrateLimits(_currentResolution)

        TrackBar_BITRATE.Minimum = CInt(Math.Ceiling(limits.MinBitrate / 100.0))
        TrackBar_BITRATE.Maximum = CInt(Math.Floor(limits.MaxBitrate / 100.0))
        TrackBar_BITRATE.Value = CInt(Math.Floor(DEFAULT_BITRATE / 100.0))
        TrackBar_BITRATE.SmallChange = 5
        TrackBar_BITRATE.LargeChange = 20
        TrackBar_BITRATE.TickFrequency = Math.Max(1, CInt((TrackBar_BITRATE.Maximum - TrackBar_BITRATE.Minimum) / 10.0))

        _currentBitrateMin = limits.MinBitrate
        _currentBitrateMax = limits.MaxBitrate

        UpdateBitrateLabel()
    End Sub

    Private Sub UpdateBitrateLimits()
        If Resolution_BOX Is Nothing OrElse Resolution_BOX.SelectedIndex < 0 Then Exit Sub

        Dim resStr As String = Resolution_BOX.SelectedItem.ToString()
        _currentResolution = resStr

        Dim limits As BitrateLimit = GetBitrateLimits(resStr)
        _currentBitrateMin = limits.MinBitrate
        _currentBitrateMax = limits.MaxBitrate

        Dim newMin As Integer = CInt(Math.Ceiling(_currentBitrateMin / 100.0))
        Dim newMax As Integer = CInt(Math.Floor(_currentBitrateMax / 100.0))

        If TrackBar_BITRATE IsNot Nothing Then
            Dim currentBitrateKbps As Integer = TrackBar_BITRATE.Value * 100

            TrackBar_BITRATE.Minimum = newMin
            TrackBar_BITRATE.Maximum = newMax
            TrackBar_BITRATE.SmallChange = 5
            TrackBar_BITRATE.LargeChange = 20
            TrackBar_BITRATE.TickFrequency = Math.Max(1, CInt((newMax - newMin) / 10.0))

            Dim validatedBitrate As Integer = ValidateBitrate(currentBitrateKbps, resStr)
            Dim newTrackBarVal As Integer = CInt(Math.Floor(validatedBitrate / 100.0))
            newTrackBarVal = Math.Max(TrackBar_BITRATE.Minimum, Math.Min(TrackBar_BITRATE.Maximum, newTrackBarVal))

            If TrackBar_BITRATE.Value <> newTrackBarVal Then
                TrackBar_BITRATE.Value = newTrackBarVal
            End If
        End If

        If lblBitrateRange IsNot Nothing Then
            lblBitrateRange.Text = LangHelper.GetText("l10n.range") & " " & (limits.MinBitrate \ 1000).ToString() & "-" & (limits.MaxBitrate \ 1000).ToString() & " Mbps"
        End If

        UpdateBitrateLabel()
    End Sub

    Public Sub UpdateBitrateLabel()
        If TrackBar_BITRATE Is Nothing Then Exit Sub

        Dim bitrateKbps As Long = CLng(TrackBar_BITRATE.Value) * 100L
        If lblBitrateValue IsNot Nothing Then
            lblBitrateValue.Text = LangHelper.GetText("l10n.bitrate") & " " & bitrateKbps.ToString() & " kbps (" & (bitrateKbps / 1000.0).ToString("F1") & " Mbps)"
        End If
    End Sub

    Private Sub TrackBar_BITRATE_Scroll(sender As Object, e As EventArgs) Handles TrackBar_BITRATE.Scroll
        UpdateBitrateLabel()
    End Sub

    Private Sub TrackBar_BITRATE_ValueChanged(sender As Object, e As EventArgs) Handles TrackBar_BITRATE.ValueChanged
        UpdateBitrateLabel()

        Dim currentBitrate As Integer = TrackBar_BITRATE.Value * 100
        Dim validated As Integer = ValidateBitrate(currentBitrate, _currentResolution)

        If validated <> currentBitrate Then
            TrackBar_BITRATE.Value = CInt(Math.Floor(validated / 100.0))
            Exit Sub
        End If

        ' ✅ Save to AppSettings.Instance if Custom preset
        If AppSettings.Instance.Recording.Preset = "Custom" Then
            SaveCustomSettings()
        End If
    End Sub
#End Region

#Region "FPS Management"
    Private Sub UpdateFPSLimit()
        If Resolution_BOX Is Nothing OrElse Resolution_BOX.SelectedIndex < 0 Then Exit Sub
        If FPS_BOX Is Nothing Then Exit Sub

        Dim res As String = Resolution_BOX.SelectedItem.ToString()
        Dim limits As FPSLimit = GetFPSLimits(res)

        _currentFPSMin = limits.MinFPS
        _currentFPSMax = limits.MaxFPS

        Dim currentFPS As Integer
        If Integer.TryParse(FPS_BOX.Text, currentFPS) Then
            Dim validatedFPS As Integer = ValidateFPS(currentFPS, res)

            If validatedFPS <> currentFPS Then
                FPS_BOX.Text = validatedFPS.ToString()
            End If
        Else
            FPS_BOX.Text = limits.RecommendedFPS.ToString()
        End If
    End Sub

    Private Function GetCurrentFPS() As Integer
        If FPS_BOX Is Nothing OrElse String.IsNullOrWhiteSpace(FPS_BOX.Text) Then
            Return GetFPSLimits(_currentResolution).RecommendedFPS
        End If

        Dim fps As Integer
        If Integer.TryParse(FPS_BOX.Text, fps) Then
            Return ValidateFPS(fps, _currentResolution)
        End If

        Return GetFPSLimits(_currentResolution).RecommendedFPS
    End Function
#End Region

#Region "Resolution Management"
    Private Sub LoadResolutionBox()
        If Resolution_BOX Is Nothing Then Exit Sub

        If Resolution_BOX.Items.Count = 0 Then
            Resolution_BOX.Items.AddRange({
                "1920x1080",
                "2560x1440",
                "3840x2160",
                "1280x720",
                "1366x768",
                "1600x900",
                "2560x1080",
                "3440x1440"
            })
        End If

        ' ✅ Load from AppSettings.Instance
        Dim savedWidth As Integer = AppSettings.Instance.Recording.Width
        Dim savedHeight As Integer = AppSettings.Instance.Recording.Height

        Dim resStr As String
        If savedWidth >= MIN_RESOLUTION_WIDTH AndAlso savedWidth <= MAX_RESOLUTION_WIDTH AndAlso
           savedHeight >= MIN_RESOLUTION_HEIGHT AndAlso savedHeight <= MAX_RESOLUTION_HEIGHT Then
            resStr = savedWidth & "x" & savedHeight
        Else
            resStr = "1920x1080"
            AppSettings.Instance.Recording.Width = 1920
            AppSettings.Instance.Recording.Height = 1080
        End If

        If Resolution_BOX.Items.Contains(resStr) Then
            Resolution_BOX.SelectedItem = resStr
        Else
            If IsValidResolution(resStr) Then
                Resolution_BOX.Items.Add(resStr)
                Resolution_BOX.SelectedItem = resStr
            Else
                Resolution_BOX.SelectedIndex = 0
            End If
        End If

        _currentResolution = Resolution_BOX.SelectedItem.ToString()

        UpdateBitrateLimits()
        UpdateFPSLimit()
    End Sub

    Private Sub Resolution_BOX_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Resolution_BOX.SelectedIndexChanged
        If Resolution_BOX Is Nothing OrElse Resolution_BOX.SelectedIndex < 0 Then Exit Sub

        Dim newResolution As String = Resolution_BOX.SelectedItem.ToString()
        _currentResolution = newResolution

        UpdateBitrateLimits()
        UpdateFPSLimit()

        ' ✅ Save to AppSettings.Instance if Custom preset
        If AppSettings.Instance.Recording.Preset = "Custom" Then
            SaveCustomSettings()
        End If
    End Sub
#End Region

    Private Sub LoadSettings()
        Try
            ' ✅ Load FPS from AppSettings.Instance
            If FPS_BOX IsNot Nothing Then
                Dim savedFPS As Integer = AppSettings.Instance.Recording.FPS
                Dim validatedFPS As Integer = ValidateFPS(savedFPS, _currentResolution)
                FPS_BOX.Text = validatedFPS.ToString()
            End If

            ' ✅ Load Encoder Preset from AppSettings.Instance
            If P_BOX IsNot Nothing Then
                Dim savedP As Integer = AppSettings.Instance.Recording.EncoderPreset
                If savedP < 1 Then savedP = 1
                If savedP > 7 Then savedP = 7
                P_BOX.Text = savedP.ToString()
            End If

            ' ✅ Load Bitrate from AppSettings.Instance
            Dim savedBitrate As Integer = AppSettings.Instance.Recording.Bitrate
            Dim validatedBitrate As Integer = ValidateBitrate(savedBitrate, _currentResolution)
            Dim trackBarVal As Integer = CInt(Math.Floor(validatedBitrate / 100.0))

            If TrackBar_BITRATE IsNot Nothing Then
                Dim limits As BitrateLimit = GetBitrateLimits(_currentResolution)
                TrackBar_BITRATE.Minimum = CInt(Math.Ceiling(limits.MinBitrate / 100.0))
                TrackBar_BITRATE.Maximum = CInt(Math.Floor(limits.MaxBitrate / 100.0))

                trackBarVal = Math.Max(TrackBar_BITRATE.Minimum, Math.Min(TrackBar_BITRATE.Maximum, trackBarVal))
                TrackBar_BITRATE.Value = trackBarVal
                UpdateBitrateLabel()
            End If

        Catch ex As Exception
            Debug.WriteLine("LoadSettings Error: " & ex.Message)
        End Try
    End Sub

#Region "Debounce SaveCustomSettings"
    Private _saveSettingsTimer As System.Windows.Forms.Timer
    Private _saveSettingsPending As Boolean = False
    Private _saveSettingsLock As New Object()

    Private Sub InitSaveSettingsDebounce()
        If _saveSettingsTimer Is Nothing Then
            _saveSettingsTimer = New System.Windows.Forms.Timer()
            _saveSettingsTimer.Interval = 300
            AddHandler _saveSettingsTimer.Tick, AddressOf OnSaveSettingsTimer
        End If
    End Sub

    Private Sub OnSaveSettingsTimer(sender As Object, e As EventArgs)
        _saveSettingsTimer.Stop()

        If _saveSettingsPending Then
            _saveSettingsPending = False
            SaveCustomSettingsNow()
        End If
    End Sub

    Private Sub SaveCustomSettings()
        SyncLock _saveSettingsLock
            _saveSettingsPending = True
            InitSaveSettingsDebounce()
            _saveSettingsTimer.Start()
        End SyncLock
    End Sub

    ''' <summary>
    ''' ✅ Save to AppSettings.Instance instead of My.Settings
    ''' </summary>
    Private Sub SaveCustomSettingsNow()
        Try
            ' Save Encoder
            If cmbEncoder IsNot Nothing AndAlso cmbEncoder.SelectedIndex >= 0 Then
                Dim selectedEncoder As String = cmbEncoder.SelectedItem.ToString()
                AppSettings.Instance.Recording.EncoderNow = selectedEncoder
                AppSettings.Instance.Recording.Encoder = selectedEncoder

                ' Apply to recorder
                Dim targetEncoder As ScreenRecorder.VideoEncoder = ScreenRecorder.VideoEncoder.LibX264
                Dim found As Boolean = False

                SyncLock _lockObj
                    If _encoderDict.ContainsKey(selectedEncoder) Then
                        targetEncoder = _encoderDict(selectedEncoder)
                        found = True
                    End If
                End SyncLock

                If found Then
                    _recorder.Value.Encoder = targetEncoder
                End If
            End If

            ' Save FPS
            If FPS_BOX IsNot Nothing Then
                Dim fps As Integer = GetCurrentFPS()
                AppSettings.Instance.Recording.FPS = fps
            End If

            ' Save Bitrate
            If TrackBar_BITRATE IsNot Nothing Then
                Dim bitrate As Integer = TrackBar_BITRATE.Value * 100
                Dim validatedBitrate As Integer = ValidateBitrate(bitrate, _currentResolution)
                AppSettings.Instance.Recording.Bitrate = validatedBitrate
            End If

            ' Save Encoder Preset
            If P_BOX IsNot Nothing Then
                Dim pVal As Integer = 4
                If Integer.TryParse(P_BOX.Text, pVal) Then
                    pVal = Math.Max(1, Math.Min(7, pVal))
                    AppSettings.Instance.Recording.EncoderPreset = pVal
                    P_BOX.Text = pVal.ToString()
                End If
            End If

            ' Save Resolution
            If Resolution_BOX IsNot Nothing AndAlso Resolution_BOX.SelectedIndex >= 0 Then
                Dim resStr As String = Resolution_BOX.SelectedItem.ToString()
                If IsValidResolution(resStr) Then
                    Dim parts() As String = resStr.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
                    If parts.Length = 2 Then
                        Dim w As Integer, h As Integer
                        If Integer.TryParse(parts(0).Trim(), w) AndAlso Integer.TryParse(parts(1).Trim(), h) Then
                            AppSettings.Instance.Recording.Width = w
                            AppSettings.Instance.Recording.Height = h
                        End If
                    End If
                End If
            End If

            ' Save to config.json
            AppSettings.Instance.Save()

        Catch ex As Exception
            Debug.WriteLine("SaveCustomSettingsNow Error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Preset Selection"
    Private Sub LowPreset_Click(sender As Object, e As EventArgs) Handles Label11.Click, Label10.Click, low.Click
        If sender Is Nothing Then Return
        AppSettings.Instance.Recording.Preset = "Low"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.Low)
    End Sub

    Private Sub MediumPreset_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click, Label8.Click, Label9.Click
        If sender Is Nothing Then Return
        AppSettings.Instance.Recording.Preset = "Medium"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.Medium)
    End Sub

    Private Sub HighPreset_Click(sender As Object, e As EventArgs) Handles PictureBox2.Click, Label7.Click, Label6.Click
        If sender Is Nothing Then Return
        AppSettings.Instance.Recording.Preset = "High"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.High)
    End Sub

    Private Sub CustomPreset_Click(sender As Object, e As EventArgs) Handles C_ICO.Click, C_BG.Click, C_TEXT.Click
        If sender Is Nothing Then Return

        ' Save current encoder
        If cmbEncoder IsNot Nothing AndAlso cmbEncoder.SelectedIndex >= 0 Then
            Dim selectedEncoder As String = cmbEncoder.SelectedItem.ToString()
            AppSettings.Instance.Recording.EncoderNow = selectedEncoder
            AppSettings.Instance.Recording.Encoder = selectedEncoder
        End If

        AppSettings.Instance.Recording.Preset = "Custom"
        AppSettings.Instance.Save()
        EnableCustomControls(True)
        UpdateBitrateLimits()
    End Sub

    Private Sub UpdateControlsFromPreset(preset As ScreenRecorder.RecordingPreset)
        Try
            _recorder.Value.Preset = preset

            If FPS_BOX IsNot Nothing Then
                Dim presetFPS As Integer = _recorder.Value.Framerate
                Dim validatedFPS As Integer = ValidateFPS(presetFPS, _currentResolution)
                FPS_BOX.Text = validatedFPS.ToString()
            End If

            Dim resStr As String = _recorder.Value.ResolutionWidth & "x" & _recorder.Value.ResolutionHeight
            If IsValidResolution(resStr) Then
                For i As Integer = 0 To Resolution_BOX.Items.Count - 1
                    If Resolution_BOX.Items(i).ToString() = resStr Then
                        Resolution_BOX.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If

            Dim bitrate As Integer = _recorder.Value.Bitrate
            Dim validatedBitrate As Integer = ValidateBitrate(bitrate, _currentResolution)
            Dim trackBarVal As Integer = CInt(Math.Floor(validatedBitrate / 100.0))

            If TrackBar_BITRATE IsNot Nothing Then
                trackBarVal = Math.Max(TrackBar_BITRATE.Minimum, Math.Min(TrackBar_BITRATE.Maximum, trackBarVal))
                TrackBar_BITRATE.Value = trackBarVal
                UpdateBitrateLabel()
            End If

            EnableCustomControls(False)
            UpdatePresetColors()

        Catch ex As Exception
            Debug.WriteLine("UpdateControlsFromPreset Error: " & ex.Message)
        End Try
    End Sub

    Private Sub EnableCustomControls(enabled As Boolean)
        If FPS_BOX IsNot Nothing Then FPS_BOX.Enabled = enabled
        If P_BOX IsNot Nothing Then P_BOX.Enabled = enabled
        If TrackBar_BITRATE IsNot Nothing Then TrackBar_BITRATE.Enabled = enabled
        If Resolution_BOX IsNot Nothing Then Resolution_BOX.Enabled = enabled
        If lblBitrateRange IsNot Nothing Then lblBitrateRange.Visible = enabled
    End Sub

    Private Sub UpdateUIFromPreset()
        Select Case AppSettings.Instance.Recording.Preset
            Case "Low"
                UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.Low)
            Case "Medium"
                UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.Medium)
            Case "High"
                UpdateControlsFromPreset(ScreenRecorder.RecordingPreset.High)
            Case "Custom"
                EnableCustomControls(True)
                UpdateBitrateLimits()
                UpdateFPSLimit()
        End Select

        UpdatePresetColors()
    End Sub
#End Region

#Region "Encoder Selection"
    Private Sub cmbEncoder_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbEncoder.SelectedIndexChanged
        If cmbEncoder Is Nothing OrElse cmbEncoder.SelectedIndex < 0 Then Exit Sub

        Try
            Dim selectedName As String = cmbEncoder.SelectedItem.ToString()

            Dim targetEncoder As ScreenRecorder.VideoEncoder = ScreenRecorder.VideoEncoder.LibX264
            Dim found As Boolean = False

            SyncLock _lockObj
                If _encoderDict.ContainsKey(selectedName) Then
                    targetEncoder = _encoderDict(selectedName)
                    found = True
                End If
            End SyncLock

            If Not found Then Return

            _recorder.Value.Encoder = targetEncoder

            ' ✅ Save to AppSettings.Instance
            AppSettings.Instance.Recording.Encoder = selectedName
            AppSettings.Instance.Recording.EncoderNow = selectedName
            AppSettings.Instance.Save()

            UpdateEncoderInfo()
            Debug.WriteLine("cmbEncoder_SelectedIndexChanged: Encoder set to " & selectedName)

        Catch ex As Exception
            Debug.WriteLine("cmbEncoder_SelectedIndexChanged Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' ✅ FIXED: Better encoder info with QuickSync-specific messages
    ''' </summary>
    Private Sub UpdateEncoderInfo()
        If lblEncoderInfo Is Nothing Then Exit Sub

        Try
            Select Case _recorder.Value.Encoder
                ' ══════════════════════════════════════════════════════════════════════════
                ' NVIDIA NVENC
                ' ══════════════════════════════════════════════════════════════════════════
                Case ScreenRecorder.VideoEncoder.NVENC_H264
                    lblEncoderInfo.Text = "NVIDIA NVENC H.264 - Best Performance"
                    lblEncoderInfo.ForeColor = Color.FromArgb(118, 185, 0) ' Green

                Case ScreenRecorder.VideoEncoder.NVENC_HEVC
                    lblEncoderInfo.Text = "NVIDIA NVENC HEVC - Best Quality"
                    lblEncoderInfo.ForeColor = Color.FromArgb(118, 185, 0)

                Case ScreenRecorder.VideoEncoder.NVENC_AV1
                    lblEncoderInfo.Text = "NVIDIA NVENC AV1 - Next-Gen Quality"
                    lblEncoderInfo.ForeColor = Color.FromArgb(118, 185, 0)

                    ' ══════════════════════════════════════════════════════════════════════════
                    ' ✅ INTEL QUICKSYNC - FIXED
                    ' ══════════════════════════════════════════════════════════════════════════
                Case ScreenRecorder.VideoEncoder.QuickSync_H264
                    lblEncoderInfo.Text = "Intel QuickSync H.264 - Great Performance"
                    lblEncoderInfo.ForeColor = Color.FromArgb(0, 150, 255) ' Intel Blue

                Case ScreenRecorder.VideoEncoder.QuickSync_HEVC
                    lblEncoderInfo.Text = "Intel QuickSync HEVC - Great Quality"
                    lblEncoderInfo.ForeColor = Color.FromArgb(0, 150, 255)

                    ' ══════════════════════════════════════════════════════════════════════════
                    ' AMD AMF
                    ' ══════════════════════════════════════════════════════════════════════════
                Case ScreenRecorder.VideoEncoder.AMF_H264
                    lblEncoderInfo.Text = "AMD AMF H.264 - Good Performance"
                    lblEncoderInfo.ForeColor = Color.FromArgb(237, 28, 36) ' AMD Red

                Case ScreenRecorder.VideoEncoder.AMF_HEVC
                    lblEncoderInfo.Text = "AMD AMF HEVC - Good Quality"
                    lblEncoderInfo.ForeColor = Color.FromArgb(237, 28, 36)

                    ' ══════════════════════════════════════════════════════════════════════════
                    ' Software Encoders
                    ' ══════════════════════════════════════════════════════════════════════════
                Case ScreenRecorder.VideoEncoder.LibX264
                    lblEncoderInfo.Text = "CPU (LibX264) - May Slow Your PC"
                    lblEncoderInfo.ForeColor = Color.Orange

                Case ScreenRecorder.VideoEncoder.LibX265
                    lblEncoderInfo.Text = "CPU (LibX265) - Slower But Efficient"
                    lblEncoderInfo.ForeColor = Color.Orange
            End Select

        Catch ex As Exception
            Debug.WriteLine("UpdateEncoderInfo Error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Quality Timer"
    Private Sub Quality_Tick(sender As Object, e As EventArgs) Handles Quality.Tick
        Try
            If AppSettings.Instance.Recording.Preset = "Custom" Then
                ResetAllPresetColors()
                EnableCustomControls(True)

                If C_BG IsNot Nothing Then C_BG.BackColor = COLOR_ACTIVE
                If C_ICO IsNot Nothing Then C_ICO.BackColor = COLOR_ACTIVE
                If C_TEXT IsNot Nothing Then C_TEXT.BackColor = COLOR_ACTIVE

                UpdateBitrateLimits()
            Else
                EnableCustomControls(False)

                If C_BG IsNot Nothing Then C_BG.BackColor = COLOR_INACTIVE
                If C_ICO IsNot Nothing Then C_ICO.BackColor = COLOR_INACTIVE
                If C_TEXT IsNot Nothing Then C_TEXT.BackColor = COLOR_INACTIVE

                UpdatePresetColors()
            End If
        Catch ex As Exception
            Debug.WriteLine("Quality_Tick Error: " & ex.Message)
        End Try
    End Sub

    Private Sub ResetAllPresetColors()
        If Label11 IsNot Nothing Then Label11.BackColor = COLOR_INACTIVE
        If Label10 IsNot Nothing Then Label10.BackColor = COLOR_INACTIVE
        If low IsNot Nothing Then low.BackColor = COLOR_INACTIVE
        If PictureBox1 IsNot Nothing Then PictureBox1.BackColor = COLOR_INACTIVE
        If Label8 IsNot Nothing Then Label8.BackColor = COLOR_INACTIVE
        If Label9 IsNot Nothing Then Label9.BackColor = COLOR_INACTIVE
        If PictureBox2 IsNot Nothing Then PictureBox2.BackColor = COLOR_INACTIVE
        If Label7 IsNot Nothing Then Label7.BackColor = COLOR_INACTIVE
        If Label6 IsNot Nothing Then Label6.BackColor = COLOR_INACTIVE
    End Sub

    Private Sub UpdatePresetColors()
        ResetAllPresetColors()

        Select Case AppSettings.Instance.Recording.Preset
            Case "Low"
                If Label11 IsNot Nothing Then Label11.BackColor = COLOR_ACTIVE
                If Label10 IsNot Nothing Then Label10.BackColor = COLOR_ACTIVE
                If low IsNot Nothing Then low.BackColor = COLOR_ACTIVE

            Case "Medium"
                If PictureBox1 IsNot Nothing Then PictureBox1.BackColor = COLOR_ACTIVE
                If Label8 IsNot Nothing Then Label8.BackColor = COLOR_ACTIVE
                If Label9 IsNot Nothing Then Label9.BackColor = COLOR_ACTIVE

            Case "High"
                If PictureBox2 IsNot Nothing Then PictureBox2.BackColor = COLOR_ACTIVE
                If Label7 IsNot Nothing Then Label7.BackColor = COLOR_ACTIVE
                If Label6 IsNot Nothing Then Label6.BackColor = COLOR_ACTIVE
        End Select
    End Sub
#End Region

#Region "TextBox Validation"
    Private _isUpdatingFPS As Boolean = False
    Private _isUpdatingP As Boolean = False

    Private Sub NumberOnly_KeyPress(sender As Object, e As KeyPressEventArgs) Handles FPS_BOX.KeyPress, P_BOX.KeyPress
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub P_BOX_TextChanged(sender As Object, e As EventArgs) Handles P_BOX.TextChanged
        If P_BOX Is Nothing OrElse String.IsNullOrEmpty(P_BOX.Text) OrElse _isUpdatingP Then Exit Sub

        Dim v As Integer
        If Integer.TryParse(P_BOX.Text, v) Then
            v = Math.Max(1, Math.Min(7, v))

            If P_BOX.Text <> v.ToString() Then
                _isUpdatingP = True
                Try
                    Dim cursorPos As Integer = Math.Min(P_BOX.SelectionStart, v.ToString().Length)
                    P_BOX.Text = v.ToString()
                    P_BOX.SelectionStart = cursorPos
                    P_BOX.SelectionLength = 0
                Finally
                    _isUpdatingP = False
                End Try
            End If
        End If
    End Sub

    Private Sub FPS_BOX_TextChanged(sender As Object, e As EventArgs) Handles FPS_BOX.TextChanged
        If FPS_BOX Is Nothing OrElse String.IsNullOrEmpty(FPS_BOX.Text) OrElse _isUpdatingFPS Then Exit Sub

        Dim v As Integer
        If Integer.TryParse(FPS_BOX.Text, v) Then
            Dim validated As Integer = ValidateFPS(v, _currentResolution)

            If FPS_BOX.Text <> validated.ToString() Then
                _isUpdatingFPS = True
                Try
                    Dim cursorPos As Integer = Math.Min(FPS_BOX.SelectionStart, validated.ToString().Length)
                    FPS_BOX.Text = validated.ToString()
                    FPS_BOX.SelectionStart = cursorPos
                    FPS_BOX.SelectionLength = 0
                Finally
                    _isUpdatingFPS = False
                End Try
            End If
        End If
    End Sub

    Private Sub FPS_BOX_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles FPS_BOX.Validating
        If FPS_BOX Is Nothing OrElse String.IsNullOrWhiteSpace(FPS_BOX.Text) Then Exit Sub

        Dim fps As Integer
        If Not Integer.TryParse(FPS_BOX.Text, fps) Then
            FPS_BOX.Text = GetFPSLimits(_currentResolution).RecommendedFPS.ToString()
            Exit Sub
        End If

        Dim validated As Integer = ValidateFPS(fps, _currentResolution)
        If validated <> fps Then
            FPS_BOX.Text = validated.ToString()
        End If
    End Sub

    Private Sub TextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles Resolution_BOX.KeyDown, FPS_BOX.KeyDown, P_BOX.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            Me.ActiveControl = Nothing

            If AppSettings.Instance.Recording.Preset = "Custom" Then
                SaveCustomSettings()
            End If
        End If
    End Sub
#End Region

#Region "Hover Effects"
    Private Sub Label11_MouseMove(sender As Object, e As MouseEventArgs) Handles Label11.MouseMove, Label10.MouseMove, low.MouseMove
        If L_B IsNot Nothing Then L_B.Visible = True
        If L_L IsNot Nothing Then L_L.Visible = True
        If L_R IsNot Nothing Then L_R.Visible = True
        If L_T IsNot Nothing Then L_T.Visible = True
    End Sub

    Private Sub Label11_MouseLeave(sender As Object, e As EventArgs) Handles Label11.MouseLeave, Label10.MouseLeave, low.MouseLeave
        If L_B IsNot Nothing Then L_B.Visible = False
        If L_L IsNot Nothing Then L_L.Visible = False
        If L_R IsNot Nothing Then L_R.Visible = False
        If L_T IsNot Nothing Then L_T.Visible = False
    End Sub

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove, Label8.MouseMove, Label9.MouseMove
        If M_B IsNot Nothing Then M_B.Visible = True
        If M_L IsNot Nothing Then M_L.Visible = True
        If M_R IsNot Nothing Then M_R.Visible = True
        If M_T IsNot Nothing Then M_T.Visible = True
    End Sub

    Private Sub PictureBox1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave, Label8.MouseLeave, Label9.MouseLeave
        If M_B IsNot Nothing Then M_B.Visible = False
        If M_L IsNot Nothing Then M_L.Visible = False
        If M_R IsNot Nothing Then M_R.Visible = False
        If M_T IsNot Nothing Then M_T.Visible = False
    End Sub

    Private Sub PictureBox2_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox2.MouseMove, Label7.MouseMove, Label6.MouseMove
        If H_B IsNot Nothing Then H_B.Visible = True
        If H_L IsNot Nothing Then H_L.Visible = True
        If H_R IsNot Nothing Then H_R.Visible = True
        If H_T IsNot Nothing Then H_T.Visible = True
    End Sub

    Private Sub PictureBox2_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox2.MouseLeave, Label7.MouseLeave, Label6.MouseLeave
        If H_B IsNot Nothing Then H_B.Visible = False
        If H_L IsNot Nothing Then H_L.Visible = False
        If H_R IsNot Nothing Then H_R.Visible = False
        If H_T IsNot Nothing Then H_T.Visible = False
    End Sub

    Private Sub ALTZ_Tick(sender As Object, e As EventArgs) Handles ALTZ.Tick
        If Base.ReplayValue = True OrElse Base.RecordValue = True Then
            Panel_SET.Visible = False
            Panel_SET.Enabled = False
            captrueblock.Visible = True
            captrueblock_ico.Visible = True
            Panel.Size = New Size(1145, 206)
        Else
            Panel_SET.Visible = True
            Panel_SET.Enabled = True
            captrueblock.Visible = False
            captrueblock_ico.Visible = False
            Panel.Size = New Size(1145, 775)
        End If
    End Sub

    Private Sub C_BG_MouseMove(sender As Object, e As MouseEventArgs) Handles C_ICO.MouseMove, C_BG.MouseMove, C_TEXT.MouseMove
        C_B.Visible = True
        C_T.Visible = True
        C_L.Visible = True
        C_R.Visible = True
    End Sub

    Private Sub C_BG_MouseLeave(sender As Object, e As EventArgs) Handles C_ICO.MouseLeave, C_BG.MouseLeave, C_TEXT.MouseLeave
        C_B.Visible = False
        C_T.Visible = False
        C_L.Visible = False
        C_R.Visible = False
    End Sub
#End Region

#Region "Command Preview"
    Private Sub UpdateCommandPreview()
        If prearg IsNot Nothing Then
            Try
                prearg.Text = "ffmpeg " & RecorderInstance.GetFFmpegArguments()
            Catch ex As Exception
                prearg.Text = "Error generating command: " & ex.Message
            End Try
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        UpdateCommandPreview()
    End Sub

    Private Sub Button_Copy_Click(sender As Object, e As EventArgs) Handles Button_Copy.Click
        Try
            If prearg IsNot Nothing AndAlso Not String.IsNullOrEmpty(prearg.Text) Then
                Clipboard.SetText(prearg.Text)

                Dim originalText As String = Button_Copy.Text
                Button_Copy.Text = "Copied!"
                Button_Copy.BackColor = Color.FromArgb(0, 150, 0)

                Dim t As New Timer With {.Interval = 1500}
                AddHandler t.Tick, Sub(s, args)
                                       t.Stop()
                                       t.Dispose()
                                       Button_Copy.Text = originalText
                                       Button_Copy.BackColor = Color.FromArgb(60, 60, 60)
                                   End Sub
                t.Start()
            End If
        Catch ex As Exception
            MessageBox.Show("Copy failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
#End Region

End Class