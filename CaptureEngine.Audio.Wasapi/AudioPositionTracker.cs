// AudioPositionTracker.cs — P13.2: the PURE, Linux-testable half of the
// position design (no COM, no threads, deterministic).
//
// ★ P13.4c (field evidence, OWNER runs 2026-08-28/29): the timeline moved
// from qpcPosition deltas to DevicePositionFrames deltas. The OWNER's
// endpoint fired 933 BACKWARDS qpc stamps in one 38s session and the
// qpc-delta timeline lost 5.65s of device time (track 32.93s vs session
// 38.56s) — qpcPosition measures WHEN the position was SAMPLED, not where
// the content is; sampling jitter (and lying drivers: USB/BT/DSP) makes
// qpc deltas useless as a content clock. DevicePositionFrames is the
// RENDER CURSOR — it advances with rendered content (silence included),
// which is exactly the content timeline this tracker exists to build.
// qpcPosition is demoted to the WALL ANCHOR (FirstQpc100ns → SyncMath)
// plus an anomaly counter (QpcAnomalies — evidence, no timeline impact).
//
// Consumes (frames, devicePositionFrames, qpcPosition100ns) per packet:
//
//   lastDevPosEnd = devPos + frames        (render cursor, END of content)
//   hole100ns     = (devPos − lastDevPosEnd) × 10⁷ / rate   (CONTENT time!)
//   bufferDur     = frames × 10⁷ / rate
//   lastEnd100ns  = FirstQpc + (lastDevPosEnd − firstDevPos) × 10⁷ / rate
//                   (content end mapped into the QPC wall domain)
//
// Hardening (doc §5 Risks, adapted to devPos):
//   Risk #2 — devicePositionFrames == 0 mid-stream ("no cursor"): assume
//   continuity (devPos = lastDevPosEnd), flag it, never fabricate gaps.
//   Backwards devPos (device re-init / endpoint switch): flagged as
//   monotonicity violations; timeline NEVER rewinds (max policy).
//   Re-anchor rule: if the ANCHOR packet had devPos == 0, the first REAL
//   cursor re-anchors with no hole (pre-reanchor absolute positions were
//   never known — a measured gap there would be stream-sized fiction).
//
// All arithmetic is integer 100ns/frames — no drift accumulates because
// the hole recomputes from each packet's OWN cursor, never by summing.

using System;

namespace CaptureEngine.Audio.Wasapi
{
    /// <summary>Outcome of feeding one packet to the tracker.</summary>
    public struct AudioGapReport
    {
        /// <summary>True for the first packet: it anchors the timeline.</summary>
        public bool AnchoredNow;

        /// <summary>CONTENT gap between the previous packet's cursor end and
        /// this packet's cursor start, in 100ns (0 when continuous). Consumers
        /// apply policy (AudioTap pads every hole; logs only > 50ms).</summary>
        public long Hole100ns;

        /// <summary>Duration of THIS packet, in 100ns.</summary>
        public long BufferDur100ns;

        /// <summary>This packet's cursor was missing (0) and continuity was
        /// assumed instead (doc §5 Risk #2 fallback).</summary>
        public bool StampFallbackUsed;

        /// <summary>This packet's cursor started before the previous content
        /// ended (overlap/re-init). Timeline did NOT rewind.</summary>
        public bool MonotonicViolation;

        /// <summary>A real cursor arrived after a fallback (zero-cursor)
        /// anchor: the timeline re-anchored onto it. No gap is measurable
        /// across a re-anchor.</summary>
        public bool ReAnchoredNow;

        /// <summary>The packet's qpcPosition went backwards vs the previous
        /// packet's — WALL-DOMAIN noise only (the OWNER machine: 933/session).
        /// Evidence counter; ZERO timeline impact since P13.4c.</summary>
        public bool QpcAnomaly;

        /// <summary>Cursor END after absorbing this packet (frames).</summary>
        public long LastDevPosEnd;

        /// <summary>Content end mapped into the QPC wall domain (100ns).</summary>
        public long LastEnd100ns;
    }

    /// <summary>Packet-timeline tracker over the device render cursor
    /// (DevicePositionFrames) — pure logic, serially fed.</summary>
    public sealed class AudioPositionTracker
    {
        private readonly long _sampleRate;
        private long _firstDevPos;
        private long _lastDevPosEnd;
        private long _firstQpc;
        private long _lastQpc;
        private bool _anchored;
        private bool _anchorWasFallback;
        private long _framesSinceAnchor;

        /// <summary>Sample rate of the device mix format (frames → time).</summary>
        public int SampleRate { get { return (int)_sampleRate; } }

        /// <summary>True once the first packet anchored the timeline.</summary>
        public bool Anchored { get { return _anchored; } }

        /// <summary>Wall anchor: the anchor packet's qpcPosition (100ns).
        /// 0 while the anchor is a zero-cursor fallback awaiting re-anchor.
        /// SyncMath maps video t0 against this.</summary>
        public long FirstQpc100ns { get { return _firstQpc; } }

        /// <summary>Cursor of the anchor packet (frames). Deltas are taken
        /// from here — absolute cursor values are meaningless, only spans.</summary>
        public long FirstDevPos { get { return _firstDevPos; } }

        /// <summary>END of the last absorbed packet's cursor span (frames).</summary>
        public long LastDevPosEnd { get { return _lastDevPosEnd; } }

        /// <summary>Content end mapped into the QPC wall domain (100ns):
        /// FirstQpc + (LastDevPosEnd − FirstDevPos) × 10⁷ / rate.</summary>
        public long LastEnd100ns { get; private set; }

        /// <summary>Total packets fed.</summary>
        public long Packets { get; private set; }

        /// <summary>Total frames fed.</summary>
        public long Frames { get; private set; }

        /// <summary>Packets that reported a hole > 0.</summary>
        public long GapPackets { get; private set; }

        /// <summary>Sum of all measured holes, 100ns.</summary>
        public long TotalHole100ns { get; private set; }

        /// <summary>Packets that needed the zero-cursor fallback (Risk #2).</summary>
        public long StampFallbacks { get; private set; }

        /// <summary>Packets whose CURSOR violated monotonicity (overlap or
        /// device re-init). Timeline never rewinds.</summary>
        public long MonotonicViolations { get; private set; }

        /// <summary>Packets whose qpcPosition went backwards — wall-domain
        /// sampling noise. Evidence only (P13.4c: the timeline no longer
        /// consumes qpc deltas at all).</summary>
        public long QpcAnomalies { get; private set; }

        public AudioPositionTracker(int sampleRate)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate),
                    "sampleRate must be > 0 (got " + sampleRate + ")");
            _sampleRate = sampleRate;
        }

        /// <summary>frames × 10⁷ / sampleRate, floored. Exact for real
        /// packets (480 @ 48000 = 100,000). Sub-100ns remainders floor;
        /// consumer gap thresholds absorb the ≤1-tick jitter.</summary>
        public long Duration100ns(int frames)
        {
            if (frames <= 0) return 0;
            return frames * 10_000_000L / _sampleRate;
        }

        /// <summary>cursorFrames × 10⁷ / sampleRate — content span in 100ns.
        /// Positive cursor deltas can be huge (36,000,000 frames/h); the
        /// intermediate product fits Int64 up to ~9.2e18 (≈ 4.4e13 frames).</summary>
        private long FramesTo100ns(long frames)
        {
            return frames / _sampleRate * 10_000_000L
                 + frames % _sampleRate * 10_000_000L / _sampleRate;
        }

        /// <summary>Feed one packet. Pure and thread-unsafe by design —
        /// the owner of the packet stream calls this serially.</summary>
        public AudioGapReport Feed(int frames, long devicePositionFrames, long qpcPosition100ns)
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

            // qpc anomaly evidence — wall-domain noise, no timeline effect.
            if (Packets > 1 && qpcPosition100ns < _lastQpc)
            {
                r.QpcAnomaly = true;
                QpcAnomalies++;
            }
            if (qpcPosition100ns > 0) _lastQpc = qpcPosition100ns;

            if (!_anchored)
            {
                // First packet: anchor, no gap possible.
                _anchored = true;
                _firstDevPos = devicePositionFrames;
                _firstQpc = qpcPosition100ns;
                if (devicePositionFrames == 0)
                {
                    // Pathological: first packet has no cursor. The qpc is
                    // still the true wall position of this content — anchor
                    // on it; the cursor base is backfilled on the first
                    // real cursor (the re-anchor branch below).
                    r.StampFallbackUsed = true;
                    StampFallbacks++;
                    _anchorWasFallback = true;
                    _framesSinceAnchor = frames;
                }
                _lastDevPosEnd = devicePositionFrames + frames;
                LastEnd100ns = _firstQpc + dur;
                r.AnchoredNow = true;
                r.Hole100ns = 0;
                r.LastDevPosEnd = _lastDevPosEnd;
                r.LastEnd100ns = LastEnd100ns;
                return r;
            }

            if (devicePositionFrames == 0)
            {
                // Risk #2: "no cursor". Assume continuity — this packet's
                // content starts exactly where the last one ended.
                r.StampFallbackUsed = true;
                StampFallbacks++;
                r.Hole100ns = 0;
                _lastDevPosEnd += frames;
                LastEnd100ns += dur;
                _framesSinceAnchor += frames;
                r.LastDevPosEnd = _lastDevPosEnd;
                r.LastEnd100ns = LastEnd100ns;
                return r;
            }

            if (_anchorWasFallback)
            {
                // First REAL cursor after a cursorless anchor. The anchor's
                // qpc IS the true wall position of the first content (the
                // packet existed — only its cursor was missing), so the
                // anchor stays; the cursor base is BACKFILLED from the
                // first real cursor assuming continuity:
                //   impliedFirstDevPos = devPos − Σ(cursorless frames)
                // After the backfill the normal cursor-delta logic runs —
                // an honest driver yields hole=0 here; a driver that jumped
                // yields a REAL hole that MUST be padded.
                _anchorWasFallback = false;
                long implied = devicePositionFrames - _framesSinceAnchor;
                _firstDevPos = implied >= 0 ? implied : 0;
                // Rebase the continuity-maintained end onto the real cursor
                // domain, or the delta below would fabricate an `implied`
                // sized hole. Continuity is the assumption here — the cursor
                // was absent, so no gap is measurable across the streak.
                _lastDevPosEnd = devicePositionFrames;
                r.ReAnchoredNow = true;
                r.Hole100ns = 0;
                // fall through to the normal cursor-delta logic below
            }
            else
            {
                r.Hole100ns = 0;
            }

            long holeFrames = devicePositionFrames - _lastDevPosEnd;
            if (holeFrames < 0)
            {
                // Backwards/overlapping cursor (device re-init): report it,
                // never rewind — max policy on the cursor end.
                r.MonotonicViolation = true;
                MonotonicViolations++;
                r.Hole100ns = 0;
                long newEnd = devicePositionFrames + frames;
                if (newEnd > _lastDevPosEnd) _lastDevPosEnd = newEnd;
            }
            else
            {
                r.Hole100ns = FramesTo100ns(holeFrames);
                if (holeFrames > 0) { GapPackets++; TotalHole100ns += r.Hole100ns; }
                _lastDevPosEnd = devicePositionFrames + frames;
            }

            LastEnd100ns = _firstQpc + FramesTo100ns(_lastDevPosEnd - _firstDevPos);
            r.LastDevPosEnd = _lastDevPosEnd;
            r.LastEnd100ns = LastEnd100ns;
            return r;
        }

        /// <summary>Forget the timeline (device switch / hot-unplug —
        /// doc §3.2: "log it, don't bridge it"). Counters stay for evidence.</summary>
        public void Reset()
        {
            _anchored = false;
            _firstDevPos = 0;
            _lastDevPosEnd = 0;
            _firstQpc = 0;
            _anchorWasFallback = false;
            _framesSinceAnchor = 0;
        }
    }
}
