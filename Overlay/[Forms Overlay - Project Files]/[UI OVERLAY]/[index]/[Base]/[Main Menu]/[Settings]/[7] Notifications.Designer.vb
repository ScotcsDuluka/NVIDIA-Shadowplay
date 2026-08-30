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
        Menu_SubText = New Label()
        BT_EnableAll = New Label()
        BT_DisableAll = New Label()
        Card_Recording = New Panel()
        Accent_Recording = New Panel()
        Header_Recording = New Label()
        Desc_RecordingStarted = New Label()
        ToggleRecordingStarted = New ToggleSwitch()
        Desc_RecordingSaved = New Label()
        ToggleRecordingSaved = New ToggleSwitch()
        Desc_RecordingError = New Label()
        ToggleRecordingError = New ToggleSwitch()
        Card_Screenshots = New Panel()
        Accent_Screenshots = New Panel()
        Header_Screenshots = New Label()
        Desc_ScreenshotSaved = New Label()
        ToggleScreenshotSaved = New ToggleSwitch()
        Desc_ValidSavePath = New Label()
        ToggleValidSavePath = New ToggleSwitch()
        Card_ShareOverlay = New Panel()
        Accent_ShareOverlay = New Panel()
        Header_ShareOverlay = New Label()
        Desc_OpenShare = New Label()
        ToggleOpenShare = New ToggleSwitch()
        Card_InstantReplay = New Panel()
        Accent_InstantReplay = New Panel()
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
        Card_SystemMonitor = New Panel()
        Accent_SystemMonitor = New Panel()
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
        Card_Updates = New Panel()
        Accent_Updates = New Panel()
        Header_Updates = New Label()
        Desc_UpdateAvailable = New Label()
        ToggleUpdateAvailable = New ToggleSwitch()
        Desc_VersionLatest = New Label()
        ToggleVersionLatest = New ToggleSwitch()
        Desc_UpdateError = New Label()
        ToggleUpdateError = New ToggleSwitch()
        Card_Errors = New Panel()
        Accent_Errors = New Panel()
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
        Card_ToastSlots = New Panel()
        Accent_ToastSlots = New Panel()
        Header_ToastSlots = New Label()
        ToggleToastSlot2 = New ToggleSwitch()
        Desc_ToastSlots = New Label()
        Desc_ToastSlots_SUB = New Label()
        ToggleToastSlot3 = New ToggleSwitch()
        Desc_ToastSlots3 = New Label()
        Card_OBS = New Panel()
        Accent_OBS = New Panel()
        Header_OBS = New Label()
        ObsEnabledToggle = New ToggleSwitch()
        Desc_OBS = New Label()
        Panel_OBS = New Panel()
        Label6 = New Label()
        PORT_BOX = New TextBox()
        KEY_BOX = New TextBox()
        Label7 = New Label()
        Label8 = New Label()
        HOST_BOX = New TextBox()
        Label9 = New Label()
        Menu_Top_Dim = New PictureBox()
        BT_Back = New Label()
        Dim_Top = New PictureBox()
        Dim_1 = New PictureBox()
        Dim_2 = New PictureBox()
        Menu_Settings.SuspendLayout()
        Card_Recording.SuspendLayout()
        Card_Screenshots.SuspendLayout()
        Card_ShareOverlay.SuspendLayout()
        Card_InstantReplay.SuspendLayout()
        Card_SystemMonitor.SuspendLayout()
        Card_Updates.SuspendLayout()
        Card_Errors.SuspendLayout()
        Card_ToastSlots.SuspendLayout()
        Card_OBS.SuspendLayout()
        Panel_OBS.SuspendLayout()
        CType(Menu_Top_Dim, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).BeginInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Menu_Settings
        ' 
        Menu_Settings.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Menu_Settings.AutoScroll = True
        Menu_Settings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Menu_Settings.Controls.Add(Menu_Text)
        Menu_Settings.Controls.Add(Menu_SubText)
        Menu_Settings.Controls.Add(Card_Recording)
        Menu_Settings.Controls.Add(Card_Screenshots)
        Menu_Settings.Controls.Add(Card_ShareOverlay)
        Menu_Settings.Controls.Add(Card_InstantReplay)
        Menu_Settings.Controls.Add(Card_SystemMonitor)
        Menu_Settings.Controls.Add(Card_Updates)
        Menu_Settings.Controls.Add(Card_Errors)
        Menu_Settings.Controls.Add(Card_ToastSlots)
        Menu_Settings.Controls.Add(Card_OBS)
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
        ' Menu_SubText
        ' 
        Menu_SubText.AutoSize = True
        Menu_SubText.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Menu_SubText.Font = New Font("Segoe UI", 10F)
        Menu_SubText.ForeColor = Color.FromArgb(CByte(154), CByte(160), CByte(166))
        Menu_SubText.Location = New Point(64, 92)
        Menu_SubText.Name = "Menu_SubText"
        Menu_SubText.Size = New Size(273, 19)
        Menu_SubText.TabIndex = 55
        Menu_SubText.Text = "Choose which notifications appear in-game"
        ' 
        ' BT_EnableAll
        ' 
        BT_EnableAll.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        BT_EnableAll.Cursor = Cursors.Hand
        BT_EnableAll.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        BT_EnableAll.ForeColor = Color.White
        BT_EnableAll.Location = New Point(286, 116)
        BT_EnableAll.Name = "BT_EnableAll"
        BT_EnableAll.Size = New Size(160, 44)
        BT_EnableAll.TabIndex = 56
        BT_EnableAll.Text = "Enable all"
        BT_EnableAll.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' BT_DisableAll
        ' 
        BT_DisableAll.BackColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        BT_DisableAll.Cursor = Cursors.Hand
        BT_DisableAll.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        BT_DisableAll.ForeColor = Color.White
        BT_DisableAll.Location = New Point(452, 121)
        BT_DisableAll.Name = "BT_DisableAll"
        BT_DisableAll.Size = New Size(160, 44)
        BT_DisableAll.TabIndex = 57
        BT_DisableAll.Text = "Disable all"
        BT_DisableAll.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Card_Recording
        ' 
        Card_Recording.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_Recording.Controls.Add(Accent_Recording)
        Card_Recording.Controls.Add(Header_Recording)
        Card_Recording.Controls.Add(Desc_RecordingStarted)
        Card_Recording.Controls.Add(ToggleRecordingStarted)
        Card_Recording.Controls.Add(Desc_RecordingSaved)
        Card_Recording.Controls.Add(ToggleRecordingSaved)
        Card_Recording.Controls.Add(Desc_RecordingError)
        Card_Recording.Controls.Add(ToggleRecordingError)
        Card_Recording.Location = New Point(20, 140)
        Card_Recording.Name = "Card_Recording"
        Card_Recording.Size = New Size(495, 180)
        Card_Recording.TabIndex = 10
        ' 
        ' Accent_Recording
        ' 
        Accent_Recording.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_Recording.Location = New Point(20, 22)
        Accent_Recording.Name = "Accent_Recording"
        Accent_Recording.Size = New Size(4, 20)
        Accent_Recording.TabIndex = 0
        ' 
        ' Header_Recording
        ' 
        Header_Recording.AutoSize = True
        Header_Recording.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_Recording.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Recording.ForeColor = Color.White
        Header_Recording.Location = New Point(34, 18)
        Header_Recording.Name = "Header_Recording"
        Header_Recording.Size = New Size(122, 28)
        Header_Recording.TabIndex = 1
        Header_Recording.Text = "RECORDING"
        ' 
        ' Desc_RecordingStarted
        ' 
        Desc_RecordingStarted.AutoSize = True
        Desc_RecordingStarted.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_RecordingStarted.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RecordingStarted.ForeColor = Color.White
        Desc_RecordingStarted.Location = New Point(78, 64)
        Desc_RecordingStarted.Name = "Desc_RecordingStarted"
        Desc_RecordingStarted.Size = New Size(142, 21)
        Desc_RecordingStarted.TabIndex = 2
        Desc_RecordingStarted.Text = "Recording started"
        ' 
        ' ToggleRecordingStarted
        ' 
        ToggleRecordingStarted.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleRecordingStarted.ImeMode = ImeMode.Off
        ToggleRecordingStarted.IsOn = False
        ToggleRecordingStarted.Location = New Point(20, 65)
        ToggleRecordingStarted.Name = "ToggleRecordingStarted"
        ToggleRecordingStarted.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecordingStarted.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecordingStarted.ShowGlow = False
        ToggleRecordingStarted.Size = New Size(48, 24)
        ToggleRecordingStarted.TabIndex = 3
        ToggleRecordingStarted.Text = "ToggleSwitch"
        ' 
        ' Desc_RecordingSaved
        ' 
        Desc_RecordingSaved.AutoSize = True
        Desc_RecordingSaved.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_RecordingSaved.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RecordingSaved.ForeColor = Color.White
        Desc_RecordingSaved.Location = New Point(78, 102)
        Desc_RecordingSaved.Name = "Desc_RecordingSaved"
        Desc_RecordingSaved.Size = New Size(132, 21)
        Desc_RecordingSaved.TabIndex = 4
        Desc_RecordingSaved.Text = "Recording saved"
        ' 
        ' ToggleRecordingSaved
        ' 
        ToggleRecordingSaved.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleRecordingSaved.ImeMode = ImeMode.Off
        ToggleRecordingSaved.IsOn = False
        ToggleRecordingSaved.Location = New Point(20, 103)
        ToggleRecordingSaved.Name = "ToggleRecordingSaved"
        ToggleRecordingSaved.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecordingSaved.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecordingSaved.ShowGlow = False
        ToggleRecordingSaved.Size = New Size(48, 24)
        ToggleRecordingSaved.TabIndex = 5
        ToggleRecordingSaved.Text = "ToggleSwitch"
        ' 
        ' Desc_RecordingError
        ' 
        Desc_RecordingError.AutoSize = True
        Desc_RecordingError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_RecordingError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RecordingError.ForeColor = Color.White
        Desc_RecordingError.Location = New Point(78, 140)
        Desc_RecordingError.Name = "Desc_RecordingError"
        Desc_RecordingError.Size = New Size(127, 21)
        Desc_RecordingError.TabIndex = 6
        Desc_RecordingError.Text = "Recording error"
        ' 
        ' ToggleRecordingError
        ' 
        ToggleRecordingError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleRecordingError.ImeMode = ImeMode.Off
        ToggleRecordingError.IsOn = False
        ToggleRecordingError.Location = New Point(20, 141)
        ToggleRecordingError.Name = "ToggleRecordingError"
        ToggleRecordingError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRecordingError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRecordingError.ShowGlow = False
        ToggleRecordingError.Size = New Size(48, 24)
        ToggleRecordingError.TabIndex = 7
        ToggleRecordingError.Text = "ToggleSwitch"
        ' 
        ' Card_Screenshots
        ' 
        Card_Screenshots.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_Screenshots.Controls.Add(Accent_Screenshots)
        Card_Screenshots.Controls.Add(Header_Screenshots)
        Card_Screenshots.Controls.Add(Desc_ScreenshotSaved)
        Card_Screenshots.Controls.Add(ToggleScreenshotSaved)
        Card_Screenshots.Controls.Add(Desc_ValidSavePath)
        Card_Screenshots.Controls.Add(ToggleValidSavePath)
        Card_Screenshots.Location = New Point(521, 140)
        Card_Screenshots.Name = "Card_Screenshots"
        Card_Screenshots.Size = New Size(848, 142)
        Card_Screenshots.TabIndex = 11
        ' 
        ' Accent_Screenshots
        ' 
        Accent_Screenshots.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_Screenshots.Location = New Point(20, 22)
        Accent_Screenshots.Name = "Accent_Screenshots"
        Accent_Screenshots.Size = New Size(4, 20)
        Accent_Screenshots.TabIndex = 0
        ' 
        ' Header_Screenshots
        ' 
        Header_Screenshots.AutoSize = True
        Header_Screenshots.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_Screenshots.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Screenshots.ForeColor = Color.White
        Header_Screenshots.Location = New Point(34, 18)
        Header_Screenshots.Name = "Header_Screenshots"
        Header_Screenshots.Size = New Size(144, 28)
        Header_Screenshots.TabIndex = 1
        Header_Screenshots.Text = "SCREENSHOTS"
        ' 
        ' Desc_ScreenshotSaved
        ' 
        Desc_ScreenshotSaved.AutoSize = True
        Desc_ScreenshotSaved.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ScreenshotSaved.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ScreenshotSaved.ForeColor = Color.White
        Desc_ScreenshotSaved.Location = New Point(78, 64)
        Desc_ScreenshotSaved.Name = "Desc_ScreenshotSaved"
        Desc_ScreenshotSaved.Size = New Size(213, 21)
        Desc_ScreenshotSaved.TabIndex = 2
        Desc_ScreenshotSaved.Text = "Screenshot saved to Gallery"
        ' 
        ' ToggleScreenshotSaved
        ' 
        ToggleScreenshotSaved.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleScreenshotSaved.ImeMode = ImeMode.Off
        ToggleScreenshotSaved.IsOn = False
        ToggleScreenshotSaved.Location = New Point(20, 65)
        ToggleScreenshotSaved.Name = "ToggleScreenshotSaved"
        ToggleScreenshotSaved.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleScreenshotSaved.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleScreenshotSaved.ShowGlow = False
        ToggleScreenshotSaved.Size = New Size(48, 24)
        ToggleScreenshotSaved.TabIndex = 3
        ToggleScreenshotSaved.Text = "ToggleSwitch"
        ' 
        ' Desc_ValidSavePath
        ' 
        Desc_ValidSavePath.AutoSize = True
        Desc_ValidSavePath.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ValidSavePath.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ValidSavePath.ForeColor = Color.White
        Desc_ValidSavePath.Location = New Point(78, 102)
        Desc_ValidSavePath.Name = "Desc_ValidSavePath"
        Desc_ValidSavePath.Size = New Size(193, 21)
        Desc_ValidSavePath.TabIndex = 4
        Desc_ValidSavePath.Text = "Invalid save path warning"
        ' 
        ' ToggleValidSavePath
        ' 
        ToggleValidSavePath.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleValidSavePath.ImeMode = ImeMode.Off
        ToggleValidSavePath.IsOn = False
        ToggleValidSavePath.Location = New Point(20, 103)
        ToggleValidSavePath.Name = "ToggleValidSavePath"
        ToggleValidSavePath.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleValidSavePath.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleValidSavePath.ShowGlow = False
        ToggleValidSavePath.Size = New Size(48, 24)
        ToggleValidSavePath.TabIndex = 5
        ToggleValidSavePath.Text = "ToggleSwitch"
        ' 
        ' Card_ShareOverlay
        ' 
        Card_ShareOverlay.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_ShareOverlay.Controls.Add(Accent_ShareOverlay)
        Card_ShareOverlay.Controls.Add(Header_ShareOverlay)
        Card_ShareOverlay.Controls.Add(Desc_OpenShare)
        Card_ShareOverlay.Controls.Add(ToggleOpenShare)
        Card_ShareOverlay.Location = New Point(20, 326)
        Card_ShareOverlay.Name = "Card_ShareOverlay"
        Card_ShareOverlay.Size = New Size(495, 104)
        Card_ShareOverlay.TabIndex = 12
        ' 
        ' Accent_ShareOverlay
        ' 
        Accent_ShareOverlay.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_ShareOverlay.Location = New Point(20, 22)
        Accent_ShareOverlay.Name = "Accent_ShareOverlay"
        Accent_ShareOverlay.Size = New Size(4, 20)
        Accent_ShareOverlay.TabIndex = 0
        ' 
        ' Header_ShareOverlay
        ' 
        Header_ShareOverlay.AutoSize = True
        Header_ShareOverlay.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_ShareOverlay.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_ShareOverlay.ForeColor = Color.White
        Header_ShareOverlay.Location = New Point(34, 18)
        Header_ShareOverlay.Name = "Header_ShareOverlay"
        Header_ShareOverlay.Size = New Size(163, 28)
        Header_ShareOverlay.TabIndex = 1
        Header_ShareOverlay.Text = "SHARE OVERLAY"
        ' 
        ' Desc_OpenShare
        ' 
        Desc_OpenShare.AutoSize = True
        Desc_OpenShare.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_OpenShare.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_OpenShare.ForeColor = Color.White
        Desc_OpenShare.Location = New Point(78, 64)
        Desc_OpenShare.Name = "Desc_OpenShare"
        Desc_OpenShare.Size = New Size(224, 21)
        Desc_OpenShare.TabIndex = 2
        Desc_OpenShare.Text = "Share overlay opened (Alt+Z)"
        ' 
        ' ToggleOpenShare
        ' 
        ToggleOpenShare.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleOpenShare.ImeMode = ImeMode.Off
        ToggleOpenShare.IsOn = False
        ToggleOpenShare.Location = New Point(20, 65)
        ToggleOpenShare.Name = "ToggleOpenShare"
        ToggleOpenShare.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleOpenShare.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleOpenShare.ShowGlow = False
        ToggleOpenShare.Size = New Size(48, 24)
        ToggleOpenShare.TabIndex = 3
        ToggleOpenShare.Text = "ToggleSwitch"
        ' 
        ' Card_InstantReplay
        ' 
        Card_InstantReplay.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_InstantReplay.Controls.Add(Accent_InstantReplay)
        Card_InstantReplay.Controls.Add(Header_InstantReplay)
        Card_InstantReplay.Controls.Add(Desc_ReplaySaved)
        Card_InstantReplay.Controls.Add(ToggleReplaySaved)
        Card_InstantReplay.Controls.Add(Desc_InstantReplayOn)
        Card_InstantReplay.Controls.Add(ToggleInstantReplayOn)
        Card_InstantReplay.Controls.Add(Desc_InstantReplayOff)
        Card_InstantReplay.Controls.Add(ToggleInstantReplayOff)
        Card_InstantReplay.Controls.Add(Desc_ReplayTurnOn)
        Card_InstantReplay.Controls.Add(ToggleReplayTurnOn)
        Card_InstantReplay.Controls.Add(Desc_ReplayError)
        Card_InstantReplay.Controls.Add(ToggleReplayError)
        Card_InstantReplay.Location = New Point(521, 288)
        Card_InstantReplay.Name = "Card_InstantReplay"
        Card_InstantReplay.Size = New Size(848, 256)
        Card_InstantReplay.TabIndex = 13
        ' 
        ' Accent_InstantReplay
        ' 
        Accent_InstantReplay.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_InstantReplay.Location = New Point(20, 22)
        Accent_InstantReplay.Name = "Accent_InstantReplay"
        Accent_InstantReplay.Size = New Size(4, 20)
        Accent_InstantReplay.TabIndex = 0
        ' 
        ' Header_InstantReplay
        ' 
        Header_InstantReplay.AutoSize = True
        Header_InstantReplay.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_InstantReplay.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_InstantReplay.ForeColor = Color.White
        Header_InstantReplay.Location = New Point(34, 18)
        Header_InstantReplay.Name = "Header_InstantReplay"
        Header_InstantReplay.Size = New Size(167, 28)
        Header_InstantReplay.TabIndex = 1
        Header_InstantReplay.Text = "INSTANT REPLAY"
        ' 
        ' Desc_ReplaySaved
        ' 
        Desc_ReplaySaved.AutoSize = True
        Desc_ReplaySaved.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ReplaySaved.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ReplaySaved.ForeColor = Color.White
        Desc_ReplaySaved.Location = New Point(78, 64)
        Desc_ReplaySaved.Name = "Desc_ReplaySaved"
        Desc_ReplaySaved.Size = New Size(159, 21)
        Desc_ReplaySaved.TabIndex = 2
        Desc_ReplaySaved.Text = "Instant Replay saved"
        ' 
        ' ToggleReplaySaved
        ' 
        ToggleReplaySaved.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleReplaySaved.ImeMode = ImeMode.Off
        ToggleReplaySaved.IsOn = False
        ToggleReplaySaved.Location = New Point(20, 65)
        ToggleReplaySaved.Name = "ToggleReplaySaved"
        ToggleReplaySaved.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleReplaySaved.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleReplaySaved.ShowGlow = False
        ToggleReplaySaved.Size = New Size(48, 24)
        ToggleReplaySaved.TabIndex = 3
        ToggleReplaySaved.Text = "ToggleSwitch"
        ' 
        ' Desc_InstantReplayOn
        ' 
        Desc_InstantReplayOn.AutoSize = True
        Desc_InstantReplayOn.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_InstantReplayOn.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_InstantReplayOn.ForeColor = Color.White
        Desc_InstantReplayOn.Location = New Point(78, 102)
        Desc_InstantReplayOn.Name = "Desc_InstantReplayOn"
        Desc_InstantReplayOn.Size = New Size(136, 21)
        Desc_InstantReplayOn.TabIndex = 4
        Desc_InstantReplayOn.Text = "Instant Replay on"
        ' 
        ' ToggleInstantReplayOn
        ' 
        ToggleInstantReplayOn.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleInstantReplayOn.ImeMode = ImeMode.Off
        ToggleInstantReplayOn.IsOn = False
        ToggleInstantReplayOn.Location = New Point(20, 103)
        ToggleInstantReplayOn.Name = "ToggleInstantReplayOn"
        ToggleInstantReplayOn.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleInstantReplayOn.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleInstantReplayOn.ShowGlow = False
        ToggleInstantReplayOn.Size = New Size(48, 24)
        ToggleInstantReplayOn.TabIndex = 5
        ToggleInstantReplayOn.Text = "ToggleSwitch"
        ' 
        ' Desc_InstantReplayOff
        ' 
        Desc_InstantReplayOff.AutoSize = True
        Desc_InstantReplayOff.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_InstantReplayOff.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_InstantReplayOff.ForeColor = Color.White
        Desc_InstantReplayOff.Location = New Point(78, 140)
        Desc_InstantReplayOff.Name = "Desc_InstantReplayOff"
        Desc_InstantReplayOff.Size = New Size(139, 21)
        Desc_InstantReplayOff.TabIndex = 6
        Desc_InstantReplayOff.Text = "Instant Replay off"
        ' 
        ' ToggleInstantReplayOff
        ' 
        ToggleInstantReplayOff.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleInstantReplayOff.ImeMode = ImeMode.Off
        ToggleInstantReplayOff.IsOn = False
        ToggleInstantReplayOff.Location = New Point(20, 141)
        ToggleInstantReplayOff.Name = "ToggleInstantReplayOff"
        ToggleInstantReplayOff.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleInstantReplayOff.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleInstantReplayOff.ShowGlow = False
        ToggleInstantReplayOff.Size = New Size(48, 24)
        ToggleInstantReplayOff.TabIndex = 7
        ToggleInstantReplayOff.Text = "ToggleSwitch"
        ' 
        ' Desc_ReplayTurnOn
        ' 
        Desc_ReplayTurnOn.AutoSize = True
        Desc_ReplayTurnOn.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ReplayTurnOn.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ReplayTurnOn.ForeColor = Color.White
        Desc_ReplayTurnOn.Location = New Point(78, 178)
        Desc_ReplayTurnOn.Name = "Desc_ReplayTurnOn"
        Desc_ReplayTurnOn.Size = New Size(195, 21)
        Desc_ReplayTurnOn.TabIndex = 8
        Desc_ReplayTurnOn.Text = "Turning on Instant Replay"
        ' 
        ' ToggleReplayTurnOn
        ' 
        ToggleReplayTurnOn.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleReplayTurnOn.ImeMode = ImeMode.Off
        ToggleReplayTurnOn.IsOn = False
        ToggleReplayTurnOn.Location = New Point(20, 179)
        ToggleReplayTurnOn.Name = "ToggleReplayTurnOn"
        ToggleReplayTurnOn.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleReplayTurnOn.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleReplayTurnOn.ShowGlow = False
        ToggleReplayTurnOn.Size = New Size(48, 24)
        ToggleReplayTurnOn.TabIndex = 9
        ToggleReplayTurnOn.Text = "ToggleSwitch"
        ' 
        ' Desc_ReplayError
        ' 
        Desc_ReplayError.AutoSize = True
        Desc_ReplayError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ReplayError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ReplayError.ForeColor = Color.White
        Desc_ReplayError.Location = New Point(78, 216)
        Desc_ReplayError.Name = "Desc_ReplayError"
        Desc_ReplayError.Size = New Size(154, 21)
        Desc_ReplayError.TabIndex = 10
        Desc_ReplayError.Text = "Instant Replay error"
        ' 
        ' ToggleReplayError
        ' 
        ToggleReplayError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleReplayError.ImeMode = ImeMode.Off
        ToggleReplayError.IsOn = False
        ToggleReplayError.Location = New Point(20, 217)
        ToggleReplayError.Name = "ToggleReplayError"
        ToggleReplayError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleReplayError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleReplayError.ShowGlow = False
        ToggleReplayError.Size = New Size(48, 24)
        ToggleReplayError.TabIndex = 11
        ToggleReplayError.Text = "ToggleSwitch"
        ' 
        ' Card_SystemMonitor
        ' 
        Card_SystemMonitor.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_SystemMonitor.Controls.Add(Accent_SystemMonitor)
        Card_SystemMonitor.Controls.Add(Header_SystemMonitor)
        Card_SystemMonitor.Controls.Add(Desc_RamWarning)
        Card_SystemMonitor.Controls.Add(ToggleRamWarning)
        Card_SystemMonitor.Controls.Add(Desc_RamWarning95)
        Card_SystemMonitor.Controls.Add(ToggleRamWarning95)
        Card_SystemMonitor.Controls.Add(Desc_RamCritical)
        Card_SystemMonitor.Controls.Add(ToggleRamCritical)
        Card_SystemMonitor.Controls.Add(Desc_CpuWarning)
        Card_SystemMonitor.Controls.Add(ToggleCpuWarning)
        Card_SystemMonitor.Controls.Add(Desc_DiskSpaceLow)
        Card_SystemMonitor.Controls.Add(ToggleDiskSpaceLow)
        Card_SystemMonitor.Location = New Point(20, 436)
        Card_SystemMonitor.Name = "Card_SystemMonitor"
        Card_SystemMonitor.Size = New Size(495, 256)
        Card_SystemMonitor.TabIndex = 14
        ' 
        ' Accent_SystemMonitor
        ' 
        Accent_SystemMonitor.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_SystemMonitor.Location = New Point(20, 22)
        Accent_SystemMonitor.Name = "Accent_SystemMonitor"
        Accent_SystemMonitor.Size = New Size(4, 20)
        Accent_SystemMonitor.TabIndex = 0
        ' 
        ' Header_SystemMonitor
        ' 
        Header_SystemMonitor.AutoSize = True
        Header_SystemMonitor.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_SystemMonitor.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_SystemMonitor.ForeColor = Color.White
        Header_SystemMonitor.Location = New Point(34, 18)
        Header_SystemMonitor.Name = "Header_SystemMonitor"
        Header_SystemMonitor.Size = New Size(182, 28)
        Header_SystemMonitor.TabIndex = 1
        Header_SystemMonitor.Text = "SYSTEM MONITOR"
        ' 
        ' Desc_RamWarning
        ' 
        Desc_RamWarning.AutoSize = True
        Desc_RamWarning.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_RamWarning.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RamWarning.ForeColor = Color.White
        Desc_RamWarning.Location = New Point(78, 64)
        Desc_RamWarning.Name = "Desc_RamWarning"
        Desc_RamWarning.Size = New Size(155, 21)
        Desc_RamWarning.TabIndex = 2
        Desc_RamWarning.Text = "RAM usage warning"
        ' 
        ' ToggleRamWarning
        ' 
        ToggleRamWarning.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleRamWarning.ImeMode = ImeMode.Off
        ToggleRamWarning.IsOn = False
        ToggleRamWarning.Location = New Point(20, 65)
        ToggleRamWarning.Name = "ToggleRamWarning"
        ToggleRamWarning.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRamWarning.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRamWarning.ShowGlow = False
        ToggleRamWarning.Size = New Size(48, 24)
        ToggleRamWarning.TabIndex = 3
        ToggleRamWarning.Text = "ToggleSwitch"
        ' 
        ' Desc_RamWarning95
        ' 
        Desc_RamWarning95.AutoSize = True
        Desc_RamWarning95.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_RamWarning95.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RamWarning95.ForeColor = Color.White
        Desc_RamWarning95.Location = New Point(78, 102)
        Desc_RamWarning95.Name = "Desc_RamWarning95"
        Desc_RamWarning95.Size = New Size(128, 21)
        Desc_RamWarning95.TabIndex = 4
        Desc_RamWarning95.Text = "RAM usage 95%"
        ' 
        ' ToggleRamWarning95
        ' 
        ToggleRamWarning95.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleRamWarning95.ImeMode = ImeMode.Off
        ToggleRamWarning95.IsOn = False
        ToggleRamWarning95.Location = New Point(20, 103)
        ToggleRamWarning95.Name = "ToggleRamWarning95"
        ToggleRamWarning95.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRamWarning95.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRamWarning95.ShowGlow = False
        ToggleRamWarning95.Size = New Size(48, 24)
        ToggleRamWarning95.TabIndex = 5
        ToggleRamWarning95.Text = "ToggleSwitch"
        ' 
        ' Desc_RamCritical
        ' 
        Desc_RamCritical.AutoSize = True
        Desc_RamCritical.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_RamCritical.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_RamCritical.ForeColor = Color.White
        Desc_RamCritical.Location = New Point(78, 140)
        Desc_RamCritical.Name = "Desc_RamCritical"
        Desc_RamCritical.Size = New Size(145, 21)
        Desc_RamCritical.TabIndex = 6
        Desc_RamCritical.Text = "RAM usage critical"
        ' 
        ' ToggleRamCritical
        ' 
        ToggleRamCritical.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleRamCritical.ImeMode = ImeMode.Off
        ToggleRamCritical.IsOn = False
        ToggleRamCritical.Location = New Point(20, 141)
        ToggleRamCritical.Name = "ToggleRamCritical"
        ToggleRamCritical.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleRamCritical.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleRamCritical.ShowGlow = False
        ToggleRamCritical.Size = New Size(48, 24)
        ToggleRamCritical.TabIndex = 7
        ToggleRamCritical.Text = "ToggleSwitch"
        ' 
        ' Desc_CpuWarning
        ' 
        Desc_CpuWarning.AutoSize = True
        Desc_CpuWarning.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_CpuWarning.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_CpuWarning.ForeColor = Color.White
        Desc_CpuWarning.Location = New Point(78, 178)
        Desc_CpuWarning.Name = "Desc_CpuWarning"
        Desc_CpuWarning.Size = New Size(149, 21)
        Desc_CpuWarning.TabIndex = 8
        Desc_CpuWarning.Text = "CPU usage warning"
        ' 
        ' ToggleCpuWarning
        ' 
        ToggleCpuWarning.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleCpuWarning.ImeMode = ImeMode.Off
        ToggleCpuWarning.IsOn = False
        ToggleCpuWarning.Location = New Point(20, 179)
        ToggleCpuWarning.Name = "ToggleCpuWarning"
        ToggleCpuWarning.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleCpuWarning.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleCpuWarning.ShowGlow = False
        ToggleCpuWarning.Size = New Size(48, 24)
        ToggleCpuWarning.TabIndex = 9
        ToggleCpuWarning.Text = "ToggleSwitch"
        ' 
        ' Desc_DiskSpaceLow
        ' 
        Desc_DiskSpaceLow.AutoSize = True
        Desc_DiskSpaceLow.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_DiskSpaceLow.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_DiskSpaceLow.ForeColor = Color.White
        Desc_DiskSpaceLow.Location = New Point(78, 216)
        Desc_DiskSpaceLow.Name = "Desc_DiskSpaceLow"
        Desc_DiskSpaceLow.Size = New Size(116, 21)
        Desc_DiskSpaceLow.TabIndex = 10
        Desc_DiskSpaceLow.Text = "Disk space low"
        ' 
        ' ToggleDiskSpaceLow
        ' 
        ToggleDiskSpaceLow.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleDiskSpaceLow.ImeMode = ImeMode.Off
        ToggleDiskSpaceLow.IsOn = False
        ToggleDiskSpaceLow.Location = New Point(20, 217)
        ToggleDiskSpaceLow.Name = "ToggleDiskSpaceLow"
        ToggleDiskSpaceLow.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleDiskSpaceLow.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleDiskSpaceLow.ShowGlow = False
        ToggleDiskSpaceLow.Size = New Size(48, 24)
        ToggleDiskSpaceLow.TabIndex = 11
        ToggleDiskSpaceLow.Text = "ToggleSwitch"
        ' 
        ' Card_Updates
        ' 
        Card_Updates.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_Updates.Controls.Add(Accent_Updates)
        Card_Updates.Controls.Add(Header_Updates)
        Card_Updates.Controls.Add(Desc_UpdateAvailable)
        Card_Updates.Controls.Add(ToggleUpdateAvailable)
        Card_Updates.Controls.Add(Desc_VersionLatest)
        Card_Updates.Controls.Add(ToggleVersionLatest)
        Card_Updates.Controls.Add(Desc_UpdateError)
        Card_Updates.Controls.Add(ToggleUpdateError)
        Card_Updates.Location = New Point(521, 550)
        Card_Updates.Name = "Card_Updates"
        Card_Updates.Size = New Size(848, 180)
        Card_Updates.TabIndex = 15
        ' 
        ' Accent_Updates
        ' 
        Accent_Updates.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_Updates.Location = New Point(20, 22)
        Accent_Updates.Name = "Accent_Updates"
        Accent_Updates.Size = New Size(4, 20)
        Accent_Updates.TabIndex = 0
        ' 
        ' Header_Updates
        ' 
        Header_Updates.AutoSize = True
        Header_Updates.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_Updates.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Updates.ForeColor = Color.White
        Header_Updates.Location = New Point(34, 18)
        Header_Updates.Name = "Header_Updates"
        Header_Updates.Size = New Size(96, 28)
        Header_Updates.TabIndex = 1
        Header_Updates.Text = "UPDATES"
        ' 
        ' Desc_UpdateAvailable
        ' 
        Desc_UpdateAvailable.AutoSize = True
        Desc_UpdateAvailable.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_UpdateAvailable.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_UpdateAvailable.ForeColor = Color.White
        Desc_UpdateAvailable.Location = New Point(78, 64)
        Desc_UpdateAvailable.Name = "Desc_UpdateAvailable"
        Desc_UpdateAvailable.Size = New Size(131, 21)
        Desc_UpdateAvailable.TabIndex = 2
        Desc_UpdateAvailable.Text = "Update available"
        ' 
        ' ToggleUpdateAvailable
        ' 
        ToggleUpdateAvailable.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleUpdateAvailable.ImeMode = ImeMode.Off
        ToggleUpdateAvailable.IsOn = False
        ToggleUpdateAvailable.Location = New Point(20, 65)
        ToggleUpdateAvailable.Name = "ToggleUpdateAvailable"
        ToggleUpdateAvailable.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleUpdateAvailable.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleUpdateAvailable.ShowGlow = False
        ToggleUpdateAvailable.Size = New Size(48, 24)
        ToggleUpdateAvailable.TabIndex = 3
        ToggleUpdateAvailable.Text = "ToggleSwitch"
        ' 
        ' Desc_VersionLatest
        ' 
        Desc_VersionLatest.AutoSize = True
        Desc_VersionLatest.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_VersionLatest.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_VersionLatest.ForeColor = Color.White
        Desc_VersionLatest.Location = New Point(78, 102)
        Desc_VersionLatest.Name = "Desc_VersionLatest"
        Desc_VersionLatest.Size = New Size(218, 21)
        Desc_VersionLatest.TabIndex = 4
        Desc_VersionLatest.Text = "Already on the latest version"
        ' 
        ' ToggleVersionLatest
        ' 
        ToggleVersionLatest.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleVersionLatest.ImeMode = ImeMode.Off
        ToggleVersionLatest.IsOn = False
        ToggleVersionLatest.Location = New Point(20, 103)
        ToggleVersionLatest.Name = "ToggleVersionLatest"
        ToggleVersionLatest.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleVersionLatest.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleVersionLatest.ShowGlow = False
        ToggleVersionLatest.Size = New Size(48, 24)
        ToggleVersionLatest.TabIndex = 5
        ToggleVersionLatest.Text = "ToggleSwitch"
        ' 
        ' Desc_UpdateError
        ' 
        Desc_UpdateError.AutoSize = True
        Desc_UpdateError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_UpdateError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_UpdateError.ForeColor = Color.White
        Desc_UpdateError.Location = New Point(78, 140)
        Desc_UpdateError.Name = "Desc_UpdateError"
        Desc_UpdateError.Size = New Size(151, 21)
        Desc_UpdateError.TabIndex = 6
        Desc_UpdateError.Text = "Update check error"
        ' 
        ' ToggleUpdateError
        ' 
        ToggleUpdateError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleUpdateError.ImeMode = ImeMode.Off
        ToggleUpdateError.IsOn = False
        ToggleUpdateError.Location = New Point(20, 141)
        ToggleUpdateError.Name = "ToggleUpdateError"
        ToggleUpdateError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleUpdateError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleUpdateError.ShowGlow = False
        ToggleUpdateError.Size = New Size(48, 24)
        ToggleUpdateError.TabIndex = 7
        ToggleUpdateError.Text = "ToggleSwitch"
        ' 
        ' Card_Errors
        ' 
        Card_Errors.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_Errors.Controls.Add(Accent_Errors)
        Card_Errors.Controls.Add(Header_Errors)
        Card_Errors.Controls.Add(Desc_AccountConfirmError)
        Card_Errors.Controls.Add(ToggleAccountConfirmError)
        Card_Errors.Controls.Add(Desc_ExtensionNotFound)
        Card_Errors.Controls.Add(ToggleExtensionNotFound)
        Card_Errors.Controls.Add(Desc_FeatureNotReady)
        Card_Errors.Controls.Add(ToggleFeatureNotReady)
        Card_Errors.Controls.Add(Desc_GpuRequired)
        Card_Errors.Controls.Add(ToggleGpuRequired)
        Card_Errors.Controls.Add(Desc_EngineNotRunning)
        Card_Errors.Controls.Add(ToggleEngineNotRunning)
        Card_Errors.Controls.Add(Desc_EngineUIInUse)
        Card_Errors.Controls.Add(ToggleEngineUIInUse)
        Card_Errors.Controls.Add(Desc_ErrorResolution)
        Card_Errors.Controls.Add(ToggleErrorResolution)
        Card_Errors.Controls.Add(Desc_DesktopCaptureDisabled)
        Card_Errors.Controls.Add(ToggleDesktopCaptureDisabled)
        Card_Errors.Location = New Point(20, 698)
        Card_Errors.Name = "Card_Errors"
        Card_Errors.Size = New Size(1349, 384)
        Card_Errors.TabIndex = 16
        ' 
        ' Accent_Errors
        ' 
        Accent_Errors.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_Errors.Location = New Point(20, 22)
        Accent_Errors.Name = "Accent_Errors"
        Accent_Errors.Size = New Size(4, 20)
        Accent_Errors.TabIndex = 0
        ' 
        ' Header_Errors
        ' 
        Header_Errors.AutoSize = True
        Header_Errors.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_Errors.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_Errors.ForeColor = Color.White
        Header_Errors.Location = New Point(34, 18)
        Header_Errors.Name = "Header_Errors"
        Header_Errors.Size = New Size(189, 28)
        Header_Errors.TabIndex = 1
        Header_Errors.Text = "ERRORS & FEEDBACK"
        ' 
        ' Desc_AccountConfirmError
        ' 
        Desc_AccountConfirmError.AutoSize = True
        Desc_AccountConfirmError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_AccountConfirmError.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_AccountConfirmError.ForeColor = Color.White
        Desc_AccountConfirmError.Location = New Point(78, 64)
        Desc_AccountConfirmError.Name = "Desc_AccountConfirmError"
        Desc_AccountConfirmError.Size = New Size(210, 21)
        Desc_AccountConfirmError.TabIndex = 2
        Desc_AccountConfirmError.Text = "Account confirmation error"
        ' 
        ' ToggleAccountConfirmError
        ' 
        ToggleAccountConfirmError.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleAccountConfirmError.ImeMode = ImeMode.Off
        ToggleAccountConfirmError.IsOn = False
        ToggleAccountConfirmError.Location = New Point(20, 65)
        ToggleAccountConfirmError.Name = "ToggleAccountConfirmError"
        ToggleAccountConfirmError.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleAccountConfirmError.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleAccountConfirmError.ShowGlow = False
        ToggleAccountConfirmError.Size = New Size(48, 24)
        ToggleAccountConfirmError.TabIndex = 3
        ToggleAccountConfirmError.Text = "ToggleSwitch"
        ' 
        ' Desc_ExtensionNotFound
        ' 
        Desc_ExtensionNotFound.AutoSize = True
        Desc_ExtensionNotFound.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ExtensionNotFound.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ExtensionNotFound.ForeColor = Color.White
        Desc_ExtensionNotFound.Location = New Point(78, 102)
        Desc_ExtensionNotFound.Name = "Desc_ExtensionNotFound"
        Desc_ExtensionNotFound.Size = New Size(222, 21)
        Desc_ExtensionNotFound.TabIndex = 4
        Desc_ExtensionNotFound.Text = "Browser extension not found"
        ' 
        ' ToggleExtensionNotFound
        ' 
        ToggleExtensionNotFound.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleExtensionNotFound.ImeMode = ImeMode.Off
        ToggleExtensionNotFound.IsOn = False
        ToggleExtensionNotFound.Location = New Point(20, 103)
        ToggleExtensionNotFound.Name = "ToggleExtensionNotFound"
        ToggleExtensionNotFound.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleExtensionNotFound.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleExtensionNotFound.ShowGlow = False
        ToggleExtensionNotFound.Size = New Size(48, 24)
        ToggleExtensionNotFound.TabIndex = 5
        ToggleExtensionNotFound.Text = "ToggleSwitch"
        ' 
        ' Desc_FeatureNotReady
        ' 
        Desc_FeatureNotReady.AutoSize = True
        Desc_FeatureNotReady.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_FeatureNotReady.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_FeatureNotReady.ForeColor = Color.White
        Desc_FeatureNotReady.Location = New Point(78, 140)
        Desc_FeatureNotReady.Name = "Desc_FeatureNotReady"
        Desc_FeatureNotReady.Size = New Size(139, 21)
        Desc_FeatureNotReady.TabIndex = 6
        Desc_FeatureNotReady.Text = "Feature not ready"
        ' 
        ' ToggleFeatureNotReady
        ' 
        ToggleFeatureNotReady.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleFeatureNotReady.ImeMode = ImeMode.Off
        ToggleFeatureNotReady.IsOn = False
        ToggleFeatureNotReady.Location = New Point(20, 141)
        ToggleFeatureNotReady.Name = "ToggleFeatureNotReady"
        ToggleFeatureNotReady.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleFeatureNotReady.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleFeatureNotReady.ShowGlow = False
        ToggleFeatureNotReady.Size = New Size(48, 24)
        ToggleFeatureNotReady.TabIndex = 7
        ToggleFeatureNotReady.Text = "ToggleSwitch"
        ' 
        ' Desc_GpuRequired
        ' 
        Desc_GpuRequired.AutoSize = True
        Desc_GpuRequired.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_GpuRequired.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_GpuRequired.ForeColor = Color.White
        Desc_GpuRequired.Location = New Point(78, 178)
        Desc_GpuRequired.Name = "Desc_GpuRequired"
        Desc_GpuRequired.Size = New Size(166, 21)
        Desc_GpuRequired.TabIndex = 8
        Desc_GpuRequired.Text = "NVIDIA GPU required"
        ' 
        ' ToggleGpuRequired
        ' 
        ToggleGpuRequired.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleGpuRequired.ImeMode = ImeMode.Off
        ToggleGpuRequired.IsOn = False
        ToggleGpuRequired.Location = New Point(20, 179)
        ToggleGpuRequired.Name = "ToggleGpuRequired"
        ToggleGpuRequired.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleGpuRequired.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleGpuRequired.ShowGlow = False
        ToggleGpuRequired.Size = New Size(48, 24)
        ToggleGpuRequired.TabIndex = 9
        ToggleGpuRequired.Text = "ToggleSwitch"
        ' 
        ' Desc_EngineNotRunning
        ' 
        Desc_EngineNotRunning.AutoSize = True
        Desc_EngineNotRunning.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_EngineNotRunning.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_EngineNotRunning.ForeColor = Color.White
        Desc_EngineNotRunning.Location = New Point(938, 64)
        Desc_EngineNotRunning.Name = "Desc_EngineNotRunning"
        Desc_EngineNotRunning.Size = New Size(148, 21)
        Desc_EngineNotRunning.TabIndex = 10
        Desc_EngineNotRunning.Text = "Engine not running"
        ' 
        ' ToggleEngineNotRunning
        ' 
        ToggleEngineNotRunning.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleEngineNotRunning.ImeMode = ImeMode.Off
        ToggleEngineNotRunning.IsOn = False
        ToggleEngineNotRunning.Location = New Point(880, 65)
        ToggleEngineNotRunning.Name = "ToggleEngineNotRunning"
        ToggleEngineNotRunning.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleEngineNotRunning.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleEngineNotRunning.ShowGlow = False
        ToggleEngineNotRunning.Size = New Size(48, 24)
        ToggleEngineNotRunning.TabIndex = 11
        ToggleEngineNotRunning.Text = "ToggleSwitch"
        ' 
        ' Desc_EngineUIInUse
        ' 
        Desc_EngineUIInUse.AutoSize = True
        Desc_EngineUIInUse.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_EngineUIInUse.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_EngineUIInUse.ForeColor = Color.White
        Desc_EngineUIInUse.Location = New Point(938, 102)
        Desc_EngineUIInUse.Name = "Desc_EngineUIInUse"
        Desc_EngineUIInUse.Size = New Size(182, 21)
        Desc_EngineUIInUse.TabIndex = 12
        Desc_EngineUIInUse.Text = "Engine UI already in use"
        ' 
        ' ToggleEngineUIInUse
        ' 
        ToggleEngineUIInUse.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleEngineUIInUse.ImeMode = ImeMode.Off
        ToggleEngineUIInUse.IsOn = False
        ToggleEngineUIInUse.Location = New Point(880, 103)
        ToggleEngineUIInUse.Name = "ToggleEngineUIInUse"
        ToggleEngineUIInUse.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleEngineUIInUse.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleEngineUIInUse.ShowGlow = False
        ToggleEngineUIInUse.Size = New Size(48, 24)
        ToggleEngineUIInUse.TabIndex = 13
        ToggleEngineUIInUse.Text = "ToggleSwitch"
        ' 
        ' Desc_ErrorResolution
        ' 
        Desc_ErrorResolution.AutoSize = True
        Desc_ErrorResolution.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ErrorResolution.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ErrorResolution.ForeColor = Color.White
        Desc_ErrorResolution.Location = New Point(938, 140)
        Desc_ErrorResolution.Name = "Desc_ErrorResolution"
        Desc_ErrorResolution.Size = New Size(129, 21)
        Desc_ErrorResolution.TabIndex = 14
        Desc_ErrorResolution.Text = "Resolution error"
        ' 
        ' ToggleErrorResolution
        ' 
        ToggleErrorResolution.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleErrorResolution.ImeMode = ImeMode.Off
        ToggleErrorResolution.IsOn = False
        ToggleErrorResolution.Location = New Point(880, 141)
        ToggleErrorResolution.Name = "ToggleErrorResolution"
        ToggleErrorResolution.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleErrorResolution.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleErrorResolution.ShowGlow = False
        ToggleErrorResolution.Size = New Size(48, 24)
        ToggleErrorResolution.TabIndex = 15
        ToggleErrorResolution.Text = "ToggleSwitch"
        ' 
        ' Desc_DesktopCaptureDisabled
        ' 
        Desc_DesktopCaptureDisabled.AutoSize = True
        Desc_DesktopCaptureDisabled.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_DesktopCaptureDisabled.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_DesktopCaptureDisabled.ForeColor = Color.White
        Desc_DesktopCaptureDisabled.Location = New Point(938, 178)
        Desc_DesktopCaptureDisabled.Name = "Desc_DesktopCaptureDisabled"
        Desc_DesktopCaptureDisabled.Size = New Size(197, 21)
        Desc_DesktopCaptureDisabled.TabIndex = 16
        Desc_DesktopCaptureDisabled.Text = "Desktop capture disabled"
        ' 
        ' ToggleDesktopCaptureDisabled
        ' 
        ToggleDesktopCaptureDisabled.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleDesktopCaptureDisabled.ImeMode = ImeMode.Off
        ToggleDesktopCaptureDisabled.IsOn = False
        ToggleDesktopCaptureDisabled.Location = New Point(880, 179)
        ToggleDesktopCaptureDisabled.Name = "ToggleDesktopCaptureDisabled"
        ToggleDesktopCaptureDisabled.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleDesktopCaptureDisabled.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleDesktopCaptureDisabled.ShowGlow = False
        ToggleDesktopCaptureDisabled.Size = New Size(48, 24)
        ToggleDesktopCaptureDisabled.TabIndex = 17
        ToggleDesktopCaptureDisabled.Text = "ToggleSwitch"
        ' 
        ' Card_ToastSlots
        ' 
        Card_ToastSlots.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_ToastSlots.Controls.Add(Accent_ToastSlots)
        Card_ToastSlots.Controls.Add(Header_ToastSlots)
        Card_ToastSlots.Controls.Add(ToggleToastSlot2)
        Card_ToastSlots.Controls.Add(Desc_ToastSlots)
        Card_ToastSlots.Controls.Add(ToggleToastSlot3)
        Card_ToastSlots.Controls.Add(Desc_ToastSlots3)
        Card_ToastSlots.Controls.Add(Desc_ToastSlots_SUB)
        Card_ToastSlots.Controls.Add(Label9)
        Card_ToastSlots.Location = New Point(20, 1098)
        Card_ToastSlots.Name = "Card_ToastSlots"
        Card_ToastSlots.Size = New Size(848, 200)
        Card_ToastSlots.TabIndex = 17
        ' 
        ' Accent_ToastSlots
        ' 
        Accent_ToastSlots.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_ToastSlots.Location = New Point(20, 22)
        Accent_ToastSlots.Name = "Accent_ToastSlots"
        Accent_ToastSlots.Size = New Size(4, 20)
        Accent_ToastSlots.TabIndex = 0
        ' 
        ' Header_ToastSlots
        ' 
        Header_ToastSlots.AutoSize = True
        Header_ToastSlots.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_ToastSlots.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_ToastSlots.ForeColor = Color.White
        Header_ToastSlots.Location = New Point(34, 18)
        Header_ToastSlots.Name = "Header_ToastSlots"
        Header_ToastSlots.Size = New Size(148, 28)
        Header_ToastSlots.TabIndex = 1
        Header_ToastSlots.Text = "TOAST SLOTS"
        ' 
        ' ToggleToastSlot2
        ' 
        ToggleToastSlot2.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleToastSlot2.ForeColor = Color.Aquamarine
        ToggleToastSlot2.ImeMode = ImeMode.Off
        ToggleToastSlot2.IsOn = False
        ToggleToastSlot2.Location = New Point(20, 65)
        ToggleToastSlot2.Name = "ToggleToastSlot2"
        ToggleToastSlot2.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleToastSlot2.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleToastSlot2.ShowGlow = False
        ToggleToastSlot2.Size = New Size(48, 24)
        ToggleToastSlot2.TabIndex = 2
        ' 
        ' Desc_ToastSlots
        ' 
        Desc_ToastSlots.AutoSize = True
        Desc_ToastSlots.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ToastSlots.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ToastSlots.ForeColor = Color.White
        Desc_ToastSlots.Location = New Point(78, 64)
        Desc_ToastSlots.Name = "Desc_ToastSlots"
        Desc_ToastSlots.Size = New Size(223, 21)
        Desc_ToastSlots.TabIndex = 3
        Desc_ToastSlots.Text = "Use a second toast slot"
        ' 
        ' ToggleToastSlot3
        ' 
        ToggleToastSlot3.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ToggleToastSlot3.ForeColor = Color.Aquamarine
        ToggleToastSlot3.ImeMode = ImeMode.Off
        ToggleToastSlot3.IsOn = False
        ToggleToastSlot3.Location = New Point(354, 65)
        ToggleToastSlot3.Name = "ToggleToastSlot3"
        ToggleToastSlot3.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ToggleToastSlot3.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ToggleToastSlot3.ShowGlow = False
        ToggleToastSlot3.Size = New Size(48, 24)
        ToggleToastSlot3.TabIndex = 4
        ' 
        ' Desc_ToastSlots3
        ' 
        Desc_ToastSlots3.AutoSize = True
        Desc_ToastSlots3.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ToastSlots3.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ToastSlots3.ForeColor = Color.White
        Desc_ToastSlots3.Location = New Point(408, 64)
        Desc_ToastSlots3.Name = "Desc_ToastSlots3"
        Desc_ToastSlots3.Size = New Size(195, 21)
        Desc_ToastSlots3.TabIndex = 5
        Desc_ToastSlots3.Text = "Use a third toast slot"
        ' 
        ' Desc_ToastSlots_SUB
        ' 
        Desc_ToastSlots_SUB.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_ToastSlots_SUB.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_ToastSlots_SUB.ForeColor = Color.White
        Desc_ToastSlots_SUB.Location = New Point(74, 128)
        Desc_ToastSlots_SUB.Name = "Desc_ToastSlots_SUB"
        Desc_ToastSlots_SUB.Size = New Size(700, 32)
        Desc_ToastSlots_SUB.TabIndex = 6
        Desc_ToastSlots_SUB.Text = "A new notification enters a free slot instead of replacing the showing one. Turn the second off to route every toast through the main slot only."
        Desc_ToastSlots_SUB.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label9
        ' 
        Label9.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Label9.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label9.ForeColor = Color.White
        Label9.Location = New Point(20, 96)
        Label9.Name = "Label9"
        Label9.Size = New Size(39, 91)
        Label9.TabIndex = 7
        Label9.Text = ""
        Label9.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Card_OBS
        ' 
        Card_OBS.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Card_OBS.Controls.Add(Accent_OBS)
        Card_OBS.Controls.Add(Header_OBS)
        Card_OBS.Controls.Add(ObsEnabledToggle)
        Card_OBS.Controls.Add(Desc_OBS)
        Card_OBS.Controls.Add(Panel_OBS)
        Card_OBS.Location = New Point(20, 1314)
        Card_OBS.Name = "Card_OBS"
        Card_OBS.Size = New Size(848, 330)
        Card_OBS.TabIndex = 18
        ' 
        ' Accent_OBS
        ' 
        Accent_OBS.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        Accent_OBS.Location = New Point(20, 22)
        Accent_OBS.Name = "Accent_OBS"
        Accent_OBS.Size = New Size(4, 20)
        Accent_OBS.TabIndex = 0
        ' 
        ' Header_OBS
        ' 
        Header_OBS.AutoSize = True
        Header_OBS.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Header_OBS.Font = New Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Header_OBS.ForeColor = Color.White
        Header_OBS.Location = New Point(34, 18)
        Header_OBS.Name = "Header_OBS"
        Header_OBS.Size = New Size(389, 28)
        Header_OBS.TabIndex = 1
        Header_OBS.Text = "OBS Studio WebSocket Integration - Beta"
        ' 
        ' ObsEnabledToggle
        ' 
        ObsEnabledToggle.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        ObsEnabledToggle.ImeMode = ImeMode.Off
        ObsEnabledToggle.IsOn = False
        ObsEnabledToggle.Location = New Point(20, 65)
        ObsEnabledToggle.Name = "ObsEnabledToggle"
        ObsEnabledToggle.OffColor = Color.FromArgb(CByte(60), CByte(63), CByte(67))
        ObsEnabledToggle.OnColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        ObsEnabledToggle.ShowGlow = False
        ObsEnabledToggle.Size = New Size(48, 24)
        ObsEnabledToggle.TabIndex = 2
        ObsEnabledToggle.Text = "ToggleSwitch"
        ' 
        ' Desc_OBS
        ' 
        Desc_OBS.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Desc_OBS.Font = New Font("Segoe UI Semibold", 11.8F)
        Desc_OBS.ForeColor = Color.White
        Desc_OBS.Location = New Point(78, 60)
        Desc_OBS.Name = "Desc_OBS"
        Desc_OBS.Size = New Size(640, 40)
        Desc_OBS.TabIndex = 3
        Desc_OBS.Text = "Allow Notifier to connect to the OBS Studio WebSocket and display notifications based on OBS Studio states or events."
        Desc_OBS.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Panel_OBS
        ' 
        Panel_OBS.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Panel_OBS.Controls.Add(Label6)
        Panel_OBS.Controls.Add(PORT_BOX)
        Panel_OBS.Controls.Add(Label7)
        Panel_OBS.Controls.Add(KEY_BOX)
        Panel_OBS.Controls.Add(Label8)
        Panel_OBS.Controls.Add(HOST_BOX)
        Panel_OBS.Location = New Point(20, 116)
        Panel_OBS.Name = "Panel_OBS"
        Panel_OBS.Size = New Size(774, 188)
        Panel_OBS.TabIndex = 4
        ' 
        ' Label6
        ' 
        Label6.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Label6.Font = New Font("Segoe UI Semibold", 11.8F)
        Label6.ForeColor = Color.White
        Label6.Location = New Point(19, 17)
        Label6.Name = "Label6"
        Label6.Size = New Size(89, 27)
        Label6.TabIndex = 0
        Label6.Text = "PORT"
        Label6.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' PORT_BOX
        ' 
        PORT_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        PORT_BOX.BorderStyle = BorderStyle.None
        PORT_BOX.Font = New Font("nvgcshare", 20F)
        PORT_BOX.ForeColor = Color.White
        PORT_BOX.Location = New Point(19, 47)
        PORT_BOX.Multiline = True
        PORT_BOX.Name = "PORT_BOX"
        PORT_BOX.Size = New Size(96, 34)
        PORT_BOX.TabIndex = 1
        PORT_BOX.Text = "Port"
        PORT_BOX.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label7
        ' 
        Label7.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Label7.Font = New Font("Segoe UI Semibold", 11.8F)
        Label7.ForeColor = Color.White
        Label7.Location = New Point(121, 17)
        Label7.Name = "Label7"
        Label7.Size = New Size(116, 27)
        Label7.TabIndex = 2
        Label7.Text = "Key/Password"
        Label7.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' KEY_BOX
        ' 
        KEY_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        KEY_BOX.BorderStyle = BorderStyle.None
        KEY_BOX.Font = New Font("nvgcshare", 20F)
        KEY_BOX.ForeColor = Color.White
        KEY_BOX.Location = New Point(121, 47)
        KEY_BOX.Multiline = True
        KEY_BOX.Name = "KEY_BOX"
        KEY_BOX.Size = New Size(633, 34)
        KEY_BOX.TabIndex = 3
        KEY_BOX.Text = "Key"
        KEY_BOX.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label8
        ' 
        Label8.BackColor = Color.FromArgb(CByte(46), CByte(53), CByte(59))
        Label8.Font = New Font("Segoe UI Semibold", 11.8F)
        Label8.ForeColor = Color.White
        Label8.Location = New Point(19, 84)
        Label8.Name = "Label8"
        Label8.Size = New Size(89, 27)
        Label8.TabIndex = 4
        Label8.Text = "HOST"
        Label8.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' HOST_BOX
        ' 
        HOST_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        HOST_BOX.BorderStyle = BorderStyle.None
        HOST_BOX.Font = New Font("nvgcshare", 20F)
        HOST_BOX.ForeColor = Color.White
        HOST_BOX.Location = New Point(19, 114)
        HOST_BOX.Multiline = True
        HOST_BOX.Name = "HOST_BOX"
        HOST_BOX.Size = New Size(218, 34)
        HOST_BOX.TabIndex = 5
        HOST_BOX.Text = "HOST"
        HOST_BOX.TextAlign = HorizontalAlignment.Center
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
        ' Base_Notifications
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Red
        ClientSize = New Size(1920, 1080)
        Controls.Add(BT_Back)
        Controls.Add(Dim_2)
        Controls.Add(BT_DisableAll)
        Controls.Add(BT_EnableAll)
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
        Card_Recording.ResumeLayout(False)
        Card_Recording.PerformLayout()
        Card_Screenshots.ResumeLayout(False)
        Card_Screenshots.PerformLayout()
        Card_ShareOverlay.ResumeLayout(False)
        Card_ShareOverlay.PerformLayout()
        Card_InstantReplay.ResumeLayout(False)
        Card_InstantReplay.PerformLayout()
        Card_SystemMonitor.ResumeLayout(False)
        Card_SystemMonitor.PerformLayout()
        Card_Updates.ResumeLayout(False)
        Card_Updates.PerformLayout()
        Card_Errors.ResumeLayout(False)
        Card_Errors.PerformLayout()
        Card_ToastSlots.ResumeLayout(False)
        Card_ToastSlots.PerformLayout()
        Card_OBS.ResumeLayout(False)
        Card_OBS.PerformLayout()
        Panel_OBS.ResumeLayout(False)
        Panel_OBS.PerformLayout()
        CType(Menu_Top_Dim, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_Top, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_1, ComponentModel.ISupportInitialize).EndInit()
        CType(Dim_2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Menu_Settings As Panel
    Friend WithEvents Menu_Text As Label
    Friend WithEvents Menu_SubText As Label
    Friend WithEvents BT_EnableAll As Label
    Friend WithEvents BT_DisableAll As Label
    Friend WithEvents BT_Back As Label
    Friend WithEvents Menu_Top_Dim As PictureBox
    Friend WithEvents Dim_Top As PictureBox
    Friend WithEvents Dim_1 As PictureBox
    Friend WithEvents Dim_2 As PictureBox
    Friend WithEvents Card_Recording As Panel
    Friend WithEvents Accent_Recording As Panel
    Friend WithEvents Header_Recording As Label
    Friend WithEvents Desc_RecordingStarted As Label
    Friend WithEvents ToggleRecordingStarted As ToggleSwitch
    Friend WithEvents Desc_RecordingSaved As Label
    Friend WithEvents ToggleRecordingSaved As ToggleSwitch
    Friend WithEvents Desc_RecordingError As Label
    Friend WithEvents ToggleRecordingError As ToggleSwitch
    Friend WithEvents Card_Screenshots As Panel
    Friend WithEvents Accent_Screenshots As Panel
    Friend WithEvents Header_Screenshots As Label
    Friend WithEvents Desc_ScreenshotSaved As Label
    Friend WithEvents ToggleScreenshotSaved As ToggleSwitch
    Friend WithEvents Desc_ValidSavePath As Label
    Friend WithEvents ToggleValidSavePath As ToggleSwitch
    Friend WithEvents Card_ShareOverlay As Panel
    Friend WithEvents Accent_ShareOverlay As Panel
    Friend WithEvents Header_ShareOverlay As Label
    Friend WithEvents Desc_OpenShare As Label
    Friend WithEvents ToggleOpenShare As ToggleSwitch
    Friend WithEvents Card_InstantReplay As Panel
    Friend WithEvents Accent_InstantReplay As Panel
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
    Friend WithEvents Card_SystemMonitor As Panel
    Friend WithEvents Accent_SystemMonitor As Panel
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
    Friend WithEvents Card_Updates As Panel
    Friend WithEvents Accent_Updates As Panel
    Friend WithEvents Header_Updates As Label
    Friend WithEvents Desc_UpdateAvailable As Label
    Friend WithEvents ToggleUpdateAvailable As ToggleSwitch
    Friend WithEvents Desc_VersionLatest As Label
    Friend WithEvents ToggleVersionLatest As ToggleSwitch
    Friend WithEvents Desc_UpdateError As Label
    Friend WithEvents ToggleUpdateError As ToggleSwitch
    Friend WithEvents Card_Errors As Panel
    Friend WithEvents Accent_Errors As Panel
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
    Friend WithEvents Card_ToastSlots As Panel
    Friend WithEvents Accent_ToastSlots As Panel
    Friend WithEvents Header_ToastSlots As Label
    Friend WithEvents ToggleToastSlot2 As ToggleSwitch
    Friend WithEvents Desc_ToastSlots As Label
    Friend WithEvents Desc_ToastSlots_SUB As Label
    Friend WithEvents ToggleToastSlot3 As ToggleSwitch
    Friend WithEvents Desc_ToastSlots3 As Label
    Friend WithEvents Card_OBS As Panel
    Friend WithEvents Accent_OBS As Panel
    Friend WithEvents Header_OBS As Label
    Friend WithEvents ObsEnabledToggle As ToggleSwitch
    Friend WithEvents Desc_OBS As Label
    Friend WithEvents Panel_OBS As Panel
    Friend WithEvents Label6 As Label
    Friend WithEvents PORT_BOX As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents KEY_BOX As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents HOST_BOX As TextBox
    Friend WithEvents Label9 As Label
End Class
