''' <summary>
''' Audio sample format contract — agreed between NAudio capture (producer)
''' and FFmpeg input args (consumer). One AudioFormat per source, so System
''' and Mic can each report their own real WASAPI format.
''' </summary>
Public Class AudioFormat

    Public Property SampleRate As Integer = 48000
    Public Property Channels As Integer = 2
    Public Property BitsPerSample As Integer = 32
    Public Property IsFloat As Boolean = True

    Public ReadOnly Property FFmpegFormatArg As String
        Get
            If IsFloat Then Return "f32le"
            Select Case BitsPerSample
                Case 16 : Return "s16le"
                Case 24 : Return "s24le"
                Case 32 : Return "s32le"
                Case Else : Return "s16le"
            End Select
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim enc As String = If(IsFloat, "float", "int")
        Return $"{SampleRate}Hz/{Channels}ch/{BitsPerSample}bit {enc}"
    End Function

End Class
