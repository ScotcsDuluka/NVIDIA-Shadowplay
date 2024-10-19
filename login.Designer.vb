<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class login
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(login))
        btnLogin = New Button()
        txtUsername = New TextBox()
        txtPassword = New TextBox()
        Button1 = New Button()
        welcome = New Label()
        Label1 = New Label()
        Label2 = New Label()
        Label3 = New Label()
        PictureBox1 = New PictureBox()
        settings_top = New PictureBox()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnLogin
        ' 
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.Font = New System.Drawing.Font("Segoe UI", 11F, Drawing.FontStyle.Bold)
        btnLogin.ForeColor = Drawing.Color.White
        btnLogin.Location = New System.Drawing.Point(364, 252)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New System.Drawing.Size(92, 37)
        btnLogin.TabIndex = 0
        btnLogin.Text = "Login"
        btnLogin.UseVisualStyleBackColor = True
        ' 
        ' txtUsername
        ' 
        txtUsername.BackColor = Drawing.Color.FromArgb(CByte(33), CByte(35), CByte(38))
        txtUsername.BorderStyle = BorderStyle.None
        txtUsername.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        txtUsername.ForeColor = Drawing.Color.White
        txtUsername.Location = New System.Drawing.Point(156, 125)
        txtUsername.MaxLength = 16
        txtUsername.Multiline = True
        txtUsername.Name = "txtUsername"
        txtUsername.Size = New System.Drawing.Size(248, 26)
        txtUsername.TabIndex = 1
        ' 
        ' txtPassword
        ' 
        txtPassword.BackColor = Drawing.Color.FromArgb(CByte(33), CByte(35), CByte(38))
        txtPassword.BorderStyle = BorderStyle.None
        txtPassword.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        txtPassword.ForeColor = Drawing.Color.White
        txtPassword.Location = New System.Drawing.Point(156, 155)
        txtPassword.MaxLength = 12
        txtPassword.Multiline = True
        txtPassword.Name = "txtPassword"
        txtPassword.Size = New System.Drawing.Size(248, 26)
        txtPassword.TabIndex = 2
        ' 
        ' Button1
        ' 
        Button1.FlatStyle = FlatStyle.Flat
        Button1.Font = New System.Drawing.Font("Segoe UI", 11F, Drawing.FontStyle.Bold)
        Button1.ForeColor = Drawing.Color.White
        Button1.Location = New System.Drawing.Point(266, 252)
        Button1.Name = "Button1"
        Button1.Size = New System.Drawing.Size(92, 37)
        Button1.TabIndex = 3
        Button1.Text = "Close"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' welcome
        ' 
        welcome.Dock = DockStyle.Top
        welcome.Font = New System.Drawing.Font("Segoe UI", 20F, Drawing.FontStyle.Bold)
        welcome.ForeColor = Drawing.Color.White
        welcome.Location = New System.Drawing.Point(0, 0)
        welcome.Name = "welcome"
        welcome.Size = New System.Drawing.Size(468, 122)
        welcome.TabIndex = 4
        welcome.Text = "Use Luka accounts" & vbCrLf & "Login"
        welcome.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Label1
        ' 
        Label1.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        Label1.ForeColor = Drawing.Color.White
        Label1.Location = New System.Drawing.Point(58, 122)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(104, 26)
        Label1.TabIndex = 6
        Label1.Text = "Username"
        Label1.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.Font = New System.Drawing.Font("Segoe UI", 13F, Drawing.FontStyle.Bold)
        Label2.ForeColor = Drawing.Color.White
        Label2.Location = New System.Drawing.Point(61, 154)
        Label2.Name = "Label2"
        Label2.Size = New System.Drawing.Size(104, 26)
        Label2.TabIndex = 7
        Label2.Text = "Password"
        Label2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' Label3
        ' 
        Label3.Cursor = Cursors.Hand
        Label3.Font = New System.Drawing.Font("Segoe UI", 7F, Drawing.FontStyle.Underline)
        Label3.ForeColor = Drawing.Color.White
        Label3.Location = New System.Drawing.Point(316, 177)
        Label3.Name = "Label3"
        Label3.Size = New System.Drawing.Size(104, 26)
        Label3.TabIndex = 8
        Label3.Text = "Forgot password"
        Label3.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Location = New System.Drawing.Point(407, 98)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New System.Drawing.Size(43, 105)
        PictureBox1.TabIndex = 9
        PictureBox1.TabStop = False
        ' 
        ' settings_top
        ' 
        settings_top.BackColor = Drawing.Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New System.Drawing.Point(-29, 0)
        settings_top.Name = "settings_top"
        settings_top.Size = New System.Drawing.Size(550, 5)
        settings_top.TabIndex = 10
        settings_top.TabStop = False
        ' 
        ' login
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        BackColor = Drawing.Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ClientSize = New System.Drawing.Size(468, 301)
        ControlBox = False
        Controls.Add(settings_top)
        Controls.Add(txtPassword)
        Controls.Add(PictureBox1)
        Controls.Add(txtUsername)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(welcome)
        Controls.Add(Button1)
        Controls.Add(btnLogin)
        Controls.Add(Label3)
        ForeColor = Drawing.Color.White
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        MaximizeBox = False
        Name = "login"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Login | Luka accounts"
        TopMost = True
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents btnLogin As Button
    Friend WithEvents txtUsername As TextBox
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents welcome As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents settings_top As PictureBox
End Class
