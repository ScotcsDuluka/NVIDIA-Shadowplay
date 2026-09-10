Option Strict On
Option Explicit On
Option Infer On

' TestMedia.vb — binary locator + deterministic synthetic media factory.
'
' OWNER RULE: format support is proven from real files. The product matrix
' validated in HANDOFF §3 (MP4 / H.264 / yuv420p / CFR / AAC / start_time=0)
' is reproduced here with real ffmpeg so integration tests exercise the same
' codec families WITHOUT assuming anything the probe does not confirm.
'
' Determinism: generated once per sandbox, fixed content (testsrc2 + sine),
' fixed frame counts. Same inputs every run → same probe/decode results.

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend Module TestMedia

        Friend FfmpegPath As String = ""
        Friend FfprobePath As String = ""
        Friend Sandbox As String = ""

        ''' <summary>Locate ffmpeg/ffprobe: env GALLERY_FFMPEG_DIR first (dir with
        ''' both), then PATH. Sets "" when unusable — callers then SKIP honestly.</summary>
        Friend Sub LocateBinaries()
            Dim dir = Environment.GetEnvironmentVariable("GALLERY_FFMPEG_DIR")
            Dim ffExe = "ffmpeg", fpExe = "ffprobe"
            If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                ffExe = "ffmpeg.exe"
                fpExe = "ffprobe.exe"
            End If

            If Not String.IsNullOrWhiteSpace(dir) AndAlso Directory.Exists(dir) Then
                Dim a = Path.Combine(dir, ffExe)
                Dim b = Path.Combine(dir, fpExe)
                If File.Exists(a) AndAlso File.Exists(b) Then
                    FfmpegPath = a
                    FfprobePath = b
                End If
            End If

            If FfmpegPath = "" Then
                ' PATH probe (FFmpegLocator validates by running -version)
                Dim pathDirs = Environment.GetEnvironmentVariable("PATH")
                If Not String.IsNullOrEmpty(pathDirs) Then
                    For Each d In pathDirs.Split(":"c, ";"c)
                        If String.IsNullOrWhiteSpace(d) Then Continue For
                        Dim a = Path.Combine(d.Trim(), ffExe)
                        Dim b = Path.Combine(d.Trim(), fpExe)
                        If File.Exists(a) AndAlso File.Exists(b) Then
                            If FFmpegLocator.IsUsableFFmpeg(a) Then
                                FfmpegPath = a
                                FfprobePath = b
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If

            If Sandbox = "" Then
                Sandbox = Path.Combine(Path.GetTempPath(), "gallery-video-tests-" & Environment.ProcessId.ToString())
                Directory.CreateDirectory(Sandbox)
            End If
        End Sub

        Friend ReadOnly Property BinariesAvailable As Boolean
            Get
                Return FfmpegPath <> "" AndAlso FfprobePath <> ""
            End Get
        End Property

        ''' <summary>Skip helper when binaries are missing.</summary>
        Friend Sub RequireBinaries()
            If Not BinariesAvailable Then
                Throw New SkipException("ffmpeg/ffprobe not found (set GALLERY_FFMPEG_DIR to a dir containing both)")
            End If
        End Sub

        ''' <summary>Generate (once) and return the path of a synthetic MP4.</summary>
        Friend Function Synthetic(name As String, kind As String) As String
            RequireBinaries()
            Dim p = Path.Combine(Sandbox, name)
            If File.Exists(p) AndAlso New FileInfo(p).Length > 1024 Then Return p

            Dim args As String
            Select Case kind
                Case "av60"
                    ' Product matrix shape: H.264 yuv420p 60fps CFR + AAC 48k stereo, start 0
                    ' (-ac 2: the product records stereo; `sine` is mono by default)
                    args = "-y -f lavfi -i testsrc2=size=320x240:rate=60:duration=4 " &
                           "-f lavfi -i sine=frequency=1000:sample_rate=48000:duration=4 " &
                           "-c:v libx264 -pix_fmt yuv420p -g 60 " &
                           "-c:a aac -b:a 128k -ac 2 -shortest """ & p & """"
                Case "video_only"
                    args = "-y -f lavfi -i testsrc2=size=320x240:rate=30:duration=2 " &
                           "-c:v libx264 -pix_fmt yuv420p -g 30 """ & p & """"
                Case "audio_only"
                    args = "-y -f lavfi -i sine=frequency=440:sample_rate=48000:duration=2 " &
                           "-c:a aac -b:a 128k """ & p & """"
                Case "av_bigger"
                    args = "-y -f lavfi -i testsrc2=size=1280x720:rate=60:duration=6 " &
                           "-f lavfi -i sine=frequency=800:sample_rate=48000:duration=6 " &
                           "-c:v libx264 -pix_fmt yuv420p -g 120 " &
                           "-c:a aac -b:a 128k -shortest """ & p & """"
                Case "real_shape"
                    ' Pinned to the REAL ShadowPlay recording (design doc §5):
                    ' Record_2026-09-10_21-18-52.mp4 measured on the owner box —
                    ' H.264 yuv420p 1680×1050 (16:10, both even), ~60fps, AAC LC
                    ' 48k stereo, mov/mp4. Short duration: full-res decode in
                    ' tests must stay fast.
                    args = "-y -f lavfi -i testsrc2=size=1680x1050:rate=60:duration=2 " &
                           "-f lavfi -i sine=frequency=1000:sample_rate=48000:duration=2 " &
                           "-c:v libx264 -pix_fmt yuv420p -g 60 " &
                           "-c:a aac -b:a 128k -ac 2 -shortest """ & p & """"
                Case "wrong_codec"
                    ' MPEG-4 Part 2 — deliberately OUTSIDE the supported set
                    ' (§7: probe gate → Faulted.UnsupportedFormat). Small + fast.
                    args = "-y -f lavfi -i testsrc2=size=320x240:rate=30:duration=1 " &
                           "-c:v mpeg4 -pix_fmt yuv420p -q:v 10 """ & p & """"
                Case Else
                    Throw New ArgumentException("unknown synthetic kind: " & kind)
            End Select

            Dim stdout As String = Nothing, stderr As String = Nothing, code As Integer = -1
            If Not MediaProbe.RunCapture(FfmpegPath, args, 120000, stdout, stderr, code) OrElse code <> 0 Then
                Throw New Exception("ffmpeg synth generation failed: " & stderr)
            End If
            Return p
        End Function

        ''' <summary>Truncated-but-real MP4 (moov lost) — the corrupt-file case.</summary>
        Friend Function CorruptFile() As String
            Dim good = Synthetic("synth_av60.mp4", "av60")
            Dim p = Path.Combine(Sandbox, "corrupt_truncated.mp4")
            Dim src = File.ReadAllBytes(good)
            Dim keep = Math.Max(256, src.Length \ 3)
            Dim dst(src.Length - 1) As Byte ' same size, mostly zeros beyond cut
            Array.Copy(src, dst, keep)
            File.WriteAllBytes(p, dst)
            Return p
        End Function

        Friend Function NotMediaFile() As String
            Dim p = Path.Combine(Sandbox, "not_media.mp4")
            File.WriteAllBytes(p, New Byte() {&H12, &H34, &H56, &H78, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8})
            Return p
        End Function

    End Module

End Namespace
