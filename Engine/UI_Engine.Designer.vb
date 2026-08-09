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
        components = New System.ComponentModel.Container()
        SuspendLayout()

        ' ── Form ──
        AutoScaleDimensions = New Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New Drawing.Size(520, 680)
        Font = New Drawing.Font("Segoe UI", 9F)
        BackColor = _bgDark
        ForeColor = _fgText
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Name = "UI_Engine"
        Text = "ShadowPlay Engine"

        ' ── Title ──
        Dim lblTitle As New System.Windows.Forms.Label()
        lblTitle.Text = "SHADOWPLAY ENGINE"
        lblTitle.Font = New Drawing.Font("Segoe UI", 16F, Drawing.FontStyle.Bold)
        lblTitle.ForeColor = _accentGreen
        lblTitle.Location = New Drawing.Point(20, 15)
        lblTitle.AutoSize = True

        ' ── Status ──
        lblStatus = New System.Windows.Forms.Label()
        lblStatus.Text = "Idle"
        lblStatus.Font = New Drawing.Font("Segoe UI", 10F)
        lblStatus.ForeColor = _fgDim
        lblStatus.Location = New Drawing.Point(20, 48)
        lblStatus.AutoSize = True

        ' ── Timer ──
        lblTimer = New System.Windows.Forms.Label()
        lblTimer.Text = "00:00:00"
        lblTimer.Font = New Drawing.Font("Consolas", 20F, Drawing.FontStyle.Bold)
        lblTimer.ForeColor = _accentGreen
        lblTimer.Location = New Drawing.Point(20, 70)
        lblTimer.AutoSize = True

        ' ── Panel: Capture Method ──
        Dim pnlCapture As New System.Windows.Forms.Panel()
        pnlCapture.Location = New Drawing.Point(15, 105)
        pnlCapture.Size = New Drawing.Size(490, 70)
        pnlCapture.BackColor = _bgPanel

        Dim lblCapTitle As New System.Windows.Forms.Label()
        lblCapTitle.Text = "Capture Method"
        lblCapTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblCapTitle.ForeColor = _fgText
        lblCapTitle.Location = New Drawing.Point(12, 8)
        lblCapTitle.AutoSize = True

        cboCaptureMethod = New System.Windows.Forms.ComboBox()
        cboCaptureMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboCaptureMethod.Location = New Drawing.Point(12, 30)
        cboCaptureMethod.Size = New Drawing.Size(466, 23)
        cboCaptureMethod.BackColor = _bgInput
        cboCaptureMethod.ForeColor = _fgText
        cboCaptureMethod.Font = New Drawing.Font("Segoe UI", 9F)
        cboCaptureMethod.Items.AddRange(New Object() {
            "ddagrab - Desktop Duplication (DXGI)",
            "gdigrab - GDI Screen Capture",
            "gfxcapture - DXGI Desktop Dup (NV)"
        })
        cboCaptureMethod.SelectedIndex = 0

        pnlCapture.Controls.Add(lblCapTitle)
        pnlCapture.Controls.Add(cboCaptureMethod)

        ' ── Panel: Encoder ──
        Dim pnlEncoder As New System.Windows.Forms.Panel()
        pnlEncoder.Location = New Drawing.Point(15, 185)
        pnlEncoder.Size = New Drawing.Size(490, 70)
        pnlEncoder.BackColor = _bgPanel

        Dim lblEncTitle As New System.Windows.Forms.Label()
        lblEncTitle.Text = "Encoder"
        lblEncTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblEncTitle.ForeColor = _fgText
        lblEncTitle.Location = New Drawing.Point(12, 8)
        lblEncTitle.AutoSize = True

        cboEncoder = New System.Windows.Forms.ComboBox()
        cboEncoder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboEncoder.Location = New Drawing.Point(12, 30)
        cboEncoder.Size = New Drawing.Size(325, 23)
        cboEncoder.BackColor = _bgInput
        cboEncoder.ForeColor = _fgText
        cboEncoder.Font = New Drawing.Font("Segoe UI", 9F)
        cboEncoder.Items.AddRange(New Object() {
            "Auto (Recommended)",
            "h264_nvenc - NVIDIA H.264",
            "hevc_nvenc - NVIDIA H.265",
            "h264_qsv - Intel QuickSync",
            "hevc_qsv - Intel QuickSync H.265",
            "h264_amf - AMD AMF H.264",
            "libx264 - CPU H.264",
            "libx265 - CPU H.265",
            "libsvtav1 - CPU SVT-AV1"
        })
        cboEncoder.SelectedIndex = 0

        btnDetect = New System.Windows.Forms.Button()
        btnDetect.Text = "Detect Encoders"
        btnDetect.Font = New Drawing.Font("Segoe UI", 8F)
        btnDetect.ForeColor = _fgText
        btnDetect.BackColor = _bgInput
        btnDetect.Location = New Drawing.Point(345, 30)
        btnDetect.Size = New Drawing.Size(133, 23)
        btnDetect.Cursor = System.Windows.Forms.Cursors.Hand

        pnlEncoder.Controls.Add(lblEncTitle)
        pnlEncoder.Controls.Add(cboEncoder)
        pnlEncoder.Controls.Add(btnDetect)

        ' ── Panel: Resolution ──
        Dim pnlRes As New System.Windows.Forms.Panel()
        pnlRes.Location = New Drawing.Point(15, 265)
        pnlRes.Size = New Drawing.Size(490, 65)
        pnlRes.BackColor = _bgPanel

        chkNativeRes = New System.Windows.Forms.CheckBox()
        chkNativeRes.Text = "Use Native Resolution"
        chkNativeRes.Font = New Drawing.Font("Segoe UI", 9F)
        chkNativeRes.ForeColor = _fgText
        chkNativeRes.BackColor = Drawing.Color.Transparent
        chkNativeRes.Location = New Drawing.Point(12, 8)
        chkNativeRes.AutoSize = True
        chkNativeRes.Checked = True

        cboResolution = New System.Windows.Forms.ComboBox()
        cboResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboResolution.Location = New Drawing.Point(12, 32)
        cboResolution.Size = New Drawing.Size(466, 23)
        cboResolution.BackColor = _bgInput
        cboResolution.ForeColor = _fgText
        cboResolution.Font = New Drawing.Font("Segoe UI", 9F)
        cboResolution.Enabled = False
        cboResolution.Items.AddRange(New Object() {
            "1920x1080 (1080p)",
            "2560x1440 (1440p)",
            "3840x2160 (4K)",
            "1280x720 (720p)"
        })

        pnlRes.Controls.Add(chkNativeRes)
        pnlRes.Controls.Add(cboResolution)

        ' ── Panel: FPS & Bitrate ──
        Dim pnlPerf As New System.Windows.Forms.Panel()
        pnlPerf.Location = New Drawing.Point(15, 340)
        pnlPerf.Size = New Drawing.Size(490, 70)
        pnlPerf.BackColor = _bgPanel

        Dim lblFPSTitle As New System.Windows.Forms.Label()
        lblFPSTitle.Text = "FPS"
        lblFPSTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblFPSTitle.ForeColor = _fgText
        lblFPSTitle.Location = New Drawing.Point(12, 8)
        lblFPSTitle.AutoSize = True

        nudFPS = New System.Windows.Forms.NumericUpDown()
        nudFPS.Location = New Drawing.Point(12, 30)
        nudFPS.Size = New Drawing.Size(100, 23)
        nudFPS.Minimum = New Decimal(1)
        nudFPS.Maximum = New Decimal(240)
        nudFPS.Value = New Decimal(60)
        nudFPS.BackColor = _bgInput
        nudFPS.ForeColor = _fgText
        nudFPS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle

        Dim lblBitTitle As New System.Windows.Forms.Label()
        lblBitTitle.Text = "Bitrate (Mbps)"
        lblBitTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblBitTitle.ForeColor = _fgText
        lblBitTitle.Location = New Drawing.Point(150, 8)
        lblBitTitle.AutoSize = True

        nudBitrate = New System.Windows.Forms.NumericUpDown()
        nudBitrate.Location = New Drawing.Point(150, 30)
        nudBitrate.Size = New Drawing.Size(120, 23)
        nudBitrate.Minimum = New Decimal(1)
        nudBitrate.Maximum = New Decimal(200)
        nudBitrate.Value = New Decimal(50)
        nudBitrate.Increment = New Decimal(5)
        nudBitrate.BackColor = _bgInput
        nudBitrate.ForeColor = _fgText
        nudBitrate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle

        lblBitrateHint = New System.Windows.Forms.Label()
        lblBitrateHint.Text = "(50 = 50 Mbps)"
        lblBitrateHint.Font = New Drawing.Font("Segoe UI", 8F)
        lblBitrateHint.ForeColor = _fgDim
        lblBitrateHint.Location = New Drawing.Point(278, 34)
        lblBitrateHint.AutoSize = True

        pnlPerf.Controls.Add(lblFPSTitle)
        pnlPerf.Controls.Add(nudFPS)
        pnlPerf.Controls.Add(lblBitTitle)
        pnlPerf.Controls.Add(nudBitrate)
        pnlPerf.Controls.Add(lblBitrateHint)

        ' ── Panel: Output ──
        Dim pnlOutput As New System.Windows.Forms.Panel()
        pnlOutput.Location = New Drawing.Point(15, 420)
        pnlOutput.Size = New Drawing.Size(490, 65)
        pnlOutput.BackColor = _bgPanel

        Dim lblOutTitle As New System.Windows.Forms.Label()
        lblOutTitle.Text = "Output Folder"
        lblOutTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblOutTitle.ForeColor = _fgText
        lblOutTitle.Location = New Drawing.Point(12, 8)
        lblOutTitle.AutoSize = True

        txtOutputDir = New System.Windows.Forms.TextBox()
        txtOutputDir.Location = New Drawing.Point(12, 30)
        txtOutputDir.Size = New Drawing.Size(420, 23)
        txtOutputDir.BackColor = _bgInput
        txtOutputDir.ForeColor = _fgText
        txtOutputDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        txtOutputDir.ReadOnly = True

        btnBrowse = New System.Windows.Forms.Button()
        btnBrowse.Text = "..."
        btnBrowse.Location = New Drawing.Point(438, 30)
        btnBrowse.Size = New Drawing.Size(40, 23)
        btnBrowse.BackColor = _bgInput
        btnBrowse.ForeColor = _fgText
        btnBrowse.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)

        pnlOutput.Controls.Add(lblOutTitle)
        pnlOutput.Controls.Add(txtOutputDir)
        pnlOutput.Controls.Add(btnBrowse)

        ' ── Panel: FFmpeg Path ──
        Dim pnlFFmpeg As New System.Windows.Forms.Panel()
        pnlFFmpeg.Location = New Drawing.Point(15, 495)
        pnlFFmpeg.Size = New Drawing.Size(490, 65)
        pnlFFmpeg.BackColor = _bgPanel

        Dim lblFfmpegTitle As New System.Windows.Forms.Label()
        lblFfmpegTitle.Text = "FFmpeg Path"
        lblFfmpegTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblFfmpegTitle.ForeColor = _fgText
        lblFfmpegTitle.Location = New Drawing.Point(12, 8)
        lblFfmpegTitle.AutoSize = True

        txtFFmpegPath = New System.Windows.Forms.TextBox()
        txtFFmpegPath.Location = New Drawing.Point(12, 30)
        txtFFmpegPath.Size = New Drawing.Size(420, 23)
        txtFFmpegPath.BackColor = _bgInput
        txtFFmpegPath.ForeColor = _fgText
        txtFFmpegPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle

        btnFFmpegBrowse = New System.Windows.Forms.Button()
        btnFFmpegBrowse.Text = "..."
        btnFFmpegBrowse.Location = New Drawing.Point(438, 30)
        btnFFmpegBrowse.Size = New Drawing.Size(40, 23)
        btnFFmpegBrowse.BackColor = _bgInput
        btnFFmpegBrowse.ForeColor = _fgText
        btnFFmpegBrowse.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)

        lblFFmpegStatus = New System.Windows.Forms.Label()
        lblFFmpegStatus.Text = ""
        lblFFmpegStatus.Font = New Drawing.Font("Segoe UI", 8F)
        lblFFmpegStatus.ForeColor = _accentRed
        lblFFmpegStatus.Location = New Drawing.Point(12, 55)
        lblFFmpegStatus.AutoSize = True

        pnlFFmpeg.Controls.Add(lblFfmpegTitle)
        pnlFFmpeg.Controls.Add(txtFFmpegPath)
        pnlFFmpeg.Controls.Add(btnFFmpegBrowse)
        pnlFFmpeg.Controls.Add(lblFFmpegStatus)

        ' ── Buttons ──
        btnRecord = New System.Windows.Forms.Button()
        btnRecord.Text = "Record"
        btnRecord.Font = New Drawing.Font("Segoe UI", 11F, Drawing.FontStyle.Bold)
        btnRecord.ForeColor = Drawing.Color.White
        btnRecord.BackColor = _bgButton
        btnRecord.Location = New Drawing.Point(15, 575)
        btnRecord.Size = New Drawing.Size(240, 45)
        btnRecord.Cursor = System.Windows.Forms.Cursors.Hand

        btnStop = New System.Windows.Forms.Button()
        btnStop.Text = "Stop"
        btnStop.Font = New Drawing.Font("Segoe UI", 11F, Drawing.FontStyle.Bold)
        btnStop.ForeColor = Drawing.Color.White
        btnStop.BackColor = _bgButtonStop
        btnStop.Location = New Drawing.Point(265, 575)
        btnStop.Size = New Drawing.Size(240, 45)
        btnStop.Cursor = System.Windows.Forms.Cursors.Hand
        btnStop.Enabled = False

        ' ── Hotkey Label ──
        lblHotkeys = New System.Windows.Forms.Label()
        lblHotkeys.Text = "Hotkeys: Start=Ctrl+Shift+F9 | Stop=Ctrl+Shift+F10"
        lblHotkeys.Font = New Drawing.Font("Segoe UI", 8F)
        lblHotkeys.ForeColor = _fgDim
        lblHotkeys.Location = New Drawing.Point(15, 638)
        lblHotkeys.AutoSize = True

        ' ── Timer ──
        tmrRecording = New System.Windows.Forms.Timer()
        tmrRecording.Interval = 1000

        ' ── Add All Controls ──
        Controls.Add(lblTitle)
        Controls.Add(lblStatus)
        Controls.Add(lblTimer)
        Controls.Add(pnlCapture)
        Controls.Add(pnlEncoder)
        Controls.Add(pnlRes)
        Controls.Add(pnlPerf)
        Controls.Add(pnlOutput)
        Controls.Add(pnlFFmpeg)
        Controls.Add(btnRecord)
        Controls.Add(btnStop)
        Controls.Add(lblHotkeys)

        ResumeLayout(False)
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

End Class
