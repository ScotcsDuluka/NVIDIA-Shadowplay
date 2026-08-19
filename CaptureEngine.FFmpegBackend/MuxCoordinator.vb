Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Diagnostics
Imports System.IO

Namespace CaptureEngine.FFmpegBackend
    ''' <summary>
    ''' Coordinates the mux step: ffprobe (get video duration) + FFmpeg mux
    ''' (combine temp video + temp audio → final output) + temp file cleanup.
    '''
    ''' P1-C SKELETON — empty stub. Implementation in next commit.
    '''
    ''' Architecture (per Engine-Audio two-process design):
    '''   - Created lazily at FFmpegPipelineBackend.Stop() time (if audio enabled)
    '''   - Step 1: ffprobe temp.video.mp4 → get exact video duration (ms precision)
    '''   - Step 2: compute per-track audio offset (videoStart - audioStart)
    '''   - Step 3: spawn mux FFmpeg: -i video.mp4 -i system.wav [-i mic.wav]
    '''             -c:v copy -c:a aac -ss <offset> -t <duration> → final.mp4
    '''   - Step 4: delete temp files (only if mux succeeded)
    '''
    ''' Thread safety:
    '''   - Run() is called from FFmpegPipelineBackend.Stop() (serialized)
    '''   - Not concurrent — only one mux at a time
    '''   - Guarded by FFmpegPipelineBackend._muxCompleted (Interlocked.Exchange)
    ''' </summary>
    Public NotInheritable Class MuxCoordinator
        Implements IDisposable

        Private ReadOnly _sync As New Object()
        Private _disposed As Boolean = False

        ' Paths (set by caller before Run)
        Public Property FFmpegPath As String = ""
        Public Property TempVideoPath As String = ""
        Public Property TempSystemWavPath As String = ""
        Public Property TempMicWavPath As String = ""
        Public Property OutputPath As String = ""

        ' Per-track sync offsets (computed by caller)
        Public Property SystemOffsetSec As Double = 0
        Public Property MicOffsetSec As Double = 0

        ' Video duration (filled by ffprobe, used by mux -t)
        Public Property VideoDurationSec As Double = 0

        ''' <summary>Get exact video duration via ffprobe (ms precision).</summary>
        Public Function ProbeVideoDuration() As Double
            ' TODO (next commit): spawn ffprobe, parse "5.171000" output
            Throw New NotImplementedException("MuxCoordinator.ProbeVideoDuration — skeleton only")
        End Function

        ''' <summary>Run the mux FFmpeg process (video + audio → final output).</summary>
        ''' <returns>True if mux succeeded (exit code 0); False if failed.</returns>
        Public Function Run() As Boolean
            ' TODO (next commit): build mux args, spawn FFmpeg, wait, check exit code
            Throw New NotImplementedException("MuxCoordinator.Run — skeleton only")
        End Function

        ''' <summary>Delete temp files (only called if mux succeeded).</summary>
        Public Sub CleanupTempFiles()
            ' TODO (next commit): delete temp video + temp wavs
            DeleteIfExists(TempVideoPath)
            DeleteIfExists(TempSystemWavPath)
            DeleteIfExists(TempMicWavPath)
        End Sub

        Private Sub DeleteIfExists(path As String)
            If String.IsNullOrEmpty(path) Then Return
            Try
                If File.Exists(path) Then File.Delete(path)
            Catch
            End Try
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
        End Sub
    End Class
End Namespace
