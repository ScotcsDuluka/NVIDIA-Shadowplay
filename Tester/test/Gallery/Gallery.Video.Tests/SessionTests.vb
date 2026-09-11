Option Strict On
Option Explicit On
Option Infer On

' SessionTests.vb — PlaybackSession-level proofs (design doc §3.3–§3.7, §7).
'
' The worker-level mechanics are proven in Decode/Seek/FaultIntegrationTests;
' THIS file proves the SESSION contract on top of them:
'   - full lifecycle: Open → Playing → Pause (decoder parked) → Play (resume
'     at paused position) → EOF → Paused+EosReached (not a fault)
'   - session seek cases (owner-mandated): seek-while-playing, pause+seek,
'     seek to beginning — first presented frame PTS ≈ target
'   - guard rules at session level: double open, play/pause/seek guards,
'     stop idempotence, play-from-stopped restart, dispose-then-everything
'   - fault mapping: FileMissing / NoVideoStream / CorruptFile / BackendMissing
'   - seek-failure recovery (§7): timeout → advisory SeekFailed, session
'     recoverable at old position — NEVER a terminal fault for a bad seek
'   - stress: 50 session open/play/stop/dispose iterations (A↔B), no leaks
'   - honest environment note: audio render degrades to video-only off-Windows
'     (counted, not hidden — §7 spirit)

Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class SessionTests

        ' PTS tolerance for session seek targets: keyframe seek + scheduling.
        Private Shared ReadOnly SeekToleranceTicks As Long = PlaybackClock.SecondsToTicks(0.7)

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("SES: open av60 → Playing, frames present, position advances",
                   AddressOf Test_OpenPlays)
            runner("SES: Pause parks decoder → Play resumes from paused position",
                   AddressOf Test_PauseResume)
            runner("SES: EOF (2s video) → Paused + EosReached, NOT a fault",
                   AddressOf Test_EofNotFault)
            runner("SES: seek while playing → first frame ≈ target",
                   AddressOf Test_SeekWhilePlaying)
            runner("SES: pause + seek → Paused at target frame → Play resumes",
                   AddressOf Test_PauseSeek)
            runner("SES: seek to beginning → first frame ≈ 0",
                   AddressOf Test_SeekToBeginning)
            runner("SES: guard rules — double open, wrong-state commands, stop idempotence",
                   AddressOf Test_Guards)
            runner("SES: play from Stopped restarts from 0",
                   AddressOf Test_PlayFromStopped)
            runner("SES: dispose-then-everything → safe rejections, no events",
                   AddressOf Test_DisposeGuards)
            runner("SES: faults → FileMissing / NoVideoStream / CorruptFile / BackendMissing",
                   AddressOf Test_FaultMapping)
            runner("SES: seek failure (timeout 0) → advisory SeekFailed + recovery to Paused",
                   AddressOf Test_SeekFailureRecovery)
            runner("SES: audio requested on non-Windows → video-only fallback counted",
                   AddressOf Test_AudioFallbackHonest)
            runner($"SES: stress 50× open/play/stop/dispose (A↔B) — bounded",
                   AddressOf Test_SessionStress50)
        End Sub

        ' ---- helpers ----

        Friend Shared Function NewOptions(Optional audioEnabled As Boolean = True,
                                          Optional seekTimeoutMs As Integer = 5000) As PlaybackSessionOptions
            Return New PlaybackSessionOptions With {
                .FfmpegExe = TestMedia.FfmpegPath,
                .FfprobeExe = TestMedia.FfprobePath,
                .RenderWindow = IntPtr.Zero,          ' headless proof path (NullVideoSink)
                .AudioEnabled = audioEnabled,
                .SeekTimeoutMs = seekTimeoutMs
            }
        End Function

        Friend Shared Sub WaitPlayingOk(s As PlaybackSession, Optional timeoutMs As Integer = 20000)
            TestRunner.Assert(s.WaitForState(PlaybackState.Playing, timeoutMs),
                              $"session reached Playing (state={s.State}, fault={s.Fault?.ToString()})")
        End Sub

        Friend Shared Sub WaitPresentedAtLeast(s As PlaybackSession, count As Long,
                                               Optional timeoutMs As Integer = 30000)
            Dim deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            While DateTime.UtcNow < deadline
                If s.FramesPresented >= count Then Return
                Thread.Sleep(20)
            End While
            TestRunner.Assert(False,
                              $"presented {s.FramesPresented} < {count} within {timeoutMs}ms " &
                              $"(state={s.State}, fault={s.Fault?.ToString()})")
        End Sub

        Friend Shared Function CollectFaults(s As PlaybackSession) As List(Of GalleryVideoFault)
            Dim faults As New List(Of GalleryVideoFault)()
            AddHandler s.FaultRaised,
                Sub(sender, f)
                    SyncLock faults
                        faults.Add(f)
                    End SyncLock
                End Sub
            Return faults
        End Function

        ' ---- tests ----

        Private Shared Sub Test_OpenPlays()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                Dim faults = CollectFaults(s)

                TestRunner.Assert(s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60")).Accepted, "Open accepted")
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 30)

                ' Position advances in the single QPC domain
                Dim p1 = s.PositionTicks
                Thread.Sleep(250)
                Dim p2 = s.PositionTicks
                TestRunner.Assert(p2 > p1, $"position advances ({PlaybackClock.TicksToSeconds(p1):0.###} → {PlaybackClock.TicksToSeconds(p2):0.###}s)")
                TestRunner.Assert(s.Media IsNot Nothing AndAlso s.Media.HasVideo, "media probed")
                TestRunner.Assert(s.DurationTicks > 0, "duration probed")

                SyncLock faults
                    TestRunner.AssertEqual(0, faults.Count, "healthy open → no fault")
                End SyncLock
            End Using
        End Sub

        Private Shared Sub Test_PauseResume()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 20)

                TestRunner.Assert(s.Pause().Accepted, "Pause accepted")
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 5000), "Paused")
                TestRunner.Assert(s.ClockFrozen, "clock frozen")

                ' Park proof: state stays Paused; nothing faults; position static
                Dim posPaused = s.PositionTicks
                Thread.Sleep(400)
                TestRunner.AssertEqual(PlaybackState.Paused, s.State, "still Paused (decoder parked)")
                TestRunner.Assert(Math.Abs(s.PositionTicks - posPaused) < PlaybackClock.SecondsToTicks(0.05),
                                  "position frozen while paused")

                TestRunner.Assert(s.Play().Accepted, "Play accepted")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 10000), "Playing again")
                WaitPresentedAtLeast(s, s.FramesPresented + 10, 30000)
            End Using
        End Sub

        Private Shared Sub Test_EofNotFault()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                Dim faults = CollectFaults(s)
                Dim eosCount As Integer = 0
                AddHandler s.EosReached, Sub(sender) eosCount += 1

                s.Open(TestMedia.Synthetic("synth_video_only.mp4", "video_only")) ' 2s @30
                WaitPlayingOk(s)

                TestRunner.Assert(s.WaitForEof(30000), "EosReached within 30s")
                TestRunner.AssertEqual(PlaybackState.Paused, s.State, "EOF → Paused (hold last frame)")
                TestRunner.AssertEqual(1, eosCount, "exactly one EosReached")
                TestRunner.AssertEqual(1L, s.EofCount, "eof counted")

                ' Position ≈ end of file (within 1s)
                Dim durSecs = PlaybackClock.TicksToSeconds(s.DurationTicks)
                Dim posSecs = PlaybackClock.TicksToSeconds(s.PositionTicks)
                TestRunner.Assert(Math.Abs(posSecs - durSecs) <= 1.0,
                                  $"EOF position ≈ duration (pos {posSecs:0.##}s vs dur {durSecs:0.##}s)")

                Thread.Sleep(200)
                SyncLock faults
                    TestRunner.AssertEqual(0, faults.Count, "EOF is NOT a fault (§7)")
                End SyncLock
            End Using
        End Sub

        Private Shared Sub Test_SeekWhilePlaying()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 20)

                TestRunner.Assert(s.Seek(3.0).Accepted, "Seek accepted while Playing")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000),
                                  $"seek completed → Playing (state={s.State}, fault={s.Fault?.ToString()})")

                ' Wait for the FIRST POST-SEEK present (state flips to Playing
                ' before the render thread's next iteration — reading the last
                ' PTS immediately would race and see a pre-seek frame).
                Dim targetTicks = PlaybackClock.SecondsToTicks(3.0)
                Dim deadline = DateTime.UtcNow.AddSeconds(15)
                While DateTime.UtcNow < deadline AndAlso
                      s.LastPresentedPtsTicks < targetTicks - SeekToleranceTicks
                    Thread.Sleep(20)
                End While

                Dim lastPts = s.LastPresentedPtsTicks
                TestRunner.Assert(lastPts >= 0, "a frame was presented after seek")
                TestRunner.Assert(Math.Abs(lastPts - targetTicks) <= SeekToleranceTicks,
                                  $"first presented PTS ≈ 3.0s (got {PlaybackClock.TicksToSeconds(lastPts):0.###}s)")
                WaitPresentedAtLeast(s, s.FramesPresented + 10, 30000) ' still flowing
            End Using
        End Sub

        Private Shared Sub Test_PauseSeek()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 10)

                TestRunner.Assert(s.Pause().Accepted, "Pause")
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 5000), "Paused")

                TestRunner.Assert(s.Seek(3.0).Accepted, "Seek accepted while Paused")
                ' pause+seek returns to PAUSED at the target (§3.7 + resume mode)
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 15000),
                                  $"seek completed → Paused (state={s.State})")

                ' Wait for the post-seek target frame (same wake-before-present
                ' race as the seek-while-playing case).
                Dim targetTicks = PlaybackClock.SecondsToTicks(3.0)
                Dim deadline = DateTime.UtcNow.AddSeconds(15)
                While DateTime.UtcNow < deadline AndAlso
                      s.LastPresentedPtsTicks < targetTicks - SeekToleranceTicks
                    Thread.Sleep(20)
                End While

                Dim lastPts = s.LastPresentedPtsTicks
                TestRunner.Assert(Math.Abs(lastPts - targetTicks) <= SeekToleranceTicks,
                                  $"paused-seek shows target frame (got {PlaybackClock.TicksToSeconds(lastPts):0.###}s)")

                ' Resume from the seek target — playback continues
                TestRunner.Assert(s.Play().Accepted, "Play after paused-seek")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 10000), "Playing")
                Dim before = s.FramesPresented
                WaitPresentedAtLeast(s, before + 10, 30000)
            End Using
        End Sub

        Private Shared Sub Test_SeekToBeginning()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 10)
                TestRunner.Assert(s.Seek(0.0).Accepted, "Seek(0) accepted")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000), "back to Playing")
                Dim lastPts = s.LastPresentedPtsTicks
                TestRunner.Assert(Math.Abs(lastPts) <= SeekToleranceTicks,
                                  $"first frame ≈ 0s (got {PlaybackClock.TicksToSeconds(lastPts):0.###}s)")
            End Using
        End Sub

        Private Shared Sub Test_Guards()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                ' --- pre-open guards ---
                TestRunner.Assert(Not s.Play().Accepted, "Play in Created rejected")
                TestRunner.Assert(Not s.Pause().Accepted, "Pause in Created rejected")
                TestRunner.Assert(Not s.Seek(1.0).Accepted, "Seek in Created rejected")

                ' --- double open ---
                TestRunner.Assert(s.Open(TestMedia.Synthetic("synth_video_only.mp4", "video_only")).Accepted, "Open #1")
                TestRunner.Assert(Not s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60")).Accepted,
                                  "Open #2 while Opening rejected")
                WaitPlayingOk(s)

                ' --- wrong-state commands ---
                TestRunner.Assert(Not s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60")).Accepted,
                                  "Open while Playing rejected")
                TestRunner.Assert(s.Pause().Accepted, "Pause ok")
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 5000), "Paused")
                TestRunner.Assert(s.Pause().Accepted, "Pause in Paused = no-op OK")
                ' Negative target is CLAMPED to 0 by design (§3.7 step 1), not rejected.
                TestRunner.Assert(s.Seek(-5.0).Accepted, "negative seek accepted & clamped")
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 20000) OrElse
                                  s.WaitForState(PlaybackState.Playing, 20000), "clamped seek completes")

                ' --- stop idempotence ---
                TestRunner.Assert(s.Stop_().Accepted, "Stop #1")
                TestRunner.Assert(s.WaitForState(PlaybackState.Stopped, 15000), "Stopped")
                TestRunner.Assert(s.Stop_().Accepted, "Stop #2 idempotent OK")
                TestRunner.AssertEqual(PlaybackState.Stopped, s.State, "still Stopped")
                TestRunner.Assert(Not s.Seek(1.0).Accepted, "Seek in Stopped rejected")
                TestRunner.Assert(Not s.Pause().Accepted, "Pause in Stopped rejected")
            End Using
        End Sub

        Private Shared Sub Test_PlayFromStopped()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_video_only.mp4", "video_only"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 10)
                s.Stop_()
                TestRunner.Assert(s.WaitForState(PlaybackState.Stopped, 15000), "Stopped")

                ' Restart from the retained media (from 0)
                TestRunner.Assert(s.Play().Accepted, "Play from Stopped accepted")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000),
                                  $"Playing again (state={s.State}, fault={s.Fault?.ToString()})")
                WaitPresentedAtLeast(s, 10, 30000)
                Dim p = s.LastPresentedPtsTicks
                TestRunner.Assert(p >= 0 AndAlso p <= PlaybackClock.SecondsToTicks(1.0),
                                  $"restart shows beginning (got {PlaybackClock.TicksToSeconds(p):0.###}s)")
            End Using
        End Sub

        Private Shared Sub Test_DisposeGuards()
            TestMedia.RequireBinaries()
            Dim eventsAfter As New List(Of String)()
            Dim s As New PlaybackSession(NewOptions())
            AddHandler s.StateChanged,
                Sub(sender, oldSt, newSt)
                    SyncLock eventsAfter
                        eventsAfter.Add($"{oldSt}->{newSt}")
                    End SyncLock
                End Sub

            s.Open(TestMedia.Synthetic("synth_video_only.mp4", "video_only"))
            WaitPlayingOk(s)
            WaitPresentedAtLeast(s, 5)

            s.Dispose()   ' from Playing — full synchronous teardown
            TestRunner.AssertEqual(PlaybackState.Disposed, s.State, "Disposed")
            Dim eventsAtDispose As Integer = eventsAfter.Count

            s.Dispose()   ' idempotent — no throw
            TestRunner.AssertEqual(PlaybackState.Disposed, s.State, "still Disposed")

            ' Every API: safe rejection, no throw
            TestRunner.Assert(Not s.Open(TestMedia.Synthetic("synth_video_only.mp4", "video_only")).Accepted, "Open after dispose")
            TestRunner.Assert(Not s.Play().Accepted, "Play after dispose")
            TestRunner.Assert(Not s.Pause().Accepted, "Pause after dispose")
            TestRunner.Assert(Not s.Seek(1.0).Accepted, "Seek after dispose (owner case)")
            TestRunner.Assert(Not s.Stop_().Accepted, "Stop after dispose")

            Thread.Sleep(300)
            TestRunner.AssertEqual(eventsAtDispose, eventsAfter.Count,
                                   "no StateChanged events after Dispose returned")
        End Sub

        Private Shared Sub Test_FaultMapping()
            TestMedia.RequireBinaries()

            ' --- FileMissing ---
            Using s As New PlaybackSession(NewOptions())
                Dim missing = IO.Path.Combine(TestMedia.Sandbox, "nope_" & Guid.NewGuid().ToString("N") & ".mp4")
                s.Open(missing)
                TestRunner.Assert(s.WaitForState(PlaybackState.Faulted, 15000), "Faulted (missing)")
                TestRunner.Assert(s.Fault.Kind = GalleryVideoFaultKind.FileMissing,
                                  $"kind=FileMissing (got {s.Fault.Kind})")
            End Using

            ' --- NoVideoStream (audio-only file in the video viewer) ---
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_audio_only.mp4", "audio_only"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Faulted, 20000), "Faulted (audio-only)")
                TestRunner.Assert(s.Fault.Kind = GalleryVideoFaultKind.NoVideoStream,
                                  $"kind=NoVideoStream (got {s.Fault.Kind})")
            End Using

            ' --- CorruptFile ---
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.CorruptFile())
                TestRunner.Assert(s.WaitForState(PlaybackState.Faulted, 20000), "Faulted (corrupt)")
                TestRunner.Assert(s.Fault.Kind = GalleryVideoFaultKind.CorruptFile,
                                  $"kind=CorruptFile (got {s.Fault.Kind})")
            End Using

            ' --- UnsupportedFormat (§7 gate: probed codec outside the set) ---
            ' mpeg4-in-mp4 is valid media (probe succeeds) but outside the
            ' supported h264/yuv420p set pinned in design doc §5 — the session
            ' must fault BEFORE any pipeline allocation (renderer/decode).
            Using s As New PlaybackSession(NewOptions())
                s.Open(TestMedia.Synthetic("synth_wrong_codec.mp4", "wrong_codec"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Faulted, 20000), "Faulted (wrong codec)")
                TestRunner.Assert(s.Fault.Kind = GalleryVideoFaultKind.UnsupportedFormat,
                                  $"kind=UnsupportedFormat (got {s.Fault.Kind})")
                TestRunner.Assert(s.Fault.Detail.Contains("mpeg4"),
                                  $"detail lists probed codec (got: {s.Fault.Detail})")
            End Using

            ' --- BackendMissing (broken binaries) ---
            Dim opts As PlaybackSessionOptions = NewOptions()
            opts.FfmpegExe = "/nonexistent/ffmpeg"
            opts.FfprobeExe = "/nonexistent/ffprobe"
            Using s As New PlaybackSession(opts)
                s.Open(TestMedia.Synthetic("synth_video_only.mp4", "video_only"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Faulted, 20000), "Faulted (backend)")
                TestRunner.Assert(s.Fault.Kind = GalleryVideoFaultKind.BackendMissing,
                                  $"kind=BackendMissing (got {s.Fault.Kind})")
            End Using
        End Sub

        Private Shared Sub Test_SeekFailureRecovery()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions(seekTimeoutMs:=0))
                Dim faults = CollectFaults(s)

                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 10)

                TestRunner.Assert(s.Seek(1.0).Accepted, "Seek issued")
                ' With SeekTimeoutMs=0 no frame can arrive in time → advisory
                ' fault + recovery to Paused at the seek ORIGIN (§7) — the
                ' session must stay USABLE (never a terminal fault here).
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 20000),
                                  $"recovered to Paused (state={s.State}, fault={s.Fault?.ToString()})")

                SyncLock faults
                    TestRunner.Assert(faults.Count > 0, "advisory SeekFailed fault event raised")
                    TestRunner.Assert(faults(0).Kind = GalleryVideoFaultKind.SeekFailed,
                                      $"kind=SeekFailed (got {faults(0).Kind})")
                End SyncLock

                ' Session remains usable: play resumes and frames flow.
                TestRunner.Assert(s.Play().Accepted, "Play after seek failure")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing again")
                WaitPresentedAtLeast(s, 10, 30000)
            End Using
        End Sub

        Private Shared Sub Test_AudioFallbackHonest()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions(audioEnabled:=True))
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 10)

                If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                    ' Real endpoint — fallback not expected (but not forbidden).
                    TestRunner.Assert(s.AudioFallbackToVideoOnly >= 0, "counter sane")
                Else
                    ' No WASAPI on Linux → honest video-only degradation, counted.
                    TestRunner.AssertEqual(1L, s.AudioFallbackToVideoOnly,
                                           "audio → video-only fallback counted off-Windows")
                End If
            End Using
        End Sub

        Private Shared Sub Test_SessionStress50()
            TestMedia.RequireBinaries()
            Dim pathA = TestMedia.Synthetic("synth_video_only.mp4", "video_only")
            Dim pathB = TestMedia.Synthetic("synth_av60.mp4", "av60")

            ' Warmup
            RunStressIteration(pathA)
            RunStressIteration(pathB)

            GC.Collect() : GC.WaitForPendingFinalizers() : GC.Collect()
            Dim memStart = GC.GetTotalMemory(True)

            Const Iterations As Integer = 50
            For i = 0 To Iterations - 1
                RunStressIteration(If(i Mod 2 = 0, pathA, pathB))
            Next

            GC.Collect() : GC.WaitForPendingFinalizers() : GC.Collect()
            Dim memEnd = GC.GetTotalMemory(True)

            ' Bounded memory trend (same band policy as StressLoopTests)
            TestRunner.Assert(memEnd < memStart * 4 + 64 * 1024 * 1024,
                              $"session memory bounded ({memStart \ 1024}KB → {memEnd \ 1024}KB)")
        End Sub

        ''' <summary>One full session cycle: open → play ≥5 frames → stop → dispose.
        ''' Throws on any failure (the runner counts it).</summary>
        Private Shared Sub RunStressIteration(path As String)
            Using s As New PlaybackSession(NewOptions())
                TestRunner.Assert(s.Open(path).Accepted, "stress open")
                WaitPlayingOk(s)
                WaitPresentedAtLeast(s, 5, 30000)
                TestRunner.Assert(s.Stop_().Accepted, "stress stop")
                TestRunner.Assert(s.WaitForState(PlaybackState.Stopped, 20000), "stress stopped")
            End Using
        End Sub

    End Class

End Namespace
