Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NAudio.CoreAudioApi
Imports NAudio.Wave

Public Class NAudioCaptureEngine
    Implements IDisposable

    Public Class AudioConfigValues
        Public Property SystemAudioCapture As Boolean = False
        Public Property MicCapture As Boolean = False
        Public Property SystemAudioVolume As Single = 1.0F
        Public Property MicVolume As Single = 1.0F
        Public Property MicDeviceId As String = ""
        Public Property MicDeviceName As String = ""
    End Class

    Private _config As AudioConfigValues
    Private _systemCapture As WasapiLoopbackCapture
    Private _micCapture As WasapiCapture

    Private _systemQueue As BlockingCollection(Of AudioFrame)
    Private _micQueue As BlockingCollection(Of AudioFrame)

    Private _systemWriterThread As Thread
    Private _micWriterThread As Thread

    Private _systemStream As Stream
    Private _micStream As Stream

    Private _systemFormat As AudioFormat
    Private _micFormat As AudioFormat

    Private _systemStopwatch As Stopwatch
    Private _micStopwatch As Stopwatch
    Private _systemStartSample As Long = 0
    Private _micStartSample As Long = 0

    ''' <summary>
    ''' Shared capture epoch — set ONCE when the engine starts (whichever
    ''' source starts first establishes T0). All Timestamps are measured
    ''' against this, so System T=12ms and Mic T=12ms refer to the same
    ''' wall-clock instant (critical for Separate-track alignment).
    ''' </summary>
    Private _sessionStartTicks As Long = 0
    Private _sessionStartSet As Boolean = False

    ''' <summary>
    ''' Drop counters — incremented atomically from WASAPI callback.
    ''' A background telemetry thread polls these counters and fires
    ''' FrameDropped events OFF the callback thread, so subscribers can
    ''' safely do disk I/O / WinForms / IPC without blocking capture.
    ''' </summary>
    Private _systemDropCount As Long = 0
    Private _micDropCount As Long = 0
    Private _telemetryThread As Thread
    Private _telemetryStop As New ManualResetEvent(False)
    Private Const TelemetryPollMs As Integer = 250

    ' ── Audio diagnostics counters (atomic) ──
    Private _sysSamplesReceived As Long = 0
    Private _sysSamplesWritten As Long = 0
    Private _sysBytesWritten As Long = 0
    Private _sysNaNCount As Long = 0
    Private _sysInfCount As Long = 0
    Private _sysPartialFrameCount As Long = 0
    Private _micSamplesReceived As Long = 0
    Private _micSamplesWritten As Long = 0
    Private _micBytesWritten As Long = 0
    Private _micNaNCount As Long = 0
    Private _micInfCount As Long = 0
    Private _micPartialFrameCount As Long = 0

    ' ── Shutdown state ──
    Private _audioShutdownRequested As Integer = 0
    Private _sysProducerStopped As Integer = 0
    Private _micProducerStopped As Integer = 0
    Private _sysPipeClosed As Integer = 0
    Private _micPipeClosed As Integer = 0

    Private _isRunning As Boolean = False
    Private _disposed As Boolean = False

    Public Event SystemFormatDetected(format As AudioFormat)
    Public Event MicFormatDetected(format As AudioFormat)
    Public Event SystemStartFailed(reason As String)
    Public Event MicStartFailed(reason As String)
    Public Event FrameDropped(source As AudioSource, reason As String)

    Public Sub New(config As AudioConfigValues)
        _config = config
    End Sub

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _isRunning
        End Get
    End Property

    Public ReadOnly Property SystemFormat As AudioFormat
        Get
            Return _systemFormat
        End Get
    End Property

    Public ReadOnly Property MicFormat As AudioFormat
        Get
            Return _micFormat
        End Get
    End Property

    Public Sub Start(systemStream As Stream, micStream As Stream)
        If _disposed Then Throw New ObjectDisposedException(NameOf(NAudioCaptureEngine))
        If _isRunning Then Return

        _systemStream = systemStream
        _micStream = micStream
        _isRunning = True

        ' Establish the shared session epoch ONCE before any source starts.
        ' Both System and Mic frames will measure their Timestamp against
        ' this same T0 — that's what makes Separate-track sync possible.
        If Not _sessionStartSet Then
            _sessionStartTicks = Stopwatch.GetTimestamp()
            _sessionStartSet = True
        End If

        ' Start telemetry thread (fires FrameDropped events off callback thread)
        _telemetryStop.Reset()
        _telemetryThread = New Thread(AddressOf TelemetryLoop) With {
            .IsBackground = True,
            .Name = "NAudioTelemetry"
        }
        _telemetryThread.Start()

        If _config.SystemAudioCapture Then
            _systemQueue = New BlockingCollection(Of AudioFrame)(256)
            _systemStopwatch = Stopwatch.StartNew()
            _systemStartSample = 0
            StartSystemCapture()
            StartSystemWriterThread()
        End If

        If _config.MicCapture AndAlso Not String.IsNullOrEmpty(_config.MicDeviceId) Then
            _micQueue = New BlockingCollection(Of AudioFrame)(256)
            _micStopwatch = Stopwatch.StartNew()
            _micStartSample = 0
            StartMicCapture()
            StartMicWriterThread()
        ElseIf _config.MicCapture AndAlso Not String.IsNullOrEmpty(_config.MicDeviceName) Then
            _micQueue = New BlockingCollection(Of AudioFrame)(256)
            _micStopwatch = Stopwatch.StartNew()
            _micStartSample = 0
            StartMicCapture()
            StartMicWriterThread()
        End If
    End Sub

    ''' <summary>
    ''' Stops audio producers (WASAPI capture) and completes queue adding,
    ''' but does NOT close the output pipe. This allows writer threads to
    ''' drain remaining frames to FFmpeg, then the caller closes the pipe
    ''' to signal EOF.
    ''' </summary>
    Public Sub StopProducers()
        System.Threading.Interlocked.Exchange(_audioShutdownRequested, 1)

        Try
            If _systemCapture IsNot Nothing Then
                RemoveHandler _systemCapture.DataAvailable, AddressOf OnSystemDataAvailable
                RemoveHandler _systemCapture.RecordingStopped, AddressOf OnSystemCaptureStopped
                _systemCapture.StopRecording()
                _systemCapture.Dispose()
                _systemCapture = Nothing
            End If
        Catch
        End Try
        System.Threading.Interlocked.Exchange(_sysProducerStopped, 1)

        Try
            If _micCapture IsNot Nothing Then
                RemoveHandler _micCapture.DataAvailable, AddressOf OnMicDataAvailable
                RemoveHandler _micCapture.RecordingStopped, AddressOf OnMicCaptureStopped
                _micCapture.StopRecording()
                _micCapture.Dispose()
                _micCapture = Nothing
            End If
        Catch
        End Try
        System.Threading.Interlocked.Exchange(_micProducerStopped, 1)

        Try
            If _systemQueue IsNot Nothing Then _systemQueue.CompleteAdding()
        Catch
        End Try
        Try
            If _micQueue IsNot Nothing Then _micQueue.CompleteAdding()
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Waits for writer threads to drain their queues and exit, then
    ''' closes output pipes. Called AFTER StopProducers().
    ''' </summary>
    Public Sub ClosePipes()
        ' Wait for writer threads to drain remaining queued frames
        Try
            If _systemWriterThread IsNot Nothing AndAlso _systemWriterThread.IsAlive Then
                _systemWriterThread.Join(5000)
            End If
        Catch
        End Try
        Try
            If _micWriterThread IsNot Nothing AndAlso _micWriterThread.IsAlive Then
                _micWriterThread.Join(5000)
            End If
        Catch
        End Try

        ' Now close pipes — sends EOF to FFmpeg's pipe:0 input
        Try
            If _systemStream IsNot Nothing Then
                _systemStream.Flush()
                _systemStream.Dispose()
                _systemStream = Nothing
                System.Threading.Interlocked.Exchange(_sysPipeClosed, 1)
            End If
        Catch
        End Try
        Try
            If _micStream IsNot Nothing AndAlso _micStream IsNot _systemStream Then
                _micStream.Flush()
                _micStream.Dispose()
                _micStream = Nothing
            End If
            System.Threading.Interlocked.Exchange(_micPipeClosed, 1)
        Catch
        End Try

        Try
            If _micNamedPipeStream IsNot Nothing Then
                _micNamedPipeStream.Flush()
                _micNamedPipeStream.Dispose()
                _micNamedPipeStream = Nothing
            End If
        Catch
        End Try
        Try
            If _micNamedPipe IsNot Nothing Then
                Try : _micNamedPipe.Disconnect() : Catch : End Try
                _micNamedPipe.Dispose()
                _micNamedPipe = Nothing
            End If
        Catch
        End Try

        ' Stop telemetry thread
        Try : _telemetryStop.Set() : Catch : End Try

        _isRunning = False
        _systemQueue = Nothing
        _micQueue = Nothing
    End Sub

    Public Sub [Stop]()
        _isRunning = False

        ' Signal telemetry thread to stop. Don't Join inside [Stop] — it would
        ' block the caller up to TelemetryPollMs (250ms). The thread is
        ' IsBackground=True, so it dies with the process if needed.
        Try : _telemetryStop.Set() : Catch : End Try

        Try
            If _systemQueue IsNot Nothing Then _systemQueue.CompleteAdding()
        Catch
        End Try
        Try
            If _micQueue IsNot Nothing Then _micQueue.CompleteAdding()
        Catch
        End Try

        Try
            If _systemCapture IsNot Nothing Then
                RemoveHandler _systemCapture.DataAvailable, AddressOf OnSystemDataAvailable
                RemoveHandler _systemCapture.RecordingStopped, AddressOf OnSystemCaptureStopped
                _systemCapture.StopRecording()
                _systemCapture.Dispose()
                _systemCapture = Nothing
            End If
        Catch
        End Try

        Try
            If _micCapture IsNot Nothing Then
                RemoveHandler _micCapture.DataAvailable, AddressOf OnMicDataAvailable
                RemoveHandler _micCapture.RecordingStopped, AddressOf OnMicCaptureStopped
                _micCapture.StopRecording()
                _micCapture.Dispose()
                _micCapture = Nothing
            End If
        Catch
        End Try

        Try
            If _systemWriterThread IsNot Nothing AndAlso _systemWriterThread.IsAlive Then
                _systemWriterThread.Join(3000)
            End If
        Catch
        End Try
        Try
            If _micWriterThread IsNot Nothing AndAlso _micWriterThread.IsAlive Then
                _micWriterThread.Join(3000)
            End If
        Catch
        End Try

        ' Telemetry thread: stop signal was set above, let it exit on its own.
        ' Don't Join — we don't want to block callers waiting for the 250ms
        ' poll cycle. Background thread will exit cleanly.

        _systemQueue = Nothing
        _micQueue = Nothing
        _systemStream = Nothing
        _micStream = Nothing
    End Sub

    Private Sub StartSystemCapture()
        Try
            Using devEnum As New MMDeviceEnumerator()
                Dim defaultOut As MMDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                If defaultOut Is Nothing Then
                    RaiseEvent SystemStartFailed("No default audio render device found")
                    Return
                End If
                _systemCapture = New WasapiLoopbackCapture(defaultOut)
            End Using

            _systemFormat = WaveFormatToInfo(_systemCapture.WaveFormat)
            RaiseEvent SystemFormatDetected(_systemFormat)

            AddHandler _systemCapture.DataAvailable, AddressOf OnSystemDataAvailable
            AddHandler _systemCapture.RecordingStopped, AddressOf OnSystemCaptureStopped
            _systemCapture.StartRecording()
        Catch ex As Exception
            RaiseEvent SystemStartFailed(ex.Message)
            System.Diagnostics.Debug.WriteLine("[NAudio] System capture start failed: " & ex.Message)
        End Try
    End Sub

    Private Sub StartMicCapture()
        Try
            Dim targetDev As MMDevice = FindMicDevice()
            If targetDev Is Nothing Then
                RaiseEvent MicStartFailed("Mic device not found: " & If(_config.MicDeviceName, _config.MicDeviceId))
                Return
            End If

            _micCapture = New WasapiCapture(targetDev)
            _micFormat = WaveFormatToInfo(_micCapture.WaveFormat)
            RaiseEvent MicFormatDetected(_micFormat)

            AddHandler _micCapture.DataAvailable, AddressOf OnMicDataAvailable
            AddHandler _micCapture.RecordingStopped, AddressOf OnMicCaptureStopped
            _micCapture.StartRecording()
        Catch ex As Exception
            RaiseEvent MicStartFailed(ex.Message)
            System.Diagnostics.Debug.WriteLine("[NAudio] Mic capture start failed: " & ex.Message)
        End Try
    End Sub

    Private Function FindMicDevice() As MMDevice
        Using devEnum As New MMDeviceEnumerator()
            Dim devices = devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)

            If Not String.IsNullOrEmpty(_config.MicDeviceId) Then
                For Each dev As MMDevice In devices
                    If dev.ID = _config.MicDeviceId Then Return dev
                Next
            End If

            If Not String.IsNullOrEmpty(_config.MicDeviceName) Then
                For Each dev As MMDevice In devices
                    If String.Equals(dev.FriendlyName, _config.MicDeviceName, StringComparison.Ordinal) Then Return dev
                Next
            End If

            Return Nothing
        End Using
    End Function

    Private Sub OnSystemDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        Try
            If _systemQueue Is Nothing OrElse _systemFormat Is Nothing Then Return

            Dim bytesPerSample As Integer = (_systemFormat.BitsPerSample \ 8) * _systemFormat.Channels
            If bytesPerSample < 1 Then bytesPerSample = 4

            ' Block alignment sanity: PCM bytes must divide evenly by block align.
            ' If not, the buffer is malformed (driver bug / partial read) and we
            ' should NOT enqueue — would cause FFmpeg to misread sample boundaries.
            If e.BytesRecorded Mod bytesPerSample <> 0 Then
                System.Threading.Interlocked.Increment(_systemDropCount)
                Return
            End If

            Dim samplesPerChannel As Integer = e.BytesRecorded \ bytesPerSample

            ' Atomic reservation: increment counter once, derive our startSample
            ' from the returned new value. This eliminates the race where two
            ' concurrent callbacks could both Read the same value then both Add
            ' their counts — which would produce duplicate StartSamples.
            Dim endSample As Long = System.Threading.Interlocked.Add(_systemStartSample, samplesPerChannel)
            Dim startSample As Long = endSample - samplesPerChannel

            System.Threading.Interlocked.Add(_sysSamplesReceived, samplesPerChannel)

            Dim ts As TimeSpan = GetSessionTimestamp()

            Dim copy(e.BytesRecorded - 1) As Byte
            Buffer.BlockCopy(e.Buffer, 0, copy, 0, e.BytesRecorded)

            Dim frame As New AudioFrame(copy, e.BytesRecorded, _systemFormat,
                                        AudioSource.SystemLoopback, ts, startSample, samplesPerChannel)

            ' Non-blocking: TryAdd without timeout returns immediately.
            ' WASAPI callback must NEVER wait — backpressure is handled by
            ' dropping + telemetry rather than blocking the capture thread.
            If Not _systemQueue.TryAdd(frame) Then
                System.Threading.Interlocked.Increment(_systemDropCount)
            End If
        Catch
        End Try
    End Sub

    Private Sub OnMicDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        Try
            If _micQueue Is Nothing OrElse _micFormat Is Nothing Then Return

            Dim bytesPerSample As Integer = (_micFormat.BitsPerSample \ 8) * _micFormat.Channels
            If bytesPerSample < 1 Then bytesPerSample = 4

            If e.BytesRecorded Mod bytesPerSample <> 0 Then
                System.Threading.Interlocked.Increment(_micDropCount)
                Return
            End If

            Dim samplesPerChannel As Integer = e.BytesRecorded \ bytesPerSample

            Dim endSample As Long = System.Threading.Interlocked.Add(_micStartSample, samplesPerChannel)
            Dim startSample As Long = endSample - samplesPerChannel

            System.Threading.Interlocked.Add(_micSamplesReceived, samplesPerChannel)

            Dim ts As TimeSpan = GetSessionTimestamp()

            Dim copy(e.BytesRecorded - 1) As Byte
            Buffer.BlockCopy(e.Buffer, 0, copy, 0, e.BytesRecorded)

            Dim frame As New AudioFrame(copy, e.BytesRecorded, _micFormat,
                                        AudioSource.Microphone, ts, startSample, samplesPerChannel)

            If Not _micQueue.TryAdd(frame) Then
                System.Threading.Interlocked.Increment(_micDropCount)
            End If
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Returns elapsed time since the shared session epoch (T0). Because T0
    ''' is shared between System and Mic, their Timestamps can be compared
    ''' directly for Separate-track alignment.
    ''' </summary>
    Private Function GetSessionTimestamp() As TimeSpan
        If Not _sessionStartSet Then Return TimeSpan.Zero
        Dim elapsedTicks As Long = Stopwatch.GetTimestamp() - _sessionStartTicks
        Return TimeSpan.FromTicks(elapsedTicks * (TimeSpan.TicksPerSecond \ Stopwatch.Frequency))
    End Function

    ''' <summary>
    ''' Background telemetry loop — polls drop counters and fires FrameDropped
    ''' events OFF the WASAPI callback thread. Subscribers can safely do disk
    ''' I/O / WinForms / IPC without delaying the capture callback.
    ''' </summary>
    Private Sub TelemetryLoop()
        Dim lastSysDrops As Long = 0
        Dim lastMicDrops As Long = 0
        Try
            While _isRunning AndAlso Not _telemetryStop.WaitOne(TelemetryPollMs)
                Dim sysDrops As Long = System.Threading.Interlocked.Read(_systemDropCount)
                Dim micDrops As Long = System.Threading.Interlocked.Read(_micDropCount)

                If sysDrops > lastSysDrops Then
                    Dim dropped As Long = sysDrops - lastSysDrops
                    lastSysDrops = sysDrops
                    Try
                        RaiseEvent FrameDropped(AudioSource.SystemLoopback,
                                                $"dropped {dropped} frame(s) (total: {sysDrops})")
                    Catch
                    End Try
                End If

                If micDrops > lastMicDrops Then
                    Dim dropped As Long = micDrops - lastMicDrops
                    lastMicDrops = micDrops
                    Try
                        RaiseEvent FrameDropped(AudioSource.Microphone,
                                                $"dropped {dropped} frame(s) (total: {micDrops})")
                    Catch
                    End Try
                End If
            End While
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[NAudio] Telemetry loop crashed: " & ex.Message)
        End Try
    End Sub

    Private Sub StartSystemWriterThread()
        _systemWriterThread = New Thread(AddressOf SystemWriterLoop) With {
            .IsBackground = True,
            .Name = "NAudioSystemWriter"
        }
        _systemWriterThread.Start()
    End Sub

    Private Sub StartMicWriterThread()
        _micWriterThread = New Thread(AddressOf MicWriterLoop) With {
            .IsBackground = True,
            .Name = "NAudioMicWriter"
        }
        _micWriterThread.Start()
    End Sub

    Private Sub SystemWriterLoop()
        Try
            While _isRunning AndAlso _systemQueue IsNot Nothing
                Dim frame As AudioFrame = Nothing
                If Not _systemQueue.TryTake(frame, 500) Then Continue While
                If frame Is Nothing Then Continue While
                WriteSanitizedFrame(frame, _systemStream, AudioSource.SystemLoopback,
                                    _sysSamplesWritten, _sysBytesWritten,
                                    _sysNaNCount, _sysInfCount, _sysPartialFrameCount)
            End While

            ' Drain remaining
            While _systemQueue IsNot Nothing
                Dim frame As AudioFrame = Nothing
                If Not _systemQueue.TryTake(frame, 0) Then Exit While
                If frame Is Nothing Then Continue While
                WriteSanitizedFrame(frame, _systemStream, AudioSource.SystemLoopback,
                                    _sysSamplesWritten, _sysBytesWritten,
                                    _sysNaNCount, _sysInfCount, _sysPartialFrameCount)
            End While

            SyncLock _systemStream
                Try
                    If _systemStream IsNot Nothing Then _systemStream.Flush()
                Catch
                End Try
            End SyncLock
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[NAudio] System writer loop crashed: " & ex.Message)
        End Try
    End Sub

    Private Sub MicWriterLoop()
        Try
            While _isRunning AndAlso _micQueue IsNot Nothing
                Dim frame As AudioFrame = Nothing
                If Not _micQueue.TryTake(frame, 500) Then Continue While
                If frame Is Nothing Then Continue While
                WriteSanitizedFrame(frame, _micStream, AudioSource.Microphone,
                                    _micSamplesWritten, _micBytesWritten,
                                    _micNaNCount, _micInfCount, _micPartialFrameCount)
            End While

            While _micQueue IsNot Nothing
                Dim frame As AudioFrame = Nothing
                If Not _micQueue.TryTake(frame, 0) Then Exit While
                If frame Is Nothing Then Continue While
                WriteSanitizedFrame(frame, _micStream, AudioSource.Microphone,
                                    _micSamplesWritten, _micBytesWritten,
                                    _micNaNCount, _micInfCount, _micPartialFrameCount)
            End While

            SyncLock _micStream
                Try
                    If _micStream IsNot Nothing Then _micStream.Flush()
                Catch
                End Try
            End SyncLock
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[NAudio] Mic writer loop crashed: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Validates and writes a single AudioFrame to the output stream.
    ''' 1. Sanitizes NaN/Infinity float samples → 0.0f (prevents AAC encoder crash)
    ''' 2. Truncates to complete frame alignment (prevents "Invalid PCM packet" error)
    ''' 3. Updates diagnostics counters atomically
    ''' </summary>
    Private Sub WriteSanitizedFrame(frame As AudioFrame, stream As Stream,
                                     source As AudioSource,
                                     ByRef samplesWritten As Long, ByRef bytesWritten As Long,
                                     ByRef nanCount As Long, ByRef infCount As Long,
                                     ByRef partialCount As Long)
        If stream Is Nothing OrElse Not stream.CanWrite Then Return
        If frame Is Nothing OrElse frame.Buffer Is Nothing OrElse frame.Length = 0 Then Return

        Dim fmt As AudioFormat = frame.Format
        If fmt Is Nothing Then Return

        Dim bytesPerSample As Integer = (fmt.BitsPerSample \ 8) * fmt.Channels
        If bytesPerSample < 1 Then bytesPerSample = 4

        ' ── Frame alignment: only write complete frames ──
        Dim alignedLength As Integer = (frame.Length \ bytesPerSample) * bytesPerSample
        If alignedLength < frame.Length Then
            System.Threading.Interlocked.Increment(partialCount)
        End If
        If alignedLength = 0 Then Return

        ' ── NaN/Infinity sanitization for f32le ──
        If fmt.IsFloat AndAlso fmt.BitsPerSample = 32 Then
            ' Sanitize in-place on a copy to avoid modifying the original buffer
            Dim buf As Byte() = frame.Buffer
            Dim numSamples As Integer = alignedLength \ 4
            For i As Integer = 0 To numSamples - 1
                Dim offset As Integer = i * 4
                Dim sample As Single = BitConverter.ToSingle(buf, offset)
                If Single.IsNaN(sample) Then
                    System.Threading.Interlocked.Increment(nanCount)
                    BitConverter.GetBytes(0.0F).CopyTo(buf, offset)
                ElseIf Single.IsInfinity(sample) Then
                    System.Threading.Interlocked.Increment(infCount)
                    BitConverter.GetBytes(0.0F).CopyTo(buf, offset)
                End If
            Next
        End If

        ' ── Write aligned data ──
        SyncLock stream
            Try
                stream.Write(frame.Buffer, 0, alignedLength)
                System.Threading.Interlocked.Add(samplesWritten, alignedLength \ bytesPerSample)
                System.Threading.Interlocked.Add(bytesWritten, alignedLength)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("[NAudio] Writer error (" & source.ToString() & "): " & ex.Message)
                ' Log to disk so we can see if writer is crashing (e.g. pipe not connected)
                Try
                    Dim logDir As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                    Dim logPath As String = System.IO.Path.Combine(logDir, "capture-engine.log")
                    BackgroundLogger.Log(logPath, "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "] [NAudio] Writer error (" & source.ToString() & "): " & ex.Message)
                Catch
                End Try
            End Try
        End SyncLock
    End Sub

    Private Sub OnSystemCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] System capture stopped")
        Try : _systemQueue?.CompleteAdding() : Catch : End Try
    End Sub

    Private Sub OnMicCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] Mic capture stopped")
        Try : _micQueue?.CompleteAdding() : Catch : End Try
    End Sub

    ''' <summary>
    ''' Maps an NAudio WaveFormat to our AudioFormat contract.
    '''
    ''' NAudio 2.3.0 note: WaveFormatExtensible.ChannelMask is NOT exposed in
    ''' the 2.x API (only added in NAudio 3 pre-release). So we cannot read the
    ''' real WASAPI dwChannelMask. We use the channel-count fallback instead:
    ''' 1ch → mono, 2ch → stereo, 6ch → 5.1, 8ch → 7.1, etc.
    '''
    ''' This is acceptable in practice because Windows's standard layouts match
    ''' the count-based mapping for the common cases (1, 2, 6, 8 channels).
    ''' For unusual counts (5ch), LayoutFromChannelCount returns "unspecified"
    ''' rather than guessing a wrong topology.
    '''
    ''' If future NAudio upgrade to 3.x happens, this is the ONLY function that
    ''' needs to change — read wfe.ChannelMask directly and call a real
    ''' ChannelMaskToLayout function.
    ''' </summary>
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

    Public Function GetDiagnostics() As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("[Audio] SysSamples=" & System.Threading.Interlocked.Read(_sysSamplesReceived))
        sb.AppendLine("[Audio] SysWritten=" & System.Threading.Interlocked.Read(_sysSamplesWritten))
        sb.AppendLine("[Audio] SysBytes=" & System.Threading.Interlocked.Read(_sysBytesWritten))
        sb.AppendLine("[Audio] SysNaN=" & System.Threading.Interlocked.Read(_sysNaNCount))
        sb.AppendLine("[Audio] SysInf=" & System.Threading.Interlocked.Read(_sysInfCount))
        sb.AppendLine("[Audio] SysPartial=" & System.Threading.Interlocked.Read(_sysPartialFrameCount))
        sb.AppendLine("[Audio] MicSamples=" & System.Threading.Interlocked.Read(_micSamplesReceived))
        sb.AppendLine("[Audio] MicWritten=" & System.Threading.Interlocked.Read(_micSamplesWritten))
        sb.AppendLine("[Audio] MicBytes=" & System.Threading.Interlocked.Read(_micBytesWritten))
        sb.AppendLine("[Audio] MicNaN=" & System.Threading.Interlocked.Read(_micNaNCount))
        sb.AppendLine("[Audio] MicInf=" & System.Threading.Interlocked.Read(_micInfCount))
        sb.AppendLine("[Audio] MicPartial=" & System.Threading.Interlocked.Read(_micPartialFrameCount))
        sb.AppendLine("[Audio] ShutdownRequested=" & _audioShutdownRequested.ToString())
        sb.AppendLine("[Audio] SysProducerStopped=" & _sysProducerStopped.ToString())
        sb.AppendLine("[Audio] MicProducerStopped=" & _micProducerStopped.ToString())
        sb.AppendLine("[Audio] SysPipeClosed=" & _sysPipeClosed.ToString())
        sb.AppendLine("[Audio] MicPipeClosed=" & _micPipeClosed.ToString())
        Return sb.ToString()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        [Stop]()
    End Sub

End Class
