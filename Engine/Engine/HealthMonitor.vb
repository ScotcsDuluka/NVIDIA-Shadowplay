' ═══════════════════════════════════════════════════════════════════════
' 📁 Classes/HealthMonitor.vb
' ═══════════════════════════════════════════════════════════════════════
Imports System.Diagnostics
Imports System.IO
Imports Captrue_Core.CaptureCore

Public Class HealthMonitor
    Private Shared _timer As Windows.Forms.Timer
    Private Shared _isRunning As Boolean = False
    Private Shared _consecutiveFailures As Integer = 0
    Private Shared _maxFailures As Integer = 3
    Private Shared _recorderRef As WeakReference(Of ScreenRecorder)

    Public Event OnHealthCheckFailed(message As String)
    Public Event OnProcessCrashed(processType As String)

    Public Shared Sub StartMonitoring(recorder As ScreenRecorder)
        If _isRunning Then Return

        _recorderRef = New WeakReference(Of ScreenRecorder)(recorder)

        _timer = New Windows.Forms.Timer()
        _timer.Interval = 5000 ' Check every 5 seconds
        AddHandler _timer.Tick, AddressOf PerformHealthCheck
        _timer.Start()
        _isRunning = True

        StableLogger.Log(StableLogger.LogLevel.INFO, "Health Monitor started")
    End Sub

    Public Shared Sub [Stop]()
        If _timer IsNot Nothing Then
            _timer.Stop()
            _isRunning = False
        End If

        StableLogger.Log(StableLogger.LogLevel.INFO, "Health Monitor stopped")
    End Sub

    Private Shared Sub PerformHealthCheck(sender As Object, e As EventArgs)
        Try
            Dim recorder As ScreenRecorder = Nothing
            If Not _recorderRef.TryGetTarget(recorder) OrElse recorder Is Nothing Then
                [Stop]()
                Return
            End If

            ' Check Recording Process
            If recorder.IsRecording Then
                CheckRecordingHealth(recorder)
            End If

            ' Check Buffer Process
            If recorder.IsBuffering Then
                CheckBufferHealth(recorder)
            End If

            ' Reset failure counter on success
            _consecutiveFailures = 0

        Catch ex As Exception
            _consecutiveFailures += 1
            StableLogger.Log(StableLogger.LogLevel.WARNING,
                          $"Health check failed ({_consecutiveFailures}/{_maxFailures}): {ex.Message}")

            If _consecutiveFailures >= _maxFailures Then
                StableLogger.Log(StableLogger.LogLevel.FATAL,
                              "Multiple health checks failed - system unstable!")
                RaiseEvent OnHealthCheckFailed("System health critical - consider restarting")
            End If
        End Try
    End Sub

    Private Shared Sub CheckRecordingHealth(recorder As ScreenRecorder)
        Try
            ' TODO: You need to expose recordingProcess as ReadOnly or add a method
            ' For now, we'll use reflection or you can add a public property

            ' Check if recording duration is reasonable
            Dim duration As TimeSpan = recorder.RecordingDuration
            If duration.TotalHours > 24 Then
                StableLogger.Log(StableLogger.LogLevel.WARNING,
                              $"Very long recording: {duration.TotalHours:F1} hours")
            End If

            ' Check disk space periodically (every minute)
            If DateTime.Now.Second Mod 60 = 0 Then
                CheckDiskSpace()
            End If

        Catch ex As Exception
            StableLogger.LogException(ex, "CheckRecordingHealth")
        End Try
    End Sub

    Private Shared Sub CheckBufferHealth(recorder As ScreenRecorder)
        Try
            Dim bufferDuration As Double = recorder.BufferCurrentDuration

            If bufferDuration <= 0 AndAlso recorder.IsBuffering Then
                StableLogger.Log(StableLogger.LogLevel.WARNING,
                              "Buffer active but no segments detected!")
            End If

            ' Check if buffer is growing
            Static lastDuration As Double = 0
            If Math.Abs(bufferDuration - lastDuration) < 0.001 AndAlso bufferDuration > 10 Then
                ' Buffer not growing for 5+ seconds after initial start
                StableLogger.Log(StableLogger.LogLevel.WARNING,
                              $"Buffer stalled at {bufferDuration:F1}s")
            End If
            lastDuration = bufferDuration

        Catch ex As Exception
            StableLogger.LogException(ex, "CheckBufferHealth")
        End Try
    End Sub

    Private Shared Sub CheckDiskSpace()
        Try
            Dim drives As DriveInfo() = DriveInfo.GetDrives()

            For Each drive In drives
                If drive.IsReady AndAlso drive.DriveType = DriveType.Fixed Then
                    Dim freeSpaceGB As Double = drive.AvailableFreeSpace / 1GB
                    
                    If freeSpaceGB < 1.0 Then ' Less than 1 GB
                        StableLogger.Log(StableLogger.LogLevel.ERROR,
                                      $"Low disk space on {drive.Name}: {freeSpaceGB:F2} GB free")

                        RaiseEvent OnHealthCheckFailed($"Low disk space: {freeSpaceGB:F2} GB on {drive.Name}")
                    ElseIf freeSpaceGB < 5.0 Then ' Less than 5 GB
                        StableLogger.Log(StableLogger.LogLevel.WARNING,
                                      $"Disk space getting low on {drive.Name}: {freeSpaceGB:F2} GB free")
                    End If
                End If
            Next

        Catch ex As Exception
            StableLogger.LogException(ex, "CheckDiskSpace")
        End Try
    End Sub
End Class