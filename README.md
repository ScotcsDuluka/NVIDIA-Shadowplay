Please check the app information at https://scotcsduluka.github.io/NVIDIA-Shadowplay/ as this README may not always reflect the latest application status.

# <img src="https://cdn2.steamgriddb.com/icon/e8855b3528cb03d1def9803220bd3cb9/32/48x48.png" alt="NVIDIA ShadowPlay Logo" width="22"> NVIDIA ShadowPlay `Custom Implementation`

> [!NOTE]
> A custom screen capture utility inspired by NVIDIA ShadowPlay, built primarily in **<img src="https://raw.githubusercontent.com/github/explore/refs/heads/main/topics/visual-basic/visual-basic.png" alt="Visual Basic Logo" width="15"> VB.NET** with a **no-hook** capture design.<br>
> Recording and encoding are powered by **FFmpeg**, with modular capture and encoder components under active development.

> [!WARNING]
> This project is still under active development. Some applications and protected/DRM content may not be capturable, depending on the capture path and operating-system restrictions.

> [!IMPORTANT]
> **Runtime Dependency:** The current desktop application targets **.NET 10** and requires the **.NET 10 Desktop Runtime** when deployed as framework-dependent.<br><br>
> **OS Requirement:** Windows is required. Supported operating-system versions depend on the .NET 10 and Windows API requirements of the selected build. Microsoft currently lists .NET 10 support for supported Windows 10 LTSC/Enterprise releases, Windows 11, and supported Windows Server releases. See the official .NET Windows support matrix for the current platform details.

## API Capture
- Windows.Graphics.Capture
- Desktop Duplication API
- GDI screen grabber

## Encoder Status
- [x] NVIDIA / NVENC ready
- [ ] Intel / Quick Sync
- [ ] AMD / AMF

> [!CAUTION]
> **Exclusive Fullscreen Limitation:** Because this project uses a no-hook capture design, **Exclusive Fullscreen** applications may not be capturable on some older Windows builds. For reliable recording, use **Borderless Windowed** mode when possible.<br>
> **Recommended capture resolution:** 1920 × 1080

> [!TIP]
> For the best performance on supported NVIDIA hardware, use the NVIDIA hardware encoder (`h264_nvenc`) through FFmpeg.

---

# Features
- [x] NVIDIA-style overlay UI
- [x] Real-time screen recording
- [x] Instant Replay / save the last moments
- [x] Screenshot capture
- [x] In-game overlay UI for Borderless Windowed applications
- [x] Modular capture-engine architecture
- [x] NVIDIA NVENC encoding path
- [ ] Intel encoder support
- [ ] AMD encoder support

# Duluka Account
Duluka Account is the project's account and identity layer. **GitHub is not the Duluka Account itself**; when used, GitHub is treated as a linked provider/authentication mechanism.

Conceptually:

```text
Duluka Account
├── Account Identity
├── Devices
├── Sessions
├── Profile
│   ├── Username
│   ├── Display Name
│   └── Profile Image
└── Linked Providers
    └── GitHub
```

The account system is under active development and includes native username/password authentication alongside GitHub-based account bootstrap/linking flows. The native account identity remains separate from any external provider identity.

---

## Development

| Branch | Purpose |
|--------|---------|
| **`Stable`** | Default branch — last known-good build |
| **`Engine-Rebuild-Stabilization`** | Active development — modular capture engine, lifecycle hardening, sync/audio stabilization, and account integration |

The project is being developed as a modular capture stack rather than a direct copy of NVIDIA's internal implementation.

Engine documentation lives in [`docs/`](docs/), starting with [`docs/PHASE_PLAN.md`](docs/PHASE_PLAN.md). The module map is documented in [`PROJECT-STRUCTURE.md`](PROJECT-STRUCTURE.md), with build and diagnostic entry points under [`scripts/`](scripts/).

### Current Engineering Focus
- Capture backend lifecycle and ownership
- Video frame delivery and timestamp correctness
- NVENC configuration authority and synchronization
- FFmpeg recording/mux integration
- Duluka Account authentication and identity semantics
- Regression and integration testing

---

## Requirements

- Windows desktop environment
- .NET 10 Desktop Runtime for framework-dependent desktop builds
- NVIDIA GPU recommended for the NVENC path
- FFmpeg components required by the selected recording pipeline

For the most accurate platform support information, also check the application website listed at the top of this README.

---

## License

![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

| File | Description | Click to View |
|---------|-------------|------------------|
| **[LICENSE](LICENSE)** | MIT License | [Open LICENSE](LICENSE) |
| **[LICENSE.NOTICE](LICENSE.NOTICE)** | Third-Party Components Attribution | [Open LICENSE.NOTICE](LICENSE.NOTICE) |

---

## Disclaimer

> [!CAUTION]
> **Trademark Notice:** This is an **independent third-party application** and is **NOT affiliated with, endorsed by, sponsored by, or approved by NVIDIA Corporation**.
>
> **NVIDIA**, **GeForce**, and **ShadowPlay** are trademarks or registered trademarks of NVIDIA Corporation in the United States and/or other countries.
>
> This project uses third-party and platform technologies such as FFmpeg, NVIDIA NVENC, and Microsoft Windows capture APIs according to their respective licensing and platform terms. This project does not imply endorsement by NVIDIA.

---

## 📦 Third-Party Components

| Component | License | Author | Source |
|-----------|---------|--------|--------|
| [NAudio](https://github.com/naudio/NAudio) | MIT | Mark Heath | NAudio.Core.dll, NAudio.Wasapi.dll |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | MIT | James Newton-King | Newtonsoft.Json.dll |
| [libmp3lame](https://lame.sourceforge.io/) | LGPL-2.0 | The LAME Project | libmp3lame.32.dll, libmp3lame.64.dll |
| [FFmpeg](https://ffmpeg.org/) | LGPL/GPL | FFmpeg Developers | Recording / Encoding pipeline |
| [.NET](https://dotnet.microsoft.com/) | MIT | Microsoft | .NET runtime / desktop components |
| [Windows SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/) | Microsoft licenses | Microsoft | Windows API / WinRT components |

See the full attribution list in **[LICENSE.NOTICE](LICENSE.NOTICE)**.

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

For substantial engine or architecture changes, please review the documentation under [`docs/`](docs/) first.

---

## Credits

| Role | Name | Description |
|------|------|-------------|
| **Creator & Lead Developer** | [ScotcsDuluka](https://github.com/ScotcsDuluka) | Architecture, UX Design, Core Engine, Overlay System, Animation Framework |
| **Tester & QA** | [ApiwitKaemanee](https://www.facebook.com/profile.php?id=61577847980691) | Testing, Validation, Stability Assurance |

---

## Contact & Links

| Resource | 🔗 Link |
|------------|--------|
| **Website** | [NVIDIA ShadowPlay Custom Implementation](https://scotcsduluka.github.io/NVIDIA-Shadowplay/) |
| **Releases** | [GitHub Releases](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases) |
| **Report Bug** | [GitHub Issues](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/issues) |
| **Creator** | [ScotcsDuluka](https://github.com/ScotcsDuluka) |

---

*Crafted with passion, engineered for performance, and continuously evolving.* ❤️
