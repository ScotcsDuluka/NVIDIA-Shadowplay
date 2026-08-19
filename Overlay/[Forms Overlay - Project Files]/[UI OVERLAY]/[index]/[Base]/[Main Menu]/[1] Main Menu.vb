Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.IO
Imports System.Net.Http.Headers
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports Windows.Graphics.Capture
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.DirectX.Direct3D11
Imports Windows.Media.Devices
Imports WinRT.Interop
Partial Public Class Base


    Public Sub PauseHotkeys()
        If _hotkeyService IsNot Nothing Then
            _hotkeyService.UnregisterAll()
        End If
    End Sub

    Public Sub ResumeHotkeys()
        ReloadHotkeys()
    End Sub

#Region "NATIVE METHODS & STRUCTURES"

    Public clickThrough As Boolean = False

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WinAPI.WM_HOTKEY Then
            If _hotkeyService IsNot Nothing Then
                _hotkeyService.ProcessHotkey(m.WParam.ToInt32())
            End If
            Return
        End If

        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If clickThrough AndAlso m.Msg = WM_NCHITTEST Then
            m.Result = CType(HTTRANSPARENT, IntPtr)
            Exit Sub
        End If

        MyBase.WndProc(m)
    End Sub


    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function



    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function CreateProcess(
        lpApplicationName As String,
        lpCommandLine As String,
        lpProcessAttributes As IntPtr,
        lpThreadAttributes As IntPtr,
        bInheritHandles As Boolean,
        dwCreationFlags As UInteger,
        lpEnvironment As IntPtr,
        lpCurrentDirectory As String,
        ByRef lpStartupInfo As StartupInfo,
        ByRef lpProcessInformation As ProcessInformation) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure StartupInfo
        Public cb As UInteger
        Public lpReserved As String
        Public lpDesktop As String
        Public lpTitle As String
        Public dwX As UInteger
        Public dwY As UInteger
        Public dwXSize As UInteger
        Public dwYSize As UInteger
        Public dwFlags As UInteger
        Public wShowWindow As UShort
        Public cbReserved2 As UShort
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=80)>
        Public lpReserved2 As Byte()
        Public hStdInput As IntPtr
        Public hStdOutput As IntPtr
        Public hStdError As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure ProcessInformation
        Public hProcess As IntPtr
        Public hThread As IntPtr
        Public dwProcessId As UInteger
        Public dwThreadId As UInteger
    End Structure

#End Region

#Region "============================================================================ CONSTANTS & FIELDS"

    Private Const AppName As String = "NVIDIA Shadowplay™"
    Private ReadOnly greenColor As System.Drawing.Color = ColorTranslator.FromHtml("#76B900")

    Private Const DataDirectoryName As String = "NVIDIA_Shadowplay_Data"

    Private Const ReplayOnFile As String = "Replay/on"
    Private Const ReplayOffFile As String = "Replay/off"
    Private Const MicOnFile As String = "mic/mic_on"
    Private Const MicOffFile As String = "mic/mic_off"
    Private Const PrivacyFile As String = "privacy"

    Private isFunctionActive As Boolean = False
    Private isKeyPressed As Boolean = False

    Private isFunctionActive_F1 As Boolean = False
    Private isKeyPressed_F1 As Boolean = False

    Private isFunctionActive_replay As Boolean = False
    Private isKeyPressed_replay As Boolean = False

    Private isFunctionActive_replay_save As Boolean = False
    Private isKeyPressed_replay_save As Boolean = False

    Private isFunctionActive_record As Boolean = False
    Private isKeyPressed_record As Boolean = False

    Private isFunctionActive_p As Boolean = False
    Private isKeyPressed_p As Boolean = False

    Private isFunctionActive_f2 As Boolean = False
    Private isKeyPressed_f2 As Boolean = False

    Public isFunctionActive_f3 As Boolean = False
    Private isKeyPressed_f3 As Boolean = False

    Private isFunctionActive_f8 As Boolean = False
    Private isKeyPressed_f8 As Boolean = False

    Private isNotiOn As Boolean = False
    Private notifierShown As Boolean = False

    Private WithEvents _hotkeyService As HotkeyService

#End Region

    Private Sub SetGroupHoverEffect(hoverColor As Color, leaveColor As Color, ParamArray ctrls() As Control)
        For Each ctrl As Control In ctrls
            AddHandler ctrl.MouseEnter, Sub()
                                            For Each c As Control In ctrls
                                                c.BackColor = hoverColor
                                            Next
                                        End Sub
            AddHandler ctrl.MouseLeave, Sub()
                                            Dim mousePos As Point = Cursor.Position
                                            Dim stillOver As Boolean = False
                                            For Each c As Control In ctrls
                                                If c.RectangleToScreen(c.ClientRectangle).Contains(mousePos) Then
                                                    stillOver = True
                                                    Exit For
                                                End If
                                            Next
                                            If Not stillOver Then
                                                For Each c As Control In ctrls
                                                    c.BackColor = leaveColor
                                                Next
                                            End If
                                        End Sub
        Next
    End Sub

    Private Sub SetHoverEffect(ctrl As Control, hoverColor As Color, leaveColor As Color)
        AddHandler ctrl.MouseEnter, Sub() ctrl.BackColor = hoverColor
        AddHandler ctrl.MouseLeave, Sub() ctrl.BackColor = leaveColor
    End Sub

    Private ReadOnly HoverColorG As Color = Color.FromArgb(64, 64, 64)
    Private ReadOnly LeaveColorG As Color = Color.FromArgb(38, 43, 47)

    Private ReadOnly HoverColorGR As Color = Color.Green
    Private ReadOnly LeaveColorGR As Color = Color.FromArgb(118, 185, 0)

    Private ReadOnly HVDG As Color = Color.FromArgb(53, 55, 58)
    Private ReadOnly VDG As Color = Color.FromArgb(33, 35, 38)

    Private ReadOnly HVDGR As Color = Color.Green
    Private ReadOnly VDGR As Color = Color.FromArgb(118, 185, 0)

    Private Sub Sub_VDUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'VD — กลุ่ม FPS
        SetGroupHoverEffect(HVDG, VDG,
        Base_RecordingsSet.FPS_BOX,
        Base_RecordingsSet.fps_bg,
        Base_RecordingsSet.FPS_DROP)

        'VD — กลุ่ม Preset (P)
        SetGroupHoverEffect(HVDG, VDG,
        Base_RecordingsSet.P_BOX,
        Base_RecordingsSet.P_bg)

        'VD — กลุ่ม Resolution
        SetGroupHoverEffect(HVDG, VDG,
        Base_RecordingsSet.Resolution_bg,
        Base_RecordingsSet.Resolution_BOX,
        Base_RecordingsSet.Resolution_DROP)

        'VD — กลุ่ม Encoder
        SetGroupHoverEffect(HVDG, VDG,
        Base_RecordingsSet.cmbEncoder,
        Base_RecordingsSet.Encoder_DROP,
        Base_RecordingsSet.Encoder_bg)
    End Sub
    Private Sub MainUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'gallery
        SetHoverEffect(Base_Gallery.Saved_l10n, HoverColorGR, LeaveColorGR)
        SetHoverEffect(Base_Gallery.Openloaction_l10n, HoverColorG, LeaveColorG)

        '0
        SetHoverEffect(Base_Settings.SW_lang, HoverColorG, LeaveColorG)
        SetHoverEffect(Base_Settings.ch, HoverColorG, LeaveColorG)
        SetHoverEffect(Base_Settings.action_fn, HoverColorGR, LeaveColorGR)
        SetHoverEffect(Base_Settings.btnExportSettings, HoverColorG, LeaveColorG)
        SetHoverEffect(Base_Settings.btnImportSettings, HoverColorG, LeaveColorG)

        '1
        SetHoverEffect(Base_Connect.action_fn, HoverColorGR, LeaveColorGR)

        '2
        SetHoverEffect(Base_Overlay_Hub.action_fn, HoverColorGR, LeaveColorGR)

        '4
        SetHoverEffect(Base_KeySet.action_fn, HoverColorGR, LeaveColorGR)
        SetHoverEffect(Base_KeySet.Reset, HoverColorG, LeaveColorG)

        '5
        SetHoverEffect(Base_RecordingsSet.action_fn, HoverColorGR, LeaveColorGR)
        SetHoverEffect(Base_RecordingsSet.vdo_resetall, HoverColorG, LeaveColorG)

        '7
        SetHoverEffect(Base_Privacy_Control.action_fn, HoverColorGR, LeaveColorGR)
    End Sub

#Region "============================================================================ FORM LOAD & INITIALIZATION"
    Private SystemMonitor As New SystemMonitor()
    Private Sub LoadCurrentLanguage()
        Dim langFolder As String = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile As String = Path.Combine(langFolder, "current.txt")

        Dim currentLang As String = "en-US"
        If File.Exists(currentFile) Then currentLang = File.ReadAllText(currentFile).Trim()

        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        If Not File.Exists(langFile) Then langFile = Path.Combine(langFolder, "en-US.json")
        LangHelper.LoadLang(langFile)

        Base_Settings.SW_lang.Text = LangHelper.GetText("meta.languageName")
    End Sub

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000


    Private Declare Function SendMessage Lib "user32" (
    hWnd As IntPtr, Msg As Integer,
    wParam As IntPtr, lParam As IntPtr
) As IntPtr
    Private Const WM_SETREDRAW As Integer = &HB



    Private _delayTimers As System.Windows.Forms.Timer
    Private _bgInitDone As Boolean = False

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        HideFromAltTab()

        ' ===== เริ่มต้นที่เบาที่สุดก่อน (ไม่มี network) =====
        AppSettings.Initialize()
        LoadCurrentLanguage()
        MainSub_Load()
        LoadFilePath()
        CreateDataDirectories()
        LoadMicState()
        TIMESLOAD()
        Base_KeySet.InitKeyLabels()
        Base_KeySet.WireEvents()
        Base_KeySet.LoadHotkeyValues()

        ' ===== Hotkeys ทันที =====
        _hotkeyService = New HotkeyService()
        _hotkeyService.RegisterAll(Handle)
        tcp.Send("Hotkeys registered!")

        ' ===== SystemMonitor (เดิม delay 2 วิ ให้ลดเป็น 500ms) =====
        _delayTimers = New System.Windows.Forms.Timer
        _delayTimers.Interval = 500
        AddHandler _delayTimers.Tick, Sub()
                                          _delayTimers.Stop()
                                          'SystemMonitor.StartMonitoring()
                                      End Sub
        _delayTimers.Start()

        ' ===== UI พร้อมแล้ว — ส่งสัญญาณ Ready =====
        File.Create(Path.Combine(Application.StartupPath, "Ready")).Dispose()

        ' ===== ทำงานหนักใน background (ไม่ block UI) =====
        Task.Run(Async Function()
                     Try
                         ' 1) GitHub User (network)
                         Await AppSettings.Instance.LoadGitHubUser()
                         Me.BeginInvoke(Sub()
                                            Base_Connect.USERSNAME_TEXT.Text = AppSettings.Instance.GitHubUser.Username
                                        End Sub)

                         ' 2) Avatar (network + decode)
                         Await AppSettings.Instance.LoadGitHubAvatar(Base_Connect.Box_PNG)

                     Catch ex As Exception
                         Debug.WriteLine("[BG Init] " & ex.Message)
                     End Try

                     ' 3) Notifier (Process.Start) — หลัง network เสร็จ
                     Me.BeginInvoke(Sub()
                                        InitializeNotifierAPI()
                                        _bgInitDone = True
                                        Dim width As Integer = Screen.PrimaryScreen.Bounds.Width
                                        Dim height As Integer = Screen.PrimaryScreen.Bounds.Height
                                        If width >= 1680 AndAlso height >= 1050 Then
                                            ShowNotifier("notificationOpenShare")
                                        Else
                                            ShowNotifier("notificationErrorResolution")
                                        End If
                                    End Sub)
                 End Function)

#If DEBUG Then
        Dim overlayExists As Boolean = File.Exists(Path.Combine(Application.StartupPath, "Dev"))
        If overlayExists Then
            Debug_UI.Show()
        End If
#End If

    End Sub

    Private Async Function WaitForConnection(timeoutMs As Integer) As Task
        Dim tcs As New TaskCompletionSource(Of Boolean)()
        Dim handler As TcpClientHelper.OnMessageReceivedEventHandler = Nothing

        handler = Sub(msg)
                      If tcp.IsConnected Then
                          RemoveHandler tcp.OnMessageReceived, handler
                          tcs.TrySetResult(True)
                      End If
                  End Sub

        AddHandler tcp.OnMessageReceived, handler

        Using timeoutCts As New CancellationTokenSource(timeoutMs)
            Dim reg = timeoutCts.Token.Register(Sub()
                                                    RemoveHandler tcp.OnMessageReceived, handler
                                                    tcs.TrySetResult(False)
                                                End Sub)
            Await tcs.Task
        End Using
    End Function

    Private Sub TIMESLOAD()
        Load_App.Start()
        Privacy_control.Start()
    End Sub

    Private Sub MainSub_Load()
        Base_RecordingsSet.Opacity = 1
    End Sub

    Private Sub InitializeNotifierAPI()

        ' Start Notifier
        Try
            Dim exePath As String = Path.Combine(Application.StartupPath, "NVIDIA Notifier.exe")
            If Not File.Exists(exePath) Then
                MessageBox.Show(
                "NVIDIA Notifier Service Could Not Be Started!" & vbCrLf &
                "Please check if the file exists and you have sufficient permissions.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
                Exit Sub
            End If
            Process.Start(exePath)
        Catch ex As Exception
            MessageBox.Show("Failed to run NVIDIA Notifier.exe: " & ex.Message)

        End Try
    End Sub

    Public Sub ReloadHotkeys()
        If _hotkeyService Is Nothing Then Return
        _hotkeyService.UnregisterAll()
        _hotkeyService.RegisterAll(Handle)
    End Sub
#End Region

#Region "============================================================================ LOCALIZATION"
    Private Function L(key As String, ParamArray args() As String) As String
        Return LangHelper.GetText(key, args)
    End Function

    Private Function GetClampedReplayDuration() As Integer
        Dim savedSeconds As Integer = 60
        Try
            Dim settingValue As Integer = AppSettings.Instance.Recording.ReplayDuration
            savedSeconds = Math.Max(15, Math.Min(1200, settingValue))
        Catch
        End Try
        Return CInt(Math.Round(savedSeconds / 15.0) * 15)
    End Function
    Private Function GetClampedReplayDurationMinutes() As Integer
        Dim savedMinutes As Integer = 1 ' default 1 นาที
        Try
            Dim settingValue As Integer = AppSettings.Instance.Recording.ReplayDuration

            ' ถ้า setting เดิมยังเป็นวินาที → แปลงเป็นนาที
            savedMinutes = settingValue \ 60

            ' clamp 1 - 20 นาที (เท่ากับ 60 - 1200 วิ)
            savedMinutes = Math.Max(1, Math.Min(20, savedMinutes))
        Catch
        End Try

        ' ปัดเป็น step 1 นาที (หรือจะเปลี่ยนเป็น 5 นาที ก็แก้ตรงนี้)
        Return savedMinutes
    End Function
    Private Sub Lang_Tick(sender As Object, e As EventArgs) Handles Lang.Tick
        ' Base_Background_Top
        Base_Background_Top.Logo_text.Text = L("l10n.nvidiashadowplay")

        ' Base_Gallery
        With Base_Gallery
            .Gallery_l10n.Text = L("l10n.gallery")
            .LoactionSaved_l10n.Text = L("l10n.LocationSaved")
            .Saved_l10n.Text = L("l10n.done")
            .Openloaction_l10n.Text = L("l10n.openLocation")
            .Shortcut_l10n.Text = L("l10n.Shortcut")
            .Load_l10n.Text = L("l10n.Load")
            .Label3.Text = L("l10n.all")
            .text_sub.Text = L("l10n.gallerynotready")
        End With

        ' Base_Game_Filter
        Base_Game_Filter.Home_settings.Text = L("l10n.mods")


        ' Base_Privacy Control
        With Base_Privacy_Control
            .Label4.Text = L("l10n.privacyControl")
            .Label2.Text = L("l10n.settingsPrivacyDescribe")
            .action_fn.Text = L("l10n.back")
            .captrueblock.Text = L("l10n.settingsVideoCaptureDisable")
        End With
        CheckPrivacyControl()

        With Base_Overlay_Hub
            '.text_settings.Text = L("l10n.hudLayout")
            .action_fn.Text = L("l10n.back")
            .Label4.Text = L("l10n.overlays")
        End With

        ' Base_RecordingsSet
        With Base_RecordingsSet
            .text_settings.Text = L("l10n.recordings")
            .action_fn.Text = L("l10n.Saved")
            .Label4.Text = L("l10n.videoCapture")
            .quality_main.Text = L("l10n.preset")
            .Preset_NVIDIA.Text = L("l10n.nvidiaPreset")
            .Preset_Custom.Text = L("l10n.advanced")
            .Preset_My_Preset.Text = L("l10n.myPreset")
            .Label10.Text = L("l10n.low")
            .Label8.Text = L("l10n.medium")
            .Label6.Text = L("l10n.high")
            .ML_TEXT.Text = L("l10n.low")
            .MM_TEXT.Text = L("l10n.medium")
            .MH_TEXT.Text = L("l10n.high")
            .C_TEXT.Text = L("l10n.custom")
            .Recommended_TEXT.Text = L("l10n.pre_recommended")
            .Maximum_TEXT.Text = L("l10n.maximum")
            .Encoder_CODE.Text = L("l10n.codecEncoder")
            .Label12.Text = L("l10n.resolution")
            .Label13.Text = L("l10n.framerate")
            .vdo_resetall.Text = L("l10n.resetToDefaults")
            .captrueblock.Text = L("l10n.settingsVideoCaptureDisable")
            .warm_re.Text = L("l10n.captrueresolutionwarm")
            .custom_main.Text = L("l10n.adjust_value")
            .advanced_main.Text = L("l10n.advanced")

            ' ═══ Dynamic labels
            .UpdatePresetStatusLabel()    ' lblPresetStatus: "NVIDIA Medium — All settings locked"
            .UpdateBitrateRangeLabel()    ' lblBitrateRange: "Range: 3.0-50.0 Mbps..."
            .UpdateBitrateLabel()         ' lblBitrateValue + lblBitratePre: "Bitrate: 8000 kbps (8.0 Mbps)"
            .UpdateBufferLabel(GetClampedReplayDuration())  ' lbl_BufferDuration + lblReplaySize
            .UpdateEncoderInfo()          ' lblEncoderInfo: "NVIDIA NVENC - Best Performance"
            .UpdateCommandPreview()       ' prearg: ffmpeg command

            ' ═══ Resolution text
            If .Resolution_BOX IsNot Nothing Then
                Dim curRes As String = AppSettings.Instance.Recording.Preset
                ' Refresh resolution display with localized text
                .Resolution_BOX.Text = LangHelper.GetText("l10n.native", ._nativeResolutionWidth & "x" & ._nativeResolutionHeight)
            End If
        End With

        With Base_Settings
            .text_sub.Text = L("l10n.selectmenu")
            .action_fn.Text = L("l10n.done")
            .ch.Text = L("l10n.checkForUpdates")
        End With

        With Base_Connect
            .text_menu.Text = L("l10n.connect")
            .action_fn.Text = L("l10n.back")
        End With


        ' Base_KeySet
        With Base_KeySet
            .text_settings.Text = L("l10n.keyboardShortcuts")
            .action_fn.Text = L("l10n.done")
            .Reset.Text = L("l10n.resetAll")
        End With
        With Base_KeySet
            .lblCat_General.Text = L("l10n.general")
            .Desc_ToggleOverlay.Text = L("l10n.openShare")
            .Desc_Test.Text = L("l10n.testNotifier")
            .Desc_Empty.Text = ""

            .Desc_Screenshot.Text = L("l10n.saveScreenshot")
            .Desc_PhotosToggle.Text = L("l10n.openClosePhotoMode")
            .Desc_GameFilterToggle.Text = L("l10n.toggleMods")

            .Desc_ManualRecordToggle.Text = L("l10n.toggleRecording")
            .Desc_InstantReplayToggle.Text = L("l10n.toggleIR")
            .Desc_InstantReplaySave.Text = L("l10n.saveLastNMins", GetClampedReplayDurationMinutes)
            .Desc_BroadcastToggle.Text = L("l10n.toggleBroadcasting")
        End With

        ' Base Form Controls
        UpdateLocalizedTexts()
        RefreshRuntimeStatusTexts()
        ' FIX: Removed AppSettings.Instance.Save() from Lang_Tick.
        '      Localization refresh must NOT have a disk-write side-effect — config.json
        '      was being re-serialized on every language refresh even though no setting
        '      changed. The existing Base_FormClosing handler already saves on shutdown,
        '      and any code path that actually mutates a setting (e.g. ToggleRecording,
        '      mic_Click, Base_KeySet.SaveHotkeyValues) already calls Save() itself.
        Lang.Stop()
    End Sub

    Public Sub UpdateLocalizedTexts()
        Lang.Start()
        ' Base
        'Main-Menu

#Region "===========================Mode"

        Text_Mode1.Text = L("l10n.screenshots")
        Text_Mode2.Text = L("l10n.photos")
        Text_Mode3.Text = L("l10n.mods")

#End Region '===========================

#Region "===========================Captrue"

        '-Replay
        Replay_Text.Text = L("l10n.instantReplay") & " - BETA"
        Replay_Stats.Text = L("l10n.off")
        Menu_Replay_text.Text = L("l10n.instantReplayStart")
        Menu_Replay_save_text.Text = L("l10n.Saved")
        Menu_Replay_Sttings_text.Text = L("l10n.settings")
        '-Record
        Record_Text.Text = L("l10n.manualRecord")
        Record_Stats.Text = L("l10n.notRecording")
        Menu_Record_text.Text = L("l10n.start")
        Menu_Record_Sttings_text.Text = L("l10n.settings")
        '-Live
        Live_Text.Text = L("l10n.broadcastLive")
        Live_Stats.Text = L("l10n.NotReady")

#End Region '===========================

#Region "===========================Options"

        Share_Text.Text = L("l10n.upload")
        Gallery_Text.Text = L("l10n.gallery")
        Settings_Text.Text = L("l10n.settings")

#End Region

#Region "===========================Preferences"

        Settings_List_Text.Text = L("l10n.preferencesHome")
        Connect_TEXT.Text = L("l10n.connect")
        HUDLayout_TEXT.Text = L("l10n.hudLayout")
        Highlights_TEXT.Text = L("l10n.highlights")
        KeyboardShortcuts_TEXT.Text = L("l10n.keyboardShortcuts")
        VideoCapture_TEXT.Text = L("l10n.videoCapture")
        Audio_TEXT.Text = L("l10n.audio")
        Engine_TEXT.Text = L("l10n.engine")
        VideoCapture_TEXT_SUB.Text = L("l10n.videoCaptureText")
        Notifications_TEXT.Text = L("l10n.notifications")
        PrivacyControl_TEXT.Text = L("l10n.privacyControl")
        About_TEXT.Text = L("l10n.about")

#End Region '===========================

    End Sub

#End Region

#Region "============================================================================ FILE & DIRECTORY OPERATIONS"

    Private Sub LoadFilePath()
        Dim GalleryPath As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            "Shadowplay",
            "Gallery"
        )

        Base_Gallery.txtFilePath.Text = AppSettings.Instance.Paths.GalleryPath

        If String.IsNullOrEmpty(Base_Gallery.txtFilePath.Text) Then
            Base_Gallery.txtFilePath.Text = GalleryPath

            AppSettings.Instance.Paths.GalleryPath = GalleryPath
        End If

        Dim directoryPath As String = Base_Gallery.txtFilePath.Text
        If Not Directory.Exists(directoryPath) Then
            Directory.CreateDirectory(directoryPath)
        End If
    End Sub

    Private Sub CreateDataDirectories()
        Dim basePath As String = Path.Combine(Application.StartupPath, DataDirectoryName)
        Dim subdirectories As String() = {"Replay", "Record", "Live", "mic"}

        For Each subdir As String In subdirectories
            My.Computer.FileSystem.CreateDirectory(Path.Combine(basePath, subdir))
        Next
    End Sub

    Private Sub CheckPrivacyControl()
        Dim privacyPath As String = Path.Combine(Application.StartupPath, DataDirectoryName, PrivacyFile)

        Base_Privacy_Control.TogglePrivacy.IsOn = File.Exists(privacyPath)
    End Sub

#End Region

#Region "============================================================================ SCREEN CAPTURE"

    Private Sub CaptureScreen()
        If Not My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, DataDirectoryName, PrivacyFile)) Then
            ShowNotifier("notificationWarningDesktopCaptureDisabled")
            ShowMainPanel()
            For Each f In allForms
                If f IsNot Base_Settings Then f?.Hide()
            Next
            Base_Settings.Show()
            Base_Settings.Main_Menu_SET.Location = New Point(695, 160)
            Base_Background_Top.Bg_SET3.Visible = False
            ME_CLOSE_BG.Visible = False
            clickThrough = True
            Opacity = 1
            a_1.Visible = False : a_2.Visible = False : a_3.Visible = False
            Settings_List.Visible = True
            shadowplay.Visible = False
            Menu_Replay.Visible = False
            Menu_Record.Visible = False
            PrivacyOpen()
            Return
        End If

        Dim filePath As String = Base_Gallery.txtFilePath.Text
        If String.IsNullOrWhiteSpace(filePath) Then
            ShowNotifier("validsavepath")
            Return
        End If

        Try
            Using bmpScreenshot As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
                Using g As Graphics = Graphics.FromImage(bmpScreenshot)
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)
                End Using

                Dim fileName As String = Path.Combine(filePath, "Shadowplay Screenshot " & DateTime.Now.ToString("dd_MM_ss") & ".png")
                bmpScreenshot.Save(fileName, System.Drawing.Imaging.ImageFormat.Png)

                ShowNotifier(If(Directory.Exists(filePath), "notificationScreenshotSavedToGallery", "validsavepath"))
            End Using
        Catch ex As Exception
            MessageBox.Show("Failed to capture screen: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region "============================================================================ NOTIFIER SYSTEM"

    Public Sub ShowNotifier(message As String)
        tcp.Send("l10n." & message)
        Dim folderPath As String = Path.Combine(Application.StartupPath, DataDirectoryName)

        '  If Not Directory.Exists(folderPath) Then
        ' Directory.CreateDirectory(folderPath)
        '  End If

        ' Dim filePath As String = Path.Combine(folderPath, "l10n." & message)
        'Try
        'File.Create(filePath).Dispose()
        'Catch ex As UnauthorizedAccessException
        'End Try
    End Sub

#End Region

#Region "============================================================================ WINDOW MANAGEMENT"

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And (Not WS_EX_APPWINDOW))
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        AlignPanelToTop()
    End Sub

    Public Sub AlignPanelToTop()
        Dim marginTop As Integer = 160

        ' Base
        Settings_List.Location = New Point(80, 160)
        Base_Background_Top.Main_menu_list.Location = New Point((Me.ClientSize.Width - Base_Background_Top.Main_menu_list.Width) / 2, marginTop)
        shadowplay.Location = New Point((Me.ClientSize.Width - shadowplay.Width) / 2, marginTop)

        ' Gallery
        Base_Gallery.settings_1.Location = New Point((Me.ClientSize.Width - Base_Gallery.settings_1.Width) / 2, marginTop)

        'Settings
        Base_Privacy_Control.settings_1.Location = New Point(80, marginTop)
        Base_RecordingsSet.setret.Location = New Point(80, marginTop)
        Base_Overlay_Hub.settings_1.Location = New Point(80, marginTop)
        Base_KeySet.keyset.Location = New Point(80, marginTop)
    End Sub

#End Region

    Private Sub Base_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        AppSettings.Instance.Save()
        _hotkeyService?.UnregisterAll()
    End Sub

    Private Sub Menu_Record_Sttings_text_Click(sender As Object, e As EventArgs) Handles Menu_Record_Sttings_text.Click, Menu_Record_Box2.Click


        OpenRecordings()

    End Sub

    Private Sub Menu_Replay_Sttings_text_Click(sender As Object, e As EventArgs) Handles Menu_Replay_Sttings_text.Click, Menu_Replay_Box3.Click


        OpenRecordings()

    End Sub


    Private shas As Control()
    Private lastMode As String = ""

    Private Sub lastMode_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lastMode = "None"
        shas = {sha1, sha2, sha3, sha4}


        For Each s In shas
            If s IsNot Nothing Then
                s.Opacity = 0
                s.Show()
                s.Visible = False
            End If
        Next
    End Sub

    Public Sub ShadowLoad()
        If shas Is Nothing Then Return

        Dim currentMode As String = ""
        Dim newSize As Size = Size.Empty

        If Menu_Replay.Visible Then
            currentMode = "Replay"
        ElseIf Menu_Record.Visible Then
            currentMode = "Record"
        Else
            currentMode = "None"
        End If

        If currentMode <> lastMode Then
            lastMode = currentMode

            Dim target As Control = Nothing

            Select Case currentMode
                Case "None"
                    For Each s In shas
                        If s IsNot Nothing Then s.Hide()
                    Next
                    Base_Background_Top.b2_all.Visible = False
                    Base_Background_Top.b1_all.Visible = False
                    Exit Sub

                Case "Replay"
                    target = bg_action
                    newSize = If(ReplayValue, New Size(240, 373), New Size(240, 329))

                Case "Record"
                    target = a_2r
                    newSize = New Size(240, 329)
            End Select

            If target Is Nothing Then Return

            Dim screenPos As Point = shadowplay.PointToScreen(target.Location)

            Using g As Graphics = Me.CreateGraphics()
                Dim scale As Single = g.DpiX / 96.0F
                Dim posX As Integer = CInt(screenPos.X / scale)
                Dim posY As Integer = CInt(screenPos.Y / scale)

                For Each s In shas
                    If s Is Nothing Then Continue For
                    s.Location = New Point(posX, posY)
                    s.Size = newSize
                Next
            End Using

            Base_Background_Top.b2_all.Visible = (currentMode = "Record")
            Base_Background_Top.b1_all.Visible = (currentMode = "Replay")

            For Each s In shas
                If s Is Nothing Then Continue For
                s.Show()
                s.TopMost = False
                s.Opacity = 1
                s.HideFromAltTab()
            Next
        End If
    End Sub

    Private Sub Load_App_Tick(sender As Object, e As EventArgs) Handles Load_App.Tick
        Menu_Replay_key.Text = Base_KeySet.lbl_InstantReplayToggle.Text
        Menu_Replay_save_key.Text = Base_KeySet.lbl_InstantReplaySave.Text
        Menu_Record_key.Text = Base_KeySet.lbl_ManualRecordToggle.Text
        Key_Mode1.Text = Base_KeySet.lbl_Screenshot.Text
        Key_Mode2.Text = Base_KeySet.lbl_PhotosToggle.Text
        Key_Mode3.Text = Base_KeySet.lbl_GameFilterToggle.Text

        If shadowplay.Visible = False Then
            Base_Background_Top.d.Visible = False
            Base_Background_Top.ME_CLOSE_BG.Visible = False
            Base_Background_Top.ME_CLOSE_BG_GRE.Visible = False
        Else
            Base_Background_Top.d.Visible = True
            Base_Background_Top.ME_CLOSE_BG.Visible = True
            Base_Background_Top.ME_CLOSE_BG_GRE.Visible = True
        End If

        ShadowLoad()

        If animationRunning Then Return

        Dim shadowSize As Size = If(ReplayValue, New Size(240, 373), New Size(240, 329))

        If Not ReplayValue AndAlso Menu_Replay.Height = 133 Then
            If Menu_Record.Visible Then
                For Each s In shas
                    If s IsNot Nothing Then s.Size = New Size(240, 329)
                Next
                Return
            End If

            ANH_Group(
                {Menu_Replay, sha1, sha2, sha3, sha4, Base_Background_Top.b1_all},
                {133, 373, 373, 373, 373, 373},
                {89, 329, 329, 329, 329, 329},
                300)
        ElseIf ReplayValue AndAlso Menu_Replay.Height = 89 Then
            ANH_Group(
                {Menu_Replay, sha1, sha2, sha3, sha4, Base_Background_Top.b1_all},
                {89, 329, 329, 329, 329, 329},
                {133, 373, 373, 373, 373, 373},
                300)
        End If
    End Sub

    Private Sub Engine_UI_Tick(sender As Object, e As EventArgs) Handles Engine_UI.Tick
        Dim EngineFile = Path.Combine(Application.StartupPath, "Engine.UI")


        Dim captureProcess = Process.GetProcessesByName("NVIDIA Capture").FirstOrDefault()
        If captureProcess Is Nothing Then
            Engine_UI.Stop()
            ShowNotifier("notificationErrorEngineNotRunning")
            Exit Sub
        End If


        If File.Exists(EngineFile) Then
            Settings_List.Visible = False
            Base_Settings.Hide()
        Else
            Settings_List.Visible = True
            Base_Settings.Show()
            Engine_UI.Stop()
        End If


    End Sub

    Private Sub Audio_UI_Tick(sender As Object, e As EventArgs) Handles Audio_UI.Tick
        Dim AudioFile = Path.Combine(Application.StartupPath, "Audio.UI")

        Dim captureProcess = Process.GetProcessesByName("NVIDIA Capture").FirstOrDefault()
        If captureProcess Is Nothing Then
            Audio_UI.Stop()
            ShowNotifier("notificationErrorEngineNotRunning")
            Exit Sub
        End If

        If File.Exists(AudioFile) Then
            Settings_List.Visible = False
            Base_Settings.Hide()
        Else
            Settings_List.Visible = True
            Base_Settings.Show()
            Audio_UI.Stop()
        End If
    End Sub
End Class