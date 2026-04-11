Imports NAudio.Wave
Imports NAudio.CoreAudioApi
Imports System.Diagnostics

Namespace CaptureCore

    ''' <summary>
    ''' WASAPI Loopback Audio Capture using NAudio
    ''' Captures system audio (what you hear) without Stereo Mix or Virtual Cable
    ''' </summary>
    Public Class AudioCapture
        Implements IDisposable

#Region "Events"
        Public Event AudioDataAvailable As EventHandler(Of AudioDataEventArgs)
        Public Event CaptureStarted As EventHandler
        Public Event CaptureStopped As EventHandler
        Public Event CaptureError As EventHandler(Of String)
#End Region

#Region "Properties"
        Private _isCapturing As Boolean = False
        Private _capture As WasapiLoopbackCapture
        Private _captureLock As New Object()
        Private _volume As Single = 1.0F
        Private _device As MMDevice

        Public ReadOnly Property IsCapturing As Boolean
            Get
                Return _isCapturing
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

        Public ReadOnly Property WaveFormat As WaveFormat
            Get
                If _capture IsNot Nothing Then
                    Return _capture.WaveFormat
                End If
                Return Nothing
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
            Try
                Using enumerator As New MMDeviceEnumerator()
                    For Each device In enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                        If device.FriendlyName.Equals(deviceName, StringComparison.OrdinalIgnoreCase) Then
                            _device = device
                            Exit Sub
                        End If
                    Next
                    ' Default to default device
                    _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)
                End Using
            Catch ex As Exception
                Debug.WriteLine("SetDevice Error: " & ex.Message)
            End Try
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

                    RaiseEvent CaptureStarted(Me, EventArgs.Empty)
                    Debug.WriteLine("AudioCapture: Started - Format: " & _capture.WaveFormat.ToString())

                Catch ex As Exception
                    Debug.WriteLine("AudioCapture Start Error: " & ex.Message)
                    RaiseEvent CaptureError(Me, ex.Message)
                End Try
            End SyncLock
        End Sub

        ''' <summary>
        ''' Stop capturing
        ''' </summary>
        Public Sub StopCapture()
            SyncLock _captureLock
                If Not _isCapturing OrElse _capture Is Nothing Then Exit Sub

                Try
                    _isCapturing = False
                    _capture.StopRecording()
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture Stop Error: " & ex.Message)
                End Try
            End SyncLock
        End Sub
#End Region

#Region "Private Methods"
        Private Sub OnDataAvailable(sender As Object, e As WaveInEventArgs)
            If Not _isCapturing OrElse e.BytesRecorded <= 0 Then Exit Sub

            Try
                ' Copy buffer
                Dim buffer As Byte() = New Byte(e.BytesRecorded - 1) {}
                Array.Copy(e.Buffer, buffer, e.BytesRecorded)

                ' Apply volume if needed
                If _volume < 0.99F Then
                    buffer = ApplyVolume(buffer, _volume)
                End If

                RaiseEvent AudioDataAvailable(Me, New AudioDataEventArgs(buffer, buffer.Length, _capture.WaveFormat))

            Catch ex As Exception
                Debug.WriteLine("OnDataAvailable Error: " & ex.Message)
            End Try
        End Sub

        Private Sub OnRecordingStopped(sender As Object, e As StoppedEventArgs)
            Debug.WriteLine("AudioCapture: Recording stopped")

            If e.Exception IsNot Nothing Then
                Debug.WriteLine("AudioCapture Error: " & e.Exception.Message)
                RaiseEvent CaptureError(Me, e.Exception.Message)
            End If

            CleanupCapture()
            RaiseEvent CaptureStopped(Me, EventArgs.Empty)
        End Sub

        Private Sub CleanupCapture()
            If _capture IsNot Nothing Then
                Try
                    RemoveHandler _capture.DataAvailable, AddressOf OnDataAvailable
                    RemoveHandler _capture.RecordingStopped, AddressOf OnRecordingStopped
                    _capture.Dispose()
                Catch
                End Try
                _capture = Nothing
            End If
            _isCapturing = False
        End Sub

        Private Function ApplyVolume(buffer As Byte(), volume As Single) As Byte()
            ' WASAPI loopback usually returns 32-bit float (IEEE float)
            ' Sample size = 4 bytes per channel
            Dim sampleCount As Integer = buffer.Length \ 4

            For i As Integer = 0 To sampleCount - 1
                Dim offset As Integer = i * 4
                Dim sample As Single = BitConverter.ToSingle(buffer, offset)
                sample *= volume
                ' Clamp to prevent clipping
                sample = Math.Max(-1.0F, Math.Min(1.0F, sample))
                Dim bytes As Byte() = BitConverter.GetBytes(sample)
                buffer(offset) = bytes(0)
                buffer(offset + 1) = bytes(1)
                buffer(offset + 2) = bytes(2)
                buffer(offset + 3) = bytes(3)
            Next

            Return buffer
        End Function
#End Region

#Region "IDisposable"
        Private disposed As Boolean = False

        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposed Then Exit Sub

            If disposing Then
                Try
                    StopCapture()
                    CleanupCapture()
                    _device = Nothing
                Catch
                End Try
            End If

            disposed = True
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

End Namespace
