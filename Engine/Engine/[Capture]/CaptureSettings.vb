' CaptureSettings.vb
' ShadowPlay Engine - Configuration Model
' JSON config: UseNativeResolution, Encoder, FPS, Bitrate, CaptureMethod, etc.

Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

<Serializable()>
Public Class CaptureSettings

    <JsonPropertyName("UseNativeResolution")>
    Public Property UseNativeResolution As Boolean = True

    <JsonPropertyName("Encoder")>
    Public Property Encoder As String = ""

    <JsonPropertyName("FPS")>
    Public Property FPS As Integer = 60

    <JsonPropertyName("Bitrate")>
    Public Property Bitrate As Long = 50000000

    <JsonPropertyName("CaptureMethod")>
    Public Property CaptureMethod As String = "ddagrab"

    <JsonPropertyName("OutputDirectory")>
    Public Property OutputDirectory As String = ""

    <JsonPropertyName("AudioCapture")>
    Public Property AudioCapture As Boolean = False

    <JsonPropertyName("AudioDevice")>
    Public Property AudioDevice As String = ""

    <JsonPropertyName("SystemAudioCapture")>
    Public Property SystemAudioCapture As Boolean = False

    <JsonPropertyName("MicCapture")>
    Public Property MicCapture As Boolean = False

    <JsonPropertyName("SystemAudioVolume")>
    Public Property SystemAudioVolume As Single = 1.0F

    <JsonPropertyName("MicVolume")>
    Public Property MicVolume As Single = 1.0F

    <JsonPropertyName("MicDeviceName")>
    Public Property MicDeviceName As String = ""

    <JsonPropertyName("MicDeviceId")>
    Public Property MicDeviceId As String = ""

    Public Enum AudioTrackModeEnum
        SingleTrack
        SeparateTrack
    End Enum

    <JsonPropertyName("AudioTrackMode")>
    Public Property AudioTrackMode As AudioTrackModeEnum = AudioTrackModeEnum.SingleTrack

    <JsonPropertyName("PixelFormat")>
    Public Property PixelFormat As String = "nv12"

    <JsonPropertyName("Preset")>
    Public Property Preset As String = "p4"

    ' ✅ P2.8: NVENC preset as integer 1-7 (from Overlay's encoder_preset).
    ' Overlay stores this as int, Engine's CaptureEngine maps it to 'p1'..'p7'.
    <JsonPropertyName("NvencPreset")>
    Public Property NvencPreset As Integer = 4

    <JsonPropertyName("RateControl")>
    Public Property RateControl As String = "cbr"

    <JsonPropertyName("FileFormat")>
    Public Property FileFormat As String = "mp4"

    <JsonPropertyName("FFmpegPath")>
    Public Property FFmpegPath As String = ""

    <JsonPropertyName("HotkeyStart")>
    Public Property HotkeyStart As String = "Control+Shift+F9"

    <JsonPropertyName("HotkeyStop")>
    Public Property HotkeyStop As String = "Control+Shift+F10"

    <JsonPropertyName("HotkeyToggle")>
    Public Property HotkeyToggle As String = "Control+Shift+F8"

    <JsonPropertyName("CustomWidth")>
    Public Property CustomWidth As Integer = 0

    <JsonPropertyName("CustomHeight")>
    Public Property CustomHeight As Integer = 0

    ' ── Config version for migration ──
    <JsonPropertyName("ConfigVersion")>
    Public Property ConfigVersion As Integer = 2

    ' ── Save / Load ──────────────────────────────────────────
    '
    ' Unified config: Engine reads from Overlay's video.json (same as Overlay).
    ' No more shadowplay-config.json — single source of truth.
    ' video.json schema (Overlay's VideoConfigClass):
    '   { "current": { fps, bitrate, encoder_preset, use_native_resolution, ... },
    '     "audio": { system_enabled, mic_enabled, system_volume, mic_volume, mic_device, mic_device_id } }
    '
    ' Engine-specific fields (FFmpegPath, CaptureMethod, OutputDirectory, etc.)
    ' are stored in config.json (Overlay's general settings, same file for both projects).

    Private Const CURRENT_VERSION As Integer = 3

    Public Sub Save(configPath As String)
        ' configPath = config.json path (general settings)
        ' Engine-specific fields saved into config.json under "engine" section
        Try
            Dim options As New JsonSerializerOptions With {.WriteIndented = True}

            ' Read existing config.json (if exists) to preserve other settings
            Dim existingJson As String = "{}"
            If File.Exists(configPath) Then
                existingJson = File.ReadAllText(configPath)
            End If

            ' Parse existing, update engine section
            Using doc As JsonDocument = JsonDocument.Parse(existingJson)
                Dim writer As New Utf8JsonWriter(New MemoryStream())
                writer.WriteStartObject()

                ' Copy existing properties except "engine"
                For Each prop As JsonProperty In doc.RootElement.EnumerateObject()
                    If prop.Name <> "engine" Then
                        prop.WriteTo(writer)
                    End If
                Next

                ' Write engine section
                writer.WriteStartObject("engine")
                writer.WriteString("Encoder", Encoder)
                writer.WriteString("CaptureMethod", CaptureMethod)
                writer.WriteString("PixelFormat", PixelFormat)
                writer.WriteString("Preset", Preset)
                writer.WriteNumber("NvencPreset", NvencPreset)
                writer.WriteString("RateControl", RateControl)
                writer.WriteString("FileFormat", FileFormat)
                writer.WriteString("FFmpegPath", FFmpegPath)
                writer.WriteString("HotkeyStart", HotkeyStart)
                writer.WriteString("HotkeyStop", HotkeyStop)
                writer.WriteString("HotkeyToggle", HotkeyToggle)
                writer.WriteNumber("CustomWidth", CustomWidth)
                writer.WriteNumber("CustomHeight", CustomHeight)
                writer.WriteNumber("AudioTrackMode", CInt(AudioTrackMode))
                writer.WriteNumber("ConfigVersion", ConfigVersion)
                writer.WriteEndObject()

                writer.WriteEndObject()
                writer.Flush()

                Dim ms As MemoryStream = DirectCast(writer.GetType().GetField("_stream", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance).GetValue(writer), MemoryStream)
                Dim json As String = System.Text.Encoding.UTF8.GetString(ms.ToArray())
                File.WriteAllText(configPath, json)
            End Using
        Catch ex As Exception
            ' Fallback: save as standalone (old behavior)
            Try
                Dim options As New JsonSerializerOptions With {.WriteIndented = True}
                Dim json As String = JsonSerializer.Serialize(Me, options)
                File.WriteAllText(configPath, json)
            Catch
            End Try
        End Try
    End Sub

    Public Shared Function Load(configPath As String) As CaptureSettings
        ' configPath = config.json path (unified: general + engine settings)
        Try
            ' ── Try reading from video.json first (Overlay's source of truth) ──
            Dim videoConfigPath As String = FindVideoJsonPath()
            Dim settings As CaptureSettings = Nothing

            If Not String.IsNullOrEmpty(videoConfigPath) AndAlso File.Exists(videoConfigPath) Then
                settings = LoadFromVideoJson(videoConfigPath)
            End If

            If settings Is Nothing Then
                settings = New CaptureSettings()
            End If

            ' ── Read engine-specific fields from config.json ──
            If File.Exists(configPath) Then
                Dim json As String = File.ReadAllText(configPath)
                Using doc As JsonDocument = JsonDocument.Parse(json)
                    If doc.RootElement.TryGetProperty("engine", JsonValueKind.Object) Then
                        Dim engineSection As JsonElement = doc.RootElement.GetProperty("engine")
                        If engineSection.TryGetProperty("Encoder", JsonValueKind.String) Then
                            settings.Encoder = engineSection.GetProperty("Encoder").GetString()
                        End If
                        If engineSection.TryGetProperty("CaptureMethod", JsonValueKind.String) Then
                            settings.CaptureMethod = engineSection.GetProperty("CaptureMethod").GetString()
                        End If
                        If engineSection.TryGetProperty("PixelFormat", JsonValueKind.String) Then
                            settings.PixelFormat = engineSection.GetProperty("PixelFormat").GetString()
                        End If
                        If engineSection.TryGetProperty("Preset", JsonValueKind.String) Then
                            settings.Preset = engineSection.GetProperty("Preset").GetString()
                        End If
                        If engineSection.TryGetProperty("NvencPreset", JsonValueKind.Number) Then
                            settings.NvencPreset = engineSection.GetProperty("NvencPreset").GetInt32()
                        End If
                        If engineSection.TryGetProperty("RateControl", JsonValueKind.String) Then
                            settings.RateControl = engineSection.GetProperty("RateControl").GetString()
                        End If
                        If engineSection.TryGetProperty("FileFormat", JsonValueKind.String) Then
                            settings.FileFormat = engineSection.GetProperty("FileFormat").GetString()
                        End If
                        If engineSection.TryGetProperty("FFmpegPath", JsonValueKind.String) Then
                            settings.FFmpegPath = engineSection.GetProperty("FFmpegPath").GetString()
                        End If
                        If engineSection.TryGetProperty("HotkeyStart", JsonValueKind.String) Then
                            settings.HotkeyStart = engineSection.GetProperty("HotkeyStart").GetString()
                        End If
                        If engineSection.TryGetProperty("HotkeyStop", JsonValueKind.String) Then
                            settings.HotkeyStop = engineSection.GetProperty("HotkeyStop").GetString()
                        End If
                        If engineSection.TryGetProperty("HotkeyToggle", JsonValueKind.String) Then
                            settings.HotkeyToggle = engineSection.GetProperty("HotkeyToggle").GetString()
                        End If
                        If engineSection.TryGetProperty("CustomWidth", JsonValueKind.Number) Then
                            settings.CustomWidth = engineSection.GetProperty("CustomWidth").GetInt32()
                        End If
                        If engineSection.TryGetProperty("CustomHeight", JsonValueKind.Number) Then
                            settings.CustomHeight = engineSection.GetProperty("CustomHeight").GetInt32()
                        End If
                        If engineSection.TryGetProperty("AudioTrackMode", JsonValueKind.Number) Then
                            settings.AudioTrackMode = DirectCast(engineSection.GetProperty("AudioTrackMode").GetInt32(), AudioTrackModeEnum)
                        End If
                        If engineSection.TryGetProperty("ConfigVersion", JsonValueKind.Number) Then
                            settings.ConfigVersion = engineSection.GetProperty("ConfigVersion").GetInt32()
                        End If
                    End If
                End Using
            End If

            ' ── Config migration ──
            If settings.ConfigVersion < CURRENT_VERSION Then
                settings.AudioCapture = False
                settings.AudioDevice = ""
                settings.ConfigVersion = CURRENT_VERSION
            End If

            ' ── Detect FFmpegPath if still empty ──
            If String.IsNullOrEmpty(settings.FFmpegPath) OrElse Not File.Exists(settings.FFmpegPath) Then
                settings.FFmpegPath = FindFFmpegPath()
            End If

            ' ── Default output directory ──
            If String.IsNullOrEmpty(settings.OutputDirectory) Then
                settings.OutputDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "ShadowPlay Recordings")
            End If

            If Not Directory.Exists(settings.OutputDirectory) Then
                Directory.CreateDirectory(settings.OutputDirectory)
            End If

            Return settings
        Catch ex As Exception
            Return CreateDefault(configPath)
        End Try
    End Function

    ''' <summary>
    ''' Find video.json path — same logic as OverlayConfig.FindConfigDir.
    ''' Searches: appdir, then parent dirs for Overlay/bin/Release/*/video.json
    ''' </summary>
    Private Shared Function FindVideoJsonPath() As String
        Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory

        ' 1. Same folder as Engine
        Dim candidate As String = Path.Combine(appDir, "video.json")
        If File.Exists(candidate) Then Return candidate

        ' 2. Parent dirs (for bin\Release\net8.0\... look for video.json in parent)
        Dim parentDir As String = appDir
        For depth As Integer = 1 To 5
            Try
                parentDir = Directory.GetParent(parentDir)?.FullName
                If String.IsNullOrWhiteSpace(parentDir) Then Exit For
                candidate = Path.Combine(parentDir, "video.json")
                If File.Exists(candidate) Then Return candidate
            Catch
                Exit For
            End Try
        Next

        ' 3. Sibling Overlay folder (running from source)
        Dim engineProjDir As String = appDir
        For depth As Integer = 1 To 6
            Try
                Dim parent As DirectoryInfo = Directory.GetParent(engineProjDir)
                If parent Is Nothing Then Exit For
                engineProjDir = parent.FullName
                Dim overlayBin As String = Path.Combine(parent.FullName, "Overlay", "bin")
                If Directory.Exists(overlayBin) Then
                    For Each configDir As String In {"Release", "Debug"}
                        Dim configPath_ As String = Path.Combine(overlayBin, configDir)
                        If Directory.Exists(configPath_) Then
                            For Each subDir As String In Directory.GetDirectories(configPath_)
                                candidate = Path.Combine(subDir, "video.json")
                                If File.Exists(candidate) Then Return candidate
                            Next
                        End If
                    Next
                    Exit For
                End If
            Catch
                Exit For
            End Try
        Next

        Return ""
    End Function

    ''' <summary>
    ''' Load capture settings from Overlay's video.json.
    ''' video.json has nested structure: { "current": {...}, "audio": {...} }
    ''' </summary>
    Private Shared Function LoadFromVideoJson(videoJsonPath As String) As CaptureSettings
        Try
            Dim json As String = File.ReadAllText(videoJsonPath)
            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim settings As New CaptureSettings()

                ' Read "current" section (video capture values)
                If doc.RootElement.TryGetProperty("current", JsonValueKind.Object) Then
                    Dim current As JsonElement = doc.RootElement.GetProperty("current")
                    If current.TryGetProperty("fps", JsonValueKind.Number) Then
                        settings.FPS = current.GetProperty("fps").GetInt32()
                    End If
                    If current.TryGetProperty("bitrate", JsonValueKind.Number) Then
                        settings.Bitrate = current.GetProperty("bitrate").GetInt32() * 1000L  ' Overlay stores in kbps
                    End If
                    If current.TryGetProperty("encoder_preset", JsonValueKind.Number) Then
                        settings.NvencPreset = current.GetProperty("encoder_preset").GetInt32()
                    End If
                    If current.TryGetProperty("use_native_resolution", JsonValueKind.False) OrElse
                       current.TryGetProperty("use_native_resolution", JsonValueKind.True) Then
                        settings.UseNativeResolution = current.GetProperty("use_native_resolution").GetBoolean()
                    End If
                    If current.TryGetProperty("width", JsonValueKind.Number) Then
                        settings.CustomWidth = current.GetProperty("width").GetInt32()
                    End If
                    If current.TryGetProperty("height", JsonValueKind.Number) Then
                        settings.CustomHeight = current.GetProperty("height").GetInt32()
                    End If
                End If

                ' Read "audio" section
                If doc.RootElement.TryGetProperty("audio", JsonValueKind.Object) Then
                    Dim audio As JsonElement = doc.RootElement.GetProperty("audio")
                    If audio.TryGetProperty("system_enabled", JsonValueKind.False) OrElse
                       audio.TryGetProperty("system_enabled", JsonValueKind.True) Then
                        settings.SystemAudioCapture = audio.GetProperty("system_enabled").GetBoolean()
                    End If
                    If audio.TryGetProperty("mic_enabled", JsonValueKind.False) OrElse
                       audio.TryGetProperty("mic_enabled", JsonValueKind.True) Then
                        settings.MicCapture = audio.GetProperty("mic_enabled").GetBoolean()
                    End If
                    If audio.TryGetProperty("system_volume", JsonValueKind.Number) Then
                        settings.SystemAudioVolume = audio.GetProperty("system_volume").GetSingle()
                    End If
                    If audio.TryGetProperty("mic_volume", JsonValueKind.Number) Then
                        settings.MicVolume = audio.GetProperty("mic_volume").GetSingle()
                    End If
                    If audio.TryGetProperty("mic_device", JsonValueKind.String) Then
                        settings.MicDeviceName = audio.GetProperty("mic_device").GetString()
                    End If
                    If audio.TryGetProperty("mic_device_id", JsonValueKind.String) Then
                        settings.MicDeviceId = audio.GetProperty("mic_device_id").GetString()
                    End If
                End If

                ' Output directory from video.json if present
                If doc.RootElement.TryGetProperty("output_directory", JsonValueKind.String) Then
                    settings.OutputDirectory = doc.RootElement.GetProperty("output_directory").GetString()
                End If

                Return settings
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    Private Shared Function FindFFmpegPath() As String
        Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim candidates As New List(Of String)()
        candidates.Add(Path.Combine(appDir, "API-Core", "ffmpeg.exe"))
        candidates.Add(Path.Combine(appDir, "api-core", "ffmpeg.exe"))
        candidates.Add(Path.Combine(appDir, "ffmpeg.exe"))

        ' Parent dirs
        Dim parentDir As String = appDir
        For depth As Integer = 1 To 5
            Try
                parentDir = Directory.GetParent(parentDir)?.FullName
                If String.IsNullOrWhiteSpace(parentDir) Then Exit For
                candidates.Add(Path.Combine(parentDir, "API-Core", "ffmpeg.exe"))
                candidates.Add(Path.Combine(parentDir, "api-core", "ffmpeg.exe"))
                candidates.Add(Path.Combine(parentDir, "ffmpeg.exe"))
            Catch
                Exit For
            End Try
        Next

        ' Sibling Overlay folder
        Try
            Dim engineProjDir As String = appDir
            For depth As Integer = 1 To 6
                Dim parent As DirectoryInfo = Directory.GetParent(engineProjDir)
                If parent Is Nothing Then Exit For
                engineProjDir = parent.FullName
                Dim overlayBin As String = Path.Combine(parent.FullName, "Overlay", "bin")
                If Directory.Exists(overlayBin) Then
                    For Each configDir As String In {"Release", "Debug"}
                        Dim configPath_ As String = Path.Combine(overlayBin, configDir)
                        If Directory.Exists(configPath_) Then
                            For Each subDir As String In Directory.GetDirectories(configPath_)
                                candidates.Add(Path.Combine(subDir, "api-core", "ffmpeg.exe"))
                                candidates.Add(Path.Combine(subDir, "API-Core", "ffmpeg.exe"))
                            Next
                        End If
                    Next
                    Exit For
                End If
            Next
        Catch
        End Try

        For Each candidate In candidates
            If File.Exists(candidate) Then Return candidate
        Next

        Return ""
    End Function

    Public Shared Function CreateDefault(configPath As String) As CaptureSettings
        Dim settings As New CaptureSettings()
        settings.FFmpegPath = FindFFmpegPath()

        If String.IsNullOrEmpty(settings.OutputDirectory) Then
            settings.OutputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "ShadowPlay Recordings")
        End If

        If Not Directory.Exists(settings.OutputDirectory) Then
            Directory.CreateDirectory(settings.OutputDirectory)
        End If

        settings.ConfigVersion = CURRENT_VERSION
        Return settings
    End Function

    ' ── Helpers ──────────────────────────────────────────────

    Public Function GetCaptureResolution() As System.ValueTuple(Of Integer, Integer)
        If UseNativeResolution OrElse CustomWidth = 0 OrElse CustomHeight = 0 Then
            Return (Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
        End If
        Return (CustomWidth, CustomHeight)
    End Function

    Public Function GenerateOutputFilename() As String
        Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
        Return Path.Combine(OutputDirectory, "ShadowPlay_" & timestamp & "." & FileFormat)
    End Function

    ''' <summary>
    ''' ✅ P2.5: ValidationResult — replaced ValueTuple(Of Boolean, String).
    ''' Old code returned (Valid As Boolean, Message As String) which requires
    ''' Option Infer On to preserve the named members across method boundaries.
    ''' Engine's project has OptionInfer=Off, so 'Dim validation = Validate()'
    ''' produced a plain ValueTuple(Of Boolean, String) without .Valid / .Message
    ''' → runtime error "Public member 'Valid' on type 'ValueTuple' not found".
    ''' </summary>
    Public Class ValidationResult
        Public Property Valid As Boolean
        Public Property Message As String
        Public Sub New(valid As Boolean, message As String)
            Me.Valid = valid
            Me.Message = message
        End Sub
    End Class

    ''' <summary>
    ''' Returns ValidationResult — check .Valid and .Message.
    ''' If AudioCapture is enabled but device name is empty,
    ''' auto-disable audio capture instead of failing.
    ''' </summary>
    Public Function Validate() As ValidationResult
        If FPS < 1 OrElse FPS > 240 Then
            Return New ValidationResult(False, "FPS must be between 1 and 240")
        End If
        If Bitrate < 1000000 Then
            Return New ValidationResult(False, "Bitrate must be at least 1 Mbps")
        End If
        If Bitrate > 200000000 Then
            Return New ValidationResult(False, "Bitrate must not exceed 200 Mbps")
        End If
        Dim validMethods As String() = {"ddagrab", "gdigrab", "gfxcapture"}
        If Not validMethods.Contains(CaptureMethod.ToLower()) Then
            Return New ValidationResult(False, "Invalid capture method. Use: " & String.Join(", ", validMethods))
        End If
        If String.IsNullOrWhiteSpace(Encoder) Then
            Return New ValidationResult(False, "No encoder selected")
        End If
        If String.IsNullOrWhiteSpace(FFmpegPath) OrElse Not File.Exists(FFmpegPath) Then
            Return New ValidationResult(False, "FFmpeg not found at: " & FFmpegPath)
        End If

        If MicCapture AndAlso String.IsNullOrWhiteSpace(MicDeviceName) Then
            Return New ValidationResult(False, "Microphone is enabled but no device name is set. Select a microphone in the Overlay settings.")
        End If

        Return New ValidationResult(True, "")
    End Function

End Class
