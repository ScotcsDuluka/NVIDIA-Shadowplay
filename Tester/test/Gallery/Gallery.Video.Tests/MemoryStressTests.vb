Option Strict On
Option Explicit On
Option Infer On

' MemoryStressTests.vb — W2: memory/queue stress DURING playback (after the
' passthrough + clock-domain fixes). Proves the bounded-memory owner mandate
' under hostile churn: repeated forward/backward seeks, pause/resume, and
' EOF cycling while sampling RAM, GC allocation, queue depth, and drops.
'
' Assertions:
'   MST-1  mixed seek/pause/resume churn (~30s):
'     - video queue NEVER exceeds its capacity (frame-buffer growth = none)
'     - process private memory growth bounded (< 60 MB over the run)
'     - allocation rate stays flat (second half ≤ 3× first half per presented
'       frame — no allocation spike)
'     - zero session faults
'   MST-2  EOF → resume → EOF cycle: bounded memory + no fault.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class MemoryStressTests

        Private Const MaxRamGrowthMb As Double = 60.0
        Private Const MaxAllocRatio As Double = 3.0

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("MEM: mixed seek/pause/resume churn — bounded RAM, bounded queue, flat allocation",
                   AddressOf Test_MixedChurn)
            runner("MEM: EOF → resume → EOF — no growth, no fault",
                   AddressOf Test_EofCycle)
        End Sub

        Private Shared Function PrivMb() As Double
            Using p = Process.GetCurrentProcess()
                Return p.PrivateMemorySize64 / 1048576.0
            End Using
        End Function

        Private Shared Function TotalAllocBytes() As Long
            Return GC.GetTotalAllocatedBytes(precise:=False)
        End Function

        Private Shared Sub Test_MixedChurn()
            TestMedia.RequireBinaries()
            Dim faults As New List(Of GalleryVideoFault)()
            Dim mem0 = PrivMb()
            Dim alloc0 = TotalAllocBytes()

            Using s As New PlaybackSession(SessionTests.NewOptions())
                AddHandler s.FaultRaised,
                Sub(sender, f)
                    SyncLock faults
                        faults.Add(f)
                    End SyncLock
                End Sub
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")
                TestRunner.Assert(s.AudioPositionTicks >= 0, "audio renderer active")

                Dim maxQueue As Integer = 0
                Dim presentedAtHalf As Long = 0
                Dim allocAtHalf As Long = 0
                Dim sw = Stopwatch.StartNew()
                Dim phase As Integer = 0
                Dim seekFwd As Boolean = True

                ' ~30s of churn: alternate forward/backward seek, pause bursts.
                While sw.Elapsed.TotalSeconds < 30.0
                    If seekFwd Then
                        s.Seek(3.0)
                    Else
                        s.Seek(0.5)
                    End If
                    seekFwd = Not seekFwd
                    s.WaitForState(PlaybackState.Playing, 15000)

                    s.Pause()
                    Thread.Sleep(250)
                    s.Play()

                    ' Sample window: queue depth + allocation.
                    Dim deadline = DateTime.UtcNow.AddMilliseconds(900)
                    While DateTime.UtcNow < deadline
                        maxQueue = Math.Max(maxQueue, s.VideoQueueCount)
                        Thread.Sleep(60)
                    End While

                    phase += 1
                    If phase = 12 Then
                        presentedAtHalf = s.FramesPresented
                        allocAtHalf = TotalAllocBytes()
                    End If
                End While

                TestRunner.Assert(maxQueue <= s.VideoQueueCapacity,
                                  $"video queue never exceeded capacity (max {maxQueue} ≤ {s.VideoQueueCapacity})")
                TestRunner.Assert(faults.Count = 0,
                                  $"zero session faults under churn ({String.Join("; ", faults)})")

                Dim mem1 = PrivMb()
                Dim growth = mem1 - mem0
                TestRunner.Assert(growth <= MaxRamGrowthMb,
                                  $"RAM growth {growth:0.#} MB ≤ {MaxRamGrowthMb} MB over the churn window")

                ' Allocation spike check: per-presented allocation in the second
                ' half vs the first half must stay flat (≤ 3×).
                Dim presented1 = s.FramesPresented
                Dim alloc1 = TotalAllocBytes()
                If presentedAtHalf > 100 AndAlso presented1 > presentedAtHalf + 100 Then
                    Dim rateA = (allocAtHalf - alloc0) / CDbl(presentedAtHalf)
                    Dim rateB = (alloc1 - allocAtHalf) / CDbl(presented1 - presentedAtHalf)
                    Dim ratio = If(rateA > 0, rateB / rateA, 0.0)
                    TestRunner.Assert(ratio <= MaxAllocRatio,
                                      $"allocation rate flat (second/first half = {ratio:0.##}× ≤ {MaxAllocRatio}×)")
                Else
                    TestRunner.Assert(s.FramesPresented > 100,
                                      $"enough frames presented to judge allocation ({s.FramesPresented})")
                End If

                TestRunner.Assert(s.DroppedLateFrames >= 0 AndAlso s.EofCount >= 0, "metrics readable")
            End Using

            Thread.Sleep(400)
            TestRunner.Assert(PrivMb() - mem0 <= MaxRamGrowthMb + 10.0,
                              $"RAM growth holds after dispose (+{PrivMb() - mem0:0.#} MB)")
        End Sub

        Private Shared Sub Test_EofCycle()
            TestMedia.RequireBinaries()
            Dim faults As New List(Of GalleryVideoFault)()
            Dim mem0 = PrivMb()

            Using s As New PlaybackSession(SessionTests.NewOptions())
                AddHandler s.FaultRaised,
                Sub(sender, f)
                    SyncLock faults
                        faults.Add(f)
                    End SyncLock
                End Sub
                s.Open(TestMedia.Synthetic("synth_av60.mp4", "av60"))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 20000), "Playing")

                TestRunner.Assert(s.WaitForEof(30000), "first EOF (EosReached)")
                TestRunner.Assert(s.State = PlaybackState.Paused, "EOF → Paused (not a fault)")

                TestRunner.Assert(s.Play().Accepted, "resume past EOF")
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 10000), "Playing again")

                TestRunner.Assert(s.WaitForEof(30000), "second EOF")
                TestRunner.Assert(faults.Count = 0, $"no fault across the EOF cycle ({String.Join("; ", faults)})")
            End Using

            Thread.Sleep(400)
            TestRunner.Assert(PrivMb() - mem0 <= MaxRamGrowthMb,
                              $"EOF cycle RAM growth bounded (+{PrivMb() - mem0:0.#} MB)")
        End Sub

    End Class

End Namespace
