<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Connect_Profile
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
        Settings_Panel = New Panel()
        Settings_TEXT = New Label()
        Profile_CARD = New Panel()
        AvatarLetter_LABEL = New Label()
        Avatar_PICTURE = New PictureBox()
        BT_ChangeImage = New Label()
        BT_RemoveImage = New Label()
        Name_LABEL = New Label()
        Name_BOX = New TextBox()
        Username_LABEL = New Label()
        Username_VALUE = New Label()
        Username_NOTE = New Label()
        BT_Save = New Label()
        Status_TEXT = New Label()
        Dim_Top = New PictureBox()
        BT_Back = New Label()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Settings_Panel.SuspendLayout()
        Profile_CARD.SuspendLayout()
        CType(Avatar_PICTURE, ComponentModel.ISupportInitialize).BeginInit()
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
        Settings_Panel.Controls.Add(Profile_CARD)
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
        Settings_TEXT.Size = New Size(198, 42)
        Settings_TEXT.TabIndex = 51
        Settings_TEXT.Text = "Edit Profile"
        '
        ' Profile_CARD
        '
        Profile_CARD.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Profile_CARD.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Profile_CARD.Controls.Add(AvatarLetter_LABEL)
        Profile_CARD.Controls.Add(Avatar_PICTURE)
        Profile_CARD.Controls.Add(BT_ChangeImage)
        Profile_CARD.Controls.Add(BT_RemoveImage)
        Profile_CARD.Controls.Add(Name_LABEL)
        Profile_CARD.Controls.Add(Name_BOX)
        Profile_CARD.Controls.Add(Username_LABEL)
        Profile_CARD.Controls.Add(Username_VALUE)
        Profile_CARD.Controls.Add(Username_NOTE)
        Profile_CARD.Controls.Add(BT_Save)
        Profile_CARD.Location = New Point(62, 130)
        Profile_CARD.Name = "Profile_CARD"
        Profile_CARD.Size = New Size(1636, 260)
        Profile_CARD.TabIndex = 90
        '
        ' AvatarLetter_LABEL
        '
        ' Letter fallback (identical to the Account Home avatar): visible only
        ' when no decoded image is pending.
        AvatarLetter_LABEL.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        AvatarLetter_LABEL.Font = New Font("GeForce", 30F, FontStyle.Bold)
        AvatarLetter_LABEL.ForeColor = Color.White
        AvatarLetter_LABEL.Location = New Point(24, 32)
        AvatarLetter_LABEL.Name = "AvatarLetter_LABEL"
        AvatarLetter_LABEL.Size = New Size(96, 96)
        AvatarLetter_LABEL.TabIndex = 91
        AvatarLetter_LABEL.Text = "D"
        AvatarLetter_LABEL.TextAlign = ContentAlignment.MiddleCenter
        '
        ' Avatar_PICTURE
        '
        Avatar_PICTURE.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Avatar_PICTURE.Location = New Point(24, 32)
        Avatar_PICTURE.Name = "Avatar_PICTURE"
        Avatar_PICTURE.Size = New Size(96, 96)
        Avatar_PICTURE.SizeMode = PictureBoxSizeMode.Zoom
        Avatar_PICTURE.TabStop = False
        Avatar_PICTURE.Visible = False
        '
        ' BT_ChangeImage
        '
        BT_ChangeImage.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_ChangeImage.Cursor = Cursors.Hand
        BT_ChangeImage.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        BT_ChangeImage.ForeColor = Color.White
        BT_ChangeImage.Location = New Point(24, 152)
        BT_ChangeImage.Name = "BT_ChangeImage"
        BT_ChangeImage.Size = New Size(180, 44)
        BT_ChangeImage.TabIndex = 92
        BT_ChangeImage.Text = "Change image…"
        BT_ChangeImage.TextAlign = ContentAlignment.MiddleCenter
        '
        ' BT_RemoveImage
        '
        BT_RemoveImage.BackColor = Color.FromArgb(CByte(52), CByte(58), CByte(64))
        BT_RemoveImage.Cursor = Cursors.Hand
        BT_RemoveImage.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        BT_RemoveImage.ForeColor = Color.White
        BT_RemoveImage.Location = New Point(214, 152)
        BT_RemoveImage.Name = "BT_RemoveImage"
        BT_RemoveImage.Size = New Size(130, 44)
        BT_RemoveImage.TabIndex = 93
        BT_RemoveImage.Text = "Remove"
        BT_RemoveImage.TextAlign = ContentAlignment.MiddleCenter
        '
        ' Name_LABEL
        '
        Name_LABEL.AutoSize = True
        Name_LABEL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Name_LABEL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Name_LABEL.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Name_LABEL.Location = New Point(248, 36)
        Name_LABEL.Name = "Name_LABEL"
        Name_LABEL.Size = New Size(100, 15)
        Name_LABEL.TabIndex = 94
        Name_LABEL.Text = "DISPLAY NAME"
        '
        ' Name_BOX
        '
        Name_BOX.BackColor = Color.FromArgb(CByte(30), CByte(33), CByte(36))
        Name_BOX.BorderStyle = BorderStyle.FixedSingle
        Name_BOX.Font = New Font("Segoe UI", 11.25F)
        Name_BOX.ForeColor = Color.White
        Name_BOX.Location = New Point(248, 58)
        Name_BOX.MaxLength = 64
        Name_BOX.Name = "Name_BOX"
        Name_BOX.Size = New Size(620, 30)
        Name_BOX.TabIndex = 95
        '
        ' Username_LABEL
        '
        Username_LABEL.AutoSize = True
        Username_LABEL.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Username_LABEL.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Username_LABEL.ForeColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Username_LABEL.Location = New Point(248, 108)
        Username_LABEL.Name = "Username_LABEL"
        Username_LABEL.Size = New Size(230, 15)
        Username_LABEL.TabIndex = 96
        Username_LABEL.Text = "USERNAME (CANNOT BE CHANGED)"
        '
        ' Username_VALUE
        '
        Username_VALUE.AutoSize = True
        Username_VALUE.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Username_VALUE.Font = New Font("Segoe UI", 11.25F)
        Username_VALUE.ForeColor = Color.Gainsboro
        Username_VALUE.Location = New Point(248, 130)
        Username_VALUE.Name = "Username_VALUE"
        Username_VALUE.Size = New Size(80, 20)
        Username_VALUE.TabIndex = 97
        Username_VALUE.Text = "—"
        '
        ' Username_NOTE
        '
        Username_NOTE.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Username_NOTE.BackColor = Color.FromArgb(CByte(46), CByte(52), CByte(57))
        Username_NOTE.Font = New Font("Segoe UI", 9F, FontStyle.Italic)
        Username_NOTE.ForeColor = Color.Silver
        Username_NOTE.Location = New Point(880, 36)
        Username_NOTE.Name = "Username_NOTE"
        Username_NOTE.Size = New Size(720, 60)
        Username_NOTE.TabIndex = 98
        Username_NOTE.Text = "Your display name and avatar are how other Duluka surfaces show you. " &
            "They are NOT your sign-in identity — changing them never affects your username, " &
            "devices or signed-in sessions."
        '
        ' BT_Save
        '
        BT_Save.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        BT_Save.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_Save.Cursor = Cursors.Hand
        BT_Save.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        BT_Save.ForeColor = Color.White
        BT_Save.Location = New Point(1412, 32)
        BT_Save.Name = "BT_Save"
        BT_Save.Size = New Size(200, 50)
        BT_Save.TabIndex = 99
        BT_Save.Text = "Save"
        BT_Save.TextAlign = ContentAlignment.MiddleCenter
        '
        ' Status_TEXT
        '
        Status_TEXT.AutoSize = True
        Status_TEXT.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Status_TEXT.Font = New Font("Segoe UI", 9.75F)
        Status_TEXT.ForeColor = Color.Silver
        Status_TEXT.Location = New Point(62, 410)
        Status_TEXT.Name = "Status_TEXT"
        Status_TEXT.Size = New Size(0, 17)
        Status_TEXT.TabIndex = 68
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
        ' Base_Connect_Profile
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
        Name = "Base_Connect_Profile"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        Settings_Panel.ResumeLayout(False)
        Settings_Panel.PerformLayout()
        Profile_CARD.ResumeLayout(False)
        Profile_CARD.PerformLayout()
        CType(Avatar_PICTURE, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Settings_Panel As Panel
    Friend WithEvents Settings_TEXT As Label
    Friend WithEvents Profile_CARD As Panel
    Friend WithEvents AvatarLetter_LABEL As Label
    Friend WithEvents Avatar_PICTURE As PictureBox
    Friend WithEvents BT_ChangeImage As Label
    Friend WithEvents BT_RemoveImage As Label
    Friend WithEvents Name_LABEL As Label
    Friend WithEvents Name_BOX As TextBox
    Friend WithEvents Username_LABEL As Label
    Friend WithEvents Username_VALUE As Label
    Friend WithEvents Username_NOTE As Label
    Friend WithEvents BT_Save As Label
    Friend WithEvents BT_Back As Label
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
    Friend WithEvents Status_TEXT As Label
End Class
