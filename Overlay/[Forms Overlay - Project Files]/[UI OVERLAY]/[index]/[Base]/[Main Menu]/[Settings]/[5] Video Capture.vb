Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks
Imports System.Text.Json
Imports System.Text.Json.Serialization
Public Class Base_RecordingsSet

#Region "Constants"
    ' IMPORTANT: MAX_BITRATE_GLOBAL / MAX_FPS_GLOBAL are the UI INPUT caps.
    ' The recorder's hard caps are different and live in Engine's CaptureSettings validation.
    Public Const MIN_BITRATE_GLOBAL As Integer = 500
    Public Const MAX_BITRATE_GLOBAL As Integer = 150000
    Public Const DEFAULT_BITRATE As Integer = 20000

    Public Const MIN_FPS_GLOBAL As Integer = 1
    Public Const MAX_FPS_GLOBAL As Integer = 800
    Public Const DEFAULT_FPS As Integer = 60

    Public Const MIN_RESOLUTION_WIDTH As Integer = 320
    Public Const MAX_RESOLUTION_WIDTH As Integer = 7680
    Public Const MIN_RESOLUTION_HEIGHT As Integer = 240
    Public Const MAX_RESOLUTION_HEIGHT As Integer = 4320

    Private Const NATIVE_RESOLUTION_KEY As String = "Native"

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
    Private Shared ReadOnly COLOR_ENABLED As Color = Color.FromArgb(220, 220, 220)
    Private Shared ReadOnly COLOR_DISABLED As Color = Color.FromArgb(120, 120, 120)
#End Region

#Region "Window Management"
    Private Const WM_DISPLAYCHANGE As Integer = &H7E

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If m.Msg = WM_NCHITTEST Then
            m.Result = New IntPtr(HTTRANSPARENT)
            Return
        End If

        ' Auto-detect resolution change
        If m.Msg = WM_DISPLAYCHANGE Then
            Dim oldRes As String = _nativeResolution
            DetectNativeResolution()
            If _nativeResolution <> oldRes Then
                Debug.WriteLine("WM_DISPLAYCHANGE: Resolution changed from " & oldRes & " to " & _nativeResolution)
                LoadResolutionBox()
                UpdateBitrateLimits()
                UpdateBitrateLabel()
                UpdatePresetStatusLabel()
                UpdateCommandPreview()
            End If
        End If

        MyBase.WndProc(m)
    End Sub

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    <DllImport("user32.dll", SetLastError:=True, EntryPoint:="SetWindowLongPtr")>
    Private Shared Function SetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, EntryPoint:="GetWindowLongPtr")>
    Private Shared Function GetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, EntryPoint:="SetWindowLong")>
    Private Shared Function SetWindowLongPtr32(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, EntryPoint:="GetWindowLong")>
    Private Shared Function GetWindowLongPtr32(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    Private Sub HideFromAltTab()
        Try
            Dim style As IntPtr
            If IntPtr.Size = 8 Then
                style = GetWindowLongPtr64(Me.Handle, GWL_EXSTYLE)
                Dim newStyle As Long = (style.ToInt64() Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
                SetWindowLongPtr64(Me.Handle, GWL_EXSTYLE, New IntPtr(newStyle))
            Else
                style = GetWindowLongPtr32(Me.Handle, GWL_EXSTYLE)
                Dim newStyle As Integer = (style.ToInt32() Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
                SetWindowLongPtr32(Me.Handle, GWL_EXSTYLE, New IntPtr(newStyle))
            End If
        Catch ex As Exception
            Debug.WriteLine("HideFromAltTab Error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Dictionaries with Default Fallbacks"
    Private Shared ReadOnly FPS_LIMITS As New Dictionary(Of String, FPSLimit) From {
        {"1280x720", New FPSLimit(1, 240, 60)},
        {"1366x768", New FPSLimit(1, 240, 60)},
        {"1600x900", New FPSLimit(1, 240, 60)},
        {"1920x1080", New FPSLimit(1, 800, 60)},
        {"2560x1080", New FPSLimit(1, 240, 60)},
        {"2560x1440", New FPSLimit(1, 120, 60)},
        {"3440x1440", New FPSLimit(1, 100, 60)},
        {"3840x2160", New FPSLimit(1, 60, 30)},
        {"7680x4320", New FPSLimit(1, 60, 30)}
    }

    Private Shared ReadOnly _bitrateLimits As New Dictionary(Of String, BitrateLimit) From {
        {"1280x720", New BitrateLimit(1500, 30000, 2500, 8000)},
        {"1366x768", New BitrateLimit(1500, 35000, 3000, 10000)},
        {"1600x900", New BitrateLimit(2000, 40000, 3500, 12000)},
        {"1920x1080", New BitrateLimit(3000, 50000, 8000, 20000)},
        {"2560x1080", New BitrateLimit(4000, 50000, 6000, 25000)},
        {"2560x1440", New BitrateLimit(6000, 80000, 10000, 30000)},
        {"3440x1440", New BitrateLimit(8000, 80000, 14000, 40000)},
        {"3840x2160", New BitrateLimit(12000, 100000, 20000, 60000)},
        {"7680x4320", New BitrateLimit(24000, 150000, 40000, 100000)}
    }

    ' Encoder Preset Tooltip
    Private WithEvents _presetToolTip As ToolTip

    ' Dropdown menu restore state
    Private _menuRestoreDrop As Control
    Private _menuRestoreBg As Control

    Private _currentBitrateMax As Integer = DEFAULT_BITRATE_LIMIT.MaxBitrate
    Private _currentBitrateMin As Integer = DEFAULT_BITRATE_LIMIT.MinBitrate
    Private _currentRecommendedMin As Integer = DEFAULT_BITRATE_LIMIT.RecommendedMin
    Private _currentRecommendedMax As Integer = DEFAULT_BITRATE_LIMIT.RecommendedMax
    Private _currentFPSMax As Integer = DEFAULT_FPS_LIMIT.MaxFPS
    Private _currentFPSMin As Integer = DEFAULT_FPS_LIMIT.MinFPS
    Private _currentResolution As String = "1920x1080"

    Private _nativeResolution As String = String.Empty
    Public _nativeResolutionWidth As Integer = 0
    Public _nativeResolutionHeight As Integer = 0

    Private _currentEncoderName As String = String.Empty
    Private _currentPresetName As String = "P6"
    Private _resolutionList As New List(Of String)()
    Private _currentResolutionIndex As Integer = -1
    Private Shared _encoderAvailabilityCache As New Dictionary(Of String, Boolean)()
    Private Shared ReadOnly _availabilityCacheLock As New Object()
#End Region

#Region "Encoder Availability Check"

    Public Shared Function CheckEncoderAvailability(ffmpegPath As String, encoderName As String) As Boolean
        SyncLock _availabilityCacheLock
            If _encoderAvailabilityCache.ContainsKey(encoderName) Then
                Return _encoderAvailabilityCache(encoderName)
            End If
        End SyncLock

        If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
            Return False
        End If

        Try
            Dim codecName As String = GetFFmpegCodecName(encoderName)
            If String.IsNullOrEmpty(codecName) Then Return False

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

                Dim stdoutTask As Task(Of String) = proc.StandardOutput.ReadToEndAsync()
                Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()

                If proc.WaitForExit(1500) Then
                    Dim output As String = stdoutTask.Result
                    Dim isAvailable As Boolean = output.Contains(codecName)

                    SyncLock _availabilityCacheLock
                        _encoderAvailabilityCache(encoderName) = isAvailable
                    End SyncLock

                    Return isAvailable
                Else
                    Try
                        proc.Kill()
                    Catch
                    End Try

                    Try
                        stdoutTask.Wait(1000)
                    Catch
                    End Try
                    Try
                        stderrTask.Wait(1000)
                    Catch
                    End Try

                    Return False
                End If
            End Using
        Catch ex As Exception
            Debug.WriteLine("CheckEncoderAvailability Error: " & ex.Message)
            Return False
        End Try
    End Function

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

    Public Shared Sub ClearEncoderAvailabilityCache()
        SyncLock _availabilityCacheLock
            _encoderAvailabilityCache.Clear()
        End SyncLock
    End Sub
#End Region

#Region "Replay TrackBar"
    Private _isUpdatingReplayTrackBar As Boolean = False

    Private Sub TrackBar_Replaylast_Scroll(sender As Object, e As EventArgs) Handles TrackBar_Replaylast.Scroll
        Dim rawValue As Integer = TrackBar_Replaylast.Value
        Dim snappedValue As Integer = CInt(Math.Round(rawValue / 15.0) * 15)
        snappedValue = Math.Max(15, Math.Min(1200, snappedValue))

        If snappedValue <> rawValue Then
            _isUpdatingReplayTrackBar = True
            TrackBar_Replaylast.Value = snappedValue
            _isUpdatingReplayTrackBar = False
        End If

        UpdateBufferLabel(snappedValue)
    End Sub

    Private Sub TrackBar_Replaylast_MouseUp(sender As Object, e As MouseEventArgs) Handles TrackBar_Replaylast.MouseUp
        Dim seconds As Integer = TrackBar_Replaylast.Value
        AppSettings.Instance.Recording.ReplayDuration = seconds
        AppSettings.Instance.Save()

        ' W2-5: removed the BUFFER_DURATION send — no Engine handler has
        ' ever matched it ([Engine] Client.vb dispatches engine_*/legacy
        ' record commands only), so it was a dead command. The duration
        ' still persists via AppSettings → config.json, where the Engine
        ' reads it (fresh reload at record start / engine_config_changed).
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

        ' Update lblReplaySize: estimated buffer size
        If lblReplaySize IsNot Nothing Then
            Dim bitrateKbps As Long = CLng(TrackBar_BITRATE.Value) * 100L
            Dim totalKB As Double = (bitrateKbps / 8.0) * seconds
            Dim sizeMB As Double = totalKB / 1024.0
            Dim sizeGB As Double = sizeMB / 1024.0
            If sizeGB >= 1.0 Then
                lblReplaySize.Text = LangHelper.GetText("l10n.sizeGB", sizeGB.ToString("F1"))
            Else
                lblReplaySize.Text = LangHelper.GetText("l10n.sizeMB", sizeMB.ToString("F0"))
            End If
        End If
    End Sub

#End Region

#Region "Helper Methods"

    Private Sub ApplyControlLockState(ctrl As Control, isLocked As Boolean, Optional bgCtrl As Control = Nothing, Optional dropCtrl As Control = Nothing)
        If ctrl Is Nothing Then Exit Sub

        If isLocked Then
            ctrl.ForeColor = COLOR_DISABLED
            ctrl.Font = New Font(ctrl.Font, ctrl.Font.Style Or FontStyle.Strikeout)
            ctrl.Cursor = Cursors.Default
            If bgCtrl IsNot Nothing Then bgCtrl.Cursor = Cursors.Default
            If dropCtrl IsNot Nothing Then dropCtrl.Visible = False
        Else
            ctrl.ForeColor = COLOR_ENABLED
            ctrl.Font = New Font(ctrl.Font, ctrl.Font.Style And Not FontStyle.Strikeout)
            ctrl.Cursor = Cursors.Hand
            If bgCtrl IsNot Nothing Then bgCtrl.Cursor = Cursors.Hand
            If dropCtrl IsNot Nothing Then dropCtrl.Visible = True
        End If
    End Sub

    Private Sub DetectNativeResolution()
        Try
            Dim primaryScreen As Screen = Screen.PrimaryScreen
            _nativeResolutionWidth = primaryScreen.Bounds.Width
            _nativeResolutionHeight = primaryScreen.Bounds.Height
            _nativeResolution = _nativeResolutionWidth & "x" & _nativeResolutionHeight
            Debug.WriteLine("DetectNativeResolution: " & _nativeResolution)
        Catch ex As Exception
            _nativeResolution = "1920x1080"
            _nativeResolutionWidth = 1920
            _nativeResolutionHeight = 1080
        End Try
    End Sub

    Private Function ResolveActualResolution(resolution As String) As String
        If resolution = NATIVE_RESOLUTION_KEY OrElse resolution.StartsWith(NATIVE_RESOLUTION_KEY & " (") Then
            Return _nativeResolution
        End If
        Return resolution
    End Function

    Private Function GetBitrateLimits(resolution As String) As BitrateLimit
        Dim actualResolution As String = ResolveActualResolution(resolution)

        If _bitrateLimits.ContainsKey(actualResolution) Then
            Return _bitrateLimits(actualResolution)
        End If

        Dim parts() As String = actualResolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
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
        Dim actualResolution As String = ResolveActualResolution(resolution)

        If FPS_LIMITS.ContainsKey(actualResolution) Then
            Return FPS_LIMITS(actualResolution)
        End If

        Dim parts() As String = actualResolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length = 2 Then
            Dim w As Integer, h As Integer
            If Integer.TryParse(parts(0).Trim(), w) AndAlso Integer.TryParse(parts(1).Trim(), h) Then
                Dim pixels As Long = CLng(w) * CLng(h)
                Return CalculateFPSLimitsFromPixels(pixels, w, h)
            End If
        End If

        Return DEFAULT_FPS_LIMIT
    End Function

    Private Function CalculateFPSLimitsFromPixels(pixels As Long, Optional width As Integer = 0, Optional height As Integer = 0) As FPSLimit
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
            If width > 0 AndAlso height > 0 AndAlso (width / CDbl(height)) > 2.0 Then
                maxFPS = 240
            Else
                maxFPS = 144
            End If
        Else
            maxFPS = 240
        End If

        Return New FPSLimit(1, maxFPS, Math.Min(60, maxFPS))
    End Function

    Private Function IsValidResolution(resolution As String) As Boolean
        If resolution = NATIVE_RESOLUTION_KEY Then Return True
        If String.IsNullOrWhiteSpace(resolution) Then Return False

        Dim parts() As String = resolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length <> 2 Then Return False

        Dim w As Integer, h As Integer
        If Not Integer.TryParse(parts(0).Trim(), w) OrElse Not Integer.TryParse(parts(1).Trim(), h) Then Return False

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

    Private Function IsEditablePreset() As Boolean
        Dim p As String = AppSettings.Instance.Recording.Preset
        Return p = "Custom" OrElse p = "MyLow" OrElse p = "MyMedium" OrElse p = "MyHigh"
    End Function

    Private Function GetLocalizedPresetName(preset As String) As String
        Select Case preset
            Case "Low" : Return LangHelper.GetText("l10n.low")
            Case "Medium" : Return LangHelper.GetText("l10n.medium")
            Case "High" : Return LangHelper.GetText("l10n.high")
            Case "Custom" : Return LangHelper.GetText("l10n.custom")
            Case "Recommended" : Return LangHelper.GetText("l10n.recommended")
            Case "Maximum" : Return LangHelper.GetText("l10n.maximum")
            Case Else : Return preset
        End Select
    End Function

    Private Function GetEncoderPresetDescription(presetName As String, encoderName As String) As String
        If encoderName.StartsWith("AMF") Then
            Select Case presetName.ToLowerInvariant()
                Case "quality" : Return LangHelper.GetText("l10n.presetQualityBest")
                Case "balanced" : Return LangHelper.GetText("l10n.presetBalanced")
                Case "speed" : Return LangHelper.GetText("l10n.presetSpeedBest")
                Case Else : Return ""
            End Select
        End If

        Select Case presetName.ToLowerInvariant()
            Case "p1", "veryslow" : Return LangHelper.GetText("l10n.presetSlowestBest")
            Case "p2", "slower" : Return LangHelper.GetText("l10n.presetSlowerHigh")
            Case "p3", "slow" : Return LangHelper.GetText("l10n.presetSlowGood")
            Case "p4", "medium" : Return LangHelper.GetText("l10n.presetMediumBalanced")
            Case "p5", "fast" : Return LangHelper.GetText("l10n.presetFastGood")
            Case "p6", "faster" : Return LangHelper.GetText("l10n.presetFasterHigh")
            Case "p7", "veryfast" : Return LangHelper.GetText("l10n.presetFastestBest")
            Case "superfast" : Return LangHelper.GetText("l10n.presetSuperFast")
            Case "ultrafast" : Return LangHelper.GetText("l10n.presetUltraFast")
            Case Else : Return ""
        End Select
    End Function

    Public Sub UpdatePresetTooltip()
        If P_BOX Is Nothing Then Exit Sub

        If _presetToolTip Is Nothing Then
            _presetToolTip = New ToolTip() With {
                .InitialDelay = 200,
                .ReshowDelay = 100,
                .AutoPopDelay = 5000,
                .ShowAlways = True
            }
        End If

        Dim desc As String = GetEncoderPresetDescription(_currentPresetName, _currentEncoderName)
        If String.IsNullOrEmpty(desc) Then
            _presetToolTip.SetToolTip(P_BOX, _currentPresetName)
        Else
            _presetToolTip.SetToolTip(P_BOX, _currentPresetName & " - " & desc)
        End If
    End Sub
#End Region

#Region "Form Events"

    Public Sub LoadAPIRECORD()
        Debug.WriteLine("LoadAPIRECORD START")
        Dim startTime As DateTime = DateTime.Now

        Try
            Quality.Enabled = False
            HideFromAltTab()

            DetectNativeResolution()

            If cmbEncoder IsNot Nothing Then
                _currentEncoderName = cmbEncoder.Text
            End If

            SetupTrackBar()

            If Not AppSettings.HardwareDetected Then
                AppSettings.DetectHardware()
            End If
            Debug.WriteLine($"Hardware: NVIDIA={AppSettings.HasNvidia}, Intel={AppSettings.HasIntel}, AMD={AppSettings.HasAMD}")

            ' GLM/6 unified config: config.json is the single source. Legacy
            ' video.json is imported once by AppSettings migration at startup.

            Dim ffmpegPath As String = FindFFmpegPath()
            If Not String.IsNullOrEmpty(ffmpegPath) Then
                AppSettings.Instance.Paths.FFmpegPath = ffmpegPath
                ClearEncoderAvailabilityCache()
                ' Tell Engine to pre-warm FFmpeg via TCP
                Try
                    Base.tcp.Send("PREWARM_FFMPEG", ffmpegPath & "|" & _currentEncoderName)
                Catch ex As Exception
                    Debug.WriteLine("PreWarmFFmpeg TCP Error: " & ex.Message)
                End Try
            End If

            PopulateEncoderDictionary(ffmpegPath)
            SelectSavedOrBestEncoder()
            LoadResolutionBox()
            LoadSettings()
            UpdateUIFromPreset()
            UpdateCommandPreview()

            Quality.Enabled = True

            Dim elapsed As TimeSpan = DateTime.Now - startTime
            Debug.WriteLine($"LoadAPIRECORD END (Success in {elapsed.TotalMilliseconds:F0}ms)")

        Catch ex As Exception
            Debug.WriteLine("LoadAPIRECORD Error: " & ex.Message)
            Quality.Enabled = True
        End Try
    End Sub

    Private Sub SetupTrackBar()
        TrackBar_Replaylast.Minimum = 15
        TrackBar_Replaylast.Maximum = 1200
        TrackBar_Replaylast.SmallChange = 15
        TrackBar_Replaylast.LargeChange = 15
        TrackBar_Replaylast.TickFrequency = 10

        Dim savedSeconds As Integer = AppSettings.Instance.Recording.ReplayDuration
        savedSeconds = Math.Max(15, Math.Min(1200, savedSeconds))
        savedSeconds = CInt(Math.Round(savedSeconds / 15.0) * 15)
        TrackBar_Replaylast.Value = savedSeconds

        SetupTrackBarDefaults()
        UpdateBufferLabel(savedSeconds)
    End Sub

    Private Function FindFFmpegPath() As String
        Dim possiblePaths As String() = {
            AppLayout.P("FFmpeg", "ffmpeg.exe"),
            Path.Combine(Application.StartupPath, "api-core", "ffmpeg.exe"),
            Path.Combine(Application.StartupPath, "ffmpeg.exe"),
            Path.Combine(Application.StartupPath, "bin", "ffmpeg.exe")
        }

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
        ' OWNER rule: every Form sets WS_EX_TOOLWINDOW in its Load handler (sticky, once).
        ' Previously this only ran via LoadAPIRECORD's try block — an early exception
        ' there silently skipped it and leaked this form into Alt-Tab/taskbar.
        HideFromAltTab()
        LoadAPIRECORD()
    End Sub

    Private Sub Base_RecordingsSet_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If _saveSettingsTimer IsNot Nothing Then
                _saveSettingsTimer.Stop()
                _saveSettingsPending = False
                _saveSettingsTimer.Dispose()
                _saveSettingsTimer = Nothing
            End If

            If _copyResetTimer IsNot Nothing Then
                _copyResetTimer.Stop()
                _copyResetTimer.Dispose()
                _copyResetTimer = Nothing
            End If

            SaveCurrentSettings()
            ' GLM/6 unified: one file — AppSettings.Save() persists everything.
            AppSettings.Instance.Save()
            Try
                If Base.tcp IsNot Nothing Then Base.tcp.Send("engine_config_changed", "video")
            Catch
            End Try
        Catch ex As Exception
            Debug.WriteLine("FormClosing Save Error: " & ex.Message)
        End Try
    End Sub

    ' Two-Group Preset System
    Private Enum PresetGroup
        NVIDIA
        [My]
    End Enum

    Private ActivePresetGroup As PresetGroup = PresetGroup.NVIDIA
    Private ActiveMyPresetLevel As String = ""

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Try
            If String.IsNullOrEmpty(FPS_BOX.Text) OrElse FPS_BOX.Text = "0" Then
                FPS_BOX.Text = DEFAULT_FPS.ToString()
            End If

            If IsMyPreset() Then
                SaveMyPresetValues()
            End If

            Me.Hide()
            vdo_resetall.ForeColor = Color.White
            vdo_resetall.Cursor = Cursors.Hand
            Base_Settings.Show()
            Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
            SaveCurrentSettings()
            Base.Settings_List.Visible = True
        Catch ex As Exception
            Debug.WriteLine("action_fn_Click Error: " & ex.Message)
        End Try
    End Sub

    Private Function IsMyPreset() As Boolean
        If ActivePresetGroup <> PresetGroup.My Then Return False

        Select Case ActiveMyPresetLevel
            Case "Low", "Medium", "High"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Sub SaveMyPresetValues()
        Dim settings = AppSettings.Instance.Recording

        Select Case ActiveMyPresetLevel
            Case "Low"
                settings.MyLowFPS = GetCurrentFPS()
                settings.MyLowBitrate = TrackBar_BITRATE.Value * 100
                settings.MyLowEncoderPreset = PresetNameToIndex(_currentPresetName)

            Case "Medium"
                settings.MyMediumFPS = GetCurrentFPS()
                settings.MyMediumBitrate = TrackBar_BITRATE.Value * 100
                settings.MyMediumEncoderPreset = PresetNameToIndex(_currentPresetName)

            Case "High"
                settings.MyHighFPS = GetCurrentFPS()
                settings.MyHighBitrate = TrackBar_BITRATE.Value * 100
                settings.MyHighEncoderPreset = PresetNameToIndex(_currentPresetName)
        End Select
    End Sub

    Private Sub vdo_resetall_Click(sender As Object, e As EventArgs) Handles vdo_resetall.Click
        If sender Is Nothing Then Return

        ' Reset to NVIDIA Preset Medium
        ActivePresetGroup = PresetGroup.NVIDIA
        ActiveMyPresetLevel = ""

        AppSettings.Instance.Recording.Preset = "Medium"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset("Medium")
    End Sub
#End Region

#Region "Encoder Selection"

    Public Sub PopulateEncoderDictionary(Optional ffmpegPath As String = Nothing)
        Debug.WriteLine("=== PopulateEncoderDictionary START ===")

        Dim addedCount As Integer = 0

        If AppSettings.HasNvidia Then
            AddEncoderSafe("NVENC_H264", addedCount)
            AddEncoderSafe("NVENC_HEVC", addedCount)

            If AppSettings.SupportsNVENCAV1 Then
                AddEncoderSafe("NVENC_AV1", addedCount)
            End If
        End If

        If AppSettings.HasIntel Then
            AddEncoderSafe("QuickSync_H264", addedCount)
            AddEncoderSafe("QuickSync_HEVC", addedCount)
        End If

        ' AMD AMF disabled - reserved for future support
        'If AppSettings.HasAMD Then
        '    AddEncoderSafe("AMF_H264", addedCount)
        '    AddEncoderSafe("AMF_HEVC", addedCount)
        'End If

        AddEncoderSafe("LibX264", addedCount)
        AddEncoderSafe("LibX265", addedCount)

        If Not String.IsNullOrEmpty(ffmpegPath) AndAlso File.Exists(ffmpegPath) Then
            Task.Run(Sub() VerifyEncodersInBackground(ffmpegPath))
        End If

        Debug.WriteLine("=== PopulateEncoderDictionary END: Added " & addedCount & " encoders ===")
    End Sub

    Private Sub VerifyEncodersInBackground(ffmpegPath As String)
        EncoderService.VerifyAllInBackground(ffmpegPath)
    End Sub

    Private _encoderNameList As New List(Of String)()

    Private Sub AddEncoderSafe(name As String, ByRef count As Integer)
        If Not _encoderNameList.Contains(name) Then
            _encoderNameList.Add(name)
        End If
        count += 1
    End Sub

    Private Sub SelectSavedOrBestEncoder()
        Debug.WriteLine("=== SelectSavedOrBestEncoder START ===")

        If _encoderNameList.Count = 0 Then Exit Sub

        Dim savedEncoder As String = AppSettings.Instance.Recording.EncoderNow
        If Not String.IsNullOrEmpty(savedEncoder) Then
            If _encoderNameList.Contains(savedEncoder) Then
                ApplyEncoderSelection(savedEncoder)
                Debug.WriteLine("Selected saved encoder: " & savedEncoder)
                Exit Sub
            End If
        End If

        ' Priority order
        Dim priorityOrder As String() = {"NVENC_HEVC", "NVENC_H264", "NVENC_AV1", "QuickSync_HEVC", "QuickSync_H264", "LibX264", "LibX265"}

        For Each enc As String In priorityOrder
            If _encoderNameList.Contains(enc) Then
                ApplyEncoderSelection(enc)
                Exit Sub
            End If
        Next
        If _encoderNameList.Count > 0 Then
            ApplyEncoderSelection(_encoderNameList(0))
        End If
    End Sub

    Private Sub ApplyEncoderSelection(encoderName As String)
        _currentEncoderName = encoderName

        If cmbEncoder IsNot Nothing Then
            cmbEncoder.Text = GetEncoderDisplayName(encoderName)
        End If

        AppSettings.Instance.Recording.Encoder = encoderName
        AppSettings.Instance.Recording.EncoderNow = encoderName
        UpdateEncoderInfo()
        UpdatePresetDisplay()
    End Sub

    Private Function GetEncoderDisplayName(encoderKey As String) As String
        Select Case encoderKey
            Case "NVENC_H264" : Return "NVIDIA NVENC H.264"
            Case "NVENC_HEVC" : Return "NVIDIA NVENC HEVC"
            Case "NVENC_AV1" : Return "NVIDIA NVENC AV1"
            Case "QuickSync_H264" : Return "Intel QuickSync H.264"
            Case "QuickSync_HEVC" : Return "Intel QuickSync HEVC"
            Case "AMF_H264" : Return "AMD AMF H.264"
            Case "AMF_HEVC" : Return "AMD AMF HEVC"
            Case "LibX264" : Return "Software x264"
            Case "LibX265" : Return "Software x265"
            Case Else : Return encoderKey
        End Select
    End Function
#End Region

#Region "TrackBar & Bitrate Management"
    Private Sub SetupTrackBarDefaults()
        If TrackBar_BITRATE Is Nothing Then Exit Sub

        Dim limits As BitrateLimit = GetBitrateLimits(_currentResolution)

        _isUpdatingBitrate = True
        TrackBar_BITRATE.Minimum = CInt(Math.Ceiling(limits.MinBitrate / 100.0))
        TrackBar_BITRATE.Maximum = CInt(Math.Floor(limits.MaxBitrate / 100.0))
        TrackBar_BITRATE.Value = CInt(Math.Floor(DEFAULT_BITRATE / 100.0))
        _isUpdatingBitrate = False

        TrackBar_BITRATE.SmallChange = 5
        TrackBar_BITRATE.LargeChange = 20

        _currentBitrateMin = limits.MinBitrate
        _currentBitrateMax = limits.MaxBitrate
        _currentRecommendedMin = limits.RecommendedMin
        _currentRecommendedMax = limits.RecommendedMax

        UpdateBitrateLabel()
    End Sub

    Private Sub UpdateBitrateLimits()
        If Resolution_BOX Is Nothing OrElse _currentResolutionIndex < 0 Then Exit Sub

        Dim resStr As String = _currentResolution

        Dim limits As BitrateLimit = GetBitrateLimits(resStr)
        _currentBitrateMin = limits.MinBitrate
        _currentBitrateMax = limits.MaxBitrate
        _currentRecommendedMin = limits.RecommendedMin
        _currentRecommendedMax = limits.RecommendedMax

        Dim newMin As Integer = CInt(Math.Ceiling(_currentBitrateMin / 100.0))
        Dim newMax As Integer = CInt(Math.Floor(_currentBitrateMax / 100.0))

        If TrackBar_BITRATE IsNot Nothing Then
            _isUpdatingBitrate = True

            TrackBar_BITRATE.Minimum = newMin
            TrackBar_BITRATE.Maximum = newMax

            Dim currentBitrateKbps As Integer = TrackBar_BITRATE.Value * 100
            Dim validatedBitrate As Integer = ValidateBitrate(currentBitrateKbps, resStr)
            Dim newTrackBarVal As Integer = CInt(Math.Floor(validatedBitrate / 100.0))
            newTrackBarVal = Math.Max(newMin, Math.Min(newMax, newTrackBarVal))
            TrackBar_BITRATE.Value = newTrackBarVal

            _isUpdatingBitrate = False
        End If

        UpdateBitrateRangeLabel()
        UpdateBitrateLabel()
    End Sub

    Public Sub UpdateBitrateRangeLabel()
        If lblBitrateRange Is Nothing OrElse TrackBar_BITRATE Is Nothing Then Exit Sub

        Dim limits As BitrateLimit = GetBitrateLimits(_currentResolution)

        Dim displayMinKbps As Integer = TrackBar_BITRATE.Minimum * 100
        Dim displayMaxKbps As Integer = TrackBar_BITRATE.Maximum * 100
        Dim minMbps As Double = displayMinKbps / 1000.0
        Dim maxMbps As Double = displayMaxKbps / 1000.0
        Dim recMinMbps As Double = limits.RecommendedMin / 1000.0
        Dim recMaxMbps As Double = limits.RecommendedMax / 1000.0

        lblBitrateRange.Text = LangHelper.GetText("l10n.bitrateRange", minMbps.ToString("F1"), maxMbps.ToString("F1"), recMinMbps.ToString("F1"), recMaxMbps.ToString("F1"))
    End Sub

    Public Sub UpdateBitrateLabel()
        If TrackBar_BITRATE Is Nothing Then Exit Sub

        Dim bitrateKbps As Long = CLng(TrackBar_BITRATE.Value) * 100L
        Dim bitrateMbps As Double = bitrateKbps / 1000.0

        If lblBitrateValue IsNot Nothing Then
            lblBitrateValue.Text = LangHelper.GetText("l10n.bitrateLabel", bitrateKbps.ToString(), bitrateMbps.ToString("F1"))
        End If

        If lblBitratePre IsNot Nothing Then
            Dim gbPerHour As Double = (bitrateKbps * 3600.0) / 8.0 / 1024.0 / 1024.0
            lblBitratePre.Text = LangHelper.GetText("l10n.mbpsValue", bitrateMbps.ToString("F1")) & Environment.NewLine & LangHelper.GetText("l10n.gbPerHour", gbPerHour.ToString("F1"))
        End If
    End Sub

    Public Sub UpdatePresetStatusLabel()
        If lblPresetStatus Is Nothing Then Exit Sub

        Dim preset As String = AppSettings.Instance.Recording.Preset
        Dim statusText As String
        Dim statusColor As Color

        Select Case preset
            Case "Low", "Medium", "High"
                Dim displayName As String = GetLocalizedPresetName(preset)
                statusText = LangHelper.GetText("l10n.presetNvidiaLocked", displayName)
                statusColor = Color.FromArgb(180, 180, 180)

            Case "Custom"
                statusText = LangHelper.GetText("l10n.presetCustomAdjustable")
                statusColor = COLOR_ACTIVE

            Case "MyLow", "MyMedium", "MyHigh"
                Dim level As String = GetLocalizedPresetName(preset.Substring(2))
                statusText = LangHelper.GetText("l10n.presetMyAdjustable", level)
                statusColor = Color.FromArgb(100, 180, 255)

            Case "Recommended"
                statusText = LangHelper.GetText("l10n.presetRecommended")
                statusColor = Color.FromArgb(255, 200, 80)

            Case "Maximum"
                statusText = LangHelper.GetText("l10n.presetMaximum")
                statusColor = Color.FromArgb(255, 120, 80)

            Case Else
                statusText = ""
                statusColor = Color.FromArgb(160, 160, 160)
        End Select

        lblPresetStatus.Text = statusText
        lblPresetStatus.ForeColor = statusColor
    End Sub

    Private Sub SetBitrateValue(targetKbps As Integer)
        If TrackBar_BITRATE Is Nothing Then Exit Sub

        Dim validatedKbps As Integer = ValidateBitrate(targetKbps, _currentResolution)
        Dim tbVal As Integer = CInt(Math.Floor(validatedKbps / 100.0))
        tbVal = Math.Max(TrackBar_BITRATE.Minimum, Math.Min(TrackBar_BITRATE.Maximum, tbVal))

        _isUpdatingBitrate = True
        TrackBar_BITRATE.Value = tbVal
        _isUpdatingBitrate = False

        UpdateBitrateLabel()
    End Sub

    Private Sub TrackBar_BITRATE_Scroll(sender As Object, e As EventArgs) Handles TrackBar_BITRATE.Scroll
        UpdateBitrateLabel()
    End Sub

    Private _isUpdatingBitrate As Boolean = False

    Private Sub TrackBar_BITRATE_ValueChanged(sender As Object, e As EventArgs) Handles TrackBar_BITRATE.ValueChanged
        If _isUpdatingBitrate Then Exit Sub

        UpdateBitrateLabel()

        Dim currentBitrate As Integer = TrackBar_BITRATE.Value * 100
        Dim validated As Integer = ValidateBitrate(currentBitrate, _currentResolution)

        If validated <> currentBitrate Then
            _isUpdatingBitrate = True
            TrackBar_BITRATE.Value = CInt(Math.Floor(validated / 100.0))
            _isUpdatingBitrate = False
            Exit Sub
        End If

        If IsEditablePreset() Then
            SaveCurrentSettings()
        End If
    End Sub
#End Region

#Region "FPS Management"
    Private Sub UpdateFPSLimit()
        If Resolution_BOX Is Nothing OrElse _currentResolutionIndex < 0 Then Exit Sub
        If FPS_BOX Is Nothing Then Exit Sub

        Dim res As String = _currentResolution
        Dim limits As FPSLimit = GetFPSLimits(res)

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

        _resolutionList.Clear()

        Dim nativeDisplay As String = NATIVE_RESOLUTION_KEY & " (" & _nativeResolution & ")"
        _resolutionList.Add(nativeDisplay)

        Dim commonResolutions As String() = {"1920x1080", "2560x1440", "3840x2160", "1280x720", "1366x768", "1600x900", "2560x1080", "3440x1440"}

        For Each res As String In commonResolutions
            If res <> _nativeResolution Then _resolutionList.Add(res)
        Next

        Dim useNative As Boolean = AppSettings.Instance.Recording.UseNativeResolution
        Dim savedWidth As Integer = AppSettings.Instance.Recording.Width
        Dim savedHeight As Integer = AppSettings.Instance.Recording.Height

        If useNative OrElse (savedWidth = _nativeResolutionWidth AndAlso savedHeight = _nativeResolutionHeight) Then
            _currentResolutionIndex = 0
            _currentResolution = NATIVE_RESOLUTION_KEY
            Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)
        Else
            Dim savedRes As String = savedWidth & "x" & savedHeight
            Dim found As Boolean = False

            For i As Integer = 0 To _resolutionList.Count - 1
                If _resolutionList(i).Contains(savedRes) Then
                    _currentResolutionIndex = i
                    found = True
                    Exit For
                End If
            Next

            If Not found Then
                If IsValidResolution(savedRes) Then
                    _resolutionList.Add(savedRes)
                    _currentResolutionIndex = _resolutionList.Count - 1
                Else
                    _currentResolutionIndex = 0
                    _currentResolution = NATIVE_RESOLUTION_KEY
                End If
            End If

            _currentResolution = savedRes
            Resolution_BOX.Text = _resolutionList(_currentResolutionIndex)
        End If

        UpdateBitrateLimits()
        UpdateFPSLimit()
    End Sub

    Private Sub ApplyResolutionSelection(resKey As String)
        If resKey = NATIVE_RESOLUTION_KEY OrElse resKey.StartsWith(NATIVE_RESOLUTION_KEY & " (") Then
            _currentResolution = NATIVE_RESOLUTION_KEY
            _currentResolutionIndex = 0
            AppSettings.Instance.Recording.UseNativeResolution = True
            AppSettings.Instance.Recording.Width = _nativeResolutionWidth
            AppSettings.Instance.Recording.Height = _nativeResolutionHeight
            Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)
        Else
            _currentResolution = resKey
            AppSettings.Instance.Recording.UseNativeResolution = False

            Dim parts() As String = resKey.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length = 2 Then
                Integer.TryParse(parts(0).Trim(), AppSettings.Instance.Recording.Width)
                Integer.TryParse(parts(1).Trim(), AppSettings.Instance.Recording.Height)
            End If
            Resolution_BOX.Text = resKey

            _currentResolutionIndex = -1
            For i As Integer = 0 To _resolutionList.Count - 1
                If _resolutionList(i).Contains(resKey) Then
                    _currentResolutionIndex = i
                    Exit For
                End If
            Next
            If _currentResolutionIndex < 0 Then _currentResolutionIndex = 0
        End If

        UpdateBitrateLimits()
        UpdateFPSLimit()

        If IsEditablePreset() Then SaveCurrentSettings()
    End Sub
#End Region

    Private Sub LoadSettings()
        Try
            If FPS_BOX IsNot Nothing Then
                Dim savedFPS As Integer = AppSettings.Instance.Recording.FPS
                FPS_BOX.Text = ValidateFPS(savedFPS, _currentResolution).ToString()
            End If

            If P_BOX IsNot Nothing Then
                UpdatePresetDisplay()
            End If

            If TrackBar_BITRATE IsNot Nothing Then
                Dim savedBitrate As Integer = AppSettings.Instance.Recording.Bitrate
                Dim validatedBitrate As Integer = ValidateBitrate(savedBitrate, _currentResolution)
                Dim trackBarVal As Integer = CInt(Math.Floor(validatedBitrate / 100.0))
                trackBarVal = Math.Max(TrackBar_BITRATE.Minimum, Math.Min(TrackBar_BITRATE.Maximum, trackBarVal))

                _isUpdatingBitrate = True
                TrackBar_BITRATE.Value = trackBarVal
                _isUpdatingBitrate = False

                UpdateBitrateLabel()
                UpdateBufferLabel(TrackBar_Replaylast.Value)
            End If
        Catch ex As Exception
            Debug.WriteLine("LoadSettings Error: " & ex.Message)
        End Try
    End Sub

#Region "video.json Save/Load"
    ' GLM/6 unified config: legacy video.json writer/reader removed — config.json is the ONE file.

    ' GLM/6 unified config: legacy video.json writer/reader removed — config.json is the ONE file.

    ' GLM/6 unified config: legacy video.json writer/reader removed — config.json is the ONE file.
#End Region

#Region "Save Settings"
    Private _saveSettingsTimer As System.Windows.Forms.Timer
    Private _saveSettingsPending As Boolean = False

    Private Sub SaveCurrentSettings()
        If _saveSettingsTimer Is Nothing Then
            _saveSettingsTimer = New System.Windows.Forms.Timer With {.Interval = 300}
            AddHandler _saveSettingsTimer.Tick, AddressOf SaveSettingsTimer_Tick
        End If

        _saveSettingsPending = True
        _saveSettingsTimer.Start()
    End Sub

    Private Sub SaveSettingsTimer_Tick(sender As Object, e As EventArgs)
        _saveSettingsTimer.Stop()
        If _saveSettingsPending Then
            _saveSettingsPending = False
            SaveSettingsNow()
        End If
    End Sub

    Private Sub SaveSettingsNow()
        Try
            If Not String.IsNullOrEmpty(_currentEncoderName) Then
                AppSettings.Instance.Recording.Encoder = _currentEncoderName
                AppSettings.Instance.Recording.EncoderNow = _currentEncoderName
            End If

            AppSettings.Instance.Recording.FPS = GetCurrentFPS()
            AppSettings.Instance.Recording.Bitrate = TrackBar_BITRATE.Value * 100

            If P_BOX IsNot Nothing Then
                AppSettings.Instance.Recording.EncoderPreset = PresetNameToIndex(_currentPresetName)
            End If

            If _currentResolution = NATIVE_RESOLUTION_KEY OrElse _currentResolution.StartsWith(NATIVE_RESOLUTION_KEY & " (") Then
                AppSettings.Instance.Recording.UseNativeResolution = True
                AppSettings.Instance.Recording.Width = _nativeResolutionWidth
                AppSettings.Instance.Recording.Height = _nativeResolutionHeight
            Else
                AppSettings.Instance.Recording.UseNativeResolution = False
                Dim parts() As String = _currentResolution.Split({"x"c}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length = 2 Then
                    Integer.TryParse(parts(0).Trim(), AppSettings.Instance.Recording.Width)
                    Integer.TryParse(parts(1).Trim(), AppSettings.Instance.Recording.Height)
                End If
            End If

            ' Save My Preset specific values
            Select Case AppSettings.Instance.Recording.Preset
                Case "MyLow"
                    AppSettings.Instance.Recording.MyLowFPS = AppSettings.Instance.Recording.FPS
                    AppSettings.Instance.Recording.MyLowBitrate = AppSettings.Instance.Recording.Bitrate
                    AppSettings.Instance.Recording.MyLowEncoderPreset = AppSettings.Instance.Recording.EncoderPreset
                Case "MyMedium"
                    AppSettings.Instance.Recording.MyMediumFPS = AppSettings.Instance.Recording.FPS
                    AppSettings.Instance.Recording.MyMediumBitrate = AppSettings.Instance.Recording.Bitrate
                    AppSettings.Instance.Recording.MyMediumEncoderPreset = AppSettings.Instance.Recording.EncoderPreset
                Case "MyHigh"
                    AppSettings.Instance.Recording.MyHighFPS = AppSettings.Instance.Recording.FPS
                    AppSettings.Instance.Recording.MyHighBitrate = AppSettings.Instance.Recording.Bitrate
                    AppSettings.Instance.Recording.MyHighEncoderPreset = AppSettings.Instance.Recording.EncoderPreset
            End Select

            AppSettings.Instance.Save()
        Catch ex As Exception
            Debug.WriteLine("SaveSettingsNow Error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Preset Selection"

    ' ════════════════════════════════════════════════════════════════
    ' NVIDIA Preset: Low / Medium / High / Custom
    ' All settings locked, uses hardcoded default values
    ' ════════════════════════════════════════════════════════════════

    ' Hardcoded NVIDIA preset values (formerly from ScreenRecorder.RecordingPreset enum)
    Private Shared ReadOnly NVIDIA_PRESETS As New Dictionary(Of String, PresetValues) From {
        {"Low", New PresetValues(30, 4000, 6, True)},
        {"Medium", New PresetValues(60, 5000, 6, True)},
        {"High", New PresetValues(60, 10000, 4, True)}
    }

    Private Structure PresetValues
        Public FPS As Integer
        Public Bitrate As Integer
        Public EncoderPreset As Integer
        Public UseNativeResolution As Boolean

        Public Sub New(fps As Integer, bitrate As Integer, encoderPreset As Integer, useNative As Boolean)
            Me.FPS = fps
            Me.Bitrate = bitrate
            Me.EncoderPreset = encoderPreset
            Me.UseNativeResolution = useNative
        End Sub
    End Structure

    Private Sub LowPreset_Click(sender As Object, e As EventArgs) Handles Label11.Click, Label10.Click, low.Click
        ActivePresetGroup = PresetGroup.NVIDIA
        ActiveMyPresetLevel = ""
        AppSettings.Instance.Recording.Preset = "Low"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset("Low")
    End Sub

    Private Sub MediumPreset_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click, Label8.Click, Label9.Click
        ActivePresetGroup = PresetGroup.NVIDIA
        ActiveMyPresetLevel = ""
        AppSettings.Instance.Recording.Preset = "Medium"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset("Medium")
    End Sub

    Private Sub HighPreset_Click(sender As Object, e As EventArgs) Handles PictureBox2.Click, Label7.Click, Label6.Click
        ActivePresetGroup = PresetGroup.NVIDIA
        ActiveMyPresetLevel = ""
        AppSettings.Instance.Recording.Preset = "High"
        AppSettings.Instance.Save()
        UpdateControlsFromPreset("High")
    End Sub

    Private Sub CustomPreset_Click(sender As Object, e As EventArgs) Handles C_ICO.Click, C_BG.Click, C_TEXT.Click
        If Not String.IsNullOrEmpty(_currentEncoderName) Then
            AppSettings.Instance.Recording.Encoder = _currentEncoderName
            AppSettings.Instance.Recording.EncoderNow = _currentEncoderName
        End If

        ActivePresetGroup = PresetGroup.NVIDIA
        ActiveMyPresetLevel = ""
        AppSettings.Instance.Recording.Preset = "Custom"
        AppSettings.Instance.Save()
        ' OWNER UX rule: paint the Custom highlight NOW. Every other preset
        ' click repaints colors immediately (UpdateControlsFromPreset /
        ' ApplyMyXxxPreset -> UpdatePresetColors), but Custom relied on the
        ' 200ms Quality poll — its active color showed up to 200ms late.
        ResetAllPresetColors()
        If C_BG IsNot Nothing Then C_BG.BackColor = COLOR_ACTIVE
        If C_ICO IsNot Nothing Then C_ICO.BackColor = COLOR_ACTIVE
        If C_TEXT IsNot Nothing Then C_TEXT.BackColor = COLOR_ACTIVE
        EnableCustomControls(True)
        UpdateBitrateLimits()
    End Sub

    ' ════════════════════════════════════════════════════════════════
    ' My Preset: MyLow / MyMedium / MyHigh / Recommended / Maximum
    ' ════════════════════════════════════════════════════════════════

    Private Sub MyLow_TEXT_Click(sender As Object, e As EventArgs) Handles ML_TEXT.Click, ML_ICO.Click, ML_BG.Click
        ActivePresetGroup = PresetGroup.My
        ActiveMyPresetLevel = "Low"
        AppSettings.Instance.Recording.Preset = "MyLow"
        AppSettings.Instance.Save()
        ApplyMyLowPreset()
    End Sub

    Private Sub MyMedium_TEXT_Click(sender As Object, e As EventArgs) Handles MM_TEXT.Click, MM_ICO.Click, MM_BG.Click
        ActivePresetGroup = PresetGroup.My
        ActiveMyPresetLevel = "Medium"
        AppSettings.Instance.Recording.Preset = "MyMedium"
        AppSettings.Instance.Save()
        ApplyMyMediumPreset()
    End Sub

    Private Sub MyHigh_TEXT_Click(sender As Object, e As EventArgs) Handles MH_TEXT.Click, MH_ICO.Click, MH_BG.Click
        ActivePresetGroup = PresetGroup.My
        ActiveMyPresetLevel = "High"
        AppSettings.Instance.Recording.Preset = "MyHigh"
        AppSettings.Instance.Save()
        ApplyMyHighPreset()
    End Sub

    Private Sub Recommended_TEXT_Click(sender As Object, e As EventArgs) Handles Recommended_TEXT.Click, Recommended_ICO.Click, Recommended_BG.Click
        ActivePresetGroup = PresetGroup.My
        ActiveMyPresetLevel = "Recommended"
        AppSettings.Instance.Recording.Preset = "Recommended"
        AppSettings.Instance.Save()
        ApplyRecommendedPreset()
    End Sub

    Private Sub Maximum_TEXT_Click(sender As Object, e As EventArgs) Handles Maximum_TEXT.Click, Maximum_ICO.Click, Maximum_BG.Click
        ActivePresetGroup = PresetGroup.My
        ActiveMyPresetLevel = "Maximum"
        AppSettings.Instance.Recording.Preset = "Maximum"
        AppSettings.Instance.Save()
        ApplyMaximumPreset()
    End Sub

    ' My Preset: MyLow
    Private Sub ApplyMyLowPreset()
        ' Force Native resolution
        _currentResolutionIndex = 0
        _currentResolution = NATIVE_RESOLUTION_KEY
        If Resolution_BOX IsNot Nothing Then Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)

        ' Update TrackBar range to match Native resolution FIRST
        UpdateBitrateLimits()

        ' Use saved MyLow values or defaults (Low defaults: 30fps, 4000kbps, P6)
        Dim myFPS As Integer = AppSettings.Instance.Recording.MyLowFPS.GetValueOrDefault(30)
        Dim myBitrate As Integer = AppSettings.Instance.Recording.MyLowBitrate.GetValueOrDefault(4000)
        Dim myEncoderPreset As Integer = AppSettings.Instance.Recording.MyLowEncoderPreset.GetValueOrDefault(6)

        If FPS_BOX IsNot Nothing Then
            FPS_BOX.Text = ValidateFPS(myFPS, _currentResolution).ToString()
        End If

        SetBitrateValue(myBitrate)

        AppSettings.Instance.Recording.FPS = myFPS
        AppSettings.Instance.Recording.Bitrate = myBitrate
        AppSettings.Instance.Recording.EncoderPreset = myEncoderPreset
        AppSettings.Instance.Recording.UseNativeResolution = True
        AppSettings.Instance.Recording.Width = _nativeResolutionWidth
        AppSettings.Instance.Recording.Height = _nativeResolutionHeight

        UpdatePresetDisplay()
        EnableMyPresetControls(True)
        UpdatePresetColors()
    End Sub

    ' My Preset: MyMedium
    Private Sub ApplyMyMediumPreset()
        ' Force Native resolution
        _currentResolutionIndex = 0
        _currentResolution = NATIVE_RESOLUTION_KEY
        If Resolution_BOX IsNot Nothing Then Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)

        UpdateBitrateLimits()

        Dim myFPS As Integer = AppSettings.Instance.Recording.MyMediumFPS.GetValueOrDefault(60)
        Dim myBitrate As Integer = AppSettings.Instance.Recording.MyMediumBitrate.GetValueOrDefault(7000)
        Dim myEncoderPreset As Integer = AppSettings.Instance.Recording.MyMediumEncoderPreset.GetValueOrDefault(4)

        If FPS_BOX IsNot Nothing Then
            FPS_BOX.Text = ValidateFPS(myFPS, _currentResolution).ToString()
        End If

        SetBitrateValue(myBitrate)

        AppSettings.Instance.Recording.FPS = myFPS
        AppSettings.Instance.Recording.Bitrate = myBitrate
        AppSettings.Instance.Recording.EncoderPreset = myEncoderPreset
        AppSettings.Instance.Recording.UseNativeResolution = True
        AppSettings.Instance.Recording.Width = _nativeResolutionWidth
        AppSettings.Instance.Recording.Height = _nativeResolutionHeight

        UpdatePresetDisplay()
        EnableMyPresetControls(True)
        UpdatePresetColors()
    End Sub

    ' My Preset: MyHigh
    Private Sub ApplyMyHighPreset()
        ' Force Native resolution
        _currentResolutionIndex = 0
        _currentResolution = NATIVE_RESOLUTION_KEY
        If Resolution_BOX IsNot Nothing Then Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)

        UpdateBitrateLimits()

        Dim myFPS As Integer = AppSettings.Instance.Recording.MyHighFPS.GetValueOrDefault(60)
        Dim myBitrate As Integer = AppSettings.Instance.Recording.MyHighBitrate.GetValueOrDefault(12000)
        Dim myEncoderPreset As Integer = AppSettings.Instance.Recording.MyHighEncoderPreset.GetValueOrDefault(2)

        If FPS_BOX IsNot Nothing Then
            FPS_BOX.Text = ValidateFPS(myFPS, _currentResolution).ToString()
        End If

        SetBitrateValue(myBitrate)

        AppSettings.Instance.Recording.FPS = myFPS
        AppSettings.Instance.Recording.Bitrate = myBitrate
        AppSettings.Instance.Recording.EncoderPreset = myEncoderPreset
        AppSettings.Instance.Recording.UseNativeResolution = True
        AppSettings.Instance.Recording.Width = _nativeResolutionWidth
        AppSettings.Instance.Recording.Height = _nativeResolutionHeight

        UpdatePresetDisplay()
        EnableMyPresetControls(True)
        UpdatePresetColors()
    End Sub

    ' My Preset: Recommended (ALL LOCKED)
    Private Sub ApplyRecommendedPreset()
        ' Force Native resolution
        _currentResolutionIndex = 0
        _currentResolution = NATIVE_RESOLUTION_KEY
        If Resolution_BOX IsNot Nothing Then Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)

        UpdateBitrateLimits()

        Dim limits As BitrateLimit = GetBitrateLimits(NATIVE_RESOLUTION_KEY)

        If FPS_BOX IsNot Nothing Then
            FPS_BOX.Text = ValidateFPS(60, _currentResolution).ToString()
        End If

        SetBitrateValue(limits.RecommendedMax)

        AppSettings.Instance.Recording.UseNativeResolution = True
        AppSettings.Instance.Recording.Width = _nativeResolutionWidth
        AppSettings.Instance.Recording.Height = _nativeResolutionHeight
        AppSettings.Instance.Recording.FPS = 60
        AppSettings.Instance.Recording.Bitrate = limits.RecommendedMax
        AppSettings.Instance.Recording.EncoderPreset = 4

        UpdatePresetDisplay()
        EnableMyPresetControls(False)
        UpdatePresetColors()
    End Sub

    ' My Preset: Maximum (ALL LOCKED)
    Private Sub ApplyMaximumPreset()
        ' Force Native resolution
        _currentResolutionIndex = 0
        _currentResolution = NATIVE_RESOLUTION_KEY
        If Resolution_BOX IsNot Nothing Then Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)

        UpdateBitrateLimits()

        Dim limits As BitrateLimit = GetBitrateLimits(NATIVE_RESOLUTION_KEY)
        Dim fpsLimits As FPSLimit = GetFPSLimits(NATIVE_RESOLUTION_KEY)
        Dim maxFPS As Integer = Math.Min(144, fpsLimits.MaxFPS)

        If FPS_BOX IsNot Nothing Then
            FPS_BOX.Text = ValidateFPS(maxFPS, _currentResolution).ToString()
        End If

        SetBitrateValue(limits.MaxBitrate)

        AppSettings.Instance.Recording.UseNativeResolution = True
        AppSettings.Instance.Recording.Width = _nativeResolutionWidth
        AppSettings.Instance.Recording.Height = _nativeResolutionHeight
        AppSettings.Instance.Recording.FPS = maxFPS
        AppSettings.Instance.Recording.Bitrate = limits.MaxBitrate
        AppSettings.Instance.Recording.EncoderPreset = 7

        UpdatePresetDisplay()
        EnableMyPresetControls(False)
        UpdatePresetColors()
    End Sub

    ' NVIDIA Preset: UpdateControlsFromPreset (String-based, no enum)
    Private Sub UpdateControlsFromPreset(presetName As String)
        If Not NVIDIA_PRESETS.ContainsKey(presetName) Then
            Debug.WriteLine("UpdateControlsFromPreset: Unknown preset " & presetName)
            Exit Sub
        End If

        Dim pv As PresetValues = NVIDIA_PRESETS(presetName)

        If FPS_BOX IsNot Nothing Then
            FPS_BOX.Text = ValidateFPS(pv.FPS, _currentResolution).ToString()
        End If

        If pv.UseNativeResolution Then
            _currentResolutionIndex = 0
            _currentResolution = NATIVE_RESOLUTION_KEY
            Resolution_BOX.Text = LangHelper.GetText("l10n.native", _nativeResolution)
        End If

        ' Update TrackBar range to match the preset's resolution FIRST
        UpdateBitrateLimits()

        SetBitrateValue(pv.Bitrate)

        AppSettings.Instance.Recording.FPS = pv.FPS
        AppSettings.Instance.Recording.Bitrate = pv.Bitrate
        AppSettings.Instance.Recording.EncoderPreset = pv.EncoderPreset

        UpdatePresetDisplay()
        EnableCustomControls(False)
        UpdatePresetColors()
    End Sub

    ' Controls Accessibility

    Private Sub EnableCustomControls(enabled As Boolean)
        ApplyControlLockState(FPS_BOX, Not enabled, fps_bg, FPS_DROP)
        ApplyControlLockState(Resolution_BOX, Not enabled, Resolution_bg, Resolution_DROP)
        ApplyControlLockState(P_BOX, Not enabled, P_bg)

        If cmbEncoder IsNot Nothing Then
            cmbEncoder.ForeColor = COLOR_ENABLED
            cmbEncoder.Cursor = Cursors.Hand
        End If
        If Encoder_bg IsNot Nothing Then Encoder_bg.Cursor = Cursors.Hand
        If Encoder_DROP IsNot Nothing Then Encoder_DROP.Visible = True

        If TrackBar_BITRATE IsNot Nothing Then TrackBar_BITRATE.Enabled = enabled

        UpdatePresetStatusLabel()
    End Sub

    Private Sub EnableMyPresetControls(enabled As Boolean)
        ApplyControlLockState(FPS_BOX, Not enabled, fps_bg, FPS_DROP)
        ApplyControlLockState(Resolution_BOX, True, Resolution_bg, Resolution_DROP)

        If cmbEncoder IsNot Nothing Then
            cmbEncoder.ForeColor = COLOR_ENABLED
            cmbEncoder.Cursor = Cursors.Hand
        End If
        If Encoder_bg IsNot Nothing Then Encoder_bg.Cursor = Cursors.Hand
        If Encoder_DROP IsNot Nothing Then Encoder_DROP.Visible = True

        ApplyControlLockState(P_BOX, Not enabled, P_bg)

        If TrackBar_BITRATE IsNot Nothing Then TrackBar_BITRATE.Enabled = enabled

        UpdatePresetStatusLabel()
    End Sub

    Private Sub UpdateUIFromPreset()
        Select Case AppSettings.Instance.Recording.Preset
            Case "Low"
                ActivePresetGroup = PresetGroup.NVIDIA
                ActiveMyPresetLevel = ""
                UpdateControlsFromPreset("Low")
            Case "Medium"
                ActivePresetGroup = PresetGroup.NVIDIA
                ActiveMyPresetLevel = ""
                UpdateControlsFromPreset("Medium")
            Case "High"
                ActivePresetGroup = PresetGroup.NVIDIA
                ActiveMyPresetLevel = ""
                UpdateControlsFromPreset("High")
            Case "MyLow"
                ActivePresetGroup = PresetGroup.My
                ActiveMyPresetLevel = "Low"
                ApplyMyLowPreset()
            Case "MyMedium"
                ActivePresetGroup = PresetGroup.My
                ActiveMyPresetLevel = "Medium"
                ApplyMyMediumPreset()
            Case "MyHigh"
                ActivePresetGroup = PresetGroup.My
                ActiveMyPresetLevel = "High"
                ApplyMyHighPreset()
            Case "Recommended"
                ActivePresetGroup = PresetGroup.My
                ActiveMyPresetLevel = "Recommended"
                ApplyRecommendedPreset()
            Case "Maximum"
                ActivePresetGroup = PresetGroup.My
                ActiveMyPresetLevel = "Maximum"
                ApplyMaximumPreset()
            Case "Custom"
                ActivePresetGroup = PresetGroup.NVIDIA
                ActiveMyPresetLevel = ""
                EnableCustomControls(True)
                UpdateBitrateLimits()
                UpdateFPSLimit()
        End Select
        UpdatePresetColors()
    End Sub
#End Region

#Region "Encoder Info"
    Public Sub UpdateEncoderInfo()
        If lblEncoderInfo Is Nothing Then Exit Sub

        ' String-based matching instead of VideoEncoder enum
        Select Case _currentEncoderName
            Case "NVENC_H264", "NVENC_HEVC"
                lblEncoderInfo.Text = LangHelper.GetText("l10n.encoderNvenc")
                lblEncoderInfo.ForeColor = COLOR_ACTIVE
            Case "NVENC_AV1"
                lblEncoderInfo.Text = LangHelper.GetText("l10n.encoderNvencAV1")
                lblEncoderInfo.ForeColor = COLOR_ACTIVE
            Case "QuickSync_H264", "QuickSync_HEVC"
                lblEncoderInfo.Text = LangHelper.GetText("l10n.encoderQuickSync")
                lblEncoderInfo.ForeColor = Color.FromArgb(0, 150, 255)
            Case "AMF_H264", "AMF_HEVC"
                lblEncoderInfo.Text = LangHelper.GetText("l10n.encoderAMF")
                lblEncoderInfo.ForeColor = Color.FromArgb(237, 28, 36)
            Case Else
                lblEncoderInfo.Text = LangHelper.GetText("l10n.encoderCPU")
                lblEncoderInfo.ForeColor = Color.Orange
        End Select
    End Sub
#End Region

#Region "Quality Timer"
    Private _lastKnownPreset As String = String.Empty

    Private Sub Quality_Tick(sender As Object, e As EventArgs) Handles Quality.Tick
        Try
            Dim currentPreset As String = AppSettings.Instance.Recording.Preset
            Dim presetChanged As Boolean = (currentPreset <> _lastKnownPreset)
            _lastKnownPreset = currentPreset

            Select Case currentPreset
                Case "Custom"
                    ResetAllPresetColors()
                    EnableCustomControls(True)
                    If C_BG IsNot Nothing Then C_BG.BackColor = COLOR_ACTIVE
                    If C_ICO IsNot Nothing Then C_ICO.BackColor = COLOR_ACTIVE
                    If C_TEXT IsNot Nothing Then C_TEXT.BackColor = COLOR_ACTIVE
                    If presetChanged Then UpdateBitrateLimits()

                Case "MyLow", "MyMedium", "MyHigh"
                    EnableMyPresetControls(True)
                    UpdatePresetColors()

                Case "Recommended", "Maximum"
                    EnableMyPresetControls(False)
                    UpdatePresetColors()

                Case Else
                    EnableCustomControls(False)
                    UpdatePresetColors()
            End Select
        Catch
        End Try
    End Sub

    Private Sub ResetAllPresetColors()
        ' NVIDIA Preset
        If Label11 IsNot Nothing Then Label11.BackColor = COLOR_INACTIVE
        If Label10 IsNot Nothing Then Label10.BackColor = COLOR_INACTIVE
        If low IsNot Nothing Then low.BackColor = COLOR_INACTIVE
        If PictureBox1 IsNot Nothing Then PictureBox1.BackColor = COLOR_INACTIVE
        If Label8 IsNot Nothing Then Label8.BackColor = COLOR_INACTIVE
        If Label9 IsNot Nothing Then Label9.BackColor = COLOR_INACTIVE
        If PictureBox2 IsNot Nothing Then PictureBox2.BackColor = COLOR_INACTIVE
        If Label7 IsNot Nothing Then Label7.BackColor = COLOR_INACTIVE
        If Label6 IsNot Nothing Then Label6.BackColor = COLOR_INACTIVE
        If C_BG IsNot Nothing Then C_BG.BackColor = COLOR_INACTIVE
        If C_ICO IsNot Nothing Then C_ICO.BackColor = COLOR_INACTIVE
        If C_TEXT IsNot Nothing Then C_TEXT.BackColor = COLOR_INACTIVE

        ' My Preset
        If ML_BG IsNot Nothing Then ML_BG.BackColor = COLOR_INACTIVE
        If ML_ICO IsNot Nothing Then ML_ICO.BackColor = COLOR_INACTIVE
        If ML_TEXT IsNot Nothing Then ML_TEXT.BackColor = COLOR_INACTIVE
        If MM_BG IsNot Nothing Then MM_BG.BackColor = COLOR_INACTIVE
        If MM_ICO IsNot Nothing Then MM_ICO.BackColor = COLOR_INACTIVE
        If MM_TEXT IsNot Nothing Then MM_TEXT.BackColor = COLOR_INACTIVE
        If MH_BG IsNot Nothing Then MH_BG.BackColor = COLOR_INACTIVE
        If MH_ICO IsNot Nothing Then MH_ICO.BackColor = COLOR_INACTIVE
        If MH_TEXT IsNot Nothing Then MH_TEXT.BackColor = COLOR_INACTIVE
        If Recommended_BG IsNot Nothing Then Recommended_BG.BackColor = COLOR_INACTIVE
        If Recommended_ICO IsNot Nothing Then Recommended_ICO.BackColor = COLOR_INACTIVE
        If Recommended_TEXT IsNot Nothing Then Recommended_TEXT.BackColor = COLOR_INACTIVE
        If Maximum_BG IsNot Nothing Then Maximum_BG.BackColor = COLOR_INACTIVE
        If Maximum_ICO IsNot Nothing Then Maximum_ICO.BackColor = COLOR_INACTIVE
        If Maximum_TEXT IsNot Nothing Then Maximum_TEXT.BackColor = COLOR_INACTIVE
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
            Case "Custom"
                If C_BG IsNot Nothing Then C_BG.BackColor = COLOR_ACTIVE
                If C_ICO IsNot Nothing Then C_ICO.BackColor = COLOR_ACTIVE
                If C_TEXT IsNot Nothing Then C_TEXT.BackColor = COLOR_ACTIVE

            Case "MyLow"
                If ML_BG IsNot Nothing Then ML_BG.BackColor = COLOR_ACTIVE
                If ML_ICO IsNot Nothing Then ML_ICO.BackColor = COLOR_ACTIVE
                If ML_TEXT IsNot Nothing Then ML_TEXT.BackColor = COLOR_ACTIVE
            Case "MyMedium"
                If MM_BG IsNot Nothing Then MM_BG.BackColor = COLOR_ACTIVE
                If MM_ICO IsNot Nothing Then MM_ICO.BackColor = COLOR_ACTIVE
                If MM_TEXT IsNot Nothing Then MM_TEXT.BackColor = COLOR_ACTIVE
            Case "MyHigh"
                If MH_BG IsNot Nothing Then MH_BG.BackColor = COLOR_ACTIVE
                If MH_ICO IsNot Nothing Then MH_ICO.BackColor = COLOR_ACTIVE
                If MH_TEXT IsNot Nothing Then MH_TEXT.BackColor = COLOR_ACTIVE
            Case "Recommended"
                If Recommended_BG IsNot Nothing Then Recommended_BG.BackColor = COLOR_ACTIVE
                If Recommended_ICO IsNot Nothing Then Recommended_ICO.BackColor = COLOR_ACTIVE
                If Recommended_TEXT IsNot Nothing Then Recommended_TEXT.BackColor = COLOR_ACTIVE
            Case "Maximum"
                If Maximum_BG IsNot Nothing Then Maximum_BG.BackColor = COLOR_ACTIVE
                If Maximum_ICO IsNot Nothing Then Maximum_ICO.BackColor = COLOR_ACTIVE
                If Maximum_TEXT IsNot Nothing Then Maximum_TEXT.BackColor = COLOR_ACTIVE
        End Select

        UpdatePresetStatusLabel()
    End Sub
#End Region

#Region "Hover Effects"
    ' NVIDIA Preset: Low
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

    ' NVIDIA Preset: Medium
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

    ' NVIDIA Preset: High
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

    ' NVIDIA Preset: Custom
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

    ' My Preset: MyLow
    Private Sub ML_MouseMove(sender As Object, e As MouseEventArgs) Handles ML_BG.MouseMove, ML_ICO.MouseMove, ML_TEXT.MouseMove
        If MH_HB IsNot Nothing Then MH_HB.Visible = True
        If MH_HL IsNot Nothing Then MH_HL.Visible = True
        If MH_HR IsNot Nothing Then MH_HR.Visible = True
        If MH_HT IsNot Nothing Then MH_HT.Visible = True
    End Sub

    Private Sub ML_MouseLeave(sender As Object, e As EventArgs) Handles ML_BG.MouseLeave, ML_ICO.MouseLeave, ML_TEXT.MouseLeave
        If MH_HB IsNot Nothing Then MH_HB.Visible = False
        If MH_HL IsNot Nothing Then MH_HL.Visible = False
        If MH_HR IsNot Nothing Then MH_HR.Visible = False
        If MH_HT IsNot Nothing Then MH_HT.Visible = False
    End Sub

    ' My Preset: MyMedium
    Private Sub MM_MouseMove(sender As Object, e As MouseEventArgs) Handles MM_BG.MouseMove, MM_ICO.MouseMove, MM_TEXT.MouseMove
        If MM_HB IsNot Nothing Then MM_HB.Visible = True
        If MM_HL IsNot Nothing Then MM_HL.Visible = True
        If MM_HR IsNot Nothing Then MM_HR.Visible = True
        If MM_HT IsNot Nothing Then MM_HT.Visible = True
    End Sub

    Private Sub MM_MouseLeave(sender As Object, e As EventArgs) Handles MM_BG.MouseLeave, MM_ICO.MouseLeave, MM_TEXT.MouseLeave
        If MM_HB IsNot Nothing Then MM_HB.Visible = False
        If MM_HL IsNot Nothing Then MM_HL.Visible = False
        If MM_HR IsNot Nothing Then MM_HR.Visible = False
        If MM_HT IsNot Nothing Then MM_HT.Visible = False
    End Sub

    ' My Preset: MyHigh
    Private Sub MH_MouseMove(sender As Object, e As MouseEventArgs) Handles MH_BG.MouseMove, MH_ICO.MouseMove, MH_TEXT.MouseMove
        If ML_HB IsNot Nothing Then ML_HB.Visible = True
        If ML_HL IsNot Nothing Then ML_HL.Visible = True
        If ML_HR IsNot Nothing Then ML_HR.Visible = True
        If ML_HT IsNot Nothing Then ML_HT.Visible = True
    End Sub

    Private Sub MH_MouseLeave(sender As Object, e As EventArgs) Handles MH_BG.MouseLeave, MH_ICO.MouseLeave, MH_TEXT.MouseLeave
        If ML_HB IsNot Nothing Then ML_HB.Visible = False
        If ML_HL IsNot Nothing Then ML_HL.Visible = False
        If ML_HR IsNot Nothing Then ML_HR.Visible = False
        If ML_HT IsNot Nothing Then ML_HT.Visible = False
    End Sub

    ' My Preset: Recommended
    Private Sub RD_MouseMove(sender As Object, e As MouseEventArgs) Handles Recommended_BG.MouseMove, Recommended_ICO.MouseMove, Recommended_TEXT.MouseMove
        If RD_B IsNot Nothing Then RD_B.Visible = True
        If RD_L IsNot Nothing Then RD_L.Visible = True
        If RD_R IsNot Nothing Then RD_R.Visible = True
        If RD_T IsNot Nothing Then RD_T.Visible = True
    End Sub

    Private Sub RD_MouseLeave(sender As Object, e As EventArgs) Handles Recommended_BG.MouseLeave, Recommended_ICO.MouseLeave, Recommended_TEXT.MouseLeave
        If RD_B IsNot Nothing Then RD_B.Visible = False
        If RD_L IsNot Nothing Then RD_L.Visible = False
        If RD_R IsNot Nothing Then RD_R.Visible = False
        If RD_T IsNot Nothing Then RD_T.Visible = False
    End Sub

    ' My Preset: Maximum
    Private Sub MX_MouseMove(sender As Object, e As MouseEventArgs) Handles Maximum_BG.MouseMove, Maximum_ICO.MouseMove, Maximum_TEXT.MouseMove
        If MX_B IsNot Nothing Then MX_B.Visible = True
        If MX_L IsNot Nothing Then MX_L.Visible = True
        If MX_R IsNot Nothing Then MX_R.Visible = True
        If MX_T IsNot Nothing Then MX_T.Visible = True
    End Sub

    Private Sub MX_MouseLeave(sender As Object, e As EventArgs) Handles Maximum_BG.MouseLeave, Maximum_ICO.MouseLeave, Maximum_TEXT.MouseLeave
        If MX_B IsNot Nothing Then MX_B.Visible = False
        If MX_L IsNot Nothing Then MX_L.Visible = False
        If MX_R IsNot Nothing Then MX_R.Visible = False
        If MX_T IsNot Nothing Then MX_T.Visible = False
    End Sub

    ' ALTZ Timer
    Private Sub ALTZ_Tick(sender As Object, e As EventArgs) Handles Recoed_IF.Tick
        If Base.ReplayValue OrElse Base.RecordValue Then
            Panel_SET.Visible = False
            Panel_SET.Enabled = False
            captrueblock.Visible = True
            captrueblock_ico.Visible = True
            captrueblock_sub.Visible = True
        Else
            Panel_SET.Visible = True
            Panel_SET.Enabled = True
            captrueblock.Visible = False
            captrueblock_ico.Visible = False
            captrueblock_sub.Visible = False
        End If
    End Sub
#End Region

#Region "Command Preview"
    ' ✅ PHASE 3 (UI spec §12/§14.4): the old preview sent "GET_FFMPEG_ARGS" —
    ' a command NO engine handler implements ([Engine] Client.vb:245-283
    ' dispatches engine_* commands only; GET_FFMPEG_ARGS fell into Case Else)
    ' — so this box forever showed "engine not connected" (dead feature,
    ' never worked). Replaced with a truthful LOCAL summary of the Requested
    ' layer (config.json model) + regime labels. The live
    ' Requested→Effective→Actual view is the Engine WinForms diagnostics
    ' panel (UI_Engine txtDiagnostics, PHASE 3).
    Public Sub UpdateCommandPreview()
        If prearg IsNot Nothing Then
            Try
                Dim r As AppSettings.RecordingSettingsClass = AppSettings.Instance.Recording
                Dim presetText As String = "p" & Math.Max(1, Math.Min(7, r.EncoderPreset)).ToString()
                Dim resText As String = If(r.UseNativeResolution, "native", String.Format("{0}x{1}", r.Width, r.Height))
                Dim apiText As String = If(String.IsNullOrEmpty(r.APICapture), "ddagrab (built-in)", r.APICapture)
                prearg.Text = String.Format(
                    "Requested (config.json): {0} · {1} fps (live per record) · {2} kbps (engine restart) · {3} (engine restart) · preset {4} (engine restart) · Capture API: {5}",
                    r.Encoder, r.FPS, r.Bitrate, resText, presetText, apiText)
            Catch ex As Exception
                prearg.Text = "(config not loaded)"
            End Try
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        UpdateCommandPreview()
    End Sub

    Private _copyResetTimer As System.Windows.Forms.Timer

    Private Sub Button_Copy_Click(sender As Object, e As EventArgs) Handles Button_Copy.Click
        Try
            If prearg IsNot Nothing AndAlso Not String.IsNullOrEmpty(prearg.Text) Then
                Clipboard.SetText(prearg.Text)
                Dim originalText As String = Button_Copy.Text
                Button_Copy.Text = LangHelper.GetText("l10n.copied")
                Button_Copy.BackColor = Color.FromArgb(0, 150, 0)

                If _copyResetTimer IsNot Nothing Then
                    _copyResetTimer.Stop()
                    _copyResetTimer.Dispose()
                End If

                _copyResetTimer = New Timer With {.Interval = 1500}
                AddHandler _copyResetTimer.Tick, Sub(s, args)
                                                     _copyResetTimer.Stop()
                                                     _copyResetTimer.Dispose()
                                                     _copyResetTimer = Nothing
                                                     If Me.IsDisposed Then Return
                                                     Button_Copy.Text = originalText
                                                     Button_Copy.BackColor = Color.FromArgb(33, 35, 38)
                                                 End Sub
                _copyResetTimer.Start()
            End If
        Catch ex As Exception
            MessageBox.Show(LangHelper.GetText("l10n.copyFailed", ex.Message), LangHelper.GetText("l10n.error"), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
#End Region

#Region "Encoder Preset System"
    Private Function GetEncoderPresets(encoderName As String) As String()
        Select Case encoderName
            Case "NVENC_H264", "NVENC_HEVC", "NVENC_AV1"
                Return {"P1", "P2", "P3", "P4", "P5", "P6", "P7"}
            Case "QuickSync_H264", "QuickSync_HEVC"
                Return {"veryslow", "slower", "slow", "medium", "fast", "faster", "veryfast"}
            Case "AMF_H264", "AMF_HEVC"
                Return {"quality", "balanced", "speed"}
            Case "LibX264", "LibX265"
                Return {"slow", "medium", "fast", "faster", "veryfast", "superfast", "ultrafast"}
            Case Else
                Return {"medium"}
        End Select
    End Function

    Private Sub UpdatePresetDisplay()
        If P_BOX Is Nothing Then Exit Sub

        Dim savedIndex As Integer = AppSettings.Instance.Recording.EncoderPreset
        savedIndex = Math.Max(1, Math.Min(7, savedIndex))

        If _currentEncoderName.StartsWith("AMF") Then
            Select Case savedIndex
                Case 1, 2 : _currentPresetName = "quality"
                Case 3, 4 : _currentPresetName = "balanced"
                Case Else : _currentPresetName = "speed"
            End Select
        Else
            Dim presets As String() = GetEncoderPresets(_currentEncoderName)
            If savedIndex <= presets.Length Then
                _currentPresetName = presets(savedIndex - 1)
            Else
                _currentPresetName = presets(presets.Length - 1)
            End If
        End If

        P_BOX.Text = _currentPresetName
        UpdatePresetTooltip()
    End Sub

    Private Function PresetNameToIndex(presetName As String) As Integer
        If _currentEncoderName.StartsWith("AMF") Then
            Select Case presetName.ToLowerInvariant()
                Case "quality" : Return 2
                Case "balanced" : Return 4
                Case "speed" : Return 6
                Case Else : Return 4
            End Select
        End If

        Dim presets As String() = GetEncoderPresets(_currentEncoderName)
        For i As Integer = 0 To presets.Length - 1
            If String.Equals(presets(i), presetName, StringComparison.OrdinalIgnoreCase) Then Return i + 1
        Next
        Return 4
    End Function
#End Region

#Region "Styled Dropdown Menus"
    Private Shared ReadOnly COLOR_MENU_BG As Color = Color.FromArgb(30, 30, 34)
    Private Shared ReadOnly COLOR_MENU_FG As Color = Color.FromArgb(220, 220, 220)
    Private Shared ReadOnly COLOR_MENU_SELECTED As Color = Color.FromArgb(100, 149, 237)

    Private Function CreateStyledMenu() As ContextMenuStrip
        Dim menu As New ContextMenuStrip()
        menu.BackColor = COLOR_MENU_BG
        menu.ForeColor = COLOR_MENU_FG
        menu.Font = New Font("Segoe UI", 10)
        menu.ShowImageMargin = False
        menu.RenderMode = ToolStripRenderMode.System
        Return menu
    End Function

    ' FPS
    Private Sub FPS_BOX_Click(sender As Object, e As EventArgs) Handles FPS_BOX.Click, FPS_DROP.Click
        If Not IsEditablePreset() Then Exit Sub
        If FPS_BOX Is Nothing Then Exit Sub

        Dim limits As FPSLimit = GetFPSLimits(_currentResolution)
        Dim cms As ContextMenuStrip = CreateStyledMenu()
        Dim currentFPS As Integer = GetCurrentFPS()

        Dim commonFPS As Integer() = {30, 60, 120, 144, 240}
        For Each fps As Integer In commonFPS
            If fps >= limits.MinFPS AndAlso fps <= limits.MaxFPS Then
                Dim lbl As String = fps.ToString() & " FPS"
                Dim item As New ToolStripMenuItem(lbl) With {.Tag = fps}
                If fps = currentFPS Then item.ForeColor = COLOR_MENU_SELECTED
                AddHandler item.Click, AddressOf FPSMenuItem_Click
                cms.Items.Add(item)
            End If
        Next

        cms.Show(FPS_BOX, 0, FPS_BOX.Height)
        FPS_BOX.BackColor = Color.FromArgb(33, 35, 38)
        FPS_DROP.Visible = False
        FPS_DROP.BackColor = Color.FromArgb(33, 35, 38)
        fps_bg.BackColor = Color.FromArgb(33, 35, 38)
        fps_bg.Cursor = Cursors.Default

        _menuRestoreDrop = FPS_DROP
        _menuRestoreBg = fps_bg
        AddHandler cms.Closed, AddressOf DropdownMenu_Closed
    End Sub

    Private Sub FPSMenuItem_Click(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        Dim fps As Integer = CInt(item.Tag)
        FPS_BOX.Text = fps.ToString()
        If IsEditablePreset() Then SaveCurrentSettings()
    End Sub

    ' Resolution
    Private Sub Resolution_BOX_Click(sender As Object, e As EventArgs) Handles Resolution_BOX.Click, Resolution_DROP.Click
        If AppSettings.Instance.Recording.Preset <> "Custom" Then Exit Sub
        If Resolution_BOX Is Nothing Then Exit Sub

        Dim cms As ContextMenuStrip = CreateStyledMenu()
        Dim currentRes As String = _currentResolution

        Dim nativeDisplay As String = LangHelper.GetText("l10n.native", _nativeResolution)
        Dim nativeItem As New ToolStripMenuItem(nativeDisplay) With {.Tag = NATIVE_RESOLUTION_KEY}
        If currentRes = NATIVE_RESOLUTION_KEY OrElse currentRes.StartsWith(NATIVE_RESOLUTION_KEY & " (") Then nativeItem.ForeColor = COLOR_MENU_SELECTED
        AddHandler nativeItem.Click, AddressOf ResolutionMenuItem_Click
        cms.Items.Add(nativeItem)

        cms.Items.Add(New ToolStripSeparator())

        Dim commonResolutions As String() = {"1920x1080", "2560x1440", "3840x2160", "1280x720", "1366x768", "1600x900", "2560x1080", "3440x1440"}
        For Each res As String In commonResolutions
            If res <> _nativeResolution Then
                Dim item As New ToolStripMenuItem(res) With {.Tag = res}
                If currentRes = res Then item.ForeColor = COLOR_MENU_SELECTED
                AddHandler item.Click, AddressOf ResolutionMenuItem_Click
                cms.Items.Add(item)
            End If
        Next

        cms.Show(Resolution_BOX, 0, Resolution_BOX.Height)
        Resolution_BOX.BackColor = Color.FromArgb(33, 35, 38)
        Resolution_DROP.Visible = False
        Resolution_bg.BackColor = Color.FromArgb(33, 35, 38)
        Resolution_bg.Cursor = Cursors.Default
        Resolution_DROP.BackColor = Color.FromArgb(33, 35, 38)

        _menuRestoreDrop = Resolution_DROP
        _menuRestoreBg = Resolution_bg
        AddHandler cms.Closed, AddressOf DropdownMenu_Closed
    End Sub

    Private Sub ResolutionMenuItem_Click(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        Dim res As String = CStr(item.Tag)
        ApplyResolutionSelection(res)
    End Sub

    ' Encoder
    Private Sub cmbEncoder_Click(sender As Object, e As EventArgs) Handles cmbEncoder.Click, Encoder_DROP.Click
        If cmbEncoder Is Nothing Then Exit Sub

        Dim cms As ContextMenuStrip = CreateStyledMenu()
        Dim currentEncoder As String = _currentEncoderName

        If AppSettings.HasNvidia Then
            AddEncoderMenuItem(cms, "NVENC_H264", "NVIDIA NVENC H.264", currentEncoder)
            AddEncoderMenuItem(cms, "NVENC_HEVC", "NVIDIA NVENC HEVC", currentEncoder)
            If AppSettings.SupportsNVENCAV1 Then AddEncoderMenuItem(cms, "NVENC_AV1", "NVIDIA NVENC AV1", currentEncoder)
            cms.Items.Add(New ToolStripSeparator())
        End If

        If AppSettings.HasIntel Then
            AddEncoderMenuItem(cms, "QuickSync_H264", "Intel QuickSync H.264", currentEncoder)
            AddEncoderMenuItem(cms, "QuickSync_HEVC", "Intel QuickSync HEVC", currentEncoder)
            cms.Items.Add(New ToolStripSeparator())
        End If

        ' AMD AMF disabled - reserved for future support

        AddEncoderMenuItem(cms, "LibX264", "Software x264", currentEncoder)
        AddEncoderMenuItem(cms, "LibX265", "Software x265", currentEncoder)

        If cms.Items.Count > 0 AndAlso TypeOf cms.Items(cms.Items.Count - 1) Is ToolStripSeparator Then
            cms.Items.RemoveAt(cms.Items.Count - 1)
        End If

        cms.Show(cmbEncoder, 0, cmbEncoder.Height)
        cmbEncoder.BackColor = Color.FromArgb(33, 35, 38)
        Encoder_DROP.Visible = False
        Encoder_DROP.BackColor = Color.FromArgb(33, 35, 38)
        Encoder_bg.BackColor = Color.FromArgb(33, 35, 38)
        Encoder_bg.Cursor = Cursors.Default

        _menuRestoreDrop = Encoder_DROP
        _menuRestoreBg = Encoder_bg
        AddHandler cms.Closed, AddressOf DropdownMenu_Closed
    End Sub

    Private Sub AddEncoderMenuItem(cms As ContextMenuStrip, encoderKey As String, displayName As String, currentEncoder As String)
        Dim item As New ToolStripMenuItem(displayName) With {.Tag = encoderKey}
        If encoderKey = currentEncoder Then item.ForeColor = COLOR_MENU_SELECTED
        AddHandler item.Click, AddressOf EncoderMenuItem_Click
        cms.Items.Add(item)
    End Sub

    Private Sub EncoderMenuItem_Click(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        Dim enc As String = CStr(item.Tag)
        ApplyEncoderSelection(enc)

        If IsEditablePreset() Then
            SaveCurrentSettings()
        Else
            AppSettings.Instance.Save()
        End If
    End Sub

    ' Preset P
    Private Sub P_BOX_Click(sender As Object, e As EventArgs) Handles P_BOX.Click
        If Not IsEditablePreset() Then Exit Sub
        If P_BOX Is Nothing Then Exit Sub

        Dim cms As ContextMenuStrip = CreateStyledMenu()
        Dim presets As String() = GetEncoderPresets(_currentEncoderName)
        Dim currentPreset As String = _currentPresetName

        For Each preset As String In presets
            Dim item As New ToolStripMenuItem(preset) With {.Tag = preset}
            If String.Equals(preset, currentPreset, StringComparison.OrdinalIgnoreCase) Then
                item.ForeColor = COLOR_MENU_SELECTED
            End If
            AddHandler item.Click, AddressOf PMenuItem_Click
            cms.Items.Add(item)
        Next

        cms.Show(P_BOX, 0, P_BOX.Height)
        P_BOX.BackColor = Color.FromArgb(33, 35, 38)
        P_bg.BackColor = Color.FromArgb(33, 35, 38)
        P_bg.Cursor = Cursors.Default

        _menuRestoreDrop = Nothing
        _menuRestoreBg = P_bg
        AddHandler cms.Closed, AddressOf DropdownMenu_Closed
    End Sub

    Private Sub DropdownMenu_Closed(sender As Object, e As ToolStripDropDownClosedEventArgs)
        If _menuRestoreDrop IsNot Nothing Then _menuRestoreDrop.Visible = True
        If _menuRestoreBg IsNot Nothing Then _menuRestoreBg.Cursor = Cursors.Hand
        _menuRestoreDrop = Nothing
        _menuRestoreBg = Nothing

        Dim cms As ContextMenuStrip = TryCast(sender, ContextMenuStrip)
        If cms IsNot Nothing Then RemoveHandler cms.Closed, AddressOf DropdownMenu_Closed
    End Sub

    Private Sub PMenuItem_Click(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        Dim preset As String = CStr(item.Tag)
        _currentPresetName = preset
        P_BOX.Text = preset
        UpdatePresetTooltip()
        AppSettings.Instance.Recording.EncoderPreset = PresetNameToIndex(preset)
        If IsEditablePreset() Then SaveCurrentSettings()
    End Sub

    Private Sub Engine_Mode1_BgSub_MouseMove(sender As Object, e As MouseEventArgs) Handles Engine_Mode1_BgSub.MouseMove, Engine_Mode1_Text.MouseMove
        Engine_Mode1_BgSub.BackColor = Color.FromArgb(118, 185, 0)
    End Sub

    Private Sub Engine_Mode1_Text_MouseLeave(sender As Object, e As EventArgs) Handles Engine_Mode1_Text.MouseLeave, Engine_Mode1_BgSub.MouseLeave
        Engine_Mode1_BgSub.BackColor = Color.FromArgb(33, 35, 38)
    End Sub


    Private Sub Engine_Mode2_BgSub_MouseMove(sender As Object, e As MouseEventArgs) Handles Engine_Mode2_BgSub.MouseMove, Engine_Mode2_Text.MouseMove
        Engine_Mode2_BgSub.BackColor = Color.FromArgb(118, 185, 0)
    End Sub

    Private Sub Engine_Mode2_Text_MouseLeave(sender As Object, e As EventArgs) Handles Engine_Mode2_Text.MouseLeave, Engine_Mode2_BgSub.MouseLeave
        Engine_Mode2_BgSub.BackColor = Color.FromArgb(33, 35, 38)
    End Sub

    Private Sub Engine_Mode3_BgSub_MouseMove(sender As Object, e As EventArgs) Handles Engine_Mode3_BgSub.MouseMove, Engine_Mode3_Text.MouseMove
        Engine_Mode3_BgSub.BackColor = Color.FromArgb(118, 185, 0)
    End Sub

    Private Sub Engine_Mode3_BgSub_MouseLeave(sender As Object, e As EventArgs) Handles Engine_Mode3_BgSub.MouseLeave, Engine_Mode3_Text.MouseLeave
        Engine_Mode3_BgSub.BackColor = Color.FromArgb(33, 35, 38)
    End Sub
#End Region
End Class
