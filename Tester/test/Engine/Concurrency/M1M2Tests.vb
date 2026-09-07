Option Strict On
Option Explicit On
Option Infer On

' M1M2Tests.vb — forensic round 2 regression tests (M1 + M2 only).
'
' M1 — CaptureSession failure cleanup:
'   An exception after _capture.Start()/_encoder.Start() used to leave the
'   capture worker running (owner-less frames forever) and the encoder
'   Running — the next session's Start() then no-opped and skipped the
'   FORCEIDR/SPS-PPS re-arm, producing a header-less H.264 stream. The fix
'   unwinds both in Run()'s Finally. These tests inject DETERMINISTIC
'   failures through the session logger sink (the one seam that fires
'   exactly at a marked log line — EngineLogger invokes its sink without a
'   try/catch) on REAL Ddagrab + NVENC backends, then require a follow-up
'   session on the SAME backends to record normally.
'
' M2 — DdagrabBackend stop lifecycle:
'   A Stop() join timeout used to declare Stopped while the worker was
'   still executing — a follow-up Start() then spawned a SECOND worker onto
'   the same duplication/device. The fix keeps 'Stopping' (Start rejected
'   from there) and the worker's exit tail completes Stopping→Stopped the
'   moment the generation actually terminates. The stall is forced
'   deterministically through a blocking test sink on the REAL backend —
'   no GPU mocks.

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.Recording
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports DdagrabState = CaptureEngine.Video.Backends.Ddagrab.DdagrabBackend.DdagrabBackendState

Namespace Engine.Concurrency.Tests

    Friend Module M1M2Tests

        Private _ffmpeg As String = ""
        Private _sandbox As String = ""
        Private ReadOnly _consoleSync As New Object()

        Friend Sub RunAll(ffmpegPath As String, sandboxDir As String)
            _ffmpeg = ffmpegPath
            _sandbox = sandboxDir

            Console.WriteLine("── M1: CaptureSession failure cleanup (real backends, injected failures) ──")
            TestRunner.RunTest("M1-T1: exception during initialization → worker never started + next session recovers", AddressOf Test_M1_InitFailure)
            TestRunner.RunTest("M1-T2: exception immediately after _capture.Start → worker stopped + next session recovers", AddressOf Test_M1_FailureAfterStart)
            TestRunner.RunTest("M1-T3: exception during finalize → worker stopped + next session recovers", AddressOf Test_M1_FailureDuringFinalize)

            Console.WriteLine("── M2: DdagrabBackend stop lifecycle (real backend, deterministic stall) ──")
            TestRunner.RunTest("M2-T1: join timeout keeps Stopping; Start rejected until the worker generation exits", AddressOf Test_M2_TimeoutKeepsStopping)
            TestRunner.RunTest("M2-STRESS: 50 Start/Stop cycles incl. slow/timeout/dispose — no leaks, no duplicate workers", AddressOf Test_M2_Stress50)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Shared plumbing
        ' ───────────────────────────────────────────────────────────────

        Private Sub DiscardLog(line As String)
            ' deliberate discard — keeps the suite output readable
        End Sub

        Private Function MakeBackendLogger() As EngineLogger
            Return New EngineLogger("m1m2-backend", EngineLogger.LogLevel.Warning, AddressOf DiscardLog)
        End Function

        Private Function MakeNormalSessionLogger() As EngineLogger
            Return New EngineLogger("m1m2-session", EngineLogger.LogLevel.Warning, AddressOf DiscardLog)
        End Function

        ''' <summary>The M1 failure injector: EngineLogger invokes its sink
        ''' synchronously WITHOUT a try/catch, so throwing here propagates out
        ''' of the exact CaptureSession log line carrying the marker — a
        ''' deterministic exception at that point of Run().</summary>
        Private NotInheritable Class InjectionLogger
            Public Fired As Boolean
            Public ReadOnly Logger As EngineLogger

            Public Sub New(marker As String)
                Logger = New EngineLogger("m1m2-session", EngineLogger.LogLevel.Info,
                    Sub(line)
                        If marker IsNot Nothing AndAlso Not Fired AndAlso line.Contains(marker) Then
                            Fired = True
                            Throw New InvalidOperationException("INJECTED FAILURE (marker): " & marker)
                        End If
                    End Sub)
            End Sub
        End Class

        Private NotInheritable Class TestBackendContext
            Implements IVideoBackendContext

            Private ReadOnly _log As EngineLogger

            Public Sub New(log As EngineLogger)
                _log = log
            End Sub

            Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
                Get
                    Return _log
                End Get
            End Property

            Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
                Get
                    Return VideoBackendKind.Ddagrab
                End Get
            End Property
        End Class

        ''' <summary>Real Ddagrab + NVENC stack — the exact production classes
        ''' RecordingEngine composes (CaptureSession borrows them).</summary>
        Private NotInheritable Class BackendStack
            Implements IDisposable

            Public ReadOnly Capture As DdagrabBackend
            Public ReadOnly Encoder As NvencEncoderBackend

            Public Sub New()
                Dim log As EngineLogger = MakeBackendLogger()
                Capture = New DdagrabBackend(log)
                Capture.Initialize(New TestBackendContext(log))
                Encoder = New NvencEncoderBackend(log)
                Dim cfg As New EncoderConfig() With {
                    .CodecKey = "NVENC_H264",
                    .BitrateBps = 2000000L,
                    .MinrateBps = 2000000L,
                    .MaxrateBps = 2000000L,
                    .BufsizeBps = 4000000L,
                    .GopSize = 60,
                    .RateControl = "cbr",
                    .Preset = "p4",
                    .FrameRateFps = 30,
                    .ExpectedWidth = Capture.OutputWidth,
                    .ExpectedHeight = Capture.OutputHeight,
                    .EncodeWidth = Capture.OutputWidth,
                    .EncodeHeight = Capture.OutputHeight
                }
                Encoder.Initialize(cfg)
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                Try : Encoder?.Dispose() : Catch : End Try
                Try : Capture?.Dispose() : Catch : End Try
            End Sub
        End Class

        Private Function MakeConfig(stack As BackendStack, outPath As String) As SessionConfig
            Return New SessionConfig() With {
                .OutputPath = outPath,
                .DurationSeconds = 2,
                .TargetFps = 30,
                .FFmpegPath = _ffmpeg,
                .AudioEnabled = False,
                .MicEnabled = False,
                .UseNativeResolution = True,
                .EncodeWidth = stack.Capture.OutputWidth,
                .EncodeHeight = stack.Capture.OutputHeight
            }
        End Function

        ''' <summary>Test sink: counts + disposes every pushed frame (Pushed →
        ''' sink owns the frame), optionally blocking every push (gate) or
        ''' slowing it (SlowMs) to park the worker deterministically.</summary>
        Private Class CountingSink
            Implements IVideoFrameSink

            Public BlockGate As ManualResetEvent = Nothing
            Public SlowMs As Integer = 0
            Public Entered As Boolean = False
            Public Received As Integer = 0

            Public Function TryPush(result As FrameAcquisitionResult) As PushOutcome Implements IVideoFrameSink.TryPush
                Entered = True
                Dim gate As ManualResetEvent = BlockGate
                If gate IsNot Nothing Then gate.WaitOne()
                If SlowMs > 0 Then Thread.Sleep(SlowMs)
                If result.Frame IsNot Nothing Then
                    Try : result.Frame.Dispose() : Catch : End Try
                End If
                Received += 1
                Return PushOutcome.Pushed
            End Function
        End Class

        ''' <summary>The M1 acceptance: a follow-up session on the SAME
        ''' backends records normally (frames received, pass, clean stop).</summary>
        Private Sub RecoverAndRecord(stack As BackendStack, fileName As String)
            Dim outPath As String = Path.Combine(_sandbox, fileName)
            Dim session As New CaptureSession(stack.Capture, stack.Encoder,
                                              MakeConfig(stack, outPath), MakeNormalSessionLogger())
            Dim result As SessionResult = session.Run()
            TestRunner.Assert(result.FramesEncoded > 0,
                              $"recovery session encoded 0 frames (err={result.ErrorMessage})")
            TestRunner.Assert(result.Pass, $"recovery session did not pass: {result.ErrorMessage}")
            TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopped,
                              $"post-recovery capture state was {stack.Capture.CurrentState}")
            TestRunner.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                              $"texture leak after recovery: created={stack.Capture.TexturesCreated} disposed={stack.Capture.TexturesDisposed}")
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' M1
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>Exception during initialization (before the video lifecycle
        ''' starts): capture/encoder must remain Initialized, zero textures,
        ''' and the next session on the same backends must recover.</summary>
        Private Sub Test_M1_InitFailure()
            Using stack As New BackendStack()
                Dim inject As New InjectionLogger("[session] Arming video capture")
                Dim session As New CaptureSession(stack.Capture, stack.Encoder,
                                                  MakeConfig(stack, Path.Combine(_sandbox, "m1_t1_fail.mp4")),
                                                  inject.Logger)
                Dim result As SessionResult = session.Run()
                TestRunner.Assert(inject.Fired, "injection marker never fired")
                TestRunner.Assert(Not String.IsNullOrEmpty(result.ErrorMessage),
                                  $"expected a session error, got pass={result.Pass}")
                TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Initialized,
                                  $"capture state after init failure was {stack.Capture.CurrentState} (expected Initialized)")
                TestRunner.Assert(stack.Encoder.CurrentState = EncoderState.Initialized,
                                  $"encoder state after init failure was {stack.Encoder.CurrentState} (expected Initialized)")
                TestRunner.Assert(stack.Capture.TexturesCreated = 0,
                                  "capture created textures although never started")
                RecoverAndRecord(stack, "m1_t1_recover.mp4")
            End Using
        End Sub

        ''' <summary>Exception IMMEDIATELY AFTER _capture.Start(): the M1 fix
        ''' must stop the capture worker and the encoder in Run()'s Finally —
        ''' before the fix the worker leaked Running and the encoder stayed
        ''' un-rearmed for the next session.</summary>
        Private Sub Test_M1_FailureAfterStart()
            Using stack As New BackendStack()
                Dim inject As New InjectionLogger("[session] Capture + Audio armed")
                Dim session As New CaptureSession(stack.Capture, stack.Encoder,
                                                  MakeConfig(stack, Path.Combine(_sandbox, "m1_t2_fail.mp4")),
                                                  inject.Logger)
                Dim result As SessionResult = session.Run()
                TestRunner.Assert(inject.Fired, "injection marker never fired")
                TestRunner.Assert(Not String.IsNullOrEmpty(result.ErrorMessage),
                                  $"expected a session error, got pass={result.Pass}")
                TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopped,
                                  $"LEAKED WORKER: capture state was {stack.Capture.CurrentState} after Run (M1 requires Stopped)")
                TestRunner.Assert(stack.Encoder.CurrentState = EncoderState.Stopped,
                                  $"encoder state was {stack.Encoder.CurrentState} after Run (M1 requires Stopped → next Start re-arms IDR)")
                TestRunner.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                  $"texture leak: created={stack.Capture.TexturesCreated} disposed={stack.Capture.TexturesDisposed}")
                RecoverAndRecord(stack, "m1_t2_recover.mp4")
            End Using
        End Sub

        ''' <summary>Exception during finalize (after the CFR loop, before the
        ''' inline capture stop): same cleanup contract as T2.</summary>
        Private Sub Test_M1_FailureDuringFinalize()
            Using stack As New BackendStack()
                Dim inject As New InjectionLogger("[session] Stop snapshot")
                Dim session As New CaptureSession(stack.Capture, stack.Encoder,
                                                  MakeConfig(stack, Path.Combine(_sandbox, "m1_t3_fail.mp4")),
                                                  inject.Logger)
                Dim result As SessionResult = session.Run()
                TestRunner.Assert(inject.Fired, "injection marker never fired")
                TestRunner.Assert(Not String.IsNullOrEmpty(result.ErrorMessage),
                                  $"expected a session error, got pass={result.Pass}")
                TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopped,
                                  $"LEAKED WORKER: capture state was {stack.Capture.CurrentState} after Run (M1 requires Stopped)")
                TestRunner.Assert(stack.Encoder.CurrentState = EncoderState.Stopped,
                                  $"encoder state was {stack.Encoder.CurrentState} after Run (M1 requires Stopped)")
                TestRunner.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                  $"texture leak: created={stack.Capture.TexturesCreated} disposed={stack.Capture.TexturesDisposed}")
                RecoverAndRecord(stack, "m1_t3_recover.mp4")
            End Using
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' M2
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>Deterministic stall: the worker parks inside a gated sink
        ''' push → Stop()'s 2s join times out → state must REMAIN Stopping and
        ''' Start must be REJECTED (the old code declared Stopped and let a
        ''' second worker spawn). When the gate opens the worker exits and its
        ''' tail completes Stopping→Stopped; restart then works with real
        ''' frames.</summary>
        Private Sub Test_M2_TimeoutKeepsStopping()
            Using stack As New BackendStack()
                Dim gate As New ManualResetEvent(False)
                Dim stalled As New CountingSink() With {.BlockGate = gate}
                stack.Capture.Start(stalled)

                Dim sw As Stopwatch = Stopwatch.StartNew()
                While Not stalled.Entered AndAlso sw.ElapsedMilliseconds < 5000
                    Thread.Sleep(10)
                End While
                TestRunner.Assert(stalled.Entered, "worker never reached the sink")

                stack.Capture.Stop()
                TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopping,
                                  $"state after join timeout was {stack.Capture.CurrentState} — M2 requires 'Stopping' (old code declared Stopped)")

                Dim rejected As Boolean = False
                Try
                    stack.Capture.Start(New CountingSink())
                Catch ex As InvalidOperationException
                    rejected = True
                End Try
                TestRunner.Assert(rejected,
                                  $"Start during Stopping was accepted (state={stack.Capture.CurrentState}) — duplicate-worker risk")

                gate.Set()
                Dim s2 As Stopwatch = Stopwatch.StartNew()
                While stack.Capture.CurrentState <> DdagrabState.Stopped AndAlso s2.ElapsedMilliseconds < 10000
                    Thread.Sleep(50)
                End While
                TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopped,
                                  $"state did not reach Stopped after the worker exited (still {stack.Capture.CurrentState})")

                Dim counting As New CountingSink()
                stack.Capture.Start(counting)
                Thread.Sleep(500)
                stack.Capture.Stop()
                TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopped,
                                  $"post-restart stop state was {stack.Capture.CurrentState}")
                TestRunner.Assert(counting.Received > 0, "restart received 0 frames")
                TestRunner.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                  $"texture leak: created={stack.Capture.TexturesCreated} disposed={stack.Capture.TexturesDisposed}")
            End Using
        End Sub

        ''' <summary>50 Start/Stop cycles on one backend — normal, slow and
        ''' timeout stops plus a final dispose — must not leak workers/threads/
        ''' textures or ever allow a second live worker.</summary>
        Private Sub Test_M2_Stress50()
            Dim proc As Process = Process.GetCurrentProcess()
            Dim threadsBefore As Integer = proc.Threads.Count

            Using stack As New BackendStack()
                Dim sink As New CountingSink()
                Dim rnd As New Random(20260906)

                For i As Integer = 1 To 50
                    Dim timeoutCycle As Boolean = (i = 30 OrElse i = 40)
                    Dim timeoutGate As ManualResetEvent = Nothing
                    If timeoutCycle Then
                        timeoutGate = New ManualResetEvent(False)
                        sink.BlockGate = timeoutGate
                    ElseIf i = 25 OrElse i = 45 Then
                        sink.SlowMs = 300      ' slow stop — join still succeeds
                    Else
                        sink.SlowMs = 0
                    End If

                    stack.Capture.Start(sink)
                    TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Running,
                                      $"cycle {i}: state after Start was {stack.Capture.CurrentState}")

                    Thread.Sleep(rnd.Next(30, 90))
                    stack.Capture.Stop()

                    If timeoutCycle Then
                        TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopping,
                                          $"cycle {i} (timeout): state was {stack.Capture.CurrentState} — expected Stopping")
                        timeoutGate.Set()
                        Dim s3 As Stopwatch = Stopwatch.StartNew()
                        While stack.Capture.CurrentState <> DdagrabState.Stopped AndAlso s3.ElapsedMilliseconds < 10000
                            Thread.Sleep(50)
                        End While
                    End If

                    TestRunner.Assert(stack.Capture.CurrentState = DdagrabState.Stopped,
                                      $"cycle {i}: state after Stop was {stack.Capture.CurrentState}")
                Next

                TestRunner.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                  $"texture leak over stress: created={stack.Capture.TexturesCreated} disposed={stack.Capture.TexturesDisposed}")

                ' Dispose contract: post-dispose Start must be refused.
                stack.Capture.Dispose()
                Dim disposeRejected As Boolean = False
                Try
                    stack.Capture.Start(sink)
                Catch ex As ObjectDisposedException
                    disposeRejected = True
                End Try
                TestRunner.Assert(disposeRejected, "Start after Dispose was accepted")
            End Using

            Dim threadsAfter As Integer = proc.Threads.Count
            Console.WriteLine($"      threads before={threadsBefore} after={threadsAfter} (all workers joined — growth indicates a leak)")
            TestRunner.Assert(threadsAfter <= threadsBefore + 8,
                              $"thread count grew {threadsBefore} → {threadsAfter} — worker leak")
        End Sub

    End Module

End Namespace
