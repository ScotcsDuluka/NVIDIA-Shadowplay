Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NAudio.CoreAudioApi
Imports NAudio.Wave

''' <summary>
''' Zero-overhead WASAPI audio capture — writes PCM directly from WASAPI callback
''' to the output stream. NO queue, NO AudioFrame allocation, NO writer thread,
''' NO BlockingCollection, NO context switch.
'''
''' This is the same pattern as the synthetic Python test that achieved 144 FPS:
''' callback → copy → pipe.Write, all on the same thread.
''' </summary>
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

    Private _systemStream As Stream
    Private _micStream As Stream

    Private _systemFormat As AudioFormat
    Private _micFormat As AudioFormat

    Private _isRunning As Boolean = False
    Private _disposed As Boolean = False

    ' Pre-allocated buffers (reused, not allocated per callback)
    Private _sysCopyBuffer As Byte()
    Private _micCopyBuffer As Byte()

    ' Silence generator — fills audio gaps when WASAPI loopback doesn't fire
    Private _silenceThread As Thread
    Private _silenceStop As New ManualResetEvent(False)
    Private Const SilenceCheckMs As Integer = 50
    Private _lastSysCallbackTicks As Long = 0
    Private _lastMicCallbackTicks As Long = 0
    Private _silenceBuffer As Byte()

    ' Counters (atomic, but minimal — only increment, no locks)
    Private _sysSamplesWritten As Long = 0
    Private _sysBytesWritten As Long = 0
    Private _micSamplesWritten As Long = 0
    Private _micBytesWritten As Long = 0
    Private _sysCallbackCount As Long = 0
    Private _micCallbackCount As Long = 0

    Public Event SystemFormatDetected(format As AudioFormat)
    Public Event MicFormatDetected(format As AudioFormat)
    Public Event SystemStartFailed(reason As String)
    Public Event MicStartFailed(reason As String)

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

        ' Pre-allocate silence buffer: 50ms of 48kHz stereo f32le = 48000 * 0.05 * 2 * 4 = 19200 bytes
        _silenceBuffer = New Byte(19199) {}

        If _config.SystemAudioCapture Then
            StartSystemCapture()
            _lastSysCallbackTicks = Stopwatch.GetTimestamp()
        End If

        If _config.MicCapture AndAlso (Not String.IsNullOrEmpty(_config.MicDeviceId) OrElse
                                       Not String.IsNullOrEmpty(_config.MicDeviceName)) Then
            StartMicCapture()
            _lastMicCallbackTicks = Stopwatch.GetTimestamp()
        End If

        ' Start silence feeder thread — fills gaps when WASAPI loopback doesn't fire
        _silenceStop.Reset()
        _silenceThread = New Thread(AddressOf SilenceFeederLoop) With {
            .IsBackground = True,
            .Name = "AudioSilenceFeeder",
            .Priority = ThreadPriority.AboveNormal
        }
        _silenceThread.Start()
    End Sub

    Public Sub StopProducers()
        _isRunning = False

        ' Stop silence feeder
        Try : _silenceStop.Set() : Catch : End Try
        Try
            If _silenceThread IsNot Nothing AndAlso _silenceThread.IsAlive Then
                _silenceThread.Join(2000)
            End If
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
    End Sub

    Public Sub ClosePipes()
        Try
            If _systemStream IsNot Nothing Then
                _systemStream.Flush()
                _systemStream.Dispose()
                _systemStream = Nothing
            End If
        Catch
        End Try

        Try
            If _micStream IsNot Nothing AndAlso _micStream IsNot _systemStream Then
                _micStream.Flush()
                _micStream.Dispose()
                _micStream = Nothing
            End If
        Catch
        End Try
    End Sub

    Public Sub [Stop]()
        StopProducers()
        ClosePipes()
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

            ' Pre-allocate copy buffer (64KB — large enough for any WASAPI buffer)
            _sysCopyBuffer = New Byte(65535) {}

            AddHandler _systemCapture.DataAvailable, AddressOf OnSystemDataAvailable
            AddHandler _systemCapture.RecordingStopped, AddressOf OnSystemCaptureStopped
            _systemCapture.StartRecording()
        Catch ex As Exception
            RaiseEvent SystemStartFailed(ex.Message)
        End Try
    End Sub

    Private Sub StartMicCapture()
        Try
            Dim targetDev As MMDevice = FindMicDevice()
            If targetDev Is Nothing Then
                RaiseEvent MicStartFailed("Mic device not found")
                Return
            End If

            _micCapture = New WasapiCapture(targetDev)
            _micFormat = WaveFormatToInfo(_micCapture.WaveFormat)
            RaiseEvent MicFormatDetected(_micFormat)

            _micCopyBuffer = New Byte(65535) {}

            AddHandler _micCapture.DataAvailable, AddressOf OnMicDataAvailable
            AddHandler _micCapture.RecordingStopped, AddressOf OnMicCaptureStopped
            _micCapture.StartRecording()
        Catch ex As Exception
            RaiseEvent MicStartFailed(ex.Message)
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

    ''' <summary>
    ''' WASAPI callback — writes DIRECTLY to pipe, no queue, no allocation, no context switch.
    ''' This is the same pattern as the synthetic Python test that achieved 144 FPS.
    ''' </summary>
    Private Sub SilenceFeederLoop()
        Try
            While _isRunning AndAlso Not _silenceStop.WaitOne(SilenceCheckMs)
                If Not _isRunning Then Exit While

                Dim nowTicks As Long = Stopwatch.GetTimestamp()

                If _config.SystemAudioCapture AndAlso _systemStream IsNot Nothing AndAlso _systemStream.CanWrite Then
                    Dim msSinceSysCallback As Double = (nowTicks - _lastSysCallbackTicks) * 1000.0 / Stopwatch.Frequency
                    If msSinceSysCallback > SilenceCheckMs Then
                        Try
                            _systemStream.Write(_silenceBuffer, 0, _silenceBuffer.Length)
                            _sysBytesWritten += _silenceBuffer.Length
                        Catch
                        End Try
                    End If
                End If
            End While
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[NAudio] Silence feeder crashed: " & ex.Message)
        End Try
    End Sub

    Private Sub OnSystemDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        Try
            If _systemStream Is Nothing OrElse Not _systemStream.CanWrite Then Return

            _lastSysCallbackTicks = Stopwatch.GetTimestamp()

            Dim bytesToCopy As Integer = Math.Min(e.BytesRecorded, _sysCopyBuffer.Length)
            Buffer.BlockCopy(e.Buffer, 0, _sysCopyBuffer, 0, bytesToCopy)

            _systemStream.Write(_sysCopyBuffer, 0, bytesToCopy)

            _sysCallbackCount += 1
            _sysBytesWritten += bytesToCopy
            Dim bytesPerSample As Integer = (_systemFormat.BitsPerSample \ 8) * _systemFormat.Channels
            If bytesPerSample > 0 Then
                _sysSamplesWritten += bytesToCopy \ bytesPerSample
            End If
        Catch
        End Try
    End Sub

    Private Sub OnMicDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        Try
            If _micStream Is Nothing OrElse Not _micStream.CanWrite Then Return

            Dim bytesToCopy As Integer = Math.Min(e.BytesRecorded, _micCopyBuffer.Length)
            Buffer.BlockCopy(e.Buffer, 0, _micCopyBuffer, 0, bytesToCopy)

            _micStream.Write(_micCopyBuffer, 0, bytesToCopy)

            _micCallbackCount += 1
            _micBytesWritten += bytesToCopy
            Dim bytesPerSample As Integer = (_micFormat.BitsPerSample \ 8) * _micFormat.Channels
            If bytesPerSample > 0 Then
                _micSamplesWritten += bytesToCopy \ bytesPerSample
            End If
        Catch
        End Try
    End Sub

    Private Sub OnSystemCaptureStopped(sender As Object, e As StoppedEventArgs)
    End Sub

    Private Sub OnMicCaptureStopped(sender As Object, e As StoppedEventArgs)
    End Sub

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
        sb.AppendLine("[Audio] SysSamples=" & _sysSamplesWritten)
        sb.AppendLine("[Audio] SysWritten=" & _sysSamplesWritten)
        sb.AppendLine("[Audio] SysBytes=" & _sysBytesWritten)
        sb.AppendLine("[Audio] SysCallbacks=" & _sysCallbackCount)
        sb.AppendLine("[Audio] MicSamples=" & _micSamplesWritten)
        sb.AppendLine("[Audio] MicWritten=" & _micSamplesWritten)
        sb.AppendLine("[Audio] MicBytes=" & _micBytesWritten)
        sb.AppendLine("[Audio] MicCallbacks=" & _micCallbackCount)
        sb.AppendLine("[Audio] ShutdownRequested=" & If(Not _isRunning, "1", "0"))
        Return sb.ToString()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        [Stop]()
    End Sub

End Class
