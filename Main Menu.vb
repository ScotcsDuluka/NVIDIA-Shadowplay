Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Drawing
Imports Microsoft.Win32
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Diagnostics
Imports System.Windows.Forms
Imports System.Runtime
Imports Newtonsoft.Json
Imports System.Data.SqlClient

Public Class Base

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

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

    Private Sub StartVideoRecording()

    End Sub

    Private Sub StopVideoRecording()

    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' ซ่อนฟอร์มจาก Alt+Tab
        HideFromAltTab()

        ' เปิดการใช้งาน Double Buffering เพื่อลดการกระพริบของฟอร์ม
        Me.DoubleBuffered = True

        ' แสดง bg_top เมื่อเริ่มการโหลด
        bg_top.Show()

        ' เริ่ม Python process และซ่อนฟอร์มหลัก
        py_cc.Start()

        ' เริ่ม Timer สำหรับการตรวจสอบการทำงาน
        ch_t.Start()

        ' โหลดพาธสำหรับบันทึกข้อมูล
        LoadFilePath()

        ' สร้างเส้นทางสำหรับ Data หากยังไม่มี
        CreateDataDirectories()

        ' โหลดข้อมูลสำหรับการตั้งค่าไมโครโฟน
        LoadMicrophoneData()

        ' ตรวจสอบการควบคุมความเป็นส่วนตัว
        CheckPrivacyControl()

        ' โหลดการตั้งค่าสำหรับ Replay
        LoadReplaySettings()

        ' เริ่มการบันทึกข้อมูล
        Load.Start()

        ' ซ่อน bg_top เมื่อการโหลดเสร็จสมบูรณ์
        bg_top.Hide()
    End Sub
    Private Sub LoadFilePath()
        Gallery_1.txtFilePath.Text = My.Settings.SavePath
        If String.IsNullOrEmpty(Gallery_1.txtFilePath.Text) Then
            Gallery_1.txtFilePath.Text = ("C:\Shadowplay")
        End If

        Dim directoryPath As String = Gallery_1.txtFilePath.Text
        If Not Directory.Exists(directoryPath) Then
            Directory.CreateDirectory(directoryPath) ' สร้างโฟลเดอร์ถ้ายังไม่มี
        End If
    End Sub

    Private Sub CreateDataDirectories()
        Dim basePath As String = Application.StartupPath & "NVIDIA_Shadowplay_Data/"
        My.Computer.FileSystem.CreateDirectory(basePath & "Replay")
        My.Computer.FileSystem.CreateDirectory(basePath & "Record")
        My.Computer.FileSystem.CreateDirectory(basePath & "Live")
        My.Computer.FileSystem.CreateDirectory(basePath & "mic")
    End Sub

    Private Sub LoadMicrophoneData()
        mic.Text = If(My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/mic/mic_on"), "", "")
    End Sub

    Private Sub CheckPrivacyControl()
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
            py.py_2.Text = ("Turn off")
        Else
            py.py_2.Text = ("Turn on")
        End If
    End Sub

    Private Sub LoadReplaySettings()
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/Replay/on") Then
            replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
            if_replay.Text = ("Turn off")
            replay_on.Visible = True
            s_replay.Text = ("on")
            s_replay.ForeColor = ColorTranslator.FromHtml("#76B900")
        Else
            replay_on.ForeColor = Color.White
            replay_on.Visible = False
            if_replay.Text = ("Turn on")
            s_replay.Text = ("off")
            s_replay.ForeColor = Color.White
        End If
    End Sub

    Private Const AppName As String = "NVIDIA Shadowplay™"

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

    Private Sub CaptureScreen()
        If Not My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
            Notifier.Show()
            Return
        End If

        Dim filePath As String = My.Settings.SavePath
        If String.IsNullOrWhiteSpace(filePath) Then
            ShowNotifier("Please select a valid save path for screenshots.", "")
            Return
        End If

        Try
            Using bmpScreenshot As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
                Using g As Graphics = Graphics.FromImage(bmpScreenshot)
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)
                End Using

                Dim fileName As String = Path.Combine(filePath, $"Shadowplay Screenshot {DateTime.Now:dd_MM_ss}.png")
                bmpScreenshot.Save(fileName, System.Drawing.Imaging.ImageFormat.Png)

                If Directory.Exists(filePath) Then
                    ShowNotifier("Screenshot has been saved to Gallery", "")
                Else
                    ShowNotifier("Please select a valid save path for screenshots.", "")
                End If
            End Using

        Catch ex As Exception
            ShowNotifier(ex.Message, "")
        End Try
    End Sub

    Private Sub ShowNotifier(message As String, iconText As String)
        Notifier.Show()
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.Text = iconText
        Notifier.text_n.Text = message
    End Sub

    Private Sub MainSub_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializePanelSizes()
        StartKeyDetection()

    End Sub

    Private Sub InitializePanelSizes()
        action_sc.Size = New Size(600, 300)
        settings_1.Size = New Size(1010, 600)
        action.Size = New Size(1100, 200)
    End Sub

    Private Sub StartKeyDetection()
        StartKeyTimer(alt_z, 1) ' Alt + Z
        StartKeyTimer(alt_f1, 1) ' Alt + F1
        StartKeyTimer(alt_shift_f10, 1) ' Alt + Shift + F10
        StartKeyTimer(save, 1) ' Alt + F10
        StartKeyTimer(record_1, 1) ' Alt + F9
        StartKeyTimer(w, 1) ' W
    End Sub

    Private Sub StartKeyTimer(timer As Timer, interval As Integer)
        timer.Interval = interval
        timer.Start()
    End Sub



    <DllImport("user32.dll")>
    Private Shared Function GetAsyncKeyState(vKey As Integer) As Short
    End Function


    Private Const VK_LBUTTON As Integer = &H1 ' ปุ่มซ้ายของเมาส์
    Private Const VK_RBUTTON As Integer = &H2 ' ปุ่มขวาของเมาส์
    Private Const VK_CANCEL As Integer = &H3 ' Cancel key
    Private Const VK_MBUTTON As Integer = &H4 ' ปุ่มกลางของเมาส์
    Private Const VK_BACK As Integer = &H8 ' Backspace key
    Private Const VK_TAB As Integer = &H9 ' Tab key
    Private Const VK_CLEAR As Integer = &HC ' Clear key
    Private Const VK_RETURN As Integer = &HD ' Enter key
    Private Const VK_SHIFT As Integer = &H10 ' Shift key
    Private Const VK_CONTROL As Integer = &H11 ' Control key
    Private Const VK_ALT As Integer = &H12 ' Alt key
    Private Const VK_PAUSE As Integer = &H13 ' Pause key
    Private Const VK_CAPITAL As Integer = &H14 ' Caps Lock key
    Private Const VK_ESCAPE As Integer = &H1B ' Escape key
    Private Const VK_SPACE As Integer = &H20 ' Spacebar
    Private Const VK_PAGEUP As Integer = &H21 ' Page Up key
    Private Const VK_PAGEDOWN As Integer = &H22 ' Page Down key
    Private Const VK_END As Integer = &H23 ' End key
    Private Const VK_HOME As Integer = &H24 ' Home key
    Private Const VK_LEFT As Integer = &H25 ' Left arrow key
    Private Const VK_UP As Integer = &H26 ' Up arrow key
    Private Const VK_RIGHT As Integer = &H27 ' Right arrow key
    Private Const VK_DOWN As Integer = &H28 ' Down arrow key
    Private Const VK_SELECT As Integer = &H29 ' Select key
    Private Const VK_PRINT As Integer = &H2A ' Print key
    Private Const VK_EXECUTE As Integer = &H2B ' Execute key
    Private Const VK_SNAPSHOT As Integer = &H2C ' Print Screen key
    Private Const VK_INSERT As Integer = &H2D ' Insert key
    Private Const VK_DELETE As Integer = &H2E ' Delete key
    Private Const VK_HELP As Integer = &H2F ' Help key


    Private Const VK_A As Integer = &H41 ' A key
    Private Const VK_B As Integer = &H42 ' B key
    Private Const VK_C As Integer = &H43 ' C key
    Private Const VK_D As Integer = &H44 ' D key
    Private Const VK_E As Integer = &H45 ' E key
    Private Const VK_F As Integer = &H46 ' F key
    Private Const VK_G As Integer = &H47 ' G key
    Private Const VK_H As Integer = &H48 ' H key
    Private Const VK_I As Integer = &H49 ' I key
    Private Const VK_J As Integer = &H4A ' J key
    Private Const VK_K As Integer = &H4B ' K key
    Private Const VK_L As Integer = &H4C ' L key
    Private Const VK_M As Integer = &H4D ' M key
    Private Const VK_N As Integer = &H4E ' N key
    Private Const VK_O As Integer = &H4F ' O key
    Private Const VK_P As Integer = &H50 ' P key
    Private Const VK_Q As Integer = &H51 ' Q key
    Private Const VK_R As Integer = &H52 ' R key
    Private Const VK_S As Integer = &H53 ' S key
    Private Const VK_T As Integer = &H54 ' T key
    Private Const VK_U As Integer = &H55 ' U key
    Private Const VK_V As Integer = &H56 ' V key         
    Private Const VK_W As Integer = &H57 ' W key
    Private Const VK_X As Integer = &H58 ' X key
    Private Const VK_Y As Integer = &H59 ' Y key
    Private Const VK_Z As Integer = &H5A ' Z key


    Private Const VK_F1 As Integer = &H70 ' F1 key
    Private Const VK_F2 As Integer = &H71 ' F2 key
    Private Const VK_F3 As Integer = &H72 ' F3 key
    Private Const VK_F4 As Integer = &H73 ' F4 key
    Private Const VK_F5 As Integer = &H74 ' F5 key
    Private Const VK_F6 As Integer = &H75 ' F6 key
    Private Const VK_F7 As Integer = &H76 ' F7 key
    Private Const VK_F8 As Integer = &H77 ' F8 key
    Private Const VK_F9 As Integer = &H78 ' F9 key
    Private Const VK_F10 As Integer = &H79 ' F10 key
    Private Const VK_F11 As Integer = &H7A ' F11 key
    Private Const VK_F12 As Integer = &H7B ' F12 key



    Private isFunctionActive As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_F1 As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_F1 As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_replay As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_replay As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_replay_save As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_replay_save As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_record As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_record As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_p As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_p As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_f2 As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_f2 As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_f3 As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_f3 As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActive_f8 As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัน
    Private isKeyPressed_f8 As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private Sub alt_F_1_2_Tick(sender As Object, e As EventArgs) Handles alt_F_1_2.Tick 'Photo Mode
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F8) And &H8000) <> 0 Then
            If Not isKeyPressed_f8 Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_f8 = Not isFunctionActive_f8 ' สลับสถานะฟังก์ชัน
                If isFunctionActive_f8 Then
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("This feature not ready")
                Else
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("This feature not ready")
                End If
                isKeyPressed_f8 = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_f8 = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F2) And &H8000) <> 0 Then
            If Not isKeyPressed_f2 Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_f2 = Not isFunctionActive_f2 ' สลับสถานะฟังก์ชัน
                If isFunctionActive_f2 Then
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("Photo Mode Not working Current GPU")
                Else
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("Photo Mode Not working Current GPU")
                End If
                isKeyPressed_f2 = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_f2 = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F3) And &H8000) <> 0 Then
            If Not isKeyPressed_f3 Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_f3 = Not isFunctionActive_f3 ' สลับสถานะฟังก์ชัน
                If isFunctionActive_f3 Then
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("Game Filter Not working Current GPU")
                Else
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("Game Filter Not working Current GPU")
                End If
                isKeyPressed_f3 = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_f3 = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub

    Private Sub w_Tick(sender As Object, e As EventArgs) Handles w.Tick 'Shared
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F12) And &H8000) <> 0 Then
            If Not isKeyPressed_p Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_p = Not isFunctionActive_p ' สลับสถานะฟังก์ชัน
                If isFunctionActive_p Then
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("This feature not ready")
                Else
                    Notifier.Show()
                    Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                    Notifier.icon_n.ForeColor = Color.White
                    Notifier.icon_n.Text = ("")
                    Notifier.text_n.Text = ("This feature not ready")
                End If
                isKeyPressed_p = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_p = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub
    Private Sub record_1_Tick(sender As Object, e As EventArgs) Handles record_1.Tick 'Alt + F9 - Record

        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F9) And &H8000) <> 0 Then
            If Not isKeyPressed_record Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_record = Not isFunctionActive_record ' สลับสถานะฟังก์ชัน
                If isFunctionActive_record Then
                    If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
                        If Notifier.text_n.Text = "Recording has started" Or Notifier.text_n.Text = "Recording has started " Then
                            ShowNotifier("", "Recording has started ", ColorTranslator.FromHtml("#76B900"), 40)
                        Else
                            StopVideoRecording()
                            ShowNotifier("", "Recording has been saved", Color.White, 40)
                            logo_record.ForeColor = Color.White
                        End If
                    Else
                        If Notifier.text_n.Text = "Recording has been saved" Or Notifier.text_n.Text = "Recording has been saved " Then
                            ShowNotifier("", "Recording has been saved ", Color.White, 40)
                        Else
                            StartVideoRecording()
                            ShowNotifier("", "Recording has started", ColorTranslator.FromHtml("#76B900"), 40)
                            logo_record.ForeColor = ColorTranslator.FromHtml("#76B900")
                        End If
                    End If
                Else
                    If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
                        If Notifier.text_n.Text = "Recording has started" Or Notifier.text_n.Text = "Recording has started " Then
                            ShowNotifier("", "Recording has started ", ColorTranslator.FromHtml("#76B900"), 40)
                        Else
                            StopVideoRecording()
                            ShowNotifier("", "Recording has been saved", Color.White, 40)
                            logo_record.ForeColor = Color.White
                        End If
                    Else
                        If Notifier.text_n.Text = "Recording has been saved" Or Notifier.text_n.Text = "Recording has been saved " Then
                            ShowNotifier("", "Recording has been saved ", Color.White, 40)
                        Else
                            StartVideoRecording()
                            ShowNotifier("", "Recording has started", ColorTranslator.FromHtml("#76B900"), 40)
                            logo_record.ForeColor = ColorTranslator.FromHtml("#76B900")
                        End If
                    End If
                End If
                isKeyPressed_record = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_record = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub

    Private Sub ShowNotifier(iconText As String, message As String, iconColor As Color, fontSize As Integer)
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, fontSize)
        Notifier.icon_n.ForeColor = iconColor
        Notifier.icon_n.Text = iconText
        Notifier.text_n.Text = message
    End Sub

    Private Sub save_Tick(sender As Object, e As EventArgs) Handles save.Tick 'Alt + F10 - Saved Replay

        ' ตรวจสอบว่าทั้ง Alt และ F10 ถูกกดพร้อมกัน                                                                       wwww
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F10) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_SHIFT) And &H8000) = 0 Then
            ' ตรวจสอบว่าปุ่มยังไม่ถูกกดอยู่
            If Not isKeyPressed_replay_save Then
                ' สลับสถานะฟังก์ชัน Instant Replay (เปิด/ปิด)
                isFunctionActive_replay_save = Not isFunctionActive_replay_save
                If isFunctionActive_replay_save Then
                    ' กรณีปิด Instant Replay
                    If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
                        Notifier.Show()
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Erorr to saved last {{replay_save}}."
                        'Notifier.text_n.Text = "Saved last {{replay_saved}} seconds."
                    Else
                        Notifier.Show()
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Turn on instant replay to save the last."
                    End If
                Else
                    If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
                        Notifier.Show()
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Erorr to saved last {{replay_save}}."
                    Else
                        Notifier.Show()
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Turn on instant replay to save the last."
                    End If
                End If
                ' ตั้งค่าสถานะว่าปุ่มถูกกดแล้วเพื่อป้องกันการเรียกซ้ำ
                isKeyPressed_replay_save = True
            End If
        Else
            ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
            isKeyPressed_replay_save = False
        End If
    End Sub
    Private Sub alt_shift_f10_Tick(sender As Object, e As EventArgs) Handles alt_shift_f10.Tick 'Alt + Shift + F10 - Relpay

        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_SHIFT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F10) And &H8000) <> 0 Then
            If Not isKeyPressed_replay Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_replay = Not isFunctionActive_replay ' สลับสถานะฟังก์ชัน
                If isFunctionActive_replay Then
                    If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
                        replay_on.Visible = False
                        replay_on.ForeColor = Color.White
                        if_replay.Text = "Turn on"
                        Notifier.Show()
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Instant Replay is now off"
                    Else
                        replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
                        if_replay.Text = "Turn off"
                        replay_on.Visible = True
                        Notifier.Show()
                        Notifier.icon_n.ForeColor = ColorTranslator.FromHtml("#76B900")
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Instant Replay is now on"
                    End If

                Else
                    If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
                        replay_on.Visible = False
                        replay_on.ForeColor = Color.White
                        if_replay.Text = "Turn on"
                        Notifier.Show()
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Instant Replay is now off"
                    Else
                        replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
                        if_replay.Text = "Turn off"
                        replay_on.Visible = True
                        Notifier.Show()
                        Notifier.icon_n.ForeColor = ColorTranslator.FromHtml("#76B900")
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Instant Replay is now on"
                    End If
                End If
                isKeyPressed_replay = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_replay = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles alt_z.Tick 'Alt + Z - OpenShare
        Dim folderPath As String = Gallery_1.txtFilePath.Text.Trim()
        ' ตรวจสอบการกด Alt + Z เพื่อเปิด/ปิดฟังก์ชัน
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_Z) And &H8000) <> 0 Then
            If Not isKeyPressed Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive = Not isFunctionActive ' สลับสถานะฟังก์ชัน
                If isFunctionActive Then
                    If www.Opacity >= 0.01 Then
                        Return
                    End If
                    nv_ty.Visible = False

                    Bg.WindowState = FormWindowState.Maximized
                    WindowState = FormWindowState.Maximized
                    Bg.Show()
                    bg_top.Show()
                    bg_top.TopMost = True
                    Show()
                    TopMost = True
                    Bg.Opacity = 0.5
                    bg_top.Opacity = 0.9
                    Opacity = 0.8
                Else
                    HideAllControls()
                End If
                isKeyPressed = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub

    Private Sub HideAllControls() 'เพิ่มการซ่อนตรงนี้
        Me.Opacity = 0
        bg_top.Opacity = 0
        Gallery_1.Opacity = 0
        replay_sc_all.Visible = False
        record_sc.Visible = False
        settings_1.Visible = False
        action.Visible = True
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Gallery_1.Hide()
        If www.Opacity >= 0.1 Then
            Return
        Else
            Bg.Opacity = 0
            Bg.Hide()
        End If
        bg_top.Hide()
        Me.Hide()
        nv_ty.Visible = True
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles alt_f1.Tick 'Alt + F1
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_F1) And &H8000) <> 0 Then
            If Not isKeyPressed_F1 Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive_F1 = Not isFunctionActive_F1 ' สลับสถานะฟังก์ชัน
                If isFunctionActive_F1 Then
                    CaptureScreen()
                Else
                    CaptureScreen()
                End If
                isKeyPressed_F1 = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed_F1 = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub

    Private Sub AlignPanelToTop()
        ' คำนวณตำแหน่ง X ให้ Panel อยู่ตรงกลางแนวนอน และ Y ให้ชิดขอบบน
        Dim marginTop As Integer = 150 ' ขยับจากขอบบนเล็กน้อย (ปรับตามที่ต้องการ)

        ' ตั้งตำแหน่งให้ Panel แต่ละตัว
        action_sc.Location = New Point((Me.ClientSize.Width - action_sc.Width) / 2, marginTop)
        action.Location = New Point((Me.ClientSize.Width - action.Width) / 2, marginTop)
        Gallery_1.settings_1.Location = New Point((Me.ClientSize.Width - Gallery_1.settings_1.Width) / 2, marginTop)
        py.settings_1.Location = New Point((Me.ClientSize.Width - py.settings_1.Width) / 2, marginTop)
        settings_1.Location = New Point((Me.ClientSize.Width - settings_1.Width) / 2, marginTop)
        set_vdo.setre.Location = New Point((Me.ClientSize.Width - set_vdo.setre.Width) / 2, marginTop)
        hub_f.settings_1.Location = New Point((Me.ClientSize.Width - hub_f.settings_1.Width) / 2, marginTop)
        bg_top.ac1.Location = New Point((Me.ClientSize.Width - bg_top.ac1.Width) / 2, marginTop)
        set_key.keyset.Location = New Point((Me.ClientSize.Width - set_key.keyset.Width) / 2, marginTop)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        AlignPanelToTop() ' ปรับตำแหน่งเมื่อขนาดฟอร์มเปลี่ยน
    End Sub
    Private Sub bg_sh_MouseMove(sender As Object, e As MouseEventArgs) Handles bg_sh.MouseMove
        s_1.Visible = True
        s_1r.Visible = True
        s_1l.Visible = True
        s_1b.Visible = True
    End Sub

    Private Sub bg_sh_MouseLeave(sender As Object, e As EventArgs) Handles bg_sh.MouseLeave
        logo_sh.ForeColor = Color.White
        sh.ForeColor = Color.White
        s_1.Visible = False
        s_1r.Visible = False
        s_1l.Visible = False
        s_1b.Visible = False
    End Sub

    Private Sub logo_sh_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_sh.MouseMove
        s_1.Visible = True
        s_1r.Visible = True
        s_1l.Visible = True
        s_1b.Visible = True
    End Sub

    Private Sub logo_sh_MouseLeave(sender As Object, e As EventArgs) Handles logo_sh.MouseLeave
        s_1.Visible = False
        s_1r.Visible = False
        s_1l.Visible = False
        s_1b.Visible = False
    End Sub

    Private Sub sh_MouseMove(sender As Object, e As MouseEventArgs) Handles sh.MouseMove
        s_1.Visible = True
        s_1r.Visible = True
        s_1l.Visible = True
        s_1b.Visible = True
    End Sub

    Private Sub sh_MouseLeave(sender As Object, e As EventArgs) Handles sh.MouseLeave
        s_1.Visible = False
        s_1r.Visible = False
        s_1l.Visible = False
        s_1b.Visible = False
    End Sub

    Private Sub logo_sh_Click(sender As Object, e As EventArgs) Handles logo_sh.Click
        CaptureScreen()
    End Sub
    Private Sub bg_sh_Click(sender As Object, e As EventArgs) Handles bg_sh.Click
        CaptureScreen()
    End Sub

    Private Sub sh_Click(sender As Object, e As EventArgs) Handles sh.Click
        CaptureScreen()
    End Sub

    Private Sub set_to_Click(sender As Object, e As EventArgs) Handles set_to.Click
        Opacity = 1
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        settings_1.Visible = True ' แสดงฟอร์ม settings_1
        action.Visible = False ' ซ่อนฟอร์ม action
        replay_sc_all.Visible = False
        record_sc.Visible = False
    End Sub


    Private Sub set_to_MouseMove(sender As Object, e As MouseEventArgs) Handles set_to.MouseMove
        s1.Visible = True
        s1r.Visible = True
        s1l.Visible = True
        s1b.Visible = True
    End Sub

    Private Sub set_to_MouseLeave(sender As Object, e As EventArgs) Handles set_to.MouseLeave
        s1.Visible = False
        s1r.Visible = False
        s1l.Visible = False
        s1b.Visible = False
    End Sub

    Private Sub box_py_MouseMove(sender As Object, e As MouseEventArgs) Handles box_py.MouseMove
        bg_py.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub box_py_MouseLeave(sender As Object, e As EventArgs) Handles box_py.MouseLeave
        bg_py.BackColor = Color.Gray
    End Sub

    Private Sub text_py_MouseMove(sender As Object, e As MouseEventArgs) Handles text_py.MouseMove
        bg_py.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub text_py_MouseLeave(sender As Object, e As EventArgs) Handles text_py.MouseLeave
        bg_py.BackColor = Color.Gray
    End Sub

    Private Sub logo_py_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_py.MouseMove
        bg_py.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub logo_py_MouseLeave(sender As Object, e As EventArgs) Handles logo_py.MouseLeave
        bg_py.BackColor = Color.Gray
    End Sub

    Private Sub logo_replay_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_replay.MouseMove
        If replay_sc_all.Visible = True Then
            a_1.Visible = True
            a_1r.Visible = False
            a_1l.Visible = False
            a_1b.Visible = False
        Else
            a_1.Visible = True
            a_1r.Visible = True
            a_1l.Visible = True
            a_1b.Visible = True
        End If
        bg_top.b1.Visible = True
    End Sub

    Private Sub logo_replay_MouseLeave(sender As Object, e As EventArgs) Handles logo_replay.MouseLeave
        If replay_sc_all.Visible = True Then
            a_1.Visible = True
            a_1r.Visible = False
            a_1l.Visible = False
            a_1b.Visible = False
        Else
            a_1.Visible = False
            a_1r.Visible = False
            a_1l.Visible = False
            a_1b.Visible = False
        End If
        logo_replay.ForeColor = Color.White
        bg_top.b1.Visible = False
    End Sub

    Private Sub logo_record_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_record.MouseMove
        If record_sc.Visible = True Then
            a_2.Visible = True
            a_2r.Visible = False
            a_2l.Visible = False
            a_2b.Visible = False
        Else
            a_2.Visible = True
            a_2r.Visible = True
            a_2l.Visible = True
            a_2b.Visible = True
        End If

        bg_top.b2.Visible = True
        bg_top.b2.Visible = True
    End Sub

    Private Sub logo_record_MouseLeave(sender As Object, e As EventArgs) Handles logo_record.MouseLeave
        If record_sc.Visible = True Then
            a_2.Visible = True
            a_2r.Visible = False
            a_2l.Visible = False
            a_2b.Visible = False
        Else
            a_2.Visible = False
            a_2r.Visible = False
            a_2l.Visible = False
            a_2b.Visible = False
        End If
        bg_top.b2.Visible = False
        If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then

            logo_record.ForeColor = ColorTranslator.FromHtml("#76B900")
        Else
            logo_record.ForeColor = Color.White

        End If
    End Sub

    Private Sub logo_live_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_live.MouseMove
        bg_top.b3.Visible = True
        a_3.Visible = True
        a_3r.Visible = True
        a_3l.Visible = True
        a_3b.Visible = True
    End Sub

    Private Sub logo_live_MouseLeave(sender As Object, e As EventArgs) Handles logo_live.MouseLeave
        a_3.Visible = False
        a_3r.Visible = False
        a_3l.Visible = False
        a_3b.Visible = False
        bg_top.b3.Visible = False
        logo_live.ForeColor = Color.White
    End Sub

    Private Sub mic_MouseMove(sender As Object, e As MouseEventArgs) Handles mic.MouseMove
        mic.ForeColor = Color.Gray
    End Sub

    Private Sub mic_MouseLeave(sender As Object, e As EventArgs) Handles mic.MouseLeave
        mic.ForeColor = Color.White
    End Sub

    Private Sub vdo_MouseMove(sender As Object, e As MouseEventArgs) Handles vdo.MouseMove
        vdo.ForeColor = Color.Gray
    End Sub

    Private Sub vdo_MouseLeave(sender As Object, e As EventArgs) Handles vdo.MouseLeave
        vdo.ForeColor = Color.White
    End Sub

    Private Sub SetGalleryColors(color As Color)
        logo_gallery.ForeColor = color
        gallery.ForeColor = color
        bg_gallery.ForeColor = color
    End Sub

    Private Sub logo_gallery_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_gallery.MouseMove
        g1.Visible = True
        g1r.Visible = True
        g1l.Visible = True
        g1b.Visible = True
    End Sub

    Private Sub logo_gallery_MouseLeave(sender As Object, e As EventArgs) Handles logo_gallery.MouseLeave
        g1.Visible = False
        g1r.Visible = False
        g1l.Visible = False
        g1b.Visible = False
    End Sub

    Private Sub gallery_MouseMove(sender As Object, e As MouseEventArgs) Handles gallery.MouseMove
        g1.Visible = True
        g1r.Visible = True
        g1l.Visible = True
        g1b.Visible = True
    End Sub

    Private Sub gallery_MouseLeave(sender As Object, e As EventArgs) Handles gallery.MouseLeave
        g1.Visible = False
        g1r.Visible = False
        g1l.Visible = False
        g1b.Visible = False
    End Sub

    Private Sub bg_gallery_MouseMove(sender As Object, e As MouseEventArgs) Handles bg_gallery.MouseMove
        g1.Visible = True
        g1r.Visible = True
        g1l.Visible = True
        g1b.Visible = True
    End Sub

    Private Sub bg_gallery_MouseLeave(sender As Object, e As EventArgs) Handles bg_gallery.MouseLeave
        g1.Visible = False
        g1r.Visible = False
        g1l.Visible = False
        g1b.Visible = False
    End Sub

    Private Sub mic_Click(sender As Object, e As EventArgs) Handles mic.Click
        ' เปลี่ยนไอคอนของไมโครโฟนเมื่อคลิก
        mic.Text = If(mic.Text = "", "", "")
    End Sub

    Private Sub logo_gamef_Click(sender As Object, e As EventArgs) Handles logo_gamef.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Game Filter Not working Current GPU")
    End Sub

    Private Sub logo_replay_Click(sender As Object, e As EventArgs) Handles logo_replay.Click
        If replay_sc_all.Visible = True Then
            replay_sc_all.Visible = False
        Else
            replay_sc_all.Visible = True
        End If
        record_sc.Visible = False
        If a_1.Visible = False Then
            a_1.Visible = True
        Else
            a_1.Visible = False
        End If
        a_2.Visible = False
        a_3.Visible = False
    End Sub

    Private Sub logo_record_Click(sender As Object, e As EventArgs) Handles logo_record.Click
        If record_sc.Visible = True Then
            record_sc.Visible = False
        Else
            record_sc.Visible = True
        End If

        replay_sc_all.Visible = False
        If a_2.Visible = False Then
            a_2.Visible = True
        Else
            a_2.Visible = False
        End If
        a_1.Visible = False
        a_2.Visible = True
        a_3.Visible = False
    End Sub

    Private Sub logo_live_Click(sender As Object, e As EventArgs) Handles logo_live.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("This feature not ready")
        replay_sc_all.Visible = False
        a_1.Visible = False
        record_sc.Visible = False
        a_2.Visible = False
    End Sub

    Private Sub sh_replay_MouseMove(sender As Object, e As MouseEventArgs) Handles sh_replay.MouseMove
        r_1.Visible = True
        r_1r.Visible = True
        r_1l.Visible = True
        r_1b.Visible = True
    End Sub

    Private Sub sh_replay_MouseLeave(sender As Object, e As EventArgs) Handles sh_replay.MouseLeave
        r_1.Visible = False
        r_1r.Visible = False
        r_1l.Visible = False
        r_1b.Visible = False
    End Sub

    Private Sub replay_sc_MouseMove(sender As Object, e As MouseEventArgs) Handles replay_sc.MouseMove
        r_1.Visible = True
        r_1r.Visible = True
        r_1l.Visible = True
        r_1b.Visible = True
    End Sub

    Private Sub replay_sc_MouseLeave(sender As Object, e As EventArgs) Handles replay_sc.MouseLeave
        r_1.Visible = False
        r_1r.Visible = False
        r_1l.Visible = False
        r_1b.Visible = False
    End Sub

    Private Sub if_replay_MouseLeave(sender As Object, e As EventArgs) Handles if_replay.MouseLeave
        r_1.Visible = False
        r_1r.Visible = False
        r_1l.Visible = False
        r_1b.Visible = False
    End Sub

    Private Sub if_replay_MouseMove(sender As Object, e As MouseEventArgs) Handles if_replay.MouseMove
        r_1.Visible = True
        r_1r.Visible = True
        r_1l.Visible = True
        r_1b.Visible = True
    End Sub

    Private Sub sh_replay_Click(sender As Object, e As EventArgs) Handles sh_replay.Click
        a_1.Visible = False
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
            replay_on.Visible = False
            replay_on.ForeColor = Color.White
            if_replay.Text = "Turn on"
            Notifier.Show()
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Instant Replay is now off"
        Else
            replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
            if_replay.Text = "Turn off"
            replay_on.Visible = True
            Notifier.Show()
            Notifier.icon_n.ForeColor = ColorTranslator.FromHtml("#76B900")
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Instant Replay is now on"
        End If
        replay_sc_all.Visible = False
    End Sub

    Private Sub replay_sc_Click(sender As Object, e As EventArgs) Handles replay_sc.Click
        a_1.Visible = False
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
            replay_on.Visible = False
            replay_on.ForeColor = Color.White
            if_replay.Text = "Turn on"
            Notifier.Show()
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Instant Replay is now off"
        Else
            replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
            if_replay.Text = "Turn off"
            replay_on.Visible = True
            Notifier.Show()
            Notifier.icon_n.ForeColor = ColorTranslator.FromHtml("#76B900")
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Instant Replay is now on"
        End If
        replay_sc_all.Visible = False

    End Sub

    Private Sub if_replay_Click(sender As Object, e As EventArgs) Handles if_replay.Click
        a_1.Visible = False
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
            replay_on.Visible = False
            replay_on.ForeColor = Color.White
            if_replay.Text = "Turn on"
            Notifier.Show()
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Instant Replay is now off"
        Else
            replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
            if_replay.Text = "Turn off"
            replay_on.Visible = True
            Notifier.Show()
            Notifier.icon_n.ForeColor = ColorTranslator.FromHtml("#76B900")
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Instant Replay is now on"
        End If

        replay_sc_all.Visible = False
    End Sub

    Private Sub replay_on_Click(sender As Object, e As EventArgs) Handles replay_on.Click
        a_1.Visible = False
        If replay_sc_all.Visible = True Then
            replay_sc_all.Visible = False
        Else
            replay_sc_all.Visible = True
        End If
        record_sc.Visible = False
        If a_1.Visible = False Then
            a_1.Visible = True
        Else
            a_1.Visible = False
        End If
        a_2.Visible = False
        a_3.Visible = False
    End Sub

    Private Sub replay_on_MouseMove(sender As Object, e As MouseEventArgs) Handles replay_on.MouseMove
        If replay_sc_all.Visible = True Then
            a_1.Visible = True
            a_1r.Visible = False
            a_1l.Visible = False
            a_1b.Visible = False
        Else
            a_1.Visible = True
            a_1r.Visible = True
            a_1l.Visible = True
            a_1b.Visible = True
        End If
        bg_top.b1.Visible = True
    End Sub

    Private Sub replay_on_MouseLeave(sender As Object, e As EventArgs) Handles replay_on.MouseLeave
        If replay_sc_all.Visible = True Then
            a_1.Visible = True
            a_1r.Visible = False
            a_1l.Visible = False
            a_1b.Visible = False
        Else
            a_1.Visible = False
            a_1r.Visible = False
            a_1l.Visible = False
            a_1b.Visible = False
        End If
        replay_on.ForeColor = ColorTranslator.FromHtml("#76B900")
        bg_top.b1.Visible = False
    End Sub
    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load.Tick

        If Process.GetProcessesByName("NVPackage 2.62.373-PRE").Length = 1 Then
            Application.Exit()
        Else

        End If

        AlignPanelToTop()


        If Process.GetProcessesByName("RobloxCrashHandler").Length = 1 Or Process.GetProcessesByName("Minecraft").Length = 1 Or Process.GetProcessesByName("java").Length = 1 Or Process.GetProcessesByName("CrashHandler").Length = 1 Or Process.GetProcessesByName("Minecraft").Length = 1 Or Process.GetProcessesByName("GTA5").Length = 1 Or Process.GetProcessesByName("HD-Player").Length = 1 Or Process.GetProcessesByName("HD-Player").Length = 1 Then
            game.Start()
        Else
            game.Stop()
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/g")
        End If



        If My.Computer.FileSystem.FileExists(Application.StartupPath & "set") Then

            Me.WindowState = FormWindowState.Maximized
            Bg.WindowState = FormWindowState.Maximized
            Bg.Show()
            bg_top.Show()
            bg_top.TopMost = True
            Me.Show()
            Me.TopMost = True
            Bg.Opacity = 0.7
            bg_top.Opacity = 0.9
            Me.Opacity = 0.85
            File.Delete(Application.StartupPath & "set")
        Else

        End If
        up.Start()

        UpdateReplayStatus()
        UpdateRecordStatus()
        UpdateMicStatus()


    End Sub

    Private Sub UpdateRecordStatus()
        If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
            Label13.Text = ("Stop")
            s_record.Text = ("    Recording")
            s_record.ForeColor = ColorTranslator.FromHtml("#76B900")
        Else
            Label13.Text = ("Start")
            s_record.Text = ("Not Recording")
            s_record.ForeColor = Color.Gray
        End If
    End Sub

    Private Sub UpdateReplayStatus()
        If if_replay.Text = "Turn off" Then
            s_replay.Text = "on"
            s_replay.ForeColor = ColorTranslator.FromHtml("#76B900")
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/Replay/off")
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/Replay/on").Dispose()
            replay_sc1.Visible = True
            Label16.Visible = True
            Label8.Visible = True
            Label7.Visible = True
        Else
            s_replay.Text = "off"
            s_replay.ForeColor = Color.Gray
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/Replay/on")
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/Replay/off").Dispose()
            replay_sc1.Visible = False
            Label16.Visible = False
            Label8.Visible = False
            Label7.Visible = False
        End If
    End Sub

    Private Sub UpdateMicStatus()
        If mic.Text = "" Then
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/mic/mic_on")
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/mic/mic_off").Dispose()
        Else
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/mic/mic_off")
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/mic/mic_on").Dispose()
        End If
    End Sub


    Private Sub save_sc_Click(sender As Object, e As EventArgs)
        Dim folderDlg As New FolderBrowserDialog
        folderDlg.Description = "Select the folder to save the capture."
        ' ถ้าผู้ใช้กด OK หลังจากเลือกโฟลเดอร์
        If folderDlg.ShowDialog = DialogResult.OK Then
            Gallery_1.txtFilePath.Text = folderDlg.SelectedPath ' แสดงเส้นทางที่เลือกใน Textbox
            My.Settings.SavePath = Gallery_1.txtFilePath.Text
            My.Settings.Save() ' บันทึกค่า Settings
        End If
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Opacity = 0.85
        settings_1.Visible = False
        action.Visible = True
    End Sub

    Private Sub bg_fn_Click(sender As Object, e As EventArgs) Handles bg_fn.Click
        Opacity = 0.85
        settings_1.Visible = False
        action.Visible = True
    End Sub
    Private Sub pf_Click(sender As Object, e As EventArgs) Handles pf.Click
        HideAllControls()
        If Opacity = 0 Then
            www.Timer1.Stop()
            www.Timer2.Start()
            Bg.Timer1.Stop()
            Bg.Timer2.Start()
        Else
        End If
        www.Show()
    End Sub

    Private Sub Logo_Click(sender As Object, e As EventArgs) Handles Logo.Click
        Notifier.Show()
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Press Alt + Z to use Shadowplay Experience in-game overlay")
    End Sub

    Private Sub bg_gallery_Click(sender As Object, e As EventArgs) Handles bg_gallery.Click
        action.Visible = False
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Gallery_1.WindowState = FormWindowState.Maximized
        Gallery_1.Opacity = 1
        replay_sc_all.Visible = False
        record_sc.Visible = False

        Gallery_1.Show()
    End Sub

    Private Sub gallery_Click(sender As Object, e As EventArgs) Handles gallery.Click
        action.Visible = False
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Gallery_1.WindowState = FormWindowState.Maximized
        Gallery_1.Opacity = 1
        replay_sc_all.Visible = False
        record_sc.Visible = False
        Gallery_1.Show()
    End Sub

    Private Sub logo_gallery_Click(sender As Object, e As EventArgs) Handles logo_gallery.Click
        action.Visible = False
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        Gallery_1.WindowState = FormWindowState.Maximized
        Gallery_1.Opacity = 1
        replay_sc_all.Visible = False
        record_sc.Visible = False
        Gallery_1.Show()
    End Sub

    Private Sub vdo_Click(sender As Object, e As EventArgs) Handles vdo.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Extension not found")
    End Sub

    Private Sub box_py_Click(sender As Object, e As EventArgs) Handles box_py.Click


        If My.Settings.User = "" Then
            login.Show()
            alt_z.Stop()
            Home_settings.Visible = False
            settings_all.Visible = False
            ch.Visible = False
            ch_bg.Visible = False
            action_fn.Visible = False
            bg_fn.Visible = False
            login.Load.Start()
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Welcome " & My.Settings.User)
        End If


    End Sub

    Private Sub text_py_Click(sender As Object, e As EventArgs) Handles text_py.Click

        If My.Settings.User = "" Then
            login.Show()
            alt_z.Stop()
            Home_settings.Visible = False
            settings_all.Visible = False
            ch.Visible = False
            ch_bg.Visible = False
            action_fn.Visible = False
            bg_fn.Visible = False
            login.Load.Start()
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Welcome " & My.Settings.User)
        End If

    End Sub

    Private Sub logo_py_Click(sender As Object, e As EventArgs) Handles logo_py.Click

        If My.Settings.User = "" Then
            login.Show()
            alt_z.Stop()
            Home_settings.Visible = False
            settings_all.Visible = False
            ch.Visible = False
            ch_bg.Visible = False
            action_fn.Visible = False
            bg_fn.Visible = False
            login.Load.Start()
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Welcome " & My.Settings.User)
        End If

    End Sub

    Private Sub replay_sc1_Click(sender As Object, e As EventArgs) Handles replay_sc1.Click
        a_1.Visible = False
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Erorr to saved last {{replay_save}}."
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Turn on instant replay to save the last."
        End If
        replay_sc_all.Visible = False
    End Sub

    Private Sub Label7_Click(sender As Object, e As EventArgs) Handles Label7.Click
        a_1.Visible = False
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Erorr to saved last {{replay_save}}."
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Turn on instant replay to save the last."
        End If
        replay_sc_all.Visible = False
    End Sub
    Private Sub bg_fps_Click(sender As Object, e As EventArgs) Handles bg_fps.Click
        HideAllControls()
        If Opacity = 0 Then
            www.Timer1.Stop()
            www.Timer2.Start()
            Bg.Timer1.Stop()
            Bg.Timer2.Start()
        Else
        End If
        www.Show()
    End Sub

    Private Sub logo_pf_Click(sender As Object, e As EventArgs) Handles logo_pf.Click
        HideAllControls()
        If Opacity = 0 Then
            www.Timer1.Stop()
            www.Timer2.Start()
            Bg.Timer1.Stop()
            Bg.Timer2.Start()
        Else
        End If
        www.Show()
    End Sub

    Private Sub hd_all_Tick(sender As Object, e As EventArgs) Handles hd_all.Tick
        Me.Hide()
        Bg.Hide()
        Bg.WindowState = FormWindowState.Maximized
        Me.WindowState = FormWindowState.Maximized
    End Sub

    Private Sub rq_Tick(sender As Object, e As EventArgs) Handles rq.Tick
    End Sub

    Private Sub Base_Click(sender As Object, e As EventArgs) Handles MyBase.Click
        HandleGalleryDisplay()
    End Sub

    ' ฟังก์ชันสำหรับแสดง Gallery_1 และตั้งค่า TopMost ถ้าตรงตามเงื่อนไข
    Private Sub HandleGalleryDisplay()
        If Gallery_1.settings_1.Visible = True Then
            Gallery_1.Show()
            Gallery_1.TopMost = True
        End If
    End Sub


    Private Sub saved_e_MouseMove(sender As Object, e As MouseEventArgs) Handles saved_e.MouseMove
        saved_e1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub saved_e_MouseLeave(sender As Object, e As EventArgs) Handles saved_e.MouseLeave
        saved_e1.BackColor = Color.Gray
    End Sub

    Private Sub Label4_MouseMove(sender As Object, e As MouseEventArgs) Handles Label4.MouseMove
        saved_e1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label4_MouseLeave(sender As Object, e As EventArgs) Handles Label4.MouseLeave
        saved_e1.BackColor = Color.Gray
    End Sub

    Private Sub Label5_MouseMove(sender As Object, e As MouseEventArgs) Handles Label5.MouseMove
        saved_e1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label5_MouseLeave(sender As Object, e As EventArgs) Handles Label5.MouseLeave
        saved_e1.BackColor = Color.Gray
    End Sub

    Private Sub saved_e_Click(sender As Object, e As EventArgs) Handles saved_e.Click
        alt_z.Stop()
        settings_1.Visible = False
        py.Show()
        py.WindowState = FormWindowState.Maximized
        py.settings_1.Visible = True
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click
        alt_z.Stop()
        settings_1.Visible = False
        py.Show()
        py.WindowState = FormWindowState.Maximized
        py.settings_1.Visible = True
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        alt_z.Stop()
        settings_1.Visible = False
        py.Show()
        py.WindowState = FormWindowState.Maximized
        py.settings_1.Visible = True
    End Sub

    Private Sub DisplayNotifierMessage(icon As String, message As String)
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = icon
        Notifier.text_n.Text = message
    End Sub

    Private Sub ch_Click(sender As Object, e As EventArgs) Handles ch.Click
        CheckForUpdates()
    End Sub


    Private Sub ch_bg_Click(sender As Object, e As EventArgs) Handles ch_bg.Click
        CheckForUpdates()
    End Sub

    Private Sub bg_fps_MouseMove(sender As Object, e As MouseEventArgs) Handles bg_fps.MouseMove
        h1.Visible = True
        h1r.Visible = True
        h1l.Visible = True
        h1b.Visible = True
    End Sub

    Private Sub bg_fps_MouseLeave(sender As Object, e As EventArgs) Handles bg_fps.MouseLeave
        h1.Visible = False
        h1r.Visible = False
        h1l.Visible = False
        h1b.Visible = False
    End Sub

    Private Sub pf_MouseMove(sender As Object, e As MouseEventArgs) Handles pf.MouseMove
        h1.Visible = True
        h1r.Visible = True
        h1l.Visible = True
        h1b.Visible = True
    End Sub

    Private Sub pf_MouseLeave(sender As Object, e As EventArgs) Handles pf.MouseLeave
        h1.Visible = False
        h1r.Visible = False
        h1l.Visible = False
        h1b.Visible = False
    End Sub

    Private Sub logo_pf_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_pf.MouseMove
        h1.Visible = True
        h1r.Visible = True
        h1l.Visible = True
        h1b.Visible = True
    End Sub

    Private Sub logo_pf_MouseLeave(sender As Object, e As EventArgs) Handles logo_pf.MouseLeave
        h1.Visible = False
        h1r.Visible = False
        h1l.Visible = False
        h1b.Visible = False
    End Sub

    Private Sub py_cc_Tick(sender As Object, e As EventArgs) Handles py_cc.Tick


        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/py") Then
            py.py_2.Text = ("Turn off")
        Else
            If replay_on.Visible = True Then
                replay_on.Visible = False
                replay_on.ForeColor = Color.White
                if_replay.Text = ("Turn on")
            End If

            If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Then
                logo_record.ForeColor = Color.White
            End If
            Label13.Text = ("Start")
            s_record.Text = ("Not Recording")
            s_record.ForeColor = Color.Gray


            py.py_2.Text = ("Turn on")
        End If
    End Sub
    Private Sub SetPhotoColors(color As Color)
        logo_pht.ForeColor = color
        pht.ForeColor = color
        bg_pht.ForeColor = color
    End Sub

    Private Sub logo_pht_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_pht.MouseMove
        s_2.Visible = True
        s_2r.Visible = True
        s_2l.Visible = True
        s_2b.Visible = True
    End Sub

    Private Sub logo_pht_MouseLeave(sender As Object, e As EventArgs) Handles logo_pht.MouseLeave
        SetPhotoColors(Color.White)
        s_2.Visible = False
        s_2r.Visible = False
        s_2l.Visible = False
        s_2b.Visible = False
    End Sub

    Private Sub pht_MouseMove(sender As Object, e As MouseEventArgs) Handles pht.MouseMove
        s_2.Visible = True
        s_2r.Visible = True
        s_2l.Visible = True
        s_2b.Visible = True
    End Sub

    Private Sub pht_MouseLeave(sender As Object, e As EventArgs) Handles pht.MouseLeave
        SetPhotoColors(Color.White)
        s_2.Visible = False
        s_2r.Visible = False
        s_2l.Visible = False
        s_2b.Visible = False
    End Sub

    Private Sub bg_pht_MouseMove(sender As Object, e As MouseEventArgs) Handles bg_pht.MouseMove
        s_2.Visible = True
        s_2r.Visible = True
        s_2l.Visible = True
        s_2b.Visible = True
    End Sub

    Private Sub bg_pht_MouseLeave(sender As Object, e As EventArgs) Handles bg_pht.MouseLeave
        SetPhotoColors(Color.White)
        s_2.Visible = False
        s_2r.Visible = False
        s_2l.Visible = False
        s_2b.Visible = False
    End Sub
    Private Sub SetGameColors(color As Color)
        logo_gamef.ForeColor = color
        game_f.ForeColor = color
        bg_gamef.ForeColor = color
    End Sub

    Private Sub logo_gamef_MouseMove(sender As Object, e As MouseEventArgs) Handles logo_gamef.MouseMove
        s_3.Visible = True
        s_3r.Visible = True
        s_3l.Visible = True
        s_3b.Visible = True
    End Sub

    Private Sub logo_gamef_MouseLeave(sender As Object, e As EventArgs) Handles logo_gamef.MouseLeave
        SetGameColors(Color.White)
        s_3.Visible = False
        s_3r.Visible = False
        s_3l.Visible = False
        s_3b.Visible = False
    End Sub

    Private Sub game_f_MouseMove(sender As Object, e As MouseEventArgs) Handles game_f.MouseMove
        s_3.Visible = True
        s_3r.Visible = True
        s_3l.Visible = True
        s_3b.Visible = True
    End Sub

    Private Sub game_f_MouseLeave(sender As Object, e As EventArgs) Handles game_f.MouseLeave
        SetGameColors(Color.White)
        s_3.Visible = False
        s_3r.Visible = False
        s_3l.Visible = False
        s_3b.Visible = False
    End Sub

    Private Sub bg_gamef_MouseMove(sender As Object, e As MouseEventArgs) Handles bg_gamef.MouseMove
        s_3.Visible = True
        s_3r.Visible = True
        s_3l.Visible = True
        s_3b.Visible = True
    End Sub

    Private Sub bg_gamef_MouseLeave(sender As Object, e As EventArgs) Handles bg_gamef.MouseLeave
        SetGameColors(Color.White)
        s_3.Visible = False
        s_3r.Visible = False
        s_3l.Visible = False
        s_3b.Visible = False
    End Sub

    Private Sub game_f_Click(sender As Object, e As EventArgs) Handles game_f.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Game Filter Not working Current GPU")
    End Sub

    Private Sub bg_gamef_Click(sender As Object, e As EventArgs) Handles bg_gamef.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Game Filter Not working Current GPU")

    End Sub

    Private Sub bg_pht_Click(sender As Object, e As EventArgs) Handles bg_pht.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Photo Mode Not working Current GPU")
    End Sub

    Private Sub pht_Click(sender As Object, e As EventArgs) Handles pht.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Photo Mode Not working Current GPU")
    End Sub

    Private Sub logo_pht_Click(sender As Object, e As EventArgs) Handles logo_pht.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("Photo Mode Not working Current GPU")
    End Sub

    Private Sub Label16_MouseMove(sender As Object, e As MouseEventArgs) Handles Label16.MouseMove
        rs1.Visible = True
        rsl.Visible = True
        rsr.Visible = True
        rsb.Visible = True
    End Sub

    Private Sub Label16_MouseLeave(sender As Object, e As EventArgs) Handles Label16.MouseLeave
        rs1.Visible = False
        rsl.Visible = False
        rsr.Visible = False
        rsb.Visible = False
    End Sub

    Private Sub replay_sc1_MouseLeave(sender As Object, e As EventArgs) Handles replay_sc1.MouseLeave
        rs1.Visible = False
        rsl.Visible = False
        rsr.Visible = False
        rsb.Visible = False
    End Sub

    Private Sub replay_sc1_MouseMove(sender As Object, e As MouseEventArgs) Handles replay_sc1.MouseMove
        rs1.Visible = True
        rsl.Visible = True
        rsr.Visible = True
        rsb.Visible = True
    End Sub

    Private Sub Label7_MouseMove(sender As Object, e As MouseEventArgs) Handles Label7.MouseMove
        rs1.Visible = True
        rsl.Visible = True
        rsr.Visible = True
        rsb.Visible = True
    End Sub

    Private Sub Label7_MouseLeave(sender As Object, e As EventArgs) Handles Label7.MouseLeave
        rs1.Visible = False
        rsl.Visible = False
        rsr.Visible = False
        rsb.Visible = False
    End Sub

    Private Sub Label16_Click(sender As Object, e As EventArgs) Handles Label16.Click
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Erorr to saved last {{replay_save}}."
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Turn on instant replay to save the last."
        End If
        a_1.Visible = False
    End Sub

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        ab_bg.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label6_MouseMove(sender As Object, e As MouseEventArgs) Handles Label6.MouseMove, Label6.MouseMove
        ab_bg.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label9_MouseMove(sender As Object, e As MouseEventArgs) Handles Label9.MouseMove
        ab_bg.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub PictureBox1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave
        ab_bg.BackColor = Color.Gray
    End Sub

    Private Sub Label6_MouseLeave(sender As Object, e As EventArgs) Handles Label6.MouseLeave
        ab_bg.BackColor = Color.Gray
    End Sub

    Private Sub Label9_MouseLeave(sender As Object, e As EventArgs) Handles Label9.MouseLeave
        ab_bg.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs)
        ShowNotifier("NVIDIA ShadowPlay™ Version 2.62.373", "")
    End Sub

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Label6.Click
        ShowNotifier("NVIDIA ShadowPlay™ Version 2.62.373", "")
    End Sub

    Private Sub Label9_Click(sender As Object, e As EventArgs) Handles Label9.Click
        ShowNotifier("NVIDIA ShadowPlay™ Version 2.62.373", "")
    End Sub

    Private Sub re_Tick(sender As Object, e As EventArgs) Handles re.Tick
        ShowNotifier("Reload Complete.", "")
    End Sub

    Private Sub Logo_text_DoubleClick(sender As Object, e As EventArgs)
        Application.Restart()
    End Sub

    Private Sub sh_record_MouseMove(sender As Object, e As MouseEventArgs) Handles sh_record.MouseMove
        st1.Visible = True
        str.Visible = True
        stl.Visible = True
        stb.Visible = True
    End Sub

    Private Sub PictureBox5_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox5.MouseMove
        st1.Visible = True
        str.Visible = True
        stl.Visible = True
        stb.Visible = True
    End Sub

    Private Sub Label13_MouseMove(sender As Object, e As MouseEventArgs) Handles Label13.MouseMove
        st1.Visible = True
        str.Visible = True
        stl.Visible = True
        stb.Visible = True
    End Sub

    Private Sub sh_record_MouseLeave(sender As Object, e As EventArgs) Handles sh_record.MouseLeave
        st1.Visible = False
        str.Visible = False
        stl.Visible = False
        stb.Visible = False
    End Sub

    Private Sub PictureBox5_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox5.MouseLeave
        st1.Visible = False
        str.Visible = False
        stl.Visible = False
        stb.Visible = False
    End Sub

    Private Sub Label13_MouseLeave(sender As Object, e As EventArgs) Handles Label13.MouseLeave
        st1.Visible = False
        str.Visible = False
        stl.Visible = False
        stb.Visible = False
    End Sub
    Private ffmpegProcess As Process
    Private Sub sh_record_Click(sender As Object, e As EventArgs) Handles sh_record.Click
        a_2.Visible = False
        If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
            If Notifier.text_n.Text = "Recording has started" Or Notifier.text_n.Text = "Recording has started " Then
                ShowNotifier("", "Recording has started ", ColorTranslator.FromHtml("#76B900"), 40)
            Else
                StopVideoRecording()
                ShowNotifier("", "Recording has been saved", Color.White, 40)
                logo_record.ForeColor = Color.White
            End If
        Else
            If Notifier.text_n.Text = "Recording has been saved" Or Notifier.text_n.Text = "Recording has been saved " Then
                ShowNotifier("", "Recording has been saved ", Color.White, 40)
            Else
                StartVideoRecording()
                ShowNotifier("", "Recording has started", ColorTranslator.FromHtml("#76B900"), 40)
                logo_record.ForeColor = ColorTranslator.FromHtml("#76B900")
            End If
        End If
        record_sc.Visible = False
    End Sub
    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        a_2.Visible = False
        If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
            If Notifier.text_n.Text = "Recording has started" Or Notifier.text_n.Text = "Recording has started " Then
                ShowNotifier("", "Recording has started ", ColorTranslator.FromHtml("#76B900"), 40)
            Else
                StopVideoRecording()
                ShowNotifier("", "Recording has been saved", Color.White, 40)
                logo_record.ForeColor = Color.White
            End If
        Else
            If Notifier.text_n.Text = "Recording has been saved" Or Notifier.text_n.Text = "Recording has been saved " Then
                ShowNotifier("", "Recording has been saved ", Color.White, 40)
            Else
                StartVideoRecording()
                ShowNotifier("", "Recording has started", ColorTranslator.FromHtml("#76B900"), 40)
                logo_record.ForeColor = ColorTranslator.FromHtml("#76B900")
            End If
        End If
        record_sc.Visible = False
    End Sub

    Private Sub Label13_Click(sender As Object, e As EventArgs) Handles Label13.Click

        a_2.Visible = False
        If logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#426800") Then
            If Notifier.text_n.Text = "Recording has started" Or Notifier.text_n.Text = "Recording has started " Then
                ShowNotifier("", "Recording has started ", ColorTranslator.FromHtml("#76B900"), 40)
            Else
                StopVideoRecording()
                ShowNotifier("", "Recording has been saved", Color.White, 40)
                logo_record.ForeColor = Color.White
            End If
        Else
            If Notifier.text_n.Text = "Recording has been saved" Or Notifier.text_n.Text = "Recording has been saved " Then
                ShowNotifier("", "Recording has been saved ", Color.White, 40)
            Else
                StartVideoRecording()
                ShowNotifier("", "Recording has started", ColorTranslator.FromHtml("#76B900"), 40)
                logo_record.ForeColor = ColorTranslator.FromHtml("#76B900")
            End If
        End If
        record_sc.Visible = False
    End Sub

    Private Sub PictureBox10_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox10.MouseMove
        hub.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label12_MouseMove(sender As Object, e As MouseEventArgs) Handles Label12.MouseMove
        hub.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label15_MouseMove(sender As Object, e As MouseEventArgs) Handles Label15.MouseMove
        hub.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub PictureBox10_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox10.MouseLeave
        hub.BackColor = Color.Gray
    End Sub

    Private Sub Label12_MouseLeave(sender As Object, e As EventArgs) Handles Label12.MouseLeave
        hub.BackColor = Color.Gray
    End Sub

    Private Sub Label15_MouseLeave(sender As Object, e As EventArgs) Handles Label15.MouseLeave
        hub.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox10_Click(sender As Object, e As EventArgs) Handles PictureBox10.Click
        hub_f.Show()
        settings_1.Visible = False
        hub_f.settings_1.Visible = True
        alt_z.Stop()
    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles Label12.Click
        hub_f.Show()
        settings_1.Visible = False
        hub_f.settings_1.Visible = True
        alt_z.Stop()
    End Sub

    Private Sub Label15_Click(sender As Object, e As EventArgs) Handles Label15.Click
        hub_f.Show()
        settings_1.Visible = False
        hub_f.settings_1.Visible = True
        alt_z.Stop()
    End Sub
    Private isNotifierOn As Boolean = False ' ตัวแปรสถานะเพื่อบอกว่าตอนนี้ Notifier เปิดอยู่หรือไม่

    Private Sub NotifyIcon_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' สร้าง NotifyIcon
        nv_ty = New NotifyIcon()

        ' ตั้งค่าไอคอนและข้อความzz
        Dim ic As String = (Application.StartupPath & "NVIDIA ShadowPlay.ico")
        Dim separator1 As New ToolStripSeparator()
        nv_ty.Icon = New Icon(ic) ' ใส่เส้นทางไปยังไฟล์ .ico ของคุณ
        nv_ty.Text = "NVIDIA Shadowplay™"
        nv_ty.Visible = True
        Dim separator2 As New ToolStripSeparator()
        Dim separator3 As New ToolStripSeparator()
        Dim separator4 As New ToolStripSeparator()
        Dim separator5 As New ToolStripSeparator()
        Dim separator6 As New ToolStripSeparator()
        ' สร้าง Context Menu
        Dim contextMenu As New ContextMenuStrip()
        Dim menuItem As New ToolStripMenuItem()
        menuItem.Text = "NVIDIA Shadowplay™"
        menuItem.Enabled = True ' ทำให้ไม่สามารถเลือกได้ (ถ้าต้องการให้เป็นเพียงข้อความ)
        menuItem.Image = Image.FromFile(Application.StartupPath & "NVIDIA ShadowPlay.ico")

        Dim menuver As New ToolStripMenuItem()

        menuver.Text = "Edit By Scotcs Duluka/Duluka Inc."
        menuver.Enabled = False ' ทำให้ไม่สามารถเลือกได้ (ถ้าต้องการให้เป็นเพียงข้อความ)
        'contextMenu.Items.Add(separator5)
        contextMenu.Items.Add(menuItem)
        contextMenu.Items.Add(separator1)
        AddHandler menuItem.Click, AddressOf MenuItem_Click ' เชื่อมโยง event handler
        contextMenu.Items.Add("Check update", Nothing, AddressOf upif)
        contextMenu.Items.Add("Open", Nothing, AddressOf settings_Notifier)
        contextMenu.Items.Add("Close Share", Nothing, AddressOf sharef)
        Dim checkst As New ToolStripMenuItem("Auto Startup")
        checkst.CheckOnClick = True ' ทำให้สามารถติ๊กได้
        checkst.Checked = True
        AddHandler checkst.CheckedChanged, AddressOf AutoStartup
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\now") Then
            checkst.Checked = True
        Else
            checkst.Checked = False
        End If
        contextMenu.Items.Add(separator2)
        contextMenu.Items.Add(checkst)
        Dim checkItem As New ToolStripMenuItem("Use Overlay")
        checkItem.CheckOnClick = True
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\on") Then
            checkItem.Checked = True
        Else
            checkItem.Checked = False
        End If
        AddHandler checkItem.CheckedChanged, AddressOf CheckItem_CheckedChanged
        contextMenu.Items.Add(checkItem)
        contextMenu.Items.Add(separator3)
        contextMenu.Items.Add("Close Menu")
        contextMenu.Items.Add("Restart", Nothing, AddressOf reset)
        contextMenu.Items.Add("Exit", Nothing, AddressOf ExitApp)
        contextMenu.Items.Add(separator4)
        contextMenu.Items.Add("Edit By Scotcs Duluka/Duluka Inc.", Nothing, AddressOf openver)
        'contextMenu.Items.Add(separator6)
        nv_ty.ContextMenuStrip = contextMenu

        ' ตั้งค่า Notifier เริ่มต้น
        'UpdateNotifierMenuText() ' อัปเดตเมนูตามสถานะ



        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data\on") Then
            ' แจ้งการเริ่มแอป
            Notifier.Show()
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Press Alt + Z to use Shadowplay Experience in-game overlay"
            Return
        Else
            alt_z.Stop()
            alt_f1.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            save.Stop()
            alt_F_1_2.Stop()
            w.Stop()
        End If

    End Sub
    Private Sub openver()
        Dim url As String = "cmd.exe /c start https://github.com/ScotcsDuluka/NVIDIA-Shadowplay"
        Process.Start("cmd.exe", "/c start " & url)
    End Sub
    Private Sub sharef(sender As Object, e As EventArgs)
        www.Close()
        Bg.Hide()
        DisplayNotifierMessage("", "NVIDIA Shared Close")
    End Sub
    Private Sub AutoStartup(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        If item.Checked Then
            AddToStartup()
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data\now").Dispose()
        Else
            RemoveFromStartup()
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data\now")
        End If
    End Sub
    Private Sub CheckItem_CheckedChanged(sender As Object, e As EventArgs)
        Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        If item.Checked Then
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data\on").Dispose()
            alt_z.Start()
            alt_f1.Start()
            alt_shift_f10.Start()
            record_1.Start()
            save.Start()
            alt_F_1_2.Start()
            w.Start()
        Else
            File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data\on")
            alt_z.Stop()
            alt_f1.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            save.Stop()
            alt_F_1_2.Stop()
            w.Stop()
        End If
    End Sub
    Public Class VersionInfo
        Public Property version As String
        Public Property updateUrl As String
        Public Property server As String
    End Class

    Private Sub CheckForUpdates()
        Dim currentVersion As String = "2.74.412" ' เวอร์ชันที่ติดตั้งอยู่
        Dim jsonUrl As String = "https://drive.google.com/uc?export=download&id=1tcsQ6tunfe2YbAJ1J0sWBzYj5k3G-aVD" ' URL สำหรับ JSON

        Try
            Dim client As New WebClient()
            Dim json As String = client.DownloadString(jsonUrl) ' ดาวน์โหลด JSON

            Dim versionInfo As VersionInfo = JsonConvert.DeserializeObject(Of VersionInfo)(json) ' Deserialize JSON

            If Not String.IsNullOrEmpty(versionInfo.version) AndAlso currentVersion <> versionInfo.version Then
                DisplayNotifierMessage("", "NVIDIA Shadowplay™ update version " & versionInfo.version & " is now available.")
                Dim url As String = "cmd.exe /c start https://www.mediafire.com/folder/hcg9p5b2fo43s/app"
                If Notifier.text_n.Text = ("NVIDIA Shadowplay™ update version " & versionInfo.version & " is now available.") Then
                    Process.Start("cmd.exe", "/c start " & versionInfo.updateUrl)

                End If
            Else
                DisplayNotifierMessage("", "Version NVIDIA Shadowplay™ is latest. Current Version : " & currentVersion)
            End If
        Catch ex As Exception
            DisplayNotifierMessage("", "Erorr to Check Update Version NVIDIA Shadowplay™.")
        End Try
    End Sub
    Private Sub upif(sender As Object, e As EventArgs)
        CheckForUpdates()
    End Sub
    Private Sub reset(sender As Object, e As EventArgs)
        Application.Restart()
    End Sub

    Private Sub MenuItem_Click(sender As Object, e As EventArgs)
        ' ฟังก์ชันที่เรียกเมื่อ menuItem ถูกคลิก
        ShowNotifier("NVIDIA ShadowPlay™ Version 2.62.373", "")
    End Sub
    ' ฟังก์ชันเปิดแอป
    Private Sub settings_Notifier(sender As Object, e As EventArgs)

        If alt_z.Enabled = True Then
            isFunctionActive = Not isFunctionActive
            replay_sc_all.Visible = False
            record_sc.Visible = False
            Me.WindowState = FormWindowState.Maximized
            Bg.WindowState = FormWindowState.Maximized
            Bg.Show()
            bg_top.Show()
            bg_top.TopMost = True
            Me.Show()
            Me.TopMost = True
            Bg.Opacity = 0.7
            bg_top.Opacity = 0.8
            Me.Opacity = 0.85
            Notifier.Show()
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Overlay is Open."
        Else
            Notifier.Show()
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Overlay not using."
        End If

    End Sub

    ' ฟังก์ชันเปลี่ยนสถานะ Notifier
    Private Sub ToggleNotifier()
        isNotifierOn = Not isNotifierOn ' เปลี่ยนสถานะ

        If isNotifierOn Then
            ' หากเปิด Notifier
            alt_z.Start()
            alt_f1.Start()
            alt_shift_f10.Start()
            record_1.Start()
            save.Start()
            alt_F_1_2.Start()
            w.Start()
        Else
            ' หากปิด Notifier
            alt_z.Stop()
            alt_f1.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            save.Stop()
            alt_F_1_2.Stop()
            w.Stop()
        End If
    End Sub

    ' ฟังก์ชันอัปเดตเมนูตามสถานะ
    Private Sub UpdateNotifierMenuText()
        Dim item As ToolStripItem = nv_ty.ContextMenuStrip.Items(5) ' เมนูรายการแรกคือ Toggle
        If isNotifierOn Then
            item.Text = "Notifier OFF"
        Else
            item.Text = "Notifier ON"
        End If
    End Sub

    'ฟังก์ชันออกจากโปรแกรม
    Private Sub ExitApp(sender As Object, e As EventArgs)
        alt_z.Stop()
        alt_f1.Stop()
        alt_shift_f10.Stop()
        record_1.Stop()
        save.Stop()
        alt_F_1_2.Stop()
        w.Stop()
        nv_ty.Visible = False ' ซ่อน NotifyIcon
        Notifier.Show()
        Notifier.icon_n.Text = ""
        Notifier.text_n.Text = "NVIDIA Shadowplay™ app is closed."
        Bg.Close()
        bg_top.Close()
        Gallery_1.Close()
        hub_f.Close()
        py.Close()
        set_key.Close()
        set_vdo.Close()
    End Sub

    Private Sub ch_t_Tick(sender As Object, e As EventArgs) Handles ch_t.Tick
        If Opacity = 0 Then
            nv_ty.Visible = True
        Else
            nv_ty.Visible = False
        End If
    End Sub

    Private Sub nv_ty_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles nv_ty.MouseDoubleClick

    End Sub

    Private Sub game_Tick(sender As Object, e As EventArgs) Handles game.Tick
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/g") Then

        Else
            File.Create(Application.StartupPath & "NVIDIA_Shadowplay_Data/g").Dispose()
            Notifier.Show()
            Notifier.icon_n.Text = ""
            Notifier.text_n.Text = "Press Alt + Z to use Shadowplay Experience in-game overlay"
        End If
    End Sub

    Private Sub PictureBox11_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox11.MouseMove
        k1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label17_MouseMove(sender As Object, e As MouseEventArgs) Handles Label17.MouseMove
        k1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label18_MouseMove(sender As Object, e As MouseEventArgs) Handles Label18.MouseMove
        k1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub PictureBox11_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox11.MouseLeave
        k1.BackColor = Color.Gray
    End Sub

    Private Sub Label17_MouseLeave(sender As Object, e As EventArgs) Handles Label17.MouseLeave
        k1.BackColor = Color.Gray
    End Sub

    Private Sub Label18_MouseLeave(sender As Object, e As EventArgs) Handles Label18.MouseLeave
        k1.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox1_Click_1(sender As Object, e As EventArgs) Handles PictureBox1.Click
        ShowNotifier("NVIDIA ShadowPlay™ Version 2.62.373", "")
    End Sub

    Private Sub PictureBox11_Click(sender As Object, e As EventArgs) Handles PictureBox11.Click
        alt_z.Stop()
        set_key.Show()
        settings_1.Visible = False
    End Sub

    Private Sub Label17_Click(sender As Object, e As EventArgs) Handles Label17.Click
        alt_z.Stop()
        set_key.Show()
        settings_1.Visible = False
    End Sub

    Private Sub Label18_Click(sender As Object, e As EventArgs) Handles Label18.Click
        alt_z.Stop()
        set_key.Show()
        settings_1.Visible = False
    End Sub

    Private Sub bg_action_Click(sender As Object, e As EventArgs) Handles bg_action.Click

    End Sub

    Private Sub hg1_Tick(sender As Object, e As EventArgs) Handles hg1.Tick
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/save") Then
            hg1.Stop()
        End If
        ' จับภาพหน้าจอ
        Dim bmpScreenshot As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
        Dim g As Graphics = Graphics.FromImage(bmpScreenshot)
        g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size)

        ' สีที่ต้องการตรวจจับ (#76B900)
        Dim targetColor As Color = ColorTranslator.FromHtml("#ACB22E")

        ' วนลูปตรวจสอบทุกพิกเซล
        For x As Integer = 0 To bmpScreenshot.Width - 1
            For y As Integer = 0 To bmpScreenshot.Height - 1
                Dim currentColor As Color = bmpScreenshot.GetPixel(x, y)

                ' ถ้าพบสีที่ต้องการ
                If currentColor = targetColor Then
                    If My.Computer.FileSystem.FileExists(Application.StartupPath & "NVIDIA_Shadowplay_Data/save") Then
                        Return
                    Else
                        Notifier.Show()
                        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
                        Notifier.icon_n.ForeColor = Color.White
                        Notifier.icon_n.Text = ""
                        Notifier.text_n.Text = "Saved last 15 seconds."
                        hg1.Stop()
                    End If

                    Exit Sub
                End If
            Next
        Next
    End Sub

    Private Sub not_save_Tick(sender As Object, e As EventArgs) Handles not_save.Tick
        File.Delete(Application.StartupPath & "NVIDIA_Shadowplay_Data/save")
        hg1.Start()
    End Sub

    Private Sub PictureBox16_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox16.MouseMove
        hg2.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label21_MouseMove(sender As Object, e As MouseEventArgs) Handles Label21.MouseMove
        hg2.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label22_MouseMove(sender As Object, e As MouseEventArgs) Handles Label22.MouseMove
        hg2.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub PictureBox16_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox16.MouseLeave
        hg2.BackColor = Color.Gray
    End Sub

    Private Sub Label21_MouseLeave(sender As Object, e As EventArgs) Handles Label21.MouseLeave
        hg2.BackColor = Color.Gray
    End Sub

    Private Sub Label22_MouseLeave(sender As Object, e As EventArgs) Handles Label22.MouseLeave
        hg2.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox13_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox13.MouseMove
        vd1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub vdo_setme_MouseMove(sender As Object, e As MouseEventArgs) Handles vdo_setme.MouseMove
        vd1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label19_MouseMove(sender As Object, e As MouseEventArgs) Handles Label19.MouseMove
        vd1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label20_MouseMove(sender As Object, e As MouseEventArgs) Handles Label20.MouseMove
        vd1.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub PictureBox13_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox13.MouseLeave
        vd1.BackColor = Color.Gray
    End Sub

    Private Sub vdo_setme_MouseLeave(sender As Object, e As EventArgs) Handles vdo_setme.MouseLeave
        vd1.BackColor = Color.Gray
    End Sub

    Private Sub Label19_MouseLeave(sender As Object, e As EventArgs) Handles Label19.MouseLeave
        vd1.BackColor = Color.Gray
    End Sub

    Private Sub Label20_MouseWheel(sender As Object, e As MouseEventArgs) Handles Label20.MouseWheel
        vd1.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox13_Click(sender As Object, e As EventArgs) Handles PictureBox13.Click
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Then

            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Capture is open, please close first.")
        Else
            settings_1.Visible = False
            alt_z.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            set_vdo.Show()
        End If
    End Sub

    Private Sub vdo_setme_Click(sender As Object, e As EventArgs) Handles vdo_setme.Click
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Then

            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Capture is open, please close first.")
        Else
            settings_1.Visible = False
            alt_z.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            set_vdo.Show()
        End If
    End Sub

    Private Sub Label19_Click(sender As Object, e As EventArgs) Handles Label19.Click
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Then

            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Capture is open, please close first.")
        Else
            settings_1.Visible = False
            alt_z.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            set_vdo.Show()
        End If
    End Sub

    Private Sub Label20_Click(sender As Object, e As EventArgs) Handles Label20.Click
        settings_1.Visible = False
        alt_z.Stop()
        alt_shift_f10.Stop()
        record_1.Stop()
        set_vdo.Show()
    End Sub

    Private Sub PictureBox16_Click(sender As Object, e As EventArgs) Handles PictureBox16.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("This feature not ready")
    End Sub

    Private Sub Label21_Click(sender As Object, e As EventArgs) Handles Label21.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("This feature not ready")
    End Sub

    Private Sub Label22_Click(sender As Object, e As EventArgs) Handles Label22.Click
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = ("")
        Notifier.text_n.Text = ("This feature not ready")
    End Sub

    Private Sub PictureBox17_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox17.MouseMove
        noy.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label23_MouseMove(sender As Object, e As MouseEventArgs) Handles nott.MouseMove
        noy.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label24_MouseMove(sender As Object, e As MouseEventArgs) Handles noty.MouseMove
        noy.BackColor = ColorTranslator.FromHtml("#76B900")
    End Sub

    Private Sub Label24_MouseUp(sender As Object, e As MouseEventArgs) Handles noty.MouseUp
        noy.BackColor = Color.Gray
    End Sub

    Private Sub Label23_MouseLeave(sender As Object, e As EventArgs) Handles nott.MouseLeave
        noy.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox17_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox17.MouseLeave
        noy.BackColor = Color.Gray
    End Sub

    Private Sub PictureBox17_Click(sender As Object, e As EventArgs) Handles PictureBox17.Click
        If noty.ForeColor = Color.Gray Then
            noty.ForeColor = Color.White
            nott.ForeColor = Color.White
        Else
            noty.ForeColor = Color.Gray
            nott.ForeColor = Color.Gray
        End If
    End Sub

    Private Sub Label23_Click(sender As Object, e As EventArgs) Handles nott.Click
        If noty.ForeColor = Color.Gray Then
            noty.ForeColor = Color.White
            nott.ForeColor = Color.White
        Else
            noty.ForeColor = Color.Gray
            nott.ForeColor = Color.Gray
        End If
    End Sub

    Private Sub noty_Click(sender As Object, e As EventArgs) Handles noty.Click
        If noty.ForeColor = Color.Gray Then
            noty.ForeColor = Color.White
            nott.ForeColor = Color.White
        Else
            noty.ForeColor = Color.Gray
            nott.ForeColor = Color.Gray
        End If
    End Sub

    Private Sub nv_ty_Click(sender As Object, e As EventArgs) Handles nv_ty.Click
    End Sub

    Private Sub PictureBox6_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox6.MouseMove
        vs1.Visible = True
        vsr.Visible = True
        vsl.Visible = True
        vsb.Visible = True
    End Sub

    Private Sub Label10_MouseMove(sender As Object, e As MouseEventArgs) Handles Label10.MouseMove
        vs1.Visible = True
        vsr.Visible = True
        vsl.Visible = True
        vsb.Visible = True
    End Sub

    Private Sub PictureBox6_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox6.MouseLeave
        vs1.Visible = False
        vsr.Visible = False
        vsl.Visible = False
        vsb.Visible = False
    End Sub

    Private Sub Label10_MouseLeave(sender As Object, e As EventArgs) Handles Label10.MouseLeave
        vs1.Visible = False
        vsr.Visible = False
        vsl.Visible = False
        vsb.Visible = False
    End Sub

    Private Sub PictureBox6_Click(sender As Object, e As EventArgs) Handles PictureBox6.Click
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Then

            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Capture is open, please close first.")
        Else
            a_2.Visible = False
            action.Visible = False
            record_sc.Visible = False
            alt_z.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            set_vdo.Show()
            Opacity = 1
        End If
    End Sub

    Private Sub Label10_Click(sender As Object, e As EventArgs) Handles Label10.Click
        If replay_on.ForeColor = ColorTranslator.FromHtml("#76B900") Or logo_record.ForeColor = ColorTranslator.FromHtml("#76B900") Then

            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.ForeColor = Color.White
            Notifier.icon_n.Text = ("")
            Notifier.text_n.Text = ("Capture is open, please close first.")
        Else
            a_2.Visible = False
            action.Visible = False
            record_sc.Visible = False
            alt_z.Stop()
            alt_shift_f10.Stop()
            record_1.Stop()
            set_vdo.Show()
            Opacity = 1
        End If
    End Sub

    Private Sub Label1_MouseMove(sender As Object, e As MouseEventArgs) Handles Label1.MouseMove
        s1.Visible = True
        s1r.Visible = True
        s1l.Visible = True
        s1b.Visible = True
    End Sub

    Private Sub Label1_MouseLeave(sender As Object, e As EventArgs) Handles Label1.MouseLeave
        s1.Visible = False
        s1r.Visible = False
        s1l.Visible = False
        s1b.Visible = False
    End Sub

    Private Sub Label2_MouseMove(sender As Object, e As MouseEventArgs) Handles Label2.MouseMove
        s1.Visible = True
        s1r.Visible = True
        s1l.Visible = True
        s1b.Visible = True
    End Sub

    Private Sub Label2_MouseLeave(sender As Object, e As EventArgs) Handles Label2.MouseLeave
        s1.Visible = False
        s1r.Visible = False
        s1l.Visible = False
        s1b.Visible = False
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        Opacity = 1
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        settings_1.Visible = True ' แสดงฟอร์ม settings_1
        action.Visible = False ' ซ่อนฟอร์ม action
        replay_sc_all.Visible = False
        record_sc.Visible = False
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        Opacity = 1
        a_1.Visible = False
        a_2.Visible = False
        a_3.Visible = False
        settings_1.Visible = True ' แสดงฟอร์ม settings_1
        action.Visible = False ' ซ่อนฟอร์ม action
        replay_sc_all.Visible = False
        record_sc.Visible = False
    End Sub

    Private Sub settings_bg_MouseMove(sender As Object, e As MouseEventArgs) Handles settings_bg.MouseMove
        login.TopMost = True
    End Sub
End Class