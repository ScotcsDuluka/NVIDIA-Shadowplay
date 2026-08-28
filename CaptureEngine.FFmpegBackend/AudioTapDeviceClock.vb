Option Strict On
Option Explicit On
Option Infer On

' AudioTapDeviceClock.vb — AudioTap v3: gap-fill from MEASURED device time.
'
' PHASE 13.3 (ShadowPlay single-clock design, docs/PHASE-13-SHADOWPLAY-CLOCK.md
' §3.2). This is the ClockMode=Device twin of the legacy AudioTap:
'
'   legacy (v2): Feed(buffer, count) — silence from STOPWATCH ARRIVAL deltas
'                (guess), pre-roll from MarkStart call time, clock steering,
'                drift baselines, idle gates — a graveyard of band-aids for
'                one missing timestamp.
'
'   v3 (this file): Feed(buffer, count, qpcPosition100ns) — WASAPI already
'                stamps every packet with the device's QPC of its first
'                frame (P13.1 evidence). The gap between packets is measured
'                by HARDWARE, not guessed: hole = qpc − lastEnd (END→START,
'                valid for variable packet sizes — P13.2 precision note).
'                Silence math keys off qpcPosition deltas ONLY — never off
'                the SILENT flag bit (Model S: the flag appears only on the
'                stream-start packet; the endpoint renders silence through
'                quiet phases and the timeline advances on its own).
'
' Delegated to AudioPositionTracker (P13.2, 13 synthetic-position tests):
' hole measurement, Risk-#2 zero-stamp fallback (continuity, no bogus gap),
' backwards-stamp max policy (never rewind), fallback-anchor re-anchor rule
' (no stream-sized fake hole on stampless-then-stamping drivers).
'
' KEPT from legacy (proven rules): block-align clamp, >0.05s minimum-gap
' threshold, 3600s cap, zero silence buffer, evidence logging, dual sink
' (WAV sidecar + live-mux pipe).
'
' DELETED (impossible by construction here): clock steering, drift
' baseline, idle gate, Stopwatch arrival logic, MarkStart pre-roll
' guessing, FinalizeToNow wall-clock estimation (tail is now padded to
' the caller's session-end QPC — exact).
'
' Platform: pure .NET — runs on Linux CI with synthetic positions.

Imports System.Threading
Imports CaptureEngine.Audio.Wasapi

Namespace CaptureEngine.FFmpegBackend

    ''' <summary>
    ''' Device-clock audio tap (v3): one capture track's gap-fill engine,
    ''' driven by WASAPI qpcPosition stamps. NOT thread-safe beyond the
    ''' packet-delivery thread + one Finalize call (same contract as legacy).
    ''' </summary>
    Public NotInheritable Class AudioTapDeviceClock

        ''' <summary>Holes shorter than this are ignored (packet jitter,
        ''' sub-threshold scheduler noise). Proven legacy value.</summary>
        Public Const MinGapSec As Double = 0.05

        ''' <summary>Holes longer than this are dropped entirely (a stalled
        ''' device is a log event, not minutes of padding). Proven value.</summary>
        Public Const MaxGapSec As Double = 3600.0

        Private ReadOnly _name As String
        Private ReadOnly _bytesPerFrame As Integer        ' block align
        Private ReadOnly _bytesPerSec As Integer
        Private ReadOnly _sink As IAudioTapSink
        Private ReadOnly _evidence As Action(Of String)
        Private ReadOnly _tracker As AudioPositionTracker

        Private _silenceBuf As Byte() = Nothing
        Private _silenceCap As Integer = 0
        Private _silenceInsertedBytes As Long = 0
        Private _dataBytes As Long = 0

        ''' <summary>
        ''' Create the tap. The (sampleRate, channels, bitsPerSample) triple
        ''' must describe the BYTES that will be passed to Feed (post-
        ''' conversion PCM16 when the caller converts the float mix format),
        ''' because frame counts are derived from byte counts. The TRACKER
        ''' runs at the device sample rate — qpcPosition deltas are time,
        ''' independent of the byte format.
        ''' </summary>
        Public Sub New(name As String,
                       sampleRate As Integer,
                       channels As Integer,
                       bitsPerSample As Integer,
                       sink As IAudioTapSink,
                       Optional evidence As Action(Of String) = Nothing)
            If String.IsNullOrEmpty(name) Then Throw New ArgumentNullException(NameOf(name))
            If sampleRate <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(sampleRate))
            If channels <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(channels))
            If sink Is Nothing Then Throw New ArgumentNullException(NameOf(sink))
            _name = name
            _bytesPerFrame = Math.Max(1, channels * (bitsPerSample \ 8))
            _bytesPerSec = sampleRate * _bytesPerFrame
            _sink = sink
            _evidence = evidence
            _tracker = New AudioPositionTracker(sampleRate)
        End Sub

        ' ── Evidence surface (legacy-compatible names) ──────────────────

        Public ReadOnly Property SilenceInsertedBytes As Long
            Get
                Return Interlocked.Read(_silenceInsertedBytes)
            End Get
        End Property

        Public ReadOnly Property DataBytes As Long
            Get
                Return Interlocked.Read(_dataBytes)
            End Get
        End Property

        Public ReadOnly Property TotalDurationSec As Double
            Get
                Return (Interlocked.Read(_silenceInsertedBytes) + Interlocked.Read(_dataBytes)) / CDbl(_bytesPerSec)
            End Get
        End Property

        ''' <summary>True once the first stamped packet anchored the timeline.</summary>
        Public ReadOnly Property Anchored As Boolean
            Get
                Return _tracker.Anchored
            End Get
        End Property

        ''' <summary>Device QPC (100ns) of the FIRST frame ever fed — the
        ''' SyncMath v2 audio anchor. 0 while unanchored.</summary>
        Public ReadOnly Property FirstQpc100ns As Long
            Get
                Return _tracker.FirstQpc100ns
            End Get
        End Property

        ''' <summary>END stamp of the last packet fed (100ns). 0 while unanchored.</summary>
        Public ReadOnly Property LastEnd100ns As Long
            Get
                Return _tracker.LastEnd100ns
            End Get
        End Property

        Public ReadOnly Property Packets As Long
            Get
                Return _tracker.Packets
            End Get
        End Property

        Public ReadOnly Property HolePackets As Long
            Get
                Return _tracker.GapPackets
            End Get
        End Property

        Public ReadOnly Property TotalHole100ns As Long
            Get
                Return _tracker.TotalHole100ns
            End Get
        End Property

        Public ReadOnly Property StampFallbacks As Long
            Get
                Return _tracker.StampFallbacks
            End Get
        End Property

        Public ReadOnly Property MonotonicViolations As Long
            Get
                Return _tracker.MonotonicViolations
            End Get
        End Property

        ' ── The v3 entry point ──────────────────────────────────────────

        ''' <summary>
        ''' Feed one raw PCM buffer together with the WASAPI hardware stamp
        ''' of its first frame (100ns domain, as normalized by
        ''' WasapiPositionCapture). Inserts measured-gap silence BEFORE the
        ''' buffer, then forwards both to the sink. Call from the packet
        ''' delivery thread, serially.
        ''' </summary>
        Public Sub Feed(buffer As Byte(), count As Integer, qpcPosition100ns As Long)
            If buffer Is Nothing OrElse count <= 0 OrElse count > buffer.Length Then Return

            Dim frames As Integer = count \ _bytesPerFrame
            If frames <= 0 Then Return

            Dim report As AudioGapReport = _tracker.Feed(frames, qpcPosition100ns)

            If report.ReAnchoredNow Then
                _evidence?.Invoke($"[tap3:{_name}] re-anchored after fallback anchor (driver began stamping) — no hole fabricated")
            ElseIf report.AnchoredNow Then
                _evidence?.Invoke($"[tap3:{_name}] anchored at qpc {report.LastEnd100ns - report.BufferDur100ns} (100ns); buffer {report.BufferDur100ns / 100000.0:0.0}ms")
            End If

            ' The hole (END→START) IS the silence. Same thresholds as the
            ' proven legacy tap: ignore sub-50ms jitter, drop >1h craters.
            If report.Hole100ns > 0 Then
                Dim holeSec As Double = report.Hole100ns / 10000000.0
                If holeSec > MinGapSec AndAlso holeSec <= MaxGapSec Then
                    Dim silBytes As Integer = CInt(holeSec * _bytesPerSec)
                    silBytes -= (silBytes Mod _bytesPerFrame)
                    If silBytes > 0 Then
                        WriteSilence(silBytes)
                        _evidence?.Invoke($"[tap3:{_name}] measured hole {holeSec * 1000.0:0}ms → padded {silBytes}B silence")
                    End If
                ElseIf holeSec > MaxGapSec Then
                    _evidence?.Invoke($"[tap3:{_name}] hole {holeSec:0.0}s exceeds {MaxGapSec:0}s cap — NOT padded (device stall?)")
                End If
            End If

            If report.MonotonicViolation Then
                _evidence?.Invoke($"[tap3:{_name}] backwards stamp absorbed (no rewind)")
            End If
            If report.StampFallbackUsed Then
                _evidence?.Invoke($"[tap3:{_name}] zero-stamp packet — continuity assumed (no hole)")
            End If

            _sink.Write(buffer, count)
            Interlocked.Add(_dataBytes, count)
        End Sub

        ''' <summary>
        ''' Close the timeline at stop: pad silence from the last packet's
        ''' END to the session-end QPC (exact — no wall-clock estimation).
        ''' Pass the session start QPC as well: if no packet ever arrived,
        ''' the whole span is padded so the mux still gets a valid silent
        ''' track instead of zero packets.
        ''' Stamps in 100ns (use WasapiPositionCapture.QpcTicksTo100ns on
        ''' Stopwatch.GetTimestamp() values — same QPC domain).
        ''' </summary>
        Public Sub FinalizeTo100ns(sessionStartQpc100ns As Long, sessionEndQpc100ns As Long)
            If sessionEndQpc100ns <= sessionStartQpc100ns Then Return

            If Not _tracker.Anchored Then
                PadSilence((sessionEndQpc100ns - sessionStartQpc100ns) / 10000000.0,
                           "never-anchored track — full-span silence")
                Return
            End If

            Dim tail100ns As Long = sessionEndQpc100ns - _tracker.LastEnd100ns
            If tail100ns > 0 Then
                PadSilence(tail100ns / 10000000.0, "tail to session end")
            End If
            _evidence?.Invoke($"[tap3:{_name}] closed: data={Interlocked.Read(_dataBytes):N0}B silence={Interlocked.Read(_silenceInsertedBytes):N0}B total={TotalDurationSec:0.00}s holes={HolePackets} fallbacks={StampFallbacks} violations={MonotonicViolations}")
        End Sub

        ' ── Internals ───────────────────────────────────────────────────

        Private Sub WriteSilence(silBytes As Integer)
            If _silenceBuf Is Nothing OrElse _silenceCap < silBytes Then
                _silenceCap = Math.Max(silBytes, 65536)
                _silenceBuf = New Byte(_silenceCap - 1) {}
            End If
            Dim off As Integer = 0
            While off < silBytes
                Dim n As Integer = Math.Min(silBytes - off, _silenceBuf.Length)
                _sink.Write(_silenceBuf, n)
                Interlocked.Add(_silenceInsertedBytes, n)
                off += n
            End While
        End Sub

        Private Sub PadSilence(sec As Double, reason As String)
            If sec <= 0 OrElse sec > MaxGapSec Then Return
            Dim silBytes As Long = CLng(sec * _bytesPerSec)
            silBytes -= (silBytes Mod _bytesPerFrame)
            If silBytes <= 0 Then Return
            If _silenceBuf Is Nothing Then
                _silenceCap = Math.Max(CInt(Math.Min(silBytes, 1048576L)), 65536)
                _silenceBuf = New Byte(_silenceCap - 1) {}
            End If
            Dim off As Long = 0
            While off < silBytes
                Dim n As Integer = CInt(Math.Min(silBytes - off, _silenceBuf.Length))
                _sink.Write(_silenceBuf, n)
                Interlocked.Add(_silenceInsertedBytes, n)
                off += n
            End While
            _evidence?.Invoke($"[tap3:{_name}] padded {sec:0.00}s silence ({reason})")
        End Sub

    End Class

End Namespace
