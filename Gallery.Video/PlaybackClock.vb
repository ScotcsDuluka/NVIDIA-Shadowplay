Option Strict On
Option Explicit On
Option Infer On

' PlaybackClock.vb — the ONE timestamp domain of a playback session (§3.6).
'
' OWNER RULE: A/V sync must live in a single timestamp domain. This engine
' uses 100-ns ticks — the same convention as FrameDiagnostics capture/PTS
' ticks and AudioPositionTracker QPC math elsewhere in the repo.
'
' Model:
'   - PresentationTicks(t) = AnchorTicks + (NowQpc - AnchorQpc) * 1e7 / QpcFreq
'   - Anchor set at Play / Seek-resume / Resume-from-pause.
'   - Pause freezes the clock (stores FrozenTicks; no wall-clock advance).
'   - Master clock source:
'       * audio present  → audio sample position (samplesPlayed / rate),
'         supplied by AudioRenderer each tick;
'       * audio absent   → QPC wall clock (this class).
'   - Late/early windows are computed against 1.5x frame interval (§3.4).
'
' This class is PURE (no thread affinity, no I/O) — Linux-testable, like
' AudioPositionTracker ("pure, Linux-testable stamp math" per PROJECT-STRUCTURE).

Imports System
Imports System.Diagnostics

Namespace Gallery.Video

    ''' <summary>Current presentation time in 100-ns ticks. (IDisposable for
    ''' Using-symmetry in tests — the clock holds no unmanaged resources.)</summary>
    Public NotInheritable Class PlaybackClock
        Implements IDisposable

        ''' <summary>QPC frequency (ticks per second) captured once per instance
        ''' (test seam: tests may inject a fake frequency + fake now source).</summary>
        Private ReadOnly _qpcFrequency As Long

        ''' <summary>Test seam — returns QPC counter now. Real = Stopwatch.GetTimestamp.</summary>
        Private ReadOnly _nowFunc As Func(Of Long)

        Private _anchorQpc As Long
        Private _anchorTicks As Long
        Private _frozenTicks As Long = -1   ' >= 0 while paused

        Public Sub New()
            Me.New(Stopwatch.Frequency, AddressOf Stopwatch.GetTimestamp)
        End Sub

        ''' <summary>Test constructor (deterministic clock math tests).</summary>
        Public Sub New(qpcFrequency As Long, nowFunc As Func(Of Long))
            If qpcFrequency <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(qpcFrequency))
            _qpcFrequency = qpcFrequency
            _nowFunc = If(nowFunc, AddressOf Stopwatch.GetTimestamp)
            AnchorAt(0L)
        End Sub

        ''' <summary>Re-anchor: presentation time `presentationTicks` happens NOW.
        ''' Used at Play-start and after every Seek (§3.7 step 5).</summary>
        Public Sub AnchorAt(presentationTicks As Long)
            _anchorQpc = _nowFunc()
            _anchorTicks = presentationTicks
            _frozenTicks = -1
        End Sub

        ''' <summary>Freeze the clock at its current presentation time (Pause).</summary>
        Public Sub Freeze()
            _frozenTicks = PresentationTicks
        End Sub

        ''' <summary>Unfreeze: re-anchor at the frozen time (Resume).</summary>
        Public Sub Unfreeze()
            If _frozenTicks < 0 Then Return
            Dim t = _frozenTicks
            AnchorAt(t)
        End Sub

        Public ReadOnly Property IsFrozen As Boolean
            Get
                Return _frozenTicks >= 0
            End Get
        End Property

        ''' <summary>Current presentation time in 100-ns ticks.</summary>
        Public ReadOnly Property PresentationTicks As Long
            Get
                If _frozenTicks >= 0 Then Return _frozenTicks
                Dim nowQpc = _nowFunc()
                Dim elapsedQpc = nowQpc - _anchorQpc
                Return _anchorTicks + CLng(elapsedQpc * 10000000.0 / _qpcFrequency)
            End Get
        End Property

        ''' <summary>Seconds helper (100-ns ticks → seconds).</summary>
        Public Shared Function TicksToSeconds(ticks As Long) As Double
            Return ticks / 10000000.0
        End Function

        Public Shared Function SecondsToTicks(seconds As Double) As Long
            Return CLng(seconds * 10000000.0)
        End Function

        ''' <summary>Frame interval window: a frame with PTS inside
        ''' [now - 1.5*interval, now + 1.5*interval] is presentable NOW;
        ''' older = late (drop), newer = early (hold). §3.4/§3.6.</summary>
        Public Shared Function FrameWindowTicks(frameIntervalSeconds As Double) As Long
            Return SecondsToTicks(Math.Max(0.001, frameIntervalSeconds) * 1.5)
        End Function

        ''' <summary>No-op — the clock owns no resources (kept for Using symmetry).</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub

    End Class

End Namespace
