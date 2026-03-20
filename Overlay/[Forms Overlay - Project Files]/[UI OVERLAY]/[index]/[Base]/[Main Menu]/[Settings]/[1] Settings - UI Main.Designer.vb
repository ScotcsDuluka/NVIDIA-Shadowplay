<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Settings
    Inherits NoCloseForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Settings))
        Main_Menu_SET = New Panel()
        Panel = New Panel()
        text_sub = New Label()
        ICON_1 = New Label()
        settings_top = New PictureBox()
        action_1 = New Label()
        Back_btn = New Label()
        text_settings = New Label()
        Block_AM = New PictureBox()
        action_fn = New Label()
        ch = New Label()
        SW_lang = New Label()
        Main_Menu_SET.SuspendLayout()
        Panel.SuspendLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Block_AM, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Main_Menu_SET
        ' 
        Main_Menu_SET.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Main_Menu_SET.BackColor = Drawing.Color.Red
        Main_Menu_SET.Controls.Add(Panel)
        Main_Menu_SET.Location = New System.Drawing.Point(695, 160)
        Main_Menu_SET.Name = "Main_Menu_SET"
        Main_Menu_SET.Size = New System.Drawing.Size(1145, 841)
        Main_Menu_SET.TabIndex = 44
        ' 
        ' Panel
        ' 
        Panel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Panel.Controls.Add(text_sub)
        Panel.Controls.Add(ICON_1)
        Panel.Location = New System.Drawing.Point(0, 4)
        Panel.Name = "Panel"
        Panel.Size = New System.Drawing.Size(1145, 837)
        Panel.TabIndex = 74
        ' 
        ' text_sub
        ' 
        text_sub.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        text_sub.BackColor = Drawing.Color.Transparent
        text_sub.Font = New System.Drawing.Font("Segoe UI Semibold", 13F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        text_sub.ForeColor = Drawing.Color.Gray
        text_sub.Location = New System.Drawing.Point(0, 452)
        text_sub.Name = "text_sub"
        text_sub.Size = New System.Drawing.Size(1145, 52)
        text_sub.TabIndex = 51
        text_sub.Text = "Privacy control"
        text_sub.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' ICON_1
        ' 
        ICON_1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        ICON_1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ICON_1.Font = New System.Drawing.Font("nvgcshare", 120F)
        ICON_1.ForeColor = Drawing.Color.DimGray
        ICON_1.Location = New System.Drawing.Point(0, 0)
        ICON_1.Name = "ICON_1"
        ICON_1.Size = New System.Drawing.Size(1145, 792)
        ICON_1.TabIndex = 74
        ICON_1.Text = ""
        ICON_1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(695, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(1145, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' action_1
        ' 
        action_1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        action_1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        action_1.Cursor = Cursors.Hand
        action_1.Font = New System.Drawing.Font("Segoe UI", 12F)
        action_1.ForeColor = Drawing.Color.White
        action_1.Location = New System.Drawing.Point(108, 607)
        action_1.Name = "action_1"
        action_1.Size = New System.Drawing.Size(200, 70)
        action_1.TabIndex = 72
        action_1.Text = "Turn on"
        action_1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        action_1.Visible = False
        ' 
        ' Back_btn
        ' 
        Back_btn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Back_btn.Cursor = Cursors.Hand
        Back_btn.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        Back_btn.ForeColor = Drawing.Color.White
        Back_btn.Location = New System.Drawing.Point(108, 725)
        Back_btn.Name = "Back_btn"
        Back_btn.Size = New System.Drawing.Size(200, 70)
        Back_btn.TabIndex = 58
        Back_btn.Text = "Back"
        Back_btn.TextAlign = Drawing.ContentAlignment.MiddleCenter
        Back_btn.Visible = False
        ' 
        ' text_settings
        ' 
        text_settings.BackColor = Drawing.Color.Black
        text_settings.Font = New System.Drawing.Font("Segoe UI Semibold", 14F, Drawing.FontStyle.Bold)
        text_settings.ForeColor = Drawing.Color.White
        text_settings.Location = New System.Drawing.Point(465, 160)
        text_settings.Name = "text_settings"
        text_settings.Size = New System.Drawing.Size(200, 50)
        text_settings.TabIndex = 56
        text_settings.Text = "Settings"
        text_settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Block_AM
        ' 
        Block_AM.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Block_AM.Location = New System.Drawing.Point(-3, -16)
        Block_AM.Name = "Block_AM"
        Block_AM.Size = New System.Drawing.Size(1951, 176)
        Block_AM.TabIndex = 73
        Block_AM.TabStop = False
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New System.Drawing.Font("Segoe UI Semibold", 12F, Drawing.FontStyle.Bold)
        action_fn.ForeColor = Drawing.Color.White
        action_fn.Location = New System.Drawing.Point(465, 220)
        action_fn.Name = "action_fn"
        action_fn.Size = New System.Drawing.Size(200, 70)
        action_fn.TabIndex = 74
        action_fn.Text = "Done"
        action_fn.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' ch
        ' 
        ch.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ch.Cursor = Cursors.Hand
        ch.Font = New System.Drawing.Font("Segoe UI Semibold", 12F)
        ch.ForeColor = Drawing.Color.White
        ch.Location = New System.Drawing.Point(465, 380)
        ch.Name = "ch"
        ch.Size = New System.Drawing.Size(200, 70)
        ch.TabIndex = 75
        ch.Text = "Check update"
        ch.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' SW_lang
        ' 
        SW_lang.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        SW_lang.Cursor = Cursors.Hand
        SW_lang.Font = New System.Drawing.Font("Segoe UI Semibold", 12F)
        SW_lang.ForeColor = Drawing.Color.White
        SW_lang.Location = New System.Drawing.Point(465, 300)
        SW_lang.Name = "SW_lang"
        SW_lang.Size = New System.Drawing.Size(200, 70)
        SW_lang.TabIndex = 76
        SW_lang.Text = "Check update"
        SW_lang.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Base_Settings
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(1920, 1080)
        Controls.Add(SW_lang)
        Controls.Add(ch)
        Controls.Add(action_fn)
        Controls.Add(settings_top)
        Controls.Add(text_settings)
        Controls.Add(Block_AM)
        Controls.Add(action_1)
        Controls.Add(Back_btn)
        Controls.Add(Main_Menu_SET)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Base_Settings"
        ShowInTaskbar = False
        Text = "Privacy control"
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        WindowState = FormWindowState.Maximized
        Main_Menu_SET.ResumeLayout(False)
        Panel.ResumeLayout(False)
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(Block_AM, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Main_Menu_SET As Panel
    Friend WithEvents Back_btn As Label
    Friend WithEvents text_settings As Label
    Friend WithEvents text_sub As Label
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents action_1 As Label
    Friend WithEvents Block_AM As PictureBox
    Friend WithEvents Panel As Panel
    Friend WithEvents ICON_1 As Label
    Friend WithEvents action_fn As Label
    Friend WithEvents ch As Label
    Friend WithEvents SW_lang As Label
End Class
