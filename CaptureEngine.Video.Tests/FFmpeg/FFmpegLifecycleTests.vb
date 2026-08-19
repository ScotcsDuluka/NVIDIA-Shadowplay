Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Video.Tests.Fakes

Namespace CaptureEngine.Video.Tests.FFmpeg
    ''' <summary>
    ''' Lifecycle tests for FFmpeg-subprocess-backed capture backends.
    ''' Mirrors the P1-B.1 BackendLifecycleTests pattern.
    '''
    ''' Uses FakeFFmpegBackend as a test double that simulates the
    ''' process lifecycle of a real FFmpegBackend.
    ''' </summary>
    Friend NotInheritable Class FFmpegLifecycleTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("FFMPEG_LIFECYCLE: Initialize -> Initialized", AddressOf Test_Initialize)
            runner("FFMPEG_LIFECYCLE: Start before Initialize (negative)", AddressOf Test_StartBeforeInitialize)
            runner("FFMPEG_LIFECYCLE: Start twice (idempotent)", AddressOf Test_StartTwice)
            runner("FFMPEG_LIFECYCLE: Stop before Start (negative)", AddressOf Test_StopBeforeStart)
            runner("FFMPEG_LIFECYCLE: Stop twice (idempotent)", AddressOf Test_StopTwice)
            runner("FFMPEG_LIFECYCLE: Dispose while Running (stop-path invoked)", AddressOf Test_DisposeWhileRunning)
            runner("FFMPEG_LIFECYCLE: Dispose twice (idempotent)", AddressOf Test_DisposeTwice)
            runner("FFMPEG_LIFECYCLE: Start after Dispose (ObjectDisposedException)", AddressOf Test_StartAfterDispose)
        End Sub

        Private Shared Sub Test_Initialize()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Initialized,
                backend.CurrentState, "state after Initialize")
            TestHelpers.Assert(backend.ProcessAlive,
                "simulated process should be alive after Initialize")
            backend.Dispose()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Disposed,
                backend.CurrentState, "state after Dispose")
        End Sub

        Private Shared Sub Test_StartBeforeInitialize()
            Dim backend = New FakeFFmpegBackend()
            TestHelpers.AssertThrows(Of InvalidOperationException)(
                Sub() backend.Start(New RecordingVideoFrameSink()),
                "Start before Initialize must throw InvalidOperationException")
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Created,
                backend.CurrentState, "state after failed Start")
            backend.Dispose()
        End Sub

        Private Shared Sub Test_StartTwice()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Running,
                backend.CurrentState, "state after first Start")
            ' Second Start: idempotent, no-op, no exception.
            backend.Start(sink)
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Running,
                backend.CurrentState, "state after second Start (idempotent)")
            backend.Stop()
            backend.Dispose()
        End Sub

        Private Shared Sub Test_StopBeforeStart()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            ' Stop before Start: no-op, NO exception.
            backend.Stop()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Initialized,
                backend.CurrentState, "state after Stop before Start (no-op)")
            ' Start should still work after the no-op Stop.
            backend.Start(New RecordingVideoFrameSink())
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Running,
                backend.CurrentState, "state after Start")
            backend.Stop()
            backend.Dispose()
        End Sub

        Private Shared Sub Test_StopTwice()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            backend.Start(New RecordingVideoFrameSink())
            backend.Stop()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Stopped,
                backend.CurrentState, "state after first Stop")
            ' Second Stop: no-op, NO exception.
            backend.Stop()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Stopped,
                backend.CurrentState, "state after second Stop (idempotent)")
            backend.Dispose()
        End Sub

        Private Shared Sub Test_DisposeWhileRunning()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            Dim sink As New RecordingVideoFrameSink()
            backend.Start(sink)
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Running,
                backend.CurrentState, "state after Start")

            ' Dispose while Running: routes through stop path internally.
            backend.Dispose()
            TestHelpers.AssertEqual(
                FakeFFmpegBackend.FFmpegBackendState.Disposed,
                backend.CurrentState, "state after Dispose while Running")
            TestHelpers.Assert(Not backend.ProcessAlive,
                "simulated process should be dead after Dispose while Running")
        End Sub

        Private Shared Sub Test_DisposeTwice()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            backend.Start(New RecordingVideoFrameSink())
            backend.Stop()
            backend.Dispose()
            ' Second Dispose: no-op, NO exception.
            backend.Dispose()
            ' Third too.
            backend.Dispose()
        End Sub

        Private Shared Sub Test_StartAfterDispose()
            Dim backend = New FakeFFmpegBackend()
            backend.Initialize(TestHelpers.CreateContext())
            backend.Dispose()
            TestHelpers.AssertThrows(Of ObjectDisposedException)(
                Sub() backend.Start(New RecordingVideoFrameSink()),
                "Start after Dispose must throw ObjectDisposedException")
        End Sub
    End Class
End Namespace
