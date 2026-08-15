' CaptureEngine.vb
' ShadowPlay Engine - FFmpeg Screen Capture Engine
' Manages FFmpeg process: Start/Stop/ForceStop
' Builds FFmpeg args based on CaptureSettings
' Supports: ddagrab, gdigrab, gfxcapture
' Encoders: NVENC, QSV, AMF, libx264, libx265, SVT-AV1
'
' ── FFmpeg Official Syntax ──
'
' ddagrab (Desktop Duplication via DXGI):
'   ffmpeg -f lavfi -i "ddagrab=output_idx=0:framerate=60" -c:v h264_nvenc out.mp4
'   Output: D3D11 hardware frames
'   -> NVENC/AMF: pass directly (d3d11 -> d3d11)
'   -> QSV: add -vf "hwmap=derive_device=qsv"
'   -> CPU: add -vf "hwdownload,format=bgra,format=yuv420p"
'
' gfxcapture (Windows.Graphics.Capture):
'   ffmpeg -f lavfi -i "gfxcapture=monitor_idx=0:max_framerate=60" -vf "fps=60" ...
'   Output: D3D11 hardware frames (VFR, needs fps filter)
'
' gdigrab (GDI Legacy Screen Capture):
'   ffmpeg -f gdigrab -framerate 60 -i desktop -c:v libx264 out.mp4
'   Output: System memory frames (BGRA)
'
' Audio capture:
'   -f dshow -i audio="Device Name"
'
' IMPORTANT: ddagrab and gfxcapture use -f lavfi -i (NOT -filter_complex alone)
' because -filter_complex without -i causes stream mapping issues with
' multiple inputs (audio from dshow). Use -vf for post-filters instead.

Imports System.Diagnostics
Imports System.IO
Imports System.IO.Pipes
Imports System.Text

Partial Public Class CaptureEngine
    Implements IDisposable

    Public Event StateChanged As Action(Of CaptureState)
    Public Event RecordingStarted As Action(Of String)
    Public Event RecordingStopped As Action(Of String)
    Public Event ErrorOccurred As Action(Of String)
    Public Event FrameCaptured As Action(Of Long)

    Public Event ProgressUpdated As Action(Of Long, TimeSpan, Long)

    Public Enum CaptureState
        Idle
        Detecting
        Recording
        Paused
        Stopping
        HasError
    End Enum

    Private Enum HwDeviceType
        None
        NVIDIA
        IntelQSV
        AMD
    End Enum

    Private _settings As CaptureSettings
    Private _ffmpegProcess As Process
    Private _state As CaptureState = CaptureState.Idle
    Private _outputFile As String = ""
    Private _stopwatch As Stopwatch
    Private _logBuffer As StringBuilder
    Private _disposed As Boolean = False
    Private _jobGuard As JobObjectGuard

    Private _audioEngine As NAudioCaptureEngine
    Private _audioPipeStream As Stream
    Private _micNamedPipe As NamedPipeServerStream
    Private _micNamedPipeStream As Stream
    Private _micPipePath As String
    Private Const MicPipePrefix As String = "nvidia_shadowplay_mic_"

    ' System audio named pipe — replaces pipe:0 (stdin) so stdin is free for
    ' FFmpeg's interactive 'q' command at shutdown.
    Private _sysNamedPipe As NamedPipeServerStream
    Private _sysPipePath As String
    Private Const AudioPipePrefix As String = "nvidia_shadowplay_audio_"

    ''' <summary>
    ''' Atomic flag (0=not stopped, 1=stop completed) — ensures RecordingStopped
    ''' fires EXACTLY once across OnExited and StopRecordingAsync. Set to 1 when
    ''' either of them fires the event, the other one sees non-zero return from
    ''' Interlocked.Exchange and skips.
    ''' </summary>
    Private _stopCompleted As Integer = 0

    ' ── Properties ────────────────────────────────────────────

    Public ReadOnly Property State As CaptureState
        Get
            Return _state
        End Get
    End Property

    Public ReadOnly Property IsRecording As Boolean
        Get
            Return _state = CaptureState.Recording
        End Get
    End Property

    Public ReadOnly Property OutputFile As String
        Get
            Return _outputFile
        End Get
    End Property

    Public ReadOnly Property RecordingDuration As TimeSpan
        Get
            If _stopwatch Is Nothing OrElse Not _stopwatch.IsRunning Then
                Return TimeSpan.Zero
            End If
            Return _stopwatch.Elapsed
        End Get
    End Property

    ' ── Constructor ───────────────────────────────────────────

    Public Sub New(settings As CaptureSettings)
        _settings = settings
        _logBuffer = New StringBuilder()
        Try
            _jobGuard = New JobObjectGuard()
        Catch ex As Exception
            ' Best-effort: if job creation fails (e.g. on Wine/older Windows),
            ' capture still works — we just lose orphan protection.
            LogDebug("JobObjectGuard init failed: " & ex.Message)
        End Try
    End Sub

    ' ── Start Recording ───────────────────────────────────────

    Public Async Function StartRecordingAsync(Optional overrideOutputPath As String = Nothing) As Task(Of Boolean)
        If _state <> CaptureState.Idle Then
            RaiseEvent ErrorOccurred("Cannot start: engine is not idle")
            Return False
        End If

        Dim validation As CaptureSettings.ValidationResult = _settings.Validate()
        If Not validation.Valid Then
            RaiseEvent ErrorOccurred(validation.Message)
            Return False
        End If

        If Not Directory.Exists(_settings.OutputDirectory) Then
            Directory.CreateDirectory(_settings.OutputDirectory)
        End If

        ' ✅ P1.5: if the caller (Overlay via Hub) supplied a specific output path,
        ' honor it. Otherwise fall back to settings.GenerateOutputFilename() which
        ' uses settings.OutputDirectory + timestamp. Old behavior always used the
        ' settings path, which meant files ended up in Engine's preferred folder
        ' instead of where the Overlay told the user they'd be saved.
        If Not String.IsNullOrEmpty(overrideOutputPath) Then
            ' Basic sanity: must end with a known video extension.
            Dim ext As String = Path.GetExtension(overrideOutputPath).ToLowerInvariant()
            If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".mkv" OrElse ext = ".avi" OrElse ext = ".m4v" Then
                _outputFile = overrideOutputPath
                ' Make sure parent dir exists.
                Dim parentDir As String = Path.GetDirectoryName(overrideOutputPath)
                If Not String.IsNullOrEmpty(parentDir) AndAlso Not Directory.Exists(parentDir) Then
                    Try
                        Directory.CreateDirectory(parentDir)
                    Catch ex As Exception
                        RaiseEvent ErrorOccurred("Cannot create output dir: " & ex.Message)
                        Return False
                    End Try
                End If
            Else
                RaiseEvent ErrorOccurred("Unsupported output extension: " & ext)
                Return False
            End If
        Else
            _outputFile = _settings.GenerateOutputFilename()
        End If

        ' ✅ C5 FIX: do NOT set Recording state until _ffmpegProcess is actually
        ' assigned inside the Task. Old code called SetState(Recording) here,
        ' before the Task even started — if StopRecordingAsync arrived in the
        ' window between SetState and the Task assigning _ffmpegProcess, Stop
        ' would see _state=Recording but _ffmpegProcess=Nothing → skip FFmpeg
        ' cleanup, set state back to Idle, raise RecordingStopped. The Task
        ' would then launch FFmpeg with no one tracking it (orphan).
        ' Now we set Recording state only AFTER _ffmpegProcess is alive.

        Return Await Task.Run(Function()
                                 Try
                                     ' ─── Pre-create audio named pipes BEFORE BuildFFmpegArguments ───
                                     ' Using named pipes instead of pipe:0 (stdin) frees up stdin
                                     ' for FFmpeg's interactive 'q' command at shutdown.
                                     Dim hasNAudio As Boolean = (_settings.SystemAudioCapture OrElse _settings.MicCapture)
                                     If hasNAudio Then
                                         _sysPipePath = "\\.\pipe\" & AudioPipePrefix & Process.GetCurrentProcess().Id.ToString() & "_" & Guid.NewGuid().ToString("N").Substring(0, 8)
                                         _sysNamedPipe = New NamedPipeServerStream(_sysPipePath.Substring(8), PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 64 * 1024, 64 * 1024)
                                         Task.Run(Sub()
                                                      Try : _sysNamedPipe.WaitForConnection() : Catch : End Try
                                                  End Sub)
                                         LogDebug("[Audio] System pipe created: " & _sysPipePath)
                                         WriteDebugLog("[Audio] System pipe created: " & _sysPipePath)

                                         ' Mic pipe for Separate mode
                                         Dim isSeparateAndMic As Boolean = (_settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack) AndAlso
                                                                           _settings.MicCapture AndAlso
                                                                           (Not String.IsNullOrEmpty(_settings.MicDeviceId) OrElse
                                                                            Not String.IsNullOrEmpty(_settings.MicDeviceName))
                                         If isSeparateAndMic Then
                                             _micPipePath = "\\.\pipe\" & MicPipePrefix & Process.GetCurrentProcess().Id.ToString() & "_" & Guid.NewGuid().ToString("N").Substring(0, 8)
                                             _micNamedPipe = New NamedPipeServerStream(_micPipePath.Substring(8), PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 64 * 1024, 64 * 1024)
                                             Task.Run(Sub()
                                                          Try : _micNamedPipe.WaitForConnection() : Catch : End Try
                                                      End Sub)
                                             LogDebug("[Audio] Mic pipe created: " & _micPipePath)
                                             WriteDebugLog("[Audio] Mic pipe created: " & _micPipePath)
                                         End If
                                     End If

                                     Dim args As String = BuildFFmpegArguments(_outputFile)
                                     LogDebug("FFmpeg command: " & _settings.FFmpegPath & " " & args)
                                     WriteDebugLog("FFmpeg command: " & _settings.FFmpegPath & " " & args)

                                     Dim si As New ProcessStartInfo()
                                     si.FileName = _settings.FFmpegPath
                                     si.Arguments = args
                                     si.UseShellExecute = False
                                     si.RedirectStandardOutput = True
                                     si.RedirectStandardError = True
                                     si.RedirectStandardInput = True
                                     si.CreateNoWindow = True

                                     _ffmpegProcess = New Process()
                                     _ffmpegProcess.StartInfo = si
                                     ' ✅ FIX: Must set EnableRaisingEvents=True BEFORE Start() or the Exited event never fires.
                                     ' Without this, a crashed FFmpeg looks "still recording" forever.
                                     _ffmpegProcess.EnableRaisingEvents = True
                                     AddHandler _ffmpegProcess.OutputDataReceived, AddressOf OnStdOut
                                     AddHandler _ffmpegProcess.ErrorDataReceived, AddressOf OnStdErr
                                     AddHandler _ffmpegProcess.Exited, AddressOf OnExited

                                     If Not _ffmpegProcess.Start() Then
                                         SetState(CaptureState.HasError)
                                         RaiseEvent ErrorOccurred("Failed to start FFmpeg process")
                                         Return False
                                     End If

                                     LogDebug($"[FFmpeg] Started PID={_ffmpegProcess.Id}")

                                     ' ✅ P1: tie the FFmpeg child to our Job Object so it dies with us.
                                     If _jobGuard IsNot Nothing Then
                                         _jobGuard.Assign(_ffmpegProcess)
                                     End If

                                     _ffmpegProcess.BeginOutputReadLine()
                                     _ffmpegProcess.BeginErrorReadLine()
                                     _stopwatch = Stopwatch.StartNew()
                                     ' Reset stop flag — fresh recording session, no Stop has fired yet.
                                     System.Threading.Interlocked.Exchange(_stopCompleted, 0)
                                     ' ✅ C5 FIX: now that _ffmpegProcess is alive, set Recording state.
                                     ' Any Stop arriving after this point will correctly see the process.
                                     SetState(CaptureState.Recording)
                                     RaiseEvent RecordingStarted(_outputFile)

                                     StartAudioCaptureIfNeeded()
                                     Return True

                                 Catch ex As Exception
                                     SetState(CaptureState.HasError)
                                     RaiseEvent ErrorOccurred("Start failed: " & ex.Message)
                                     LogDebug("Exception: " & ex.ToString())
                                     WriteDebugLog("Start exception: " & ex.ToString())
                                     Return False
                                 End Try
                             End Function)
    End Function

    ' ── Stop Recording ────────────────────────────────────────

    Public Async Function StopRecordingAsync() As Task(Of Boolean)
        If _state <> CaptureState.Recording Then
            Return False
        End If

        SetState(CaptureState.Stopping)
        _stopwatch?.Stop()

        Return Await Task.Run(Function()
                                 Try
                                     Dim stopMsg As String = "[FFmpeg] Stop requested. State=" & _state.ToString()
                                     LogDebug(stopMsg)
                                     WriteDebugLog(stopMsg)

                                     If _ffmpegProcess IsNot Nothing Then
                                         Dim beforeMsg As String = $"[FFmpeg] BeforeStop HasExited={_ffmpegProcess.HasExited.ToString()}"
                                         LogDebug(beforeMsg)
                                         WriteDebugLog(beforeMsg)
                                     End If

                                     ' ─── SHUTDOWN ORDER (named pipe approach) ───
                                     ' Audio now uses a named pipe (NOT stdin). stdin is free for
                                     ' FFmpeg's interactive 'q' command.
                                     '
                                     ' 1. Stop audio producers (WASAPI stops, no new PCM)
                                     ' 2. ClosePipes: drain queue + close named pipe (audio EOF)
                                     ' 3. Send 'q' through StandardInput (stdin is free!)
                                     ' 4. WaitForExit → FFmpeg stops ddagrab, finalizes MP4, exits 0
                                     ' 5. Kill only as absolute last resort

                                     If _audioEngine IsNot Nothing Then
                                         Dim prodMsg As String = "[Audio] Stopping producers…"
                                         LogDebug(prodMsg)
                                         WriteDebugLog(prodMsg)
                                         _audioEngine.StopProducers()
                                         Dim prodStoppedMsg As String = "[Audio] Producers stopped. Draining queues…"
                                         LogDebug(prodStoppedMsg)
                                         WriteDebugLog(prodStoppedMsg)
                                         _audioEngine.ClosePipes()
                                         Dim pipesClosedMsg As String = "[Audio] Pipes closed (audio named pipe EOF sent)."
                                         LogDebug(pipesClosedMsg)
                                         WriteDebugLog(pipesClosedMsg)

                                         Dim diagMsg As String = _audioEngine.GetDiagnostics()
                                         LogDebug(diagMsg)
                                         WriteDebugLog(diagMsg)

                                         _audioEngine.Dispose()
                                         _audioEngine = Nothing
                                     End If

                                     ' Send 'q' through StandardInput — stdin is NOT used for
                                     ' audio anymore (audio goes through named pipe), so this
                                     ' tells FFmpeg to stop all inputs (including ddagrab) and
                                     ' finalize the MP4.
                                     If _ffmpegProcess IsNot Nothing AndAlso Not _ffmpegProcess.HasExited Then
                                         Dim qMsg As String = $"[FFmpeg] Sending quit command (q)… PID={_ffmpegProcess.Id}"
                                         LogDebug(qMsg)
                                         WriteDebugLog(qMsg)
                                         Try
                                             _ffmpegProcess.StandardInput.Write("q" & vbLf)
                                             _ffmpegProcess.StandardInput.Flush()
                                         Catch ex As Exception
                                             Dim qErrMsg As String = "[FFmpeg] Failed to send q: " & ex.Message
                                             LogDebug(qErrMsg)
                                             WriteDebugLog(qErrMsg)
                                         End Try

                                         Dim waitMsg As String = $"[FFmpeg] WaitForExit(10000) after q… PID={_ffmpegProcess.Id}"
                                         LogDebug(waitMsg)
                                         WriteDebugLog(waitMsg)
                                         Dim exited As Boolean = _ffmpegProcess.WaitForExit(10000)
                                         Dim waitRetMsg As String = $"[FFmpeg] WaitForExit returned. HasExited={_ffmpegProcess.HasExited.ToString()}"
                                         LogDebug(waitRetMsg)
                                         WriteDebugLog(waitRetMsg)

                                         If Not _ffmpegProcess.HasExited Then
                                             Dim killMsg As String = $"[FFmpeg] WaitForExit TIMEOUT → KILL PID={_ffmpegProcess.Id}"
                                             LogDebug(killMsg)
                                             WriteDebugLog(killMsg)
                                             Try
                                                 _ffmpegProcess.Kill()
                                                 _ffmpegProcess.WaitForExit(2000)
                                                 Dim killDoneMsg As String = $"[FFmpeg] Kill completed. ExitCode={_ffmpegProcess.ExitCode.ToString()}"
                                                 LogDebug(killDoneMsg)
                                                 WriteDebugLog(killDoneMsg)
                                             Catch ex As Exception
                                                 Dim killFailMsg As String = "[FFmpeg] Kill failed: " & ex.Message
                                                 LogDebug(killFailMsg)
                                                 WriteDebugLog(killFailMsg)
                                             End Try
                                         End If
                                     Else
                                         Dim alreadyMsg As String = "[FFmpeg] Process already exited."
                                         LogDebug(alreadyMsg)
                                         WriteDebugLog(alreadyMsg)
                                     End If

                                     ' Use Interlocked.Exchange to ensure RecordingStopped fires exactly once
                                     ' — OnExited may have already fired during WaitForExit, in which case
                                     ' skip this branch entirely.
                                     If System.Threading.Interlocked.Exchange(_stopCompleted, 1) = 0 Then
                                         SetState(CaptureState.Idle)
                                         RaiseEvent RecordingStopped(_outputFile)
                                         LogDebug("Recording saved: " & _outputFile)
                                     End If
                                     Return True

                                 Catch ex As Exception
                                     SetState(CaptureState.HasError)
                                     RaiseEvent ErrorOccurred("Stop failed: " & ex.Message)
                                     Return False
                                 End Try
                             End Function)
    End Function

    ' ── Force Stop ────────────────────────────────────────────

    Public Sub ForceStop()
        Try
            LogDebug("[FFmpeg] FORCE STOP — stopping audio capture…")
            StopAudioCaptureIfNeeded()

            If _ffmpegProcess IsNot Nothing AndAlso Not _ffmpegProcess.HasExited Then
                LogDebug("[FFmpeg] FORCE KILL")
                _ffmpegProcess.Kill()
                _ffmpegProcess.WaitForExit(5000)
            End If
        Catch
        Finally
            If _ffmpegProcess IsNot Nothing Then
                _ffmpegProcess.Dispose()
                _ffmpegProcess = Nothing
            End If
            If _stopwatch IsNot Nothing Then
                _stopwatch.Stop()
            End If
            SetState(CaptureState.Idle)
        End Try
    End Sub

    ' ── FFmpeg Process Events ─────────────────────────────────

    Private Sub OnStdOut(sender As Object, e As DataReceivedEventArgs)
        If e.Data IsNot Nothing Then
            LogDebug("[stdout] " & e.Data)
        End If
    End Sub

    Private Sub OnStdErr(sender As Object, e As DataReceivedEventArgs)
        If e.Data Is Nothing Then Return

        LogDebug("[stderr] " & e.Data)
        WriteDebugLog("[stderr] " & e.Data)

        ' Parse frame progress: "frame=  120 fps=60 ..."
        If e.Data.IndexOf("frame=", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Try
                Dim idx As Integer = e.Data.IndexOf("frame=") + 6
                If idx < e.Data.Length Then
                    Dim remaining As String = e.Data.Substring(idx).TrimStart()
                    Dim spaceIdx As Integer = remaining.IndexOf(" "c)
                    Dim frameStr As String = ""
                    If spaceIdx > 0 Then
                        frameStr = remaining.Substring(0, spaceIdx).Trim()
                    Else
                        frameStr = remaining.Trim()
                    End If
                    Dim frameNum As Long = 0
                    If Long.TryParse(frameStr, frameNum) Then
                        RaiseEvent FrameCaptured(frameNum)

                        ' ✅ P2.9: also parse "time=00:00:42.50" and "size=    8420KiB"
                        ' and fire ProgressUpdated. Overlay uses this to show real-time
                        ' recording timer + file size in its UI.
                        Dim duration As TimeSpan = TimeSpan.Zero
                        Dim sizeBytes As Long = 0

                        ' Parse time=HH:MM:SS.mmm
                        Dim timeIdx As Integer = e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase)
                        If timeIdx >= 0 Then
                            Dim timeStr As String = e.Data.Substring(timeIdx + 5).TrimStart()
                            Dim timeEnd As Integer = timeStr.IndexOf(" "c)
                            If timeEnd > 0 Then timeStr = timeStr.Substring(0, timeEnd)
                            Dim parsed As TimeSpan
                            ' ✅ C3 FIX: use InvariantCulture so the parser works on
                            ' machines with non-English locales (de-DE uses ',' as
                            ' decimal separator, which breaks Double.TryParse for the
                            ' millisecond portion).
                            If TimeSpan.TryParse(timeStr, Globalization.CultureInfo.InvariantCulture, parsed) Then
                                duration = parsed
                            End If
                        End If

                        ' Parse size=NNNNKiB or NNNNMiB
                        Dim sizeIdx As Integer = e.Data.IndexOf("size=", StringComparison.OrdinalIgnoreCase)
                        If sizeIdx >= 0 Then
                            Dim sizeStr As String = e.Data.Substring(sizeIdx + 5).TrimStart()
                            Dim sizeEnd As Integer = sizeStr.IndexOf(" "c)
                            If sizeEnd > 0 Then sizeStr = sizeStr.Substring(0, sizeEnd)
                            ' Strip trailing unit (kB, KiB, MB, MiB, etc.)
                            Dim unitStart As Integer = -1
                            For i As Integer = 0 To sizeStr.Length - 1
                                If Not (Char.IsDigit(sizeStr(i)) OrElse sizeStr(i) = "."c) Then
                                    unitStart = i
                                    Exit For
                                End If
                            Next
                            Dim numStr As String = If(unitStart >= 0, sizeStr.Substring(0, unitStart), sizeStr)
                            Dim unitStr As String = If(unitStart >= 0, sizeStr.Substring(unitStart).Trim(), "")
                            Dim sizeNum As Double = 0
                            ' ✅ C3 FIX: InvariantCulture for the same reason.
                            If Double.TryParse(numStr, Globalization.CultureInfo.InvariantCulture, sizeNum) Then
                                Select Case unitStr.ToUpperInvariant()
                                    Case "B" : sizeBytes = CLng(sizeNum)
                                    Case "KB", "KIB" : sizeBytes = CLng(sizeNum * 1024)
                                    Case "MB", "MIB" : sizeBytes = CLng(sizeNum * 1024 * 1024)
                                    Case "GB", "GIB" : sizeBytes = CLng(sizeNum * 1024 * 1024 * 1024)
                                End Select
                            End If
                        End If

                        ' Fallback: use stopwatch if FFmpeg's time= is missing.
                        If duration = TimeSpan.Zero AndAlso _stopwatch IsNot Nothing AndAlso _stopwatch.IsRunning Then
                            duration = _stopwatch.Elapsed
                        End If

                        RaiseEvent ProgressUpdated(frameNum, duration, sizeBytes)
                    End If
                End If
            Catch
            End Try
        End If

        ' Report errors — tightened: only match real FFmpeg error markers.
        ' Old code matched the substring "error" which fires on benign lines like
        ' "Error resilience", "errordetect", "max_error_rate", x264 "[error]:" notices, etc.
        Dim low As String = e.Data.ToLowerInvariant()
        Dim isError As Boolean =
            low.Contains("[error]") OrElse
            low.Contains("conversion failed") OrElse
            low.Contains("could not open") OrElse
            low.Contains("no such file or directory") OrElse
            low.Contains("invalid argument") OrElse
            low.Contains("device not found") OrElse
            low.Contains("unknown encoder") OrElse
            low.Contains("not currently supported in output") OrElse
            low.StartsWith("error") OrElse
            low.Contains("av_interleaved_write_header")

        If isError Then
            RaiseEvent ErrorOccurred(e.Data)
            WriteDebugLog("[ERROR] " & e.Data)
        End If
    End Sub

    Private Sub OnExited(sender As Object, e As EventArgs)
        ' Capture exit code + PID first — _ffmpegProcess may be nulled by ForceStop()/Dispose() concurrently.
        Dim exitCode As String = "?"
        Dim pidStr As String = "?"
        Dim proc As Process = _ffmpegProcess
        If proc IsNot Nothing Then
            Try
                pidStr = proc.Id.ToString()
                exitCode = proc.ExitCode.ToString()
            Catch
            End Try
        End If

        LogDebug($"[FFmpeg] Exited PID={pidStr} ExitCode={exitCode} State={_state.ToString()}")
        WriteDebugLog($"FFmpeg exited with code: {exitCode} (state was {_state.ToString()}, PID={pidStr})")

        ' GPT P0 fix: handle BOTH Recording AND Stopping. Previously only Recording
        ' was handled — if user pressed Stop (state becomes Stopping) and FFmpeg then
        ' exited, OnExited would do nothing. That left it up to StopRecordingAsync to
        ' fire RecordingStopped, but if StopRecordingAsync was blocked on WaitForExit
        ' and FFmpeg exited with non-zero code, no error event was raised.
        If _state = CaptureState.Recording OrElse _state = CaptureState.Stopping Then
            If _stopwatch IsNot Nothing Then _stopwatch.Stop()

            If exitCode = "0" Then
                ' Clean exit. Use Interlocked.Exchange so RecordingStopped fires EXACTLY
                ' ONCE — either here in OnExited OR in StopRecordingAsync, never both.
                If System.Threading.Interlocked.Exchange(_stopCompleted, 1) = 0 Then
                    SetState(CaptureState.Idle)
                    RaiseEvent RecordingStopped(_outputFile)
                End If
            ElseIf exitCode <> "?" Then
                ' Non-zero exit code → real error. StopRecordingAsync may have sent q
                ' but FFmpeg may have crashed mid-write. Surface as error event.
                If System.Threading.Interlocked.Exchange(_stopCompleted, 1) = 0 Then
                    SetState(CaptureState.HasError)
                    RaiseEvent ErrorOccurred($"FFmpeg exited unexpectedly with code {exitCode}")
                End If
            End If
            ' exitCode = "?" → couldn't read (process nulled concurrently). Stay silent —
            ' StopRecordingAsync / ForceStop will handle the rest of the lifecycle.
        End If
    End Sub

    ' ── Helpers ──────────────────────────────────────────────

    Private Sub SetState(newState As CaptureState)
        _state = newState
        RaiseEvent StateChanged(newState)
    End Sub

    Private Sub LogDebug(message As String)
        Dim line As String = "[" & DateTime.Now.ToString("HH:mm:ss.fff") & "] " & message
        ' ✅ C8 FIX: StringBuilder is not thread-safe. LogDebug is called from
        ' FFmpeg stdout, stderr, Exited, UI, and TCP listener threads concurrently.
        ' Without a lock, concurrent appends can corrupt internal state.
        SyncLock _logBuffer
            _logBuffer.AppendLine(line)
            If _logBuffer.Length > 10240 Then
                _logBuffer.Remove(0, _logBuffer.Length - 8192)
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Write debug info to log file on disk for troubleshooting.
    ''' </summary>
    Private Sub WriteDebugLog(message As String)
        Try
            Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            Dim logPath As String = Path.Combine(logDir, "capture-engine.log")
            Dim logLine As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "] " & message
            ' ✅ P1: route through BackgroundLogger instead of File.AppendAllText per line.
            ' FFmpeg progress goes to stderr at up to 60 lines/sec (one per frame); the old
            ' per-line AppendAllText was a real disk-thrash on long recordings.
            BackgroundLogger.Log(logPath, logLine)
        Catch
        End Try
    End Sub

    ' ── Dispose ────────────────────────────────────────────────

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                StopAudioCaptureIfNeeded()
                ForceStop()
                If _jobGuard IsNot Nothing Then
                    _jobGuard.Dispose()
                    _jobGuard = Nothing
                End If
            End If
            _disposed = True
        End If
    End Sub

    Private Sub StartAudioCaptureIfNeeded()
        Dim hasNAudio As Boolean = (_settings.SystemAudioCapture OrElse _settings.MicCapture)
        If Not hasNAudio Then Return

        Try
            Dim isSeparateAndMic As Boolean = (_settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack) AndAlso
                                              _settings.MicCapture AndAlso
                                              (Not String.IsNullOrEmpty(_settings.MicDeviceId) OrElse
                                               Not String.IsNullOrEmpty(_settings.MicDeviceName))

            Dim sysStream As Stream = Nothing
            Dim micStream As Stream = Nothing

            sysStream = New BufferedStream(_sysNamedPipe, 64 * 1024)

            If isSeparateAndMic Then
                ' Mic pipe was pre-created in StartRecordingAsync
                _micNamedPipeStream = New BufferedStream(_micNamedPipe, 64 * 1024)
                micStream = _micNamedPipeStream
                LogDebug("[Audio] Separate mode — mic pipe: " & _micPipePath)
            Else
                micStream = sysStream
            End If

            Dim cfg As New NAudioCaptureEngine.AudioConfigValues() With {
                .SystemAudioCapture = _settings.SystemAudioCapture,
                .MicCapture = _settings.MicCapture,
                .SystemAudioVolume = _settings.SystemAudioVolume,
                .MicVolume = _settings.MicVolume,
                .MicDeviceId = _settings.MicDeviceId,
                .MicDeviceName = _settings.MicDeviceName
            }
            _audioEngine = New NAudioCaptureEngine(cfg)
            _audioPipeStream = sysStream
            _audioEngine.Start(sysStream, micStream)
            LogDebug("[Audio] NAudio capture started (mode=" & _settings.AudioTrackMode.ToString() &
                     ", system=" & _settings.SystemAudioCapture.ToString() &
                     ", mic=" & _settings.MicCapture.ToString() & ")")
        Catch ex As Exception
            LogDebug("[Audio] NAudio start error: " & ex.Message)
            WriteDebugLog("[Audio] NAudio start error: " & ex.Message)
        End Try
    End Sub

    Private Sub StopAudioCaptureIfNeeded()
        Try
            If _audioEngine IsNot Nothing Then
                _audioEngine.Stop()
                _audioEngine.Dispose()
                _audioEngine = Nothing
                LogDebug("[Audio] NAudio capture stopped")
            End If
        Catch
        End Try

        Try
            If _audioPipeStream IsNot Nothing Then
                _audioPipeStream.Flush()
                _audioPipeStream.Dispose()
                _audioPipeStream = Nothing
            End If
        Catch
        End Try

        Try
            If _micNamedPipeStream IsNot Nothing Then
                _micNamedPipeStream.Flush()
                _micNamedPipeStream.Dispose()
                _micNamedPipeStream = Nothing
            End If
        Catch
        End Try

        Try
            If _micNamedPipe IsNot Nothing Then
                Try : _micNamedPipe.Disconnect() : Catch : End Try
                _micNamedPipe.Dispose()
                _micNamedPipe = Nothing
            End If
        Catch
        End Try

        Try
            If _sysNamedPipe IsNot Nothing Then
                Try : _sysNamedPipe.Disconnect() : Catch : End Try
                _sysNamedPipe.Dispose()
                _sysNamedPipe = Nothing
            End If
        Catch
        End Try
    End Sub

End Class
