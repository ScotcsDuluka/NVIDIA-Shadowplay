Option Strict On
Option Explicit On
Option Infer On

' GalleryLibrary.vb — the ONE filesystem/ffprobe boundary of the Gallery data
' layer (W3, design doc §3.2 layout + §9 "Gallery file listing/index").
'
' CONTRACT WITH THE UI (W2):
'   - The UI NEVER touches the filesystem or spawns ffprobe/ffmpeg. It holds
'     a GalleryLibrary, reads Entries/Query, and calls Rescan/RescanAsync —
'     refresh results arrive via the LibraryChanged event.
'   - Every read returns an immutable snapshot in deterministic order
'     (primary key + full-path tie-break — never filesystem enumeration
'     order, which is unspecified).
'   - No method throws for user-space conditions: missing dirs, corrupt
'     files, locked files, vanished files and thumbnail failures become
'     statuses/counters (repo containment culture, §7).
'
' MEDIA FACTS come from MediaProbe.Probe (the existing ffprobe contract).
' The library adds NO codec assumptions and no filename parsing — any *.mp4
' under the roots is a candidate, whatever produced it.
'
' REFRESH MODEL: explicit Rescan (sync, bounded) / RescanAsync (task) +
' optional FileSystemWatcher (Created/Deleted/Renamed only — content writes
' during an in-progress recording must not trigger probe storms) with a
' quiet-period debounce. Every completed rescan publishes the snapshot and
' fires LibraryChanged exactly once.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Namespace Gallery.Video

    ''' <summary>Library configuration. Roots are the recording directories
    ''' (production: config.json Paths.GalleryPath, fallback SavePath — the
    ''' SAME value the recorder writes into; the library never reads config
    ''' itself, the host passes it in).</summary>
    Public NotInheritable Class GalleryLibraryOptions

        ''' <summary>Directories to index. At least one is required.</summary>
        Public Property GalleryRoots As New List(Of String)()

        Public Property FfmpegExe As String = ""
        Public Property FfprobeExe As String = ""

        ''' <summary>Index subdirectories of each root (duplicate file names
        ''' are legal; ordering stays deterministic via the path tie-break).</summary>
        Public Property Recursive As Boolean = False

        ''' <summary>Watch the roots and auto-rescan (debounced) when files
        ''' appear/disappear/rename. Default OFF — deterministic explicit
        ''' rescans unless the UI opts in.</summary>
        Public Property EnableFileSystemWatcher As Boolean = False
        Public Property WatchDebounceMs As Integer = 500

        ''' <summary>Thumbnail cache directory. Empty = thumbnails disabled
        ''' (EnsureThumbnail reports Disabled). No credential-like data is
        ''' ever written here — only PNG frames and hash-named files.</summary>
        Public Property ThumbnailDirectory As String = ""

        Public Property ThumbnailWidth As Integer = 480
        ''' <summary>Seek target for the thumbnail frame (clamped to half the
        ''' clip duration — deterministic, never "first frame" which may be
        ''' an encoder black frame).</summary>
        Public Property ThumbnailSeconds As Double = 1.0
        Public Property ThumbnailTimeoutMs As Integer = 15000

        ''' <summary>Simple cache bound: oldest PNGs (by LastWriteTimeUtc) are
        ''' evicted beyond this count.</summary>
        Public Property ThumbnailMaxCacheFiles As Integer = 1000

    End Class

    ''' <summary>Outcome of one completed rescan — additive counts against the
    ''' previous snapshot, so a UI can update incrementally or just re-read.</summary>
    Public NotInheritable Class GalleryScanResult

        Public Sub New(added As Integer, removed As Integer, updated As Integer,
                       total As Integer, probedFailures As Integer, problems As IReadOnlyList(Of String))
            Me.Added = added
            Me.Removed = removed
            Me.Updated = updated
            Me.Total = total
            Me.ProbeFailures = probedFailures
            Me.Problems = If(problems IsNot Nothing, problems.ToList(), New List(Of String)()).AsReadOnly()
        End Sub

        Public ReadOnly Property Added As Integer
        Public ReadOnly Property Removed As Integer
        Public ReadOnly Property Updated As Integer
        Public ReadOnly Property Total As Integer
        ''' <summary>Entries whose probe did not reach Ready (Corrupt/
        ''' Unsupported/Failed) — surfaced for UI banners, never thrown.</summary>
        Public ReadOnly Property ProbeFailures As Integer
        ''' <summary>Bounded list of scan-level problems (unreadable root,
        ''' file vanished between enumeration and probe, …).</summary>
        Public ReadOnly Property Problems As IReadOnlyList(Of String)

        Public Overrides Function ToString() As String
            Return $"total={Total} +{Added} -{Removed} ~{Updated} probeFail={ProbeFailures} problems={Problems.Count}"
        End Function
    End Class

    Public NotInheritable Class GalleryLibrary
        Implements IDisposable

        Private ReadOnly _opts As GalleryLibraryOptions
        Private ReadOnly _gate As New Object()
        Private _entries As IReadOnlyList(Of GalleryEntry) = CType(New List(Of GalleryEntry)(), IReadOnlyList(Of GalleryEntry))
        Private _disposed As Boolean = False

        ' optional watchers (one per root)
        Private ReadOnly _watchers As New List(Of FileSystemWatcher)()
        ' shared debounce for all roots: timer restarted on every event
        Private _watchTimer As Timer = Nothing
        Private ReadOnly _watchDebounceMs As Integer

        Public Sub New(opts As GalleryLibraryOptions)
            If opts Is Nothing Then Throw New ArgumentNullException(NameOf(opts))
            If opts.GalleryRoots Is Nothing OrElse opts.GalleryRoots.Count = 0 Then
                Throw New ArgumentException("GalleryLibraryOptions.GalleryRoots requires at least one root.", NameOf(opts))
            End If
            If opts.WatchDebounceMs < 50 Then Throw New ArgumentOutOfRangeException(NameOf(opts), "WatchDebounceMs must be >= 50")
            _opts = opts
            _watchDebounceMs = opts.WatchDebounceMs
            If opts.EnableFileSystemWatcher Then StartWatchers()
        End Sub

        ''' <summary>Fired after EVERY completed rescan (sync or watcher-
        ' driven), outside the internal lock. Handlers may read Entries
        ''' immediately — the snapshot they see is the one this result
        ''' describes. Never fires after Dispose.</summary>
        Public Event LibraryChanged(sender As GalleryLibrary, result As GalleryScanResult)

        ' ---- reads (immutable snapshots) ----

        ''' <summary>Current index in the default order: LastWriteTimeUtc
        ''' descending (newest recording first), full-path tie-break.</summary>
        Public ReadOnly Property Entries As IReadOnlyList(Of GalleryEntry)
            Get
                SyncLock _gate
                    Return _entries
                End SyncLock
            End Get
        End Property

        ''' <summary>Deterministic sorted + filtered view of the current
        ''' snapshot. Ties on the primary key always break by full path
        ''' (OrdinalIgnoreCase, ascending) — the same input set can never
        ''' produce two different orders.</summary>
        Public Function Query(sortKey As GallerySortKey, descending As Boolean,
                              Optional textFilter As String = Nothing,
                              Optional audioOnly As Boolean = False,
                              Optional readyOnly As Boolean = False) As IReadOnlyList(Of GalleryEntry)
            Dim snapshot = Entries
            Dim result As New List(Of GalleryEntry)(snapshot.Count)

            Dim needle As String = If(textFilter, "").Trim()
            For Each e In snapshot
                If readyOnly AndAlso Not e.IsReady Then Continue For
                If audioOnly AndAlso Not e.HasAudio Then Continue For
                If needle.Length > 0 AndAlso
                   e.Name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0 Then Continue For
                result.Add(e)
            Next

            result.Sort(Function(x, y) CompareEntries(x, y, sortKey, descending))
            Return result.AsReadOnly()
        End Function

        ''' <summary>Entry by exact path (OrdinalIgnoreCase) or Nothing.</summary>
        Public Function Find(fullPath As String) As GalleryEntry
            If String.IsNullOrWhiteSpace(fullPath) Then Return Nothing
            For Each e In Entries
                If String.Equals(e.FullPath, fullPath, StringComparison.OrdinalIgnoreCase) Then Return e
            Next
            Return Nothing
        End Function

        Private Shared Function CompareEntries(x As GalleryEntry, y As GalleryEntry,
                                               key As GallerySortKey, descending As Boolean) As Integer
            Dim primary As Integer
            Select Case key
                Case GallerySortKey.Name
                    primary = String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
                Case GallerySortKey.DurationSec
                    primary = x.DurationSec.CompareTo(y.DurationSec)
                Case GallerySortKey.SizeBytes
                    primary = x.LengthBytes.CompareTo(y.LengthBytes)
                Case Else ' LastWriteTimeUtc
                    primary = DateTime.Compare(x.LastWriteTimeUtc, y.LastWriteTimeUtc)
            End Select
            If descending AndAlso primary <> 0 Then primary = -primary
            If primary <> 0 Then Return primary
            ' tie-break: full path, always ascending — the determinism anchor
            Return String.Compare(x.FullPath, y.FullPath, StringComparison.OrdinalIgnoreCase)
        End Function

        ' ---- refresh ----

        ''' <summary>Synchronous bounded rescan (probe work scales with file
        ' count; each probe is individually timeout-bounded by MediaProbe).
        ' Publishes the new snapshot and fires LibraryChanged once.</summary>
        Public Function Rescan() As GalleryScanResult
            Dim previous As IReadOnlyList(Of GalleryEntry)
            SyncLock _gate
                If _disposed Then
                    Return New GalleryScanResult(0, 0, 0, 0, 0, Nothing)
                End If
                previous = _entries
            End SyncLock

            Dim problems As New List(Of String)()
            Dim nextEntries = ScanCore(previous, _opts, problems)

            Dim result As GalleryScanResult
            SyncLock _gate
                If _disposed Then
                    Return New GalleryScanResult(0, 0, 0, nextEntries.Count, 0, problems)
                End If
                result = Diff(previous, nextEntries, problems)
                _entries = nextEntries
            End SyncLock

            RaiseEvent LibraryChanged(Me, result)
            Return result
        End Function

        ''' <summary>Non-blocking rescan for UI triggers (watcher / nav
        ' refresh button). Completes via LibraryChanged.</summary>
        Public Function RescanAsync() As Task
            Return Task.Run(Sub() Rescan())
        End Function

        ''' <summary>Scan core — the ONLY place that touches the filesystem
        ' and ffprobe. Deterministic: files are enumerated, name-sorted, then
        ' probed in that order; output ordering is applied by the caller.</summary>
        Private Function ScanCore(previous As IReadOnlyList(Of GalleryEntry),
                                  opts As GalleryLibraryOptions,
                                  problems As List(Of String)) As IReadOnlyList(Of GalleryEntry)
            Dim paths As New List(Of String)()

            ' binary sanity ONCE per scan (FFmpegLocator validates by spawn —
            ' per-file calls would multiply that cost)
            Dim probeUsable As Boolean = False
            If Not String.IsNullOrWhiteSpace(opts.FfprobeExe) Then
                probeUsable = FFmpegLocator.IsUsableFFmpeg(opts.FfprobeExe)
            End If
            If Not probeUsable Then
                problems.Add($"ffprobe unusable: ""{opts.FfprobeExe}"" — entries cannot be probed this scan")
            End If

            Dim soo = If(opts.Recursive, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)
            For Each root In opts.GalleryRoots
                Try
                    If Not Directory.Exists(root) Then
                        problems.Add($"root missing: {root}")
                        Continue For
                    End If
                    For Each f In Directory.EnumerateFiles(root, "*.mp4", soo)
                        ' strict extension (Windows search patterns match
                        ' longer names too: *.mp4 would hit *.mp4.bak)
                        If Not String.Equals(Path.GetExtension(f), ".mp4", StringComparison.OrdinalIgnoreCase) Then Continue For
                        paths.Add(Path.GetFullPath(f))
                    Next
                Catch ex As Exception
                    problems.Add($"root {root}: {ex.Message}")
                End Try
            Next

            ' enumeration order is unspecified everywhere → pin it
            paths.Sort(StringComparer.OrdinalIgnoreCase)

            Dim outList As New List(Of GalleryEntry)(paths.Count)
            For Each p In paths
                Dim fi As FileInfo = Nothing
                Try
                    fi = New FileInfo(p)
                    If Not fi.Exists Then
                        ' vanished between enumeration and stat — not a
                        ' gallery member; bounded problem note
                        If problems.Count < 100 Then problems.Add($"vanished during scan: {p}")
                        Continue For
                    End If
                Catch ex As Exception
                    If problems.Count < 100 Then problems.Add($"stat failed: {p}: {ex.Message}")
                    Continue For
                End Try

                Dim status As GalleryEntryStatus
                Dim durationSec As Double = 0.0
                Dim fps As Double = 0.0
                Dim vCodec As String = ""
                Dim w As Integer = 0, h As Integer = 0
                Dim hasAudio As Boolean = False
                Dim aCodec As String = ""
                Dim detail As String = ""

                If Not probeUsable Then
                    status = GalleryEntryStatus.ProbeFailed
                    detail = "ffprobe unusable this scan"
                Else
                    Dim info As MediaInfo = Nothing
                    Try
                        info = MediaProbe.Probe(p, opts.FfprobeExe)
                    Catch ex As MediaProbe.ProbeIOException
                        status = GalleryEntryStatus.ProbeFailed
                        detail = ex.Message
                        info = Nothing
                    End Try

                    If info Is Nothing Then
                        status = GalleryEntryStatus.CorruptFile
                        detail = "ffprobe rejected the file"
                    ElseIf Not info.HasVideo OrElse info.Video.Width <= 0 OrElse info.Video.Height <= 0 Then
                        status = GalleryEntryStatus.UnsupportedFormat
                        detail = If(info.HasVideo, "video stream without dimensions", "no video stream (audio-only)")
                    Else
                        status = GalleryEntryStatus.Ready
                        durationSec = info.Format.DurationSec
                        fps = MediaInfo.ParseFrameRate(info.Video.AvgFrameRate)
                        vCodec = info.Video.CodecName
                        w = info.Video.Width
                        h = info.Video.Height
                        hasAudio = info.HasAudio
                        aCodec = If(info.Audio?.CodecName, "")
                    End If
                End If

                outList.Add(New GalleryEntry(p, fi.Length, fi.LastWriteTimeUtc, status,
                                             durationSec, fps, vCodec, w, h, hasAudio, aCodec, detail))
            Next

            ' default published order: newest recording first, path tie-break
            outList.Sort(Function(x, y) CompareEntries(x, y, GallerySortKey.LastWriteTimeUtc, True))
            Return outList.AsReadOnly()
        End Function

        Private Shared Function Diff(previous As IReadOnlyList(Of GalleryEntry),
                                     nextList As IReadOnlyList(Of GalleryEntry),
                                     problems As List(Of String)) As GalleryScanResult
            Dim prevMap As New Dictionary(Of String, GalleryEntry)(StringComparer.OrdinalIgnoreCase)
            For Each e In previous
                prevMap(e.FullPath) = e
            Next
            Dim nextMap As New Dictionary(Of String, GalleryEntry)(StringComparer.OrdinalIgnoreCase)
            For Each e In nextList
                nextMap(e.FullPath) = e
            Next

            Dim added As Integer = 0, removed As Integer = 0, updated As Integer = 0, failures As Integer = 0
            For Each e In nextList
                If Not prevMap.ContainsKey(e.FullPath) Then added += 1
                If Not e.IsReady Then failures += 1
            Next
            For Each e In previous
                Dim fresh As GalleryEntry = Nothing
                If Not nextMap.TryGetValue(e.FullPath, fresh) Then
                    removed += 1
                ElseIf Not fresh.Equals(e) Then
                    updated += 1
                End If
            Next

            Return New GalleryScanResult(added, removed, updated, nextList.Count, failures, problems)
        End Function

        ' ---- optional watcher ----

        Private Sub StartWatchers()
            For Each root In _opts.GalleryRoots
                Try
                    Dim fsw As New FileSystemWatcher(Path.GetFullPath(root), "*.mp4") With {
                        .IncludeSubdirectories = _opts.Recursive,
                        .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.DirectoryName
                    }
                    ' Content writes (Changed) are deliberately ignored: an
                    ' in-progress recording rewrites LastWrite constantly and
                    ' must not trigger probe storms. Appear/disappear/rename
                    ' are the events that change the INDEX.
                    AddHandler fsw.Created, AddressOf OnWatchEvent
                    AddHandler fsw.Deleted, AddressOf OnWatchEvent
                    AddHandler fsw.Renamed, AddressOf OnWatchEvent
                    AddHandler fsw.Error, AddressOf OnWatchEvent
                    fsw.EnableRaisingEvents = True
                    _watchers.Add(fsw)
                Catch
                    ' root vanished / unreadable — Rescan reports it; a
                    ' watcher failure never breaks the library
                End Try
            Next
            _watchTimer = New Timer(AddressOf WatchTimerTick, Nothing, Timeout.Infinite, Timeout.Infinite)
        End Sub

        Private Sub OnWatchEvent(sender As Object, e As EventArgs)
            ' debounce: restart the quiet-period timer on every burst event
            Try
                _watchTimer?.Change(_watchDebounceMs, Timeout.Infinite)
            Catch
                ' disposed timer — ignore
            End Try
        End Sub

        Private Sub WatchTimerTick(state As Object)
            Dim l = Interlocked.Exchange(_watchTickGate, 1)
            If l <> 0 Then Return ' a rescan is already in flight
            Try
                SyncLock _gate
                    If _disposed Then Return
                End SyncLock
                Rescan()
            Finally
                Interlocked.Exchange(_watchTickGate, 0)
            End Try
        End Sub
        Private _watchTickGate As Integer = 0

        ' ---- dispose ----

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _gate
                If _disposed Then Return
                _disposed = True
            End SyncLock
            For Each w In _watchers
                Try
                    w.EnableRaisingEvents = False
                    w.Dispose()
                Catch
                End Try
            Next
            _watchers.Clear()
            Try
                _watchTimer?.Dispose()
            Catch
            End Try
        End Sub

    End Class

End Namespace
