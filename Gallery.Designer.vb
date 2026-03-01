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
        settings_bg = New PictureBox()
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
        FlowLayoutPanel1 = New FlowLayoutPanel()
        Openloaction_l10n = New Label()
        PictureBox5 = New PictureBox()
        Label3 = New Label()
        PictureBox4 = New PictureBox()
        TextBox1 = New TextBox()
        PictureBox2 = New PictureBox()
        PictureBox1 = New PictureBox()
        Label1 = New Label()
        ImageList1 = New ImageList(components)
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).BeginInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).BeginInit()
        settings_1.SuspendLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Drawing.Color.Black
        box_settings.Location = New System.Drawing.Point(0, 0)
        box_settings.Name = "box_settings"
        box_settings.Size = New System.Drawing.Size(200, 200)
        box_settings.TabIndex = 55
        box_settings.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(230, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(550, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' settings_bg
        ' 
        settings_bg.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_bg.Location = New System.Drawing.Point(230, 4)
        settings_bg.Name = "settings_bg"
        settings_bg.Size = New System.Drawing.Size(550, 596)
        settings_bg.TabIndex = 1
        settings_bg.TabStop = False
        ' 
        ' Shortcut_l10n
        ' 
        Shortcut_l10n.AutoSize = True
        Shortcut_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Shortcut_l10n.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        Shortcut_l10n.ForeColor = Drawing.Color.White
        Shortcut_l10n.Location = New System.Drawing.Point(289, 109)
        Shortcut_l10n.Name = "Shortcut_l10n"
        Shortcut_l10n.Size = New System.Drawing.Size(75, 21)
        Shortcut_l10n.TabIndex = 43
        Shortcut_l10n.Text = "Shortcut"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label5.Font = New System.Drawing.Font("nvgcshare", 22F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Drawing.Color.White
        Label5.Location = New System.Drawing.Point(253, 38)
        Label5.Name = "Label5"
        Label5.Size = New System.Drawing.Size(43, 30)
        Label5.TabIndex = 50
        Label5.Text = ""
        ' 
        ' LoactionSaved_l10n
        ' 
        LoactionSaved_l10n.AutoSize = True
        LoactionSaved_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        LoactionSaved_l10n.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        LoactionSaved_l10n.ForeColor = Drawing.Color.White
        LoactionSaved_l10n.Location = New System.Drawing.Point(289, 42)
        LoactionSaved_l10n.Name = "LoactionSaved_l10n"
        LoactionSaved_l10n.Size = New System.Drawing.Size(126, 21)
        LoactionSaved_l10n.TabIndex = 51
        LoactionSaved_l10n.Text = "Loaction Saved"
        ' 
        ' save_sc
        ' 
        save_sc.AutoSize = True
        save_sc.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        save_sc.Cursor = Cursors.Hand
        save_sc.Font = New System.Drawing.Font("nvgcshare", 20F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        save_sc.ForeColor = Drawing.Color.White
        save_sc.Location = New System.Drawing.Point(709, 73)
        save_sc.Name = "save_sc"
        save_sc.Size = New System.Drawing.Size(39, 27)
        save_sc.TabIndex = 52
        save_sc.Text = ""
        ' 
        ' icon_settings
        ' 
        icon_settings.AutoSize = True
        icon_settings.BackColor = Drawing.Color.Black
        icon_settings.Font = New System.Drawing.Font("nvgcshare", 75F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        icon_settings.ForeColor = Drawing.Color.White
        icon_settings.Location = New System.Drawing.Point(31, 51)
        icon_settings.Name = "icon_settings"
        icon_settings.Size = New System.Drawing.Size(142, 100)
        icon_settings.TabIndex = 53
        icon_settings.Text = ""
        ' 
        ' Gallery_l10n
        ' 
        Gallery_l10n.BackColor = Drawing.Color.Black
        Gallery_l10n.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        Gallery_l10n.ForeColor = Drawing.Color.White
        Gallery_l10n.Location = New System.Drawing.Point(0, 14)
        Gallery_l10n.Name = "Gallery_l10n"
        Gallery_l10n.Size = New System.Drawing.Size(200, 21)
        Gallery_l10n.TabIndex = 56
        Gallery_l10n.Text = "Gallery"
        Gallery_l10n.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' bg_fn
        ' 
        bg_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bg_fn.Cursor = Cursors.Hand
        bg_fn.Location = New System.Drawing.Point(810, 0)
        bg_fn.Name = "bg_fn"
        bg_fn.Size = New System.Drawing.Size(200, 70)
        bg_fn.TabIndex = 57
        bg_fn.TabStop = False
        ' 
        ' Saved_l10n
        ' 
        Saved_l10n.AutoSize = True
        Saved_l10n.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Saved_l10n.Cursor = Cursors.Hand
        Saved_l10n.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        Saved_l10n.ForeColor = Drawing.Color.White
        Saved_l10n.Location = New System.Drawing.Point(883, 24)
        Saved_l10n.Name = "Saved_l10n"
        Saved_l10n.Size = New System.Drawing.Size(56, 21)
        Saved_l10n.TabIndex = 58
        Saved_l10n.Text = "Saved"
        ' 
        ' txtFilePath
        ' 
        txtFilePath.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        txtFilePath.BorderStyle = BorderStyle.None
        txtFilePath.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        txtFilePath.ForeColor = Drawing.Color.White
        txtFilePath.Location = New System.Drawing.Point(260, 73)
        txtFilePath.Multiline = True
        txtFilePath.Name = "txtFilePath"
        txtFilePath.ReadOnly = True
        txtFilePath.Size = New System.Drawing.Size(488, 28)
        txtFilePath.TabIndex = 46
        ' 
        ' settings_1
        ' 
        settings_1.BackColor = Drawing.Color.Red
        settings_1.Controls.Add(Load_l10n)
        settings_1.Controls.Add(PictureBox3)
        settings_1.Controls.Add(FlowLayoutPanel1)
        settings_1.Controls.Add(Openloaction_l10n)
        settings_1.Controls.Add(PictureBox5)
        settings_1.Controls.Add(Label3)
        settings_1.Controls.Add(PictureBox4)
        settings_1.Controls.Add(TextBox1)
        settings_1.Controls.Add(PictureBox2)
        settings_1.Controls.Add(save_sc)
        settings_1.Controls.Add(txtFilePath)
        settings_1.Controls.Add(PictureBox1)
        settings_1.Controls.Add(Shortcut_l10n)
        settings_1.Controls.Add(Label1)
        settings_1.Controls.Add(Saved_l10n)
        settings_1.Controls.Add(bg_fn)
        settings_1.Controls.Add(Gallery_l10n)
        settings_1.Controls.Add(icon_settings)
        settings_1.Controls.Add(LoactionSaved_l10n)
        settings_1.Controls.Add(Label5)
        settings_1.Controls.Add(settings_bg)
        settings_1.Controls.Add(settings_top)
        settings_1.Controls.Add(box_settings)
        settings_1.Location = New System.Drawing.Point(12, 12)
        settings_1.Name = "settings_1"
        settings_1.Size = New System.Drawing.Size(1010, 723)
        settings_1.TabIndex = 43
        ' 
        ' Load_l10n
        ' 
        Load_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Load_l10n.Cursor = Cursors.Hand
        Load_l10n.Font = New System.Drawing.Font("Segoe UI", 12F)
        Load_l10n.ForeColor = Drawing.Color.White
        Load_l10n.Location = New System.Drawing.Point(810, 195)
        Load_l10n.Name = "Load_l10n"
        Load_l10n.Size = New System.Drawing.Size(200, 21)
        Load_l10n.TabIndex = 74
        Load_l10n.Text = "Load"
        Load_l10n.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' PictureBox3
        ' 
        PictureBox3.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox3.Cursor = Cursors.Hand
        PictureBox3.Location = New System.Drawing.Point(810, 171)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New System.Drawing.Size(200, 70)
        PictureBox3.TabIndex = 73
        PictureBox3.TabStop = False
        ' 
        ' FlowLayoutPanel1
        ' 
        FlowLayoutPanel1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        FlowLayoutPanel1.Location = New System.Drawing.Point(260, 205)
        FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        FlowLayoutPanel1.Size = New System.Drawing.Size(488, 368)
        FlowLayoutPanel1.TabIndex = 72
        ' 
        ' Openloaction_l10n
        ' 
        Openloaction_l10n.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Openloaction_l10n.Cursor = Cursors.Hand
        Openloaction_l10n.Font = New System.Drawing.Font("Segoe UI", 12F)
        Openloaction_l10n.ForeColor = Drawing.Color.White
        Openloaction_l10n.Location = New System.Drawing.Point(810, 110)
        Openloaction_l10n.Name = "Openloaction_l10n"
        Openloaction_l10n.Size = New System.Drawing.Size(200, 21)
        Openloaction_l10n.TabIndex = 70
        Openloaction_l10n.Text = "Openloaction_l10n"
        Openloaction_l10n.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' PictureBox5
        ' 
        PictureBox5.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox5.Cursor = Cursors.Hand
        PictureBox5.Location = New System.Drawing.Point(810, 86)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New System.Drawing.Size(200, 70)
        PictureBox5.TabIndex = 69
        PictureBox5.TabStop = False
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New System.Drawing.Font("Segoe UI", 12F)
        Label3.ForeColor = Drawing.Color.White
        Label3.Location = New System.Drawing.Point(258, 179)
        Label3.Name = "Label3"
        Label3.Size = New System.Drawing.Size(70, 21)
        Label3.TabIndex = 67
        Label3.Text = "All items"
        ' 
        ' PictureBox4
        ' 
        PictureBox4.BackColor = Drawing.Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox4.Location = New System.Drawing.Point(258, 203)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New System.Drawing.Size(492, 372)
        PictureBox4.TabIndex = 66
        PictureBox4.TabStop = False
        ' 
        ' TextBox1
        ' 
        TextBox1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        TextBox1.BorderStyle = BorderStyle.None
        TextBox1.Enabled = False
        TextBox1.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        TextBox1.ForeColor = Drawing.Color.White
        TextBox1.Location = New System.Drawing.Point(260, 141)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.Size = New System.Drawing.Size(136, 28)
        TextBox1.TabIndex = 62
        TextBox1.Text = "Alt + F1"
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Drawing.Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox2.Location = New System.Drawing.Point(258, 139)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New System.Drawing.Size(140, 32)
        PictureBox2.TabIndex = 63
        PictureBox2.TabStop = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Drawing.Color.FromArgb(CByte(56), CByte(56), CByte(56))
        PictureBox1.Location = New System.Drawing.Point(258, 71)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New System.Drawing.Size(492, 32)
        PictureBox1.TabIndex = 61
        PictureBox1.TabStop = False
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New System.Drawing.Font("nvgcshare", 22F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Drawing.Color.White
        Label1.Location = New System.Drawing.Point(253, 106)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(43, 30)
        Label1.TabIndex = 60
        Label1.Text = ""
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
        ClientSize = New System.Drawing.Size(1300, 820)
        Controls.Add(settings_1)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Base_Gallery"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Gallery"
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).EndInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).EndInit()
        settings_1.ResumeLayout(False)
        settings_1.PerformLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents box_settings As PictureBox
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents settings_bg As PictureBox
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
End Class
