# NvNode backend + hosts + container dissection (GFE 3.28.0.412 payload) — 2026-09-25

> ภาคต่อ docs/osc/20-21 — ครบทุกชิ้นที่เกี่ยวกับ ShadowPlay นอกโฟลเดอร์ ShadowPlay\
> แหล่ง: `C:\My Project\gfe-3.28.0.412-extract\` (ไม่ใน git)

## 1) `nodejs\` = NvNode backend ตัวจริง (JavaScript อ่านได้ทั้งหมด!)

| ไฟล์ | บทบาท |
|---|---|
| `index.js` (842 บรรทัด) | entry — express + **socket.io** + http server |
| `NvShadowPlayAPI.js` (2,879 บรรทัด) | ⭐ REST API ของ ShadowPlay ทั้งชุด (ดู §2) |
| `NvShadowPlayAPINode.node` | native bridge → `_nvspcaps64` server (`_register_NvSpCapsAPINode_`, `ShowOverlay`, `SetupHighlightSession`, `SaveHighlightVideo/Screenshot`, `GetProcInformation`, `WhitelistProcess`…) |
| `NvBackendAPI.js`, `NvAccountAPI.js`, `NvGalleryAPI.js`, `NvGameShareAPI.js`, `NvGameStreamAPI.js`, `NvCameraAPI.js`, `NvSDKAPI.js`, `NvABHubAPI.js` | โมดูล API อื่น ๆ (แต่ละตัวมี .node คู่ — native) |
| `nvnodejslauncher.exe` | launcher ที่ Share.exe เรียก — ถามสถานะ container service ก่อน ("Cannot get container service status") + use-case session (ShadowPlay_v1_0_GetRecordPaths, Gallery_v1_0_GetRecentFiles) |
| `NVIDIA Web Helper.exe` (28MB) | **node.exe เต็มตัว** (V8 exports 17,143 ตัว!) — รัน `\NVIDIA Corporation\NvNode\index.js` |

boot order จาก index.js: PiplConfig → NvBackendAPI → NvAccountAPI → DriverInstall →
downloader → ABHub → GameStream → Gallery → Camera → **LoadShadowPlay()** (deferred
จน dependencies พร้อม) → LoadSDK() (เฉพาะเมื่อ ShadowPlayAPI ขึ้น) — โหลด fail โมดูลไหน
ก็ `ReportOptionalModuleLoadError` แล้วรันต่อ (ไม่ล้มทั้ง backend)

security: `nvUtil.IsSecurityCheckEnabled()` + header/handshake `X_LOCAL_SECURITY_COOKIE`
(CORS allow-list ระบุ header นี้) — ตรงกับ registry `Global\NvNode port=59001 +
disableSecurity=1` ที่ docs/osc/16 บันทึกไว้

## 2) ShadowPlay REST API surface จริง (จาก NvShadowPlayAPI.js)

กลุ่ม endpoint (70+):
- InstantReplay: Enable(GET/POST) Running Settings(GET/POST) **Save** BufferLength
- Record: Enable Running Settings Concurrency/:mode
- Broadcast: Enable Pause Running Support Settings SessionParam(v1.0+v1.1/:type)
  LastProvider Provider Title Viewers(/Max) IngestServer 2KSupport 2KEnable FBLiveSupport
- OSC: `v1.0/OSC/GetCustomize/InstantReplay|Record|Broadcast`, OSC/Init, OSC/MainView,
  OscNotification, Osc
- **Hotkey: `POST/GET /ShadowPlay/v.1.0/Hotkey/:hk`** ← Alt+Z ของ hotkey-listener ยิง
  `/Hotkey/Toggle` → `:hk="Toggle"` ตรงตัว
- OpenOsc / OpenOscPreferences / OpenOscState / Launch (GET/POST)
- GetHDRState GetSupported Video/Trim 4KSupport 8k60 BitRates/:quality/:resolution
  Resolutions(/:quality) Framerates(/:quality) RecordPaths
- Capture: State / PIDMode / ProcessInfo/:PID
- Webcam(Enable/Toggle/Shown/Present/Settings) Microphone(+PTT/:index/Settings)
  Audio AudioSettings
- CoPlay Indicator/:id DesktopCapture(Enable/Support/Reason)
  CustomOverlay(Enable/Path/Support/DefaultPath/Display — v1.0+v1.1/:index)
- Screenshot(Support/Capture/NGXShot/NGXCancelShot) Input
  Highlights(Customize/GalleryImport/Session)

## 3) ⭐ สาม CEF host = ไบนารีเดียวกัน

`NVIDIA Share.exe` = `NVIDIA GeForce Experience.exe` = `NVIDIA Notification.exe`
(**ทั้งสาม 3,269KB — strings เหมือนกันทุกตัวอักษร**) ต่างกันด้วยไฟล์ .json ข้างตัว
(Share.json / GeForce Experience.json / NVIDIA Notification.json) — one binary,
many configs. switches ที่รองรับ: `nv-osc, nv-gpu-accel, nv-url-relative,
nv-url-absolute, nv-node-app, nv-node-data, nv-remote-debugging-port,
enable-media-stream` + window-message protocol `QUERY_WIN_OPEN_OSC /
QUERY_WIN_CLOSE_OSC / QUERY_OSC_SET_PAINTING / QUERY_OSC_SET_DISPLAY_RECTS /
QUERY_OSC_REGISTER_CLOSE_EVENT / QUERY_OSC_DROP_URL / QUERY_OSC_SET_EXPERIMENTAL`
+ WMI file notifications + `nvnodejslauncher` spawn + `cookiename/portNumber`.

## 4) NvContainer + MessageBus

- `NvContainer.exe` (1.2MB, export `NvOptimusEnablement`): Service host
  (ServiceThread/ServiceConfigThread, `\NvcServiceHost..`) + directory listener
- `MessageBus.dll` (7.4MB, 12 exports): `messageBusNew/PostMessage/
  PostEncryptedMessage(→fallback postMessage)/AddObserver...` — **พอร์ตอ่านจาก
  registry `HKLM\...\NvContainer\MessageBus\MessageBusPort`**, pipe ต่อ session
  `MessageBus_%u_0x%LX`, built on Poco, IoCompletionPort
- `NvPluginWatchdog.dll`: crash-rate policy ต่อ session ("crash rate exceeded
  maximum → Disable it"), สร้าง child process พร้อม DACL (`NvpChildProcessCreate`),
  ตาม WTS session events

## 5) NvBackend / telemetry

- `NvBackend64.dll` (plugin) — OpenSSL static, services ฝั่ง account/telemetry
- `NvTmRep.exe` — "NVIDIA crash and telemetry reporter" (Gfe_v1_0_StreamingAssets,
  isStreaming, VRSessionTimeInMsec…)
- `NvSHIM.exe` — system-info shim

## 6) สรุปสถาปัตยกรรมครบสาย (osc + engine + container)

```
nvcontainer (service, NvContainerLocalSystem)
  ├─ spawn nvsphelper64 (ShadowPlay engine — docs/osc/21) ผ่าน _nvspserviceplugin64
  ├─ MessageBus.dll : MessageBusPort (registry) — control plane กลาง
  └─ NvPluginWatchdog: crash policy + session management

NVIDIA Share.exe (CEF host — ไบนารีเดียวกับ GFE/Notification)
  ├─ อ่าน Share.json switches[] → spawn nvnodejslauncher → Web Helper.exe(node)
  │   รัน NvNode backend (index.js) → HTTP+socket.io :59001 (security cookie
  │   ปิดผ่าน disableSecurity=1)
  ├─ NvShadowPlayAPI.js = REST 70+ endpoint (Hotkey/:hk รวม)
  │   ↕ NvShadowPlayAPINode.node → _nvspcaps64 server (session API, ShowOverlay)
  └─ เปิดหน้าต่าง osc → โหลด osc/index.html (same-origin 59001)

Alt+Z (Intel): hotkey-listener.ps1 → POST /ShadowPlay/v.1.0/Hotkey/Toggle
  (native จริง = nvsphelperplugin64 → MessageBus/MMF — docs/osc/20 §4)
```

## 7) แนวทางต่อยอดสำหรับโปรเจกต์เรา

1. **one-binary-many-configs**: พิจารณาทำ CEF host ของเราเป็นไบนารีเดียวสลับ .json
   (Share/Notification) ตามแบบ NVIDIA — ลดงาน build/layout
2. **API parity**: routes/shadowplay.js ของเราเทียบชุด endpoint §2 แบบรายเส้น
   (esp. Hotkey/:hk, InstantReplay/Save, OSC/GetCustomize/*) — osc page จะคุยได้
   100% กับ backend เรา
3. **socket.io**: osc จริงใช้ websocket ด้วย — backend เราติดตั้ง socket.io แล้ว
   (package.json) แต่ยังไม่เปิด channel — ตรวจว่าหน้าใช้ channel ไหนบ้าง
4. **security cookie**: รองรับ `X_LOCAL_SECURITY_COOKIE` ตั้งแต่วันนี้
   (backend เรา securityCheck=false อยู่แล้ว — เตรียม path เดียวกันไว้)
5. diff `nodejs\index.js`/`NvShadowPlayAPI.js` กับ routes/*.js ของเราแบบ
   endpoint-by-endpoint = backlog ที่วัดผลได้
