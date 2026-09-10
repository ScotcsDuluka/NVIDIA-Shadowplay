Option Strict On
Option Explicit On
Option Infer On

' Gallery.Video.Tests — console runner harness.
'
' Pattern cloned from Engine.Concurrency.Tests/Program.vb (repo convention):
'   RunTest / RunSkip / Assert + honest SKIPs (never converted to PASS).
'
' Environment seams:
'   GALLERY_FFMPEG_DIR — dir containing ffmpeg(.exe) + ffprobe(.exe).
'   When absent: ffmpeg/ffprobe resolved from PATH (Linux dev box has them).
'   When NEITHER yields a usable binary: integration tiers report SKIP with
'   the reason (same discipline as RRT_FFMPEG in RuntimeSyncTests).
'
' CLI: optional suite-name args select a subset (slow boxes / CI shards):
'   dotnet run -- SessionTests StressLoopTests
'   Names are the test module class names; no args → full suite (default).

Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Namespace Gallery.Video.Tests

    Friend Class SkipException
        Inherits Exception
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

    Friend Module TestRunner

        Friend _passed As Integer = 0
        Friend _failed As Integer = 0
        Friend _skipped As Integer = 0
        Friend ReadOnly _failures As New List(Of String)()
        Friend ReadOnly _skips As New List(Of String)()

        Friend Sub RunTest(name As String, test As Action)
            Console.Write($"  {name} ... ")
            Try
                test()
                Console.WriteLine("PASS")
                _passed += 1
            Catch ex As SkipException
                Console.WriteLine("SKIP")
                Console.WriteLine($"      → {ex.Message}")
                _skips.Add(name & ": " & ex.Message)
                _skipped += 1
            Catch ex As Exception
                Console.WriteLine("FAIL")
                Console.WriteLine($"      → {ex.Message}")
                _failures.Add(name & ": " & ex.Message)
                _failed += 1
            End Try
        End Sub

        Friend Sub Assert(cond As Boolean, message As String)
            If Not cond Then Throw New Exception(message)
        End Sub

        Friend Sub AssertEqual(expected As Object, actual As Object, what As String)
            If Not Object.Equals(expected, actual) Then
                Throw New Exception($"{what}: expected {expected}, got {actual}")
            End If
        End Sub
    End Module

    Friend Module Program

        ''' <summary>Unix hard exit: skips the managed shutdown entirely.
        ''' Evidence (Linux box): the full suite prints its summary then — in
        ''' ~3 of 5 observed runs — the CALLER (sandbox/CI) was held open even
        ''' though the summary was complete. Two independent causes are covered
        ''' below: (a) managed teardown hanging on a native wait → libc exit;
        ''' (b) a rare orphaned ffmpeg blocked forever on a full stdout pipe
        ''' after its parent vanished → KillOrphanedChildren before exit.
        ''' Tests already stopped every worker they own before this point.</summary>
        ' NOTE: no explicit 'Shared' here — Module members are implicitly
        ' Shared in VB, and an explicit modifier is a compile error (BC30433).
        ' (Landed in 7c49302 after the last rebuild — caught on first fresh-
        ' sandbox build; proof the shard discipline must include a build.)
        <Runtime.InteropServices.DllImport("libc", EntryPoint:="exit")>
        Private Sub LibcExit(status As Integer)
        End Sub

        ''' <summary>Kill any direct child still alive (ppid == us). On Linux
        ''' this reads /proc — no external tools. A blocked-on-write ffmpeg
        ''' orphan must never outlive the runner: it holds the caller (sandbox,
        ''' CI job, interactive shell) open indefinitely.</summary>
        Private Sub KillOrphanedChildren()
            Try
                If Not RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then Return
                Dim myPid = Process.GetCurrentProcess().Id
                For Each procDir In IO.Directory.GetDirectories("/proc")
                    Dim name = IO.Path.GetFileName(procDir)
                    Dim pid As Integer
                    If Not Integer.TryParse(name, pid) OrElse pid = myPid Then Continue For
                    Try
                        Dim stat = IO.File.ReadAllText(IO.Path.Combine(procDir, "stat"))
                        ' Field 4 (ppid) sits after the comm field, which may
                        ' itself contain spaces inside parentheses → parse
                        ' after the LAST ')'.
                        Dim close = stat.LastIndexOf(")"c)
                        If close < 0 OrElse close + 2 >= stat.Length Then Continue For
                        Dim fields = stat.Substring(close + 2).Split(" "c)
                        ' fields(0)=state, fields(1)=ppid (fields of stat minus comm)
                        If fields.Length < 2 Then Continue For
                        Dim ppid As Integer
                        If Integer.TryParse(fields(1), ppid) AndAlso ppid = myPid Then
                            Using p As Process = Process.GetProcessById(pid)
                                Try : p.Kill() : Catch : End Try
                            End Using
                        End If
                    Catch
                        ' raced exit / permission — not ours to worry about
                    End Try
                Next
            Catch
                ' hygiene must never break the exit path
            End Try
        End Sub

        ''' <summary>Suite filter: no args → every module; otherwise only the
        ''' named modules run (OrdinalIgnoreCase). Unknown names are ignored
        ''' (they simply match nothing — the summary still reports totals).</summary>
        Private Function Want(args As String(), moduleName As String) As Boolean
            If args Is Nothing OrElse args.Length = 0 Then Return True
            For Each a In args
                If String.Equals(If(a, "").Trim(), moduleName, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
            Return False
        End Function

        Function Main(args As String()) As Integer
            Console.WriteLine("==================================================")
            Console.WriteLine(" Gallery.Video.Tests — playback engine prototype")
            If args IsNot Nothing AndAlso args.Length > 0 Then
                Console.WriteLine($" suite filter: {String.Join(", ", args)}")
            End If
            Console.WriteLine("==================================================")

            ' ---- Environment resolution (honest gates) ----
            TestMedia.LocateBinaries()
            Console.WriteLine($" ffmpeg : {If(TestMedia.FfmpegPath <> "", TestMedia.FfmpegPath, "(not found)")}")
            Console.WriteLine($" ffprobe: {If(TestMedia.FfprobePath <> "", TestMedia.FfprobePath, "(not found)")}")
            Console.WriteLine()

            ' ---- Tier 1: pure unit tests (no binaries needed) ----
            If Want(args, NameOf(StateMachineTests)) OrElse Want(args, NameOf(FrameQueueTests)) OrElse
               Want(args, NameOf(PlaybackClockTests)) Then
                Console.WriteLine(" [Tier 1] Unit — state machine, frame queue, clock")
                If Want(args, NameOf(StateMachineTests)) Then StateMachineTests.RunAll(AddressOf TestRunner.RunTest)
                If Want(args, NameOf(FrameQueueTests)) Then FrameQueueTests.RunAll(AddressOf TestRunner.RunTest)
                If Want(args, NameOf(PlaybackClockTests)) Then PlaybackClockTests.RunAll(AddressOf TestRunner.RunTest)
                Console.WriteLine()
            End If

            ' ---- Tier 2: deterministic integration (REAL ffmpeg/ffprobe) ----
            If Want(args, NameOf(MediaProbeTests)) OrElse Want(args, NameOf(DecodeIntegrationTests)) OrElse
               Want(args, NameOf(SeekIntegrationTests)) OrElse Want(args, NameOf(FaultIntegrationTests)) OrElse
               Want(args, NameOf(SessionTests)) Then
                Console.WriteLine(" [Tier 2] Deterministic integration — real ffmpeg/ffprobe")
                If Want(args, NameOf(MediaProbeTests)) Then MediaProbeTests.RunAll(AddressOf TestRunner.RunTest)
                If Want(args, NameOf(DecodeIntegrationTests)) Then DecodeIntegrationTests.RunAll(AddressOf TestRunner.RunTest)
                If Want(args, NameOf(SeekIntegrationTests)) Then SeekIntegrationTests.RunAll(AddressOf TestRunner.RunTest)
                If Want(args, NameOf(FaultIntegrationTests)) Then FaultIntegrationTests.RunAll(AddressOf TestRunner.RunTest)
                If Want(args, NameOf(SessionTests)) Then SessionTests.RunAll(AddressOf TestRunner.RunTest)
                Console.WriteLine()
            End If

            ' ---- Tier 2b: stress loops (real ffmpeg, off-hardware safe) ----
            If Want(args, NameOf(StressLoopTests)) Then
                Console.WriteLine(" [Tier 2b] Stress — open/close A↔B loops")
                StressLoopTests.RunAll(AddressOf TestRunner.RunTest)
                Console.WriteLine()
            End If

            ' ---- Tier 3: hardware smoke (Windows + GPU only) ----
            If Want(args, NameOf(HardwareGatedTests)) Then
                Console.WriteLine(" [Tier 3] Hardware smoke — honest gates")
                HardwareGatedTests.RunAll(AddressOf TestRunner.RunTest)
                Console.WriteLine()
            End If

            ' ---- Summary ----
            Console.WriteLine("--------------------------------------------------")
            Console.WriteLine($" PASS {_passed}   FAIL {_failed}   SKIP {_skipped} (honest gates)")
            For Each f In TestRunner._failures
                Console.WriteLine($"   FAIL: {f}")
            Next
            For Each s In TestRunner._skips
                Console.WriteLine($"   SKIP: {s}")
            Next
            Console.WriteLine("--------------------------------------------------")

            Dim exitCode = If(TestRunner._passed + TestRunner._failed + TestRunner._skipped = 0, 2,
                           If(TestRunner._failed = 0, 0, 1))
            ' Exit code 2 = filter matched nothing (shard misconfiguration must
            ' never masquerade as a green run).
            ' Deterministic process exit: tests spawn subprocesses and worker
            ' threads; a lingering handle must never hang the runner after the
            ' summary is printed. Flush first (libc exit skips managed flush),
            ' then hard-exit on Unix; Environment.Exit stays as the Windows
            ' path (the owner's Tier-3 box).
            Console.Out.Flush()
            Console.Error.Flush()
            KillOrphanedChildren()
            If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                Environment.Exit(exitCode)
            Else
                LibcExit(exitCode)
            End If
            Return exitCode
        End Function

    End Module

End Namespace
