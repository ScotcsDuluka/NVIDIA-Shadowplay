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
        Label1 = New Label()
        Home_settings = New Label()
        PictureBox2 = New PictureBox()
        Main_Filter = New Panel()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        Main_Filter.SuspendLayout()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.BackColor = Drawing.Color.FromArgb(CByte(1), CByte(0), CByte(1))
        Label1.Cursor = Cursors.Hand
        Label1.Font = New System.Drawing.Font("nvgcshare", 20.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Drawing.Color.White
        Label1.Location = New System.Drawing.Point(212, 6)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(53, 67)
        Label1.TabIndex = 45
        Label1.Text = ""
        Label1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Home_settings
        ' 
        Home_settings.BackColor = Drawing.Color.FromArgb(CByte(1), CByte(0), CByte(1))
        Home_settings.Font = New System.Drawing.Font("Segoe UI Semibold", 15.75F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        Home_settings.ForeColor = Drawing.Color.White
        Home_settings.Location = New System.Drawing.Point(70, 0)
        Home_settings.Name = "Home_settings"
        Home_settings.Size = New System.Drawing.Size(159, 76)
        Home_settings.TabIndex = 44
        Home_settings.Text = "Game filter"
        Home_settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Drawing.Color.FromArgb(CByte(1), CByte(0), CByte(1))
        PictureBox2.BackgroundImage = CType(resources.GetObject("PictureBox2.BackgroundImage"), Drawing.Image)
        PictureBox2.BackgroundImageLayout = ImageLayout.Center
        PictureBox2.Location = New System.Drawing.Point(0, 0)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New System.Drawing.Size(79, 76)
        PictureBox2.TabIndex = 2
        PictureBox2.TabStop = False
        ' 
        ' Main_Filter
        ' 
        Main_Filter.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Main_Filter.BackColor = Drawing.Color.FromArgb(CByte(1), CByte(0), CByte(1))
        Main_Filter.Controls.Add(PictureBox2)
        Main_Filter.Controls.Add(Label1)
        Main_Filter.Controls.Add(Home_settings)
        Main_Filter.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25F)
        Main_Filter.Location = New System.Drawing.Point(0, 0)
        Main_Filter.Name = "Main_Filter"
        Main_Filter.Size = New System.Drawing.Size(268, 719)
        Main_Filter.TabIndex = 46
        ' 
        ' Base_Game_Filter
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.FromArgb(CByte(1), CByte(0), CByte(1))
        ClientSize = New System.Drawing.Size(268, 719)
        Controls.Add(Main_Filter)
        FormBorderStyle = FormBorderStyle.None
        Name = "Base_Game_Filter"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Game_Filter"
        TopMost = True
        TransparencyKey = Drawing.Color.FromArgb(CByte(1), CByte(0), CByte(1))
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        Main_Filter.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents Home_settings As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Main_Filter As Panel
End Class
