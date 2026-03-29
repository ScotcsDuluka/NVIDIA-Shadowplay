Imports NAudio.Wave
Imports NAudio.CoreAudioApi
Imports System.Diagnostics

Namespace CaptureCore

    ''' <summary>
    ''' WASAPI Loopback Audio Capture using NAudio
    ''' Captures system audio (what you hear) without Stereo Mix or Virtual Cable
    ''' 
    ''' v2.0 Improvements:
    '''   - ApplyVolume now handles IEEE Float (32/64-bit) AND PCM (16/24/32-bit)
    '''   - Added PeakLevel property for audio metering
    '''   - Proper MMDevice disposal on device change
    '''   - No more silent empty Catch blocks - all errors are logged
    '''   - Thread-safe WaveFormat access
    ''' </summary>
    Public Class AudioCapture
        Implements IDisposable

#Region "Events"
        Public Event AudioDataAvailable As EventHandler(Of AudioDataEventArgs)
        Public Event CaptureStarted As EventHandler
        Public Event CaptureStopped As EventHandler
        Public Event CaptureError As EventHandler(Of String)
#End Region

#Region "Fields"
        Private _isCapturing As Boolean = False
        Private _capture As WasapiLoopbackCapture
        Private _captureLock As New Object()
        Private _volume As Single = 1.0F
        Private _device As MMDevice
        Private _disposed As Boolean = False

        ' Audio level metering
        Private _peakLevel As Single = 0.0F
        Private _levelDecay As Single = 0.0F
        Private _levelLock As New Object()
        Private Const LEVEL_DECAY_RATE As Single = 0.05F
#End Region

#Region "Properties"
        Public ReadOnly Property IsCapturing As Boolean
            Get
                SyncLock _captureLock
                    Return _isCapturing
                End SyncLock
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

        ''' <summary>
        ''' Current peak audio level (0.0 - 1.0) for metering.
        ''' Thread-safe.
        ''' </summary>
        Public ReadOnly Property PeakLevel As Single
            Get
                SyncLock _levelLock
                    Return _peakLevel
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property WaveFormat As WaveFormat
            Get
                SyncLock _captureLock
                    If _capture IsNot Nothing Then
                        Return _capture.WaveFormat
                    End If
                    Return Nothing
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Get available audio output devices
        ''' </summary>
        Public Shared Function GetOutputDevices() As List(Of String)
            Dim devices As New List(Of String)()
            Try
                Using enumerator As New MMDeviceEnumerator()
                    For Each device In enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                        devices.Add(device.FriendlyName)
                    Next
                End Using
            Catch ex As Exception
                Debug.WriteLine("GetOutputDevices Error: " & ex.Message)
            End Try
            Return devices
        End Function
#End Region

#Region "Constructor"
        Public Sub New()
        End Sub

        Public Sub New(deviceName As String)
            SetDevice(deviceName)
        End Sub

        Public Sub SetDevice(deviceName As String)
            Dim wasCapturing As Boolean = False

            SyncLock _captureLock
                wasCapturing = _isCapturing

                If wasCapturing Then
                    Try
                        If _capture IsNot Nothing Then
                            _isCapturing = False
                            RemoveHandler _capture.DataAvailable, AddressOf OnDataAvailable
                            RemoveHandler _capture.RecordingStopped, AddressOf OnRecordingStopped
                            _capture.StopRecording()
                            _capture.Dispose()
                            _capture = Nothing
                        End If
                    Catch ex As Exception
                        Debug.WriteLine("SetDevice: Error stopping capture - " & ex.Message)
                    End Try
                End If

                Try
                    ' Dispose old device if switching
                    If _device IsNot Nothing Then
                        Try : _device.Dispose() : Catch : End Try
                        _device = Nothing
                    End If

                    Using enumerator As New MMDeviceEnumerator()
                        For Each device In enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                            If device.FriendlyName.Equals(deviceName, StringComparison.OrdinalIgnoreCase) Then
                                _device = device
                                Exit Try
                            End If
                        Next
                        ' Default to default device
                        _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("SetDevice Error: " & ex.Message)
                End Try
            End SyncLock

            ' Auto-restart if was capturing
            If wasCapturing Then
                StartCapture()
            End If
        End Sub
#End Region

#Region "Public Methods"
        ''' <summary>
        ''' Start capturing system audio
        ''' </summary>
        Public Sub StartCapture()
            SyncLock _captureLock
                If _isCapturing Then Exit Sub

                Try
                    ' Create WASAPI loopback capture
                    If _device IsNot Nothing Then
                        _capture = New WasapiLoopbackCapture(_device)
                    Else
                        _capture = New WasapiLoopbackCapture()
                    End If

                    AddHandler _capture.DataAvailable, AddressOf OnDataAvailable
                    AddHandler _capture.RecordingStopped, AddressOf OnRecordingStopped

                    _capture.StartRecording()
                    _isCapturing = True

                    ' Reset level meter
                    SyncLock _levelLock
                        _peakLevel = 0.0F
                        _levelDecay = 0.0F
                    End SyncLock

                    RaiseEvent CaptureStarted(Me, EventArgs.Empty)
                    Debug.WriteLine("AudioCapture: Started - Format: " & _capture.WaveFormat.ToString())

                Catch ex As Exception
                    Debug.WriteLine("AudioCapture Start Error: " & ex.Message)
                    CleanupCapture()
                    RaiseEvent CaptureError(Me, "Failed to start capture: " & ex.Message)
                End Try
            End SyncLock
        End Sub

        ''' <summary>
        ''' Stop capturing
        ''' </summary>
        Public Sub StopCapture()
            Dim captureToStop As WasapiLoopbackCapture = Nothing

            SyncLock _captureLock
                If Not _isCapturing Then Exit Sub

                Try
                    _isCapturing = False
                    captureToStop = _capture
                    _capture = Nothing
                Catch ex As Exception
                    Debug.WriteLine("StopCapture Error: " & ex.Message)
                    RaiseEvent CaptureError(Me, "Failed to stop capture: " & ex.Message)
                    Exit Sub
                End Try
            End SyncLock

            ' Cleanup outside lock to prevent deadlock with callbacks
            If captureToStop IsNot Nothing Then
                Try
                    RemoveHandler captureToStop.DataAvailable, AddressOf OnDataAvailable
                    RemoveHandler captureToStop.RecordingStopped, AddressOf OnRecordingStopped
                Catch ex As Exception
                    Debug.WriteLine("StopCapture: Error removing handlers - " & ex.Message)
                End Try

                Try
                    captureToStop.StopRecording()
                Catch ex As Exception
                    Debug.WriteLine("StopCapture: Error stopping - " & ex.Message)
                End Try

                Try
                    captureToStop.Dispose()
                Catch ex As Exception
                    Debug.WriteLine("StopCapture: Error disposing - " & ex.Message)
                End Try
            End If

            SyncLock _levelLock
                _peakLevel = 0.0F
                _levelDecay = 0.0F
            End SyncLock

            RaiseEvent CaptureStopped(Me, EventArgs.Empty)
        End Sub
#End Region

#Region "Private Methods"
        Private Sub OnDataAvailable(sender As Object, e As WaveInEventArgs)
            If Not Threading.Volatile.Read(_isCapturing) Then Exit Sub
            If e.BytesRecorded <= 0 Then Exit Sub

            Try
                ' Snapshot capture reference to prevent NullReferenceException during Dispose
                Dim capture As WasapiLoopbackCapture = _capture
                Dim format As WaveFormat = If(capture IsNot Nothing, capture.WaveFormat, Nothing)
                If capture Is Nothing OrElse format Is Nothing Then Exit Sub

                ' Copy buffer
                Dim buffer As Byte() = New Byte(e.BytesRecorded - 1) {}
                Array.Copy(e.Buffer, buffer, e.BytesRecorded)

                ' Apply volume if needed
                If _volume < 0.99F Then
                    buffer = ApplyVolume(buffer, format, _volume)
                End If

                ' Update audio level meter
                UpdatePeakLevel(buffer, format)

                RaiseEvent AudioDataAvailable(Me, New AudioDataEventArgs(buffer, buffer.Length, format))

            Catch ex As Exception
                Debug.WriteLine("OnDataAvailable Error: " & ex.Message)
            End Try
        End Sub

        Private Sub OnRecordingStopped(sender As Object, e As StoppedEventArgs)
            Debug.WriteLine("AudioCapture: Recording stopped")
            If e.Exception IsNot Nothing Then
                Debug.WriteLine("AudioCapture Error: " & e.Exception.Message)
                RaiseEvent CaptureError(Me, "Capture stopped with error: " & e.Exception.Message)
            End If
            ' Note: Cleanup is handled by StopCapture, not here
        End Sub

        Private Sub CleanupCapture()
            ' Sets _capture to Nothing if not already done by StopCapture
            SyncLock _captureLock
                If _capture IsNot Nothing Then
                    Try
                        RemoveHandler _capture.DataAvailable, AddressOf OnDataAvailable
                        RemoveHandler _capture.RecordingStopped, AddressOf OnRecordingStopped
                    Catch ex As Exception
                    End Try
                    Try
                        _capture.Dispose()
                    Catch ex As Exception
                    End Try
                    _capture = Nothing
                End If
            End SyncLock
            _isCapturing = False
        End Sub

        ''' <summary>
        ''' Apply volume to audio buffer.
        ''' v2.0: Now properly handles IEEE Float (32/64-bit) AND PCM (16/24/32-bit int).
        ''' Original version assumed all data was 32-bit float (divide by 4) which was incorrect
        ''' for PCM formats.
        ''' </summary>
        Private Function ApplyVolume(buffer As Byte(), format As WaveFormat, volume As Single) As Byte()
            If format Is Nothing Then Return buffer

            Try
                Select Case format.Encoding
                    Case WaveFormatEncoding.IeeeFloat
                        buffer = ApplyVolumeFloat(buffer, format.BitsPerSample, volume)

                    Case WaveFormatEncoding.Pcm
                        buffer = ApplyVolumePCM(buffer, format.BitsPerSample, volume)

                    Case Else
                        ' Unknown format - pass through without modification
                        Debug.WriteLine(String.Format("AudioCapture: Unsupported format for volume: {0}, passing through", format.Encoding.ToString()))
                End Select
            Catch ex As Exception
                Debug.WriteLine("ApplyVolume Error: " & ex.Message)
            End Try

            Return buffer
        End Function

        ''' <summary>
        ''' Apply volume to IEEE Float audio (32-bit or 64-bit)
        ''' </summary>
        Private Function ApplyVolumeFloat(buffer As Byte(), bitsPerSample As Integer, volume As Single) As Byte()
            If bitsPerSample = 32 Then
                Return ApplyVolumeFloat32Fast(buffer, volume)
            ElseIf bitsPerSample = 64 Then
                Return ApplyVolumeFloat64(buffer, volume)
            End If
            Return buffer
        End Function

        ''' <summary>
        ''' Fast 32-bit float volume using BitConverter (compatible with all VB.NET versions)
        ''' </summary>
        Private Function ApplyVolumeFloat32Fast(buffer As Byte(), volume As Single) As Byte()
            If buffer.Length Mod 4 <> 0 Then Return buffer

            Dim sampleCount As Integer = buffer.Length \ 4
            Dim sample As Single
            Dim bytes As Byte()

            For i As Integer = 0 To sampleCount - 1
                Dim offset As Integer = i * 4
                sample = BitConverter.ToSingle(buffer, offset)
                sample *= volume
                If sample > 1.0F Then sample = 1.0F Else If sample < -1.0F Then sample = -1.0F
                bytes = BitConverter.GetBytes(sample)
                buffer(offset) = bytes(0)
                buffer(offset + 1) = bytes(1)
                buffer(offset + 2) = bytes(2)
                buffer(offset + 3) = bytes(3)
            Next

            Return buffer
        End Function

        ''' <summary>
        ''' Apply volume to 64-bit float audio (BitConverter fallback)
        ''' </summary>
        Private Function ApplyVolumeFloat64(buffer As Byte(), volume As Single) As Byte()
            If buffer.Length Mod 8 <> 0 Then
                Debug.WriteLine("AudioCapture: Float64 buffer size not aligned, skipping volume")
                Return buffer
            End If

            Dim sampleCount As Integer = buffer.Length \ 8
            Dim sample As Double
            Dim bytes As Byte()
            For i As Integer = 0 To sampleCount - 1
                Dim offset As Integer = i * 8
                sample = BitConverter.ToDouble(buffer, offset)
                sample *= volume
                sample = Math.Max(-1.0, Math.Min(1.0, sample))
                bytes = BitConverter.GetBytes(sample)
                Array.Copy(bytes, 0, buffer, offset, 8)
            Next

            Return buffer
        End Function

        ''' <summary>
        ''' Apply volume to PCM audio (16-bit, 24-bit, or 32-bit integer)
        ''' </summary>
        Private Function ApplyVolumePCM(buffer As Byte(), bitsPerSample As Integer, volume As Single) As Byte()
            Select Case bitsPerSample
                Case 16
                    ' 16-bit PCM - most common PCM format
                    If buffer.Length Mod 2 <> 0 Then Return buffer

                    Dim sampleCount As Integer = buffer.Length \ 2
                    Dim sample As Short
                    Dim scaled As Double
                    Dim bytes As Byte()
                    For i As Integer = 0 To sampleCount - 1
                        Dim offset As Integer = i * 2
                        sample = BitConverter.ToInt16(buffer, offset)
                        scaled = CDbl(sample) * volume
                        scaled = Math.Max(Short.MinValue, Math.Min(Short.MaxValue, scaled))
                        bytes = BitConverter.GetBytes(CShort(scaled))
                        buffer(offset) = bytes(0)
                        buffer(offset + 1) = bytes(1)
                    Next

                Case 24
                    ' 24-bit PCM
                    If buffer.Length Mod 3 <> 0 Then Return buffer

                    Dim sampleCount As Integer = buffer.Length \ 3
                    For i As Integer = 0 To sampleCount - 1
                        Dim offset As Integer = i * 3
                        ' Read 24-bit value (little-endian, signed)
                        Dim raw As Integer = CInt(buffer(offset)) Or (CInt(buffer(offset + 1)) << 8) Or (CInt(buffer(offset + 2)) << 16)
                        If raw >= &H800000 Then raw -= &H1000000

                        Dim scaled As Double = CDbl(raw) * volume
                        scaled = Math.Max(-8388608.0, Math.Min(8388607.0, scaled))
                        Dim val As Integer = CInt(scaled)

                        buffer(offset) = CByte(val And &HFF)
                        buffer(offset + 1) = CByte((val >> 8) And &HFF)
                        buffer(offset + 2) = CByte((val >> 16) And &HFF)
                    Next

                Case 32
                    ' 32-bit PCM integer
                    If buffer.Length Mod 4 <> 0 Then Return buffer

                    Dim sampleCount As Integer = buffer.Length \ 4
                    Dim sample As Integer
                    Dim scaled As Double
                    Dim bytes As Byte()
                    For i As Integer = 0 To sampleCount - 1
                        Dim offset As Integer = i * 4
                        sample = BitConverter.ToInt32(buffer, offset)
                        scaled = CDbl(sample) * volume
                        scaled = Math.Max(Integer.MinValue, Math.Min(Integer.MaxValue, scaled))
                        bytes = BitConverter.GetBytes(CInt(scaled))
                        Array.Copy(bytes, 0, buffer, offset, 4)
                    Next

                Case Else
                    Debug.WriteLine(String.Format("AudioCapture: Unsupported PCM bit depth: {0}", bitsPerSample))
            End Select

            Return buffer
        End Function

        ''' <summary>
        ''' Update peak audio level for metering (only processes IEEE Float 32-bit for speed).
        ''' Called from OnDataAvailable on the capture thread.
        ''' </summary>
        Private Sub UpdatePeakLevel(buffer As Byte(), format As WaveFormat)
            If format Is Nothing Then Exit Sub

            Dim floatSize As Integer = 4

            ' Only compute level for 32-bit float (most common WASAPI format)
            If format.Encoding <> WaveFormatEncoding.IeeeFloat OrElse format.BitsPerSample <> 32 Then
                Exit Sub
            End If

            If buffer.Length Mod floatSize <> 0 Then Exit Sub

            Dim sampleCount As Integer = buffer.Length \ floatSize
            Dim maxVal As Single = 0.0F

            ' Sample every Nth value for performance (check ~100 samples max)
            Dim sampleStep As Integer = Math.Max(1, sampleCount \ 100)

            For i As Integer = 0 To sampleCount - 1 Step sampleStep
                Dim offset As Integer = i * floatSize
                Dim sample As Single = BitConverter.ToSingle(buffer, offset)
                Dim absVal As Single = Math.Abs(sample)
                If absVal > maxVal Then maxVal = absVal
            Next

            SyncLock _levelLock
                If maxVal > _peakLevel Then
                    _peakLevel = maxVal
                    _levelDecay = 0.0F
                Else
                    ' Gradual decay
                    _levelDecay += LEVEL_DECAY_RATE
                    _peakLevel = CSng(maxVal + (_peakLevel - maxVal) * Math.Exp(-_levelDecay * 3.0F))
                End If
                ' Clamp to valid range
                If _peakLevel < 0.0F Then _peakLevel = 0.0F
                If _peakLevel > 1.0F Then _peakLevel = 1.0F
            End SyncLock
        End Sub

#End Region

#Region "IDisposable"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If _disposed Then Exit Sub

            If disposing Then
                Try
                    StopCapture()
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture.Dispose (StopCapture): " & ex.Message)
                End Try

                Try
                    CleanupCapture()
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture.Dispose (Cleanup): " & ex.Message)
                End Try

                Try
                    If _device IsNot Nothing Then
                        _device.Dispose()
                        _device = Nothing
                    End If
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture.Dispose (Device): " & ex.Message)
                End Try
            End If

            _disposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
#End Region

    End Class

    ''' <summary>
    ''' Event args for audio data
    ''' </summary>
    Public Class AudioDataEventArgs
        Inherits EventArgs

        Public Property Buffer As Byte()
        Public Property BytesRecorded As Integer
        Public Property WaveFormat As WaveFormat

        Public Sub New(buffer As Byte(), bytesRecorded As Integer, format As WaveFormat)
            Me.Buffer = buffer
            Me.BytesRecorded = bytesRecorded
            Me.WaveFormat = format
        End Sub
    End Class

    ''' <summary>
    ''' Microphone Audio Capture using NAudio WaveIn
    ''' Captures microphone input with consistent latency matching AudioCapture
    ''' 
    ''' v1.0: Uses WaveIn API for microphone capture (replaces DirectShow approach)
    ''' </summary>
    Public Class MicCapture
        Implements IDisposable

#Region "Events"
        Public Event AudioDataAvailable As EventHandler(Of AudioDataEventArgs)
        Public Event CaptureStarted As EventHandler
        Public Event CaptureStopped As EventHandler
        Public Event CaptureError As EventHandler(Of String)
#End Region

#Region "Fields"
        Private _isCapturing As Boolean = False
        Private _waveIn As WaveIn
        Private _captureLock As New Object()
        Private _volume As Single = 1.0F
        Private _disposed As Boolean = False
        Private _deviceNumber As Integer = -1
        Private _waveFormat As WaveFormat

        ' Audio level metering
        Private _peakLevel As Single = 0.0F
        Private _levelDecay As Single = 0.0F
        Private _levelLock As New Object()
        Private Const LEVEL_DECAY_RATE As Single = 0.05F
#End Region

#Region "Properties"
        Public ReadOnly Property IsCapturing As Boolean
            Get
                SyncLock _captureLock
                    Return _isCapturing
                End SyncLock
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

        Public ReadOnly Property PeakLevel As Single
            Get
                SyncLock _levelLock
                    Return _peakLevel
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property WaveFormat As WaveFormat
            Get
                SyncLock _captureLock
                    Return _waveFormat
                End SyncLock
            End Get
        End Property
#End Region

#Region "Shared Methods"
        ''' <summary>
        ''' Get available microphone input devices
        ''' </summary>
        Public Shared Function GetInputDevices() As List(Of String)
            Dim devices As New List(Of String)()
            Try
                Dim deviceCount As Integer = WaveIn.DeviceCount
                For i As Integer = 0 To deviceCount - 1
                    Dim caps As WaveInCapabilities = WaveIn.GetCapabilities(i)
                    devices.Add(caps.ProductName)
                Next
            Catch ex As Exception
                Debug.WriteLine("GetInputDevices Error: " & ex.Message)
            End Try
            Return devices
        End Function
#End Region

#Region "Constructor"
        Public Sub New()
        End Sub

        Public Sub New(deviceName As String)
            SetDevice(deviceName)
        End Sub
#End Region

#Region "Public Methods"
        Public Sub SetDevice(deviceName As String)
            SyncLock _captureLock
                Try
                    Dim deviceCount As Integer = WaveIn.DeviceCount
                    For i As Integer = 0 To deviceCount - 1
                        Dim caps As WaveInCapabilities = WaveIn.GetCapabilities(i)
                        If caps.ProductName.Equals(deviceName, StringComparison.OrdinalIgnoreCase) Then
                            _deviceNumber = i
                            Exit Sub
                        End If
                    Next
                    _deviceNumber = -1
                Catch ex As Exception
                    Debug.WriteLine("MicCapture.SetDevice Error: " & ex.Message)
                End Try
            End SyncLock
        End Sub

        Public Sub StartCapture()
            SyncLock _captureLock
                If _isCapturing Then Exit Sub

                Try
                    _waveFormat = New WaveFormat(48000, 16, 2)

                    If _deviceNumber >= 0 Then
                        _waveIn = New WaveIn()
                        _waveIn.DeviceNumber = _deviceNumber
                    Else
                        _waveIn = New WaveIn()
                    End If

                    _waveIn.WaveFormat = _waveFormat
                    AddHandler _waveIn.DataAvailable, AddressOf OnDataAvailable
                    AddHandler _waveIn.RecordingStopped, AddressOf OnRecordingStopped

                    _waveIn.StartRecording()
                    _isCapturing = True

                    SyncLock _levelLock
                        _peakLevel = 0.0F
                        _levelDecay = 0.0F
                    End SyncLock

                    RaiseEvent CaptureStarted(Me, EventArgs.Empty)
                    Debug.WriteLine("MicCapture: Started - Format: " & _waveFormat.ToString())

                Catch ex As Exception
                    Debug.WriteLine("MicCapture Start Error: " & ex.Message)
                    CleanupCapture()
                    RaiseEvent CaptureError(Me, "Failed to start capture: " & ex.Message)
                End Try
            End SyncLock
        End Sub

        Public Sub StopCapture()
            Dim waveInToStop As WaveIn = Nothing

            SyncLock _captureLock
                If Not _isCapturing Then Exit Sub
                Try
                    _isCapturing = False
                    waveInToStop = _waveIn
                    _waveIn = Nothing
                Catch ex As Exception
                    Debug.WriteLine("MicCapture StopCapture Error: " & ex.Message)
                    RaiseEvent CaptureError(Me, "Failed to stop capture: " & ex.Message)
                End Try
            End SyncLock

            If waveInToStop IsNot Nothing Then
                Try
                    RemoveHandler waveInToStop.DataAvailable, AddressOf OnDataAvailable
                    RemoveHandler waveInToStop.RecordingStopped, AddressOf OnRecordingStopped
                Catch ex As Exception
                End Try
                Try
                    waveInToStop.StopRecording()
                Catch ex As Exception
                End Try
                Try
                    waveInToStop.Dispose()
                Catch ex As Exception
                End Try
            End If

            SyncLock _levelLock
                _peakLevel = 0.0F
            End SyncLock

            RaiseEvent CaptureStopped(Me, EventArgs.Empty)
        End Sub
#End Region

#Region "Private Methods"
        Private Sub OnDataAvailable(sender As Object, e As WaveInEventArgs)
            If Not Threading.Volatile.Read(_isCapturing) Then Exit Sub
            If e.BytesRecorded <= 0 Then Exit Sub

            Try
                Dim buffer As Byte() = New Byte(e.BytesRecorded - 1) {}
                Array.Copy(e.Buffer, buffer, e.BytesRecorded)

                If _volume < 0.99F Then
                    buffer = ApplyVolume16BitPCM(buffer, _volume)
                End If

                UpdatePeakLevel(buffer)

                RaiseEvent AudioDataAvailable(Me, New AudioDataEventArgs(buffer, buffer.Length, _waveFormat))
            Catch ex As Exception
                Debug.WriteLine("MicCapture.OnDataAvailable Error: " & ex.Message)
            End Try
        End Sub

        Private Sub OnRecordingStopped(sender As Object, e As StoppedEventArgs)
            Debug.WriteLine("MicCapture: Recording stopped")
            If e.Exception IsNot Nothing Then
                Debug.WriteLine("MicCapture Error: " & e.Exception.Message)
                RaiseEvent CaptureError(Me, "Mic stopped with error: " & e.Exception.Message)
            End If
        End Sub

        Private Function ApplyVolume16BitPCM(buffer As Byte(), volume As Single) As Byte()
            If buffer.Length Mod 2 <> 0 Then Return buffer
            Dim sampleCount As Integer = buffer.Length \ 2
            Dim sample As Short
            Dim scaled As Double
            Dim bytes As Byte()

            For i As Integer = 0 To sampleCount - 1
                Dim offset As Integer = i * 2
                sample = BitConverter.ToInt16(buffer, offset)
                scaled = CDbl(sample) * volume
                If scaled > Short.MaxValue Then scaled = Short.MaxValue
                If scaled < Short.MinValue Then scaled = Short.MinValue
                bytes = BitConverter.GetBytes(CShort(scaled))
                buffer(offset) = bytes(0)
                buffer(offset + 1) = bytes(1)
            Next

            Return buffer
        End Function

        Private Sub UpdatePeakLevel(buffer As Byte())
            If buffer.Length Mod 2 <> 0 Then Exit Sub
            Dim sampleCount As Integer = buffer.Length \ 2
            Dim maxVal As Single = 0.0F
            Dim sampleStep As Integer = Math.Max(1, sampleCount \ 100)

            For i As Integer = 0 To sampleCount - 1 Step sampleStep
                Dim offset As Integer = i * 2
                Dim sample As Short = BitConverter.ToInt16(buffer, offset)
                Dim absVal As Single = CSng(Math.Abs(sample)) / CSng(Short.MaxValue)
                If absVal > maxVal Then maxVal = absVal
            Next

            SyncLock _levelLock
                If maxVal > _peakLevel Then
                    _peakLevel = maxVal
                    _levelDecay = 0.0F
                Else
                    _levelDecay += LEVEL_DECAY_RATE
                    _peakLevel = CSng(maxVal + (_peakLevel - maxVal) * Math.Exp(-_levelDecay * 3.0F))
                End If
                If _peakLevel < 0.0F Then _peakLevel = 0.0F
                If _peakLevel > 1.0F Then _peakLevel = 1.0F
            End SyncLock
        End Sub

        Private Sub CleanupCapture()
            SyncLock _captureLock
                If _waveIn IsNot Nothing Then
                    Try
                        RemoveHandler _waveIn.DataAvailable, AddressOf OnDataAvailable
                        RemoveHandler _waveIn.RecordingStopped, AddressOf OnRecordingStopped
                    Catch ex As Exception
                    End Try
                    Try
                        _waveIn.Dispose()
                    Catch ex As Exception
                    End Try
                    _waveIn = Nothing
                End If
            End SyncLock
            _isCapturing = False
        End Sub
#End Region

#Region "IDisposable"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If _disposed Then Exit Sub
            If disposing Then
                Try : StopCapture() : Catch : End Try
                Try : CleanupCapture() : Catch : End Try
            End If
            _disposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
#End Region

    End Class

End Namespace
