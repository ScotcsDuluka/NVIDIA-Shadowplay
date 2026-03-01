<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_Game_Filter_Sub
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
        BG = New PictureBox()
        CType(BG, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' BG
        ' 
        BG.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        BG.BackColor = Drawing.SystemColors.ActiveCaptionText
        BG.Location = New System.Drawing.Point(-500, 0)
        BG.Name = "BG"
        BG.Size = New System.Drawing.Size(268, 717)
        BG.TabIndex = 1
        BG.TabStop = False
        ' 
        ' Base_Game_Filter_Sub
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Coral
        ClientSize = New System.Drawing.Size(268, 717)
        Controls.Add(BG)
        FormBorderStyle = FormBorderStyle.None
        Name = "Base_Game_Filter_Sub"
        Opacity = 0R
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Game_Filter_Sub"
        TopMost = True
        TransparencyKey = Drawing.Color.Coral
        CType(BG, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents BG As PictureBox
End Class
