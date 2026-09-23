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

- [x] app.js deep-extraction (ZCode mission): done 2026-09-24, results in
      docs/OSC-APPJS-EXTRACTION.md + section 5 below.
- [ ] NvBackendAPI/NvGalleryAPI route bodies (phase 4/5 dependencies).
- [ ] BitRates/:quality/:resolution + GetQualityDefaultData bounds table
      (encoder-quality authority the osc page trusts).

## 5. app.js findings (ZCode) — 2026-09-24

Full detail + evidence anchors (charOffset into app.js): docs/OSC-APPJS-EXTRACTION.md.

### 5.1 Routes the PAGE calls that were missing from v1 section 2

The page talks to **15 endpoint families**, not just `/ShadowPlay/*`. URL scheme
(app.js @79860): `{host}{port}/{family}/{version}{url}` where host/port/secret
come from a CEF IPC handshake (`QUERY_WIN_NODE_INFO` @413120 → `{port, secret,
active}`), defaulting to `http://localhost:3000`. Missing from v1:

- POST `/ShadowPlay/v.1.0/InstantReplay/Upload` (@589495)
- GET+POST `/ShadowPlay/v.1.0/Hotkey/Monitor`, POST `/Hotkey/DynamicToggle` (@596096, @596155, @596935)
- `/SDK/v.1.0/*` — 11 Highlights routes (@580708-581556): Highlights/{Active,
  RecoverSpace, GetConfig, SetConfig, Enable, GetRecent, GetGamesConfig,
  GetHighlights}, {Get,Set}Permissions, NotifyOverlayState (+ socket `/SDK/v.1.0/Notification`)
- `/QuietMode2/v.1.0/{support,state}` (whisper mode; POST {enabled, baseFrameRate, fanVolume}) @358527
- `/GameShare/v.1.0/*` — CoPlay sessions: CreateSession POST,
  ConfigureControllerMapping PUT, ModifySession/:id PUT, Session/:id DELETE,
  FullScreenProcessId/:activeWindow GET (@576499-576867)
- `/Nis2/v.1.0/…` (sharpen) + `/DeepDVC/v.1.0/…` (vibrance) state GET/POST (@582269, @577461)
- `/Feedback/v.0.1/` POST, `/HardwareInformation/v.0.1/` GET (@578091, @580206)
- `/PiplConfig/v.1.0/data` GET (@587359)
- `/Settings/v.1.0/Language` GET+POST + bare `/beta` GET (@587867, @588192)
- `/gfeupdate/autoGFEDownload/autoGFEbeta` GET (@578843 — no version segment)
- `/Account/v.1.0/UserToken` + `/PrivacySettings` GET+POST (`vendor:172850`; served on `localhost:3000`)
- `/NvCamera/v.1.0|v.1.1/*` — 39 Ansel/Freestyle endpoints (@583027-585962)
- `/abHubAPI/v.0.1/{Post,Add,Delete,Status}` (@575608)

Registered in v1 §2 but **never called by the page**: POST `/GetSupported`,
GET `v1.0/OSC/GetCustomize/*` (strings absent from app.js). Gallery browsing
uses CEF IPC (`QUERY_WIN_DIR_INFO`), not HTTP. CoPlay streaming uses
`wss://…:47984` WebRTC (@349194).

### 5.2 Socket events actually consumed (27)

socket.io connects to the same node host with `{query:{X_LOCAL_SECURITY_COOKIE:
secret}}` (@81217). **Every backend `io.emit` name must be the literal Express
route path** (corrects v1 §0/§2 "dynamic event names"):
`/ShadowPlay/v.1.0/{InstantReplay/Enable, InstantReplay/Started,
InstantReplay/Save, InstantReplay/Upload, Record/Enable, Notification,
DisplayOscNotification, Hotkey, WindowState, DisplayOscPreferences,
DisplayOscState, Broadcast/Enable, Broadcast/Pause, Broadcast/SessionEvent}`,
`/GameShare/v.1.0/{SessionUpdate, CreateSession}`,
`/NvCamera/v.1.0/Notifications`, `/SDK/v.1.0/Notification`,
`/QuietMode2/v.1.0/{state, support}`, `/Account/v.1.0/{UserToken,
PrivacySettings}`, `/abHubAPI/v.0.1/{Message, Status}`,
`/PiplConfig/v.1.0/update`, `/Settings/v.1.0/Language`,
`/gfeupdate/autoGFEDownload/autoGFEbeta`.

### 5.3 New keys / state facts (page-side model)

- Quality ids: `Average|Good|VeryGood|UltraGood|Custom` (UI: Low/Medium/High/
  Ultra/Custom); pickers: Resolutions `In-game,4320p 8K,2160p 4K,1440p HD,
  1080p HD,720p HD,480p,360p` (broadcast caps at 1080p HD), Framerates 30/60;
  IR length 0.25-20 min; bitrate slider 10-130 Mbps local, 1-9 (18 for 2K)
  broadcast; POST payload `{quality[,resolution,framerate,bitrateBps]
  [,replayLengthSeconds][,provider]}`.
- `BitRates/:quality/:resolution` is called with portal-prefixed quality for
  broadcast (`Gamecast`/`GamecastYtl`/`GamecastFbl` + quality) — config.js
  `connect.*.qualityPrefix`.
- l10n: 673 flat keys in `l10n/en-US.json` (BOM-prefixed), loaded as
  `$translate` part `l10n`; settings-screen key map in OSC-APPJS-EXTRACTION §D.
- Telemetry: POST `{jsEvents.server}/v1.0/events/json` with
  `X-Event-Protocol: 1.1` — server is `""` in this build (inert).
