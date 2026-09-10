Option Strict On
Option Explicit On
Option Infer On

' FrameQueue.vb — bounded playback frame queue (design doc §3.4).
'
' SEMANTICS (every case defined, unit-tested — owner mandate):
'   - BOUNDED by capacity. Enqueue BLOCKS when full (backpressure) with a
'     timeout — decode is a subprocess, so blocking it throttles ffmpeg
'     itself via the pipe. No unbounded memory growth is possible by design.
'   - Generation tokens: every frame carries the session generation it was
'     produced under. Flush(minGeneration) atomically disposes every queued
'     frame of an older generation and raises the internal floor, so both
'     enqueue and dequeue sides drop stale frames FOREVER AFTER — the
'     stale-frame-after-seek guarantee.
'   - Dequeue purges stale frames from the head before handing anything out
'     (the presenter never sees, and never has to check, stale frames).
'   - Pause/Stop reuse Flush: pause drains via clock-freeze (render side),
'     stop disposes everything (queue.Dispose disposes queued frames).
'   - Dispose is one-shot; queued frames are disposed exactly once; metrics
'     Enqueued/Dequeued/DroppedStale/DroppedOnFlush/DisposedClosed make the
'     leak invariant (every enqueued frame accounted) testable.
'
' Threading: Monitor-based condition variable (Wait/PulseAll) — single lock,
' no semaphore/token juggling, waiters wake on enqueue/dequeue/flush/dispose.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading

Namespace Gallery.Video

    Public NotInheritable Class FrameQueue
        Implements IDisposable

        Private ReadOnly _capacity As Integer
        Private ReadOnly _lock As New Object()
        Private ReadOnly _slots As New Queue(Of PlaybackFrame)()

        ''' <summary>Frames with Generation &lt; floor are stale everywhere.</summary>
        Private _generationFloor As Long = 0
        Private _disposedState As Integer = 0

        ' ---- Metrics (leak-invariant evidence, Interlocked-safe) ----
        Private _enqueuedCount As Long
        Private _enqueuedRejectedCount As Long
        Private _dequeuedCount As Long
        Private _droppedStaleCount As Long      ' purged on enqueue/dequeue because old generation
        Private _droppedOnFlushCount As Long    ' disposed by an explicit Flush
        Private _disposedByCloseCount As Long   ' disposed by queue.Dispose
        Private _framesDisposedCount As Long    ' every Dispose call we performed
        Private _peakCount As Long

        Public Sub New(capacity As Integer)
            If capacity < 1 Then Throw New ArgumentOutOfRangeException(NameOf(capacity), "capacity must be >= 1")
            _capacity = capacity
        End Sub

        Public ReadOnly Property Capacity As Integer
            Get
                Return _capacity
            End Get
        End Property

        Public ReadOnly Property Count As Integer
            Get
                SyncLock _lock
                    Return _slots.Count
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property GenerationFloor As Long
            Get
                SyncLock _lock
                    Return _generationFloor
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return Volatile.Read(_disposedState) <> 0
            End Get
        End Property

        ' ---- Metrics readers (diagnostics + tests) ----
        Public ReadOnly Property EnqueuedCount As Long
            Get
                Return Volatile.Read(_enqueuedCount)
            End Get
        End Property

        Public ReadOnly Property EnqueuedRejectedCount As Long
            Get
                Return Volatile.Read(_enqueuedRejectedCount)
            End Get
        End Property

        Public ReadOnly Property DequeuedCount As Long
            Get
                Return Volatile.Read(_dequeuedCount)
            End Get
        End Property

        Public ReadOnly Property DroppedStaleCount As Long
            Get
                Return Volatile.Read(_droppedStaleCount)
            End Get
        End Property

        Public ReadOnly Property DroppedOnFlushCount As Long
            Get
                Return Volatile.Read(_droppedOnFlushCount)
            End Get
        End Property

        Public ReadOnly Property DisposedByCloseCount As Long
            Get
                Return Volatile.Read(_disposedByCloseCount)
            End Get
        End Property

        Public ReadOnly Property FramesDisposedCount As Long
            Get
                Return Volatile.Read(_framesDisposedCount)
            End Get
        End Property

        Public ReadOnly Property PeakCount As Long
            Get
                Return Volatile.Read(_peakCount)
            End Get
        End Property

        ''' <summary>
        ''' Enqueue a frame. Blocks while the queue is full (backpressure) up to
        ''' timeoutMs. Ownership TRANSFERS to the queue on success. Returns False
        ''' (and disposes the frame, keeping the one-dispose invariant) when:
        ''' queue closed, frame is stale (older generation than floor), or timeout.
        ''' </summary>
        Public Function TryEnqueue(frame As PlaybackFrame, timeoutMs As Integer) As Boolean
            If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))
            Dim deadline = Stopwatch.GetTimestamp() + CLng(timeoutMs / 1000.0 * Stopwatch.Frequency)

            SyncLock _lock
                While True
                    If Volatile.Read(_disposedState) <> 0 Then
                        _enqueuedRejectedCount += 1
                        DisposeFrameLocked(frame)
                        Return False
                    End If

                    ' A producer that slept through a flush may wake holding a
                    ' stale frame — verify generation AFTER every wake.
                    If frame.Generation < _generationFloor Then
                        _enqueuedRejectedCount += 1
                        DisposeFrameLocked(frame)
                        Monitor.PulseAll(_lock)
                        Return False
                    End If

                    If _slots.Count < _capacity Then
                        _slots.Enqueue(frame)
                        _enqueuedCount += 1
                        If _slots.Count > _peakCount Then _peakCount = _slots.Count
                        Monitor.PulseAll(_lock)
                        Return True
                    End If

                    Dim remainingTicks = deadline - Stopwatch.GetTimestamp()
                    If remainingTicks <= 0 Then
                        ' Backpressure timeout: producer gives up; frame disposed
                        ' (never leaked, never double-disposed later).
                        _enqueuedRejectedCount += 1
                        DisposeFrameLocked(frame)
                        Monitor.PulseAll(_lock)
                        Return False
                    End If

                    Dim remainingMs = CInt(remainingTicks / Stopwatch.Frequency * 1000.0)
                    Monitor.Wait(_lock, Math.Max(1, remainingMs))
                End While

                ' Unreachable (loop exits only via Return) — compiler-visible
                ' fallback so BC42353 stays silent.
                Return False
            End SyncLock
        End Function

        ''' <summary>
        ''' Dequeue the oldest non-stale frame. Blocks up to timeoutMs. Ownership
        ''' TRANSFERS to the caller on success — caller MUST Dispose.
        ''' Stale frames at the head are disposed here and never surface.
        ''' </summary>
        Public Function TryDequeue(timeoutMs As Integer, ByRef frame As PlaybackFrame) As Boolean
            frame = Nothing
            Dim deadline = Stopwatch.GetTimestamp() + CLng(timeoutMs / 1000.0 * Stopwatch.Frequency)

            SyncLock _lock
                While True
                    ' Purge stale head frames (flush may have landed while the
                    ' queue held them). The presenter NEVER sees stale frames.
                    While _slots.Count > 0 AndAlso _slots.Peek().Generation < _generationFloor
                        Dim stale = _slots.Dequeue()
                        _droppedStaleCount += 1
                        DisposeFrameLocked(stale)
                    End While

                    If _slots.Count > 0 Then
                        frame = _slots.Dequeue()
                        _dequeuedCount += 1
                        Monitor.PulseAll(_lock) ' wake blocked producers (slot freed)
                        Return True
                    End If

                    If Volatile.Read(_disposedState) <> 0 Then Return False

                    Dim remainingTicks = deadline - Stopwatch.GetTimestamp()
                    If remainingTicks <= 0 Then Return False
                    Dim remainingMs = CInt(remainingTicks / Stopwatch.Frequency * 1000.0)
                    Monitor.Wait(_lock, Math.Max(1, remainingMs))
                End While

                ' Unreachable (loop exits only via Return) — compiler-visible
                ' fallback so BC42353 stays silent.
                Return False
            End SyncLock
        End Function

        ''' <summary>
        ''' Raise the generation floor and dispose every queued frame older than
        ''' it. Called on Seek (minGeneration = new generation) and Stop/Dispose
        ''' (minGeneration = Long.MaxValue via Dispose). Idempotent.
        ''' </summary>
        Public Sub Flush(minGeneration As Long)
            SyncLock _lock
                If minGeneration > _generationFloor Then _generationFloor = minGeneration
                While _slots.Count > 0 AndAlso _slots.Peek().Generation < _generationFloor
                    Dim stale = _slots.Dequeue()
                    _droppedOnFlushCount += 1
                    DisposeFrameLocked(stale)
                End While
                Monitor.PulseAll(_lock)
            End SyncLock
        End Sub

        ''' <summary>One-shot close. Disposes every queued frame exactly once,
        ''' wakes all waiters (they observe IsDisposed and exit cleanly).</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _lock
                If Interlocked.CompareExchange(_disposedState, 1, 0) <> 0 Then Return
                While _slots.Count > 0
                    Dim f = _slots.Dequeue()
                    _disposedByCloseCount += 1
                    DisposeFrameLocked(f)
                End While
                Monitor.PulseAll(_lock)
            End SyncLock
        End Sub

        ''' <summary>Dispose a frame under the lock, counting every actual Dispose
        ''' the queue performs. Leak invariant: every frame ACCEPTED into the
        ''' queue exits through exactly one of Dequeued / DroppedStale /
        ''' DroppedOnFlush / DisposedByClose, and each exit disposes exactly
        ''' once:  Enqueued == Dequeued + DroppedStale + DroppedOnFlush + Closed.
        ''' Frames rejected on enqueue never entered; they count only in
        ''' EnqueuedRejected. Never throws — repo dispose pattern.</summary>
        Private Sub DisposeFrameLocked(frame As PlaybackFrame)
            Try
                frame.Dispose()
                _framesDisposedCount += 1
            Catch
                ' Swallow — never throw from a disposal path.
            End Try
        End Sub

    End Class

End Namespace
