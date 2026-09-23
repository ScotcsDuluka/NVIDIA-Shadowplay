# OSC REQUIREMENTS INVENTORY (v1 — lead extraction 2026-09-24)

Source evidence pulled from the LIVE system + user's A:\ backup (GFE 3.28.0.412).
Evidence copies: `docs/reference/osc/`. ZCode deep-extraction of minified
`app.js` (1.6 MB) queued via `taskboard/OSC-ZCODE-MISSION.md`.

## 0. Topology (what talks to what)

```
osc WebView (Angular, index.html -> vendor.js + common.js + app.js;
             config.js + user-config.js inject OSC_CONFIG / OSCCLIENT_USER_CONFIG)
   |  HTTP + socket.io (windowName "shareclient")
   v
Node host inside NvNode ("NVIDIA Web Helper.exe", backend :59001)
   |  Express routes registered by nodejs/*.js  +  io.emit() notifications
   v
native modules (NvShadowPlayAPINode.node, NvBackendAPINode.node, ...)
   |  api.* calls
   v
ShadowPlay capture services
```

Our rebuilt backend must serve the Express surface below for the REAL osc
page to function. The engine already implements a working subset
(Record/Enable, Record/Settings, RecordPaths, FrameRates, Resolutions,
Duluka state/actual, Hotkey, InstantReplay stubs, Broadcast stubs,
Audio, Microphone, Highlights, DesktopCapture, SystemInfo, HardwareInfo).

## 1. Keys / ids (osc/config.js — OSC_CONFIG)

| Purpose | Key |
|---|---|
| osc jsEvents oscClientId | `11615747663535760` |
| jarvis clientId | `144326972728672375` |
| gfeClientId (shared w/ nodejs config.json) | `135333107684344109` |
| Twitch clientId | `cxcbwgocuez8axmeqe0cmz29ivvxjt2` |
| Imgur clientId | `af3ff87d603599e` |
| Google OAuth clientId | `954449761280-u6t6u90f9okkk4buhesve4gfn4lrc5u4.apps.googleusercontent.com` |
| Facebook clientId | `1679326302390196` |
| Weibo clientId | `3774042265` |
| Shot With GeForce clientId | `163900107807260888` (api-prod.nvidia.com) |
| userAgent | `NVIDIAOSCClient` |
| windowName | `shareclient` |
| auth header (backend, pinned) | `X_LOCAL_SECURITY_COOKIE` |

OSC_BUILD_INFO: oscPackageVersion `3.28.0.412`, branch `rel_03_28`,
gitHash `ffe15e48d9`, buildType `prod`.

jsEvents: schemaVersion `2.7`, maxRetries 2, msBetweenRetries 1000,
defaultTimeout 30000, msBetweenSendRequest 5000, maxEventsPerRequest 128.

Portal quality bounds (broadcast supported_settings, fps -> resolutions):
- Twitch: 30/60 -> 360p/480p/720p HD/1080p HD, bitrateMax 9 (2K 9)
- YouTube: 30 -> 360p..1440p HD; 60 -> 720p/1080p/1440p, bitrateMax 9 (2K 18)
- Facebook: 30 -> 360p/480p/720p, bitrateMax 4, max_duration 240
- redirectUrl: https://rds-assets.nvidia.com/main/redirect/share-redirect.html

## 2. Express surface of the node host (NvShadowPlayAPI.js, 2700 lines)

`RegisterExpressEndpoints(app, io, logger)` registers ~100 routes under
`/ShadowPlay/v.1.0`, `/ShadowPlay/v.1.1`, `/ShadowPlay/v1.0/OSC/GetCustomize/*`
(full list: docs/reference/osc/ + extraction below). Notifications go to the
page via `io.emit(name, data)` (dynamic event names from native callbacks:
SetHotkeyCallback, SetGeneralNotificationCallback,
SetOscCaptureStateChangeNotificationCallback,
SetOscWindowStateChangeNotificationCallback, SetBroadcastSessionNotificationCallback...).

GET routes:
4KSupport, 8k60, Audio, AudioSettings, BitRates/:quality/:resolution,
Broadcast/{2KEnable,2KSupport,Enable,FBLiveSupport,IngestServer,LastProvider,
Provider,Running,Settings,Support,Title}, Capture/{PIDMode,ProcessInfo/:PID/,State},
CoPlay/{Enable,Support}, CustomOverlay/{DefaultPath,Display,Enable,Path,Support},
DesktopCapture/{Enable,Support,Support/Reason}, Framerates, Framerates/:quality/,
GetHDRState, Highlights/{Customize,Session}, Hotkey/:hk,
Indicator/:id/{Settings,Support}, InstantReplay/{BufferLength,Enable,Running,Settings},
Launch, Microphone, Microphone/:index/Settings, Microphone/Present,
Microphone/Settings, OSC/Init, Record/Concurrency/:mode, Record/Enable,
Record/Running, Record/Settings, RecordPaths, Resolutions, Resolutions/:quality/,
Screenshot/Support, Webcam/{Enable,Present,Settings,Shown},
v.1.1/CustomOverlay/{Enable/:index,Path/:index},
v1.0/OSC/GetCustomize/{Broadcast,InstantReplay,Record}

POST routes:
Audio, AudioSettings, Broadcast/{Enable,IngestServer,LastProvider,Pause,
Provider,SessionParam,Settings,Viewers,Viewers/Max}, CoPlay/Enable,
CustomOverlay/{Enable,Path}, DesktopCapture/Enable, GetSupported,
Highlights/{Customize,GalleryImport}, Hotkey/:hk, Indicator/:id/Settings,
Input, InstantReplay/{Enable,Save,Settings}, Launch, Microphone,
Microphone/:index/Settings, Microphone/PTT, OSC/MainView, OpenOsc,
OpenOscPreferences, OpenOscState, Osc, OscNotification, Record/Enable,
Record/Settings, RecordPaths, Screenshot/{Capture,NGXCancelShot,NGXShot},
Video/Trim, Webcam/{Enable,Settings,Toggle},
v.1.1/Broadcast/SessionParam/:type, v.1.1/CustomOverlay/{Enable/:index,Path/:index}

Native api.* surface (NvShadowPlayAPINode.node): BroadcastAction,
InstantReplayAction, GetWebcamStatus, SetBroadcastSessionParam,
RecordingPaths, MultipleCustomOverlayPath, MultipleCustomOverlayEnable,
IndicatorOverlaySettings, HotKeyMonitor, HotKey, HighlightsCustomize,
GetQualityDefaultData, GetMicSettings, GetManualRecordStatus,
GetInstantReplayStatus, GetBroadcastStatus, DesktopCaptureEnable,
CustomOverlayPath, CustomOverlayEnable, CoplayEnable,
BroadcastTwitchIngestServer, BroadcastProvider, AudioSettings, AudioMode,
WebcamToggle, WebcamOverlaySettings, WebcamEnable, TrimVideo,
ShadowPlaySupported, ShadowPlayEnable + the notification-callback setters.

## 3. Remaining node-host route files (same Express app)

| File | Routes | Notable |
|---|---|---|
| NvBackendAPI.js | 63 | backend health/session/misc |
| NvCameraAPI.js | 47 | camera/Ansel |
| NvGalleryAPI.js | 19 | gallery store (phase 5 handoff) |
| NvAccountAPI.js | 8 | account/user token |
| NvAbHubAPI.js | 4 | A/B hub |

nodejs/config.json: gfservices OTA `https://ota.nvidia.com/GFE/` v1.0,
jarvis clientId `135333107684344109`, gx-target experiments
`gx-target-experiments-frontend-api.gx.nvidia.com` v3 (cloudvariables,
GfePiplConfig, IsMandatoryUpdate).

index.js.patched.ours == index.js + `console.log('ENTRY_MARKER', __dirname)`
(old debug artifact, no functional delta).

## 4. Open items

- [ ] app.js deep-extraction (ZCode mission): request/response shapes the
      PAGE builds for the routes above, l10n key -> settings field map,
      client-side state machine, socket.io event names actually consumed.
- [ ] NvBackendAPI/NvGalleryAPI route bodies (phase 4/5 dependencies).
- [ ] BitRates/:quality/:resolution + GetQualityDefaultData bounds table
      (encoder-quality authority the osc page trusts).
