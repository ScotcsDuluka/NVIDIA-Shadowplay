Option Strict On
Option Explicit On
Option Infer On

' TestRig.vb — sandbox, tool resolution, and the helper-exe seam.
'
' Helper seam (same proven pattern as G2/F03): the engine is pointed at
' THIS executable via CaptureSettings.FFmpegPath = Environment.ProcessPath.
' Program.Main dispatches to HelperMode.Run when the first argument looks
' like an ffmpeg command. The helper is env-controlled:
'
'   LMHLP_SRC / LMHLP_COPYTO   copy a fixture file to the engine's output
'                              (video-only mode: dst IS the final output)
'   LMHLP_CREATE_EMPTY         create a 0-byte file at LMHLP_COPYTO
'                              (the false-success garbage fixture)
'   LMHLP_SLEEP / LMHLP_EXIT   hold until 'q' (graceful) then exit code

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Threading

Namespace TimingGate.Tests

    Friend Module TestRig

        Friend _ffmpeg As String = ""
        Friend _ffprobe As String = ""
        Friend _sandbox As String = ""
        Friend Const SandboxPrefix As String = "timing-gate-tests-"

        Friend Function Setup() As Boolean
            _ffmpeg = ResolveFfmpeg()
            If String.IsNullOrEmpty(_ffmpeg) OrElse Not File.Exists(_ffmpeg) Then Return False
            Dim probe As String = Path.Combine(Path.GetDirectoryName(_ffmpeg), "ffprobe.exe")
            _ffprobe = If(File.Exists(probe), probe, "")
            _sandbox = Path.Combine(Path.GetTempPath(), SandboxPrefix & DateTime.Now.ToString("yyyyMMdd_HHmmss"))
            Directory.CreateDirectory(_sandbox)
            Return True
        End Function

        Friend Sub CleanupSandbox()
            If String.IsNullOrEmpty(_sandbox) Then Return
            Try
                If Directory.Exists(_sandbox) Then Directory.Delete(_sandbox, True)
            Catch
            End Try
        End Sub

        Friend Function ResolveFfmpeg() As String
            Dim dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
            For depth As Integer = 0 To 10
                If dir Is Nothing Then Exit For
                Dim candidate As String = Path.Combine(dir.FullName, "Overlay", "API-Core", "ffmpeg.exe")
                If File.Exists(candidate) Then Return candidate
                Dim binCandidate As String = Path.Combine(dir.FullName, "Overlay", "bin", "Release",
                                                          "net10.0-windows10.0.26100.0", "FFmpeg", "ffmpeg.exe")
                If File.Exists(binCandidate) Then Return binCandidate
                dir = dir.Parent
            Next
            Return ""
        End Function

        Friend Function FfmpegCount() As Integer
            Try
                Dim n As Integer = 0
                For Each p As Process In Process.GetProcessesByName("ffmpeg")
                    Try
                        If Not p.HasExited Then n += 1
                    Catch
                    End Try
                    Try : p.Dispose() : Catch : End Try
                Next
                Return n
            Catch
                Return 0
            End Try
        End Function

        ''' <summary>Synthetic CFR fixture via the REAL ffmpeg (testsrc):
        ''' perfectly gridded at <paramref name="fps"/>, <paramref name="sec"/>s.</summary>
        Friend Function GenerateTestSrc(fps As Integer, sec As Double) As String
            Dim outPath As String = Path.Combine(_sandbox, $"testsrc_{fps}fps_{sec:0.00}s.mp4")
            Dim psi As New ProcessStartInfo With {
                .FileName = _ffmpeg,
                .Arguments = $"-y -v error -f lavfi -i testsrc=duration={sec.ToString(Globalization.CultureInfo.InvariantCulture)}:size=320x240:rate={fps} " &
                             $"-c:v libx264 -preset ultrafast -g {fps} -pix_fmt yuv420p ""{outPath}""",
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True, .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                p.StandardError.ReadToEnd()
                If Not p.WaitForExit(30000) Then
                    Try : p.Kill() : Catch : End Try
                    Throw New Exception($"testsrc fixture generation failed at {fps}fps")
                End If
                If p.ExitCode <> 0 Then Throw New Exception($"testsrc fixture exit {p.ExitCode} at {fps}fps")
            End Using
            Return outPath
        End Function

        Friend Sub SetEnv(name As String, value As String)
            Environment.SetEnvironmentVariable(name, value)
        End Sub

        Friend Sub ClearHelperEnv()
            SetEnv("LMHLP_SRC", Nothing)
            SetEnv("LMHLP_COPYTO", Nothing)
            SetEnv("LMHLP_CREATE_EMPTY", Nothing)
            SetEnv("LMHLP_SLEEP", Nothing)
            SetEnv("LMHLP_EXIT", Nothing)
        End Sub

        ''' <summary>Legacy engine settings for the helper seam (encoder string
        ''' is irrelevant — the helper never encodes; video-only mode keeps
        ''' the assertion focused on the single-process path).</summary>
        Friend Function HelperSettings(fps As Integer) As NVIDIA_Capture.CaptureSettings
            Dim s As New NVIDIA_Capture.CaptureSettings()
            s.FFmpegPath = Environment.ProcessPath
            s.Encoder = "libx264"
            s.CaptureMethod = "ddagrab"
            s.FPS = fps
            s.Bitrate = 2000000L
            s.UseNativeResolution = True
            s.OutputDirectory = _sandbox
            s.SystemAudioCapture = False       ' single-process video-only
            s.MicCapture = False
            Return s
        End Function

        ''' <summary>Legacy engine settings for the REAL QSV capture lane —
        ''' the production default mapping from M2-W3 (ddagrab → hwdownload →
        ''' h264_qsv cbr), audio disabled for the video-only mode.</summary>
        Friend Function QsvSettings(fps As Integer) As NVIDIA_Capture.CaptureSettings
            Dim s As New NVIDIA_Capture.CaptureSettings()
            s.FFmpegPath = _ffmpeg
            s.Encoder = "h264_qsv"
            s.CaptureMethod = "ddagrab"
            s.FPS = fps
            s.Bitrate = 20000000L
            s.NvencPreset = 4
            s.RateControl = "cbr"
            s.PixelFormat = "nv12"
            s.UseNativeResolution = True
            s.OutputDirectory = _sandbox
            s.SystemAudioCapture = False       ' single-process video-only
            s.MicCapture = False
            Return s
        End Function

    End Module

    ''' <summary>The suite exe acting as the engine's "ffmpeg".</summary>
    Friend Module HelperMode

        Friend Function Run(args As String()) As Integer
            ' Probe-shaped invocations ("-v error -i ...") — video-only engine
            ' paths do not spawn these today; answer honestly and cheaply.
            If args.Length > 0 AndAlso args(0) = "-v" Then
                Console.Error.WriteLine("[LMHLP] probe mode → exit 0")
                Return 0
            End If

            Dim dst As String = Environment.GetEnvironmentVariable("LMHLP_COPYTO")
            If Environment.GetEnvironmentVariable("LMHLP_CREATE_EMPTY") = "1" Then
                Try
                    Using fs As FileStream = File.Create(dst)
                        ' deliberately 0 bytes — the garbage fixture
                    End Using
                    Console.Error.WriteLine("[LMHLP] created EMPTY output " & dst)
                Catch ex As Exception
                    Console.Error.WriteLine("[LMHLP] create-empty failed: " & ex.Message)
                End Try
            Else
                Dim src As String = Environment.GetEnvironmentVariable("LMHLP_SRC")
                If Not String.IsNullOrEmpty(src) AndAlso Not String.IsNullOrEmpty(dst) Then
                    Try
                        File.Copy(src, dst, True)
                        Console.Error.WriteLine("[LMHLP] fixture copied → " & dst)
                    Catch ex As Exception
                        Console.Error.WriteLine("[LMHLP] copy failed: " & ex.Message)
                    End Try
                End If
            End If

            Console.Error.WriteLine("[LMHLP] Output #0, mp4, to 'helper'")

            Dim sleepS As Integer = 0
            Integer.TryParse(Environment.GetEnvironmentVariable("LMHLP_SLEEP"), sleepS)

            Dim quitReceived As New ManualResetEvent(False)
            Dim stdinReader As New Thread(
                Sub()
                    Try
                        While True
                            Dim line As String = Console.ReadLine()
                            If line Is Nothing Then Exit While
                            If line.Trim().StartsWith("q", StringComparison.Ordinal) Then
                                quitReceived.Set()
                                Exit While
                            End If
                        End While
                    Catch
                    End Try
                End Sub) With {.IsBackground = True}
            stdinReader.Start()

            If quitReceived.WaitOne(Math.Max(0, sleepS) * 1000) Then
                Console.Error.WriteLine("[LMHLP] quit received — graceful exit")
            Else
                Console.Error.WriteLine("[LMHLP] sleep elapsed")
            End If

            Dim exitCode As Integer = 0
            Integer.TryParse(Environment.GetEnvironmentVariable("LMHLP_EXIT"), exitCode)
            Console.Error.WriteLine("[LMHLP] record exit " & exitCode)
            Return exitCode
        End Function

    End Module

End Namespace
