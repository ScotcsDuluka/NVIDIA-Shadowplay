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
        Public Property MicDeviceName As String = ""
    End Class

    Private _config As AudioConfigValues
    Private _systemCapture As WasapiLoopbackCapture
    Private _micCapture As WasapiCapture
    Private _pipeStream As Stream
    Private _pipeLock As New Object()
    Private _isRunning As Boolean = False
    Private _disposed As Boolean = False

    Public Sub New(config As AudioConfigValues)
        _config = config
    End Sub

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _isRunning
        End Get
    End Property

    Public Function ExpectedSampleRate() As Integer
        Try
            Using dev As New MMDeviceEnumerator()
                Dim defaultOut As MMDevice = dev.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                If defaultOut IsNot Nothing Then
                    Dim wfx = defaultOut.AudioClient.MixFormat
                    If wfx IsNot Nothing Then Return wfx.SampleRate
                End If
            End Using
        Catch
        End Try
        Return 48000
    End Function

    Public Function ExpectedChannels() As Integer
        Try
            Using dev As New MMDeviceEnumerator()
                Dim defaultOut As MMDevice = dev.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                If defaultOut IsNot Nothing Then
                    Dim wfx = defaultOut.AudioClient.MixFormat
                    If wfx IsNot Nothing Then Return wfx.Channels
                End If
            End Using
        Catch
        End Try
        Return 2
    End Function

    Public Sub Start(pipeStream As Stream)
        If _disposed Then Throw New ObjectDisposedException(NameOf(NAudioCaptureEngine))
        If _isRunning Then Return

        _pipeStream = pipeStream
        _isRunning = True

        If _config.SystemAudioCapture Then
            StartSystemCapture()
        End If

        If _config.MicCapture AndAlso Not String.IsNullOrEmpty(_config.MicDeviceName) Then
            StartMicCapture()
        End If
    End Sub

    Public Sub [Stop]()
        _isRunning = False
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

        _pipeStream = Nothing
    End Sub

    Private Sub StartSystemCapture()
        Try
            Using devEnum As New MMDeviceEnumerator()
                Dim defaultOut As MMDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                If defaultOut Is Nothing Then Return
                _systemCapture = New WasapiLoopbackCapture(defaultOut)
            End Using

            AddHandler _systemCapture.DataAvailable, AddressOf OnSystemDataAvailable
            AddHandler _systemCapture.RecordingStopped, AddressOf OnSystemCaptureStopped
            _systemCapture.StartRecording()
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[NAudio] System capture start failed: " & ex.Message)
        End Sub
    End Sub

    Private Sub StartMicCapture()
        Try
            Dim targetDev As MMDevice = Nothing
            Using devEnum As New MMDeviceEnumerator()
                For Each dev As MMDevice In devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                    If dev.FriendlyName = _config.MicDeviceName Then
                        targetDev = dev
                        Exit For
                    End If
                Next
                If targetDev Is Nothing Then
                    For Each dev As MMDevice In devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                        If dev.FriendlyName.IndexOf(_config.MicDeviceName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                            targetDev = dev
                            Exit For
                        End If
                    Next
                End If
            End Using
            If targetDev Is Nothing Then
                System.Diagnostics.Debug.WriteLine("[NAudio] Mic device not found: " & _config.MicDeviceName)
                Return
            End If

            _micCapture = New WasapiCapture(targetDev)
            AddHandler _micCapture.DataAvailable, AddressOf OnMicDataAvailable
            AddHandler _micCapture.RecordingStopped, AddressOf OnMicCaptureStopped
            _micCapture.StartRecording()
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("[NAudio] Mic capture start failed: " & ex.Message)
        End Sub
    End Sub

    Private Sub OnSystemDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        SyncLock _pipeLock
            Try
                If _pipeStream IsNot Nothing AndAlso _pipeStream.CanWrite Then
                    If _config.SystemAudioVolume >= 0.999F Then
                        _pipeStream.Write(e.Buffer, 0, e.BytesRecorded)
                    Else
                        Dim adjusted As Byte() = ApplyVolume16(e.Buffer, e.BytesRecorded, _config.SystemAudioVolume)
                        _pipeStream.Write(adjusted, 0, adjusted.Length)
                    End If
                    _pipeStream.Flush()
                End If
            Catch
            End Try
        End SyncLock
    End Sub

    Private Sub OnMicDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        SyncLock _pipeLock
            Try
                If _pipeStream IsNot Nothing AndAlso _pipeStream.CanWrite Then
                    If _config.MicVolume >= 0.999F Then
                        _pipeStream.Write(e.Buffer, 0, e.BytesRecorded)
                    Else
                        Dim adjusted As Byte() = ApplyVolume16(e.Buffer, e.BytesRecorded, _config.MicVolume)
                        _pipeStream.Write(adjusted, 0, adjusted.Length)
                    End If
                    _pipeStream.Flush()
                End If
            Catch
            End Try
        End SyncLock
    End Sub

    Private Function ApplyVolume16(buffer As Byte(), bytesRecorded As Integer, volume As Single) As Byte()
        Dim out As Byte() = New Byte(bytesRecorded - 1) {}
        For i As Integer = 0 To bytesRecorded - 1 Step 2
            If i + 1 >= bytesRecorded Then Exit For
            Dim sample As Int16 = CShort(buffer(i) Or CShort(buffer(i + 1) << 8))
            Dim scaled As Integer = CInt(Math.Round(sample * volume))
            If scaled > Int16.MaxValue Then scaled = Int16.MaxValue
            If scaled < Int16.MinValue Then scaled = Int16.MinValue
            Dim v As Int16 = CShort(scaled)
            out(i) = CByte(v And &HFF)
            out(i + 1) = CByte((v >> 8) And &HFF)
        Next
        Return out
    End Function

    Private Sub OnSystemCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] System capture stopped")
    End Sub

    Private Sub OnMicCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] Mic capture stopped")
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        [Stop]()
    End Sub

End Class
