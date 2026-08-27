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
