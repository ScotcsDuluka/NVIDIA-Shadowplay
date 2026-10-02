# ShadowPlay engine dissection — full folder map (GFE 3.28.0.412 official payload)

> แกะทุก PE ใน `ShadowPlay\` จาก payload แท้ (dumpbin exports/dependents + strings)
> ต่อจาก docs/osc/20 — ที่มา: session Intel 2026-09-25

## ภาพรวมสถาปัตยกรรม

```
nvsphelper64.exe (engine host, 824KB — ไม่มี export; โหลด plugins)
 ├─ nvsphelperplugin64.dll .... Hotkey plugin (docs/osc/20 §4)
 ├─ _nvspcaps64.dll ........... SP server/caps plugin — ⭐ มี ShadowPlayMockServer
 ├─ _nvspserviceplugin64.dll .. service plugin (spawn process ใน user session, ETW)
 ├─ capcore64.dll ............. capture pipeline (NvCreateCaptureCore)
 │    ├─ frame source: nvfp64.dll → NvFBC (driver-bound) หรือ DDA (DX11 Duplication)
 │    ├─ stages: camera / logo / encoder (DXVA ProcAmp, HDR flag)
 │    ├─ audio: nvaudcap64v.dll (driver component)
 │    └─ mux: nvmf64.dll (NvCreateMPEG4MuxSink — h264/hevc + AAC)
 ├─ NvRemux64.dll ............. MF remuxer + Trim (Instant Replay highlight)
 ├─ NvRtmpStreamer64.dll ...... RTMP publish (Twitch) — OpenSSL
 ├─ nvspscreenshot64.dll ...... screenshot (รวม HDR แบบ JXR/WIC)
 └─ nvspapi64/x64.dll ......... CShadowPlayApi (CreateCaptureSession ฯลฯ) — IpcSyncCall
      ↕ ipccommon64.dll (MessageBus client — PROTOBUF, libprotobuf.dll)
        ↕ nvcontainer.exe / NVIDIA Share.exe / NVIDIA Web Helper.exe

nvspcap64.dll / nvspcap.dll (ติดตั้งลง System32/SysWOW64 — hook เข้าเกม!)
  exports: CreateShadowPlayProxyShimInterface, QueryShadowPlayDdiShimInterface,
           QueryShadowPlayDdiShimStatus
  - CSPShareStateManager: per-process capture state ผ่าน MMF slots (rotating
    "New MMF opened with id [%d]" / OpenLatestMMF)
  - CSPOscOverlayRenderer: render overlay IN-GAME — ภาพจาก backend มาทาง
    Pixel MMFs (OpenPixelMMFs/ProcessSlot) แล้ว composite ลน swap chain เกม
  - CSPOverlayRenderer: FPS counter, viewer count (broadcast) MMF
  - NvFBC per-PID capture ("NvFBC created for capture type[%d] with PID[%d]")
```

## ตารางรายไฟล์

| ไฟล์ | export | บทบาท | หลักฐานเด่น |
|---|---|---|---|
| nvsphelper64.exe | — | engine host, โหลด plugins, อ่าน Cfg2/Cfg3 | อ้างชื่อ Share.exe / nvcontainer.exe / Web Helper.exe |
| capcore64.dll | NvCreateCaptureCore | pipeline จับภาพ: source→stages→encoder | eVFrameSourceNVFBC **และ** eVFrameSourceDDA, GamecastMode (broadcast+record FPS คู่), d3d9/dxva2/nvaudcap64v |
| nvfp64.dll | FrameProviderCreateInterface | frame providers | `CFrameProviderFBCToDx9` (NvFBC) + `CFrameProviderDDA` (DDAImpl::Init/GetCapturedFrame/ScaleCrop) — "DDA does not support PID capture" |
| nvspcap64.dll / nvspcap.dll | ProxyShim/DdiShim ×3 | hook ในเกม: state MMF + overlay renderer + screenshot | Pixel MMFs, OVLY viewer-count MMF, NvFBC per-PID |
| nvspapi64.dll / nvspapix64.dll | CreateOverlayApiInterface, CreateShadowPlayApiInterface | public API ให้ Share/GFE เรียก | CShadowPlayApi::Create/DestroyCaptureSession (version-checked, IpcSyncCall), telemetry SPStat_*, enum eCaptureMode/eSPCaptureState/eSPToggleOrigin, gate `Global\ShadowPlay\NVSPCAPS → IsShadowPlayEnabled / IsShadowPlayEnabledUser` |
| ipccommon64.dll | CreateIpcClient, CreateIpcProxyInterface | MessageBus client | **protobuf** (BusMessage.pb.cc), join/leave "System/Module", CaptureCore.log |
| _nvspcaps64.dll | NvPluginGetInfo | SP server plugin | ⭐ **ShadowPlayMockServer** implement session API ครบ (Create/Destroy/Set/GetCaptureSession*, CaptureSessionControl) แบบไม่จับจริง + CServerImpl ("Restarting OSC...", "SharedDataMMF invalid → Disabling Shadowplay", ToggleHotKeyDetection) |
| _nvspserviceplugin64.dll | NvPluginGetInfo | service-side plugin | ShadowPlayController::CreateProcessInSession (WTSQueryUserToken → CreateProcessAsUser — StartHotkeyProcess / LaunchNvContainer ในนาม user session), ETWController (trace วินิจฉัย) |
| nvspscreenshot64.dll | NvCreateScreenshotInterface | screenshot | MMF ScreenShotMMF, HDR → **JXR (WIC, metadata Xbox)**, gdiplus |
| NvRemux64.dll | CreateInstance | remux/trim | CNvMFTMuxSink (MF MediaSink สองแบบ Win/Nv), CMFTRemux::Trim (Instant Replay) — h264/hevc + AAC |
| NvRtmpStreamer64.dll | NvCreateRTMP, NvRtmpTest | broadcast | RTMP amf3 (createStream/NetStream.Publish.Start), OpenSSL 1.1, NvRtmpScheduler |
| nvmf64.dll | NvCreateMPEG4MuxSink | MP4 mux sink | CNvMediaMuxSink + CMediaStream (h264/hevc/AAC, SPS/PPS, ลบไฟล์เสียอัตโนมัติ) |
| ShadowPlayExt.dll | COM (DllGetClassObject ฯลฯ) | installer extension | CaptureServer lifecycle ตอน install/uninstall ("nvspcap.exe close failure requires reboot") |

## ข้อสรุปสำคัญต่อโปรเจกต์เรา

1. **MMF คือศูนย์กลาง IPC ของ engine** (state slots หมุนเวอร์ชัน, Pixel MMF สำหรับ
   overlay in-game, ScreenShot MMF, viewer-count MMF, OVLY-COMMON สำหรับ UWP) —
   control-plane ใช้ MessageBus (protobuf) ผ่าน nvcontainer
2. **nvspcap.dll ที่เราทำ placeholder** หน้าที่จริง = DDI shim ในเกม +
   per-process capture state + **render overlay in-game จาก Pixel MMF** —
   เว้นแต่ง overlay ผ่าน window แยกแบบเรา ตัวจริงวาด "ใน" เกม
3. **`_nvspcaps64.dll` พิสูจน์ว่า NVIDIA แยก session API ออกจาก driver capture**
   — เขามี MockServer ที่ implement session ครบโดยไม่จับจริง → สถาปัตยกรรม
   CaptureEngine เรา (session API + DDA + ffmpeg mux) เดินตามรอยเดียวกัน
   โดย DDA ของเรา = `CFrameProviderDDA` ของเขา (มีข้อจำกัดเหมือนกัน: ไม่ support
   PID capture)
4. **ครบทั้ง Instant Replay แบบ native**: capcore (ring) → NvRemux64::Trim —
   เส้นทางนี้ใช้ NVENC+MF mux ของเขา; เราใช้ ffmpeg backend (LM-SEP known-fail
   อยู่ที่ mux ตัวเดียวกันเชิงแนวคิด)
5. Hotkey config อยู่ registry `Global\GFExperience\ShadowPlay` Cfg2/Cfg3 —
   settings page ของเราควรจับ key ชุดนี้ (GFEOverlayHKeyV2 ฯลฯ) ให้ตรง GFE

## ข้อจำกัดบนเครื่อง Intel (สรุปซ้ำ)

- NvFBC/capcore/nvaudcap = driver-bound → จับภาพจริงด้วย engine แท้ไม่ได้
  (DDA provider มีใน nvfp64 แต่ต้องโหลดผ่าน nvsphelper ซึ่งถูก gate ที่
  IsShadowPlayEnabled/NVSPCAPS)
- ทางของเรา: CaptureEngine.Ddagrab/FFmpegBackend ของเราครอบ DDA+ffmpeg อยู่แล้ว
  (Video.Tests 66/66 บน 1080 Ti; DDA tests env-skip บน Intel เพราะ gate NVIDIA
  adapter — ต้องทำ hardware-gate แบบ _nvspcaps64 MockServer แนวเดียวกัน)
