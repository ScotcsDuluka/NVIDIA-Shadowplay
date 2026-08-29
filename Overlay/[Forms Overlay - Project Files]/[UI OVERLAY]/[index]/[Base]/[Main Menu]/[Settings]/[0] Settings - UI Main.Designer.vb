<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Settings
    Inherits System.Windows.Forms.Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Settings))
        Main_Menu_SET = New Panel()
        Panel = New Panel()
        Panel1 = New Panel()
        Label8 = New Label()
        HOST_BOX = New TextBox()
        Label7 = New Label()
        Label6 = New Label()
        KEY_BOX = New TextBox()
        PORT_BOX = New TextBox()
        Label1 = New Label()
        Label2 = New Label()
        Label5 = New Label()
        Label4 = New Label()
        Desc_UseWindowsSnip = New Label()
        Desc_UseWindowsSnip_SUB = New Label()
        ObsEnabledToggle = New ToggleSwitch()
        Label3 = New Label()
        settings_top = New PictureBox()
        Block_AM = New PictureBox()
        action_fn = New Label()
        ch = New Label()
        SW_lang = New Label()
        PictureBox9 = New PictureBox()
        PictureBox1 = New PictureBox()
        btnExportSettings = New Label()
        btnImportSettings = New Label()
        Main_Menu_SET.SuspendLayout()
        Panel.SuspendLayout()
        Panel1.SuspendLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Block_AM, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Main_Menu_SET
        ' 
        Main_Menu_SET.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Main_Menu_SET.BackColor = Color.Red
        Main_Menu_SET.Controls.Add(Panel)
        Main_Menu_SET.Location = New Point(465, 160)
        Main_Menu_SET.Name = "Main_Menu_SET"
        Main_Menu_SET.Size = New Size(1000, 579)
        Main_Menu_SET.TabIndex = 44
        ' 
        ' Panel
        ' 
        Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Panel.Controls.Add(ObsEnabledToggle)
        Panel.Controls.Add(Panel1)
        Panel.Controls.Add(Label1)
        Panel.Controls.Add(Label2)
        Panel.Controls.Add(Label5)
        Panel.Controls.Add(Label4)
        Panel.Controls.Add(Desc_UseWindowsSnip)
        Panel.Controls.Add(Desc_UseWindowsSnip_SUB)
        Panel.Controls.Add(Label3)
        Panel.Location = New Point(0, -7)
        Panel.Name = "Panel"
        Panel.Size = New Size(1000, 586)
        Panel.TabIndex = 74
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Panel1.Controls.Add(Label8)
        Panel1.Controls.Add(HOST_BOX)
        Panel1.Controls.Add(Label7)
        Panel1.Controls.Add(Label6)
        Panel1.Controls.Add(KEY_BOX)
        Panel1.Controls.Add(PORT_BOX)
        Panel1.Location = New Point(106, 323)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(774, 188)
        Panel1.TabIndex = 84
        ' 
        ' Label8
        ' 
        Label8.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label8.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label8.Font = New Font("Segoe UI Semibold", 11.8F)
        Label8.ForeColor = Color.White
        Label8.Location = New Point(19, 84)
        Label8.Name = "Label8"
        Label8.Size = New Size(89, 27)
        Label8.TabIndex = 143
        Label8.Text = "HOST"
        Label8.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' HOST_BOX
        ' 
        HOST_BOX.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        HOST_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        HOST_BOX.BorderStyle = BorderStyle.None
        HOST_BOX.Font = New Font("nvgcshare", 20F)
        HOST_BOX.ForeColor = Color.White
        HOST_BOX.Location = New Point(19, 114)
        HOST_BOX.Multiline = True
        HOST_BOX.Name = "HOST_BOX"
        HOST_BOX.Size = New Size(218, 34)
        HOST_BOX.TabIndex = 144
        HOST_BOX.Text = "HOST"
        HOST_BOX.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label7
        ' 
        Label7.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label7.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label7.Font = New Font("Segoe UI Semibold", 11.8F)
        Label7.ForeColor = Color.White
        Label7.Location = New Point(121, 17)
        Label7.Name = "Label7"
        Label7.Size = New Size(116, 27)
        Label7.TabIndex = 142
        Label7.Text = "Key/Password"
        Label7.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label6
        ' 
        Label6.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label6.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label6.Font = New Font("Segoe UI Semibold", 11.8F)
        Label6.ForeColor = Color.White
        Label6.Location = New Point(19, 17)
        Label6.Name = "Label6"
        Label6.Size = New Size(89, 27)
        Label6.TabIndex = 85
        Label6.Text = "PORT"
        Label6.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' KEY_BOX
        ' 
        KEY_BOX.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        KEY_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        KEY_BOX.BorderStyle = BorderStyle.None
        KEY_BOX.Font = New Font("nvgcshare", 20F)
        KEY_BOX.ForeColor = Color.White
        KEY_BOX.Location = New Point(121, 47)
        KEY_BOX.Multiline = True
        KEY_BOX.Name = "KEY_BOX"
        KEY_BOX.Size = New Size(633, 34)
        KEY_BOX.TabIndex = 141
        KEY_BOX.Text = "Key"
        KEY_BOX.TextAlign = HorizontalAlignment.Center
        ' 
        ' PORT_BOX
        ' 
        PORT_BOX.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        PORT_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        PORT_BOX.BorderStyle = BorderStyle.None
        PORT_BOX.Font = New Font("nvgcshare", 20F)
        PORT_BOX.ForeColor = Color.White
        PORT_BOX.Location = New Point(19, 47)
        PORT_BOX.Multiline = True
        PORT_BOX.Name = "PORT_BOX"
        PORT_BOX.Size = New Size(96, 34)
        PORT_BOX.TabIndex = 140
        PORT_BOX.Text = "Port"
        PORT_BOX.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(125, 216)
        Label1.Name = "Label1"
        Label1.Size = New Size(389, 28)
        Label1.TabIndex = 81
        Label1.Text = "OBS Studio WebSocket Integration - Beta"
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label2.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New Font("Segoe UI Semibold", 11.8F)
        Label2.ForeColor = Color.White
        Label2.Location = New Point(151, 234)
        Label2.Name = "Label2"
        Label2.Size = New Size(596, 91)
        Label2.TabIndex = 83
        Label2.Text = "Allow Notifier to connect to the OBS Studio WebSocket and display notifications based on OBS Studio states or events."
        Label2.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label5
        ' 
        Label5.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label5.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.White
        Label5.Location = New Point(106, 234)
        Label5.Name = "Label5"
        Label5.Size = New Size(39, 91)
        Label5.TabIndex = 82
        Label5.Text = ""
        Label5.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label4
        ' 
        Label4.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label4.Font = New Font("GeForce", 24F, FontStyle.Bold)
        Label4.ForeColor = Color.White
        Label4.Location = New Point(62, 43)
        Label4.Name = "Label4"
        Label4.Size = New Size(514, 60)
        Label4.TabIndex = 80
        Label4.Text = "General"
        ' 
        ' Desc_UseWindowsSnip
        ' 
        Desc_UseWindowsSnip.AutoSize = True
        Desc_UseWindowsSnip.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_UseWindowsSnip.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_UseWindowsSnip.ForeColor = Color.White
        Desc_UseWindowsSnip.Location = New Point(125, 107)
        Desc_UseWindowsSnip.Name = "Desc_UseWindowsSnip"
        Desc_UseWindowsSnip.Size = New Size(362, 28)
        Desc_UseWindowsSnip.TabIndex = 76
        Desc_UseWindowsSnip.Text = "Take screenshots with Windows - Beta"
        ' 
        ' Desc_UseWindowsSnip_SUB
        ' 
        Desc_UseWindowsSnip_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_UseWindowsSnip_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_UseWindowsSnip_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_UseWindowsSnip_SUB.ForeColor = Color.White
        Desc_UseWindowsSnip_SUB.Location = New Point(151, 125)
        Desc_UseWindowsSnip_SUB.Name = "Desc_UseWindowsSnip_SUB"
        Desc_UseWindowsSnip_SUB.Size = New Size(596, 91)
        Desc_UseWindowsSnip_SUB.TabIndex = 78
        Desc_UseWindowsSnip_SUB.Text = "Screenshots are taken with Windows (Win+Shift+S), letting you select the area to capture."
        Desc_UseWindowsSnip_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ObsEnabledToggle
        ' 
        ObsEnabledToggle.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ObsEnabledToggle.ImeMode = ImeMode.Off
        ObsEnabledToggle.IsOn = False
        ObsEnabledToggle.Location = New Point(71, 220)
        ObsEnabledToggle.Name = "ObsEnabledToggle"
        ObsEnabledToggle.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ObsEnabledToggle.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ObsEnabledToggle.ShowGlow = False
        ObsEnabledToggle.Size = New Size(48, 24)
        ObsEnabledToggle.TabIndex = 146
        ObsEnabledToggle.Text = "ToggleSwitch"
        ' 
        ' Label3
        ' 
        Label3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.White
        Label3.Location = New Point(106, 125)
        Label3.Name = "Label3"
        Label3.Size = New Size(39, 91)
        Label3.TabIndex = 77
        Label3.Text = ""
        Label3.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(465, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(1000, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' Block_AM
        ' 
        Block_AM.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Block_AM.Location = New Point(-3, -16)
        Block_AM.Name = "Block_AM"
        Block_AM.Size = New Size(1796, 176)
        Block_AM.TabIndex = 73
        Block_AM.TabStop = False
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(465, 110)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 50)
        action_fn.TabIndex = 74
        action_fn.Text = "Done"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ch
        ' 
        ch.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ch.Cursor = Cursors.Hand
        ch.Font = New Font("Segoe UI Semibold", 12F)
        ch.ForeColor = Color.White
        ch.Location = New Point(877, 115)
        ch.Name = "ch"
        ch.Size = New Size(200, 50)
        ch.TabIndex = 75
        ch.Text = "Check update"
        ch.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' SW_lang
        ' 
        SW_lang.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        SW_lang.Cursor = Cursors.Hand
        SW_lang.Font = New Font("Segoe UI Semibold", 12F)
        SW_lang.ForeColor = Color.White
        SW_lang.Location = New Point(671, 115)
        SW_lang.Name = "SW_lang"
        SW_lang.Size = New Size(200, 50)
        SW_lang.TabIndex = 76
        SW_lang.Text = "SW_lang"
        SW_lang.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PictureBox9
        ' 
        PictureBox9.Anchor = AnchorStyles.Bottom
        PictureBox9.BackColor = Color.Blue
        PictureBox9.BackgroundImageLayout = ImageLayout.None
        PictureBox9.Location = New Point(1220, 739)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(80, 80)
        PictureBox9.TabIndex = 91
        PictureBox9.TabStop = False
        PictureBox9.Visible = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.Blue
        PictureBox1.BackgroundImageLayout = ImageLayout.None
        PictureBox1.Location = New Point(1465, 279)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(80, 80)
        PictureBox1.TabIndex = 92
        PictureBox1.TabStop = False
        PictureBox1.Visible = False
        ' 
        ' btnExportSettings
        ' 
        btnExportSettings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        btnExportSettings.Cursor = Cursors.Hand
        btnExportSettings.Font = New Font("Segoe UI Semibold", 12F)
        btnExportSettings.ForeColor = Color.White
        btnExportSettings.Location = New Point(1083, 115)
        btnExportSettings.Name = "btnExportSettings"
        btnExportSettings.Size = New Size(200, 50)
        btnExportSettings.TabIndex = 93
        btnExportSettings.Text = "Export Settings"
        btnExportSettings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' btnImportSettings
        ' 
        btnImportSettings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        btnImportSettings.Cursor = Cursors.Hand
        btnImportSettings.Font = New Font("Segoe UI Semibold", 12F)
        btnImportSettings.ForeColor = Color.White
        btnImportSettings.Location = New Point(1289, 115)
        btnImportSettings.Name = "btnImportSettings"
        btnImportSettings.Size = New Size(200, 50)
        btnImportSettings.TabIndex = 94
        btnImportSettings.Text = "Import Settings"
        btnImportSettings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Base_Settings
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1545, 819)
        Controls.Add(btnImportSettings)
        Controls.Add(btnExportSettings)
        Controls.Add(PictureBox1)
        Controls.Add(PictureBox9)
        Controls.Add(SW_lang)
        Controls.Add(ch)
        Controls.Add(action_fn)
        Controls.Add(settings_top)
        Controls.Add(Block_AM)
        Controls.Add(Main_Menu_SET)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Settings"
        ShowInTaskbar = False
        Text = "Privacy control"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        Main_Menu_SET.ResumeLayout(False)
        Panel.ResumeLayout(False)
        Panel.PerformLayout()
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(Block_AM, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Main_Menu_SET As Panel
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents Block_AM As PictureBox
    Friend WithEvents Panel As Panel
    Friend WithEvents ch As Label
    Friend WithEvents SW_lang As Label
    Public WithEvents action_fn As Label
    Friend WithEvents PictureBox9 As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents btnExportSettings As Label
    Friend WithEvents btnImportSettings As Label
    Friend WithEvents ToggleUseWindowsSnip As ToggleSwitch
    Friend WithEvents Desc_UseWindowsSnip As Label
    Friend WithEvents Desc_UseWindowsSnip_SUB As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PORT_BOX As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents HOST_BOX As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents KEY_BOX As TextBox
    Friend WithEvents ObsEnabledToggle As ToggleSwitch
End Class
