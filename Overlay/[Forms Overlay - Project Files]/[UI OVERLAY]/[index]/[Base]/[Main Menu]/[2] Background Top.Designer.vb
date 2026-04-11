<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Background_Top
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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Background_Top))
        Main_Top = New Panel()
        d = New Label()
        ME_CLOSE_BG = New Label()
        ME_CLOSE_BG_GRE = New Label()
        PictureGFE = New PictureBox()
        Logo_text = New Label()
        ANIME = New Timer(components)
        Main_menu_list = New Panel()
        b1_all = New Label()
        b2_all = New Label()
        Mode_u = New Panel()
        s_3r = New PictureBox()
        Bg_Mode1 = New Label()
        Bg_Mode2 = New Label()
        Bg_Mode3 = New Label()
        b1 = New Label()
        Bg_SET2 = New Label()
        Bg_SET1 = New Label()
        b3 = New Label()
        b2 = New Label()
        Bg_SET3 = New Label()
        Main_Top.SuspendLayout()
        CType(PictureGFE, ComponentModel.ISupportInitialize).BeginInit()
        Main_menu_list.SuspendLayout()
        Mode_u.SuspendLayout()
        CType(s_3r, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Main_Top
        ' 
        Main_Top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Main_Top.Controls.Add(d)
        Main_Top.Controls.Add(ME_CLOSE_BG)
        Main_Top.Controls.Add(ME_CLOSE_BG_GRE)
        Main_Top.Controls.Add(PictureGFE)
        Main_Top.Controls.Add(Logo_text)
        Main_Top.Location = New Point(0, 0)
        Main_Top.Name = "Main_Top"
        Main_Top.Size = New Size(1460, 80)
        Main_Top.TabIndex = 10
        ' 
        ' d
        ' 
        d.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        d.BackColor = Color.Black
        d.Cursor = Cursors.Hand
        d.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        d.ForeColor = Color.White
        d.Location = New Point(1406, 23)
        d.Name = "d"
        d.Size = New Size(28, 34)
        d.TabIndex = 89
        d.Text = ""
        d.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG
        ' 
        ME_CLOSE_BG.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG.BackColor = Color.Black
        ME_CLOSE_BG.Cursor = Cursors.Hand
        ME_CLOSE_BG.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG.ForeColor = Color.White
        ME_CLOSE_BG.Location = New Point(1402, 23)
        ME_CLOSE_BG.Name = "ME_CLOSE_BG"
        ME_CLOSE_BG.Size = New Size(34, 34)
        ME_CLOSE_BG.TabIndex = 88
        ME_CLOSE_BG.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG_GRE
        ' 
        ME_CLOSE_BG_GRE.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG_GRE.BackColor = Color.Black
        ME_CLOSE_BG_GRE.Cursor = Cursors.No
        ME_CLOSE_BG_GRE.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG_GRE.ForeColor = Color.White
        ME_CLOSE_BG_GRE.Location = New Point(1399, 20)
        ME_CLOSE_BG_GRE.Name = "ME_CLOSE_BG_GRE"
        ME_CLOSE_BG_GRE.Size = New Size(40, 40)
        ME_CLOSE_BG_GRE.TabIndex = 87
        ME_CLOSE_BG_GRE.Text = ""
        ME_CLOSE_BG_GRE.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PictureGFE
        ' 
        PictureGFE.BackColor = Color.Black
        PictureGFE.BackgroundImage = My.Resources.Resources.osc_img_appicon_64x64
        PictureGFE.BackgroundImageLayout = ImageLayout.None
        PictureGFE.Location = New Point(8, 8)
        PictureGFE.Name = "PictureGFE"
        PictureGFE.Size = New Size(64, 64)
        PictureGFE.TabIndex = 46
        PictureGFE.TabStop = False
        ' 
        ' Logo_text
        ' 
        Logo_text.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Logo_text.BackColor = Color.Black
        Logo_text.Font = New Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Logo_text.ForeColor = Color.White
        Logo_text.Location = New Point(0, 0)
        Logo_text.Name = "Logo_text"
        Logo_text.Size = New Size(1460, 80)
        Logo_text.TabIndex = 8
        Logo_text.Text = "NVIDIA Shadowplay OBT 1"
        Logo_text.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ANIME
        ' 
        ANIME.Enabled = True
        ANIME.Interval = 16
        ' 
        ' Main_menu_list
        ' 
        Main_menu_list.BackColor = Color.Blue
        Main_menu_list.Controls.Add(b1_all)
        Main_menu_list.Controls.Add(b2_all)
        Main_menu_list.Controls.Add(Mode_u)
        Main_menu_list.Controls.Add(b1)
        Main_menu_list.Controls.Add(Bg_SET2)
        Main_menu_list.Controls.Add(Bg_SET1)
        Main_menu_list.Controls.Add(b3)
        Main_menu_list.Controls.Add(b2)
        Main_menu_list.Controls.Add(Bg_SET3)
        Main_menu_list.Location = New Point(101, 644)
        Main_menu_list.Name = "Main_menu_list"
        Main_menu_list.Size = New Size(1280, 483)
        Main_menu_list.TabIndex = 46
        ' 
        ' b1_all
        ' 
        b1_all.BackColor = Color.Black
        b1_all.Cursor = Cursors.Hand
        b1_all.Font = New Font("nvgcshare", 90F)
        b1_all.ForeColor = Color.White
        b1_all.ImageAlign = ContentAlignment.TopCenter
        b1_all.Location = New Point(280, 0)
        b1_all.Name = "b1_all"
        b1_all.Size = New Size(240, 373)
        b1_all.TabIndex = 86
        b1_all.TextAlign = ContentAlignment.MiddleCenter
        b1_all.Visible = False
        ' 
        ' b2_all
        ' 
        b2_all.BackColor = Color.Black
        b2_all.Cursor = Cursors.Hand
        b2_all.Font = New Font("nvgcshare", 90F)
        b2_all.ForeColor = Color.White
        b2_all.ImageAlign = ContentAlignment.TopCenter
        b2_all.Location = New Point(520, 0)
        b2_all.Name = "b2_all"
        b2_all.Size = New Size(240, 329)
        b2_all.TabIndex = 85
        b2_all.TextAlign = ContentAlignment.MiddleCenter
        b2_all.Visible = False
        ' 
        ' Mode_u
        ' 
        Mode_u.Controls.Add(s_3r)
        Mode_u.Controls.Add(Bg_Mode1)
        Mode_u.Controls.Add(Bg_Mode2)
        Mode_u.Controls.Add(Bg_Mode3)
        Mode_u.Location = New Point(0, 0)
        Mode_u.Name = "Mode_u"
        Mode_u.Size = New Size(240, 240)
        Mode_u.TabIndex = 84
        ' 
        ' s_3r
        ' 
        s_3r.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        s_3r.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        s_3r.Location = New Point(0, 300)
        s_3r.Name = "s_3r"
        s_3r.Size = New Size(3, 80)
        s_3r.TabIndex = 62
        s_3r.TabStop = False
        s_3r.Visible = False
        ' 
        ' Bg_Mode1
        ' 
        Bg_Mode1.BackColor = Color.Black
        Bg_Mode1.Cursor = Cursors.Hand
        Bg_Mode1.Font = New Font("Microsoft Sans Serif", 80F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Bg_Mode1.ForeColor = Color.White
        Bg_Mode1.ImageAlign = ContentAlignment.TopCenter
        Bg_Mode1.Location = New Point(0, 0)
        Bg_Mode1.Name = "Bg_Mode1"
        Bg_Mode1.Size = New Size(240, 80)
        Bg_Mode1.TabIndex = 29
        Bg_Mode1.TextAlign = ContentAlignment.MiddleCenter
        Bg_Mode1.Visible = False
        ' 
        ' Bg_Mode2
        ' 
        Bg_Mode2.BackColor = Color.Black
        Bg_Mode2.Cursor = Cursors.Hand
        Bg_Mode2.Font = New Font("Microsoft Sans Serif", 80F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Bg_Mode2.ForeColor = Color.White
        Bg_Mode2.ImageAlign = ContentAlignment.TopCenter
        Bg_Mode2.Location = New Point(0, 80)
        Bg_Mode2.Name = "Bg_Mode2"
        Bg_Mode2.Size = New Size(240, 80)
        Bg_Mode2.TabIndex = 31
        Bg_Mode2.TextAlign = ContentAlignment.MiddleCenter
        Bg_Mode2.Visible = False
        ' 
        ' Bg_Mode3
        ' 
        Bg_Mode3.BackColor = Color.Black
        Bg_Mode3.Cursor = Cursors.Hand
        Bg_Mode3.Font = New Font("Microsoft Sans Serif", 80F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Bg_Mode3.ForeColor = Color.White
        Bg_Mode3.ImageAlign = ContentAlignment.TopCenter
        Bg_Mode3.Location = New Point(0, 160)
        Bg_Mode3.Name = "Bg_Mode3"
        Bg_Mode3.Size = New Size(240, 80)
        Bg_Mode3.TabIndex = 30
        Bg_Mode3.TextAlign = ContentAlignment.MiddleCenter
        Bg_Mode3.Visible = False
        ' 
        ' b1
        ' 
        b1.BackColor = Color.Black
        b1.Cursor = Cursors.Hand
        b1.Font = New Font("nvgcshare", 90F)
        b1.ForeColor = Color.White
        b1.ImageAlign = ContentAlignment.TopCenter
        b1.Location = New Point(280, 0)
        b1.Name = "b1"
        b1.Size = New Size(240, 240)
        b1.TabIndex = 41
        b1.TextAlign = ContentAlignment.MiddleCenter
        b1.Visible = False
        ' 
        ' Bg_SET2
        ' 
        Bg_SET2.BackColor = Color.Black
        Bg_SET2.Cursor = Cursors.Hand
        Bg_SET2.Font = New Font("Microsoft Sans Serif", 80F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Bg_SET2.ForeColor = Color.White
        Bg_SET2.ImageAlign = ContentAlignment.TopCenter
        Bg_SET2.Location = New Point(1040, 80)
        Bg_SET2.Name = "Bg_SET2"
        Bg_SET2.Size = New Size(240, 80)
        Bg_SET2.TabIndex = 16
        Bg_SET2.TextAlign = ContentAlignment.MiddleCenter
        Bg_SET2.Visible = False
        ' 
        ' Bg_SET1
        ' 
        Bg_SET1.BackColor = Color.Black
        Bg_SET1.Cursor = Cursors.Hand
        Bg_SET1.Font = New Font("Microsoft Sans Serif", 80F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Bg_SET1.ForeColor = Color.White
        Bg_SET1.ImageAlign = ContentAlignment.TopCenter
        Bg_SET1.Location = New Point(1040, 0)
        Bg_SET1.Name = "Bg_SET1"
        Bg_SET1.Size = New Size(240, 80)
        Bg_SET1.TabIndex = 14
        Bg_SET1.TextAlign = ContentAlignment.MiddleCenter
        Bg_SET1.Visible = False
        ' 
        ' b3
        ' 
        b3.BackColor = Color.Black
        b3.Cursor = Cursors.Hand
        b3.Font = New Font("nvgcshare", 90F)
        b3.ForeColor = Color.White
        b3.ImageAlign = ContentAlignment.TopCenter
        b3.Location = New Point(760, 0)
        b3.Name = "b3"
        b3.Size = New Size(240, 240)
        b3.TabIndex = 13
        b3.TextAlign = ContentAlignment.MiddleCenter
        b3.Visible = False
        ' 
        ' b2
        ' 
        b2.BackColor = Color.Black
        b2.Cursor = Cursors.Hand
        b2.Font = New Font("nvgcshare", 90F)
        b2.ForeColor = Color.White
        b2.ImageAlign = ContentAlignment.TopCenter
        b2.Location = New Point(520, 0)
        b2.Name = "b2"
        b2.Size = New Size(240, 240)
        b2.TabIndex = 11
        b2.TextAlign = ContentAlignment.MiddleCenter
        b2.Visible = False
        ' 
        ' Bg_SET3
        ' 
        Bg_SET3.BackColor = Color.Black
        Bg_SET3.Cursor = Cursors.Hand
        Bg_SET3.Font = New Font("Microsoft Sans Serif", 80F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Bg_SET3.ForeColor = Color.White
        Bg_SET3.ImageAlign = ContentAlignment.TopCenter
        Bg_SET3.Location = New Point(1040, 160)
        Bg_SET3.Name = "Bg_SET3"
        Bg_SET3.Size = New Size(240, 80)
        Bg_SET3.TabIndex = 76
        Bg_SET3.TextAlign = ContentAlignment.MiddleCenter
        Bg_SET3.Visible = False
        ' 
        ' Base_Background_Top
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Blue
        ClientSize = New Size(1460, 1165)
        ControlBox = False
        Controls.Add(Main_menu_list)
        Controls.Add(Main_Top)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Background_Top"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Background Top"
        TopMost = True
        TransparencyKey = Color.Blue
        WindowState = FormWindowState.Maximized
        Main_Top.ResumeLayout(False)
        CType(PictureGFE, ComponentModel.ISupportInitialize).EndInit()
        Main_menu_list.ResumeLayout(False)
        Mode_u.ResumeLayout(False)
        CType(s_3r, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Main_Top As Panel
    Friend WithEvents ac1 As Panel
    Friend WithEvents b3 As Label
    Friend WithEvents b2 As Label
    Friend WithEvents b1 As Label
    Friend WithEvents Logo_text As Label
    Friend WithEvents ANIME As Timer
    Friend WithEvents Bg_Settings As Label
    Friend WithEvents Bg_Gallery As Label
    Friend WithEvents Bg_Share As Label
    Friend WithEvents Bg_Mode1 As Label
    Friend WithEvents PictureGFE As PictureBox
    Friend WithEvents d As Label
    Friend WithEvents ME_CLOSE_BG As Label
    Friend WithEvents ME_CLOSE_BG_GRE As Label
    Friend WithEvents action_sc As Panel
    Friend WithEvents sub_record As Panel
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents sub_replay As Panel
    Friend WithEvents replay_sc As PictureBox
    Friend WithEvents replay_sc1 As PictureBox
    Friend WithEvents Main_menu_list As Panel
    Friend WithEvents Mode_u As Panel
    Friend WithEvents s_3r As PictureBox
    Friend WithEvents Bg_Mode2 As Label
    Friend WithEvents Bg_Mode3 As Label
    Friend WithEvents logo_replay As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents logo_live As Label
    Friend WithEvents logo_record As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents sub_replay_setodv As PictureBox
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents Bg_SET1 As Label
    Friend WithEvents Bg_SET2 As Label
    Friend WithEvents Bg_SET3 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents b2_all As Label
    Friend WithEvents b1_all As Label
End Class
