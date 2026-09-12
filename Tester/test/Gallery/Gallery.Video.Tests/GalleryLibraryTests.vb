Option Strict On
Option Explicit On
Option Infer On

' GalleryLibraryTests.vb — Gallery data-layer regression (W3).
'
' REAL filesystem + REAL ffprobe/ffmpeg (Tier 2). Every test owns an
' isolated temp directory tree (created + disposed in the test) so scan
' results never cross-contaminate. Deterministic ordering is asserted by
' repeating rescans and by pinning equal mtimes.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading

Namespace Gallery.Video.Tests

    Friend Class GalleryLibraryTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("GLIB: empty folder → 0 entries, clean result", AddressOf Test_EmptyFolder)
            runner("GLIB: single file → probed metadata exact", AddressOf Test_SingleFileMetadata)
            runner("GLIB: many files → order identical across rescans", AddressOf Test_ManyDeterministic)
            runner("GLIB: equal mtime → deterministic full-path tie-break", AddressOf Test_TieBreakByPath)
            runner("GLIB: duplicate names in subdirs (recursive)", AddressOf Test_DuplicateNames)
            runner("GLIB: deleted file → removed + LibraryChanged(-1)", AddressOf Test_DeletedFile)
            runner("GLIB: corrupt MP4 → CorruptFile, kept, no throw", AddressOf Test_CorruptFile)
            runner("GLIB: refresh adds file + filters + sorting", AddressOf Test_RefreshAddAndQuery)
            runner("GLIB: thumbnails → generate, cache hit, failure, no temp litter", AddressOf Test_Thumbnails)
            runner("GLIB: watcher auto-rescans on file add", AddressOf Test_Watcher)
            runner("GLIB: non-mp4 names ignored (incl. *.mp4.bak)", AddressOf Test_NonMp4Ignored)
            runner("GLIB-E2E: 240fps file → probed ≈240fps → headless playback to EOF", AddressOf Test_E2E_240Fps)
            runner("GLIB-E2E: real-shape recording (1680x1050@60 h264+aac) → probe + headless playback", AddressOf Test_E2E_RealShape)
        End Sub

        ' ---- helpers ----

        Private Shared Function NewDir() As String
            Dim d = Path.Combine(Path.GetTempPath(), "glib-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(d)
            Return d
        End Function

        Private Shared Function Gen(dir As String, name As String, spec As String) As String
            Dim p = Path.Combine(dir, name)
            Dim core As String
            Select Case spec
                Case "v1" : core = "-f lavfi -i testsrc2=size=320x240:rate=30:duration=1 -c:v libx264 -pix_fmt yuv420p -g 30"
                Case "v2" : core = "-f lavfi -i testsrc2=size=320x240:rate=30:duration=2 -c:v libx264 -pix_fmt yuv420p -g 30"
                Case "v4" : core = "-f lavfi -i testsrc2=size=640x360:rate=30:duration=4 -c:v libx264 -pix_fmt yuv420p -g 30"
                Case "av4" : core = "-f lavfi -i testsrc2=size=320x240:rate=30:duration=4 " &
                                    "-f lavfi -i sine=frequency=1000:sample_rate=48000:duration=4 " &
                                    "-c:v libx264 -pix_fmt yuv420p -g 30 -c:a aac -b:a 128k -shortest"
                Case Else
                    Throw New ArgumentException("unknown spec " & spec)
            End Select
            Dim args = $"-y {core} ""{p}"""
            Dim so As String = Nothing, se As String = Nothing
            Dim code As Integer = -1
            If Not MediaProbe.RunCapture(TestMedia.FfmpegPath, args, 120000, so, se, code) OrElse code <> 0 Then
                Throw New SkipException($"synthetic generation failed ({name}): {se}")
            End If
            Return p
        End Function

        Private Shared Function Corrupt(dir As String, name As String) As String
            Dim p = Path.Combine(dir, name)
            File.WriteAllText(p, "this is definitely not an MP4 container")
            Return p
        End Function

        Private Shared Function NewLib(root As String,
                                       Optional recursive As Boolean = False,
                                       Optional watcher As Boolean = False,
                                       Optional thumbDir As String = "") As GalleryLibrary
            Return New GalleryLibrary(New GalleryLibraryOptions With {
                .GalleryRoots = New List(Of String) From {root},
                .FfmpegExe = TestMedia.FfmpegPath,
                .FfprobeExe = TestMedia.FfprobePath,
                .Recursive = recursive,
                .EnableFileSystemWatcher = watcher,
                .ThumbnailDirectory = thumbDir
            })
        End Function

        Private Shared Function CollectEvents(library As GalleryLibrary) As List(Of GalleryScanResult)
            Dim evts As New List(Of GalleryScanResult)()
            AddHandler library.LibraryChanged,
                Sub(sender, r)
                    SyncLock evts
                        evts.Add(r)
                    End SyncLock
                End Sub
            Return evts
        End Function

        Private Shared Function PathsOf(entries As IReadOnlyList(Of GalleryEntry)) As List(Of String)
            Dim l As New List(Of String)()
            For Each e In entries
                l.Add(e.FullPath)
            Next
            Return l
        End Function

        ' ---- tests ----

        Private Shared Sub Test_EmptyFolder()
            Dim dir = NewDir()
            Try
                Using library As GalleryLibrary = NewLib(dir)
                    Dim r = library.Rescan()
                    TestRunner.AssertEqual(0, r.Total, "empty folder → 0 entries")
                    TestRunner.AssertEqual(0, r.Added, "0 added")
                    TestRunner.AssertEqual(0, r.ProbeFailures, "0 probe failures")
                    TestRunner.AssertEqual(0, library.Entries.Count, "snapshot empty")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_SingleFileMetadata()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Gen(dir, "rec.mp4", "v2")
                Using library As GalleryLibrary = NewLib(dir)
                    Dim r = library.Rescan()
                    TestRunner.AssertEqual(1, r.Total, "1 entry")
                    Dim e = library.Entries(0)
                    TestRunner.AssertEqual(GalleryEntryStatus.Ready, e.Status, "Ready")
                    TestRunner.Assert(Math.Abs(e.DurationSec - 2.0) <= 0.5, $"duration ≈2s (got {e.DurationSec:0.##})")
                    TestRunner.Assert(Math.Abs(e.Fps - 30.0) <= 1.0, $"fps ≈30 (got {e.Fps:0.##})")
                    TestRunner.AssertEqual("h264", e.VideoCodec, "video codec")
                    TestRunner.AssertEqual(320, e.Width, "width")
                    TestRunner.AssertEqual(240, e.Height, "height")
                    TestRunner.AssertEqual(False, e.HasAudio, "video-only file")
                    TestRunner.Assert(e.LengthBytes > 0, "size from fs")
                    TestRunner.Assert(e.LastWriteTimeUtc > DateTime.UtcNow.AddMinutes(-5), "mtime sane")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_ManyDeterministic()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Gen(dir, "b_second.mp4", "v2")
                Gen(dir, "c_third.mp4", "v4")
                Gen(dir, "a_first.mp4", "v1")
                Using library As GalleryLibrary = NewLib(dir)
                    library.Rescan()
                    Dim order1 = PathsOf(library.Entries)
                    TestRunner.AssertEqual(3, order1.Count, "3 entries")

                    ' rescan must reproduce the EXACT same order (determinism)
                    library.Rescan()
                    Dim order2 = PathsOf(library.Entries)
                    For i = 0 To 2
                        TestRunner.AssertEqual(order1(i), order2(i), $"order[{i}] stable across rescans")
                    Next

                    ' default order: newest first by ACTUAL completion mtime
                    ' but generation order was b_second, c_third, a_first)
                    TestRunner.AssertEqual(Path.Combine(dir, "a_first.mp4"), order1(0), "newest first")
                    TestRunner.AssertEqual(Path.Combine(dir, "c_third.mp4"), order1(1), "middle by mtime")
                    TestRunner.AssertEqual(Path.Combine(dir, "b_second.mp4"), order1(2), "oldest last")

                    ' explicit name sort is alphabetical, ascending
                    Dim byName = library.Query(GallerySortKey.Name, False)
                    TestRunner.AssertEqual("a_first.mp4", byName(0).Name, "name sort asc")
                    TestRunner.AssertEqual("c_third.mp4", byName(2).Name, "name sort asc last")

                    ' descending by duration: 4s first
                    Dim byDur = library.Query(GallerySortKey.DurationSec, True)
                    TestRunner.Assert(Math.Abs(byDur(0).DurationSec - 4.0) <= 0.5, "longest first")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_TieBreakByPath()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Dim pB = Gen(dir, "zzz.mp4", "v1")
                Dim pA = Gen(dir, "aaa.mp4", "v1")
                ' pin IDENTICAL mtimes → primary key ties → path must decide
                Dim pinned As New DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                File.SetLastWriteTimeUtc(pB, pinned)
                File.SetLastWriteTimeUtc(pA, pinned)

                Using library As GalleryLibrary = NewLib(dir)
                    library.Rescan()
                    Dim order = PathsOf(library.Entries)
                    TestRunner.AssertEqual(2, order.Count, "2 entries")
                    TestRunner.AssertEqual(pA, order(0), "tie-break → aaa before zzz (path asc)")
                    TestRunner.AssertEqual(pB, order(1), "tie-break second")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_DuplicateNames()
            TestMedia.RequireBinaries()
            Dim root = NewDir()
            Dim subDir = Path.Combine(root, "2026-09")
            Directory.CreateDirectory(subDir)
            Try
                Dim p1 = Gen(root, "clip.mp4", "v1")
                Dim p2 = Gen(subDir, "clip.mp4", "v2")
                Using library As GalleryLibrary = NewLib(root, recursive:=True)
                    Dim r = library.Rescan()
                    TestRunner.AssertEqual(2, r.Total, "both duplicates indexed")
                    Dim order = PathsOf(library.Entries)
                    ' path tie-break: root sorts before root\2026-09
                    TestRunner.AssertEqual(p2, order(0), "subdir copy first ('2' < 'c')")
                    TestRunner.AssertEqual(p1, order(1), "root copy second")
                    TestRunner.Assert(library.Find(p2) IsNot Nothing, "Find by exact path")
                    TestRunner.Assert(library.Find(Path.Combine(root, "nope.mp4")) Is Nothing, "Find miss → Nothing")
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        Private Shared Sub Test_DeletedFile()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Dim keep = Gen(dir, "keep.mp4", "v2")
                Dim gone = Gen(dir, "gone.mp4", "v1")
                Using library As GalleryLibrary = NewLib(dir)
                    Dim evts = CollectEvents(library)
                    library.Rescan()
                    TestRunner.AssertEqual(2, library.Entries.Count, "2 before delete")

                    File.Delete(gone)
                    library.Rescan()

                    TestRunner.AssertEqual(1, library.Entries.Count, "removed from index")
                    TestRunner.AssertEqual(keep, library.Entries(0).FullPath, "survivor intact")
                    SyncLock evts
                        TestRunner.Assert(evts.Count >= 2, "LibraryChanged fired per rescan")
                        Dim last = evts(evts.Count - 1)
                        TestRunner.AssertEqual(1, last.Removed, "last event reports the removal")
                        TestRunner.AssertEqual(1, last.Total, "event total = 1")
                    End SyncLock
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_CorruptFile()
            Dim dir = NewDir()
            Try
                Corrupt(dir, "broken.mp4")
                Using library As GalleryLibrary = NewLib(dir)
                    Dim r = library.Rescan()
                    TestRunner.AssertEqual(1, r.Total, "corrupt file still indexed (UI must see it)")
                    TestRunner.AssertEqual(1, r.ProbeFailures, "surfaced as probe failure")
                    Dim e = library.Entries(0)
                    TestRunner.AssertEqual(GalleryEntryStatus.CorruptFile, e.Status, "CorruptFile")
                    TestRunner.AssertEqual(0.0, e.DurationSec, "no metadata invented")
                    TestRunner.AssertEqual(0, e.Width, "no dims invented")
                    TestRunner.AssertEqual(False, e.HasAudio, "no audio invented")
                    TestRunner.Assert(e.Detail.Length > 0, "detail explains")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_RefreshAddAndQuery()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Gen(dir, "clip_one.mp4", "v2") ' video-only
                Using library As GalleryLibrary = NewLib(dir)
                    Dim evts = CollectEvents(library)
                    library.Rescan()
                    TestRunner.AssertEqual(1, library.Entries.Count, "1 before refresh")

                    Dim added = Gen(dir, "clip_two_audio.mp4", "av4")
                    Dim r2 = library.Rescan()
                    TestRunner.AssertEqual(1, r2.Added, "refresh reports the add")
                    TestRunner.AssertEqual(2, library.Entries.Count, "2 after refresh")
                    SyncLock evts
                        TestRunner.Assert(evts.Count >= 2, "refresh fired LibraryChanged")
                    End SyncLock

                    ' filters
                    Dim audioOnly = library.Query(GallerySortKey.Name, True, audioOnly:=True)
                    TestRunner.AssertEqual(1, audioOnly.Count, "audioOnly → 1")
                    TestRunner.AssertEqual(added, audioOnly(0).FullPath, "the av file")
                    TestRunner.Assert(audioOnly(0).HasAudio AndAlso audioOnly(0).AudioCodec.Length > 0,
                                      "audio codec surfaced")
                    TestRunner.Assert(audioOnly(0).Width = 320, "av video dims probed")

                    Dim textHit = library.Query(GallerySortKey.Name, True, textFilter:="TWO")
                    TestRunner.AssertEqual(1, textHit.Count, "text filter case-insensitive")

                    Dim readyOnly = library.Query(GallerySortKey.Name, True, readyOnly:=True)
                    TestRunner.AssertEqual(2, readyOnly.Count, "readyOnly keeps both healthy files")

                    ' audioOnly on the video-only library state would be 0 — sanity
                    Dim none = library.Query(GallerySortKey.Name, True, audioOnly:=True, textFilter:="clip_one")
                    TestRunner.AssertEqual(0, none.Count, "video-only file filtered out by audioOnly")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_Thumbnails()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Dim cacheDir = NewDir()
            Try
                Dim good = Gen(dir, "good.mp4", "v2")
                Dim broken = Corrupt(dir, "broken.mp4")

                Dim svc = New ThumbnailService(TestMedia.FfmpegPath, cacheDir, width:=320, secondsInto:=1.0)
                    Dim e = New GalleryEntry(good, 999, File.GetLastWriteTimeUtc(good),
                                             GalleryEntryStatus.Ready, 2.0, 30.0, "h264",
                                             320, 240, False, "", "")

                    Dim first = svc.EnsureThumbnail(good, e.LastWriteTimeUtc.Ticks, e.LengthBytes, e.DurationSec)
                    TestRunner.AssertEqual(GalleryThumbnailStatus.Ready, first.Status, "first call generates")
                    TestRunner.Assert(File.Exists(first.ThumbnailPath), "png exists in cache")
                    TestRunner.Assert(first.ThumbnailPath.StartsWith(cacheDir, StringComparison.OrdinalIgnoreCase),
                                      "png inside cache dir")

                    Dim second = svc.EnsureThumbnail(good, e.LastWriteTimeUtc.Ticks, e.LengthBytes, e.DurationSec)
                    TestRunner.AssertEqual(GalleryThumbnailStatus.FromCache, second.Status, "second call → cache hit")
                    TestRunner.AssertEqual(first.ThumbnailPath, second.ThumbnailPath, "same cache file")

                    ' same path, different mtime → new key → regenerate
                    Dim bumped = DateTime.UtcNow
                    File.SetLastWriteTimeUtc(good, bumped)
                    Dim third = svc.EnsureThumbnail(good, bumped.Ticks, e.LengthBytes, e.DurationSec)
                    TestRunner.AssertEqual(GalleryThumbnailStatus.Ready, third.Status, "edited file regenerates")
                    TestRunner.Assert(third.ThumbnailPath <> first.ThumbnailPath, "new cache entry for new version")

                    ' failure: corrupt source → Failed, no throw
                    Dim bad = svc.EnsureThumbnail(broken, 12345L, 42L, 2.0)
                    TestRunner.AssertEqual(GalleryThumbnailStatus.Failed, bad.Status, "corrupt → Failed")
                    TestRunner.Assert(bad.Detail.Length > 0, "failure detail present")

                    ' missing source → Failed, no throw
                    Dim missing = svc.EnsureThumbnail(Path.Combine(dir, "ghost.mp4"), 1L, 1L, 1.0)
                    TestRunner.AssertEqual(GalleryThumbnailStatus.Failed, missing.Status, "missing → Failed")

                    ' no temp litter left behind
                    Dim litter = Directory.GetFiles(cacheDir, "*.tmp-*")
                    TestRunner.AssertEqual(0, litter.Length, "no partial temp files in cache")
            Finally
                Directory.Delete(dir, True)
                Directory.Delete(cacheDir, True)
            End Try
        End Sub

        Private Shared Sub Test_Watcher()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Using library As GalleryLibrary = NewLib(dir, watcher:=True)
                    library.Rescan()
                    TestRunner.AssertEqual(0, library.Entries.Count, "starts empty")

                    Gen(dir, "watched.mp4", "v1")

                    ' debounced auto-rescan must surface the file without an
                    ' explicit Rescan call (generous bound for FSW latency)
                    Dim deadline = DateTime.UtcNow.AddSeconds(15)
                    While DateTime.UtcNow < deadline AndAlso library.Entries.Count = 0
                        Thread.Sleep(50)
                    End While
                    TestRunner.AssertEqual(1, library.Entries.Count, "watcher-driven rescan found the file")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_NonMp4Ignored()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                Gen(dir, "real.mp4", "v1")
                File.WriteAllText(Path.Combine(dir, "notes.txt"), "x")
                File.WriteAllText(Path.Combine(dir, "clip.mp4.bak"), "x") ' Windows *.mp4 pattern trap
                File.WriteAllText(Path.Combine(dir, "other.mkv"), "x")
                Using library As GalleryLibrary = NewLib(dir)
                    Dim r = library.Rescan()
                    TestRunner.AssertEqual(1, r.Total, "only the real .mp4 indexed")
                    TestRunner.AssertEqual("real.mp4", library.Entries(0).Name, "and it is the real one")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub


        ' ---- GLIB-E2E: data layer → PlaybackSession (the W2 UI's exact chain,
        ' minus the WinForms surface) ----

        Private Shared Function GenSpec(dir As String, name As String, core As String) As String
            Dim p = Path.Combine(dir, name)
            Dim args = $"-y {core} ""{p}"""
            Dim so As String = Nothing, se As String = Nothing
            Dim code As Integer = -1
            If Not MediaProbe.RunCapture(TestMedia.FfmpegPath, args, 180000, so, se, code) OrElse code <> 0 Then
                Throw New SkipException($"generation failed ({name}): {se}")
            End If
            Return p
        End Function

        Private Shared Sub PlayHeadlessToEof(path As String, waitPlayingMs As Integer, waitEofMs As Integer)
            ' The [2] GalleryPlayerForm contract, headless: Open → Playing →
            ' frames present → EOF → Paused. RenderWindow stays IntPtr.Zero
            ' (NullVideoSink) — present-path hardware is the owner-box gate.
            Dim opts As New PlaybackSessionOptions With {
                .FfmpegExe = TestMedia.FfmpegPath,
                .FfprobeExe = TestMedia.FfprobePath,
                .RenderWindow = IntPtr.Zero,
                .AudioEnabled = False
            }
            Using s As New PlaybackSession(opts)
                Dim faults As New List(Of GalleryVideoFault)()
                AddHandler s.FaultRaised,
                    Sub(sender, f)
                        SyncLock faults
                            faults.Add(f)
                        End SyncLock
                    End Sub

                s.Open(path)
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, waitPlayingMs),
                                  $"Playing (state={s.State}, fault={s.Fault?.ToString()})")
                SessionTestHelper.WaitPresentedAtLeast(s, 1, 30000)

                s.Seek(Math.Max(0.1, PlaybackClock.TicksToSeconds(s.DurationTicks) / 2.0))
                TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 15000), "Playing after mid-clip seek")

                TestRunner.Assert(s.WaitForEof(waitEofMs), "EOF reached")
                TestRunner.AssertEqual(PlaybackState.Paused, s.State, "EOF → Paused")
                SyncLock faults
                    TestRunner.AssertEqual(0, faults.Count, "no faults across the journey")
                End SyncLock
            End Using
        End Sub

        Private Shared Sub Test_E2E_240Fps()
            TestMedia.RequireBinaries()
            Dim dir = NewDir()
            Try
                ' 240fps question (W3 reconciliation): the data layer must PROBE
                ' the real rate and the playback contract must handle it — no
                ' fps assumption anywhere in the chain.
                Dim f = GenSpec(dir, "hi240.mp4",
                                "-f lavfi -i testsrc2=size=320x240:rate=240:duration=1.5 " &
                                "-c:v libx264 -pix_fmt yuv420p -g 240")
                Using library As GalleryLibrary = NewLib(dir)
                    library.Rescan()
                    TestRunner.AssertEqual(1, library.Entries.Count, "indexed")
                    Dim e = library.Entries(0)
                    TestRunner.AssertEqual(GalleryEntryStatus.Ready, e.Status, "Ready")
                    TestRunner.Assert(Math.Abs(e.Fps - 240.0) <= 4.0, $"fps probed ≈240 (got {e.Fps:0.#})")
                End Using
                PlayHeadlessToEof(f, 20000, 30000)
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

        Private Shared Sub Test_E2E_RealShape()
            TestMedia.RequireBinaries()
            ' The REAL ShadowPlay recording shape pinned in TestMedia §5
            ' (1680x1050 h264 yuv420p ~60fps + AAC 48k stereo). A genuine
            ' Record_*.mp4 was not present in this environment — the real
            ' file validation remains an owner-box step.
            Dim dir = NewDir()
            Try
                Dim f = GenSpec(dir, "real_shape.mp4",
                                "-f lavfi -i testsrc2=size=1680x1050:rate=60:duration=2 " &
                                "-f lavfi -i sine=frequency=1000:sample_rate=48000:duration=2 " &
                                "-c:v libx264 -pix_fmt yuv420p -g 60 " &
                                "-c:a aac -b:a 128k -ac 2 -shortest")
                Using library As GalleryLibrary = NewLib(dir)
                    library.Rescan()
                    Dim e = library.Entries(0)
                    TestRunner.AssertEqual(GalleryEntryStatus.Ready, e.Status, "Ready")
                    TestRunner.AssertEqual("h264", e.VideoCodec, "h264")
                    TestRunner.AssertEqual(1680, e.Width, "width 1680")
                    TestRunner.AssertEqual(1050, e.Height, "height 1050")
                    TestRunner.Assert(e.HasAudio, "audio present")
                    TestRunner.Assert(e.AudioCodec = "aac", "aac audio")
                End Using
                ' Playback with AUDIO on this file exercises the W2 audio path
                ' through the real (endpoint-permitting) master clock; session
                ' containment makes endpoint absence a counted fallback.
                Dim opts As New PlaybackSessionOptions With {
                    .FfmpegExe = TestMedia.FfmpegPath,
                    .FfprobeExe = TestMedia.FfprobePath,
                    .RenderWindow = IntPtr.Zero,
                    .AudioEnabled = True
                }
                Using s As New PlaybackSession(opts)
                    s.Open(f)
                    TestRunner.Assert(s.WaitForState(PlaybackState.Playing, 25000), "Playing (real shape)")
                    SessionTestHelper.WaitPresentedAtLeast(s, 1, 30000)
                    TestRunner.Assert(s.WaitForEof(30000), "EOF")
                    TestRunner.AssertEqual(PlaybackState.Paused, s.State, "EOF → Paused")
                End Using
            Finally
                Directory.Delete(dir, True)
            End Try
        End Sub

    End Class

End Namespace
