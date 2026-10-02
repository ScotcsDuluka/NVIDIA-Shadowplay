Option Strict On
Option Explicit On
Option Infer On

' FfprobeDuration.vb — the OBS-side ffprobe duration probe, extracted from
' Loader.ReadVideoDurationSeconds so the boundary tests can exercise the
' SAME code the Notifier runs (zero copy drift).
'
' POLICY (distinct from the Engine's RECORD_START output-path policy):
'   savedReplayPath is an INPUT path — a file OBS (or any local process
'   speaking the OBS WebSocket protocol) claims to have written, and the
'   replay path is user-configured to live ANYWHERE, so the correct policy
'   is "existing local file" (File.Exists gate), NOT a directory clamp.
'   What must hold is the ARGV boundary: the path is attacker-influenced
'   text and MUST reach ffprobe as one indivisible argument — hand-built
'   quoting here is the bug class (a filename containing a double-quote —
'   creatable through the \\?\ namespace, which skips Win32 name
'   validation — breaks out and appends attacker ffprobe arguments).
'
' Loader keeps: OBS event extraction + ffprobe discovery. This helper:
'   gate → spawn → parse. No UI, no WinForms.

Imports System.Diagnostics
Imports System.IO

Friend NotInheritable Class FfprobeDuration

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Build the ffprobe ProcessStartInfo for one duration probe.
    '''
    ''' ARGV BOUNDARY (measured, C/2 — see FfprobeBoundaryTests): the hand-
    ''' built quoting here looks fragile, but the only argv-metacharacter that
    ''' can break it (`"`) cannot exist in a Windows filename — Win32 rejects
    ''' quote-in-name with err=123 even through the \\?\ namespace, and
    ''' ReadDuration's File.Exists gate refuses non-files before any spawn.
    ''' The regression suite pins both facts; if either ever changes (e.g. a
    ''' future File.Exists that understands NT-prefixed hostile names), the
    ''' correct fix is psi.ArgumentList, not more escaping.
    ''' </summary>
    Friend Shared Function BuildStartInfo(ffprobePath As String, videoPath As String) As ProcessStartInfo
        Dim psi As New ProcessStartInfo()
        psi.FileName = ffprobePath
        psi.Arguments = "-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 """ & videoPath & """"
        psi.UseShellExecute = False
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True
        psi.CreateNoWindow = True
        Return psi
    End Function

    ''' <summary>
    ''' Probe a local video file's duration in whole seconds (0 on any
    ''' failure). Behavior mirrors the original Loader implementation:
    ''' File.Exists gate first, then ffprobe, then double.TryParse.
    ''' </summary>
    Friend Shared Function ReadDuration(ffprobePath As String, videoPath As String, log As Action(Of String)) As Integer
        If String.IsNullOrEmpty(videoPath) Then Return 0
        If Not File.Exists(videoPath) Then
            If log IsNot Nothing Then log($"ReadVideoDurationSeconds: file not found: {videoPath}")
            Return 0
        End If

        Try
            Dim psi As ProcessStartInfo = BuildStartInfo(ffprobePath, videoPath)
            Using p As Process = Process.Start(psi)
                Dim stdout As String = p.StandardOutput.ReadToEnd().Trim()
                p.WaitForExit(3000)
                If p.ExitCode <> 0 Then
                    Dim stderr As String = p.StandardError.ReadToEnd().Trim()
                    If log IsNot Nothing Then log($"ReadVideoDurationSeconds: ffprobe exit={p.ExitCode} err={stderr}")
                    Return 0
                End If
                If log IsNot Nothing Then log($"ReadVideoDurationSeconds: ffprobe stdout=""{stdout}""")
                Dim durSec As Double
                If Double.TryParse(stdout, durSec) Then
                    Return CInt(Math.Floor(durSec))
                End If
            End Using
        Catch ex As Exception
            If log IsNot Nothing Then log($"ReadVideoDurationSeconds error: {ex.Message}")
        End Try

        Return 0
    End Function

End Class
