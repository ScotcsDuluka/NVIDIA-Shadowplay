Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic

Namespace CaptureEngine.FFmpegBackend
    ''' <summary>
    ''' Parses FFmpeg stderr output lines into structured diagnostics.
    '''
    ''' P1-C SKELETON — empty stub. Implementation in next commit.
    '''
    ''' Parses lines like:
    '''   frame= 6540 fps=143 q=8.0 size= 94278KiB time=00:00:45.41 bitrate=17005.4kbits/s dup=760 drop=1 speed=0.997x
    '''   [error] Could not open output file
    '''   Lsize= 94300KiB time=00:00:60.00 bitrate=12870.0kbits/s speed=1.01x
    '''
    ''' Thread safety:
    '''   - Called from Process.ErrorDataReceived (threadpool thread).
    '''   - GetSnapshot() may be called from any thread.
    '''   - All mutations use SyncLock.
    ''' </summary>
    Public NotInheritable Class FFmpegStderrParser
        Private ReadOnly _sync As New Object()

        ' Parsed stats (updated on each stderr line)
        Private _frameCount As Long = 0
        Private _fps As Double = 0
        Private _dupCount As Long = 0
        Private _dropCount As Long = 0
        Private _speed As Double = 0
        Private _lastSizeBytes As Long = 0
        Private _lastError As String = ""
        Private _hasError As Boolean = False

        ''' <summary>Process a single stderr line from FFmpeg.</summary>
        Public Sub ProcessLine(line As String)
            If String.IsNullOrEmpty(line) Then Return
            ' TODO (next commit): parse frame=, fps=, dup=, drop=, speed=, [error]
            Throw New NotImplementedException("FFmpegStderrParser.ProcessLine — skeleton only")
        End Sub

        ''' <summary>Get a snapshot of all parsed stats (thread-safe, never throws).</summary>
        Public Function GetSnapshot() As IReadOnlyDictionary(Of String, Long)
            SyncLock _sync
                Return New Dictionary(Of String, Long) From {
                    {"frame", _frameCount},
                    {"fps", CLng(_fps)},
                    {"dup", _dupCount},
                    {"drop", _dropCount},
                    {"speed", CLng(_speed * 1000)}
                }
            End SyncLock
        End Function

        ''' <summary>True if an [error] line was detected.</summary>
        Public ReadOnly Property HasError As Boolean
            Get
                SyncLock _sync
                    Return _hasError
                End SyncLock
            End Get
        End Property

        ''' <summary>Last error message from stderr (empty if no error).</summary>
        Public ReadOnly Property LastError As String
            Get
                SyncLock _sync
                    Return _lastError
                End SyncLock
            End Get
        End Property
    End Class
End Namespace
