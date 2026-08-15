''' <summary>
''' Audio frame contract — one chunk of PCM bytes plus its source metadata.
''' Producers (NAudioCapture callbacks) build AudioFrames, consumers (pipe writers)
''' read them. Keeps Buffer/Length separate from format so a frame can outlive
''' format negotiation (format is fixed per source; only bytes flow).
''' </summary>
Public Class AudioFrame

    Public Property Buffer As Byte()
    Public Property Length As Integer
    Public Property Format As AudioFormat
    Public Property Source As AudioSource

    Public Sub New(buffer As Byte(), length As Integer, format As AudioFormat, source As AudioSource)
        Me.Buffer = buffer
        Me.Length = length
        Me.Format = format
        Me.Source = source
    End Sub

End Class
