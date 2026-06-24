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
    Private _disposed As Boolean

    Private ReadOnly _appName As String
    Private ReadOnly _host As String
    Private ReadOnly _port As Integer
    Private ReadOnly _autoReconnect As Boolean
    Private ReadOnly _reconnectIntervalMs As Integer = 3000

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

        Connect()
    End Sub

    Public Sub Connect()
        If _disposed Then Return
        CancelCurrentOperations()
        _cts = New CancellationTokenSource()

        Try
            _client = New TcpClient()
            _client.Connect(_host, _port)

            Dim stream As NetworkStream = _client.GetStream()
            _writer = New StreamWriter(stream) With {.AutoFlush = True}
            _reader = New StreamReader(stream)

            _isConnected = True

            Dim token = _cts.Token
            Task.Run(Sub() ListenLoop(token), token)
            Task.Run(Sub() PingLoop(token), token)

        Catch ex As Exception
            _isConnected = False
            CleanupStreams()

            If _autoReconnect Then
                Dim token = _cts.Token
                Task.Run(Sub() ReconnectLoop(token), token)
            End If
        End Try
    End Sub

    Private Sub CleanupStreams()
        SyncLock _writeLock
            Try
                If _writer IsNot Nothing Then
                    _writer.Dispose()
                    _writer = Nothing
                End If
            Catch
            End Try
            Try
                If _reader IsNot Nothing Then
                    _reader.Dispose()
                    _reader = Nothing
                End If
            Catch
            End Try
        End SyncLock

        Try
            If _client IsNot Nothing Then
                _client.Close()
                _client = Nothing
            End If
        Catch
        End Try
    End Sub

    Private Sub CancelCurrentOperations()
        _isConnected = False
        If _cts IsNot Nothing Then
            Try : _cts.Cancel() : Catch : End Try
            Try : _cts.Dispose() : Catch : End Try
            _cts = Nothing
        End If
        CleanupStreams()
    End Sub

    Public Sub Disconnect()
        CancelCurrentOperations()
    End Sub

    Public Function Send(cmd As String, Optional value As String = "") As Boolean
        SyncLock _writeLock
            If Not _isConnected OrElse _writer Is Nothing Then Return False

            Try
                Dim msg As String
                If value = "" Then
                    msg = $"[Send] {_appName}|{cmd}"
                Else
                    msg = $"[Send] {_appName}|{cmd}:{value}"
                End If

                _writer.WriteLine(msg)
                Return True
            Catch
                _isConnected = False
                Return False
            End Try
        End SyncLock
    End Function

    Public Function SendLog(message As String) As Boolean
        SyncLock _writeLock
            If Not _isConnected OrElse _writer Is Nothing Then Return False

            Try
                _writer.WriteLine($"[Receive] {_appName}|{message}")
                Return True
            Catch
                _isConnected = False
                Return False
            End Try
        End SyncLock
    End Function

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _isConnected AndAlso _client IsNot Nothing AndAlso _client.Connected
        End Get
    End Property

    Private Sub ListenLoop(token As CancellationToken)
        Try
            While _isConnected AndAlso Not token.IsCancellationRequested
                If _reader Is Nothing Then Exit While

                Dim msg = _reader.ReadLine()
                If msg Is Nothing Then Exit While

                If msg = "[System]|pong" Then Continue While

                RaiseEvent OnMessageReceived(msg)
            End While
        Catch ex As OperationCanceledException
        Catch
        End Try

        _isConnected = False
        RaiseEvent OnDisconnected()
    End Sub

    Private Sub PingLoop(token As CancellationToken)
        Try
            While _isConnected AndAlso Not token.IsCancellationRequested
                Thread.Sleep(10000)

                If Not IsConnected OrElse _writer Is Nothing Then Exit While
                If token.IsCancellationRequested Then Exit While

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
        Catch ex As OperationCanceledException
        Catch
        End Try
    End Sub

    Private Sub ReconnectLoop(token As CancellationToken)
        RaiseEvent OnReconnecting()

        While Not _isConnected AndAlso _autoReconnect AndAlso Not token.IsCancellationRequested
            Try
                Thread.Sleep(_reconnectIntervalMs)
            Catch ex As ThreadInterruptedException
                Exit While
            End Try

            If token.IsCancellationRequested Then Exit While
            If _disposed Then Exit While

            Try
                _client = New TcpClient()
                _client.Connect(_host, _port)

                Dim stream As NetworkStream = _client.GetStream()
                _writer = New StreamWriter(stream) With {.AutoFlush = True}
                _reader = New StreamReader(stream)

                _isConnected = True

                Dim loopToken = _cts.Token
                Task.Run(Sub() ListenLoop(loopToken), loopToken)
                Task.Run(Sub() PingLoop(loopToken), loopToken)
                Return

            Catch
                _isConnected = False
                CleanupStreams()
            End Try
        End While
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        Disconnect()
    End Sub

End Class