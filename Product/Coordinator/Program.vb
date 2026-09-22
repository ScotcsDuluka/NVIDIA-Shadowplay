' Program.vb — Share.exe #1 (Coordinator): the 3-process launcher/monitor.
'
' Product tree (GFE layout, see Product/README.md):
'
'   <root>\Coordinator\Share.exe      <- THIS process (HKCU Run autostart entry)
'   <root>\Desktop\NVIDIA Share.exe   <- #2  the Overlay.Engine osc host (existing build)
'   <root>\Hook\Share.exe             <- #3  hook host placeholder
'
' Contract:
'   - spawn #2 and #3 ONCE, each with --parent-pid <coordinator pid>
'     (both children self-exit when the coordinator dies — no orphans)
'   - monitor: any child that dies is respawned with exponential backoff
'     (3s -> 60s, reset after a stable 30s run — same constants as the
'     proven EngineProcessSupervisor)
'   - health check the backend (:59001/Backend/v.1.0/health) every cycle;
'     STATUS CHANGES are logged (informational — the backend owns its own
'     lifecycle and its own autostart)
'   - adopt already-running children instead of double-spawning (matched
'     by exe path, NOT by name — all three processes are named Share)
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

    Private Shared _desktop As Child
    Private Shared _hook As Child

    Private Sub New()
    End Sub

    Public Shared Sub Main(args As String())
        Dim desktopPath As String = ReadArg(args, "--desktop-path")
        Dim hookPath As String = ReadArg(args, "--hook-path")
        Dim parentArg As String = ReadArg(args, "--parent-pid")
        Dim backendArg As String = ReadArg(args, "--backend-url")
        If backendArg IsNot Nothing Then _healthUrl = backendArg

        If parentArg IsNot Nothing Then
            Integer.TryParse(parentArg, _parentPid)
        End If

        InitLog()

        ' ── resolve children (env > arg > product-tree default) ─────
        Dim root As String = AppContext.BaseDirectory
        Dim desktop As String = If(Environment.GetEnvironmentVariable("SHARE_DESKTOP_PATH"), desktopPath)
        If String.IsNullOrWhiteSpace(desktop) Then
            desktop = Path.Combine(root, "Desktop", "NVIDIA Share.exe")
            If Not File.Exists(desktop) Then desktop = Path.Combine(root, "Desktop", "Share.exe")
        End If
        Dim hook As String = If(Environment.GetEnvironmentVariable("SHARE_HOOK_PATH"), hookPath)
        If String.IsNullOrWhiteSpace(hook) Then
            hook = Path.Combine(root, "Hook", "Share.exe")
            If Not File.Exists(hook) Then hook = Path.Combine(root, "Hook", "NVIDIA Share.exe")
        End If

        Dim myPid As String = Process.GetCurrentProcess().Id.ToString()
        _desktop = New Child("Desktop", desktop, "--parent-pid " & myPid)
        _hook = New Child("Hook", hook, "--parent-pid " & myPid)

        Log($"[Coordinator] Share coordinator starting (pid {myPid})")
        Log($"[Coordinator] root={root}")
        Log($"[Coordinator] Desktop={desktop} (exists={File.Exists(desktop)})")
        Log($"[Coordinator] Hook={hook} (exists={File.Exists(hook)})")
        Log($"[Coordinator] backend health={_healthUrl}")

        ' initial spawns (best effort — monitor loop retries with backoff)
        SpawnChild(_desktop, "startup")
        SpawnChild(_hook, "startup")

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
        While Not _shuttingDown
            Try
                Supervise(_desktop)
                Supervise(_hook)

                Dim h As String = CheckHealth()
                If h <> lastHealth Then
                    Log($"[Coordinator] backend health: {h}")
                    lastHealth = h
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

    ' All three product processes are named Share — match by exe path.
    Private Shared Function FindRunningByPath(exePath As String) As Process
        Dim wanted As String = Path.GetFullPath(exePath).TrimEnd("\"c)
        Dim candidates As String() = {"Share", "NVIDIA Share"}
        For Each name As String In candidates
            Dim procs As Process() = Process.GetProcessesByName(name)
            For Each p As Process In procs
                Try
                    Dim path As String = p.MainModule.FileName
                    If String.Equals(Path.GetFullPath(path).TrimEnd("\"c), wanted, StringComparison.OrdinalIgnoreCase) Then
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

    ' ── backend health (informational) ──────────────────────────────
    Private Shared Function CheckHealth() As String
        Try
            Dim req As HttpWebRequest = CType(WebRequest.Create(_healthUrl), HttpWebRequest)
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

    ' ── parent watch: coordinator may itself be spawned by a launcher ──
    Private Shared Sub WatchParent()
        Try
            Dim parent As Process = Process.GetProcessById(_parentPid)
            Log($"[Coordinator] watching parent pid {_parentPid} (poll {ParentPollMs}ms)")
            While Not _shuttingDown
                parent.Refresh()
                If parent.HasExited Then
                    Log("[Coordinator] parent gone — coordinator exits (children self-exit via their own --parent-pid watch)")
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
