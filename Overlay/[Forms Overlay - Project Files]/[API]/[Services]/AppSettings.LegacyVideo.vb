' AppSettings (legacy video.json) — the pre-unification schema, kept for one
' thing only: importing an old video.json / audio.json into config.json on
' first run (MigrateLegacyConfigFiles, called from AppSettings.Load).
' New code reads AppSettings.Instance.Recording / .Audio — never video.json.
'
' NOTE: the nested video.json shape below (current / my_presets / audio) is
' ALSO the live shape of config.json's Recording section since the detailed
' file schema (ConfigFileDto in AppSettings.vb) — old video.json and the new
' section are the same layout, so a migration is a straight field-for-field
' copy onto the flat model.

Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json

Partial Public Class AppSettings
#Region "Legacy video.json (migration source only)"
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
        Public Property MicDeviceId As String = ""
        Public Property TrackMode As Integer = 0
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
    End Class

    ''' <summary>
    ''' GLM/6 UNIFIED CONFIG — one-time legacy migration:
    '''   video.json + audio.json → config.json (the ONE config file).
    '''
    ''' Runs when config.json was just created (fresh install beside old files).
    ''' Import order: video.json (richest, includes audio) then audio.json
    ''' (audio-specific fields win if both exist). After a successful import +
    ''' save, legacy files are renamed to *.legacy (best-effort, non-destructive).
    ''' Stale video.json on an install that already has config.json is ALSO
    ''' renamed away — nothing reads it anymore and config.json always wins.
    ''' </summary>
    Private Sub MigrateLegacyConfigFiles()
        Try
            Dim imported As Boolean = False

            If _configWasMissingOnLoad Then
                ' ── Import video.json (video + audio + presets) ──
                If File.Exists(VideoConfigPath) Then
                    Dim video = LoadVideoSettings()
                    If video IsNot Nothing Then
                        ApplyVideoConfig(video)
                        imported = True
                        Debug.WriteLine("[Migrate] imported legacy video.json → config.json")
                    End If
                End If

                ' ── Import audio.json (audio-specific schema wins if present) ──
                Dim audioPath As String = AppLayout.P("Config", "audio.json")
                If File.Exists(audioPath) Then
                    Try
                        Dim json As String = File.ReadAllText(audioPath)
                        Using doc As JsonDocument = JsonDocument.Parse(json)
                            Dim p As JsonElement
                            Dim a = AppSettings.Instance.Audio
                            If doc.RootElement.TryGetProperty("SystemEnabled", p) Then a.SystemAudioEnabled = p.GetBoolean()
                            If doc.RootElement.TryGetProperty("MicEnabled", p) Then a.MicEnabled = p.GetBoolean()
                            If doc.RootElement.TryGetProperty("SystemVolume", p) Then a.SystemAudioVolume = p.GetSingle()
                            If doc.RootElement.TryGetProperty("MicVolume", p) Then a.MicVolume = p.GetSingle()
                            If doc.RootElement.TryGetProperty("MicDevice", p) Then a.MicDeviceName = p.GetString()
                            If doc.RootElement.TryGetProperty("MicDeviceId", p) Then a.MicDeviceId = p.GetString()
                            If doc.RootElement.TryGetProperty("AudioTrackMode", p) Then a.TrackMode = p.GetInt32()
                            ' P13.4 plumbing: carry the Device-clock flag across
                            ' the legacy → unified migration (normalized).
                            If doc.RootElement.TryGetProperty("AudioClockMode", p) Then
                                Dim clock As String = If(p.GetString(), "").Trim()
                                a.AudioClockMode = If(String.Equals(clock, "Device", StringComparison.OrdinalIgnoreCase),
                                                      "Device", "Legacy")
                            End If
                            imported = True
                            Debug.WriteLine("[Migrate] imported legacy audio.json → config.json")
                        End Using
                    Catch ex As Exception
                        Debug.WriteLine("[Migrate] audio.json import failed: " & ex.Message)
                    End Try
                End If

                If imported Then Save()
            End If

            ' ── Rename stale legacy files out of the way (best-effort) ──
            RenameLegacyAway(VideoConfigPath, "video.json")
            RenameLegacyAway(AppLayout.P("Config", "audio.json"), "audio.json")

        Catch ex As Exception
            Debug.WriteLine("[Migrate] legacy migration error: " & ex.Message)
        End Try
    End Sub

    Private Sub RenameLegacyAway(path As String, baseName As String)
        Try
            If File.Exists(path) Then
                Dim dest As String = path & ".legacy"
                If File.Exists(dest) Then File.Delete(dest)
                File.Move(path, dest)
                Debug.WriteLine($"[Migrate] renamed {baseName} → {baseName}.legacy")
            End If
        Catch ex As Exception
            ' Engine may hold a read at this exact moment — retry next boot.
            Debug.WriteLine($"[Migrate] rename {baseName} deferred: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' โหลด video settings จาก video.json — LEGACY: migration source only.
    ''' New code must read AppSettings.Instance.Recording (config.json).
    ''' </summary>
    Private Function LoadVideoSettings() As VideoConfigClass
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
    ''' LEGACY migration helper: apply a video.json payload onto Recording + Audio.
    ''' Called by MigrateLegacyConfigFiles on first run after upgrade.
    ''' </summary>
    Private Sub ApplyVideoConfig(video As VideoConfigClass)
        If video Is Nothing Then Return

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
        Audio.MicDeviceId = video.Audio.MicDeviceId
        Audio.TrackMode = video.Audio.TrackMode

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

        Debug.WriteLine("ApplyVideoConfig: applied legacy video payload → Recording + Audio")
    End Sub

#End Region

End Class

