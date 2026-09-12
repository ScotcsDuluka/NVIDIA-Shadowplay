Option Strict On
Option Explicit On
Option Infer On

' GalleryMediaStore.vb — W2 Gallery UI data layer (UI-side ONLY).
'
' Scope discipline (owner spec): this file is presentation-support code for
' the Gallery screen. It uses the EXISTING Gallery.Video API (MediaProbe for
' metadata) and the EXISTING ffmpeg binary (via FFmpegLocator / AppSettings)
' for thumbnail frame extraction. It adds NO backend contract to
' Gallery.Video and touches no playback-engine semantics — the playback
' engine is consumed exclusively through PlaybackSession's public API.
'
' Responsibilities:
'   - folder scan (*.mp4, newest first)
'   - lazy per-file metadata (MediaProbe — duration/resolution)
'   - thumbnail extraction with a bounded disk cache
'   - bounded concurrency + cancellation for all background work

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Threading
Imports System.Threading.Tasks
Imports Gallery.Video

Friend NotInheritable Class GalleryMediaItem

    Public Property FilePath As String = ""
    Public Property DisplayName As String = ""
    Public Property LengthBytes As Long = 0
    Public Property ModifiedUtc As DateTime = DateTime.MinValue

    ' Filled lazily by probe (0 = unknown / probe failed — never blocks the grid)
    Public Property DurationSec As Double = 0
    Public Property Resolution As String = ""

    Public ReadOnly Property SizeText As String
        Get
            If LengthBytes >= 1073741824L Then
                Return (LengthBytes / 1073741824.0).ToString("0.##", CultureInfo.InvariantCulture) & " GB"
            End If
            Return (LengthBytes / 1048576.0).ToString("0.#", CultureInfo.InvariantCulture) & " MB"
        End Get
    End Property

    Public ReadOnly Property DurationText As String
        Get
            If DurationSec <= 0 Then Return ""
            Dim ts = TimeSpan.FromSeconds(DurationSec)
            If ts.TotalHours >= 1 Then
                Return ts.Hours.ToString("0") & ":" & ts.Minutes.ToString("00") & ":" & ts.Seconds.ToString("00")
            End If
            Return ts.Minutes.ToString("0") & ":" & ts.Seconds.ToString("00")
        End Get
    End Property

End Class

''' <summary>Folder scan + metadata + thumbnails for the Gallery screen.
''' All heavy work is Task-based with cancellation; the UI thread only
''' receives completed results through the returned tasks.</summary>
Friend NotInheritable Class GalleryMediaStore

    ' Thumbnail extraction is a short-lived ffmpeg subprocess per file —
    ' bounded to 2 concurrent so scrolling a big folder cannot stampede.
    Private Shared ReadOnly _thumbGate As New SemaphoreSlim(2, 2)

    Private ReadOnly _ffmpegPath As String

    Public Sub New(ffmpegPath As String)
        _ffmpegPath = If(ffmpegPath, "")
    End Sub

    Public ReadOnly Property HasFFmpeg As Boolean
        Get
            Return FFmpegLocator.IsUsableFFmpeg(_ffmpegPath)
        End Get
    End Property

    ''' <summary>Scan `folder` for MP4 recordings, newest first. Missing or
    ''' unreadable folder → empty list (the caller renders the error state
    ''' from the exception-free contract: empty + folderExists flag).</summary>
    Public Function ScanAsync(folder As String, ct As CancellationToken) As Task(Of List(Of GalleryMediaItem))
        Return Task.Run(
            Function()
                Dim items As New List(Of GalleryMediaItem)()
                If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then Return items
                For Each f In Directory.EnumerateFiles(folder, "*.mp4", SearchOption.TopDirectoryOnly)
                    ct.ThrowIfCancellationRequested()
                    Try
                        Dim fi As New FileInfo(f)
                        If fi.Length <= 0 Then Continue For
                        items.Add(New GalleryMediaItem With {
                            .FilePath = f,
                            .DisplayName = Path.GetFileNameWithoutExtension(f),
                            .LengthBytes = fi.Length,
                            .ModifiedUtc = fi.LastWriteTimeUtc
                        })
                    Catch
                        ' A file vanishing mid-scan is normal — skip it.
                    End Try
                Next
                items.Sort(Function(a, b) b.ModifiedUtc.CompareTo(a.ModifiedUtc))
                Return items
            End Function, ct)
    End Function

    ''' <summary>Lazy metadata: duration + resolution via the EXISTING
    ''' MediaProbe (ffprobe JSON). Never throws; failures leave 0/"".</summary>
    Public Function ProbeAsync(item As GalleryMediaItem, ct As CancellationToken) As Task
        Dim ffprobe = Path.Combine(Path.GetDirectoryName(_ffmpegPath), "ffprobe.exe")
        Return Task.Run(
            Sub()
                Try
                    Dim info = MediaProbe.Probe(item.FilePath, ffprobe)
                    If info Is Nothing OrElse Not info.HasVideo Then Return
                    item.DurationSec = Math.Max(0.0, info.Format.DurationSec)
                    item.Resolution = info.Video.Width.ToString(CultureInfo.InvariantCulture) & "×" &
                                      info.Video.Height.ToString(CultureInfo.InvariantCulture)
                Catch
                    ' Probe failure is a tile-level concern — never a crash.
                End Try
            End Sub, ct)
    End Function

    ''' <summary>Extract one frame (~1s in) as the tile thumbnail, cached on
    ''' disk by (path, mtime, size). Returns the cached image path, or Nothing
    ''' when ffmpeg is missing / extraction failed (tile falls back).</summary>
    Public Function GetThumbnailAsync(item As GalleryMediaItem, width As Integer, ct As CancellationToken) As Task(Of String)
        Return Task.Run(
            Function()
                Try
                    If Not FFmpegLocator.IsUsableFFmpeg(_ffmpegPath) Then Return Nothing

                    Dim cacheDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Duluka", "Gallery", "Thumbs")
                    Directory.CreateDirectory(cacheDir)

                    Dim key = item.FilePath & "|" & item.ModifiedUtc.Ticks.ToString(CultureInfo.InvariantCulture) &
                              "|" & item.LengthBytes.ToString(CultureInfo.InvariantCulture) & "|" & width.ToString(CultureInfo.InvariantCulture)
                    Dim hash As String = ""
                    Using sha = SHA256.Create()
                        hash = BitConverter.ToString(sha.ComputeHash(
                            System.Text.Encoding.UTF8.GetBytes(key))).Replace("-", "").ToLowerInvariant()
                    End Using
                    Dim cachePath = Path.Combine(cacheDir, hash & ".bmp")
                    If File.Exists(cachePath) AndAlso New FileInfo(cachePath).Length > 0 Then Return cachePath

                    ct.ThrowIfCancellationRequested()
                    _thumbGate.Wait(ct)
                    Try
                        If File.Exists(cachePath) AndAlso New FileInfo(cachePath).Length > 0 Then Return cachePath

                        Dim args = $"-y -v error -ss 1 -i ""{item.FilePath}"" -frames:v 1 " &
                                   $"-vf scale={width}:-2 -f image2 ""{cachePath}"""
                        Using p As New Process With {
                            .StartInfo = New ProcessStartInfo With {
                                .FileName = _ffmpegPath,
                                .Arguments = args,
                                .UseShellExecute = False,
                                .CreateNoWindow = True
                            }
                        }
                            p.Start()
                            ' Bounded wait: a wedged ffmpeg must never pin a tile.
                            If Not p.WaitForExit(20000) Then
                                Try : p.Kill(True) : Catch : End Try
                                Return Nothing
                            End If
                            If p.ExitCode <> 0 OrElse Not File.Exists(cachePath) Then Return Nothing
                        End Using
                        Return cachePath
                    Finally
                        _thumbGate.Release()
                    End Try
                Catch
                    Return Nothing
                End Try
            End Function, ct)
    End Function

End Class
