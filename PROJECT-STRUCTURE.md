# Project Structure

This repository is split into several app modules and a modular capture-engine
solution that lives beside them.

## Applications (VB.NET WinForms)

- `API/`
  Windows capture API host and related WinForms UI files.
- `App Experience/`
  Main desktop app experience and helper UI.
- `Engine/`
  Legacy runtime capture engine — FFmpeg process management, capture settings,
  encoder detection, and Engine UI.
- `Notifier/`
  Notification app and overlay-related notifier flow.
- `Overlay/`
  Overlay UI, resources, local runtime data, and the main overlay solution.
  Bundles `Overlay/API-Core/ffmpeg.exe` and related FFmpeg DLLs at runtime
  (that directory is local-only and ignored).
- `Web/`
  Static website assets and pages for the project site.

## Capture Engine (modular, active development)

The engine was split out of the legacy `Engine/` project into focused
projects. These are where current stabilization work happens
(branch: `Engine-Rebuild-Stabilization`).

Core pipeline:

- `CaptureEngine/`
  Session orchestration and shared engine core.
- `CaptureEngine.Video/`
  Frame capture sources (WGC / Desktop Duplication pipeline contracts).
- `CaptureEngine.Video.Ddagrab/`
  DDAGRAB-based capture backend.
- `CaptureEngine.Encoder/`
  Encoder abstraction layer.
- `CaptureEngine.Encoder.Nvenc/`
  NVENC hardware encoder backend.
- `CaptureEngine.FFmpegBackend/`
  FFmpeg process plumbing: audio taps, sidecars, live muxing.
- `CaptureEngine.Recording/`
  Recording session state machine (start/stop/pause, instant replay).
- `CaptureEngine.Audio.Wasapi/`
  P13.2 position-aware WASAPI capture: direct COM interop (verbatim from the
  P13.1 spike) + `WasapiPositionCapture` (Windows runtime) +
  `AudioPositionTracker` (pure, Linux-testable stamp math).
- `CaptureEngine.Recording.ConsoleDriver/`
  Console host for driving recording sessions headless (tests / diagnostics).

Test projects (kept in-tree, run by build scripts):

- `CaptureEngine.Tests/`
- `CaptureEngine.Video.Tests/`
- `CaptureEngine.Encoder.Tests/`
- `CaptureEngine.Recording.Tests/`
- `CaptureEngine.FFmpegTests/`
- `CaptureEngine.FrameContractTests/`
- `CaptureEngine.ConfigTests/`

## Support directories

- `docs/`
  Phase plans, architecture specs, postmortems, NVENC contract notes, and
  status snapshots. Start at `docs/PHASE_PLAN.md`.
- `spikes/`
  Throwaway C# probes used to isolate hardware/API behavior (WGC timestamps,
  NVENC benchmarks) before touching production code. Evidence for several
  sync fixes traces back here.
- `scripts/`
  Build and diagnostic entry points (`build-all.ps1` / `build-all.sh`),
  startup stress, sync verification, phase validation.

## Notes

- `bin/`, `obj/`, `.vs/`, restored packages, and local runtime data are
  intentionally ignored. Archives (`*.zip`), shortcuts (`*.lnk`), and other
  local-only artifacts are ignored too — see `.gitignore`.
- NVIDIA Video Codec SDK headers (`nvEncodeAPI.h` etc.) are NOT tracked;
  the OWNER keeps the SDK zip locally for the NVENC work.
- Some folders contain designer-generated WinForms files inside bracketed
  directories such as `[Forms - Project Files]`. Those are source files and
  should stay tracked.
