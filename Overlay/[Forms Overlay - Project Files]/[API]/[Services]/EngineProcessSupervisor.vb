Option Strict On
Option Explicit On
Option Infer On

Imports System.Diagnostics
Imports System.IO
Imports System.Threading

Public NotInheritable Class EngineProcessSupervisor

    Private Const ProcessName As String = "NVIDIA Capture"     
    Private Const ExeFileName As String = "NVIDIA Capture.exe"

    Private Const RespawnBaseDelayMs As Integer = 3000
    Private Const RespawnMaxDelayMs As Integer = 60000
    Private Const StableRunMs As Integer = 30000                

    Private Shared ReadOnly _sync As New Object()
    Private Shared _started As Boolean = False
    Private Shared _shuttingDown As Boolean = False
    Private Shared _monitorThread As Thread
    Private Shared _lastSpawnedPid As Integer = 0
    Private Shared _lastSpawnAt As DateTime = DateTime.MinValue
    Private Shared _respawnDelayMs As Integer = RespawnBaseDelayMs

    Private Shared _spawnInFlight As Boolean = False

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

    Public Shared Sub Shutdown()
        SyncLock _sync
            _shuttingDown = True
        End SyncLock
    End Sub

    Public Shared Function FindEngineProcess() As Process
        Dim procs As Process() = Process.GetProcessesByName(ProcessName)
        If procs Is Nothing OrElse procs.Length = 0 Then Return Nothing
        Dim first As Process = procs(0)
        For i As Integer = 1 To procs.Length - 1
            Try : procs(i).Dispose() : Catch : End Try
        Next
        Return first
    End Function

    Private Shared Sub SpawnIfNotRunning(reason As String)
        SyncLock _sync
            If _shuttingDown Then Return

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
                .UseShellExecute = True,          
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

    Private Shared Function ResolveEngineExePath() As String
        Try
            Dim baseDir As String = AppLayout.Dir

            Dim layoutCandidate As String = Path.Combine(baseDir, "Application", ExeFileName)
            If File.Exists(layoutCandidate) Then Return layoutCandidate

            Dim candidate As String = Path.Combine(baseDir, ExeFileName)
            If File.Exists(candidate) Then Return candidate

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

    Private Shared Sub MonitorLoop()
        
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
