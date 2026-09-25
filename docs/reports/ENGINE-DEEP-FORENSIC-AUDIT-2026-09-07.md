# ENGINE DEEP FORENSIC AUDIT

> Audit type: Deep forensic pass (static proof + test execution + two fix rounds — round 1: 2 minimal engine fixes; round 2: full P2/P3 queue + 1 production bug found by stress-running the suites)
> Date: 2026-09-07
> Auditor: Machine B — Deep Engine Audit / Forensic Pass

## Repository / Branch / HEAD

- Repository: `C:\My Project\NVIDIA-Shadowplay`
- Branch: `Engine-Rebuild-Stabilization` (unchanged; verified via GitHubDesktop-bundled git — git not on PATH)
- HEAD: `8bce24b034dd08aef1714a7b84c346c7e05e993e` ("Update build.txt")

## Working tree

**PRESERVED.** Pre-existing modification `Overlay/build.txt` (owner counter, documented practice) untouched.
Audit edits (declared below): `CaptureEngine.Recording/CaptureSession.vb`, `CaptureEngine.Recording/RecordingEngine.vb`.
No reset / rebase / stash / force checkout used. No concurrent modification encountered during the audit window.

## Scope actually audited (dependency-real, not name-guessed)

```
Engine host (RecordingEngineHost / UI_Engine dispatch)
  → RecordingEngine (state machine, persistent backends)
    → CaptureSession (CFR loop, audio engine, LiveMux)
      → DdagrabBackend (DXGI worker) → D3D11VideoFrame → BoundedVideoFrameSink
      → NvencEncoderBackend (native hot path, C/2/C/6 unwind) → NvencResources
      → AudioEngineSession (WasapiPositionCapture + AudioPositionTracker)
      → AudioEngineMuxSink → LiveMuxSession/PipeFeed (named pipes → ffmpeg)
      → DeferredVideoFrameDisposer
  → Legacy CaptureEngine (two-process FFmpeg fallback) — G2/G3/F03 surface
```

Ownership model (verified): RecordingEngine owns backends; CaptureSession BORROWS capture+encoder and OWNS sink/disposer/audio engine/live-mux; D3D11VideoFrame owns its texture+NT handle (one-shot dispose); PipeFeed owns queue+writer thread; AudioEngineSession owns tracks+timeline. Clock ownership = single common T0 (`_timelineStartTicks`/`_timelineStartQpc100ns`) frozen before any producer arms.

## Verdicts

| Area | Verdict |
|---|---|
| Architecture | PASS |
| Lifecycle | PASS |
| Worker ownership | PASS |
| Start/Stop | PASS |
| Dispose | PASS (1 honesty defect fixed; L12 gate semantics unchanged by design) |
| Restart | PASS (code + suite proof; runtime re-proof requires NVIDIA machine) |
| Concurrency | PASS (remaining items P2-latent/P3 below) |
| Timestamp | PASS |
| Queue / Backpressure | PASS (2 P3 accounting holes reported) |
| Capture | PASS |
| Encoder | PASS |
| Device loss | PASS (AccessLost self-heal; worker-crash C-2C state tail; NVENC Faulted containment) |
| Configuration | PASS (fresh-reload per record start; AudioClockMode phantom = P3 owner decision) |
| Audio/Video isolation | PASS |
| M1 (CaptureSession cleanup) | PASS — code verified (`captureRunning`/`encoderRunning` unwind, F-05 guard order) + M1M2Tests T1-T3 (SKIP on this machine: no NVIDIA adapter — gate honest) |
| M2 (Ddagrab stop lifecycle) | PASS — code verified (`Stopping` latched on join timeout, Start rejected, worker exit tail completes transition, C-2C crash tail) + M2MTests T1/STRESS50 (SKIP: same reason) |
| Regression result | PASS |
| Build | PASS (0 errors, all touched + test projects) |
| Architecture changes | NONE |

## Proof separation

- **CODE PROOF**: every finding below cites file:line read in this pass (HEAD 8bce24b + declared edits).
- **TEST PROOF** (this machine, Intel iGPU — no NVIDIA adapter; hardware-gated suites SKIP honestly, never fake-PASS):
  - Engine.Concurrency.Tests: **20 passed / 0 failed / 7 skipped** (5 = M1/M2 need NVIDIA; 2 = loopback endpoint silent), orphan ffmpeg 0 → 0
  - Engine.ConfigTruth.Tests: **49 passed / 3 failed** — all 3 are stale source paths after the Overlay UI refactor (`[5] Video Capture.vb` → `[5] Video Capture\[Main] Video Capture.vb`; `[6] Audio Capture.Designer.vb` no longer exists at the old path). Test-maintenance defect, NOT an engine defect. Not fixed here (pointing tests at renamed UI files needs an owner-visible mapping decision).
  - CaptureEngine.Tests (Foundation): **14/14**
  - CaptureEngine.FrameContractTests: **8/8**
  - CaptureEngine.Recording.Tests: **40/40**
  - CaptureEngine.FFmpegTests: **70/70**
  - CaptureEngine.Encoder.Tests: **64 passed / 0 failed / 5 skipped** (NVENC-native hardware tests)
  - CaptureEngine.Video.Tests: **38 passed / 0 failed / 28 skipped** (DXGI hardware tests)
- **LIVE PROOF**: not obtainable on this machine for the NVENC/DXGI path (no NVIDIA GPU). M1/M2 runtime re-verification (and the M2-STRESS 50-cycle) must be re-run on the 1080 Ti machine; the suite is ready and gates correctly.

## Findings

### P0 — Critical
None found in the engine surface. (The repo-level P0s from the 2026-09-06 report — payload reproducibility, version chaos — are outside this engine audit and remain open there.)

### P1 — High
None new. Prior P1-engine items verified FIXED since the 2026-09-06 report:
- L1 `_audioEngine` leak on failure paths → `CaptureSession.vb` Finally stops+disposes (lines ~1195-1196).
- L13/L14 Pass-contract vacuity → `AudioEngineSession.Dispatch` counts sink failures into `TrackRuntime.DroppedBytes` (`AudioEngineSession.cs:330-348`), `RebuildDiagnostics` publishes it (`:421`), `CaptureSession` maps it to `result.AudioDroppedBytes` and `Pass` fails on it (`RecordingDTOs.vb:310-321`).
- L2 recording ffmpeg not job-owned → `SessionConfig.OnProcessStarted` is now passed into `LiveMuxSession` (`CaptureSession.vb:601`), invoked at spawn + remux + verify probe (`LiveMuxSession.vb:182,424`).
- L3 NVENC Initialize leak → full unwind with `unwindDone` double-destroy guard (`NvencEncoderBackend.vb:386-417`).
- L5 post-fault encode spam → `encoderFaulted` latch aborts the CFR loop into the normal stop sequence (`CaptureSession.vb:679-783`).
- L10 Dispose-without-kill orphan → `LiveMuxSession.Dispose` kills a wedged ffmpeg (`LiveMuxSession.vb:449-452`).
- T12 ExitCode-after-Kill throw → guarded try/catch with honest error string (`LiveMuxSession.vb:361-366`).

### P2 — Medium
1. **L12 remains a functional brick (behavior unchanged; lying log FIXED by this audit).**
   `RecordingEngine.vb:358-365` — on a 30 s Dispose wait timeout, `_disposeRequested` stays set, so every future `StartSession` throws `ObjectDisposedException` (`:198`); the engine is unusable until process restart. State stays `Recording`, which is actually TRUE (the session task is still alive) — the *comment/log* claimed "safe retry", which was false. **This audit fixed the message only** (truthful "StartSession is rejected until the process restarts"). Any behavioral change (Faulted transition, session abort) would alter stop semantics and needs an owner call.
   Trigger: session unwind > 30 s after Stop (e.g. wedged ffmpeg finalize + slow GPU drain). Risk: rare; requires app restart; no corruption.
2. **T3 legacy `_state` cross-thread plain field + start TOCTOU (unchanged, CONFIRMED-latent).**
   `Engine\Engine\[Capture]\CaptureEngine.vb:61,186-189,352,1243-1246` — `_state` written/read from UI thread, Task.Run, Exited threadpool thread, and stderr callback threads with no synchronization; the start guard (`<> Idle`) and `SetState(Recording)` are separated by the whole ffmpeg spawn. Double-start window is narrow and partially mitigated by the UI-side `IsRecordingLifecycleActive` guard (`:150-156`). Owner decision: interlock or accept (legacy fallback path).
3. **Encoder swap outside `_sync` during FPS rebuild (unchanged).**
   `RecordingEngine.vb:262-277` — `_encoder` field is swapped while a concurrent `Dispose()` may read it. Reference-atomic; worst case = a rebuild racing engine Dispose fails the session loudly. Needs a concurrent rebuild+dispose test before touching.

### P3 — Low
4. **AudioEngineMuxSink pending-cap silent drop uncounted** (`AudioEngineMuxSink.vb:60-65`): packets discarded while unaligned/unattached beyond the 16 MB pending cap increment no counter. Window is tiny (pre-alignment only). Same class as the fixed L13; fix requires plumbing a counter into `SessionResult`.
5. **PipeFeed post-close Feed drop uncounted** (`LiveMuxSession.vb:780-806` + `:543-589`): a chunk enqueued after `RequestStopAndDrain`'s residual fold is neither written nor counted. In the production flow audio producers stop before `LiveMux.Stop`, so the window is negligible; accounting-only.
6. **Ddagrab eternal DuplicateOutput retry** (`DdagrabBackend.vb:729-735, 1002-1009`): creation failure retried at 250 ms forever with no escalation/fault after N failures. Silent-by-design but logged; escalation policy is an owner call.
7. **Synchronous `Me.Invoke` from socket threads in the host** (`RecordingEngineHost.vb:152,171,331,460`): UI-thread stall class; everything else in the app uses BeginInvoke deliberately.
8. **CFR-loop video Feed can stall Stop up to 10 s** (`LiveMuxSession.vb:548-563` bounded block) — bounded, counted, and inside the 30 s stop budget; documented trade-off (never drop mid-GOP).
9. **LiveMux "fail hard" comment vs actual drop** (`LiveMuxSession.vb:551-560`) — comment/code mismatch; the code drops + counts (whole-packet, corruption-safe). Comment fix only.
10. **ConfigTruth stale UI paths (3 failures)** — see TEST PROOF above; test maintenance item, owner-visible mapping decision.

### Confirmed bugs fixed by this audit (minimal, behavior-safe)
- **F-A (CaptureSession unbounded probe reads)** — `CaptureSession.vb:1096-1113` (duration probe) and `:1144-1162` (verify probe): synchronous `StandardError.ReadToEnd()` ran BEFORE `WaitForExit(5000)`; EOF only arrives at process exit, so a hung/locked-output ffmpeg never reached the timeout — the stop path wedged indefinitely and the probe process outlived the session. Fix mirrors the legacy engine's proven pattern (P9): `ReadToEndAsync()` + `WaitForExit(5000)` + `Kill()` + `WaitForExit(2000)` + bounded task wait. Normal-path behavior unchanged (ffmpeg -i exits in ms).
- **F-B (Dispose-timeout lying log)** — `RecordingEngine.vb:358-365`: message now states the real contract (session still running; backends alive; StartSession rejected until restart). Zero behavior change.

### Likely bugs / suspicions (no fix without owner)
- T4-class `StopAudioWriter` re-entrance (legacy): verified BENIGN now — every inner op is guarded (`AudioEngineSession.Stop` idempotent under `_sync`; `WavSidecarWriter.Complete` one-shot via `_finalized` at `WavSidecarWriter.vb:231-238`; `WasapiPositionCapture.Dispose` idempotent). Downgraded to NO ISSUE.
- `RecordingEngineState.Stopping` is never assigned (dead enum slot) — cosmetic; the Ddagrab backend's own `Stopping` state IS live and load-bearing (M2).
- L7 direct cross-device NVENC texture path remains the production default (`UseSharedHandle` never set outside the spike) — prior-audit owner decision still open.

## Phase 7 — Timestamp forensics (summary)
Single QPC hardware counter end-to-end: DXGI `LastPresentTime`/acquire stamps → `QpcTicksTo100ns` (overflow-safe quotient/remainder form, `DdagrabBackend.vb:976-980`) → frame `CaptureTimeTicks` (100 ns) → CFR target arithmetic in the same 100 ns domain → `inputTimeStamp` (inert downstream: raw H.264 carries no PTS; the mux declares CFR rate). Backward/malformed source stamps are clamped monotonically with a fallback counter (`DdagrabBackend.vb:785-797`). `_lastSourceQpcTicks` resets per session — no cross-session clamp. Restart creates a fresh mux timeline per session (no PTS regression/jump mechanism exists; duplicate inputTimeStamps on CFR duplicate frames are inert). Audio: `AudioPositionTracker` treats `DevicePositionFrames` as the content clock with backwards-qpc immunity (933/session field evidence honored), zero-cursor continuity fallback, 3600 s silence cap, 1 s chunked synthetic silence. **PASS.**

## Phase 8 — Queue/backpressure (summary)
Video pipe: bounded 96 MB, producer blocks ≤10 s, whole-packet drop counted (corruption-safe); audio pipes: 8 MB drop-oldest block-aligned; sink: bounded 16 DropOldest with evict-dispose outside the lock; stop drains queue-first (bounded), folds residual into `DroppedBytes`, closes pipe only after connect-wait (G1 fix verified). Shutdown with non-empty queues loses nothing silently (residual counting verified at `LiveMuxSession.vb:793-806`). **PASS** (holes #4/#5 above are accounting-only edge windows).

## Test gap analysis (missing regression tests)
1. RecordingEngine Dispose-timeout path (L12 semantics) — untested anywhere.
2. Encoder FPS-rebuild swap vs concurrent Dispose — untested (P2 #3).
3. AudioEngineMuxSink pending-cap drop injection (accounting hole #4).
4. LiveMux Feed-after-close accounting window (#5).
5. M1/M2 runtime re-run on NVIDIA hardware (5 SKIPs on this machine).
6. ConfigTruth stale-path repair (3 failures) — test maintenance.

## Tests added
None. Audit fixes are covered by existing suites (Recording.Tests 40/40, FFmpegTests 70/70, Concurrency 20 passed/0 failed re-run green after the edits; all suites compile against the modified `CaptureEngine.Recording`).

## Tests executed
See TEST PROOF. Total across 8 suites: **303 passed / 3 failed (non-engine, stale paths) / 40 skipped (hardware/environment)** — plus re-runs after fixes: Recording 40/40, FFmpeg 70/70, Concurrency 20/0/7.

## Build
PASS — `CaptureEngine.Recording.Tests`, `CaptureEngine.FFmpegTests`, `Engine.Concurrency.Tests`, `Engine.ConfigTruth.Tests`, `CaptureEngine.Tests`, `CaptureEngine.FrameContractTests`, `CaptureEngine.Encoder.Tests`, `CaptureEngine.Video.Tests` all build 0 errors/warnings on net10.0 with the fixes applied.

## Files changed
- `CaptureEngine.Recording/CaptureSession.vb` — bounded probes (F-A) + sink-drop accounting fold + AudioClockMode honesty log
- `CaptureEngine.Recording/RecordingEngine.vb` — truthful Dispose-timeout message (F-B) + encoder swap under `_sync` with dispose-race guard + Dispose reads backend refs under `_sync`
- `CaptureEngine.Recording/AudioEngineMuxSink.vb` — pending-cap drops counted (`PendingDroppedBytes`)
- `CaptureEngine.FFmpegBackend/LiveMuxSession.vb` — post-close Feed drops counted (`_closed` gate) + "fail hard" comment corrected to the real whole-packet drop behavior
- `CaptureEngine.Video.Ddagrab/DdagrabBackend.vb` — L8 escalation: 120 consecutive duplication-recreate failures (~30 s) fault the worker loudly instead of the eternal 250 ms retry
- `Engine/Engine/[Capture]/CaptureEngine.vb` — T3 fix (`_state` under `_stateLock`, `Detecting` reservation closing the double-start TOCTOU, terminal states on every failure bail, OnExited snapshot) + F03-B fix (mux-failure fallback validates BEFORE move; no-audio rename path validates before promoting)
- `Engine.ConfigTruth.Tests/P3UIContractTests.vb`, `W2OverlayHonestyTests.vb` — stale UI paths repaired after the per-form folder refactor (3 failures → green)
- `CaptureEngine.Recording.Tests/vbproj + Program.vb + MuxSinkAccountingTests.vb` (new) — 3 deterministic regression tests for the sink accounting fix

No other file touched. `Overlay/build.txt` owner counter not touched (its diff state changed externally during the session — build-counter behavior, owner practice applies).

## Architecture changes
NONE.

## FINAL CHECK — M1/M2 invariant audit
- **M1**: capture/encoder ownership flags claimed BEFORE `Start()` (C-1 order, `CaptureSession.vb:617-620`), inline stops release them on the success path, Finally stops whatever a failure left running (`:1175-1184`), retained frames retired exactly once before the Nothing assignments (F-05, `:926-940`), audio engine stopped+disposed on every exit path (`:1195-1196`). Invariants hold: worker ownership / state truth / restart safety / dispose safety.
- **M2**: join-timeout leaves `Stopping` (Start rejected — no duplicate worker), the exiting generation completes `Stopping→Stopped` in its exit tail including the crash path (`DdagrabBackend.vb:475-494, 947-968`), Dispose-while-Stopping joins before releasing COM objects (`:522-553`), Dispose-from-Running stops first then cleans up. Invariants hold.
- Both suites are present, deterministic, and hardware-gated honestly; runtime green on the 1080 Ti remains the outstanding (environmental) item.

## Overall
**SAFE (engine surface)** — after two fix rounds the engine lifecycle/concurrency/timestamp/queue findings from this audit are all fixed and regression-green; remaining open items are the owner decisions (L7 NVENC direct path, L12 behavior policy, dead-knob cleanup) and the NVIDIA-machine re-run of the M1/M2 hardware suites.

---

# FIX ROUND 2 (2026-09-07, same session — "ทำๆให้หมดเลย")

Owner directive: execute the whole queue. Everything below is engine-scope, minimal-fix policy, each with regression proof.

## Fixed in round 2

| # | Fix | File | Proof |
|---|---|---|---|
| R2-1 | **ConfigTruth stale UI paths** — tests pointed at `[5] Video Capture.vb` / `[6] Audio Capture.vb` before the per-form folder refactor. Constants updated to `[5] Video Capture/[Main] Video Capture.vb` and `[6] Audio Capture/[Main] Audio Capture.vb` (designer path derivation fixed accordingly). | `Engine.ConfigTruth.Tests/P3UIContractTests.vb`, `W2OverlayHonestyTests.vb` | ConfigTruth **52/52** (was 49 passed / 3 failed) |
| R2-2 | **Encoder-swap vs Dispose race (P2 #3)** — FPS-rebuild `_encoder` swap now happens under `_sync` with a dispose-race re-check: if Dispose won the race the freshly-built backend is discarded instead of being published into a dying engine; `Dispose()` reads both backend refs under `_sync` so it can never tear down a different encoder than the one the swap published. | `RecordingEngine.vb` | build 0E + all suites green; behavioral change is a strict race-window narrowing (no path change in the non-race case) |
| R2-3 | **Legacy `_state` race + double-start TOCTOU (T3, P2 #2)** — all `_state` reads/writes go through `_stateLock`; `StartRecordingAsync` now RESERVES the `Detecting` state atomically on the caller thread (the previously dead enum slot), so two concurrent starts can no longer both pass the Idle check; every pre-spawn failure bail now lands a terminal state (`HasError`/`Idle`) so the reservation can never strand; `OnExited` snapshots the state under the lock; `StopRecordingAsync` guard synchronized. | `Engine/Engine/[Capture]/CaptureEngine.vb` | G2/G3/F03 lifecycle suites (which drive real Start/Stop/Dispose flows) green across **10 consecutive full-suite runs**; H2-A dispose-during-start still PASS |
| R2-4 | **Ddagrab eternal retry escalation (L8, P3 #6)** — 120 CONSECUTIVE failed duplication recreations (≈30 s at the 250 ms self-heal cadence) now fault the worker loudly (same outer-catch → worker-exit state tail as a natural crash); a single success resets the counter. A wedged device fails the session honestly instead of silently recording nothing forever. | `DdagrabBackend.vb` | code path shares the proven crash tail (M2/C-2C); hardware re-proof pending NVIDIA machine |
| R2-5 | **LiveMux post-close Feed drops counted (P3 #5)** — `PipeFeed._closed` gate set after RequestStopAndDrain's residual fold; Feed arriving after close counts into `DroppedBytes` instead of vanishing. "Fail hard" comment corrected to the actual behavior (whole-packet drop + counted, corruption-safe). | `LiveMuxSession.vb` | FFmpegTests 70/70 |
| R2-6 | **AudioEngineMuxSink pending-cap accounting (P3 #4 / gap #3)** — over-cap discards in the unaligned/unattached window now increment `PendingDroppedBytes`; `CaptureSession` folds it into `result.AudioDroppedBytes`/`MicDroppedBytes`, so `AudioAccountingOk`/`Pass` see every loss window. | `AudioEngineMuxSink.vb`, `CaptureSession.vb` | new `MuxSinkAccountingTests` (3 tests, byte-exact cap-rule mirror) — Recording.Tests **43/43** |
| R2-7 | **AudioClockMode honesty log** — session logs that the setting is superseded (the shared AudioEngine is device-clock by construction); the user-visible knob can no longer be silently ignored. Behavioral removal still an owner call (P13.5 inventory). | `CaptureSession.vb` | code proof |
| R2-8 | **F03-B FALSE-SAVED production bug (found by 18-run stress of the suite)** — TWO unvalidated promotion paths existed in the legacy mux fallback: (a) the mux-failure path validated AFTER `File.Move` (a transiently locked delete left the garbage at the final path), and (b) the **no-audio rename path never validated at all** — on a silent desktop (loopback delivers 0 packets, common on this machine) a moov-less temp video was renamed to the final output and announced as saved. Both paths now `ValidatePlayback` the TEMP video BEFORE promoting — an unplayable file can never exist at the final path, making the Step-4 honesty error deterministic. | `Engine/Engine/[Capture]/CaptureEngine.vb` | flake rate 2/18 → **0/10** (10 consecutive green full-suite runs) |

## Round-2 test evidence (final regression gate)

| Suite | Result |
|---|---|
| CaptureEngine.Tests (Foundation) | 14/14 |
| CaptureEngine.FrameContractTests | 8/8 |
| CaptureEngine.Recording.Tests | **43/43** (+3 new MUXSINK) |
| CaptureEngine.FFmpegTests | 70/70 |
| Engine.ConfigTruth.Tests | **52/52** (was 49/3) |
| Engine.Concurrency.Tests | **20/0/7skip × 10 consecutive runs** (was 2 flaky failures in 18 runs) |
| CaptureEngine.Encoder.Tests | 64 passed / 0 failed / 5 skipped |
| CaptureEngine.Video.Tests | 38 passed / 0 failed / 28 skipped |

All builds 0 errors. Orphan ffmpeg 0 → 0 in every Concurrency run.

## Still open (owner decisions, NOT fixed by design)
1. **L12 behavior** — Dispose-timeout semantics (message now truthful; behavior change needs an owner call).
2. **L7** — NVENC direct cross-device texture path is production default (`UseSharedHandle` knob dead).
3. **AudioClockMode** — implement-or-remove decision (honesty log added).
4. **M1/M2 runtime re-proof on the 1080 Ti** (5 hardware-gated SKIPs here).
5. Dead-code/scope items outside engine audit scope (per 2026-09-06 report queues).
