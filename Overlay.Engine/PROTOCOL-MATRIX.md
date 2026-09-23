# OscEngine Protocol Matrix (M1) — ground truth from real code

Everything in this file was read from the actual production sources, not
inferred from design. Every claim cites `file:line`. This is the contract
`OscEngineClient` must reproduce and the tests must assert byte-exactly.

## Wire framing (hub: `API/[Forms - Project Files]/[API]/Server.vb`)

| Fact | Evidence |
|---|---|
| Line-delimited TCP, port **5001**, loopback-only (127.0.0.1 + ::1) | Server.vb:15, StartServer |
| Every message a client writes is **broadcast verbatim** to all other clients (sender excluded) — the `[Send] ` prefix is NOT rewritten | Server.vb:340 `Broadcast(msg, info)` → :433 `c.Writer.WriteLine(msg)` |
| `ping` is answered point-to-point with `[System]|pong` (never broadcast) | Server.vb:378-388 |
| Max line 64 KB; 60 s inactivity kill; ping every 10 s keeps clients alive | Server.vb:314, 333, HeartbeatMonitor |
| `register:<name>` sets the hub's client list display name (optional) | Server.vb:391-394 |

### Parser contract (identical in Engine + Overlay + Hub)

All consumers do the same parse:

```vb
parts = msg.Split("|"c)          ' ALL pipes
data  = parts(1)                 ' ONLY the second segment is used
cmd   = data up to FIRST ":"c    ' value may contain ":" (paths!)
value = data after first ":"
sender= parts(0) minus "[Send] "/"[Receive] " prefix (engine self-filter, exact compare)
```
Evidence: Engine `[Engine] Client.vb:145-168`, Overlay `[Overlay] Client.vb:142-157`, Hub `Server.vb:358-376`.

### ⚠ The pipe rule (measured defect in the live system)

Because the hub broadcasts verbatim and every parser takes `parts(1)` only,
**any `|` inside a value is truncated away at the receiver.** This is not
theoretical — it silently eats data today:

- `PREWARM_FFMPEG:<path>|<encoder>` sent by Overlay (`[1] Sub_Record` / Client.vb:187) arrives at the Engine as value `<path>`; the encoder suffix is dropped (`[Engine] Client.vb:173-176` looks for a pipe inside `value`, which can never be there).
- `engine_get_status` rehydration data `Recording|<sec>|<path>` sent by `UI_Engine.vb:617` / `RecordingEngineHost.vb:642` arrives as `…,ok,Recording` — elapsed/path are lost (Overlay's `statusFields(1)` rehydration at `[Overlay] Client.vb:387-414` never fires beyond the state word).
- `engine_recording_progress:<sec>|<frames>|<size>` sent by `RecordingEngineHost.vb:185` arrives as just `<sec>`; the Overlay's 3-field parse (`<Overlay> Client.vb:256-257`) bails, so size/frames never render.

**OscEngine rules that follow:**
1. NEVER put `|` in a value we send. `RECORD_START` paths cannot contain `|` (illegal in Windows filenames — also asserted by the engine's own comment, `UI_Engine.vb:612-613`).
2. `PREWARM_FFMPEG` is sent **path-only** (engine-visible effect identical to the Overlay's truncated send, without the dead suffix).
3. Receiving side parses defensively: `engine_get_status` state = `value.Split(","c)(2)` first pipe-free token; progress uses field 0 only if the rest is truncated.

## Commands OscEngine SENDS (M1)

Format: `[Send] NVIDIA Overlay Engine|<cmd>[:<value>]`

| Command | Exact line (template) | Trigger | Engine handler |
|---|---|---|---|
| register | `[Send] NVIDIA Overlay Engine\|register:NVIDIA Overlay Engine` | after connect/reconnect (mirrors Engine's own register, `Client.vb:105`) | Server.vb:391 |
| RECORD_START | `[Send] NVIDIA Overlay Engine\|RECORD_START:<outputPath>` where outputPath = `<SavePath>\Record_yyyy-MM-dd_HH-mm-ss.mp4` | record toggle in osc / tray | alias → `engine_record_start`, `Client.vb:190` |
| RECORD_STOP | `[Send] NVIDIA Overlay Engine\|RECORD_STOP` | record toggle off | alias → `engine_record_stop` |
| PREWARM_FFMPEG | `[Send] NVIDIA Overlay Engine\|PREWARM_FFMPEG:<ffmpegPath>` (path ONLY — see pipe rule) | on `engine_ready` (mirrors Overlay Client.vb:182-190) | `HandleEnginePrewarmFFmpeg`, UI_Engine.vb:646 |
| engine_get_status | `[Send] NVIDIA Overlay Engine\|engine_get_status` | bounded pull 2 s × 10 on start + on reconnect + on engine_ready (mirrors Overlay Client.vb:54-105) | UI_Engine.vb:607 / RecordingEngineHost.vb:623 |
| ping | `[Send] NVIDIA Overlay Engine\|ping` every 10 s | TcpClientHelper.PingLoop (verbatim reuse) | Server.vb:378 |
| engine_config_changed | `[Send] NVIDIA Overlay Engine\|engine_config_changed:video` (or `audio`) | M1: only when osc's `/Record/Settings` POST changes a value (if that surface is reachable); normally M2 | Client.vb:182 |

Value sources: `Paths.SavePath` / `Paths.FFmpegPath` read via `AppConfigShared.ReadString` (Common/AppConfigShared.vb:101) from `Config\config.json` — the same file the Forms overlay's AppSettings owns. Missing → fallbacks: SavePath → `Documents\NVIDIA ShadowPlay\videos`, FFmpegPath → skip the send.

## Events OscEngine RECEIVES (M1)

Exact received lines (hub rebroadcasts the sender's `[Send] …` verbatim):

| Received line | Meaning | OscEngine action |
|---|---|---|
| `[Send] NVIDIA Engine\|engine_ready` | engine connected/reconnected | re-send PREWARM_FFMPEG + engine_get_status (mirror Client.vb:180-200) |
| `[Send] NVIDIA Engine\|engine_response:engine_record_start,ok` | record accepted | osc state → recording; toast via osc Notification channel |
| `[Send] NVIDIA Engine\|engine_response:engine_record_start,error[,reason]` | record rejected | osc state → idle; surface reason |
| `[Send] NVIDIA Engine\|engine_response:engine_record_stop,ok[,why]` | stop accepted (why=pass/fail evidence, RecordingEngineHost.vb:607-612) | osc state → idle |
| `[Send] NVIDIA Engine\|engine_response:engine_get_status,ok,<state>[|<sec>|<path>]` (only `<state>` and possibly `<sec>` survive the pipe rule — parse defensively) | status answer; ANY answer retires the bounded pull | set osc recording state; seed elapsed if present |
| `[Send] NVIDIA Engine\|engine_state_changed:<State>` | Recording/Idle/Stopping/HasError reconcile (UI_Engine.vb:1093-1098) | same reconcile as Overlay Client.vb:233-252 |
| `[Send] NVIDIA Engine\|engine_recording_progress:<sec>[|<frames>|<size>]` (frames/size usually truncated away) | 1 s heartbeat while recording | update osc elapsed |
| `[Send] NVIDIA Engine\|engine_recording_saved:<filePath>` | session saved (RecordingEngineHost.vb:603) | osc idle + notification |
| `[Send] NVIDIA Engine\|engine_recording_error:<message>` | session failed | osc idle + error notification |
| `[Send] Launcher\|open_overlay` | Launcher-requested toggle (Launcher Main.vb:199) | push `overlayToggle` to osc |
| `[System]\|pong` | filtered inside TcpClientHelper | — |

Sender identification: `[Send] ` prefix stripped, exact compare against `NVIDIA Engine` / `Launcher` — NOT substring (the M9 self-filter lesson, `Client.vb:150-156`).

## Controller-server contract (osc side) — golden-verified

Captured by running the real client libraries (`engine.io-client@3.5.4` +
`socket.io-client@2.5.0`, the same family bundled in `osc/vendor.js` —
fingerprint: `has-binary`/`json3`/`parsejson`/`component-emitter` modules +
`EIO=u.protocol` with `protocol=3`) against a scripted server:
`Tester/test/Overlay/Overlay.OscEngine.Tests/golden/golden-transcript.json`,
harness in the same folder.

| Fact | Golden evidence |
|---|---|
| Handshake `GET /socket.io/?X_LOCAL_SECURITY_COOKIE=<secret>&EIO=3&transport=polling&t=<rand>&b64=1` — custom query opts ride in the URL | step[0] |
| Open packet response is length-prefixed like every batch: `<len>:0{"sid":…,"upgrades":[],"pingInterval":2000,"pingTimeout":60000}` — an UNPREFIXED `0{…}` fails the client with `parser error` (measured) | harness run 1 |
| The client never sends CONNECT (`40`) for the default namespace — the SERVER sends `40` on the first poll; only then the client surfaces `connect` (`socket.io-client/lib/socket.js:190-198`: `if ('/' !== this.nsp)` skips the connect packet) — **true for socket.io-client v2; the v1.x client bundled in osc DOES also POST a `40` after open; our server tolerates both** | steps[2-3], vendor.js |
| Events to the page: `<len>:42["<channel>",<payload-json>]`; multiple packets per response are allowed (`<len1>:…<len2>:…`) | steps[4], batch push |
| Client emits POST `<len>:42["<channel>",<payload>]` | steps[POST] |
| **Heartbeat: the CLIENT pings** — POST body `1:2` every `pingInterval`; server must deliver `3` in a later poll response (measured with pingInterval=2000; connection stayed alive across cycles) | POST bodies |
| Batch encoding: `<len>:<packet>` concatenated; len counts the packet string's chars | all responses |
| osc consumes via `io("http://localhost:"+port, {query:{X_LOCAL_SECURITY_COOKIE:secret}})` and `socket.on("/ShadowPlay/v.1.0/…")` — `socketService`/`oscDisplayService` in app.js | vendor/app analysis |

### ⚠ Polling RESPONSE MODE (browser clients) — measured against the real page

The osc bundle ships **socket.io-client 1.x-era libraries** (fingerprint:
`has-binary`/`json3`/`blob`/`arraybuffer.slice` modules in manifest.json).
Its polling XHR sets `responseType="arraybuffer"` and its `onLoad`
(vendor.js, verbatim) routes the response:

```js
e = "application/octet-stream" === contentType ? xhr.response
   : supportsBinary ? "ok"                       // ← dummy → parser error!
   : xhr.responseText;
```

Therefore, for browser clients (handshake WITHOUT `b64=1`):
1. Every GET response MUST carry **`Content-Type: application/octet-stream`**
   — a text/plain body is discarded and replaced by the literal string
   `"ok"`, which fails the parser (`polling got data "ok"` → `parser error`
   → infinite reconnect storm of bare handshakes — measured live).
2. The body uses the **binary framing** of engine.io-parser 1.x
   `decodePayloadAsBinary` (extracted verbatim from vendor.js):
   per packet: `0x00` | ASCII decimal byte-length | `0xFF` | utf8 payload.
3. POST responses must decode to ZERO packets — empty octet-stream body.

Clients that request `b64=1` (the npm golden harness) keep the text
framing `<len>:<packet>` with text/plain. OscControllerServer switches
per request on the `b64` query parameter.

Channels the page subscribes (M1 subset in brackets):
`/ShadowPlay/v.1.0/WindowState` [`windowMsg: overlayToggle|showHotkeyMessage|dismiss|fullscreenTransition`],
`/ShadowPlay/v.1.0/DisplayOscState` [`{state,params}`],
`/ShadowPlay/v.1.0/Notification`,
`/ShadowPlay/v.1.0/DisplayOscPreferences`,
`/Settings/v.1.0/Language`,
`/PiplConfig/v.1.0/update` (push `{}` — keeps external servers empty, offline-first).

Native bridge (cefQuery): `QUERY_WIN_NODE_INFO` → `{"port":<p>,"secret":"<s>"}` (boot-critical; without it osc degrades to port 3000 + empty cookie), `QUERY_FULLSCREEN_STATE`, `QUERY_OSC_SET_DISPLAY_RECTS`, `QUERY_OSC_SET_PAINTING`, `QUERY_OSC_REGISTER_CLOSE_EVENT` (persistent), `QUERY_OSC_DISPLAY_IS_DESKTOP_MODE`, `QUERY_WIN_OPEN_OSC`/`QUERY_WIN_CLOSE_OSC`, `QUERY_READ/WRITE_SHARED_STORAGE`, `QUERY_LOAD_STRING_TABLE`, `QUERY_WIN_COPY_TO_CLIPBOARD`; everything else → logged + rejected so services degrade cleanly.

### Phase 5 additions (2026-09-24 — ZCODE-CEFQUERY-PHASE5, evidence in docs/OSC-GALLERY-IPC-EXTRACTION.md + unpacked bundles)

| Command | Request | Our behavior | Consumer evidence |
|---|---|---|---|
| `QUERY_BROWSE_DIRECTORY` | `{name}` (start folder) | native `FolderBrowserDialog` on the UI thread (OscHostForm injects `CefQueryBridge.FolderPicker`), owned by the overlay so it stacks above the topmost window; on OK opens the picked folder via shell and resolves `"true"`; on cancel resolves `"false"` — deterministic, no hang; picker unwired → fail `-1 folder_picker_unavailable` | app module 181:86 (gallery "Open Location": `closeOSC()` then `browseDirectory(folder)`, result ignored); cefService wrapper maps `"true"`/`"false"` → boolean, errorCode 204 = cancel |
| `QUERY_OSC_DROP_URL` | `{url, xpos, ypos}` | log + resolve `"true"` (ack). No file-drop pipeline — the page registers the expected drop URL next to its own `$document` dragover/drop handlers and ignores the response | app module 4:163 + vendor 129 `uploadService` (`oscCreateDropUrl(e,0,0)`, return value unused) |
| `QUERY_WIN_KB_MESSAGE` | `{keycode, keymodifier}` | log + resolve `"true"` (ack). No global keyboard hooks, no new listeners — our host has no native widgets to focus | app modules 91/218 (gamepad poll → `oscSendWinKBMessage(code, modifier)`, e.g. TAB ± SHIFT, result unused) |
| `QUERY_TIME_INFO` | `{type}` | NOT implemented — stays on the `-1 not_implemented` failure path | wrapper only (vendor 125:212); zero callers in the shipped page (vendor=definition, app=0, common=0) and no response shape recoverable — mission card forbids guessing |
| `QUERY_SYSTEM_INFO` | — | NOT implemented — same failure path | wrapper only (vendor 125:539); every page `getSystemInfo()` goes through `hardwareService` → HTTP `/HardwareInformation`, never cefQuery (modules 22/25/40/136/260) |

## Serving strategy (origin/CSP decision)

osc static files are served by the SAME controller server (same origin
`http://localhost:<port>`), so the page's REST/socket.io calls are
same-origin — no CORS, no preflight, no virtual-host mapping, no
cross-origin policy surface. Loopback-only bind + secret cookie on API
routes; static GETs stay cookie-free (Angular's `$translate` fetches l10n
relatively without headers).
