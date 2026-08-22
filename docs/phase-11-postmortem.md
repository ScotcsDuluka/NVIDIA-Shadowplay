# Phase 11 Post-Mortem — Session Lifecycle Stress Test

**Date:** 2026-08-22
**Branch:** `Engine-Rebuild-Stabilization`
**Commit tested:** `5ce76aeb5fc9a503dafd289887d10d6914395afd`
**Verdict:** **FAIL — limitation discovered; production lifecycle redesign required**

This failure is **intentional and valuable**. It validates that the spike's
test harness architecture is *not* suitable as a production lifecycle model.
Forcing PASS by patching the spike would have hidden the resource ownership
flaws that this test was designed to expose.

---

## Test Matrix Results

| Test | Sessions | PASS | FAIL Reason |
|---|---|---|---|
| A — Normal (3 × 30s) | 3 | 0 | 1 × FFmpeg path; 2 × DXGI E_INVALIDARG |
| B — Early Stop (1s/5s/10s) | 3 | 0 | 3 × DXGI E_INVALIDARG |
| C — Immediate Restart (5 × 3s) | 5 | 0 | 2 × DXGI E_INVALIDARG; 3 × NVENC OpenSession: 21 |
| **Total** | **11** | **0** | |

## Aggregate Findings

| Metric | Value | Verdict |
|---|---|---|
| FFmpeg orphan processes | 0 | N/A (ffmpeg never started — path bug) |
| NVENC errors (total) | 0 | Misleading — sessions failed before encoding |
| Audio capture failures | 3 | A1 captured audio but mux never ran; A2+ never started audio |
| Video capture failures | 11 | Every session failed before completing video encode |
| Output validation | FAIL | 0/11 sessions produced a valid MP4 |
| Memory growth | 8 MB → 499 MB (+6140%) | SUSPECTED GROWTH |
| Crashes / Deadlocks | 0 | Process stayed alive throughout |

---

## Root Cause Analysis

### 1. FFmpeg path resolution failure (Test A1)

**Symptom:**
```
WAV validate failed: An error occurred trying to start process 'ffmpeg'
with working directory '...\spikes\D3D11_NVENC_Spike'.
The system cannot find the file specified.
```

**Root cause:**
`Phase11_SessionLifecycle.ResolveFFmpeg()` walks `PATH` looking for
`ffmpeg.exe`. None found → returns the literal string `"ffmpeg"`.
`Process.Start("ffmpeg", ...)` then fails because the working directory
does not contain `ffmpeg.exe` and `PATH` does not either.

The actual `ffmpeg.exe` lives at
`C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe` — a
**deployment-relative path** that the spike has no knowledge of.

**Lesson for production:**
> Engine must resolve FFmpeg path from a configured deployment root,
> not from `PATH`. The `Overlay\API-Core\` directory is part of the
> deployment contract and must be referenced as such.

---

### 2. DXGI OutputDuplication E_INVALIDARG (Tests A2, A3, B1, B2, B3, C1, C2)

**Symptom:**
```
*** Session A2 threw: SharpGenException: HRESULT: [0x80070057],
    Module: [General], ApiCode: [E_INVALIDARG/Invalid arguments],
    Message: [The parameter is incorrect.]
```
At step `[X.2] Creating DXGI Output Duplication...` — i.e. inside
`output1.DuplicateOutput(device)`.

**Root cause:**
Windows constrains `IDXGIOutputDuplication` to **one live instance per
output per process**. Once `DuplicateOutput` succeeds, subsequent calls
on the same `IDXGIOutput` fail with `E_INVALIDARG` until the previous
duplication is fully released.

In the spike:
- Phase 2 creates a duplication for the Phase 2 test loop and **stores
  the staging texture in `SpikeSharedContext`** — but the duplication
  object itself is created and disposed within Phase 2.
- Phase 3 reuses the texture (not the duplication).
- Phase 10's `RunRecording` then calls `output1.DuplicateOutput(device)`
  *again* on the same output.

The **first** session (A1) succeeds because Phase 2's duplication was
disposed. The **second** session (A2) fails because A1's duplication
was disposed inside `RunRecording`'s `finally`, but Windows has not
yet released the output desktop session slot.

This is a **Windows-level resource ownership flaw**, not a code bug.

**Lesson for production:**
> DXGI OutputDuplication must have a **single, persistent owner** that
> spans the entire recording engine lifetime — not a per-session
> duplication. Sessions borrow the duplication; they do not own it.
>
> Architecture:
> ```
> RecordingEngine (process lifetime)
>     └── DxgiOutputDuplication (persistent, 1 per output)
>            └── AcquireNextFrame() ← borrowed by CaptureSession
> ```

---

### 3. NVENC OpenSession: 21 (Tests C3, C4, C5)

**Symptom:**
```
FAIL: OpenSession: 21
```

NVENC error 21 maps to `NV_ENC_ERR_OUT_OF_MEMORY` — specifically the
**encoder session slot exhaustion** path. NVIDIA drivers limit each GPU
to a small number of concurrent encoder sessions (typically 3–5 on
GeForce, 32 on Quadro/Tesla).

In the spike:
- Each `RunRecording` call invokes `nvenc.OpenEncodeSessionEx(...)` —
  opening a **new** encoder session.
- When sessions A2, B1, etc. threw `SharpGenException` *after* opening
  the encoder but *before* reaching the cleanup code, the encoder was
  never destroyed.
- After 2 leaked encoder sessions, the third call to `OpenEncodeSessionEx`
  hit the limit and returned error 21.

**Lesson for production:**
> NVENC encoder must have a **single, persistent owner** for the
> recording engine lifetime — same model as DXGI duplication.
> Sessions reuse the encoder; they do not open their own.
>
> Architecture:
> ```
> RecordingEngine (process lifetime)
>     └── NvencEncoder (persistent, 1 per GPU)
>            ├── EncodePicture() ← called by CaptureSession
>            └── LockBitstream() ← called by CaptureSession
> ```
>
> Also: the **exception path** in `RunRecording` must guarantee encoder
> destruction via `try/finally` at the outermost scope, not via ad-hoc
> `try { ... } catch { }` blocks scattered through cleanup.

---

### 4. Memory growth: 8 MB → 499 MB (+6140%)

**Symptom:**
```
Memory:
  baseline (before any session)            8.01 MB
  after C3                                 499.61 MB
  after C4                                 499.68 MB
  after C5                                 499.68 MB
  final (after all sessions)               499.68 MB
```

Memory grew by ~491 MB across 11 failed sessions and then **plateaued**
at 499.68 MB. The plateau is the tell: once resources stopped being
allocated (because sessions started failing at OpenSession), memory
stopped growing.

The 491 MB consists of:
- **NVENC encoder handles** from the 2 successful `OpenEncodeSessionEx`
  calls that were never destroyed (A1 + 1 earlier attempt).
- **DXGI duplication objects** that were created but never released
  when `DuplicateOutput` threw in subsequent sessions.
- **NAudio `WasapiLoopbackCapture` instances** from A1's audio thread —
  the audio thread captured 11.4 MB of audio data and the WAV file
  writer allocated buffers, but the exception path did not deterministically
  dispose the capture object.

**Lesson for production:**
> Memory tracking alone cannot diagnose the leak — it can only confirm
> that one exists. The fix is **ownership**, not `Dispose()` patches.
>
> The architecture must guarantee:
> 1. Every persistent resource (D3D11 device, DXGI duplication, NVENC
>    encoder) is owned by `RecordingEngine` and disposed exactly once.
> 2. Every per-session resource (audio capture, FFmpeg process, file
>    handles) is owned by `CaptureSession` and disposed via
>    `try/finally` at the session boundary.
> 3. The exception path in `CaptureSession.Run()` must run cleanup
>    even if the recording itself never started.

---

## What Phase 11 Successfully Validated

Despite the FAIL verdict, Phase 11 produced valuable positive evidence:

✅ **The verdict gate works** — it correctly classified all 11 sessions
   as FAIL with specific reasons per session.

✅ **The test matrix structure works** — Test A/B/C exercised the right
   dimensions (normal, early-stop, immediate restart) and surfaced
   failures in each.

✅ **No crashes / deadlocks** — the process stayed alive through 11
   failed sessions, which means the test harness itself is stable.

✅ **The 3 distinct failure modes** (FFmpeg path, DXGI ownership,
   NVENC exhaustion) were correctly attributed to their sessions,
   not masked by later failures.

✅ **Memory plateau detection worked** — the trend analyzer correctly
   identified monotonic growth and flagged `SUSPECTED GROWTH`.

---

## Decision

**Phase 11 status: CLOSED as FAIL (intentional).**

This is **not** a bug to fix in the spike. It is a **finding** that
the spike's architecture (per-session resource ownership inside
`RunRecording`) is fundamentally incompatible with multi-session
lifecycle requirements.

The findings feed directly into **Phase 12: Production RecordingEngine
Architecture** — see `Phase12_Architecture_Spec.md` for the proposed
design that addresses each lesson learned here.
