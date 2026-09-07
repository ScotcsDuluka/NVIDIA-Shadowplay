Option Strict On
Option Explicit On
Option Infer On

' F03LegacyTests.vb — F-03: legacy CaptureEngine terminal-state determinism
' around the mux-failure FALLBACK path, plus the fallback-honesty contract.
'
' Triangulated production flow (Engine\Engine\[Capture]\CaptureEngine.vb):
'
'   Start → FFmpeg records → user Stop → 'q' → FFmpeg exits → audio finalized
'     → AwaitOrRunMux:
'         mux exit 0    → temp cleanup → RecordingStopped once → Idle
'         mux exit ≠ 0  → FALLBACK: rename temp video → RecordingStopped once → Idle
'
'   F03-A pins the STOP-path mux failure (G2-C covered the OnExited path):
'   with a video-only temp video the fallback output must be a probe-able
'   MP4 with a video stream and NO audio stream, saved exactly once, Idle.
'
'   F03-B pins the honesty contract the fallback was missing: when the temp
'   video itself is UNPLAYABLE (moov-less truncated MP4 — exactly what a
'   crashed/killed ffmpeg leaves behind), delegating the mux to the REAL
'   ffmpeg makes the mux fail, the fallback renames the garbage file, and
'   the old code announced RecordingStopped for an unplayable file — a
'   false "saved". The regression asserts: NO RecordingStopped, exactly one
'   honesty error, terminal state deterministic, no orphans.
'
' No NVIDIA dependency anywhere: the record path uses the helper-exe seam
' and the mux/probe verdicts come from the REAL bundled ffmpeg via
' delegation (LMHLP_MUX_REAL / LMHLP_PROBE_REAL).

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NVIDIA_Capture
Imports EngineCapture = NVIDIA_Capture.CaptureEngine

Namespace Engine.Concurrency.Tests

    Friend Module F03LegacyTests

        Private _ffmpeg As String = ""
        Private _sandbox As String = ""
        Private _videoOnly As String = ""      ' valid video-only MP4 (fallback source)
        Private _moovless As String = ""       ' truncated MP4 (unplayable — moov gone)

        Friend Sub RunAll(ffmpegPath As String, sandboxDir As String)
            _ffmpeg = ffmpegPath
            _sandbox = sandboxDir
            _videoOnly = IO.Path.Combine(_sandbox, "f03_valid.mp4")
            _moovless = IO.Path.Combine(_sandbox, "f03_moovless.mp4")

            If Not GenerateMedia() Then
                Console.WriteLine("(F03 tests skipped — media generation failed)")
                Return
            End If

            Console.WriteLine("── F03: legacy CaptureEngine mux-fallback determinism & honesty ──")
            TestRunner.RunTest("F03-A: stop-path mux failure → fallback saves a VALID video-only MP4 exactly once",
                               AddressOf Test_StopPathMuxFailureFallback)
            TestRunner.RunTest("F03-B: unplayable temp video → real mux fails → fallback must NOT announce saved",
                               AddressOf Test_FallbackHonesty_UnplayableTemp)
        End Sub

        ' ───────────────────────────────────────────────────────────────

        Private Function GenerateMedia() As Boolean
            ' Valid video-only MP4 (what a successfully-finalized ffmpeg leaves).
            If Not RunGen("-y -f lavfi -i testsrc=duration=1:size=320x240:rate=15 " &
                          "-c:v libx264 -preset ultrafast -pix_fmt yuv420p """ & _videoOnly & """") Then Return False

            ' Unplayable stand-in for a crashed finalize: truncate a normal MP4's
            ' tail — the moov atom (written last) is gone, demux fails at open.
            Dim all As Byte() = IO.File.ReadAllBytes(_videoOnly)
            Dim cut As Integer = CInt(all.Length * 0.6)
            Dim truncated(cut - 1) As Byte
            Buffer.BlockCopy(all, 0, truncated, 0, cut)
            IO.File.WriteAllBytes(_moovless, truncated)

            ' Sanity: the REAL mux must fail on the truncated file and succeed
            ' on the valid one — otherwise this suite's premise is broken.
            Dim muxOnValid As Integer = RealFfmpegExit("-v error -i """ & _videoOnly & """ -c copy -y """ &
                                                       IO.Path.Combine(_sandbox, "f03_probe_ok.mp4") & """")
            Dim muxOnBroken As Integer = RealFfmpegExit("-v error -i """ & _moovless & """ -c copy -y """ &
                                                        IO.Path.Combine(_sandbox, "f03_probe_bad.mp4") & """")
            Return muxOnValid = 0 AndAlso muxOnBroken <> 0
        End Function

        Private Function RunGen(args As String) As Boolean
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg, .Arguments = args,
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True, .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                p.StandardError.ReadToEnd()
                If Not p.WaitForExit(30000) Then
                    Try : p.Kill() : Catch : End Try
                    Return False
                End If
                Return p.ExitCode = 0
            End Using
        End Function

        Private Function RealFfmpegExit(args As String) As Integer
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg, .Arguments = args,
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True, .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                p.StandardError.ReadToEnd()
                If Not p.WaitForExit(30000) Then
                    Try : p.Kill() : Catch : End Try
                    Return -999
                End If
                Return p.ExitCode
            End Using
        End Function

        Private NotInheritable Class Rec
            Public ReadOnly Stopped As New List(Of String)()
            Public ReadOnly Errors As New List(Of String)()

            Public Sub Wire(engine As EngineCapture)
                AddHandler engine.RecordingStopped, Sub(f) LockAdd(Stopped, f)
                AddHandler engine.ErrorOccurred, Sub(m) LockAdd(Errors, m)
            End Sub

            Private Shared Sub LockAdd(list As List(Of String), item As String)
                SyncLock list : list.Add(item) : End SyncLock
            End Sub

            Public Function StoppedCount() As Integer
                SyncLock Stopped : Return Stopped.Count : End SyncLock
            End Function

            Public Function ErrorCount() As Integer
                SyncLock Errors : Return Errors.Count : End SyncLock
            End Function
        End Class

        Private Function MakeSettings(sandbox As String) As CaptureSettings
            Dim s As New CaptureSettings()
            s.FFmpegPath = Environment.ProcessPath   ' helper-exe seam
            s.Encoder = "libx264"
            s.CaptureMethod = "ddagrab"
            s.FPS = 15
            s.Bitrate = 2000000L
            s.UseNativeResolution = True
            s.OutputDirectory = sandbox
            s.SystemAudioCapture = True              ' two-process path (mux/fallback exercised)
            s.MicCapture = False
            Return s
        End Function

        Private Function WaitFor(condition As Func(Of Boolean), timeoutMs As Integer) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.ElapsedMilliseconds < timeoutMs
                If condition() Then Return True
                Thread.Sleep(50)
            End While
            Return condition()
        End Function

        Private Function HelperProcesses() As Integer
            Dim name As String = IO.Path.GetFileNameWithoutExtension(Environment.ProcessPath)
            Dim procs As Process() = Process.GetProcessesByName(name)
            Dim n As Integer = 0
            For Each p As Process In procs
                Try
                    If Not p.HasExited Then n += 1
                Catch
                End Try
                Try : p.Dispose() : Catch : End Try
            Next
            Return n
        End Function

        Private Sub SetEnv(name As String, value As String)
            Environment.SetEnvironmentVariable(name, value)
        End Sub

        Private Sub ClearHelperEnv()
            SetEnv("LMHLP_SRC", Nothing)
            SetEnv("LMHLP_COPYTO", Nothing)
            SetEnv("LMHLP_SLEEP", Nothing)
            SetEnv("LMHLP_EXIT", Nothing)
            SetEnv("LMHLP_MUX_EXIT", Nothing)
            SetEnv("LMHLP_MUX_SRC", Nothing)
            SetEnv("LMHLP_MUX_REAL", Nothing)
            SetEnv("LMHLP_PROBE_REAL", Nothing)
            SetEnv("LMHLP_PROBE_EXIT", Nothing)
        End Sub

        Private Sub SetStandardRecordEnv(tempVideo As String, source As String)
            SetEnv("LMHLP_SLEEP", "30")
            SetEnv("LMHLP_EXIT", "0")
            SetEnv("LMHLP_SRC", source)
            SetEnv("LMHLP_COPYTO", tempVideo)
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Scenarios
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>F03-A: user-stop path where the MUX fails (forced exit 7)
        ''' → fallback renames the VALID video-only temp → saved exactly once,
        ''' output probes as Video-with-NO-Audio, state lands Idle, no orphans.
        ''' (Terminal-state determinism on the STOP path — G2-C covered the
        ''' unexpected-exit path; this is the F-03 triangulation twin.)</summary>
        Private Sub Test_StopPathMuxFailureFallback()
            Dim outPath As String = IO.Path.Combine(_sandbox, "f03_a.mp4")
            Dim tempVideo As String = IO.Path.Combine(_sandbox, "f03_a.video.tmp.mp4")
            ClearHelperEnv()
            SetStandardRecordEnv(tempVideo, _videoOnly)
            SetEnv("LMHLP_MUX_EXIT", "7")            ' forced mux failure on the STOP path

            Dim rec As New Rec()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(WaitFor(Function() IO.File.Exists(tempVideo), 5000),
                                  "helper never produced the temp video")

                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "stop returned False")

                ' Deterministic terminal outcome: fallback saved the temp video.
                TestRunner.Assert(rec.StoppedCount() = 1,
                                  $"expected RecordingStopped exactly once, got {rec.StoppedCount()}")
                Dim saved As String = ""
                SyncLock rec.Stopped : saved = rec.Stopped(0) : End SyncLock
                TestRunner.Assert(saved = outPath, $"saved path {saved} ≠ {outPath}")
                TestRunner.Assert(rec.ErrorCount() = 0,
                                  $"fallback recovery must not emit errors, got: [{String.Join(" | ", rec.Errors)}]")
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"post-fallback state = {engine.State}")

                ' F-02 truthfulness: the fallback output is a REAL video-only MP4.
                MediaAssert.AssertValidMp4(_ffmpeg, outPath, True, False, "F03-A")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1, 8000),
                              "helper process survived F03-A")
        End Sub

        ''' <summary>F03-B: the temp video is UNPLAYABLE (moov-less — what a
        ''' crashed ffmpeg finalize leaves). The mux is delegated to the REAL
        ''' ffmpeg, which fails; the fallback renames the garbage file.
        ''' Honesty contract: the engine must NOT announce RecordingStopped for
        ''' a file that cannot be played back — exactly one honesty error and
        ''' a deterministic failed terminal state instead.
        '
        ' REGRESSION ORDER: this test was written first and FAILED on the old
        ' code (RecordingStopped fired for the garbage file — false "saved"),
        ' then the AwaitOrRunMux fallback gained playback validation.</summary>
        Private Sub Test_FallbackHonesty_UnplayableTemp()
            Dim outPath As String = IO.Path.Combine(_sandbox, "f03_b.mp4")
            Dim tempVideo As String = IO.Path.Combine(_sandbox, "f03_b.video.tmp.mp4")
            ClearHelperEnv()
            SetStandardRecordEnv(tempVideo, _moovless)
            SetEnv("LMHLP_MUX_REAL", _ffmpeg)        ' mux verdict = REAL ffmpeg
            SetEnv("LMHLP_PROBE_REAL", _ffmpeg)      ' validation verdict = REAL ffmpeg

            Dim rec As New Rec()
            Dim engine As New EngineCapture(MakeSettings(_sandbox))
            rec.Wire(engine)
            Try
                Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
                TestRunner.Assert(started, "start returned False")
                TestRunner.Assert(WaitFor(Function() IO.File.Exists(tempVideo), 5000),
                                  "helper never produced the temp video")

                Dim stopOk As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
                TestRunner.Assert(stopOk, "stop returned False")

                ' Honesty: the unplayable fallback output must NOT be announced
                ' as saved. Exactly one terminal error, deterministic state.
                TestRunner.Assert(rec.StoppedCount() = 0,
                                  $"FALSE SAVED: RecordingStopped fired for an unplayable fallback output ({rec.StoppedCount()}×)")
                TestRunner.Assert(rec.ErrorCount() = 1,
                                  $"expected exactly one honesty error, got {rec.ErrorCount()}: [{String.Join(" | ", rec.Errors)}]")
                Dim msg As String = ""
                SyncLock rec.Errors : msg = rec.Errors(0) : End SyncLock
                TestRunner.Assert(msg.Contains("not saved"),
                                  $"terminal error should be the honesty error, got: {msg}")
                ' Terminal-state contract (same as G2-A's honesty path): the
                ' stop flow reports the failure via exactly-one error event and
                ' settles Idle — deterministic before StopRecordingAsync returns.
                TestRunner.Assert(engine.State = EngineCapture.CaptureState.Idle,
                                  $"post-honesty state = {engine.State} (expected Idle per Step-4 contract)")
            Finally
                ClearHelperEnv()
                engine.Dispose()
            End Try
            TestRunner.Assert(WaitFor(Function() HelperProcesses() = 1, 8000),
                              "helper process survived F03-B")
        End Sub

    End Module

End Namespace
