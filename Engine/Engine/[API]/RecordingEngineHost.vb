Option Strict On
Option Explicit On
Option Infer On

' RecordingEngineHost.vb
'
' Partial class of UI_Engine that adds Phase 12b RecordingEngine integration.
' Replaces the legacy CaptureEngine (FFmpeg subprocess) with the new
' RecordingEngine (D3D11 + NVENC + NAudio + FFmpeg mux).
'
' Architecture:
'   RecordingEngine (process-lifetime, persistent GPU resources)
'       ↓ StartSession(config) — BLOCKS until duration or Stop()
'       ↓ Run on background thread (Task.Run) → UI thread not blocked
'   CaptureSession (per-session: audio + FFmpeg + mux → MP4)
'
' Integration:
'   HandleEngineRecordStart → start background task → respond "ok" immediately
'   HandleEngineRecordStop  → _recordingEngine.Stop() → await task → respond
'   HandleEngineGetStatus   → _recordingEngine.GetStatus() → respond

Imports System.IO
Imports System.Threading.Tasks
Imports CaptureEngine.Recording
Imports CaptureEngine.Diagnostics

Partial Public Class UI_Engine

    ' ─── RecordingEngine (Phase 12b) ─────────────────────────────────
    Private _recordingEngine As RecordingEngine
    Private _recordingTask As Task(Of SessionResult)
    Private _useNewEngine As Boolean = True  ' True = use RecordingEngine, False = legacy

    ''' <summary>
    ''' Initialize RecordingEngine. Called from UI_Engine_Load or InitializeEngine.
    ''' Creates persistent D3D11 + DXGI + NVENC resources (process-lifetime).
    ''' </summary>
    Private Sub InitializeRecordingEngine()
        Try
            Dim logger As New EngineLogger("RecordingEngine", EngineLogger.LogLevel.Info, AddressOf DebugLog)
            _recordingEngine = New RecordingEngine(logger)
            _recordingEngine.Initialize()
            DebugLog("[RecordingEngine] initialized — Idle")
        Catch ex As Exception
            DebugLog($"[RecordingEngine] Initialize FAILED: {ex.Message}")
            _recordingEngine = Nothing
            _useNewEngine = False
            DebugLog("[RecordingEngine] falling back to legacy CaptureEngine")
        End Try
    End Sub

    ''' <summary>
    ''' Dispose RecordingEngine. Called from UI_Engine_FormClosing.
    ''' </summary>
    Private Sub DisposeRecordingEngine()
        Try
            _recordingEngine?.Dispose()
            DebugLog("[RecordingEngine] disposed")
        Catch ex As Exception
            DebugLog($"[RecordingEngine] dispose error: {ex.Message}")
        End Try
    End Sub

    ' ─── Recording command handlers (Phase 12b) ──────────────────────

    ''' <summary>
    ''' Start recording using RecordingEngine. Non-blocking — starts a
    ''' background task that runs StartSession() and responds immediately.
    ''' </summary>
    Private Async Function HandleRecordingStart(value As String, reqId As String) As Task
        Try
            If _recordingEngine Is Nothing Then
                SendResponse("engine_record_start", "error", "engine_not_initialized", reqId)
                Return
            End If

            ' Resolve FFmpeg path
            Dim ffmpegPath As String = _settings?.FFmpegPath
            If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
                SendResponse("engine_record_start", "error", "ffmpeg_not_found", reqId)
                Return
            End If

            ' Create session config
            Dim config As New SessionConfig() With {
                .OutputPath = value,
                .DurationSeconds = 3600,  ' no fixed duration — stop via command
                .FFmpegPath = ffmpegPath
            }

            DebugLog($"[RecordingEngine] starting session: path={value}")

            ' Start on background thread — StartSession blocks until done
            _recordingTask = Task.Run(Function() _recordingEngine.StartSession(config))

            ' Respond immediately — recording has started
            ' UI update on UI thread
            Me.Invoke(Sub()
                          lblStatus.Text = "Recording (Hub)..."
                          lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                          tmrRecording.Start()
                          btnRecord.Enabled = False
                          btnStop.Enabled = True
                      End Sub)

            SendResponse("engine_record_start", "ok", value, reqId)
        Catch ex As Exception
            DebugLog($"[RecordingEngine] start error: {ex.Message}")
            SendResponse("engine_record_start", "error", ex.Message, reqId)
        End Try
    End Function

    ''' <summary>
    ''' Stop recording. Signals RecordingEngine.Stop() and waits for the
    ''' background task to complete. Returns the session result.
    ''' </summary>
    Private Async Function HandleRecordingStop(reqId As String) As Task
        Try
            If _recordingEngine Is Nothing OrElse _recordingTask Is Nothing Then
                SendResponse("engine_record_stop", "error", "not_recording", reqId)
                Return
            End If

            DebugLog("[RecordingEngine] stopping...")
            _recordingEngine.Stop()

            ' Wait for the session to complete (should be quick after Stop)
            Dim result As SessionResult = Await _recordingTask
            _recordingTask = Nothing

            DebugLog($"[RecordingEngine] stopped: pass={result.Pass}, file={result.OutputPath}")

            ' UI update
            Me.Invoke(Sub()
                          tmrRecording.Stop()
                          lblTimer.Text = "00:00:00"
                          lblStatus.Text = "Saved: " & Path.GetFileName(result.OutputPath)
                          lblStatus.ForeColor = Drawing.Color.FromArgb(118, 185, 0)
                          btnRecord.Enabled = True
                          btnStop.Enabled = False
                      End Sub)

            If result.Pass Then
                SendResponse("engine_record_stop", "ok", result.OutputPath, reqId)
            Else
                SendResponse("engine_record_stop", "error", result.ErrorMessage, reqId)
            End If
        Catch ex As Exception
            DebugLog($"[RecordingEngine] stop error: {ex.Message}")
            SendResponse("engine_record_stop", "error", ex.Message, reqId)
        End Try
    End Function

    ''' <summary>
    ''' Get recording status.
    ''' </summary>
    Private Sub HandleRecordingGetStatus(reqId As String)
        Try
            If _recordingEngine Is Nothing Then
                SendResponse("engine_get_status", "ok", "Idle", reqId)
                Return
            End If

            Dim status As EngineStatus = _recordingEngine.GetStatus()
            SendResponse("engine_get_status", "ok", status.State.ToString(), reqId)
        Catch ex As Exception
            SendResponse("engine_get_status", "error", ex.Message, reqId)
        End Try
    End Sub

End Class
