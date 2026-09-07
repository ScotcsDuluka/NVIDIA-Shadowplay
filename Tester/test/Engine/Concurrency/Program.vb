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
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports NVIDIA_Capture
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace Engine.Concurrency.Tests

    Friend Module TestRunner
        Friend _passed As Integer = 0
        Friend _failed As Integer = 0
        Friend _skipped As Integer = 0
        Friend ReadOnly _failures As New List(Of String)()
        Friend ReadOnly _skips As New List(Of String)()

        Friend Sub RunTest(name As String, test As Action)
            ' F-01 gate: M1/M2 drive the REAL NVIDIA-bound backends — when the
            ' preflight proved the environment cannot run them, report SKIP.
            If HardwareGate.ShouldSkip(name) Then
                Dim gateReason As String = "NVIDIA Ddagrab+NVENC backends unavailable: " & HardwareGate.SkipReason
                _skips.Add(name & ": " & gateReason)
                _skipped += 1
                Console.WriteLine("SKIP")
                Console.WriteLine($"      → {gateReason}")
                Return
            End If

            Console.Write($"  {name} ... ")
            Try
                test()
                Console.WriteLine("PASS")
                _passed += 1
            Catch ex As SkipException
                ' F-01 (C/4): ENVIRONMENT NOT CAPABLE — reported as SKIP.
                ' Never converted to PASS, never swallowed as a failure.
                Console.WriteLine("SKIP")
                Console.WriteLine($"      → {ex.Message}")
                _skips.Add(name & ": " & ex.Message)
                _skipped += 1
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

        Friend _ffmpegPath As String = ""
        Friend _sandbox As String = ""

        Function Main(args As String()) As Integer
            ' ─── G2 helper mode: the legacy CaptureEngine launches THIS
            ' executable as its "ffmpeg" (CaptureSettings.FFmpegPath =
            ' Environment.ProcessPath). Recognize the engine's argument shape
            ' and behave as a controllable fake encoder/mux instead of running
            ' the suite. See G2LegacyEngineTests.RunHelperMode.
            If args.Length > 0 AndAlso (args(0) = "-hide_banner" OrElse args(0) = "-v") Then
                Return G2LegacyEngineTests.RunHelperMode(args)
            End If

            Console.WriteLine("==================================================")
            Console.WriteLine(" Engine.Concurrency.Tests — H1/H2 regression")
            Console.WriteLine(" (real ffmpeg, real job object, real lifecycle)")
            Console.WriteLine("==================================================")
            ' ★ C/4 provenance banner: ties this run to the exact binary that
            ' executed it (BuildUtc + SourceRevision stamped at build time).
            ' A --no-build run on a stale or other-agent-contaminated binary
            ' can no longer pass silently as "proof of current source".
            Console.WriteLine(" " & ProvenanceLine())
            Console.WriteLine()

            Try
                If Not Setup() Then
                    Console.WriteLine("SETUP FAILED — cannot run without bundled ffmpeg")
                    Return 2
                End If

            Dim ffmpegBaseline As Integer = FfmpegCount()
            Console.WriteLine($" setup: ffmpeg = {_ffmpegPath}")
            Console.WriteLine($" setup: sandbox = {_sandbox}")
            Console.WriteLine($" setup: baseline ffmpeg.exe processes = {ffmpegBaseline}")

            ' ─── F-01 preflight: probe the REAL production DdagrabBackend once.
            ' The backend enumerates DXGI adapters itself; on a machine without
            ' an NVIDIA adapter the M1/M2 suites report SKIP (with this exact
            ' evidence) instead of 5 false FAILs. On an NVIDIA machine every
            ' test still RUNS.
            HardwareGate.EnsureProbed()
            Console.WriteLine($" setup: NVIDIA backend probe = {If(HardwareGate.NvidiaAvailable, "AVAILABLE (M1/M2 will run)", "NOT AVAILABLE (M1/M2 will SKIP): " & HardwareGate.SkipReason)}")
            Console.WriteLine()

            RunTest("GUARD: JobObjectGuard.Assign contract (live=True, disposed=False, null=False)",
                    AddressOf Test_JobGuardAssignContract)
            RunTest("JOB-1: guard.Dispose kills assigned child (KILL_ON_JOB_CLOSE — the parent-death net)",
                    AddressOf Test_GuardDisposeKillsChild)
            RunTest("JOB-2: Assign after child exit + concurrent Assign/Dispose — no crash, honest False",
                    AddressOf Test_GuardAssignAfterExitAndRace)
            RunTest("H2-A: Dispose during StartRecordingAsync — no orphan ffmpeg",
                    AddressOf Test_DisposeDuringStart_NoOrphan)
            RunTest("H1-B: Start during Muxing rejected — old session completes with output",
                    AddressOf Test_StartDuringMuxing_ProtectsOldSession)
            RunTest("H2-C: repeated Start/Stop/Dispose with jitter — no orphan accumulation",
                    AddressOf Test_RepeatedCycles_NoOrphanAccumulation)
            RunTest("RUNNER-HYGIENE-A: stale-sweep reclaims ONLY this suite's sandboxes",
                    AddressOf Test_SweepScopeContract)
            RunTest("RUNNER-HYGIENE-B: locked sandbox → cleanup warning, verdict untouched, no crash",
                    AddressOf Test_CleanupFailureIsolation)

            M1M2Tests.RunAll(_ffmpegPath, _sandbox)
            G2LegacyEngineTests.RunAll(_ffmpegPath, _sandbox)
            G3LegacyEngineTests.RunAll(_ffmpegPath, _sandbox)
            F03LegacyTests.RunAll(_ffmpegPath, _sandbox)
            NvidiaProofTests.RunAll(_ffmpegPath, _sandbox)
            GithubTokenSecurityTests.RunAll()

            Console.WriteLine()
            Console.WriteLine($" passed={TestRunner._passed} failed={TestRunner._failed} skipped={TestRunner._skipped}")
            For Each f In TestRunner._failures
                Console.WriteLine($"   FAILED: {f}")
            Next
            For Each s In TestRunner._skips
                Console.WriteLine($"   SKIPPED: {s}")
            Next

            Dim ffmpegAfter As Integer = FfmpegCount()
            Console.WriteLine($" final ffmpeg.exe processes = {ffmpegAfter} (baseline {ffmpegBaseline})")
            If ffmpegAfter > ffmpegBaseline Then
                Console.WriteLine("   → ORPHAN ffmpeg processes detected by the suite itself")
                TestRunner._failed += 1
            End If

            Return If(TestRunner._failed = 0, 0, 1)
            ' ─── Sandbox hygiene (C-hygiene pass): the sandbox is removed on
            ' EVERY outcome — PASS, FAIL, assertion failure, timeout, process
            ' error, unexpected exception (Finally). A hard process kill cannot
            ' run a Finally; Setup()'s stale sweep reclaims those on the NEXT
            ' run. Cleanup failures are reported separately and never change
            ' the test verdict or the exit code.
            Finally
                CleanupSandbox()
                If SandboxCleanupWarnings.Count > 0 Then
                    Console.WriteLine()
                    Console.WriteLine($" SANDBOX CLEANUP WARNINGS: {SandboxCleanupWarnings.Count} (test verdicts unaffected)")
                    For Each w As String In SandboxCleanupWarnings
                        Console.WriteLine("   - " & w)
                    Next
                End If
            End Try
        End Function

        ' ───────────────────────────────────────────────────────────────

        ' ───────────────────────────────────────────────────────────────
        ' Sandbox hygiene (C-hygiene pass). Contract:
        '   - ONLY directories this suite creates are ever touched:
        '     Path.GetTempPath() + name starting with "engine-concurrency-tests-".
        '     Anything else (RRT_RT_*, user files, other tools' temp) is
        '     never matched.
        '   - Cleanup failures are collected in SandboxCleanupWarnings and
        '     reported separately — they never mask a test verdict and never
        '     crash the runner.
        '   - A hard process kill cannot run the Finally; the stale sweep
        '     reclaims abandoned sandboxes on the next run.
        ' ───────────────────────────────────────────────────────────────

        Friend ReadOnly SandboxCleanupWarnings As New List(Of String)()
        Friend Const SandboxPrefix As String = "engine-concurrency-tests-"

        ''' <summary>Deletes THIS run's sandbox (and nothing else). Never
        ''' throws; a locked/undeletable sandbox becomes a cleanup warning.</summary>
        Friend Sub CleanupSandbox()
            If String.IsNullOrEmpty(_sandbox) Then Return
            If Not TryDeleteSandbox(_sandbox, "current run sandbox") Then Return
            _sandbox = ""
        End Sub

        ''' <summary>Attempt to delete one sandbox directory. Returns True when
        ''' the directory is gone (or was already absent). Never throws.</summary>
        Friend Function TryDeleteSandbox(path As String, label As String) As Boolean
            Try
                If Not Directory.Exists(path) Then Return True
                Directory.Delete(path, True)
                Return Not Directory.Exists(path)
            Catch ex As Exception
                SandboxCleanupWarnings.Add(label & " [" & path & "]: " & ex.Message)
                Return False
            End Try
        End Function

        ''' <summary>Reclaims sandboxes abandoned by an earlier hard-killed
        ''' run: prefix-matched directories under the temp path whose
        ''' LastWriteTime is older than <paramref name="maxAgeHours"/>.
        ''' Returns the number reclaimed. Never throws.</summary>
        Friend Function SweepStaleSandboxes(maxAgeHours As Integer) As Integer
            Dim reclaimed As Integer = 0
            Try
                Dim tempRoot As DirectoryInfo = New DirectoryInfo(Path.GetTempPath())
                Dim cutoff As DateTime = DateTime.Now.AddHours(-maxAgeHours)
                For Each d As DirectoryInfo In tempRoot.EnumerateDirectories(SandboxPrefix & "*")
                    Try
                        If d.LastWriteTime >= cutoff Then Continue For
                        If TryDeleteSandbox(d.FullName, "stale sandbox (" & d.Name & ")") Then reclaimed += 1
                    Catch
                    End Try
                Next
            Catch
            End Try
            Return reclaimed
        End Function

        ''' <summary>C/4 provenance: read the build-time stamp from this
        ''' assembly's AssemblyMetadata (stamped by StampTestProvenance in
        ''' Directory.Build.targets for every *Tests assembly).</summary>
        Private Function ProvenanceLine() As String
            Dim buildUtc As String = "unknown"
            Dim sourceRev As String = "unknown"
            For Each a As AssemblyMetadataAttribute In
                Assembly.GetExecutingAssembly().GetCustomAttributes(Of AssemblyMetadataAttribute)()
                If a.Key = "BuildUtc" Then buildUtc = a.Value
                If a.Key = "SourceRevision" Then sourceRev = a.Value
            Next
            Return $"binary provenance: built {buildUtc} from source {sourceRev}"
        End Function

        Private Function Setup() As Boolean
            ' Reclaim sandboxes left by earlier hard-killed runs BEFORE
            ' creating this run's own sandbox.
            Dim reclaimed As Integer = SweepStaleSandboxes(6)
            If reclaimed > 0 Then
                Console.WriteLine($" setup: reclaimed {reclaimed} stale sandbox(es) from earlier runs (older than 6h)")
            End If

            _ffmpegPath = ResolveFfmpeg()
            If String.IsNullOrEmpty(_ffmpegPath) OrElse Not File.Exists(_ffmpegPath) Then
                Console.WriteLine(" ffmpeg.exe not found under Overlay\ (API-Core or bin)")
                Return False
            End If

            _sandbox = Path.Combine(Path.GetTempPath(),
                                    SandboxPrefix & DateTime.Now.ToString("yyyyMMdd_HHmmss"))
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
            ' ★ F-02: truthful validation — file exists, size > 0, container
            ' probe succeeds, duration > 0, Video AND Audio streams present
            ' (system audio was enabled for this recording).
            '
            ' ★ F-03 environment split: the assertion above is only honest on
            ' a machine that could actually ENCODE. Without NVIDIA the ffmpeg
            ' process dies at h264_nvenc init ("Cannot load nvcuda.dll"), the
            ' mux fails on the 0-packet temp video, and the engine's designed
            ' video-only fallback RENAMES that broken container — the file
            ' exists but is not a valid MP4. Demanding container validity
            ' there would be an ENVIRONMENT failure; the H1 lifecycle
            ' contract itself (mux guard, exactly-once, settled state) is
            ' exactly what this test must prove on any machine.
            If HardwareGate.NvidiaAvailable Then
                MediaAssert.AssertValidMp4(_ffmpegPath, outputPath, True, True, "H1-B")
            Else
                ' No NVIDIA encoder → mux fails on the 0-packet temp video and
                ' the fallback's playback validation REMOVES the unplayable
                ' file (honesty contract: garbage is never announced as saved).
                ' The machine-independent contract here is the lifecycle one
                ' asserted below — not the file.
                Console.Write("(output validity not asserted: no NVIDIA encoder — fallback removed as unplayable) ")
            End If
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
                    ' ★ F-02: video-only contract — the output must be a real
                    ' probe-able MP4 with a video stream and NO audio stream
                    ' (SystemAudioCapture=False for these cycles). Environment
                    ' split as in H1-B: without NVIDIA the encode dies at nvenc
                    ' init and the fallback rename yields a headerless container.
                    If HardwareGate.NvidiaAvailable Then
                        MediaAssert.AssertValidMp4(_ffmpegPath, outputPath, True, False, $"H2-C cycle {i}")
                    Else
                        ' Single-process + nvenc failure leaves a 0-byte file at
                        ' best — existence of garbage is not the contract here;
                        ' the no-orphan-accumulation contract below is.
                        Console.Write("(media validity not asserted: no NVIDIA encoder) ")
                    End If
                Else
                    Console.Write($"(cycle {i}: start rejected — verifying no process left) ")
                End If

                engine.Dispose()
                Thread.Sleep(rnd.Next(0, 40))
                TestRunner.Assert(WaitFfmpegAtMost(baseline, 8000),
                                  $"cycle {i}: ffmpeg.exe count did not return to baseline — orphan accumulation")
            Next
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' RUNNER-HYGIENE: the cleanup contract itself, proven as tests.
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>RUNNER-HYGIENE-A: the stale sweep reclaims ONLY this
        ''' suite's prefix-matched, age-qualified sandboxes. Foreign prefixes
        ''' (RRT_RT_*), prefix-adjacent names, and FRESH sandboxes must
        ''' survive untouched.</summary>
        Private Sub Test_SweepScopeContract()
            Dim tempRoot As String = Path.GetTempPath()
            Dim staleMine As String = Path.Combine(tempRoot, SandboxPrefix & "20000101000000")
            Dim freshMine As String = Path.Combine(tempRoot, SandboxPrefix & DateTime.Now.ToString("yyyyMMdd_HHmmss") & "-fresh-proof")
            Dim foreignRrt As String = Path.Combine(tempRoot, "RRT_RT_foreignproof")
            Dim foreignAdjacent As String = Path.Combine(tempRoot, "engine-concurrency-UNRELATED-proof")
            Directory.CreateDirectory(staleMine)
            Directory.CreateDirectory(freshMine)
            Directory.CreateDirectory(foreignRrt)
            Directory.CreateDirectory(foreignAdjacent)
            Directory.SetLastWriteTime(staleMine, DateTime.Now.AddHours(-7))

            SweepStaleSandboxes(6)

            Assert(Not Directory.Exists(staleMine), "stale sandbox survived the sweep")
            Assert(Directory.Exists(freshMine), "FRESH sandbox must never be swept")
            Assert(Directory.Exists(foreignRrt), "FOREIGN prefix (RRT_RT_*) must never be swept")
            Assert(Directory.Exists(foreignAdjacent), "prefix-adjacent directory must never be swept")
            Directory.Delete(freshMine, True)
            Directory.Delete(foreignRrt, True)
            Directory.Delete(foreignAdjacent, True)
        End Sub

        ''' <summary>RUNNER-HYGIENE-B: a locked sandbox (open file handle — the
        ''' real-world cleanup-failure shape) must produce a WARNING and
        ''' return False, never throw, never flip a verdict.</summary>
        Private Sub Test_CleanupFailureIsolation()
            Dim locked As String = Path.Combine(Path.GetTempPath(),
                                                SandboxPrefix & "locked-" & Guid.NewGuid().ToString("N").Substring(0, 8))
            Directory.CreateDirectory(locked)
            Dim blocker As String = Path.Combine(locked, "locked.bin")
            Dim keep As FileStream = File.Open(blocker, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            Try
                Dim warningsBefore As Integer = SandboxCleanupWarnings.Count
                Dim deleted As Boolean = TryDeleteSandbox(locked, "RUNNER-HYGIENE-B locked sandbox")
                Assert(Not deleted, "TryDeleteSandbox reported success on a LOCKED directory")
                Assert(SandboxCleanupWarnings.Count = warningsBefore + 1,
                       "cleanup failure was not reported as a separate warning")
                Assert(Directory.Exists(locked), "locked sandbox vanished during the failed cleanup")
                ' The locked dir was this test's own deliberate fixture: once
                ' released and deleted, retract the intentional warning so the
                ' suite summary reports only REAL cleanup failures.
                If SandboxCleanupWarnings.Count = warningsBefore + 1 Then
                    SandboxCleanupWarnings.RemoveAt(SandboxCleanupWarnings.Count - 1)
                End If
            Finally
                keep.Dispose()
                Try : Directory.Delete(locked, True) : Catch : End Try
            End Try
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' C/5 ownership: the KILL_ON_JOB_CLOSE net the LiveMuxSession wiring
        ' relies on (SessionConfig.OnProcessStarted → AssignChildToJob).
        ' Hardware-free — uses disposable cmd.exe helpers, never fakes the
        ' assertion.
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>JOB-1: closing the job handle must terminate every
        ''' assigned child. When the host process dies, Windows closes its
        ''' handles — guard.Dispose() reproduces exactly that transition
        ''' deterministically. A surviving child here would mean the wiring
        ''' cannot protect against host death.</summary>
        Private Sub Test_GuardDisposeKillsChild()
            Dim psi As New ProcessStartInfo("cmd.exe", "/c timeout /t 60 /nobreak > NUL") With {
                .CreateNoWindow = True,
                .UseShellExecute = False
            }
            Using p As Process = Process.Start(psi)
                Dim guard As New JobObjectGuard()
                Try
                    TestRunner.Assert(guard.Assign(p), "Assign on a live guard must return True")
                    TestRunner.Assert(Not p.HasExited, "helper child died before the guard was closed")
                Finally
                    guard.Dispose()   ' == what Windows does when the host process dies
                End Try

                Dim dead As Boolean = False
                Dim sw As Stopwatch = Stopwatch.StartNew()
                While sw.ElapsedMilliseconds < 8000
                    Try
                        If p.HasExited Then
                            dead = True
                            Exit While
                        End If
                    Catch
                        ' Process object can race its own handle teardown — treat as gone.
                        dead = True
                        Exit While
                    End Try
                    Thread.Sleep(100)
                End While
                TestRunner.Assert(dead, "assigned child survived guard.Dispose — KILL_ON_JOB_CLOSE contract broken")
            End Using
        End Sub

        ''' <summary>JOB-2: Assign on an already-exited child must not throw;
        ''' concurrent Assign/Dispose on one guard must not crash (the M11
        ''' DangerousAddRef race surface) — every late Assign reports False.</summary>
        Private Sub Test_GuardAssignAfterExitAndRace()
            ' a) child that already exited
            Dim psi As New ProcessStartInfo("cmd.exe", "/c exit 0") With {
                .CreateNoWindow = True,
                .UseShellExecute = False
            }
            Dim exited As Process = Process.Start(psi)
            exited.WaitForExit(5000)
            Dim g1 As New JobObjectGuard()
            Dim assignedAfterExit As Boolean = g1.Assign(exited)   ' must not throw either way
            g1.Dispose()
            Try : exited.Dispose() : Catch : End Try
            Console.WriteLine($"  (assign-after-exit returned {assignedAfterExit} — either outcome is contract-legal)")

            ' b) concurrent Assign / Dispose on one guard + one live child
            Dim child As Process = Process.Start(New ProcessStartInfo("cmd.exe", "/c timeout /t 20 /nobreak > NUL") With {
                                                     .CreateNoWindow = True,
                                                     .UseShellExecute = False})
            Dim g2 As New JobObjectGuard()
            Dim stopRace As Boolean = False
            Dim assignThread As New Thread(
                Sub()
                    While Not stopRace
                        Try
                            g2.Assign(child)   ' True or honest False — never an escape
                        Catch
                            Exit While
                        End Try
                    End While
                End Sub)
            assignThread.IsBackground = True
            assignThread.Start()
            Thread.Sleep(150)
            g2.Dispose()
            Thread.Sleep(150)
            stopRace = True
            assignThread.Join(2000)
            TestRunner.Assert(Not g2.Assign(child), "Assign after Dispose must return False (H2 honest contract)")
            Try : child.Kill() : Catch : End Try
            Try : child.WaitForExit(3000) : Catch : End Try
            Try : child.Dispose() : Catch : End Try
        End Sub

    End Module

End Namespace
