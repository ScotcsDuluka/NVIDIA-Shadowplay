<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect_Setup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect_Setup))
        Settings_Panel = New Panel()
        Settings_TEXT = New Label()
        Auth_PROMPT = New Label()
        Card_PANEL = New Panel()
        Username_LBL = New Label()
        Username_BOX = New TextBox()
        Password_LBL = New Label()
        Password_BOX = New TextBox()
        Confirm_LBL = New Label()
        Confirm_BOX = New TextBox()
        BT_Setup = New Label()
        Setup_NOTE = New Label()
        Status_TEXT = New Label()
        BT_Back = New Label()
        Dim_Top = New PictureBox()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Box_Bg = New PictureBox()
        Settings_Panel.SuspendLayout()
        Card_PANEL.SuspendLayout()
        CType(Dim_Top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).BeginInit()
        CType(Box_Bg, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Settings_Panel
        ' 
        Settings_Panel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Settings_Panel.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_Panel.Controls.Add(Settings_TEXT)
        Settings_Panel.Controls.Add(Auth_PROMPT)
        Settings_Panel.Controls.Add(Card_PANEL)
        Settings_Panel.Controls.Add(Setup_NOTE)
        Settings_Panel.Controls.Add(Status_TEXT)
        Settings_Panel.Controls.Add(BT_Back)
        Settings_Panel.Controls.Add(Box_Bg)
        Settings_Panel.Location = New Point(80, 160)
        Settings_Panel.Name = "Settings_Panel"
        Settings_Panel.Size = New Size(1760, 840)
        Settings_Panel.TabIndex = 45
        ' 
        ' Settings_TEXT
        ' 
        Settings_TEXT.Anchor = AnchorStyles.Top
        Settings_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_TEXT.Font = New Font("GeForce", 24F, FontStyle.Bold)
        Settings_TEXT.ForeColor = Color.White
        Settings_TEXT.Location = New Point(700, 43)
        Settings_TEXT.Name = "Settings_TEXT"
        Settings_TEXT.Size = New Size(360, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Duluka Account"
        Settings_TEXT.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Auth_PROMPT
        ' 
        Auth_PROMPT.Anchor = AnchorStyles.Top
        Auth_PROMPT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Auth_PROMPT.Font = New Font("Segoe UI", 11.25F)
        Auth_PROMPT.ForeColor = Color.Gainsboro
        Auth_PROMPT.Location = New Point(700, 96)
        Auth_PROMPT.Name = "Auth_PROMPT"
        Auth_PROMPT.Size = New Size(360, 20)
        Auth_PROMPT.TabIndex = 69
        Auth_PROMPT.Text = "Set up your account"
        Auth_PROMPT.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Card_PANEL
        ' 
        Card_PANEL.Anchor = AnchorStyles.Top
        Card_PANEL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Card_PANEL.Controls.Add(Username_LBL)
        Card_PANEL.Controls.Add(Username_BOX)
        Card_PANEL.Controls.Add(Password_LBL)
        Card_PANEL.Controls.Add(Password_BOX)
        Card_PANEL.Controls.Add(Confirm_LBL)
        Card_PANEL.Controls.Add(Confirm_BOX)
        Card_PANEL.Controls.Add(BT_Setup)
        Card_PANEL.Location = New Point(700, 132)
        Card_PANEL.Name = "Card_PANEL"
        Card_PANEL.Size = New Size(360, 290)
        Card_PANEL.TabIndex = 84
        ' 
        ' Username_LBL
        ' 
        Username_LBL.AutoSize = True
        Username_LBL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Username_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Username_LBL.ForeColor = Color.Gainsboro
        Username_LBL.Location = New Point(24, 20)
        Username_LBL.Name = "Username_LBL"
        Username_LBL.Size = New Size(64, 15)
        Username_LBL.TabIndex = 85
        Username_LBL.Text = "Username"
        ' 
        ' Username_BOX
        ' 
        Username_BOX.BackColor = Color.FromArgb(CByte(30), CByte(34), CByte(38))
        Username_BOX.BorderStyle = BorderStyle.FixedSingle
        Username_BOX.Font = New Font("Segoe UI", 11.25F)
        Username_BOX.ForeColor = Color.White
        Username_BOX.Location = New Point(24, 40)
        Username_BOX.Name = "Username_BOX"
        Username_BOX.Size = New Size(312, 27)
        Username_BOX.TabIndex = 86
        ' 
        ' Password_LBL
        ' 
        Password_LBL.AutoSize = True
        Password_LBL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Password_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Password_LBL.ForeColor = Color.Gainsboro
        Password_LBL.Location = New Point(24, 78)
        Password_LBL.Name = "Password_LBL"
        Password_LBL.Size = New Size(59, 15)
        Password_LBL.TabIndex = 87
        Password_LBL.Text = "Password"
        ' 
        ' Password_BOX
        ' 
        Password_BOX.BackColor = Color.FromArgb(CByte(30), CByte(34), CByte(38))
        Password_BOX.BorderStyle = BorderStyle.FixedSingle
        Password_BOX.Font = New Font("Segoe UI", 11.25F)
        Password_BOX.ForeColor = Color.White
        Password_BOX.Location = New Point(24, 98)
        Password_BOX.Name = "Password_BOX"
        Password_BOX.Size = New Size(312, 27)
        Password_BOX.TabIndex = 88
        Password_BOX.UseSystemPasswordChar = True
        ' 
        ' Confirm_LBL
        ' 
        Confirm_LBL.AutoSize = True
        Confirm_LBL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Confirm_LBL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Confirm_LBL.ForeColor = Color.Gainsboro
        Confirm_LBL.Location = New Point(24, 136)
        Confirm_LBL.Name = "Confirm_LBL"
        Confirm_LBL.Size = New Size(104, 15)
        Confirm_LBL.TabIndex = 89
        Confirm_LBL.Text = "Confirm password"
        ' 
        ' Confirm_BOX
        ' 
        Confirm_BOX.BackColor = Color.FromArgb(CByte(30), CByte(34), CByte(38))
        Confirm_BOX.BorderStyle = BorderStyle.FixedSingle
        Confirm_BOX.Font = New Font("Segoe UI", 11.25F)
        Confirm_BOX.ForeColor = Color.White
        Confirm_BOX.Location = New Point(24, 156)
        Confirm_BOX.Name = "Confirm_BOX"
        Confirm_BOX.Size = New Size(312, 27)
        Confirm_BOX.TabIndex = 90
        Confirm_BOX.UseSystemPasswordChar = True
        ' 
        ' BT_Setup
        ' 
        BT_Setup.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_Setup.Cursor = Cursors.Hand
        BT_Setup.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        BT_Setup.ForeColor = Color.White
        BT_Setup.Location = New Point(24, 202)
        BT_Setup.Name = "BT_Setup"
        BT_Setup.Size = New Size(312, 46)
        BT_Setup.TabIndex = 91
        BT_Setup.Text = "Set Up Account"
        BT_Setup.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Setup_NOTE
        ' 
        Setup_NOTE.Anchor = AnchorStyles.Top
        Setup_NOTE.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Setup_NOTE.Font = New Font("Segoe UI", 8.75F)
        Setup_NOTE.ForeColor = Color.FromArgb(CByte(150), CByte(160), CByte(165))
        Setup_NOTE.Location = New Point(651, 444)
        Setup_NOTE.Name = "Setup_NOTE"
        Setup_NOTE.Size = New Size(458, 15)
        Setup_NOTE.TabIndex = 74
        Setup_NOTE.Text = "Your username is permanent — it cannot be changed later. You will sign in with it and your password."
        Setup_NOTE.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Status_TEXT
        ' 
        Status_TEXT.Anchor = AnchorStyles.Top
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 9.75F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(651, 474)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(458, 17)
        Status_TEXT.TabIndex = 68
        Status_TEXT.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' BT_Back
        ' 
        BT_Back.Anchor = AnchorStyles.Top
        BT_Back.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_Back.Cursor = Cursors.Hand
        BT_Back.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        BT_Back.ForeColor = Color.White
        BT_Back.Location = New Point(651, 514)
        BT_Back.Name = "BT_Back"
        BT_Back.Size = New Size(458, 39)
        BT_Back.TabIndex = 58
        BT_Back.Text = "Back"
        BT_Back.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Dim_Top
        ' 
        Dim_Top.Anchor = AnchorStyles.Top
        Dim_Top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Dim_Top.Location = New Point(731, 160)
        Dim_Top.Name = "Dim_Top"
        Dim_Top.Size = New Size(458, 5)
        Dim_Top.TabIndex = 0
        Dim_Top.TabStop = False
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
        ' Box_Bg
        ' 
        Box_Bg.Anchor = AnchorStyles.Top
        Box_Bg.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Box_Bg.Location = New Point(651, 0)
        Box_Bg.Name = "Box_Bg"
        Box_Bg.Size = New Size(458, 569)
        Box_Bg.TabIndex = 95
        Box_Bg.TabStop = False
        ' 
        ' Base_Connect_Setup
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1070)
        Controls.Add(Dim_2)
        Controls.Add(Dim_1)
        Controls.Add(Dim_Top)
        Controls.Add(Settings_Panel)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Connect_Setup"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        Settings_Panel.ResumeLayout(False)
        Card_PANEL.ResumeLayout(False)
        Card_PANEL.PerformLayout()
        CType(Box_Bg, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Settings_Panel As Panel
    Friend WithEvents Settings_TEXT As Label
    Friend WithEvents Auth_PROMPT As Label
    Friend WithEvents Card_PANEL As Panel
    Friend WithEvents Username_LBL As Label
    Friend WithEvents Username_BOX As TextBox
    Friend WithEvents Password_LBL As Label
    Friend WithEvents Password_BOX As TextBox
    Friend WithEvents Confirm_LBL As Label
    Friend WithEvents Confirm_BOX As TextBox
    Friend WithEvents BT_Setup As Label
    Friend WithEvents Setup_NOTE As Label
    Friend WithEvents Status_TEXT As Label
    Friend WithEvents BT_Back As Label
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
    Friend WithEvents Box_Bg As PictureBox
End Class
