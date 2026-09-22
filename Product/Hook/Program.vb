' Program.vb — Share.exe #3 (Hook): placeholder process slot.
'
' The 3-process Share model (Product/README.md) mirrors the real host's
' process tree: Coordinator #1 spawns Desktop #2 (osc UI) and Hook #3.
' The real NVIDIA stack runs a hook-bearing Share instance for the
' capture/injection plumbing (nvspcap-style). This project reserves that
' process slot TODAY so the tree shape, supervision contract and process
' naming are final — the actual hook payload lands in a later phase
' WITHOUT changing the Coordinator, the autostart entry or the installer.
'
' Behavior (deliberately minimal):
'   - stays alive in an idle loop (no window, no tray, no CPU)
'   - self-exits when the Coordinator (--parent-pid) dies — no orphans
'   - heartbeat to the log every 60s so the process is observable
'   - exits on the ShareHook_Exit named event (Operator tooling handle)

Option Strict On
Option Explicit On
Option Infer On

Imports System.Diagnostics
Imports System.IO
Imports System.Threading

Public NotInheritable Class Program

    Private Const ParentPollMs As Integer = 2000
    Private Const HeartbeatSec As Integer = 60
    Private Shared ReadOnly ExitEventName As String = "Local\ShareHook_Exit"

    Private Shared ReadOnly _logLock As New Object()
    Private Shared _logPath As String = String.Empty

    Private Sub New()
    End Sub

    Public Shared Sub Main(args As String())
        Dim parentPid As Integer = 0
        Dim parentArg As String = ReadArg(args, "--parent-pid")
        If parentArg IsNot Nothing Then Integer.TryParse(parentArg, parentPid)

        InitLog()
        Dim myPid As String = Process.GetCurrentProcess().Id.ToString()
        Log($"[Hook] Share hook host starting (pid {myPid}, placeholder slot)")
        If parentPid > 0 Then Log($"[Hook] watching parent pid {parentPid}")

        Dim exitEvent As EventWaitHandle = Nothing
        Try
            exitEvent = New EventWaitHandle(False, EventResetMode.ManualReset, ExitEventName)
        Catch ex As Exception
            Log($"[Hook] exit event unavailable ({ex.Message})")
        End Try

        Dim lastBeat As DateTime = DateTime.UtcNow
        While True
            ' parent watch — coordinator gone means the tree is going down
            If parentPid > 0 Then
                Try
                    Using p As Process = Process.GetProcessById(parentPid)
                        p.Refresh()
                        If p.HasExited Then
                            Log("[Hook] parent gone — exiting")
                            Return
                        End If
                    End Using
                Catch ex As Exception
                    Log($"[Hook] parent check failed ({ex.Message}) — exiting")
                    Return
                End Try
            End If

            ' operator exit handle
            If exitEvent IsNot Nothing AndAlso exitEvent.WaitOne(ParentPollMs) Then
                Log("[Hook] exit event signaled — exiting")
                Return
            End If

            If (DateTime.UtcNow - lastBeat).TotalSeconds >= HeartbeatSec Then
                Log($"[Hook] heartbeat (pid {myPid}, idle)")
                lastBeat = DateTime.UtcNow
            End If
        End While
    End Sub

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
            _logPath = Path.Combine(dir, "hook.log")
        Catch
            _logPath = Path.Combine(Path.GetTempPath(), "NVIDIA-Share-Hook.log")
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
