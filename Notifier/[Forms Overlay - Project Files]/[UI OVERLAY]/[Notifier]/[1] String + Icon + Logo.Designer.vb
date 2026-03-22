<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Notifier_Sub
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
        Notifier_black = New Panel()
        PictureBox1 = New PictureBox()
        text_n = New Label()
        icon_n = New Label()
        Timer1 = New Timer(components)
        Animation_Engine = New Timer(components)
        Notifier_black.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Notifier_black
        ' 
        Notifier_black.BackColor = Color.Black
        Notifier_black.Controls.Add(PictureBox1)
        Notifier_black.Controls.Add(text_n)
        Notifier_black.Controls.Add(icon_n)
        Notifier_black.ForeColor = Color.White
        Notifier_black.Location = New Point(5, 0)
        Notifier_black.Name = "Notifier_black"
        Notifier_black.Size = New Size(295, 90)
        Notifier_black.TabIndex = 2
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.Transparent
        PictureBox1.BackgroundImage = My.Resources.Resources.osc_img_appicon_64x64
        PictureBox1.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox1.Location = New Point(20, 13)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(47, 64)
        PictureBox1.TabIndex = 8
        PictureBox1.TabStop = False
        PictureBox1.Visible = False
        ' 
        ' text_n
        ' 
        text_n.BackColor = Color.Black
        text_n.Font = New Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        text_n.ForeColor = Color.White
        text_n.Location = New Point(86, 0)
        text_n.Name = "text_n"
        text_n.Size = New Size(199, 90)
        text_n.TabIndex = 4
        text_n.Text = "{{text}}"
        text_n.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' icon_n
        ' 
        icon_n.BackColor = Color.Black
        icon_n.Dock = DockStyle.Left
        icon_n.Font = New Font("nvgcshare", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        icon_n.ForeColor = Color.White
        icon_n.Location = New Point(0, 0)
        icon_n.Name = "icon_n"
        icon_n.Size = New Size(93, 90)
        icon_n.TabIndex = 3
        icon_n.Text = "{}     "
        icon_n.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Timer1
        ' 
        ' 
        ' Animation_Engine
        ' 
        Animation_Engine.Interval = 1
        ' 
        ' Notifier_Sub
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(300, 90)
        Controls.Add(Notifier_black)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Name = "Notifier_Sub"
        ShowInTaskbar = False
        Text = "Notifier_Sub"
        TopMost = True
        TransparencyKey = Color.Red
        Notifier_black.ResumeLayout(False)
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Notifier_black As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents text_n As Label
    Friend WithEvents icon_n As Label
    Friend WithEvents Timer1 As Timer
    Friend WithEvents Animation_Engine As Timer
End Class
