<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_Privacy_Control
    Inherits NoCloseForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Privacy_Control))
        settings_1 = New Panel()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        Label4 = New Label()
        settings_bg = New PictureBox()
        settings_top = New PictureBox()
        py_2 = New Label()
        action_fn = New Label()
        text_settings = New Label()
        PictureBox6 = New PictureBox()
        settings_1.SuspendLayout()
        CType(settings_bg, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' settings_1
        ' 
        settings_1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_1.BackColor = Drawing.Color.Red
        settings_1.Controls.Add(Label3)
        settings_1.Controls.Add(Label2)
        settings_1.Controls.Add(Label1)
        settings_1.Controls.Add(Label4)
        settings_1.Controls.Add(settings_bg)
        settings_1.Location = New System.Drawing.Point(695, 160)
        settings_1.Name = "settings_1"
        settings_1.Size = New System.Drawing.Size(1145, 841)
        settings_1.TabIndex = 44
        ' 
        ' Label3
        ' 
        Label3.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New System.Drawing.Font("nvgcshare", 50F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Drawing.Color.White
        Label3.Location = New System.Drawing.Point(119, 147)
        Label3.Name = "Label3"
        Label3.Size = New System.Drawing.Size(39, 67)
        Label3.TabIndex = 74
        Label3.Text = ""
        Label3.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label2.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New System.Drawing.Font("Segoe UI Semibold", 13F)
        Label2.ForeColor = Drawing.Color.White
        Label2.Location = New System.Drawing.Point(164, 155)
        Label2.Name = "Label2"
        Label2.Size = New System.Drawing.Size(855, 91)
        Label2.TabIndex = 74
        Label2.Text = "Lets you capture Gameplay Capture/Desktop Capture/Instant Replay/Manual Recording/Screenshot Capture/Live Streaming/Highlights Capture/Notifier."
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New System.Drawing.Font("Segoe UI Semibold", 15F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Drawing.Color.White
        Label1.Location = New System.Drawing.Point(119, 119)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(162, 28)
        Label1.TabIndex = 68
        Label1.Text = "Desktop capture"
        ' 
        ' Label4
        ' 
        Label4.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label4.Font = New System.Drawing.Font("Segoe UI", 17F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Drawing.Color.White
        Label4.Location = New System.Drawing.Point(62, 43)
        Label4.Name = "Label4"
        Label4.Size = New System.Drawing.Size(176, 60)
        Label4.TabIndex = 51
        Label4.Text = "Privacy control"
        ' 
        ' settings_bg
        ' 
        settings_bg.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        settings_bg.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_bg.Location = New System.Drawing.Point(0, 4)
        settings_bg.Name = "settings_bg"
        settings_bg.Size = New System.Drawing.Size(1145, 257)
        settings_bg.TabIndex = 1
        settings_bg.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(695, 159)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(1145, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' py_2
        ' 
        py_2.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        py_2.Cursor = Cursors.Hand
        py_2.Font = New System.Drawing.Font("Segoe UI", 14F)
        py_2.ForeColor = Drawing.Color.White
        py_2.Location = New System.Drawing.Point(465, 300)
        py_2.Name = "py_2"
        py_2.Size = New System.Drawing.Size(200, 70)
        py_2.TabIndex = 72
        py_2.Text = "Turn on"
        py_2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        action_fn.ForeColor = Drawing.Color.White
        action_fn.Location = New System.Drawing.Point(465, 220)
        action_fn.Name = "action_fn"
        action_fn.Size = New System.Drawing.Size(200, 70)
        action_fn.TabIndex = 58
        action_fn.Text = "Back"
        action_fn.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' text_settings
        ' 
        text_settings.BackColor = Drawing.Color.Black
        text_settings.Font = New System.Drawing.Font("Segoe UI Semibold", 14F, Drawing.FontStyle.Bold)
        text_settings.ForeColor = Drawing.Color.White
        text_settings.Location = New System.Drawing.Point(465, 160)
        text_settings.Name = "text_settings"
        text_settings.Size = New System.Drawing.Size(200, 50)
        text_settings.TabIndex = 56
        text_settings.Text = "Privacy control"
        text_settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' PictureBox6
        ' 
        PictureBox6.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox6.Location = New System.Drawing.Point(-3, -16)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New System.Drawing.Size(1951, 176)
        PictureBox6.TabIndex = 73
        PictureBox6.TabStop = False
        ' 
        ' Base_Privacy_Control
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(1920, 1080)
        Controls.Add(PictureBox6)
        Controls.Add(text_settings)
        Controls.Add(py_2)
        Controls.Add(settings_top)
        Controls.Add(action_fn)
        Controls.Add(settings_1)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Base_Privacy_Control"
        ShowInTaskbar = False
        Text = "Privacy control"
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        WindowState = FormWindowState.Maximized
        settings_1.ResumeLayout(False)
        settings_1.PerformLayout()
        CType(settings_bg, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents settings_1 As Panel
    Friend WithEvents action_fn As Label
    Friend WithEvents text_settings As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents settings_bg As PictureBox
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents py_2 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents Label3 As Label
End Class
