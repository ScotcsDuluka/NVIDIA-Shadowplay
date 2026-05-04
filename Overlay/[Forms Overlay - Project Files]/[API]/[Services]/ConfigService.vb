Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Http.Headers
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports CaptureEngine.CaptureCore
Imports System.Net.Http
Imports System.Security.Cryptography

Public Class AppSettings
    Private Shared ReadOnly NvidiaKeywords As String() = {"NVIDIA", "GEFORCE", "GTX", "RTX"}
    Private Shared ReadOnly AmdKeywords As String() = {"AMD", "RADEON", "RX "}
    Private Shared ReadOnly IntelKeywords As String() = {"INTEL"}
    Private Shared ReadOnly IntelIGpuKeywords As String() = {"UHD", "IRIS", "HD GRAPHICS", "INTEL(R) GRAPHICS"}

#Region "Settings Classes (Grouped) - สำหรับ JSON Serialization"

    ''' <summary>
    ''' Recording settings: encoder, fps, bitrate, resolution, preset
    ''' </summary>
    Public Class RecordingSettingsClass
        Public Property UseNativeResolution As Boolean = True
        Public Property Encoder As String = "NVENC_H264"
        Public Property EncoderNow As String = "NVENC_H264"
        Public Property FPS As Integer = 60
        Public Property Bitrate As Integer = 8000
        Public Property Width As Integer = 1920
        Public Property Height As Integer = 1080
        Public Property Preset As String = "Medium"
        Public Property EncoderPreset As Integer = 4
        Public Property ReplayDuration As Integer = 60

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' Path settings: gallery, save, ffmpeg paths
    ''' </summary>
    Public Class PathSettingsClass
        Public Property GalleryPath As String = ""
        Public Property SavePath As String = ""
        Public Property FFmpegPath As String = ""

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' UI settings: language, theme
    ''' </summary>
    Public Class UISettingsClass
        Public Property Language As String = "en-US"
        Public Property Theme As String = "Dark"

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' ✅ Audio settings: system audio, microphone, volume
    ''' </summary>
    Public Class AudioSettingsClass
        ''' <summary>
        ''' Enable system audio capture (WASAPI Loopback via NAudio)
        ''' </summary>
        Public Property SystemAudioEnabled As Boolean = True

        ''' <summary>
        ''' Enable microphone capture (DirectShow)
        ''' </summary>
        Public Property MicEnabled As Boolean = False

        ''' <summary>
        ''' System audio volume (0.0 - 1.0)
        ''' </summary>
        Public Property SystemAudioVolume As Single = 1.0F

        ''' <summary>
        ''' Microphone volume (0.0 - 1.0)
        ''' </summary>
        Public Property MicVolume As Single = 1.0F

        ''' <summary>
        ''' Microphone device name (empty = default device)
        ''' </summary>
        Public Property MicDeviceName As String = ""

        ' ═══════════════════════════════════════════════════════════════════════
        ' Legacy properties for backward compatibility with old config.json
        ' ═══════════════════════════════════════════════════════════════════════

        ''' <summary>
        ''' Legacy: Mic volume as integer (0-100) - converted to MicVolume
        ''' </summary>
        <Obsolete("Use MicVolume instead")>
        Public Property MicVolumePercent As Integer
            Get
                Return CInt(MicVolume * 100)
            End Get
            Set(value As Integer)
                MicVolume = Math.Max(0, Math.Min(100, value)) / 100.0F
            End Set
        End Property

        ''' <summary>
        ''' Legacy: System volume as integer (0-100) - converted to SystemAudioVolume
        ''' </summary>
        <Obsolete("Use SystemAudioVolume instead")>
        Public Property SystemVolumePercent As Integer
            Get
                Return CInt(SystemAudioVolume * 100)
            End Get
            Set(value As Integer)
                SystemAudioVolume = Math.Max(0, Math.Min(100, value)) / 100.0F
            End Set
        End Property

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' ✅ GitHub User settings - เก็บข้อมูลผู้ใช้ GitHub
    ''' </summary>
    Public Class GitHubUserClass
        Public Property Username As String = ""
        Public Property AvatarUrl As String = ""
        Public Property IsLoggedIn As Boolean = False
        Public Property LastLogin As DateTime = DateTime.MinValue

        Public Sub New()
        End Sub
    End Class

    ' ====================================================================
    ' <<<< ลบ HotkeySettingsClass ทิ้งไปแล้ว ใช้ Dictionary แทน >>>
    ' ====================================================================

    ''' <summary>
    ''' ✅ GitHub Token สำหรับ OAuth
    ''' </summary>
    Public Property GitHubUser As New GitHubUserClass()
    Public Property GitHubToken As String = ""

#End Region

#Region "✅ GitHub Management Methods"

    ''' <summary>
    ''' บันทึกข้อมูล GitHub User และ Token
    ''' </summary>
    Public Sub SaveGitHubUser(username As String, avatarUrl As String, token As String)
        GitHubUser.Username = username
        GitHubUser.AvatarUrl = avatarUrl
        GitHubUser.IsLoggedIn = True
        GitHubUser.LastLogin = DateTime.Now
        GitHubToken = token
        Save()
        Debug.WriteLine($"SaveGitHubUser: Saved user '{username}' to config.json")
    End Sub

    ''' <summary>
    ''' ล้างข้อมูล GitHub User (Logout)
    ''' </summary>
    Public Sub ClearGitHubUser()
        GitHubUser.Username = ""
        GitHubUser.AvatarUrl = ""
        GitHubUser.IsLoggedIn = False
        GitHubUser.LastLogin = DateTime.MinValue
        GitHubToken = ""
        Save()
        Debug.WriteLine("ClearGitHubUser: User logged out")
    End Sub

    ''' <summary>
    ''' โหลดข้อมูล GitHub User จาก API (ถ้ามี token)
    ''' </summary>
    Public Async Function LoadGitHubUser() As Task
        Try
            If String.IsNullOrEmpty(GitHubToken) Then
                Debug.WriteLine("LoadGitHubUser: No token, skipping")
                Return
            End If

            ' เช็คเน็ต
            Using ping As New Net.NetworkInformation.Ping()
                Dim reply = Await ping.SendPingAsync("api.github.com", 2000)
                If reply.Status <> Net.NetworkInformation.IPStatus.Success Then
                    Debug.WriteLine("LoadGitHubUser: No internet connection")
                    Throw New Exception("No Internet")
                End If
            End Using

            ' โหลดจาก GitHub API
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", GitHubToken)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("VBApp")

                Dim json = Await client.GetStringAsync("https://api.github.com/user")
                Dim doc = JsonDocument.Parse(json)

                GitHubUser.Username = doc.RootElement.GetProperty("login").GetString()
                GitHubUser.AvatarUrl = doc.RootElement.GetProperty("avatar_url").GetString()
                GitHubUser.IsLoggedIn = True

                Debug.WriteLine($"LoadGitHubUser: Loaded user '{GitHubUser.Username}' from API")
            End Using

            ' บันทึก config.json
            Save()

        Catch ex As Exception
            Debug.WriteLine($"LoadGitHubUser Error: {ex.Message}")
            ' offline ใช้ข้อมูลเก่าใน GitHubUser
        End Try
    End Function

    ''' <summary>
    ''' โหลด Avatar จาก URL และแสดงใน PictureBox
    ''' </summary>
    Public Async Function LoadGitHubAvatar(pb As PictureBox) As Task
        If String.IsNullOrEmpty(GitHubUser.AvatarUrl) Then
            Debug.WriteLine("LoadGitHubAvatar: No avatar URL")
            Return
        End If

        Try
            ' พยายามโหลดจาก cache ก่อน
            Dim avatarPath As String = Path.Combine(Application.StartupPath, "avatar.png")
            If File.Exists(avatarPath) Then
                Try
                    ' ✅ ใช้ MemoryStream เพื่อหลีกเลี่ยง file lock
                    Dim bytes As Byte() = File.ReadAllBytes(avatarPath)
                    Using ms As New MemoryStream(bytes)
                        pb.BackgroundImage = New Bitmap(ms)
                    End Using
                    Debug.WriteLine("LoadGitHubAvatar: Loaded from cache")
                    Return
                Catch
                    ' ถ้า cache ใช้ไม่ได้ โหลดใหม่จาก URL
                End Try
            End If

            ' โหลดจาก URL
            Using client As New HttpClient()
                Dim bytes() As Byte = Await client.GetByteArrayAsync(GitHubUser.AvatarUrl)

                ' ✅ แก้ bug: Clone image แทนการใช้ stream โดยตรง
                Using ms As New MemoryStream(bytes)
                    pb.BackgroundImage = New Bitmap(ms)
                End Using

                ' บันทึก cache
                Try
                    File.WriteAllBytes(avatarPath, bytes)
                    Debug.WriteLine("LoadGitHubAvatar: Saved to cache")
                Catch ex As Exception
                    Debug.WriteLine($"LoadGitHubAvatar: Failed to save cache - {ex.Message}")
                End Try
            End Using

            Debug.WriteLine("LoadGitHubAvatar: Loaded from URL")

        Catch ex As Exception
            Debug.WriteLine($"LoadGitHubAvatar Error: {ex.Message}")
            ' offline / error ไม่โหลดรูป
        End Try
    End Function

    ''' <summary>
    ''' โหลด Avatar จาก Byte Array (หลัง login)
    ''' </summary>
    Public Sub SaveAvatarFromBytes(avatarBytes As Byte())
        Try
            Dim avatarPath As String = Path.Combine(Application.StartupPath, "avatar.png")
            File.WriteAllBytes(avatarPath, avatarBytes)
            Debug.WriteLine($"SaveAvatarFromBytes: Saved to {avatarPath}")
        Catch ex As Exception
            Debug.WriteLine($"SaveAvatarFromBytes Error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' ตรวจสอบว่ามีการ login อยู่หรือไม่
    ''' </summary>
    Public ReadOnly Property IsGitHubLoggedIn As Boolean
        Get
            Return GitHubUser.IsLoggedIn AndAlso Not String.IsNullOrEmpty(GitHubToken)
        End Get
    End Property

#End Region

#Region "Properties (Public สำหรับ JSON Serialization)"

    Public Property Recording As New RecordingSettingsClass()
    Public Property Paths As New PathSettingsClass()
    Public Property UI As New UISettingsClass()
    Public Property Audio As New AudioSettingsClass()

    ' <<< เปลี่ยนจาก HotkeySettingsClass เป็น Dictionary >>>
    Public Property Hotkeys As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

#End Region

#Region "Singleton"

    Private Shared _instance As AppSettings = Nothing
    Private Shared ReadOnly _lock As New Object()
    Private Shared _isLoaded As Boolean = False

    ' Hardware detection flag
    Private Shared _hardwareDetected As Boolean = False

    ''' <summary>
    ''' Get singleton instance - Load() จะถูกเรียกอัตโนมัติครั้งแรก
    ''' </summary>
    Public Shared ReadOnly Property Instance As AppSettings
        Get
            If _instance Is Nothing Then
                SyncLock _lock
                    If _instance Is Nothing Then
                        _instance = New AppSettings()
                        _instance.Load()
                        _isLoaded = True
                    End If
                End SyncLock
            End If
            Return _instance
        End Get
    End Property

    ''' <summary>
    ''' Check if hardware detection has been run
    ''' </summary>
    Public Shared ReadOnly Property HardwareDetected As Boolean
        Get
            Return _hardwareDetected
        End Get
    End Property

    ''' <summary>
    ''' Parameterless constructor สำหรับ JSON deserialization
    ''' </summary>
    Public Sub New()
        Recording = New RecordingSettingsClass()
        Paths = New PathSettingsClass()
        UI = New UISettingsClass()
        Audio = New AudioSettingsClass()
        GitHubUser = New GitHubUserClass()

        ' <<< เปลี่ยนจาก HotkeySettingsClass เป็น Dictionary >>>
        Hotkeys = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    End Sub

    ''' <summary>
    ''' Initialize และ Load config - เรียกตอน app start (optional)
    ''' </summary>
    Public Shared Sub Initialize()
        SyncLock _lock
            If _instance Is Nothing Then
                _instance = New AppSettings()
            End If
            If Not _isLoaded Then
                _instance.Load()
                _isLoaded = True
            End If
            If Not _hardwareDetected Then
                DetectHardware()
            End If
        End SyncLock
    End Sub

#End Region

#Region "JSON File Path"

    Private _configPath As String = Nothing

    Private ReadOnly Property ConfigPath As String
        Get
            If _configPath Is Nothing Then
                _configPath = Path.Combine(Application.StartupPath, "config.json")
            End If
            Return _configPath
        End Get
    End Property

#End Region

#Region "Load / Save"

    ''' <summary>
    ''' Load settings from config.json
    ''' </summary>
    Public Sub Load()
        Try
            Debug.WriteLine("══════════ AppSettings.Load ══════════")
            Debug.WriteLine("ConfigPath: " & ConfigPath)

            If File.Exists(ConfigPath) Then
                Dim json As String = File.ReadAllText(ConfigPath)

                If Not String.IsNullOrWhiteSpace(json) Then
                    Dim options As New JsonSerializerOptions With {
                        .PropertyNameCaseInsensitive = True,
                        .AllowTrailingCommas = True,
                        .ReadCommentHandling = JsonCommentHandling.Skip
                    }

                    Dim loaded As AppSettings = JsonSerializer.Deserialize(Of AppSettings)(json, options)

                    If loaded IsNot Nothing Then
                        ApplyLoadedSettings(loaded)

                        Debug.WriteLine("AppSettings.Load: SUCCESS")
                        Debug.WriteLine($"  GitHub User: {GitHubUser.Username}")
                        Debug.WriteLine($"  GitHub Logged In: {GitHubUser.IsLoggedIn}")
                    End If
                End If
            Else
                ' Create default config
                Save()
                Debug.WriteLine("AppSettings.Load: Created default config")
            End If

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load Error: " & ex.Message)
        End Try
    End Sub

    Private Sub ApplyLoadedSettings(loaded As AppSettings)
        If loaded Is Nothing Then Return

        ApplyRecordingSettings(loaded.Recording)
        ApplyPathSettings(loaded.Paths)
        ApplyUISettings(loaded.UI)
        ApplyAudioSettings(loaded.Audio)
        ApplyGitHubUserSettings(loaded.GitHubUser)

        ' <<< ลบ ApplyHotkeySettings แล้ว ใช้วิธี Copy Dictionary แทน >>>
        If loaded.Hotkeys IsNot Nothing Then
            Hotkeys = New Dictionary(Of String, String)(loaded.Hotkeys, StringComparer.OrdinalIgnoreCase)
        End If

        GitHubToken = loaded.GitHubToken
    End Sub

    Private Sub ApplyRecordingSettings(loadedRecording As RecordingSettingsClass)
        If loadedRecording Is Nothing Then Return

        Recording.Encoder = loadedRecording.Encoder
        Recording.EncoderNow = loadedRecording.EncoderNow
        Recording.FPS = loadedRecording.FPS
        Recording.Bitrate = loadedRecording.Bitrate
        Recording.Width = loadedRecording.Width
        Recording.Height = loadedRecording.Height
        Recording.Preset = loadedRecording.Preset
        Recording.EncoderPreset = loadedRecording.EncoderPreset
        Recording.ReplayDuration = loadedRecording.ReplayDuration
        Recording.UseNativeResolution = loadedRecording.UseNativeResolution
    End Sub

    Private Sub ApplyPathSettings(loadedPaths As PathSettingsClass)
        If loadedPaths Is Nothing Then Return

        Paths.GalleryPath = loadedPaths.GalleryPath
        Paths.SavePath = loadedPaths.SavePath
        Paths.FFmpegPath = loadedPaths.FFmpegPath
    End Sub

    Private Sub ApplyUISettings(loadedUI As UISettingsClass)
        If loadedUI Is Nothing Then Return

        UI.Language = loadedUI.Language
        UI.Theme = loadedUI.Theme
    End Sub

    Private Sub ApplyAudioSettings(loadedAudio As AudioSettingsClass)
        If loadedAudio Is Nothing Then Return

        Audio.SystemAudioEnabled = loadedAudio.SystemAudioEnabled
        Audio.MicEnabled = loadedAudio.MicEnabled
        Audio.SystemAudioVolume = loadedAudio.SystemAudioVolume
        Audio.MicVolume = loadedAudio.MicVolume
        Audio.MicDeviceName = loadedAudio.MicDeviceName
    End Sub

    Private Sub ApplyGitHubUserSettings(loadedGitHubUser As GitHubUserClass)
        If loadedGitHubUser Is Nothing Then Return

        GitHubUser.Username = loadedGitHubUser.Username
        GitHubUser.AvatarUrl = loadedGitHubUser.AvatarUrl
        GitHubUser.IsLoggedIn = loadedGitHubUser.IsLoggedIn
        GitHubUser.LastLogin = loadedGitHubUser.LastLogin
    End Sub

    ' <<< ลบเมธอด ApplyHotkeySettings ทิ้งไปแล้ว >>>

    ''' <summary>
    ''' Save settings to config.json
    ''' </summary>
    Public Sub Save()
        Try
            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True,
                .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }

            Dim json As String = JsonSerializer.Serialize(Me, options)
            File.WriteAllText(ConfigPath, json)
            Debug.WriteLine("AppSettings.Save: Saved to " & ConfigPath)

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Save Error: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Apply to CaptureEngine.CaptureCore.ScreenRecorder"

    ''' <summary>
    ''' Apply settings from config.json to CaptureEngine.CaptureCore.ScreenRecorder
    ''' </summary>
    Public Sub ApplyToRecorder(recorder As CaptureEngine.CaptureCore.ScreenRecorder)
        Try
            ' ═══════════════════════════════════════════════════════════════════════
            ' IMPORTANT: Set Preset FIRST (เพราะ setter จะทับค่าอื่นๆ)
            ' ═══════════════════════════════════════════════════════════════════════
            Select Case Recording.Preset
                Case "Low"
                    recorder.Preset = CaptureEngine.CaptureCore.ScreenRecorder.RecordingPreset.Low
                Case "Medium"
                    recorder.Preset = CaptureEngine.CaptureCore.ScreenRecorder.RecordingPreset.Medium
                Case "High"
                    recorder.Preset = CaptureEngine.CaptureCore.ScreenRecorder.RecordingPreset.High
                Case "Custom"
                    recorder.Preset = CaptureEngine.CaptureCore.ScreenRecorder.RecordingPreset.Custom
                Case Else
                    recorder.Preset = CaptureEngine.CaptureCore.ScreenRecorder.RecordingPreset.Medium
            End Select

            ' ═══════════════════════════════════════════════════════════════════════
            ' Now apply custom settings (will override preset defaults)
            ' ═══════════════════════════════════════════════════════════════════════

            ' FPS
            recorder.Framerate = Recording.FPS

            ' Bitrate
            recorder.Bitrate = Recording.Bitrate

            ' Resolution
            recorder.ResolutionWidth = Recording.Width
            recorder.ResolutionHeight = Recording.Height

            ' Encoder Preset (1-7)
            recorder.EncoderPreset = Recording.EncoderPreset

            ' Encoder
            SetEncoder(recorder, Recording.Encoder)

            ' Replay Duration
            recorder.BufferDurationSeconds = Recording.ReplayDuration

        Catch ex As Exception
            Debug.WriteLine("ApplyToRecorder Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' ✅ Apply Audio settings to CaptureEngine.CaptureCore.ScreenRecorder
    ''' </summary>
    Public Sub ApplyAudioSettings(recorder As CaptureEngine.CaptureCore.ScreenRecorder)
        Try
            ' Set audio mode
            If Audio.SystemAudioEnabled AndAlso Audio.MicEnabled Then
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.Both
            ElseIf Audio.SystemAudioEnabled Then
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.SystemOnly
            ElseIf Audio.MicEnabled Then
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.MicOnly
            Else
                recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.None
            End If

            ' Set volumes
            recorder.SystemAudioVolume = Audio.SystemAudioVolume
            recorder.MicVolume = Audio.MicVolume

            ' Set mic device name (if specified)
            If Audio.MicEnabled AndAlso Not String.IsNullOrEmpty(Audio.MicDeviceName) Then
                recorder.MicDeviceName = Audio.MicDeviceName
            End If

            Debug.WriteLine($"ApplyAudioSettings: Mode={recorder.AudioMode}, SystemVol={Audio.SystemAudioVolume:P0}, MicVol={Audio.MicVolume:P0}")

        Catch ex As Exception
            Debug.WriteLine("ApplyAudioSettings Error: " & ex.Message)
            ' Default to system audio only
            recorder.AudioMode = CaptureEngine.CaptureCore.ScreenRecorder.VideoCaptureMode.SystemOnly
            recorder.SystemAudioVolume = 1.0F
        End Try
    End Sub

    Private Sub SetEncoder(recorder As CaptureEngine.CaptureCore.ScreenRecorder, encoderName As String)
        Try
            Select Case encoderName
                Case "NVENC_H264"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264
                Case "NVENC_HEVC"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.NVENC_HEVC
                Case "NVENC_AV1"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.NVENC_AV1
                Case "QuickSync_H264"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264
                Case "QuickSync_HEVC"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_HEVC
                Case "AMF_H264"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264
                Case "AMF_HEVC"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.AMF_HEVC
                Case "LibX264"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.LibX264
                Case "LibX265"
                    recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.LibX265
                Case Else
                    ' Auto-select based on hardware
                    If _hasNvidia.GetValueOrDefault(False) Then
                        recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.NVENC_H264
                    ElseIf _hasIntel.GetValueOrDefault(False) Then
                        recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.QuickSync_H264
                    ElseIf _hasAMD.GetValueOrDefault(False) Then
                        recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.AMF_H264
                    Else
                        recorder.Encoder = CaptureEngine.CaptureCore.ScreenRecorder.VideoEncoder.LibX264
                    End If
            End Select

        Catch ex As Exception
            Debug.WriteLine("SetEncoder Error: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Hardware Detection"

    Private Shared _hasNvidia As Boolean? = Nothing
    Private Shared _hasIntel As Boolean? = Nothing
    Private Shared _hasAMD As Boolean? = Nothing
    Private Shared _gpuName As String = ""
    Private Shared _intelGpuName As String = ""
    Private Shared _supportsAV1 As Boolean? = Nothing

    ' Store all detected GPU names
    Private Shared _allGpuNames As New List(Of String)()

    ''' <summary>
    ''' Check if NVIDIA GPU is available
    ''' </summary>
    Public Shared ReadOnly Property HasNvidia As Boolean
        Get
            Return _hasNvidia.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Check if Intel GPU is available
    ''' </summary>
    Public Shared ReadOnly Property HasIntel As Boolean
        Get
            Return _hasIntel.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Check if AMD GPU is available
    ''' </summary>
    Public Shared ReadOnly Property HasAMD As Boolean
        Get
            Return _hasAMD.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Get primary GPU name (NVIDIA > AMD > Intel)
    ''' </summary>
    Public Shared ReadOnly Property GPUName As String
        Get
            Return _gpuName
        End Get
    End Property

    ''' <summary>
    ''' Get Intel iGPU name
    ''' </summary>
    Public Shared ReadOnly Property IntelGPUName As String
        Get
            Return _intelGpuName
        End Get
    End Property

    ''' <summary>
    ''' Check if GPU supports AV1 encoding (RTX 40 series+)
    ''' </summary>
    Public Shared ReadOnly Property SupportsNVENCAV1 As Boolean
        Get
            If _supportsAV1 Is Nothing Then
                DetectAV1Support()
            End If
            Return _supportsAV1.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Detect AV1 support - RTX 40 series or newer
    ''' </summary>
    Private Shared Sub DetectAV1Support()
        _supportsAV1 = False

        If Not _hasNvidia.GetValueOrDefault(False) Then
            Exit Sub
        End If

        ' AV1 supported on: RTX 40 series (Ada Lovelace)
        Dim gpuUpper As String = _gpuName.ToUpperInvariant()

        If gpuUpper.Contains("RTX 40") OrElse
           gpuUpper.Contains("RTX 50") OrElse
           gpuUpper.Contains("ADA") Then
            _supportsAV1 = True
        End If

        Debug.WriteLine("AV1 Support: " & _supportsAV1.ToString() & " (GPU: " & _gpuName & ")")
    End Sub

    ''' <summary>
    ''' Detect available GPUs
    ''' </summary>
    Public Shared Sub DetectHardware()
        ' Skip if already detected
        If _hardwareDetected Then
            Debug.WriteLine("DetectHardware: Already detected, skipping")
            Exit Sub
        End If

        Try
            Debug.WriteLine("══════════ DetectHardware START ══════════")

            _hasNvidia = False
            _hasIntel = False
            _hasAMD = False
            _allGpuNames.Clear()

            ' Method 1: PowerShell Get-CimInstance
            DetectGPUsViaPowerShell()

            ' Method 2: Registry Detection
            DetectGPUsViaRegistry()

            ' Method 3: DLL Check (final fallback)
            Dim system32 As String = Environment.SystemDirectory

            ' NVIDIA - ต้องมี nvenc.dll
            If Not _hasNvidia.GetValueOrDefault(False) Then
                If File.Exists(Path.Combine(system32, "nvenc.dll")) Then
                    _hasNvidia = True
                    Debug.WriteLine("NVIDIA detected via nvenc.dll")
                End If
            End If

            ' AMD - amdocl64.dll
            If Not _hasAMD.GetValueOrDefault(False) Then
                If File.Exists(Path.Combine(system32, "amdocl64.dll")) Then
                    _hasAMD = True
                    Debug.WriteLine("AMD detected via amdocl64.dll")
                End If
            End If

            ' Set primary GPU name
            If _hasNvidia.GetValueOrDefault(False) Then
                _gpuName = _allGpuNames.FirstOrDefault(Function(n) n.ToUpperInvariant().Contains("NVIDIA"), "NVIDIA GPU")
            ElseIf _hasAMD.GetValueOrDefault(False) Then
                _gpuName = _allGpuNames.FirstOrDefault(Function(n) n.ToUpperInvariant().Contains("AMD") OrElse n.ToUpperInvariant().Contains("RADEON"), "AMD GPU")
            ElseIf _hasIntel.GetValueOrDefault(False) Then
                _gpuName = _intelGpuName
            End If

            ' Mark as detected
            _hardwareDetected = True

            Debug.WriteLine("══════════ DetectHardware RESULT ══════════")
            Debug.WriteLine("  NVIDIA: " & _hasNvidia.ToString())
            Debug.WriteLine("  Intel:  " & _hasIntel.ToString())
            Debug.WriteLine("  AMD:    " & _hasAMD.ToString())
            Debug.WriteLine("  Primary GPU: " & _gpuName)
            Debug.WriteLine("═══════════════════════════════════════════")

        Catch ex As Exception
            Debug.WriteLine("DetectHardware Error: " & ex.Message)
            _hardwareDetected = True ' Still mark as detected to prevent loops
        End Try
    End Sub

    ''' <summary>
    ''' Detect GPUs using PowerShell
    ''' </summary>
    Private Shared Sub DetectGPUsViaPowerShell()
        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = "powershell.exe",
                .Arguments = "-NoProfile -Command " & Chr(34) & "Get-CimInstance Win32_VideoController | Select-Object -ExpandProperty Name" & Chr(34),
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .CreateNoWindow = True
            }

            Using proc As Process = Process.Start(psi)
                If proc IsNot Nothing Then
                    Dim output As String = proc.StandardOutput.ReadToEnd()
                    proc.WaitForExit(5000)

                    Debug.WriteLine("PowerShell GPU Output: " & output.Trim())

                    Dim lines() As String = output.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)

                    For Each line As String In lines
                        Dim trimmed As String = line.Trim()
                        If String.IsNullOrEmpty(trimmed) Then Continue For
                        If trimmed.ToUpperInvariant().Contains("NAME") Then Continue For

                        AddGpuNameIfMissing(trimmed)
                        UpdateGpuFlagsFromName(trimmed)
                    Next
                End If
            End Using

        Catch ex As Exception
            Debug.WriteLine("DetectGPUsViaPowerShell Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Detect GPUs via Windows Registry
    ''' </summary>
    Private Shared Sub DetectGPUsViaRegistry()
        Try
            Const GPU_REGISTRY_PATH As String = "SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"

            Using key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(GPU_REGISTRY_PATH)
                If key Is Nothing Then Exit Sub

                For Each subKeyName As String In key.GetSubKeyNames()
                    Using subKey = key.OpenSubKey(subKeyName)
                        If subKey Is Nothing Then Continue For

                        Dim driverDesc As String = subKey.GetValue("DriverDesc", "").ToString()
                        If String.IsNullOrEmpty(driverDesc) Then Continue For
                        AddGpuNameIfMissing(driverDesc)
                        UpdateGpuFlagsFromName(driverDesc, True)
                    End Using
                Next
            End Using

        Catch ex As Exception
            Debug.WriteLine("DetectGPUsViaRegistry Error: " & ex.Message)
        End Try
    End Sub

    Private Shared Sub AddGpuNameIfMissing(gpuName As String)
        If String.IsNullOrWhiteSpace(gpuName) Then Return
        If Not _allGpuNames.Contains(gpuName) Then
            _allGpuNames.Add(gpuName)
        End If
    End Sub

    Private Shared Sub UpdateGpuFlagsFromName(gpuName As String, Optional fromRegistry As Boolean = False)
        Dim upper As String = gpuName.ToUpperInvariant()
        Dim source As String = If(fromRegistry, " via Registry", "")

        If ContainsAny(upper, NvidiaKeywords) Then
            If Not _hasNvidia.GetValueOrDefault(False) Then
                _hasNvidia = True
            End If
            Debug.WriteLine("  NVIDIA detected" & source & ": " & gpuName)
            Return
        End If

        If ContainsAny(upper, AmdKeywords) Then
            If Not _hasAMD.GetValueOrDefault(False) Then
                _hasAMD = True
            End If
            Debug.WriteLine("  AMD detected" & source & ": " & gpuName)
            Return
        End If

        If ContainsAny(upper, IntelKeywords) AndAlso Not ContainsAny(upper, NvidiaKeywords) AndAlso Not ContainsAny(upper, AmdKeywords) Then
            If fromRegistry AndAlso Not ContainsAny(upper, IntelIGpuKeywords) Then
                Return
            End If

            If Not _hasIntel.GetValueOrDefault(False) Then
                _hasIntel = True
                _intelGpuName = gpuName
            End If
            Debug.WriteLine("  Intel detected" & source & ": " & gpuName)
        End If
    End Sub

    Private Shared Function ContainsAny(value As String, keywords As IEnumerable(Of String)) As Boolean
        For Each keyword As String In keywords
            If value.IndexOf(keyword, StringComparison.Ordinal) >= 0 Then
                Return True
            End If
        Next
        Return False
    End Function

#End Region

#Region "Reset to Defaults"

    ''' <summary>
    ''' Reset all settings to default values
    ''' </summary>
    Public Sub ResetDefaults()
        Recording = New RecordingSettingsClass()
        Paths = New PathSettingsClass()
        UI = New UISettingsClass()
        Audio = New AudioSettingsClass()

        ' <<< เปลี่ยนจาก HotkeySettingsClass เป็น Dictionary >>>
        Hotkeys = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        ' ✅ ไม่ reset GitHub user
        Save()
    End Sub

#End Region

End Class