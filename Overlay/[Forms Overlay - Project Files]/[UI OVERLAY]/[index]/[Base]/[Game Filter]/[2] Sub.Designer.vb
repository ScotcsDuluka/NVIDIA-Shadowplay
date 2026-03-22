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
        BG = New Panel()
        d = New Label()
        ME_CLOSE_BG = New Label()
        ME_CLOSE_BG_GRE = New Label()
        BG.SuspendLayout()
        SuspendLayout()
        ' 
        ' BG
        ' 
        BG.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        BG.BackColor = Color.Black
        BG.Controls.Add(d)
        BG.Controls.Add(ME_CLOSE_BG)
        BG.Controls.Add(ME_CLOSE_BG_GRE)
        BG.Location = New Point(0, 0)
        BG.Name = "BG"
        BG.Size = New Size(268, 717)
        BG.TabIndex = 1
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
        d.TabIndex = 96
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
        ME_CLOSE_BG.Location = New Point(222, 21)
        ME_CLOSE_BG.Name = "ME_CLOSE_BG"
        ME_CLOSE_BG.Size = New Size(34, 34)
        ME_CLOSE_BG.TabIndex = 95
        ME_CLOSE_BG.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ME_CLOSE_BG_GRE
        ' 
        ME_CLOSE_BG_GRE.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        ME_CLOSE_BG_GRE.BackColor = Color.Black
        ME_CLOSE_BG_GRE.Cursor = Cursors.Hand
        ME_CLOSE_BG_GRE.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ME_CLOSE_BG_GRE.ForeColor = Color.White
        ME_CLOSE_BG_GRE.Location = New Point(219, 18)
        ME_CLOSE_BG_GRE.Name = "ME_CLOSE_BG_GRE"
        ME_CLOSE_BG_GRE.Size = New Size(40, 40)
        ME_CLOSE_BG_GRE.TabIndex = 94
        ME_CLOSE_BG_GRE.TextAlign = ContentAlignment.MiddleCenter
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
        BG.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents BG As Panel
    Friend WithEvents ME_CLOSE_BG As Label
    Friend WithEvents ME_CLOSE_BG_GRE As Label
    Friend WithEvents d As Label
End Class
