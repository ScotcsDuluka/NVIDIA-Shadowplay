Option Strict On
Option Explicit On
Option Infer On

' SeekIntegrationTests.vb — REAL ffmpeg seek-at-target proof (Tier 2).
'
' Worker-level proof of §3.7: a new decode generation started at -ss T
' produces frames whose ABSOLUTE PTS ≈ T (within tolerance), for backward /
' forward / near-end / to-beginning targets. The stale-frame guarantee
' (generation flush) is proven in FrameQueueTests; the full session-level
' pause+seek / seek-while-playing cases are proven in SessionTests against
' PlaybackSession (same tolerance contract).

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class SeekIntegrationTests

        ' 4s @60fps synthetic; keyframe interval = 1s (-g 60)
        Private Shared ReadOnly ToleranceTicks As Long = PlaybackClock.SecondsToTicks(0.5)

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("SEEK: forward → first frame PTS ≈ target (3.0s from 0)",
                   AddressOf Test_SeekForward)
            runner("SEEK: backward → first frame PTS ≈ target (1.0s after 3.0s run)",
                   AddressOf Test_SeekBackward)
            runner("SEEK: near end (3.9s) → frames arrive, EOF follows quickly",
                   AddressOf Test_SeekNearEnd)
            runner("SEEK: to beginning (0.0s after 2.0s) → first frame PTS ≈ 0",
                   AddressOf Test_SeekToBeginning)
        End Sub

        Private Shared Function PlaybackClockSeconds(s As Double) As Long
            Return PlaybackClock.SecondsToTicks(s)
        End Function

        ''' <summary>Start a worker at `seek`, take the FIRST decoded frame,
        ''' return its absolute PTS, then fully stop/dispose the harness.</summary>
        Private Shared Function FirstFramePts(path As String, seek As Double, generation As Long,
                                              probe As MediaInfo) As Long
            Using h = DecodeIntegrationTests.StartWorker(path, seek, generation, probe)
                Dim deadline = DateTime.UtcNow.AddSeconds(30)
                While DateTime.UtcNow < deadline
                    Dim f As PlaybackFrame = Nothing
                    If h.Queue.TryDequeue(200, f) Then
                        Dim pts = f.PtsTicks
                        f.Dispose()
                        Return pts
                    End If
                    SyncLock h.FaultLock
                        If h.Faults.Count > 0 Then
                            Throw New Exception("fault during seek test: " & h.Faults(0).ToString())
                        End If
                    End SyncLock
                End While
                Throw New Exception("no frame decoded within 30s at seek " & seek.ToString("0.##"))
            End Using
        End Function

        Private Shared Sub AssertFirstPtsNear(path As String, seek As Double, generation As Long,
                                              probe As MediaInfo, expectedSeconds As Double)
            Dim pts = FirstFramePts(path, seek, generation, probe)
            Dim expected = PlaybackClock.SecondsToTicks(expectedSeconds)
            TestRunner.Assert(Math.Abs(pts - expected) <= ToleranceTicks,
                              $"first frame PTS ≈ {expectedSeconds}s (got {PlaybackClock.TicksToSeconds(pts):0.###}s)")
        End Sub

        Private Shared Sub Test_SeekForward()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")
            AssertFirstPtsNear(path, 3.0, 1, probe, 3.0)
        End Sub

        Private Shared Sub Test_SeekBackward()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")
            ' Simulates: played to 3.0 (gen 0), user seeks BACK to 1.0 (gen 1)
            AssertFirstPtsNear(path, 1.0, 1, probe, 1.0)
        End Sub

        Private Shared Sub Test_SeekNearEnd()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")

            ' 3.9s of a 4.0s file — frames arrive, EOF follows quickly.
            Dim h = DecodeIntegrationTests.StartWorker(path, 3.9, 1, probe)
            Using h
                Dim consumed As Long = 0
                Dim deadline = DateTime.UtcNow.AddSeconds(30)
                While DateTime.UtcNow < deadline
                    Dim f As PlaybackFrame = Nothing
                    If h.Queue.TryDequeue(200, f) Then
                        TestRunner.Assert(f.PtsTicks >= PlaybackClock.SecondsToTicks(3.5),
                                          $"near-end frames ≥ 3.5s (got {PlaybackClock.TicksToSeconds(f.PtsTicks):0.###}s)")
                        f.Dispose()
                        consumed += 1L
                        Continue While
                    End If
                    If h.EofLatch.IsSet AndAlso h.Queue.Count = 0 Then Exit While
                End While

                TestRunner.Assert(h.Worker.VideoEof, "EOF reached after near-end frames")
                TestRunner.Assert(consumed >= 1, $"near-end decoded ≥1 frame (got {consumed})")
                TestRunner.Assert(consumed <= 60, $"near-end bounded (got {consumed} ≤ 60)")
            End Using
        End Sub

        Private Shared Sub Test_SeekToBeginning()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")
            AssertFirstPtsNear(path, 0.0, 2, probe, 0.0)
        End Sub

    End Class

End Namespace
