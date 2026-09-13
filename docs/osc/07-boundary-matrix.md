# 07 — Boundary Matrix (Phase 8): KEEP / REIMPLEMENT / REMOVE / UNKNOWN

Rule: REMOVE requires artifact evidence; anything unproven stays UNKNOWN.

## KEEP (contract — must not change)

| Item | Evidence |
|---|---|
| `window.cefQuery` request/response semantics (true/false/push/204/reject-on-missing) | vendor.js cefService; host CefQueryBridge.vb mirrors it |
| All `QUERY_*` command names + payload fields the host implements (14 commands, see 03) | bundle extraction + CefQueryBridge.Dispatch |
| `QUERY_OSC_SET_DISPLAY_RECTS` click-through model (rects in client coords; empty while closed) | oscDisplayService + OscHostForm.WM_NCHITTEST |
| `QUERY_OSC_REGISTER_CLOSE_EVENT` persistent close push | oscDisplayService.I() |
| socket.io v2/engine.io v3 polling wire (handshake, server CONNECT, client ping, length-prefixed batches, 401 on bad cookie) | golden transcript |
| All 24 socket channel names + WindowState payload semantics | app.js literals |
| REST auth: `X_LOCAL_SECURITY_COOKIE` header (REST) / query (socket) | localSdk + endpoint factory |
| M1 REST routes `/state`, `/Record/Enable`, `/Record/Settings`, `/RecordPaths`, `/Language`, `/uiReady`, `/support`; unknown→`{}` | OscControllerServer.vb |
| Record payload shape `{status:"true"|"false"}` | shadowPlayEndpoints data template |
| Localization: `l10n/<lang>.json` tables (673 keys), language from `/Language` | l10n dir + REST routes |
| Shared storage keys via `QUERY_READ/WRITE_SHARED_STORAGE` | cefService + SharedStorageStore |
| Engine reconcile semantics: `Recording/Idle/Stopping/HasError` (Stopping keeps state) | OscProtocol.ShouldShowRecording |
| Boot ordering: node info → config → socket → uiReady | base resolve chain + M1 host logging |

## REIMPLEMENT (presentation — free to change, changed in `next/`)

| Item | Old | New |
|---|---|---|
| Framework | Angular 1.x + ngMaterial + webpack bundle (3 MB JS) | Vanilla ES modules, zero build, ~30 KB |
| Layout | Full-screen GFE menu grid | Right-side drawer (380 px) + toasts |
| Visual language | GFE 3.28 theme | Modern NVIDIA dark: `#76b900` accent, cards, glass panel |
| Navigation | ui-router URLs + resolves | view stack in shell (no URL semantics — page is a single overlay window) |
| Record control | tile in main-menu grid | hero card with REC indicator, elapsed, stop&save |
| Settings | 15 preference sections | single Recording section + save path (M1 surface) |
| State layer | scattered service internals | one observable store |
| Template delivery | inlined in bundle | ES module DOM builders |

## REMOVE (evidence-backed, M1 scope)

| Feature | Evidence |
|---|---|
| Broadcast (twitch/youtube/facebook/weibo), uploads (imgur/google/swgf) | providers configured in config.js but M1 host has no broadcast/upload REST or channels; OAuth proxy (`QUERY_HTTPSERVER_START`) fails fast by host design |
| Gallery / upload history | no gallery REST on M1 host; gallerySdk dead |
| Co-play / guest controls / GameShare | no M1 contract |
| NvCamera / Ansel / mods / edge / OC-tool (octoolmenu) | feature-flagged (`nvCamera`, `mods`, `perfmonOCTool` in config.js) and no M1 contract; `QUERY_OSC_SET_EXPERIMENTAL` answered but never drives UI |
| Highlights | `/Highlights/*` REST exists in legacy scheme; M1 host catch-all `{}` → dead |
| Telemetry / jarvis / jsEvents | servers configured empty (config.js `server:""`, `redirect.server:""`) — offline-first by design |
| Facebook reactions assets | tied to removed broadcast |
| Webrtc chat | coplay-only |

## UNKNOWN (do not guess — needs host/backend work first)

| Item | Why unknown |
|---|---|
| OSD status/perf indicators on the new UI | display-rects/painting commands exist, but no M1 source pushes `/DisplayOscState`; the Forms overlay currently owns OSD |
| OAuth/account surfaces | `QUERY_HTTPSERVER_START` implemented-but-fails in M1; revisit when account scope lands |
| Replay controls | engine client M1 has no replay commands; wire when OscEngineClient grows them |
| Language switching | M1 `/Language` fixed `en-US`; loader + 28 tables already wired |
| `QUERY_OSC_SET_PAINTING` from the menu | legacy menu path never calls it (only perf paths); left unsent by ReUI — harmless either way |
| Legacy REST prefixes (`/ShadowPlay/v.1.0/…`) | documented for the future full backend; M1 host uses unprefixed routes |

## Old vs New invariant (the acceptance bar)

```
OLD OSC behavior ≈ NEW OSC behavior   on every KEEP row
OLD UI         ≠ NEW UI               on every REIMPLEMENT row
```
