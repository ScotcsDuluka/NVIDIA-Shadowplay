# ShadowPlay-related payload — final sweep (round 3) + configs — 2026-09-25

> ปิดชุด docs/osc/20-22 — ครบทุก PE + ทุก config ที่เกี่ยวกับ ShadowPlay
> แหล่ง: `C:\My Project\gfe-3.28.0.412-extract\`

## 1) ชิ้นที่เหลือ (รอบ 3)

| ไฟล์ | export | สรุป |
|---|---|---|
| NvContainerInternal.exe | NvOptimusEnablement | NvContainer ตัว internal + **telemetry helper** (InitTelemetry/DeInitTelemetry, \NvcTelemetryHelP) |
| NvBackendAPI64.dll | **132 exports** | ⭐ API lib ของ GFE main: Application_GetSettings/GetSliderSettings/OptimizeForSDK/SetBattery/RevertOPS/RegisterStateChangedCallback + **HTTP-over-named-pipe** (`\\.\pipe\`, `http_over_pipe://`, "Server: NVIDIA Stream HTTP server/2008") — ใช้คุยเรื่อง game settings/Optimize/Streaming |
| NvBackendExt.dll | COM ×5 | installer ext — telemetry consent |
| NvBatteryBoostCheck64.dll | NvPluginGetInfo | **BatteryBoost** — เงื่อนไขแบตเตอรี่ก่อน record (โน้ตบุ๊ค!) |
| NvDriverUpdateCheck64.dll | NvPluginGetInfo | เช็ค driver ผ่าน **NvAPI_DRS** (driver profiles) |
| GFExperienceExt.dll | COM ×5 | installer ext — ติดตั้ง/rollback driver |
| OSCExt.dll | COM ×5 | installer ext ของ package OSC |
| GfeXCode64.dll | 14 exports | ⭐ **image processing ด้วย CUDA + NVSDK_NGX_CUDA_*** (GfeXcodeFunc/Image/Montage) — โซ่ NGXShot / Ansel-style screenshot |
| NvStreamSrvExt.dll | COM ×5 | installer ext ของ GameStream (gs_04_50) |
| NvTelemetry64.dll | 29 exports | telemetry SDK เต็มตัว (protobuf + **opentracing/LightStep**) |
| NvTelemetryAPI64.dll | 25 exports | API บาง (Init/SendEvent/SendFeedback/DeviceId — WMI device id) |

หมายเหตุ: `NVGalleryAPINode.node` ไม่ได้อยู่ใน GFExperience\ — อยู่ใน nodejs\ (ตามที่ docs/osc/22)

## 2) config ที่อ่านได้ (สำคัญต่อ backend เรา)

- `nodejs\config.json`:
  - gfservices → `https://ota.nvidia.com/GFE/` (v1.0)
  - jarvis → clientId `135333107684344109`, description "NVIDIA Web Helper"
  - **gxtarget** → `gx-target-experiments-frontend-api.gx.nvidia.com` (cloud
    variables / feature-flag: piplConfigCvName=GfePiplConfig,
    IsMandatoryUpdate) — กลไก remote-config ของ GFE
- `NvBackend\config.json`: product GFE 3.28.0.412 + openTracing
  `https://lightstep.kaizen.nvidia.com/api/v2/reports`
- `NvBackend\FeatureWhitelist.json`: whitelist รายเกม —
  `cms_appId → isShadowPlaySupported / isReflexIntegrated / isFreeStyleSupported /
  isNgxSupported` — เกมบางตัวถูกปิด ShadowPlay ชัด ๆ จาก server-side list

## 3) สถานะความครบถ้วนของการแกะ (ที่เกี่ยวกับ ShadowPlay)

| กลุ่ม | สถานะ |
|---|---|
| ShadowPlay\ (engine 15 PE รวม hook/API/plugins) | ✅ ครบ (docs/osc/21) |
| hotkey plugin เจาะลึก | ✅ (docs/osc/20 §4) |
| nodejs\ backend จริง (index.js + NvShadowPlayAPI.js + .node bridges + launcher + Web Helper) | ✅ (docs/osc/22) |
| NvContainer แกน (exe + MessageBus + Watchdog + Internal) | ✅ (22 + 23) |
| NvBackend ทั้งชุด (API/Ext/BatteryBoost/DriverUpdateCheck/TmRep/SHIM) | ✅ (22 + 23) |
| GFExperience hosts (Share == GFE == Notification, .json, ext dlls, GfeXCode) | ✅ (22 + 23) |
| NvStreamSrv (GameStream ext) | ✅ surface-level (ext; amd64 body เป็น GameStream ไม่ผูก ShadowPlay โดยตรง) |
| NvTelemetry ทั้งชุด | ✅ surface-level (telemetry ไม่ได้เป็นตรรกะ ShadowPlay) |
| configs อ่านได้ (config.json ×2, FeatureWhitelist, .nvi manifests) | ✅ ตัวสำคัญครบ |
| ไม่แกะ (นอกขอบเขต ShadowPlay โดยตรง) | Display.Update/Optimus, MSVCRT, NVI2 (installer infra), FrameViewSDK, NvVAD/NvvHCI, Update.Core — เป็น runtime/infra ทั่วไป |

## 4) สิ่งที่ payload ชุดนี้สอนเพิ่มเติม

1. **HTTP-over-named-pipe** (`http_over_pipe://`) — รูปแบบ IPC ที่ GFE ใช้
   คุยกับ backend ภายใน (นอกจาก TCP 59001 และ MessageBus) — ทางเลือกเสริม
   ถ้า backend เราอยาก loopback แบบไม่เปิดพอร์ต
2. **gxtarget cloud variables** = feature flags รายเกม/รายผู้ใช้ — osc ปัจจุบัน
   ของเรา hard-code ทั้งหมด; ถ้าจะเลียนระดับ production ต้องมีแหล่ง config กลาง
   (จริง ๆ เรามี providers.default.json + Duluka server แล้ว — เสริม pattern
   เดียวกันได้)
3. **FeatureWhitelist รายเกม** — อธิบายว่าทำไมบางเกม "ไม่มี ShadowPlay" ใน GFE
   — เกณฑ์ server-driven ไม่ใช่ detection ฝั่ง client เท่านั้น
4. **GfeXCode + NGX CUDA** — ภาพ screenshot ระดับ NVIDIA (NGXShot ที่เห็นใน
   endpoint) ประมวลผลด้วย CUDA/NGX — บน Intel ทางเรา (WIC/JXR + ffmpeg) คือ
  ทางที่ถูกต้องอยู่แล้ว
