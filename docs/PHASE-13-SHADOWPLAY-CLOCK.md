# PHASE 13 — ShadowPlay-Style Single-Clock Design

> OWNER directive (2026-08-28): "ขอแบบ ShadowPlay" — hardware-timestamped
> A/V sync. One clock. No steering, no hand-calibrated leads.
>
> Status: DESIGN — not yet implemented. Owner approves before any code.

## 1. Why the current model plateaus

The pipeline feeds ffmpeg through **raw byte pipes** (`-f s16le`, raw H.264).
Pipes carry no time, so two software clocks were built to synthesize it:

- **Video clock**: CFR pacing loop (`CaptureSession.Run`) — tick counting
  with `timeBeginPeriod(1)`.
- **Audio clock**: `AudioTap` reconstructs wall-clock from WASAPI callback
  *arrival times* (Stopwatch), then gap-fills silence.

Every stabilizer that exists today (steering, drift baseline, idle-gate,
pre-roll, hand-calibrated 50ms lead, silence caps) is a compensation for
those two clocks disagreeing. They reduce disagreement; they cannot
eliminate it, because both clocks are estimates.

## 2. The ShadowPlay principle, adapted to a no-hook app

Real ShadowPlay stamps frames at driver flip time and audio at the device
clock — **the hardware is the clock**. On Windows, everything we touch
already shares ONE hardware time base:

- `Stopwatch.GetTimestamp()` in .NET = **QPC** (QueryPerformanceCounter).
- WASAPI hands out per-packet `qpcPosition` (QPC of the first frame in the
  buffer) and `devicePosition` (sample counter of the device clock).
- Windows.Graphics.Capture `SystemRelativeTime` is QPC-derived (spikes
  V2–V4 validated this).

So the fix is not "build a better clock" — it is **stop ignoring the
timestamp WASAPI already gives us**.

```
BEFORE (two synthesized clocks):          AFTER (one hardware clock):

WASAPI ─cbk→ AudioTap (Stopwatch guess,   WASAPI ─GetBuffer(qpcPos)→ AudioTap v3
         gap-fill, steering, lead)              (gap = qpcPos deltas — measured,
         ↓ pipe (raw s16le, no time)            not guessed)
                                                ↓ pipe (unchanged)
CFR loop ─ticks→ NVENC → pipe       →      CFR loop ─ticks→ NVENC → pipe
         ↓                                       ↓
        ffmpeg ←── pipe-head anchors (SyncMath) ffmpeg
              offsets = QPC subtractions, exact
```

## 3. Changes

### 3.1 NEW — `WasapiPositionCapture` (in `CaptureEngine.Recording`)

> **P13.2 UPDATE (2026-08-28, SHIPPED):** the class lives in a new C#
> library **`CaptureEngine.Audio.Wasapi`** (plain `net8.0`) instead of the
> VB project — the proven COM interop must not be hand-translated, and a
> plain-net8.0 lib is referenceable from BOTH `net8.0-windows` consumers
> (Recording) and the Linux CI test project (FFmpegTests, plain net8.0).
> Contents: `WasapiDirectInterop.cs` (verbatim from the spike; only the
> namespace changed), `WasapiPositionCapture` (Start/Stop + PacketReady on
> a capture thread; Windows-guarded; ladder + `ReleaseBuffer(frames)`
> + runaway tripwire all verbatim), `WasapiPacket` (stamps normalized to
> 100ns at source — `QpcTicksTo100ns` is overflow-safe for QPC > 29 days),
> `AudioPositionTracker` (PURE stamp math: holes, Risk-#2 zero-stamp
> fallback, backwards-stamp max-policy, `Reset()` for device switch).
> Gate: FFmpegTests green — 11 new WPOS tests (synthetic positions,
> deterministic, Linux-run), suite 35/35 PASS; Recording cross-compiles
> with the reference (0 warnings).

> **P13.2 HARDENING PASS (2026-08-28, OWNER directive "ดีที่สุด stable
> ที่สุด"):** three stability gaps closed, all pre-consumer (AudioTap v3
> not wired yet):
> 1. **Exception containment in the capture loop** — a `PacketReady`
>    subscriber throwing (or a `Marshal.Copy` failure) used to escape as
>    an unhandled background-thread exception, which TERMINATES the whole
>    .NET process. Now contained: loop exits, `IAudioClient.Stop()` runs,
>    `StoppedWithError` carries the full report, process survives.
> 2. **Partial-failure cleanup in `Start()`** — a throw after
>    `ActivateAudioClient` (GetMixFormat / Initialize ladder /
>    GetBufferSize / Start) now best-effort stops the client and drops
>    every COM reference, so a retry with a fresh instance cannot trip
>    over residue. Options (`PollIntervalMs`, `BufferDuration100ns`) are
>    validated up front.
> 3. **Tracker hygiene** — `Feed(frames < 0)` throws instead of silently
>    corrupting counters, and a first REAL stamp after a zero-stamp
>    fallback anchor RE-ANCHORS the timeline (`ReAnchoredNow` in the
>    report) instead of "measuring" a hole the size of the whole stream
>    (which would have made AudioTap v3 pad seconds of silence on a
>    stampless-then-stamping driver).
> Suite: 13 WPOS tests, 37/37 PASS; all three projects 0 warnings.

> **P13.1 UPDATE (2026-08-28, compile-verified in V5 spike):** NAudio 2.2.1
> cannot expose positions — high-level wrapper lacks the overload (CS1501);
> raw `Interfaces.IAudioCaptureClient` is internal (CS0122). The class
> therefore ships **direct WASAPI COM interop** ported from
> `spikes/V5_WASAPI_Position_Spike/WasapiDirectInterop.cs` (~150 lines,
> zero new dependencies, exception-on-HRESULT so AUDCLNT codes surface).

Direct capture loop over the WASAPI COM interfaces:

- Default render device → `IAudioClient.Initialize(Shared, LOOPBACK)` with
  the mix format, then a polling loop of
  `GetNextPacketSize → GetBuffer → ReleaseBuffer(0)`.
- Every packet raises `DataAvailable(buffer, count, devicePosition, qpcPosition)`.
- V5 spike already builds this exact loop (0 warnings / 0 errors
  cross-compiling on Linux); P13.1 runtime evidence is the remaining gate.

### 3.2 REWRITE — `AudioTap` v3 (gap-fill from measured time)

> **P13.1 UPDATE (2026-08-27, v6 silence test on OWNER machine):** loopback
> silence is a **non-event** — through 40 s of total silence the endpoint
> kept delivering 100 packets/s with `devicePosition` advancing exactly
> 480 frames/packet at 48 000 fps (**Model S**: the engine renders silence;
> the timeline advances on its own). The SILENT flag appears only on the
> stream-start packet, never on quiet-phase packets — silence math must
> key off `qpcPosition` deltas, never off the flag bit. Consequences:
> `SilenceKeepAlive` is **not required** and joins the deletion list
> below; the qpcPosition gap formula stays as the safety net for
> pathological drivers. Evidence: `spikes/V5_WASAPI_Position_Spike/
> evidence/ANALYSIS.md` addendum.

`Feed(buffer, count, qpcPos100ns)` — silence math becomes:

> **P13.2 precision note:** `lastEnd100ns` is the previous packet's **END**
> (`lastQpc + lastDur`), so `gap = qpc − lastEnd` is exactly the silence
> AudioTap pads before writing this packet's bytes — valid for variable
> packet sizes too (the pseudo-code below predates the spike and measured
> from the previous START; keep END-to-START semantics). Backwards stamps
> are flagged and absorbed with `max(prevEnd, newEnd)` — never rewind.
> Reference implementation + 11 synthetic-position tests:
> `CaptureEngine.Audio.Wasapi/AudioPositionTracker.cs`.

```
gap100ns      = qpcPos100ns − lastEnd100ns      ' measured by hardware
bufferDur100ns= count / bytesPerSec × 10⁷
silenceNeeded = gap100ns − bufferDur100ns        ' block-aligned, as today
lastEnd100ns  = qpcPos100ns + bufferDur100ns
```

**DELETED** (the band-aid graveyard):

- clock steering + drift baseline + `_lastSteerCheckBytes` idle-gate
- Stopwatch arrival-gap logic (`Stopwatch.GetTimestamp` in `Feed`)
- first-callback pre-roll measurement (`MarkStart` origin guessing —
  qpcPosition states where the head really is)
- `FinalizeToNow` tail estimation (pad from lastEnd to session-end QPC —
  still trivial, but now exact)

**KEPT**: block-align clamp, silence buffer, evidence logging, the
`>0.05s` minimum-gap threshold, cap raised to 3600s.

Device hot-unplug / switch mid-session → new tap instance (position
counters reset with the device; log it, don't bridge it).

### 3.3 SIMPLIFY — `SyncMath` v2

Video t0 = QPC (Stopwatch) of first encoded frame — unchanged.
Audio stream start = first packet's `qpcPosition` — **was** a
`StartRecording()` call-time guess with a hand-calibrated 50ms lead.

```
offsetSec = (videoT0Qpc − firstAudioQpc) / QpcFrequency   ' exact
```

- `SystemAudioLeadSec / MicAudioLeadSec = 0` by default; retained as an
  optional residual knob (loopback path latency measured by
  `scripts/sync-verify.ps1`, not by ear).
- `BeginTimelines(discard, pad)` semantics unchanged — values now come
  from exact QPC arithmetic, so the clamp `[-2s, +5s]` stays only as a
  safety net against pathological runs.

### 3.4 UNCHANGED

- CFR duplicate-frame loop + `timeBeginPeriod(1)` — still needed as the
  encoder cadence and for the pipe's byte-clock; audio no longer chases
  it, they rendezvous at the QPC anchors.
- Named-pipe transport, `PipeFeed`, `LiveMuxSession` ffmpeg arg template,
  fragmented-MP4 finalize/remux.

## 4. Migration phases (each gated, each revertible)

| Phase | Deliverable | Gate |
|-------|-------------|------|
| P13.1 | Spike: capture loop prints 60s of `(devicePos, qpcPos)` pairs; verify QPC↔Stopwatch agreement (<1ms skew) | Evidence log reviewed by OWNER |
| P13.2 | `WasapiPositionCapture` class + unit tests with **synthetic positions** (deterministic, runs on Linux CI) | FFmpegTests green |
| P13.3 | `AudioTap` v3 behind a config flag (`ClockMode=Device` vs `Legacy`); position-driven tests replace gap-driven ones | A/B runs identical on silence-gap suite |
| P13.4 | `SyncMath` v2 QPC anchors; leads zeroed; `sync-verify.ps1` on 5 runs: ±15ms without calibration | OWNER listens: 3 recordings incl. 30-min long-run |
| P13.5 | Delete legacy path + dead band-aids; update PROJECT_MEMORY + README humor line | Owner sign-off |

## 5. Risks

1. ~~**NAudio wrapper surface** — if the exposed `IAudioCaptureClient`
   overload is missing/incomplete in the pinned NAudio version, fall back
   to ~150 lines of direct WASAPI COM interop~~ **RESOLVED (P13.1,
   compile-verified)**: both NAudio routes are closed; direct interop
   (`WasapiDirectInterop.cs` → engine port) is the chosen path.
2. **qpcPosition = 0** on some drivers when flags say silent/discontinuity
   → treat as "no stamp" and fall back to lastEnd + bufferDur for that
   packet only.
3. **Loopback residual latency** — device stamps are capture-side; audible
   latency of the loopback path remains and is machine-dependent. Covered
   by the optional measured lead (3.3), not by per-ear calibration.
4. **Mic clock ≠ loopback clock** — each tap owns its own position stream;
   both map into QPC, so they still share the master domain. Mic drift vs
   loopback over hours: bounded by QPC, observable in sync-verify.

## 6. Acceptance

- 30-minute recording: lip-sync within ±15ms start-to-end, **zero**
  steering corrections possible (the code no longer exists).
- First run on a fresh machine correct with no hand calibration.
- All FFmpegTests green on Linux CI (synthetic positions).
- `docs/` band-aid inventory in §3.2 marked as removed in
  PROJECT_MEMORY.
