<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Shadow
    Inherits BlockClose

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
        components = New ComponentModel.Container()
        Timer1 = New Timer(components)
        SuspendLayout()
        ' 
        ' Timer1
        ' 
        Timer1.Enabled = True
        Timer1.Interval = 1
        ' 
        ' Shadow
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ClientSize = New Size(300, 90)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Name = "Shadow"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "z"
        TopMost = True
        ResumeLayout(False)
    End Sub

    Friend WithEvents Timer1 As Timer
End Class
