Imports System.IO
Imports System.Runtime.InteropServices

Public Class Load
    Inherits BlockClose

    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
            Return cp
        End Get
    End Property

    Private ReadOnly greenColor As Color = ColorTranslator.FromHtml("#76B900")
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
        If Me.IsHandleCreated Then
            Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
            Dim newStyle As Integer = (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
            SetWindowLong(Me.Handle, GWL_EXSTYLE, newStyle)
        End If
    End Sub

    Private Sub ShowNotification(iconText As String, messageText As String, showImage As Boolean)
        ' Ensure UI updates run on the UI thread
        If Me.InvokeRequired Then
            Me.Invoke(Sub() ShowNotification(iconText, messageText, showImage))
            Return
        End If

        Try
            Notifier.Show()
        Catch
            ' ignore if already shown or not available
        End Try

        Try
            Notifier_Sub.icon_n.Text = iconText
            Notifier_Sub.text_n.Text = messageText
            Notifier_Sub.PictureBox1.Visible = showImage
            Notifier_Sub.TopMost = True
            Notifier_Sub.Show()
        Catch
            ' If controls/forms are not available, fail silently
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Notifier.Show()
        Notifier_Sub.icon_n.Text = ""
        Notifier_Sub.text_n.Text = "Press Alt + Z to use Shadowplay Experience in-game overlay"
        Notifier_Sub.PictureBox1.Visible = True
    End Sub
    Private Function GetSavedReplayDuration() As (minutes As Integer, seconds As Integer)
        Dim dataDir As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data\Replay")
        Dim minutes As Integer = 0
        Dim seconds As Integer = 0

        Try
            For Each file As String In Directory.GetFiles(dataDir, "*.m")
                Dim fileName As String = Path.GetFileNameWithoutExtension(file)
                Integer.TryParse(fileName, minutes)
                Exit For
            Next

            For Each file As String In Directory.GetFiles(dataDir, "*.s")
                Dim fileName As String = Path.GetFileNameWithoutExtension(file)
                Integer.TryParse(fileName, seconds)
                Exit For
            Next
        Catch ex As Exception
            Console.WriteLine("Error: " & ex.Message)
        End Try

        Return (minutes, seconds)
    End Function
    Private Sub RUN_API_Tick(sender As Object, e As EventArgs) Handles RUN_API.Tick
        ' 1. Language Handling
        Dim langFolder As String = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile As String = Path.Combine(langFolder, "current.txt")

        Dim currentLang As String = "en-US"
        If File.Exists(currentFile) Then
            currentLang = File.ReadAllText(currentFile).Trim()
        End If

        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        LangHelper.LoadLang(langFile)
        GetSavedReplayDuration()
        Dim checks As New Dictionary(Of String, (Message As String, ShowImage As Boolean, Icon As String, IconColor As Color)) From {
        {"l10n.notificationOpenShare", (LangHelper.GetText("l10n.notificationOpenShare", "Alt + Z"), True, "", Color.White)},
        {"notuse", (LangHelper.GetText("l10n.notificationWarningGameRequired"), False, "", Color.White)},
        {"l10n.notificationWarningNvidiaGpuRequired", (LangHelper.GetText("l10n.notificationWarningNvidiaGpuRequired"), False, "", Color.White)},
        {"l10n.Screenshot", (LangHelper.GetText("l10n.notificationScreenshotSavedToGallery"), False, "", Color.White)},
        {"l10n.validsavepath", (LangHelper.GetText("l10n.validsavepath"), False, "", Color.White)},
        {"l10n.recording_started", (LangHelper.GetText("l10n.notificationManualRecordStarted"), False, "", greenColor)},
        {"l10n.recording_saved", (LangHelper.GetText("l10n.notificationManualRecordStopped"), False, "", Color.White)},
        {"l10n.replay_error", (LangHelper.GetText("l10n.notificationReplaySaveError"), False, "", Color.White)},
        {"l10n.replay_turn_on", (LangHelper.GetText("l10n.notificationTurnOnInstantReplay"), False, "", Color.White)},
        {"l10n.instant_replay_off", (LangHelper.GetText("l10n.notificationInstantReplayStopped"), False, "", Color.White)},
        {"l10n.instant_replay_on", (LangHelper.GetText("l10n.notificationInstantReplayStarted"), False, "", greenColor)},
        {"l10n.photo_mode_error", (LangHelper.GetText("l10n.notificationWarningPhotographyNotAllowed"), False, "", Color.White)},
        {"l10n.feature_not_ready", (LangHelper.GetText("l10n.notificationFeatureNotReady"), False, "", Color.White)},
        {"l10n.extension_not_found", (LangHelper.GetText("l10n.notificationCustomOverlayFileNotFound"), False, "", Color.White)},
        {"l10n.error_confirm", (LangHelper.GetText("l10n.notificationaccountconfirmerror"), False, "", Color.White)},
        {"l10n.saved_last_15", (LangHelper.GetText("l10n.notificationInstantReplaySaved", GetSavedReplayDuration.minutes, GetSavedReplayDuration.seconds), False, "", Color.White)},
        {"l10n.notifier_open", (LangHelper.GetText("l10n.notifierOpen"), False, "", Color.White)},
        {"l10n.notifier_not_using", (LangHelper.GetText("l10n.notifierNotUsing"), False, "", Color.White)},
        {"l10n.app_closed", (LangHelper.GetText("l10n.notificationAppClosed"), False, "", Color.White)},
        {"l10n.shared_close", (LangHelper.GetText("l10n.notificationSharedClose"), False, "", Color.White)},
        {"l10n.update_available", (LangHelper.GetText("l10n.notificationUpdateAvailable"), False, "", Color.White)},
        {"l10n.privacy", (LangHelper.GetText("l10n.notificationWarningDesktopCaptureDisabled"), False, "", Color.White)},
        {"l10n.version_latest", (LangHelper.GetText("l10n.notificationVersionLatest"), False, "", Color.White)},
        {"l10n.account_confirm_error", (LangHelper.GetText("l10n.notificationaccountconfirmerror"), False, "", Color.White)},
        {"l10n.ErrorResolution", (LangHelper.GetText("l10n.notificationErrorResolution"), False, "", Color.White)},
        {"l10n.error_general", (LangHelper.GetText("l10n.notificationErrorGeneral"), False, "", Color.White)},
        {"l10n.foldererror", (LangHelper.GetText("l10n.foldererror"), False, "", Color.White)},
        {"l10n.Capture_notuse", (LangHelper.GetText("l10n.Capture_notuse"), False, "", Color.White)},
        {"l10n.openLocation", (LangHelper.GetText("l10n.openLocation"), False, "", Color.White)}
    }


        ' Path to the special "py" file
        Dim pyFilePath As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "privacy")
        Dim isPyActive As Boolean = File.Exists(pyFilePath)

        ' 3. Iterate through checks
        For Each kvp In checks
            Dim filePath = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", kvp.Key)

            If File.Exists(filePath) Then
                ' ดึงค่าจาก Dictionary (เพิ่ม displayColor เข้ามา)
                Dim displayMessage As String = kvp.Value.Message
                Dim displayIcon As String = kvp.Value.Icon
                Dim showImage As Boolean = kvp.Value.ShowImage
                Dim displayColor As Color = kvp.Value.IconColor

                ' LOGIC: 
                If Not isPyActive AndAlso displayIcon <> "" Then
                    displayIcon = ""
                    displayMessage = "Privacy control capture has off. Turn on to use"
                    showImage = False
                    displayColor = Color.White
                End If

                ' 4. Update UI 
                UpdateNotifier(displayMessage, showImage, displayIcon, displayColor)

                ' 5. Cleanup
                SafeDelete(filePath)
            End If
        Next
        Dim filePaths As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifier")

        Try
            If Notifier.Visible Then
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

        Dim filePathss As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "notifiermainoff")

        Try
            If File.Exists(filePathss) Then
                File.Delete(filePathss)
                Notifier.IF_N.Start()
                Notifier_Sub.Timer1.Start()
            End If

        Catch ex As Exception
            Debug.WriteLine("Error handling notifier file: " & ex.Message)
        End Try
    End Sub








    Public Class NotificationData
        Public Property Key As String
        Public Property Ico As String = ""
        Public Property Png As Boolean = False
        Public Property Color As Color = Color.White
        Public Property Args As String() = {}

        Public Sub New(key As String, Optional ico As String = "", Optional png As Boolean = False, Optional color As Color = Nothing, Optional args As String() = Nothing)
            Me.Key = key
            Me.Ico = ico
            Me.Png = png
            Me.Color = If(color = Nothing, Color.White, color)
            Me.Args = If(args, {})
        End Sub
    End Class

    Private ReadOnly notifications As New List(Of NotificationData)()

    Public Sub New()
        InitializeComponent()

        ' === Notifications without args ===
        notifications.Add(New NotificationData("l10n.test", "", False, Color.White))

        ' === Notifications with args ===
        notifications.Add(New NotificationData("l10n.testarg", "", False, Color.White, {"1", "2"}))
        notifications.Add(New NotificationData("l10n.notificationInstantReplaySaved", "", False, Color.White, {GetSavedReplayDuration.minutes, GetSavedReplayDuration.seconds}))

    End Sub
    Private Sub RUN_NEW_Tick(sender As Object, e As EventArgs) Handles RUN_API.Tick
        ' Language Handling
        Dim langFolder As String = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile As String = Path.Combine(langFolder, "current.txt")
        Dim currentLang As String = "en-US"
        If File.Exists(currentFile) Then
            currentLang = File.ReadAllText(currentFile).Trim()
        End If
        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        LangHelper.LoadLang(langFile)

        ' Privacy check
        Dim pyFilePath As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data", "privacy")
        Dim isPyActive As Boolean = File.Exists(pyFilePath)

        ' Process notifications
        Dim dataDir As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data")

        For Each data As NotificationData In notifications
            Dim filePath As String = Path.Combine(dataDir, data.Key)

            If File.Exists(filePath) Then
                Dim message As String = LangHelper.GetText(data.Key, data.Args)
                Dim icon As String = data.Ico
                Dim showImage As Boolean = data.Png
                Dim iconColor As Color = data.Color

                ' Privacy check
                If Not isPyActive AndAlso icon <> "" Then
                    icon = ""
                    message = "Privacy control capture has off. Turn on to use"
                    showImage = False
                    iconColor = Color.White
                End If

                UpdateNotifier(message, showImage, icon, iconColor)
                SafeDelete(filePath)
                Dim dataDira As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data\Replay")

                If Directory.Exists(dataDira) Then
                    ' ลบไฟล์ทั้งหมดในโฟลเดอร์
                    For Each files As String In Directory.GetFiles(dataDir)
                        File.Delete(files)
                    Next

                    ' ถ้าอยากลบโฟลเดอร์ย่อยทั้งหมดด้วย
                    For Each dir As String In Directory.GetDirectories(dataDir)
                        Directory.Delete(dir, True) ' True = ลบพร้อมเนื้อหาในโฟลเดอร์
                    Next
                End If
            End If
        Next

        ' Notifier state tracking
        Dim filePaths As String = Path.Combine(dataDir, "notifier")
        Try
            If Notifier.Visible Then
                If Not File.Exists(filePaths) Then File.Create(filePaths).Dispose()
            Else
                If File.Exists(filePaths) Then File.Delete(filePaths)
            End If
        Catch
        End Try

        Dim filePathss As String = Path.Combine(dataDir, "notifiermainoff")
        Try
            If File.Exists(filePathss) Then
                File.Delete(filePathss)
                Notifier.IF_N.Start()
                Notifier_Sub.Timer1.Start()
            End If
        Catch
        End Try
    End Sub














    Private Sub UpdateNotifier(message As String, showImage As Boolean, icon As String, iconColor As Color)
        Notifier_Sub.TopMost = True
        Notifier.Show()

        With Notifier_Sub.icon_n
            .Font = New Font(.Font.FontFamily, 35)
            .ForeColor = iconColor
            .Text = icon
        End With

        Notifier_Sub.text_n.Text = message
        Notifier_Sub.PictureBox1.Visible = showImage
    End Sub
    Private Sub SafeDelete(path As String)

        For i As Integer = 0 To 5
            Try
                If File.Exists(path) Then
                    File.Delete(path)
                End If
                Exit Sub
            Catch
                Threading.Thread.Sleep(50)
            End Try
        Next

    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Notifier_Sub.Show()
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        HideFromAltTab()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        Dim langFolder As String = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile As String = Path.Combine(langFolder, "current.txt")

        Dim currentLang As String = "en-US"
        If File.Exists(currentFile) Then
            currentLang = File.ReadAllText(currentFile).Trim()
        End If

        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        LangHelper.LoadLang(langFile)


    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Notifier.IF_N.Start()
        Notifier_Sub.Timer1.Start()
    End Sub
End Class
