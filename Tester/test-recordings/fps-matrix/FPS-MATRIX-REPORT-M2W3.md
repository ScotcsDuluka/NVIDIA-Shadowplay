# FPS MATRIX REPORT — M2-W3 (FPS behavior / timing validation)

**Date:** 2026-09-11
**Machine:** Windows 11 Pro (26300) · Intel(R) UHD Graphics 0x8086:0x9b41 (+ inactive Parsec Virtual Display Adapter) · 1920×1080 @ 59 Hz · **no NVIDIA GPU**
**Repo:** `C:\My Project\NVIDIA-Shadowplay` (branch `Engine-Rebuild-Stabilization` per project docs; `git` is not installed on this machine — see §7)
**Mission:** validate target FPS 30 / 60 / 120 / 144 / 240 with fresh recordings, multiple runs per mode, from actual frame PTS.

---

## 1. Environment capability (measured, not assumed)

| Check | Result | Evidence |
|---|---|---|
| NVIDIA adapter present | **NO** — only Intel UHD (0x8086:0x9b41) ×2 + Microsoft Basic Render Driver | probe logs `native-probe-01/02.log` (adapter enumeration); `wmic win32_VideoController` |
| Native engine (Duluka: Ddagrab + NVENC) | **Cannot initialize** — `DdagrabBackend: no NVIDIA adapter found. Capture requires an NVIDIA GPU.` at `DdagrabBackend.vb:251` | `native-probe-01.log`, `native-probe-02.log` (2/2 identical) |
| Legacy FFmpeg regime (production `engine_mode=ffmpeg` path: ddagrab → `hwdownload,format=nv12` → `h264_qsv -rc cbr -b:v 20M -g <fps> -fps_mode cfr`) | **Works on this machine** for 30/60/120/144; **fails at 240** (§5) | this matrix, §4 |
| Monitor refresh | 59 Hz (so the desktop physically produces ~59–60 new frames/s max) | `wmic CurrentRefreshRate=59` |

Native-engine note: `RecordingEngine.Initialize` (RecordingEngine.vb:77–119) initializes the
capture backend **before** any FPS-dependent encoder configuration; therefore the NVIDIA-adapter
gate is **FPS-independent** — every target FPS mode is unreachable for the native engine on this
machine, one probe is evidence for all five modes (2 probes run to confirm determinism).

## 2. What was run

**Native engine (Duluka, production default):** `CaptureEngine.Recording.ConsoleDriver.exe --videocheck`
through the canonical config chain (`fps=60` echoed from effective config; failure precedes FPS handling).
Result: **BLOCKED** for all modes (§1).

**Legacy engine (production FFmpeg regime):** recordings produced by the REAL production engine class
`NVIDIA_Capture.CaptureEngine` via its public API (`StartRecordingAsync`/`StopRecordingAsync`),
driven by a new measurement harness `Tester\test\Engine\FpsMatrix\FpsMatrixDriver.vbproj`
(compile pattern copied from `Engine.Concurrency.Tests`; **no production code touched**).
Settings = production defaults: `CaptureMethod=ddagrab`, `Encoder=h264_qsv` (Intel QSV branch of
`FFmpegArgumentBuilder`), `RateControl=cbr`, `Bitrate=20 Mbps`, `PixelFormat=nv12`,
`UseNativeResolution=true`, `SystemAudioCapture=false`.

Deviations declared:
- **Audio disabled** → engine runs its single-process video-only mode (direct final MP4, no mux stage).
  Matrix scope is video PTS; audio path excluded so the measured grid is the video pipeline's own output.
- 5 s wall-time per run, 2 runs per mode (10 recordings attempted; 8 usable files).

## 3. Measurement method

- **Encoded cadence** measured from **actual frame PTS**: `ffprobe -select_streams v:0
  -show_entries frame=pts_time` over every decoded frame (not container tags, not average FPS).
  Analyzer: `analyze-pts.ps1` (this folder) + raw CSV `pts-matrix.csv`.
- ΔPTS = consecutive `pts_time` differences (ms). Container timebase 1/15360 s → ±0.033 ms
  quantization floor; at 144 fps (15360/144 is non-integer) deltas quantize to 6.944/6.945 ms —
  expected container rounding, not engine behavior.
- **Verdict rules (declared before reading results):**
  PASS = strictly monotonic PTS, 0 duplicate-PTS steps, 0 negative deltas, median Δ within 5 %+1 tick
  of target Δ, effective FPS ((n−1)/PTS-span) within 5 % of target, file decodable.
  FAIL = mode produced an unusable/no file or violated any rule above.
  BLOCKED = environment prevents the engine from reaching the FPS code path at all.

## 4. THE MATRIX

### 4a. Legacy FFmpeg regime (production `engine_mode=ffmpeg` path) — fresh recordings, 5 s × 2

| FPS | Run | Frames | Duration | First Δ | Median Δ | Mean Δ | P95 | P99 | Min | Max | Monotonic | Anomaly | Verdict |
|----:|:---:|-------:|---------:|--------:|---------:|-------:|----:|----:|----:|----:|:---------:|:--------|:-------:|
| 30 | 1 | 156 | 5.200 s | 33.333 ms | 33.333 ms | 33.3333 | 33.334 | 33.334 | 33.333 | 33.334 | YES | – | **PASS** |
| 30 | 2 | 156 | 5.200 s | 33.333 ms | 33.333 ms | 33.3333 | 33.334 | 33.334 | 33.333 | 33.334 | YES | – | **PASS** |
| 60 | 1 | 310 | 5.167 s | 16.667 ms | 16.667 ms | 16.6667 | 16.667 | 16.667 | 16.666 | 16.667 | YES | – | **PASS** |
| 60 | 2 | 310 | 5.167 s | 16.667 ms | 16.667 ms | 16.6667 | 16.667 | 16.667 | 16.666 | 16.667 | YES | – | **PASS** |
| 120 | 1 | 626 | 5.217 s | 8.333 ms | 8.333 ms | 8.3333 | 8.334 | 8.334 | 8.333 | 8.334 | YES | – | **PASS** |
| 120 | 2 | 624 | 5.200 s | 8.333 ms | 8.333 ms | 8.3333 | 8.334 | 8.334 | 8.333 | 8.334 | YES | – | **PASS** |
| 144 | 1 | 746 | 5.181 s | 6.944 ms | 6.944 ms | 6.9444 | 6.945 | 6.945 | 6.944 | 6.945 | YES | – | **PASS** |
| 144 | 2 | 749 | 5.201 s | 6.944 ms | 6.944 ms | 6.9444 | 6.945 | 6.945 | 6.944 | 6.945 | YES | – | **PASS** |
| 240 | 1 | 0 | — (0-byte file, `moov atom not found`) | — | — | — | — | — | — | — | — | QSV encoder-open rejection at ~0.8 s → ffmpeg exit −40 | **FAIL** |
| 240 | 2 | 0 | — (0-byte file, `moov atom not found`) | — | — | — | — | — | — | — | — | identical 2/2 | **FAIL** |

All PASS rows: duplicate PTS = 0, negative PTS = 0, effective FPS = exactly 30.000 / 60.000 /
120.000 / 144.000. Container tags agree (`avg_frame_rate=30/1 … 144/1`).

### 4b. Native Duluka engine (Ddagrab + NVENC) — all five modes

| FPS | Run | Recording produced | Evidence | Verdict |
|----:|:---:|:---|:---|:-------:|
| 30–240 (any) | probe 1, probe 2 | **No** — Initialize throws before session start | `native-probe-01.log`, `native-probe-02.log`: `no NVIDIA adapter found` (`DdagrabBackend.vb:251`), 2/2 identical | **BLOCKED** |

## 5. 240 fps FAIL — isolation evidence (cause located upstream of PTS behavior)

Per the mission's separation rule, this is **evidence attribution, not a final root-cause claim**:

1. Engine runs (2/2): `Could not open encoder before EOF` → `Task finished with error code: -22
   (Invalid argument)` → `Conversion failed!` → ffmpeg exit −40, state Recording→HasError at ~0.8 s,
   0-byte file (`driver-legacy-240-run1/2.log`).
2. **Isolation** (raw ffmpeg, engine-identical argument set, outside the engine): same failure,
   0-byte file (`raw-240-isolation.mp4`).
3. Parameter narrowing: `Current frame rate is unsupported` at 240 fps with bitrate 20M, 12M **and**
   8M → the QSV runtime rejects the **frame rate** (not the bitrate); control probe at 144 fps @20M
   succeeds (exit 0).
4. Gap: `CaptureSettings.Validate()` accepts FPS ≤ 240 (`CaptureSettings.vb:284-285`), so the mode
   is configurable and passed to the encoder without a QSV capability check on this hardware.

Conclusion within evidence: on **this machine's Intel UHD QSV runtime**, 240 fps is rejected at
encoder open, deterministically, engine-independent. Whether the GTX 1080 Ti / NVENC native path
accepts 240 fps is **not tested here** (environment-blocked) — no cross-machine claim is made.

## 6. Three-way separation (target FPS vs source capture cadence vs encoded cadence)

| Layer | Value | How measured |
|---|---|---|
| **Target FPS** | 30 / 60 / 120 / 144 / 240 (config request) | driver input → `settings.FPS` → `ddagrab=framerate=N` + `-g N -fps_mode cfr` |
| **Source capture cadence** (new content captured from the ~59 Hz desktop) | ~1.9 / 6.9 / 13.3 / 14.4 unique frames/s observed in run1 of 30/60/120/144 (lower-bound estimate: 32×18 gray decode-hash; static desktop during runs) | decode+hash uniqueness count |
| **Encoded cadence** (file PTS grid) | exactly N.000 fps CFR, monotonic, zero duplicate-PTS steps | ffprobe `pts_time` deltas (§4) |

The encoded grid is **synthetic CFR**: the file's PTS cadence is exact at every supported target
while the source contributed far fewer unique frames (static desktop, ≤59 Hz). The three layers
are reported separately above and never conflated into one "FPS" number.

## 7. Findings & compliance

1. **Finding — false success on failure:** for both 240 runs the engine surfaced
   `[engine-error]` + state `HasError`, but `StopRecordingAsync()` still returned `True` and the
   harness exit code was 0 with a 0-byte, un-decodable file (`result OK fps=240`).
   Consistent with the known `SessionResult.Pass`-style honesty pattern documented in HANDOFF §9 /
   F03 tests. **Not fixed** (out of mission scope) — flagged for the owner.
2. **Forbidden compliance:** no production timing code was modified (the only new source files are
   the measurement harness `Tester\test\Engine\FpsMatrix\*` and `analyze-pts.ps1`); **nothing was
   committed** (`git` is not installed on this machine; all artifacts are untracked files on disk);
   **no 38 ms root-cause conclusion is drawn from this matrix** — the "First Δ" column is the CFR
   grid spacing of the encoded stream, not a pipeline latency measurement, and the 38 ms question
   is untouched by this evidence set. The native engine's NVENC FPS-rebuild path (per-session FPS
   reconciliation) is also **not tested here** — it requires NVIDIA hardware (BLOCKED).
3. Machine note: `test-recordings\` artifacts referenced in older scripts (`baseline_*.txt`,
   `forensic-*.csv`) were empty stubs from other-machine runs; all evidence in this report was
   freshly generated on this machine (see file list below).

## 8. Artifacts (all under `test-recordings\fps-matrix\`)

- Recordings: `legacy-{30,60,120,144}-run{1,2}.mp4` (valid), `legacy-240-run{1,2}.mp4` (0-byte FAIL evidence),
  `raw-240-isolation.mp4` (0-byte isolation evidence)
- Driver logs: `driver-legacy-*.log` (10); native probes: `native-probe-01.log`, `native-probe-02.log`
- Analysis: `analyze-pts.ps1`, `pts-matrix.csv`
- Harness: `Tester\test\Engine\FpsMatrix\FpsMatrixDriver.vbproj` + `Program.vb` (untracked tooling)
