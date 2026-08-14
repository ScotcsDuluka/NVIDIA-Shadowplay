# Phase Plan — Engine-Audio Reset

> **Goal**: Rebuild the audio capture pipeline cleanly, phase by phase,
> with verifiable acceptance criteria at each step.

## Architecture Lock (final, do not change)

| Component | Status |
|---|---|
| `Engine/` | ✅ Runtime engine (only one) |
| `CaptureEngine/` | ❌ Removed in Phase 0 |
| Audio | Phase 2 — write fresh from NAudio docs |
| NVENC | Phase 1 — first baseline |
| AudioMixer UI | Phase 3 |
| Intel QSV | Phase 4 — last, after NVIDIA path proven |

## Phase 0 — FREEZE (current)

**Status**: Complete after this commit.

- Removed `CaptureEngine/` orphan project
- Documented baseline in `docs/PHASE0_BASELINE.md`
- Documented build protocol in `docs/BUILD_PROTOCOL.md`
- No `.vb` logic changes

**Acceptance** (must be verified on Windows machine):
- `dotnet build Engine/NVIDIA Capture.vbproj` exit 0
- `dotnet build Overlay/NVIDIA Overlay.vbproj` exit 0
- App still records video-only (no audio) using NVENC, same as before Phase 0

---

## Phase 1 — NVIDIA Video Only

**Goal**: Prove the video capture path end-to-end with NVENC. **No audio.**

### Scope
- Verify `BuildFFmpegArguments` produces correct NVENC args
- Verify ddagrab → D3D11 → hevc_nvenc pipeline works
- Verify output mp4 has video > 0 KiB
- **Disable audio entirely** (remove or short-circuit `-f dshow` branch)

### Acceptance Criteria
| # | Criterion | How to verify |
|---|---|---|
| 1.1 | Build succeeds | `dotnet build` exit 0 |
| 2.1 | FFmpeg exit code = 0 | log tail |
| 2.2 | Video stream mapped | `Stream #0:0 -> #0:0 (... -> hevc_nvenc)` in log |
| 2.3 | Output video > 0 KiB | `video:XXXXKiB` in log |
| 2.4 | FPS ≈ target | `fps=` in progress line |
| 2.5 | Resolution correct | `Stream #0:0: Video: ..., WIDTHxHEIGHT` |
| 2.6 | No audio stream in output | no `Stream #0:1: Audio` line |

### Out of scope
- Audio (any kind)
- AudioMixerForm
- Intel QSV
- Custom resolution scaling

### PR
- One PR: `phase 1: NVENC video-only baseline`
- Touch only `Engine/Engine/CaptureEngine.vb` (and maybe `CaptureSettings.vb` for `AudioCapture = False` default)

---

## Phase 2 — Audio Backend (NAudio, written fresh)

**Goal**: Replace `-f dshow` with NAudio WasapiCapture + WasapiLoopbackCapture piped to FFmpeg via named pipes.

### Scope
- Create new files in `Engine/Engine/`:
  - `AudioCapture.vb` — WasapiLoopbackCapture (game)
  - `MicCapture.vb` — WasapiCapture (mic)
  - `AudioPipe.vb` — named pipe + writer thread
  - `MicPipe.vb` — mirror AudioPipe
- Modify `CaptureEngine.vb`:
  - Spawn pipes before FFmpeg starts
  - Stop pipes after FFmpeg exits
  - Replace `-f dshow -i audio="..."` with `-i \\.\pipe\...`
  - Build `amix` filter_complex when both sources enabled
- No UI changes

### Acceptance Criteria
| # | Criterion | How to verify |
|---|---|---|
| 1.1 | Build succeeds | `dotnet build` exit 0 |
| 2.1 | Game audio only — output has audio | `Stream #0:1: Audio: aac` in log |
| 2.2 | Mic only — output has audio | same |
| 2.3 | Both sources — amix in filter_complex | `-filter_complex "[1:a]...[2:a]...amix..."` |
| 2.4 | Video still mapped | `Stream #0:0 -> #0:0` |
| 2.5 | FFmpeg exit code = 0 | log tail |
| 2.6 | Output file plays both audio sources | manual listen |

### Out of scope
- AudioMixerForm UI (next phase)
- Volume control UI (settings only)
- Intel QSV

### PR
- One PR: `phase 2: NAudio audio backend`
- Touch: new files in `Engine/Engine/` + `CaptureEngine.vb` + `CaptureSettings.vb`

---

## Phase 3 — Settings + Mixer UI

**Goal**: Wire audio settings through Overlay's `video.json` and provide a Mixer dialog.

### Scope
- Add fields to `CaptureSettings.vb`:
  - `SystemAudioCapture`, `SystemAudioVolume`, `GameDeviceName`
  - `MicCapture`, `MicVolume`, `MicDeviceName`
- Update `SyncWithOverlayConfig` in `UI_Engine.vb` to read/write these
- Add `SaveVideoConfig` to `OverlayConfig.vb`
- Create `AudioMixerForm.vb` + Designer + resx in `Engine/Engine/`
- Add "Mixer..." button to `UI_Engine.Designer.vb`
- Wire button click handler in `UI_Engine.vb`

### Acceptance Criteria
| # | Criterion |
|---|---|
| 1.1 | Build succeeds |
| 2.1 | Open Mixer form — no crash |
| 2.2 | Device dropdowns populated |
| 2.3 | Volume sliders work |
| 2.4 | Peak meters update during recording |
| 2.5 | Apply saves to `shadowplay-config.json` AND `video.json` |
| 2.6 | Cancel does not persist changes |

### PR
- One PR: `phase 3: audio mixer UI + settings sync`

---

## Phase 4 — Intel QSV

**Goal**: Make QSV path match official FFmpeg documentation.

### Scope
- Add `-init_hw_device d3d11va:,vendor_id=0x8086` before ddagrab
- Add `format=qsv` after `hwmap=derive_device=qsv` in video filter
- Add `MapQsvPreset` to `OverlayConfig.vb` (1-7 → veryfast..veryslow)
- Add `-g <fps>` and `-fps_mode cfr` to QSV encoder args
- Remove deprecated `-rc cbr` for QSV

### Acceptance Criteria
| # | Criterion |
|---|---|
| 1.1 | Build succeeds |
| 2.1 | QSV recording produces video > 0 KiB |
| 2.2 | Bitrate within ±10% of target (was ~95% before) |
| 2.3 | FPS stable (no drops) |
| 2.4 | Preset slider in Overlay UI affects QSV preset |

### PR
- One PR: `phase 4: Intel QSV parity with NVENC`

---

## Rules Across All Phases

1. **One PR per phase** — no mixed-phase commits
2. **Clean build before merge** — see `docs/BUILD_PROTOCOL.md`
3. **No force-push** without explicit agreement
4. **No code transplant** from old `CaptureEngine/` — write fresh
5. **FFmpeg official docs > AI assumption > source comments** — when in doubt, check ffmpeg.org
6. **Runtime log > source claim** — if log says X but source says Y, log wins
