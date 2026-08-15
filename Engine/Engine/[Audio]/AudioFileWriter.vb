Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NAudio.CoreAudioApi
Imports NAudio.Wave

''' <summary>
''' File-based audio recorder — writes WASAPI capture directly to .wav files.
'''
''' ARCHITECTURE (Two-Process Recording):
'''   Video: FFmpeg ddagrab → NVENC → temp_video.mp4 (NO audio input)
'''   Audio: This class → temp_system.wav + temp_mic.wav (direct file I/O)
'''   Mux:   FFmpeg -i video.mp4 -i audio.wav -c:v copy -c:a aac final.mp4
'''
''' This replaces the old NAudioCapture + named pipe approach which caused
''' video FPS regression because FFmpeg's two-input synchronization overhead
''' starved the ddagrab capture thread.
'''
''' Key differences from old NAudioCapture:
'''   - NO named pipe (eliminates pipe I/O blocking + silence feeder thread)
'''   - NO FFmpeg subprocess for audio (eliminates two-input contention)
'''   - Direct WaveFileWriter (NAudio built-in, zero allocations per callback)
'''   - Writes to .wav (raw PCM) — muxed into final MP4 at stop time
'''   - Complete isolation: audio failure never affects video recording
''' </summary>
Public Class AudioFileWriter
    Implements IDisposable

    Private Class TrackConfig
        Public Property Enabled As Boolean
        Public Property IsSystem As Boolean
        Public Property DeviceId As String
        Public Property DeviceName As String
        Public Property OutputPath As String
        Public Property Volume As Single
    End Class

    Private Class TrackState
        Public Property Config As TrackConfig
        Public Property Capture As WasapiCapture
        Public Property Writer As WaveFileWriter
        Public Property CopyBuffer As Byte()
        Public Property CallbackCount As Long
        Public Property BytesWritten As Long
        Public Property SamplesWritten As Long
        Public Property LastCallbackTicks As Long
        Public Property Failed As Boolean
        Public Property FailReason As String
    End Class

    Public Class AudioConfigValues
        Public Property SystemAudioCapture As Boolean = False
        Public Property MicCapture As Boolean = False
        Public Property SystemAudioVolume As Single = 1.0F
        Public Property MicVolume As Single = 1.0F
        Public Property MicDeviceId As String = ""
        Public Property MicDeviceName As String = ""
    End Class

    Private _config As AudioConfigValues
    Private _tracks As New List(Of TrackState)
    Private _isRunning As Boolean = False
    Private _disposed As Boolean = False

    Public Event SystemStartFailed(reason As String)
    Public Event MicStartFailed(reason As String)
    Public Event SystemFormatDetected(format As AudioFormat)
    Public Event MicFormatDetected(format As AudioFormat)

    Public Sub New(config As AudioConfigValues)
        _config = config
    End Sub

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _isRunning
        End Get
    End Property

    ''' <summary>
    ''' Start recording audio to .wav files.
    ''' systemPath / micPath are the temp .wav file paths.
    ''' Returns True if at least one track started successfully.
    ''' </summary>
    Public Function Start(systemPath As String, micPath As String) As Boolean
        If _disposed Then Throw New ObjectDisposedException(NameOf(AudioFileWriter))
        If _isRunning Then Return True

        Dim anyStarted As Boolean = False

        ' ── System audio track ──
        If _config.SystemAudioCapture Then
            Dim sysTrack As New TrackState With {
                .Config = New TrackConfig With {
                    .Enabled = True,
                    .IsSystem = True,
                    .DeviceId = "",
                    .DeviceName = "",
                    .OutputPath = systemPath,
                    .Volume = Math.Max(0.0F, Math.Min(2.0F, _config.SystemAudioVolume))
                }
            }
            If StartTrack(sysTrack) Then
                anyStarted = True
            Else
                RaiseEvent SystemStartFailed(sysTrack.FailReason)
            End If
            _tracks.Add(sysTrack)
        End If

        ' ── Mic track ──
        If _config.MicCapture AndAlso
           (Not String.IsNullOrEmpty(_config.MicDeviceId) OrElse
            Not String.IsNullOrEmpty(_config.MicDeviceName)) Then
            Dim micTrack As New TrackState With {
                .Config = New TrackConfig With {
                    .Enabled = True,
                    .IsSystem = False,
                    .DeviceId = _config.MicDeviceId,
                    .DeviceName = _config.MicDeviceName,
                    .OutputPath = micPath,
                    .Volume = Math.Max(0.0F, Math.Min(2.0F, _config.MicVolume))
                }
            }
            If StartTrack(micTrack) Then
                anyStarted = True
            Else
                RaiseEvent MicStartFailed(micTrack.FailReason)
            End If
            _tracks.Add(micTrack)
        End If

        _isRunning = anyStarted
        Return anyStarted
    End Function

    Private Function StartTrack(track As TrackState) As Boolean
        Try
            Dim device As MMDevice = Nothing
            If track.Config.IsSystem Then
                Using devEnum As New MMDeviceEnumerator()
                    device = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                End Using
                If device Is Nothing Then
                    track.Failed = True
                    track.FailReason = "No default audio render device found"
                    Return False
                End If
                track.Capture = New WasapiLoopbackCapture(device)
            Else
                device = FindMicDevice(track.Config.DeviceId, track.Config.DeviceName)
                If device Is Nothing Then
                    track.Failed = True
                    track.FailReason = "Mic device not found: " & track.Config.DeviceName
                    Return False
                End If
                track.Capture = New WasapiCapture(device)
            End If

            ' Create directory if needed
            Dim dir As String = Path.GetDirectoryName(track.Config.OutputPath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            ' WaveFileWriter handles WAV header + PCM data automatically.
            ' Volume is applied per-callback via simple float multiply (if != 1.0)
            track.Writer = New WaveFileWriter(track.Config.OutputPath, track.Capture.WaveFormat)

            ' Pre-allocate copy buffer (64KB — large enough for any WASAPI buffer)
            track.CopyBuffer = New Byte(65535) {}
            track.LastCallbackTicks = Stopwatch.GetTimestamp()

            Dim handlerRef As TrackState = track
            AddHandler track.Capture.DataAvailable, Sub(sender As Object, e As WaveInEventArgs)
                                                         OnDataAvailable(handlerRef, e)
                                                     End Sub

            track.Capture.StartRecording()

            ' Fire format-detected event
            Dim fmt As AudioFormat = WaveFormatToInfo(track.Capture.WaveFormat)
            If track.Config.IsSystem Then
                RaiseEvent SystemFormatDetected(fmt)
            Else
                RaiseEvent MicFormatDetected(fmt)
            End If

            Return True
        Catch ex As Exception
            track.Failed = True
            track.FailReason = ex.Message
            Return False
        End Try
    End Function

    Private Sub OnDataAvailable(track As TrackState, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        If track.Writer Is Nothing Then Return

        Try
            track.LastCallbackTicks = Stopwatch.GetTimestamp()
            Dim bytesToWrite As Integer = Math.Min(e.BytesRecorded, track.CopyBuffer.Length)

            ' Apply volume if != 1.0 (only for float format; for int16 we skip volume
            ' and let FFmpeg handle -af volume= at mux time — simpler + reliable)
            If track.Config.Volume = 1.0F Then
                ' Fast path: write directly from source buffer (zero copy)
                track.Writer.Write(e.Buffer, 0, bytesToWrite)
            Else
                ' Volume path: copy + scale (only if volume != 1.0)
                Buffer.BlockCopy(e.Buffer, 0, track.CopyBuffer, 0, bytesToWrite)
                ApplyVolumeInPlace(track.CopyBuffer, bytesToWrite, track.Capture.WaveFormat, track.Config.Volume)
                track.Writer.Write(track.CopyBuffer, 0, bytesToWrite)
            End If

            track.CallbackCount += 1
            track.BytesWritten += bytesToWrite
            Dim bytesPerSample As Integer = (track.Capture.WaveFormat.BitsPerSample \ 8) * track.Capture.WaveFormat.Channels
            If bytesPerSample > 0 Then
                track.SamplesWritten += bytesToWrite \ bytesPerSample
            End If
        Catch
            ' Swallow — we don't want audio errors to crash video recording
        End Try
    End Sub

    ''' <summary>
    ''' Apply volume multiplier in-place on the buffer.
    ''' Only supports IEEE float (WASAPI loopback default) and 16-bit PCM.
    ''' </summary>
    Private Sub ApplyVolumeInPlace(buffer As Byte(), length As Integer, wf As WaveFormat, volume As Single)
        If wf.Encoding = WaveFormatEncoding.IeeeFloat AndAlso wf.BitsPerSample = 32 Then
            ' f32le: 4 bytes per sample
            Dim sampleCount As Integer = length \ 4
            For i As Integer = 0 To sampleCount - 1
                Dim offset As Integer = i * 4
                Dim sample As Single = BitConverter.ToSingle(buffer, offset)
                sample *= volume
                BitConverter.GetBytes(sample).CopyTo(buffer, offset)
            Next
        ElseIf wf.Encoding = WaveFormatEncoding.Pcm AndAlso wf.BitsPerSample = 16 Then
            ' s16le: 2 bytes per sample
            Dim sampleCount As Integer = length \ 2
            For i As Integer = 0 To sampleCount - 1
                Dim offset As Integer = i * 2
                Dim sample As Short = BitConverter.ToInt16(buffer, offset)
                sample = CShort(Math.Max(Short.MinValue, Math.Min(Short.MaxValue, sample * volume)))
                BitConverter.GetBytes(sample).CopyTo(buffer, offset)
            Next
        End If
        ' Other formats: skip volume (FFmpeg -af will handle at mux time)
    End Sub

    Private Function FindMicDevice(deviceId As String, deviceName As String) As MMDevice
        Using devEnum As New MMDeviceEnumerator()
            Dim devices As MMDeviceCollection = devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)

            If Not String.IsNullOrEmpty(deviceId) Then
                For Each dev As MMDevice In devices
                    If dev.ID = deviceId Then Return dev
                Next
            End If

            If Not String.IsNullOrEmpty(deviceName) Then
                For Each dev As MMDevice In devices
                    If String.Equals(dev.FriendlyName, deviceName, StringComparison.Ordinal) Then Return dev
                Next
            End If

            Return Nothing
        End Using
    End Function

    Private Function WaveFormatToInfo(wf As WaveFormat) As AudioFormat
        Dim info As New AudioFormat()
        If wf Is Nothing Then Return info
        info.SampleRate = wf.SampleRate
        info.Channels = wf.Channels
        info.BitsPerSample = wf.BitsPerSample
        info.IsFloat = (wf.Encoding = WaveFormatEncoding.IeeeFloat)
        info.ChannelLayout = AudioFormat.LayoutFromChannelCount(wf.Channels)
        Return info
    End Function

    ''' <summary>
    ''' Stop all captures. Each WaveFileWriter is flushed + disposed which
    ''' finalizes the WAV header (writes correct data length + seeks back
    ''' to header). The resulting .wav files are ready for FFmpeg muxing.
    ''' </summary>
    Public Sub [Stop]()
        _isRunning = False

        For Each track As TrackState In _tracks
            Try
                If track.Capture IsNot Nothing Then
                    track.Capture.StopRecording()
                End If
            Catch
            End Try

            Try
                If track.Writer IsNot Nothing Then
                    track.Writer.Flush()
                    track.Writer.Dispose()
                    track.Writer = Nothing
                End If
            Catch
            End Try

            Try
                If track.Capture IsNot Nothing Then
                    track.Capture.Dispose()
                    track.Capture = Nothing
                End If
            Catch
            End Try
        Next
    End Sub

    Public Function GetDiagnostics() As String
        Dim sb As New Text.StringBuilder()
        For Each track As TrackState In _tracks
            Dim label As String = If(track.Config.IsSystem, "Sys", "Mic")
            sb.AppendLine("[Audio] " & label & "Callbacks=" & track.CallbackCount)
            sb.AppendLine("[Audio] " & label & "Bytes=" & track.BytesWritten)
            sb.AppendLine("[Audio] " & label & "Samples=" & track.SamplesWritten)
            sb.AppendLine("[Audio] " & label & "Path=" & track.Config.OutputPath)
            If track.Failed Then
                sb.AppendLine("[Audio] " & label & "FAILED: " & track.FailReason)
            End If
        Next
        sb.AppendLine("[Audio] ShutdownRequested=" & If(Not _isRunning, "1", "0"))
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Returns True if the given .wav file exists and has non-zero size.
    ''' Used by CaptureEngine to decide whether to include it in mux.
    ''' </summary>
    Public Shared Function HasAudioData(wavPath As String) As Boolean
        Try
            Return File.Exists(wavPath) AndAlso New FileInfo(wavPath).Length > 44
        Catch
            Return False
        End Try
    End Function

    Public Shared Function ListMicDevices() As List(Of Tuple(Of String, String))
        Dim result As New List(Of Tuple(Of String, String))()
        Try
            Using devEnum As New MMDeviceEnumerator()
                For Each dev As MMDevice In devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                    result.Add(New Tuple(Of String, String)(dev.ID, dev.FriendlyName))
                Next
            End Using
        Catch
        End Try
        Return result
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        [Stop]()
    End Sub

End Class
