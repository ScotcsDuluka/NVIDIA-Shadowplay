Option Strict On
Option Explicit On
Option Infer On

' H1OutputPathTests.vb — C/5 RECORD_START output-path security contract
' (deterministic, no ffmpeg, no hardware, no recording).
'
' Chain under test (C/5):
'   IPC client → Hub (loopback-only since F-04) → RECORD_START:<path> →
'   [Engine] Client.vb parser → RecordingEngineHost.HandleRecordingStart /
'   HandleEngineRecordStart (legacy) → NextRecordingConfig.MapSessionConfig
'   → SessionConfig.OutputPath → CaptureSession → LiveMuxSession
'   (_fragPath/_finalPath) → FFmpeg ProcessStart arguments.
'
' CONTRACT (product design, not invented):
'   The output DIRECTORY authority is the user — config.json
'   Paths.GalleryPath/SavePath is a user-settable arbitrary directory, and
'   the Overlay sends per-recording paths under it. Absolute paths, other
'   drives, UNC shares, traversal relative to the chosen root, spaces and
'   unicode are therefore BY DESIGN and must remain accepted.
'
'   What can NEVER occur in a legitimate Windows file path is a double
'   quote or a control character (NTFS forbids both in names). Their only
'   possible effect is Windows argument-parsing breakout when the path is
'   embedded into the FFmpeg command line — every production builder quotes
'   but does not escape: LiveMuxSession BuildArgs()/RunRemux()
'   (""{_fragPath}"" / ""{_finalPath}"") and legacy FFmpegArgumentBuilder
'   (-y ""{outputFile}""). A breakout injects extra FFmpeg outputs/flags
'   (arbitrary file write via the -y overwrite path) — so these two
'   character classes are REJECTED at the single mapping seam every
'   RECORD_START crosses.
'
' Root causes separated (C/5 rule — one fix per root cause):
'   (a) path policy        → by design, unchanged (asserted by H1-D)
'   (b) FFmpeg quoting     → neutralized by rejecting quote/control chars
'   (c) arbitrary write    → enabled by (b); closed with it
'   (d) shell injection    → not present (UseShellExecute=False everywhere)

Imports System
Imports CaptureEngine.Recording
Imports Engine.ConfigTruth.Tests

Friend Module H1OutputPathTests

    Friend Sub RunAll()
        TestRunner.RunTest("H1-A: MapSessionConfig REJECTS quote-breakout output path",
                           AddressOf H1A_RejectQuoteBreakout)
        TestRunner.RunTest("H1-B: MapSessionConfig REJECTS control characters in output path",
                           AddressOf H1B_RejectControlChars)
        TestRunner.RunTest("H1-C: MapSessionConfig REJECTS empty / Nothing output path",
                           AddressOf H1C_RejectEmpty)
        TestRunner.RunTest("H1-D: policy preserved — normal/./.. /drive/UNC/\\?\ /spaces/unicode/trailing chars accepted",
                           AddressOf H1D_PolicyPreserved)
        TestRunner.RunTest("H1-E: BuildSessionConfig routes the same rejection",
                           AddressOf H1E_BuildSessionConfigThrows)
    End Sub

    ''' <summary>A: the classic breakout shape — a quote closes the output
    ''' argument early and smuggles new FFmpeg arguments after it. The
    ''' mapping seam must refuse to carry this string toward ProcessStart.</summary>
    Private Sub H1A_RejectQuoteBreakout()
        Dim hostile As String = "out"" ""-i"" ""\\attacker\share\x.mp4"
        Dim threw As Boolean = False
        Try
            NextRecordingConfig.MapSessionConfig(Nothing, hostile, "ffmpeg", Nothing)
        Catch ex As ArgumentException
            threw = True
        End Try
        TestRunner.Assert(threw, "quote-bearing output path must be rejected at the mapping seam")
    End Sub

    ''' <summary>B: control characters cannot occur in NTFS names either and
    ''' are argument/protocol smuggling surface — same rejection.</summary>
    Private Sub H1B_RejectControlChars()
        For Each hostile In New String() {"out" & vbLf & ".mp4", "out" & vbTab & ".mp4", "out" & Chr(1) & ".mp4"}
            Dim threw As Boolean = False
            Try
                NextRecordingConfig.MapSessionConfig(Nothing, hostile, "ffmpeg", Nothing)
            Catch ex As ArgumentException
                threw = True
            End Try
            TestRunner.Assert(threw, "control character in output path must be rejected: " & hostile.Replace(vbLf, "\n").Replace(vbTab, "\t"))
        Next
    End Sub

    ''' <summary>C: an empty target names no output at all — reject with the
    ''' same seam (never spawn FFmpeg without an output path).</summary>
    Private Sub H1C_RejectEmpty()
        For Each hostile As String In New String() {Nothing, "", " ", vbTab}
            Dim threw As Boolean = False
            Try
                NextRecordingConfig.MapSessionConfig(Nothing, hostile, "ffmpeg", Nothing)
            Catch ex As ArgumentException
                threw = True
            End Try
            TestRunner.Assert(threw, "empty/blank output path must be rejected")
        Next
    End Sub

    ''' <summary>D: POLICY PRESERVATION — everything a real user/gallery can
    ''' produce must keep mapping. If any of these start failing, the fix
    ''' over-reached into the user's directory authority.</summary>
    Private Sub H1D_PolicyPreserved()
        Dim allowed As String() = {
            "Record_2026-09-06.mp4",                          ' normal
            "C:\Users\me\Videos\Shadowplay\Gallery\r.mp4",    ' absolute
            "..\outside.mp4", "..\..\outside.mp4",            ' traversal (relative to gallery root)
            "C:relative.mp4",                                 ' drive-relative
            "\\server\share\out.mp4",                         ' UNC
            "\\?\C:\very\deep\out.mp4",                       ' extended-length
            "file with spaces.mp4",                           ' spaces
            "อัดวีดีโอ_ก่อนเที่ยง.mp4",                        ' unicode
            "out.mp4.", "out.mp4 ",                           ' trailing dot / space (FS normalizes)
            "CON.mp4"                                         ' reserved name (FS decides, no arg risk)
        }
        For Each candidate As String In allowed
            Dim cfg As SessionConfig = NextRecordingConfig.MapSessionConfig(Nothing, candidate, "ffmpeg", Nothing)
            TestRunner.Assert(cfg.OutputPath = candidate, "policy path must map verbatim: " & candidate)
        Next
    End Sub

    ''' <summary>E: the ConsoleDriver / CT-4 entry (BuildSessionConfig) goes
    ''' through the same seam — same rejection, so no second unguarded door.</summary>
    Private Sub H1E_BuildSessionConfigThrows()
        Dim hostile As String = "out"" ""-y"" ""C:\Users\me\Desktop\evil.mp4"
        Dim threw As Boolean = False
        Try
            NextRecordingConfig.BuildSessionConfig(hostile, "ffmpeg", Nothing)
        Catch ex As ArgumentException
            threw = True
        End Try
        TestRunner.Assert(threw, "BuildSessionConfig must route the same rejection")
    End Sub

End Module
