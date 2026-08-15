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
        Public Property TrackMode As CaptureSettings.AudioTrackModeEnum = CaptureSettings.AudioTrackModeEnum.Single
    End Class

    Private _config As AudioConfigValues
    Private _systemCapture As WasapiLoopbackCapture
    Private _micCapture As WasapiCapture
    Private _systemStream As Stream
    Private _micStream As Stream
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

    Public Sub Start(systemStream As Stream, micStream As Stream)
        If _disposed Then Throw New ObjectDisposedException(NameOf(NAudioCaptureEngine))
        If _isRunning Then Return

        _systemStream = systemStream
        _micStream = micStream
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

        _systemStream = Nothing
        _micStream = Nothing
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
                Dim sysVol As Single = Math.Max(0.0F, Math.Min(1.5F, _config.SystemAudioVolume))

                If _config.TrackMode = CaptureSettings.AudioTrackModeEnum.Separate Then
                    If _systemStream IsNot Nothing AndAlso _systemStream.CanWrite Then
                        Dim data As Byte() = If(sysVol >= 0.999F,
                                                CopyBytes(e.Buffer, e.BytesRecorded),
                                                ScaleBytesF32(e.Buffer, e.BytesRecorded, sysVol))
                        _systemStream.Write(data, 0, data.Length)
                        _systemStream.Flush()
                    End If
                Else
                    If _systemStream IsNot Nothing AndAlso _systemStream.CanWrite Then
                        Dim data As Byte() = If(sysVol >= 0.999F,
                                                CopyBytes(e.Buffer, e.BytesRecorded),
                                                ScaleBytesF32(e.Buffer, e.BytesRecorded, sysVol))
                        _systemStream.Write(data, 0, data.Length)
                        _systemStream.Flush()
                    End If
                End If
            Catch
            End Try
        End SyncLock
    End Sub

    Private Sub OnMicDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        SyncLock _pipeLock
            Try
                Dim micVol As Single = Math.Max(0.0F, Math.Min(1.5F, _config.MicVolume))

                If _config.TrackMode = CaptureSettings.AudioTrackModeEnum.Separate Then
                    If _micStream IsNot Nothing AndAlso _micStream.CanWrite Then
                        Dim data As Byte() = If(micVol >= 0.999F,
                                                CopyBytes(e.Buffer, e.BytesRecorded),
                                                ScaleBytesF32(e.Buffer, e.BytesRecorded, micVol))
                        _micStream.Write(data, 0, data.Length)
                        _micStream.Flush()
                    End If
                Else
                    If _systemStream IsNot Nothing AndAlso _systemStream.CanWrite Then
                        Dim data As Byte() = If(micVol >= 0.999F,
                                                CopyBytes(e.Buffer, e.BytesRecorded),
                                                ScaleBytesF32(e.Buffer, e.BytesRecorded, micVol))
                        _systemStream.Write(data, 0, data.Length)
                        _systemStream.Flush()
                    End If
                End If
            Catch
            End Try
        End SyncLock
    End Sub

    Private Function CopyBytes(buffer As Byte(), length As Integer) As Byte()
        Dim out As Byte() = New Byte(length - 1) {}
        Array.Copy(buffer, out, length)
        Return out
    End Function

    Private Function ScaleBytesF32(buffer As Byte(), length As Integer, volume As Single) As Byte()
        Dim out As Byte() = New Byte(length - 1) {}
        For i As Integer = 0 To length - 1 Step 4
            If i + 3 >= length Then Exit For
            Dim sample As Single = BitConverter.ToSingle(buffer, i)
            sample *= volume
            If Single.IsNaN(sample) OrElse Single.IsInfinity(sample) Then sample = 0
            Dim bytes As Byte() = BitConverter.GetBytes(sample)
            out(i) = bytes(0)
            out(i + 1) = bytes(1)
            out(i + 2) = bytes(2)
            out(i + 3) = bytes(3)
        Next
        Return out
    End Function

    Private Sub OnSystemCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] System capture stopped")
    End Sub

    Private Sub OnMicCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] Mic capture stopped")
    End Sub

    Public Shared Function ListMicDevices() As List(Of String)
        Dim result As New List(Of String)()
        Try
            Using devEnum As New MMDeviceEnumerator()
                For Each dev As MMDevice In devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                    result.Add(dev.FriendlyName)
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
