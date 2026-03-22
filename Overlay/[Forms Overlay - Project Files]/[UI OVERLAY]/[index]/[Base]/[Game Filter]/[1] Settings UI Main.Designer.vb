<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_Game_Filter
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Game_Filter))
        Home_settings = New Label()
        PictureBox2 = New PictureBox()
        Main_Filter = New Panel()
        d = New Label()
        ME_CLOSE_BG = New Label()
        ME_CLOSE_BG_GRE = New Label()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        Main_Filter.SuspendLayout()
        SuspendLayout()
        ' 
        ' Home_settings
        ' 
        Home_settings.BackColor = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        Home_settings.Font = New Font("Segoe UI Semibold", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Home_settings.ForeColor = Color.White
        Home_settings.Location = New Point(70, 0)
        Home_settings.Name = "Home_settings"
        Home_settings.Size = New Size(143, 76)
        Home_settings.TabIndex = 44
        Home_settings.Text = "Game filter"
        Home_settings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        PictureBox2.BackgroundImage = CType(resources.GetObject("PictureBox2.BackgroundImage"), Image)
        PictureBox2.BackgroundImageLayout = ImageLayout.Center
        PictureBox2.Location = New Point(0, 0)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(79, 76)
        PictureBox2.TabIndex = 2
        PictureBox2.TabStop = False
        ' 
        ' Main_Filter
        ' 
        Main_Filter.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Main_Filter.BackColor = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        Main_Filter.Controls.Add(d)
        Main_Filter.Controls.Add(ME_CLOSE_BG)
        Main_Filter.Controls.Add(ME_CLOSE_BG_GRE)
        Main_Filter.Controls.Add(PictureBox2)
        Main_Filter.Controls.Add(Home_settings)
        Main_Filter.Font = New Font("Microsoft Sans Serif", 8.25F)
        Main_Filter.Location = New Point(0, 0)
        Main_Filter.Name = "Main_Filter"
        Main_Filter.Size = New Size(268, 719)
        Main_Filter.TabIndex = 46
        ' 
        ' d
        ' 
        d.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        d.BackColor = Color.Black
        d.Cursor = Cursors.Hand
        d.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        d.ForeColor = Color.White
        d.Location = New Point(226, 21)
        d.Name = "d"
        d.Size = New Size(28, 34)
        d.TabIndex = 98
        d.Text = ""
        d.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG
        ' 
        ME_CLOSE_BG.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG.BackColor = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        ME_CLOSE_BG.Cursor = Cursors.Hand
        ME_CLOSE_BG.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG.ForeColor = Color.White
        ME_CLOSE_BG.Location = New Point(222, 21)
        ME_CLOSE_BG.Name = "ME_CLOSE_BG"
        ME_CLOSE_BG.Size = New Size(34, 34)
        ME_CLOSE_BG.TabIndex = 97
        ME_CLOSE_BG.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG_GRE
        ' 
        ME_CLOSE_BG_GRE.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG_GRE.BackColor = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        ME_CLOSE_BG_GRE.Cursor = Cursors.Hand
        ME_CLOSE_BG_GRE.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG_GRE.ForeColor = Color.White
        ME_CLOSE_BG_GRE.Location = New Point(219, 18)
        ME_CLOSE_BG_GRE.Name = "ME_CLOSE_BG_GRE"
        ME_CLOSE_BG_GRE.Size = New Size(40, 40)
        ME_CLOSE_BG_GRE.TabIndex = 96
        ME_CLOSE_BG_GRE.Text = ""
        ME_CLOSE_BG_GRE.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Base_Game_Filter
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        ClientSize = New Size(268, 719)
        Controls.Add(Main_Filter)
        FormBorderStyle = FormBorderStyle.None
        Name = "Base_Game_Filter"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Game_Filter"
        TopMost = True
        TransparencyKey = Color.FromArgb(CByte(1), CByte(0), CByte(1))
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        Main_Filter.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents Home_settings As Label
    Friend WithEvents Main_Filter As Panel
    Friend WithEvents ME_CLOSE_BG As Label
    Friend WithEvents ME_CLOSE_BG_GRE As Label
    Friend WithEvents d As Label
End Class
