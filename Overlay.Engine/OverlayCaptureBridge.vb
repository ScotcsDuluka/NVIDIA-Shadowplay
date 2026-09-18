Imports System
Imports System.Threading

''' <summary>
''' Single command seam between the OSC UI and the existing capture backend.
''' The UI never needs to know whether the backend is the local engine,
''' FFmpeg, or a future capture implementation.
''' </summary>
Public NotInheritable Class OverlayCaptureBridge
    Private ReadOnly _client As OscEngineClient
    Private ReadOnly _gate As New Object()
    Private _recording As Boolean
    Private _lastCommandTick As Long
    Private Const CommandDebounceMs As Integer = 250

    Public Event LogLine(message As String)

    Public Sub New(client As OscEngineClient)
        If client Is Nothing Then Throw New ArgumentNullException(NameOf(client))
        _client = client
        AddHandler _client.RecordStartedConfirmed, Sub()
                                                        SyncLock _gate
                                                            _recording = True
                                                        End SyncLock
                                                    End Sub
        AddHandler _client.RecordingSaved, Sub()
                                               SyncLock _gate
                                                   _recording = False
                                               End SyncLock
                                           End Sub
        AddHandler _client.RecordingError, Sub()
                                               SyncLock _gate
                                                   _recording = False
                                               End SyncLock
                                           End Sub
        AddHandler _client.EngineStateChanged, AddressOf OnEngineStateChanged
    End Sub

    Public ReadOnly Property IsRecording As Boolean
        Get
            SyncLock _gate
                Return _recording
            End SyncLock
        End Get
    End Property

    Public Sub SetRecordingEnabled(enabled As Boolean)
        SyncLock _gate
            If Not AcceptCommandLocked() Then Return
            If enabled = _recording Then
                Log("capture command ignored: already " & If(enabled, "recording", "stopped"))
                Return
            End If
            If enabled Then
                Dim savePath As String = AppConfigShared.ReadString("Paths", "SavePath", "")
                Dim outputPath As String = OscProtocol.RecordOutputPath(savePath, DateTime.Now)
                _client.SendRecordStart(outputPath)
                Log("capture start → " & outputPath)
            Else
                _client.SendRecordStop()
                Log("capture stop")
            End If
        End SyncLock
    End Sub

    Public Sub ToggleRecording()
        SetRecordingEnabled(Not IsRecording)
    End Sub

    Private Sub OnEngineStateChanged(stateName As String)
        SyncLock _gate
            Select Case stateName
                Case "Recording"
                    _recording = True
                Case "Idle", "HasError"
                    _recording = False
            End Select
        End SyncLock
    End Sub

    Private Function AcceptCommandLocked() As Boolean
        Dim now As Long = Environment.TickCount64
        If _lastCommandTick <> 0 AndAlso now - _lastCommandTick < CommandDebounceMs Then
            Log("capture command ignored: debounce")
            Return False
        End If
        _lastCommandTick = now
        Return True
    End Function

    Private Sub Log(message As String)
        RaiseEvent LogLine(message)
    End Sub
End Class
