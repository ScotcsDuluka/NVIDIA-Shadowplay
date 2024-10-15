Imports System.Drawing

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Notifier
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Notifier))
        Notifier_green = New Panel()
        text_n = New Label()
        icon_n = New Label()
        Notifier_black = New Panel()
        Logo = New PictureBox()
        Notifier_green_stop = New PictureBox()
        Timer5 = New Timer(components)
        Timer2 = New Timer(components)
        Timer1 = New Timer(components)
        load = New Timer(components)
        Timer3 = New Timer(components)
        De = New Timer(components)
        py = New Timer(components)
        Notifier_black.SuspendLayout()
        CType(Logo, ComponentModel.ISupportInitialize).BeginInit()
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
        text_n.Dock = DockStyle.Right
        text_n.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        text_n.ForeColor = Color.White
        text_n.Location = New Point(102, 0)
        text_n.Name = "text_n"
        text_n.Size = New Size(198, 90)
        text_n.TabIndex = 4
        text_n.Text = "{{text}}"
        text_n.TextAlign = ContentAlignment.MiddleLeft
        text_n.Visible = False
        ' 
        ' icon_n
        ' 
        icon_n.BackColor = Color.Black
        icon_n.Dock = DockStyle.Right
        icon_n.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        icon_n.ForeColor = Color.White
        icon_n.Location = New Point(6, 0)
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
        Notifier_black.Controls.Add(Logo)
        Notifier_black.Controls.Add(icon_n)
        Notifier_black.Controls.Add(text_n)
        Notifier_black.Controls.Add(Notifier_green_stop)
        Notifier_black.ForeColor = Color.White
        Notifier_black.Location = New Point(0, 0)
        Notifier_black.Name = "Notifier_black"
        Notifier_black.Size = New Size(300, 90)
        Notifier_black.TabIndex = 1
        ' 
        ' Logo
        ' 
        Logo.BackgroundImage = CType(resources.GetObject("Logo.BackgroundImage"), Image)
        Logo.BackgroundImageLayout = ImageLayout.Stretch
        Logo.Location = New Point(28, 20)
        Logo.Name = "Logo"
        Logo.Size = New Size(50, 50)
        Logo.TabIndex = 7
        Logo.TabStop = False
        Logo.Visible = False
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
        ' Timer5
        ' 
        Timer5.Enabled = True
        Timer5.Interval = 1
        ' 
        ' Timer2
        ' 
        Timer2.Enabled = True
        Timer2.Interval = 1
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 1
        ' 
        ' load
        ' 
        ' 
        ' Timer3
        ' 
        Timer3.Interval = 1
        ' 
        ' De
        ' 
        De.Interval = 1
        ' 
        ' py
        ' 
        py.Interval = 1
        ' 
        ' overlay
        ' 
        AllowDrop = True
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Coral
        CausesValidation = False
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
        Name = "overlay"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Notifier"
        TopMost = True
        TransparencyKey = Color.Coral
        Notifier_black.ResumeLayout(False)
        CType(Logo, ComponentModel.ISupportInitialize).EndInit()
        CType(Notifier_green_stop, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Notifier_green As Panel
    Friend WithEvents Timer5 As Timer
    Friend WithEvents Notifier_black As Panel
    Friend WithEvents Notifier_green_stop As PictureBox
    Friend WithEvents icon_n As Label
    Friend WithEvents text_n As Label
    Friend WithEvents Timer2 As Timer
    Friend WithEvents Timer1 As Timer
    Friend WithEvents load As Timer
    Friend WithEvents Logo As PictureBox
    Friend WithEvents Timer3 As Timer
    Friend WithEvents De As Timer
    Friend WithEvents py As Timer
End Class
