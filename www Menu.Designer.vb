<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class www_en
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(www_en))
        settings_top = New PictureBox()
        settings_bg = New PictureBox()
        Menu_www = New Panel()
        O_1 = New Button()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).BeginInit()
        Menu_www.SuspendLayout()
        SuspendLayout()
        ' 
        ' settings_top
        ' 
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Dock = DockStyle.Top
        settings_top.Location = New System.Drawing.Point(0, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(10, 5)
        settings_top.TabIndex = 2
        settings_top.TabStop = False
        ' 
        ' settings_bg
        ' 
        settings_bg.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        settings_bg.Dock = DockStyle.Fill
        settings_bg.Location = New System.Drawing.Point(0, 0)
        settings_bg.Name = "settings_bg"
        settings_bg.Size = New System.Drawing.Size(10, 64)
        settings_bg.TabIndex = 3
        settings_bg.TabStop = False
        ' 
        ' Menu_www
        ' 
        Menu_www.Controls.Add(O_1)
        Menu_www.Controls.Add(settings_top)
        Menu_www.Controls.Add(settings_bg)
        Menu_www.Dock = DockStyle.Top
        Menu_www.Location = New System.Drawing.Point(0, 0)
        Menu_www.Name = "Menu_www"
        Menu_www.Size = New System.Drawing.Size(10, 64)
        Menu_www.TabIndex = 4
        ' 
        ' O_1
        ' 
        O_1.BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        O_1.Cursor = Cursors.Hand
        O_1.FlatStyle = FlatStyle.Flat
        O_1.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        O_1.ForeColor = Drawing.Color.White
        O_1.Location = New System.Drawing.Point(12, 16)
        O_1.Name = "O_1"
        O_1.Size = New System.Drawing.Size(92, 36)
        O_1.TabIndex = 4
        O_1.Text = "New Tab"
        O_1.UseVisualStyleBackColor = False
        ' 
        ' www_en
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.Red
        ClientSize = New System.Drawing.Size(10, 10)
        Controls.Add(Menu_www)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Name = "www_en"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "Menu"
        TopMost = True
        TransparencyKey = Drawing.Color.Red
        WindowState = FormWindowState.Maximized
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_bg, ComponentModel.ISupportInitialize).EndInit()
        Menu_www.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents settings_top As PictureBox
    Friend WithEvents settings_bg As PictureBox
    Friend WithEvents Menu_www As Panel
    Friend WithEvents O_1 As Button
End Class
