<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Notifier_Sub
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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Notifier_Sub))
        Timer1 = New Timer(components)
        SuspendLayout()
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 1000
        ' 
        ' overlay_sub
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Black
        CausesValidation = False
        ClientSize = New System.Drawing.Size(295, 90)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "overlay_sub"
        Text = "Notifier"
        TopMost = True
        ResumeLayout(False)
    End Sub

    Friend WithEvents Timer1 As Timer
End Class
