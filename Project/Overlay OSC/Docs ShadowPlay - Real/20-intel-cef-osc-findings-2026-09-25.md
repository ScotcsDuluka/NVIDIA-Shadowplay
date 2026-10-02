# Intel machine findings — official GFE payload + CEF runtime + hotkey plugin (2026-09-25)

> เขียนจาก session เครื่อง Intel (HUAWEI-PC) หลังแกะ payload จาก installer แท้
> GFE 3.28.0.412 ที่โหลดจาก NVIDIA CDN โดยตรง
> ประกอบ docs/osc/16-real-host-portable.md (รอบ port ก่อน) และ
> HANDOFF-GFE-INTEL-2026-09-25.md

## 1) payload จาก installer แท้ (แตกด้วย 7z — ไม่ต้อง install)

```
us.download.nvidia.com/GFE/GFEClient/3.28.0.412/GeForce_Experience_v3.28.0.412.exe
  (125.6MB — bootstrap "NVIDIA Package Launcher"; 7z x ได้ตรง ๆ)
  → 73 folders / 1060 files / 638MB:
    GFExperience\        → NVIDIA Share.exe (3,269KB) + libcef.dll (NVIDIA build!)
                           + osc\ + www\ + NVIDIA Share.json + data\
    nodejs\              → NvNode backend ตัวจริง: index.js (JS!) + Downloader.node ฯลฯ
                           (package "NvNodejs" แยกต่างหากตอน install → กลายเป็น NvNode\)
    ShadowPlay\          → engine: nvsphelper64.exe + nvsphelperplugin64.dll
                           + capcore64.dll + Plugins\
    NvContainer\         → supervisor; NvBackend\ → NvTmRep/NvSHIM
```

เครื่อง Intel เก็บ extract ไว้ที่ `C:\My Project\gfe-3.28.0.412-extract\` (ไม่ใน git)

## 2) NVIDIA Share.json ตัวจริง = รูปแบบ `switches[]` (ต่างจากของเรา)

```json
{"switches":[
  "nv-osc=true",
  "nv-gpu-accel=true",
  "nv-url-relative=osc/index.html",
  "nv-node-app=NvNode\\nvnodejslauncher.exe",   ← Share.exe เป็นคน spawn backend เอง
  "nv-node-data=NvNode\\nodejs.json",
  "nv-plugin-folder-relative=./cef/share;./cef/common",
  "nv-plugin-dependencies-relative=./dependencies"
]}
```

ต่างจากเลย์เอาต์ที่ port มา (object nv-* / url-relative=Resources/osc/index.html /
nv-node-app ว่าง) — ของเราออกแบบให้ Web Helper.exe เป็นคน spawn node แทน ซึ่ง
concept เดียวกัน แค่แบ่งหน้าที่คนละจุด

## 3) ⭐ จอดำของ CEF host บน Intel = ตัว libcef ไม่ใช่โค้ดเรา

| libcef.dll | version | ผลบน Intel UHD |
|---|---|---|
| Spotify stock `73.1.13+g6e3c989` | 105MB | หน้าต่างเปิด โหลดหน้าได้ title ขึ้น แต่**จอดำ** และรอบแรก UI thread ค้าง ("Not responding") — แม้แต่ตอน `nv-gpu-accel=false` |
| **NVIDIA custom build (จาก GFE จริง)** `73.0.0-HEAD.1933+gee4b49f` | 106MB | หน้าต่างขึ้น โหลด osc จาก backend 59001 ได้ **Responding=True ทุก process** |

- Chromium base เดียวกัน (73.0.3683.75) — NVIDIA เขาคอมไฟล์ CEF เอง
  (NVIDIA patch layer มี workaround ฝั่ง GPU/driver ที่ stock ไม่มี)
- **วิธีแก้ที่ทำแล้วบน Intel**: ใช้ Spotify SDK **`73.1.12+gee4b49f`** (CEF revision
  เดียวกับ hash ของ NVIDIA → C API hash ตรงกัน) เป็น headers/wrapper สำหรับ compile
  host แล้ว**ทับ runtime** ใน `cef73\Release\` + `cef73\Resources\` ด้วยไฟล์จริงจาก
  `Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\`
  (libcef.dll, chrome_elf.dll, libEGL/libGLESv2, d3dcompiler 43+47, icudtl.dat,
  *.pak, natives/snapshot/v8 blobs, locales\)
- build-dev.ps1 stage จาก `cef73\Release` → การแก้นี้**ติดถาวรต่อการ build**
  (cef73 เป็น machine-local payload นอก git เสมอ)
- หมายเหตุ: หลังสลับ runtime หน้าต่าง host เรา "Responding" และโหลดหน้าได้
  แต่ผิวยังดำ — ต้องตามต่อที่ window creation ของ share_win.cpp (เทียบกับ
  วิธีเปิดหน้าต่างของ Share.exe จริง) — libcef ไม่ใช่ตัวการที่เหลือแล้ว

## 4) ⭐ กลไก Alt+Z ตัวจริง (แกะจาก nvsphelperplugin64.dll)

PDB: `...\shadowplay2\hotkey\plugin\win7_amd64_release\nvsphelperplugin64.pdb`
exports เพียง 2: `NvPluginGetInfo`, `NvGetPluginState` ("Hotkey Plugin",
host = NVIDIA Share.exe / NVIDIA Web Helper.exe)

```
CHotKeySettings อ่าน registry
  HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience\ShadowPlay (Cfg2/Cfg3):
    GFEOverlayHKeyV2            ← Alt+Z
    FPSOverlayHKey / CustomOverlayHKey[A-C] / IRToggleHKey / MicToggleHKey /
    FBChatToggleHKey / PMOCOverlay* (+ ต่อท้าย %d แบบ indexed)
→ RegisterHotKey + GetAsyncKeyState + RawInput (PTT)
→ WM_HOTKEY → CShadowPlayHelper::NotifyHotkeyPress
→ ส่ง 3 ช่องทาง: MessageBus (NvContainer MessageBus.dll) + named pipes +
  MMF (OVLY-COMMON, redirect สำหรับ UWP)
→ ปลายทาง: "NVIDIA Share.exe" / "NVIDIA Web Helper.exe"
```

- ด่านกันบนเครื่องไม่มี NVIDIA: `IsShadowPlayEnabled` +
  `HKLM\...\Global\ShadowPlay\NVSPCAPS` + ต้องโหลด capture stack ของ driver
  → บน Intel plugin นี้ไม่ทำงาน → ตัว port เสียบ `hotkey-listener.ps1`
  (RegisterHotKey + POST `/ShadowPlay/v.1.0/Hotkey/Toggle` ไปที่ backend :59001)
  เลียนแทนเฉพาะชั้น toggle — ซึ่ง backend ทั้งตัว port และ NvBackend เรา
  (routes/shadowplay.js) รับอยู่แล้ว (ยืนยัน 200 กับ Web Helper จริงด้วย)
- **สาย Alt+Z บนเครื่องนี้**: hotkey-listener.ps1 (HKCU Run `NvPortableHotkey`)
  → POST :59001 → backend ที่ถือพอร์ตตอนนั้นตัดสินว่า osc ตัวไหนโดนยิง
  — ระหว่าง lane เราถือ 59001 ปุ่มจะยิงเข้า backend เรา (osc เราจอดำ เลยดูเหมือน
  "กดแล้วเงียบ") — กลับมา backend จริงแล้วทุกอย่างปกติ

## 5) สถานะเครื่อง Intel หลังวันนี้

- GFE ที่ port มา (portable install ของ GFE 3.28 ทั้งชุด) กลับมารันครบ:
  service NvContainerLocalSystem + nvcontainer ×2 + NVIDIA Share.exe (osc) +
  Web Helper (59001) — Alt+Z ผ่าน hotkey-listener + backend จริง
- Node.js v24.19.0 LTS ติดตั้งแล้ว (probe path `%ProgramFiles%\nodejs` ของ
  NodeRuntime.vb) — lane เรา build เขียวด้วย CEF runtime ของ NVIDIA พร้อมรอ
  debug ต่อเรื่อง render
- PENDING คงเหลือ: FFmpeg payload, nvspcap.dll, Project\Data (โครงสร้างกำลัง
  ถูกจัดใหม่จาก 1080 Ti)

## 6) งานต่อที่ชัดขึ้นจากการแกะครั้งนี้

1. share_win.cpp — เทียบ window creation กับ Share.exe จริง (จอดำที่เหลือ)
2. diff `nodejs\index.js` (backend จริง 638MB extract) กับ `routes/*.js` ของเรา
   — ชุด endpoint/field ที่หลุดไป
3. hotkey config: osc settings page ควรอ่าน/เขียน key เดียวกับ
   `GFEOverlayHKeyV2` (registry Cfg2/Cfg3) เพื่อ compat กับ GFE จริง
4. Share.json ของเราควรรองรับรูปแบบ `switches[]` แบบ official ด้วย (drop-in)
