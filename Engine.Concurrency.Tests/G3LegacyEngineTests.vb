Option Strict On
Option Explicit On
Option Infer On

' G3LegacyEngineTests.vb — G3: legacy CaptureEngine lifecycle boundaries and
' finalization edges that G1 (LiveMuxSession) and G2 (unexpected-exit
' recovery) do not cover. Same REAL engine + helper-exe seam as G2 — no new
' fake abstraction; the helper gained stdin-'q' watching (graceful stop) and
' mux-success output production so the FULL user-stop path can be exercised.
'
'   G3-A  Dispose while recording — form-close path: ForceStop must reap the
'         process, normalize state to Idle, stay idempotent, and stay
'         terminal-quiet (no recovery events racing the teardown).
'   G3-B  Double Start — the engine-level state guard: second start rejected
'         with exactly one error, first recording unaffected, graceful stop
'         completes exactly once with a valid output.
'   G3-C  Start validation failures — invalid FPS / unsupported extension:
'         refused before any process spawn, error surfaced once, state Idle.
'   G3-D  Stop before Start — refused, no events, state untouched.
'   G3-E  Graceful 'q' stop — the full user-stop contract: 'q' → helper exits
'         0 → audio finalized → mux SUCCEEDS → RecordingStopped exactly once
'         → output probe-able (Video+Audio) → ALL temp files cleaned.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NVIDIA_Capture
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace Engine.Concurrency.Tests

    Friend Module G3LegacyEngineTests

        Private _ffmpeg As String = ""
        Private _sandbox As String = ""
        Private _sourceMp4 As String = ""        ' A/V — mux-SUCCESS output source
        Private _sourceVideoOnly As String = ""  ' video-only — record-copy (temp video)

        Friend Sub RunAll(ffmpegPath As String, sandboxDir As String)
            _ffmpeg = ffmpegPath
            _sandbox = sandboxDir
            _sourceMp4 = IO.Path.Combine(_sandbox, "g3_source.mp4")

            ' Source MP4 WITH audio — mux-success outputs must probe as A/V.
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg,
                .Arguments = "-y -f lavfi -i testsrc=duration=1:size=320x240:rate=15 " &
                             "-f lavfi -i sine=frequency=440:duration=1 " &
                             "-c:v libx264 -preset ultrafast -pix_fmt yuv420p -c:a aac -shortest """ & _sourceMp4 & """",
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
                Console.WriteLine("(G3 tests skipped — source mp4 generation failed)")
                Return
            End If

            ' Video-only source for the record-copy seam: with THIS as the temp
            ' video, the no-audio RENAME path would leave a video-only output —
            ' so any test asserting an Audio stream truly went through the MUX.
            _sourceVideoOnly = IO.Path.Combine(_sandbox, "g3_srcvideo.mp4")
            Dim psi2 As New ProcessStartInfo With {
                .FileName = _ffmpeg,
                .Arguments = "-y -f lavfi -i testsrc=duration=1:size=320x240:rate=15 " &
                             "-c:v libx264 -preset ultrafast -pix_fmt yuv420p -an """ & _sourceVideoOnly & """",
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True
            }
            Using p2 As Process = Process.Start(psi2)
                p2.StandardError.ReadToEnd()
                If Not p2.WaitForExit(30000) Then
                    Try : p2.Kill() : Catch : End Try
                End If
            End Using
            If Not IO.File.Exists(_sourceVideoOnly) Then
                Console.WriteLine("(G3 tests skipped — video-only source generation failed)")
                Return
            End If

            Console.WriteLine("── G3: legacy CaptureEngine lifecycle boundaries & finalization edges ──")
            TestRunner.RunTest("G3-A: Dispose while recording — reaped, idempotent, terminal-quiet, Idle",
                               AddressOf Test_DisposeWhileRecording)
            TestRunner.RunTest("G3-B: double Start — second rejected once, first unaffected, saved once",
                               AddressOf Test_DoubleStart)
            TestRunner.RunTest("G3-C: Start validation failures — refused pre-spawn, error once, Idle",
                               AddressOf Test_StartValidationFailures)
            TestRunner.RunTest("G3-D: Stop before Start — refused, no events", AddressOf Test_StopBeforeStart)
            TestRunner.RunTest("G3-E: graceful 'q' stop — mux success, saved once, probe A/V, temp hygiene",
                               AddressOf Test_GracefulStopMuxSuccess)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Plumbing (local copies — modules stay independent)
        ' ───────────────────────────────────────────────────────────────

        Private NotInheritable Class EventRecorder
            Public ReadOnly Started As New List(Of String)()
            Public ReadOnly Stopped As New List(Of String)()
            Public ReadOnly Errors As New List(Of String)()

            Public Sub Wire(engine As EngineCapture)
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

        Private Function MakeSettings(sandbox As String) As CaptureSettings
            Dim s As New CaptureSettings()
            s.FFmpegPath = Environment.ProcessPath   ' helper-exe seam
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
        End Sub

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

        ' ───────────────────────────────────────────────────────────────
        ' Scenarios
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>G3-A: form-close mid-recording. Dispose must reap the
        ''' running helper (ForceStop kill + job guard), be idempotent, land
        ''' Idle, and stay terminal-quiet — the teardown owns the exit; the
        ''' OnExited recovery path must not fire over it.</summary>
        Private Sub Test_DisposeWhileRecording()
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "60")
            SetEnv("LMHLP_EXIT", "0")

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Dim outPath As String = IO.Path.Combine(_sandbox, "g3_a.mp4")

            Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
            TestRunner.Assert(started, "start returned False")
            TestRunner.Assert(engine.State = EngineCapture.CaptureState.Recording,
                              $"state after start = {engine.State}")
            TestRunner.Assert(HelperProcesses() = 2, "helper process not running after start")

            engine.Dispose()

            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1, 8000),
                              "helper process survived Dispose while recording")
            TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                              $"state after Dispose = {engine.State} (ForceStop normalizes to Idle)")
            TestRunner.Assert(rec.StoppedCount() = 0,
                              "Dispose must not emit RecordingStopped (teardown, not a save)")

            ' Idempotent dispose + dead-engine start.
            engine.Dispose()
            Dim restarted As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
            TestRunner.Assert(Not restarted, "StartRecordingAsync on a disposed engine must return False")

            ' ★ Terminal-quiet: no recovery error racing the ForceStop teardown.
            ' (OnExited may observe state=Recording while ForceStop is still
            ' between Kill and its finally — if a spurious
            ' "exited unexpectedly" error EVER appears here, that is the
            ' G3 dispose-race bug. This assertion pins the quiet contract.)
            Thread.Sleep(500)
            TestRunner.Assert(rec.ErrorCount() = 0,
                              $"SPURIOUS terminal error during Dispose teardown: [{String.Join(" | ", rec.Errors)}]")
            ClearHelperEnv()
        End Sub

        ''' <summary>G3-B: engine-level double-start guard. The second start
        ''' must be refused with exactly one error and must not disturb the
        ''' first recording; the first then stops gracefully and saves once.</summary>
        Private Sub Test_DoubleStart()
            Dim outPath As String = IO.Path.Combine(_sandbox, "g3_b.mp4")
            Dim tempVideoB As String = IO.Path.Combine(_sandbox, "g3_b.video.tmp.mp4")
            Dim tempSysB As String = IO.Path.Combine(_sandbox, "g3_b.system.tmp.wav")
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "30")
            SetEnv("LMHLP_EXIT", "0")
            SetEnv("LMHLP_SRC", _sourceVideoOnly)
            SetEnv("LMHLP_COPYTO", tempVideoB)
            SetEnv("LMHLP_MUX_SRC", _sourceMp4)

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "first start returned False")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Recording,
                                  $"state after first start = {engine.State}")

                Dim secondPath As String = IO.Path.Combine(_sandbox, "g3_b_second.mp4")
                Dim second As Boolean = engine.StartRecordingAsync(secondPath).GetAwaiter().GetResult()
                TestRunner.Assert(Not second, "second start must be refused")
                TestRunner.Assert(rec.ErrorCount() = 1,
                                  $"expected exactly one refusal error, got {rec.ErrorCount()}: [{String.Join(" | ", rec.Errors)}]")
                Dim msg As String = ""
                SyncLock rec.Errors : msg = rec.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("not idle"), $"refusal should name the state guard, got: {msg}")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Recording,
                                  $"first recording disturbed by second start: {engine.State}")
                TestRunner.Assert(Not IO.File.Exists(secondPath),
                                  "refused start must not create a second output")

                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "graceful stop returned False")
                TestRunner.Assert(rec.StoppedCount() = 1,
                                  $"expected RecordingStopped exactly once, got {rec.StoppedCount()}")
                Dim saved As String = ""
                SyncLock rec.Stopped : saved = rec.Stopped(0) : End SyncLock
                TestRunner.Assert(saved = outPath, $"saved path {saved} ≠ {outPath}")
                TestRunner.Assert(rec.ErrorCount() = 1, "no new errors may appear during the stop")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"post-stop state = {engine.State}")

                Dim probe As String = ProbeStreams(outPath)
                TestRunner.Assert(probe.Contains("Video:"), "output has no video stream")
                TestRunner.Assert(probe.Contains("Audio:"), "output has no audio stream")

                ' Mux-success discriminator: the record-copy source is VIDEO-ONLY,
                ' so an Audio stream in the output proves the MUX ran (the
                ' no-audio RENAME path would leave the video-only temp as output
                ' AND keep the temp wav file alive).
                TestRunner.Assert(Not IO.File.Exists(tempVideoB),
                                  "temp video survived — mux success should have cleaned it")
                TestRunner.Assert(Not IO.File.Exists(tempSysB),
                                  "temp wav survived — the no-audio RENAME path ran instead of the MUX")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1, 8000),
                              "helper process survived the double-start scenario")
        End Sub

        ''' <summary>G3-C: Start validation failures must be refused BEFORE any
        ''' process spawn — error surfaced once, state stays Idle, nothing runs.</summary>
        Private Sub Test_StartValidationFailures()
            ClearHelperEnv()

            ' 1) invalid FPS — fails Validate() pre-spawn
            Dim badFps As CaptureSettings = MakeSettings(_sandbox)
            badFps.FPS = 0
            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(badFps)
            rec.Wire(engine)
            Try
                Dim ok As Boolean = engine.StartRecordingAsync(
                    IO.Path.Combine(_sandbox, "g3_c_fps.mp4")).GetAwaiter().GetResult()
                TestRunner.Assert(Not ok, "invalid FPS start must be refused")
                TestRunner.Assert(rec.ErrorCount() = 1,
                                  $"expected one validation error, got {rec.ErrorCount()}")
                Dim msg As String = ""
                SyncLock rec.Errors : msg = rec.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("FPS"), $"error should name FPS validation, got: {msg}")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"state after validation failure = {engine.State}")
                TestRunner.Assert(HelperProcesses() = 1, "validation failure must not spawn a helper process")
            Finally
                engine.Dispose()
            End Try

            ' 2) unsupported output extension — fails the override guard
            Dim rec2 As New EventRecorder()
            Dim engine2 As New EngineCapture(MakeSettings(_sandbox))
            rec2.Wire(engine2)
            Try
                Dim ok2 As Boolean = engine2.StartRecordingAsync(
                    IO.Path.Combine(_sandbox, "g3_c_bad.xyz")).GetAwaiter().GetResult()
                TestRunner.Assert(Not ok2, "unsupported extension must be refused")
                TestRunner.Assert(rec2.ErrorCount() = 1,
                                  $"expected one extension error, got {rec2.ErrorCount()}")
                Dim msg2 As String = ""
                SyncLock rec2.Errors : msg2 = rec2.Errors(0) : End SyncLock
                TestRunner.Assert(msg2.Contains("Unsupported output extension"),
                                  $"error should name the extension guard, got: {msg2}")
                TestRunner.Assert(engine2.State = EngineCapture.CaptureState.Idle,
                                  $"state after extension failure = {engine2.State}")
                TestRunner.Assert(Not IO.File.Exists(IO.Path.Combine(_sandbox, "g3_c_bad.xyz")),
                                  "refused start must not create the output file")
            Finally
                engine2.Dispose()
            End Try
            ClearHelperEnv()
        End Sub

        ''' <summary>G3-D: StopRecordingAsync before any start — refused, no
        ''' events, state untouched.</summary>
        Private Sub Test_StopBeforeStart()
            ClearHelperEnv()
            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(Not stopped, "Stop before Start must be refused")
                TestRunner.Assert(rec.StoppedCount() = 0 AndAlso rec.ErrorCount() = 0,
                                  $"stop-before-start must be event-silent, got stopped={rec.StoppedCount()} errors={rec.ErrorCount()}")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"state after refused stop = {engine.State}")
            Finally
                engine.Dispose()
            End Try
        End Sub

        ''' <summary>G3-E: the FULL user-stop contract with a mux SUCCESS —
        ''' 'q' gracefully ends the helper, audio is finalized, the mux runs
        ''' and produces output, RecordingStopped fires exactly once, the file
        ''' probes as Video+Audio, and every temp file is cleaned up.</summary>
        Private Sub Test_GracefulStopMuxSuccess()
            Dim outPath As String = IO.Path.Combine(_sandbox, "g3_e.mp4")
            Dim tempVideo As String = IO.Path.Combine(_sandbox, "g3_e.video.tmp.mp4")
            Dim tempSys As String = IO.Path.Combine(_sandbox, "g3_e.system.tmp.wav")
            Dim tempMic As String = IO.Path.Combine(_sandbox, "g3_e.mic.tmp.wav")
            ClearHelperEnv()
            SetEnv("LMHLP_SLEEP", "30")
            SetEnv("LMHLP_EXIT", "0")
            SetEnv("LMHLP_SRC", _sourceVideoOnly)
            SetEnv("LMHLP_COPYTO", tempVideo)
            SetEnv("LMHLP_MUX_SRC", _sourceMp4)

            Dim rec As New EventRecorder()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(WaitFor(Function() IO.File.Exists(tempVideo), 5000),
                                  "helper never produced the temp video (seam broken)")

                ' Wait until the audio sink is ALIGNED (anchor = the helper's
                ' "Output #0" stderr line) and has written real data — a bare
                ' 44-byte header wav would take the no-audio RENAME path.
                TestRunner.Assert(WaitFor(Function()
                                              Dim fi As New IO.FileInfo(tempSys)
                                              Return fi.Exists AndAlso fi.Length > 44
                                          End Function, 10000),
                                  "system wav never received aligned audio data (anchor/seam broken)")

                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "graceful stop returned False")

                TestRunner.Assert(rec.StoppedCount() = 1,
                                  $"expected RecordingStopped exactly once, got {rec.StoppedCount()}")
                Dim saved As String = ""
                SyncLock rec.Stopped : saved = rec.Stopped(0) : End SyncLock
                TestRunner.Assert(saved = outPath, $"saved path {saved} ≠ {outPath}")
                TestRunner.Assert(rec.ErrorCount() = 0,
                                  $"success path must be error-free, got: [{String.Join(" | ", rec.Errors)}]")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"post-stop state = {engine.State}")
                TestRunner.Assert(engine.LastAudioDiagnostics.Length > 0,
                                  "audio was never finalized during the stop")

                Dim probe As String = ProbeStreams(outPath)
                TestRunner.Assert(probe.Contains("Video:"), "final output has no video stream")
                TestRunner.Assert(probe.Contains("Audio:"), "final output has no audio stream")

                ' Temp hygiene after a successful mux — all three temps gone.
                TestRunner.Assert(Not IO.File.Exists(tempVideo), "temp video survived a successful mux")
                TestRunner.Assert(Not IO.File.Exists(tempSys), "temp system wav survived a successful mux")
                TestRunner.Assert(Not IO.File.Exists(tempMic), "temp mic wav survived a successful mux")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1, 8000),
                              "helper process survived the graceful-stop scenario")
        End Sub

    End Module

End Namespace
