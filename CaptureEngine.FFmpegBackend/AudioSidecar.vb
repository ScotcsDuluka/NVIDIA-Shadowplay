Option Strict On
Option Explicit On
Option Infer On

Imports System

Namespace CaptureEngine.FFmpegBackend
    ''' <summary>
    ''' Audio sidecar: records WASAPI audio to temp .wav files independently
    ''' of the FFmpeg video process.
    '''
    ''' P1-C SKELETON — empty stub. Implementation in next commit.
    '''
    ''' Architecture (per Engine-Audio two-process design):
    '''   - Start() is called AFTER FFmpegProcessHost.Start() (video starts first)
    '''   - Stop() is called AFTER FFmpegProcessHost exits (audio records through
    '''     FFmpeg shutdown time; extra audio is trimmed by mux -t <videoDuration>)
    '''   - Two temp .wav files: temp.system.wav + temp.mic.wav
    '''   - Per-track start timestamps captured for mux offset calculation
    '''
    ''' Thread safety:
    '''   - Start/Stop are called from the FFmpegPipelineBackend thread (serialized)
    '''   - WASAPI callbacks fire on threadpool threads
    '''   - Internal state protected by SyncLock
    ''' </summary>
    Public NotInheritable Class AudioSidecar
        Implements IDisposable

        Private ReadOnly _sync As New Object()
        Private _disposed As Boolean = False
        Private _started As Boolean = False

        ' Temp file paths (set by caller before Start)
        Public Property TempSystemWavPath As String = ""
        Public Property TempMicWavPath As String = ""

        ' Config flags
        Public Property SystemAudioEnabled As Boolean = False
        Public Property MicEnabled As Boolean = False

        ' Per-track start timestamps (QPC ticks, for mux offset)
        Public Property SystemStartTicks As Long = 0
        Public Property MicStartTicks As Long = 0

        ''' <summary>Start audio recording (WASAPI loopback + optional mic capture).</summary>
        Public Sub Start()
            If Not (SystemAudioEnabled OrElse MicEnabled) Then Return
            ' TODO (next commit): initialize WASAPI, create WaveFileWriter(s)
            Throw New NotImplementedException("AudioSidecar.Start — skeleton only")
        End Sub

        ''' <summary>Stop audio recording + finalize .wav files (write correct WAV headers).</summary>
        Public Sub [Stop]()
            If Not _started Then Return
            ' TODO (next commit): stop WASAPI, dispose WaveFileWriter(s)
            Throw New NotImplementedException("AudioSidecar.Stop — skeleton only")
        End Sub

        ''' <summary>True if at least one audio track has data (non-empty .wav).</summary>
        Public ReadOnly Property HasAudioData As Boolean
            Get
                ' TODO (next commit): check .wav file sizes > 44 bytes (WAV header)
                Return False
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            ' TODO (next commit): dispose WASAPI resources
        End Sub
    End Class
End Namespace
