Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Http.Headers
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Security.Cryptography.ProtectedData
Imports System.Text

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
        Public Property Bitrate As Integer = 20000
        Public Property Width As Integer = 1920
        Public Property Height As Integer = 1080
        Public Property Preset As String = "Medium"
        Public Property EncoderPreset As Integer = 4
        Public Property ReplayDuration As Integer = 60

        ' ═══ My Preset saved values (Nothing = use default) ═══
        Public Property MyLowFPS As Integer? = Nothing
        Public Property MyLowBitrate As Integer? = Nothing
        Public Property MyLowEncoderPreset As Integer? = Nothing

        Public Property MyMediumFPS As Integer? = Nothing
        Public Property MyMediumBitrate As Integer? = Nothing
        Public Property MyMediumEncoderPreset As Integer? = Nothing

        Public Property MyHighFPS As Integer? = Nothing
        Public Property MyHighBitrate As Integer? = Nothing
        Public Property MyHighEncoderPreset As Integer? = Nothing

        ''' <summary>Custom renameable name for MY preset group (e.g. "P4", "P6")</summary>
        Public Property MyPresetName As String = "MY"

        ''' <summary>Capture API: ddagrab, gfxcapture, GDIGrab, or null (auto)</summary>
        Public Property APICapture As String = Nothing

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
    ''' Audio settings: system audio, microphone, volume
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
    ''' GitHub User settings - เก็บข้อมูลผู้ใช้ GitHub
    ''' </summary>
    Public Class GitHubUserClass
        Public Property Username As String = ""
        Public Property AvatarUrl As String = ""
        Public Property IsLoggedIn As Boolean = False
        Public Property LastLogin As DateTime = DateTime.MinValue

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' GitHub Token สำหรับ OAuth
    ''' ✅ P1: stored encrypted (DPAPI, CurrentUser scope) in config.json as
    ''' GitHubTokenEncrypted. Never written to disk as plain text. The plain
    ''' GitHubToken property below is computed (decrypt-on-read) and only
    ''' exists in memory. On first load after upgrade, an old plain-text
    ''' GitHubToken value is automatically migrated to encrypted form.
    ''' </summary>
    Public Property GitHubUser As New GitHubUserClass()

    ''' <summary>Encrypted GitHub token (Base64 of DPAPI-protected bytes). Persisted.</summary>
    <JsonPropertyName("GitHubTokenEncrypted")>
    Public Property GitHubTokenEncrypted As String = ""

    ''' <summary>
    ''' Plain-text GitHub token. NOT serialized to JSON (JsonIgnore on the
    ''' backing field below). Reading it decrypts GitHubTokenEncrypted; writing
    ''' it encrypts and stores in GitHubTokenEncrypted. On failed decrypt the
    ''' getter returns "" (treats token as missing).
    ''' </summary>
    <JsonIgnore>
    Public Property GitHubToken As String
        Get
            Return DecryptToken(GitHubTokenEncrypted)
        End Get
        Set(value As String)
            GitHubTokenEncrypted = EncryptToken(value)
        End Set
    End Property

    ' ── DPAPI helpers ─────────────────────────────────────────
    Private Shared Function EncryptToken(plain As String) As String
        If String.IsNullOrEmpty(plain) Then Return ""
        Try
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(plain)
            Dim cipher As Byte() = ProtectedData.Protect(bytes, Nothing, DataProtectionScope.CurrentUser)
            Return Convert.ToBase64String(cipher)
        Catch ex As Exception
            Debug.WriteLine($"EncryptToken failed: {ex.Message}")
            Return ""
        End Try
    End Function

    Private Shared Function DecryptToken(cipherB64 As String) As String
        If String.IsNullOrEmpty(cipherB64) Then Return ""
        Try
            Dim cipher As Byte() = Convert.FromBase64String(cipherB64)
            Dim plain As Byte() = ProtectedData.Unprotect(cipher, Nothing, DataProtectionScope.CurrentUser)
            Return Encoding.UTF8.GetString(plain)
        Catch ex As Exception
            ' Wrong user, corrupted, or it's actually a legacy plain-text token.
            Debug.WriteLine($"DecryptToken failed: {ex.Message}")
            Return ""
        End Try
    End Function

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

#Region "JSON File Paths"

    Private _configPath As String = Nothing
    Private _videoConfigPath As String = Nothing

    ''' <summary>
    ''' Path to config.json (general overlay settings)
    ''' </summary>
    Private ReadOnly Property ConfigPath As String
        Get
            If _configPath Is Nothing Then
                _configPath = Path.Combine(Application.StartupPath, "config.json")
            End If
            Return _configPath
        End Get
    End Property

    ''' <summary>
    ''' Path to video.json (video capture settings — saved on exit from Video Capture page)
    ''' </summary>
    Public ReadOnly Property VideoConfigPath As String
        Get
            If _videoConfigPath Is Nothing Then
                _videoConfigPath = Path.Combine(Application.StartupPath, "video.json")
            End If
            Return _videoConfigPath
        End Get
    End Property

#End Region

#Region "config.json — Load / Save"

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

                        ' ✅ P1: Migration — if the JSON on disk still has a legacy
                        ' plain-text GitHubToken field (from before DPAPI encryption),
                        ' encrypt it and store it as GitHubTokenEncrypted, then trigger
                        ' a save so the plain-text field is wiped from disk.
                        ' The <JsonIgnore> on the GitHubToken setter means it never
                        ' deserializes directly — we have to peek at the raw JSON.
                        Dim legacyPlainToken As String = TryGetLegacyPlainToken(json)
                        If Not String.IsNullOrEmpty(legacyPlainToken) Then
                            GitHubToken = legacyPlainToken  ' triggers EncryptToken via setter
                            Debug.WriteLine("AppSettings.Load: migrated legacy plain-text GitHubToken → encrypted")
                            Try
                                Save()  ' persist the encrypted form and wipe the plain-text field
                            Catch ex As Exception
                                Debug.WriteLine("AppSettings.Load: migration save failed: " & ex.Message)
                            End Try
                        End If

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

    ''' <summary>
    ''' Peek at the raw JSON to find a legacy plain-text "GitHubToken" field.
    ''' Returns "" if not present or if the value is empty.
    ''' </summary>
    Private Shared Function TryGetLegacyPlainToken(json As String) As String
        Try
            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = doc.RootElement
                Dim tok As JsonElement
                ' PropertyNameCaseInsensitive at the parser level is not a thing —
                ' check both common casings explicitly.
                If root.TryGetProperty("GitHubToken", tok) AndAlso tok.ValueKind = JsonValueKind.String Then
                    Dim s As String = tok.GetString()
                    If Not String.IsNullOrEmpty(s) Then Return s
                End If
                If root.TryGetProperty("githubtoken", tok) AndAlso tok.ValueKind = JsonValueKind.String Then
                    Dim s As String = tok.GetString()
                    If Not String.IsNullOrEmpty(s) Then Return s
                End If
            End Using
        Catch
        End Try
        Return ""
    End Function

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

        ' ✅ P1: copy the encrypted token directly (the plain GitHubToken property
        ' is <JsonIgnore> so the deserializer can't set it; copying the encrypted
        ' form preserves the value without a decrypt-then-encrypt round-trip that
        ' could subtly corrupt the bytes). Legacy plain-text migration is handled
        ' separately in Load() via TryGetLegacyPlainToken.
        GitHubTokenEncrypted = loaded.GitHubTokenEncrypted
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

        ' ═══ My Preset saved values (ถ้าไม่มีใน config.json เก่า จะเป็น Nothing → ใช้ default) ═══
        Recording.MyLowFPS = loadedRecording.MyLowFPS
        Recording.MyLowBitrate = loadedRecording.MyLowBitrate
        Recording.MyLowEncoderPreset = loadedRecording.MyLowEncoderPreset

        Recording.MyMediumFPS = loadedRecording.MyMediumFPS
        Recording.MyMediumBitrate = loadedRecording.MyMediumBitrate
        Recording.MyMediumEncoderPreset = loadedRecording.MyMediumEncoderPreset

        Recording.MyHighFPS = loadedRecording.MyHighFPS
        Recording.MyHighBitrate = loadedRecording.MyHighBitrate
        Recording.MyHighEncoderPreset = loadedRecording.MyHighEncoderPreset
        Recording.MyPresetName = loadedRecording.MyPresetName
        Recording.APICapture = loadedRecording.APICapture
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

#Region "video.json — Load / Save (TCP Architecture)"

    ''' <summary>
    ''' Video capture settings สำหรับ Engine — ส่งผ่าน TCP
    ''' ถูก Save ตอนปิด Video Capture page และ Load ตอนเปิดหน้า
    ''' 
    ''' JSON structure:
    ''' {
    '''   "encoder": "NVENC_H264",
    '''   "active_preset": "my_medium",
    '''   "current": { "fps": 60, "bitrate": 9000, "encoder_preset": 4, ... },
    '''   "replay_duration": 60,
    '''   "audio": { "system_enabled": true, "mic_enabled": false, ... },
    '''   "my_presets": { "low": {...}, "medium": {...}, "high": {...} }
    ''' }
    ''' </summary>
    Public Class VideoConfigClass
        ''' <summary>Encoder string: NVENC_H264, NVENC_HEVC, QuickSync_H264, etc.</summary>
        Public Property Encoder As String = "NVENC_H264"

        ''' <summary>Encoder currently in use (may differ from Encoder during transitions)</summary>
        Public Property EncoderNow As String = "NVENC_H264"

        ''' <summary>Currently selected preset name (e.g. "Medium", "MyLow", "Custom")</summary>
        Public Property ActivePreset As String = "Medium"

        ''' <summary>Current actual recording values being used</summary>
        Public Property Current As New VideoCurrentValuesClass()

        ''' <summary>Replay buffer duration in seconds</summary>
        Public Property ReplayDuration As Integer = 60

        ''' <summary>Audio settings</summary>
        Public Property Audio As New VideoAudioConfigClass()

        ''' <summary>MY preset definitions (low/medium/high)</summary>
        Public Property MyPresets As New VideoMyPresetsClass()

        ''' <summary>Capture API: ddagrab, gfxcapture, GDIGrab, or null (auto)</summary>
        Public Property APICapture As String = Nothing

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' Current actual recording values (nested under "current" in video.json)
    ''' </summary>
    Public Class VideoCurrentValuesClass
        Public Property FPS As Integer = 60
        Public Property Bitrate As Integer = 20000
        Public Property EncoderPreset As Integer = 4
        Public Property UseNativeResolution As Boolean = True
        Public Property Width As Integer = 0
        Public Property Height As Integer = 0

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' Audio settings (nested under "audio" in video.json)
    ''' </summary>
    Public Class VideoAudioConfigClass
        Public Property SystemEnabled As Boolean = True
        Public Property MicEnabled As Boolean = False
        Public Property SystemVolume As Single = 1.0F
        Public Property MicVolume As Single = 1.0F
        Public Property MicDevice As String = ""

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' Single MY preset slot (low / medium / high)
    ''' </summary>
    Public Class MyPresetSlotClass
        ''' <summary>FPS (Nothing = use NVIDIA default)</summary>
        Public Property FPS As Integer? = Nothing

        ''' <summary>Bitrate in kbps (Nothing = use NVIDIA default)</summary>
        Public Property Bitrate As Integer? = Nothing

        ''' <summary>Encoder preset index 1-7 (Nothing = use NVIDIA default)</summary>
        Public Property EncoderPreset As Integer? = Nothing

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' MY preset container (nested under "my_presets" in video.json)
    ''' </summary>
    Public Class VideoMyPresetsClass
        ''' <summary>Custom renameable name for MY preset group (e.g. "P4", "P6")</summary>
        Public Property Name As String = "MY"

        Public Property Low As New MyPresetSlotClass()
        Public Property Medium As New MyPresetSlotClass()
        Public Property High As New MyPresetSlotClass()

        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' โหลด video settings จาก video.json
    ''' </summary>
    Public Function LoadVideoSettings() As VideoConfigClass
        Try
            Debug.WriteLine("══════════ AppSettings.LoadVideoSettings ══════════")

            If File.Exists(VideoConfigPath) Then
                Dim json As String = File.ReadAllText(VideoConfigPath)

                If Not String.IsNullOrWhiteSpace(json) Then
                    Dim options As New JsonSerializerOptions With {
                        .PropertyNameCaseInsensitive = True,
                        .AllowTrailingCommas = True,
                        .ReadCommentHandling = JsonCommentHandling.Skip
                    }

                    Dim loaded As VideoConfigClass = JsonSerializer.Deserialize(Of VideoConfigClass)(json, options)

                    If loaded IsNot Nothing Then
                        Debug.WriteLine("AppSettings.LoadVideoSettings: SUCCESS")
                        Debug.WriteLine($"  Encoder: {loaded.Encoder}, ActivePreset: {loaded.ActivePreset}")
                        Debug.WriteLine($"  FPS: {loaded.Current.FPS}, Bitrate: {loaded.Current.Bitrate}, Res: {loaded.Current.Width}x{loaded.Current.Height}")
                        Return loaded
                    End If
                End If
            Else
                Debug.WriteLine("AppSettings.LoadVideoSettings: video.json not found, using defaults")
            End If

        Catch ex As Exception
            Debug.WriteLine("AppSettings.LoadVideoSettings Error: " & ex.Message)
        End Try

        ' Return defaults
        Return Nothing
    End Function

    ''' <summary>
    ''' Save video settings to video.json — เรียกตอนออกจาก Video Capture page
    ''' </summary>
    Public Sub SaveVideoSettings(video As VideoConfigClass)
        Try
            If video Is Nothing Then Return

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True,
                .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }

            Dim json As String = JsonSerializer.Serialize(video, options)
            File.WriteAllText(VideoConfigPath, json)

            Debug.WriteLine("AppSettings.SaveVideoSettings: Saved to " & VideoConfigPath)
            Debug.WriteLine($"  Encoder: {video.Encoder}, ActivePreset: {video.ActivePreset}")
            Debug.WriteLine($"  FPS: {video.Current.FPS}, Bitrate: {video.Current.Bitrate}")

        Catch ex As Exception
            Debug.WriteLine("AppSettings.SaveVideoSettings Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Sync current Recording + Audio settings → VideoConfigClass and save to video.json
    ''' เรียกจาก Video Capture page เมื่อกดปิด/ออก
    ''' 
    ''' Produces JSON:
    ''' {
    '''   "encoder": "NVENC_H264",
    '''   "active_preset": "MyMedium",
    '''   "current": { "fps": 60, "bitrate": 9000, "encoder_preset": 4, ... },
    '''   "replay_duration": 60,
    '''   "audio": { "system_enabled": true, ... },
    '''   "my_presets": { "low": {...}, "medium": {...}, "high": {...} }
    ''' }
    ''' </summary>
    Public Sub SyncAndSaveVideoConfig()
        Dim video As New VideoConfigClass()

        ' ═══ Top-level ═══
        video.Encoder = Recording.Encoder
        video.EncoderNow = Recording.EncoderNow
        video.ActivePreset = Recording.Preset
        video.ReplayDuration = Recording.ReplayDuration

        ' ═══ My Preset name ═══
        video.MyPresets.Name = Recording.MyPresetName

        ' ═══ API Capture ═══
        video.APICapture = Recording.APICapture

        ' ═══ Current values (nested) ═══
        video.Current.FPS = Recording.FPS
        video.Current.Bitrate = Recording.Bitrate
        video.Current.EncoderPreset = Recording.EncoderPreset
        video.Current.UseNativeResolution = Recording.UseNativeResolution
        video.Current.Width = Recording.Width
        video.Current.Height = Recording.Height

        ' ═══ Audio settings (nested) ═══
        video.Audio.SystemEnabled = Audio.SystemAudioEnabled
        video.Audio.MicEnabled = Audio.MicEnabled
        video.Audio.SystemVolume = Audio.SystemAudioVolume
        video.Audio.MicVolume = Audio.MicVolume
        video.Audio.MicDevice = Audio.MicDeviceName

        ' ═══ My Preset saved values (nested) ═══
        video.MyPresets.Low.FPS = Recording.MyLowFPS
        video.MyPresets.Low.Bitrate = Recording.MyLowBitrate
        video.MyPresets.Low.EncoderPreset = Recording.MyLowEncoderPreset

        video.MyPresets.Medium.FPS = Recording.MyMediumFPS
        video.MyPresets.Medium.Bitrate = Recording.MyMediumBitrate
        video.MyPresets.Medium.EncoderPreset = Recording.MyMediumEncoderPreset

        video.MyPresets.High.FPS = Recording.MyHighFPS
        video.MyPresets.High.Bitrate = Recording.MyHighBitrate
        video.MyPresets.High.EncoderPreset = Recording.MyHighEncoderPreset

        ' Save to video.json
        SaveVideoSettings(video)

        ' Also update config.json (keeps both in sync)
        Save()
    End Sub

    ''' <summary>
    ''' Load video.json and apply to Recording + Audio properties
    ''' เรียกจาก Video Capture page เมื่อเปิดหน้า
    ''' 
    ''' Reads nested JSON:
    ''' { "encoder", "active_preset", "current": {...}, "replay_duration", "audio": {...}, "my_presets": {...} }
    ''' </summary>
    Public Sub LoadAndApplyVideoConfig()
        Dim video = LoadVideoSettings()

        If video Is Nothing Then
            Debug.WriteLine("LoadAndApplyVideoConfig: No video.json, using current config.json values")
            Return
        End If

        ' ═══ Top-level fields ═══
        Recording.Encoder = video.Encoder
        Recording.EncoderNow = video.EncoderNow
        Recording.Preset = video.ActivePreset
        Recording.ReplayDuration = video.ReplayDuration

        ' ═══ My Preset name ═══
        Recording.MyPresetName = video.MyPresets.Name

        ' ═══ API Capture ═══
        Recording.APICapture = video.APICapture

        ' ═══ Current values (nested) ═══
        Recording.FPS = video.Current.FPS
        Recording.Bitrate = video.Current.Bitrate
        Recording.EncoderPreset = video.Current.EncoderPreset
        Recording.UseNativeResolution = video.Current.UseNativeResolution
        Recording.Width = If(video.Current.Width = 0, Recording.Width, video.Current.Width)
        Recording.Height = If(video.Current.Height = 0, Recording.Height, video.Current.Height)

        ' ═══ Audio settings (nested) ═══
        Audio.SystemAudioEnabled = video.Audio.SystemEnabled
        Audio.MicEnabled = video.Audio.MicEnabled
        Audio.SystemAudioVolume = video.Audio.SystemVolume
        Audio.MicVolume = video.Audio.MicVolume
        Audio.MicDeviceName = video.Audio.MicDevice

        ' ═══ My Preset values (nested) ═══
        Recording.MyLowFPS = video.MyPresets.Low.FPS
        Recording.MyLowBitrate = video.MyPresets.Low.Bitrate
        Recording.MyLowEncoderPreset = video.MyPresets.Low.EncoderPreset

        Recording.MyMediumFPS = video.MyPresets.Medium.FPS
        Recording.MyMediumBitrate = video.MyPresets.Medium.Bitrate
        Recording.MyMediumEncoderPreset = video.MyPresets.Medium.EncoderPreset

        Recording.MyHighFPS = video.MyPresets.High.FPS
        Recording.MyHighBitrate = video.MyPresets.High.Bitrate
        Recording.MyHighEncoderPreset = video.MyPresets.High.EncoderPreset

        ' Also save to config.json (keeps both in sync)
        Save()

        Debug.WriteLine("LoadAndApplyVideoConfig: Applied video.json → Recording + Audio properties")
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

        ' Also reset video.json
        If File.Exists(VideoConfigPath) Then
            Try
                File.Delete(VideoConfigPath)
                Debug.WriteLine("ResetDefaults: Deleted video.json")
            Catch ex As Exception
                Debug.WriteLine("ResetDefaults: Failed to delete video.json - " & ex.Message)
            End Try
        End If
    End Sub

#End Region

End Class
