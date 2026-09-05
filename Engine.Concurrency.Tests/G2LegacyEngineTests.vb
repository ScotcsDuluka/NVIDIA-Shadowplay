Option Strict On
Option Explicit On
Option Infer On

' G2LegacyEngineTests.vb — G2: dedicated coverage for the LEGACY
' CaptureEngine's unexpected-FFmpeg-exit recovery (the two-process path).
'
' Production flow pinned here (Engine\Engine\[Capture]\CaptureEngine.vb):
'
'   StartRecordingAsync → ffmpeg process (state=Recording)
'     → ffmpeg exits BY ITSELF (unexpected — user never pressed stop)
'     → OnExited (Exited event, threadpool):
'         exit=0  → stop/finalize audio → AwaitOrRunMux (guarded once)
'                   → mux skip/fallback/rename → terminal event exactly once
'         exit≠0  → stop/finalize audio → HasError + ErrorOccurred exactly once
'
' Deterministic process failure WITHOUT touching production code: the test
' executable ITSELF acts as the controllable helper. The engine launches
' whatever CaptureSettings.FFmpegPath points to — here Environment.ProcessPath
' (this test exe). Main() recognizes the engine's "-hide_banner" argument
' shape and dispatches to HelperMode (see Program.Main):
'
'   record mode (args contain "ddagrab")
'       optionally copy LMHLP_SRC → LMHLP_COPYTO (a real MP4 standing in
'       for the encoded temp video), emit a stderr line, sleep
'       LMHLP_SLEEP seconds, exit with LMHLP_EXIT
'   mux mode (args contain "-map")
'       exit with LMHLP_MUX_EXIT (default 0)
'
' Everything lives inside the test lifecycle — no persistence, no machine
' impact, no fake abstraction replacing production behavior: the REAL
' CaptureEngine, REAL WASAPI audio engine, REAL event/state machine and
' REAL file verification (ffprobe-equivalent via bundled ffmpeg) run.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NVIDIA_Capture
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace Engine.Concurrency.Tests

    Friend Module G2LegacyEngineTests

        Private _ffmpeg As String = ""
        Private _sandbox As String = ""
        Private _sourceMp4 As String = ""

        ' ───────────────────────────────────────────────────────────────
        ' Helper-mode entry — called from Program.Main when the ENGINE
        ' launches this executable as its ffmpeg. MUST stay side-effect
        ' free beyond the engine's own temp files.
        ' ───────────────────────────────────────────────────────────────
        Friend Function RunHelperMode(args As String()) As Integer
            Dim joined As String = String.Join(" ", args)

            ' F-03 PROBE mode — the engine's playback validation spawns
            ' ffmpeg with "-v error -i <file> -t 1 -f null -". Delegate to the
            ' REAL ffmpeg (LMHLP_PROBE_REAL) so the verdict is genuine, or
            ' return LMHLP_PROBE_EXIT for synthetic cases.
            If args.Length > 0 AndAlso args(0) = "-v" Then
                Dim realProbe As String = Environment.GetEnvironmentVariable("LMHLP_PROBE_REAL")
                If Not String.IsNullOrEmpty(realProbe) Then
                    Return RunRealFfmpeg(realProbe, joined, 30000)
                End If
                Dim probeExit As Integer = 0
                Integer.TryParse(Environment.GetEnvironmentVariable("LMHLP_PROBE_EXIT"), probeExit)
                Console.Error.WriteLine("[LMHLP] probe mode → exit " & probeExit)
                Return probeExit
            End If

            ' MUX mode — AwaitOrRunMux builds args containing "-map".
            If joined.Contains("-map ") Then
                ' F-03: LMHLP_MUX_REAL delegates to the REAL ffmpeg with the
                ' engine's exact mux arguments — the mux verdict is then the
                ' real decoder's verdict (used for the fallback-honesty test).
                Dim realMux As String = Environment.GetEnvironmentVariable("LMHLP_MUX_REAL")
                If Not String.IsNullOrEmpty(realMux) Then
                    Dim realExit As Integer = RunRealFfmpeg(realMux, joined, 60000)
                    Console.Error.WriteLine("[LMHLP] mux delegated → exit " & realExit)
                    Return realExit
                End If
                Dim muxExit As Integer = 0
                Integer.TryParse(Environment.GetEnvironmentVariable("LMHLP_MUX_EXIT"), muxExit)
                Console.Error.WriteLine("[LMHLP] mux mode → exit " & muxExit)
                If muxExit = 0 Then
                    ' G3: a mux SUCCESS must leave a real output file, or the
                    ' engine's honesty check reports "not saved". Copy the
                    ' source MP4 to the output path embedded in the mux args.
                    Dim muxSrc As String = Environment.GetEnvironmentVariable("LMHLP_MUX_SRC")
                    Dim outPath As String = ExtractMuxOutputPath(args)
                    Console.Error.WriteLine($"[LMHLP] mux src={If(muxSrc, "(null)")} out={If(outPath, "(null)")}")
                    If muxSrc IsNot Nothing AndAlso outPath IsNot Nothing Then
                        Try
                            IO.File.Copy(muxSrc, outPath, True)
                            Console.Error.WriteLine("[LMHLP] mux produced " & outPath)
                        Catch ex As Exception
                            Console.Error.WriteLine("[LMHLP] mux copy failed: " & ex.Message)
                        End Try
                    End If
                End If
                Return muxExit
            End If

            ' RECORD mode — BuildFFmpegArguments always requests ddagrab.
            Dim src As String = Environment.GetEnvironmentVariable("LMHLP_SRC")
            Dim dst As String = Environment.GetEnvironmentVariable("LMHLP_COPYTO")
            If Not String.IsNullOrEmpty(src) AndAlso Not String.IsNullOrEmpty(dst) Then
                Try
                    IO.File.Copy(src, dst, True)
                Catch
                End Try
            End If
            Console.Error.WriteLine("[LMHLP] record start")
            ' Real ffmpeg prints "Output #0" before the transcode loop; the
            ' engine uses it as the video-start anchor — without it the audio
            ' sinks never align and the wav stays a bare 44-byte header.
            Console.Error.WriteLine("[LMHLP] Output #0, mp4, to 'helper'")

            Dim sleepS As Integer = 0
            Integer.TryParse(Environment.GetEnvironmentVariable("LMHLP_SLEEP"), sleepS)

            ' G3: watch stdin for the engine's graceful quit ("q") — exercises
            ' the REAL StopRecordingAsync 'q' → WaitForExit contract. Background
            ' thread: a kill (dispose paths) tears it down with the process.
            Dim quitReceived As New ManualResetEvent(False)
            Dim stdinReader As New Thread(
                Sub()
                    Try
                        While True
                            Dim line As String = Console.ReadLine()
                            If line Is Nothing Then Exit While
                            If line.Trim().StartsWith("q", StringComparison.Ordinal) Then
                                quitReceived.Set()
                                Exit While
                            End If
                        End While
                    Catch
                    End Try
                End Sub) With {.IsBackground = True}
            stdinReader.Start()

            If quitReceived.WaitOne(Math.Max(0, sleepS) * 1000) Then
                Console.Error.WriteLine("[LMHLP] quit received — graceful exit")
            Else
                Console.Error.WriteLine("[LMHLP] sleep elapsed")
            End If

            Dim exitCode As Integer = 0
            Integer.TryParse(Environment.GetEnvironmentVariable("LMHLP_EXIT"), exitCode)
            Console.Error.WriteLine("[LMHLP] record exit " & exitCode)
            Return exitCode
        End Function

        ''' <summary>★ G3: .NET command-line parsing STRIPS the quotes before
        ''' args reach us, so the paths arrive as bare tokens. The mux OUTPUT is
        ''' the last .mp4 argument that is not one of the engine's .tmp inputs
        ''' (inputs: video.tmp.mp4 / system.tmp.wav; output: final .mp4).</summary>
        ''' <summary>F-03: run the REAL ffmpeg with the given argument string
        ''' and return its exit code (bounded). Test-only delegation so mux and
        ''' playback-validation verdicts come from the real decoder.</summary>
        Private Function RunRealFfmpeg(realExe As String, argumentLine As String, timeoutMs As Integer) As Integer
            Dim psi As New ProcessStartInfo With {
                .FileName = realExe,
                .Arguments = argumentLine,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                Dim errTask = p.StandardError.ReadToEndAsync()
                Dim outTask = p.StandardOutput.ReadToEndAsync()
                If Not p.WaitForExit(timeoutMs) Then
                    Try : p.Kill() : p.WaitForExit(2000) : Catch : End Try
                    Return -999
                End If
                Try : errTask.Wait(1000) : Catch : End Try
                Try : outTask.Wait(500) : Catch : End Try
                Return p.ExitCode
            End Using
        End Function

        Private Function ExtractMuxOutputPath(args As String()) As String
            Dim result As String = Nothing
            For Each a As String In args
                Dim t As String = If(a, "").Trim()
                If t.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) AndAlso
                   Not t.Contains(".tmp") Then
                    result = t
                End If
            Next
            Return result
        End Function

        ' ───────────────────────────────────────────────────────────────
        ' Suite
        ' ───────────────────────────────────────────────────────────────

        Friend Sub RunAll(ffmpegPath As String, sandboxDir As String)
            _ffmpeg = ffmpegPath
            _sandbox = sandboxDir
            _sourceMp4 = IO.Path.Combine(_sandbox, "g2_source.mp4")

            ' Real MP4 standing in for the "encoded" temp video.
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg,
                .Arguments = "-y -f lavfi -i testsrc=duration=1:size=320x240:rate=15 " &
                             "-c:v libx264 -preset ultrafast -pix_fmt yuv420p """ & _sourceMp4 & """",
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True
            }
            Using p As Process = Process.Start(psi)
                p.StandardError.ReadToEnd()
                If Not p.WaitForExit(30000) Then
                    Try : p.Kill() : Catch : End Try
                End If
            End Using
            If Not IO.File.Exists(_sourceMp4) Then
                Console.WriteLine("(G2 tests skipped — source mp4 generation failed)")
                Return
            End If

            Console.WriteLine("── G2: legacy CaptureEngine unexpected-exit recovery (real engine + helper exe) ──")
            RunTest("G2-A: unexpected exit 0 → audio finalized → mux skips (no temp video) → 'not saved' exactly once",
                    AddressOf Test_UnexpectedExit0_NoTempVideo)
            RunTest("G2-B: unexpected exit 3 → HasError + error exactly once + audio finalized + no saved",
                    AddressOf Test_UnexpectedExitNonZero)
            RunTest("G2-C: unexpected exit 0 + real temp video → mux fails → video-only fallback → saved exactly once",
                    AddressOf Test_MuxFailureFallback)
            RunTest("G2-F: post-failure Stop/Dispose idempotency — no double terminal event, no orphan",
                    AddressOf Test_PostFailureIdempotency)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Plumbing
        ' ───────────────────────────────────────────────────────────────

        Private Function HelperExePath() As String
            Return Environment.ProcessPath
        End Function

        Private Function MakeSettings(sandbox As String) As CaptureSettings
            Dim s As New CaptureSettings()
            ' The engine launches THIS test executable as its "ffmpeg" —
            ' Main() dispatches helper mode by the "-hide_banner" arg shape.
            s.FFmpegPath = HelperExePath()
            s.Encoder = "libx264"
            s.CaptureMethod = "ddagrab"
            s.FPS = 15
            s.Bitrate = 2000000L
            s.UseNativeResolution = True
            s.OutputDirectory = sandbox
            s.SystemAudioCapture = True    ' two-process path + real WASAPI audio
            s.MicCapture = False
            Return s
        End Function

        Private NotInheritable class EventRecorder
            Public ReadOnly States As New List(Of String)()
            Public ReadOnly Started As New List(Of String)()
            Public ReadOnly Stopped As New List(Of String)()
            Public ReadOnly Errors As New List(Of String)()

            Public Sub Wire(engine As EngineCapture)
                AddHandler engine.StateChanged, Sub(st) LockAdd(States, st.ToString())
                AddHandler engine.RecordingStarted, Sub(f) LockAdd(Started, f)
                AddHandler engine.RecordingStopped, Sub(f) LockAdd(Stopped, f)
                AddHandler engine.ErrorOccurred, Sub(m) LockAdd(Errors, m)
            End Sub

            Private Shared Sub LockAdd(list As List(Of String), item As String)
                SyncLock list
                    list.Add(item)
                End SyncLock
            End Sub

            Public Function StoppedCount() As Integer
                SyncLock Stopped : Return Stopped.Count : End SyncLock
            End Function

            Public Function ErrorCount() As Integer
                SyncLock Errors : Return Errors.Count : End SyncLock
            End Function
        End Class

        Private Function WaitFor(condition As Func(Of Boolean), timeoutMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < timeoutMs
                If condition() Then Return True
                Thread.Sleep(50)
            End While
            Return condition()
        End Function

        Private Function HelperProcesses() As Integer
            Dim name As String = IO.Path.GetFileNameWithoutExtension(Environment.ProcessPath)
            Dim procs As Process() = Process.GetProcessesByName(name)
            Dim n As Integer = 0
            For Each p As Process In procs
                Try
                    If Not p.HasExited Then n += 1   ' ignore dying processes still in the table
                Catch
                End Try
                Try : p.Dispose() : Catch : End Try
            Next
            Return n
        End Function

        Private Function FfmpegCount() As Integer
            Dim procs As Process() = Process.GetProcessesByName("ffmpeg")
            Dim n As Integer = 0
            For Each p As Process In procs
                Try
                    If Not p.HasExited Then n += 1   ' ignore dying processes still in the table
                Catch
                End Try
                Try : p.Dispose() : Catch : End Try
            Next
            Return n
        End Function

        Private Function ProbeStreams(path As String) As String
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg,
                .Arguments = "-hide_banner -i """ & path & """",
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True
            }
            Using p As Process = Process.Start(psi)
                Dim errTask = p.StandardError.ReadToEndAsync()
                p.WaitForExit(10000)
                errTask.Wait(2000)
                Return If(errTask.Status = Threading.Tasks.TaskStatus.RanToCompletion, errTask.Result, "")
            End Using
        End Function

        Private Sub SetEnv(name As String, value As String)
            If value Is Nothing Then
                Environment.SetEnvironmentVariable(name, Nothing)
            Else
                Environment.SetEnvironmentVariable(name, value)
            End If
        End Sub

        Private Sub ClearHelperEnv()
            SetEnv("LMHLP_SRC", Nothing)
            SetEnv("LMHLP_COPYTO", Nothing)
            SetEnv("LMHLP_SLEEP", Nothing)
            SetEnv("LMHLP_EXIT", Nothing)
            SetEnv("LMHLP_MUX_EXIT", Nothing)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Scenarios
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>G2-A: ffmpeg exits BY ITSELF with code 0 before producing
        ''' any temp video → audio must still be finalized, the mux must skip
        ''' honestly, and the terminal outcome must be a SINGLE "not saved"
        ''' error — never a false RecordingStopped.</summary>
        Private Sub Test_UnexpectedExit0_NoTempVideo()
            Dim outPath As String = IO.Path.Combine(_sandbox, "g2_a.mp4")
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "3")
            SetEnv("LMHLP_EXIT", "0")

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Recording,
                                  $"state after start = {engine.State}")

                ' Terminal = first RecordingStopped OR first ErrorOccurred.
                TestRunner.Assert(
                    WaitFor(Function() rec.StoppedCount() + rec.ErrorCount() > 0, 30000),
                    "no terminal event within 30s of the unexpected exit")

                TestRunner.Assert(rec.StoppedCount() = 0,
                                  "FALSE SAVED: RecordingStopped fired without an output file")
                TestRunner.Assert(rec.ErrorCount() = 1,
                                  $"expected exactly ONE terminal error, got {rec.ErrorCount()}: [{String.Join(" | ", rec.Errors)}]")
                Dim msg As String = ""
                SyncLock rec.Errors : msg = rec.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("not saved"),
                                  $"terminal error should be the honesty error, got: {msg}")

                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"post-recovery state = {engine.State}")
                TestRunner.Assert(Not IO.File.Exists(outPath), "output file must not exist")
                TestRunner.Assert(engine.LastAudioDiagnostics.Length > 0,
                                  "audio engine was never finalized after the unexpected exit")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1 AndAlso FfmpegCount() = 0, 5000),
                              "leftover helper/ffmpeg process after scenario A")
        End Sub

        ''' <summary>G2-B: ffmpeg exits BY ITSELF with code 3 → failure surfaced
        ''' once (HasError + error event), audio finalized, NO saved event,
        ''' output state matches reality (no file).</summary>
        Private Sub Test_UnexpectedExitNonZero()
            Dim outPath As String = IO.Path.Combine(_sandbox, "g2_b.mp4")
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "3")
            SetEnv("LMHLP_EXIT", "3")

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")

                TestRunner.Assert(
                    WaitFor(Function() rec.ErrorCount() > 0, 30000),
                    "no error event within 30s of the unexpected exit")

                TestRunner.Assert(rec.ErrorCount() = 1,
                  $"expected exactly ONE error event, got {rec.ErrorCount()}: [{String.Join(" | ", rec.Errors)}]")
                Dim msg As String = ""
                SyncLock rec.Errors : msg = rec.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("exited unexpectedly with code 3"),
                                  $"error should name the unexpected exit code, got: {msg}")
                TestRunner.Assert(rec.StoppedCount() = 0, "exit≠0 must never fire RecordingStopped")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.HasError,
                                  $"state after failed exit = {engine.State} (expected HasError)")
                TestRunner.Assert(Not IO.File.Exists(outPath), "no output may exist after exit≠0")
                TestRunner.Assert(engine.LastAudioDiagnostics.Length > 0,
                                  "audio engine was never finalized after the unexpected exit")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
        End Sub

        ''' <summary>G2-C/D: unexpected exit 0 WITH a real temp video (helper
        ''' copies a real MP4) and a real flushed wav → mux runs and FAILS
        ''' (helper mux exit 7) → video-only fallback renames the temp video →
        ''' RecordingStopped exactly once with a probe-able file. The mux only
        ''' saw audio because the audio sink had already flushed (D ordering).</summary>
        Private Sub Test_MuxFailureFallback()
            Dim outPath As String = IO.Path.Combine(_sandbox, "g2_c.mp4")
            Dim tempVideo As String = IO.Path.Combine(_sandbox, "g2_c.video.tmp.mp4")
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "3")
            SetEnv("LMHLP_EXIT", "0")
            SetEnv("LMHLP_SRC", _sourceMp4)
            SetEnv("LMHLP_COPYTO", tempVideo)
            SetEnv("LMHLP_MUX_EXIT", "7")

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")

                TestRunner.Assert(
                    WaitFor(Function() rec.StoppedCount() + rec.ErrorCount() > 0, 30000),
                    "no terminal event within 30s of the unexpected exit")

                TestRunner.Assert(rec.ErrorCount() = 0,
                                  $"fallback recovery must not emit errors, got: [{String.Join(" | ", rec.Errors)}]")
                TestRunner.Assert(rec.StoppedCount() = 1,
                                  $"expected RecordingStopped EXACTLY ONCE, got {rec.StoppedCount()}")
                Dim savedPath As String = ""
                SyncLock rec.Stopped : savedPath = rec.Stopped(0) : End SyncLock
                TestRunner.Assert(savedPath = outPath, $"saved path {savedPath} ≠ {outPath}")

                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"post-fallback state = {engine.State}")
                TestRunner.Assert(IO.File.Exists(outPath), "fallback output missing")
                TestRunner.Assert(Not IO.File.Exists(tempVideo),
                                  "temp video must have been consumed by the fallback rename")
                TestRunner.Assert(engine.LastAudioDiagnostics.Length > 0,
                                  "audio must be finalized before the mux/fallback runs (G2-D)")

                ' ★ F-02: truthful fallback validation — video-only contract
                ' (the record-copy source carries no audio track).
                MediaAssert.AssertValidMp4(_ffmpeg, outPath, True, False, "G2-C")

                ' Double-mux guard: a Stop after the OnExited recovery must NOT
                ' run AwaitOrRunMux again (_muxCompleted already latched) and
                ' must not emit a second terminal event.
                ' State is already Idle → StopRecordingAsync correctly refuses
                ' (returns False) — the pinned property is that this NO-OP stop
                ' neither re-muxes nor re-emits any terminal event.
                engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(rec.StoppedCount() = 1, "duplicate RecordingStopped after post-recovery Stop")
                TestRunner.Assert(rec.ErrorCount() = 0, "unexpected error after post-recovery Stop")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1 AndAlso FfmpegCount() = 0, 5000),
                              "leftover helper process after scenario C")
        End Sub

        ''' <summary>G2-F: after an unexpected-exit failure (HasError), the
        ''' cleanup surface must be idempotent — Stop/Dispose/Dispose emit no
        ''' additional terminal events and leave no processes behind.</summary>
        Private Sub Test_PostFailureIdempotency()
            Dim outPath As String = IO.Path.Combine(_sandbox, "g2_f.mp4")
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "2")
            SetEnv("LMHLP_EXIT", "3")

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(
                    WaitFor(Function() rec.ErrorCount() > 0, 30000),
                    "no error event within 30s")
                Dim errorsAtRecovery As Integer = rec.ErrorCount()
                TestRunner.Assert(errorsAtRecovery = 1, "recovery should emit exactly one error")

                ' Idempotency surface: stop-after-failure, double dispose.
                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "Stop after unexpected exit returned False")
                engine.Dispose()
                engine.Dispose()

                Thread.Sleep(300)
                TestRunner.Assert(rec.ErrorCount() = errorsAtRecovery,
                                  $"duplicate terminal events after cleanup: [{String.Join(" | ", rec.Errors)}]")
                TestRunner.Assert(rec.StoppedCount() = 0, "no saved event may appear post-failure")
                ' Dispose → ForceStop normalizes the state to Idle by contract —
                ' the idempotency property under test is NO DUPLICATE EVENTS,
                ' not the state value after an explicit Dispose.
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"state after Dispose = {engine.State} (ForceStop normalizes to Idle)")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1 AndAlso FfmpegCount() = 0, 5000),
                              "leftover helper/ffmpeg process after idempotency scenario")
        End Sub

    End Module

End Namespace
