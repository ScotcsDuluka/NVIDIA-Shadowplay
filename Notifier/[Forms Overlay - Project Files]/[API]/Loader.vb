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
            ObsLog($"OnObsEvent: {eventType}")
            If Not obsCfg.ShouldForward(eventType) Then Return

            If eventType = "ReplayBufferSaved" Then
                If Not ShouldShowToast("l10n.notificationInstantReplaySaved") Then Return
                Task.Run(Sub() HandleReplayBufferSaved(eventData))
                Return
            End If

            Dim mapped = ObsEventMap.TryMap(eventType, eventData)
            If mapped Is Nothing Then Return

            If Not ShouldShowToast(mapped.Key) Then Return

            Debug.WriteLine($"[OBS]   → mapped to {mapped.Key}")
            ObsLog($"  → mapped to {mapped.Key}")
            Dim msg As String = $"[NVIDIA Overlay]|{mapped.Key}"

            If Me.InvokeRequired Then
                Me.Invoke(Sub() OnMessage(msg))
            Else
                OnMessage(msg)
            End If
        Catch ex As Exception
            ObsLog($"OnObsEvent error ({eventType}): {ex.Message}")
        End Try
    End Sub

    Private _lastToastTime As DateTime = DateTime.MinValue
    Private Const ToastThrottleMs As Integer = 300

    Private Function ShouldShowToast(key As String) As Boolean
        Dim now As DateTime = DateTime.Now
        If (now - _lastToastTime).TotalMilliseconds < ToastThrottleMs Then
            ObsLog($"  → throttled (within {ToastThrottleMs}ms of last toast) — suppressed: {key}")
            Return False
        End If
        _lastToastTime = now
        Return True
    End Function

    Private Sub ObsLog(message As String)
        Debug.WriteLine($"[OBS] {message}")
        Try
            Dim logPath As String = AppLayout.P("Logs", "notifier_obs.log")
            Using fs As New FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
                Using sw As New StreamWriter(fs)
                    sw.WriteLine($"[OBS] {DateTime.Now:HH:mm:ss.fff} {message}")
                End Using
            End Using
        Catch
        End Try
    End Sub

    Private _lastRecordKey As String = ""
    Private _lastReplayKey As String = ""
    Private Const StateDedupWindowMs As Integer = 1500
    Private _lastRecordTime As DateTime = DateTime.MinValue
    Private _lastReplayTime As DateTime = DateTime.MinValue

    Private Sub HandleReplayBufferSaved(eventData As Newtonsoft.Json.Linq.JObject)
        ObsLog("HandleReplayBufferSaved: start")
        Dim mins As Integer = 0
        Dim secs As Integer = 0

        Dim savedPath As String = ""
        If eventData IsNot Nothing Then
            Dim savedPathTok As Newtonsoft.Json.Linq.JToken = eventData("savedReplayPath")
            If savedPathTok IsNot Nothing Then
                Try
                    savedPath = CType(savedPathTok, String)
                Catch
                    Try
                        savedPath = CStr(savedPathTok)
                    Catch
                        savedPath = ""
                    End Try
                End Try
            End If
        End If

        If Not String.IsNullOrEmpty(savedPath) Then
            ObsLog($"HandleReplayBufferSaved: savedReplayPath={savedPath}")
            Dim durSec As Integer = ReadVideoDurationSeconds(savedPath)
            If durSec > 0 Then
                mins = durSec \ 60
                secs = durSec Mod 60
                ObsLog($"HandleReplayBufferSaved: ffprobe duration={durSec}s → {mins}m {secs}s")
            Else
                ObsLog("HandleReplayBufferSaved: could not read duration from video file")
            End If
        Else
            ObsLog("HandleReplayBufferSaved: savedReplayPath empty")
        End If

        Dim msg As String = $"[NVIDIA Overlay]|l10n.notificationInstantReplaySaved"
        Dim args As String() = {mins.ToString(), secs.ToString()}

        ObsLog($"HandleReplayBufferSaved: showing toast with args=[{mins}, {secs}]")
        If Me.InvokeRequired Then
            Me.Invoke(Sub() OnMessageWithArgs(msg, args))
        Else
            OnMessageWithArgs(msg, args)
        End If
    End Sub

    Private Function ReadVideoDurationSeconds(videoPath As String) As Integer
        If String.IsNullOrEmpty(videoPath) Then Return 0
        If Not File.Exists(videoPath) Then
            ObsLog($"ReadVideoDurationSeconds: file not found: {videoPath}")
            Return 0
        End If

        Try
            Dim startupPath As String = AppLayout.Dir
            ObsLog($"ReadVideoDurationSeconds: layout root={startupPath}")

            Dim candidates As String() = {
                AppLayout.P("FFmpeg", "ffprobe.exe"),
                Path.Combine(startupPath, "API-Core", "ffprobe.exe"),
                Path.Combine(startupPath, "ffprobe.exe"),
                Path.Combine(startupPath, "ffmpeg", "ffprobe.exe"),
                Path.Combine(startupPath, "..", "API-Core", "ffprobe.exe"),
                Path.Combine(startupPath, "..", "..", "API-Core", "ffprobe.exe"),
                Path.Combine(startupPath, "..", "..", "..", "API-Core", "ffprobe.exe"),
                Path.Combine(startupPath, "..", "..", "..", "..", "API-Core", "ffprobe.exe"),
                Path.Combine(startupPath, "..", "..", "..", "..", "..", "Overlay", "bin", "Release", "net10.0-windows10.0.26100.0", "API-Core", "ffprobe.exe"),
                Path.Combine(startupPath, "..", "..", "..", "..", "..", "Overlay", "bin", "x64", "Release", "net10.0-windows10.0.26100.0", "API-Core", "ffprobe.exe")
            }

            Dim ffprobePath As String = ""
            For Each c In candidates
                Dim exists As Boolean = File.Exists(c)
                ObsLog($"ReadVideoDurationSeconds: trying {c} → {If(exists, "EXISTS", "missing")}")
                If exists Then
                    ffprobePath = c
                    Exit For
                End If
            Next

            If String.IsNullOrEmpty(ffprobePath) Then
                ObsLog("ReadVideoDurationSeconds: ffprobe.exe not found in any candidate path")
                Return 0
            End If

            Dim psi As New ProcessStartInfo()
            psi.FileName = ffprobePath
            psi.Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 ""{videoPath}"""
            psi.UseShellExecute = False
            psi.RedirectStandardOutput = True
            psi.RedirectStandardError = True
            psi.CreateNoWindow = True

            Using p As Process = Process.Start(psi)
                Dim stdout As String = p.StandardOutput.ReadToEnd().Trim()
                p.WaitForExit(3000)
                If p.ExitCode <> 0 Then
                    Dim stderr As String = p.StandardError.ReadToEnd().Trim()
                    ObsLog($"ReadVideoDurationSeconds: ffprobe exit={p.ExitCode} err={stderr}")
                    Return 0
                End If
                ObsLog($"ReadVideoDurationSeconds: ffprobe stdout=""{stdout}""")
                Dim durSec As Double
                If Double.TryParse(stdout, durSec) Then
                    Return CInt(Math.Floor(durSec))
                End If
            End Using
        Catch ex As Exception
            ObsLog($"ReadVideoDurationSeconds error: {ex.Message}")
        End Try

        Return 0
    End Function

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        HideFromAltTab()
    End Sub

    Private Sub LoadLanguage(Optional langCode As String = Nothing)
        Dim langFolder As String = AppLayout.P("Languages")
        ' Single-source config: the current language lives in config.json
        ' UI.Language (was: the Languages/current.txt pointer file).
        Dim currentLang As String = If(langCode, AppConfigShared.ReadString("UI", "Language", "en-US"))
        Dim langFile As String = Path.Combine(langFolder, currentLang & ".json")
        LangHelper.LoadLang(langFile)
    End Sub

    ' ==== Toast slot routing (OWNER spec) ====
    ' Rapid bursts — a toast arriving within FastArrivalWindowMs of the previous
    ' one — go to the first FREE slot instead of clobbering the showing one.
    ' Slot 1 = main toast (original flow, below). Slots 2/3 = copies stacking
    ' below the main toast with a 10px vertical gap (see the slot forms).
    ' If every slot is busy, fall back to the classic replace dance on slot 1.
    Private _lastToastArrival As DateTime = DateTime.MinValue

    ' OWNER spec: rapid = a toast arriving within 400ms of the previous one.
    Private Const FastArrivalWindowMs As Integer = 400

    ' Which notification key each slot is currently showing. A repeat of the
    ' same key while that toast is still live = a toggle → refresh the live
    ' toast IN PLACE (content swap + restart the 6s close window) instead of
    ' spawning a new toast or running the slide-out/slide-in dance.
    ' ("Updater UI" style — same behavior the T27 router had, on the old forms.)
    Private _mainToastKey As String = ""
    Private _side2ToastKey As String = ""
    Private _side3ToastKey As String = ""

    Private Shared Function SameToastKey(a As String, b As String) As Boolean
        Return Not String.IsNullOrEmpty(a) AndAlso
               String.Equals(a, b, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function MainSlotBusy() As Boolean
        Return Notifier.Visible OrElse Notifier.Notifier_green_stop.Visible
    End Function

    Private Function TryShowInFreeSideSlot(message As String, showImage As Boolean, icon As String, iconColor As Color, key As String) As Boolean
        If Not SideSlotBusy(Notifier2) Then
            ShowSideSlot(Notifier2, Notifier_Sub2, message, showImage, icon, iconColor, key)
            Return True
        End If
        If Not SideSlotBusy(Notifier3) Then
            ShowSideSlot(Notifier3, Notifier_Sub3, message, showImage, icon, iconColor, key)
            Return True
        End If
        Return False
    End Function

    ' A side slot is busy from the moment its unit form is shown (the whole
    ' slide-in dance) until its SlideOutAll closes it — Visible covers exactly
    ' that window, green_stop.Visible alone would miss the dance.
    Private Function SideSlotBusy(unit As Notifier2) As Boolean
        Return unit.Visible OrElse unit.Notifier_green_stop.Visible
    End Function

    Private Function SideSlotBusy(unit As Notifier3) As Boolean
        Return unit.Visible OrElse unit.Notifier_green_stop.Visible
    End Function

    ' Mirrors the main toast's fresh-show path: Show() runs the Form_Load
    ' slide-in dance; content is set up front exactly like the main path does.
    ' NOTE: no autoClose.Start() here — a fresh unit's Timer still has the 100ms
    ' default Interval until its Form_Load dance sets 6000, so starting it here
    ' would auto-close the toast mid-dance. Form_Load owns the timer instead.
    Private Sub ShowSideSlot(unit As Notifier2, content As Notifier_Sub2, message As String, showImage As Boolean, icon As String, iconColor As Color, key As String)
        unit.autoClose.Stop()
        content.TopMost = True
        unit.Show()
        _side2ToastKey = key
        With content.icon_n
            .Font = New Font(.Font.FontFamily, 35)
            .ForeColor = iconColor
            .Text = icon
        End With
        content.text_n.Text = message
        content.PictureBox1.Visible = showImage
    End Sub

    Private Sub ShowSideSlot(unit As Notifier3, content As Notifier_Sub3, message As String, showImage As Boolean, icon As String, iconColor As Color, key As String)
        unit.autoClose.Stop()
        content.TopMost = True
        unit.Show()
        _side3ToastKey = key
        With content.icon_n
            .Font = New Font(.Font.FontFamily, 35)
            .ForeColor = iconColor
            .Text = icon
        End With
        content.text_n.Text = message
        content.PictureBox1.Visible = showImage
    End Sub

    ' ฟังก์ชัน UpdateNotifier (จัดการ UI)
    ' key = the notification key this toast belongs to (l10n.* / raw key).
    ' Same key arriving again while its toast is still live → in-place refresh.
    Public Sub UpdateNotifier(message As String, showImage As Boolean, icon As String, iconColor As Color, Optional key As String = Nothing)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() UpdateNotifier(message, showImage, icon, iconColor, key))
            Return
        End If

        ' tcp.SendLog("notifier_show") ' ถ้าต้องการ

        Dim now As DateTime = DateTime.Now
        Dim fastArrival As Boolean = (now - _lastToastArrival).TotalMilliseconds <= FastArrivalWindowMs
        _lastToastArrival = now

        ' 1) TOGGLE — same key already live on a slot → refresh that toast IN
        '    PLACE ("Updater UI"): swap content + restart the 6s close window.
        '    No new toast, no slide dance, nothing moves.
        If Not String.IsNullOrEmpty(key) Then
            If MainSlotBusy() AndAlso SameToastKey(_mainToastKey, key) Then
                With Notifier_Sub.icon_n
                    .Font = New Font(.Font.FontFamily, 35)
                    .ForeColor = iconColor
                    .Text = icon
                End With
                Notifier_Sub.text_n.Text = message
                Notifier_Sub.PictureBox1.Visible = showImage
                If Notifier.Notifier_green_stop.Visible Then
                    Notifier.autoClose.Stop()
                    Notifier.autoClose.Start()
                End If
                Exit Sub
            End If
            If SideSlotBusy(Notifier2) AndAlso SameToastKey(_side2ToastKey, key) Then
                With Notifier_Sub2.icon_n
                    .Font = New Font(.Font.FontFamily, 35)
                    .ForeColor = iconColor
                    .Text = icon
                End With
                Notifier_Sub2.text_n.Text = message
                Notifier_Sub2.PictureBox1.Visible = showImage
                Notifier2.autoClose.Stop()
                Notifier2.autoClose.Start()
                Exit Sub
            End If
            If SideSlotBusy(Notifier3) AndAlso SameToastKey(_side3ToastKey, key) Then
                With Notifier_Sub3.icon_n
                    .Font = New Font(.Font.FontFamily, 35)
                    .ForeColor = iconColor
                    .Text = icon
                End With
                Notifier_Sub3.text_n.Text = message
                Notifier_Sub3.PictureBox1.Visible = showImage
                Notifier3.autoClose.Stop()
                Notifier3.autoClose.Start()
                Exit Sub
            End If
        End If

        ' 2) Rapid burst + main slot busy → route to the first free side slot.
        ' When the main slot is free we fall through to the normal fresh-show
        ' path below (slot preference order: main → 2 → 3). If ALL slots are
        ' busy we also fall through into the classic replace dance.
        If fastArrival AndAlso MainSlotBusy() Then
            If TryShowInFreeSideSlot(message, showImage, icon, iconColor, key) Then Exit Sub
        End If

        ' 3) Classic single-slot flow (fresh show / replace dance on the main
        ' toast) — this slot now owns `key` for toggle matching.
        _mainToastKey = key

        ' Refresh the close window only while a toast is actually showing —
        ' on a fresh show Form_Load owns autoClose (and its Interval), so
        ' starting the timer here would fire AutoClose at the 100ms default.
        If Notifier.Notifier_green_stop.Visible Then
            Notifier.autoClose.Stop()
            Notifier.autoClose.Start()
        End If
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

    ' Test buttons — go through the REAL UpdateNotifier with a stable fake key,
    ' so double-pressing Button1 demos the toggle in-place refresh exactly like
    ' a real repeat notification does.
    Private Const TestButtonKey As String = "l10n.testbutton"

    ' ฟังก์ชัน GetSavedReplayDuration
    Public Function GetSavedReplayDuration() As (minutes As Integer, seconds As Integer)
        Dim dataDir As String = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "Replay")
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
        Dim replayDir = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "Replay")
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
        Dim dataDir = AppLayout.P("Data", "NVIDIA_Shadowplay_Data")
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
        ' Real router path — press twice fast: first press = fresh show,
        ' second press within 6s = toggle in-place refresh (no dance).
        UpdateNotifier("Press Alt + Z to use Shadowplay Experience in-game overlay",
                       True, "", Color.White, TestButtonKey)
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
End Class