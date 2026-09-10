Option Strict On
Option Explicit On
Option Infer On

' PlaybackClockTests.vb — clock math with a FAKE QPC source (pure, §3.6).
'
' Same discipline as AudioPositionTracker tests ("pure, Linux-testable stamp
' math"): inject frequency + now-func, assert exact tick math. No sleeps.

Imports System
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class PlaybackClockTests

        ''' <summary>1 tick of the fake QPC = 1 ms (frequency 1000).</summary>
        Private _fakeNow As Long = 0
        Private ReadOnly _fakeFreq As Long = 1000 ' ticks-per-second; 1 tick = 1ms

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            Dim t As New PlaybackClockTests()
            runner("CLK: presentation advances with fake QPC (1 tick = 1 ms)",
                   AddressOf t.Test_Advance)
            runner("CLK: Freeze stops the clock; Unfreeze re-anchors at frozen time",
                   AddressOf t.Test_FreezeUnfreeze)
            runner("CLK: AnchorAt re-bases (seek contract)",
                   AddressOf t.Test_ReAnchor)
            runner("CLK: 100-ns tick conversions",
                   AddressOf t.Test_Conversions)
        End Sub

        Private Function MakeClock() As PlaybackClock
            Return New PlaybackClock(_fakeFreq, Function() _fakeNow)
        End Function

        Private Sub Test_Advance()
            _fakeNow = 0
            Using clock = MakeClock()
                clock.AnchorAt(0L)
                TestRunner.AssertEqual(0L, clock.PresentationTicks, "t=0")

                _fakeNow = 1000 ' 1000 fake ticks = 1 s
                TestRunner.AssertEqual(10000000L, clock.PresentationTicks, "t=1s in 100-ns ticks")

                _fakeNow = 1500 ' +0.5 s
                TestRunner.AssertEqual(15000000L, clock.PresentationTicks, "t=1.5s")
            End Using
        End Sub

        Private Sub Test_FreezeUnfreeze()
            _fakeNow = 0
            Using clock = MakeClock()
                clock.AnchorAt(0L)
                _fakeNow = 2000 ' 2 s
                clock.Freeze()
                TestRunner.Assert(clock.IsFrozen, "frozen")
                TestRunner.AssertEqual(20000000L, clock.PresentationTicks, "frozen at 2s")

                _fakeNow = 9999 ' wall clock moves but frozen clock must not
                TestRunner.AssertEqual(20000000L, clock.PresentationTicks, "still 2s while frozen")

                clock.Unfreeze()
                TestRunner.Assert(Not clock.IsFrozen, "unfrozen")
                TestRunner.AssertEqual(20000000L, clock.PresentationTicks, "resumes from frozen point")

                _fakeNow = 10099 ' +100 fake ticks after resume = +100 ms (freq 1000)
                TestRunner.AssertEqual(21000000L, clock.PresentationTicks, "advances after resume (2s + 100ms)")
            End Using
        End Sub

        Private Sub Test_ReAnchor()
            _fakeNow = 0
            Using clock = MakeClock()
                ' Seek contract: presentation time 30s happens NOW.
                clock.AnchorAt(300000000L) ' 30 s in 100-ns ticks
                TestRunner.AssertEqual(300000000L, clock.PresentationTicks, "anchor at 30s")

                _fakeNow = 250 ' +250 ms
                TestRunner.AssertEqual(302500000L, clock.PresentationTicks, "30.25s")
            End Using
        End Sub

        Private Sub Test_Conversions()
            TestRunner.AssertEqual(10000000L, PlaybackClock.SecondsToTicks(1.0), "1s")
            TestRunner.AssertEqual(166667L, PlaybackClock.SecondsToTicks(1.0 / 60.0), "1/60s frame")
            TestRunner.AssertEqual(1.0, PlaybackClock.TicksToSeconds(10000000L), "back to seconds")

            ' 60 fps → window = 1.5 frames = 25 ms
            TestRunner.AssertEqual(250000L, PlaybackClock.FrameWindowTicks(1.0 / 60.0), "60fps window")
        End Sub

    End Class

End Namespace
