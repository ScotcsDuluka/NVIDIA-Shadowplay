<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Background_Top
    Inherits System.Windows.Forms.Form

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
        PictureGFE = New PictureBox()
        Logo_text = New Label()
        ac1 = New Panel()
        Bg_Settings = New Label()
        Bg_Gallery = New Label()
        Bg_Share = New Label()
        Bg_Mode3 = New Label()
        Bg_Mode2 = New Label()
        Bg_Mode1 = New Label()
        b3 = New Label()
        b2 = New Label()
        b1 = New Label()
        ANIME = New Timer(components)
        d = New Label()
        ME_CLOSE_BG = New Label()
        ME_CLOSE_BG_GRE = New Label()
        Main_Top.SuspendLayout()
        CType(PictureGFE, ComponentModel.ISupportInitialize).BeginInit()
        ac1.SuspendLayout()
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
        Main_Top.Location = New System.Drawing.Point(0, 0)
        Main_Top.Name = "Main_Top"
        Main_Top.Size = New System.Drawing.Size(1460, 80)
        Main_Top.TabIndex = 10
        ' 
        ' PictureGFE
        ' 
        PictureGFE.BackColor = Drawing.Color.Black
        PictureGFE.BackgroundImage = CType(resources.GetObject("PictureGFE.BackgroundImage"), Drawing.Image)
        PictureGFE.BackgroundImageLayout = ImageLayout.Zoom
        PictureGFE.Location = New System.Drawing.Point(0, 0)
        PictureGFE.Name = "PictureGFE"
        PictureGFE.Size = New System.Drawing.Size(340, 80)
        PictureGFE.TabIndex = 46
        PictureGFE.TabStop = False
        ' 
        ' Logo_text
        ' 
        Logo_text.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Logo_text.BackColor = Drawing.Color.Black
        Logo_text.Font = New System.Drawing.Font("TypeTwo", 24F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Logo_text.ForeColor = Drawing.Color.White
        Logo_text.Location = New System.Drawing.Point(0, 0)
        Logo_text.Name = "Logo_text"
        Logo_text.Size = New System.Drawing.Size(1460, 80)
        Logo_text.TabIndex = 8
        Logo_text.Text = "NVIDIA Shadowplay OBT 1"
        Logo_text.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' ac1
        ' 
        ac1.Controls.Add(Bg_Settings)
        ac1.Controls.Add(Bg_Gallery)
        ac1.Controls.Add(Bg_Share)
        ac1.Controls.Add(Bg_Mode3)
        ac1.Controls.Add(Bg_Mode2)
        ac1.Controls.Add(Bg_Mode1)
        ac1.Controls.Add(b3)
        ac1.Controls.Add(b2)
        ac1.Controls.Add(b1)
        ac1.Location = New System.Drawing.Point(246, 228)
        ac1.Name = "ac1"
        ac1.Size = New System.Drawing.Size(1100, 200)
        ac1.TabIndex = 45
        ' 
        ' Bg_Settings
        ' 
        Bg_Settings.BackColor = Drawing.Color.Black
        Bg_Settings.Cursor = Cursors.Hand
        Bg_Settings.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Bg_Settings.ForeColor = Drawing.Color.White
        Bg_Settings.ImageAlign = Drawing.ContentAlignment.TopCenter
        Bg_Settings.Location = New System.Drawing.Point(900, 132)
        Bg_Settings.Name = "Bg_Settings"
        Bg_Settings.Size = New System.Drawing.Size(200, 66)
        Bg_Settings.TabIndex = 47
        Bg_Settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Bg_Settings.Visible = False
        ' 
        ' Bg_Gallery
        ' 
        Bg_Gallery.BackColor = Drawing.Color.Black
        Bg_Gallery.Cursor = Cursors.Hand
        Bg_Gallery.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Bg_Gallery.ForeColor = Drawing.Color.White
        Bg_Gallery.ImageAlign = Drawing.ContentAlignment.TopCenter
        Bg_Gallery.Location = New System.Drawing.Point(900, 66)
        Bg_Gallery.Name = "Bg_Gallery"
        Bg_Gallery.Size = New System.Drawing.Size(200, 66)
        Bg_Gallery.TabIndex = 46
        Bg_Gallery.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Bg_Gallery.Visible = False
        ' 
        ' Bg_Share
        ' 
        Bg_Share.BackColor = Drawing.Color.Black
        Bg_Share.Cursor = Cursors.Hand
        Bg_Share.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Bg_Share.ForeColor = Drawing.Color.White
        Bg_Share.ImageAlign = Drawing.ContentAlignment.TopCenter
        Bg_Share.Location = New System.Drawing.Point(900, 0)
        Bg_Share.Name = "Bg_Share"
        Bg_Share.Size = New System.Drawing.Size(200, 66)
        Bg_Share.TabIndex = 45
        Bg_Share.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Bg_Share.Visible = False
        ' 
        ' Bg_Mode3
        ' 
        Bg_Mode3.BackColor = Drawing.Color.Black
        Bg_Mode3.Cursor = Cursors.Hand
        Bg_Mode3.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Bg_Mode3.ForeColor = Drawing.Color.White
        Bg_Mode3.ImageAlign = Drawing.ContentAlignment.TopCenter
        Bg_Mode3.Location = New System.Drawing.Point(0, 132)
        Bg_Mode3.Name = "Bg_Mode3"
        Bg_Mode3.Size = New System.Drawing.Size(200, 66)
        Bg_Mode3.TabIndex = 44
        Bg_Mode3.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Bg_Mode3.Visible = False
        ' 
        ' Bg_Mode2
        ' 
        Bg_Mode2.BackColor = Drawing.Color.Black
        Bg_Mode2.Cursor = Cursors.Hand
        Bg_Mode2.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Bg_Mode2.ForeColor = Drawing.Color.White
        Bg_Mode2.ImageAlign = Drawing.ContentAlignment.TopCenter
        Bg_Mode2.Location = New System.Drawing.Point(0, 66)
        Bg_Mode2.Name = "Bg_Mode2"
        Bg_Mode2.Size = New System.Drawing.Size(200, 66)
        Bg_Mode2.TabIndex = 43
        Bg_Mode2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Bg_Mode2.Visible = False
        ' 
        ' Bg_Mode1
        ' 
        Bg_Mode1.BackColor = Drawing.Color.Black
        Bg_Mode1.Cursor = Cursors.Hand
        Bg_Mode1.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Bg_Mode1.ForeColor = Drawing.Color.White
        Bg_Mode1.ImageAlign = Drawing.ContentAlignment.TopCenter
        Bg_Mode1.Location = New System.Drawing.Point(0, 0)
        Bg_Mode1.Name = "Bg_Mode1"
        Bg_Mode1.Size = New System.Drawing.Size(200, 66)
        Bg_Mode1.TabIndex = 42
        Bg_Mode1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Bg_Mode1.Visible = False
        ' 
        ' b3
        ' 
        b3.Anchor = AnchorStyles.Top
        b3.BackColor = Drawing.Color.Black
        b3.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        b3.ForeColor = Drawing.Color.White
        b3.ImageAlign = Drawing.ContentAlignment.TopCenter
        b3.Location = New System.Drawing.Point(650, 0)
        b3.Name = "b3"
        b3.Size = New System.Drawing.Size(200, 200)
        b3.TabIndex = 41
        b3.TextAlign = Drawing.ContentAlignment.MiddleCenter
        b3.Visible = False
        ' 
        ' b2
        ' 
        b2.Anchor = AnchorStyles.Top
        b2.BackColor = Drawing.Color.Black
        b2.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        b2.ForeColor = Drawing.Color.Gray
        b2.ImageAlign = Drawing.ContentAlignment.TopCenter
        b2.Location = New System.Drawing.Point(450, 0)
        b2.Name = "b2"
        b2.Size = New System.Drawing.Size(200, 200)
        b2.TabIndex = 41
        b2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        b2.Visible = False
        ' 
        ' b1
        ' 
        b1.Anchor = AnchorStyles.Top
        b1.BackColor = Drawing.Color.Black
        b1.Font = New System.Drawing.Font("nvgcshare", 80F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        b1.ForeColor = Drawing.Color.Green
        b1.ImageAlign = Drawing.ContentAlignment.TopCenter
        b1.Location = New System.Drawing.Point(250, 0)
        b1.Name = "b1"
        b1.Size = New System.Drawing.Size(200, 200)
        b1.TabIndex = 41
        b1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        b1.Visible = False
        ' 
        ' ANIME
        ' 
        ANIME.Enabled = True
        ANIME.Interval = 1
        ' 
        ' d
        ' 
        d.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        d.BackColor = Drawing.Color.Black
        d.Cursor = Cursors.Hand
        d.Font = New System.Drawing.Font("nvgcshare", 26.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        d.ForeColor = Drawing.Color.White
        d.Location = New System.Drawing.Point(1406, 23)
        d.Name = "d"
        d.Size = New System.Drawing.Size(28, 34)
        d.TabIndex = 89
        d.Text = ""
        d.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG
        ' 
        ME_CLOSE_BG.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG.BackColor = Drawing.Color.Black
        ME_CLOSE_BG.Cursor = Cursors.Hand
        ME_CLOSE_BG.Font = New System.Drawing.Font("nvgcshare", 26.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG.ForeColor = Drawing.Color.White
        ME_CLOSE_BG.Location = New System.Drawing.Point(1402, 23)
        ME_CLOSE_BG.Name = "ME_CLOSE_BG"
        ME_CLOSE_BG.Size = New System.Drawing.Size(34, 34)
        ME_CLOSE_BG.TabIndex = 88
        ME_CLOSE_BG.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG_GRE
        ' 
        ME_CLOSE_BG_GRE.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG_GRE.BackColor = Drawing.Color.Black
        ME_CLOSE_BG_GRE.Cursor = Cursors.No
        ME_CLOSE_BG_GRE.Font = New System.Drawing.Font("nvgcshare", 26.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG_GRE.ForeColor = Drawing.Color.White
        ME_CLOSE_BG_GRE.Location = New System.Drawing.Point(1399, 20)
        ME_CLOSE_BG_GRE.Name = "ME_CLOSE_BG_GRE"
        ME_CLOSE_BG_GRE.Size = New System.Drawing.Size(40, 40)
        ME_CLOSE_BG_GRE.TabIndex = 87
        ME_CLOSE_BG_GRE.Text = ""
        ME_CLOSE_BG_GRE.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Base_Background_Top
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Blue
        ClientSize = New System.Drawing.Size(1460, 768)
        ControlBox = False
        Controls.Add(ac1)
        Controls.Add(Main_Top)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Base_Background_Top"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Background Top"
        TopMost = True
        TransparencyKey = Drawing.Color.Blue
        WindowState = FormWindowState.Maximized
        Main_Top.ResumeLayout(False)
        CType(PictureGFE, ComponentModel.ISupportInitialize).EndInit()
        ac1.ResumeLayout(False)
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
    Friend WithEvents Bg_Mode2 As Label
    Friend WithEvents Bg_Mode3 As Label
    Friend WithEvents PictureGFE As PictureBox
    Friend WithEvents d As Label
    Friend WithEvents ME_CLOSE_BG As Label
    Friend WithEvents ME_CLOSE_BG_GRE As Label
End Class
