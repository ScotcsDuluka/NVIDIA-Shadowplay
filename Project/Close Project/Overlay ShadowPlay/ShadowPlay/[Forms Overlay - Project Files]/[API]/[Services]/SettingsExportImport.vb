Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

Public Class SettingsExportImport

#Region "JSON Structure for Export/Import"

    Public Class PortableSettings
        Public Property Recording As AppSettings.RecordingSettingsClass
        Public Property Audio As AppSettings.AudioSettingsClass
        Public Property UI As AppSettings.UISettingsClass
        Public Property ExportVersion As String = "1.0"
        Public Property ExportDate As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

        Public Sub New()
        End Sub
    End Class
#End Region

#Region "Export"

    Public Shared Function ExportToFile(filePath As String) As Boolean
        Try
            If String.IsNullOrEmpty(filePath) Then Return False

            Dim settings As New PortableSettings() With {
                .Recording = CloneRecording(AppSettings.Instance.Recording),
                .Audio = CloneAudio(AppSettings.Instance.Audio),
                .UI = CloneUI(AppSettings.Instance.UI)
            }

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True,
                .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }

            Dim json As String = JsonSerializer.Serialize(settings, options)
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8)

            Debug.WriteLine($"SettingsExportImport.Export: Saved to {filePath}")
            Return True

        Catch ex As Exception
            Debug.WriteLine($"SettingsExportImport.Export Error: {ex.Message}")
            Return False
        End Try
    End Function

    Public Shared Function ExportWithDialog(owner As Form) As String
        Using sfd As New SaveFileDialog()
            sfd.Filter = "Settings File (*.json)|*.json"
            sfd.DefaultExt = ".json"
            sfd.FileName = "ShadowPlay_Settings_" & DateTime.Now.ToString("yyyyMMdd_HHmmss")
            sfd.Title = LangHelper.GetText("l10n.exportSettings")

            If sfd.ShowDialog(owner) = DialogResult.OK Then
                If ExportToFile(sfd.FileName) Then
                    Return sfd.FileName
                End If
            End If
        End Using

        Return Nothing
    End Function
#End Region

#Region "Import"

    Public Shared Function ImportFromFile(filePath As String) As Boolean
        Try
            If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then Return False

            Dim json As String = File.ReadAllText(filePath, System.Text.Encoding.UTF8)

            Dim options As New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True,
                .AllowTrailingCommas = True,
                .ReadCommentHandling = JsonCommentHandling.Skip
            }

            Dim imported As PortableSettings = JsonSerializer.Deserialize(Of PortableSettings)(json, options)

            If imported Is Nothing Then Return False

            If imported.Recording IsNot Nothing Then
                ApplyImportedRecording(imported.Recording)
            End If

            If imported.Audio IsNot Nothing Then
                ApplyImportedAudio(imported.Audio)
            End If

            If imported.UI IsNot Nothing Then
                AppSettings.Instance.UI.Language = imported.UI.Language
            End If

            AppSettings.Instance.Save()

            Try
                If Base.tcp IsNot Nothing Then
                    Base.tcp.Send("engine_config_changed", "")
                End If
            Catch ex As Exception
                Debug.WriteLine($"SettingsExportImport: engine_config_changed failed: {ex.Message}")
            End Try

            Debug.WriteLine($"SettingsExportImport.Import: Loaded from {filePath}")
            Return True

        Catch ex As Exception
            Debug.WriteLine($"SettingsExportImport.Import Error: {ex.Message}")
            Return False
        End Try
    End Function

    Public Shared Function ImportWithDialog(owner As Form) As Boolean
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Settings File (*.json)|*.json"
            ofd.Title = LangHelper.GetText("l10n.importSettings")

            If ofd.ShowDialog(owner) = DialogResult.OK Then
                Return ImportFromFile(ofd.FileName)
            End If
        End Using

        Return False
    End Function
#End Region

#Region "Clone Helpers (deep copy to avoid mutating original)"
    Private Shared Function CloneRecording(src As AppSettings.RecordingSettingsClass) As AppSettings.RecordingSettingsClass
        Dim clone As New AppSettings.RecordingSettingsClass()
        clone.UseNativeResolution = src.UseNativeResolution
        clone.Encoder = src.Encoder
        clone.EncoderNow = src.EncoderNow
        clone.FPS = src.FPS
        clone.Bitrate = src.Bitrate
        clone.Width = src.Width
        clone.Height = src.Height
        clone.Preset = src.Preset
        clone.EncoderPreset = src.EncoderPreset
        clone.ReplayDuration = src.ReplayDuration
        clone.MyLowFPS = src.MyLowFPS
        clone.MyLowBitrate = src.MyLowBitrate
        clone.MyLowEncoderPreset = src.MyLowEncoderPreset
        clone.MyMediumFPS = src.MyMediumFPS
        clone.MyMediumBitrate = src.MyMediumBitrate
        clone.MyMediumEncoderPreset = src.MyMediumEncoderPreset
        clone.MyHighFPS = src.MyHighFPS
        clone.MyHighBitrate = src.MyHighBitrate
        clone.MyHighEncoderPreset = src.MyHighEncoderPreset
        Return clone
    End Function

    Private Shared Function CloneAudio(src As AppSettings.AudioSettingsClass) As AppSettings.AudioSettingsClass
        Dim clone As New AppSettings.AudioSettingsClass()
        clone.SystemAudioEnabled = src.SystemAudioEnabled
        clone.MicEnabled = src.MicEnabled
        clone.SystemAudioVolume = src.SystemAudioVolume
        clone.MicVolume = src.MicVolume
        clone.MicDeviceName = src.MicDeviceName

        clone.MicDeviceId = src.MicDeviceId
        clone.TrackMode = src.TrackMode
        clone.AudioClockMode = src.AudioClockMode
        Return clone
    End Function

    Private Shared Function CloneUI(src As AppSettings.UISettingsClass) As AppSettings.UISettingsClass
        Dim clone As New AppSettings.UISettingsClass()
        clone.Language = src.Language
        clone.Theme = src.Theme
        Return clone
    End Function
#End Region

#Region "Apply Imported Settings"
    Private Shared Sub ApplyImportedRecording(imported As AppSettings.RecordingSettingsClass)
        Dim rec = AppSettings.Instance.Recording

        rec.Encoder = imported.Encoder
        rec.EncoderNow = imported.EncoderNow
        rec.FPS = imported.FPS
        rec.Bitrate = imported.Bitrate
        rec.Preset = imported.Preset
        rec.EncoderPreset = imported.EncoderPreset
        rec.ReplayDuration = imported.ReplayDuration
        rec.UseNativeResolution = imported.UseNativeResolution

        If Not imported.UseNativeResolution Then
            rec.Width = imported.Width
            rec.Height = imported.Height
        End If

        If imported.MyLowFPS.HasValue Then rec.MyLowFPS = imported.MyLowFPS
        If imported.MyLowBitrate.HasValue Then rec.MyLowBitrate = imported.MyLowBitrate
        If imported.MyLowEncoderPreset.HasValue Then rec.MyLowEncoderPreset = imported.MyLowEncoderPreset

        If imported.MyMediumFPS.HasValue Then rec.MyMediumFPS = imported.MyMediumFPS
        If imported.MyMediumBitrate.HasValue Then rec.MyMediumBitrate = imported.MyMediumBitrate
        If imported.MyMediumEncoderPreset.HasValue Then rec.MyMediumEncoderPreset = imported.MyMediumEncoderPreset

        If imported.MyHighFPS.HasValue Then rec.MyHighFPS = imported.MyHighFPS
        If imported.MyHighBitrate.HasValue Then rec.MyHighBitrate = imported.MyHighBitrate
        If imported.MyHighEncoderPreset.HasValue Then rec.MyHighEncoderPreset = imported.MyHighEncoderPreset
    End Sub

    Private Shared Sub ApplyImportedAudio(imported As AppSettings.AudioSettingsClass)
        Dim aud = AppSettings.Instance.Audio

        aud.SystemAudioEnabled = imported.SystemAudioEnabled
        aud.MicEnabled = imported.MicEnabled
        aud.SystemAudioVolume = imported.SystemAudioVolume
        aud.MicVolume = imported.MicVolume

        aud.TrackMode = imported.TrackMode
        aud.AudioClockMode = imported.AudioClockMode
    End Sub
#End Region

End Class
