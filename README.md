Please check the app information at https://scotcsduluka.github.io/NVIDIA-Shadowplay/ as the information in this README may not be current.

# <img src="https://cdn2.steamgriddb.com/icon/e8855b3528cb03d1def9803220bd3cb9/32/48x48.png" alt="NVIDIA ShadowPlay Logo" width="22"> NVIDIA ShadowPlay `Custom Implementation`

> [!NOTE]
> A third-party screen capture utility inspired by NVIDIA ShadowPlay, built primarily in **VB.NET** with a WinForms overlay and **no game-process hook**.
>
> The current modular recording engine is built around **Desktop Duplication + native NVIDIA NVENC + WASAPI + FFmpeg**.

> [!IMPORTANT]
> ## Current Engine
>
> The active `Engine-Rebuild-Stabilization` branch uses the following production recording path:
>
> ```text
> DdagrabBackend (DXGI Desktop Duplication)
>         ↓
> D3D11 video frame
>         ↓
> Native NVENC H.264
>         ↓
> LiveMuxSession
>         ↓
> FFmpeg
>         ↓
> MP4 output
> ```
>
> Audio is handled separately through the shared WASAPI audio path:
>
> ```text
> WASAPI System Audio ─┐
>                      ├→ Audio timeline / live mux → FFmpeg → MP4
> WASAPI Microphone ───┘
> ```

## Runtime Requirements

- **Windows x64**
- **.NET 10 Desktop Runtime / SDK** for the current projects
- An **NVIDIA GPU with NVENC H.264 support** for the production encoder path
- FFmpeg runtime payload for recording/muxing

> [!WARNING]
> The current modular Engine is **NVIDIA-focused**. Intel QSV and AMD AMF are not production encoder paths in this branch.

> [!CAUTION]
> DRM-protected content may not be capturable. Capture behavior also depends on the Windows display/capture restrictions of the target application.

---

# Capture & Recording

### Production capture backend

- [x] **DXGI Desktop Duplication / `DdagrabBackend`**
- [ ] Windows Graphics Capture as the current production backend
- [ ] GDI / `gdigrab` as the current production backend

The modular Engine currently has **one production video backend: DdagrabBackend**. Other capture methods may exist in legacy code, contracts, or research, but they are not represented here as active production backends.

### Frame & ownership model

- D3D11 capture textures are owned by the capture backend.
- Per-frame resources are transferred to the frame sink when accepted.
- Dropped frames are disposed by the producer.
- Capture and encoder lifetimes are independent and explicitly managed.
- Start / Stop / Dispose / Restart behavior is covered by lifecycle tests.

### Video format

```text
Desktop capture: BGRA8 / D3D11
        ↓
NVENC input: ARGB path
        ↓
H.264 output: yuv420p in MP4
```

### Encoder

- [x] NVIDIA NVENC
- [x] H.264 production path
- [x] Configurable bitrate / rate control / preset / GOP
- [x] Runtime FPS reconciliation through encoder rebuild when required
- [ ] Intel QSV production path
- [ ] AMD AMF production path
- [ ] NVENC HEVC production integration
- [ ] NVENC AV1 production integration

The current native NVENC backend rejects unsupported codec keys instead of silently substituting another encoder.

---

# Resolution & FPS

The recording engine separates **capture resolution** from **encode resolution**.

- Native-resolution mode captures the desktop and encodes at the captured size.
- A smaller requested encode size is handled by NVENC GPU scaling.
- Upscaling above the captured desktop size is rejected rather than silently falling back.
- Display refresh rate is used where required for video timing evidence.
- Per-session FPS changes rebuild the persistent encoder before frames are submitted so the native stream timing stays consistent.

---

# Audio

The active modular recording path supports:

- [x] WASAPI system / loopback audio
- [x] WASAPI microphone capture
- [x] Separate system and microphone timelines
- [x] Audio gap accounting / bounded delivery
- [x] Device-clock-aware system audio path in the current implementation
- [x] Audio/video timeline alignment before live muxing

The audio hot path is designed so capture callbacks do not perform disk I/O directly.

---

# Live Mux / FFmpeg

The current recording path uses an **OBS-style live mux architecture**:

```text
Video H.264 ───────────────┐
                           ├→ named pipes → one FFmpeg process → fragmented MP4
System audio PCM ──────────┤
Microphone PCM ────────────┘
```

The FFmpeg layer is responsible for process lifetime, stderr draining, stream coordination, and final output handling.

The live mux path includes bounded queues and explicit byte/drop accounting. Video production is protected from arbitrary packet dropping that could corrupt an H.264 access sequence.

---

# Recording Engine Architecture

The current modular engine is split into focused projects:

| Project | Responsibility |
|---------|----------------|
| `CaptureEngine` | Core contracts and engine configuration |
| `CaptureEngine.Video` | Video frame contracts and handoff |
| `CaptureEngine.Video.Ddagrab` | DXGI Desktop Duplication capture |
| `CaptureEngine.Encoder` | Encoder abstraction and contracts |
| `CaptureEngine.Encoder.Nvenc` | Native NVIDIA NVENC H.264 backend |
| `CaptureEngine.Audio` | Shared audio engine and audio types |
| `CaptureEngine.Audio.Wasapi` | WASAPI / device-clock capture support |
| `CaptureEngine.FFmpegBackend` | FFmpeg process, live mux, timeline helpers |
| `CaptureEngine.Recording` | Recording session orchestration |
| `CaptureEngine.Recording.ConsoleDriver` | Headless recording/test driver |

The process-lifetime `RecordingEngine` owns the persistent capture and encoder backends and creates a `CaptureSession` for each recording session.

---

# Duluka Account

The project includes a **Duluka Account** identity layer for account, device, and session management.

```text
Duluka Account
├── Username
├── Display Name
├── Profile Image
├── Devices
├── Sessions
└── Linked Providers
    └── GitHub
```

GitHub is a linked provider/authentication mechanism. It is **not** the Duluka Account identity itself.

Current account work includes native username/password authentication, device registration, session persistence, and provider linking. Profile editing is kept separate from the immutable account username semantics.

---

# Current Development Status

The main active development branch is:

```text
Engine-Rebuild-Stabilization
```

Current engine focus:

- Modular capture engine stabilization
- Ddagrab lifecycle and ownership hardening
- Native NVENC H.264 integration
- Video configuration authority (FPS, bitrate, rate control, preset, GOP, resolution)
- WASAPI system/microphone audio and timeline handling
- Live FFmpeg muxing and A/V synchronization
- Recording lifecycle and regression coverage

The engine has extensive deterministic contract, lifecycle, recording, encoder, FFmpeg, concurrency, and configuration tests. Hardware-specific validation is tracked separately and is not treated as equivalent to software-only test proof.

> [!NOTE]
> The repository contains historical experiments, legacy engine code, and research spikes. Their presence does not mean they are the active production recording path.

---

# Project Layout

```text
NVIDIA-Shadowplay/
├── CaptureEngine/                    Core contracts and configuration
├── CaptureEngine.Video/              Video frame contracts / handoff
├── CaptureEngine.Video.Ddagrab/      DXGI Desktop Duplication backend
├── CaptureEngine.Encoder/             Encoder contracts
├── CaptureEngine.Encoder.Nvenc/      Native NVIDIA NVENC backend
├── CaptureEngine.Audio/               Shared audio engine
├── CaptureEngine.Audio.Wasapi/        WASAPI capture / device-clock support
├── CaptureEngine.FFmpegBackend/       FFmpeg process + live mux + sync helpers
├── CaptureEngine.Recording/           Recording session orchestration
├── Engine/                             Legacy/runtime integration
├── Overlay/                            Overlay UI and account experience
├── Launcher/                           Desktop launcher
├── Notifier/                           Notification / event forwarding
├── Duluka/                             Duluka Account / server components
├── Tester/test/                        Automated test projects
├── docs/                               Architecture, audits, status, protocols
├── spikes/                             Hardware/API research probes
├── scripts/                            Build and diagnostics
└── installer/                          Installer source
```

See [`PROJECT-STRUCTURE.md`](PROJECT-STRUCTURE.md) for the repository map.

---

# Development

| Branch | Purpose |
|--------|---------|
| **`Stable`** | Stable/release-oriented branch |
| **`Engine-Rebuild-Stabilization`** | Active modular Engine development and stabilization |

Important engineering documentation lives in [`docs/`](docs/).

The repository deliberately separates **architecture claims**, **implementation status**, **automated test proof**, and **hardware runtime proof** so that unfinished or hardware-gated components are not presented as production-ready.

---

# Limitations

- No-hook capture cannot guarantee capture of every application or display mode.
- DRM-protected content may be blocked or captured as black frames.
- The current modular production video backend is NVIDIA/DXGI-focused.
- Intel QSV and AMD AMF are not current production encoder paths.
- Some hardware validation requires an NVIDIA system with a compatible NVENC-capable GPU.

---

# License

![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

This project is distributed under the MIT License. See [`LICENSE`](LICENSE).

Third-party attribution is listed in [`LICENSE.NOTICE`](LICENSE.NOTICE).

---

# Disclaimer

> [!CAUTION]
> This is an **independent third-party application** and is **NOT affiliated with, endorsed by, sponsored by, or approved by NVIDIA Corporation**.
>
> **NVIDIA**, **GeForce**, and **ShadowPlay** are trademarks or registered trademarks of NVIDIA Corporation in the United States and/or other countries.
>
> This project uses technologies such as NVIDIA NVENC, Microsoft Windows APIs, NAudio, and FFmpeg according to their respective licenses and runtime requirements.

---

# Third-Party Components

| Component | Purpose |
|-----------|---------|
| [FFmpeg](https://ffmpeg.org/) | Live muxing and media processing |
| [NAudio](https://github.com/naudio/NAudio) | Windows audio capture support |
| [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows) | D3D11 / DXGI interop |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | JSON support in legacy/integration components |

See [`LICENSE.NOTICE`](LICENSE.NOTICE) for the complete attribution list.

---

# Contributing

Contributions are welcome.

1. Fork the repository.
2. Create a feature branch.
3. Make and test your changes.
4. Open a Pull Request with a clear description of the change and validation performed.

For Engine work, please read the relevant documents in [`docs/`](docs/) before changing architecture or lifecycle contracts.

---

# Credits

| Role | Name | Description |
|------|------|-------------|
| **Creator & Lead Developer** | [ScotcsDuluka](https://github.com/ScotcsDuluka) | Architecture, UX, Core Engine, Overlay System, Animation Framework |
| **Testing & Validation** | — | Testing, Validation, Stability Assurance |

---

# Links

| Resource | Link |
|----------|------|
| **Project Website** | [NVIDIA ShadowPlay](https://scotcsduluka.github.io/NVIDIA-Shadowplay/) |
| **Creator** | [ScotcsDuluka](https://github.com/ScotcsDuluka) |
| **Releases** | [GitHub Releases](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases) |
| **Issues** | [GitHub Issues](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/issues) |

---

*Independent implementation inspired by NVIDIA ShadowPlay — continuously evolving.* ❤️
