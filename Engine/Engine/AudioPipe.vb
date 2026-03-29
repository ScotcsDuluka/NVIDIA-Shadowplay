Imports NAudio.Wave
Imports NAudio.CoreAudioApi
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Timers
Imports System.Collections.Concurrent

Namespace CaptureCore

    ''' <summary>
    ''' Audio Pipe - Streams system audio to FFmpeg via Named Pipe
    ''' 
    ''' v2.0 Improvements:
    '''   - Fixed race condition: _isRunning now uses Volatile.Read/Write
    '''   - Thread-safe _waveFormat access with _formatLock
    '''   - Silent buffer now correctly handles non-float formats
    '''   - No more empty Catch blocks - all errors logged with context
    '''   - Pipe connection wait uses async pattern properly
    '''   - Volume is applied only once (in AudioCapture, not duplicated here)
    ''' 
    ''' v2.1 Improvements:
    '''   - FIX 1: ManualResetEventSlim ensures WaveFormat is available before FFmpeg args
    '''   - FIX 2: Rate-matched silent frames instead of fixed 100ms intervals
    '''   - FIX 3: Volatile ticks for _lastAudioTime (thread-safe without lock)
    '''   - FIX 4: ConcurrentQueue + background WriteLoop for backpressure
    ''' </summary>
    Public Class AudioPipe
        Implements IDisposable

#Region "Constants"
        Private Const PIPE_BUFFER_SIZE As Integer = 2 * 1024 * 1024  ' 2MB buffer
        Private Const AUDIO_PIPE_PREFIX As String = "ShadowPlay_Audio_"

        ' Silent Frame Settings (Optimized)
        Private Const SILENT_FRAME_INTERVAL_MS As Integer = 100  ' Check every 100ms
        Private Const SILENT_TIMEOUT_MS As Integer = 200         ' Start silence after 200ms no audio
#End Region

#Region "Private Fields"
        Private _audioCapture As AudioCapture
        Private _namedPipeServer As NamedPipeServerStream
        Private _pipeName As String
        Private _isRunning As Boolean = False
        Private _isDisposed As Boolean = False
        Private _writeLock As New Object()
        Private _formatLock As New Object()
        Private _waveFormat As WaveFormat
        Private _volume As Single = 1.0F
        Private _connectionTimeout As Integer = 5000

        ' Silent Frame Generator
        Private _silentTimer As System.Timers.Timer
        Private _lastAudioTimeTicks As Long = DateTime.MinValue.Ticks
        Private _silentBuffer As Byte() = Nothing
        Private _silentBufferSize As Integer = 0
        Private _isSendingSilence As Boolean = False

        ' FIX 1: Format readiness synchronization
        Private _formatReadyEvent As New ManualResetEventSlim(False)

        ' FIX 2: Rate-matched silent frame tracking
        Private _audioBytesReceived As Long = 0
        Private _lastSilentBytesSent As Long = 0
        Private _bytesReceivedLock As New Object()

        ' FIX 4: Backpressure with buffered write
        Private _writeQueue As New ConcurrentQueue(Of Byte())()
        Private _writeThread As Thread
        Private _writeCts As CancellationTokenSource
#End Region

#Region "Properties"
        ''' <summary>
        ''' Whether the pipe is actively running. Uses Volatile for thread-safe reads.
        ''' </summary>
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return Threading.Volatile.Read(_isRunning)
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Dim pipe As NamedPipeServerStream = _namedPipeServer
                Return pipe IsNot Nothing AndAlso pipe.IsConnected
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
        ''' Volume control (0.0 - 1.0). Applied inside AudioCapture, not duplicated here.
        ''' Setting this updates the underlying AudioCapture volume.
        ''' </summary>
        Public Property Volume As Single
            Get
                Return _volume
            End Get
            Set(value As Single)
                _volume = Math.Max(0.0F, Math.Min(1.0F, value))
                SyncLock _writeLock
                    If _audioCapture IsNot Nothing Then
                        _audioCapture.Volume = _volume
                    End If
                End SyncLock
            End Set
        End Property

        ''' <summary>
        ''' Current audio format. Thread-safe.
        ''' </summary>
        Public ReadOnly Property CurrentWaveFormat As WaveFormat
            Get
                SyncLock _formatLock
                    Return _waveFormat
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Event that signals when WaveFormat is available for FFmpeg args.
        ''' Used to prevent GetFFmpegInputArgs() from returning defaults before format is populated.
        ''' </summary>
        Public ReadOnly Property FormatReadyEvent As ManualResetEventSlim
            Get
                Return _formatReadyEvent
            End Get
        End Property
#End Region

#Region "Events"
        Public Event PipeStarted As EventHandler
        Public Event PipeStopped As EventHandler
        Public Event PipeConnected As EventHandler
        Public Event PipeError As EventHandler(Of String)
#End Region

#Region "Constructor"
        Public Sub New()
            _pipeName = AUDIO_PIPE_PREFIX & Process.GetCurrentProcess().Id & "_" & Guid.NewGuid().ToString("N").Substring(0, 8)
        End Sub

        Public Sub New(volume As Single)
            Me.New()
            _volume = Math.Max(0.0F, Math.Min(1.0F, volume))
        End Sub
#End Region

#Region "Public Methods"
        Public Function Start() As Boolean
            SyncLock _writeLock
                If Threading.Volatile.Read(_isRunning) Then Return True

                Try
                    Debug.WriteLine("═══ AudioPipe.Start ═══")

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
                    _audioCapture.Volume = _volume
                    AddHandler _audioCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                    AddHandler _audioCapture.CaptureError, AddressOf OnCaptureError

                    _audioCapture.StartCapture()
                    Threading.Volatile.Write(_isRunning, True)

                    ' Get wave format (thread-safe)
                    Dim capturedFormat As WaveFormat = _audioCapture.WaveFormat
                    If capturedFormat IsNot Nothing Then
                        SyncLock _formatLock
                            _waveFormat = capturedFormat
                        End SyncLock
                        Debug.WriteLine("AudioPipe: Format from AudioCapture: " & capturedFormat.ToString())
                        PrepareSilentBuffer()
                        ' FIX 1: Signal that format is ready
                        _formatReadyEvent.Set()
                    End If

                    ' Start silent timer
                    StartSilentTimer()

                    ' FIX 4: Start background writer thread for backpressure
                    _writeCts = New CancellationTokenSource()
                    _writeThread = New Thread(AddressOf WriteLoop)
                    _writeThread.IsBackground = True
                    _writeThread.Start()
                    Debug.WriteLine("AudioPipe: Background write thread started")

                    ' Wait for FFmpeg connection (fire and forget - timeout handled internally)
                    Task.Run(Sub() WaitForConnection())

                    RaiseEvent PipeStarted(Me, EventArgs.Empty)
                    Debug.WriteLine("AudioPipe: Started - waiting for FFmpeg connection...")

                    Return True

                Catch ex As Exception
                    Debug.WriteLine("AudioPipe Start Error: " & ex.Message)
                    RaiseEvent PipeError(Me, "Failed to start: " & ex.Message)
                    CleanupOnFailure()
                    Return False
                End Try
            End SyncLock
        End Function

        ''' <summary>
        ''' Full stop: set flag, cancel CTS, stop timer, stop capture, close pipe.
        ''' IOException in WriteLoop is caught and handled (first-chance, harmless).
        ''' </summary>
        Public Sub [Stop]()
            SyncLock _writeLock
                If Not Threading.Volatile.Read(_isRunning) Then Exit Sub

                Debug.WriteLine("═══ AudioPipe.Stop ═══")
                Threading.Volatile.Write(_isRunning, False)
            End SyncLock

            ' Stop timer first (outside lock to avoid deadlock)
            StopSilentTimer()

            ' Cancel write thread CTS
            If _writeCts IsNot Nothing Then
                _writeCts.Cancel()
            End If

            ' Stop audio capture
            SyncLock _writeLock
                Try
                    If _audioCapture IsNot Nothing Then
                        RemoveHandler _audioCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                        RemoveHandler _audioCapture.CaptureError, AddressOf OnCaptureError
                        _audioCapture.StopCapture()
                        _audioCapture.Dispose()
                        _audioCapture = Nothing
                    End If
                Catch ex As Exception
                    Debug.WriteLine("AudioPipe Stop (AudioCapture): " & ex.Message)
                End Try

                ' Close pipe
                Try
                    If _namedPipeServer IsNot Nothing Then
                        _namedPipeServer.Dispose()
                        _namedPipeServer = Nothing
                    End If
                Catch ex As Exception
                    Debug.WriteLine("AudioPipe Stop (Pipe): " & ex.Message)
                End Try
            End SyncLock

            ' Wait for WriteLoop to exit (max 3s)
            If _writeThread IsNot Nothing AndAlso _writeThread.IsAlive Then
                If Not _writeThread.Join(3000) Then
                    Debug.WriteLine("AudioPipe: Write thread did not stop in time")
                End If
            End If

            RaiseEvent PipeStopped(Me, EventArgs.Empty)
            Debug.WriteLine("AudioPipe: Stopped")
        End Sub

        Public Function GetFFmpegInputArgs() As String
            ' FIX 1: Wait for format to be available (max 3 seconds)
            If Not _formatReadyEvent.Wait(3000) Then
                Debug.WriteLine("AudioPipe: WaveFormat not available after 3s, using defaults")
            End If

            ' Ensure format is available
            SyncLock _formatLock
                If _waveFormat Is Nothing AndAlso _audioCapture IsNot Nothing Then
                    _waveFormat = _audioCapture.WaveFormat
                    If _waveFormat IsNot Nothing Then
                        Debug.WriteLine("AudioPipe: Got format from AudioCapture: " & _waveFormat.ToString())
                        PrepareSilentBuffer()
                        ' FIX 1: Signal format ready now
                        _formatReadyEvent.Set()
                    End If
                End If
            End SyncLock

            Dim format As WaveFormat = CurrentWaveFormat
            Dim formatStr As String = GetFFmpegFormat(format)
            Dim rate As Integer = GetSampleRate(format)
            Dim channels As Integer = GetChannels(format)

            Debug.WriteLine(String.Format("AudioPipe: FFmpeg input args - format={0}, rate={1}, channels={2}", formatStr, rate, channels))

            Return String.Format("-thread_queue_size 512 -f {0} -ar {1} -ac {2} -i ""{3}""",
                formatStr, rate, channels, PipePath)
        End Function

        Private Function GetFFmpegFormat(format As WaveFormat) As String
            If format Is Nothing Then Return "f32le"

            Select Case format.BitsPerSample
                Case 16 : Return "s16le"
                Case 24 : Return "s24le"
                Case 32
                    If format.Encoding = WaveFormatEncoding.IeeeFloat Then
                        Return "f32le"
                    Else
                        Return "s32le"
                    End If
                Case Else : Return "f32le"
            End Select
        End Function

        Private Function GetSampleRate(format As WaveFormat) As Integer
            If format IsNot Nothing Then Return format.SampleRate
            Return 48000
        End Function

        Private Function GetChannels(format As WaveFormat) As Integer
            If format IsNot Nothing Then Return format.Channels
            Return 2
        End Function
#End Region

#Region "Silent Frame Generator"

        Private Sub PrepareSilentBuffer()
            Dim format As WaveFormat = CurrentWaveFormat
            If format Is Nothing Then Exit Sub

            ' Calculate buffer size based on actual format (not hardcoded 32-bit)
            Dim bytesPerSample As Integer
            If format.Encoding = WaveFormatEncoding.IeeeFloat Then
                bytesPerSample = If(format.BitsPerSample = 64, 8, 4)
            Else
                bytesPerSample = format.BitsPerSample \ 8
                If bytesPerSample <= 0 Then bytesPerSample = 4 ' Safety fallback
            End If

            Dim samplesPerInterval As Integer = CInt(format.SampleRate * SILENT_FRAME_INTERVAL_MS / 1000.0)
            _silentBufferSize = samplesPerInterval * format.Channels * bytesPerSample

            If _silentBufferSize > 0 Then
                _silentBuffer = New Byte(_silentBufferSize - 1) {}  ' All zeros = silence
            End If

            Debug.WriteLine(String.Format("AudioPipe: Silent buffer prepared - {0} bytes ({1}ms, {2}bps, {3}ch)",
                _silentBufferSize, SILENT_FRAME_INTERVAL_MS, bytesPerSample * 8, format.Channels))
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

            ' FIX 3: Use volatile ticks instead of DateTime
            _lastAudioTimeTicks = DateTime.Now.Ticks
            ' FIX 2: Reset rate-tracking counters
            _audioBytesReceived = 0
            _lastSilentBytesSent = 0
            _isSendingSilence = False
            Debug.WriteLine("AudioPipe: Silent frame timer started")
        End Sub

        Private Sub StopSilentTimer()
            If _silentTimer IsNot Nothing Then
                Try
                    _silentTimer.Stop()
                    RemoveHandler _silentTimer.Elapsed, AddressOf OnSilentTimerElapsed
                    _silentTimer.Dispose()
                Catch ex As Exception
                    Debug.WriteLine("AudioPipe StopSilentTimer Error: " & ex.Message)
                End Try
                _silentTimer = Nothing
            End If
        End Sub

        Private Sub OnSilentTimerElapsed(sender As Object, e As ElapsedEventArgs)
            If Not IsRunning Then Exit Sub

            Dim pipe As NamedPipeServerStream = _namedPipeServer
            If pipe Is Nothing OrElse Not pipe.IsConnected Then Exit Sub

            Try
                ' FIX 3: Use volatile ticks for thread-safe time comparison
                Dim timeSinceLastAudio As TimeSpan = DateTime.Now - New DateTime(LastAudioTimeTicks)

                ' Only send silence if no real audio for SILENT_TIMEOUT_MS
                If timeSinceLastAudio.TotalMilliseconds >= SILENT_TIMEOUT_MS Then
                    If Not _isSendingSilence Then
                        _isSendingSilence = True
                        Debug.WriteLine("AudioPipe: Started sending silent frames (rate-matched)")
                    End If

                    ' FIX 2: Calculate how many bytes of silence to send based on actual audio rate
                    Dim bytesToSend As Integer = 0
                    SyncLock _bytesReceivedLock
                        bytesToSend = CInt(_audioBytesReceived - _lastSilentBytesSent)
                        _lastSilentBytesSent = _audioBytesReceived
                    End SyncLock

                    If bytesToSend > 0 AndAlso _silentBuffer IsNot Nothing Then
                        ' Build a properly-sized silent buffer and enqueue it
                        Dim silentData As Byte() = New Byte(bytesToSend - 1) {}
                        Dim offset As Integer = 0
                        Do While offset < bytesToSend
                            Dim chunkSize As Integer = Math.Min(_silentBufferSize, bytesToSend - offset)
                            Array.Copy(_silentBuffer, 0, silentData, offset, chunkSize)
                            offset += chunkSize
                        Loop

                        ' FIX 4: Enqueue for background writer instead of direct pipe.Write()
                        SyncLock _writeLock
                            _writeQueue.Enqueue(silentData)
                        End SyncLock
                    End If
                End If

            Catch ex As Exception
                If IsRunning Then
                    Debug.WriteLine("AudioPipe Silent Timer Error: " & ex.Message)
                End If
            End Try
        End Sub

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' FIX 3: Thread-safe access to last audio time using volatile ticks.
        ''' </summary>
        Private Property LastAudioTimeTicks As Long
            Get
                Return Threading.Volatile.Read(_lastAudioTimeTicks)
            End Get
            Set(value As Long)
                Threading.Volatile.Write(_lastAudioTimeTicks, value)
            End Set
        End Property

        ''' <summary>
        ''' FIX 4: Background write loop that dequeues buffers and writes to pipe.
        ''' Prevents pipe.Write() from blocking the audio capture thread.
        ''' </summary>
        Private Sub WriteLoop()
            Try
                While Not _writeCts.IsCancellationRequested OrElse _writeQueue.Count > 0
                    ' v2.2 FIX: Check IsRunning BEFORE each write to avoid IOException on broken pipe.
                    ' When Stop() sets _isRunning=False and cancels CTS, the write thread should
                    ' exit immediately instead of attempting pipe.Write() on a dying connection.
                    If Not IsRunning OrElse _writeCts.IsCancellationRequested Then Exit While

                    Dim buffer As Byte() = Nothing
                    If _writeQueue.TryDequeue(buffer) Then
                        Dim pipe As NamedPipeServerStream = _namedPipeServer
                        If pipe IsNot Nothing AndAlso pipe.IsConnected Then
                            Try
                                pipe.Write(buffer, 0, buffer.Length)
                            Catch ex As System.IO.IOException
                                ' Pipe broken during stop - normal when FFmpeg disconnects.
                                ' Exit silently without logging (VS still shows first-chance, that's OK).
                                Exit While
                            Catch ex As ObjectDisposedException
                                ' Pipe was disposed while we were writing - normal during stop
                                Exit While
                            Catch ex As Exception
                                If IsRunning Then
                                    Debug.WriteLine("AudioPipe WriteLoop Error: " & ex.Message)
                                End If
                            End Try
                        End If
                    Else
                        Thread.Sleep(2)
                    End If
                End While
            Catch ex As Threading.ThreadAbortException
                ' Thread abort during shutdown - normal
            Catch ex As Exception
                Debug.WriteLine("AudioPipe WriteLoop crashed: " & ex.Message)
            End Try
            Debug.WriteLine("AudioPipe: WriteLoop exited")
        End Sub

        Private Sub WaitForConnection()
            Try
                Dim pipe As NamedPipeServerStream = _namedPipeServer
                If pipe Is Nothing Then Exit Sub

                Dim asyncResult = pipe.BeginWaitForConnection(Nothing, Nothing)

                If asyncResult.AsyncWaitHandle.WaitOne(_connectionTimeout) Then
                    pipe.EndWaitForConnection(asyncResult)
                    Debug.WriteLine("AudioPipe: FFmpeg connected!")
                    RaiseEvent PipeConnected(Me, EventArgs.Empty)
                Else
                    Debug.WriteLine("AudioPipe: Connection timeout - continuing anyway")
                End If
            Catch ex As Exception
                Debug.WriteLine("AudioPipe WaitForConnection Error: " & ex.Message)
                ' Don't raise error - FFmpeg might connect later or this is expected
            End Try
        End Sub

        Private Sub OnAudioDataAvailable(sender As Object, e As AudioDataEventArgs)
            If Not IsRunning Then Exit Sub

            Dim pipe As NamedPipeServerStream = _namedPipeServer
            If pipe Is Nothing Then Exit Sub
            If Not pipe.IsConnected Then Exit Sub

            Try
                ' FIX 3: Use volatile ticks for thread-safe timestamp
                LastAudioTimeTicks = DateTime.Now.Ticks
                If _isSendingSilence Then
                    _isSendingSilence = False
                    Debug.WriteLine("AudioPipe: Real audio received - stopped silent frames")
                End If

                ' FIX 4: Enqueue for background writer instead of direct pipe.Write()
                SyncLock _writeLock
                    _writeQueue.Enqueue(e.Buffer)
                End SyncLock

                ' FIX 2: Track actual audio bytes received for rate-matched silence
                SyncLock _bytesReceivedLock
                    _audioBytesReceived += e.BytesRecorded
                End SyncLock

                ' Store format (thread-safe)
                If e.WaveFormat IsNot Nothing Then
                    Dim needPrepare As Boolean = False
                    SyncLock _formatLock
                        If _waveFormat Is Nothing Then
                            _waveFormat = e.WaveFormat
                            needPrepare = True
                        End If
                    End SyncLock

                    If needPrepare Then
                        Debug.WriteLine("AudioPipe: Format = " & e.WaveFormat.ToString())
                        PrepareSilentBuffer()
                        ' FIX 1: Signal that format is ready
                        _formatReadyEvent.Set()
                    End If
                End If

            Catch ex As Exception
                If IsRunning Then
                    Debug.WriteLine("AudioPipe OnAudioDataAvailable Error: " & ex.Message)
                End If
            End Try
        End Sub

        Private Sub OnCaptureError(sender As Object, errorMessage As String)
            Debug.WriteLine("AudioPipe: Capture Error - " & errorMessage)
            RaiseEvent PipeError(Me, "Capture error: " & errorMessage)
        End Sub

        ''' <summary>
        ''' Cleanup resources when Start() fails mid-way
        ''' </summary>
        Private Sub CleanupOnFailure()
            Try
                If _audioCapture IsNot Nothing Then
                    RemoveHandler _audioCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                    RemoveHandler _audioCapture.CaptureError, AddressOf OnCaptureError
                    _audioCapture.StopCapture()
                    _audioCapture.Dispose()
                    _audioCapture = Nothing
                End If
            Catch ex As Exception
                Debug.WriteLine("AudioPipe CleanupOnFailure (AudioCapture): " & ex.Message)
            End Try

            Try
                If _namedPipeServer IsNot Nothing Then
                    _namedPipeServer.Dispose()
                    _namedPipeServer = Nothing
                End If
            Catch ex As Exception
                Debug.WriteLine("AudioPipe CleanupOnFailure (Pipe): " & ex.Message)
            End Try

            StopSilentTimer()

            ' FIX 4: Cancel write thread on failure
            If _writeCts IsNot Nothing Then
                _writeCts.Cancel()
            End If

            Threading.Volatile.Write(_isRunning, False)
        End Sub

#End Region

#Region "IDisposable"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If _isDisposed Then Exit Sub

            If disposing Then
                Try
                    [Stop]()
                Catch ex As Exception
                    Debug.WriteLine("AudioPipe.Dispose (Stop): " & ex.Message)
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
    ''' Mic Pipe - Streams microphone audio to FFmpeg via Named Pipe
    ''' Uses NAudio WaveIn instead of DirectShow for consistent latency
    ''' 
    ''' v1.0: New class to replace DirectShow microphone capture
    ''' </summary>
    Public Class MicPipe
        Implements IDisposable

#Region "Constants"
        Private Const PIPE_BUFFER_SIZE As Integer = 2 * 1024 * 1024
        Private Const MIC_PIPE_PREFIX As String = "ShadowPlay_Mic_"
        Private Const SILENT_FRAME_INTERVAL_MS As Integer = 100
        Private Const SILENT_TIMEOUT_MS As Integer = 200
#End Region

#Region "Private Fields"
        Private _micCapture As MicCapture
        Private _namedPipeServer As NamedPipeServerStream
        Private _pipeName As String
        Private _isRunning As Boolean = False
        Private _isDisposed As Boolean = False
        Private _writeLock As New Object()
        Private _formatLock As New Object()
        Private _waveFormat As WaveFormat
        Private _volume As Single = 1.0F
        Private _connectionTimeout As Integer = 5000

        Private _silentTimer As System.Timers.Timer
        Private _lastAudioTimeTicks As Long = DateTime.MinValue.Ticks
        Private _silentBuffer As Byte() = Nothing
        Private _silentBufferSize As Integer = 0
        Private _isSendingSilence As Boolean = False
        Private _formatReadyEvent As New ManualResetEventSlim(False)

        Private _writeQueue As New ConcurrentQueue(Of Byte())()
        Private _writeThread As Thread
        Private _writeCts As CancellationTokenSource

        Private _audioBytesReceived As Long = 0
        Private _lastSilentBytesSent As Long = 0
#End Region

#Region "Properties"
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return Threading.Volatile.Read(_isRunning)
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Dim pipe As NamedPipeServerStream = _namedPipeServer
                Return pipe IsNot Nothing AndAlso pipe.IsConnected
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

        Public Property Volume As Single
            Get
                Return _volume
            End Get
            Set(value As Single)
                _volume = Math.Max(0.0F, Math.Min(1.0F, value))
                SyncLock _writeLock
                    If _micCapture IsNot Nothing Then
                        _micCapture.Volume = _volume
                    End If
                End SyncLock
            End Set
        End Property

        Public ReadOnly Property CurrentWaveFormat As WaveFormat
            Get
                SyncLock _formatLock
                    Return _waveFormat
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property FormatReadyEvent As ManualResetEventSlim
            Get
                Return _formatReadyEvent
            End Get
        End Property
#End Region

#Region "Events"
        Public Event PipeStarted As EventHandler
        Public Event PipeStopped As EventHandler
        Public Event PipeConnected As EventHandler
        Public Event PipeError As EventHandler(Of String)
#End Region

#Region "Constructor"
        Public Sub New()
            _pipeName = MIC_PIPE_PREFIX & Process.GetCurrentProcess().Id & "_" & Guid.NewGuid().ToString("N").Substring(0, 8)
        End Sub

        Public Sub New(volume As Single)
            Me.New()
            _volume = Math.Max(0.0F, Math.Min(1.0F, volume))
        End Sub
#End Region

#Region "Public Methods"
        Public Function Start() As Boolean
            SyncLock _writeLock
                If Threading.Volatile.Read(_isRunning) Then Return True

                Try
                    Debug.WriteLine("=== MicPipe.Start ===")

                    _namedPipeServer = New NamedPipeServerStream(
                        _pipeName, PipeDirection.Out, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
                        PIPE_BUFFER_SIZE, PIPE_BUFFER_SIZE
                    )

                    _micCapture = New MicCapture()
                    _micCapture.Volume = _volume
                    AddHandler _micCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                    AddHandler _micCapture.CaptureError, AddressOf OnCaptureError

                    _micCapture.StartCapture()
                    Threading.Volatile.Write(_isRunning, True)

                    Dim capturedFormat As WaveFormat = _micCapture.WaveFormat
                    If capturedFormat IsNot Nothing Then
                        SyncLock _formatLock
                            _waveFormat = capturedFormat
                        End SyncLock
                        PrepareSilentBuffer()
                        _formatReadyEvent.Set()
                    End If

                    StartSilentTimer()

                    ' Start background writer
                    _writeCts = New CancellationTokenSource()
                    _writeThread = New Thread(AddressOf WriteLoop)
                    _writeThread.IsBackground = True
                    _writeThread.Start()

                    Task.Run(Sub() WaitForConnection())

                    RaiseEvent PipeStarted(Me, EventArgs.Empty)
                    Return True

                Catch ex As Exception
                    Debug.WriteLine("MicPipe Start Error: " & ex.Message)
                    RaiseEvent PipeError(Me, "Failed to start: " & ex.Message)
                    CleanupOnFailure()
                    Return False
                End Try
            End SyncLock
        End Function

        ''' <summary>
        ''' Full stop: set flag, cancel CTS, stop timer, stop capture, close pipe.
        ''' </summary>
        Public Sub [Stop]()
            SyncLock _writeLock
                If Not Threading.Volatile.Read(_isRunning) Then Exit Sub
                Debug.WriteLine("=== MicPipe.Stop ===")
                Threading.Volatile.Write(_isRunning, False)
            End SyncLock

            StopSilentTimer()

            If _writeCts IsNot Nothing Then
                _writeCts.Cancel()
            End If

            SyncLock _writeLock
                Try
                    If _micCapture IsNot Nothing Then
                        RemoveHandler _micCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                        RemoveHandler _micCapture.CaptureError, AddressOf OnCaptureError
                        _micCapture.StopCapture()
                        _micCapture.Dispose()
                        _micCapture = Nothing
                    End If
                Catch ex As Exception
                    Debug.WriteLine("MicPipe Stop (MicCapture): " & ex.Message)
                End Try

                Try
                    If _namedPipeServer IsNot Nothing Then
                        _namedPipeServer.Dispose()
                        _namedPipeServer = Nothing
                    End If
                Catch ex As Exception
                    Debug.WriteLine("MicPipe Stop (Pipe): " & ex.Message)
                End Try
            End SyncLock

            If _writeThread IsNot Nothing AndAlso _writeThread.IsAlive Then
                If Not _writeThread.Join(3000) Then
                    Debug.WriteLine("MicPipe: Write thread did not stop in time")
                End If
            End If

            RaiseEvent PipeStopped(Me, EventArgs.Empty)
            Debug.WriteLine("MicPipe: Stopped")
        End Sub

        Public Function GetFFmpegInputArgs() As String
            If Not _formatReadyEvent.Wait(3000) Then
                Debug.WriteLine("MicPipe: WaveFormat not available after 3s, using defaults")
            End If

            SyncLock _formatLock
                If _waveFormat Is Nothing AndAlso _micCapture IsNot Nothing Then
                    _waveFormat = _micCapture.WaveFormat
                    If _waveFormat IsNot Nothing Then
                        _formatReadyEvent.Set()
                        PrepareSilentBuffer()
                    End If
                End If
            End SyncLock

            Dim format As WaveFormat = CurrentWaveFormat
            Dim formatStr As String = GetFFmpegFormat(format)
            Dim rate As Integer = If(format IsNot Nothing, format.SampleRate, 48000)
            Dim channels As Integer = If(format IsNot Nothing, format.Channels, 2)

            Return String.Format("-thread_queue_size 512 -f {0} -ar {1} -ac {2} -i ""{3}""",
                formatStr, rate, channels, PipePath)
        End Function
#End Region

#Region "Private Methods"
        Private Sub OnAudioDataAvailable(sender As Object, e As AudioDataEventArgs)
            If Not IsRunning Then Exit Sub

            Threading.Volatile.Write(_lastAudioTimeTicks, DateTime.Now.Ticks)
            If _isSendingSilence Then
                _isSendingSilence = False
            End If

            SyncLock _writeLock
                ' FIX 4: Enqueue for background writer instead of direct pipe.Write()
                _writeQueue.Enqueue(e.Buffer)
            End SyncLock

            Threading.Interlocked.Add(_audioBytesReceived, e.BytesRecorded)

            If e.WaveFormat IsNot Nothing Then
                Dim needPrepare As Boolean = False
                SyncLock _formatLock
                    If _waveFormat Is Nothing Then
                        _waveFormat = e.WaveFormat
                        needPrepare = True
                    End If
                End SyncLock
                If needPrepare Then
                    _formatReadyEvent.Set()
                    PrepareSilentBuffer()
                End If
            End If
        End Sub

        Private Sub WriteLoop()
            Try
                While Not _writeCts.IsCancellationRequested OrElse _writeQueue.Count > 0
                    ' v2.2 FIX: Check IsRunning BEFORE each write to avoid IOException on broken pipe.
                    If Not IsRunning OrElse _writeCts.IsCancellationRequested Then Exit While

                    Dim buffer As Byte() = Nothing
                    If _writeQueue.TryDequeue(buffer) Then
                        Dim pipe As NamedPipeServerStream = _namedPipeServer
                        If pipe IsNot Nothing AndAlso pipe.IsConnected Then
                            Try
                                pipe.Write(buffer, 0, buffer.Length)
                            Catch ex As System.IO.IOException
                                ' Pipe broken during stop - normal when FFmpeg disconnects
                                Exit While
                            Catch ex As ObjectDisposedException
                                ' Pipe was disposed while we were writing - normal during stop
                                Exit While
                            Catch ex As Exception
                                If IsRunning Then
                                    Debug.WriteLine("MicPipe WriteLoop Error: " & ex.Message)
                                End If
                            End Try
                        End If
                    Else
                        Thread.Sleep(2)
                    End If
                End While
            Catch ex As Threading.ThreadAbortException
                ' Thread abort during shutdown - normal
            Catch ex As Exception
                Debug.WriteLine("MicPipe WriteLoop crashed: " & ex.Message)
            End Try
        End Sub

        Private Sub WaitForConnection()
            Try
                Dim pipe As NamedPipeServerStream = _namedPipeServer
                If pipe Is Nothing Then Exit Sub
                Dim asyncResult = pipe.BeginWaitForConnection(Nothing, Nothing)
                If asyncResult.AsyncWaitHandle.WaitOne(_connectionTimeout) Then
                    pipe.EndWaitForConnection(asyncResult)
                    Debug.WriteLine("MicPipe: FFmpeg connected!")
                    RaiseEvent PipeConnected(Me, EventArgs.Empty)
                Else
                    Debug.WriteLine("MicPipe: Connection timeout")
                End If
            Catch ex As Exception
                Debug.WriteLine("MicPipe WaitForConnection Error: " & ex.Message)
            End Try
        End Sub

        Private Sub OnCaptureError(sender As Object, errorMessage As String)
            Debug.WriteLine("MicPipe: Capture Error - " & errorMessage)
            RaiseEvent PipeError(Me, errorMessage)
        End Sub

        Private Sub PrepareSilentBuffer()
            Dim format As WaveFormat = CurrentWaveFormat
            If format Is Nothing Then Exit Sub

            Dim bytesPerSample As Integer = format.BitsPerSample \ 8
            If bytesPerSample <= 0 Then bytesPerSample = 2

            Dim samplesPerInterval As Integer = CInt(format.SampleRate * SILENT_FRAME_INTERVAL_MS / 1000.0)
            _silentBufferSize = samplesPerInterval * format.Channels * bytesPerSample

            If _silentBufferSize > 0 Then
                _silentBuffer = New Byte(_silentBufferSize - 1) {}
            End If
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
            _lastAudioTimeTicks = DateTime.Now.Ticks
            _audioBytesReceived = 0
            _lastSilentBytesSent = 0
            _isSendingSilence = False
        End Sub

        Private Sub StopSilentTimer()
            If _silentTimer IsNot Nothing Then
                Try
                    _silentTimer.Stop()
                    RemoveHandler _silentTimer.Elapsed, AddressOf OnSilentTimerElapsed
                    _silentTimer.Dispose()
                Catch ex As Exception
                End Try
                _silentTimer = Nothing
            End If
        End Sub

        Private Sub OnSilentTimerElapsed(sender As Object, e As ElapsedEventArgs)
            If Not IsRunning Then Exit Sub
            Dim pipe As NamedPipeServerStream = _namedPipeServer
            If pipe Is Nothing OrElse Not pipe.IsConnected Then Exit Sub
            If _silentBuffer Is Nothing Then Exit Sub

            Try
                Dim timeSinceLastAudio As TimeSpan = DateTime.Now - New DateTime(Threading.Volatile.Read(_lastAudioTimeTicks))

                If timeSinceLastAudio.TotalMilliseconds >= SILENT_TIMEOUT_MS Then
                    If Not _isSendingSilence Then
                        _isSendingSilence = True
                    End If

                    Dim bytesToSend As Integer = CInt(Threading.Interlocked.Read(_audioBytesReceived) - Threading.Interlocked.Read(_lastSilentBytesSent))
                    Threading.Interlocked.Exchange(_lastSilentBytesSent, Threading.Interlocked.Read(_audioBytesReceived))

                    If bytesToSend > 0 Then
                        ' Build a properly-sized silent buffer and enqueue for background writer
                        Dim silentData As Byte() = New Byte(bytesToSend - 1) {}
                        Dim offset As Integer = 0
                        Do While offset < bytesToSend
                            Dim chunkSize As Integer = Math.Min(_silentBufferSize, bytesToSend - offset)
                            Array.Copy(_silentBuffer, 0, silentData, offset, chunkSize)
                            offset += chunkSize
                        Loop

                        SyncLock _writeLock
                            _writeQueue.Enqueue(silentData)
                        End SyncLock
                    End If
                End If
            Catch ex As Exception
                If IsRunning Then
                    Debug.WriteLine("MicPipe Silent Timer Error: " & ex.Message)
                End If
            End Try
        End Sub

        Private Function GetFFmpegFormat(format As WaveFormat) As String
            If format Is Nothing Then Return "s16le"
            Select Case format.BitsPerSample
                Case 16 : Return "s16le"
                Case 24 : Return "s24le"
                Case 32
                    If format.Encoding = WaveFormatEncoding.IeeeFloat Then
                        Return "f32le"
                    Else
                        Return "s32le"
                    End If
                Case Else : Return "s16le"
            End Select
        End Function

        Private Sub CleanupOnFailure()
            Try
                If _micCapture IsNot Nothing Then
                    RemoveHandler _micCapture.AudioDataAvailable, AddressOf OnAudioDataAvailable
                    RemoveHandler _micCapture.CaptureError, AddressOf OnCaptureError
                    _micCapture.StopCapture()
                    _micCapture.Dispose()
                    _micCapture = Nothing
                End If
            Catch : End Try
            Try
                If _namedPipeServer IsNot Nothing Then
                    _namedPipeServer.Dispose()
                    _namedPipeServer = Nothing
                End If
            Catch : End Try
            StopSilentTimer()
            If _writeCts IsNot Nothing Then _writeCts.Cancel()
            Threading.Volatile.Write(_isRunning, False)
        End Sub
#End Region

#Region "IDisposable"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If _isDisposed Then Exit Sub
            If disposing Then
                Try : [Stop]() : Catch : End Try
            End If
            _isDisposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace
