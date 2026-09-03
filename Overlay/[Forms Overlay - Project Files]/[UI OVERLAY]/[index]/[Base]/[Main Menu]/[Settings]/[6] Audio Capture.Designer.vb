<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_AudioSet
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        setret = New Panel()
        quality_main = New Label()
        ICO_MENU1 = New Label()
        text_settings = New Label()
        Menu_Top_Dim = New PictureBox()
        radSingle = New RadioButton()
        radSeparate = New RadioButton()
        chkSystem = New CheckBox()
        lblSystemVolTitle = New Label()
        trkSystemVol = New TrackBar()
        lblSystemVol = New Label()
        chkMic = New CheckBox()
        lblMicVolTitle = New Label()
        trkMicVol = New TrackBar()
        lblMicVol = New Label()
        lblMicDevice = New Label()
        cboMic = New ComboBox()
        lblStatus = New Label()
        action_fn = New Label()
        btnRefresh = New Label()
        setret.SuspendLayout()
        CType(Menu_Top_Dim, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkSystemVol, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkMicVol, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' setret
        ' 
        setret.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        setret.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        setret.Controls.Add(quality_main)
        setret.Controls.Add(ICO_MENU1)
        setret.Controls.Add(text_settings)
        setret.Controls.Add(Menu_Top_Dim)
        setret.Controls.Add(radSingle)
        setret.Controls.Add(radSeparate)
        setret.Controls.Add(chkSystem)
        setret.Controls.Add(lblSystemVolTitle)
        setret.Controls.Add(trkSystemVol)
        setret.Controls.Add(lblSystemVol)
        setret.Controls.Add(chkMic)
        setret.Controls.Add(lblMicVolTitle)
        setret.Controls.Add(trkMicVol)
        setret.Controls.Add(lblMicVol)
        setret.Controls.Add(lblMicDevice)
        setret.Controls.Add(cboMic)
        setret.Controls.Add(lblStatus)
        setret.Location = New Point(80, 160)
        setret.Name = "setret"
        setret.Size = New Size(1520, 630)
        setret.TabIndex = 0
        ' 
        ' quality_main
        ' 
        quality_main.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        quality_main.Font = New Font("Segoe UI Semibold", 18F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        quality_main.ForeColor = Color.White
        quality_main.Location = New Point(129, 85)
        quality_main.Name = "quality_main"
        quality_main.Size = New Size(515, 67)
        quality_main.TabIndex = 112
        quality_main.Text = "Track Mode:"
        quality_main.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ICO_MENU1
        ' 
        ICO_MENU1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ICO_MENU1.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ICO_MENU1.ForeColor = Color.White
        ICO_MENU1.Location = New Point(62, 85)
        ICO_MENU1.Name = "ICO_MENU1"
        ICO_MENU1.Size = New Size(61, 67)
        ICO_MENU1.TabIndex = 113
        ICO_MENU1.Text = ""
        ICO_MENU1.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' text_settings
        ' 
        text_settings.AutoSize = True
        text_settings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        text_settings.Font = New Font("GeForce", 24F, FontStyle.Bold)
        text_settings.ForeColor = Color.White
        text_settings.Location = New Point(62, 43)
        text_settings.Name = "text_settings"
        text_settings.Size = New Size(210, 42)
        text_settings.TabIndex = 1
        text_settings.Text = "Audio Capture"
        ' 
        ' Menu_Top_Dim
        ' 
        Menu_Top_Dim.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Menu_Top_Dim.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Menu_Top_Dim.Location = New Point(0, 0)
        Menu_Top_Dim.Name = "Menu_Top_Dim"
        Menu_Top_Dim.Size = New Size(1760, 5)
        Menu_Top_Dim.TabIndex = 21
        Menu_Top_Dim.TabStop = False
        ' 
        ' radSingle
        ' 
        radSingle.AutoSize = True
        radSingle.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        radSingle.Checked = True
        radSingle.Font = New Font("Segoe UI", 10.5F)
        radSingle.ForeColor = Color.White
        radSingle.Location = New Point(129, 155)
        radSingle.Name = "radSingle"
        radSingle.Size = New Size(146, 23)
        radSingle.TabIndex = 4
        radSingle.TabStop = True
        radSingle.Text = "Single Track (mixed)"
        radSingle.UseVisualStyleBackColor = True
        ' 
        ' radSeparate
        ' 
        radSeparate.AutoSize = True
        radSeparate.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        radSeparate.Font = New Font("Segoe UI", 10.5F)
        radSeparate.ForeColor = Color.White
        radSeparate.Location = New Point(349, 155)
        radSeparate.Name = "radSeparate"
        radSeparate.Size = New Size(232, 23)
        radSeparate.TabIndex = 5
        radSeparate.Text = "Separate Track (mic on own track)"
        radSeparate.UseVisualStyleBackColor = True
        ' 
        ' chkSystem
        ' 
        chkSystem.AutoSize = True
        chkSystem.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        chkSystem.Checked = True
        chkSystem.CheckState = CheckState.Checked
        chkSystem.FlatStyle = FlatStyle.Flat
        chkSystem.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        chkSystem.ForeColor = Color.White
        chkSystem.Location = New Point(129, 196)
        chkSystem.Name = "chkSystem"
        chkSystem.Size = New Size(195, 25)
        chkSystem.TabIndex = 6
        chkSystem.Text = "Capture System Audio"
        chkSystem.UseVisualStyleBackColor = True
        ' 
        ' lblSystemVolTitle
        ' 
        lblSystemVolTitle.AutoSize = True
        lblSystemVolTitle.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblSystemVolTitle.Font = New Font("Segoe UI", 10.5F)
        lblSystemVolTitle.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblSystemVolTitle.Location = New Point(147, 234)
        lblSystemVolTitle.Name = "lblSystemVolTitle"
        lblSystemVolTitle.Size = New Size(103, 19)
        lblSystemVolTitle.TabIndex = 7
        lblSystemVolTitle.Text = "System Volume"
        ' 
        ' trkSystemVol
        ' 
        trkSystemVol.LargeChange = 10
        trkSystemVol.Location = New Point(147, 259)
        trkSystemVol.Maximum = 100
        trkSystemVol.Name = "trkSystemVol"
        trkSystemVol.Size = New Size(400, 45)
        trkSystemVol.TabIndex = 8
        trkSystemVol.TickFrequency = 10
        trkSystemVol.Value = 100
        ' 
        ' lblSystemVol
        ' 
        lblSystemVol.AutoSize = True
        lblSystemVol.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblSystemVol.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        lblSystemVol.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        lblSystemVol.Location = New Point(330, 198)
        lblSystemVol.Name = "lblSystemVol"
        lblSystemVol.Size = New Size(51, 21)
        lblSystemVol.TabIndex = 9
        lblSystemVol.Text = "100%"
        ' 
        ' chkMic
        ' 
        chkMic.AutoSize = True
        chkMic.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        chkMic.FlatStyle = FlatStyle.Flat
        chkMic.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        chkMic.ForeColor = Color.White
        chkMic.Location = New Point(129, 336)
        chkMic.Name = "chkMic"
        chkMic.Size = New Size(183, 25)
        chkMic.TabIndex = 10
        chkMic.Text = "Capture Microphone"
        chkMic.UseVisualStyleBackColor = True
        ' 
        ' lblMicVolTitle
        ' 
        lblMicVolTitle.AutoSize = True
        lblMicVolTitle.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblMicVolTitle.Font = New Font("Segoe UI", 10.5F)
        lblMicVolTitle.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblMicVolTitle.Location = New Point(147, 374)
        lblMicVolTitle.Name = "lblMicVolTitle"
        lblMicVolTitle.Size = New Size(133, 19)
        lblMicVolTitle.TabIndex = 11
        lblMicVolTitle.Text = "Microphone Volume"
        ' 
        ' trkMicVol
        ' 
        trkMicVol.LargeChange = 10
        trkMicVol.Location = New Point(147, 399)
        trkMicVol.Maximum = 100
        trkMicVol.Name = "trkMicVol"
        trkMicVol.Size = New Size(400, 45)
        trkMicVol.TabIndex = 12
        trkMicVol.TickFrequency = 10
        trkMicVol.Value = 100
        ' 
        ' lblMicVol
        ' 
        lblMicVol.AutoSize = True
        lblMicVol.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblMicVol.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        lblMicVol.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        lblMicVol.Location = New Point(318, 338)
        lblMicVol.Name = "lblMicVol"
        lblMicVol.Size = New Size(51, 21)
        lblMicVol.TabIndex = 13
        lblMicVol.Text = "100%"
        ' 
        ' lblMicDevice
        ' 
        lblMicDevice.AutoSize = True
        lblMicDevice.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblMicDevice.Font = New Font("Segoe UI", 10.5F)
        lblMicDevice.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblMicDevice.Location = New Point(129, 447)
        lblMicDevice.Name = "lblMicDevice"
        lblMicDevice.Size = New Size(127, 19)
        lblMicDevice.TabIndex = 14
        lblMicDevice.Text = "Microphone Device"
        ' 
        ' cboMic
        ' 
        cboMic.BackColor = Color.FromArgb(CByte(28), CByte(32), CByte(36))
        cboMic.DropDownStyle = ComboBoxStyle.DropDownList
        cboMic.FlatStyle = FlatStyle.Flat
        cboMic.ForeColor = Color.White
        cboMic.FormattingEnabled = True
        cboMic.ItemHeight = 15
        cboMic.Location = New Point(129, 472)
        cboMic.Name = "cboMic"
        cboMic.Size = New Size(500, 23)
        cboMic.TabIndex = 15
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblStatus.Font = New Font("Segoe UI", 10F)
        lblStatus.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblStatus.Location = New Point(581, 649)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(0, 19)
        lblStatus.TabIndex = 17
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(80, 110)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 50)
        action_fn.TabIndex = 59
        action_fn.Text = "Saved"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' btnRefresh
        ' 
        btnRefresh.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        btnRefresh.Cursor = Cursors.Hand
        btnRefresh.Font = New Font("Segoe UI Semibold", 12F)
        btnRefresh.ForeColor = Color.White
        btnRefresh.Location = New Point(286, 115)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(200, 50)
        btnRefresh.TabIndex = 114
        btnRefresh.Text = "Refresh"
        btnRefresh.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Base_AudioSet
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Red
        ClientSize = New Size(1680, 945)
        Controls.Add(btnRefresh)
        Controls.Add(action_fn)
        Controls.Add(setret)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Name = "Base_AudioSet"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Audio"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        setret.ResumeLayout(False)
        setret.PerformLayout()
        CType(Menu_Top_Dim, ComponentModel.ISupportInitialize).EndInit()
        CType(trkSystemVol, ComponentModel.ISupportInitialize).EndInit()
        CType(trkMicVol, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents setret As Panel
    Friend WithEvents text_settings As Label
    Friend WithEvents radSingle As RadioButton
    Friend WithEvents radSeparate As RadioButton
    Friend WithEvents chkSystem As CheckBox
    Friend WithEvents lblSystemVolTitle As Label
    Friend WithEvents trkSystemVol As TrackBar
    Friend WithEvents lblSystemVol As Label
    Friend WithEvents chkMic As CheckBox
    Friend WithEvents lblMicVolTitle As Label
    Friend WithEvents trkMicVol As TrackBar
    Friend WithEvents lblMicVol As Label
    Friend WithEvents lblMicDevice As Label
    Friend WithEvents cboMic As ComboBox
    Friend WithEvents lblStatus As Label
    Friend WithEvents Menu_Top_Dim As PictureBox
    Friend WithEvents action_fn As Label
    Friend WithEvents quality_main As Label
    Friend WithEvents ICO_MENU1 As Label
    Friend WithEvents btnRefresh As Label
End Class
