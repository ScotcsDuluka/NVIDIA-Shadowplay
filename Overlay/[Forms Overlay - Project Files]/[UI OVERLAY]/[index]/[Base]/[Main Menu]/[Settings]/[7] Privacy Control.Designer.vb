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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Privacy_Control))
        settings_1 = New Panel()
        TogglePrivacy = New ToggleSwitch()
        Label1 = New Label()
        Label3 = New Label()
        Label2 = New Label()
        Label4 = New Label()
        settings_top = New PictureBox()
        action_fn = New Label()
        PictureBox6 = New PictureBox()
        IF_Use_Engine = New Timer(components)
        captrueblock = New Label()
        captrueblock_ico = New Label()
        settings_1.SuspendLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' settings_1
        ' 
        settings_1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_1.Controls.Add(TogglePrivacy)
        settings_1.Controls.Add(Label1)
        settings_1.Controls.Add(Label3)
        settings_1.Controls.Add(Label2)
        settings_1.Controls.Add(Label4)
        settings_1.Location = New Point(80, 160)
        settings_1.Name = "settings_1"
        settings_1.Size = New Size(1760, 240)
        settings_1.TabIndex = 44
        ' 
        ' TogglePrivacy
        ' 
        TogglePrivacy.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        TogglePrivacy.ForeColor = Color.Aquamarine
        TogglePrivacy.ImeMode = ImeMode.Off
        TogglePrivacy.IsOn = False
        TogglePrivacy.Location = New Point(71, 111)
        TogglePrivacy.Name = "TogglePrivacy"
        TogglePrivacy.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        TogglePrivacy.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        TogglePrivacy.ShowGlow = False
        TogglePrivacy.Size = New Size(48, 24)
        TogglePrivacy.TabIndex = 75
        TogglePrivacy.Text = "ToggleSwitch"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(125, 107)
        Label1.Name = "Label1"
        Label1.Size = New Size(162, 28)
        Label1.TabIndex = 68
        Label1.Text = "Desktop capture"
        ' 
        ' Label3
        ' 
        Label3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.White
        Label3.Location = New Point(106, 125)
        Label3.Name = "Label3"
        Label3.Size = New Size(39, 91)
        Label3.TabIndex = 74
        Label3.Text = ""
        Label3.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label2.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New Font("Segoe UI Semibold", 11.8F)
        Label2.ForeColor = Color.White
        Label2.Location = New Point(151, 125)
        Label2.Name = "Label2"
        Label2.Size = New Size(1455, 91)
        Label2.TabIndex = 74
        Label2.Text = "Lets you capture Gameplay Capture/Desktop Capture/Instant Replay/Manual Recording/Screenshot Capture/Live Streaming/Highlights Capture/Notifier."
        Label2.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label4
        ' 
        Label4.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label4.Font = New Font("Segoe UI", 17F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Color.White
        Label4.Location = New Point(62, 43)
        Label4.Name = "Label4"
        Label4.Size = New Size(514, 60)
        Label4.TabIndex = 51
        Label4.Text = "Privacy control"
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(80, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(1760, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(80, 110)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 50)
        action_fn.TabIndex = 58
        action_fn.Text = "Back"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PictureBox6
        ' 
        PictureBox6.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox6.Location = New Point(-3, -16)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(1951, 176)
        PictureBox6.TabIndex = 73
        PictureBox6.TabStop = False
        ' 
        ' IF_Use_Engine
        ' 
        IF_Use_Engine.Enabled = True
        ' 
        ' captrueblock
        ' 
        captrueblock.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        captrueblock.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        captrueblock.Font = New Font("Segoe UI", 11.9F)
        captrueblock.ForeColor = Color.White
        captrueblock.Location = New Point(353, 110)
        captrueblock.Name = "captrueblock"
        captrueblock.Size = New Size(1267, 50)
        captrueblock.TabIndex = 132
        captrueblock.Text = "Settings"
        captrueblock.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' captrueblock_ico
        ' 
        captrueblock_ico.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        captrueblock_ico.Cursor = Cursors.Hand
        captrueblock_ico.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        captrueblock_ico.ForeColor = Color.Peru
        captrueblock_ico.Location = New Point(276, 110)
        captrueblock_ico.Name = "captrueblock_ico"
        captrueblock_ico.Size = New Size(91, 50)
        captrueblock_ico.TabIndex = 133
        captrueblock_ico.Text = ""
        captrueblock_ico.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Base_Privacy_Control
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1080)
        Controls.Add(action_fn)
        Controls.Add(captrueblock)
        Controls.Add(captrueblock_ico)
        Controls.Add(PictureBox6)
        Controls.Add(settings_top)
        Controls.Add(settings_1)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Privacy_Control"
        ShowInTaskbar = False
        Text = "Privacy control"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        settings_1.ResumeLayout(False)
        settings_1.PerformLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents settings_1 As Panel
    Friend WithEvents action_fn As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents Label3 As Label
    Friend WithEvents TogglePrivacy As ToggleSwitch
    Friend WithEvents IF_Use_Engine As Timer
    Friend WithEvents captrueblock As Label
    Friend WithEvents captrueblock_ico As Label
End Class
