<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Shadow2
    Inherits Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Shadow2))
        Timer1 = New Timer(components)
        HideShadow = New Timer(components)
        SuspendLayout()
        ' 
        ' Timer1
        ' 
        Timer1.Enabled = True
        Timer1.Interval = 16
        ' 
        ' HideShadow
        ' 
        HideShadow.Enabled = True
        HideShadow.Interval = 1
        ' 
        ' Shadow2
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        AutoValidate = AutoValidate.Disable
        BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ClientSize = New Size(300, 90)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Shadow2"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Shadow2"
        TopMost = True
        ResumeLayout(False)
    End Sub

    Friend WithEvents Timer1 As Timer
    Friend WithEvents HideShadow As Timer
End Class
