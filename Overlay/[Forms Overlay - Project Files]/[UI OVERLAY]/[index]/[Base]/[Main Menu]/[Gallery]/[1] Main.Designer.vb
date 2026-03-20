<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_Gallery
    Inherits NoCloseForm

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
        Load_l10n = New Label()
        PictureBox3 = New PictureBox()
        Openloaction_l10n = New Label()
        PictureBox5 = New PictureBox()
        Base_Submenu = New Panel()
        text_sub = New Label()
        Label2 = New Label()
        Label3 = New Label()
        FlowLayoutPanel1 = New FlowLayoutPanel()
        Label1 = New Label()
        PictureBox2 = New PictureBox()
        TextBox1 = New TextBox()
        PictureBox1 = New PictureBox()
        PictureBox4 = New PictureBox()
        ImageList1 = New ImageList(components)
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).BeginInit()
        settings_1.SuspendLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        Base_Submenu.SuspendLayout()
        FlowLayoutPanel1.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Drawing.Color.Black
        box_settings.Location = New System.Drawing.Point(0, 0)
        box_settings.Name = "box_settings"
        box_settings.Size = New System.Drawing.Size(240, 240)
        box_settings.TabIndex = 55
        box_settings.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(284, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(753, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' Shortcut_l10n
        ' 
        Shortcut_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Shortcut_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Shortcut_l10n.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        Shortcut_l10n.ForeColor = Drawing.Color.White
        Shortcut_l10n.Location = New System.Drawing.Point(352, 0)
        Shortcut_l10n.Name = "Shortcut_l10n"
        Shortcut_l10n.Size = New System.Drawing.Size(75, 32)
        Shortcut_l10n.TabIndex = 43
        Shortcut_l10n.Text = "Shortcut"
        Shortcut_l10n.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' Label5
        ' 
        Label5.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label5.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label5.Font = New System.Drawing.Font("nvgcshare", 35F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Drawing.Color.White
        Label5.Location = New System.Drawing.Point(3, 283)
        Label5.Name = "Label5"
        Label5.Size = New System.Drawing.Size(70, 45)
        Label5.TabIndex = 50
        Label5.Text = ""
        ' 
        ' LoactionSaved_l10n
        ' 
        LoactionSaved_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        LoactionSaved_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        LoactionSaved_l10n.Font = New System.Drawing.Font("Segoe UI Semibold", 12F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        LoactionSaved_l10n.ForeColor = Drawing.Color.White
        LoactionSaved_l10n.Location = New System.Drawing.Point(16, 184)
        LoactionSaved_l10n.Name = "LoactionSaved_l10n"
        LoactionSaved_l10n.Size = New System.Drawing.Size(138, 32)
        LoactionSaved_l10n.TabIndex = 51
        LoactionSaved_l10n.Text = "Loaction Saved"
        LoactionSaved_l10n.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' save_sc
        ' 
        save_sc.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        save_sc.AutoSize = True
        save_sc.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        save_sc.Cursor = Cursors.Hand
        save_sc.Font = New System.Drawing.Font("nvgcshare", 20F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        save_sc.ForeColor = Drawing.Color.White
        save_sc.Location = New System.Drawing.Point(692, 186)
        save_sc.Name = "save_sc"
        save_sc.Size = New System.Drawing.Size(39, 27)
        save_sc.TabIndex = 52
        save_sc.Text = ""
        ' 
        ' icon_settings
        ' 
        icon_settings.BackColor = Drawing.Color.Black
        icon_settings.Font = New System.Drawing.Font("nvgcshare", 100F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        icon_settings.ForeColor = Drawing.Color.White
        icon_settings.Location = New System.Drawing.Point(31, 0)
        icon_settings.Name = "icon_settings"
        icon_settings.Size = New System.Drawing.Size(183, 240)
        icon_settings.TabIndex = 53
        icon_settings.Text = ""
        icon_settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Gallery_l10n
        ' 
        Gallery_l10n.BackColor = Drawing.Color.Black
        Gallery_l10n.Font = New System.Drawing.Font("Segoe UI Semibold", 14F, Drawing.FontStyle.Bold)
        Gallery_l10n.ForeColor = Drawing.Color.White
        Gallery_l10n.Location = New System.Drawing.Point(0, 14)
        Gallery_l10n.Name = "Gallery_l10n"
        Gallery_l10n.Size = New System.Drawing.Size(240, 31)
        Gallery_l10n.TabIndex = 56
        Gallery_l10n.Text = "Gallery"
        Gallery_l10n.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' bg_fn
        ' 
        bg_fn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        bg_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bg_fn.Cursor = Cursors.Hand
        bg_fn.Location = New System.Drawing.Point(1081, 0)
        bg_fn.Name = "bg_fn"
        bg_fn.Size = New System.Drawing.Size(200, 70)
        bg_fn.TabIndex = 57
        bg_fn.TabStop = False
        ' 
        ' Saved_l10n
        ' 
        Saved_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Saved_l10n.AutoSize = True
        Saved_l10n.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Saved_l10n.Cursor = Cursors.Hand
        Saved_l10n.Font = New System.Drawing.Font("Segoe UI Semibold", 12F, Drawing.FontStyle.Bold)
        Saved_l10n.ForeColor = Drawing.Color.White
        Saved_l10n.Location = New System.Drawing.Point(1154, 24)
        Saved_l10n.Name = "Saved_l10n"
        Saved_l10n.Size = New System.Drawing.Size(54, 21)
        Saved_l10n.TabIndex = 58
        Saved_l10n.Text = "Saved"
        ' 
        ' txtFilePath
        ' 
        txtFilePath.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtFilePath.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        txtFilePath.BorderStyle = BorderStyle.None
        txtFilePath.Enabled = False
        txtFilePath.Font = New System.Drawing.Font("Segoe UI Semibold", 13F, Drawing.FontStyle.Bold)
        txtFilePath.ForeColor = Drawing.Color.White
        txtFilePath.Location = New System.Drawing.Point(157, 186)
        txtFilePath.Multiline = True
        txtFilePath.Name = "txtFilePath"
        txtFilePath.ReadOnly = True
        txtFilePath.Size = New System.Drawing.Size(578, 28)
        txtFilePath.TabIndex = 46
        txtFilePath.WordWrap = False
        ' 
        ' settings_1
        ' 
        settings_1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_1.BackColor = Drawing.Color.Red
        settings_1.Controls.Add(Load_l10n)
        settings_1.Controls.Add(PictureBox3)
        settings_1.Controls.Add(Openloaction_l10n)
        settings_1.Controls.Add(PictureBox5)
        settings_1.Controls.Add(Saved_l10n)
        settings_1.Controls.Add(bg_fn)
        settings_1.Controls.Add(Gallery_l10n)
        settings_1.Controls.Add(icon_settings)
        settings_1.Controls.Add(settings_top)
        settings_1.Controls.Add(box_settings)
        settings_1.Controls.Add(Base_Submenu)
        settings_1.Location = New System.Drawing.Point(12, 12)
        settings_1.Name = "settings_1"
        settings_1.Size = New System.Drawing.Size(1280, 904)
        settings_1.TabIndex = 43
        ' 
        ' Load_l10n
        ' 
        Load_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Load_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Load_l10n.Cursor = Cursors.Hand
        Load_l10n.Font = New System.Drawing.Font("Segoe UI Semibold", 12F)
        Load_l10n.ForeColor = Drawing.Color.White
        Load_l10n.Location = New System.Drawing.Point(1081, 195)
        Load_l10n.Name = "Load_l10n"
        Load_l10n.Size = New System.Drawing.Size(200, 21)
        Load_l10n.TabIndex = 74
        Load_l10n.Text = "Load"
        Load_l10n.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Load_l10n.Visible = False
        ' 
        ' PictureBox3
        ' 
        PictureBox3.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        PictureBox3.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox3.Cursor = Cursors.Hand
        PictureBox3.Location = New System.Drawing.Point(1081, 170)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New System.Drawing.Size(200, 70)
        PictureBox3.TabIndex = 73
        PictureBox3.TabStop = False
        PictureBox3.Visible = False
        ' 
        ' Openloaction_l10n
        ' 
        Openloaction_l10n.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        Openloaction_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Openloaction_l10n.Cursor = Cursors.Hand
        Openloaction_l10n.Font = New System.Drawing.Font("Segoe UI Semibold", 12F)
        Openloaction_l10n.ForeColor = Drawing.Color.White
        Openloaction_l10n.Location = New System.Drawing.Point(1081, 110)
        Openloaction_l10n.Name = "Openloaction_l10n"
        Openloaction_l10n.Size = New System.Drawing.Size(200, 21)
        Openloaction_l10n.TabIndex = 70
        Openloaction_l10n.Text = "Openloaction_l10n"
        Openloaction_l10n.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' PictureBox5
        ' 
        PictureBox5.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        PictureBox5.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox5.Cursor = Cursors.Hand
        PictureBox5.Location = New System.Drawing.Point(1081, 86)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New System.Drawing.Size(200, 70)
        PictureBox5.TabIndex = 69
        PictureBox5.TabStop = False
        ' 
        ' Base_Submenu
        ' 
        Base_Submenu.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Base_Submenu.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Base_Submenu.Controls.Add(text_sub)
        Base_Submenu.Controls.Add(Label2)
        Base_Submenu.Controls.Add(Label3)
        Base_Submenu.Controls.Add(Label5)
        Base_Submenu.Controls.Add(LoactionSaved_l10n)
        Base_Submenu.Controls.Add(FlowLayoutPanel1)
        Base_Submenu.Controls.Add(save_sc)
        Base_Submenu.Controls.Add(txtFilePath)
        Base_Submenu.Controls.Add(PictureBox1)
        Base_Submenu.Controls.Add(PictureBox4)
        Base_Submenu.Location = New System.Drawing.Point(284, 5)
        Base_Submenu.Name = "Base_Submenu"
        Base_Submenu.Size = New System.Drawing.Size(753, 235)
        Base_Submenu.TabIndex = 75
        ' 
        ' text_sub
        ' 
        text_sub.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        text_sub.BackColor = Drawing.Color.Transparent
        text_sub.Font = New System.Drawing.Font("Segoe UI Semibold", 13F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        text_sub.ForeColor = Drawing.Color.Gray
        text_sub.Location = New System.Drawing.Point(0, 126)
        text_sub.Name = "text_sub"
        text_sub.Size = New System.Drawing.Size(753, 36)
        text_sub.TabIndex = 77
        text_sub.Text = "Privacy control"
        text_sub.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label2.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New System.Drawing.Font("nvgcshare", 120F)
        Label2.ForeColor = Drawing.Color.DimGray
        Label2.Location = New System.Drawing.Point(16, 0)
        Label2.Name = "Label2"
        Label2.Size = New System.Drawing.Size(737, 147)
        Label2.TabIndex = 76
        Label2.Text = ""
        Label2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Label3
        ' 
        Label3.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label3.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New System.Drawing.Font("Segoe UI", 13F)
        Label3.ForeColor = Drawing.Color.White
        Label3.Location = New System.Drawing.Point(61, 291)
        Label3.Name = "Label3"
        Label3.Size = New System.Drawing.Size(129, 30)
        Label3.TabIndex = 67
        Label3.Text = "All items"
        ' 
        ' FlowLayoutPanel1
        ' 
        FlowLayoutPanel1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        FlowLayoutPanel1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        FlowLayoutPanel1.Controls.Add(Label1)
        FlowLayoutPanel1.Controls.Add(PictureBox2)
        FlowLayoutPanel1.Controls.Add(TextBox1)
        FlowLayoutPanel1.Controls.Add(Shortcut_l10n)
        FlowLayoutPanel1.Location = New System.Drawing.Point(17, 332)
        FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        FlowLayoutPanel1.Size = New System.Drawing.Size(720, 374)
        FlowLayoutPanel1.TabIndex = 72
        ' 
        ' Label1
        ' 
        Label1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New System.Drawing.Font("nvgcshare", 35F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Drawing.Color.White
        Label1.Location = New System.Drawing.Point(3, 0)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(55, 47)
        Label1.TabIndex = 60
        Label1.Text = ""
        ' 
        ' PictureBox2
        ' 
        PictureBox2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox2.BackColor = Drawing.Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox2.Location = New System.Drawing.Point(64, 3)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New System.Drawing.Size(140, 32)
        PictureBox2.TabIndex = 63
        PictureBox2.TabStop = False
        ' 
        ' TextBox1
        ' 
        TextBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        TextBox1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        TextBox1.BorderStyle = BorderStyle.None
        TextBox1.Enabled = False
        TextBox1.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        TextBox1.ForeColor = Drawing.Color.White
        TextBox1.Location = New System.Drawing.Point(210, 3)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.Size = New System.Drawing.Size(136, 28)
        TextBox1.TabIndex = 62
        TextBox1.Text = "Alt + F1"
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox1.BackColor = Drawing.Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox1.Location = New System.Drawing.Point(155, 184)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New System.Drawing.Size(582, 32)
        PictureBox1.TabIndex = 61
        PictureBox1.TabStop = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox4.BackColor = Drawing.Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox4.Location = New System.Drawing.Point(16, 331)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New System.Drawing.Size(722, 376)
        PictureBox4.TabIndex = 66
        PictureBox4.TabStop = False
        ' 
        ' ImageList1
        ' 
        ImageList1.ColorDepth = ColorDepth.Depth32Bit
        ImageList1.ImageSize = New System.Drawing.Size(160, 90)
        ImageList1.TransparentColor = Drawing.Color.Transparent
        ' 
        ' Base_Gallery
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(1920, 1080)
        Controls.Add(settings_1)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Base_Gallery"
        ShowInTaskbar = False
        Text = "Gallery"
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).EndInit()
        settings_1.ResumeLayout(False)
        settings_1.PerformLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents Load_l10n As Label
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents Base_Submenu As Panel
    Friend WithEvents Label2 As Label
    Friend WithEvents text_sub As Label
End Class
