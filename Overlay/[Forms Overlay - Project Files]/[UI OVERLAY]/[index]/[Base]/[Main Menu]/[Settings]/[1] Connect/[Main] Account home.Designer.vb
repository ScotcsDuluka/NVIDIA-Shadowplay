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
        Box_PNG = New PictureBox()
        USERSNAME_TEXT = New Label()
        Account_META = New Label()
        BT_Logout = New Label()
        Status_TEXT = New Label()
        BT_Devices = New Label()
        BT_Security = New Label()
        BT_Providers = New Label()
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Settings_Panel.SuspendLayout()
        CType(Box_PNG, ComponentModel.ISupportInitialize).BeginInit()
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
        Settings_Panel.Controls.Add(Box_PNG)
        Settings_Panel.Controls.Add(USERSNAME_TEXT)
        Settings_Panel.Controls.Add(Account_META)
        Settings_Panel.Controls.Add(BT_Logout)
        Settings_Panel.Controls.Add(Status_TEXT)
        Settings_Panel.Location = New Point(80, 160)
        Settings_Panel.Name = "Settings_Panel"
        Settings_Panel.Size = New Size(1760, 568)
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
        ' Box_PNG
        ' 
        Box_PNG.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        Box_PNG.BackgroundImageLayout = ImageLayout.Zoom
        Box_PNG.Location = New Point(62, 111)
        Box_PNG.Name = "Box_PNG"
        Box_PNG.Size = New Size(96, 96)
        Box_PNG.TabIndex = 60
        Box_PNG.TabStop = False
        Box_PNG.Visible = False
        ' 
        ' USERSNAME_TEXT
        ' 
        USERSNAME_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        USERSNAME_TEXT.Font = New Font("GeForce", 16F, FontStyle.Bold)
        USERSNAME_TEXT.ForeColor = Color.White
        USERSNAME_TEXT.Location = New Point(164, 111)
        USERSNAME_TEXT.Name = "USERSNAME_TEXT"
        USERSNAME_TEXT.Size = New Size(121, 29)
        USERSNAME_TEXT.TabIndex = 61
        USERSNAME_TEXT.Visible = False
        ' 
        ' Account_META
        ' 
        Account_META.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Account_META.Font = New Font("Segoe UI", 9.75F)
        Account_META.ForeColor = Color.Gainsboro
        Account_META.Location = New Point(164, 160)
        Account_META.Name = "Account_META"
        Account_META.Size = New Size(121, 41)
        Account_META.TabIndex = 62
        Account_META.Visible = False
        ' 
        ' BT_Logout
        ' 
        BT_Logout.BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40))
        BT_Logout.Cursor = Cursors.Hand
        BT_Logout.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_Logout.ForeColor = Color.White
        BT_Logout.Location = New Point(62, 460)
        BT_Logout.Name = "BT_Logout"
        BT_Logout.Size = New Size(200, 50)
        BT_Logout.TabIndex = 67
        BT_Logout.Text = "Sign out"
        BT_Logout.TextAlign = ContentAlignment.MiddleCenter
        BT_Logout.Visible = False
        ' 
        ' Status_TEXT
        ' 
        Status_TEXT.AutoSize = True
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 9.75F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(62, 780)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(0, 17)
        Status_TEXT.TabIndex = 68
        ' 
        ' BT_Devices
        ' 
        BT_Devices.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        BT_Devices.Cursor = Cursors.Hand
        BT_Devices.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        BT_Devices.ForeColor = Color.White
        BT_Devices.Location = New Point(286, 115)
        BT_Devices.Name = "BT_Devices"
        BT_Devices.Size = New Size(200, 50)
        BT_Devices.TabIndex = 64
        BT_Devices.Text = "Devices"
        BT_Devices.TextAlign = ContentAlignment.MiddleCenter
        BT_Devices.Visible = False
        ' 
        ' BT_Security
        ' 
        BT_Security.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        BT_Security.Cursor = Cursors.Hand
        BT_Security.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        BT_Security.ForeColor = Color.White
        BT_Security.Location = New Point(492, 115)
        BT_Security.Name = "BT_Security"
        BT_Security.Size = New Size(200, 50)
        BT_Security.TabIndex = 65
        BT_Security.Text = "Sessions & Security"
        BT_Security.TextAlign = ContentAlignment.MiddleCenter
        BT_Security.Visible = False
        ' 
        ' BT_Providers
        ' 
        BT_Providers.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        BT_Providers.Cursor = Cursors.Hand
        BT_Providers.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        BT_Providers.ForeColor = Color.White
        BT_Providers.Location = New Point(698, 115)
        BT_Providers.Name = "BT_Providers"
        BT_Providers.Size = New Size(200, 50)
        BT_Providers.TabIndex = 66
        BT_Providers.Text = "Linked accounts"
        BT_Providers.TextAlign = ContentAlignment.MiddleCenter
        BT_Providers.Visible = False
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
        Controls.Add(BT_Security)
        Controls.Add(BT_Devices)
        Controls.Add(BT_Back)
        Controls.Add(Dim_2)
        Controls.Add(Dim_1)
        Controls.Add(BT_Providers)
        Controls.Add(Dim_Top)
        Controls.Add(Settings_Panel)
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
        CType(Box_PNG, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Settings_Panel As Panel
    Friend WithEvents BT_Back As Label
    Friend WithEvents Settings_TEXT As Label
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
    Friend WithEvents Box_PNG As PictureBox
    Friend WithEvents USERSNAME_TEXT As Label
    Friend WithEvents Account_META As Label
    Friend WithEvents BT_Devices As Label
    Friend WithEvents BT_Security As Label
    Friend WithEvents BT_Providers As Label
    Friend WithEvents BT_Logout As Label
    Friend WithEvents Status_TEXT As Label
End Class
