<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class NVIDIA_Shadowplay_Helper
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(NVIDIA_Shadowplay_Helper))
        Panel_MAIN = New Panel()
        PictureBox10 = New PictureBox()
        Panel_About = New Panel()
        Label2 = New Label()
        PictureBox8 = New PictureBox()
        PictureBox7 = New PictureBox()
        Label3 = New Label()
        Use_Overlay = New ToggleSwitch()
        openoverlay = New Label()
        PictureBox2 = New PictureBox()
        PictureBox9 = New PictureBox()
        Label1 = New Label()
        overlay_text = New Label()
        API_OVERLAY = New CheckBox()
        overlay_game = New CheckBox()
        PictureBox1 = New PictureBox()
        PictureBox4 = New PictureBox()
        PictureBox6 = New PictureBox()
        NVAPI = New CheckBox()
        PictureBox3 = New PictureBox()
        BOX_LOGO = New PictureBox()
        RadioButton1 = New RadioButton()
        IF_APP = New Timer(components)
        PictureBox5 = New PictureBox()
        Timer1 = New Timer(components)
        RadioButton2 = New RadioButton()
        Panel_MAIN.SuspendLayout()
        CType(PictureBox10, ComponentModel.ISupportInitialize).BeginInit()
        Panel_About.SuspendLayout()
        CType(PictureBox8, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(BOX_LOGO, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Panel_MAIN
        ' 
        Panel_MAIN.BackColor = Color.Transparent
        Panel_MAIN.BackgroundImage = CType(resources.GetObject("Panel_MAIN.BackgroundImage"), Image)
        Panel_MAIN.BackgroundImageLayout = ImageLayout.Stretch
        Panel_MAIN.Controls.Add(PictureBox10)
        Panel_MAIN.Controls.Add(Panel_About)
        Panel_MAIN.Controls.Add(PictureBox8)
        Panel_MAIN.Controls.Add(PictureBox7)
        Panel_MAIN.Controls.Add(Label3)
        Panel_MAIN.Controls.Add(Use_Overlay)
        Panel_MAIN.Controls.Add(openoverlay)
        Panel_MAIN.Controls.Add(PictureBox2)
        Panel_MAIN.Controls.Add(PictureBox9)
        Panel_MAIN.Controls.Add(Label1)
        Panel_MAIN.Controls.Add(overlay_text)
        Panel_MAIN.Controls.Add(API_OVERLAY)
        Panel_MAIN.Controls.Add(overlay_game)
        Panel_MAIN.Controls.Add(PictureBox1)
        Panel_MAIN.Controls.Add(PictureBox4)
        Panel_MAIN.Controls.Add(PictureBox6)
        Panel_MAIN.Location = New Point(3, 41)
        Panel_MAIN.Name = "Panel_MAIN"
        Panel_MAIN.Size = New Size(1083, 585)
        Panel_MAIN.TabIndex = 1
        ' 
        ' PictureBox10
        ' 
        PictureBox10.BackColor = Color.Transparent
        PictureBox10.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox10.Location = New Point(3, 0)
        PictureBox10.Name = "PictureBox10"
        PictureBox10.Size = New Size(824, 10)
        PictureBox10.TabIndex = 61
        PictureBox10.TabStop = False
        ' 
        ' Panel_About
        ' 
        Panel_About.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        Panel_About.Controls.Add(Label2)
        Panel_About.Location = New Point(9, 3)
        Panel_About.Name = "Panel_About"
        Panel_About.Size = New Size(818, 158)
        Panel_About.TabIndex = 60
        ' 
        ' Label2
        ' 
        Label2.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        Label2.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.White
        Label2.Location = New Point(17, 18)
        Label2.Name = "Label2"
        Label2.Size = New Size(166, 23)
        Label2.TabIndex = 62
        Label2.Text = "About"
        ' 
        ' PictureBox8
        ' 
        PictureBox8.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        PictureBox8.BackgroundImage = CType(resources.GetObject("PictureBox8.BackgroundImage"), Image)
        PictureBox8.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox8.Location = New Point(833, 79)
        PictureBox8.Name = "PictureBox8"
        PictureBox8.Size = New Size(241, 10)
        PictureBox8.TabIndex = 59
        PictureBox8.TabStop = False
        ' 
        ' PictureBox7
        ' 
        PictureBox7.BackColor = Color.Transparent
        PictureBox7.BackgroundImage = CType(resources.GetObject("PictureBox7.BackgroundImage"), Image)
        PictureBox7.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox7.Location = New Point(9, 160)
        PictureBox7.Name = "PictureBox7"
        PictureBox7.Size = New Size(818, 19)
        PictureBox7.TabIndex = 57
        PictureBox7.TabStop = False
        ' 
        ' Label3
        ' 
        Label3.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        Label3.Cursor = Cursors.Hand
        Label3.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold)
        Label3.ForeColor = SystemColors.HighlightText
        Label3.Location = New Point(908, 47)
        Label3.Name = "Label3"
        Label3.Size = New Size(79, 17)
        Label3.TabIndex = 21
        Label3.Text = "OFF / ON"
        ' 
        ' Use_Overlay
        ' 
        Use_Overlay.BackColor = Color.Transparent
        Use_Overlay.IsOn = False
        Use_Overlay.Location = New Point(868, 47)
        Use_Overlay.Name = "Use_Overlay"
        Use_Overlay.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        Use_Overlay.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Use_Overlay.ShowGlow = False
        Use_Overlay.Size = New Size(34, 17)
        Use_Overlay.TabIndex = 20
        Use_Overlay.Text = "Use_Overlay"
        ' 
        ' openoverlay
        ' 
        openoverlay.AutoSize = True
        openoverlay.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        openoverlay.Cursor = Cursors.Hand
        openoverlay.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold)
        openoverlay.ForeColor = SystemColors.HighlightText
        openoverlay.Location = New Point(993, 92)
        openoverlay.Name = "openoverlay"
        openoverlay.Size = New Size(49, 15)
        openoverlay.TabIndex = 19
        openoverlay.Text = "[Open]"
        openoverlay.Visible = False
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Color.Transparent
        PictureBox2.BackgroundImage = CType(resources.GetObject("PictureBox2.BackgroundImage"), Image)
        PictureBox2.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox2.Location = New Point(922, 0)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(152, 10)
        PictureBox2.TabIndex = 5
        PictureBox2.TabStop = False
        ' 
        ' PictureBox9
        ' 
        PictureBox9.BackColor = Color.Transparent
        PictureBox9.BackgroundImage = CType(resources.GetObject("PictureBox9.BackgroundImage"), Image)
        PictureBox9.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox9.Location = New Point(833, 0)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(222, 10)
        PictureBox9.TabIndex = 16
        PictureBox9.TabStop = False
        ' 
        ' Label1
        ' 
        Label1.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        Label1.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(833, 141)
        Label1.Name = "Label1"
        Label1.Size = New Size(241, 20)
        Label1.TabIndex = 11
        Label1.Text = "Press Alt + Z to use in-game overlay"
        Label1.TextAlign = ContentAlignment.TopCenter
        ' 
        ' overlay_text
        ' 
        overlay_text.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        overlay_text.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        overlay_text.ForeColor = Color.White
        overlay_text.Location = New Point(849, 21)
        overlay_text.Name = "overlay_text"
        overlay_text.Size = New Size(166, 23)
        overlay_text.TabIndex = 8
        overlay_text.Text = "ShadowPlay"
        ' 
        ' API_OVERLAY
        ' 
        API_OVERLAY.AutoCheck = False
        API_OVERLAY.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        API_OVERLAY.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        API_OVERLAY.ForeColor = Color.White
        API_OVERLAY.Location = New Point(847, 116)
        API_OVERLAY.Name = "API_OVERLAY"
        API_OVERLAY.Size = New Size(140, 19)
        API_OVERLAY.TabIndex = 7
        API_OVERLAY.Text = "Notifier API"
        API_OVERLAY.UseVisualStyleBackColor = False
        ' 
        ' overlay_game
        ' 
        overlay_game.AutoCheck = False
        overlay_game.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        overlay_game.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        overlay_game.ForeColor = Color.White
        overlay_game.Location = New Point(847, 91)
        overlay_game.Name = "overlay_game"
        overlay_game.Size = New Size(140, 19)
        overlay_game.TabIndex = 6
        overlay_game.Text = "IN-GAME OVERLAY"
        overlay_game.UseVisualStyleBackColor = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        PictureBox1.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox1.Location = New Point(833, 3)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(241, 76)
        PictureBox1.TabIndex = 4
        PictureBox1.TabStop = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        PictureBox4.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox4.Location = New Point(833, 73)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(241, 88)
        PictureBox4.TabIndex = 58
        PictureBox4.TabStop = False
        ' 
        ' PictureBox6
        ' 
        PictureBox6.BackColor = Color.Transparent
        PictureBox6.BackgroundImage = CType(resources.GetObject("PictureBox6.BackgroundImage"), Image)
        PictureBox6.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox6.Location = New Point(833, 160)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(241, 19)
        PictureBox6.TabIndex = 12
        PictureBox6.TabStop = False
        ' 
        ' NVAPI
        ' 
        NVAPI.AutoCheck = False
        NVAPI.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        NVAPI.CheckAlign = ContentAlignment.MiddleRight
        NVAPI.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        NVAPI.ForeColor = Color.White
        NVAPI.Location = New Point(911, 12)
        NVAPI.Name = "NVAPI"
        NVAPI.Size = New Size(97, 19)
        NVAPI.TabIndex = 11
        NVAPI.Text = "NVIDIA API"
        NVAPI.TextAlign = ContentAlignment.MiddleRight
        NVAPI.UseVisualStyleBackColor = False
        ' 
        ' PictureBox3
        ' 
        PictureBox3.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        PictureBox3.BackgroundImage = CType(resources.GetObject("PictureBox3.BackgroundImage"), Image)
        PictureBox3.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox3.Location = New Point(3, 3)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(88, 35)
        PictureBox3.TabIndex = 4
        PictureBox3.TabStop = False
        PictureBox3.Visible = False
        ' 
        ' BOX_LOGO
        ' 
        BOX_LOGO.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        BOX_LOGO.BackgroundImageLayout = ImageLayout.Center
        BOX_LOGO.Location = New Point(3, 3)
        BOX_LOGO.Name = "BOX_LOGO"
        BOX_LOGO.Size = New Size(1083, 35)
        BOX_LOGO.TabIndex = 2
        BOX_LOGO.TabStop = False
        ' 
        ' RadioButton1
        ' 
        RadioButton1.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        RadioButton1.Checked = True
        RadioButton1.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        RadioButton1.ForeColor = SystemColors.Control
        RadioButton1.Location = New Point(1020, 3)
        RadioButton1.Name = "RadioButton1"
        RadioButton1.RightToLeft = RightToLeft.Yes
        RadioButton1.Size = New Size(57, 35)
        RadioButton1.TabIndex = 3
        RadioButton1.TabStop = True
        RadioButton1.Text = "Exit"
        RadioButton1.UseVisualStyleBackColor = False
        ' 
        ' IF_APP
        ' 
        IF_APP.Enabled = True
        ' 
        ' PictureBox5
        ' 
        PictureBox5.BackColor = Color.FromArgb(CByte(25), CByte(25), CByte(25))
        PictureBox5.Location = New Point(-4, -7)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New Size(1097, 10)
        PictureBox5.TabIndex = 11
        PictureBox5.TabStop = False
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 10
        ' 
        ' RadioButton2
        ' 
        RadioButton2.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        RadioButton2.Checked = True
        RadioButton2.Font = New Font("Segoe UI Black", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        RadioButton2.ForeColor = SystemColors.Control
        RadioButton2.Location = New Point(12, 3)
        RadioButton2.Name = "RadioButton2"
        RadioButton2.Size = New Size(141, 35)
        RadioButton2.TabIndex = 55
        RadioButton2.TabStop = True
        RadioButton2.Text = "installer Mode"
        RadioButton2.UseVisualStyleBackColor = False
        ' 
        ' NVIDIA_Shadowplay_Helper
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        BackColor = Color.FromArgb(CByte(25), CByte(25), CByte(25))
        ClientSize = New Size(1089, 629)
        Controls.Add(RadioButton2)
        Controls.Add(PictureBox3)
        Controls.Add(PictureBox5)
        Controls.Add(RadioButton1)
        Controls.Add(NVAPI)
        Controls.Add(BOX_LOGO)
        Controls.Add(Panel_MAIN)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "NVIDIA_Shadowplay_Helper"
        StartPosition = FormStartPosition.CenterScreen
        Text = "NVIDIA Shadowplay Helper"
        Panel_MAIN.ResumeLayout(False)
        Panel_MAIN.PerformLayout()
        CType(PictureBox10, ComponentModel.ISupportInitialize).EndInit()
        Panel_About.ResumeLayout(False)
        CType(PictureBox8, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(BOX_LOGO, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents Panel_MAIN As Panel
    Friend WithEvents BOX_LOGO As PictureBox
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents overlay_game As CheckBox
    Friend WithEvents IF_APP As Timer
    Friend WithEvents overlay_text As Label
    Friend WithEvents API_OVERLAY As CheckBox
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents NVAPI As CheckBox
    Friend WithEvents Label1 As Label
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents Timer1 As Timer
    Friend WithEvents PictureBox9 As PictureBox
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents openoverlay As Label
    Friend WithEvents Use_Overlay As ToggleSwitch
    Friend WithEvents Label3 As Label
    Friend WithEvents PictureBox7 As PictureBox
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents PictureBox8 As PictureBox
    Friend WithEvents Panel_About As Panel
    Friend WithEvents PictureBox10 As PictureBox
    Friend WithEvents Label2 As Label

End Class
