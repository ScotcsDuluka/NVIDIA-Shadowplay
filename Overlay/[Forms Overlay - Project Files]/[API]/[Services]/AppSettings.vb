' AppSettings — THE single user-facing configuration model (config.json).
' One class, split across partial files for readability:
'   AppSettings.vb                   → JSON model classes + Load / Save + singleton
'   AppSettings.GitHub.vb            → GitHub account (login, avatar, API)
'   AppSettings.LegacyVideo.vb       → legacy video.json schema + one-time migration
'   AppSettings.HardwareDetection.vb → GPU detection (NVIDIA / AMD / Intel, AV1)

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

Partial Public Class AppSettings
#Region "JSON Model Classes"
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
    End Class

    ''' <summary>
    ''' Path settings: gallery, save, ffmpeg paths
    ''' </summary>
    Public Class PathSettingsClass
        Public Property GalleryPath As String = ""
        Public Property SavePath As String = ""
        Public Property FFmpegPath As String = ""
    End Class

    ''' <summary>
    ''' UI settings: language, theme
    ''' </summary>
    Public Class UISettingsClass
        Public Property Language As String = "en-US"
        Public Property Theme As String = "Dark"
        Public Property UseWindowsSnip As Boolean = False
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

        ''' <summary>
        ''' Microphone device ID (NAudio MMDevice ID — stable across renames).
        ''' Ported from Engine's AudioSettingsForm so the Overlay audio page can
        ''' persist the exact device the Engine selects by ID.
        ''' </summary>
        Public Property MicDeviceId As String = ""

        ''' <summary>
        ''' Audio track mode: 0 = Single (mixed), 1 = Separate mic track.
        ''' Mirrors CaptureSettings.AudioTrackModeEnum consumed by the Engine
        ''' via audio.json (AudioTrackMode).
        ''' </summary>
        Public Property TrackMode As Integer = 0

        ''' <summary>
        ''' ★ P13.3/P13.4 A/B flag (unified-config plumbing fix): "Device" =
        ''' hardware-stamped system-audio timeline, "Legacy" = proven v2 tap.
        ''' Consumed by the Engine through unified config.json (normalized
        ''' there — anything but "Device" means Legacy). Temporary knob:
        ''' the whole flag is deleted at P13.5. Declared here so a
        ''' hand-edited config.json value survives Overlay saves (without
        ''' this property the serializer would silently erase the key).
        ''' </summary>
        Public Property AudioClockMode As String = "Legacy"

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
    End Class

    ''' <summary>
    ''' Privacy settings — user consent flags that used to live as the marker
    ''' file Data/NVIDIA_Shadowplay_Data/privacy. Imported once by
    ''' MigrateLegacyMarkerFiles, then config.json is the only source.
    ''' </summary>
    Public Class PrivacySettingsClass
        ''' <summary>Desktop capture allowed (was: privacy marker file exists).</summary>
        Public Property DesktopCaptureEnabled As Boolean = False
    End Class

    ''' <summary>
    ''' Overlay stack switch that used to live as the Flags\Use_Overlay marker
    ''' file. OWNED by the NVIDIA Experience (Launcher) toggle; the NVIDIA API
    ''' hub reads it every second to start/keep-alive or kill the overlay
    ''' stack. The Overlay itself only carries the value here so its
    ''' full-model Save() cannot erase a Launcher-written flip (see Save()).
    ''' </summary>
    Public Class OverlaySettingsClass
        ''' <summary>Overlay stack enabled (was: Flags/Use_Overlay exists).</summary>
        Public Property UseOverlayEnabled As Boolean = False
    End Class

    ''' <summary>
    ''' Per-category notification switches (Settings → Notifications page).
    ''' All default True — missing keys/sections show as before, so the
    ''' pre-toggles behavior is unchanged. The Notifier reads the same
    ''' keys via AppConfigShared at display time (the single choke point
    ''' every toast passes through: the TCP path AND the OBS bridge).
    ''' </summary>
    Public Class NotificationsSettingsClass
        Public Property Recording As Boolean = True
        Public Property InstantReplay As Boolean = True
        Public Property Screenshots As Boolean = True
        Public Property ShareOverlay As Boolean = True
        Public Property SystemMonitor As Boolean = True
        Public Property Updates As Boolean = True
        Public Property Errors As Boolean = True
    End Class

    ''' <summary>
    ''' GitHub User settings - เก็บข้อมูลผู้ใช้ GitHub
    ''' </summary>
    Public Class GitHubUserClass
        Public Property Username As String = ""
        Public Property AvatarUrl As String = ""
        Public Property IsLoggedIn As Boolean = False
        Public Property LastLogin As DateTime = DateTime.MinValue
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

#Region "Config Sections (Recording / Paths / UI / Audio / Privacy / Overlay / Hotkeys)"
    Public Property Recording As New RecordingSettingsClass()
    Public Property Paths As New PathSettingsClass()
    Public Property UI As New UISettingsClass()
    Public Property Audio As New AudioSettingsClass()
    Public Property Privacy As New PrivacySettingsClass()
    Public Property Overlay As New OverlaySettingsClass()
    Public Property Notifications As New NotificationsSettingsClass()

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

    ''' <summary>True when config.json did NOT exist at the last Load() — legacy files must be migrated in.</summary>
    Private _configWasMissingOnLoad As Boolean = False

    ''' <summary>
    ''' Path to config.json — THE single user-facing config file (GLM/6 unified
    ''' config: Paths + UI + Recording + Audio + Hotkeys all live here).
    ''' </summary>
    Private ReadOnly Property ConfigPath As String
        Get
            If _configPath Is Nothing Then
                _configPath = AppLayout.P("Config", "config.json")
            End If
            Return _configPath
        End Get
    End Property

    ''' <summary>
    ''' Path to legacy video.json — READ ONCE for migration only. Nothing writes
    ''' it anymore; after a successful migration it is renamed to video.json.legacy.
    ''' </summary>
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
                _configWasMissingOnLoad = True
                Save()
                Debug.WriteLine("AppSettings.Load: Created default config")
            End If

            ' GLM/6 UNIFIED CONFIG: one-time legacy migration (video.json +
            ' audio.json → config.json). Runs only when config.json was just
            ' created (first run / fresh install next to old files).
            MigrateLegacyConfigFiles()

            ' One-time import of the last settings that lived OUTSIDE
            ' config.json (privacy marker, Use_Overlay flag, current.txt).
            MigrateLegacyMarkerFiles()

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load Error: " & ex.Message)
        End Try
    End Sub

#Region "Legacy marker-file migration (privacy / Use_Overlay / current.txt)"
    ''' <summary>
    ''' One-time import of user settings that used to live OUTSIDE config.json:
    '''   Data/NVIDIA_Shadowplay_Data/privacy → Privacy.DesktopCaptureEnabled
    '''   Flags/Use_Overlay                   → Overlay.UseOverlayEnabled
    '''   Languages/current.txt               → UI.Language
    ''' Each source is imported once and then deleted, so config.json becomes
    ''' the single source of truth. Runs on every Load() but is a no-op once
    ''' the legacy files are gone.
    ''' </summary>
    Private Sub MigrateLegacyMarkerFiles()
        Dim changed As Boolean = False

        ' ── Privacy consent marker (existence = user opted in) ──
        Dim privacyMarker As String = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "privacy")
        If File.Exists(privacyMarker) Then
            Privacy.DesktopCaptureEnabled = True
            AppLayout.DeleteFileIfExists(privacyMarker)
            changed = True
            Debug.WriteLine("[Migrate] privacy marker → config.json Privacy.DesktopCaptureEnabled = True")
        End If

        ' ── Overlay stack toggle (Flags/Use_Overlay) ──
        Dim useOverlayFlag As String = AppLayout.P("Flags", "Use_Overlay")
        If File.Exists(useOverlayFlag) Then
            Overlay.UseOverlayEnabled = True
            AppLayout.DeleteFileIfExists(useOverlayFlag)
            changed = True
            Debug.WriteLine("[Migrate] Flags/Use_Overlay → config.json Overlay.UseOverlayEnabled = True")
        End If

        ' ── Current language pointer ──
        ' Import only when the code maps to an existing Languages\<code>.json
        ' (defends against a hand-edited/garbage pointer); the pointer file is
        ' removed either way so the migration always completes.
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
        ApplyPrivacySettings(loaded.Privacy)
        ApplyOverlaySettings(loaded.Overlay)
        ApplyNotificationsSettings(loaded.Notifications)

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
        ' GLM/6 unification added MicDeviceId + TrackMode to the class but this
        ' field-copy was never updated — every app start silently dropped both
        ' (mic selection reset in the Overlay UI; the Engine was unaffected
        ' because it reads config.json itself). AudioClockMode rides the fix.
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
        Notifications.Recording = loadedNotifications.Recording
        Notifications.InstantReplay = loadedNotifications.InstantReplay
        Notifications.Screenshots = loadedNotifications.Screenshots
        Notifications.ShareOverlay = loadedNotifications.ShareOverlay
        Notifications.SystemMonitor = loadedNotifications.SystemMonitor
        Notifications.Updates = loadedNotifications.Updates
        Notifications.Errors = loadedNotifications.Errors
    End Sub

    ''' <summary>
    ''' Save settings to config.json
    ''' </summary>
    Public Sub Save()
        Try
            ' Foreign-key guard: Overlay.UseOverlayEnabled is owned by the
            ' NVIDIA Experience toggle (Launcher) and enforced by the API hub
            ' every second. Refresh it from the file right before serializing
            ' so an Overlay save can never clobber a toggle flip that happened
            ' after our Load(). (Privacy/UI.Language are Overlay-owned — no
            ' other process writes them.)
            Overlay.UseOverlayEnabled = AppConfigShared.ReadBool("Overlay", "UseOverlayEnabled", Overlay.UseOverlayEnabled)

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

End Class

