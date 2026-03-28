<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect
    Inherits NoCloseForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect))
        settings_1 = New Panel()
        Login_BT = New Button()
        Password_Box = New TextBox()
        Usersname_Box = New TextBox()
        Box_PNG = New PictureBox()
        USERSNAME_TEXT = New Label()
        Github_Text = New Label()
        Connect_ICO = New Label()
        Connect_Box = New PictureBox()
        Connect_Box_Sub = New PictureBox()
        text_menu = New Label()
        text_settings = New Label()
        box_settings = New PictureBox()
        settings_top = New PictureBox()
        action_fn = New Label()
        PictureBox6 = New PictureBox()
        settings_1.SuspendLayout()
        CType(Box_PNG, ComponentModel.ISupportInitialize).BeginInit()
        CType(Connect_Box, ComponentModel.ISupportInitialize).BeginInit()
        CType(Connect_Box_Sub, ComponentModel.ISupportInitialize).BeginInit()
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' settings_1
        ' 
        settings_1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        settings_1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_1.Controls.Add(Login_BT)
        settings_1.Controls.Add(Password_Box)
        settings_1.Controls.Add(Usersname_Box)
        settings_1.Controls.Add(Box_PNG)
        settings_1.Controls.Add(USERSNAME_TEXT)
        settings_1.Controls.Add(Github_Text)
        settings_1.Controls.Add(Connect_ICO)
        settings_1.Controls.Add(Connect_Box)
        settings_1.Controls.Add(Connect_Box_Sub)
        settings_1.Controls.Add(text_menu)
        settings_1.Location = New Point(695, 160)
        settings_1.Name = "settings_1"
        settings_1.Size = New Size(1145, 841)
        settings_1.TabIndex = 45
        ' 
        ' Login_BT
        ' 
        Login_BT.Location = New Point(326, 359)
        Login_BT.Name = "Login_BT"
        Login_BT.Size = New Size(75, 23)
        Login_BT.TabIndex = 60
        Login_BT.Text = "Login"
        Login_BT.UseVisualStyleBackColor = True
        Login_BT.Visible = False
        ' 
        ' Password_Box
        ' 
        Password_Box.Location = New Point(245, 288)
        Password_Box.Name = "Password_Box"
        Password_Box.Size = New Size(141, 23)
        Password_Box.TabIndex = 59
        Password_Box.Visible = False
        ' 
        ' Usersname_Box
        ' 
        Usersname_Box.Location = New Point(245, 250)
        Usersname_Box.Name = "Usersname_Box"
        Usersname_Box.Size = New Size(141, 23)
        Usersname_Box.TabIndex = 58
        Usersname_Box.Text = "Usersname"
        Usersname_Box.Visible = False
        ' 
        ' Box_PNG
        ' 
        Box_PNG.BackgroundImageLayout = ImageLayout.Stretch
        Box_PNG.Location = New Point(648, 182)
        Box_PNG.Name = "Box_PNG"
        Box_PNG.Size = New Size(100, 100)
        Box_PNG.TabIndex = 57
        Box_PNG.TabStop = False
        ' 
        ' USERSNAME_TEXT
        ' 
        USERSNAME_TEXT.AutoSize = True
        USERSNAME_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        USERSNAME_TEXT.Font = New Font("Segoe UI Semibold", 17F, FontStyle.Bold)
        USERSNAME_TEXT.ForeColor = Color.White
        USERSNAME_TEXT.Location = New Point(795, 202)
        USERSNAME_TEXT.Name = "USERSNAME_TEXT"
        USERSNAME_TEXT.Size = New Size(99, 31)
        USERSNAME_TEXT.TabIndex = 56
        USERSNAME_TEXT.Text = "Connect"
        ' 
        ' Github_Text
        ' 
        Github_Text.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Github_Text.Cursor = Cursors.Hand
        Github_Text.Font = New Font("Segoe UI Semibold", 11F, FontStyle.Bold)
        Github_Text.ForeColor = Color.White
        Github_Text.Location = New Point(119, 98)
        Github_Text.Name = "Github_Text"
        Github_Text.Size = New Size(131, 56)
        Github_Text.TabIndex = 55
        Github_Text.Text = "l10n.connect"
        Github_Text.TextAlign = ContentAlignment.MiddleLeft
        Github_Text.Visible = False
        ' 
        ' Connect_ICO
        ' 
        Connect_ICO.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Connect_ICO.Cursor = Cursors.Hand
        Connect_ICO.Font = New Font("nvgcshare", 26F)
        Connect_ICO.ForeColor = Color.White
        Connect_ICO.Location = New Point(62, 98)
        Connect_ICO.Name = "Connect_ICO"
        Connect_ICO.Size = New Size(63, 56)
        Connect_ICO.TabIndex = 54
        Connect_ICO.Text = ""
        Connect_ICO.TextAlign = ContentAlignment.MiddleCenter
        Connect_ICO.Visible = False
        ' 
        ' Connect_Box
        ' 
        Connect_Box.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Connect_Box.Cursor = Cursors.Hand
        Connect_Box.Location = New Point(61, 98)
        Connect_Box.Name = "Connect_Box"
        Connect_Box.Size = New Size(298, 56)
        Connect_Box.TabIndex = 53
        Connect_Box.TabStop = False
        Connect_Box.Visible = False
        ' 
        ' Connect_Box_Sub
        ' 
        Connect_Box_Sub.BackColor = Color.DimGray
        Connect_Box_Sub.Cursor = Cursors.Hand
        Connect_Box_Sub.Location = New Point(59, 96)
        Connect_Box_Sub.Name = "Connect_Box_Sub"
        Connect_Box_Sub.Size = New Size(302, 60)
        Connect_Box_Sub.TabIndex = 52
        Connect_Box_Sub.TabStop = False
        Connect_Box_Sub.Visible = False
        ' 
        ' text_menu
        ' 
        text_menu.AutoSize = True
        text_menu.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        text_menu.Font = New Font("Segoe UI Semibold", 17F, FontStyle.Bold)
        text_menu.ForeColor = Color.White
        text_menu.Location = New Point(62, 43)
        text_menu.Name = "text_menu"
        text_menu.Size = New Size(99, 31)
        text_menu.TabIndex = 51
        text_menu.Text = "Connect"
        ' 
        ' text_settings
        ' 
        text_settings.BackColor = Color.Black
        text_settings.Font = New Font("Segoe UI Semibold", 14F, FontStyle.Bold)
        text_settings.ForeColor = Color.White
        text_settings.Location = New Point(465, 160)
        text_settings.Name = "text_settings"
        text_settings.Size = New Size(200, 50)
        text_settings.TabIndex = 56
        text_settings.Text = "Connect"
        text_settings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Color.Black
        box_settings.Location = New Point(465, 160)
        box_settings.Name = "box_settings"
        box_settings.Size = New Size(200, 48)
        box_settings.TabIndex = 55
        box_settings.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(695, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(1145, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(465, 220)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 70)
        action_fn.TabIndex = 58
        action_fn.Text = "Back"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PictureBox6
        ' 
        PictureBox6.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox6.Location = New Point(454, -16)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(1435, 176)
        PictureBox6.TabIndex = 46
        PictureBox6.TabStop = False
        ' 
        ' Base_Connect
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1080)
        Controls.Add(text_settings)
        Controls.Add(box_settings)
        Controls.Add(settings_top)
        Controls.Add(PictureBox6)
        Controls.Add(settings_1)
        Controls.Add(action_fn)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Connect"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        settings_1.ResumeLayout(False)
        settings_1.PerformLayout()
        CType(Box_PNG, ComponentModel.ISupportInitialize).EndInit()
        CType(Connect_Box, ComponentModel.ISupportInitialize).EndInit()
        CType(Connect_Box_Sub, ComponentModel.ISupportInitialize).EndInit()
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents settings_1 As Panel
    Friend WithEvents action_fn As Label
    Friend WithEvents text_settings As Label
    Friend WithEvents text_menu As Label
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents box_settings As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents Box_PNG As PictureBox
    Friend WithEvents USERSNAME_TEXT As Label
    Friend WithEvents Github_Text As Label
    Friend WithEvents Connect_ICO As Label
    Friend WithEvents Connect_Box As PictureBox
    Friend WithEvents Connect_Box_Sub As PictureBox
    Friend WithEvents Password_Box As TextBox
    Friend WithEvents Usersname_Box As TextBox
    Friend WithEvents Login_BT As Button
End Class
