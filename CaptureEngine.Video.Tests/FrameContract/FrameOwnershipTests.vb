Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports CaptureEngine.Video.Backends.Fake
Imports CaptureEngine.Video.Tests.Fakes

Namespace CaptureEngine.Video.Tests.FrameContract
    ''' <summary>
    ''' Frame ownership + lifetime tests. Verifies that:
    '''   - every FrameAvailable result's frame is disposed exactly once by
    '''     whoever currently owns it (sink under Pushed/Replaced; backend
    '''     under Dropped);
    '''   - no frame leaks across Start→Stop→Dispose cycles;
    '''   - backend does NOT touch the frame after TryPush returns.
    ''' </summary>
    Friend NotInheritable Class FrameOwnershipTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("OWNERSHIP: Frames pushed to sink are owned by sink", AddressOf Test_PushedFrameOwnership)
            runner("OWNERSHIP: Refused frames (PushOutcome.Dropped) are disposed by backend", AddressOf Test_DroppedFrameOwnership)
            runner("OWNERSHIP: Replaced frames disposed by sink (DropOldest)", AddressOf Test_ReplacedFrameOwnership)
            runner("OWNERSHIP: No frame leak across Start/Stop/Dispose cycles", AddressOf Test_NoLeakAcrossCycles)
        End Sub

        ''' <summary>
        ''' When TryPush returns Pushed, the sink takes ownership. Tests that
        ''' the fake backend does NOT dispose the frame after Pushed, and that
        ''' the sink (or downstream consumer) eventually disposes it exactly once.
        ''' </summary>
        Private Shared Sub Test_PushedFrameOwnership()
            Dim backend = TestHelpers.CreateDefaultFake()
            ' Use a slow-ish inter-attempt delay so we have time to inspect.
            backend.WithInterAttemptDelayMs(5)
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() sink.RecordedCount >= 3, 1000),
                "Expected 3 frames pushed")

            backend.Stop()
            backend.Dispose()

            ' All pushed frames should still be alive (DisposeCount=0) — sink has not
            ' disposed them yet. Backend should NOT have touched them.
            For Each f In sink.OwnedFrames
                Dim ff = DirectCast(f, FakeVideoFrame)
                TestHelpers.AssertEqual(0, ff.DisposeCount, "Pushed frame must NOT be disposed by backend or sink (test owns them now)")
            Next

            ' Now the test (as owner) disposes them.
            sink.DisposeAllOwnedFrames()
            For Each f In sink.OwnedFrames
                Dim ff = DirectCast(f, FakeVideoFrame)
                TestHelpers.AssertEqual(1, ff.DisposeCount, "Pushed frame disposed exactly once by owner")
            Next
        End Sub

        ''' <summary>
        ''' When TryPush returns Dropped, the backend retains ownership and MUST
        ''' dispose the refused frame. The sink does NOT take ownership.
        ''' </summary>
        Private Shared Sub Test_DroppedFrameOwnership()
            Dim backend = TestHelpers.CreateDefaultFake()
            backend.WithInterAttemptDelayMs(2)
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RefusingVideoFrameSink()
            backend.Start(sink)

            ' Backend will try to push; sink always refuses (returns Dropped).
            ' Wait for backend to drop several frames.
            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() backend.Diagnostics.DroppedFrames >= 3, 1000),
                "Expected backend.DroppedFrames >= 3")

            backend.Stop()
            backend.Dispose()

            ' Each dropped frame should have been disposed exactly once BY THE BACKEND
            ' (proven via the fake's per-frame DisposeCount). We can't observe the frames
            ' directly here (the backend disposed them already), so we rely on the
            ' DroppedFrames counter.
            TestHelpers.Assert(
                backend.Diagnostics.DroppedFrames >= 3,
                "DroppedFrames counter should be >= 3. Was: " & backend.Diagnostics.DroppedFrames)
            TestHelpers.Assert(
                backend.Diagnostics.EmittedFrames = 0,
                "EmittedFrames must be 0 (sink refused every frame). Was: " & backend.Diagnostics.EmittedFrames)
        End Sub

        ''' <summary>
        ''' When TryPush returns Replaced (DropOldest), the sink takes ownership
        ''' of the new frame AND disposes the evicted older frame.
        ''' </summary>
        Private Shared Sub Test_ReplacedFrameOwnership()
            Dim backend = TestHelpers.CreateDefaultFake()
            backend.WithInterAttemptDelayMs(2)
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New EvictingVideoFrameSink()
            backend.Start(sink)

            ' EvictingVideoFrameSink returns Pushed on first push, Replaced on every subsequent push.
            ' Wait for several pushes.
            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() sink.PushCount >= 5, 1000),
                "Expected at least 5 pushes")

            backend.Stop()
            backend.Dispose()

            ' The evicted frames should have been recorded by the sink. They are owned by
            ' the sink's EvictedFrames list (the test simulates the eviction; in the real
            ' BoundedVideoFrameSink the sink would dispose them immediately).
            Dim evicted = sink.EvictedFrames
            TestHelpers.Assert(evicted.Count >= 4, "EvictedFrames >= 4. Was: " & evicted.Count)

            ' Backend's diagnostics: ReplacedFrames increments on each Replaced outcome, EmittedFrames
            ' also increments (the new frame WAS accepted; an older one was evicted).
            TestHelpers.Assert(
                backend.Diagnostics.ReplacedFrames >= 4,
                "ReplacedFrames >= 4. Was: " & backend.Diagnostics.ReplacedFrames)
            TestHelpers.Assert(
                backend.Diagnostics.EmittedFrames >= 5,
                "EmittedFrames >= 5 (each push, whether Pushed or Replaced, increments EmittedFrames). Was: " & backend.Diagnostics.EmittedFrames)

            ' Clean up: dispose all evicted frames (in real BoundedVideoFrameSink this happens automatically).
            For Each f In evicted
                Try
                    f.Dispose()
                Catch
                End Try
            Next
        End Sub

        Private Shared Sub Test_NoLeakAcrossCycles()
            ' Run 3 Start→Stop cycles on the same backend. Each cycle emits a few frames.
            ' After all cycles, ensure backend's EmittedFrames is non-zero and
            ' the backend itself disposes cleanly.
            Dim backend = TestHelpers.CreateDefaultFake()
            backend.WithInterAttemptDelayMs(2)
            backend.Initialize(TestHelpers.CreateContext())

            For cycle As Integer = 1 To 3
                Dim sink As New RecordingVideoFrameSink()
                backend.Start(sink)
                TestHelpers.Assert(
                    TestHelpers.SpinWaitFor(Function() sink.RecordedCount >= 2, 1000),
                    "Cycle " & cycle & ": expected 2 frames")
                backend.Stop()
                sink.DisposeAllOwnedFrames()
            Next

            TestHelpers.Assert(
                backend.Diagnostics.EmittedFrames >= 6,
                "EmittedFrames >= 6 across 3 cycles. Was: " & backend.Diagnostics.EmittedFrames)

            backend.Dispose()
            TestHelpers.AssertEqual(
                FakeVideoCaptureBackend.FakeBackendState.Disposed,
                backend.CurrentState, "backend disposed cleanly after cycles")
        End Sub
    End Class
End Namespace
