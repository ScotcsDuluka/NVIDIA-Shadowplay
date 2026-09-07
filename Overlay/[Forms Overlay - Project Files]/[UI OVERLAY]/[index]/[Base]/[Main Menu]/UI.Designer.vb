#If DEBUG Then
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Debug_UI
    Inherits System.Windows.Forms.Form

    
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

    
    Private components As System.ComponentModel.IContainer

    
    
    
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Debug_UI))
        SuspendLayout()
        
        
        
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1264, 681)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MinimumSize = New Size(1280, 720)
        Name = "Debug_UI"
        Text = "index"
        ResumeLayout(False)
    End Sub
End Class
#End If