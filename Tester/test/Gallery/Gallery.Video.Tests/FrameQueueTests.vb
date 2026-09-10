Option Strict On
Option Explicit On
Option Infer On

' FrameQueueTests.vb — bounded queue semantics (design doc §3.4).
'
' Every case the owner spec names, with the leak invariant asserted:
'   Enqueued == Dequeued + DroppedStale + DroppedOnFlush + DisposedByClose
' and every emitted frame disposed exactly once (FramesDisposedCount).
' Concurrency cases use REAL threads (producer blocks under backpressure,
' flush wakes it holding a stale frame — the classic seek race).

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class FrameQueueTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("FQ: enqueue/dequeue FIFO + ownership transfer",
                   AddressOf Test_Fifo)
            runner("FQ: full queue blocks producer (backpressure) then wakes on dequeue",
                   AddressOf Test_Backpressure)
            runner("FQ: stale generation rejected at enqueue (never enters)",
                   AddressOf Test_StaleEnqueue)
            runner("FQ: stale generation purged at dequeue (presenter never sees it)",
                   AddressOf Test_StaleDequeue)
            runner("FQ: Flush disposes older generations atomically (seek contract)",
                   AddressOf Test_Flush)
            runner("FQ: Dispose disposes all queued exactly once + wakes waiters",
                   AddressOf Test_DisposeWake)
            runner("FQ: leak invariant under concurrent producer/consumer/flush (stress 200 frames)",
                   AddressOf Test_ConcurrentLeakInvariant)
        End Sub

        ' ---- frame factory ----

        Private Shared Function Frame(pts As Long, generation As Long, seq As Long) As PlaybackFrame
            Dim pixels(16 * 16 * 4 - 1) As Byte ' 16x16 BGRA
            Return New PlaybackFrame(pixels, 16, 16, pts, generation, seq)
        End Function

        ' ---- tests ----

        Private Shared Sub Test_Fifo()
            Using q As New FrameQueue(4)
                Dim f1 = Frame(100, 0, 0)
                Dim f2 = Frame(200, 0, 1)
                Dim f3 = Frame(300, 0, 2)

                TestRunner.Assert(q.TryEnqueue(f1, 1000), "enq1")
                TestRunner.Assert(q.TryEnqueue(f2, 1000), "enq2")
                TestRunner.Assert(q.TryEnqueue(f3, 1000), "enq3")
                TestRunner.AssertEqual(3, q.Count, "count")

                Dim out1 As PlaybackFrame = Nothing
                TestRunner.Assert(q.TryDequeue(1000, out1), "deq1")
                TestRunner.AssertEqual(100L, out1.PtsTicks, "fifo pts1")
                out1.Dispose()

                Dim out2 As PlaybackFrame = Nothing
                TestRunner.Assert(q.TryDequeue(1000, out2), "deq2")
                TestRunner.AssertEqual(200L, out2.PtsTicks, "fifo pts2")
                out2.Dispose()

                Dim out3 As PlaybackFrame = Nothing
                q.TryDequeue(1000, out3)
                TestRunner.AssertEqual(300L, out3.PtsTicks, "fifo pts3")
                out3.Dispose()

                TestRunner.AssertEqual(3L, q.EnqueuedCount, "enqueued")
                TestRunner.AssertEqual(3L, q.DequeuedCount, "dequeued")
            End Using
        End Sub

        Private Shared Sub Test_Backpressure()
            Using q As New FrameQueue(2)
                q.TryEnqueue(Frame(1, 0, 0), 1000)
                q.TryEnqueue(Frame(2, 0, 1), 1000)
                TestRunner.AssertEqual(2, q.Count, "full")

                ' Producer must BLOCK (not fail fast) while full, then succeed
                ' after the consumer drains one slot.
                Dim producerOk As Integer = 0
                Dim producerTask = Task.Run(
                    Sub()
                        If q.TryEnqueue(Frame(3, 0, 2), 5000) Then producerOk = 1
                    End Sub)

                Thread.Sleep(150) ' full longer than a fast-fail would tolerate
                TestRunner.AssertEqual(0, producerOk, "producer still blocked while full")

                Dim drained As PlaybackFrame = Nothing
                TestRunner.Assert(q.TryDequeue(1000, drained), "consumer drains")
                drained.Dispose()

                TestRunner.Assert(producerTask.Wait(5000), "producer finished")
                TestRunner.AssertEqual(1, producerOk, "producer completed after slot freed")
            End Using
        End Sub

        Private Shared Sub Test_StaleEnqueue()
            Using q As New FrameQueue(2)
                q.Flush(5) ' floor = 5 (as if a seek to generation 5 happened)
                Dim stale = Frame(1, 4, 0)
                Dim accepted = q.TryEnqueue(stale, 1000)
                TestRunner.AssertEqual(False, accepted, "stale frame rejected")
                TestRunner.Assert(stale.IsDisposed, "stale frame disposed by queue")
                ' Long literals: metrics are Long — Object.Equals(Integer, Long) is False.
                TestRunner.AssertEqual(1L, q.EnqueuedRejectedCount, "rejected count")
                TestRunner.AssertEqual(0L, q.EnqueuedCount, "never entered")
                TestRunner.AssertEqual(0, q.Count, "queue still empty")
            End Using
        End Sub

        ''' <summary>The REAL dequeue-path purge case: a stale frame can only
        ''' sit BEHIND a current one ([g0, g1, g0] — gen-0 producer raced the
        ''' floor raise). Flush's head-purge stops at g1; the tail g0 must be
        ''' purged by TryDequeue so the presenter NEVER sees it.</summary>
        Private Shared Sub Test_StaleDequeue()
            Using q As New FrameQueue(4)
                q.TryEnqueue(Frame(1, 0, 0), 1000)
                q.TryEnqueue(Frame(2, 1, 0), 1000)
                q.TryEnqueue(Frame(3, 0, 1), 1000) ' g0 AFTER g1 — legal at floor 0

                q.Flush(1) ' head-purge removes g0@head, stops at g1
                TestRunner.AssertEqual(1L, q.DroppedOnFlushCount, "flush purged head g0")
                TestRunner.AssertEqual(2, q.Count, "[g1, g0] remain")

                Dim got As PlaybackFrame = Nothing
                TestRunner.Assert(q.TryDequeue(1000, got), "current generation emits")
                TestRunner.AssertEqual(2L, got.PtsTicks, "fresh pts")
                got.Dispose()

                ' Next dequeue: tail g0 purged HERE — presenter never sees it.
                Dim none As PlaybackFrame = Nothing
                TestRunner.AssertEqual(False, q.TryDequeue(100, none), "stale tail not emitted")
                TestRunner.Assert(none Is Nothing, "no frame surfaced")
                TestRunner.AssertEqual(1L, q.DroppedStaleCount, "stale purged at dequeue path")
            End Using
        End Sub

        Private Shared Sub Test_Flush()
            Using q As New FrameQueue(8)
                For i = 0 To 5
                    q.TryEnqueue(Frame(100 + i, 0, i), 1000)
                Next
                TestRunner.AssertEqual(6, q.Count, "pre-flush count")

                q.Flush(1) ' seek: everything of generation 0 is garbage now

                TestRunner.AssertEqual(0, q.Count, "flush emptied")
                TestRunner.AssertEqual(6L, q.DroppedOnFlushCount, "flush disposed 6")
                TestRunner.AssertEqual(6L, q.FramesDisposedCount, "frames disposed exactly once")
                TestRunner.AssertEqual(1L, q.GenerationFloor, "floor raised")
            End Using
        End Sub

        ''' <summary>Close (a) disposes every queued frame exactly once and
        ''' (b) wakes a consumer PARKED on the empty queue with False — no hang.</summary>
        Private Shared Sub Test_DisposeWake()
            ' (a) queued frame disposal on close
            Using q As New FrameQueue(2)
                q.TryEnqueue(Frame(1, 0, 0), 1000)
                q.Dispose()
                TestRunner.AssertEqual(1L, q.DisposedByCloseCount, "queued frame disposed on close")
                TestRunner.AssertEqual(1L, q.FramesDisposedCount, "exactly once")

                Dim f2 As PlaybackFrame = Nothing
                TestRunner.AssertEqual(False, q.TryDequeue(500, f2), "dequeue on closed = False")
            End Using

            ' (b) blocked consumer wakes on close
            Using q As New FrameQueue(2)
                Dim consumerTask = Task.Run(
                    Function()
                        Dim f As PlaybackFrame = Nothing
                        Return q.TryDequeue(10000, f)
                    End Function)
                Thread.Sleep(150) ' consumer now parked on the EMPTY queue
                q.Dispose()
                TestRunner.Assert(consumerTask.Wait(5000), "consumer woke on close")
                TestRunner.AssertEqual(False, consumerTask.Result, "parked dequeue returns False on close")
            End Using
        End Sub

        Private Shared Sub Test_ConcurrentLeakInvariant()
            Using q As New FrameQueue(3)
                Dim stopFlag As Integer = 0
                Dim gate As New ManualResetEventSlim(True) ' open — producer runs

                ' Producer: gens cycle 0,1,2 forever. Whenever the floor is
                ' raised while it runs, the next gen-0/gen-1 frames are
                ' DETERMINISTICALLY rejected at enqueue (≤3 frames to hit one).
                Dim producer = Task.Run(
                    Sub()
                        Dim seq = 0L
                        Dim i = 0
                        While Volatile.Read(stopFlag) = 0
                            gate.Wait(5000)
                            If Volatile.Read(stopFlag) <> 0 Then Exit While
                            Dim g = CLng(i Mod 3)
                            q.TryEnqueue(Frame(i, g, seq), 50)
                            seq += 1L
                            i += 1
                            Thread.Sleep(1)
                        End While
                    End Sub)

                ' Consumer: drains what it can.
                Dim consumer = Task.Run(
                    Sub()
                        While Volatile.Read(stopFlag) = 0 OrElse q.Count > 0
                            Dim f As PlaybackFrame = Nothing
                            If q.TryDequeue(20, f) Then
                                f.Dispose()
                            End If
                        End While
                    End Sub)

                ' Two seek-flushes, each while the producer is PARKED at the
                ' gate, then a guaranteed post-floor run of frames.
                Thread.Sleep(40)
                gate.Reset() : Thread.Sleep(40)
                q.Flush(1)
                gate.Set() : Thread.Sleep(60)     ' producer pushes past floor 1 → gen-0 rejected

                gate.Reset() : Thread.Sleep(40)
                q.Flush(2)
                gate.Set() : Thread.Sleep(60)     ' producer pushes past floor 2 → gen-0/1 rejected

                Volatile.Write(stopFlag, 1)
                gate.Set()
                TestRunner.Assert(producer.Wait(10000), "producer exits")
                TestRunner.Assert(consumer.Wait(10000), "consumer exits")

                ' Drain whatever remains, then close.
                While True
                    Dim f As PlaybackFrame = Nothing
                    If Not q.TryDequeue(50, f) Then Exit While
                    f.Dispose()
                End While
                q.Dispose()

                ' THE INVARIANT (owner rule: no unbounded growth, no leak):
                TestRunner.AssertEqual(q.EnqueuedCount,
                                       q.DequeuedCount + q.DroppedStaleCount + q.DroppedOnFlushCount + q.DisposedByCloseCount,
                                       "leak invariant")
                TestRunner.Assert(q.DequeuedCount > 0, "consumer actually consumed")
                TestRunner.Assert(q.EnqueuedRejectedCount > 0,
                                  "post-floor frames rejected at enqueue (stale never enter)")
            End Using
        End Sub

    End Class

End Namespace
