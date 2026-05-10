Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>
''' Export / Import recording settings to/from a .json file.
''' Does NOT export machine-specific paths or GitHub credentials.
''' </summary>
Public Class SettingsExportImport

#Region "JSON Structure for Export/Import"
    ''' <summary>
    ''' Portable settings DTO — only recording + audio + UI settings.
    ''' Paths, GitHub credentials, and hotkeys are excluded.
    ''' </summary>
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
    ''' <summary>
    ''' Export current recording/audio/UI settings to a JSON file.
    ''' Returns True on success, False on failure.
    ''' </summary>
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

    ''' <summary>
    ''' Show SaveFileDialog and export settings.
    ''' Returns the saved file path, or Nothing if cancelled/failed.
    ''' </summary>
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
    ''' <summary>
    ''' Import settings from a JSON file and apply to AppSettings.
    ''' Returns True on success, False on failure.
    ''' Does NOT overwrite paths, GitHub credentials, or hotkeys.
    ''' </summary>
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

            ' Apply recording settings
            If imported.Recording IsNot Nothing Then
                ApplyImportedRecording(imported.Recording)
            End If

            ' Apply audio settings
            If imported.Audio IsNot Nothing Then
                ApplyImportedAudio(imported.Audio)
            End If

            ' Apply UI settings (language only, not theme)
            If imported.UI IsNot Nothing Then
                AppSettings.Instance.UI.Language = imported.UI.Language
            End If

            AppSettings.Instance.Save()

            Debug.WriteLine($"SettingsExportImport.Import: Loaded from {filePath}")
            Return True

        Catch ex As Exception
            Debug.WriteLine($"SettingsExportImport.Import Error: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Show OpenFileDialog and import settings.
    ''' Returns True if imported successfully, False if cancelled/failed.
    ''' </summary>
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

        ' Only apply resolution if NOT native (native auto-detects)
        If Not imported.UseNativeResolution Then
            rec.Width = imported.Width
            rec.Height = imported.Height
        End If

        ' My Preset values (only overwrite if the import has them)
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
        ' Do NOT import MicDeviceName — it's machine-specific
    End Sub
#End Region

End Class
