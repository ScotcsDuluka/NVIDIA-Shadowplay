# V5 WASAPI Position Spike (PHASE 13.1)

Purpose: prove that WASAPI per-packet hardware stamps
(`devicePosition`, `qpcPosition` from `IAudioCaptureClient::GetBuffer`)
are readable and trustworthy enough to be the **single master clock**
for the P13 audio redesign (`docs/PHASE-13-SHADOWPLAY-CLOCK.md`).

## FINDINGS (already proven — before OWNER even runs it)

1. **NAudio 2.2.1 cannot expose positions. Compile-verified** on .NET 8 SDK:
   - high-level `AudioCaptureClient.GetBuffer` has **no** position overload
     (CS1501 on first build);
   - `NAudio.CoreAudioApi.Interfaces.IAudioCaptureClient` is **internal**
     (CS0122) and `AudioClient.GetService` is not accessible (CS1061).
   → Decision locked for P13.2: `WasapiPositionCapture` ships the direct
   COM interop from `WasapiDirectInterop.cs` (~150 lines, zero deps).
2. This spike **builds clean** cross-compiling `net8.0-windows10.0.19041.0`
   on Linux (`-p:EnableWindowsTargeting=true`): 0 warnings, 0 errors.
   Runtime evidence still requires a Windows box (below).
3. **v2/v3 runtime lesson (OWNER's machine): the bare `Out of memory.` was
   a REAL managed OOM caused by `ReleaseBuffer(0)` wedging the read
   cursor (see Version history v4) — not the CLR's HRESULT map.** What
   actually found it: v3's instrumentation — heartbeats exposed a
   physically impossible packet rate (21M packets/10s) and the FATAL
   handler printed the exact stack (`WriteCsv` → `StringBuilder
   .ExpandByABlock`). `[PreserveSig]` + manual HRESULT checks stay:
   failures must always name their call.
4. **v6 silence test (OWNER run 2026-08-27 20:40): silence is a
   non-event for loopback.** 60 s quiet/sound/quiet: packet flow stayed
   100/s in ALL three phases, `devicePosition` stepped exactly 480 frames
   through silence at 48 000 fps — **Model S** (engine renders silence,
   timeline advances on its own). The one SILENT flag was row 0
   (stream start); quiet-phase packets are genuine zero buffers WITHOUT
   the flag. → SilenceKeepAlive not required; AudioTap v3 keys silence
   math off qpcPosition deltas, never off the flag bit. Skew +5.8 ppm,
   chunk spread 0.39 ms — gate margins intact through silence (see
   `evidence/ANALYSIS.md` addendum).

## Prerequisites
- Windows 10+ (WASAPI loopback)
- .NET 8 SDK (x64)
- A default render device (speakers/headphones); audio playing during the
  run is optional — SILENT runs are equally valid evidence (flags show it)

## Build & Run

```cmd
cd spikes\V5_WASAPI_Position_Spike
dotnet build -c Release

:: 1) default: 60s loopback, direct COM interop (zero package deps)
dotnet run -c Release -- --duration 60

:: 2) microphone path (share the same stamp contract?)
dotnet run -c Release -- --duration 30 --source mic

:: 3) long-run drift (the interesting one — crystal drift vs Stopwatch)
dotnet run -c Release -- --duration 600

:: 4) silence behavior: 20s quiet -> 20s sound -> 20s quiet
::    (answers "what happens when there is NO audio?")
dotnet run -c Release -- --duration 60 --silence-test

:: (--via naudio is gone — see FINDINGS; passing it falls back to interop)
```

## What the summary tells you

| Line | Meaning |
|------|---------|
| `unit` | Empirical unit of `qpcPosition`: **QPC ticks** (same domain as Stopwatch — the single-clock jackpot) or **100-ns units** (needs ×Frequency/1e7 conversion). Auto-detected from Δqpc/Δsw ratio. |
| `devpos` | ΔdevicePosition / Σframes ≈ 1.0 → devicePosition is in sample frames. |
| `latency` | Mean read-lag behind the hardware stamp = read-loop latency (5ms polling + buffering). NOT clock drift. Informative only. |
| `fit` | OLS over ALL packets: clock-rate skew vs Stopwatch in ppm. **Gate: \|skew\| < 200 ppm.** |
| `stable` | Per-chunk mean-offset spread — clock stability over the run. **Gate: < 5 ms.** This is the P13.4 anchoring error budget. |
| `jitter` | Arrival residual p95 — OUR polling jitter, not stamp noise. Informational. |
| `driftEP` | Old v4 endpoint metric (first vs last packet). Arrival-phase sensitive, informational only — it flagged the healthy 2026-08-27 run FAIL at 5.49 ms; see evidence/ANALYSIS.md. |
| `flags` | silent / discontinuity / timestampError counts. Any timestampError = FAIL regardless of stability. |
| `gaps` | Longest no-packet window + what the stamps say across it (devicePosition jump vs expectation, resume flags). Runs in EVERY mode. |
| `phases`/`silence` | Silence-test only: per-phase packet counts and the machine's silence model (Model S = engine renders silent packets; Model I = endpoint idles, resume stamped exactly by qpcPosition). |
| `VERDICT` | PASS = hardware stamps may anchor P13.2 `WasapiPositionCapture`. |

## Evidence to send back to OWNER (P13.1 gate)

1. `out\summary_<ts>.txt` from all four runs above
2. One `out\position_log_<ts>.csv` (any run) — spot-check 3 rows:
   qpc deltas should grow ~linearly with sw_ticks deltas

## Troubleshooting

- **Bare `Out of memory.` (v2/v3)**: root cause was `ReleaseBuffer(0)`
  wedging the read cursor → ~120M duplicate rows → oversized CSV string
  → real OOM. Fixed in v4 (`ReleaseBuffer(frames)` + streaming CSV +
  tripwires).
- **`Initialize failed on ALL fallback attempts (last 0x8007000E
  E_OUTOFMEMORY)`**: the audio engine refused stream creation. Known
  causes: an app holds the endpoint in EXCLUSIVE mode (ASIO, some voice
  chat), a Bluetooth headset mid-call (HFP profile switch), or a wedged
  audio driver. Fixes: close suspect apps, `Restart-Service audiosrv`
  (admin), replug/switch the default device, or run `--source mic` to
  prove the other path.
- **`Too few packets`**: loopback only sees the engine when the endpoint
  is rendering. Play any audio (YouTube is fine) during the run.
- **`stopped early: GetNextPacketSize: 0x88890004 ...`**: device was
  invalidated mid-run (unplug/default-device switch). Partial evidence is
  still written — send it along.
- **`runaway capture loop: N consecutive identical stamps` / `hard row cap
  hit`**: the read cursor stopped advancing (v3 pathology — ReleaseBuffer
  must echo GetBuffer's frame count). v4 fixes the cause and these guards
  remain as tripwires; if either fires in v4+, send the full output.

## Expected outcomes & contingencies

- **qpcPosition in QPC ticks** (most likely): Stopwatch is QPC on Windows →
  audio and video already share one domain → `SyncMath` v2 becomes plain
  subtraction. Proceed to P13.2.
- **qpcPosition in 100-ns units**: same conclusion after one multiply.
  Proceed to P13.2 with the conversion factored in.
- ~~`--via naudio` fails to compile~~ **RESOLVED PRE-RUN** — see FINDINGS:
  both NAudio routes are closed; direct interop is the P13.2 path.
- **timestampError > 0 or unrecognized units**: capture the summary + CSV,
  stop. OWNER reviews before any further phase.

## Non-goals

- No audio samples are read or written (stamps only).
- No engine code is modified (spike only, per P13.1 gate definition).

## Version history

- **v1** — NAudio routes (closed at compile time; see FINDINGS #1).
- **v2** — direct interop with `void` COM declarations; died on OWNER's
  machine with a bare `Out of memory.`. (Initial blame on the CLR's
  HRESULT->exception map was WRONG — Initialize returned S_OK all along;
  the true culprit was `ReleaseBuffer(0)`, see v4.)
- **v3** — `[PreserveSig]` + manual HRESULT checks with decoded names,
  3-step Initialize fallback ladder (100ms -> engine default ->
  +NOPERSIST), per-step init logging, 10s heartbeats, partial evidence
  kept on loop errors, corrected AUDCLNT_BUFFERFLAGS constants (v2 had
  SILENT/TSERR/DISCONT rotated, which would have mislabeled the verdict).
- **v4** — **the actual root cause found & fixed**: `ReleaseBuffer(0)`
  wedged the engine's read cursor (capture side must echo GetBuffer's
  frame count), so the loop re-read the same packet ~2.4M times/s and
  60s of "capture" was ~120M duplicate rows; the giant single-string CSV
  write then threw OutOfMemory. v4: `ReleaseBuffer(frames)`, duplicate-
  stamp tripwire (1000 identical stamps => break), hard row cap
  (10k/s), streaming CSV writer, heartbeat rate display. Lesson for
  P13.2: the capture loop contract is GetBuffer -> process ->
  ReleaseBuffer(framesRead), never 0.
- **v5** — analysis rework after reviewing OWNER's real 60s evidence run
  (6000 packets, `evidence/`): endpoint drift replaced by full-series OLS
  (skew ppm, gate <200) + chunk-offset stability (gate <5 ms). The
  evidence run measured **+3.3 ppm skew, 0.27–0.55 ms stability** — the
  P13.1 gate is met; the old 5.49 ms FAIL was endpoint arrival-phase
  noise. VERDICT gates on skew + stability + timestampError + unit.
- **v6** — silence test (OWNER question: "what if there is NO sound?"):
  `--silence-test` (quiet -> sound -> quiet phases, per-phase packet
  counts) + gap analysis in every run (longest no-packet window, what
  devicePosition/qpcPosition did across it, DISCONTINUITY on resume).
  Classifies the machine's silence model: Model S (engine renders silent
  packets — timeline advances by itself) vs Model I (endpoint idles —
  timeline freezes, resume stamped exactly by qpcPosition, gap formula
  fills it). Also fixes a latent v5 compile bug: local `n` was declared
  twice in AnalyzeAndSummarize (CS0128) — v5 never shipped to a machine;
  caught during v6 reconstruction. Latency counter renamed `nLat`.
