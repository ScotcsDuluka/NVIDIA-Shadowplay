Option Strict On
Option Explicit On
Option Infer On

' DecodeStallWatchdog.vb — F4 liveness decision (design doc §7 spirit).
'
' PROBLEM (W3 audit, real evidence): a decode worker can stay ALIVE while
' producing nothing (hung ffmpeg, wedged pipe, suspended process). The render
' loop's only end-of-starvation signals are EOF and worker faults — neither
' fires for a silent-but-alive decoder, so the session stayed Playing forever
' with a frozen frame. This class is the deterministic decision core that
' closes that hole; PlaybackSession.RenderLoop polls it each tick.
'
' PROGRESS SIGNAL: the decode worker's FramesDecoded counter (monotonic,
' increments per complete frame read from the pipe — FfmpegDecodeWorker
' OnCompleteFrame). A decoder that produces frames — even under queue
' backpressure, where frames are rejected — still advances the counter, so
' the watchdog never fires on a healthy-but-slow pipeline. It fires only
' when NO complete frame has arrived for the whole window.
'
' DETERMINISM: all time enters through explicit nowQpc arguments (caller
' samples Stopwatch.GetTimestamp(); tests inject synthetic values) and the
' window converts through an injectable QPC frequency — no hidden wall-clock
' reads, no thread affinity. Pure + Linux-testable, like PlaybackClock.
'
' EOF CONTRACT: decodeEof short-circuits to False (the EOF path owns the
' transition; the watchdog must never race it with a fault).

Imports System
Imports System.Diagnostics

Namespace Gallery.Video

    Public NotInheritable Class DecodeStallWatchdog

        Private ReadOnly _timeoutTicks As Long
        Private _lastProgressQpc As Long
        Private _lastFramesDecoded As Long = -1

        ''' <summary>Real constructor. timeoutMs must be >= 1.</summary>
        Public Sub New(timeoutMs As Integer)
            Me.New(timeoutMs, Stopwatch.Frequency)
        End Sub

        ''' <summary>Test constructor — inject the QPC frequency so window
        ''' math is exact under synthetic nowQpc values.</summary>
        Public Sub New(timeoutMs As Integer, qpcFrequency As Long)
            If timeoutMs < 1 Then Throw New ArgumentOutOfRangeException(NameOf(timeoutMs))
            If qpcFrequency <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(qpcFrequency))
            _timeoutTicks = CLng(timeoutMs / 1000.0 * qpcFrequency)
        End Sub

        ''' <summary>Re-arm for a fresh decode generation (counter domain
        ''' restarts at 0 — without a reset the first poll would read the
        ''' domain change as progress, which is harmless, but an explicit
        ''' reset makes the first window start exactly at spawn).</summary>
        Public Sub Reset(nowQpc As Long)
            _lastProgressQpc = nowQpc
            _lastFramesDecoded = -1
        End Sub

        ''' <summary>External progress signal (a frame left the queue toward
        ''' the presenter). Resets the window without touching the counter —
        ''' the queue may still hold frames from before a hang, and draining
        ''' them IS forward motion until the buffer runs dry.</summary>
        Public Sub MarkProgress(nowQpc As Long)
            _lastProgressQpc = nowQpc
        End Sub

        ''' <summary>
        ''' Poll the stall decision. True when the decode counter has not
        ''' advanced for the full window (and the decoder has not hit EOF).
        ''' The comparison itself feeds the tracker — one call per poll.
        ''' After a True return the caller performs the single-shot
        ''' transition (state guard makes repeats no-ops) or Resets.
        ''' </summary>
        Public Function IsStalled(framesDecoded As Long, decodeEof As Boolean, nowQpc As Long) As Boolean
            If decodeEof Then
                _lastFramesDecoded = framesDecoded
                _lastProgressQpc = nowQpc
                Return False
            End If
            If framesDecoded <> _lastFramesDecoded Then
                _lastFramesDecoded = framesDecoded
                _lastProgressQpc = nowQpc
                Return False
            End If
            Return (nowQpc - _lastProgressQpc) >= _timeoutTicks
        End Function

        ''' <summary>Diagnostics: ticks remaining in the current window
        ''' (negative = already expired). Test/diagnostic seam.</summary>
        Public Function TicksRemaining(nowQpc As Long) As Long
            Return _timeoutTicks - (nowQpc - _lastProgressQpc)
        End Function

    End Class

End Namespace
