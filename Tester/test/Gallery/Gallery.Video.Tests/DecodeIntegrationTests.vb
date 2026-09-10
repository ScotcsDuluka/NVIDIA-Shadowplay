Option Strict On
Option Explicit On
Option Infer On

' DecodeIntegrationTests.vb — REAL ffmpeg decode (Tier 2, design doc §8).
'
' Proves the decode-generation mechanics end to end with real ffmpeg:
' open → frames flow (BGRA8, correct size, monotonic PTS) → EOF event →
' audio PCM flows → queue leak invariant still holds.
' Runs anywhere ffmpeg exists (Linux dev box included).

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class DecodeIntegrationTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("DEC: open synth av60 → ≥200 frames, monotonic PTS, EOF, no frame lost",
                   AddressOf Test_OpenDecodeEof)
            runner("DEC: audio PCM flows alongside video + RequestStop joins fast",
                   AddressOf Test_AudioFlows)
            runner("DEC: consumer-side disposal — every frame disposed exactly once",
                   AddressOf Test_ConsumerDisposal)
        End Sub

        ' ---- worker harness ----

        Friend Class WorkerHarness
            Implements IDisposable

            Public Queue As New FrameQueue(4)
            Public Audio As New AudioPcmBuffer(48000 * 2 * 2 * 10) ' ~800ms @48k stereo s16
            Public EofLatch As New ManualResetEventSlim(False)
            Public Faults As New List(Of GalleryVideoFault)()
            Public FaultLock As New Object()
            Public Worker As FfmpegDecodeWorker

            Public Sub Dispose() Implements IDisposable.Dispose
                If Worker IsNot Nothing Then Worker.RequestStop()
                Queue.Dispose()
                Audio.Dispose()
                EofLatch.Dispose()
            End Sub
        End Class

        Friend Shared Function StartWorker(path As String, seekSeconds As Double,
                                           generation As Long, probe As MediaInfo) As WorkerHarness
            Dim h As New WorkerHarness()
            Dim v = probe.Video
            Dim fps = MediaInfo.ParseFrameRate(v.AvgFrameRate)
            If fps <= 0 Then fps = 30.0

            Dim cfg As New DecodeGenerationConfig With {
                .FilePath = path,
                .FfmpegExe = TestMedia.FfmpegPath,
                .Generation = generation,
                .SeekSeconds = seekSeconds,
                .VideoEnabled = probe.HasVideo,
                .AudioEnabled = probe.HasAudio,
                .FrameWidth = v.Width,
                .FrameHeight = v.Height,
                .FrameRate = fps,
                .AudioSampleRate = If(probe.HasAudio, probe.Audio.SampleRate, 48000),
                .AudioChannels = If(probe.HasAudio, probe.Audio.Channels, 2),
                .VideoQueue = h.Queue,
                .AudioBuffer = h.Audio
            }

            h.Worker = New FfmpegDecodeWorker(cfg)
            AddHandler h.Worker.VideoEofReached, Sub(w) h.EofLatch.Set()
            AddHandler h.Worker.FaultDetected,
                Sub(w, f)
                    SyncLock h.FaultLock
                        h.Faults.Add(f)
                    End SyncLock
                    h.EofLatch.Set() ' fault unblocks waiters too
                End Sub
            h.Worker.Start()
            Return h
        End Function

        ' ---- tests ----

        Private Shared Sub Test_OpenDecodeEof()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")

            Using h = StartWorker(path, 0.0, 0, probe)
                Dim consumedPts As New List(Of Long)()
                Dim consumed As Long = 0

                ' Consume until EOF + queue empty (bounded wait: 60s)
                Dim deadline = DateTime.UtcNow.AddSeconds(60)
                While DateTime.UtcNow < deadline
                    Dim f As PlaybackFrame = Nothing
                    If h.Queue.TryDequeue(100, f) Then
                        consumedPts.Add(f.PtsTicks)
                        f.Dispose()
                        consumed += 1L
                        Continue While
                    End If
                    If h.EofLatch.IsSet AndAlso h.Queue.Count = 0 Then Exit While
                End While

                TestRunner.Assert(h.Worker.VideoEof, "video EOF reached")
                TestRunner.Assert(consumedPts.Count >= 200,
                                  $"decoded ≥200 frames of a 4s@60 file (got {consumedPts.Count})")

                ' PTS monotonic + starts ≈ 0 + interval ≈ 1/60s
                For i = 1 To consumedPts.Count - 1
                    TestRunner.Assert(consumedPts(i) > consumedPts(i - 1),
                                      $"PTS monotonic at frame {i}")
                Next
                TestRunner.Assert(consumedPts(0) <= PlaybackClock.SecondsToTicks(0.2),
                                  $"first pts ≈ 0 (got {PlaybackClock.TicksToSeconds(consumedPts(0))}s)")
                Dim firstInterval = consumedPts(1) - consumedPts(0)
                Dim expected = PlaybackClock.SecondsToTicks(1.0 / 60.0)
                TestRunner.Assert(Math.Abs(firstInterval - expected) <= expected / 4,
                                  $"interval ≈ 1/60s (got {firstInterval} ticks, expected {expected})")

                ' No frame lost: created == consumed (clean EOF run, nothing rejected)
                TestRunner.AssertEqual(h.Worker.FramesDecoded, consumed,
                                       "decoded == consumed (no frame lost or duplicated)")

                ' Queue leak invariant after full drain
                TestRunner.AssertEqual(h.Queue.EnqueuedCount,
                                       h.Queue.DequeuedCount + h.Queue.DroppedStaleCount +
                                       h.Queue.DroppedOnFlushCount + h.Queue.DisposedByCloseCount,
                                       "queue leak invariant")

                ' No fault raised on a healthy file
                SyncLock h.FaultLock
                    TestRunner.AssertEqual(0, h.Faults.Count, "healthy file → no fault")
                End SyncLock
            End Using
        End Sub

        Private Shared Sub Test_AudioFlows()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")

            Using h = StartWorker(path, 0.0, 0, probe)
                ' Wait for a solid chunk of audio (4s file → ~768KB expected)
                Dim deadline = DateTime.UtcNow.AddSeconds(60)
                While DateTime.UtcNow < deadline
                    If h.Worker.AudioBytesDecoded > 48000 * 2 * 2 Then Exit While
                    Thread.Sleep(50)
                End While

                TestRunner.Assert(h.Worker.AudioBytesDecoded > 48000 * 2 * 2,
                                  $"audio PCM decoded (got {h.Worker.AudioBytesDecoded}B)")

                ' Read some PCM back through the bounded ring (device-side view)
                Dim dest(1920 * 2 - 1) As Byte
                Dim got = h.Audio.Read(dest, 0, dest.Length, 0, 500)
                TestRunner.Assert(got > 0, "PCM readable from buffer")

                ' Stop mid-stream: RequestStop must join both decoders quickly
                Dim sw = System.Diagnostics.Stopwatch.StartNew()
                h.Worker.RequestStop()
                TestRunner.Assert(sw.ElapsedMilliseconds < 5000,
                                  $"RequestStop joined < 5s (took {sw.ElapsedMilliseconds}ms)")
            End Using
        End Sub

        Private Shared Sub Test_ConsumerDisposal()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_video_only.mp4", "video_only")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")

            Using h = StartWorker(path, 0.0, 0, probe)
                Dim consumed As Long = 0
                Dim disposeCalls As Long = 0
                Dim deadline = DateTime.UtcNow.AddSeconds(30)
                While DateTime.UtcNow < deadline
                    Dim f As PlaybackFrame = Nothing
                    If h.Queue.TryDequeue(100, f) Then
                        ' OnDisposed is the one-shot metric hook (repo pattern):
                        ' set it before Dispose; it must fire EXACTLY once.
                        f.OnDisposed = Sub() disposeCalls += 1L
                        f.Dispose()
                        consumed += 1L
                        Continue While
                    End If
                    If h.EofLatch.IsSet AndAlso h.Queue.Count = 0 Then Exit While
                End While

                ' 2s @30fps = 60 frames
                TestRunner.Assert(consumed >= 50, $"video_only decoded (got {consumed})")
                ' Every frame disposed exactly once — the one-dispose invariant
                TestRunner.AssertEqual(consumed, disposeCalls, "one dispose per frame")
            End Using
        End Sub

    End Class

End Namespace
