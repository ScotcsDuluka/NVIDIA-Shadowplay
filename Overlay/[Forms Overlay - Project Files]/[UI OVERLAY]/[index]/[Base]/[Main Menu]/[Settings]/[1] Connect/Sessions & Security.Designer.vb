<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect_Security
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect_Security))
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Bg1 = New PictureBox()
        Status_TEXT = New Label()
        BT_DeleteAccount = New Label()
        DzPassword_BOX = New TextBox()
        DzPassword_LBL = New Label()
        DzNote_META = New Label()
        DzHeader_LBL = New Label()
        BT_ChangePassword = New Label()
        PwConfirm_BOX = New TextBox()
        PwConfirm_LBL = New Label()
        PwNew_BOX = New TextBox()
        PwNew_LBL = New Label()
        PwCurrent_BOX = New TextBox()
        PwCurrent_LBL = New Label()
        PwUsername_BOX = New TextBox()
        PwUsername_LBL = New Label()
        PwHeader_LBL = New Label()
        BT_RevokeAll = New Label()
        BT_RefreshSession = New Label()
        Info_META = New Label()
        Settings_TEXT = New Label()
        Settings_Panel = New Panel()
        CType(Dim_Top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).BeginInit()
        CType(Bg1, ComponentModel.ISupportInitialize).BeginInit()
        Settings_Panel.SuspendLayout()
        SuspendLayout()
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
        ' Bg1
        ' 
        Bg1.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        Bg1.Location = New Point(62, 452)
        Bg1.Name = "Bg1"
        Bg1.Size = New Size(1207, 330)
        Bg1.TabIndex = 89
        Bg1.TabStop = False
        ' 
        ' Status_TEXT
        ' 
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 10.0F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(1298, 360)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(383, 19)
        Status_TEXT.TabIndex = 73
        ' 
        ' BT_DeleteAccount
        ' 
        BT_DeleteAccount.BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40))
        BT_DeleteAccount.Cursor = Cursors.Hand
        BT_DeleteAccount.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_DeleteAccount.ForeColor = Color.White
        BT_DeleteAccount.Location = New Point(248, 655)
        BT_DeleteAccount.Name = "BT_DeleteAccount"
        BT_DeleteAccount.Size = New Size(340, 50)
        BT_DeleteAccount.TabIndex = 88
        BT_DeleteAccount.Text = "Delete this account permanently"
        BT_DeleteAccount.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' DzPassword_BOX
        ' 
        DzPassword_BOX.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        DzPassword_BOX.BorderStyle = BorderStyle.FixedSingle
        DzPassword_BOX.Font = New Font("Segoe UI", 10.5F)
        DzPassword_BOX.ForeColor = Color.White
        DzPassword_BOX.Location = New Point(248, 603)
        DzPassword_BOX.Name = "DzPassword_BOX"
        DzPassword_BOX.Size = New Size(260, 26)
        DzPassword_BOX.TabIndex = 87
        DzPassword_BOX.UseSystemPasswordChar = True
        DzPassword_BOX.Visible = False
        ' 
        ' DzPassword_LBL
        ' 
DzPassword_LBL.AutoSize = True
        DzPassword_LBL.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DzPassword_LBL.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        DzPassword_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        DzPassword_LBL.ForeColor = Color.Gainsboro
        DzPassword_LBL.Location = New Point(125, 607)
        DzPassword_LBL.Name = "DzPassword_LBL"
        DzPassword_LBL.Size = New Size(105, 15)
        DzPassword_LBL.TabIndex = 86
        DzPassword_LBL.Text = "Current password"
        DzPassword_LBL.Visible = False
        ' 
        ' DzNote_META
        ' 
        DzNote_META.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        DzNote_META.Font = New Font("Segoe UI", 9.25F)
        DzNote_META.ForeColor = Color.Silver
        DzNote_META.Location = New Point(125, 537)
        DzNote_META.Name = "DzNote_META"
        DzNote_META.Size = New Size(1100, 58)
        DzNote_META.TabIndex = 85
        DzNote_META.Text = resources.GetString("DzNote_META.Text")
        ' 
        ' DzHeader_LBL
        ' 
DzHeader_LBL.AutoSize = True
        DzHeader_LBL.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DzHeader_LBL.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        DzHeader_LBL.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        DzHeader_LBL.ForeColor = Color.White
        DzHeader_LBL.Location = New Point(125, 500)
        DzHeader_LBL.Name = "DzHeader_LBL"
        DzHeader_LBL.Size = New Size(94, 19)
        DzHeader_LBL.TabIndex = 84
        DzHeader_LBL.Text = "Danger zone"
        ' 
        ' BT_ChangePassword
        ' 
        BT_ChangePassword.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_ChangePassword.Cursor = Cursors.Hand
        BT_ChangePassword.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        BT_ChangePassword.ForeColor = Color.White
        BT_ChangePassword.Location = New Point(1421, 313)
        BT_ChangePassword.Name = "BT_ChangePassword"
        BT_ChangePassword.Size = New Size(187, 42)
        BT_ChangePassword.TabIndex = 81
        BT_ChangePassword.Text = "Change password"
        BT_ChangePassword.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PwConfirm_BOX
        ' 
        PwConfirm_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwConfirm_BOX.BorderStyle = BorderStyle.FixedSingle
        PwConfirm_BOX.Font = New Font("Segoe UI", 10.5F)
        PwConfirm_BOX.ForeColor = Color.White
        PwConfirm_BOX.Location = New Point(1421, 273)
        PwConfirm_BOX.Name = "PwConfirm_BOX"
        PwConfirm_BOX.Size = New Size(260, 26)
        PwConfirm_BOX.TabIndex = 80
        PwConfirm_BOX.UseSystemPasswordChar = True
        ' 
        ' PwConfirm_LBL
        ' 
        PwConfirm_LBL.AutoSize = True
        PwConfirm_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwConfirm_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        PwConfirm_LBL.ForeColor = Color.Gainsboro
        PwConfirm_LBL.Location = New Point(1298, 277)
        PwConfirm_LBL.Name = "PwConfirm_LBL"
        PwConfirm_LBL.Size = New Size(79, 15)
        PwConfirm_LBL.TabIndex = 79
        PwConfirm_LBL.Text = "Confirm new"
        ' 
        ' PwNew_BOX
        ' 
        PwNew_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwNew_BOX.BorderStyle = BorderStyle.FixedSingle
        PwNew_BOX.Font = New Font("Segoe UI", 10.5F)
        PwNew_BOX.ForeColor = Color.White
        PwNew_BOX.Location = New Point(1421, 237)
        PwNew_BOX.Name = "PwNew_BOX"
        PwNew_BOX.Size = New Size(260, 26)
        PwNew_BOX.TabIndex = 78
        PwNew_BOX.UseSystemPasswordChar = True
        ' 
        ' PwNew_LBL
        ' 
        PwNew_LBL.AutoSize = True
        PwNew_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwNew_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        PwNew_LBL.ForeColor = Color.Gainsboro
        PwNew_LBL.Location = New Point(1298, 241)
        PwNew_LBL.Name = "PwNew_LBL"
        PwNew_LBL.Size = New Size(88, 15)
        PwNew_LBL.TabIndex = 77
        PwNew_LBL.Text = "New password"
        ' 
        ' PwCurrent_BOX
        ' 
        PwCurrent_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwCurrent_BOX.BorderStyle = BorderStyle.FixedSingle
        PwCurrent_BOX.Font = New Font("Segoe UI", 10.5F)
        PwCurrent_BOX.ForeColor = Color.White
        PwCurrent_BOX.Location = New Point(1421, 201)
        PwCurrent_BOX.Name = "PwCurrent_BOX"
        PwCurrent_BOX.Size = New Size(260, 26)
        PwCurrent_BOX.TabIndex = 76
        PwCurrent_BOX.UseSystemPasswordChar = True
        ' 
        ' PwCurrent_LBL
        ' 
        PwCurrent_LBL.AutoSize = True
        PwCurrent_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwCurrent_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        PwCurrent_LBL.ForeColor = Color.Gainsboro
        PwCurrent_LBL.Location = New Point(1298, 205)
        PwCurrent_LBL.Name = "PwCurrent_LBL"
        PwCurrent_LBL.Size = New Size(105, 15)
        PwCurrent_LBL.TabIndex = 75
        PwCurrent_LBL.Text = "Current password"
        ' 
        ' PwUsername_BOX
        ' 
        PwUsername_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwUsername_BOX.BorderStyle = BorderStyle.FixedSingle
        PwUsername_BOX.Font = New Font("Segoe UI", 10.5F)
        PwUsername_BOX.ForeColor = Color.White
        PwUsername_BOX.Location = New Point(1421, 165)
        PwUsername_BOX.Name = "PwUsername_BOX"
        PwUsername_BOX.Size = New Size(260, 26)
        PwUsername_BOX.TabIndex = 83
        PwUsername_BOX.Visible = False
        ' 
        ' PwUsername_LBL
        ' 
        PwUsername_LBL.AutoSize = True
        PwUsername_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwUsername_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        PwUsername_LBL.ForeColor = Color.Gainsboro
        PwUsername_LBL.Location = New Point(1298, 169)
        PwUsername_LBL.Name = "PwUsername_LBL"
        PwUsername_LBL.Size = New Size(114, 15)
        PwUsername_LBL.TabIndex = 82
        PwUsername_LBL.Text = "Choose a username"
        PwUsername_LBL.Visible = False
        ' 
        ' PwHeader_LBL
        ' 
        PwHeader_LBL.AutoSize = True
        PwHeader_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwHeader_LBL.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        PwHeader_LBL.ForeColor = Color.White
        PwHeader_LBL.Location = New Point(1298, 129)
        PwHeader_LBL.Name = "PwHeader_LBL"
        PwHeader_LBL.Size = New Size(144, 21)
        PwHeader_LBL.TabIndex = 74
        PwHeader_LBL.Text = "Change password"
        ' 
        ' BT_RevokeAll
        ' 
        BT_RevokeAll.BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40))
        BT_RevokeAll.Cursor = Cursors.Hand
        BT_RevokeAll.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_RevokeAll.ForeColor = Color.White
        BT_RevokeAll.Location = New Point(929, 379)
        BT_RevokeAll.Name = "BT_RevokeAll"
        BT_RevokeAll.Size = New Size(340, 50)
        BT_RevokeAll.TabIndex = 72
        BT_RevokeAll.Text = "Sign out on ALL devices"
        BT_RevokeAll.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' BT_RefreshSession
        ' 
        BT_RefreshSession.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_RefreshSession.Cursor = Cursors.Hand
        BT_RefreshSession.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        BT_RefreshSession.ForeColor = Color.White
        BT_RefreshSession.Location = New Point(62, 379)
        BT_RefreshSession.Name = "BT_RefreshSession"
        BT_RefreshSession.Size = New Size(260, 50)
        BT_RefreshSession.TabIndex = 71
        BT_RefreshSession.Text = "Extend session"
        BT_RefreshSession.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Info_META
        ' 
        Info_META.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        Info_META.Font = New Font("Consolas", 11.25F)
        Info_META.ForeColor = Color.Gainsboro
        Info_META.Location = New Point(62, 120)
        Info_META.Name = "Info_META"
        Info_META.Size = New Size(1207, 235)
        Info_META.TabIndex = 70
        ' 
        ' Settings_TEXT
        ' 
        Settings_TEXT.AutoSize = True
        Settings_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_TEXT.Font = New Font("GeForce", 24.0F, FontStyle.Bold)
        Settings_TEXT.ForeColor = Color.White
        Settings_TEXT.Location = New Point(62, 43)
        Settings_TEXT.Name = "Settings_TEXT"
        Settings_TEXT.Size = New Size(285, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Sessions / Security"
        ' 
        ' Settings_Panel
        ' 
        Settings_Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Settings_Panel.AutoScroll = True
        Settings_Panel.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_Panel.Controls.Add(Settings_TEXT)
        Settings_Panel.Controls.Add(Info_META)
        Settings_Panel.Controls.Add(BT_RefreshSession)
        Settings_Panel.Controls.Add(BT_RevokeAll)
        Settings_Panel.Controls.Add(PwHeader_LBL)
        Settings_Panel.Controls.Add(DzHeader_LBL)
        Settings_Panel.Controls.Add(PwUsername_LBL)
        Settings_Panel.Controls.Add(PwUsername_BOX)
        Settings_Panel.Controls.Add(PwCurrent_LBL)
        Settings_Panel.Controls.Add(PwCurrent_BOX)
        Settings_Panel.Controls.Add(PwNew_LBL)
        Settings_Panel.Controls.Add(PwNew_BOX)
        Settings_Panel.Controls.Add(PwConfirm_LBL)
        Settings_Panel.Controls.Add(PwConfirm_BOX)
        Settings_Panel.Controls.Add(BT_ChangePassword)
        Settings_Panel.Controls.Add(DzNote_META)
        Settings_Panel.Controls.Add(DzPassword_LBL)
        Settings_Panel.Controls.Add(DzPassword_BOX)
        Settings_Panel.Controls.Add(BT_DeleteAccount)
        Settings_Panel.Controls.Add(Status_TEXT)
        Settings_Panel.Controls.Add(Bg1)
        Settings_Panel.Location = New Point(80, 160)
        Settings_Panel.Name = "Settings_Panel"
        Settings_Panel.Size = New Size(1760, 840)
        Settings_Panel.TabIndex = 45
        ' 
        ' Base_Connect_Security
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1070)
        Controls.Add(BT_Back)
        Controls.Add(Dim_2)
        Controls.Add(Dim_1)
        Controls.Add(Dim_Top)
        Controls.Add(Settings_Panel)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Connect_Security"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        CType(Bg1, ComponentModel.ISupportInitialize).EndInit()
        Settings_Panel.ResumeLayout(False)
        Settings_Panel.PerformLayout()
        ResumeLayout(False)
    End Sub
    Friend WithEvents BT_Back As Label
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
    Friend WithEvents Bg1 As PictureBox
    Friend WithEvents Status_TEXT As Label
    Friend WithEvents BT_DeleteAccount As Label
    Friend WithEvents DzPassword_BOX As TextBox
    Friend WithEvents DzPassword_LBL As Label
    Friend WithEvents DzNote_META As Label
    Friend WithEvents DzHeader_LBL As Label
    Friend WithEvents BT_ChangePassword As Label
    Friend WithEvents PwConfirm_BOX As TextBox
    Friend WithEvents PwConfirm_LBL As Label
    Friend WithEvents PwNew_BOX As TextBox
    Friend WithEvents PwNew_LBL As Label
    Friend WithEvents PwCurrent_BOX As TextBox
    Friend WithEvents PwCurrent_LBL As Label
    Friend WithEvents PwUsername_BOX As TextBox
    Friend WithEvents PwUsername_LBL As Label
    Friend WithEvents PwHeader_LBL As Label
    Friend WithEvents BT_RevokeAll As Label
    Friend WithEvents BT_RefreshSession As Label
    Friend WithEvents Info_META As Label
    Friend WithEvents Settings_TEXT As Label
    Friend WithEvents Settings_Panel As Panel
End Class
