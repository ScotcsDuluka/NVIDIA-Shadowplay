# GENUINE GFE 3.28 STACK — ติดตั้งและรันสำเร็จบนเครื่องหลัก (2026-09-26)

> สถานะ: **genuine stack เต็มตัวรันแล้ว** — host แท้ + node v11 แท้ + native addon แท้
> + container service — ข้อมูลจริงทุกชั้น (HardwareInformation ตอบของจริงจากเครื่อง)
> Alt+Z = overlay แท้ของ NVIDIA

## 1. สถาปัตยกรรมที่รันอยู่

```
NVIDIA Share.exe (แท้ — C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\)
   ├── spawn โดย host เอง: nvnodejslauncher.exe
   │     └── NVIDIA Web Helper.exe (node v11.13 แท้ 29MB — ชื่อปลอมเป็น helper)
   │           └── genuine backend: index.js + 17 JS modules (11,430 บรรทัด)
   │                 + native addon แท้ 11 ตัว (NvUtil/NvBackendAPINode/...)
   │                 + node_modules (express, socket.io v2.5.1, ...)
   │                 └── :59001 — REAL data (HardwareInformation ตอบเมนบอร์ด/BIOS/RAM)
   ├── osc แท้ (โหลด file:// จาก Resources/osc ของตัวเอง)
   ├── CEF plugins (cef/share + cef/common + dependencies)
   └── NvContainerLocalSystem service (container แท้ = สมองกลาง MessageBus/HotkeyPlugin)
```

## 2. สิ่งที่ติดตั้ง (install-host.ps1 ของ Intel + ขั้นที่เพิ่มเอง)

| ขั้น | สิ่งที่ติดตั้ง | ตำแหน่ง |
|---|---|---|
| 2 | family แท้: GFExperience/ShadowPlay/NvContainer/NvNode/NvBackend | C:\Program Files\NVIDIA Corporation\ + (x86) |
| 3 | hook dll: nvspcap64.dll (System32), nvspcap.dll (SysWOW64) | ระวังล็อก — ข้ามได้ถ้ามีอยู่ |
| 4 | ProgramData seed: piplConfig.json | C:\ProgramData\NVIDIA Corporation\NvNode\ |
| 5 | registry: NvNode {port=59001, disableSecurity=1} + GFEx identity | HKLM\SOFTWARE\NVIDIA Corporation\Global\ (+WOW64) |
| 6 | **registry ModuleMap** (MessageBus/protobuf/Poco dll paths) — **กุญแจของ 126!** | HKLM\...\NvContainer\ModuleMap |
| 7 | **NvContainerLocalSystem service** (container แท้) + DACL | service ของระบบ |
| 8 | SPUser watchdog (capture plugin home) | HKLM\...\Watchdog\SPUserX64 |
| 10 | เริ่ม NVIDIA Share.exe (แท้) | host spawn node เอง |

## 3. Share.json = สวิตช์โหมดทั้งหมด (อยู่ beside Share.exe)

```json
{ "switches": [
  "nv-osc=true",                                  // true=overlay โปร่งใส, false=หน้าต่างปกติ (debug ง่าย!)
  "nv-gpu-accel=true",                            // GPU pipeline (dx9 offscreen ของ NVIDIA เอง)
  "nv-url-relative=osc/index.html",               // หน้า = file:// จากโฟลเดอร์ตัวเอง (ไม่ใช่ http!)
  "nv-node-app=NvNode\\nvnodejslauncher.exe",     // host spawn node เองผ่านตัวนี้
  "nv-node-data=NvNode\\nodejs.json",
  "nv-plugin-folder-relative=./cef/share;./cef/common",
  "nv-plugin-dependencies-relative=./dependencies",
  "nv-remote-debugging-port=9222"                 // (เพิ่มเอง — CDP ตรวจหน้าได้)
] }
```

## 4. Flow ของระบบแท้ (สิ่งที่แกะได้จริง)

1. Share.exe boot → อ่าน Share.json → spawn `NvNode\nvnodejslauncher.exe`
2. launcher spawn `NVIDIA Web Helper.exe` (= node v11.13 x86 — node binary แท้ปลอมชื่อ) รัน index.js
3. node: WaitSystemService(NvContainerLocalSystem) → container ต้องรัน (service แท้)
4. node โหลด native addons ผ่าน **nvLoadLibraryFromTrustedLocation** →
   **ต้องมี registry ModuleMap** (ชี้ MessageBus/protobuf/Poco ที่ NvContainer\) — ไม่มี = 126
5. node: ConfirmInitialization({port, secret}) → กลับขึ้น launcher → host
6. host: ส่ง {port, secret} ให้หน้า (QUERY_WIN_NODE_INFO) → หน้า connect socket + REST
7. Alt+Z: native hotkey (HotkeyPlugin ใน container ผ่าน MessageBus) →
   node emit `/ShadowPlay/v.1.0/WindowState {windowMsg:'overlayToggle'}` → หน้า toggle เอง

## 5. บทเรียนสำคัญ (ตัวที่ทำให้เสียเวลา)

| ปม | คำตอบ |
|---|---|
| node แท้ = อะไร | `NVIDIA Web Helper.exe` 29MB ใน NvNode = node v11.13 x86 เปลี่ยนชื่อ |
| trusted-location | ฝังใน node binary — addon ต้องอยู่ dir เดียวกับ node (junction ผ่านไม่ได้ — GetFinalPathName ตีกลับ) |
| 126 | **ModuleMap registry ไม่มี** — addon โหลด MessageBus ผ่าน map; ใส่ map = ผ่าน |
| node v11 lock | index.js เช็ค `process.version !== 'v11.13.0'` — node แท้จึงต้อง v11.13 (binary ปลอมชื่อใน NvNode ✓) |
| host spawn node ที่ไหน | **hardcode Program Files (x86)\NVIDIA Corporation\NvNode** (ไม่อ่าน relative จาก Share.json) |
| container คืออะไร | NvContainerLocalSystem service = สมองกลาง (MessageBus + HotkeyPlugin + ShadowPlay service) |
| osc โหลดจากไหน | file:// จากโฟลเดอร์ Share.exe (nv-url-relative) — ไม่ใช่ HTTP |

## 6. API surface ที่หน้าใช้จริง (boot + เปิด overlay)

```
GET  /Account/v.1.0/PrivacySettings, /Account/v.1.0/UserToken
GET  /HardwareInformation/v.0.1          ← REAL data (เมนบอร์ด/BIOS/RAM)
GET  /PiplConfig/v.1.0/data, /Settings/v.1.0/Language, /beta
GET  /ShadowPlay/v.1.0/Capture/State, /Capture/ProcessInfo/:pid
GET  /ShadowPlay/v.1.0/DesktopCapture/Support/Reason   ← {support,unsupportReason} ต้องมี!
GET  /ShadowPlay/v.1.0/Launch, /beta, /gfeupdate/...
POST /ShadowPlay/v.1.0/Launch, /Hotkey/DynamicToggle, /OSC/MainView {fetchPartial}
POST /Feedback/v.0.1
SOCKET /ShadowPlay/v.1.0/WindowState {windowMsg:'overlayToggle'|'dismiss'|...}
```

## 7. สถานะ lane ของเรา

- **CEF Overlay lane ของเรา (C++ OSR host) = ตัดออก** — genuine แทน (host แท้ + node แท้)
- **Duluka layer ต่อยอด**: NvNode แท้ mount DulukaAPI.js ได้ (NodeRuntime รองรับ) —
  ข้อมูล/บริการของเราเสริมผ่าน genuine routes — ไม่แทน NVIDIA
- **backend rewrite ของเรา (Backend\)** = เก็บใน repo เป็น reference — ไม่ใช่ runtime แล้ว
- **node แท้บน node v24 + shims** = พิสูจน์แล้วบูตได้ (Project/NvNode + shims) — เส้นทางสำรอง
  ถ้าต้องการ custom โดยไม่แตะ node v11 แท้
