Option Strict On
Option Explicit On
Option Infer On

' MediaProbe.vb — ffprobe JSON → MediaInfo (design doc §3.2, §5).
'
' Subprocess discipline copied from repo precedent:
'   - FFmpegLocator.vb: async pipe drain (undrained 64KB stderr deadlocks),
'     bounded WaitForExit, binary sanity BEFORE trusting the spawn.
'   - CaptureSession.vb: ffprobe is the metadata authority.

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.Json

Namespace Gallery.Video

    Public NotInheritable Class MediaProbe

        ''' <summary>Timeout for ffprobe spawn. Cold AV first-scan is the slowest
        ''' legitimate case (same rationale as FFmpegLocator.ProbeTimeoutMs).</summary>
        Public Shared Property ProbeTimeoutMs As Integer = 10000

        ''' <summary>
        ''' Probe a media file with ffprobe. Returns Nothing (never throws) when:
        '''   - file missing / unreadable, or
        '''   - ffprobe binary unusable, or
        '''   - ffprobe rejects the file (corrupt/unsupported container), or
        '''   - spawn timed out (child killed, no orphan).
        ''' The CALLER decides which GalleryVideoFaultKind each Nothing maps to
        ''' (pre-checks distinguish missing/locked before calling this).
        ''' </summary>
        Public Shared Function Probe(path As String, ffprobeExe As String) As MediaInfo
            If String.IsNullOrWhiteSpace(path) OrElse String.IsNullOrWhiteSpace(ffprobeExe) Then Return Nothing
            If Not File.Exists(path) Then Return Nothing
            If Not FFmpegLocator.IsUsableFFmpeg(ffprobeExe) Then Return Nothing

            Dim args As String = ProbeArgs(path)
            Dim stdout As String = Nothing
            Dim stderr As String = Nothing
            Dim exitCode As Integer

            Try
                If Not RunCapture(ffprobeExe, args, ProbeTimeoutMs, stdout, stderr, exitCode) Then Return Nothing
            Catch ex As Exception
                ' Locked file typically surfaces here as IOException/Win32Exception
                Throw New ProbeIOException(path, ex)
            End Try

            If exitCode <> 0 Then Return Nothing ' ffprobe rejected the file
            If String.IsNullOrWhiteSpace(stdout) Then Return Nothing

            Try
                Return ParseJson(path, stdout)
            Catch ex As Exception
                ' Malformed JSON from a weird build — treat as probe failure.
                Throw New ProbeIOException(path, ex)
            End Try
        End Function

        Public NotInheritable Class ProbeIOException
            Inherits Exception
            Public ReadOnly Property ProbedPath As String
            Public Sub New(path As String, inner As Exception)
                MyBase.New(inner?.Message, inner)
                Me.ProbedPath = path
            End Sub
        End Class

        ''' <summary>Extracted for tests: the exact ffprobe contract line.</summary>
        Public Shared Function ProbeArgs(path As String) As String
            ' -v quiet + JSON; both streams (format + streams); buffered read.
            Return $"-v quiet -print_format json -show_format -show_streams ""{path}"""
        End Function

        ''' <summary>Parse ffprobe JSON (tolerant: missing nodes → defaults,
        ''' format.duration/stream fields may be absent per stream type).
        ''' NOTE: never reuse the target element as the source — TryGetProperty
        ''' output variables must be FRESH locals (self-shadowing turns a
        ''' string element into the next lookup's root and throws).</summary>
        Public Shared Function ParseJson(path As String, json As String) As MediaInfo
            Dim info As New MediaInfo With {.FilePath = path}
            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim rootEl = doc.RootElement

                Dim formatEl As JsonElement
                If rootEl.TryGetProperty("format", formatEl) AndAlso formatEl.ValueKind = JsonValueKind.Object Then
                    Dim propEl As JsonElement
                    If formatEl.TryGetProperty("format_name", propEl) Then info.Format.FormatName = propEl.GetString()
                    If formatEl.TryGetProperty("duration", propEl) Then
                        Double.TryParse(propEl.GetString(),
                                        Global.System.Globalization.NumberStyles.Float,
                                        Global.System.Globalization.CultureInfo.InvariantCulture,
                                        info.Format.DurationSec)
                    End If
                    If formatEl.TryGetProperty("size", propEl) Then
                        Long.TryParse(propEl.GetString(), info.Format.SizeBytes)
                    End If
                    If formatEl.TryGetProperty("bit_rate", propEl) Then
                        Long.TryParse(propEl.GetString(), info.Format.BitRate)
                    End If
                End If

                Dim streamsEl As JsonElement
                If rootEl.TryGetProperty("streams", streamsEl) AndAlso streamsEl.ValueKind = JsonValueKind.Array Then
                    For Each streamEl In streamsEl.EnumerateArray()
                        Dim si As New StreamInfo()
                        Dim propEl As JsonElement

                        If streamEl.TryGetProperty("index", propEl) Then si.Index = propEl.GetInt32()
                        If streamEl.TryGetProperty("codec_type", propEl) Then si.CodecType = propEl.GetString()
                        If streamEl.TryGetProperty("codec_name", propEl) Then si.CodecName = propEl.GetString()
                        If streamEl.TryGetProperty("profile", propEl) Then si.Profile = propEl.GetString()
                        If streamEl.TryGetProperty("width", propEl) Then si.Width = propEl.GetInt32()
                        If streamEl.TryGetProperty("height", propEl) Then si.Height = propEl.GetInt32()
                        If streamEl.TryGetProperty("pix_fmt", propEl) Then si.PixFmt = propEl.GetString()
                        If streamEl.TryGetProperty("avg_frame_rate", propEl) Then si.AvgFrameRate = propEl.GetString()
                        If streamEl.TryGetProperty("time_base", propEl) Then si.TimeBase = propEl.GetString()
                        If streamEl.TryGetProperty("start_time", propEl) Then si.StartTime = propEl.GetString()
                        If streamEl.TryGetProperty("duration", propEl) Then si.Duration = propEl.GetString()
                        If streamEl.TryGetProperty("sample_rate", propEl) Then
                            Integer.TryParse(propEl.GetString(), si.SampleRate)
                        End If
                        If streamEl.TryGetProperty("channels", propEl) Then si.Channels = propEl.GetInt32()
                        If streamEl.TryGetProperty("channel_layout", propEl) Then si.ChannelLayout = propEl.GetString()
                        info.Streams.Add(si)
                    Next
                End If
            End Using
            Return info
        End Function

        ''' <summary>Synchronous process run with BOTH pipes drained asynchronously
        ''' and a hard kill on timeout (no orphans — FFmpegLocator lesson).
        ''' Public: tests + thumbnail service use the same disciplined spawn.</summary>
        Public Shared Function RunCapture(fileName As String, args As String, timeoutMs As Integer,
                                          ByRef stdout As String, ByRef stderr As String,
                                          ByRef exitCode As Integer) As Boolean
            stdout = Nothing
            stderr = Nothing
            exitCode = -1

            Dim psi As New ProcessStartInfo With {
                .FileName = fileName,
                .Arguments = args,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .StandardOutputEncoding = Encoding.UTF8
            }

            Dim p As Process = Process.Start(psi)
            If p Is Nothing Then Return False

            Try
                Dim soTask = p.StandardOutput.ReadToEndAsync()
                Dim seTask = p.StandardError.ReadToEndAsync()
                If Not p.WaitForExit(timeoutMs) Then
                    Try
                        p.Kill(entireProcessTree:=True)
                    Catch
                    End Try
                    Try
                        p.WaitForExit(2000)
                    Catch
                    End Try
                    Return False
                End If
                stdout = soTask.GetAwaiter().GetResult()
                stderr = seTask.GetAwaiter().GetResult()
                exitCode = p.ExitCode
                Return True
            Finally
                p.Dispose()
            End Try
        End Function

    End Class

End Namespace
