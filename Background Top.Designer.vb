<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class bg_top
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(bg_top))
        Main_Top = New Panel()
        cpuLabel = New Label()
        Logo = New PictureBox()
        top_logo = New PictureBox()
        gfe = New Label()
        ac1 = New Panel()
        record_sc = New Panel()
        Label10 = New Label()
        Label11 = New Label()
        sh_record = New Label()
        Label13 = New Label()
        Label14 = New Label()
        PictureBox5 = New PictureBox()
        PictureBox6 = New PictureBox()
        replay_sc_all = New Panel()
        Label7 = New Label()
        Label16 = New Label()
        Label8 = New Label()
        sh_replay = New Label()
        if_replay = New Label()
        icon_replay = New Label()
        replay_sc = New PictureBox()
        replay_sc1 = New PictureBox()
        b3 = New Label()
        b2 = New Label()
        b1 = New Label()
        tpsLabel = New Label()
        Load = New Timer(components)
        Main_Top.SuspendLayout()
        CType(Logo, ComponentModel.ISupportInitialize).BeginInit()
        CType(top_logo, ComponentModel.ISupportInitialize).BeginInit()
        ac1.SuspendLayout()
        record_sc.SuspendLayout()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        replay_sc_all.SuspendLayout()
        CType(replay_sc, ComponentModel.ISupportInitialize).BeginInit()
        CType(replay_sc1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Main_Top
        ' 
        Main_Top.Controls.Add(cpuLabel)
        Main_Top.Controls.Add(Logo)
        Main_Top.Controls.Add(top_logo)
        Main_Top.Controls.Add(gfe)
        Main_Top.Dock = DockStyle.Top
        Main_Top.Location = New System.Drawing.Point(0, 0)
        Main_Top.Name = "Main_Top"
        Main_Top.Size = New System.Drawing.Size(834, 80)
        Main_Top.TabIndex = 10
        ' 
        ' cpuLabel
        ' 
        cpuLabel.BackColor = Drawing.Color.Black
        cpuLabel.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Bold)
        cpuLabel.ForeColor = Drawing.Color.White
        cpuLabel.Location = New System.Drawing.Point(68, 27)
        cpuLabel.Name = "cpuLabel"
        cpuLabel.Size = New System.Drawing.Size(158, 23)
        cpuLabel.TabIndex = 47
        cpuLabel.Text = "CPU :"
        cpuLabel.TextAlign = Drawing.ContentAlignment.MiddleLeft
        cpuLabel.Visible = False
        ' 
        ' Logo
        ' 
        Logo.BackgroundImage = CType(resources.GetObject("Logo.BackgroundImage"), Drawing.Image)
        Logo.BackgroundImageLayout = ImageLayout.None
        Logo.Location = New System.Drawing.Point(10, 10)
        Logo.Name = "Logo"
        Logo.Size = New System.Drawing.Size(58, 58)
        Logo.TabIndex = 6
        Logo.TabStop = False
        ' 
        ' top_logo
        ' 
        top_logo.BackColor = Drawing.Color.Black
        top_logo.Location = New System.Drawing.Point(0, 62)
        top_logo.Name = "top_logo"
        top_logo.Size = New System.Drawing.Size(5500, 43)
        top_logo.TabIndex = 13
        top_logo.TabStop = False
        ' 
        ' gfe
        ' 
        gfe.BackColor = Drawing.Color.Black
        gfe.Dock = DockStyle.Top
        gfe.Font = New System.Drawing.Font("Segoe UI", 20F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        gfe.ForeColor = Drawing.Color.White
        gfe.Location = New System.Drawing.Point(0, 0)
        gfe.Name = "gfe"
        gfe.Size = New System.Drawing.Size(834, 75)
        gfe.TabIndex = 8
        gfe.Text = "Shadowplay Experience"
        gfe.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' ac1
        ' 
        ac1.Controls.Add(record_sc)
        ac1.Controls.Add(replay_sc_all)
        ac1.Controls.Add(b3)
        ac1.Controls.Add(b2)
        ac1.Controls.Add(b1)
        ac1.Location = New System.Drawing.Point(68, 111)
        ac1.Name = "ac1"
        ac1.Size = New System.Drawing.Size(600, 300)
        ac1.TabIndex = 45
        ' 
        ' record_sc
        ' 
        record_sc.Controls.Add(Label10)
        record_sc.Controls.Add(Label11)
        record_sc.Controls.Add(sh_record)
        record_sc.Controls.Add(Label13)
        record_sc.Controls.Add(Label14)
        record_sc.Controls.Add(PictureBox5)
        record_sc.Controls.Add(PictureBox6)
        record_sc.Location = New System.Drawing.Point(200, 200)
        record_sc.Name = "record_sc"
        record_sc.Size = New System.Drawing.Size(200, 100)
        record_sc.TabIndex = 44
        record_sc.Visible = False
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.BackColor = Drawing.Color.Black
        Label10.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label10.ForeColor = Drawing.Color.Gray
        Label10.Location = New System.Drawing.Point(26, 36)
        Label10.Name = "Label10"
        Label10.Size = New System.Drawing.Size(58, 19)
        Label10.TabIndex = 45
        Label10.Text = "Settings"
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.BackColor = Drawing.Color.Black
        Label11.Font = New System.Drawing.Font("nvgcshare", 15F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label11.ForeColor = Drawing.Color.Gray
        Label11.Location = New System.Drawing.Point(3, 35)
        Label11.Name = "Label11"
        Label11.RightToLeft = RightToLeft.Yes
        Label11.Size = New System.Drawing.Size(29, 20)
        Label11.TabIndex = 46
        Label11.Text = ""
        ' 
        ' sh_record
        ' 
        sh_record.AutoSize = True
        sh_record.BackColor = Drawing.Color.Black
        sh_record.Font = New System.Drawing.Font("Segoe UI", 8F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        sh_record.ForeColor = Drawing.Color.Gray
        sh_record.Location = New System.Drawing.Point(156, 9)
        sh_record.Name = "sh_record"
        sh_record.Size = New System.Drawing.Size(41, 13)
        sh_record.TabIndex = 41
        sh_record.Text = "Alt+F9"
        ' 
        ' Label13
        ' 
        Label13.AutoSize = True
        Label13.BackColor = Drawing.Color.Black
        Label13.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label13.ForeColor = Drawing.Color.White
        Label13.Location = New System.Drawing.Point(26, 6)
        Label13.Name = "Label13"
        Label13.Size = New System.Drawing.Size(38, 19)
        Label13.TabIndex = 41
        Label13.Text = "Start"
        ' 
        ' Label14
        ' 
        Label14.AutoSize = True
        Label14.BackColor = Drawing.Color.Black
        Label14.Font = New System.Drawing.Font("nvgcshare", 15F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label14.ForeColor = Drawing.Color.White
        Label14.Location = New System.Drawing.Point(3, 5)
        Label14.Name = "Label14"
        Label14.RightToLeft = RightToLeft.Yes
        Label14.Size = New System.Drawing.Size(29, 20)
        Label14.TabIndex = 44
        Label14.Text = ""
        ' 
        ' PictureBox5
        ' 
        PictureBox5.BackColor = Drawing.Color.Black
        PictureBox5.Location = New System.Drawing.Point(0, 0)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New System.Drawing.Size(200, 30)
        PictureBox5.TabIndex = 0
        PictureBox5.TabStop = False
        ' 
        ' PictureBox6
        ' 
        PictureBox6.BackColor = Drawing.Color.Black
        PictureBox6.Location = New System.Drawing.Point(0, 30)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New System.Drawing.Size(200, 30)
        PictureBox6.TabIndex = 42
        PictureBox6.TabStop = False
        ' 
        ' replay_sc_all
        ' 
        replay_sc_all.Controls.Add(Label7)
        replay_sc_all.Controls.Add(Label16)
        replay_sc_all.Controls.Add(Label8)
        replay_sc_all.Controls.Add(sh_replay)
        replay_sc_all.Controls.Add(if_replay)
        replay_sc_all.Controls.Add(icon_replay)
        replay_sc_all.Controls.Add(replay_sc)
        replay_sc_all.Controls.Add(replay_sc1)
        replay_sc_all.Location = New System.Drawing.Point(0, 200)
        replay_sc_all.Name = "replay_sc_all"
        replay_sc_all.Size = New System.Drawing.Size(200, 100)
        replay_sc_all.TabIndex = 43
        replay_sc_all.Visible = False
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.BackColor = Drawing.Color.Black
        Label7.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label7.ForeColor = Drawing.Color.White
        Label7.Location = New System.Drawing.Point(26, 36)
        Label7.Name = "Label7"
        Label7.Size = New System.Drawing.Size(45, 19)
        Label7.TabIndex = 45
        Label7.Text = "Saved"
        ' 
        ' Label16
        ' 
        Label16.AutoSize = True
        Label16.BackColor = Drawing.Color.Black
        Label16.Font = New System.Drawing.Font("Segoe UI", 8F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label16.ForeColor = Drawing.Color.Gray
        Label16.Location = New System.Drawing.Point(150, 39)
        Label16.Name = "Label16"
        Label16.Size = New System.Drawing.Size(47, 13)
        Label16.TabIndex = 50
        Label16.Text = "Alt+F10"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.BackColor = Drawing.Color.Black
        Label8.Font = New System.Drawing.Font("nvgcshare", 15F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label8.ForeColor = Drawing.Color.White
        Label8.Location = New System.Drawing.Point(3, 35)
        Label8.Name = "Label8"
        Label8.RightToLeft = RightToLeft.Yes
        Label8.Size = New System.Drawing.Size(29, 20)
        Label8.TabIndex = 46
        Label8.Text = ""
        ' 
        ' sh_replay
        ' 
        sh_replay.AutoSize = True
        sh_replay.BackColor = Drawing.Color.Black
        sh_replay.Font = New System.Drawing.Font("Segoe UI", 8F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        sh_replay.ForeColor = Drawing.Color.Gray
        sh_replay.Location = New System.Drawing.Point(118, 9)
        sh_replay.Name = "sh_replay"
        sh_replay.Size = New System.Drawing.Size(79, 13)
        sh_replay.TabIndex = 41
        sh_replay.Text = "Alt+Shift+F10"
        ' 
        ' if_replay
        ' 
        if_replay.AutoSize = True
        if_replay.BackColor = Drawing.Color.Black
        if_replay.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        if_replay.ForeColor = Drawing.Color.White
        if_replay.Location = New System.Drawing.Point(26, 6)
        if_replay.Name = "if_replay"
        if_replay.Size = New System.Drawing.Size(57, 19)
        if_replay.TabIndex = 41
        if_replay.Text = "Turn on"
        ' 
        ' icon_replay
        ' 
        icon_replay.AutoSize = True
        icon_replay.BackColor = Drawing.Color.Black
        icon_replay.Font = New System.Drawing.Font("nvgcshare", 15F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        icon_replay.ForeColor = Drawing.Color.White
        icon_replay.Location = New System.Drawing.Point(3, 5)
        icon_replay.Name = "icon_replay"
        icon_replay.RightToLeft = RightToLeft.Yes
        icon_replay.Size = New System.Drawing.Size(29, 20)
        icon_replay.TabIndex = 44
        icon_replay.Text = ""
        ' 
        ' replay_sc
        ' 
        replay_sc.BackColor = Drawing.Color.Black
        replay_sc.Location = New System.Drawing.Point(0, 0)
        replay_sc.Name = "replay_sc"
        replay_sc.Size = New System.Drawing.Size(200, 30)
        replay_sc.TabIndex = 0
        replay_sc.TabStop = False
        ' 
        ' replay_sc1
        ' 
        replay_sc1.BackColor = Drawing.Color.Black
        replay_sc1.Location = New System.Drawing.Point(0, 30)
        replay_sc1.Name = "replay_sc1"
        replay_sc1.Size = New System.Drawing.Size(200, 30)
        replay_sc1.TabIndex = 42
        replay_sc1.TabStop = False
        ' 
        ' b3
        ' 
        b3.BackColor = Drawing.Color.Black
        b3.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        b3.ForeColor = Drawing.Color.White
        b3.ImageAlign = Drawing.ContentAlignment.TopCenter
        b3.Location = New System.Drawing.Point(400, 0)
        b3.Name = "b3"
        b3.Size = New System.Drawing.Size(200, 200)
        b3.TabIndex = 41
        b3.TextAlign = Drawing.ContentAlignment.MiddleCenter
        b3.Visible = False
        ' 
        ' b2
        ' 
        b2.BackColor = Drawing.Color.Black
        b2.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        b2.ForeColor = Drawing.Color.Gray
        b2.ImageAlign = Drawing.ContentAlignment.TopCenter
        b2.Location = New System.Drawing.Point(200, 0)
        b2.Name = "b2"
        b2.Size = New System.Drawing.Size(200, 200)
        b2.TabIndex = 41
        b2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        b2.Visible = False
        ' 
        ' b1
        ' 
        b1.BackColor = Drawing.Color.Black
        b1.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        b1.ForeColor = Drawing.Color.Green
        b1.ImageAlign = Drawing.ContentAlignment.TopCenter
        b1.Location = New System.Drawing.Point(0, 0)
        b1.Name = "b1"
        b1.Size = New System.Drawing.Size(200, 200)
        b1.TabIndex = 41
        b1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        b1.Visible = False
        ' 
        ' tpsLabel
        ' 
        tpsLabel.BackColor = Drawing.Color.Black
        tpsLabel.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Bold)
        tpsLabel.ForeColor = Drawing.Color.White
        tpsLabel.Location = New System.Drawing.Point(68, 5)
        tpsLabel.Name = "tpsLabel"
        tpsLabel.Size = New System.Drawing.Size(158, 23)
        tpsLabel.TabIndex = 46
        tpsLabel.Text = "FPS :"
        tpsLabel.TextAlign = Drawing.ContentAlignment.MiddleLeft
        tpsLabel.Visible = False
        ' 
        ' Load
        ' 
        Load.Enabled = True
        Load.Interval = 1
        ' 
        ' bg_top
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Blue
        ClientSize = New System.Drawing.Size(834, 502)
        ControlBox = False
        Controls.Add(tpsLabel)
        Controls.Add(ac1)
        Controls.Add(Main_Top)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "bg_top"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Background Top"
        TopMost = True
        TransparencyKey = Drawing.Color.Blue
        WindowState = FormWindowState.Maximized
        Main_Top.ResumeLayout(False)
        CType(Logo, ComponentModel.ISupportInitialize).EndInit()
        CType(top_logo, ComponentModel.ISupportInitialize).EndInit()
        ac1.ResumeLayout(False)
        record_sc.ResumeLayout(False)
        record_sc.PerformLayout()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        replay_sc_all.ResumeLayout(False)
        replay_sc_all.PerformLayout()
        CType(replay_sc, ComponentModel.ISupportInitialize).EndInit()
        CType(replay_sc1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Main_Top As Panel
    Friend WithEvents Logo As PictureBox
    Friend WithEvents top_logo As PictureBox
    Friend WithEvents gfe As Label
    Friend WithEvents ac1 As Panel
    Friend WithEvents record_sc As Panel
    Friend WithEvents Label10 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents sh_record As Label
    Friend WithEvents Label13 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents replay_sc_all As Panel
    Friend WithEvents Label7 As Label
    Friend WithEvents Label16 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents sh_replay As Label
    Friend WithEvents if_replay As Label
    Friend WithEvents icon_replay As Label
    Friend WithEvents replay_sc As PictureBox
    Friend WithEvents replay_sc1 As PictureBox
    Friend WithEvents b3 As Label
    Friend WithEvents b2 As Label
    Friend WithEvents b1 As Label
    Friend WithEvents tpsLabel As Label
    Friend WithEvents cpuLabel As Label
    Friend WithEvents Load As Timer
End Class
