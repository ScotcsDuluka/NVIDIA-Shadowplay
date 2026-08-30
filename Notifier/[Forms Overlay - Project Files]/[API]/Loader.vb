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

        ' T30 RaiseStack: 100ms heartbeat drives reflow / queue / topmost.
        StartStackHeartbeat()

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

    ' ==== Toast slot routing (OWNER spec T29.2/T29.3) ====
    ' Slot COUNT is user-configurable: config.json Notifications.SlotCount,
    ' 1, 2 or 3 (Settings - General: "Use a second toast slot" + "Use a
    ' third toast slot"; default 2). The Notifier re-reads it on EVERY
    ' toast, so flipping the toggles in Settings applies live - no
    ' restart. Slot 3 is the overflow slot: it only joins the routing in
    ' 3-slot mode, taking a new group when main AND slot 2 are both busy.
    '
    ' Group model - toasts carry a GROUP (recording start/stop = ONE group,
    ' replay start/stop = another - see NotificationGroup in the TCP client):
    '   1. A group already live on a slot -> "Updater UI": the classic
    '      slide-out/slide-in dance replaces the content IN THAT SLOT, and
    '      the 6s close window restarts. Its start/stop toasts keep
    '      updating each other where they live - never a stacked duplicate.
    '   2. A NEW group -> the first FREE slot gets a fresh show (Record
    '      start/stop living in slot 2 + Replay arriving -> Replay takes
    '      slot 1 - OWNER example).
    '   3. Every slot busy (or 1-slot mode) -> the classic replace dance on
    '      the main toast - a new toast is never dropped silently.
    Private _mainGroup As String = ""
    Private _side2Group As String = ""
    Private _side3Group As String = ""

    Private Shared Function SameToastGroup(a As String, b As String) As Boolean
        Return Not String.IsNullOrEmpty(a) AndAlso
               String.Equals(a, b, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function MainSlotBusy() As Boolean
        Return Notifier.Visible OrElse Notifier.Notifier_green_stop.Visible
    End Function

    Private Shared Function ConfiguredSlotCount() As Integer
        Dim n As Integer = AppConfigShared.ReadInt("Notifications", "SlotCount", 2)
        If n < 1 Then n = 1
        If n > 3 Then n = 3
        Return n
    End Function

    ' A side slot is busy from the moment its unit form is shown (the whole
    ' slide-in dance) until its SlideOutAll closes it - Visible covers exactly
    ' that window, green_stop.Visible alone would miss the dance.
    Private Function SideSlotBusy(unit As Notifier2) As Boolean
        Return unit.Visible OrElse unit.Notifier_green_stop.Visible
    End Function

    ' Same liveness test for the slot 3 unit (T29.3: 3-slot mode).
    Private Function SideSlotBusy(unit As Notifier3) As Boolean
        Return unit.Visible OrElse unit.Notifier_green_stop.Visible
    End Function

    ' Fresh show on slot 3 - mirror of ShowSideSlot (see the notes there:
    ' Form_Load owns the autoClose timer on a fresh unit, so starting it
    ' here would auto-close the toast mid-dance).
    Private Sub ShowSideSlot3(content As Notifier_Sub3, message As String, showImage As Boolean, icon As String, iconColor As Color, group As String)
        _side3Group = group
        Notifier3.autoClose.Stop()
        ' T30.1: no TopMost setter (can activate) - RaiseUnit()/heartbeat own z-order.
        Notifier3.Show()
        With content.icon_n
            .Font = New Font(.Font.FontFamily, 35)
            .ForeColor = iconColor
            .Text = icon
        End With
        content.text_n.Text = message
        content.PictureBox1.Visible = showImage
    End Sub

    ' Updater UI on slot 3 - mirror of DanceSideToast on the slot 3 unit:
    ' slide out -> swap content -> slide back in. Guarded by
    ' green_stop.Visible (steady showing), never mid-exit-slide.
    ' T29.4: same in-flight coalescing as slot 2.
    Private Sub DanceSideToast3(message As String, showImage As Boolean, icon As String, iconColor As Color, group As String)
        _side3Group = group
        Notifier3.autoClose.Stop()
        Notifier3.autoClose.Start()
        ' T30.1: no TopMost setter (can activate) - see ShowSideSlot3.

        Dim paintSub = Sub()
                           With Notifier_Sub3.icon_n
                               .Font = New Font(.Font.FontFamily, 35)
                               .ForeColor = iconColor
                               .Text = icon
                           End With
                           Notifier_Sub3.text_n.Text = message
                           Notifier_Sub3.PictureBox1.Visible = showImage
                       End Sub

        If Not Notifier3.BeginDance() Then
            paintSub()
            Exit Sub
        End If

        ' T30.2: the rider STAYS alive and rides the slide-out with the card
        ' (the engine carries it in the same tick). It used to be Closed at
        ' the dance start - the text blinked off, the card slid out empty,
        ' slid back empty and the text faded in after the stop.
        Notifier3.StartSlide(Notifier3.Notifier_black, Notifier3.Notifier_black.Left, Notifier3.Width + 300, 600)

        Dim delay As New Timer()
        delay.Interval = 200
        AddHandler delay.Tick, Sub()
                                   delay.Stop()
                                   delay.Dispose()
                                   ' T30.2: swap the content at the TURNAROUND - the
                                   ' card is fully offscreen here, so the new text/ico
                                   ' is already riding back in with it (no pop, no fade).
                                   paintSub()
                                   Notifier3.StartSlide(Notifier3.Notifier_black, Notifier3.Width, Notifier3.Width - 300, 300,
                                       Sub()
                                           Notifier_Sub3.Show()
                                           Notifier3.EndDance()
                                           Notifier3.FadeInShadow()
                                       End Sub)
                               End Sub
        delay.Start()
    End Sub

    ' Fresh show on slot 2. Mirrors the main toast's fresh-show path:
    ' Show() runs the Form_Load slide-in dance; content is set up front.
    ' NOTE: no autoClose.Start() here - a fresh unit's Timer still has the
    ' 100ms default Interval until its Form_Load dance sets 6000, so
    ' starting it here would auto-close the toast mid-dance. Form_Load owns
    ' the timer instead.
    Private Sub ShowSideSlot(content As Notifier_Sub2, message As String, showImage As Boolean, icon As String, iconColor As Color, group As String)
        _side2Group = group
        Notifier2.autoClose.Stop()
        ' T30.1: no TopMost setter (can activate) - RaiseUnit()/heartbeat own z-order.
        Notifier2.Show()
        With content.icon_n
            .Font = New Font(.Font.FontFamily, 35)
            .ForeColor = iconColor
            .Text = icon
        End With
        content.text_n.Text = message
        content.PictureBox1.Visible = showImage
    End Sub

    ' Updater UI on slot 2 - mirrors ShowOnMain's replace dance on the side
    ' unit: slide out -> swap content -> slide back in. Guarded by
    ' green_stop.Visible (steady showing), never mid-exit-slide.
    ' T29.4: a dance already in flight COALESCES the content instead of
    ' stacking a second overlapping animation (OWNER 20ms-spam glitch).
    Private Sub DanceSideToast(message As String, showImage As Boolean, icon As String, iconColor As Color, group As String)
        _side2Group = group
        Notifier2.autoClose.Stop()
        Notifier2.autoClose.Start()
        ' T30.1: no TopMost setter (can activate) - see ShowSideSlot.

        Dim paintSub = Sub()
                           With Notifier_Sub2.icon_n
                               .Font = New Font(.Font.FontFamily, 35)
                               .ForeColor = iconColor
                               .Text = icon
                           End With
                           Notifier_Sub2.text_n.Text = message
                           Notifier_Sub2.PictureBox1.Visible = showImage
                       End Sub

        If Not Notifier2.BeginDance() Then
            ' A dance is already in flight on slot 2 - overwrite the live
            ' rider's content; the running dance surfaces the latest
            ' message (latest wins).
            paintSub()
            Exit Sub
        End If

        ' T30.2: the rider STAYS alive and rides the slide-out with the card
        ' (the engine carries it in the same tick). It used to be Closed at
        ' the dance start - the text blinked off, the card slid out empty,
        ' slid back empty and the text faded in after the stop.
        Notifier2.StartSlide(Notifier2.Notifier_black, Notifier2.Notifier_black.Left, Notifier2.Width + 300, 600)

        Dim delay As New Timer()
        delay.Interval = 200
        AddHandler delay.Tick, Sub()
                                   delay.Stop()
                                   delay.Dispose()
                                   ' T30.2: swap the content at the TURNAROUND - the
                                   ' card is fully offscreen here, so the new text/ico
                                   ' is already riding back in with it (no pop, no fade).
                                   paintSub()
                                   Notifier2.StartSlide(Notifier2.Notifier_black, Notifier2.Width, Notifier2.Width - 300, 300,
                                       Sub()
                                           Notifier_Sub2.Show()
                                           Notifier2.EndDance()
                                           Notifier2.FadeInShadow()
                                       End Sub)
                               End Sub
        delay.Start()
    End Sub

    ' ฟังก์ชัน UpdateNotifier (จัดการ UI)
    ' group = the notification group this toast belongs to (recording /
    ' replay / per-key fallback - see NotificationGroup). Routing (T29.3
    ' slots + T29.4 anti-spam + T30 RaiseStack):
    '   1. A group steadily showing on a side slot -> Updater UI dance there.
    '   2. A NEW group + main busy -> first FREE configured side slot; when
    '      EVERY configured slot is busy -> T30 QUEUE (FIFO, capped,
    '      same-group dedup) instead of stealing a slot or dropping.
    '   2b. A group live on a side slot but not steady yet (mid-dance /
    '       mid-entrance / mid-exit) -> coalesce onto that unit, or queue
    '       if the unit is sliding out (the pending Close() would eat it).
    '   3. Main slot -> fresh show at the BOTTOM of the active stack (T30
    '      reflow glides the rest up), same-group dance, or coalesce.
    Public Sub UpdateNotifier(message As String, showImage As Boolean, icon As String, iconColor As Color, Optional group As String = Nothing)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() UpdateNotifier(message, showImage, icon, iconColor, group))
            Return
        End If

        ' tcp.SendLog("notifier_show") ' ถ้าต้องการ

        ' T30: reconcile stack bookkeeping with the live forms BEFORE any
        ' routing decision - after a close, the freed slot must leave the
        ' stack order immediately, never one heartbeat tick later.
        UpdateSlotLiveness()

        Dim slotCount As Integer = ConfiguredSlotCount()

        ' 1) The group is already live on a side slot and steadily showing ->
        '    Updater UI dance in that slot itself.
        If slotCount >= 2 AndAlso Notifier2.Notifier_green_stop.Visible AndAlso
           SideSlotBusy(Notifier2) AndAlso SameToastGroup(_side2Group, group) Then
            DanceSideToast(message, showImage, icon, iconColor, group)
            Exit Sub
        End If
        If slotCount >= 3 AndAlso Notifier3.Notifier_green_stop.Visible AndAlso
           SideSlotBusy(Notifier3) AndAlso SameToastGroup(_side3Group, group) Then
            DanceSideToast3(message, showImage, icon, iconColor, group)
            Exit Sub
        End If

        Dim groupLiveSomewhere As Boolean =
            SameToastGroup(_mainGroup, group) OrElse
            (slotCount >= 2 AndAlso SameToastGroup(_side2Group, group)) OrElse
            (slotCount >= 3 AndAlso SameToastGroup(_side3Group, group))

        ' 2) A NEW group while the main toast is up -> the first FREE
        '    configured side slot gets a fresh show (OWNER example: Record
        '    start/stop living in slot 2, Replay start/stop arrives -> it
        '    takes the free slot). Every configured slot busy -> T30 QUEUE:
        '    the toast waits and surfaces on the first slot that frees -
        '    never dropped, never stealing another group's slot. A group
        '    already live somewhere skips this branch so its own slot's
        '    dance (below) keeps updating where it lives.
        If MainSlotBusy() AndAlso Not groupLiveSomewhere Then
            If slotCount >= 2 AndAlso Not SideSlotBusy(Notifier2) Then
                AssignUnitY(2)
                ShowSideSlot(Notifier_Sub2, message, showImage, icon, iconColor, group)
                RegisterActiveSlot(2)
                Exit Sub
            End If
            If slotCount >= 3 AndAlso Not SideSlotBusy(Notifier3) Then
                AssignUnitY(3)
                ShowSideSlot3(Notifier_Sub3, message, showImage, icon, iconColor, group)
                RegisterActiveSlot(3)
                Exit Sub
            End If
            EnqueueToast(message, showImage, icon, iconColor, group)
            Exit Sub
        End If

        ' 2b) The group lives on a side slot but is NOT steady (mid-dance /
        '     mid-entrance). Coalesce onto that unit's rider - or, if the
        '     unit is sliding out to close, queue instead of painting a
        '     corpse (T29.4 lesson: the pending Me.Close() eats the toast).
        If slotCount >= 2 AndAlso SideSlotBusy(Notifier2) AndAlso SameToastGroup(_side2Group, group) Then
            If Notifier2.InTransition Then
                EnqueueToast(message, showImage, icon, iconColor, group)
            Else
                DanceSideToast(message, showImage, icon, iconColor, group)
            End If
            Exit Sub
        End If
        If slotCount >= 3 AndAlso SideSlotBusy(Notifier3) AndAlso SameToastGroup(_side3Group, group) Then
            If Notifier3.InTransition Then
                EnqueueToast(message, showImage, icon, iconColor, group)
            Else
                DanceSideToast3(message, showImage, icon, iconColor, group)
            End If
            Exit Sub
        End If

        ' 3) The main slot. Fresh show at the BOTTOM of the active stack
        '    (T30 reflow glides the others up), the Updater UI replace dance
        '    when it is showing, T29.4 coalescing while a dance is running.
        '    This also covers 1-slot mode.
        If Notifier.InTransition Then
            ' Main is sliding out to close - anything painted now dies with
            ' it. Queue the toast; the heartbeat shows it on the next free
            ' slot (often this very one, about a second later).
            EnqueueToast(message, showImage, icon, iconColor, group)
            Exit Sub
        End If
        AssignUnitY(1)
        ShowOnMain(message, showImage, icon, iconColor, group)
        RegisterActiveSlot(1)
    End Sub

    ' The main toast, original flow: refresh the close window while showing,
    ' then the classic slide dance if a toast is up, else a plain fresh show.
    ' T29.4 anti-spam: a 20ms burst never stacks a second dance on a unit
    ' that is already dancing or sliding out - content is coalesced onto
    ' the rider and the running animation surfaces it (latest wins).
    Private Sub ShowOnMain(message As String, showImage As Boolean, icon As String, iconColor As Color, group As String)
        _mainGroup = group

        ' Refresh the close window only while a toast is actually showing -
        ' on a fresh show Form_Load owns autoClose (and its Interval), so
        ' starting the timer here would fire AutoClose at the 100ms default.
        If Notifier.Notifier_green_stop.Visible Then
            Notifier.autoClose.Stop()
            Notifier.autoClose.Start()
        End If
        ' T30.1: no TopMost setter (can activate) - see ShowSideSlot.

        Dim paintSub = Sub()
                           With Notifier_Sub.icon_n
                               .Font = New Font(.Font.FontFamily, 35)
                               .ForeColor = iconColor
                               .Text = icon
                           End With
                           Notifier_Sub.text_n.Text = message
                           Notifier_Sub.PictureBox1.Visible = showImage
                       End Sub

        ' Logic เดิมเรื่องการสไลด์
        If Notifier.Notifier_green_stop.Visible Then
            ' แก้: เอา Application.DoEvents() ออก — กัน reentrancy
            If Notifier.BeginDance() Then
                ' T30.2: the rider STAYS alive and rides the slide-out with
                ' the card (the engine carries it in the same tick). It used
                ' to be Closed at the dance start - the text blinked off, the
                ' card slid out empty, slid back empty and the text faded in
                ' after the stop: the whole sequence read as flicker.
                Notifier.StartSlide(Notifier.Notifier_black, Notifier.Notifier_black.Left, Notifier.Width + 300, 600)

                Dim delay As New Timer()
                delay.Interval = 200
                AddHandler delay.Tick, Sub()
                                           delay.Stop()
                                           delay.Dispose()
                                           ' T30.2: swap the content at the TURNAROUND - the
                                           ' card is fully offscreen here, so the new text/ico
                                           ' is already riding back in with it (no pop, no fade).
                                           paintSub()
                                           Notifier.StartSlide(Notifier.Notifier_black, Notifier.Width, Notifier.Width - 300, 300,
                                               Sub()
                                                   Notifier_Sub.Show()
                                                   Notifier.EndDance()
                                                   Notifier.FadeInShadow()
                                               End Sub)
                                       End Sub
                delay.Start()
            Else
                ' A dance is already in flight - overwrite the live rider's
                ' content; the running dance surfaces the latest message.
                paintSub()
            End If
            Exit Sub
        End If

        ' Fresh show - only on a unit that is free or still running its
        ' entrance (its Form_Load dance surfaces the content). NEVER poke
        ' a unit that is sliding out to close: its pending Me.Close()
        ' would eat the toast right after showing it.
        If Notifier.InTransition Then
            paintSub()
            Exit Sub
        End If

        Notifier.Show()
        paintSub()
    End Sub

    ' ==== T30 RaiseStack — stack manager (reflow + queue + topmost) ====
    ' OWNER spec T30 "RaiseStack":
    '   A) When a toast closes, the toasts BELOW it glide up (StartSlideY
    '      per unit: card + content, shadow rides its 16ms sync timer) to
    '      fill the gap - a compact vertical stack, Windows-style.
    '   B) The whole stack is re-asserted HWND_TOPMOST (SWP_NOACTIVATE -
    '      no focus steal) every ~2s while visible, so fullscreen
    '      borderless games / other topmost apps can never bury it.
    '   C) A toast arriving when EVERY configured slot is busy is QUEUED
    '      (FIFO, capped, same-group dedup) instead of being dropped or
    '      stealing another group's slot - the heartbeat surfaces it the
    '      moment a slot frees.
    ' Explosion-proofing (OWNER request - "กัน Users ระเบิด แอป"):
    '   - The heartbeat is idempotent: EVERY tick recomputes the truth
    '     from the live forms (liveness, ranks, free slots). Any race -
    '     burst spam, settings flip, mid-animation close - self-heals
    '     within 100ms instead of stacking up.
    '   - The whole tick body is wrapped in Try/Catch: a heartbeat error
    '     logs and returns, it can never kill the app.
    '   - Animations refuse disposed targets ("never animate a corpse"):
    '     unit engines drop panels/forms closed mid-flight instead of
    '     throwing ObjectDisposedException on the UI thread.
    '   - Queue caps at MaxPendingToasts; overflow drops the OLDEST and
    '     logs - memory stays bounded no matter how hard users spam.
    '   - A fresh show never pokes a unit mid-transition (T29.4 guard
    '     extended): mid-exit toasts are queued, not painted.
    Private Class PendingToast
        Public Message As String
        Public ShowImage As Boolean
        Public Icon As String
        Public IconColor As Color
        Public Group As String
    End Class

    Private ReadOnly _pendingToasts As New Queue(Of PendingToast)()
    Private Const MaxPendingToasts As Integer = 8
    Private Const StackPitchPx As Integer = 100      ' card 90px + 10px gap (matches SlotOffsetY)
    Private Const StackReflowMs As Integer = 250
    Private ReadOnly _slotOrder As New List(Of Integer)()   ' active slots, first = top of the stack
    Private ReadOnly _wasAlive(3) As Boolean
    Private _stackTimer As System.Windows.Forms.Timer
    Private _raiseCounter As Integer

    ' Base Y of the stack - identical rule the unit forms use on a legacy
    ' show (notifier_main shifts everything one notch down).
    Private Function StackBaseY() As Integer
        If My.Computer.FileSystem.FileExists(AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "notifier_main")) Then Return 205
        Return 105
    End Function

    Private Sub StartStackHeartbeat()
        If _stackTimer IsNot Nothing Then Return
        _stackTimer = New System.Windows.Forms.Timer()
        _stackTimer.Interval = 100
        AddHandler _stackTimer.Tick, AddressOf OnStackHeartbeat
        _stackTimer.Start()
        Debug.WriteLine("[Stack] heartbeat started (100ms)")
    End Sub

    Private Sub OnStackHeartbeat(sender As Object, e As EventArgs)
        If Me.IsDisposed OrElse Me.Disposing Then
            If _stackTimer IsNot Nothing Then _stackTimer.Stop()
            Return
        End If
        Try
            UpdateSlotLiveness()       ' deaths -> leave the stack order + clear group
            CompactStack()             ' glide every unit to its rank (no-op when aligned)
            TryDequeueIntoFreeSlot()   ' T30-C: surface queued toasts
            _raiseCounter += 1
            If _raiseCounter >= 20 Then
                _raiseCounter = 0
                RaiseVisibleStack()    ' T30-B: HWND_TOPMOST re-assert (~2s cadence)
            End If
        Catch ex As Exception
            ' NEVER let the heartbeat kill the app - log and try again.
            Debug.WriteLine("[Stack] heartbeat error: " & ex.Message)
        End Try
    End Sub

    Private Function SlotIsAlive(slotIdx As Integer) As Boolean
        Select Case slotIdx
            Case 1 : Return Notifier.Visible OrElse Notifier.Notifier_green_stop.Visible
            Case 2 : Return Notifier2.Visible OrElse Notifier2.Notifier_green_stop.Visible
            Case 3 : Return Notifier3.Visible OrElse Notifier3.Notifier_green_stop.Visible
        End Select
        Return False
    End Function

    ' A fully idle unit: not showing AND not mid-dance/mid-exit. Only then
    ' may a queued toast reuse it for a fresh show (T29.4: never poke a
    ' unit mid-transition - its pending Me.Close() eats the toast).
    Private Function SlotIsIdle(slotIdx As Integer) As Boolean
        If SlotIsAlive(slotIdx) Then Return False
        Select Case slotIdx
            Case 1 : Return Not Notifier.InTransition
            Case 2 : Return Not Notifier2.InTransition
            Case 3 : Return Not Notifier3.InTransition
        End Select
        Return False
    End Function

    Private Sub ClearSlotGroup(slotIdx As Integer)
        Select Case slotIdx
            Case 1 : _mainGroup = ""
            Case 2 : _side2Group = ""
            Case 3 : _side3Group = ""
        End Select
    End Sub

    Private Sub UpdateSlotLiveness()
        For i As Integer = 1 To 3
            Dim alive As Boolean = SlotIsAlive(i)
            If _wasAlive(i) AndAlso Not alive Then
                _slotOrder.Remove(i)
                ClearSlotGroup(i)
                Debug.WriteLine("[Stack] slot " & i & " freed")
            End If
            _wasAlive(i) = alive
        Next
    End Sub

    ' Called after a successful FRESH show. Slots already in the order
    ' keep their rank (a dance/coalesce never re-ranks the stack).
    Private Sub RegisterActiveSlot(slotIdx As Integer)
        If _slotOrder.Contains(slotIdx) Then Return
        _slotOrder.Add(slotIdx)   ' newest toast = bottom of the stack
        _wasAlive(slotIdx) = True
    End Sub

    ' Assign the next show Y for a unit: its rank is the number of slots
    ' currently active (0-based), so a new toast always enters at the
    ' BOTTOM of the stack. Idle slots keep their legacy resting spot.
    Private Sub AssignUnitY(slotIdx As Integer)
        Dim y As Integer = StackBaseY() + _slotOrder.Count * StackPitchPx
        Select Case slotIdx
            Case 1 : Notifier.UnitTargetY = y
            Case 2 : Notifier2.UnitTargetY = y
            Case 3 : Notifier3.UnitTargetY = y
        End Select
        Debug.WriteLine("[Stack] slot " & slotIdx & " assigned Y=" & y & " (rank " & _slotOrder.Count & ")")
    End Sub

    ' T30-A: compact the stack - every active unit glides to
    ' baseY + rank * pitch. Idempotent and self-healing: running it every
    ' heartbeat tick fixes any drift (post-dance, post-exit, base shift,
    ' aborted animation) without dedicated bookkeeping. Units mid-dance or
    ' mid-exit are skipped this tick; the next tick catches them.
    Private Sub CompactStack()
        Dim baseY As Integer = StackBaseY()
        For rank As Integer = 0 To _slotOrder.Count - 1
            Dim slot As Integer = _slotOrder(rank)
            Dim target As Integer = baseY + rank * StackPitchPx
            Select Case slot
                Case 1
                    If Notifier.Visible AndAlso Not Notifier.InTransition AndAlso Notifier.Top <> target Then
                        Notifier.ReflowTo(target, StackReflowMs)
                    End If
                Case 2
                    If Notifier2.Visible AndAlso Not Notifier2.InTransition AndAlso Notifier2.Top <> target Then
                        Notifier2.ReflowTo(target, StackReflowMs)
                    End If
                Case 3
                    If Notifier3.Visible AndAlso Not Notifier3.InTransition AndAlso Notifier3.Top <> target Then
                        Notifier3.ReflowTo(target, StackReflowMs)
                    End If
            End Select
        Next
    End Sub

    ' T30-C: FIFO queue with same-group dedup (a repeat toast refreshes
    ' its queued copy in place) and a hard cap - overflow drops the OLDEST
    ' and logs, so spamming can grow memory or starve the stack forever.
    Private Sub EnqueueToast(message As String, showImage As Boolean, icon As String, iconColor As Color, group As String)
        For Each p As PendingToast In _pendingToasts
            If SameToastGroup(p.Group, group) Then
                p.Message = message
                p.ShowImage = showImage
                p.Icon = icon
                p.IconColor = iconColor
                Debug.WriteLine("[Stack] queue: refreshed pending group " & group)
                Return
            End If
        Next
        If _pendingToasts.Count >= MaxPendingToasts Then
            Dim dropped As PendingToast = _pendingToasts.Dequeue()
            Debug.WriteLine("[Stack] queue FULL (" & MaxPendingToasts & ") - dropped oldest group " & dropped.Group)
        End If
        _pendingToasts.Enqueue(New PendingToast With {
            .Message = message, .ShowImage = showImage,
            .Icon = icon, .IconColor = iconColor, .Group = group})
        Debug.WriteLine("[Stack] queued group " & group & " (depth " & _pendingToasts.Count & ")")
    End Sub

    Private Sub TryDequeueIntoFreeSlot()
        If _pendingToasts.Count = 0 Then Return

        Dim slotCount As Integer = ConfiguredSlotCount()
        Dim target As Integer = 0
        If SlotIsIdle(1) Then
            target = 1
        ElseIf slotCount >= 2 AndAlso SlotIsIdle(2) Then
            target = 2
        ElseIf slotCount >= 3 AndAlso SlotIsIdle(3) Then
            target = 3
        End If
        If target = 0 Then Return

        Dim p As PendingToast = _pendingToasts.Dequeue()
        UpdateSlotLiveness()   ' refresh ranks with the CURRENT liveness
        AssignUnitY(target)
        Debug.WriteLine("[Stack] dequeue group " & p.Group & " -> slot " & target)
        Select Case target
            Case 1
                ShowOnMain(p.Message, p.ShowImage, p.Icon, p.IconColor, p.Group)
                RegisterActiveSlot(1)
            Case 2
                ShowSideSlot(Notifier_Sub2, p.Message, p.ShowImage, p.Icon, p.IconColor, p.Group)
                RegisterActiveSlot(2)
            Case 3
                ShowSideSlot3(Notifier_Sub3, p.Message, p.ShowImage, p.Icon, p.IconColor, p.Group)
                RegisterActiveSlot(3)
        End Select
    End Sub

    ' T30-B: re-assert topmost on every window of every visible unit.
    ' RaiseUnit uses SetWindowPos + SWP_NOACTIVATE - nothing can steal
    ' focus or yank the user out of a game.
    Private Sub RaiseVisibleStack()
        For i As Integer = 1 To 3
            If SlotIsAlive(i) Then
                Select Case i
                    Case 1 : Notifier.RaiseUnit()
                    Case 2 : Notifier2.RaiseUnit()
                    Case 3 : Notifier3.RaiseUnit()
                End Select
            End If
        Next
    End Sub

    ' Test buttons - go through the REAL UpdateNotifier with a stable fake
    ' group, so double-pressing Button1 demos the Updater UI dance exactly
    ' like a real repeat notification does.
    Private Const TestButtonGroup As String = "l10n.testbutton"

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
        ' Real router path — press twice: first press = fresh show,
        ' second press while it is up = Updater UI dance (slide out/in).
        UpdateNotifier("Press Alt + Z to use Shadowplay Experience in-game overlay",
                       True, "", Color.White, TestButtonGroup)
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
            If _stackTimer IsNot Nothing Then
                _stackTimer.Stop()
                _stackTimer.Dispose()
                _stackTimer = Nothing
            End If
        Catch
        End Try
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