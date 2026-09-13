' OscEngineClient.vb — the Overlay Engine's hub client. It is the exact
' counterpart of the Forms overlay's Base TCP layer ([Overlay] Client.vb):
' same TcpClientHelper source (linked, not forked), same bounded status
' pull, same PREWARM on engine_ready — but the commands come from the osc
' web UI instead of WinForms buttons, and every received event is
' forwarded to the controller server / form via .NET events.
'
' Protocol ground truth lives in Overlay.Engine/PROTOCOL-MATRIX.md.
' Pipe rule: this client NEVER puts '|' in a value it sends, and parses
' defensively when other senders still do.

Imports System
Imports System.Diagnostics
Imports System.Threading

Public Class OscEngineClient
    Implements IDisposable

    Private ReadOnly _tcp As TcpClientHelper
    Private ReadOnly _pullTimer As System.Threading.Timer
    Private _engineStatusPulled As Boolean
    Private _disposed As Boolean

    Public Event LogLine(message As String)
    Public Event HubConnected()
    Public Event HubDisconnected()
    Public Event OpenOverlayRequested()
    Public Event EngineReady()
    Public Event EngineResponse(resp As OscProtocol.EngineResponse)
    Public Event EngineStateChanged(stateName As String)
    Public Event RecordingProgress(elapsedSec As Integer, tailFields As String())
    Public Event RecordingSaved(filePath As String)
    Public Event RecordingError(message As String)
    Public Event RecordStartedConfirmed()
    Public Event RecordFailed(reason As String)

    Public Sub New()
        _tcp = New TcpClientHelper(OscProtocol.AppName)
        AddHandler _tcp.OnMessageReceived, AddressOf OnMessage
        AddHandler _tcp.OnDisconnected, AddressOf OnDisconnected
        AddHandler _tcp.OnReconnected, AddressOf OnReconnected
        ' bounded status pull: 2 s × 10, identical cadence to the Forms
        ' overlay's StartBoundedStatusPull (System.Windows.Forms.Timer there;
        ' a Threading.Timer here because this client is UI-agnostic).
        _pullTimer = New System.Threading.Timer(AddressOf StatusPullTick, Nothing, Timeout.Infinite, Timeout.Infinite)
    End Sub

    Public Sub Connect()
        _tcp.ConnectAsync()
        StartBoundedStatusPull()
    End Sub

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _tcp.IsConnected
        End Get
    End Property

    ' ── sends ──────────────────────────────────────────────────

    Public Sub SendRecordStart(outputPath As String)
        _tcp.Send("RECORD_START", OscProtocol.RecordStartValue(outputPath))
        Log("RECORD_START → " & outputPath)
    End Sub

    Public Sub SendRecordStop()
        _tcp.Send("RECORD_STOP")
        Log("RECORD_STOP")
    End Sub

    Public Sub SendPrewarm(ffmpegPath As String)
        If String.IsNullOrEmpty(ffmpegPath) Then Return
        _tcp.Send("PREWARM_FFMPEG", OscProtocol.PrewarmValue(ffmpegPath))
        Log("PREWARM_FFMPEG → " & ffmpegPath)
    End Sub

    Public Sub SendGetStatus()
        _tcp.Send("engine_get_status")
    End Sub

    ' ── receive loop ───────────────────────────────────────────

    Private Sub OnMessage(msg As String)
        Dim m As OscProtocol.HubMessage = OscProtocol.ParseHubLine(msg)
        If m Is Nothing Then Return

        Select Case m.Cmd
            Case "open_overlay"
                Log("open_overlay from " & m.Sender)
                RaiseEvent OpenOverlayRequested()

            Case "engine_ready"
                If String.Equals(m.Sender, OscProtocol.EngineAppName, StringComparison.Ordinal) Then
                    Log("engine_ready → prewarm + status pull")
                    RaiseEvent EngineReady()
                    Dim ffmpeg As String = AppConfigShared.ReadString("Paths", "FFmpegPath", "")
                    SendPrewarm(ffmpeg)
                    _engineStatusPulled = False
                    SendGetStatus()
                End If

            Case "engine_response"
                If String.Equals(m.Sender, OscProtocol.EngineAppName, StringComparison.Ordinal) Then
                    Dim resp As OscProtocol.EngineResponse = OscProtocol.ParseEngineResponse(m.FullValue)
                    If resp Is Nothing Then Return
                    HandleEngineResponse(resp)
                End If

            Case "engine_state_changed"
                If String.Equals(m.Sender, OscProtocol.EngineAppName, StringComparison.Ordinal) Then
                    Log("engine_state_changed: " & m.Value)
                    RaiseEvent EngineStateChanged(m.Value)
                End If

            Case "engine_recording_progress"
                If String.Equals(m.Sender, OscProtocol.EngineAppName, StringComparison.Ordinal) Then
                    ' m.Value holds only <sec> when the sender's '|' fields
                    ' were truncated; m.FullValue carries the tail when the
                    ' hub let it through. Parse from FullValue defensively.
                    Dim fields As String() = If(m.FullValue, "").Split("|"c)
                    Dim sec As Integer
                    If fields.Length >= 1 AndAlso Integer.TryParse(fields(0).Trim(), sec) Then
                        RaiseEvent RecordingProgress(sec, fields)
                    End If
                End If

            Case "engine_recording_saved"
                If String.Equals(m.Sender, OscProtocol.EngineAppName, StringComparison.Ordinal) Then
                    Log("recording saved: " & m.Value)
                    RaiseEvent RecordingSaved(m.Value)
                End If

            Case "engine_recording_error"
                If String.Equals(m.Sender, OscProtocol.EngineAppName, StringComparison.Ordinal) Then
                    Log("recording error: " & m.Value)
                    RaiseEvent RecordingError(m.Value)
                End If

                ' Case Else: [System]|pong filtered inside TcpClientHelper;
                ' anything else is another app's chatter — ignore silently.
        End Select
    End Sub

    Private Sub HandleEngineResponse(resp As OscProtocol.EngineResponse)
        Log("engine_response: " & resp.Cmd & "," & resp.Status & If(String.IsNullOrEmpty(resp.Data), "", "," & resp.Data))
        Select Case resp.Cmd
            Case "engine_record_start"
                If resp.Status = "ok" Then
                    RaiseEvent RecordStartedConfirmed()
                Else
                    RaiseEvent RecordFailed(If(resp.Data, "unknown"))
                End If
                RaiseEvent EngineResponse(resp)

            Case "engine_get_status"
                ' ANY answer retires the bounded pull (same latch contract
                ' as the Forms overlay).
                _engineStatusPulled = True
                StopBoundedStatusPull()
                RaiseEvent EngineResponse(resp)

            Case Else
                RaiseEvent EngineResponse(resp)
        End Select
    End Sub

    ' ── bounded status pull (L1 rehydration contract) ──────────

    Private Sub StartBoundedStatusPull()
        _pullAttempts = 10
        _pullTimer.Change(2000, 2000)
        Log("bounded engine status pull started (max 10 attempts)")
    End Sub

    Private _pullAttempts As Integer

    Private Sub StatusPullTick(state As Object)
        If _disposed OrElse _engineStatusPulled Then
            _pullTimer.Change(Timeout.Infinite, Timeout.Infinite)
            Return
        End If
        _pullAttempts -= 1
        If _pullAttempts <= 0 Then
            _pullTimer.Change(Timeout.Infinite, Timeout.Infinite)
            Log("bounded status pull exhausted — engine state unknown; broadcasts still reconcile")
            Return
        End If
        If _tcp.IsConnected Then
            SendGetStatus()
        End If
    End Sub

    Private Sub StopBoundedStatusPull()
        _pullTimer.Change(Timeout.Infinite, Timeout.Infinite)
    End Sub

    Private Sub OnDisconnected()
        Log("hub disconnected")
        RaiseEvent HubDisconnected()
    End Sub

    Private Sub OnReconnected()
        ' L1: a UI-side reconnect is invisible to the Engine — re-pull.
        _engineStatusPulled = False
        _pullAttempts = 10
        _pullTimer.Change(2000, 2000)
        Log("hub reconnected → pulled engine_get_status")
        SendGetStatus()
        RaiseEvent HubConnected()
    End Sub

    Private Sub Log(message As String)
        RaiseEvent LogLine(message)
        Debug.WriteLine("[OscEngine/TCP] " & message)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _disposed = True
        Try : _pullTimer.Dispose() : Catch : End Try
        Try : _tcp.Dispose() : Catch : End Try
    End Sub

End Class
