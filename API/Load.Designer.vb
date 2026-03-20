<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class API_RUN
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
        components = New ComponentModel.Container()
        Load_APP = New Timer(components)
        SuspendLayout()
        ' 
        ' Load_APP
        ' 
        Load_APP.Enabled = True
        Load_APP.Interval = 1000
        ' 
        ' API_RUN
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        ClientSize = New Size(143, 137)
        ControlBox = False
        Enabled = False
        FormBorderStyle = FormBorderStyle.None
        Name = "API_RUN"
        Opacity = 0R
        ShowIcon = False
        ShowInTaskbar = False
        Text = "NVIDIA API"
        TransparencyKey = SystemColors.Control
        WindowState = FormWindowState.Minimized
        ResumeLayout(False)
    End Sub

    Friend WithEvents Load_APP As Timer

End Class
