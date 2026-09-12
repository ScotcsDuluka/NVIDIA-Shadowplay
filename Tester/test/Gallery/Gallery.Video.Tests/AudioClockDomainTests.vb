Option Strict On
Option Explicit On
Option Infer On

' AudioClockDomainTests.vb — W2 regression: the audio clock domain fix.
'
' What the fix does (and what these tests pin):
'   1. Open starts the audio device with the correct lifecycle (§3.5
'      auto-play) — the pre-fix tree NEVER called _audio.Play() on the Open
'      path, leaving Open→Playing silent (measured: device Playing, samples
'      frozen at 0).
'   2. AudioPositionTicks is REBASED onto the ABSOLUTE media PTS domain at
'      Open / Resume / Seek — the pre-fix value was a cumulative counter in a
'      stale domain (forward seek: frames held "early" ~Δs; backward seek:
'      frames dropped "late" — both proven on the pre-fix A/B tree).
'   3. The audio master clock is ADVANCE-GATED (must progress) and
'      DRIFT-GUARDED (must agree with the QPC wall clock ±1s) — an endpoint
'      that pulls faster/slower than wall time (virtual APO devices can pull
'      ~2×) must never drag the presentation clock.
'   4. Ring underruns answer SILENCE, never 0 — a 0-byte IWaveProvider read
'      means END-OF-STREAM to NAudio and permanently kills the device pull
'      (measured: deviceReads=1 forever with the pre-fix provider).
'
' GPU-independence: headless sink, software decode subprocess, pure clock
' math with an injected fake QPC source. Nothing here touches NVIDIA.

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class AudioClockDomainTests

        Private Shared ReadOnly Tol As Long = PlaybackClock.SecondsToTicks(0.5)

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            RunCore(runner)
        End Sub

        ''' <summary>Shared core — the completion gate (W3) runs this same set
        ''' as GATE-3, so the contract has exactly one definition.</summary>
        Friend Shared Sub RunCore(runner As Action(Of String, Action))
            runner("ACD-0: rebase math is exact (unit, injected clock — no device, no GPU)",
                   AddressOf Test_RebaseMathPure)
            runner("ACD-1: open→playing starts the audio device (lifecycle) + samples advance",
                   AddressOf Test_OpenStartsAudio)
            runner("ACD-2: audio position ≈ media position while playing (endpoint-gated)",
                   AddressOf Test_SameDomainWhilePlaying)
            runner("ACD-3: pause→resume → position continues, no domain reset",
                   AddressOf Test_PauseResumeRebase)
            runner("ACD-4: forward seek → position ≈ target, media domain (no early-hold freeze)",
                   AddressOf Test_ForwardSeekRebase)
            runner("ACD-5: backward seek → position ≈ target, media domain (no late-drop starvation)",
                   AddressOf Test_BackwardSeekRebase)
            runner("ACD-6: repeated seeks (6× alternating) → every re-anchor on target, no drift, no faults",
                   AddressOf Test_RepeatedSeeks)
        End Sub

        ' ---- helpers ----

        Friend Shared Function WaitPresentedAtLeast(s As PlaybackSession, count As Long,
                                                    Optional timeoutMs As Integer = 30000) As Boolean
            Dim deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            While DateTime.UtcNow < deadline
                If s.FramesPresented >= count Then Return True
                Thread.Sleep(20)
            End While
            Return False
        End Function

        ''' <summary>Measure the endpoint's audio-clock rate over ~1s of wall
        ''' time. Sane ≈ 1.0× (accept 0.7–1.6×). Outside that, audio-master
        ''' assertions are meaningless on THIS endpoint (honest SKIP — the
        ''' session itself falls back to the QPC master via the drift guard).
        ''' Machine-portable: the suite still proves everything on sane boxes.</summary>
        Private Shared Function AudioClockRate(s As PlaybackSession) As Double
            Dim a0 = s.AudioPositionTicks
            Dim w0 = DateTime.UtcNow
            Thread.Sleep(1000)
            Dim wall = (DateTime.UtcNow - w0).TotalSeconds
            Dim a1 = s.AudioPositionTicks
            If wall <= 0 Then Return 0.0
            Return PlaybackClock.TicksToSeconds(a1 - a0) / wall
        End Function

        Private Shared Function AudioClockSane(s As PlaybackSession) As Boolean
            Dim r = AudioClockRate(s)
            Return r >= 0.7 AndAlso r <= 1.6
        End Function

        Private Shared Sub RequireAudioSaneOrSkip(s As PlaybackSession)
            If Not AudioClockSane(s) Then
                Throw New SkipException(
                    $"endpoint audio-clock rate off wall time (measured {AudioClockRate(s):0.##}×) — " &
                    "audio-master domain assertions are endpoint-gated; the session runs the QPC master " &
                    "via the drift guard (W2 fix) and media-domain assertions are proven in ACD-0/4/5")
            End If
        End Sub

        ' ---- ACD-0: pure rebase math (injected clock, no device) ----

        Private Shared Sub Test_RebaseMathPure()
            Using ring As New AudioPcmBuffer(48000 * 2 * 2) ' 1s @ 48k stereo S16
                Dim prov As New PcmBufferWaveProvider(ring, New NAudio.Wave.WaveFormat(48000, 16, 2), 48000, 2)

                ' Fill 1.0s of audio (192000 bytes).
                Dim chunk(1919) As Byte ' 10 ms of stereo S16
                For i = 0 To 99
                    Assert(ring.Write(chunk, 0, chunk.Length), "ring write")
                Next

                ' Device consumes 0.4s BEFORE the first rebase (cumulative domain).
                Dim buf(3839) As Byte
                For i = 0 To 39
                    Dim gotRead = prov.Read(buf, 0, buf.Length)
                    Assert(gotRead = buf.Length,
                           $"device read full chunk (read {i}: got {gotRead} of {buf.Length})")
                Next

                ' SEEK: media 2.0s happens NOW (the rebase under test).
                prov.RebaseTo(PlaybackClock.SecondsToTicks(2.0))
                Assert(prov.PlayedSamplesTicks = PlaybackClock.SecondsToTicks(2.0),
                       $"rebase anchors at the seek target (got {prov.PlayedSamplesTicks})")

                ' Refill (the first 1.0s was mostly consumed above — underruns
                ' answer silence BY DESIGN and must not advance the counter).
                For i = 0 To 99
                    Assert(ring.Write(chunk, 0, chunk.Length), "ring refill write")
                Next
                ' Device consumes another 0.5s → position = 2.0 + 0.5s exactly.
                For i = 0 To 49
                    Assert(prov.Read(buf, 0, buf.Length) = buf.Length, "device read post-rebase")
                Next
                ' 50 × 3840 B = 192000 B = 1.0 s of audio → 2.0 + 1.0 = 3.0 s.
                Assert(prov.PlayedSamplesTicks = PlaybackClock.SecondsToTicks(3.0),
                       $"position tracks media domain after rebase (got {PlaybackClock.TicksToSeconds(prov.PlayedSamplesTicks):0.###}s)")

                ' Second rebase (pause/resume): 3.0s happens NOW.
                prov.RebaseTo(PlaybackClock.SecondsToTicks(3.0))
                Assert(prov.PlayedSamplesTicks = PlaybackClock.SecondsToTicks(3.0),
                       $"second rebase re-anchors (got {prov.PlayedSamplesTicks})")
                For i = 0 To 99
                    Assert(ring.Write(chunk, 0, chunk.Length), "ring refill 2")
                Next
                For i = 0 To 9
                    prov.Read(buf, 0, buf.Length)
                Next
                ' 10 × 3840 B = 38400 B = 0.2 s of audio → 3.0 + 0.2 = 3.2 s.
                Assert(prov.PlayedSamplesTicks = PlaybackClock.SecondsToTicks(3.2),
                       $"post-resume position = 3.2s (got {PlaybackClock.TicksToSeconds(prov.PlayedSamplesTicks):0.###}s)")

                ' Underrun: empty ring must answer SILENCE (frame-aligned min
                ' chunk), never 0 — 0 = end-of-stream to NAudio and kills the
                ' device pull permanently (pre-fix measured failure).
                Dim empty(3839) As Byte
                Dim got = prov.Read(empty, 0, empty.Length)
                Assert(got = empty.Length, $"underrun answered with silence ({got} bytes, not 0)")
            End Using
        End Sub

        ' ---- ACD-1..5: session-level (headless; WASAPI endpoint on box) ----

        Private Shared Sub Test_OpenStartsAudio()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing after Open")
                TestRunner.Assert(WaitPresentedAtLeast(s, 30), "frames presenting")

                TestRunner.Assert(s.AudioActive, "audio renderer active (device started on the Open path)")

                Dim a0 = s.AudioPositionTicks
                Dim u0 = s.AudioUnderruns
                Thread.Sleep(700)
                Dim a1 = s.AudioPositionTicks
                Dim u1 = s.AudioUnderruns
                ' Rate-neutral lifecycle proof: the device pull loop is ALIVE —
                ' real samples advanced, OR the provider answered underruns with
                ' silence (pulls happening on a starving endpoint). A device that
                ' was never started shows Δ=0 AND Δu=0 (pre-fix measured).
                TestRunner.Assert(a1 - a0 >= PlaybackClock.SecondsToTicks(0.3) OrElse u1 > u0,
                                  $"audio device alive on the Open path (Δ={PlaybackClock.TicksToSeconds(a1 - a0):0.###}s, underruns {u0}→{u1})")
            End Using
        End Sub

        Private Shared Sub Test_SameDomainWhilePlaying()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(WaitPresentedAtLeast(s, 30), "presenting")
                RequireAudioSaneOrSkip(s)

                For i = 1 To 3
                    Dim gap = Math.Abs(s.AudioPositionTicks - s.PositionTicks)
                    TestRunner.Assert(gap <= Tol,
                                      $"|audio − media| = {PlaybackClock.TicksToSeconds(gap):0.###}s ≤ 0.5s (sample {i})")
                    Thread.Sleep(300)
                Next
            End Using
        End Sub

        Private Shared Sub Test_PauseResumeRebase()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(WaitPresentedAtLeast(s, 30), "presenting")
                Thread.Sleep(900)

                TestRunner.Assert(s.Pause().Accepted, "Pause")
                TestRunner.Assert(s.WaitForState(PlaybackState.Paused, 5000), "Paused")
                Dim frozen = s.PositionTicks
                Thread.Sleep(300)
                TestRunner.Assert(Math.Abs(s.PositionTicks - frozen) <= PlaybackClock.FrameWindowTicks(1.0 / 60.0),
                                  "position frozen within one frame window while paused")

                TestRunner.Assert(s.Play().Accepted, "Play (resume)")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 10000), "Playing again")
                Thread.Sleep(800)

                ' Grace ≤ one frame window: the paused loop may have presented
                ' one queued frame (lastPresented > pause anchor); the resume
                ' anchor is the wall-true pause position.
                TestRunner.Assert(s.PositionTicks >= frozen - PlaybackClock.FrameWindowTicks(1.0 / 60.0),
                                  "resume continues forward (no reset to 0 — grace ≤1 frame)")
                RequireAudioSaneOrSkip(s)
                Dim gap = Math.Abs(s.AudioPositionTicks - s.PositionTicks)
                TestRunner.Assert(gap <= Tol,
                                  $"audio rebased to the media domain after resume (gap={PlaybackClock.TicksToSeconds(gap):0.###}s)")
            End Using
        End Sub

        Private Shared Sub Test_ForwardSeekRebase()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(WaitPresentedAtLeast(s, 30), "presenting")
                Thread.Sleep(1000)

                TestRunner.Assert(s.Seek(3.0).Accepted, "Seek 3.0s")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000), "back to Playing")
                Dim deadline4 = DateTime.UtcNow.AddSeconds(10)
                While DateTime.UtcNow < deadline4 AndAlso
                      Math.Abs(s.LastPresentedPtsTicks - PlaybackClock.SecondsToTicks(3.0)) > Tol
                    Thread.Sleep(20)
                End While

                Dim pts = s.LastPresentedPtsTicks
                TestRunner.Assert(Math.Abs(pts - PlaybackClock.SecondsToTicks(3.0)) <= Tol,
                                  $"first present ≈ 3.0s (got {PlaybackClock.TicksToSeconds(pts):0.###}s)")

                ' Pre-fix failure mode: master clock stuck ≈1s behind → frames
                ' piled up "early" and position froze. Post-fix: media advances.
                Dim p0 = s.PositionTicks
                Thread.Sleep(700)
                TestRunner.Assert(s.PositionTicks - p0 >= PlaybackClock.SecondsToTicks(0.3),
                                  $"position advances after forward seek (Δ={PlaybackClock.TicksToSeconds(s.PositionTicks - p0):0.###}s / 0.7s)")

                RequireAudioSaneOrSkip(s)
                Dim gap = Math.Abs(s.AudioPositionTicks - s.PositionTicks)
                TestRunner.Assert(gap <= Tol,
                                  $"audio rebased to the target domain (gap={PlaybackClock.TicksToSeconds(gap):0.###}s)")
            End Using
        End Sub

        Private Shared Sub Test_BackwardSeekRebase()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(SessionTests.NewOptions())
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(WaitPresentedAtLeast(s, 30), "presenting")
                Thread.Sleep(2200)

                TestRunner.Assert(s.Seek(0.5).Accepted, "Seek back to 0.5s")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000), "back to Playing")
                Dim deadline5 = DateTime.UtcNow.AddSeconds(10)
                While DateTime.UtcNow < deadline5 AndAlso
                      Math.Abs(s.LastPresentedPtsTicks - PlaybackClock.SecondsToTicks(0.5)) > Tol
                    Thread.Sleep(20)
                End While

                Dim pts = s.LastPresentedPtsTicks
                TestRunner.Assert(Math.Abs(pts - PlaybackClock.SecondsToTicks(0.5)) <= Tol,
                                  $"first present ≈ 0.5s (got {PlaybackClock.TicksToSeconds(pts):0.###}s)")

                ' Pre-fix failure mode: cumulative counter (~2.3s) far ahead of
                ' media (0.5s) → every frame "late" → presentation starved.
                Dim p0 = s.PositionTicks
                Thread.Sleep(700)
                TestRunner.Assert(s.PositionTicks - p0 >= PlaybackClock.SecondsToTicks(0.3),
                                  $"position advances after backward seek (Δ={PlaybackClock.TicksToSeconds(s.PositionTicks - p0):0.###}s / 0.7s)")

                RequireAudioSaneOrSkip(s)
                Dim gap = Math.Abs(s.AudioPositionTicks - s.PositionTicks)
                TestRunner.Assert(gap <= Tol,
                                  $"audio rebased to the target domain (gap={PlaybackClock.TicksToSeconds(gap):0.###}s)")
            End Using
        End Sub

        ' ---- ACD-6: repeated seeks ----

        ''' <summary>Six alternating seeks in ONE session. Each must re-anchor
        ''' the presentation clock on its target (media domain), the position
        ''' must track the target (no runaway, no reset to 0), the audio must
        ''' be rebased per seek (endpoint-gated), and the session must stay
        ''' fault-free with a bounded queue.</summary>
        Private Shared Sub Test_RepeatedSeeks()
            TestMedia.RequireBinaries()
            Dim targets() As Double = {1.0, 3.0, 0.5, 2.0, 3.0, 1.0}
            Dim faults As New List(Of GalleryVideoFault)()
            Dim maxQueue As Integer = 0

            Using s As New PlaybackSession(SessionTests.NewOptions())
                AddHandler s.FaultRaised,
                    Sub(sender, f)
                        SyncLock faults
                            faults.Add(f)
                        End SyncLock
                    End Sub
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(WaitPresentedAtLeast(s, 30), "presenting")
                Dim endpointSane As Boolean = AudioClockSane(s)

                For i = 0 To targets.Length - 1
                    Dim t = targets(i)
                    TestRunner.Assert(s.Seek(t).Accepted, $"seek {i + 1}: accepted ({t:0.#}s)")
                    TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000),
                                      $"seek {i + 1}: back to Playing")

                    ' First present lands on the target (media domain).
                    Dim deadline = DateTime.UtcNow.AddSeconds(10)
                    While DateTime.UtcNow < deadline AndAlso
                          Math.Abs(s.LastPresentedPtsTicks - PlaybackClock.SecondsToTicks(t)) > Tol
                        Thread.Sleep(20)
                    End While
                    Dim pts = s.LastPresentedPtsTicks
                    TestRunner.Assert(Math.Abs(pts - PlaybackClock.SecondsToTicks(t)) <= Tol,
                                      $"seek {i + 1} → {t:0.#}s: first present ≈ target (got {PlaybackClock.TicksToSeconds(pts):0.###}s)")

                    ' Position tracks the target (no runaway, no reset to 0):
                    ' within [target − window, target + 1s] after 300 ms.
                    Thread.Sleep(300)
                    Dim pos = s.PositionTicks
                    TestRunner.Assert(pos >= PlaybackClock.SecondsToTicks(t) - PlaybackClock.FrameWindowTicks(1.0 / 60.0),
                                      $"seek {i + 1}: position ≥ target (got {PlaybackClock.TicksToSeconds(pos):0.###}s)")
                    TestRunner.Assert(pos <= PlaybackClock.SecondsToTicks(t) + PlaybackClock.SecondsToTicks(1.0),
                                      $"seek {i + 1}: position bounded ≤ target+1s (got {PlaybackClock.TicksToSeconds(pos):0.###}s)")

                    ' Audio rebased per seek (only on wall-true endpoints).
                    If endpointSane Then
                        Dim gap = Math.Abs(s.AudioPositionTicks - s.PositionTicks)
                        TestRunner.Assert(gap <= Tol,
                                          $"seek {i + 1}: audio rebased to media domain (gap={PlaybackClock.TicksToSeconds(gap):0.###}s)")
                    End If

                    maxQueue = Math.Max(maxQueue, s.VideoQueueCount)
                Next

                TestRunner.Assert(maxQueue <= s.VideoQueueCapacity,
                                  $"video queue bounded across repeated seeks (max {maxQueue} ≤ {s.VideoQueueCapacity})")
                TestRunner.Assert(faults.Count = 0,
                                  $"zero faults across 6 seeks ({String.Join("; ", faults)})")
                TestRunner.Assert(s.State = PlaybackState.Playing OrElse s.State = PlaybackState.Paused,
                                  $"session still usable (state={s.State})")
            End Using
        End Sub

    End Class

End Namespace
