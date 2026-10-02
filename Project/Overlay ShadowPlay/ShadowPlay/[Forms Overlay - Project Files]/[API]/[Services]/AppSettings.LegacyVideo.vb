

Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json

Partial Public Class AppSettings
#Region "Legacy video.json (migration source only)"

    Public Class VideoConfigClass
        
        Public Property Encoder As String = "NVENC_H264"

        Public Property EncoderNow As String = "NVENC_H264"

        Public Property ActivePreset As String = "Medium"

        Public Property Current As New VideoCurrentValuesClass()

        Public Property ReplayDuration As Integer = 60

        Public Property Audio As New VideoAudioConfigClass()

        Public Property MyPresets As New VideoMyPresetsClass()

        Public Property APICapture As String = Nothing
    End Class

    Public Class VideoCurrentValuesClass
        Public Property FPS As Integer = 60
        Public Property Bitrate As Integer = 20000
        Public Property EncoderPreset As Integer = 4
        Public Property UseNativeResolution As Boolean = True
        Public Property Width As Integer = 0
        Public Property Height As Integer = 0
    End Class

    Public Class VideoAudioConfigClass
        Public Property SystemEnabled As Boolean = True
        Public Property MicEnabled As Boolean = False
        Public Property SystemVolume As Single = 1.0F
        Public Property MicVolume As Single = 1.0F
        Public Property MicDevice As String = ""
        Public Property MicDeviceId As String = ""
        Public Property TrackMode As Integer = 0
    End Class

    Public Class MyPresetSlotClass
        
        Public Property FPS As Integer? = Nothing

        Public Property Bitrate As Integer? = Nothing

        Public Property EncoderPreset As Integer? = Nothing
    End Class

    Public Class VideoMyPresetsClass
        
        Public Property Name As String = "MY"

        Public Property Low As New MyPresetSlotClass()
        Public Property Medium As New MyPresetSlotClass()
        Public Property High As New MyPresetSlotClass()
    End Class

    Private Sub MigrateLegacyConfigFiles()
        Try
            Dim imported As Boolean = False

            If _configWasMissingOnLoad Then
                
                If File.Exists(VideoConfigPath) Then
                    Dim video = LoadVideoSettings()
                    If video IsNot Nothing Then
                        ApplyVideoConfig(video)
                        imported = True
                        Debug.WriteLine("[Migrate] imported legacy video.json → config.json")
                    End If
                End If

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
            
            Debug.WriteLine($"[Migrate] rename {baseName} deferred: " & ex.Message)
        End Try
    End Sub

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

        Return Nothing
    End Function

    Private Sub ApplyVideoConfig(video As VideoConfigClass)
        If video Is Nothing Then Return

        Recording.Encoder = video.Encoder
        Recording.EncoderNow = video.EncoderNow
        Recording.Preset = video.ActivePreset
        Recording.ReplayDuration = video.ReplayDuration

        Recording.MyPresetName = video.MyPresets.Name

        Recording.APICapture = video.APICapture

        Recording.FPS = video.Current.FPS
        Recording.Bitrate = video.Current.Bitrate
        Recording.EncoderPreset = video.Current.EncoderPreset
        Recording.UseNativeResolution = video.Current.UseNativeResolution
        Recording.Width = If(video.Current.Width = 0, Recording.Width, video.Current.Width)
        Recording.Height = If(video.Current.Height = 0, Recording.Height, video.Current.Height)

        Audio.SystemAudioEnabled = video.Audio.SystemEnabled
        Audio.MicEnabled = video.Audio.MicEnabled
        Audio.SystemAudioVolume = video.Audio.SystemVolume
        Audio.MicVolume = video.Audio.MicVolume
        Audio.MicDeviceName = video.Audio.MicDevice
        Audio.MicDeviceId = video.Audio.MicDeviceId
        Audio.TrackMode = video.Audio.TrackMode

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

