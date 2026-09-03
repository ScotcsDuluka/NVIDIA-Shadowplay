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
        ''' Legacy: Mic volume as integer (0-100) - converted to MicVolume.
        ''' Computed duplicate — never persisted (JsonIgnore): it exists only
        ''' so old callers can keep passing percent values; config.json
        ''' carries the real MicVolume only.
        ''' </summary>
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

        ''' <summary>
        ''' Legacy: System volume as integer (0-100) - converted to SystemAudioVolume.
        ''' Computed duplicate — never persisted (JsonIgnore), same as MicVolumePercent.
        ''' </summary>
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
    ''' file. OWNED by the Launcher.exe toggle; the NVIDIA API
    ''' hub reads it every second to start/keep-alive or kill the overlay
    ''' stack. The Overlay itself only carries the value here so its
    ''' full-model Save() cannot erase a Launcher-written flip (see Save()).
    ''' </summary>
    Public Class OverlaySettingsClass
        ''' <summary>Overlay stack enabled (was: Flags/Use_Overlay exists).</summary>
        Public Property UseOverlayEnabled As Boolean = False
    End Class

    ''' <summary>
    ''' Per-notification switches (Settings → Notifications page) — one key
    ''' PER toast, grouped in the UI under static headers. All default True
    ''' — missing keys/sections show as before. The Notifier reads the same
    ''' keys via AppConfigShared at display time (the single choke point
    ''' every toast passes through: the TCP path AND the OBS bridge).
    ''' </summary>
    Public Class NotificationsSettingsClass
        ' RECORDING
        Public Property RecordingStarted As Boolean = True
        Public Property RecordingSaved As Boolean = True
        Public Property RecordingError As Boolean = True
        ' INSTANT REPLAY
        Public Property ReplaySaved As Boolean = True
        Public Property InstantReplayOn As Boolean = True
        Public Property InstantReplayOff As Boolean = True
        Public Property ReplayTurnOn As Boolean = True
        Public Property ReplayError As Boolean = True
        ' SCREENSHOTS
        Public Property ScreenshotSaved As Boolean = True
        Public Property ValidSavePath As Boolean = True
        ' SHARE OVERLAY
        Public Property OpenShare As Boolean = True
        ' SYSTEM MONITOR
        Public Property RamWarning As Boolean = True
        Public Property RamWarning95 As Boolean = True
        Public Property RamCritical As Boolean = True
        Public Property CpuWarning As Boolean = True
        Public Property DiskSpaceLow As Boolean = True
        ' UPDATES
        Public Property UpdateAvailable As Boolean = True
        Public Property VersionLatest As Boolean = True
        Public Property UpdateError As Boolean = True
        ' ERRORS & FEEDBACK
        Public Property AccountConfirmError As Boolean = True
        Public Property ExtensionNotFound As Boolean = True
        Public Property FeatureNotReady As Boolean = True
        Public Property GpuRequired As Boolean = True
        Public Property EngineNotRunning As Boolean = True
        Public Property EngineUIInUse As Boolean = True
        Public Property ErrorResolution As Boolean = True
        Public Property DesktopCaptureDisabled As Boolean = True
        ' TOAST SLOTS — how many simultaneous toast slots the Notifier uses
        ' (1 = every toast funnels through the main slot; 2 = a new
        ' notification group enters the free slot instead of replacing the
        ' showing one; 3 = slot 3 joins as the overflow when main AND slot
        ' 2 are both busy). Settings → General "second" + "third toast
        ' slot" toggles.
        Public Property SlotCount As Integer = 2
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

    ' ── GitHub account + token persistence — declared in the Config
    ' Sections region AFTER Hotkeys, so serialization order keeps config.json
    ' in Settings-page order with the account block last.
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

    ' ── GitHub account block LAST — declaration order = config.json key
    ' order, so the file opens with the app sections (Recording → Hotkeys)
    ' and closes with the account block.

    ''' <summary>
    ''' GitHub account + token persistence.
    ''' ✅ P1: token stored encrypted (DPAPI, CurrentUser scope) in config.json
    ''' as GitHubTokenEncrypted. Never written to disk as plain text. The
    ''' plain GitHubToken property below is computed (decrypt-on-read) and
    ''' only exists in memory. On first load after upgrade, an old plain-text
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
#End Region

#Region "Config file DTO — Recording stored video.json-shaped (the detailed file schema)"
    ' The in-memory model stays FLAT (Recording.FPS / Recording.MyLowFPS …) so
    ' every Settings page, the TCP payload and the Engine sync code keep
    ' working untouched. Only the FILE shape is nested: config.json's
    ' Recording section keeps the old video.json layout — current { fps,
    ' bitrate, … } / my_presets { low, medium, high } — the "detailed"
    ' structure the flat 20-key section had replaced. Mapping happens
    ' exclusively in BuildRecordingDto (save) / ApplyRecordingDto (load).
    ' Property names ARE the JSON keys: snake_case inside the section,
    ' PascalCase section names — same convention as the Engine-side mirror's
    ' OverlayConfig.VideoConfig classes, which parse this exact shape.

    ''' <summary>Nested under "current" — the values actually in use.</summary>
    Public Class VideoCurrentDto
        Public Property fps As Integer = 60
        Public Property bitrate As Integer = 20000
        Public Property encoder_preset As Integer = 4
        Public Property use_native_resolution As Boolean = True
        Public Property width As Integer = 1920
        Public Property height As Integer = 1080
    End Class

    ''' <summary>One MY preset slot — null = use NVIDIA defaults.</summary>
    Public Class MyPresetSlotDto
        Public Property fps As Integer? = Nothing
        Public Property bitrate As Integer? = Nothing
        Public Property encoder_preset As Integer? = Nothing
    End Class

    Public Class MyPresetsDto
        ''' <summary>Custom renameable name for MY preset group (e.g. "P4", "P6")</summary>
        Public Property name As String = "MY"
        Public Property low As MyPresetSlotDto = New MyPresetSlotDto()
        Public Property medium As MyPresetSlotDto = New MyPresetSlotDto()
        Public Property high As MyPresetSlotDto = New MyPresetSlotDto()
    End Class

    Public Class RecordingSectionDto
        ''' <summary>Encoder string: NVENC_H264, NVENC_HEVC, QuickSync_H264, etc.</summary>
        Public Property encoder As String = "NVENC_H264"
        ''' <summary>Encoder currently in use (may differ during transitions)</summary>
        Public Property encoder_now As String = "NVENC_H264"
        ''' <summary>Currently selected preset name (e.g. "Medium", "MyLow", "Custom")</summary>
        Public Property active_preset As String = "Medium"
        Public Property current As VideoCurrentDto = New VideoCurrentDto()
        ''' <summary>Replay buffer duration in seconds</summary>
        Public Property replay_duration As Integer = 60
        Public Property my_presets As MyPresetsDto = New MyPresetsDto()
        ''' <summary>Capture API: ddagrab, gfxcapture, GDIGrab, or null (auto)</summary>
        Public Property api_capture As Integer = 1
    End Class

    ''' <summary>
    ''' Full config.json file shape — Recording nested, everything else
    ''' identical to the model (the same classes → the same keys as today).
    ''' Declaration order = key order in the file.
    ''' </summary>
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
    ' Serializes Save() — two threads saving at once could otherwise
    ' interleave the read-modify-write guard below or trip over each
    ' other's temp file. (Instance creation uses _lock; saves get their
    ' own gate so a long save never blocks first access.)
    Private Shared ReadOnly _saveLock As New Object()
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
    ''' Load settings from config.json. Accepts BOTH file schema generations:
    ''' nested Recording (current — video.json-shaped) and flat Recording
    ''' (legacy files from before the detailed schema); a legacy-shaped file
    ''' is rewritten once in the nested shape right after a successful read.
    ''' </summary>
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

                ' Crash-proofing: a truncated config.json (hard kill / power
                ' loss mid-Save) must not silently reset the user to defaults.
                ' Every atomic save keeps the previous good content as
                ' config.json.bak — try it before giving up.
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
                    ' One-time schema upgrade + crash repair: a file read in
                    ' the legacy flat shape is rewritten once in the nested
                    ' shape; a file recovered from .bak is rewritten over the
                    ' corrupt one right away so the bad state never spreads.
                    If recoveredFromBackup OrElse Not ConfigJsonIsNested(json) Then
                        Save()
                    End If

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
                Else
                    Debug.WriteLine("AppSettings.Load: config.json unreadable and no usable .bak — running on defaults")
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

    ''' <summary>
    ''' Parse + deserialize config.json content with the tolerant options set.
    ''' Returns Nothing when the text is not usable JSON — caller may recover.
    ''' </summary>
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

    ''' <summary>
    ''' Parse + apply config.json content in EITHER schema generation:
    ''' nested (current — Recording stored video.json-shaped) or legacy flat
    ''' (Recording as 20 PascalCase keys). Returns False when the text is not
    ''' usable JSON in either shape — the caller may then try the .bak.
    ''' </summary>
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

    ''' <summary>
    ''' True when the Recording section is in the nested (video.json-shaped)
    ''' schema — detected by the "current" child object, which the legacy flat
    ''' generation never had. Tolerates casing, comments and trailing commas.
    ''' </summary>
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

    ''' <summary>
    ''' Parse config.json content into the nested-shape file DTO with the
    ''' tolerant options set. Returns Nothing when the text is not usable.
    ''' </summary>
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

    ''' <summary>
    ''' Last-resort recovery source: config.json.bak (kept by every atomic
    ''' save). Returns the raw backup text (Nothing when absent/unreadable) —
    ''' the caller runs the same dual-shape apply on it as on the live file.
    ''' </summary>
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

    ''' <summary>Apply a nested-shape ConfigFileDto onto the flat in-memory model.</summary>
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

        ' ✅ P1: copy the encrypted token directly (same contract as
        ' ApplyLoadedSettings — no decrypt-then-encrypt round-trip).
        GitHubTokenEncrypted = dto.GitHubTokenEncrypted
    End Sub

    ' ═══ Flat model → nested DTO (save) ═══

    ''' <summary>Flat in-memory Recording → nested (video.json-shaped) DTO.</summary>
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
        d.api_capture = Recording.APICapture
        Return d
    End Function

    Private Shared Function BuildPresetSlotDto(fps As Integer?, bitrate As Integer?, encoderPreset As Integer?) As MyPresetSlotDto
        Return New MyPresetSlotDto With {.fps = fps, .bitrate = bitrate, .encoder_preset = encoderPreset}
    End Function

    ' ═══ Nested DTO → flat model (load) ═══

    ''' <summary>
    ''' Nested (video.json-shaped) DTO → flat in-memory Recording. Missing
    ''' keys keep their defaults — same semantics as ApplyLoadedSettings.
    ''' </summary>
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

        ' Nothing = auto — direct assignment preserves the nullable semantics.
        Recording.APICapture = d.api_capture
    End Sub

    ''' <summary>
    ''' Atomic config write: write a per-PID temp file, keep the current
    ''' content as config.json.bak, then rename the temp over config.json
    ''' (same volume — atomic on NTFS). A crash mid-save can only cost the
    ''' newest change, never the whole file. The per-PID temp name stops two
    ''' processes (Overlay save vs Launcher WriteBool) from clobbering one
    ''' shared temp file mid-write.
    ''' </summary>
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
        ' RECORDING
        Notifications.RecordingStarted = loadedNotifications.RecordingStarted
        Notifications.RecordingSaved = loadedNotifications.RecordingSaved
        Notifications.RecordingError = loadedNotifications.RecordingError
        ' INSTANT REPLAY
        Notifications.ReplaySaved = loadedNotifications.ReplaySaved
        Notifications.InstantReplayOn = loadedNotifications.InstantReplayOn
        Notifications.InstantReplayOff = loadedNotifications.InstantReplayOff
        Notifications.ReplayTurnOn = loadedNotifications.ReplayTurnOn
        Notifications.ReplayError = loadedNotifications.ReplayError
        ' SCREENSHOTS
        Notifications.ScreenshotSaved = loadedNotifications.ScreenshotSaved
        Notifications.ValidSavePath = loadedNotifications.ValidSavePath
        ' SHARE OVERLAY
        Notifications.OpenShare = loadedNotifications.OpenShare
        ' SYSTEM MONITOR
        Notifications.RamWarning = loadedNotifications.RamWarning
        Notifications.RamWarning95 = loadedNotifications.RamWarning95
        Notifications.RamCritical = loadedNotifications.RamCritical
        Notifications.CpuWarning = loadedNotifications.CpuWarning
        Notifications.DiskSpaceLow = loadedNotifications.DiskSpaceLow
        ' UPDATES
        Notifications.UpdateAvailable = loadedNotifications.UpdateAvailable
        Notifications.VersionLatest = loadedNotifications.VersionLatest
        Notifications.UpdateError = loadedNotifications.UpdateError
        ' ERRORS & FEEDBACK
        Notifications.AccountConfirmError = loadedNotifications.AccountConfirmError
        Notifications.ExtensionNotFound = loadedNotifications.ExtensionNotFound
        Notifications.FeatureNotReady = loadedNotifications.FeatureNotReady
        Notifications.GpuRequired = loadedNotifications.GpuRequired
        Notifications.EngineNotRunning = loadedNotifications.EngineNotRunning
        Notifications.EngineUIInUse = loadedNotifications.EngineUIInUse
        Notifications.ErrorResolution = loadedNotifications.ErrorResolution
        Notifications.DesktopCaptureDisabled = loadedNotifications.DesktopCaptureDisabled
        ' TOAST SLOTS - clamp to the supported range (1..3 slots); a
        ' hand-edited config can never push the router out of bounds.
        Notifications.SlotCount = Math.Min(3, Math.Max(1, loadedNotifications.SlotCount))
    End Sub

    ''' <summary>
    ''' Save settings to config.json
    ''' </summary>
    Public Sub Save()
        Try
            SyncLock _saveLock
                ' Foreign-key guard: Overlay.UseOverlayEnabled is owned by the
                ' Launcher.exe toggle and enforced by the API hub
                ' every second. Refresh it from the file right before serializing
                ' so an Overlay save can never clobber a toggle flip that happened
                ' after our Load(). (Privacy/UI.Language are Overlay-owned — no
                ' other process writes them.)
                Overlay.UseOverlayEnabled = AppConfigShared.ReadBool("Overlay", "UseOverlayEnabled", Overlay.UseOverlayEnabled)

                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True
                }
                ' NOTE: no DefaultIgnoreCondition — null fields are written
                ' explicitly (e.g. "fps": null in a MY preset slot) so
                ' config.json always shows the FULL schema: every section,
                ' every key, in a stable order. All readers are null-tolerant
                ' (typed mirror models on the Overlay/Engine sides, TryGetValue
                ' fallbacks in AppConfigShared, and value-type keys are never
                ' null).

                ' The FILE schema nests Recording video.json-style (current /
                ' my_presets — the "detailed" layout); the in-memory model
                ' stays flat. ConfigFileDto is that file shape: Recording is
                ' built from the flat model, every other section is written
                ' as-is (the same classes the model uses → same keys).
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

