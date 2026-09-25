# HANDOFF — GFE ระบบ WinForm ย้ายไปเครื่อง Intel (HUAWEI-PC)

> เขียนหลัง session ย้ายระบบ gfe-rebuild ทั้งหมด (2026-09-25)
> อ่านก่อนเริ่มเซสชันใหม่บนเครื่อง Intel — เอกสารนี้คือ "แชท" ที่ย้ายมา
> Session ต้นทาง (1080 Ti = desktop-duluka): ZCode `sess_086dcfaa-9769-4fbd-abb8-27fdd14690cd`

---

## 1) สถานะปลายทาง (อ่านก่อน)

- **branch หลักของ repo ตอนนี้ = `gfe-rebuild`** (default branch บน GitHub แล้ว, push ถึง `a15efa27f4`)
- ระบบ WinForm ใช้งานได้จริงบน 1080 Ti (desktop-duluka): build เขียว, test เขียว
- ต้นไม้หลัก `C:\My Project\NVIDIA-Shadowplay` บนเครื่อง 1080 Ti ตอนนี้ checkout ด้วย `gfe-rebuild` แล้ว (worktree `-gfe` ถูกถอดแล้ว)

## 2) สถาปัตยกรรมระบบ (5 ตระกูล + CEF)

| Component | Exe | บทบาท |
|---|---|---|
| NvContainer.exe | supervisor | เปิดตลอด — spawn/respawn nvsphelper64 (อ่าน config จาก `NvContainer\Config\NvContainer.json`) |
| nvsphelper64.exe | engine | จับภาพ (NVENC native + DdagrabBackend) — ต้องเปิดตลอด |
| NVIDIA Backend.exe | TCP hub :5001 | เปิดพร้อม Launcher, keep-alive overlay stack ตาม `NvConfig\config.json` |
| NVIDIA ShadowPlay.exe | WinForm overlay | หน้า settings ทั้งหมด (ชื่อเดิม NVIDIA Share.exe) |
| NVIDIA Notifier.exe | tray | — |
| NVIDIA Share.exe (CEF) | `NvOverlay\Cef\` | controller overlay — probe backend :59001 อัตโนมัติ |
| NVIDIA Web Helper.exe | `NvBackend\` | host node backend :59001 (osc same-origin) — เฉพาะโหมด ENGINE OVERLAY |

Launcher: toggle `Use_Overlay` (WinForm family) + `ENGINE OVERLAY` (CEF lane) — default = WinForm family

## 3) ขั้นตอนตั้งเครื่อง Intel (HUAWEI-PC)

### 3.1 Prerequisites
- git, .NET 10 SDK, Node.js (จะใช้ system node ก็ได้ — `NodeRuntime.vb` หาตามลำดับ: env `NVBACKEND_NODE_EXE` → `NvBackend\node.exe` → GFE → `%ProgramFiles%\nodejs`)
- MSBuild (VS 2022 BuildTools/Community — build-dev.ps1 หาที่ `C:\Visual Studio\MSBuild` ก่อน แล้วค่อย Program Files)
- CEF SDK: ต้องมี `C:\My Project\cef-sdk\cef73` (458MB) — **ต้องโอนจาก 1080 Ti** (ดู §5)

### 3.2 โคลน + payload
```cmd
git clone https://github.com/ScotcsDuluka/NVIDIA-Shadowplay "C:\My Project\NVIDIA-Shadowplay"
cd "C:\My Project\NVIDIA-Shadowplay"   (branch gfe-rebuild มาให้เอง — default)
```
โอน payload จาก 1080 Ti (ดู §5) แล้ววาง:
- `gfe-preserved\FFmpeg\`  →  `Project\ShadowPlay\FFmpeg\`
- `cef-sdk\cef73\`         →  `C:\My Project\cef-sdk\cef73\`
- (อยากได้ evidence: `gfe-preserved\node-runtime\` = NvNode ของ GFE จริง 44MB)

### 3.3 Build + Run
```powershell
cd "C:\My Project\NVIDIA-Shadowplay"
powershell -ExecutionPolicy Bypass -File Scripts\build-dev.ps1 -Clean
# output: Build\NVIDIA ShadowPlay\  (owner dedupe ทำงานเอง)
Start-Process '.\Build\NVIDIA ShadowPlay\Launcher.exe'
```
Launcher จะเริ่ม NvContainer + nvsphelper64 + NVIDIA Backend + WinForm family เอง
toggle ENGINE OVERLAY = เพิ่ม Web Helper (node :59001) + CEF NVIDIA Share.exe

### 3.4 ทดสอบมาตรฐาน
```powershell
cd Project\ShadowPlay\NvCapture
$env:RRT_FFMPEG = "C:\My Project\NVIDIA-Shadowplay\Project\ShadowPlay\FFmpeg"
# รัน exe ใน CaptureEngine.*Tests\bin\Release\...\ ทีละตัว
# ค่าที่ควรได้: Config 95/95, Frame 8/8, Recording 43 pass, Video 66/66 (blend off)
```

## 4) สิ่งที่แก้ไว้ใน session นี้ (สรุป)

- sln orphan GUID (MSB5023) + เพิ่ม Web Helper เข้า sln; NVIDIA API → `NvBackend\NVIDIA Backend`
- ชื่อ family: NVIDIA ShadowPlay.exe (เดิม NVIDIA Share.exe), NVIDIA Backend.exe (เดิม NvBackend), nvsphelper64.exe คงเดิม
- build-dev.ps1: owner dedupe (−74 dll ซ้ำ), Languages/FFmpeg staging ถูกที่, hub staging คืน
- CEF host: single-instance guard (mutex ต่อ exe path), primary-screen topmost borderless, ดำไม่แวบ, auto-probe :59001, osc bundle จริง (GFE 3.28) vendored ที่ `NvOverlay\Cef\osc\`
- console ซ่อนถาวร: NvContainer/Web Helper เป็น WinExe; `"showConsole": true` ใน json = AllocConsole
- node.exe ซ่อน (BackendProcess `CreateNoWindow=True`, output เข้า Web Helper log `[NODE]`)
- DdagrabBackend: cursor composition (GetFramePointerShape + blend 3 แบบ shape) + `CaptureCursor` switch

## 5) Payload ที่ต้องโอนจาก 1080 Ti (~740MB)

| จาก (1080 Ti) | ขนาด | ไป (Intel) |
|---|---|---|
| `C:\My Project\cef-sdk\cef73\` | 458M | `C:\My Project\cef-sdk\cef73\` |
| `C:\My Project\gfe-preserved\FFmpeg\` | 239M | `Project\ShadowPlay\FFmpeg\` |
| `C:\My Project\gfe-preserved\node-runtime\` | 44M | (evidence — เก็บไว้ข้าง repo) |

ทางโอน: OneDrive/Google Drive (เครื่องลงทั้งคู่) หรือ Tailscale (เปิด tailscale บน HUAWEI-PC แล้ว `\\HUAWEI-PC\...` หรือ tailscale cp)

## 6) สิ่งค้าง / กับดักที่รู้แล้ว

- **Capture Cursor default OFF** — เปิดแล้ว `DXGI_ERROR_DEVICE_REMOVED` ใน stress tests ของ 1080 Ti (Video.Tests 7 fail เมื่อ ON, 66/66 เมื่อ OFF) → ต้องผ่าน D3D11 debug layer ก่อน โค้ดอยู่ที่ `DdagrabBackend.BlendCursorOnto`
- **nvspcap.dll ยังเป็น placeholder** (PENDING artifact เดียวของ build)
- **LM-SEP known-fail** (FFmpeg trac #1663 — named-pipe input ที่สามหลุดใน muxer)
- **`Duluka\.server-secret` ถูกลบไปพร้อมเก็บกวาด** — generate ใหม่ใน GitHub OAuth App หรือใช้ env `DULUKA_GitHub__ClientSecret`
- **อย่าแก้ osc app.js ตรง ๆ** (1.6MB minified — พังทั้ง overlay) ใช้ mods/ system
- **งานที่ commit แล้วบน remote**: `dbfa6d9e9a` (taskboard 2+3) ถูก merge แล้วใน `a15efa27f4`
- Stable ยังมี 5 commit เก่า (upload/zip artifacts) ที่ไม่ได้รวม — เก่าเกินจำเป็น

## 7) ไฟล์/พาธสำคัญ

| Path | คือ |
|---|---|
| `Scripts\build-dev.ps1` | canonical build (อย่าแก้นอกกฎ owner dedupe) |
| `Build\Build-Config\dev-layout.json` | layout authority (rootDirectories + owners) |
| `Project\NvOverlay\Cef\osc\` | osc bundle จริง (GFE 3.28) — vendor ตรงนี้ |
| `NvContainer\Config\NvContainer.json` | worker config (อ่านจากโฟลเดอร์ **Config\** เท่านั้น!) |
| `NvConfig\config.json` | toggle กลาง: Overlay.UseOverlayEnabled / EngineOverlayMode |
| `Config\engine.json` | engine knobs: CaptureMethod, **CaptureCursor** |
| `Common\AppLayout.vb` | resolver — probe folders คือสัญญาของ owner tree |
| `docs\GFE-PROCESS-MAP.md` | สัญญา process 1..4 + ports (59001/59004) |
| `docs\osc\18-session-handoff.md` | handoff รอบก่อน (บริบท Intel เดิม) |

## 8) กฎเหล็ก (ห้ามลืม)

1. อย่าแก้ `osc\app.js` ตรง ๆ
2. อย่า kill nvcontainer ของ GFE จริง (pid ที่ path = Program Files)
3. อย่าลบ `gfe-preserved\` บน 1080 Ti (FFmpeg/RE evidence ไม่อยู่ใน git)
4. อย่าตั้งชื่อ form class ชนกับ root namespace (Launcher.Launcher = พังทั้ง Designer)
5. หลังแก้ engine ให้รัน `Scripts\build-dev.ps1 -Clean` เสมอ (stale DLL = #1 false bug)
