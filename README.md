Please check the app information at https://scotcsduluka.github.io/NVIDIA-Shadowplay/ as the information in this ReadME may not be current.


# <img src="https://cdn2.steamgriddb.com/icon/e8855b3528cb03d1def9803220bd3cb9/32/48x48.png" alt="NVIDIA ShadowPlay Logo" width="22"> NVIDIA ShadowPlay `Custom Implementation`


> [!NOTE]
> A screen capture utility inspired by NVIDIA ShadowPlay with overlay UI, built in **<img src="https://raw.githubusercontent.com/github/explore/refs/heads/main/topics/visual-basic/visual-basic.png" alt="Visual Basic Logo" width="15"> VB.NET** **[**No Hook**]**. <br>
> Record Engine Powered by FFmpeg

> [!WARNING]
> Some apps (**Netflix** / **DRM content**) cannot be recorded.<br>
> Still under active development.

> [!IMPORTANT]
> **Runtime Dependency:** This application requires **.NET 8.0 Desktop Runtime** and **4.8** installed on your system. The program will not launch without it.<br><br>
> **OS Requirement:** **Windows 8 / Server 2012 or newer is required.** It will not work on Windows 7. <br>
> ## **API Capture**
> - Windows.Graphics.Capture
> - Desktop Duplication API
> - GDI screen grabber
> ## **The **Encoder** is Ready**
> - [X] NVIDIA-READY
> - [ ] INTEL-NEXT
> - [ ] AMD-Q

> [!CAUTION]
> **Exclusive Fullscreen Limitation:** Due to the no-hook design, capturing "Exclusive Fullscreen" applications is not supported on older Windows builds. Please use "Borderless Windowed" mode in your games for reliable recording.<br>
> **Recommended capture resolution: 1920 x 1080**

> [!TIP]
> For the best performance, it is highly recommended to use NVIDIA hardware encoders (e.g., `h264_nvenc`) via FFmpeg.

---

# Features Main
- [X] UI NVIDIA Shadowplay
- [x] Real-time screen recording
- [x] Instant Replay (save last moments)
- [x] Screenshot capture
- [x] In-game overlay UI [`Borderless Windowed`]

# More
- This project is inspired by NVIDIA ShadowPlay.<br>
- Built over 3 years focusing on animation system, overlay UX, and performance.<br>
- บางทีก็อัดได้ บางทีก็ไม่… แล้วแต่ดวง 555555665

---

## Development

| Branch | Purpose |
|--------|---------|
| **`Stable`** | Default branch — last known-good build |
| **`Engine-Rebuild-Stabilization`** | Active development — modular capture engine rewrite, sync/audio stabilization |

Engine docs live in [`docs/`](docs/) (start at `docs/PHASE_PLAN.md`), the module map in [`PROJECT-STRUCTURE.md`](PROJECT-STRUCTURE.md), and build/diagnostic entry points in [`scripts/`](scripts/).

---

## ![License: MIT](https://img.shields.io/badge/License-MIT-green.svg) 

| File | Description | Click to View |
|---------|-------------|------------------|
| **[LICENSE](LICENSE)** | MIT License (Copyright 2023-2024) | [Open LICENSE](LICENSE) |
| **[LICENSE.NOTICE](LICENSE.NOTICE)** | Third-Party Components Attribution | [Open LICENSE.NOTICE](LICENSE.NOTICE) |

DISCLAIMER
> [!CAUTION]
> **Trademark Notice:** This is an **independent third-party application** and is **NOT affiliated with, endorsed by, sponsored, or approved by NVIDIA Corporation**.
> 
> "**NVIDIA**", "**GeForce**", and "**ShadowPlay**" are **trademarks or registered trademarks of NVIDIA Corporation** in the United States and/or other countries.
>
> This software uses official **NVIDIA NVENC/intel/Amd encoder technology through FFmpeg** and **Microsoft Windows.Graphics.Capture API** in compliance with their respective license agreements.

---

## 📦 Third-Party Components

| Component | License | Author | Source |
|-----------|---------|--------|--------|
| [NAudio](https://github.com/naudio/NAudio) | MIT | Mark Heath | NAudio.Core.dll, NAudio.Wasapi.dll |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | MIT | James Newton-King | Newtonsoft.Json.dll |
| [libmp3lame](https://lame.sourceforge.io/) | LGPL-2.0 | The LAME Project | libmp3lame.32.dll, libmp3lame.64.dll |
| [FFmpeg](https://ffmpeg.org/) | LGPL/GPL | FFmpeg Developers | Encoding Pipeline |
| [.NET 8 Runtime](https://dotnet.microsoft.com/) | MIT | Microsoft | Microsoft.Windows.SDK.NET.dll |
| [Windows SDK.NET](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/) | MIT | Microsoft | WinRT.Runtime.dll |

See full attribution: **[LICENSE.NOTICE](LICENSE.NOTICE)** ← Click!

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

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
| **Website** | [ScotcsDuluka](https://scotcsduluka.github.io/ScotcsDuluka/) |
| **Releases** | [GitHub Releases](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases) |
| **Report Bug** | [GitHub Issues](https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/issues) |
| **Creator** | [ScotcsDuluka](https://github.com/ScotcsDuluka) |

---

*Crafted with passion, engineered for performance, and continuously evolving.* ❤️
