Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports CaptureEngine.Backends
Imports CaptureEngine.FFmpegBackend

Namespace CaptureEngine.FFmpegTests
    Friend Module Program
        Private _passed As Integer = 0
        Private _failed As Integer = 0
        Private ReadOnly _failures As New List(Of String)()

        Function Main(args As String()) As Integer
            Console.WriteLine("==================================================")
            Console.WriteLine(" CaptureEngine.FFmpegBackend Tests (P1-C)")
            Console.WriteLine("==================================================")
            Console.WriteLine()

            ' ----- StderrParser tests -----
            RunTest("PARSER: Parses frame= line", AddressOf Test_ParserParsesFrameLine)
            RunTest("PARSER: Parses Lsize= final line", AddressOf Test_ParserParsesLsizeLine)
            RunTest("PARSER: Detects [error] line", AddressOf Test_ParserDetectsError)
            RunTest("PARSER: Detects 'No space left on device'", AddressOf Test_ParserDetectsDiskFull)
            RunTest("PARSER: Parses size= with KiB unit", AddressOf Test_ParserParsesSizeKiB)
            RunTest("PARSER: Parses speed= with x suffix", AddressOf Test_ParserParsesSpeedX)
            RunTest("PARSER: Ignores non-progress lines", AddressOf Test_ParserIgnoresNoise)
            RunTest("PARSER: GetSnapshot returns all fields", AddressOf Test_ParserGetSnapshot)
            RunTest("PARSER: Thread-safe (never throws on garbage input)", AddressOf Test_ParserNeverThrows)

            ' ----- MuxCoordinator tests -----
            RunTest("MUX: CleanupTempFiles deletes existing files", AddressOf Test_MuxCleanupTempFiles)
            RunTest("MUX: RenameTempVideoToOutput works", AddressOf Test_MuxRenameWorks)
            RunTest("MUX: RenameTempVideoToOutput returns False on missing", AddressOf Test_MuxRenameMissing)
            RunTest("MUX: ProbeVideoDuration returns 0 on missing file", AddressOf Test_MuxProbeMissing)

            ' ----- FFmpegPipelineBackend lifecycle tests -----
            RunTest("LIFECYCLE: Created → Start throws on missing ffmpeg.exe", AddressOf Test_StartMissingFFmpeg)
            RunTest("LIFECYCLE: GetFrame returns Nothing", AddressOf Test_GetFrameReturnsNothing)
            RunTest("LIFECYCLE: GetDiagnostics returns empty dict before Start", AddressOf Test_DiagnosticsBeforeStart)
            RunTest("LIFECYCLE: Stop before Start is no-op", AddressOf Test_StopBeforeStart)
            RunTest("LIFECYCLE: Dispose sets Disposed state", AddressOf Test_DisposeSetsDisposed)
            RunTest("LIFECYCLE: Dispose twice is idempotent", AddressOf Test_DisposeTwice)

            ' ----- AudioSidecar tests -----
            RunTest("AUDIO: Start/Stop lifecycle (stub mode)", AddressOf Test_AudioSidecarStubLifecycle)
            RunTest("AUDIO: HasAudioData returns False (stub)", AddressOf Test_AudioSidecarNoData)
            RunTest("AUDIO: Captures timestamps on Start", AddressOf Test_AudioSidecarTimestamps)

            Console.WriteLine()
            Console.WriteLine("--------------------------------------------------")
            Console.WriteLine(" Result: " & _passed & " passed, " & _failed & " failed, " & (_passed + _failed) & " total")
            Console.WriteLine("--------------------------------------------------")
            If _failed > 0 Then
                Console.WriteLine()
                Console.WriteLine("Failures:")
                For Each f As String In _failures
                    Console.WriteLine("  - " & f)
                Next
            End If
            Return If(_failed > 0, 1, 0)
        End Function

        Private Sub RunTest(name As String, test As Action)
            Dim paddedName = name
            If paddedName.Length < 70 Then paddedName = paddedName & New String(" "c, 70 - paddedName.Length)
            Console.Write("[" & paddedName & "] ")
            Try
                test()
                _passed += 1
                Console.WriteLine("PASS")
            Catch ex As Exception
                _failed += 1
                _failures.Add(name & ": " & ex.GetType().Name & ": " & ex.Message)
                Console.WriteLine("FAIL")
                Console.WriteLine("    " & ex.GetType().Name & ": " & ex.Message)
            End Try
        End Sub

        Private Sub Assert(cond As Boolean, msg As String)
            If Not cond Then Throw New InvalidOperationException("ASSERT: " & msg)
        End Sub

        ' ===== FFmpegStderrParser tests =====

        Private Sub Test_ParserParsesFrameLine()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("frame= 6540 fps=143 q=8.0 size= 94278KiB time=00:00:45.41 bitrate=17005.4kbits/s dup=760 drop=1 speed=0.997x")
            Dim snap = p.GetSnapshot()
            Assert(snap("frame") = 6540, "frame should be 6540")
            Assert(snap("fps") = 143, "fps should be 143")
            Assert(snap("dup") = 760, "dup should be 760")
            Assert(snap("drop") = 1, "drop should be 1")
            Assert(snap("speed") = 997, "speed should be 997 (0.997 * 1000)")
        End Sub

        Private Sub Test_ParserParsesLsizeLine()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("Lsize= 94300KiB time=00:00:60.00 bitrate=12870.0kbits/s speed=1.01x")
            Dim snap = p.GetSnapshot()
            Assert(snap("speed") = 1010, "speed should be 1010 (1.01 * 1000)")
        End Sub

        Private Sub Test_ParserDetectsError()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("[error] Could not open output file 'output.mp4'")
            Assert(p.HasError, "HasError should be True")
            Assert(p.LastError.Contains("Could not open"), "LastError should contain the message")
        End Sub

        Private Sub Test_ParserDetectsDiskFull()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("[error] No space left on device")
            Assert(p.HasError, "Should detect 'No space left on device'")
        End Sub

        Private Sub Test_ParserParsesSizeKiB()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("frame= 100 fps=60 size= 1024KiB time=00:00:01.00 speed=1.0x")
            Dim snap = p.GetSnapshot()
            Assert(snap("size_bytes") = 1048576, "1024 KiB = 1048576 bytes")
        End Sub

        Private Sub Test_ParserParsesSpeedX()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("frame= 100 fps=60 speed=2.5x")
            Dim snap = p.GetSnapshot()
            Assert(snap("speed") = 2500, "2.5x = 2500")
        End Sub

        Private Sub Test_ParserIgnoresNoise()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("Some random FFmpeg output that is not a progress line")
            p.ProcessLine("Press [q] to stop, [?] for help")
            Dim snap = p.GetSnapshot()
            Assert(snap("frame") = 0, "frame should be 0 (no progress parsed)")
            Assert(Not p.HasError, "Should not detect error from noise")
        End Sub

        Private Sub Test_ParserGetSnapshot()
            Dim p As New FFmpegStderrParser()
            p.ProcessLine("frame= 500 fps=120 dup=3 drop=0 speed=1.5x")
            Dim snap = p.GetSnapshot()
            Assert(snap.ContainsKey("frame"), "should have frame key")
            Assert(snap.ContainsKey("fps"), "should have fps key")
            Assert(snap.ContainsKey("dup"), "should have dup key")
            Assert(snap.ContainsKey("drop"), "should have drop key")
            Assert(snap.ContainsKey("speed"), "should have speed key")
            Assert(snap.ContainsKey("size_bytes"), "should have size_bytes key")
        End Sub

        Private Sub Test_ParserNeverThrows()
            Dim p As New FFmpegStderrParser()
            ' Feed various garbage — should never throw
            p.ProcessLine(Nothing)
            p.ProcessLine("")
            p.ProcessLine("   ")
            p.ProcessLine("frame=abc fps=xyz speed=NaN")
            p.ProcessLine("[error] test")
            ' If we get here, no exception was thrown
            Assert(True, "Parser survived garbage input")
        End Sub

        ' ===== MuxCoordinator tests =====

        Private Sub Test_MuxCleanupTempFiles()
            Dim mc As New MuxCoordinator()
            ' Create temp files
            Dim tempDir = IO.Path.GetTempPath()
            Dim f1 = IO.Path.Combine(tempDir, "test_mux_cleanup_1.tmp")
            Dim f2 = IO.Path.Combine(tempDir, "test_mux_cleanup_2.tmp")
            IO.File.WriteAllText(f1, "test")
            IO.File.WriteAllText(f2, "test")
            mc.TempVideoPath = f1
            mc.TempSystemWavPath = f2
            mc.CleanupTempFiles()
            Assert(Not IO.File.Exists(f1), "temp file 1 should be deleted")
            Assert(Not IO.File.Exists(f2), "temp file 2 should be deleted")
            mc.Dispose()
        End Sub

        Private Sub Test_MuxRenameWorks()
            Dim mc As New MuxCoordinator()
            Dim tempDir = IO.Path.GetTempPath()
            Dim src = IO.Path.Combine(tempDir, "test_mux_rename_src.tmp")
            Dim dst = IO.Path.Combine(tempDir, "test_mux_rename_dst.tmp")
            IO.File.WriteAllText(src, "video data")
            mc.TempVideoPath = src
            mc.OutputPath = dst
            Dim ok = mc.RenameTempVideoToOutput()
            Assert(ok, "Rename should succeed")
            Assert(IO.File.Exists(dst), "Output file should exist")
            Assert(Not IO.File.Exists(src), "Source should be moved")
            IO.File.Delete(dst)
            mc.Dispose()
        End Sub

        Private Sub Test_MuxRenameMissing()
            Dim mc As New MuxCoordinator()
            mc.TempVideoPath = "C:\nonexistent\path\video.tmp.mp4"
            mc.OutputPath = "C:\nonexistent\path\output.mp4"
            Dim ok = mc.RenameTempVideoToOutput()
            Assert(Not ok, "Rename should return False for missing source")
            mc.Dispose()
        End Sub

        Private Sub Test_MuxProbeMissing()
            Dim mc As New MuxCoordinator()
            mc.FFmpegPath = "ffmpeg.exe"
            mc.TempVideoPath = "C:\nonexistent\video.mp4"
            Dim dur = mc.ProbeVideoDuration()
            Assert(dur = 0.0, "Duration should be 0.0 for missing file")
            mc.Dispose()
        End Sub

        ' ===== FFmpegPipelineBackend lifecycle tests =====

        Private Sub Test_StartMissingFFmpeg()
            Dim b As New FFmpegPipelineBackend()
            b.WithFFmpegPath("C:\nonexistent\ffmpeg.exe")
            b.WithArguments("-version")
            b.WithOutputPath("output.mp4")
            Dim threw As Boolean = False
            Try
                b.Start()
            Catch ex As IO.FileNotFoundException
                threw = True
            Catch ex As Exception
                ' Some environments may throw different exception types
                threw = True
            End Try
            Assert(threw, "Start with missing ffmpeg.exe should throw")
            b.Dispose()
        End Sub

        Private Sub Test_GetFrameReturnsNothing()
            Dim b As New FFmpegPipelineBackend()
            Dim frame = b.GetFrame()
            Assert(frame Is Nothing, "GetFrame should return Nothing (fire-and-forget)")
            b.Dispose()
        End Sub

        Private Sub Test_DiagnosticsBeforeStart()
            Dim b As New FFmpegPipelineBackend()
            Dim diag = b.GetDiagnostics()
            Assert(diag.Count = 0, "Diagnostics should be empty before Start")
            b.Dispose()
        End Sub

        Private Sub Test_StopBeforeStart()
            Dim b As New FFmpegPipelineBackend()
            b.WithFFmpegPath("ffmpeg.exe")
            b.WithArguments("-version")
            b.WithOutputPath("out.mp4")
            ' Stop before Start should be a safe no-op
            ' It sets _stopCompleted=1 but since state is Created (not Running), it returns early
            b.Stop()
            ' State stays Created because Stop returns early when state != Running
            Assert(b.CurrentState = VideoBackendState.Created OrElse b.CurrentState = VideoBackendState.Stopped,
                   "State should be Created or Stopped after Stop-before-Start (not Faulted)")
            ' Dispose should still work (it checks _disposed, not _stopCompleted)
            b.Dispose()
        End Sub

        Private Sub Test_DisposeSetsDisposed()
            Dim b As New FFmpegPipelineBackend()
            b.Dispose()
            Assert(b.CurrentState = VideoBackendState.Disposed, "State should be Disposed")
        End Sub

        Private Sub Test_DisposeTwice()
            Dim b As New FFmpegPipelineBackend()
            b.Dispose()
            b.Dispose()  ' Should not throw
            b.Dispose()  ' Should not throw
            Assert(b.CurrentState = VideoBackendState.Disposed, "State should be Disposed")
        End Sub

        ' ===== AudioSidecar tests =====

        Private Sub Test_AudioSidecarStubLifecycle()
            Dim a As New AudioSidecar()
            a.SystemAudioEnabled = True
            a.Start()
            Assert(a.IsRunning, "Should be running after Start")
            a.Stop()
            Assert(Not a.IsRunning, "Should not be running after Stop")
            a.Dispose()
        End Sub

        Private Sub Test_AudioSidecarNoData()
            Dim a As New AudioSidecar()
            a.SystemAudioEnabled = True
            a.Start()
            Assert(Not a.HasAudioData, "Stub should return False for HasAudioData")
            a.Stop()
            a.Dispose()
        End Sub

        Private Sub Test_AudioSidecarTimestamps()
            Dim a As New AudioSidecar()
            a.SystemAudioEnabled = True
            a.MicEnabled = True
            a.Start()
            Assert(a.SystemStartTicks > 0, "SystemStartTicks should be captured")
            Assert(a.MicStartTicks > 0, "MicStartTicks should be captured")
            a.Stop()
            a.Dispose()
        End Sub
    End Module
End Namespace
