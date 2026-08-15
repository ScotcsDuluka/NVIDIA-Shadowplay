Imports NAudio.Wave

Public Class AudioFrame

    Public Enum AudioSource
        SystemLoopback
        Microphone
    End Enum

    Public Property Buffer As Byte()
    Public Property Length As Integer
    Public Property SampleRate As Integer
    Public Property Channels As Integer
    Public Property Source As AudioSource

    Public Sub New(buffer As Byte(), length As Integer, sampleRate As Integer, channels As Integer, source As AudioSource)
        Me.Buffer = buffer
        Me.Length = length
        Me.SampleRate = sampleRate
        Me.Channels = channels
        Me.Source = source
    End Sub

End Class

Public Class AudioFormatInfo
    Public Property SampleRate As Integer = 48000
    Public Property Channels As Integer = 2
    Public Property BitsPerSample As Integer = 32
    Public Property IsFloat As Boolean = True

    Public ReadOnly Property FFmpegFormatArg As String
        Get
            If IsFloat Then
                Return "f32le"
            ElseIf BitsPerSample = 16 Then
                Return "s16le"
            ElseIf BitsPerSample = 24 Then
                Return "s24le"
            ElseIf BitsPerSample = 32 Then
                Return "s32le"
            Else
                Return "s16le"
            End If
        End Get
    End Property
End Class
