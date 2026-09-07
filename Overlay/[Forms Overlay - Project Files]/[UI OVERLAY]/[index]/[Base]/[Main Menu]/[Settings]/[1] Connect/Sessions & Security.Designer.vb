<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect_Security
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect_Security))
        Settings_Panel = New Panel()
        Settings_TEXT = New Label()
        Info_META = New Label()
        BT_RefreshSession = New Label()
        BT_RevokeAll = New Label()
        PwHeader_LBL = New Label()
        PwUsername_LBL = New Label()
        PwUsername_BOX = New TextBox()
        PwCurrent_LBL = New Label()
        PwCurrent_BOX = New TextBox()
        PwNew_LBL = New Label()
        PwNew_BOX = New TextBox()
        PwConfirm_LBL = New Label()
        PwConfirm_BOX = New TextBox()
        BT_ChangePassword = New Label()
        Status_TEXT = New Label()
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Settings_Panel.SuspendLayout()
        CType(Dim_Top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
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
        ' PwUsername_LBL / PwUsername_BOX MUST be parented here: they are the
        ' FIRST-TIME SETUP row (provider-only accounts choosing their username).
        ' They were instantiated below but never added to the panel — the row
        ' stayed invisible, BT_ChangePassword's handler always read an empty
        ' username and silently dead-ended (the "Set button does nothing" bug).
        Settings_Panel.Controls.Add(PwUsername_LBL)
        Settings_Panel.Controls.Add(PwUsername_BOX)
        Settings_Panel.Controls.Add(PwCurrent_LBL)
        Settings_Panel.Controls.Add(PwCurrent_BOX)
        Settings_Panel.Controls.Add(PwNew_LBL)
        Settings_Panel.Controls.Add(PwNew_BOX)
        Settings_Panel.Controls.Add(PwConfirm_LBL)
        Settings_Panel.Controls.Add(PwConfirm_BOX)
        Settings_Panel.Controls.Add(BT_ChangePassword)
        Settings_Panel.Controls.Add(Status_TEXT)
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
        Settings_TEXT.Size = New Size(285, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Sessions / Security"
        ' 
        ' Info_META
        ' 
        Info_META.Font = New Font("Consolas", 11.25F)
        Info_META.ForeColor = Color.Gainsboro
        Info_META.Location = New Point(62, 120)
        Info_META.Name = "Info_META"
        Info_META.Size = New Size(1100, 140)
        Info_META.TabIndex = 70
        ' 
        ' BT_RefreshSession
        ' 
        BT_RefreshSession.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_RefreshSession.Cursor = Cursors.Hand
        BT_RefreshSession.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        BT_RefreshSession.ForeColor = Color.White
        BT_RefreshSession.Location = New Point(62, 320)
        BT_RefreshSession.Name = "BT_RefreshSession"
        BT_RefreshSession.Size = New Size(260, 50)
        BT_RefreshSession.TabIndex = 71
        BT_RefreshSession.Text = "Extend session"
        BT_RefreshSession.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' BT_RevokeAll
        ' 
        BT_RevokeAll.BackColor = Color.FromArgb(CByte(140), CByte(40), CByte(40))
        BT_RevokeAll.Cursor = Cursors.Hand
        BT_RevokeAll.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_RevokeAll.ForeColor = Color.White
        BT_RevokeAll.Location = New Point(62, 400)
        BT_RevokeAll.Name = "BT_RevokeAll"
        BT_RevokeAll.Size = New Size(340, 50)
        BT_RevokeAll.TabIndex = 72
        BT_RevokeAll.Text = "Sign out on ALL devices"
        BT_RevokeAll.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PwHeader_LBL
        ' 
        PwHeader_LBL.AutoSize = True
        PwHeader_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwHeader_LBL.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        PwHeader_LBL.ForeColor = Color.White
        PwHeader_LBL.Location = New Point(62, 560)
        PwHeader_LBL.Name = "PwHeader_LBL"
        PwHeader_LBL.Size = New Size(127, 19)
        PwHeader_LBL.TabIndex = 74
        PwHeader_LBL.Text = "Change password"
        ' 
        ' PwUsername_LBL
        ' 
        PwUsername_LBL.AutoSize = True
        PwUsername_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwUsername_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        PwUsername_LBL.ForeColor = Color.Gainsboro
        PwUsername_LBL.Location = New Point(62, 600)
        PwUsername_LBL.Name = "PwUsername_LBL"
        PwUsername_LBL.Size = New Size(105, 15)
        PwUsername_LBL.TabIndex = 82
        PwUsername_LBL.Text = "Choose a username"
        PwUsername_LBL.Visible = False
        ' 
        ' PwUsername_BOX
        ' 
        PwUsername_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwUsername_BOX.BorderStyle = BorderStyle.FixedSingle
        PwUsername_BOX.Font = New Font("Segoe UI", 10.5F)
        PwUsername_BOX.ForeColor = Color.White
        PwUsername_BOX.Location = New Point(185, 596)
        PwUsername_BOX.Name = "PwUsername_BOX"
        PwUsername_BOX.Size = New Size(260, 26)
        PwUsername_BOX.TabIndex = 83
        PwUsername_BOX.Visible = False
        ' 
        ' PwCurrent_LBL
        ' 
        PwCurrent_LBL.AutoSize = True
        PwCurrent_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwCurrent_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        PwCurrent_LBL.ForeColor = Color.Gainsboro
        PwCurrent_LBL.Location = New Point(62, 636)
        PwCurrent_LBL.Name = "PwCurrent_LBL"
        PwCurrent_LBL.Size = New Size(105, 15)
        PwCurrent_LBL.TabIndex = 75
        PwCurrent_LBL.Text = "Current password"
        ' 
        ' PwCurrent_BOX
        ' 
        PwCurrent_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwCurrent_BOX.BorderStyle = BorderStyle.FixedSingle
        PwCurrent_BOX.Font = New Font("Segoe UI", 10.5F)
        PwCurrent_BOX.ForeColor = Color.White
        PwCurrent_BOX.Location = New Point(185, 632)
        PwCurrent_BOX.Name = "PwCurrent_BOX"
        PwCurrent_BOX.Size = New Size(260, 26)
        PwCurrent_BOX.TabIndex = 76
        PwCurrent_BOX.UseSystemPasswordChar = True
        ' 
        ' PwNew_LBL
        ' 
        PwNew_LBL.AutoSize = True
        PwNew_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwNew_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        PwNew_LBL.ForeColor = Color.Gainsboro
        PwNew_LBL.Location = New Point(62, 672)
        PwNew_LBL.Name = "PwNew_LBL"
        PwNew_LBL.Size = New Size(88, 15)
        PwNew_LBL.TabIndex = 77
        PwNew_LBL.Text = "New password"
        ' 
        ' PwNew_BOX
        ' 
        PwNew_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwNew_BOX.BorderStyle = BorderStyle.FixedSingle
        PwNew_BOX.Font = New Font("Segoe UI", 10.5F)
        PwNew_BOX.ForeColor = Color.White
        PwNew_BOX.Location = New Point(185, 668)
        PwNew_BOX.Name = "PwNew_BOX"
        PwNew_BOX.Size = New Size(260, 26)
        PwNew_BOX.TabIndex = 78
        PwNew_BOX.UseSystemPasswordChar = True
        ' 
        ' PwConfirm_LBL
        ' 
        PwConfirm_LBL.AutoSize = True
        PwConfirm_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PwConfirm_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        PwConfirm_LBL.ForeColor = Color.Gainsboro
        PwConfirm_LBL.Location = New Point(62, 708)
        PwConfirm_LBL.Name = "PwConfirm_LBL"
        PwConfirm_LBL.Size = New Size(79, 15)
        PwConfirm_LBL.TabIndex = 79
        PwConfirm_LBL.Text = "Confirm new"
        ' 
        ' PwConfirm_BOX
        ' 
        PwConfirm_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        PwConfirm_BOX.BorderStyle = BorderStyle.FixedSingle
        PwConfirm_BOX.Font = New Font("Segoe UI", 10.5F)
        PwConfirm_BOX.ForeColor = Color.White
        PwConfirm_BOX.Location = New Point(185, 704)
        PwConfirm_BOX.Name = "PwConfirm_BOX"
        PwConfirm_BOX.Size = New Size(260, 26)
        PwConfirm_BOX.TabIndex = 80
        PwConfirm_BOX.UseSystemPasswordChar = True
        ' 
        ' BT_ChangePassword
        ' 
        BT_ChangePassword.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_ChangePassword.Cursor = Cursors.Hand
        BT_ChangePassword.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        BT_ChangePassword.ForeColor = Color.White
        BT_ChangePassword.Location = New Point(185, 748)
        BT_ChangePassword.Name = "BT_ChangePassword"
        BT_ChangePassword.Size = New Size(200, 42)
        BT_ChangePassword.TabIndex = 81
        BT_ChangePassword.Text = "Change password"
        BT_ChangePassword.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Status_TEXT
        ' 
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 10F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(62, 812)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(383, 19)
        Status_TEXT.TabIndex = 73
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
        Settings_Panel.ResumeLayout(False)
        Settings_Panel.PerformLayout()
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
    Friend WithEvents Info_META As Label
    Friend WithEvents BT_RefreshSession As Label
    Friend WithEvents BT_RevokeAll As Label
    Friend WithEvents PwHeader_LBL As Label
    Friend WithEvents PwUsername_LBL As Label
    Friend WithEvents PwUsername_BOX As TextBox
    Friend WithEvents PwCurrent_LBL As Label
    Friend WithEvents PwCurrent_BOX As TextBox
    Friend WithEvents PwNew_LBL As Label
    Friend WithEvents PwNew_BOX As TextBox
    Friend WithEvents PwConfirm_LBL As Label
    Friend WithEvents PwConfirm_BOX As TextBox
    Friend WithEvents BT_ChangePassword As Label
    Friend WithEvents Status_TEXT As Label
End Class
