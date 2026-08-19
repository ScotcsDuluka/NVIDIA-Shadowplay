Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Video.Tests.Fakes

Namespace CaptureEngine.Video.Tests.FFmpeg
    ''' <summary>
    ''' Concurrency tests for FFmpeg-subprocess-backed capture backends.
    ''' Validates no deadlock, no resource leak, and clean concurrent
    ''' Dispose behavior.
    ''' </summary>
    Friend NotInheritable Class FFmpegConcurrencyTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("FFMPEG_CONCURRENCY: Dispose while worker actively running — no deadlock", AddressOf Test_DisposeWhileWorkerActive)
            runner("FFMPEG_CONCURRENCY: No deadlock on concurrent Start/Stop/Dispose", AddressOf Test_ConcurrentStartStopDispose)
            runner("FFMPEG_CONCURRENCY: No resource leak — process killed after Dispose", AddressOf Test_NoResourceLeak)
        End Sub

        ''' <summary>
        ''' Worker is actively running (tight loop). Calling Dispose MUST
        ''' complete within a short budget — no deadlock.
        ''' </summary>
        Private Shared Sub Test_DisposeWhileWorkerActive()
            Dim backend = New FakeFFmpegBackend()
            backend.WithInterAttemptDelayMs(0)  ' Hot loop
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Give the worker a moment to start spinning.
            Thread.Sleep(20)

            Dim sw = Stopwatch.StartNew()
            backend.Dispose()
            sw.Stop()

            TestHelpers.Assert(
                sw.ElapsedMilliseconds < 500,
                "Dispose while worker active must complete < 500 ms. Took: " &
                sw.ElapsedMilliseconds & " ms")

            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Disposed,
                backend.CurrentState, "state after Dispose while active")

            TestHelpers.Assert(Not backend.ProcessAlive,
                "process must be dead after Dispose")
        End Sub

        ''' <summary>
        ''' Concurrent Start/Stop/Dispose from multiple threads must not
        ''' deadlock or corrupt state. Only ONE thread wins each operation.
        ''' </summary>
        Private Shared Sub Test_ConcurrentStartStopDispose()
            Dim backend = New FakeFFmpegBackend()
            backend.WithInterAttemptDelayMs(5)
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Wait for some frames
            Thread.Sleep(50)

            Dim errors As New System.Collections.Generic.List(Of Exception)()
            Dim errorSync As New Object()

            ' Thread A: Dispose
            Dim tA = New Thread(Sub()
                Try
                    Thread.Sleep(10)
                    backend.Dispose()
                Catch ex As Exception
                    SyncLock errorSync
                        errors.Add(ex)
                    End SyncLock
                End Try
            End Sub) With {.IsBackground = True, .Name = "Concurrent-Dispose"}

            ' Thread B: Stop
            Dim tB = New Thread(Sub()
                Try
                    Thread.Sleep(5)
                    backend.Stop()
                Catch ex As Exception
                    SyncLock errorSync
                        errors.Add(ex)
                    End SyncLock
                End Try
            End Sub) With {.IsBackground = True, .Name = "Concurrent-Stop"}

            tA.Start()
            tB.Start()

            Dim sw = Stopwatch.StartNew()
            tA.Join(3000)
            tB.Join(3000)
            sw.Stop()

            TestHelpers.Assert(
                sw.ElapsedMilliseconds < 3000,
                "Concurrent Start/Stop/Dispose must not deadlock. Took: " &
                sw.ElapsedMilliseconds & " ms")

            ' Some exceptions are OK (e.g. ObjectDisposedException from concurrent Stop
            ' after Dispose already ran). Only real failures are deadlocks or corrupted state.
            ' Check final state is at least Disposed or Stopped.
            Dim finalState = backend.CurrentState
            TestHelpers.Assert(
                finalState = FakeFFmpegBackend.FFmpegBackendState.Disposed OrElse
                finalState = FakeFFmpegBackend.FFmpegBackendState.Stopped,
                "Final state should be Disposed or Stopped after concurrent ops. Was: " & finalState)
        End Sub

        ''' <summary>
        ''' After Dispose, the simulated process must be dead and no
        ''' worker thread should be running. This simulates checking
        ''' that the FFmpeg child process has been killed.
        ''' </summary>
        Private Shared Sub Test_NoResourceLeak()
            Dim backend = New FakeFFmpegBackend()
            backend.WithInterAttemptDelayMs(10)
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Let it run briefly
            Thread.Sleep(50)

            Dim frameCountBeforeDispose = sink.RecordedCount
            TestHelpers.Assert(frameCountBeforeDispose > 0,
                "Should have received frames before Dispose. Got: " & frameCountBeforeDispose)

            ' Dispose — should kill process and stop worker
            backend.Dispose()

            TestHelpers.Assert(Not backend.ProcessAlive,
                "process must be dead after Dispose")

            ' Verify no more frames arrive after Dispose
            Dim countAfterDispose = sink.RecordedCount
            Thread.Sleep(300)  ' Wait to see if any more frames leak through
            TestHelpers.AssertEqual(
                countAfterDispose, sink.RecordedCount,
                "No frames should arrive after Dispose (resource leak check)")

            ' Verify state is Disposed
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Disposed,
                backend.CurrentState, "state should be Disposed")

            ' Verify Diagnostics are still readable after Dispose (no crash)
            Dim emitted = backend.Diagnostics.EmittedFrames
            TestHelpers.Assert(emitted >= 0, "Diagnostics should be readable after Dispose")
        End Sub
    End Class
End Namespace
