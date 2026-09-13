# OSC REVERSE MAP — On-Screen Control (Overlay/osc)

Reverse-engineered from the actual artifacts in `Overlay/osc/` (compiled
Webpack bundles — **no source maps exist anywhere in the repository**, see
02-bundle-map). Every claim cites its evidence. Companion to the host-side
`Overlay.Engine/PROTOCOL-MATRIX.md` (worker-owned, read-only for this effort).

## Files

| Doc | Content |
|---|---|
| [01-inventory.md](01-inventory.md) | Phase 0 — artifact inventory (FILE/TYPE/SIZE/ROLE) |
| [02-bundle-map.md](02-bundle-map.md) | Phase 1/2 — webpack + Angular module map, source-map verdict |
| [03-protocol-map.md](03-protocol-map.md) | Phase 3/4 — cefQuery commands, socket.io channels, REST endpoints, wire facts |
| [04-state-model.md](04-state-model.md) | Phase 5 — state the UI keeps and where each value comes from |
| [05-screen-map.md](05-screen-map.md) | Phase 6 — ui-router state tree (38 states, exact URLs) |
| [06-ui-components.md](06-ui-components.md) | Phase 7 — directives/controllers/services matrix |
| [07-boundary-matrix.md](07-boundary-matrix.md) | Phase 8 — KEEP / REIMPLEMENT / REMOVE / UNKNOWN |
| [08-host-contract.md](08-host-contract.md) | Phase 11/12 — WebView2 host facts, M1 host surface, hotkey ownership |
| [09-record-flow.md](09-record-flow.md) | **Behavior reconstruction** — Manual Record: UI tile → service → POST /Record/Enable → confirm channels → state |
| [10-replay-flow.md](10-replay-flow.md) | **Behavior reconstruction** — Instant Replay start/stop/save + buffer-length toast chain |
| [11-window-flow.md](11-window-flow.md) | **Behavior reconstruction** — overlay open/close/toggle refcounting, fullscreen transitions, WindowState channel |
| [unpacked/](unpacked/README.md) | **FULL bundle unpack**: every module of vendor.js (291) + app.js (487) as beautified per-module files, extracted HTML templates + CSS, `names.json` (Angular name → module), per-bundle INDEX with requires graph |

## Headline findings (all evidence-backed)

1. **The OSC is a GFE 3.28 Angular 1.x SPA** (`ng-app="main"`, ui-router +
   ngMaterial + pascalprecht.translate) shipped as a 3-file Webpack bundle
   (`vendor.js` 1.4 MB, `app.js` 1.6 MB, `common.js` 0.9 KB — common.js is the
   webpack runtime; chunk loading is configured but no extra chunks ship).
2. **Two transports, one origin.** The page talks to its host over
   `window.cefQuery` (CEF message router; in WebView2 the host polyfills it
   over `chrome.webview.postMessage`) and over **socket.io v2 on engine.io v3
   polling** to `http://localhost:<port>` with the auth secret in the URL
   query (`X_LOCAL_SECURITY_COOKIE`). REST calls ride the same server with
   the secret as a header. There is no WebSocket upgrade — polling only.
3. **Boot is resolve-gated**: the `base` ui-router state resolves
   `QUERY_WIN_NODE_INFO → {port, secret}` before anything renders; without
   the node info the app degrades to port 3000 + empty cookie (and the M1
   host then 401s every socket poll — measured).
4. **24 socket channels** are referenced (16 top-level + 8 ShadowPlay
   sub-channels); the M1 host pushes only `/ShadowPlay/v.1.0/Notification`
   today. The full list is subscribed by the legacy app and preserved by the
   new UI, so nothing the backend already sends is dropped.
5. **The click-through contract is `QUERY_OSC_SET_DISPLAY_RECTS`**: the host
   passes mouse input only inside rects the page reports. The legacy menu
   clears rects on open and reports panel bounds once rendered. This is the
   single most host-visible UI behavior and must be preserved by any ReUI.
6. **Hotkeys never belong to the page.** The page only *displays* hotkey
   strings; registration lives with the Forms overlay / host (M1: none —
   `OscHostForm.vb` header). The page must not register hotkeys or the
   WinForms overlay and the engine would fight over Alt+Z/Alt+F9/Alt+F10.
7. **Not everything legacy is needed for ShadowPlay**: broadcast/upload/
   co-play/NvCamera/mods/OC-tool surfaces have no M1 host contract; the
   boundary matrix marks each REMOVE/UNKNOWN with its evidence.
