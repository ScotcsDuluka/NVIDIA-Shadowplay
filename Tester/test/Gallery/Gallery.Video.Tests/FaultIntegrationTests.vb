Option Strict On
Option Explicit On
Option Infer On

' FaultIntegrationTests.vb — failure matrix with REAL ffmpeg (design doc §7).
'
' Every fault must surface as a GalleryVideoFault event (or probe Nothing),
' never a crash, never a hang. Session-level mapping (FileMissing pre-check
' before spawn, etc.) is asserted in SessionTests.

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class FaultIntegrationTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("FAULT: corrupt/truncated MP4 → decode fault CorruptFile (0 frames)",
                   AddressOf Test_CorruptFile)
            runner("FAULT: not-media bytes → decode fault (never hang)",
                   AddressOf Test_NotMedia)
            runner("FAULT: missing file at worker level → decode fault backstop",
                   AddressOf Test_MissingFileBackstop)
            runner("FAULT: broken ffmpeg path → BackendMissing, no crash",
                   AddressOf Test_BackendMissing)
            runner("FAULT: audio-only file → video worker faults; probe says HasVideo=False",
                   AddressOf Test_AudioOnlyFile)
        End Sub

        ' ---- helpers ----

        Private NotInheritable Class FaultRunResult
            Public Faults As New List(Of GalleryVideoFault)()
            Public FramesDecoded As Long
        End Class

        Private Shared Function RunUntilFaultOrEof(path As String,
                                                   Optional ffmpegExe As String = Nothing) As FaultRunResult

            Using q As New FrameQueue(4)
                Using audio As New AudioPcmBuffer(96000)
                    Dim cfg As New DecodeGenerationConfig With {
                        .FilePath = path,
                        .FfmpegExe = If(ffmpegExe, TestMedia.FfmpegPath),
                        .Generation = 0,
                        .SeekSeconds = 0.0,
                        .VideoEnabled = True,
                        .AudioEnabled = False,
                        .FrameWidth = 320,
                        .FrameHeight = 240,
                        .FrameRate = 60.0,
                        .VideoQueue = q,
                        .AudioBuffer = audio
                    }

                    Dim worker As New FfmpegDecodeWorker(cfg)
                    Dim faults As New List(Of GalleryVideoFault)()
                    Dim faultEvent As New ManualResetEventSlim(False)

                    AddHandler worker.FaultDetected,
                        Sub(w, f)
                            SyncLock faults
                                faults.Add(f)
                            End SyncLock
                            faultEvent.Set()
                        End Sub
                    AddHandler worker.VideoEofReached, Sub(w) faultEvent.Set()

                    worker.Start()
                    TestRunner.Assert(faultEvent.Wait(20000), "fault/EOF within 20s (never hang)")
                    worker.RequestStop()

                    Dim result As New FaultRunResult()
                    SyncLock faults
                        result.Faults = New List(Of GalleryVideoFault)(faults)
                    End SyncLock
                    result.FramesDecoded = worker.FramesDecoded
                    Return result
                End Using
            End Using
        End Function

        ' ---- tests ----

        Private Shared Sub Test_CorruptFile()
            TestMedia.RequireBinaries()
            Dim result = RunUntilFaultOrEof(TestMedia.CorruptFile())
            TestRunner.Assert(result.Faults.Count > 0, "fault event raised")
            TestRunner.Assert(result.Faults(0).Kind = GalleryVideoFaultKind.CorruptFile,
                              $"kind=CorruptFile (got {result.Faults(0).Kind})")
            TestRunner.AssertEqual(0L, result.FramesDecoded, "0 complete frames")
        End Sub

        Private Shared Sub Test_NotMedia()
            TestMedia.RequireBinaries()
            Dim result = RunUntilFaultOrEof(TestMedia.NotMediaFile())
            TestRunner.Assert(result.Faults.Count > 0, "fault event raised")
            TestRunner.AssertEqual(0L, result.FramesDecoded, "0 frames")
        End Sub

        Private Shared Sub Test_MissingFileBackstop()
            TestMedia.RequireBinaries()
            ' Session pre-checks File.Exists (→ FileMissing) BEFORE spawning;
            ' this proves the decode-side BACKSTOP also holds if a file
            ' disappears between check and spawn (owner case: never crash).
            Dim missing = System.IO.Path.Combine(TestMedia.Sandbox, "vanished_" & Guid.NewGuid().ToString("N") & ".mp4")
            Dim result = RunUntilFaultOrEof(missing)
            TestRunner.Assert(result.Faults.Count > 0, "backstop fault raised")
            TestRunner.AssertEqual(0L, result.FramesDecoded, "0 frames")
        End Sub

        Private Shared Sub Test_BackendMissing()
            TestMedia.RequireBinaries()
            Dim result = RunUntilFaultOrEof(TestMedia.Synthetic("synth_av60.mp4", "av60"),
                                            ffmpegExe:="/nonexistent/ffmpeg-definitely-missing")
            TestRunner.Assert(result.Faults.Count > 0, "BackendMissing fault raised")
            TestRunner.Assert(result.Faults(0).Kind = GalleryVideoFaultKind.BackendMissing,
                              $"kind=BackendMissing (got {result.Faults(0).Kind})")
        End Sub

        Private Shared Sub Test_AudioOnlyFile()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_audio_only.mp4", "audio_only")

            ' Probe layer: session reads HasVideo=False → Faulted.NoVideoStream
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe parses")
            TestRunner.Assert(Not probe.HasVideo, "probe: no video stream")

            ' Worker layer (given a video mapping anyway): ffmpeg -map 0:v:0 on
            ' an audio-only file fails fast → decode fault, no hang.
            Dim result = RunUntilFaultOrEof(path)
            TestRunner.Assert(result.Faults.Count > 0, "worker backstop fault raised")
        End Sub

    End Class

End Namespace
