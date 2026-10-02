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
