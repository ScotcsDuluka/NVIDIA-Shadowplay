<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect_Create
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect_Create))
        Settings_Panel = New Panel()
        Settings_TEXT = New Label()
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Info_TEXT = New Label()
        Username_LBL = New Label()
        Username_BOX = New TextBox()
        Password_LBL = New Label()
        Password_BOX = New TextBox()
        Confirm_LBL = New Label()
        Confirm_BOX = New TextBox()
        BT_Create = New Label()
        Status_TEXT = New Label()
        Settings_Panel.SuspendLayout()
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
        Settings_Panel.Controls.Add(Info_TEXT)
        Settings_Panel.Controls.Add(Username_LBL)
        Settings_Panel.Controls.Add(Username_BOX)
        Settings_Panel.Controls.Add(Password_LBL)
        Settings_Panel.Controls.Add(Password_BOX)
        Settings_Panel.Controls.Add(Confirm_LBL)
        Settings_Panel.Controls.Add(Confirm_BOX)
        Settings_Panel.Controls.Add(BT_Create)
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
        Settings_TEXT.Font = New Font("GeForce", 24.0F, FontStyle.Bold)
        Settings_TEXT.ForeColor = Color.White
        Settings_TEXT.Location = New Point(62, 43)
        Settings_TEXT.Name = "Settings_TEXT"
        Settings_TEXT.Size = New Size(330, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Create your Duluka Account"
        ' 
        ' Info_TEXT
        ' 
        Info_TEXT.Font = New Font("Segoe UI", 10.5F)
        Info_TEXT.ForeColor = Color.Gainsboro
        Info_TEXT.Location = New Point(62, 110)
        Info_TEXT.Name = "Info_TEXT"
        Info_TEXT.Size = New Size(1100, 40)
        Info_TEXT.TabIndex = 70
        Info_TEXT.Text = "One account for the Duluka ecosystem. You can link GitHub later as an authentication method."
        ' 
        ' Username_LBL
        ' 
        Username_LBL.AutoSize = True
        Username_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Username_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        Username_LBL.ForeColor = Color.Gainsboro
        Username_LBL.Location = New Point(62, 175)
        Username_LBL.Name = "Username_LBL"
        Username_LBL.Size = New Size(64, 15)
        Username_LBL.TabIndex = 71
        Username_LBL.Text = "Username"
        ' 
        ' Username_BOX
        ' 
        Username_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        Username_BOX.BorderStyle = BorderStyle.FixedSingle
        Username_BOX.Font = New Font("Segoe UI", 10.5F)
        Username_BOX.ForeColor = Color.White
        Username_BOX.Location = New Point(62, 195)
        Username_BOX.Name = "Username_BOX"
        Username_BOX.Size = New Size(300, 25)
        Username_BOX.TabIndex = 72
        ' 
        ' Password_LBL
        ' 
        Password_LBL.AutoSize = True
        Password_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Password_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        Password_LBL.ForeColor = Color.Gainsboro
        Password_LBL.Location = New Point(62, 240)
        Password_LBL.Name = "Password_LBL"
        Password_LBL.Size = New Size(63, 15)
        Password_LBL.TabIndex = 73
        Password_LBL.Text = "Password"
        ' 
        ' Password_BOX
        ' 
        Password_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        Password_BOX.BorderStyle = BorderStyle.FixedSingle
        Password_BOX.Font = New Font("Segoe UI", 10.5F)
        Password_BOX.ForeColor = Color.White
        Password_BOX.Location = New Point(62, 260)
        Password_BOX.Name = "Password_BOX"
        Password_BOX.Size = New Size(300, 25)
        Password_BOX.TabIndex = 74
        Password_BOX.UseSystemPasswordChar = True
        ' 
        ' Confirm_LBL
        ' 
        Confirm_LBL.AutoSize = True
        Confirm_LBL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Confirm_LBL.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        Confirm_LBL.ForeColor = Color.Gainsboro
        Confirm_LBL.Location = New Point(62, 305)
        Confirm_LBL.Name = "Confirm_LBL"
        Confirm_LBL.Size = New Size(105, 15)
        Confirm_LBL.TabIndex = 75
        Confirm_LBL.Text = "Confirm Password"
        ' 
        ' Confirm_BOX
        ' 
        Confirm_BOX.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        Confirm_BOX.BorderStyle = BorderStyle.FixedSingle
        Confirm_BOX.Font = New Font("Segoe UI", 10.5F)
        Confirm_BOX.ForeColor = Color.White
        Confirm_BOX.Location = New Point(62, 325)
        Confirm_BOX.Name = "Confirm_BOX"
        Confirm_BOX.Size = New Size(300, 25)
        Confirm_BOX.TabIndex = 76
        Confirm_BOX.UseSystemPasswordChar = True
        ' 
        ' BT_Create
        ' 
        BT_Create.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_Create.Cursor = Cursors.Hand
        BT_Create.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        BT_Create.ForeColor = Color.White
        BT_Create.Location = New Point(62, 375)
        BT_Create.Name = "BT_Create"
        BT_Create.Size = New Size(260, 46)
        BT_Create.TabIndex = 77
        BT_Create.Text = "Create Account"
        BT_Create.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Status_TEXT
        ' 
        Status_TEXT.AutoSize = True
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 9.75F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(62, 450)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(0, 17)
        Status_TEXT.TabIndex = 78
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
        BT_Back.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
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
        ' Base_Connect_Create
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
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
        Name = "Base_Connect_Create"
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
    Friend WithEvents Info_TEXT As Label
    Friend WithEvents Username_LBL As Label
    Friend WithEvents Username_BOX As TextBox
    Friend WithEvents Password_LBL As Label
    Friend WithEvents Password_BOX As TextBox
    Friend WithEvents Confirm_LBL As Label
    Friend WithEvents Confirm_BOX As TextBox
    Friend WithEvents BT_Create As Label
    Friend WithEvents Status_TEXT As Label
End Class
