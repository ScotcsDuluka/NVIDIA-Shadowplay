<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_Game_Filter
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Game_Filter))
        Main_Filter = New Panel()
        Label1 = New Label()
        Home_settings = New Label()
        PictureBox2 = New PictureBox()
        PictureBox1 = New PictureBox()
        settings_top = New PictureBox()
        Main_Filter.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Main_Filter
        ' 
        Main_Filter.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        Main_Filter.BackColor = Drawing.SystemColors.AppWorkspace
        Main_Filter.Controls.Add(settings_top)
        Main_Filter.Controls.Add(Label1)
        Main_Filter.Controls.Add(Home_settings)
        Main_Filter.Controls.Add(PictureBox2)
        Main_Filter.Controls.Add(PictureBox1)
        Main_Filter.Location = New System.Drawing.Point(0, 0)
        Main_Filter.Name = "Main_Filter"
        Main_Filter.Size = New System.Drawing.Size(268, 719)
        Main_Filter.TabIndex = 1
        ' 
        ' Label1
        ' 
        Label1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Cursor = Cursors.Hand
        Label1.Font = New System.Drawing.Font("nvgcshare", 20.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Drawing.Color.White
        Label1.Location = New System.Drawing.Point(212, 27)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(33, 32)
        Label1.TabIndex = 45
        Label1.Text = ""
        Label1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Home_settings
        ' 
        Home_settings.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Home_settings.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        Home_settings.ForeColor = Drawing.Color.White
        Home_settings.Location = New System.Drawing.Point(85, 5)
        Home_settings.Name = "Home_settings"
        Home_settings.Size = New System.Drawing.Size(108, 76)
        Home_settings.TabIndex = 44
        Home_settings.Text = "Game filter"
        Home_settings.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox2.BackgroundImage = CType(resources.GetObject("PictureBox2.BackgroundImage"), Drawing.Image)
        PictureBox2.BackgroundImageLayout = ImageLayout.Center
        PictureBox2.Location = New System.Drawing.Point(0, 5)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New System.Drawing.Size(79, 76)
        PictureBox2.TabIndex = 2
        PictureBox2.TabStop = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox1.Location = New System.Drawing.Point(0, 5)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New System.Drawing.Size(268, 76)
        PictureBox1.TabIndex = 46
        PictureBox1.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(0, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(269, 5)
        settings_top.TabIndex = 47
        settings_top.TabStop = False
        ' 
        ' Base_Game_Filter
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.SystemColors.AppWorkspace
        ClientSize = New System.Drawing.Size(268, 719)
        Controls.Add(Main_Filter)
        FormBorderStyle = FormBorderStyle.None
        Name = "Base_Game_Filter"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Game_Filter"
        TopMost = True
        TransparencyKey = Drawing.SystemColors.AppWorkspace
        Main_Filter.ResumeLayout(False)
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents Main_Filter As Panel
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents Home_settings As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents settings_top As PictureBox
End Class
