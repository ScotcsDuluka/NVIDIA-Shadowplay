Option Strict On
Option Explicit On
Option Infer On

' GalleryVideoFaults.vb — failure taxonomy (design doc §7).
'
' OWNER RULE: every failure case must map to a fault reason + a non-crashing,
' non-deadlocking outcome. Nothing in the playback engine is allowed to
' surface an unhandled exception to the Gallery UI.

Namespace Gallery.Video

    Public Enum GalleryVideoFaultKind
        ''' <summary>Path does not exist (checked before spawn, and on probe fail).</summary>
        FileMissing
        ''' <summary>File exists but cannot be opened for read (sharing violation etc.).</summary>
        FileLocked
        ''' <summary>ffprobe invalid or ffmpeg exits immediately with 0 frames.</summary>
        CorruptFile
        ''' <summary>Probed codec/pixfmt outside the supported set (reason carries values).</summary>
        UnsupportedFormat
        ''' <summary>No usable ffmpeg/ffprobe binary (FFmpegLocator rejected all candidates).</summary>
        BackendMissing
        ''' <summary>D3D11 device creation failed, or device-lost twice (recreate-once policy).</summary>
        RendererUnavailable
        ''' <summary>Probe found zero video streams (audio-only file in the video viewer).</summary>
        NoVideoStream
        ''' <summary>Seek target spawn failed or no frame arrived within SeekTimeoutMs.</summary>
        SeekFailed
        ''' <summary>Unexpected internal error — carries exception message; still no crash.</summary>
        InternalError
    End Enum

    Public NotInheritable Class GalleryVideoFault
        Public Property Kind As GalleryVideoFaultKind
        Public Property Detail As String = ""
        Public Property ExceptionType As String = ""

        Public Sub New(kind As GalleryVideoFaultKind, Optional detail As String = "", Optional exType As String = "")
            Me.Kind = kind
            Me.Detail = If(detail, "")
            Me.ExceptionType = If(exType, "")
        End Sub

        Public Overrides Function ToString() As String
            Dim base = $"[{Kind}]"
            If Not String.IsNullOrEmpty(ExceptionType) Then base &= $" {ExceptionType}:"
            If Not String.IsNullOrEmpty(Detail) Then base &= $" {Detail}"
            Return base
        End Function

        ''' <summary>Map an arbitrary exception to the taxonomy. Never throws.</summary>
        Public Shared Function FromException(path As String, ex As Exception) As GalleryVideoFault
            Dim io = TryCast(ex, System.IO.IOException)
            If io IsNot Nothing Then
                ' COMException sharing violations surface as IOException on read
                Return New GalleryVideoFault(GalleryVideoFaultKind.FileLocked, $"{path}: {io.Message}", ex.GetType().Name)
            End If
            Dim fnf = TryCast(ex, System.IO.FileNotFoundException)
            If fnf IsNot Nothing Then
                Return New GalleryVideoFault(GalleryVideoFaultKind.FileMissing, fnf.FileName, ex.GetType().Name)
            End If
            Return New GalleryVideoFault(GalleryVideoFaultKind.InternalError, $"{path}: {ex.Message}", ex.GetType().Name)
        End Function
    End Class

End Namespace
