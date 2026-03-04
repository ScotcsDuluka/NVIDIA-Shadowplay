<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_KeySet
    Inherits System.Windows.Forms.Form

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
        PictureBox5 = New PictureBox()
        keyset = New Panel()
        Reset = New Label()
        action_fn = New Label()
        bg_fn = New PictureBox()
        text_settings = New Label()
        icon_settings = New Label()
        Key_Tx = New Label()
        settings_bg = New PictureBox()
        settings_top = New PictureBox()
        box_settings = New PictureBox()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        keyset.SuspendLayout()
        CType(bg_fn, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' PictureBox5
        ' 
        PictureBox5.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox5.Cursor = Cursors.Hand
        PictureBox5.Location = New System.Drawing.Point(810, 86)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New System.Drawing.Size(200, 70)
        PictureBox5.TabIndex = 69
        PictureBox5.TabStop = False
        ' 
        ' keyset
        ' 
        keyset.BackColor = Drawing.Color.Red
        keyset.Controls.Add(Reset)
        keyset.Controls.Add(PictureBox5)
        keyset.Controls.Add(action_fn)
        keyset.Controls.Add(bg_fn)
        keyset.Controls.Add(text_settings)
        keyset.Controls.Add(icon_settings)
        keyset.Controls.Add(Key_Tx)
        keyset.Controls.Add(settings_bg)
        keyset.Controls.Add(settings_top)
        keyset.Controls.Add(box_settings)
        keyset.Location = New System.Drawing.Point(12, 12)
        keyset.Name = "keyset"
        keyset.Size = New System.Drawing.Size(1010, 723)
        keyset.TabIndex = 45
        ' 
        ' Reset
        ' 
        Reset.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Reset.Cursor = Cursors.Hand
        Reset.Font = New System.Drawing.Font("Segoe UI", 12F)
        Reset.ForeColor = Drawing.Color.White
        Reset.Location = New System.Drawing.Point(810, 110)
        Reset.Name = "Reset"
        Reset.Size = New System.Drawing.Size(200, 21)
        Reset.TabIndex = 70
        Reset.Text = "Reset"
        Reset.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        action_fn.ForeColor = Drawing.Color.White
        action_fn.Location = New System.Drawing.Point(810, 24)
        action_fn.Name = "action_fn"
        action_fn.Size = New System.Drawing.Size(200, 21)
        action_fn.TabIndex = 58
        action_fn.Text = "Saved"
        action_fn.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' bg_fn
        ' 
        bg_fn.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bg_fn.Cursor = Cursors.Hand
        bg_fn.Location = New System.Drawing.Point(810, 0)
        bg_fn.Name = "bg_fn"
        bg_fn.Size = New System.Drawing.Size(200, 70)
        bg_fn.TabIndex = 57
        bg_fn.TabStop = False
        ' 
        ' text_settings
        ' 
        text_settings.BackColor = Drawing.Color.Black
        text_settings.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        text_settings.ForeColor = Drawing.Color.White
        text_settings.Location = New System.Drawing.Point(0, 14)
        text_settings.Name = "text_settings"
        text_settings.Size = New System.Drawing.Size(200, 21)
        text_settings.TabIndex = 56
        text_settings.Text = "Keyboard Shortcut "
        text_settings.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' icon_settings
        ' 
        icon_settings.AutoSize = True
        icon_settings.BackColor = Drawing.Color.Black
        icon_settings.Font = New System.Drawing.Font("nvgcshare", 75F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        icon_settings.ForeColor = Drawing.Color.White
        icon_settings.Location = New System.Drawing.Point(32, 51)
        icon_settings.Name = "icon_settings"
        icon_settings.Size = New System.Drawing.Size(142, 100)
        icon_settings.TabIndex = 53
        icon_settings.Text = ""
        ' 
        ' Key_Tx
        ' 
        Key_Tx.AutoSize = True
        Key_Tx.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Key_Tx.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        Key_Tx.ForeColor = Drawing.Color.White
        Key_Tx.Location = New System.Drawing.Point(258, 43)
        Key_Tx.Name = "Key_Tx"
        Key_Tx.Size = New System.Drawing.Size(156, 21)
        Key_Tx.TabIndex = 51
        Key_Tx.Text = "Keyboard Shortcut "
        ' 
        ' settings_bg
        ' 
        settings_bg.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_bg.Location = New System.Drawing.Point(230, 4)
        settings_bg.Name = "settings_bg"
        settings_bg.Size = New System.Drawing.Size(550, 596)
        settings_bg.TabIndex = 1
        settings_bg.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(230, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(550, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Drawing.Color.Black
        box_settings.Location = New System.Drawing.Point(0, 0)
        box_settings.Name = "box_settings"
        box_settings.Size = New System.Drawing.Size(200, 200)
        box_settings.TabIndex = 55
        box_settings.TabStop = False
        ' 
        ' Base_KeySet
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(1300, 820)
        Controls.Add(keyset)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "Base_KeySet"
        ShowInTaskbar = False
        Text = "Keyboard Shortcut "
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        WindowState = FormWindowState.Maximized
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        keyset.ResumeLayout(False)
        keyset.PerformLayout()
        CType(bg_fn, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents keyset As Panel
    Friend WithEvents Reset As Label
    Friend WithEvents action_fn As Label
    Friend WithEvents bg_fn As PictureBox
    Friend WithEvents text_settings As Label
    Friend WithEvents icon_settings As Label
    Friend WithEvents Key_Tx As Label
    Friend WithEvents settings_bg As PictureBox
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents box_settings As PictureBox
End Class
