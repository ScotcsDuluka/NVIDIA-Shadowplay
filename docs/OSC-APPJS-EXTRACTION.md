# OSC app.js DEEP-EXTRACTION (ZCode mission 2026-09-24)

Source: `C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\WebView\osc\app.js`
(1,596,161 chars, 45 physical lines, webpack-bundled Angular 1.x; vendor.js 1.43 MB
holds socket.io-client, NvEndpoint/NvEndpointFactory, nvAccount SDK, jsEvents SDK;
common.js is only the webpack chunk loader). **All offsets are 0-based charOffset
into app.js unless prefixed `vendor:` or `config.js:`.** Claims were verified by
string-slice reads around each offset; nothing under `C:\Program Files\...` or
`A:\NV OSC Backup\...` was modified.

Key to repeated anchors:

- `E-FACTORY` = `newLocalEndpointFactory` def @79533..79900: URL =
  `nodeConfig.server + nodeConfig.port + "/" + family + "/" + version + endpoint.url`,
  headers = request headers merged with `commonHeaders` (carries
  `X_LOCAL_SECURITY_COOKIE`). Generic request engine: `vendor:173800` (NvEndpoint:
  `:param` substitution + query-string encode + retries/timeout).
- `BLOCK-A` = @426997..427700 provider config: defaults `server=LOCALHOST_ADDR`
  (`"http://localhost:"` @1745), `port=LOCALHOST_PORT` (3e3=3000, @1753),
  `commonHeaders:{X_LOCAL_SECURITY_COOKIE:""}`; versions: `v.1.0` →
  shadowPlay/highlights/coplay/settings/nvCamera/gfeUpdates/quietMode2/nis/dvc
  endpoints, `v.0.1` → hardware/feedback/abHub endpoints.
- `NODEINFO` = ui-router state `base` resolve @413050: `cefService.localNodeInfo()`
  → CEF IPC `QUERY_WIN_NODE_INFO` (`vendor:194393`) → JSON `{port, secret, active}`
  → `updateNodeInfo` retargets localSdk + nvAccountEndpoints + ugcLib. **The page
  learns the real node port + security cookie over CEF IPC, not HTTP.**
- `SOCKET-CONN` = socketService @80280: `io(nodeConfig.server+nodeConfig.port,
  {query:{X_LOCAL_SECURITY_COOKIE: cookie}})` @81217, `connect()` from
  `initializeUI` run-block @412050 (`T.connect()`).

---

## A) CLIENT CALLS

185 `createEndpoint({...})` declarations in app.js + 6 in vendor (Account) +
1 (jsEvents). Inventory of every declaration with offset is in the extraction
session; grouped below. Method+URL as declared; "(+s)" = socket push twin in §B.

### A.0 Bootstrap / meta (not ShadowPlay)

| Call | Purpose / response use | Evidence |
|---|---|---|
| CEF IPC `QUERY_WIN_NODE_INFO` | returns `{port, secret, active}`; feeds NODEINFO | @413120, `vendor:194393` |
| GET `http://localhost:3000/Account/v.1.0/UserToken` | `getJarvisUserToken` → `e.data` (user token) | `vendor:172850` |
| POST `/Account/v.1.0/UserToken` `{userToken, userInfo}` | persist token after portal login; then POST PrivacySettings if `dataTracking` | `vendor:171780` |
| GET `/Account/v.1.0/PrivacySettings?userId=` / `?clientId=` | GDPR consent read → `e.data` | `vendor:173010` |
| POST `/Account/v.1.0/PrivacySettings` `{userId, consentSettings}` / `{clientId, consentSettings}` | GDPR consent store | `vendor:172990` |
| GET `/PiplConfig/v.1.0/data` | localized/PIPL config; `piplConfigEndpointsProvider.setConfig(OSC_CONFIG.pipl)` (`server:"PiplConfig"`, `version:"v.1.0"`) | @587359, @430087, `config.js:285` |
| GET `/Settings/v.1.0/Language` | `e.data.language` → sets `$translate` locale | @587867, @300200 |
| POST `/Settings/v.1.0/Language` `{language}` | persist UI language | @587918 |
| GET `/beta` (bare: `{host}{port}/beta`, no family/version) | GFE beta-channel flag (`getBetaPackage`) | @588192, @588060 |
| GET `/gfeupdate/autoGFEDownload/autoGFEbeta` | "experimental features" flag; custom URL gen `{host}{port}/gfeupdate/…` (no version) | @578843, @578700 |
| POST `{jsEvents.server}/v1.0/events/json` (+ `X-Event-Protocol: 1.1`, sync-XHR option) | telemetry batches (localStorage + IndexedDB queue, 5 s interval). `server:""` in this build → inert | `vendor:231240`, `vendor:230700`, `config.js:16` |
| GET `{gfwsl.server}nvidia_web_services/controller.gfeclientcontent.NG.php/com.nvidia.services.GFEClientContent_NG.{methodName}/{params}` | GFE web services (NG); `server:""` in this build | @579667, @579540, `config.js:278` |

### A.1 Record (manual)

| Call | Payload → use | Evidence |
|---|---|---|
| GET `/ShadowPlay/v.1.0/Record/Enable` | `getMREnableStatus` (+s) | @589953 |
| GET `/ShadowPlay/v.1.0/Record/Running` | `getMRRunningStatus` | @590010 |
| POST `/ShadowPlay/v.1.0/Record/Enable` `{status}` | toggle manual record; expect save notification | @590068 |
| GET `/ShadowPlay/v.1.0/Record/Settings` | `getCurrentSettingsMR` → VM `D(e,t)` | @590143 |
| POST `…/Record/Settings` `{quality}` | preset save | @590202 |
| POST `…/Record/Settings` `{quality, resolution, framerate, bitrateBps}` | custom save | @590280 |
| GET `/ShadowPlay/v.1.0/Record/Concurrency/Broadcast` | can record while broadcasting | @590399 |
| GET `/ShadowPlay/v.1.0/Record/Concurrency/Gamestream` | can record during GameStream | @590526 |
| POST `/ShadowPlay/v.1.0/RecordPaths` `{videos, tempFiles}` | folder persistence (folder-browser UI) | @595853 |
| GET `/ShadowPlay/v.1.0/RecordPaths` | folder read (fed to `recordingPathConfigWrapper`) | @595797 |
| POST `/ShadowPlay/v.1.0/Video/Trim` `{input, output, headTrimMs, lengthMs}` | gallery trim | @596608 |
| POST `/ShadowPlay/v.1.0/Input` `{redirect, type, hid}` | HID/keyboard/gamepad redirect (gamepad nav) | @596716 |
| POST `/ShadowPlay/v.1.0/Screenshot/Capture` `{scale, effect}` | screenshot (effect = active filter) | @596295 |
| POST `/ShadowPlay/v.1.0/Screenshot/NGXShot` `{scale, path}` | RTX/AI screenshot | @596385 |
| POST `/ShadowPlay/v.1.0/Screenshot/NGXCancelShot` | cancel AI shot | @596473 |
| GET `/ShadowPlay/v.1.0/Screenshot/Support` | screenshot capability gate | @596232 |
| GET `/ShadowPlay/v.1.0/GetHDRState` | HDR gates HDR capture errors (l10n `HdrScreenshot/Record/Broadcast/HL`) | @590471 |
| GET `/ShadowPlay/v.1.0/4KSupport`, GET `/8k60` | picker ceilings (8K60 extends bitrate ticks) | @594239, @594293, @490220 |
| GET `/ShadowPlay/v.1.0/Capture/State`, `/Capture/PIDMode`, `/Capture/ProcessInfo/:PID` | which fullscreen process is capturable | @595135, @595196, @595279 |

### A.2 Instant Replay

| Call | Payload → use | Evidence |
|---|---|---|
| GET `/ShadowPlay/v.1.0/InstantReplay/Enable` | `getIREnableStatus` (+s) | @589221 |
| GET `/ShadowPlay/v.1.0/InstantReplay/Running` | `getIRRunningStatus` | @589285 |
| POST `/ShadowPlay/v.1.0/InstantReplay/Enable` `{status}` | start/stop IR | @589350 |
| POST `/ShadowPlay/v.1.0/InstantReplay/Save` | save last N min → gallery | @589432 |
| POST `/ShadowPlay/v.1.0/InstantReplay/Upload` | upload IR directly (share flow) — **MISSING from inventory v1** | @589495 |
| GET `/ShadowPlay/v.1.0/InstantReplay/BufferLength` | current buffer length | @589560 |
| GET `/ShadowPlay/v.1.0/InstantReplay/Settings` | `getCurrentSettingsIR` | @589630 |
| POST `…/InstantReplay/Settings` `{replayLengthSeconds, quality}` | preset save | @589696 |
| POST `…/InstantReplay/Settings` `{replayLengthSeconds, quality, resolution, framerate, bitrateBps}` | custom save | @589804 |

### A.3 Broadcast

| Call | Payload → use | Evidence |
|---|---|---|
| GET `/ShadowPlay/v.1.0/Broadcast/Support` | capability gate | @590982 |
| GET `/ShadowPlay/v.1.0/Broadcast/Enable` | status (+s) | @591043 |
| POST `…/Broadcast/Enable` `{status}` | go live / stop | @591103 |
| POST `…/Broadcast/Pause` `{pause}` (+s) | pause/resume | @591181 |
| GET `/ShadowPlay/v.1.0/Broadcast/Running` | running poll | @592220 |
| GET `/ShadowPlay/v.1.0/Broadcast/Settings` | `getCurrentSettingsBR(portalId)` | @591490 |
| POST `…/Broadcast/Settings` `{quality, provider}` | preset save | @591552 |
| POST `…/Broadcast/Settings` `{provider, quality, resolution, framerate, bitrateBps}` | custom save | @591645 |
| POST `…/Broadcast/SessionParam` `{sessionUrl, provider, …}` | session params (pre-v1.1) | @591257 |
| POST `/ShadowPlay/v.1.1/Broadcast/SessionParam/:type` | typed session param (v1.1 twin) | @591359 |
| GET/POST `/ShadowPlay/v.1.0/Broadcast/Title` | stream title | @591779 |
| GET/POST `…/Broadcast/Provider` `{provider}` | selected portal persistence (`getLastUsedService`) | @591838, @591900 |
| GET/POST `…/Broadcast/IngestServer` `{ingestserver}` | Twitch ingest selection (cached by `twitchIngestServerConfigWrapper` @274700) | @591982, @592048 |
| POST `…/Broadcast/Viewers/Max` `{count}` | viewer cap | @592138 |
| POST `…/Broadcast/Viewers` `{ViewerCountImage, ImageHeight, ImageWidth}` | viewer-count OSD image | @593156 |
| GET `/ShadowPlay/v.1.0/Broadcast/2KSupport`, GET `/2KEnable` | 2K gates Ultra + bitrateMax2K | @593275, @593339, @484940 |

### A.4 Gallery (local)

No `/Gallery` HTTP routes are called. Gallery file discovery/thumbnails go through
**CEF IPC**: `cefService.localDirectoryExplorer(includeFiles, filter, Win7dlg)` =
`QUERY_WIN_DIR_INFO` (`vendor:194470`), shared storage via
`QUERY_READ/WRITE_SHARED_STORAGE` (`vendor:194340`), drop-upload via
`oscCreateDropUrl` (@19750). `/Gallery` strings in app.js (@512809, @525471,
@533548, @541376) are ui-router state names only. Uploads go to portals (§A.8).
Gallery video trim is `Video/Trim` (§A.1); Highlights gallery import §A.6.

### A.5 Hotkeys / overlay control

| Call | Payload → use | Evidence |
|---|---|---|
| GET `/ShadowPlay/v.1.0/Hotkey/:hk` | read binding (`hk` = `openshare, ptt, fps, screenshot, …` per `hotKeyMapping` @273100) (+s) | @595940 |
| POST `…/Hotkey/:hk` | rebind (capture dialog) | @596010 |
| GET `/ShadowPlay/v.1.0/Hotkey/Monitor` | **MISSING from inventory v1** | @596096 |
| POST `…/Hotkey/Monitor` `{enable}` | **MISSING from inventory v1** | @596155 |
| POST `…/Hotkey/DynamicToggle` `{enable, hotkeyNames}` | contextual hotkeys — **MISSING from inventory v1** | @596935 |
| GET/POST `/ShadowPlay/v.1.0/CustomOverlay/Enable` `{enable}` / `Path` `{path}` (+s) | OSD overlay enable/path (single-slot legacy) | @592346, @592410, @592492, @592554 |
| GET/POST `/ShadowPlay/v.1.1/CustomOverlay/Enable/:index` / `Path/:index` | multi-slot (v1.1 twins) | @592632, @592721, @592829, @592917 |
| GET `…/CustomOverlay/Support`, `/DefaultPath`, `/Display` | slots + display state | @592281, @593020, @593090 |
| GET/POST `/ShadowPlay/v.1.0/Indicator/:id/Support` / `Indicator/:id/Settings` | per-indicator OSD (fps counter, viewer count, status, camera) | @595522, @595603, @595685 |
| POST `/ShadowPlay/v.1.0/Osc` `{ready}` | page-ready handshake to backend | @596543 |
| GET `/ShadowPlay/v.1.0/OSC/Init` | init fetch | @596801 |
| POST `/ShadowPlay/v.1.0/OSC/MainView` `{fetchPartial}` | main view request | @596854 |
| POST `/ShadowPlay/v.1.0/Launch` `{launch:""}` / GET `/Launch` | ShadowPlay launch + status | @589103, @589171 |

### A.6 Highlights

| Call | Payload → use | Evidence |
|---|---|---|
| GET/POST `/ShadowPlay/v.1.0/Highlights/Customize` GET; POST `{sizeMB}`; POST `{tempSaveFolder}` | disk budget + temp folder | @597033, @597098, @597181 |
| GET `/ShadowPlay/v.1.0/Highlights/Session` | session status | @597424 |
| POST `/ShadowPlay/v.1.0/Highlights/GalleryImport` `{file, property, gameName, groupId, id, headTrimMs, lengthMs}` | import highlight into gallery | @597272 |
| **`/SDK/v.1.0/Highlights/Active` GET** | active highlight state — whole family **MISSING from inventory v1** | @580708 |
| `/SDK/v.1.0/Highlights/RecoverSpace` POST | reclaim disk | @580768 |
| `/SDK/v.1.0/Highlights/GetConfig` POST `{shortName}` / `SetConfig` POST `{enabled,…}` | per-game config | @580835, @580919 |
| `/SDK/v.1.0/GetPermissions` POST `{shortName}` / `SetPermissions` POST `{shortName, permissions}` | per-game permissions | @581001, @581079 |
| `/SDK/v.1.0/Highlights/Enable` GET + POST `{enabled,…}` | global enable | @581172, @581232 |
| `/SDK/v.1.0/NotifyOverlayState` POST `{open, state}` | overlay open state → SDK | @581311 |
| `/SDK/v.1.0/Highlights/GetRecent` POST `{game, maxItems}` | recent highlights | @581397 |
| `/SDK/v.1.0/Highlights/GetGamesConfig` GET | all games config | @581488 |
| `/SDK/v.1.0/Highlights/GetHighlights` POST `{gameName,…}` | list highlights | @581556 |

### A.7 Audio / Mic / Webcam / Desktop capture / CoPlay

| Call | Payload → use | Evidence |
|---|---|---|
| GET/POST `/ShadowPlay/v.1.0/Audio` `{mode}` | mic mode (`ptt | alwayson | off`, TELEMETRY_OSC_MIC_MODE @376442) | @593958, @594008 |
| GET/POST `…/AudioSettings` `{systemVolumePercent, separateTracks}` | volume slider + track separation | @594074, @594132 |
| GET `/ShadowPlay/v.1.0/Microphone` + POST `{mode}` | mic on/off mode | @593465, @593520 |
| POST `…/Microphone/PTT` `{mode}` | PTT enable | @593591 |
| GET `…/Microphone/Present`, `/Microphone/Settings`, `/Microphone/:index/Settings` GET+POST | device enumeration + per-device gain/boost | @593402, @593666, @593730, @593819 |
| GET `/ShadowPlay/v.1.0/Webcam/{Present, Enable}` + POST `Enable {status}` + POST `/Toggle` | webcam overlay power | @594342, @594401, @594459, @594535 |
| GET/POST `/ShadowPlay/v.1.0/Webcam/Settings` `{enable, position, size}` | position (`upperLeft/upperRight/lowerLeft/lowerRight`), size (`small/large`) — l10n @l10n keys | @595364, @595424 |
| GET `…/Webcam/Shown` | current visibility | @594594 |
| GET `/ShadowPlay/v.1.0/DesktopCapture/Enable` + POST `{enable}` + `/Support` + `/Support/Reason` | desktop capture (reason drives l10n warnings) | @594651, @594717, @594801, @594868 |
| GET/POST `/ShadowPlay/v.1.0/CoPlay/Enable` `{enable}` + GET `/CoPlay/Support` | GameStream co-op | @594942, @595000, @595076 |

### A.8 Connectivity portals (external, via oauth proxy / portal services)

No portal API hosts are hardcoded in app.js (`api.twitch`, `graph.facebook`,
`googleapis`, `imgur.com` → NONE). Portal identities/quality come from
`OSC_CONFIG.connect.*` (config.js:43-276): clientId, `qualityPrefix`
(`Gamecast`/`GamecastYtl`/`GamecastFbl`), `bitrateMax`, `bitrateMax2K`,
`supported_settings` (fps→resolutions), privacy options, `max_duration`
(Facebook 240 min). `connectService.endpoints[providerName]` (@485020) adapts
each portal; `getBroadcastQuality(q)` applies `qualityPrefix`. OAuth itself is
proxied (`useOauthProxy: true` for google/facebook/weibo, `redirectUriOverride:
http://localhost:{{portNumber}}`, config.js:100-103). Broadcast start consumes
`Broadcast/Title`, `/Provider`, `/IngestServer`, `/SessionParam` (§A.3).

CoPlay remote play transport is a **WebSocket**, not Express:
`wss://{host}:47984/upgrade?sessionid={id}` (host) and
`wss://{host}/server/{id}` (guest) @349194; controller mapping via
`/GameShare/v.1.0/ConfigureControllerMapping` PUT `{mode}`
(`blocked/mirrored/exclusive/exclusiveLocalPriority` @2095).

### A.9 Ansel / Freestyle / Mods (NvCamera family — 39 endpoints)

`/NvCamera/v.1.0/…` (38) + `/NvCamera/v.1.1/Filter/:id/Attribute` (1).
Service surface (@586213): systemInfo, gameIntegrationFlag, captureTypes,
captureControl, supportedFilterTypes, readyForGameEngine, captureScreenshot,
cancelScreenshot, filters (`/Filter`, `/Filter/Remove`, `/Filter/ResetStack`,
`/Filter/Reset`, `/Filter/ResetAll`, `/Filter/GetInfo`,
`/Filter/:id/Attribute`, `/Filter/:id/SetFilterAndAttributes`,
`/Filter/setMultipleFiltersAndAttributes`,
`/SetFilterAndAttributesSupported`, `/SetMultipleFilterAPISupport`),
camera (`/Camera/GetRange`, `/Camera/GetAdjust`, `/Camera/Adjust` — roll/fov),
capture resolutions `/Capture/GetResolutions/:type`, screenshot
`POST /` `{type, resolutionMultiplier, width, height, panoramaResolutionW,
panoramaResolutionH, saveAsExr, enhance}`, `/Cancel`, integration
(`/GetAvailable`, `/Compatible`, `/GetIntegration`, `/GameEngine`,
`/GetFreestyleSupport` `{profileName}`, `/ReshadeSupported`,
`/GetNvCameraConfig`, `/SetSharpnessForApp` `{sharpness}`),
UI plumbing (`/Language`, `/IPC` `{enable}`, `/EnableMods` `{globalEnable}`,
`/uiReady`, `/uiControlChanged`, `/reportControlVisibility`,
`/GetProcessInfo`, `/GridOfThirds` `{enable}`).
Offsets: @583027..@585962. Notifications: `/NvCamera/v.1.0/Notifications` (§B).

### A.10 Display-enhancement families (missing from inventory v1)

| Family | Calls | Evidence |
|---|---|---|
| `QuietMode2/v.1.0` (whisper mode) | GET `/support`, GET `/state`, POST `/state` `{enabled, baseFrameRate, fanVolume}` | @358527-358625 |
| `Nis2/v.1.0` (image sharpening) | GET `/:cmsId/state`, POST `/state` `{cmsId, enabled, sharpen, selectedResolutionIndex, cmsId}` | @582269, @582340 |
| `DeepDVC/v.1.0` (DLSDR/vibrance) | GET `/:cmsId/state`, POST `/state` `{cmsId, enabled, supported, vibrance, saveToDRS}` | @577461, @577532 |
| `Feedback/v.0.1` | POST `""` `{category, message, email, relatedApplications, relatedFiles}` | @578091 |
| `HardwareInformation/v.0.1` | GET `""` (system description → upload descriptions) | @580206 |
| `GameShare/v.1.0` (CoPlay sessions) | POST `/CreateSession` `{activeWindow, displayName, inviteMode, emailId}`; PUT `/ConfigureControllerMapping` `{mode}`; PUT `/ModifySession/:id`; DELETE `/Session/:id`; GET `/FullScreenProcessId/:activeWindow` (timeout 500 ms) | @576499-576867 |
| `abHubAPI/v.0.1` (A/B experiments) | POST `/Post` `{messageType:"GET", userId, clientName, clientVer, experiments[]}`; POST `/Add`, POST `/Delete`, GET `/Status` | @575608-575987 |
| `Settings/v.1.0` | GET/POST `/Language`; bare GET `/beta` | @587867-588192 |

### A.11 MISSING from inventory v1 §2 (page calls → backend must add)

Page-called routes absent from v1's route list:

1. POST `/ShadowPlay/v.1.0/InstantReplay/Upload` (@589495)
2. GET+POST `/ShadowPlay/v.1.0/Hotkey/Monitor` (@596096, @596155)
3. POST `/ShadowPlay/v.1.0/Hotkey/DynamicToggle` (@596935)
4. Entire `/SDK/v.1.0/…` family — 11 Highlights routes (@580708-581556) incl. `/SDK/v.1.0/Notification` socket twin
5. Entire `/QuietMode2/v.1.0/…` family (3) @358527
6. Entire `/GameShare/v.1.0/…` family (5) @576499 — note `PUT`/`DELETE` verbs
7. `/Nis2/v.1.0/…` (2) @582269; `/DeepDVC/v.1.0/…` (2) @577461
8. `/Feedback/v.0.1/` (1) @578091; `/HardwareInformation/v.0.1/` (1) @580206
9. `/PiplConfig/v.1.0/data` (1) @587359
10. `/Settings/v.1.0/Language` GET+POST (@587867, @587918) + bare `/beta` GET (@588192)
11. `/gfeupdate/autoGFEDownload/autoGFEbeta` GET (@578843)
12. `/Account/v.1.0/UserToken` GET+POST + `/PrivacySettings` GET+POST×2 (`vendor:172850-173060`; host `localhost:3000` from BLOCK-A)
13. `/NvCamera/v.1.0|v.1.1/…` 39 endpoints (@583027-585962) — NvCameraAPI.js exists (§3 of inventory) but its routes weren't in §2
14. `/abHubAPI/v.0.1/*` — file listed in §3, routes not in §2

Backend-only (registered per inventory v1 §2 but **never called by app.js**):
POST `/GetSupported` and GET `v1.0/OSC/GetCustomize/{Broadcast,InstantReplay,Record}`
(exact strings absent from app.js; `getSupported*`/`customize` hits are
page-side function/UI names, e.g. @41305, @484700). They may serve other GFE
surfaces — keep but don't prioritize for osc parity.

Count summary A: 185 endpoint declarations in app.js (+6 Account +1 jsEvents in
vendor.js) across 15 families; 12 route groups missing from inventory v1 §2.

---

## B) SOCKET.IO CONSUMERS

Transport: socket.io-client (protocol 4, `vendor:101297`) →
`io(nodeConfig.server+nodeConfig.port, {query:{X_LOCAL_SECURITY_COOKIE: secret}})`
@81217; connect from `initializeUI` @412050; lifecycle events
`connect/disconnect/error` (@1248 SOCKETIO_EVENTS) are re-broadcast to the
eventAggregator @80360. Registration helper `socketService.register(routePath,
aggregatorName)` @80420 — **every `io.emit` name the backend uses is the literal
Express route path** (this corrects inventory v1 §2's "dynamic event names").

All 27 listener registrations (`socketService.register(...)`):

| io event name (= route path) | Drives | Evidence |
|---|---|---|
| `/ShadowPlay/v.1.0/InstantReplay/Enable` | IR on/off state → `instantReplayNotifier` | @27395 |
| `/ShadowPlay/v.1.0/InstantReplay/Started` | IR recording started → `instantReplayRecordingNotifier` | @27437 |
| `/ShadowPlay/v.1.0/InstantReplay/Save` | save → `instantReplaySaveNotifier` | @27470 |
| `/ShadowPlay/v.1.0/InstantReplay/Upload` | upload done → `instantReplayUploadNotifier` | @27500 |
| `/ShadowPlay/v.1.0/Record/Enable` | record start/stop → `recordNotifier` | @27528 |
| `/ShadowPlay/v.1.0/Notification` | generic notification pump → `processNotification` | @27600 |
| `/ShadowPlay/v.1.0/DisplayOscNotification` | show-OSD notification → `processShowNotification` | @27635 |
| `/ShadowPlay/v.1.0/Hotkey` | hotkey press (payload matched by `hotKeyMapping` @273100: `openshare, ptt, fps, screenshot, RecordSave, …`) → `hotkeyEvent` | @272925 |
| `/ShadowPlay/v.1.0/WindowState` | overlay window open/close → `windowStateEvent` | @70700 |
| `/ShadowPlay/v.1.0/DisplayOscPreferences` | OSD prefs push → `displaySettingsEvent` | @70720 |
| `/ShadowPlay/v.1.0/DisplayOscState` | OSD state push → `displayStateEvent` | @70740 |
| `/ShadowPlay/v.1.0/Broadcast/Enable` | broadcast state → `broadcastNotifier` | @151290 |
| `/ShadowPlay/v.1.0/Broadcast/Pause` | pause state → `broadcastPauseNotifier` | @151335 |
| `/ShadowPlay/v.1.0/Broadcast/SessionEvent` | provider session events (comments, viewers…) → `broadcastSessionEventNotifier` | @151370 |
| `/GameShare/v.1.0/SessionUpdate` | coplay session update → `coplaySessionUpdate` | @170195 |
| `/GameShare/v.1.0/CreateSession` | coplay invite → `coplaySessionCreate` | @170225 |
| `/NvCamera/v.1.0/Notifications` | Ansel/Freestyle notifications → `processNvCameraNotifications` | @219030 |
| `/SDK/v.1.0/Notification` | Highlights events → `highlightsNotifier` | @293900 |
| `/QuietMode2/v.1.0/state` | whisper state → `quietMode2.state.update` | @342890 |
| `/QuietMode2/v.1.0/support` | whisper support → `quietMode2.support.update` | @342918 |
| `/Account/v.1.0/UserToken` | login/logout → `UserTokenChanged` | @275595 |
| `/Account/v.1.0/PrivacySettings` | consent change → `UserConsentChanged` | @144870 |
| `/abHubAPI/v.0.1/Message` | A/B experiment context | @445000 |
| `/abHubAPI/v.0.1/Status` | A/B hub status | @445083 |
| `/PiplConfig/v.1.0/update` | localized/PIPL config push → re-init connect services | @448309 |
| `/Settings/v.1.0/Language` | language change → reload locale | @300406 |
| `/gfeupdate/autoGFEDownload/autoGFEbeta` | experimental-flag change | @300503 |

(All names also available as constants: ACCOUNT_SOCKET_EVENTS @14090,
AB_HUB_SOCKET_EVENTS @14542, PIPL_CONFIG_SOCKET_EVENTS @16082; the ShadowPlay
ones are inline locals at the sites above.)

Internal eventAggregator names the UI actually reacts to (produced from the
notifications above + HTTP polls): SHADOWPLAY_EVENTS @3611 —
`StatusChangeRecord, StatusChangeBroadcast, MicStateChange, WebcamStateChange,
IRRecordingStateChanged, RecordingSaved, CoplayEnabledChanged,
FileReadyToUpload, ViewerCountUpdate, HighlightsStatusChange, GameAppStarted,
GameAppExited, BroadcastDisplayViewerCount, HdrScreenshot`; HOTKEY_EVENTS @2491
(31 hotkey names `Hotkey_OSC … HotKey_ToggleLogging`); WINDOW_EVENTS @3545;
COMMON_EVENTS @8711; QUIET_MODE2_EVENTS @15338; PIPL_CONFIG_SERVICE_EVENTS
@16174; HIGHLIGHTS_EVENTS @11464; NGX_NOTIFICATIONS @11274. Notification
display strings map via NOTIFIER_SELECTIONS @4222 → l10n (§D).

Client→server `emit` is available (`socketService.emit` @80480, with ack
callback) but no page code path emits a named server event besides the connect
handshake — the page is notification-driven, HTTP for commands.

Count B: 27 socket listeners, all route-path-named.

---

## C) SETTINGS STATE

### C.1 Quality model (SettingsCustomizeController @484700)

- API quality ids: `Average, Good, VeryGood, UltraGood, Custom`; UI ids
  `Low, Medium, High, Ultra, Custom`; bidirectional maps @484740/@484760.
  `N.Qualities` @489372: `{id, title:"l10n.low|medium|high|ultra|custom",
  icon:"icon-low|…", supported}` — `Ultra.supported` only when
  `feature===BROADCAST` **and** the portal's `supported_settings` includes
  1080p HD (recomputed in `refreshParams` @492980).
- `N.feature` = OSC_MODE: `InstantReplay | Manual | Broadcast` (@1621).
- Per-portal VM class `z` @490850: `{quality, bitrate, resolution, framerate,
  rememberedResolution, rememberedFramerate, rememberedBitrate, portal,
  updateSettings}`; `N.settingsData.default` for IR/MR, one per portal for
  Broadcast (@494480).

### C.2 Pickers (values + support flagging)

- `N.Resolutions` @489940: ids 1-8 = `In-game, 4320p 8K, 2160p 4K, 1440p HD,
  1080p HD, 720p HD, 480p, 360p`. Broadcast supports only 1080p HD and below
  (`supported:N.feature!==m.BROADCAST` on the first four).
- `N.Framerates` @490070: `30`, `60` (ids 1,2).
- Support flagging @494620: GET `Resolutions` → `e.data.resolutions` names;
  GET `Framerates` → `e.data.framerates`; anything not in the list gets
  `supported=!1`.
- `ReplayLengthSlider` @490100: min .25, max 20 (minutes), step .25, ticks
  [.25,5,10,15,20]; seconds on the wire: `replayLengthSeconds = 60*minutes`
  (`T(e)` @484870).
- `BitrateSlider` @490130: Broadcast min 1, max = `bitrateMax` (9) or
  `bitrateMax2K` (18, Twitch/YouTube per config.js) when `Broadcast/2KSupport`,
  step .5 (1 when 2K); IR/MR min 10 max 130 step 5; tick spacing ×4 when
  `8k60` supported (non-broadcast). Units: UI Mbps → `bitrateBps = Mbps*1e6`
  (`k()` @484850); estimate text uses `60*minutes*bitrate/8` MB @493560.
- Bitrate range authority: `getBitrateRange(quality, resolutionName)` @484905 →
  GET `/ShadowPlay/v.1.0/BitRates/:quality/:resolution` → response `t.data`
  (min/max range). For Broadcast, `quality` is portal-prefixed:
  `connectService.endpoints[portal].getBroadcastQuality(id)` @484940 applies
  `qualityPrefix` (`Gamecast`, `GamecastYtl`, `GamecastFbl` — config.js:52,
  111, 173).
- Defaults: GET `Resolutions/:quality` → `t.data.resolution`
  (`getDefaultResolution` @40850), GET `Framerates/:quality` →
  `t.data.framerate` @40900.

### C.3 Load / save flow

- Load: `getSettingsFunctionCreator` @493080 → `getCurrentSettingsBR(portal.id)`
  / `getCurrentSettingsIR()` / `getCurrentSettingsMR()` (GET Broadcast|IR|Record
  Settings) → `D(e,t)` copies response fields into VM `z`.
- Save (`N.save` @493370): for each VM with `updateSettings` →
  `saveSettingsFunctionCreator` @493400:
  - preset (quality ≠ Custom): `POST {quality}` (+ `replayLengthSeconds` for IR;
    + `provider=portal.portalIdentifier` for Broadcast);
  - Custom: `POST {quality:"Custom", resolution:<name>, framerate:<int>,
    bitrateBps:<bps>}` (+ same extras). Resolution/FPS/bitrate edits force
    `quality=Custom` (`resolutionChanged` @491180, `useDefaultBitRate` flag).
- Dispatch `d.setQualitySettings(quality, feature, n)` @493530 → the
  setInstantReplay/ManualRecord/Broadcast preset/custom endpoints (§A.1-A.3).
- Any change marks `N.settings.updateSettings=!0` (@491080); closing always
  saves when opened from Preferences (`N.back` @493420).

### C.4 Other persisted state (which endpoint stores what)

| Setting | Endpoint + payload | Evidence |
|---|---|---|
| UI language | POST `/Settings/v.1.0/Language` `{language}` | @587918 |
| Experimental/beta flag | GET `/gfeupdate/autoGFEDownload/autoGFEbeta` (read); POST `/beta`-adjacent not used | @578843, @588192 |
| Record folders | POST `/RecordPaths` `{videos, tempFiles}` | @595853 |
| Mic mode / PTT | POST `/Audio` `{mode}`; POST `/Microphone/PTT` `{mode}`; POST `/Microphone/:index/Settings` | @594008, @593591, @593819 |
| Volume / tracks | POST `/AudioSettings` `{systemVolumePercent, separateTracks}` | @594132 |
| Webcam overlay | POST `/Webcam/Settings` `{enable, position, size}` | @595424 |
| Custom overlays | POST `/CustomOverlay/Enable {enable}` + `/Path {path}` (+ v1.1 `/:index` twins) | @592410-592917 |
| Desktop capture | POST `/DesktopCapture/Enable` `{enable}` | @594717 |
| Highlights budget | POST `/Highlights/Customize` `{sizeMB}` / `{tempSaveFolder}` | @597098, @597181 |
| Highlights per-game | POST `/SDK/v.1.0/Highlights/SetConfig` `{enabled,…}`, `SetPermissions` `{shortName, permissions}` | @580919, @581079 |
| Whisper mode | POST `/QuietMode2/v.1.0/state` `{enabled, baseFrameRate, fanVolume}` | @358625 |
| Image sharpening | POST `/Nis2/v.1.0/state` `{cmsId, enabled, sharpen, selectedResolutionIndex}` | @582340 |
| DLSDR/vibrance | POST `/DeepDVC/v.1.0/state` `{cmsId, enabled, vibrance, saveToDRS}` | @577532 |
| Broadcast provider/ingest/title/viewport | POST `/Broadcast/{Provider, IngestServer, Title, Viewers/Max, Viewers, SessionParam(/:type)}` | §A.3 |
| Portal login token | POST `/Account/v.1.0/UserToken` `{userToken, userInfo}` | `vendor:171780` |
| GDPR consent | POST `/Account/v.1.0/PrivacySettings` `{userId|clientId, consentSettings}` | `vendor:172990` |
| CoPlay mapping | PUT `/GameShare/v.1.0/ConfigureControllerMapping` `{mode}` | @576619 |

OSD state itself is push-synced (`DisplayOscPreferences`/`DisplayOscState`
sockets, §B) and indicator rects via `Indicator/:id/Settings` (@595685).

Count C: 5 quality ids ×8 resolutions ×2 fps with per-portal caps; 16 persistence
endpoint groups.

---

## D) L10N KEY MAP

`l10n/en-US.json` (45,073 B, UTF-8 **with BOM**, flat map, 673 keys, all
prefixed `l10n.` except typo `l10m.created`). Loader: `$translatePartialLoader`
`urlTemplate:"{part}/{lang}.json"` + `addPart("l10n")` (@428600, @411990) →
`l10n/en-US.json`. Usage: `translate("l10n.key")` / `"l10n.x"` data attrs
(e.g. @19650, @489372). 28 language files ship in `l10n/`.

Keys identifying user-visible screens (recommended names for our UI):

- **Top nav / modes**: `l10n.gfe, l10n.instantReplay, l10n.manualRecord ("Record"),
  l10n.stream, l10n.gallery, l10n.highlights, l10n.broadcast, l10n.goLive,
  l10n.openShare, l10n.preferences, l10n.preferencesHome, l10n.settings,
  l10n.customize, l10n.connect`
- **Preferences tabs**: `l10n.preferencesConnect, l10n.preferencesHUD,
  l10n.preferencesKeyboard, l10n.preferencesRecordings, l10n.preferencesStream,
  l10n.preferencesBroadcast, l10n.preferencesNotifications, l10n.preferencesPrivacy`
- **Record/IR customize**: `l10n.quality, l10n.resolution, l10n.framerate,
  l10n.bitrate, l10n.replayLength, l10n.instantReplayLength, l10n.low,
  l10n.medium, l10n.high, l10n.ultra, l10n.custom, l10n.inGame,
  l10n.videoQuality, l10n.videoFrameRate, l10n.videoBitRate,
  l10n.videoMaxLength, l10n.videoMinutes, l10n.customizeNote, l10n.gotoSettings`
- **Audio**: `l10n.audio, l10n.systemSounds, l10n.systemSoundsVolume,
  l10n.microphone, l10n.microphoneSource, l10n.recordingDevice, l10n.source,
  l10n.volume, l10n.boost, l10n.sampleRate, l10n.audioTrack, l10n.singleTrack,
  l10n.separateTrack, l10n.channels, l10n.stereo, l10n.surround, l10n.noMic,
  l10n.warningAudio{Ir,Mr,Br,Hl}{T1,T2}`
- **HUD/OSD**: `l10n.overlays, l10n.hudLayout, l10n.statusIndicator,
  l10n.statusIndicators, l10n.fpsCounter, l10n.frameRateCounter, l10n.viewers,
  l10n.viewerCount, l10n.myRigDetails, l10n.myRig, l10n.comments,
  l10n.facebookComments, l10n.showHideComments, l10n.position, l10n.size,
  l10n.positionSetting, l10n.upperLeft/upperRight/lowerLeft/lowerRight,
  l10n.small, l10n.large, l10n.customOverlayFile, l10n.broadcastOverlay(s),
  l10n.slotEmpty, l10n.switchOverlay`
- **Keyboard**: `l10n.keyboardShortcuts, l10n.hotkey, l10n.activatePushToTalk,
  l10n.toggleMic, l10n.toggleFPS, l10n.toggleIR, l10n.saveScreenshot,
  l10n.saveLast5Mins, l10n.saveLastNMins, l10n.toggleRecording,
  l10n.toggleBroadcasting, l10n.pauseResume, l10n.toggleCamera,
  l10n.toggleOverlay, l10n.commentHotkey, l10n.assignHotkey,
  l10n.resetShortcutsQuestion, l10n.keyboardShortcutDuplicate,
  l10n.keyboardShortcutUsedByGFN`
- **Recordings/folders**: `l10n.recordings, l10n.recordingsMta, l10n.screenshots
  (+EXR/3D/360 variants), l10n.videosLabel, l10n.tempFilesLabel,
  l10n.folderBrowserTitle{Videos,TempFiles,Highlights,Logging},
  l10n.galleryLocation, l10n.tempFilesLocation, l10n.storage, l10n.readOnlyFolder`
- **Broadcast/destinations**: `l10n.broadcastTarget, l10n.destination,
  l10n.destinationAndFormat, l10n.broadcastIngest, l10n.twitchIngest,
  l10n.ingestServerForTwitch, l10n.alwaysAskMe, l10n.postAs, l10n.title,
  l10n.audience, l10n.public, l10n.unlisted, l10n.private, l10n.friends,
  l10n.onlyMe, l10n.timeline, l10n.managedPage, l10n.channel, l10n.groupMember,
  l10n.doNotBroadcast, l10n.format, l10n.videoRecording, l10n.animatedGIF,
  l10n.shareTo`
- **Portals**: `l10n.twitch, l10n.youTube, l10n.imgur, l10n.googlePhotos,
  l10n.facebook, l10n.weibo, l10n.shotWithGeForce, l10n.qq, l10n.hint{Twitch,
  Youtube,Imgur,GooglePhotos,Facebook,Weibo}, l10n.connectAccounts,
  l10n.connectLogin, l10n.connectLogout, l10n.connectedAs,
  l10n.automaticallyConnectedAs, l10n.tooltipUpload*`
- **Highlights**: `l10n.highlightsInfo, l10n.highlightsMaxSpace,
  l10n.highlightsMaxSpaceUsed, l10n.turnOnHighlights, l10n.enableHighlights,
  l10n.enableHighlightsFooter, l10n.highlightsCapture, l10n.moveHighlights,
  l10n.noHighlightsPossible, l10n.noHighlightsAvailable, l10n.noRecentAvailable`
- **Perfmon/OC tool**: ~80 keys `l10n.perfmonoc.*` (performanceOverlay,
  performanceTuning, GPUClockOffset, MemoryClockOffset, voltageMaximum,
  powerMaximum, temperatureTarget, fanSpeedTarget, latency, reflexAnalyzer,
  e2eSystemLatency, CPUUtilization, GPUUtilization, resetAverages,
  toggleLogging, fileLogging, …)
- **Whisper/QuietMode2**: `l10n.whisperModeSettings, l10n.quiet, l10n.quieter,
  l10n.balancedMode, l10n.enabledWhisperModeSettings`
- **Ansel/Freestyle/Mods**: `l10n.inGamePhotography, l10n.capture,
  l10n.sizeAndPosition, l10n.captureResolution, l10n.filter(s), l10n.styles,
  l10n.mods, l10n.openMods, l10n.freestyle.*, l10n.ansel.*,
  l10n.styleTransfer*, l10n.imageSharpening, l10n.deepDvc.vibrance`
- **Desktop capture / HDR / experimental / privacy switches**:
  `l10n.desktopCapture{Screenshot,Record,Broadcast}{Question,Yes},
  l10n.desktopCaptureHint, l10n.hdrRaw, l10n.hdrRawColor, l10n.experimental,
  l10n.settingsPrivacySwitch/Describe/Disable, l10n.settingsStreamSwitch,
  l10n.settings{BroadcastLive,Highlights,VideoCapture,Recordings}Disable`

Notifications (OSD toasts) map 1:1 from NOTIFIER_SELECTIONS constants →
`l10n.notification*` keys (see @4222 list vs. l10n keys).

Count D: 673 keys total; ~180 identify settings screens/controls.

---

## Method note

All findings derive from regex sweeps + targeted string slices of app.js
(never full-file dumps): URL-literal inventory, `createEndpoint` +
`newLocalEndpointFactory` receiver mapping, `socketService.register` sweep,
`.constant(` catalog, SettingsCustomizeController slice, en-US.json key dump.
No files under Program Files or the A:\ backup were modified; only files in
`C:\My Project\NVIDIA-Shadowplay-gfe\docs\` were created.
