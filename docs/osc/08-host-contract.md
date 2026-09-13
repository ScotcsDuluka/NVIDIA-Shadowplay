# 08 — Host Contract: WebView2, M1 Host, Hotkeys (Phase 11 + 12)

The host is the OTHER worker's active work area (`Overlay.Engine/` — files
were being modified during this analysis). Everything here was read from
their sources as evidence; **nothing in `Overlay.Engine/` was modified**.

## Serving model (FACT — OscControllerServer.vb / OscHostForm.vb)

| Fact | Evidence |
|---|---|
| Static root: first of `StartupPath\osc`, `..\osc`, `..\..\osc` containing `index.html` | OscHostForm.ResolveOscRoot |
| Page navigated to `http://127.0.0.1:<port>/index.html` — **same origin for REST+socket** (no CORS, no virtual host mapping) | OnWebViewReady |
| Loopback-only bind on a free port (fallback 3000 = legacy LOCALHOST_PORT) | OscControllerServer ctor/FindFreePort |
| Secret: 6 digits + 12 hex chars per boot; static GETs are cookie-free; API routes + socket require it (header or query) | HasCookie |
| cefQuery polyfill injected **before page scripts** via `AddScriptToExecuteOnDocumentCreated`; page↔host messages: `{__cef:1,id,request,persistent}` ↔ `{__cefResponse:1,id,ok,response|errorCode}` | CefQueryBridge.PolyfillSource |
| WebView2: transparent background, DevTools only with `OSCENGINE_DEVTOOLS=1`, default context menus disabled | InitWebView |
| Window: borderless, covers primary screen, TopMost, WS_EX_TOOLWINDOW; closed = Opacity 0 + HTTRANSPARENT everywhere; open = click-through outside reported displayRects | OscHostForm |
| Push channel host→page: `PushEvent(channel, json)` over the socket.io session | OscControllerServer.PushEvent |

## M1 cefQuery dispatch (implemented commands)

`QUERY_WIN_NODE_INFO` (returns the controller-server `{port, secret}` —
boot-critical), `QUERY_FULLSCREEN_STATE` (foreground-vs-monitor probe,
`hdractive:false, borderlessMode:null`), `QUERY_OSC_DISPLAY_IS_DESKTOP_MODE`
(false), `QUERY_OSC_SET_DISPLAY_RECTS` (stored, drives WM_NCHITTEST),
`QUERY_OSC_SET_PAINTING` (stored), `QUERY_OSC_SET_EXPERIMENTAL` (ack),
`QUERY_OSC_REGISTER_CLOSE_EVENT` (persistent; **no immediate response** —
pushed later via RequestCloseFromPage), `QUERY_WIN_OPEN_OSC`, 
`QUERY_WIN_CLOSE_OSC`, `QUERY_READ/WRITE_SHARED_STORAGE`,
`QUERY_LOAD_STRING_TABLE` (ack), `QUERY_WIN_COPY_TO_CLIPBOARD`,
`QUERY_HTTPSERVER_START` (**fails fast** `oauth_not_implemented`).
Unknown → `-1 not_implemented`. ⚠ Parity trap observed: a server that ANSWERS
REGISTER_CLOSE_EVENT immediately makes the page close itself at boot — the
test harness initially did this and was fixed to match the host (hold the
response until the push).

## M1 engine path (page → TCP hub :5001)

```
POST /Record/Enable {"status":"true"}
  → OscHostForm.OnRecordEnableRequested
  → RECORD_START:<SavePath>\Record_yyyy-MM-dd_HH-mm-ss.mp4   (no '|' in value!)
  → hub broadcast → NVIDIA Engine
  ← engine_response:engine_record_start,ok / engine_state_changed / 
    engine_recording_progress / engine_recording_saved|error
  → host fields (_recording, _elapsedSec) → GET /state for the page
```
The page never sees hub messages directly — the host mediates. Pipe rule:
**values must never contain `|`** (hub truncates at the receiver).

## Hotkey ownership (Phase 12 — FACT)

| Fact | Evidence |
|---|---|
| M1 host registers **NO hotkeys** | OscHostForm.vb header: "Hotkeys: NONE in M1 (ownership stays with the Forms overlay)" |
| Alt+Z / Alt+F9 / Alt+F10 / Alt+Shift+F10 remain with the **WinForms overlay** | same header + PROTOCOL-MATRIX hotkey note |
| Toggle paths in M1: hub `open_overlay`, tray menu, or the page itself (`QUERY_WIN_OPEN_OSC`/`QUERY_WIN_CLOSE_OSC` events wired to `ToggleOverlay`) | OscHostForm.StartStack |
| Page-side `/ShadowPlay/v.1.0/Hotkey` channel: logged only (M2 mapping) | OnSocketEvent |
| The page MUST NOT register system hotkeys (double-registration risk) | design rule derived from the above |

ReUI compliance: the new UI registers no hotkeys; Escape handling is
page-internal (only when the webview already has focus); all hotkey strings
shown in UI come from push events, never from local registration.

## Environment facts that shape the UI (WebView2 compatibility list)

- Origin = `http://127.0.0.1:<port>` → use `127.0.0.1` (not `localhost`) for
  every call or the page goes cross-origin (CORS/preflight).
- ES modules: WebView2 (Chromium) supports them — the new UI ships ES modules
  directly; the legacy bundle needed webpack only for packaging.
- Transparency: `DefaultBackgroundColor = Transparent`; html/body must stay
  unpainted outside panels.
- Click-through: only `data-osc-interactive` bounds receive input — layout
  changes MUST re-report rects (ResizeObserver + rAF debounce).
- `ng-csp` irrelevant to the new UI (no Angular).
- User-data folder: `Data/WebView2-osc` (AppLayout) — separate profile.
- Process teardown: server stops long-polls before webview dispose — page
  reconnect logic must tolerate instant 401/net errors on shutdown.
