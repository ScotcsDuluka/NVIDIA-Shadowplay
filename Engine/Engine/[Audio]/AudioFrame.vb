''' <summary>
''' Audio frame contract — one chunk of PCM bytes plus its source metadata
''' AND timing info. Producers (NAudioCapture callbacks) build AudioFrames,
''' consumers (pipe writers) read them.
'''
''' Timing fields enable:
'''   - A/V drift detection (compare against video PTS)
'''   - Drop detection (gap between StartSample of consecutive frames)
'''   - Queue latency measurement (Timestamp = when captured, vs. when written)
'''   - Separate-track synchronization (align Sys T1 to Mic T1)
''' </summary>
Public Class AudioFrame

    Public Property Buffer As Byte()
    Public Property Length As Integer
    Public Property Format As AudioFormat
    Public Property Source As AudioSource

    ''' <summary>
    ''' Monotonic session-relative timestamp — when this frame was captured,
    ''' measured against the SHARED capture epoch (not per-source stopwatch).
    ''' This means System frame T=12ms and Mic frame T=12ms refer to the same
    ''' wall-clock instant, enabling Separate-track alignment by direct comparison.
    ''' </summary>
    Public Property Timestamp As TimeSpan

    ''' <summary>
    ''' Sample position since capture start (per source, monotonic). This is
    ''' the audio equivalent of PTS — use for A/V sync.
    ''' </summary>
    Public Property StartSample As Long

    ''' <summary>
    ''' Number of samples PER CHANNEL in this frame (NOT total samples across
    ''' channels — e.g. a stereo frame of 480 bytes @ 32-bit float = 60 samples
    ''' per channel = 120 total). SampleCount × Channels × (BitsPerSample/8) = Length.
    ''' </summary>
    Public Property SamplesPerChannel As Integer

    Public Sub New(buffer As Byte(), length As Integer, format As AudioFormat, source As AudioSource,
                   timestamp As TimeSpan, startSample As Long, samplesPerChannel As Integer)
        Me.Buffer = buffer
        Me.Length = length
        Me.Format = format
        Me.Source = source
        Me.Timestamp = timestamp
        Me.StartSample = startSample
        Me.SamplesPerChannel = samplesPerChannel
    End Sub

End Class
