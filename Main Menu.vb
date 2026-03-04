Imports System.Drawing
Imports System.Drawing.Text
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Reflection
Imports System.Drawing.Drawing2D
Public Class Base



#Region "============================================================================ Animation Engine"

    Private animStart As DateTime
    Private animDuration As Double
    Private startValue As Integer
    Private targetValue As Integer
    Private animationRunning As Boolean = False
    Private currentControl As Control

    Private WithEvents AnimationTimer As New Timer With {.Interval = 15}

    Public Sub StartSlideY(ctrl As Control,
                        fromY As Integer,
                        toY As Integer,
                        duration As Double)

        If animationRunning Then Return

        currentControl = ctrl
        startValue = fromY
        targetValue = toY
        animDuration = duration

        ctrl.Top = fromY
        animationRunning = True
        animStart = DateTime.Now

        AnimationTimer.Start()

    End Sub

    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs) Handles AnimationTimer.Tick

        If Not animationRunning Then Return

        Dim elapsed = (DateTime.Now - animStart).TotalMilliseconds
        Dim t As Double = elapsed / animDuration

        If t >= 1 Then
            t = 1
            animationRunning = False
            AnimationTimer.Stop()
        End If

        Dim eased As Double = 1 - Math.Pow(1 - t, 3)
        Dim newY As Integer = startValue + (targetValue - startValue) * eased

        currentControl.Top = newY

    End Sub

#End Region

#Region "============================================================================ Fonts"

    Private pfc As New PrivateFontCollection()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load



        Dim fontPath As String = IO.Path.Combine(Application.StartupPath, "_icon.ttf")

        pfc.AddFontFile(fontPath)

        Dim controlsToChange As Control() = {
    Logo_Mode1, set_to, logo_py, logo_replay, logo_record, logo_live, mic, vdo,
    logo_gallery, Logo_Mode2, Logo_Mode3, logo_pf, Label5, Label15, Label18,
    Label22, Label20, noty, Label9, icon_settings, Label14, icon_replay, Label8
}

        For Each ctrl As Control In controlsToChange
            ctrl.Font = New Font(pfc.Families(0), ctrl.Font.Size, ctrl.Font.Style)
        Next
    End Sub
#End Region

#Region "============================================================================ NATIVE METHODS & STRUCTURES"

    ' Window Style Constants
    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    ' Virtual Key Codes - Modifier Keys
    Private Const VK_ALT As Integer = &H12
    Private Const VK_SHIFT As Integer = &H10
    Private Const VK_CONTROL As Integer = &H11

    ' Virtual Key Codes - Function Keys
    Private Const VK_F1 As Integer = &H70
    Private Const VK_F2 As Integer = &H71
    Private Const VK_F3 As Integer = &H72
    Private Const VK_F8 As Integer = &H77
    Private Const VK_F9 As Integer = &H78
    Private Const VK_F10 As Integer = &H79
    Private Const VK_F12 As Integer = &H7B

    ' Virtual Key Codes - Letter Keys
    Private Const VK_Z As Integer = &H5A
    Private Const VK_T As Integer = &H54
    Private Const VK_W As Integer = &H57

    ' Virtual Key Codes - Navigation Keys
    Private Const VK_LEFT As Integer = &H25
    Private Const VK_UP As Integer = &H26
    Private Const VK_RIGHT As Integer = &H27
    Private Const VK_DOWN As Integer = &H28

    ' Virtual Key Codes - Other Keys
    Private Const VK_LBUTTON As Integer = &H1
    Private Const VK_RBUTTON As Integer = &H2
    Private Const VK_MBUTTON As Integer = &H4
    Private Const VK_CANCEL As Integer = &H3
    Private Const VK_BACK As Integer = &H8
    Private Const VK_TAB As Integer = &H9
    Private Const VK_CLEAR As Integer = &HC
    Private Const VK_RETURN As Integer = &HD
    Private Const VK_PAUSE As Integer = &H13
    Private Const VK_CAPITAL As Integer = &H14
    Private Const VK_ESCAPE As Integer = &H1B
    Private Const VK_SPACE As Integer = &H20
    Private Const VK_PAGEUP As Integer = &H21
    Private Const VK_PAGEDOWN As Integer = &H22
    Private Const VK_END As Integer = &H23
    Private Const VK_HOME As Integer = &H24
    Private Const VK_SELECT As Integer = &H29
    Private Const VK_PRINT As Integer = &H2A
    Private Const VK_EXECUTE As Integer = &H2B
    Private Const VK_SNAPSHOT As Integer = &H2C
    Private Const VK_INSERT As Integer = &H2D
    Private Const VK_DELETE As Integer = &H2E
    Private Const VK_HELP As Integer = &H2F

    ' Virtual Key Codes - A-Z
    Private Const VK_A As Integer = &H41
    Private Const VK_B As Integer = &H42
    Private Const VK_C As Integer = &H43
    Private Const VK_D As Integer = &H44
    Private Const VK_E As Integer = &H45
    Private Const VK_F As Integer = &H46
    Private Const VK_G As Integer = &H47
    Private Const VK_H As Integer = &H48
    Private Const VK_I As Integer = &H49
    Private Const VK_J As Integer = &H4A
    Private Const VK_K As Integer = &H4B
    Private Const VK_L As Integer = &H4C
    Private Const VK_M As Integer = &H4D
    Private Const VK_N As Integer = &H4E
    Private Const VK_O As Integer = &H4F
    Private Const VK_P As Integer = &H50
    Private Const VK_Q As Integer = &H51
    Private Const VK_R As Integer = &H52
    Private Const VK_S As Integer = &H53
    Private Const VK_U As Integer = &H55
    Private Const VK_V As Integer = &H56
    Private Const VK_X As Integer = &H58
    Private Const VK_Y As Integer = &H59

    ' DLL Imports - User32
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetAsyncKeyState(vKey As Integer) As Short
    End Function

    ' DLL Imports - Kernel32
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

    ' Structures
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

    ' Application Constants
    Private Const AppName As String = "NVIDIA Shadowplay™"
    Private ReadOnly greenColor As Color = ColorTranslator.FromHtml("#76B900")

    ' Default Paths
    Private Const DefaultSavePath As String = "C:\Shadowplay"
    Private Const DataDirectoryName As String = "NVIDIA_Shadowplay_Data"

    ' File/Directory Names
    Private Const ReplayOnFile As String = "Replay/on"
    Private Const ReplayOffFile As String = "Replay/off"
    Private Const MicOnFile As String = "mic/mic_on"
    Private Const MicOffFile As String = "mic/mic_off"
    Private Const PrivacyFile As String = "py"

    ' Replay State
    Private Replay_value As Boolean = False

    ' Key State Tracking - General
    Private isFunctionActive As Boolean = False
    Private isKeyPressed As Boolean = False

    ' Key State Tracking - Specific Functions
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

    Private isFunctionActive_f3 As Boolean = False
    Private isKeyPressed_f3 As Boolean = False

    Private isFunctionActive_f8 As Boolean = False
    Private isKeyPressed_f8 As Boolean = False

    ' State Variables
    Private isNotiOn As Boolean = False
    Private notifierShown As Boolean = False
    Private ffmpegProcess As Process

#End Region

#Region "============================================================================ FORM LOAD & INITIALIZATION"
    Private Sub LoadCurrentLanguage()
        Dim langFolder As String = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile As String = Path.Combine(langFolder, "current.txt")

        ' อ่านค่าภาษาปัจจุบัน
        Dim currentLang As String = "en-US"
        If File.Exists(currentFile) Then currentLang = File.ReadAllText(currentFile).Trim()

        ' โหลดไฟล์ JSON
        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        If Not File.Exists(langFile) Then langFile = Path.Combine(langFolder, "en-US.json")
        LangHelper.LoadLang(langFile)

        ' ตั้งชื่อปุ่มจาก JSON
        SW_lang.Text = LangHelper.GetText("meta.languageName")
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadCurrentLanguage()


        ' Initialize Notifier API
        InitializeNotifierAPI()

        ' Configure window appearance
        HideFromAltTab()

        ' Show initial UI elements
        Base_Background_Top.Show()
        Privacy_control.Start()

        ' Load application data
        LoadMicrophoneData()
        CheckPrivacyControl()
        If My.Settings.MicStatus = True Then
            mic.Text = ""
        Else
            mic.Text = ""
        End If
        Load_App.Start()

        ' Finalize initialization
        Base_Background_Top.Hide()
        LoadFilePath()
        CreateDataDirectories()
    End Sub

    Private Async Sub MainSub_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitializePanelSizes()
        StartKeyDetection()


    End Sub

    Private Sub InitializeNotifierAPI()
        Try
            Dim exePath As String = Path.Combine(Application.StartupPath, "Notifier-API.exe")
            If Not File.Exists(exePath) Then
                MessageBox.Show(
                    "Notifier-API Service Could Not Be Started!" & vbCrLf &
                    "Please check if the file exists and you have sufficient permissions.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
                Application.Exit()
            End If
            Process.Start(exePath)
        Catch ex As Exception
            MessageBox.Show("Failed to run Notifier-API.exe: " & ex.Message)
        End Try
        ShowNotifier("game_n")
    End Sub

    Private Sub InitializePanelSizes()
        ' action_sc.Size = New Size(600, 300)
        settings_1.Size = New Size(1010, 600)
        'action.Size = New Size(1100, 200)
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
            .Saved_l10n.Text = LangHelper.GetText("l10n.Saved")
            .Openloaction_l10n.Text = LangHelper.GetText("l10n.openLocation")
            .Shortcut_l10n.Text = LangHelper.GetText("l10n.Shortcut")
            .Load_l10n.Text = LangHelper.GetText("l10n.Load")
            .Label3.Text = LangHelper.GetText("l10n.all")
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

        With Base_RecordingsSet
            .text_settings.Text = LangHelper.GetText("l10n.recordings")
            .action_fn.Text = LangHelper.GetText("l10n.Saved")
            .Label4.Text = LangHelper.GetText("l10n.videoCapture")
            .Label1.Text = LangHelper.GetText("l10n.quality")
            .Label10.Text = LangHelper.GetText("l10n.low")
            .Label8.Text = LangHelper.GetText("l10n.medium")
            .Label6.Text = LangHelper.GetText("l10n.high")
            .Label5.Text = LangHelper.GetText("l10n.custom")
            .Label12.Text = LangHelper.GetText("l10n.resolution")
            .Label13.Text = LangHelper.GetText("l10n.framerate")
            .Label14.Text = LangHelper.GetText("l10n.bitrate")
        End With

        ' Base Form Controls
        UpdateLocalizedTexts()
        My.Settings.Save()
        Lang.Stop()
    End Sub

    Private Sub UpdateLocalizedTexts()
        Lang.Start()
        text_settings.Text = (Assembly.GetExecutingAssembly().GetName().Version.ToString())
        'LangHelper.GetText("l10n.settings")
        Label3.Text = LangHelper.GetText("l10n.settings")
        Home_settings.Text = LangHelper.GetText("l10n.preferencesHome")
        action_fn.Text = LangHelper.GetText("l10n.done")
        ch.Text = LangHelper.GetText("l10n.checkForUpdates")
        text_py.Text = LangHelper.GetText("l10n.connect")
        Label12.Text = LangHelper.GetText("l10n.hudLayout")
        Label21.Text = LangHelper.GetText("l10n.highlights")
        Label17.Text = LangHelper.GetText("l10n.keyboardShortcuts")
        Label19.Text = LangHelper.GetText("l10n.videoCapture")
        nott.Text = LangHelper.GetText("l10n.notifications")
        Text_Mode1.Text = LangHelper.GetText("l10n.screenshots")
        Text_Mode2.Text = LangHelper.GetText("l10n.photos")
        Text_Mode3.Text = LangHelper.GetText("l10n.mods")
        replay.Text = LangHelper.GetText("l10n.instantReplay")
        record.Text = LangHelper.GetText("l10n.manualRecord")
        live.Text = LangHelper.GetText("l10n.broadcastLive")
        s_replay.Text = LangHelper.GetText("l10n.off")
        s_record.Text = LangHelper.GetText("l10n.notRecording")
        s_live.Text = LangHelper.GetText("l10n.NotReady")
        pf.Text = LangHelper.GetText("l10n.upload")
        gallery.Text = LangHelper.GetText("l10n.gallery")
        Label2.Text = LangHelper.GetText("l10n.settings")
        Label10.Text = LangHelper.GetText("l10n.settings")
        Label13.Text = LangHelper.GetText("l10n.start")
        if_replay.Text = LangHelper.GetText("l10n.instantReplayStart")
        Label7.Text = LangHelper.GetText("l10n.Saved")
        Label4.Text = LangHelper.GetText("l10n.privacyControl")
    End Sub

#End Region

#Region "============================================================================ FILE & DIRECTORY OPERATIONS"

    Private Sub LoadFilePath()
        Base_Gallery.txtFilePath.Text = My.Settings.SavePath
        If String.IsNullOrEmpty(Base_Gallery.txtFilePath.Text) Then
            Base_Gallery.txtFilePath.Text = DefaultSavePath
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

    Private Sub LoadMicrophoneData()
        Dim micOnPath As String = Path.Combine(Application.StartupPath, DataDirectoryName, MicOnFile)
        mic.Text = If(My.Computer.FileSystem.FileExists(micOnPath), "", "")
    End Sub

    Private Sub CheckPrivacyControl()
        Dim privacyPath As String = Path.Combine(Application.StartupPath, DataDirectoryName, PrivacyFile)
        Base_Privacy_Control.py_2.Text = If(
            My.Computer.FileSystem.FileExists(privacyPath),
            "Turn off",
            "Turn on"
        )
    End Sub

#End Region

#Region "============================================================================ SCREEN CAPTURE"

    Private Sub CaptureScreen()
        If Not My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, DataDirectoryName, PrivacyFile)) Then
            ShowNotifier("Screenshot")
            Return
        End If

        Dim filePath As String = My.Settings.SavePath
        If String.IsNullOrWhiteSpace(filePath) Then
            ShowNotifier("validsavepath")
            Return
        End If

        Try
            Using bmpScreenshot As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
                Using g As Graphics = Graphics.FromImage(bmpScreenshot)
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)
                End Using

                Dim fileName As String = Path.Combine(filePath, $"Shadowplay Screenshot {DateTime.Now:dd_MM_ss}.png")
                bmpScreenshot.Save(fileName, System.Drawing.Imaging.ImageFormat.Png)

                ShowNotifier(If(Directory.Exists(filePath), "Screenshot", "validsavepath"))
            End Using
        Catch ex As Exception
            MessageBox.Show("Failed to capture screen: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region "============================================================================ NOTIFIER SYSTEM"

    Private Sub ShowNotifier(message As String)
        Dim folderPath As String = Path.Combine(Application.StartupPath, DataDirectoryName)

        If Not Directory.Exists(folderPath) Then
            Directory.CreateDirectory(folderPath)
        End If

        Dim filePath As String = Path.Combine(folderPath, message & "-api")
        Try
            File.Create(filePath).Dispose()
        Catch ex As UnauthorizedAccessException
            ' Silently ignore access errors
        End Try
    End Sub

#End Region

#Region "============================================================================ WINDOW MANAGEMENT"

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        ' Ensure correct operator precedence: add TOOLWINDOW then remove APPWINDOW
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And (Not WS_EX_APPWINDOW))
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        AlignPanelToTop()
    End Sub

    Private Sub AlignPanelToTop()
        Dim marginTop As Integer = 160

        'action_sc.Location = New Point((Me.ClientSize.Width - action_sc.Width) / 2, marginTop)
        Main_menu_list.Location = New Point((Me.ClientSize.Width - Main_menu_list.Width) / 2, marginTop)
        Base_Gallery.settings_1.Location = New Point((Me.ClientSize.Width - Base_Gallery.settings_1.Width) / 2, marginTop)
        Base_Privacy_Control.settings_1.Location = New Point((Me.ClientSize.Width - Base_Privacy_Control.settings_1.Width) / 2, marginTop)
        settings_1.Location = New Point((Me.ClientSize.Width - settings_1.Width) / 2, marginTop)
        Base_RecordingsSet.setre.Location = New Point((Me.ClientSize.Width - Base_RecordingsSet.setre.Width) / 2, marginTop)
        Base_Overlay_Hub.settings_1.Location = New Point((Me.ClientSize.Width - Base_Overlay_Hub.settings_1.Width) / 2, marginTop)
        Base_Background_Top.Main_menu_list.Location = New Point((Me.ClientSize.Width - Base_Background_Top.Main_menu_list.Width) / 2, marginTop)
        Base_KeySet.keyset.Location = New Point((Me.ClientSize.Width - Base_KeySet.keyset.Width) / 2, marginTop)
    End Sub

#End Region

#Region "============================================================================ STARTUP REGISTRY"

    Private Sub AddToStartup()
        Dim exePath As String = Application.ExecutablePath
        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            If key IsNot Nothing Then
                key.SetValue(AppName, exePath)
            End If
        End Using
    End Sub

    Private Sub RemoveFromStartup()
        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            If key IsNot Nothing Then
                key.DeleteValue(AppName, False)
            End If
        End Using
    End Sub

#End Region

#Region "============================================================================ KEYBOARD DETECTION"

    Private Sub StartKeyDetection()
        ' === SHARE PANEL ===
        StartKeyTimer(ALT_Z, 1)           ' Alt + Z - Share Panel

        ' === PHOTO MODE SET ===
        StartKeyTimer(ALT_F1, 1)          ' Alt + F1 - Screenshot
        StartKeyTimer(alt_F2_F3_F8, 1)    ' Alt + F2, F3, F8
        StartKeyTimer(ALT_F12, 1)         ' Alt + F12

        ' === RECORDING / VIDEO SET ===
        StartKeyTimer(ALT_F9, 1)          ' Alt + F9 - Record
        StartKeyTimer(ALT_F10, 1)         ' Alt + F10 - Save Replay
        StartKeyTimer(ALT_SHIFT_F10, 1)   ' Alt + Shift + F10 - Instant Replay
    End Sub

    Private Sub StartKeyTimer(timer As Timer, interval As Integer)
        timer.Interval = interval
        timer.Start()
    End Sub

#Region "=== SHARE PANEL ==="

    ' Alt + Z - Open/Close Share Panel
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles ALT_Z.Tick
        Dim folderPath As String = Base_Gallery.txtFilePath.Text.Trim()

        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_Z) And &H8000) <> 0 Then
            If Not isKeyPressed Then
                isFunctionActive = Not isFunctionActive
                If isFunctionActive Then
                    If Base_www.Opacity >= 0.01 Then Return
                    isFunctionActive_f3 = False
                    ShowMainPanel()
                    Base_Game_Filter_Sub.Opacity = 0
                    Base_Game_Filter.Opacity = 0
                    Base_Game_Filter.Hide()
                    Base_Game_Filter_Sub.Hide()
                Else
                    HideAllControls()
                End If
                isKeyPressed = True
            End If
        Else
            isKeyPressed = False
        End If

        ' Alt + T - Game Notification
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_T) And &H8000) <> 0 Then
            If Not isKeyPressed_t Then
                ShowNotifier("game_n")
                isKeyPressed_t = True
            End If
        Else
            isKeyPressed_t = False
        End If
    End Sub

    Public Sub HideAllControls()
        isFunctionActive = False

        Me.Opacity = 0
        Base_Background_Top.Opacity = 0
        Base_Gallery.Opacity = 0
        hd.Size = New Size(10000, 10000)

        ' Hide panels
        sub_replay.Visible = False
        sub_record.Visible = False
        settings_1.Visible = False
        Main_menu_list.Visible = True

        ' Hide action indicators
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False    ' FIXED: Falsez -> False

        Base_Gallery.Hide()

        If Base_www.Opacity < 0.1 Then
            Base_Background.Opacity = 0
            Base_Background.Hide()
        End If

        Base_Background_Top.Hide()
        Me.Hide()
    End Sub

    Public Sub ShowMainPanel()
        isFunctionActive = True
        hd.Size = New Size(0, 0)
        Me.WindowState = FormWindowState.Maximized
        Base_Background.WindowState = FormWindowState.Maximized
        Base_Background.Show()
        Base_Background_Top.Show()
        Base_Background_Top.TopMost = True
        Me.Show()
        Me.TopMost = True
        Base_Background.Opacity = 0.5
        Base_Background_Top.Opacity = 1
        Me.Opacity = 0.85
    End Sub

#End Region

#Region "=== PHOTO MODE SET (F1, F2, F3, F8, F12) ==="

    ' Alt + F1 - Screenshot
    Private Sub CaptureScreen_Tick(sender As Object, e As EventArgs) Handles ALT_F1.Tick
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F1) And &H8000) <> 0 Then
            If Not isKeyPressed_F1 Then
                isFunctionActive_F1 = Not isFunctionActive_F1
                CaptureScreen()
                isKeyPressed_F1 = True
            End If
        Else
            isKeyPressed_F1 = False
        End If
    End Sub

    ' Alt + F2, F3, F8 - Photo Mode Functions
    Private Sub alt_F_1_2_Tick(sender As Object, e As EventArgs) Handles alt_F2_F3_F8.Tick

        ' Alt + F2 - Photo Mode Error
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F2) And &H8000) <> 0 Then
            If Not isKeyPressed_f2 Then
                isFunctionActive_f2 = Not isFunctionActive_f2
                ShowNotifier("photo_mode_error")
                isKeyPressed_f2 = True
            End If
        Else
            isKeyPressed_f2 = False
        End If

        ' Alt + F3 - Game Filter
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F3) And &H8000) <> 0 Then
            If Not isKeyPressed_f3 Then
                isFunctionActive_f3 = Not isFunctionActive_f3
                ToggleGameFilter()
                isKeyPressed_f3 = True
            End If
        Else
            isKeyPressed_f3 = False    ' FIXED: Added reset
        End If

        ' Alt + F8 - Photo Mode (Not Ready)
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F8) And &H8000) <> 0 Then
            If Not isKeyPressed_f8 Then
                isFunctionActive_f8 = Not isFunctionActive_f8
                ShowNotifier("feature_not_ready")
                isKeyPressed_f8 = True
            End If
        Else
            isKeyPressed_f8 = False
        End If
    End Sub

    Private Sub ToggleGameFilter()

        ShowNotifier("notworkgpu")
        Base_Game_Filter.Main_Filter.Location = New Point(-500, 0)
        Base_Game_Filter_Sub.BG.Location = New Point(-500, 0)
        If isFunctionActive_f3 Then
            isKeyPressed = False
            ShowNotifier("notworkgpu")
            HideAllControls()
            Base_Game_Filter_Sub.Show()
            Base_Game_Filter_Sub.Opacity = 0.78
            Base_Game_Filter.Show()
            Base_Game_Filter.Opacity = 1
        Else
            Base_Game_Filter_Sub.Opacity = 0
            Base_Game_Filter.Opacity = 0
            Base_Game_Filter.Hide()
            Base_Game_Filter_Sub.Hide()
        End If
    End Sub

    ' Alt + F12 - Feature Not Ready
    Private Sub ALT_F2_Tick(sender As Object, e As EventArgs) Handles ALT_F12.Tick
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F12) And &H8000) <> 0 Then
            If Not isKeyPressed_p Then
                isFunctionActive_p = Not isFunctionActive_p
                ShowNotifier("feature_not_ready")
                isKeyPressed_p = True
            End If
        Else
            isKeyPressed_p = False
        End If
    End Sub

#End Region

#Region "=== RECORDING / VIDEO SET (F9, F10, Shift+F10) ==="

    ' Alt + F9 - Start/Stop Recording
    Private Sub Record_Tick(sender As Object, e As EventArgs) Handles ALT_F9.Tick
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F9) And &H8000) <> 0 Then
            If Not isKeyPressed_record Then
                isFunctionActive_record = Not isFunctionActive_record
                ToggleRecording()
                isKeyPressed_record = True
            End If
        Else
            isKeyPressed_record = False
        End If
    End Sub

    Private Sub ToggleRecording()
        Dim isRecording As Boolean = logo_record.ForeColor = greenColor OrElse
                                      logo_record.ForeColor = ColorTranslator.FromHtml("#426800")

        If isRecording Then
            logo_record.ForeColor = Color.White
            ShowNotifier("recording_saved")
        Else
            logo_record.ForeColor = greenColor
            ShowNotifier("recording_started")
            Base_Notifier.Show()
            With Base_Notifier
                .icon_n.Font = New Font(.icon_n.Font.FontFamily, If(isValidPath, 50, 40))
                .icon_n.ForeColor = Color.White
                .icon_n.Text = ""
                .text_n.Text = LangHelper.GetText("l10n.notificationErrorNVENC")
            End With
        End If
    End Sub

    ' Alt + Shift + F10 - Toggle Instant Replay
    Private Sub alt_shift_f10_Tick(sender As Object, e As EventArgs) Handles ALT_SHIFT_F10.Tick
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_SHIFT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F10) And &H8000) <> 0 Then
            If Not isKeyPressed_replay Then
                isFunctionActive_replay = Not isFunctionActive_replay
                ToggleInstantReplay()
                isKeyPressed_replay = True
            End If
        Else
            isKeyPressed_replay = False
        End If
    End Sub

    Private Sub ToggleInstantReplay()
        Replay_value = Not Replay_value

        If Replay_value Then
            logo_replay.ForeColor = greenColor
            if_replay.Text = LangHelper.GetText("l10n.instantReplayStop")
            ShowNotifier("instant_replay_on")
            Base_Notifier.Show()
            With Base_Notifier
                .icon_n.Font = New Font(.icon_n.Font.FontFamily, If(isValidPath, 50, 40))
                .icon_n.ForeColor = Color.White
                .icon_n.Text = ""
                .text_n.Text = LangHelper.GetText("l10n.notificationErrorNVENC")
            End With
        Else
            logo_replay.ForeColor = Color.White
            if_replay.Text = LangHelper.GetText("l10n.instantReplayStart")
            ShowNotifier("instant_replay_off")
        End If

        UpdateReplayStatus()
    End Sub

    ' Alt + F10 - Save Replay
    Private Sub save_Tick(sender As Object, e As EventArgs) Handles ALT_F10.Tick
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_F10) And &H8000) <> 0 AndAlso
           (GetAsyncKeyState(VK_SHIFT) And &H8000) = 0 Then
            If Not isKeyPressed_replay_save Then
                isFunctionActive_replay_save = Not isFunctionActive_replay_save
                If Replay_value Then
                    ShowNotifier("saved_last_15")
                Else
                    ShowNotifier("replay_turn_on")
                End If
                isKeyPressed_replay_save = True
            End If
        Else
            isKeyPressed_replay_save = False
        End If
    End Sub

#End Region

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - SCREENSHOT"

    Private Sub bg_sh_MouseMove(sender As Object, e As MouseEventArgs) Handles Bg_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub bg_sh_MouseLeave(sender As Object, e As EventArgs) Handles Bg_Mode1.MouseLeave
        ResetScreenshotColors()
        SetScreenshotBorder(False)
        Base_Background_Top.Bg_Mode1.Visible = False
    End Sub

    Private Sub logo_sh_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub logo_sh_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode1.MouseLeave
        SetScreenshotBorder(False)
        Base_Background_Top.Bg_Mode1.Visible = False
    End Sub

    Private Sub sh_MouseMove(sender As Object, e As MouseEventArgs) Handles Text_Mode1.MouseMove, Key_Mode1.MouseMove
        SetScreenshotBorder(True)
        Base_Background_Top.Bg_Mode1.Visible = True
    End Sub

    Private Sub sh_MouseLeave(sender As Object, e As EventArgs) Handles Text_Mode1.MouseLeave, Key_Mode1.MouseLeave
        SetScreenshotBorder(False)
        Base_Background_Top.Bg_Mode1.Visible = False
    End Sub

    Private Sub SetScreenshotBorder(isVisible As Boolean)
        s_1.Visible = isVisible
        s_1r.Visible = isVisible
        s_1l.Visible = isVisible
        s_1b.Visible = isVisible
    End Sub

    Private Sub ResetScreenshotColors()
        Logo_Mode1.ForeColor = Color.White
        Text_Mode1.ForeColor = Color.White
    End Sub

    Private Sub logo_sh_Click(sender As Object, e As EventArgs) Handles Logo_Mode1.Click
        CaptureScreen()
    End Sub

    Private Sub bg_sh_Click(sender As Object, e As EventArgs) Handles Bg_Mode1.Click
        CaptureScreen()
    End Sub

    Private Sub sh_Click(sender As Object, e As EventArgs) Handles Text_Mode1.Click
        CaptureScreen()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - SETTINGS"

    Private Sub set_to_Click(sender As Object, e As EventArgs) Handles set_to.Click
        Opacity = 1
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        settings_1.Visible = True
        Main_menu_list.Visible = False
        sub_replay.Visible = False
        sub_record.Visible = False
    End Sub

    Private Sub set_to_MouseMove(sender As Object, e As MouseEventArgs)
        SetSettingsBorder(True)
    End Sub

    Private Sub set_to_MouseLeave(sender As Object, e As EventArgs)
        SetSettingsBorder(False)
    End Sub

    Private Sub SetSettingsBorder(isVisible As Boolean)
        s1.Visible = isVisible
        s1r.Visible = isVisible
        s1l.Visible = isVisible
        s1b.Visible = isVisible
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - PRIVACY/CONNECT"

    Private Sub box_py_MouseMove(sender As Object, e As MouseEventArgs) Handles box_py.MouseMove
        bg_py.BackColor = greenColor
    End Sub

    Private Sub box_py_MouseLeave(sender As Object, e As EventArgs) Handles box_py.MouseLeave
        bg_py.BackColor = Color.Gray
    End Sub

    Private Sub text_py_MouseMove(sender As Object, e As MouseEventArgs) Handles text_py.MouseMove
        bg_py.BackColor = greenColor
    End Sub

    Private Sub text_py_MouseLeave(sender As Object, e As EventArgs) Handles text_py.MouseLeave
        bg_py.BackColor = Color.Gray
    End Sub

    Private Sub logo_py_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_py.MouseMove
        bg_py.BackColor = greenColor
    End Sub

    Private Sub logo_py_MouseLeave(sender As Object, e As EventArgs) Handles logo_py.MouseLeave
        bg_py.BackColor = Color.Gray
    End Sub

    Private Sub box_py_Click(sender As Object, e As EventArgs) Handles box_py.Click
        ShowNotifier("account_confirm_error")
    End Sub

    Private Sub text_py_Click(sender As Object, e As EventArgs) Handles text_py.Click
        ShowNotifier("account_confirm_error")
    End Sub

    Private Sub logo_py_Click(sender As Object, e As EventArgs) Handles logo_py.Click
        ShowNotifier("account_confirm_error")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - REPLAY"

    Private Sub replay_on_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_replay.MouseMove
        SetReplayBorder(Not sub_replay.Visible)
        Base_Background_Top.b1.Visible = True
    End Sub

    Private Sub replay_on_MouseLeave(sender As Object, e As EventArgs) Handles logo_replay.MouseLeave
        SetReplayBorder(False)
        logo_replay.ForeColor = If(Replay_value, greenColor, Color.White)
        Base_Background_Top.b1.Visible = False
    End Sub

    Private Sub SetReplayBorder(isVisible As Boolean)

        a_1.Visible = sub_replay.Visible OrElse isVisible
        a_1r.Visible = isVisible
        a_1l.Visible = isVisible
        a_1b.Visible = isVisible
    End Sub

    Private Sub logo_replay_Click(sender As Object, e As EventArgs)
        sub_replay.Visible = Not sub_replay.Visible
        sub_record.Visible = False
        a_1.Visible = Not a_1.Visible
        a_2.Visible = False
        a_3.Visible = False
    End Sub

    Private Sub replay_on_Click(sender As Object, e As EventArgs) Handles logo_replay.Click
        sub_replay.Visible = Not sub_replay.Visible
        sub_record.Visible = False
        a_1.Visible = Not a_1.Visible
        a_2.Visible = False
        a_3.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - RECORD"

    Private Sub logo_record_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_record.MouseMove
        SetRecordBorder(Not sub_record.Visible)
        Base_Background_Top.b2.Visible = True
    End Sub
    Private Sub logo_record_MouseLeave(sender As Object, e As EventArgs) Handles logo_record.MouseLeave
        SetRecordBorder(False)
        Base_Background_Top.b2.Visible = False
        ' Maintain recording color if active
        If logo_record.ForeColor = greenColor OrElse logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
            logo_record.ForeColor = greenColor
        Else
            logo_record.ForeColor = Color.White
        End If
    End Sub

    Private Sub SetRecordBorder(isVisible As Boolean)
        a_2.Visible = sub_record.Visible OrElse isVisible
        a_2r.Visible = isVisible
        a_2l.Visible = isVisible
        a_2b.Visible = isVisible
    End Sub

    Private Sub logo_record_Click(sender As Object, e As EventArgs) Handles logo_record.Click
        sub_record.Visible = Not sub_record.Visible
        sub_replay.Visible = False
        a_2.Visible = True
        a_1.Visible = False
        a_3.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - LIVE STREAM"

    Private Sub logo_live_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_live.MouseMove
        a_3.Visible = True
        a_3r.Visible = True
        a_3l.Visible = True
        a_3b.Visible = True
        Base_Background_Top.b3.Visible = True
    End Sub

    Private Sub logo_live_MouseLeave(sender As Object, e As EventArgs) Handles logo_live.MouseLeave
        a_3.Visible = False
        a_3r.Visible = False
        a_3l.Visible = False
        a_3b.Visible = False
        Base_Background_Top.b3.Visible = False
        logo_live.ForeColor = Color.White
    End Sub

    Private Sub logo_live_Click(sender As Object, e As EventArgs) Handles logo_live.Click
        File.Create(Path.Combine(Application.StartupPath, DataDirectoryName, "feature_not_ready-api")).Dispose()
        sub_replay.Visible = False
        a_1.Visible = False
        sub_record.Visible = False
        a_2.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - MICROPHONE & VIDEO"

    Private Sub mic_MouseMove(sender As Object, e As MouseEventArgs) Handles mic.MouseMove
        mic.ForeColor = Color.Gray
    End Sub

    Private Sub mic_MouseLeave(sender As Object, e As EventArgs) Handles mic.MouseLeave
        mic.ForeColor = Color.White
    End Sub

    Private Sub mic_Click(sender As Object, e As EventArgs) Handles mic.Click
        mic.Text = If(mic.Text = "", "", "")
    End Sub

    Private Sub vdo_MouseMove(sender As Object, e As MouseEventArgs) Handles vdo.MouseMove
        vdo.ForeColor = Color.Gray
    End Sub

    Private Sub vdo_MouseLeave(sender As Object, e As EventArgs) Handles vdo.MouseLeave
        vdo.ForeColor = Color.White
    End Sub

    Private Sub vdo_Click(sender As Object, e As EventArgs) Handles vdo.Click
        ShowNotifier("extension_not_found")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - GALLERY"

    Private Sub SetGalleryColors(color As Color)
        logo_gallery.ForeColor = color
        gallery.ForeColor = color
        bg_gallery.ForeColor = color
    End Sub

    Private Sub SetGalleryBorder(isVisible As Boolean)
        g1.Visible = isVisible
        g1r.Visible = isVisible
        g1l.Visible = isVisible
        g1b.Visible = isVisible
    End Sub


    Private Sub logo_gallery_MouseMove_1(sender As Object, e As MouseEventArgs) Handles logo_gallery.MouseMove, gallery.MouseMove, bg_gallery.MouseMove
        Base_Background_Top.Bg_SET2.Visible = True
        SetGalleryBorder(True)
    End Sub

    Private Sub logo_gallery_MouseLeave_1(sender As Object, e As EventArgs) Handles logo_gallery.MouseLeave, gallery.MouseLeave, bg_gallery.MouseLeave
        Base_Background_Top.Bg_SET2.Visible = False
        SetGalleryBorder(False)
    End Sub



    Private Sub bg_gallery_Click(sender As Object, e As EventArgs) Handles bg_gallery.Click
        ShowGallery()
    End Sub

    Private Sub gallery_Click(sender As Object, e As EventArgs) Handles gallery.Click
        ShowGallery()
    End Sub

    Private Sub logo_gallery_Click(sender As Object, e As EventArgs) Handles logo_gallery.Click
        ShowGallery()
    End Sub

    Private Sub ShowGallery()
        Main_menu_list.Visible = False
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Base_Gallery.WindowState = FormWindowState.Maximized
        Base_Gallery.Opacity = 1
        sub_replay.Visible = False
        sub_record.Visible = False
        Base_Gallery.Show()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - REPLAY CONTROLS"

    Private Sub sh_replay_MouseMove(sender As Object, e As MouseEventArgs) Handles sh_replay.MouseMove
        SetReplayControlBorder(True)
    End Sub

    Private Sub sh_replay_MouseLeave(sender As Object, e As EventArgs) Handles sh_replay.MouseLeave
        SetReplayControlBorder(False)
    End Sub

    Private Sub replay_sc_MouseMove(sender As Object, e As MouseEventArgs) Handles replay_sc.MouseMove
        SetReplayControlBorder(True)
    End Sub

    Private Sub replay_sc_MouseLeave(sender As Object, e As EventArgs) Handles replay_sc.MouseLeave
        SetReplayControlBorder(False)
    End Sub

    Private Sub if_replay_MouseMove(sender As Object, e As MouseEventArgs) Handles if_replay.MouseMove
        SetReplayControlBorder(True)
    End Sub

    Private Sub if_replay_MouseLeave(sender As Object, e As EventArgs) Handles if_replay.MouseLeave
        SetReplayControlBorder(False)
    End Sub

    Private Sub SetReplayControlBorder(isVisible As Boolean)
        r_1.Visible = isVisible
        r_1r.Visible = isVisible
        r_1l.Visible = isVisible
        r_1b.Visible = isVisible
    End Sub

    Private Sub sh_replay_Click(sender As Object, e As EventArgs) Handles sh_replay.Click
        HandleReplayToggle()
    End Sub

    Private Sub replay_sc_Click(sender As Object, e As EventArgs) Handles replay_sc.Click
        HandleReplayToggle()
    End Sub

    Private Sub if_replay_Click(sender As Object, e As EventArgs) Handles if_replay.Click
        HandleReplayToggle()
    End Sub

    Private Sub HandleReplayToggle()
        a_1.Visible = False
        ToggleInstantReplay()
        sub_replay.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - REPLAY SAVE CONTROLS"

    Private Sub replay_sc1_MouseMove(sender As Object, e As MouseEventArgs) Handles replay_sc1.MouseMove
        SetReplaySaveBorder(True)
    End Sub

    Private Sub replay_sc1_MouseLeave(sender As Object, e As EventArgs) Handles replay_sc1.MouseLeave
        SetReplaySaveBorder(False)
    End Sub

    Private Sub Label7_MouseMove(sender As Object, e As MouseEventArgs) Handles Label7.MouseMove
        SetReplaySaveBorder(True)
    End Sub

    Private Sub Label7_MouseLeave(sender As Object, e As EventArgs) Handles Label7.MouseLeave
        SetReplaySaveBorder(False)
    End Sub

    Private Sub Label16_MouseMove(sender As Object, e As MouseEventArgs) Handles Label16.MouseMove
        SetReplaySaveBorder(True)
    End Sub

    Private Sub Label16_MouseLeave(sender As Object, e As EventArgs) Handles Label16.MouseLeave
        SetReplaySaveBorder(False)
    End Sub

    Private Sub SetReplaySaveBorder(isVisible As Boolean)
        rs1.Visible = isVisible
        rsl.Visible = isVisible
        rsr.Visible = isVisible
        rsb.Visible = isVisible
    End Sub

    Private Sub replay_sc1_Click(sender As Object, e As EventArgs) Handles replay_sc1.Click
        SaveReplay()
    End Sub

    Private Sub Label7_Click(sender As Object, e As EventArgs) Handles Label7.Click
        SaveReplay()
    End Sub

    Private Sub Label16_Click(sender As Object, e As EventArgs) Handles Label16.Click
        SaveReplay()
        a_1.Visible = False
    End Sub

    Private Sub SaveReplay()
        a_1.Visible = False
        If Replay_value Then
            ' Replay เปิดอยู่ - สามารถบันทึกได้
            ShowNotifier("saved_last_15")
        Else
            ' Replay ปิดอยู่ - ต้องเปิดก่อน
            ShowNotifier("replay_turn_on")
        End If
        sub_replay.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - RECORD CONTROLS"

    Private Sub sh_record_MouseMove(sender As Object, e As MouseEventArgs) Handles sh_record.MouseMove
        SetRecordControlBorder(True)
    End Sub

    Private Sub sh_record_MouseLeave(sender As Object, e As EventArgs) Handles sh_record.MouseLeave
        SetRecordControlBorder(False)
    End Sub

    Private Sub PictureBox5_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox5.MouseMove
        SetRecordControlBorder(True)
    End Sub

    Private Sub PictureBox5_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox5.MouseLeave
        SetRecordControlBorder(False)
    End Sub

    Private Sub Label13_MouseMove(sender As Object, e As MouseEventArgs) Handles Label13.MouseMove
        SetRecordControlBorder(True)
    End Sub

    Private Sub Label13_MouseLeave(sender As Object, e As EventArgs) Handles Label13.MouseLeave
        SetRecordControlBorder(False)
    End Sub

    Private Sub SetRecordControlBorder(isVisible As Boolean)
        st1.Visible = isVisible
        str.Visible = isVisible
        stl.Visible = isVisible
        stb.Visible = isVisible
    End Sub

    Private Sub sh_record_Click(sender As Object, e As EventArgs) Handles sh_record.Click
        HandleRecordToggle()
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        HandleRecordToggle()
    End Sub

    Private Sub Label13_Click(sender As Object, e As EventArgs) Handles Label13.Click
        HandleRecordToggle()
    End Sub

    Private Sub HandleRecordToggle()
        a_2.Visible = False
        ToggleRecording()
        sub_record.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - PHOTO MODE"

    Private Sub SetPhotoColors(color As Color)
        Logo_Mode2.ForeColor = color
        Text_Mode2.ForeColor = color
        Bg_Mode2.ForeColor = color
    End Sub

    Private Sub SetPhotoBorder(isVisible As Boolean)
        s_2.Visible = isVisible
        s_2r.Visible = isVisible
        s_2l.Visible = isVisible
        s_2b.Visible = isVisible
    End Sub

    Private Sub Photo_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode2.MouseMove, Text_Mode2.MouseMove, Bg_Mode2.MouseMove
        SetPhotoBorder(True)
        Base_Background_Top.Bg_Mode2.Visible = True
    End Sub

    Private Sub Photo_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode2.MouseLeave, Text_Mode2.MouseLeave, Bg_Mode2.MouseLeave
        SetPhotoColors(Color.White)
        SetPhotoBorder(False)
        Base_Background_Top.Bg_Mode2.Visible = False
    End Sub

    Private Sub bg_pht_Click(sender As Object, e As EventArgs) Handles Bg_Mode2.Click
        ShowNotifier("photo_mode_error")
    End Sub

    Private Sub pht_Click(sender As Object, e As EventArgs) Handles Text_Mode2.Click
        ShowNotifier("photo_mode_error")
    End Sub

    Private Sub logo_pht_Click(sender As Object, e As EventArgs) Handles Logo_Mode2.Click
        ShowNotifier("photo_mode_error")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - GAME FILTER"

    Private Sub SetGameColors(color As Color)
        Logo_Mode3.ForeColor = color
        Text_Mode3.ForeColor = color
        Bg_Mode3.ForeColor = color
    End Sub

    Private Sub SetGameBorder(isVisible As Boolean)
        s_3.Visible = isVisible
        s_3r.Visible = isVisible
        s_3l.Visible = isVisible
        s_3b.Visible = isVisible
        Base_Background_Top.Bg_Mode3.Visible = isVisible
    End Sub

    Private Sub logo_gamef_MouseMove(sender As Object, e As MouseEventArgs) Handles Logo_Mode3.MouseMove
        SetGameBorder(True)
    End Sub

    Private Sub logo_gamef_MouseLeave(sender As Object, e As EventArgs) Handles Logo_Mode3.MouseLeave
        SetGameColors(Color.White)
        SetGameBorder(False)
    End Sub

    Private Sub game_f_MouseMove(sender As Object, e As MouseEventArgs) Handles Text_Mode3.MouseMove
        SetGameBorder(True)
    End Sub

    Private Sub game_f_MouseLeave(sender As Object, e As EventArgs) Handles Text_Mode3.MouseLeave
        SetGameColors(Color.White)
        SetGameBorder(False)
    End Sub

    Private Sub bg_gamef_MouseMove(sender As Object, e As MouseEventArgs) Handles Bg_Mode3.MouseMove
        SetGameBorder(True)
    End Sub

    Private Sub bg_gamef_MouseLeave(sender As Object, e As EventArgs) Handles Bg_Mode3.MouseLeave
        SetGameColors(Color.White)
        SetGameBorder(False)
    End Sub

    Private Sub logo_gamef_Click(sender As Object, e As EventArgs) Handles Logo_Mode3.Click, Text_Mode3.Click, Bg_Mode3.Click
        ShowNotifier("notworkgpu")
        isFunctionActive_f3 = True
        ToggleGameFilter()
        HideAllControls()
        isFunctionActive = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - UPLOAD/SHARE"

    Private Sub SetUploadBorder(isVisible As Boolean)
        h1.Visible = isVisible
        h1r.Visible = isVisible
        h1l.Visible = isVisible
        h1b.Visible = isVisible
    End Sub
    Private Sub pf_MouseMove_1(sender As Object, e As MouseEventArgs) Handles bg_fps.MouseMove, pf.MouseMove, logo_pf.MouseMove
        Base_Background_Top.Bg_SET1.Visible = True
        SetUploadBorder(True)
    End Sub

    Private Sub pf_MouseLeave_1(sender As Object, e As EventArgs) Handles bg_fps.MouseLeave, pf.MouseLeave, logo_pf.MouseLeave
        Base_Background_Top.Bg_SET1.Visible = False
        SetUploadBorder(False)
    End Sub

    Private Sub pf_Click(sender As Object, e As EventArgs) Handles pf.Click
        OpenUploadPanel()
    End Sub

    Private Sub bg_fps_Click(sender As Object, e As EventArgs) Handles bg_fps.Click
        OpenUploadPanel()
    End Sub

    Private Sub logo_pf_Click(sender As Object, e As EventArgs) Handles logo_pf.Click
        OpenUploadPanel()
    End Sub

    Private Sub OpenUploadPanel()
        HideAllControls()
        If Opacity = 0 Then
            Base_www.Timer1.Stop()
            Base_www.Timer2.Start()
            Base_Background.Timer1.Stop()
            Base_Background.Timer2.Start()
        End If
        Base_www.Show()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - SETTINGS PANEL"

    Private Sub SW_lang_MouseLeave(sender As Object, e As EventArgs) Handles SW_lang.MouseLeave
        SW_lang.BackColor = Color.FromArgb(38, 43, 47)
    End Sub

    Private Sub SW_lang_MouseMove(sender As Object, e As MouseEventArgs) Handles SW_lang.MouseMove
        SW_lang.BackColor = Color.FromArgb(64, 64, 64)
    End Sub
    Private Sub SWchang_MouseLeave(sender As Object, e As EventArgs) Handles ch.MouseLeave
        ch.BackColor = Color.FromArgb(38, 43, 47)
    End Sub

    Private Sub SW_lanchg_MouseMove(sender As Object, e As MouseEventArgs) Handles ch.MouseMove
        ch.BackColor = Color.FromArgb(64, 64, 64)
    End Sub

    Private Sub VS_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox6.MouseMove, Label10.MouseMove
        'SetVSBorder(True)
    End Sub

    Private Sub VS_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox6.MouseLeave, Label10.MouseLeave
        'SetVSBorder(False)
    End Sub

    Private Sub VS_Click(sender As Object, e As EventArgs) Handles PictureBox6.Click, Label10.Click
        OpenRecordings()
    End Sub

    Private Sub SetS1Border(isVisible As Boolean)
        s1.Visible = isVisible
        s1r.Visible = isVisible
        s1l.Visible = isVisible
        s1b.Visible = isVisible
    End Sub
    Private Sub Label1_MouseMove(sender As Object, e As MouseEventArgs) Handles set_to.MouseMove, Label1.MouseMove, Label2.MouseMove
        Base_Background_Top.Bg_SET3.Visible = True
        SetS1Border(True)
    End Sub

    Private Sub Label1_MouseLeave(sender As Object, e As EventArgs) Handles set_to.MouseLeave, Label1.MouseLeave, Label2.MouseLeave
        Base_Background_Top.Bg_SET3.Visible = False
        SetS1Border(False)
    End Sub

    Private Sub Settings_Click(sender As Object, e As EventArgs) Handles Label1.Click, Label2.Click
        OpenSettings()
    End Sub

    Private Sub OpenSettings()
        Opacity = 1
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        settings_1.Visible = True
        Main_menu_list.Visible = False
        sub_replay.Visible = False
        sub_record.Visible = False
    End Sub

    Private Sub OpenRecordings()
        a_2.Visible = False
        Main_menu_list.Visible = False
        sub_record.Visible = False
        ALT_Z.Stop()
        ALT_SHIFT_F10.Stop()
        ALT_F9.Stop()
        Base_RecordingsSet.Show()
        Opacity = 1
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - PRIVACY SETTINGS"

    Private Sub saved_e_MouseMove(sender As Object, e As MouseEventArgs) Handles saved_e.MouseMove
        saved_e1.BackColor = greenColor
    End Sub

    Private Sub saved_e_MouseLeave(sender As Object, e As EventArgs) Handles saved_e.MouseLeave
        saved_e1.BackColor = Color.Gray
    End Sub

    Private Sub Label4_MouseMove(sender As Object, e As MouseEventArgs) Handles Label4.MouseMove
        saved_e1.BackColor = greenColor
    End Sub

    Private Sub Label4_MouseLeave(sender As Object, e As EventArgs) Handles Label4.MouseLeave
        saved_e1.BackColor = Color.Gray
    End Sub

    Private Sub Label5_MouseMove(sender As Object, e As MouseEventArgs) Handles Label5.MouseMove
        saved_e1.BackColor = greenColor
    End Sub

    Private Sub Label5_MouseLeave(sender As Object, e As EventArgs) Handles Label5.MouseLeave
        saved_e1.BackColor = Color.Gray
    End Sub

    Private Sub saved_e_Click(sender As Object, e As EventArgs) Handles saved_e.Click
        OpenPrivacySettings()
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click
        OpenPrivacySettings()
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        OpenPrivacySettings()
    End Sub

    Private Sub OpenPrivacySettings()
        ALT_Z.Stop()
        settings_1.Visible = False
        Base_Privacy_Control.Show()
        Base_Privacy_Control.WindowState = FormWindowState.Maximized
        Base_Privacy_Control.settings_1.Visible = True
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - OVERLAY HUB"

    Private Sub PictureBox10_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox10.MouseMove
        hub.BackColor = greenColor
    End Sub

    Private Sub PictureBox10_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox10.MouseLeave
        hub.BackColor = Color.Gray
    End Sub

    Private Sub Label12_MouseMove(sender As Object, e As MouseEventArgs) Handles Label12.MouseMove
        hub.BackColor = greenColor
    End Sub

    Private Sub Label12_MouseLeave(sender As Object, e As EventArgs) Handles Label12.MouseLeave
        hub.BackColor = Color.Gray
    End Sub

    Private Sub Label15_MouseMove(sender As Object, e As MouseEventArgs) Handles Label15.MouseMove
        hub.BackColor = greenColor
    End Sub

    Private Sub Label15_MouseLeave(sender As Object, e As EventArgs) Handles Label15.MouseLeave
        hub.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox10_Click(sender As Object, e As EventArgs) Handles PictureBox10.Click
        OpenOverlayHub()
    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles Label12.Click
        OpenOverlayHub()
    End Sub

    Private Sub Label15_Click(sender As Object, e As EventArgs) Handles Label15.Click
        OpenOverlayHub()
    End Sub

    Private Sub OpenOverlayHub()
        Base_Overlay_Hub.Show()
        settings_1.Visible = False
        Base_Overlay_Hub.settings_1.Visible = True
        ALT_Z.Stop()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - KEYBOARD SHORTCUTS"

    Private Sub K1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox11.MouseMove, Label17.MouseMove, Label18.MouseMove
        k1.BackColor = greenColor
    End Sub

    Private Sub K1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox11.MouseLeave, Label17.MouseLeave, Label18.MouseLeave
        k1.BackColor = Color.Gray
    End Sub

    Private Sub K1_Click(sender As Object, e As EventArgs) Handles PictureBox11.Click, Label17.Click, Label18.Click
        OpenKeySettings()
    End Sub

    Private Sub OpenKeySettings()
        ALT_Z.Stop()
        Base_KeySet.Show()
        settings_1.Visible = False
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - HIGHLIGHTS"

    Private Sub hg2_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox16.MouseMove, Label21.MouseMove, Label22.MouseMove
        hg2.BackColor = greenColor
    End Sub

    Private Sub hg2_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox16.MouseLeave, Label21.MouseLeave, Label22.MouseLeave
        hg2.BackColor = Color.Gray
    End Sub

    Private Sub FeatureNotReady_Click(sender As Object, e As EventArgs) Handles PictureBox16.Click, Label21.Click, Label22.Click
        ShowNotifier("feature_not_ready")
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - VIDEO CAPTURE SETTINGS"

    Private Sub vd1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox13.MouseMove, vdo_setme.MouseMove, Label19.MouseMove, Label20.MouseMove
        vd1.BackColor = greenColor
    End Sub

    Private Sub vd1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox13.MouseLeave, vdo_setme.MouseLeave, Label19.MouseLeave
        vd1.BackColor = Color.Gray
    End Sub

    Private Sub vd1_Click(sender As Object, e As EventArgs) Handles PictureBox13.Click, vdo_setme.Click, Label19.Click, Label20.Click
        OpenRecordingSettings()
    End Sub

    Private Sub OpenRecordingSettings()
        settings_1.Visible = False
        ALT_Z.Stop()
        ALT_SHIFT_F10.Stop()
        ALT_F9.Stop()
        Base_RecordingsSet.Show()
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - NOTIFICATIONS"

    Private Sub noy_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox17.MouseMove, nott.MouseMove, noty.MouseMove
        noy.BackColor = greenColor
    End Sub

    Private Sub noy_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox17.MouseLeave, nott.MouseLeave
        noy.BackColor = Color.Gray
    End Sub

    Private Sub Noti_Click(sender As Object, e As EventArgs) Handles PictureBox17.Click, nott.Click, noty.Click
        ToggleNoti()
    End Sub

    Private Sub ToggleNoti()
        isNotiOn = Not isNotiOn
        Dim targetColor As Color = If(isNotiOn, Color.White, Color.Gray)
        noty.ForeColor = targetColor
        nott.ForeColor = targetColor
    End Sub

#End Region

#Region "============================================================================ MOUSE EVENT HANDLERS - ABOUT"

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        ab_bg.BackColor = greenColor
    End Sub

    Private Sub PictureBox1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave
        ab_bg.BackColor = Color.Gray
    End Sub

    Private Sub Label6_MouseMove(sender As Object, e As MouseEventArgs) Handles Label6.MouseMove
        ab_bg.BackColor = greenColor
    End Sub

    Private Sub Label6_MouseLeave(sender As Object, e As EventArgs) Handles Label6.MouseLeave
        ab_bg.BackColor = Color.Gray
    End Sub

    Private Sub Label9_MouseMove(sender As Object, e As MouseEventArgs) Handles Label9.MouseMove
        ab_bg.BackColor = greenColor
    End Sub

    Private Sub Label9_MouseLeave(sender As Object, e As EventArgs) Handles Label9.MouseLeave
        ab_bg.BackColor = Color.Gray
    End Sub

#End Region

#Region "============================================================================ TIMER EVENT HANDLERS"

    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load_App.Tick





        If sub_record.Visible = True Then
            Base_Background_Top.b2_all.Visible = True
        Else
            Base_Background_Top.b2_all.Visible = False
        End If
        If sub_replay.Visible = True Then
            Base_Background_Top.b1_all.Visible = True
        Else
            Base_Background_Top.b1_all.Visible = False
        End If

        AlignPanelToTop()
        UpdateReplayStatus()
        UpdateRecordStatus()
        UpdateMicStatus()
        Dim filePaths As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier_main")

        Try
            If Base_Notifier.Visible Then
                If Not File.Exists(filePaths) Then
                    File.Create(filePaths).Dispose()
                End If
            Else
                If File.Exists(filePaths) Then
                    File.Delete(filePaths)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub py_cc_Tick(sender As Object, e As EventArgs) Handles Privacy_control.Tick
        Dim privacyPath As String = Path.Combine(Application.StartupPath, DataDirectoryName, PrivacyFile)

        If My.Computer.FileSystem.FileExists(privacyPath) Then
            ' Privacy control เปิดอยู่
            Base_Privacy_Control.py_2.Text = LangHelper.GetText("l10n.instantReplayStop")
        Else
            ' Privacy control ปิดอยู่ - รีเซ็ตทุกสถานะ

            ' รีเซ็ต Instant Replay
            If Replay_value Then
                Replay_value = False
                logo_replay.ForeColor = Color.White
                if_replay.Text = LangHelper.GetText("l10n.instantReplayStart")
            End If

            ' รีเซ็ต Recording
            If logo_record.ForeColor = greenColor Then
                logo_record.ForeColor = Color.White
            End If

            Label13.Text = LangHelper.GetText("l10n.start")
            s_record.Text = LangHelper.GetText("l10n.notRecording")
            s_record.ForeColor = Color.Gray

            Base_Privacy_Control.py_2.Text = LangHelper.GetText("l10n.instantReplayStart")
        End If
    End Sub

    Private Sub hg1_Tick(sender As Object, e As EventArgs) Handles hg1.Tick
        If My.Computer.FileSystem.FileExists(Path.Combine(Application.StartupPath, DataDirectoryName, "save")) Then
            hg1.Stop()
            Return
        End If

        ' Capture screen and detect target color
        Using bmpScreenshot As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
            Using g As Graphics = Graphics.FromImage(bmpScreenshot)
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)
            End Using

            Dim targetColor As Color = ColorTranslator.FromHtml("#ACB22E")

            For x As Integer = 0 To bmpScreenshot.Width - 1
                For y As Integer = 0 To bmpScreenshot.Height - 1
                    If bmpScreenshot.GetPixel(x, y) = targetColor Then
                        If Not My.Computer.FileSystem.FileExists(Application.StartupPath & DataDirectoryName & "/save") Then
                            ShowNotifier("saved_last_15")
                            hg1.Stop()
                        End If
                        Exit Sub
                    End If
                Next
            Next
        End Using
    End Sub

    Private Sub not_save_Tick(sender As Object, e As EventArgs) Handles not_save.Tick
        File.Delete(Path.Combine(Application.StartupPath, DataDirectoryName, "save"))
        hg1.Start()
    End Sub


#End Region

#Region "============================================================================ STATUS UPDATE METHODS"

    Private Sub UpdateRecordStatus()
        If logo_record.ForeColor = greenColor OrElse logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
            Label13.Text = LangHelper.GetText("l10n.stopAndSave")
            s_record.Text = LangHelper.GetText("l10n.recording")
            s_record.ForeColor = greenColor
            s_record.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        Else
            Label13.Text = LangHelper.GetText("l10n.start")
            s_record.Text = LangHelper.GetText("l10n.notRecording")
            s_record.ForeColor = Color.Gray
            s_record.Font = New Font("Segoe UI", 12, FontStyle.Regular)
        End If

    End Sub

    Private Sub UpdateReplayStatus()
        Dim dataPath As String = Application.StartupPath & DataDirectoryName & "/"
        If Replay_value Then
            s_replay.Text = LangHelper.GetText("l10n.on")
            s_replay.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            s_replay.ForeColor = greenColor
            logo_replay.ForeColor = greenColor
            Label8.ForeColor = Color.White
        Else
            s_replay.Text = LangHelper.GetText("l10n.off")
            s_replay.Font = New Font("Segoe UI", 12, FontStyle.Regular)
            s_replay.ForeColor = Color.Gray
            logo_replay.ForeColor = Color.White
            Label8.ForeColor = Color.Gray
        End If
    End Sub

    Private Sub UpdateMicStatus()

        If mic.Text = "" Then
            My.Settings.MicStatus = True
            My.Settings.Save()
        Else
            My.Settings.MicStatus = False
            My.Settings.Save()
        End If
    End Sub

#End Region

#Region "============================================================================ GAME DETECTION"

    Private Sub GAMES_IN_Tick(sender As Object, e As EventArgs) Handles GAMES_IN.Tick
        Dim processes() As Process = Process.GetProcesses()
        Dim targetGames As String() = {
            "minecraft", "javaw", "robloxplayerbeta", "robloxcrashhandler", "java",
            "crashhandler", "gta5", "hd-player", "a dance of fire and ice", "aot",
            "aot2_as", "iw5mp", "iw5sp", "obscure", "genshinimpact", "gta5_enhanced",
            "dwrg", "dungeons", "minecraftlegends.windows", "secret neighbour",
            "smash_legends", "asphalt9_steam_x64_rtl", "furmark_gui"
        }

        Dim isGameRunning As Boolean = False

        For Each proc In processes
            Dim procName As String = proc.ProcessName.ToLower()
            For Each gameName As String In targetGames
                If procName = gameName Then
                    isGameRunning = True
                    Exit For
                End If
            Next
            If isGameRunning Then Exit For
        Next

        ' Show notification once when game is detected
        If isGameRunning AndAlso Not notifierShown Then
            ShowNotifier("game_n")
            notifierShown = True
        ElseIf Not isGameRunning Then
            notifierShown = False
        End If
    End Sub

#End Region

#Region "============================================================================ MISC EVENT HANDLERS"

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Opacity = 0.85
        settings_1.Visible = False
        Main_menu_list.Visible = True
    End Sub


    Private Sub action_fn_MouseLeave(sender As Object, e As EventArgs) Handles action_fn.MouseLeave
        action_fn.BackColor = Color.FromArgb(118, 185, 0)
    End Sub


    Private Sub action_fn_MouseMove(sender As Object, e As EventArgs) Handles action_fn.MouseMove
        action_fn.BackColor = Color.FromArgb(0, 192, 0)
    End Sub

    Private Sub Logo_Click(sender As Object, e As EventArgs) Handles Logo.Click
        File.Create(Application.StartupPath & DataDirectoryName & "\game_n-api").Dispose()
    End Sub

    Private Sub Logo_text_DoubleClick(sender As Object, e As EventArgs)
        Application.Restart()
    End Sub

    Private Sub Base_Click(sender As Object, e As EventArgs) Handles MyBase.Click
        HandleGalleryDisplay()
    End Sub

    Private Sub HandleGalleryDisplay()
        If Base_Gallery.settings_1.Visible = True Then
            Base_Gallery.Show()
            Base_Gallery.TopMost = True
        End If
    End Sub

    Private Sub save_sc_Click(sender As Object, e As EventArgs)
        Using folderDlg As New FolderBrowserDialog
            folderDlg.Description = "Select the folder to save the capture."
            If folderDlg.ShowDialog = DialogResult.OK Then
                Base_Gallery.txtFilePath.Text = folderDlg.SelectedPath
                My.Settings.SavePath = Base_Gallery.txtFilePath.Text
                My.Settings.Save()
            End If
        End Using
    End Sub

    Private Sub SW_lang_Click(sender As Object, e As EventArgs) Handles SW_lang.Click

        Dim langFolder = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile = Path.Combine(langFolder, "current.txt")

        ' อ่านค่าปัจจุบัน
        Dim currentLang = "en-US"
        If File.Exists(currentFile) Then currentLang = File.ReadAllText(currentFile).Trim

        ' สลับภาษา
        Dim newLang As String
        Select Case currentLang
            Case "en-US"
                newLang = "th-TH"
            Case "th-TH"
                newLang = "zh-CHS"
            Case Else
                newLang = "en-US"
        End Select

        ' บันทึก
        File.WriteAllText(currentFile, newLang)

        ' โหลดภาษาใหม่
        Dim langFile = Path.Combine(langFolder, newLang & ".json")
        LoadLang(langFile)

        ' อัปเดต UI
        UpdateLocalizedTexts()

        ' ตั้งชื่อปุ่มจาก JSON
        SW_lang.Text = GetText("meta.languageName")

    End Sub
    Private Sub ch_Click(sender As Object, e As EventArgs) Handles ch.Click
        ch.Enabled = False
        CheckForUpdateAsync()
    End Sub

    Private Sub PictureBox19_MouseMove(sender As Object, e As MouseEventArgs) Handles menu_record_sub.MouseMove, menu_record_subkey.MouseMove
        menu_record_subbg.BackColor = greenColor
    End Sub

    Private Sub PictureBox19_MouseLeave(sender As Object, e As EventArgs) Handles menu_record_sub.MouseLeave, menu_record_subkey.MouseLeave
        menu_record_subbg.BackColor = Color.Black
    End Sub

    Private Sub ME_CLOSE_BG_MouseMove(sender As Object, e As MouseEventArgs) Handles ME_CLOSE_BG.MouseMove, d.MouseMove
        Base_Background_Top.ME_CLOSE_BG_GRE.BackColor = greenColor
    End Sub

    Private Sub ME_CLOSE_BG_MouseLeave(sender As Object, e As EventArgs) Handles ME_CLOSE_BG.MouseLeave, d.MouseLeave
        Base_Background_Top.ME_CLOSE_BG_GRE.BackColor = Color.Black
    End Sub

    Private Sub ME_CLOSE_BG_Click(sender As Object, e As EventArgs) Handles ME_CLOSE_BG.Click, d.Click
        HideAllControls()
    End Sub
    Private Sub PictureBox24_Click(sender As Object, e As EventArgs) Handles sub_replay_setodv.Click, Label3.Click
        OpenRecordings()
    End Sub

    Private Sub Main_menu_Paint(sender As Object, e As PaintEventArgs) Handles Main_menu.Paint

    End Sub

    Private Sub logo_replay_MouseHover(sender As Object, e As EventArgs) Handles logo_replay.MouseHover
        If Base_Background_Top.b2_all.Visible = True Then
            sub_replay.Visible = Not sub_replay.Visible
            sub_record.Visible = False
            a_1.Visible = Not a_1.Visible
            a_2.Visible = False
            a_3.Visible = False
        End If
    End Sub

    Private Sub logo_record_MouseHover(sender As Object, e As EventArgs) Handles logo_record.MouseHover
        If Base_Background_Top.b1_all.Visible = True Then
            sub_record.Visible = Not sub_record.Visible
            sub_replay.Visible = False
            a_2.Visible = True
            a_1.Visible = False
            a_3.Visible = False
        End If
    End Sub




#End Region

End Class