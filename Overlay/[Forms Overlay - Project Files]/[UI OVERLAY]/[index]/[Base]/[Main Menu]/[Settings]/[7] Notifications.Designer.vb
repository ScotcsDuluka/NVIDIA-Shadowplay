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
        Header_Recording = New Label()
        Desc_RecordingStarted = New Label()
        ToggleRecordingStarted = New ToggleSwitch()
        Desc_RecordingSaved = New Label()
        ToggleRecordingSaved = New ToggleSwitch()
        Desc_RecordingError = New Label()
        ToggleRecordingError = New ToggleSwitch()
        Header_InstantReplay = New Label()
        Desc_ReplaySaved = New Label()
        ToggleReplaySaved = New ToggleSwitch()
        Desc_InstantReplayOn = New Label()
        ToggleInstantReplayOn = New ToggleSwitch()
        Desc_InstantReplayOff = New Label()
        ToggleInstantReplayOff = New ToggleSwitch()
        Desc_ReplayTurnOn = New Label()
        ToggleReplayTurnOn = New ToggleSwitch()
        Desc_ReplayError = New Label()
        ToggleReplayError = New ToggleSwitch()
        Header_Screenshots = New Label()
        Desc_ScreenshotSaved = New Label()
        ToggleScreenshotSaved = New ToggleSwitch()
        Desc_ValidSavePath = New Label()
        ToggleValidSavePath = New ToggleSwitch()
        Header_ShareOverlay = New Label()
        Desc_OpenShare = New Label()
        ToggleOpenShare = New ToggleSwitch()
        Header_SystemMonitor = New Label()
        Desc_RamWarning = New Label()
        ToggleRamWarning = New ToggleSwitch()
        Desc_RamWarning95 = New Label()
        ToggleRamWarning95 = New ToggleSwitch()
        Desc_RamCritical = New Label()
        ToggleRamCritical = New ToggleSwitch()
        Desc_CpuWarning = New Label()
        ToggleCpuWarning = New ToggleSwitch()
        Desc_DiskSpaceLow = New Label()
        ToggleDiskSpaceLow = New ToggleSwitch()
        Header_Updates = New Label()
        Desc_UpdateAvailable = New Label()
        ToggleUpdateAvailable = New ToggleSwitch()
        Desc_VersionLatest = New Label()
        ToggleVersionLatest = New ToggleSwitch()
        Desc_UpdateError = New Label()
        ToggleUpdateError = New ToggleSwitch()
        Header_Errors = New Label()
        Desc_AccountConfirmError = New Label()
        ToggleAccountConfirmError = New ToggleSwitch()
        Desc_ExtensionNotFound = New Label()
        ToggleExtensionNotFound = New ToggleSwitch()
        Desc_FeatureNotReady = New Label()
        ToggleFeatureNotReady = New ToggleSwitch()
        Desc_GpuRequired = New Label()
        ToggleGpuRequired = New ToggleSwitch()
        Desc_EngineNotRunning = New Label()
        ToggleEngineNotRunning = New ToggleSwitch()
        Desc_EngineUIInUse = New Label()
        ToggleEngineUIInUse = New ToggleSwitch()
        Desc_ErrorResolution = New Label()
        ToggleErrorResolution = New ToggleSwitch()
        Desc_DesktopCaptureDisabled = New Label()
        ToggleDesktopCaptureDisabled = New ToggleSwitch()
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
        Menu_Settings.Controls.Add(Header_Recording)
        Menu_Settings.Controls.Add(Desc_RecordingStarted)
        Menu_Settings.Controls.Add(ToggleRecordingStarted)
        Menu_Settings.Controls.Add(Desc_RecordingSaved)
        Menu_Settings.Controls.Add(ToggleRecordingSaved)
        Menu_Settings.Controls.Add(Desc_RecordingError)
        Menu_Settings.Controls.Add(ToggleRecordingError)
        Menu_Settings.Controls.Add(Header_InstantReplay)
        Menu_Settings.Controls.Add(Desc_ReplaySaved)
        Menu_Settings.Controls.Add(ToggleReplaySaved)
        Menu_Settings.Controls.Add(Desc_InstantReplayOn)
        Menu_Settings.Controls.Add(ToggleInstantReplayOn)
        Menu_Settings.Controls.Add(Desc_InstantReplayOff)
        Menu_Settings.Controls.Add(ToggleInstantReplayOff)
        Menu_Settings.Controls.Add(Desc_ReplayTurnOn)
        Menu_Settings.Controls.Add(ToggleReplayTurnOn)
        Menu_Settings.Controls.Add(Desc_ReplayError)
        Menu_Settings.Controls.Add(ToggleReplayError)
        Menu_Settings.Controls.Add(Header_Screenshots)
        Menu_Settings.Controls.Add(Desc_ScreenshotSaved)
        Menu_Settings.Controls.Add(ToggleScreenshotSaved)
        Menu_Settings.Controls.Add(Desc_ValidSavePath)
        Menu_Settings.Controls.Add(ToggleValidSavePath)
        Menu_Settings.Controls.Add(Header_ShareOverlay)
        Menu_Settings.Controls.Add(Desc_OpenShare)
        Menu_Settings.Controls.Add(ToggleOpenShare)
        Menu_Settings.Controls.Add(Header_SystemMonitor)
        Menu_Settings.Controls.Add(Desc_RamWarning)
        Menu_Settings.Controls.Add(ToggleRamWarning)
        Menu_Settings.Controls.Add(Desc_RamWarning95)
        Menu_Settings.Controls.Add(ToggleRamWarning95)
        Menu_Settings.Controls.Add(Desc_RamCritical)
        Menu_Settings.Controls.Add(ToggleRamCritical)
        Menu_Settings.Controls.Add(Desc_CpuWarning)
        Menu_Settings.Controls.Add(ToggleCpuWarning)
        Menu_Settings.Controls.Add(Desc_DiskSpaceLow)
        Menu_Settings.Controls.Add(ToggleDiskSpaceLow)
        Menu_Settings.Controls.Add(Header_Updates)
        Menu_Settings.Controls.Add(Desc_UpdateAvailable)
        Menu_Settings.Controls.Add(ToggleUpdateAvailable)
        Menu_Settings.Controls.Add(Desc_VersionLatest)
        Menu_Settings.Controls.Add(ToggleVersionLatest)
        Menu_Settings.Controls.Add(Desc_UpdateError)
        Menu_Settings.Controls.Add(ToggleUpdateError)
        Menu_Settings.Controls.Add(Header_Errors)
        Menu_Settings.Controls.Add(Desc_AccountConfirmError)
        Menu_Settings.Controls.Add(ToggleAccountConfirmError)
        Menu_Settings.Controls.Add(Desc_ExtensionNotFound)
        Menu_Settings.Controls.Add(ToggleExtensionNotFound)
        Menu_Settings.Controls.Add(Desc_FeatureNotReady)
        Menu_Settings.Controls.Add(ToggleFeatureNotReady)
        Menu_Settings.Controls.Add(Desc_GpuRequired)
        Menu_Settings.Controls.Add(ToggleGpuRequired)
        Menu_Settings.Controls.Add(Desc_EngineNotRunning)
        Menu_Settings.Controls.Add(ToggleEngineNotRunning)
        Menu_Settings.Controls.Add(Desc_EngineUIInUse)
        Menu_Settings.Controls.Add(ToggleEngineUIInUse)
        Menu_Settings.Controls.Add(Desc_ErrorResolution)
        Menu_Settings.Controls.Add(ToggleErrorResolution)
        Menu_Settings.Controls.Add(Desc_DesktopCaptureDisabled)
        Menu_Settings.Controls.Add(ToggleDesktopCaptureDisabled)
        Menu_Settings.Location = New Point(80, 160)
        Menu_Settings.Name = "Menu_Settings"
        Menu_Settings.Size = New Size(1760, 840)
        Menu_Settings.TabIndex = 45
        Menu_Settings.AutoScroll = True
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
        ' Header_Recording
        ' 
        Header_Recording.AutoSize = True
        Header_Recording.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_Recording.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Recording.ForeColor = Color.White
        Header_Recording.Location = New Point(71, 40)
        Header_Recording.Name = "Header_Recording"
        Header_Recording.Size = New Size(260, 28)
        Header_Recording.TabIndex = 60
        Header_Recording.Text = "RECORDING"
        ' 
        ' Desc_RecordingStarted
        ' 
        Desc_RecordingStarted.AutoSize = True
        Desc_RecordingStarted.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_RecordingStarted.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RecordingStarted.ForeColor = Color.White
        Desc_RecordingStarted.Location = New Point(125, 88)
        Desc_RecordingStarted.Name = "Desc_RecordingStarted"
        Desc_RecordingStarted.Size = New Size(300, 27)
        Desc_RecordingStarted.TabIndex = 61
        Desc_RecordingStarted.Text = "Recording started"
        ' 
        ' ToggleRecordingStarted
        ' 
        ToggleRecordingStarted.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRecordingStarted.ImeMode = ImeMode.Off
        ToggleRecordingStarted.IsOn = False
        ToggleRecordingStarted.Location = New Point(71, 89)
        ToggleRecordingStarted.Name = "ToggleRecordingStarted"
        ToggleRecordingStarted.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecordingStarted.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecordingStarted.ShowGlow = False
        ToggleRecordingStarted.Size = New Size(48, 24)
        ToggleRecordingStarted.TabIndex = 62
        ToggleRecordingStarted.Text = "ToggleSwitch"
        ' 
        ' Desc_RecordingSaved
        ' 
        Desc_RecordingSaved.AutoSize = True
        Desc_RecordingSaved.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_RecordingSaved.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RecordingSaved.ForeColor = Color.White
        Desc_RecordingSaved.Location = New Point(125, 132)
        Desc_RecordingSaved.Name = "Desc_RecordingSaved"
        Desc_RecordingSaved.Size = New Size(300, 27)
        Desc_RecordingSaved.TabIndex = 63
        Desc_RecordingSaved.Text = "Recording saved"
        ' 
        ' ToggleRecordingSaved
        ' 
        ToggleRecordingSaved.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRecordingSaved.ImeMode = ImeMode.Off
        ToggleRecordingSaved.IsOn = False
        ToggleRecordingSaved.Location = New Point(71, 133)
        ToggleRecordingSaved.Name = "ToggleRecordingSaved"
        ToggleRecordingSaved.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecordingSaved.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecordingSaved.ShowGlow = False
        ToggleRecordingSaved.Size = New Size(48, 24)
        ToggleRecordingSaved.TabIndex = 64
        ToggleRecordingSaved.Text = "ToggleSwitch"
        ' 
        ' Desc_RecordingError
        ' 
        Desc_RecordingError.AutoSize = True
        Desc_RecordingError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_RecordingError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RecordingError.ForeColor = Color.White
        Desc_RecordingError.Location = New Point(125, 176)
        Desc_RecordingError.Name = "Desc_RecordingError"
        Desc_RecordingError.Size = New Size(300, 27)
        Desc_RecordingError.TabIndex = 65
        Desc_RecordingError.Text = "Recording error"
        ' 
        ' ToggleRecordingError
        ' 
        ToggleRecordingError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRecordingError.ImeMode = ImeMode.Off
        ToggleRecordingError.IsOn = False
        ToggleRecordingError.Location = New Point(71, 177)
        ToggleRecordingError.Name = "ToggleRecordingError"
        ToggleRecordingError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecordingError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecordingError.ShowGlow = False
        ToggleRecordingError.Size = New Size(48, 24)
        ToggleRecordingError.TabIndex = 66
        ToggleRecordingError.Text = "ToggleSwitch"
        ' 
        ' 
        ' Header_InstantReplay
        ' 
        Header_InstantReplay.AutoSize = True
        Header_InstantReplay.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_InstantReplay.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_InstantReplay.ForeColor = Color.White
        Header_InstantReplay.Location = New Point(71, 236)
        Header_InstantReplay.Name = "Header_InstantReplay"
        Header_InstantReplay.Size = New Size(260, 28)
        Header_InstantReplay.TabIndex = 67
        Header_InstantReplay.Text = "INSTANT REPLAY"
        ' 
        ' Desc_ReplaySaved
        ' 
        Desc_ReplaySaved.AutoSize = True
        Desc_ReplaySaved.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ReplaySaved.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ReplaySaved.ForeColor = Color.White
        Desc_ReplaySaved.Location = New Point(125, 284)
        Desc_ReplaySaved.Name = "Desc_ReplaySaved"
        Desc_ReplaySaved.Size = New Size(300, 27)
        Desc_ReplaySaved.TabIndex = 68
        Desc_ReplaySaved.Text = "Instant Replay saved"
        ' 
        ' ToggleReplaySaved
        ' 
        ToggleReplaySaved.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleReplaySaved.ImeMode = ImeMode.Off
        ToggleReplaySaved.IsOn = False
        ToggleReplaySaved.Location = New Point(71, 285)
        ToggleReplaySaved.Name = "ToggleReplaySaved"
        ToggleReplaySaved.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleReplaySaved.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleReplaySaved.ShowGlow = False
        ToggleReplaySaved.Size = New Size(48, 24)
        ToggleReplaySaved.TabIndex = 69
        ToggleReplaySaved.Text = "ToggleSwitch"
        ' 
        ' Desc_InstantReplayOn
        ' 
        Desc_InstantReplayOn.AutoSize = True
        Desc_InstantReplayOn.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_InstantReplayOn.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_InstantReplayOn.ForeColor = Color.White
        Desc_InstantReplayOn.Location = New Point(125, 328)
        Desc_InstantReplayOn.Name = "Desc_InstantReplayOn"
        Desc_InstantReplayOn.Size = New Size(300, 27)
        Desc_InstantReplayOn.TabIndex = 70
        Desc_InstantReplayOn.Text = "Instant Replay on"
        ' 
        ' ToggleInstantReplayOn
        ' 
        ToggleInstantReplayOn.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleInstantReplayOn.ImeMode = ImeMode.Off
        ToggleInstantReplayOn.IsOn = False
        ToggleInstantReplayOn.Location = New Point(71, 329)
        ToggleInstantReplayOn.Name = "ToggleInstantReplayOn"
        ToggleInstantReplayOn.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleInstantReplayOn.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleInstantReplayOn.ShowGlow = False
        ToggleInstantReplayOn.Size = New Size(48, 24)
        ToggleInstantReplayOn.TabIndex = 71
        ToggleInstantReplayOn.Text = "ToggleSwitch"
        ' 
        ' Desc_InstantReplayOff
        ' 
        Desc_InstantReplayOff.AutoSize = True
        Desc_InstantReplayOff.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_InstantReplayOff.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_InstantReplayOff.ForeColor = Color.White
        Desc_InstantReplayOff.Location = New Point(125, 372)
        Desc_InstantReplayOff.Name = "Desc_InstantReplayOff"
        Desc_InstantReplayOff.Size = New Size(300, 27)
        Desc_InstantReplayOff.TabIndex = 72
        Desc_InstantReplayOff.Text = "Instant Replay off"
        ' 
        ' ToggleInstantReplayOff
        ' 
        ToggleInstantReplayOff.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleInstantReplayOff.ImeMode = ImeMode.Off
        ToggleInstantReplayOff.IsOn = False
        ToggleInstantReplayOff.Location = New Point(71, 373)
        ToggleInstantReplayOff.Name = "ToggleInstantReplayOff"
        ToggleInstantReplayOff.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleInstantReplayOff.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleInstantReplayOff.ShowGlow = False
        ToggleInstantReplayOff.Size = New Size(48, 24)
        ToggleInstantReplayOff.TabIndex = 73
        ToggleInstantReplayOff.Text = "ToggleSwitch"
        ' 
        ' Desc_ReplayTurnOn
        ' 
        Desc_ReplayTurnOn.AutoSize = True
        Desc_ReplayTurnOn.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ReplayTurnOn.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ReplayTurnOn.ForeColor = Color.White
        Desc_ReplayTurnOn.Location = New Point(125, 416)
        Desc_ReplayTurnOn.Name = "Desc_ReplayTurnOn"
        Desc_ReplayTurnOn.Size = New Size(300, 27)
        Desc_ReplayTurnOn.TabIndex = 74
        Desc_ReplayTurnOn.Text = "Turning on Instant Replay"
        ' 
        ' ToggleReplayTurnOn
        ' 
        ToggleReplayTurnOn.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleReplayTurnOn.ImeMode = ImeMode.Off
        ToggleReplayTurnOn.IsOn = False
        ToggleReplayTurnOn.Location = New Point(71, 417)
        ToggleReplayTurnOn.Name = "ToggleReplayTurnOn"
        ToggleReplayTurnOn.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleReplayTurnOn.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleReplayTurnOn.ShowGlow = False
        ToggleReplayTurnOn.Size = New Size(48, 24)
        ToggleReplayTurnOn.TabIndex = 75
        ToggleReplayTurnOn.Text = "ToggleSwitch"
        ' 
        ' Desc_ReplayError
        ' 
        Desc_ReplayError.AutoSize = True
        Desc_ReplayError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ReplayError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ReplayError.ForeColor = Color.White
        Desc_ReplayError.Location = New Point(125, 460)
        Desc_ReplayError.Name = "Desc_ReplayError"
        Desc_ReplayError.Size = New Size(300, 27)
        Desc_ReplayError.TabIndex = 76
        Desc_ReplayError.Text = "Instant Replay error"
        ' 
        ' ToggleReplayError
        ' 
        ToggleReplayError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleReplayError.ImeMode = ImeMode.Off
        ToggleReplayError.IsOn = False
        ToggleReplayError.Location = New Point(71, 461)
        ToggleReplayError.Name = "ToggleReplayError"
        ToggleReplayError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleReplayError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleReplayError.ShowGlow = False
        ToggleReplayError.Size = New Size(48, 24)
        ToggleReplayError.TabIndex = 77
        ToggleReplayError.Text = "ToggleSwitch"
        ' 
        ' 
        ' Header_Screenshots
        ' 
        Header_Screenshots.AutoSize = True
        Header_Screenshots.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_Screenshots.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Screenshots.ForeColor = Color.White
        Header_Screenshots.Location = New Point(71, 520)
        Header_Screenshots.Name = "Header_Screenshots"
        Header_Screenshots.Size = New Size(260, 28)
        Header_Screenshots.TabIndex = 78
        Header_Screenshots.Text = "SCREENSHOTS"
        ' 
        ' Desc_ScreenshotSaved
        ' 
        Desc_ScreenshotSaved.AutoSize = True
        Desc_ScreenshotSaved.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ScreenshotSaved.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ScreenshotSaved.ForeColor = Color.White
        Desc_ScreenshotSaved.Location = New Point(125, 568)
        Desc_ScreenshotSaved.Name = "Desc_ScreenshotSaved"
        Desc_ScreenshotSaved.Size = New Size(300, 27)
        Desc_ScreenshotSaved.TabIndex = 79
        Desc_ScreenshotSaved.Text = "Screenshot saved to Gallery"
        ' 
        ' ToggleScreenshotSaved
        ' 
        ToggleScreenshotSaved.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleScreenshotSaved.ImeMode = ImeMode.Off
        ToggleScreenshotSaved.IsOn = False
        ToggleScreenshotSaved.Location = New Point(71, 569)
        ToggleScreenshotSaved.Name = "ToggleScreenshotSaved"
        ToggleScreenshotSaved.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleScreenshotSaved.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleScreenshotSaved.ShowGlow = False
        ToggleScreenshotSaved.Size = New Size(48, 24)
        ToggleScreenshotSaved.TabIndex = 80
        ToggleScreenshotSaved.Text = "ToggleSwitch"
        ' 
        ' Desc_ValidSavePath
        ' 
        Desc_ValidSavePath.AutoSize = True
        Desc_ValidSavePath.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ValidSavePath.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ValidSavePath.ForeColor = Color.White
        Desc_ValidSavePath.Location = New Point(125, 612)
        Desc_ValidSavePath.Name = "Desc_ValidSavePath"
        Desc_ValidSavePath.Size = New Size(300, 27)
        Desc_ValidSavePath.TabIndex = 81
        Desc_ValidSavePath.Text = "Invalid save path warning"
        ' 
        ' ToggleValidSavePath
        ' 
        ToggleValidSavePath.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleValidSavePath.ImeMode = ImeMode.Off
        ToggleValidSavePath.IsOn = False
        ToggleValidSavePath.Location = New Point(71, 613)
        ToggleValidSavePath.Name = "ToggleValidSavePath"
        ToggleValidSavePath.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleValidSavePath.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleValidSavePath.ShowGlow = False
        ToggleValidSavePath.Size = New Size(48, 24)
        ToggleValidSavePath.TabIndex = 82
        ToggleValidSavePath.Text = "ToggleSwitch"
        ' 
        ' 
        ' Header_ShareOverlay
        ' 
        Header_ShareOverlay.AutoSize = True
        Header_ShareOverlay.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_ShareOverlay.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_ShareOverlay.ForeColor = Color.White
        Header_ShareOverlay.Location = New Point(71, 672)
        Header_ShareOverlay.Name = "Header_ShareOverlay"
        Header_ShareOverlay.Size = New Size(260, 28)
        Header_ShareOverlay.TabIndex = 83
        Header_ShareOverlay.Text = "SHARE OVERLAY"
        ' 
        ' Desc_OpenShare
        ' 
        Desc_OpenShare.AutoSize = True
        Desc_OpenShare.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_OpenShare.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_OpenShare.ForeColor = Color.White
        Desc_OpenShare.Location = New Point(125, 720)
        Desc_OpenShare.Name = "Desc_OpenShare"
        Desc_OpenShare.Size = New Size(300, 27)
        Desc_OpenShare.TabIndex = 84
        Desc_OpenShare.Text = "Share overlay opened (Alt+Z)"
        ' 
        ' ToggleOpenShare
        ' 
        ToggleOpenShare.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleOpenShare.ImeMode = ImeMode.Off
        ToggleOpenShare.IsOn = False
        ToggleOpenShare.Location = New Point(71, 721)
        ToggleOpenShare.Name = "ToggleOpenShare"
        ToggleOpenShare.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleOpenShare.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleOpenShare.ShowGlow = False
        ToggleOpenShare.Size = New Size(48, 24)
        ToggleOpenShare.TabIndex = 85
        ToggleOpenShare.Text = "ToggleSwitch"
        ' 
        ' 
        ' Header_SystemMonitor
        ' 
        Header_SystemMonitor.AutoSize = True
        Header_SystemMonitor.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_SystemMonitor.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_SystemMonitor.ForeColor = Color.White
        Header_SystemMonitor.Location = New Point(71, 780)
        Header_SystemMonitor.Name = "Header_SystemMonitor"
        Header_SystemMonitor.Size = New Size(260, 28)
        Header_SystemMonitor.TabIndex = 86
        Header_SystemMonitor.Text = "SYSTEM MONITOR"
        ' 
        ' Desc_RamWarning
        ' 
        Desc_RamWarning.AutoSize = True
        Desc_RamWarning.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_RamWarning.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RamWarning.ForeColor = Color.White
        Desc_RamWarning.Location = New Point(125, 828)
        Desc_RamWarning.Name = "Desc_RamWarning"
        Desc_RamWarning.Size = New Size(300, 27)
        Desc_RamWarning.TabIndex = 87
        Desc_RamWarning.Text = "RAM usage warning"
        ' 
        ' ToggleRamWarning
        ' 
        ToggleRamWarning.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRamWarning.ImeMode = ImeMode.Off
        ToggleRamWarning.IsOn = False
        ToggleRamWarning.Location = New Point(71, 829)
        ToggleRamWarning.Name = "ToggleRamWarning"
        ToggleRamWarning.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRamWarning.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRamWarning.ShowGlow = False
        ToggleRamWarning.Size = New Size(48, 24)
        ToggleRamWarning.TabIndex = 88
        ToggleRamWarning.Text = "ToggleSwitch"
        ' 
        ' Desc_RamWarning95
        ' 
        Desc_RamWarning95.AutoSize = True
        Desc_RamWarning95.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_RamWarning95.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RamWarning95.ForeColor = Color.White
        Desc_RamWarning95.Location = New Point(125, 872)
        Desc_RamWarning95.Name = "Desc_RamWarning95"
        Desc_RamWarning95.Size = New Size(300, 27)
        Desc_RamWarning95.TabIndex = 89
        Desc_RamWarning95.Text = "RAM usage 95%"
        ' 
        ' ToggleRamWarning95
        ' 
        ToggleRamWarning95.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRamWarning95.ImeMode = ImeMode.Off
        ToggleRamWarning95.IsOn = False
        ToggleRamWarning95.Location = New Point(71, 873)
        ToggleRamWarning95.Name = "ToggleRamWarning95"
        ToggleRamWarning95.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRamWarning95.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRamWarning95.ShowGlow = False
        ToggleRamWarning95.Size = New Size(48, 24)
        ToggleRamWarning95.TabIndex = 90
        ToggleRamWarning95.Text = "ToggleSwitch"
        ' 
        ' Desc_RamCritical
        ' 
        Desc_RamCritical.AutoSize = True
        Desc_RamCritical.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_RamCritical.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RamCritical.ForeColor = Color.White
        Desc_RamCritical.Location = New Point(125, 916)
        Desc_RamCritical.Name = "Desc_RamCritical"
        Desc_RamCritical.Size = New Size(300, 27)
        Desc_RamCritical.TabIndex = 91
        Desc_RamCritical.Text = "RAM usage critical"
        ' 
        ' ToggleRamCritical
        ' 
        ToggleRamCritical.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleRamCritical.ImeMode = ImeMode.Off
        ToggleRamCritical.IsOn = False
        ToggleRamCritical.Location = New Point(71, 917)
        ToggleRamCritical.Name = "ToggleRamCritical"
        ToggleRamCritical.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRamCritical.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRamCritical.ShowGlow = False
        ToggleRamCritical.Size = New Size(48, 24)
        ToggleRamCritical.TabIndex = 92
        ToggleRamCritical.Text = "ToggleSwitch"
        ' 
        ' Desc_CpuWarning
        ' 
        Desc_CpuWarning.AutoSize = True
        Desc_CpuWarning.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_CpuWarning.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_CpuWarning.ForeColor = Color.White
        Desc_CpuWarning.Location = New Point(125, 960)
        Desc_CpuWarning.Name = "Desc_CpuWarning"
        Desc_CpuWarning.Size = New Size(300, 27)
        Desc_CpuWarning.TabIndex = 93
        Desc_CpuWarning.Text = "CPU usage warning"
        ' 
        ' ToggleCpuWarning
        ' 
        ToggleCpuWarning.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleCpuWarning.ImeMode = ImeMode.Off
        ToggleCpuWarning.IsOn = False
        ToggleCpuWarning.Location = New Point(71, 961)
        ToggleCpuWarning.Name = "ToggleCpuWarning"
        ToggleCpuWarning.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleCpuWarning.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleCpuWarning.ShowGlow = False
        ToggleCpuWarning.Size = New Size(48, 24)
        ToggleCpuWarning.TabIndex = 94
        ToggleCpuWarning.Text = "ToggleSwitch"
        ' 
        ' Desc_DiskSpaceLow
        ' 
        Desc_DiskSpaceLow.AutoSize = True
        Desc_DiskSpaceLow.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_DiskSpaceLow.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_DiskSpaceLow.ForeColor = Color.White
        Desc_DiskSpaceLow.Location = New Point(125, 1004)
        Desc_DiskSpaceLow.Name = "Desc_DiskSpaceLow"
        Desc_DiskSpaceLow.Size = New Size(300, 27)
        Desc_DiskSpaceLow.TabIndex = 95
        Desc_DiskSpaceLow.Text = "Disk space low"
        ' 
        ' ToggleDiskSpaceLow
        ' 
        ToggleDiskSpaceLow.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleDiskSpaceLow.ImeMode = ImeMode.Off
        ToggleDiskSpaceLow.IsOn = False
        ToggleDiskSpaceLow.Location = New Point(71, 1005)
        ToggleDiskSpaceLow.Name = "ToggleDiskSpaceLow"
        ToggleDiskSpaceLow.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleDiskSpaceLow.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleDiskSpaceLow.ShowGlow = False
        ToggleDiskSpaceLow.Size = New Size(48, 24)
        ToggleDiskSpaceLow.TabIndex = 96
        ToggleDiskSpaceLow.Text = "ToggleSwitch"
        ' 
        ' 
        ' Header_Updates
        ' 
        Header_Updates.AutoSize = True
        Header_Updates.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_Updates.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Updates.ForeColor = Color.White
        Header_Updates.Location = New Point(71, 1064)
        Header_Updates.Name = "Header_Updates"
        Header_Updates.Size = New Size(260, 28)
        Header_Updates.TabIndex = 97
        Header_Updates.Text = "UPDATES"
        ' 
        ' Desc_UpdateAvailable
        ' 
        Desc_UpdateAvailable.AutoSize = True
        Desc_UpdateAvailable.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_UpdateAvailable.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_UpdateAvailable.ForeColor = Color.White
        Desc_UpdateAvailable.Location = New Point(125, 1112)
        Desc_UpdateAvailable.Name = "Desc_UpdateAvailable"
        Desc_UpdateAvailable.Size = New Size(300, 27)
        Desc_UpdateAvailable.TabIndex = 98
        Desc_UpdateAvailable.Text = "Update available"
        ' 
        ' ToggleUpdateAvailable
        ' 
        ToggleUpdateAvailable.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleUpdateAvailable.ImeMode = ImeMode.Off
        ToggleUpdateAvailable.IsOn = False
        ToggleUpdateAvailable.Location = New Point(71, 1113)
        ToggleUpdateAvailable.Name = "ToggleUpdateAvailable"
        ToggleUpdateAvailable.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleUpdateAvailable.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleUpdateAvailable.ShowGlow = False
        ToggleUpdateAvailable.Size = New Size(48, 24)
        ToggleUpdateAvailable.TabIndex = 99
        ToggleUpdateAvailable.Text = "ToggleSwitch"
        ' 
        ' Desc_VersionLatest
        ' 
        Desc_VersionLatest.AutoSize = True
        Desc_VersionLatest.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_VersionLatest.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_VersionLatest.ForeColor = Color.White
        Desc_VersionLatest.Location = New Point(125, 1156)
        Desc_VersionLatest.Name = "Desc_VersionLatest"
        Desc_VersionLatest.Size = New Size(300, 27)
        Desc_VersionLatest.TabIndex = 100
        Desc_VersionLatest.Text = "Already on the latest version"
        ' 
        ' ToggleVersionLatest
        ' 
        ToggleVersionLatest.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleVersionLatest.ImeMode = ImeMode.Off
        ToggleVersionLatest.IsOn = False
        ToggleVersionLatest.Location = New Point(71, 1157)
        ToggleVersionLatest.Name = "ToggleVersionLatest"
        ToggleVersionLatest.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleVersionLatest.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleVersionLatest.ShowGlow = False
        ToggleVersionLatest.Size = New Size(48, 24)
        ToggleVersionLatest.TabIndex = 101
        ToggleVersionLatest.Text = "ToggleSwitch"
        ' 
        ' Desc_UpdateError
        ' 
        Desc_UpdateError.AutoSize = True
        Desc_UpdateError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_UpdateError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_UpdateError.ForeColor = Color.White
        Desc_UpdateError.Location = New Point(125, 1200)
        Desc_UpdateError.Name = "Desc_UpdateError"
        Desc_UpdateError.Size = New Size(300, 27)
        Desc_UpdateError.TabIndex = 102
        Desc_UpdateError.Text = "Update check error"
        ' 
        ' ToggleUpdateError
        ' 
        ToggleUpdateError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleUpdateError.ImeMode = ImeMode.Off
        ToggleUpdateError.IsOn = False
        ToggleUpdateError.Location = New Point(71, 1201)
        ToggleUpdateError.Name = "ToggleUpdateError"
        ToggleUpdateError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleUpdateError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleUpdateError.ShowGlow = False
        ToggleUpdateError.Size = New Size(48, 24)
        ToggleUpdateError.TabIndex = 103
        ToggleUpdateError.Text = "ToggleSwitch"
        ' 
        ' 
        ' Header_Errors
        ' 
        Header_Errors.AutoSize = True
        Header_Errors.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Header_Errors.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Errors.ForeColor = Color.White
        Header_Errors.Location = New Point(71, 1260)
        Header_Errors.Name = "Header_Errors"
        Header_Errors.Size = New Size(260, 28)
        Header_Errors.TabIndex = 104
        Header_Errors.Text = "ERRORS & FEEDBACK"
        ' 
        ' Desc_AccountConfirmError
        ' 
        Desc_AccountConfirmError.AutoSize = True
        Desc_AccountConfirmError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_AccountConfirmError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_AccountConfirmError.ForeColor = Color.White
        Desc_AccountConfirmError.Location = New Point(125, 1308)
        Desc_AccountConfirmError.Name = "Desc_AccountConfirmError"
        Desc_AccountConfirmError.Size = New Size(300, 27)
        Desc_AccountConfirmError.TabIndex = 105
        Desc_AccountConfirmError.Text = "Account confirmation error"
        ' 
        ' ToggleAccountConfirmError
        ' 
        ToggleAccountConfirmError.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleAccountConfirmError.ImeMode = ImeMode.Off
        ToggleAccountConfirmError.IsOn = False
        ToggleAccountConfirmError.Location = New Point(71, 1309)
        ToggleAccountConfirmError.Name = "ToggleAccountConfirmError"
        ToggleAccountConfirmError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleAccountConfirmError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleAccountConfirmError.ShowGlow = False
        ToggleAccountConfirmError.Size = New Size(48, 24)
        ToggleAccountConfirmError.TabIndex = 106
        ToggleAccountConfirmError.Text = "ToggleSwitch"
        ' 
        ' Desc_ExtensionNotFound
        ' 
        Desc_ExtensionNotFound.AutoSize = True
        Desc_ExtensionNotFound.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ExtensionNotFound.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ExtensionNotFound.ForeColor = Color.White
        Desc_ExtensionNotFound.Location = New Point(125, 1352)
        Desc_ExtensionNotFound.Name = "Desc_ExtensionNotFound"
        Desc_ExtensionNotFound.Size = New Size(300, 27)
        Desc_ExtensionNotFound.TabIndex = 107
        Desc_ExtensionNotFound.Text = "Browser extension not found"
        ' 
        ' ToggleExtensionNotFound
        ' 
        ToggleExtensionNotFound.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleExtensionNotFound.ImeMode = ImeMode.Off
        ToggleExtensionNotFound.IsOn = False
        ToggleExtensionNotFound.Location = New Point(71, 1353)
        ToggleExtensionNotFound.Name = "ToggleExtensionNotFound"
        ToggleExtensionNotFound.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleExtensionNotFound.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleExtensionNotFound.ShowGlow = False
        ToggleExtensionNotFound.Size = New Size(48, 24)
        ToggleExtensionNotFound.TabIndex = 108
        ToggleExtensionNotFound.Text = "ToggleSwitch"
        ' 
        ' Desc_FeatureNotReady
        ' 
        Desc_FeatureNotReady.AutoSize = True
        Desc_FeatureNotReady.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_FeatureNotReady.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_FeatureNotReady.ForeColor = Color.White
        Desc_FeatureNotReady.Location = New Point(125, 1396)
        Desc_FeatureNotReady.Name = "Desc_FeatureNotReady"
        Desc_FeatureNotReady.Size = New Size(300, 27)
        Desc_FeatureNotReady.TabIndex = 109
        Desc_FeatureNotReady.Text = "Feature not ready"
        ' 
        ' ToggleFeatureNotReady
        ' 
        ToggleFeatureNotReady.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleFeatureNotReady.ImeMode = ImeMode.Off
        ToggleFeatureNotReady.IsOn = False
        ToggleFeatureNotReady.Location = New Point(71, 1397)
        ToggleFeatureNotReady.Name = "ToggleFeatureNotReady"
        ToggleFeatureNotReady.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleFeatureNotReady.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleFeatureNotReady.ShowGlow = False
        ToggleFeatureNotReady.Size = New Size(48, 24)
        ToggleFeatureNotReady.TabIndex = 110
        ToggleFeatureNotReady.Text = "ToggleSwitch"
        ' 
        ' Desc_GpuRequired
        ' 
        Desc_GpuRequired.AutoSize = True
        Desc_GpuRequired.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_GpuRequired.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_GpuRequired.ForeColor = Color.White
        Desc_GpuRequired.Location = New Point(125, 1440)
        Desc_GpuRequired.Name = "Desc_GpuRequired"
        Desc_GpuRequired.Size = New Size(300, 27)
        Desc_GpuRequired.TabIndex = 111
        Desc_GpuRequired.Text = "NVIDIA GPU required"
        ' 
        ' ToggleGpuRequired
        ' 
        ToggleGpuRequired.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleGpuRequired.ImeMode = ImeMode.Off
        ToggleGpuRequired.IsOn = False
        ToggleGpuRequired.Location = New Point(71, 1441)
        ToggleGpuRequired.Name = "ToggleGpuRequired"
        ToggleGpuRequired.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleGpuRequired.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleGpuRequired.ShowGlow = False
        ToggleGpuRequired.Size = New Size(48, 24)
        ToggleGpuRequired.TabIndex = 112
        ToggleGpuRequired.Text = "ToggleSwitch"
        ' 
        ' Desc_EngineNotRunning
        ' 
        Desc_EngineNotRunning.AutoSize = True
        Desc_EngineNotRunning.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_EngineNotRunning.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_EngineNotRunning.ForeColor = Color.White
        Desc_EngineNotRunning.Location = New Point(125, 1484)
        Desc_EngineNotRunning.Name = "Desc_EngineNotRunning"
        Desc_EngineNotRunning.Size = New Size(300, 27)
        Desc_EngineNotRunning.TabIndex = 113
        Desc_EngineNotRunning.Text = "Engine not running"
        ' 
        ' ToggleEngineNotRunning
        ' 
        ToggleEngineNotRunning.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleEngineNotRunning.ImeMode = ImeMode.Off
        ToggleEngineNotRunning.IsOn = False
        ToggleEngineNotRunning.Location = New Point(71, 1485)
        ToggleEngineNotRunning.Name = "ToggleEngineNotRunning"
        ToggleEngineNotRunning.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleEngineNotRunning.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleEngineNotRunning.ShowGlow = False
        ToggleEngineNotRunning.Size = New Size(48, 24)
        ToggleEngineNotRunning.TabIndex = 114
        ToggleEngineNotRunning.Text = "ToggleSwitch"
        ' 
        ' Desc_EngineUIInUse
        ' 
        Desc_EngineUIInUse.AutoSize = True
        Desc_EngineUIInUse.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_EngineUIInUse.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_EngineUIInUse.ForeColor = Color.White
        Desc_EngineUIInUse.Location = New Point(125, 1528)
        Desc_EngineUIInUse.Name = "Desc_EngineUIInUse"
        Desc_EngineUIInUse.Size = New Size(300, 27)
        Desc_EngineUIInUse.TabIndex = 115
        Desc_EngineUIInUse.Text = "Engine UI already in use"
        ' 
        ' ToggleEngineUIInUse
        ' 
        ToggleEngineUIInUse.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleEngineUIInUse.ImeMode = ImeMode.Off
        ToggleEngineUIInUse.IsOn = False
        ToggleEngineUIInUse.Location = New Point(71, 1529)
        ToggleEngineUIInUse.Name = "ToggleEngineUIInUse"
        ToggleEngineUIInUse.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleEngineUIInUse.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleEngineUIInUse.ShowGlow = False
        ToggleEngineUIInUse.Size = New Size(48, 24)
        ToggleEngineUIInUse.TabIndex = 116
        ToggleEngineUIInUse.Text = "ToggleSwitch"
        ' 
        ' Desc_ErrorResolution
        ' 
        Desc_ErrorResolution.AutoSize = True
        Desc_ErrorResolution.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_ErrorResolution.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ErrorResolution.ForeColor = Color.White
        Desc_ErrorResolution.Location = New Point(125, 1572)
        Desc_ErrorResolution.Name = "Desc_ErrorResolution"
        Desc_ErrorResolution.Size = New Size(300, 27)
        Desc_ErrorResolution.TabIndex = 117
        Desc_ErrorResolution.Text = "Resolution error"
        ' 
        ' ToggleErrorResolution
        ' 
        ToggleErrorResolution.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleErrorResolution.ImeMode = ImeMode.Off
        ToggleErrorResolution.IsOn = False
        ToggleErrorResolution.Location = New Point(71, 1573)
        ToggleErrorResolution.Name = "ToggleErrorResolution"
        ToggleErrorResolution.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleErrorResolution.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleErrorResolution.ShowGlow = False
        ToggleErrorResolution.Size = New Size(48, 24)
        ToggleErrorResolution.TabIndex = 118
        ToggleErrorResolution.Text = "ToggleSwitch"
        ' 
        ' Desc_DesktopCaptureDisabled
        ' 
        Desc_DesktopCaptureDisabled.AutoSize = True
        Desc_DesktopCaptureDisabled.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Desc_DesktopCaptureDisabled.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_DesktopCaptureDisabled.ForeColor = Color.White
        Desc_DesktopCaptureDisabled.Location = New Point(125, 1616)
        Desc_DesktopCaptureDisabled.Name = "Desc_DesktopCaptureDisabled"
        Desc_DesktopCaptureDisabled.Size = New Size(300, 27)
        Desc_DesktopCaptureDisabled.TabIndex = 119
        Desc_DesktopCaptureDisabled.Text = "Desktop capture disabled"
        ' 
        ' ToggleDesktopCaptureDisabled
        ' 
        ToggleDesktopCaptureDisabled.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        ToggleDesktopCaptureDisabled.ImeMode = ImeMode.Off
        ToggleDesktopCaptureDisabled.IsOn = False
        ToggleDesktopCaptureDisabled.Location = New Point(71, 1617)
        ToggleDesktopCaptureDisabled.Name = "ToggleDesktopCaptureDisabled"
        ToggleDesktopCaptureDisabled.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleDesktopCaptureDisabled.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleDesktopCaptureDisabled.ShowGlow = False
        ToggleDesktopCaptureDisabled.Size = New Size(48, 24)
        ToggleDesktopCaptureDisabled.TabIndex = 120
        ToggleDesktopCaptureDisabled.Text = "ToggleSwitch"
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
    Friend WithEvents Header_Recording As Label
    Friend WithEvents Desc_RecordingStarted As Label
    Friend WithEvents ToggleRecordingStarted As ToggleSwitch
    Friend WithEvents Desc_RecordingSaved As Label
    Friend WithEvents ToggleRecordingSaved As ToggleSwitch
    Friend WithEvents Desc_RecordingError As Label
    Friend WithEvents ToggleRecordingError As ToggleSwitch
    Friend WithEvents Header_InstantReplay As Label
    Friend WithEvents Desc_ReplaySaved As Label
    Friend WithEvents ToggleReplaySaved As ToggleSwitch
    Friend WithEvents Desc_InstantReplayOn As Label
    Friend WithEvents ToggleInstantReplayOn As ToggleSwitch
    Friend WithEvents Desc_InstantReplayOff As Label
    Friend WithEvents ToggleInstantReplayOff As ToggleSwitch
    Friend WithEvents Desc_ReplayTurnOn As Label
    Friend WithEvents ToggleReplayTurnOn As ToggleSwitch
    Friend WithEvents Desc_ReplayError As Label
    Friend WithEvents ToggleReplayError As ToggleSwitch
    Friend WithEvents Header_Screenshots As Label
    Friend WithEvents Desc_ScreenshotSaved As Label
    Friend WithEvents ToggleScreenshotSaved As ToggleSwitch
    Friend WithEvents Desc_ValidSavePath As Label
    Friend WithEvents ToggleValidSavePath As ToggleSwitch
    Friend WithEvents Header_ShareOverlay As Label
    Friend WithEvents Desc_OpenShare As Label
    Friend WithEvents ToggleOpenShare As ToggleSwitch
    Friend WithEvents Header_SystemMonitor As Label
    Friend WithEvents Desc_RamWarning As Label
    Friend WithEvents ToggleRamWarning As ToggleSwitch
    Friend WithEvents Desc_RamWarning95 As Label
    Friend WithEvents ToggleRamWarning95 As ToggleSwitch
    Friend WithEvents Desc_RamCritical As Label
    Friend WithEvents ToggleRamCritical As ToggleSwitch
    Friend WithEvents Desc_CpuWarning As Label
    Friend WithEvents ToggleCpuWarning As ToggleSwitch
    Friend WithEvents Desc_DiskSpaceLow As Label
    Friend WithEvents ToggleDiskSpaceLow As ToggleSwitch
    Friend WithEvents Header_Updates As Label
    Friend WithEvents Desc_UpdateAvailable As Label
    Friend WithEvents ToggleUpdateAvailable As ToggleSwitch
    Friend WithEvents Desc_VersionLatest As Label
    Friend WithEvents ToggleVersionLatest As ToggleSwitch
    Friend WithEvents Desc_UpdateError As Label
    Friend WithEvents ToggleUpdateError As ToggleSwitch
    Friend WithEvents Header_Errors As Label
    Friend WithEvents Desc_AccountConfirmError As Label
    Friend WithEvents ToggleAccountConfirmError As ToggleSwitch
    Friend WithEvents Desc_ExtensionNotFound As Label
    Friend WithEvents ToggleExtensionNotFound As ToggleSwitch
    Friend WithEvents Desc_FeatureNotReady As Label
    Friend WithEvents ToggleFeatureNotReady As ToggleSwitch
    Friend WithEvents Desc_GpuRequired As Label
    Friend WithEvents ToggleGpuRequired As ToggleSwitch
    Friend WithEvents Desc_EngineNotRunning As Label
    Friend WithEvents ToggleEngineNotRunning As ToggleSwitch
    Friend WithEvents Desc_EngineUIInUse As Label
    Friend WithEvents ToggleEngineUIInUse As ToggleSwitch
    Friend WithEvents Desc_ErrorResolution As Label
    Friend WithEvents ToggleErrorResolution As ToggleSwitch
    Friend WithEvents Desc_DesktopCaptureDisabled As Label
    Friend WithEvents ToggleDesktopCaptureDisabled As ToggleSwitch
End Class
