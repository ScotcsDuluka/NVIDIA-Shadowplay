# Phase 12b — Implementation Notes & Evidence

**Branch:** `Engine-Rebuild-Stabilization`
**Date:** 2026-08-23
**Scope:** Production Host hardening + AUDIO hard-blocker rework + validation kit

---

## What changed and why

### AUDIO — hard blocker (CaptureSession rework)

The Phase-12a `CaptureSession` wrote WAV directly from the WASAPI
`DataAvailable` callback and "waited" for finalization with
`Thread.Sleep(500)`. That violated the proven sidecar model
(`Engine/Engine/[Audio]/AudioFileWriter.vb` +
`ENGINE-REALTIME-ARCHITECTURE.md`): *WASAPI callbacks only copy/enqueue;
disk writes happen on a writer thread.*

| File | Change |
|---|---|
| `CaptureEngine.FFmpegBackend/WavSidecarWriter.vb` | **NEW** — bounded ConcurrentQueue + dedicated writer thread + byte accounting (`enqueued = written + dropped`, residual must be 0) + canonical 44-byte PCM WAV header patched at finalize. No NAudio dependency (testable on net8.0). |
| `CaptureEngine.FFmpegBackend/SyncMath.vb` | **NEW** — the proven legacy offset model extracted verbatim: `offset = (videoStart − audioStart)/freq`, clamped [−2 s, +5 s]; `videoStart` = first frame from sink; `audioStart` = **StartRecording() call time** (NOT first callback — the historical −10 s bug). |
| `CaptureEngine.Recording/CaptureSession.vb` | Audio path replaced: callback → copy → `WavSidecarWriter.EnqueueChunk` (never disk I/O on the WASAPI thread); IeeeFloat→PCM16 conversion; `RecordingStopped` event wait replaces `Sleep(500)`; mux uses `SyncMath` offset + **ffprobe'd container duration** (was wall-clock); wrap `-r` uses `DdagrabBackend.OutputRefreshRate` (was hardcoded `75`). |
| `CaptureEngine.FFmpegBackend/MuxCoordinator.vb` | Additive only: `OnProcessStarted` hook (job-object assignment) — behavior unchanged when `Nothing`. |

### VIDEO — proven path preserved

`DdagrabBackend` / `NvencEncoderBackend` capture+encode paths are **untouched**
except one additive property:

| File | Change |
|---|---|
| `CaptureEngine.Video.Ddagrab/DdagrabBackend.vb` | `OutputRefreshRate` (Hz) from `GetDisplayModeList` matched to desktop size, rational rounded (59950/1000 → 60). Best-effort; 0 → caller falls back. Evidence: OWNER decision commit `20932aa` ("use display refresh rate, not achieved FPS") + the in-source `TODO: expose from DdagrabBackend`. |

### PHASE 12b — Production Host

| File | Change |
|---|---|
| `CaptureEngine.Recording/RecordingDTOs.vb` | `SessionConfig` += `AudioEnabled`, `SystemVolume`, `OnProcessStarted`. NEW `EngineStartupConfig` (codec/bitrate/GOP/preset — process-lifetime). `SessionResult` += sync/accounting evidence fields. |
| `CaptureEngine.Recording/RecordingEngine.vb` | `Initialize(startup)` overload (parameterless preserved); encoder config from startup instead of hardcoded; `GetStatus` now carries `LastSessionResult`. |
| `Engine/Engine/[API]/RecordingEngineHost.vb` | **Init moved off the UI thread** (D3D11+NVENC init no longer blocks form load); engine-readiness gate on record start; **FFmpeg resolution per Phase-11 lesson #1** (settings → `{exe}\API-Core\ffmpeg.exe` → exe dir → PATH, loudly logged); full settings→config mapping; **JobObjectGuard** owns every spawned ffmpeg/ffprobe via the `OnProcessStarted` hook; dispose order engine→guard. |

### TESTING + DOCS

| Path | Change |
|---|---|
| `CaptureEngine.Recording.Tests/` | **NEW** — SyncMath units, WavSidecar units, and **real-ffmpeg runtime sync validation** (see evidence below). Runs on Windows AND Linux. |
| `CaptureEngine.Recording.ConsoleDriver/Program.vb` | Full validation matrix: A (3×10 s), B (early-stop 1/5/10 s), C (5×3 s restarts) — per-session DoD checks (streams, accounting, orphan ffmpeg, engine-back-to-Idle, temp leftovers) + markdown evidence output. `--quick`/`--single` modes preserved. |
| `CaptureEngine.Video.Tests/CaptureEngine.Video.Tests.vbproj` | TFM `net8.0` → `net8.0-windows` — **pre-existing NU1201 breakage** (commit `0281321` switched Ddagrab to `-windows` but left this project behind; whole-solution build was impossible until fixed). |
| `Overlay/NVIDIA Overlay.sln` | All 14 new projects added — "build whole solution" is now a single command. |
| `scripts/build-all.ps1` / `.sh` | Clean-first whole-solution build + test runner (BUILD_PROTOCOL compliant). |
| `scripts/validate-phase12b.ps1` | One-command Windows validation: build → test suites → production matrix → **crash test** (taskkill mid-session → assert zero orphan ffmpeg → evidence file). |

---

## Evidence — what is PROVEN where

### Proven in this session (Linux box, real binaries)

| Suite | Result | What it proves |
|---|---|---|
| `CaptureEngine.Recording.Tests` (SyncMath) | **7/7 PASS** | Offset math matches the proven legacy model incl. clamps + adelay/-ss mapping |
| `CaptureEngine.Recording.Tests` (WavSidecar) | **4/4 PASS** | 44-byte header validity, accounting invariant (incl. deterministic drop-on-full flood + 2-thread producers), idempotent finalize |
| `CaptureEngine.Recording.Tests` (Runtime sync, real ffmpeg 7.1.5) | **5/5 PASS** | End-to-end: sidecar WAV → wrap → `MuxCoordinator` + `SyncMath` → final MP4. A 440 Hz tone placed at content t=1.0 s lands at **1.000 s (aligned) / 1.500 s (audio 0.5 s late → adelay) / 0.500 s (audio 0.5 s early → -ss)**, measured by `silencedetect`, tolerance ±80 ms. Video-only mux + temp cleanup verified. |
| Whole solution `dotnet build` (20 projects incl. net8.0-windows via `EnableWindowsTargeting`) | **0 W / 0 E** | Everything compiles, incl. Engine host + Overlay + all tests |
| Regression: existing suites | `CaptureEngine.Tests` 14/14 · `FFmpegTests` 24/24 · `FrameContract` 8/8 · `ConfigTests` 91/91 · `Encoder.Tests` 45/45 | No behavior broken by the MuxCoordinator/DTO additions |

Bugs found and fixed **by** the new tests (reproduce → root cause → patch → retest):
1. `PatchHeaderSizes` wrote the RIFF size at offset 0..3, clobbering the `RIFF` magic → Seek(4) first.
2. WAV header was 42 bytes: sample-rate field written `CUShort` instead of 4-byte `CUInt` → ffprobe could not parse the sidecar WAV (cascade failure of all sync scenarios).

Both were caught by the runtime suite on Linux before ever reaching Windows.

### Requires the owner's Windows machine (cannot run here)

D3D11/ddagrab, native NVENC, WASAPI loopback and the WinForms host are
Windows-only. Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\validate-phase12b.ps1
```

which executes the DoD checklist end-to-end and writes evidence:

- `evidence\phase-12b-crash-*.txt` — orphan-ffmpeg check after `taskkill /F`
- `CaptureEngine.Recording.ConsoleDriver\evidence\phase-12b-validation-*.md` — per-session tables for Test A/B/C

**Nothing on this page claims the GPU-path DoD as PASS until that script exits 0
on Windows** (rule: never infer success from static analysis; never weaken
acceptance criteria).

---

## Definition-of-Done mapping

| Criterion | Status |
|---|---|
| NVIDIA ShadowPlay.exe → TCP → NVIDIA Capture.exe → RecordingEngine | Wired (`RECORD_START` alias → `engine_record_start`; dispatcher routes to RecordingEngine when ready) |
| Single session / second session / repeated sessions | Awaiting Windows matrix (Test A/C exercise exactly this) |
| Video + audio / A/V sync | Mux+sync chain proven on Linux; live WASAPI alignment awaits Windows run (offset evidence recorded per session) |
| No orphan FFmpeg | JobObjectGuard hook on every spawn; crash-test script included |
| Clean shutdown / crash-safe cleanup | Dispose order engine→guard; event-driven WAV finalize; wrap/mux timeouts+kill |
| Build whole solution | ✅ 20 projects, 0 W / 0 E (both Windows script and Linux cross-compile) |

## Follow-ups (not blockers)

- Mic track: `SessionConfig`/mux already support two tracks; CaptureSession wires system-loopback only (matches Phase 12 scope).
- Replay buffer path (`engine_replay_*`) still runs the legacy engine.
- Overlay does not yet spawn `NVIDIA Capture.exe` automatically — deployment currently starts it out-of-band.
