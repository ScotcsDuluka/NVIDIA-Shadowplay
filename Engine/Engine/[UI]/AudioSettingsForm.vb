Imports System.IO
Imports System.Runtime.InteropServices
Imports Newtonsoft.Json.Linq

Public Class AudioSettingsForm
    Const WS_EX_TRANSPARENT As Integer = &H20

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If m.Msg = WM_NCHITTEST Then
            Dim pos As Point = Me.PointToClient(Cursor.Position)
            If Me.GetChildAtPoint(pos) Is Nothing Then
                m.Result = CType(HTTRANSPARENT, IntPtr)
                Return
            End If
        End If

        MyBase.WndProc(m)
    End Sub

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Private Sub HideFromAltTab()
        If Me.IsDisposed OrElse Me.Disposing Then Return
        If Not Me.IsHandleCreated Then Return

        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub

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
        cboMic.DisplayMember = "Item2"
        RefreshMicDevices()
        LoadFromSettings()
        UpdateVolumeLabels()
        OPEN_UI.Start()
    End Sub

    Private Sub LoadFromSettings()
        If _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack Then
            radSeparate.Checked = True
        Else
            radSingle.Checked = True
        End If

        chkSystem.Checked = _settings.SystemAudioCapture
        chkMic.Checked = _settings.MicCapture

        trkSystemVol.Value = CInt(Math.Max(0, Math.Min(150, _settings.SystemAudioVolume * 100)))
        trkMicVol.Value = CInt(Math.Max(0, Math.Min(150, _settings.MicVolume * 100)))

        Dim micId As String = _settings.MicDeviceId
        Dim micName As String = _settings.MicDeviceName

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

    Private Sub SaveToSettings()
        If radSeparate.Checked Then
            _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack
        Else
            _settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SingleTrack
        End If

        _settings.SystemAudioCapture = chkSystem.Checked
        _settings.MicCapture = chkMic.Checked

        _settings.SystemAudioVolume = CSng(trkSystemVol.Value) / 100.0F
        _settings.MicVolume = CSng(trkMicVol.Value) / 100.0F

        If cboMic.SelectedItem IsNot Nothing Then
            Dim item As Tuple(Of String, String) = TryCast(cboMic.SelectedItem, Tuple(Of String, String))
            If item IsNot Nothing Then
                _settings.MicDeviceId = item.Item1
                _settings.MicDeviceName = item.Item2
            End If
        End If
        _settings.AudioCapture = _settings.SystemAudioCapture OrElse _settings.MicCapture

        ' Save to audio.json (new unified config)
        Dim audioJsonPath As String = AppLayout.P("Config", "audio.json")
        _settings.SaveAudio(audioJsonPath)

        ' Also save engine settings to engine.json
        Dim engineJsonPath As String = AppLayout.P("Config", "engine.json")
        _settings.Save(engineJsonPath)

        ' Also save to Overlay's video.json audio section (backward compat with Overlay)
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
            audio("mic_device_id") = If(_settings.MicDeviceId, "")

            AppLayout.EnsureParentDir(_overlayVideoPath)   ' Config\ is runtime-created
            File.WriteAllText(_overlayVideoPath, root.ToString(Newtonsoft.Json.Formatting.Indented))
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[AudioSettingsForm] SaveToOverlayVideoJson error: " & ex.Message)
        End Try
    End Sub

    Private Sub RefreshMicDevices()
        cboMic.Items.Clear()
        Try
            For Each dev As Tuple(Of String, String) In AudioFileWriter.ListMicDevices()
                cboMic.Items.Add(dev)
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
        Dim micId As String = _settings.MicDeviceId
        Dim micName As String = _settings.MicDeviceName
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

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Dim uiFile = AppLayout.P("Flags", "Audio.UI")
        Try
            If File.Exists(uiFile) Then
                File.Delete(uiFile)
            End If
        Catch ex As IOException
        End Try
    End Sub

    Private Sub btnTest_Click(sender As Object, e As EventArgs) Handles btnTest.Click
        SaveToSettings()
        lblStatus.Text = "Settings saved. Start recording to test."
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        Dim uiFile = AppLayout.P("Flags", "Audio.UI")
        Try
            If File.Exists(uiFile) Then
                File.Delete(uiFile)
            End If
        Catch ex As IOException
        End Try
    End Sub

    Private Sub OPEN_UI_Tick(sender As Object, e As EventArgs) Handles OPEN_UI.Tick
        HideFromAltTab()

        Dim uiFile = AppLayout.P("Flags", "Audio.UI")

        If File.Exists(uiFile) Then
            Me.WindowState = FormWindowState.Maximized
            Me.Opacity = 1

            If Not Me.Visible Then
                Me.Show()
            End If
        Else
            Me.Opacity = 0
            Me.WindowState = FormWindowState.Minimized
        End If
    End Sub

    Private Sub BT_Back_MouseMove(sender As Object, e As MouseEventArgs) Handles BT_Back.MouseMove
        BT_Back.BackColor = Color.Green
    End Sub

    Private Sub BT_Back_MouseLeave(sender As Object, e As EventArgs) Handles BT_Back.MouseLeave
        BT_Back.BackColor = Color.FromArgb(118, 185, 0)
    End Sub
End Class
