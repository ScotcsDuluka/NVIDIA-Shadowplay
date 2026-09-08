' FFmpegLocator.vb — corrupt-binary-proof FFmpeg resolution (2026-09-08)
'
' ┌────────────────────────────────────────────────────────────────────┐
' │ POSTMORTEM: every recording session aborted with Win32Exception    │
' │ 193 ("...ffmpeg.exe ... is not a valid application for this OS     │
' │ platform") because the resolvers accepted the FIRST candidate      │
' │ whose File.Exists was True. File.Exists says nothing about the     │
' │ file being a runnable Windows binary — one corrupt/stale copy in   │
' │ {bin}\FFmpeg\ dead-ended every lookup while a perfectly good       │
' │ API-Core\ffmpeg.exe sat one candidate later.                       │
' └────────────────────────────────────────────────────────────────────┘
'
' Contract now — a candidate is usable only when ALL hold:
'   1. it exists,
'   2. it is at least MinSaneBytes (git-LFS pointer files, HTML error
'      pages saved as .exe, and truncated copies are far smaller),
'   3. (Windows) it carries the PE "MZ" DOS-header mark,
'   4. running `<path> -version` exits 0 within ProbeTimeoutMs — the
'      ground truth that ALSO validates the FFmpeg DLL bundle beside
'      the exe (missing/mismatched av*.dll fails the spawn exactly like
'      a corrupt PE does).
' Probes are cached per (length, last-write-time) so the -version cost
' is paid once per file version and a repaired file is re-probed.
'
' Threading: resolvers run on settings-load and record-start threads;
' the cache is guarded by a SyncLock (volume is trivial).

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices

Public Module FFmpegLocator

    ''' <summary>Test seam — when set, replaces the real process probe.
    ''' Tests MUST reset this to Nothing in their cleanup.</summary>
    Friend _probeHook As Func(Of String, Boolean) = Nothing

    Private ReadOnly _cacheLock As New Object()
    ' path → (fileVersionKey, usable)
    Private ReadOnly _cache As New Dictionary(Of String, Tuple(Of String, Boolean))()

    ''' <summary>Floor under which a file cannot be a real ffmpeg.exe.
    ''' LFS pointers / placeholders / interrupted copies are far smaller;
    ''' the smallest real dynamically-linked ffmpeg builds are ≫ this.</summary>
    Private Const MinSaneBytes As Long = 65536

    ''' <summary>ffmpeg -version must exit inside this window. Cold-start
    ''' with an antivirus first-scan is the slowest legitimate case.</summary>
    Private Const ProbeTimeoutMs As Integer = 10000

    ''' <summary>
    ''' True when <paramref name="path"/> is an ffmpeg we can actually start.
    ''' Never throws — any I/O error means "not usable".
    ''' </summary>
    Public Function IsUsableFFmpeg(path As String) As Boolean
        If String.IsNullOrEmpty(path) Then Return False

        Dim versionKey As String = ""
        Try
            If Not File.Exists(path) Then Return False
            Dim fi As New FileInfo(path)
            If fi.Length < MinSaneBytes Then Return False

            ' Windows PE sanity: the DOS-header mark. On non-Windows dev
            ' tooling the -version probe below is the only authority.
            If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                Using fs As FileStream = fi.OpenRead()
                    Dim b0 As Integer = fs.ReadByte()
                    Dim b1 As Integer = fs.ReadByte()
                    If b0 <> 77 OrElse b1 <> 90 Then Return False ' "M", "Z"
                End Using
            End If

            versionKey = fi.Length.ToString(Global.System.Globalization.CultureInfo.InvariantCulture) &
                         ":" & fi.LastWriteTimeUtc.Ticks.ToString(Global.System.Globalization.CultureInfo.InvariantCulture)
        Catch
            Return False
        End Try

        Dim usable As Boolean
        SyncLock _cacheLock
            Dim hit As Tuple(Of String, Boolean) = Nothing
            If _cache.TryGetValue(path, hit) AndAlso hit.Item1 = versionKey Then
                Return hit.Item2
            End If
        End SyncLock

        usable = ProbeRuns(path)

        SyncLock _cacheLock
            _cache(path) = Tuple.Create(versionKey, usable)
        End SyncLock
        Return usable
    End Function

    ''' <summary>
    ''' Returns the FIRST candidate that is actually usable, or "" when
    ''' none is. <paramref name="log"/> receives one line per candidate
    ''' that EXISTS but is rejected (missing candidates are normal and
    ''' stay silent — most lookup paths have 2-3 non-existent entries).
    ''' </summary>
    Public Function FirstUsableFFmpeg(candidates As IEnumerable(Of String), log As Action(Of String)) As String
        If candidates Is Nothing Then Return ""
        For Each c As String In candidates
            If String.IsNullOrEmpty(c) Then Continue For
            Try
                If IsUsableFFmpeg(c) Then Return c
                ' Distinguish the diagnosable case only: file present but bad.
                If File.Exists(c) Then
                    log?.Invoke("FFmpegLocator: candidate rejected (exists but not a runnable ffmpeg): " & c)
                End If
            Catch ex As Exception
                log?.Invoke("FFmpegLocator: candidate probe error " & c & ": " & ex.Message)
            End Try
        Next
        Return ""
    End Function

    ''' <summary>Spawn <paramref name="path"/> -version and require a clean
    ''' exit. Both stdio pipes are drained asynchronously (the Encoder
    ''' Detector pipe lesson: an undrained 64KB stderr buffer deadlocks
    ''' the child). A Win32Exception here is the diagnosis — invalid PE,
    ''' wrong architecture, or broken DLL bundle.</summary>
    Private Function ProbeRuns(path As String) As Boolean
        If _probeHook IsNot Nothing Then Return _probeHook(path)
        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = path,
                .Arguments = "-version",
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }
            Using p As Process = Process.Start(psi)
                If p Is Nothing Then Return False
                Dim so As Threading.Tasks.Task = p.StandardOutput.ReadToEndAsync()
                Dim se As Threading.Tasks.Task = p.StandardError.ReadToEndAsync()
                If Not p.WaitForExit(ProbeTimeoutMs) Then
                    Try
                        p.Kill()
                    Catch
                    End Try
                    Return False
                End If
                GC.KeepAlive(so)
                GC.KeepAlive(se)
                Return p.ExitCode = 0
            End Using
        Catch
            Return False
        End Try
    End Function

End Module
