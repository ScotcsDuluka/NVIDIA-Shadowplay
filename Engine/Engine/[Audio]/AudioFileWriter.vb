Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NAudio.CoreAudioApi
Imports NAudio.Wave

''' <summary>
''' File-based audio recorder — writes WASAPI capture to .wav files via
''' a lock-free queue + dedicated writer thread.
'''
''' ARCHITECTURE (Two-Process Recording):
'''   Video: FFmpeg ddagrab → NVENC → temp_video.mp4 (NO audio input)
'''   Audio: This class → temp_system.wav + temp_mic.wav (direct file I/O)
'''   Mux:   FFmpeg -i video.mp4 -i audio.wav -c:v copy -c:a aac final.mp4
'''
''' DESIGN PRINCIPLES (per GPT code review):
'''   1. NO wall-clock silence feeder thread — gap detection happens IN the
'''      callback, inserting silence BEFORE real audio (no overlap possible)
'''   2. NO synchronous disk I/O in callback — callback just memcpy + enqueue
'''   3. NO volume processing in callback — volume applied at mux stage
'''   4. NO fake latency measurement — per-track actual start timestamps
'''   5. Proper shutdown: stop capture → drain → flush writer → finalize
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
        Public Property Queue As BlockingCollection(Of Byte())
        Public Property WriterThread As Thread
        Public Property CallbackCount As Long
        Public Property BytesEnqueued As Long
        Public Property SamplesEnqueued As Long
        Public Property DroppedChunks As Long
        Public Property Failed As Boolean
        Public Property FailReason As String
        Public Property LastCallbackTicks As Long
        Public Property ActualStartTicks As Long
        Public Property Started As Boolean
        Public Property BytesPerSecond As Integer
        Public Property FrameSize As Integer
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

    ' ── Per-track actual start timestamps ──
    ' Set when the FIRST WASAPI callback fires for each track.
    ' This is the true capture start time (after device init), used for
    ' precise audio-video sync alignment at mux time.
    Private _systemStartTicks As Long = 0
    Private _micStartTicks As Long = 0

    Public ReadOnly Property SystemStartTicks As Long
        Get
            Return _systemStartTicks
        End Get
    End Property

    Public ReadOnly Property MicStartTicks As Long
        Get
            Return _micStartTicks
        End Get
    End Property

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
    ''' Returns True if at least one track started successfully.
    ''' </summary>
    Public Function Start(systemPath As String, micPath As String) As Boolean
        If _disposed Then Throw New ObjectDisposedException(NameOf(AudioFileWriter))
        If _isRunning Then Return True

        Dim anyStarted As Boolean = False

        If _config.SystemAudioCapture Then
            Dim sysTrack As New TrackState With {
                .Config = New TrackConfig With {
                    .Enabled = True,
                    .IsSystem = True,
                    .OutputPath = systemPath,
                    .Volume = _config.SystemAudioVolume
                }
            }
            If StartTrack(sysTrack) Then
                anyStarted = True
            Else
                RaiseEvent SystemStartFailed(sysTrack.FailReason)
            End If
            _tracks.Add(sysTrack)
        End If

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
                    .Volume = _config.MicVolume
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

            Dim dir As String = Path.GetDirectoryName(track.Config.OutputPath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            track.Writer = New WaveFileWriter(track.Config.OutputPath, track.Capture.WaveFormat)
            track.BytesPerSecond = track.Capture.WaveFormat.AverageBytesPerSecond
            track.FrameSize = (track.Capture.WaveFormat.BitsPerSample \ 8) * track.Capture.WaveFormat.Channels

            ' Bounded queue: 1000 items ≈ 10 seconds of audio.
            ' If writer can't keep up (slow disk), we drop oldest to prevent
            ' blocking the WASAPI capture callback.
            track.Queue = New BlockingCollection(Of Byte())(1000)

            ' Start writer thread (consumer)
            track.WriterThread = New Thread(Sub() WriterLoop(track)) With {
                .IsBackground = True,
                .Name = If(track.Config.IsSystem, "AudioWriter-Sys", "AudioWriter-Mic"),
                .Priority = ThreadPriority.Normal
            }
            track.WriterThread.Start()

            Dim handlerRef As TrackState = track
            AddHandler track.Capture.DataAvailable, Sub(sender As Object, e As WaveInEventArgs)
                                                         OnDataAvailable(handlerRef, e)
                                                     End Sub

            track.Capture.StartRecording()

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

    ''' <summary>
    ''' WASAPI callback — ultra-light: detect gap, copy buffer, enqueue, return.
    '''
    ''' NO disk I/O, NO volume processing, NO locks on the writer.
    ''' All heavy lifting happens on the writer thread.
    '''
    ''' Gap detection:
    '''   When WASAPI loopback doesn't fire (no audio playing), the .wav file
    '''   would be shorter than the video. To fix this, we detect gaps between
    '''   callbacks and insert silence BEFORE the current audio data.
    '''
    '''   The silence represents the time gap that ALREADY occurred (between
    '''   the last callback and this one). It's inserted before the current
    '''   audio data, so there's NO overlap with future callbacks.
    '''
    '''   This is fundamentally different from the old wall-clock feeder which
    '''   could insert silence that would be "overlapped" by a pending callback.
    ''' </summary>
    Private Sub OnDataAvailable(track As TrackState, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return

        Dim nowTicks As Long = Stopwatch.GetTimestamp()

        ' Capture actual start time on first callback
        If Not track.Started Then
            track.Started = True
            track.ActualStartTicks = nowTicks
            If track.Config.IsSystem Then
                _systemStartTicks = nowTicks
            Else
                _micStartTicks = nowTicks
            End If
        End If

        ' ── Gap detection (in-callback, no race) ──
        ' If time since last callback > expected, there was a gap (WASAPI
        ' loopback didn't fire because no audio was playing). Insert silence
        ' to fill the gap BEFORE the current audio data.
        '
        ' Threshold: gap must be > 50ms to trigger (filters out normal
        ' jitter of ±10ms). Capped at 5 seconds to prevent runaway silence.
        If track.LastCallbackTicks > 0 Then
            Dim elapsedSec As Double = (nowTicks - track.LastCallbackTicks) / Stopwatch.Frequency
            Dim bufferSec As Double = CDbl(e.BytesRecorded) / track.BytesPerSecond
            Dim gapSec As Double = elapsedSec - bufferSec

            If gapSec > 0.05 AndAlso gapSec < 5.0 Then
                Dim silenceBytes As Integer = CInt(gapSec * track.BytesPerSecond)
                If track.FrameSize > 0 Then silenceBytes = (silenceBytes \ track.FrameSize) * track.FrameSize
                If silenceBytes > 0 Then
                    EnqueueSilence(track, silenceBytes)
                End If
            End If
        End If

        ' ── Copy audio data + enqueue (NO disk I/O here) ──
        ' Allocate exact-size buffer (NAudio reuses its internal buffer, so we must copy)
        Dim copy As Byte() = New Byte(e.BytesRecorded - 1) {}
        Buffer.BlockCopy(e.Buffer, 0, copy, 0, e.BytesRecorded)

        ' Try to enqueue without blocking. If queue is full, drop oldest
        ' to make room (prevents blocking the capture callback → buffer overrun)
        If Not track.Queue.TryAdd(copy, 0) Then
            Dim dropped As Byte() = Nothing
            If track.Queue.TryTake(dropped) Then
                track.DroppedChunks += 1
                track.Queue.TryAdd(copy, 0)
            End If
        End If

        track.CallbackCount += 1
        track.BytesEnqueued += e.BytesRecorded
        If track.FrameSize > 0 Then
            track.SamplesEnqueued += e.BytesRecorded \ track.FrameSize
        End If
        track.LastCallbackTicks = nowTicks
    End Sub

    ''' <summary>
    ''' Enqueue silence in chunks (avoids huge allocations).
    ''' Called from the callback thread.
    ''' </summary>
    Private Sub EnqueueSilence(track As TrackState, byteCount As Integer)
        If byteCount <= 0 Then Return
        If track.FrameSize > 0 Then byteCount = (byteCount \ track.FrameSize) * track.FrameSize
        If byteCount <= 0 Then Return

        ' Enqueue in 16KB chunks
        Const chunkSize As Integer = 16384
        Dim remaining As Integer = byteCount
        While remaining > 0
            Dim size As Integer = Math.Min(remaining, chunkSize)
            Dim silence As Byte() = New Byte(size - 1) {}
            If Not track.Queue.TryAdd(silence, 0) Then
                Dim dropped As Byte() = Nothing
                If track.Queue.TryTake(dropped) Then
                    track.DroppedChunks += 1
                    track.Queue.TryAdd(silence, 0)
                End If
            End If
            track.BytesEnqueued += size
            remaining -= size
        End While
    End Sub

    ''' <summary>
    ''' Writer thread — consumes from queue, writes to disk.
    ''' This is the ONLY thread that touches WaveFileWriter, so no lock needed.
    ''' </summary>
    Private Sub WriterLoop(track As TrackState)
        Try
            While True
                Dim chunk As Byte() = Nothing
                If track.Queue.TryTake(chunk, 1000) Then
                    Try
                        If track.Writer IsNot Nothing Then
                            track.Writer.Write(chunk, 0, chunk.Length)
                        End If
                    Catch
                        Exit While
                    End Try
                ElseIf track.Queue.IsCompleted Then
                    Exit While
                End If
            End While
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[AudioFileWriter] Writer thread crashed: " & ex.Message)
        End Try
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
    ''' Stop all captures. Proper shutdown sequence:
    '''   1. Stop WASAPI capture (no new callbacks)
    '''   2. Brief drain period (pending callbacks finish)
    '''   3. Signal queue complete (writer thread can finish remaining items)
    '''   4. Wait for writer thread (flushes all data to disk)
    '''   5. Finalize WAV files (flush + dispose = writes correct header)
    '''
    ''' This ensures NO audio data is lost during shutdown, and the .wav
    ''' files are properly finalized for muxing.
    ''' </summary>
    Public Sub [Stop]()
        _isRunning = False

        ' ── Step 1: Stop captures (no new callbacks) ──
        For Each track As TrackState In _tracks
            Try
                If track.Capture IsNot Nothing Then
                    track.Capture.StopRecording()
                End If
            Catch
            End Try
        Next

        ' ── Step 2: Drain pending callbacks (100ms) ──
        ' WASAPI may have callbacks in-flight. Give them time to complete
        ' and enqueue their data before we signal the queue to complete.
        Thread.Sleep(100)

        ' ── Step 3: Signal queues to complete ──
        For Each track As TrackState In _tracks
            Try
                If track.Queue IsNot Nothing Then
                    track.Queue.CompleteAdding()
                End If
            Catch
            End Try
        Next

        ' ── Step 4: Wait for writer threads to finish ──
        ' This flushes ALL remaining data in the queue to disk.
        For Each track As TrackState In _tracks
            Try
                If track.WriterThread IsNot Nothing AndAlso track.WriterThread.IsAlive Then
                    track.WriterThread.Join(10000)
                End If
            Catch
            End Try

            ' ── Step 5: Finalize WAV file ──
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
            sb.AppendLine("[Audio] " & label & "BytesEnqueued=" & track.BytesEnqueued)
            sb.AppendLine("[Audio] " & label & "SamplesEnqueued=" & track.SamplesEnqueued)
            sb.AppendLine("[Audio] " & label & "DroppedChunks=" & track.DroppedChunks)
            sb.AppendLine("[Audio] " & label & "Started=" & track.Started.ToString())
            sb.AppendLine("[Audio] " & label & "StartTicks=" & track.ActualStartTicks)
            sb.AppendLine("[Audio] " & label & "Path=" & track.Config.OutputPath)
            If track.Failed Then
                sb.AppendLine("[Audio] " & label & "FAILED: " & track.FailReason)
            End If
        Next
        sb.AppendLine("[Audio] ShutdownRequested=" & If(Not _isRunning, "1", "0"))
        Return sb.ToString()
    End Function

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
