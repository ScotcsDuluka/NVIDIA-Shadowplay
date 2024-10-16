Imports System.Drawing
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FPS
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FPS))
        run = New Timer(components)
        lblFPS = New Label()
        Timer1 = New Timer(components)
        SuspendLayout()
        ' 
        ' run
        ' 
        ' 
        ' lblFPS
        ' 
        lblFPS.AutoSize = True
        lblFPS.BackColor = Color.LightCoral
        lblFPS.Font = New Font("Segoe UI", 15F, FontStyle.Bold)
        lblFPS.ForeColor = Color.Red
        lblFPS.Location = New Point(0, 0)
        lblFPS.Name = "lblFPS"
        lblFPS.Size = New Size(45, 28)
        lblFPS.TabIndex = 46
        lblFPS.Text = "FPS"
        ' 
        ' per
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.LightCoral
        ClientSize = New Size(1048, 781)
        Controls.Add(lblFPS)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "per"
        StartPosition = FormStartPosition.Manual
        Text = "Performances"
        TopMost = True
        TransparencyKey = Color.LightCoral
        WindowState = FormWindowState.Maximized
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents run As Timer
    Friend WithEvents lblFPS As Label
    Friend WithEvents Timer1 As Timer
End Class
