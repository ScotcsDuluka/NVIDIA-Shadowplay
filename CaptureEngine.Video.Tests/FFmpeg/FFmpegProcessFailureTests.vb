Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Threading
Imports CaptureEngine.Video.Tests.Fakes

Namespace CaptureEngine.Video.Tests.FFmpeg
    ''' <summary>
    ''' Process failure tests for FFmpeg-subprocess-backed capture backends.
    ''' Validates behavior when the underlying process fails.
    ''' </summary>
    Friend NotInheritable Class FFmpegProcessFailureTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("FFMPEG_PROC: FFmpeg missing → ConfigurationFault", AddressOf Test_FFmpegMissing)
            runner("FFMPEG_PROC: Process exits unexpectedly → ErrorCount incremented", AddressOf Test_ProcessExitUnexpected)
            runner("FFMPEG_PROC: Stop during running process → clean shutdown", AddressOf Test_StopDuringRunningProcess)
        End Sub

        ''' <summary>
        ''' When the FFmpeg binary is not found, Initialize must throw
        ''' VideoBackendConfigurationException and the backend must enter
        ''' Faulted state.
        ''' </summary>
        Private Shared Sub Test_FFmpegMissing()
            Dim backend = New FakeFFmpegBackend().WithFFmpegMissing()
            TestHelpers.AssertThrows(Of VideoBackendConfigurationException)(
                Sub() backend.Initialize(TestHelpers.CreateContext()),
                "FFmpeg missing must throw VideoBackendConfigurationException")
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Faulted,
                backend.CurrentState, "state after FFmpeg missing")
            TestHelpers.Assert(Not backend.ProcessAlive,
                "process should NOT be alive when FFmpeg is missing")
            ' Dispose should still work cleanly from Faulted state.
            backend.Dispose()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Disposed,
                backend.CurrentState, "state after Dispose from Faulted")
        End Sub

        ''' <summary>
        ''' When the FFmpeg process exits unexpectedly during capture,
        ''' the backend must detect the exit, increment ErrorCount,
        ''' and the worker thread must terminate cleanly.
        ''' </summary>
        Private Shared Sub Test_ProcessExitUnexpected()
            Dim backend = New FakeFFmpegBackend()
            backend.WithProcessExitAfterFrames(5)  ' Exit after 5 frames
            backend.WithInterAttemptDelayMs(2)     ' Fast loop for test
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Wait for the process to "exit" and worker to detect it.
            ' Give it up to 2 seconds — should detect in <100ms.
            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() backend.ProcessExitedUnexpectedly, 2000),
                "Process should exit unexpectedly within 2s")

            ' ErrorCount should be incremented.
            TestHelpers.Assert(
                backend.Diagnostics.ErrorCount >= 1,
                "ErrorCount should be >= 1 after unexpected process exit. Was: " &
                backend.Diagnostics.ErrorCount)

            ' Worker should have stopped (no more frames being delivered).
            Dim countAfterExit = sink.RecordedCount
            Thread.Sleep(200)  ' Wait to see if any more frames arrive
            TestHelpers.AssertEqual(
                countAfterExit, sink.RecordedCount,
                "No more frames should arrive after process exit")

            ' Stop should still work cleanly (worker already exited).
            backend.Stop()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Stopped,
                backend.CurrentState, "state after Stop from process-exit")
            backend.Dispose()
        End Sub

        ''' <summary>
        ''' Calling Stop while the worker is actively producing frames
        ''' must result in a clean shutdown:
        ''' - No deadlock
        ''' - Process is killed
        ''' - State transitions to Stopped
        ''' </summary>
        Private Shared Sub Test_StopDuringRunningProcess()
            Dim backend = New FakeFFmpegBackend()
            backend.WithInterAttemptDelayMs(5)  ' Some delay so worker is actively running
            backend.Initialize(TestHelpers.CreateContext())

            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)

            ' Wait for some frames to arrive
            TestHelpers.Assert(
                TestHelpers.SpinWaitFor(Function() sink.RecordedCount >= 3, 2000),
                "Expected at least 3 frames before Stop")

            Dim frameCountBeforeStop = sink.RecordedCount

            ' Stop while worker is active — must complete within budget.
            Dim sw = Stopwatch.StartNew()
            backend.Stop()
            sw.Stop()

            TestHelpers.Assert(
                sw.ElapsedMilliseconds < 2000,
                "Stop during running process must complete < 2s. Took: " &
                sw.ElapsedMilliseconds & " ms")

            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Stopped,
                backend.CurrentState, "state after Stop")

            TestHelpers.Assert(Not backend.ProcessAlive,
                "process should be dead after Stop")

            ' No more frames should arrive after Stop.
            Thread.Sleep(200)
            TestHelpers.AssertEqual(
                frameCountBeforeStop, sink.RecordedCount,
                "No more frames should arrive after Stop")

            backend.Dispose()
        End Sub
    End Class
End Namespace
