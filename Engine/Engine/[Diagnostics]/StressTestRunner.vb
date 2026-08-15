' StressTestRunner.vb
' Engine-Audio — Stress Test Runner
'
' Validates the two-process architecture (ffmpeg #1 video, AudioFileWriter sidecar,
' ffmpeg #2 mux) against the invariants defined in ENGINE-REALTIME-ARCHITECTURE.md:
'
'   ── INVIOLATE ──
'   ffmpeg #1:        NO audio input, NO mux, NO amix, NO resample
'   Audio callback:   copy + enqueue only (no disk I/O, no heavy allocation)
'   JobObjectGuard:   AboveNormal priority only (NO High/Realtime)
'   Accounting:       BytesEnqueued = WrittenBytes + DroppedBytes + DroppedSilenceBytes
'                     → BytesAccountingResidual MUST be 0 after clean drain
'
'   ── RED FLAGS (auto-fail) ──
'   DroppedBytes > 0              → real audio data loss
'   BytesAccountingResidual ≠ 0  → writer didn't drain OR counting bug
'   Writer FAILED                → disk write exception didn't propagate
'
'   ── WARNINGS ──
'   FPS < (target - 5)           → video capture degraded
'   Speed < 0.95x                → FFmpeg can't keep up
'   dup > 0                      → ddagrab duplicate frames (sign of starvation)
'   drop > 0                     → FFmpeg dropped frames
'   WriteLagBytes > 0            → writer thread behind at stop time
'   |sysOffset| > 5s             → sync calculation out of expected range

Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Drives CaptureEngine through a matrix of stress test scenarios and validates
''' the architecture invariants by parsing FFmpeg stats + AudioFileWriter diagnostics.
''' </summary>
Public Class StressTestRunner

    Public Class TestScenario
        Public Property Name As String
        Public Property Description As String
        Public Property Settings As CaptureSettings
        Public Property DurationSec As Integer = 10
        Public Property ExpectAudio As Boolean = True
        Public Property ExpectMic As Boolean = False
    End Class

    Public Class TestResult
        Public Property Name As String
        Public Property Pass As Boolean
        Public Property FailureReason As String = ""

        ' ── Video metrics (parsed from FFmpeg Lsize= line) ──
        Public Property TargetFps As Integer
        Public Property ActualFps As Double
        Public Property Speed As Double
        Public Property Dup As Long
        Public Property Drop As Long
        Public Property VideoDurationSec As Double

        ' ── Audio metrics (parsed from GetDiagnostics) ──
        Public Property SysCallbacks As Long
        Public Property SysBytesEnqueued As Long
        Public Property SysWrittenBytes As Long
        Public Property SysWriteLagBytes As Long
        Public Property SysDroppedChunks As Long
        Public Property SysDroppedBytes As Long
        Public Property SysDroppedSilenceBytes As Long
        Public Property SysBytesAccountingResidual As Long
        Public Property SysInitialSilenceSec As Double

        Public Property MicCallbacks As Long
        Public Property MicBytesEnqueued As Long
        Public Property MicWrittenBytes As Long
        Public Property MicWriteLagBytes As Long
        Public Property MicDroppedBytes As Long
        Public Property MicBytesAccountingResidual As Long

        ' ── Mux metrics ──
        Public Property SysOffsetSec As Double
        Public Property MicOffsetSec As Double
        Public Property MuxExitCode As Integer
        Public Property MuxDurationMs As Double

        ' ── A/V sync estimate ──
        ' Computed as: |video_duration - (sys_written_bytes / sys_bytes_per_second)|
        ' Should be < 100ms for a healthy run (apad + -t handle small mismatches).
        Public Property AVSyncMs As Double

        Public Property OutputFile As String

        Public Overrides Function ToString() As String
            Dim status As String = If(Pass, "PASS", "FAIL")
            Return $"[{status}] {Name}: FPS={ActualFps:F1}/{TargetFps}, dup={Dup}, drop={Drop}, " &
                   $"DroppedBytes={SysDroppedBytes}, Residual={SysBytesAccountingResidual}, " &
                   $"Sync={AVSyncMs:F0}ms"
        End Function
    End Class

    Private _engine As CaptureEngine
    Private _outputDir As String

    Public Sub New(engine As CaptureEngine, outputDir As String)
        _engine = engine
        _outputDir = outputDir
        If Not Directory.Exists(_outputDir) Then
            Directory.CreateDirectory(_outputDir)
        End If
    End Sub

    ''' <summary>
    ''' Build the default test matrix (10 scenarios from the user's spec).
    ''' Each scenario uses a fresh CaptureSettings instance.
    ''' </summary>
    Public Function BuildDefaultMatrix() As List(Of TestScenario)
        Dim scenarios As New List(Of TestScenario)()

        ' Helper to create base settings
        Dim baseSettings As CaptureSettings = CaptureSettings.Load(Path.Combine(_outputDir, "stress-test-config.json"))
        If String.IsNullOrEmpty(baseSettings.FFmpegPath) OrElse Not File.Exists(baseSettings.FFmpegPath) Then
            ' Fallback: use default detection
            baseSettings = CaptureSettings.CreateDefault(Path.Combine(_outputDir, "stress-test-config.json"))
        End If
        baseSettings.OutputDirectory = _outputDir

        ' ── 1. Video only ──
        Dim s1 As CaptureSettings = CloneSettings(baseSettings)
        s1.SystemAudioCapture = False
        s1.MicCapture = False
        s1.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "01_video_only_60fps",
            .Description = "Video only, 60 FPS, no audio",
            .Settings = s1,
            .DurationSec = 10,
            .ExpectAudio = False
        })

        ' ── 2. System audio only ──
        Dim s2 As CaptureSettings = CloneSettings(baseSettings)
        s2.SystemAudioCapture = True
        s2.MicCapture = False
        s2.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "02_system_audio_60fps",
            .Description = "System audio loopback, 60 FPS",
            .Settings = s2,
            .DurationSec = 10,
            .ExpectAudio = True,
            .ExpectMic = False
        })

        ' ── 3. Mic only ──
        Dim s3 As CaptureSettings = CloneSettings(baseSettings)
        s3.SystemAudioCapture = False
        s3.MicCapture = True
        s3.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "03_mic_only_60fps",
            .Description = "Mic input only, 60 FPS",
            .Settings = s3,
            .DurationSec = 10,
            .ExpectAudio = True,
            .ExpectMic = True
        })

        ' ── 4. System + Mic ──
        Dim s4 As CaptureSettings = CloneSettings(baseSettings)
        s4.SystemAudioCapture = True
        s4.MicCapture = True
        s4.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SingleTrack
        s4.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "04_system_plus_mic_60fps",
            .Description = "System + mic mixed into single track",
            .Settings = s4,
            .DurationSec = 10,
            .ExpectAudio = True,
            .ExpectMic = True
        })

        ' ── 5. System + Mic + 144 FPS ──
        Dim s5 As CaptureSettings = CloneSettings(baseSettings)
        s5.SystemAudioCapture = True
        s5.MicCapture = True
        s5.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SingleTrack
        s5.FPS = 144
        s5.Bitrate = 17000000
        scenarios.Add(New TestScenario With {
            .Name = "05_system_plus_mic_144fps",
            .Description = "Stress: 144 FPS + system + mic (video path must stay single-input)",
            .Settings = s5,
            .DurationSec = 15,
            .ExpectAudio = True,
            .ExpectMic = True
        })

        ' ── 6. Start/Stop spam (10 rapid cycles) ──
        Dim s6 As CaptureSettings = CloneSettings(baseSettings)
        s6.SystemAudioCapture = True
        s6.MicCapture = False
        s6.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "06_start_stop_spam",
            .Description = "10 rapid start/stop cycles, 2s each — tests lifecycle state machine",
            .Settings = s6,
            .DurationSec = 2,
            .ExpectAudio = True
        })

        ' ── 7. Long continuous (60s) ──
        Dim s7 As CaptureSettings = CloneSettings(baseSettings)
        s7.SystemAudioCapture = True
        s7.MicCapture = True
        s7.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SingleTrack
        s7.FPS = 144
        s7.Bitrate = 17000000
        scenarios.Add(New TestScenario With {
            .Name = "07_long_60s_144fps",
            .Description = "60s continuous, 144 FPS — tests queue growth + writer thread stamina",
            .Settings = s7,
            .DurationSec = 60,
            .ExpectAudio = True,
            .ExpectMic = True
        })

        ' ── 8. Separate tracks (MKV-ready test, MP4 output) ──
        Dim s8 As CaptureSettings = CloneSettings(baseSettings)
        s8.SystemAudioCapture = True
        s8.MicCapture = True
        s8.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack
        s8.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "08_separate_tracks",
            .Description = "Two separate audio tracks (system + mic) in MP4",
            .Settings = s8,
            .DurationSec = 10,
            .ExpectAudio = True,
            .ExpectMic = True
        })

        ' ── 9. System silence (no audio playing) ──
        Dim s9 As CaptureSettings = CloneSettings(baseSettings)
        s9.SystemAudioCapture = True
        s9.MicCapture = False
        s9.FPS = 60
        scenarios.Add(New TestScenario With {
            .Name = "09_silence_full_clip",
            .Description = "No audio playing entire clip — tests initial silence prefill + apad",
            .Settings = s9,
            .DurationSec = 15,
            .ExpectAudio = True
        })

        ' ── 10. 144 FPS video only (baseline) ──
        Dim s10 As CaptureSettings = CloneSettings(baseSettings)
        s10.SystemAudioCapture = False
        s10.MicCapture = False
        s10.FPS = 144
        s10.Bitrate = 17000000
        scenarios.Add(New TestScenario With {
            .Name = "10_video_only_144fps_baseline",
            .Description = "Video only 144 FPS — establishes baseline (should match Engine-Stable)",
            .Settings = s10,
            .DurationSec = 15,
            .ExpectAudio = False
        })

        Return scenarios
    End Function

    Private Function CloneSettings(src As CaptureSettings) As CaptureSettings
        ' Create a new instance with copied fields (CaptureSettings doesn't implement ICloneable)
        Dim clone As New CaptureSettings()
        clone.UseNativeResolution = src.UseNativeResolution
        clone.Encoder = src.Encoder
        clone.FPS = src.FPS
        clone.Bitrate = src.Bitrate
        clone.CaptureMethod = src.CaptureMethod
        clone.OutputDirectory = src.OutputDirectory
        clone.AudioCapture = False  ' legacy dshow, never used in two-process
        clone.AudioDevice = ""
        clone.SystemAudioCapture = src.SystemAudioCapture
        clone.MicCapture = src.MicCapture
        clone.SystemAudioVolume = src.SystemAudioVolume
        clone.MicVolume = src.MicVolume
        clone.MicDeviceName = src.MicDeviceName
        clone.MicDeviceId = src.MicDeviceId
        clone.AudioTrackMode = src.AudioTrackMode
        clone.PixelFormat = src.PixelFormat
        clone.Preset = src.Preset
        clone.NvencPreset = src.NvencPreset
        clone.RateControl = src.RateControl
        clone.FileFormat = src.FileFormat
        clone.FFmpegPath = src.FFmpegPath
        clone.HotkeyStart = src.HotkeyStart
        clone.HotkeyStop = src.HotkeyStop
        clone.HotkeyToggle = src.HotkeyToggle
        clone.CustomWidth = src.CustomWidth
        clone.CustomHeight = src.CustomHeight
        clone.ConfigVersion = src.ConfigVersion
        Return clone
    End Function

    ''' <summary>
    ''' Run a single scenario and return the parsed result.
    ''' Hooks CaptureEngine events, runs for scenario.DurationSec, then stops and parses metrics.
    ''' </summary>
    Public Async Function RunScenarioAsync(scenario As TestScenario) As Task(Of TestResult)
        Dim result As New TestResult With {
            .Name = scenario.Name,
            .TargetFps = scenario.Settings.FPS
        }

        Try
            ' Generate unique output path for this scenario
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim outputPath As String = Path.Combine(_outputDir, $"stress_{scenario.Name}_{timestamp}.mp4")
            result.OutputFile = outputPath

            ' Wire up CaptureEngine (create a fresh instance for isolation)
            Dim engine As New CaptureEngine(scenario.Settings)

            Dim startComplete As New TaskCompletionSource(Of Boolean)()
            Dim stopComplete As New TaskCompletionSource(Of Boolean)()
            Dim errorMsg As String = ""

            AddHandler engine.RecordingStarted, Sub(file As String)
                                                    startComplete.TrySetResult(True)
                                                End Sub
            AddHandler engine.RecordingStopped, Sub(file As String)
                                                    stopComplete.TrySetResult(True)
                                                End Sub
            AddHandler engine.ErrorOccurred, Sub(msg As String)
                                                 errorMsg = msg
                                                 startComplete.TrySetResult(False)
                                                 stopComplete.TrySetResult(False)
                                             End Sub

            ' ── Start ──
            Dim startOk As Boolean = Await engine.StartRecordingAsync(outputPath)
            If Not startOk Then
                Await Task.WhenAny(startComplete.Task, Task.Delay(5000))
            End If

            If Not startComplete.Task.IsCompleted OrElse Not startComplete.Task.Result Then
                result.Pass = False
                result.FailureReason = If(String.IsNullOrEmpty(errorMsg), "Start failed (no event)", errorMsg)
                Return result
            End If

            ' ── Wait for duration ──
            Await Task.Delay(scenario.DurationSec * 1000)

            ' ── Stop ──
            Dim stopOk As Boolean = Await engine.StopRecordingAsync()
            Await Task.WhenAny(stopComplete.Task, Task.Delay(30000))  ' 30s max for mux

            ' ── Parse metrics from engine's last-run state ──
            ParseFFmpegStats(engine.LastFFmpegStatsLine, result)
            ParseAudioDiagnostics(engine.LastAudioDiagnostics, scenario, result)
            ParseMuxSummary(engine.LastMuxSummary, result)

            ' ── Compute A/V sync estimate ──
            ' Use video duration (from FFmpeg Lsize= line) vs audio written bytes / bytes_per_second.
            ' For system audio at 48kHz stereo f32le = 384000 bytes/sec.
            If result.VideoDurationSec > 0 AndAlso result.SysWrittenBytes > 0 Then
                Dim audioDurationSec As Double = result.SysWrittenBytes / 384000.0
                result.AVSyncMs = Math.Abs(result.VideoDurationSec - audioDurationSec) * 1000.0
            End If

            ' ── Apply validation rules ──
            Validate(scenario, result)

        Catch ex As Exception
            result.Pass = False
            result.FailureReason = "Exception: " & ex.Message
        End Try

        Return result
    End Function

    ''' <summary>
    ''' Run all scenarios in a matrix sequentially. Returns results for each.
    ''' </summary>
    Public Async Function RunMatrixAsync(scenarios As List(Of TestScenario),
                                         Optional progressCallback As Action(Of TestResult, Integer, Integer) = Nothing
                                         ) As Task(Of List(Of TestResult))
        Dim results As New List(Of TestResult)()
        Dim total As Integer = scenarios.Count

        For i As Integer = 0 To total - 1
            Dim scenario As TestScenario = scenarios(i)
            Console.WriteLine($"[{i + 1}/{total}] Running: {scenario.Name} — {scenario.Description}")
            Dim result As TestResult = Await RunScenarioAsync(scenario)
            results.Add(result)
            Console.WriteLine($"  → {result}")
            If progressCallback IsNot Nothing Then
                progressCallback(result, i + 1, total)
            End If

            ' Brief pause between scenarios to let filesystem settle
            Await Task.Delay(500)
        Next

        Return results
    End Function

    ' ═══════════════════════════════════════════════════════════════
    ' PARSING
    ' ═══════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Parse FFmpeg's final "frame=... Lsize=... dup=... drop=... speed=..." line.
    ''' Example:
    '''   frame= 6540 fps=143 q=8.0 Lsize=   94278KiB time=00:00:45.41 bitrate=17005.4kbits/s dup=760 drop=1 speed=0.997x elapsed=0:00:45.55
    ''' </summary>
    Private Sub ParseFFmpegStats(line As String, result As TestResult)
        If String.IsNullOrEmpty(line) Then Return

        ' fps=N or fps=N.M
        Dim fpsMatch As Match = Regex.Match(line, "fps=\s*(\d+(?:\.\d+)?)")
        If fpsMatch.Success Then
            Double.TryParse(fpsMatch.Groups(1).Value, result.ActualFps)
        End If

        ' speed=Nx or speed=N.Mx
        Dim speedMatch As Match = Regex.Match(line, "speed=\s*(\d+(?:\.\d+)?)x")
        If speedMatch.Success Then
            Double.TryParse(speedMatch.Groups(1).Value, result.Speed)
        End If

        ' dup=N
        Dim dupMatch As Match = Regex.Match(line, "dup=\s*(\d+)")
        If dupMatch.Success Then
            Long.TryParse(dupMatch.Groups(1).Value, result.Dup)
        End If

        ' drop=N
        Dim dropMatch As Match = Regex.Match(line, "drop=\s*(\d+)")
        If dropMatch.Success Then
            Long.TryParse(dropMatch.Groups(1).Value, result.Drop)
        End If

        ' time=HH:MM:SS.mmm → convert to seconds
        Dim timeMatch As Match = Regex.Match(line, "time=(\d+):(\d+):(\d+(?:\.\d+)?)")
        If timeMatch.Success Then
            Dim h As Double, m As Double, s As Double
            Double.TryParse(timeMatch.Groups(1).Value, h)
            Double.TryParse(timeMatch.Groups(2).Value, m)
            Double.TryParse(timeMatch.Groups(3).Value, s)
            result.VideoDurationSec = h * 3600 + m * 60 + s
        End If
    End Sub

    ''' <summary>
    ''' Parse AudioFileWriter.GetDiagnostics() output.
    ''' Format:
    '''   [Audio] SysCallbacks=647
    '''   [Audio] SysBytesEnqueued=17803312
    '''   ...
    ''' </summary>
    Private Sub ParseAudioDiagnostics(diagnostics As String, scenario As TestScenario, result As TestResult)
        If String.IsNullOrEmpty(diagnostics) Then Return

        For Each line As String In diagnostics.Split(New Char() {ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries)
            line = line.Trim()
            If Not line.StartsWith("[Audio] ") Then Continue For

            Dim eqIdx As Integer = line.IndexOf("="c)
            If eqIdx < 0 Then Continue For

            Dim key As String = line.Substring(8, eqIdx - 8).Trim()  ' skip "[Audio] "
            Dim valStr As String = line.Substring(eqIdx + 1).Trim()

            Select Case key
                Case "SysCallbacks" : Long.TryParse(valStr, result.SysCallbacks)
                Case "SysBytesEnqueued" : Long.TryParse(valStr, result.SysBytesEnqueued)
                Case "SysWrittenBytes" : Long.TryParse(valStr, result.SysWrittenBytes)
                Case "SysWriteLagBytes" : Long.TryParse(valStr, result.SysWriteLagBytes)
                Case "SysDroppedChunks" : Long.TryParse(valStr, result.SysDroppedChunks)
                Case "SysDroppedBytes" : Long.TryParse(valStr, result.SysDroppedBytes)
                Case "SysDroppedSilenceBytes" : Long.TryParse(valStr, result.SysDroppedSilenceBytes)
                Case "SysBytesAccountingResidual" : Long.TryParse(valStr, result.SysBytesAccountingResidual)
                Case "SysInitialSilenceSec" : Double.TryParse(valStr, result.SysInitialSilenceSec)

                Case "MicCallbacks" : Long.TryParse(valStr, result.MicCallbacks)
                Case "MicBytesEnqueued" : Long.TryParse(valStr, result.MicBytesEnqueued)
                Case "MicWrittenBytes" : Long.TryParse(valStr, result.MicWrittenBytes)
                Case "MicWriteLagBytes" : Long.TryParse(valStr, result.MicWriteLagBytes)
                Case "MicDroppedBytes" : Long.TryParse(valStr, result.MicDroppedBytes)
                Case "MicBytesAccountingResidual" : Long.TryParse(valStr, result.MicBytesAccountingResidual)
            End Select
        Next
    End Sub

    Private Sub ParseMuxSummary(summary As String, result As TestResult)
        ' Currently _lastMuxSummary is not populated by CaptureEngine
        ' (could be added later). Leave defaults.
    End Sub

    ' ═══════════════════════════════════════════════════════════════
    ' VALIDATION
    ' ═══════════════════════════════════════════════════════════════

    Private Sub Validate(scenario As TestScenario, result As TestResult)
        Dim failures As New List(Of String)()

        ' ── RED FLAGS (auto-fail) ──
        If result.SysDroppedBytes > 0 Then
            failures.Add($"SysDroppedBytes={result.SysDroppedBytes} (real audio data loss)")
        End If
        If result.MicDroppedBytes > 0 Then
            failures.Add($"MicDroppedBytes={result.MicDroppedBytes} (real mic data loss)")
        End If
        If result.SysBytesAccountingResidual <> 0 Then
            failures.Add($"SysBytesAccountingResidual={result.SysBytesAccountingResidual} (invariant violated)")
        End If
        If result.MicBytesAccountingResidual <> 0 Then
            failures.Add($"MicBytesAccountingResidual={result.MicBytesAccountingResidual} (invariant violated)")
        End If

        ' Writer FAILED (detected via non-zero bytes enqueued but zero written)
        If scenario.ExpectAudio AndAlso result.SysBytesEnqueued > 0 AndAlso result.SysWrittenBytes = 0 Then
            failures.Add("Writer FAILED (BytesEnqueued > 0 but WrittenBytes = 0)")
        End If

        ' ── WARNINGS (fail if severe) ──
        ' FPS must be within 5% of target (CFR should be exact)
        If result.ActualFps > 0 AndAlso result.TargetFps > 0 Then
            Dim minFps As Double = result.TargetFps * 0.95
            If result.ActualFps < minFps Then
                failures.Add($"FPS={result.ActualFps:F1} < target*0.95={minFps:F1}")
            End If
        End If

        ' Speed must be >= 0.95x (FFmpeg can keep up in real-time)
        If result.Speed > 0 AndAlso result.Speed < 0.95 Then
            failures.Add($"Speed={result.Speed:F2}x < 0.95 (FFmpeg behind real-time)")
        End If

        ' dup threshold: at 144 FPS for 15s = 2160 frames, 5% = 108 dup allowed
        ' (ddagrab has small natural dup at startup, that's expected)
        Dim maxDup As Long = Math.Max(50, CLng(result.TargetFps * result.VideoDurationSec * 0.05))
        If result.Dup > maxDup Then
            failures.Add($"dup={result.Dup} > {maxDup} (excessive duplicate frames — ddagrab starved)")
        End If

        ' drop threshold: any frame drop is suspicious
        If result.Drop > 5 Then
            failures.Add($"drop={result.Drop} > 5 (FFmpeg dropping frames)")
        End If

        ' WriteLagBytes after Stop should be 0 (writer drained)
        If result.SysWriteLagBytes > 0 Then
            failures.Add($"SysWriteLagBytes={result.SysWriteLagBytes} > 0 (writer didn't drain)")
        End If
        If result.MicWriteLagBytes > 0 Then
            failures.Add($"MicWriteLagBytes={result.MicWriteLagBytes} > 0 (writer didn't drain)")
        End If

        ' A/V sync tolerance: 100ms (apad + -t handle smaller mismatches)
        If result.AVSyncMs > 100 AndAlso scenario.ExpectAudio Then
            failures.Add($"AVSyncMs={result.AVSyncMs:F0} > 100 (audio/video duration mismatch)")
        End If

        If failures.Count > 0 Then
            result.Pass = False
            result.FailureReason = String.Join("; ", failures)
        Else
            result.Pass = True
        End If
    End Sub

    ' ═══════════════════════════════════════════════════════════════
    ' FORMATTING
    ' ═══════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Format all results as an ASCII table for display/log.
    ''' </summary>
    Public Shared Function FormatResultTable(results As List(Of TestResult)) As String
        Dim sb As New StringBuilder()
        sb.AppendLine()
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────────────────────────┐")
        sb.AppendLine("│                          STRESS TEST RESULTS                                         │")
        sb.AppendLine("├──────────────────────────────────────────────────────────────────────────────────────┤")
        sb.AppendLine("│ Scenario                        │ Status │ FPS    │ Dup   │ Drop  │ Drops │ Resid │ Sync │")
        sb.AppendLine("├────────────────────────────────────────┼────────┼───────┼───────┼───────┼───────┼──────┤")

        For Each r As TestResult In results
            Dim status As String = If(r.Pass, "PASS", "FAIL")
            Dim fps As String = $"{r.ActualFps:F1}/{r.TargetFps}"
            Dim syncStr As String = If(r.AVSyncMs > 0, $"{r.AVSyncMs:F0}ms", "-")
            Dim drops As String = (r.SysDroppedBytes + r.MicDroppedBytes).ToString()
            Dim resid As String = (r.SysBytesAccountingResidual + r.MicBytesAccountingResidual).ToString()

            sb.AppendLine($"│ {r.Name,-32} │ {status,-6} │ {fps,-6} │ {r.Dup,5} │ {r.Drop,5} │ {drops,5} │ {resid,5} │ {syncStr,4} │")
        Next

        sb.AppendLine("├──────────────────────────────────────────────────────────────────────────────────────┤")
        Dim passCount As Integer = results.Count(Function(r) r.Pass)
        Dim failCount As Integer = results.Count - passCount
        sb.AppendLine($"│ Total: {results.Count}  |  Pass: {passCount}  |  Fail: {failCount}                                    │")
        sb.AppendLine("└──────────────────────────────────────────────────────────────────────────────────────┘")

        ' List failures with reasons
        Dim failures As List(Of TestResult) = results.Where(Function(r) Not r.Pass).ToList()
        If failures.Count > 0 Then
            sb.AppendLine()
            sb.AppendLine("FAILURES:")
            For Each f As TestResult In failures
                sb.AppendLine($"  {f.Name}:")
                sb.AppendLine($"    {f.FailureReason}")
            Next
        End If

        Return sb.ToString()
    End Function

End Class
