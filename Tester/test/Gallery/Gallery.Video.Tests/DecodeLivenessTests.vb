Option Strict On
Option Explicit On
Option Infer On

' DecodeLivenessTests.vb — F4 decode-stall guard (DecodeStallWatchdog +
' PlaybackSession.RenderLoop wiring). Covers the W3-audit freeze family:
' a decode worker that stays ALIVE while producing nothing used to hold
' Playing forever (no EOF, no fault event ever fired).
'
' Tiers:
'   WDOG  — pure decision-math tests (no ffmpeg, no threads, synthetic QPC).
'   LIVE  — real ffmpeg + real PlaybackSession:
'     EOF          : natural end → Paused + EosReached, watchdog must NOT fire.
'     pipe-close   : video ffmpeg KILLED mid-play → stdout closes → EOF path
'                    → Paused (bounded; never stuck Playing).
'     silent decode: decoder SUSPENDED right after start → zero progress →
'                    DecodeStalled fault + Paused at ~DecodeStallTimeoutMs.
'     process-stall: decoder SUSPENDED after frames flowed → same guard.
'
' Process suspension: SIGSTOP/SIGCONT via libc on Linux, NtSuspendProcess/
' NtResumeProcess on Windows — both suspend the WHOLE process, so the pipe
' stays open (a real alive-but-silent decoder, exactly the F4 signature).
' The exact PID comes from the worker's VideoProcessId seam — never a
' name-based ffmpeg lookup (other tests' children would race).

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Threading

Namespace Gallery.Video.Tests

    Friend Module ProcessControl

        <DllImport("ntdll.dll")>
        Private Function NtSuspendProcess(handle As IntPtr) As Integer
        End Function

        <DllImport("ntdll.dll")>
        Private Function NtResumeProcess(handle As IntPtr) As Integer
        End Function

        ' libc kill: SIGSTOP = 19, SIGCONT = 18
        <DllImport("libc", SetLastError:=True)>
        Private Function kill(pid As Integer, sig As Integer) As Integer
        End Function

        Friend Sub Suspend(pid As Integer)
            Try
                If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                    Using p = Process.GetProcessById(pid)
                        NtSuspendProcess(p.Handle)
                    End Using
                Else
                    kill(pid, 19)
                End If
            Catch
                ' process already gone — best-effort control
            End Try
        End Sub

        Friend Sub [Resume](pid As Integer)
            Try
                If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                    Using p = Process.GetProcessById(pid)
                        NtResumeProcess(p.Handle)
                    End Using
                Else
                    kill(pid, 18)
                End If
            Catch
                ' process already gone — best-effort control
            End Try
        End Sub

        Friend Sub KillTree(pid As Integer)
            Try
                Using p = Process.GetProcessById(pid)
                    p.Kill(entireProcessTree:=True)
                End Using
            Catch
                ' already gone
            End Try
        End Sub

    End Module

    Friend Class DecodeLivenessTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("WDOG: silent decode fires exactly at the timeout window",
                   AddressOf Test_Watchdog_SilentFiresAtTimeout)
            runner("WDOG: counter advance resets the window",
                   AddressOf Test_Watchdog_ProgressResets)
            runner("WDOG: EOF short-circuits — never fires",
                   AddressOf Test_Watchdog_EofShortCircuits)
            runner("WDOG: drain-progress (MarkProgress) delays firing",
                   AddressOf Test_Watchdog_MarkProgressDelays)
            runner("WDOG: Reset re-arms a fresh window (generation semantics)",
                   AddressOf Test_Watchdog_ResetRearms)
            runner("LIVE: EOF natural end → Paused, zero stall faults (no false positive)",
                   AddressOf Test_Live_EofNoFalseStall)
            runner("LIVE: pipe-close (video ffmpeg killed) → bounded Paused, never stuck Playing",
                   AddressOf Test_Live_PipeClose)
            runner("LIVE: silent decode (suspended at start) → DecodeStalled + Paused",
                   AddressOf Test_Live_SilentDecode)
            runner("LIVE: process-stall (suspended mid-play) → DecodeStalled + Paused",
                   AddressOf Test_Live_ProcessStall)
        End Sub

        ' ---- WDOG: pure decision math (frequency injected, nowQpc synthetic) ----

        Private Const Freq As Long = 10000000L ' 100ns/tick — 1 tick = 1 unit below

        Private Shared Function Wdog(timeoutMs As Integer) As DecodeStallWatchdog
            Return New DecodeStallWatchdog(timeoutMs, Freq)
        End Function

        Private Shared Sub Test_Watchdog_SilentFiresAtTimeout()
            Dim w = Wdog(500) ' 500ms = 5,000,000 ticks
            w.Reset(0L)
            ' First poll establishes the counter baseline — never a stall.
            TestRunner.Assert(Not w.IsStalled(0, False, 0L), "first poll is baseline")
            ' Just before the window: silent.
            TestRunner.Assert(Not w.IsStalled(0, False, 4999999L), "just under window")
            ' Exactly at the window: stalled.
            TestRunner.Assert(w.IsStalled(0, False, 5000000L), "exactly at window")
            ' Latched: still stalled afterwards (single-shot is the caller's guard).
            TestRunner.Assert(w.IsStalled(0, False, 6000000L), "latched after expiry")
        End Sub

        Private Shared Sub Test_Watchdog_ProgressResets()
            Dim w = Wdog(500)
            w.Reset(0L)
            w.IsStalled(0, False, 0L)
            ' Counter advanced at t=4,999,999 (just before expiry) → window restarts.
            TestRunner.Assert(Not w.IsStalled(5, False, 4999999L), "progress at the wire resets")
            TestRunner.Assert(Not w.IsStalled(5, False, 9999998L), "new window still open")
            TestRunner.Assert(w.IsStalled(5, False, 9999999L), "new window expires exactly")
        End Sub

        Private Shared Sub Test_Watchdog_EofShortCircuits()
            Dim w = Wdog(100)
            w.Reset(0L)
            w.IsStalled(7, False, 0L)
            ' Counter frozen, window long expired — but EOF owns the transition.
            TestRunner.Assert(Not w.IsStalled(7, True, 99999999L), "EOF short-circuits")
            ' Same frozen state WITHOUT EOF fires — proves the short-circuit
            ' above was the EOF branch, not lost state.
            Dim w2 = Wdog(100)
            w2.Reset(0L)
            w2.IsStalled(7, False, 0L)
            TestRunner.Assert(w2.IsStalled(7, False, 99999999L), "same state without EOF fires")
        End Sub

        Private Shared Sub Test_Watchdog_MarkProgressDelays()
            Dim w = Wdog(500)
            w.Reset(0L)
            w.IsStalled(3, False, 0L)
            ' Queue drain: external progress at t=4,900,000 (counter frozen).
            w.MarkProgress(4900000L)
            TestRunner.Assert(Not w.IsStalled(3, False, 9799999L), "drain kept window open")
            TestRunner.Assert(w.IsStalled(3, False, 9900000L), "drain ended → fires")
        End Sub

        Private Shared Sub Test_Watchdog_ResetRearms()
            Dim w = Wdog(100)
            w.Reset(0L)
            w.IsStalled(50, False, 0L)
            TestRunner.Assert(w.IsStalled(50, False, 99999999L), "expired before reset")
            ' New generation: counter domain restarts at 0, window re-armed.
            w.Reset(100000000L)
            TestRunner.Assert(Not w.IsStalled(0, False, 100000000L), "fresh baseline")
            TestRunner.Assert(Not w.IsStalled(0, False, 100999999L), "fresh window open")
            TestRunner.Assert(w.IsStalled(0, False, 101000000L), "fresh window expires")
        End Sub

        ' ---- LIVE: real ffmpeg + real session ----

        ''' <summary>8s video-only file: decode of 240 frames keeps the
        ''' decoder busy long enough to suspend mid-play deterministically
        ''' (the shared 2s clip can decode to natural EOF before the harness
        ''' ever suspends it — a race, not a test).</summary>
        Private Shared Function LongVideo() As String
            Dim p = IO.Path.Combine(TestMedia.Sandbox, "dlv_long.mp4")
            If IO.File.Exists(p) AndAlso New IO.FileInfo(p).Length > 1024 Then Return p
            Dim args = "-y -f lavfi -i testsrc2=size=320x240:rate=30:duration=8 " &
                       "-c:v libx264 -pix_fmt yuv420p -g 30 """ & p & """"
            Dim so As String = Nothing, se As String = Nothing
            Dim code As Integer = -1
            If Not MediaProbe.RunCapture(TestMedia.FfmpegPath, args, 120000, so, se, code) OrElse code <> 0 Then
                Throw New SkipException("8s synth generation failed: " & se)
            End If
            Return p
        End Function

        Private Shared Function NewOptions(stallTimeoutMs As Integer) As PlaybackSessionOptions
            Return New PlaybackSessionOptions With {
                .FfmpegExe = TestMedia.FfmpegPath,
                .FfprobeExe = TestMedia.FfprobePath,
                .RenderWindow = IntPtr.Zero,
                .AudioEnabled = True,
                .DecodeStallTimeoutMs = stallTimeoutMs
            }
        End Function

        Private Shared Function CollectFaults(s As PlaybackSession) As List(Of GalleryVideoFault)
            Dim faults As New List(Of GalleryVideoFault)()
            AddHandler s.FaultRaised,
                Sub(sender, f)
                    SyncLock faults
                        faults.Add(f)
                    End SyncLock
                End Sub
            Return faults
        End Function

        Private Shared Function HasFault(faults As List(Of GalleryVideoFault), kind As GalleryVideoFaultKind) As Boolean
            SyncLock faults
                For Each f In faults
                    If f.Kind = kind Then Return True
                Next
            End SyncLock
            Return False
        End Function

        ''' <summary>Wait until the live generation reports a video PID (the
        ''' spawn is async relative to Open's state transition).</summary>
        Private Shared Function WaitVideoPid(s As PlaybackSession, timeoutMs As Integer) As Integer
            Dim deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            While DateTime.UtcNow < deadline
                Dim pid = s.DecodeVideoProcessId
                If pid > 0 Then Return pid
                Thread.Sleep(10)
            End While
            TestRunner.Assert(False, $"video pid never appeared (state={s.State}, fault={s.Fault?.ToString()})")
            Return -1
        End Function

        Private Shared Sub Test_Live_EofNoFalseStall()
            TestMedia.RequireBinaries()
            ' Stall timeout deliberately SHORTER than the full playback of the
            ' 2s file? No — must exceed any legal decode gap. 10s default, file
            ' ends in ~2s: the guard must stay silent through the natural end.
            Using s As New PlaybackSession(NewOptions(10000))
                Dim faults = CollectFaults(s)
                Dim eosCount As Integer = 0
                AddHandler s.EosReached, Sub(sender) eosCount += 1

                s.Open(TestMedia.Synthetic("dlv_video_only.mp4", "video_only")) ' 2s @30
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(s.WaitForEof(30000), "natural EOF")
                TestRunner.AssertEqual(PlaybackState.Paused, s.State, "EOF → Paused")
                TestRunner.AssertEqual(1, eosCount, "one EosReached")
                Thread.Sleep(200)
                TestRunner.Assert(Not HasFault(faults, GalleryVideoFaultKind.DecodeStalled),
                                  "natural EOF must not raise DecodeStalled")
                TestRunner.AssertEqual(0L, s.DecodeStallCount, "stall count stays 0")
            End Using
        End Sub

        Private Shared Sub Test_Live_PipeClose()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions(10000))
                Dim faults = CollectFaults(s)
                Dim eosCount As Integer = 0
                AddHandler s.EosReached, Sub(sender) eosCount += 1

                s.Open(LongVideo())
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                Dim pid = WaitVideoPid(s, 10000)

                ' Let real frames flow first (the corrupt-signature gate faults
                ' only when ZERO complete frames were decoded — we close a
                ' decoder that demonstrably produced output).
                SessionTestHelper.WaitPresentedAtLeast(s, 10, 30000)

                ' PIPE-CLOSE: kill the video decoder without RequestStop — the
                ' stdout pipe closes exactly like a crashed process.
                ProcessControl.KillTree(pid)

                ' Deterministic outcome: decode end + drained queues → EOF path
                ' → Paused. Bounded; Playing forever is the bug this proves gone.
                TestRunner.Assert(s.WaitForEof(10000), "pipe-close → EOF within 10s")
                TestRunner.AssertEqual(PlaybackState.Paused, s.State, "pipe-close → Paused")
                TestRunner.AssertEqual(1, eosCount, "pipe-close raises EosReached once")
                Thread.Sleep(200)
                TestRunner.Assert(Not HasFault(faults, GalleryVideoFaultKind.DecodeStalled),
                                  "pipe-close is an EOF, not a stall fault")
            End Using
        End Sub

        Private Shared Sub Test_Live_SilentDecode()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions(800)) ' fast window for the test
                Dim faults = CollectFaults(s)

                s.Open(LongVideo())
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                Dim pid = WaitVideoPid(s, 10000)

                ' Suspend IMMEDIATELY — the decoder stays alive, pipe open,
                ' producing nothing (worst case a couple of frames raced in).
                ProcessControl.Suspend(pid)
                Try
                    Dim stalled = s.WaitForState(PlaybackState.Paused, 8000)
                    TestRunner.Assert(stalled,
                                      $"silent decode → Paused within 8s (state={s.State}, fault={s.Fault?.ToString()})")
                    TestRunner.Assert(HasFault(faults, GalleryVideoFaultKind.DecodeStalled),
                                      "DecodeStalled fault raised")
                    TestRunner.AssertEqual(1L, s.DecodeStallCount, "exactly one stall transition")
                Finally
                    ProcessControl.Resume(pid)  ' let ffmpeg die cleanly in teardown
                End Try
            End Using
        End Sub

        Private Shared Sub Test_Live_ProcessStall()
            TestMedia.RequireBinaries()
            Using s As New PlaybackSession(NewOptions(800))
                Dim faults = CollectFaults(s)

                s.Open(LongVideo())
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                Dim pid = WaitVideoPid(s, 10000)

                ' Frames demonstrably flowed, THEN the decoder hangs alive.
                SessionTestHelper.WaitPresentedAtLeast(s, 10, 30000)
                ProcessControl.Suspend(pid)
                Try
                    Dim stalled = s.WaitForState(PlaybackState.Paused, 8000)
                    TestRunner.Assert(stalled,
                                      $"mid-play stall → Paused within 8s (state={s.State}, fault={s.Fault?.ToString()})")
                    TestRunner.Assert(HasFault(faults, GalleryVideoFaultKind.DecodeStalled),
                                      "DecodeStalled fault raised")
                    TestRunner.AssertEqual(1L, s.DecodeStallCount, "exactly one stall transition")
                    ' Freeze-frame contract: the last presented frame stays (no black).
                    TestRunner.Assert(s.FramesPresented > 0, "frames were presented before the stall")
                Finally
                    ProcessControl.Resume(pid)
                End Try
            End Using
        End Sub

    End Class

    ''' <summary>Shared helpers live in SessionTests; re-exposed here without
    ''' coupling the modules (Friend Shared, same assembly).</summary>
    Friend Module SessionTestHelper
        Public Sub WaitPresentedAtLeast(s As PlaybackSession, count As Long, timeoutMs As Integer)
            Dim deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            While DateTime.UtcNow < deadline
                If s.FramesPresented >= count Then Return
                Thread.Sleep(20)
            End While
            TestRunner.Assert(False,
                              $"presented {s.FramesPresented} < {count} within {timeoutMs}ms " &
                              $"(state={s.State}, fault={s.Fault?.ToString()})")
        End Sub
    End Module

End Namespace
