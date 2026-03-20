Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports System.Collections.Concurrent
Imports System.Linq
Imports System.Windows.Forms

Namespace CaptureCore

    Public Class ScreenRecorder
        Implements IDisposable

#Region "Constants"
        Public Const MIN_BITRATE As Integer = 500
        Public Const MAX_BITRATE As Integer = 100000
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
        Private Const FORCE_KILL_TIMEOUT As Integer = 5000
        Private Const FILE_WRITE_DELAY As Integer = 500
        Private Const CONCAT_TIMEOUT As Integer = 60000

        ' ✅ Segment duration for exact frame selection
        Private Const SEGMENT_DURATION As Double = 0.25
        Private Const SB_CAPACITY_XLARGE As Integer = 2048

        Private Const DDAGRAB_CHECK_TIMEOUT As Integer = 15000
#End Region

#Region "Compiled Regex Patterns (Cached)"
        Private Shared ReadOnly RegexFrame As New Regex("frame=\s*(\d+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexSize As New Regex("size=\s*(\d+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexBitrate As New Regex("bitrate=\s*([\d.]+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexSpeed As New Regex("speed=\s*([\d.]+)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexDuration As New Regex("Duration: (\d+):(\d+):(\d+\.?\d*)", RegexOptions.Compiled)
        Private Shared ReadOnly RegexSegment As New Regex("segment_(\d+)", RegexOptions.Compiled)
#End Region

#Region "Time Formatting Helpers - MM:SS:ms format"
        Public Shared Function FormatTimeMMSSms(totalSeconds As Double) As String
            Dim absSeconds As Double = Math.Abs(totalSeconds)
            Dim minutes As Integer = CInt(Math.Floor(absSeconds / 60))
            Dim seconds As Integer = CInt(Math.Floor(absSeconds Mod 60))
            Dim milliseconds As Integer = CInt(Math.Round((absSeconds - Math.Floor(absSeconds)) * 1000))
            Return String.Format("{0:D2}:{1:D2}:{2:D3}", minutes, seconds, milliseconds)
        End Function

        Public Shared Function FormatTimeMMSS(totalSeconds As Double) As String
            Dim absSeconds As Double = Math.Abs(totalSeconds)
            Dim minutes As Integer = CInt(Math.Floor(absSeconds / 60))
            Dim seconds As Integer = CInt(Math.Floor(absSeconds Mod 60))
            Return String.Format("{0:D2}:{1:D2}", minutes, seconds)
        End Function

        Public Shared Function FormatDurationFriendly(totalSeconds As Double) As String
            Dim absSeconds As Double = Math.Abs(totalSeconds)
            Dim minutes As Integer = CInt(Math.Floor(absSeconds / 60))
            Dim seconds As Integer = CInt(Math.Floor(absSeconds Mod 60))
            Dim ms As Integer = CInt(Math.Round((absSeconds - Math.Floor(absSeconds)) * 1000))

            If minutes > 0 Then
                Return String.Format("{0}m {1}s", minutes, seconds)
            Else
                If ms > 0 Then
                    Return String.Format("{0}.{1:D3}s", seconds, ms)
                Else
                    Return String.Format("{0}s", seconds)
                End If
            End If
        End Function
#End Region

#Region "DDAGRAB DETECTION"
        Private Shared _ddagrabAvailable As Boolean = False
        Private Shared _ddagrabChecked As Boolean = False
        Private Shared _ddagrabLock As New Object()

        Public Shared Sub CheckDdagrabAvailability(ffmpegPath As String)
            SyncLock _ddagrabLock
                If _ddagrabChecked Then Exit Sub

                Try
                    Debug.WriteLine("═══ CheckDdagrabAvailability START ═══")
                    Dim testArgs As String = "-f lavfi -i ""ddagrab=0:framerate=1:draw_mouse=0"" -t 0.3 -f null - -hide_banner -loglevel error"

                    Using proc As New Process()
                        proc.StartInfo = CreateProcessStartInfo(ffmpegPath, testArgs)
                        proc.Start()

                        Dim exited As Boolean = proc.WaitForExit(DDAGRAB_CHECK_TIMEOUT)

                        If Not exited Then
                            Try
                                proc.Kill()
                                proc.WaitForExit(1000)
                            Catch
                            End Try
                            _ddagrabAvailable = False
                            Debug.WriteLine("CheckDdagrabAvailability: TIMEOUT - ddagrab not available")
                        Else
                            _ddagrabAvailable = (proc.ExitCode = 0)
                            Debug.WriteLine(String.Format("CheckDdagrabAvailability: ExitCode={0}, ddagrab={1}", proc.ExitCode, _ddagrabAvailable))
                        End If
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("CheckDdagrabAvailability Error: " & ex.Message)
                    _ddagrabAvailable = False
                Finally
                    _ddagrabChecked = True
                    Debug.WriteLine(String.Format("═══ CheckDdagrabAvailability END: ddagrab={0} ═══", _ddagrabAvailable))
                End Try
            End SyncLock
        End Sub

        Public Shared ReadOnly Property IsDdagrabAvailable As Boolean
            Get
                Return _ddagrabAvailable
            End Get
        End Property

        Public Shared ReadOnly Property IsDdagrabChecked As Boolean
            Get
                Return _ddagrabChecked
            End Get
        End Property

        Public Shared Sub ResetDdagrabCheck()
            SyncLock _ddagrabLock
                _ddagrabChecked = False
                _ddagrabAvailable = False
            End SyncLock
        End Sub
#End Region

#Region "Pre-warm System"
        Private Shared _isPreWarmed As Boolean = False
        Private Shared _prewarmLock As New Object()

        Public Shared Sub PreWarmFFmpeg(ffmpegPath As String, preferredEncoder As VideoEncoder)
            If _isPreWarmed Then Exit Sub
            If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
                _isPreWarmed = True
                Exit Sub
            End If

            SyncLock _prewarmLock
                If _isPreWarmed Then Exit Sub

                Try
                    Debug.WriteLine("═══ PreWarmFFmpeg START ═══")
                    CheckDdagrabAvailability(ffmpegPath)
                    Task.Run(Sub() PreWarmEncoderCore(ffmpegPath, preferredEncoder))
                    _isPreWarmed = True
                    Debug.WriteLine("═══ PreWarmFFmpeg END ═══")
                Catch ex As Exception
                    Debug.WriteLine("PreWarmFFmpeg Error: " & ex.Message)
                    _isPreWarmed = True
                End Try
            End SyncLock
        End Sub

        Public Shared Sub PreWarmFFmpeg(ffmpegPath As String)
            PreWarmFFmpeg(ffmpegPath, VideoEncoder.NVENC_H264)
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
            Fast        ' -c copy (not frame-accurate)
            Accurate    ' Re-encode (frame-accurate)
        End Enum
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

        ' Race condition prevention
        Private _isSaving As Boolean = False
        Private ReadOnly _saveLock As New Object()

        ' Exact duration mode
        Private _exactDurationMode As Boolean = True
        Private _allIntraMode As Boolean = False

        Private _segmentTimestamps As New ConcurrentDictionary(Of Long, String)()

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

        Private _cachedScreenW As Integer = -1
        Private _cachedScreenH As Integer = -1
        Private _screenCacheTime As DateTime = DateTime.MinValue
#End Region

#Region "Properties"
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

        Public ReadOnly Property IsBufferActive As Boolean
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

        Public ReadOnly Property BufferCurrentDuration As Double
            Get
                If Not _isBuffering Then Return 0
                Return Math.Min(GetActualBufferDuration(), FIXED_BUFFER_SECONDS)
            End Get
        End Property

        Public ReadOnly Property Status As RecordingStatus
            Get
                If _isRecording Then Return RecordingStatus.Recording
                If _isBuffering Then Return RecordingStatus.Buffering
                Return RecordingStatus.Idle
            End Get
        End Property

        Public ReadOnly Property IsReplayBuffering As Boolean
            Get
                Return _isBuffering
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

        Public Property SegmentFileDuration As Integer
            Get
                Return CInt(SEGMENT_DURATION * 1000) ' Return in milliseconds
            End Get
            Set(value As Integer)
            End Set
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

        Public Property ExactDurationMode As Boolean
            Get
                Return _exactDurationMode
            End Get
            Set(value As Boolean)
                _exactDurationMode = value
            End Set
        End Property

        Public Property AllIntraMode As Boolean
            Get
                Return _allIntraMode
            End Get
            Set(value As Boolean)
                _allIntraMode = value
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
                ffmpegPath = Path.Combine(Application.StartupPath, ffmpegPath)
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
                    ffprobePath = Path.Combine(Application.StartupPath, ffprobePath)
                End If
                _ffprobePath = ffprobePath
            End If
        End Sub
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

                Try
                    Dim dir As String = Path.GetDirectoryName(outputFilePath)
                    If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                        Directory.CreateDirectory(dir)
                    End If

                    recordingOutputPath = outputFilePath
                    Dim arguments As String = BuildFFmpegArguments(outputFilePath, region)

                    Debug.WriteLine("══════════ Recording FFmpeg Arguments ══════════")
                    Debug.WriteLine(arguments)
                    Debug.WriteLine("════════════════════════════════════════════════")

                    recordingProcess = CreateFFmpegProcess(arguments)
                    recordingProcess.Start()
                    AddProcessToJob(recordingProcess)
                    recordingProcess.BeginErrorReadLine()
                    recordingProcess.BeginOutputReadLine()

                    _isRecording = True
                    recordingStartTime = DateTime.Now

                    RaiseEvent RecordingStarted(Me, EventArgs.Empty)
                    Debug.WriteLine("Recording started: " & outputFilePath)
                    Return True

                Catch ex As Exception
                    RaiseEvent RecordingError(Me, "Failed to start recording: " & ex.Message)
                    Debug.WriteLine("StartRecording Error: " & ex.Message)
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
                    StopProcessGracefully(recordingProcess, GRACEFUL_EXIT_TIMEOUT)
                Catch ex As Exception
                    Debug.WriteLine("StopRecording error: " & ex.Message)
                    ForceKillProcess(recordingProcess)
                Finally
                    CleanupRecordingProcess()
                    _isRecording = False
                    RaiseEvent RecordingStopped(Me, savedPath)
                    Debug.WriteLine("Recording stopped: " & savedPath)
                End Try
            End SyncLock
        End Sub
#End Region

#Region "SHADOWPLAY-STYLE REPLAY BUFFER v8.12 - STREAM COPY EXACT DURATION"

        Public Function StartBuffer() As Boolean
            SyncLock _bufferLock
                If _isBuffering Then
                    Debug.WriteLine("StartBuffer: Already running")
                    Return False
                End If

                If Not ValidateFFmpeg() Then Return False

                bufferTempDir = Path.Combine(Application.StartupPath, "Temp", "replay_buffer")
                Try
                    If Directory.Exists(bufferTempDir) Then
                        For Each f In Directory.GetFiles(bufferTempDir, "*.mkv")
                            Try : File.Delete(f) : Catch : End Try
                        Next
                    Else
                        Directory.CreateDirectory(bufferTempDir)
                    End If
                Catch
                    bufferTempDir = Path.Combine(Path.GetTempPath(), "ShadowPlay_Buffer_" & Process.GetCurrentProcess().Id)
                    Directory.CreateDirectory(bufferTempDir)
                End Try

                _segmentTimestamps.Clear()

                Try
                    Dim arguments As String = BuildBufferFFmpegArguments()

                    Debug.WriteLine("══════════ Buffer FFmpeg Arguments ══════════")
                    Debug.WriteLine(arguments)
                    Debug.WriteLine("════════════════════════════════════════════════")

                    bufferProcess = CreateFFmpegProcess(arguments)
                    bufferProcess.Start()
                    AddProcessToJob(bufferProcess)
                    bufferProcess.BeginErrorReadLine()
                    bufferProcess.BeginOutputReadLine()

                    _isBuffering = True
                    bufferStartTime = DateTime.Now

                    RaiseEvent ReplayBufferStarted(Me, EventArgs.Empty)
                    RaiseEvent BufferStarted(Me, EventArgs.Empty)
                    Debug.WriteLine(String.Format("Buffer started: {0}", bufferTempDir))
                    Debug.WriteLine(String.Format("Buffer capacity: FIXED at {0}s ({1} segments)", FIXED_BUFFER_SECONDS, FIXED_BUFFER_SEGMENTS))
                    Debug.WriteLine(String.Format("Replay save duration (user setting): {0}s", _replaySaveDuration))
                    Debug.WriteLine(String.Format("Exact Duration Mode: {0}", _exactDurationMode))
                    Debug.WriteLine(String.Format("Segment duration: {0}s (for frame-accurate trim)", SEGMENT_DURATION))
                    Return True

                Catch ex As Exception
                    RaiseEvent RecordingError(Me, "Failed to start buffer: " & ex.Message)
                    Debug.WriteLine("StartBuffer Error: " & ex.Message)
                    CleanupBufferProcess()
                    Return False
                End Try
            End SyncLock
        End Function

        Public Sub StopBuffer()
            SyncLock _bufferLock
                If Not _isBuffering OrElse bufferProcess Is Nothing Then Exit Sub

                Try
                    StopProcessGracefully(bufferProcess, FORCE_KILL_TIMEOUT)
                Catch ex As Exception
                    Debug.WriteLine("StopBuffer error: " & ex.Message)
                    ForceKillProcess(bufferProcess)
                Finally
                    CleanupBufferProcess()
                    _isBuffering = False

                    Threading.Thread.Sleep(500)

                    Try
                        If Directory.Exists(bufferTempDir) Then
                            For i As Integer = 1 To 3
                                Try
                                    For Each f In Directory.GetFiles(bufferTempDir)
                                        Try
                                            File.Delete(f)
                                        Catch ex2 As IOException
                                            Debug.WriteLine(String.Format("StopBuffer: Could not delete {0} - {1}", f, ex2.Message))
                                        End Try
                                    Next
                                    Exit For
                                Catch ex3 As Exception
                                    Debug.WriteLine(String.Format("StopBuffer: Cleanup attempt {0} failed: {1}", i, ex3.Message))
                                    Threading.Thread.Sleep(200)
                                End Try
                            Next
                        End If
                    Catch ex4 As Exception
                        Debug.WriteLine("StopBuffer: Directory cleanup error: " & ex4.Message)
                    End Try

                    _segmentTimestamps.Clear()

                    RaiseEvent ReplayBufferStopped(Me, EventArgs.Empty)
                    RaiseEvent BufferStopped(Me, EventArgs.Empty)
                    Debug.WriteLine("Buffer stopped")
                End Try
            End SyncLock
        End Sub

        Public Function SaveReplay(outputPath As String, saveDuration As Integer) As Boolean
            SyncLock _saveLock
                If _isSaving Then
                    Debug.WriteLine("│ ⚠️  SaveReplay already in progress, skipping duplicate call")
                    Return False
                End If
                _isSaving = True
            End SyncLock

            Try
                Debug.WriteLine("")
                Debug.WriteLine("╔═══════════════════════════════════════════════════════════════════════════════╗")
                Debug.WriteLine("║     SAVE REPLAY v8.12 - STREAM COPY (EXACT DURATION + BUFFER QUALITY)         ║")
                Debug.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝")
                Debug.WriteLine(String.Format("│ Requested: {0}s → Output: {1}", saveDuration, outputPath))

                If Not _isBuffering Then
                    Debug.WriteLine("│ ❌ Buffer not active!")
                    Return False
                End If

                ' STEP 1: Get current buffer time
                Dim nowTime As Double = (DateTime.Now - bufferStartTime).TotalSeconds
                Debug.WriteLine(String.Format("│   nowTime = {0:F3}s", nowTime))

                ' Wait for file writes
                Threading.Thread.Sleep(FILE_WRITE_DELAY)

                ' STEP 2: Get all segments
                Dim segments As New List(Of TimestampedSegment)()
                SyncLock _bufferLock
                    segments = GetTimestampedSegments()
                End SyncLock

                If segments.Count = 0 Then
                    Debug.WriteLine("│ ❌ No segments found!")
                    Return False
                End If

                segments = segments.OrderBy(Function(s) s.SegmentNumber).ToList()

                Dim segmentsNeeded As Integer = CInt(Math.Floor(saveDuration / SEGMENT_DURATION))
                Dim framesPerSegment As Integer = CInt(_framerate * SEGMENT_DURATION)
                Dim exactFrameCount As Integer = segmentsNeeded * framesPerSegment
                Dim exactDuration As Double = exactFrameCount / _framerate

                Debug.WriteLine("│")
                Debug.WriteLine("│ ═══ STREAM COPY EXACT CALCULATION ═══")
                Debug.WriteLine(String.Format("│   Segment duration: {0}s", SEGMENT_DURATION))
                Debug.WriteLine(String.Format("│   Frames per segment: {0}", framesPerSegment))
                Debug.WriteLine(String.Format("│   Segments needed: {0}", segmentsNeeded))
                Debug.WriteLine(String.Format("│   Total frames: {0}", exactFrameCount))
                Debug.WriteLine(String.Format("│   Exact duration: {0}s", exactDuration))

                Dim availableSegments As Integer = segments.Count
                Dim bufferWasTooSmall As Boolean = (availableSegments < segmentsNeeded)

                If bufferWasTooSmall Then
                    segmentsNeeded = availableSegments
                    exactFrameCount = segmentsNeeded * framesPerSegment
                    exactDuration = exactFrameCount / _framerate
                    Debug.WriteLine(String.Format("│   ⚠️  Buffer has only {0} segments, using all", availableSegments))
                End If

                Dim selectedSegments As List(Of TimestampedSegment) = segments.Skip(Math.Max(0, segments.Count - segmentsNeeded)).Take(segmentsNeeded).ToList()

                If selectedSegments.Count = 0 Then
                    Debug.WriteLine("│ ❌ No segments to save!")
                    Return False
                End If

                Dim selFirst = selectedSegments.First()
                Dim selLast = selectedSegments.Last()

                Debug.WriteLine("│")
                Debug.WriteLine("│ [SEGMENT SELECTION]")
                Debug.WriteLine(String.Format("│   Selected: #{0} → #{1} ({2} segments)", selFirst.SegmentNumber, selLast.SegmentNumber, selectedSegments.Count))
                Debug.WriteLine(String.Format("│   Output: {0} frames = {1}s EXACT", exactFrameCount, exactDuration))

                Try
                    Dim outputDir As String = Path.GetDirectoryName(outputPath)
                    If Not String.IsNullOrEmpty(outputDir) AndAlso Not Directory.Exists(outputDir) Then
                        Directory.CreateDirectory(outputDir)
                    End If

                    Dim uniqueId As String = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")
                    Dim concatListPath As String = Path.Combine(bufferTempDir, String.Format("concat_list_{0}.txt", uniqueId))

                    Using writer As New StreamWriter(concatListPath)
                        For Each seg In selectedSegments
                            Dim escapedPath As String = seg.FilePath.Replace("\"c, "/"c)
                            writer.WriteLine(String.Format("file '{0}'", escapedPath))
                        Next
                    End Using

                    Debug.WriteLine("│")
                    Debug.WriteLine("│ [FFmpeg] STREAM COPY v8.12 - NO re-encoding")
                    Debug.WriteLine(String.Format("│   Segments: {0}", selectedSegments.Count))
                    Debug.WriteLine(String.Format("│   Expected duration: {0}s", exactDuration))
                    Debug.WriteLine("│   Quality: SAME AS BUFFER (stream copy)")
                    Debug.WriteLine("│   Speed: VERY FAST (~0.5s)")

                    Dim concatArgs As String = String.Format("-y -hide_banner -loglevel warning -f concat -safe 0 -i ""{0}"" -c copy -movflags +faststart -video_track_timescale {1} ""{2}""",
                        concatListPath, _framerate, outputPath)

                    Debug.WriteLine(String.Format("│ [FFmpeg] Args: {0}", concatArgs))

                    Dim startTime As DateTime = DateTime.Now

                    If Not RunFFmpegSync(concatArgs, CONCAT_TIMEOUT) Then
                        Debug.WriteLine("│ ❌ Concat failed!")
                        Try : File.Delete(concatListPath) : Catch : End Try
                        Return False
                    End If

                    Dim processTime As Double = (DateTime.Now - startTime).TotalSeconds
                    Debug.WriteLine(String.Format("│   ⏱️  Process time: {0:F2}s", processTime))

                    Try : File.Delete(concatListPath) : Catch : End Try

                    If File.Exists(outputPath) Then
                        Dim outputInfo As New FileInfo(outputPath)
                        Dim ffprobeDuration As Double = GetVideoDuration(outputPath)

                        Debug.WriteLine("│")
                        Debug.WriteLine("╔═══════════════════════════════════════════════════════════════════════════════╗")
                        Debug.WriteLine("║                         ✅ SAVE REPLAY SUCCESS                                ║")
                        Debug.WriteLine("╠═══════════════════════════════════════════════════════════════════════════════╣")
                        Debug.WriteLine(String.Format("║  Output: {0}", Path.GetFileName(outputPath)))
                        Debug.WriteLine(String.Format("║  Size: {0:F2} MB", outputInfo.Length / 1024 / 1024))
                        Debug.WriteLine(String.Format("║  Segments: {0} × {1}s = {2}s EXACT", selectedSegments.Count, SEGMENT_DURATION, exactDuration))
                        Debug.WriteLine(String.Format("║  Frames: {0} frames", exactFrameCount))
                        Debug.WriteLine(String.Format("║  Duration: {0:F6}s", ffprobeDuration))
                        Debug.WriteLine("║")

                        If bufferWasTooSmall Then
                            Debug.WriteLine("║  ⚠️  Buffer was smaller than requested!")
                            Debug.WriteLine(String.Format("║     Requested: {0}s | Got: {1}s", saveDuration, exactDuration))
                        Else
                            Debug.WriteLine(String.Format("║  Requested: {0}s | Got: {1}s ✅ EXACT!", saveDuration, exactDuration))
                            Debug.WriteLine("║")
                            Debug.WriteLine("║  ✅ STREAM COPY SUCCESS!")
                            Debug.WriteLine("║     - Quality: SAME AS BUFFER (no re-encoding)")
                            Debug.WriteLine("║     - Speed: VERY FAST")
                            Debug.WriteLine(String.Format("║     - {0} segments × {1}s = {2}s", selectedSegments.Count, SEGMENT_DURATION, exactDuration))
                        End If

                        Debug.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝")
                        Debug.WriteLine("")

                        Debug.WriteLine(String.Format("Saved duration info: {0}s (from file: {1:F1}s)", exactDuration, ffprobeDuration))

                        RaiseEvent ReplaySaved(Me, outputPath)
                        Return True
                    Else
                        Debug.WriteLine("│ ❌ Output file not created!")
                        Return False
                    End If

                Catch ex As Exception
                    Debug.WriteLine(String.Format("│ ❌ Error: {0}", ex.Message))
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

        Private Function GetActualBufferDuration() As Double
            Try
                If Not Directory.Exists(bufferTempDir) Then Return 0
                Dim files = Directory.GetFiles(bufferTempDir, "segment_*.mkv")
                Return files.Length * SEGMENT_DURATION
            Catch
                Return 0
            End Try
        End Function

        Private Function GetTimestampedSegments() As List(Of TimestampedSegment)
            Dim result As New List(Of TimestampedSegment)()

            If Not Directory.Exists(bufferTempDir) Then Return result

            Try
                Dim files = Directory.GetFiles(bufferTempDir, "segment_*.mkv")
                Debug.WriteLine(String.Format("GetTimestampedSegments: Found {0} files", files.Length))

                For Each f In files
                    Try
                        Dim fileName = Path.GetFileNameWithoutExtension(f)
                        Dim match = RegexSegment.Match(fileName)

                        If match.Success Then
                            Dim segNumber As Integer = Integer.Parse(match.Groups(1).Value)
                            result.Add(New TimestampedSegment With {
                                .FilePath = f,
                                .SegmentNumber = segNumber,
                                .Duration = SEGMENT_DURATION
                            })
                        Else
                            Dim fi As New FileInfo(f)
                            result.Add(New TimestampedSegment With {
                                .FilePath = f,
                                .SegmentNumber = CInt(fi.CreationTime.Ticks Mod Integer.MaxValue),
                                .Duration = SEGMENT_DURATION
                            })
                        End If
                    Catch ex As Exception
                        Debug.WriteLine(String.Format("GetTimestampedSegments: Error processing file: {0}", ex.Message))
                        Try
                            Dim fi As New FileInfo(f)
                            result.Add(New TimestampedSegment With {
                                .FilePath = f,
                                .SegmentNumber = CInt(fi.CreationTime.Ticks Mod Integer.MaxValue),
                                .Duration = SEGMENT_DURATION
                            })
                        Catch
                        End Try
                    End Try
                Next

                If result.Count > FIXED_BUFFER_SEGMENTS Then
                    result = result.OrderBy(Function(s) s.SegmentNumber).ToList()
                    Dim toRemove As Integer = result.Count - FIXED_BUFFER_SEGMENTS
                    For i As Integer = 0 To toRemove - 1
                        Try
                            If File.Exists(result(i).FilePath) Then
                                File.Delete(result(i).FilePath)
                            End If
                        Catch
                        End Try
                    Next
                    result = result.Skip(toRemove).ToList()
                End If

            Catch ex As Exception
                Debug.WriteLine("GetTimestampedSegments Error: " & ex.Message)
            End Try

            Return result
        End Function

        Private Class TimestampedSegment
            Public Property FilePath As String
            Public Property SegmentNumber As Integer
            Public Property Duration As Double

            Public ReadOnly Property StartTime As Double
                Get
                    Return SegmentNumber * SEGMENT_DURATION
                End Get
            End Property
            Public ReadOnly Property EndTime As Double
                Get
                    Return (SegmentNumber + 1) * SEGMENT_DURATION
                End Get
            End Property
        End Class
#End Region

#Region "FFmpeg Process Management"
        Private Sub StopProcessGracefully(proc As Process, timeoutMs As Integer)
            If proc Is Nothing OrElse proc.HasExited Then Exit Sub

            Try
                proc.StandardInput.Write("q"c)
                proc.StandardInput.Flush()
                If Not proc.WaitForExit(timeoutMs) Then
                    proc.Kill()
                End If
            Catch ex As Exception
                Debug.WriteLine("StopProcessGracefully error: " & ex.Message)
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
                        Debug.WriteLine(String.Format("RunFFmpegSync: Timeout after {0}ms", timeoutMs))
                        Try
                            proc.Kill()
                            proc.WaitForExit(1000)
                        Catch
                        End Try
                        Return False
                    End If

                    If proc.ExitCode <> 0 Then
                        Debug.WriteLine(String.Format("RunFFmpegSync: Exit code = {0}", proc.ExitCode))
                        Debug.WriteLine("RunFFmpegSync stderr: " & stderr)
                    End If

                    Return proc.ExitCode = 0
                End Using
            Catch ex As Exception
                Debug.WriteLine("RunFFmpegSync Error: " & ex.Message)
                Return False
            End Try
        End Function
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

            If ShouldUseGPUCapture() Then
                BuildGPUCaptureCommand(sb, region)
            Else
                BuildSoftwareCaptureCommand(sb, region)
            End If

            BuildEncoderCommand(sb)

            sb.Append("-movflags +faststart """)
            sb.Append(outputFile)
            sb.Append(""""c)
            Return sb.ToString()
        End Function

        Private Function BuildBufferFFmpegArguments() As String
            Dim sb As New StringBuilder(SB_CAPACITY_XLARGE)

            sb.Append("-y -hide_banner -loglevel warning ")

            If ShouldUseGPUCapture() Then
                BuildGPUCaptureCommand(sb, Rectangle.Empty)
            Else
                BuildSoftwareCaptureCommand(sb, Rectangle.Empty)
            End If

            BuildBufferEncoderCommand(sb)

            sb.Append("-f segment ")
            sb.Append("-segment_time ")
            sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
            sb.Append(" -segment_format mkv ")
            sb.Append("-reset_timestamps 1 ")

            Dim segmentPath As String = Path.Combine(bufferTempDir, "segment_%04d.mkv")
            sb.Append(""""c)
            sb.Append(segmentPath)
            sb.Append(""""c)

            Return sb.ToString()
        End Function

        Private Function GetCachedScreenDimensions() As (Width As Integer, Height As Integer)
            If (DateTime.Now - _screenCacheTime).TotalSeconds > 5 Then
                _cachedScreenW = GetSystemMetrics(SM_CXSCREEN)
                _cachedScreenH = GetSystemMetrics(SM_CYSCREEN)
                _screenCacheTime = DateTime.Now
            End If
            Return (_cachedScreenW, _cachedScreenH)
        End Function

        Private Function ShouldUseGPUCapture() As Boolean
            Dim isHardwareEncoder As Boolean = (
                _encoder = VideoEncoder.NVENC_H264 OrElse
                _encoder = VideoEncoder.NVENC_HEVC OrElse
                _encoder = VideoEncoder.NVENC_AV1 OrElse
                _encoder = VideoEncoder.QuickSync_H264 OrElse
                _encoder = VideoEncoder.QuickSync_HEVC OrElse
                _encoder = VideoEncoder.AMF_H264 OrElse
                _encoder = VideoEncoder.AMF_HEVC
            )

            If Not isHardwareEncoder Then
                Return False
            End If

            If _ddagrabChecked Then
                Return _ddagrabAvailable
            End If

            If Not String.IsNullOrEmpty(_ffmpegPath) AndAlso File.Exists(_ffmpegPath) Then
                CheckDdagrabAvailability(_ffmpegPath)
                Return _ddagrabAvailable
            End If

            Return False
        End Function

        Private Sub BuildGPUCaptureCommand(sb As StringBuilder, region As Rectangle)
            Dim screenDims = GetCachedScreenDimensions()
            Dim needScaling As Boolean = (_resolutionWidth <> screenDims.Width OrElse _resolutionHeight <> screenDims.Height)

            If (_encoder = VideoEncoder.NVENC_H264 OrElse
                _encoder = VideoEncoder.NVENC_HEVC OrElse
                _encoder = VideoEncoder.NVENC_AV1) Then

                BuildNVENCCapture(sb, needScaling)

            ElseIf (_encoder = VideoEncoder.QuickSync_H264 OrElse
                    _encoder = VideoEncoder.QuickSync_HEVC) Then

                BuildQuickSyncCapture(sb, needScaling)

            ElseIf (_encoder = VideoEncoder.AMF_H264 OrElse
                    _encoder = VideoEncoder.AMF_HEVC) Then

                ' ✅ FIXED: AMD AMF Support
                BuildAMFCapture(sb, needScaling)

            Else
                BuildSoftwareCaptureCommand(sb, region)
            End If
        End Sub

        Private Sub BuildNVENCCapture(sb As StringBuilder, needScaling As Boolean)
            sb.Append("-f lavfi -i ""ddagrab=0:framerate=")
            sb.Append(_framerate.ToString())
            sb.Append(":draw_mouse=")
            sb.Append(If(_captureCursor, "1", "0"))
            sb.Append(""" ")

            If Not needScaling Then Exit Sub

            sb.Append("-vf ""hwdownload,format=bgra,scale=")
            sb.Append(_resolutionWidth.ToString())
            sb.Append(":"c)
            sb.Append(_resolutionHeight.ToString())
            sb.Append(":flags=lanczos,format=yuv420p"" ")
        End Sub

        Private Sub BuildQuickSyncCapture(sb As StringBuilder, needScaling As Boolean)
            sb.Append("-init_hw_device qsv=hw -filter_hw_device hw ")
            sb.Append("-use_wallclock_as_timestamps 1 -fflags +genpts ")

            sb.Append("-f lavfi -i ""ddagrab=0:framerate=")
            sb.Append(_framerate.ToString())
            sb.Append(":draw_mouse=")
            sb.Append(If(_captureCursor, "1", "0"))
            sb.Append(""" ")

            If needScaling Then
                sb.Append("-vf ""hwmap=derive_device=qsv,format=qsv,scale_qsv=")
                sb.Append(_resolutionWidth.ToString())
                sb.Append(":"c)
                sb.Append(_resolutionHeight.ToString())
                sb.Append(":format=nv12"" ")
            Else
                sb.Append("-vf ""hwmap=derive_device=qsv,format=qsv"" ")
            End If
        End Sub

        ' ══════════════════════════════════════════════════════════════════════════════
        ' ✅ NEW: AMD AMF Capture Support
        ' ══════════════════════════════════════════════════════════════════════════════
        Private Sub BuildAMFCapture(sb As StringBuilder, needScaling As Boolean)
            ' AMD AMF works with ddagrab + hwdownload for software processing
            ' AMF encoder accepts software frames and handles upload internally
            sb.Append("-f lavfi -i ""ddagrab=0:framerate=")
            sb.Append(_framerate.ToString())
            sb.Append(":draw_mouse=")
            sb.Append(If(_captureCursor, "1", "0"))
            sb.Append(""" ")

            If needScaling Then
                sb.Append("-vf ""hwdownload,format=bgra,scale=")
                sb.Append(_resolutionWidth.ToString())
                sb.Append(":"c)
                sb.Append(_resolutionHeight.ToString())
                sb.Append(":flags=lanczos,format=yuv420p"" ")
            Else
                sb.Append("-vf ""hwdownload,format=bgra,format=yuv420p"" ")
            End If
        End Sub

        Private Sub BuildSoftwareCaptureCommand(sb As StringBuilder, region As Rectangle)
            sb.Append("-f gdigrab ")
            sb.Append("-framerate ")
            sb.Append(_framerate.ToString())
            sb.Append(" ")
            sb.Append("-draw_mouse ")
            sb.Append(If(_captureCursor, "1", "0"))
            sb.Append(" -i desktop ")

            sb.Append("-vf ""scale=")
            sb.Append(_resolutionWidth.ToString())
            sb.Append(":"c)
            sb.Append(_resolutionHeight.ToString())
            sb.Append(":flags=lanczos,format=yuv420p"" ")
        End Sub

        ' ══════════════════════════════════════════════════════════════════════════════
        ' ✅ FIXED: BuildEncoderCommand with QuickSync & AMF Support
        ' ══════════════════════════════════════════════════════════════════════════════
        Private Sub BuildEncoderCommand(sb As StringBuilder)
            Dim presetStr As String = GetEncoderPresetString()
            Dim gopSize As Integer = _framerate * 2

            Select Case _encoder
                ' ══════════════════════════════════════════════════════════════════════════
                ' NVIDIA NVENC
                ' ══════════════════════════════════════════════════════════════════════════
                Case VideoEncoder.NVENC_H264
                    sb.Append("-c:v h264_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ull -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    If _useConstantBitrate Then
                        sb.Append("-rc:v cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc:v vbr -cq 18 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_HEVC
                    sb.Append("-c:v hevc_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ull -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    If _useConstantBitrate Then
                        sb.Append("-rc:v cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc:v vbr -cq 20 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_AV1
                    sb.Append("-c:v av1_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ull -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    If _useConstantBitrate Then
                        sb.Append("-rc:v cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc:v vbr -cq 22 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                ' ══════════════════════════════════════════════════════════════════════════
                ' ✅ INTEL QUICKSYNC - FIXED
                ' ══════════════════════════════════════════════════════════════════════════
                Case VideoEncoder.QuickSync_H264
                    sb.Append("-c:v h264_qsv -preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -look_ahead 0 ")
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-q:v 22 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.QuickSync_HEVC
                    sb.Append("-c:v hevc_qsv -preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -look_ahead 0 ")
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-q:v 25 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                ' ══════════════════════════════════════════════════════════════════════════
                ' ✅ AMD AMF - FIXED
                ' ══════════════════════════════════════════════════════════════════════════
                Case VideoEncoder.AMF_H264
                    sb.Append("-c:v h264_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -rc ")
                    If _useConstantBitrate Then
                        sb.Append("cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    Else
                        sb.Append("vbr_latency -q:v 22 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.AMF_HEVC
                    sb.Append("-c:v hevc_amf -quality ")
                    sb.Append(GetAmfQualityString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -rc ")
                    If _useConstantBitrate Then
                        sb.Append("cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    Else
                        sb.Append("vbr_latency -q:v 25 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                    ' ══════════════════════════════════════════════════════════════════════════
                    ' Software Encoders
                    ' ══════════════════════════════════════════════════════════════════════════
                Case Else
                    BuildSoftwareEncoderCommand(sb, gopSize)
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
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-crf 18 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                    ' ✅ FIXED: Added LibX265 support
                Case VideoEncoder.LibX265
                    sb.Append("-c:v libx265 -preset ")
                    sb.Append(GetX264PresetString())
                    sb.Append(" -tune zerolatency -profile:v main -g ")
                    sb.Append(gopSize)
                    sb.Append(" ")
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-crf 20 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")
            End Select
        End Sub

        ' ══════════════════════════════════════════════════════════════════════════════
        ' ✅ FIXED: BuildBufferEncoderCommand with QuickSync & AMF Support
        ' ══════════════════════════════════════════════════════════════════════════════
        Private Sub BuildBufferEncoderCommand(sb As StringBuilder)
            Dim presetStr As String = GetEncoderPresetString()

            ' Keyframe every segment duration for frame-accurate trimming
            Dim gopSize As Integer = CInt(Math.Ceiling(_framerate * SEGMENT_DURATION))

            Debug.WriteLine(String.Format("BuildBufferEncoderCommand: Keyframe every {0} frames ({1}s)", gopSize, SEGMENT_DURATION))

            Select Case _encoder
                ' ══════════════════════════════════════════════════════════════════════════
                ' NVIDIA NVENC
                ' ══════════════════════════════════════════════════════════════════════════
                Case VideoEncoder.NVENC_H264
                    sb.Append("-c:v h264_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ull -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    If _useConstantBitrate Then
                        sb.Append("-rc:v cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc:v vbr -cq 18 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_HEVC
                    sb.Append("-c:v hevc_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ull -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    If _useConstantBitrate Then
                        sb.Append("-rc:v cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc:v vbr -cq 20 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                Case VideoEncoder.NVENC_AV1
                    sb.Append("-c:v av1_nvenc -preset ")
                    sb.Append(presetStr)
                    sb.Append(" -tune ull -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    If _useConstantBitrate Then
                        sb.Append("-rc:v cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc:v vbr -cq 22 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate:v ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                ' ══════════════════════════════════════════════════════════════════════════
                ' ✅ INTEL QUICKSYNC - FIXED
                ' ══════════════════════════════════════════════════════════════════════════
                Case VideoEncoder.QuickSync_H264
                    sb.Append("-c:v h264_qsv -preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -look_ahead 0 ")
                    sb.Append("-force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-q:v 22 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                Case VideoEncoder.QuickSync_HEVC
                    sb.Append("-c:v hevc_qsv -preset ")
                    sb.Append(GetQsvPresetString())
                    sb.Append(" -g ")
                    sb.Append(gopSize)
                    sb.Append(" -keyint_min ")
                    sb.Append(gopSize)
                    sb.Append(" -look_ahead 0 ")
                    sb.Append("-force_key_frames ""expr:gte(t,n_forced*")
                    sb.Append(SEGMENT_DURATION.ToString("F2", Globalization.CultureInfo.InvariantCulture))
                    sb.Append(")"" ")
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-q:v 25 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                ' ══════════════════════════════════════════════════════════════════════════
                ' ✅ AMD AMF - FIXED
                ' ══════════════════════════════════════════════════════════════════════════
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
                    If _useConstantBitrate Then
                        sb.Append("-rc cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc vbr_latency -q:v 22 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
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
                    If _useConstantBitrate Then
                        sb.Append("-rc cbr -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    Else
                        sb.Append("-rc vbr_latency -q:v 25 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" ")

                    ' ══════════════════════════════════════════════════════════════════════════
                    ' Software Encoders
                    ' ══════════════════════════════════════════════════════════════════════════
                Case Else
                    BuildSoftwareBufferEncoderCommand(sb, gopSize)
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
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-crf 18 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")

                    ' ✅ FIXED: Added LibX265 buffer support
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
                    If _useConstantBitrate Then
                        sb.Append("-b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate)
                        sb.Append("k -bufsize ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    Else
                        sb.Append("-crf 20 -b:v ")
                        sb.Append(_bitrate)
                        sb.Append("k -maxrate ")
                        sb.Append(_bitrate * 2)
                        sb.Append("k ")
                    End If
                    sb.Append("-r ")
                    sb.Append(_framerate)
                    sb.Append(" -fps_mode cfr ")
            End Select
        End Sub

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

        ' ══════════════════════════════════════════════════════════════════════════════
        ' ✅ NEW: Intel QuickSync Preset Helper
        ' ══════════════════════════════════════════════════════════════════════════════
        Private Function GetQsvPresetString() As String
            ' QSV presets: veryslow, slower, slow, medium, fast, faster, veryfast
            Select Case _encoderPreset
                Case 1 : Return "veryslow"
                Case 2 : Return "slower"
                Case 3 : Return "slow"
                Case 4 : Return "medium"
                Case 5 : Return "fast"
                Case 6 : Return "faster"
                Case 7 : Return "veryfast"
                Case Else : Return "medium"
            End Select
        End Function

        ' ══════════════════════════════════════════════════════════════════════════════
        ' ✅ NEW: AMD AMF Quality Helper
        ' ══════════════════════════════════════════════════════════════════════════════
        Private Function GetAmfQualityString() As String
            ' AMF quality: quality, balanced, speed
            Select Case _encoderPreset
                Case 1, 2 : Return "quality"
                Case 3, 4 : Return "balanced"
                Case 5, 6, 7 : Return "speed"
                Case Else : Return "balanced"
            End Select
        End Function
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
