<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_Gallery
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Gallery))
        box_settings = New PictureBox()
        settings_top = New PictureBox()
        Shortcut_l10n = New Label()
        Label5 = New Label()
        LoactionSaved_l10n = New Label()
        save_sc = New Label()
        icon_settings = New Label()
        Gallery_l10n = New Label()
        bg_fn = New PictureBox()
        Saved_l10n = New Label()
        txtFilePath = New TextBox()
        settings_1 = New Panel()
        PictureBox6 = New PictureBox()
        PictureBox7 = New PictureBox()
        Load_l10n = New Label()
        PictureBox3 = New PictureBox()
        Base_Submenu = New Panel()
        Label4 = New Label()
        text_sub = New Label()
        Label2 = New Label()
        Label3 = New Label()
        FlowLayoutPanel1 = New FlowLayoutPanel()
        Label1 = New Label()
        PictureBox2 = New PictureBox()
        TextBox1 = New TextBox()
        PictureBox1 = New PictureBox()
        PictureBox4 = New PictureBox()
        Openloaction_l10n = New Label()
        Timer1 = New Timer(components)
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).BeginInit()
        settings_1.SuspendLayout()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        Base_Submenu.SuspendLayout()
        FlowLayoutPanel1.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Color.Black
        box_settings.Location = New Point(0, 0)
        box_settings.Name = "box_settings"
        box_settings.Size = New Size(240, 240)
        box_settings.TabIndex = 55
        box_settings.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(280, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(720, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' Shortcut_l10n
        ' 
        Shortcut_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Shortcut_l10n.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Shortcut_l10n.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        Shortcut_l10n.ForeColor = Color.White
        Shortcut_l10n.Location = New Point(352, 0)
        Shortcut_l10n.Name = "Shortcut_l10n"
        Shortcut_l10n.Size = New Size(75, 32)
        Shortcut_l10n.TabIndex = 43
        Shortcut_l10n.Text = "Shortcut"
        Shortcut_l10n.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label5
        ' 
        Label5.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label5.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label5.Font = New Font("nvgcshare", 35F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.White
        Label5.Location = New Point(3, 283)
        Label5.Name = "Label5"
        Label5.Size = New Size(37, 45)
        Label5.TabIndex = 50
        Label5.Text = ""
        ' 
        ' LoactionSaved_l10n
        ' 
        LoactionSaved_l10n.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        LoactionSaved_l10n.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LoactionSaved_l10n.ForeColor = Color.White
        LoactionSaved_l10n.Location = New Point(16, 184)
        LoactionSaved_l10n.Name = "LoactionSaved_l10n"
        LoactionSaved_l10n.Size = New Size(138, 32)
        LoactionSaved_l10n.TabIndex = 51
        LoactionSaved_l10n.Text = "Loaction Saved"
        LoactionSaved_l10n.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' save_sc
        ' 
        save_sc.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        save_sc.AutoSize = True
        save_sc.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        save_sc.Cursor = Cursors.Hand
        save_sc.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        save_sc.ForeColor = Color.White
        save_sc.Location = New Point(659, 186)
        save_sc.Name = "save_sc"
        save_sc.Size = New Size(39, 27)
        save_sc.TabIndex = 52
        save_sc.Text = ""
        ' 
        ' icon_settings
        ' 
        icon_settings.BackColor = Color.Black
        icon_settings.Font = New Font("nvgcshare", 100F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        icon_settings.ForeColor = Color.White
        icon_settings.Location = New Point(31, 0)
        icon_settings.Name = "icon_settings"
        icon_settings.Size = New Size(183, 240)
        icon_settings.TabIndex = 53
        icon_settings.Text = ""
        icon_settings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Gallery_l10n
        ' 
        Gallery_l10n.BackColor = Color.Black
        Gallery_l10n.Font = New Font("Segoe UI Semibold", 14F, FontStyle.Bold)
        Gallery_l10n.ForeColor = Color.White
        Gallery_l10n.Location = New Point(0, 14)
        Gallery_l10n.Name = "Gallery_l10n"
        Gallery_l10n.Size = New Size(240, 31)
        Gallery_l10n.TabIndex = 56
        Gallery_l10n.Text = "Gallery"
        Gallery_l10n.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' bg_fn
        ' 
        bg_fn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        bg_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bg_fn.Cursor = Cursors.Hand
        bg_fn.Location = New Point(1040, 0)
        bg_fn.Name = "bg_fn"
        bg_fn.Size = New Size(240, 81)
        bg_fn.TabIndex = 57
        bg_fn.TabStop = False
        ' 
        ' Saved_l10n
        ' 
        Saved_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Saved_l10n.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Saved_l10n.Cursor = Cursors.Hand
        Saved_l10n.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        Saved_l10n.ForeColor = Color.White
        Saved_l10n.Location = New Point(1040, 0)
        Saved_l10n.Name = "Saved_l10n"
        Saved_l10n.Size = New Size(240, 81)
        Saved_l10n.TabIndex = 58
        Saved_l10n.Text = "Saved"
        Saved_l10n.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' txtFilePath
        ' 
        txtFilePath.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtFilePath.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        txtFilePath.BorderStyle = BorderStyle.None
        txtFilePath.Enabled = False
        txtFilePath.Font = New Font("Segoe UI Semilight", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtFilePath.ForeColor = Color.White
        txtFilePath.Location = New Point(160, 190)
        txtFilePath.Name = "txtFilePath"
        txtFilePath.ReadOnly = True
        txtFilePath.Size = New Size(542, 20)
        txtFilePath.TabIndex = 46
        txtFilePath.Text = "C:\Users\ScotcsDuluka\Videos\Shadowplay\Gallery"
        txtFilePath.WordWrap = False
        ' 
        ' settings_1
        ' 
        settings_1.Anchor = AnchorStyles.Top
        settings_1.BackColor = Color.Red
        settings_1.Controls.Add(PictureBox6)
        settings_1.Controls.Add(PictureBox7)
        settings_1.Controls.Add(Load_l10n)
        settings_1.Controls.Add(PictureBox3)
        settings_1.Controls.Add(Saved_l10n)
        settings_1.Controls.Add(bg_fn)
        settings_1.Controls.Add(Gallery_l10n)
        settings_1.Controls.Add(icon_settings)
        settings_1.Controls.Add(settings_top)
        settings_1.Controls.Add(box_settings)
        settings_1.Controls.Add(Base_Submenu)
        settings_1.Controls.Add(Openloaction_l10n)
        settings_1.Location = New Point(150, 197)
        settings_1.Name = "settings_1"
        settings_1.Size = New Size(1280, 483)
        settings_1.TabIndex = 43
        ' 
        ' PictureBox6
        ' 
        PictureBox6.BackColor = Color.Blue
        PictureBox6.BackgroundImageLayout = ImageLayout.None
        PictureBox6.Location = New Point(1000, 87)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(40, 80)
        PictureBox6.TabIndex = 92
        PictureBox6.TabStop = False
        PictureBox6.Visible = False
        ' 
        ' PictureBox7
        ' 
        PictureBox7.BackColor = Color.Blue
        PictureBox7.BackgroundImageLayout = ImageLayout.None
        PictureBox7.Location = New Point(240, 99)
        PictureBox7.Name = "PictureBox7"
        PictureBox7.Size = New Size(40, 80)
        PictureBox7.TabIndex = 91
        PictureBox7.TabStop = False
        PictureBox7.Visible = False
        ' 
        ' Load_l10n
        ' 
        Load_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Load_l10n.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Load_l10n.Cursor = Cursors.Hand
        Load_l10n.Font = New Font("Segoe UI Semibold", 12F)
        Load_l10n.ForeColor = Color.White
        Load_l10n.Location = New Point(1040, 200)
        Load_l10n.Name = "Load_l10n"
        Load_l10n.Size = New Size(240, 21)
        Load_l10n.TabIndex = 74
        Load_l10n.Text = "Load"
        Load_l10n.TextAlign = ContentAlignment.MiddleCenter
        Load_l10n.Visible = False
        ' 
        ' PictureBox3
        ' 
        PictureBox3.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        PictureBox3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox3.Cursor = Cursors.Hand
        PictureBox3.Location = New Point(1040, 175)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(240, 70)
        PictureBox3.TabIndex = 73
        PictureBox3.TabStop = False
        PictureBox3.Visible = False
        ' 
        ' Base_Submenu
        ' 
        Base_Submenu.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Base_Submenu.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Base_Submenu.Controls.Add(save_sc)
        Base_Submenu.Controls.Add(txtFilePath)
        Base_Submenu.Controls.Add(Label4)
        Base_Submenu.Controls.Add(text_sub)
        Base_Submenu.Controls.Add(Label2)
        Base_Submenu.Controls.Add(Label3)
        Base_Submenu.Controls.Add(Label5)
        Base_Submenu.Controls.Add(LoactionSaved_l10n)
        Base_Submenu.Controls.Add(FlowLayoutPanel1)
        Base_Submenu.Controls.Add(PictureBox1)
        Base_Submenu.Controls.Add(PictureBox4)
        Base_Submenu.Location = New Point(280, 5)
        Base_Submenu.Name = "Base_Submenu"
        Base_Submenu.Size = New Size(720, 235)
        Base_Submenu.TabIndex = 75
        ' 
        ' Label4
        ' 
        Label4.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        Label4.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label4.Cursor = Cursors.Hand
        Label4.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Color.White
        Label4.Location = New Point(157, 186)
        Label4.Name = "Label4"
        Label4.Size = New Size(545, 28)
        Label4.TabIndex = 78
        ' 
        ' text_sub
        ' 
        text_sub.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        text_sub.BackColor = Color.Transparent
        text_sub.Font = New Font("Segoe UI Semibold", 13F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        text_sub.ForeColor = Color.Gray
        text_sub.Location = New Point(0, 126)
        text_sub.Name = "text_sub"
        text_sub.Size = New Size(720, 36)
        text_sub.TabIndex = 77
        text_sub.Text = "Privacy control"
        text_sub.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label2.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New Font("nvgcshare", 120F)
        Label2.ForeColor = Color.DimGray
        Label2.Location = New Point(16, 0)
        Label2.Name = "Label2"
        Label2.Size = New Size(704, 147)
        Label2.TabIndex = 76
        Label2.Text = ""
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label3
        ' 
        Label3.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New Font("Segoe UI", 13F)
        Label3.ForeColor = Color.White
        Label3.Location = New Point(61, 291)
        Label3.Name = "Label3"
        Label3.Size = New Size(96, 30)
        Label3.TabIndex = 67
        Label3.Text = "All items"
        ' 
        ' FlowLayoutPanel1
        ' 
        FlowLayoutPanel1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        FlowLayoutPanel1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        FlowLayoutPanel1.Controls.Add(Label1)
        FlowLayoutPanel1.Controls.Add(PictureBox2)
        FlowLayoutPanel1.Controls.Add(TextBox1)
        FlowLayoutPanel1.Controls.Add(Shortcut_l10n)
        FlowLayoutPanel1.Location = New Point(17, 332)
        FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        FlowLayoutPanel1.Size = New Size(687, 374)
        FlowLayoutPanel1.TabIndex = 72
        ' 
        ' Label1
        ' 
        Label1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New Font("nvgcshare", 35F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(3, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(55, 47)
        Label1.TabIndex = 60
        Label1.Text = ""
        ' 
        ' PictureBox2
        ' 
        PictureBox2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox2.BackColor = Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox2.Location = New Point(64, 3)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(140, 32)
        PictureBox2.TabIndex = 63
        PictureBox2.TabStop = False
        ' 
        ' TextBox1
        ' 
        TextBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        TextBox1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        TextBox1.BorderStyle = BorderStyle.None
        TextBox1.Enabled = False
        TextBox1.Font = New Font("Segoe UI", 13F, FontStyle.Bold)
        TextBox1.ForeColor = Color.White
        TextBox1.Location = New Point(210, 3)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.Size = New Size(136, 28)
        TextBox1.TabIndex = 62
        TextBox1.Text = "Alt + F1"
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox1.BackColor = Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox1.Location = New Point(155, 184)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(549, 32)
        PictureBox1.TabIndex = 61
        PictureBox1.TabStop = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox4.BackColor = Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox4.Location = New Point(16, 331)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(689, 376)
        PictureBox4.TabIndex = 66
        PictureBox4.TabStop = False
        ' 
        ' Openloaction_l10n
        ' 
        Openloaction_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Openloaction_l10n.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Openloaction_l10n.Cursor = Cursors.Hand
        Openloaction_l10n.Font = New Font("Segoe UI Semibold", 12F)
        Openloaction_l10n.ForeColor = Color.White
        Openloaction_l10n.Location = New Point(1040, 91)
        Openloaction_l10n.Name = "Openloaction_l10n"
        Openloaction_l10n.Size = New Size(240, 70)
        Openloaction_l10n.TabIndex = 70
        Openloaction_l10n.Text = "Openloaction_l10n"
        Openloaction_l10n.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Timer1
        ' 
        Timer1.Enabled = True
        Timer1.Interval = 1
        ' 
        ' Base_Gallery
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Red
        ClientSize = New Size(1679, 898)
        Controls.Add(settings_1)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Gallery"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "Gallery"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).EndInit()
        settings_1.ResumeLayout(False)
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        Base_Submenu.ResumeLayout(False)
        Base_Submenu.PerformLayout()
        FlowLayoutPanel1.ResumeLayout(False)
        FlowLayoutPanel1.PerformLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents box_settings As PictureBox
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents Shortcut_l10n As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents LoactionSaved_l10n As Label
    Friend WithEvents save_sc As Label
    Friend WithEvents icon_settings As Label
    Friend WithEvents Gallery_l10n As Label
    Friend WithEvents bg_fn As PictureBox
    Friend WithEvents Saved_l10n As Label
    Friend WithEvents txtFilePath As TextBox
    Friend WithEvents settings_1 As Panel
    Friend WithEvents Label1 As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents Label3 As Label
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents Openloaction_l10n As Label
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents Load_l10n As Label
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents Base_Submenu As Panel
    Friend WithEvents Label2 As Label
    Friend WithEvents text_sub As Label
    Friend WithEvents PictureBox7 As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents Timer1 As Timer
    Friend WithEvents Label4 As Label
End Class
