Imports System.Drawing

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Notifier
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Notifier))
        Notifier_green = New Panel()
        text_n = New Label()
        icon_n = New Label()
        Notifier_black = New Panel()
        PictureBox1 = New PictureBox()
        Notifier_green_stop = New PictureBox()
        Animation_Engine = New Timer(components)
        Notifier_black.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
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
        Notifier_black.Controls.Add(PictureBox1)
        Notifier_black.Controls.Add(text_n)
        Notifier_black.Controls.Add(Notifier_green_stop)
        Notifier_black.Controls.Add(icon_n)
        Notifier_black.ForeColor = Color.White
        Notifier_black.Location = New Point(0, 90)
        Notifier_black.Name = "Notifier_black"
        Notifier_black.Size = New Size(300, 90)
        Notifier_black.TabIndex = 1
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackgroundImage = My.Resources.Resources.osc_img_appicon_64x64
        PictureBox1.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox1.Location = New Point(21, 15)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(60, 60)
        PictureBox1.TabIndex = 8
        PictureBox1.TabStop = False
        PictureBox1.Visible = False
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
        Animation_Engine.Enabled = True
        Animation_Engine.Interval = 1
        ' 
        ' Base_Notifier
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Coral
        ClientSize = New Size(300, 182)
        ControlBox = False
        Controls.Add(Notifier_black)
        Controls.Add(Notifier_green)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MdiChildrenMinimizedAnchorBottom = False
        MinimizeBox = False
        Name = "Base_Notifier"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Notifier"
        TopMost = True
        TransparencyKey = Color.Coral
        Notifier_black.ResumeLayout(False)
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(Notifier_green_stop, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Notifier_green As Panel
    Friend WithEvents Animation_Engine As Timer
    Friend WithEvents Notifier_black As Panel
    Friend WithEvents Notifier_green_stop As PictureBox
    Friend WithEvents icon_n As Label
    Friend WithEvents text_n As Label
    Friend WithEvents PictureBox1 As PictureBox
End Class
