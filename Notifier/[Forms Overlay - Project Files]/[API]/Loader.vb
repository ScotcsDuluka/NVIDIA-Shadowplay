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
        ' T28: slot router state lives here; toast units are explicit new
        ' ToastUnit() instances created per show — no Shared events, no
        ' default instances to resurrect.
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

    ' ====================================================================
    ' T27 + T28: 2-slot toast router
    '
    ' Slot model — a "slot" is a SCREEN ROW, not a fixed form:
    '   row 0 = top toast, row 1 = the toast below it.
    ' A toast is ONE self-contained window (ToastUnit — T28): shadow,
    ' accent, icon, text rendered into a single per-pixel-alpha surface.
    ' No rider windows, no default instances, nothing to desync.
    '
    ' Routing rules, in order:
    '   0. HEAL   — a slot stuck non-Free with a disposed unit self-heals
    '               to Free (T27.2 defense, FormRef-based).
    '   1. TOGGLE — same notification key already live (entering/showing)
    '               → refresh that unit's content in place + restart its
    '               6s window. No new toast, no slide.
    '   2. FREE   — a new explicit ToastUnit → fresh show in the lowest
    '               free row.
    '   3. QUEUE  — both busy → the toast waits (max 4, same key merges
    '               into its queued twin) and is served the moment a unit
    '               closes — DEFERRED out of the Close() stack (T27.2).
    '
    ' Reflow (OWNER spec): when the row-0 toast slides out, the row-1
    ' toast glides up and takes row 0 — a slot is a position, so the
    ' next fresh toast enters at the bottom row.
    ' ====================================================================
    Private Enum SlotPhase
        Free
        Entering
        Showing
        Closing
    End Enum

    Private Class SlotState
        Public Id As Integer
        Public Phase As SlotPhase = SlotPhase.Free
        Public Row As Integer = 0
        Public Key As String = ""
        ''' <summary>T27.2: strong ref to the unit this slot last showed.
        ''' Explicit instance — checking liveness on it can never
        ''' auto-recreate a ghost the way a VB default instance would.</summary>
        Public FormRef As ToastUnit
    End Class

    Private ReadOnly S1 As New SlotState With {.Id = 1}
    Private ReadOnly S2 As New SlotState With {.Id = 2}

    Private Class PendingToast
        Public Key As String
        Public Message As String
        Public Png As Boolean
        Public Ico As String
        Public Color As Color
    End Class

    Private ReadOnly _pendingToasts As New List(Of PendingToast)()
    Private Const MaxPendingToasts As Integer = 4
    Private _reflowTimer As Timer

    ' ---- single entry point: one toast, already localized ----
    Public Sub RouteToast(key As String, message As String, showImage As Boolean, icon As String, iconColor As Color)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() RouteToast(key, message, showImage, icon, iconColor))
            Return
        End If

        ' 0) T27.2: self-heal FIRST — a slot wedged at Entering/Showing/Closing
        ' with a disposed form (any historical race) must not eat this toast
        ' into a dead unit or a phantom toggle match.
        HealStaleSlots()

        ' 1) toggle — refresh the live unit already showing this key
        Dim live = FindLiveByKey(key)
        If live IsNot Nothing Then
            UpdateInPlace(live, message, showImage, icon, iconColor)
            Return
        End If

        ' 2) free unit — fresh show in the lowest free row
        Dim free = FindFreeUnit()
        If free IsNot Nothing Then
            ShowFresh(free, key, message, showImage, icon, iconColor)
            Return
        End If

        ' 3) both busy — wait in the queue
        Enqueue(key, message, showImage, icon, iconColor)
    End Sub

    Private Function FindLiveByKey(key As String) As SlotState
        For Each s In {S1, S2}
            If (s.Phase = SlotPhase.Entering OrElse s.Phase = SlotPhase.Showing) AndAlso
               String.Equals(s.Key, key, StringComparison.Ordinal) Then
                Return s
            End If
        Next
        Return Nothing
    End Function

    Private Function FindFreeUnit() As SlotState
        ' prefer the free unit already sitting on the lowest row;
        ' ties (both free) go to unit 1 — the original main toast
        Dim best As SlotState = Nothing
        For Each s In {S1, S2}
            If s.Phase <> SlotPhase.Free Then Continue For
            If best Is Nothing OrElse s.Row < best.Row Then best = s
        Next
        Return best
    End Function

    Private Function LowestFreeRow(exclude As SlotState) As Integer
        ' rows held by any unit that is not Free — a CLOSING unit still owns
        ' its row until it is really gone, so a fresh toast never lands on
        ' a dying one
        Dim used As New HashSet(Of Integer)()
        For Each s In {S1, S2}
            If s Is exclude OrElse s.Phase = SlotPhase.Free Then Continue For
            used.Add(s.Row)
        Next
        If Not used.Contains(0) Then Return 0
        Return 1
    End Function

    Private Sub ShowFresh(s As SlotState, key As String, message As String, showImage As Boolean, icon As String, iconColor As Color)
        ' T27.2: hard reset — if the slot's previous unit is somehow still
        ' alive (wedged phase from an older race), silently dispose it.
        Try
            If s.FormRef IsNot Nothing AndAlso Not s.FormRef.IsDisposed Then
                Debug.WriteLine($"[Router] unit {s.Id}: disposing stale form before fresh show")
                s.FormRef.Dispose()
            End If
        Catch
        End Try

        s.Row = LowestFreeRow(s)
        s.Key = key
        s.Phase = SlotPhase.Entering
        Debug.WriteLine($"[Router] fresh show key={key} unit={s.Id} row={s.Row}")

        ' T28: explicit fresh instance per toast — clean state guaranteed
        ' by construction, not by default-instance resurrection timing.
        Dim u As New ToastUnit(s.Id)
        s.FormRef = u
        AddHandler u.DanceCompleted, AddressOf OnUnitDanceCompleted
        AddHandler u.SlideOutStarted, AddressOf OnUnitSlideOutStarted
        AddHandler u.UnitClosed, AddressOf OnUnitClosed
        u.ShowToast(s.Row, message, showImage, icon, iconColor)
    End Sub

    ''' <summary>T27.2: a slot stuck at Entering/Showing/Closing whose form is
    ''' gone can never serve a toast again — every arrival toggles a dead unit
    ''' or queues forever (OWNER: "toast จบแล้ว ค้างเลย"). Reads FormRef only —
    ''' never the default instance, which would auto-recreate the very ghost
    ''' we are checking for.</summary>
    Private Sub HealStaleSlots()
        For Each s In {S1, S2}
            If s.Phase = SlotPhase.Free Then Continue For
            If s.FormRef Is Nothing OrElse s.FormRef.IsDisposed Then
                Debug.WriteLine($"[Router] heal: unit {s.Id} phase={s.Phase} but form gone → Free")
                s.Phase = SlotPhase.Free
                s.Key = ""
            End If
        Next
    End Sub

    Private Sub UpdateInPlace(s As SlotState, message As String, showImage As Boolean, icon As String, iconColor As Color)
        Debug.WriteLine($"[Router] toggle-update key={s.Key} unit={s.Id} phase={s.Phase}")
        Dim u As ToastUnit = s.FormRef
        If u Is Nothing OrElse u.IsDisposed Then
            HealStaleSlots()
            Return
        End If
        ' the unit restarts its own 6s window only when Showing — during
        ' the entrance the entrance owns the timer (T27 rule preserved)
        u.UpdateContent(message, showImage, icon, iconColor)
    End Sub

    Private Sub Enqueue(key As String, message As String, showImage As Boolean, icon As String, iconColor As Color)
        Dim twin = _pendingToasts.Find(Function(p) p.Key = key)
        If twin IsNot Nothing Then
            twin.Message = message
            twin.Png = showImage
            twin.Ico = icon
            twin.Color = iconColor
            Debug.WriteLine($"[Router] queue: merged into pending twin key={key}")
            Return
        End If
        If _pendingToasts.Count >= MaxPendingToasts Then
            _pendingToasts.RemoveAt(0)   ' oldest loses its seat — newest wins
        End If
        _pendingToasts.Add(New PendingToast With {.Key = key, .Message = message, .Png = showImage, .Ico = icon, .Color = iconColor})
        Debug.WriteLine($"[Router] queue: waiting ({_pendingToasts.Count}/{MaxPendingToasts}) key={key}")
    End Sub

    Private Sub TryDequeueNext()
        If _pendingToasts.Count = 0 Then Return
        Dim nextToast = _pendingToasts(0)
        _pendingToasts.RemoveAt(0)
        Debug.WriteLine($"[Router] dequeue key={nextToast.Key} (remaining {_pendingToasts.Count})")
        RouteToast(nextToast.Key, nextToast.Message, nextToast.Png, nextToast.Ico, nextToast.Color)
    End Sub

    ' ---- unit lifecycle events (instance events, subscribed per unit in ShowFresh) ----
    Private Sub OnUnitDanceCompleted(u As ToastUnit)
        Dim s = StateOf(u)
        If s Is Nothing Then Return
        If s.Phase = SlotPhase.Entering Then s.Phase = SlotPhase.Showing
    End Sub

    Private Sub OnUnitSlideOutStarted(u As ToastUnit)
        Dim s = StateOf(u)
        If s Is Nothing Then Return
        s.Phase = SlotPhase.Closing
        Debug.WriteLine($"[Router] unit {s.Id} closing (row {s.Row})")

        ' Reflow: only the top row's exit pulls the bottom toast up
        If s.Row <> 0 Then Return
        Dim below = If(s Is S1, S2, S1)
        If below.Phase <> SlotPhase.Showing AndAlso below.Phase <> SlotPhase.Entering Then Return
        If below.Row <> 1 Then Return

        If _reflowTimer IsNot Nothing Then
            _reflowTimer.Stop()
            _reflowTimer.Dispose()
            _reflowTimer = Nothing
        End If
        _reflowTimer = New Timer()
        _reflowTimer.Interval = 320   ' let the top toast clear out first
        AddHandler _reflowTimer.Tick,
            Sub()
                _reflowTimer.Stop()
                _reflowTimer.Dispose()
                _reflowTimer = Nothing
                ' re-check at fire time — the picture may have changed
                If below.Phase <> SlotPhase.Showing AndAlso below.Phase <> SlotPhase.Entering Then Return
                If below.Row <> 1 Then Return
                ReflowUp(below)
            End Sub
        _reflowTimer.Start()
    End Sub

    ''' <summary>OWNER spec: the bottom toast glides up into the freed top
    ''' slot. T28: the unit is ONE window — GlideToRow moves everything at
    ''' once; no riders, no SettleRiders, nothing to desync.</summary>
    Private Sub ReflowUp(s As SlotState)
        ' T27.2: never glide a dead unit — if its form is already gone,
        ' heal the slot and bail instead of animating a ghost.
        If s.FormRef Is Nothing OrElse s.FormRef.IsDisposed Then
            Debug.WriteLine($"[Router] reflow: unit {s.Id} form gone — heal + skip")
            s.Phase = SlotPhase.Free
            s.Key = ""
            Return
        End If
        s.Row = 0
        s.FormRef.GlideToRow(0)
    End Sub

    Private Sub OnUnitClosed(u As ToastUnit)
        Dim s = StateOf(u)
        If s Is Nothing Then Return
        s.Phase = SlotPhase.Free
        s.Key = ""
        Debug.WriteLine($"[Router] unit {s.Id} closed → free")

        ' T27.2: DEFER the dequeue. UnitClosed fires inside Form.Close()'s
        ' call stack, BEFORE the dying form is disposed. Dequeuing here made
        ' the old default-instance router grab the not-yet-disposed corpse
        ' and the slot froze at Entering forever (OWNER: "toast จบแล้ว
        ' ค้างเลย"). BeginInvoke lands on a clean stack after Close() returns.
        Try
            Me.BeginInvoke(Sub() TryDequeueNext())
        Catch
            ' Loader tearing down — nothing left to serve
        End Try
    End Sub

    Private Function StateOf(u As ToastUnit) As SlotState
        If u.UnitId = 1 Then Return S1
        If u.UnitId = 2 Then Return S2
        Return Nothing
    End Function

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

    ' จัดการสถานะ Notifier — T28: liveness อ่านจาก FormRef (explicit unit)
    Public Sub ManageNotifierState()
        Dim dataDir = AppLayout.P("Data", "NVIDIA_Shadowplay_Data")
        Dim notifierFile = Path.Combine(dataDir, "notifier")
        Dim mainOffFile = Path.Combine(dataDir, "notifiermainoff")
        Try
            Dim anyVisible As Boolean = False
            For Each s In {S1, S2}
                Dim u = s.FormRef
                If u IsNot Nothing AndAlso Not u.IsDisposed AndAlso u.Visible Then anyVisible = True
            Next
            If anyVisible Then
                If Not File.Exists(notifierFile) Then File.Create(notifierFile).Dispose()
            Else
                If File.Exists(notifierFile) Then File.Delete(notifierFile)
            End If
        Catch
        End Try
        Try
            If File.Exists(mainOffFile) Then
                File.Delete(mainOffFile)
                For Each s In {S1, S2}
                    Dim u = s.FormRef
                    If u IsNot Nothing AndAlso Not u.IsDisposed Then u.StartMainOffRecovery()
                Next
            End If
        Catch
        End Try
    End Sub

    ' ปุ่มทดสอบ — every button goes through the REAL router, so toggle /
    ' slot / queue / reflow behavior is exactly what a TCP toast gets.
    ' Button1 twice in a row = toggle-update demo (same key refreshed in place).
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        RouteToast("l10n.debug.a", "Press Alt + Z to use Shadowplay Experience in-game overlay", True, "", greenColor)
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        RouteToast("l10n.debug.b", "Second toast — fills the free slot while slot 1 is busy", False, "", greenColor)
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ' T28: legacy "main off" recovery trigger on the live slot-1 unit
        If S1.FormRef IsNot Nothing AndAlso Not S1.FormRef.IsDisposed Then
            S1.FormRef.StartMainOffRecovery()
        End If
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