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

    ' Default values for old config migration
    Private Const CURRENT_VERSION As Integer = 2

    ' ── Save / Load ──────────────────────────────────────────

    Public Sub Save(configPath As String)
        Try
            Dim options As New JsonSerializerOptions With {.WriteIndented = True}
            Dim json As String = JsonSerializer.Serialize(Me, options)
            File.WriteAllText(configPath, json)
        Catch ex As Exception
            Throw New Exception("Failed to save config: " & ex.Message, ex)
        End Try
    End Sub

    Public Shared Function Load(configPath As String) As CaptureSettings
        Try
            If Not File.Exists(configPath) Then
                Return CreateDefault(configPath)
            End If
            Dim json As String = File.ReadAllText(configPath)
            Dim options As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}
            Dim settings As CaptureSettings = JsonSerializer.Deserialize(Of CaptureSettings)(json, options)
            If settings Is Nothing Then Return CreateDefault(configPath)

            ' ── Config migration ──
            ' Version 1 (or missing) had audio enabled with hardcoded "stereo mix"
            ' which doesn't exist on most machines. Force-reset audio on old configs.
            If settings.ConfigVersion < CURRENT_VERSION Then
                settings.AudioCapture = False
                settings.AudioDevice = ""
                settings.ConfigVersion = CURRENT_VERSION
                settings.Save(configPath)
            End If

            Return settings
        Catch ex As Exception
            Return CreateDefault(configPath)
        End Try
    End Function

    Public Shared Function CreateDefault(configPath As String) As CaptureSettings
        Dim settings As New CaptureSettings()

        Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory

        ' Build candidate paths for ffmpeg.exe
        ' Checks: appDir itself, subfolders, parent dirs (for bin\Release\ builds)
        Dim candidates As New List(Of String)()
        candidates.Add(Path.Combine(appDir, "API-Core", "ffmpeg.exe"))
        candidates.Add(Path.Combine(appDir, "ffmpeg.exe"))
        candidates.Add(Path.Combine(appDir, "Tools", "ffmpeg.exe"))
        candidates.Add("ffmpeg.exe")

        ' Also check parent directories (app may be in bin\Release\net8.0\...)
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

        ' ✅ P1.6: also search sibling Overlay project output folder.
        ' When running from source, Engine's bin\Release\ is a sibling of
        ' Overlay's bin\Release\. Overlay always has api-core\ffmpeg.exe
        ' (it's the one that bundles it). Engine's project references it
        ' too but sometimes the build doesn't copy it (race with Overlay
        ' build, or .gitignore excludes the .exe). Searching Overlay's
        ' folder as a fallback is the pragmatic fix.
        Try
            Dim engineProjDir As String = appDir
            For depth As Integer = 1 To 6
                Dim parent As DirectoryInfo = Directory.GetParent(engineProjDir)
                If parent Is Nothing Then Exit For
                engineProjDir = parent.FullName
                ' Look for ..\Overlay\bin\Release\*\api-core\ffmpeg.exe
                Dim overlayBin As String = Path.Combine(parent.FullName, "Overlay", "bin")
                If Directory.Exists(overlayBin) Then
                    For Each configDir As String In {"Release", "Debug"}
                        Dim configPath_ As String = Path.Combine(overlayBin, configDir)
                        If Directory.Exists(configPath_) Then
                            ' Walk into the TFM subfolder (net8.0-windows10.0.26100.0)
                            For Each subDir As String In Directory.GetDirectories(configPath_)
                                Dim ffmpegCandidate As String = Path.Combine(subDir, "api-core", "ffmpeg.exe")
                                If File.Exists(ffmpegCandidate) Then
                                    candidates.Add(ffmpegCandidate)
                                End If
                                Dim ffmpegCandidate2 As String = Path.Combine(subDir, "API-Core", "ffmpeg.exe")
                                If File.Exists(ffmpegCandidate2) Then
                                    candidates.Add(ffmpegCandidate2)
                                End If
                            Next
                        End If
                    Next
                    Exit For ' found the Overlay/bin folder, no need to go higher
                End If
            Next
        Catch
        End Try

        For Each candidate In candidates
            If File.Exists(candidate) Then
                settings.FFmpegPath = candidate
                Exit For
            End If
        Next

        ' If still not found, leave it empty so the user knows to browse
        If String.IsNullOrEmpty(settings.FFmpegPath) Then
            settings.FFmpegPath = ""
        End If

        If String.IsNullOrEmpty(settings.OutputDirectory) Then
            settings.OutputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "ShadowPlay Recordings")
        End If

        If Not Directory.Exists(settings.OutputDirectory) Then
            Directory.CreateDirectory(settings.OutputDirectory)
        End If

        settings.Save(configPath)
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
