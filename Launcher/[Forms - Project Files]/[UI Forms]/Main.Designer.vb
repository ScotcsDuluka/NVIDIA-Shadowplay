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
        RadioButton2 = New NvButton()
        openoverlay = New NvButton()
        Use_Overlay = New NvToggleButton()
        NvIconButton2 = New NvIconButton()
        Label4 = New Label()
        PictureBox2 = New PictureBox()
        NvStatusDot_NVNOTIFIER = New NvStatusDot()
        NvStatusDot_OVERLAYAPI = New NvStatusDot()
        NvStatusDot_NVAPI = New NvStatusDot()
        Label5 = New Label()
        overlay_text = New Label()
        NVAPI = New CheckBox()
        PictureBox8 = New PictureBox()
        Label1 = New Label()
        API_OVERLAY = New CheckBox()
        overlay_game = New CheckBox()
        PictureBox1 = New PictureBox()
        PictureBox4 = New PictureBox()
        NvIconButton1 = New NvIconButton()
        PictureBox3 = New PictureBox()
        BOX_LOGO = New PictureBox()
        IF_APP = New Timer(components)
        Timer1 = New Timer(components)
        Label2 = New Label()
        RadioButton1 = New PictureBox()
        Panel_MAIN.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox8, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(BOX_LOGO, ComponentModel.ISupportInitialize).BeginInit()
        CType(RadioButton1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Panel_MAIN
        ' 
        Panel_MAIN.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel_MAIN.BackColor = Color.FromArgb(CByte(26), CByte(26), CByte(26))
        Panel_MAIN.BackgroundImageLayout = ImageLayout.Stretch
        Panel_MAIN.Controls.Add(RadioButton2)
        Panel_MAIN.Controls.Add(openoverlay)
        Panel_MAIN.Controls.Add(Use_Overlay)
        Panel_MAIN.Controls.Add(NvIconButton2)
        Panel_MAIN.Controls.Add(Label4)
        Panel_MAIN.Controls.Add(PictureBox2)
        Panel_MAIN.Controls.Add(NvStatusDot_NVNOTIFIER)
        Panel_MAIN.Controls.Add(NvStatusDot_OVERLAYAPI)
        Panel_MAIN.Controls.Add(NvStatusDot_NVAPI)
        Panel_MAIN.Controls.Add(Label5)
        Panel_MAIN.Controls.Add(overlay_text)
        Panel_MAIN.Controls.Add(NVAPI)
        Panel_MAIN.Controls.Add(PictureBox8)
        Panel_MAIN.Controls.Add(Label1)
        Panel_MAIN.Controls.Add(API_OVERLAY)
        Panel_MAIN.Controls.Add(overlay_game)
        Panel_MAIN.Controls.Add(PictureBox1)
        Panel_MAIN.Controls.Add(PictureBox4)
        Panel_MAIN.Controls.Add(NvIconButton1)
        Panel_MAIN.Controls.Add(PictureBox3)
        Panel_MAIN.Location = New Point(3, 41)
        Panel_MAIN.Name = "Panel_MAIN"
        Panel_MAIN.Size = New Size(1083, 267)
        Panel_MAIN.TabIndex = 1
        ' 
        ' RadioButton2
        ' 
        RadioButton2.BackColor = Color.FromArgb(CByte(50), CByte(50), CByte(50))
        RadioButton2.ButtonImage = Nothing
        RadioButton2.ButtonImageSize = New Size(16, 16)
        RadioButton2.ButtonSizeSetting = NvButton.NvBtnSize.Small
        RadioButton2.ButtonVariant = NvButton.NvButtonVariant.Green
        RadioButton2.CornerRadius = 12
        RadioButton2.Font = New Font("Bahnschrift", 12.0F)
        RadioButton2.ForeColor = Color.Snow
        RadioButton2.Location = New Point(47, 196)
        RadioButton2.Name = "RadioButton2"
        RadioButton2.Size = New Size(139, 32)
        RadioButton2.TabIndex = 68
        RadioButton2.Text = "Installer Mode"
        ' 
        ' openoverlay
        ' 
        openoverlay.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        openoverlay.ButtonImage = Nothing
        openoverlay.ButtonImageSize = New Size(16, 16)
        openoverlay.ButtonSizeSetting = NvButton.NvBtnSize.Large
        openoverlay.ButtonVariant = NvButton.NvButtonVariant.Surface
        openoverlay.CornerRadius = 8
        openoverlay.Font = New Font("Bahnschrift", 10.0F, FontStyle.Bold)
        openoverlay.Location = New Point(929, 140)
        openoverlay.Name = "openoverlay"
        openoverlay.Size = New Size(142, 27)
        openoverlay.TabIndex = 81
        openoverlay.Text = "Open Overlay"
        ' 
        ' Use_Overlay
        ' 
        Use_Overlay.BackColor = Color.FromArgb(CByte(50), CByte(50), CByte(50))
        Use_Overlay.IsOn = False
        Use_Overlay.Location = New Point(747, 201)
        Use_Overlay.Name = "Use_Overlay"
        Use_Overlay.OffColor = Color.FromArgb(CByte(74), CByte(74), CByte(74))
        Use_Overlay.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Use_Overlay.ShowGlow = False
        Use_Overlay.Size = New Size(36, 20)
        Use_Overlay.SizeMode = NvToggleButton.ToggleSizeMode.Small
        Use_Overlay.TabIndex = 77
        Use_Overlay.Text = "NvToggleButton1"
        ' 
        ' NvIconButton2
        ' 
        NvIconButton2.BackColor = Color.FromArgb(CByte(50), CByte(50), CByte(50))
        NvIconButton2.ButtonVariant = NvIconButton.IconVariant.Ghost
        NvIconButton2.CornerRadius = 18
        NvIconButton2.Enabled = False
        NvIconButton2.IconImage = Nothing
        NvIconButton2.IsActive = True
        NvIconButton2.Location = New Point(24, 189)
        NvIconButton2.Name = "NvIconButton2"
        NvIconButton2.Size = New Size(785, 45)
        NvIconButton2.TabIndex = 84
        NvIconButton2.Text = "NvIconButton2"
        ' 
        ' Label4
        ' 
        Label4.BackColor = Color.FromArgb(CByte(42), CByte(42), CByte(42))
        Label4.Font = New Font("MS Reference Sans Serif", 10.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Color.FromArgb(CByte(102), CByte(102), CByte(102))
        Label4.Location = New Point(74, 95)
        Label4.Name = "Label4"
        Label4.Size = New Size(555, 91)
        Label4.TabIndex = 61
        Label4.Text = resources.GetString("Label4.Text")
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Color.FromArgb(CByte(42), CByte(42), CByte(42))
        PictureBox2.BackgroundImage = CType(resources.GetObject("PictureBox2.BackgroundImage"), Image)
        PictureBox2.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox2.Location = New Point(584, 59)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(211, 135)
        PictureBox2.TabIndex = 82
        PictureBox2.TabStop = False
        ' 
        ' NvStatusDot_NVNOTIFIER
        ' 
        NvStatusDot_NVNOTIFIER.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        NvStatusDot_NVNOTIFIER.DotSize = 8
        NvStatusDot_NVNOTIFIER.Location = New Point(855, 104)
        NvStatusDot_NVNOTIFIER.Name = "NvStatusDot_NVNOTIFIER"
        NvStatusDot_NVNOTIFIER.Size = New Size(25, 21)
        NvStatusDot_NVNOTIFIER.Status = NvStatusDot.DotStatus.Stopped
        NvStatusDot_NVNOTIFIER.TabIndex = 80
        NvStatusDot_NVNOTIFIER.Text = "NvStatusDot1"
        ' 
        ' NvStatusDot_OVERLAYAPI
        ' 
        NvStatusDot_OVERLAYAPI.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        NvStatusDot_OVERLAYAPI.DotSize = 8
        NvStatusDot_OVERLAYAPI.Location = New Point(855, 77)
        NvStatusDot_OVERLAYAPI.Name = "NvStatusDot_OVERLAYAPI"
        NvStatusDot_OVERLAYAPI.Size = New Size(25, 21)
        NvStatusDot_OVERLAYAPI.Status = NvStatusDot.DotStatus.Stopped
        NvStatusDot_OVERLAYAPI.TabIndex = 79
        NvStatusDot_OVERLAYAPI.Text = "NvStatusDot1"
        ' 
        ' NvStatusDot_NVAPI
        ' 
        NvStatusDot_NVAPI.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        NvStatusDot_NVAPI.DotSize = 8
        NvStatusDot_NVAPI.Location = New Point(855, 50)
        NvStatusDot_NVAPI.Name = "NvStatusDot_NVAPI"
        NvStatusDot_NVAPI.Size = New Size(25, 21)
        NvStatusDot_NVAPI.Status = NvStatusDot.DotStatus.Stopped
        NvStatusDot_NVAPI.TabIndex = 78
        NvStatusDot_NVAPI.Text = "NvStatusDot1"
        ' 
        ' Label5
        ' 
        Label5.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        Label5.Font = New Font("Sans Serif Collection", 8.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Label5.Location = New Point(833, 140)
        Label5.Name = "Label5"
        Label5.Size = New Size(250, 37)
        Label5.TabIndex = 62
        Label5.Text = "  F E A T U R E S"
        ' 
        ' overlay_text
        ' 
        overlay_text.BackColor = Color.FromArgb(CByte(42), CByte(42), CByte(42))
        overlay_text.Font = New Font("Nirmala UI", 15.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        overlay_text.ForeColor = Color.FromArgb(CByte(232), CByte(232), CByte(232))
        overlay_text.Location = New Point(47, 61)
        overlay_text.Name = "overlay_text"
        overlay_text.Size = New Size(272, 34)
        overlay_text.TabIndex = 8
        overlay_text.Text = "NVIDIA ShadowPlay"
        overlay_text.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' NVAPI
        ' 
        NVAPI.AutoCheck = False
        NVAPI.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        NVAPI.FlatStyle = FlatStyle.Flat
        NVAPI.Font = New Font("Bahnschrift", 9.0F, FontStyle.Bold)
        NVAPI.ForeColor = Color.White
        NVAPI.Location = New Point(865, 52)
        NVAPI.Name = "NVAPI"
        NVAPI.Size = New Size(140, 19)
        NVAPI.TabIndex = 11
        NVAPI.Text = "N V I D I A  A P I"
        NVAPI.UseVisualStyleBackColor = False
        ' 
        ' PictureBox8
        ' 
        PictureBox8.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        PictureBox8.BackgroundImage = CType(resources.GetObject("PictureBox8.BackgroundImage"), Image)
        PictureBox8.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox8.Location = New Point(833, 28)
        PictureBox8.Name = "PictureBox8"
        PictureBox8.Size = New Size(264, 10)
        PictureBox8.TabIndex = 59
        PictureBox8.TabStop = False
        ' 
        ' Label1
        ' 
        Label1.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        Label1.Font = New Font("Sans Serif Collection", 8.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.FromArgb(CByte(102), CByte(102), CByte(102))
        Label1.Location = New Point(833, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(241, 29)
        Label1.TabIndex = 11
        Label1.Text = "  S E R V I C E S"
        ' 
        ' API_OVERLAY
        ' 
        API_OVERLAY.AutoCheck = False
        API_OVERLAY.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        API_OVERLAY.FlatStyle = FlatStyle.Flat
        API_OVERLAY.Font = New Font("Bahnschrift", 9.0F, FontStyle.Bold)
        API_OVERLAY.ForeColor = Color.White
        API_OVERLAY.Location = New Point(865, 106)
        API_OVERLAY.Name = "API_OVERLAY"
        API_OVERLAY.Size = New Size(187, 19)
        API_OVERLAY.TabIndex = 7
        API_OVERLAY.Text = "N O T I F I E R  A P I"
        API_OVERLAY.UseVisualStyleBackColor = False
        ' 
        ' overlay_game
        ' 
        overlay_game.AutoCheck = False
        overlay_game.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        overlay_game.FlatStyle = FlatStyle.Flat
        overlay_game.Font = New Font("Bahnschrift", 9.0F, FontStyle.Bold)
        overlay_game.ForeColor = Color.White
        overlay_game.Location = New Point(865, 79)
        overlay_game.Name = "overlay_game"
        overlay_game.Size = New Size(185, 19)
        overlay_game.TabIndex = 6
        overlay_game.Text = "O V E R L A Y  A P I"
        overlay_game.UseVisualStyleBackColor = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.FromArgb(CByte(18), CByte(18), CByte(18))
        PictureBox1.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox1.Location = New Point(833, 0)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(271, 34)
        PictureBox1.TabIndex = 4
        PictureBox1.TabStop = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        PictureBox4.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox4.Location = New Point(833, 32)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(264, 564)
        PictureBox4.TabIndex = 58
        PictureBox4.TabStop = False
        ' 
        ' NvIconButton1
        ' 
        NvIconButton1.BackColor = Color.Transparent
        NvIconButton1.ButtonVariant = NvIconButton.IconVariant.Surface
        NvIconButton1.CornerRadius = 18
        NvIconButton1.Enabled = False
        NvIconButton1.IconImage = Nothing
        NvIconButton1.IsActive = False
        NvIconButton1.Location = New Point(24, 34)
        NvIconButton1.Name = "NvIconButton1"
        NvIconButton1.Size = New Size(785, 193)
        NvIconButton1.TabIndex = 83
        NvIconButton1.Text = "NvIconButton1"
        ' 
        ' PictureBox3
        ' 
        PictureBox3.BackColor = Color.FromArgb(CByte(26), CByte(26), CByte(26))
        PictureBox3.BackgroundImage = CType(resources.GetObject("PictureBox3.BackgroundImage"), Image)
        PictureBox3.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox3.Location = New Point(24, 233)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(785, 34)
        PictureBox3.TabIndex = 85
        PictureBox3.TabStop = False
        ' 
        ' BOX_LOGO
        ' 
        BOX_LOGO.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        BOX_LOGO.BackgroundImageLayout = ImageLayout.Center
        BOX_LOGO.Location = New Point(-12, 0)
        BOX_LOGO.Name = "BOX_LOGO"
        BOX_LOGO.Size = New Size(1112, 38)
        BOX_LOGO.TabIndex = 2
        BOX_LOGO.TabStop = False
        ' 
        ' IF_APP
        ' 
        IF_APP.Enabled = True
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 10
        ' 
        ' Label2
        ' 
        Label2.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        Label2.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.FromArgb(CByte(232), CByte(232), CByte(232))
        Label2.Location = New Point(12, 0)
        Label2.Name = "Label2"
        Label2.Size = New Size(185, 38)
        Label2.TabIndex = 61
        Label2.Text = "NVIDIA Experience"
        Label2.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' RadioButton1
        ' 
        RadioButton1.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(30))
        RadioButton1.BackgroundImage = CType(resources.GetObject("RadioButton1.BackgroundImage"), Image)
        RadioButton1.BackgroundImageLayout = ImageLayout.Zoom
        RadioButton1.Location = New Point(1059, 3)
        RadioButton1.Name = "RadioButton1"
        RadioButton1.Size = New Size(21, 35)
        RadioButton1.TabIndex = 68
        RadioButton1.TabStop = False
        ' 
        ' NVIDIA_Shadowplay_Helper
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        BackColor = Color.FromArgb(CByte(25), CByte(25), CByte(25))
        ClientSize = New Size(1089, 311)
        Controls.Add(RadioButton1)
        Controls.Add(Label2)
        Controls.Add(BOX_LOGO)
        Controls.Add(Panel_MAIN)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "NVIDIA_Shadowplay_Helper"
        StartPosition = FormStartPosition.CenterScreen
        Text = "NVIDIA Shadowplay Helper"
        Panel_MAIN.ResumeLayout(False)
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox8, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(BOX_LOGO, ComponentModel.ISupportInitialize).EndInit()
        CType(RadioButton1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents Panel_MAIN As Panel
    Friend WithEvents BOX_LOGO As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents overlay_game As CheckBox
    Friend WithEvents IF_APP As Timer
    Friend WithEvents overlay_text As Label
    Friend WithEvents API_OVERLAY As CheckBox
    Friend WithEvents NVAPI As CheckBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Timer1 As Timer
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents PictureBox8 As PictureBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents RadioButton1 As PictureBox
    Friend WithEvents RadioButton2 As NvButton
    Friend WithEvents NvStatusDot_NVAPI As NvStatusDot
    Friend WithEvents Use_Overlay As NvToggleButton
    Friend WithEvents NvStatusDot_NVNOTIFIER As NvStatusDot
    Friend WithEvents NvStatusDot_OVERLAYAPI As NvStatusDot
    Friend WithEvents openoverlay As NvButton
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents NvIconButton1 As NvIconButton
    Friend WithEvents NvIconButton2 As NvIconButton
    Friend WithEvents PictureBox3 As PictureBox

End Class
