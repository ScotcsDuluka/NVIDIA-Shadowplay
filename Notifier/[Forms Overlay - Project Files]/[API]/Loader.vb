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
        tcp = New TcpClientHelper("NVIDIA Notifier")
        AddHandler tcp.OnMessageReceived, AddressOf OnMessage

        LoadLanguage()
        InitNotifications()
        HideFromAltTab()

        obsCfg = ObsConfig.Load()
        If obsCfg.Enabled Then
            StartObsBridge()
        End If

        StartObsConfigWatcher()
    End Sub

    Private Sub StartObsConfigWatcher()
        obsConfigWatcher = New System.Windows.Forms.Timer()
        obsConfigWatcher.Interval = ObsConfigPollMs
        AddHandler obsConfigWatcher.Tick, AddressOf OnObsConfigWatcherTick
        obsConfigWatcher.Start()
        Debug.WriteLine($"[OBS] Config watcher started (every {ObsConfigPollMs}ms)")
    End Sub

    Private Sub OnObsConfigWatcherTick(sender As Object, e As EventArgs)
        Try
            If obsCfg Is Nothing Then Return
            If Not obsCfg.HasFileChanged() Then Return

            Debug.WriteLine("[OBS] notifier_obs.json changed — reloading…")
            If Not obsCfg.Reload() Then
                Debug.WriteLine("[OBS] reload failed — keeping previous config")
                Return
            End If

            If Not obsCfg.Enabled Then
                If obs IsNot Nothing Then
                    Debug.WriteLine("[OBS] config disabled — stopping OBS bridge")
                    obs.Dispose()
                    obs = Nothing
                End If
                Return
            End If

            If obs Is Nothing Then
                Debug.WriteLine("[OBS] config enabled — starting OBS bridge")
                StartObsBridge()
            Else
                obs.UpdateEndpoint(obsCfg.Host, obsCfg.Port, obsCfg.Password)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[OBS] Config watcher error: {ex.Message}")
        End Try
    End Sub

    Private Sub StartObsBridge()
        Try
            obs = New ObsWebSocketClient(obsCfg.Host, obsCfg.Port, obsCfg.Password, autoReconnect:=True)
            AddHandler obs.OnEvent, AddressOf OnObsEvent
            AddHandler obs.OnConnected, Sub()
                                            Debug.WriteLine("[OBS] Connected — bridge active")
                                        End Sub
            AddHandler obs.OnDisconnected, Sub()
                                              Debug.WriteLine("[OBS] Disconnected — will auto-reconnect")
                                          End Sub
            obs.Connect()
        Catch ex As Exception
            Debug.WriteLine($"[OBS] StartObsBridge error: {ex.Message}")
        End Try
    End Sub

    Private Sub OnObsEvent(eventType As String, eventData As Newtonsoft.Json.Linq.JObject, raw As Newtonsoft.Json.Linq.JObject)
        Try
            Debug.WriteLine($"[OBS] OnObsEvent: {eventType}")
            If Not obsCfg.ShouldForward(eventType) Then
                Debug.WriteLine($"[OBS]   → skipped by config")
                Return
            End If

            If eventType = "ReplayBufferSaved" Then
                If obs IsNot Nothing AndAlso obs.ShouldSuppressDuplicate("l10n.notificationInstantReplaySaved") Then Return
                If obs IsNot Nothing Then obs.MarkReplaySavedSuppression()
                HandleReplayBufferSaved()
                Return
            End If

            If eventType = "ReplayBufferStateChanged" AndAlso obs IsNot Nothing AndAlso obs.IsReplayStateSuppressed() Then
                Debug.WriteLine($"[OBS]   → ReplayBufferStateChanged suppressed (within save window)")
                Return
            End If

            Dim mapped = ObsEventMap.TryMap(eventType, eventData)
            If mapped Is Nothing Then
                Debug.WriteLine($"[OBS]   → no mapping for {eventType}")
                Return
            End If

            If obs IsNot Nothing AndAlso obs.ShouldSuppressDuplicate(mapped.Key) Then Return

            Debug.WriteLine($"[OBS]   → mapped to {mapped.Key}")
            Dim msg As String = $"[NVIDIA Overlay]|{mapped.Key}"

            If Me.InvokeRequired Then
                Me.Invoke(Sub() OnMessage(msg))
            Else
                OnMessage(msg)
            End If
        Catch ex As Exception
            Debug.WriteLine($"[OBS] OnObsEvent error ({eventType}): {ex.Message}")
        End Try
    End Sub

    Private Sub HandleReplayBufferSaved()
        Dim mins As Integer = 0
        Dim secs As Integer = 0

        Try
            Dim resp = obs.SendRequest("GetReplayBufferStatus")
            If resp IsNot Nothing Then
                Dim dataTok As Newtonsoft.Json.Linq.JToken = resp("responseData")
                If dataTok IsNot Nothing Then
                    Dim durTok As Newtonsoft.Json.Linq.JToken = dataTok("replayBufferDuration")
                    If durTok IsNot Nothing Then
                        Dim durMs As Double
                        Try
                            durMs = CDbl(durTok)
                        Catch
                            Try
                                durMs = CType(durTok, Long)
                            Catch
                                durMs = 0
                            End Try
                        End Try
                        Dim durSec As Integer = CInt(Math.Round(durMs / 1000.0))
                        mins = durSec \ 60
                        secs = durSec Mod 60
                        Debug.WriteLine($"[OBS]   → ReplayBufferSaved durMs={durMs} → {mins}m {secs}s")
                    End If
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"[OBS] GetReplayBufferStatus error: {ex.Message}")
        End Try

        Dim msg As String = $"[NVIDIA Overlay]|l10n.notificationInstantReplaySaved"
        Dim args As String() = {mins.ToString(), secs.ToString()}

        If Me.InvokeRequired Then
            Me.Invoke(Sub() OnMessageWithArgs(msg, args))
        Else
            OnMessageWithArgs(msg, args)
        End If
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
            ' แก้: เอา Application.DoEvents() ออก — กัน reentrancy

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

    ' แก้: SafeDelete ไม่นอนบน UI thread
    Public Sub SafeDelete(path As String)
        For i As Integer = 0 To 5
            Try
                If File.Exists(path) Then
                    File.Delete(path)
                End If
                Exit Sub
            Catch
                System.Threading.Tasks.Task.Delay(50).Wait()
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

    ' แก้: Dispose TCP + OBS + watcher ตอน form ปิด
    Private Sub Load_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If obsConfigWatcher IsNot Nothing Then
                obsConfigWatcher.Stop()
                obsConfigWatcher.Dispose()
                obsConfigWatcher = Nothing
            End If
        Catch
        End Try
        Try
            If tcp IsNot Nothing Then
                tcp.Disconnect()
                tcp.Dispose()
            End If
        Catch
        End Try
        Try
            If obs IsNot Nothing Then
                obs.Dispose()
            End If
        Catch
        End Try
    End Sub

    Private Sub RUN_API_Tick(sender As Object, e As EventArgs) Handles RUN_API.Tick

    End Sub
End Class