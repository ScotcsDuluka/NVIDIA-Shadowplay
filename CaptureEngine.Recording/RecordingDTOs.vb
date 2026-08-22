Option Strict On
Option Explicit On
Option Infer On

' DTOs for CaptureEngine.Recording

Namespace CaptureEngine.Recording

    ''' <summary>
    ''' Recording engine state. Distinct from Foundation's EngineState
    ''' (which is the lifecycle state machine for CaptureEngine.vb).
    ''' </summary>
    Public Enum RecordingEngineState
        Created
        Initializing
        Idle
        Recording
        Stopping
        Faulted
        Disposed
    End Enum

    ''' <summary>
    ''' Configuration for a single recording session.
    ''' </summary>
    Public NotInheritable Class SessionConfig
        Public Property OutputPath As String = ""
        Public Property DurationSeconds As Integer = 30
        Public Property UseSharedHandle As Boolean = False

        ' FFmpeg path (from EngineConfigV2.Runtime.FFmpegPath or CLI override)
        Public Property FFmpegPath As String = ""
    End Class

    ''' <summary>
    ''' Result of a recording session.
    ''' </summary>
    Public NotInheritable Class SessionResult
        Public Property OutputPath As String = ""
        Public Property RequestedDurationSec As Integer
        Public Property ActualDurationSec As Double
        Public Property FramesCaptured As Long
        Public Property FramesEncoded As Long
        Public Property Drops As Long
        Public Property NvencErrors As Long
        Public Property TotalVideoBytes As Long
        Public Property AudioSamples As Long
        Public Property AudioBytes As Long
        Public Property VideoStreamFound As Boolean
        Public Property AudioStreamFound As Boolean
        Public Property FileExists As Boolean
        Public Property FileSize As Long
        Public Property ErrorMessage As String = ""

        Public ReadOnly Property Pass As Boolean
            Get
                Return FramesEncoded > 0 AndAlso
                       NvencErrors = 0 AndAlso
                       FileExists AndAlso
                       FileSize > 0 AndAlso
                       VideoStreamFound AndAlso
                       AudioStreamFound
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Engine status — safe to poll from any thread.
    ''' </summary>
    Public NotInheritable Class EngineStatus
        Public Property State As RecordingEngineState
        Public Property CurrentSessionId As String  ' Nothing if Idle
        Public Property FramesEncodedThisSession As Long
        Public Property LastSessionResult As SessionResult  ' Nothing if no session yet
    End Class

End Namespace
