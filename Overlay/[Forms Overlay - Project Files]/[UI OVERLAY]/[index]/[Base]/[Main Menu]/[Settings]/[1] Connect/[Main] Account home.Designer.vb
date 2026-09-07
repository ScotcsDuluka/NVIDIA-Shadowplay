<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect))
        Settings_Panel = New Panel()
        Settings_TEXT = New Label()
        Status_TEXT = New Label()
        Card_PANEL = New Panel()
        Avatar_BOX = New Label()
        Avatar_PICTURE = New PictureBox()
        USERSNAME_TEXT = New Label()
        Account_META = New Label()
        BT_Logout = New Label()
        BT_Devices = New Label()
        BT_Security = New Label()
        BT_Providers = New Label()
        BT_EditProfile = New Label()
        Session_PANEL = New Panel()
        Session_TITLE = New Label()
        Session_META = New Label()
        Nudge_PANEL = New Panel()
        Nudge_TITLE = New Label()
        Nudge_META = New Label()
        BT_SetupNow = New Label()
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Settings_Panel.SuspendLayout()
        Card_PANEL.SuspendLayout()
        Session_PANEL.SuspendLayout()
        CType(Dim_Top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' Settings_Panel
        '
        Settings_Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Settings_Panel.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_Panel.Controls.Add(Settings_TEXT)
        Settings_Panel.Controls.Add(Status_TEXT)
        Settings_Panel.Controls.Add(Card_PANEL)
        Settings_Panel.Controls.Add(BT_Devices)
        Settings_Panel.Controls.Add(BT_Security)
        Settings_Panel.Controls.Add(BT_Providers)
        Settings_Panel.Controls.Add(BT_EditProfile)
        Settings_Panel.Controls.Add(Session_PANEL)
        Settings_Panel.Controls.Add(Nudge_PANEL)
        Settings_Panel.Location = New Point(80, 160)
        Settings_Panel.Name = "Settings_Panel"
        Settings_Panel.Size = New Size(1760, 840)
        Settings_Panel.TabIndex = 45
        '
        ' Settings_TEXT
        '
        Settings_TEXT.AutoSize = True
        Settings_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_TEXT.Font = New Font("GeForce", 24F, FontStyle.Bold)
        Settings_TEXT.ForeColor = Color.White
        Settings_TEXT.Location = New Point(62, 43)
        Settings_TEXT.Name = "Settings_TEXT"
        Settings_TEXT.Size = New Size(232, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Duluka Account"
        '
        ' Status_TEXT
        '
        Status_TEXT.AutoSize = True
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 9.75F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(62, 528)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(0, 17)
        Status_TEXT.TabIndex = 68
        '
        ' Card_PANEL
        '
        Card_PANEL.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Card_PANEL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Card_PANEL.Controls.Add(Avatar_BOX)
        Card_PANEL.Controls.Add(Avatar_PICTURE)
        Card_PANEL.Controls.Add(USERSNAME_TEXT)
        Card_PANEL.Controls.Add(Account_META)
        Card_PANEL.Controls.Add(BT_Logout)
        Card_PANEL.Location = New Point(62, 130)
        Card_PANEL.Name = "Card_PANEL"
        Card_PANEL.Size = New Size(1636, 160)
        Card_PANEL.TabIndex = 90
        '
        ' Avatar_BOX
        '
        Avatar_BOX.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Avatar_BOX.Font = New Font("GeForce", 30F, FontStyle.Bold)
        Avatar_BOX.ForeColor = Color.White
        Avatar_BOX.Location = New Point(24, 32)
        Avatar_BOX.Name = "Avatar_BOX"
        Avatar_BOX.Size = New Size(96, 96)
        Avatar_BOX.TabIndex = 91
        Avatar_BOX.Text = "D"
        Avatar_BOX.TextAlign = ContentAlignment.MiddleCenter
        '
        ' Avatar_PICTURE
        '
        ' Sits exactly over Avatar_BOX: visible ONLY when the account has a
        ' decoded profile image (the letter label is the fallback). Zoom keeps
        ' any avatar aspect inside the 96px square without distortion.
        Avatar_PICTURE.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Avatar_PICTURE.Location = New Point(24, 32)
        Avatar_PICTURE.Name = "Avatar_PICTURE"
        Avatar_PICTURE.Size = New Size(96, 96)
        Avatar_PICTURE.SizeMode = PictureBoxSizeMode.Zoom
        Avatar_PICTURE.TabStop = False
        Avatar_PICTURE.Visible = False
        '
        ' USERSNAME_TEXT
        '
        USERSNAME_TEXT.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        USERSNAME_TEXT.Font = New Font("GeForce", 16F, FontStyle.Bold)
        USERSNAME_TEXT.ForeColor = Color.White
        USERSNAME_TEXT.Location = New Point(144, 40)
        USERSNAME_TEXT.Name = "USERSNAME_TEXT"
        USERSNAME_TEXT.Size = New Size(900, 36)
        USERSNAME_TEXT.TabIndex = 92
        USERSNAME_TEXT.Text = "Duluka Account"
        '
        ' Account_META
        '
        Account_META.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Account_META.Font = New Font("Segoe UI", 9.75F)
        Account_META.ForeColor = Color.Gainsboro
        Account_META.Location = New Point(144, 86)
        Account_META.Name = "Account_META"
        Account_META.Size = New Size(900, 54)
        Account_META.TabIndex = 93
        '
        ' BT_Logout
        '
        BT_Logout.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        BT_Logout.BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40))
        BT_Logout.Cursor = Cursors.Hand
        BT_Logout.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_Logout.ForeColor = Color.White
        BT_Logout.Location = New Point(1387, 52)
        BT_Logout.Name = "BT_Logout"
        BT_Logout.Size = New Size(200, 50)
        BT_Logout.TabIndex = 94
        BT_Logout.Text = "Sign out"
        BT_Logout.TextAlign = ContentAlignment.MiddleCenter
        '
        ' BT_Devices
        '
        BT_Devices.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        BT_Devices.Cursor = Cursors.Hand
        BT_Devices.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        BT_Devices.ForeColor = Color.White
        BT_Devices.Location = New Point(62, 310)
        BT_Devices.Name = "BT_Devices"
        BT_Devices.Size = New Size(397, 64)
        BT_Devices.TabIndex = 64
        BT_Devices.Text = "Devices"
        BT_Devices.TextAlign = ContentAlignment.MiddleCenter
        '
        ' BT_Security
        '
        BT_Security.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        BT_Security.Cursor = Cursors.Hand
        BT_Security.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        BT_Security.ForeColor = Color.White
        BT_Security.Location = New Point(475, 310)
        BT_Security.Name = "BT_Security"
        BT_Security.Size = New Size(397, 64)
        BT_Security.TabIndex = 65
        BT_Security.Text = "Sessions && Security"
        BT_Security.TextAlign = ContentAlignment.MiddleCenter
        '
        ' BT_Providers
        '
        ' NB: no Left/Right anchor here — the action row is laid out in code
        ' (LayoutRow) so the FOUR buttons always divide the width into exact
        ' quarters; an anchor would stretch only this button and skew the row.
        BT_Providers.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        BT_Providers.Cursor = Cursors.Hand
        BT_Providers.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        BT_Providers.ForeColor = Color.White
        BT_Providers.Location = New Point(888, 310)
        BT_Providers.Name = "BT_Providers"
        BT_Providers.Size = New Size(397, 64)
        BT_Providers.TabIndex = 66
        BT_Providers.Text = "Linked accounts"
        BT_Providers.TextAlign = ContentAlignment.MiddleCenter
        '
        ' BT_EditProfile
        '
        ' NB: no anchor — the action row is laid out in code (LayoutRow) so the
        ' FOUR buttons always divide the width into exact quarters.
        BT_EditProfile.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        BT_EditProfile.Cursor = Cursors.Hand
        BT_EditProfile.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        BT_EditProfile.ForeColor = Color.White
        BT_EditProfile.Location = New Point(1301, 310)
        BT_EditProfile.Name = "BT_EditProfile"
        BT_EditProfile.Size = New Size(397, 64)
        BT_EditProfile.TabIndex = 67
        BT_EditProfile.Text = "Edit Profile"
        BT_EditProfile.TextAlign = ContentAlignment.MiddleCenter
        '
        ' Session_PANEL
        '
        Session_PANEL.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Session_PANEL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Session_PANEL.Controls.Add(Session_TITLE)
        Session_PANEL.Controls.Add(Session_META)
        Session_PANEL.Location = New Point(62, 398)
        Session_PANEL.Name = "Session_PANEL"
        Session_PANEL.Size = New Size(1636, 110)
        Session_PANEL.TabIndex = 95
        '
        ' Session_TITLE
        '
        Session_TITLE.AutoSize = True
        Session_TITLE.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Session_TITLE.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Session_TITLE.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Session_TITLE.Location = New Point(24, 16)
        Session_TITLE.Name = "Session_TITLE"
        Session_TITLE.Size = New Size(113, 15)
        Session_TITLE.TabIndex = 96
        Session_TITLE.Text = "CURRENT SESSION"
        '
        ' Session_META
        '
        Session_META.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Session_META.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Session_META.Font = New Font("Segoe UI", 9.75F)
        Session_META.ForeColor = Color.Silver
        Session_META.Location = New Point(24, 44)
        Session_META.Name = "Session_META"
        Session_META.Size = New Size(1588, 44)
        Session_META.TabIndex = 97
        ' 
        ' Nudge_PANEL
        ' 
        Nudge_PANEL.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Nudge_PANEL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Nudge_PANEL.Controls.Add(Nudge_TITLE)
        Nudge_PANEL.Controls.Add(Nudge_META)
        Nudge_PANEL.Controls.Add(BT_SetupNow)
        Nudge_PANEL.Location = New Point(62, 560)
        Nudge_PANEL.Name = "Nudge_PANEL"
        Nudge_PANEL.Size = New Size(1636, 110)
        Nudge_PANEL.TabIndex = 98
        Nudge_PANEL.Visible = False
        ' 
        ' Nudge_TITLE
        ' 
        Nudge_TITLE.AutoSize = True
        Nudge_TITLE.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Nudge_TITLE.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Nudge_TITLE.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Nudge_TITLE.Location = New Point(24, 16)
        Nudge_TITLE.Name = "Nudge_TITLE"
        Nudge_TITLE.Size = New Size(154, 15)
        Nudge_TITLE.TabIndex = 99
        Nudge_TITLE.Text = "FINISH SETTING UP"
        ' 
        ' Nudge_META
        ' 
        Nudge_META.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Nudge_META.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Nudge_META.Font = New Font("Segoe UI", 9.75F)
        Nudge_META.ForeColor = Color.Silver
        Nudge_META.Location = New Point(24, 44)
        Nudge_META.Name = "Nudge_META"
        Nudge_META.Size = New Size(1300, 44)
        Nudge_META.TabIndex = 100
        Nudge_META.Text = "This account has no username or password yet — GitHub alone cannot always sign you in."
        ' 
        ' BT_SetupNow
        ' 
        BT_SetupNow.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        BT_SetupNow.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_SetupNow.Cursor = Cursors.Hand
        BT_SetupNow.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_SetupNow.ForeColor = Color.White
        BT_SetupNow.Location = New Point(1387, 30)
        BT_SetupNow.Name = "BT_SetupNow"
        BT_SetupNow.Size = New Size(200, 50)
        BT_SetupNow.TabIndex = 101
        BT_SetupNow.Text = "Set Up Account"
        BT_SetupNow.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Dim_Top
        '
        Dim_Top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Dim_Top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Dim_Top.Location = New Point(80, 160)
        Dim_Top.Name = "Dim_Top"
        Dim_Top.Size = New Size(1760, 5)
        Dim_Top.TabIndex = 0
        Dim_Top.TabStop = False
        '
        ' BT_Back
        '
        BT_Back.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_Back.Cursor = Cursors.Hand
        BT_Back.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        BT_Back.ForeColor = Color.White
        BT_Back.Location = New Point(80, 110)
        BT_Back.Name = "BT_Back"
        BT_Back.Size = New Size(200, 50)
        BT_Back.TabIndex = 58
        BT_Back.Text = "Back"
        BT_Back.TextAlign = ContentAlignment.MiddleCenter
        '
        ' Dim_1
        '
        Dim_1.BackColor = Color.Blue
        Dim_1.BackgroundImageLayout = ImageLayout.None
        Dim_1.Location = New Point(0, 203)
        Dim_1.Name = "Dim_1"
        Dim_1.Size = New Size(80, 80)
        Dim_1.TabIndex = 93
        Dim_1.TabStop = False
        Dim_1.Visible = False
        '
        ' Dim_2
        '
        Dim_2.BackColor = Color.Blue
        Dim_2.BackgroundImageLayout = ImageLayout.None
        Dim_2.Location = New Point(1840, 166)
        Dim_2.Name = "Dim_2"
        Dim_2.Size = New Size(80, 80)
        Dim_2.TabIndex = 94
        Dim_2.TabStop = False
        Dim_2.Visible = False
        '
        ' Base_Connect
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1070)
        Controls.Add(BT_Back)
        Controls.Add(Dim_Top)
        Controls.Add(Settings_Panel)
        Controls.Add(Dim_1)
        Controls.Add(Dim_2)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Connect"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        Settings_Panel.ResumeLayout(False)
        Settings_Panel.PerformLayout()
        Card_PANEL.ResumeLayout(False)
        Session_PANEL.ResumeLayout(False)
        Session_PANEL.PerformLayout()
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Settings_Panel As Panel
    Friend WithEvents Card_PANEL As Panel
    Friend WithEvents Avatar_BOX As Label
    Friend WithEvents USERSNAME_TEXT As Label
    Friend WithEvents Account_META As Label
    Friend WithEvents BT_Logout As Label
    Friend WithEvents Session_PANEL As Panel
    Friend WithEvents Session_TITLE As Label
    Friend WithEvents Session_META As Label
    Friend WithEvents BT_Back As Label
    Friend WithEvents Settings_TEXT As Label
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
    Friend WithEvents BT_Devices As Label
    Friend WithEvents BT_Security As Label
    Friend WithEvents BT_Providers As Label
    Friend WithEvents BT_EditProfile As Label
    Friend WithEvents Avatar_PICTURE As PictureBox
    Friend WithEvents Status_TEXT As Label
    Friend WithEvents Nudge_PANEL As Panel
    Friend WithEvents Nudge_TITLE As Label
    Friend WithEvents Nudge_META As Label
    Friend WithEvents BT_SetupNow As Label
End Class
