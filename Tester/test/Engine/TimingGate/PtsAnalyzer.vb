Option Strict On
Option Explicit On
Option Infer On

' PtsAnalyzer.vb — W3 timing gate core (pure math, no I/O).
'
' Port of test-recordings\fps-matrix\analyze-pts.ps1 (M2-W3 forensic tool)
' into an assertable library. THE VFR/CADENCE VERDICT IS COMPUTED FROM THE
' ΔPTS DISTRIBUTION ONLY. The container's avg_frame_rate tag is carried as
' an informational string and is NEVER an input to any decision — a stream
' whose packets average to the target rate while carrying duplicate/gapped
' PTS must FAIL (pinned by W3-L0-V1).
'
' Thresholds are anchored on the M2-W3 measured baseline (2026-09-11,
' legacy ddagrab+qsv 5s×2 per mode): perfect CFR grids at 30/60/120/144,
' zero duplicate/negative deltas, effFPS == target exactly.

Namespace TimingGate.Tests

    ''' <summary>Container timebase the engine's MP4s use (1/15360 s). At
    ''' 144 fps (15360/144 non-integer) CFR deltas quantize within ±1 tick
    ''' of the target — TIMEBASE_SAWTOOTH, not a defect.</summary>
    Public NotInheritable Class PtsConstants
        Public Const TimebaseTicksPerSec As Double = 15360.0
        Public Shared ReadOnly TickSec As Double = 1.0 / TimebaseTicksPerSec
        ''' <summary>Δ ≤ ±1 µs is inside ffprobe's decimal print resolution
        ''' — such a step is a duplicate PTS, not real spacing.</summary>
        Public Const DupEpsilonSec As Double = 0.000001
    End Class

    Public Enum PtsClass
        GRID_OK
        TIMEBASE_SAWTOOTH
        START_OFFSET
        FIRST_GAP
        FRAME_COUNT
        RATE_DRIFT
        PACKET_MISMATCH
        DUPLICATE_PTS
        NEGATIVE_DELTA
        UNDECODABLE
    End Enum

    Public NotInheritable Class PtsReport
        Public Property Frames As Integer
        Public Property Packets As Integer = -1          ' -1 = not probed
        Public Property ContainerDurationSec As Double
        Public Property RequestedSec As Double
        Public Property TargetFps As Integer
        Public Property TargetDeltaSec As Double

        Public Property FirstDeltaMs As Double
        Public Property MedianDeltaMs As Double
        Public Property MeanDeltaMs As Double
        Public Property P95Ms As Double
        Public Property P99Ms As Double
        Public Property MinDeltaMs As Double
        Public Property MaxDeltaMs As Double
        Public Property EffFps As Double
        Public Property DupCount As Integer
        Public Property NegCount As Integer
        Public Property FirstPtsSec As Double

        ''' <summary>INFORMATIONAL ONLY — never an input to Verdict.</summary>
        Public Property ContainerAvgRate As String = ""

        Public Property PrimaryClass As PtsClass
        Public ReadOnly Property IsPass As Boolean
            Get
                Return PrimaryClass = PtsClass.GRID_OK OrElse
                       PrimaryClass = PtsClass.TIMEBASE_SAWTOOTH
            End Get
        End Property
        Public Property Notes As New List(Of String)()

        Public Overrides Function ToString() As String
            Dim pkt As String = If(Packets >= 0, Packets.ToString(), "n/a")
            Return $"class={PrimaryClass} frames={Frames} packets={pkt} dur={ContainerDurationSec:0.000}s " &
                   $"Δms first={FirstDeltaMs:0.000} med={MedianDeltaMs:0.000} mean={MeanDeltaMs:0.000} " &
                   $"P95={P95Ms:0.000} P99={P99Ms:0.000} min={MinDeltaMs:0.000} max={MaxDeltaMs:0.000} " &
                   $"eff={EffFps:0.000} dup={DupCount} neg={NegCount} avgTag='{ContainerAvgRate}' " &
                   $"verdict={If(IsPass, "PASS", "FAIL")}"
        End Function
    End Class

    Public NotInheritable Class PtsAnalyzer

        ''' <summary>Classify one stream's presentation timeline.
        ''' pts = per-frame presentation timestamps in seconds (ffprobe pts_time),
        ''' ascending index order as stored in the container.</summary>
        Public Shared Function Analyze(pts As Double(),
                                       targetFps As Integer,
                                       requestedSec As Double,
                                       containerDurationSec As Double,
                                       containerAvgRate As String,
                                       Optional packetCount As Integer = -1) As PtsReport

            Dim rep As New PtsReport() With {
                .TargetFps = targetFps,
                .RequestedSec = requestedSec,
                .ContainerDurationSec = containerDurationSec,
                .ContainerAvgRate = If(containerAvgRate, ""),
                .Packets = packetCount
            }
            rep.TargetDeltaSec = If(targetFps > 0, 1.0 / targetFps, 0.0)

            If pts Is Nothing OrElse pts.Length < 2 Then
                rep.PrimaryClass = PtsClass.UNDECODABLE
                rep.Notes.Add($"only {If(pts Is Nothing, 0, pts.Length)} decodable frames with PTS")
                Return rep
            End If
            rep.Frames = pts.Length
            rep.FirstPtsSec = pts(0)

            Dim n As Integer = pts.Length
            Dim deltas(n - 2) As Double
            For i As Integer = 1 To n - 1
                deltas(i - 1) = pts(i) - pts(i - 1)
            Next

            ' ── stats ──
            rep.FirstDeltaMs = deltas(0) * 1000.0
            rep.MeanDeltaMs = deltas.Average() * 1000.0
            Dim sorted As Double() = deltas.OrderBy(Function(d) d).ToArray()
            rep.MedianDeltaMs = Percentile(sorted, 0.5) * 1000.0
            rep.P95Ms = Percentile(sorted, 0.95) * 1000.0
            rep.P99Ms = Percentile(sorted, 0.99) * 1000.0
            rep.MinDeltaMs = sorted(0) * 1000.0
            rep.MaxDeltaMs = sorted(sorted.Length - 1) * 1000.0

            Dim span As Double = pts(n - 1) - pts(0)
            rep.EffFps = If(span > 0, (n - 1) / span, 0.0)

            Dim dup As Integer = 0, neg As Integer = 0
            For Each d As Double In deltas
                If Math.Abs(d) <= PtsConstants.DupEpsilonSec Then dup += 1
                If d < -PtsConstants.DupEpsilonSec Then neg += 1
            Next
            rep.DupCount = dup
            rep.NegCount = neg

            ' ── classification (priority = most severe first) ──
            ' 1. PTS ordering — the hard integrity floor.
            If neg > 0 Then
                rep.PrimaryClass = PtsClass.NEGATIVE_DELTA
                rep.Notes.Add($"{neg} negative-delta step(s)")
                Return rep
            End If
            If dup > 0 Then
                rep.PrimaryClass = PtsClass.DUPLICATE_PTS
                rep.Notes.Add($"{dup} duplicate-PTS step(s)")
                Return rep
            End If

            ' 2. encode→packet integrity: every encoded frame must have landed
            '    as exactly one packet in the container. (Container-duration vs
            '    frames/fps is deliberately NOT a separate rule: a cadence
            '    violation already shifts duration, and it must be diagnosed
            '    as RATE_DRIFT, not packet loss.)
            If packetCount >= 0 AndAlso packetCount <> n Then
                rep.PrimaryClass = PtsClass.PACKET_MISMATCH
                rep.Notes.Add($"packets={packetCount} ≠ decoded frames={n}")
                Return rep
            End If

            ' 3. cadence — median/mean/effective rate against the 1/fps grid.
            Dim cadenceTol As Double = Math.Max(0.05 * rep.TargetDeltaSec, PtsConstants.TickSec)
            If rep.TargetDeltaSec > 0 Then
                Dim medDev As Double = Math.Abs((rep.MedianDeltaMs / 1000.0) - rep.TargetDeltaSec)
                Dim meanDev As Double = Math.Abs((rep.MeanDeltaMs / 1000.0) - rep.TargetDeltaSec)
                Dim effDev As Double = Math.Abs(rep.EffFps - targetFps) / targetFps
                If medDev > cadenceTol OrElse meanDev > cadenceTol OrElse effDev > 0.02 Then
                    rep.PrimaryClass = PtsClass.RATE_DRIFT
                    rep.Notes.Add($"median dev {medDev * 1000.0:0.000}ms, effFPS dev {effDev:P2}")
                    Return rep
                End If
            End If

            ' 4. frame-count integrity against the target rate.
            Dim durRef As Double = If(containerDurationSec > 0, containerDurationSec, requestedSec)
            If durRef > 0 AndAlso targetFps > 0 Then
                Dim lo As Double = 0.7 * targetFps * durRef
                Dim hi As Double = 1.5 * targetFps * durRef
                If n < lo OrElse n > hi Then
                    rep.PrimaryClass = PtsClass.FRAME_COUNT
                    rep.Notes.Add($"frames {n} outside [{lo:0}/{hi:0}] (fps={targetFps}, durRef={durRef:0.000}s)")
                    Return rep
                End If
            End If

            ' 5. first-frame classification. FIRST_GAP fires ABOVE the grid by
            '    max(3 ticks, 12% of T) so the 38ms-class anomaly is caught even
            '    at 30 fps (T=33.3ms: 38ms → FIRST_GAP), while container tick
            '    quantization (±1 tick, e.g. 144fps) never trips it.
            If rep.TargetDeltaSec > 0 Then
                Dim firstGapLim As Double = rep.TargetDeltaSec + Math.Max(3.0 * PtsConstants.TickSec, 0.12 * rep.TargetDeltaSec)
                If deltas(0) > firstGapLim Then
                    rep.PrimaryClass = PtsClass.FIRST_GAP
                    rep.Notes.Add($"first Δ {rep.FirstDeltaMs:0.000}ms > grid+margin {firstGapLim * 1000.0:0.000}ms")
                    Return rep
                End If
                If pts(0) > 2.0 * PtsConstants.TickSec Then
                    rep.PrimaryClass = PtsClass.START_OFFSET
                    rep.Notes.Add($"first PTS {pts(0) * 1000.0:0.000}ms > 2 ticks from zero")
                    Return rep
                End If
            End If

            ' 6. container duration sanity vs the request (capture pacing).
            If requestedSec > 0 AndAlso containerDurationSec > 0 Then
                If containerDurationSec < 0.7 * requestedSec OrElse containerDurationSec > 1.5 * requestedSec Then
                    rep.PrimaryClass = PtsClass.FRAME_COUNT
                    rep.Notes.Add($"container duration {containerDurationSec:0.000}s outside [0.7,1.5]× request {requestedSec:0.000}s")
                    Return rep
                End If
            End If

            ' 7. TIMEBASE_SAWTOOTH — expected quantization, never a defect:
            '    deltas vary ≤ 2 ticks around the target while the container
            '    timebase cannot express 1/fps exactly (e.g. 15360/144).
            Dim ticksPerFrame As Double = If(targetFps > 0, PtsConstants.TimebaseTicksPerSec / targetFps, 0.0)
            Dim spreadTicks As Double = (sorted(sorted.Length - 1) - sorted(0)) / PtsConstants.TickSec
            If ticksPerFrame > 0 AndAlso Math.Abs(ticksPerFrame - Math.Round(ticksPerFrame)) > 0.0001 Then
                If spreadTicks <= 2.0 Then
                    rep.PrimaryClass = PtsClass.TIMEBASE_SAWTOOTH
                    rep.Notes.Add($"non-integer ticks/frame ({ticksPerFrame:0.000}), Δ spread {spreadTicks:0.0} ticks — container quantization")
                    Return rep
                End If
            End If

            rep.PrimaryClass = PtsClass.GRID_OK
            Return rep
        End Function

        Public Shared Function Percentile(sorted As Double(), p As Double) As Double
            If sorted Is Nothing OrElse sorted.Length = 0 Then Return 0.0
            If sorted.Length = 1 Then Return sorted(0)
            Dim idx As Double = p * (sorted.Length - 1)
            Dim lo As Integer = CInt(Math.Floor(idx))
            Dim hi As Integer = CInt(Math.Ceiling(idx))
            If lo = hi Then Return sorted(lo)
            Return sorted(lo) + (sorted(hi) - sorted(lo)) * (idx - lo)
        End Function

    End Class

End Namespace
