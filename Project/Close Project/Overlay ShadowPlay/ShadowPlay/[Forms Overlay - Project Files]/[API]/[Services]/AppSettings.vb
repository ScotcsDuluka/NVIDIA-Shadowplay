

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

Partial Public Class AppSettings
#Region "JSON Model Classes"

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

        Public Property MyLowFPS As Integer? = Nothing
        Public Property MyLowBitrate As Integer? = Nothing
        Public Property MyLowEncoderPreset As Integer? = Nothing

        Public Property MyMediumFPS As Integer? = Nothing
        Public Property MyMediumBitrate As Integer? = Nothing
        Public Property MyMediumEncoderPreset As Integer? = Nothing

        Public Property MyHighFPS As Integer? = Nothing
        Public Property MyHighBitrate As Integer? = Nothing
        Public Property MyHighEncoderPreset As Integer? = Nothing

        Public Property MyPresetName As String = "MY"

        Public Property EngineMode As String = Nothing

        Public Property APICapture As String = Nothing
    End Class

    Public Class PathSettingsClass
        Public Property GalleryPath As String = ""
        Public Property SavePath As String = ""
        Public Property FFmpegPath As String = ""
    End Class

    Public Class UISettingsClass
        Public Property Language As String = "en-US"
        Public Property Theme As String = "Dark"
        Public Property UseWindowsSnip As Boolean = False
    End Class

    Public Class AudioSettingsClass

        Public Property SystemAudioEnabled As Boolean = True

        Public Property MicEnabled As Boolean = False

        Public Property SystemAudioVolume As Single = 1.0F

        Public Property MicVolume As Single = 1.0F

        Public Property MicDeviceName As String = ""

        Public Property MicDeviceId As String = ""

        Public Property TrackMode As Integer = 0

        Public Property AudioClockMode As String = "Legacy"

        <Obsolete("Use MicVolume instead")>
        <JsonIgnore>
        Public Property MicVolumePercent As Integer
            Get
                Return CInt(MicVolume * 100)
            End Get
            Set(value As Integer)
                MicVolume = Math.Max(0, Math.Min(100, value)) / 100.0F
            End Set
        End Property

        <Obsolete("Use SystemAudioVolume instead")>
        <JsonIgnore>
        Public Property SystemVolumePercent As Integer
            Get
                Return CInt(SystemAudioVolume * 100)
            End Get
            Set(value As Integer)
                SystemAudioVolume = Math.Max(0, Math.Min(100, value)) / 100.0F
            End Set
        End Property
    End Class

    Public Class PrivacySettingsClass
        
        Public Property DesktopCaptureEnabled As Boolean = False
    End Class

    Public Class OverlaySettingsClass
        
        Public Property UseOverlayEnabled As Boolean = False
    End Class

    Public Class NotificationsSettingsClass
        
        Public Property RecordingStarted As Boolean = True
        Public Property RecordingSaved As Boolean = True
        Public Property RecordingError As Boolean = True
        
        Public Property ReplaySaved As Boolean = True
        Public Property InstantReplayOn As Boolean = True
        Public Property InstantReplayOff As Boolean = True
        Public Property ReplayTurnOn As Boolean = True
        Public Property ReplayError As Boolean = True
        
        Public Property ScreenshotSaved As Boolean = True
        Public Property ValidSavePath As Boolean = True
        
        Public Property OpenShare As Boolean = True
        
        Public Property RamWarning As Boolean = True
        Public Property RamWarning95 As Boolean = True
        Public Property RamCritical As Boolean = True
        Public Property CpuWarning As Boolean = True
        Public Property DiskSpaceLow As Boolean = True
        
        Public Property UpdateAvailable As Boolean = True
        Public Property VersionLatest As Boolean = True
        Public Property UpdateError As Boolean = True
        
        Public Property AccountConfirmError As Boolean = True
        Public Property ExtensionNotFound As Boolean = True
        Public Property FeatureNotReady As Boolean = True
        Public Property GpuRequired As Boolean = True
        Public Property EngineNotRunning As Boolean = True
        Public Property EngineUIInUse As Boolean = True
        Public Property ErrorResolution As Boolean = True
        Public Property DesktopCaptureDisabled As Boolean = True

        Public Property SlotCount As Integer = 2
    End Class

    Public Class GitHubUserClass
        Public Property Username As String = ""
        Public Property AvatarUrl As String = ""
        Public Property IsLoggedIn As Boolean = False
        Public Property LastLogin As DateTime = DateTime.MinValue
    End Class

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
            
            Debug.WriteLine($"DecryptToken failed: {ex.Message}")
            Return ""
        End Try
    End Function
#End Region

#Region "Config Sections (Recording / Paths / UI / Audio / Privacy / Overlay / Hotkeys)"
    Public Property Recording As New RecordingSettingsClass()
    Public Property Paths As New PathSettingsClass()
    Public Property UI As New UISettingsClass()
    Public Property Audio As New AudioSettingsClass()
    Public Property Privacy As New PrivacySettingsClass()
    Public Property Overlay As New OverlaySettingsClass()
    Public Property Notifications As New NotificationsSettingsClass()

    Public Property Hotkeys As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    Public Property GitHubUser As New GitHubUserClass()

    <JsonPropertyName("GitHubTokenEncrypted")>
    Public Property GitHubTokenEncrypted As String = ""

    <JsonIgnore>
    Public Property GitHubToken As String
        Get
            Return DecryptToken(GitHubTokenEncrypted)
        End Get
        Set(value As String)
            GitHubTokenEncrypted = EncryptToken(value)
        End Set
    End Property
#End Region

#Region "Config file DTO — Recording stored video.json-shaped (the detailed file schema)"

    Public Class VideoCurrentDto
        Public Property fps As Integer = 60
        Public Property bitrate As Integer = 20000
        Public Property encoder_preset As Integer = 4
        Public Property use_native_resolution As Boolean = True
        Public Property width As Integer = 1920
        Public Property height As Integer = 1080
    End Class

    Public Class MyPresetSlotDto
        Public Property fps As Integer? = Nothing
        Public Property bitrate As Integer? = Nothing
        Public Property encoder_preset As Integer? = Nothing
    End Class

    Public Class MyPresetsDto
        
        Public Property name As String = "MY"
        Public Property low As MyPresetSlotDto = New MyPresetSlotDto()
        Public Property medium As MyPresetSlotDto = New MyPresetSlotDto()
        Public Property high As MyPresetSlotDto = New MyPresetSlotDto()
    End Class

    Public Class RecordingSectionDto
        
        Public Property encoder As String = "NVENC_H264"
        
        Public Property encoder_now As String = "NVENC_H264"
        
        Public Property active_preset As String = "Medium"
        Public Property current As VideoCurrentDto = New VideoCurrentDto()
        
        Public Property replay_duration As Integer = 60
        Public Property my_presets As MyPresetsDto = New MyPresetsDto()
        
        Public Property engine_mode As String = Nothing
        
        Public Property api_capture As String = Nothing
    End Class

    Public Class ConfigFileDto
        Public Property Recording As RecordingSectionDto = New RecordingSectionDto()
        Public Property Paths As PathSettingsClass
        Public Property UI As UISettingsClass
        Public Property Audio As AudioSettingsClass
        Public Property Privacy As PrivacySettingsClass
        Public Property Overlay As OverlaySettingsClass
        Public Property Notifications As NotificationsSettingsClass
        Public Property Hotkeys As Dictionary(Of String, String)
        Public Property GitHubUser As GitHubUserClass
        Public Property GitHubTokenEncrypted As String
    End Class
#End Region

#Region "Singleton"
    Private Shared _instance As AppSettings = Nothing
    Private Shared ReadOnly _lock As New Object()

    Private Shared ReadOnly _saveLock As New Object()
    Private Shared _isLoaded As Boolean = False

    Private Shared _hardwareDetected As Boolean = False

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

    Public Shared ReadOnly Property HardwareDetected As Boolean
        Get
            Return _hardwareDetected
        End Get
    End Property

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

    Private _configWasMissingOnLoad As Boolean = False

    Private ReadOnly Property ConfigPath As String
        Get
            If _configPath Is Nothing Then
                _configPath = AppLayout.P("Config", "config.json")
            End If
            Return _configPath
        End Get
    End Property

    Private ReadOnly Property VideoConfigPath As String
        Get
            If _videoConfigPath Is Nothing Then
                _videoConfigPath = AppLayout.P("Config", "video.json")
            End If
            Return _videoConfigPath
        End Get
    End Property
#End Region

#Region "config.json — Load / Save"

    Public Sub Load()
        Try
            Debug.WriteLine("══════════ AppSettings.Load ══════════")
            Debug.WriteLine("ConfigPath: " & ConfigPath)

            If File.Exists(ConfigPath) Then
                Dim json As String = File.ReadAllText(ConfigPath)
                Dim applied As Boolean = False
                Dim recoveredFromBackup As Boolean = False

                If Not String.IsNullOrWhiteSpace(json) Then
                    applied = TryApplyConfigJson(json)
                End If

                If Not applied Then
                    Dim bakJson As String = TryReadBackupText()
                    If bakJson IsNot Nothing Then
                        applied = TryApplyConfigJson(bakJson)
                        If applied Then
                            json = bakJson
                            recoveredFromBackup = True
                        End If
                    End If
                End If

                If applied Then

                    If recoveredFromBackup OrElse Not ConfigJsonIsNested(json) Then
                        Save()
                    End If

                    Dim legacyPlainToken As String = TryGetLegacyPlainToken(json)
                    If Not String.IsNullOrEmpty(legacyPlainToken) Then
                        GitHubToken = legacyPlainToken  
                        Debug.WriteLine("AppSettings.Load: migrated legacy plain-text GitHubToken → encrypted")
                        Try
                            Save()  
                        Catch ex As Exception
                            Debug.WriteLine("AppSettings.Load: migration save failed: " & ex.Message)
                        End Try
                    End If

                    Debug.WriteLine("AppSettings.Load: SUCCESS")
                    Debug.WriteLine($"  GitHub User: {GitHubUser.Username}")
                    Debug.WriteLine($"  GitHub Logged In: {GitHubUser.IsLoggedIn}")
                Else
                    Debug.WriteLine("AppSettings.Load: config.json unreadable and no usable .bak — running on defaults")
                End If
            Else
                
                _configWasMissingOnLoad = True
                Save()
                Debug.WriteLine("AppSettings.Load: Created default config")
            End If

            MigrateLegacyConfigFiles()

            MigrateLegacyMarkerFiles()

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load Error: " & ex.Message)
        End Try
    End Sub

#Region "Legacy marker-file migration (privacy / Use_Overlay / current.txt)"

    Private Sub MigrateLegacyMarkerFiles()
        Dim changed As Boolean = False

        Dim privacyMarker As String = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "privacy")
        If File.Exists(privacyMarker) Then
            Privacy.DesktopCaptureEnabled = True
            AppLayout.DeleteFileIfExists(privacyMarker)
            changed = True
            Debug.WriteLine("[Migrate] privacy marker → config.json Privacy.DesktopCaptureEnabled = True")
        End If

        Dim useOverlayFlag As String = AppLayout.P("Flags", "Use_Overlay")
        If File.Exists(useOverlayFlag) Then
            Overlay.UseOverlayEnabled = True
            AppLayout.DeleteFileIfExists(useOverlayFlag)
            changed = True
            Debug.WriteLine("[Migrate] Flags/Use_Overlay → config.json Overlay.UseOverlayEnabled = True")
        End If

        Dim currentTxt As String = AppLayout.P("Languages", "current.txt")
        If File.Exists(currentTxt) Then
            Try
                Dim code As String = File.ReadAllText(currentTxt).Trim()
                Dim langJson As String = AppLayout.P("Languages", code & ".json")
                If code.Length > 0 AndAlso code.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 AndAlso File.Exists(langJson) Then
                    UI.Language = code
                    Debug.WriteLine("[Migrate] Languages/current.txt → config.json UI.Language = " & code)
                End If
            Catch ex As Exception
                Debug.WriteLine("[Migrate] current.txt import failed: " & ex.Message)
            End Try
            AppLayout.DeleteFileIfExists(currentTxt)
            changed = True
        End If

        If changed Then Save()
    End Sub
#End Region

    Private Shared Function TryGetLegacyPlainToken(json As String) As String
        Try
            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = doc.RootElement
                Dim tok As JsonElement

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

    Private Function TryDeserializeConfig(json As String) As AppSettings
        Try
            Dim options As New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True,
                .AllowTrailingCommas = True,
                .ReadCommentHandling = JsonCommentHandling.Skip
            }
            Return JsonSerializer.Deserialize(Of AppSettings)(json, options)
        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load: config.json parse failed: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Function TryApplyConfigJson(json As String) As Boolean
        If ConfigJsonIsNested(json) Then
            Dim dto As ConfigFileDto = TryDeserializeConfigDto(json)
            If dto Is Nothing Then Return False
            ApplyDtoSettings(dto)
        Else
            Dim loaded As AppSettings = TryDeserializeConfig(json)
            If loaded Is Nothing Then Return False
            ApplyLoadedSettings(loaded)
        End If
        Return True
    End Function

    Private Shared Function ConfigJsonIsNested(json As String) As Boolean
        Try
            Dim docOpts As New JsonDocumentOptions With {
                .AllowTrailingCommas = True,
                .CommentHandling = JsonCommentHandling.Skip
            }
            Using doc As JsonDocument = JsonDocument.Parse(json, docOpts)
                Dim root As JsonElement = doc.RootElement
                If root.ValueKind <> JsonValueKind.Object Then Return False
                Dim sec As JsonElement
                If Not root.TryGetProperty("Recording", sec) AndAlso
                   Not root.TryGetProperty("recording", sec) Then Return False
                If sec.ValueKind <> JsonValueKind.Object Then Return False
                Dim cur As JsonElement
                Return sec.TryGetProperty("current", cur) AndAlso cur.ValueKind = JsonValueKind.Object
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Shared Function TryDeserializeConfigDto(json As String) As ConfigFileDto
        Try
            Dim options As New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True,
                .AllowTrailingCommas = True,
                .ReadCommentHandling = JsonCommentHandling.Skip
            }
            Return JsonSerializer.Deserialize(Of ConfigFileDto)(json, options)
        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load: config.json (nested) parse failed: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Function TryReadBackupText() As String
        Try
            Dim bakPath As String = ConfigPath & ".bak"
            If Not File.Exists(bakPath) Then Return Nothing
            Dim bakJson As String = File.ReadAllText(bakPath)
            If String.IsNullOrWhiteSpace(bakJson) Then Return Nothing
            Return bakJson
        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load: backup read failed: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Sub ApplyDtoSettings(dto As ConfigFileDto)
        If dto Is Nothing Then Return

        ApplyRecordingDto(dto.Recording)
        ApplyPathSettings(dto.Paths)
        ApplyUISettings(dto.UI)
        ApplyAudioSettings(dto.Audio)
        ApplyGitHubUserSettings(dto.GitHubUser)
        ApplyPrivacySettings(dto.Privacy)
        ApplyOverlaySettings(dto.Overlay)
        ApplyNotificationsSettings(dto.Notifications)

        If dto.Hotkeys IsNot Nothing Then
            Hotkeys = New Dictionary(Of String, String)(dto.Hotkeys, StringComparer.OrdinalIgnoreCase)
        End If

        GitHubTokenEncrypted = dto.GitHubTokenEncrypted
    End Sub

    Private Function BuildRecordingDto() As RecordingSectionDto
        Dim d As New RecordingSectionDto()
        d.encoder = Recording.Encoder
        d.encoder_now = Recording.EncoderNow
        d.active_preset = Recording.Preset
        d.current = New VideoCurrentDto With {
            .fps = Recording.FPS,
            .bitrate = Recording.Bitrate,
            .encoder_preset = Recording.EncoderPreset,
            .use_native_resolution = Recording.UseNativeResolution,
            .width = Recording.Width,
            .height = Recording.Height
        }
        d.replay_duration = Recording.ReplayDuration
        d.my_presets = New MyPresetsDto With {
            .name = Recording.MyPresetName,
            .low = BuildPresetSlotDto(Recording.MyLowFPS, Recording.MyLowBitrate, Recording.MyLowEncoderPreset),
            .medium = BuildPresetSlotDto(Recording.MyMediumFPS, Recording.MyMediumBitrate, Recording.MyMediumEncoderPreset),
            .high = BuildPresetSlotDto(Recording.MyHighFPS, Recording.MyHighBitrate, Recording.MyHighEncoderPreset)
        }
        d.engine_mode = Recording.EngineMode
        d.api_capture = Recording.APICapture
        Return d
    End Function

    Private Shared Function BuildPresetSlotDto(fps As Integer?, bitrate As Integer?, encoderPreset As Integer?) As MyPresetSlotDto
        Return New MyPresetSlotDto With {.fps = fps, .bitrate = bitrate, .encoder_preset = encoderPreset}
    End Function

    Private Sub ApplyRecordingDto(d As RecordingSectionDto)
        If d Is Nothing Then Return

        If Not String.IsNullOrEmpty(d.encoder) Then Recording.Encoder = d.encoder
        If Not String.IsNullOrEmpty(d.encoder_now) Then Recording.EncoderNow = d.encoder_now
        If Not String.IsNullOrEmpty(d.active_preset) Then Recording.Preset = d.active_preset

        If d.current IsNot Nothing Then
            If d.current.fps > 0 Then Recording.FPS = d.current.fps
            If d.current.bitrate > 0 Then Recording.Bitrate = d.current.bitrate
            If d.current.encoder_preset > 0 Then Recording.EncoderPreset = d.current.encoder_preset
            Recording.UseNativeResolution = d.current.use_native_resolution
            If d.current.width > 0 Then Recording.Width = d.current.width
            If d.current.height > 0 Then Recording.Height = d.current.height
        End If

        If d.replay_duration > 0 Then Recording.ReplayDuration = d.replay_duration

        If d.my_presets IsNot Nothing Then
            If Not String.IsNullOrEmpty(d.my_presets.name) Then Recording.MyPresetName = d.my_presets.name
            If d.my_presets.low IsNot Nothing Then
                Recording.MyLowFPS = d.my_presets.low.fps
                Recording.MyLowBitrate = d.my_presets.low.bitrate
                Recording.MyLowEncoderPreset = d.my_presets.low.encoder_preset
            End If
            If d.my_presets.medium IsNot Nothing Then
                Recording.MyMediumFPS = d.my_presets.medium.fps
                Recording.MyMediumBitrate = d.my_presets.medium.bitrate
                Recording.MyMediumEncoderPreset = d.my_presets.medium.encoder_preset
            End If
            If d.my_presets.high IsNot Nothing Then
                Recording.MyHighFPS = d.my_presets.high.fps
                Recording.MyHighBitrate = d.my_presets.high.bitrate
                Recording.MyHighEncoderPreset = d.my_presets.high.encoder_preset
            End If
        End If

        If Not String.IsNullOrWhiteSpace(d.engine_mode) Then
            Recording.EngineMode = NormalizeEngineMode(d.engine_mode)
        Else
            
            Recording.EngineMode = If(String.Equals(d.api_capture, "ddagrab", StringComparison.OrdinalIgnoreCase), "Duluka", "FFmpeg")
        End If

        Recording.APICapture = d.api_capture
    End Sub

    Private Shared Function NormalizeEngineMode(value As String) As String
        Dim mode As String = If(value, "").Trim().ToLowerInvariant()
        If mode = "duluka" OrElse mode = "ddagrab" OrElse mode = "native" Then Return "Duluka"
        If mode = "ffmpeg" OrElse mode = "legacy" Then Return "FFmpeg"
        Return "FFmpeg"
    End Function

    Private Sub WriteConfigFileAtomic(json As String)
        Dim tmpPath As String = ConfigPath & "." & Process.GetCurrentProcess().Id.ToString() & ".tmp"
        Dim bakPath As String = ConfigPath & ".bak"
        AppLayout.EnsureParentDir(ConfigPath)
        File.WriteAllText(tmpPath, json)
        If File.Exists(ConfigPath) Then
            File.Copy(ConfigPath, bakPath, True)
        End If
        File.Move(tmpPath, ConfigPath, True)
    End Sub

    Private Sub ApplyLoadedSettings(loaded As AppSettings)
        If loaded Is Nothing Then Return

        ApplyRecordingSettings(loaded.Recording)
        ApplyPathSettings(loaded.Paths)
        ApplyUISettings(loaded.UI)
        ApplyAudioSettings(loaded.Audio)
        ApplyGitHubUserSettings(loaded.GitHubUser)
        ApplyPrivacySettings(loaded.Privacy)
        ApplyOverlaySettings(loaded.Overlay)
        ApplyNotificationsSettings(loaded.Notifications)

        If loaded.Hotkeys IsNot Nothing Then
            Hotkeys = New Dictionary(Of String, String)(loaded.Hotkeys, StringComparer.OrdinalIgnoreCase)
        End If

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
        UI.UseWindowsSnip = loadedUI.UseWindowsSnip
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

        Audio.MicDeviceId = loadedAudio.MicDeviceId
        Audio.TrackMode = loadedAudio.TrackMode
        Audio.AudioClockMode = loadedAudio.AudioClockMode
    End Sub

    Private Sub ApplyGitHubUserSettings(loadedGitHubUser As GitHubUserClass)
        If loadedGitHubUser Is Nothing Then Return

        GitHubUser.Username = loadedGitHubUser.Username
        GitHubUser.AvatarUrl = loadedGitHubUser.AvatarUrl
        GitHubUser.IsLoggedIn = loadedGitHubUser.IsLoggedIn
        GitHubUser.LastLogin = loadedGitHubUser.LastLogin
    End Sub

    Private Sub ApplyPrivacySettings(loadedPrivacy As PrivacySettingsClass)
        If loadedPrivacy Is Nothing Then Return
        Privacy.DesktopCaptureEnabled = loadedPrivacy.DesktopCaptureEnabled
    End Sub

    Private Sub ApplyOverlaySettings(loadedOverlay As OverlaySettingsClass)
        If loadedOverlay Is Nothing Then Return
        Overlay.UseOverlayEnabled = loadedOverlay.UseOverlayEnabled
    End Sub

    Private Sub ApplyNotificationsSettings(loadedNotifications As NotificationsSettingsClass)
        If loadedNotifications Is Nothing Then Return
        
        Notifications.RecordingStarted = loadedNotifications.RecordingStarted
        Notifications.RecordingSaved = loadedNotifications.RecordingSaved
        Notifications.RecordingError = loadedNotifications.RecordingError
        
        Notifications.ReplaySaved = loadedNotifications.ReplaySaved
        Notifications.InstantReplayOn = loadedNotifications.InstantReplayOn
        Notifications.InstantReplayOff = loadedNotifications.InstantReplayOff
        Notifications.ReplayTurnOn = loadedNotifications.ReplayTurnOn
        Notifications.ReplayError = loadedNotifications.ReplayError
        
        Notifications.ScreenshotSaved = loadedNotifications.ScreenshotSaved
        Notifications.ValidSavePath = loadedNotifications.ValidSavePath
        
        Notifications.OpenShare = loadedNotifications.OpenShare
        
        Notifications.RamWarning = loadedNotifications.RamWarning
        Notifications.RamWarning95 = loadedNotifications.RamWarning95
        Notifications.RamCritical = loadedNotifications.RamCritical
        Notifications.CpuWarning = loadedNotifications.CpuWarning
        Notifications.DiskSpaceLow = loadedNotifications.DiskSpaceLow
        
        Notifications.UpdateAvailable = loadedNotifications.UpdateAvailable
        Notifications.VersionLatest = loadedNotifications.VersionLatest
        Notifications.UpdateError = loadedNotifications.UpdateError
        
        Notifications.AccountConfirmError = loadedNotifications.AccountConfirmError
        Notifications.ExtensionNotFound = loadedNotifications.ExtensionNotFound
        Notifications.FeatureNotReady = loadedNotifications.FeatureNotReady
        Notifications.GpuRequired = loadedNotifications.GpuRequired
        Notifications.EngineNotRunning = loadedNotifications.EngineNotRunning
        Notifications.EngineUIInUse = loadedNotifications.EngineUIInUse
        Notifications.ErrorResolution = loadedNotifications.ErrorResolution
        Notifications.DesktopCaptureDisabled = loadedNotifications.DesktopCaptureDisabled

        Notifications.SlotCount = Math.Min(3, Math.Max(1, loadedNotifications.SlotCount))
    End Sub

    Public Sub Save()
        Try
            SyncLock _saveLock

                Overlay.UseOverlayEnabled = AppConfigShared.ReadBool("Overlay", "UseOverlayEnabled", Overlay.UseOverlayEnabled)

                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True
                }

                Dim dto As New ConfigFileDto With {
                    .Recording = BuildRecordingDto(),
                    .Paths = Paths,
                    .UI = UI,
                    .Audio = Audio,
                    .Privacy = Privacy,
                    .Overlay = Overlay,
                    .Notifications = Notifications,
                    .Hotkeys = Hotkeys,
                    .GitHubUser = GitHubUser,
                    .GitHubTokenEncrypted = GitHubTokenEncrypted
                }

                Dim json As String = JsonSerializer.Serialize(dto, options)
                WriteConfigFileAtomic(json)
                Debug.WriteLine("AppSettings.Save: Saved to " & ConfigPath)
            End SyncLock

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Save Error: " & ex.Message)
        End Try
    End Sub
#End Region

End Class

