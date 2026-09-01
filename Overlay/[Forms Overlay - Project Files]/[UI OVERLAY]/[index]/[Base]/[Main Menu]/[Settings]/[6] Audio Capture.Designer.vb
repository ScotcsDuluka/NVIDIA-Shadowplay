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
        components = New ComponentModel.Container()
        setret = New Panel()
        text_settings = New Label()
        lblPageTitle = New Label()
        lblTrackMode = New Label()
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
        btnRefresh = New Button()
        lblStatus = New Label()
        btnApply = New Button()
        btnTest = New Button()
        action_fn = New Button()
        setret.SuspendLayout()
        CType(trkSystemVol, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkMicVol, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' setret — content host (same contract as Base_RecordingsSet.setret:
        ' OpenPanel() repositions this control to (80,160))
        ' 
        setret.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        setret.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        setret.Controls.Add(text_settings)
        setret.Controls.Add(lblPageTitle)
        setret.Controls.Add(lblTrackMode)
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
        setret.Controls.Add(btnRefresh)
        setret.Controls.Add(lblStatus)
        setret.Controls.Add(btnApply)
        setret.Controls.Add(btnTest)
        setret.Location = New Point(80, 160)
        setret.Name = "setret"
        setret.Size = New Size(1520, 707)
        setret.TabIndex = 0
        ' 
        ' text_settings
        ' 
        text_settings.AutoSize = True
        text_settings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        text_settings.Font = New Font("GeForce", 24F, FontStyle.Bold)
        text_settings.ForeColor = Color.White
        text_settings.Location = New Point(62, 43)
        text_settings.Name = "text_settings"
        text_settings.Size = New Size(61, 42)
        text_settings.TabIndex = 1
        text_settings.Text = "AU"
        ' 
        ' lblPageTitle
        ' 
        lblPageTitle.AutoSize = True
        lblPageTitle.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblPageTitle.Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold)
        lblPageTitle.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblPageTitle.Location = New Point(150, 55)
        lblPageTitle.Name = "lblPageTitle"
        lblPageTitle.Size = New Size(150, 25)
        lblPageTitle.TabIndex = 2
        lblPageTitle.Text = "Audio Capture"
        ' 
        ' lblTrackMode
        ' 
        lblTrackMode.AutoSize = True
        lblTrackMode.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblTrackMode.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        lblTrackMode.ForeColor = Color.White
        lblTrackMode.Location = New Point(62, 130)
        lblTrackMode.Name = "lblTrackMode"
        lblTrackMode.Size = New Size(100, 21)
        lblTrackMode.TabIndex = 3
        lblTrackMode.Text = "Track Mode"
        ' 
        ' radSingle
        ' 
        radSingle.AutoSize = True
        radSingle.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        radSingle.Checked = True
        radSingle.Font = New Font("Segoe UI", 10.5F)
        radSingle.ForeColor = Color.White
        radSingle.Location = New Point(80, 165)
        radSingle.Name = "radSingle"
        radSingle.Size = New Size(180, 24)
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
        radSeparate.Location = New Point(300, 165)
        radSeparate.Name = "radSeparate"
        radSeparate.Size = New Size(230, 24)
        radSeparate.TabIndex = 5
        radSeparate.Text = "Separate Track (mic on own track)"
        radSeparate.UseVisualStyleBackColor = True
        ' 
        ' chkSystem
        ' 
        chkSystem.AutoSize = True
        chkSystem.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        chkSystem.Checked = True
        chkSystem.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        chkSystem.ForeColor = Color.White
        chkSystem.Location = New Point(62, 220)
        chkSystem.Name = "chkSystem"
        chkSystem.Size = New Size(220, 25)
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
        lblSystemVolTitle.Location = New Point(80, 258)
        lblSystemVolTitle.Name = "lblSystemVolTitle"
        lblSystemVolTitle.Size = New Size(100, 19)
        lblSystemVolTitle.TabIndex = 7
        lblSystemVolTitle.Text = "System Volume"
        ' 
        ' trkSystemVol
        ' 
        trkSystemVol.LargeChange = 10
        trkSystemVol.Location = New Point(80, 283)
        trkSystemVol.Maximum = 100
        trkSystemVol.Name = "trkSystemVol"
        trkSystemVol.Size = New Size(400, 56)
        trkSystemVol.SmallChange = 1
        trkSystemVol.TabIndex = 8
        trkSystemVol.TickFrequency = 10
        trkSystemVol.Value = 100
        ' 
        ' lblSystemVol
        ' 
        lblSystemVol.AutoSize = True
        lblSystemVol.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblSystemVol.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        lblSystemVol.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        lblSystemVol.Location = New Point(500, 295)
        lblSystemVol.Name = "lblSystemVol"
        lblSystemVol.Size = New Size(50, 21)
        lblSystemVol.TabIndex = 9
        lblSystemVol.Text = "100%"
        ' 
        ' chkMic
        ' 
        chkMic.AutoSize = True
        chkMic.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        chkMic.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        chkMic.ForeColor = Color.White
        chkMic.Location = New Point(62, 360)
        chkMic.Name = "chkMic"
        chkMic.Size = New Size(200, 25)
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
        lblMicVolTitle.Location = New Point(80, 398)
        lblMicVolTitle.Name = "lblMicVolTitle"
        lblMicVolTitle.Size = New Size(150, 19)
        lblMicVolTitle.TabIndex = 11
        lblMicVolTitle.Text = "Microphone Volume"
        ' 
        ' trkMicVol
        ' 
        trkMicVol.LargeChange = 10
        trkMicVol.Location = New Point(80, 423)
        trkMicVol.Maximum = 100
        trkMicVol.Name = "trkMicVol"
        trkMicVol.Size = New Size(400, 56)
        trkMicVol.SmallChange = 1
        trkMicVol.TabIndex = 12
        trkMicVol.TickFrequency = 10
        trkMicVol.Value = 100
        ' 
        ' lblMicVol
        ' 
        lblMicVol.AutoSize = True
        lblMicVol.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblMicVol.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        lblMicVol.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        lblMicVol.Location = New Point(500, 435)
        lblMicVol.Name = "lblMicVol"
        lblMicVol.Size = New Size(50, 21)
        lblMicVol.TabIndex = 13
        lblMicVol.Text = "100%"
        ' 
        ' lblMicDevice
        ' 
        lblMicDevice.AutoSize = True
        lblMicDevice.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblMicDevice.Font = New Font("Segoe UI", 10.5F)
        lblMicDevice.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblMicDevice.Location = New Point(80, 500)
        lblMicDevice.Name = "lblMicDevice"
        lblMicDevice.Size = New Size(120, 19)
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
        cboMic.ItemHeight = 20
        cboMic.Location = New Point(80, 525)
        cboMic.Name = "cboMic"
        cboMic.Size = New Size(500, 28)
        cboMic.TabIndex = 15
        ' 
        ' btnRefresh
        ' 
        btnRefresh.BackColor = Color.FromArgb(CByte(50), CByte(56), CByte(61))
        btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        btnRefresh.FlatStyle = FlatStyle.Flat
        btnRefresh.ForeColor = Color.White
        btnRefresh.Location = New Point(600, 523)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(100, 30)
        btnRefresh.TabIndex = 16
        btnRefresh.Text = "Refresh"
        btnRefresh.UseVisualStyleBackColor = False
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblStatus.Font = New Font("Segoe UI", 10.0F)
        lblStatus.ForeColor = Color.FromArgb(CByte(160), CByte(165), CByte(170))
        lblStatus.Location = New Point(80, 575)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(0, 19)
        lblStatus.TabIndex = 17
        ' 
        ' btnApply
        ' 
        btnApply.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        btnApply.FlatAppearance.BorderSize = 0
        btnApply.FlatStyle = FlatStyle.Flat
        btnApply.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        btnApply.ForeColor = Color.Black
        btnApply.Location = New Point(80, 625)
        btnApply.Name = "btnApply"
        btnApply.Size = New Size(140, 40)
        btnApply.TabIndex = 18
        btnApply.Text = "Apply"
        btnApply.UseVisualStyleBackColor = False
        ' 
        ' btnTest
        ' 
        btnTest.BackColor = Color.FromArgb(CByte(50), CByte(56), CByte(61))
        btnTest.FlatAppearance.BorderColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        btnTest.FlatStyle = FlatStyle.Flat
        btnTest.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        btnTest.ForeColor = Color.White
        btnTest.Location = New Point(240, 625)
        btnTest.Name = "btnTest"
        btnTest.Size = New Size(160, 40)
        btnTest.TabIndex = 19
        btnTest.Text = "Save && Test"
        btnTest.UseVisualStyleBackColor = False
        ' 
        ' action_fn — back button (same handler contract as Base_RecordingsSet)
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(50), CByte(56), CByte(61))
        action_fn.FlatAppearance.BorderColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.FlatStyle = FlatStyle.Flat
        action_fn.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(80, 100)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(100, 40)
        action_fn.TabIndex = 20
        action_fn.Text = "Back"
        action_fn.UseVisualStyleBackColor = False
        ' 
        ' Base_AudioSet
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Red
        ClientSize = New Size(1680, 945)
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
        CType(trkSystemVol, ComponentModel.ISupportInitialize).EndInit()
        CType(trkMicVol, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents setret As Panel
    Friend WithEvents text_settings As Label
    Friend WithEvents lblPageTitle As Label
    Friend WithEvents lblTrackMode As Label
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
    Friend WithEvents btnRefresh As Button
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnApply As Button
    Friend WithEvents btnTest As Button
    Friend WithEvents action_fn As Button
End Class
