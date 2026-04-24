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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(API_RUN))
        Load_APP = New Timer(components)
        lstLog = New ListBox()
        GroupBox1 = New GroupBox()
        SuspendLayout()
        ' 
        ' Load_APP
        ' 
        Load_APP.Enabled = True
        Load_APP.Interval = 1000
        ' 
        ' lstLog
        ' 
        lstLog.FormattingEnabled = True
        lstLog.IntegralHeight = False
        lstLog.ItemHeight = 15
        lstLog.Location = New Point(12, 30)
        lstLog.Name = "lstLog"
        lstLog.SelectionMode = SelectionMode.MultiSimple
        lstLog.Size = New Size(480, 383)
        lstLog.TabIndex = 1
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Location = New Point(12, 12)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(480, 110)
        GroupBox1.TabIndex = 2
        GroupBox1.TabStop = False
        GroupBox1.Text = "Server log"
        ' 
        ' API_RUN
        ' 
        AutoScaleMode = AutoScaleMode.None
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        ClientSize = New Size(843, 425)
        Controls.Add(lstLog)
        Controls.Add(GroupBox1)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        Name = "API_RUN"
        StartPosition = FormStartPosition.CenterScreen
        Text = "API Server"
        TopMost = True
        ResumeLayout(False)
    End Sub

    Friend WithEvents Load_APP As Timer
    Friend WithEvents lstLog As ListBox
    Friend WithEvents GroupBox1 As GroupBox

End Class
