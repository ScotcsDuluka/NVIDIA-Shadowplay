<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_KeySet
    Inherits NoCloseForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_KeySet))
        keyset = New Panel()
        Key_Tx = New Label()
        settings_bg = New PictureBox()
        Reset = New Label()
        text_settings = New Label()
        box_settings = New PictureBox()
        settings_top = New PictureBox()
        action_fn = New Label()
        PictureBox6 = New PictureBox()
        PictureBox9 = New PictureBox()
        PictureBox1 = New PictureBox()
        keyset.SuspendLayout()
        CType(settings_bg, ComponentModel.ISupportInitialize).BeginInit()
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' keyset
        ' 
        keyset.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        keyset.BackColor = Color.Red
        keyset.Controls.Add(Key_Tx)
        keyset.Controls.Add(settings_bg)
        keyset.Location = New Point(695, 160)
        keyset.Name = "keyset"
        keyset.Size = New Size(1145, 840)
        keyset.TabIndex = 45
        ' 
        ' Key_Tx
        ' 
        Key_Tx.AutoSize = True
        Key_Tx.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Key_Tx.Font = New Font("Segoe UI", 17F, FontStyle.Bold)
        Key_Tx.ForeColor = Color.White
        Key_Tx.Location = New Point(62, 43)
        Key_Tx.Name = "Key_Tx"
        Key_Tx.Size = New Size(222, 31)
        Key_Tx.TabIndex = 51
        Key_Tx.Text = "Keyboard Shortcut "
        ' 
        ' settings_bg
        ' 
        settings_bg.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        settings_bg.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_bg.Location = New Point(0, 4)
        settings_bg.Name = "settings_bg"
        settings_bg.Size = New Size(1145, 836)
        settings_bg.TabIndex = 1
        settings_bg.TabStop = False
        ' 
        ' Reset
        ' 
        Reset.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Reset.Cursor = Cursors.Hand
        Reset.Font = New Font("Segoe UI", 14F)
        Reset.ForeColor = Color.White
        Reset.Location = New Point(465, 300)
        Reset.Name = "Reset"
        Reset.Size = New Size(200, 70)
        Reset.TabIndex = 70
        Reset.Text = "Reset"
        Reset.TextAlign = ContentAlignment.MiddleCenter
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
        text_settings.Text = "Keyboard Shortcut"
        text_settings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Color.Black
        box_settings.Location = New Point(465, 160)
        box_settings.Name = "box_settings"
        box_settings.Size = New Size(200, 49)
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
        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(465, 220)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 70)
        action_fn.TabIndex = 58
        action_fn.Text = "s"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PictureBox6
        ' 
        PictureBox6.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox6.Location = New Point(-3, -16)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(1951, 176)
        PictureBox6.TabIndex = 71
        PictureBox6.TabStop = False
        ' 
        ' PictureBox9
        ' 
        PictureBox9.BackColor = Color.Blue
        PictureBox9.BackgroundImageLayout = ImageLayout.None
        PictureBox9.Location = New Point(1130, 1000)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(80, 80)
        PictureBox9.TabIndex = 91
        PictureBox9.TabStop = False
        PictureBox9.Visible = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.Blue
        PictureBox1.BackgroundImageLayout = ImageLayout.None
        PictureBox1.Location = New Point(1840, 678)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(80, 80)
        PictureBox1.TabIndex = 92
        PictureBox1.TabStop = False
        PictureBox1.Visible = False
        ' 
        ' Base_KeySet
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Red
        ClientSize = New Size(1920, 1080)
        Controls.Add(PictureBox1)
        Controls.Add(PictureBox9)
        Controls.Add(Reset)
        Controls.Add(text_settings)
        Controls.Add(settings_top)
        Controls.Add(PictureBox6)
        Controls.Add(keyset)
        Controls.Add(action_fn)
        Controls.Add(box_settings)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_KeySet"
        ShowInTaskbar = False
        Text = "Keyboard Shortcut "
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        keyset.ResumeLayout(False)
        keyset.PerformLayout()
        CType(settings_bg, ComponentModel.ISupportInitialize).EndInit()
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents keyset As Panel
    Friend WithEvents Reset As Label
    Friend WithEvents action_fn As Label
    Friend WithEvents text_settings As Label
    Friend WithEvents Key_Tx As Label
    Friend WithEvents settings_bg As PictureBox
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents box_settings As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents PictureBox9 As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
End Class
