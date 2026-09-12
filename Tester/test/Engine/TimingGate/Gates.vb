Option Strict On
Option Explicit On
Option Infer On

' PtsProbe.vb — extracts the presentation timeline of a real MP4 with the
' bundled ffprobe (beside the product ffmpeg) and runs PtsAnalyzer on it.
'
' Gates.vb — environment capability preflights, per the C/4 truthfulness
' contract: a lane that cannot run on this machine reports BLOCKED with the
' probe's own evidence — never a PASS and never a swallowed failure.
'   QsvGate     — legacy ddagrab+h264_qsv lane (Intel machines).
'   NativeGate  — native RecordingEngine lane (Ddagrab+NVENC, NVIDIA only).

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Text.RegularExpressions
Imports NVIDIA_Capture
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab

Namespace TimingGate.Tests

    Friend Class SkipException
        Inherits Exception
        Public Sub New(reason As String)
            MyBase.New(reason)
        End Sub
    End Class

    Friend NotInheritable Class PtsProbe

        ''' <summary>Analyze a real file end-to-end. Throws SkipException when
        ''' ffprobe is unavailable (analyzer environment), plain Exception with
        ''' evidence when the file itself is un-decodable.</summary>
        Friend Shared Function AnalyzeFile(ffprobeExe As String, path As String,
                                           targetFps As Integer, requestedSec As Double,
                                           label As String) As PtsReport
            If Not File.Exists(path) Then
                Throw New Exception($"{label}: output file missing — {path}")
            End If
            If (New FileInfo(path)).Length = 0 Then
                Throw New Exception($"{label}: output file is EMPTY (0 bytes) — {path}")
            End If

            Dim meta As String = Probe(ffprobeExe,
                $"-v error -select_streams v:0 -show_entries stream=avg_frame_rate -show_entries format=duration -of default=noprint_wrappers=1 ""{path}""")
            Dim avgRate As String = ""
            Dim mRate As Match = Regex.Match(meta, "avg_frame_rate=(\S+)")
            If mRate.Success Then avgRate = mRate.Groups(1).Value
            Dim durSec As Double = 0.0
            Dim mDur As Match = Regex.Match(meta, "duration=([\d.]+)")
            If mDur.Success Then Double.TryParse(mDur.Groups(1).Value,
                Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, durSec)

            Dim raw As String = Probe(ffprobeExe,
                $"-v error -select_streams v:0 -show_entries frame=pts_time -of csv=p=0 ""{path}""")
            If raw.Contains("moov atom not found") OrElse raw.Contains("Invalid data") Then
                Throw New Exception($"{label}: container un-decodable — {FirstLine(raw)}")
            End If

            Dim pts As New List(Of Double)()
            For Each line As String In raw.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
                Dim v As Double = 0.0
                If line.StartsWith("N/A", StringComparison.Ordinal) Then Continue For
                ' ffprobe csv emits a trailing comma on some lines (observed on
                ' the first frame: "0.000000,") — strip it or the frame is lost.
                Dim cleaned As String = line.Trim().TrimEnd(","c)
                If cleaned.Length = 0 Then Continue For
                If Double.TryParse(cleaned, Globalization.NumberStyles.Float,
                                   Globalization.CultureInfo.InvariantCulture, v) Then
                    pts.Add(v)
                End If
            Next

            Dim packets As Integer = -1
            Dim pkRaw As String = Probe(ffprobeExe,
                $"-v error -select_streams v:0 -count_packets -show_entries stream=nb_read_packets -of csv=p=0 ""{path}""")
            Dim mPk As Match = Regex.Match(pkRaw, "(\d+)")
            If mPk.Success Then packets = CInt(mPk.Groups(1).Value)

            Return PtsAnalyzer.Analyze(pts.ToArray(), targetFps, requestedSec,
                                       durSec, avgRate, packets)
        End Function

        Private Shared Function Probe(ffprobeExe As String, args As String) As String
            If Not File.Exists(ffprobeExe) Then
                Throw New SkipException($"analyzer unavailable: ffprobe not found at {ffprobeExe}")
            End If
            Dim psi As New ProcessStartInfo With {
                .FileName = ffprobeExe, .Arguments = args,
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True, .RedirectStandardOutput = True
            }
            Using p As Process = Process.Start(psi)
                Dim outTask = p.StandardOutput.ReadToEndAsync()
                Dim errTask = p.StandardError.ReadToEndAsync()
                If Not p.WaitForExit(30000) Then
                    Try : p.Kill() : Catch : End Try
                    Throw New Exception("ffprobe timed out")
                End If
                Try : outTask.Wait(2000) : Catch : End Try
                Try : errTask.Wait(1000) : Catch : End Try
                Dim outS As String = If(outTask.Status = Threading.Tasks.TaskStatus.RanToCompletion, outTask.Result, "")
                Dim errS As String = If(errTask.Status = Threading.Tasks.TaskStatus.RanToCompletion, errTask.Result, "")
                Return outS & errS
            End Using
        End Function

        Friend Shared Function FirstLine(text As String) As String
            Dim one As String = If(text, "").Replace(vbCr, "").Replace(vbLf, " | ")
            Return If(one.Length > 220, one.Substring(0, 220) & "…", one)
        End Function
    End Class

    ''' <summary>Legacy QSV lane probe: can this machine's ffmpeg open
    ''' h264_qsv at all, and at which target fps? Each probe is a REAL
    ''' 0.2–0.4s testsrc encode — the verdict is the encoder's own.</summary>
    Friend Module QsvGate
        Private _probed As Boolean = False
        Private _qsvAvailable As Boolean = False
        Private _reason As String = ""

        Friend Sub EnsureProbed(ffmpegExe As String)
            If _probed Then Return
            _probed = True
            _qsvAvailable = ProbeFps(ffmpegExe, 15) AndAlso ProbeFps(ffmpegExe, 60)
            If Not _qsvAvailable Then _reason = "h264_qsv testsrc probe failed on this machine"
        End Sub

        Friend ReadOnly Property QsvAvailable As Boolean
            Get
                Return _qsvAvailable
            End Get
        End Property

        Friend ReadOnly Property Reason As String
            Get
                Return _reason
            End Get
        End Property

        ''' <summary>Does the QSV runtime ACCEPT this frame rate? Decides the
        ''' per-mode cell expectation: True → GRID assertions; False →
        ''' HONEST_FAILURE assertions (the mode must fail loudly and honestly).
        ''' M2-W3 evidence: Intel UHD 0x9B41 rejects 240 with "Current frame
        ''' rate is unsupported" at every bitrate probed.</summary>
        Friend Function ModeSupported(ffmpegExe As String, fps As Integer) As Boolean
            EnsureProbed(ffmpegExe)
            If Not _qsvAvailable Then Return False
            Return ProbeFps(ffmpegExe, fps)
        End Function

        Private Function ProbeFps(ffmpegExe As String, fps As Integer) As Boolean
            Dim psi As New ProcessStartInfo With {
                .FileName = ffmpegExe,
                .Arguments = $"-v error -f lavfi -i testsrc=duration=0.3:size=320x240:rate={fps} " &
                             $"-c:v h264_qsv -frames:v 4 -f null NUL",
                .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardError = True, .RedirectStandardOutput = True
            }
            Try
                Using p As Process = Process.Start(psi)
                    p.StandardError.ReadToEnd()
                    If Not p.WaitForExit(20000) Then
                        Try : p.Kill() : Catch : End Try
                        Return False
                    End If
                    Return p.ExitCode = 0
                End Using
            Catch
                Return False
            End Try
        End Function
    End Module

    ''' <summary>Native lane probe — same semantics as Engine.Concurrency's
    ''' HardwareGate (own copy: Friend types cannot cross test assemblies):
    ''' build the REAL DdagrabBackend once; its adapter enumeration IS the
    ''' evidence.</summary>
    Friend Module NativeGate
        Private _probed As Boolean = False
        Private _nativeAvailable As Boolean = False
        Private _reason As String = ""

        Friend Sub EnsureProbed()
            If _probed Then Return
            _probed = True
            Dim backend As DdagrabBackend = Nothing
            Try
                backend = New DdagrabBackend(New EngineLogger("native-preflight", EngineLogger.LogLevel.Warning))
                backend.Initialize(New PreflightContext())
                _nativeAvailable = True
            Catch ex As Exception
                _nativeAvailable = False
                _reason = ex.Message
            Finally
                If backend IsNot Nothing Then
                    Try : backend.Dispose() : Catch : End Try
                End If
            End Try
        End Sub

        Friend ReadOnly Property NativeAvailable As Boolean
            Get
                EnsureProbed()
                Return _nativeAvailable
            End Get
        End Property

        Friend ReadOnly Property Reason As String
            Get
                EnsureProbed()
                Return _reason
            End Get
        End Property

        Private NotInheritable Class PreflightContext
            Implements IVideoBackendContext

            Private ReadOnly _log As EngineLogger

            Public Sub New()
                _log = New EngineLogger("native-preflight", EngineLogger.LogLevel.Warning)
            End Sub

            Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
                Get
                    Return _log
                End Get
            End Property

            Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
                Get
                    Return VideoBackendKind.Ddagrab
                End Get
            End Property
        End Class
    End Module

End Namespace
