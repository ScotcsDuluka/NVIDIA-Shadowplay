# OSC GALLERY / CEF-IPC EXTRACTION (ZCode mission 3, 2026-09-24)

Sources (READ-ONLY): `WebView\osc\app.js` (1,596,161 chars) + `vendor.js`
(1,431,625) on the live install — anchors are charOffsets; `NvGalleryAPI.js`
from repo evidence copy `docs/reference/osc/nodejs/` (581 lines, line
anchors); original native host binaries under
`A:\NV OSC Backup\GeForce_Experience_v3.28.0.412\` (strings only).

Headline corrections vs. the mission card:

1. **The osc page NEVER sends `QUERY_WIN_DIR_INFO`** (zero occurrences in
   app.js). Its gallery/dir flows use **HTTP** (`/Gallery/...` via the
   `galleryEndpoints` SDK inside vendor.js) and **`QUERY_BROWSE_DIRECTORY`**
   (native folder dialog) for "Open Location".
2. The live `C:\Program Files\...\NVIDIA GeForce Experience\` tree is OUR
   portable install (Coordinator/Hook/Notifier/WebView/WinForm) — the original
   native host binaries live in the A:\ backup.
3. The production cefQuery answerer is the **native CEF host**
   `GFExperience\NVIDIA Share.exe` (3,347,496 B) — it contains the ASCII
   command strings (`QUERY_WIN_DIR_INFO` found at 1 hit via byte grep; other
   commands same family). Not the node Web Helper, not WebViewHook.

---

## A) The gallery/dir browse story (evidence-based)

### A.1 QUERY_WIN_DIR_INFO (vendor.js:194783)

```js
a.localDirectoryExplorer=function(e,t,n){return i({command:"QUERY_WIN_DIR_INFO",
  includeFiles:e, filter:t, Win7dlg:n})}
```

- Request: `{command, includeFiles:bool, filter:string, Win7dlg:bool}` — no
  path field; the CEF host tracks/browses its current directory server-side.
- **Answered by**: native CEF host `GFExperience\NVIDIA Share.exe` (A:\ backup;
  ASCII string present, byte-grep hit). The osc WebView2 page does not call it
  — consumers are Experience-side (WinForm) galleries.
- Our host: implement as dir-listing over a host-side cursor (includeFiles /
  filter / Win7 dialog flag) only if we rebuild Experience-side panels; the
  osc page needs none of it.

### A.2 What the osc page actually uses for browsing

- `QUERY_BROWSE_DIRECTORY` `{name}` (vendor.js:201555,
  `a.browseDirectory=function(e)`) — native folder picker.
  Sole osc caller: **"Open Location"** in the gallery/upload flow
  (app.js:513864): takes `displayFileString`, cuts at the last `\`,
  `a.closeOSC()`, then `f.browseDirectory(folder)` — opens the native dialog
  at that folder and (after user picks) the shell opens it.
- **Directory/file listing = HTTP**, not IPC: `galleryEndpoints` SDK in
  vendor.js (see §B) — `getGalleryFolderListing` → POST `/Gallery/v.1.2/
  GetFolderListing` (app.js:780221), `enumerateDrives` → GET `/EnumerateDrives`
  (app.js:779664), `isDirectoryWritable` → POST `/isDirectoryWritable`
  (app.js:778338). These serve both the gallery directory view AND the
  preferences folder pickers (RecordPaths temp/videos folder selection).

### A.3 Gallery HTTP vs IPC split (page-verified)

| Flow | Mechanism | Evidence |
|---|---|---|
| Drive enumeration | GET `/Gallery/v.1.0/EnumerateDrives` | app.js:779664 |
| Folder/file listing | POST `/Gallery/v.1.2/GetFolderListing` | app.js:780221 |
| Writability check (folder pickers) | POST `/Gallery/v.1.0/isDirectoryWritable` | app.js:778338 |
| Open Location | IPC `QUERY_BROWSE_DIRECTORY` | app.js:513864 |
| Thumbnail images | POST `/Gallery/v.1.0/GetThumbnail` (server-generated; NvGalleryAPINode native) | vendor SDK @271653 region |
| Remove/copy/trim/transcode | HTTP `/Remove`, `/CopyFile`, `/ShadowPlay/v.1.0/Video/Trim`, `/TranscodeMediaFile` | app.js:536411, 538380 |

---

## B) Gallery HTTP surface the page really uses

`galleryEndpoints` provider (vendor.js:249459): two URL bases —
`r = server+port+"/Gallery/"+version` (version **v.1.0** injected by app.js:427775
`d.setLocalConfig({version:"v.1.0"})` through ugcLib) and `i = ...+"/Gallery/v.1.2"`
(hardcoded default, vendor.js:269798 `a="v.1.2"`). Two endpoint factories:
**M** on `r` (v.1.0) and **k** on `i` (v.1.2). After the node handshake,
`updateNodeInfo` re-bases both on the real port (SDK tail `A=function(s){...
t=n+s.port+"/Gallery/"...}`, vendor.js:272420).

Endpoints (declared vendor.js:270340-272400; server routes in
NvGalleryAPI.js — page-called marked ✓):

| Endpoint (method, url, request fields) | Base | Page call | Server route |
|---|---|---|---|
| POST `/GetFolderListing` `{directory, shouldWatch, shouldGetOnlyNv, excludeDirectoryType, shouldShowEXR}` | **k (v.1.2)** | ✓ app.js:780221 | NvGalleryAPI.js — `post /Gallery/v.1.2/GetFolderListing` |
| POST `/GetStats` `{directory, width, height, shouldGetOnlyNv, quickCheck, shouldShowEXR}` | M | — | `post /Gallery/v.1.0/GetStats` |
| POST `/GetFolderCRC` `{directory}` | M | — | ✓ server |
| GET `/Recent/8` | M | — (Experience uses) | ✓ server |
| POST `/Recent/Clear` | M | — | ✓ |
| GET/POST `/Upload/History`, POST `/Upload/History/Remove` `{url}`, POST `/Upload/History/Clear` | M | via upload flow | ✓ |
| POST `/Remove` `{file, forceDelete}` | M | ✓ (removeGalleryItem — app.js:433981, 533694, 538380, 538798) | ✓ |
| POST `/GetFileMetaData` `{file, width, height}` / `{file}` | M | via player/upload | ✓ |
| POST `/GetImageFileDimensions` `{file}` | M | via upload | ✓ |
| POST `/GetThumbnail` `{file, size}` | M | gallery grid | ✓ |
| POST `/TranscodeMediaFile` `{file, maxFileSizeMB, quality, newHeight, newFps, targetPath, newDuration, userData, memeImage}` | M | ✓ upload prep (app.js:538380; response fields `{newFile, newFileWidth, newFileHeight, newFileSizeB}` app.js:538460) | ✓ |
| POST `/TranscodeVideoToGIF` `{...}` | M | GIF export | ✓ |
| GET `/EnumerateDrives` | M | ✓ app.js:779664 | ✓ |
| POST `/CopyFile` `{source, destination}` | M | ✓ app.js:536411/536469 (upload copy) | ✓ |
| POST `/isDirectoryWritable` `{directory}` (lowercase i!) | M | ✓ app.js:778338 | server: `/IsDirectoryWritable` (capital I) — **case mismatch; Express matches exact spelling only → the SERVER route as listed never fires; verify real route casing at runtime** |
| POST `/WriteEncryptedBmp` `{bitmapImage}` | M | — | ✓ |

Backend-only (page never calls): `Recent/8`, `Recent/Clear`, `GetFolderCRC`,
`GetStats`, `Upload/History*` are wired for the Experience gallery; keep them
cheap (static responses) for parity.

Thumbnail pipeline: server-side via `GetThumbnail` (native
`NVGalleryAPINode.node` decodes/transcodes; no client-side thumb code found).

---

## C) Playback flow

- Gallery/upload **preview player** = HTML5 `<video>`; source assignment:
  `B.videoSrc = B.fileToUpload.fullFilename.replace(/\\/g,"/")`
  (app.js:551777) — the **local path with normalized slashes is the src**
  (CEF serves/reads it as a file URL; no custom scheme, no external player).
- **Video editor player**: `g.videoSrc = $sce.trustAsResourceUrl(g.nvSrc)`
  (app.js:557420, 558441, 564782) — `nvSrc` arrives as a scope attribute from
  the gallery listing entry; `$sce.trustAsResourceUrl` is the only
  sanitization layer, plus backslash normalization. No allow-list check on the
  path was found — the security boundary is the CEF host's file access.
- Trim before re-save: `trimVideo(fullFilename, ...)` → POST
  `/ShadowPlay/v.1.0/Video/Trim` `{input, output, headTrimMs, lengthMs}`
  (app.js:536143 region); output naming `<base>-<startSec>-<durSec>.mp4`
  (app.js:536003 region).
- **`QUERY_HTTPSERVER_START` is the OAuth loop, not media serving**:
  `{ports:[2259,6460,7119,8870,9096], redirectUrl:"https://google.com",
  redirectParams:["error"]}` as a **persistent query** whose progress events
  deliver `{callbackReason:"serverCreated", portNumber}` then auth redirects
  (app.js:277236). `redirectUrl.replace("{{portNumber}}", ...)` matches
  `config.js` `redirectUriOverride: "http://localhost:{{portNumber}}"`.

---

## D) FULL cefQuery command enumeration (82 total)

Wrapper: `vendor.js` crimson `cefService` (`i(request, persistent?)` →
`window.cefQuery`); app.js sends 5 queries directly via the generic
`s.query(request)` / raw `a.cefQuery({request, persistent, onSuccess})`.
"✓" = implemented in our CefQueryBridge.vb (14 of 82).

### D.1 Window / shell host (vendor.js:194699-198200)

| Command | Request fields | Purpose / response | Impl |
|---|---|---|---|
| QUERY_WIN_NODE_INFO | — | `{port, secret}` boot handshake | ✓ |
| QUERY_WIN_DIR_INFO | includeFiles, filter, Win7dlg | dir listing (native host) | ✗ (osc unused) |
| QUERY_TIME_INFO | type | host time info | ✗ |
| QUERY_WIN_ALLOW_CLOSE | enable | allow/block window close | ✗ |
| QUERY_WIN_IS_BORDERLESS | — | borderless? | ✗ |
| QUERY_IS_UI_REFRESHED | — | UI ready flag | ✗ |
| QUERY_GET_MAX_WINDOW_SIZE | — | max window size | ✗ |
| QUERY_WIN_IS_MAXIMIZED | — | maximized? | ✗ |
| QUERY_WIN_CLOSE / QUERY_WIN_MINIMIZE / QUERY_WIN_MAXIMIZE / QUERY_WIN_RESTORE | — | window ops | ✗ |
| QUERY_HIDE_APPLICATION | — | hide window | ✗ |
| QUERY_REQUEST_USER_ATTENTION | — | flash taskbar | ✗ |
| QUERY_WIN_FOCUS | name | focus window | ✗ |
| QUERY_WIN_MOUSE_START | region ('move'\|region) | begin drag/resize | ✗ |
| QUERY_WIN_RECT | x,y,w,h | set window rect | ✗ |
| QUERY_WIN_LANG_CODE | app_name | host language code | ✗ |
| QUERY_WIN_TASKBAR_PROGRESS | state, percent | taskbar progress | ✗ |
| QUERY_WIN_ALLOW_SET_FOREGROUND | pid | foreground permission | ✗ |
| QUERY_WIN_ANIMATE_ACTIONS | action | UI animation trigger | ✗ |
| QUERY_LAUNCH_COMPANION_APP | appName | launch companion | ✗ |
| QUERY_NOTIFICATION_DATA | — | last notification payload | ✗ |
| QUERY_IS_APPLICATION_INSTALLED / QUERY_IS_APPLICATION_RUNNING | (name) | app presence | ✗ |
| QUERY_IS_IN_FULLSCREEN_EXCLUSIVE | — | fullscreen-exclusive? | ✗ |
| QUERY_RESTART_APP | launchArguments | restart host app | ✗ |
| QUERY_NODE_RESTART | reload | restart node backend | ✗ |
| QUERY_REGISTER_WINDOW_EVENTS_CALLBACK / QUERY_REGISTER_APPLICATION_LIFETIME_EVENTS_CALLBACK | — (persistent) | event push registration | ✗ |
| QUERY_DEVICE_ID | — | device id | ✗ |
| QUERY_DELETE_COOKIES | url, cookiename | clear cookies | ✗ |
| QUERY_WIN_COPY_TO_CLIPBOARD | clipBoardData | clipboard write | ✓ |
| QUERY_LOAD_STRING_TABLE | stringTable | localized strings | ✓ |
| QUERY_BROWSE_DIRECTORY | name | **native folder dialog** (gallery Open Location) | ✗ |
| QUERY_WIN_KB_MESSAGE | keycode, keymodifier | host keyboard message | ✗ |

### D.2 OSC window control (vendor.js:196173-196500)

| Command | Request fields | Purpose / response | Impl |
|---|---|---|---|
| QUERY_WIN_OPEN_OSC | enableInput | open overlay window | ✓ |
| QUERY_WIN_CLOSE_OSC | — | close overlay | ✓ |
| QUERY_OSC_DISPLAY_IS_DESKTOP_MODE | — | desktop vs fullscreen mode | ✓ |
| QUERY_OSC_SET_PAINTING | enablePainting, startImmediately | overlay painting control | ✓ |
| QUERY_OSC_DROP_URL | url, xpos, ypos | drag-drop transfer URL (oscCreateDropUrl) | ✗ |

### D.3 App.js direct queries

| Command | Request fields | Purpose / response | Impl |
|---|---|---|---|
| QUERY_FULLSCREEN_STATE (app.js:66196) | — | fullscreen state poll | ✓ |
| QUERY_OSC_SET_DISPLAY_RECTS (app.js:68333) | displayRects:[...] | OSD rect layout | ✓ |
| QUERY_OSC_REGISTER_CLOSE_EVENT (app.js:67781) | — (persistent:true) | push: close message → closeOSC() | ✓ |
| QUERY_OSC_SET_EXPERIMENTAL (app.js:83163) | isExperimental | perfmon experimental toggle | ✓ |
| QUERY_HTTPSERVER_START (app.js:277236) | ports:[2259,6460,7119,8870,9096], redirectUrl, redirectParams (persistent) | OAuth loopback server; progress push `{callbackReason:"serverCreated", portNumber}` | ✓ |

### D.4 OSR (out-of-process rendering / Ansel SDL) (vendor.js:201143-202500)

QUERY_OSR_SHOW_SDL_WINDOW, QUERY_OSR_HIDE_SDL_WINDOW, QUERY_WIN_OPEN_OSR
{enableInput}, QUERY_WIN_CLOSE_OSR, QUERY_OSR_INPUT_MONITOR {enable},
QUERY_OSR_REGISTER_INPUT_MONITOR (persistent), QUERY_OSR_REGISTER_KEYPRESS_CALLBACK (persistent), QUERY_OSR_REGISTER_KEYPRESS {name, keyCombination} — Ansel/OSR input plumbing; ✗ all (only needed for Ansel UI).

### D.5 GFN / GameStream (vendor.js:198333-203600) — ✗ all (CoPlay remote play)

QUERY_GFN_PREPARE {address, serverType, port, profile, deviceId,
advancedLatencyOptimization, directInput[, streamingProfileWidth/Height/Fps/
Drc/VSync]}, QUERY_GFN_SET_AUTH_INFO {token, tokenType}, QUERY_GFN_START
{appId, frameStatsEnabled, summaryStatsEnabled, maxControllersForSingleSession,
advancedLatencyOptimization, session}, QUERY_GFN_RESUME, QUERY_GFN_STOP
{session}, QUERY_GFN_CANCEL, QUERY_GFN_REGISTER_CALLBACK (persistent),
QUERY_CONTROL_STATS {option, enable}, QUERY_GFN_GET_ACTIVE_SESSIONS,
QUERY_GFN_SET_AUTH_TOKEN {token}, QUERY_STREAMER_CLOSE, QUERY_STREAMER_LAUNCH
{streamer, cmsId, appName, shortName, iconUrl}, QUERY_STREAMER_IS_INSTALLED
{cmsId}, QUERY_STREAMER_INSTALL {streamer, cmsId, appName, shortName, iconUrl},
QUERY_READ_UPDATE_TICKET, QUERY_GFN_NETWORK_TEST {address, user, deviceId,
platformId, profiles, latencyLimit, latencyRecommended, frameLossLimit, ...},
QUERY_GFN_LATENCY_BASED_ROUTING {user, deviceId, platformId, addresses},
QUERY_GFN_UPDATE_APP.

### D.6 IPC + config store (vendor.js:194339, 203514-204400)

| Command | Request fields | Purpose | Impl |
|---|---|---|---|
| QUERY_IPC_EXTENSION_MESSAGE | module, request (JSON-stringified) — or {system:"CrimsonNative", module, method, payload} for nativeQuery | generic IPC to host extensions (nativeQuery / onJsonMessage / registerForNotification persistent) | ✗ |
| QUERY_IPC_PUSH_MESSAGE / POP / CLEAR / GET_NUMBER_OF_MESSAGES | message / — | host message queue | ✗ |
| QUERY_READ_CONFIG {appname} / QUERY_WRITE_CONFIG {appname, data} | — | host app-config store | ✗ |
| QUERY_OPEN_CUSTOM_LAYER / QUERY_CLOSE_CUSTOM_LAYER | — | custom HUD layer | ✗ |

### D.7 Shared storage (✓ both)

QUERY_READ_SHARED_STORAGE {path}, QUERY_WRITE_SHARED_STORAGE {path, data}
(vendor.js:194517/194608) — implemented.

**Diff summary: 14 implemented / 82 known.** Not-implemented but *osc-page-relevant*
(after the gallery pivot): QUERY_BROWSE_DIRECTORY (Open Location), QUERY_OSC_DROP_URL
(drag-drop to upload), QUERY_WIN_KB_MESSAGE (on-screen keyboard), QUERY_TIME_INFO /
QUERY_SYSTEM_INFO (host pages). Everything in D.4-D.6 is Ansel/GFN/Experience-side —
skip until those features are rebuilt.
