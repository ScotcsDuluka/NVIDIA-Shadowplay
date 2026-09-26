# PATH-MANIFEST — แผนที่ติดตั้ง OSC Stack ฉบับสมบูรณ์ (snapshot 2026-09-27 00:20-00:24)

วัตถุประสงค์: ก๊อปของจริงออกก่อนถอด GFE → ประกอบกลับให้ NVIDIA Share.exe แท้ทำงานสมบูรณ์
ทุกค่าในเอกสารนี้มาจากของจริงบนเครื่อง (robocopy log + reg export + sc qc) — ไม่มีการเดา

---

## 1) Payload Inventory (Project\OscProvision\Payload\)

| Payload folder | Source (ต้นทางจริง) | ขนาด | หน้าที่ |
|---|---|---|---|
| `NvContainer\` | `C:\Program Files\NVIDIA Corporation\NvContainer\` | 46 MB | โฮสต์ปลั๊กอิน: broker + spawn helper + Watchdog |
| `NvContainerX86Dlls\` | `C:\Program Files (x86)\NVIDIA Corporation\NvContainer\` | 14 MB | DLL x86 สำหรับ client 32-bit |
| `NvContainerX86Exe\` | `C:\Program Files (x86)\NVIDIA Corporation\NvContainer-x86\` | 20 MB | nvcontainer x86 + NvContainerInternal.exe (ตัวไม่เช็ค signature) |
| `ShadowPlay\` | `C:\Program Files\NVIDIA Corporation\ShadowPlay\` | 22 MB | engine: capcore64, nvfp64, nvmf64, NvRemux64, nvaudcap, screenshot, nvspapi64/x64, ipccommon64, nvsphelper64.exe + plugin, NVSPCAPS\_nvspcaps64.dll, Plugins\LocalSystem\_nvspserviceplugin64.dll |
| `NvNode\` | `C:\Program Files (x86)\NVIDIA Corporation\NvNode\` | 64 MB | node v11.13.0 (NVIDIA Web Helper.exe) + index.js + addons + node_modules + nvnodejslauncher.exe |
| `GfeBundle\` | `C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\` | 224 MB | เต็มโฟลเดอร์ (กันขาด): Share.exe + Share.json[nv-osc=true] + osc\ + cef\ + paks + blobs + locales\ + dependencies\ + www\ + nvc\ |
| `NvStreamSrv\` | `C:\Program Files\NVIDIA Corporation\NvStreamSrv\` | 120 MB | ModuleMap ชี้หา NvStreamBase.dll + RtspServer.dll + GameStream plugin targets |
| `NvBackend\` | `C:\Program Files\NVIDIA Corporation\NvBackend\` | 11 MB | agent ฝั่ง User container (pristine node ต้องมี agent chain) |
| `NvTelemetry\` | `C:\Program Files\NVIDIA Corporation\NvTelemetry\` | 6.2 MB | telemetry service files |
| `System32Shims\` | `C:\Windows\System32\nvspcap64.dll` + `C:\Windows\SysWOW64\nvspcap.dll` | 4.9 MB | shim ในเกม/dwm (ไดรเวอร์เป็นคนโหลด) |

หมายเหตุ symlink: ต้นฉบับ `NvContainer\plugins\LocalSystem\{ShadowPlay,GameStream,NvTelemetry,Watchdog}` และ `plugins\User\NvBackend` เป็น **symlink** ชี้หาโฟลเดอร์อื่น — payload นี้เก็บเป็น**ไฟล์จริง**แล้ว (container อ่านโฟลเดอร์จริงได้เหมือนกัน: SymlinkDirectoryEnumerator traverse ตามชื่อ ไม่ผูกชนิด)

หมายเหตุ Share.json: สถานะปัจจุบัน `nv-osc=true` (แก้จาก false เมื่อ 26 ก.ย. — ห้ามกลับไป false: ปิด native OSC handler ทั้งชุด)

---

## 2) Reassembly Map — payload → ปลายทาง (restore path เดิมเป๊ะ)

| Payload | → วางกลับที่ |
|---|---|
| `NvContainer\` | `C:\Program Files\NVIDIA Corporation\NvContainer\` |
| `NvContainerX86Dlls\` | `C:\Program Files (x86)\NVIDIA Corporation\NvContainer\` |
| `NvContainerX86Exe\` | `C:\Program Files (x86)\NVIDIA Corporation\NvContainer-x86\` |
| `ShadowPlay\` | `C:\Program Files\NVIDIA Corporation\ShadowPlay\` |
| `NvNode\` | `C:\Program Files (x86)\NVIDIA Corporation\NvNode\` |
| `GfeBundle\` | `C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\` |
| `NvStreamSrv\` | `C:\Program Files\NVIDIA Corporation\NvStreamSrv\` |
| `NvBackend\` | `C:\Program Files\NVIDIA Corporation\NvBackend\` |
| `NvTelemetry\` | `C:\Program Files\NVIDIA Corporation\NvTelemetry\` |
| `System32Shims\nvspcap64.dll` | `C:\Windows\System32\nvspcap64.dll` ← **แตะ System32 — แจ้งก่อนทำ** |
| `System32Shims\SysWOW64\nvspcap.dll` | `C:\Windows\SysWOW64\nvspcap.dll` ← เช่นกัน |

---

## 3) Registry — ค่าจริงที่ต้องมี (restore จาก .reg ใน `Registry\` แล้วตรวจทานด้วยรายการนี้)

ไฟล์ export: `HKLM-NVIDIA-Corporation-64.reg`, `HKLM-NVIDIA-Corporation-WOW64.reg`, `HKLM-Service-NvContainerLocalSystem.reg`, `HKCU-NVIDIA-Corporation.reg`

### 3.1 Service `NvContainerLocalSystem` (AUTO_START, LocalSystem)
```
ImagePath = "C:\Program Files\NVIDIA Corporation\NvContainer\nvcontainer.exe" -s NvContainerLocalSystem -a -f "C:\ProgramData\NVIDIA\NvContainerLocalSystem.log" -l 3 -d "C:\Program Files\NVIDIA Corporation\NvContainer\plugins\LocalSystem" -r -p 30000 -st "C:\Program Files\NVIDIA Corporation\NvContainer\NvContainerTelemetryApi.dll" -ert
```

### 3.2 `HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap` (64-bit view — 9 ค่า)
```
MessageBus.dll      = C:\Program Files\NVIDIA Corporation\NvContainer\MessageBus.dll
libprotobuf.dll     = C:\Program Files\NVIDIA Corporation\NvContainer\libprotobuf.dll
libcrypto-1_1.dll   = C:\Program Files\NVIDIA Corporation\NvContainer\libcrypto-1_1.dll
libssl-1_1.dll      = C:\Program Files\NVIDIA Corporation\NvContainer\libssl-1_1.dll
Poco.dll            = C:\Program Files\NVIDIA Corporation\NvContainer\Poco.dll
PocoInitializer.dll = C:\Program Files\NVIDIA Corporation\NvContainer\PocoInitializer.dll
NvStreamBase.dll    = C:\Program Files\NVIDIA Corporation\NvStreamSrv\NvStreamBase.dll
RtspServer.dll      = C:\Program Files\NVIDIA Corporation\NvStreamSrv\RtspServer.dll
CrimsonUtil.dll     = C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\dependencies\CrimsonUtil.dll
```
### 3.3 `HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap` (32-bit view — 6 ค่า)
```
MessageBus.dll / libprotobuf.dll / libcrypto-1_1.dll / libssl-1_1.dll / Poco.dll / PocoInitializer.dll
= C:\Program Files (x86)\NVIDIA Corporation\NvContainer\<ชื่อไฟล์>
```

### 3.4 `Global\NvNode` (ทั้ง 2 views)
```
port            = DWORD 0xe679 (59001)
disableSecurity = DWORD 1
```

### 3.5 `Global\ShadowPlay\NVSPCAPS`
```
IsShadowPlayEnabled     = DWORD 1
IsShadowPlayEnabledUser = DWORD 1
```

### 3.6 `NvContainer\Watchdog` — 4 profiles (ตัวปลุก container ลูกตาม session)
```
SPUserX64:      Folder=...\plugins\SPUser      Container=...\nvcontainer.exe
                Parameters= -f "C:\ProgramData\NVIDIA\NvContainerUser%dSPUser.log" -d "...\plugins\SPUser" -r -l 3 -p 30000 -st "...NvContainerTelemetryApi.dll"
UserX64:        Folder=...\plugins\User         Parameters= -f "C:\ProgramData\NVIDIA\NvContainerUser%d.log" ... (ไม่มี -ert)
SessionX64:     Folder=...\plugins\Session      RunElevated=1, Parameters มี -ert
NetworkServiceX64: Folder=...\plugins\NetworkService   RunAsServiceUser=NetworkService
ทุก profile: Policy="10/300/5", GenerateDump=0, LogFile=C:\ProgramData\NVIDIA\NvContainerWatchdog.log
```

### 3.7 MessageBus config `NvContainer\MessageBus` (ทั้ง 2 views)
```
LogPath = C:\ProgramData\NVIDIA
LogLevel = 4
InstallPath = C:\Program Files\NVIDIA Corporation\NvContainer   (WOW: (x86))
IsAdministratorAccount = true (64-bit view)
```

---

## 4) Boot Order (หลังประกอบไฟล์+registry)

1. `sc create`/`sc config` + `sc start NvContainerLocalSystem` → รอ 3-5 วินาที → ตรวจ: nvcontainer.exe (service) + watchdog ปลุกลูก SPUser/User ใน session user + nvsphelper64.exe
2. Node: Share.exe จะเรียก `nvnodejslauncher.exe --launcher="NVIDIA Share"` เอง → launcher เช็ค "Node already running" → spawn `NVIDIA Web Helper.exe` → index.js (pristine) → :59001
   - ตรวจ: `netstat -ano | grep 59001` LISTENING + nvnode.log "Initialization complete" + **ไม่มี 126/trusted**
3. เริ่ม `NVIDIA Share.exe` (จาก GfeBundle path) → ESTABLISHED :59001
4. Alt+Z → UI สลับ #/base ↔ #/base/main-menu

## 5) จุดตรวจหลังบูต (pass criteria)

- [ ] tasklist: nvcontainer.exe ×3 (1 service + 2 console) + nvsphelper64 + NVIDIA Web Helper.exe + NVIDIA Share.exe ×1-3
- [ ] :59001 LISTENING (PID ของ NVIDIA Web Helper.exe)
- [ ] `C:\Users\<user>\AppData\Local\NVIDIA Corporation\NvNode\nvnode.log`: "Initialization complete." และ grep "126|trusted" = 0
- [ ] CaptureCore.log: "MessageBus joined - System: Shadowplay, Module: ShadowplayServer" + "System: Hotkey, Module: HotkeyPlugin"
- [ ] Share debug.log: "Node info request success" + ไม่มี "Did not handle cef query QUERY_OSC_*" ใหม่
- [ ] Alt+Z (ฉีดคีย์) → window title สลับ index.html#/base ↔ index.html#/base/main-menu

## 6) สิ่งที่ย้ายไม่ได้ / ไม่อยู่ใน payload (อ้างอิง)

- `C:\Windows\System32\nvfbc64.dll`, `nvaudcap64v.dll`, DriverStore = ของ **driver installer** (ไม่ใช่ของ GFE) — ยังอยู่ครบหลังถอด GFE
- Runtime data dirs สร้างเองตอนรัน: `%LOCALAPPDATA%\NVIDIA Corporation\{NvNode,NVIDIA Share}\`, `C:\ProgramData\NVIDIA\`, `C:\ProgramData\NVIDIA Corporation\{NvNode,ShadowPlay}\`
- `piplConfig.json` seed ที่ `C:\ProgramData\NVIDIA Corporation\NvNode\` — ถ้า uninstaller ลบ: node สร้างใหม่เองได้ (เคย EPERM แค่ตอนเขียนไม่ได้ — ไม่ fatal)

---

## 7) ⚠️ บทเรียนจากการประกอบจริง (2026-09-27) — ชิ้นที่ payload แรกขาด + กับดัก 4 อย่าง

การประกอบรอบแรกล้ม 4 ครั้งก่อนผ่าน — ทุกจุดคือ knowledge ที่ต้องจำ:

### 7.1 ชิ้นที่ payload แรกขาด (x86 tree + Update Core) — เติมจาก GFE extract `C:\My Project\GFE\GeForce_Experience_v3.28.0.412\`

| ไฟล์ | ปลายทางที่ถูกต้อง | ใครต้องใช้ | อาการเมื่อขาด |
|---|---|---|---|
| `NvBackendAPI32.dll` | **`C:\Program Files (x86)\NVIDIA Corporation\Update Core\`** | node x86 (`nvLoadLibraryFromTrustedLocation(NvBackendAPI)`) | node ตายทันที: "failed with 126" → Shutdown |
| `NvTmRep.exe`, `NvSHIM.exe`, `ApplicationOntology.7z`, `FeatureWhitelist.json` | `PF(x86)\Update Core\` | agent x86 | (ตาม manifest `BackendAPI32BitDllPath = {UpdateCoreX86Path}\NvBackendAPI32.dll`) |
| `NvBackendAPI64.dll` | `C:\Program Files\NVIDIA Corporation\Update Core\` | ฝั่ง x64 | — |
| `NvDriverUpdateCheck64.dll` | `C:\Program Files\NVIDIA Corporation\NvDriverUpdateCheck\` | driver update check | — |
| `nvspapi.dll`, `ipccommon.dll` (x86 ทั้งชุด ShadowPlay component) | **`C:\Program Files (x86)\NVIDIA Corporation\ShadowPlay\`** | node x86 (`nvLoadSystemLibrary for ShadowPlay API DLL`) | module โหลดไม่ได้ (non-fatal!): ShadowPlay routes 404 ทั้งหมด, node ไม่ join `Hotkey:Node`, Alt+Z เงียบ |
| `config.json` | `C:\ProgramData\NVIDIA Corporation\NvTelemetry\` | telemetry | — |

หลักฐาน path: `ShadowPlay.nvi` + `NvBackend.nvi` มี `BackendAPI32BitDllPath = {UpdateCoreX86Path}\{BackendApiDllNameX86}` และ `InstallLocation = {UpdateCoreX86Path}` (UpdateCoreX86Path = `{NvidiaProgramFilesX86}\Update Core`)

### 7.2 กับดักที่เจอ (เรียงตามที่เจอ)

1. **Service DACL**: `New-Service` สร้าง service ด้วย DACL default ที่ไม่ให้ Interactive User อ่าน → node (unelevated) เปิด service ไม่ได้ → `Failed to open NvContainerLocalSystem service with 5` → node Shutdown. **แก้**: `sc sdset NvContainerLocalSystem "D:(A;;CCLCSWRPRC;;;WD)(A;;CCDCLCSWRP;;;BA)(A;;CCLCSWLOCRRC;;;IU)(A;;CCLCSWLOCRRC;;;SU)"` (ตรงตาม doc 16 ที่เตือนไว้)
2. **`sc create` ใน PowerShell**: quoting พัง (exit 1639) — ใช้ `New-Service -BinaryPathName` แทน หรือ import .reg service key
3. **`.nvi` manifest = แผนที่ติดตั้งที่แท้จริง**: ทุก component มี `.nvi` บอก copyFile target ครบทุกไฟล์ รวมถึงตัวแปร `{UpdateCoreX86Path}` — **อ่าน .nvi ก่อนเสมอ** แทนการเดา path
4. **node restart จำเป็นเมื่อแก้ optional module**: `Loading ShadowPlayAPI module...` พลาด = non-fatal ("Optional module load error!") node รอดต่อแต่ไม่มี routes ShadowPlay ตลอดชีวิต process — แก้ไฟล์แล้วต้อง **kill NVIDIA Web Helper.exe** (Share relaunch อย่างเดียวไม่พอ — launcher เจอ "Node already running" แล้วต่อ node เก่า)

### 7.3 ลำดับบูตที่พิสูจน์แล้วว่าผ่าน (หลังถอด GFE)

```
1. files + registry (.reg import) + System32 shims
2. sc start NvContainerLocalSystem → 3 containers + helper (spawn เอง)
3. NVIDIA Share.exe → launcher → node (จะตรวจ service ผ่าน DACL → โหลด NvBackendAPI32.dll จาก Update Core → Initialization complete → :59001)
4. node โหลด ShadowPlayAPI module (nvspapi.dll จาก PF(x86)\ShadowPlay) → join Hotkey:Node + register routes
5. Alt+Z → WMHK 7 → MessageBus → node callback {"hotkeyId":"OSC"} → UI สลับ → OVERLAY ขึ้นจอจริง (ยืนยันด้วย screenshot: Share dialog + Gallery + notification)
```

### 7.4 ผลยืนยันขั้นสุดท้าย (01:11 local)

- `Initialization complete.` / :59001 LISTENING / openshare=[18,90] / InstantReplay 200
- `Hotkey Id: 7` → `{"hotkeyId":"OSC","hotkeyState":"Down"}` → `POST /OSC/MainView 200` + `NotifyOverlayState`
- **หน้าจอจริง: overlay UI แสดง (Share dialog + Gallery + "Screenshot has been saved to Gallery")**
- เครื่องนี้ **ไม่มี GFE ติดตั้ง** (ถอดแล้ว) = ประกอบเองได้สมบูรณ์ ✓

---

## 8) Phase B ผ่าน — build path (2026-09-27 01:21)

### 8.1 โครงสร้างที่ผ่าน
```
C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\
├── NvNode\   (399 ไฟล์ — node binary แท้ + addons + pristine index.js)
└── Share\    (686 ไฟล์ — NVIDIA Share.exe + osc\ + nv-osc=true)
```
- **node binary แท้รัน index.js จากโฟลเดอร์ตัวเองเสมอ** (ไม่สน argv/cwd) → ย้ายทั้งโฟลเดอร์ = บูต pristine ได้ทันที ใช้ NvUtil.node แท้ = MMF pairing แท้ (ไม่ต้องแตะ shim)
- จุด hardcode ที่ยังอ้าง PF: `NvBackendAPI32.dll` (Update Core), `nvspapi.dll` (PF(x86)\ShadowPlay), nvnodejslauncher — ทั้งหมดยังอยู่ที่ path แท้ = ตัว "NVIDIA Plugin" ฝั่ง service

### 8.2 ผลยืนยัน
- node process path = `build\...\NvNode\NVIDIA Web Helper.exe` · :59001 LISTENING (PID 32808)
- Share.exe process path = `build\...\Share\NVIDIA Share.exe` · ESTABLISHED ×2 ไป build node
- Alt+Z → `Hotkey Id: 7` → `OSC MainView 200` → **overlay main menu เต็มรูปแบบขึ้นจอจริง** (Screenshot/Instant Replay/Record/Broadcast/Performance/Gallery)
- presentation ทำงานเอง (DT/offscreen layer) — ไม่ต้องพึ่งเกม

### 8.3 สคริปต์บูตมาตรฐาน (build path)
```powershell
# 1) ปิด node เดิม (mutex conflict)
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | Stop-Process -Force
# 2) node จาก build (จับ mutex/MMF ก่อน)
Start-Process 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvNode\NVIDIA Web Helper.exe' -WorkingDirectory 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvNode'
# 3) Share จาก build (launcher เห็น "Node already running" → อ่าน MMF)
Start-Process 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share\NVIDIA Share.exe' -WorkingDirectory 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share'
```

---

## 9) Phase C/D สถานะจริง (2026-09-27 03:35)

### สำเร็จ
- container service รันจาก **build path ครบ 3 ตัว** (ImagePath + Watchdog profiles flip ผ่าน PowerShell registry provider — sc config โดน quote ตายเสมอ)
- helper spawn โดย service ได้เอง (SHGetFolderPath anchor PF\ShadowPlay ยังทำงาน)
- node+Share จาก build path pairing ผ่าน (Node info success)
- nvaudcap64v.dll restore จาก Installer2\VirtualAudio.Driver cache → System32 (capcore 0x7E หาย)

### ⛔ ตัวที่เหลือ (การอัดยัง fail)
```
node: Manual Record enable request
  → Caught Exception: setting Capture State Change Mutex: device or resource busy
  → CServerImpl::CreateCaptureSession: E_INVALIDARG m_pSettings
```
- **m_pSettings ว่าง** = settings ของหน้า (SetProperty 237 รายการ) ไปตกกับ container ตัวเก่าที่ตายไป —
  **page ไม่ส่ง settings sync ซ้ำให้ container ใหม่** (node log ไม่มี settings POST หลัง Share relaunch)
- ต้องหา trigger ของ settings-sync ฝั่ง page (sync รอบ boot เท่านั้นหรือ?) หรือ replay settings เข้า container ใหม่
- MonitorHotKeysState: True เมื่อ flip สะอาด / False เมื่อ stack ยุ่ง — ตัวแปรที่แท้ยังต้องสืบ

### บทเรียนกับดักซ้ำ
1. sc config binPath ใน PowerShell = quote พังเสมอ → ใช้ Set-ItemProperty registry provider
2. FailureActions = self-heal ที่ต้องล้างก่อน stop service
3. kill container ตอน Share รัน = Share ตายตาม (bus หลุด) — ต้อง restart Share ทีหลังเสมอ
4. settings sync = one-shot ต่อ page boot — container ตาย = settings หาย ต้องรีเฟรช page
