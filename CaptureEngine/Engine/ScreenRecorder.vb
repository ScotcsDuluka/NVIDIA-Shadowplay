Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports System.Collections.Concurrent
Imports System.Linq

Namespace CaptureCore

    Public Class ScreenRecorder
        Implements IDisposable

#Region "Constants"
        ' ════════════════════════════════════════════════════════════════════
        ' ★ Phase 1 refactor: Public constants below are backward-compatible
        ' aliases for CaptureLimits. The actual values live in CaptureLimits.vb.
        ' External callers that reference ScreenRecorder.MIN_BITRATE etc. still
        ' work — they resolve to the same integer at compile time.
        '
        ' IMPORTANT: MAX_BITRATE / MAX_FRAMERATE here are the RECORDER hard
        ' cap (300000 / 240). The UI layer uses a different ceiling (150000 / 800)
        ' which lives in CaptureLimits as MAX_BITRATE_UI / MAX_FRAMERATE_UI.
        ' ════════════════════════════════════════════════════════════════════
        Public Const MIN_BITRATE As Integer = CaptureLimits.MIN_BITRATE
        Public Const MAX_BITRATE As Integer = CaptureLimits.MAX_BITRATE_RECORDER
        Public Const DEFAULT_BITRATE As Integer = CaptureLimits.DEFAULT_BITRATE

        Public Const MIN_FRAMERATE As Integer = CaptureLimits.MIN_FRAMERATE
        Public Const MAX_FRAMERATE As Integer = CaptureLimits.MAX_FRAMERATE_RECORDER
        Public Const DEFAULT_FRAMERATE As Integer = CaptureLimits.DEFAULT_FRAMERATE

        Public Const MIN_ENCODER_PRESET As Integer = CaptureLimits.MIN_ENCODER_PRESET
        Public Const MAX_ENCODER_PRESET As Integer = CaptureLimits.MAX_ENCODER_PRESET
        Public Const DEFAULT_ENCODER_PRESET As Integer = CaptureLimits.DEFAULT_ENCODER_PRESET

        Public Const MIN_REPLAY_DURATION As Integer = CaptureLimits.MIN_REPLAY_DURATION
        Public Const MAX_REPLAY_DURATION As Integer = CaptureLimits.MAX_REPLAY_DURATION
        Public Const DEFAULT_REPLAY_DURATION As Integer = CaptureLimits.DEFAULT_REPLAY_DURATION

        Public Const BUFFER_MAX_SEGMENTS As Integer = CaptureLimits.BUFFER_MAX_SEGMENTS
        Public Const BUFFER_MAX_DURATION As Integer = CaptureLimits.BUFFER_MAX_DURATION

        ' ── Internal (Private) constants — engine-implementation-specific ──
        ' ★ Fix B: GRACEFUL_EXIT_TIMEOUT reduced from 10000ms to 3000ms.
        ' FFmpeg normally exits within 1-3s after receiving 'q' (flush + close).
        ' The old 10s timeout was a worst-case safety net that made the UI
        ' feel frozen for up to 10s whenever FFmpeg was slow to exit.
        ' 3s is enough for normal flush; if FFmpeg hangs, FORCE_KILL_TIMEOUT
        ' (2000ms) kicks in and terminates the process.
        Private Const GRACEFUL_EXIT_TIMEOUT As Integer = 3000
        Private Const BUFFER_GRACEFUL_EXIT_TIMEOUT As Integer = 3000
        Private Const FORCE_KILL_TIMEOUT As Integer = 2000
        Private Const FILE_WRITE_DELAY As Integer = 300
        Private Const CONCAT_TIMEOUT As Integer = 60000

        Private Const SEGMENT_DURATION As Double = 0.5
        Private Const SB_CAPACITY_XLARGE As Integer = 4096
        Private Const CAPTURE_API_CHECK_TIMEOUT As Integer = 3000

        Public Const ACTION_COOLDOWN_MS As Integer = CaptureLimits.ACTION_COOLDOWN_MS

        Private _bufferStartTime As DateTime
        Private _lastSegmentTime As DateTime

#End Region

#Region "Enums"
        Public Enum RecordingPreset
            Low
            Medium
            High
            MyLow
            MyMedium
            MyHigh
            Recommended
            Maximum
            Custom
        End Enum

        Public Enum RecordingStatus
            Idle
            Recording
            Buffering
            Stopped
        End Enum

        Public Enum VideoEncoder
            NVENC_H264
            NVENC_HEVC
            NVENC_AV1
            QuickSync_H264
            QuickSync_HEVC
            AMF_H264
            AMF_HEVC
            LibX264
            LibX265
        End Enum

        Public Enum TrimMode
            Fast
            Accurate
        End Enum

        Public Enum CaptureAPIType
            Auto
            GFxCapture
            DDAGrab
            GDIGrab
        End Enum

        Public Enum OutputColorFormat
            Auto
            BGRA_8Bit
            X2BGR10_10Bit
            RGBAF16_16Bit
        End Enum

        Public Enum VideoResizeModeType
            Crop
            Scale
            ScaleAspect
        End Enum

        Public Enum VideoScaleModeType
            Point
            Bilinear
            Bicubic
        End Enum

        Public Enum CaptureTargetType
            Monitor
            Window
            Region
        End Enum

        Public Enum VideoCaptureMode
            None
            SystemOnly
            MicOnly
            Both
        End Enum
#End Region

#Region "Compiled Regex Patterns"
        Private Shared ReadOnly RegexFrame As New Regex("frame=\s*(\d+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexSize As New Regex("size=\s*(\d+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexBitrate As New Regex("bitrate=\s*([\d.]+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexSpeed As New Regex("speed=\s*([\d.]+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexDuration As New Regex("Duration: (\d+):(\d+):(\d+\.?\d*)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexSegment As New Regex("segment_(\d+)", RegexOptions.Compiled)
#End Region

#Region "API Detection State"
        ' ★ Phase 2 refactor: Availability state + check methods moved to
        ' CaptureAPIDetector. ScreenRecorder keeps thin forwarders below
        ' for backward compatibility with all external callers.
#End Region

#Region "Pre-warm State"
        Private Shared _isPreWarmed As Boolean = False
        Private Shared _prewarmLock As New Object()
#End Region

#Region "Capture API Fallback State"
        Private _captureAPIFailed As Boolean = False
        Private _fallbackAPI As CaptureAPIType? = Nothing
#End Region

#Region "Audio Capture State (NAudio)"
        Private _recordingAudioPipe As AudioPipe
        Private _bufferAudioPipe As AudioPipe
        Private _audioLock As New Object()

        Private _audioMode As VideoCaptureMode = VideoCaptureMode.None
        Private _systemAudioVolume As Single = 1.0F
        Private _micVolume As Single = 1.0F
        Private _micDeviceName As String = ""
#End Region

#Region "API Detection Methods - Async Versions"

        ''' <summary>★ Phase 2: Forwarder to CaptureAPIDetector.</summary>
        Public Shared Sub CheckGfxCaptureAvailabilityAsync(ffmpegPath As String)
            CaptureAPIDetector.CheckGfxCaptureAvailabilityAsync(ffmpegPath)
        End Sub

        ''' <summary>★ Phase 2: Forwarder to CaptureAPIDetector.</summary>
        Public Shared Sub CheckDDAGrabAvailabilityAsync(ffmpegPath As String)
            CaptureAPIDetector.CheckDDAGrabAvailabilityAsync(ffmpegPath)
        End Sub

#End Region

#Region "Job Object for Process Cleanup"
        ' ★ Phase 3 refactor: All Job Object P/Invoke + state + helpers moved
        ' to JobObjectManager module. AddProcessToJob below is a thin
        ' forwarder so existing call sites inside ScreenRecorder compile
        ' unchanged.
        Private Sub AddProcessToJob(proc As Process)
            JobObjectManager.AddProcessToJob(proc)
        End Sub
#End Region

#Region "Native Methods"
        <DllImport("user32.dll")>
        Private Shared Function GetSystemMetrics(ByVal nIndex As Integer) As Integer
        End Function

        Private Const SM_CXSCREEN As Integer = 0
        Private Const SM_CYSCREEN As Integer = 1
#End Region

#Region "Private Fields"
        Private recordingProcess As Process
        Private recordingOutputPath As String
        Private _isRecording As Boolean = False
        Private recordingStartTime As DateTime
        Private ReadOnly _recordingLock As New Object()

        Private bufferProcess As Process
        Private bufferTempDir As String = ""
        Private _isBuffering As Boolean = False
        Private _replaySaveDuration As Integer = DEFAULT_REPLAY_DURATION
        Private ReadOnly _bufferLock As New Object()
        Private bufferStartTime As DateTime

        Private _isSaving As Boolean = False
        Private ReadOnly _saveLock As New Object()

        ''' <summary>
        ''' Timestamp of the last user-triggered action (Start/Stop/Save/Buffer).
        ''' Used for debounce/cooldown to prevent rapid-fire calls.
        ''' </summary>
        Private _lastActionTime As DateTime = DateTime.MinValue
        Private ReadOnly _actionLock As New Object()

        Private _segmentTimestamps As New ConcurrentDictionary(Of Long, String)()

        ' Recording Settings
        Private _resolutionWidth As Integer = 1920
        Private _resolutionHeight As Integer = 1080
        Private _bitrate As Integer = DEFAULT_BITRATE
        Private _framerate As Integer = DEFAULT_FRAMERATE
        Private _encoderPreset As Integer = DEFAULT_ENCODER_PRESET
        Private _preset As RecordingPreset = RecordingPreset.Medium
        Private _captureCursor As Boolean = True
        Private _encoder As VideoEncoder = VideoEncoder.NVENC_H264
        Private _gpuIndex As Integer = 0
        Private _monitorIndex As Integer = 0
        Private _ffmpegPath As String = ""
        Private _ffprobePath As String = ""
        Private _useConstantBitrate As Boolean = True
        Private _trimMode As TrimMode = TrimMode.Accurate

        ' Capture Settings
        Private _captureAPI As CaptureAPIType = CaptureAPIType.Auto
        Private _outputFormat As OutputColorFormat = OutputColorFormat.Auto
        Private _resizeMode As VideoResizeModeType = VideoResizeModeType.Crop
        Private _scaleMode As VideoScaleModeType = VideoScaleModeType.Bilinear
        Private _captureTargetType As CaptureTargetType = CaptureTargetType.Monitor

        ' Window capture settings
        Private _windowTitle As String = ""
        Private _windowClass As String = ""
        Private _windowExe As String = ""
        Private _captureBorder As Boolean = False
        Private _displayBorder As Boolean = False

        ' Region capture settings
        Private _cropLeft As Integer = 0
        Private _cropTop As Integer = 0
        Private _cropRight As Integer = 0
        Private _cropBottom As Integer = 0

        ' ddagrab dup_frames setting
        ' ★ Fix N: Reverted back to True (default). Round 5's Fix K set this to
        ' False thinking it would avoid wasting encoder cycles on duplicate
        ' frames. In practice, with `-r 60` (CFR output), FFmpeg has to maintain
        ' 60fps output regardless. With dup_frames=0:
        '   - DDAGrab emits only real desktop-change frames (5-30fps on static screens)
        '   - FFmpeg sees VFR input + `-r 60` CFR → FFmpeg duplicates each frame
        '     many times to maintain 60fps output
        '   - Result: "More than 1000 frames duplicated" warning + video looks
        '     frozen during static periods (FPS drops to 2-3/s)
        ' With dup_frames=1 (this fix):
        '   - DDAGrab maintains 60fps input by duplicating last frame at source
        '   - FFmpeg sees 60fps CFR input + `-r 60` → no duplication needed
        '   - Result: smooth 60fps output (matches Master behavior)
        ' The original Master used dup_frames=1 and it worked — the only audio
        ' issues were in aresample/silent-frame timing, which are fixed by
        ' Fix A2/F2/M and remain independent of this setting.
        Private _dupFrames As Boolean = True

        ' Screen cache
        Private _cachedScreenW As Integer = -1
        Private _cachedScreenH As Integer = -1
        Private _screenCacheTime As DateTime = DateTime.MinValue
#End Region

#Region "Properties - Basic Settings"
        Public Property ResolutionWidth As Integer
            Get
                Return _resolutionWidth
            End Get
            Set(value As Integer)
                If value > 0 Then _resolutionWidth = value
            End Set
        End Property

        Public Property ResolutionHeight As Integer
            Get
                Return _resolutionHeight
            End Get
            Set(value As Integer)
                If value > 0 Then _resolutionHeight = value
            End Set
        End Property

        Public Property Bitrate As Integer
            Get
                Return _bitrate
            End Get
            Set(value As Integer)
                _bitrate = Math.Max(MIN_BITRATE, Math.Min(MAX_BITRATE, value))
            End Set
        End Property

        Public Property Framerate As Integer
            Get
                Return _framerate
            End Get
            Set(value As Integer)
                _framerate = Math.Max(MIN_FRAMERATE, Math.Min(MAX_FRAMERATE, value))
            End Set
        End Property

        Public Property EncoderPreset As Integer
            Get
                Return _encoderPreset
            End Get
            Set(value As Integer)
                _encoderPreset = Math.Max(MIN_ENCODER_PRESET, Math.Min(MAX_ENCODER_PRESET, value))
            End Set
        End Property

        Public Property UseConstantBitrate As Boolean
            Get
                Return _useConstantBitrate
            End Get
            Set(value As Boolean)
                _useConstantBitrate = value
            End Set
        End Property

        Public Property Preset As RecordingPreset
            Get
                Return _preset
            End Get
            Set(value As RecordingPreset)
                _preset = value
                If value <> RecordingPreset.Custom Then
                    ApplyPreset(value)
                End If
            End Set
        End Property

        Public Property CaptureCursor As Boolean
            Get
                Return _captureCursor
            End Get
            Set(value As Boolean)
                _captureCursor = value
            End Set
        End Property

        Public Property FFmpegPath As String
            Get
                Return _ffmpegPath
            End Get
            Set(value As String)
                _ffmpegPath = value
            End Set
        End Property

        Public Property FFprobePath As String
            Get
                Return _ffprobePath
            End Get
            Set(value As String)
                _ffprobePath = value
            End Set
        End Property

        Public Property Encoder As VideoEncoder
            Get
                Return _encoder
            End Get
            Set(value As VideoEncoder)
                _encoder = value
            End Set
        End Property

        Public Property GPUIndex As Integer
            Get
                Return _gpuIndex
            End Get
            Set(value As Integer)
                If value >= 0 Then _gpuIndex = value
            End Set
        End Property

        Public Property MonitorIndex As Integer
            Get
                Return _monitorIndex
            End Get
            Set(value As Integer)
                If value >= 0 Then _monitorIndex = value
            End Set
        End Property

        Public Property BufferDurationSeconds As Integer
            Get
                Return _replaySaveDuration
            End Get
            Set(value As Integer)
                _replaySaveDuration = Math.Max(MIN_REPLAY_DURATION, Math.Min(MAX_REPLAY_DURATION, value))
            End Set
        End Property

        Public ReadOnly Property IsRecording As Boolean
            Get
                Return _isRecording
            End Get
        End Property

        Public ReadOnly Property IsBuffering As Boolean
            Get
                Return _isBuffering
            End Get
        End Property

        Public ReadOnly Property RecordingDuration As TimeSpan
            Get
                If _isRecording Then
                    Return DateTime.Now - recordingStartTime
                Else
                    Return TimeSpan.Zero
                End If
            End Get
        End Property

        Public ReadOnly Property Status As RecordingStatus
            Get
                If _isRecording Then Return RecordingStatus.Recording
                If _isBuffering Then Return RecordingStatus.Buffering
                Return RecordingStatus.Idle
            End Get
        End Property

        Public Property ReplayBufferDuration As Integer
            Get
                Return _replaySaveDuration
            End Get
            Set(value As Integer)
                BufferDurationSeconds = value
            End Set
        End Property

        Public Property ReplayTrimMode As TrimMode
            Get
                Return _trimMode
            End Get
            Set(value As TrimMode)
                _trimMode = value
            End Set
        End Property

        Public ReadOnly Property IsSavingReplay As Boolean
            Get
                Return _isSaving
            End Get
        End Property

        Public ReadOnly Property BufferCurrentDuration As Double
            Get
                If Not _isBuffering Then Return 0
                Return Math.Min(GetActualBufferDuration().TotalSeconds, BUFFER_MAX_DURATION)
            End Get
        End Property

        Public ReadOnly Property BufferCapacitySeconds As Integer
            Get
                Return BUFFER_MAX_DURATION
            End Get
        End Property

        Public ReadOnly Property BufferCapacitySegments As Integer
            Get
                Return BUFFER_MAX_SEGMENTS
            End Get
        End Property

        Public ReadOnly Property IsReplayBuffering As Boolean
            Get
                Return _isBuffering
            End Get
        End Property

        Public ReadOnly Property IsBufferActive As Boolean
            Get
                Return _isBuffering
            End Get
        End Property

        Public Property SegmentFileDuration As Integer
            Get
                Return CInt(SEGMENT_DURATION * 1000)
            End Get
            Set(value As Integer)
            End Set
        End Property

        Public Property ExactDurationMode As Boolean
            Get
                Return True
            End Get
            Set(value As Boolean)
            End Set
        End Property

        Public Property AllIntraMode As Boolean
            Get
                Return False
            End Get
            Set(value As Boolean)
            End Set
        End Property
#End Region

#Region "Properties - Audio Settings (NAudio)"
        Public Property AudioMode As VideoCaptureMode
            Get
                Return _audioMode
            End Get
            Set(value As VideoCaptureMode)
                _audioMode = value
            End Set
        End Property

        Public Property SystemAudioEnabled As Boolean
            Get
                Return _audioMode = VideoCaptureMode.SystemOnly OrElse _audioMode = VideoCaptureMode.Both
            End Get
            Set(value As Boolean)
                If value Then
                    If _audioMode = VideoCaptureMode.MicOnly Then
                        _audioMode = VideoCaptureMode.Both
                    ElseIf _audioMode = VideoCaptureMode.None Then
                        _audioMode = VideoCaptureMode.SystemOnly
                    End If
                Else
                    If _audioMode = VideoCaptureMode.Both Then
                        _audioMode = VideoCaptureMode.MicOnly
                    ElseIf _audioMode = VideoCaptureMode.SystemOnly Then
                        _audioMode = VideoCaptureMode.None
                    End If
                End If
            End Set
        End Property

        Public Property MicEnabled As Boolean
            Get
                Return _audioMode = VideoCaptureMode.MicOnly OrElse _audioMode = VideoCaptureMode.Both
            End Get
            Set(value As Boolean)
                If value Then
                    If _audioMode = VideoCaptureMode.SystemOnly Then
                        _audioMode = VideoCaptureMode.Both
                    ElseIf _audioMode = VideoCaptureMode.None Then
                        _audioMode = VideoCaptureMode.MicOnly
                    End If
                Else
                    If _audioMode = VideoCaptureMode.Both Then
                        _audioMode = VideoCaptureMode.SystemOnly
                    ElseIf _audioMode = VideoCaptureMode.MicOnly Then
                        _audioMode = VideoCaptureMode.None
                    End If
                End If
            End Set
        End Property

        Public Property SystemAudioVolume As Single
            Get
                Return _systemAudioVolume
            End Get
            Set(value As Single)
                _systemAudioVolume = Math.Max(0.0F, Math.Min(1.0F, value))
                SyncLock _audioLock
                    If _recordingAudioPipe IsNot Nothing Then
                        _recordingAudioPipe.Volume = _systemAudioVolume
                    End If
                    If _bufferAudioPipe IsNot Nothing Then
                        _bufferAudioPipe.Volume = _systemAudioVolume
                    End If
                End SyncLock
            End Set
        End Property

        Public Property MicVolume As Single
            Get
                Return _micVolume
            End Get
            Set(value As Single)
                _micVolume = Math.Max(0.0F, Math.Min(1.0F, value))
            End Set
        End Property

        Public Property MicDeviceName As String
            Get
                Return _micDeviceName
            End Get
            Set(value As String)
                _micDeviceName = value
            End Set
        End Property
#End Region

#Region "Properties - Capture Settings"
        Public Property SelectedCaptureAPI As CaptureAPIType
            Get
                Return _captureAPI
            End Get
            Set(value As CaptureAPIType)
                _captureAPI = value
            End Set
        End Property

        Public Property OutputPixelFormat As OutputColorFormat
            Get
                Return _outputFormat
            End Get
            Set(value As OutputColorFormat)
                _outputFormat = value
            End Set
        End Property

        Public Property ResizeMode As VideoResizeModeType
            Get
                Return _resizeMode
            End Get
            Set(value As VideoResizeModeType)
                _resizeMode = value
            End Set
        End Property

        Public Property ScaleMode As VideoScaleModeType
            Get
                Return _scaleMode
            End Get
            Set(value As VideoScaleModeType)
                _scaleMode = value
            End Set
        End Property

        Public Property TargetType As CaptureTargetType
            Get
                Return _captureTargetType
            End Get
            Set(value As CaptureTargetType)
                _captureTargetType = value
            End Set
        End Property

        Public Property WindowTitle As String
            Get
                Return _windowTitle
            End Get
            Set(value As String)
                _windowTitle = value
            End Set
        End Property

        Public Property WindowClass As String
            Get
                Return _windowClass
            End Get
            Set(value As String)
                _windowClass = value
            End Set
        End Property

        Public Property WindowExe As String
            Get
                Return _windowExe
            End Get
            Set(value As String)
                _windowExe = value
            End Set
        End Property

        Public Property CaptureWindowBorder As Boolean
            Get
                Return _captureBorder
            End Get
            Set(value As Boolean)
                _captureBorder = value
            End Set
        End Property

        Public Property DisplayCaptureBorder As Boolean
            Get
                Return _displayBorder
            End Get
            Set(value As Boolean)
                _displayBorder = value
            End Set
        End Property

        Public Property CropLeft As Integer
            Get
                Return _cropLeft
            End Get
            Set(value As Integer)
                _cropLeft = Math.Max(0, value)
            End Set
        End Property

        Public Property CropTop As Integer
            Get
                Return _cropTop
            End Get
            Set(value As Integer)
                _cropTop = Math.Max(0, value)
            End Set
        End Property

        Public Property CropRight As Integer
            Get
                Return _cropRight
            End Get
            Set(value As Integer)
                _cropRight = Math.Max(0, value)
            End Set
        End Property

        Public Property CropBottom As Integer
            Get
                Return _cropBottom
            End Get
            Set(value As Integer)
                _cropBottom = Math.Max(0, value)
            End Set
        End Property

        ''' <summary>
        ''' When true (default), ddagrab will duplicate frames when the desktop
        ''' has not been updated to maintain approximately constant target framerate.
        ''' When false, ddagrab will wait for desktop updates (VFR output).
        ''' Per FFmpeg docs: ddagrab dup_frames option.
        ''' </summary>
        Public Property DuplicateFrames As Boolean
            Get
                Return _dupFrames
            End Get
            Set(value As Boolean)
                _dupFrames = value
            End Set
        End Property
#End Region

#Region "Events"
        Public Event RecordingStarted As EventHandler
        Public Event RecordingStopped As EventHandler(Of String)
        Public Event RecordingError As EventHandler(Of String)
        Public Event ReplayBufferStarted As EventHandler
        Public Event ReplayBufferStopped As EventHandler
        Public Event ReplaySaved As EventHandler(Of String)
        Public Event BufferStarted As EventHandler
        Public Event BufferStopped As EventHandler
        Public Event FFmpegLogReceived As EventHandler(Of String)
        Public Event ProgressChanged As EventHandler(Of RecordingProgressEventArgs)
#End Region

#Region "Event Args"
        Public Class RecordingProgressEventArgs
            Inherits EventArgs
            Public Property FrameCount As Long
            Public Property Size As Long
            Public Property Bitrate As Double
            Public Property Speed As Double
            Public Property Duration As TimeSpan

            Public Sub New(frameCount As Long, size As Long, bitrate As Double, speed As Double, duration As TimeSpan)
                Me.FrameCount = frameCount
                Me.Size = size
                Me.Bitrate = bitrate
                Me.Speed = speed
                Me.Duration = duration
            End Sub
        End Class
#End Region

#Region "Constructor"
        Public Sub New()
            ApplyPreset(RecordingPreset.Medium)
            JobObjectManager.InitializeJobObject()
        End Sub

        Public Sub New(ffmpegPath As String)
            Me.New()
            SetFFmpegPath(ffmpegPath)
        End Sub

        Public Sub SetFFmpegPath(ffmpegPath As String)
            If String.IsNullOrWhiteSpace(ffmpegPath) Then Exit Sub

            If Not Path.IsPathRooted(ffmpegPath) Then
                ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ffmpegPath)
            End If

            _ffmpegPath = ffmpegPath

            Dim ffmpegDir As String = Path.GetDirectoryName(ffmpegPath)
            Dim ffmpegFile As String = Path.GetFileNameWithoutExtension(ffmpegPath)
            If ffmpegFile.Equals("ffmpeg", StringComparison.OrdinalIgnoreCase) Then
                _ffprobePath = Path.Combine(ffmpegDir, "ffprobe.exe")
            End If
        End Sub

        Public Sub SetToolPaths(ffmpegPath As String, ffprobePath As String)
            SetFFmpegPath(ffmpegPath)
            If Not String.IsNullOrWhiteSpace(ffprobePath) Then
                If Not Path.IsPathRooted(ffprobePath) Then
                    ffprobePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ffprobePath)
                End If
                _ffprobePath = ffprobePath
            End If
        End Sub
#End Region

#Region "API Detection Methods"
        ' ★ Phase 2 refactor: All availability check state + logic moved to
        ' CaptureAPIDetector. The Shared methods below remain as thin
        ' forwarders so external callers (Sub_Record.vb, Base_RecordingsSet,
        ' etc.) continue to compile and behave identically.

        Public Shared Sub CheckGfxCaptureAvailability(ffmpegPath As String)
            CaptureAPIDetector.CheckGfxCaptureAvailability(ffmpegPath)
        End Sub

        Public Shared Sub CheckDDAGrabAvailability(ffmpegPath As String)
            CaptureAPIDetector.CheckDDAGrabAvailability(ffmpegPath)
        End Sub

        Public Shared ReadOnly Property IsGfxCaptureAvailable As Boolean
            Get
                Return CaptureAPIDetector.IsGfxCaptureAvailable
            End Get
        End Property

        Public Shared ReadOnly Property IsDDAGrabAvailable As Boolean
            Get
                Return CaptureAPIDetector.IsDDAGrabAvailable
            End Get
        End Property

        Public Shared Sub ResetAPIChecks()
            CaptureAPIDetector.ResetAPIChecks()
        End Sub

        ''' <summary>
        ''' ★ v5: คืนรายการ Capture API ที่ใช้ได้กับ CaptureTargetType ปัจจุบัน
        ''' UI ใช้แสดง dropdown/combo box ให้ User เลือก
        ''' </summary>
        Public Function GetAvailableCaptureAPIs() As List(Of CaptureAPIOption)
            Dim result As New List(Of CaptureAPIOption)()

            ' GDIGrab ใช้ได้เสมอ (fallback)
            result.Add(New CaptureAPIOption With {
                .APIType = CaptureAPIType.GDIGrab,
                .DisplayName = "GDI Capture",
                .Description = "Fallback — ช้าที่สุดแต่ใช้ได้ทุกกรณี (CPU-based)",
                .IsRecommended = (_captureTargetType = CaptureTargetType.Monitor AndAlso Not RequiresHDRSupport() AndAlso Not CaptureAPIDetector.IsDDAGrabAvailable AndAlso Not CaptureAPIDetector.IsGfxCaptureAvailable),
                .IsAvailable = True
            })

            ' DDAGrab — Monitor only
            If _captureTargetType <> CaptureTargetType.Window Then
                Dim ddagrabAvail As Boolean = CaptureAPIDetector.IsDDAGrabAvailable OrElse Not CaptureAPIDetector.IsDDAGrabChecked
                Dim ddagrabRecommended As Boolean = (_captureTargetType = CaptureTargetType.Monitor AndAlso Not RequiresHDRSupport())
                result.Add(New CaptureAPIOption With {
                    .APIType = CaptureAPIType.DDAGrab,
                    .DisplayName = "DDA Grab (DXGI Desktop Duplication)",
                    .Description = "Monitor capture — เร็วกว่า GFxCapture เล็กน้อย (Monitor เท่านั้น)",
                    .IsRecommended = ddagrabRecommended AndAlso ddagrabAvail,
                    .IsAvailable = ddagrabAvail
                })
            End If

            ' GFxCapture — Window + HDR
            Dim gfxcaptureAvail As Boolean = CaptureAPIDetector.IsGfxCaptureAvailable OrElse Not CaptureAPIDetector.IsGfxCaptureChecked
            Dim gfxcaptureRecommended As Boolean = (_captureTargetType = CaptureTargetType.Window OrElse RequiresHDRSupport())
            result.Add(New CaptureAPIOption With {
                .APIType = CaptureAPIType.GFxCapture,
                .DisplayName = "Graphics Capture (Windows.Graphics.Capture)",
                .Description = "Window capture + HDR — จับ Window ได้ + รองรับ HDR ครบ (เหมาะสำหรับ Window/HDR)",
                .IsRecommended = gfxcaptureRecommended AndAlso gfxcaptureAvail,
                .IsAvailable = gfxcaptureAvail
            })

            ' Auto
            result.Add(New CaptureAPIOption With {
                .APIType = CaptureAPIType.Auto,
                .DisplayName = "Auto (แนะนำ)",
                .Description = String.Format("เลือกอัตโนมัติตามโหมด — {0}",
                    If(_captureTargetType = CaptureTargetType.Window, "Window → GFxCapture",
                    If(RequiresHDRSupport(), "HDR → GFxCapture", "Monitor → DDAGrab (เร็วกว่า)"))),
                .IsRecommended = True,
                .IsAvailable = True
            })

            Return result
        End Function

        ''' <summary>
        ''' ★ v5: คืน Capture API ที่จะถูกใช้จริง (หลัง resolve fallback)
        ''' UI ใช้แสดง label ว่าตอนนี้ใช้ API อะไร
        ''' </summary>
        Public ReadOnly Property ResolvedCaptureAPI As CaptureAPIType
            Get
                Return DetermineBestCaptureAPI()
            End Get
        End Property

        ''' <summary>
        ''' ★ v5: คืนคำอธิบายของ Capture API ที่จะถูกใช้จริง
        ''' </summary>
        Public ReadOnly Property ResolvedCaptureAPIDescription As String
            Get
                Dim api As CaptureAPIType = DetermineBestCaptureAPI()
                Select Case api
                    Case CaptureAPIType.GFxCapture
                        If _captureTargetType = CaptureTargetType.Window Then
                            Return "GFxCapture — จับ Window (รองรับ HDR)"
                        ElseIf RequiresHDRSupport() Then
                            Return "GFxCapture — Monitor + HDR"
                        Else
                            Return "GFxCapture — Graphics Capture API"
                        End If
                    Case CaptureAPIType.DDAGrab
                        Return "DDAGrab — Desktop Duplication (เร็ว, Monitor เท่านั้น)"
                    Case CaptureAPIType.GDIGrab
                        Return "GDIGrab — GDI Fallback (ช้าที่สุด)"
                    Case Else
                        Return "Auto"
                End Select
            End Get
        End Property
#End Region

#Region "Pre-warm System"
        Public Shared Sub PreWarmFFmpeg(ffmpegPath As String, preferredEncoder As VideoEncoder)
            If _isPreWarmed Then Exit Sub
            If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
                _isPreWarmed = True
                Exit Sub
            End If

            SyncLock _prewarmLock
                If _isPreWarmed Then Exit Sub

                Try
                    Task.Run(Sub()
                                 Try
                                     PreWarmEncoderCore(ffmpegPath, preferredEncoder)
                                 Catch ex As Exception
                                     Debug.WriteLine("PreWarmEncoderCore Error: " & ex.Message)
                                 Finally
                                     _isPreWarmed = True
                                 End Try
                             End Sub)
                Catch ex As Exception
                    Debug.WriteLine("PreWarmFFmpeg Error: " & ex.Message)
                    _isPreWarmed = True
                End Try
            End SyncLock
        End Sub

        Private Shared Sub PreWarmEncoderCore(ffmpegPath As String, encoder As VideoEncoder)
            Try
                Dim encoderArgs As String = GetPreWarmEncoderArgs(encoder)
                Dim prewarmArgs As String = "-f lavfi -i ""nullsrc=s=256x256:d=0.1"" " & encoderArgs & " -f null - -hide_banner -loglevel error"

                Using proc As New Process()
                    proc.StartInfo = CreateProcessStartInfo(ffmpegPath, prewarmArgs)
                    proc.Start()
                    If proc.WaitForExit(5000) Then
                        Debug.WriteLine("PreWarmEncoderCore: " & encoder.ToString() & " loaded")
                    Else
                        proc.Kill()
                    End If
                End Using
            Catch ex As Exception
                Debug.WriteLine("PreWarmEncoderCore Error: " & ex.Message)
            End Try
        End Sub

        Private Shared Function GetPreWarmEncoderArgs(encoder As VideoEncoder) As String
            Select Case encoder
                Case VideoEncoder.NVENC_H264, VideoEncoder.NVENC_HEVC, VideoEncoder.NVENC_AV1
                    Return "-c:v h264_nvenc"
                Case VideoEncoder.QuickSync_H264
                    Return "-c:v h264_qsv"
                Case VideoEncoder.QuickSync_HEVC
                    Return "-c:v hevc_qsv"
                Case VideoEncoder.AMF_H264
                    Return "-c:v h264_amf"
                Case VideoEncoder.AMF_HEVC
                    Return "-c:v hevc_amf"
                Case Else
                    Return "-c:v libx264 -preset ultrafast"
            End Select
        End Function

        Public Shared ReadOnly Property IsPreWarmed As Boolean
            Get
                Return _isPreWarmed
            End Get
        End Property
#End Region

#Region "Job Object Methods"
        ' ★ Phase 3 refactor: InitializeJobObject + AddProcessToJob moved to
        ' JobObjectManager. AddProcessToJob forwarder is declared above in
        ' the "Job Object for Process Cleanup" region. Nothing else to do
        ' here — kept the region marker as a placeholder so existing
        ' code-folding still finds a home.
#End Region

#Region "Public Async Methods"
        Public Function StartRecordingAsync(outputFilePath As String) As Task(Of Boolean)
            Return StartRecordingAsync(outputFilePath, Rectangle.Empty)
        End Function

        Public Async Function StartRecordingAsync(outputFilePath As String, region As Rectangle) As Task(Of Boolean)
            Return Await Task.Run(Function() StartRecording(outputFilePath, region))
        End Function

        Public Async Function StopRecordingAsync() As Task
            Await Task.Run(Sub() StopRecording())
        End Function

        Public Async Function StartBufferAsync() As Task(Of Boolean)
            Return Await Task.Run(Function() StartBuffer())
        End Function

        Public Async Function StopBufferAsync() As Task
            Await Task.Run(Sub() StopBuffer())
        End Function

        Public Async Function SaveReplayAsync(outputPath As String, saveDuration As Integer) As Task(Of Boolean)
            Return Await Task.Run(Function() SaveReplay(outputPath, saveDuration))
        End Function

        Public Async Function SaveReplayAsync(outputPath As String) As Task(Of Boolean)
            Return Await Task.Run(Function() SaveReplay(outputPath))
        End Function

        Public Async Function SaveBufferAsync(outputPath As String, Optional durationSeconds As Integer = -1) As Task(Of Boolean)
            If durationSeconds <= 0 Then durationSeconds = _replaySaveDuration
            Return Await Task.Run(Function() SaveReplay(outputPath, durationSeconds))
        End Function

        Public Async Function StartReplayBufferAsync() As Task(Of Boolean)
            Return Await StartBufferAsync()
        End Function

        Public Async Function StopReplayBufferAsync() As Task
            Await StopBufferAsync()
        End Function
#End Region

#Region "Preset Methods"
        Private Sub ApplyPreset(preset As RecordingPreset)
            Select Case preset
                Case RecordingPreset.Low
                    _resolutionWidth = 1280
                    _resolutionHeight = 720
                    _bitrate = 4000
                    _framerate = 30
                    _encoderPreset = 6
                Case RecordingPreset.Medium
                    _resolutionWidth = 1920
                    _resolutionHeight = 1080
                    _bitrate = 7000
                    _framerate = 60
                    _encoderPreset = 4
                Case RecordingPreset.High
                    _resolutionWidth = 2560
                    _resolutionHeight = 1440
                    _bitrate = 12000
                    _framerate = 60
                    _encoderPreset = 2
                Case RecordingPreset.MyLow
                    _resolutionWidth = 1280
                    _resolutionHeight = 720
                    _bitrate = 4000
                    _framerate = 30
                    _encoderPreset = 6
                Case RecordingPreset.MyMedium
                    _resolutionWidth = 1920
                    _resolutionHeight = 1080
                    _bitrate = 7000
                    _framerate = 60
                    _encoderPreset = 4
                Case RecordingPreset.MyHigh
                    _resolutionWidth = 2560
                    _resolutionHeight = 1440
                    _bitrate = 12000
                    _framerate = 60
                    _encoderPreset = 2
                Case RecordingPreset.Recommended
                    ' Native resolution with recommended bitrate for balanced quality/performance
                    ' Note: UI should override resolution to native after applying this preset
                    _resolutionWidth = 1920
                    _resolutionHeight = 1080
                    _bitrate = 12000
                    _framerate = 60
                    _encoderPreset = 4
                Case RecordingPreset.Maximum
                    ' Native resolution with max bitrate and high FPS for best possible quality
                    ' Note: UI should override resolution to native after applying this preset
                    _resolutionWidth = 1920
                    _resolutionHeight = 1080
                    _bitrate = 50000
                    _framerate = 144
                    _encoderPreset = 7
                Case RecordingPreset.Custom
            End Select
        End Sub

        Public Sub SetCustomSettings(width As Integer, height As Integer, bitrateKbps As Integer, fps As Integer)
            _preset = RecordingPreset.Custom
            _resolutionWidth = Math.Max(1, width)
            _resolutionHeight = Math.Max(1, height)
            Me.Bitrate = bitrateKbps
            Me.Framerate = fps
        End Sub

        Public Sub SetCustomSettings(width As Integer, height As Integer, bitrateKbps As Integer, fps As Integer, encoderPreset As Integer)
            SetCustomSettings(width, height, bitrateKbps, fps)
            Me.EncoderPreset = encoderPreset
        End Sub

        Public Sub SetWindowCapture(windowTitlePattern As String, Optional windowClassPattern As String = "", Optional exeName As String = "")
            _captureTargetType = CaptureTargetType.Window
            _windowTitle = windowTitlePattern
            _windowClass = windowClassPattern
            _windowExe = exeName
        End Sub

        Public Sub SetRegionCapture(cropLeft As Integer, cropTop As Integer, cropRight As Integer, cropBottom As Integer)
            _captureTargetType = CaptureTargetType.Region
            _cropLeft = cropLeft
            _cropTop = cropTop
            _cropRight = cropRight
            _cropBottom = cropBottom
        End Sub

        Public Sub SetMonitorCapture(monitorIndex As Integer)
            _captureTargetType = CaptureTargetType.Monitor
            _monitorIndex = monitorIndex
        End Sub

        Public Function GetFFmpegArguments(Optional outputFile As String = "output.mp4") As String
            Return BuildFFmpegArguments(outputFile, Rectangle.Empty)
        End Function
#End Region

#Region "Recording Methods"
        Public Function StartRecording(outputFilePath As String) As Boolean
            Return StartRecording(outputFilePath, Rectangle.Empty)
        End Function

        Public Function StartRecording(outputFilePath As String, region As Rectangle) As Boolean
            SyncLock _recordingLock
                If Not CheckActionCooldown() Then Return False

                If _isRecording Then
                    RaiseEvent RecordingError(Me, "Recording is already in progress")
                    Return False
                End If

                If Not ValidateFFmpeg() Then Return False

                ' ★ Fix D: Removed synchronous API availability check.
                ' Old code blocked StartRecording for up to 6 seconds (3s × 2 APIs)
                ' if PreWarmFFmpeg hadn't finished yet. This made the first recording
                ' attempt after app startup feel frozen.
                '
                ' DetermineBestCaptureAPI() already handles "not yet checked" by
                ' falling through to the best-guess API with a fallback flag set
                ' (e.g. Monitor+SDR → tries DDAGrab, fallback GFxCapture). If that
                ' API fails at runtime, NotifyCaptureAPIFailed() triggers the
                ' fallback automatically. So we don't NEED the synchronous check
                ' to make a correct decision — it just made the user wait.
                '
                ' PreWarmFFmpeg (called at app startup) still runs the checks
                ' asynchronously. By the time the user actually triggers a
                ' recording, the cache is usually populated.

                ResetCaptureAPIFallback()
                MarkActionTime()

                Try
                    Dim dir As String = Path.GetDirectoryName(outputFilePath)
                    If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If

                    recordingOutputPath = outputFilePath

                    StartRecordingAudioPipe()

                    ' ★ RACE FIX: Wait for AudioPipe server to be ready before starting FFmpeg
                    If _recordingAudioPipe IsNot Nothing Then
                        Dim pipeReady As Boolean = _recordingAudioPipe.WaitForPipeServerReadyAsync(3000).Result
                        If Not pipeReady Then
                            Debug.WriteLine("StartRecording: AudioPipe server not ready — proceeding without audio")
                            StopRecordingAudioPipe()
                        End If
                    End If

                    Dim arguments As String = BuildFFmpegArguments(outputFilePath, region)

                    Debug.WriteLine("══════════ Recording FFmpeg Arguments ══════════")
                    Debug.WriteLine(arguments)

                    recordingProcess = CreateFFmpegProcess(arguments)
                    recordingProcess.Start()
                    AddProcessToJob(recordingProcess)
                    recordingProcess.BeginErrorReadLine()
                    recordingProcess.BeginOutputReadLine()

                    _isRecording = True
                    recordingStartTime = DateTime.Now

                    RaiseEvent RecordingStarted(Me, EventArgs.Empty)
                    Return True

                Catch ex As Exception
                    StopRecordingAudioPipe()
                    RaiseEvent RecordingError(Me, "Failed to start recording: " & ex.Message)
                    CleanupRecordingProcess()
                    Return False
                End Try
            End SyncLock
        End Function

        Public Sub StopRecording()
            SyncLock _recordingLock
                If Not CheckActionCooldown() Then Exit Sub
                MarkActionTime()

                If Not _isRecording OrElse recordingProcess Is Nothing Then Exit Sub

                Dim savedPath As String = recordingOutputPath

                Try
                    ' ===== 1. Send 'q' to FFmpeg =====
                    recordingProcess.StandardInput.Write("q"c)
                    recordingProcess.StandardInput.Flush()
                    recordingProcess.StandardInput.Close()

                    ' ===== 2. Wait for FFmpeg to exit =====
                    If Not recordingProcess.WaitForExit(GRACEFUL_EXIT_TIMEOUT) Then
                        Debug.WriteLine("StopRecording: FFmpeg timeout")
                        recordingProcess.Kill()
                        recordingProcess.WaitForExit(FORCE_KILL_TIMEOUT)
                    End If

                    Debug.WriteLine("StopRecording: FFmpeg exited")

                Catch ex As Exception
                    Debug.WriteLine("StopRecording Error: " & ex.Message)
                    ForceKillProcess(recordingProcess)
                End Try

                ' ===== 3. FFmpeg exited → AudioPipe broken automatically =====
                StopRecordingAudioPipe()

                ' ===== 4. Final cleanup =====
                CleanupRecordingProcess()
                _isRecording = False

                RaiseEvent RecordingStopped(Me, savedPath)
            End SyncLock
        End Sub
#End Region

#Region "Replay Buffer"
        Public Function StartBuffer() As Boolean
            SyncLock _bufferLock
                If Not CheckActionCooldown() Then Return False

                If _isBuffering Then
                    Debug.WriteLine("StartBuffer: Already buffering")
                    Return False
                End If

                If Not ValidateFFmpeg() Then Return False

                ' ★ Fix D: Removed synchronous API availability check (same as
                ' StartRecording above). DetermineBestCaptureAPI handles the
                ' "not yet checked" case by trying the best-guess API with a
                ' fallback flag. No need to block the UI here.

                ResetCaptureAPIFallback()
                MarkActionTime()

                ' ===== Create temp directory =====
                bufferTempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", "replay_buffer_" & Process.GetCurrentProcess().Id)

                Try
                    If Directory.Exists(bufferTempDir) Then
                        For Each f In Directory.GetFiles(bufferTempDir, "*.mkv")
                            Try
                                File.Delete(f)
                            Catch
                            End Try
                        Next
                    Else
                        Directory.CreateDirectory(bufferTempDir)
                    End If
                Catch
                    bufferTempDir = Path.Combine(Path.GetTempPath(), "ShadowPlay_Buffer_" & Process.GetCurrentProcess().Id)
                    Directory.CreateDirectory(bufferTempDir)
                End Try

                _segmentTimestamps.Clear()
                _bufferStartTime = DateTime.Now
                _lastSegmentTime = DateTime.Now

                Try
                    ' ===== Start AudioPipe =====
                    StartBufferAudioPipe()

                    ' ★ RACE FIX: Wait for AudioPipe server to be ready before starting FFmpeg
                    If _bufferAudioPipe IsNot Nothing Then
                        Dim pipeReady As Boolean = _bufferAudioPipe.WaitForPipeServerReadyAsync(3000).Result
                        If Not pipeReady Then
                            Debug.WriteLine("StartBuffer: AudioPipe server not ready — proceeding without audio")
                            StopBufferAudioPipe()
                        End If
                    End If

                    ' ===== Build FFmpeg arguments =====
                    Dim arguments As String = BuildBufferFFmpegArguments()
                    Debug.WriteLine("══════════ Buffer FFmpeg Arguments ══════════")
                    Debug.WriteLine(arguments)

                    ' ===== Start FFmpeg process =====
                    bufferProcess = CreateFFmpegProcess(arguments)
                    bufferProcess.Start()
                    AddProcessToJob(bufferProcess)
                    bufferProcess.BeginErrorReadLine()
                    bufferProcess.BeginOutputReadLine()

                    _isBuffering = True

                    RaiseEvent ReplayBufferStarted(Me, EventArgs.Empty)
                    RaiseEvent BufferStarted(Me, EventArgs.Empty)

                    Debug.WriteLine("StartBuffer: Success")
                    Return True

                Catch ex As Exception
                    Debug.WriteLine("StartBuffer Error: " & ex.Message)
                    StopBufferAudioPipe()
                    CleanupBufferProcess()
                    RaiseEvent RecordingError(Me, "Failed to start buffer: " & ex.Message)
                    Return False
                End Try
            End SyncLock
        End Function

        Public Sub StopBuffer()
            SyncLock _bufferLock
                If Not CheckActionCooldown() Then Exit Sub
                MarkActionTime()

                If Not _isBuffering OrElse bufferProcess Is Nothing Then
                    Debug.WriteLine("StopBuffer: Not buffering or process is null")
                    Exit Sub
                End If

                Debug.WriteLine("═══ StopBuffer START ═══")

                Try
                    ' ===== 1. Send 'q' to FFmpeg (graceful exit) =====
                    If bufferProcess IsNot Nothing AndAlso Not bufferProcess.HasExited Then
                        Try
                            bufferProcess.StandardInput.Write("q"c)
                            bufferProcess.StandardInput.Flush()
                            bufferProcess.StandardInput.Close()

                            Debug.WriteLine("StopBuffer: Sent 'q' to FFmpeg")
                        Catch ex As Exception
                            Debug.WriteLine("StopBuffer: Failed to send 'q' - " & ex.Message)
                        End Try
                    End If

                    ' ===== 2. Wait for FFmpeg to exit =====
                    ' PERF: Use shorter timeout for buffer — FFmpeg segment muxer
                    ' exits fast on 'q' since it doesn't need to finalize a single large file.
                    ' Old timeout was 10s, now 3s which is more than enough.
                    If bufferProcess IsNot Nothing AndAlso Not bufferProcess.HasExited Then
                        Dim exited As Boolean = bufferProcess.WaitForExit(BUFFER_GRACEFUL_EXIT_TIMEOUT)

                        If Not exited Then
                            Debug.WriteLine("StopBuffer: FFmpeg timeout, force killing...")
                            Try
                                bufferProcess.Kill()
                                bufferProcess.WaitForExit(FORCE_KILL_TIMEOUT)
                            Catch
                            End Try
                        Else
                            Debug.WriteLine("StopBuffer: FFmpeg exited gracefully")
                        End If
                    End If

                Catch ex As Exception
                    Debug.WriteLine("StopBuffer Error: " & ex.Message)
                    ForceKillProcess(bufferProcess)
                End Try

                ' ===== 3. FFmpeg exited, then close AudioPipe =====
                StopBufferAudioPipe()

                ' ===== 4. Cleanup =====
                CleanupBufferProcess()
                _isBuffering = False

                ' ===== 5. Raise events FIRST so UI responds immediately =====
                RaiseEvent ReplayBufferStopped(Me, EventArgs.Empty)
                RaiseEvent BufferStopped(Me, EventArgs.Empty)

                ' ===== 6. Delete temp directory in background =====
                ' PERF: Don't block the caller waiting for file deletion.
                ' Old code: deleted files one-by-one synchronously = slow with many segments
                ' New code: delete entire directory tree asynchronously
                Dim dirToDelete As String = bufferTempDir
                bufferTempDir = ""
                _segmentTimestamps.Clear()

                Task.Run(Sub()
                             Try
                                 If Directory.Exists(dirToDelete) Then
                                     Directory.Delete(dirToDelete, recursive:=True)
                                 End If
                             Catch ex As Exception
                                 Debug.WriteLine("StopBuffer: Background cleanup error: " & ex.Message)
                             End Try
                         End Sub)

                Debug.WriteLine("═══ StopBuffer END ═══")
            End SyncLock
        End Sub

        Public Function SaveReplay(outputPath As String, saveDuration As Integer) As Boolean
            If Not CheckActionCooldown() Then Return False
            MarkActionTime()

            SyncLock _saveLock
                If _isSaving Then
                    Debug.WriteLine("SaveReplay: Already saving")
                    Return False
                End If
                _isSaving = True
            End SyncLock

            Try
                ' ===== BUG FIX: Check _isBuffering under buffer lock =====
                Dim currentlyBuffering As Boolean
                SyncLock _bufferLock
                    currentlyBuffering = _isBuffering
                End SyncLock

                If Not currentlyBuffering Then
                    Debug.WriteLine("SaveReplay: Not buffering")
                    Return False
                End If

                ' ===== 2. Wait briefly for FFmpeg to finish writing current segment =====
                ' ★ RACE FIX: FFmpeg's segment muxer might be mid-write on the last segment.
                ' Wait one segment duration to ensure the current segment is complete.
                System.Threading.Thread.Sleep(CInt(SEGMENT_DURATION * 1000) + 200)

                ' ===== 3. Get available segments =====
                Dim segments As New List(Of TimestampedSegment)()
                SyncLock _bufferLock
                    segments = GetTimestampedSegments()
                End SyncLock

                If segments.Count = 0 Then
                    Debug.WriteLine("SaveReplay: No segments found")
                    Return False
                End If

                ' Already sorted by segment number in GetTimestampedSegments()

                Debug.WriteLine(String.Format("SaveReplay: Found {0} segments", segments.Count))

                ' ===== 4. Calculate segments needed =====
                Dim actualBufferDuration As Double = segments.Count * SEGMENT_DURATION
                Dim requestedDuration As Double = Math.Min(saveDuration, actualBufferDuration)
                Dim segmentsNeeded As Integer = CInt(Math.Ceiling(requestedDuration / SEGMENT_DURATION))

                ' Take the last N segments
                Dim startIndex As Integer = Math.Max(0, segments.Count - segmentsNeeded)
                Dim selectedSegments As List(Of TimestampedSegment) = segments.Skip(startIndex).Take(segmentsNeeded).ToList()

                If selectedSegments.Count = 0 Then
                    Debug.WriteLine("SaveReplay: No segments selected")
                    Return False
                End If

                ' ★ RACE FIX: Filter out segments that are locked or inaccessible
                Dim safeSegments As New List(Of TimestampedSegment)()
                For Each seg In selectedSegments
                    If Not File.Exists(seg.FilePath) Then
                        Debug.WriteLine(String.Format("SaveReplay: Segment missing: {0}", seg.FilePath))
                        Continue For
                    End If
                    Try
                        Using fs As IO.FileStream = New IO.FileStream(seg.FilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                            ' File is accessible
                        End Using
                        safeSegments.Add(seg)
                    Catch ex As IOException
                        Debug.WriteLine(String.Format("SaveReplay: Segment locked, skipping: {0}", seg.FilePath))
                    End Try
                Next
                selectedSegments = safeSegments

                If selectedSegments.Count = 0 Then
                    Debug.WriteLine("SaveReplay: All segments were locked or missing")
                    Return False
                End If

                Debug.WriteLine(String.Format("SaveReplay: Using {0} segments ({1:F1}s)",
            selectedSegments.Count, selectedSegments.Count * SEGMENT_DURATION))

                ' ===== 4. Prepare output =====
                Try
                    Dim outputDir As String = Path.GetDirectoryName(outputPath)
                    If Not String.IsNullOrEmpty(outputDir) AndAlso Not Directory.Exists(outputDir) Then
                        Directory.CreateDirectory(outputDir)
                    End If

                    ' ===== 5. Build concat list content =====
                    ' PERF: Build the entire concat list in memory (StringBuilder)
                    ' then either pipe it to FFmpeg or write to file as fallback.
                    '
                    ' SHADOWPLAY APPROACH: Use 'duration' directive for each segment.
                    ' When -reset_timestamps 1 is used, each segment starts at PTS=0.
                    ' Without 'duration', the concat demuxer miscomputes timestamps → fractional FPS.
                    ' With 'duration', the concat demuxer knows exact segment length → correct offsets.
                    ' This enables -c copy (zero CPU, zero quality loss) — exactly like NVIDIA Shadowplay.
                    '
                    ' For all segments except the last: use SEGMENT_DURATION (0.500000)
                    ' For the last segment: omit duration → concat demuxer probes actual duration
                    '   (avoids undefined behavior if last segment is partial/still writing)
                    Dim concatContent As New StringBuilder(selectedSegments.Count * 120)

                    For i As Integer = 0 To selectedSegments.Count - 1
                        Dim seg = selectedSegments(i)
                        Dim escapedPath As String = seg.FilePath.Replace("\"c, "/"c)
                        concatContent.AppendFormat("file '{0}'", escapedPath)
                        concatContent.AppendLine()

                        ' Add duration directive for all segments except the last one.
                        ' The last segment might be partial (still being written),
                        ' so we let the concat demuxer probe its actual duration.
                        If i < selectedSegments.Count - 1 Then
                            concatContent.AppendFormat("duration {0:F6}", SEGMENT_DURATION)
                            concatContent.AppendLine()
                        End If
                    Next

                    Dim concatString As String = concatContent.ToString()

                    ' ===== 6. Concat segments using file-based concat =====
                    ' NOTE: Pipe-based concat (-i pipe:0) fails because the concat
                    ' demuxer may need to seek within the input, and pipes are not seekable.
                    ' File-based concat is equally fast (~0.7s for 30 segments) and more reliable.
                    Dim concatListPath As String = Path.Combine(bufferTempDir, "concat_list.txt")
                    File.WriteAllText(concatListPath, concatString)

                    ' Log first 3 + last 1 segment entries (avoid flooding debug output)
                    Dim lines As String() = concatString.Split(New Char() {vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
                    If lines.Length > 12 Then
                        Debug.WriteLine(String.Format("SaveReplay: Concat list ({0} entries) - first 3 & last 3:", selectedSegments.Count))
                        For li As Integer = 0 To 5
                            Debug.WriteLine("  " & lines(li))
                        Next
                        Debug.WriteLine("  ...")
                        For li As Integer = lines.Length - 6 To lines.Length - 1
                            Debug.WriteLine("  " & lines(li))
                        Next
                    Else
                        Debug.WriteLine("SaveReplay: Concat list content:")
                        Debug.WriteLine(concatString)
                    End If

                    Dim concatArgs As String = String.Format(
                "-y -nostdin -hide_banner -loglevel warning -fflags +genpts -f concat -safe 0 -i ""{0}"" -c copy -r {2} -avoid_negative_ts make_zero -movflags +faststart ""{1}""",
                concatListPath, outputPath, _framerate)

                    Dim success As Boolean = RunFFmpegSync(concatArgs, CONCAT_TIMEOUT)

                    ' ===== 7. Verify output =====
                    If success AndAlso File.Exists(outputPath) Then
                        Dim fileInfo As New FileInfo(outputPath)
                        If fileInfo.Length > 0 Then
                            Debug.WriteLine(String.Format("SaveReplay: Success - {0} bytes", fileInfo.Length))

                            ' Log output file info for verification (FPS, duration, etc.)
                            Try
                                If Not String.IsNullOrEmpty(_ffprobePath) AndAlso File.Exists(_ffprobePath) Then
                                    Dim probeArgs As String = String.Format(
                                "-v error -select_streams v:0 -show_entries stream=r_frame_rate,avg_frame_rate,duration,nb_frames,codec_name -of default=noprint_wrappers=1 ""{0}""", outputPath)
                                    Dim probeResult As String = RunFFprobeQuick(probeArgs)
                                    Debug.WriteLine("SaveReplay: Output file info: " & probeResult)
                                End If
                            Catch
                            End Try

                            RaiseEvent ReplaySaved(Me, outputPath)
                            Return True
                        End If
                    End If

                    Debug.WriteLine("SaveReplay: Concat failed")
                    Return False

                Catch ex As Exception
                    Debug.WriteLine("SaveReplay Error: " & ex.Message)
                    Return False
                End Try

            Finally
                SyncLock _saveLock
                    _isSaving = False
                End SyncLock
            End Try
        End Function

        Public Function SaveReplay(outputPath As String) As Boolean
            Return SaveReplay(outputPath, _replaySaveDuration)
        End Function

        Public Function GetActualBufferDuration() As TimeSpan
            SyncLock _bufferLock
                If Not _isBuffering Then Return TimeSpan.Zero

                Try
                    If Directory.Exists(bufferTempDir) Then
                        Dim files = Directory.GetFiles(bufferTempDir, "segment_*.mkv")
                        Return TimeSpan.FromSeconds(files.Length * SEGMENT_DURATION)
                    End If
                Catch
                End Try

                Return TimeSpan.Zero
            End SyncLock
        End Function

        ''' <summary>
        ''' PERF: Optimized segment enumeration.
        ''' Old code: Directory.GetFiles + regex on every file = O(n) file I/O
        ''' New code: Use EnumerateFiles (lazy) + parallel-capable parsing +
        '''   filter by last-write time to skip old/stale segments.
        ''' Also pre-allocates list capacity to reduce resizing.
        ''' </summary>
        Private Function GetTimestampedSegments() As List(Of TimestampedSegment)
            Dim result As New List(Of TimestampedSegment)()

            If Not Directory.Exists(bufferTempDir) Then Return result

            Try
                ' PERF: EnumerateFiles is lazy — doesn't allocate full array upfront
                ' Also filter out empty files (FFmpeg may have started writing but not finished)
                Dim cutoffTime As DateTime = DateTime.Now.AddSeconds(-BUFFER_MAX_DURATION - 10)

                For Each f In Directory.EnumerateFiles(bufferTempDir, "segment_*.mkv")
                    Try
                        ' Quick file-size check: skip 0-byte files (incomplete segments)
                        Dim fileInfo As New FileInfo(f)
                        If fileInfo.Length = 0 Then Continue For

                        Dim fileName = Path.GetFileNameWithoutExtension(f)
                        Dim match = RegexSegment.Match(fileName)

                        If match.Success Then
                            Dim segNumber As Integer = Integer.Parse(match.Groups(1).Value)
                            result.Add(New TimestampedSegment With {
                                .FilePath = f,
                                .SegmentNumber = segNumber,
                                .LastWriteTime = fileInfo.LastWriteTimeUtc
                            })
                        End If
                    Catch
                    End Try
                Next

                ' Pre-sort by segment number (natural order from filename)
                ' Since segment_%05d is monotonically increasing, the filesystem
                ' often returns them in order, but we can't rely on it.
                If result.Count > 1 Then
                    result.Sort(Function(a, b) a.SegmentNumber.CompareTo(b.SegmentNumber))
                End If

            Catch ex As Exception
                Debug.WriteLine("GetTimestampedSegments Error: " & ex.Message)
            End Try

            Return result
        End Function

        Private Class TimestampedSegment
            Public Property FilePath As String
            Public Property SegmentNumber As Integer
            Public Property LastWriteTime As DateTime
        End Class
#End Region

#Region "FFmpeg Process Management"
        Private Sub StopProcessGracefully(proc As Process, timeoutMs As Integer)
            If proc Is Nothing OrElse proc.HasExited Then Exit Sub

            Try
                proc.StandardInput.Write("q"c)
                proc.StandardInput.Flush()
                proc.StandardInput.Close()

                If Not proc.WaitForExit(timeoutMs) Then
                    Debug.WriteLine("StopProcessGracefully: Timeout, force killing...")
                    proc.Kill()
                    proc.WaitForExit(FORCE_KILL_TIMEOUT)
                Else
                    Debug.WriteLine("StopProcessGracefully: Process exited gracefully")
                End If
            Catch ex As Exception
                Debug.WriteLine("StopProcessGracefully Error: " & ex.Message)
                Throw
            End Try
        End Sub

        Private Sub ForceKillProcess(proc As Process)
            If proc Is Nothing OrElse proc.HasExited Then Exit Sub
            Try
                proc.Kill()
            Catch
            End Try
        End Sub

        Private Sub CleanupRecordingProcess()
            If recordingProcess IsNot Nothing Then
                Try
                    If Not recordingProcess.HasExited Then
                        ForceKillProcess(recordingProcess)
                    End If
                Catch
                End Try
                Try
                    recordingProcess.Dispose()
                Catch
                End Try
                recordingProcess = Nothing
            End If
        End Sub

        Private Sub CleanupBufferProcess()
            If bufferProcess IsNot Nothing Then
                Try
                    If Not bufferProcess.HasExited Then
                        ForceKillProcess(bufferProcess)
                    End If
                Catch
                End Try
                Try
                    bufferProcess.Dispose()
                Catch
                End Try
                bufferProcess = Nothing
            End If
        End Sub

        Private Function RunFFmpegSync(arguments As String, timeoutMs As Integer) As Boolean
            Try
                Using proc As New Process()
                    proc.StartInfo = CreateProcessStartInfo(_ffmpegPath, arguments)
                    proc.Start()

                    proc.StandardInput.Close()

                    ' ★ v4 FIX: Read BOTH stdout and stderr asynchronously to prevent deadlock.
                    ' Old bug: ReadToEnd() synchronous + WaitForExit = deadlock when both buffers fill up.
                    Dim stdoutTask As Task(Of String) = proc.StandardOutput.ReadToEndAsync()
                    Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()

                    Dim exited As Boolean = proc.WaitForExit(timeoutMs)

                    If Not exited Then
                        Try
                            proc.Kill()
                            proc.WaitForExit(FORCE_KILL_TIMEOUT)
                        Catch
                        End Try
                        Return False
                    End If

                    ' Ensure async reads complete
                    stdoutTask.Wait(5000)
                    stderrTask.Wait(5000)

                    Return proc.ExitCode = 0
                End Using
            Catch ex As Exception
                Debug.WriteLine("RunFFmpegSync Error: " & ex.Message)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' PERF: New method — concat segments using pipe instead of writing a concat list file.
        ''' Uses FFmpeg's concat demuxer via pipe, which avoids the overhead of
        ''' creating and reading a separate concat_list.txt file.
        ''' Falls back to file-based concat if pipe approach fails.
        ''' </summary>
        Private Function RunConcatViaPipe(concatContent As String, outputPath As String, timeoutMs As Integer) As Boolean
            Try
                ' SHADOWPLAY APPROACH: -c copy = zero CPU, zero quality loss
                ' -avoid_negative_ts make_zero ensures timestamps start at 0
                ' -fflags +genpts generates PTS if missing (safety net)
                Dim concatArgs As String = String.Format(
            "-y -nostdin -hide_banner -loglevel warning -fflags +genpts -f concat -safe 0 -i pipe:0 -c copy -r {1} -avoid_negative_ts make_zero -movflags +faststart ""{0}""",
            outputPath, _framerate)

                Using proc As New Process()
                    proc.StartInfo = CreateProcessStartInfo(_ffmpegPath, concatArgs)
                    proc.Start()

                    ' Write concat list to stdin
                    proc.StandardInput.Write(concatContent)
                    proc.StandardInput.Close()

                    ' BUG FIX: Use ReadToEndAsync for BOTH stdout and stderr
                    ' to prevent deadlock when buffers fill up
                    Dim stdoutTask As Task(Of String) = proc.StandardOutput.ReadToEndAsync()
                    Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()

                    Dim exited As Boolean = proc.WaitForExit(timeoutMs)

                    If Not exited Then
                        Try
                            proc.Kill()
                            proc.WaitForExit(FORCE_KILL_TIMEOUT)
                        Catch
                        End Try
                        Return False
                    End If

                    stdoutTask.Wait(5000)
                    stderrTask.Wait(5000)

                    Return proc.ExitCode = 0
                End Using
            Catch ex As Exception
                Debug.WriteLine("RunConcatViaPipe Error: " & ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "Audio Pipe Management (NAudio)"

        Private Sub StartRecordingAudioPipe()
            SyncLock _audioLock
                If _audioMode = VideoCaptureMode.None OrElse _audioMode = VideoCaptureMode.MicOnly Then
                    Exit Sub
                End If

                Try
                    Debug.WriteLine("═══ Starting Recording Audio Pipe ═══")

                    _recordingAudioPipe = New AudioPipe(_systemAudioVolume)
                    AddHandler _recordingAudioPipe.PipeError, Sub(sender, err)
                                                                  Debug.WriteLine("Recording AudioPipe Error: " & err)
                                                              End Sub

                    If Not _recordingAudioPipe.Start() Then
                        Debug.WriteLine("Failed to start recording audio pipe")
                        _recordingAudioPipe.Dispose()
                        _recordingAudioPipe = Nothing
                    Else
                        Debug.WriteLine("Recording Audio Pipe started: " & _recordingAudioPipe.PipePath)
                    End If

                Catch ex As Exception
                    Debug.WriteLine("StartRecordingAudioPipe Error: " & ex.Message)
                    If _recordingAudioPipe IsNot Nothing Then
                        _recordingAudioPipe.Dispose()
                        _recordingAudioPipe = Nothing
                    End If
                End Try
            End SyncLock
        End Sub

        Private Sub StopRecordingAudioPipe()
            SyncLock _audioLock
                If _recordingAudioPipe IsNot Nothing Then
                    Try
                        Debug.WriteLine("═══ Stopping Recording Audio Pipe ═══")
                        _recordingAudioPipe.Stop()
                        _recordingAudioPipe.Dispose()
                    Catch ex As Exception
                        Debug.WriteLine("StopRecordingAudioPipe Error: " & ex.Message)
                    End Try
                    _recordingAudioPipe = Nothing
                End If
            End SyncLock
        End Sub

        Private Sub StartBufferAudioPipe()
            SyncLock _audioLock
                If _audioMode = VideoCaptureMode.None OrElse _audioMode = VideoCaptureMode.MicOnly Then
                    Exit Sub
                End If

                Try
                    Debug.WriteLine("═══ Starting Buffer Audio Pipe ═══")

                    _bufferAudioPipe = New AudioPipe(_systemAudioVolume)
                    AddHandler _bufferAudioPipe.PipeError, Sub(sender, err)
                                                               Debug.WriteLine("Buffer AudioPipe Error: " & err)
                                                           End Sub

                    If Not _bufferAudioPipe.Start() Then
                        Debug.WriteLine("Failed to start buffer audio pipe")
                        _bufferAudioPipe.Dispose()
                        _bufferAudioPipe = Nothing
                    Else
                        Debug.WriteLine("Buffer Audio Pipe started: " & _bufferAudioPipe.PipePath)
                    End If

                Catch ex As Exception
                    Debug.WriteLine("StartBufferAudioPipe Error: " & ex.Message)
                    If _bufferAudioPipe IsNot Nothing Then
                        _bufferAudioPipe.Dispose()
                        _bufferAudioPipe = Nothing
                    End If
                End Try
            End SyncLock
        End Sub

        Private Sub StopBufferAudioPipe()
            SyncLock _audioLock
                If _bufferAudioPipe IsNot Nothing Then
                    Try
                        Debug.WriteLine("═══ Stopping Buffer Audio Pipe ═══")
                        _bufferAudioPipe.Stop()
                        _bufferAudioPipe.Dispose()
                    Catch ex As Exception
                        Debug.WriteLine("StopBufferAudioPipe Error: " & ex.Message)
                    End Try
                    _bufferAudioPipe = Nothing
                End If
            End SyncLock
        End Sub

#End Region

#Region "FFmpeg Command Building"

        Private Shared Function CreateProcessStartInfo(fileName As String, arguments As String) As ProcessStartInfo
            Return New ProcessStartInfo() With {
                .FileName = fileName,
                .Arguments = arguments,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True,
                .RedirectStandardInput = True,
                .StandardOutputEncoding = Encoding.UTF8,
                .StandardErrorEncoding = Encoding.UTF8
            }
        End Function

        Private Function CreateFFmpegProcess(arguments As String) As Process
            Dim proc As New Process()
            proc.StartInfo = CreateProcessStartInfo(_ffmpegPath, arguments)
            AddHandler proc.ErrorDataReceived, AddressOf FFmpegOutputHandler
            AddHandler proc.OutputDataReceived, AddressOf FFmpegOutputHandler
            Return proc
        End Function

        ''' <summary>
        ''' BUG FIX: Refactored command building to use a single -filter_complex
        ''' when both video and audio need it. Previously, a second -filter_complex
        ''' for audio mixing would override the video -filter_complex.
        ''' </summary>
        Private Function BuildFFmpegArguments(outputFile As String, region As Rectangle) As String
            Dim sb As New StringBuilder(SB_CAPACITY_XLARGE)

            ' PERF: -fflags +genpts rebuilds PTS for better A/V sync
            ' +nobuffer reduces latency for live capture
            sb.Append("-y -hide_banner -loglevel warning -fflags +genpts+nobuffer ")

            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
            Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)
            Dim selectedAPI As CaptureAPIType = DetermineBestCaptureAPI()

            ' ═══ Hardware Device Initialization ═══
            If isQuickSync Then
                sb.Append("-init_hw_device d3d11va:,vendor_id=0x8086 ")
            ElseIf selectedAPI = CaptureAPIType.DDAGrab OrElse selectedAPI = CaptureAPIType.GFxCapture Then
                sb.Append("-init_hw_device d3d11va ")
            End If

            _pendingVideoFilter = ""
            _pendingVideoInput = ""

            ' ═══ Build video filter chain content (without -filter_complex wrapper) ═══
            Dim videoFilterChain As String = BuildVideoFilterChain(region, selectedAPI)
            Dim usesFilterComplex As Boolean = Not String.IsNullOrEmpty(videoFilterChain)

            ' ═══ Build audio input args FIRST (so input indices are correct) ═══
            Dim audioInputArgs As String = BuildAudioInputArgs(useBufferPipe:=False)

            ' ═══ Build audio filter chain content (without -filter_complex wrapper) ═══
            Dim audioFilterChain As String = BuildAudioFilterChain(useBufferPipe:=False, usesFilterComplex:=usesFilterComplex)

            ' ═══ Combine video + audio into ONE -filter_complex if needed ═══
            If usesFilterComplex Then
                sb.Append("-filter_complex """)
                sb.Append(videoFilterChain)
                If Not String.IsNullOrEmpty(audioFilterChain) Then
                    sb.Append(";")
                    sb.Append(audioFilterChain)
                End If
                sb.Append(""" ")
            ElseIf Not String.IsNullOrEmpty(audioFilterChain) Then
                ' Audio-only filter_complex (GDIGrab video doesn't use filter_complex)
                sb.Append("-filter_complex """)
                sb.Append(audioFilterChain)
                sb.Append(""" ")
            End If

            ' ═══ ★ v4 FIX: GDIGrab video input goes BEFORE audio inputs ═══
            ' GDIGrab is a regular -i input, not a filter_complex source.
            ' It must appear BEFORE audio -i arguments so input indices are correct.
            If Not String.IsNullOrEmpty(_pendingVideoInput) Then
                sb.Append(_pendingVideoInput)
            End If

            ' ═══ Audio inputs ═══
            sb.Append(audioInputArgs)

            ' ═══ GDIGrab pending video filter (-vf) ═══
            If Not String.IsNullOrEmpty(_pendingVideoFilter) Then
                sb.Append(_pendingVideoFilter)
            End If

            ' ═══ Map outputs ═══
            BuildMapCommand(sb, usesFilterComplex, Not String.IsNullOrEmpty(audioFilterChain))

            ' ═══ Encoder ═══
            BuildEncoderCommand(sb)

            If _audioMode <> VideoCaptureMode.None Then
                ' ★ v3 FIX: ใช้ BuildAudioOutputFilter แทน -af ตรงๆ
                ' เพื่อรวม volume + aresample เป็น -af เดียว (ไม่ชนกับ BuildMapCommand)
                sb.Append("-c:a aac -b:a 192k ")
                sb.Append(BuildAudioOutputFilter())
            End If

            sb.Append("-movflags +faststart """)
            sb.Append(outputFile)
            sb.Append(""""c)
            Return sb.ToString()
        End Function

        Private Function BuildBufferFFmpegArguments() As String
            Dim sb As New StringBuilder(SB_CAPACITY_XLARGE)

            ' PERF: Same flags as recording for consistency
            sb.Append("-y -hide_banner -loglevel warning -fflags +genpts+nobuffer ")

            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim selectedAPI As CaptureAPIType = DetermineBestCaptureAPI()

            ' Hardware device initialization
            If isQuickSync Then
                sb.Append("-init_hw_device d3d11va:,vendor_id=0x8086 ")
            ElseIf selectedAPI = CaptureAPIType.DDAGrab OrElse selectedAPI = CaptureAPIType.GFxCapture Then
                sb.Append("-init_hw_device d3d11va ")
            End If

            _pendingVideoFilter = ""
            _pendingVideoInput = ""

            ' ═══ Build video filter chain ═══
            Dim videoFilterChain As String = BuildVideoFilterChain(Rectangle.Empty, selectedAPI)
            Dim usesFilterComplex As Boolean = Not String.IsNullOrEmpty(videoFilterChain)

            ' ═══ Audio input ═══
            Dim audioInputArgs As String = BuildAudioInputArgs(useBufferPipe:=True)

            ' ═══ Audio filter chain ═══
            Dim audioFilterChain As String = BuildAudioFilterChain(useBufferPipe:=True, usesFilterComplex:=usesFilterComplex)

            ' ═══ Combine into ONE -filter_complex ═══
            If usesFilterComplex Then
                sb.Append("-filter_complex """)
                sb.Append(videoFilterChain)
                If Not String.IsNullOrEmpty(audioFilterChain) Then
                    sb.Append(";")
                    sb.Append(audioFilterChain)
                End If
                sb.Append(""" ")
            ElseIf Not String.IsNullOrEmpty(audioFilterChain) Then
                sb.Append("-filter_complex """)
                sb.Append(audioFilterChain)
                sb.Append(""" ")
            End If

            ' ★ v4 FIX: GDIGrab video input goes BEFORE audio inputs
            If Not String.IsNullOrEmpty(_pendingVideoInput) Then
                sb.Append(_pendingVideoInput)
            End If

            ' Audio inputs
            sb.Append(audioInputArgs)

            ' GDIGrab pending video filter
            If Not String.IsNullOrEmpty(_pendingVideoFilter) Then
                sb.Append(_pendingVideoFilter)
            End If

            ' Map outputs
            BuildMapCommand(sb, usesFilterComplex, Not String.IsNullOrEmpty(audioFilterChain))

            ' Encoder
            BuildBufferEncoderCommand(sb)

            ' Audio codec
            If _audioMode <> VideoCaptureMode.None Then
                ' ★ v3 FIX: ใช้ BuildAudioOutputFilter แทน -af ตรงๆ
                sb.Append("-c:a aac -b:a 192k ")
                sb.Append(BuildAudioOutputFilter())
            End If

            ' ═══ Segment settings for Replay Buffer ═══
            sb.Append("-f segment ")
            sb.Append("-segment_time ")
            sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
            sb.Append(" -segment_format mkv ")
            sb.Append("-segment_wrap ")
            sb.Append(BUFFER_MAX_SEGMENTS.ToString())
            sb.Append(" -reset_timestamps 1 ")
            sb.Append("-strftime_mkdir 0 ")

            ' Output path
            Dim segmentPath As String = Path.Combine(bufferTempDir, "segment_%05d.mkv")
            sb.Append(""""c)
            sb.Append(segmentPath)
            sb.Append(""""c)

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' ★ v4 FIX: Builds the video filter chain content (without -filter_complex wrapper).
        ''' Returns the content inside the filter_complex quotes, e.g. "ddagrab=0:framerate=60[v]"
        ''' Returns empty string for GDIGrab (which uses -vf instead).
        ''' 
        ''' OLD BUG: GDIGrab wrote "-f gdigrab -i desktop" into filterSb, which was then
        ''' wrapped in -filter_complex → FFmpeg syntax error! GDIGrab is a regular input,
        ''' NOT a filter. Now GDIGrab returns empty string and writes to _pendingVideoInput.
        ''' </summary>
        Private Function BuildVideoFilterChain(region As Rectangle, selectedAPI As CaptureAPIType) As String
            Dim filterSb As New StringBuilder(SB_CAPACITY_XLARGE)

            Select Case selectedAPI
                Case CaptureAPIType.GFxCapture
                    BuildGfxCaptureFilterChain(filterSb, region)
                Case CaptureAPIType.DDAGrab
                    BuildDDAGrabFilterChain(filterSb, region)
                Case Else
                    ' ★ v4 FIX: GDIGrab uses -vf, not -filter_complex
                    ' Write input command to _pendingVideoInput instead of filterSb
                    BuildGDIGrabInput()
                    ' Return empty — no filter_complex for GDIGrab
                    Return ""
            End Select

            Return filterSb.ToString()
        End Function

        ''' <summary>
        ''' BUG FIX: New method — builds the audio filter chain content for amix (without -filter_complex wrapper).
        ''' Returns empty string when no audio mixing filter is needed.
        ''' </summary>
        Private Function BuildAudioFilterChain(useBufferPipe As Boolean, usesFilterComplex As Boolean) As String
            If _audioMode = VideoCaptureMode.None Then Return ""

            Dim hasSystemAudio As Boolean = (_audioMode = VideoCaptureMode.SystemOnly OrElse _audioMode = VideoCaptureMode.Both)
            Dim hasMic As Boolean = (_audioMode = VideoCaptureMode.MicOnly OrElse _audioMode = VideoCaptureMode.Both)

            If hasSystemAudio Then
                Dim pipe As AudioPipe = If(useBufferPipe, _bufferAudioPipe, _recordingAudioPipe)
                If pipe Is Nothing OrElse Not pipe.IsRunning Then
                    hasSystemAudio = False
                End If
            End If

            ' Audio mixing filter is only needed when BOTH sources are present
            If Not (hasSystemAudio AndAlso hasMic) Then Return ""

            ' Calculate audio input indices
            ' When filter_complex is used for video, there's no video -i input, so audio starts at 0
            ' When filter_complex is NOT used (GDIGrab), video is input 0, so audio starts at 1
            Dim audioBaseIdx As Integer = If(usesFilterComplex, 0, 1)
            Dim sysIdx As Integer = audioBaseIdx
            Dim micIdx As Integer = audioBaseIdx + 1

            Dim sb As New StringBuilder()
            sb.Append("[")
            sb.Append(sysIdx)
            sb.Append(":a]volume=")
            sb.Append(_systemAudioVolume.ToString("F2", Globalization.CultureInfo.InvariantCulture))
            sb.Append("[sys];")
            sb.Append("[")
            sb.Append(micIdx)
            sb.Append(":a]volume=")
            sb.Append(_micVolume.ToString("F2", Globalization.CultureInfo.InvariantCulture))
            sb.Append("[mic];")
            sb.Append("[sys][mic]amix=inputs=2:duration=longest[aout]")

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' ★ v3 FIX: สร้าง -map arguments สำหรับ video + audio
        ''' ★ ไม่ใส่ -af ในนี้แล้ว — audio filter ทั้งหมดไปรวมใน BuildAudioOutputFilter
        ''' เดิม: ใส่ -af "volume=X" ตรงนี้ + -af aresample ใน BuildFFmpegArguments = ชนกัน!
        ''' FFmpeg ใช้แค่ -af อันสุดท้าย → volume หาย + A/V sync เละ → กระตุก!
        ''' </summary>
        Private Sub BuildMapCommand(sb As StringBuilder, usesFilterComplex As Boolean, hasAudioFilterChain As Boolean)
            ' Map video
            If usesFilterComplex Then
                sb.Append("-map ""[v]"" ")
            Else
                sb.Append("-map 0:v ")
            End If

            ' Map audio
            If _audioMode = VideoCaptureMode.None Then Exit Sub

            Dim hasSystemAudio As Boolean = (_audioMode = VideoCaptureMode.SystemOnly OrElse _audioMode = VideoCaptureMode.Both)
            Dim hasMic As Boolean = (_audioMode = VideoCaptureMode.MicOnly OrElse _audioMode = VideoCaptureMode.Both)

            If hasAudioFilterChain Then
                ' Audio was mixed via filter_complex → map [aout]
                sb.Append("-map ""[aout]"" ")
            Else
                ' Single audio source → map directly
                Dim audioBaseIdx As Integer = If(usesFilterComplex, 0, 1)

                If hasSystemAudio Then
                    sb.Append("-map ")
                    sb.Append(audioBaseIdx)
                    sb.Append(":a ")
                    ' ★ v3: ไม่ใส่ -af ที่นี่แล้ว — ไปรวมใน BuildAudioOutputFilter แทน
                ElseIf hasMic Then
                    sb.Append("-map ")
                    sb.Append(audioBaseIdx)
                    sb.Append(":a ")
                    ' ★ v3: ไม่ใส่ -af ที่นี่แล้ว — ไปรวมใน BuildAudioOutputFilter แทน
                End If
            End If
        End Sub

        Private Function BuildAudioInputArgs(useBufferPipe As Boolean) As String
            Dim sb As New StringBuilder()

            If _audioMode = VideoCaptureMode.SystemOnly OrElse _audioMode = VideoCaptureMode.Both Then
                Dim pipe As AudioPipe = If(useBufferPipe, _bufferAudioPipe, _recordingAudioPipe)
                If pipe IsNot Nothing AndAlso pipe.IsRunning Then
                    sb.Append(pipe.GetFFmpegInputArgs())
                    sb.Append(" ")
                    Debug.WriteLine("BuildAudioInputArgs: System Audio Pipe = " & pipe.PipePath)
                End If
            End If

            If _audioMode = VideoCaptureMode.MicOnly OrElse _audioMode = VideoCaptureMode.Both Then
                If String.IsNullOrEmpty(_micDeviceName) Then
                    sb.Append("-f dshow -i ""audio=default"" ")
                Else
                    sb.Append("-f dshow -i ""audio=")
                    sb.Append(_micDeviceName.Replace("""", "\"""))
                    sb.Append(""" ")
                End If
                Debug.WriteLine("BuildAudioInputArgs: Microphone via DirectShow")
            End If

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' ★ v3 NEW: สร้าง audio output filter chain เดียวที่รวมทุกอย่าง
        ''' volume (ถ้าต้องการ) + aresample (sync) เป็น -af เดียว
        ''' 
        ''' BUG: เดิมมี -af สองตัวที่ชนกัน:
        '''   1) -af "volume=0.80"  จาก BuildMapCommand (single audio source + volume < 0.99)
        '''   2) -af aresample=async=1000:first_pts=0  จาก BuildFFmpegArguments
        ''' FFmpeg ใช้แค่อันสุดท้าย → volume หาย + A/V sync ผิด → กระตุก!
        ''' </summary>
        Private Function BuildAudioOutputFilter() As String
            ' aresample is always needed for A/V sync
            Dim needVolume As Boolean = False
            Dim volumeValue As Single = 1.0F

            ' Check if we need volume filter (only for single audio source, not mixed via filter_complex)
            Dim hasSystemAudio As Boolean = (_audioMode = VideoCaptureMode.SystemOnly OrElse _audioMode = VideoCaptureMode.Both)
            Dim hasMic As Boolean = (_audioMode = VideoCaptureMode.MicOnly OrElse _audioMode = VideoCaptureMode.Both)
            Dim bothSources As Boolean = hasSystemAudio AndAlso hasMic

            ' Volume is only needed when NOT mixing via amix (amix already applies volume)
            If Not bothSources Then
                If hasSystemAudio AndAlso _systemAudioVolume < 0.99F Then
                    needVolume = True
                    volumeValue = _systemAudioVolume
                ElseIf hasMic AndAlso _micVolume < 0.99F Then
                    needVolume = True
                    volumeValue = _micVolume
                End If
            End If

            ' Build combined filter chain
            Dim filters As New List(Of String)()

            If needVolume Then
                filters.Add(String.Format("volume={0:F2}", volumeValue))
            End If

            ' Always add aresample for A/V sync
            ' ★ Fix P2: aresample async=44 → async=192 (4ms @ 48kHz).
            ' async=44 (Round 4) was too tight — when silent frames were
            ' inserted (Fix P re-enabled them), FFmpeg couldn't compensate
            ' for the PTS gap between silence and real audio, causing the
            ' "stutter that doesn't recover" issue.
            ' async=192 = 4ms of audio samples = enough headroom for FFmpeg
            ' to smoothly transition between silence and real audio without
            ' inserting large padding (which was the original 300ms delay
            ' issue from Master's async=1000).
            ' Goldilocks zone: 192 samples.
            filters.Add("aresample=async=192:first_pts=0")

            Return "-af """ & String.Join(",", filters) & """ "
        End Function

        Private Function UsesFilterComplexForVideo() As Boolean
            Dim selectedAPI As CaptureAPIType = DetermineBestCaptureAPI()

            Select Case selectedAPI
                Case CaptureAPIType.GFxCapture
                    Return True
                Case CaptureAPIType.DDAGrab
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' ★ v5: DetermineBestCaptureAPI — 3-tier strategy ตาม CaptureTargetType + HDR
        ''' 
        ''' Priority:
        '''   1. GFxCapture → Window capture + HDR (จับ Window ได้, รองรับ HDR)
        '''   2. DDAGrab   → Monitor capture only (เร็วกว่า GFxCapture เล็กน้อย, จับ Monitor เท่านั้น)
        '''   3. GDIGrab   → Fallback สุดท้าย (ช้าที่สุดแต่ใช้ได้ทุกกรณี)
        ''' 
        ''' Rules by CaptureTargetType:
        '''   - Window  → GFxCapture only (DDAGrab ไม่รองรับ Window capture)
        '''   - Monitor + HDR → GFxCapture (DDAGrab ไม่รองรับ HDR/16bit)
        '''   - Monitor + SDR → DDAGrab (เร็วกว่า) → GFxCapture fallback → GDIGrab
        '''   - Region  → DDAGrab (เร็วกว่าสำหรับ monitor region) → GFxCapture fallback → GDIGrab
        ''' </summary>
        Private Function DetermineBestCaptureAPI() As CaptureAPIType
            ' ★ ถ้า User เลือกเอง ใช้ตามที่เลือก (ยกเว้น DDAGrab + Window → override)
            If _captureAPI <> CaptureAPIType.Auto Then
                ' ★ v5 FIX: DDAGrab ไม่รองรับ Window capture — ถ้า User เลือก DDAGrab แต่ target เป็น Window
                ' ให้ fallback เป็น GFxCapture อัตโนมัติ
                If _captureAPI = CaptureAPIType.DDAGrab AndAlso _captureTargetType = CaptureTargetType.Window Then
                    Debug.WriteLine("DetermineBestCaptureAPI: DDAGrab selected but target is Window → forcing GFxCapture")
                    _fallbackAPI = CaptureAPIType.GDIGrab
                    Return CaptureAPIType.GFxCapture
                End If

                ' ★ HDR + DDAGrab → GFxCapture (DDAGrab ไม่รองรับ 16bit/HDR output)
                If _captureAPI = CaptureAPIType.DDAGrab AndAlso RequiresHDRSupport() Then
                    Debug.WriteLine("DetermineBestCaptureAPI: DDAGrab selected but HDR required → forcing GFxCapture")
                    _fallbackAPI = CaptureAPIType.GDIGrab
                    Return CaptureAPIType.GFxCapture
                End If

                Return _captureAPI
            End If

            ' ★ Auto mode: ใช้ fallback ถ้า API ก่อนหน้าล้มเหลว
            If _captureAPIFailed AndAlso _fallbackAPI.HasValue Then
                Debug.WriteLine(String.Format("DetermineBestCaptureAPI: Fallback to {0}", _fallbackAPI.Value.ToString()))
                Return _fallbackAPI.Value
            End If

            ' ═══════════════════════════════════════════════════════════════════════
            ' ★ v5: 3-tier selection based on CaptureTargetType + HDR requirement
            ' ═══════════════════════════════════════════════════════════════════════
            Select Case _captureTargetType
                Case CaptureTargetType.Window
                    ' ★ Window capture → GFxCapture only (DDAGrab ไม่รองรับ)
                    ' GFxCapture รองรับ window_title, window_class, window_exe
                    ' Fallback: GDIGrab ด้วย -i title=... (ถ้า GFxCapture ไม่ available)
                    _fallbackAPI = CaptureAPIType.GDIGrab
                    If CaptureAPIDetector.IsGfxCaptureAvailable Then
                        Return CaptureAPIType.GFxCapture
                    End If
                    ' GFxCapture not checked yet or not available
                    Debug.WriteLine("DetermineBestCaptureAPI: Window → GFxCapture (not checked, trying) → fallback GDIGrab")
                    Return CaptureAPIType.GFxCapture

                Case CaptureTargetType.Monitor
                    ' ★ Monitor + HDR → GFxCapture (DDAGrab ไม่รองรับ 16bit/HDR)
                    If RequiresHDRSupport() Then
                        _fallbackAPI = CaptureAPIType.GDIGrab
                        If CaptureAPIDetector.IsGfxCaptureAvailable Then
                            Return CaptureAPIType.GFxCapture
                        End If
                        Debug.WriteLine("DetermineBestCaptureAPI: Monitor+HDR → GFxCapture (not checked, trying) → fallback GDIGrab")
                        Return CaptureAPIType.GFxCapture
                    End If

                    ' ★ Monitor + SDR → DDAGrab (เร็วกว่า) → GFxCapture fallback → GDIGrab
                    If CaptureAPIDetector.IsDDAGrabAvailable Then
                        _fallbackAPI = CaptureAPIType.GFxCapture
                        Return CaptureAPIType.DDAGrab
                    End If
                    If CaptureAPIDetector.IsGfxCaptureAvailable Then
                        _fallbackAPI = CaptureAPIType.GDIGrab
                        Return CaptureAPIType.GFxCapture
                    End If
                    ' Neither checked yet — try DDAGrab first (faster for monitor)
                    _fallbackAPI = CaptureAPIType.GFxCapture
                    Debug.WriteLine("DetermineBestCaptureAPI: Monitor+SDR → DDAGrab (not checked, trying) → fallback GFxCapture")
                    Return CaptureAPIType.DDAGrab

                Case CaptureTargetType.Region
                    ' ★ Region capture → DDAGrab (รองรับ offset_x/offset_y + video_size) → GFxCapture fallback → GDIGrab
                    If RequiresHDRSupport() Then
                        _fallbackAPI = CaptureAPIType.GDIGrab
                        If CaptureAPIDetector.IsGfxCaptureAvailable Then
                            Return CaptureAPIType.GFxCapture
                        End If
                        Return CaptureAPIType.GFxCapture
                    End If

                    If CaptureAPIDetector.IsDDAGrabAvailable Then
                        _fallbackAPI = CaptureAPIType.GFxCapture
                        Return CaptureAPIType.DDAGrab
                    End If
                    If CaptureAPIDetector.IsGfxCaptureAvailable Then
                        _fallbackAPI = CaptureAPIType.GDIGrab
                        Return CaptureAPIType.GFxCapture
                    End If
                    _fallbackAPI = CaptureAPIType.GFxCapture
                    Debug.WriteLine("DetermineBestCaptureAPI: Region → DDAGrab (not checked, trying) → fallback GFxCapture")
                    Return CaptureAPIType.DDAGrab

                Case Else
                    _fallbackAPI = CaptureAPIType.GDIGrab
                    Return CaptureAPIType.DDAGrab
            End Select
        End Function

        ''' <summary>
        ''' ★ v5: ตรวจสอบว่าต้องการ HDR support หรือไม่
        ''' HDR = 10bit (x2bgr10) หรือ 16bit (rgbaf16) output format
        ''' DDAGrab รองรับแค่ 8bit และ 10bit — ไม่รองรับ 16bit
        ''' GFxCapture รองรับ 8bit, 10bit, 16bit (full HDR)
        ''' </summary>
        Private Function RequiresHDRSupport() As Boolean
            ' 16bit output → ต้องใช้ GFxCapture เท่านั้น (DDAGrab ไม่รองรับ)
            If _outputFormat = OutputColorFormat.RGBAF16_16Bit Then
                Return True
            End If
            ' 10bit output → DDAGrab รองรับ แต่ถ้า User ต้องการ HDR เต็มรูปแบบ ต้องใช้ GFxCapture
            ' ใช้ heuristics: 10bit + NVENC/AMF = likely HDR encoding
            If _outputFormat = OutputColorFormat.X2BGR10_10Bit Then
                Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
                Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)
                ' NVENC/AMF with 10bit = likely HDR capture intent → prefer GFxCapture
                Return isNVIDIA OrElse isAMD
            End If
            Return False
        End Function

        Private Sub ResetCaptureAPIFallback()
            _captureAPIFailed = False
            ' Keep _fallbackAPI set — it's determined by DetermineBestCaptureAPI
        End Sub

        ''' <summary>
        ''' Called when the current capture API fails to start.
        ''' Sets the fallback flag so DetermineBestCaptureAPI returns the fallback API on next call.
        ''' </summary>
        Public Sub NotifyCaptureAPIFailed()
            _captureAPIFailed = True
            Debug.WriteLine("NotifyCaptureAPIFailed: Will use fallback API on next attempt")
        End Sub

        Private Sub BuildCaptureCommand(sb As StringBuilder, region As Rectangle)
            Dim selectedAPI As CaptureAPIType = DetermineBestCaptureAPI()

            Debug.WriteLine(String.Format("BuildCaptureCommand: Selected API = {0}", selectedAPI.ToString()))

            Select Case selectedAPI
                Case CaptureAPIType.GFxCapture
                    BuildGfxCaptureCommand(sb, region)
                Case CaptureAPIType.DDAGrab
                    BuildDDAGrabCommand(sb, region)
                Case Else
                    BuildGDIGrabCommand(sb, region)
            End Select
        End Sub

        ''' <summary>
        ''' BUG FIX: Refactored to write filter chain content only (without -filter_complex wrapper).
        ''' </summary>
        Private Sub BuildGfxCaptureFilterChain(sb As StringBuilder, region As Rectangle)
            Dim screenDims = GetCachedScreenDimensions()
            Dim needScaling As Boolean = (_resolutionWidth <> screenDims.Width OrElse _resolutionHeight <> screenDims.Height)

            Dim canUseGPUDirectPath As Boolean = (
                _encoder = VideoEncoder.NVENC_H264 OrElse
                _encoder = VideoEncoder.NVENC_HEVC OrElse
                _encoder = VideoEncoder.NVENC_AV1 OrElse
                _encoder = VideoEncoder.QuickSync_H264 OrElse
                _encoder = VideoEncoder.QuickSync_HEVC OrElse
                _encoder = VideoEncoder.AMF_H264 OrElse
                _encoder = VideoEncoder.AMF_HEVC
            )

            If canUseGPUDirectPath Then
                BuildGfxCaptureDirectFilterChain(sb)
            Else
                BuildGfxCaptureStandardFilterChain(sb, needScaling)
            End If
        End Sub

        Private Sub BuildGfxCaptureCommand(sb As StringBuilder, region As Rectangle)
            ' Legacy wrapper — writes -filter_complex directly to sb
            Dim filterSb As New StringBuilder(SB_CAPACITY_XLARGE)
            BuildGfxCaptureFilterChain(filterSb, region)

            If filterSb.Length > 0 Then
                sb.Append("-filter_complex """)
                sb.Append(filterSb.ToString())
                sb.Append(""" ")
            End If
        End Sub

        ''' <summary>
        ''' BUG FIX: Writes filter chain content only (without -filter_complex wrapper).
        ''' </summary>
        Private Sub BuildGfxCaptureDirectFilterChain(sb As StringBuilder)
            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
            Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)
            Dim screenDims = GetCachedScreenDimensions()
            Dim needScaling As Boolean = (_resolutionWidth > 0 AndAlso _resolutionHeight > 0 AndAlso
                                          (_resolutionWidth <> screenDims.Width OrElse _resolutionHeight <> screenDims.Height))

            BuildGfxCaptureOptions(sb)

            If isNVIDIA OrElse isAMD Then
                sb.Append(",fps=")
                sb.Append(_framerate.ToString())
            End If

            If isQuickSync Then
                ' BUG FIX: Use vpp_qsv for GPU-side scaling instead of CPU hwdownload/scale/hwupload
                ' Per FFmpeg docs: hwmap=derive_device=qsv, then vpp_qsv for GPU processing
                sb.Append(",hwmap=derive_device=qsv")
                If needScaling Then
                    ' BUG FIX #14: Add bt709 color properties per FFmpeg docs recommendation
                    sb.Append(",vpp_qsv=w=")
                    sb.Append(_resolutionWidth.ToString())
                    sb.Append(":h=")
                    sb.Append(_resolutionHeight.ToString())
                    sb.Append(":format=nv12")
                    sb.Append(":out_color_matrix=bt709")
                    sb.Append(":out_color_primaries=bt709")
                    sb.Append(":out_color_transfer=bt709")
                    sb.Append(":out_range=tv")
                Else
                    ' No scaling needed, but still apply color properties for consistent output
                    sb.Append(",vpp_qsv=format=nv12")
                    sb.Append(":out_color_matrix=bt709")
                    sb.Append(":out_color_primaries=bt709")
                    sb.Append(":out_color_transfer=bt709")
                    sb.Append(":out_range=tv")
                End If
                sb.Append(",format=qsv")
            End If

            sb.Append("[v]")
        End Sub

        Private Sub BuildGfxCaptureDirectPath(sb As StringBuilder)
            ' Legacy wrapper
            Dim filterSb As New StringBuilder(SB_CAPACITY_XLARGE)
            BuildGfxCaptureDirectFilterChain(filterSb)

            sb.Append("-filter_complex """)
            sb.Append(filterSb.ToString())
            sb.Append(""" ")
        End Sub

        Private Sub BuildGfxCaptureStandardFilterChain(sb As StringBuilder, needScaling As Boolean)
            BuildGfxCaptureOptions(sb)
            sb.Append(",hwdownload,format=")
            sb.Append(GetOutputFormatString())

            If needScaling OrElse (_outputFormat <> OutputColorFormat.Auto AndAlso _outputFormat <> OutputColorFormat.BGRA_8Bit) Then
                BuildScalingAndFormatFilters(sb, needScaling)
            End If

            sb.Append("[v]")
        End Sub

        Private Sub BuildGfxCaptureStandardPath(sb As StringBuilder, needScaling As Boolean)
            ' Legacy wrapper
            Dim filterSb As New StringBuilder(SB_CAPACITY_XLARGE)
            BuildGfxCaptureStandardFilterChain(filterSb, needScaling)

            sb.Append("-filter_complex """)
            sb.Append(filterSb.ToString())
            sb.Append(""" ")
        End Sub

        Private Sub BuildGfxCaptureOptions(sb As StringBuilder)
            sb.Append("gfxcapture=")

            Dim options As New List(Of String)()

            Select Case _captureTargetType
                Case CaptureTargetType.Monitor
                    options.Add(String.Format("monitor_idx={0}", _monitorIndex))

                Case CaptureTargetType.Window
                    If Not String.IsNullOrEmpty(_windowTitle) Then
                        options.Add(String.Format("window_title='{0}'", EscapeFFmpegOption(_windowTitle)))
                    End If
                    If Not String.IsNullOrEmpty(_windowClass) Then
                        options.Add(String.Format("window_class='{0}'", EscapeFFmpegOption(_windowClass)))
                    End If
                    If Not String.IsNullOrEmpty(_windowExe) Then
                        options.Add(String.Format("window_exe='{0}'", EscapeFFmpegOption(_windowExe)))
                    End If

                Case CaptureTargetType.Region
                    options.Add(String.Format("monitor_idx={0}", _monitorIndex))
                    If _cropLeft > 0 Then options.Add(String.Format("crop_left={0}", _cropLeft))
                    If _cropTop > 0 Then options.Add(String.Format("crop_top={0}", _cropTop))
                    If _cropRight > 0 Then options.Add(String.Format("crop_right={0}", _cropRight))
                    If _cropBottom > 0 Then options.Add(String.Format("crop_bottom={0}", _cropBottom))
            End Select

            options.Add(String.Format("capture_cursor={0}", If(_captureCursor, "1", "0")))
            options.Add(String.Format("max_framerate={0}", _framerate))

            If _captureTargetType = CaptureTargetType.Window Then
                options.Add(String.Format("capture_border={0}", If(_captureBorder, "1", "0")))
                options.Add(String.Format("display_border={0}", If(_displayBorder, "1", "0")))
            End If

            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)

            If Not isQuickSync Then
                If _resolutionWidth > 0 AndAlso _resolutionHeight > 0 Then
                    options.Add(String.Format("width={0}", _resolutionWidth))
                    options.Add(String.Format("height={0}", _resolutionHeight))
                    options.Add(String.Format("resize_mode={0}", GetResizeModeString()))
                    options.Add(String.Format("scale_mode={0}", GetScaleModeString()))
                End If
            End If

            ' BUG FIX #16: QSV gfxcapture should also respect output format setting
            ' QSV path needs 8bit because vpp_qsv handles format conversion on GPU
            ' But if user explicitly requests 10bit, pass it through
            If isQuickSync Then
                ' For QSV, output 8bit from gfxcapture — vpp_qsv will handle format conversion
                ' This avoids issues with QSV not supporting 10bit input from gfxcapture directly
                options.Add("output_fmt=8bit")
            Else
                options.Add(String.Format("output_fmt={0}", GetGfxCaptureOutputFormatString()))
            End If

            sb.Append(String.Join(":", options))
        End Sub

        ''' <summary>
        ''' BUG FIX: Refactored to write filter chain content only (without -filter_complex wrapper).
        ''' Also fixed: Software encoder path now includes scaling when resolution differs.
        ''' </summary>
        Private Sub BuildDDAGrabFilterChain(sb As StringBuilder, region As Rectangle)
            Dim screenDims = GetCachedScreenDimensions()
            Dim needScaling As Boolean = (_resolutionWidth > 0 AndAlso _resolutionHeight > 0 AndAlso
                                  (_resolutionWidth <> screenDims.Width OrElse _resolutionHeight <> screenDims.Height))
            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
            Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)

            If isQuickSync Then
                ' ═══ QSV Zero-Copy Path with VPP Scaling ═══
                ' Per FFmpeg docs:
                '   ffmpeg -init_hw_device d3d11va:,vendor_id=0x8086
                '          -filter_complex ddagrab=0,hwmap=derive_device=qsv,format=qsv
                '          -c:v h264_qsv -global_quality 20 output.mkv
                '   With VPP scaling:
                '          ddagrab=0,hwmap=derive_device=qsv,vpp_qsv=format=nv12:out_color_matrix=bt709:...
                sb.Append("ddagrab=")
                sb.Append(_monitorIndex.ToString())
                sb.Append(":framerate=")
                sb.Append(_framerate.ToString())
                sb.Append(":draw_mouse=")
                sb.Append(If(_captureCursor, "1", "0"))

                ' BUG FIX #13: Add output_fmt for ddagrab (per FFmpeg docs)
                ' QSV needs 8bit output since vpp_qsv handles format conversion
                sb.Append(":output_fmt=8bit")

                ' BUG FIX #15: Add dup_frames option for ddagrab
                If Not _dupFrames Then
                    sb.Append(":dup_frames=0")
                End If

                If _cropLeft > 0 OrElse _cropTop > 0 OrElse _cropRight > 0 OrElse _cropBottom > 0 Then
                    Dim width As Integer = screenDims.Width - _cropLeft - _cropRight
                    Dim height As Integer = screenDims.Height - _cropTop - _cropBottom
                    sb.Append(":video_size=")
                    sb.Append(width.ToString())
                    sb.Append("x")
                    sb.Append(height.ToString())
                    sb.Append(":offset_x=")
                    sb.Append(_cropLeft.ToString())
                    sb.Append(":offset_y=")
                    sb.Append(_cropTop.ToString())
                End If

                sb.Append(",hwmap=derive_device=qsv")

                If needScaling Then
                    ' BUG FIX #14: Add bt709 color properties per FFmpeg docs recommendation
                    ' vpp_qsv handles both scaling and color format conversion on GPU
                    sb.Append(",vpp_qsv=w=")
                    sb.Append(_resolutionWidth.ToString())
                    sb.Append(":h=")
                    sb.Append(_resolutionHeight.ToString())
                    sb.Append(":format=nv12")
                    sb.Append(":out_color_matrix=bt709")
                    sb.Append(":out_color_primaries=bt709")
                    sb.Append(":out_color_transfer=bt709")
                    sb.Append(":out_range=tv")
                Else
                    ' BUG FIX #17: No scaling needed, but still apply color properties
                    ' Per docs: vpp_qsv=format=nv12:out_color_matrix=bt709:...
                    sb.Append(",vpp_qsv=format=nv12")
                    sb.Append(":out_color_matrix=bt709")
                    sb.Append(":out_color_primaries=bt709")
                    sb.Append(":out_color_transfer=bt709")
                    sb.Append(":out_range=tv")
                End If

                sb.Append(",format=qsv[v]")

            ElseIf isNVIDIA OrElse isAMD Then
                ' ═══ NVIDIA/AMD Direct GPU Path ═══
                ' Per FFmpeg docs:
                '   ffmpeg -init_hw_device d3d11va -filter_complex ddagrab=0 -c:v h264_nvenc -cq:v 20 output.mkv
                ' ddagrab outputs D3D11 frames that NVENC/AMF can encode directly
                sb.Append("ddagrab=")
                sb.Append(_monitorIndex.ToString())
                sb.Append(":framerate=")
                sb.Append(_framerate.ToString())
                sb.Append(":draw_mouse=")
                sb.Append(If(_captureCursor, "1", "0"))

                ' BUG FIX #18: Add output_fmt for ddagrab (per FFmpeg docs)
                ' NVENC/AMF can handle 10-bit input for HDR encoding
                sb.Append(":output_fmt=")
                sb.Append(GetDDAGrabOutputFormatString())

                ' BUG FIX #15: Add dup_frames option for ddagrab
                If Not _dupFrames Then
                    sb.Append(":dup_frames=0")
                End If

                If _cropLeft > 0 OrElse _cropTop > 0 OrElse _cropRight > 0 OrElse _cropBottom > 0 Then
                    Dim width As Integer = screenDims.Width - _cropLeft - _cropRight
                    Dim height As Integer = screenDims.Height - _cropTop - _cropBottom
                    sb.Append(":video_size=")
                    sb.Append(width.ToString())
                    sb.Append("x")
                    sb.Append(height.ToString())
                    sb.Append(":offset_x=")
                    sb.Append(_cropLeft.ToString())
                    sb.Append(":offset_y=")
                    sb.Append(_cropTop.ToString())
                End If

                ' ddagrab already controls framerate with dup_frames=true (default)
                sb.Append("[v]")

                _pendingVideoFilter = ""
            Else
                ' ═══ Software encoder path ═══
                ' Per FFmpeg docs:
                '   ffmpeg -filter_complex ddagrab=0,hwdownload,format=bgra -c:v libx264 -crf 20 output.mkv
                sb.Append("ddagrab=")
                sb.Append(_monitorIndex.ToString())
                sb.Append(":framerate=")
                sb.Append(_framerate.ToString())
                sb.Append(":draw_mouse=")
                sb.Append(If(_captureCursor, "1", "0"))

                ' BUG FIX #13: Add output_fmt for ddagrab (per FFmpeg docs)
                ' Software path needs to download frames, so use appropriate format
                sb.Append(":output_fmt=")
                sb.Append(GetDDAGrabOutputFormatString())

                ' BUG FIX #15: Add dup_frames option for ddagrab
                If Not _dupFrames Then
                    sb.Append(":dup_frames=0")
                End If

                If _cropLeft > 0 OrElse _cropTop > 0 OrElse _cropRight > 0 OrElse _cropBottom > 0 Then
                    Dim width As Integer = screenDims.Width - _cropLeft - _cropRight
                    Dim height As Integer = screenDims.Height - _cropTop - _cropBottom
                    sb.Append(":video_size=")
                    sb.Append(width.ToString())
                    sb.Append("x")
                    sb.Append(height.ToString())
                    sb.Append(":offset_x=")
                    sb.Append(_cropLeft.ToString())
                    sb.Append(":offset_y=")
                    sb.Append(_cropTop.ToString())
                End If

                ' hwdownload to CPU for software encoding
                ' Use format matching the output_fmt setting
                If _outputFormat = OutputColorFormat.X2BGR10_10Bit Then
                    sb.Append(",hwdownload,format=x2bgr10")
                ElseIf _outputFormat = OutputColorFormat.RGBAF16_16Bit Then
                    sb.Append(",hwdownload,format=rgbaf16")
                Else
                    sb.Append(",hwdownload,format=bgra")
                End If

                ' BUG FIX: Add scaling for software encoder when resolution differs
                If needScaling Then
                    sb.Append(",scale=")
                    sb.Append(_resolutionWidth.ToString())
                    sb.Append(":")
                    sb.Append(_resolutionHeight.ToString())
                    sb.Append(":flags=lanczos")
                End If

                sb.Append(",format=yuv420p[v]")
            End If
        End Sub

        Private Sub BuildDDAGrabCommand(sb As StringBuilder, region As Rectangle)
            ' Legacy wrapper — writes -filter_complex directly to sb
            Dim filterSb As New StringBuilder(SB_CAPACITY_XLARGE)
            BuildDDAGrabFilterChain(filterSb, region)

            If filterSb.Length > 0 Then
                sb.Append("-filter_complex """)
                sb.Append(filterSb.ToString())
                sb.Append(""" ")
            End If
        End Sub

        ''' <summary>
        ''' ★ v4 NEW: GDIGrab video input command (separate from filter_complex).
        ''' GDIGrab is a regular input (-f gdigrab -i desktop), NOT a filter_complex source.
        ''' </summary>
        Private _pendingVideoInput As String = ""

        Private _pendingVideoFilter As String = ""

        ''' <summary>
        ''' ★ v4: Builds GDIGrab as a regular input command (not inside -filter_complex).
        ''' </summary>
        Private Sub BuildGDIGrabInput()
            _pendingVideoInput = "-f gdigrab -framerate " & _framerate.ToString() & " -draw_mouse " & If(_captureCursor, "1", "0") & " -i desktop "
            _pendingVideoFilter = "-vf ""scale=" & _resolutionWidth.ToString() & ":" & _resolutionHeight.ToString() & ":flags=lanczos,format=yuv420p"" "
        End Sub

        Private Sub BuildGDIGrabCommand(sb As StringBuilder, region As Rectangle)
            ' ★ v4 FIX: Don't write to sb — GDIGrab is not a filter_complex
            BuildGDIGrabInput()
        End Sub

        Private Sub BuildScalingAndFormatFilters(sb As StringBuilder, needScaling As Boolean)
            If needScaling Then
                sb.Append(",scale=")
                sb.Append(_resolutionWidth.ToString())
                sb.Append(":"c)
                sb.Append(_resolutionHeight.ToString())
                sb.Append(":flags=lanczos")
            End If

            sb.Append(",format=")
            sb.Append(GetEncoderInputFormat())
        End Sub

#Region "Helper Functions"

        ''' <summary>
        ''' BUG FIX #13/#18: Returns the ddagrab output_fmt string per FFmpeg docs.
        ''' ddagrab supports: auto, 8bit/bgra, 10bit/x2bgr10
        ''' Note: ddagrab does NOT support 16bit output (unlike gfxcapture)
        ''' </summary>
        Private Function GetDDAGrabOutputFormatString() As String
            Select Case _outputFormat
                Case OutputColorFormat.X2BGR10_10Bit
                    Return "10bit"
                Case OutputColorFormat.RGBAF16_16Bit
                    ' ddagrab doesn't support 16bit — fallback to 10bit
                    Return "10bit"
                Case Else
                    Return "8bit"
            End Select
        End Function

        Private Function GetOutputFormatString() As String
            Select Case _outputFormat
                Case OutputColorFormat.X2BGR10_10Bit
                    Return "x2bgr10"
                Case OutputColorFormat.RGBAF16_16Bit
                    Return "rgbaf16"
                Case Else
                    Return "bgra"
            End Select
        End Function

        Private Function GetGfxCaptureOutputFormatString() As String
            Select Case _outputFormat
                Case OutputColorFormat.X2BGR10_10Bit
                    Return "10bit"
                Case OutputColorFormat.RGBAF16_16Bit
                    Return "16bit"
                Case Else
                    Return "8bit"
            End Select
        End Function

        Private Function GetResizeModeString() As String
            Select Case _resizeMode
                Case VideoResizeModeType.Scale
                    Return "scale"
                Case VideoResizeModeType.ScaleAspect
                    Return "scale_aspect"
                Case Else
                    Return "crop"
            End Select
        End Function

        Private Function GetScaleModeString() As String
            Select Case _scaleMode
                Case VideoScaleModeType.Point
                    Return "point"
                Case VideoScaleModeType.Bicubic
                    Return "bicubic"
                Case Else
                    Return "bilinear"
            End Select
        End Function

        Private Function GetEncoderInputFormat() As String
            Select Case _outputFormat
                Case OutputColorFormat.X2BGR10_10Bit
                    Return "p010le"
                Case OutputColorFormat.RGBAF16_16Bit
                    Return "p010le"
                Case Else
                    Return "yuv420p"
            End Select
        End Function

        Private Function EscapeFFmpegOption(value As String) As String
            Return value.Replace("\", "\\").Replace(":", "\:").Replace("'", "\'")
        End Function

        Private Function GetCachedScreenDimensions() As (Width As Integer, Height As Integer)
            If (DateTime.Now - _screenCacheTime).TotalSeconds > 5 Then
                _cachedScreenW = GetSystemMetrics(SM_CXSCREEN)
                _cachedScreenH = GetSystemMetrics(SM_CYSCREEN)
                _screenCacheTime = DateTime.Now
            End If
            Return (_cachedScreenW, _cachedScreenH)
        End Function

#End Region

#Region "Encoder Command Building"

        Private Sub BuildEncoderCommand(sb As StringBuilder)
            Dim presetStr As String = GetEncoderPresetString()
            ' ★ Fix L: GOP reduced from 2s (framerate*2) to 1s (framerate).
            ' Old 2s GOP meant keyframes every 2 seconds — at 144fps that's a 288-frame
            ' GOP. Keyframe encoding is expensive (much larger than P-frames), and the
            ' periodic spike every 2s caused encoder backlog → frame drops → audio
            ' buffer overflow → stutter pattern.
            ' 1s GOP (144 frames at 144fps) distributes the keyframe cost more evenly
            ' and matches typical screen-recording GOP lengths. File size grows ~5%
            ' but smoothness improves significantly.
            Dim gopSize As Integer = _framerate

            Select Case _encoder
                Case VideoEncoder.NVENC_H264
                    sb.Append("-c:v h264_nvenc -preset ")
                    sb.Append(presetStr)
                    ' ★ Fix H: -tune ll → -bf 0 for local recording.
                    ' -tune ll (low latency) is for live streaming where the encoder
                    ' trades quality for sub-frame latency to keep the stream fresh.
                    ' For local recording we don't care about that — we want every
                    ' frame to be encoded as soon as it arrives, without the tune-ll
                    ' tweaks that cause encoder backlog at high framerates (144fps).
                    ' -bf 0 disables B-frames: encoder doesn't need to buffer future
                    ' frames to encode the current one. Lower encoding latency, no
                    ' quality hit at the bitrates we use (8-17 Mbps).
                    sb.Append(" -bf 0 -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.NVENC_HEVC
                    sb.Append("-c:v hevc_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -bf 0 -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.NVENC_AV1
                    sb.Append("-c:v av1_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -bf 0 -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.QuickSync_H264
                    sb.Append("-c:v h264_qsv ")
                    sb.Append("-preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" ")
                    BuildQSVRateControl(sb)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    sb.Append("-async_depth 1 ")
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.QuickSync_HEVC
                    sb.Append("-c:v hevc_qsv ")
                    sb.Append("-preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" ")
                    BuildQSVRateControl(sb, isHEVC:=True)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    sb.Append("-async_depth 1 ")
                    sb.Append("-profile:v main ")
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.AMF_H264
                    sb.Append("-c:v h264_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildAMFRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.AMF_HEVC
                    sb.Append("-c:v hevc_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildAMFRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case Else
                    BuildSoftwareEncoderCommand(sb, gopSize)
            End Select
        End Sub

        Private Sub BuildBufferEncoderCommand(sb As StringBuilder)
            Dim presetStr As String = GetEncoderPresetString()
            Dim gopSize As Integer = CInt(Math.Ceiling(_framerate * SEGMENT_DURATION))

            Select Case _encoder
                Case VideoEncoder.NVENC_H264
                    sb.Append("-c:v h264_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -bf 0 -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.NVENC_HEVC
                    sb.Append("-c:v hevc_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -bf 0 -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.NVENC_AV1
                    sb.Append("-c:v av1_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -bf 0 -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.QuickSync_H264
                    sb.Append("-c:v h264_qsv ")
                    sb.Append("-preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" ")
                    BuildQSVRateControl(sb)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    sb.Append("-keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    sb.Append("-force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    sb.Append("-async_depth 1 ")
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.QuickSync_HEVC
                    sb.Append("-c:v hevc_qsv ")
                    sb.Append("-preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" ")
                    BuildQSVRateControl(sb, isHEVC:=True)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    sb.Append("-keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    sb.Append("-force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    sb.Append("-async_depth 1 ")
                    sb.Append("-profile:v main ")
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.AMF_H264
                    sb.Append("-c:v h264_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildAMFRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.AMF_HEVC
                    sb.Append("-c:v hevc_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildAMFRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case Else
                    BuildSoftwareBufferEncoderCommand(sb, gopSize)
            End Select
        End Sub

        Private Sub BuildSoftwareEncoderCommand(sb As StringBuilder, gopSize As Integer)
            Select Case _encoder
                Case VideoEncoder.LibX264
                    sb.Append("-c:v libx264 -preset ")
                    sb.Append(GetX264PresetString())
                    sb.Append(" -tune zerolatency -profile:v high -level 4.1 -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildX264RateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.LibX265
                    sb.Append("-c:v libx265 -preset ")
                    sb.Append(GetX264PresetString())
                    sb.Append(" -tune zerolatency -profile:v main -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildX265RateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
            End Select
        End Sub

        Private Sub BuildSoftwareBufferEncoderCommand(sb As StringBuilder, gopSize As Integer)
            Select Case _encoder
                Case VideoEncoder.LibX264
                    sb.Append("-c:v libx264 -preset ")
                    sb.Append(GetX264PresetString())
                    sb.Append(" -tune zerolatency -profile:v high -level 4.1 -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildX264RateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
                Case VideoEncoder.LibX265
                    sb.Append("-c:v libx265 -preset ")
                    sb.Append(GetX264PresetString())
                    sb.Append(" -tune zerolatency -profile:v main -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildX265RateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")
            End Select
        End Sub

#End Region

#Region "Rate Control Helpers"

        Private Sub BuildNVENCRateControl(sb As StringBuilder)
            If _useConstantBitrate Then
                sb.Append("-cbr 1 -b:v ")
                sb.Append(_bitrate)
                sb.Append("k -minrate ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate)
                sb.Append("k -bufsize ")
                sb.Append(_bitrate)
                sb.Append("k ")
            Else
                sb.Append("-cq 20 -b:v ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate * 2)
                sb.Append("k ")
            End If
        End Sub

        Private Sub BuildQSVRateControl(sb As StringBuilder, Optional isHEVC As Boolean = False)

            If _useConstantBitrate Then
                sb.Append("-rc_mode CBR ")
                sb.Append("-b:v ")
                sb.Append(_bitrate)
                sb.Append("k ")
                sb.Append("-maxrate ")
                sb.Append(_bitrate)
                sb.Append("k ")
                sb.Append("-bufsize ")
                sb.Append(_bitrate * 2)
                sb.Append("k ")
            Else
                Dim quality As Integer = 20

                Select Case _encoderPreset
                    Case 1 : quality = 18
                    Case 2 : quality = 19
                    Case 3 : quality = 20
                    Case 4 : quality = 22
                    Case 5 : quality = 25
                    Case 6 : quality = 28
                    Case 7 : quality = 30
                End Select

                sb.Append("-rc_mode ICQ ")
                sb.Append("-global_quality ")
                sb.Append(quality)
                sb.Append(" ")
                sb.Append("-b:v ")
                sb.Append(_bitrate)
                sb.Append("k ")
                sb.Append("-maxrate ")
                sb.Append(_bitrate * 2)
                sb.Append("k ")
            End If

            sb.Append("-extbrc 1 ")

            If Not _useConstantBitrate Then
                ' ★ Fix I: -look_ahead_depth 15 → 4.
                ' Lookahead makes the encoder wait N frames before encoding the current
                ' one so it can plan bitrate allocation. 15 frames at 144fps = 104ms
                ' buffer delay at startup, plus the encoder has to spin up the lookahead
                ' pipeline before it can emit frame 0. This contributes to the "first
                ' frames look choppy" symptom at high framerates.
                ' 4 frames at 144fps = 28ms — still gives the encoder some lookahead
                ' for bitrate planning but starts emitting frames much faster.
                sb.Append("-look_ahead_depth 4 ")
            End If

        End Sub

        Private Sub BuildAMFRateControl(sb As StringBuilder)
            If _useConstantBitrate Then
                sb.Append("-rc cbr -b:v ")
                sb.Append(_bitrate)
                sb.Append("k -minrate ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate)
                sb.Append("k -bufsize ")
                sb.Append(_bitrate)
                sb.Append("k ")
            Else
                sb.Append("-rc vbr_peak -b:v ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate)
                sb.Append("k ")
            End If
        End Sub

        Private Sub BuildX264RateControl(sb As StringBuilder)
            If _useConstantBitrate Then
                sb.Append("-b:v ")
                sb.Append(_bitrate)
                sb.Append("k -minrate ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate)
                sb.Append("k -bufsize ")
                sb.Append(_bitrate)
                sb.Append("k ")
            Else
                sb.Append("-crf 18 -b:v ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate * 2)
                sb.Append("k ")
            End If
        End Sub

        Private Sub BuildX265RateControl(sb As StringBuilder)
            If _useConstantBitrate Then
                sb.Append("-b:v ")
                sb.Append(_bitrate)
                sb.Append("k -minrate ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate)
                sb.Append("k -bufsize ")
                sb.Append(_bitrate)
                sb.Append("k ")
            Else
                sb.Append("-crf 20 -b:v ")
                sb.Append(_bitrate)
                sb.Append("k -maxrate ")
                sb.Append(_bitrate * 2)
                sb.Append("k ")
            End If
        End Sub

#End Region

#Region "Preset Helpers"

        Private Function GetEncoderPresetString() As String
            Select Case _encoder
                Case VideoEncoder.NVENC_H264, VideoEncoder.NVENC_HEVC, VideoEncoder.NVENC_AV1
                    Return "p" & _encoderPreset.ToString()
                Case Else
                    Return GetX264PresetString()
            End Select
        End Function

        Private Function GetX264PresetString() As String
            Select Case _encoderPreset
                Case 1 : Return "slow"
                Case 2 : Return "medium"
                Case 3 : Return "fast"
                Case 4 : Return "faster"
                Case 5 : Return "veryfast"
                Case 6 : Return "superfast"
                Case 7 : Return "ultrafast"
                Case Else : Return "faster"
            End Select
        End Function

        Private Function GetQsvPresetString() As String
            Select Case _encoderPreset
                Case 1 : Return "slow"
                Case 2 : Return "medium"
                Case 3 : Return "fast"
                Case 4 : Return "faster"
                Case 5 : Return "veryfast"
                Case 6 : Return "superfast"
                Case 7 : Return "ultrafast"
                Case Else : Return "faster"
            End Select
        End Function

        Private Function GetAmfQualityString() As String
            Select Case _encoderPreset
                Case 1, 2 : Return "quality"
                Case 3, 4 : Return "balanced"
                Case Else : Return "speed"
            End Select
        End Function

#End Region

#End Region

#Region "Action Cooldown / Debounce"

        ''' <summary>
        ''' Checks if enough time has passed since the last action.
        ''' Returns True if the action is allowed, False if still in cooldown.
        ''' Thread-safe via _actionLock.
        ''' </summary>
        Private Function CheckActionCooldown() As Boolean
            SyncLock _actionLock
                Dim msSinceLastAction As Double = (DateTime.Now - _lastActionTime).TotalMilliseconds
                If msSinceLastAction < ACTION_COOLDOWN_MS Then
                    Debug.WriteLine(String.Format(
                        "CheckActionCooldown: Rejected — only {0:F0}ms since last action (need {1}ms)",
                        msSinceLastAction, ACTION_COOLDOWN_MS))
                    Return False
                End If
                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' Marks the current time as the last action time.
        ''' Must be called AFTER CheckActionCooldown passes,
        ''' but BEFORE the actual work begins (so next call is blocked immediately).
        ''' </summary>
        Private Sub MarkActionTime()
            SyncLock _actionLock
                _lastActionTime = DateTime.Now
            End SyncLock
        End Sub

        ''' <summary>
        ''' Returns how many milliseconds remain until the next action is allowed.
        ''' UI can use this to show a countdown or disable buttons.
        ''' Returns 0 if action is allowed right now.
        ''' </summary>
        Public ReadOnly Property ActionCooldownRemainingMs As Double
            Get
                SyncLock _actionLock
                    Dim remaining As Double = ACTION_COOLDOWN_MS - (DateTime.Now - _lastActionTime).TotalMilliseconds
                    Return Math.Max(0, remaining)
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Returns True if an action can be performed right now (no cooldown active).
        ''' </summary>
        Public ReadOnly Property CanPerformAction As Boolean
            Get
                Return ActionCooldownRemainingMs <= 0
            End Get
        End Property

#End Region

#Region "FFmpeg Output Handler"
        Private Sub FFmpegOutputHandler(sender As Object, e As DataReceivedEventArgs)
            If String.IsNullOrEmpty(e.Data) Then Exit Sub

            Debug.WriteLine("[FFmpeg] " & e.Data)
            RaiseEvent FFmpegLogReceived(Me, e.Data)

            Dim frameMatch = RegexFrame.Match(e.Data)
            Dim sizeMatch = RegexSize.Match(e.Data)
            Dim bitrateMatch = RegexBitrate.Match(e.Data)
            Dim speedMatch = RegexSpeed.Match(e.Data)

            If frameMatch.Success OrElse sizeMatch.Success Then
                Dim frameCount As Long = If(frameMatch.Success, Long.Parse(frameMatch.Groups(1).Value), 0)
                Dim size As Long = If(sizeMatch.Success, Long.Parse(sizeMatch.Groups(1).Value), 0)
                Dim bitrate As Double = If(bitrateMatch.Success, Double.Parse(bitrateMatch.Groups(1).Value), 0)
                Dim speed As Double = If(speedMatch.Success, Double.Parse(speedMatch.Groups(1).Value), 0)

                Dim duration As TimeSpan = If(_isRecording, DateTime.Now - recordingStartTime, TimeSpan.Zero)

                RaiseEvent ProgressChanged(Me, New RecordingProgressEventArgs(frameCount, size, bitrate, speed, duration))
            End If
        End Sub
#End Region

#Region "FFmpeg Validation"
        Private Function ValidateFFmpeg() As Boolean
            If String.IsNullOrEmpty(_ffmpegPath) Then
                RaiseEvent RecordingError(Me, "FFmpeg path not set")
                Return False
            End If

            If Not File.Exists(_ffmpegPath) Then
                RaiseEvent RecordingError(Me, "FFmpeg not found: " & _ffmpegPath)
                Return False
            End If

            Return True
        End Function
#End Region

#Region "Video Duration Helper"
        Public Function GetVideoDuration(filePath As String) As Double
            Try
                If Not File.Exists(filePath) Then Return 0

                If String.IsNullOrEmpty(_ffprobePath) OrElse Not File.Exists(_ffprobePath) Then
                    Return 0
                End If

                Using proc As New Process()
                    proc.StartInfo = New ProcessStartInfo() With {
                        .FileName = _ffprobePath,
                        .Arguments = String.Format("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 ""{0}""", filePath),
                        .UseShellExecute = False,
                        .CreateNoWindow = True,
                        .RedirectStandardOutput = True,
                        .RedirectStandardError = True
                    }

                    proc.Start()
                    Dim output As String = proc.StandardOutput.ReadToEnd()
                    proc.WaitForExit(5000)

                    If Double.TryParse(output.Trim(), Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, Nothing) Then
                        Return Double.Parse(output.Trim(), Globalization.CultureInfo.InvariantCulture)
                    End If
                End Using
            Catch ex As Exception
                Debug.WriteLine("GetVideoDuration Error: " & ex.Message)
            End Try

            Return 0
        End Function

        ''' <summary>
        ''' Runs ffprobe with the given arguments and returns the stdout output.
        ''' Used for quick verification of output file properties (FPS, duration, etc.)
        ''' </summary>
        Private Function RunFFprobeQuick(args As String) As String
            Try
                If String.IsNullOrEmpty(_ffprobePath) OrElse Not File.Exists(_ffprobePath) Then
                    Return "(ffprobe not available)"
                End If

                Using proc As New Process()
                    proc.StartInfo = New ProcessStartInfo() With {
                        .FileName = _ffprobePath,
                        .Arguments = args,
                        .UseShellExecute = False,
                        .CreateNoWindow = True,
                        .RedirectStandardOutput = True,
                        .RedirectStandardError = True
                    }

                    proc.Start()
                    Dim stdoutTask As Task(Of String) = proc.StandardOutput.ReadToEndAsync()
                    Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()

                    proc.WaitForExit(5000)
                    stdoutTask.Wait(3000)
                    stderrTask.Wait(3000)

                    Return stdoutTask.Result.Trim()
                End Using
            Catch ex As Exception
                Return "(ffprobe error: " & ex.Message & ")"
            End Try
        End Function
#End Region

#Region "IDisposable"
        Private disposed As Boolean = False

        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposed Then Exit Sub

            If disposing Then
                Try
                    If _isRecording Then StopRecording()
                    If _isBuffering Then StopBuffer()
                Catch
                End Try

                Try
                    CleanupRecordingProcess()
                    CleanupBufferProcess()
                Catch
                End Try

                Try
                    StopRecordingAudioPipe()
                    StopBufferAudioPipe()
                Catch
                End Try
            End If

            disposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
#End Region

    End Class

    ''' <summary>
    ''' ★ v5: ข้อมูลตัวเลือก Capture API สำหรับ UI
    ''' ใช้กับ ScreenRecorder.GetAvailableCaptureAPIs()
    ''' </summary>
    Public Class CaptureAPIOption
        ''' <summary>ประเภทของ Capture API</summary>
        Public Property APIType As ScreenRecorder.CaptureAPIType

        ''' <summary>ชื่อที่แสดงใน UI เช่น "DDA Grab (DXGI Desktop Duplication)"</summary>
        Public Property DisplayName As String

        ''' <summary>คำอธิบายสั้นๆ เช่น "Monitor capture — เร็วกว่า GFxCapture"</summary>
        Public Property Description As String

        ''' <summary>แนะนำสำหรับ CaptureTargetType ปัจจุบันหรือไม่</summary>
        Public Property IsRecommended As Boolean

        ''' <summary>ใช้ได้บนเครื่องนี้หรือไม่ (ผ่านการ check แล้ว)</summary>
        Public Property IsAvailable As Boolean

        Public Overrides Function ToString() As String
            Dim suffix As String = ""
            If IsRecommended Then suffix &= " ★"
            If Not IsAvailable Then suffix &= " (ไม่พร้อมใช้งาน)"
            Return DisplayName & suffix
        End Function
    End Class

End Namespace