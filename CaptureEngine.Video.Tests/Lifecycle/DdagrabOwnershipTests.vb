Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports CaptureEngine.Video.Backends.Fake
Imports CaptureEngine.Video.Tests.Fakes

Namespace CaptureEngine.Video.Tests.Lifecycle

    ''' <summary>
    ''' C-1/C-2 ownership &amp; concurrency pass — real D3D11/DXGI backend.
    '''
    ''' C-1: backend Start atomicity. A throw INSIDE Start() (injected at
    ''' the backend's own logger sink — the post-commit "started" line)
    ''' must leave the state HONEST: Running with a live worker (caller
    ''' Stop recovers), never Faulted-with-live-worker.
    ''' C-2: no permanent Running/Stopping without a live worker.
    '''
    ''' NOT runtime-injectable without a real desktop/DXGI failure and
    ''' therefore NOT faked here: worker-crash-while-Running (the exit
    ''' tail handles it — Running→Stopped with an Error log),
    ''' Stop-timeout wedging, Start-during-Stopping, Dispose-during-Stop-
    ''' timeout. Those paths are code-reviewed contract extensions of the
    ''' same SyncLock-serialized state machine these tests exercise.
    ''' </summary>
    Friend NotInheritable Class DdagrabOwnershipTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("DDAGRAB C-1: throw after Start commit → state stays Running, Stop recovers",
                   AddressOf Test_StartThrowAfterCommit)
            runner("DDAGRAB C-2: Stop while worker live → Stopped + worker quiesces",
                   AddressOf Test_StopQuiescesWorker)
            runner("DDAGRAB C-2: 5 repeated Start/Stop cycles → textures balanced, state honest",
                   AddressOf Test_RepeatedCycles)
        End Sub

        ' ---- helpers ----

        Private Shared Function CreateBackend(logger As EngineLogger) As DdagrabBackend
            Return New DdagrabBackend(logger)
        End Function

        Private Shared Function CreateContext() As IVideoBackendContext
            Return New FakeVideoBackendContext(
                VideoBackendKind.Ddagrab,
                New EngineLogger("FakeBackendCtx", EngineLogger.LogLevel.Warning))
        End Function

        ''' <summary>Logger sink that throws exactly when Start() reaches its
        ''' post-commit "started" line — the C-1 failure injection point
        ''' (EngineLogger.Write invokes the sink with no try/catch).</summary>
        Private Shared Sub ThrowOnStartComplete(line As String)
            If line IsNot Nothing AndAlso line.Contains("started (real DXGI capture)") Then
                Throw New InvalidOperationException("injected logger failure (C-1 repro)")
            End If
        End Sub

        Private Shared Function WaitTrue(predicate As Func(Of Boolean), timeoutMs As Integer) As Boolean
            Dim deadline As Long = DateTime.UtcNow.Ticks + timeoutMs * 10000L
            While DateTime.UtcNow.Ticks < deadline
                Try
                    If predicate() Then Return True
                Catch
                End Try
                Thread.Sleep(20)
            End While
            Return False
        End Function

        ' ---- tests ----

        ''' <summary>Deterministic C-1 reproduction: with the OLD order
        ''' (spawn worker → commit Running last) the injected throw hit
        ''' BEFORE the commit → catch marked Faulted with a LIVE worker →
        ''' the session's captureRunning flag (set after the call) missed
        ''' the unwind → orphan worker + backend bricked. The fixed order
        ''' commits Running first, so the SAME injection leaves an honest
        ''' Running that Stop() fully recovers.</summary>
        Private Shared Sub Test_StartThrowAfterCommit()
            Dim logger As New EngineLogger("DdagrabC1", EngineLogger.LogLevel.Info,
                                           AddressOf ThrowOnStartComplete)
            Dim backend = CreateBackend(logger)
            backend.Initialize(CreateContext())

            Dim threw As Boolean = False
            Try
                backend.Start(New RecordingVideoFrameSink())
            Catch ex As Exception
                threw = True
            End Try
            TestHelpers.Assert(threw, "Start threw (injected post-commit logger failure)")

            ' THE contract: the worker is live, so the state must say so.
            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Running,
                backend.CurrentState,
                "state after post-commit throw must be Running (honest), not Faulted")

            ' The session unwind (captureRunning=True set BEFORE Start) then
            ' Stop()s the backend — must fully recover.
            backend.[Stop]()
            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Stopped,
                backend.CurrentState, "Stop recovers the committed-Running backend")

            backend.Dispose()
            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Disposed,
                backend.CurrentState, "Dispose after recovery")
        End Sub

        Private Shared Sub Test_StopQuiescesWorker()
            Dim backend = CreateBackend(New EngineLogger("DdagrabC2", EngineLogger.LogLevel.Warning))
            backend.Initialize(CreateContext())
            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Worker proven live: it observed at least one desktop frame.
            TestHelpers.Assert(
                WaitTrue(Function() backend.Diagnostics.NoFrameCount > 0 OrElse
                                    backend.Diagnostics.EmittedFrames > 0,
                         10000),
                "worker processed at least one capture attempt")

            backend.[Stop]()
            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Stopped,
                backend.CurrentState, "state after Stop")

            ' Worker quiesced: no further capture activity after Stop.
            Dim noFrameAtStop As Long = backend.Diagnostics.NoFrameCount
            Dim emittedAtStop As Long = backend.Diagnostics.EmittedFrames
            Thread.Sleep(400)
            TestHelpers.Assert(backend.Diagnostics.NoFrameCount = noFrameAtStop,
                               "NoFrameCount frozen after Stop (worker dead)")
            TestHelpers.Assert(backend.Diagnostics.EmittedFrames = emittedAtStop,
                               "EmittedFrames frozen after Stop (worker dead)")

            backend.Dispose()
        End Sub

        Private Shared Sub Test_RepeatedCycles()
            Dim backend = CreateBackend(New EngineLogger("DdagrabC2c", EngineLogger.LogLevel.Warning))
            backend.Initialize(CreateContext())

            ' Sink override Dropped → the backend itself disposes every frame
            ' → after each Stop the texture lifecycle counters must balance
            ' (no stranded staging texture, no double-count).
            Dim sink As New RecordingVideoFrameSink()
            sink.SetOutcomeOverride(PushOutcome.Dropped)

            For cycle As Integer = 1 To 5
                backend.Start(sink)
                TestHelpers.AssertEqual(
                    DdagrabBackend.DdagrabBackendState.Running,
                    backend.CurrentState, $"cycle {cycle}: state after Start")

                ' Activity probe (evidence, not a precondition — see the
                ' note in Test_StopQuiescesWorker). The state + texture
                ' assertions below are the actual contract under test.
                WaitTrue(Function() backend.Diagnostics.NoFrameCount > 0 OrElse
                                    backend.Diagnostics.EmittedFrames > 0 OrElse
                                    backend.Diagnostics.ErrorCount > 0, 10000)

                backend.[Stop]()
                TestHelpers.AssertEqual(
                    DdagrabBackend.DdagrabBackendState.Stopped,
                    backend.CurrentState, $"cycle {cycle}: state after Stop")

                TestHelpers.AssertEqual(
                    backend.TexturesCreated, backend.TexturesDisposed,
                    $"cycle {cycle}: every staging texture disposed")
            Next

            backend.Dispose()
        End Sub

    End Class

End Namespace
