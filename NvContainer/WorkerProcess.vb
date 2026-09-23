Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading

' One supervised process: spawn, monitor, restart with backoff.
' States: Stopped | Starting | Running | Backoff | Failed
Friend Class WorkerProcess

    Private ReadOnly _spec As WorkerSpec
    Private ReadOnly _lock As New Object()
    Private _proc As Process
    Private _state As String = "Stopped"
    Private _pid As Integer = 0
    Private _exitCode As Integer = 0
    Private _restarts As Integer = 0
    Private _stopRequested As Boolean = False
    Private _adopted As Boolean = False
    Private _startedAtUtc As DateTime = DateTime.MinValue
    Private _monitor As Thread

    Public Sub New(spec As WorkerSpec)
        _spec = spec
    End Sub

    Public ReadOnly Property Name As String
        Get
            Return _spec.Name
        End Get
    End Property

    Public ReadOnly Property Spec As WorkerSpec
        Get
            Return _spec
        End Get
    End Property

    Public Function Status() As Dictionary(Of String, Object)
        SyncLock _lock
            Dim d As New Dictionary(Of String, Object)
            d("name") = _spec.Name
            d("state") = _state
            d("pid") = _pid
            d("restarts") = _restarts
            d("exitCode") = _exitCode
            d("adopted") = _adopted
            d("exe") = _spec.Exe
            Return d
        End SyncLock
    End Function

    Public Sub Start(reason As String)
        SyncLock _lock
            If _proc IsNot Nothing AndAlso Not _proc.HasExited Then
                ContainerLog.Log("worker '" & _spec.Name & "' already running (pid " & _pid.ToString() & ")")
                Return
            End If
            If Not System.IO.File.Exists(_spec.Exe) Then
                _state = "Failed"
                ContainerLog.Log("worker '" & _spec.Name & "' exe missing: " & _spec.Exe)
                Return
            End If
            _stopRequested = False

            ' ── Adopt path: an instance of this exact image may already be
            ' running (e.g. spawned by the old scheduled task before the
            ' ownership handover). Adopt it instead of spawning a duplicate —
            ' that would be a silent dual-engine. The monitor below then
            ' watches the adopted pid like any other; if it dies the normal
            ' restart policy takes over and the container becomes the parent
            ' of the next spawn.
            If _spec.AdoptExisting Then
                Dim ap As Process = FindAdoptable()
                If ap IsNot Nothing Then
                    _adopted = True
                    _proc = ap
                    _pid = ap.Id
                    _state = "Running"
                    _exitCode = 0
                    _startedAtUtc = DateTime.UtcNow
                    ContainerLog.Log("worker '" & _spec.Name & "' ADOPTED existing process (pid " &
                                     _pid.ToString() & ") — " & _spec.Exe)
                    GoTo MonitorStart
                End If
            End If
            _adopted = False

            _state = "Starting"
            ContainerLog.Log("worker '" & _spec.Name & "' spawn (" & reason & "): " & _spec.Exe &
                             If(_spec.Args <> "", " " & _spec.Args, ""))
            Try
                Dim psi As New ProcessStartInfo()
                psi.FileName = _spec.Exe
                psi.Arguments = _spec.Args
                psi.UseShellExecute = False
                psi.CreateNoWindow = True
                If _spec.WorkingDirectory <> "" Then psi.WorkingDirectory = _spec.WorkingDirectory
                _proc = Process.Start(psi)
                If _proc Is Nothing Then
                    _state = "Failed"
                    ContainerLog.Log("worker '" & _spec.Name & "' spawn returned null")
                    Return
                End If
                _pid = _proc.Id
                _state = "Running"
                _startedAtUtc = DateTime.UtcNow
                ContainerLog.Log("worker '" & _spec.Name & "' running (pid " & _pid.ToString() & ")")
            Catch ex As Exception
                _state = "Failed"
                ContainerLog.Log("worker '" & _spec.Name & "' spawn failed: " & ex.Message)
                Return
            End Try
        End SyncLock

MonitorStart:
        If _monitor Is Nothing OrElse Not _monitor.IsAlive Then
            _monitor = New Thread(AddressOf MonitorLoop)
            _monitor.IsBackground = True
            _monitor.Start()
        End If
    End Sub

    ' Look for a live process whose full image path equals _spec.Exe.
    ' GetProcessesByName matches by image name only — the path check is what
    ' keeps us from adopting the WRONG instance (name collisions are real:
    ' NVIDIA ships their own nvcontainer.exe in Program Files).

    Private Function FindAdoptable() As Process
        Try
            Dim wantName As String = System.IO.Path.GetFileNameWithoutExtension(_spec.Exe)
            Dim candidates As Process() = Process.GetProcessesByName(wantName)
            For Each c As Process In candidates
                Try
                    Dim imgPath As String = c.MainModule.FileName
                    If String.Equals(imgPath, _spec.Exe, StringComparison.OrdinalIgnoreCase) Then
                        Return c
                    End If
                Catch
                    ' MainModule unreadable (elevation/arch mismatch) — not adoptable
                End Try
            Next
        Catch
        End Try
        Return Nothing
    End Function

    Public Sub Shutdown()
        Dim p As Process = Nothing
        SyncLock _lock
            _stopRequested = True
            p = _proc
        End SyncLock
        If p IsNot Nothing AndAlso Not p.HasExited Then
            ContainerLog.Log("worker '" & _spec.Name & "' stop requested (pid " & p.Id.ToString() & ")")
            Try
                p.Kill(True)
            Catch ex As Exception
                ContainerLog.Log("worker '" & _spec.Name & "' kill failed: " & ex.Message)
            End Try
            Try
                p.WaitForExit(5000)
            Catch
            End Try
        End If
        SyncLock _lock
            _state = "Stopped"
            _pid = 0
            _adopted = False
            _proc = Nothing
        End SyncLock
    End Sub

    Private Sub MonitorLoop()
        While True
            Thread.Sleep(1000)
            Dim p As Process = Nothing
            SyncLock _lock
                If _stopRequested Then
                    Return
                End If
                p = _proc
            End SyncLock
            If p Is Nothing Then
                Continue While
            End If

            Dim exited As Boolean = False
            Try
                exited = p.HasExited
            Catch
                exited = False
            End Try
            If Not exited Then
                Continue While
            End If

            Dim code As Integer = 0
            Dim ranSec As Integer = 0
            Try
                code = p.ExitCode
            Catch
            End Try
            SyncLock _lock
                ranSec = CInt((DateTime.UtcNow - _startedAtUtc).TotalSeconds)
                _exitCode = code
                If ranSec > 60 Then
                    _restarts = 0
                End If
                If _stopRequested Then
                    _state = "Stopped"
                    Return
                End If
            End SyncLock

            SyncLock _lock
                If _restarts < _spec.MaxRestarts Then
                    _restarts += 1
                    _state = "Backoff"
                Else
                    _state = "Failed"
                End If
            End SyncLock
            ContainerLog.Log("worker '" & _spec.Name & "' exited (code " & code.ToString() &
                             ", ran " & ranSec.ToString() & "s, restart " & _restarts.ToString() & "/" &
                             _spec.MaxRestarts.ToString() & ")")

            If _state = "Failed" Then
                ContainerLog.Log("worker '" & _spec.Name & "' exceeded max restarts — giving up")
                Return
            End If

            Thread.Sleep(Math.Max(1, _spec.RestartBackoffSeconds) * 1000)
            SyncLock _lock
                If _stopRequested Then
                    Return
                End If
            End SyncLock
            ' Keep THIS monitor thread looping: it becomes the watcher of the
            ' restarted process (Start() sees the live monitor and skips spawn).
            Start("auto-restart")
        End While
    End Sub

End Class
