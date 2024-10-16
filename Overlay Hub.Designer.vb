<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class hub_f
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(hub_f))
        settings_1 = New Panel()
        PictureBox2 = New PictureBox()
        PictureBox1 = New PictureBox()
        action_fn = New Label()
        bg_fn = New PictureBox()
        text_settings = New Label()
        icon_settings = New Label()
        Label4 = New Label()
        settings_bg = New PictureBox()
        settings_top = New PictureBox()
        box_settings = New PictureBox()
        settings_1.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' settings_1
        ' 
        settings_1.BackColor = Drawing.Color.Red
        settings_1.Controls.Add(PictureBox2)
        settings_1.Controls.Add(PictureBox1)
        settings_1.Controls.Add(action_fn)
        settings_1.Controls.Add(bg_fn)
        settings_1.Controls.Add(text_settings)
        settings_1.Controls.Add(icon_settings)
        settings_1.Controls.Add(Label4)
        settings_1.Controls.Add(settings_bg)
        settings_1.Controls.Add(settings_top)
        settings_1.Controls.Add(box_settings)
        settings_1.Location = New System.Drawing.Point(12, 12)
        settings_1.Name = "settings_1"
        settings_1.Size = New System.Drawing.Size(1010, 723)
        settings_1.TabIndex = 45
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox2.BackgroundImage = My.Resources.Resources.display_graphic
        PictureBox2.Location = New System.Drawing.Point(512, 85)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New System.Drawing.Size(250, 205)
        PictureBox2.TabIndex = 74
        PictureBox2.TabStop = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Drawing.Color.Black
        PictureBox1.Location = New System.Drawing.Point(503, 85)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New System.Drawing.Size(3, 250)
        PictureBox1.TabIndex = 73
        PictureBox1.TabStop = False
        ' 
        ' action_fn
        ' 
        action_fn.AutoSize = True
        action_fn.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        action_fn.ForeColor = Drawing.Color.White
        action_fn.Location = New System.Drawing.Point(888, 24)
        action_fn.Name = "action_fn"
        action_fn.Size = New System.Drawing.Size(46, 21)
        action_fn.TabIndex = 58
        action_fn.Text = "Back"
        ' 
        ' bg_fn
        ' 
        bg_fn.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        bg_fn.Cursor = Cursors.Hand
        bg_fn.Location = New System.Drawing.Point(810, 0)
        bg_fn.Name = "bg_fn"
        bg_fn.Size = New System.Drawing.Size(200, 70)
        bg_fn.TabIndex = 57
        bg_fn.TabStop = False
        ' 
        ' text_settings
        ' 
        text_settings.AutoSize = True
        text_settings.BackColor = Drawing.Color.Black
        text_settings.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        text_settings.ForeColor = Drawing.Color.White
        text_settings.Location = New System.Drawing.Point(65, 14)
        text_settings.Name = "text_settings"
        text_settings.Size = New System.Drawing.Size(72, 21)
        text_settings.TabIndex = 56
        text_settings.Text = "Settings"
        ' 
        ' icon_settings
        ' 
        icon_settings.AutoSize = True
        icon_settings.BackColor = Drawing.Color.Black
        icon_settings.Font = New System.Drawing.Font("nvgcshare", 75F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        icon_settings.ForeColor = Drawing.Color.White
        icon_settings.Location = New System.Drawing.Point(33, 51)
        icon_settings.Name = "icon_settings"
        icon_settings.Size = New System.Drawing.Size(142, 100)
        icon_settings.TabIndex = 53
        icon_settings.Text = ""
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label4.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Drawing.Color.White
        Label4.Location = New System.Drawing.Point(258, 43)
        Label4.Name = "Label4"
        Label4.Size = New System.Drawing.Size(105, 21)
        Label4.TabIndex = 51
        Label4.Text = "Overlay Hub"
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
        ' settings_top
        ' 
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(230, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(550, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
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
        ' hub_f
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(1300, 820)
        Controls.Add(settings_1)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "hub_f"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay Hub"
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        WindowState = FormWindowState.Maximized
        settings_1.ResumeLayout(False)
        settings_1.PerformLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(bg_fn, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents settings_1 As Panel
    Friend WithEvents action_fn As Label
    Friend WithEvents bg_fn As PictureBox
    Friend WithEvents text_settings As Label
    Friend WithEvents icon_settings As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents settings_bg As PictureBox
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents box_settings As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
End Class
