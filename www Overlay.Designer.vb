<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class www
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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(www))
        WebView21 = New Microsoft.Web.WebView2.WinForms.WebView2()
        key = New Timer(components)
        Timer1 = New Timer(components)
        Timer2 = New Timer(components)
        key_ne = New Timer(components)
        url = New TextBox()
        menu_p = New Timer(components)
        newff = New Timer(components)
        CType(WebView21, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' WebView21
        ' 
        WebView21.AllowExternalDrop = True
        WebView21.CreationProperties = Nothing
        WebView21.DefaultBackgroundColor = Drawing.Color.White
        WebView21.Dock = DockStyle.Fill
        WebView21.Location = New System.Drawing.Point(0, 0)
        WebView21.Name = "WebView21"
        WebView21.Size = New System.Drawing.Size(884, 561)
        WebView21.Source = New Uri("https://www.google.com/", UriKind.Absolute)
        WebView21.TabIndex = 0
        WebView21.ZoomFactor = 1R
        ' 
        ' key
        ' 
        key.Enabled = True
        key.Interval = 1
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 1
        ' 
        ' Timer2
        ' 
        Timer2.Interval = 1
        ' 
        ' key_ne
        ' 
        key_ne.Enabled = True
        key_ne.Interval = 1
        ' 
        ' url
        ' 
        url.Dock = DockStyle.Top
        url.Location = New System.Drawing.Point(0, 0)
        url.Multiline = True
        url.Name = "url"
        url.Size = New System.Drawing.Size(884, 23)
        url.TabIndex = 1
        url.Text = "https://"
        url.Visible = False
        ' 
        ' menu_p
        ' 
        menu_p.Enabled = True
        menu_p.Interval = 1
        ' 
        ' newff
        ' 
        newff.Interval = 1000
        ' 
        ' www
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(884, 561)
        ControlBox = False
        Controls.Add(url)
        Controls.Add(WebView21)
        DoubleBuffered = True
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        MaximizeBox = False
        MdiChildrenMinimizedAnchorBottom = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(160, 90)
        Name = "www"
        Opacity = 0R
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "Alt + Caps lock - Hide/Show"
        TopMost = True
        CType(WebView21, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents WebView21 As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents key As Timer
    Friend WithEvents Timer1 As Timer
    Friend WithEvents Timer2 As Timer
    Friend WithEvents key_ne As Timer
    Friend WithEvents url As TextBox
    Friend WithEvents menu_p As Timer
    Friend WithEvents newff As Timer
End Class
