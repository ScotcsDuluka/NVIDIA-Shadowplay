# Phase 0 — Baseline (FREEZE)

> **Status**: FROZEN — no code changes until Phase 1 explicitly starts.
> **Date**: 2026-08-14
> **Branch**: `Engine-Audio`

## Purpose

This document records the known-good baseline state of the repository
after removing the orphan `CaptureEngine/` project. It is the single
source of truth that subsequent phases build on.

## Final Architecture Decision

| Component | Role | Status |
|---|---|---|
| `Engine/` | Runtime capture engine — FFmpeg process management, settings, UI | ✅ Active (only engine project) |
| `Overlay/` | Main overlay UI — bundles `Overlay/API-Core/ffmpeg.exe` | ✅ Active |
| `API/`, `App Experience/`, `Notifier/` | Supporting apps | ✅ Active |
| `Web/` | Static project site | ✅ Active |
| `CaptureEngine/` | Old NAudio-based donor project | ❌ **REMOVED** in Phase 0 |

**Rule**: From this point onward, `Engine/` is the only engine project.
No code is transplanted from the old `CaptureEngine/` folder. NAudio
audio code will be written fresh in Phase 2.

## Repository State

### Git
- **Branch**: `Engine-Audio`
- **Head commit before Phase 0**: `807a656` (UI #8 — last Engine-Capture commit)
- **Head commit after Phase 0**: _(filled in after commit)_

### Project Layout (verified)
```
NVIDIA-Shadowplay/
├── API/                          (TCP Hub server, .NET 8 WinForms)
├── App Experience/               (Helper app)
├── Engine/                       ← RUNTIME ENGINE
│   ├── NVIDIA Capture.vbproj     (net8.0-windows, WinForms, NAudio 2.3.0)
│   └── Engine/
│       ├── CaptureEngine.vb      (main — FFmpeg only)
│       ├── CaptureSettings.vb    (config schema)
│       ├── EncoderDetector.vb
│       ├── UI_Engine.vb          (main form)
│       └── [API]/
│           ├── BackgroundLogger.vb
│           ├── JobObjectGuard.vb
│           ├── OverlayConfig.vb
│           ├── TcpClientHelper.vb
│           └── [Engine] Client.vb
├── Notifier/
├── Overlay/                      ← RUNTIME UI + bundles ffmpeg.exe
│   ├── NVIDIA Overlay.vbproj
│   └── API-Core/                 (10 FFmpeg binaries, ~240MB, tracked)
├── Web/
└── docs/                         ← this folder
```

## Build Protocol

**Required**: clean build every time after `git pull` to prevent stale
DLL issues (this was the root cause of multiple false-positive bugs
during earlier development).

### Windows (cmd)
```cmd
cd "C:\My Project\NVIDIA-Shadowplay"

:: Pull latest
git fetch origin
git switch Engine-Audio
git pull origin Engine-Audio

:: Clean build artifacts (CRITICAL — prevents stale DLL)
dotnet clean ".\Engine\NVIDIA Capture.vbproj" -c Release
dotnet clean ".\Overlay\NVIDIA Overlay.vbproj" -c Release
rmdir /s /q Engine\bin
rmdir /s /q Engine\obj
rmdir /s /q Overlay\bin
rmdir /s /q Overlay\obj

:: Build Engine first (Overlay depends on it via ProjectReference)
dotnet build ".\Engine\NVIDIA Capture.vbproj" -c Release

:: Then build Overlay
dotnet build ".\Overlay\NVIDIA Overlay.vbproj" -c Release
```

### Build order matters
1. `Engine/NVIDIA Capture.vbproj` builds first
2. `Overlay/NVIDIA Overlay.vbproj` references Engine via `<ProjectReference>`
3. Engine DLL is copied to `Overlay/bin/Release/net8.0-windows10.0.26100.0/NVIDIA Capture.dll`
4. **This is the runtime DLL** — stack traces in crashes will show this path

## Runtime Component Map

```
┌────────────────────────────────────────────────────────────────┐
│  OVERLAY (process entry point)                                  │
│  Overlay/NVIDIA Overlay.vbproj                                  │
│  ├── Reads: Overlay/config.json (AppConfig)                     │
│  │         Overlay/video.json (VideoConfig)                     │
│  ├── Sends TCP: PREWARM_FFMPEG, RECORD_START, RECORD_STOP       │
│  └── Bundles: Overlay/API-Core/ffmpeg.exe + 9 DLLs (~240MB)    │
└─────────────────────┬──────────────────────────────────────────┘
                      │ ProjectReference (build-time)
                      ▼
┌────────────────────────────────────────────────────────────────┐
│  ENGINE (NVIDIA Capture.dll)                                    │
│  Engine/NVIDIA Capture.vbproj                                   │
│  └── Engine/                                                    │
│      ├── UI_Engine.vb                                           │
│      │   ├── HandleEngineRecordStart (entry)                    │
│      │   │   ├── CaptureSettings.Load(_configPath)              │
│      │   │   ├── SyncWithOverlayConfig(s)                       │
│      │   │   │   ├── OverlayConfig.LoadVideoConfig()            │
│      │   │   │   └── OverlayConfig.LoadConfig()                 │
│      │   │   │       └── appCfg.Paths.FFmpegPath                │
│      │   │   ├── New CaptureEngine(_settings)                   │
│      │   │   └── _captureEngine.StartRecordingAsync()           │
│      │   └── HandleEnginePrewarmFFmpeg (TCP handler)            │
│      ├── CaptureEngine.vb                                       │
│      │   └── BuildFFmpegArguments() — current state:            │
│      │       ├── -f lavfi -i "ddagrab=output_idx=0:..."         │
│      │       ├── -vf hwmap=derive_device=qsv (QSV only)         │
│      │       ├── -f dshow -i audio="..." (BROKEN — Phase 2)     │
│      │       ├── -c:v <encoder>                                 │
│      │       └── -c:a aac -b:a 320k                             │
│      ├── CaptureSettings.vb                                     │
│      │   ├── AudioCapture (legacy bool)                         │
│      │   ├── AudioDevice (legacy string)                        │
│      │   └── ConfigVersion = 2                                  │
│      └── [API]/OverlayConfig.vb                                 │
│          ├── VideoAudioConfig (defined, unused)                 │
│          ├── MapEncoderToFfmpeg()                               │
│          └── MapNvencPreset()                                   │
└────────────────────────────────────────────────────────────────┘
```

## FFmpeg Binary Resolution (priority order)

1. `appCfg.Paths.FFmpegPath` (from Overlay's `config.json`)
2. `PREWARM_FFMPEG` TCP message (Overlay → Engine on `engine_ready`)
3. `CaptureSettings.CreateDefault` fallback search:
   - `<appDir>/API-Core/ffmpeg.exe`
   - `<appDir>/ffmpeg.exe`
   - `<appDir>/Tools/ffmpeg.exe`
   - parent dirs (up to 5 levels) — `API-Core/ffmpeg.exe` or `api-core/ffmpeg.exe`
   - sibling `Overlay/bin/Release/*/api-core/ffmpeg.exe`

## Known Issues (carried forward, NOT fixed in Phase 0)

These are documented for visibility but **must not be touched** until
the relevant phase:

| ID | Issue | Phase that fixes it |
|---|---|---|
| R1 | Stale DLL in `Overlay/bin/Release/` if build not cleaned | Build protocol (this doc) |
| R4 | `-f dshow -i audio="..."` in CaptureEngine.vb line 434 | Phase 2 |
| R5 | `VideoAudioConfig` defined but `SyncWithOverlayConfig` doesn't use it | Phase 3 |
| R6 | `CaptureSettings` lacks `SystemAudioCapture`/`MicCapture`/etc. | Phase 3 |
| R7 | QSV path missing `-init_hw_device d3d11va:,vendor_id=0x8086` + `format=qsv` | Phase 4 |
| R9 | `MapQsvPreset` missing in OverlayConfig.vb | Phase 4 |

## Phase 0 Acceptance Criteria

- [x] `CaptureEngine/` folder removed from repo
- [x] No `CaptureCore` namespace references remain in source
- [x] No `..\CaptureEngine` path references in `.vbproj`/`.sln`/`.slnx`
- [x] `PROJECT-STRUCTURE.md` updated to reflect new layout
- [x] `docs/PHASE0_BASELINE.md` exists (this file)
- [ ] `dotnet build Engine/NVIDIA Capture.vbproj` exit 0 (verify on Windows machine)
- [ ] `dotnet build Overlay/NVIDIA Overlay.vbproj` exit 0 (verify on Windows machine)

The build verification steps must be performed on a Windows machine
with .NET 8 SDK installed. The sandbox used to write this doc does not
have the .NET SDK available.

## Next Steps

**Phase 1 — NVIDIA Video Only**: Make video-only recording work
end-to-end with NVENC, no audio. Acceptance criteria will be defined
in `docs/PHASE1_NVENC_VIDEO.md` when Phase 1 starts.

Until then: **FREEZE**. No `.vb` file modifications.
