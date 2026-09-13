# 03 — Protocol Map (Phase 3 + Phase 4)

Three transports, one origin. Cross-checked against the host-side
`Overlay.Engine/PROTOCOL-MATRIX.md` (independent worker, golden-transcript
method) — **zero contradictions found**; disagreements are called out.

---

## 1. window.cefQuery (CEF message router / WebView2 bridge)

### Wrapper semantics (HIGH — vendor.js `crimson.cefService`, literal)

```
window.cefQuery({
  request: <JSON STRING>,          // {command: "QUERY_…", …args}
  persistent: <bool>,              // persistent = push channel via onSuccess
  onSuccess: (responseString) => …,
  onFailure: (errorCode, errorMessage) => …
}) → handle {id, cancel()}
```

| Response string | cefService behavior |
|---|---|
| `"true"` | resolve(true) |
| `"false"` | resolve(false) |
| persistent query | every push → notify (stream) |
| anything else | resolve(string) |
| failure | reject `{errorCode, errorMessage}`; **204 = cancelled** |
| no bridge | reject `"Cannot find cefQuery. cmd=…"` → service degrades |

### Command table

Caller column: which service layer issues it. Usage: what breaks if the
command is missing. **The list below is exhaustive over both bundles**
(extraction of every `QUERY_[A-Z_0-9]+` literal; per-command context read).

| Command | Caller | Arguments | Response | Persistent | UI usage / notes |
|---|---|---|---|---|---|
| `QUERY_WIN_NODE_INFO` | cefService.localNodeInfo | — | JSON `{port, secret}` | no | **boot-critical** (base resolve); without it: port 3000 + empty cookie |
| `QUERY_FULLSCREEN_STATE` | oscDisplayService | — | JSON `{fullscreen, hdractive, borderlessMode}` | no | desktop/fullscreen layout split, HDR flag |
| `QUERY_OSC_DISPLAY_IS_DESKTOP_MODE` | cefService.isInDesktopMode | — | `"true"/"false"` | no | layout choice |
| `QUERY_OSC_SET_DISPLAY_RECTS` | oscDisplayService.setDisplayRects | `displayRects: [{x,y,width,height}] \| [[x,y,w,h]]` | `"true"` | no | **click-through contract**; empty array while closed |
| `QUERY_OSC_SET_PAINTING` | cefService.allowOSCPainting | `enablePainting, startImmediately` | `"true"` | no | perf-overlay painting path (menu path never calls it) |
| `QUERY_OSC_SET_EXPERIMENTAL` | octoolService | `isExperimental` | — | no | OC tool feature flag |
| `QUERY_OSC_REGISTER_CLOSE_EVENT` | oscDisplayService (inline cefQuery) | — | push on close request | **yes** | onSuccess → closeOSC() |
| `QUERY_OSC_DROP_URL` | cefService.oscCreateDropUrl | `url, xpos, ypos` | — | no | drag-drop OSD |
| `QUERY_WIN_OPEN_OSC` | cefService.openOSC | `enableInput` | — | no | host-driven open |
| `QUERY_WIN_CLOSE_OSC` | cefService.closeOSC | — | — | no | host-driven close |
| `QUERY_WIN_COPY_TO_CLIPBOARD` | cefService.setClipboardData | `clipBoardData` | — | no | copy invite/links |
| `QUERY_READ_SHARED_STORAGE` | cefService.readSharedStorage | `path` | stored string | no | persisted UI state |
| `QUERY_WRITE_SHARED_STORAGE` | cefService.writeSharedStorage | `path, data` | — | no | persisted UI state |
| `QUERY_LOAD_STRING_TABLE` | cefService.loadStringTable | `stringTable` | — | no | localization handshake |
| `QUERY_WIN_DIR_INFO` | cefService.localDirectoryExplorer | `includeFiles, filter, Win7dlg` | dir listing | no | folder browser (recordings path) |
| `QUERY_TIME_INFO` | cefService.getTimeInfo | `type` | — | no | time queries |
| `QUERY_WIN_ALLOW_CLOSE` | cefService.enableCloseButton | `enable` | — | no | window chrome |
| `QUERY_WIN_IS_BORDERLESS` / `QUERY_WIN_IS_MAXIMIZED` / `QUERY_WIN_MAXIMIZE` / `QUERY_WIN_RESTORE` / `QUERY_WIN_MINIMIZE` / `QUERY_WIN_CLOSE` / `QUERY_WIN_FOCUS` | cefService | — / `name` | bool / — | no | window controls (borderless OSC window) |
| `QUERY_WIN_MOUSE_START` | cefService.resizeStart/moveStart | `region` | — | no | drag/resize |
| `QUERY_HIDE_APPLICATION` / `QUERY_REQUEST_USER_ATTENTION` | cefService | — | — | no | taskbar UX |
| `QUERY_GET_MAX_WINDOW_SIZE` / `QUERY_WIN_TASKBAR_PROGRESS` / `QUERY_WIN_ALLOW_SET_FOREGROUND` | cefService | `state, percent` / `pid` | — | no | window UX |
| `QUERY_IS_UI_REFRESHED` | cefService.isUIRefreshed | — | — | no | readiness probe |
| `QUERY_NODE_RESTART` | cefService.restartNode | `reload` | — | no | node restart |
| `QUERY_REGISTER_WINDOW_EVENTS_CALLBACK` / `QUERY_REGISTER_APPLICATION_LIFETIME_EVENTS_CALLBACK` | cefService | — | pushes | **yes** | host event streams |
| `QUERY_DELETE_COOKIES` | cefService.deleteCookies | `url, cookiename` | — | no | logout |
| `QUERY_WIN_KB_MESSAGE` | cefService.oscSendWinKBMessage | `keycode, keymodifier` | — | no | synthetic keys |
| `QUERY_IPC_EXTENSION_MESSAGE` | cefService (IPC) | `module, request` | pushes | yes | extension IPC |
| `QUERY_IPC_{PUSH,POP,CLEAR,GET_NUMBER_OF}_MESSAGES` | IPC queue | — | — | no | extension IPC |
| `QUERY_READ_CONFIG` / `QUERY_WRITE_CONFIG` | config bridge | — | — | no | node config |
| `QUERY_DEVICE_ID` / `QUERY_SYSTEM_INFO` / `QUERY_DNS` / `QUERY_BROWSE_DIRECTORY` | info probes | — | — | no | hardware/system services |
| `QUERY_NOTIFICATION_DATA` | notifier | — | — | no | notifications |
| `QUERY_LAUNCH_COMPANION_APP` / `QUERY_IS_APPLICATION_INSTALLED` / `QUERY_IS_APPLICATION_RUNNING` / `QUERY_STREAMER_*` (INSTALL/LAUNCH/IS_INSTALLED/CLOSE) | companion/streamer | — | — | no | external app control |
| `QUERY_GFN_*` (PREPARE/START/STOP/RESUME/CANCEL/…) | GeForce NOW | — | — | no | GFN flow (dead for ShadowPlay) |
| `QUERY_OSR_*` / `QUERY_OSR_REGISTER_KEYPRESS*` / `QUERY_OSR_{SHOW,HIDE}_SDL_WINDOW` | OSR/SDL | — | — | no | NvCamera/OSR window |
| `QUERY_OPEN_CUSTOM_LAYER` / `QUERY_CLOSE_CUSTOM_LAYER` | custom layers | — | — | no | overlay layers |
| `QUERY_CONTROL_STATS` / `QUERY_READ_UPDATE_TICKET` / `QUERY_UPDATE…`/`QUERY_RESTART_APP` | misc | — | — | no | — |
| `QUERY_HTTPSERVER_START` | oauth proxy | `ports:[2259,6460,7119,8870,9096], redirectUrl, redirectParams` | progress pushes (`serverCreated`…) | yes | OAuth loopback capture — M1 host **fails fast** `oauth_not_implemented` |

Legend: rows marked `—` for response = fire-and-forget (promise resolves on
any ack string). Every unknown command on the M1 host is failed with
`errorCode -1 not_implemented` → calling service degrades (measured,
CefQueryBridge.Dispatch default).

---

## 2. socket.io v2 over engine.io v3 polling

### Wire facts (HIGH — golden transcript `Tester/test/Overlay/Overlay.OscEngine.Tests/golden/golden-transcript.json`, captured with the REAL client libs engine.io-client@3.5.4 + socket.io-client@2.5.0)

| Fact | Golden evidence |
|---|---|
| Handshake `GET /socket.io/?X_LOCAL_SECURITY_COOKIE=<secret>&EIO=3&transport=polling&t=<rand>&b64=1` | step[0] |
| Open packet: `<len>:0{"sid":…,"upgrades":[],"pingInterval":2000,"pingTimeout":60000}` — **length-prefixed batches; `len` counts packet chars** | step[1] |
| **The SERVER sends socket.io CONNECT `40`** on the client's first poll; a v2 client never sends CONNECT for the default namespace | steps[2-3] |
| Events to page: `<len>:42["<channel>",<payload>]`; batches allowed | step[4] |
| Client emits: POST `<len>:42["<channel>",<payload>]` | POST step |
| **CLIENT pings** `1:2` every `pingInterval`; server answers `3` in a later poll | POST bodies |
| Idle poll → `1:6` (NOOP) | steps |
| Reconnect defaults (vendor.js): reconnection on, attempts ∞, delay 1000→5000 ms, randomizationFactor 0.5 | vendor literal |
| Unauthorized (bad cookie) → **401** handshake | host code + matrix |

Sample golden vector (verbatim from transcript):
```
GET  /socket.io/?X_LOCAL_SECURITY_COOKIE=GOLDENSECRET42&EIO=3&transport=polling&t=Q2MlRPm&b64=1
→    74:0{"sid":"GOLDENSID","upgrades":[],"pingInterval":2000,"pingTimeout":60000}
GET  …&sid=GOLDENSID
→    2:40
GET  …&sid=GOLDENSID
→    65:42["/ShadowPlay/v.1.0/WindowState",{"windowMsg":"overlayToggle"}]
POST …&sid=GOLDENSID   body "1:2"      (client ping)
→    "ok"; PONG "3" delivered in a later poll batch
```

### Channel registry (exhaustive — 24 literals in app.js)

| Channel | Direction | Payload (from call sites) | Registered by | M1 host |
|---|---|---|---|---|
| `/ShadowPlay/v.1.0/WindowState` | s→c | `{windowMsg: "dismiss"\|"fullscreenTransition"\|"overlayToggle"\|"showHotkeyMessage"}` | oscDisplayService | not pushed |
| `/ShadowPlay/v.1.0/Notification` | s→c | `{title, message}` (+fields at real backend) | shadowPlayService (processNotification) | **pushed (M1: `{title,message}`)** |
| `/ShadowPlay/v.1.0/DisplayOscNotification` | s→c | display-notification payload | shadowPlayService (processShowNotification) | not pushed |
| `/ShadowPlay/v.1.0/DisplayOscState` | s→c | `{state, params}` (open/close of OSD surfaces) | oscDisplayService | not pushed |
| `/ShadowPlay/v.1.0/DisplayOscPreferences` | s→c | preferences push | oscDisplayService | not pushed |
| `/ShadowPlay/v.1.0/Hotkey` | s→c | hotkey info | hotkeyService | logged only |
| `/ShadowPlay/v.1.0/Record/Enable` | s→c | `{status: bool}` | shadowPlayService (recordNotifier) | not pushed (M1 REST owns truth) |
| `/ShadowPlay/v.1.0/InstantReplay/Enable` | s→c | `{status}` | shadowPlayService | not pushed |
| `/ShadowPlay/v.1.0/InstantReplay/Started` | s→c | start info | shadowPlayService | not pushed |
| `/ShadowPlay/v.1.0/InstantReplay/Save` | s→c | save info | shadowPlayService | not pushed |
| `/ShadowPlay/v.1.0/InstantReplay/Upload` | s→c | upload info | shadowPlayService | not pushed |
| `/ShadowPlay/v.1.0/Broadcast/{Enable,Pause,SessionEvent}` | s→c | broadcast status | broadcastService | n/a |
| `/Settings/v.1.0/Language` | s→c | `{language}` | settingsService | not pushed (REST serves it) |
| `/PiplConfig/v.1.0/update` | s→c | `{}` config push | piplConfigService | pushed `{}` (offline-first) |
| `/Account/v.1.0/UserToken` / `/Account/v.1.0/PrivacySettings` | s→c | account events | account SDK | n/a |
| `/SDK/v.1.0/Notification` | s→c | SDK notifications | sdkService | not pushed |
| `/NvCamera/v.1.0/Notifications` | s→c | camera notifications | nvCameraService | n/a |
| `/GameShare/v.1.0/{CreateSession,SessionUpdate}` | s→c | coplay sessions | coplayService | n/a |
| `/abHubAPI/v.0.1/{Message,Status}` | s→c | AB test hub | abHubSdk | n/a |

---

## 3. REST (same HTTP server, header auth)

### URL construction (HIGH — `localSdk` + `NvEndpointFactory`)

```
full = node.server + node.port + "/" + service + "/" + version + endpoint.url
headers: every request carries X_LOCAL_SECURITY_COOKIE (merged commonHeaders)
```

### Legacy REST surface (HIGH — endpoint literals in app.js)

**ShadowPlay** (`/ShadowPlay/v.1.0/…`, some at v.1.1):
`GET ""`, `POST /Launch {launch}`, `GET|POST /InstantReplay/Enable {status}`,
`GET /InstantReplay/Running`, `POST /InstantReplay/Save`,
`POST /InstantReplay/Upload`, `GET /InstantReplay/BufferLength`,
`GET|POST /InstantReplay/Settings {replayLengthSeconds, quality[, resolution,
framerate, bitrateBps]}`, `GET /Record/Enable`, `GET /Record/Running`,
`POST /Record/Enable {status}`, `GET|POST /Record/Settings {quality[,
resolution, framerate, bitrateBps]}`, `GET /Record/Concurrency/{Broadcast,
Gamestream}`, `GET /GetHDRState`, `GET /Resolutions`, `GET /FrameRates`,
`GET /Resolutions/:quality`, `GET /Framerates/:quality`,
`GET /BitRates/:quality/:resolution`, `GET /Broadcast/Support`, …

**Settings**: `GET|POST /Settings/v.1.0/Language {language}`, `GET /beta`.
**HardwareInformation, SDK, NvCamera, Feedback, QuietMode2 (/support,
/state), DeepDVC, Nis2, GameShare, abHubAPI**: per-feature SDKs (see 02).

### M1 host REST (the backend that EXISTS today — OscControllerServer.vb)

| Route | Method | Response | Notes |
|---|---|---|---|
| `/state` | GET | `{record, instantReplay, broadcast, elapsedSec}` | engine truth (OscHostForm fields) |
| `/Record/Enable` | POST | `{}` | body containing `"true"` ⇒ engine `RECORD_START:<path>` else `RECORD_STOP` |
| `/Record/Enable` | GET | `{}` | (legacy read path — no data in M1) |
| `/Record/Settings` | GET/POST | provider JSON / `{}` | POST accepted, no-op in M1 |
| `/RecordPaths` | GET | `{savePath}` | from `Config\config.json` `Paths.SavePath` |
| `/Language` | GET | `{"language":"en-US"}` | fixed in M1 |
| `/uiReady` | POST | `{}` | page → host readiness event |
| `/support` | GET | `{}` | |
| anything else | GET/POST | `{}` (200) | **unknown endpoints must never reject** — boot resolves depend on it |
| auth | — | 401 | missing/wrong `X_LOCAL_SECURITY_COOKIE` (header or query) |

**Divergence note (FACT):** legacy REST paths carry the service prefix
(`/ShadowPlay/v.1.0/Record/Enable`); the M1 host routes the **unprefixed**
paths. On the M1 host every legacy-prefixed call falls into the `{}` catch-all.
The new UI therefore speaks the M1 unprefixed surface (the host contract that
exists), with the legacy scheme documented here for the future full backend.
