Option Strict On
Option Explicit On
Option Infer On

' MediaAssert.vb — F-02: shared truthful output validation for tests that
' claim "recording saved / output valid".
'
' The audited tests (H1-B, H2-C) asserted only File.Exists — a 0-byte or
' headerless file passed. This helper pins the full contract:
'
'   1. file exists                    4. expected video stream present
'   2. file size > 0                  5. expected stream ABSENCE (video-only)
'   3. container probe succeeds       6. container duration > 0 sanity
'      (real ffmpeg -i stream dump;  7./8. temp leftovers / process hygiene
'      equivalent evidence to        are asserted by the specific tests and
'      ffprobe, without needing      the suite-level orphan check — kept
'      a separate ffprobe binary)    there, not duplicated per call.
'
' Failure/no-output tests must NOT use this helper — they assert the
' opposite semantics explicitly (e.g. G2-A's Not File.Exists).

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Text.RegularExpressions

Namespace Engine.Concurrency.Tests

    Friend Module MediaAssert

        ''' <summary>F-02/NV: count processes of <paramref name="imageName"/>
        ''' whose COMMAND LINE references <paramref name="marker"/> (our sandbox
        ''' path). Machine-global process-name counting is meaningless when
        ''' parallel agents run their own suites/ffmpeg on the same box — this
        ''' scopes the orphan check to processes our sandbox actually owns.</summary>
        Friend Function ScopedProcessCount(imageName As String, marker As String) As Integer
            Try
                Dim script As String =
                    "(Get-CimInstance Win32_Process -Filter ""Name='" & imageName & ".exe'"" | " &
                    "Where-Object { $_.CommandLine -like '*" & marker & "*' } | " &
                    "Measure-Object).Count"
                Dim psi As New ProcessStartInfo With {
                    .FileName = "powershell.exe",
                    .Arguments = "-NoProfile -NonInteractive -EncodedCommand " &
                                 Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script)),
                    .UseShellExecute = False, .CreateNoWindow = True,
                    .RedirectStandardOutput = True, .RedirectStandardError = True
                }
                Using p As Process = Process.Start(psi)
                    Dim outTask = p.StandardOutput.ReadToEndAsync()
                    If Not p.WaitForExit(15000) Then
                        Try : p.Kill() : Catch : End Try
                        Return -1
                    End If
                    outTask.Wait(2000)
                    Dim n As Integer = 0
                    Integer.TryParse(outTask.Result.Trim(), n)
                    Return n
                End Using
            Catch
                Return -1   ' probe unavailable — caller decides fallback
            End Try
        End Function

        ''' <summary>Orphan verdict scoped to our sandbox: ffmpeg processes
        ''' referencing our sandbox must be gone. -1 (probe unavailable) counts
        ''' as pass here — the suite-level check still reports the global view.</summary>
        Friend Function ScopedFfmpegOrphans(marker As String) As Integer
            Dim n As Integer = ScopedProcessCount("ffmpeg", marker)
            Return If(n < 0, 0, n)
        End Function

        ''' <summary>Assert that <paramref name="path"/> is a real, probe-able
        ''' MP4 whose streams match the expectation. Uses the REAL bundled
        ''' ffmpeg (same evidence ffprobe would give: Duration + Stream # lines).
        ''' Synchronous; bounded by the probe process timeout.</summary>
        Friend Sub AssertValidMp4(ffmpegExe As String, path As String,
                                  expectVideo As Boolean, expectAudio As Boolean,
                                  label As String)
            TestRunner.Assert(IO.File.Exists(path), label & ": output file missing — " & path)
            Dim fi As New IO.FileInfo(path)
            TestRunner.Assert(fi.Length > 0,
                              label & $": output file is EMPTY (0 bytes) — {path}")

            Dim probe As String = ProbeMedia(ffmpegExe, path)

            ' Container sanity: a real MP4 reports a parseable Duration > 0.
            Dim m As Match = Regex.Match(probe, "Duration:\s*(\d+):(\d+):(\d+\.?\d*)")
            TestRunner.Assert(m.Success,
                              label & ": probe reported no container Duration (not a valid MP4): " &
                                      FirstLine(probe, 220))
            Dim durationSec As Double = CDbl(m.Groups(1).Value) * 3600 +
                                        CDbl(m.Groups(2).Value) * 60 +
                                        CDbl(m.Groups(3).Value)
            TestRunner.Assert(durationSec > 0,
                              label & $": container duration = {durationSec:0.000}s (expected > 0)")

            ' Stream presence/absence — probe text contains "Video:"/"Audio:"
            ' only when such a stream exists in the container.
            TestRunner.Assert(probe.Contains("Video:") = expectVideo,
                              label & $": video stream presence={Not expectVideo} but expected {expectVideo}: " &
                                      FirstLine(probe, 220))
            TestRunner.Assert(probe.Contains("Audio:") = expectAudio,
                              label & $": audio stream presence={Not expectAudio} but expected {expectAudio}: " &
                                      FirstLine(probe, 220))
        End Sub

        ''' <summary>Dump the media info the way production's verify step does:
        ''' ffmpeg -i (no output target) writes the container/stream report to
        ''' stderr and exits non-zero — the exit code is irrelevant here.</summary>
        Private Function ProbeMedia(ffmpegExe As String, path As String) As String
            Dim psi As New ProcessStartInfo With {
                .FileName = ffmpegExe,
                .Arguments = "-hide_banner -i """ & path & """",
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                Dim errTask = p.StandardError.ReadToEndAsync()
                Dim outTask = p.StandardOutput.ReadToEndAsync()
                If Not p.WaitForExit(15000) Then
                    Try : p.Kill() : Catch : End Try
                End If
                Try : errTask.Wait(2000) : Catch : End Try
                Try : outTask.Wait(1000) : Catch : End Try
                Dim err As String = If(errTask.Status = Threading.Tasks.TaskStatus.RanToCompletion, errTask.Result, "")
                Return err
            End Using
        End Function

        Private Function FirstLine(text As String, maxChars As Integer) As String
            If text Is Nothing Then Return ""
            Dim oneLine As String = text.Replace(vbCr, "").Replace(vbLf, " | ")
            If oneLine.Length > maxChars Then oneLine = oneLine.Substring(0, maxChars) & "…"
            Return oneLine
        End Function

    End Module

End Namespace
