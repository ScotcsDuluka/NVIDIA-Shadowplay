Imports System
Imports System.IO
Imports System.Threading

' NvContainer — root/control-plane authority (skeleton, phase 1).
' Boot order: log init -> config load -> supervisor (spawn workers) ->
' TCP authority server -> wait for shutdown signal -> graceful stop.
Friend Module Program

    Private ReadOnly StopEvent As New ManualResetEventSlim(False)

    <STAThread>
    Sub Main(args As String())
        Dim configPath As String = "Config\NvContainer.json"
        Dim i As Integer = 0
        While i < args.Length
            If args(i) = "--config" AndAlso i + 1 < args.Length Then
                configPath = args(i + 1)
                i += 2
            Else
                i += 1
            End If
        End While

        Dim baseDir As String = AppContext.BaseDirectory
        If Not Path.IsPathRooted(configPath) Then
            configPath = Path.Combine(baseDir, configPath)
        End If

        Dim logsDir As String = Path.Combine(baseDir, "Logs")
        Directory.CreateDirectory(logsDir)
        ContainerLog.Init(Path.Combine(logsDir, "NvContainer.log"))

        AddHandler Console.CancelKeyPress,
            Sub(sender As Object, e As ConsoleCancelEventArgs)
                e.Cancel = True
                StopEvent.Set()
            End Sub

        ContainerLog.Log("NvContainer boot — root/control-plane authority (phase 1 skeleton)")
        ContainerLog.Log("config: " & configPath)

        Dim config As ContainerConfig = ContainerConfig.Load(configPath, baseDir)
        ContainerLog.Log("authority TCP port " & config.Port.ToString() &
                         " | security cookie: " & If(config.SecurityCookie.Length > 8,
                                                      config.SecurityCookie.Substring(0, 8) & "...",
                                                      "<short>") &
                         " | workers configured: " & config.Workers.Count.ToString())

        Dim supervisor As New Supervisor(config)
        Dim authority As New AuthorityServer(config, supervisor)

        supervisor.StartAll()
        authority.Start()

        ContainerLog.Log("NvContainer ready — TCP authority listening on 0.0.0.0:" & config.Port.ToString() &
                         ", supervising " & supervisor.Count().ToString() & " worker(s)")

        StopEvent.Wait()

        ContainerLog.Log("shutdown signal — stopping authority + workers")
        authority.StopListening()
        supervisor.StopAll()
        ContainerLog.Log("NvContainer stopped cleanly")
    End Sub

End Module
