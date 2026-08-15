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
    ''' Wall-clock time when this frame was captured (Stopwatch-based, monotonic).
    ''' Use for queue latency + drop detection. NOT for A/V sync — that's
    ''' StartSample's job.
    ''' </summary>
    Public Property Timestamp As TimeSpan

    ''' <summary>
    ''' Sample position since capture start — the audio equivalent of PTS.
    ''' Monotonic per source: starts at 0, increments by SampleCount per frame.
    ''' Use this for A/V sync + Separate-track alignment.
    ''' </summary>
    Public Property StartSample As Long

    ''' <summary>
    ''' How many samples (NOT bytes) this frame contains. Length (bytes) =
    ''' SampleCount * Channels * (BitsPerSample / 8). Useful for buffer math
    ''' without re-deriving from Format every time.
    ''' </summary>
    Public Property SampleCount As Integer

    Public Sub New(buffer As Byte(), length As Integer, format As AudioFormat, source As AudioSource,
                   timestamp As TimeSpan, startSample As Long, sampleCount As Integer)
        Me.Buffer = buffer
        Me.Length = length
        Me.Format = format
        Me.Source = source
        Me.Timestamp = timestamp
        Me.StartSample = startSample
        Me.SampleCount = sampleCount
    End Sub

End Class
