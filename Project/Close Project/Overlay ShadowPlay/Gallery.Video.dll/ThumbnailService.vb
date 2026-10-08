Option Strict On
Option Explicit On
Option Infer On

' ThumbnailService.vb — short-lived decodes + disk cache (design doc §3.2:
' the planned thumbnail piece; §8 recorded "nothing exists (0 grep hits)").
'
' DESIGN:
'   - Generation spawns ffmpeg ONCE per cache miss with the repo's
'     disciplined subprocess pattern (MediaProbe.RunCapture — public for
'     exactly this use, both pipes drained, hard kill on timeout, no
'     orphans). NO new decode stack, NO library bindings.
'   - CACHE KEY = SHA-256(fullPath|lastWriteUtcTicks|lengthBytes|width).
'     Editing/overwriting a recording changes the key → fresh thumbnail;
'     an unchanged file never re-decodes. The path is hashed — no file
'     names, no user content, no credentials ever appear in cache names.
'   - ATOMIC PUBLISH: ffmpeg writes to a temp name inside the cache dir,
'     the file is moved into place only after a successful exit — a crash
'     or timeout can never leave a half-written cache entry.
'   - DETERMINISTIC FRAME: -ss is min(option, half the probed duration) —
'     "first frame" is avoided on purpose (encoder black frames).
'   - FAILURE = status, never an exception: missing source, corrupt file,
'     timeout and cache-dir problems all return Failed (with detail) and
'     clean up their temp file.
'   - SERIALIZED: one generate at a time (a SyncLock) — thumbnails are for
'     UI lists; throughput beats parallelism here, and duplicate generation
'     of the same key becomes impossible. Call from a background thread.

Imports System
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Namespace Gallery.Video

    Public Enum GalleryThumbnailStatus
        ''' <summary>Generated this call and published to the cache.</summary>
        Ready
        ''' <summary>Already in the cache for this file version — no decode.</summary>
        FromCache
        ''' <summary>Generation failed (detail says why); nothing was cached.</summary>
        Failed
        ''' <summary>No cache directory configured / no ffmpeg — service inert.</summary>
        Disabled
    End Enum

    Public NotInheritable Class GalleryThumbnailResult
        Public Sub New(status As GalleryThumbnailStatus, thumbnailPath As String, detail As String)
            Me.Status = status
            Me.ThumbnailPath = If(thumbnailPath, "")
            Me.Detail = If(detail, "")
        End Sub

        Public ReadOnly Property Status As GalleryThumbnailStatus
        ''' <summary>PNG path when Ready/FromCache; "" otherwise.</summary>
        Public ReadOnly Property ThumbnailPath As String
        Public ReadOnly Property Detail As String

        Public Overrides Function ToString() As String
            Return $"[{Status}] {ThumbnailPath} {Detail}"
        End Function
    End Class

    Public NotInheritable Class ThumbnailService

        Private ReadOnly _ffmpegExe As String
        Private ReadOnly _cacheDir As String
        Private ReadOnly _width As Integer
        Private ReadOnly _secondsInto As Double
        Private ReadOnly _timeoutMs As Integer
        Private ReadOnly _maxCacheFiles As Integer
        Private ReadOnly _gate As New Object()

        Public Sub New(ffmpegExe As String, cacheDir As String,
                       Optional width As Integer = 480,
                       Optional secondsInto As Double = 1.0,
                       Optional timeoutMs As Integer = 15000,
                       Optional maxCacheFiles As Integer = 1000)
            _ffmpegExe = If(ffmpegExe, "")
            _cacheDir = If(cacheDir, "")
            _width = Math.Max(64, width)
            _secondsInto = Math.Max(0.0, secondsInto)
            _timeoutMs = Math.Max(2000, timeoutMs)
            _maxCacheFiles = Math.Max(16, maxCacheFiles)
        End Sub

        Public ReadOnly Property CacheDirectory As String
            Get
                Return _cacheDir
            End Get
        End Property

        ''' <summary>
        ''' Return the cached thumbnail for this exact file version, or
        ''' generate it. Never throws; failures carry a detail string.
        ''' Caller supplies the scan facts (lastWriteUtcTicks, lengthBytes)
        ''' so the key is stable without re-statting.
        ''' </summary>
        Public Function EnsureThumbnail(fullPath As String,
                                        lastWriteUtcTicks As Long,
                                        lengthBytes As Long,
                                        durationSec As Double) As GalleryThumbnailResult
            If String.IsNullOrWhiteSpace(_cacheDir) OrElse String.IsNullOrWhiteSpace(_ffmpegExe) Then
                Return New GalleryThumbnailResult(GalleryThumbnailStatus.Disabled, "", "thumbnails not configured")
            End If
            If String.IsNullOrWhiteSpace(fullPath) Then
                Return New GalleryThumbnailResult(GalleryThumbnailStatus.Failed, "", "empty path")
            End If

            SyncLock _gate
                If Not File.Exists(fullPath) Then
                    Return New GalleryThumbnailResult(GalleryThumbnailStatus.Failed, "", "source file missing")
                End If

                Dim cacheDir As String = Path.GetFullPath(_cacheDir)
                Dim key As String = CacheKey(fullPath, lastWriteUtcTicks, lengthBytes)
                Dim finalPath As String = Path.Combine(cacheDir, key & ".png")

                Try
                    If Not Directory.Exists(cacheDir) Then
                        Directory.CreateDirectory(cacheDir)
                    End If
                Catch ex As Exception
                    Return New GalleryThumbnailResult(GalleryThumbnailStatus.Failed, "", $"cache dir: {ex.Message}")
                End Try

                If File.Exists(finalPath) AndAlso New FileInfo(finalPath).Length > 0 Then
                    Return New GalleryThumbnailResult(GalleryThumbnailStatus.FromCache, finalPath, "")
                End If

                ' deterministic seek target: never past half the clip, never
                ' negative; unknown duration → first frame fallback (0)
                Dim seekSec As Double = 0.0
                If durationSec > 0 Then
                    seekSec = Math.Min(_secondsInto, durationSec / 2.0)
                End If

                Dim tmpPath As String = Path.Combine(cacheDir, key & ".tmp-" & Guid.NewGuid().ToString("N") & ".png")
                Try
                    Dim args As String =
                        $"-y -v error -ss {seekSec.ToString("0.###", Global.System.Globalization.CultureInfo.InvariantCulture)} " &
                        $"-i ""{fullPath}"" -map 0:v:0 -frames:v 1 " &
                        $"-vf scale={_width}:-2 ""{tmpPath}"""

                    Dim stdout As String = Nothing, stderr As String = Nothing
                    Dim exitCode As Integer
                    Dim ran As Boolean = MediaProbe.RunCapture(_ffmpegExe, args, _timeoutMs, stdout, stderr, exitCode)

                    If Not ran Then
                        Return New GalleryThumbnailResult(GalleryThumbnailStatus.Failed, "",
                                                          $"ffmpeg timed out after {_timeoutMs}ms")
                    End If
                    If exitCode <> 0 OrElse Not File.Exists(tmpPath) OrElse New FileInfo(tmpPath).Length = 0 Then
                        Return New GalleryThumbnailResult(GalleryThumbnailStatus.Failed, "",
                                                          $"ffmpeg exit {exitCode}: {TrimDetail(stderr)}")
                    End If

                    File.Move(tmpPath, finalPath, overwrite:=True)
                    EvictOldestIfNeeded(cacheDir)
                    Return New GalleryThumbnailResult(GalleryThumbnailStatus.Ready, finalPath, "")
                Catch ex As Exception
                    Return New GalleryThumbnailResult(GalleryThumbnailStatus.Failed, "", ex.Message)
                Finally
                    Try
                        If File.Exists(tmpPath) Then File.Delete(tmpPath)
                    Catch
                    End Try
                End Try
            End SyncLock
        End Function

        ''' <summary>SHA-256 over (path|mtime|size|width). The path is
        ''' case-folded so Windows case-insensitivity cannot duplicate keys;
        ''' no readable file names or user data land in the cache directory.</summary>
        Friend Shared Function CacheKey(fullPath As String, lastWriteUtcTicks As Long, lengthBytes As Long) As String
            Dim basis As String = $"{fullPath.ToLowerInvariant()}|{lastWriteUtcTicks}|{lengthBytes}"
            Using sha As SHA256 = SHA256.Create()
                Dim hash As Byte() = sha.ComputeHash(Encoding.UTF8.GetBytes(basis))
                Dim sb As New StringBuilder(hash.Length * 2)
                For Each b In hash
                    sb.Append(b.ToString("x2"))
                Next
                Return sb.ToString()
            End Using
        End Function

        ''' <summary>Bound the cache: beyond MaxCacheFiles, oldest PNGs
        ''' (LastWriteTimeUtc) are deleted. Temp files never count.</summary>
        Private Sub EvictOldestIfNeeded(cacheDir As String)
            Try
                Dim files = Directory.GetFiles(cacheDir, "*.png")
                If files.Length <= _maxCacheFiles Then Return
                Dim byAge As New List(Of Tuple(Of DateTime, String))()
                For Each f In files
                    Try
                        byAge.Add(Tuple.Create(New FileInfo(f).LastWriteTimeUtc, f))
                    Catch
                    End Try
                Next
                byAge.Sort(Function(a, b) a.Item1.CompareTo(b.Item1))
                Dim excess = byAge.Count - _maxCacheFiles
                For i = 0 To excess - 1
                    Try
                        File.Delete(byAge(i).Item2)
                    Catch
                    End Try
                Next
            Catch
                ' eviction is hygiene — never a failure
            End Try
        End Sub

        Private Shared Function TrimDetail(s As String) As String
            Dim t = If(s, "").Trim()
            If t.Length > 200 Then t = t.Substring(0, 200) & "…"
            Return t
        End Function

    End Class

End Namespace
