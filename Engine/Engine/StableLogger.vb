Imports System.IO
Imports System.Text

Public Class StableLogger
    Private Shared _logPath As String
    Private Shared _logFile As String
    Private Shared _lock As New Object()

    Public Enum LogLevel As Integer
        DEBUG = 0
        INFO = 1
        WARNING = 2
        ERROR_LVL = 3
        FATAL = 4
    End Enum

    Public Shared Sub Initialize(Optional customPath As String = "")
        SyncLock _lock
            Try
                If String.IsNullOrEmpty(customPath) Then
                    _logPath = Path.Combine(Application.StartupPath, "Logs")
                Else
                    _logPath = customPath
                End If

                If Not Directory.Exists(_logPath) Then
                    Directory.CreateDirectory(_logPath)
                End If

                Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
                _logFile = Path.Combine(_logPath, $"ShadowPlay-{today}.log")

                ' Clear old logs (>7 days)
                CleanupOldLogs()

                Log(LogLevel.INFO, "========================================")
                Log(LogLevel.INFO, $"NVIDIA ShadowPlay Started - {DateTime.Now}")
                Log(LogLevel.INFO, $"OS: {Environment.OSVersion}")
                Log(LogLevel.INFO, $".NET: {Environment.Version}")

            Catch ex As Exception
                Debug.WriteLine($"Logger Init Failed: {ex.Message}")
            End Try
        End SyncLock
    End Sub

    Public Shared Sub Log(level As LogLevel, message As String, Optional context As String = "")
        SyncLock _lock
            Try
                Dim timestamp As String = DateTime.Now.ToString("HH:mm:ss.fff")
                Dim levelStr As String = level.ToString().PadRight(7)
                Dim ctxStr As String = If(String.IsNullOrEmpty(context), "", $"[{context}]")
                Dim logEntry As String = $"[{timestamp}] [{levelStr}]{ctxStr} {message}"

#If DEBUG Then
                Debug.WriteLine(logEntry)
#End If

                Using writer As StreamWriter = New StreamWriter(_logFile, True, Encoding.UTF8)
                    writer.WriteLine(logEntry)
                End Using

            Catch ex As Exception
                ' Don't crash if logging fails!
                Debug.WriteLine($"Logging Failed: {ex.Message}")
            End Try
        End SyncLock
    End Sub

    Public Shared Sub LogException(ex As Exception, context As String)
        Log(LogLevel.FATAL, $"EXCEPTION in {context}:")
        Log(LogLevel.FATAL, $"  Message: {ex.Message}")
        Log(LogLevel.FATAL, $"  StackTrace: {ex.StackTrace}")

        If ex.InnerException IsNot Nothing Then
            Log(LogLevel.FATAL, $"  InnerException: {ex.InnerException.Message}")
        End If

        If ex.TargetSite IsNot Nothing Then
            Log(LogLevel.FATAL, $"  TargetSite: {ex.TargetSite.Name}")
        End If
    End Sub

    Public Shared Function GetLogFilePath() As String
        Return _logFile
    End Function

    Private Shared Sub CleanupOldLogs()
        Try
            Dim files As String() = Directory.GetFiles(_logPath, "ShadowPlay-*.log")

            For Each f In files
                Try
                    Dim fileInfo As New FileInfo(f)
                    If (DateTime.Now - fileInfo.CreationTime).Days > 7 Then
                        File.Delete(f)
                        Debug.WriteLine($"Deleted old log: {f}")
                    End If
                Catch
                End Try
            Next

        Catch ex As Exception
            Debug.WriteLine($"CleanupOldLogs Error: {ex.Message}")
        End Try
    End Sub
End Class