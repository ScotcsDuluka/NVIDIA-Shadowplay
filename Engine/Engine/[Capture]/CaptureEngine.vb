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
Imports CaptureEngine.Audio
Imports CaptureEngine.Audio.Wasapi

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
    Private _audioEngine As AudioEngineSession
    Private _systemAudioSink As AudioTimelineWavSink
    Private _micAudioSink As AudioTimelineWavSink
    Private _audioStopTicks As Long = 0
    Private _tempVideoPath As String
    Private _tempSystemWav As String
    Private _tempMicWav As String
    Private _useTwoProcess As Boolean = False

    ' ── Per-track sync timestamps (per GPT review) ──
    ' Each track has its OWN start timestamp (when its first WASAPI callback fired).
    ' This is more accurate than a single "audio start" because system and mic
    ' devices initialize independently and may start capturing at different times.
    '
    ' _videoStartTicks: back-calculated from first frame= status line
    ' _systemStartTicks: when AudioFileWriter called StartRecording (TRUE capture start)
    ' _micStartTicks: same for mic track
    '
    ' At mux time, each audio input gets its OWN -ss offset:
    '   systemOffset = (videoStart - systemStart) / freq
    '   micOffset = (videoStart - micStart) / freq
    '
    ' NOTE: These are StartRecording call times, NOT first-callback times.
    ' WASAPI loopback may delay first callback by seconds when no audio plays,
    ' so first-callback-time would give wrong offsets (saw -10s offset bug).
    Private _videoStartTicks As Long = 0
    Private _videoStartDetected As Boolean = False
    Private _systemStartTicks As Long = 0
    Private _micStartTicks As Long = 0

    Private _stopCompleted As Integer = 0
    Private _muxCompleted As Integer = 0

    ' ── Last-run diagnostics (for stress test runner) ──
    ' Set during Stop, retained for post-recording analysis.
    ' LastAudioDiagnostics: full GetDiagnostics() output (multiline string)
    ' LastFFmpegStatsLine: final "frame=... Lsize=... dup=... drop=... speed=..." line
    ' LastMuxSummary: mux exit code + duration + offset values
    Private _lastAudioDiagnostics As String = ""
    Private _lastFFmpegStatsLine As String = ""
    Private _lastMuxSummary As String = ""

    Public ReadOnly Property LastAudioDiagnostics As String
        Get
            Return _lastAudioDiagnostics
        End Get
    End Property

    Public ReadOnly Property LastFFmpegStatsLine As String
        Get
            Return _lastFFmpegStatsLine
        End Get
    End Property

    Public ReadOnly Property LastMuxSummary As String
        Get
            Return _lastMuxSummary
        End Get
    End Property

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
                                         _videoStartTicks = 0
                                         _videoStartDetected = False
                                         _systemStartTicks = 0
                                         _micStartTicks = 0
                                         _audioStopTicks = 0
                                         System.Threading.Interlocked.Exchange(_muxCompleted, 0)

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

                                     If _useTwoProcess Then
                                         StartAudioRecorder()
                                     End If

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


                                     Return True
                                 Catch ex As Exception
                                     SetState(CaptureState.HasError)
                                     RaiseEvent ErrorOccurred("Start failed: " & ex.Message)
                                     LogDebug("Exception: " & ex.ToString())
                                     WriteDebugLog("Start exception: " & ex.ToString())
                                     Try : StopAudioWriter() : Catch : End Try
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
        _audioStopTicks = Stopwatch.GetTimestamp()
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
                                     ' MUST complete before mux: the mux decision reads the wav files,
                                     ' and an unflushed sink looks like a header-only file (the
                                     ' 2026-09-05 regression shipped video-only mp4s with the audio
                                     ' still buffered inside the AudioEngine).
                                     If _audioWriter IsNot Nothing Then
                                         LogDebug("[Audio] Stopping audio recorder (flushing .wav files)…")
                                         WriteDebugLog("[Audio] Stopping audio recorder…")
                                         ' _systemStartTicks / _micStartTicks were captured at StartRecording
                                         ' time (in StartAudioRecorder), no need to re-fetch here.
                                         _audioWriter.Stop()
                                         Dim diagMsg As String = _audioWriter.GetDiagnostics()
                                         _lastAudioDiagnostics = diagMsg  ' retain for stress test runner
                                         LogDebug(diagMsg)
                                         WriteDebugLog(diagMsg)
                                         _audioWriter.Dispose()
                                         _audioWriter = Nothing
                                         LogDebug("[Audio] Audio recorder stopped.")
                                     End If
                                     StopAudioWriter()

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

        ' Guard against double-mux: if StopRecordingAsync and OnExited both fire,
        ' only the first one runs mux. The second sees _muxCompleted=1 and returns.
        If System.Threading.Interlocked.Exchange(_muxCompleted, 1) = 1 Then
            LogDebug("[Mux] Already completed (double-mux prevented)")
            Return
        End If

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

        ' ═══ PER-TRACK HIGH-PRECISION SYNC ═══
        ' 1. Get exact video duration via ffprobe (millisecond precision)
        ' 2. Compute PER-TRACK offset:
        '    systemOffset = (videoStart - systemStart) / freq
        '    micOffset = (videoStart - micStart) / freq
        '    Each track gets its OWN -ss because system and mic devices start
        '    capturing at different times (independent WASAPI initialization).
        ' 3. Positive offset → skip audio (-ss)
        '    Negative offset → delay audio (adelay filter at mux time)
        ' 4. apad filter extends short audio with silence to match video duration
        ' 5. -t trims output to exact video duration
        '
        ' NO clamping to zero — negative offsets (audio starts after video) are
        ' supported via adelay filter in BuildMuxArguments.
        ' Range: -2s to +5s (prevents malformed values from timestamp errors)

        Dim videoDurationSec As Double = GetVideoDurationSec(_tempVideoPath)
        ' Audio Engine already aligns both tracks to the same video QPC origin.
        ' Legacy mux must therefore apply ZERO additional head trim/delay.
        Dim systemOffsetSec As Double = 0.0
        Dim micOffsetSec As Double = 0.0

        LogDebug($"[Mux] videoDuration={videoDurationSec:F3}s, shared-audio offsets=0.000s (alignment owned by AudioEngine)")
        WriteDebugLog($"[Mux] videoDuration={videoDurationSec:F3}s, shared-audio offsets=0.000s, videoStartDetected={_videoStartDetected}")

        ' Run mux FFmpeg with per-track alignment
        Dim muxArgs As String = BuildMuxArguments(_tempVideoPath,
                                                   _tempSystemWav, hasSystem,
                                                   _tempMicWav, hasMic,
                                                   _outputFile,
                                                   systemOffsetSec, micOffsetSec,
                                                   videoDurationSec)

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
                    ' The partial mux output has no moov atom and is unplayable;
                    ' the temp video is a complete stream-copy-ready h264 file,
                    ' so ship it video-only instead of leaving a broken mp4.
                    LogDebug("[Mux] FFmpeg mux FAILED — falling back to video-only output")
                    WriteDebugLog($"[Mux] FFmpeg mux ExitCode={muxExitCode} — removing broken output, renaming temp video to final output")
                    Try
                        If File.Exists(_outputFile) Then File.Delete(_outputFile)
                        File.Move(_tempVideoPath, _outputFile)
                        LogDebug("[Mux] Video-only fallback output: " & _outputFile)
                        WriteDebugLog("[Mux] Video-only fallback output: " & _outputFile)
                    Catch ex2 As Exception
                        LogDebug("[Mux] Video-only fallback failed: " & ex2.Message)
                        WriteDebugLog("[Mux] Video-only fallback failed: " & ex2.Message)
                    End Try
                    ' Audio sidecars stay behind for inspection
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
            probePsi.RedirectStandardError = False  ' don't redirect stderr (avoids deadlock)
            probePsi.CreateNoWindow = True

            Using probeProc As New Process()
                probeProc.StartInfo = probePsi
                probeProc.Start()

                ' Read stdout ASYNCHRONOUSLY — prevents deadlock if ffprobe hangs
                ' (old code used ReadToEnd which blocks before WaitForExit timeout)
                Dim stdoutTask As Task(Of String) = probeProc.StandardOutput.ReadToEndAsync()

                ' Wait for process to exit with real timeout
                Dim exited As Boolean = probeProc.WaitForExit(5000)
                If Not exited Then
                    LogDebug("[Mux] ffprobe TIMEOUT (5s) — killing")
                    Try : probeProc.Kill() : Catch : End Try
                    Try : probeProc.WaitForExit(1000) : Catch : End Try
                End If

                ' Get stdout result (task should be complete if process exited)
                Dim stdout As String = ""
                Try
                    stdout = stdoutTask.Result.Trim()
                Catch
                End Try

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

        ' ── Capture last FFmpeg stats line (for stress test runner) ──
        ' FFmpeg emits progress lines like:
        '   frame= 6540 fps=143 q=8.0 size= 94278KiB time=00:00:45.41 bitrate=17005.4kbits/s dup=760 drop=1 speed=0.997x elapsed=0:00:45.55
        ' The FINAL line has "Lsize=" marker (L = last). Capture it for metrics.
        If e.Data.Contains("frame=") AndAlso e.Data.Contains("Lsize=") Then
            _lastFFmpegStatsLine = e.Data
        End If

        ' ── Detect video start (HIGH-PRECISION sync) ──
        ' The output file's PTS 0 frame is the FIRST frame ddagrab grabbed, so
        ' the wall time of that grab is the anchor the audio timeline must map
        ' to. ffmpeg prints "Output #0" right before the transcode loop starts
        ' — within one loop iteration (~10-30ms) of the first grab. That is the
        ' earliest authoritative marker.
        '
        ' The old back-calculation (T0 = now - statusTime) is biased LATE by
        ' everything between the first grab and the status line: on Intel QSV
        ' the encoder init blocks while ddagrab frames queue up, so the first
        ' parseable status line can report time=0.7s at a moment when the
        ' actual content began ~0.5s earlier — the 2026-09-05 "~500ms audio
        ' offset" on this machine. The back-calc is kept ONLY as evidence and
        ' as a fallback when "Output #0" was never seen.
        If Not _videoStartDetected AndAlso _useTwoProcess Then
            If e.Data.Contains("Output #0") Then
                _videoStartTicks = Stopwatch.GetTimestamp()
                _videoStartDetected = True
                PropagateVideoStart("Output#0", -1)
            ElseIf e.Data.IndexOf("frame=", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
                   e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase) >= 0 Then
                ' First frame= status line — fallback anchor + evidence of the
                ' back-calc bias versus the Output#0 anchor.
                Try
                    Dim timeIdx As Integer = e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase) + 5
                    Dim timeStr As String = e.Data.Substring(timeIdx).TrimStart()
                    Dim timeEnd As Integer = timeStr.IndexOf(" "c)
                    If timeEnd > 0 Then timeStr = timeStr.Substring(0, timeEnd)
                    Dim videoTime As TimeSpan
                    If TimeSpan.TryParse(timeStr, Globalization.CultureInfo.InvariantCulture, videoTime) Then
                        Dim nowTicks As Long = Stopwatch.GetTimestamp()
                        Dim backCalcTicks As Long = nowTicks - CLng(videoTime.TotalSeconds * Stopwatch.Frequency)
                        If Not _videoStartDetected Then
                            ' Fallback: no Output#0 marker seen — use the
                            ' back-calculation (legacy behavior).
                            _videoStartTicks = backCalcTicks
                            _videoStartDetected = True
                            PropagateVideoStart("frame=back-calc", videoTime.TotalMilliseconds)
                        ElseIf _videoStartTicks > 0 AndAlso videoTime.TotalSeconds >= 0 Then
                            ' Evidence: how far the back-calc sits from the
                            ' Output#0 anchor (positive = back-calc later =
                            ' encoder-init backlog + status latency).
                            Dim deltaMs As Double = (backCalcTicks - _videoStartTicks) * 1000.0 / Stopwatch.Frequency
                            LogDebug($"[SYNC-TRACE] anchor=Output#0 ticks={_videoStartTicks}, backCalc={backCalcTicks}, backCalcLaterByMs={deltaMs:F1}, frameTime={videoTime.TotalSeconds:F3}s")
                            WriteDebugLog($"[SYNC-TRACE] anchor=Output#0 ticks={_videoStartTicks}, backCalc={backCalcTicks}, backCalcLaterByMs={deltaMs:F1}, frameTime={videoTime.TotalSeconds:F3}s")
                        End If
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

        ' OnExited handles UNEXPECTED exits (FFmpeg crashed or finished during
        ' Recording state). If state is Stopping, StopRecordingAsync is handling
        ' the full shutdown flow (stop audio → mux → fire event) — we do nothing here.
        '
        ' If state is Recording (unexpected exit), we must:
        '   1. Stop audio writer FIRST (finalize .wav files)
        '   2. THEN mux (if exit code = 0 and two-process mode)
        '   3. Fire RecordingStopped/ErrorOccurred exactly once
        '
        ' This fixes the P0 race where OnExited called mux while audio was still
        ' running, producing incomplete .wav files.
        If _state = CaptureState.Recording Then
            If _stopwatch IsNot Nothing Then _stopwatch.Stop()

            ' Step 1: Stop audio writer (finalize .wav files)
            StopAudioWriter()

            If exitCode = "0" Then
                ' Step 2: Mux if needed (guarded by _muxCompleted to prevent double-mux
                ' if StopRecordingAsync also runs)
                If _useTwoProcess Then
                    AwaitOrRunMux()
                End If
                ' Step 3: Fire event exactly once
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

    ''' <summary>Publish the video T0 anchor to the shared audio timeline.
    ''' backCalcFrameMs &gt;= 0 marks the call as coming from the frame= fallback
    ''' (logged for evidence); -1 marks the Output#0 anchor.</summary>
    Private Sub PropagateVideoStart(anchor As String, backCalcFrameMs As Double)
        Try
            If _videoStartTicks <= 0 Then Return
            Dim videoT0Qpc100ns As Long = WasapiPositionCapture.StopwatchTicksTo100ns(_videoStartTicks)
            LogDebug($"[SYNC-TRACE] video T0 anchored via {anchor}: ticks={_videoStartTicks} qpc100ns={videoT0Qpc100ns}" &
                     If(backCalcFrameMs >= 0, $" frameTimeMs={backCalcFrameMs:F1}", ""))
            WriteDebugLog($"[SYNC-TRACE] video T0 anchored via {anchor}: ticks={_videoStartTicks} qpc100ns={videoT0Qpc100ns}" &
                          If(backCalcFrameMs >= 0, $" frameTimeMs={backCalcFrameMs:F1}", ""))
            If _audioEngine IsNot Nothing Then
                _audioEngine.SetVideoStartQpc100ns(videoT0Qpc100ns)
            End If
            _systemAudioSink?.SetVideoStart(videoT0Qpc100ns)
            _micAudioSink?.SetVideoStart(videoT0Qpc100ns)
            Dim sysOffsetMs As Double = If(_systemStartTicks > 0, (_videoStartTicks - _systemStartTicks) * 1000.0 / Stopwatch.Frequency, 0)
            Dim micOffsetMs As Double = If(_micStartTicks > 0, (_videoStartTicks - _micStartTicks) * 1000.0 / Stopwatch.Frequency, 0)
            LogDebug($"[Sync] Video start ({anchor}). sys offset={sysOffsetMs:F1}ms, mic offset={micOffsetMs:F1}ms")
        Catch ex As Exception
            LogDebug("[Sync] PropagateVideoStart failed: " & ex.Message)
        End Try
    End Sub

    Private Sub StartAudioRecorder()
        Try
            _audioEngine = New AudioEngineSession(New AudioEngineConfig With {
                .SystemEnabled = _settings.SystemAudioCapture,
                .MicrophoneEnabled = _settings.MicCapture,
                .MicrophoneDeviceId = If(_settings.MicDeviceId, ""),
                .MicrophoneDeviceName = If(_settings.MicDeviceName, "")
            }, AddressOf LogDebug)

            If _settings.SystemAudioCapture Then
                _systemAudioSink = New AudioTimelineWavSink(_tempSystemWav)
                _audioEngine.AddSink(AudioTrackKind.System, _systemAudioSink)
            End If
            If _settings.MicCapture Then
                _micAudioSink = New AudioTimelineWavSink(_tempMicWav)
                _audioEngine.AddSink(AudioTrackKind.Microphone, _micAudioSink)
            End If

            _audioEngine.Start()
            LogDebug("[Audio] AudioEngine started (shared owner): system=" & _settings.SystemAudioCapture.ToString() &
                     ", mic=" & _settings.MicCapture.ToString())
            WriteDebugLog("[Audio] AudioEngine started — shared WASAPI/device-clock timeline")
        Catch ex As Exception
            LogDebug("[Audio] StartAudioRecorder exception: " & ex.Message)
            WriteDebugLog("[Audio] StartAudioRecorder exception: " & ex.ToString())
            _audioEngine = Nothing
        End Try
    End Sub

    Private Sub StopAudioWriter()
        Try
            If _audioEngine IsNot Nothing Then
                Dim endTicks As Long = If(_audioStopTicks > 0, _audioStopTicks, Stopwatch.GetTimestamp())
                Dim endQpc As Long = WasapiPositionCapture.StopwatchTicksTo100ns(endTicks)
                _audioEngine.Stop(endQpc)
                _systemAudioSink?.Complete(endQpc)
                _micAudioSink?.Complete(endQpc)
                _lastAudioDiagnostics = _audioEngine.Diagnostics.ToString()
                LogDebug(_lastAudioDiagnostics)
                WriteDebugLog(_lastAudioDiagnostics)
                _systemAudioSink = Nothing
                _micAudioSink = Nothing
                _audioEngine.Dispose()
                _audioEngine = Nothing
                LogDebug("[Audio] AudioEngine stopped")
            End If
        Catch ex As Exception
            LogDebug("[Audio] AudioEngine stop exception: " & ex.Message)
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
            Dim logDir As String = AppLayout.P("Logs")
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
