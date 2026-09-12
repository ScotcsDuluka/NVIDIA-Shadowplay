Option Strict On
Option Explicit On
Option Infer On

' Program.vb — Phase 4B Timing Causality A/B driver (EXPERIMENT — production untouched).
'
' Proves end-to-end how the production 10ms timeline delay
' (CaptureSession.vb:279, `_timelineStartTicks = ... + Stopwatch.Frequency \ 10`)
' propagates through the focus chain:
'
'   Capture → Queue → CFR → NVENC → Packet → Mux → MP4 PTS
'
' Arms:
'   --arm on   : production delay (Stopwatch.Frequency \ 10)
'   --arm off  : no delay (0) — the Phase 4B hypothesis arm
' t0-modes:
'   --t0-mode arm  : production order (T0 armed at Run() entry, before mux/capture start)
'   --t0-mode loop : T0 re-armed at CFR-loop entry (producers genuinely armed
'                    before T0 — the design intent stated by the production log)
'
' Backends:
'   --backend synthetic (default) : real-QPC synthetic capture + canned real-H264
'                                   encoder — runs on ANY machine incl. Intel.
'   --backend production          : real DdagrabBackend + NvencEncoderBackend —
'                                   NVIDIA machine only (runbook).
'
' Per run this driver records: the pre-run origin stamp (Stopwatch ticks +
' QPC 100ns), the full session log (all forensic instrumentation), the
' SessionResult, and ffprobe facts of the final MP4 (first video PTS,
' start_time, duration, packet count).

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.Recording
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports Phase4b

Module Program

    Private _logLock As New Object()
    Private _logWriter As StreamWriter = Nothing
    Private _summaryCsv As StringBuilder = New StringBuilder()
    Private _evidenceMd As StringBuilder = New StringBuilder()

    Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8

        Dim armArg As String = "both"
        Dim runs As Integer = 3
        Dim durationSec As Integer = 3
        Dim fps As Integer = 60
        Dim warmupMs As Integer = 28      ' Phase-4 measured first-frame acquisition residual
        Dim frameMs As Double = 1000.0 / 60.0
        Dim width As Integer = 640
        Dim height As Integer = 360
        Dim t0ModeArg As String = "both"
        Dim backend As String = "synthetic"
        Dim outDir As String = ""
        Dim ffmpegPath As String = "ffmpeg"

        For i As Integer = 0 To args.Length - 2
            Dim v As String = args(i + 1)
            Select Case args(i)
                Case "--arm" : armArg = v
                Case "--runs" : Integer.TryParse(v, runs)
                Case "--duration" : Integer.TryParse(v, durationSec)
                Case "--fps" : Integer.TryParse(v, fps)
                Case "--warmup-ms" : Integer.TryParse(v, warmupMs)
                Case "--frame-ms" : Double.TryParse(v, frameMs)
                Case "--width" : Integer.TryParse(v, width)
                Case "--height" : Integer.TryParse(v, height)
                Case "--t0-mode" : t0ModeArg = v
                Case "--backend" : backend = v
                Case "--out" : outDir = v
                Case "--ffmpeg" : ffmpegPath = v
            End Select
        Next

        If ffmpegPath = "ffmpeg" Then
            Dim candidates As String() = {
                Path.Combine(AppContext.BaseDirectory, "API-Core", "ffmpeg.exe"),
                "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe",
                "C:\My Project\NVIDIA-Shadowplay\Overlay\bin\Release\net10.0-windows10.0.26100.0\FFmpeg\ffmpeg.exe"
            }
            For Each c In candidates
                If File.Exists(c) Then ffmpegPath = c : Exit For
            Next
        End If
        If Not File.Exists(ffmpegPath) Then
            Console.Error.WriteLine("*** FATAL: ffmpeg not found — pass --ffmpeg <path>")
            Return 2
        End If
        Dim ffprobePath As String = Path.Combine(Path.GetDirectoryName(ffmpegPath), "ffprobe.exe")
        If Not File.Exists(ffprobePath) Then ffprobePath = "ffprobe"

        If String.IsNullOrEmpty(outDir) Then
            outDir = "phase4b-evidence\" & DateTime.Now.ToString("yyyyMMdd-HHmmss")
        End If
        Directory.CreateDirectory(outDir)

        Dim logPath As String = Path.Combine(outDir, "phase4b-driver.log")
        _logWriter = New StreamWriter(logPath, append:=False) With {.AutoFlush = True}
        Dim logger As New EngineLogger("Phase4b", EngineLogger.LogLevel.Info,
                                       Sub(line)
                                           SyncLock _logLock
                                               Console.WriteLine(line)
                                               _logWriter.WriteLine(line)
                                           End SyncLock
                                       End Sub)

        logger.Info("============================================================")
        logger.Info(" Phase 4B — Timing Causality A/B (production pipeline fork)")
        logger.Info("============================================================")
        logger.Info($"[phase4b] machine={Environment.MachineName} os={Environment.OSVersion}")
        logger.Info($"[phase4b] cpu={Environment.ProcessorCount} cores, 64bit={Environment.Is64BitProcess}")
        logger.Info($"[phase4b] stopwatch frequency={Stopwatch.Frequency} Hz")
        logger.Info($"[phase4b] args: arm={armArg} runs={runs} duration={durationSec}s fps={fps} warmupMs={warmupMs} frameMs={frameMs:0.###} t0Mode={t0ModeArg} backend={backend}")
        logger.Info($"[phase4b] ffmpeg={ffmpegPath}")
        logger.Info($"[phase4b] outDir={Path.GetFullPath(outDir)}")

        _summaryCsv.AppendLine("run,arm,t0mode,mp4,pass,frames_captured,frames_encoded,frames_duplicated,nvenc_errors,video_bytes,file_size,mux_duration_s,mp4_start_time,mp4_first_pts,mp4_packets,actual_duration_s")

        Dim exitCode As Integer = 0

        Dim t0Modes As New List(Of String)()
        Select Case t0ModeArg
            Case "arm" : t0Modes.Add("arm")
            Case "loop" : t0Modes.Add("loop")
            Case Else : t0Modes.Add("arm") : t0Modes.Add("loop")
        End Select

        Dim arms As New List(Of String)()
        Select Case armArg
            Case "on" : arms.Add("on")
            Case "on10" : arms.Add("on10")
            Case "off" : arms.Add("off")
            Case "onoff" : arms.Add("on") : arms.Add("off")
            Case Else : arms.Add("on") : arms.Add("on10") : arms.Add("off")
        End Select

        ' Interleave arms per t0-mode (on,off,on,off...) so thermal/load drift
        ' hits both arms equally instead of biasing one block.
        For Each t0Mode In t0Modes
            Dim interleaved As New List(Of String)()
            For r As Integer = 1 To runs
                For Each a In arms : interleaved.Add($"{a}-{r}") : Next
            Next

            ' One backend set per t0-mode block (production model: persistent
            ' backends reused across sessions).
            Dim capture As IPhase4bVideoBackend = Nothing
            Dim encoder As IEncoderBackend = Nothing
            Try
                If backend = "production" Then
                    Dim realCapture As New DdagrabBackend(logger)
                    realCapture.Initialize(New Phase4bVideoBackendContext(logger))
                    capture = New Phase4bDdagrabAdapter(realCapture)

                    Dim realEncoder As New NvencEncoderBackend(logger)
                    Dim encCfg As New EncoderConfig() With {
                        .CodecKey = "NVENC_H264",
                        .BitrateBps = 20_000_000L,
                        .GopSize = 60,
                        .RateControl = "cbr",
                        .Preset = "p4",
                        .FrameRateFps = fps,
                        .ExpectedWidth = width,
                        .ExpectedHeight = height
                    }
                    realEncoder.Initialize(encCfg)
                    encoder = realEncoder
                Else
                    Dim synCapture As New Phase4bSyntheticCapture(logger, warmupMs, frameMs, width, height)
                    synCapture.Initialize(New Phase4bVideoBackendContext(logger))
                    capture = synCapture

                    Dim canned As New Phase4bCannedEncoder(logger)
                    canned.Initialize(New EncoderConfig() With {
                        .CodecKey = "NVENC_H264",
                        .FrameRateFps = fps,
                        .ExpectedWidth = width,
                        .ExpectedHeight = height
                    })
                    encoder = canned
                End If

                ' ── JIT/allocator warm-up run (discarded) — production warms
                ' up at engine init; without this, run 1 of the first block
                ' carries a one-time ~300ms JIT bias that pollutes the A/B.
                Using warmSession As New Phase4bCaptureSession(
                        capture, encoder,
                        New SessionConfig() With {
                            .OutputPath = Path.Combine(outDir, $"phase4b-{t0Mode}-warmup.discarded.mp4"),
                            .DurationSeconds = 1,
                            .FFmpegPath = ffmpegPath,
                            .AudioEnabled = False,
                            .TargetFps = fps,
                            .UseNativeResolution = False,
                            .EncodeWidth = width,
                            .EncodeHeight = height
                        },
                        logger, 0L, False)
                    Dim warmRes = warmSession.Run()
                    logger.Info($"[phase4b] WARMUP run discarded (pass={warmRes.Pass}, frames={warmRes.FramesEncoded})")
                    Try : File.Delete(Path.Combine(outDir, $"phase4b-{t0Mode}-warmup.discarded.mp4")) : Catch : End Try
                End Using

                For Each runLabel As String In interleaved
                    Dim armName As String = runLabel.Split("-"c)(0)
                    Dim runNo As Integer = CInt(runLabel.Split("-"c)(1))
                    Dim delayTicks As Long
                    Select Case armName
                        Case "on" : delayTicks = Stopwatch.Frequency \ 10    ' production line 279
                        Case "on10" : delayTicks = Stopwatch.Frequency \ 100 ' the "10ms" the docs hypothesized
                        Case Else : delayTicks = 0L                          ' no-delay arm
                    End Select
                    Dim t0AtLoop As Boolean = (t0Mode = "loop")
                    Dim mp4Name As String = $"phase4b-{t0Mode}-{armName}{runNo}.mp4"
                    Dim mp4Path As String = Path.Combine(outDir, mp4Name)

                    Dim preRunTicks As Long = Stopwatch.GetTimestamp()
                    Dim preRunQpc100ns As Long = Phase4bSyntheticCapture.TicksTo100ns(preRunTicks)
                    logger.Info($"[phase4b] RUN {t0Mode}-{runLabel} START preRunTicks={preRunTicks} preRunQpc100ns={preRunQpc100ns} delayTicks={delayTicks} t0AtLoop={t0AtLoop}")

                    Dim cfg As New SessionConfig() With {
                        .OutputPath = mp4Path,
                        .DurationSeconds = durationSec,
                        .FFmpegPath = ffmpegPath,
                        .AudioEnabled = False,
                        .TargetFps = fps,
                        .UseNativeResolution = False,
                        .RequestedWidth = width,
                        .RequestedHeight = height,
                        .EncodeWidth = width,
                        .EncodeHeight = height
                    }

                    Dim result As SessionResult
                    Using session As New Phase4bCaptureSession(capture, encoder, cfg, logger, delayTicks, t0AtLoop)
                        result = session.Run()
                    End Using
                    Dim postRunTicks As Long = Stopwatch.GetTimestamp()
                    logger.Info($"[phase4b] RUN {t0Mode}-{runLabel} END postRunTicks={postRunTicks} wallMs={(postRunTicks - preRunTicks) * 1000.0 / Stopwatch.Frequency:0.###}")

                    ' ── ffprobe facts ──
                    Dim firstPts As String = ProbeFirstVideoPts(ffprobePath, mp4Path)
                    Dim startTime As String = ProbeFormat(ffprobePath, mp4Path, "start_time")
                    Dim fmtDuration As String = ProbeFormat(ffprobePath, mp4Path, "duration")
                    Dim packetCount As String = ProbePacketCount(ffprobePath, mp4Path)
                    Dim firstPtsNum As Double = 0.0
                    Double.TryParse(firstPts, Globalization.NumberStyles.Float,
                                    Globalization.CultureInfo.InvariantCulture, firstPtsNum)

                    logger.Info($"[phase4b] PROBE {mp4Name}: first_video_pts={firstPts}s start_time={startTime}s duration={fmtDuration}s video_packets={packetCount}")

                    Dim csvLine As String = $"{t0Mode}-{runLabel},{armName},{t0Mode},{mp4Name},{result.Pass},{result.FramesCaptured},{result.FramesEncoded},{result.FramesDuplicated},{result.NvencErrors},{result.TotalVideoBytes},{result.FileSize},{result.MuxVideoDurationSec:0.000},{startTime},{firstPts},{packetCount},{result.ActualDurationSec:0.000}"
                    _summaryCsv.AppendLine(csvLine)

                    _evidenceMd.AppendLine($"### run {t0Mode}-{runLabel}")
                    _evidenceMd.AppendLine($"- pass={result.Pass} framesEncoded={result.FramesEncoded} (captured={result.FramesCaptured}, dup={result.FramesDuplicated}) nvencErrors={result.NvencErrors}")
                    _evidenceMd.AppendLine($"- mp4: first_video_pts={firstPts}s start_time={startTime}s duration={fmtDuration}s packets={packetCount} size={result.FileSize}B")
                    _evidenceMd.AppendLine($"- log: {mp4Name.Replace(".mp4", ".log")}")
                    _evidenceMd.AppendLine()

                    If Not result.Pass Then exitCode = 1
                Next
            Catch ex As Exception
                logger.Error($"[phase4b] FATAL: {ex.Message}", ex)
                _evidenceMd.AppendLine($"**FATAL ({t0Mode}): {ex.Message}**")
                exitCode = 3
            Finally
                If TypeOf capture Is IDisposable Then
                    Try : DirectCast(capture, IDisposable).Dispose() : Catch : End Try
                End If
                If TypeOf encoder Is IDisposable Then
                    Try : DirectCast(encoder, IDisposable).Dispose() : Catch : End Try
                End If
            End Try
        Next

        ' ── Evidence files ──
        File.WriteAllText(Path.Combine(outDir, "phase4b-summary.csv"), _summaryCsv.ToString())
        Dim md As New StringBuilder()
        md.AppendLine("# Phase 4B — Raw Run Evidence")
        md.AppendLine()
        md.AppendLine($"- date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        md.AppendLine($"- machine: {Environment.MachineName} · {Environment.OSVersion}")
        md.AppendLine($"- stopwatch frequency: {Stopwatch.Frequency} Hz")
        md.AppendLine($"- args: arm={armArg} runs={runs} duration={durationSec}s fps={fps} warmupMs={warmupMs} frameMs={frameMs:0.###} t0Mode={t0ModeArg} backend={backend}")
        md.AppendLine($"- ffmpeg: `{ffmpegPath}`")
        md.AppendLine($"- production HEAD: cca2b2d (Engine-Rebuild-Stabilization) — production sources untouched")
        md.AppendLine()
        md.Append(_evidenceMd.ToString())
        File.WriteAllText(Path.Combine(outDir, "phase4b-evidence.md"), md.ToString())

        logger.Info($"[phase4b] evidence: {Path.GetFullPath(Path.Combine(outDir, "phase4b-evidence.md"))}")
        logger.Info($"[phase4b] summary: {Path.GetFullPath(Path.Combine(outDir, "phase4b-summary.csv"))}")
        logger.Info($"[phase4b] exit={exitCode}")

        _logWriter.Dispose()
        Return exitCode
    End Function

    Private Function ProbeFirstVideoPts(ffprobePath As String, mp4Path As String) As String
        Return RunCapture(ffprobePath,
            $"-v error -select_streams v:0 -show_entries packet=pts_time -of csv=p=0 -read_intervals %+#3 ""{mp4Path}""")
    End Function

    Private Function ProbeFormat(ffprobePath As String, mp4Path As String, entry As String) As String
        Return RunCapture(ffprobePath,
            $"-v error -show_entries format={entry} -of csv=p=0 ""{mp4Path}""")
    End Function

    Private Function ProbePacketCount(ffprobePath As String, mp4Path As String) As String
        Return RunCapture(ffprobePath,
            $"-v error -select_streams v:0 -count_packets -show_entries stream=nb_read_packets -of csv=p=0 ""{mp4Path}""")
    End Function

    Private Function RunCapture(exe As String, args As String) As String
        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = exe,
                .Arguments = args,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .CreateNoWindow = True
            }
            Using p As Process = Process.Start(psi)
                Dim outp As String = p.StandardOutput.ReadToEnd().Trim()
                p.StandardError.ReadToEnd()
                If Not p.WaitForExit(10000) Then
                    Try : p.Kill() : Catch : End Try
                    Return "timeout"
                End If
                Dim lines As String() = outp.Split({ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries)
                Return If(lines.Length > 0, lines(0), "n/a")
            End Using
        Catch ex As Exception
            Return "err: " & ex.Message
        End Try
    End Function

End Module
