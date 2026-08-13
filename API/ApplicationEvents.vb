Imports Microsoft.VisualBasic.ApplicationServices
Imports System.IO

Namespace My
    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed. This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active.
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.

    ' **NEW** ApplyApplicationDefaults: Raised when the application queries default values to be set for the application.

    Partial Friend Class MyApplication

        ' ✅ m11 FIX: log unhandled exceptions to a file before the app dies.
        ' Without this, a crash on a non-UI thread (TcpListener accept, HeartbeatMonitor,
        ' Broadcast) tears the process down with no diagnostic info — the user just sees
        ' the hub disappear from the tray.
        Private Sub MyApplication_UnhandledException(
                sender As Object,
                e As UnhandledExceptionEventArgs) Handles Me.UnhandledException

            Try
                Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                If Not Directory.Exists(logDir) Then Directory.CreateDirectory(logDir)
                Dim logPath As String = Path.Combine(logDir, "api-crash.log")
                Dim logLine As String =
                    "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "] " &
                    "UNHANDLED EXCEPTION" & Environment.NewLine &
                    "Message: " & e.Exception.Message & Environment.NewLine &
                    "Type: " & e.Exception.GetType().FullName & Environment.NewLine &
                    "Stack:" & Environment.NewLine & e.Exception.StackTrace & Environment.NewLine &
                    New String("-"c, 60) & Environment.NewLine
                File.AppendAllText(logPath, logLine)
            Catch
                ' Last-resort: if even logging fails, swallow silently.
                ' The app is about to die anyway.
            End Try

            ' Mark as handled so Windows Error Reporting doesn't pop up.
            ' The user can read api-crash.log to see what happened.
            e.ExitApplication = True
        End Sub

    End Class
End Namespace
