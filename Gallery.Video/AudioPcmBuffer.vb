Option Strict On
Option Explicit On
Option Infer On

' AudioPcmBuffer.vb — bounded S16LE PCM ring between the audio decode pipe
' and the audio output (design doc §3.4 audio ring ≈ 200 ms).
'
' Semantics:
'   - BOUNDED: Write never grows beyond capacity. On overflow the OLDEST
'     bytes are dropped (audio late-drop is the standard playback behavior —
'     the listener cares about NOW, not 2s ago) and counted.
'   - Read returns what is available up to the request; optional min-wait
'     for the device thread (avoids busy-spin inside WasapiOut callbacks).
'   - Sample-count accounting (played samples ÷ rate) is the master clock
'     source when audio is present (§3.6).
'   - No locks held during I/O outside the tiny ring copy; single lock.

Imports System
Imports System.Threading

Namespace Gallery.Video

    Public NotInheritable Class AudioPcmBuffer
        Implements IDisposable

        Private ReadOnly _buffer As Byte()
        Private ReadOnly _lock As New Object()
        Private _writePos As Integer
        Private _readPos As Integer
        Private _count As Integer
        Private _disposed As Integer = 0

        ' Metrics
        Private _bytesWritten As Long
        Private _bytesRead As Long
        Private _bytesDroppedOldest As Long

        Public Sub New(capacityBytes As Integer)
            If capacityBytes < 2 OrElse capacityBytes Mod 2 <> 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(capacityBytes), "capacity must be even (S16 samples)")
            End If
            _buffer = New Byte(capacityBytes - 1) {}
        End Sub

        Public ReadOnly Property CapacityBytes As Integer
            Get
                Return _buffer.Length
            End Get
        End Property

        Public ReadOnly Property BufferedBytes As Integer
            Get
                SyncLock _lock
                    Return _count
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property BytesWritten As Long
            Get
                Return Volatile.Read(_bytesWritten)
            End Get
        End Property

        Public ReadOnly Property BytesRead As Long
            Get
                Return Volatile.Read(_bytesRead)
            End Get
        End Property

        Public ReadOnly Property BytesDroppedOldest As Long
            Get
                Return Volatile.Read(_bytesDroppedOldest)
            End Get
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return Volatile.Read(_disposed) <> 0
            End Get
        End Property

        ''' <summary>Append PCM bytes. Oldest bytes dropped on overflow (counted).
        ''' Returns False when the buffer is disposed (writer exits cleanly).</summary>
        Public Function Write(data As Byte(), offset As Integer, count As Integer) As Boolean
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If count <= 0 Then Return Not IsDisposed

            SyncLock _lock
                If Volatile.Read(_disposed) <> 0 Then Return False

                ' Whole-buffer overflow: clamp to newest data (drop-oldest).
                If count > _buffer.Length Then
                    Dim skip = count - _buffer.Length
                    offset += skip
                    count -= skip
                    _bytesDroppedOldest += skip
                End If

                ' Make room, dropping oldest.
                Dim overflow = _count + count - _buffer.Length
                If overflow > 0 Then
                    _readPos = (_readPos + overflow) Mod _buffer.Length
                    _count -= overflow
                    _bytesDroppedOldest += overflow
                End If

                ' Two-segment copy (wrap-aware).
                Dim first = Math.Min(count, _buffer.Length - _writePos)
                Array.Copy(data, offset, _buffer, _writePos, first)
                If count > first Then
                    Array.Copy(data, offset + first, _buffer, 0, count - first)
                End If
                _writePos = (_writePos + count) Mod _buffer.Length
                _count += count
                _bytesWritten += count
                Monitor.PulseAll(_lock)
                Return True
            End SyncLock
        End Function

        ''' <summary>Read up to `count` bytes. When fewer than `minBytes` are
        ''' available, waits up to `waitMs` for at least `minBytes` (returns
        ''' whatever is there afterwards — possibly 0). Never throws when
        ''' disposed; returns 0.</summary>
        Public Function Read(destination As Byte(), offset As Integer, count As Integer,
                             minBytes As Integer, waitMs As Integer) As Integer
            If destination Is Nothing Then Throw New ArgumentNullException(NameOf(destination))
            If count <= 0 Then Return 0

            Dim deadline = DateTime.UtcNow.AddMilliseconds(waitMs)

            SyncLock _lock
                While _count < minBytes AndAlso _count < count
                    If Volatile.Read(_disposed) <> 0 Then Return 0
                    Dim remainingMs = CInt((deadline - DateTime.UtcNow).TotalMilliseconds)
                    If remainingMs <= 0 Then Exit While
                    Monitor.Wait(_lock, Math.Min(remainingMs, 50))
                End While

                Dim toRead = Math.Min(count, _count)
                If toRead <= 0 Then Return 0

                Dim first = Math.Min(toRead, _buffer.Length - _readPos)
                Array.Copy(_buffer, _readPos, destination, offset, first)
                If toRead > first Then
                    Array.Copy(_buffer, 0, destination, offset + first, toRead - first)
                End If
                _readPos = (_readPos + toRead) Mod _buffer.Length
                _count -= toRead
                _bytesRead += toRead
                Monitor.PulseAll(_lock)
                Return toRead
            End SyncLock
        End Function

        ''' <summary>Clear buffered audio (seek/pause protocol) — counters stay
        ''' (evidence), content goes (stale audio must never play).</summary>
        Public Sub Clear()
            SyncLock _lock
                _readPos = 0
                _writePos = 0
                _count = 0
                Monitor.PulseAll(_lock)
            End SyncLock
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _lock
                If Interlocked.CompareExchange(_disposed, 1, 0) <> 0 Then Return
                Monitor.PulseAll(_lock)
            End SyncLock
        End Sub

    End Class

End Namespace
