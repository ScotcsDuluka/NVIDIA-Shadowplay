Imports NAudio.Wave
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports System.Collections.Concurrent
Imports System.Timers

Namespace CaptureCore

    Public Class AudioPipe
        Implements IDisposable

#Region "Constants"
        ' ★ Fix F2: PIPE_BUFFER_SIZE 256KB → 1MB (Round 1 had 256KB which was too small).
        ' Round 1 reduced from 2MB → 256KB to cut latency, but 256KB = ~0.7s of audio
        ' which was NOT enough headroom when FFmpeg's encoder was busy warming up at
        ' recording start. Result: audio buffer overflowed → audio drop → stutter
        ' + first frames looked choppy.
        ' 1MB = ~2.7s of audio at 48kHz stereo f32le. Still much smaller than the
        ' original 2MB (so latency is cut roughly in half) but big enough to absorb
        ' encoder init burst without dropping.
        Private Const PIPE_BUFFER_SIZE As Integer = 1024 * 1024  ' 1MB pipe buffer
        Private Const AUDIO_PIPE_PREFIX As String = "ShadowPlay_Audio_"

        ' ★ Fix M: SILENT_TIMEOUT_MS 50 → 500ms (was 50 in Round 1, 80 in Master).
        ' WASAPI loopback normally emits audio every 10-30ms, but during system
        ' load (high CPU, disk I/O) gaps can stretch to 100-300ms. Round 1's
        ' 50ms timeout was too aggressive — every time WASAPI paused >50ms the
        ' silent timer kicked in and inserted a silent frame, then immediately
        ' stopped when real audio arrived. This START-STOP-START-STOP pattern
        ' appeared in the log ~15 times in a few seconds and produced audible
        ' stuttering in the output mp4.
        ' 500ms = only insert silence if WASAPI has been truly silent for half
        ' a second (genuinely no audio playing). Real audio bursts of 100-300ms
        ' gap will NOT trigger silent frame insertion.
        ' SILENT_FRAME_INTERVAL_MS kept at 30ms (Round 1 value) — when silence
        ' IS needed, send it at 30ms intervals to match real audio cadence.
        Private Const SILENT_FRAME_INTERVAL_MS As Integer = 30   ' 30ms for tighter silence
        Private Const SILENT_TIMEOUT_MS As Integer = 500         ' was 50ms — too aggressive

        ''' <summary>Max items in write queue before we start dropping oldest</summary>
        Private Const QUEUE_DROP_THRESHOLD As Integer = 30

        ''' <summary>How long to wait for pipe connection before considering it failed.
        ''' ★ v4 FIX: Must match _connectionTimeout to avoid race where WaitForConnection
        ''' gives up but writer thread is still waiting (or vice versa).</summary>
        Private Const PIPE_CONNECT_TIMEOUT_MS As Integer = 15000
#End Region

#Region "Private Fields"
        Private _audioCapture As AudioCapture
        Private _namedPipeServer As NamedPipeServerStream
        Private _pipeName As String
        Private _isRunning As Boolean = False
        Private _isDisposed As Boolean = False
        Private _waveFormat As WaveFormat
        Private _volume As Single = 1.0F
        ''' <summary>★ v4 FIX: Unified timeout — same as PIPE_CONNECT_TIMEOUT_MS.
        ''' Old bug: _connectionTimeout=5s but writer thread waited 15s.
        ''' If FFmpeg connected at 6-15s, WaitForConnection already gave up,
        ''' EndWaitForConnection was never called → pipe in broken state.</summary>
        Private _connectionTimeout As Integer = PIPE_CONNECT_TIMEOUT_MS

        ' ★ v3.1: Connection state — CRITICAL for writer thread
        ' _ffmpegConnected = True only after FFmpeg actually opens the pipe
        ' Before that, writer thread must NOT try to write (would fail + mark disconnected)
        Private _ffmpegConnected As Boolean = False

        ' ★ v3.1: Producer/Consumer queue — WASAPI callback pushes, writer thread drains
        Private _writeQueue As New ConcurrentQueue(Of Byte())()
        Private _writerThread As Thread
        Private _writerSignal As New AutoResetEvent(False)
        Private _stopWriter As New ManualResetEvent(False)
        Private _connectedEvent As New ManualResetEvent(False)

        ' Silent Frame Generator
        Private _silentTimer As System.Timers.Timer
        Private _lastAudioTimeMs As Long = 0
        Private _silentBuffer As Byte() = Nothing
        Private _silentBufferSize As Integer = 0
        Private _isSendingSilence As Boolean = False
        Private _silenceStopwatch As New Stopwatch()

        ' Pipe health tracking
        Private _pipeDisconnected As Boolean = False
        Private _bytesWritten As Long = 0
#End Region

#Region "Properties"
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return _isRunning
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _ffmpegConnected AndAlso _namedPipeServer IsNot Nothing AndAlso _namedPipeServer.IsConnected AndAlso Not _pipeDisconnected
            End Get
        End Property

        Public ReadOnly Property PipeName As String
            Get
                Return _pipeName
            End Get
        End Property

        Public ReadOnly Property PipePath As String
            Get
                Return "\\.\pipe\" & _pipeName
            End Get
        End Property

        ''' <summary>
        ''' ★ v4 FIX: Volume is stored but NOT propagated to AudioCapture.
        ''' Old bug: AudioCapture.ApplyVolumeFast + FFmpeg -af volume = double volume!
        ''' AudioPipe stores volume so ScreenRecorder can read it, but the actual
        ''' volume application happens ONLY in FFmpeg (-af or filter_complex).
        ''' </summary>
        Public Property Volume As Single
            Get
                Return _volume
            End Get
            Set(value As Single)
                _volume = Math.Max(0.0F, Math.Min(1.0F, value))
                ' ★ v4: Do NOT propagate to AudioCapture — volume is handled by FFmpeg only
            End Set
        End Property

        Public ReadOnly Property CurrentWaveFormat As WaveFormat
            Get
                Return _waveFormat
            End Get
        End Property
#End Region

#Region "Events"
        Public Event PipeStarted As EventHandler
        Public Event PipeStopped As EventHandler
        Public Event PipeConnected As EventHandler
        Public Event PipeError As EventHandler(Of String)

        ''' <summary>
        ''' ★ v3: แจกจ่ายข้อมูลเสียงให้ subscriber อื่น (เช่น HighlightDetector)
        ''' ส่ง copy ของข้อมูล — subscriber ไม่ต้อง copy เอง
        ''' </summary>
        Public Event AudioTapped As EventHandler(Of AudioTappedEventArgs)
#End Region

#Region "Constructor"
        Public Sub New()
            _pipeName = AUDIO_PIPE_PREFIX & Process.GetCurrentProcess().Id & "_" & Guid.NewGuid().ToString("N").Substring(0, 8)
        End Sub

        Public Sub New(volume As Single)
            Me.New()
            _volume = volume
        End Sub
#End Region

#Region "Public Methods"
        Public Function Start() As Boolean
            If _isRunning Then Return True

            Try
                Debug.WriteLine("═══ AudioPipe.Start ═══")

                _pipeDisconnected = False
                _ffmpegConnected = False
                _bytesWritten = 0
                _lastAudioTimeMs = 0

                ' Create named pipe
                _namedPipeServer = New NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.Out,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    PIPE_BUFFER_SIZE,
                    PIPE_BUFFER_SIZE
                )

                Debug.WriteLine("AudioPipe: Created named pipe: " & PipePath)

                ' Start audio capture
                _audioCapture = New AudioCapture()
                ' ★ v4: Don't set _audioCapture.Volume — volume handled by FFmpeg only
                AddHandler _audioCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                AddHandler _audioCapture.CaptureError, AddressOf OnCaptureError

                _audioCapture.StartCapture()
                _isRunning = True

                ' Get wave format
                If _audioCapture.WaveFormat IsNot Nothing Then
                    _waveFormat = _audioCapture.WaveFormat
                    Debug.WriteLine("AudioPipe: Format from AudioCapture: " & _waveFormat.ToString())
                    PrepareSilentBuffer()
                End If

                ' ★ v3.1: Reset sync events before starting threads
                _stopWriter.Reset()
                _writerSignal.Reset()
                _connectedEvent.Reset()

                ' ★ v3.1: Start writer thread (drains queue → pipe)
                _writerThread = New Thread(AddressOf WriterThreadProc) With {
                    .Name = "AudioPipeWriter",
                    .IsBackground = True,
                    .Priority = ThreadPriority.AboveNormal
                }
                _writerThread.Start()

                ' Start silent timer
                StartSilentTimer()

                ' Wait for FFmpeg connection (async)
                Task.Run(Sub() WaitForConnection())

                RaiseEvent PipeStarted(Me, EventArgs.Empty)
                Debug.WriteLine("AudioPipe: Started - waiting for FFmpeg connection...")

                Return True

            Catch ex As Exception
                Debug.WriteLine("AudioPipe Start Error: " & ex.Message)
                RaiseEvent PipeError(Me, ex.Message)
                Return False
            End Try
        End Function

        Public Sub [Stop]()
            If Not _isRunning Then Exit Sub

            Debug.WriteLine("═══ AudioPipe.Stop ═══")
            _isRunning = False

            Try
                ' ★ v3.1: Stop order matters:
                ' 1. Stop timer (no more silence data)
                ' 2. Signal writer thread to stop
                ' 3. Stop capture (no more real audio data)
                ' 4. Wait for writer thread to drain remaining queue
                ' 5. Close pipe

                StopSilentTimer()

                ' Signal writer thread to stop
                _stopWriter.Set()
                _writerSignal.Set()

                ' Stop capture
                If _audioCapture IsNot Nothing Then
                    Try
                        RemoveHandler _audioCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                        RemoveHandler _audioCapture.CaptureError, AddressOf OnCaptureError
                    Catch
                    End Try
                    _audioCapture.StopCapture()
                    _audioCapture.Dispose()
                    _audioCapture = Nothing
                End If

                ' Wait for writer thread to finish (with timeout)
                If _writerThread IsNot Nothing Then
                    If Not _writerThread.Join(2000) Then
                        Debug.WriteLine("AudioPipe: Writer thread didn't exit in time")
                    End If
                    _writerThread = Nothing
                End If

                ' Close pipe
                If _namedPipeServer IsNot Nothing Then
                    Try
                        ' ★ v4 FIX: Don't Flush() — it can block forever if FFmpeg already exited.
                        ' The data in the pipe buffer will be lost anyway since FFmpeg is gone.
                        ' Just dispose the pipe immediately.
                        _namedPipeServer.Dispose()
                    Catch ex As Exception
                        Debug.WriteLine("AudioPipe: Pipe close error (expected): " & ex.Message)
                    End Try
                    _namedPipeServer = Nothing
                End If

                ' Clear any remaining queue items
                Dim dummy As Byte() = Nothing
                While _writeQueue.TryDequeue(dummy)
                End While

                RaiseEvent PipeStopped(Me, EventArgs.Empty)
                Debug.WriteLine(String.Format("AudioPipe: Stopped (wrote {0:N0} bytes total)", _bytesWritten))

            Catch ex As Exception
                Debug.WriteLine("AudioPipe Stop Error: " & ex.Message)
            End Try
        End Sub

        Public Function GetFFmpegInputArgs() As String
            If _waveFormat Is Nothing AndAlso _audioCapture IsNot Nothing Then
                _waveFormat = _audioCapture.WaveFormat
                If _waveFormat IsNot Nothing Then
                    Debug.WriteLine("AudioPipe: Got format from AudioCapture: " & _waveFormat.ToString())
                    PrepareSilentBuffer()
                End If
            End If

            Dim format As String = GetFFmpegFormat()
            Dim rate As Integer = GetSampleRate()
            Dim channels As Integer = GetChannels()

            Debug.WriteLine(String.Format("AudioPipe: FFmpeg input args - format={0}, rate={1}, channels={2}", format, rate, channels))

            Return String.Format("-thread_queue_size 512 -f {0} -ar {1} -ac {2} -i ""{3}""",
                format, rate, channels, PipePath)
        End Function

        Private Function GetFFmpegFormat() As String
            If _waveFormat Is Nothing Then Return "f32le"

            Select Case _waveFormat.BitsPerSample
                Case 16 : Return "s16le"
                Case 24 : Return "s24le"
                Case 32
                    If _waveFormat.Encoding = WaveFormatEncoding.IeeeFloat Then
                        Return "f32le"
                    Else
                        Return "s32le"
                    End If
                Case Else : Return "f32le"
            End Select
        End Function

        Private Function GetSampleRate() As Integer
            If _waveFormat IsNot Nothing Then Return _waveFormat.SampleRate
            Return 48000
        End Function

        Private Function GetChannels() As Integer
            If _waveFormat IsNot Nothing Then Return _waveFormat.Channels
            Return 2
        End Function
#End Region

#Region "Writer Thread"
        ''' <summary>
        ''' ★ v3.1: Dedicated writer thread that drains the ConcurrentQueue and writes to pipe.
        ''' 
        ''' CRITICAL FIX: Writer thread now WAITS for FFmpeg connection before writing.
        ''' Old v3 bug: If FFmpeg hadn't connected yet, writer thread saw IsConnected=False
        ''' and set _pipeDisconnected=True, which DROPPED ALL AUDIO forever!
        ''' 
        ''' Flow:
        '''   1. Wait for signal OR stop
        '''   2. If not connected yet → wait for _connectedEvent (with timeout)
        '''   3. Once connected → drain queue → write to pipe
        ''' </summary>
        Private Sub WriterThreadProc()
            Debug.WriteLine("AudioPipe: Writer thread started")

            Dim waitHandles() As WaitHandle = {_writerSignal, _stopWriter}

            Do While True
                ' Wait for signal or stop
                WaitHandle.WaitAny(waitHandles, 200)

                ' Check if we should stop first
                If _stopWriter.WaitOne(0) Then Exit Do
                If Not _isRunning Then Exit Do

                ' ═══════════════════════════════════════════════════════════════════════
                ' ★ v3.1 FIX: If FFmpeg hasn't connected yet, DON'T mark pipe as disconnected!
                ' Just wait. Data stays in queue (bounded by QUEUE_DROP_THRESHOLD).
                ' ═══════════════════════════════════════════════════════════════════════
                If Not Volatile.Read(_ffmpegConnected) Then
                    ' Wait for FFmpeg to connect (with timeout to check stop periodically)
                    Dim connectedWaitHandles() As WaitHandle = {_connectedEvent, _stopWriter}
                    Dim connectResult As Integer = WaitHandle.WaitAny(connectedWaitHandles, PIPE_CONNECT_TIMEOUT_MS)

                    If connectResult = 1 Then Exit Do  ' stopWriter signaled
                    If connectResult = WaitHandle.WaitTimeout Then
                        Debug.WriteLine("AudioPipe: FFmpeg connection timeout in writer thread")
                        Volatile.Write(_pipeDisconnected, True)
                        ' Discard queue
                        Dim discard As Byte() = Nothing
                        While _writeQueue.TryDequeue(discard)
                        End While
                        ' ★ v4: Stop WASAPI capture on timeout
                        TryStopCaptureOnPipeDisconnect()
                        Exit Do
                    End If

                    ' FFmpeg connected! Continue to drain queue.
                End If

                ' ═══════════════════════════════════════════════════════════════════════
                ' Drain all available data from queue
                ' ═══════════════════════════════════════════════════════════════════════
                Dim data As Byte() = Nothing

                While _writeQueue.TryDequeue(data)
                    ' ★ v4 FIX: Use Volatile.Read for thread-safe check of _pipeDisconnected
                    If Volatile.Read(_pipeDisconnected) OrElse _namedPipeServer Is Nothing OrElse Not _namedPipeServer.IsConnected Then
                        Volatile.Write(_pipeDisconnected, True)
                        ' Discard remaining queue
                        Dim discard As Byte() = Nothing
                        While _writeQueue.TryDequeue(discard)
                        End While
                        ' ★ v4: Stop WASAPI capture when pipe dies — saves CPU
                        TryStopCaptureOnPipeDisconnect()
                        Exit Do
                    End If

                    Try
                        _namedPipeServer.Write(data, 0, data.Length)
                        _bytesWritten += data.Length
                    Catch ex As IOException
                        ' Pipe disconnected — FFmpeg likely exited
                        Volatile.Write(_pipeDisconnected, True)
                        Debug.WriteLine("AudioPipe: Pipe disconnected in writer thread (FFmpeg exited?)")
                        Dim discard As Byte() = Nothing
                        While _writeQueue.TryDequeue(discard)
                        End While
                        TryStopCaptureOnPipeDisconnect()
                        Exit Do
                    Catch ex As ObjectDisposedException
                        Volatile.Write(_pipeDisconnected, True)
                        TryStopCaptureOnPipeDisconnect()
                        Exit Do
                    Catch ex As Exception
                        If Volatile.Read(_isRunning) Then
                            Debug.WriteLine("AudioPipe Write Error: " & ex.Message)
                        End If
                        Volatile.Write(_pipeDisconnected, True)
                        TryStopCaptureOnPipeDisconnect()
                        Exit Do
                    End Try
                End While

                ' Check stop again before looping
                If _stopWriter.WaitOne(0) Then Exit Do
            Loop

            Debug.WriteLine("AudioPipe: Writer thread exiting")
        End Sub
#End Region

#Region "Silent Frame Generator"

        Private Sub PrepareSilentBuffer()
            If _waveFormat Is Nothing Then Exit Sub

            Dim bytesPerSample As Integer = 4
            If _waveFormat.Encoding <> WaveFormatEncoding.IeeeFloat Then
                bytesPerSample = _waveFormat.BitsPerSample \ 8
            End If

            Dim samplesPerInterval As Integer = CInt(_waveFormat.SampleRate * SILENT_FRAME_INTERVAL_MS / 1000.0)
            _silentBufferSize = samplesPerInterval * _waveFormat.Channels * bytesPerSample
            _silentBuffer = New Byte(_silentBufferSize - 1) {}

            Debug.WriteLine(String.Format("AudioPipe: Silent buffer prepared - {0} bytes ({1}ms)",
                _silentBufferSize, SILENT_FRAME_INTERVAL_MS))
        End Sub

        Private Sub StartSilentTimer()
            If _silentTimer IsNot Nothing Then
                _silentTimer.Stop()
                _silentTimer.Dispose()
            End If

            _silentTimer = New System.Timers.Timer(SILENT_FRAME_INTERVAL_MS)
            AddHandler _silentTimer.Elapsed, AddressOf OnSilentTimerElapsed
            _silentTimer.AutoReset = True
            _silentTimer.Start()

            _silenceStopwatch.Restart()
            _lastAudioTimeMs = _silenceStopwatch.ElapsedMilliseconds
            _isSendingSilence = False
            Debug.WriteLine("AudioPipe: Silent frame timer started")
        End Sub

        Private Sub StopSilentTimer()
            If _silentTimer IsNot Nothing Then
                Try
                    _silentTimer.Stop()
                    RemoveHandler _silentTimer.Elapsed, AddressOf OnSilentTimerElapsed
                    _silentTimer.Dispose()
                Catch
                End Try
                _silentTimer = Nothing
            End If
            _silenceStopwatch.Stop()
        End Sub

        Private Sub OnSilentTimerElapsed(sender As Object, e As ElapsedEventArgs)
            If Not Volatile.Read(_isRunning) Then Exit Sub
            If Volatile.Read(_pipeDisconnected) Then Exit Sub
            If _silentBuffer Is Nothing Then Exit Sub

            Try
                Dim nowMs As Long = _silenceStopwatch.ElapsedMilliseconds
                Dim timeSinceLastAudio As Long = nowMs - _lastAudioTimeMs

                If timeSinceLastAudio >= SILENT_TIMEOUT_MS Then
                    If Not _isSendingSilence Then
                        _isSendingSilence = True
                        Debug.WriteLine("AudioPipe: Started sending silent frames")
                    End If

                    ' Enqueue silence copy
                    Dim silenceCopy As Byte() = New Byte(_silentBufferSize - 1) {}
                    EnqueueAudioData(silenceCopy)
                End If

            Catch ex As Exception
                If _isRunning Then
                    Debug.WriteLine("AudioPipe Silent Timer Error: " & ex.Message)
                End If
            End Try
        End Sub

#End Region

#Region "Private Methods — Audio Data Flow"

        Private Sub EnqueueAudioData(data As Byte())
            If Volatile.Read(_pipeDisconnected) Then Exit Sub

            ' Drop-on-full: if queue is too large, drop oldest items
            While _writeQueue.Count >= QUEUE_DROP_THRESHOLD
                Dim dropped As Byte() = Nothing
                _writeQueue.TryDequeue(dropped)
            End While

            _writeQueue.Enqueue(data)
            _writerSignal.Set()
        End Sub

        Private Sub WaitForConnection()
            Try
                Dim asyncResult = _namedPipeServer.BeginWaitForConnection(Nothing, Nothing)

                If asyncResult.AsyncWaitHandle.WaitOne(_connectionTimeout) Then
                    Try
                        _namedPipeServer.EndWaitForConnection(asyncResult)

                        ' ★ v4 FIX: Use Volatile.Write for thread-safe flag
                        Volatile.Write(_ffmpegConnected, True)
                        _connectedEvent.Set()

                        Debug.WriteLine("AudioPipe: FFmpeg connected!")
                        RaiseEvent PipeConnected(Me, EventArgs.Empty)
                    Catch ex As ObjectDisposedException
                        Debug.WriteLine("AudioPipe: Connection cancelled (pipe disposed)")
                    End Try
                Else
                    Debug.WriteLine("AudioPipe: Connection timeout - continuing anyway")
                    ' Don't set _ffmpegConnected — writer thread will handle timeout
                End If
            Catch ex As Exception
                Debug.WriteLine("AudioPipe WaitForConnection Error: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' ★ RACE FIX: Waits for the pipe to be ready for FFmpeg to connect.
        ''' Returns True if the pipe server is created and waiting for connection.
        ''' This is a SHORT wait — it only ensures the pipe server exists,
        ''' not that FFmpeg has connected (that happens asynchronously).
        ''' </summary>
        Public Async Function WaitForPipeServerReadyAsync(timeoutMs As Integer) As Task(Of Boolean)
            ' Wait for the pipe stream to be created (should be near-instant after Start())
            Dim sw As New Stopwatch()
            sw.Start()
            
            While sw.ElapsedMilliseconds < timeoutMs
                If _namedPipeServer IsNot Nothing AndAlso _isRunning Then
                    Return True
                End If
                Await Task.Delay(5)
            End While
            
            Debug.WriteLine("AudioPipe: WaitForPipeServerReadyAsync timed out - pipe server never created")
            Return False
        End Function

        Private Sub OnAudioDataAvailable(sender As Object, e As AudioDataEventArgs)
            If Not Volatile.Read(_isRunning) Then
                e.ReturnBuffer()
                Exit Sub
            End If

            Try
                ' Update silence tracking
                _lastAudioTimeMs = _silenceStopwatch.ElapsedMilliseconds
                If _isSendingSilence Then
                    _isSendingSilence = False
                    Debug.WriteLine("AudioPipe: Real audio received - stopped silent frames")
                End If

                ' Copy data and return buffer to pool immediately
                Dim dataCopy As Byte() = New Byte(e.BytesRecorded - 1) {}
                Array.Copy(e.Buffer, dataCopy, e.BytesRecorded)
                e.ReturnBuffer()

                ' Fire AudioTapped event with the COPY
                RaiseEvent AudioTapped(Me, New AudioTappedEventArgs(dataCopy, e.BytesRecorded, e.WaveFormat))

                ' Enqueue for writer thread (non-blocking)
                EnqueueAudioData(dataCopy)

                ' Store format
                If _waveFormat Is Nothing AndAlso e.WaveFormat IsNot Nothing Then
                    _waveFormat = e.WaveFormat
                    Debug.WriteLine("AudioPipe: Format = " & _waveFormat.ToString())
                    PrepareSilentBuffer()
                End If

            Catch ex As Exception
                If Volatile.Read(_isRunning) Then
                    Debug.WriteLine("AudioPipe AudioData Error: " & ex.Message)
                End If
                e.ReturnBuffer()
            End Try
        End Sub

        ''' <summary>
        ''' ★ v4: Stop WASAPI capture when pipe disconnects (FFmpeg exited).
        ''' Without this, WASAPI Loopback keeps running even though there's nowhere
        ''' to send the data — wasting CPU and causing the writer thread queue to fill up.
        ''' </summary>
        Private Sub TryStopCaptureOnPipeDisconnect()
            If _audioCapture IsNot Nothing AndAlso _audioCapture.IsCapturing Then
                Try
                    Debug.WriteLine("AudioPipe: Pipe disconnected — stopping WASAPI capture to save CPU")
                    _audioCapture.StopCapture()
                Catch ex As Exception
                    Debug.WriteLine("AudioPipe: Error stopping capture on pipe disconnect: " & ex.Message)
                End Try
            End If
        End Sub

        Private Sub OnCaptureError(sender As Object, errorMessage As String)
            Debug.WriteLine("AudioPipe: Capture Error - " & errorMessage)
            RaiseEvent PipeError(Me, errorMessage)
        End Sub
#End Region

#Region "IDisposable"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If _isDisposed Then Exit Sub

            If disposing Then
                Try
                    [Stop]()
                Catch
                End Try

                ' Dispose sync events
                Try
                    _writerSignal.Dispose()
                Catch
                End Try
                Try
                    _stopWriter.Dispose()
                Catch
                End Try
                Try
                    _connectedEvent.Dispose()
                Catch
                End Try
            End If

            _isDisposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

    ''' <summary>
    ''' ★ v3: Event args สำหรับ AudioTapped — ส่งข้อมูลเสียงดิบให้ subscriber
    ''' ข้อมูลเป็น COPY — subscriber ถือได้โดยไม่มีผลต่อ pipe
    ''' </summary>
    Public Class AudioTappedEventArgs
        Inherits EventArgs

        Public ReadOnly Property Buffer As Byte()
        Public ReadOnly Property BytesRecorded As Integer
        Public ReadOnly Property WaveFormat As WaveFormat

        Public Sub New(buffer As Byte(), bytesRecorded As Integer, format As WaveFormat)
            Me.Buffer = buffer
            Me.BytesRecorded = bytesRecorded
            Me.WaveFormat = format
        End Sub
    End Class

End Namespace
