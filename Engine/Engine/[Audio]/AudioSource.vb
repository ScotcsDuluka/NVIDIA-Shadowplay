''' <summary>
''' Identifies the audio source within the capture pipeline.
''' Keeps Source vs Sink semantic separation so the engine can grow
''' beyond just System + Microphone in the future.
''' </summary>
Public Enum AudioSource
    SystemLoopback
    Microphone
End Enum
