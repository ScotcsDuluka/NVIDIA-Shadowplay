' Program.vb — NVIDIA Notifier.exe: tray notification slot for the GFE
' process map (docs/GFE-PROCESS-MAP.md).
'
' In the real host the notifications come from the Share family; in OUR
' five-family model Notifier stays its own component (the legacy stack ran
' NVIDIA Notifier.exe off the TCP :5000 hub). This rebuild keeps the role
' but rewires it to the NEW API: it polls the Web Helper surface on
' :59001 and raises tray balloons on record-state edges — no legacy TCP,
' no private hub:
'
'   GET /ShadowPlay/v.1.0/Record/Running  -> {"running":bool}
'   GET /ShadowPlay/v.1.0/Record/Enable   -> {"status":bool}
'
' Edges (only CHANGES fire a balloon; boot state is silent):
'   Enable false->true   "ShadowPlay recording enabled"
'   Enable true->false   "ShadowPlay recording disabled"
'   Running false->true  "Recording started"
'   Running true->false  "Recording stopped"
'
' Behavior:
'   - hidden form + NotifyIcon (tray) — no window ever shown
'   - backend down = no balloons, poll keeps running (never crashes)
'   - self-exits when the Coordinator (--parent-pid) dies
'   - single-instance mutex Global\NVIDIA_Notifier_SingleInstance

Option Strict On
Option Explicit On
Option Infer On

Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Threading
Imports System.Windows.Forms

Public NotInheritable Class Program

    Private Const PollMs As Integer = 2000
    Private Const HttpTimeoutMs As Integer = 2500
    Private Const ParentPollMs As Integer = 2000

    Private Shared _backendUrl As String = "http://127.0.0.1:59001"
    Private Shared _parentPid As Integer = 0

    Private Sub New()
    End Sub

    <STAThread()>
    Public Shared Sub Main(args As String())
        Dim backendArg As String = ReadArg(args, "--backend-url")
        If backendArg IsNot Nothing Then _backendUrl = backendArg.TrimEnd("/"c)
        Dim parentArg As String = ReadArg(args, "--parent-pid")
        If parentArg IsNot Nothing Then Integer.TryParse(parentArg, _parentPid)

        Dim created As Boolean = False
        Using mutex As New Mutex(True, "Global\NVIDIA_Notifier_SingleInstance", created)
            If Not created Then Return

            InitLog()
            Log($"[Notifier] NVIDIA Notifier.exe starting (pid {Process.GetCurrentProcess().Id}, backend {_backendUrl})")

            If _parentPid > 0 Then
                Dim watcher As New Thread(AddressOf WatchParent) With {.Name = "NotifierParentWatch", .IsBackground = True}
                watcher.Start()
            End If

            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New NotifierContext())
        End Using
    End Sub

    ' ── hidden context: owns the tray icon + poll timer ─────────────
    Private NotInheritable Class NotifierContext
        Inherits ApplicationContext

        Private ReadOnly _icon As NotifyIcon
        Private ReadOnly _timer As New System.Windows.Forms.Timer()
        Private _lastEnable As Boolean? = Nothing
        Private _lastRunning As Boolean? = Nothing

        Public Sub New()
            _icon = New NotifyIcon With {
                .Text = "NVIDIA ShadowPlay",
                .Visible = True,
                .Icon = SystemIcons.Shield
            }
            _timer.Interval = PollMs
            AddHandler _timer.Tick, AddressOf OnPoll
            _timer.Start()
            Log("[Notifier] tray up — polling /Record/Enable + /Record/Running")
        End Sub

        Private Sub OnPoll(sender As Object, e As EventArgs)
            Dim enable As Boolean? = PollBool("/ShadowPlay/v.1.0/Record/Enable", """status"":true")
            If enable.HasValue AndAlso _lastEnable.HasValue AndAlso enable.Value <> _lastEnable.Value Then
                Balloon(If(enable.Value, "ShadowPlay recording enabled", "ShadowPlay recording disabled"))
                Log($"[Notifier] Enable edge -> {enable.Value}")
            End If
            If enable.HasValue Then _lastEnable = enable.Value

            Dim running As Boolean? = PollBool("/ShadowPlay/v.1.0/Record/Running", """running"":true")
            If running.HasValue AndAlso _lastRunning.HasValue AndAlso running.Value <> _lastRunning.Value Then
                Balloon(If(running.Value, "Recording started", "Recording stopped"))
                Log($"[Notifier] Running edge -> {running.Value}")
            End If
            If running.HasValue Then _lastRunning = running.Value
        End Sub

        Private Sub Balloon(text As String)
            Try
                _icon.BalloonTipTitle = "NVIDIA ShadowPlay"
                _icon.BalloonTipText = text
                _icon.BalloonTipIcon = ToolTipIcon.Info
                _icon.ShowBalloonTip(3000)
            Catch ex As Exception
                Log($"[Notifier] balloon failed: {ex.Message}")
            End Try
        End Sub

        Private Function PollBool(path As String, trueMarker As String) As Boolean?
            Try
                Dim req As HttpWebRequest = CType(WebRequest.Create(_backendUrl & path), HttpWebRequest)
                req.Method = "GET"
                req.Timeout = HttpTimeoutMs
                req.ReadWriteTimeout = HttpTimeoutMs
                Using resp As WebResponse = req.GetResponse()
                    Using reader As New IO.StreamReader(resp.GetResponseStream())
                        Dim body As String = reader.ReadToEnd()
                        Return body IsNot Nothing AndAlso body.ToLowerInvariant().Contains(trueMarker)
                    End Using
                End Using
            Catch
                ' backend down / route missing — silent, keep last state
                Return Nothing
            End Try
        End Function

        Protected Overrides Sub ExitThreadCore()
            Try
                _timer.Stop()
                _icon.Visible = False
                _icon.Dispose()
            Catch
            End Try
            MyBase.ExitThreadCore()
        End Sub
    End Class

    ' ── parent watch (Coordinator death => tree is going down) ──────
    Private Shared Sub WatchParent()
        While True
            Try
                Using parent As Process = Process.GetProcessById(_parentPid)
                    parent.Refresh()
                    If parent.HasExited Then
                        Log("[Notifier] parent gone — exiting")
                        Environment.Exit(0)
                    End If
                End Using
            Catch ex As Exception
                Log($"[Notifier] parent check failed ({ex.Message}) — exiting")
                Environment.Exit(0)
            End Try
            Thread.Sleep(ParentPollMs)
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

    ' ── log helpers ─────────────────────────────────────────────────
    Private Shared _logPath As String = String.Empty
    Private Shared ReadOnly _logLock As New Object()

    Private Shared Sub InitLog()
        Try
            Dim dir As String = Path.Combine(AppContext.BaseDirectory, "logs")
            Directory.CreateDirectory(dir)
            _logPath = Path.Combine(dir, "notifier.log")
        Catch
            _logPath = Path.Combine(Path.GetTempPath(), "NVIDIA-Notifier.log")
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
