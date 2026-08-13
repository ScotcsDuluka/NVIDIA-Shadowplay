' UI_Engine.Designer.vb
' ShadowPlay Engine - Settings Form Layout
' Dark NVIDIA-style theme

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UI_Engine
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    ' ── Color Scheme ──

    Private _bgDark As Drawing.Color = Drawing.Color.FromArgb(32, 32, 36)
    Private _bgPanel As Drawing.Color = Drawing.Color.FromArgb(44, 44, 48)
    Private _bgInput As Drawing.Color = Drawing.Color.FromArgb(55, 55, 60)
    Private _bgButton As Drawing.Color = Drawing.Color.FromArgb(118, 185, 0)
    Private _bgButtonStop As Drawing.Color = Drawing.Color.FromArgb(200, 50, 50)
    Private _fgText As Drawing.Color = Drawing.Color.FromArgb(230, 230, 230)
    Private _fgDim As Drawing.Color = Drawing.Color.FromArgb(160, 160, 160)
    Private _accentGreen As Drawing.Color = Drawing.Color.FromArgb(118, 185, 0)
    Private _accentRed As Drawing.Color = Drawing.Color.FromArgb(200, 50, 50)

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UI_Engine))
        lblTitle = New Label()
        lblStatus = New Label()
        lblTimer = New Label()
        pnlCapture = New Panel()
        lblCapTitle = New Label()
        cboCaptureMethod = New ComboBox()
        pnlEncoder = New Panel()
        lblEncTitle = New Label()
        cboEncoder = New ComboBox()
        btnDetect = New Button()
        pnlRes = New Panel()
        chkNativeRes = New CheckBox()
        cboResolution = New ComboBox()
        pnlPerf = New Panel()
        lblFPSTitle = New Label()
        nudFPS = New NumericUpDown()
        lblBitTitle = New Label()
        nudBitrate = New NumericUpDown()
        lblBitrateHint = New Label()
        pnlPreset = New Panel()
        lblPresetTitle = New Label()
        lblPresetValue = New Label()
        lblNvencPreset = New Label()
        lblReplayTitle = New Label()
        nudReplayDuration = New NumericUpDown()
        pnlAudio = New Panel()
        lblAudioTitle = New Label()
        chkSysAudio = New CheckBox()
        chkMic = New CheckBox()
        lblSysVol = New Label()
        trkSysVol = New TrackBar()
        lblMicVol = New Label()
        trkMicVol = New TrackBar()
        lblMicDevice = New Label()
        pnlOutput = New Panel()
        lblOutTitle = New Label()
        txtOutputDir = New TextBox()
        btnBrowse = New Button()
        pnlFFmpeg = New Panel()
        lblFfmpegTitle = New Label()
        txtFFmpegPath = New TextBox()
        btnFFmpegBrowse = New Button()
        lblFFmpegStatus = New Label()
        pnlGitHub = New Panel()
        lblGitHubTitle = New Label()
        lblGitHubUser = New Label()
        lblGitHubStatus = New Label()
        pnlHub = New Panel()
        lblHubTitle = New Label()
        lblHubStatus = New Label()
        lblHubClients = New Label()
        lblConfigSource = New Label()
        btnRecord = New Button()
        btnStop = New Button()
        lblHotkeys = New Label()
        tmrRecording = New System.Windows.Forms.Timer(components)
        tmrRefresh = New System.Windows.Forms.Timer(components)
        pnlCapture.SuspendLayout()
        pnlEncoder.SuspendLayout()
        pnlRes.SuspendLayout()
        pnlPerf.SuspendLayout()
        CType(nudFPS, ComponentModel.ISupportInitialize).BeginInit()
        CType(nudBitrate, ComponentModel.ISupportInitialize).BeginInit()
        pnlPreset.SuspendLayout()
        CType(nudReplayDuration, ComponentModel.ISupportInitialize).BeginInit()
        pnlAudio.SuspendLayout()
        CType(trkSysVol, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkMicVol, ComponentModel.ISupportInitialize).BeginInit()
        pnlOutput.SuspendLayout()
        pnlFFmpeg.SuspendLayout()
        pnlGitHub.SuspendLayout()
        pnlHub.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Segoe UI", 16F, FontStyle.Bold)
        lblTitle.Location = New Point(20, 15)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(179, 30)
        lblTitle.TabIndex = 0
        lblTitle.Text = "NVIDIA Capture"
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Font = New Font("Segoe UI", 10F)
        lblStatus.Location = New Point(20, 48)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(31, 19)
        lblStatus.TabIndex = 1
        lblStatus.Text = "Idle"
        ' 
        ' lblTimer
        ' 
        lblTimer.AutoSize = True
        lblTimer.Font = New Font("Consolas", 20F, FontStyle.Bold)
        lblTimer.Location = New Point(20, 70)
        lblTimer.Name = "lblTimer"
        lblTimer.Size = New Size(134, 32)
        lblTimer.TabIndex = 2
        lblTimer.Text = "00:00:00"
        ' 
        ' pnlCapture
        ' 
        pnlCapture.Controls.Add(lblCapTitle)
        pnlCapture.Controls.Add(cboCaptureMethod)
        pnlCapture.Location = New Point(15, 105)
        pnlCapture.Name = "pnlCapture"
        pnlCapture.Size = New Size(490, 70)
        pnlCapture.TabIndex = 3
        ' 
        ' lblCapTitle
        ' 
        lblCapTitle.AutoSize = True
        lblCapTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCapTitle.Location = New Point(12, 8)
        lblCapTitle.Name = "lblCapTitle"
        lblCapTitle.Size = New Size(98, 15)
        lblCapTitle.TabIndex = 0
        lblCapTitle.Text = "Capture Method"
        ' 
        ' cboCaptureMethod
        ' 
        cboCaptureMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboCaptureMethod.Font = New Font("Segoe UI", 9F)
        cboCaptureMethod.Items.AddRange(New Object() {"ddagrab - Desktop Duplication (DXGI)", "gdigrab - GDI Screen Capture", "gfxcapture - DXGI Desktop Dup (NV)"})
        cboCaptureMethod.Location = New Point(12, 30)
        cboCaptureMethod.Name = "cboCaptureMethod"
        cboCaptureMethod.Size = New Size(466, 23)
        cboCaptureMethod.TabIndex = 1
        ' 
        ' pnlEncoder
        ' 
        pnlEncoder.Controls.Add(lblEncTitle)
        pnlEncoder.Controls.Add(cboEncoder)
        pnlEncoder.Controls.Add(btnDetect)
        pnlEncoder.Location = New Point(15, 185)
        pnlEncoder.Name = "pnlEncoder"
        pnlEncoder.Size = New Size(490, 70)
        pnlEncoder.TabIndex = 4
        ' 
        ' lblEncTitle
        ' 
        lblEncTitle.AutoSize = True
        lblEncTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblEncTitle.Location = New Point(12, 8)
        lblEncTitle.Name = "lblEncTitle"
        lblEncTitle.Size = New Size(52, 15)
        lblEncTitle.TabIndex = 0
        lblEncTitle.Text = "Encoder"
        ' 
        ' cboEncoder
        ' 
        cboEncoder.DropDownStyle = ComboBoxStyle.DropDownList
        cboEncoder.Font = New Font("Segoe UI", 9F)
        cboEncoder.Items.AddRange(New Object() {"Auto (Recommended)", "h264_nvenc - NVIDIA H.264", "hevc_nvenc - NVIDIA H.265", "h264_qsv - Intel QuickSync", "hevc_qsv - Intel QuickSync H.265", "h264_amf - AMD AMF H.264", "libx264 - CPU H.264", "libx265 - CPU H.265", "libsvtav1 - CPU SVT-AV1"})
        cboEncoder.Location = New Point(12, 30)
        cboEncoder.Name = "cboEncoder"
        cboEncoder.Size = New Size(325, 23)
        cboEncoder.TabIndex = 1
        ' 
        ' btnDetect
        ' 
        btnDetect.Cursor = Cursors.Hand
        btnDetect.Font = New Font("Segoe UI", 8F)
        btnDetect.Location = New Point(345, 30)
        btnDetect.Name = "btnDetect"
        btnDetect.Size = New Size(133, 23)
        btnDetect.TabIndex = 2
        btnDetect.Text = "Detect Encoders"
        ' 
        ' pnlRes
        ' 
        pnlRes.Controls.Add(chkNativeRes)
        pnlRes.Controls.Add(cboResolution)
        pnlRes.Location = New Point(15, 265)
        pnlRes.Name = "pnlRes"
        pnlRes.Size = New Size(490, 65)
        pnlRes.TabIndex = 5
        ' 
        ' chkNativeRes
        ' 
        chkNativeRes.AutoSize = True
        chkNativeRes.BackColor = Color.Transparent
        chkNativeRes.Checked = True
        chkNativeRes.CheckState = CheckState.Checked
        chkNativeRes.Font = New Font("Segoe UI", 9F)
        chkNativeRes.Location = New Point(12, 8)
        chkNativeRes.Name = "chkNativeRes"
        chkNativeRes.Size = New Size(141, 19)
        chkNativeRes.TabIndex = 0
        chkNativeRes.Text = "Use Native Resolution"
        chkNativeRes.UseVisualStyleBackColor = False
        ' 
        ' cboResolution
        ' 
        cboResolution.DropDownStyle = ComboBoxStyle.DropDownList
        cboResolution.Enabled = False
        cboResolution.Font = New Font("Segoe UI", 9F)
        cboResolution.Items.AddRange(New Object() {"1920x1080 (1080p)", "2560x1440 (1440p)", "3840x2160 (4K)", "1280x720 (720p)"})
        cboResolution.Location = New Point(12, 32)
        cboResolution.Name = "cboResolution"
        cboResolution.Size = New Size(466, 23)
        cboResolution.TabIndex = 1
        ' 
        ' pnlPerf
        ' 
        pnlPerf.Controls.Add(lblFPSTitle)
        pnlPerf.Controls.Add(nudFPS)
        pnlPerf.Controls.Add(lblBitTitle)
        pnlPerf.Controls.Add(nudBitrate)
        pnlPerf.Controls.Add(lblBitrateHint)
        pnlPerf.Location = New Point(15, 340)
        pnlPerf.Name = "pnlPerf"
        pnlPerf.Size = New Size(490, 70)
        pnlPerf.TabIndex = 6
        ' 
        ' lblFPSTitle
        ' 
        lblFPSTitle.AutoSize = True
        lblFPSTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblFPSTitle.Location = New Point(12, 8)
        lblFPSTitle.Name = "lblFPSTitle"
        lblFPSTitle.Size = New Size(27, 15)
        lblFPSTitle.TabIndex = 0
        lblFPSTitle.Text = "FPS"
        ' 
        ' nudFPS
        ' 
        nudFPS.BorderStyle = BorderStyle.FixedSingle
        nudFPS.Location = New Point(12, 30)
        nudFPS.Maximum = New Decimal(New Integer() {240, 0, 0, 0})
        nudFPS.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        nudFPS.Name = "nudFPS"
        nudFPS.Size = New Size(100, 23)
        nudFPS.TabIndex = 1
        nudFPS.Value = New Decimal(New Integer() {60, 0, 0, 0})
        ' 
        ' lblBitTitle
        ' 
        lblBitTitle.AutoSize = True
        lblBitTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblBitTitle.Location = New Point(150, 8)
        lblBitTitle.Name = "lblBitTitle"
        lblBitTitle.Size = New Size(87, 15)
        lblBitTitle.TabIndex = 2
        lblBitTitle.Text = "Bitrate (Mbps)"
        ' 
        ' nudBitrate
        ' 
        nudBitrate.BorderStyle = BorderStyle.FixedSingle
        nudBitrate.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        nudBitrate.Location = New Point(150, 30)
        nudBitrate.Maximum = New Decimal(New Integer() {200, 0, 0, 0})
        nudBitrate.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        nudBitrate.Name = "nudBitrate"
        nudBitrate.Size = New Size(120, 23)
        nudBitrate.TabIndex = 3
        nudBitrate.Value = New Decimal(New Integer() {50, 0, 0, 0})
        ' 
        ' lblBitrateHint
        ' 
        lblBitrateHint.AutoSize = True
        lblBitrateHint.Font = New Font("Segoe UI", 8F)
        lblBitrateHint.Location = New Point(278, 34)
        lblBitrateHint.Name = "lblBitrateHint"
        lblBitrateHint.Size = New Size(83, 13)
        lblBitrateHint.TabIndex = 4
        lblBitrateHint.Text = "(50 = 50 Mbps)"
        ' 
        ' pnlPreset
        ' 
        pnlPreset.Controls.Add(lblPresetTitle)
        pnlPreset.Controls.Add(lblPresetValue)
        pnlPreset.Controls.Add(lblNvencPreset)
        pnlPreset.Controls.Add(lblReplayTitle)
        pnlPreset.Controls.Add(nudReplayDuration)
        pnlPreset.Location = New Point(15, 420)
        pnlPreset.Name = "pnlPreset"
        pnlPreset.Size = New Size(490, 70)
        pnlPreset.TabIndex = 11
        ' 
        ' lblPresetTitle
        ' 
        lblPresetTitle.AutoSize = True
        lblPresetTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblPresetTitle.Location = New Point(12, 8)
        lblPresetTitle.Name = "lblPresetTitle"
        lblPresetTitle.Size = New Size(43, 15)
        lblPresetTitle.TabIndex = 0
        lblPresetTitle.Text = "Preset"
        ' 
        ' lblPresetValue
        ' 
        lblPresetValue.AutoSize = True
        lblPresetValue.Font = New Font("Segoe UI", 9F)
        lblPresetValue.Location = New Point(60, 8)
        lblPresetValue.Name = "lblPresetValue"
        lblPresetValue.Size = New Size(61, 15)
        lblPresetValue.TabIndex = 1
        lblPresetValue.Text = "Maximum"
        ' 
        ' lblNvencPreset
        ' 
        lblNvencPreset.AutoSize = True
        lblNvencPreset.Font = New Font("Segoe UI", 8F)
        lblNvencPreset.Location = New Point(200, 9)
        lblNvencPreset.Name = "lblNvencPreset"
        lblNvencPreset.Size = New Size(62, 13)
        lblNvencPreset.TabIndex = 2
        lblNvencPreset.Text = "NVENC: p7"
        ' 
        ' lblReplayTitle
        ' 
        lblReplayTitle.AutoSize = True
        lblReplayTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblReplayTitle.Location = New Point(12, 32)
        lblReplayTitle.Name = "lblReplayTitle"
        lblReplayTitle.Size = New Size(112, 15)
        lblReplayTitle.TabIndex = 3
        lblReplayTitle.Text = "Replay Duration (s)"
        ' 
        ' nudReplayDuration
        ' 
        nudReplayDuration.BorderStyle = BorderStyle.FixedSingle
        nudReplayDuration.Location = New Point(140, 30)
        nudReplayDuration.Maximum = New Decimal(New Integer() {1200, 0, 0, 0})
        nudReplayDuration.Minimum = New Decimal(New Integer() {15, 0, 0, 0})
        nudReplayDuration.Name = "nudReplayDuration"
        nudReplayDuration.Size = New Size(80, 23)
        nudReplayDuration.TabIndex = 4
        nudReplayDuration.Value = New Decimal(New Integer() {60, 0, 0, 0})
        ' 
        ' pnlAudio
        ' 
        pnlAudio.Controls.Add(lblAudioTitle)
        pnlAudio.Controls.Add(chkSysAudio)
        pnlAudio.Controls.Add(chkMic)
        pnlAudio.Controls.Add(lblSysVol)
        pnlAudio.Controls.Add(trkSysVol)
        pnlAudio.Controls.Add(lblMicVol)
        pnlAudio.Controls.Add(trkMicVol)
        pnlAudio.Controls.Add(lblMicDevice)
        pnlAudio.Location = New Point(15, 500)
        pnlAudio.Name = "pnlAudio"
        pnlAudio.Size = New Size(490, 105)
        pnlAudio.TabIndex = 12
        ' 
        ' lblAudioTitle
        ' 
        lblAudioTitle.AutoSize = True
        lblAudioTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblAudioTitle.Location = New Point(12, 8)
        lblAudioTitle.Name = "lblAudioTitle"
        lblAudioTitle.Size = New Size(39, 15)
        lblAudioTitle.TabIndex = 0
        lblAudioTitle.Text = "Audio"
        ' 
        ' chkSysAudio
        ' 
        chkSysAudio.AutoSize = True
        chkSysAudio.Font = New Font("Segoe UI", 9F)
        chkSysAudio.Location = New Point(12, 32)
        chkSysAudio.Name = "chkSysAudio"
        chkSysAudio.Size = New Size(99, 19)
        chkSysAudio.TabIndex = 1
        chkSysAudio.Text = "System Audio"
        ' 
        ' chkMic
        ' 
        chkMic.AutoSize = True
        chkMic.Font = New Font("Segoe UI", 9F)
        chkMic.Location = New Point(140, 32)
        chkMic.Name = "chkMic"
        chkMic.Size = New Size(91, 19)
        chkMic.TabIndex = 2
        chkMic.Text = "Microphone"
        ' 
        ' lblSysVol
        ' 
        lblSysVol.AutoSize = True
        lblSysVol.Font = New Font("Segoe UI", 8F)
        lblSysVol.Location = New Point(12, 58)
        lblSysVol.Name = "lblSysVol"
        lblSysVol.Size = New Size(75, 13)
        lblSysVol.TabIndex = 3
        lblSysVol.Text = "Sys Vol: 100%"
        ' 
        ' trkSysVol
        ' 
        trkSysVol.Location = New Point(95, 55)
        trkSysVol.Maximum = 100
        trkSysVol.Name = "trkSysVol"
        trkSysVol.Size = New Size(120, 45)
        trkSysVol.TabIndex = 4
        trkSysVol.Value = 100
        ' 
        ' lblMicVol
        ' 
        lblMicVol.AutoSize = True
        lblMicVol.Font = New Font("Segoe UI", 8F)
        lblMicVol.Location = New Point(240, 58)
        lblMicVol.Name = "lblMicVol"
        lblMicVol.Size = New Size(77, 13)
        lblMicVol.TabIndex = 5
        lblMicVol.Text = "Mic Vol: 100%"
        ' 
        ' trkMicVol
        ' 
        trkMicVol.Location = New Point(325, 55)
        trkMicVol.Maximum = 100
        trkMicVol.Name = "trkMicVol"
        trkMicVol.Size = New Size(120, 45)
        trkMicVol.TabIndex = 6
        trkMicVol.Value = 100
        ' 
        ' lblMicDevice
        ' 
        lblMicDevice.AutoSize = True
        lblMicDevice.Font = New Font("Segoe UI", 8F)
        lblMicDevice.Location = New Point(12, 85)
        lblMicDevice.Name = "lblMicDevice"
        lblMicDevice.Size = New Size(74, 13)
        lblMicDevice.TabIndex = 7
        lblMicDevice.Text = "Mic: (default)"
        ' 
        ' pnlOutput
        ' 
        pnlOutput.Controls.Add(lblOutTitle)
        pnlOutput.Controls.Add(txtOutputDir)
        pnlOutput.Controls.Add(btnBrowse)
        pnlOutput.Location = New Point(15, 420)
        pnlOutput.Name = "pnlOutput"
        pnlOutput.Size = New Size(490, 65)
        pnlOutput.TabIndex = 7
        ' 
        ' lblOutTitle
        ' 
        lblOutTitle.AutoSize = True
        lblOutTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblOutTitle.Location = New Point(12, 8)
        lblOutTitle.Name = "lblOutTitle"
        lblOutTitle.Size = New Size(85, 15)
        lblOutTitle.TabIndex = 0
        lblOutTitle.Text = "Output Folder"
        ' 
        ' txtOutputDir
        ' 
        txtOutputDir.BorderStyle = BorderStyle.FixedSingle
        txtOutputDir.Location = New Point(12, 30)
        txtOutputDir.Name = "txtOutputDir"
        txtOutputDir.ReadOnly = True
        txtOutputDir.Size = New Size(420, 23)
        txtOutputDir.TabIndex = 1
        ' 
        ' btnBrowse
        ' 
        btnBrowse.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnBrowse.Location = New Point(438, 30)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(40, 23)
        btnBrowse.TabIndex = 2
        btnBrowse.Text = "..."
        ' 
        ' pnlFFmpeg
        ' 
        pnlFFmpeg.Controls.Add(lblFfmpegTitle)
        pnlFFmpeg.Controls.Add(txtFFmpegPath)
        pnlFFmpeg.Controls.Add(btnFFmpegBrowse)
        pnlFFmpeg.Controls.Add(lblFFmpegStatus)
        pnlFFmpeg.Location = New Point(15, 495)
        pnlFFmpeg.Name = "pnlFFmpeg"
        pnlFFmpeg.Size = New Size(490, 87)
        pnlFFmpeg.TabIndex = 8
        ' 
        ' lblFfmpegTitle
        ' 
        lblFfmpegTitle.AutoSize = True
        lblFfmpegTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblFfmpegTitle.Location = New Point(12, 8)
        lblFfmpegTitle.Name = "lblFfmpegTitle"
        lblFfmpegTitle.Size = New Size(79, 15)
        lblFfmpegTitle.TabIndex = 0
        lblFfmpegTitle.Text = "FFmpeg Path"
        ' 
        ' txtFFmpegPath
        ' 
        txtFFmpegPath.BorderStyle = BorderStyle.FixedSingle
        txtFFmpegPath.Location = New Point(12, 30)
        txtFFmpegPath.Name = "txtFFmpegPath"
        txtFFmpegPath.Size = New Size(420, 23)
        txtFFmpegPath.TabIndex = 1
        ' 
        ' btnFFmpegBrowse
        ' 
        btnFFmpegBrowse.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnFFmpegBrowse.Location = New Point(438, 30)
        btnFFmpegBrowse.Name = "btnFFmpegBrowse"
        btnFFmpegBrowse.Size = New Size(40, 23)
        btnFFmpegBrowse.TabIndex = 2
        btnFFmpegBrowse.Text = "..."
        ' 
        ' lblFFmpegStatus
        ' 
        lblFFmpegStatus.AutoSize = True
        lblFFmpegStatus.Font = New Font("Segoe UI", 8F)
        lblFFmpegStatus.Location = New Point(12, 55)
        lblFFmpegStatus.Name = "lblFFmpegStatus"
        lblFFmpegStatus.Size = New Size(0, 13)
        lblFFmpegStatus.TabIndex = 3
        ' 
        ' pnlGitHub
        ' 
        pnlGitHub.Controls.Add(lblGitHubTitle)
        pnlGitHub.Controls.Add(lblGitHubUser)
        pnlGitHub.Controls.Add(lblGitHubStatus)
        pnlGitHub.Location = New Point(15, 615)
        pnlGitHub.Name = "pnlGitHub"
        pnlGitHub.Size = New Size(490, 50)
        pnlGitHub.TabIndex = 13
        ' 
        ' lblGitHubTitle
        ' 
        lblGitHubTitle.AutoSize = True
        lblGitHubTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblGitHubTitle.Location = New Point(12, 8)
        lblGitHubTitle.Name = "lblGitHubTitle"
        lblGitHubTitle.Size = New Size(50, 15)
        lblGitHubTitle.TabIndex = 0
        lblGitHubTitle.Text = "GitHub:"
        ' 
        ' lblGitHubUser
        ' 
        lblGitHubUser.AutoSize = True
        lblGitHubUser.Font = New Font("Segoe UI", 9F)
        lblGitHubUser.Location = New Point(70, 8)
        lblGitHubUser.Name = "lblGitHubUser"
        lblGitHubUser.Size = New Size(72, 15)
        lblGitHubUser.TabIndex = 1
        lblGitHubUser.Text = "(not loaded)"
        ' 
        ' lblGitHubStatus
        ' 
        lblGitHubStatus.AutoSize = True
        lblGitHubStatus.Font = New Font("Segoe UI", 8F)
        lblGitHubStatus.Location = New Point(12, 28)
        lblGitHubStatus.Name = "lblGitHubStatus"
        lblGitHubStatus.Size = New Size(116, 13)
        lblGitHubStatus.TabIndex = 2
        lblGitHubStatus.Text = "Status: not logged in"
        ' 
        ' pnlHub
        ' 
        pnlHub.Controls.Add(lblHubTitle)
        pnlHub.Controls.Add(lblHubStatus)
        pnlHub.Controls.Add(lblHubClients)
        pnlHub.Location = New Point(15, 675)
        pnlHub.Name = "pnlHub"
        pnlHub.Size = New Size(490, 50)
        pnlHub.TabIndex = 14
        ' 
        ' lblHubTitle
        ' 
        lblHubTitle.AutoSize = True
        lblHubTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblHubTitle.Location = New Point(12, 8)
        lblHubTitle.Name = "lblHubTitle"
        lblHubTitle.Size = New Size(33, 15)
        lblHubTitle.TabIndex = 0
        lblHubTitle.Text = "Hub:"
        ' 
        ' lblHubStatus
        ' 
        lblHubStatus.AutoSize = True
        lblHubStatus.Font = New Font("Segoe UI", 9F)
        lblHubStatus.Location = New Point(50, 8)
        lblHubStatus.Name = "lblHubStatus"
        lblHubStatus.Size = New Size(78, 15)
        lblHubStatus.TabIndex = 1
        lblHubStatus.Text = "disconnected"
        ' 
        ' lblHubClients
        ' 
        lblHubClients.AutoSize = True
        lblHubClients.Font = New Font("Segoe UI", 8F)
        lblHubClients.Location = New Point(12, 28)
        lblHubClients.Name = "lblHubClients"
        lblHubClients.Size = New Size(54, 13)
        lblHubClients.TabIndex = 2
        lblHubClients.Text = "Clients: 0"
        ' 
        ' lblConfigSource
        ' 
        lblConfigSource.AutoSize = True
        lblConfigSource.Font = New Font("Segoe UI", 7F)
        lblConfigSource.Location = New Point(15, 730)
        lblConfigSource.Name = "lblConfigSource"
        lblConfigSource.Size = New Size(93, 12)
        lblConfigSource.TabIndex = 15
        lblConfigSource.Text = "Config: (searching...)"
        ' 
        ' btnRecord
        ' 
        btnRecord.Cursor = Cursors.Hand
        btnRecord.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        btnRecord.ForeColor = Color.White
        btnRecord.Location = New Point(15, 588)
        btnRecord.Name = "btnRecord"
        btnRecord.Size = New Size(240, 32)
        btnRecord.TabIndex = 9
        btnRecord.Text = "Record"
        ' 
        ' btnStop
        ' 
        btnStop.Cursor = Cursors.Hand
        btnStop.Enabled = False
        btnStop.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        btnStop.ForeColor = Color.White
        btnStop.Location = New Point(265, 588)
        btnStop.Name = "btnStop"
        btnStop.Size = New Size(240, 32)
        btnStop.TabIndex = 10
        btnStop.Text = "Stop"
        ' 
        ' lblHotkeys
        ' 
        lblHotkeys.AutoSize = True
        lblHotkeys.Font = New Font("Segoe UI", 8F)
        lblHotkeys.Location = New Point(15, 638)
        lblHotkeys.Name = "lblHotkeys"
        lblHotkeys.Size = New Size(273, 13)
        lblHotkeys.TabIndex = 11
        lblHotkeys.Text = "Hotkeys: Start=Ctrl+Shift+F9 | Stop=Ctrl+Shift+F10"
        ' 
        ' tmrRecording
        ' 
        tmrRecording.Interval = 1000
        ' 
        ' tmrRefresh
        ' 
        tmrRefresh.Interval = 2000
        ' 
        ' UI_Engine
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(873, 778)
        Controls.Add(lblTitle)
        Controls.Add(lblStatus)
        Controls.Add(lblTimer)
        Controls.Add(pnlCapture)
        Controls.Add(pnlEncoder)
        Controls.Add(pnlRes)
        Controls.Add(pnlPerf)
        Controls.Add(pnlPreset)
        Controls.Add(pnlAudio)
        Controls.Add(pnlOutput)
        Controls.Add(pnlFFmpeg)
        Controls.Add(pnlGitHub)
        Controls.Add(pnlHub)
        Controls.Add(lblConfigSource)
        Controls.Add(btnRecord)
        Controls.Add(btnStop)
        Controls.Add(lblHotkeys)
        Font = New Font("Segoe UI", 9F)
        FormBorderStyle = FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        Name = "UI_Engine"
        StartPosition = FormStartPosition.CenterScreen
        Text = "NVIDIA Capture"
        pnlCapture.ResumeLayout(False)
        pnlCapture.PerformLayout()
        pnlEncoder.ResumeLayout(False)
        pnlEncoder.PerformLayout()
        pnlRes.ResumeLayout(False)
        pnlRes.PerformLayout()
        pnlPerf.ResumeLayout(False)
        pnlPerf.PerformLayout()
        CType(nudFPS, ComponentModel.ISupportInitialize).EndInit()
        CType(nudBitrate, ComponentModel.ISupportInitialize).EndInit()
        pnlPreset.ResumeLayout(False)
        pnlPreset.PerformLayout()
        CType(nudReplayDuration, ComponentModel.ISupportInitialize).EndInit()
        pnlAudio.ResumeLayout(False)
        pnlAudio.PerformLayout()
        CType(trkSysVol, ComponentModel.ISupportInitialize).EndInit()
        CType(trkMicVol, ComponentModel.ISupportInitialize).EndInit()
        pnlOutput.ResumeLayout(False)
        pnlOutput.PerformLayout()
        pnlFFmpeg.ResumeLayout(False)
        pnlFFmpeg.PerformLayout()
        pnlGitHub.ResumeLayout(False)
        pnlGitHub.PerformLayout()
        pnlHub.ResumeLayout(False)
        pnlHub.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    ' ── Control Declarations ──

    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblTimer As System.Windows.Forms.Label
    Friend WithEvents cboCaptureMethod As System.Windows.Forms.ComboBox
    Friend WithEvents cboEncoder As System.Windows.Forms.ComboBox
    Friend WithEvents chkNativeRes As System.Windows.Forms.CheckBox
    Friend WithEvents cboResolution As System.Windows.Forms.ComboBox
    Friend WithEvents nudFPS As System.Windows.Forms.NumericUpDown
    Friend WithEvents nudBitrate As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblBitrateHint As System.Windows.Forms.Label
    Friend WithEvents txtOutputDir As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents txtFFmpegPath As System.Windows.Forms.TextBox
    Friend WithEvents btnFFmpegBrowse As System.Windows.Forms.Button
    Friend WithEvents lblFFmpegStatus As System.Windows.Forms.Label
    Friend WithEvents btnRecord As System.Windows.Forms.Button
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents btnDetect As System.Windows.Forms.Button
    Friend WithEvents lblHotkeys As System.Windows.Forms.Label
    Friend WithEvents tmrRecording As System.Windows.Forms.Timer
    Friend WithEvents lblTitle As Label
    Friend WithEvents pnlCapture As Panel
    Friend WithEvents lblCapTitle As Label
    Friend WithEvents pnlEncoder As Panel
    Friend WithEvents lblEncTitle As Label
    Friend WithEvents pnlRes As Panel
    Friend WithEvents pnlPerf As Panel
    Friend WithEvents lblFPSTitle As Label
    Friend WithEvents lblBitTitle As Label
    Friend WithEvents pnlOutput As Panel
    Friend WithEvents lblOutTitle As Label
    Friend WithEvents pnlFFmpeg As Panel
    Friend WithEvents lblFfmpegTitle As Label

    ' ✅ P2: new controls for full Overlay config display
    Friend WithEvents pnlPreset As Panel
    Friend WithEvents lblPresetTitle As Label
    Friend WithEvents lblPresetValue As Label
    Friend WithEvents lblNvencPreset As Label
    Friend WithEvents lblReplayTitle As Label
    Friend WithEvents nudReplayDuration As NumericUpDown

    Friend WithEvents pnlAudio As Panel
    Friend WithEvents lblAudioTitle As Label
    Friend WithEvents chkSysAudio As CheckBox
    Friend WithEvents chkMic As CheckBox
    Friend WithEvents lblSysVol As Label
    Friend WithEvents trkSysVol As TrackBar
    Friend WithEvents lblMicVol As Label
    Friend WithEvents trkMicVol As TrackBar
    Friend WithEvents lblMicDevice As Label

    Friend WithEvents pnlGitHub As Panel
    Friend WithEvents lblGitHubTitle As Label
    Friend WithEvents lblGitHubUser As Label
    Friend WithEvents lblGitHubStatus As Label

    Friend WithEvents pnlHub As Panel
    Friend WithEvents lblHubTitle As Label
    Friend WithEvents lblHubStatus As Label
    Friend WithEvents lblHubClients As Label

    Friend WithEvents lblConfigSource As Label
    Friend WithEvents tmrRefresh As System.Windows.Forms.Timer

End Class
