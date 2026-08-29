<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_Notifications
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_Notifications))
        Menu_Settings = New Panel()
        Menu_Text = New Label()
        Menu_Top_Dim = New PictureBox()
        BT_Back = New Label()
        Dim_Top = New PictureBox()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Desc_Recording = New Label()
        Desc_Recording_SUB = New Label()
        ToggleRecording = New ToggleSwitch()
        Desc_InstantReplay = New Label()
        Desc_InstantReplay_SUB = New Label()
        ToggleInstantReplay = New ToggleSwitch()
        Desc_Screenshots = New Label()
        Desc_Screenshots_SUB = New Label()
        ToggleScreenshots = New ToggleSwitch()
        Desc_ShareOverlay = New Label()
        Desc_ShareOverlay_SUB = New Label()
        ToggleShareOverlay = New ToggleSwitch()
        Desc_SystemMonitor = New Label()
        Desc_SystemMonitor_SUB = New Label()
        ToggleSystemMonitor = New ToggleSwitch()
        Desc_Updates = New Label()
        Desc_Updates_SUB = New Label()
        ToggleUpdates = New ToggleSwitch()
        Desc_Errors = New Label()
        Desc_Errors_SUB = New Label()
        ToggleErrors = New ToggleSwitch()
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
        Menu_Settings.Controls.Add(Desc_Recording)
        Menu_Settings.Controls.Add(Desc_Recording_SUB)
        Menu_Settings.Controls.Add(ToggleRecording)
        Menu_Settings.Controls.Add(Desc_InstantReplay)
        Menu_Settings.Controls.Add(Desc_InstantReplay_SUB)
        Menu_Settings.Controls.Add(ToggleInstantReplay)
        Menu_Settings.Controls.Add(Desc_Screenshots)
        Menu_Settings.Controls.Add(Desc_Screenshots_SUB)
        Menu_Settings.Controls.Add(ToggleScreenshots)
        Menu_Settings.Controls.Add(Desc_ShareOverlay)
        Menu_Settings.Controls.Add(Desc_ShareOverlay_SUB)
        Menu_Settings.Controls.Add(ToggleShareOverlay)
        Menu_Settings.Controls.Add(Desc_SystemMonitor)
        Menu_Settings.Controls.Add(Desc_SystemMonitor_SUB)
        Menu_Settings.Controls.Add(ToggleSystemMonitor)
        Menu_Settings.Controls.Add(Desc_Updates)
        Menu_Settings.Controls.Add(Desc_Updates_SUB)
        Menu_Settings.Controls.Add(ToggleUpdates)
        Menu_Settings.Controls.Add(Desc_Errors)
        Menu_Settings.Controls.Add(Desc_Errors_SUB)
        Menu_Settings.Controls.Add(ToggleErrors)
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
        Menu_Text.Size = New Size(193, 42)
        Menu_Text.TabIndex = 51
        Menu_Text.Text = "Notifications"
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
        ' Desc_Recording
        ' 
        Desc_Recording.AutoSize = True
        Desc_Recording.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Recording.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_Recording.ForeColor = Color.White
        Desc_Recording.Location = New Point(125, 56)
        Desc_Recording.Name = "Desc_Recording"
        Desc_Recording.Size = New Size(136, 28)
        Desc_Recording.TabIndex = 60
        Desc_Recording.Text = "Recording"
        ' 
        ' Desc_Recording_SUB
        ' 
        Desc_Recording_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_Recording_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Recording_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_Recording_SUB.ForeColor = Color.White
        Desc_Recording_SUB.Location = New Point(151, 76)
        Desc_Recording_SUB.Name = "Desc_Recording_SUB"
        Desc_Recording_SUB.Size = New Size(1400, 44)
        Desc_Recording_SUB.TabIndex = 61
        Desc_Recording_SUB.Text = "Show notifications when recording starts, is saved, or fails."
        Desc_Recording_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleRecording
        ' 
        ToggleRecording.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRecording.ImeMode = ImeMode.Off
        ToggleRecording.IsOn = False
        ToggleRecording.Location = New Point(71, 60)
        ToggleRecording.Name = "ToggleRecording"
        ToggleRecording.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecording.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecording.ShowGlow = False
        ToggleRecording.Size = New Size(48, 24)
        ToggleRecording.TabIndex = 62
        ToggleRecording.Text = "ToggleSwitch"
        ' 
        ' Desc_InstantReplay
        ' 
        Desc_InstantReplay.AutoSize = True
        Desc_InstantReplay.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_InstantReplay.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_InstantReplay.ForeColor = Color.White
        Desc_InstantReplay.Location = New Point(125, 168)
        Desc_InstantReplay.Name = "Desc_InstantReplay"
        Desc_InstantReplay.Size = New Size(172, 28)
        Desc_InstantReplay.TabIndex = 63
        Desc_InstantReplay.Text = "Instant Replay"
        ' 
        ' Desc_InstantReplay_SUB
        ' 
        Desc_InstantReplay_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_InstantReplay_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_InstantReplay_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_InstantReplay_SUB.ForeColor = Color.White
        Desc_InstantReplay_SUB.Location = New Point(151, 188)
        Desc_InstantReplay_SUB.Name = "Desc_InstantReplay_SUB"
        Desc_InstantReplay_SUB.Size = New Size(1400, 44)
        Desc_InstantReplay_SUB.TabIndex = 64
        Desc_InstantReplay_SUB.Text = "Show notifications when Instant Replay turns on or off and when a replay is saved."
        Desc_InstantReplay_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleInstantReplay
        ' 
        ToggleInstantReplay.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleInstantReplay.ImeMode = ImeMode.Off
        ToggleInstantReplay.IsOn = False
        ToggleInstantReplay.Location = New Point(71, 172)
        ToggleInstantReplay.Name = "ToggleInstantReplay"
        ToggleInstantReplay.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleInstantReplay.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleInstantReplay.ShowGlow = False
        ToggleInstantReplay.Size = New Size(48, 24)
        ToggleInstantReplay.TabIndex = 65
        ToggleInstantReplay.Text = "ToggleSwitch"
        ' 
        ' Desc_Screenshots
        ' 
        Desc_Screenshots.AutoSize = True
        Desc_Screenshots.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Screenshots.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_Screenshots.ForeColor = Color.White
        Desc_Screenshots.Location = New Point(125, 280)
        Desc_Screenshots.Name = "Desc_Screenshots"
        Desc_Screenshots.Size = New Size(152, 28)
        Desc_Screenshots.TabIndex = 66
        Desc_Screenshots.Text = "Screenshots"
        ' 
        ' Desc_Screenshots_SUB
        ' 
        Desc_Screenshots_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_Screenshots_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Screenshots_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_Screenshots_SUB.ForeColor = Color.White
        Desc_Screenshots_SUB.Location = New Point(151, 300)
        Desc_Screenshots_SUB.Name = "Desc_Screenshots_SUB"
        Desc_Screenshots_SUB.Size = New Size(1400, 44)
        Desc_Screenshots_SUB.TabIndex = 67
        Desc_Screenshots_SUB.Text = "Show notifications when a screenshot is saved to the Gallery."
        Desc_Screenshots_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleScreenshots
        ' 
        ToggleScreenshots.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleScreenshots.ImeMode = ImeMode.Off
        ToggleScreenshots.IsOn = False
        ToggleScreenshots.Location = New Point(71, 284)
        ToggleScreenshots.Name = "ToggleScreenshots"
        ToggleScreenshots.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleScreenshots.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleScreenshots.ShowGlow = False
        ToggleScreenshots.Size = New Size(48, 24)
        ToggleScreenshots.TabIndex = 68
        ToggleScreenshots.Text = "ToggleSwitch"
        ' 
        ' Desc_ShareOverlay
        ' 
        Desc_ShareOverlay.AutoSize = True
        Desc_ShareOverlay.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ShareOverlay.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_ShareOverlay.ForeColor = Color.White
        Desc_ShareOverlay.Location = New Point(125, 392)
        Desc_ShareOverlay.Name = "Desc_ShareOverlay"
        Desc_ShareOverlay.Size = New Size(166, 28)
        Desc_ShareOverlay.TabIndex = 69
        Desc_ShareOverlay.Text = "Share overlay"
        ' 
        ' Desc_ShareOverlay_SUB
        ' 
        Desc_ShareOverlay_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_ShareOverlay_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ShareOverlay_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ShareOverlay_SUB.ForeColor = Color.White
        Desc_ShareOverlay_SUB.Location = New Point(151, 412)
        Desc_ShareOverlay_SUB.Name = "Desc_ShareOverlay_SUB"
        Desc_ShareOverlay_SUB.Size = New Size(1400, 44)
        Desc_ShareOverlay_SUB.TabIndex = 70
        Desc_ShareOverlay_SUB.Text = "Show the hint notification when the Share overlay opens."
        Desc_ShareOverlay_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleShareOverlay
        ' 
        ToggleShareOverlay.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleShareOverlay.ImeMode = ImeMode.Off
        ToggleShareOverlay.IsOn = False
        ToggleShareOverlay.Location = New Point(71, 396)
        ToggleShareOverlay.Name = "ToggleShareOverlay"
        ToggleShareOverlay.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleShareOverlay.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleShareOverlay.ShowGlow = False
        ToggleShareOverlay.Size = New Size(48, 24)
        ToggleShareOverlay.TabIndex = 71
        ToggleShareOverlay.Text = "ToggleSwitch"
        ' 
        ' Desc_SystemMonitor
        ' 
        Desc_SystemMonitor.AutoSize = True
        Desc_SystemMonitor.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_SystemMonitor.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_SystemMonitor.ForeColor = Color.White
        Desc_SystemMonitor.Location = New Point(125, 504)
        Desc_SystemMonitor.Name = "Desc_SystemMonitor"
        Desc_SystemMonitor.Size = New Size(187, 28)
        Desc_SystemMonitor.TabIndex = 72
        Desc_SystemMonitor.Text = "System monitor"
        ' 
        ' Desc_SystemMonitor_SUB
        ' 
        Desc_SystemMonitor_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_SystemMonitor_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_SystemMonitor_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_SystemMonitor_SUB.ForeColor = Color.White
        Desc_SystemMonitor_SUB.Location = New Point(151, 524)
        Desc_SystemMonitor_SUB.Name = "Desc_SystemMonitor_SUB"
        Desc_SystemMonitor_SUB.Size = New Size(1400, 44)
        Desc_SystemMonitor_SUB.TabIndex = 73
        Desc_SystemMonitor_SUB.Text = "Show CPU, memory and disk space warnings."
        Desc_SystemMonitor_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleSystemMonitor
        ' 
        ToggleSystemMonitor.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleSystemMonitor.ImeMode = ImeMode.Off
        ToggleSystemMonitor.IsOn = False
        ToggleSystemMonitor.Location = New Point(71, 508)
        ToggleSystemMonitor.Name = "ToggleSystemMonitor"
        ToggleSystemMonitor.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleSystemMonitor.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleSystemMonitor.ShowGlow = False
        ToggleSystemMonitor.Size = New Size(48, 24)
        ToggleSystemMonitor.TabIndex = 74
        ToggleSystemMonitor.Text = "ToggleSwitch"
        ' 
        ' Desc_Updates
        ' 
        Desc_Updates.AutoSize = True
        Desc_Updates.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Updates.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_Updates.ForeColor = Color.White
        Desc_Updates.Location = New Point(125, 616)
        Desc_Updates.Name = "Desc_Updates"
        Desc_Updates.Size = New Size(110, 28)
        Desc_Updates.TabIndex = 75
        Desc_Updates.Text = "Updates"
        ' 
        ' Desc_Updates_SUB
        ' 
        Desc_Updates_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_Updates_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Updates_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_Updates_SUB.ForeColor = Color.White
        Desc_Updates_SUB.Location = New Point(151, 636)
        Desc_Updates_SUB.Name = "Desc_Updates_SUB"
        Desc_Updates_SUB.Size = New Size(1400, 44)
        Desc_Updates_SUB.TabIndex = 76
        Desc_Updates_SUB.Text = "Show notifications when a new version is available."
        Desc_Updates_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleUpdates
        ' 
        ToggleUpdates.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleUpdates.ImeMode = ImeMode.Off
        ToggleUpdates.IsOn = False
        ToggleUpdates.Location = New Point(71, 620)
        ToggleUpdates.Name = "ToggleUpdates"
        ToggleUpdates.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleUpdates.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleUpdates.ShowGlow = False
        ToggleUpdates.Size = New Size(48, 24)
        ToggleUpdates.TabIndex = 77
        ToggleUpdates.Text = "ToggleSwitch"
        ' 
        ' Desc_Errors
        ' 
        Desc_Errors.AutoSize = True
        Desc_Errors.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Errors.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Desc_Errors.ForeColor = Color.White
        Desc_Errors.Location = New Point(125, 728)
        Desc_Errors.Name = "Desc_Errors"
        Desc_Errors.Size = New Size(206, 28)
        Desc_Errors.TabIndex = 78
        Desc_Errors.Text = "Errors & feedback"
        ' 
        ' Desc_Errors_SUB
        ' 
        Desc_Errors_SUB.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Desc_Errors_SUB.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_Errors_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_Errors_SUB.ForeColor = Color.White
        Desc_Errors_SUB.Location = New Point(151, 748)
        Desc_Errors_SUB.Name = "Desc_Errors_SUB"
        Desc_Errors_SUB.Size = New Size(1400, 44)
        Desc_Errors_SUB.TabIndex = 79
        Desc_Errors_SUB.Text = "Show errors and status feedback when an action can't be completed."
        Desc_Errors_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ToggleErrors
        ' 
        ToggleErrors.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleErrors.ImeMode = ImeMode.Off
        ToggleErrors.IsOn = False
        ToggleErrors.Location = New Point(71, 732)
        ToggleErrors.Name = "ToggleErrors"
        ToggleErrors.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleErrors.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleErrors.ShowGlow = False
        ToggleErrors.Size = New Size(48, 24)
        ToggleErrors.TabIndex = 80
        ToggleErrors.Text = "ToggleSwitch"
        ' 
        ' Base_Notifications
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
        Name = "Base_Notifications"
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
    Friend WithEvents Desc_Recording As Label
    Friend WithEvents Desc_Recording_SUB As Label
    Friend WithEvents ToggleRecording As ToggleSwitch
    Friend WithEvents Desc_InstantReplay As Label
    Friend WithEvents Desc_InstantReplay_SUB As Label
    Friend WithEvents ToggleInstantReplay As ToggleSwitch
    Friend WithEvents Desc_Screenshots As Label
    Friend WithEvents Desc_Screenshots_SUB As Label
    Friend WithEvents ToggleScreenshots As ToggleSwitch
    Friend WithEvents Desc_ShareOverlay As Label
    Friend WithEvents Desc_ShareOverlay_SUB As Label
    Friend WithEvents ToggleShareOverlay As ToggleSwitch
    Friend WithEvents Desc_SystemMonitor As Label
    Friend WithEvents Desc_SystemMonitor_SUB As Label
    Friend WithEvents ToggleSystemMonitor As ToggleSwitch
    Friend WithEvents Desc_Updates As Label
    Friend WithEvents Desc_Updates_SUB As Label
    Friend WithEvents ToggleUpdates As ToggleSwitch
    Friend WithEvents Desc_Errors As Label
    Friend WithEvents Desc_Errors_SUB As Label
    Friend WithEvents ToggleErrors As ToggleSwitch
End Class
