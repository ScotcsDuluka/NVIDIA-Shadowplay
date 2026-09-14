# 15 — Overlay Engine Handoff (NVIDIA osc on WebView2)

สถานะ ณ 2026-09-14 07:58 — เอกสารส่งมอบให้ agent ตัวถัดไป — **อ่านไฟล์เดียวจบ ต่อได้เลย**
เอกสารประกอบ: `docs/osc/01–14` (inventory/protocol/state/screen/flow), `Overlay.Engine/PROTOCOL-MATRIX.md`
(ground-truth contract + file:line evidence), `Overlay.Engine/README.md`, รายงาน `W1-*.md`
(หมายเหตุ: README ของ engine บางย่อหน้าล้าสมัย — ยึดเอกสารนี้กับโค้ดจริงเป็นหลัก)

---

## -1. อัปเดตล่าสุด (07:58 วันเดียวกัน) — งาน in-game hook ครบวงจรแล้ว

1. **CDP publish = เสร็จแล้ว (พิสูจน์จริง)**: `HookCdpCapture` วน
   `Page.captureScreenshot` → PNG → `PublishFrame` ลง MMF ต่อเนื่อง
   (`hookcdp.log`: "published frame #N 1680x1050", เปิด/ปิดตาม `CaptureEnabled`,
   log ครบทุกจุดแล้ว — send-fail / no-response / decode ทั้งหมดถูกบันทึก)
2. **Port+secret ใน MMF header**: +28 = controller port (int32),
   +32..+62 = secret ASCII NUL-terminated — DLL อ่านแล้วยิง POST เองได้
   โดยไม่ต้องมีไฟล์ใดๆ ในโฟลเดอร์เกม
3. **Input forwarding = เสร็จแล้ว (พิสูจน์จริง)**: `InputThread` ใน nvhook.cpp
   วน 60Hz — อ่าน visible/port/secret จาก header → ส่ง mousemove/mousedown/mouseup
   เมื่อ cursor เปลี่ยน (WinHTTP, cookie `X_LOCAL_SECURITY_COOKIE`) →
   `POST /ShadowPlay/v.1.0/Hook/Input` → `OscControllerServer.HookInput` →
   `OscHostForm.OnHookInput` → synthetic DOM events (engine log นับ 25+ POST ระหว่างทดสอบ)
4. **DLL วาด overlay จริงแล้ว** (ไม่ใช่ bisect เขียว): pixel shader sample
   texture + blend SRC_ALPHA/INV_SRC_ALPHA — `BuildResources ok (1680x1050)`,
   `Draw(3,0) executed`, live counter (+20) วิ่งทุก Present
5. **Alt+Z toggle ในเกม 2 ทิศทาง** (header flag เท่านั้น เกมไม่เสียโฟกัส): ปิด =
   `capture disabled` + DLL หยุดวาด / เปิด = `capture ENABLED` + publish ต่อ
6. **แก้ไฟล์ในเกม (กฎเหล็ก)**: session เก่าเขียน `NvidiaShareHook.json`
   (port+secret) ลงโฟลเดอร์เกมเพราะ `wantBridge` default True — แก้เป็น
   **opt-in only** (`bridgeFile:true` เท่านั้น) และลบไฟล์ทิ้งแล้ว —
   โฟลเดอร์เกมกลับมาเป็นศูนย์ไฟล์ของเรา (engine = memory only)
7. **กับดักใหม่ §6.21-24**: Dungeons.exe ตัวจริงอยู่
   `E:\SteamLibrary\Steamapps\Common\Minecraft Dungeons\Dungeons\Binaries\Win64\`
   (ตัวที่ root = launcher stub ค้าง 2 threads) — launch ตรงได้เมื่อ Steam รันอยู่;
   `%TEMP%\osc-golden\` โดนลับระหว่างทาง (สร้าง altz2.ps1 ใหม่แล้ว)
8. **(08:50) วาดในเกมจริงครบวงจร — เห็นทั้งบนจอและใน PrintWindow/taskbar
   thumbnail แล้ว** ต้องแก้กับดัก 4 ตัวก่อนถึงสำเร็จ:
   - **§6.21 TopMost band**: ใน in-game branch ห้ามคง `TopMost=True` —
     HWND_BOTTOM ใน topmost band ยังลอยเหนือเกม (normal band) เสมอ =
     WebView บังเกม + thumbnail เห็นเฟรมเกมเปล่า → `TopMost=False` ก่อน
     HWND_BOTTOM เสมอ
   - **§6.22 orphaned MMF**: engine restart ทิ้ง section เก่าที่ DLL ยังถือ
     (เป็น unnamed orphan) — logic เทียบ PID ใน header เดิมไม่มีวัน trigger
     → เพิ่ม **epoch @+56** (engine เขียนตอนสร้าง) + DLL poll ชื่อ section
     ทุก 500ms สลับเมื่อ epoch ต่าง (`ReopenIfEngineRestarted`)
   - **§6.23 vtable slot ผิด = crash**: `IDXGISwapChain1::Present1` = **slot 14**
     (base ครอง 0..13; slot 12 คือ GetFrameStatistics — patch ผิดแล้วเกม
     crash ทันที วาดกลาง frame)
   - **§6.24 interface หลายชั้น**: เกม QI เป็น SwapChain2/3/4 แล้วเรียก
     Present บน vtable ก้อนนั้น — ต้อง patch slot 8+14 บนทุก interface
     (base+1+2+3+4) หลักฐานสำเร็จ: `draw #1200` @60fps + เกมเสถียร +
     PrintWindow เห็นเมนูในเฟรมเกม
   - **§6.25 vertex triangle ผิดทิศ = วาดแต่ล่องหน**: SV_VertexID
     fullscreen triangle ต้องเป็น `(-1,-1),(3,-1),(-1,3)` — แบบเดิม
     `(3,3),(-1,3),(3,-1)` ครอบแค่ x+y>2 (มุมจอจุดเดียว) draw รัน @60fps
     แต่มองไม่เห็นเลย (บั๊กตัวสุดท้ายที่ทำให้ "วาดแล้วแต่จอสะอาด")
   - **พิสูจน์ A/B สำเร็จ (09:03)**: vis=1 เฟรมเกมมีเมนู / vis=0 สะอาด —
     PrintWindow(เกม) = แหล่งเดียวกับ taskbar thumbnail ยืนยันว่า overlay
     ผสมเข้าเฟรมเกมจริง แบบ GFE แท้ (ไม่ใช่หน้าต่างลอย) — เจ้าของยืนยัน
     เห็นบนจอแล้ว
9. **(09:55) Auto-inject + FPS bump**: `HookAutoInject.vb` — engine เฝ้า
   whitelist (แยกชื่อ process จาก "exe" โดยตัด .exe — GetProcessesByName
   ไม่รับนามสกุล, บั๊กแรกที่ทำใject เงียบ) แล้ว inject DLL เองทาง memory
   (DLL อยู่ `<runtime>\Hooks\`, ศูนย์ไฟล์ในเกม); CDP loop sleep 200→30ms
   + `optimizeForSpeed` = **3.7 → 10.5fps**; engine restart กลางเกม:
   DLL สลับ section เอง (epoch) + input วิ่งต่อ — พิสูจน์ครบ:
   "injected into Dungeons (pid 14624)" + draw #1 + vis=1
10. **(09:59) คีย์บอร์ด forwarding เสร็จ**: DLL poll GetAsyncKeyState
    (transition เท่านั้น, เคลียร์ state ตอน overlay ปิด — ไม่มี stuck key,
    ไม่ log ค่าคีย์ลง disk) → POST {type:keydown|keyup,vk,shift,ctrl} →
    `VkToJsKey` แปลง VK → JS key name → KeyboardEvent บน document;
    ไม่ส่ง Win/IME/mouse VK พิสูจน์ end-to-end ด้วยพฤติกรรมจริง:
    กด Escape ในเกม → เมนู osc ปิดเอง (capture disabled ทันที)
11. **(10:05) คลิ้อทะลุ → ใช้ได้จริง**: อาการ "กดแล้วทะลุ" มี 2 ต้นตอ
    - **Angular ผูก (click)/(pointer) — synthetic mousedown+mouseup
      อย่างเดียวไม่เกิด click event** → engine ตอนนี้ dispatch ครบ
      sequence: pointerdown+mousedown / pointerup+mouseup+**click**
      (cancelable:true ด้วย)
    - **เกมยังรับ click ตรงๆ** → DLL subclass WndProc เกม (retry จนกว่า
      window เกิด) กลืน WM_MOUSE*, WM_KEY*, WM_INPUT ตอน overlay เปิด
      (GetAsyncKeyState อ่าน system state — input forwarding ไม่กระทบ)
    พิสูจน์: คลิ้อ tile Instant Replay ในเกม → **หน้า settings Instant
    Replay เปิดจริง** (sliders เวลา/คุณภาพ โชว์ในเฟรมเกม)



## 0. TL;DR — ตอบคำถาม "ทำไม Hotkey/Settings/Video ยังใช้ไม่ได้"

แยกเป็น 2 ชั้น:

- **ชั้นเซฟ (สำเร็จแล้ว)**: หน้า UI ทุกหมวด ปรับค่า → POST → เก็บถาวรใน `Data\osc-settings.json`
  → เปิดใหม่ค่ายังอยู่ (หลักฐาน: ไฟล์จริงวันนี้มี **34 sections / 25 รายการ `hotkey:*`**
  ที่หน้า Keyboard-shortcuts เซฟเอง + audio/audioSettings/indicator:record|fps|viewer/broadcast*/desktopCapture/instantReplay)
- **ชั้น "ลงมือทำ" (ทยอยเสร็จ บางส่วนยังไม่มี)** — worklist หลักอยู่ §4:
  - **rebind hotkey จริง = เสร็จแล้ว** (ไฟล์ใหม่ `OscHotkeyApplier.vb`: ค่าที่เซฟ → `RegisterHotKey` จริงทุกครั้งที่กด save;
    action จริงแล้ว — Screenshot ได้ไฟล์ PNG จริง (พิสูจน์แล้ว), RecordToggle ผ่าน TCP, เปิดหน้า Ansel preview — ที่เหลือยัง toast)
  - **Record/Settings ⇆ engine config = เสร็จแล้ว (2026-09-14 ล่าสุด)**: GET อ่านจาก `config.json → Recording.current`
    (framerate/bitrateBps/resolution ตอนนี้ตอบจริง ไม่ใช่ {}), POST → เขียน `Recording.current.{fps,bitrate(kbps),width,height,use_native_resolution}`
    — engine อ่านเองตอน record start (`ApplyUnifiedToCaptureSettings`); option lists `/Resolutions` `/FrameRates` `/BitRates/:q/:r` ให้ครบ
    (บั๊กต้นตอ: `Case "/Record/Settings"` = dead code ตั้งแต่ M1 — path จริงต้องเป็น `/ShadowPlay/v.1.0/Record/Settings`, ดู §6.16)
  - **Audio devices จริง = เสร็จแล้ว**: `/Microphone/{i}/Settings` คืนชื่อไมค์จริงจาก WinMM (`waveInGetDevCapsW` — struct ต้อง `CharSet.Unicode` + `wReserved1`, ดู §6.18); `/AudioSettings` = {systemVolumePercent,separateTracks}
  - **Ansel panel events = ทำแล้วยังไม่เวิร์ก**: PushEvent `{type:gameResolution|highResResolutions}` บน channel `/NvCamera/v.1.0/Notifications` (channel จริงจาก NvCameraAPI.js `io.emit`) แล้วแต่ panel ยัง 0x0 — ต้องตาม registration flow `Le()`/`g.register` (ดูข้อ 3)
  - OSD → ยังไม่มี surface วาดจริง

ไฟล์ NVIDIA (`C:\Program Files\NVIDIA Corporation\…` รวมถึงโฟลเดอร์ `ห้ามแตะห้ามใช้\`) **ไม่ถูกแตะแม้แต่ไฟล์เดียว** — ทุกอย่างเป็น source/Content ของเรา

## 1. สถาปัตยกรรม

```
NVIDIA Overlay Engine.exe (WinForms, net10.0-windows10.0.26100.0, PerMonitorV2)
│
├─ OscHostForm ............ หน้าต่าง TopMost เต็มจอหลัก (TOOLWINDOW, ไม่อยู่ Alt-Tab)
│    ปิด = HTTRANSPARENT + park ใต้จอหลัก / เปิด = HTCLIENT ทั้งหน้า
│    └─ WebView2 (transparent) โหลด NVIDIA osc แท้: http://localhost:<port>/index.html
│         (Angular bundle ใน Overlay/osc — PRODUCTION = แท้เท่านั้น, /next/ ถูกลบถาวร)
├─ OscControllerServer .... ลูปแบ็กพอร์ตสุ่ม + secret สุ่มทุก boot; 2 โปรโตคอลพอร์ตเดียว:
│    (1) static osc + REST subset  (2) engine.io v3 polling + socket.io v2 (golden-verified)
│    FULL MODE: GET ตอบจาก responses\ (146 endpoint จับจาก GFE จริง) ก่อน fallback handler
│    settings store ถาวร: <runtime>\Data\osc-settings.json (POST = merge verbatim, GET = คืนค่าที่เซฟ)
├─ CefQueryBridge ......... ฝั่ง host ของ window.cefQuery (polyfill ฉีดก่อน page script ทุกตัว)
│    + unlock shim (nvCameraService) + host backdrop div + WinFullscreen probe
├─ OscHotkeys ............. Alt+Z toggle เฉพาะเมื่อ "ไม่มี GFE" (first-come-first-served)
├─ OscHotkeyApplier ....... ผู้บริโภค settings store: hotkey:{name} → RegisterHotKey จริง (id≥100)
│    re-apply ทุกครั้งที่หน้าเซฟ (event HotkeySaved) และตอน boot
├─ OscEngineClient ........ TCP hub :5001 (NVIDIA API.exe) — RECORD_START/STOP, open_overlay,
│    engine_ready/state_changed/progress/saved/error/RecordStartedConfirmed/RecordFailed
├─ EngineProcessSupervisor (linked จาก Forms overlay) — ปลุก NVIDIA Capture.exe เอง
├─ NvShadowPlayRecorder (linked จาก Engine) — GFE detector + inject Alt+F9 + tray "Toggle GFE overlay"
└─ Tray icon — toggle / inject Alt+Z ให้ overlay จริง / Exit
```

- **เครื่องนี้ (ผลตรวจจริง)**: GFE ไม่ถูกตรวจพบ → engine เข้า branch "GFE not installed" และ**เป็นเจ้าของ Alt+Z เอง**;
  ถ้าเครื่องไหนมี GFE → launch `NVIDIA Share.exe` ให้แล้วยก Alt+Z ให้ overlay จริง (เราเหลือ tray/hub)
- **รันจริงจาก** `C:\Users\ScotcsDuluka\Downloads\ShadowPlay.v1\` (staging จาก
  `Overlay.Engine/bin/Release/net10.0-windows10.0.26100.0` — sync ต้อง exclude `Logs\` และ `Data\`, ดูกับดัก §6.12)
- `Data\`: `osc-settings.json` (settings store), `osc-shared-storage.json` (cefQuery KV), `WebView2-osc\` (UDF)
- `Logs\overlay-engine.log` — **บรรทัด `controller server on http://127.0.0.1:<port> secret=<secret>` = ตัวต่อ REST/socket ด้วยมือ**
- osc root resolve ลำดับ: `Overlay/osc` ขึ้น 4 ชั้นจาก exe → `..\osc` → `osc` ข้าง exe
- ต้นแบบ shape ทุก endpoint (เทียบได้): `C:\Users\ScotcsDuluka\Downloads\GeForce_Experience_v3.28.0.412\nodejs\`
  (`index.js` router, `NvShadowPlayAPI.js`, `NvCameraAPI.js`, `NvPiplConfig.js` — POST เก็บผ่าน native store,
  เราเลียนแบบด้วย JSON store) + `docs/osc/real-controller-responses/`

## 2. ไฟล์ที่ patch ทั้งหมด (ของเราทั้งหมด — ไม่มีไฟล์ NVIDIA ถูกแก้)

| ไฟล์ | การแก้ |
|---|---|
| `Overlay.Engine/Program.vb` | mutex `Global\NVIDIA_Shadowplay_OscEngine_SingleInstance`; ApplicationContext เปล่า — **ฟอร์ม new แล้วห้าม auto-show** (Show ครั้งแรกเกิดตอน toggle — แก้บั๊ก Chromium ไปเกาะจอ portrait ตอน park -32000); UI/unhandled exception ลง log ทุกครั้ง; `AppLayout.Initialize()` ก่อนทุกอย่าง |
| `Overlay.Engine/OscHostForm.vb` | ctor เรียก `Start()` เอง; GFE ตรวจพบ → launch `NVIDIA Share.exe` + ให้ overlay จริงเป็นเจ้าของ Alt+Z / ไม่มี GFE → `OscHotkeys` จด Alt+Z เอง; park ใต้จอหลัก (`b.Bottom+40` ไม่ใช่ -32000); open ครั้งแรก = `Show()`; ZoomFactor = `min(W/1920,H/1080)` ตั้งตอนสร้างครั้งเดียว; navigate same-origin `/index.html`; toggle debounce 500ms; closed-loop push `WindowState overlayToggle/dismiss` ×4 + เช็ค `location.hash` จนหน้า converge; WndProc: ปิด=HTTRANSPARENT, เปิด=HTCLIENT (**ห้าม branch displayRects ตอนเปิด**); screenshot จริง; shutdown ครบ (unregister → supervisor → server → webview → `Environment.Exit(0)`) |
| `Overlay.Engine/CefQueryBridge.vb` | `QUERY_OSC_DISPLAY_IS_DESKTOP_MODE` → "true" (เมนูเต็ม ไม่ใช่ sidebar); polyfill `window.cefQuery/cefQueryCancel/__cefDeliver` (ผ่าน `chrome.webview.postMessage`) ฉีดก่อน page script; backdrop `#oscengine-backdrop` รอ DOMContentLoaded; **unlock shim**: ยิงทุก 300ms ถึง 150 ครั้ง → patch `nvCameraService` (isOn/isModsOn/isGfeAnselSupported → true) + `launchUIForNvCamera/launchUIForMods → $state.go('nvcamera'/'mods')` (preview); dispatch ครบ: `QUERY_WIN_NODE_INFO` (port+secret — boot-critical), `QUERY_FULLSCREEN_STATE` (ตัด own pid + maximized มี WS_CAPTION ไม่นับ), SET_DISPLAY_RECTS/PAINTING/EXPERIMENTAL, REGISTER_CLOSE_EVENT (persistent), OPEN/CLOSE_OSC, READ/WRITE_SHARED_STORAGE, LOAD_STRING_TABLE, COPY_TO_CLIPBOARD, `QUERY_HTTPSERVER_START`→fail fast (oauth), unknown→fail -1 (หน้า degrade สวย) |
| `Overlay.Engine/OscControllerServer.vb` | settings store ถาวร `Data\osc-settings.json` (POST = merge verbatim, GET = คืนค่าที่เซฟ — **หน้าเป็น shape authority**); พอร์ตว่างสุ่ม (TcpListener :0) + secret สุ่ม; auth cookie `X_LOCAL_SECURITY_COOKIE` (static GET ยกเว้น); FULL MODE catalog lookup longest-prefix; handlers: `/uiReady, /state, /Record/Settings (GET/POST→store), /RecordPaths, /Record/Enable→hub, /Language ({} ทำ crash .indexOf — ตอบ en-US เสมอ), /support, Screenshot/Support+Capture, Microphone/Present (จำนวน mic จริง winmm), Webcam/Present (registry DeviceClasses), Webcam/Enable, Record/Running, InstantReplay/Running+Enable, HardwareInformation/v.0.1 (fake GPU กัน gfwsl crash), DesktopCapture/Support/Reason (ต้องมี support หรือ string — ไม่งั้น crash), Hotkey/{name} GET/POST `{keys:[vk...]}` (เก็บ verbatim ยกเว้น monitor/dynamictoggle → event HotkeySaved), Capture/ProcessInfo/*, Broadcast/Provider+Enable, DesktopCapture/Enable, Audio, AudioSettings, Record/Concurrency/*, Indicator/{id}/Support+Settings, NvCamera/v.1.0/GetResolutions (1080p/1440p/4K), unknown→{}`; `PushEvent()` socket.io v2; แฟรมมิ่ง golden: `b64=1`→text ไม่งั้น binary, POST ack = 0 packet, noop `1:6`, client ping→pong; preview hotkey defaults = GFE จริง (Alt+F1 Screenshot, Alt+F9 RecordToggle, …) |
| `Overlay.Engine/OscHotkeys.vb` | Alt+Z เมื่อไม่มี GFE (อ่าน config `Hotkeys.ToggleOverlay`), first-come-first-served ไม่แย่ง, parser A-Z/0-9/F1-F24 (unit-tested) |
| `Overlay.Engine/OscHotkeyApplier.vb` **(ใหม่ล่าสุด)** | **ผู้บริโภค settings store**: อ่าน `hotkey:{name}` (`{keys:[vk...]}`, 16=Shift 17=Ctrl 18=Alt 91/92=Win) → `RegisterHotKey` จริง id≥100, Reapply ทุกครั้งที่เซฟ; 9 actions: Screenshot, RecordToggle, RecordSave, DVRToggle, NvCameraUI, ModsUI, CameraToggle, MicToggle, BroadcastToggle (**Overlay/OpenShare ยกให้ OscHotkeys โดยเจตนา** — Alt+Z ownership) |
| `Overlay.Engine/OscEngineClient.vb` | TCP hub :5001; บรรทัดคำสั่ง golden (`RECORD_START <path>` ฯลฯ); events → form |
| `Overlay.Engine/OscWire.vb`, `OscProtocol.vb` | แฟรมมิ่ง engine.io v3/socket.io v2 (golden transcript); `RecordOutputPath`, `ShouldShowRecording`, `ParseDisplayRects`, `SocketEventPacket` |
| `Overlay.Engine/SharedStorageStore.vb` | `Data\osc-shared-storage.json` — KV ของ QUERY_READ/WRITE_SHARED_STORAGE; corrupt อ่านเป็นว่าง; เขียน tmp+`.bak` |
| `Overlay.Engine/responses/` | **146 real captured endpoints** จับจาก GFE จริง (FULL MODE); แก้เอง 2 จุด: `PiplConfig_v.1.0_data.json` → `{daysToExpire:1, isConnectEnabled:true, configData:{servers:""}}` (**isConnectEnabled = gate ของ Broadcast tile + หน้า Connect**), `...getAnselReady/getFreeStyleReady` → `{"criteria":{"overallState":"true"}}` |
| `Overlay.Engine/app.manifest` + vbproj | PerMonitorV2 DPI (เลียนแบบ Share.exe); `ApplicationManifest`; Content `osc\**` + `responses\**`; **linked Compile ข้ามโปรเจกต์**: `TcpClientHelper.vb`, `EngineProcessSupervisor.vb` (Forms overlay), `NvShadowPlayRecorder.vb` (Engine) — **แก้ที่ต้นทางเดียว** |
| `Overlay/osc/config.js` | ปัจจุบัน `nvCamera:true, anselLite:true, mods:true` (ปลดล็อกหน้า preview) — README ที่เขียน "false" ล้าสมัย; เคยต้องปิดเพราะ bundle bug ใน Ansel init path (`.catch` อ้างตัวแปร `error` ที่ไม่มีอยู่) |
| `Overlay/osc/{index.html,app.js,common.js,vendor.js}` | เพิ่ม NVIDIA copyright header +9 บรรทัดเท่ากันทุกไฟล์เท่านั้น — ไม่มี logic change (มาจาก session อื่น, ตรวจแล้ว W1 §13) |
| `Overlay.Engine/tools/sanitize-catalog.py` | ล้าง marker `// status=NNN` ที่ capture tool ทิ้งใน `responses/*.json`; salvage `{...}`/`[...]` แล้ว fallback `{}` — **catalog ต้องเป็น JSON บริสุทธิ์ ไม่งั้น boot สะดุด** |
| `Directory.Build.targets` | staging: copy `osc\**` ไป product tree (`Overlay\osc\`) แล้วลบ source dir หลัง copy |
| `Engine/Engine/[Capture]/NvShadowPlayRecorder.vb` (shared, ใหม่) | external recorder ผ่าน ShadowPlay จริง: Start/Stop = inject Alt+F9, ผลจริง = ไฟล์ในโฟลเดอร์; `IsAvailable()` = มี NVIDIA Share.exe; `InjectAltZ()` สำหรับ tray |
| `Overlay.Engine/README.md`, `PROTOCOL-MATRIX.md` | ground-truth contract (TCP line, pipe rule, framing, REST shapes + file:line evidence) — README ย่อหน้า M0/hotkey/config flags ล้าสมัยเล็กน้อย ยึดโค้ดจริง |
| `Tester/test/Overlay/Overlay.OscEngine.Tests/` (ใหม่) | 20 tests (wire 9 / hub 7 / rects / bridge / hotkey parser) + `golden/golden-transcript.json` จาก engine.io-client@3.5.4 + socket.io-client@2.5.0 จริง + `tools/open-overlay.js` |

## 3. สิ่งที่ทำงานแล้ว (พิสูจน์แล้ว)

1. Alt+Z toggle (เครื่องนี้ engine ถือเอง) / tray double-click / hub `open_overlay` — debounce 500ms
2. เมนู tiles ครบ 10 (Screenshot/Photo mode/Game filter/IR/Record/Broadcast/Gallery/mic/cam/settings) + ป้าย hotkey ครบ (Alt+F1/F2/F3/F8/F9/F10) — ไม่มี tile โดน "Disabled" จาก keys หาย (preview defaults + store)
3. **เซฟถาวรทุกหน้า settings** → `Data\osc-settings.json` — รอด restart (keyboard shortcuts / record / recordPaths / audio / HUD layout / broadcast / instantReplay)
4. **rebind hotkey → hotkey จริงของ OS** (OscHotkeyApplier, re-apply ทุก save) + action จริง: **Screenshot = ได้ไฟล์ PNG จริง** (`Pictures\NVIDIA ShadowPlay\Screenshots\` + notification contract `{notification:"screenshot",result:0,file}` — Alt+F1 ยิงแล้วพิสูจน์แล้ว), RecordToggle (TCP จริง — ต้องมี Engine process), NvCameraUI/ModsUI (เปิดหน้า preview)
5. Record จริงผ่าน TCP chain: POST `/Record/Enable` → `SendRecordStart(RecordOutputPath)` → hub :5001; `RecordStartedConfirmed` → toast; `engine_recording_saved` → `recordingSaved` notification + gallery
6. FULL MODE: GET ทุกตัวตอบจาก catalog GFE จริง 146 endpoints — หน้าเห็นคำตอบเดียวกับ host แท้
7. Unlock: Photo mode / Game filter / Mods เปิดหน้าเต็มได้ทุกหน้า (shim + DESKTOP_MODE=true + PiplConfig) — Settings ครบทุกหมวด (Connect/Broadcast LIVE/Highlights ฯลฯ)
8. Mic count จริง (winmm) / webcam present จริง (registry)
9. Suite `Overlay.OscEngine.Tests` 20/20, Release build 0 errors (W1 gate matrix ผ่านหมดยกเว้น G9 ที่เคย blocked โดยสภาพแวดล้อม)

## 4. Worklist ตัวถัดไป (เรียงตามความคุ้ม)

1. **Record/Settings: shape + ผู้บริโภคจริง** — GET ตอบ `{}` จนกว่าจะมี POST แรก (store ยังไม่มี section `recordSettings`; `RecordSettingsProvider` ยังไม่ถูก wire ใน OscHostForm) → หน้า Video settings อาจแสดงค่าว่าง; และค่าที่เซฟยังไม่ถูก forward ไป CaptureEngine/RecordingEngine (engine_mode, fps, bitrate, resolution) — หา shape จาก `settingsService` ใน `Overlay/osc/app.js` + `docs/osc/real-controller-responses/`
2. **Audio devices จริง** — `/Audio` ตอบ `{mode:"off"}` เดา; หน้า Audio ต้องได้ list อุปกรณ์จริง (NAudio หรือ registry) ทั้ง input/output + per-device enable
3. **Ansel/Photo-mode panel ผ่าน socket event** — REST `GetResolutions` ตอบ array แล้วแต่หน้ารับค่าจริงผ่าน socket: constant `NVCAMERA_EVENTS` ใน `app.js` = `{stop, filters, highResResolutions, screenshotResolution, cameraFOVRange, cameraRollRange, cameraFOVValue, setRoll, setFov, available, gameResolution, currentFilterSettings, …}` (ใน bundle: `highResResolutions"===e.type → f.trigger(A.HIGHRES_RESOLUTIONS, e.resolutions)`) — ต้อง emit ทาง `PushEvent()` ให้ตรง; Resolution slider 0x0 = ยังไม่ emit; Filters (0 applied) ต้อง `GetFilters` + event `filters`
4. **Tile Photo mode บาง state ยัง Disabled** — hotkey โอเคแล้ว (applier) เหลือ availability event ของ nvCameraService ยังไม่ emit
5. **OSD indicators** — `Indicator/{id}/Support+Settings` เซฟได้แล้ว (`indicator:record/fps/viewer` ใน store) แต่**ไม่มี surface วาดจริง** (HUD fps/record/viewer ต้องเป็น overlay window/WebView เสริม)
6. **Engine จริงต่อ feature** — Game Filter = MH hook milestone; Broadcast = streaming backend จริง (Provider+Enable เก็บแล้ว); Connect = Duluka server (`tools/duluka-account/`); Ansel capture = ต่อ screenshot จริงแทน preview; Instant Replay = host endpoint ยังไม่ expose (ตอบ honest `{"running":false,"status":"off"}`)
7. **ขยาย hotkey actions** — RecordSave/DVRToggle/CameraToggle/MicToggle/BroadcastToggle ยังตอบ toast preview; ต่อเข้า engine จริง; `hotkey:overlaytoggle` ที่หน้าเซฟ**ไม่ถูก apply โดยเจตนา** (Alt+Z ownership arbitration — ถ้าจะทำ ต้องยุติเจ้าของก่อน)
8. **Known issue จากการทดสอบล่าสุด**: RecordToggle ต้องมี Engine process — hub ยังติด "bounded status pull exhausted" ใน standalone layout (ค่า status ไม่ยืนยัน → record pending ไม่ converge); ตรวจการ hand-shake ของ status pull ระหว่าง engine↔hub ก่อนแก้อย่างอื่นที่พาดผ่าน

> เวลาแก้: จำไว้ว่า store เป็น "หน้าเป็น shape authority" — POST เก็บ verbatim ดังนั้นสิ่งที่ต้องทำคือ **เพิ่มผู้บริโภค** ไม่ใช่แก้ store

## 5. เครื่องมือ

| งาน | วิธี |
|---|---|
| Build/Run | `dotnet build "Overlay.Engine/NVIDIA Overlay Engine.vbproj" -c Release` → รัน exe จาก bin, หรือ staging ที่ `Downloads\ShadowPlay.v1\` |
| CDP (คุมหน้าเว็บด้วยสคริปต์) | env `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS=--remote-debugging-port=9224` → probe `http://127.0.0.1:9224/json` (สคริปต์ชุดเดิม `%TEMP%\osc-golden\*.js`) |
| DevTools ในหน้าต่าง | env `OSCENGINE_DEVTOOLS=1` |
| กด Alt+Z แทนผู้ใช้ | `%TEMP%\osc-golden\altz2.ps1` — **เคลียร์ modifier ก่อน/หลังเสมอ** (ไม่งั้นคีย์บอร์ดผู้ใช้พัง) |
| Log | `<runtime>\Logs\overlay-engine.log` — บรรทัด `controller server on … secret=…` คือตัวต่อ REST/socket ด้วยมือ |
| เปิด overlay จาก hub | `node Tester\test\Overlay\Overlay.OscEngine.Tests\tools\open-overlay.js` |
| ทดสอบ | `Tester\test\Overlay\Overlay.OscEngine.Tests\` (20 tests, golden transcript); harness duplicate-subscription: `tools/osc-reui/` (POST `/__transcript`) |
| ล้าง catalog | `python Overlay.Engine\tools\sanitize-catalog.py [paths…]` (default `responses\`) |
| เทียบ shape จริง | `docs/osc/real-controller-responses/` + nodejs ต้นแบบใน Downloads (§1) |
| Env knobs | `OSCENGINE_PING_INTERVAL` (default 25000, test ใช้ 1500), `OSCENGINE_DEVTOOLS=1`, `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` |

## 6. กับดักที่โดนมาแล้ว (อ่านก่อนแก้ — ทุกข้อคือบทเรียนที่วัดจริง)

1. **จอ portrait**: ห้าม park ที่ -32000 — Chromium derive `window.screen` จาก monitor ที่ HWND rect → บูตแล้วไปเกาะจอ portrait (#2, 1080×1920) เมนูวางผิดที่ — park ใต้จอหลัก (`b.Bottom+40`) เท่านั้น และห้ามย้ายหน้าต่างขณะเปิด (alpha surface พัง)
2. **Zoom**: ตั้ง `min(W/1920,H/1080)` ตอนสร้าง WebView ครั้งเดียว — ตั้งตอนเปิดอยู่ = จอดำ (osc ออกแบบ canvas FHD)
3. **Same-origin เท่านั้น**: serve หน้าจาก `http://localhost:<port>` เดียวกับ REST — cross-host ทำให้ทุก call ส่ง preflight OPTIONS ที่ไม่มี auth cookie และโดนปฏิเสธ
4. **engine.io polling**: POST response ต้อง decode ได้ **0 packet** (octet-stream เปล่า); โหมดตอบ `b64=1` → text/plain, ปกติ → binary framing — ผิด = client parser ตาย → **handshake storm**
5. **JSON ปนเปื้อน**: `responses/*.json` ต้องเป็น JSON บริสุทธิ์ — marker `// status=` จาก capture tool ทำ SyntaxError → boot สะดุด + socket push ไม่ถูกอ่าน (`sanitize-catalog.py` มีไว้เพราะเรื่องนี้)
6. **Click-through**: ตอนเมนูเปิด = หน้าเป็นเจ้าของ input ทั้งจอ (HTCLIENT เสมอ) — **อย่า** branch ตาม displayRects เพราะ toast/transition รายงาน rect ไม่ครอบเมนู → จอคลิกไม่ได้ทั้งจอจนกด toggle
7. **Backdrop div**: สร้างหลัง DOMContentLoaded เท่านั้น (documentElement = null ตอน document-created script; catch กลืน error เงียบ → div ไม่เกิด → จอขาว)
8. **Fullscreen probe**: ตัด own-process pid ออก (overlay เราครอบจอ = ไม่ใช่เกม — ไม่งั้นเมนู auto-dismiss ~0.5s); app maximized (Telegram) มี WS_CAPTION → ไม่นับเป็นเกม
9. **InitWebView ห้าม `.Wait()`** — resume บน UI thread → deadlock หน้าค้าง about:blank (วัดจริง)
10. **`/next/` ห้ามกลับมา**: owner directive ลบถาวรแล้ว — production = `/index.html` NVIDIA osc แท้ (ResolveOscRoot ก็ต้องเจอ index.html ที่ root)
11. **ขอบเขตไฟล์**: ห้ามแตะ `C:\Program Files\NVIDIA Corporation\` (รวม `…\ห้ามแตะห้ามใช้\`); ไฟล์ที่แก้ได้ = ตาราง §2 เท่านั้น; vbproj ลิงก์ source ข้ามโปรเจกต์ (Supervisor/Recorder) — แก้ที่ต้นทางเดียวแล้วกระทบทุก consumer
12. **Sync/staging**: robocopy จาก bin → `ShadowPlay.v1\` **ต้อง exclude `Logs\` และ `Data\`** — เคยทับ log ทิ้ง (log = หลักฐาน port/secret ที่ใช้ debug)
13. **เอกสารล้าสมัย**: README ของ engine (M0/hotkey/config flags) กับความจริงต่างกันแล้ว — ยึดโค้ด + เอกสารนี้; `config.js` ปัจจุบัน = flags true
14. **Concurrent agent sessions**: repo นี้มีงานเข้าจากหลาย session (เคยชนกันจนต้อง STOP/INSPECT/PRESERVE ตาม W1 §8; เอกสารนี้เองถูกเขียนทับระหว่างทางหนึ่งครั้ง) — ก่อน drive interactive ตรวจว่าไม่มีใครคุม overlay อยู่, ไฟล์ใหม่อาจโผล่ระหว่างทาง, แก้อะไร re-read ก่อนเขียนทับ
15. **Hotkey ownership**: RegisterHotKey first-come-first-served — ไม่แย่ง ไม่ retry รุนแรง; ถ้า Forms overlay/GFE ถือคอมโบอยู่ ให้ degrade ไป tray/hub อย่างซื่อสัตย์
16. **Dead-case ใน Select Case (บั๊ก settings ไม่เซฟมาตลอด)**: `Case "/Record/Settings"` ไม่ match `/ShadowPlay/v.1.0/Record/Settings` — case ต้องเป็น **full path** เสมอ; ตอนนี้แก้แล้ว 3 case (Record/Settings, RecordPaths, Record/Enable) — เช็ค case อื่นที่ path สั้นเช่นกัน
17. **Catalog shadowing**: `FindCatalogResponse` (longest-prefix) รันก่อน handlers — ไฟล์ `ShadowPlay_v.1.0_*.json` ที่เป็น `{}` บัง live handlers ทุกตัว (เคยทำให้ Screenshot/Support = undefined, Microphone/Present = {}); ลบ 12 ไฟล์ shadow แล้ว — **เพิ่ม live handler ใหม่ = ต้องลบ catalog file ทาง prefix เดียวกันด้วย**
18. **robocopy //E ไม่ลบไฟล์ที่ถูกลบจากต้นทาง** — ไฟล์ที่ลบใน bin ยังค้างใน ShadowPlay.v1 (ดู "Extras" column) — ลบตรง ๆ เสมอ
19. **WAVEINCAPS P/Invoke**: ByValTStr default = ANSI (struct 48 ไบต์ → waveInGetDevCapsW คืน 11 INVALPARAM) — ต้อง `StructLayout(CharSet:=CharSet.Unicode)` + ใส่ `wReserved1` ให้ครบ 80 ไบต์
20. **Assembly เปลี่ยนชื่อแล้ว**: output = `NVIDIA Share.exe` (AssemblyName + ลบไฟล์เก่า `NVIDIA Overlay Engine.*` จาก staging ด้วย); `OscProtocol.AppName` = "NVIDIA Share"; `Overlay.Engine/gfe/` (198MB installer junk) ถูกลบแล้ว

## 7. Quick start ของ agent ตัวถัดไป

1. อ่านเอกสารนี้ + `PROTOCOL-MATRIX.md` (สัญญา ground truth)
2. Build Release → sync ไป `ShadowPlay.v1\` (exclude `Logs\`/`Data\` §6.12) → รัน → toggle ด้วย tray หรือ `open-overlay.js`
3. เปิด log ค้างไว้ (port+secret อยู่บรรทัด `controller server on`) — ต่อ REST/socket ด้วยมือได้ทันที
4. เลือกงานจาก §4 (แนะนำเริ่มข้อ 1: หา shape ของ Record/Settings แล้ว wire `RecordSettingsProvider` + ส่งต่อ CaptureEngine; และข้อ 8 ตรวจ "bounded status pull exhausted" ก่อนขยับ record flow)
5. เจอกับดักใหม่ → เพิ่มเป็นข้อใน §6 แล้ว update เอกสารนี้ทุกครั้ง
