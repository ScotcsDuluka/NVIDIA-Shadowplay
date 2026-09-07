Option Strict On
Option Explicit On
Option Infer On

' Program.vb — C/3 final real-hardware soak scenarios.
'   A  repeated recording lifecycle (100, media-validated every 10th)
'   B  mixed stop stress (100: 30 normal / 20 slow / 20 timeout / 15 cancel / 15 crash)
'   C  encoder fault / recovery (30 real NVENC dimension-mismatch cycles)
'   D  dispose race (50: concurrent Stop+Dispose / Dispose-while-Running)
'   E  combined soak (200 rotating ops with cumulative resource trends)

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.Recording
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab

Friend Module SoakProgram

    Private TimeoutCyclesB As Integer = 0
    Private CrashCyclesB As Integer = 0
    Private LastThreadsE As Integer = -1
    Private MonotonicRunE As Integer = 0

    Function Main(args As String()) As Integer
        Console.WriteLine("==================================================")
        Console.WriteLine(" NVIDIA.Soak — C/3 final real-hardware soak")
        Console.WriteLine("==================================================")

        SoakShared.FfmpegPath = FindFfmpeg()
        If SoakShared.FfmpegPath Is Nothing Then
            Console.WriteLine("FATAL: ffmpeg.exe not found — soak cannot run")
            Return 2
        End If
        Console.WriteLine("ffmpeg: " & SoakShared.FfmpegPath)

        Dim repoRoot As String = AppContext.BaseDirectory
        Dim dir As New DirectoryInfo(repoRoot)
        For depth As Integer = 1 To 10
            If File.Exists(Path.Combine(dir.FullName, ".git", "HEAD")) Then
                repoRoot = dir.FullName
                Exit For
            End If
            If dir.Parent Is Nothing Then Exit For
            dir = dir.Parent
        Next
        SoakShared.Sandbox = Path.Combine(repoRoot, ".tmp", "soak")
        Directory.CreateDirectory(SoakShared.Sandbox)
        For Each stale As String In Directory.GetFiles(SoakShared.Sandbox, "*.mp4")
            Try
                File.Delete(stale)
            Catch
            End Try
        Next

        Dim proc As Process = Process.GetCurrentProcess()
        Dim baselineThreads As Integer = proc.Threads.Count
        Dim baselineHandles As Integer = proc.HandleCount
        Console.WriteLine("GPU: " & ProbeGpuName())
        Console.WriteLine("baseline: threads=" & baselineThreads & " handles=" & baselineHandles &
                          " ffmpeg=" & SoakShared.CountFfmpeg())

        Dim swAll As Stopwatch = Stopwatch.StartNew()

        ' NVIDIA detection proof: DdagrabBackend.Initialize THROWS when no
        ' 0x10DE adapter exists — a successful stack IS the detection proof.
        Using probe As New SoakStack()
            Console.WriteLine("NVIDIA detection: DdagrabBackend initialized " &
                              probe.Capture.OutputWidth & "x" & probe.Capture.OutputHeight &
                              " @ " & probe.Capture.OutputRefreshRate & "Hz on the NVIDIA adapter")
        End Using

        Dim only As String = If(args IsNot Nothing AndAlso args.Length > 0, args(0).Trim().ToUpperInvariant(), "")
        Dim failA As Integer = 0, failB As Integer = 0, failC As Integer = 0, failD As Integer = 0, failE As Integer = 0
        If only = "" OrElse only = "A" Then failA = ScenarioA(If(only = "", 100, 20))
        If only = "" OrElse only = "B" Then failB = ScenarioB(If(only = "", 100, 20))
        If only = "" OrElse only = "C" Then failC = ScenarioC(If(only = "", 30, 5))
        If only = "" OrElse only = "D" Then failD = ScenarioD(If(only = "", 50, 10))
        If only = "" OrElse only = "E" Then failE = ScenarioE(If(only = "", 200, 40))

        swAll.Stop()
        proc.Refresh()
        Dim finalFfmpeg As Integer = SoakShared.CountFfmpeg()

        Console.WriteLine()
        Console.WriteLine("==================================================")
        Console.WriteLine(" SOAK COMPLETE in " & swAll.Elapsed.TotalMinutes.ToString("0.0") & " min")
        Console.WriteLine(" final: threads=" & proc.Threads.Count & " (baseline " & baselineThreads & ")" &
                          " handles=" & proc.HandleCount & " (baseline " & baselineHandles & ")" &
                          " ffmpeg=" & finalFfmpeg)
        Console.WriteLine(" encoder instances created total: " & SoakCounters.EncoderInstancesCreated)
        Console.WriteLine(" failures: A=" & failA & " B=" & failB & " C=" & failC & " D=" & failD & " E=" & failE)
        Console.WriteLine("==================================================")
        If SoakFailures.List.Count > 0 Then
            Console.WriteLine(" FAILURES:")
            SyncLock SoakFailures.SyncRoot
                For Each f As String In SoakFailures.List
                    Console.WriteLine("  - " & f)
                Next
            End SyncLock
        End If

        If failA + failB + failC + failD + failE + finalFfmpeg = 0 Then
            Return 0
        End If
        Return 1
    End Function

    Private Function FindFfmpeg() As String
        Dim dir As New DirectoryInfo(AppContext.BaseDirectory)
        For depth As Integer = 0 To 10
            If dir Is Nothing Then Exit For
            Dim c1 As String = Path.Combine(dir.FullName, "Overlay", "bin", "Release", "net10.0-windows10.0.26100.0", "FFmpeg", "ffmpeg.exe")
            Dim c2 As String = Path.Combine(dir.FullName, "Overlay", "API-Core", "ffmpeg.exe")
            Dim c3 As String = Path.Combine(dir.FullName, "ffmpeg.exe")
            If File.Exists(c1) Then Return c1
            If File.Exists(c2) Then Return c2
            If File.Exists(c3) Then Return c3
            dir = dir.Parent
        Next
        Return Nothing
    End Function

    Private Function ProbeGpuName() As String
        Try
            Dim category As New System.Management.ManagementClass("Win32_VideoController")
            For Each o As System.Management.ManagementBaseObject In category.GetInstances()
                Dim n As String = ""
                If o("Name") IsNot Nothing Then n = o("Name").ToString()
                If n.Contains("NVIDIA") Then Return n
            Next
        Catch
        End Try
        Return "NVIDIA (name probe unavailable)"
    End Function

    ' ─────────────────────────────────────────────────────────────
    ' SCENARIO A
    ' ─────────────────────────────────────────────────────────────

    Private Function ScenarioA(iterations As Integer) As Integer
        SoakShared.LogLine("── A: repeated recording lifecycle x" & iterations & " (real NVENC output each iteration) ──")
        Dim fails As Integer = 0
        Using stack As New SoakStack()
            For i As Integer = 1 To iterations
                Dim outPath As String = Path.Combine(SoakShared.Sandbox, "a_" & i & ".mp4")
                Try
                    Dim result As SessionResult = SoakShared.RunStoppedSession(stack, outPath, 2000, "A#" & i)
                    Dim ok As Boolean = result.FramesEncoded > 0 AndAlso result.FileExists AndAlso result.Pass
                    If Not ok Then
                        fails += 1
                        SoakFailures.Record("A#" & i, "pass=" & result.Pass & " frames=" & result.FramesEncoded &
                                            " file=" & result.FileExists & " err=" & result.ErrorMessage)
                        SoakShared.LogLine("  A " & i & "/" & iterations & ": FAILED — pass=" & result.Pass &
                                           " frames=" & result.FramesEncoded & " err=" & result.ErrorMessage)
                    End If

                    SoakShared.Assert(stack.Capture.CurrentState = DdagrabBackend.DdagrabBackendState.Stopped,
                                      "A#" & i & ": capture state " & stack.Capture.CurrentState.ToString())
                    SoakShared.Assert(stack.Encoder.CurrentState = EncoderState.Stopped,
                                      "A#" & i & ": encoder state " & stack.Encoder.CurrentState.ToString())
                    SoakShared.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                      "A#" & i & ": texture imbalance")

                    If i Mod 10 = 0 Then
                        SoakShared.ValidateMedia(outPath, "A#" & i & " media")
                        Dim ff As Integer = SoakShared.CountFfmpeg()
                        SoakShared.Assert(ff = 0, "A#" & i & ": " & ff & " orphan ffmpeg after iteration")
                        SoakShared.LogLine("  A " & i & "/" & iterations & ": media ok, textures " &
                                           stack.Capture.TexturesCreated & "/" & stack.Capture.TexturesDisposed)
                    End If
                Catch ex As SoakAssertionException
                    fails += 1
                    SoakFailures.Record("A#" & i, ex.Message)
                    SoakShared.LogLine("  A " & i & "/" & iterations & ": FAILED — " & ex.Message)
                Catch ex As Exception
                    fails += 1
                    SoakFailures.Record("A#" & i, "unexpected " & ex.GetType().Name & ": " & ex.Message)
                    SoakShared.LogLine("  A " & i & "/" & iterations & ": FAILED — " & ex.GetType().Name & ": " & ex.Message)
                End Try
                Try
                    File.Delete(outPath)
                Catch
                End Try
            Next
        End Using
        SoakShared.LogLine("  A done: " & (iterations - fails) & "/" & iterations & " clean")
        Return fails
    End Function

    ' ─────────────────────────────────────────────────────────────
    ' SCENARIO B
    ' ─────────────────────────────────────────────────────────────

    Private Function ScenarioB(iterations As Integer) As Integer
        SoakShared.LogLine("── B: mixed stop stress x" & iterations & " (30 normal / 20 slow / 20 timeout / 15 cancel / 15 crash) ──")
        Dim fails As Integer = 0
        Dim plan As New List(Of String)
        For i As Integer = 1 To 30
            plan.Add("normal")
        Next
        For i As Integer = 1 To 20
            plan.Add("slow")
        Next
        For i As Integer = 1 To 20
            plan.Add("timeout")
        Next
        For i As Integer = 1 To 15
            plan.Add("cancel")
        Next
        For i As Integer = 1 To 15
            plan.Add("crash")
        Next

        Using stack As New SoakStack()
            For i As Integer = 1 To iterations
                Dim kind As String = plan(i - 1)
                Try
                    If kind = "normal" Then
                        stack.Capture.Start(New DisposingSink())
                        SoakShared.Assert(stack.Capture.CurrentState = DdagrabBackend.DdagrabBackendState.Running,
                                          "B#" & i & ": post-Start not Running")
                        Thread.Sleep(60)
                        stack.Capture.[Stop]()
                    ElseIf kind = "slow" Then
                        Dim slow As New SlowSink()
                        slow.SlowMs = 25
                        stack.Capture.Start(slow)
                        Thread.Sleep(40)
                        stack.Capture.[Stop]()
                    ElseIf kind = "timeout" Then
                        Dim gate As New ManualResetEvent(False)
                        Dim gated As New GatedSink(gate)
                        stack.Capture.Start(gated)
                        Dim sw As Stopwatch = Stopwatch.StartNew()
                        While Not gated.Entered AndAlso sw.ElapsedMilliseconds < 5000
                            Thread.Sleep(10)
                        End While
                        stack.Capture.[Stop]()
                        SoakShared.Assert(stack.Capture.CurrentState = DdagrabBackend.DdagrabBackendState.Stopping,
                                          "B#" & i & " (timeout): must hold Stopping, got " & stack.Capture.CurrentState.ToString())
                        TimeoutCyclesB += 1
                        gate.Set()
                    ElseIf kind = "cancel" Then
                        stack.Capture.Start(New DisposingSink())
                        stack.Capture.[Stop]()
                    Else
                        stack.Capture.Start(New DisposingSink())
                        SoakShared.WaitActivity(stack.Capture, Nothing, 5000)
                        stack.Capture.RequestWorkerCrashOnce()
                        CrashCyclesB += 1
                    End If

                    SoakShared.Assert(SoakShared.WaitState(stack.Capture, DdagrabBackend.DdagrabBackendState.Stopped, 10000),
                                      "B#" & i & " (" & kind & "): terminal Stopped not reached (state=" &
                                      stack.Capture.CurrentState.ToString() & ")")
                    SoakShared.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                      "B#" & i & " (" & kind & "): texture imbalance")
                Catch ex As SoakAssertionException
                    fails += 1
                    SoakFailures.Record("B#" & i & " (" & kind & ")", ex.Message)
                    SoakShared.LogLine("  B " & i & "/" & iterations & " (" & kind & "): FAILED — " & ex.Message)
                Catch ex As Exception
                    fails += 1
                    SoakFailures.Record("B#" & i & " (" & kind & ")", "unexpected " & ex.GetType().Name & ": " & ex.Message)
                    SoakShared.LogLine("  B " & i & "/" & iterations & " (" & kind & "): FAILED — " & ex.GetType().Name)
                End Try
            Next
        End Using
        SoakShared.LogLine("  B done: " & (iterations - fails) & "/" & iterations & " clean (timeout=" & TimeoutCyclesB & ", crash=" & CrashCyclesB & ")")
        Return fails
    End Function

    ' ─────────────────────────────────────────────────────────────
    ' SCENARIO C
    ' ─────────────────────────────────────────────────────────────

    Private Function ScenarioC(cycles As Integer) As Integer
        SoakShared.LogLine("── C: encoder fault / recovery x" & cycles & " (real NVENC dimension-mismatch fault) ──")
        Dim fails As Integer = 0
        Dim capture As New DdagrabBackend(New EngineLogger("soak-c", EngineLogger.LogLevel.Warning))
        capture.Initialize(New SoakBackendContext(New EngineLogger("soak-c", EngineLogger.LogLevel.Warning)))

        For c As Integer = 1 To cycles
            Dim enc As NvencEncoderBackend = Nothing
            Dim latest As New LatestSink()
            Dim fresh As NvencEncoderBackend = Nothing
            Try
                enc = New NvencEncoderBackend(New EngineLogger("soak-c", EngineLogger.LogLevel.Warning))
                Dim cfg As New EncoderConfig()
                cfg.CodecKey = "NVENC_H264"
                cfg.BitrateBps = 4000000L
                cfg.MinrateBps = 4000000L
                cfg.MaxrateBps = 4000000L
                cfg.BufsizeBps = 8000000L
                cfg.GopSize = 60
                cfg.RateControl = "cbr"
                cfg.Preset = "p4"
                cfg.FrameRateFps = 30
                cfg.ExpectedWidth = capture.OutputWidth
                cfg.ExpectedHeight = capture.OutputHeight
                cfg.EncodeWidth = capture.OutputWidth
                cfg.EncodeHeight = capture.OutputHeight
                enc.Initialize(cfg)
                SoakCounters.NoteEncoderCreated()
                enc.Start()

                capture.Start(latest)
                SoakShared.Assert(SoakShared.WaitActivity(capture, latest, 10000), "C#" & c & ": no capture activity")

                Dim goodPackets As Integer = 0
                For k As Integer = 1 To 8
                    Dim frame As IVideoFrame = latest.TakeLatest()
                    If frame Is Nothing Then
                        Thread.Sleep(40)
                    Else
                        Dim packet As EncodedPacket = Nothing
                        If enc.Encode(frame, packet) AndAlso packet IsNot Nothing Then
                            goodPackets += 1
                            packet.Dispose()
                        End If
                        frame.Dispose()
                    End If
                Next
                SoakShared.Assert(goodPackets >= 1, "C#" & c & ": 0 good NVENC encodes before the fault")

                Using wrongDims As IVideoFrame = SoakShared.MakeWrongDimsFrame()
                    Dim packet2 As EncodedPacket = Nothing
                    Dim faulted As Boolean = False
                    Try
                        enc.Encode(wrongDims, packet2)
                    Catch ex As EncoderRuntimeException
                        faulted = True
                    End Try
                    SoakShared.Assert(faulted, "C#" & c & ": wrong-dims encode did not fault the encoder")
                End Using
                SoakShared.Assert(enc.CurrentState = EncoderState.Faulted,
                                  "C#" & c & ": encoder state after fault was " & enc.CurrentState.ToString())

                For spam As Integer = 1 To 5
                    Dim f2 As IVideoFrame = latest.TakeLatest()
                    Dim threw As Boolean = False
                    Dim swSpam As Stopwatch = Stopwatch.StartNew()
                    If f2 IsNot Nothing Then
                        Try
                            Dim p3 As EncodedPacket = Nothing
                            enc.Encode(f2, p3)
                        Catch
                            threw = True
                        End Try
                        f2.Dispose()
                    Else
                        threw = True
                    End If
                    SoakShared.Assert(threw, "C#" & c & ": post-fault Encode #" & spam & " did not throw (no-spam contract)")
                    SoakShared.Assert(swSpam.ElapsedMilliseconds < 200,
                                      "C#" & c & ": post-fault Encode #" & spam & " took " & swSpam.ElapsedMilliseconds & "ms")
                Next

                capture.[Stop]()
                SoakShared.Assert(capture.CurrentState = DdagrabBackend.DdagrabBackendState.Stopped,
                                  "C#" & c & ": capture state after stop " & capture.CurrentState.ToString())
                ' Only NOW is the worker joined and the sink's retained frame
                ' the sole outstanding texture — release and assert balance.
                latest.DisposeLatest()
                SoakShared.Assert(capture.TexturesCreated = capture.TexturesDisposed,
                                  "C#" & c & ": capture texture imbalance created=" & capture.TexturesCreated & " disposed=" & capture.TexturesDisposed)

                Dim swDispose As Stopwatch = Stopwatch.StartNew()
                enc.Dispose()
                enc = Nothing
                SoakShared.Assert(swDispose.ElapsedMilliseconds < 10000,
                                  "C#" & c & ": faulted encoder dispose took " & swDispose.ElapsedMilliseconds & "ms")

                fresh = New NvencEncoderBackend(New EngineLogger("soak-c-fresh", EngineLogger.LogLevel.Warning))
                Dim cfg2 As New EncoderConfig()
                cfg2.CodecKey = "NVENC_H264"
                cfg2.BitrateBps = 4000000L
                cfg2.MinrateBps = 4000000L
                cfg2.MaxrateBps = 4000000L
                cfg2.BufsizeBps = 8000000L
                cfg2.GopSize = 60
                cfg2.RateControl = "cbr"
                cfg2.Preset = "p4"
                cfg2.FrameRateFps = 30
                cfg2.ExpectedWidth = capture.OutputWidth
                cfg2.ExpectedHeight = capture.OutputHeight
                cfg2.EncodeWidth = capture.OutputWidth
                cfg2.EncodeHeight = capture.OutputHeight
                fresh.Initialize(cfg2)
                SoakCounters.NoteEncoderCreated()
                fresh.Start()
                SoakShared.Assert(fresh.CurrentState = EncoderState.Running,
                                  "C#" & c & ": fresh encoder not Running")
                fresh.[Stop]()
                fresh.Dispose()
                fresh = Nothing
            Catch ex As SoakAssertionException
                fails += 1
                SoakFailures.Record("C#" & c, ex.Message)
                SoakShared.LogLine("  C " & c & "/" & cycles & ": FAILED — " & ex.Message)
            Catch ex As Exception
                fails += 1
                SoakFailures.Record("C#" & c, "unexpected " & ex.GetType().Name & ": " & ex.Message)
                SoakShared.LogLine("  C " & c & "/" & cycles & ": FAILED — " & ex.GetType().Name & ": " & ex.Message)
            Finally
                Try
                    If enc IsNot Nothing Then enc.Dispose()
                Catch
                End Try
                Try
                    If fresh IsNot Nothing Then fresh.Dispose()
                Catch
                End Try
                Try
                    If capture.CurrentState <> DdagrabBackend.DdagrabBackendState.Created AndAlso
                       capture.CurrentState <> DdagrabBackend.DdagrabBackendState.Stopped AndAlso
                       capture.CurrentState <> DdagrabBackend.DdagrabBackendState.Disposed Then
                        capture.[Stop]()
                    End If
                Catch
                End Try
                latest.DisposeLatest()
            End Try
        Next

        capture.Dispose()
        SoakShared.LogLine("  C done: " & (cycles - fails) & "/" & cycles & " clean")
        Return fails
    End Function

    ' ─────────────────────────────────────────────────────────────
    ' SCENARIO D
    ' ─────────────────────────────────────────────────────────────

    Private Function ScenarioD(iterations As Integer) As Integer
        SoakShared.LogLine("── D: dispose race x" & iterations & " (25 concurrent Stop+Dispose / 25 Dispose-while-Running + repeated) ──")
        Dim fails As Integer = 0
        For i As Integer = 1 To iterations
            Try
                Dim backend As New DdagrabBackend(New EngineLogger("soak-d", EngineLogger.LogLevel.Warning))
                backend.Initialize(New SoakBackendContext(New EngineLogger("soak-d", EngineLogger.LogLevel.Warning)))
                backend.Start(New LatestSink())

                If i Mod 2 = 1 Then
                    Dim exs As New List(Of String)
                    Dim t1 As Task = Task.Run(Sub()
                                                  Try
                                                      backend.[Stop]()
                                                  Catch ex As Exception
                                                      SyncLock SoakFailures.SyncRoot
                                                          exs.Add("Stop: " & ex.GetType().Name)
                                                      End SyncLock
                                                  End Try
                                              End Sub)
                    Dim t2 As Task = Task.Run(Sub()
                                                  Try
                                                      backend.Dispose()
                                                  Catch ex As Exception
                                                      SyncLock SoakFailures.SyncRoot
                                                          exs.Add("Dispose: " & ex.GetType().Name)
                                                      End SyncLock
                                                  End Try
                                              End Sub)
                    SoakShared.Assert(Task.WaitAll({t1, t2}, 15000), "D#" & i & ": concurrent Stop/Dispose deadlocked")
                    SoakShared.Assert(backend.CurrentState = DdagrabBackend.DdagrabBackendState.Disposed,
                                      "D#" & i & ": state after concurrent race was " & backend.CurrentState.ToString())
                    SoakShared.Assert(exs.Count <= 2, "D#" & i & ": unexpected exception fan-out")
                Else
                    backend.Dispose()
                    SoakShared.Assert(backend.CurrentState = DdagrabBackend.DdagrabBackendState.Disposed,
                                      "D#" & i & ": first Dispose not terminal")
                    backend.Dispose()
                    backend.Dispose()
                    Dim rejected As Boolean = False
                    Try
                        backend.Start(New LatestSink())
                    Catch ex As ObjectDisposedException
                        rejected = True
                    End Try
                    SoakShared.Assert(rejected, "D#" & i & ": Start after Dispose accepted")
                End If
            Catch ex As SoakAssertionException
                fails += 1
                SoakFailures.Record("D#" & i, ex.Message)
                SoakShared.LogLine("  D " & i & "/" & iterations & ": FAILED — " & ex.Message)
            Catch ex As Exception
                fails += 1
                SoakFailures.Record("D#" & i, "unexpected " & ex.GetType().Name & ": " & ex.Message)
                SoakShared.LogLine("  D " & i & "/" & iterations & ": FAILED — " & ex.GetType().Name)
            End Try
        Next
        SoakShared.LogLine("  D done: " & (iterations - fails) & "/" & iterations & " clean")
        Return fails
    End Function

    ' ─────────────────────────────────────────────────────────────
    ' SCENARIO E
    ' ─────────────────────────────────────────────────────────────

    Private Function ScenarioE(ops As Integer) As Integer
        SoakShared.LogLine("── E: combined soak x" & ops & " (rotating: session/cancel/crash/slow/timeout/encoder-fault/dispose+recreate) ──")
        Dim fails As Integer = 0
        Dim stack As New SoakStack()
        Dim procStart As Process = Process.GetCurrentProcess()
        baselineThreadsForE = procStart.Threads.Count

        Try
            For i As Integer = 1 To ops
                Dim kind As String = SelectOp(i)
                Dim outPath As String = Path.Combine(SoakShared.Sandbox, "e_" & i & ".mp4")
                Try
                    If kind = "session" Then
                        Dim result As SessionResult = SoakShared.RunStoppedSession(stack, outPath, 600, "E#" & i & " session")
                        SoakShared.Assert(result.Pass OrElse result.FramesEncoded > 0,
                                          "E#" & i & ": session produced nothing (err=" & result.ErrorMessage & ")")
                        If i Mod 10 = 0 Then
                            SoakShared.ValidateMedia(outPath, "E#" & i & " media")
                        End If
                    ElseIf kind = "cancel" Then
                        stack.Capture.Start(New DisposingSink())
                        stack.Capture.[Stop]()
                        SoakShared.Assert(SoakShared.WaitState(stack.Capture, DdagrabBackend.DdagrabBackendState.Stopped, 10000),
                                          "E#" & i & ": cancel not Stopped")
                    ElseIf kind = "crash" Then
                        stack.Capture.Start(New DisposingSink())
                        SoakShared.WaitActivity(stack.Capture, Nothing, 5000)
                        stack.Capture.RequestWorkerCrashOnce()
                        SoakShared.Assert(SoakShared.WaitState(stack.Capture, DdagrabBackend.DdagrabBackendState.Stopped, 10000),
                                          "E#" & i & ": crash not Stopped")
                    ElseIf kind = "slow" Then
                        Dim slow As New SlowSink()
                        slow.SlowMs = 20
                        stack.Capture.Start(slow)
                        Thread.Sleep(30)
                        stack.Capture.[Stop]()
                        SoakShared.Assert(SoakShared.WaitState(stack.Capture, DdagrabBackend.DdagrabBackendState.Stopped, 10000),
                                          "E#" & i & ": slow not Stopped")
                    ElseIf kind = "timeout" Then
                        Dim gate As New ManualResetEvent(False)
                        Dim gated As New GatedSink(gate)
                        stack.Capture.Start(gated)
                        Dim sw As Stopwatch = Stopwatch.StartNew()
                        While Not gated.Entered AndAlso sw.ElapsedMilliseconds < 5000
                            Thread.Sleep(10)
                        End While
                        stack.Capture.[Stop]()
                        SoakShared.Assert(stack.Capture.CurrentState = DdagrabBackend.DdagrabBackendState.Stopping,
                                          "E#" & i & ": timeout not Stopping")
                        gate.Set()
                        SoakShared.Assert(SoakShared.WaitState(stack.Capture, DdagrabBackend.DdagrabBackendState.Stopped, 10000),
                                          "E#" & i & ": timeout resume not Stopped")
                    ElseIf kind = "encoderfault" Then
                        RunCompactEncoderFault(stack)
                    Else
                        stack.Dispose()
                        stack = New SoakStack()
                    End If

                    SoakShared.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                      "E#" & i & " (" & kind & "): texture imbalance")
                Catch ex As SoakAssertionException
                    fails += 1
                    SoakFailures.Record("E#" & i & " (" & kind & ")", ex.Message)
                    SoakShared.LogLine("  E " & i & "/" & ops & " (" & kind & "): FAILED — " & ex.Message)
                Catch ex As Exception
                    fails += 1
                    SoakFailures.Record("E#" & i & " (" & kind & ")", "unexpected " & ex.GetType().Name & ": " & ex.Message)
                    SoakShared.LogLine("  E " & i & "/" & ops & " (" & kind & "): FAILED — " & ex.GetType().Name)
                End Try
                Try
                    File.Delete(outPath)
                Catch
                End Try

                If i Mod 20 = 0 Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    Thread.Sleep(400)   ' let D3D11/DXGI device threads retire
                    Dim proc As Process = Process.GetCurrentProcess()
                    proc.Refresh()
                    Dim ff As Integer = SoakShared.CountFfmpeg()
                    SoakShared.LogLine("  E " & i & "/" & ops & ": threads=" & proc.Threads.Count &
                                       " handles=" & proc.HandleCount & " ffmpeg=" & ff &
                                       " encoders=" & SoakCounters.EncoderInstancesCreated &
                                       " textures=" & stack.Capture.TexturesCreated & "/" & stack.Capture.TexturesDisposed)
                    SoakShared.Assert(ff = 0, "E#" & i & ": " & ff & " orphan ffmpeg at checkpoint")
                    SoakShared.Assert(stack.Capture.TexturesCreated = stack.Capture.TexturesDisposed,
                                      "E#" & i & ": cumulative texture imbalance at checkpoint")
                    TrendCheck(proc)
                End If
            Next

            Dim p2 As Process = Process.GetCurrentProcess()
            p2.Refresh()
            SoakShared.LogLine("  E final: threads=" & p2.Threads.Count & " handles=" & p2.HandleCount &
                               " ffmpeg=" & SoakShared.CountFfmpeg() & " encoders=" & SoakCounters.EncoderInstancesCreated)
            SoakShared.Assert(SoakShared.CountFfmpeg() = 0, "E: orphan ffmpeg at end")
            SoakShared.Assert(p2.Threads.Count <= baselineThreadsForE + 24,
                              "E: thread growth " & baselineThreadsForE & " -> " & p2.Threads.Count & " beyond slack")
        Finally
            Try
                stack.Dispose()
            Catch
            End Try
        End Try
        SoakShared.LogLine("  E done: " & (ops - fails) & "/" & ops & " clean")
        Return fails
    End Function

    Private baselineThreadsForE As Integer = 0

    Private Function SelectOp(i As Integer) As String
        Select Case i Mod 8
            Case 0
                Return "dispose"
            Case 1
                Return "session"
            Case 2
                Return "crash"
            Case 3
                Return "encoderfault"
            Case 4
                Return "timeout"
            Case 5
                Return "session"
            Case 6
                Return "slow"
            Case Else
                Return "cancel"
        End Select
    End Function

    ''' <summary>Compact encoder-fault op for Scenario E: real encode -> real
    ''' dimension-mismatch fault -> no-spam x2 -> clean dispose. Uses a
    ''' THROWAWAY encoder (Faulted is terminal) and keeps the stack capture
    ''' untouched unless it is idle.</summary>
    Private Sub RunCompactEncoderFault(stack As SoakStack)
        Dim tempEncoder As NvencEncoderBackend = Nothing
        Dim latest As New LatestSink()
        Dim captureStarted As Boolean = False
        Try
            tempEncoder = New NvencEncoderBackend(New EngineLogger("soak-e-enc", EngineLogger.LogLevel.Warning))
            Dim cfg As New EncoderConfig()
            cfg.CodecKey = "NVENC_H264"
            cfg.BitrateBps = 4000000L
            cfg.MinrateBps = 4000000L
            cfg.MaxrateBps = 4000000L
            cfg.BufsizeBps = 8000000L
            cfg.GopSize = 60
            cfg.RateControl = "cbr"
            cfg.Preset = "p4"
            cfg.FrameRateFps = 30
            cfg.ExpectedWidth = stack.Capture.OutputWidth
            cfg.ExpectedHeight = stack.Capture.OutputHeight
            cfg.EncodeWidth = stack.Capture.OutputWidth
            cfg.EncodeHeight = stack.Capture.OutputHeight
            tempEncoder.Initialize(cfg)
            SoakCounters.NoteEncoderCreated()
            tempEncoder.Start()

            If stack.Capture.CurrentState = DdagrabBackend.DdagrabBackendState.Stopped Then
                stack.Capture.Start(latest)
                captureStarted = True
                SoakShared.WaitActivity(stack.Capture, latest, 10000)
            End If

            Using wrongDims As IVideoFrame = SoakShared.MakeWrongDimsFrame()
                Dim p As EncodedPacket = Nothing
                Dim faulted As Boolean = False
                Try
                    tempEncoder.Encode(wrongDims, p)
                Catch ex As EncoderRuntimeException
                    faulted = True
                End Try
                SoakShared.Assert(faulted, "wrong-dims encode unexpectedly succeeded")
            End Using
            SoakShared.Assert(tempEncoder.CurrentState = EncoderState.Faulted,
                              "encoder not Faulted after mismatch")

            For spam As Integer = 1 To 2
                Dim f As IVideoFrame = latest.TakeLatest()
                Dim threw As Boolean = False
                If f IsNot Nothing Then
                    Try
                        Dim p2 As EncodedPacket = Nothing
                        tempEncoder.Encode(f, p2)
                    Catch
                        threw = True
                    End Try
                    f.Dispose()
                Else
                    threw = True
                End If
                SoakShared.Assert(threw, "post-fault Encode #" & spam & " did not throw")
            Next
        Finally
            Try
                If tempEncoder IsNot Nothing Then tempEncoder.Dispose()
            Catch
            End Try
            Try
                If captureStarted Then stack.Capture.[Stop]()
            Catch
            End Try
            latest.DisposeLatest()
        End Try
    End Sub

    Private Sub TrendCheck(proc As Process)
        Dim t As Integer = proc.Threads.Count
        If LastThreadsE > 0 Then
            If t > LastThreadsE + 4 Then
                MonotonicRunE += 1
            Else
                MonotonicRunE = 0
            End If
            SoakShared.Assert(MonotonicRunE < 5,
                              "resource trend: thread count rose monotonically for " & MonotonicRunE &
                              " consecutive checkpoints (" & LastThreadsE & " -> " & t & ") — leak suspected")
        End If
        LastThreadsE = t
    End Sub

End Module
