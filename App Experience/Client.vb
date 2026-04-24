Imports System.IO
Imports System.Net.Sockets
Imports System.Threading

' ══════════════════════════════════════════════
' 🌐 TCP CLIENT CLASS (FIXED - No Deadlock!)
' ══════════════════════════════════════════════

Public Class TcpClientHelper

    ' === VARIABLES ===
    Private _client As TcpClient
    Private _writer As StreamWriter
    Private _reader As StreamReader
    Private _stream As NetworkStream
    Private _isConnected As Boolean = False
    Private _cts As CancellationTokenSource
    Private _logCallback As Action(Of String)

    ' ✅ FIX: แยก Lock สำหรับ Read/Write
    Private _writeLock As New Object()
    Private _readLock As New Object()


    ' === CONSTRUCTOR ===
    Public Sub New(Optional onLog As Action(Of String) = Nothing)
        _logCallback = onLog
        Connect()
    End Sub


    ' === CONNECT ===
    Public Sub Connect()
        Try
            Disconnect()

            ' ✅ FIX: Reset cancellation token
            _cts = New CancellationTokenSource()

            _client = New TcpClient()
            _client.ReceiveTimeout = 5000
            _client.SendTimeout = 5000
            _client.Connect("127.0.0.1", 5000)

            _stream = _client.GetStream()
            _writer = New StreamWriter(_stream) With {.AutoFlush = True}
            _reader = New StreamReader(_stream)

            _isConnected = True
            Log("✓ Connected to 127.0.0.1:5000")

            ' Start listening (Background)
            Task.Run(AddressOf ListenLoop, _cts.Token)

        Catch ex As Exception
            _isConnected = False
            Log($"Connect Failed: {ex.Message}")
        End Try
    End Sub


    ' === DISCONNECT ===
    Public Sub Disconnect()
        _isConnected = False

        ' ✅ FIX: Cancel background task
        If _cts IsNot Nothing Then
            _cts.Cancel()
            Threading.Thread.Sleep(100) ' รอให้หยุด
        End If

        Try : _writer?.Close() : Catch : End Try
        Try : _reader?.Close() : Catch : End Try
        Try : _stream?.Close() : Catch : End Try
        Try : _client?.Close() : Catch : End Try

        _client = Nothing
        _stream = Nothing
        _writer = Nothing
        _reader = Nothing

        Log("🔌 Disconnected")
    End Sub


    ' === SEND MESSAGE (PUBLIC - SAFE!) ===
    ''' <summary>
    ''' ส่งคำสั่งไป Server - ไม่ค้าง!
    ''' </summary>
    Public Sub Send(action As String)
        ' Quick check
        If Not IsConnected Then
            Log("✗ Not connected!")
            Return
        End If

        Try
            ' ✅ FIX: Lock เฉพาะ Write อย่างเดียว!
            SyncLock _writeLock
                Dim msg = $"NVIDIA_APP|{action}"
                _writer.WriteLine(msg)
                _writer.Flush() ' Force send immediately
            End SyncLock

            Log($">> SENT: {action}")

        Catch ex As Exception
            Log($"✗ Send Error: {ex.Message}")
            _isConnected = False

            ' Auto reconnect (optional - comment out if not needed)
            ' Task.Run(Sub() 
            '     Threading.Thread.Sleep(1000)
            '     Connect()
            ' End Sub)
        End Try
    End Sub


    ' === PROPERTY ===
    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected AndAlso
                   _client IsNot Nothing AndAlso
                   _client.Connected AndAlso
                   _stream IsNot Nothing AndAlso
                   _stream.CanWrite
        End Get
    End Property


    ' === LISTEN LOOP (Background - Safe!) ===
    Private Sub ListenLoop()
        Log("👂 Listening started...")

        While _isConnected AndAlso Not _cts.IsCancellationRequested
            Try
                Dim msg As String = Nothing

                ' ✅ FIX: Check data available FIRST (no blocking!)
                Dim dataAvailable As Boolean = False

                SyncLock _readLock
                    If _stream IsNot Nothing AndAlso _stream.DataAvailable Then
                        dataAvailable = True
                        msg = _reader.ReadLine()
                    End If
                End SyncLock

                ' Process message
                If msg IsNot Nothing Then
                    Log($"<< RECV: {msg}")

                    ' Raise event (safe invoke)
                    Try
                        RaiseEvent OnMessageReceived(msg)
                    Catch
                    End Try
                ElseIf Not dataAvailable Then
                    ' No data, sleep briefly to prevent CPU 100%
                    Threading.Thread.Sleep(50)
                End If

            Catch ex As IO.IOException
                Log($"✗ Connection lost: {ex.Message}")
                Exit While

            Catch ex As OperationCanceledException
                Log("🛑 Listening cancelled")
                Exit While

            Catch ex As Exception
                Log($"⚠ Listen Error: {ex.Message}")
                ' Continue listening on minor errors
            End Try
        End While

        _isConnected = False
        Log("👂 Stopped")

        Try
            RaiseEvent OnDisconnected()
        Catch
        End Try
    End Sub


    ' === LOG ===
    Private Sub Log(msg As String)
        Dim line = $"[{DateTime.Now:HH:mm:ss}] {msg}"

        Debug.WriteLine(line)

        If _logCallback IsNot Nothing Then
            Try
                _logCallback.Invoke(line)
            Catch
            End Try
        End If
    End Sub


    ' === EVENTS ===
    Public Event OnMessageReceived(message As String)
    Public Event OnDisconnected()


    ' === DESTRUCTOR ===
    Protected Overrides Sub Finalize()
        Disconnect()
        MyBase.Finalize()
    End Sub

End Class