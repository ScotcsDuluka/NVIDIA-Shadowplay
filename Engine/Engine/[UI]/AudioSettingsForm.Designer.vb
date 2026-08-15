<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AudioSettingsForm
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        grpTrackMode = New GroupBox()
        radSingle = New RadioButton()
        radSeparate = New RadioButton()
        lblModeHint = New Label()
        grpSystem = New GroupBox()
        chkSystem = New CheckBox()
        trkSystemVol = New TrackBar()
        lblSystemVol = New Label()
        grpMic = New GroupBox()
        chkMic = New CheckBox()
        cboMic = New ComboBox()
        lblMicDevice = New Label()
        btnRefresh = New Button()
        trkMicVol = New TrackBar()
        lblMicVol = New Label()
        btnApply = New Button()
        btnCancel = New Button()
        btnTest = New Button()
        lblStatus = New Label()
        DIMBOX_2 = New PictureBox()
        BT_Back = New Label()
        settings_top = New PictureBox()
        settings_menu = New Panel()
        Label2 = New Label()
        PictureBox5 = New PictureBox()
        PictureBox4 = New PictureBox()
        PictureBox3 = New PictureBox()
        PictureBox16 = New PictureBox()
        hg2 = New PictureBox()
        Audio_Capture_Menutext = New Label()
        OPEN_UI = New System.Windows.Forms.Timer(components)
        grpTrackMode.SuspendLayout()
        grpSystem.SuspendLayout()
        CType(trkSystemVol, ComponentModel.ISupportInitialize).BeginInit()
        grpMic.SuspendLayout()
        CType(trkMicVol, ComponentModel.ISupportInitialize).BeginInit()
        CType(DIMBOX_2, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        settings_menu.SuspendLayout()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox16, ComponentModel.ISupportInitialize).BeginInit()
        CType(hg2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        Me.components = New System.ComponentModel.Container()

        Me.lblTitle = New System.Windows.Forms.Label()
        Me.grpTrackMode = New System.Windows.Forms.GroupBox()
        Me.radSingle = New System.Windows.Forms.RadioButton()
        Me.radSeparate = New System.Windows.Forms.RadioButton()
        Me.lblModeHint = New System.Windows.Forms.Label()

        Me.grpSystem = New System.Windows.Forms.GroupBox()
        Me.chkSystem = New System.Windows.Forms.CheckBox()
        Me.trkSystemVol = New System.Windows.Forms.TrackBar()
        Me.lblSystemVol = New System.Windows.Forms.Label()

        Me.grpMic = New System.Windows.Forms.GroupBox()
        Me.chkMic = New System.Windows.Forms.CheckBox()
        Me.cboMic = New System.Windows.Forms.ComboBox()
        Me.lblMicDevice = New System.Windows.Forms.Label()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.trkMicVol = New System.Windows.Forms.TrackBar()
        Me.lblMicVol = New System.Windows.Forms.Label()

        Me.btnApply = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnTest = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()

        Me.grpTrackMode.SuspendLayout()
        Me.grpSystem.SuspendLayout()
        CType(Me.trkSystemVol, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMic.SuspendLayout()
        CType(Me.trkMicVol, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

        '
        ' lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 14.0F, System.Drawing.FontStyle.Bold)
        Me.lblTitle.Location = New System.Drawing.Point(12, 9)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(141, 25)
        Me.lblTitle.Text = "Audio Settings"

        '
        ' grpTrackMode
        '
        Me.grpTrackMode.Controls.Add(Me.radSingle)
        Me.grpTrackMode.Controls.Add(Me.radSeparate)
        Me.grpTrackMode.Controls.Add(Me.lblModeHint)
        Me.grpTrackMode.Location = New System.Drawing.Point(12, 45)
        Me.grpTrackMode.Name = "grpTrackMode"
        Me.grpTrackMode.Size = New System.Drawing.Size(488, 95)
        Me.grpTrackMode.TabStop = False
        Me.grpTrackMode.Text = "Audio Track Mode"

        Me.radSingle.AutoSize = True
        Me.radSingle.Location = New System.Drawing.Point(15, 22)
        Me.radSingle.Name = "radSingle"
        Me.radSingle.Text = "Single track (system + mic mixed)"
        Me.radSingle.Checked = True

        Me.radSeparate.AutoSize = True
        Me.radSeparate.Location = New System.Drawing.Point(15, 45)
        Me.radSeparate.Name = "radSeparate"
        Me.radSeparate.Text = "Separate tracks (system + mic split)"

        Me.lblModeHint.AutoSize = True
        Me.lblModeHint.ForeColor = System.Drawing.Color.Gray
        Me.lblModeHint.Location = New System.Drawing.Point(15, 68)
        Me.lblModeHint.Name = "lblModeHint"
        Me.lblModeHint.Size = New System.Drawing.Size(460, 15)
        Me.lblModeHint.Text = "Single: 1 AAC track, mix of both sources.   Separate: 2 AAC tracks in same file."

        '
        ' grpSystem
        '
        Me.grpSystem.Controls.Add(Me.chkSystem)
        Me.grpSystem.Controls.Add(Me.trkSystemVol)
        Me.grpSystem.Controls.Add(Me.lblSystemVol)
        Me.grpSystem.Location = New System.Drawing.Point(12, 150)
        Me.grpSystem.Name = "grpSystem"
        Me.grpSystem.Size = New System.Drawing.Size(488, 90)
        Me.grpSystem.TabStop = False
        Me.grpSystem.Text = "System Audio (loopback)"

        Me.chkSystem.AutoSize = True
        Me.chkSystem.Location = New System.Drawing.Point(15, 22)
        Me.chkSystem.Name = "chkSystem"
        Me.chkSystem.Text = "Capture system audio"
        Me.chkSystem.Checked = True

        Me.trkSystemVol.Location = New System.Drawing.Point(15, 45)
        Me.trkSystemVol.Name = "trkSystemVol"
        Me.trkSystemVol.Size = New System.Drawing.Size(360, 45)
        Me.trkSystemVol.Minimum = 0
        Me.trkSystemVol.Maximum = 150
        Me.trkSystemVol.Value = 100

        Me.lblSystemVol.AutoSize = True
        Me.lblSystemVol.Location = New System.Drawing.Point(385, 50)
        Me.lblSystemVol.Name = "lblSystemVol"
        Me.lblSystemVol.Text = "100%"

        '
        ' grpMic
        ' 
        grpMic.Controls.Add(chkMic)
        grpMic.Controls.Add(cboMic)
        grpMic.Controls.Add(lblMicDevice)
        grpMic.Controls.Add(btnRefresh)
        grpMic.Controls.Add(trkMicVol)
        grpMic.Controls.Add(lblMicVol)
        grpMic.ForeColor = Color.White
        grpMic.Location = New Point(129, 395)
        grpMic.Name = "grpMic"
        grpMic.Size = New Size(488, 130)
        grpMic.TabIndex = 3
        grpMic.TabStop = False
        grpMic.Text = "Microphone"
        ' 
        ' chkMic
        ' 
        chkMic.AutoSize = True
        chkMic.Location = New Point(15, 22)
        chkMic.Name = "chkMic"
        chkMic.Size = New Size(136, 19)
        chkMic.TabIndex = 0
        chkMic.Text = "Capture microphone"
        ' 
        ' cboMic
        ' 
        cboMic.DropDownStyle = ComboBoxStyle.DropDownList
        cboMic.Location = New Point(70, 45)
        cboMic.Name = "cboMic"
        cboMic.Size = New Size(330, 23)
        cboMic.TabIndex = 1
        ' 
        ' lblMicDevice
        ' 
        lblMicDevice.AutoSize = True
        lblMicDevice.Location = New Point(15, 48)
        lblMicDevice.Name = "lblMicDevice"
        lblMicDevice.Size = New Size(45, 15)
        lblMicDevice.TabIndex = 2
        lblMicDevice.Text = "Device:"
        ' 
        ' btnRefresh
        ' 
        btnRefresh.Location = New Point(408, 44)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(70, 25)
        btnRefresh.TabIndex = 3
        btnRefresh.Text = "Refresh"
        ' 
        ' trkMicVol
        ' 
        trkMicVol.Location = New Point(15, 80)
        trkMicVol.Maximum = 150
        trkMicVol.Name = "trkMicVol"
        trkMicVol.Size = New Size(360, 45)
        trkMicVol.TabIndex = 4
        trkMicVol.Value = 100
        ' 
        ' lblMicVol
        ' 
        lblMicVol.AutoSize = True
        lblMicVol.Location = New Point(385, 85)
        lblMicVol.Name = "lblMicVol"
        lblMicVol.Size = New Size(35, 15)
        lblMicVol.TabIndex = 5
        lblMicVol.Text = "100%"
        ' 
        ' btnApply
        ' 
        btnApply.ForeColor = Color.Black
        btnApply.Location = New Point(351, 540)
        btnApply.Name = "btnApply"
        btnApply.Size = New Size(85, 30)
        btnApply.TabIndex = 4
        btnApply.Text = "Apply"
        btnApply.UseVisualStyleBackColor = True
        ' 
        ' btnCancel
        ' 
        btnCancel.ForeColor = Color.Black
        btnCancel.Location = New Point(442, 540)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(85, 30)
        btnCancel.TabIndex = 5
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnTest
        ' 
        btnTest.ForeColor = Color.Black
        btnTest.Location = New Point(129, 540)
        btnTest.Name = "btnTest"
        btnTest.Size = New Size(85, 30)
        btnTest.TabIndex = 6
        btnTest.Text = "Test"
        btnTest.UseVisualStyleBackColor = True
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.ForeColor = Color.Gray
        lblStatus.Location = New Point(222, 547)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(0, 15)
        lblStatus.TabIndex = 7
        ' 
        ' DIMBOX_2
        ' 
        DIMBOX_2.BackColor = Color.Blue
        DIMBOX_2.BackgroundImageLayout = ImageLayout.None
        DIMBOX_2.Location = New Point(0, 160)
        DIMBOX_2.Name = "DIMBOX_2"
        DIMBOX_2.Size = New Size(80, 80)
        DIMBOX_2.TabIndex = 94
        DIMBOX_2.TabStop = False
        DIMBOX_2.Visible = False
        ' 
        ' BT_Back
        ' 
        BT_Back.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_Back.Cursor = Cursors.Hand
        BT_Back.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        BT_Back.ForeColor = Color.White
        BT_Back.Location = New Point(80, 110)
        BT_Back.Name = "BT_Back"
        BT_Back.Size = New Size(200, 50)
        BT_Back.TabIndex = 99
        BT_Back.Text = "Back"
        BT_Back.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(80, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(1760, 5)
        settings_top.TabIndex = 97
        settings_top.TabStop = False
        ' 
        ' settings_menu
        ' 
        settings_menu.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        settings_menu.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_menu.Controls.Add(Label2)
        settings_menu.Controls.Add(PictureBox5)
        settings_menu.Controls.Add(PictureBox4)
        settings_menu.Controls.Add(PictureBox3)
        settings_menu.Controls.Add(PictureBox16)
        settings_menu.Controls.Add(grpTrackMode)
        settings_menu.Controls.Add(grpSystem)
        settings_menu.Controls.Add(hg2)
        settings_menu.Controls.Add(grpMic)
        settings_menu.Controls.Add(btnApply)
        settings_menu.Controls.Add(btnCancel)
        settings_menu.Controls.Add(btnTest)
        settings_menu.Controls.Add(lblStatus)
        settings_menu.Controls.Add(Audio_Capture_Menutext)
        settings_menu.ForeColor = Color.White
        settings_menu.Location = New Point(80, 160)
        settings_menu.Name = "settings_menu"
        settings_menu.Size = New Size(1760, 840)
        settings_menu.TabIndex = 98
        ' 
        ' Label2
        ' 
        Label2.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New Font("nvgcshare", 50.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.White
        Label2.Location = New Point(62, 85)
        Label2.Name = "Label2"
        Label2.Size = New Size(61, 67)
        Label2.TabIndex = 111
        Label2.Text = ""
        Label2.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' PictureBox5
        ' 
        PictureBox5.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        PictureBox5.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox5.Location = New Point(269, 2190)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New Size(254, 116)
        PictureBox5.TabIndex = 91
        PictureBox5.TabStop = False
        PictureBox5.Visible = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        PictureBox4.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox4.Location = New Point(12, 2190)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(256, 116)
        PictureBox4.TabIndex = 90
        PictureBox4.TabStop = False
        PictureBox4.Visible = False
        ' 
        ' PictureBox3
        ' 
        PictureBox3.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        PictureBox3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox3.Location = New Point(269, 2069)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(254, 120)
        PictureBox3.TabIndex = 89
        PictureBox3.TabStop = False
        PictureBox3.Visible = False
        ' 
        ' PictureBox16
        ' 
        PictureBox16.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        PictureBox16.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox16.Location = New Point(12, 2069)
        PictureBox16.Name = "PictureBox16"
        PictureBox16.Size = New Size(256, 120)
        PictureBox16.TabIndex = 88
        PictureBox16.TabStop = False
        PictureBox16.Visible = False
        ' 
        ' hg2
        ' 
        hg2.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        hg2.BackColor = Color.DimGray
        hg2.Location = New Point(11, 2068)
        hg2.Name = "hg2"
        hg2.Size = New Size(513, 239)
        hg2.TabIndex = 87
        hg2.TabStop = False
        hg2.Visible = False
        ' 
        ' Audio_Capture_Menutext
        ' 
        Audio_Capture_Menutext.AutoSize = True
        Audio_Capture_Menutext.Font = New Font("Segoe UI", 17.0F, FontStyle.Bold)
        Audio_Capture_Menutext.Location = New Point(62, 43)
        Audio_Capture_Menutext.Name = "Audio_Capture_Menutext"
        Audio_Capture_Menutext.Size = New Size(169, 31)
        Audio_Capture_Menutext.TabIndex = 0
        Audio_Capture_Menutext.Text = "Audio Capture"
        ' 
        ' OPEN_UI
        ' 
        OPEN_UI.Enabled = True
        OPEN_UI.Interval = 1
        ' 
        ' AudioSettingsForm
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Coral
        ClientSize = New Size(1920, 1080)
        Controls.Add(BT_Back)
        Controls.Add(settings_top)
        Controls.Add(settings_menu)
        Controls.Add(DIMBOX_2)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "AudioSettingsForm"
        Opacity = 0R
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "Audio Settings — ShadowPlay Engine"
        TopMost = True
        TransparencyKey = Color.Coral
        WindowState = FormWindowState.Maximized
        grpTrackMode.ResumeLayout(False)
        grpTrackMode.PerformLayout()
        grpSystem.ResumeLayout(False)
        grpSystem.PerformLayout()
        CType(trkSystemVol, ComponentModel.ISupportInitialize).EndInit()
        grpMic.ResumeLayout(False)
        grpMic.PerformLayout()
        CType(trkMicVol, ComponentModel.ISupportInitialize).EndInit()
        CType(DIMBOX_2, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        settings_menu.ResumeLayout(False)
        settings_menu.PerformLayout()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox16, ComponentModel.ISupportInitialize).EndInit()
        CType(hg2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        '
        Me.grpMic.Controls.Add(Me.chkMic)
        Me.grpMic.Controls.Add(Me.cboMic)
        Me.grpMic.Controls.Add(Me.lblMicDevice)
        Me.grpMic.Controls.Add(Me.btnRefresh)
        Me.grpMic.Controls.Add(Me.trkMicVol)
        Me.grpMic.Controls.Add(Me.lblMicVol)
        Me.grpMic.Location = New System.Drawing.Point(12, 250)
        Me.grpMic.Name = "grpMic"
        Me.grpMic.Size = New System.Drawing.Size(488, 130)
        Me.grpMic.TabStop = False
        Me.grpMic.Text = "Microphone"

        Me.chkMic.AutoSize = True
        Me.chkMic.Location = New System.Drawing.Point(15, 22)
        Me.chkMic.Name = "chkMic"
        Me.chkMic.Text = "Capture microphone"

        Me.lblMicDevice.AutoSize = True
        Me.lblMicDevice.Location = New System.Drawing.Point(15, 48)
        Me.lblMicDevice.Name = "lblMicDevice"
        Me.lblMicDevice.Text = "Device:"

        Me.cboMic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMic.Location = New System.Drawing.Point(70, 45)
        Me.cboMic.Name = "cboMic"
        Me.cboMic.Size = New System.Drawing.Size(330, 23)

        Me.btnRefresh.Location = New System.Drawing.Point(408, 44)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(70, 25)
        Me.btnRefresh.Text = "Refresh"

        Me.trkMicVol.Location = New System.Drawing.Point(15, 80)
        Me.trkMicVol.Name = "trkMicVol"
        Me.trkMicVol.Size = New System.Drawing.Size(360, 45)
        Me.trkMicVol.Minimum = 0
        Me.trkMicVol.Maximum = 150
        Me.trkMicVol.Value = 100

        Me.lblMicVol.AutoSize = True
        Me.lblMicVol.Location = New System.Drawing.Point(385, 85)
        Me.lblMicVol.Name = "lblMicVol"
        Me.lblMicVol.Text = "100%"

        '
        ' btnApply / btnCancel / btnTest
        '
        Me.btnApply.Location = New System.Drawing.Point(234, 395)
        Me.btnApply.Name = "btnApply"
        Me.btnApply.Size = New System.Drawing.Size(85, 30)
        Me.btnApply.Text = "Apply"
        Me.btnApply.UseVisualStyleBackColor = True

        Me.btnCancel.Location = New System.Drawing.Point(325, 395)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(85, 30)
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True

        Me.btnTest.Location = New System.Drawing.Point(12, 395)
        Me.btnTest.Name = "btnTest"
        Me.btnTest.Size = New System.Drawing.Size(85, 30)
        Me.btnTest.Text = "Test"
        Me.btnTest.UseVisualStyleBackColor = True

        Me.lblStatus.AutoSize = True
        Me.lblStatus.ForeColor = System.Drawing.Color.Gray
        Me.lblStatus.Location = New System.Drawing.Point(105, 402)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Text = ""

        '
        ' AudioSettingsForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0F, 15.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(516, 437)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.grpTrackMode)
        Me.Controls.Add(Me.grpSystem)
        Me.Controls.Add(Me.grpMic)
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnTest)
        Me.Controls.Add(Me.lblStatus)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "AudioSettingsForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Audio Settings — ShadowPlay Engine"
        Me.grpTrackMode.ResumeLayout(False)
        Me.grpTrackMode.PerformLayout()
        Me.grpSystem.ResumeLayout(False)
        Me.grpSystem.PerformLayout()
        CType(Me.trkSystemVol, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMic.ResumeLayout(False)
        Me.grpMic.PerformLayout()
        CType(Me.trkMicVol, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents grpTrackMode As System.Windows.Forms.GroupBox
    Friend WithEvents radSingle As System.Windows.Forms.RadioButton
    Friend WithEvents radSeparate As System.Windows.Forms.RadioButton
    Friend WithEvents lblModeHint As System.Windows.Forms.Label
    Friend WithEvents grpSystem As System.Windows.Forms.GroupBox
    Friend WithEvents chkSystem As System.Windows.Forms.CheckBox
    Friend WithEvents trkSystemVol As System.Windows.Forms.TrackBar
    Friend WithEvents lblSystemVol As System.Windows.Forms.Label
    Friend WithEvents grpMic As System.Windows.Forms.GroupBox
    Friend WithEvents chkMic As System.Windows.Forms.CheckBox
    Friend WithEvents cboMic As System.Windows.Forms.ComboBox
    Friend WithEvents lblMicDevice As System.Windows.Forms.Label
    Friend WithEvents btnRefresh As System.Windows.Forms.Button
    Friend WithEvents trkMicVol As System.Windows.Forms.TrackBar
    Friend WithEvents lblMicVol As System.Windows.Forms.Label
    Friend WithEvents btnApply As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnTest As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
End Class
