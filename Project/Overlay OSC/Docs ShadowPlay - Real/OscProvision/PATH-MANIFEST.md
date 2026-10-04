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

## §10 — Phase C: boot script ล็อกแล้ว (2026-09-27 15:38)
- `build\NVIDIA ShadowPlay\start-osc.ps1` = ลำดับบูตมาตรฐาน + ตรวจทุกขั้น + idempotent (รันซ้ำ = "อยู่แล้ว ไม่แตะ" ทุกขั้น)
- ลำดับสุดท้าย: [1] service (RUNNING = ไม่แตะ) → [2] containers ×3 + agent (เช็ค CommandLine `plugins\User` ไม่ใช่แค่ชื่อ process) → [3] node :59001 ตอบ 200 → [4] helper → [5] Share → [6] รายงาน + tail gate
- **บทเรียนใหม่ (สำคัญ): helper ต้องสตาร์ตหลัง node ตอบ 200 เท่านั้น** — สตาร์ตก่อน node พร้อม = join `Hotkey:HotkeyPlugin` แล้วตายใน ~20 วิ (HELPR 8116); หลัง node พร้อม = alive ต่อเนื่อง + `UWPSetOSCDismissHotKeys: eSPHKID_OSC Hotkeys set` (HELPR 12412)
- สแตกที่บูตแล้วจริง (15:36): service PF ✓ · containers ×3 (agent 34544) ✓ · node 2508 :59001 LISTENING + Share ESTABLISHED ✓ · helper 12412 join HotkeyPlugin ✓ · Share ×2 ✓
- เปิดเผย: gate `MonitorHotKeysState = False` ยังโชว์ใน CaptureCore.log (เป็น runtime state ไม่ใช่ registry — 0 hits ทุก hive); ยุค overlay ขึ้นจอ (07:46) ใช้สแตกรูปแบบเดียวกับนี้ — ถ้า Alt+Z ยังเงียบ = ไล่ 4 ชั้น (helper WMHK → node Hotkey Id → page Hotkey:OSC → native Starting DT) แก้จุดเดียว; SP Server enable 0x80040233 (record) ยังไม่แก้ — นอกงาน OSC

## §11 — Phase D: ห่วงโซ่ Alt+Z ฝั่ง stack แท้ (2026-09-27 ค่ำ)
**ผ่านแล้ว (หลักฐานใน CaptureCore.log):**
1. helper จับ Alt+Z (WMHK 7) ✓
2. node แท้ join `System: Hotkey, Module: Node` ได้แล้ว 18:13 (CShadowPlayHotkeyReceiver) — ครั้งแรกของ node แท้
3. `HotKey callback {"hotkeyId":"OSC","hotkeyState":"Down"}` ยิงถึงหน้าแล้ว
4. container spawn Share เองได้ (COscProcMgr::StartProcess) — overlay สร้างได้ (hr[0] 1680×1050)

**สิ่งที่แก้เพื่อไปถึงจุดนี้:**
- `GFExperience\FullPath` (ทั้ง 2 view) → build\Share\NVIDIA Share.exe — เดิมชี้ GFE ที่ถอนไปแล้ว = "failed to start Share Process"
- hardlink NVIDIA Share.exe + MessageBus.dll/libprotobuf.dll/Poco*.dll เข้า PF\...\NVIDIA GeForce Experience\ (COscProcMgr หา path นี้ก่อนเสมอ; MMF libs ต้องอยู่ข้าง Share ที่โดน spawn)
- node เคยไม่ join Hotkey:Node เพราะ MMF lib โหลดไม่ได้ ("Error loading MMF library" จาก NvSDKAPINode.node) — ไม่ใช่ MonitorHotKeysState

**บล็อกสุดท้าย (ตัวเดียว):** `CServerImpl.m_pSettings = NULL` → Get/SetCaptureSessionParam + Get/SetProperty + /Hotkey/monitor ทั้งหมด E_INVALIDARG → enable 0x80004005 → container ฆ่า OSC ที่ spawn ภายใน ~1 วิ → หน้าไม่บูต → settings ไม่ sync (วนกลับมา)
- uiCmd(19) timeout ตอน Share boot = ปกติ ไม่ใช่ตัวขวาง (ยุค Light ก็เจอและผ่าน)
- ยุค Light 03:59:21 พิสูจน์: Share มีชีวิตก่อน container init → CreateServerImplInterface แนบ 1.6 วิ → CreateSettings สำเร็จ → enable hr[0] → อัด+overlay+settings ใช้ได้หมด
- ต่างที่เหลือระหว่างยุค Light กับตอนนี้ = enable IPC ของ shim node (origin 4) vs node แท้ → ขั้นต่อไป: อ่าน shim SP module (Project\NvShadowPlayAPI\NvAPI\shims\) หา settings-init IPC ที่ shim ส่ง แล้ว replay ด้วย node แท้
- เกณฑ์เจ้าของ: Share ต้อง ×3 (browser + 2 CEF worker = หน้าบูตเต็ม) ถึงนับ OSC 100% · ระบบ capture ยังไม่ต้องเปิด · ห้ามเปิด Share ตรง ๆ แก้ปัญหา

## §12 — nv-osc=false + re-arm (2026-09-27 19:0x)
- Owner theory CONFIRMED: render attach (GPU overlay) เป็นส่วนของ spiral ที่ฆ่า Share — GFE dir Share.json มี nv-gpu-accel=true (build มี false) → nv-osc=false ทั้งสองไฟล์แล้ว → Share (build, 2 ตัว) มีชีวิตนิ่ง + หน้า poll node ได้ไกลกว่าเดิม (HardwareInformation 200 / beta 200 / Capture/State 500 = ส่วน m_pSettings ที่ยังค้าง)
- บทเรียนใหม่: node hotkey receiver ต้อง re-arm ด้วย POST /Launch ทุกครั้งหลัง container รีเกิด (Registering callback type 1 → JOIN Hotkey:Node) — 19:04 นายกด Alt+Z แล้ว node ไม่รับเพราะ receiver หลุดไปตอน container รีเกิด
- ยังเปิด: m_pSettings (Capture/State 500) + Share ×3 เกณฑ์ + SP Server run failure count:4

## §13 — ✅ OSC ใช้งานได้จริง — สูตรสุดท้าย (2026-09-27 ~20:00)
**เจ้าของยืนยันด้วยตา: "Share.exe มี 2 ตัว แต่ใช้ได้ทุกอย่างเลย Hook เข้าก็ยังได้"**

## สูตร: "ทุกอย่างจาก Build ยกเว้น nvcontainer.exe"
| ชิ้น | Path | เหตุผล |
|---|---|---|
| nvcontainer (service+SPUser) | PF แท้ | ShadowPlayController::ValidatePID ปฏิเสธ path อื่น ("Unknown executable path") |
| NVIDIA Share.exe | build (nv-osc=true) | ใช้โหมด attach: Share ยืนรอ → container แนบ |
| NVIDIA Web Helper.exe | build | — |
| nvsphelper64.exe | build | ต้องสตาร์ตหลัง node ตอบ 200 |

## ห่วงโซ่ Alt+Z ครบ (ทุกชั้นพิสูจน์ด้วย log)
helper WMHK 7 → node CShadowPlayHotkeyReceiver (System: Hotkey, Module: Node) → HotKey callback {"hotkeyId":"OSC"} → หน้า (socket.io) → overlay

## บทเรียนวันนี้ (รากทั้งหมดที่เจอ)
1. GFExperience\FullPath ชี้ GFE ที่ถอน → "failed to start Share Process" → แก้เป็น build + hardlink Share.exe เข้า GFE dir
2. MMF libs (MessageBus/libprotobuf/Poco) ต้องอยู่ข้าง Share ที่ spawn — node ไม่ join Hotkey:Node เพราะ MMF lib โหลดพัง (ไม่ใช่ MonitorHotKeysState)
3. ValidatePID whitelist = ตัวขวางใหญ่สุด — nvcontainer ต้องเป็น PF เท่านั้น
4. nvspcap64.dll/nvspcap.dll หายจาก System32 → restore จาก payload
5. โหมด spawn ของ container = Share crash (0xc0000005 offset 0x13892c) → ใช้โหมด attach แทน (Share ยืนรอ → container แนบ)
6. receiver ต้อง re-arm ด้วย POST /Launch หลัง container cycle
7. uiCmd(19) timeout ตอน Share boot = ปกติ ไม่ใช่ตัวขวาง
8. Share ×3 = เฉพาะโหมด spawn — ×2 (attach mode) ใช้ได้ครบ (เจ้าของยืนยัน)

## ค้าง (ยอมรับโดยเจ้าของ)
- บั๊ก capture/record (m_pSettings/session) — เจ้าของสั่งไว้ก่อน: "ระบบ Capture ไม่ต้องเปิด ทำ OSC ให้ติดก่อน"
- crash โหมด spawn (0x13892c) — ไม่จำเป็นถ้าใช้ attach mode
- nvspcap64 render in-game integration — ขั้นถัดไป

## §14 — NvPlugins.exe v1 (2026-09-27 ค่ำ) — แกนกลางแบบ Plugin Host
- โค้ด: `Project\NvPlugins\` (C# net10.0-windows WinForms) → build\NvPlugins\NvPlugins.exe
- สถาปัตยกรรม: Plugin Host (INvPlugin: GetInfo/Init/Start/Stop — โหลด plugins\*.dll ผ่าน AssemblyLoadContext collectible) + GUI 3 แท็บ (System tree / Plugins / Inject-Eject) + CLI (check/fix headless)
- CheckPlugin (built-in แต่ผ่าน contract เดียวกัน): File 14 · Registry 8 · Server 6 · Port 2 · DLL 3 · Connection 5 = 38 รายการ
- ผลแรกบน 1080 Ti: OK 36 · FAIL 0 · WARN 2 (FailureActions ไม่มี=ปลอดภัย, :59002 optional)
- Inject/Eject: kernel32 classic (VirtualAllocEx + CreateRemoteThread LoadLibraryW / Toolhelp32 หา base + FreeLibrary)
- ค้างรอบหน้า: RenderPlugin (gpu|hook|topmost) + CapturePlugin (NVIDIA แท้/FFmpeg/OBS) + /DulukaServer/v.1.0/settings + Launcher.Cef dropdown + hook DLL สำหรับ inject
- หลักการ: NvPlugins = แกนกลางทุกระบบยึด (เลียนแบบ NvContainer ที่เป็น generic plugin host) · WinForms NvShadowPlay.exe เดิม = FREEZE · ค่ากลาง = NvConfig\config.json

## §15 — NvPlugins v2: Designer-friendly + EN + Download API :15246 (2026-09-27 ค่ำ)
- MainForm แยก Designer.cs (InitializeComponent มาตรฐาน — เจ้าของแก้ design เองใน VS ได้) + EN UI ทั้งหมด
- คลังกลาง `C:\My Project\NVIDIA-Plugins\Plugins` (22 ไฟล์ 6 หมวด: System32/SysWOW64/GFE/ShadowPlay/NvContainer/NvNode)
- Download API :15246 (**ไม่ใช่ 15249 — พอร์ตนั้น sshd ของเครื่องจับอยู่**): `GET /` ปุ่มเดียว+%bar · `GET /api/manifest` · `GET /file/<rel>` — ทดสอบแล้ว byte ตรง · public URL = http://scotcsduluka.totddns.com:15246 (DNS → 125.24.223.61; ต้อง forward 15246 ที่ router)
- Client auto-deploy: โหลดทีละไฟล์ (% n/N) → วาง hardcoded destinations → set registry (FullPath/NvNode/NVSPCAPS/Watchdog) → บูตทั้งชุด (service→Share รอ→attach→node→re-arm→helper)
- โหมด: GUI · `check` · `fix` · `serve` (headless รันจนถูก kill)
- รอบหน้า: Intel โหลดจาก http://scotcsduluka.totddns.com:15246 → render topmost/hook · ถ้าต้องการ port 15249 จริง ต้องย้าย sshd ก่อน (เจ้าของสั่งเอง)

## §17 — ตัวจริงตัวสุดท้าย: require defaults.js พัง (2026-09-28 03:03)
**root cause ตัวสุดท้าย**: `NvShadowPlayAPINode.js` (shim) require `../../NVIDIA Web Helper.exe/Backend/lib/defaults.js` — โฟลเดอร์ถูกย้าย (reorg → Close Project) → module NvShadowPlayAPI โหลดพังทั้งตัว → fireServer :59002 ไม่เคยขึ้นหลัง wipe → Alt+Z/fire เข้าไม่ถึงหน้า
**fix**: ลบ require เดิม + data-floor.js ฉบับ self-contained (inline ทุก shape: HOTKEY_DEFAULTS 30 ตัว · settings · states · dropdowns — ต้นฉบับ = Close Project\...\Backend\lib\)
**ผล**: fireServer :59002 LISTENING (03:02:42) · :59001+59002 = 200 · fired OpenShare → overlayToggle emitted (03:03:19) — ห่วงโซ่ครบ: nvsphelper(Alt+Z) → :59002 → shim → WindowState → หน้าเปิดเอง
**สถาปัตยกรรมสุดท้าย (ผู้ใช้กำหนด)**: NVIDIA แค่ Share.exe + Web Helper.exe/node — nvcontainer NVIDIA ถูกแทนด้วย NvContainer.exe (ของเรา: spawn/ดูแล node+Share + ถือ pairing) + nvsphelper.exe (hotkey-only) — ทั้งหมดอยู่ในต้นไม้ build\NVIDIA ShadowPlay\ (portable)

## §18 — Phase 1: การทดลองสะอาด nvcontainer แท้จาก build path (2026-10-03)

**Evidence ทั้งหมด**: `OscProvision\Logs\phase1-evidence\` (phase1-clean.log, phase1-RESULT.txt, log-*.log 24 ไฟล์, node-direct-run-stderr.txt) · backup ก่อน flip: `Logs\phase1-before\` (roll back = reg import 2 ไฟล์)

### ✅ ผ่าน (FACT — log จริง)
1. **nvcontainer.exe แท้รันเป็น service จาก build path ได้จริง** — ImagePath → `build\...\NvContainer\genuine\nvcontainer.exe` · service Running · โหลด plugin ครบ (NvcPluginManager: Watchdog/Telemetry/Broker Started) · PID 26176
2. **Watchdog ปลุก container ลูกจาก build path ได้** — log แสดง `Folder: ...\genuine\plugins\SPUser` + `Restart container for ...\genuine\plugins\User` (SPUser/User spawn จาก build จริง; พฤติกรรม spawn-and-exit = UNKNOWN อาจปกติเมื่อ Share ยังไม่ attach)
3. **"Unknown executable path" = 0 บรรทัด** — grep ครบทุก log ทุก dir (ProgramData\NVIDIA, ProgramData\NVIDIA Corporation, LOCALAPPDATA\NVIDIA Corporation) ทั้ง 2 รอบการทดลอง — **บทเรียน §13 ข้อ 3 ("ValidatePID whitelist ตัวขวางใหญ่สุด") ไม่พบหลักฐานสนับสนุนเลยแม้แต่บรรทัดเดียว → ข้อสรุปเดิมมีแนวโน้มเป็น confound สูง** (ยังไม่ปิด — Share/helper ยังไม่ได้วิ่งครบในการทดลอง)
4. Payload stage ครบ: container (nvcontainer 1248KB + TelemetryApi + plugins 5+AIUser จาก PF), node (399 ไฟล์), helper (ShadowPlay set)

### ⛔ ตัวขวางใหม่ (FACT — ไม่เคยรู้มาก่อน)
**node แท้ hardcode ที่อยู่ backend ผ่าน SHGetFolderPath**: binary มี string `SHGetFolderPath failed with %u` + `\NVIDIA Corporation\NvNode\index.js` → ประกอบ path = `PF(x86) + \NVIDIA Corporation\NvNode\index.js` — **ไม่อ่าน registry/config/cwd/ตำแหน่งตัวเอง** (stderr: `Cannot find module 'C:\Program Files (x86)\NVIDIA Corporation\NvNode\index.js'` แม้รันจาก build)
- §8.2 "node แท้รัน index.js จากโฟลเดอร์ตัวเองเสมอ" = **INTERPRETATION ที่ถูกหักล้าง** — nvnode.log ยุคทำงาน (GenuineRuntime, 29 ก.ย.) โหลด addons จาก PF(x86)\NvNode ตลอด → Phase B-D ผ่านเพราะ reassembly §2 ฟื้น PF(x86)\NvNode ไว้จริง
- วันนี้ **NVIDIA App ลบทิ้งหมด**: PF(x86)\NvNode (ว่าง) + registry `Global\NvNode` (port=59001, disableSecurity) + `Global\GFExperience\FullPath` (ทั้ง 2 view) — ทางเดิมตาย

### 🎯 จุดตัดสินใจ OWNER (หยุดรอตามกติกา — แตะ binary หรือของที่ย้ายไม่เจอต้องถามก่อน)
| ทาง | หมายถึง | เสี่ยง |
|---|---|---|
| ก) ฟื้น PF(x86)\NvNode จาก Payload (~350 ไฟล์) | node บูตทันที (กลไกเดิมที่เคยผ่าน) — container/Share/helper ยังอยู่ build | มี dependency ที่ PF อีก 1 ทาง (NVIDIA App update อาจลบซ้ำ) |
| ข) Junction PF(x86)\NvNode → build\NvNode | ไฟล์จริงอยู่ build | บทเรียน §5: GetFinalPathName ตีกลับ → trusted-location ตรวจ addon อาจตาย; ต้อง admin |
| ค) Binary patch Web Helper.exe | ปลด PF ทั้งหมด | ต้องอนุมัติ OWNER + node ตรวจ signature ไฟล์ที่โหลด (log: "File signature verified") — patch อาจพังทั้งสาย |

ทุกทางต้อง re-create registry: `Global\NvNode` (port=59001, disableSecurity=1) + `Global\GFExperience\FullPath` (2 views) — ตามสูตรพิสูจน์แล้วเดิม

### ✅ §18.1 ผลสุดท้าย (2026-10-03 16:30) — OWNER เลือกทาง ก + boot ผ่านครบ

**EXITCODE=0 · เกณฑ์อ้างอิง mission ตรงเป๊ะ: Share: 2 · helper: 1 · node :59001 → 200 · containers จาก build genuine**

หมุดหมายหลักฐาน (CaptureCore.log + nvnode.log — คัดลอกอยู่ Logs\phase1-evidence\):
- `MessageBus joined - System: Hotkey, Module: HotkeyPlugin` ✓ (หมุด §10 ตรงเป๊ะ)
- `UWPSetOSCDismissHotKeys: [2] eSPHKID_OSC Hotkeys set for UWP` ✓
- node แท้ Initialize ครบทุก module (PiplConfig/BackendAPI/Account/DriverInstall/downloader/GameStream/Gallery/Camera) + serve request จริงหลายพัน request
- **grep "Unknown executable path" = 0 บรรทัด ทุก log ทุก dir (final)** — ข้อสรุปเดิม "ValidatePID ปฏิเสธ build path" = **confound ยืนยัน** (หลัง boot ผ่านครบ)

**สูตร restore ที่ต้องมี (NVIDIA App ลบทั้งหมด — start-osc.ps1 [0e]/[1b] ทำ idempotent ทุกบูต + manifest MD5):**
| องค์ประกอบ | ที่ตั้ง | ที่มา |
|---|---|---|
| node tree (399 ไฟล์ + MessageBus x86) | PF(x86)\NvNode\ | Payload\NvNode (+แทน MessageBus x64→x86 จาก NvContainerX86Dlls) |
| agent home | PF\NvBackend\ | Payload\NvBackend |
| NvStreamSrv (bridge x86 + server x64) | PF(x86)\NvStreamSrv\ + PF\NvStreamSrv\ | GFE extract GFExperience.NvStreamSrv |
| NvTelemetry API/Bridge | PF(x86)\NvTelemetry\ + PF\NvTelemetry\ | GFE extract NvTelemetry |
| ShadowPlay ชุดเต็ม (helper anchor) | PF\ShadowPlay\ | Payload\ShadowPlay |
| Update Core (NvBackendAPI32/64 + agent files) | PF(x86)\Update Core\ + PF\Update Core\ | GFE extract NvBackend + Payload\NvBackend |
| NvDriverUpdateCheck | PF\NvDriverUpdateCheck\ | GFE extract |
| nvspapi/ipccommon x86 | PF(x86)\ShadowPlay\ | GFE extract ShadowPlay |
| piplConfig seed | ProgramData\NVIDIA Corporation\NvNode\ | Payload\ProgramDataSeeds |
| registry | Global\NvNode (port/disableSecurity 2 views) · GFExperience (FullPath→build Share, Version, Installed, Architecture) · NVSPCAPS · ModuleMap 9+6 ค่า (NVIDIA App hijack คืนค่าแท้) | .reg harvest + §3.2/§3.3 |

**กับดักใหม่ที่ต้องจำ (บล็อกเรียงตามลำดับที่เจอ):**
1. `Copy-Item src\* dst -Recurse` flatten node_modules → node ตาย require ไม่เจอ — ใช้ copy ทีละไฟล์ (Copy-Genuine-Tree)
2. node แท้ hardcode index.js ผ่าน SHGetFolderPath(PF_x86) — §8.2 "รันจากโฟลเดอร์ตัวเอง" = ผิด
3. NvBackendPlugin init ตาย 2ms ถ้าไม่มี PF\NvBackend (CSIDL_PROGRAM_FILES anchor)
4. GetGFEVersionSync อ่าน registry GFExperience Version — ขาด = RegQueryValueExW(2) fatal
5. Watchdog หมด retry แล้วไม่ลองใหม่ — ต้อง restart service เพื่อ re-arm (เมื่อ service รันอยู่แต่ไม่มี container ลูก)
6. bridge 193 = MessageBus x64 ปนใน tree x86 (loader เจอที่ dir ของ exe ก่อน)
7. consent fatal ต้องมี NvTelemetry API/Bridge ครบทั้ง 2 views + piplConfig
8. helper "Could not locate %s (126)" = SHGetKnownFolderPath(PF)\ShadowPlay anchor — ต้องมีชุดเต็ม
9. nvnode async logger ทิ้ง stack ตอน fatal — stderr capture เท่านั้นที่ได้จริง

**ค้าง (นอกขอบเขต OSC — OWNER สั่งไว้):** POST /Launch → 500 (ตระกูล settings-sync gap — capture layer ยังไม่เปิด 0x80040233) · nvsphelper64 ยังต้องทดสอบ Alt+Z จริงโดย OWNER


## §19 — m_pSettings deadlock แมปครบ (2026-10-03 ค่ำ — input ของ Phase 2/3)

**FACT จาก log + binary strings (ทาง 1 ขั้นแรก):**
1. `CServerImpl::CreateSettings` = ถูกเรียกจาก **constructor/Initialize ของ CServerImpl เอง** (strings: "CServerImpl::CServerImpl: CreateSettings failed" / "Initialize: CreateSettings failed") — ไม่มี failure log = **m_pSettings ถูกสร้างแล้วแต่ "ว่าง"**
2. settings = in-memory ล้วน (ไม่มี settings file — ค้น .json/.dat/.db ใน _nvspcaps64.dll = ไม่มี) — เติมด้วย IPC SetProperty stream จากหน้า osc เท่านั้น
3. หน้า osc **connected แล้ว** (`Socket XA27uQE04tV--rARAAAA connected` 21:35) — boot หน้า: HardwareInformation 200 → UserToken 500 (ไม่ login) → PrivacySettings 200 → **DynamicToggle 500** → Launch 200 → Language/beta 200 → **Capture/State 500 → หน้าหยุด** → DesktopCapture/Support/Reason ไม่ถูกเรียก → ไม่มี settings sync
4. `COverlayApi::CreateOverlay: hr[0]` = overlay สร้างสำเร็จบน Share ตัวใหม่ ✓
5. `CreateSymbolicLink failed = 3` = หายไปแล้วหลังฟื้นชุด x86 ShadowPlay (NvRemux.dll ฯลฯ) — ไม่ใช่ตัวขวางแล้ว
6. ตัวฆ่า Share = **full service restart เท่านั้น** · SPUser-only respawn ปลอดภัย (Watchdog respawn รอบแรก, Share รอด — พิสูจน์ด้วย A4 + ProcMon) → recovery ของ supervisor = `-RecoverSpUser` (ใส่ใน start-osc แล้ว)

**deadlock เชิงโครงสร้าง**: settings ว่าง → Capture/State 500 → หน้าไม่ sync → settings ว่าง (วงจร) — การเปิดต้องแก้ที่ handshake IpcCommon ระหว่าง SP Server ↔ Share's shadowplay2 (ทาง 1) หรือ deviation ที่ต้องขออนุมัติ (patch หน้า osc / binary)

**A3 ตัดทาง C**: เครื่อง Intel (HUAWEI-PC) ไม่มี key `GFExperience\ShadowPlay` เลย → GFE ไม่เขียน Cfg2/Cfg3 บนเครื่องไม่มี NVIDIA GPU → ตัดตามเงื่อนไข ✓

**สถานะ stack ปัจจุบัน (รอดหลัง A7 boot 20:16 + fixes)**: container ×3 (Service/User/SPUser — ShadowplayServer joined) · node 3528 (200, callback armed) · Share ×2 (overlay hr[0]) · helper armed · Launch 200 · เหลือ: settings sync + hotkey config

### §19.1 — HANDOFF session ถัดไป (2026-10-03 ค่ำ — OWNER decision: ทาง 1 สายหลัก, C ตัด, ทาง 2 งด)

**สถานะปัจจุบัน (แก้ไขจาก §19 เดิม — ปลดความกำกวม):**
- m_pSettings = **มีตัวตน** (สร้างเงียบใน CServerImpl constructor — log เฉพาะตอน fail, ไม่เคย fail) **แต่ว่าง** → ทุก Get/Set คืน E_INVALIDARG 0x80070057
- หน้า osc **connected แล้ว** (socket.io) — boot หน้า: HardwareInformation 200 → UserToken 500 (ไม่ login) → PrivacySettings 200 → DynamicToggle 500 → Launch 200 → Language/beta 200 → **Capture/State 500 → หน้าหยุด**
- **จุดตัดสินใจเดียวของหน้า = GET /ShadowPlay/v.1.0/Capture/State** — ถ้า 200 ทั้ง settings sync และ hotkey จะไหลเอง

**ทาง 2 งด (พิสูจน์จาก app.js unpacked — docs/osc/unpacked/src/app/0135.appService.js):**
- boot chain แบบคลื่น `.then()`: wave 2 = `h.init()` = shadowPlayService.init (มี Capture/State — 500 = reject) → `t.all()` ตาย → **wave 7 (`g.init()` = hotkeyService.init — ตัว register OSC_TOGGLE handler) + wave 8 (`h.setOscReady()` = หมุด OSC-ready) ไม่มีวันรัน**
- handler ถูก register **หลัง** Capture/State → เขียน Cfg2/Cfg3 ให้ helper ก็ Alt+Z ยังเงียบ (หน้าไม่มี handler) — งด RE format
- อธิบายยุค §13: Alt+Z ได้เพราะ chain หน้าเคยจบครั้งแรกแล้ว (Cfg2/Cfg3 ถูกเขียนช่วงนั้น) — NVIDIA App มาเคาะทีหลัง

**ขั้นต่อไปของทาง 1 (เรียงตามลำดับ):**
1. RE `CServerImpl::GetCaptureState` ใน _nvspcaps64.dll — ทำไม settings ว่าง = E_INVALIDARG (0x80070007) แทนการตอบ default state — จุดเดียวจบวงจร
2. ถ้า patch/แก้จุดเดียวได้ (ต้องอนุมัติ OWNER ถ้าเป็น binary) → Capture/State 200 → หน้า boot ต่อ → DesktopCapture/Support/Reason → settings sync → CreateSettings/SetProperty → hotkey SET → Cfg2/Cfg3 → Alt+Z ครบทั้งสายโดยไม่แตะ handshake
3. ถ้าจุดเดียวไม่พอ ค่อยขยายเป็น IpcCommon handshake (SP Server ↔ shadowplay2 ของ Share)

**recovery procedure (ห้ามลืม):** ตัวฆ่า Share = full service restart เท่านั้น · ฟื้นตัว = `start-osc.ps1 -RecoverSpUser` (kill เฉพาะ SPUser ห้าม restart service) · ลำดับ Launch-200 ถาวร = [5] kill zombie → Share สด attach 15 วิ → [6] Launch ครั้งเดียว (guard fail-2)

**A3:** ทาง C ถูกตัด — เครื่อง Intel (HUAWEI-PC) ไม่มี key `GFExperience\ShadowPlay` เลย (reg query คืน ERROR)

### §19.2 — เจาะ error ของ Capture/State ได้เป๊ะ (ยิบหลักฐาน — ปรับเป้า RE ให้แคบลง)

- `GET /Capture/State` (21:41:06, request #25) → node raise **ในเครื่องทันที**: `{code:-2147024809 = 0x80070007}` — **CaptureCore เงียบสนิทช่วงนั้น** = ไม่ได้ยิงไป SP Server เลย (ต่างจาก GetProperty/SetCaptureSessionParam ที่เด้ง m_pSettings จาก container)
- ตัว raise = **`CShadowPlayApi::GetCaptureState` ฝั่ง node** (NvShadowPlayAPINode.node) — fail ก่อนติดต่อ container
- บริบทรองรับ hint ของ OWNER: `GET /Account/v.1.0/UserToken → 500` (ไม่ login) + SP Server log `UserID=`undefined`` → เงื่อนไข fail น่าจะเป็น **user context** ไม่ใช่ settings เอง
- **เป้า RE ถัดไป (แคบมาก)**: หาเงื่อนไข early-exit ใน `CShadowPlayApi::GetCaptureState` (NvShadowPlayAPINode.node) ว่าอ่าน state จากไหน (user session? MMF? registry?) — แก้ให้คืน default state ได้ = หน้า boot ต่อ = ทั้ง settings sync + hotkey ไหลเอง

### §19.3 — แก้บันทึก + ผลการอ่าน call site JS (ปิด session 2026-10-03 ค่ำ — คำสั่ง OWNER)

**1. แก้ error code (OWNER ถูกต้อง):** -2147024809 = **0x80070057 = E_INVALIDARG** (ไม่ใช่ 0x80070007 ตามที่เคยเขียน) — E_INVALIDARG = อาร์กิวเมนต์ไม่ถูกต้อง → ตอน RE ให้ไล่จาก "ใครส่งอะไรเข้ามาผิด" ไม่ใช่ "state อ่านจากไหน" อย่างเดียว

**2. call site JS อ่านแล้ว (ฟรี — ก่อน RE binary):**
- `NvShadowPlayAPI.js:1893` — `GET /Capture/State` → `api.CaptureState(doReply)` — **native รับแค่ callback ไม่มี args อื่น** (ไม่มี token/userId/session ส่งจาก JS) → "ใครส่งผิด" = internal state ของ addon ล้วน
- **UserToken 500 = `Failed to get User Info` (0x80070002 ERROR_FILE_NOT_FOUND)** — native อ่าน user info จาก **accounts store ที่หายไป**: strings ใน NvAccountAPINode.node = `\NVIDIA\accounts` + `userid`/`null` + `User-*-NotificationSettings.dat` + `InstallerGrantsConsent.txt` — NVIDIA App เคาะ store นี้ไป
- **ยืนยัน insight ของ OWNER**: ยุคทำงาน UserToken = 401 (account service ตอบ offline ปกติ — มี store) · ของเรา = 500 (store หาย — สาย account พังก่อนตอบปกติ) → **การแก้อาจอยู่ที่ provisioning ฝั่ง account store ไม่ใช่ RE GetCaptureState เลย**
- **⚠ crash mode ใหม่ที่ค้นพบ (ห้ามทำซ้ำ):** สร้าง `%LOCALAPPDATA%\NVIDIA\accounts` เปล่า → native ตอบ NoAccount **ไม่มี err + userInfo ว่าง** → `NvAutoDownload.js:965 JSON.parse(response.userInfo)` throw "Unexpected end of JSON input" → node ตายทันที — **ต้อง seed ไฟล์ accounts ให้ตรง format ถึงจะปลอดภัย** (dir เปล่า = พิษ) — ทดลองแล้ว revert คืนสถานะเสถียรเรียบร้อย (node 200 ✓)

**ขั้นแรกของ session ถัดไป (เรียงตามลำดับ):**
1. ไล่ user context: หา format/ชื่อไฟล์ใน `\NVIDIA\accounts` (RE เฉพาะจุด: strings รอบ userid/User- ใน NvAccountAPINode.node + ProcMon ตอน UserToken) → seed ให้ UserToken กลับเป็น 401/200 ตามยุคทำงาน
2. แล้ว GET /Capture/State จะตอบตาม (ถ้า root คือ user context) → หน้า boot ต่อ → settings sync → hotkey SET → Alt+Z
3. RE binary (`GetCaptureState` early-exit) = เฉพาะเมื่อข้อ 1 ไม่พาไปถึง

## §20 — คำตัดสินใจใหม่ของ OWNER: สถาปัตยกรรมปลายทาง + ลำดับงาน (จดตามคำสั่ง OWNER 2026-10-03 หลัง §19.3)

**สถาปัตยกรรมปลายทาง (หมุดหมายสูงสุด — ทุกงานต่อจากนี้วัดกับสิ่งนี้):**
- **OSC แท้ 100%** — UI + host + node ทำงานตามระบบแท้ทั้งหมด
- **CaptureEngine เป็นของเรา: NvCapture.exe** — NVIDIA capture engine ถ้าข้าม/ถอดได้ให้ข้าม

**ลำดับงาน (OWNER สั่ง — เรียงตามลำดับ):**
1. **ปลด Capture/State → 500 ให้หน้า osc init ครบ** — เริ่มจากอ่าน call site ใน NvShadowPlayAPI.js ก่อน RE binary ตาม §19.2 (error จริง = **E_INVALIDARG 0x80070057 = args ไม่ถูก** → ไล่จาก **UserToken 500-vs-401 ก่อน**)
   - สถานะตาม §19.3: call site อ่านแล้ว (`api.CaptureState(doReply)` — native รับ callback อย่างเดียว ไม่มี args จาก JS) → "args ไม่ถูก" = internal state ของ addon → ตัวตั้งต้น = **UserToken 500 (0x80070002) จาก accounts store หาย** → งานถัดไปตาม §19.3 = seed accounts store ให้ตรง format (dir เปล่า = พิษ ห้ามทำซ้ำ) ก่อน RE binary
2. **เป้าหมาย Capture/State ระยะยาว = ตอบจากสถานะจริงของ NvCapture.exe** (แนว /Duluka plane ของสาย shim ที่เคยพิสูจน์) — **ไม่ใช่จาก SP Server**
3. **ห้ามเสียเวลาแก้ 0x80040233 / SP Server enable / NVIDIA capture แท้** — OWNER ตัดสินแล้ว (ต่อยอดคำสั่งเดิม §13 "ระบบ Capture ไม่ต้องเปิด" — ตอนนี้ชัด: ของแท้ไม่แก้ แล้ว capture มีเจ้าของใหม่ = NvCapture.exe)
4. **การถอด capture stack ทำหลัง Alt+Z ผ่านเท่านั้น** — ทีละชิ้น ตรวจ boot ทุกครั้ง (**HotkeyPlugin ถูก gate ด้วย capture stack**)

**ขอบเขตของหลักการ "ไม่พึ่ง SP Server" (OWNER ชี้แจงเพิ่ม — ผูกกับข้อ 2/3):** ขอบเขต = **capture เท่านั้น** — settings/hotkey handshake ยังไหลผ่าน container ตามระบบแท้ (หน้า sync → SetProperty → Cfg2/Cfg3 → helper register) ดังนั้น: ถ้า Capture/State ผ่านแล้วหน้า sync แต่ container ยังปฏิเสธ SetProperty — งาน m_pSettings SET (เดิมคือทาง 1 ขั้น 3 IpcCommon) **กลับมาเป็น contingency หลักทันที ไม่ใช่ถูกตัดทิ้ง**

**กติกาเดิม (ยังผูกทุกข้อ):** verify ด้วย log จริง / ห้าม push GitHub / UAC ผ่านอัตโนมัติแล้ว

**ผลต่อแผนเดิม (สรุปเพื่อ session ถัดไป):**
- ขั้นแรก §19.3 (seed accounts store → UserToken กลับ 401/200) = ยังอยู่เส้นทางเดิม สอดคล้องข้อ 1 ตรง — ไม่ต้องเปลี่ยนแผน
- RE `GetCaptureState` early-exit (§19.2 / §19.3 ข้อ 3) = เฉพาะเมื่อ seed ไม่พาไปถึง — ถ้าต้อง patch ยาว ให้เขียนไปทางตอบจากสถานะ NvCapture.exe ไม่ใช่อาศัย SP Server
- IpcCommon handshake (ทาง 1 ขั้น 3 ของ §19.1) = **contingency หลัก** ตามขอบเขตด้านบน (ยังไม่ตัดทิ้ง) — ใช้ทันทีเมื่อหน้า sync แล้ว container ยังปฏิเสธ SetProperty
- recovery `-RecoverSpUser` (§19.1) + Launch-200 ถาวร [5]→[6] = ยังผูกเหมือนเดิม

### §20.1 — ผลการทำงานตาม §19.3 (2026-10-03 ดึก — session หลัง commit §20 3aceb8fe0a)

**1. Accounts store แก้สำเร็จ (FACT — curl + log จริง):**
- `\NVIDIA\accounts` = **ไฟล์ ไม่ใช่โฟลเดอร์** (`%LOCALAPPDATA%\NVIDIA\accounts`) — format = **Base64(UTF-16LE(JSON(userInfo)))** — decode จากไฟล์ 656 bytes ที่ native เขียนเอง
- อธิบายครบ 3 อาการ: ไฟล์หาย → CreateFile fail 0x80070002 → "Failed to get User Info" → 500 · สร้าง**โฟลเดอร์**เปล่า (§19.3 crash test) → native อ่านได้ 0 bytes → NoAccount + userInfo ว่าง → NvAutoDownload `JSON.parse("")` ตาย · มีไฟล์ถูก format → อ่านคืนปกติ
- **ขั้นตอน seed ที่ปลอดภัย (ไม่ต้อง RE format ด้วยมือ):** `POST http://127.0.0.1:59001/Account/v.1.0/UserToken` body `{"userInfo":{...}}` (object — handler stringify ให้เอง, NvAccountAPI.js:350 write mode) → native เขียนไฟล์ใน format มันเอง → ทดสอบแล้ว: 200 + ไฟล์โผล่ + GET คืน userInfo ครบ · ตัวอย่าง body ที่ใช้: `{userId:"0", deviceId:"<GUID>", displayName:"", email:"", buildPreference:"", dataTracking:{trackFunctionalData/trackTechnicalData:{level:"Full"}, trackBehavioralData:{level:"None"}}}`
- **ผลหลัง restart node (start-osc.ps1 elevated — kill node ก่อนแล้วรันสคริปต์):** `GET /Account/v.1.0/UserToken = 200 เสถียร` · หน้า boot ไกลขึ้นจริง: PrivacySettings เริ่มส่ง `userId=0` ตาม seed (nvnode.log #7)
- ยืนยัน insight §19.3: **401 ยุคทำงานไม่ได้มาจาก node handler** — `replyWithError` แจกแค่ 400/500 (NvAccountAPI.js:95-108) — 401 ต้องมาจากชั้นอื่น; สถานะเราตอนนี้ = 200 + userInfo "ยังไม่ login"

**2. Capture/State ยัง 500 — และ §19.2 ถูกหักล้าง (FACT — log คู่ขนานเป๊ะ):**
- `GET /Capture/State` **ไม่ได้ raise ในเครื่อง** — ยิงถึง server จริงผ่าน `GetProperty("DwmEnabled")`: คู่ขนาน NODJS `CShadowPlayApi::GetProperty: FAIL[0x80070057] name DwmEnabled` ↔ CNTNR `CServerImpl::GetProperty: E_INVALIDARG m_pSettings` — เข้าคู่กับ request #13 ของหน้า (22:44:29.908/.909) และ curl ของ session นี้ (22:54:06.787/.788) ทั้งสองครั้ง
- root จึงกลับเป็น **จุดเดียวของ §19.1: CServerImpl ปฏิเสธ Get/Set ทั้งหมด** — วันนี้ Get/Set สำเร็จ = **0 ครั้งทั้งวัน** (grep `out pArgs hr[0]` ทั้งไฟล์ = 22 ครั้ง ล้วนเป็น EnableShadowPlay ที่ plugin override เป็น error code อยู่ดี)
- CSettings มีชีวิต: ไม่มี "CreateSettings failed" เลย (count=0) · NVEnc caps probe ครบ H264/H265/10-bit (22:44:12) — ของที่พังคือ guard Get/Set ของ CServerImpl ไม่ใช่ตัว settings object
- **EnableShadowPlay วันนี้**: `SetSP failed! 0x80004005` (22:44:13 หลัง attach) + `KillProcess: failed to kill 4684 (error 5)` — ตระกูล enable ที่ OWNER ตัดไว้ · เช้านี้ (A1 window) = `failed to start Share Process!` 0x80040233

**3. ผลตัดสิน contingency (จุดที่ OWNER สั่งให้วัด):**
- เงื่อนไขใน §20 ("Capture/State ผ่านแล้วค่อยดู SetProperty") **ไม่เกิด** — Capture/State ชนกำแพงเดียวกับ SetProperty (server guard เดียวกัน)
- **งาน m_pSettings / Get-Set ฝั่ง server = critical path ไม่ใช่ contingency** — แต่ทางแก้ต้องไม่ใช่ SP Server enable แท้ (OWNER ตัด) → เหลือ 2 ทาง รอ OWNER เลือก:
  - **(ก) patch `NvShadowPlayAPINode.node`** ให้ `GetCaptureState` ตอบ default state โดยไม่ถาม DwmEnabled จาก server (เข้ากับ strings addon: "Shadowplay is not running..." status gate) — ⚠ ต้องอนุมัติ OWNER (แตะ binary NVIDIA) + **ต้อง patch ที่ Payload/stage ด้วย** เพราะ start-osc.ps1 [0e] จะ re-stage จาก Payload ทุกบูต (MD5) — patch ในไฟล์เดียวจะโดนทับ
  - **(ข) เดินสถาปัตยกรรม §20 ตรง ๆ:** ให้ NvCapture.exe เป็นผู้ตอบ Capture/State ผ่านแนว /Duluka plane (shim/MMF พิสูจน์แล้ว §17) — settings/hotkey ยังไหลผ่าน container ตามขอบเขต §20

**4. กับดัก/บทเรียนใหม่ (ห้ามลืม):**
- start-osc.ps1 ต้อง elevated (guard [0e]) — รันผ่าน `Start-Process -Verb RunAs` + wrapper script เก็บ output ลง `%TEMP%\start-osc-run.log`
- CaptureCore.log timestamp = `[DD:HH:MM:SS:mmm]` (ไม่มีเดือน/ปี) · ไฟล์ครอบคลุมเฉพาะวันนี้ — **log ยุคสำเร็จ (§8.2/§13) ไม่เคยถูก archive** → กติกาใหม่: ถ้า boot ผ่านเกณฑ์ ให้ copy CaptureCore.log เก็บไว้ทุกครั้ง
- node peers: `ShadowplayApi22580, ShadowplayServer` — bus เห็นกันทั้งคู่ (MessageBus join ปกติทั้งสองฝั่ง)

### §20.2 — OWNER ตัดสิน: เลือก (ข) ปฏิเสธ (ก) + นโยบาย "genuine + registered patches" (2026-10-03 ดึก)

**คำตัดสินใจ:** ตาม §20.1 ข้อ 3 — **เลือก (ข)** (สถาปัตยกรรม §20 ตรง: NvCapture.exe เป็นผู้ตอบ capture boundary) · **ปฏิเสธ (ก)** (patch .node binary) เหตุผล 3 ข้อ:
1. .node addon ถูก signature-verify ตอน boot (nvnode.log: "File signature verified" ต่อโมดูล — พิสูจน์แล้ว) — patch = เสี่ยงโมดูลไม่โหลดทั้งสาย ShadowPlayAPI
2. ขัดหลัก "OSC แท้ 100% ทำงานตามระบบแท้" ที่ OWNER ย้ำ
3. แก้ GetCaptureState เดี่ยว ๆ ยังชนกำแพงเดิมตอน settings sync ต่อ (Get/Set สำเร็จ = 0 ตลอดวัน) — เสียแรงไม่ปลดปลายทาง

**Implementation (ข) — ตัดที่ชั้น JS (NvShadowPlayAPI.js = ไฟล์ JS ธรรมดา ไม่ถูก sign — สาย shim เคยพิสูจน์แนว surgical edit):**
- จุดตัด = ขอบเขต capture ตาม §20: **Capture/State + Capture/PIDMode → ตอบจากสถานะ NvCapture.exe แนว /Duluka plane**
- **INTERIM (จดตามคำสั่ง OWNER):** ถ้า wire ไป NvCapture.exe ยังไม่พร้อม → ตอบ static ก่อน:
  - `Capture/State` → `{"state":"Ready"}` (shape จากหน้า: `e.data.state` — 0135/0004.shadowPlayService.js:1405; ห้ามตอบ "PID" = ทาง notebook co-proc)
  - `Capture/PIDMode` → `{"valid":false}` (shape: `e.data.valid` — :130; desktop แท้ = PID mode ไม่ valid)
- Patch อยู่ที่ **PF(x86)\NvNode\NvShadowPlayAPI.js** (tree ที่ node โหลดจริง — §18) · genuine คงไว้ที่ Payload\NvNode · marker: `[NvCapture-interim §20.2]`

**นโยบาย MD5 manifest ใหม่ (OWNER กำหนด):** นิยาม = **"genuine + registered patches"** — patch ชั้น JS ต้อง**ลงทะเบียน**เป็น patch ที่ตั้งใจ (พร้อมเหตุผล — ทะเบียนอยู่ในหัวข้อนี้) ไม่ใช่ถูก flag เป็น corruption แล้ว re-stage ทับ · start-osc.ps1 มี step **[0h]** re-apply patch ถ้า marker หาย (idempotent — กัน NVIDIA App ลบ tree แล้ว [0e] restore ของแท้ทับ; backup ของแท้ = `Logs\phase1-evidence\patch-backup\NvShadowPlayAPI.js.genuine` + Payload\NvNode)

**คาดหน้าต่อไป + วิธีวัด (OWNER กำหนด):** หลัง Capture/State ไม่ 500 หน้าเดินต่อใน init chain (DesktopCapture/Support/Reason → Broadcast2K → MainView → AudioSettings → 8K60) — จับ request log หา gate ถัดไป (น่าจะ settings sync) · **ถ้า container ยังปฏิเสธ Get/Set ให้สืบสาย `CSettings::Refresh GetMsHybridSystemConfigInfo failed` ก่อนหนึ่งรอบ** — m_pSystemConfig init ไม่ครบ อาจเป็นสาเหตุจริงที่ Get/Set ตาย (แก้จุดนั้น = ทางแท้สุด ไม่ต้อง patch อะไร) — "benign บน desktop" ตอนนี้เป็น assumption ยังไม่ใช่ evidence

**กติกา:** "archive log ทุกครั้งที่ boot ผ่าน" — OWNER อนุมัติแล้ว ใช้ได้เลย

### §20.3 — ผล patch [0h]: หน้าผ่าน Capture/State ครั้งแรก + gate ถัดไป + MsHybrid ถูกตัด (2026-10-03 ดึก — หลักฐาน: Logs\phase1-evidence\log-*-20261003-2348-postPatch-firstPass.log)

**1. Registered patch [0h] ใช้งานได้จริง (FACT — curl + request log จริง):**
- patch ลงที่ PF(x86)\NvNode\NvShadowPlayAPI.js — marker `[NvCapture-interim §20.2]` 2 จุด (บรรทัด ~1907/~1931) · genuine call ถูก comment ไว้คืนทุกจุด · start-osc.ps1 มี step **[0h]** re-apply อัตโนมัติถ้า marker หาย (idempotent) · backup ของแท้ = `Logs\phase1-evidence\patch-backup\NvShadowPlayAPI.js.genuine` + Payload\NvNode
- `GET /Capture/State → 200 {"state":"Ready"}` · `GET /Capture/PIDMode → 200 {"valid":false}` (curl จริง)
- **หน้า boot ขยับครั้งแรกตั้งแต่ §19**: chain #13 `Capture/State → 200` (เดิม = จุดตาย wave 2 ตาม §19.1) → เดินต่อทันที
- กับดักเล็ก: node ที่เกิดจาก script elevated kill จาก shell ปกติไม่ได้ (Access denied) — ต้อง kill ผ่าน elevated wrapper แล้วรัน start-osc ใหม่ ไม่งั้น script เห็น "node อยู่แล้ว path ตรง" และ**ไม่โหลด patch** (JS โหลดตอน node start เท่านั้น)

**2. Gate map ของ init chain (ณ จบ session นี้):**
- ผ่านแล้ว: HardwareInformation 200 · **UserToken 200 (§20.1)** · PrivacySettings 200 · Launch 200 · **Capture/State 200 (patch นี้)**
- ไม่ block: POST Hotkey/DynamicToggle 500 (หน้าเดินต่อได้) · GET /Launch 200 (แต่ POST /Launch ยัง 500 — script [6] guard)
- **gate ถัดไป = `DesktopCapture/Support/Reason → 500`** (#15, 23:38:19) — addon คิวรี `GetProperty("IsDesktopCaptureSupportedReason")` → server E_INVALIDARG (คู่ขนาน NODJS↔CNTNR เหมือนเดิม)
- อยู่ถัดจากนั้น (ยังไม่ถึง): Broadcast2K → MainView → AudioSettings → 8K60 — ล้วนอ่านผ่าน Get/Set สายเดียวกัน
- หมายเหตุ: DesktopCapture/Support/Reason เป็น**capability query** — attach-time probes ของ server คำนวณครบแล้ว (หน้าต่าง CreateServerImplInterface: GetMPOSupportedReason/GetHDRScreenshotSupportedReason/NVEnc caps ฯลฯ ผ่านหมด) แต่ถูก guard ปฏิเสธก่อนอ่านได้

**3. MsHybrid round (คำสั่ง OWNER หนึ่งรอบก่อน deep-RE): ตัด — ไม่ใช่ root**
- `CSettings::Refresh GetMsHybridSystemConfigInfo failed` + `CSystemConfiguration::GetMsHybridSystemConfigInfo Adapter enumeration is invalid` เกิด**รอบเดียวทั้งวัน** (บูต 21:07 CNTNR 10248) · log level **[R] ไม่ใช่ [E]** · เป็น refresh path ไม่ใช่ init path
- Get/Set ถูกปฏิเสธใน**ทุกบูต** รวมบูตที่ไม่มี MsHybrid fail · `GetMsHybridByDefaultSupportedReason dwRetCode(0x1)` ปกติทุกบูต
- สรุป: ไม่ใช่สาเหตุของ Get/Set ตาย — ไม่ต้องตามต่อ

**4. ต่อไปตามแผน OWNER: deep-RE guard Get/Set ของ CServerImpl (_nvspcaps64.dll — RE อ่านอย่างเดียว)**
- คำถามที่ RE ต้องตอบ: guard ไหนยิง E_INVALIDARG — (i) m_pSettings จริง ๆ เป็น NULL (ขัด CreateSettings-ไม่เคย-fail) (ii) settings map ว่าง + SetProperty ก็โดน guard ก่อนเขียน (iii) state gate (server "not enabled" หลัง SetSP fail)
- hypothesis แรกจาก strings: `CSettings::SetParam: m_eActiveSPClient[%d]` — server track **active SP client** · วันนี้ Get/Set ล้วนมาจาก NODJS client · **Share client ไม่เคยยิง Get/Set เลย** (0 ครั้งทั้ง log — ทำแค่ CreateShadowPlayApiInterface + GetCaptureSessionParam ตอน attach) · enable มาจาก rundll32 origin(7) + NODJS origin(4) — ถ้า guard ผูก active client กับ enable/attach handshake = ตรง hunch §19.1 ทาง 1 ขั้น 3 (IpcCommon handshake SP Server ↔ Share's shadowplay2)
- กติกา: RE = อ่านอย่างเดียว · patch binary ต้องขออนุมัติ OWNER (กฎ §18)
- **หมายเหตุ repo:** build\NVIDIA ShadowPlay\start-osc.ps1 ([0h] ใหม่) อยู่**นอก git** (build tree = portable ไม่ track) — ทะเบียน patch ฉบับเต็มอยู่ใน §20.2 + ไฟล์จริงบนดิสก์

### §20.4 — ROOT CAUSE: ValidatePID ปฏิเสธ nvcontainer จาก build path ในขั้น SetSP (2026-10-03 ดึกล่าง — ก่อนเข้า deep-RE)

**ขอบเขตชี้แจง OWNER (จดตามคำสั่ง):** "ห้ามแก้ SP enable" ใช้กับ **NVIDIA capture engine เท่านั้น** — ถ้า RE พิสูจน์ว่า state machine (SetSP/enable) เป็นเงื่อนไขของช่องทาง settings/hotkey ที่ UI ยังต้องใช้ การแก้ = **อยู่ใน scope** (ทำให้ระบบแท้ทำงานจริง) — capture ยังเป็น NvCapture.exe ตาม §20 เสมอ

**หลักฐาน 3 ชิ้นฟรี (ตามคำสั่ง):**
1. **client registration**: Share client ทำแค่ `CreateShadowPlayApiInterface: IN ver(10008) client(0)` — ไม่มี RegisterClient จาก Share เลย (เจอเฉพาะ NODJS → `HelperClientInterfaceImpl::RegisterClient Client connected sucessfully` ตอน node 22:21) — **แต่ไม่ใช่จุดขวาง** (ดูข้อ 3 — ตัวจริงอยู่ที่ SetSP)
2. **Share debug.log วันนี้**: CEF boot ปกติ (Node already running → Node info request success → Site load done) — ไม่มี shadowplay2-fail · มี `should close osc` ซ้ำหลายรอบ = container ฆ่า OSC เมื่อ boot ไม่สมบูรณ์ (พฤติกรรม §11 ตรง) — ขั้น shadowplay2 ไม่ได้หาย แต่ถูกฆ่าท้ายสาย
3. **SetSP failed ไม่ได้ตายเพราะอ่าน property — ตายที่ ValidatePID** (log คู่เดียวกันทุกบูตที่ enable ถึงขั้นนี้: 18:11/18:29/18:33/18:50/23:38):
```
CServerIpc::SetSP : IN(16272, 5)                                    ← SPUser container (PID 16272) แจ้ง state 5 ไป service container
ShadowPlayController::ValidatePIDUnknown executable path C:\My Project\...\build\NVIDIA ShadowPlay\NvContainer\genuine\nvcontainer.exe
ShadowPlayController::SetSP: ValidatePID failed with error 0x000003F0
ShadowPlayServicePlugin::ProcessReceivedIpcMessage: ConfigureSP failed 0x000003F0
CServerImpl::EnableShadowPlay: SetSP failed! 0x80004005
```
- ลำดับ enable ก่อนตาย: `EnableNvFBC: OUT ret 0x0` (ผ่าน) → `ControlHeplerProcess` → `Starting nvosc` (125ms ผ่าน) → `SetSP` ← **ตายที่นี่ทุกครั้ง** · caller ที่โดนเช็ค = **exe path ของ SPUser container ที่รันจาก build**

**สายใย root เดียว (ประกอบครบทุกเหตุการณ์ตั้งแต่ §18):**
- §13 รู้ไว้แล้ว: "nvcontainer ต้องเป็น PF เท่านั้น — ShadowPlayController::ValidatePID ปฏิเสธ path อื่น ('Unknown executable path')" — สูตร §13 บูต container จาก PF → SetSP ผ่าน → enable hr[0] → Get/Set ใช้ได้ทั้งระบบ
- §18 ย้าย container ไป build ("containers จาก build genuine" = เกณฑ์ §18.1) → SetSP ตายทุกบูต → enable ไม่มีวันสำเร็จ → server ค้างสถานะ unauthorized → **กำแพง Get/Set E_INVALIDARG ทั้งหมด (m_pSettings)** → Capture/State-เดิม 500 → DesktopCapture/Support/Reason 500 → settings sync ไม่ไหล → หน้าบูตไม่จบ
- **แก้บันทึก §18: สรุป "ValidatePID = confound" ผิดจังหวะ** — การทดลอง §18 ไม่เคยไปถึงขั้น SetSP (attach/enable ไม่ครบ) จึง grep เจอ 0 บรรทัด · วันนี้ enable ถึงขั้นนั้นทุกบูต → error โผล่ตรง ๆ (string `ValidatePIDUnknown executable path` = คำต่อกัน มี substring ตามที่ §13 บันทึก)
- deep-RE แบบ disassembly: ไม่มี objdump/binutils บนเครื่อง — strings-RE ถึงขีดแล้ว แต่**ไม่จำเป็นแล้ว**: root ที่เห็นตรงเป็นทดสอบได้ด้วยการ flip path (ถูกกว่า RE)

**ข้อเสนอ (ทางแท้ — ไม่ patch อะไรเลย — ⏸ รออนุมัติ OWNER ตามกฎ §18):**
- **กลับสู่สูตร §13: nvcontainer (service + SPUser) จาก PF แท้** — node/Share/helper ยังอยู่ build ตาม §8/§17 · แก้ = ImagePath + Watchdog SPUserX64 profile ชี้ PF + ปรับ start-osc (เกณฑ์ §18.1 "containers จาก build genuine" ต้องแก้ตาม) · ทดสอบคาดหวัง: SetSP ผ่าน → enable hr[0] → Get/Set ฟื้น → DesktopCapture/Support/Reason 200 → settings sync ไหล → หน้า boot จนจบ
- ทางเลือกสุดท้าย (ถ้า OWNER ยืนยัน container จาก build): patch whitelist ของ ValidatePID ใน nvcontainer.exe — binary patch ขัด genuine-100% → **ไม่แนะนำ**
- patch [0h] ที่ route JS คงอยู่เหมือนเดิม (interim ตาม §20.2) — ถ้า flip สำเร็จ ค่อยประเมินว่าจะถอดหรือคงไว้

### §20.5 — FLIP สำเร็จ: container แท้จาก PF → SetSP ผ่าน → enable hr[0] → Get/Set ฟื้นครบ (2026-10-04 00:1x — OWNER อนุมัติตามข้อเสนอ §20.4)

**เงื่อนไข 4 ข้อของ OWNER (ผลครบ):**
1. **PF anchor แบบ [0e] ✓** — start-osc.ps1 step **[0i]**: restore `Payload\NvContainer` → `PF\NvContainer` ทีละไฟล์ (Copy-Genuine-Tree) + **mirror-clean** (Remove-Extra-Tree — ลบของแปลกที่ไม่มีใน Payload) + MD5 manifest `Logs\pf-container-manifest.txt` · step **[1d]**: flip idempotent (ImagePath + Watchdog Folder/Container/Parameters — string-replace คำนำหน้า path เดียว คง flag/log path ทั้งหมด)
2. **เกณฑ์ §18.1 แก้ใหม่**: acceptance = **"container แท้จาก PF anchor (provisioned by start-osc [0i]) · node/Share/helper จาก build"** — justification: ValidatePID ทำงานที่ขั้น SetSP (หลักฐาน log คู่ §20.4) · patch whitelist ถูกปฏิเสธตามหลัก genuine-100%
3. **ลำดับทดสอบผ่านเกือบครบ** (หลักฐาน: `Logs\phase1-evidence\log-*-20261004-0017-PF-flip-pass.log`):
   - SetSP ผ่าน: `CServerIpc::SetSP : IN(19724, 5)` — **ไม่มี ValidatePID fail อีกเลย**
   - enable hr[0] สะอาด: `EnableShadowPlay: IN → out hr[0]` ไม่มี failed-to-start / SetSP-failed
   - Get/Set ฟื้น (จาก 0 ตลอดวัน → นับไม่ถ้วน): DesktopCapture/Support/Reason **200 {"support":true}** · 8k60 **200 {"support":false}** (1080 Ti — ถูกต้อง) · **POST OSC/MainView 200** (webcam/mic×3/audioMode/instantReplay ครบ) · **AudioSettings 200 {"systemVolumePercent":100}** · POST /Launch **200** · POST /Osc 200 · hotkey routes 200 ทั้งชุด
   - หน้า boot สด (Share restart, container อุ่น): **chain จบครบ** Capture/State 200 (patch [0h]) → DesktopCapture 200 → MainView 200 → AudioSettings 200 → hotkey/* 200 → 8k60 200 → Osc 200 — ไม่มี 500 ที่เป็นความพังของ stack (เหลือ 500 ที่ = คำตอบถูกต้องเมื่อไม่มีเกม: Capture/ProcessInfo/4294967293 + DeepDVC)
   - ⏳ Alt+Z จริง = รอ OWNER กด (ฉีดคีย์ keybd_event ไม่ผ่าน — helper กรอง injected keys; ตาม §18 Alt+Z จริงเป็นของ OWNER อยู่แล้ว)
4. **กติกาเดิมครบ** — archive ✓ (`log-*-20261004-0017-PF-flip-pass.log`) · ห้าม push ✓ · recovery = -RecoverSpUser ✓

**กับดักใหม่ที่เจอระหว่าง flip (สำคัญ — เคย kill service ทั้งหมด exit 14109):**
- **NVIDIA App ทิ้ง duplicate plugin ที่ PF**: `plugins\LocalSystem\NvMessageBusBroadcast.dll` (ตัวไม่มีขีดล่าง ของ App) + `_NvMessageBusBroadcast.dll` (ตัวแท้) อยู่ร่วมกัน → container โหลดตัว App ก่อน (N ก่อน _) แล้วชนชื่อ plugin ตัวเอง → "Failing loading of plugin ... because of existing plugin 'NvMessageBusBroadcast'" → exit **14109** · fix = **Remove-Extra-Tree** ใน [0i] (mirror ตาม Payload — ของแปลกถูกลบทุกบูต)

**บทเรียน/สถานะค้างเล็ก:**
- **cold-cache first-call timeout**: property บางตัว (hotkey props, 8k60, MainView/Audio data) ครั้งแรกหลัง container เกิดใหม่ server คำนวณเกิน timeout 500ms ของ client IpcSyncCall → 500 หนึ่งครั้ง → ครั้งถัดไป 200 (cache) · ผล = หน้าที่ boot "สดหลัง container ใหม่" อาจชน 500 ครั้งแรกของบาง route — page reload รอบสองผ่านครบ (หรือ warm ด้วย curl ก่อน) — ยังไม่แก้ที่ต้นเหตุ (timeout คือพฤติกรรมแท้)
- Cfg2/Cfg3 ยังไม่ถูกเขียน (reg query ไม่เจอ) — hotkey ผู้ใช้ยังว่าง (`keys: []`); node ใช้ default (openshare [18,88]) — การ SET hotkey จริงจากหน้า = งานถัดไปหลัง OWNER ยืนยัน Alt+Z
- [0h] patch (Capture/State boundary) คงอยู่ตามคำสั่ง OWNER — ไม่เกี่ยวกับ flip (ขอบเขต capture ของ NvCapture.exe ตาม §20)
- **กับดักหลังไฟดับ/reboot เย็น (4 ต.ค. พิสูจน์จริง)**: ต้นไม้ shim เก่า (`build\...\Overlay OSC\NvNode\node.exe`) อาจถูกปลุกตอนบูตและจับ :59001 ก่อน node แท้ (เห็น bare `node.exe` PID 5344 จาก shim tree LISTENING :59001 + Web Helper จาก path เก่า) — **start-osc [3] จัดการเอง** (เห็น path ผิด → kill → สตาร์ต node แท้) — recovery หลังไฟดับ = รัน start-osc รอบเดียวจบ · ยืนยันหลังฟื้น: Launch 200×3 · Capture/State 200 · MainView 200 · SetCaptureSessionParam ไม่มี error (กำแพงไม่กลับมา)

### §20.6 — Alt+Z: ห่วงโซ่ถึง MainView 200 ครบ แต่ชั้นนำเสนอ (presentation layer) ยังไม่วาดลงจอ (2026-10-04 หลังเที่ยง — OWNER กดจริง + ยืนยัน "ไม่เห็น")

**ผลการกดจริงของ OWNER (16:49:31):**
- hotkey → osc spawn ใหม่ → หน้าบูตสะอาดครบ: `POST /Launch 200×3 → Capture/State 200 → POST /OSC/MainView 200` (node log "OSC Main View info: {...}" = หน้าได้ข้อมูลครบ)
- toggle เปิด/ปิด: "should close osc" ตรงจังหวะกดซ้ำ (16:49:01/11/26 ปิด · 16:49:31 เปิดรอบใหม่ · 16:52:32 ปิด)
- node receiver armed (`JOIN Hotkey:Node` ตอน node boot — ไม่ต้อง re-arm เพิ่ม)
- **แต่ OWNER ยืนยัน: ไม่เห็น overlay บนจอ** — และการฉีดคีย์ของ session (keybd_event) ได้แค่ปฏิกิริยาบางส่วน (Launch 200 ไม่มี MainView) + A/B pixel diff = ไม่มีแผงโผล่ (ฉีดไม่เทียบเท่ากดจริง)

**ชั้นที่หาย = ชั้น 4 ของ §10 (native presentation):** overlay object ถูกสร้างแล้ว (`COverlayApi::CreateOverlay: hr[0] usingGPUOverlay[0]` 1680×1050) แต่ไม่มี ShowOverlay/presentation log เลย — ตรงกับที่ §13 จดค้างไว้เอง: "nvspcap64 render in-game integration — ขั้นถัดไป" · ยุค §8-13 ที่ overlay ขึ้นจอ ตัววาดคือสาย shim (NvOverlay/WindowState — "presentation ทำงานเอง DT/offscreen layer") ซึ่งไม่ได้รันในสายแท้ปัจจุบัน

**ตัวเลือกที่มีใน build (ของเรา — เข้า §20 "render เป็นของเรา"):**
- `NvOverlay\Cef\` — OSC overlay CEF host เต็มชุด (NVIDIA Share.exe + **OSC.nvi + OSCExt.dll** + cef runtime) — ไม่มี process รันอยู่
- `Overlay OSC\NVIDIA OSC Native\` — native presenter ต้นไม้ §17 — ไม่มี process รันอยู่
- (หมายเหตุ: GFE-dir Share.json = คอนฟิกยุค §17 (`nv-node-app` ชี้ Overlay OSC\NvNode shim launcher — ต้นตอ shim node จับ :59001 หลังไฟดับ) + `nv-gpu-accel=false` — ตั้งใจตาม §12 anti-spiral; §13 เคยเห็น overlay ด้วย flag เดียวกัน จึงไม่ใช่ตัวปิดกั้นการมองเห็น)

**งานถัดไป (รอ OWNER เลือกทาง):** ต่อชั้นนำเสนอ — (ก) รัน/ต่อ presenter ของเรา (NvOverlay\Cef หรือ NVIDIA OSC Native) ให้รับ toggle จากหน้าแล้ววาด topmost — ตรงสถาปัตยกรรม §20 (render = NvCapture.exe ของเรา) · (ข) ต่อ render attach แท้ของ nvspcap64 (§13 ค้าง) — เสี่ยง spiral §12 ต้องระวัง

### §20.7 — ทาง (ค): mirror คอนฟิกแท้ + ยืนยัน binary แล้ว — ยังไม่วาด (รายงานตามเงื่อนไข OWNER: rect + log ครบ) (2026-10-04 บ่ายแก่)

**1. ตัวตน binary ✓** — NVIDIA Share.exe ที่รัน (GFE dir) = **3,347,496 bytes = genuine 3.3MB** (เท่ากับ NvOverlay\Cef\genuine\ และ Overlay OSC\NVIDIA Share\ ทุกตัว) · bootstrap 1MB (NvOverlay\Cef\) ไม่ได้รัน · ที่มาของ Share = `$ShareExe = build\...\Overlay OSC\NVIDIA Share\` ([0f] hardlink → GFE dir)

**2. Share.json 3 ฉบับ + mirror ✓ (⚠ discrepancy ต้องรายงาน):**
- **genuine harvest** (NvOverlay\Cef\genuine\ 353B): `nv-osc=true · nv-gpu-accel=FALSE · nv-url-relative=osc/index.html · nv-node-app=NvNode\nvnodejslauncher.exe (relative ใน layout แท้) · nv-node-data · nv-plugin-* · nv-remote-debugging-port=9222`
- **ข้อขัดแย้งกับคาดหมาย OWNER**: ฉบับแท้ระบุ `nv-gpu-accel=false` ไม่ใช่ true — ตามกติกา "เทียบไม่ตรง = รายงาน + ตามฉบับแท้" ผม**คง false** (§12 ก็ตั้ง false กัน spiral มาก่อน) — ถ้า OWNER ต้องการ true แม้ฉบับแท้เป็น false = registered deviation ต้องชัดเจน
- **สิ่งที่แก้จริง**: ตัด absolute shim `nv-node-app` ทิ้ง (ต้นตอ shim node จับ :59001) → mirror ฉบับแท้ byte-for-byte ทั้ง build tree + ตำแหน่งรัน (GFE dir) — relative path ตายใน layout เรา = ไม่มีวัน spawn → **MMF pairing กับ node แท้ตาม fallback ของ OWNER** + ได้ `:9222` (CEF remote debugging — เครื่องมือใหม่ล้ำค่า)
- backup คอนฟิกเดิม: `patch-backup\NVIDIA Share.json.{build,gfe-dir}.pre-genuine-mirror`
- **[0f.1] เพิ่มใน start-osc.ps1**: force-mirror Share.json build → GFE dir ทุกบูต (เดิม hardlink loop ข้ามไฟล์ที่มีอยู่ — คอนฟิกไม่เคยอัปเดต) — ลงทะเบียนตามนโยบาย genuine + registered patches

**3. ผลทดสอบ Alt+Z รอบใหม่ (CDP ยิง `Hotkey_OSC` เข้าหน้าตรง + กดจริงของ OWNER 16:49):**
- หน้าตอบสนอง handler ทุกครั้ง แต่ route ค้าง `#/base` — ไม่เคยผ่าน `$viewContentLoaded → allowOSCPainting(true)` (ประตูวาดของหน้าเอง)
- **window rect ระหว่างยิง (EnumWindows, pid 4416)**: `NVIDIA GeForce Overlay` = vis=**True** (0,0)-(1680,1050) · `NVIDIA GeForce Overlay DT` = vis=**False** (0,0)-(1680,1050) ← พื้นผิวนำเสนอ (ชั้น DT §10) ซ่อน · CEF browser window = vis=False (156,156)-(1416,887)
- **close-flood**: "should close osc" 22 ครั้ง/นาที ตอน 17:18:27 (หลัง restart Share ด้วยคอนฟิกใหม่) แล้วเป็นคู่ ๆ ตามจังหวะ toggle = osc ถูกสร้างแล้วถูกปิดวนลูป — ไม่ใช่ page reload (socket connected = 2 ครั้งทั้งวัน)
- ไม่มี ShowOverlay/OpenShare/NotifyOverlayState log แม้แต่บรรทัดเดียวทั้งวัน

**4. สรุปทางเทคนิค:** ทุกชั้นบน (helper→node→page→MainView) ผ่าน · หน้าต่าง overlay มีอยู่จริงและ vis=True · พื้นผิว DT ซ่อน + หน้าไม่ยอม allowOSCPainting + ถูก close-flood รังควาน = การวาดไม่เคยเริ่ม · คอนฟิก/binary แท้ยังไม่พอ — ตัวขับ DT/presentation (ชั้น 4) คือชิ้นที่ขาดจริง ๆ (เดิมเป็นหน้าที่ของสาย shim)

**5. เครื่องมือใหม่ที่ได้จากคอนฟิกแท้:** `:9222` CDP — อ่าน/สั่งหน้า osc ได้ตรง (eval JS, ดู hash, ยิง event) — ใช้ต่อได้ทุกงาน

### §20.8 — 🏆 PAINT สำเร็จผ่าน NvPlugins/DulukaPort topology — hotkey ตัวจริงคือ OscHotkey ไม่ใช่ nvsphelper64 (2026-10-04 ค่ำ — OWNER ทดสอบใน NvPlugins GUI แล้วยืนยัน "ติดแล้ว")

**ผลทดสอบ (log + pixel จริง):**
- **paint ยืนยัน**: screenshot diff เทียบ baseline จอเปล่า = **51,024 sampled pixels เปลี่ยนทั่วจอ** (0,0)-(1676,1048) — genuine chain ตอนล้มเหลว = 37 pixels (noise)
- **ต้นสาย hotkey ที่จริง**: `dulukaport.out` = `[DulukaPort] Alt+Z -> POST /?hk=OpenShare → toggle -> 200` ซ้ำ — **DulukaPort OscHotkey (in-app hook) จับ Alt+Z เอง → fire :59002/?hk=OpenShare → overlayToggle → หน้าเปิดเอง** — nvsphelper64 WMHK → MessageBus → node receiver ไม่เคยพา overlay ไปถึงจอ (ชั้น 4 ตายมาตลอด)
- **topology ที่ติด**: NvPlugins.exe (OscHotkey) + shim node :59001 (Custom Data) + :59002 fireServer + **Share.exe จาก build tree** (`Overlay OSC\NVIDIA Share\` — pid 12992: :9222 LISTENING + ต่อ :59001 ESTABLISHED หลายช่อง + Overlay window vis=True) — nvcontainer SPUser/User จาก PF ยังรัน
- BootGenuine (18:54) ทำหน้าที่ของมันครบ: kill shim node ค้าง + re-arm Launch 200 — แต่การ paint เกิดผ่านสาย Custom

**diff ตัวการ (จาก 4 ข้อที่สงสัย):**
| จุด | start-osc (genuine chain) | NvPlugins Custom (WORKS) |
|---|---|---|
| ตัวจับ Alt+Z | nvsphelper64 WMHK → MessageBus → node receiver (ไม่เคยถึงหน้า) | **DulukaPort OscHotkey → POST OpenShare → 200 ทันที** |
| node | genuine node :59001 | shim node :59001 + :59002 fireServer |
| การเปิด | รอ page handler ผ่าน socket (ไม่มีวันมา) | **overlayToggle ยิงเข้าหน้าตรง — เปิดเอง** |
| ผล DT | DT vis=False + close-flood + route ค้าง #/base | **หน้าเปิด + paint 51k pixels** |

**ข้อสรุปสถาปัตยกรรม (ต่อยอด §20 + Phase 2):** หัวใจที่หายของสายแท้ = **ตัวจับ Alt+Z + สั่งเปิดแบบตรง (OpenShare fire)** — หน้าต่าง/พื้นผิววาด (Overlay window + DT) ของ Share แท้ใช้ได้อยู่แล้ว เมื่อหน้า "เปิด" ถูกต้อง · **NvPlugins เป็น supervisor** ตามแผน Phase 2: OscHotkey = ชั้น hotkey, OpenOsc = ชั่นสั่งเปิด, node ฝั่งไหนตอบ Data ค่อยรวมตาม §20 (OSC แท้ 100% — shim node เป็น interim ตอบ Data)

**สถานะเครื่องหลังทดสอบ:** ทิ้ง topology ที่ติดไว้ตามที่ OWNER ใช้งานอยู่ (ไม่ kill อะไร) · archive: poll logs run1-run5 + dulukaport.out อยู่ที่ตำแหน่งเดิม · ไฟล์ Intel harvest ยังรอผลเสริม (ไม่บล็อกแล้ว — ทางตายเดินได้จริงแล้ว)

### §20.9 — Registered BINARY patch: ValidatePID whitelist (OWNER อนุมัติ ยกเลิกมติเดิม — 2026-10-04 ค่ำ)

**ไฟล์เป้าหมาย (2 สำเนา, Payload ห้ามแตะ):** `build\NvContainer\genuine\plugins\LocalSystem\ShadowPlay\_nvspserviceplugin64.dll` (1,957,416 B) + `PF\NvContainer\plugins\LocalSystem\ShadowPlay\_nvspserviceplugin64.dll` (run location)

**ผล RE (ก่อน patch — หลักฐานจาก strings + PE xref scan):**
- `ShadowPlayController::ValidatePID` ตรวจ **path ของ CALLER ของ SetSP = process container SPUser เอง** (log พิมพ์ caller exe path ตอน fail) — ไม่ใช่ Share/helper
- โครงสร้าง: build candidate path array (2 รายการ = PF x64/x86) จาก literal `NVIDIA Corporation` (UTF-16 @file 0x168020, VA 0x180168C20, xref LEA @0x33541) + SHGetKnownFolderPath + `%s\%s` → วนเทียบ (compare helper call) → `test eax,eax; jns` (0x3357F) → compose expected → compare รอบสุดท้าย → `test rax,rax; jne` (0x33600) = verdict → ไม่ผ่าน = log `Unknown executable path %S` (ANSI @0x168081)
- whitelist = **branch compare บน composed path ที่ anchor ด้วย KnownFolder (Program Files)** — string-patch เดี่ยวไม่พอ (build path ไม่มี "C:\Program Files" prefix) → **branch patch 2 bytes**

**Patch (2 bytes — ทั้งสองอยู่บนเส้นทางรันปกติของกรณี PF ผ่าน จึง pointer-safe):**
| offset (file) | เดิม | ใหม่ | ความหมาย |
|---|---|---|---|
| 0x3357F | 79 (jns +2E) | EB (jmp) | บังคับเข้าสาย compose/success เสมอ (ไม่สนผล candidate match) |
| 0x33600 | 75 (jne +2E) | EB (jmp) | verdict = success เสมอ |

- md5 หลัง patch (build copy): `D1DAC81FEEF2D9649E698591DABFC21C`
- patcher: `build\NVIDIA ShadowPlay\Runtime\apply-validatepid-patch.ps1` (idempotent — locate ด้วย unique byte-pattern `4C 8D 4C 24 30 48 03 C8 45 33 C0 33 D2 E8` + `48 85 C0 75 2E` + distance assert; เจอของแปลก = FAIL ไม่เขียน)
- re-apply ทุกบูต: start-osc step **[0j]** (เพราะ [0] stage + [0i] mirror จะ restore จาก Payload ทับทุกครั้ง)
- revert: copy `_nvspserviceplugin64.dll` จาก `Payload\NvContainer\plugins\LocalSystem\ShadowPlay\` ทับ (แล้ว restart service)

**ผลตรวจหลัง patch + flip container กลับ build:**
- ✓ container รันจาก build path (`build\NvContainer\genuine\nvcontainer.exe` — service/SPUser/User ครบ)
- ✓ **ValidatePID fail = 0** ตั้งแต่ patch (ไม่มี "Unknown executable path" อีก)
- ✓ Launch 200 / MainView 200 / DesktopCapture 200 (ผ่าน topology Custom ที่คุณใช้ — shim node :59001)
- ⏳ "SetSP ผ่าน end-to-end" ยังพิสูจน์ไม่จบ: auto-enable trigger (rundll32 origin(7)) ใช้ retry 5 ครั้งหมดตั้งแต่ 19:06 (ตอน FullPath ยังชี้ path ตาย — แก้เป็น `build\...\Overlay OSC\NVIDIA Share\NVIDIA Share.exe` แล้ว) และไม่ยอม respawn หลัง service restart หลายรอบ = ปม trigger แยกต่างหาก (ไม่ใช่ patch) — จะปิดพร้อมงานรวมเส้นทางเดียว Phase 2 (การกลับมาของ genuine node :59001 จะพา enable กลับมาเอง)
- แยกชัดตามคำสั่ง OWNER: patch นี้ = ปลด container anchor เท่านั้น — ปม route ค้างของหน้า (Alt+Z → CDP) ยังเป็นงานต่างหาก
- **หมุดสถานะ (OWNER สั่งจด): "patch installed · end-to-end proof pending trigger lifecycle"** — ValidatePID fail=0 ตอนนี้ยังเป็นหลักฐานอ่อน (SetSP ยังไม่ถูกเรียกเลยหลัง patch เพราะ trigger ตายก่อน) — จะปิดเต็มเมื่อ enable ยิงใหม่ (หลังสลับ topology) และ SetSP ผ่านจริง
- **อัปเดตหลัง OWNER แก้ไขทิศทาง (23:2x): patch = registered-DORMANT** — revert plugin DLL จาก Payload ทั้ง 2 สำเนาแล้ว (signature กลับมาสมบูรณ์) + container กลับ PF ตามที่ OWNER ยืนยัน topology ที่ "ติด" · patch ยังลงทะเบียน/มี patcher/[0j] ค้างไว้เป็น option A (ต้องแก้ signature-verify ชั้น 2 ก่อนจึงใช้ได้จริง)
- **blocker ปัจจุบันของ enable (เปลี่ยนจาก ValidatePID แล้ว): "failed to start Share Process!" 0x80040233** — enable attempt ใช้เวลารอ Share ~2 วิ/ครั้ง แต่ Share ใหม่บูตตัวเอง ~20 วิ (CEF) → attempt 5 ครั้งหมดก่อน Share attach (พิสูจน์: Share 22864 join bus 23:23:39 หลัง attempts หมดที่ 23:22:46) · แม้ Share attach แล้วและยิง enable ใหม่ (23:23:42, 23:33) ก็ยัง fail = กลไก detect ของ container ไม่ใช่แค่ "มี process Share" (ต้องศึกษาเงื่อนไข detect เพิ่ม — งานถัดไป) · §20.5 เคยผ่านสะอาดเมื่อ Share attach **ก่อน** container เกิด (ลำดับ attach-mode แท้)
- สถานะเครื่อง: containers จาก PF (genuine plugin, ServicePlugin บน bus ✓) · genuine node 7060 (:59001) · Share ×2 build-tree attach แล้ว · page poll อยู่ (Get/Set 500 = m_pSettings ว่าง — จะหายเมื่อ enable ผ่าน)

### §20.10 — รอบสลับ topology + 2 การค้นพบใหญ่ (2026-10-04 ค่ำแก่ — รอบเดียวตามแผน OWNER)

**1. genuine node ขึ้นได้แล้ว (pid 7060 ครอง :59001) — และสาเหตุที่เคยตาย:**
- node แท้ตายตอน start ด้วย `NVIDIA Web Helper file already exists in this session` (uncaughtException → shutdown สะอาด)
- RE กลไก (strings NvUtil.node): `ClaimSingleInstance` = CreateEvent `{23FC14F6-CBB0-400C-AB0D-D94A864ED2B7}` + **CreateFileMappingW `{8BA1E16C-FC54-4595-9782-E370A5FBE8DA}`** — mapping ยังอยู่ = ตายทันที
- **ผู้ถือ mapping ตัวจริง = NvPlugins GUI (OscHotkey "pairing MMF + event")** — ตายยากเพราะ Share ที่ container เลี้ยง + GUI ถือ handle ค้าง · **นัยสำคัญ Phase 2: NvPlugins กับ node แท้กันเองบน pairing MMF — §21 ต้องให้ OscHotkey เปิด mapping แบบ open-by-name/ปล่อยเมื่อโหมด genuine**
- ลำดับที่ผ่าน (หลัง kill NvPlugins + Share + node เก่า): **start node ก่อน → :59001 200 → start service** (สูตร §8.3 ที่แท้จริง)

**2. Patched plugin ถูก container ปฏิเสธเพราะ SIGNATURE (จุดตัดสินใหม่ — รอ OWNER):**
- หลักฐาน: service container log (เวลาใน log = **UTC** — 13:19 UTC = 20:19 ท้องถิ่น บูตปัจจุบัน): `Failed to load library ...build..._nvspserviceplugin64.dll. Error 2148098064 (กลุ่ม 0x8009 = CERT/TRUST)` → **Secure loading ตรวจ signature plugin → 2-byte patch ทำ signature พัง → ShadowPlayServicePlugin ไม่ถูกโหลด → "ServicePlugin" หายจาก bus (เทียบ peers boot แท้) → ข้อความ enable/SetSP จาก node หมดเวลา (0x800705b4)** — ValidatePID ยังไม่ได้ถูกเรียกจริง (หมุดอ่อน §20.9 ยืนยันตัวเอง)
- สิ่งที่ยังทำงาน: ShadowplayServer (SPUser, plugin ไม่ได้ patch) อยู่บน bus → MainView/CaptureState ผ่าน (cold-cache ต้องยิงซ้ำหลัง container ใหม่) · genuine node + Launch route พร้อม
- `-safemode` flag ทดสอบแล้ว = service ไม่ขึ้นเลย (ไม่ใช่ทาง) — ถอนแล้ว
- **ทางเลือกรอ OWNER ตัดสิน:**
  - **(A) binary patch ชั้นที่ 2** — ปิด branch signature-verify ของ Secure loader ใน nvcontainer.exe (WinVerifyTrust path) ให้ plugin (ที่ patch แล้ว) โหลดได้ → ต่อสาย SetSP จบ §20.9 — ต้องอนุมัติ (แตะ container core)
  - **(B) ถอน patch + กลับสูตร PF (§20.5)** — chain แท้สมบูรณ์แบบพิสูจน์แล้ว (SetSP ผ่าน/enable hr[0]/Get/Set ฟื้น) ยกเว้นข้อเดียว: container ต้องอยู่ PF — ปม build-path จบด้วย "ยอมแพ้ PF anchor" + OscHotkey (NvPlugins) ยังเป็นชั้น hotkey/open
  - **(C) hybrid**: container จาก PF (plugin signature สมบูรณ์) + คง patch ไว้เฉพาะไฟล์ build (ไม่ถูกโหลด) เผื่ออนาคต

**3. สถานะเครื่อง (ณ รายงาน):** genuine node 7060 (:59001) · service+containers ×3 จาก build (patched, ServicePlugin ไม่โหลด) · Share ไม่รัน (ถูก kill ใน cycle) · OscHotkey/NvPlugins ถูก kill (BootCustom เปิดคืนได้) · FullPath registry = build Overlay OSC Share ถูกต้องแล้ว

### §21 — Phase 2 spec: รวม boot logic เข้า NvPlugins (supervisor) — OWNER กำหนดหลักการ (2026-10-04)

**หลักการ 3 ข้อ:** modular (แยก step ชัดแต่ละอันตรวจ/รันเดี่ยวได้) · transparent (ทุก step log อ่านได้ ตรวจสอบได้) · trustworthy (self-heal + rollback ทุก step)

**โครงสร้างที่ต้องมี:**
1. **แยก step** — boot = ลำดับ step ย่อยที่ประกาศชัด (service → PF anchor → registered patches → Share → node → helper → re-arm) แต่ละ step มีเงื่อนไขผ่าน/ไม่ผ่านของตัวเอง
2. **status command** — `NvPlugins.exe status` = รายงานสถานะทุก step ปัจจุบัน (process ไหนรันจาก path ไหน, port, registry, patch marker) โดยไม่แตะอะไร
3. **self-heal** — step ไหนพัง = ซ่อมเฉพาะ step นั้นจากแหล่งจริง (Payload/patch registry) ไม่ restart ทั้งชุด
4. **rollback** — ทุก patch/flip ต้องมีทางย้อน (revert = copy จาก Payload, flip = สลับกลับได้) และ rollback ต้องไม่พังทีละหลาย step

**สถาปัตยกรรมเป้าหมายปลายทาง (ต่อ §20):** NvPlugins = supervisor (OscHotkey ชั้น hotkey + OpenOsc ชั้นสั่งเปิด) · containers จาก build (patched) · node แท้ครอง :59001 · Share จาก build tree · shim node = interim ตอบ Data จน node แท้/NvCapture รับงานครบ

**สถานะ:** spec จดแล้ว — implementation = งานถัดไปหลังปิดรอบ CDP/§20.9 นี้
