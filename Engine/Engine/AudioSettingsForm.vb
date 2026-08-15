Imports System.IO

Public Class AudioSettingsForm

    Private _settings As CaptureSettings
    Private _configPath As String
    Private _overlayVideoPath As String

    Public Sub New(settings As CaptureSettings, configPath As String, overlayVideoPath As String)
        InitializeComponent()
        _settings = settings
        _configPath = configPath
        _overlayVideoPath = overlayVideoPath
    End Sub

    Private Sub AudioSettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RefreshMicDevices()
        LoadFromSettings()
        UpdateVolumeLabels()
    End Sub

    Private Sub LoadFromSettings()
        If _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.Separate Then
            radSeparate.Checked = True
        Else
            radSingle.Checked = True
        End If

        chkSystem.Checked = _settings.SystemAudioCapture
        chkMic.Checked = _settings.MicCapture

        trkSystemVol.Value = CInt(Math.Max(0, Math.Min(150, _settings.SystemAudioVolume * 100)))
        trkMicVol.Value = CInt(Math.Max(0, Math.Min(150, _settings.MicVolume * 100)))

        If Not String.IsNullOrEmpty(_settings.MicDeviceName) Then
            For i As Integer = 0 To cboMic.Items.Count - 1
                If CStr(cboMic.Items(i)) = _settings.MicDeviceName Then
                    cboMic.SelectedIndex = i
                    Exit For
                End If
            Next
        End If
        If cboMic.SelectedIndex < 0 AndAlso cboMic.Items.Count > 0 Then
            cboMic.SelectedIndex = 0
        End If
    End Sub

    Private Sub SaveToSettings()
        If radSeparate.Checked Then
            _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.Separate
        Else
            _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.Single
        End If

        _settings.SystemAudioCapture = chkSystem.Checked
        _settings.MicCapture = chkMic.Checked

        _settings.SystemAudioVolume = CSng(trkSystemVol.Value) / 100.0F
        _settings.MicVolume = CSng(trkMicVol.Value) / 100.0F

        If cboMic.SelectedItem IsNot Nothing Then
            _settings.MicDeviceName = CStr(cboMic.SelectedItem)
        End If
        _settings.AudioCapture = _settings.SystemAudioCapture OrElse _settings.MicCapture

        Try
            _settings.Save(_configPath)
        Catch ex As Exception
            MessageBox.Show(Me, "Failed to save config: " & ex.Message, "Save error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try

        Try
            SaveToOverlayVideoJson()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub SaveToOverlayVideoJson()
        If String.IsNullOrEmpty(_overlayVideoPath) OrElse Not File.Exists(_overlayVideoPath) Then Return

        Try
            Dim json As String = File.ReadAllText(_overlayVideoPath)
            Dim root As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.Linq.JObject.Parse(json)

            Dim audioTok As Newtonsoft.Json.Linq.JToken = root("audio")
            If audioTok Is Nothing Then
                audioTok = New Newtonsoft.Json.Linq.JObject()
                root("audio") = audioTok
            End If
            Dim audio As Newtonsoft.Json.Linq.JObject = TryCast(audioTok, Newtonsoft.Json.Linq.JObject)
            If audio Is Nothing Then Return

            audio("system_enabled") = _settings.SystemAudioCapture
            audio("mic_enabled") = _settings.MicCapture
            audio("system_volume") = _settings.SystemAudioVolume
            audio("mic_volume") = _settings.MicVolume
            audio("mic_device") = If(_settings.MicDeviceName, "")

            File.WriteAllText(_overlayVideoPath, root.ToString(Newtonsoft.Json.Formatting.Indented))
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[AudioSettingsForm] SaveToOverlayVideoJson error: " & ex.Message)
        End Try
    End Sub

    Private Sub RefreshMicDevices()
        cboMic.Items.Clear()
        Try
            For Each name As String In NAudioCaptureEngine.ListMicDevices()
                cboMic.Items.Add(name)
            Next
        Catch
        End Try
        lblStatus.Text = cboMic.Items.Count.ToString() & " mic(s) found"
    End Sub

    Private Sub UpdateVolumeLabels()
        lblSystemVol.Text = trkSystemVol.Value.ToString() & "%"
        lblMicVol.Text = trkMicVol.Value.ToString() & "%"
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        RefreshMicDevices()
        If Not String.IsNullOrEmpty(_settings.MicDeviceName) Then
            For i As Integer = 0 To cboMic.Items.Count - 1
                If CStr(cboMic.Items(i)) = _settings.MicDeviceName Then
                    cboMic.SelectedIndex = i
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub trkSystemVol_Scroll(sender As Object, e As EventArgs) Handles trkSystemVol.Scroll
        UpdateVolumeLabels()
    End Sub

    Private Sub trkMicVol_Scroll(sender As Object, e As EventArgs) Handles trkMicVol.Scroll
        UpdateVolumeLabels()
    End Sub

    Private Sub btnApply_Click(sender As Object, e As EventArgs) Handles btnApply.Click
        SaveToSettings()
        lblStatus.Text = "Saved."
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnTest_Click(sender As Object, e As EventArgs) Handles btnTest.Click
        SaveToSettings()
        lblStatus.Text = "Settings saved. Start recording to test."
    End Sub

End Class
