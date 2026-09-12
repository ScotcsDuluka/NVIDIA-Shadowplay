Option Strict On
Option Explicit On
Option Infer On

' Program.vb — canonical legacy-engine recording driver (W3).
'
' Drives the REAL production legacy engine (NVIDIA_Capture.CaptureEngine)
' for ONE recording per invocation, under the canonical-runner contract:
'
'   STRICT ARGUMENTS — an unknown/malformed argument is a hard error
'   (exit 2 + ##RESULT## verdict "ERROR-ARGS"). Nothing is silently ignored;
'   this is the fix for the M1/W3 audit finding where stale scripts fed
'   unsupported flags (--fps/--duration/--output) to a driver that never
'   parsed them.
'
'   MACHINE-READABLE — the final line of stdout is always
'     ##RESULT## {json}
'   carrying the binary identity (path + SHA256), the effective settings
'   echo, and the raw engine facts. The PASS/FAIL verdict itself belongs
'   to the runner (it owns the ffprobe/PTS analysis); this driver reports
'   RECORDED / FAILED / ERROR-ARGS honestly and never inspects media.
'
' Settings are the production defaults with the matrix's single declared
' deviation: SystemAudioCapture=False → single-process video-only mode.
' Nothing here touches production code.

Imports System.Diagnostics
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports EngineCapture = NVIDIA_Capture.CaptureEngine
Imports EngineSettings = NVIDIA_Capture.CaptureSettings

Module Program

    Private _stateLog As New List(Of String)()
    Private _errors As New List(Of String)()

    Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8

        ' ── STRICT argument parsing (unknown args FAIL LOUDLY) ──
        Dim fps As Integer = 0
        Dim seconds As Integer = 0
        Dim outPath As String = ""
        Dim ffmpegPath As String = ""
        Dim encoder As String = "h264_qsv"

        Dim seen As New HashSet(Of String)()
        Dim i As Integer = 0
        While i < args.Length
            Dim a As String = args(i)
            If Not a.StartsWith("--", StringComparison.Ordinal) Then
                Return UsageError($"unexpected token '{a}' — only --flag value pairs are accepted")
            End If
            If a <> "--encoder" AndAlso Not IsKnownFlag(a) Then
                Return UsageError($"UNKNOWN ARG '{a}' — supported: --fps --seconds --out --ffmpeg --encoder")
            End If
            If i + 1 >= args.Length Then
                Return UsageError($"flag '{a}' is missing its value")
            End If
            If seen.Contains(a) Then
                Return UsageError($"duplicate flag '{a}'")
            End If
            seen.Add(a)
            Dim value As String = args(i + 1)
            Select Case a
                Case "--fps" : If Not Integer.TryParse(value, fps) OrElse fps <= 0 Then Return UsageError($"--fps needs a positive integer, got '{value}'")
                Case "--seconds" : If Not Integer.TryParse(value, seconds) OrElse seconds <= 0 Then Return UsageError($"--seconds needs a positive integer, got '{value}'")
                Case "--out" : outPath = value
                Case "--ffmpeg" : ffmpegPath = value
                Case "--encoder" : encoder = value
            End Select
            i += 2
        End While
        If fps = 0 Then Return UsageError("--fps is required")
        If seconds = 0 Then Return UsageError("--seconds is required")
        If String.IsNullOrWhiteSpace(outPath) Then Return UsageError("--out is required")

        ' ── ffmpeg resolution (runner normally pins it via --ffmpeg) ──
        If String.IsNullOrEmpty(ffmpegPath) Then
            Dim candidates As String() = {
                "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
            }
            For Each c In candidates
                If File.Exists(c) Then ffmpegPath = c : Exit For
            Next
        End If

        ' ── effective settings (canonical, echoed in the result) ──
        Dim settings As New EngineSettings()
        settings.FPS = fps
        settings.Bitrate = 20_000_000L
        settings.Encoder = encoder
        settings.CaptureMethod = "ddagrab"     ' production capture method
        settings.PixelFormat = "nv12"
        settings.RateControl = "cbr"
        settings.NvencPreset = 4
        settings.UseNativeResolution = True
        settings.SystemAudioCapture = False    ' video-only single-process mode
        settings.MicCapture = False
        settings.AudioCapture = False
        settings.FFmpegPath = ffmpegPath
        settings.OutputDirectory = Path.GetDirectoryName(outPath)

        Dim validation = settings.Validate()
        If Not validation.Valid Then
            EmitResult("ERROR-ARGS", fps, seconds, outPath, ffmpegPath, encoder,
                       False, False, "HasError", _stateLog, _errors, 0.0,
                       "settings invalid: " & validation.Message)
            Console.Error.WriteLine($"SETTINGS INVALID: {validation.Message}")
            Return 2
        End If

        Dim outDir As String = Path.GetDirectoryName(outPath)
        If Not String.IsNullOrEmpty(outDir) AndAlso Not Directory.Exists(outDir) Then
            Directory.CreateDirectory(outDir)
        End If

        ' ── record through the real engine ──
        Dim swTotal As Stopwatch = Stopwatch.StartNew()
        Dim engine As New EngineCapture(settings)
        AddHandler engine.ErrorOccurred, Sub(msg As String)
                                             LockAdd(_errors, msg)
                                             Console.WriteLine($"[engine-error] {msg}")
                                         End Sub
        AddHandler engine.StateChanged, Sub(state As EngineCapture.CaptureState)
                                            LockAdd(_stateLog, $"{swTotal.Elapsed.TotalSeconds:F2}s {state}")
                                        End Sub
        AddHandler engine.RecordingStarted, Sub(path As String)
                                                Console.WriteLine($"[recording-started] {path}")
                                            End Sub

        Dim startedAt As Double = 0.0
        Dim started As Boolean = engine.StartRecordingAsync(outPath).GetAwaiter().GetResult()
        startedAt = swTotal.Elapsed.TotalSeconds
        Console.WriteLine($"[start] requested fps={fps} → started={started} at t={startedAt:F2}s state={engine.State}")

        If Not started Then
            EmitResult("FAILED", fps, seconds, outPath, ffmpegPath, encoder,
                       False, False, engine.State.ToString(), _stateLog, _errors, swTotal.Elapsed.TotalSeconds,
                       "StartRecordingAsync returned False")
            Return 1
        End If

        Dim deadline As DateTime = DateTime.UtcNow.AddSeconds(seconds)
        While DateTime.UtcNow < deadline
            System.Threading.Thread.Sleep(100)
        End While

        Dim stoppedAt As Double = swTotal.Elapsed.TotalSeconds
        Dim stopped As Boolean = engine.StopRecordingAsync().GetAwaiter().GetResult()
        Console.WriteLine($"[stop] stopped={stopped} at t={stoppedAt:F2}s state={engine.State}")
        Console.WriteLine("[states] " & String.Join(" | ", _stateLog.ToArray()))

        Dim fileExists As Boolean = File.Exists(outPath)
        Dim fileSize As Long = If(fileExists, New FileInfo(outPath).Length, -1L)

        Dim verdict As String = If(stopped AndAlso fileExists, "RECORDED", "FAILED")
        EmitResult(verdict, fps, seconds, outPath, ffmpegPath, encoder,
                   started, stopped, engine.State.ToString(), _stateLog, _errors, swTotal.Elapsed.TotalSeconds, "")
        Return If(verdict = "RECORDED", 0, 1)
    End Function

    Private Function IsKnownFlag(a As String) As Boolean
        Return a = "--fps" OrElse a = "--seconds" OrElse a = "--out" OrElse a = "--ffmpeg"
    End Function

    Private Function UsageError(message As String) As Integer
        Console.Error.WriteLine($"USAGE ERROR: {message}")
        Console.Error.WriteLine("usage: FpsMatrixDriver --fps N --seconds N --out <mp4> [--ffmpeg <path>] [--encoder <id>]")
        EmitResult("ERROR-ARGS", 0, 0, "", "", "", False, False, "", _stateLog, _errors, 0.0, message)
        Return 2
    End Function

    Private Sub LockAdd(list As List(Of String), item As String)
        SyncLock list : list.Add(item) : End SyncLock
    End Sub

    ' ── machine-readable result (always the LAST stdout line) ──

    Private Const Q As String = """"   ' exactly one double-quote character

    Private Sub EmitResult(verdict As String, fps As Integer, seconds As Integer, outPath As String,
                           ffmpegPath As String, encoder As String, started As Boolean, stopped As Boolean,
                           stateFinal As String, states As List(Of String), errors As List(Of String),
                           wallSec As Double, reason As String)
        Dim exePath As String = Environment.ProcessPath
        Dim sb As New StringBuilder()
        sb.Append("{" & Q & "schema" & Q & ":" & Q & "fpsmatrix-result-v1" & Q)
        sb.Append("," & Q & "verdict" & Q & ":" & Q & verdict & Q)
        sb.Append("," & Q & "reason" & Q & ":" & Q & JsonEsc(reason) & Q)
        sb.Append("," & Q & "binary" & Q & ":{" & Q & "path" & Q & ":" & Q & JsonEsc(exePath) & Q &
                  "," & Q & "sha256" & Q & ":" & Q & SelfSha256(exePath) & Q & "}")
        sb.Append("," & Q & "settings" & Q & ":{" & Q & "lane" & Q & ":" & Q & "legacy" & Q &
                  "," & Q & "encoder" & Q & ":" & Q & JsonEsc(encoder) & Q &
                  "," & Q & "captureMethod" & Q & ":" & Q & "ddagrab" & Q &
                  "," & Q & "bitrate" & Q & ":20000000" &
                  "," & Q & "rateControl" & Q & ":" & Q & "cbr" & Q &
                  "," & Q & "pixelFormat" & Q & ":" & Q & "nv12" & Q &
                  "," & Q & "audioEnabled" & Q & ":false}")
        sb.Append("," & Q & "run" & Q & ":{" & Q & "fps" & Q & ":" & fps.ToString(Globalization.CultureInfo.InvariantCulture) &
                  "," & Q & "seconds" & Q & ":" & seconds.ToString(Globalization.CultureInfo.InvariantCulture) &
                  "," & Q & "out" & Q & ":" & Q & JsonEsc(outPath) & Q &
                  "," & Q & "ffmpeg" & Q & ":" & Q & JsonEsc(ffmpegPath) & Q & "}")
        sb.Append("," & Q & "engine" & Q & ":{" & Q & "started" & Q & ":" & If(started, "true", "false") &
                  "," & Q & "stopped" & Q & ":" & If(stopped, "true", "false") &
                  "," & Q & "stateFinal" & Q & ":" & Q & JsonEsc(stateFinal) & Q &
                  "," & Q & "states" & Q & ":[" & JsonArray(states) & "]" &
                  "," & Q & "errors" & Q & ":[" & JsonArray(errors) & "]}")
        Dim fileExists As Boolean = Not String.IsNullOrEmpty(outPath) AndAlso File.Exists(outPath)
        Dim fileSize As Long = If(fileExists, New FileInfo(outPath).Length, -1L)
        sb.Append("," & Q & "file" & Q & ":{" & Q & "exists" & Q & ":" & If(fileExists, "true", "false") &
                  "," & Q & "size" & Q & ":" & fileSize.ToString(Globalization.CultureInfo.InvariantCulture) & "}")
        sb.Append("," & Q & "wallSec" & Q & ":" & wallSec.ToString(Globalization.CultureInfo.InvariantCulture))
        sb.Append("," & Q & "utc" & Q & ":" & Q & DateTime.UtcNow.ToString("o") & Q)
        sb.Append("}")
        Console.WriteLine("##RESULT## " & sb.ToString())
    End Sub

    Private Function JsonArray(items As List(Of String)) As String
        Dim escaped As New List(Of String)()
        SyncLock items
            For Each s As String In items
                escaped.Add(Q & JsonEsc(s) & Q)
            Next
        End SyncLock
        Return String.Join(",", escaped.ToArray())
    End Function

    Private Function JsonEsc(s As String) As String
        If s Is Nothing Then Return ""
        Dim sb As New StringBuilder()
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

    Private Function SelfSha256(exePath As String) As String
        Try
            Using sha As SHA256 = SHA256.Create()
                Using fs As FileStream = File.OpenRead(exePath)
                    Dim hash As Byte() = sha.ComputeHash(fs)
                    Dim sb As New StringBuilder(hash.Length * 2)
                    For Each b As Byte In hash
                        sb.Append(b.ToString("x2"))
                    Next
                    Return sb.ToString()
                End Using
            End Using
        Catch
            Return "unavailable"
        End Try
    End Function

End Module
