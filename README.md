Please check the app information at https://scotcsduluka.github.io/NVIDIA-Shadowplay/ as the information in this README may not be current.

# <img src="https://cdn2.steamgriddb.com/icon/e8855b3528cb03d1def9803220bd3cb9/32/48x48.png" alt="NVIDIA ShadowPlay Logo" width="22"> NVIDIA ShadowPlay

[**OPEN SOURCE PROJECT**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#)

- [**OBT3 LIVE →**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/obt3.html)
- [**Features**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#features)
- [**Overlay**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#overlay)
- [**API Capture**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#api-specs)
- [**Requirements**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#requirements)
- [**Architecture**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#architecture)
- [**FAQ**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#faq)
- [**Team**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/#team)
- [**Discord Server**](https://discord.gg/v5qUGVD3jZ)

# Capture Every Moment

### Zero latency. Zero compromise. Built for performance.

**Flagship Build — OBT3**

> The current open beta build focuses on the rebuilt audio/video timeline and the production native capture path.

**BUILD: OBT3 · SEP 2026**
**SDK 26100 · NVENC READY · A/V 0.000s**

[**✦ WHAT'S NEW — OBT3**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/obt3.html)
Side-by-side comparison and measured results are available on the OBT3 page.

---

# Overlay

Precision-engineered, borderless overlay that keeps you in control without breaking immersion.

## In-game Overlay

- Borderless Windowed design
- Real-time performance checks
- Keyboard shortcut access
- [View Supported Games](https://scotcsduluka.github.io/NVIDIA-Shadowplay/games.html)

![In-game Overlay UI](https://scotcsduluka.github.io/NVIDIA-Shadowplay/Assets/overlay.png)

---

# Downloads

## NVIDIA ShadowPlay OBT3

**Open Beta Test 3 · Windows x64**

The current flagship build.

Audio-timeline rebuild, fixed video-only path, honest pass reporting, Intel QSV through the FFmpeg engine path, and WASAPI on a common QPC time base.

[**Download OBT3 Installer**](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases/download/Release-OBT-3/NVIDIA.ShadowPlay.OBT3.exe)

[**See OBT3 vs Stable →**](https://scotcsduluka.github.io/NVIDIA-Shadowplay/obt3.html)

## NVIDIA ShadowPlay 3.3.2057.1

**Windows x64 · App + Runtime + Resources**

A stable release for everyday use.

[**Download Stable Installer**](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases/download/3.3.2057.1-BETA/NVIDIA.ShadowPlay.3.3.2057.1-BETA.exe)

## NVIDIA ShadowPlay 3.35.2542.30

**Windows x64 · Experimental · App + Runtime + Resources**

An experimental build for users who want to test earlier changes and ongoing improvements.

[**Download Pre-Release Installer**](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases/download/3.35.2542.30-PRE/NVIDIA.ShadowPlay.3.35.2542.30-PRE.exe)

---

# Features Overview

## Capture Your Best Moments

Professional-grade features inspired by NVIDIA ShadowPlay.

## Real-time Screen Recording

Record gameplay or desktop in real time with hardware-accelerated video encoding when the native NVIDIA path is available.

- Hardware-accelerated encoding
- Low CPU overhead on the NVIDIA path
- FFmpeg-based alternative engine path

## Instant Replay

The replay feature is part of the application architecture, but the rebuilt production path is still under active development and should not be treated as fully available in the current flagship build.

## Screenshot

High-resolution screenshot capture remains under active development in the current rebuild.

## No-Hook Architecture

Designed without memory injection or gameplay hooks. The overlay uses supported Windows APIs and global hotkeys instead of injecting into the target application.

## NVIDIA Hardware Acceleration

The native production engine uses NVIDIA NVENC for in-app H.264 encoding with a D3D11 texture submission path.

---

# Encoder & Capture

## Capture Engines

The project currently has two engine regimes:

| Engine | Status | Purpose |
|---|---|---|
| **Duluka Capture** | **Production** | Native D3D11 + NVIDIA NVENC path |
| **FFmpeg Capture** | **Supported / Broader** | FFmpeg-managed capture and encoding paths |
| **OBS Capture** | **Soon** | Event forwarding bridge only |

The native production path is the engine behind the rebuilt recording flow and overlay integration.

## Encoder Support

### NVIDIA NVENC

**Ready**

Native in-app NVIDIA encoding on the production path. Current backend targets H.264 with CBR/CFR configuration, D3D11 texture submission, per-session frame-rate reconciliation, and explicit keyframe handling.

### Intel QSV

**FFmpeg Engine Path**

Intel QSV belongs to the FFmpeg-engine regime rather than the native Duluka NVENC backend.

### AMD AMF

**Soon**

AMD Advanced Media Framework support remains on the roadmap.

---

# API Capture

## DXGI Desktop Duplication

**Active**

The production capture path. `DdagrabBackend` uses DXGI Desktop Duplication with D3D11 resources and driver presentation timestamps in the QPC time domain.

## Windows.Graphics.Capture

**Coming**

The modular engine has a dedicated `GfxCapture` slot, but it is not the active production backend in the current native path.

## Window / Region / Game Capture

**Coming**

Additional capture modes remain planned for future backend implementations.

---

# Requirements

## System Requirements

### Operating System

**Windows 10 (19045+) / Windows 11 · 64-bit**

The application targets **.NET 10**. Older Windows versions are not supported.

### .NET Runtime

**.NET Desktop Runtime 10 (x64)**

The installer may include the required runtime package depending on the release. Install .NET Desktop Runtime 10 separately when the selected package does not bundle it.

### Hardware

For the native production capture/encode path:

- NVIDIA GPU with a supported NVENC encoder
- Windows D3D11 / DXGI support
- Adequate GPU memory and system resources for the selected capture and encode resolution

---

# Known Limitations

- **DRM content:** Services such as Netflix and Disney+ can block screen capture through Windows content protection.
- **Exclusive fullscreen:** The no-hook design does not guarantee capture of exclusive-fullscreen applications. Borderless Windowed mode is recommended.
- **Hardware-gated paths:** Native capture and NVENC require an NVIDIA-capable machine. Some validation suites intentionally skip hardware-only checks on systems without the required adapter.

## Performance Optimization

For best results, use the NVIDIA hardware path with an appropriate NVENC profile and a practical capture resolution such as **1920 × 1080** when your hardware and display permit it.

---

# Duluka Account

Duluka Account is the primary identity layer for the application.

```text
Duluka Account
├── Username
├── Password
├── Display Name
├── Profile Image
├── Devices
├── Sessions
└── Linked Providers
    └── GitHub
```

GitHub is a linked provider / authentication mechanism. **GitHub Account ≠ Duluka Account.**

The account system supports native username/password authentication and provider linking. Account profile data such as display name and profile image belongs to the Duluka Account, not to the GitHub identity.

---

# Architecture

## Modular Capture Engine

The rebuilt engine is split into focused modules with explicit ownership and lifecycle boundaries.

### CaptureEngine/

Core contracts, configuration, diagnostics, and engine-level orchestration.

### Capture — Ddagrab/

Production video capture using DXGI Desktop Duplication on D3D11. Frames carry capture-side presentation timing and ownership is transferred through the bounded frame handoff.

### Encoder — NVENC/

Native NVIDIA encoder backend. The encoder owns its D3D11/NVENC resources and consumes caller-owned video frames without taking frame ownership.

### Audio — WASAPI/

Shared WASAPI audio capture for system audio and microphone. The rebuilt path keeps audio timing in the same QPC clock domain and uses explicit queue/drop accounting.

### Live Mux — FFmpeg/

Named-pipe live mux path. Encoded H.264 and PCM audio are fed into FFmpeg during recording, producing fragmented MP4 before finalization.

### Overlay/

Borderless in-game overlay with recording controls, settings, hotkeys, account UI, and gallery-related integration. The overlay does not own the engine's native GPU resources.

### Hub — NVIDIA API/

Local application hub used to coordinate the app family and carry engine commands between the desktop components.

## Two Engine Regimes

The current project intentionally keeps two distinct regimes:

```text
Duluka Native Engine
    D3D11 Desktop Duplication
        ↓
    Native NVENC H.264
        ↓
    Audio / LiveMux orchestration

FFmpeg Engine Regime
    FFmpeg-managed capture backends
        ↓
    FFmpeg encoders / filters
        ↓
    Wider hardware support, including Intel QSV paths
```

The architecture keeps capture, encoding, audio, and output responsibilities separated so individual backends can evolve without redefining the entire recording pipeline.

---

# Frequently Asked Questions

### Is the application a modified NVIDIA ShadowPlay?

No. This is an independent third-party implementation inspired by the workflow and user experience of NVIDIA ShadowPlay.

### Does it use memory injection or hooks?

No. The design uses Windows capture APIs, D3D11 resources, FFmpeg, and supported hotkey mechanisms instead of injecting into games.

### Can it record Netflix or other DRM video?

Some DRM-protected applications can block screen capture. This is a Windows/content-protection limitation rather than an application feature toggle.

### Does it support Intel GPUs?

Yes, through the FFmpeg engine regime where the corresponding FFmpeg/QSV path is available and validated. The native Duluka production engine currently targets NVIDIA NVENC.

### Does it support AMD?

AMD AMF remains on the roadmap.

### How can I contribute?

Fork the repository, make your changes, test them, and submit a pull request.

---

# Meet The Team

Driven by 3+ years of continuous development, testing, experimentation, and refinement.

## ScotcsDuluka (Agkarath Truajnok)

**Builder · Owner · Lead Developer**

Creator and driving force behind the project, responsible for architecture, UX, core engine development, overlay system, animation framework, and product direction.

## Natthawut Fueangkaew

**Architect of Continuity · Supporting Contributor**

Supports the project's continuity through resources, encouragement, and long-term project support.

## Apiwit Kaemanee

**Core Contributor · QA Specialist · System Tester**

Contributes through rigorous testing, validation, issue discovery, and system stability verification.

---

# License

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

This project is released under the MIT License. See:

- [LICENSE](LICENSE)
- [LICENSE.NOTICE](LICENSE.NOTICE)

---

# Third-Party Components

| Component | License | Source |
|---|---|---|
| [NAudio](https://github.com/naudio/NAudio) | MIT | Audio capture support |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | MIT | JSON support |
| [libmp3lame](https://lame.sourceforge.io/) | LGPL-2.0 | Audio codec dependency |
| [FFmpeg](https://ffmpeg.org/) | LGPL/GPL | Encoding and live mux engine |
| [.NET](https://dotnet.microsoft.com/) | MIT | Runtime/platform |
| [Windows SDK / WinRT](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/) | Microsoft licensing | Windows API interop |

See full attribution in [LICENSE.NOTICE](LICENSE.NOTICE).

---

# Disclaimer

> [!CAUTION]
> **Trademark Notice:** NVIDIA ShadowPlay is an independent third-party application and is **NOT affiliated with, endorsed by, sponsored by, or approved by NVIDIA Corporation**.
>
> "NVIDIA", "GeForce", and "ShadowPlay" are trademarks or registered trademarks of NVIDIA Corporation in the United States and/or other countries.
>
> This project uses Microsoft Windows APIs and NVIDIA encoding technology through documented interfaces and/or FFmpeg components as applicable to each engine regime.

---

# Contributing

1. Fork the project.
2. Create a feature branch.
3. Make focused changes.
4. Run the relevant tests and builds.
5. Push your branch.
6. Open a Pull Request.

---

# Contact & Links

| Resource | Link |
|---|---|
| **Project Website** | [NVIDIA ShadowPlay](https://scotcsduluka.github.io/NVIDIA-Shadowplay/) |
| **OBT3** | [Live OBT3 page](https://scotcsduluka.github.io/NVIDIA-Shadowplay/obt3.html) |
| **Repository** | [GitHub](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay) |
| **Releases** | [GitHub Releases](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases) |
| **Issues** | [GitHub Issues](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/issues) |
| **Creator** | [ScotcsDuluka](https://github.com/ScotcsDuluka) |
| **Discord** | [Discord Server](https://discord.gg/v5qUGVD3jZ) |

---

*Crafted with passion, engineered for performance, and continuously evolving.* ❤️
