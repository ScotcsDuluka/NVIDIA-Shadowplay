Option Strict On
Option Explicit On
Option Infer On

' FrameRetirementContractTests.vb — F-05 regression pin (C/6 evidence).
'
' CaptureSession's CFR-loop tail retires the two retained GPU frames
' (pendingFrame = freshest captured, lastFrame = last displayed). The
' final fresh-frame encode transfers ownership ONLY on success, so when
' it throws (Catch swallows → control reaches the tail normally),
' pendingFrame still owns a D3D11 staging texture. The guard that retires
' it MUST execute BEFORE the `pendingFrame = Nothing` assignment — a
' premature assignment turned the guard into dead code and stranded one
' staging texture per failure (D3D11VideoFrame has no finalizer).
'
' This test pins the ORDER in the real production source: it fails on the
' pre-F-05 code (guard after the assignment) and passes on the fixed
' control flow. It also pins the exception-path backstop (the session
' Finally disposes both retained frames on every exit path).

Imports System
Imports System.IO
Imports Engine.ConfigTruth.Tests

Friend Module FrameRetirementContractTests

    Private Const CaptureSessionVb As String = "CaptureEngine.Recording/CaptureSession.vb"

    Private Function RepoRoot() As String
        Dim dir As New DirectoryInfo(AppContext.BaseDirectory)
        For depth As Integer = 0 To 12
            Dim probe As String = Path.Combine(dir.FullName, "Engine.ConfigTruth.Tests")
            If Directory.Exists(probe) AndAlso
               Directory.Exists(Path.Combine(dir.FullName, "docs")) AndAlso
               Directory.Exists(Path.Combine(dir.FullName, ".git")) Then
                Return dir.FullName
            End If
            If dir.Parent Is Nothing Then Exit For
            dir = dir.Parent
        Next
        Throw New Exception("repo root not found above " & AppContext.BaseDirectory)
    End Function

    Public Sub RunAll()
        Console.WriteLine()
        Console.WriteLine("── F-05 frame-retirement order (CaptureSession tail) ──")
        TestRunner.RunTest("F-05: retire-guard runs BEFORE the Nothing assignment (no dead guard)",
                           AddressOf T_GuardPrecedesNothing)
        TestRunner.RunTest("F-05: session Finally disposes both retained frames on every exit path",
                           AddressOf T_FinallyBackstop)
    End Sub

    Private Sub T_GuardPrecedesNothing()
        Dim src As String = File.ReadAllText(
            Path.Combine(RepoRoot(), CaptureSessionVb.Replace("/"c, Path.DirectorySeparatorChar)))

        Dim guardNeedle As String = "If pendingFrame IsNot Nothing AndAlso pendingFrame IsNot lastFrame Then"
        Dim guardIdx As Integer = src.LastIndexOf(guardNeedle, StringComparison.Ordinal)
        Assert(guardIdx >= 0, "retire guard present in CaptureSession tail")

        ' The tail assignment is the LAST `pendingFrame = Nothing` in the
        ' method — the guard must strictly precede it, otherwise the guard
        ' reads a Nothing reference and never fires (the F-05 bug shape).
        Dim nothingIdx As Integer = src.LastIndexOf("pendingFrame = Nothing", StringComparison.Ordinal)
        Assert(nothingIdx > guardIdx,
               "guard must run BEFORE `pendingFrame = Nothing` " &
               $"(guard@{guardIdx}, nothing@{nothingIdx})")

        ' Both retained frames must reach the disposer.
        Assert(src.Contains("frameDisposer.Enqueue(pendingFrame)"),
               "pendingFrame handed to the disposer")
        Assert(src.Contains("frameDisposer.Enqueue(lastFrame)"),
               "lastFrame handed to the disposer")
    End Sub

    Private Sub T_FinallyBackstop()
        Dim src As String = File.ReadAllText(
            Path.Combine(RepoRoot(), CaptureSessionVb.Replace("/"c, Path.DirectorySeparatorChar)))

        ' Exception paths: the session Finally is the backstop that disposes
        ' the retained frames on every exit path (one-shot Dispose makes the
        ' happy path a no-op there).
        Assert(src.Contains("Try : pendingFrame?.Dispose() : Catch : End Try"),
               "Finally disposes pendingFrame")
        Assert(src.Contains("Try : lastFrame?.Dispose() : Catch : End Try"),
               "Finally disposes lastFrame")
    End Sub

End Module
