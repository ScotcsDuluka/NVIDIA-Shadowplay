Option Strict On
Option Explicit On
Option Infer On

' Program.vb — Phase 12a-5 Console Test
'
' Tests RecordingEngine orchestration:
'   Session 1: 10s recording → MP4 with video + audio
'   Session 2: 10s recording → MP4 with video + audio (same engine, reused backends)
'
' Usage:
'   dotnet run -c Release -- --ffmpeg "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
'
' IMPORTANT: Play audio on your system during each session!

Imports System.IO
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Recording

Module Program

    Public Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8
        Console.WriteLine("============================================================")
        Console.WriteLine(" Phase 12a-5 — RecordingEngine Orchestration Test")
        Console.WriteLine(" 2 sessions × 10s → MP4 with video + audio")
        Console.WriteLine("============================================================")
        Console.WriteLine()

        ' Parse --ffmpeg arg
        Dim ffmpegPath As String = "ffmpeg"
        For i As Integer = 0 To args.Length - 2
            If args(i) = "--ffmpeg" Then ffmpegPath = args(i + 1)
        Next

        ' Try to find ffmpeg in API-Core
        If ffmpegPath = "ffmpeg" Then
            Dim candidates As String() = {
                Path.Combine(AppContext.BaseDirectory, "API-Core", "ffmpeg.exe"),
                Path.Combine(AppContext.BaseDirectory, "..", "API-Core", "ffmpeg.exe"),
                "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
            }
            For Each c In candidates
                If File.Exists(c) Then ffmpegPath = c : Exit For
            Next
        End If
        Console.WriteLine($"  FFmpeg: {ffmpegPath}")
        Console.WriteLine()

        Console.WriteLine(">>> IMPORTANT: PLAY AUDIO on your system during each session!")
        Console.WriteLine()

        Dim logger As New EngineLogger("Test", EngineLogger.LogLevel.Info, AddressOf Console.WriteLine)
        Dim overallOk As Boolean = True

        ' ─── Create RecordingEngine ──────────────────────────────────
        Console.WriteLine(">>> Creating RecordingEngine...")
        Dim engine As New RecordingEngine(logger)

        Try
            engine.Initialize()
            Console.WriteLine(">>> RecordingEngine initialized (Idle)")
            Console.WriteLine()

            ' ─── Session 1 ────────────────────────────────────────────
            Console.WriteLine(">>> Session 1: 10s recording → session1.mp4")
            Console.WriteLine("    >>> PLAY AUDIO NOW! <<<")
            Console.WriteLine()
            Dim config1 As New SessionConfig() With {
                .OutputPath = "session1.mp4",
                .DurationSeconds = 10,
                .FFmpegPath = ffmpegPath
            }
            Dim result1 As SessionResult = engine.StartSession(config1)
            PrintSessionResult("Session 1", result1)
            overallOk = overallOk AndAlso result1.Pass
            Console.WriteLine()

            ' ─── Session 2 (same engine — backends reused) ────────────
            Console.WriteLine(">>> Session 2: 10s recording → session2.mp4 (same engine)")
            Console.WriteLine("    >>> PLAY AUDIO NOW! <<<")
            Console.WriteLine()
            Dim config2 As New SessionConfig() With {
                .OutputPath = "session2.mp4",
                .DurationSeconds = 10,
                .FFmpegPath = ffmpegPath
            }
            Dim result2 As SessionResult = engine.StartSession(config2)
            PrintSessionResult("Session 2", result2)
            overallOk = overallOk AndAlso result2.Pass
            Console.WriteLine()

        Catch ex As Exception
            Console.Error.WriteLine($"*** FATAL: {ex.Message}")
            Console.Error.WriteLine(ex.StackTrace)
            overallOk = False
        Finally
            engine.Dispose()
        End Try

        ' ─── Verdict ────────────────────────────────────────────────
        Console.WriteLine("============================================================")
        Console.WriteLine(" PHASE 12a-5 VERDICT")
        Console.WriteLine("============================================================")
        Console.WriteLine($"  Session 1: {If(result1.Pass, "PASS", "FAIL")}")
        Console.WriteLine($"  Session 2: {If(result2.Pass, "PASS", "FAIL")}")
        Console.WriteLine($"  Resource reuse: {If(overallOk, "PROVEN", "UNPROVEN")}")
        Console.WriteLine($"  MP4 with video+audio: {If(overallOk, "PROVEN", "UNPROVEN")}")
        Console.WriteLine()
        Console.WriteLine($"  OVERALL: {If(overallOk, "PASS", "FAIL")}")
        Console.WriteLine("============================================================")

        Return If(overallOk, 0, 1)
    End Function

    Private Sub PrintSessionResult(label As String, r As SessionResult)
        Console.WriteLine($"────────────────────────────────────────────────────────────")
        Console.WriteLine($" {label} — Result")
        Console.WriteLine($"────────────────────────────────────────────────────────────")
        Console.WriteLine($"  output:               {r.OutputPath}")
        Console.WriteLine($"  duration:              {r.ActualDurationSec:F2}s (target {r.RequestedDurationSec}s)")
        Console.WriteLine($"  frames_captured:      {r.FramesCaptured}")
        Console.WriteLine($"  frames_encoded:       {r.FramesEncoded}")
        Console.WriteLine($"  nvenc_errors:          {r.NvencErrors}")
        Console.WriteLine($"  video_bytes:          {r.TotalVideoBytes:N0}")
        Console.WriteLine($"  audio_samples:        {r.AudioSamples:N0}")
        Console.WriteLine($"  audio_bytes:          {r.AudioBytes:N0}")
        Console.WriteLine($"  video_stream:         {If(r.VideoStreamFound, "FOUND", "MISSING")}")
        Console.WriteLine($"  audio_stream:         {If(r.AudioStreamFound, "FOUND", "MISSING")}")
        Console.WriteLine($"  file_exists:          {r.FileExists}")
        Console.WriteLine($"  file_size:            {r.FileSize:N0} bytes ({r.FileSize / 1024.0 / 1024.0:F2} MB)")
        If Not String.IsNullOrEmpty(r.ErrorMessage) Then
            Console.WriteLine($"  error:                {r.ErrorMessage}")
        End If
        Console.WriteLine($"  pass:                  {r.Pass}")
        Console.WriteLine($"────────────────────────────────────────────────────────────")
    End Sub

End Module
