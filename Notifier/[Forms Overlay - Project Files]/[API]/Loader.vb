Imports System.IO
Imports System.Runtime.InteropServices

Partial Public Class Loader
    Inherits Form
    ' ส่วน API และสไตล์ฟอร์ม
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

    ' Events ฟอร์ม
    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' TCP Client
        tcp = New TcpClientHelper("NVIDIA Notifier")
        AddHandler tcp.OnMessageReceived, AddressOf OnMessage
        ' เปิดเชื่อมต่อ TCP (ปรับตามจริง)
        ' tcp.Connect("127.0.0.1", 12345) ' หรือ .Start()

        ' โหลดภาษาครั้งแรก
        LoadLanguage()

        ' สร้างรายการแจ้งเตือน
        InitNotifications()

        ' ซ่อนจาก Alt+Tab
        HideFromAltTab()
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        HideFromAltTab()
    End Sub

    Private Sub LoadLanguage(Optional langCode As String = Nothing)
        Dim langFolder As String = Path.Combine(Application.StartupPath, "Languages")
        Dim currentFile As String = Path.Combine(langFolder, "current.txt")
        Dim currentLang As String = If(langCode, "en-US")
        If String.IsNullOrEmpty(langCode) AndAlso File.Exists(currentFile) Then
            currentLang = File.ReadAllText(currentFile).Trim()
        End If
        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        LangHelper.LoadLang(langFile)
    End Sub

    ' ฟังก์ชัน UpdateNotifier (จัดการ UI)
    Public Sub UpdateNotifier(message As String, showImage As Boolean, icon As String, iconColor As Color)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() UpdateNotifier(message, showImage, icon, iconColor))
            Return
        End If

        ' tcp.SendLog("notifier_show") ' ถ้าต้องการ

        Notifier.autoClose.Stop()
        Notifier.autoClose.Start()
        Notifier_Sub.TopMost = True

        ' Logic เดิมเรื่องการสไลด์
        If Notifier.Notifier_green_stop.Visible Then
            Notifier_Sub.Close()
            Application.DoEvents()

            Notifier.StartSlide(Notifier.Notifier_black, Notifier.Notifier_black.Left, Notifier.Width + 300, 600)

            With Notifier_Sub.icon_n
                .Font = New Font(.Font.FontFamily, 35)
                .ForeColor = iconColor
                .Text = icon
            End With
            Notifier_Sub.text_n.Text = message
            Notifier_Sub.PictureBox1.Visible = showImage

            Dim delay As New Timer()
            delay.Interval = 200
            AddHandler delay.Tick, Sub()
                                       delay.Stop()
                                       delay.Dispose()
                                       Notifier.StartSlide(Notifier.Notifier_black, Notifier.Width, Notifier.Width - 300, 300,
                                           Sub() Notifier_Sub.Show())
                                   End Sub
            delay.Start()
            Exit Sub
        End If

        Notifier.Show()
        With Notifier_Sub.icon_n
            .Font = New Font(.Font.FontFamily, 35)
            .ForeColor = iconColor
            .Text = icon
        End With
        Notifier_Sub.text_n.Text = message
        Notifier_Sub.PictureBox1.Visible = showImage
    End Sub

    ' ฟังก์ชัน GetSavedReplayDuration
    Public Function GetSavedReplayDuration() As (minutes As Integer, seconds As Integer)
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
            Console.WriteLine("Error reading replay duration: " & ex.Message)
        End Try
        Return (minutes, seconds)
    End Function

    ' SafeDelete
    Public Sub SafeDelete(path As String)
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

    ' จัดการไฟล์ Replay หลังอ่าน
    Public Sub DeleteReplayFiles()
        Dim replayDir = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data\Replay")
        Try
            If Directory.Exists(replayDir) Then
                For Each f In Directory.GetFiles(replayDir)
                    File.Delete(f)
                Next
                For Each d In Directory.GetDirectories(replayDir)
                    Directory.Delete(d, True)
                Next
            End If
        Catch
        End Try
    End Sub

    ' จัดการสถานะ Notifier
    Public Sub ManageNotifierState()
        Dim dataDir = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data")
        Dim notifierFile = Path.Combine(dataDir, "notifier")
        Dim mainOffFile = Path.Combine(dataDir, "notifiermainoff")
        Try
            If Notifier.Visible Then
                If Not File.Exists(notifierFile) Then File.Create(notifierFile).Dispose()
            Else
                If File.Exists(notifierFile) Then File.Delete(notifierFile)
            End If
        Catch
        End Try
        Try
            If File.Exists(mainOffFile) Then
                File.Delete(mainOffFile)
                Notifier.IF_N.Start()
                Notifier_Sub.Timer1.Start()
            End If
        Catch
        End Try
    End Sub

    ' ปุ่มทดสอบ
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Notifier.Show()
        Notifier_Sub.icon_n.Text = ""
        Notifier_Sub.text_n.Text = "Press Alt + Z to use Shadowplay Experience in-game overlay"
        Notifier_Sub.PictureBox1.Visible = True
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Notifier_Sub.Show()
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Notifier.IF_N.Start()
        Notifier_Sub.Timer1.Start()
    End Sub

    Private Sub Load_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Dispose TCP
        ' tcp?.Dispose()
    End Sub

    Private Sub RUN_API_Tick(sender As Object, e As EventArgs) Handles RUN_API.Tick

    End Sub
End Class