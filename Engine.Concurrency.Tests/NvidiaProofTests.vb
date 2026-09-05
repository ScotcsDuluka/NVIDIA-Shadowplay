Option Strict On
Option Explicit On
Option Infer On

' NvidiaProofTests.vb — C/1 NVIDIA RUNTIME PROOF for the LEGACY failure
' lifecycle (machine: GTX 1080 Ti — h264_nvenc verified available).
'
' Everything here runs the REAL production record path: real ddagrab (DXGI
' duplication) + real h264_nvenc encode + real WASAPI two-process mux, and
' then applies DETERMINISTIC failures once real media exists:
'
'   NV-H1B  real NVENC recording → controlled mux failure (a DIRECTORY is
'           created at the output path, so the real mux cannot open it) →
'           StopRecordingAsync → terminal truth (exactly-once events, Idle,
'           not-saved semantics) → REAL MEDIA PROOF: the temp video produced
'           by the real NVENC pipeline is probed (duration + h264 stream)
'           and the capture log proves "h264 (h264_nvenc)" ran in the encode
'           chain. "NVENC initialized" alone is never accepted as proof.
'
'   NV-F03  a real NVENC recording is taken with a NORMAL stop (valid saved
'           MP4 = ground truth), that real media is truncated into a
'           moov-less file (what a crashed finalize leaves), and a second
'           session feeds it through the REAL mux (delegated) → fallback →
'           ValidatePlayback → must NOT report Saved.
'
'   NV-STRESS ≥10 real mixed cycles: normal start/stop, controlled mux
'           failure, retry after failure, stop-after-failure, dispose —
'           with orphan + temp hygiene checks.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports NVIDIA_Capture
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace Engine.Concurrency.Tests

    Friend Module NvidiaProofTests

        Private _ffmpeg As String = ""
        Private _ffprobe As String = ""
        Private _sandbox As String = ""
        Private _engineLog As String = ""

        Friend Sub RunAll(ffmpegPath As String, sandboxDir As String)
            _ffmpeg = ffmpegPath
            _sandbox = sandboxDir

            ' ffprobe sits next to the real ffmpeg in the deployment.
            _ffprobe = IO.Path.Combine(IO.Path.GetDirectoryName(_ffmpeg), "ffprobe.exe")
            _engineLog = IO.Path.Combine(AppLayout.P("Logs"), "capture-engine.log")

            ' NVENC must be usable on THIS machine, or the proof is void.
            Dim smoke As Integer = RunReal("-f lavfi -i testsrc=duration=0.3:size=320x240:rate=15 " &
                                           "-c:v h264_nvenc -f null -", 30000)
            If smoke <> 0 Then
                Console.WriteLine($"(NVIDIA proof tests skipped — h264_nvenc smoke encode failed, exit {smoke})")
                Return
            End If

            Console.WriteLine($"      [NV] GPU proof: real ffmpeg = {_ffmpeg}")
            Console.WriteLine($"      [NV] NVENC smoke encode OK; engine log = {_engineLog}")

            TestRunner.RunTest("NV-H1B: real NVENC recording → controlled mux failure → terminal truth + real-media proof",
                               AddressOf Test_RealNvencMuxFailure)
            TestRunner.RunTest("NV-F03: real NVENC media, truncated → real mux → ValidatePlayback must NOT save",
                               AddressOf Test_RealMediaTruncatedFallback)
            TestRunner.RunTest("NV-STRESS: 10 mixed real NVENC cycles (normal/failure/retry/dispose) — zero orphans",
                               AddressOf Test_NvencStress10)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Plumbing
        ' ───────────────────────────────────────────────────────────────

        Private NotInheritable Class NvRec
            Public ReadOnly Stopped As New List(Of String)()
            Public ReadOnly Errors As New List(Of String)()
            Public ReadOnly Started As New List(Of String)()

            Public Sub Wire(engine As EngineCapture)
                AddHandler engine.RecordingStarted, Sub(f) LockAdd(Started, f)
                AddHandler engine.RecordingStopped, Sub(f) LockAdd(Stopped, f)
                AddHandler engine.ErrorOccurred, Sub(m) LockAdd(Errors, m)
            End Sub

            Private Shared Sub LockAdd(list As List(Of String), item As String)
                SyncLock list : list.Add(item) : End SyncLock
            End Sub

            Public Function StoppedCount() As Integer
                SyncLock Stopped : Return Stopped.Count : End SyncLock
            End Function

            Public Function ErrorCount() As Integer
                SyncLock Errors : Return Errors.Count : End SyncLock
            End Function
        End Class

        Private Function MakeRealSettings(sandbox As String) As CaptureSettings
            Dim s As New CaptureSettings()
            s.FFmpegPath = _ffmpeg                  ' REAL ffmpeg — real ddagrab + real h264_nvenc
            s.Encoder = "h264_nvenc"
            s.CaptureMethod = "ddagrab"
            s.FPS = 15
            s.Bitrate = 2000000L
            s.UseNativeResolution = True
            s.OutputDirectory = sandbox
            s.SystemAudioCapture = True             ' two-process path: real WASAPI + real mux
            s.MicCapture = False
            Return s
        End Function

        ''' <summary>Start a REAL NVENC recording, retrying on fresh engines if
        ''' the environment (e.g. DXGI contention from parallel agents) refuses
        ''' the capture. Returns Nothing if all attempts fail to start.</summary>
        Private Function StartRealRecording(sandbox As String, outPath As String, rec As NvRec,
                                            Optional logOffsetBefore As Long = -1) As EngineCapture
            For attempt As Integer = 1 To 3
                Dim engine As New EngineCapture(MakeRealSettings(sandbox))
                rec.Wire(engine)
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                If started Then
                    TestRunner.Assert(engine.State = EngineCapture.CaptureState.Recording,
                                      $"state after start = {engine.State}")
                    Return engine
                End If
                engine.Dispose()
                Thread.Sleep(1000)   ' DXGI/driver contention backoff
            Next
            Return Nothing
        End Function

        Private Function WaitFor(condition As Func(Of Boolean), timeoutMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < timeoutMs
                If condition() Then Return True
                Thread.Sleep(250)
            End While
            Return condition()
        End Function

        ''' <summary>Helper-exe settings — used ONLY by the NV-F03 phase 3
        ''' session (record-copy of the truncated REAL NVENC media), while the
        ''' real mux/validation verdicts are delegated to the real ffmpeg.</summary>
        Private Function MakeSettings(sandbox As String) As CaptureSettings
            Dim s As New CaptureSettings()
            s.FFmpegPath = Environment.ProcessPath
            s.Encoder = "libx264"
            s.CaptureMethod = "ddagrab"
            s.FPS = 15
            s.Bitrate = 2000000L
            s.UseNativeResolution = True
            s.OutputDirectory = sandbox
            s.SystemAudioCapture = True
            s.MicCapture = False
            Return s
        End Function

        Private Function RunReal(args As String, timeoutMs As Integer) As Integer
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg, .Arguments = args,
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True, .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                p.StandardError.ReadToEnd()
                If Not p.WaitForExit(timeoutMs) Then
                    Try : p.Kill() : Catch : End Try
                    Return -999
                End If
                Return p.ExitCode
            End Using
        End Function

        ''' <summary>Media proof: ffprobe (real deployment) → codec + duration;
        ''' falls back to the ffmpeg -i report. Returns the probe text.</summary>
        Private Function ProbeMedia(path As String) As String
            If IO.File.Exists(_ffprobe) Then
                Dim psi As New ProcessStartInfo With {
                    .FileName = _ffprobe,
                    .Arguments = "-v error -show_entries format=duration -show_entries stream=codec_name,codec_type " &
                                 "-of default=noprint_wrappers=1 """ & path & """",
                    .UseShellExecute = False, .CreateNoWindow = True,
                    .RedirectStandardError = True, .RedirectStandardOutput = True
                }
                Using p As Process = Process.Start(psi)
                    Dim outTask = p.StandardOutput.ReadToEndAsync()
                    If Not p.WaitForExit(15000) Then
                        Try : p.Kill() : Catch : End Try
                    End If
                    outTask.Wait(2000)
                    Return If(outTask.Status = Threading.Tasks.TaskStatus.RanToCompletion, outTask.Result, "")
                End Using
            End If
            ' fallback: the ffmpeg -i report
            Dim psi2 As New ProcessStartInfo With {
                .FileName = _ffmpeg, .Arguments = "-hide_banner -i """ & path & """",
                .UseShellExecute = False, .CreateNoWindow = True, .RedirectStandardError = True
            }
            Using p As Process = Process.Start(psi2)
                Dim errTask = p.StandardError.ReadToEndAsync()
                p.WaitForExit(15000)
                errTask.Wait(2000)
                Return If(errTask.Status = Threading.Tasks.TaskStatus.RanToCompletion, errTask.Result, "")
            End Using
        End Function

        ''' <summary>Read the engine debug log appended since <paramref name="offset"/>.
        ''' The engine mirrors every ffmpeg stderr line into this log, so the
        ''' encode chain (h264_nvenc) is provable from the real session output.</summary>
        Private Function ReadLogTail(offset As Long) As String
            Try
                If Not IO.File.Exists(_engineLog) Then Return ""
                Using fs As New IO.FileStream(_engineLog, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                    If fs.Length <= offset Then Return ""
                    fs.Seek(offset, IO.SeekOrigin.Begin)
                    Using sr As New IO.StreamReader(fs)
                        Return sr.ReadToEnd()
                    End Using
                End Using
            Catch
                Return ""
            End Try
        End Function

        Private Function LogLength() As Long
            Try
                If IO.File.Exists(_engineLog) Then Return New IO.FileInfo(_engineLog).Length
            Catch
            End Try
            Return 0
        End Function

        Private Function HelperProcesses() As Integer
            Dim exeName As String = IO.Path.GetFileNameWithoutExtension(Environment.ProcessPath)
            Return MediaAssert.ScopedProcessCount(exeName, _sandbox)
        End Function

        Private Function FfmpegCount() As Integer
            Return MediaAssert.ScopedFfmpegOrphans(_sandbox)
        End Function

        Private Sub SetEnv(name As String, value As String)
            Environment.SetEnvironmentVariable(name, value)
        End Sub

        Private Sub ClearHelperEnv()
            SetEnv("LMHLP_SRC", Nothing)
            SetEnv("LMHLP_COPYTO", Nothing)
            SetEnv("LMHLP_SLEEP", Nothing)
            SetEnv("LMHLP_EXIT", Nothing)
            SetEnv("LMHLP_MUX_EXIT", Nothing)
            SetEnv("LMHLP_MUX_SRC", Nothing)
            SetEnv("LMHLP_MUX_REAL", Nothing)
            SetEnv("LMHLP_PROBE_REAL", Nothing)
            SetEnv("LMHLP_PROBE_EXIT", Nothing)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' NV-H1B
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>REAL NVENC recording → controlled mux failure → the stop
        ''' flow must land Idle with exactly-one honesty error, no false save,
        ''' and the temp video must be REAL NVENC media (probe + log proof).</summary>
        Private Sub Test_RealNvencMuxFailure()
            Dim outPath As String = IO.Path.Combine(_sandbox, "nv_h1b.mp4")
            Dim tempVideo As String = IO.Path.Combine(_sandbox, "nv_h1b.video.tmp.mp4")
            Dim tempWav As String = IO.Path.Combine(_sandbox, "nv_h1b.system.tmp.wav")
            ClearHelperEnv()

            Dim rec As New NvRec()
            Dim logBefore As Long = LogLength()
            Dim engine As EngineCapture = StartRealRecording(_sandbox, outPath, rec, logBefore)
            TestRunner.Assert(engine IsNot Nothing, "real NVENC recording failed to start (3 attempts)")
            Try
                ' Let the real pipeline produce actual media (ddagrab frames → NVENC).
                Thread.Sleep(2500)

                ' ── Controlled mux failure: the mux output path becomes a
                ' directory — the real mux cannot open it and must fail.
                IO.Directory.CreateDirectory(outPath)

                Dim logAtStop As Long = LogLength()
                Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopped, "StopRecordingAsync returned False")

                ' ── Terminal truth (required assertions)
                TestRunner.Assert(Not engine.IsRecordingLifecycleActive,
                                  "lifecycle still active at terminal return")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"terminal state = {engine.State} (honesty path → Idle)")
                TestRunner.Assert(rec.StoppedCount() = 0,
                                  $"FALSE SAVED: RecordingStopped fired {rec.StoppedCount()}× without a valid output")
                TestRunner.Assert(rec.ErrorCount() = 1,
                                  $"expected exactly ONE honesty error, got {rec.ErrorCount()}: [{String.Join(" | ", rec.Errors)}]")
                Dim msg As String = ""
                SyncLock rec.Errors : msg = rec.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("not saved"), $"error should be the honesty error, got: {msg}")
                TestRunner.Assert(Not IO.File.Exists(outPath) OrElse IO.Directory.Exists(outPath),
                                  "output must not exist as a playable file (only the failure-directory remains)")

                ' ── REAL MEDIA PROOF: the temp video is the actual NVENC encode.
                TestRunner.Assert(IO.File.Exists(tempVideo),
                                  "temp video (real NVENC media) missing after the mux failure")
                Dim fi As New IO.FileInfo(tempVideo)
                TestRunner.Assert(fi.Length > 10000, $"real NVENC media suspiciously small: {fi.Length}B")

                Dim probe As String = ProbeMedia(tempVideo)
                TestRunner.Assert(Regex.IsMatch(probe, "codec_name=h264"),
                                  "probed temp video is not H.264: " & probe.Replace(vbCr, "").Replace(vbLf, " | "))
                Dim dur As Match = Regex.Match(probe, "duration=([\d\.]+)")
                TestRunner.Assert(dur.Success AndAlso CDbl(dur.Groups(1).Value) > 1.5,
                                  $"probed duration {If(dur.Success, dur.Groups(1).Value, "?")}s — real recording window was ~2.5s")

                ' ── NVENC chain proof from the real session stderr (mirrored
                ' into the engine log by WriteDebugLog): the encode mapping
                ' must show h264 (h264_nvenc) between start and stop.
                Dim sessionLog As String = ReadLogTail(logBefore)
                TestRunner.Assert(sessionLog.Contains("h264 (h264_nvenc)"),
                                  "engine log does not prove the h264_nvenc encode chain ran")

                ' Temp hygiene: the wav sidecar is intentionally kept by the
                ' fallback path ("stay behind for inspection") — document, and
                ' no OTHER temp flavor may exist.
                TestRunner.Assert(Not IO.File.Exists(tempVideo.Replace(".video.tmp.mp4", ".mic.tmp.wav")),
                                  "unexpected mic temp file (mic disabled)")

                ' Real media is the evidence for NV-F03 — keep this file.
                IO.File.Copy(tempVideo, IO.Path.Combine(_sandbox, "nv_real_media.mp4"), True)
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try

            ' Cleanup the scenario artifacts (media already copied aside).
            Try
                If IO.Directory.Exists(outPath) Then IO.Directory.Delete(outPath, True)
                If IO.File.Exists(tempVideo) Then IO.File.Delete(tempVideo)
                If IO.File.Exists(tempWav) Then IO.File.Delete(tempWav)
            Catch
            End Try

            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 0 AndAlso FfmpegCount() = 0, 8000),
                              "orphan helper/ffmpeg after NV-H1B")
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' NV-F03
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>Phase 1: a real NVENC recording with a NORMAL stop →
        ''' valid saved MP4 (ground truth). Phase 2: truncate that REAL NVENC
        ''' media into a moov-less file. Phase 3: feed it through the real mux
        ''' (delegated) → fallback → ValidatePlayback → must NOT save.</summary>
        Private Sub Test_RealMediaTruncatedFallback()
            Dim outPath As String = IO.Path.Combine(_sandbox, "nv_f03.mp4")
            Dim truncated As String = IO.Path.Combine(_sandbox, "nv_f03_truncated.mp4")
            ClearHelperEnv()

            ' ── Phase 1: real NVENC recording, normal stop — ground truth.
            Dim rec As New NvRec()
            Dim engine As EngineCapture = StartRealRecording(_sandbox, outPath, rec)
            TestRunner.Assert(engine IsNot Nothing, "phase-1 real recording failed to start")
            Try
                Thread.Sleep(2000)
                Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopped, "phase-1 stop returned False")
                TestRunner.Assert(rec.StoppedCount() = 1,
                                  $"phase-1 expected RecordingStopped exactly once, got {rec.StoppedCount()}")
                TestRunner.Assert(rec.ErrorCount() = 0,
                                  $"phase-1 success path must be error-free: [{String.Join(" | ", rec.Errors)}]")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"phase-1 state = {engine.State}")
                TestRunner.Assert(IO.File.Exists(outPath), "phase-1 output missing")
                TestRunner.Assert(Not IO.File.Exists(IO.Path.Combine(_sandbox, "nv_f03.video.tmp.mp4")),
                                  "phase-1 temp video survived a successful mux (temp hygiene)")

                Dim probe As String = ProbeMedia(outPath)
                TestRunner.Assert(Regex.IsMatch(probe, "codec_name=h264"),
                                  "phase-1 output is not H.264: " & probe.Replace(vbCr, "").Replace(vbLf, " | "))
                Dim dur As Match = Regex.Match(probe, "duration=([\d\.]+)")
                TestRunner.Assert(dur.Success AndAlso CDbl(dur.Groups(1).Value) > 1.0,
                                  "phase-1 output duration implausible for the recorded window")
            Finally
                engine.Dispose()
            End Try

            ' ── Phase 2: build the crashed-finalize stand-in from the REAL
            ' NVENC media. An in-flight recording file has its moov atom at
            ' the END (the +faststart rewrite is a finalizing pass a crash
            ' never reaches), so re-mux the media into tail-moov layout first,
            ' then truncate — the moov atom is gone, exactly like a crashed
            ' finalize leaves behind.
            Dim tailMoov As String = IO.Path.Combine(_sandbox, "nv_f03_tailmoov.mp4")
            TestRunner.Assert(RunReal("-y -i """ & outPath & """ -c copy """ & tailMoov & """", 30000) = 0,
                              "re-mux of the real NVENC media into tail-moov layout failed")
            Dim all As Byte() = IO.File.ReadAllBytes(tailMoov)
            Dim cut As Integer = CInt(all.Length * 0.6)
            Dim truncatedBytes(cut - 1) As Byte
            Buffer.BlockCopy(all, 0, truncatedBytes, 0, cut)
            IO.File.WriteAllBytes(truncated, truncatedBytes)

            ' Premise checks against the REAL verdicts each stage will see:
            ' the mux stream-copies the WHOLE file, playback validation decodes
            ' one second — valid media must pass both, truncated must fail both.
            Dim scratch1 As String = IO.Path.Combine(_sandbox, "nv_f03_p1.mp4")
            Dim scratch2 As String = IO.Path.Combine(_sandbox, "nv_f03_p2.mp4")
            TestRunner.Assert(RunReal("-v error -i """ & outPath & """ -c copy -y """ & scratch1 & """", 30000) = 0,
                              "premise broken: real mux rejected the VALID NVENC media")
            TestRunner.Assert(RunReal("-v error -i """ & truncated & """ -c copy -y """ & scratch2 & """", 30000) <> 0,
                              "premise broken: real mux accepted the TRUNCATED media")
            TestRunner.Assert(RunReal("-v error -i """ & truncated & """ -t 1 -f null -", 30000) <> 0,
                              "premise broken: playback validation accepted the TRUNCATED media")

            ' ── Phase 3: helper record-copy of the truncated REAL media →
            ' REAL mux (delegated) must fail → fallback → ValidatePlayback.
            Dim out2 As String = IO.Path.Combine(_sandbox, "nv_f03_b.mp4")
            Dim tempVideo As String = IO.Path.Combine(_sandbox, "nv_f03_b.video.tmp.mp4")
            SetEnv("LMHLP_SLEEP", "30")
            SetEnv("LMHLP_EXIT", "0")
            SetEnv("LMHLP_SRC", truncated)
            SetEnv("LMHLP_COPYTO", tempVideo)
            SetEnv("LMHLP_MUX_REAL", _ffmpeg)
            SetEnv("LMHLP_PROBE_REAL", _ffmpeg)

            Dim rec2 As New NvRec()
            Dim engine2 As New EngineCapture(MakeSettings(_sandbox))
            rec2.Wire(engine2)
            Try
                Dim started As Boolean = engine2.StartRecordingAsync(out2).GetAwaiter().GetResult()
                TestRunner.Assert(started, "phase-3 start returned False")
                TestRunner.Assert(WaitFor(Function() IO.File.Exists(tempVideo), 5000),
                                  "helper never produced the truncated temp video")

                Dim stopOk As Boolean = engine2.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "phase-3 stop returned False")

                TestRunner.Assert(rec2.StoppedCount() = 0,
                                  $"FALSE SAVED: RecordingStopped fired for truncated REAL NVENC media ({rec2.StoppedCount()}×)")
                TestRunner.Assert(rec2.ErrorCount() = 1,
                                  $"expected exactly one honesty error, got {rec2.ErrorCount()}: [{String.Join(" | ", rec2.Errors)}]")
                Dim msg As String = ""
                SyncLock rec2.Errors : msg = rec2.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("not saved"), $"expected the honesty error, got: {msg}")
                TestRunner.Assert(engine2.State = EngineCapture.CaptureState.Idle,
                                  $"phase-3 terminal state = {engine2.State} (Step-4 honesty contract → Idle)")
                TestRunner.Assert(Not IO.File.Exists(out2),
                                  "unplayable fallback output must have been removed by ValidatePlayback")
                TestRunner.Assert(Not IO.File.Exists(tempVideo),
                                  "temp video must have been consumed by the (deleted) fallback rename")
            Finally
                ClearHelperEnv()
                engine2.Dispose()
            End Try
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' NV-STRESS
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>Ten REAL NVENC cycles: normal start/stop (saved once,
        ''' temps cleaned), controlled mux failure (honesty error once),
        ''' retry after failure, stop-after-failure, and dispose — with
        ''' orphan checks between phases.</summary>
        Private Sub Test_NvencStress10()
            Dim failures As New List(Of String)()
            Dim rnd As New Random(20260906)

            For i As Integer = 1 To 10
                Dim outPath As String = IO.Path.Combine(_sandbox, $"nv_stress_{i}.mp4")
                Dim tempVideo As String = IO.Path.Combine(_sandbox, $"nv_stress_{i}.video.tmp.mp4")
                Dim failureMode As Boolean = (i Mod 2 = 0)
                ClearHelperEnv()

                Dim rec As New NvRec()
                Dim engine As EngineCapture = StartRealRecording(_sandbox, outPath, rec)
                If engine Is Nothing Then
                    failures.Add($"cycle {i}: start failed (3 attempts)")
                    Continue For
                End If
                Try
                    Thread.Sleep(Math.Max(800, rnd.Next(900, 1600)))

                    If failureMode Then
                        IO.Directory.CreateDirectory(outPath)   ' controlled mux failure
                    End If

                    Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                    TestRunner.Assert(stopped, $"cycle {i}: stop returned False")

                    If failureMode Then
                        TestRunner.Assert(rec.StoppedCount() = 0,
                                          $"cycle {i}: FALSE SAVED on forced mux failure")
                        TestRunner.Assert(rec.ErrorCount() = 1,
                                          $"cycle {i}: expected one honesty error, got {rec.ErrorCount()}")
                        TestRunner.Assert(IO.File.Exists(tempVideo),
                                          $"cycle {i}: real NVENC temp media missing after mux failure")
                        TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                          $"cycle {i}: state {engine.State} after failure recovery")
                        ' Real media evidence per failure cycle — probe once.
                        Dim probe As String = ProbeMedia(tempVideo)
                        TestRunner.Assert(Regex.IsMatch(probe, "codec_name=h264"),
                                          $"cycle {i}: temp media is not real H.264")
                        ' Retry after failure: a fresh engine records normally.
                        Dim retryOut As String = IO.Path.Combine(_sandbox, $"nv_stress_{i}_retry.mp4")
                        Dim recRetry As New NvRec()
                        Dim engineRetry As EngineCapture = StartRealRecording(_sandbox, retryOut, recRetry)
                        TestRunner.Assert(engineRetry IsNot Nothing, $"cycle {i}: retry start failed")
                        Try
                            Thread.Sleep(700)
                            TestRunner.Assert(engineRetry.StopRecordingAsync().GetAwaiter().GetResult(),
                                              $"cycle {i}: retry stop returned False")
                            TestRunner.Assert(recRetry.StoppedCount() = 1 AndAlso recRetry.ErrorCount() = 0,
                                              $"cycle {i}: retry after failure did not save cleanly")
                            TestRunner.Assert(New IO.FileInfo(retryOut).Length > 0,
                                              $"cycle {i}: retry output empty")
                        Finally
                            engineRetry.Dispose()
                        End Try
                        Try : IO.File.Delete(tempVideo) : Catch : End Try
                    Else
                        TestRunner.Assert(rec.StoppedCount() = 1,
                                          $"cycle {i}: expected RecordingStopped exactly once, got {rec.StoppedCount()}")
                        TestRunner.Assert(rec.ErrorCount() = 0,
                                          $"cycle {i}: unexpected errors: [{String.Join(" | ", rec.Errors)}]")
                        TestRunner.Assert(New IO.FileInfo(outPath).Length > 0, $"cycle {i}: output empty")
                        TestRunner.Assert(Not IO.File.Exists(tempVideo),
                                          $"cycle {i}: temp video survived a successful mux")
                        TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                          $"cycle {i}: state {engine.State} after normal stop")
                    End If

                    TestRunner.Assert(Not engine.IsRecordingLifecycleActive,
                                      $"cycle {i}: lifecycle active at terminal return")
                Finally
                    engine.Dispose()
                End Try

                TestRunner.Assert(WaitFor(Function() FfmpegCount() = 0, 8000),
                                  $"cycle {i}: orphan ffmpeg after the cycle")
            Next

            ' ── Stop-after-failure + dispose-hygiene on a real session.
            Dim sOut As String = IO.Path.Combine(_sandbox, "nv_stress_after.mp4")
            ClearHelperEnv()
            Dim recF As New NvRec()
            Dim engineF As EngineCapture = StartRealRecording(_sandbox, sOut, recF)
            TestRunner.Assert(engineF IsNot Nothing, "post-stress start failed")
            Try
                Thread.Sleep(900)
                IO.Directory.CreateDirectory(sOut)   ' force mux failure
                engineF.StopRecordingAsync().GetAwaiter().GetResult()
                Dim errorsBefore As Integer = recF.ErrorCount()
                engineF.StopRecordingAsync().GetAwaiter().GetResult()   ' stop-after-failure
                engineF.Dispose()
                engineF.Dispose()                        ' double dispose
                Thread.Sleep(300)
                TestRunner.Assert(recF.ErrorCount() = errorsBefore AndAlso recF.StoppedCount() = 0,
                                  "duplicate terminal events after stop-after-failure/dispose")
            Finally
                ClearHelperEnv()
                engineF.Dispose()
            End Try
            Try
                If IO.Directory.Exists(sOut) Then IO.Directory.Delete(sOut, True)
            Catch
            End Try

            TestRunner.Assert(WaitFor(Function() FfmpegCount() = 0 AndAlso HelperProcesses() = 0, 20000),
                              "orphan processes at stress end")

            If failures.Count > 0 Then
                TestRunner.Assert(False, "stress failures: " & String.Join(" ; ", failures))
            End If
        End Sub

    End Module

End Namespace
