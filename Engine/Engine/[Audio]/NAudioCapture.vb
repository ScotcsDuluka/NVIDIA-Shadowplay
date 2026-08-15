Imports System.Collections.Concurrent
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

    Private _systemQueue As BlockingCollection(Of Byte())
    Private _micQueue As BlockingCollection(Of Byte())

    Private _systemWriterThread As Thread
    Private _micWriterThread As Thread

    Private _systemStream As Stream
    Private _micStream As Stream

    Private _systemFormat As AudioFormat
    Private _micFormat As AudioFormat

    Private _isRunning As Boolean = False
    Private _disposed As Boolean = False

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

        If _config.SystemAudioCapture Then
            _systemQueue = New BlockingCollection(Of Byte())(256)
            StartSystemCapture()
            StartSystemWriterThread()
        End If

        If _config.MicCapture AndAlso Not String.IsNullOrEmpty(_config.MicDeviceId) Then
            _micQueue = New BlockingCollection(Of Byte())(256)
            StartMicCapture()
            StartMicWriterThread()
        ElseIf _config.MicCapture AndAlso Not String.IsNullOrEmpty(_config.MicDeviceName) Then
            _micQueue = New BlockingCollection(Of Byte())(256)
            StartMicCapture()
            StartMicWriterThread()
        End If
    End Sub

    Public Sub [Stop]()
        _isRunning = False

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
        End Function
    End Function

    Private Sub OnSystemDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        Try
            If _systemQueue Is Nothing Then Return
            Dim copy(e.BytesRecorded - 1) As Byte
            Buffer.BlockCopy(e.Buffer, 0, copy, 0, e.BytesRecorded)
            If Not _systemQueue.TryAdd(copy, 100) Then
                System.Diagnostics.Debug.WriteLine("[NAudio] System queue full — dropping packet")
            End If
        Catch
        End Try
    End Sub

    Private Sub OnMicDataAvailable(sender As Object, e As WaveInEventArgs)
        If Not _isRunning OrElse e.BytesRecorded = 0 Then Return
        Try
            If _micQueue Is Nothing Then Return
            Dim copy(e.BytesRecorded - 1) As Byte
            Buffer.BlockCopy(e.Buffer, 0, copy, 0, e.BytesRecorded)
            If Not _micQueue.TryAdd(copy, 100) Then
                System.Diagnostics.Debug.WriteLine("[NAudio] Mic queue full — dropping packet")
            End If
        Catch
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
                Dim chunk As Byte() = Nothing
                If Not _systemQueue.TryTake(chunk, 500) Then Continue While
                If chunk Is Nothing Then Continue While
                SyncLock _systemStream
                    Try
                        If _systemStream IsNot Nothing AndAlso _systemStream.CanWrite Then
                            _systemStream.Write(chunk, 0, chunk.Length)
                        End If
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("[NAudio] System writer error: " & ex.Message)
                        Exit While
                    End Try
                End SyncLock
            End While

            While _systemQueue IsNot Nothing
                Dim chunk As Byte() = Nothing
                If Not _systemQueue.TryTake(chunk, 0) Then Exit While
                If chunk Is Nothing Then Continue While
                SyncLock _systemStream
                    Try
                        If _systemStream IsNot Nothing AndAlso _systemStream.CanWrite Then
                            _systemStream.Write(chunk, 0, chunk.Length)
                        End If
                    Catch
                        Exit While
                    End Try
                End SyncLock
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
                Dim chunk As Byte() = Nothing
                If Not _micQueue.TryTake(chunk, 500) Then Continue While
                If chunk Is Nothing Then Continue While
                SyncLock _micStream
                    Try
                        If _micStream IsNot Nothing AndAlso _micStream.CanWrite Then
                            _micStream.Write(chunk, 0, chunk.Length)
                        End If
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("[NAudio] Mic writer error: " & ex.Message)
                        Exit While
                    End Try
                End SyncLock
            End While

            While _micQueue IsNot Nothing
                Dim chunk As Byte() = Nothing
                If Not _micQueue.TryTake(chunk, 0) Then Exit While
                If chunk Is Nothing Then Continue While
                SyncLock _micStream
                    Try
                        If _micStream IsNot Nothing AndAlso _micStream.CanWrite Then
                            _micStream.Write(chunk, 0, chunk.Length)
                        End If
                    Catch
                        Exit While
                    End Try
                End SyncLock
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

    Private Sub OnSystemCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] System capture stopped")
        Try : _systemQueue?.CompleteAdding() : Catch : End Try
    End Sub

    Private Sub OnMicCaptureStopped(sender As Object, e As StoppedEventArgs)
        System.Diagnostics.Debug.WriteLine("[NAudio] Mic capture stopped")
        Try : _micQueue?.CompleteAdding() : Catch : End Try
    End Sub

    Private Function WaveFormatToInfo(wf As WaveFormat) As AudioFormat
        Dim info As New AudioFormat()
        If wf Is Nothing Then Return info

        info.SampleRate = wf.SampleRate
        info.Channels = wf.Channels

        If TypeOf wf Is WaveFormatExtensible Then
            Dim wfe As WaveFormatExtensible = DirectCast(wf, WaveFormatExtensible)
            info.IsFloat = (wfe.Encoding = WaveFormatEncoding.IeeeFloat)
            info.BitsPerSample = wfe.BitsPerSample
        Else
            info.IsFloat = (wf.Encoding = WaveFormatEncoding.IeeeFloat)
            info.BitsPerSample = wf.BitsPerSample
        End If
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

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        [Stop]()
    End Sub

End Class
