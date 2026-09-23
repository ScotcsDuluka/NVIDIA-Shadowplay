' Program.vb — NVIDIA Share.exe #1 (Overlay Controller): the 4-instance
' launcher/monitor (GFE process map, see docs/GFE-PROCESS-MAP.md).
'
' Product tree (all four Share slots carry the SAME exe name, like the
' real host — they are distinguished by folder + arguments):
'
'   <root>\Coordinator\NVIDIA Share.exe  <- #1 THIS process (controller)
'   <root>\WinForm\NVIDIA Share.exe      <- #2  WinForm overlay system
'   <root>\WebView\NVIDIA Share.exe      <- #3  WebView desktop overlay
'                                          (Overlay.Engine build, --desktop)
'   <root>\Hook\NVIDIA Share.exe         <- #4  WebView hook slot
'                                          (placeholder, show port :59004)
'   <root>\Notifier\NVIDIA Notifier.exe  <- support child (tray balloons,
'                                          polls the backend; not a Share
'                                          instance — supervised anyway,
'                                          mirroring the real host's
'                                          "Share spawns Notifier" graph)
'
' Contract:
'   - spawn #2/#3/#4 (+ Notifier) ONCE, each with --parent-pid
'     (children self-exit when the controller dies — no orphans)
'   - monitor: any child that dies is respawned with exponential backoff
'     (3s -> 60s, reset after a stable 30s run — EngineProcessSupervisor
'     constants)
'   - health check the backend (:59001/Backend/v.1.0/health) AND the hook
'     show port (:59004/hook/status) every cycle; STATUS CHANGES are
'     logged (informational — each slot owns its own lifecycle)
'   - adopt already-running children instead of double-spawning (matched
'     by exe path, NOT by name — every Share instance is named the same)
'
' Reuse-if-running + backoff semantics mirror
' Overlay\[Forms Overlay - Project Files]\[API]\[Services]\EngineProcessSupervisor.vb.

Option Strict On
Option Explicit On
Option Infer On

Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Threading

Public NotInheritable Class Program

    ' ── tuning (EngineProcessSupervisor constants) ──────────────────
    Private Const MonitorIntervalMs As Integer = 5000
    Private Const RespawnBaseDelayMs As Integer = 3000
    Private Const RespawnMaxDelayMs As Integer = 60000
    Private Const StableRunMs As Integer = 30000
    Private Const ParentPollMs As Integer = 2000
    Private Const HealthTimeoutMs As Integer = 2500

    ' ── per-child supervision state ─────────────────────────────────
    Private NotInheritable Class Child
        Public ReadOnly Role As String
        Public ReadOnly ExePath As String
        Public ReadOnly Arguments As String
        Public Current As Process
        Public LastSpawnAt As DateTime = DateTime.MinValue
        Public RespawnDelayMs As Integer = RespawnBaseDelayMs
        Public LastRefusedAt As DateTime = DateTime.MinValue

        Public Sub New(role As String, exePath As String, arguments As String)
            Me.Role = role
            Me.ExePath = exePath
            Me.Arguments = arguments
        End Sub
    End Class

    Private Shared ReadOnly _sync As New Object()
    Private Shared ReadOnly _logLock As New Object()
    Private Shared _logPath As String = String.Empty
    Private Shared _shuttingDown As Boolean = False
    Private Shared _parentPid As Integer = 0
    Private Shared _healthUrl As String = "http://127.0.0.1:59001/Backend/v.1.0/health"
    Private Shared _hookUrl As String = "http://127.0.0.1:59004/hook/status"

    Private Shared _winform As Child
    Private Shared _webview As Child
    Private Shared _hook As Child
    Private Shared _notifier As Child

    Private Sub New()
    End Sub

    Public Shared Sub Main(args As String())
        Dim winformPath As String = ReadArg(args, "--winform-path")
        Dim webviewPath As String = ReadArg(args, "--webview-path")
        Dim desktopPath As String = ReadArg(args, "--desktop-path") ' legacy alias -> webview
        Dim hookPath As String = ReadArg(args, "--hook-path")
        Dim notifierPath As String = ReadArg(args, "--notifier-path")
        Dim parentArg As String = ReadArg(args, "--parent-pid")
        Dim backendArg As String = ReadArg(args, "--backend-url")
        If backendArg IsNot Nothing Then _healthUrl = backendArg

        If parentArg IsNot Nothing Then
            Integer.TryParse(parentArg, _parentPid)
        End If

        InitLog()

        ' ── resolve children (env > arg > product-tree default > legacy) ──
        Dim root As String = AppContext.BaseDirectory
        Dim myPid As String = Process.GetCurrentProcess().Id.ToString()

        Dim winform As String = If(Environment.GetEnvironmentVariable("SHARE_WINFORM_PATH"), winformPath)
        If String.IsNullOrWhiteSpace(winform) Then
            winform = Path.Combine(root, "WinForm", "NVIDIA Share.exe")
            If Not File.Exists(winform) Then winform = Path.Combine(root, "WinForm", "NVIDIA ShadowPlay.exe") ' pre-rename build
            If Not File.Exists(winform) Then winform = Path.Combine(root, "WinForm", "Share.exe")
        End If

        Dim webview As String = If(Environment.GetEnvironmentVariable("SHARE_WEBVIEW_PATH"), webviewPath)
        If String.IsNullOrWhiteSpace(webview) Then webview = desktopPath ' legacy alias
        If String.IsNullOrWhiteSpace(webview) Then
            webview = If(Environment.GetEnvironmentVariable("SHARE_DESKTOP_PATH"), Nothing)
        End If
        If String.IsNullOrWhiteSpace(webview) Then
            webview = Path.Combine(root, "WebView", "NVIDIA Share.exe")
            If Not File.Exists(webview) Then webview = Path.Combine(root, "WebView", "Share.exe")
            If Not File.Exists(webview) Then webview = Path.Combine(root, "Desktop", "NVIDIA Share.exe") ' pre-4.1 install
            If Not File.Exists(webview) Then webview = Path.Combine(root, "Desktop", "Share.exe")
        End If

        Dim hook As String = If(Environment.GetEnvironmentVariable("SHARE_HOOK_PATH"), hookPath)
        If String.IsNullOrWhiteSpace(hook) Then
            hook = Path.Combine(root, "Hook", "NVIDIA Share.exe")
            If Not File.Exists(hook) Then hook = Path.Combine(root, "Hook", "Share.exe")
        End If

        Dim notifier As String = If(Environment.GetEnvironmentVariable("SHARE_NOTIFIER_PATH"), notifierPath)
        If String.IsNullOrWhiteSpace(notifier) Then
            notifier = Path.Combine(root, "Notifier", "NVIDIA Notifier.exe")
            If Not File.Exists(notifier) Then notifier = Path.Combine(root, "Notifier", "Notifier.exe")
        End If

        _winform = New Child("WinForm#2", winform, "--parent-pid " & myPid)
        _webview = New Child("WebView#3", webview, "--desktop --parent-pid " & myPid)
        _hook = New Child("Hook#4", hook, "--parent-pid " & myPid)
        _notifier = New Child("Notifier", notifier, "--parent-pid " & myPid)

        Log($"[Coordinator] NVIDIA Share.exe [instance 1/4 — controller] starting (pid {myPid})")
        Log($"[Coordinator] root={root}")
        Log($"[Coordinator] #2 WinForm={winform} (exists={File.Exists(winform)})")
        Log($"[Coordinator] #3 WebView={webview} (exists={File.Exists(webview)})")
        Log($"[Coordinator] #4 Hook={hook} (exists={File.Exists(hook)})")
        Log($"[Coordinator] Notifier={notifier} (exists={File.Exists(notifier)})")
        Log($"[Coordinator] backend health={_healthUrl} | hook show port={_hookUrl}")

        ' initial spawns (best effort — monitor loop retries with backoff)
        SpawnChild(_winform, "startup")
        SpawnChild(_webview, "startup")
        SpawnChild(_hook, "startup")
        SpawnChild(_notifier, "startup")

        If _parentPid > 0 Then
            Dim monitor As New Thread(AddressOf MonitorLoop) With {.Name = "ShareCoordinator", .IsBackground = True}
            monitor.Start()
            WatchParent()
        Else
            MonitorLoop()
        End If
    End Sub

    ' ── monitor: respawn + health ───────────────────────────────────
    Private Shared Sub MonitorLoop()
        Dim lastHealth As String = "?"
        Dim lastHook As String = "?"
        While Not _shuttingDown
            Try
                Supervise(_winform)
                Supervise(_webview)
                Supervise(_hook)
                Supervise(_notifier)

                Dim h As String = CheckHealth(_healthUrl)
                If h <> lastHealth Then
                    Log($"[Coordinator] backend health: {h}")
                    lastHealth = h
                End If
                Dim k As String = CheckHealth(_hookUrl)
                If k <> lastHook Then
                    Log($"[Coordinator] hook show port: {k}")
                    lastHook = k
                End If
            Catch ex As Exception
                Log($"[Coordinator] monitor cycle error: {ex.Message}")
            End Try
            Thread.Sleep(MonitorIntervalMs)
        End While
    End Sub

    Private Shared Sub Supervise(child As Child)
        SyncLock _sync
            If _shuttingDown Then Return

            Dim alive As Boolean = False
            If child.Current IsNot Nothing Then
                Try
                    child.Current.Refresh()
                    alive = Not child.Current.HasExited
                Catch ex As Exception
                    alive = False
                End Try
            End If

            If alive Then
                ' stable run resets the backoff window
                If (DateTime.UtcNow - child.LastSpawnAt).TotalMilliseconds > StableRunMs Then
                    child.RespawnDelayMs = RespawnBaseDelayMs
                End If
                Return
            End If

            ' dead (or never spawned): respect the backoff since the last attempt
            If child.Current IsNot Nothing AndAlso child.Current.HasExited Then
                Log($"[Coordinator] {child.Role} exited (code {child.Current.ExitCode}) — next respawn in {child.RespawnDelayMs}ms")
                Try
                    child.Current.Dispose()
                Catch
                End Try
                child.Current = Nothing
            End If
            If (DateTime.UtcNow - child.LastSpawnAt).TotalMilliseconds < child.RespawnDelayMs Then
                Return
            End If
            If (DateTime.UtcNow - child.LastRefusedAt).TotalMilliseconds < child.RespawnDelayMs Then
                Return
            End If

            SpawnChild(child, "respawn")
            child.RespawnDelayMs = Math.Min(child.RespawnDelayMs * 2, RespawnMaxDelayMs)
        End SyncLock
    End Sub

    Private Shared Sub SpawnChild(child As Child, reason As String)
        Try
            If Not File.Exists(child.ExePath) Then
                Log($"[Coordinator] {child.Role} exe missing ({child.ExePath}) — cannot spawn ({reason})")
                child.LastRefusedAt = DateTime.UtcNow
                Return
            End If

            Dim adopted As Process = FindRunningByPath(child.ExePath)
            If adopted IsNot Nothing Then
                child.Current = adopted
                child.LastSpawnAt = DateTime.UtcNow
                Log($"[Coordinator] {child.Role} already running (pid {adopted.Id}) — adopted ({reason})")
                Return
            End If

            Dim psi As New ProcessStartInfo With {
                .FileName = child.ExePath,
                .Arguments = child.Arguments,
                .UseShellExecute = True,
                .WorkingDirectory = Path.GetDirectoryName(child.ExePath)
            }
            Dim p As Process = Process.Start(psi)
            If p IsNot Nothing Then
                child.Current = p
                child.LastSpawnAt = DateTime.UtcNow
                Log($"[Coordinator] {child.Role} spawned (pid {p.Id}) ({reason})")
            End If
        Catch ex As Exception
            Log($"[Coordinator] {child.Role} spawn failed ({reason}): {ex.Message}")
            child.LastRefusedAt = DateTime.UtcNow
        End Try
    End Sub

    ' Every Share instance is named "NVIDIA Share" — match by exe path.
    Private Shared Function FindRunningByPath(exePath As String) As Process
        Dim wanted As String = Path.GetFullPath(exePath).TrimEnd("\"c)
        Dim candidates As String() = {"NVIDIA Share", "Share"}
        For Each name As String In candidates
            Dim procs As Process() = Process.GetProcessesByName(name)
            For Each p As Process In procs
                Try
                    Dim candPath As String = p.MainModule.FileName
                    If String.Equals(Path.GetFullPath(candPath).TrimEnd("\"c), wanted, StringComparison.OrdinalIgnoreCase) Then
                        For Each other As Process In procs
                            If other.Id <> p.Id Then other.Dispose()
                        Next
                        Return p
                    End If
                Catch
                    ' elevated/system processes may refuse MainModule — ignore
                End Try
                p.Dispose()
            Next
        Next
        Return Nothing
    End Function

    ' ── backend / hook health (informational) ───────────────────────
    Private Shared Function CheckHealth(url As String) As String
        Try
            Dim req As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            req.Method = "GET"
            req.Timeout = HealthTimeoutMs
            req.ReadWriteTimeout = HealthTimeoutMs
            Using resp As WebResponse = req.GetResponse()
                Dim code As Integer = CInt(CType(resp, HttpWebResponse).StatusCode)
                Return If(code = 200, "OK", "HTTP " & code.ToString())
            End Using
        Catch ex As WebException
            If ex.Response IsNot Nothing Then
                Return "HTTP " & CInt(CType(ex.Response, HttpWebResponse).StatusCode).ToString()
            End If
            Return "DOWN"
        Catch ex As Exception
            Return "ERROR (" & ex.Message & ")"
        End Try
    End Function

    ' ── parent watch: the controller may itself be spawned by a launcher ──
    Private Shared Sub WatchParent()
        Try
            Dim parent As Process = Process.GetProcessById(_parentPid)
            Log($"[Coordinator] watching parent pid {_parentPid} (poll {ParentPollMs}ms)")
            While Not _shuttingDown
                parent.Refresh()
                If parent.HasExited Then
                    Log("[Coordinator] parent gone — controller exits (children self-exit via their own --parent-pid watch)")
                    _shuttingDown = True
                    Environment.Exit(0)
                End If
                Thread.Sleep(ParentPollMs)
            End While
        Catch ex As Exception
            ' parent already gone / access denied — run standalone forever
            Log($"[Coordinator] parent watch unavailable ({ex.Message}) — running standalone")
            Thread.Sleep(Timeout.Infinite)
        End Try
    End Sub

    ' ── args + log helpers ──────────────────────────────────────────
    Private Shared Function ReadArg(args As String(), name As String) As String
        If args Is Nothing Then Return Nothing
        For i As Integer = 0 To args.Length - 2
            If String.Equals(args(i), name, StringComparison.OrdinalIgnoreCase) Then
                Return args(i + 1)
            End If
        Next
        Return Nothing
    End Function

    Private Shared Sub InitLog()
        Try
            Dim dir As String = Path.Combine(AppContext.BaseDirectory, "logs")
            Directory.CreateDirectory(dir)
            _logPath = Path.Combine(dir, "coordinator.log")
        Catch
            _logPath = Path.Combine(Path.GetTempPath(), "NVIDIA-Share-Coordinator.log")
        End Try
    End Sub

    Private Shared Sub Log(message As String)
        Debug.WriteLine(message)
        Try
            SyncLock _logLock
                File.AppendAllText(_logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}")
            End SyncLock
        Catch
        End Try
    End Sub

End Class
