Imports NAudio.Wave
Imports NAudio.CoreAudioApi
Imports System.Diagnostics
Imports System.Runtime.InteropServices

Namespace CaptureCore

    Public Class AudioCapture
        Implements IDisposable

#Region "Events"
        Public Event AudioDataAvailable As EventHandler(Of AudioDataEventArgs)
        Public Event CaptureStarted As EventHandler
        Public Event CaptureStopped As EventHandler
        Public Event CaptureError As EventHandler(Of String)
#End Region

#Region "Buffer Pool"
        ''' <summary>
        ''' ★ Pre-allocated buffer pool to eliminate GC pressure on hot path.
        ''' WASAPI typically delivers ~4800 bytes per callback at 48kHz stereo f32le.
        ''' Pool size = 8 buffers × 32KB each = 256KB total, enough for any callback.
        ''' </summary>
        Private Const POOL_BUFFER_SIZE As Integer = 32 * 1024  ' 32KB per slot
        Private Const POOL_SLOT_COUNT As Integer = 8
        Private _bufferPool As Byte()()  ' Jagged array: array of Byte arrays — sized in InitBufferPool()
        Private _bufferPoolAvailable(POOL_SLOT_COUNT - 1) As Boolean
        Private _poolLock As New Object()

        Private Sub InitBufferPool()
            ReDim _bufferPool(POOL_SLOT_COUNT - 1)
            ReDim _bufferPoolAvailable(POOL_SLOT_COUNT - 1)
            For i As Integer = 0 To POOL_SLOT_COUNT - 1
                _bufferPool(i) = New Byte(POOL_BUFFER_SIZE - 1) {}
                _bufferPoolAvailable(i) = True
            Next
        End Sub

        ''' <summary>Rent a buffer from pool. Returns Nothing if pool exhausted (rare).</summary>
        Private Function RentBuffer() As Byte()
            SyncLock _poolLock
                For i As Integer = 0 To POOL_SLOT_COUNT - 1
                    If _bufferPoolAvailable(i) Then
                        _bufferPoolAvailable(i) = False
                        Return _bufferPool(i)
                    End If
                Next
            End SyncLock
            ' Fallback: allocate if pool exhausted (shouldn't happen normally)
            Return New Byte(POOL_BUFFER_SIZE - 1) {}
        End Function

        ''' <summary>Return a buffer to pool.</summary>
        Private Sub ReturnBuffer(buf As Byte())
            SyncLock _poolLock
                For i As Integer = 0 To POOL_SLOT_COUNT - 1
                    If ReferenceEquals(_bufferPool(i), buf) Then
                        _bufferPoolAvailable(i) = True
                        Exit Sub
                    End If
                Next
            End SyncLock
            ' Not from pool — let GC collect it
        End Sub
#End Region

#Region "Properties"
        Private _isCapturing As Boolean = False
        Private _capture As WasapiLoopbackCapture
        Private _captureLock As New Object()
        Private _volume As Single = 1.0F
        Private _device As MMDevice
        Private _deviceEnumerator As MMDeviceEnumerator

        ''' <summary>
        ''' ★ v4 REMOVED: Volume is NO LONGER applied in AudioCapture.
        ''' Old bug: AudioCapture.ApplyVolumeFast + FFmpeg -af volume = double volume!
        ''' Now AudioCapture sends RAW audio data. Volume is handled by:
        '''   - FFmpeg -af "volume=X" for single source
        '''   - FFmpeg filter_complex [X:a]volume=X for amix
        ''' </summary>

        ''' <summary>จำนวนครั้งที่ WASAPI reconnect อัตโนมัติ</summary>
        Private _reconnectCount As Integer = 0
        Private Const MAX_RECONNECT As Integer = 3

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
            InitBufferPool()
        End Sub

        Public Sub New(deviceName As String)
            InitBufferPool()
            SetDevice(deviceName)
        End Sub

        Public Sub SetDevice(deviceName As String)
            Try
                ' ★ v4 FIX: Dispose old _device before replacing
                If _device IsNot Nothing Then
                    Try
                        _device.Dispose()
                    Catch ex As Exception
                        Debug.WriteLine("AudioCapture: SetDevice (dispose old device) Error: " & ex.Message)
                    End Try
                    _device = Nothing
                End If

                If _deviceEnumerator IsNot Nothing Then
                    _deviceEnumerator.Dispose()
                    _deviceEnumerator = Nothing
                End If

                _deviceEnumerator = New MMDeviceEnumerator()

                Dim found As Boolean = False
                For Each device In _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                    If device.FriendlyName.Equals(deviceName, StringComparison.OrdinalIgnoreCase) Then
                        _device = device
                        found = True
                    Else
                        ' ★ v4 FIX: Dispose non-matching devices (COM objects must be released)
                        Try
                            device.Dispose()
                        Catch ex As Exception
                            Debug.WriteLine("AudioCapture: SetDevice (dispose non-matching device) Error: " & ex.Message)
                        End Try
                    End If
                Next

                If Not found Then
                    _device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)
                End If
            Catch ex As Exception
                Debug.WriteLine("SetDevice Error: " & ex.Message)
            End Try
        End Sub
#End Region

#Region "Public Methods"
        Public Sub StartCapture()
            SyncLock _captureLock
                If _isCapturing Then Exit Sub

                Try
                    If _device IsNot Nothing Then
                        _capture = New WasapiLoopbackCapture(_device)
                    Else
                        _capture = New WasapiLoopbackCapture()
                    End If

                    AddHandler _capture.DataAvailable, AddressOf OnDataAvailable
                    AddHandler _capture.RecordingStopped, AddressOf OnRecordingStopped

                    _capture.StartRecording()
                    _isCapturing = True
                    _reconnectCount = 0

                    RaiseEvent CaptureStarted(Me, EventArgs.Empty)
                    Debug.WriteLine("AudioCapture: Started - Format: " & _capture.WaveFormat.ToString())

                Catch ex As Exception
                    Debug.WriteLine("AudioCapture Start Error: " & ex.Message)
                    RaiseEvent CaptureError(Me, ex.Message)
                End Try
            End SyncLock
        End Sub

        Public Sub StopCapture()
            SyncLock _captureLock
                If Not _isCapturing OrElse _capture Is Nothing Then Exit Sub

                Try
                    _isCapturing = False
                    _capture.StopRecording()
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture Stop Error: " & ex.Message)
                    CleanupCapture()
                End Try
            End SyncLock
        End Sub
#End Region

#Region "Private Methods"
        Private Sub OnDataAvailable(sender As Object, e As WaveInEventArgs)
            If Not _isCapturing OrElse e.BytesRecorded <= 0 Then Exit Sub

            Try
                ' ═══════════════════════════════════════════════════════════════════════
                ' ★ v3.1: Rent buffer from pool (zero GC on hot path)
                ' ═══════════════════════════════════════════════════════════════════════
                Dim buffer As Byte() = RentBuffer()
                If buffer Is Nothing Then
                    Debug.WriteLine("AudioCapture: Buffer pool exhausted, skipping frame")
                    Exit Sub
                End If

                Dim bytesToCopy As Integer = Math.Min(e.BytesRecorded, buffer.Length)
                Array.Copy(e.Buffer, buffer, bytesToCopy)

                ' ★ v4: Volume is NOT applied here anymore!
                ' Old bug: ApplyVolumeFast + FFmpeg -af volume = double volume (0.5*0.5=0.25)
                ' Now we send RAW audio — let FFmpeg handle volume in a single place.
                RaiseEvent AudioDataAvailable(Me, New AudioDataEventArgs(buffer, bytesToCopy, _capture.WaveFormat, AddressOf ReturnBuffer))

            Catch ex As Exception
                Debug.WriteLine("OnDataAvailable Error: " & ex.Message)
            End Try
        End Sub

        Private Sub OnRecordingStopped(sender As Object, e As StoppedEventArgs)
            Debug.WriteLine("AudioCapture: Recording stopped")

            If e.Exception IsNot Nothing Then
                Debug.WriteLine("AudioCapture Error: " & e.Exception.Message)
            End If

            ' ★ v4 FIX: Entire reconnect logic is now INSIDE _captureLock
            ' Old bug: reconnect happened outside lock → race with StopCapture()
            '   Thread A: OnRecordingStopped → exits lock → CleanupCapture → new _capture
            '   Thread B: StopCapture → enters lock → sees _isCapturing=True → _capture.StopRecording on OLD (disposed) capture
            ' Now: everything is serialized by _captureLock
            SyncLock _captureLock
                If _isCapturing AndAlso _reconnectCount < MAX_RECONNECT Then
                    _reconnectCount += 1
                    Debug.WriteLine(String.Format("AudioCapture: Auto-reconnect attempt {0}/{1}", _reconnectCount, MAX_RECONNECT))
                    Try
                        CleanupCapture()

                        If _device IsNot Nothing Then
                            _capture = New WasapiLoopbackCapture(_device)
                        Else
                            _capture = New WasapiLoopbackCapture()
                        End If

                        AddHandler _capture.DataAvailable, AddressOf OnDataAvailable
                        AddHandler _capture.RecordingStopped, AddressOf OnRecordingStopped

                        _capture.StartRecording()
                        Debug.WriteLine("AudioCapture: Reconnected successfully")
                        Return
                    Catch ex2 As Exception
                        Debug.WriteLine("AudioCapture: Reconnect failed: " & ex2.Message)
                        _isCapturing = False
                    End Try
                Else
                    _isCapturing = False
                End If
            End SyncLock

            CleanupCapture()

            If e.Exception IsNot Nothing Then
                RaiseEvent CaptureError(Me, e.Exception.Message)
            End If

            RaiseEvent CaptureStopped(Me, EventArgs.Empty)
        End Sub

        Private Sub CleanupCapture()
            If _capture IsNot Nothing Then
                Try
                    RemoveHandler _capture.DataAvailable, AddressOf OnDataAvailable
                    RemoveHandler _capture.RecordingStopped, AddressOf OnRecordingStopped
                    _capture.Dispose()
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture: CleanupCapture Error: " & ex.Message)
                End Try
                _capture = Nothing
            End If
        End Sub

        ' ★ v4 REMOVED: ApplyVolumeFast — volume is now handled entirely by FFmpeg.
        ' See the v4 note at the top of this class for the rationale.
#End Region

#Region "IDisposable"
        Private disposed As Boolean = False

        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposed Then Exit Sub

            If disposing Then
                Try
                    StopCapture()
                    CleanupCapture()
                Catch ex As Exception
                    Debug.WriteLine("AudioCapture: Dispose Error: " & ex.Message)
                End Try

                If _deviceEnumerator IsNot Nothing Then
                    Try
                        _deviceEnumerator.Dispose()
                    Catch ex As Exception
                        Debug.WriteLine("AudioCapture: Dispose (deviceEnumerator) Error: " & ex.Message)
                    End Try
                    _deviceEnumerator = Nothing
                End If
                _device = Nothing
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
    ''' ★ v3: Added ReturnCallback for buffer pool return
    ''' </summary>
    Public Class AudioDataEventArgs
        Inherits EventArgs

        Public Property Buffer As Byte()
        Public Property BytesRecorded As Integer
        Public Property WaveFormat As WaveFormat

        Private _returnCallback As Action(Of Byte())

        Public Sub New(buffer As Byte(), bytesRecorded As Integer, format As WaveFormat, returnCallback As Action(Of Byte()))
            Me.Buffer = buffer
            Me.BytesRecorded = bytesRecorded
            Me.WaveFormat = format
            _returnCallback = returnCallback
        End Sub

        ''' <summary>Call this when you're done with the buffer to return it to the pool</summary>
        Public Sub ReturnBuffer()
            If _returnCallback IsNot Nothing AndAlso Buffer IsNot Nothing Then
                _returnCallback(Buffer)
                Buffer = Nothing
            End If
        End Sub
    End Class

End Namespace
