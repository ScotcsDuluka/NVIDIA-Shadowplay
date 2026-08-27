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
3. **v2 runtime lesson (OWNER's machine): a bare `Out of memory.` is NOT a
   memory bug.** With `void` COM declarations the CLR maps failing HRESULTs
   to exception types — `E_OUTOFMEMORY` (0x8007000E) becomes
   `System.OutOfMemoryException("Out of memory.")`, hiding which call
   failed and why. v3 makes every call `[PreserveSig]` + manual HRESULT
   check, so failures print `CallName: 0xNNNNNNNN (AUDCLNT_E_*)`.

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

:: (--via naudio is gone — see FINDINGS; passing it falls back to interop)
```

## What the summary tells you

| Line | Meaning |
|------|---------|
| `unit` | Empirical unit of `qpcPosition`: **QPC ticks** (same domain as Stopwatch — the single-clock jackpot) or **100-ns units** (needs ×Frequency/1e7 conversion). Auto-detected from Δqpc/Δsw ratio. |
| `devpos` | ΔdevicePosition / Σframes ≈ 1.0 → devicePosition is in sample frames. |
| `latency` | Mean read-lag behind the hardware stamp = read-loop latency (5ms polling + buffering). NOT clock drift. Informative only. |
| `drift` | Stopwatch-vs-hardware elapsed mismatch over the whole run. **The gate: |drift| < 5 ms.** |
| `flags` | silent / discontinuity / timestampError counts. Any timestampError = FAIL regardless of drift. |
| `VERDICT` | PASS = hardware stamps may anchor P13.2 `WasapiPositionCapture`. |

## Evidence to send back to OWNER (P13.1 gate)

1. `out\summary_<ts>.txt` from all four runs above
2. One `out\position_log_<ts>.csv` (any run) — spot-check 3 rows:
   qpc deltas should grow ~linearly with sw_ticks deltas

## Troubleshooting

- **Bare `Out of memory.` (v2 and earlier)**: fixed in v3 — that was the
  CLR renaming an `E_OUTOFMEMORY` HRESULT from `IAudioClient::Initialize`.
  v3 prints the real call + hex + decoded name.
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
  machine with a bare `Out of memory.` (CLR HRESULT map renamed an
  `E_OUTOFMEMORY` from Initialize).
- **v3** — `[PreserveSig]` + manual HRESULT checks with decoded names,
  3-step Initialize fallback ladder (100ms -> engine default ->
  +NOPERSIST), per-step init logging, 10s heartbeats, partial evidence
  kept on loop errors, corrected AUDCLNT_BUFFERFLAGS constants (v2 had
  SILENT/TSERR/DISCONT rotated, which would have mislabeled the verdict).
