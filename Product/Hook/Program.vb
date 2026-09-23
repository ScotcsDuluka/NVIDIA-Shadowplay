' Program.vb — NVIDIA Share.exe #4 (WebView Hook): placeholder slot with
' the SHOW PORT live (per the GFE process map — docs/GFE-PROCESS-MAP.md).
'
' The 4-instance Share model mirrors the real host's process tree:
' Coordinator #1 spawns WinForm #2, WebView #3 (--desktop) and THIS #4.
' The real NVIDIA stack runs a hook-bearing Share instance for the
' capture/injection plumbing (nvspcap-style). This project reserves that
' slot TODAY — the tree shape, supervision contract and process naming
' are final — while the actual hook payload lands in a later phase
' WITHOUT changing the Coordinator, the autostart entry or the installer.
'
' Show port (127.0.0.1:59004) — the slot is OBSERVABLE instead of invisible:
'   GET /              -> text banner (instance 4/4)
'   GET /hook/status   -> JSON {mode, instance, status, pid, parentPid,
'                               uptimeSec, port, inject, endpoints}
'   GET /hook/inject   -> 501 JSON (payload not implemented — reserved)
'   GET /hook/exit     -> graceful exit (operator handle)
' CORS is open (*) so an osc page or a dev page can surface the status.
'
' Behavior (deliberately minimal besides the port):
'   - stays alive in an idle loop (no window, no tray, no CPU)
'   - self-exits when the Coordinator (--parent-pid) dies — no orphans
'   - heartbeat to the log every 60s so the process is observable
'   - exits on the ShareHook_Exit named event (Operator tooling handle)
'   - if HTTP.SYS refuses the prefix (URLACL), degrade to the idle slot
'     and keep logging — the port is a showpiece, not a dependency

Option Strict On
Option Explicit On
Option Infer On

Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading

Public NotInheritable Class Program

    Private Const ParentPollMs As Integer = 2000
    Private Const HeartbeatSec As Integer = 60
    Private Const ShowPort As Integer = 59004
    Private Shared ReadOnly ExitEventName As String = "Local\ShareHook_Exit"

    Private Shared ReadOnly _logLock As New Object()
    Private Shared _logPath As String = String.Empty
    Private Shared _parentPid As Integer = 0
    Private Shared _startedAtUtc As DateTime = DateTime.UtcNow
    Private Shared _listener As HttpListener
    Private Shared _listenerAlive As Boolean = False

    Private Sub New()
    End Sub

    Public Shared Sub Main(args As String())
        Dim parentArg As String = ReadArg(args, "--parent-pid")
        If parentArg IsNot Nothing Then Integer.TryParse(parentArg, _parentPid)

        InitLog()
        Dim myPid As String = Process.GetCurrentProcess().Id.ToString()
        Log($"[Hook] NVIDIA Share.exe [instance 4/4 — webview-hook] starting (pid {myPid}, placeholder slot)")
        If _parentPid > 0 Then Log($"[Hook] watching parent pid {_parentPid}")

        StartShowPort()

        Dim exitEvent As EventWaitHandle = Nothing
        Try
            exitEvent = New EventWaitHandle(False, EventResetMode.ManualReset, ExitEventName)
        Catch ex As Exception
            Log($"[Hook] exit event unavailable ({ex.Message})")
        End Try

        Dim lastBeat As DateTime = DateTime.UtcNow
        While True
            ' parent watch — coordinator gone means the tree is going down
            If _parentPid > 0 Then
                Try
                    Using p As Process = Process.GetProcessById(_parentPid)
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
                Log($"[Hook] heartbeat (pid {myPid}, show-port={If(_listenerAlive, "up", "down")}, idle)")
                lastBeat = DateTime.UtcNow
            End If
        End While
    End Sub

    ' ── show port ───────────────────────────────────────────────────
    Private Shared Sub StartShowPort()
        Try
            _listener = New HttpListener()
            _listener.Prefixes.Add("http://127.0.0.1:" & ShowPort.ToString() & "/")
            _listener.Start()
            _listenerAlive = True
            Log($"[Hook] show port listening on http://127.0.0.1:{ShowPort}/ (status / inject / exit)")
            Dim t As New Thread(AddressOf ListenerLoop) With {.Name = "ShareHookShowPort", .IsBackground = True}
            t.Start()
        Catch ex As Exception
            _listenerAlive = False
            Log($"[Hook] show port unavailable ({ex.Message}) — continuing as idle slot")
        End Try
    End Sub

    Private Shared Sub ListenerLoop()
        While _listener IsNot Nothing AndAlso _listener.IsListening
            Dim ctx As HttpListenerContext = Nothing
            Try
                ctx = _listener.GetContext()
                Handle(ctx)
            Catch ex As Exception
                If _listener IsNot Nothing AndAlso _listener.IsListening Then
                    Log($"[Hook] show port request error: {ex.Message}")
                End If
                Try
                    If ctx IsNot Nothing Then ctx.Response.Close()
                Catch
                End Try
            End Try
        End While
    End Sub

    Private Shared Sub Handle(ctx As HttpListenerContext)
        Dim req As HttpListenerRequest = ctx.Request
        Dim path As String = If(req.Url, New Uri("http://127.0.0.1/")).AbsolutePath.ToLowerInvariant()
        Dim res As HttpListenerResponse = ctx.Response

        res.Headers("Access-Control-Allow-Origin") = "*"
        res.Headers("Access-Control-Allow-Methods") = "GET,POST"
        res.Headers("Access-Control-Allow-Headers") = "Content-Type"

        Select Case path
            Case "/"
                SendText(res, 200,
                    "NVIDIA Share.exe [instance 4/4 — WebView Hook] placeholder slot" & Environment.NewLine &
                    "show port " & ShowPort.ToString() & " — payload not implemented yet (reserved)" & Environment.NewLine &
                    "endpoints: /hook/status | /hook/inject | /hook/exit" & Environment.NewLine)
            Case "/hook/status"
                Dim uptime As Long = CLng((DateTime.UtcNow - _startedAtUtc).TotalSeconds)
                Dim json As String = "{""mode"":""webview-hook""," &
                    """instance"":4," &
                    """status"":""placeholder""," &
                    """pid"":" & Process.GetCurrentProcess().Id.ToString() & "," &
                    """parentPid"":" & _parentPid.ToString() & "," &
                    """uptimeSec"":" & uptime.ToString() & "," &
                    """port"":" & ShowPort.ToString() & "," &
                    """startedUtc"":""" & _startedAtUtc.ToString("o") & """," &
                    """inject"":{""implemented"":false,""reason"":""payload pending — reserved slot (nvspcap-style hook lands later)""}," &
                    """endpoints"":[""/hook/status"",""/hook/inject"",""/hook/exit""]}"
                SendText(res, 200, json & Environment.NewLine, "application/json")
            Case "/hook/inject"
                SendText(res, 501,
                    "{""status"":false,""code"":-1,""codeText"":""NotImplemented""," &
                    """message"":""webview hook payload not implemented — placeholder show port""}" & Environment.NewLine,
                    "application/json")
            Case "/hook/exit"
                SendText(res, 200, "{""status"":true,""message"":""hook slot exiting""}" & Environment.NewLine,
                    "application/json")
                Log("[Hook] /hook/exit — graceful exit requested over the show port")
                Task.Run(Sub()
                             Thread.Sleep(150)
                             Try
                                 If _listener IsNot Nothing Then _listener.Stop()
                             Catch
                             End Try
                             Environment.Exit(0)
                         End Sub)
            Case Else
                SendText(res, 404,
                    "{""status"":false,""code"":-1,""codeText"":""UnknownEndpoint""}" & Environment.NewLine,
                    "application/json")
        End Select
    End Sub

    Private Shared Sub SendText(res As HttpListenerResponse, code As Integer, body As String,
                                Optional contentType As String = "text/plain")
        Try
            res.StatusCode = code
            res.ContentType = contentType
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(body)
            res.ContentLength64 = bytes.Length
            res.OutputStream.Write(bytes, 0, bytes.Length)
            res.OutputStream.Close()
        Catch
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
