# GFE 3.28 — ความเข้าใจสถาปัตยกรรมฉบับสังเคราะห์ + gap map กับ gfe-rebuild

> สังเคราะห์จากการแกะ payload แท้ (docs/osc/20-23) — เอกสารนี้คือ mental model
> เดียวที่ใช้อ้างได้ ไม่ใช่ string dump — 2026-09-25, session เครื่อง Intel

## 1) Process tree ที่ถูกต้อง (ยืนยันจาก .nvi + การทำงานจริง)

```
NvContainerLocalSystem (service, nvcontainer.exe -ert)
  │   plugins\<variant>\ ← registry Global\<Service>\PluginFolderPath
  │   (Watchdog = crash-rate policy, ServicePlugin = spawn ใน user session)
  └─ worker: nvsphelper64.exe (ShadowPlay engine)
       ├─ plugins: nvsphelperplugin64(hotkey) + _nvspcaps64(SP server) +
       │            _nvspserviceplugin64
       └─ capcore64 → nvfp64 (NvFBC|DDA) → encoder(NVENC) → nvmf64 (MP4 mux)
            → NvRemux64 (trim) ; audio = nvaudcap64v (driver)

NVIDIA Share.exe == GeForce Experience.exe == Notification.exe  (ไบนารีเดียว)
  ├─ อ่าน <ชื่อตัวเอง>.json switches[] → spawn NvNode\nvnodejslauncher.exe
  └─ เปิดหน้าต่าง osc (borderless topmost) → โหลด osc จาก same-origin

NvNode = nodejs\ → install ลง Program Files (x86)\...\NvNode  (nodejs.nvi)
  ├─ NVIDIA Web Helper.exe = node เต็มตัว รัน NvNode\index.js
  │    express + socket.io :59001 (security cookie ตาม registry)
  │    mount: PiplConfig → BackendAPI → Account → DriverInstall → downloader
  │             → ABHub → GameStream → Gallery → Camera → ShadowPlayAPI → SDK
  └─ NvShadowPlayAPI.js (70+ endpoint) ↕ NvShadowPlayAPINode.node
        ↕ native → _nvspcaps64 server (session API, ShowOverlay, Highlights)

Alt+Z: nvsphelperplugin64 (RegisterHotKey, registry Cfg2/Cfg3: GFEOverlayHKeyV2)
  → MessageBus + pipes + MMF → Share.exe/Web Helper
  (Intel: hotkey-listener.ps1 → POST /ShadowPlay/v.1.0/Hotkey/Toggle แทนชั้นเดียว)
```

## 2) 5 ระนาบ IPC (สำคัญที่สุดของทั้งระบบ)

| # | ระนาบ | พาหะ | ใช้ทำ |
|---|---|---|---|
| 1 | REST + WebSocket | HTTP 127.0.0.1:59001 + socket.io | osc page ↔ backend (70+ endpoint, jsEvents RPC) |
| 2 | MessageBus | registry `NvContainer\MessageBus\MessageBusPort` + protobuf + pipes ต่อ session (`MessageBus_%u_0x%LX`) | control plane กลาง (hotkey, state, container↔plugins) |
| 3 | MMF family | shared memory: state slots หมุน id, OVLY-COMMON, Pixel MMF (overlay in-game), ScreenShot MMF, viewer-count | ข้อมูลเรียลไทม์หนัก ๆ |
| 4 | http_over_pipe | `\\.\pipe\` + `http_over_pipe://` (NvBackendAPI64) | ทาง loopback ที่สอง (ไม่เปิดพอร์ต TCP) |
| 5 | Window messages | `QUERY_WIN_OPEN_OSC / CLOSE_OSC / SET_PAINTING / SET_DISPLAY_RECTS / REGISTER_CLOSE_EVENT / DROP_URL` | native ↔ หน้าต่าง CEF host |

## 3) Config hierarchy (7 ชั้น ใครชนะใคร)

1. registry `Global\NvNode` (port=59001, disableSecurity=1) — พอร์ต/ความปลอดภัย
2. registry `Global\GFExperience\ShadowPlay` Cfg2/Cfg3 — hotkey + SP settings
3. registry `Global\ShadowPlay\NVSPCAPS` (`IsShadowPlayEnabled`) — ประตู engine
4. `<exe>.json` switches — ต่อ CEF host (Share/GFE/Notification)
5. `NvNode\config.json` — external services (ota / gxtarget / jarvis)
6. `NvBackend\config.json` + FeatureWhitelist.json — telemetry + feature รายเกม (server-driven)
7. gxtarget cloud variables — feature flag รายผู้ใช้ แบบ remote

## 4) ⭐ Gap map: GFE จริง vs gfe-rebuild (สถานะ 2026-09-25)

| ส่วน | GFE จริง | ของเรา | สถานะ |
|---|---|---|---|
| CEF host | Share.exe (3.2MB, switches[], plugins ./cef/share+common) | NvShim/`NVIDIA Share.exe` ของเรา (990KB, json object) | ✅ รันได้ — เหลือแก้ render จอดำ (share_win.cpp) + พิจารณา one-binary-many-configs |
| CEF runtime | libcef NVIDIA build | **libcef NVIDIA แล้ว** (สลับเข้า cef73 แบบถาวร) | ✅ วิธีถูกแล้ว |
| Node backend | NvNode (index.js + 70+ endpoint + socket.io + security cookie) | NvBackend (routes/*.js — ชุดเดียวกันเชิง concept) | 🟡 ต้อง diff ราย endpoint + เปิด socket.io channel |
| ShadowPlay REST | NvShadowPlayAPI.js (InstantReplay/Record/Broadcast/Hotkey/:hk/Webcam/Mic/CustomOverlay/Highlights…) | routes/shadowplay.js | 🟡 parity รายเส้น — ใช้ §API list ใน doc 22 เป็น checklist |
| Session API→capture | CShadowPlayApi → _nvspcaps64 server → capcore (NvFBC/DDA) → NVENC → nvmf/remux | CaptureEngine (session + Ddagrab/FFmpeg backend + ffmpeg mux) | ✅ สถาปัตยกรรมเดียวกัน, 🟡 hardware-gate ยังเข้มกว่าจำเป็น (DDA tests env-skip บน Intel — เลียนแบบ MockServer mindset) |
| Hook เกม | nvspcap*.dll (DDI shim + in-game overlay ผ่าน Pixel MMF) | placeholder (PENDING) | ⛔ ยังไม่ทำ — ตัดสินใจได้ว่าจะทำ in-game จริง หรือใช้ window แยกต่อ |
| Instant Replay trim | NvRemux64::Trim (native MF) | ffmpeg (LM-SEP known-fail) | 🟡 ทางเลือก: MF mux เหมือนเขา หรือแก้ ffmpeg pipe |
| Broadcast | NvRtmpStreamer64 (RTMP/Twitch) | ยังไม่มี | ⛔ ไม่เร่ง |
| Screenshot | nvspscreenshot64 (MMF + HDR JXR) + GfeXCode (CUDA/NGX) | ยังไม่มี | ⛔ ทาง WIC/JXR ของเราพอ |
| Hotkey | native plugin (MessageBus/MMF) | hotkey-listener.ps1 → HTTP toggle | ✅ บน Intel ใช้ได้จริง — ระยะยาวควรย้ายเข้า nvsphelper64 เรา |
| Config hierarchy | 7 ชั้น (§3) | NvConfig/config.json + providers.json + engine.json | 🟡 เพิ่ม hotkey registry key ให้ตรง GFE ก่อน |
| Supervisor | NvContainer (plugins\<variant>, watchdog crash policy) | NvContainer.exe เรา (worker contract เดียวกัน) | ✅ |

## 5) ลำดับงานถัดไปที่ "ความเข้าใจชุดนี้" ชี้ทาง

1. **osc page → backend parity** (ผลลัพธ์เร็วสุด): diff ราย endpoint ตาม
   NvShadowPlayAPI.js (doc 22 §2) — เพราะ osc bundle ของเราคือตัวจริง มันจะเรียก
   endpoint ตรงตามนั้นทุกเส้น
2. **share_win.cpp render จอดำ** — libcef แก้แล้วเหลือ window creation;
   ใช้ `nv-remote-debugging-port` (ทั้ง Share.exe จริงและของเรารองรับ switch
   เดียวกัน!) เปิด devtools ดูว่าหน้า paint หรือไม่
3. **hotkey registry**: settings page เราอ่าน/เขียน `GFEOverlayHKeyV2` ฯลฯ ให้ตรง
4. **MockServer mindset**: hardware-gate ของ DDA tests ทำแบบ `_nvspcaps64` —
   session API ต้องรันได้แม้ไม่มี driver (Intel-ready)
5. nvspcap hook: ตัดสินใจภายหลัง (in-game vs window) — ข้อมูลครบแล้วทั้งสองทาง
