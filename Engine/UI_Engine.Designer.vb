
' UI_Engine.Designer.vb
' ShadowPlay Engine - GFE-Style Settings Form Layout
' Dark NVIDIA theme - 580x820

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

    ' ── Color Scheme (accessible from partial class) ──

    Private _bgDark As Drawing.Color = Drawing.Color.FromArgb(32, 32, 36)
    Private _bgPanel As Drawing.Color = Drawing.Color.FromArgb(44, 44, 48)
    Private _bgInput As Drawing.Color = Drawing.Color.FromArgb(55, 55, 60)
    Private _bgButton As Drawing.Color = Drawing.Color.FromArgb(118, 185, 0)
    Private _bgButtonStop As Drawing.Color = Drawing.Color.FromArgb(200, 50, 50)
    Private _bgButtonQual As Drawing.Color = Drawing.Color.FromArgb(55, 55, 60)
    Private _bgButtonQualActive As Drawing.Color = Drawing.Color.FromArgb(118, 185, 0)
    Private _fgText As Drawing.Color = Drawing.Color.FromArgb(230, 230, 230)
    Private _fgDim As Drawing.Color = Drawing.Color.FromArgb(160, 160, 160)
    Private _accentGreen As Drawing.Color = Drawing.Color.FromArgb(118, 185, 0)
    Private _accentRed As Drawing.Color = Drawing.Color.FromArgb(200, 50, 50)

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Me.SuspendLayout()

        ' ══════════════════════════════════════════════════════════
        ' FORM
        ' ══════════════════════════════════════════════════════════
        Me.AutoScaleDimensions = New Drawing.SizeF(7F, 15F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New Drawing.Size(580, 820)
        Me.Font = New Drawing.Font("Segoe UI", 9F)
        Me.BackColor = _bgDark
        Me.ForeColor = _fgText
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Name = "UI_Engine"
        Me.Text = "ShadowPlay Engine"

        ' ══════════════════════════════════════════════════════════
        ' TOP SECTION (fixed, not in a panel)
        ' ══════════════════════════════════════════════════════════

        ' ── Title ──
        lblTitle = New System.Windows.Forms.Label()
        lblTitle.Text = "SHADOWPLAY ENGINE"
        lblTitle.Font = New Drawing.Font("Segoe UI", 16F, Drawing.FontStyle.Bold)
        lblTitle.ForeColor = _accentGreen
        lblTitle.Location = New Drawing.Point(20, 12)
        lblTitle.AutoSize = True

        ' ── Status ──
        lblStatus = New System.Windows.Forms.Label()
        lblStatus.Text = "Idle"
        lblStatus.Font = New Drawing.Font("Segoe UI", 9F)
        lblStatus.ForeColor = _fgDim
        lblStatus.Location = New Drawing.Point(20, 42)
        lblStatus.AutoSize = True

        ' ── Timer ──
        lblTimer = New System.Windows.Forms.Label()
        lblTimer.Text = "00:00:00"
        lblTimer.Font = New Drawing.Font("Consolas", 20F, Drawing.FontStyle.Bold)
        lblTimer.ForeColor = _accentGreen
        lblTimer.Location = New Drawing.Point(20, 64)
        lblTimer.AutoSize = True

        ' ══════════════════════════════════════════════════════════
        ' PANEL 1: QUALITY  (y=100, h=58)
        ' ══════════════════════════════════════════════════════════

        pnlQuality = New System.Windows.Forms.Panel()
        pnlQuality.Location = New Drawing.Point(15, 100)
        pnlQuality.Size = New Drawing.Size(550, 58)
        pnlQuality.BackColor = _bgPanel

        Dim lblQualityTitle As New System.Windows.Forms.Label()
        lblQualityTitle.Text = "Quality"
        lblQualityTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblQualityTitle.ForeColor = _fgText
        lblQualityTitle.Location = New Drawing.Point(12, 6)
        lblQualityTitle.AutoSize = True

        ' Low
        btnQualityLow = New System.Windows.Forms.Button()
        btnQualityLow.Text = "Low"
        btnQualityLow.Tag = "low"
        btnQualityLow.Font = New Drawing.Font("Segoe UI", 9F)
        btnQualityLow.ForeColor = _fgText
        btnQualityLow.BackColor = _bgButtonQual
        btnQualityLow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnQualityLow.FlatAppearance.BorderSize = 0
        btnQualityLow.Location = New Drawing.Point(12, 28)
        btnQualityLow.Size = New Drawing.Size(125, 26)
        btnQualityLow.Cursor = System.Windows.Forms.Cursors.Hand

        ' Medium
        btnQualityMedium = New System.Windows.Forms.Button()
        btnQualityMedium.Text = "Medium"
        btnQualityMedium.Tag = "medium"
        btnQualityMedium.Font = New Drawing.Font("Segoe UI", 9F)
        btnQualityMedium.ForeColor = _fgText
        btnQualityMedium.BackColor = _bgButtonQual
        btnQualityMedium.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnQualityMedium.FlatAppearance.BorderSize = 0
        btnQualityMedium.Location = New Drawing.Point(143, 28)
        btnQualityMedium.Size = New Drawing.Size(125, 26)
        btnQualityMedium.Cursor = System.Windows.Forms.Cursors.Hand

        ' High (default selected)
        btnQualityHigh = New System.Windows.Forms.Button()
        btnQualityHigh.Text = "High"
        btnQualityHigh.Tag = "high"
        btnQualityHigh.Font = New Drawing.Font("Segoe UI", 9F)
        btnQualityHigh.ForeColor = Drawing.Color.White
        btnQualityHigh.BackColor = _bgButtonQualActive
        btnQualityHigh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnQualityHigh.FlatAppearance.BorderSize = 0
        btnQualityHigh.Location = New Drawing.Point(274, 28)
        btnQualityHigh.Size = New Drawing.Size(125, 26)
        btnQualityHigh.Cursor = System.Windows.Forms.Cursors.Hand

        ' Custom
        btnQualityCustom = New System.Windows.Forms.Button()
        btnQualityCustom.Text = "Custom"
        btnQualityCustom.Tag = "custom"
        btnQualityCustom.Font = New Drawing.Font("Segoe UI", 9F)
        btnQualityCustom.ForeColor = _fgText
        btnQualityCustom.BackColor = _bgButtonQual
        btnQualityCustom.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnQualityCustom.FlatAppearance.BorderSize = 0
        btnQualityCustom.Location = New Drawing.Point(405, 28)
        btnQualityCustom.Size = New Drawing.Size(133, 26)
        btnQualityCustom.Cursor = System.Windows.Forms.Cursors.Hand

        pnlQuality.Controls.Add(lblQualityTitle)
        pnlQuality.Controls.Add(btnQualityLow)
        pnlQuality.Controls.Add(btnQualityMedium)
        pnlQuality.Controls.Add(btnQualityHigh)
        pnlQuality.Controls.Add(btnQualityCustom)

        ' ══════════════════════════════════════════════════════════
        ' PANEL 2: SETTINGS  (y=166, h=195)
        ' ══════════════════════════════════════════════════════════

        pnlSettings = New System.Windows.Forms.Panel()
        pnlSettings.Location = New Drawing.Point(15, 166)
        pnlSettings.Size = New Drawing.Size(550, 195)
        pnlSettings.BackColor = _bgPanel

        Dim lblSettingsTitle As New System.Windows.Forms.Label()
        lblSettingsTitle.Text = "Settings"
        lblSettingsTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblSettingsTitle.ForeColor = _fgText
        lblSettingsTitle.Location = New Drawing.Point(12, 6)
        lblSettingsTitle.AutoSize = True

        ' ── Row 1: Frame Rate, Preset, Resolution ──

        lblFPS = New System.Windows.Forms.Label()
        lblFPS.Text = "Frame Rate"
        lblFPS.Font = New Drawing.Font("Segoe UI", 9F)
        lblFPS.ForeColor = _fgDim
        lblFPS.Location = New Drawing.Point(12, 30)
        lblFPS.AutoSize = True

        cboFPS = New System.Windows.Forms.ComboBox()
        cboFPS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboFPS.Location = New Drawing.Point(12, 48)
        cboFPS.Size = New Drawing.Size(75, 23)
        cboFPS.BackColor = _bgInput
        cboFPS.ForeColor = _fgText
        cboFPS.Font = New Drawing.Font("Segoe UI", 9F)
        cboFPS.Items.AddRange(New Object() {"30", "60", "120", "144"})

        lblPreset = New System.Windows.Forms.Label()
        lblPreset.Text = "Preset"
        lblPreset.Font = New Drawing.Font("Segoe UI", 9F)
        lblPreset.ForeColor = _fgDim
        lblPreset.Location = New Drawing.Point(97, 30)
        lblPreset.AutoSize = True

        cboPreset = New System.Windows.Forms.ComboBox()
        cboPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboPreset.Location = New Drawing.Point(97, 48)
        cboPreset.Size = New Drawing.Size(62, 23)
        cboPreset.BackColor = _bgInput
        cboPreset.ForeColor = _fgText
        cboPreset.Font = New Drawing.Font("Segoe UI", 9F)
        cboPreset.Items.AddRange(New Object() {"P1", "P2", "P3", "P4", "P5", "P6", "P7"})

        lblResolution = New System.Windows.Forms.Label()
        lblResolution.Text = "Resolution"
        lblResolution.Font = New Drawing.Font("Segoe UI", 9F)
        lblResolution.ForeColor = _fgDim
        lblResolution.Location = New Drawing.Point(170, 30)
        lblResolution.AutoSize = True

        cboResolution = New System.Windows.Forms.ComboBox()
        cboResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboResolution.Location = New Drawing.Point(170, 48)
        cboResolution.Size = New Drawing.Size(150, 23)
        cboResolution.BackColor = _bgInput
        cboResolution.ForeColor = _fgText
        cboResolution.Font = New Drawing.Font("Segoe UI", 9F)
        cboResolution.Items.AddRange(New Object() {"Native", "1920x1080", "1280x720", "854x480"})

        ' ── Row 2: Bitrate ──

        lblBitrate = New System.Windows.Forms.Label()
        lblBitrate.Text = "Bitrate"
        lblBitrate.Font = New Drawing.Font("Segoe UI", 9F)
        lblBitrate.ForeColor = _fgDim
        lblBitrate.Location = New Drawing.Point(12, 80)
        lblBitrate.AutoSize = True

        trkBitrate = New System.Windows.Forms.TrackBar()
        trkBitrate.Location = New Drawing.Point(12, 100)
        trkBitrate.Size = New Drawing.Size(330, 42)
        trkBitrate.Minimum = 5000
        trkBitrate.Maximum = 150000
        trkBitrate.Value = 50000
        trkBitrate.SmallChange = 1000
        trkBitrate.LargeChange = 5000
        trkBitrate.TickFrequency = 10000
        trkBitrate.BackColor = _bgPanel

        lblBitrateValue = New System.Windows.Forms.Label()
        lblBitrateValue.Text = "50.0 Mbps"
        lblBitrateValue.Font = New Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Bold)
        lblBitrateValue.ForeColor = _accentGreen
        lblBitrateValue.Location = New Drawing.Point(350, 108)
        lblBitrateValue.AutoSize = True

        lblBitrateRange = New System.Windows.Forms.Label()
        lblBitrateRange.Text = "Range: 5 - 150 Mbps (Recommended: 50 - 80 Mbps)"
        lblBitrateRange.Font = New Drawing.Font("Segoe UI", 8F)
        lblBitrateRange.ForeColor = _fgDim
        lblBitrateRange.Location = New Drawing.Point(12, 144)
        lblBitrateRange.AutoSize = True

        lblStorageEstimate = New System.Windows.Forms.Label()
        lblStorageEstimate.Text = "~ 22.5 GB/hour"
        lblStorageEstimate.Font = New Drawing.Font("Segoe UI", 9F)
        lblStorageEstimate.ForeColor = _accentGreen
        lblStorageEstimate.Location = New Drawing.Point(12, 166)
        lblStorageEstimate.AutoSize = True

        pnlSettings.Controls.Add(lblSettingsTitle)
        pnlSettings.Controls.Add(lblFPS)
        pnlSettings.Controls.Add(cboFPS)
        pnlSettings.Controls.Add(lblPreset)
        pnlSettings.Controls.Add(cboPreset)
        pnlSettings.Controls.Add(lblResolution)
        pnlSettings.Controls.Add(cboResolution)
        pnlSettings.Controls.Add(lblBitrate)
        pnlSettings.Controls.Add(trkBitrate)
        pnlSettings.Controls.Add(lblBitrateValue)
        pnlSettings.Controls.Add(lblBitrateRange)
        pnlSettings.Controls.Add(lblStorageEstimate)

        ' ══════════════════════════════════════════════════════════
        ' PANEL 3: ADVANCED  (y=369, h=155)
        ' ══════════════════════════════════════════════════════════

        pnlAdvanced = New System.Windows.Forms.Panel()
        pnlAdvanced.Location = New Drawing.Point(15, 369)
        pnlAdvanced.Size = New Drawing.Size(550, 155)
        pnlAdvanced.BackColor = _bgPanel

        Dim lblAdvancedTitle As New System.Windows.Forms.Label()
        lblAdvancedTitle.Text = "Advanced"
        lblAdvancedTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblAdvancedTitle.ForeColor = _fgText
        lblAdvancedTitle.Location = New Drawing.Point(12, 6)
        lblAdvancedTitle.AutoSize = True

        lblFFmpegPreview = New System.Windows.Forms.Label()
        lblFFmpegPreview.Text = "FFmpeg Preview"
        lblFFmpegPreview.Font = New Drawing.Font("Segoe UI", 9F)
        lblFFmpegPreview.ForeColor = _fgDim
        lblFFmpegPreview.Location = New Drawing.Point(12, 28)
        lblFFmpegPreview.AutoSize = True

        txtFFmpegPreview = New System.Windows.Forms.TextBox()
        txtFFmpegPreview.Location = New Drawing.Point(12, 46)
        txtFFmpegPreview.Size = New Drawing.Size(440, 62)
        txtFFmpegPreview.BackColor = _bgInput
        txtFFmpegPreview.ForeColor = Drawing.Color.FromArgb(180, 220, 130)
        txtFFmpegPreview.Font = New Drawing.Font("Consolas", 8F)
        txtFFmpegPreview.Multiline = True
        txtFFmpegPreview.ReadOnly = True
        txtFFmpegPreview.ScrollBars = System.Windows.ScrollBars.Vertical
        txtFFmpegPreview.WordWrap = True
        txtFFmpegPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle

        btnPreviewRefresh = New System.Windows.Forms.Button()
        btnPreviewRefresh.Text = "Refresh"
        btnPreviewRefresh.Font = New Drawing.Font("Segoe UI", 8F)
        btnPreviewRefresh.ForeColor = _fgText
        btnPreviewRefresh.BackColor = _bgInput
        btnPreviewRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnPreviewRefresh.FlatAppearance.BorderSize = 0
        btnPreviewRefresh.Location = New Drawing.Point(458, 46)
        btnPreviewRefresh.Size = New Drawing.Size(40, 24)
        btnPreviewRefresh.Cursor = System.Windows.Forms.Cursors.Hand

        btnPreviewCopy = New System.Windows.Forms.Button()
        btnPreviewCopy.Text = "Copy"
        btnPreviewCopy.Font = New Drawing.Font("Segoe UI", 8F)
        btnPreviewCopy.ForeColor = _fgText
        btnPreviewCopy.BackColor = _bgInput
        btnPreviewCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnPreviewCopy.FlatAppearance.BorderSize = 0
        btnPreviewCopy.Location = New Drawing.Point(504, 46)
        btnPreviewCopy.Size = New Drawing.Size(34, 24)
        btnPreviewCopy.Cursor = System.Windows.Forms.Cursors.Hand

        lblCodecEncoder = New System.Windows.Forms.Label()
        lblCodecEncoder.Text = "Codec Encoder"
        lblCodecEncoder.Font = New Drawing.Font("Segoe UI", 9F)
        lblCodecEncoder.ForeColor = _fgDim
        lblCodecEncoder.Location = New Drawing.Point(12, 116)
        lblCodecEncoder.AutoSize = True

        cboCodecEncoder = New System.Windows.Forms.ComboBox()
        cboCodecEncoder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboCodecEncoder.Location = New Drawing.Point(120, 114)
        cboCodecEncoder.Size = New Drawing.Size(418, 23)
        cboCodecEncoder.BackColor = _bgInput
        cboCodecEncoder.ForeColor = _fgText
        cboCodecEncoder.Font = New Drawing.Font("Segoe UI", 9F)

        pnlAdvanced.Controls.Add(lblAdvancedTitle)
        pnlAdvanced.Controls.Add(lblFFmpegPreview)
        pnlAdvanced.Controls.Add(txtFFmpegPreview)
        pnlAdvanced.Controls.Add(btnPreviewRefresh)
        pnlAdvanced.Controls.Add(btnPreviewCopy)
        pnlAdvanced.Controls.Add(lblCodecEncoder)
        pnlAdvanced.Controls.Add(cboCodecEncoder)

        ' ══════════════════════════════════════════════════════════
        ' PANEL 4: CAPTURE METHOD  (y=532, h=55)
        ' ══════════════════════════════════════════════════════════

        pnlCapture = New System.Windows.Forms.Panel()
        pnlCapture.Location = New Drawing.Point(15, 532)
        pnlCapture.Size = New Drawing.Size(550, 55)
        pnlCapture.BackColor = _bgPanel

        lblCaptureTitle = New System.Windows.Forms.Label()
        lblCaptureTitle.Text = "Capture Method"
        lblCaptureTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblCaptureTitle.ForeColor = _fgText
        lblCaptureTitle.Location = New Drawing.Point(12, 6)
        lblCaptureTitle.AutoSize = True

        cboCaptureMethod = New System.Windows.Forms.ComboBox()
        cboCaptureMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboCaptureMethod.Location = New Drawing.Point(12, 28)
        cboCaptureMethod.Size = New Drawing.Size(526, 23)
        cboCaptureMethod.BackColor = _bgInput
        cboCaptureMethod.ForeColor = _fgText
        cboCaptureMethod.Font = New Drawing.Font("Segoe UI", 9F)
        cboCaptureMethod.Items.AddRange(New Object() {"ddagrab", "gdigrab", "gfxcapture"})
        cboCaptureMethod.SelectedIndex = 0

        pnlCapture.Controls.Add(lblCaptureTitle)
        pnlCapture.Controls.Add(cboCaptureMethod)

        ' ══════════════════════════════════════════════════════════
        ' PANEL 5: OUTPUT  (y=595, h=52)
        ' ══════════════════════════════════════════════════════════

        pnlOutput = New System.Windows.Forms.Panel()
        pnlOutput.Location = New Drawing.Point(15, 595)
        pnlOutput.Size = New Drawing.Size(550, 52)
        pnlOutput.BackColor = _bgPanel

        lblOutputTitle = New System.Windows.Forms.Label()
        lblOutputTitle.Text = "Output"
        lblOutputTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblOutputTitle.ForeColor = _fgText
        lblOutputTitle.Location = New Drawing.Point(12, 6)
        lblOutputTitle.AutoSize = True

        txtOutputDir = New System.Windows.Forms.TextBox()
        txtOutputDir.Location = New Drawing.Point(12, 28)
        txtOutputDir.Size = New Drawing.Size(488, 23)
        txtOutputDir.BackColor = _bgInput
        txtOutputDir.ForeColor = _fgText
        txtOutputDir.BorderStyle = System.Windows.Forms.BorderStyle.None
        txtOutputDir.ReadOnly = True

        btnBrowse = New System.Windows.Forms.Button()
        btnBrowse.Text = "..."
        btnBrowse.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        btnBrowse.ForeColor = _fgText
        btnBrowse.BackColor = _bgInput
        btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnBrowse.FlatAppearance.BorderSize = 0
        btnBrowse.Location = New Drawing.Point(506, 28)
        btnBrowse.Size = New Drawing.Size(32, 23)
        btnBrowse.Cursor = System.Windows.Forms.Cursors.Hand

        pnlOutput.Controls.Add(lblOutputTitle)
        pnlOutput.Controls.Add(txtOutputDir)
        pnlOutput.Controls.Add(btnBrowse)

        ' ══════════════════════════════════════════════════════════
        ' PANEL 6: FFMPEG PATH  (y=655, h=60)
        ' ══════════════════════════════════════════════════════════

        pnlFFmpeg = New System.Windows.Forms.Panel()
        pnlFFmpeg.Location = New Drawing.Point(15, 655)
        pnlFFmpeg.Size = New Drawing.Size(550, 60)
        pnlFFmpeg.BackColor = _bgPanel

        lblFFmpegTitle = New System.Windows.Forms.Label()
        lblFFmpegTitle.Text = "FFmpeg Path"
        lblFFmpegTitle.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        lblFFmpegTitle.ForeColor = _fgText
        lblFFmpegTitle.Location = New Drawing.Point(12, 6)
        lblFFmpegTitle.AutoSize = True

        txtFFmpegPath = New System.Windows.Forms.TextBox()
        txtFFmpegPath.Location = New Drawing.Point(12, 28)
        txtFFmpegPath.Size = New Drawing.Size(488, 23)
        txtFFmpegPath.BackColor = _bgInput
        txtFFmpegPath.ForeColor = _fgText
        txtFFmpegPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle

        btnFFmpegBrowse = New System.Windows.Forms.Button()
        btnFFmpegBrowse.Text = "..."
        btnFFmpegBrowse.Font = New Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        btnFFmpegBrowse.ForeColor = _fgText
        btnFFmpegBrowse.BackColor = _bgInput
        btnFFmpegBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnFFmpegBrowse.FlatAppearance.BorderSize = 0
        btnFFmpegBrowse.Location = New Drawing.Point(506, 28)
        btnFFmpegBrowse.Size = New Drawing.Size(32, 23)
        btnFFmpegBrowse.Cursor = System.Windows.Forms.Cursors.Hand

        lblFFmpegStatus = New System.Windows.Forms.Label()
        lblFFmpegStatus.Text = ""
        lblFFmpegStatus.Font = New Drawing.Font("Segoe UI", 8F)
        lblFFmpegStatus.ForeColor = _accentRed
        lblFFmpegStatus.Location = New Drawing.Point(12, 54)
        lblFFmpegStatus.AutoSize = True

        pnlFFmpeg.Controls.Add(lblFFmpegTitle)
        pnlFFmpeg.Controls.Add(txtFFmpegPath)
        pnlFFmpeg.Controls.Add(btnFFmpegBrowse)
        pnlFFmpeg.Controls.Add(lblFFmpegStatus)

        ' ══════════════════════════════════════════════════════════
        ' BOTTOM: RECORD / STOP + HOTKEYS
        ' ══════════════════════════════════════════════════════════

        btnRecord = New System.Windows.Forms.Button()
        btnRecord.Text = "RECORD"
        btnRecord.Font = New Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        btnRecord.ForeColor = Drawing.Color.White
        btnRecord.BackColor = _bgButton
        btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRecord.FlatAppearance.BorderSize = 0
        btnRecord.Location = New Drawing.Point(15, 725)
        btnRecord.Size = New Drawing.Size(265, 42)
        btnRecord.Cursor = System.Windows.Forms.Cursors.Hand

        btnStop = New System.Windows.Forms.Button()
        btnStop.Text = "STOP"
        btnStop.Font = New Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        btnStop.ForeColor = Drawing.Color.White
        btnStop.BackColor = _bgButtonStop
        btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnStop.FlatAppearance.BorderSize = 0
        btnStop.Location = New Drawing.Point(290, 725)
        btnStop.Size = New Drawing.Size(265, 42)
        btnStop.Cursor = System.Windows.Forms.Cursors.Hand
        btnStop.Enabled = False

        lblHotkeys = New System.Windows.Forms.Label()
        lblHotkeys.Text = "Hotkeys: Start=Ctrl+Shift+F9 | Stop=Ctrl+Shift+F10"
        lblHotkeys.Font = New Drawing.Font("Segoe UI", 8F)
        lblHotkeys.ForeColor = _fgDim
        lblHotkeys.Location = New Drawing.Point(15, 775)
        lblHotkeys.AutoSize = True

        ' ── Recording Timer ──
        tmrRecording = New System.Windows.Forms.Timer(components)
        tmrRecording.Interval = 1000

        ' ══════════════════════════════════════════════════════════
        ' ADD ALL TOP-LEVEL CONTROLS
        ' ══════════════════════════════════════════════════════════

        Me.Controls.Add(lblTitle)
        Me.Controls.Add(lblStatus)
        Me.Controls.Add(lblTimer)
        Me.Controls.Add(pnlQuality)
        Me.Controls.Add(pnlSettings)
        Me.Controls.Add(pnlAdvanced)
        Me.Controls.Add(pnlCapture)
        Me.Controls.Add(pnlOutput)
        Me.Controls.Add(pnlFFmpeg)
        Me.Controls.Add(btnRecord)
        Me.Controls.Add(btnStop)
        Me.Controls.Add(lblHotkeys)

        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    ' ══════════════════════════════════════════════════════════
    ' CONTROL DECLARATIONS
    ' ══════════════════════════════════════════════════════════

    ' Top section
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblTimer As System.Windows.Forms.Label

    ' Panel 1: Quality
    Friend WithEvents pnlQuality As System.Windows.Forms.Panel
    Friend WithEvents btnQualityLow As System.Windows.Forms.Button
    Friend WithEvents btnQualityMedium As System.Windows.Forms.Button
    Friend WithEvents btnQualityHigh As System.Windows.Forms.Button
    Friend WithEvents btnQualityCustom As System.Windows.Forms.Button

    ' Panel 2: Settings
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents lblFPS As System.Windows.Forms.Label
    Friend WithEvents cboFPS As System.Windows.Forms.ComboBox
    Friend WithEvents lblPreset As System.Windows.Forms.Label
    Friend WithEvents cboPreset As System.Windows.Forms.ComboBox
    Friend WithEvents lblResolution As System.Windows.Forms.Label
    Friend WithEvents cboResolution As System.Windows.Forms.ComboBox
    Friend WithEvents lblBitrate As System.Windows.Forms.Label
    Friend WithEvents trkBitrate As System.Windows.Forms.TrackBar
    Friend WithEvents lblBitrateValue As System.Windows.Forms.Label
    Friend WithEvents lblBitrateRange As System.Windows.Forms.Label
    Friend WithEvents lblStorageEstimate As System.Windows.Forms.Label

    ' Panel 3: Advanced
    Friend WithEvents pnlAdvanced As System.Windows.Forms.Panel
    Friend WithEvents lblFFmpegPreview As System.Windows.Forms.Label
    Friend WithEvents txtFFmpegPreview As System.Windows.Forms.TextBox
    Friend WithEvents btnPreviewRefresh As System.Windows.Forms.Button
    Friend WithEvents btnPreviewCopy As System.Windows.Forms.Button
    Friend WithEvents lblCodecEncoder As System.Windows.Forms.Label
    Friend WithEvents cboCodecEncoder As System.Windows.Forms.ComboBox

    ' Panel 4: Capture Method
    Friend WithEvents pnlCapture As System.Windows.Forms.Panel
    Friend WithEvents lblCaptureTitle As System.Windows.Forms.Label
    Friend WithEvents cboCaptureMethod As System.Windows.Forms.ComboBox

    ' Panel 5: Output
    Friend WithEvents pnlOutput As System.Windows.Forms.Panel
    Friend WithEvents lblOutputTitle As System.Windows.Forms.Label
    Friend WithEvents txtOutputDir As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowse As System.Windows.Forms.Button

    ' Panel 6: FFmpeg Path
    Friend WithEvents pnlFFmpeg As System.Windows.Forms.Panel
    Friend WithEvents lblFFmpegTitle As System.Windows.Forms.Label
    Friend WithEvents txtFFmpegPath As System.Windows.Forms.TextBox
    Friend WithEvents btnFFmpegBrowse As System.Windows.Forms.Button
    Friend WithEvents lblFFmpegStatus As System.Windows.Forms.Label

    ' Bottom
    Friend WithEvents btnRecord As System.Windows.Forms.Button
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents lblHotkeys As System.Windows.Forms.Label
    Friend WithEvents tmrRecording As System.Windows.Forms.Timer

End Class
