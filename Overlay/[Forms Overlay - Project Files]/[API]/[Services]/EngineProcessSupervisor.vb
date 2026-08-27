Option Strict On
Option Explicit On
Option Infer On

' EngineProcessSupervisor.vb — Auto-spawn lifecycle for NVIDIA Capture.exe
'
' GPT→GLM/5 standing decision (2026-08-23): Auto-spawn → Mic → Instant Replay.
'
' Contract:
'   Capture.exe ไม่อยู่  → spawn → (engine connects hub by itself) → engine_ready
'   Capture.exe อยู่แล้ว → reuse (no duplicate spawn)
'   Capture.exe ตาย      → monitor detects → respawn (rate-limited)
'
' Design notes (evidence-based):
'   - Single-owner guarantee:
'       · Engine side already enforces SingleInstance=true (My.Application,
'         WindowsFormsApplicationBase) — a second spawn focuses the existing
'         instance instead of creating one. That is the hard safety net.
'       · Supervisor side: SyncLock + process-existence check before EVERY
'         spawn + one monitor thread. No overlapping spawns.
'   - IPC-ready signal: the Engine broadcasts "engine_ready" through the Hub
'     when its TCP client connects; the Overlay's existing engine_ready
'     handler then re-sends PREWARM_FFMPEG. No new ready protocol needed —
'     spawning is sufficient, wiring is automatic.
'   - Do NOT kill the engine when the Overlay closes: the engine is a
'     standalone production host (it may still be muxing); its own
'     JobObjectGuard cleans up ffmpeg children if it dies.
'   - Deployment fact: ProjectReference copies "NVIDIA Capture.exe" into the
'     Overlay's output folder — resolve relative to AppDomain.BaseDirectory,
'     with repo-build fallbacks for dev layouts.
'   - Respawn backoff: 3 s → 6 s → 12 s (cap 60 s). Reset on a stable run
'     (engine alive ≥ 30 s). Mirrors TcpClientHelper backoff philosophy.

Imports System.Diagnostics
Imports System.IO
Imports System.Threading

''' <summary>
''' Ensures the NVIDIA Capture production host (engine) is running.
''' Call EnsureEngineRunning() once at Overlay startup; the supervisor
''' keeps a background monitor afterwards. Thread-safe.
''' </summary>
Public NotInheritable Class EngineProcessSupervisor

    Private Const ProcessName As String = "NVIDIA Capture"     ' without .exe
    Private Const ExeFileName As String = "NVIDIA Capture.exe"

    ' Respawn policy
    Private Const RespawnBaseDelayMs As Integer = 3000
    Private Const RespawnMaxDelayMs As Integer = 60000
    Private Const StableRunMs As Integer = 30000                ' alive this long → reset backoff

    Private Shared ReadOnly _sync As New Object()
    Private Shared _started As Boolean = False
    Private Shared _shuttingDown As Boolean = False
    Private Shared _monitorThread As Thread
    Private Shared _lastSpawnedPid As Integer = 0
    Private Shared _lastSpawnAt As DateTime = DateTime.MinValue
    Private Shared _respawnDelayMs As Integer = RespawnBaseDelayMs

    ' ★ Race invariant (GPT challenge): "At most 1 spawn attempt in flight".
    ' Checked+set atomically under _sync; cleared in Finally after Process.Start
    ' returns. Concurrent callers skip — the monitor re-checks next cycle.
    Private Shared _spawnInFlight As Boolean = False

    ' ★ Runtime evidence log (first Windows T0-T3 run showed Debug.WriteLine
    ' is invisible outside a debugger). Append-only, thread-safe, best-effort
    ' — logging failures must never break supervision.
    Private Shared ReadOnly _logLock As New Object()
    Private Shared ReadOnly LogFilePath As String =
        Path.Combine(Path.GetTempPath(), "NVIDIA-Shadowplay-Supervisor.log")

    Private Shared Sub Log(message As String)
        Debug.WriteLine(message)
        Try
            SyncLock _logLock
                File.AppendAllText(LogFilePath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}")
            End SyncLock
        Catch
        End Try
    End Sub

    Private Sub New()
    End Sub

    ' ─── Public API ──────────────────────────────────────────────────

    ''' <summary>
    ''' Called from Overlay Base_Load (after TcpClientHelper creation).
    ''' Idempotent — safe to call multiple times. Starts the monitor thread
    ''' on first call. Never throws.
    ''' </summary>
    Public Shared Sub EnsureEngineRunning()
        SyncLock _sync
            If _started Then Return
            _started = True
            _shuttingDown = False
        End SyncLock

        Try
            SpawnIfNotRunning(reason:="startup")
        Catch ex As Exception
            Log($"[EngineSupervisor] startup spawn failed: {ex.Message}")
        End Try

        _monitorThread = New Thread(AddressOf MonitorLoop) With {
            .Name = "EngineProcessSupervisor",
            .IsBackground = True
        }
        _monitorThread.Start()
    End Sub

    ''' <summary>
    ''' Stop supervising (respawning). Called from Overlay FormClosing.
    ''' Does NOT kill the engine — see class docs.
    ''' </summary>
    Public Shared Sub Shutdown()
        SyncLock _sync
            _shuttingDown = True
        End SyncLock
    End Sub

    ''' <summary>Test hook — current engine process, or Nothing.</summary>
    Public Shared Function FindEngineProcess() As Process
        Dim procs As Process() = Process.GetProcessesByName(ProcessName)
        If procs Is Nothing OrElse procs.Length = 0 Then Return Nothing
        Dim first As Process = procs(0)
        For i As Integer = 1 To procs.Length - 1
            Try : procs(i).Dispose() : Catch : End Try
        Next
        Return first
    End Function

    ' ─── Core ────────────────────────────────────────────────────────

    Private Shared Sub SpawnIfNotRunning(reason As String)
        SyncLock _sync
            If _shuttingDown Then Return

            ' ★ Race invariant: only ONE spawn attempt may be in flight.
            ' (startup path and monitor respawn path both land here; an
            ' external user-start during our pre-spawn window is covered by
            ' the process check below plus the engine's own SingleInstance.)
            If _spawnInFlight Then
                Log($"[EngineSupervisor] spawn already in flight — skip ({reason})")
                Return
            End If

            Using existing As Process = FindEngineProcess()
                If existing IsNot Nothing Then
                    _lastSpawnedPid = existing.Id
                    Log($"[EngineSupervisor] engine already running (pid {_lastSpawnedPid}) — reuse ({reason})")
                    Return
                End If
            End Using

            _spawnInFlight = True
        End SyncLock

        Dim exePath As String = Nothing
        Try
            exePath = ResolveEngineExePath()
            If exePath Is Nothing Then
                Log($"[EngineSupervisor] {ExeFileName} not found — cannot spawn ({reason})")
                Return
            End If

            Dim psi As New ProcessStartInfo With {
                .FileName = exePath,
                .UseShellExecute = True,          ' normal launch, like a user opening it
                .WorkingDirectory = Path.GetDirectoryName(exePath)
            }
            Using p As Process = Process.Start(psi)
                If p IsNot Nothing Then
                    SyncLock _sync
                        _lastSpawnedPid = p.Id
                        _lastSpawnAt = DateTime.Now
                    End SyncLock
                    Log($"[EngineSupervisor] spawned engine pid {p.Id} from {exePath} ({reason})")
                End If
            End Using

        Catch ex As Exception
            Log($"[EngineSupervisor] spawn failed ({reason}): {ex.Message}")
        Finally
            SyncLock _sync
                _spawnInFlight = False
            End SyncLock
        End Try
    End Sub

    ''' <summary>
    ''' Resolution order:
    '''   1. {Overlay exe dir}\NVIDIA Capture.exe     (deployment — ProjectReference copy)
    '''   2. {dir}\..\..\Engine\bin\...\NVIDIA Capture.exe (dev fallback, any config)
    ''' </summary>
    Private Shared Function ResolveEngineExePath() As String
        Try
            Dim baseDir As String = AppLayout.Dir

            ' 0. ROOT-FIXED LAYOUT: the engine host lives in Application\
            Dim layoutCandidate As String = Path.Combine(baseDir, "Application", ExeFileName)
            If File.Exists(layoutCandidate) Then Return layoutCandidate

            ' 1. Legacy deployment: engine exe sits next to the Overlay exe
            Dim candidate As String = Path.Combine(baseDir, ExeFileName)
            If File.Exists(candidate) Then Return candidate

            ' 2. Dev: walk up to repo root, then find Engine\bin\**\NVIDIA Capture.exe
            Dim dir As New DirectoryInfo(baseDir)
            For i As Integer = 1 To 8
                If dir Is Nothing Then Exit For
                Dim engineBin As String = Path.Combine(dir.FullName, "Engine", "bin")
                If Directory.Exists(engineBin) Then
                    Dim hits As String() = Directory.GetFiles(
                        engineBin, ExeFileName, SearchOption.AllDirectories)
                    If hits IsNot Nothing AndAlso hits.Length > 0 Then Return hits(0)
                End If
                dir = dir.Parent
            Next
        Catch ex As Exception
            Log($"[EngineSupervisor] resolve error: {ex.Message}")
        End Try
        Return Nothing
    End Function

    ' ─── Monitor ─────────────────────────────────────────────────────

    Private Shared Sub MonitorLoop()
        ' Track how long the engine stayed alive to reset respawn backoff.
        Dim lastSeenAlive As DateTime = DateTime.MinValue

        While Not Volatile.Read(_shuttingDown)
            Thread.Sleep(2000)
            Dim externalStartDetected As Boolean = False

            Try
                SyncLock _sync
                    If _shuttingDown Then Exit While
                End SyncLock

                Dim aliveNow As Boolean = False
                Using p As Process = FindEngineProcess()
                    If p IsNot Nothing Then
                        aliveNow = True
                        Dim pid As Integer = p.Id
                        If pid = _lastSpawnedPid AndAlso
                           (DateTime.Now - _lastSpawnAt).TotalMilliseconds >= StableRunMs AndAlso
                           _respawnDelayMs > RespawnBaseDelayMs Then
                            _respawnDelayMs = RespawnBaseDelayMs
                            Log("[EngineSupervisor] engine stable — respawn backoff reset")
                        End If
                        lastSeenAlive = DateTime.Now
                    End If
                End Using

                ' Duplicate watch (race-window evidence): if an external start
                ' slipped past the pre-spawn check, we may briefly see >1 engine.
                ' The engine's SingleInstance=true collapses it — we just log it.
                If aliveNow Then
                    Dim procs As Process() = Process.GetProcessesByName(ProcessName)
                    If procs IsNot Nothing AndAlso procs.Length > 1 Then
                        Log($"[EngineSupervisor] WARNING — {procs.Length} engine processes observed " &
                                        "(external start raced us; engine SingleInstance will collapse it)")
                    End If
                    For Each ap As Process In procs
                        Try : ap.Dispose() : Catch : End Try
                    Next
                    Continue While
                End If

                ' Engine is gone. Wait out the backoff, then respawn.
                Dim waitMs As Integer
                SyncLock _sync
                    waitMs = _respawnDelayMs
                End SyncLock

                Log("[EngineSupervisor] engine not running (last seen " &
                                $"{If(lastSeenAlive = DateTime.MinValue, "never", lastSeenAlive.ToString("HH:mm:ss"))}) " &
                                $"— respawn in {waitMs \ 1000}s")

                Dim waited As Integer = 0
                While waited < waitMs AndAlso Not Volatile.Read(_shuttingDown)
                    Thread.Sleep(500)
                    waited += 500
                    Using external As Process = FindEngineProcess()
                        If external IsNot Nothing Then
                            ' Someone (the user?) started it while we waited.
                            _lastSpawnedPid = external.Id
                            externalStartDetected = True
                            Exit While
                        End If
                    End Using
                End While

                If Not externalStartDetected AndAlso Not Volatile.Read(_shuttingDown) Then
                    SyncLock _sync
                        _respawnDelayMs = Math.Min(_respawnDelayMs * 2, RespawnMaxDelayMs)
                    End SyncLock
                    SpawnIfNotRunning(reason:="respawn")
                End If

            Catch ex As Exception
                Log($"[EngineSupervisor] monitor error: {ex.Message}")
                Thread.Sleep(5000)
            End Try
        End While

        Log("[EngineSupervisor] monitor stopped")
    End Sub

End Class
