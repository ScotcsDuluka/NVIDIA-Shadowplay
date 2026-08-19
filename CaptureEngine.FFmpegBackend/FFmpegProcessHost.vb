Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Diagnostics
Imports System.IO

Namespace CaptureEngine.FFmpegBackend
    ''' <summary>
    ''' Manages a single FFmpeg process lifecycle: spawn, stdin 'q' quit,
    ''' stderr redirection, WaitForExit, Kill.
    '''
    ''' P1-C SKELETON — empty stub. Implementation in next commit.
    '''
    ''' Thread safety:
    '''   - Start/Stop/Kill are NOT thread-safe by themselves.
    '''   - Caller (FFmpegPipelineBackend) must serialize access via SyncLock.
    '''   - The Exited event fires on a threadpool thread.
    '''
    ''' Critical rule (per OWNER):
    '''   ❌ NEVER hold a lock across WaitForExit() / Kill() / Mux() / ffprobe()
    '''   ✅ Capture state under lock → release lock → call blocking ops → re-acquire
    ''' </summary>
    Public NotInheritable Class FFmpegProcessHost
        Implements IDisposable

        Private _process As Process
        Private _disposed As Boolean = False

        ''' <summary>FFmpeg executable path (set by caller before Start).</summary>
        Public Property FFmpegPath As String = ""

        ''' <summary>FFmpeg argument string (built by FFmpegCommandBuilder).</summary>
        Public Property Arguments As String = ""

        ''' <summary>Output file path (for diagnostics / mux coordination).</summary>
        Public Property OutputPath As String = ""

        ''' <summary>Raised when the FFmpeg process exits (graceful or crash).</summary>
        Public Event Exited As Action(Of Integer)

        ''' <summary>Raised for each stderr line (parsed by FFmpegStderrParser).</summary>
        Public Event StderrLine As Action(Of String)

        ''' <summary>Start the FFmpeg process.</summary>
        Public Sub Start()
            ' TODO (next commit): spawn Process, redirect stderr, wire Exited handler
            Throw New NotImplementedException("FFmpegProcessHost.Start — skeleton only")
        End Sub

        ''' <summary>Send 'q' to FFmpeg stdin (graceful stop).</summary>
        Public Sub SendQuit()
            ' TODO (next commit): write 'q' + LF to stdin, flush
            Throw New NotImplementedException("FFmpegProcessHost.SendQuit — skeleton only")
        End Sub

        ''' <summary>Wait for FFmpeg to exit (with timeout).</summary>
        Public Function WaitForExit(timeoutMs As Integer) As Boolean
            ' TODO (next commit): _process.WaitForExit(timeoutMs)
            Throw New NotImplementedException("FFmpegProcessHost.WaitForExit — skeleton only")
        End Function

        ''' <summary>Force-kill the FFmpeg process.</summary>
        Public Sub Kill()
            ' TODO (next commit): _process.Kill() + WaitForExit(2000)
            Throw New NotImplementedException("FFmpegProcessHost.Kill — skeleton only")
        End Sub

        ''' <summary>True if the process has exited.</summary>
        Public ReadOnly Property HasExited As Boolean
            Get
                Return _process IsNot Nothing AndAlso _process.HasExited
            End Get
        End Property

        ''' <summary>Exit code of the process (valid after HasExited = True).</summary>
        Public ReadOnly Property ExitCode As Integer
            Get
                If _process Is Nothing OrElse Not _process.HasExited Then Return -1
                Return _process.ExitCode
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            ' TODO (next commit): dispose process + streams
            If _process IsNot Nothing Then
                _process.Dispose()
                _process = Nothing
            End If
        End Sub
    End Class
End Namespace
