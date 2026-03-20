Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Diagnostics
Imports Captrue_Core.CaptureCore

Public Class AppSettings

#Region "Settings Classes (Grouped) - สำหรับ JSON Serialization"

    ''' <summary>
    ''' Recording settings: encoder, fps, bitrate, resolution, preset
    ''' </summary>
    Public Class RecordingSettingsClass
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
    ''' Audio settings: microphone, volume
    ''' </summary>
    Public Class AudioSettingsClass
        Public Property MicEnabled As Boolean = False
        Public Property MicVolume As Integer = 100
        Public Property SystemVolume As Integer = 100

        Public Sub New()
        End Sub
    End Class

#End Region

#Region "Properties (Public สำหรับ JSON Serialization)"

    Public Property Recording As New RecordingSettingsClass()
    Public Property Paths As New PathSettingsClass()
    Public Property UI As New UISettingsClass()
    Public Property Audio As New AudioSettingsClass()

#End Region

#Region "Singleton"

    Private Shared _instance As AppSettings = Nothing
    Private Shared ReadOnly _lock As New Object()
    Private Shared _isLoaded As Boolean = False

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
    ''' Parameterless constructor สำหรับ JSON deserialization
    ''' </summary>
    Public Sub New()
        Recording = New RecordingSettingsClass()
        Paths = New PathSettingsClass()
        UI = New UISettingsClass()
        Audio = New AudioSettingsClass()
    End Sub

    ''' <summary>
    ''' ✅ Initialize และ Load config - เรียกตอน app start (optional)
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
            DetectHardware()
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
                Debug.WriteLine("JSON Content: " & json)

                If Not String.IsNullOrWhiteSpace(json) Then
                    Dim options As New JsonSerializerOptions With {
                        .PropertyNameCaseInsensitive = True,
                        .AllowTrailingCommas = True,
                        .ReadCommentHandling = JsonCommentHandling.Skip
                    }

                    Dim loaded As AppSettings = JsonSerializer.Deserialize(Of AppSettings)(json, options)

                    If loaded IsNot Nothing Then
                        ' Recording
                        If loaded.Recording IsNot Nothing Then
                            Recording.Encoder = loaded.Recording.Encoder
                            Recording.EncoderNow = loaded.Recording.EncoderNow
                            Recording.FPS = loaded.Recording.FPS
                            Recording.Bitrate = loaded.Recording.Bitrate
                            Recording.Width = loaded.Recording.Width
                            Recording.Height = loaded.Recording.Height
                            Recording.Preset = loaded.Recording.Preset
                            Recording.EncoderPreset = loaded.Recording.EncoderPreset
                            Recording.ReplayDuration = loaded.Recording.ReplayDuration
                        End If

                        ' Paths
                        If loaded.Paths IsNot Nothing Then
                            Paths.GalleryPath = loaded.Paths.GalleryPath
                            Paths.SavePath = loaded.Paths.SavePath
                            Paths.FFmpegPath = loaded.Paths.FFmpegPath
                        End If

                        ' UI
                        If loaded.UI IsNot Nothing Then
                            UI.Language = loaded.UI.Language
                            UI.Theme = loaded.UI.Theme
                        End If

                        ' Audio
                        If loaded.Audio IsNot Nothing Then
                            Audio.MicEnabled = loaded.Audio.MicEnabled
                            Audio.MicVolume = loaded.Audio.MicVolume
                            Audio.SystemVolume = loaded.Audio.SystemVolume
                        End If

                        Debug.WriteLine("AppSettings.Load: SUCCESS")
                        Debug.WriteLine($"  Encoder: {Recording.Encoder}")
                        Debug.WriteLine($"  FPS: {Recording.FPS}")
                        Debug.WriteLine($"  Bitrate: {Recording.Bitrate}")
                        Debug.WriteLine($"  Resolution: {Recording.Width}x{Recording.Height}")
                        Debug.WriteLine($"  Preset: {Recording.Preset}")
                        Debug.WriteLine($"  ReplayDuration: {Recording.ReplayDuration}s")
                    End If
                End If
            Else
                ' Create default config
                Save()
                Debug.WriteLine("AppSettings.Load: Created default config")
            End If

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Load Error: " & ex.Message)
            Debug.WriteLine("Stack: " & ex.StackTrace)
        End Try
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

            Debug.WriteLine("AppSettings.Save: SUCCESS to " & ConfigPath)

        Catch ex As Exception
            Debug.WriteLine("AppSettings.Save Error: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Apply to ScreenRecorder"

    ''' <summary>
    ''' Apply settings from config.json to ScreenRecorder
    ''' </summary>
    Public Sub ApplyToRecorder(recorder As ScreenRecorder)
        Try
            Debug.WriteLine("══════════ ApplyToRecorder ══════════")
            Debug.WriteLine("  Loading from AppSettings.Instance.Recording:")
            Debug.WriteLine("    Encoder: " & Recording.Encoder)
            Debug.WriteLine("    FPS: " & Recording.FPS)
            Debug.WriteLine("    Bitrate: " & Recording.Bitrate)
            Debug.WriteLine("    Resolution: " & Recording.Width & "x" & Recording.Height)
            Debug.WriteLine("    Preset: " & Recording.Preset)
            Debug.WriteLine("    EncoderPreset: " & Recording.EncoderPreset)
            Debug.WriteLine("    ReplayDuration: " & Recording.ReplayDuration)

            ' ═══════════════════════════════════════════════════════════════════════
            ' ✅ IMPORTANT: Set Preset FIRST (เพราะ setter จะทับค่าอื่นๆ)
            ' ═══════════════════════════════════════════════════════════════════════
            Select Case Recording.Preset
                Case "Low"
                    recorder.Preset = ScreenRecorder.RecordingPreset.Low
                Case "Medium"
                    recorder.Preset = ScreenRecorder.RecordingPreset.Medium
                Case "High"
                    recorder.Preset = ScreenRecorder.RecordingPreset.High
                Case "Custom"
                    recorder.Preset = ScreenRecorder.RecordingPreset.Custom
                Case Else
                    recorder.Preset = ScreenRecorder.RecordingPreset.Medium
            End Select

            ' ═══════════════════════════════════════════════════════════════════════
            ' ✅ Now apply custom settings (will override preset defaults)
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

            Debug.WriteLine("  Applied to recorder:")
            Debug.WriteLine("    recorder.Framerate = " & recorder.Framerate)
            Debug.WriteLine("    recorder.Bitrate = " & recorder.Bitrate)
            Debug.WriteLine("    recorder.Resolution = " & recorder.ResolutionWidth & "x" & recorder.ResolutionHeight)
            Debug.WriteLine("    recorder.Encoder = " & recorder.Encoder.ToString())
            Debug.WriteLine("    recorder.Preset = " & recorder.Preset.ToString())
            Debug.WriteLine("═════════════════════════════════════")

        Catch ex As Exception
            Debug.WriteLine("ApplyToRecorder Error: " & ex.Message)
        End Try
    End Sub

    Private Sub SetEncoder(recorder As ScreenRecorder, encoderName As String)
        Try
            Select Case encoderName
                Case "NVENC_H264"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.NVENC_H264
                Case "NVENC_HEVC"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.NVENC_HEVC
                Case "NVENC_AV1"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.NVENC_AV1
                Case "QuickSync_H264"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.QuickSync_H264
                Case "QuickSync_HEVC"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.QuickSync_HEVC
                Case "AMF_H264"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.AMF_H264
                Case "AMF_HEVC"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.AMF_HEVC
                Case "LibX264"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.LibX264
                Case "LibX265"
                    recorder.Encoder = ScreenRecorder.VideoEncoder.LibX265
                Case Else
                    ' Auto-select based on hardware
                    If _hasNvidia.GetValueOrDefault(False) Then
                        recorder.Encoder = ScreenRecorder.VideoEncoder.NVENC_H264
                    ElseIf _hasIntel.GetValueOrDefault(False) Then
                        recorder.Encoder = ScreenRecorder.VideoEncoder.QuickSync_H264
                    ElseIf _hasAMD.GetValueOrDefault(False) Then
                        recorder.Encoder = ScreenRecorder.VideoEncoder.AMF_H264
                    Else
                        recorder.Encoder = ScreenRecorder.VideoEncoder.LibX264
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

    ' ✅ NEW: Store all detected GPU names
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

    Private Shared _gpuName As String = ""
    Private Shared _intelGpuName As String = ""
    Private Shared _supportsAV1 As Boolean? = Nothing

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
        ' Not supported on: GTX 10xx, GTX 16xx, RTX 20xx, RTX 30xx
        Dim gpuUpper As String = _gpuName.ToUpperInvariant()

        If gpuUpper.Contains("RTX 40") OrElse
           gpuUpper.Contains("RTX 50") OrElse
           gpuUpper.Contains("ADA") Then
            _supportsAV1 = True
        End If

        Debug.WriteLine("AV1 Support: " & _supportsAV1.ToString() & " (GPU: " & _gpuName & ")")
    End Sub

    ''' <summary>
    ''' ✅ FIXED: Detect available GPUs - ตรวจจับทุก GPU อย่างอิสระ
    ''' </summary>
    Public Shared Sub DetectHardware()
        Try
            Debug.WriteLine("══════════ DetectHardware START ══════════")

            _hasNvidia = False
            _hasIntel = False
            _hasAMD = False
            _allGpuNames.Clear()

            ' ═════════════════════════════════════════════════════════════════
            ' Method 1: PowerShell Get-CimInstance (แทน wmic)
            ' ═════════════════════════════════════════════════════════════════
            DetectGPUsViaPowerShell()

            ' ═════════════════════════════════════════════════════════════════
            ' Method 2: Registry Detection (สำหรับ GPU ที่ PowerShell พลาด)
            ' ═════════════════════════════════════════════════════════════════
            DetectGPUsViaRegistry()

            ' ═════════════════════════════════════════════════════════════════
            ' Method 3: DLL Check (final fallback)
            ' ═════════════════════════════════════════════════════════════════
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

            ' ═════════════════════════════════════════════════════════════════
            ' ✅ REMOVED: Don't suppress Intel when there's dedicated GPU!
            ' Many systems have BOTH Intel iGPU + NVIDIA/AMD dGPU
            ' QuickSync is useful even with dedicated GPU
            ' ═════════════════════════════════════════════════════════════════

            ' Set primary GPU name
            If _hasNvidia.GetValueOrDefault(False) Then
                _gpuName = _allGpuNames.FirstOrDefault(Function(n) n.ToUpperInvariant().Contains("NVIDIA"), "NVIDIA GPU")
            ElseIf _hasAMD.GetValueOrDefault(False) Then
                _gpuName = _allGpuNames.FirstOrDefault(Function(n) n.ToUpperInvariant().Contains("AMD") OrElse n.ToUpperInvariant().Contains("RADEON"), "AMD GPU")
            ElseIf _hasIntel.GetValueOrDefault(False) Then
                _gpuName = _intelGpuName
            End If

            Debug.WriteLine("══════════ DetectHardware RESULT ══════════")
            Debug.WriteLine("  NVIDIA: " & _hasNvidia.ToString())
            Debug.WriteLine("  Intel:  " & _hasIntel.ToString() & If(_hasIntel.GetValueOrDefault(False), " (" & _intelGpuName & ")", ""))
            Debug.WriteLine("  AMD:    " & _hasAMD.ToString())
            Debug.WriteLine("  Primary GPU: " & _gpuName)
            Debug.WriteLine("═══════════════════════════════════════════")

        Catch ex As Exception
            Debug.WriteLine("DetectHardware Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' ✅ FIXED: Detect GPUs using PowerShell - ตรวจจับแต่ละ brand อย่างอิสระ
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

                        ' Add to list
                        _allGpuNames.Add(trimmed)

                        Dim upper As String = trimmed.ToUpperInvariant()

                        ' ═════════════════════════════════════════════════════════════════
                        ' ✅ FIXED: Detect each GPU brand INDEPENDENTLY
                        ' ═════════════════════════════════════════════════════════════════

                        ' NVIDIA Detection
                        If upper.Contains("NVIDIA") OrElse upper.Contains("GEFORCE") OrElse upper.Contains("GTX") OrElse upper.Contains("RTX") Then
                            _hasNvidia = True
                            Debug.WriteLine("  NVIDIA detected: " & trimmed)
                            Continue For
                        End If

                        ' AMD Detection
                        If upper.Contains("AMD") OrElse upper.Contains("RADEON") OrElse upper.Contains("RX ") Then
                            _hasAMD = True
                            Debug.WriteLine("  AMD detected: " & trimmed)
                            Continue For
                        End If

                        ' ✅ Intel Detection - Check independently!
                        ' Common Intel iGPU names: "Intel(R) UHD Graphics", "Intel(R) Iris(R) Xe Graphics"
                        If upper.Contains("INTEL") Then
                            ' Skip if it's Intel + NVIDIA/AMD combined string
                            If upper.Contains("NVIDIA") OrElse upper.Contains("AMD") OrElse upper.Contains("RADEON") Then
                                Continue For
                            End If

                            _hasIntel = True
                            _intelGpuName = trimmed
                            Debug.WriteLine("  Intel detected: " & trimmed)
                            Continue For
                        End If
                    Next
                End If
            End Using

        Catch ex As Exception
            Debug.WriteLine("DetectGPUsViaPowerShell Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' ✅ NEW: Detect GPUs via Windows Registry
    ''' </summary>
    Private Shared Sub DetectGPUsViaRegistry()
        Try
            ' GPU Registry path
            Const GPU_REGISTRY_PATH As String = "SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"

            Using key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(GPU_REGISTRY_PATH)
                If key Is Nothing Then Exit Sub

                For Each subKeyName As String In key.GetSubKeyNames()
                    Using subKey = key.OpenSubKey(subKeyName)
                        If subKey Is Nothing Then Continue For

                        Dim driverDesc As String = subKey.GetValue("DriverDesc", "").ToString()
                        Dim adapterString As String = subKey.GetValue("AdapterString", "").ToString()
                        Dim combined As String = (driverDesc & " " & adapterString).ToUpperInvariant()

                        If String.IsNullOrEmpty(driverDesc) AndAlso String.IsNullOrEmpty(adapterString) Then
                            Continue For
                        End If

                        ' NVIDIA
                        If combined.Contains("NVIDIA") OrElse combined.Contains("GEFORCE") Then
                            If Not _hasNvidia.GetValueOrDefault(False) Then
                                _hasNvidia = True
                                _allGpuNames.Add(driverDesc)
                                Debug.WriteLine("  NVIDIA detected via Registry: " & driverDesc)
                            End If
                        End If

                        ' AMD
                        If combined.Contains("AMD") OrElse combined.Contains("RADEON") Then
                            If Not _hasAMD.GetValueOrDefault(False) Then
                                _hasAMD = True
                                _allGpuNames.Add(driverDesc)
                                Debug.WriteLine("  AMD detected via Registry: " & driverDesc)
                            End If
                        End If

                        ' ═════════════════════════════════════════════════════════════════
                        ' ✅ Intel iGPU Detection via Registry
                        ' ═════════════════════════════════════════════════════════════════
                        If combined.Contains("INTEL") AndAlso
                           Not combined.Contains("NVIDIA") AndAlso
                           Not combined.Contains("AMD") AndAlso
                           Not combined.Contains("RADEON") Then

                            ' Additional check: Intel iGPU usually has these keywords
                            If combined.Contains("UHD") OrElse
                               combined.Contains("IRIS") OrElse
                               combined.Contains("HD GRAPHICS") OrElse
                               combined.Contains("INTEL(R) GRAPHICS") Then

                                If Not _hasIntel.GetValueOrDefault(False) Then
                                    _hasIntel = True
                                    _intelGpuName = driverDesc
                                    _allGpuNames.Add(driverDesc)
                                    Debug.WriteLine("  Intel iGPU detected via Registry: " & driverDesc)
                                End If
                            End If
                        End If
                    End Using
                Next
            End Using

        Catch ex As Exception
            Debug.WriteLine("DetectGPUsViaRegistry Error: " & ex.Message)
        End Try
    End Sub

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
        Save()
        Debug.WriteLine("AppSettings.ResetDefaults: Done")
    End Sub

#End Region

End Class
