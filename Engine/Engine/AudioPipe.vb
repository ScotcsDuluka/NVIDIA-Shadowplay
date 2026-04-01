Imports NAudio.Wave
Imports NAudio.CoreAudioApi
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Timers

Namespace CaptureCore

    ''' <summary>
    ''' Audio Pipe - Streams system audio to FFmpeg via Named Pipe
    ''' 
    ''' ✅ Optimized Silent Frame Generator:
    '''    - Only sends silence when NO audio for 200ms
    '''    - Uses 100ms intervals (not 20ms - too fast!)
    '''    - Stops immediately when real audio arrives
    ''' </summary>
    Public Class AudioPipe
        Implements IDisposable

#Region "Constants"
        Private Const PIPE_BUFFER_SIZE As Integer = 2 * 1024 * 1024  ' 2MB buffer
        Private Const AUDIO_PIPE_PREFIX As String = "ShadowPlay_Audio_"

        ' ═══════════════════════════════════════════════════════════════════════
        ' ✅ Silent Frame Settings (Optimized)
        ' ═══════════════════════════════════════════════════════════════════════
        Private Const SILENT_FRAME_INTERVAL_MS As Integer = 100  ' Check every 100ms (was 20ms)
        Private Const SILENT_TIMEOUT_MS As Integer = 200         ' Start silence after 200ms no audio
#End Region

#Region "Private Fields"
        Private _audioCapture As AudioCapture
        Private _namedPipeServer As NamedPipeServerStream
        Private _pipeName As String
        Private _isRunning As Boolean = False
        Private _isDisposed As Boolean = False
        Private _writeLock As New Object()
        Private _waveFormat As WaveFormat
        Private _volume As Single = 1.0F
        Private _connectionTimeout As Integer = 5000

        ' Silent Frame Generator
        Private _silentTimer As System.Timers.Timer
        Private _lastAudioTime As DateTime = DateTime.MinValue
        Private _silentBuffer As Byte() = Nothing
        Private _silentBufferSize As Integer = 0
        Private _isSendingSilence As Boolean = False
#End Region

#Region "Properties"
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return _isRunning
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _namedPipeServer IsNot Nothing AndAlso _namedPipeServer.IsConnected
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
                _isRunning = True

                ' Get wave format
                If _audioCapture.WaveFormat IsNot Nothing Then
                    _waveFormat = _audioCapture.WaveFormat
                    Debug.WriteLine("AudioPipe: Format from AudioCapture: " & _waveFormat.ToString())
                    PrepareSilentBuffer()
                End If

                ' Start silent timer
                StartSilentTimer()

                ' Wait for FFmpeg connection
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
                ' Stop timer first
                StopSilentTimer()

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

                ' Close pipe
                If _namedPipeServer IsNot Nothing Then
                    Try
                        If _namedPipeServer.IsConnected Then
                            _namedPipeServer.WaitForPipeDrain()
                        End If
                        _namedPipeServer.Dispose()
                    Catch
                    End Try
                    _namedPipeServer = Nothing
                End If

                RaiseEvent PipeStopped(Me, EventArgs.Empty)
                Debug.WriteLine("AudioPipe: Stopped")

            Catch ex As Exception
                Debug.WriteLine("AudioPipe Stop Error: " & ex.Message)
            End Try
        End Sub

        Public Function GetFFmpegInputArgs() As String
            ' Get format from AudioCapture if available
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

#Region "Silent Frame Generator"

        Private Sub PrepareSilentBuffer()
            If _waveFormat Is Nothing Then Exit Sub

            ' Calculate buffer size for SILENT_FRAME_INTERVAL_MS of audio
            Dim bytesPerSample As Integer = 4 ' 32-bit float
            If _waveFormat.Encoding <> WaveFormatEncoding.IeeeFloat Then
                bytesPerSample = _waveFormat.BitsPerSample \ 8
            End If

            Dim samplesPerInterval As Integer = CInt(_waveFormat.SampleRate * SILENT_FRAME_INTERVAL_MS / 1000.0)
            _silentBufferSize = samplesPerInterval * _waveFormat.Channels * bytesPerSample
            _silentBuffer = New Byte(_silentBufferSize - 1) {}  ' All zeros = silence

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

            _lastAudioTime = DateTime.Now
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
        End Sub

        Private Sub OnSilentTimerElapsed(sender As Object, e As ElapsedEventArgs)
            If Not _isRunning Then Exit Sub
            If _namedPipeServer Is Nothing OrElse Not _namedPipeServer.IsConnected Then Exit Sub
            If _silentBuffer Is Nothing Then Exit Sub

            Try
                Dim timeSinceLastAudio As TimeSpan = DateTime.Now - _lastAudioTime

                ' ═══════════════════════════════════════════════════════════════════════
                ' ✅ Only send silence if:
                '    1. No real audio for SILENT_TIMEOUT_MS
                '    2. We're not already sending silence (avoid duplicate)
                ' ═══════════════════════════════════════════════════════════════════════
                If timeSinceLastAudio.TotalMilliseconds >= SILENT_TIMEOUT_MS Then
                    If Not _isSendingSilence Then
                        _isSendingSilence = True
                        Debug.WriteLine("AudioPipe: Started sending silent frames")
                    End If

                    SyncLock _writeLock
                        _namedPipeServer.Write(_silentBuffer, 0, _silentBufferSize)
                    End SyncLock
                End If

            Catch ex As Exception
                If _isRunning Then
                    Debug.WriteLine("AudioPipe Silent Timer Error: " & ex.Message)
                End If
            End Try
        End Sub

#End Region

#Region "Private Methods"
        Private Sub WaitForConnection()
            Try
                Dim asyncResult = _namedPipeServer.BeginWaitForConnection(Nothing, Nothing)

                If asyncResult.AsyncWaitHandle.WaitOne(_connectionTimeout) Then
                    _namedPipeServer.EndWaitForConnection(asyncResult)
                    Debug.WriteLine("AudioPipe: FFmpeg connected!")
                    RaiseEvent PipeConnected(Me, EventArgs.Empty)
                Else
                    Debug.WriteLine("AudioPipe: Connection timeout - continuing anyway")
                End If
            Catch ex As Exception
                Debug.WriteLine("AudioPipe WaitForConnection Error: " & ex.Message)
            End Try
        End Sub

        Private Sub OnAudioDataAvailable(sender As Object, e As AudioDataEventArgs)
            If Not _isRunning OrElse _namedPipeServer Is Nothing Then Exit Sub
            If Not _namedPipeServer.IsConnected Then Exit Sub

            Try
                ' ═══════════════════════════════════════════════════════════════════════
                ' ✅ Real audio received - update timestamp and stop silence flag
                ' ═══════════════════════════════════════════════════════════════════════
                _lastAudioTime = DateTime.Now
                If _isSendingSilence Then
                    _isSendingSilence = False
                    Debug.WriteLine("AudioPipe: Real audio received - stopped silent frames")
                End If

                SyncLock _writeLock
                    _namedPipeServer.Write(e.Buffer, 0, e.BytesRecorded)
                End SyncLock

                ' Store format
                If _waveFormat Is Nothing AndAlso e.WaveFormat IsNot Nothing Then
                    _waveFormat = e.WaveFormat
                    Debug.WriteLine("AudioPipe: Format = " & _waveFormat.ToString())
                    PrepareSilentBuffer()
                End If

            Catch ex As Exception
                If _isRunning Then
                    Debug.WriteLine("AudioPipe Write Error: " & ex.Message)
                End If
            End Try
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
