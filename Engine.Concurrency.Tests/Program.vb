Option Strict On
Option Explicit On
Option Infer On

' Engine.Concurrency.Tests — H1/H2 regression tests (forensic audit round).
'
' Proves the no-orphan / lifecycle-guard contracts introduced by the
' "fix: close verified recording concurrency races" change:
'
'   H2  CaptureEngine.StartRecordingAsync must re-check engine lifetime
'       AFTER Process.Start() and before/after JobObjectGuard.Assign —
'       a started ffmpeg must end the start operation either OWNED by the
'       job guard or TERMINATED. JobObjectGuard.Assign must report
'       ownership honestly (False after Dispose — never silent success).
'
'   H1  The recording lifecycle predicate (Recording/Stopping/Muxing)
'       must be True for the WHOLE stop flow including the MUX phase, so
'       the UI dispose-guards can never tear down an engine mid-mux.
'       Test B asserts the exact predicate the fixed UI guards now use,
'       plus the observable behavior: a start request arriving during
'       Muxing is rejected, the mux completes, and the final output exists.
'
'   C   Repeated Start → Stop → Dispose cycles with jittered timing must
'       not accumulate ffmpeg.exe processes.
'
' Requires: Windows + the bundled ffmpeg.exe (real recordings, real mux).

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports NVIDIA_Capture
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace Engine.Concurrency.Tests

    Friend Module TestRunner
        Friend _passed As Integer = 0
        Friend _failed As Integer = 0
        Friend ReadOnly _failures As New List(Of String)()

        Friend Sub RunTest(name As String, test As Action)
            Console.Write($"  {name} ... ")
            Try
                test()
                Console.WriteLine("PASS")
                _passed += 1
            Catch ex As Exception
                Console.WriteLine("FAIL")
                Console.WriteLine($"      → {ex.Message}")
                _failures.Add(name & ": " & ex.Message)
                _failed += 1
            End Try
        End Sub

        Friend Sub Assert(cond As Boolean, message As String)
            If Not cond Then Throw New Exception(message)
        End Sub
    End Module

    Friend Module Program

        Private _ffmpegPath As String = ""
        Private _sandbox As String = ""

        Function Main(args As String()) As Integer
            Console.WriteLine("==================================================")
            Console.WriteLine(" Engine.Concurrency.Tests — H1/H2 regression")
            Console.WriteLine(" (real ffmpeg, real job object, real lifecycle)")
            Console.WriteLine("==================================================")
            Console.WriteLine()

            If Not Setup() Then
                Console.WriteLine("SETUP FAILED — cannot run without bundled ffmpeg")
                Return 2
            End If

            Dim ffmpegBaseline As Integer = FfmpegCount()
            Console.WriteLine($" setup: ffmpeg = {_ffmpegPath}")
            Console.WriteLine($" setup: sandbox = {_sandbox}")
            Console.WriteLine($" setup: baseline ffmpeg.exe processes = {ffmpegBaseline}")
            Console.WriteLine()

            RunTest("GUARD: JobObjectGuard.Assign contract (live=True, disposed=False, null=False)",
                    AddressOf Test_JobGuardAssignContract)
            RunTest("H2-A: Dispose during StartRecordingAsync — no orphan ffmpeg",
                    AddressOf Test_DisposeDuringStart_NoOrphan)
            RunTest("H1-B: Start during Muxing rejected — old session completes with output",
                    AddressOf Test_StartDuringMuxing_ProtectsOldSession)
            RunTest("H2-C: repeated Start/Stop/Dispose with jitter — no orphan accumulation",
                    AddressOf Test_RepeatedCycles_NoOrphanAccumulation)

            Console.WriteLine()
            Console.WriteLine($" passed={TestRunner._passed} failed={TestRunner._failed}")
            For Each f In TestRunner._failures
                Console.WriteLine($"   FAILED: {f}")
            Next

            Dim ffmpegAfter As Integer = FfmpegCount()
            Console.WriteLine($" final ffmpeg.exe processes = {ffmpegAfter} (baseline {ffmpegBaseline})")
            If ffmpegAfter > ffmpegBaseline Then
                Console.WriteLine("   → ORPHAN ffmpeg processes detected by the suite itself")
                TestRunner._failed += 1
            End If

            Return If(TestRunner._failed = 0, 0, 1)
        End Function

        ' ───────────────────────────────────────────────────────────────

        Private Function Setup() As Boolean
            _ffmpegPath = ResolveFfmpeg()
            If String.IsNullOrEmpty(_ffmpegPath) OrElse Not File.Exists(_ffmpegPath) Then
                Console.WriteLine(" ffmpeg.exe not found under Overlay\ (API-Core or bin)")
                Return False
            End If

            _sandbox = Path.Combine(Path.GetTempPath(),
                                    "engine-concurrency-tests-" & DateTime.Now.ToString("yyyyMMdd_HHmmss"))
            Directory.CreateDirectory(_sandbox)
            Return True
        End Function

        ''' <summary>Walk up from the test exe to the repo root, then use the
        ''' same ffmpeg the product deploys (Overlay\API-Core), with the dev
        ''' Overlay\bin layout as fallback.</summary>
        Private Function ResolveFfmpeg() As String
            Dim dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
            For depth As Integer = 0 To 10
                If dir Is Nothing Then Exit For
                Dim candidate As String = Path.Combine(dir.FullName, "Overlay", "API-Core", "ffmpeg.exe")
                If File.Exists(candidate) Then Return candidate
                Dim binCandidate As String = Path.Combine(dir.FullName, "Overlay", "bin", "Release", "net10.0-windows10.0.26100.0", "FFmpeg", "ffmpeg.exe")
                If File.Exists(binCandidate) Then Return binCandidate
                dir = dir.Parent
            Next
            Return ""
        End Function

        Private Function MakeSettings(outputDir As String, systemAudio As Boolean) As CaptureSettings
            Dim s As New CaptureSettings()
            s.FFmpegPath = _ffmpegPath
            s.Encoder = "h264_nvenc"
            s.CaptureMethod = "ddagrab"
            s.FPS = 15
            s.Bitrate = 2000000L
            s.UseNativeResolution = True
            s.OutputDirectory = outputDir
            s.SystemAudioCapture = systemAudio
            s.MicCapture = False
            Return s
        End Function

        Private Function FfmpegCount() As Integer
            Dim procs As Process() = Process.GetProcessesByName("ffmpeg")
            Dim n As Integer = procs.Length
            For Each p As Process In procs
                Try : p.Dispose() : Catch : End Try
            Next
            Return n
        End Function

        ''' <summary>Poll until no more than maxCount ffmpeg.exe processes are
        ''' alive. Returns False on timeout (orphan evidence).</summary>
        Private Function WaitFfmpegAtMost(maxCount As Integer, budgetMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < budgetMs
                If FfmpegCount() <= maxCount Then Return True
                Thread.Sleep(100)
            End While
            Return FfmpegCount() <= maxCount
        End Function

        ' ───────────────────────────────────────────────────────────────

        ''' <summary>Deterministic contract test of the H2 JobObjectGuard
        ''' change: Assign reports True while the guard is alive, False after
        ''' Dispose (the old Sub silently "succeeded" here), False for null.</summary>
        Private Sub Test_JobGuardAssignContract()
            Dim psi As New ProcessStartInfo("cmd.exe", "/c timeout /t 30 /nobreak > NUL") With {
                .CreateNoWindow = True,
                .UseShellExecute = False
            }
            Using p As Process = Process.Start(psi)
                Dim guard As New JobObjectGuard()
                Try
                    TestRunner.Assert(guard.Assign(p), "Assign on a live guard must return True (ownership granted)")
                Finally
                    guard.Dispose()
                End Try
                TestRunner.Assert(Not guard.Assign(p), "Assign after Dispose must return False (H2: no silent success)")
                TestRunner.Assert(Not guard.Assign(Nothing), "Assign(Nothing) must return False")
                Try
                    p.Kill()
                    p.WaitForExit(3000)
                Catch
                End Try
            End Using
        End Sub

        ''' <summary>H2: race Dispose() against StartRecordingAsync across the
        ''' critical window (before/during/after Process.Start). Every started
        ''' ffmpeg must be owned by the job guard or terminated — asserted as
        ''' "no ffmpeg.exe above baseline after each round".</summary>
        Private Sub Test_DisposeDuringStart_NoOrphan()
            Dim baseline As Integer = FfmpegCount()
            Dim delays As Integer() = {0, 10, 25, 50, 100}

            For Each delayMs As Integer In delays
                Dim engine As New EngineCapture(MakeSettings(_sandbox, False))
                Dim outputPath As String = Path.Combine(_sandbox, $"disposeRace_{delayMs}.mp4")

                Dim startTask As Task(Of Boolean) = engine.StartRecordingAsync(outputPath)
                Thread.Sleep(delayMs)
                engine.Dispose()
                Dim started As Boolean = startTask.GetAwaiter().GetResult()

                TestRunner.Assert(Not engine.IsRecordingLifecycleActive,
                                  $"delay={delayMs}ms: lifecycle still active after Dispose (started={started})")
                TestRunner.Assert(WaitFfmpegAtMost(baseline, 8000),
                                  $"delay={delayMs}ms: orphan ffmpeg detected (started={started})")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"delay={delayMs}ms: engine state after Dispose was {engine.State}")
            Next
        End Sub

        ''' <summary>H1: while the stop flow is inside its MUX phase, the
        ''' lifecycle predicate the fixed UI guards use must hold, and a start
        ''' request arriving in that window must be rejected — then the mux
        ''' completes and the final output file exists (old session intact).</summary>
        Private Sub Test_StartDuringMuxing_ProtectsOldSession()
            Dim engine As New EngineCapture(MakeSettings(_sandbox, True))
            Dim outputPath As String = Path.Combine(_sandbox, "muxguard.mp4")

            Dim muxObserved As Boolean = False
            Dim predicateDuringMux As Boolean = False
            Dim startDuringMuxResult As Boolean? = Nothing

            ' Fires on the stop-flow worker thread, synchronously inside
            ' SetState(CaptureState.Muxing) — exactly the moment the fixed
            ' UI guard must refuse to dispose the old engine.
            AddHandler engine.StateChanged,
                Sub(s As EngineCapture.CaptureState)
                    If s = EngineCapture.CaptureState.Muxing Then
                        muxObserved = True
                        predicateDuringMux = engine.IsRecordingLifecycleActive
                        ' Start request arriving DURING muxing: the UI guard
                        ' rejects it before any Dispose — at engine level this
                        ' must be refused too (not idle).
                        Dim probeTask As Task(Of Boolean) =
                            engine.StartRecordingAsync(Path.Combine(_sandbox, "during_mux.mp4"))
                        startDuringMuxResult = probeTask.GetAwaiter().GetResult()
                    End If
                End Sub

            Dim started As Boolean = engine.StartRecordingAsync(outputPath).GetAwaiter().GetResult()
            TestRunner.Assert(started, "StartRecordingAsync returned False (environment: check encoder/audio)")
            Thread.Sleep(3000)

            Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
            TestRunner.Assert(stopped, "StopRecordingAsync returned False")

            TestRunner.Assert(muxObserved, "Muxing state never observed — two-process stop flow did not run")
            TestRunner.Assert(predicateDuringMux, "IsRecordingLifecycleActive was False during Muxing (H1 guard would not hold)")
            TestRunner.Assert(startDuringMuxResult.HasValue, "start-during-mux probe never ran")
            TestRunner.Assert(Not startDuringMuxResult.Value, "StartRecordingAsync during Muxing was NOT rejected")
            TestRunner.Assert(File.Exists(outputPath), $"final output missing after mux: {outputPath}")
            TestRunner.Assert(Not engine.IsRecordingLifecycleActive, "lifecycle still active after stop completed")
            TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle, $"post-stop state was {engine.State}")

            ' Old session was never disposed mid-mux: the engine is still
            ' usable and disposes cleanly now that the lifecycle is done.
            engine.Dispose()
        End Sub

        ''' <summary>H2: six full Start → record → Stop → Dispose cycles with
        ''' jittered timing must never accumulate ffmpeg.exe processes.</summary>
        Private Sub Test_RepeatedCycles_NoOrphanAccumulation()
            Dim baseline As Integer = FfmpegCount()
            Dim rnd As New Random(20260905)

            For i As Integer = 1 To 6
                Dim engine As New EngineCapture(MakeSettings(_sandbox, False))
                Dim outputPath As String = Path.Combine(_sandbox, $"stress_{i}.mp4")

                Dim started As Boolean = engine.StartRecordingAsync(outputPath).GetAwaiter().GetResult()
                If started Then
                    Thread.Sleep(rnd.Next(200, 500))
                    Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                    TestRunner.Assert(stopped, $"cycle {i}: StopRecordingAsync returned False")
                    TestRunner.Assert(File.Exists(outputPath), $"cycle {i}: output file missing — {outputPath}")
                Else
                    Console.Write($"(cycle {i}: start rejected — verifying no process left) ")
                End If

                engine.Dispose()
                Thread.Sleep(rnd.Next(0, 40))
                TestRunner.Assert(WaitFfmpegAtMost(baseline, 8000),
                                  $"cycle {i}: ffmpeg.exe count did not return to baseline — orphan accumulation")
            Next
        End Sub

    End Module

End Namespace
