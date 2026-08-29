<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Empty
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Empty))
        Menu_Settings = New Panel()
        Menu_Text = New Label()
        Menu_Top_Dim = New PictureBox()
        BT_Back = New Label()
        Dim_Top = New PictureBox()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Menu_Settings.SuspendLayout()
        CType(Menu_Top_Dim, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Menu_Settings
        ' 
        Menu_Settings.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Menu_Settings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Menu_Settings.Controls.Add(Menu_Text)
        Menu_Settings.Location = New Point(80, 160)
        Menu_Settings.Name = "Menu_Settings"
        Menu_Settings.Size = New Size(1760, 840)
        Menu_Settings.TabIndex = 45
        ' 
        ' Menu_Text
        ' 
        Menu_Text.AutoSize = True
        Menu_Text.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Menu_Text.Font = New Font("GeForce", 24F, FontStyle.Bold)
        Menu_Text.ForeColor = Color.White
        Menu_Text.Location = New Point(62, 43)
        Menu_Text.Name = "Menu_Text"
        Menu_Text.Size = New Size(128, 42)
        Menu_Text.TabIndex = 51
        Menu_Text.Text = "Connect"
        ' 
        ' Menu_Top_Dim
        ' 
        Menu_Top_Dim.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Menu_Top_Dim.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Menu_Top_Dim.Location = New Point(80, 160)
        Menu_Top_Dim.Name = "Menu_Top_Dim"
        Menu_Top_Dim.Size = New Size(1760, 5)
        Menu_Top_Dim.TabIndex = 0
        Menu_Top_Dim.TabStop = False
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
        ' Dim_Top
        ' 
        Dim_Top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Dim_Top.Location = New Point(-24, -16)
        Dim_Top.Name = "Dim_Top"
        Dim_Top.Size = New Size(1913, 176)
        Dim_Top.TabIndex = 46
        Dim_Top.TabStop = False
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
        ' Base_Empty
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1080)
        Controls.Add(BT_Back)
        Controls.Add(Dim_2)
        Controls.Add(Dim_1)
        Controls.Add(Menu_Top_Dim)
        Controls.Add(Menu_Settings)
        Controls.Add(Dim_Top)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_Empty"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        Text = "Overlay"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        Menu_Settings.ResumeLayout(False)
        Menu_Settings.PerformLayout()
        CType(Menu_Top_Dim, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Menu_Settings As Panel
    Friend WithEvents BT_Back As Label
    Friend WithEvents Menu_Text As Label
    Friend WithEvents Menu_Top_Dim As PictureBox
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
End Class
