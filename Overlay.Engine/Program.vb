' Program.vb — NVIDIA Share (osc overlay host) entry point.
'
' The Overlay Engine is the SECOND overlay host (M1): a transparent
' per-screen window hosting the NVIDIA GFE "osc" web app in WebView2,
' driven over the same loopback TCP hub protocol as the Forms overlay.
' It runs IN PARALLEL with NVIDIA ShadowPlay.exe (the Forms overlay) and
' never competes for global state:
'   - own single-instance mutex (Global\NVIDIA_Shadowplay_OscEngine)
'   - registers NO global hotkeys (HotkeyService ownership stays with the
'     Forms overlay; toggling comes from the TCP hub "open_overlay" or the
'     tray menu)
'   - no changes to any other process's behavior
'
' M1 scope: osc boot + cefQuery handshake + controller server + TCP
' RECORD_START/STOP + engine_get_status rehydration. Replay, notifications
' parity, settings UI and hotkey ownership arbitration are M2.

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms

Public Class Program

    <STAThread()>
    Public Shared Sub Main(args As String())
        Dim desktopMode As Boolean = args IsNot Nothing AndAlso
            args.Any(Function(a) String.Equals(a, "--desktop", StringComparison.OrdinalIgnoreCase))
        Dim parentPid As Integer = ReadArgumentInt(args, "--parent-pid")
        ' Main owns the lifecycle; Desktop owns WebView2, UI and injection.
        ' They use separate mutexes so the supervisor can launch its child.
        Dim mutexName As String = If(desktopMode,
            "Global\NVIDIA_Shadowplay_OscDesktop_SingleInstance",
            "Global\NVIDIA_Shadowplay_OscMain_SingleInstance")
        Dim created As Boolean = False
        Using mutex As New Mutex(True, mutexName, created)
            If Not created Then
                Return
            End If

            Try
                ' Root-fixed layout: assembly resolver + CWD to layout root.
                ' Must run before any type touches AppLayout paths.
                AppLayout.Initialize()
            Catch ex As Exception
                ' Dev runs outside the staged tree still work: AppLayout falls
                ' back to the exe dir. Never block startup on layout errors.
                Trace.WriteLine($"[OscEngine] AppLayout.Initialize: {ex.Message}")
            End Try

            ' Config mirror: keep the Forms overlay's Config\config.json in
            ' sync with ours (newest copy wins at boot, every write mirrors).
            Try
                AppConfigShared.RegisterMirror(
                    "C:/My Project/NVIDIA-Shadowplay/Overlay/bin/Release/net10.0-windows10.0.26100.0/Config/config.json")
                ' Engine Capture + API Capture own configs stay in sync too
                AppConfigShared.RegisterMirror(
                    "C:/My Project/NVIDIA-Shadowplay/Engine/bin/Debug/net10.0-windows10.0.26100.0/Config/config.json")
                AppConfigShared.RegisterMirror(
                    "C:/My Project/NVIDIA-Shadowplay/API/bin/Debug/net10.0-windows10.0.26100.0/Config/config.json")
            Catch ex As Exception
                Trace.WriteLine($"[OscEngine] RegisterMirror: {ex.Message}")
            End Try

            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            ' Any unhandled exception must leave a trace in the engine log —
            ' an exit without an explanation cost a debugging round during
            ' M1 bring-up (exit code 1 with a silent log).
            AddHandler Application.ThreadException, Sub(s, e)
                                                        Trace.WriteLine("[OscEngine] UI thread exception: " & e.Exception.Message)
                                                        Try
                                                            IO.File.AppendAllText(
                                                                IO.Path.Combine(AppLayout.P("Logs", "overlay-engine.log")),
                                                                DateTime.Now.ToString("HH:mm:ss.fff") & " UI EXCEPTION: " &
                                                                e.Exception.ToString() & Environment.NewLine)
                                                        Catch
                                                        End Try
                                                    End Sub
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
            AddHandler AppDomain.CurrentDomain.UnhandledException, Sub(s, e)
                                                                       Try
                                                                           IO.File.AppendAllText(
                                                                               IO.Path.Combine(AppLayout.P("Logs", "overlay-engine.log")),
                                                                               DateTime.Now.ToString("HH:mm:ss.fff") & " UNHANDLED: " &
                                                                               e.ExceptionObject.ToString() & Environment.NewLine)
                                                                       Catch
                                                                       End Try
                                                                   End Sub
            If Not desktopMode Then
                MainLog("mode=main args=" & String.Join(" ", args))
                RunMainSupervisor()
                Return
            End If

            ' Desktop process: the form is constructed up front but never
            ' shown until the first toggle. It owns WebView2 and injection.
            MainLog("mode=desktop pid=" & Process.GetCurrentProcess().Id.ToString())
            StartDesktopHeartbeat(parentPid)
            Dim ipcToken As String = ReadArgument(args, "--ipc-token")
            If parentPid > 0 AndAlso Not String.IsNullOrWhiteSpace(ipcToken) Then
                MainDesktopIpc.RunClient(ipcToken,
                    Function() Environment.HasShutdownStarted,
                    Process.GetCurrentProcess().Id,
                    Function() "desktop-ready")
            End If
            Dim overlay As New OscHostForm()
            HookAutoInject.Start()
            Application.Run(New ApplicationContext())
        End Using
    End Sub

    Private Shared Sub RunMainSupervisor()
        Try
            MainLog("main supervisor starting")
            Dim exe As String = Process.GetCurrentProcess().MainModule.FileName
            Dim ipcToken As String = MainDesktopIpc.CreateToken()
            Dim stopIpc As Boolean = False
            Dim lastIpcTicks As Long = DateTime.UtcNow.Ticks
            MainDesktopIpc.RunServer(ipcToken,
                Function() stopIpc,
                Sub(command, reason)
                    If String.Equals(command, "hello", StringComparison.OrdinalIgnoreCase) OrElse
                       String.Equals(command, "heartbeat", StringComparison.OrdinalIgnoreCase) Then
                        Interlocked.Exchange(lastIpcTicks, DateTime.UtcNow.Ticks)
                    End If
                    MainLog("ipc command=" & If(command, "") & " reason=" & If(reason, ""))
                End Sub)
            While True
                Interlocked.Exchange(lastIpcTicks, DateTime.UtcNow.Ticks)
                Dim desktop As Process = Process.Start(New ProcessStartInfo With {
                    .FileName = exe,
                    .Arguments = "--desktop --parent-pid " & Process.GetCurrentProcess().Id.ToString() &
                                 " --ipc-token " & ipcToken,
                    .WorkingDirectory = AppContext.BaseDirectory,
                    .UseShellExecute = False,
                    .CreateNoWindow = True})
                If desktop Is Nothing Then Return
                MainLog("desktop child started pid=" & desktop.Id.ToString())
                Trace.WriteLine("[OscMain] Desktop process started pid=" & desktop.Id.ToString())
                While Not desktop.HasExited
                    Thread.Sleep(1000)
                    If DesktopHeartbeatStale() OrElse
                       DateTime.UtcNow - New DateTime(Interlocked.Read(lastIpcTicks), DateTimeKind.Utc) >
                       TimeSpan.FromSeconds(8) Then
                        MainLog("desktop health stale; restarting child")
                        Try : desktop.Kill() : Catch : End Try
                        Exit While
                    End If
                End While
                MainLog("desktop child exited code=" & desktop.ExitCode.ToString())
                Trace.WriteLine("[OscMain] Desktop process exited code=" & desktop.ExitCode.ToString())
                desktop.Dispose()
                Thread.Sleep(500)
            End While
        Catch ex As Exception
                MainLog("supervisor failed: " & ex.ToString())
                Trace.WriteLine("[OscMain] Desktop process failed: " & ex.Message)
        End Try
    End Sub

    Private Shared Function ReadArgument(args As String(), name As String) As String
        If args Is Nothing Then Return Nothing
        For i As Integer = 0 To args.Length - 2
            If String.Equals(args(i), name, StringComparison.OrdinalIgnoreCase) Then
                Return args(i + 1)
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function ReadArgumentInt(args As String(), name As String) As Integer
        If args Is Nothing Then Return 0
        For i As Integer = 0 To args.Length - 2
            If String.Equals(args(i), name, StringComparison.OrdinalIgnoreCase) Then
                Dim value As Integer
                If Integer.TryParse(args(i + 1), value) Then Return value
            End If
        Next
        Return 0
    End Function

    Private Shared Function DesktopHeartbeatPath() As String
        Return Path.Combine(AppContext.BaseDirectory, "Logs", "desktop-health.json")
    End Function

    Private Shared Function DesktopHeartbeatStale() As Boolean
        Try
            Dim p As String = DesktopHeartbeatPath()
            Return Not File.Exists(p) OrElse DateTime.UtcNow - File.GetLastWriteTimeUtc(p) > TimeSpan.FromSeconds(8)
        Catch
            Return True
        End Try
    End Function

    Private Shared Sub StartDesktopHeartbeat(parentPid As Integer)
        Dim t As New Thread(
            Sub()
                While True
                    Try
                        If parentPid > 0 Then
                            Using parent = Process.GetProcessById(parentPid)
                                If parent.HasExited Then
                                    MainLog("parent exited; desktop shutting down")
                                    Environment.Exit(0)
                                End If
                            End Using
                        End If
                        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"))
                        Dim content As String = "{""pid"":" & Process.GetCurrentProcess().Id.ToString() &
                            ",""parentPid"":" & parentPid.ToString() &
                            ",""utc"":""" & DateTime.UtcNow.ToString("O") & """}"
                        File.WriteAllText(DesktopHeartbeatPath(), content)
                    Catch
                        If parentPid > 0 Then
                            MainLog("parent unavailable; desktop shutting down")
                            Environment.Exit(0)
                        End If
                    End Try
                    Thread.Sleep(2000)
                End While
            End Sub) With {.IsBackground = True, .Name = "DesktopHeartbeat"}
        t.Start()
    End Sub

    Private Shared Sub MainLog(message As String)
        Try
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"))
            File.AppendAllText(
                Path.Combine(AppContext.BaseDirectory, "Logs", "main-supervisor.log"),
                DateTime.Now.ToString("HH:mm:ss.fff") & " " & message & Environment.NewLine)
        Catch
        End Try
    End Sub

End Class
