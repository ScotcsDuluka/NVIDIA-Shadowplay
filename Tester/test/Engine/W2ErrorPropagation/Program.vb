Option Strict On
Option Explicit On
Option Infer On

' Program.vb — M2-W2 False-Success / Error-Propagation audit repro (W2)
'
' Reproduces the false-success contract gaps found by the audit, using the
' REAL production classes and the EXACT production invocations — on any
' machine (no capture/encoder hardware needed):
'
'   CASE-A (control)  session verify predicate on a healthy MP4 → True
'   CASE-B (GAP-2)    session verify predicate on a TRUNCATED MP4 → True
'                     while a full decode (-xerror) exits nonzero
'                     (CaptureSession.vb:1196-1226: VideoStreamFound is
'                     stderr header-presence only; verify exit code unused)
'   CASE-C (GAP-1)    REAL LiveMuxSession.Stop with a remux ffmpeg that
'                     exits 1 → salvage move → Succeeded=True
'                     (LiveMuxSession.vb:387-406: remux failure is demoted
'                     to an ErrorMessage string nothing downstream reads)
'   CASE-D (control)  REAL LiveMuxSession.Stop with mux ffmpeg exit 1 →
'                     Succeeded=False, no file (fail-closed works there)
'   CASE-E (synthesis) real SessionResult.Pass over a salvage-produced
'                     session shape → Pass=True (RecordingDTOs.vb:303-322)
'
' The stub ffmpeg is a .cmd: mux invocation copies a real fragmented MP4 to
' the frag path and exits 0; the remux invocation (args contain "+faststart")
' exits 1 — isolating exactly the salvage decision under test.
'
' NOTHING in production is modified. Artifacts + report land in
' evidence\w2-error-propagation\ (gitignored).

Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports CaptureEngine.FFmpegBackend
Imports CaptureEngine.Recording

Module Program

    Private _logLines As New List(Of String)()

    Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8

        Dim ffmpegPath As String = ""
        Dim outDir As String = ""

        For i As Integer = 0 To args.Length - 2
            If args(i) = "--ffmpeg" Then ffmpegPath = args(i + 1)
            If args(i) = "--out" Then outDir = args(i + 1)
        Next

        If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
            ffmpegPath = "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
        End If
        If Not File.Exists(ffmpegPath) Then
            Console.Error.WriteLine("FATAL: ffmpeg.exe not found — pass --ffmpeg <path>")
            Return 2
        End If
        If String.IsNullOrEmpty(outDir) Then
            outDir = "C:\My Project\NVIDIA-Shadowplay\evidence\w2-error-propagation"
        End If
        Directory.CreateDirectory(outDir)

        Log("")
        Log("════════ W2 False-Success / Error-Propagation repro ════════")
        Log("ffmpeg: " & ffmpegPath)
        Log("out:    " & outDir)
        Log("")

        Dim verdicts As New Dictionary(Of String, String)()

        ' ─── Fixtures ─────────────────────────────────────────────────
        Dim healthy = Path.Combine(outDir, "fixture-healthy.mp4")
        Dim fragOk = Path.Combine(outDir, "fixture-frag.mp4")
        Dim truncated = Path.Combine(outDir, "fixture-truncated.mp4")

        Dim mk1 = RunProcess(ffmpegPath,
            $"-y -hide_banner -loglevel error -f lavfi -i testsrc=duration=1:size=320x240:rate=30 " &
            $"-c:v libx264 -pix_fmt yuv420p -movflags +faststart ""{healthy}""", 30000)
        If mk1.ExitCode <> 0 OrElse Not File.Exists(healthy) Then
            Log("FATAL: could not build healthy fixture: " & mk1.StderrTail)
            Return 2
        End If
        Dim mk2 = RunProcess(ffmpegPath,
            $"-y -hide_banner -loglevel error -f lavfi -i testsrc=duration=1:size=320x240:rate=30 " &
            $"-c:v libx264 -pix_fmt yuv420p -movflags +frag_keyframe+empty_moov+default_base_moof ""{fragOk}""", 30000)
        If mk2.ExitCode <> 0 OrElse Not File.Exists(fragOk) Then
            Log("FATAL: could not build fragmented fixture: " & mk2.StderrTail)
            Return 2
        End If
        ' Truncate 55% in — faststart puts moov up front, so the container
        ' header parses (stream list prints) while the mdat payload is cut.
        Dim allBytes = File.ReadAllBytes(healthy)
        Dim keep As Integer = CInt(allBytes.Length * 0.55R)
        Dim cut(keep - 1) As Byte
        Array.Copy(allBytes, cut, keep)
        File.WriteAllBytes(truncated, cut)
        Log($"fixtures: healthy={New FileInfo(healthy).Length}B frag={New FileInfo(fragOk).Length}B truncated={New FileInfo(truncated).Length}B")
        Log("")

        ' ─── CASE-A / CASE-B — session verify predicate vs full decode ──
        Dim decHealthy = FullDecode(ffmpegPath, healthy)
        Dim predHealthy = SessionVerifyPredicate(ffmpegPath, healthy)
        Dim caseAok = predHealthy.VideoStreamFound AndAlso decHealthy.ExitCode = 0
        verdicts("CASE-A") = If(caseAok, "CONTROL-OK", "UNEXPECTED")
        Log($"[CASE-A control] healthy.mp4: VideoStreamFound={predHealthy.VideoStreamFound} fullDecodeExit={decHealthy.ExitCode} → {verdicts("CASE-A")}")

        Dim decTrunc = FullDecode(ffmpegPath, truncated)
        Dim predTrunc = SessionVerifyPredicate(ffmpegPath, truncated)
        Dim caseBok = predTrunc.VideoStreamFound AndAlso decTrunc.ExitCode <> 0
        verdicts("CASE-B") = If(caseBok, "GAP-REPRODUCED", "NOT-REPRODUCED")
        Log($"[CASE-B GAP-2]  truncated.mp4: VideoStreamFound={predTrunc.VideoStreamFound} fullDecodeExit={decTrunc.ExitCode} decodeErrors=""{Trim(decTrunc.StderrTail, 120)}""")
        Log($"                 → session-level verify says stream found; full decode fails → {verdicts("CASE-B")}")
        Log("")

        ' ─── CASE-C / CASE-D — REAL LiveMuxSession salvage + fail-closed ─
        Dim stubRemuxFail = WriteStubFfmpeg(Path.Combine(outDir, "stub-remuxfail.cmd"), fragOk, True)
        Dim stubAlwaysFail = WriteStubFfmpeg(Path.Combine(outDir, "stub-alwaysexit1.cmd"), fragOk, False)

        Dim finalC = Path.Combine(outDir, "casec-final.mp4")
        Dim resC = DriveLiveMux(stubRemuxFail, finalC)
        Dim finalCExists = File.Exists(finalC)
        Dim predC = If(finalCExists, SessionVerifyPredicate(ffmpegPath, finalC), New VerifyOutcome(False, 0, ""))
        Dim caseCok = resC.Succeeded AndAlso Not resC.UsedFaststartRemux AndAlso
                      resC.ErrorMessage.Length > 0 AndAlso finalCExists AndAlso predC.VideoStreamFound
        verdicts("CASE-C") = If(caseCok, "GAP-REPRODUCED", "NOT-REPRODUCED")
        Log($"[CASE-C GAP-1]  LiveMuxSession(stub remux exit=1): ok={resC.Succeeded} exit={resC.FFmpegExitCode} faststart={resC.UsedFaststartRemux} finalExists={finalCExists}")
        Log($"                 LiveMuxResult.ErrorMessage=""{resC.ErrorMessage}""   ← CaptureSession never maps this field")
        Log($"                 final file passes session verify: VideoStreamFound={predC.VideoStreamFound} → {verdicts("CASE-C")}")

        Dim finalD = Path.Combine(outDir, "cased-final.mp4")
        Dim resD = DriveLiveMux(stubAlwaysFail, finalD)
        Dim caseDok = (Not resD.Succeeded) AndAlso Not File.Exists(finalD)
        verdicts("CASE-D") = If(caseDok, "CONTROL-OK", "UNEXPECTED")
        Log($"[CASE-D control] LiveMuxSession(stub mux exit=1): ok={resD.Succeeded} exit={resD.FFmpegExitCode} finalExists={File.Exists(finalD)} → {verdicts("CASE-D")}")
        Log("")

        ' ─── CASE-E — real SessionResult.Pass over the salvage shape ────
        ' Fields replicate a video-only session whose live-mux was salvaged
        ' (CASE-C) and whose final file is the salvaged fragmented MP4 that
        ' the session verify predicate accepts (CASE-C predicate result).
        Dim salvageSession As New SessionResult() With {
            .OutputPath = finalC,
            .RequestedDurationSec = 3,
            .ActualDurationSec = 3.1,
            .FramesCaptured = 90,
            .FramesEncoded = 30,
            .NvencErrors = 0,
            .FileExists = finalCExists,
            .FileSize = If(finalCExists, New FileInfo(finalC).Length, 0),
            .VideoStreamFound = predC.VideoStreamFound,
            .AudioRequested = False,
            .AudioStreamFound = False,
            .MuxDroppedBytes = 0,
            .AudioDroppedBytes = 0,
            .MicDroppedBytes = 0,
            .AudioAccountingOk = True,
            .MicAccountingOk = True
        }
        Dim passValue = salvageSession.Pass
        Dim caseEok = passValue
        verdicts("CASE-E") = If(caseEok, "GAP-REPRODUCED", "NOT-REPRODUCED")
        Log($"[CASE-E synthesis] SessionResult(salvage shape): FramesEncoded={salvageSession.FramesEncoded} NvencErrors={salvageSession.NvencErrors} FileExists={salvageSession.FileExists} FileSize={salvageSession.FileSize} VideoStreamFound={salvageSession.VideoStreamFound}")
        Log($"                    → SessionResult.Pass = {passValue} → {verdicts("CASE-E")}")
        Log("")

        ' ─── Report ────────────────────────────────────────────────────
        Dim gapCount = verdicts.Values.Where(Function(v) v = "GAP-REPRODUCED").Count()
        Dim ctrlFail = verdicts.Values.Where(Function(v) v = "UNEXPECTED").Count()
        WriteReport(outDir, ffmpegPath, verdicts, resC, resD, predTrunc, decTrunc, passValue)

        Log("────────────────────────────────────────────────────────────")
        Log($"W2 REPRO RESULT: {gapCount} gap(s) reproduced, {ctrlFail} control failure(s)")
        Log($"Report: {Path.Combine(outDir, "report.md")}")
        If ctrlFail > 0 Then Return 1
        Return 0
    End Function

    ' ─── production-mirroring helpers ────────────────────────────────

    ''' <summary>Bare ffmpeg invocation for fixture building (exit + stderr tail).</summary>
    Private Function RunProcess(exe As String, argLine As String, timeoutMs As Integer) As VerifyOutcome
        Dim psi As New ProcessStartInfo With {
            .FileName = exe,
            .Arguments = argLine,
            .UseShellExecute = False,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }
        Try
            Using proc As Process = Process.Start(psi)
                Dim errTask = proc.StandardError.ReadToEndAsync()
                proc.WaitForExit(timeoutMs)
                Dim stderr As String = If(errTask.Wait(2000), errTask.Result, "")
                Return New VerifyOutcome(False, proc.ExitCode, stderr)
            End Using
        Catch ex As Exception
            Return New VerifyOutcome(False, -999, ex.Message)
        End Try
    End Function

    ''' <summary>The EXACT verify invocation of CaptureSession.vb:1202-1221:
    ''' ffmpeg -hide_banner -i <file>, stderr scanned for header markers.
    ''' Exit code is not part of the production predicate (and is -1 style
    ''' for probe-only runs), so it is recorded but unused — like production.</summary>
    Private Function SessionVerifyPredicate(ffmpegPath As String, mp4 As String) As VerifyOutcome
        Dim psi As New ProcessStartInfo With {
            .FileName = ffmpegPath,
            .Arguments = $"-hide_banner -i ""{mp4}""",
            .UseShellExecute = False,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }
        Try
            Using proc As Process = Process.Start(psi)
                Dim errTask = proc.StandardError.ReadToEndAsync()
                If Not proc.WaitForExit(5000) Then
                    Try : proc.Kill() : proc.WaitForExit(2000) : Catch : End Try
                End If
                Dim stderr As String = If(errTask.Wait(1000), errTask.Result, "")
                Dim found = stderr.Contains("Stream #") AndAlso stderr.Contains("Video:")
                Return New VerifyOutcome(found, proc.ExitCode, stderr)
            End Using
        Catch ex As Exception
            Return New VerifyOutcome(False, -999, ex.Message)
        End Try
    End Function

    ''' <summary>Full-decode validation (what the production gate does NOT do):
    ''' ffmpeg -v error -xerror -i <file> -f null - → exit 0 only if decodable.</summary>
    Private Function FullDecode(ffmpegPath As String, mp4 As String) As VerifyOutcome
        Dim psi As New ProcessStartInfo With {
            .FileName = ffmpegPath,
            .Arguments = $"-v error -xerror -i ""{mp4}"" -f null -",
            .UseShellExecute = False,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }
        Try
            Using proc As Process = Process.Start(psi)
                Dim errTask = proc.StandardError.ReadToEndAsync()
                proc.WaitForExit(30000)
                Dim stderr As String = If(errTask.Wait(2000), errTask.Result, "")
                Return New VerifyOutcome(False, proc.ExitCode, stderr)
            End Using
        Catch ex As Exception
            Return New VerifyOutcome(False, -999, ex.Message)
        End Try
    End Function

    ''' <summary>Drive the REAL LiveMuxSession through Start → Stop with a stub
    ''' ffmpeg. Video-only (rates 0 = audio disabled, per the ctor contract);
    ''' nothing is fed, so the stub's behavior alone decides the outcome.</summary>
    Private Function DriveLiveMux(stubPath As String, finalPath As String) As LiveMuxResult
        Dim mux As New LiveMuxSession(stubPath, finalPath, 30, 0, 0, 0, 0, False, 1.0F, 1.0F,
                                      AddressOf Log)
        Dim started As Boolean = False
        Try
            started = mux.Start()
            If Not started Then
                Dim r As New LiveMuxResult()
                r.ErrorMessage = "start failed"
                Return r
            End If
            Return mux.[Stop](15000)
        Finally
            mux.Dispose()
        End Try
    End Function

    ''' <summary>Stub ffmpeg .cmd. remuxFails=True: mux invocation (no "+faststart"
    ''' in args) copies the fragmented fixture onto the LAST argument (the frag
    ''' path) and exits 0; the remux invocation (args contain "+faststart")
    ''' exits 1. remuxFails=False: every invocation exits 1 (mux-failure control).</summary>
    Private Function WriteStubFfmpeg(stubPath As String, fragSource As String, remuxFails As Boolean) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("@echo off")
        sb.AppendLine("setlocal enabledelayedexpansion")
        sb.AppendLine("set ""last=""")

        sb.AppendLine("for %%A in (%*) do set ""last=%%~A""")
        If remuxFails Then
            sb.AppendLine("echo(%* | findstr /C:""+faststart"" >nul 2>&1")
            sb.AppendLine("if !errorlevel! EQU 0 exit /b 1")
            sb.AppendLine($"copy /y ""{fragSource}"" ""!last!"" >nul 2>&1")
            sb.AppendLine("exit /b 0")
        Else
            sb.AppendLine("exit /b 1")
        End If
        File.WriteAllText(stubPath, sb.ToString(), New System.Text.UTF8Encoding(False))
        Return stubPath
    End Function

    Private Function Trim(s As String, max As Integer) As String
        If String.IsNullOrEmpty(s) Then Return ""
        Dim oneLine = s.Replace(vbCr, " ").Replace(vbLf, " ").Trim()
        If oneLine.Length > max Then Return oneLine.Substring(0, max) & "…"
        Return oneLine
    End Function

    Private Sub Log(msg As String)
        Console.WriteLine(msg)
        _logLines.Add(msg)
    End Sub

    Private Sub WriteReport(outDir As String, ffmpegPath As String,
                            verdicts As Dictionary(Of String, String),
                            resC As LiveMuxResult, resD As LiveMuxResult,
                            predTrunc As VerifyOutcome, decTrunc As VerifyOutcome,
                            salvagePass As Boolean)
        Dim sb As New StringBuilder()
        sb.AppendLine("# W2 False-Success / Error-Propagation repro — evidence")
        sb.AppendLine("")
        sb.AppendLine("- Date (UTC): " & DateTime.UtcNow.ToString("o"))
        sb.AppendLine("- Machine: " & Environment.MachineName)
        sb.AppendLine("- ffmpeg: `" & ffmpegPath & "`")
        sb.AppendLine("- Production files under audit (NOT modified):")
        sb.AppendLine("  - `CaptureEngine.Recording\\CaptureSession.vb:1196-1226` (verify predicate — header-presence only)")
        sb.AppendLine("  - `CaptureEngine.Recording\\RecordingDTOs.vb:303-322` (SessionResult.Pass — no exit-code input)")
        sb.AppendLine("  - `CaptureEngine.FFmpegBackend\\LiveMuxSession.vb:387-406` (remux-fail salvage → Succeeded=True)")
        sb.AppendLine("")
        sb.AppendLine("| Case | Expectation | Result |")
        sb.AppendLine("|---|---|---|")
        For Each kv In verdicts
            sb.AppendLine($"| {kv.Key} | see driver header | {kv.Value} |")
        Next
        sb.AppendLine("")
        sb.AppendLine("## Raw facts")
        sb.AppendLine("")
        sb.AppendLine($"- CASE-B truncated.mp4: VideoStreamFound={predTrunc.VideoStreamFound}, full-decode exit={decTrunc.ExitCode}, decode stderr: `{Trim(decTrunc.StderrTail, 200)}`")
        sb.AppendLine($"- CASE-C LiveMuxResult: ok={resC.Succeeded} exit={resC.FFmpegExitCode} faststart={resC.UsedFaststartRemux} dropped={resC.DroppedBytes}B err=""{resC.ErrorMessage}""")
        sb.AppendLine($"- CASE-D LiveMuxResult: ok={resD.Succeeded} exit={resD.FFmpegExitCode} err=""{resD.ErrorMessage}""")
        sb.AppendLine($"- CASE-E SessionResult.Pass (salvage shape) = {salvagePass}")
        sb.AppendLine("")
        sb.AppendLine("## Conclusion (audit)")
        sb.AppendLine("")
        sb.AppendLine("GAP-1: a failed +faststart remux (exit code 1) is converted into LiveMuxResult.Succeeded=True by the salvage move; CaptureSession maps only DroppedBytes, so the failure never reaches SessionResult.")
        sb.AppendLine("GAP-2: SessionResult.VideoStreamFound is satisfied by container-header presence in `ffmpeg -i` stderr; an undecodable (truncated) MP4 passes, so Pass=True does not imply a decodable file.")
        sb.AppendLine("GAP-3: FFmpeg exit codes (mux/remux/verify) are absent from the SessionResult contract entirely; only file existence + header parse + NvencErrors + frames guard it.")
        sb.AppendLine("Positive controls: hard mux failure fails closed (CASE-D); StartSession exceptions carry ErrorMessage with Pass=False; MuxDroppedBytes>0 fails audioOk; legacy engine HasError/StopRecordingAsync contracts are covered by Engine.Concurrency.Tests G2/G3.")
        File.WriteAllText(Path.Combine(outDir, "report.md"), sb.ToString(), New System.Text.UTF8Encoding(True))
        File.WriteAllText(Path.Combine(outDir, "console.log"), String.Join(Environment.NewLine, _logLines), New System.Text.UTF8Encoding(True))
    End Sub

    ''' <summary>Outcome of one ffmpeg invocation (stream-found predicate result
    ''' + raw exit code + stderr tail for evidence).</summary>
    Private Structure VerifyOutcome
        Public ReadOnly VideoStreamFound As Boolean
        Public ReadOnly ExitCode As Integer
        Public ReadOnly StderrTail As String

        Public Sub New(found As Boolean, exitCode As Integer, stderrTail As String)
            Me.VideoStreamFound = found
            Me.ExitCode = exitCode
            Me.StderrTail = stderrTail
        End Sub
    End Structure

End Module
