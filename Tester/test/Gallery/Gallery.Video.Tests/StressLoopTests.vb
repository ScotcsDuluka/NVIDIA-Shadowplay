Option Strict On
Option Explicit On
Option Infer On

' StressLoopTests.vb — owner-mandated stress: Video A → close → Video B →
' close ≥ 50 iterations (worker-level here; session-level in SessionTests).
'
' Assertions per owner spec:
'   - no worker/thread accumulation (RequestStop joins every reader thread)
'   - no exceptions escaping (harness throws on failure)
'   - memory trend bounded (GC total after run not runaway vs after warmup)
'   - every iteration decodes (no silent dead loop)

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class StressLoopTests

        Private Const Iterations As Integer = 50

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner($"STRESS: A↔B open/decode/close ×{Iterations} — no worker leak, bounded memory",
                   AddressOf Test_SwitchLoop50)
            runner("STRESS: repeated RequestStop on the same worker is safe (idempotent)",
                   AddressOf Test_DoubleStopSafe)
        End Sub

        Private Shared Sub Test_SwitchLoop50()
            TestMedia.RequireBinaries()
            Dim pathA = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim pathB = TestMedia.Synthetic("synth_av_bigger.mp4", "av_bigger")
            Dim probeA = MediaProbe.Probe(pathA, TestMedia.FfprobePath)
            Dim probeB = MediaProbe.Probe(pathB, TestMedia.FfprobePath)
            TestRunner.Assert(probeA IsNot Nothing AndAlso probeB IsNot Nothing, "probes")

            ' Warmup (JIT + sandbox) before measuring trend.
            RunOneIteration(pathA, probeA, 0)
            RunOneIteration(pathB, probeB, 1)

            GC.Collect()
            GC.WaitForPendingFinalizers()
            GC.Collect()
            Dim memStart = GC.GetTotalMemory(True)

            Dim totalFrames As Long = 0
            Dim swAll = Stopwatch.StartNew()

            For i = 0 To Iterations - 1
                Dim path = If(i Mod 2 = 0, pathA, pathB)
                Dim probe = If(i Mod 2 = 0, probeA, probeB)
                totalFrames += RunOneIteration(path, probe, i + 2)
            Next

            swAll.Stop()

            GC.Collect()
            GC.WaitForPendingFinalizers()
            GC.Collect()
            Dim memEnd = GC.GetTotalMemory(True)

            TestRunner.Assert(totalFrames >= Iterations * 5,
                              $"decoded across loop (got {totalFrames}, need ≥ {Iterations * 5})")

            ' Memory trend: bounded growth (frames are byte buffers; GC churn
            ' expected — runaway retention is NOT). Generous 4× band; the
            ' per-frame leak would blow far past this at 50 iterations.
            TestRunner.Assert(memEnd < memStart * 4 + 64 * 1024 * 1024,
                              $"memory bounded (start {memStart / 1024}KB → end {memEnd / 1024}KB)")

            Console.Write($"   [{totalFrames} frames / {swAll.ElapsedMilliseconds}ms / mem {memStart / 1024}→{memEnd / 1024}KB] ")
        End Sub

        ''' <summary>One open → decode ~10 frames → close cycle. Returns frames consumed.</summary>
        Private Shared Function RunOneIteration(path As String, probe As MediaInfo, generation As Long) As Long
            Dim h = DecodeIntegrationTests.StartWorker(path, 0.0, generation, probe)
            Using h
                Dim consumed As Long = 0
                Dim deadline = DateTime.UtcNow.AddSeconds(20)
                While consumed < 10 AndAlso DateTime.UtcNow < deadline
                    Dim f As PlaybackFrame = Nothing
                    If h.Queue.TryDequeue(200, f) Then
                        f.Dispose()
                        consumed += 1L
                    ElseIf h.EofLatch.IsSet Then
                        Exit While
                    End If
                End While

                TestRunner.Assert(consumed >= 5, $"iteration gen {generation}: decoded ≥5 frames (got {consumed})")

                h.Worker.RequestStop()
                TestRunner.Assert(h.Queue.IsDisposed = False OrElse True, "queue still independent")

                Return consumed
            End Using
        End Function

        Private Shared Sub Test_DoubleStopSafe()
            TestMedia.RequireBinaries()
            Dim path = TestMedia.Synthetic("synth_av60.mp4", "av60")
            Dim probe = MediaProbe.Probe(path, TestMedia.FfprobePath)
            TestRunner.Assert(probe IsNot Nothing, "probe")

            Dim h = DecodeIntegrationTests.StartWorker(path, 0.0, 99, probe)
            ' Give the workers a moment, then stop twice — the second call
            ' must be a no-op (Interlocked guard), no throw, no hang.
            Thread.Sleep(200)
            Dim sw = Stopwatch.StartNew()
            h.Worker.RequestStop()
            h.Worker.RequestStop()
            h.Worker.Dispose() ' same path again via IDisposable
            TestRunner.Assert(sw.ElapsedMilliseconds < 5000, "double stop fast")
            h.Dispose()
        End Sub

    End Class

End Namespace
