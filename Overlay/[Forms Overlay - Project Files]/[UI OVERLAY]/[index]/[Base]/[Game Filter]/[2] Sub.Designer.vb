<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Game_Filter_Sub
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
        BG = New PictureBox()
        CType(BG, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' BG
        ' 
        BG.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        BG.BackColor = SystemColors.ActiveCaptionText
        BG.Location = New Point(-500, 0)
        BG.Name = "BG"
        BG.Size = New Size(268, 717)
        BG.TabIndex = 1
        BG.TabStop = False
        ' 
        ' Base_Game_Filter_Sub
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Coral
        ClientSize = New Size(268, 717)
        Controls.Add(BG)
        FormBorderStyle = FormBorderStyle.None
        Name = "Base_Game_Filter_Sub"
        Opacity = 0R
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Game_Filter_Sub"
        TopMost = True
        TransparencyKey = Color.Coral
        CType(BG, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents BG As PictureBox
End Class
