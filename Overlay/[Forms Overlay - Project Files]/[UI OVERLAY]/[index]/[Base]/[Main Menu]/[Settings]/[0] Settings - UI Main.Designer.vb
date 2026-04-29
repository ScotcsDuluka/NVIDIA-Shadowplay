<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Settings
    Inherits NoCloseForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Settings))
        Main_Menu_SET = New Panel()
        Panel = New Panel()
        text_sub = New Label()
        ICON_1 = New Label()
        settings_top = New PictureBox()
        action_1 = New Label()
        Back_btn = New Label()
        text_settings = New Label()
        Block_AM = New PictureBox()
        action_fn = New Label()
        ch = New Label()
        SW_lang = New Label()
        Panel1 = New Panel()
        PictureBox9 = New PictureBox()
        PictureBox1 = New PictureBox()
        Panel2 = New Panel()
        Main_Menu_SET.SuspendLayout()
        Panel.SuspendLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Block_AM, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        Panel2.SuspendLayout()
        SuspendLayout()
        ' 
        ' Main_Menu_SET
        ' 
        Main_Menu_SET.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Main_Menu_SET.BackColor = Color.Red
        Main_Menu_SET.Controls.Add(Panel)
        Main_Menu_SET.Location = New Point(695, 160)
        Main_Menu_SET.Name = "Main_Menu_SET"
        Main_Menu_SET.Size = New Size(770, 579)
        Main_Menu_SET.TabIndex = 44
        ' 
        ' Panel
        ' 
        Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Panel.Controls.Add(Panel2)
        Panel.Location = New Point(0, -7)
        Panel.Name = "Panel"
        Panel.Size = New Size(770, 586)
        Panel.TabIndex = 74
        ' 
        ' text_sub
        ' 
        text_sub.Anchor = AnchorStyles.Bottom
        text_sub.BackColor = Color.Transparent
        text_sub.Font = New Font("Segoe UI Semibold", 13F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        text_sub.ForeColor = Color.Gray
        text_sub.Location = New Point(0, 130)
        text_sub.Name = "text_sub"
        text_sub.Size = New Size(770, 83)
        text_sub.TabIndex = 51
        text_sub.Text = "Privacy control"
        text_sub.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ICON_1
        ' 
        ICON_1.Anchor = AnchorStyles.Top
        ICON_1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ICON_1.Font = New Font("nvgcshare", 120F)
        ICON_1.ForeColor = Color.DimGray
        ICON_1.Location = New Point(0, 0)
        ICON_1.Name = "ICON_1"
        ICON_1.Size = New Size(770, 145)
        ICON_1.TabIndex = 74
        ICON_1.Text = ""
        ICON_1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(695, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(770, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' action_1
        ' 
        action_1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        action_1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        action_1.Cursor = Cursors.Hand
        action_1.Font = New Font("Segoe UI", 12F)
        action_1.ForeColor = Color.White
        action_1.Location = New Point(110, 220)
        action_1.Name = "action_1"
        action_1.Size = New Size(200, 0)
        action_1.TabIndex = 72
        action_1.Text = "Turn on"
        action_1.TextAlign = ContentAlignment.MiddleCenter
        action_1.Visible = False
        ' 
        ' Back_btn
        ' 
        Back_btn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Back_btn.Cursor = Cursors.Hand
        Back_btn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        Back_btn.ForeColor = Color.White
        Back_btn.Location = New Point(110, 300)
        Back_btn.Name = "Back_btn"
        Back_btn.Size = New Size(200, 70)
        Back_btn.TabIndex = 58
        Back_btn.Text = "Back"
        Back_btn.TextAlign = ContentAlignment.MiddleCenter
        Back_btn.Visible = False
        ' 
        ' text_settings
        ' 
        text_settings.BackColor = Color.Black
        text_settings.Font = New Font("Segoe UI Semibold", 14F, FontStyle.Bold)
        text_settings.ForeColor = Color.White
        text_settings.Location = New Point(465, 160)
        text_settings.Name = "text_settings"
        text_settings.Size = New Size(200, 50)
        text_settings.TabIndex = 56
        text_settings.Text = "Settings"
        text_settings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Block_AM
        ' 
        Block_AM.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Block_AM.Location = New Point(-3, -16)
        Block_AM.Name = "Block_AM"
        Block_AM.Size = New Size(1796, 176)
        Block_AM.TabIndex = 73
        Block_AM.TabStop = False
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(465, 220)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 70)
        action_fn.TabIndex = 74
        action_fn.Text = "Done"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ch
        ' 
        ch.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ch.Cursor = Cursors.Hand
        ch.Font = New Font("Segoe UI Semibold", 12F)
        ch.ForeColor = Color.White
        ch.Location = New Point(465, 380)
        ch.Name = "ch"
        ch.Size = New Size(200, 70)
        ch.TabIndex = 75
        ch.Text = "Check update"
        ch.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' SW_lang
        ' 
        SW_lang.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        SW_lang.Cursor = Cursors.Hand
        SW_lang.Font = New Font("Segoe UI Semibold", 12F)
        SW_lang.ForeColor = Color.White
        SW_lang.Location = New Point(465, 300)
        SW_lang.Name = "SW_lang"
        SW_lang.Size = New Size(200, 70)
        SW_lang.TabIndex = 76
        SW_lang.Text = "SW_lang"
        SW_lang.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Panel1
        ' 
        Panel1.Location = New Point(428, 160)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(261, 329)
        Panel1.TabIndex = 77
        ' 
        ' PictureBox9
        ' 
        PictureBox9.Anchor = AnchorStyles.Bottom
        PictureBox9.BackColor = Color.Blue
        PictureBox9.BackgroundImageLayout = ImageLayout.None
        PictureBox9.Location = New Point(1220, 739)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(80, 80)
        PictureBox9.TabIndex = 91
        PictureBox9.TabStop = False
        PictureBox9.Visible = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.Blue
        PictureBox1.BackgroundImageLayout = ImageLayout.None
        PictureBox1.Location = New Point(1465, 279)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(80, 80)
        PictureBox1.TabIndex = 92
        PictureBox1.TabStop = False
        PictureBox1.Visible = False
        ' 
        ' Panel2
        ' 
        Panel2.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        Panel2.Controls.Add(text_sub)
        Panel2.Controls.Add(ICON_1)
        Panel2.Location = New Point(0, 190)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(770, 213)
        Panel2.TabIndex = 75
        ' 
        ' Base_Settings
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1545, 819)
        Controls.Add(PictureBox1)
        Controls.Add(PictureBox9)
        Controls.Add(SW_lang)
        Controls.Add(ch)
        Controls.Add(action_fn)
        Controls.Add(settings_top)
        Controls.Add(text_settings)
        Controls.Add(Block_AM)
        Controls.Add(action_1)
        Controls.Add(Back_btn)
        Controls.Add(Main_Menu_SET)
        Controls.Add(Panel1)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Settings"
        ShowInTaskbar = False
        Text = "Privacy control"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        Main_Menu_SET.ResumeLayout(False)
        Panel.ResumeLayout(False)
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(Block_AM, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        Panel2.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents Main_Menu_SET As Panel
    Friend WithEvents Back_btn As Label
    Friend WithEvents text_settings As Label
    Friend WithEvents text_sub As Label
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents action_1 As Label
    Friend WithEvents Block_AM As PictureBox
    Friend WithEvents Panel As Panel
    Friend WithEvents ICON_1 As Label
    Friend WithEvents ch As Label
    Friend WithEvents SW_lang As Label
    Public WithEvents action_fn As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBox9 As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Panel2 As Panel
End Class
