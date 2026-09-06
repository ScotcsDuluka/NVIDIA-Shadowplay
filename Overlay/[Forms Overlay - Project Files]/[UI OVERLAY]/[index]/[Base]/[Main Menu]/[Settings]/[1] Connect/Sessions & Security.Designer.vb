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
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Info_META = New Label()
        BT_RefreshSession = New Label()
        BT_RevokeAll = New Label()
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
        Settings_Panel.Controls.Add(Info_META)
        Settings_Panel.Controls.Add(BT_RefreshSession)
        Settings_Panel.Controls.Add(BT_RevokeAll)
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
        Settings_TEXT.Size = New Size(128, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Sessions & Security"
        ' 
        ' Info_META
        ' 
        Info_META.Font = New Font("Consolas", 11.25F)
        Info_META.ForeColor = Color.Gainsboro
        Info_META.Location = New Point(62, 120)
        Info_META.Name = "Info_META"
        Info_META.Size = New Size(1100, 140)
        Info_META.TabIndex = 70
        Info_META.Text = ""
        ' 
        ' BT_RefreshSession
        ' 
        BT_RefreshSession.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_RefreshSession.Cursor = Cursors.Hand
        BT_RefreshSession.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
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
        ' Status_TEXT
        ' 
        Status_TEXT.AutoSize = True
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 10.0F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(62, 500)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(60, 19)
        Status_TEXT.TabIndex = 73
        Status_TEXT.Text = ""
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
        ' Base_Connect_Security
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
    Friend WithEvents Status_TEXT As Label
End Class
