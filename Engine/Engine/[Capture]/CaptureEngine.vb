' CaptureEngine.vb
' ShadowPlay Engine - Two-Process Recording Architecture
'
' ┌─────────────────────────────────────────────────────────────────────┐
' │                    TWO-PROCESS ARCHITECTURE                        │
' ├─────────────────────────────────────────────────────────────────────┤
' │                                                                     │
' │  When audio is DISABLED (VideoOnly):                               │
' │    FFmpeg(ddagrab → NVENC → final.mp4)                              │
' │    Direct output, identical to Engine-Stable. Zero audio overhead. │
' │                                                                     │
' │  When audio is ENABLED:                                            │
' │    1. Video FFmpeg: ddagrab → NVENC → temp.video.mp4 (NO audio)    │
' │    2. AudioFileWriter: WASAPI → temp.system.wav + temp.mic.wav     │
' │    3. On stop: Mux FFmpeg combines them → final.mp4                │
' │       ffmpeg -i video.mp4 -i audio.wav -c:v copy -c:a aac out.mp4   │
' │                                                                     │
' │  Benefits:                                                          │
' │    - Video FFmpeg has ZERO audio overhead → guaranteed 144fps      │
' │    - Audio is isolated → failure never affects video               │
' │    - No named pipe, no silence feeder, no two-input contention     │
' │    - Clean shutdown: just stop WASAPI + send 'q' to FFmpeg         │
' │                                                                     │
' └─────────────────────────────────────────────────────────────────────┘

Imports System.Diagnostics
Imports System.IO
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
        Muxing
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

    ' ── Two-process recording state ──
    ' When audio is enabled, video goes to a temp file and audio goes to .wav
    ' files. At stop time, MuxVideoAudio() combines them into _outputFile.
    Private _audioWriter As AudioFileWriter
    Private _tempVideoPath As String
    Private _tempSystemWav As String
    Private _tempMicWav As String
    Private _useTwoProcess As Boolean = False

    ' ── High-precision sync timestamps ──
    ' These are used at mux time to align audio with video to sample accuracy.
    ' _audioStartTicks: when AudioFileWriter.Start() was called
    ' _videoStartTicks: when FFmpeg's "Output #0" was detected (= first frame encoded)
    ' _stopTicks: when Stop was called
    ' audioOffset = (_videoStartTicks - _audioStartTicks) / freq → seconds of audio
    '                to skip at mux time (leading silence before video started)
    Private _audioStartTicks As Long = 0
    Private _videoStartTicks As Long = 0
    Private _videoStartDetected As Boolean = False

    Private _stopCompleted As Integer = 0

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

    Public Sub New(settings As CaptureSettings)
        _settings = settings
        _logBuffer = New StringBuilder()
        Try
            _jobGuard = New JobObjectGuard()
        Catch ex As Exception
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

        If Not String.IsNullOrEmpty(overrideOutputPath) Then
            Dim ext As String = Path.GetExtension(overrideOutputPath).ToLowerInvariant()
            If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".mkv" OrElse ext = ".avi" OrElse ext = ".m4v" Then
                _outputFile = overrideOutputPath
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

        Return Await Task.Run(Function()
                                 Try
                                     ' ═══ DETERMINE RECORDING MODE ═══
                                     ' Two-process mode is used when ANY audio capture is enabled.
                                     ' This keeps video FFmpeg single-input (= Engine-Stable performance).
                                     Dim hasAudio As Boolean = (_settings.SystemAudioCapture OrElse _settings.MicCapture)
                                     _useTwoProcess = hasAudio

                                     Dim ffmpegOutputPath As String = _outputFile

                                     If _useTwoProcess Then
                                         ' Video goes to temp file; audio goes to .wav files.
                                         ' Final mux produces _outputFile at stop time.
                                         Dim baseDir As String = Path.GetDirectoryName(_outputFile)
                                         Dim baseName As String = Path.GetFileNameWithoutExtension(_outputFile)
                                         _tempVideoPath = Path.Combine(baseDir, baseName & ".video.tmp.mp4")
                                         _tempSystemWav = Path.Combine(baseDir, baseName & ".system.tmp.wav")
                                         _tempMicWav = Path.Combine(baseDir, baseName & ".mic.tmp.wav")

                                         ' Clean up any stale temp files from previous failed runs
                                         DeleteTempFile(_tempVideoPath)
                                         DeleteTempFile(_tempSystemWav)
                                         DeleteTempFile(_tempMicWav)

                                         ' Reset sync timestamps for this recording session
                                         _audioStartTicks = 0
                                         _videoStartTicks = 0
                                         _videoStartDetected = False

                                         ffmpegOutputPath = _tempVideoPath
                                         LogDebug("[Two-Process] Audio enabled — video → temp, audio → wav, mux at stop")
                                     Else
                                         LogDebug("[Single-Process] Video-only — direct output (Engine-Stable mode)")
                                     End If

                                     Dim args As String = BuildFFmpegArguments(ffmpegOutputPath)
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

                                     If _jobGuard IsNot Nothing Then
                                         _jobGuard.Assign(_ffmpegProcess)
                                     End If

                                     _ffmpegProcess.BeginOutputReadLine()
                                     _ffmpegProcess.BeginErrorReadLine()
                                     _stopwatch = Stopwatch.StartNew()
                                     System.Threading.Interlocked.Exchange(_stopCompleted, 0)
                                     SetState(CaptureState.Recording)
                                     RaiseEvent RecordingStarted(_outputFile)

                                     ' ═══ START AUDIO RECORDER (if enabled) ═══
                                     ' Audio runs completely independently — separate thread, separate
                                     ' file output. No pipe, no FFmpeg subprocess, no shared state with
                                     ' the video FFmpeg. Failure here does NOT stop video recording.
                                     If _useTwoProcess Then
                                         StartAudioRecorder()
                                     End If

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
        If _state <> CaptureState.Recording AndAlso _state <> CaptureState.HasError Then
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

                                     ' ═══ SHUTDOWN ORDER (Two-Process) ═══
                                     ' 1. Stop audio recorder first — flushes + finalizes .wav files
                                     '    (WaveFileWriter.Dispose writes correct WAV header)
                                     ' 2. Send 'q' to video FFmpeg — stops ddagrab, finalizes video.mp4
                                     ' 3. WaitForExit on video FFmpeg
                                     ' 4. If two-process: Mux video + audio → final output file
                                     ' 5. Delete temp files
                                     '
                                     ' Note: NO pipe to close, NO silence feeder to stop.
                                     ' Shutdown is just "stop audio writer, send q, wait, mux".

                                     ' ═══ SHUTDOWN ORDER (Two-Process, High-Precision) ═══
                                     ' 1. Send 'q' to FFmpeg FIRST — video keeps encoding until q is processed
                                     ' 2. WaitForExit — FFmpeg finalizes video.mp4 (writes moov atom, etc.)
                                     ' 3. THEN stop audio writer — audio has been recording the ENTIRE video
                                     '    duration + FFmpeg shutdown time (200-500ms). The extra audio gets
                                     '    trimmed by -t <video_duration> at mux time.
                                     ' 4. ffprobe video.mp4 → get exact video duration (to millisecond)
                                     ' 5. Compute audioOffset = (videoStart - audioStart) / freq
                                     ' 6. Mux: -ss <audioOffset> -t <videoDuration> → sample-accurate sync
                                     '
                                     ' This order is CRITICAL: if we stop audio before sending q, the audio
                                     ' file is shorter than the video file → audio cuts off at the end.

                                     ' ─── Step 1: Send 'q' to FFmpeg ───
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

                                         Dim waitMsg As String = $"[FFmpeg] WaitForExit(10000)… PID={_ffmpegProcess.Id}"
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

                                     ' ─── Step 2: Stop audio recorder (AFTER FFmpeg exited) ───
                                     ' Audio has been recording through the entire video duration.
                                     ' The .wav file is slightly longer than the video — mux trims it.
                                     If _audioWriter IsNot Nothing Then
                                         LogDebug("[Audio] Stopping audio recorder (flushing .wav files)…")
                                         WriteDebugLog("[Audio] Stopping audio recorder…")
                                         _audioWriter.Stop()
                                         Dim diagMsg As String = _audioWriter.GetDiagnostics()
                                         LogDebug(diagMsg)
                                         WriteDebugLog(diagMsg)
                                         _audioWriter.Dispose()
                                         _audioWriter = Nothing
                                         LogDebug("[Audio] Audio recorder stopped.")
                                     End If

                                     ' ─── Step 3: Mux video + audio (if two-process) ───
                                     If _useTwoProcess Then
                                         AwaitOrRunMux()
                                     End If

                                     ' ─── Step 4: Fire RecordingStopped (exactly once) ───
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

    ' ── Mux video + audio ─────────────────────────────────────

    Private Sub AwaitOrRunMux()
        If Not _useTwoProcess Then Return
        If String.IsNullOrEmpty(_tempVideoPath) Then Return

        ' Verify temp video exists
        If Not File.Exists(_tempVideoPath) Then
            LogDebug("[Mux] Temp video file missing — skipping mux, temp files cleaned")
            WriteDebugLog("[Mux] Temp video file missing — skipping mux")
            Return
        End If

        SetState(CaptureState.Muxing)

        Dim hasSystem As Boolean = AudioFileWriter.HasAudioData(_tempSystemWav)
        Dim hasMic As Boolean = AudioFileWriter.HasAudioData(_tempMicWav)

        LogDebug($"[Mux] hasSystem={hasSystem}, hasMic={hasMic}")
        WriteDebugLog($"[Mux] hasSystem={hasSystem}, hasMic={hasMic}")

        If Not hasSystem AndAlso Not hasMic Then
            ' No audio captured (WASAPI never fired, or mic not connected).
            ' Just rename the temp video to the final output — no mux needed.
            LogDebug("[Mux] No audio data — renaming temp video to final output (no mux)")
            WriteDebugLog("[Mux] No audio data — renaming temp video to final output")
            Try
                ' Delete final if exists (stale), then move temp → final
                If File.Exists(_outputFile) Then File.Delete(_outputFile)
                File.Move(_tempVideoPath, _outputFile)
                LogDebug("[Mux] Renamed temp video → " & _outputFile)
            Catch ex As Exception
                LogDebug("[Mux] Rename failed: " & ex.Message)
                WriteDebugLog("[Mux] Rename failed: " & ex.Message)
            End Try
            Return
        End If

        ' ═══ HIGH-PRECISION SYNC: ffprobe + offset + duration ═══
        ' 1. Get exact video duration via ffprobe (to millisecond precision)
        ' 2. Compute audioOffset = (videoStartTicks - audioStartTicks) / freq
        '    This is the leading audio that was recorded BEFORE video started
        '    (AudioFileWriter starts at process launch, ddagrab takes ~400ms to init)
        ' 3. Pass both to BuildMuxArguments → -ss <offset> -t <videoDuration>
        '    -ss skips the leading silence, -t trims output to exact video length

        Dim videoDurationSec As Double = GetVideoDurationSec(_tempVideoPath)
        Dim audioOffsetSec As Double = 0.0
        If _videoStartDetected AndAlso _audioStartTicks > 0 Then
            audioOffsetSec = (_videoStartTicks - _audioStartTicks) / Stopwatch.Frequency
            ' Clamp to reasonable range (0-5s) to avoid malformed -ss values
            If audioOffsetSec < 0 Then audioOffsetSec = 0
            If audioOffsetSec > 5.0 Then audioOffsetSec = 5.0
        End If

        LogDebug($"[Mux] videoDuration={videoDurationSec:F3}s, audioOffset={audioOffsetSec:F3}s")
        WriteDebugLog($"[Mux] videoDuration={videoDurationSec:F3}s, audioOffset={audioOffsetSec:F3}s, videoStartDetected={_videoStartDetected}")

        ' Run mux FFmpeg with high-precision alignment
        Dim muxArgs As String = BuildMuxArguments(_tempVideoPath,
                                                   _tempSystemWav, hasSystem,
                                                   _tempMicWav, hasMic,
                                                   _outputFile,
                                                   audioOffsetSec, videoDurationSec)

        LogDebug("[Mux] FFmpeg command: " & _settings.FFmpegPath & " " & muxArgs)
        WriteDebugLog("[Mux] FFmpeg command: " & _settings.FFmpegPath & " " & muxArgs)

        Dim muxStart As DateTime = DateTime.Now
        Try
            Dim muxPsi As New ProcessStartInfo()
            muxPsi.FileName = _settings.FFmpegPath
            muxPsi.Arguments = muxArgs
            muxPsi.UseShellExecute = False
            muxPsi.RedirectStandardOutput = True
            muxPsi.RedirectStandardError = True
            muxPsi.CreateNoWindow = True

            Using muxProc As New Process()
                muxProc.StartInfo = muxPsi
                muxProc.Start()
                If _jobGuard IsNot Nothing Then
                    _jobGuard.Assign(muxProc)
                End If

                ' Read stdout/stderr to completion (prevents deadlock on long output)
                Dim stdoutTask As Task(Of String) = muxProc.StandardOutput.ReadToEndAsync()
                Dim stderrTask As Task(Of String) = muxProc.StandardError.ReadToEndAsync()

                muxProc.WaitForExit(60000) ' 60s max for mux (should be ~1-5s for stream copy + AAC)

                If Not muxProc.HasExited Then
                    LogDebug("[Mux] FFmpeg mux TIMEOUT — killing")
                    Try
                        muxProc.Kill()
                        muxProc.WaitForExit(2000)
                    Catch
                    End Try
                End If

                Dim muxTime As TimeSpan = DateTime.Now - muxStart
                Dim muxExitCode As Integer = -1
                Try
                    muxExitCode = muxProc.ExitCode
                Catch
                End Try

                LogDebug($"[Mux] FFmpeg mux completed in {muxTime.TotalMilliseconds:F0}ms, ExitCode={muxExitCode}")
                WriteDebugLog($"[Mux] FFmpeg mux ExitCode={muxExitCode}, duration={muxTime.TotalMilliseconds:F0}ms")

                ' Log mux stderr (for debugging)
                Dim stderrResult As String = stderrTask.Result
                If Not String.IsNullOrEmpty(stderrResult) Then
                    ' Only log first + last 500 chars to avoid spam
                    If stderrResult.Length > 1000 Then
                        stderrResult = stderrResult.Substring(0, 500) & "…[truncated]…" & stderrResult.Substring(stderrResult.Length - 500)
                    End If
                    WriteDebugLog("[Mux stderr] " & stderrResult)
                End If

                If muxExitCode <> 0 Then
                    LogDebug("[Mux] FFmpeg mux FAILED — keeping temp files for inspection")
                    WriteDebugLog("[Mux] FFmpeg mux FAILED — temp files preserved")
                    ' Don't delete temp files so user can inspect
                    Return
                End If
            End Using

            ' Mux succeeded — delete temp files
            DeleteTempFile(_tempVideoPath)
            DeleteTempFile(_tempSystemWav)
            DeleteTempFile(_tempMicWav)
            LogDebug("[Mux] Temp files cleaned up")
            WriteDebugLog("[Mux] Temp files cleaned up")

        Catch ex As Exception
            LogDebug("[Mux] Exception: " & ex.Message)
            WriteDebugLog("[Mux] Exception: " & ex.ToString())
        End Try
    End Sub

    Private Sub DeleteTempFile(path As String)
        If String.IsNullOrEmpty(path) Then Return
        Try
            If File.Exists(path) Then File.Delete(path)
        Catch ex As Exception
            LogDebug("[Mux] Failed to delete temp file " & path & ": " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Get exact video duration using ffprobe.
    ''' Returns duration in seconds (e.g., 5.171000), or 0.0 if ffprobe fails.
    '''
    ''' Uses ffprobe.exe which is in the same directory as ffmpeg.exe.
    ''' Command: ffprobe -v error -show_entries format=duration -of csv=p=0 video.mp4
    ''' Output: "5.171000\n"
    ''' </summary>
    Private Function GetVideoDurationSec(videoPath As String) As Double
        Try
            ' Find ffprobe.exe — it's in the same directory as ffmpeg.exe
            Dim ffmpegDir As String = Path.GetDirectoryName(_settings.FFmpegPath)
            Dim ffprobePath As String = Path.Combine(ffmpegDir, "ffprobe.exe")
            If Not File.Exists(ffprobePath) Then
                ' Try alternative capitalization
                ffprobePath = Path.Combine(ffmpegDir, "API-Core", "ffprobe.exe")
                If Not File.Exists(ffprobePath) Then
                    ffprobePath = Path.Combine(ffmpegDir, "api-core", "ffprobe.exe")
                End If
            End If
            If Not File.Exists(ffprobePath) Then
                LogDebug("[Mux] ffprobe.exe not found — using -shortest fallback (no -t)")
                WriteDebugLog("[Mux] ffprobe.exe not found at " & ffprobePath)
                Return 0.0
            End If

            Dim probePsi As New ProcessStartInfo()
            probePsi.FileName = ffprobePath
            probePsi.Arguments = "-v error -show_entries format=duration -of csv=p=0 """ & videoPath & """"
            probePsi.UseShellExecute = False
            probePsi.RedirectStandardOutput = True
            probePsi.RedirectStandardError = True
            probePsi.CreateNoWindow = True

            Using probeProc As New Process()
                probeProc.StartInfo = probePsi
                probeProc.Start()
                Dim stdout As String = probeProc.StandardOutput.ReadToEnd().Trim()
                probeProc.WaitForExit(5000)
                If Not probeProc.HasExited Then
                    Try : probeProc.Kill() : Catch : End Try
                End If

                If Not String.IsNullOrEmpty(stdout) Then
                    Dim dur As Double = 0.0
                    If Double.TryParse(stdout, Globalization.CultureInfo.InvariantCulture, dur) AndAlso dur > 0 Then
                        LogDebug($"[Mux] ffprobe duration = {dur:F3}s")
                        Return dur
                    End If
                End If
                LogDebug("[Mux] ffprobe returned empty/invalid output: '" & stdout & "'")
                WriteDebugLog("[Mux] ffprobe output: '" & stdout & "'")
            End Using
        Catch ex As Exception
            LogDebug("[Mux] ffprobe exception: " & ex.Message)
            WriteDebugLog("[Mux] ffprobe exception: " & ex.ToString())
        End Try
        Return 0.0
    End Function

    ' ── Force Stop ────────────────────────────────────────────

    Public Sub ForceStop()
        Try
            LogDebug("[FFmpeg] FORCE STOP")
            StopAudioWriter()

            If _ffmpegProcess IsNot Nothing AndAlso Not _ffmpegProcess.HasExited Then
                LogDebug("[FFmpeg] FORCE KILL")
                _ffmpegProcess.Kill()
                _ffmpegProcess.WaitForExit(5000)
            End If

            ' Clean up temp files on force stop
            DeleteTempFile(_tempVideoPath)
            DeleteTempFile(_tempSystemWav)
            DeleteTempFile(_tempMicWav)
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

        ' ── Detect video start (HIGH-PRECISION sync) ──
        ' Instead of using "Output #0" (which appears BEFORE the first frame is
        ' actually captured, introducing ~80-130ms error), we use the FIRST
        ' "frame=" status line and back-calculate the exact video start time.
        '
        ' The first "frame=" line includes "time=00:00:00.XX" which tells us
        ' how much video time has elapsed. By subtracting that from the current
        ' real-time timestamp, we get the EXACT moment video frame 0 was captured:
        '
        '   videoStartTicks = nowTicks - (videoTimeSeconds × freq)
        '
        ' This reduces sync error from ~80ms to <5ms (sub-frame at 144fps).
        If Not _videoStartDetected AndAlso _useTwoProcess Then
            If e.Data.Contains("Output #0") Then
                ' Mark that Output #0 was seen (so we know to look for first frame=)
                _videoStartTicks = Stopwatch.GetTimestamp()
            ElseIf e.Data.IndexOf("frame=", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
                   e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
                   _videoStartTicks > 0 Then
                ' First frame= status line — parse "time=" and back-calculate
                Try
                    Dim timeIdx As Integer = e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase) + 5
                    Dim timeStr As String = e.Data.Substring(timeIdx).TrimStart()
                    Dim timeEnd As Integer = timeStr.IndexOf(" "c)
                    If timeEnd > 0 Then timeStr = timeStr.Substring(0, timeEnd)
                    Dim videoTime As TimeSpan
                    If TimeSpan.TryParse(timeStr, Globalization.CultureInfo.InvariantCulture, videoTime) Then
                        ' Back-calculate: videoStart = now - videoTime
                        Dim nowTicks As Long = Stopwatch.GetTimestamp()
                        Dim videoTimeTicks As Long = CLng(videoTime.TotalSeconds * Stopwatch.Frequency)
                        _videoStartTicks = nowTicks - videoTimeTicks
                        _videoStartDetected = True
                        Dim elapsedMs As Double = (_videoStartTicks - _audioStartTicks) * 1000.0 / Stopwatch.Frequency
                        LogDebug($"[Sync] Video start computed. frame time={videoTime.TotalSeconds:F3}s, audio offset={elapsedMs:F1}ms")
                        WriteDebugLog($"[Sync] Video start at ticks={_videoStartTicks}, offset from audio={elapsedMs:F1}ms (back-calculated from time={videoTime.TotalSeconds:F3}s)")
                    End If
                Catch
                End Try
            End If
        End If

        ' Parse frame progress
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

                        Dim duration As TimeSpan = TimeSpan.Zero
                        Dim sizeBytes As Long = 0

                        Dim timeIdx As Integer = e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase)
                        If timeIdx >= 0 Then
                            Dim timeStr As String = e.Data.Substring(timeIdx + 5).TrimStart()
                            Dim timeEnd As Integer = timeStr.IndexOf(" "c)
                            If timeEnd > 0 Then timeStr = timeStr.Substring(0, timeEnd)
                            Dim parsed As TimeSpan
                            If TimeSpan.TryParse(timeStr, Globalization.CultureInfo.InvariantCulture, parsed) Then
                                duration = parsed
                            End If
                        End If

                        Dim sizeIdx As Integer = e.Data.IndexOf("size=", StringComparison.OrdinalIgnoreCase)
                        If sizeIdx >= 0 Then
                            Dim sizeStr As String = e.Data.Substring(sizeIdx + 5).TrimStart()
                            Dim sizeEnd As Integer = sizeStr.IndexOf(" "c)
                            If sizeEnd > 0 Then sizeStr = sizeStr.Substring(0, sizeEnd)
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
                            If Double.TryParse(numStr, Globalization.CultureInfo.InvariantCulture, sizeNum) Then
                                Select Case unitStr.ToUpperInvariant()
                                    Case "B" : sizeBytes = CLng(sizeNum)
                                    Case "KB", "KIB" : sizeBytes = CLng(sizeNum * 1024)
                                    Case "MB", "MIB" : sizeBytes = CLng(sizeNum * 1024 * 1024)
                                    Case "GB", "GIB" : sizeBytes = CLng(sizeNum * 1024 * 1024 * 1024)
                                End Select
                            End If
                        End If

                        If duration = TimeSpan.Zero AndAlso _stopwatch IsNot Nothing AndAlso _stopwatch.IsRunning Then
                            duration = _stopwatch.Elapsed
                        End If

                        RaiseEvent ProgressUpdated(frameNum, duration, sizeBytes)
                    End If
                End If
            Catch
            End Try
        End If

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

        ' Only fire RecordingStopped from OnExited if we're NOT in Stopping state —
        ' StopRecordingAsync handles the full mux flow + RecordingStopped.
        ' OnExited only handles unexpected exits (crash during Recording).
        If _state = CaptureState.Recording Then
            If _stopwatch IsNot Nothing Then _stopwatch.Stop()

            If exitCode = "0" Then
                ' Clean exit during Recording state (shouldn't normally happen unless
                ' ddagrab finished or user quit via another path). Do mux if needed.
                If _useTwoProcess Then
                    AwaitOrRunMux()
                End If
                If System.Threading.Interlocked.Exchange(_stopCompleted, 1) = 0 Then
                    SetState(CaptureState.Idle)
                    RaiseEvent RecordingStopped(_outputFile)
                End If
            ElseIf exitCode <> "?" Then
                If System.Threading.Interlocked.Exchange(_stopCompleted, 1) = 0 Then
                    SetState(CaptureState.HasError)
                    RaiseEvent ErrorOccurred($"FFmpeg exited unexpectedly with code {exitCode}")
                End If
            End If
        End If
    End Sub

    ' ── Audio Recorder Lifecycle ─────────────────────────────

    Private Sub StartAudioRecorder()
        Try
            Dim cfg As New AudioFileWriter.AudioConfigValues() With {
                .SystemAudioCapture = _settings.SystemAudioCapture,
                .MicCapture = _settings.MicCapture,
                .SystemAudioVolume = _settings.SystemAudioVolume,
                .MicVolume = _settings.MicVolume,
                .MicDeviceId = _settings.MicDeviceId,
                .MicDeviceName = _settings.MicDeviceName
            }
            _audioWriter = New AudioFileWriter(cfg)

            AddHandler _audioWriter.SystemStartFailed, Sub(reason As String)
                                                           LogDebug("[Audio] System start failed: " & reason)
                                                           WriteDebugLog("[Audio] System start failed: " & reason)
                                                       End Sub
            AddHandler _audioWriter.MicStartFailed, Sub(reason As String)
                                                       LogDebug("[Audio] Mic start failed: " & reason)
                                                       WriteDebugLog("[Audio] Mic start failed: " & reason)
                                                   End Sub

            Dim ok As Boolean = _audioWriter.Start(_tempSystemWav, _tempMicWav)
            If ok Then
                _audioStartTicks = Stopwatch.GetTimestamp()
                Dim offsetMsg As String = $"[Audio] AudioFileWriter started at ticks={_audioStartTicks}"
                LogDebug("[Audio] AudioFileWriter started (system=" & _settings.SystemAudioCapture.ToString() &
                         ", mic=" & _settings.MicCapture.ToString() & ")")
                WriteDebugLog(offsetMsg)
            Else
                LogDebug("[Audio] AudioFileWriter failed to start — video continues without audio")
                WriteDebugLog("[Audio] AudioFileWriter failed to start — video continues without audio")
            End If
        Catch ex As Exception
            LogDebug("[Audio] StartAudioRecorder exception: " & ex.Message)
            WriteDebugLog("[Audio] StartAudioRecorder exception: " & ex.ToString())
        End Try
    End Sub

    Private Sub StopAudioWriter()
        Try
            If _audioWriter IsNot Nothing Then
                _audioWriter.Stop()
                _audioWriter.Dispose()
                _audioWriter = Nothing
                LogDebug("[Audio] AudioFileWriter stopped")
            End If
        Catch
        End Try
    End Sub

    ' ── Helpers ──────────────────────────────────────────────

    Private Sub SetState(newState As CaptureState)
        _state = newState
        RaiseEvent StateChanged(newState)
    End Sub

    Private Sub LogDebug(message As String)
        Dim line As String = "[" & DateTime.Now.ToString("HH:mm:ss.fff") & "] " & message
        SyncLock _logBuffer
            _logBuffer.AppendLine(line)
            If _logBuffer.Length > 10240 Then
                _logBuffer.Remove(0, _logBuffer.Length - 8192)
            End If
        End SyncLock
    End Sub

    Private Sub WriteDebugLog(message As String)
        Try
            Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            Dim logPath As String = Path.Combine(logDir, "capture-engine.log")
            Dim logLine As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "] " & message
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
                StopAudioWriter()
                ForceStop()
                If _jobGuard IsNot Nothing Then
                    _jobGuard.Dispose()
                    _jobGuard = Nothing
                End If
            End If
            _disposed = True
        End If
    End Sub

End Class
