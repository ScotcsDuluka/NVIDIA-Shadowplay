Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports CaptureEngine.Backends

Namespace CaptureEngine.FFmpegBackend
    ''' <summary>
    ''' FFmpegPipelineBackend — the orchestrator. Implements IVideoBackend
    ''' (fire-and-forget model: GetFrame() returns Nothing).
    '''
    ''' P1-C SKELETON — empty stub. Implementation in next commit.
    '''
    ''' Lifecycle:
    '''   Created → Starting → Running → Stopping → [Muxing] → Stopped → Disposed
    '''                                                    ↘ Faulted ↗
    '''
    ''' Ownership:
    '''   - FFmpegProcessHost (owns the FFmpeg Process)
    '''   - FFmpegStderrParser (parses stderr stats)
    '''   - AudioSidecar (WASAPI → temp.wav, conditional on audio config)
    '''   - MuxCoordinator (ffprobe + mux FFmpeg, created lazily at Stop time)
    '''
    ''' Critical thread safety rules (per OWNER):
    '''   🔴 NEVER hold SyncLock across:
    '''       - WaitForExit()
    '''       - Kill()
    '''       - Mux()
    '''       - ffprobe()
    '''   🔴 NEVER RaiseEvent inside SyncLock:
    '''       - Capture state under lock → release lock → fire event
    '''   🔴 Guard against 3-way race:
    '''       FFmpeg exited (OnExited, threadpool) + Stop() (caller) + Dispose() (caller/finalizer)
    '''       Use Interlocked.Exchange(_stopCompleted, 1) — first wins, others no-op.
    ''' </summary>
    Public NotInheritable Class FFmpegPipelineBackend
        Implements IVideoBackend
        Implements IDisposable

        ' ── State ──
        Private ReadOnly _sync As New Object()
        Private _state As VideoBackendState = VideoBackendState.Created
        Private _disposed As Boolean = False
        Private _stopCompleted As Integer = 0  ' Interlocked guard for 3-way race

        ' ── Components (owned) ──
        Private _processHost As FFmpegProcessHost
        Private _stderrParser As FFmpegStderrParser
        Private _audioSidecar As AudioSidecar
        Private _muxCoordinator As MuxCoordinator

        ' ── Config (set before Start) ──
        Private _ffmpegPath As String = ""
        Private _arguments As String = ""
        Private _outputPath As String = ""
        Private _useTwoProcess As Boolean = False  ' True if audio enabled
        Private _tempVideoPath As String = ""
        Private _tempSystemWav As String = ""
        Private _tempMicWav As String = ""

        ' ── Sync timestamps ──
        Private _videoStartTicks As Long = 0
        Private _videoStartDetected As Boolean = False
        Private _systemStartTicks As Long = 0
        Private _micStartTicks As Long = 0

        ' ── Events (fired OUTSIDE lock) ──
        Public Event StateChanged As Action(Of VideoBackendState)
        Public Event RecordingStarted As Action(Of String)
        Public Event RecordingStopped As Action(Of String)
        Public Event ErrorOccurred As Action(Of String)

        ' ===== IVideoBackend =====

        Public ReadOnly Property CurrentState As VideoBackendState Implements IVideoBackend.CurrentState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        Public Sub Start() Implements IVideoBackend.Start
            Throw New NotImplementedException("FFmpegPipelineBackend.Start — skeleton only")
        End Sub

        Public Function GetFrame() As VideoFrame Implements IVideoBackend.GetFrame
            ' Full-pipeline backend: FFmpeg writes to file, not to frames.
            ' GetFrame() always returns Nothing (fire-and-forget model).
            Return Nothing
        End Function

        Public Sub [Stop]() Implements IVideoBackend.[Stop]
            Throw New NotImplementedException("FFmpegPipelineBackend.Stop — skeleton only")
        End Sub

        Public Function GetDiagnostics() As IReadOnlyDictionary(Of String, Long) Implements IVideoBackend.GetDiagnostics
            If _stderrParser IsNot Nothing Then
                Return _stderrParser.GetSnapshot()
            End If
            Return New Dictionary(Of String, Long)()
        End Function

        ' ===== IDisposable =====

        Public Sub Dispose() Implements IDisposable.Dispose
            Throw New NotImplementedException("FFmpegPipelineBackend.Dispose — skeleton only")
        End Sub

        ' ===== Configuration (fluent API, called before Start) =====

        Public Function WithFFmpegPath(path As String) As FFmpegPipelineBackend
            _ffmpegPath = path
            Return Me
        End Function

        Public Function WithArguments(args As String) As FFmpegPipelineBackend
            _arguments = args
            Return Me
        End Function

        Public Function WithOutputPath(path As String) As FFmpegPipelineBackend
            _outputPath = path
            Return Me
        End Function

        Public Function WithTwoProcess(enabled As Boolean,
                                       tempVideoPath As String,
                                       tempSystemWav As String,
                                       tempMicWav As String) As FFmpegPipelineBackend
            _useTwoProcess = enabled
            _tempVideoPath = tempVideoPath
            _tempSystemWav = tempSystemWav
            _tempMicWav = tempMicWav
            Return Me
        End Function

        ' ===== Internal helpers (to be implemented in next commit) =====

        Private Sub TransitionTo(newState As VideoBackendState)
            ' TODO: thread-safe state transition + fire event OUTSIDE lock
            Throw New NotImplementedException()
        End Sub

        Private Sub FireStateChanged(newState As VideoBackendState)
            ' Fire event OUTSIDE lock (per OWNER rule)
            RaiseEvent StateChanged(newState)
        End Sub

        Private Sub OnProcessExited(exitCode As Integer)
            ' TODO: handle FFmpeg exit (graceful or crash)
            ' Guard against 3-way race via Interlocked.Exchange(_stopCompleted, 1)
            Throw New NotImplementedException()
        End Sub

        Private Sub OnStderrLine(line As String)
            If _stderrParser IsNot Nothing Then
                _stderrParser.ProcessLine(line)
            End If
        End Sub
    End Class
End Namespace
