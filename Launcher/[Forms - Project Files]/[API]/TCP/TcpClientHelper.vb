Imports System.IO
Imports System.Net.Sockets
Imports System.Threading

Public Class TcpClientHelper
    Implements IDisposable

    Private _client As TcpClient
    Private _writer As StreamWriter
    Private _reader As StreamReader
    Private _isConnected As Boolean
    Private _cts As CancellationTokenSource
    Private _writeLock As New Object()

    ' ★ FIX (concurrency): single-flight reconnect gate — Connect() failure and
    ' ListenLoop exit both spawn ReconnectLoop; without a gate two loops can
    ' fight over _client/_writer/_reader and end up double-connecting.
    Private _reconnectGate As Integer = 0

    ' ★ FIX (concurrency): connection generation — bumped on every new
    ' connection and on Disconnect. Stale Listen/Ping/Reconnect loops compare
    ' their captured generation and exit silently when it no longer matches,
    ' so an old loop can never kill or respawn a newer connection.
    Private _generation As Integer = 0

    Private ReadOnly _appName As String
    Private ReadOnly _host As String
    Private ReadOnly _port As Integer
    Private ReadOnly _autoReconnect As Boolean
    Private ReadOnly _reconnectIntervalMs As Integer = 3000
    Private _currentReconnectDelay As Integer = 1000 ' ★ Start at 1s, doubles each attempt (exponential backoff)

    Public Event OnMessageReceived(msg As String)
    Public Event OnDisconnected()
    Public Event OnReconnecting()

    Public Sub New(appName As String,
                   Optional host As String = "127.0.0.1",
                   Optional port As Integer = 5000,
                   Optional autoReconnect As Boolean = True)

        _appName = appName
        _host = host
        _port = port
        _autoReconnect = autoReconnect

        ' ★ FIX (startup): the constructor used to call Connect() synchronously,
        ' which blocked the UI thread inside TcpClient.Connect. Call
        ' ConnectAsync() explicitly instead.
    End Sub

    ''' <summary>★ FIX: non-blocking connect — safe to call from the UI thread.</summary>
    Public Sub ConnectAsync()
        Dim gen As Integer = _generation
        Task.Run(Sub()
                     ' Skip if Disconnect()/Dispose() happened before we started.
                     If gen = _generation Then Connect()
                 End Sub)
    End Sub

    Public Sub Connect()
        Try
            Disconnect()

            _cts = New CancellationTokenSource()

            _client = New TcpClient()
            _client.Connect(_host, _port)

            Dim stream As NetworkStream = _client.GetStream()

            ' ★ FIX (race): publish writer/reader under the same lock that
            ' Send/SendLog/Ping use, so a write can never interleave with a swap.
            SyncLock _writeLock
                _writer = New StreamWriter(stream) With {.AutoFlush = True}
                _reader = New StreamReader(stream)
                _generation += 1
                _isConnected = True
            End SyncLock

            Task.Run(AddressOf ListenLoop)
            Task.Run(AddressOf PingLoop)

        Catch ex As Exception
            _isConnected = False

            If _autoReconnect Then
                Task.Run(AddressOf ReconnectLoop)
            End If
        End Try
    End Sub

    Public Sub Disconnect()
        ' ★ FIX (race): bump generation first so stale loops exit as no-ops.
        SyncLock _writeLock
            _generation += 1
            _isConnected = False
        End SyncLock
        If _cts IsNot Nothing Then
            Try : _cts.Cancel() : Catch : End Try
        End If
        If _client IsNot Nothing Then
            Try : _client.Close() : Catch : End Try
        End If
        _client = Nothing
    End Sub

    Public Sub Send(cmd As String, Optional value As String = "")
        If Not IsConnected Then Exit Sub

        Try
            SyncLock _writeLock
                ' ★ FIX (race): hold the lock across the whole write so a
                ' reconnect swap can never replace _writer mid-line.
                If _writer IsNot Nothing Then
                    Dim msg As String

                    If value = "" Then
                        msg = $"[Send] {_appName}|{cmd}"
                    Else
                        msg = $"[Send] {_appName}|{cmd}:{value}"
                    End If

                    _writer.WriteLine(msg)
                End If
            End SyncLock
        Catch ex As Exception
            Debug.WriteLine($"TcpClientHelper.Send Error ({_appName}): {ex.Message}")
            _isConnected = False
        End Try
    End Sub

    Public Sub SendLog(message As String)
        If Not IsConnected Then Exit Sub

        Try
            SyncLock _writeLock
                If _writer IsNot Nothing Then
                    _writer.WriteLine($"[Receive] {_appName}|{message}")
                End If
            End SyncLock
        Catch ex As Exception
            Debug.WriteLine($"TcpClientHelper.SendLog Error ({_appName}): {ex.Message}")
            _isConnected = False
        End Try
    End Sub

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected AndAlso _client IsNot Nothing AndAlso _client.Connected
        End Get
    End Property

    Private Sub ListenLoop()
        ' ★ FIX (race): capture generation + local reader. If a newer
        ' connection replaces them, this loop exits without touching state.
        Dim myGen As Integer = _generation
        Dim reader As StreamReader = _reader

        Try
            While _isConnected AndAlso myGen = _generation AndAlso Not _cts.IsCancellationRequested
                Dim msg = reader.ReadLine()
                If msg Is Nothing Then Exit While

                If msg = "[System]|pong" Then Continue While

                RaiseEvent OnMessageReceived(msg)
            End While
        Catch ex As Exception
            Debug.WriteLine($"TcpClientHelper.ListenLoop Error ({_appName}): {ex.Message}")
        End Try

        ' ★ FIX (race): only the loop of the CURRENT generation may mark the
        ' client disconnected and trigger reconnect — a stale loop exiting
        ' after a manual reconnect used to kill the fresh connection's state.
        If myGen = _generation Then
            _isConnected = False
            RaiseEvent OnDisconnected()

            If _autoReconnect Then
                Task.Run(AddressOf ReconnectLoop)
            End If
        End If
    End Sub

    Private Sub PingLoop()
        ' ★ FIX (race): generation guard — an old ping loop dies with its
        ' connection instead of running alongside the new one.
        Dim myGen As Integer = _generation

        Try
            While _isConnected AndAlso myGen = _generation AndAlso Not _cts.IsCancellationRequested
                Thread.Sleep(10000)

                If myGen <> _generation OrElse Not IsConnected Then Exit While

                Try
                    SyncLock _writeLock
                        If _writer IsNot Nothing Then
                            _writer.WriteLine($"[Send] {_appName}|ping")
                        End If
                    End SyncLock
                Catch
                    Exit While
                End Try
            End While
        Catch ex As Exception
            Debug.WriteLine($"TcpClientHelper.PingLoop Error ({_appName}): {ex.Message}")
        End Try
    End Sub

    Private Sub ReconnectLoop()
        ' ★ FIX (duplicate spawn): Connect() failure AND ListenLoop exit both
        ' spawn this loop. The gate guarantees exactly one loop runs; late
        ' arrivals return immediately.
        If Interlocked.CompareExchange(_reconnectGate, 1, 0) <> 0 Then Return

        Try
            Dim startGen As Integer = _generation
            RaiseEvent OnReconnecting()

            While Not IsConnected AndAlso _autoReconnect AndAlso startGen = _generation
                Try
                    ' ★ FIX: Exponential backoff instead of fixed interval
                    _currentReconnectDelay = Math.Min(_currentReconnectDelay * 2, 30000) ' max 30s
                    Thread.Sleep(_currentReconnectDelay)

                    ' ★ FIX (dispose race): Disconnect()/new Connect() during
                    ' backoff must stop this loop.
                    If startGen <> _generation Then Exit While

                    _cts = New CancellationTokenSource()
                    _client = New TcpClient()
                    _client.Connect(_host, _port)

                    ' ← แก้: ประกาศ type ชัดเจน
                    Dim stream As NetworkStream = _client.GetStream()

                    ' ★ FIX (race): same lock discipline as Connect()/Send().
                    SyncLock _writeLock
                        _writer = New StreamWriter(stream) With {.AutoFlush = True}
                        _reader = New StreamReader(stream)
                        _generation += 1
                        _isConnected = True
                    End SyncLock

                    _currentReconnectDelay = 1000 ' ★ Reset backoff on successful connection

                    Task.Run(AddressOf ListenLoop)
                    Task.Run(AddressOf PingLoop)
                    Return

                Catch ex As Exception
                    Debug.WriteLine($"TcpClientHelper.ReconnectLoop Error ({_appName}): {ex.Message}")
                    _isConnected = False
                    Try : _client.Close() : Catch ex2 As Exception : Debug.WriteLine($"TcpClientHelper.ReconnectLoop cleanup Error: {ex2.Message}") : End Try
                End Try
            End While
        Finally
            Interlocked.Exchange(_reconnectGate, 0)
        End Try
    End Sub

    ''' <summary>★ FIX: Implements IDisposable for proper cleanup</summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Disconnect()
    End Sub

End Class
