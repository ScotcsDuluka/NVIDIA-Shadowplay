# 14 — SHADOWPLAY DEPENDENCY INVENTORY (สิ่งที่ ShadowPlay ต้องใช้จริง)

ทั้งหมดสังเกตจากการรัน **GFE แท้** บนเครื่องนี้ (nvnode.log = server log ของ
controller จริง, console.log = client log ของ Share.exe) ระหว่างการใช้งาน
จริงของเจ้าของ (สร้างเมื่อ 2026-09-13)

## 1. Processes ที่ต้องรัน (ตามลำดับ boot)

| Process | ใครปลุก | หน้าที่ | Spawn on-demand? |
|---|---|---|---|
| `nvcontainer.exe` ×3 | service (auto-start) | host ของ NvNode controller | — |
| **NvNode controller** (ใน nvcontainer) | nvcontainer | HTTP + Socket.IO server (:65062 dynamic) — ตอบ osc ทุก endpoint | — |
| `NVIDIA Web Helper.exe` | Share.exe / GFE | browser helper + service discovery (port 65062 publish) | — |
| **NVIDIA Share.exe** ×3 | GFE / ตัวเอง | CEF host — โหลด osc/index.html, ตอบ QUERY_WIN_NODE_INFO | — |
| `nvspcap64.exe` | **Share.exe เมื่อ overlay เปิด / เริ่มอัด** | ตัวอัดจริง (driver capture) | ✅ ON-DEMAND (ไม่รันก็ตอบ state ได้!) |

## 2. Controller API — 70 endpoints (ครบตามที่ใช้จริง)

Server: `http://127.0.0.1:<dynamic>/` + header `X_LOCAL_SECURITY_COOKIE: <secret>`

Secret + port: **dynamic ต่อ NvNode restart** — Share.exe ได้รับผ่าน IPC
ตอนสตาร์ท ("Node info request success") และถ่ายทอดให้ osc ผ่าน
QUERY_WIN_NODE_INFO

### Endpoint groups (เต็มรายการใน log)

- **State/Status**: Record/Running, Record/State, InstantReplay/Running+Enable,
  Capture/State, Capture/ProcessInfo/<id>, Nis2/state, DeepDVC/state
- **Control (POST)**: Record/Enable, InstantReplay/Enable+Save, Launch,
  Osc, OSC/MainView, Hotkey/DynamicToggle, SDK/NotifyOverlayState
- **Capability**: GetSupported, DesktopCapture/Support(+Reason), Screenshot/Support,
  Webcam/Present+Enable, Microphone/Present, Broadcast/2KSupport,
  Record/Concurrency/*, NvCamera/* (Compatible/ReshadeSupported/...)
- **Hotkey states**: Hotkey/RecordToggle, RecordSave, Screenshot, DVRToggle,
  BroadcastToggle+PauseToggle, CameraToggle, NvCameraUI, ModsUI,
  pmocoverlay, pmocsidebar (GET → GFE ส่ง hotkey config เป็น data)
- **Hardware**: HardwareInformation/v.0.1+v.0.2(+generic), SignedGPUID
- **Config/Account**: PiplConfig/data, Settings/Language+globalConfig,
  Account/UserToken+PrivacySettings, beta, gfeupdate/*, geoLocation
- **Overlay lifecycle**: Indicator/record+viewer (Support/Settings),
  CoPlay/Enable, Highlights/*

## 3. รูปทรง response จริง (จับได้แล้วใน real-controller-responses/)

146 ไฟล์จาก console.log (ฝั่ง client) + ตัวอย่างจาก nvnode.log (ฝั่ง server)
— สำคัญที่สุด:

```
DesktopCapture/Support/Reason → {"support":true}
Record/Running                → {"running":false}
HardwareInformation/v.0.2     → {"MoboType":"Desktop","BIOSVersion":"...","PhysicalMemoryCapacity":"17179869184","JarvisDeviceId":"..."}
PiplConfig/data               → {jarvis/gfwsl/aem/vrs/jsEvents/telemetry servers}
```

## 4. Security model (จริง)

- Secret = **rotates ต่อ NvNode restart** (จับได้: 1F20...→966E... ภายในชั่วโมง)
- ส่งผ่าน header `X_LOCAL_SECURITY_COOKIE` ทุก request (รวม socket.io query)
- **Web Helper = publisher**: เป็นคนแจก {port, secret} ให้ Share.exe ตอนสตาร์ท

## 5. สิ่งที่ BYPASS ได้แล้วด้วยระบบเรา (สถานะปัจจุบัน)

| NVIDIA ตัวจริง | ของเรา | สถานะ |
|---|---|---|
| Share.exe (window) | Overlay.Engine WebView2 | ✅ |
| NvNode controller (70 endpoints) | OscControllerServer + FULL MODE (146 real responses) | ✅ data-driven |
| piplConfig (NVIDIA cloud) | Duluka mod (apply.ps1) | ✅ applied |
| jarvis account | Duluka stub (:59870) | 🟡 รอ shapes จากการล็อกอิน |
| nvspcap64 recorder | **ใช้ตัวจริง (on-demand)** หรือ Duluka Capture | ✅ ยืดหยุ่น |

## 6. สิ่งที่ยังต้องเรียนรู้เพิ่ม (UNKNOWN)

- Shapes ของ jarvis login/profile (รอ log จากการล็อกอินผ่าน stub)
- Recorder start เงื่อนไขอะไรบ้าง (GFE settings ไหนบังคับ)
- Socket.IO push events ที่ NvNode ส่งเอง (นอกจากที่ page subscribe)
