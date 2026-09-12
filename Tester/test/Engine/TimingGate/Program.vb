Option Strict On
Option Explicit On
Option Infer On

' Program.vb — W3 timing/FPS regression gate runner.
'
' Verdict vocabulary (per mission): PASS / FAIL / BLOCKED.
'   BLOCKED = environment cannot exercise the contract (C/4 truthfulness —
'             never converted to PASS, never counted as a failure).
'   FAIL    = the contract is violated on this machine.
'   PASS    = contract held.
'
' KNOWN-RED registry: tests marked knownRed=True are documented baseline
' failures (see W3-REGRESSION-GATE-DESIGN.md §6). They still print FAIL —
' never swallowed — but do not flip the exit code, and a known-red that
' turns GREEN is reported loudly as a baseline change. Any NEW failure
' fails the gate (exit 1).
'
' Mission rule enforced in the core: VFR/cadence verdicts come from the
' ΔPTS distribution only. avg_frame_rate is carried as an informational
' tag and never decides anything (pinned by W3-L0-V1).

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Reflection

Namespace TimingGate.Tests

    Friend Module TestRunner

        Friend _passed As Integer = 0
        Friend _failed As Integer = 0
        Friend _blocked As Integer = 0
        Friend _knownRedFailed As Integer = 0
        Friend ReadOnly _failures As New List(Of String)()
        Friend ReadOnly _knownReds As New List(Of String)()
        Friend ReadOnly _blocks As New List(Of String)()
        Friend ReadOnly _results As New List(Of KeyValuePair(Of String, String))()  ' name → PASS/FAIL/BLOCKED
        Friend _lastReported As String = ""

        Friend Sub RunTest(name As String, test As Action, Optional knownRed As Boolean = False)
            Console.Write($"  {name} ... ")
            _lastReported = ""
            Try
                test()
                Console.WriteLine("PASS")
                If _lastReported.Length > 0 Then Console.WriteLine($"      {_lastReported.TrimEnd()}")
                _passed += 1
                _results.Add(New KeyValuePair(Of String, String)(name, "PASS"))
            Catch ex As SkipException
                Console.WriteLine("BLOCKED")
                Console.WriteLine($"      → {ex.Message}")
                _blocks.Add(name & ": " & ex.Message)
                _blocked += 1
                _results.Add(New KeyValuePair(Of String, String)(name, "BLOCKED"))
            Catch ex As Exception
                Console.WriteLine("FAIL")
                Console.WriteLine($"      → {ex.Message}")
                If _lastReported.Length > 0 Then Console.WriteLine($"      {_lastReported.TrimEnd()}")
                If knownRed Then
                    _knownReds.Add(name & ": " & ex.Message)
                    _knownRedFailed += 1
                    Console.WriteLine("      (KNOWN-RED baseline 2026-09-11 — expected until the production fix; does not flip the gate alone)")
                    _results.Add(New KeyValuePair(Of String, String)(name, "KNOWN-RED"))
                Else
                    _failures.Add(name & ": " & ex.Message)
                    _failed += 1
                    _results.Add(New KeyValuePair(Of String, String)(name, "FAIL"))
                End If
            End Try
        End Sub

        Friend Sub Assert(cond As Boolean, message As String)
            If Not cond Then Throw New Exception(message)
        End Sub

        ''' <summary>Attach the PtsReport line to the current test output.</summary>
        Friend Sub Report(rep As PtsReport)
            _lastReported = rep.ToString()
        End Sub

    End Module

    Friend Module Program

        Function Main(args As String()) As Integer
            ' ─── Helper seam: the engine launches THIS exe as its "ffmpeg".
            If args.Length > 0 AndAlso (args(0) = "-hide_banner" OrElse args(0) = "-v") Then
                Return HelperMode.Run(args)
            End If

            ' ─── Canonical-runner modes (additive; no args = full gate) ───
            If args.Length > 0 AndAlso Array.IndexOf(args, "--analyze") >= 0 Then
                Return RunAnalyzeMode(args)
            End If
            If args.Length > 0 AndAlso Array.IndexOf(args, "--probe") >= 0 Then
                Return RunProbeMode(args)
            End If

            Console.WriteLine("======================================================")
            Console.WriteLine(" Engine.TimingGate.Tests — W3 timing/FPS regression gate")
            Console.WriteLine("   verdicts: PASS / FAIL / BLOCKED")
            Console.WriteLine("   VFR verdict from ΔPTS distribution only (avg tag = info)")
            Console.WriteLine("======================================================")
            Console.WriteLine(" " & ProvenanceLine())
            Console.WriteLine()

            If Not TestRig.Setup() Then
                Console.WriteLine("SETUP FAILED — bundled ffmpeg.exe not found under Overlay\")
                Return 2
            End If

            Dim baselineFfmpeg As Integer = TestRig.FfmpegCount()
            Console.WriteLine($" setup: ffmpeg  = {TestRig._ffmpeg}")
            Console.WriteLine($" setup: ffprobe = {If(TestRig._ffprobe.Length > 0, TestRig._ffprobe, "(MISSING — grid cells will be BLOCKED)")}")
            Console.WriteLine($" setup: sandbox = {TestRig._sandbox}")
            Console.WriteLine($" setup: baseline ffmpeg.exe processes = {baselineFfmpeg}")
            Console.WriteLine()

            Try
                L0Tests.RunAll()
                L1Tests.RunAll()
                L2Tests.RunAll()
            Finally
                TestRig.CleanupSandbox()
            End Try

            ' ─── Summary ───
            Console.WriteLine()
            Console.WriteLine("──────────────────────────────────────────────")
            Console.WriteLine($" RESULT: PASS={TestRunner._passed}  FAIL={TestRunner._failed}  BLOCKED={TestRunner._blocked}  KNOWN-RED-FAIL={TestRunner._knownRedFailed}")

            For Each f As String In TestRunner._failures
                Console.WriteLine($"   FAILED: {f}")
            Next
            For Each k As String In TestRunner._knownReds
                Console.WriteLine($"   KNOWN-RED (baseline): {k}")
            Next
            For Each b As String In TestRunner._blocks
                Console.WriteLine($"   BLOCKED: {b}")
            Next

            ' ─── Mission coverage table ───
            ' Area verdict semantics: FAIL dominates; a documented KNOWN-RED
            ' failure outranks a PASS (the contract is currently violated);
            ' PASS outranks BLOCKED (coverage proven beats coverage this
            ' machine cannot exercise); BLOCKED only when nothing ran.
            Console.WriteLine()
            Console.WriteLine(" MISSION COVERAGE (FAIL > KNOWN-RED > PASS > BLOCKED):")
            Dim areas As String() = {"PTS monotonic", "1/fps cadence", "first-gap classification",
                                     "frame-count integrity", "encode→packet integrity",
                                     "restart", "failure propagation", "240 FPS"}
            For Each area As String In areas
                Dim worst As String = "(no test)"
                For Each kv As KeyValuePair(Of String, String) In TestRunner._results
                    If Not Covers(area, kv.Key) Then Continue For
                    If Rank(kv.Value) > Rank(worst) Then worst = kv.Value
                Next
                Console.WriteLine($"   {area,-28} → {worst}")
            Next

            ' ─── Orphan check (suite-level) ───
            Dim finalFfmpeg As Integer = TestRig.FfmpegCount()
            Console.WriteLine()
            Console.WriteLine($" final ffmpeg.exe processes = {finalFfmpeg} (baseline {baselineFfmpeg})")
            Dim orphanFail As Boolean = False
            If finalFfmpeg > baselineFfmpeg Then
                Console.WriteLine("   → ORPHAN ffmpeg processes detected by the gate itself")
                orphanFail = True
            End If

            Console.WriteLine()
            If TestRunner._failed > 0 OrElse orphanFail Then
                Console.WriteLine(" GATE VERDICT: FAIL")
                Return 1
            End If
            If TestRunner._knownRedFailed > 0 Then
                Console.WriteLine(" GATE VERDICT: PASS (with documented KNOWN-RED baseline failures — see W3-REGRESSION-GATE-DESIGN.md §6)")
            Else
                Console.WriteLine(" GATE VERDICT: PASS")
            End If
            Return 0
        End Function

        Private Function Covers(area As String, testName As String) As Boolean
            Select Case area
                Case "PTS monotonic"
                    Return testName.StartsWith("W3-L0-A2") OrElse testName.StartsWith("W3-L2-Q1") OrElse
                           testName.StartsWith("W3-L2-Q2") OrElse testName.StartsWith("W3-L2-Q3") OrElse
                           testName.StartsWith("W3-L2-Q4")
                Case "1/fps cadence"
                    Return testName.StartsWith("W3-L0-A1") OrElse testName.StartsWith("W3-L2-Q1") OrElse
                           testName.StartsWith("W3-L2-Q2") OrElse testName.StartsWith("W3-L2-Q3") OrElse
                           testName.StartsWith("W3-L2-Q4")
                Case "first-gap classification"
                    Return testName.StartsWith("W3-L0-A3") OrElse testName.StartsWith("W3-L2-Q1") OrElse
                           testName.StartsWith("W3-L2-Q2") OrElse testName.StartsWith("W3-L2-Q3") OrElse
                           testName.StartsWith("W3-L2-Q4")
                Case "frame-count integrity"
                    Return testName.StartsWith("W3-L0-A1") OrElse testName.StartsWith("W3-L2-Q1") OrElse
                           testName.StartsWith("W3-L2-Q2") OrElse testName.StartsWith("W3-L2-Q3") OrElse
                           testName.StartsWith("W3-L2-Q4")
                Case "encode→packet integrity"
                    Return testName.StartsWith("W3-L1-B4") OrElse testName.StartsWith("W3-L2-Q1") OrElse
                           testName.StartsWith("W3-L2-Q2") OrElse testName.StartsWith("W3-L2-Q3") OrElse
                           testName.StartsWith("W3-L2-Q4")
                Case "restart"
                    Return testName.StartsWith("W3-L1-C1") OrElse testName.StartsWith("W3-L1-B5") OrElse
                           testName.StartsWith("W3-L2-C1") OrElse testName.StartsWith("W3-L2-N6")
                Case "failure propagation"
                    Return testName.StartsWith("W3-L1-B1") OrElse testName.StartsWith("W3-L1-B2") OrElse
                           testName.StartsWith("W3-L2-Q5a")
                Case "240 FPS"
                    Return testName.StartsWith("W3-L2-Q5a") OrElse testName.StartsWith("W3-L2-Q5b") OrElse
                           testName.StartsWith("W3-L2-N5") OrElse testName.StartsWith("W3-L0-A5")
            End Select
            Return False
        End Function

        ''' <summary>Area-verdict ranking: higher = worse (dominates).</summary>
        Private Function Rank(verdict As String) As Integer
            Select Case verdict
                Case "FAIL" : Return 4
                Case "KNOWN-RED" : Return 3
                Case "PASS" : Return 2
                Case "BLOCKED" : Return 1
                Case Else : Return 0
            End Select
        End Function

        Private Function ProvenanceLine() As String
            Dim buildUtc As String = "unknown"
            Dim sourceRev As String = "unknown"
            For Each a As AssemblyMetadataAttribute In
                Assembly.GetExecutingAssembly().GetCustomAttributes(Of AssemblyMetadataAttribute)()
                If a.Key = "BuildUtc" Then buildUtc = a.Value
                If a.Key = "SourceRevision" Then sourceRev = a.Value
            Next
            Return $"binary provenance: built {buildUtc} from source {sourceRev}"
        End Function

        ' ═══════════════════════════════════════════════════════════
        ' Canonical-runner modes (machine-readable; strict args — the
        ' M1/W3 audit rule: unknown arguments must FAIL LOUDLY, never
        ' be silently ignored).
        ' ═══════════════════════════════════════════════════════════

        ''' <summary>--analyze <file> --fps N [--seconds S] — run the SAME
        ''' PtsAnalyzer the gate uses on one file. Exit 0=PASS 1=FAIL 2=usage
        ''' 3=BLOCKED (analyzer/tools unavailable). Last stdout line is
        ''' ##PTSRESULT## {json}.</summary>
        Private Function RunAnalyzeMode(args As String()) As Integer
            Dim file As String = ""
            Dim fps As Integer = 0
            Dim seconds As Double = 0.0
            Dim seen As New HashSet(Of String)()
            Dim i As Integer = 0
            While i < args.Length
                Dim a As String = args(i)
                If a <> "--analyze" AndAlso a <> "--fps" AndAlso a <> "--seconds" Then
                    Console.Error.WriteLine($"USAGE ERROR: unknown arg '{a}' — supported: --analyze <file> --fps N [--seconds S]")
                    Return 2
                End If
                If seen.Contains(a) OrElse i + 1 >= args.Length Then
                    Console.Error.WriteLine($"USAGE ERROR: malformed flag '{a}' (missing value or duplicated)")
                    Return 2
                End If
                seen.Add(a)
                Dim v As String = args(i + 1)
                Select Case a
                    Case "--analyze" : file = v
                    Case "--fps"
                        If Not Integer.TryParse(v, fps) OrElse fps <= 0 Then
                            Console.Error.WriteLine($"USAGE ERROR: --fps needs a positive integer, got '{v}'")
                            Return 2
                        End If
                    Case "--seconds"
                        If Not Double.TryParse(v, Globalization.NumberStyles.Float,
                                               Globalization.CultureInfo.InvariantCulture, seconds) OrElse seconds < 0 Then
                            Console.Error.WriteLine($"USAGE ERROR: --seconds needs a non-negative number, got '{v}'")
                            Return 2
                        End If
                End Select
                i += 2
            End While
            If String.IsNullOrWhiteSpace(file) OrElse fps = 0 Then
                Console.Error.WriteLine("USAGE ERROR: --analyze <file> and --fps N are required")
                Return 2
            End If

            If Not TestRig.Setup() Then
                Console.WriteLine("##PTSRESULT## " & SimpleJson(New String()() {}, "verdict", "BLOCKED", "reason", "ffmpeg/ffprobe not found"))
                Console.Error.WriteLine("SETUP FAILED — bundled ffmpeg.exe not found")
                Return 3
            End If
            Try
                Dim rep As PtsReport = PtsProbe.AnalyzeFile(TestRig._ffprobe, file, fps, seconds, "analyze")
                Console.WriteLine(rep.ToString())
                Console.WriteLine("##PTSRESULT## " & ReportJson(rep, file, fps, seconds, "PASS/FAIL"))
                Return If(rep.IsPass, 0, 1)
            Catch ex As SkipException
                Console.WriteLine("##PTSRESULT## " & SimpleJson(New String()() {},
                    "verdict", "BLOCKED", "reason", ex.Message, "file", file, "fps", fps.ToString()))
                Return 3
            Catch ex As Exception
                Console.Error.WriteLine($"ANALYZE ERROR: {ex.Message}")
                Console.WriteLine("##PTSRESULT## " & SimpleJson(New String()() {},
                    "verdict", "FAIL", "reason", ex.Message, "file", file, "fps", fps.ToString()))
                Return 1
            Finally
                TestRig.CleanupSandbox()
            End Try
        End Function

        ''' <summary>--probe [--ffmpeg <path>] — capability probe for lane/mode
        ''' decisions. Exit 0 always (probe succeeded); JSON carries the truth.
        ''' Last stdout line is ##PROBE## {json}.</summary>
        Private Function RunProbeMode(args As String()) As Integer
            Dim ffmpegOverride As String = ""
            Dim seen As New HashSet(Of String)()
            Dim i As Integer = 0
            While i < args.Length
                Dim a As String = args(i)
                If a <> "--probe" AndAlso a <> "--ffmpeg" Then
                    Console.Error.WriteLine($"USAGE ERROR: unknown arg '{a}' — supported: --probe [--ffmpeg <path>]")
                    Return 2
                End If
                If seen.Contains(a) OrElse (a = "--ffmpeg" AndAlso i + 1 >= args.Length) Then
                    Console.Error.WriteLine($"USAGE ERROR: malformed flag '{a}'")
                    Return 2
                End If
                seen.Add(a)
                If a = "--ffmpeg" Then
                    ffmpegOverride = args(i + 1)
                    i += 2
                Else
                    i += 1
                End If
            End While

            If Not TestRig.Setup() Then
                Console.WriteLine("##PROBE## " & SimpleJson(New String()() {}, "verdict", "BLOCKED", "reason", "ffmpeg not found"))
                Return 0
            End If
            If Not String.IsNullOrEmpty(ffmpegOverride) AndAlso File.Exists(ffmpegOverride) Then
                TestRig._ffmpeg = ffmpegOverride
                Dim probe As String = Path.Combine(Path.GetDirectoryName(ffmpegOverride), "ffprobe.exe")
                TestRig._ffprobe = If(File.Exists(probe), probe, "")
            End If

            QsvGate.EnsureProbed(TestRig._ffmpeg)
            Dim modes As New Dictionary(Of Integer, Boolean)()
            For Each fps As Integer In New Integer() {30, 60, 120, 144, 240}
                modes(fps) = QsvGate.ModeSupported(TestRig._ffmpeg, fps)
            Next
            Dim nativeOk As Boolean = NativeGate.NativeAvailable

            Console.WriteLine($" probe: ffmpeg = {TestRig._ffmpeg}")
            Console.WriteLine($" probe: ffprobe = {TestRig._ffprobe}")
            Console.WriteLine($" probe: qsvAvailable = {QsvGate.QsvAvailable}  nativeAvailable = {nativeOk}")
            For Each fps As Integer In New Integer() {30, 60, 120, 144, 240}
                Console.WriteLine($" probe: modeSupported {fps}fps = {modes(fps)}")
            Next

            Dim sb As New Text.StringBuilder()
            Const Q As String = """"   ' exactly one double-quote character
            sb.Append("{" & Q & "schema" & Q & ":" & Q & "timinggate-probe-v1" & Q)
            sb.Append($",""ffmpeg"":""{JsonEsc(TestRig._ffmpeg)}""")
            sb.Append($",""ffprobe"":""{JsonEsc(TestRig._ffprobe)}""")
            sb.Append($",""qsvAvailable"":{If(QsvGate.QsvAvailable, "true", "false")}")
            sb.Append($",""nativeAvailable"":{If(nativeOk, "true", "false")}")
            sb.Append($",""nativeReason"":""{JsonEsc(If(nativeOk, "", NativeGate.Reason))}""")
            sb.Append(",""modeSupported"":{")
            Dim first As Boolean = True
            For Each fps As Integer In New Integer() {30, 60, 120, 144, 240}
                If Not first Then sb.Append(",")
                sb.Append($"""{fps}"":{If(modes(fps), "true", "false")}")
                first = False
            Next
            sb.Append("}")
            sb.Append($",""utc"":""{DateTime.UtcNow.ToString("o")}""")
            sb.Append("}")
            Console.WriteLine("##PROBE## " & sb.ToString())
            Return 0
        End Function

        Private Function ReportJson(rep As PtsReport, file As String, fps As Integer, seconds As Double, verdict As String) As String
            Const Q As String = """"   ' exactly one double-quote character
            Dim inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
            Dim sb As New Text.StringBuilder()
            sb.Append("{" & Q & "schema" & Q & ":" & Q & "timinggate-analyze-v1" & Q)
            sb.Append("," & Q & "file" & Q & ":" & Q & JsonEsc(file) & Q)
            sb.Append("," & Q & "fps" & Q & ":" & fps.ToString(inv))
            sb.Append("," & Q & "seconds" & Q & ":" & seconds.ToString(inv))
            sb.Append("," & Q & "verdict" & Q & ":" & Q & If(rep.IsPass, "PASS", "FAIL") & Q)
            sb.Append("," & Q & "primaryClass" & Q & ":" & Q & rep.PrimaryClass.ToString() & Q)
            sb.Append("," & Q & "frames" & Q & ":" & rep.Frames.ToString(inv))
            sb.Append("," & Q & "packets" & Q & ":" & rep.Packets.ToString(inv))
            sb.Append("," & Q & "containerDurationSec" & Q & ":" & rep.ContainerDurationSec.ToString(inv))
            sb.Append("," & Q & "effFps" & Q & ":" & rep.EffFps.ToString(inv))
            sb.Append("," & Q & "firstDeltaMs" & Q & ":" & rep.FirstDeltaMs.ToString(inv))
            sb.Append("," & Q & "medianDeltaMs" & Q & ":" & rep.MedianDeltaMs.ToString(inv))
            sb.Append("," & Q & "meanDeltaMs" & Q & ":" & rep.MeanDeltaMs.ToString(inv))
            sb.Append("," & Q & "p95Ms" & Q & ":" & rep.P95Ms.ToString(inv))
            sb.Append("," & Q & "p99Ms" & Q & ":" & rep.P99Ms.ToString(inv))
            sb.Append("," & Q & "minDeltaMs" & Q & ":" & rep.MinDeltaMs.ToString(inv))
            sb.Append("," & Q & "maxDeltaMs" & Q & ":" & rep.MaxDeltaMs.ToString(inv))
            sb.Append("," & Q & "dupCount" & Q & ":" & rep.DupCount.ToString(inv))
            sb.Append("," & Q & "negCount" & Q & ":" & rep.NegCount.ToString(inv))
            sb.Append("," & Q & "avgRateTag" & Q & ":" & Q & JsonEsc(rep.ContainerAvgRate) & Q)
            sb.Append("," & Q & "avgRateTagInformationalOnly" & Q & ":true")
            Dim notes As New List(Of String)()
            For Each n As String In rep.Notes
                notes.Add(Q & JsonEsc(n) & Q)
            Next
            sb.Append("," & Q & "notes" & Q & ":[" & String.Join(",", notes.ToArray()) & "]")
            sb.Append("," & Q & "utc" & Q & ":" & Q & DateTime.UtcNow.ToString("o") & Q)
            sb.Append("}")
            Return sb.ToString()
        End Function

        Private Function SimpleJson(pairs As String()(), ParamArray kv As String()) As String
            Dim sb As New Text.StringBuilder()
            sb.Append("{")
            Dim first As Boolean = True
            For idx As Integer = 0 To kv.Length - 2 Step 2
                If Not first Then sb.Append(",")
                sb.Append($"""{kv(idx)}"":""{JsonEsc(kv(idx + 1))}""")
                first = False
            Next
            sb.Append("}")
            Return sb.ToString()
        End Function

        Friend Function JsonEsc(s As String) As String
            If s Is Nothing Then Return ""
            Dim sb As New Text.StringBuilder()
            For Each ch As Char In s
                If ch = ChrW(92) Then          ' backslash
                    sb.Append("\\")
                ElseIf ch = ChrW(34) Then      ' double quote
                    sb.Append("\""")
                ElseIf ch = vbCr Then
                    sb.Append("\r")
                ElseIf ch = vbLf Then
                    sb.Append("\n")
                ElseIf AscW(ch) < 32 Then
                    sb.Append("\u").Append(AscW(ch).ToString("x4"))
                Else
                    sb.Append(ch)
                End If
            Next
            Return sb.ToString()
        End Function

    End Module

End Namespace
