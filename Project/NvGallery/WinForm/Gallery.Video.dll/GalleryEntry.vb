Option Strict On
Option Explicit On
Option Infer On

' GalleryEntry.vb — one immutable Gallery item (data layer model, W3).
'
' OWNER RULE (inherited from MediaInfo): media facts are PROBED, never
' assumed. Duration/fps/codec/dims/audio come from MediaProbe (ffprobe) —
' extension names and file naming conventions are never consulted. Filesystem
' facts (size, mtime) come from the scan itself.
'
' Instances are immutable after construction — the library publishes fresh
' snapshots; the UI can hold a reference without seeing it change.

Imports System

Namespace Gallery.Video

    ''' <summary>Probe outcome for one gallery file. Missing taxonomy on
    ''' purpose: a file that vanished is removed from the index by the next
    ''' Rescan, not represented as an entry.</summary>
    Public Enum GalleryEntryStatus
        ''' <summary>ffprobe parsed the file; media facts are populated.</summary>
        Ready
        ''' <summary>ffprobe rejected the file (corrupt/unsupported container
        ''' or ffprobe unusable at scan time) — metadata stays zeroed.</summary>
        CorruptFile
        ''' <summary>ffprobe parsed the container but found no usable video
        ''' stream (audio-only mp4) or a video stream without dimensions.</summary>
        UnsupportedFormat
        ''' <summary>The probe itself failed (spawn/IO error, locked file,
        ''' timeout) — the file may still be fine; retried next Rescan.</summary>
        ProbeFailed
    End Enum

    ''' <summary>Sort keys for Query. Every key sorts with a full-path
    ''' tie-break so the order is deterministic even for equal keys.</summary>
    Public Enum GallerySortKey
        Name
        LastWriteTimeUtc
        DurationSec
        SizeBytes
    End Enum

    Public NotInheritable Class GalleryEntry

        Public Sub New(fullPath As String,
                       lengthBytes As Long,
                       lastWriteTimeUtc As DateTime,
                       status As GalleryEntryStatus,
                       durationSec As Double,
                       fps As Double,
                       videoCodec As String,
                       width As Integer,
                       height As Integer,
                       hasAudio As Boolean,
                       audioCodec As String,
                       detail As String)
            Me.FullPath = If(fullPath, "")
            Me.LengthBytes = lengthBytes
            Me.LastWriteTimeUtc = lastWriteTimeUtc
            Me.Status = status
            Me.DurationSec = durationSec
            Me.Fps = fps
            Me.VideoCodec = If(videoCodec, "")
            Me.Width = width
            Me.Height = height
            Me.HasAudio = hasAudio
            Me.AudioCodec = If(audioCodec, "")
            Me.Detail = If(detail, "")
        End Sub

        ''' <summary>Absolute path — the entry identity and the UI list key.</summary>
        Public ReadOnly Property FullPath As String
        Public ReadOnly Property LengthBytes As Long
        Public ReadOnly Property LastWriteTimeUtc As DateTime
        Public ReadOnly Property Status As GalleryEntryStatus
        ''' <summary>Container duration in seconds (0 when not Ready).</summary>
        Public ReadOnly Property DurationSec As Double
        ''' <summary>Probed average frame rate (0 when unparsable — never a fallback guess).</summary>
        Public ReadOnly Property Fps As Double
        Public ReadOnly Property VideoCodec As String
        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer
        Public ReadOnly Property HasAudio As Boolean
        Public ReadOnly Property AudioCodec As String
        ''' <summary>Why the status is not Ready (empty for Ready).</summary>
        Public ReadOnly Property Detail As String

        Public ReadOnly Property Name As String
            Get
                Return IO.Path.GetFileName(FullPath)
            End Get
        End Property

        Public ReadOnly Property DirectoryName As String
            Get
                Return IO.Path.GetDirectoryName(FullPath)
            End Get
        End Property

        Public ReadOnly Property IsReady As Boolean
            Get
                Return Status = GalleryEntryStatus.Ready
            End Get
        End Property

        ''' <summary>Equals-by-identity + observable facts: two scans of the
        ''' same unchanged file produce equal entries (diff engine relies on
        ''' this). Media facts compare at display precision (double compare
        ''' with epsilon) so probe float noise never churns the index.</summary>
        Public Overloads Function Equals(other As GalleryEntry) As Boolean
            If other Is Nothing Then Return False
            Return String.Equals(FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase) AndAlso
                   LengthBytes = other.LengthBytes AndAlso
                   LastWriteTimeUtc = other.LastWriteTimeUtc AndAlso
                   Status = other.Status AndAlso
                   Math.Abs(DurationSec - other.DurationSec) < 0.001 AndAlso
                   Math.Abs(Fps - other.Fps) < 0.001 AndAlso
                   String.Equals(VideoCodec, other.VideoCodec, StringComparison.Ordinal) AndAlso
                   Width = other.Width AndAlso Height = other.Height AndAlso
                   HasAudio = other.HasAudio AndAlso
                   String.Equals(AudioCodec, other.AudioCodec, StringComparison.Ordinal)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Name} [{Status}] {DurationSec:0.##}s {Width}x{Height}{If(HasAudio, "+audio", "")}"
        End Function

    End Class

End Namespace
