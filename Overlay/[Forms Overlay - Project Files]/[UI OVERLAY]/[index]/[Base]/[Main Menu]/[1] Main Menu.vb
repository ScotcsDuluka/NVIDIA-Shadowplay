Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.IO
Imports System.Net.Http.Headers
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Windows.Forms
Imports Captrue_Core
Imports Captrue_Core.CaptureCore
Imports Microsoft.Win32
Imports Windows.Graphics.Capture
Imports Windows.Graphics.DirectX
Imports Windows.Graphics.DirectX.Direct3D11
Imports Windows.Media.Devices
Imports WinRT.Interop

Partial Public Class Base
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

    ' UI
    Private Sub SetHoverEffect(ctrl As Control, hoverColor As Color, leaveColor As Color)
        AddHandler ctrl.MouseMove, Sub() ctrl.BackColor = hoverColor
        AddHandler ctrl.MouseLeave, Sub() ctrl.BackColor = leaveColor
    End Sub

    Private ReadOnly HoverColorG As Color = Color.FromArgb(64, 64, 64)
    Private ReadOnly LeaveColorG As Color = Color.FromArgb(38, 43, 47)

    Private ReadOnly HoverColorGR As Color = Color.Green
    Private ReadOnly LeaveColorGR As Color = Color.FromArgb(118, 185, 0)

    Private Sub MainUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '0
        SetHoverEffect(Base_Settings.SW_lang, HoverColorG, LeaveColorG)
        SetHoverEffect(Base_Settings.ch, HoverColorG, LeaveColorG)
        SetHoverEffect(Base_Settings.action_fn, HoverColorGR, LeaveColorGR)

        '1
        SetHoverEffect(Base_Connect.action_fn, HoverColorG, LeaveColorG)

        '2
        SetHoverEffect(Base_Overlay_Hub.action_fn, HoverColorG, LeaveColorG)

        '4
        SetHoverEffect(Base_KeySet.action_fn, HoverColorG, LeaveColorG)

        '5
        SetHoverEffect(Base_RecordingsSet.action_fn, HoverColorGR, LeaveColorGR)

        '7
        SetHoverEffect(Base_Privacy_Control.action_fn, HoverColorGR, LeaveColorGR)
        SetHoverEffect(Base_Privacy_Control.py_2, HoverColorG, LeaveColorG)
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

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Dim handle As IntPtr = Me.Handle
        _hotkeyService = New HotkeyService()
        _hotkeyService.RegisterAll(handle)
        Debug.WriteLine("Hotkeys registered!")

        AppSettings.Initialize()
        AppSettings.Instance.LoadGitHubUser()
        Base_Connect.USERSNAME_TEXT.Text = AppSettings.Instance.GitHubUser.Username
        AppSettings.Instance.LoadGitHubAvatar(Base_Connect.Box_PNG)

        LoadCurrentLanguage()
        InitializeNotifierAPI()
        CheckPrivacyControl()
        MainSub_Load()
        LoadFilePath()
        CreateDataDirectories()
        LoadMicState()
        TIMESLOAD()
        SystemMonitor.StartMonitoring()

        ShowNotifier("notificationOpenShare")
        File.Create(Path.Combine(Application.StartupPath, "Ready")).Dispose()
    End Sub



    Private Sub TIMESLOAD()
        Load_App.Start()
        Privacy_control.Start()
    End Sub

    Private Sub MainSub_Load()
        Base_Background_Top.Show()
        Base_Background_Top.Hide()
        Base_RecordingsSet.Show()
        Base_RecordingsSet.Hide()
        Base_RecordingsSet.Opacity = 1
    End Sub

    Private Sub InitializeNotifierAPI()
        ' Check Resolution ก่อน
        Dim width As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim height As Integer = Screen.PrimaryScreen.Bounds.Height

        If width <> 1920 OrElse height <> 1080 Then
            ShowNotifier("ErrorResolution")
            File.Delete(Path.Combine(Application.StartupPath, "Use_Overlay"))
            Application.Exit()
            Exit Sub
        End If

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
                Application.Exit()
                Exit Sub
            End If
            Process.Start(exePath)
        Catch ex As Exception
            MessageBox.Show("Failed to run NVIDIA Notifier.exe: " & ex.Message)
            Application.Exit()
        End Try
    End Sub
#End Region

#Region "============================================================================ LOCALIZATION"
    Private Sub Lang_Tick(sender As Object, e As EventArgs) Handles Lang.Tick
        ' Base_Background_Top
        Base_Background_Top.Logo_text.Text = LangHelper.GetText("l10n.nvidiashadowplay")

        ' Base_Gallery
        With Base_Gallery
            .Gallery_l10n.Text = LangHelper.GetText("l10n.gallery")
            .LoactionSaved_l10n.Text = LangHelper.GetText("l10n.LoactionSaved")
            .Saved_l10n.Text = LangHelper.GetText("l10n.done")
            .Openloaction_l10n.Text = LangHelper.GetText("l10n.openLocation")
            .Shortcut_l10n.Text = LangHelper.GetText("l10n.Shortcut")
            .Load_l10n.Text = LangHelper.GetText("l10n.Load")
            .Label3.Text = LangHelper.GetText("l10n.all")
            .text_sub.Text = LangHelper.GetText("l10n.gallerynotready")
        End With

        ' Base_Game_Filter
        Base_Game_Filter.Home_settings.Text = LangHelper.GetText("l10n.mods")

        ' Base_KeySet
        With Base_KeySet
            .text_settings.Text = LangHelper.GetText("l10n.keyboardShortcuts")
            .Key_Tx.Text = LangHelper.GetText("l10n.keyboardShortcuts")
            .action_fn.Text = LangHelper.GetText("l10n.Saved")
            .Reset.Text = LangHelper.GetText("l10n.resetAll")
        End With

        ' Base_Privacy Control
        With Base_Privacy_Control
            .text_settings.Text = LangHelper.GetText("l10n.privacyControl")
            .Label4.Text = LangHelper.GetText("l10n.settingsPrivacySwitch")
            .Label2.Text = LangHelper.GetText("l10n.settingsPrivacyDescribe")
            .action_fn.Text = LangHelper.GetText("l10n.back")
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\privacy") Then
                .py_2.Text = LangHelper.GetText("l10n.instantReplayStop")
            Else
                .py_2.Text = LangHelper.GetText("l10n.instantReplayStart")
            End If
        End With

        With Base_KeySet
            .text_settings.Text = LangHelper.GetText("l10n.keyboardShortcuts")
            .Key_Tx.Text = LangHelper.GetText("l10n.keyboardShortcuts")
            .action_fn.Text = LangHelper.GetText("l10n.Saved")
        End With

        With Base_Overlay_Hub
            .text_settings.Text = LangHelper.GetText("l10n.hudLayout")
            .action_fn.Text = LangHelper.GetText("l10n.back")
            .Label4.Text = LangHelper.GetText("l10n.overlays")
        End With

        ' Base_RecordingsSet
        With Base_RecordingsSet
            .text_settings.Text = LangHelper.GetText("l10n.recordings")
            .action_fn.Text = LangHelper.GetText("l10n.Saved")
            .Label4.Text = LangHelper.GetText("l10n.videoCapture")
            .Label1.Text = LangHelper.GetText("l10n.quality")
            .Label10.Text = LangHelper.GetText("l10n.low")
            .Label8.Text = LangHelper.GetText("l10n.medium")
            .Label6.Text = LangHelper.GetText("l10n.high")
            .C_TEXT.Text = LangHelper.GetText("l10n.custom")
            .Label12.Text = LangHelper.GetText("l10n.resolution")
            .Label13.Text = LangHelper.GetText("l10n.framerate")
            '----
            Dim savedSeconds As Integer = 60
            Try
                Dim settingValue As Integer = AppSettings.Instance.Recording.ReplayDuration
                savedSeconds = Math.Max(15, Math.Min(1200, settingValue))
            Catch
            End Try
            savedSeconds = CInt(Math.Round(savedSeconds / 15.0) * 15)
            .UpdateBufferLabel(savedSeconds)
            .UpdateBitrateLabel()
            '----
            .vdo_resetall.Text = LangHelper.GetText("l10n.resetToDefaults")
            .captrueblock.Text = LangHelper.GetText("l10n.settingsVideoCaptureDisable")
            .warm_re.Text = LangHelper.GetText("l10n.captrueresolutionwarm")
            .custom_main.Text = LangHelper.GetText("l10n.custom")
            .advanced_main.Text = LangHelper.GetText("l10n.advanced")
        End With

        With Base_Settings
            .text_settings.Text = LangHelper.GetText("l10n.settings")
            .Back_btn.Text = LangHelper.GetText("l10n.back")
            .text_sub.Text = LangHelper.GetText("l10n.selectmenu")
            .action_fn.Text = LangHelper.GetText("l10n.done")
            .ch.Text = LangHelper.GetText("l10n.checkForUpdates")
        End With

        With Base_Connect
            .text_settings.Text = LangHelper.GetText("l10n.connect")
            .text_menu.Text = LangHelper.GetText("l10n.connect")
            .action_fn.Text = LangHelper.GetText("l10n.back")
        End With

        ' Base Form Controls
        UpdateLocalizedTexts()
        AppSettings.Instance.Save()
        Lang.Stop()
    End Sub

    Public Sub UpdateLocalizedTexts()
        Lang.Start()
        ' Base
        'Main-Menu

#Region "===========================Mode"

        Text_Mode1.Text = LangHelper.GetText("l10n.screenshots")
        Text_Mode2.Text = LangHelper.GetText("l10n.photos")
        Text_Mode3.Text = LangHelper.GetText("l10n.mods")

#End Region '===========================

#Region "===========================Captrue"

        '-Replay
        Replay_Text.Text = LangHelper.GetText("l10n.instantReplay")
        Replay_Stats.Text = LangHelper.GetText("l10n.off")
        Menu_Replay_text.Text = LangHelper.GetText("l10n.instantReplayStart")
        Menu_Replay_save_text.Text = LangHelper.GetText("l10n.Saved")
        Menu_Replay_Sttings_text.Text = LangHelper.GetText("l10n.settings")
        '-Record
        Record_Text.Text = LangHelper.GetText("l10n.manualRecord")
        Record_Stats.Text = LangHelper.GetText("l10n.notRecording")
        Menu_Record_text.Text = LangHelper.GetText("l10n.start")
        Menu_Record_Sttings_text.Text = LangHelper.GetText("l10n.settings")
        '-Live
        Live_Text.Text = LangHelper.GetText("l10n.broadcastLive")
        Live_Stats.Text = LangHelper.GetText("l10n.NotReady")

#End Region '===========================

#Region "===========================Options"

        Share_Text.Text = LangHelper.GetText("l10n.upload")
        Gallery_Text.Text = LangHelper.GetText("l10n.gallery")
        Settings_Text.Text = LangHelper.GetText("l10n.settings")

#End Region

#Region "===========================Preferences"

        Settings_List_Text.Text = LangHelper.GetText("l10n.preferencesHome")
        Connect_Text.Text = LangHelper.GetText("l10n.connect")
        Label12.Text = LangHelper.GetText("l10n.hudLayout")
        Label21.Text = LangHelper.GetText("l10n.highlights")
        Label17.Text = LangHelper.GetText("l10n.keyboardShortcuts")
        Label19.Text = LangHelper.GetText("l10n.videoCapture")
        vdo_setme.Text = LangHelper.GetText("l10n.videoCaptureText")
        nott.Text = LangHelper.GetText("l10n.notifications")
        Label4.Text = LangHelper.GetText("l10n.privacyControl")
        About_Text.Text = LangHelper.GetText("l10n.about")

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
        Base_Privacy_Control.py_2.Text = If(
            My.Computer.FileSystem.FileExists(privacyPath),
            LangHelper.GetText("l10n.instantReplayStop"),
           LangHelper.GetText("l10n.instantReplayStart")
        )
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
            AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
            Base_Background_Top.Bg_SET3.Visible = False
            ME_CLOSE_BG.Visible = False
            d.Visible = False
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
        Dim folderPath As String = Path.Combine(Application.StartupPath, DataDirectoryName)

        If Not Directory.Exists(folderPath) Then
            Directory.CreateDirectory(folderPath)
        End If

        Dim filePath As String = Path.Combine(folderPath, "l10n." & message)
        Try
            File.Create(filePath).Dispose()
        Catch ex As UnauthorizedAccessException
        End Try
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

    Private Sub AlignPanelToTop()
        Dim marginTop As Integer = 160

        ' Base
        Settings_List.Location = New Point(80, 160)
        Base_Background_Top.Main_menu_list.Location = New Point((Me.ClientSize.Width - Base_Background_Top.Main_menu_list.Width) / 2, marginTop)
        shadowplay.Location = New Point((Me.ClientSize.Width - shadowplay.Width) / 2, marginTop)

        ' Gallery
        Base_Gallery.settings_1.Location = New Point((Me.ClientSize.Width - Base_Gallery.settings_1.Width) / 2, marginTop)

        ' Settings
        Base_Privacy_Control.settings_1.Location = New Point(695, marginTop)
        Base_RecordingsSet.setre.Location = New Point(695, marginTop)
        Base_Overlay_Hub.settings_1.Location = New Point(695, marginTop)
        Base_Settings.Main_Menu_SET.Location = New Point(695, marginTop)
        Base_KeySet.keyset.Location = New Point(695, marginTop)
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


End Class