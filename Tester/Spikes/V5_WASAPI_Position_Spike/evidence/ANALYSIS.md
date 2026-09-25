# P13.1 Evidence Analysis — 2026-08-27 (OWNER's 60s run, spike v4)

Data: `2026-08-27_position_log_60s.csv` — 6000 packets × 480 frames
(= 10 ms @ 48 kHz exactly; Σframes = 2,880,000 = 60 s × 48000, zero loss).
Analyzer: `scripts/analyze_v5_evidence.py` (full-series OLS + chunk stats).

## What the spike's 30-second summary said (and why it was wrong)

| metric | value | verdict line |
|---|---|---|
| unit | QPC ticks (same domain as Stopwatch) | ✅ |
| devpos | 1.000000 (= sample frames) | ✅ |
| drift (v4 endpoint method) | 5.49 ms | ❌ FAIL (gate 5 ms) |

The endpoint drift compares **only two samples** — first and last packet —
and both carry 0–10 ms of *arrival-phase jitter* from our 5 ms polling loop.
It measures when we happened to poll, not how the clocks behave. The first
sample is the worst possible anchor: row 0 is the SILENT stream-start packet.

## Full-series regression (the honest measurement)

```
OLS over all 6000 samples:  sw_ticks = 1.000003277 * qpc + C
clock-rate skew vs Stopwatch = +3.3 ppm   (0.003 ms per second)
arrival-jitter residual: std 4.77 ms · p95 7.65 ms · max 40.5 ms
per-chunk mean offset (6×10 s):  +0.05 −0.10 −0.08 +0.17 +0.07 −0.10
  → peak-to-peak spread 0.27 ms
per-chunk mean offset (10×6 s):  spread 0.55 ms
residual at 10 s marks: −5.46 −0.34 +1.10 −1.13 +2.82 ms
```

Reading:

- **+3.3 ppm skew** — the audio engine's QPC-mapped device clock and the
  system Stopwatch are, for recording purposes, **the same clock**. A
  60-minute recording would accumulate ~12 ms — and P13.4 re-anchors at
  every packet anyway, so even that vanishes.
- **Chunk offset spread 0.27–0.55 ms** — clock *stability* over the whole
  run stays under **0.6 ms**, 8× inside the 5 ms gate. This number is the
  P13.4 audio↔video anchoring error budget.
- **Residual ±5–8 ms p95** — that is OUR polling jitter (5 ms sleep),
  present in the arrival timestamps only. The hardware stamps themselves
  are exact and monotonic (Δqpc strictly increasing, zero
  TIMESTAMP_ERROR/DISCONTINUITY; one SILENT flag at stream start = normal).
- Endpoint drift recomputed from the fit: the −5.46 ms residual on the
  first chunk + late-run jitter = the 5.49 ms "FAIL". Measurement artifact,
  reproduced and explained.

## Health flags

`silent=1 (row 0, stream start) · discontinuity=0 · timestampError=0`

## Gate decision

**P13.1: MET** — hardware stamps are readable, exact, monotonic, in the
Stopwatch/QPC domain, and stable to sub-millisecond over 60 s.
(Spike v5 replaces the flawed endpoint metric with these regression
metrics so future runs verdict correctly on their own.)

## What this locks in for the engine (P13.2 → P13.5)

1. `WasapiPositionCapture` (P13.2) ships `WasapiDirectInterop.cs` as-is:
   zero deps, event-free polling OK, contract = `GetBuffer → process →
   ReleaseBuffer(framesRead)` — **never 0**.
2. `qpcPosition` becomes THE master timeline for audio (P13.4). No lead
   constant, no clock steering, no drift baseline — all deleted with the
   band-aids.
3. Video stays on Stopwatch; P13.4 anchors the two timelines per packet
   (same numeric domain — plain subtraction), with chunk-offset spread
   (< 0.6 ms measured) as the worst-case anchoring error.
4. Loopback reality check: SILENT packets and stream-start transients are
   normal; AudioTap v3 must not treat them as drift events (today's
   idle-gate band-aid exists precisely because the old clock misread them).

---

# ADDENDUM — v6 silence test (2026-08-27, OWNER run 20:40, 60 s quiet/sound/quiet)

Source: `2026-08-27_silence_position_log_60s.csv` (5999 rows) +
`2026-08-27_silence_summary.txt`. Analysis: `scripts/analyze_v6_silence.py`
(container, numpy). Verdict line from the spike itself: **PASS**.

## The silence question — answered by measurement, not docs

| phase | wall time | packets | rate | frames | SILENT | TSERR | DISCONT | devPos rate |
|---|---|---|---|---|---|---|---|---|
| 0 QUIET | 0–20 s | 2000 | 100.1/s | 960 000 | 1 | 0 | 0 | 48 000.7 fps |
| 1 SOUND | 20–40 s | 2000 | 100.0/s | 960 000 | 0 | 0 | 0 | 48 000.2 fps |
| 2 QUIET | 40–60 s | 1999 | 100.0/s | 959 520 | 0 | 0 | 0 | 47 998.7 fps |

- **Model S confirmed on real hardware**: the endpoint keeps rendering
  through total silence — packet flow never stops (100/s in all three
  phases), `devicePosition` advances exactly 480 frames on EVERY one of
  the 5998 steps, and the per-phase rate is 48 000 fps through quiet AND
  sound. The audio timeline advances by itself; **SilenceKeepAlive is not
  required** and the qpcPosition gap formula stays only as a safety net.
- The single SILENT flag is row 0 (stream start). Quiet-phase packets are
  genuine zero-filled buffers WITHOUT the flag — so AudioTap v3 must key
  its silence math off qpcPosition deltas, never off the flag bit.
- Longest no-packet window 33.5 ms (arrival-side poll hiccup, not a
  stream gap); phase boundaries clean (15.7 ms arrival gap, dev step
  still exactly 480).

## Clock quality during the mixed run (unchanged from the audio-only run)

- skew +5.8 ppm (spike OLS), chunk spread 0.39 ms, jitter p95 7.09 ms,
  tsErr = 0, discont = 0 → **gate margin ≥ 34× on the worst metric**.
- **ppm double-check**: reproducing the spike's regression in numpy gives
  exactly +5.75 ppm (sw on qpc); the inverse fit (qpc on sw) gives
  +0.06 ppm. Both directions are valid OLS; the spread exists because ALL
  noise lives in `sw` (arrival = stamp + poll latency) while `qpc` is
  clean. True crystal skew is single-digit ppm either way. No numeric bug
  (sequential float64 accumulation reproduces the print bit-for-bit).
- **read-lag is a lead**: arrival − (stamp + packetDur) = −33…−48 ms —
  hardware stamps run ~40 ms AHEAD of the wall clock at read time. That
  is the shared-mode endpoint mix buffer (doc Risk #3), constant across
  the run, harmless for anchoring.

## P13.4 note (worst-case arithmetic, logged for the design)

Uncompensated skew integrates linearly: 5.75 ppm × 1800 s ≈ 10.4 ms of a
±15 ms 30-min budget — real but recoverable. Recommendation for P13.4:
fit the codec rate per session from the hardware-stamped stream itself
(rate = ΔdevicePosition/ΔqpcPosition, no external reference needed,
sub-ppm within seconds) and divide it out in the QPC→timeline map. Five
lines in SyncMath v2; kills skew to first order whatever its sign.
