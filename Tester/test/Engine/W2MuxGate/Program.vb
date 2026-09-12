Option Strict On
Option Explicit On
Option Infer On

' Program.vb — M2-W2 observation gate: Encode → FeedVideo → MP4
'
' THE question this gate answers, per recording:
'
'     FramesEncoded == MP4 frames ?
'
' and when the answer is NO, it classifies WHERE the chain broke, using the
' counter chain of the production code (read-only, nothing modified):
'
'   encoder → FramesEncoded / TotalVideoBytes   (CaptureSession.vb:819-821 —
'              counted when FeedVideo is CALLED, not when bytes enter the pipe)
'   pipe    → MuxDroppedBytes                   (PipeFeed: block-timeout drops,
'              post-close feeds, stop-drain residual, broken pipe)
'   mux     → LiveMuxResult.FFmpegExitCode      (Succeeded is NOT a gate input:
'              the salvage path reports Succeeded=True after a failed remux —
'              W2 audit GAP-1 — so the exit code + file outcome decide)
'   file    → ffprobe nb_read_packets / nb_read_frames of the final MP4
'
' Classification (priority order, all signals always recorded in the detail):
'   EMPTY-PAYLOAD     FramesEncoded=0 or TotalVideoBytes=0
'   MUX-FAILURE       bytes were offered but no probeable final MP4
'                     (exit≠0 / missing file / unprobeable file)
'   PIPE-DROPPED-AU   MuxDroppedBytes>0 while the MP4 itself completed
'                     (the file is missing exactly the dropped AUs)
'   CLOSED-EQUAL      FramesEncoded == MP4 packets == MP4 frames
'   UNACCOUNTED-LOSS  none of the above and counts still differ —
'                     bytes reached ffmpeg, MP4 valid, frames missing anyway
'
' Modes:
'   selftest (default, runs on any machine — no NVENC needed):
'     drives the REAL LiveMuxSession + REAL ffmpeg through four variants:
'       V1 HEALTHY       feed a real x264 AU stream → expects CLOSED-EQUAL
'       V2 EMPTY-PAYLOAD zero-length / null payloads → EMPTY-PAYLOAD
'       V3 PIPE-DROP     post-close feeds (PipeFeed counts them, the file
'                        completed normally) → PIPE-DROPPED-AU
'       V4 MUX-FAILURE   output into a nonexistent directory → MUX-FAILURE
'   --session (the NVIDIA-machine leg, full production chain):
'     W2MuxGate --session --output <mp4> --frames-encoded N --total-video-bytes B
'               --mux-dropped-bytes D [--nvenc-errors E]
'     runs ffprobe on the session's real MP4 and prints the gate verdict.
'     (FFmpeg exit codes are not part of SessionResult — audit GAP-3 — so the
'      session leg decides on file presence/probeability + drop counters.)

Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports CaptureEngine.FFmpegBackend

Module Program

    Private _logLines As New List(Of String)()
    Private _ffmpegPath As String = ""
    Private _ffprobePath As String = ""

    Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8

        _ffmpegPath = "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
        Dim outDir As String = ""
        Dim sessionMode As Boolean = False

        For i As Integer = 0 To args.Length - 1
            If args(i) = "--session" Then sessionMode = True
            If args(i) = "--ffmpeg" AndAlso i + 1 < args.Length Then _ffmpegPath = args(i + 1)
            If args(i) = "--out" AndAlso i + 1 < args.Length Then outDir = args(i + 1)
        Next
        If Not File.Exists(_ffmpegPath) Then
            Console.Error.WriteLine("FATAL: ffmpeg.exe not found — pass --ffmpeg <path>")
            Return 2
        End If
        Dim probe As String = Path.Combine(Path.GetDirectoryName(_ffmpegPath), "ffprobe.exe")
        _ffprobePath = If(File.Exists(probe), probe, "ffprobe")

        If sessionMode Then
            Return RunSessionGate(args)
        End If

        If String.IsNullOrEmpty(outDir) Then
            outDir = "C:\My Project\NVIDIA-Shadowplay\evidence\w2-mux-gate"
        End If
        Directory.CreateDirectory(outDir)
        Return RunSelftest(outDir)
    End Function

    ' ═══════════════════════ SELFTEST (mux-leg gate, any machine) ═══════════

    Private Function RunSelftest(outDir As String) As Integer
        Log("")
        Log("════════ W2 MUX GATE — Encode → FeedVideo → MP4 observation gate ════════")
        Log("ffmpeg:   " & _ffmpegPath)
        Log("ffprobe:  " & _ffprobePath)
        Log("out:      " & outDir)
        Log("")

        ' ─── Fixture: real x264 AU stream with a KNOWN count ────────────────
        Dim canned = Path.Combine(outDir, "canned-60au.h264")
        Dim mk = Run(_ffmpegPath,
            $"-y -hide_banner -loglevel error -f lavfi -i testsrc=duration=2:size=320x240:rate=30 " &
            $"-c:v libx264 -pix_fmt yuv420p -f h264 ""{canned}""", 30000)
        If mk.ExitCode <> 0 OrElse Not File.Exists(canned) Then
            Log("FATAL: fixture build failed: " & mk.StderrTail)
            Return 2
        End If
        Dim encodeAUs As Long = CountStream(canned)
        Dim encodeBytes As Long = New FileInfo(canned).Length
        If encodeAUs <= 0 Then
            Log("FATAL: could not count AUs in the fixture")
            Return 2
        End If
        Log($"fixture: {encodeAUs} AUs, {encodeBytes:N0} bytes (ffprobe-counted on the raw stream)")
        Log("")

        Dim verdicts As New Dictionary(Of String, String)()
        Dim anyUnexpected As Boolean = False

        ' ─── V1 HEALTHY — equality must hold ────────────────────────────────
        Dim out1 = Path.Combine(outDir, "v1-healthy.mp4")
        Dim r1 = DriveMux(out1,
                          Sub(mux) mux.FeedVideo(File.ReadAllBytes(canned), CInt(encodeBytes)),
                          stopTimeoutMs:=30000)
        Dim v1 = Evaluate("V1-HEALTHY", encodeAUs, encodeBytes, out1, r1, expectEqual:=True)
        verdicts("V1-HEALTHY") = v1.Classification
        If v1.Classification <> "CLOSED-EQUAL" Then anyUnexpected = True
        Log("")

        ' ─── V2 EMPTY-PAYLOAD — encoder yielded no bytes ────────────────────
        ' (session analogue: FramesEncoded may still count zero-length AUs —
        '  TotalVideoBytes=0 is the discriminating signal)
        Dim out2 = Path.Combine(outDir, "v2-empty.mp4")
        Dim zero(0) As Byte
        Dim r2 = DriveMux(out2,
                          Sub(mux)
                              mux.FeedVideo(zero, 0)          ' zero-length payload — PipeFeed ignores
                              mux.FeedVideo(Nothing, 0)       ' null payload — ignored
                          End Sub,
                          stopTimeoutMs:=15000)
        Dim v2 = Evaluate("V2-EMPTY-PAYLOAD", 0, 0, out2, r2, expectEqual:=False)
        verdicts("V2-EMPTY-PAYLOAD") = v2.Classification
        If v2.Classification <> "EMPTY-PAYLOAD" Then anyUnexpected = True
        Log("")

        ' ─── V3 PIPE-DROPPED-AU — AUs lost at the pipe, file completed ──────
        ' (production analogue: an encoder tick racing past the stop-drain
        '  fold — PipeFeed.Feed counts post-close bytes into DroppedBytes,
        '  LiveMuxSession.vb:556-561 — while the MP4 itself finalized fine.
        '  Stop() freezes its DroppedBytes snapshot mid-drain, so a second
        '  Stop() call collects the honest total including the post-close
        '  drops before the gate evaluates.)
        Dim out3 = Path.Combine(outDir, "v3-pipedrop.mp4")
        Dim r3 As LiveMuxResult
        Dim fedAUs3 As Long = encodeAUs * 2L
        Dim mux3 As New LiveMuxSession(_ffmpegPath, out3, 30, 0, 0, 0, 0, False, 1.0F, 1.0F, AddressOf Log)
        Try
            If mux3.Start() Then
                mux3.FeedVideo(File.ReadAllBytes(canned), CInt(encodeBytes))
                Threading.Thread.Sleep(2000)   ' let ffmpeg connect + probe (see DriveMux note)
                mux3.[Stop](30000)
                ' Stop returned → pipes folded → these AUs can only be dropped:
                mux3.FeedVideo(File.ReadAllBytes(canned), CInt(encodeBytes))
                r3 = mux3.[Stop](30000)   ' re-collect: DroppedBytes now includes post-close feeds
            Else
                r3 = New LiveMuxResult()
                r3.ErrorMessage = "start failed"
            End If
        Finally
            mux3.Dispose()
        End Try
        Dim v3 = Evaluate("V3-PIPE-DROP", fedAUs3, encodeBytes * 2L, out3, r3, expectEqual:=False)
        verdicts("V3-PIPE-DROP") = v3.Classification
        If v3.Classification <> "PIPE-DROPPED-AU" Then anyUnexpected = True
        Log("")

        ' ─── V4 MUX-FAILURE — bytes offered, ffmpeg cannot produce the file ──
        ' (output directory does not exist → ffmpeg exits nonzero)
        Dim out4 = Path.Combine(outDir, "nodir", "v4-muxfail.mp4")
        Dim r4 = DriveMux(out4,
                          Sub(mux) mux.FeedVideo(File.ReadAllBytes(canned), CInt(encodeBytes)),
                          stopTimeoutMs:=15000)
        Dim v4 = Evaluate("V4-MUX-FAILURE", encodeAUs, encodeBytes, out4, r4, expectEqual:=False)
        verdicts("V4-MUX-FAILURE") = v4.Classification
        If v4.Classification <> "MUX-FAILURE" Then anyUnexpected = True
        Log("")

        ' ─── Report ──────────────────────────────────────────────────────────
        Log("────────────────────────────────────────────────────────────")
        For Each kv In verdicts
            Log($"gate[{kv.Key}] = {kv.Value}")
        Next
        Log($"SELFTEST: {If(anyUnexpected, "UNEXPECTED RESULT — gate model needs review", "all four variants classified as predicted")}")
        WriteSelftestReport(outDir, verdicts, encodeAUs, encodeBytes)
        Log($"Report: {Path.Combine(outDir, "report.md")}")
        Return If(anyUnexpected, 1, 0)
    End Function

    ' ═════════════════ SESSION GATE (full chain, NVIDIA machine) ═══════════

    ''' <summary>Closes the FULL chain after a real session:
    ''' SessionResult fields → ffprobe of the produced MP4 → verdict.</summary>
    Private Function RunSessionGate(args As String()) As Integer
        Dim output As String = ""
        Dim framesEncoded As Long = -1, totalVideoBytes As Long = -1
        Dim muxDropped As Long = -1, nvencErrors As Long = -1
        For i As Integer = 0 To args.Length - 2
            If args(i) = "--output" Then output = args(i + 1)
            If args(i) = "--frames-encoded" Then Long.TryParse(args(i + 1), framesEncoded)
            If args(i) = "--total-video-bytes" Then Long.TryParse(args(i + 1), totalVideoBytes)
            If args(i) = "--mux-dropped-bytes" Then Long.TryParse(args(i + 1), muxDropped)
            If args(i) = "--nvenc-errors" Then Long.TryParse(args(i + 1), nvencErrors)
        Next
        If String.IsNullOrEmpty(output) OrElse framesEncoded < 0 OrElse totalVideoBytes < 0 OrElse muxDropped < 0 Then
            Console.Error.WriteLine("usage: W2MuxGate --session --output <mp4> --frames-encoded N --total-video-bytes B --mux-dropped-bytes D [--nvenc-errors E]")
            Return 2
        End If

        Dim obs = ObserveFile(output)
        Dim v = Evaluate("SESSION", framesEncoded, totalVideoBytes, -1, muxDropped, 0, obs, expectEqual:=True)
        If nvencErrors > 0 Then
            Log($"[gate note] SessionResult.NvencErrors={nvencErrors} — encoder-side errors present; see CaptureSession containment (C/6)")
        End If
        Log($"W2-GATE VERDICT: {v.Classification} | FramesEncoded({framesEncoded}) == MP4 frames({obs.Mp4Frames})? {If(framesEncoded = obs.Mp4Frames AndAlso obs.Mp4Frames >= 0, "YES", "NO")}")
        Return If(v.Classification = "CLOSED-EQUAL", 0, 1)
    End Function

    ' ═══════════════════════ gate core ══════════════════════════════════════

    Private Class MuxObservation
        Public Property FinalExists As Boolean
        Public Property FileSize As Long
        Public Property Probed As Boolean
        Public Property Mp4Packets As Long = -1
        Public Property Mp4Frames As Long = -1
    End Class

    Private Class GateVerdict
        Public Property Label As String
        Public Property Classification As String
        Public Property EqualityHolds As Boolean
        Public Property Detail As String
    End Class

    Private Function ObserveFile(path As String) As MuxObservation
        Dim obs As New MuxObservation()
        obs.FinalExists = File.Exists(path)
        If obs.FinalExists Then
            obs.FileSize = New FileInfo(path).Length
            obs.Mp4Packets = CountStream(path)
            obs.Mp4Frames = CountFrames(path)
            obs.Probed = (obs.Mp4Packets >= 0)
        End If
        Return obs
    End Function

    ''' <summary>The gate decision — priority: EMPTY-PAYLOAD → MUX-FAILURE →
    ''' PIPE-DROPPED-AU → CLOSED-EQUAL → UNACCOUNTED-LOSS. Every signal is
    ''' recorded in Detail regardless of which one wins. bytesFedToPipe = -1
    ''' when unknown (session leg: SessionResult does not carry it).</summary>
    Private Function Evaluate(label As String,
                              framesEncoded As Long, totalVideoBytes As Long,
                              bytesFedToPipe As Long, muxDroppedBytes As Long,
                              muxExitCode As Integer,
                              obs As MuxObservation,
                              expectEqual As Boolean) As GateVerdict
        Dim v As New GateVerdict() With {.Label = label}

        Dim sb As New StringBuilder()
        sb.Append($"enc(AUs)={framesEncoded} enc(bytes)={totalVideoBytes:N0} ")
        sb.Append(If(bytesFedToPipe >= 0, $"pipeWritten={bytesFedToPipe:N0} ", "pipeWritten=unknown "))
        sb.Append($"dropped={muxDroppedBytes:N0}B muxExit={muxExitCode} ")
        sb.Append($"final(exists={obs.FinalExists} probed={obs.Probed} pkts={obs.Mp4Packets} frames={obs.Mp4Frames} size={obs.FileSize:N0}B)")

        v.EqualityHolds = (obs.Mp4Frames >= 0 AndAlso framesEncoded = obs.Mp4Frames)

        If framesEncoded = 0 OrElse totalVideoBytes = 0 Then
            v.Classification = "EMPTY-PAYLOAD"
        ElseIf (Not obs.FinalExists) OrElse (Not obs.Probed) OrElse muxExitCode <> 0 Then
            v.Classification = "MUX-FAILURE"
            If muxDroppedBytes > 0 Then sb.Append($" [secondary: {muxDroppedBytes:N0}B dropped at the pipe]")
        ElseIf muxDroppedBytes > 0 Then
            v.Classification = "PIPE-DROPPED-AU"
        ElseIf v.EqualityHolds Then
            v.Classification = "CLOSED-EQUAL"
        Else
            v.Classification = "UNACCOUNTED-LOSS"
        End If

        Log($"[gate {label}] FramesEncoded == MP4 frames? {If(v.EqualityHolds, "YES", "NO")} → {v.Classification}")
        Log($"              {sb.ToString()}")
        v.Detail = sb.ToString()
        Return v
    End Function

    ''' <summary>Selftest bridge: observe the final file, then run the gate core
    ''' with the counters from the REAL LiveMuxResult.</summary>
    Private Function Evaluate(label As String, encodeAUs As Long, encodeBytes As Long,
                              finalPath As String, res As LiveMuxResult,
                              expectEqual As Boolean) As GateVerdict
        Dim obs = ObserveFile(finalPath)
        Return Evaluate(label, encodeAUs, encodeBytes, res.VideoBytesFed,
                        res.DroppedBytes, res.FFmpegExitCode, obs, expectEqual)
    End Function

    ''' <summary>Drive the REAL LiveMuxSession with the REAL ffmpeg (video-only:
    ''' rates 0 = audio pipes absent, per the ctor contract). feedAction performs
    ''' the payloads; the caller decides the fault. preStopDelayMs gives ffmpeg
    ''' time to connect + probe before the stop race — production sessions run
    ''' for seconds between feed and stop; an instant Stop() would set
    ''' _stopWriter before ffmpeg connects and the writer discards everything
    ''' (PipeFeed.WriterLoop: LiveMuxSession.vb:624-629).</summary>
    Private Function DriveMux(finalPath As String,
                              feedAction As Action(Of LiveMuxSession),
                              stopTimeoutMs As Integer,
                              Optional preStopDelayMs As Integer = 2000) As LiveMuxResult
        Dim mux As New LiveMuxSession(_ffmpegPath, finalPath, 30, 0, 0, 0, 0, False, 1.0F, 1.0F, AddressOf Log)
        Try
            If Not mux.Start() Then
                Dim r As New LiveMuxResult()
                r.ErrorMessage = "start failed"
                Return r
            End If
            feedAction(mux)
            Threading.Thread.Sleep(preStopDelayMs)
            Return mux.[Stop](stopTimeoutMs)
        Finally
            mux.Dispose()
        End Try
    End Function

    ' ═══════════════════════ helpers ════════════════════════════════════════

    ''' <summary>ffprobe packet/frame count of a stream (raw h264 or MP4).
    ''' -1 = probe failed (recorded as evidence, never swallowed).</summary>
    Private Function CountProbe(filePath As String, entries As String) As Long
        Dim psi As New ProcessStartInfo With {
            .FileName = _ffprobePath,
            .Arguments = $"-v error -count_packets -count_frames -select_streams v:0 " &
                         $"-show_entries stream={entries} -of csv=p=0 ""{filePath}""",
            .UseShellExecute = False,
            .RedirectStandardError = True,
            .RedirectStandardOutput = True,
            .CreateNoWindow = True
        }
        Try
            Using p As Process = Process.Start(psi)
                Dim outTask = p.StandardOutput.ReadToEndAsync()
                Dim errTask = p.StandardError.ReadToEndAsync()
                p.WaitForExit(30000)
                Dim outText As String = If(outTask.Wait(2000), outTask.Result, "").Trim()
                Dim errText As String = If(errTask.Wait(1000), errTask.Result, "").Trim()
                If p.ExitCode <> 0 OrElse outText.Length = 0 Then
                    Log($"[count] ffprobe failed on {Path.GetFileName(filePath)} (exit {p.ExitCode}): {errText}")
                    Return -1
                End If
                Dim n As Long
                If Long.TryParse(outText.Split(","c)(0).Trim(), n) Then Return n
                Return -1
            End Using
        Catch ex As Exception
            Log($"[count] ffprobe exception on {Path.GetFileName(filePath)}: {ex.Message}")
            Return -1
        End Try
    End Function

    Private Function CountStream(path As String) As Long
        Return CountProbe(path, "nb_read_packets")
    End Function

    Private Function CountFrames(path As String) As Long
        Return CountProbe(path, "nb_read_frames")
    End Function

    Private Function Run(exe As String, argLine As String, timeoutMs As Integer) As VerifyOutcome
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
                Return New VerifyOutcome(proc.ExitCode, stderr)
            End Using
        Catch ex As Exception
            Return New VerifyOutcome(-999, ex.Message)
        End Try
    End Function

    Private Sub WriteSelftestReport(outDir As String,
                                    verdicts As Dictionary(Of String, String),
                                    encodeAUs As Long, encodeBytes As Long)
        Dim sb As New StringBuilder()
        sb.AppendLine("# W2 mux gate selftest — Encode → FeedVideo → MP4 observation gate")
        sb.AppendLine("")
        sb.AppendLine("- Date (UTC): " & DateTime.UtcNow.ToString("o"))
        sb.AppendLine("- Machine: " & Environment.MachineName)
        sb.AppendLine("- ffmpeg: `" & _ffmpegPath & "`")
        sb.AppendLine($"- Fixture: real x264 stream, {encodeAUs} AUs / {encodeBytes:N0} bytes (AU count via ffprobe on the raw stream)")
        sb.AppendLine("- Under observation (NOT modified): `LiveMuxSession` (pipes, drops, exit code) + ffprobe counting")
        sb.AppendLine("")
        sb.AppendLine("| Variant | Gate classification |")
        sb.AppendLine("|---|---|")
        For Each kv In verdicts
            sb.AppendLine($"| {kv.Key} | {kv.Value} |")
        Next
        sb.AppendLine("")
        sb.AppendLine("## Counter chain observed per variant")
        sb.AppendLine("")
        sb.AppendLine("See `console.log` — each gate line records: encode AUs/bytes → pipe bytes written → dropped bytes → mux exit → final file exists/probe/packets/frames/size.")
        sb.AppendLine("")
        sb.AppendLine("## Production observations carried by this gate (evidence, no changes made)")
        sb.AppendLine("")
        sb.AppendLine("1. `CaptureSession.vb:819-821` — `FramesEncoded`/`TotalVideoBytes` count FeedVideo CALLS, not bytes that entered the pipe; the compensating signal is `MuxDroppedBytes`.")
        sb.AppendLine("2. `RecordingDTOs.vb:310-313` — `MuxDroppedBytes` gates `Pass` only under `AudioRequested`; a video-only session does not fail Pass on video-AU loss.")
        sb.AppendLine("3. `SessionResult` carries no FFmpeg exit code (audit GAP-3) — the `--session` leg decides on file presence/probeability + drop counters.")
        sb.AppendLine("4. The gate treats `LiveMuxResult.Succeeded` as a RECORDED signal, not a gate input: the salvage path reports Succeeded=True after a failed remux (audit GAP-1).")
        sb.AppendLine("")
        sb.AppendLine("## Session-leg usage (NVIDIA machine — closes the full chain)")
        sb.AppendLine("")
        sb.AppendLine("```")
        sb.AppendLine("W2MuxGate --session --output <session mp4> --frames-encoded <SessionResult.FramesEncoded> \")
        sb.AppendLine("  --total-video-bytes <SessionResult.TotalVideoBytes> --mux-dropped-bytes <SessionResult.MuxDroppedBytes>")
        sb.AppendLine("```")
        File.WriteAllText(Path.Combine(outDir, "report.md"), sb.ToString(), New System.Text.UTF8Encoding(True))
        File.WriteAllText(Path.Combine(outDir, "console.log"), String.Join(Environment.NewLine, _logLines), New System.Text.UTF8Encoding(True))
    End Sub

    Private Sub Log(msg As String)
        Console.WriteLine(msg)
        _logLines.Add(msg)
    End Sub

    Private Structure VerifyOutcome
        Public ReadOnly ExitCode As Integer
        Public ReadOnly StderrTail As String
        Public Sub New(exitCode As Integer, stderrTail As String)
            Me.ExitCode = exitCode
            Me.StderrTail = stderrTail
        End Sub
    End Structure

End Module
