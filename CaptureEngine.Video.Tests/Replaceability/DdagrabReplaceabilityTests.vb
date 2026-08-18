Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video.Backends.Ddagrab
Imports CaptureEngine.Video.Backends.Fake
Imports CaptureEngine.Video.Tests.Fakes

Namespace CaptureEngine.Video.Tests.Replaceability
    ''' <summary>
    ''' Replaceability tests for DdagrabBackend (GLM-1). The central proof
    ''' (P1-A v1.3.1 §8.4) that DdagrabBackend is interchangeable with
    ''' FakeVideoCaptureBackend modulo Sequence values.
    '''
    ''' In skeleton mode, DdagrabBackend emits NoFrame forever (no
    ''' FrameAvailable results reach the sink). The replaceability test
    ''' therefore verifies that:
    '''   - The sink receives ZERO results from DdagrabBackend (because
    '''     NoFrame is internal per §6.4).
    '''   - The Diagnostics surface (EmittedFrames / DroppedFrames /
    '''     ReplacedFrames / NoFrameCount / ErrorCount) increments
    '''     correctly: NoFrameCount > 0; others = 0.
    '''   - The backend survives both a recording sink (Pushed path) and
    '''     a refusing sink (Dropped path) — even though in skeleton mode
    '''     no frames are pushed, the lifecycle contract must still hold.
    ''' </summary>
    Friend NotInheritable Class DdagrabReplaceabilityTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("DDAGRAB REPLACEABILITY: Skeleton emits no frames to sink", AddressOf Test_SkeletonEmitsNoFramesToSink)
            runner("DDAGRAB REPLACEABILITY: Backend survives refusing sink (DropNewest-like)", AddressOf Test_SurvivesRefusingSink)
            runner("DDAGRAB REPLACEABILITY: Backend survives evicting sink (DropOldest-like)", AddressOf Test_SurvivesEvictingSink)
            runner("DDAGRAB REPLACEABILITY: Same backend across Start/Stop cycles", AddressOf Test_SameBackendAcrossCycles)
            runner("DDAGRAB REPLACEABILITY: Diagnostics surface contract", AddressOf Test_DiagnosticsSurfaceContract)
        End Sub

        ' ---- helpers ----

        Private Shared Function CreateDefaultBackend() As DdagrabBackend
            Return New DdagrabBackend(
                New EngineLogger("DdagrabBackend", EngineLogger.LogLevel.Warning))
        End Function

        Private Shared Function CreateDdagrabContext() As IVideoBackendContext
            Return New FakeVideoBackendContext(
                VideoBackendKind.Ddagrab,
                New EngineLogger("FakeBackendCtx", EngineLogger.LogLevel.Warning))
        End Function

        ' ---- tests ----

        Private Shared Sub Test_SkeletonEmitsNoFramesToSink()
            Dim backend = CreateDefaultBackend()
            backend.WithInterAttemptDelayMs(1)
            backend.Initialize(CreateDdagrabContext())
            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Let the worker spin long enough to emit several NoFrame iterations.
            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() backend.Diagnostics.NoFrameCount >= 10, 1000),
                "Expected NoFrameCount >= 10. Was: " & backend.Diagnostics.NoFrameCount)

            backend.Stop()
            backend.Dispose()

            ' Skeleton: no FrameAvailable results reach the sink (NoFrame is internal).
            TestHelpers.AssertEqual(0, sink.RecordedCount, "sink must receive ZERO results (skeleton emits NoFrame only)")
            TestHelpers.Assert(backend.Diagnostics.NoFrameCount >= 10, "NoFrameCount >= 10")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.EmittedFrames, "EmittedFrames = 0")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.DroppedFrames, "DroppedFrames = 0")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.ReplacedFrames, "ReplacedFrames = 0")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.ErrorCount, "ErrorCount = 0")
        End Sub

        Private Shared Sub Test_SurvivesRefusingSink()
            ' Even with a sink that always returns Dropped (DropNewest-like),
            ' the DdagrabBackend skeleton survives — because it never pushes
            ' anything (NoFrame is internal). The test verifies the lifecycle
            ' contract: Start → spin briefly → Stop completes within budget.
            Dim backend = CreateDefaultBackend()
            backend.WithInterAttemptDelayMs(1)
            backend.Initialize(CreateDdagrabContext())
            Dim sink As New RefusingVideoFrameSink()
            backend.Start(sink)

            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() backend.Diagnostics.NoFrameCount >= 5, 1000),
                "Expected NoFrameCount >= 5 even under refusing sink. Was: " & backend.Diagnostics.NoFrameCount)

            backend.Stop()
            backend.Dispose()

            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Disposed,
                backend.CurrentState, "backend Disposed cleanly after refusing sink")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.EmittedFrames, "EmittedFrames = 0 (skeleton)")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.DroppedFrames, "DroppedFrames = 0 (skeleton never pushes)")
        End Sub

        Private Shared Sub Test_SurvivesEvictingSink()
            ' Same as above but with an evicting sink (DropOldest-like).
            Dim backend = CreateDefaultBackend()
            backend.WithInterAttemptDelayMs(1)
            backend.Initialize(CreateDdagrabContext())
            Dim sink As New EvictingVideoFrameSink()
            backend.Start(sink)

            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() backend.Diagnostics.NoFrameCount >= 5, 1000),
                "Expected NoFrameCount >= 5 even under evicting sink. Was: " & backend.Diagnostics.NoFrameCount)

            backend.Stop()
            backend.Dispose()

            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Disposed,
                backend.CurrentState, "backend Disposed cleanly after evicting sink")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.EmittedFrames, "EmittedFrames = 0 (skeleton)")
            TestHelpers.AssertEqual(0L, backend.Diagnostics.ReplacedFrames, "ReplacedFrames = 0 (skeleton never pushes)")
        End Sub

        Private Shared Sub Test_SameBackendAcrossCycles()
            ' Run 3 Start → Stop cycles on the same backend. Each cycle the
            ' skeleton worker should spin and emit NoFrame; counters accumulate
            ' across cycles (NoFrameCount is monotonic across Start/Stop).
            Dim backend = CreateDefaultBackend()
            backend.WithInterAttemptDelayMs(0)
            backend.Initialize(CreateDdagrabContext())

            For cycle As Integer = 1 To 3
                Dim sink As New RecordingVideoFrameSink()
                backend.Start(sink)
                ' Skeleton worker with 0 ms inter-attempt delay spins fast.
                ' Wait for at least 3 NoFrame results per cycle (conservative —
                ' the worker may need a moment to ramp up).
                TestHelpers.Assert(
                    TestHelpers.SpinWaitFor(Function() backend.Diagnostics.NoFrameCount >= cycle * 3, 1000),
                    "Cycle " & cycle & ": expected NoFrameCount >= " & (cycle * 3).ToString() &
                    ". Was: " & backend.Diagnostics.NoFrameCount)
                backend.Stop()

                ' Skeleton: no results reach the sink.
                TestHelpers.AssertEqual(
                    0, sink.RecordedCount,
                    "cycle " & cycle & ": sink must receive ZERO results (skeleton)")
            Next

            Dim totalNoFrame = backend.Diagnostics.NoFrameCount
            TestHelpers.Assert(
                totalNoFrame >= 9,
                "Total NoFrameCount >= 9 across 3 cycles (3 per cycle). Was: " & totalNoFrame)

            backend.Dispose()
            TestHelpers.AssertEqual(
                DdagrabBackend.DdagrabBackendState.Disposed,
                backend.CurrentState, "backend disposed cleanly after 3 cycles")
        End Sub

        Private Shared Sub Test_DiagnosticsSurfaceContract()
            ' The IVideoBackendDiagnostics surface MUST be readable from any
            ' thread. This test reads all 5 counters from a different thread
            ' than the worker, while the worker is running.
            Dim backend = CreateDefaultBackend()
            backend.WithInterAttemptDelayMs(1)
            backend.Initialize(CreateDdagrabContext())
            backend.Start(New RecordingVideoFrameSink())

            ' Give the worker a moment.
            Thread.Sleep(20)

            ' Read counters from a different thread.
            Dim capturedEmitted As Long = 0
            Dim capturedDropped As Long = 0
            Dim capturedReplaced As Long = 0
            Dim capturedNoFrame As Long = 0
            Dim capturedError As Long = 0
            Dim reader As New Thread(
                Sub()
                    Dim d = backend.Diagnostics
                    capturedEmitted = d.EmittedFrames
                    capturedDropped = d.DroppedFrames
                    capturedReplaced = d.ReplacedFrames
                    capturedNoFrame = d.NoFrameCount
                    capturedError = d.ErrorCount
                End Sub)
            reader.IsBackground = True
            reader.Start()
            reader.Join(TimeSpan.FromSeconds(2))

            backend.Stop()
            backend.Dispose()

            TestHelpers.Assert(capturedNoFrame > 0, "NoFrameCount > 0 read from other thread. Was: " & capturedNoFrame)
            TestHelpers.AssertEqual(0L, capturedEmitted, "EmittedFrames = 0 (skeleton) read from other thread")
            TestHelpers.AssertEqual(0L, capturedDropped, "DroppedFrames = 0 read from other thread")
            TestHelpers.AssertEqual(0L, capturedReplaced, "ReplacedFrames = 0 read from other thread")
            TestHelpers.AssertEqual(0L, capturedError, "ErrorCount = 0 read from other thread")
        End Sub
    End Class
End Namespace
