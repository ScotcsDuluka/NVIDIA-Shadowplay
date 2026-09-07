Option Strict On
Option Explicit On
Option Infer On

' FfprobeBoundaryTests.vb — C/2: the OBS savedReplayPath → ffprobe argv
' boundary, proven with the REAL bundled ffprobe/ffmpeg binaries.
'
' Chain under test (production): OBS event savedReplayPath → Loader
'   HandleReplayBufferSaved → FfprobeDuration.ReadDuration → Process.Start.
' The path is attacker-influenced text from the OBS WebSocket server (any
' local process can speak that protocol on loopback).
'
' POLICY (deliberately NOT a directory clamp): the OBS replay path is
' user-configured anywhere on disk — the contract is "existing local file,
' delivered to ffprobe as ONE indivisible argument". The Engine's
' RECORD_START output-path policy is a DIFFERENT chain and is NOT exercised
' here.
'
' Hostile vectors:
'   FP-1  benign file → duration parses (positive control)
'   FP-2  quote breakout: a REAL file whose name contains `"` (creatable
'         only through the \\?\ NT namespace, which skips Win32 name
'         validation) — hand-built Arguments let the quote close the
'         argument early and append ATTACKER ffprobe arguments (witness:
'         `-report` materializes ffprobe-report*.log in the CWD);
'         correct one-argument delivery reads the SAME file and never
'         creates the witness
'   FP-3  unexpected path (URL) → File.Exists gate refuses, no process

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks

Friend Module FfprobeBoundaryTests

    Private _ffprobe As String = Nothing
    Private _ffmpeg As String = Nothing

    Private Function LocateBinaries() As Boolean
        If _ffprobe IsNot Nothing Then Return True
        Dim dir As String = AppContext.BaseDirectory
        For i As Integer = 1 To 10
            Dim probe As String = Path.Combine(dir, "Overlay", "bin", "Release", "net10.0-windows10.0.26100.0", "FFmpeg", "ffprobe.exe")
            If File.Exists(probe) Then
                _ffprobe = probe
                _ffmpeg = Path.Combine(Path.GetDirectoryName(probe), "ffmpeg.exe")
                Return File.Exists(_ffmpeg)
            End If
            Dim parent As String = Path.GetDirectoryName(dir.TrimEnd("\"c, "/"c))
            If parent Is Nothing OrElse parent = dir Then Exit For
            dir = parent
        Next
        Return False
    End Function

    Private Function NewTempDir() As String
        Dim d As String = Path.Combine(Path.GetTempPath(), "fpt-" & Guid.NewGuid().ToString("N").Substring(0, 10))
        Directory.CreateDirectory(d)
        Return d
    End Function

    Private Function ntRoot(workDir As String) As String
        Return "\\?\" & workDir
    End Function

    ' ── attacker-side native fixture creation ──────────────────────────
    ' Managed File.Copy refuses quote-in-name even with \\?\ — but a native
    ' local process is not bound by managed validation. (kernel32, raw)
    <System.Runtime.InteropServices.DllImport("kernel32.dll",
        CharSet:=System.Runtime.InteropServices.CharSet.Unicode, SetLastError:=True)>
    Private Function CreateFileW(lpFileName As String,
                                        dwDesiredAccess As UInteger,
                                        dwShareMode As UInteger,
                                        lpSecurityAttributes As IntPtr,
                                        dwCreationDisposition As UInteger,
                                        dwFlagsAndAttributes As UInteger,
                                        hTemplateFile As IntPtr) As IntPtr
    End Function

    <System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError:=True)>
    Private Function CloseHandle(hObject As IntPtr) As Boolean
    End Function

    Private Const GENERIC_WRITE As UInteger = &H40000000UI
    Private Const CREATE_ALWAYS As UInteger = 2UI
    Private Const FILE_ATTRIBUTE_NORMAL As UInteger = &H80UI
    Private Const INVALID_HANDLE_VALUE As Long = -1L

    ''' <summary>Create a file with an arbitrary (hostile) NT-namespace name.</summary>
    Private Sub CreateHostileFile(ntPath As String, sizeBytes As Integer)
        Dim h As IntPtr = CreateFileW(ntPath, GENERIC_WRITE, 0UI, IntPtr.Zero, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
        If h.ToInt64() = INVALID_HANDLE_VALUE Then
            Throw New Exception($"CreateFileW failed for hostile name (err={System.Runtime.InteropServices.Marshal.GetLastWin32Error()})")
        End If
        Using fs As New FileStream(New Microsoft.Win32.SafeHandles.SafeFileHandle(h, True), FileAccess.Write)
            fs.Write(New Byte(sizeBytes - 1) {}, 0, sizeBytes)
        End Using
    End Sub

    ''' <summary>Ladder probe: which NT-namespace name forms does CreateFileW accept?</summary>
    Private Sub ProbeNameForms(workDir As String)
        Dim forms As New List(Of String)
        forms.Add(workDir & "\plain.mp4")
        forms.Add(workDir & "\a" & ChrW(34) & "b.mp4")
        forms.Add(workDir & "\weird.mp4" & ChrW(34) & " -report " & ChrW(34))
        For Each f In forms
            Dim nt As String = "\\?\" & f
            Dim h As IntPtr = CreateFileW(nt, GENERIC_WRITE, 0UI, IntPtr.Zero, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
            If h.ToInt64() = INVALID_HANDLE_VALUE Then
                Console.WriteLine($"      [probe] ERR{System.Runtime.InteropServices.Marshal.GetLastWin32Error()} <- [{f}]")
            Else
                CloseHandle(h)
                Console.WriteLine($"      [probe] OK <- [{f}]")
            End If
        Next
    End Sub

    ''' <summary>Native existence check (GetFileAttributesW via CreateFileW probe).</summary>
    Private Function NativeFileExists(ntPath As String) As Boolean
        Dim h As IntPtr = CreateFileW(ntPath, &H80000000UI, 1UI, IntPtr.Zero, 3UI, &H80UI, IntPtr.Zero)
        If h.ToInt64() = INVALID_HANDLE_VALUE Then Return False
        CloseHandle(h)
        Return True
    End Function

    Private Function MakeOneSecondMp4(ffmpeg As String, dir As String) As String
        Dim outPath As String = Path.Combine(dir, "out.mp4")
        Dim psi As New ProcessStartInfo()
        psi.FileName = ffmpeg
        psi.Arguments = "-hide_banner -loglevel error -f lavfi -i testsrc=duration=1:size=128x96:rate=10 -pix_fmt yuv420p -y """ & outPath & """"
        psi.UseShellExecute = False
        psi.CreateNoWindow = True
        Using p As Process = Process.Start(psi)
            p.WaitForExit(30000)
            If p.ExitCode <> 0 OrElse Not File.Exists(outPath) Then
                Throw New Exception("ffmpeg fixture creation failed (exit " & p.ExitCode & ")")
            End If
        End Using
        Return outPath
    End Function

    Private Function Spawn(psi As ProcessStartInfo, ByRef stdout As String) As Integer
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True
        Using p As Process = Process.Start(psi)
            ' Read BOTH pipes concurrently — draining only stdout lets a
            ' chatty stderr (usage dumps) fill the 4KB pipe buffer and wedge
            ' the child mid-write.
            Dim outTask As Task(Of String) = p.StandardOutput.ReadToEndAsync()
            Dim errTask As Task(Of String) = p.StandardError.ReadToEndAsync()
            If Not p.WaitForExit(8000) Then
                Try : p.Kill() : Catch : End Try
            End If
            Task.WaitAll(New Task() {outTask, errTask}, 5000)
            stdout = If(outTask.Status = TaskStatus.RanToCompletion, outTask.Result.Trim(), "")
            Return If(p.HasExited, p.ExitCode, -1)
        End Using
    End Function

    Public Sub RunAll()
        Console.WriteLine(" ──── Ffprobe argv boundary (C/2) ────")

        If Not LocateBinaries() Then
            Console.WriteLine("  FP-* skipped: bundled FFmpeg/ffprobe.exe not found above " & AppContext.BaseDirectory)
            Return
        End If

        ' ── FP-1: positive control ──
        Dim work As String = NewTempDir()
        Dim src As String = MakeOneSecondMp4(_ffmpeg, work)
        TestRunner.RunTest("FP-1 benign path → duration=1 via real ffprobe", Sub()
            Dim dur As Integer = FfprobeDuration.ReadDuration(_ffprobe, src, Nothing)
            TestRunner.Assert(dur = 1, $"expected duration 1, got {dur}")
        End Sub)

        ' ── FP-2: quote breakout through a real hostile filename ──
        ' Attack model: a LOCAL NATIVE process can create names containing `"`
        ' (Win32 CreateFileW + \\?\ skips name validation — managed APIs refuse),
        ' then hand the NT-prefixed path to OBS as savedReplayPath. Whether the
        ' managed production chain can even SEE that file decides reachability.
        Dim q As Char = ChrW(34)
        Dim weirdName As String = "weird.mp4" & q & " -report " & q
        Dim weirdPath As String = work & "\" & weirdName
        Dim ntPath As String = "\\?\" & weirdPath
        ProbeNameForms(work)
        ' Native creation attempt — on Windows this is MEASURED to fail with
        ' err=123 (Win32 refuses quote-in-name even via \\?\), which makes the
        ' breakout fixture impossible and the branch below documents it.
        Dim created As Boolean = True
        Try
            CreateHostileFile(ntPath, 2048)   ' native creation — attacker side
        Catch ex As Exception
            created = False
            Console.WriteLine($"      [fp2] native fixture refused: {ex.Message}")
        End Try
        TestRunner.Assert(NativeFileExists(ntPath) OrElse Not created,
            "fixture neither created nor platform-refused")

        Dim gateSeesIt As Boolean = File.Exists(ntPath)
        Console.WriteLine($"      [fp2] File.Exists(NT quote-name) from managed .NET = {gateSeesIt}")

        If Not gateSeesIt Then
            ' Managed production chain cannot see the hostile file → the gate
            ' refuses BEFORE any spawn → argv injection unreachable from the
            ' managed chain. (Still verify the builder delivers one argument
            ' correctly for a LEGAL weird-but-passing name below in FP-2b.)
            TestRunner.RunTest("FP-2 quote-breakout filename → gate refuses before spawn (unreachable)", Sub()
                Dim sawSpawn As Boolean = False
                Dim dur As Integer = FfprobeDuration.ReadDuration(_ffprobe, ntPath,
                    Sub(m)
                        If m.Contains("exit=") Then sawSpawn = True
                    End Sub)
                TestRunner.Assert(dur = 0, "gate must refuse with 0")
                TestRunner.Assert(Not sawSpawn, "ffprobe must not be spawned for a name managed code cannot validate")
            End Sub)
        Else
            TestRunner.RunTest("FP-2 quote-breakout filename → one argument, no injected flags", Sub()
                Dim psi As ProcessStartInfo = FfprobeDuration.BuildStartInfo(_ffprobe, weirdPath)
                psi.WorkingDirectory = work          ' witness CWD for -report
                Dim stdout As String = Nothing
                Dim exitCode As Integer = Spawn(psi, stdout)

                Dim reports As String() = Directory.GetFiles(ntRoot(work), "ffprobe-report*")
                TestRunner.Assert(reports.Length = 0,
                    $"argv injection: ffprobe received attacker flags (report files: {reports.Length})")

                Dim dur As Double
                Dim parsed As Boolean = Double.TryParse(stdout, dur)
                TestRunner.Assert(exitCode = 0 AndAlso parsed AndAlso Math.Floor(dur) = 1,
                    $"expected exit=0 duration=1 for the full hostile filename, got exit={exitCode} stdout=[{stdout}]")
            End Sub)
        End If

        ' ── FP-2b: delivery correctness for a LEGAL-but-tricky name ──
        ' Spaces + trailing dots visible to managed code (trailing dot passes
        ' Win32? no — but interior spaces do): proves one-argument delivery.
        Dim tricky As String = Path.Combine(work, "clip with space & dash.mp4")
        File.Copy(src, tricky)
        TestRunner.RunTest("FP-2b legal tricky name (spaces/&/dash) → duration=1", Sub()
            Dim psi As ProcessStartInfo = FfprobeDuration.BuildStartInfo(_ffprobe, tricky)
            Dim stdout As String = Nothing
            Dim exitCode As Integer = Spawn(psi, stdout)
            Dim dur As Double
            Dim parsed As Boolean = Double.TryParse(stdout, dur)
            TestRunner.Assert(exitCode = 0 AndAlso parsed AndAlso Math.Floor(dur) = 1,
                $"expected exit=0 duration=1, got exit={exitCode} stdout=[{stdout}]")
        End Sub)

        ' ── FP-3: unexpected path (not a local file) → refused before spawn ──
        TestRunner.RunTest("FP-3 URL-like path → File.Exists gate refuses (no spawn)", Sub()
            Dim spawned As Boolean = False
            Dim dur As Integer = FfprobeDuration.ReadDuration(_ffprobe, "https://attacker.example/clip.mp4",
                Sub(m)
                    If m.Contains("exit=") Then spawned = True
                End Sub)
            TestRunner.Assert(dur = 0, "unexpected path must yield 0")
            TestRunner.Assert(Not spawned, "ffprobe must not be spawned for a non-file path")
        End Sub)

        ' cleanup
        Try
            Directory.Delete("\\?\" & work, True)
        Catch
        End Try
    End Sub

End Module
