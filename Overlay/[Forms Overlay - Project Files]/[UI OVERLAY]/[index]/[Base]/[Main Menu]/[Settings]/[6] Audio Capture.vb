' NOTE: no 'Option Strict On' here — the Overlay project compiles with
' strict OFF (project-wide convention). This file follows the same
' convention as every other Overlay form.

Imports System.IO

Public Class Base_AudioSet

#Region "Load / refresh"

    Private Sub Base_AudioSet_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cboMic.DisplayMember = "Item2"
        RefreshMicDevices()
        LoadFromSettings()
        UpdateVolumeLabels()
    End Sub

    ''' <summary>Push AppSettings audio model → controls (same semantics as the old Engine form).</summary>
    Private Sub LoadFromSettings()
        Dim audio As AudioSettingsClass = AppSettings.Instance.Audio

        If audio.TrackMode = 1 Then
            radSeparate.Checked = True
        Else
            radSingle.Checked = True
        End If

        chkSystem.Checked = audio.SystemAudioEnabled
        chkMic.Checked = audio.MicEnabled

        trkSystemVol.Value = CInt(Math.Max(0, Math.Min(150, audio.SystemAudioVolume * 100.0F)))
        trkMicVol.Value = CInt(Math.Max(0, Math.Min(150, audio.MicVolume * 100.0F)))

        SelectCurrentMic()
    End Sub

    ''' <summary>Select the saved mic (by Id first, then by name) — mirrors old form logic.</summary>
    Private Sub SelectCurrentMic()
        Dim audio As AudioSettingsClass = AppSettings.Instance.Audio
        Dim micId As String = audio.MicDeviceId
        Dim micName As String = audio.MicDeviceName

        If Not String.IsNullOrEmpty(micId) OrElse Not String.IsNullOrEmpty(micName) Then
            For i As Integer = 0 To cboMic.Items.Count - 1
                Dim item As Tuple(Of String, String) = TryCast(cboMic.Items(i), Tuple(Of String, String))
                If item Is Nothing Then Continue For
                If (Not String.IsNullOrEmpty(micId) AndAlso item.Item1 = micId) OrElse
                   (Not String.IsNullOrEmpty(micName) AndAlso item.Item2 = micName) Then
                    cboMic.SelectedIndex = i
                    Exit For
                End If
            Next
        End If
        If cboMic.SelectedIndex < 0 AndAlso cboMic.Items.Count > 0 Then
            cboMic.SelectedIndex = 0
        End If
    End Sub

    ''' <summary>Fill the mic dropdown from NAudio device enumeration (Engine project helper).</summary>
    Private Sub RefreshMicDevices()
        cboMic.Items.Clear()
        Try
            For Each dev As Tuple(Of String, String) In NVIDIA_Capture.AudioFileWriter.ListMicDevices()
                cboMic.Items.Add(dev)
            Next
        Catch ex As Exception
            Debug.WriteLine("[AudioSet] ListMicDevices failed: " & ex.Message)
        End Try
        lblStatus.Text = cboMic.Items.Count.ToString() & " mic(s) found"
    End Sub

    Private Sub UpdateVolumeLabels()
        lblSystemVol.Text = trkSystemVol.Value.ToString() & "%"
        lblMicVol.Text = trkMicVol.Value.ToString() & "%"
    End Sub

#End Region

#Region "Save"

    ''' <summary>
    ''' Controls → AppSettings → config.json + video.json(audio) + audio.json +
    ''' engine_config_changed broadcast. Same persistence set the old Engine
    ''' form produced, now owned by the Overlay.
    ''' </summary>
    Private Sub SaveToSettings()
        Dim audio As AudioSettingsClass = AppSettings.Instance.Audio

        audio.TrackMode = If(radSeparate.Checked, 1, 0)
        audio.SystemAudioEnabled = chkSystem.Checked
        audio.MicEnabled = chkMic.Checked
        audio.SystemAudioVolume = CSng(trkSystemVol.Value) / 100.0F
        audio.MicVolume = CSng(trkMicVol.Value) / 100.0F

        If cboMic.SelectedItem IsNot Nothing Then
            Dim item As Tuple(Of String, String) = TryCast(cboMic.SelectedItem, Tuple(Of String, String))
            If item IsNot Nothing Then
                audio.MicDeviceId = item.Item1
                audio.MicDeviceName = item.Item2
            End If
        End If

        ' 1) Overlay's unified config.json — THE single config file
        ' (GLM/6: Recording + Audio + Paths all persist here)
        Try
            AppSettings.Instance.Save()
        Catch ex As Exception
            Debug.WriteLine("[AudioSet] AppSettings.Save error: " & ex.Message)
        End Try

        ' 2) Tell the Engine to reload now (don't wait for its file poll)
        Try
            If Base.tcp IsNot Nothing AndAlso Base.tcp.IsConnected Then
                Base.tcp.Send("engine_config_changed", "video")
            End If
        Catch ex As Exception
            Debug.WriteLine("[AudioSet] engine_config_changed broadcast failed: " & ex.Message)
        End Try
    End Sub

 


#End Region

#Region "UI handlers"

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        RefreshMicDevices()
        SelectCurrentMic()
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
    End Sub

    Private Sub btnTest_Click(sender As Object, e As EventArgs) Handles btnTest.Click
        SaveToSettings()
        lblStatus.Text = "Settings saved. Start recording to test."
    End Sub

    ''' <summary>Back → settings home (same contract as Base_RecordingsSet.action_fn_Click).</summary>
    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Try
            SaveToSettings()
            Me.Hide()
            Base_Settings.Show()
            Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
            Base.Settings_List.Visible = True
        Catch ex As Exception
            Debug.WriteLine("[AudioSet] action_fn_Click error: " & ex.Message)
        End Try
    End Sub

#End Region

End Class
