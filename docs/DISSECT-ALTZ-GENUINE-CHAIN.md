# แกะจริง: โซ่ Alt+Z ของ GFE 3.28 แท้บนเครื่องนี้ (พิสูจน์ด้วย log + ทดลองควบคุม)

วันที่: 2026-09-26 | สถานะ: hotkey chain ผ่านครบทุกชั้น, ค้างที่ presentation สุดท้าย

## สแตกที่รันอยู่จริงตอนทดสอบ (ไม่มี shim เลย)
| ชั้น | ไฟล์ | บทบาทยืนยันจาก log |
|---|---|---|
| Container | `C:\Program Files\NVIDIA Corporation\NvContainer\nvcontainer.exe` ×3 | MessageBus broker, spawn helper/Share |
| Engine | nvsphelper64.exe (spawn 20:05:47 หลัง container 20:05:44) | join MessageBus เป็น `System: Hotkey, Module: HotkeyPlugin` |
| Node backend | `Program Files (x86)\NVIDIA Corporation\NvNode\NVIDIA Web Helper.exe` (node v11.13.0 แท้, index.js **pristine**) | :59001 LISTENING, log = `%LOCALAPPDATA%\NVIDIA Corporation\NvNode\nvnode.log` |
| Frontend OSC | `NVIDIA Share.exe` ×3 (browser หลัก + 2 worker ไร้หน้าต่าง) | โหลด `osc/index.html` (ตาม Share.json `nv-url-relative`) |

## ข้อสรุปเรื่อง 126 (ตรวจแล้ว ไม่ใช่การนับซ้ำ)
- `nvnode.log` ทั้งไฟล์ grep "126|trusted" → 3 hits เป็นแค่ substring (`11264` VRAM, request #126) — **ไม่มี trusted-location fail แล้วจริง**
- pristine `index.js:23 require('./NvUtil.node')` ผ่าน → boot `Initialization complete` (ModuleMap registry ที่แก้ไว้คือตัวแก้จริง)
- 126 ยังมีบทบาทที่อื่น: `NVIDIA Share\debug.log` เจอ `Failed to load dependency libeay32/ssleay32/poco/PocoInitializer.dll. Error 126` ฝั่ง **plugin loader ของ Share.exe** (ดูหัวข้อ nv-osc)

## โซ่ Alt+Z ที่พิสูจน์ทีละข้อ (มี control: Alt+X = ไม่มี trace เลย)
1. helper จับคีย์ → `CShadowPlayHelper::ProcessWindowMessges: WMHK 7` (CaptureCore.log)
2. ผ่าน MessageBus → `CShadowPlayHotkeyReceiver::PluginCallback: Received message 7` (ฝั่ง node)
3. node: `Hotkey Id: 7 State: 1` → `HotKey callback with data:{"hotkeyId":"OSC","hotkeyState":"Down"}`
4. node push หา frontend ผ่าน socket.io: `EmitNotification('/ShadowPlay/v.1.0/Hotkey', hotkeyData)` (NvShadowPlayAPI.js:2834-2838)
5. frontend `osc/app.js`: dispatcher `"OSC"===e.hotkeyId ? t=i.OSC_TOGGLE` → event `Hotkey_OSC` → `function T(){ z || (P ? M.closeOSC() : M.openOSC()) }`
6. ผลจริง: หน้าต่าง Share สลับ hash `index.html#/base` ↔ `index.html#/base/main-menu` (ยืนยันจาก window title 2 ครั้ง)

## รัฐเครื่องของ frontend (decompile osc/app.js — ชื่อตัวแปร minified แต่พฤติกรรมชัด)
- `P` = "ผมเปิดอยู่" (isShown) — **frontend boot มาแล้วคิดว่า P=true** (กดแรกเดิน path close พ่น `NotifyOverlayState {open:false,state:"main"}`)
- `z` = transitioning (กัน re-entry, ตั้งใน `_()` ตอน open)
- `D` = pending native open — `$stateChangeStart`: `D&&(D=!1, s.openOSC(!0).then(... notifyOverlayState(!0)))` = **native resolve แล้วค่อยรายงาน open:true**
- `R` = counter "เปิดโดย notification อยู่"
- handler `A` รับ push `/ShadowPlay/v.1.0/WindowState`: `dismiss` → closeOSC, `overlayToggle` → T(), `fullscreenTransition` → transition, `showHotkeyMessage` → DISPLAY_HOTKEY (ตรงกับโมเดล windowMsg string ที่ตั้งไว้)

## สาเหตุที่หน้าต่างไม่ขึ้น (แก้แล้ว 1 จุด)
1. **`nv-osc=false` ใน `NVIDIA Share.json` ที่ติดตั้ง (ของแท้ = `true`)** → Share.exe ไม่ลงทะเบียน handler ฝั่ง native → `debug.log`: `Did not handle cef query QUERY_OSC_SET_DISPLAY_RECTS / QUERY_WIN_OPEN_OSC / QUERY_OSC_SET_PAINTING / QUERY_FULLSCREEN_STATE` (รวม 21 ครั้ง) → `s.openOSC(!0)` ไม่ resolve → ไม่มี notifyOverlayState(true), หน้าต่างจม -32000 ตลอด
   - **แก้แล้ว**: สลับเป็น `nv-osc=true` (backup: `%TEMP%\Share.json.bak`) → รีสตาร์ต Share → native เริ่มทำงานจริง: `fullscreen check` → `Starting DT` → `initializing osc d3d11 renderer to: 1680x1050` → `OnFocus: 1`
   - บทเรียน: อย่าแก้ Share.json เป็น false อีก — สวิตช์นี้คือประตู OSC ฝั่ง native ทั้งบาน
2. **(ค้าง)** หลังเปิด: DT (desktop capture) โดน `OnCaptureLost` วน → `should close osc` → ปิดตัวเองใน ~10 วิ
   - ปัจจัยที่ log ชี้: เกมที่เล่นอยู่ `ComputeAppSettings: isGame:0 isEnableHook:0` (ไม่มี profile → ไม่มี in-game hook ให้ composite) + มีผู้จับ desktop duplication แย่งในเครื่อง
   - ชั้นนี้คือ **presentation/in-game hook = แดน engine** (ตามสถาปัตยกรรม 3 ชั้น คือ CaptureEngine ของเราในอนาคต)

## โปรโตคอลสำคัญที่ควรจำ (สำหรับขั้นทำ nvsphelper64 ของเรา)
- hotkey event: helper → MessageBus (`System: Hotkey`) → node รับ → REST shape ที่ frontend ใช้ = `{"hotkeyId":"OSC","hotkeyState":"Down"|"Up"}` (hotkeyId อื่น: DVR/Manual/GameCast/MicPTT/Camera/PauseResume/FPS/Screenshot/CustomOverlay/NvCameraUI/CommentsToggle/...)
- hotkey config: frontend GET `/ShadowPlay/v.1.0/Hotkey/<name>`; node อ่าน registry ไม่เจอจะ fall back default — `openshare` default = `[18,90]` (Alt+Z)
- overlay state รายงานกลับ: `POST /SDK/v.1.0/NotifyOverlayState {open:bool, state:"main"|"permission"|"highlightsSummary"}`
- หน้าต่าง OSC: offscreen D3D11 (`offscreen_windowing_d3d11`) — UIA มองไม่เห็น, ต้องแคป composite ด้วย CopyFromScreen
- สถานะ enable ของ helper: `MonitorHotKeysState` ใน state dump (เคยเห็น True 1 ครั้งที่ 20:05:33, ที่เหลือ False — ยังไม่หาตัวแปรกำหนด; Alt+Z ยังโดนจับแม้ False)

## สิ่งแวดล้อมตอนทดสอบ
- เกม: Minecraft Dungeons (Dungeons-Win64-Shipping.exe, หน้าต่าง 1680x1050, borderless-ish) — ผู้ใช้กำลังเล่นมัลติเพลย์ → หยุดทดสอบสดเพื่อไม่กวน
- คำสั่งที่ใช้เจาะ: tasklist/netstat, อ่าน nvnode.log + CaptureCore.log + Share debug.log/console.log, computer-use ส่ง Alt+Z/Alt+X + list_windows, CopyFromScreen
