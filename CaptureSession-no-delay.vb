Option Strict On
Option Explicit On
Option Infer On

' CaptureSession.vb - NO 10ms DELAY
'
' Per-session resource owner. Composes:
'   - BoundedVideoFrameSink (latest-oriented bounded frame handoff)
'   - System audio → WavSidecarWriter (bounded queue + writer thread)
'   - Raw H.264 file (native NVENC packets)
'   - FFmpeg wrap (raw H.264 → temp MP4 @ display refresh rate)
'   - MuxCoordinator (video + audio → final MP4, A/V sync offsets)
'
' Lifecycle (per session):
'   1. Start audio sidecar (WASAPI loopback → WavSidecarWriter queue)
'   2. Start video capture (DdagrabBackend.Start(sink)) + encoder
'   3. Capture/encode loop: sink.Take → encoder.Encode → write H.264 to file
'   4. Stop video → drain sink → stop encoder
'   5. Stop audio → event-driven WAV finalize (accounting invariant)
'   6. FFmpeg wrap: H.264 → temp MP4 at DISPLAY REFRESH RATE
'   7. ffprobe temp MP4 → exact video duration
'   8. FFmpeg mux with SyncMath offsets → final MP4
'   9. Verify MP4 streams → SessionResult
'
' Phase 12b audio model (AUDIO hard-blocker rework):
'   PRODUCER  : WASAPI DataAvailable callback → copy → enqueue. NO disk I/O
'               on the callback thread (proven AudioFileWriter lesson).
'   CONSUMER  : WavSidecarWriter dedicated thread drains bounded queue.
'   ACCOUNTING: enqueued = written + dropped (residual must be 0).
'   SYNC      : systemStartTicks captured at StartRecording() CALL time

Imports System
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports System.Threading.Tasks

Namespace CaptureEngine.Recording

    Public Class CaptureSession
        Implements IDisposable

        ' Session state
        Private _disposed As Boolean = False
        Private _sessionState As SessionState = SessionState.Idle
        Private _sessionStartQpc100ns As Long = 0
        Private _sessionStartTicks As Long = 0
        Private _sessionStartStopwatch As Stopwatch = Stopwatch.StartNew()

        ' Timeline state - NO 10ms DELAY
        Private _timelineStartTicks As Long = 0
        Private _timelineStartQpc100ns As Long = 0
        Private _nextCfrTick As Long = 0

        ' CFR state
        Private _cfrFps As Integer = 60
        Private _cfrPeriod100ns As Long = 16666667 ' 16.666667ms at 60fps
        Private _cfrTick As Long = 0
        Private _cfrQpc As Long = 0

        ' Resources
        Private _sink As BoundedVideoFrameSink
        Private _encoder As NvencEncoderBackend
        Private _muxCoordinator As MuxCoordinator

        ' CFR selection state
        Private _selectedSourceSeq As Integer = -1
        Private _selectedSourceTs As Long = 0
        Private _selectedSourceLag As Long = 0
        Private _selectedSourceTick As Long = 0
        Private _selectedSourceQpc As Long = 0

        ' CFR telemetry
        Private _ticks As Integer = 0
        Private _selectedSources As Integer = 0
        Private _seqSkips As Integer = 0
        Private _maxSelectedLag As Long = 0
        Private _maxSourceGap As Long = 0
        Private _avgEncode As Double = 0
        Private _maxEncode As Long = 0
        Private _lateTicks As Integer = 0
        Private _maxTickLate As Long = 0

        ' Session resources
        Private _audioEngine As AudioEngine
        Private _liveMux As LiveMux

        ' CFR selection debug
        Private _cfrSelectionDebug As New List(Of CfrSelectionDebug)()

        Public Sub New(ByVal sink As BoundedVideoFrameSink, ByVal encoder As NvencEncoderBackend, ByVal muxCoordinator As MuxCoordinator)
            _sink = sink
            _encoder = encoder
            _muxCoordinator = muxCoordinator
        End Sub

        Public Sub StartRecording(ByVal fps As Integer, ByVal durationMs As Integer, ByVal commonTimeline As CommonTimeline)
            If _disposed Then Throw New ObjectDisposedException(Me.GetType().Name)
            If _sessionState <> SessionState.Idle Then Throw New InvalidOperationException("Session already started")

            ' Initialize session state
            _sessionState = SessionState.Starting
            _cfrFps = fps
            _cfrPeriod100ns = CInt(1000000000.0 / fps)
            _ticks = 0
            _selectedSources = 0
            _seqSkips = 0
            _maxSelectedLag = 0
            _maxSourceGap = 0
            _avgEncode = 0
            _maxEncode = 0
            _lateTicks = 0
            _maxTickLate = 0
            _cfrSelectionDebug.Clear()

            ' Capture session start timestamps
            _sessionStartQpc100ns = Stopwatch.GetTimestamp() * 100
            _sessionStartTicks = Stopwatch.GetTimestamp()

            ' Initialize timeline - NO 10ms DELAY
            _timelineStartTicks = _sessionStartTicks
            _timelineStartQpc100ns = _sessionStartQpc100ns
            _nextCfrTick = _timelineStartTicks

            ' Initialize CFR state
            _cfrTick = _timelineStartTicks
            _cfrQpc = _timelineStartQpc100ns

            ' Start resources
            _encoder.Start()
            _sink.Start()

            ' Set session state to running
            _sessionState = SessionState.Running
        End Sub

        Public Sub StopRecording()
            If _disposed Then Throw New ObjectDisposedException(Me.GetType().Name)
            If _sessionState <> SessionState.Running Then Return

            ' Stop resources
            _sink.Stop()
            _encoder.Stop()

            ' Set session state to stopped
            _sessionState = SessionState.Stopped
        End Sub

        Public Sub ProcessCfrTick()
            If _disposed Then Throw New ObjectDisposedException(Me.GetType().Name)
            If _sessionState <> SessionState.Running Then Return

            ' CFR tick processing
            _ticks += 1

            ' CFR selection debug
            Dim selectionDebug As New CfrSelectionDebug()
            selectionDebug.TargetQpc100ns = _nextCfrTick
            selectionDebug.TimelineStartQpc100ns = _timelineStartQpc100ns
            selectionDebug.NextTick = _nextCfrTick
            selectionDebug.TimelineStartTicks = _timelineStartTicks
            selectionDebug.CfrTick = _cfrTick
            selectionDebug.CfrQpc = _cfrQpc

            ' Select frame for CFR tick
            Dim selectedFrame As VideoFrame = _sink.Take()
            If selectedFrame IsNot Nothing Then
                ' Update selection state
                _selectedSourceSeq = selectedFrame.Sequence
                _selectedSourceTs = selectedFrame.Timestamp
                _selectedSourceLag = _cfrTick - selectedFrame.Timestamp
                _selectedSourceTick = _cfrTick
                _selectedSourceQpc = _cfrQpc

                ' Update telemetry
                _selectedSources += 1
                If _selectedSourceLag > _maxSelectedLag Then _maxSelectedLag = _selectedSourceLag
                If _selectedSourceLag > 16666667 Then _lateTicks += 1
                If _selectedSourceLag > _maxTickLate Then _maxTickLate = _selectedSourceLag

                ' CFR selection debug
                selectionDebug.SelectedSeq = selectedFrame.Sequence
                selectionDebug.SelectedTs = selectedFrame.Timestamp
                selectionDebug.SelectedLag = _selectedSourceLag
                selectionDebug.SelectedTick = _selectedSourceTick
                selectionDebug.SelectedQpc = _selectedSourceQpc

                ' Feed to encoder
                _encoder.Encode(selectedFrame)

                ' Muxer feed
                Dim muxFeedTick As Long = _cfrTick + 1000000 ' 1ms buffer
                Dim muxFeedQpc As Long = _cfrQpc + 10000 ' 0.1ms buffer
                _muxCoordinator.FeedVideoPacket(selectedFrame, muxFeedTick, muxFeedQpc)

                ' CFR selection debug - mux feed
                selectionDebug.MuxFeedTick = muxFeedTick
                selectionDebug.MuxFeedQpc = muxFeedQpc
            Else
                ' No frame available, skip this tick
                _seqSkips += 1
            End If

            ' Update CFR state for next tick
            _cfrTick += _cfrPeriod100ns
            _cfrQpc += _cfrPeriod100ns
            _nextCfrTick = _cfrTick

            ' Add to selection debug
            _cfrSelectionDebug.Add(selectionDebug)
        End Sub

        ' ... rest of the class remains the same ...

    End Class

End Namespace