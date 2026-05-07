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
        Public Const MIN_BITRATE As Integer = 500
        Public Const MAX_BITRATE As Integer = 300000
        Public Const DEFAULT_BITRATE As Integer = 8000

        Public Const MIN_FRAMERATE As Integer = 1
        Public Const MAX_FRAMERATE As Integer = 240
        Public Const DEFAULT_FRAMERATE As Integer = 60

        Public Const MIN_ENCODER_PRESET As Integer = 1
        Public Const MAX_ENCODER_PRESET As Integer = 7
        Public Const DEFAULT_ENCODER_PRESET As Integer = 4

        Public Const MIN_REPLAY_DURATION As Integer = 15
        Public Const MAX_REPLAY_DURATION As Integer = 1200
        Public Const DEFAULT_REPLAY_DURATION As Integer = 60

        Public Const FIXED_BUFFER_SEGMENTS As Integer = 2400
        Public Const FIXED_BUFFER_SECONDS As Integer = 1200

        Private Const GRACEFUL_EXIT_TIMEOUT As Integer = 10000
        Private Const FORCE_KILL_TIMEOUT As Integer = 2000
        Private Const FILE_WRITE_DELAY As Integer = 300
        Private Const CONCAT_TIMEOUT As Integer = 60000

        Private Const SEGMENT_DURATION As Double = 0.5
        Private Const BUFFER_MAX_SEGMENTS As Integer = 2400
        Private Const BUFFER_MAX_DURATION As Integer = 1200
        Private Const SB_CAPACITY_XLARGE As Integer = 4096
        Private Const CAPTURE_API_CHECK_TIMEOUT As Integer = 3000
        Private _bufferStartTime As DateTime
        Private _lastSegmentTime As DateTime

#End Region

#Region "Enums"
        Public Enum RecordingPreset
            Low
            Medium
            High
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
        Private Shared _gfxcaptureAvailable As Boolean = False
        Private Shared _gfxcaptureChecked As Boolean = False
        Private Shared _ddagrabAvailable As Boolean = False
        Private Shared _ddagrabChecked As Boolean = False
        Private Shared _apiLock As New Object()
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

        Public Shared Sub CheckGfxCaptureAvailabilityAsync(ffmpegPath As String)
            Task.Run(Sub()
                         Try
                             CheckGfxCaptureAvailability(ffmpegPath)
                         Catch ex As Exception
                             Debug.WriteLine("CheckGfxCaptureAvailabilityAsync Error: " & ex.Message)
                         End Try
                     End Sub)
        End Sub

        Public Shared Sub CheckDDAGrabAvailabilityAsync(ffmpegPath As String)
            Task.Run(Sub()
                         Try
                             CheckDDAGrabAvailability(ffmpegPath)
                         Catch ex As Exception
                             Debug.WriteLine("CheckDDAGrabAvailabilityAsync Error: " & ex.Message)
                         End Try
                     End Sub)
        End Sub

#End Region

#Region "Job Object for Process Cleanup"
        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)>
        Private Shared Function CreateJobObject(ByVal lpJobAttributes As IntPtr, ByVal lpName As String) As IntPtr
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function AssignProcessToJobObject(ByVal hJob As IntPtr, ByVal hProcess As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function SetInformationJobObject(ByVal hJob As IntPtr, ByVal JobObjectInfoClass As JOBOBJECTINFOCLASS, ByVal lpJobObjectInfo As JOBOBJECT_BASIC_LIMIT_INFORMATION, ByVal cbJobObjectInfoLength As UInteger) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function CloseHandle(ByVal hObject As IntPtr) As Boolean
        End Function

        Private Enum JOBOBJECTINFOCLASS
            BasicLimitInformation = 2
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Private Structure JOBOBJECT_BASIC_LIMIT_INFORMATION
            Public PerProcessUserTimeLimit As Long
            Public PerJobUserTimeLimit As Long
            Public LimitFlags As UInteger
            Public MinimumWorkingSetSize As IntPtr
            Public MaximumWorkingSetSize As IntPtr
            Public ActiveProcessLimit As UInteger
            Public Affinity As IntPtr
            Public PriorityClass As UInteger
            Public SchedulingClass As UInteger
        End Structure

        Private Const JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE As UInteger = &H2000

        Private Shared jobHandle As IntPtr = IntPtr.Zero
        Private Shared jobInitialized As Boolean = False
        Private Shared jobLock As New Object()
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
                Return Math.Min(GetActualBufferDuration().TotalSeconds, FIXED_BUFFER_SECONDS)
            End Get
        End Property

        Public ReadOnly Property BufferCapacitySeconds As Integer
            Get
                Return FIXED_BUFFER_SECONDS
            End Get
        End Property

        Public ReadOnly Property BufferCapacitySegments As Integer
            Get
                Return FIXED_BUFFER_SEGMENTS
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
            InitializeJobObject()
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
        Public Shared Sub CheckGfxCaptureAvailability(ffmpegPath As String)
            SyncLock _apiLock
                If _gfxcaptureChecked Then Exit Sub

                Try
                    Debug.WriteLine("═══ CheckGfxCaptureAvailability START ═══")
                    Dim testArgs As String = "-filter_complex ""gfxcapture=monitor_idx=0:max_framerate=1:capture_cursor=0,hwdownload,format=bgra"" -t 0.1 -f null - -hide_banner -loglevel error"

                    Using proc As New Process()
                        proc.StartInfo = CreateProcessStartInfo(ffmpegPath, testArgs)
                        proc.Start()

                        Dim exited As Boolean = proc.WaitForExit(CAPTURE_API_CHECK_TIMEOUT)

                        If Not exited Then
                            Try
                                proc.Kill()
                                proc.WaitForExit(1000)
                            Catch
                            End Try
                            _gfxcaptureAvailable = False
                        Else
                            _gfxcaptureAvailable = (proc.ExitCode = 0)
                        End If
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("CheckGfxCaptureAvailability Error: " & ex.Message)
                    _gfxcaptureAvailable = False
                Finally
                    _gfxcaptureChecked = True
                End Try
            End SyncLock
        End Sub

        Public Shared Sub CheckDDAGrabAvailability(ffmpegPath As String)
            SyncLock _apiLock
                If _ddagrabChecked Then Exit Sub

                Try
                    Debug.WriteLine("═══ CheckDDAGrabAvailability START ═══")
                    Dim testArgs As String = "-f lavfi -i ""ddagrab=0:framerate=1:draw_mouse=0"" -t 0.1 -f null - -hide_banner -loglevel error"

                    Using proc As New Process()
                        proc.StartInfo = CreateProcessStartInfo(ffmpegPath, testArgs)
                        proc.Start()

                        Dim exited As Boolean = proc.WaitForExit(CAPTURE_API_CHECK_TIMEOUT)

                        If Not exited Then
                            Try
                                proc.Kill()
                                proc.WaitForExit(1000)
                            Catch
                            End Try
                            _ddagrabAvailable = False
                        Else
                            _ddagrabAvailable = (proc.ExitCode = 0)
                        End If
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("CheckDDAGrabAvailability Error: " & ex.Message)
                    _ddagrabAvailable = False
                Finally
                    _ddagrabChecked = True
                End Try
            End SyncLock
        End Sub

        Public Shared ReadOnly Property IsGfxCaptureAvailable As Boolean
            Get
                Return _gfxcaptureAvailable
            End Get
        End Property

        Public Shared ReadOnly Property IsDDAGrabAvailable As Boolean
            Get
                Return _ddagrabAvailable
            End Get
        End Property

        Public Shared Sub ResetAPIChecks()
            SyncLock _apiLock
                _gfxcaptureChecked = False
                _gfxcaptureAvailable = False
                _ddagrabChecked = False
                _ddagrabAvailable = False
            End SyncLock
        End Sub
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
                    Task.Run(Sub() PreWarmEncoderCore(ffmpegPath, preferredEncoder))
                    _isPreWarmed = True
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
                Case VideoEncoder.QuickSync_H264, VideoEncoder.QuickSync_HEVC
                    Return "-c:v h264_qsv"
                Case VideoEncoder.AMF_H264, VideoEncoder.AMF_HEVC
                    Return "-c:v h264_amf"
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
        Private Shared Sub InitializeJobObject()
            SyncLock jobLock
                If jobInitialized Then Exit Sub

                Try
                    jobHandle = CreateJobObject(IntPtr.Zero, Nothing)
                    If jobHandle = IntPtr.Zero Then Exit Sub

                    Dim info As New JOBOBJECT_BASIC_LIMIT_INFORMATION()
                    info.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE

                    SetInformationJobObject(
                        jobHandle,
                        JOBOBJECTINFOCLASS.BasicLimitInformation,
                        info,
                        CUInt(Marshal.SizeOf(GetType(JOBOBJECT_BASIC_LIMIT_INFORMATION)))
                    )

                    jobInitialized = True
                Catch ex As Exception
                    Debug.WriteLine("Job Object init error: " & ex.Message)
                End Try
            End SyncLock
        End Sub

        Private Sub AddProcessToJob(proc As Process)
            If proc Is Nothing Then Exit Sub
            InitializeJobObject()

            SyncLock jobLock
                If jobHandle <> IntPtr.Zero Then
                    Try
                        AssignProcessToJobObject(jobHandle, proc.Handle)
                    Catch ex As Exception
                        Debug.WriteLine("AddProcessToJob error: " & ex.Message)
                    End Try
                End If
            End SyncLock
        End Sub
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
                If _isRecording Then
                    RaiseEvent RecordingError(Me, "Recording is already in progress")
                    Return False
                End If

                If Not ValidateFFmpeg() Then Return False

                ResetCaptureAPIFallback()

                Try
                    Dim dir As String = Path.GetDirectoryName(outputFilePath)
                    If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If

                    recordingOutputPath = outputFilePath

                    StartRecordingAudioPipe()

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
                If Not _isRecording OrElse recordingProcess Is Nothing Then Exit Sub

                Dim savedPath As String = recordingOutputPath

                Try
                    ' ===== 1. ส่ง 'q' ให้ FFmpeg =====
                    recordingProcess.StandardInput.Write("q"c)
                    recordingProcess.StandardInput.Flush()
                    recordingProcess.StandardInput.Close()

                    ' ===== 2. รอให้ FFmpeg exit =====
                    If Not recordingProcess.WaitForExit(10000) Then
                        Debug.WriteLine("StopRecording: FFmpeg timeout")
                        recordingProcess.Kill()
                        recordingProcess.WaitForExit(1000)
                    End If

                    Debug.WriteLine("StopRecording: FFmpeg exited")

                Catch ex As Exception
                    Debug.WriteLine("StopRecording Error: " & ex.Message)
                    ForceKillProcess(recordingProcess)
                End Try

                ' ===== 3. FFmpeg exit แล้ว = AudioPipe broken อัตโนมัติ =====
                ' เรียก Stop เพื่อ cleanup (จะไม่ส่ง silent frame เพิ่ม)
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
                If _isBuffering Then
                    Debug.WriteLine("StartBuffer: Already buffering")
                    Return False
                End If

                If Not ValidateFFmpeg() Then Return False

                ResetCaptureAPIFallback()

                ' ===== Create temp directory =====
                bufferTempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", "replay_buffer_" & Process.GetCurrentProcess().Id)

                Try
                    If Directory.Exists(bufferTempDir) Then
                        ' Clean old segments
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
                    ' Fallback to temp folder
                    bufferTempDir = Path.Combine(Path.GetTempPath(), "ShadowPlay_Buffer_" & Process.GetCurrentProcess().Id)
                    Directory.CreateDirectory(bufferTempDir)
                End Try

                _segmentTimestamps.Clear()
                _bufferStartTime = DateTime.Now
                _lastSegmentTime = DateTime.Now

                Try
                    ' ===== Start AudioPipe =====
                    StartBufferAudioPipe()

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
                If Not _isBuffering OrElse bufferProcess Is Nothing Then
                    Debug.WriteLine("StopBuffer: Not buffering or process is null")
                    Exit Sub
                End If

                Debug.WriteLine("═══ StopBuffer START ═══")

                Try
                    ' ===== 1. ส่ง 'q' ให้ FFmpeg (graceful exit) =====
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

                    ' ===== 2. รอให้ FFmpeg exit (สำคัญมาก!) =====
                    If bufferProcess IsNot Nothing AndAlso Not bufferProcess.HasExited Then
                        Dim exited As Boolean = bufferProcess.WaitForExit(GRACEFUL_EXIT_TIMEOUT)

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

                ' ===== 3. FFmpeg exit แล้ว ค่อยปิด AudioPipe =====
                StopBufferAudioPipe()

                ' ===== 4. Cleanup =====
                CleanupBufferProcess()
                _isBuffering = False

                ' ===== 5. Small delay for file system =====
                Threading.Thread.Sleep(FILE_WRITE_DELAY)

                ' ===== 6. Delete temp segments (optional) =====
                Try
                    If Directory.Exists(bufferTempDir) Then
                        For Each f In Directory.GetFiles(bufferTempDir)
                            Try
                                File.Delete(f)
                            Catch
                            End Try
                        Next
                    End If
                Catch
                End Try

                _segmentTimestamps.Clear()

                RaiseEvent ReplayBufferStopped(Me, EventArgs.Empty)
                RaiseEvent BufferStopped(Me, EventArgs.Empty)

                Debug.WriteLine("═══ StopBuffer END ═══")
            End SyncLock
        End Sub

        Public Function SaveReplay(outputPath As String, saveDuration As Integer) As Boolean
            SyncLock _saveLock
                If _isSaving Then
                    Debug.WriteLine("SaveReplay: Already saving")
                    Return False
                End If
                _isSaving = True
            End SyncLock

            Try
                If Not _isBuffering Then
                    Debug.WriteLine("SaveReplay: Not buffering")
                    Return False
                End If

                ' ===== 1. Wait for file system =====
                Threading.Thread.Sleep(FILE_WRITE_DELAY)

                ' ===== 2. Get available segments =====
                Dim segments As New List(Of TimestampedSegment)()
                SyncLock _bufferLock
                    segments = GetTimestampedSegments()
                End SyncLock

                If segments.Count = 0 Then
                    Debug.WriteLine("SaveReplay: No segments found")
                    Return False
                End If

                ' Sort by segment number
                segments = segments.OrderBy(Function(s) s.SegmentNumber).ToList()

                Debug.WriteLine(String.Format("SaveReplay: Found {0} segments", segments.Count))

                ' ===== 3. Calculate segments needed =====
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

                Debug.WriteLine(String.Format("SaveReplay: Using {0} segments ({1:F1}s)",
            selectedSegments.Count, selectedSegments.Count * SEGMENT_DURATION))

                ' ===== 4. Prepare output =====
                Try
                    Dim outputDir As String = Path.GetDirectoryName(outputPath)
                    If Not String.IsNullOrEmpty(outputDir) AndAlso Not Directory.Exists(outputDir) Then
                        Directory.CreateDirectory(outputDir)
                    End If

                    ' ===== 5. Create concat list =====
                    Dim concatListPath As String = Path.Combine(bufferTempDir, "concat_list.txt")

                    Using writer As New StreamWriter(concatListPath)
                        For Each seg In selectedSegments
                            If File.Exists(seg.FilePath) Then
                                Dim fileInfo As New FileInfo(seg.FilePath)
                                If fileInfo.Length > 0 Then
                                    Dim escapedPath As String = seg.FilePath.Replace("\"c, "/"c)
                                    writer.WriteLine(String.Format("file '{0}'", escapedPath))
                                End If
                            End If
                        Next
                    End Using

                    ' ===== 6. Concat segments =====
                    Dim concatArgs As String = String.Format(
                "-y -hide_banner -loglevel warning -f concat -safe 0 -i ""{0}"" -c copy -movflags +faststart ""{1}""",
                concatListPath, outputPath)

                    Dim success As Boolean = RunFFmpegSync(concatArgs, 60000)

                    ' ===== 7. Verify output =====
                    If success AndAlso File.Exists(outputPath) Then
                        Dim fileInfo As New FileInfo(outputPath)
                        If fileInfo.Length > 0 Then
                            Debug.WriteLine(String.Format("SaveReplay: Success - {0} bytes", fileInfo.Length))
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

        Private Function GetTimestampedSegments() As List(Of TimestampedSegment)
            Dim result As New List(Of TimestampedSegment)()

            If Not Directory.Exists(bufferTempDir) Then Return result

            Try
                Dim files = Directory.GetFiles(bufferTempDir, "segment_*.mkv")

                For Each f In files
                    Try
                        Dim fileName = Path.GetFileNameWithoutExtension(f)
                        Dim match = RegexSegment.Match(fileName)

                        If match.Success Then
                            Dim segNumber As Integer = Integer.Parse(match.Groups(1).Value)
                            result.Add(New TimestampedSegment With {
                                .FilePath = f,
                                .SegmentNumber = segNumber
                            })
                        End If
                    Catch
                    End Try
                Next

            Catch ex As Exception
                Debug.WriteLine("GetTimestampedSegments Error: " & ex.Message)
            End Try

            Return result
        End Function

        Private Class TimestampedSegment
            Public Property FilePath As String
            Public Property SegmentNumber As Integer
        End Class
#End Region

#Region "FFmpeg Process Management"
        Private Sub StopProcessGracefully(proc As Process, timeoutMs As Integer)
            If proc Is Nothing OrElse proc.HasExited Then Exit Sub

            Try
                ' ===== ส่ง 'q' เพื่อให้ FFmpeg ปิดไฟล์อย่างถูกต้อง =====
                proc.StandardInput.Write("q"c)
                proc.StandardInput.Flush()
                proc.StandardInput.Close()  ' ✅ ปิด stdin

                ' ===== รอให้ FFmpeg exit =====
                If Not proc.WaitForExit(timeoutMs) Then
                    Debug.WriteLine("StopProcessGracefully: Timeout, force killing...")
                    proc.Kill()
                    proc.WaitForExit(1000)
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
                    Dim stderr As String = proc.StandardError.ReadToEnd()

                    Dim exited As Boolean = proc.WaitForExit(timeoutMs)

                    If Not exited Then
                        Try
                            proc.Kill()
                            proc.WaitForExit(1000)
                        Catch
                        End Try
                        Return False
                    End If

                    Return proc.ExitCode = 0
                End Using
            Catch ex As Exception
                Debug.WriteLine("RunFFmpegSync Error: " & ex.Message)
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

        Private Function BuildFFmpegArguments(outputFile As String, region As Rectangle) As String
            Dim sb As New StringBuilder(SB_CAPACITY_XLARGE)

            sb.Append("-y -hide_banner -loglevel warning ")

            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
            Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)
            Dim selectedAPI As CaptureAPIType = DetermineBestCaptureAPI()

            ' ═══════════════════════════════════════════════════════════════════════
            ' Hardware Device Initialization
            ' 
            ' 🔧 FIX for QSV: DO NOT create qsv device separately!
            '    hwmap=derive_device=qsv will create QSV context from D3D11VA
            ' 
            ' ✅ Correct (from FFmpeg docs):
            '    ffmpeg -init_hw_device d3d11va:,vendor_id=0x8086 
            '           -filter_complex ddagrab=0,hwmap=derive_device=qsv,format=qsv 
            '           -c:v h264_qsv -global_quality 20 output.mkv
            ' ═══════════════════════════════════════════════════════════════════════
            If isQuickSync Then
                ' QSV: Only init d3d11va - hwmap will derive qsv from it
                sb.Append("-init_hw_device d3d11va:,vendor_id=0x8086 ")
            ElseIf selectedAPI = CaptureAPIType.DDAGrab OrElse selectedAPI = CaptureAPIType.GFxCapture Then
                sb.Append("-init_hw_device d3d11va ")
            End If

            _pendingVideoFilter = ""

            BuildCaptureCommand(sb, region)

            Dim audioInputArgs As String = BuildAudioInputArgs(useBufferPipe:=False)
            sb.Append(audioInputArgs)

            If Not String.IsNullOrEmpty(_pendingVideoFilter) Then
                sb.Append(_pendingVideoFilter)
            End If

            BuildAudioMapCommand(sb, useBufferPipe:=False)

            BuildEncoderCommand(sb)

            If _audioMode <> VideoCaptureMode.None Then
                sb.Append("-c:a aac -b:a 192k -async 1 ")
            End If

            sb.Append("-movflags +faststart """)
            sb.Append(outputFile)
            sb.Append(""""c)
            Return sb.ToString()
        End Function

        Private Function BuildBufferFFmpegArguments() As String
            Dim sb As New StringBuilder(SB_CAPACITY_XLARGE)

            sb.Append("-y -hide_banner -loglevel warning ")

            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim selectedAPI As CaptureAPIType = DetermineBestCaptureAPI()

            ' Hardware device initialization
            If isQuickSync Then
                sb.Append("-init_hw_device d3d11va:,vendor_id=0x8086 ")
            ElseIf selectedAPI = CaptureAPIType.DDAGrab OrElse selectedAPI = CaptureAPIType.GFxCapture Then
                sb.Append("-init_hw_device d3d11va ")
            End If

            _pendingVideoFilter = ""

            ' Build capture command
            BuildCaptureCommand(sb, Rectangle.Empty)

            ' Audio input
            Dim audioInputArgs As String = BuildAudioInputArgs(useBufferPipe:=True)
            sb.Append(audioInputArgs)

            If Not String.IsNullOrEmpty(_pendingVideoFilter) Then
                sb.Append(_pendingVideoFilter)
            End If

            ' Audio map
            BuildAudioMapCommand(sb, useBufferPipe:=True)

            ' Encoder
            BuildBufferEncoderCommand(sb)

            ' Audio codec
            If _audioMode <> VideoCaptureMode.None Then
                sb.Append("-c:a aac -b:a 192k -async 1 ")
            End If

            ' ═══════════════════════════════════════════════════════════════════════
            ' ✅ Segment settings for Replay Buffer
            ' ═══════════════════════════════════════════════════════════════════════
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

        Private Sub BuildAudioMapCommand(sb As StringBuilder, useBufferPipe As Boolean)
            Dim usesFilterComplex As Boolean = UsesFilterComplexForVideo()

            If usesFilterComplex Then
                sb.Append("-map ""[v]"" ")
            Else
                sb.Append("-map 0:v ")
            End If

            If _audioMode = VideoCaptureMode.None Then Exit Sub

            Dim hasSystemAudio As Boolean = (_audioMode = VideoCaptureMode.SystemOnly OrElse _audioMode = VideoCaptureMode.Both)
            Dim hasMic As Boolean = (_audioMode = VideoCaptureMode.MicOnly OrElse _audioMode = VideoCaptureMode.Both)

            If hasSystemAudio Then
                Dim pipe As AudioPipe = If(useBufferPipe, _bufferAudioPipe, _recordingAudioPipe)
                If pipe Is Nothing OrElse Not pipe.IsRunning Then
                    hasSystemAudio = False
                End If
            End If

            If Not hasSystemAudio AndAlso Not hasMic Then Exit Sub

            Dim audioBaseIdx As Integer = If(usesFilterComplex, 0, 1)

            If hasSystemAudio AndAlso hasMic Then
                Dim sysIdx As Integer = audioBaseIdx
                Dim micIdx As Integer = audioBaseIdx + 1

                sb.Append("-filter_complex """)
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
                sb.Append("[sys][mic]amix=inputs=2:duration=longest[aout]"" ")

                sb.Append("-map ""[aout]"" ")

            ElseIf hasSystemAudio Then
                sb.Append("-map ")
                sb.Append(audioBaseIdx)
                sb.Append(":a ")

                If _systemAudioVolume < 0.99F Then
                    sb.Append("-af ""volume=")
                    sb.Append(_systemAudioVolume.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(""" ")
                End If

            ElseIf hasMic Then
                sb.Append("-map ")
                sb.Append(audioBaseIdx)
                sb.Append(":a ")

                If _micVolume < 0.99F Then
                    sb.Append("-af ""volume=")
                    sb.Append(_micVolume.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(""" ")
                End If
            End If
        End Sub

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

        Private Function DetermineBestCaptureAPI() As CaptureAPIType
            If _captureAPI <> CaptureAPIType.Auto Then
                Return _captureAPI
            End If

            Dim isHardwareEncoder As Boolean = (
                _encoder = VideoEncoder.NVENC_H264 OrElse
                _encoder = VideoEncoder.NVENC_HEVC OrElse
                _encoder = VideoEncoder.NVENC_AV1 OrElse
                _encoder = VideoEncoder.QuickSync_H264 OrElse
                _encoder = VideoEncoder.QuickSync_HEVC OrElse
                _encoder = VideoEncoder.AMF_H264 OrElse
                _encoder = VideoEncoder.AMF_HEVC
            )

            If isHardwareEncoder Then
                Debug.WriteLine("DetermineBestCaptureAPI: Hardware Encoder -> DDAGrab")
                Return CaptureAPIType.DDAGrab
            End If

            Debug.WriteLine("DetermineBestCaptureAPI: Software -> GDIGrab")
            Return CaptureAPIType.GDIGrab
        End Function

        Private Sub ResetCaptureAPIFallback()
            _captureAPIFailed = False
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

        Private Sub BuildGfxCaptureCommand(sb As StringBuilder, region As Rectangle)
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
                BuildGfxCaptureDirectPath(sb)
            Else
                BuildGfxCaptureStandardPath(sb, needScaling)
            End If
        End Sub

        Private Sub BuildGfxCaptureDirectPath(sb As StringBuilder)
            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
            Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)
            Dim screenDims = GetCachedScreenDimensions()
            Dim needScaling As Boolean = (_resolutionWidth > 0 AndAlso _resolutionHeight > 0 AndAlso
                                          (_resolutionWidth <> screenDims.Width OrElse _resolutionHeight <> screenDims.Height))

            sb.Append("-filter_complex """)
            BuildGfxCaptureOptions(sb)

            If isNVIDIA OrElse isAMD Then
                sb.Append(",fps=")
                sb.Append(_framerate.ToString())
            End If

            If isQuickSync Then
                sb.Append(",hwmap=derive_device=qsv")
                If needScaling Then
                    sb.Append(",hwdownload,format=nv12,scale=")
                    sb.Append(_resolutionWidth.ToString())
                    sb.Append(":")
                    sb.Append(_resolutionHeight.ToString())
                    sb.Append(":flags=lanczos,hwupload")
                End If
            End If

            sb.Append("[v]"" ")
        End Sub

        Private Sub BuildGfxCaptureStandardPath(sb As StringBuilder, needScaling As Boolean)
            sb.Append("-filter_complex """)
            BuildGfxCaptureOptions(sb)
            sb.Append(",hwdownload,format=")
            sb.Append(GetOutputFormatString())

            If needScaling OrElse (_outputFormat <> OutputColorFormat.Auto AndAlso _outputFormat <> OutputColorFormat.BGRA_8Bit) Then
                BuildScalingAndFormatFilters(sb, needScaling)
            End If

            sb.Append("[v]"" ")
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

            If isQuickSync Then
                options.Add("output_fmt=8bit")
            Else
                options.Add(String.Format("output_fmt={0}", GetGfxCaptureOutputFormatString()))
            End If

            sb.Append(String.Join(":", options))
        End Sub

        Private Sub BuildDDAGrabCommand(sb As StringBuilder, region As Rectangle)
            Dim screenDims = GetCachedScreenDimensions()
            Dim needScaling As Boolean = (_resolutionWidth > 0 AndAlso _resolutionHeight > 0 AndAlso
                                          (_resolutionWidth <> screenDims.Width OrElse _resolutionHeight <> screenDims.Height))
            Dim isQuickSync As Boolean = (_encoder = VideoEncoder.QuickSync_H264 OrElse _encoder = VideoEncoder.QuickSync_HEVC)
            Dim isNVIDIA As Boolean = (_encoder = VideoEncoder.NVENC_H264 OrElse _encoder = VideoEncoder.NVENC_HEVC OrElse _encoder = VideoEncoder.NVENC_AV1)
            Dim isAMD As Boolean = (_encoder = VideoEncoder.AMF_H264 OrElse _encoder = VideoEncoder.AMF_HEVC)

            If isQuickSync Then
                ' ═══════════════════════════════════════════════════════════════════════
                ' ✅ QSV Zero-Copy Path (per FFmpeg docs - VERIFIED WORKING)
                '    
                '    ffmpeg -init_hw_device d3d11va:,vendor_id=0x8086 
                '           -filter_complex "ddagrab=0,hwmap=derive_device=qsv,format=qsv" 
                '           -c:v h264_qsv -global_quality 20 output.mkv
                '
                '    IMPORTANT: Do NOT create qsv device separately!
                '               hwmap=derive_device=qsv creates QSV context from D3D11VA
                ' ═══════════════════════════════════════════════════════════════════════
                sb.Append("-filter_complex ""ddagrab=")
                sb.Append(_monitorIndex.ToString())
                sb.Append(":framerate=")
                sb.Append(_framerate.ToString())
                sb.Append(":draw_mouse=")
                sb.Append(If(_captureCursor, "1", "0"))

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

                ' ✅ hwmap=derive_device=qsv creates QSV context from D3D11VA
                sb.Append(",hwmap=derive_device=qsv,format=qsv[v]"" ")

            ElseIf isNVIDIA OrElse isAMD Then
                sb.Append("-filter_complex ""ddagrab=")
                sb.Append(_monitorIndex.ToString())
                sb.Append(":framerate=")
                sb.Append(_framerate.ToString())
                sb.Append(":draw_mouse=")
                sb.Append(If(_captureCursor, "1", "0"))

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

                sb.Append(",fps=")
                sb.Append(_framerate.ToString())
                sb.Append("[v]"" ")

                _pendingVideoFilter = ""
            Else
                sb.Append("-filter_complex ""ddagrab=")
                sb.Append(_monitorIndex.ToString())
                sb.Append(":framerate=")
                sb.Append(_framerate.ToString())
                sb.Append(":draw_mouse=")
                sb.Append(If(_captureCursor, "1", "0"))

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

                sb.Append(",hwdownload,format=bgra[v]"" ")
            End If
        End Sub

        Private _pendingVideoFilter As String = ""

        Private Sub BuildGDIGrabCommand(sb As StringBuilder, region As Rectangle)
            sb.Append("-f gdigrab ")
            sb.Append("-framerate ")
            sb.Append(_framerate.ToString())
            sb.Append(" ")
            sb.Append("-draw_mouse ")
            sb.Append(If(_captureCursor, "1", "0"))
            sb.Append(" -i desktop ")

            _pendingVideoFilter = "-vf ""scale=" & _resolutionWidth.ToString() & ":" & _resolutionHeight.ToString() & ":flags=lanczos,format=yuv420p"" "
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
            Dim gopSize As Integer = _framerate * 2

            Select Case _encoder
                Case VideoEncoder.NVENC_H264
                    sb.Append("-c:v h264_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ll -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_HEVC
                    sb.Append("-c:v hevc_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ll -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_AV1
                    sb.Append("-c:v av1_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ll -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.QuickSync_H264
                    sb.Append("-c:v h264_qsv ")
                    BuildQSVRateControl(sb)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.QuickSync_HEVC
                    sb.Append("-c:v hevc_qsv ")
                    BuildQSVRateControl(sb, isHEVC:=True)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.AMF_H264
                    sb.Append("-c:v h264_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildAMFRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.AMF_HEVC
                    sb.Append("-c:v hevc_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildAMFRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

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
                    sb.Append(" -tune ll -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_HEVC
                    sb.Append("-c:v hevc_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ll -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_AV1
                    sb.Append("-c:v av1_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ll -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    BuildNVENCRateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.QuickSync_H264
                    sb.Append("-c:v h264_qsv ")
                    BuildQSVRateControl(sb)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.QuickSync_HEVC
                    sb.Append("-c:v hevc_qsv ")
                    BuildQSVRateControl(sb, isHEVC:=True)
                    sb.Append("-g ")
                    sb.Append(gopSize)
                    sb.Append(" -fps_mode cfr ")

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
                    sb.Append(" -fps_mode cfr ")

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
                    sb.Append(" -fps_mode cfr ")

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
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.LibX265
                    sb.Append("-c:v libx265 -preset ")
                    sb.Append(GetX264PresetString())
                    sb.Append(" -tune zerolatency -profile:v main -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    BuildX265RateControl(sb)
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")
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
                    sb.Append(" -fps_mode cfr ")

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
                    sb.Append(" -fps_mode cfr ")
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
            sb.Append("-b:v ")
            sb.Append(_bitrate)
            sb.Append("k -maxrate ")
            sb.Append(_bitrate * 2)
            sb.Append("k ")
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
                Case Else : Return "veryfast"
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

End Namespace
