# PHASE 1 VIDEO — VALIDATION STATUS & BLOCKER REPORT

State: PHASE 1 Video code checkpoint `06667e9` (published to
`origin/Engine-Rebuild-Stabilization`). This document is the honest
boundary between what is VERIFIED, what is READY-TO-VERIFY on Windows,
and what is BLOCKED. No claim below is extrapolated beyond its evidence.

---

## 1. Verification ladder (field-by-field)

Levels: `CONFIG ONLY → MAPPING VERIFIED → RUNTIME VERIFIED → HARDWARE VERIFIED → OUTPUT VERIFIED`

| Field | Requested source | Effective mapping | Level reached | Evidence |
|---|---|---|---|---|
| FPS | `Recording.current.fps` | `NextRecordingConfig.MapStartupConfig` → `EngineStartupConfig.Fps` → `NvEncParamBuilder.BuildInitializeParams` (`frameRateNum`) + CFR pacing | **MAPPING VERIFIED** | V-CT1 (ConfigTruth 12/12), V-CT5b (Encoder.Tests 52/52) |
| Resolution | `Recording.current.use_native_resolution/width/height` | `EngineStartupConfig.ResolveEncodeDimensions` → `encodeWidth/Height` + `maxEncodeWidth/Height` | **MAPPING VERIFIED** | V-CT2, V-CT2b/c; single implementation (dead duplicate removed in `06667e9`) |
| Bitrate | `Recording.current.bitrate` (kbps→bps in loader) | → `EncoderConfig.BitrateBps` → `NV_ENC_RC_PARAMS.averageBitRate` (CBR) | **MAPPING VERIFIED** | V-CT3, V-CT3b, V-CT5c/d |
| Preset | `Recording.current.encoder_preset` (single mapper; engine.json `Preset` = compat fallback) | → preset GUID (`NV_ENC_PRESET_P1..P7_GUID`, byte-exact vs SDK 13.0) → `presetGUID` | **MAPPING VERIFIED** | V-CT4/4b, V-CT5f, **V-CT5g literal GUID pin** (BLOCKER B1 fixed in `06667e9`) |
| Encoder | `Recording.encoder` (`h264_nvenc`/`hevc_nvenc`) | normalized both ways → `NV_ENC_CODEC_H264_GUID` | **MAPPING VERIFIED** | V-CT5b (encodeGUID assert) |
| GOP | engine default (60) | NEVER derived from FPS (FPS→GOP coupling removed) | **MAPPING VERIFIED** | V-CT1c, V-CT5c (`gopLength=60` independent of fps=75) |
| CaptureMethod | `Recording.api_capture` / engine.json `CaptureMethod` | requested→selected→actual echo | **MAPPING VERIFIED (echo)** — see GAP G1 | RecordingEngine.vb:123-128 |
| PixelFormat | engine.json `PixelFormat` | requested→runtime echo | **BLOCKED** — see BLOCKER B2 | RecordingEngine.vb:135-138 |

The mapping layer cannot be verified further on Linux: struct sizes and
field offsets are pinned against `nvEncodeAPI.h` SDK 13.0
(nv-codec-headers n13.0.19.0) via V-CT5a, and preset GUIDs are pinned to
the header string literals via V-CT5g — but the driver actually reading
those structures is a Windows/NVENC runtime fact.

## 2. Windows real-record validation — READY, NOT YET EXECUTED

The execution kit is committed and compiles (0W/0E):

- **Driver**: `CaptureEngine.Recording.ConsoleDriver` gained
  `--videocheck` mode (`Program.vb:330-377`). It drives the canonical
  chain — `LoadEffectiveSettings → MapStartupConfig → Initialize →
  BuildSessionConfig (fresh reload) → StartSession` — the SAME
  composition `Engine.ConfigTruth.Tests` runs on Linux and
  `RecordingEngineHost` runs in the Overlay. Linked engine files
  (`vbproj:23-36`) guarantee zero drift from production source.
- **Scenario runner**: `scripts/windows-phase1-video-validation.ps1` —
  7 scenarios (S1 FPS, S2 native, S3 custom 720p, S4 bitrate, S5
  preset, S6 encoder, S7 gfxcapture-gap), each writes a temp config
  dir (user config untouched), records a real session, ffprobe-asserts
  the produced MP4, and writes `evidence/phase1-video/report.md`.

Run on the OWNER's machine:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-phase1-video-validation.ps1
```

**Status: RUNTIME/HARDWARE/OUTPUT VERIFIED cannot be claimed until this
kit has run and its report.md exists.** This is the only remaining gap
between the current MAPPING VERIFIED state and OUTPUT VERIFIED.

## 3. BLOCKER B1 — ENVIRONMENT: no Windows/NVENC execution context (resolved by kit)

The Linux validation environment cannot execute the real pipeline:

- `CaptureEngine.Video.Tests`: 18/61 failures, **all** `DllNotFoundException:
  dxgi.dll` (Linux has no DXGI/D3D11); stash-verified identical to
  baseline 43/18 — no regression, pure environment limitation.
- `DdagrabBackend` requires DXGI Desktop Duplication + D3D11 (Windows).
- `NvencEncoderBackend` requires `nvEncodeAPI.dll` + NVIDIA driver.

Not fakeable: no log-only or simulated result is accepted as runtime
evidence. The kit in §2 exists precisely so the OWNER's machine can
produce the real numbers.

## 4. BLOCKER B2 — PixelFormat (nv12) NOT implemented

- Requested `PixelFormat` is **evidence-only**; the pipeline is
  BGRA8 (D3D11 capture) → NVENC ARGB end-to-end.
- Evidence: `CaptureEngine.Recording/RecordingEngine.vb:135-138` —
  `pixel format: config='…' → runtime=BGRA8 (D3D11 capture) → NVENC
  input=ARGB — nv12 conversion NOT implemented (BLOCKER P1-PIXFMT);
  recording continues BGRA/ARGB`.
- Impact: a user setting `nv12` gets a loud truth line, not a silent
  substitution — but also not the requested format. Any PixelFormat
  validation must expect BGRA/ARGB behavior until a GPU conversion
  layer (BGRA→NV12) is implemented between capture and NVENC.
- Required to close: implement + validate the conversion layer, then
  assert pix_fmt via ffprobe (`-show_entries stream=pix_fmt`).

## 5. GAP G1 — gfxcapture NOT implemented (loud, not silent)

- Config accepts `gfxcapture` (`Overlay/[Forms Overlay - Project
  Files]/[API]/[Services]/AppSettings.vb:49`), but the New Engine has
  exactly one production backend.
- Evidence: `CaptureEngine.Recording/RecordingEngine.vb:123-128` —
  requested `gfxcapture` logs `GAP: not implemented in the New Engine
  (only production backend = ddagrab) — running DdagrabBackend. Gap
  recorded, NOT silently accepted.`
- Factory contract: `CaptureEngine.Video.Ddagrab/DdagrabBackendFactory.vb:41-45`
  throws `VideoBackendConfigurationException` for any kind ≠ Ddagrab;
  `CaptureEngine.Video.Tests/Lifecycle/DdagrabBackendLifecycleTests.vb:256-257`
  pins that contract.
- Scenario S7 of the kit asserts the GAP warning appears (honest-gap
  contract), not that gfxcapture works.

## 6. What "closing PHASE 1 Video" still requires

1. **OWNER runs the Windows kit** (§2) on the NVIDIA machine → attach
   `evidence/phase1-video/report.md` → FPS/Resolution/Bitrate/Preset/
   Encoder move to RUNTIME VERIFIED (+ OUTPUT VERIFIED where ffprobe
   asserts the file).
2. **PixelFormat** (B2): implement BGRA→NV12 GPU conversion (or
   formally de-scope nv12 from the user config schema) — then validate
   via ffprobe `pix_fmt`.
3. **gfxcapture** (G1): implement `GfxCaptureBackendFactory` +
   backend, or keep the loud GAP echo as the documented behavior.
4. Bitstream-level preset fingerprinting (qp-offsets analysis) remains
   optional future work; preset runtime evidence is currently the init
   echo + struct-level mapping, which the kit captures from the driver
   log.
