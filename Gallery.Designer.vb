<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Gallery_1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Gallery_1))
        box_settings = New PictureBox()
        settings_top = New PictureBox()
        settings_bg = New PictureBox()
        chc = New Label()
        load_save = New Label()
        save_sc = New Label()
        icon_settings = New Label()
        text_settings = New Label()
        bg_fn = New PictureBox()
        action_fn = New Label()
        txtFilePath = New TextBox()
        settings_1 = New Panel()
        Label6 = New Label()
        PictureBox3 = New PictureBox()
        FlowLayoutPanel1 = New FlowLayoutPanel()
        opf = New Label()
        PictureBox5 = New PictureBox()
        allit = New Label()
        PictureBox4 = New PictureBox()
        TextBox1 = New TextBox()
        PictureBox2 = New PictureBox()
        PictureBox1 = New PictureBox()
        ALTZ = New Timer(components)
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
        ' chc
        ' 
        chc.AutoSize = True
        chc.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        chc.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        chc.ForeColor = Drawing.Color.White
        chc.Location = New System.Drawing.Point(258, 110)
        chc.Name = "chc"
        chc.Size = New System.Drawing.Size(75, 21)
        chc.TabIndex = 43
        chc.Text = "Shortcut"
        ' 
        ' load_save
        ' 
        load_save.AutoSize = True
        load_save.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        load_save.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        load_save.ForeColor = Drawing.Color.White
        load_save.Location = New System.Drawing.Point(258, 43)
        load_save.Name = "load_save"
        load_save.Size = New System.Drawing.Size(126, 21)
        load_save.TabIndex = 51
        load_save.Text = "Loaction Saved"
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
        ' text_settings
        ' 
        text_settings.BackColor = Drawing.Color.Black
        text_settings.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        text_settings.ForeColor = Drawing.Color.White
        text_settings.Location = New System.Drawing.Point(0, 14)
        text_settings.Name = "text_settings"
        text_settings.Size = New System.Drawing.Size(200, 21)
        text_settings.TabIndex = 56
        text_settings.Text = "Gallery"
        text_settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
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
        ' action_fn
        ' 
        action_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        action_fn.ForeColor = Drawing.Color.White
        action_fn.Location = New System.Drawing.Point(810, 24)
        action_fn.Name = "action_fn"
        action_fn.Size = New System.Drawing.Size(200, 21)
        action_fn.TabIndex = 58
        action_fn.Text = "Saved"
        action_fn.TextAlign = Drawing.ContentAlignment.MiddleCenter
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
        settings_1.Controls.Add(Label6)
        settings_1.Controls.Add(PictureBox3)
        settings_1.Controls.Add(FlowLayoutPanel1)
        settings_1.Controls.Add(opf)
        settings_1.Controls.Add(PictureBox5)
        settings_1.Controls.Add(allit)
        settings_1.Controls.Add(PictureBox4)
        settings_1.Controls.Add(TextBox1)
        settings_1.Controls.Add(PictureBox2)
        settings_1.Controls.Add(save_sc)
        settings_1.Controls.Add(txtFilePath)
        settings_1.Controls.Add(PictureBox1)
        settings_1.Controls.Add(chc)
        settings_1.Controls.Add(action_fn)
        settings_1.Controls.Add(bg_fn)
        settings_1.Controls.Add(text_settings)
        settings_1.Controls.Add(icon_settings)
        settings_1.Controls.Add(load_save)
        settings_1.Controls.Add(settings_bg)
        settings_1.Controls.Add(settings_top)
        settings_1.Controls.Add(box_settings)
        settings_1.Location = New System.Drawing.Point(12, 12)
        settings_1.Name = "settings_1"
        settings_1.Size = New System.Drawing.Size(1010, 723)
        settings_1.TabIndex = 43
        ' 
        ' Label6
        ' 
        Label6.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label6.Cursor = Cursors.Hand
        Label6.Font = New System.Drawing.Font("Segoe UI", 12F)
        Label6.ForeColor = Drawing.Color.White
        Label6.Location = New System.Drawing.Point(810, 195)
        Label6.Name = "Label6"
        Label6.Size = New System.Drawing.Size(200, 21)
        Label6.TabIndex = 74
        Label6.Text = "Load"
        Label6.TextAlign = Drawing.ContentAlignment.MiddleCenter
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
        ' opf
        ' 
        opf.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        opf.Cursor = Cursors.Hand
        opf.Font = New System.Drawing.Font("Segoe UI", 12F)
        opf.ForeColor = Drawing.Color.White
        opf.Location = New System.Drawing.Point(810, 110)
        opf.Name = "opf"
        opf.Size = New System.Drawing.Size(200, 21)
        opf.TabIndex = 70
        opf.Text = "Open file loaction"
        opf.TextAlign = Drawing.ContentAlignment.MiddleCenter
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
        ' allit
        ' 
        allit.AutoSize = True
        allit.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        allit.Font = New System.Drawing.Font("Segoe UI", 12F)
        allit.ForeColor = Drawing.Color.White
        allit.Location = New System.Drawing.Point(258, 179)
        allit.Name = "allit"
        allit.Size = New System.Drawing.Size(70, 21)
        allit.TabIndex = 67
        allit.Text = "All items"
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
        ' ImageList1
        ' 
        ImageList1.ColorDepth = ColorDepth.Depth32Bit
        ImageList1.ImageSize = New System.Drawing.Size(160, 90)
        ImageList1.TransparentColor = Drawing.Color.Transparent
        ' 
        ' Gallery_1
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(1300, 820)
        Controls.Add(settings_1)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Gallery_1"
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
    Friend WithEvents chc As Label
    Friend WithEvents load_save As Label
    Friend WithEvents save_sc As Label
    Friend WithEvents icon_settings As Label
    Friend WithEvents text_settings As Label
    Friend WithEvents bg_fn As PictureBox
    Friend WithEvents action_fn As Label
    Friend WithEvents txtFilePath As TextBox
    Friend WithEvents settings_1 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents allit As Label
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents opf As Label
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents ALTZ As Timer
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents Label6 As Label
    Friend WithEvents PictureBox3 As PictureBox
End Class
