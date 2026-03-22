Imports System.Drawing

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Notifier
    Inherits BlockClose

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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Notifier))
        Notifier_green = New Panel()
        text_n = New Label()
        icon_n = New Label()
        Notifier_black = New Panel()
        Notifier_green_stop = New PictureBox()
        Animation_Engine = New Timer(components)
        IF_N = New Timer(components)
        Notifier_black.SuspendLayout()
        CType(Notifier_green_stop, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Notifier_green
        ' 
        Notifier_green.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Notifier_green.Location = New Point(0, 0)
        Notifier_green.Name = "Notifier_green"
        Notifier_green.Size = New Size(300, 90)
        Notifier_green.TabIndex = 0
        ' 
        ' text_n
        ' 
        text_n.BackColor = Color.Black
        text_n.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        text_n.ForeColor = Color.White
        text_n.Location = New Point(99, 0)
        text_n.Name = "text_n"
        text_n.Size = New Size(200, 90)
        text_n.TabIndex = 4
        text_n.Text = "{{text}}"
        text_n.TextAlign = ContentAlignment.MiddleLeft
        text_n.Visible = False
        ' 
        ' icon_n
        ' 
        icon_n.BackColor = Color.Black
        icon_n.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        icon_n.ForeColor = Color.White
        icon_n.Location = New Point(7, 0)
        icon_n.Name = "icon_n"
        icon_n.Size = New Size(96, 90)
        icon_n.TabIndex = 3
        icon_n.Text = "{}     "
        icon_n.TextAlign = ContentAlignment.MiddleCenter
        icon_n.Visible = False
        ' 
        ' Notifier_black
        ' 
        Notifier_black.BackColor = Color.Black
        Notifier_black.Controls.Add(text_n)
        Notifier_black.Controls.Add(Notifier_green_stop)
        Notifier_black.Controls.Add(icon_n)
        Notifier_black.ForeColor = Color.White
        Notifier_black.Location = New Point(0, 90)
        Notifier_black.Name = "Notifier_black"
        Notifier_black.Size = New Size(300, 90)
        Notifier_black.TabIndex = 1
        ' 
        ' Notifier_green_stop
        ' 
        Notifier_green_stop.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Notifier_green_stop.Location = New Point(0, 0)
        Notifier_green_stop.Name = "Notifier_green_stop"
        Notifier_green_stop.Size = New Size(5, 90)
        Notifier_green_stop.TabIndex = 2
        Notifier_green_stop.TabStop = False
        ' 
        ' Animation_Engine
        ' 
        Animation_Engine.Interval = 1
        ' 
        ' IF_N
        ' 
        ' 
        ' Notifier
        ' 
        AutoScaleMode = AutoScaleMode.Inherit
        BackColor = Color.Coral
        ClientSize = New Size(300, 90)
        ControlBox = False
        Controls.Add(Notifier_black)
        Controls.Add(Notifier_green)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MdiChildrenMinimizedAnchorBottom = False
        MinimizeBox = False
        Name = "Notifier"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Notifier"
        TopMost = True
        TransparencyKey = Color.Coral
        Notifier_black.ResumeLayout(False)
        CType(Notifier_green_stop, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Notifier_green As Panel
    Friend WithEvents Animation_Engine As Timer
    Friend WithEvents Notifier_black As Panel
    Friend WithEvents Notifier_green_stop As PictureBox
    Friend WithEvents icon_n As Label
    Friend WithEvents text_n As Label
    Friend WithEvents IF_N As Timer
End Class
