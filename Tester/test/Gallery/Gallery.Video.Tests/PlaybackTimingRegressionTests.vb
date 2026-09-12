Option Strict On
Option Explicit On
Option Infer On

' PlaybackTimingRegressionTests.vb — W1: playback timing regression for the
' Open / Play / Pause / Seek / Resume surface, PROVEN GPU-INDEPENDENT.
'
' GPU-independence argument, enforced by construction here:
'   - Every session runs headless (RenderWindow = Zero → NullVideoSink): no
'     D3D11 device, no swapchain, no GPU vendor code on the path at all.
'   - Decode is the ffmpeg SUBPROCESS (software h264 — Gallery.Video contains
'     no hwaccel/cuda/qsv wiring; verified by source audit).
'   - The clock math itself is proven with an INJECTED fake QPC source
'     (PBT-5): pure functions of (frequency, now) — no real timer, no GPU.
'   Therefore the Open/Play/Pause/Seek/Resume timing + rebase semantics are
'   properties of the session logic, not of any GPU.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class PlaybackTimingRegressionTests

        Private Shared ReadOnly Tol As Long = PlaybackClock.SecondsToTicks(0.5)

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("PBT: open→playing latency bounded; position advances while playing",
                   AddressOf Test_OpenPlayTiming)
            runner("PBT: pause freezes position exactly; resume continues forward",
                   AddressOf Test_PauseFreezeExact)
            runner("PBT: forward seek completes within SeekTimeout; first present ≈ target",
                   AddressOf Test_ForwardSeekTiming)
            runner("PBT: backward seek completes within SeekTimeout; first present ≈ target",
                   AddressOf Test_BackwardSeekTiming)
            runner("PBT: clock rebase math is pure (injected fake QPC — no GPU, no wall clock)",
                   AddressOf Test_ClockRebasePureMath)
            runner("PBT: full journey runs GPU-free (headless) with zero faults",
                   AddressOf Test_FullJourneyHeadless)
        End Sub

        ' ---- tests ----

        Private Shared Sub Test_OpenPlayTiming()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                Dim sw = Stopwatch.StartNew()
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing after Open")
                TestRunner.Assert(sw.ElapsedMilliseconds < 15000,
                                  $"open→playing {sw.ElapsedMilliseconds}ms < 15s")

                Dim advance0 = s.PositionTicks
                Thread.Sleep(600)
                Dim advance1 = s.PositionTicks
                TestRunner.Assert(advance1 > advance0, "presentation clock advances while playing")

                ' Clock keeps tracking real time across three windows.
                For i = 1 To 3
                    Dim d = advance1
                    Thread.Sleep(400)
                    advance1 = s.PositionTicks
                    TestRunner.Assert(advance1 > d, $"clock liveness window {i}")
                Next
            End Using
        End Sub

        Private Shared Sub Test_PauseFreezeExact()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                Thread.Sleep(800)

                TestRunner.Assert(s.Pause().Accepted, "Pause")
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 5000), "Paused")

                Dim frozen = s.PositionTicks
                ' Grace: the paused render loop may present frames already in
                ' the bounded queue (design: last DECODED frame stays on
                ' screen) — allow ≤ one 1.5-frame window of settle, then the
                ' position must be EXACTLY stable.
                Dim settle = PlaybackClock.FrameWindowTicks(1.0 / 60.0)
                Thread.Sleep(300)
                Dim settled = s.PositionTicks
                TestRunner.Assert(Math.Abs(settled - frozen) <= settle,
                                  $"pause settle within one frame window ({frozen} → {settled})")
                For i = 1 To 5
                    Thread.Sleep(80)
                    TestRunner.Assert(s.PositionTicks = settled,
                                      $"position frozen exactly after settle (sample {i}: {s.PositionTicks} vs {settled})")
                Next

                TestRunner.Assert(s.Play().Accepted, "Resume")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 10000), "Playing")
                Thread.Sleep(500)
                ' Grace ≤ one frame window: the paused loop may have presented
                ' one queued frame ahead of the wall-true pause anchor.
                TestRunner.Assert(s.PositionTicks >= frozen - PlaybackClock.FrameWindowTicks(1.0 / 60.0),
                                  "resume continues from the frozen position (grace ≤1 frame)")
            End Using
        End Sub

        Private Shared Sub Test_ForwardSeekTiming()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                Thread.Sleep(500)

                Dim sw = Stopwatch.StartNew()
                TestRunner.Assert(s.Seek(3.0).Accepted, "Seek 3.0s")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000),
                                  "seek completed within timeout (back to Playing)")
                TestRunner.Assert(sw.ElapsedMilliseconds < 15000,
                                  $"seek round-trip {sw.ElapsedMilliseconds}ms")

                Dim deadline = DateTime.UtcNow.AddSeconds(5)
                While DateTime.UtcNow < deadline AndAlso s.LastPresentedPtsTicks < PlaybackClock.SecondsToTicks(2.5)
                    Thread.Sleep(20)
                End While
                Dim pts = s.LastPresentedPtsTicks
                TestRunner.Assert(Math.Abs(pts - PlaybackClock.SecondsToTicks(3.0)) <= Tol,
                                  $"first present ≈ 3.0s (got {PlaybackClock.TicksToSeconds(pts):0.###}s)")
            End Using
        End Sub

        Private Shared Sub Test_BackwardSeekTiming()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                Thread.Sleep(1500)

                TestRunner.Assert(s.Seek(0.5).Accepted, "Seek back to 0.5s")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000), "back to Playing")

                Dim deadline = DateTime.UtcNow.AddSeconds(5)
                While DateTime.UtcNow < deadline AndAlso s.LastPresentedPtsTicks > PlaybackClock.SecondsToTicks(1.0)
                    Thread.Sleep(20)
                End While
                Dim pts = s.LastPresentedPtsTicks
                TestRunner.Assert(Math.Abs(pts - PlaybackClock.SecondsToTicks(0.5)) <= Tol,
                                  $"first present ≈ 0.5s (got {PlaybackClock.TicksToSeconds(pts):0.###}s)")
            End Using
        End Sub

        ''' <summary>Deterministic clock math with an injected QPC source:
        ''' anchor/rebase, freeze, unfreeze — exact to the tick.</summary>
        Private Shared Sub Test_ClockRebasePureMath()
            Dim nowQpc As Long = 1000000L
            Dim freq As Long = 10000000L                 ' 1 tick = 100 ns
            Dim nowFunc As Func(Of Long) = Function() nowQpc ' closure over the mutable

            Using clk As New PlaybackClock(freq, nowFunc)
                clk.AnchorAt(20000000L)                  ' media 2.0s happens NOW
                nowQpc += 2500000L                       ' +0.25s of QPC time
                TestRunner.Assert(clk.PresentationTicks = 22500000L,
                                  $"anchor + ΔQPC = media ticks (got {clk.PresentationTicks})")

                clk.Freeze()
                nowQpc += 9000000L
                TestRunner.Assert(clk.PresentationTicks = 22500000L, "freeze holds the value exactly")
                TestRunner.Assert(clk.IsFrozen, "IsFrozen")

                clk.Unfreeze()
                TestRunner.Assert(clk.PresentationTicks = 22500000L, "unfreeze re-anchors at the frozen value")
                nowQpc += 5000000L
                TestRunner.Assert(clk.PresentationTicks = 27500000L, "post-unfreeze advance exact")

                ' Mid-stream re-anchor (the Seek semantics): 5.0s happens NOW.
                clk.AnchorAt(50000000L)
                nowQpc += 1000000L
                TestRunner.Assert(clk.PresentationTicks = 51000000L, "AnchorAt rebases the domain")

                ' Frame window: 1.5 × (1/60) = 250000 ticks.
                TestRunner.Assert(PlaybackClock.FrameWindowTicks(1.0 / 60.0) = 250000L,
                                  $"FrameWindowTicks(1/60) = {PlaybackClock.FrameWindowTicks(1.0 / 60.0)}")
            End Using
        End Sub

        Private Shared Sub Test_FullJourneyHeadless()
            TestMedia.RequireBinaries()
            Dim faults As New List(Of GalleryVideoFault)()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                AddHandler s.FaultRaised,
                Sub(sender, f)
                    SyncLock faults
                        faults.Add(f)
                    End SyncLock
                End Sub

                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Open→Playing")
                TestRunner.Assert(s.Pause().Accepted AndAlso s.WaitForState(PlaybackState.Paused, 5000), "Pause")
                TestRunner.Assert(s.Seek(2.0).Accepted AndAlso s.WaitForState(PlaybackState.Paused, 15000), "Seek while paused")
                TestRunner.Assert(s.Play().Accepted AndAlso s.WaitForState(PlaybackState.Playing, 10000), "Resume")
                TestRunner.Assert(s.Seek(1.0).Accepted AndAlso s.WaitForState(PlaybackState.Playing, 15000), "Seek while playing")
                TestRunner.Assert(s.FramesPresented > 0, "frames presented headless (no GPU anywhere)")

                TestRunner.Assert(faults.Count = 0,
                                  $"zero faults on the GPU-free journey ({String.Join("; ", faults)})")
            End Using
        End Sub

    End Class

End Namespace
