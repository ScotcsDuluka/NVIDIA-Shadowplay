<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect_Providers
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Connect_Providers))
        Settings_Panel = New Panel()
        Settings_TEXT = New Label()
        List_PANEL = New Panel()
        BT_LinkNew = New Label()
        BT_RefreshProviders = New Label()
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
        Settings_Panel.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Settings_Panel.Controls.Add(Settings_TEXT)
        Settings_Panel.Controls.Add(List_PANEL)
        Settings_Panel.Controls.Add(BT_LinkNew)
        Settings_Panel.Controls.Add(BT_RefreshProviders)
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
        Settings_TEXT.Size = New Size(236, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Linked accounts"
        ' 
        ' List_PANEL
        ' 
        List_PANEL.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        List_PANEL.AutoScroll = True
        List_PANEL.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        List_PANEL.Location = New Point(62, 120)
        List_PANEL.Name = "List_PANEL"
        List_PANEL.Size = New Size(1636, 600)
        List_PANEL.TabIndex = 80
        ' 
        ' BT_LinkNew
        ' 
        BT_LinkNew.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        BT_LinkNew.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_LinkNew.Cursor = Cursors.Hand
        BT_LinkNew.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        BT_LinkNew.ForeColor = Color.White
        BT_LinkNew.Location = New Point(62, 740)
        BT_LinkNew.Name = "BT_LinkNew"
        BT_LinkNew.Size = New Size(180, 44)
        BT_LinkNew.TabIndex = 81
        BT_LinkNew.Text = "Link GitHub"
        BT_LinkNew.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' BT_RefreshProviders
        ' 
        BT_RefreshProviders.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        BT_RefreshProviders.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_RefreshProviders.Cursor = Cursors.Hand
        BT_RefreshProviders.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        BT_RefreshProviders.ForeColor = Color.White
        BT_RefreshProviders.Location = New Point(260, 740)
        BT_RefreshProviders.Name = "BT_RefreshProviders"
        BT_RefreshProviders.Size = New Size(160, 44)
        BT_RefreshProviders.TabIndex = 82
        BT_RefreshProviders.Text = "Refresh"
        BT_RefreshProviders.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Status_TEXT
        ' 
        Status_TEXT.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Status_TEXT.AutoSize = True
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 10F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(440, 752)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(0, 19)
        Status_TEXT.TabIndex = 83
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
        ' Base_Connect_Providers
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
        Name = "Base_Connect_Providers"
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
    Friend WithEvents List_PANEL As Panel
    Friend WithEvents BT_LinkNew As Label
    Friend WithEvents BT_RefreshProviders As Label
    Friend WithEvents Status_TEXT As Label
End Class
