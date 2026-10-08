Option Strict On
Option Explicit On
Option Infer On

' MediaInfo.vb — probe result DTOs.
'
' OWNER RULE (spec): file format is PROBED, never assumed. Everything the
' engine decides (codec support, frame size, fps, audio presence) comes from
' this structure — produced by ffprobe JSON output — not from extension names
' or product matrices.

Imports System.Collections.Generic

Namespace Gallery.Video

    ''' <summary>One decoded-stream description from ffprobe -show_streams.</summary>
    Public NotInheritable Class StreamInfo
        Public Property Index As Integer
        Public Property CodecType As String = ""       ' "video" | "audio" | other
        Public Property CodecName As String = ""       ' "h264", "aac", … (lowercase)
        Public Property Profile As String = ""
        Public Property Width As Integer
        Public Property Height As Integer
        Public Property PixFmt As String = ""          ' "yuv420p", …
        ''' <summary>Average frame rate as ffprobe rational, e.g. "60000/1001".</summary>
        Public Property AvgFrameRate As String = ""
        ''' <summary>Time base as ffprobe rational, e.g. "1/15360".</summary>
        Public Property TimeBase As String = ""
        ''' <summary>start_time in seconds (string form from ffprobe).</summary>
        Public Property StartTime As String = ""
        Public Property Duration As String = ""
        Public Property SampleRate As Integer
        Public Property Channels As Integer
        Public Property ChannelLayout As String = ""
    End Class

    ''' <summary>Container-level result from ffprobe -show_format.</summary>
    Public NotInheritable Class FormatInfo
        Public Property FormatName As String = ""      ' "mov,mp4,m4a,3gp,3g2,mj2"
        Public Property DurationSec As Double
        Public Property SizeBytes As Long
        Public Property BitRate As Long
    End Class

    ''' <summary>Full probe result for one media file.</summary>
    Public NotInheritable Class MediaInfo
        Public Property FilePath As String = ""
        Public Property Format As New FormatInfo()
        Public Property Streams As New List(Of StreamInfo)()

        ''' <summary>First video stream or Nothing (probe authority, not guess).</summary>
        Public ReadOnly Property Video As StreamInfo
            Get
                For Each s In Streams
                    If s.CodecType = "video" Then Return s
                Next
                Return Nothing
            End Get
        End Property

        ''' <summary>First audio stream or Nothing.</summary>
        Public ReadOnly Property Audio As StreamInfo
            Get
                For Each s In Streams
                    If s.CodecType = "audio" Then Return s
                Next
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property HasVideo As Boolean
            Get
                Return Video IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property HasAudio As Boolean
            Get
                Return Audio IsNot Nothing
            End Get
        End Property

        ''' <summary>Parse "60000/1001" → 59.94 fps. Returns 0 when unparsable
        ''' (caller then falls back to 30 and LOGS — never silently).</summary>
        Public Shared Function ParseFrameRate(rational As String) As Double
            If String.IsNullOrEmpty(rational) Then Return 0.0
            Dim parts = rational.Split("/"c)
            If parts.Length = 2 Then
                Dim num, den As Integer
                If Integer.TryParse(parts(0), num) AndAlso Integer.TryParse(parts(1), den) AndAlso den > 0 AndAlso num > 0 Then
                    Return num / den
                End If
            ElseIf parts.Length = 1 Then
                Dim singleVal As Double
                If Double.TryParse(parts(0), singleVal) Then Return singleVal
            End If
            Return 0.0
        End Function

        ''' <summary>Parse ffprobe "0.041708" style seconds string → ticks (100ns).
        ''' Returns 0 for "N/A"/empty — callers treat as zero-start.</summary>
        Public Shared Function ParseSecondsToTicks(s As String) As Long
            If String.IsNullOrEmpty(s) Then Return 0L
            Dim d As Double
            If Double.TryParse(s, Global.System.Globalization.CultureInfo.InvariantCulture, d) Then
                Return CLng(d * 10000000.0)
            End If
            Return 0L
        End Function
    End Class

End Namespace
