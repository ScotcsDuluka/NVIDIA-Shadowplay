' CommandHandler.vb
' ShadowPlay Engine - TCP Command Handler
' Receives commands from Overlay Client (port 5000) and executes capture actions.
'
' Protocol (same as Overlay's TcpClientHelper):
'   Message format: "[Send] AppName|command:value"
'   Example: "[Send] NVIDIA Overlay|RECORD_START:NVENC_H264,60,50000000,ddagrab"
'
' Engine sends responses back:
'   "[Receive] Engine|STATUS:value"

Imports System.IO
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Json
Imports System.Threading

' ── Hardware device type enum (mirrors CaptureEngine's Private enum) ──
Public Enum HwDeviceType
    None        ' CPU software encoder
    NVIDIA      ' h264_nvenc, hevc_nvenc (accepts d3d11 directly)
    IntelQSV    ' h264_qsv, hevc_qsv  (needs hwmap from d3d11)
    AMD         ' h264_amf, hevc_amf  (accepts d3d11 directly)
End Enum

Public Class CommandHandler
    Implements IDisposable

    ' ── Constants ──────────────────────────────────────────────

    Private Const ENGINE_PORT As Integer = 5001
    Private Const BUFFER_SIZE As Integer = 4096

    ' ── Fields ────────────────────────────────────────────────

    Private _listener As TcpListener
    Private _cts As CancellationTokenSource
    Private _disposed As Boolean = False
    Private _captureEngine As CaptureEngine
    Private _settings As CaptureSettings
    Private _configPath As String

    ' ── Replay buffer state ────────────────────────────────────

    Private _replayProcess As Process
    Private _replayIsRunning As Boolean = False
    Private _replayOutputFile As String = ""
    Private _replayStopwatch As Stopwatch
    Private _replayDuration As Integer = 345 ' seconds, from video.json

    ' ── Events ────────────────────────────────────────────────

    Public Event StatusChanged(state As String)
    Public Event LogMessage(message As String)

    ' ── Constructor ────────────────────────────────────────────

    Public Sub New(settings As CaptureSettings, configPath As String)
        _settings = settings
        _configPath = configPath
    End Sub

    ' ════════════════════════════════════════════════════════════
    ' ── Start / Stop TCP Server ──────────────────────────────
    ' ════════════════════════════════════════════════════════════

    Public Sub StartServer()
        Try
            _cts = New CancellationTokenSource()
            _listener = New TcpListener(System.Net.IPAddress.Loopback, ENGINE_PORT)
            _listener.Start()
            RaiseEvent LogMessage("Engine TCP Server listening on port " & ENGINE_PORT.ToString())

            Task.Run(Sub() AcceptLoop(_cts.Token))
        Catch ex As Exception
            RaiseEvent LogMessage("Failed to start TCP server: " & ex.Message)
        End Try
    End Sub

    Public Sub StopServer()
        Try
            _cts?.Cancel()
            _listener?.Stop()
            RaiseEvent LogMessage("Engine TCP Server stopped")
        Catch
        End Try
    End Sub

    Private Sub AcceptLoop(token As CancellationToken)
        Try
            While Not token.IsCancellationRequested
                Try
                    ' Use Await pattern via Task.Run to avoid AddHandler+ContinueWith
                    Dim clientTask = _listener.AcceptTcpClientAsync()
                    clientTask.Wait(500)

                    If clientTask.IsCompleted AndAlso Not clientTask.IsFaulted Then
                        Dim client As TcpClient = clientTask.Result
                        Task.Run(Sub() HandleClient(client, token))
                    End If
                Catch ex As Exception
                    If token.IsCancellationRequested Then Exit While
                End Try

                ' Small delay to prevent tight loop if listener stops
                Thread.Sleep(100)
            End While
        Catch ex As OperationCanceledException
            ' Normal shutdown
        Catch ex As Exception
            RaiseEvent LogMessage("AcceptLoop error: " & ex.Message)
        End Try
    End Sub

    ' ════════════════════════════════════════════════════════════
    ' ── Client Handler ────────────────────────────────────────
    ' ════════════════════════════════════════════════════════════

    Private Sub HandleClient(client As TcpClient, token As CancellationToken)
        Using client
            Try
                Dim stream As NetworkStream = client.GetStream()
                Dim reader As New StreamReader(stream, Encoding.UTF8)
                Dim writer As New StreamWriter(stream, Encoding.UTF8) With {.AutoFlush = True}

                RaiseEvent LogMessage("Overlay Client connected from " & client.Client.RemoteEndPoint.ToString())

                While Not token.IsCancellationRequested AndAlso client.Connected
                    Dim line As String = Nothing
                    Try
                        ' Use DataAvailable check with timeout
                        If stream.DataAvailable Then
                            line = reader.ReadLine()
                        Else
                            Thread.Sleep(50)
                            Continue While
                        End If
                    Catch ex As IOException
                        Exit While
                    End Try

                    If String.IsNullOrWhiteSpace(line) Then Continue While

                    ' Skip ping/pong system messages
                    If line.Contains("ping") OrElse line.Contains("pong") Then
                        If line.Contains("ping") Then
                            writer.WriteLine("[System]|pong")
                        End If
                        Continue While
                    End If

                    RaiseEvent LogMessage("Received: " & line)

                    ' Parse message: "[Send] AppName|command:value"
                    Dim response As String = ProcessMessage(line)

                    If Not String.IsNullOrEmpty(response) Then
                        writer.WriteLine(response)
                        RaiseEvent LogMessage("Sent: " & response)
                    End If
                End While
            Catch ex As Exception
                RaiseEvent LogMessage("Client disconnected: " & ex.Message)
            End Try
        End Using
    End Sub

    ' ════════════════════════════════════════════════════════════
    ' ── Command Processing ────────────────────────────────────
    ' ════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Parse incoming message and dispatch to appropriate handler.
    ''' Message format: "[Send] AppName|command:value"
    ''' </summary>
    Private Function ProcessMessage(message As String) As String
        Try
            ' Split by "|" to get the command part
            Dim pipeIndex As Integer = message.IndexOf("|"c)
            If pipeIndex < 0 Then Return ""

            Dim data As String = message.Substring(pipeIndex + 1)

            ' Split command:value
            Dim colonIndex As Integer = data.IndexOf(":"c)
            Dim cmd As String
            Dim value As String

            If colonIndex >= 0 Then
                cmd = data.Substring(0, colonIndex)
                value = data.Substring(colonIndex + 1)
            Else
                cmd = data.Trim()
                value = ""
            End If

            Select Case cmd.ToUpper().Trim()

                Case "RECORD_START"
                    Return HandleRecordStart(value)

                Case "RECORD_STOP"
                    Return HandleRecordStop()

                Case "REPLAY_START"
                    Return HandleReplayStart(value)

                Case "REPLAY_STOP"
                    Return HandleReplayStop()

                Case "REPLAY_SAVE"
                    Return HandleReplaySave(value)

                Case "GET_STATUS"
                    Return HandleGetStatus()

                Case "LOAD_CONFIG"
                    Return HandleLoadConfig()

                Case "SET_ENCODER"
                    Return HandleSetEncoder(value)

                Case "HEARTBEAT"
                    Return "[Receive] Engine|HEARTBEAT:ok"

                Case Else
                    Return "[Receive] Engine|ERROR:unknown_command:" & cmd

            End Select
        Catch ex As Exception
            Return "[Receive] Engine|ERROR:" & ex.Message
        End Try
    End Function

    ' ── Record Handlers ────────────────────────────────────────

    ''' <summary>
    ''' Start recording. Value format: "encoder,fps,bitrate,captureMethod"
    ''' Example: "h264_nvenc,60,50000000,ddagrab"
    ''' </summary>
    Private Function HandleRecordStart(value As String) As String
        Try
            ' Update settings from command parameters
            If Not String.IsNullOrWhiteSpace(value) Then
                UpdateSettingsFromCommand(value)
            End If

            ' Ensure output directory exists
            If Not Directory.Exists(_settings.OutputDirectory) Then
                Directory.CreateDirectory(_settings.OutputDirectory)
            End If

            ' Validate
            Dim validation = _settings.Validate()
            If Not validation.Valid Then
                Return "[Receive] Engine|RECORD_ERROR:" & validation.Message
            End If

            ' Create and start engine
            If _captureEngine IsNot Nothing Then
                _captureEngine.Dispose()
                _captureEngine = Nothing
            End If

            _captureEngine = New CaptureEngine(_settings)
            AddHandler _captureEngine.StateChanged, AddressOf OnCaptureStateChanged
            AddHandler _captureEngine.RecordingStarted, AddressOf OnCaptureRecordingStarted
            AddHandler _captureEngine.RecordingStopped, AddressOf OnCaptureRecordingStopped
            AddHandler _captureEngine.ErrorOccurred, AddressOf OnCaptureError

            Dim task As Task(Of Boolean) = _captureEngine.StartRecordingAsync()
            task.Wait()

            If task.Result Then
                RaiseEvent StatusChanged("Recording")
                Return "[Receive] Engine|RECORD_STARTED:" & _captureEngine.OutputFile
            Else
                Return "[Receive] Engine|RECORD_ERROR:Failed to start recording"
            End If
        Catch ex As Exception
            RaiseEvent LogMessage("RecordStart error: " & ex.Message)
            Return "[Receive] Engine|RECORD_ERROR:" & ex.Message
        End Try
    End Function

    Private Function HandleRecordStop() As String
        Try
            If _captureEngine Is Nothing OrElse Not _captureEngine.IsRecording Then
                Return "[Receive] Engine|RECORD_ERROR:Not recording"
            End If

            Dim task As Task(Of Boolean) = _captureEngine.StopRecordingAsync()
            task.Wait()

            RaiseEvent StatusChanged("Idle")
            Return "[Receive] Engine|RECORD_STOPPED:" & _captureEngine.OutputFile
        Catch ex As Exception
            Return "[Receive] Engine|RECORD_ERROR:" & ex.Message
        End Try
    End Function

    ' ── Replay Handlers ────────────────────────────────────────

    ''' <summary>
    ''' Start replay buffer recording.
    ''' Value format: "encoder,fps,bitrate,captureMethod,duration"
    ''' </summary>
    Private Function HandleReplayStart(value As String) As String
        Try
            If _replayIsRunning Then
                Return "[Receive] Engine|REPLAY_ERROR:Already recording replay"
            End If

            ' Update settings from command parameters
            If Not String.IsNullOrWhiteSpace(value) Then
                UpdateSettingsFromCommand(value)
            End If

            ' Ensure output directory exists
            If Not Directory.Exists(_settings.OutputDirectory) Then
                Directory.CreateDirectory(_settings.OutputDirectory)
            End If

            ' Validate
            Dim validation = _settings.Validate()
            If Not validation.Valid Then
                Return "[Receive] Engine|REPLAY_ERROR:" & validation.Message
            End If

            ' Create replay output filename (temp)
            Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
            _replayOutputFile = Path.Combine(_settings.OutputDirectory, "ShadowPlay_Replay_" & timestamp & "." & _settings.FileFormat)

            _replayStopwatch = Stopwatch.StartNew()
            _replayIsRunning = True

            ' Start replay FFmpeg process
            StartReplayProcess()

            RaiseEvent StatusChanged("Replay")
            Return "[Receive] Engine|REPLAY_STARTED:" & _replayOutputFile
        Catch ex As Exception
            RaiseEvent LogMessage("ReplayStart error: " & ex.Message)
            Return "[Receive] Engine|REPLAY_ERROR:" & ex.Message
        End Try
    End Function

    Private Function HandleReplayStop() As String
        Try
            If Not _replayIsRunning Then
                Return "[Receive] Engine|REPLAY_ERROR:Replay not recording"
            End If

            StopReplayProcess()
            _replayIsRunning = False

            ' Delete temp replay file (no save = discard)
            Try
                If File.Exists(_replayOutputFile) Then
                    File.Delete(_replayOutputFile)
                End If
            Catch
            End Try

            RaiseEvent StatusChanged("Idle")
            Return "[Receive] Engine|REPLAY_STOPPED"
        Catch ex As Exception
            Return "[Receive] Engine|REPLAY_ERROR:" & ex.Message
        End Try
    End Function

    ''' <summary>
    ''' Save the current replay buffer.
    ''' Value: optional custom filename
    ''' </summary>
    Private Function HandleReplaySave(value As String) As String
        Try
            If Not _replayIsRunning Then
                Return "[Receive] Engine|REPLAY_SAVE_ERROR:No active replay"
            End If

            ' Determine save filename
            Dim saveFile As String
            If Not String.IsNullOrWhiteSpace(value) Then
                saveFile = Path.Combine(_settings.OutputDirectory, value)
            Else
                Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
                saveFile = Path.Combine(_settings.OutputDirectory, "ShadowPlay_Save_" & timestamp & "." & _settings.FileFormat)
            End If

            ' Stop recording gracefully
            StopReplayProcess()

            ' Rename temp file to save file
            Try
                If File.Exists(_replayOutputFile) AndAlso Not _replayOutputFile.Equals(saveFile, StringComparison.OrdinalIgnoreCase) Then
                    If File.Exists(saveFile) Then File.Delete(saveFile)
                    File.Move(_replayOutputFile, saveFile)
                ElseIf Not File.Exists(_replayOutputFile) Then
                    saveFile = _replayOutputFile
                End If
            Catch
                saveFile = _replayOutputFile
            End Try

            _replayIsRunning = False
            RaiseEvent StatusChanged("Idle")

            Return "[Receive] Engine|REPLAY_SAVED:" & saveFile
        Catch ex As Exception
            Return "[Receive] Engine|REPLAY_SAVE_ERROR:" & ex.Message
        End Try
    End Function

    ' ── Status / Config Handlers ───────────────────────────────

    Private Function HandleGetStatus() As String
        Dim state As String = "Idle"
        If _captureEngine IsNot Nothing AndAlso _captureEngine.IsRecording Then
            state = "Recording"
        ElseIf _replayIsRunning Then
            state = "Replay"
        End If
        Return "[Receive] Engine|STATUS:" & state
    End Function

    Private Function HandleLoadConfig() As String
        Try
            _settings = CaptureSettings.Load(_configPath)
            Return "[Receive] Engine|CONFIG_LOADED"
        Catch ex As Exception
            Return "[Receive] Engine|CONFIG_ERROR:" & ex.Message
        End Try
    End Function

    Private Function HandleSetEncoder(value As String) As String
        Try
            If String.IsNullOrWhiteSpace(value) Then
                Return "[Receive] Engine|ENCODER_ERROR:no_encoder_specified"
            End If
            _settings.Encoder = value.Trim()
            _settings.Save(_configPath)
            Return "[Receive] Engine|ENCODER_SET:" & _settings.Encoder
        Catch ex As Exception
            Return "[Receive] Engine|ENCODER_ERROR:" & ex.Message
        End Try
    End Function

    ' ════════════════════════════════════════════════════════════
    ' ── Settings Parser ───────────────────────────────────────
    ' ════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Parse comma-separated settings from command value.
    ''' Format: "encoder,fps,bitrate,captureMethod[,duration]"
    ''' </summary>
    Private Sub UpdateSettingsFromCommand(value As String)
        Try
            Dim parts As String() = value.Split(","c)
            If parts.Length >= 1 AndAlso Not String.IsNullOrWhiteSpace(parts(0)) Then
                _settings.Encoder = parts(0).Trim()
            End If
            If parts.Length >= 2 Then
                Dim fps As Integer
                If Integer.TryParse(parts(2).Trim(), fps) Then
                    _settings.FPS = Math.Max(1, Math.Min(240, fps))
                End If
            End If
            If parts.Length >= 3 Then
                Dim bitrate As Long
                If Long.TryParse(parts(3).Trim(), bitrate) Then
                    _settings.Bitrate = Math.Max(1000000, Math.Min(200000000, bitrate))
                End If
            End If
            If parts.Length >= 4 Then
                _settings.CaptureMethod = parts(3).Trim().ToLower()
                Dim validMethods As String() = {"ddagrab", "gdigrab", "gfxcapture", "null"}
                Dim isValid As Boolean = False
                For Each m In validMethods
                    If _settings.CaptureMethod = m Then
                        isValid = True
                        Exit For
                    End If
                Next
                If _settings.CaptureMethod = "null" Then
                    _settings.CaptureMethod = "ddagrab"
                ElseIf Not isValid Then
                    _settings.CaptureMethod = "ddagrab"
                End If
            End If
            If parts.Length >= 5 Then
                Dim dur As Integer
                If Integer.TryParse(parts(4).Trim(), dur) Then
                    _replayDuration = Math.Max(10, Math.Min(3600, dur))
                End If
            End If
        Catch ex As Exception
            RaiseEvent LogMessage("Settings parse warning: " & ex.Message)
        End Try
    End Sub

    ' ════════════════════════════════════════════════════════════
    ' ── Replay Process Management ─────────────────────────────
    ' ════════════════════════════════════════════════════════════

    Private Sub StartReplayProcess()
        Try
            Dim args As String = BuildReplayArguments(_replayOutputFile)

            RaiseEvent LogMessage("Replay FFmpeg: " & _settings.FFmpegPath & " " & args)

            Dim si As New ProcessStartInfo()
            si.FileName = _settings.FFmpegPath
            si.Arguments = args
            si.UseShellExecute = False
            si.RedirectStandardOutput = True
            si.RedirectStandardError = True
            si.RedirectStandardInput = True
            si.CreateNoWindow = True

            _replayProcess = New Process()
            _replayProcess.StartInfo = si
            AddHandler _replayProcess.ErrorDataReceived, AddressOf OnReplayStdErr
            AddHandler _replayProcess.Exited, AddressOf OnReplayExited

            If Not _replayProcess.Start() Then
                Throw New Exception("Failed to start replay FFmpeg process")
            End If

            _replayProcess.BeginErrorReadLine()
            _replayProcess.BeginOutputReadLine()
        Catch ex As Exception
            _replayIsRunning = False
            Throw
        End Try
    End Sub

    Private Function BuildReplayArguments(outputFile As String) As String
        Dim sb As New StringBuilder()
        Dim fpsStr As String = _settings.FPS.ToString()
        Dim br As String = _settings.Bitrate.ToString()
        Dim buf As String = (_settings.Bitrate * 2).ToString()
        Dim hwType As HwDeviceType = DetectHwDeviceType(_settings.Encoder)

        sb.Append("-hide_banner -loglevel info ")

        ' Video input by capture method
        Dim videoFilter As String = ""

        Select Case _settings.CaptureMethod.ToLower()
            Case "ddagrab"
                sb.Append("-f lavfi -i ""ddagrab=output_idx=0:framerate=" & fpsStr & """ ")
                Select Case hwType
                    Case HwDeviceType.IntelQSV
                        videoFilter = "hwmap=derive_device=qsv"
                    Case HwDeviceType.None
                        videoFilter = "hwdownload,format=bgra,format=yuv420p"
                    Case HwDeviceType.NVIDIA, HwDeviceType.AMD
                        videoFilter = ""
                End Select

            Case "gdigrab"
                sb.Append("-f gdigrab -framerate " & fpsStr & " -i desktop ")
                videoFilter = ""

            Case "gfxcapture"
                sb.Append("-f lavfi -i ""gfxcapture=monitor_idx=0:max_framerate=" & fpsStr & """ ")
                Select Case hwType
                    Case HwDeviceType.IntelQSV
                        videoFilter = "fps=" & fpsStr & ",hwmap=derive_device=qsv"
                    Case HwDeviceType.None
                        videoFilter = "fps=" & fpsStr & ",hwdownload,format=bgra,format=yuv420p"
                    Case HwDeviceType.NVIDIA, HwDeviceType.AMD
                        videoFilter = "fps=" & fpsStr
                End Select
        End Select

        ' Audio input
        If _settings.AudioCapture Then
            sb.Append("-f dshow -i audio=""" & _settings.AudioDevice & """ ")
        End If

        ' Video filter chain
        If videoFilter.Length > 0 Then
            sb.Append("-vf """ & videoFilter & """ ")
        End If

        ' Video encoder
        sb.Append("-c:v " & _settings.Encoder & " ")

        Select Case hwType
            Case HwDeviceType.NVIDIA
                sb.Append("-preset p4 -tune ll ")
                sb.Append("-b:v " & br & " -rc cbr -bufsize " & buf & " ")
                sb.Append("-zerolatency 1 -spatial-aq 1 -temporal-aq 1 ")
            Case HwDeviceType.IntelQSV
                sb.Append("-preset medium ")
                sb.Append("-b:v " & br & " -rc cbr -bufsize " & buf & " ")
                sb.Append("-look_ahead 1 ")
            Case HwDeviceType.AMD
                sb.Append("-preset balanced -usage transcoding ")
                sb.Append("-b:v " & br & " -rc cbr -bufsize " & buf & " ")
            Case HwDeviceType.None
                If _settings.Encoder.IndexOf("libx265", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-preset medium ")
                    sb.Append("-b:v " & br & " -crf 23 -pix_fmt yuv420p10le ")
                ElseIf _settings.Encoder.IndexOf("libx264", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-preset ultrafast -tune zerolatency ")
                    sb.Append("-b:v " & br & " -crf 18 -pix_fmt yuv420p ")
                ElseIf _settings.Encoder.IndexOf("svtav1", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-preset 6 ")
                    sb.Append("-b:v " & br & " -crf 35 -pix_fmt yuv420p ")
                Else
                    sb.Append("-b:v " & br & " ")
                End If
        End Select

        ' Audio encoding
        If _settings.AudioCapture Then
            sb.Append("-c:a aac -b:a 320k -ar 48000 ")
        End If

        sb.Append("-y """ & outputFile & """")
        Return sb.ToString()
    End Function

    Private Sub StopReplayProcess()
        Try
            If _replayProcess IsNot Nothing AndAlso Not _replayProcess.HasExited Then
                Try
                    _replayProcess.StandardInput.Write("q" & vbLf)
                    _replayProcess.StandardInput.Flush()
                Catch
                End Try

                If Not _replayProcess.WaitForExit(10000) Then
                    _replayProcess.Kill()
                End If
            End If
        Catch
        Finally
            _replayStopwatch?.Stop()
            If _replayProcess IsNot Nothing Then
                _replayProcess.Dispose()
                _replayProcess = Nothing
            End If
        End Try
    End Sub

    Private Sub OnReplayStdErr(sender As Object, e As DataReceivedEventArgs)
        If e.Data Is Nothing Then Return
        RaiseEvent LogMessage("[Replay stderr] " & e.Data)
    End Sub

    Private Sub OnReplayExited(sender As Object, e As EventArgs)
        If _replayIsRunning Then
            _replayIsRunning = False
            _replayStopwatch?.Stop()
            RaiseEvent StatusChanged("Idle")
            RaiseEvent LogMessage("Replay process exited")
        End If
    End Sub

    ' ── Capture Engine Events ──────────────────────────────────

    Private Sub OnCaptureStateChanged(state As CaptureEngine.CaptureState)
        RaiseEvent LogMessage("Capture state: " & state.ToString())
    End Sub

    Private Sub OnCaptureRecordingStarted(filename As String)
        RaiseEvent LogMessage("Capture started: " & filename)
    End Sub

    Private Sub OnCaptureRecordingStopped(filename As String)
        RaiseEvent LogMessage("Capture stopped: " & filename)
    End Sub

    Private Sub OnCaptureError(message As String)
        RaiseEvent LogMessage("Capture error: " & message)
    End Sub

    ' ── Hardware Detection Helper ──────────────────────────────

    ''' <summary>
    ''' Detect hardware device type from encoder string.
    ''' Mirrors CaptureEngine.DetectHwDeviceType logic.
    ''' </summary>
    Private Function DetectHwDeviceType(encoderId As String) As HwDeviceType
        If encoderId.IndexOf("nvenc", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return HwDeviceType.NVIDIA
        End If
        If encoderId.IndexOf("qsv", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return HwDeviceType.IntelQSV
        End If
        If encoderId.IndexOf("amf", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return HwDeviceType.AMD
        End If
        Return HwDeviceType.None
    End Function

    ' ── Dispose ────────────────────────────────────────────────

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                StopServer()
                If _captureEngine IsNot Nothing Then
                    _captureEngine.Dispose()
                    _captureEngine = Nothing
                End If
                StopReplayProcess()
            End If
            _disposed = True
        End If
    End Sub

End Class
