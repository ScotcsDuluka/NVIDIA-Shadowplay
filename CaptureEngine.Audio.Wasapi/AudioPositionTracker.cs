// AudioPositionTracker.cs — P13.2: the PURE, Linux-testable half of the
// position design (no COM, no threads, deterministic).
//
// Consumes (frames, qpcPosition100ns) per packet and maintains the packet
// timeline the way AudioTap v3 (P13.3) will consume it:
//
//   lastEnd100ns  = END of the last packet's content  (lastQpc + lastDur)
//   hole100ns     = qpc − lastEnd                     (MEASURED by hardware)
//   bufferDur     = frames × 10^7 / sampleRate        (this packet's span)
//
// Semantics note: PHASE-13-SHADOWPLAY-CLOCK.md §3.2's pseudo-code measured
// lastEnd as the previous packet's START; the tracker uses END-to-START
// (hole is exactly the silence AudioTap must pad before writing the packet's
// bytes). For uniform 10ms packets the two agree; for variable sizes END-to-
// START is the correct one. The doc gets a precision pass in P13.3.
//
// All arithmetic is integer 100ns — no drift can accumulate because lastEnd
// is recomputed from each packet's OWN stamp, never by summing increments.
//
// Hardening covered (doc §5 Risks):
//   Risk #2 — qpcPosition == 0 on some drivers ("no stamp"): the tracker
//   assumes continuity (stamp = lastEnd), flags it, never fabricates gaps.
//   Backwards stamps: flagged as monotonicity violations; timeline never
//   rewinds (max policy), hole clamped to zero.
//   Re-anchor rule (stability pass): if the ANCHOR packet itself was
//   stampless (anchored at 0), the first REAL stamp re-anchors the
//   timeline with no hole — the pre-reanchor absolute positions were
//   never known, so "measuring" a hole there would fabricate one the
//   size of the whole stream (AudioTap would pad seconds of silence).

using System;

namespace CaptureEngine.Audio.Wasapi
{
    /// <summary>Outcome of feeding one packet to the tracker.</summary>
    public struct AudioGapReport
    {
        /// <summary>True for the first packet: it anchors the timeline
        /// (FirstQpc100ns becomes meaningful, no gap is possible).</summary>
        public bool AnchoredNow;

        /// <summary>Hole between the previous packet's END and this
        /// packet's START, in 100ns (0 when continuous). Raw measurement —
        /// consumers apply policy (AudioTap's 0.05s minimum threshold).</summary>
        public long Hole100ns;

        /// <summary>Duration of THIS packet, in 100ns.</summary>
        public long BufferDur100ns;

        /// <summary>This packet's stamp was missing (0) and continuity was
        /// assumed instead (doc §5 Risk #2 fallback).</summary>
        public bool StampFallbackUsed;

        /// <summary>This packet's stamp started before the previous
        /// content ended (overlap/backwards). Timeline did NOT rewind.</summary>
        public bool MonotonicViolation;

        /// <summary>A real hardware stamp arrived after a fallback
        /// (zero-stamp) anchor: the timeline re-anchored onto it. No gap
        /// is measurable across a re-anchor — absolute positions before
        /// it were unknown.</summary>
        public bool ReAnchoredNow;

        /// <summary>Timeline END after absorbing this packet.</summary>
        public long LastEnd100ns;
    }

    /// <summary>Packet-timeline tracker over hardware stamps (pure logic).</summary>
    public sealed class AudioPositionTracker
    {
        private readonly long _sampleRate;
        private long _lastEnd;
        private long _firstQpc;
        private bool _anchored;
        private bool _anchorWasFallback;

        /// <summary>Sample rate of the device mix format (frames → time).</summary>
        public int SampleRate { get { return (int)_sampleRate; } }

        /// <summary>True once the first packet anchored the timeline.</summary>
        public bool Anchored { get { return _anchored; } }

        /// <summary>Hardware stamp the timeline is anchored on (100ns):
        /// the first packet's stamp — or, if that packet was stampless,
        /// the first REAL stamp seen (re-anchor). 0 until anchored.</summary>
        public long FirstQpc100ns { get { return _firstQpc; } }

        /// <summary>END of the last absorbed packet's content (100ns).</summary>
        public long LastEnd100ns { get { return _lastEnd; } }

        /// <summary>Total packets fed.</summary>
        public long Packets { get; private set; }

        /// <summary>Total frames fed.</summary>
        public long Frames { get; private set; }

        /// <summary>Packets that reported a hole > 0.</summary>
        public long GapPackets { get; private set; }

        /// <summary>Sum of all measured holes, 100ns.</summary>
        public long TotalHole100ns { get; private set; }

        /// <summary>Packets that needed the zero-stamp fallback (Risk #2).</summary>
        public long StampFallbacks { get; private set; }

        /// <summary>Packets whose stamp violated monotonicity.</summary>
        public long MonotonicViolations { get; private set; }

        public AudioPositionTracker(int sampleRate)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate),
                    "sampleRate must be > 0 (got " + sampleRate + ")");
            _sampleRate = sampleRate;
        }

        /// <summary>frames × 10^7 / sampleRate, floored. Exact for real
        /// packets (480 @ 48000 = 10,000,000). Sub-100ns remainders floor;
        /// consumer gap thresholds absorb the ≤1-tick jitter.</summary>
        public long Duration100ns(int frames)
        {
            if (frames <= 0) return 0;
            return frames * 10_000_000L / _sampleRate;
        }

        /// <summary>Feed one packet. Pure and thread-unsafe by design —
        /// the owner of the packet stream calls this serially.</summary>
        public AudioGapReport Feed(int frames, long qpcPosition100ns)
        {
            if (frames < 0)
                throw new ArgumentOutOfRangeException(nameof(frames),
                    "frames must be >= 0 (got " + frames + ") — a negative " +
                    "count would silently corrupt the evidence counters");

            var r = new AudioGapReport();
            long dur = Duration100ns(frames);
            r.BufferDur100ns = dur;
            Packets++;
            Frames += frames;

            if (!_anchored)
            {
                // First packet: anchor, no gap possible.
                _anchored = true;
                _firstQpc = qpcPosition100ns;
                if (qpcPosition100ns == 0)
                {
                    // Pathological: first packet has no stamp. Anchor at 0
                    // and treat content as starting the timeline.
                    r.StampFallbackUsed = true;
                    StampFallbacks++;
                    _anchorWasFallback = true;
                }
                _lastEnd = qpcPosition100ns + dur;
                r.AnchoredNow = true;
                r.Hole100ns = 0;
                r.LastEnd100ns = _lastEnd;
                return r;
            }

            if (qpcPosition100ns == 0)
            {
                // Risk #2: "no stamp". Assume continuity — this packet's
                // content starts exactly where the last one ended.
                r.StampFallbackUsed = true;
                StampFallbacks++;
                r.Hole100ns = 0;
                _lastEnd = _lastEnd + dur;
                r.LastEnd100ns = _lastEnd;
                return r;
            }

            if (_anchorWasFallback)
            {
                // Stability pass: the anchor was a zero-stamp fallback, so
                // _lastEnd sits on a fictitious 0-based timeline. Comparing
                // a REAL stamp against it would report a hole the size of
                // the whole stream. Re-anchor instead: packet 1's absolute
                // position was never known, so no gap is measurable there.
                _anchorWasFallback = false;
                _firstQpc = qpcPosition100ns;
                r.ReAnchoredNow = true;
                r.Hole100ns = 0;
                _lastEnd = qpcPosition100ns + dur;
                r.LastEnd100ns = _lastEnd;
                return r;
            }

            long hole = qpcPosition100ns - _lastEnd;
            if (hole < 0)
            {
                // Backwards/overlapping stamp: report it, never rewind.
                r.MonotonicViolation = true;
                MonotonicViolations++;
                r.Hole100ns = 0;
                long end = qpcPosition100ns + dur;
                if (end > _lastEnd) _lastEnd = end;
                r.LastEnd100ns = _lastEnd;
                return r;
            }

            r.Hole100ns = hole;
            if (hole > 0) { GapPackets++; TotalHole100ns += hole; }
            _lastEnd = qpcPosition100ns + dur;
            r.LastEnd100ns = _lastEnd;
            return r;
        }

        /// <summary>Forget the timeline (device switch / hot-unplug —
        /// doc §3.2: "log it, don't bridge it"). Counters stay for evidence.</summary>
        public void Reset()
        {
            _anchored = false;
            _lastEnd = 0;
            _firstQpc = 0;
            _anchorWasFallback = false;
        }
    }
}
