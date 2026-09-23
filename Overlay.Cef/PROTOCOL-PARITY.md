# PROTOCOL-PARITY.md — cefQuery bridge: proven VB contract ↔ CEF lane C++ port

Authority: `Overlay.Engine\CefQueryBridge.vb` (missions 3 + phase 5, commits
`587b60d298`..`a31ecb28d6`). The CEF lane host (`NVIDIA Share.dll`,
`ShareQueryHandler`) ports the dispatch 1:1 onto the NATIVE CEF message
router — the mechanism the real GFE `NVIDIA Share.exe` used
(`window.cefQuery` / `window.cefQueryCancel` are the router's default
integration functions; CEF 73 `cef_message_router.h:200-210`).

## Page-visible contract (unchanged)

```
window.cefQuery({
  request:     <JSON STRING>   // {command: "QUERY_…", …args}
  persistent:  <bool>          // persistent = push channel (onSuccess repeatedly)
  onSuccess:   (responseString) => …
  onFailure:   (errorCode, errorMessage) => …
}) → handle { id, cancel() }
```

The `{__cef:1}` / `{__cefResponse:1}` envelope in CefQueryBridge.vb:11-12
exists ONLY on the WebView2 polyfill wire. On the CEF host the router
carries the same payloads natively: `OnQuery(request string)` → response
string to `onSuccess` / `(code, message)` to `onFailure`. Cancel
(`handle.cancel()`) → `OnQueryCanceled` (page-side isCancelled detection,
crimson wrapper `docs/osc/unpacked/src/vendor/0125.applicationLifetimeService.js:169-184`).

## Command parity table (CefQueryBridge.vb line → CEF lane)

| Command | VB response (line) | CEF lane response | Status |
|---|---|---|---|
| QUERY_WIN_NODE_INFO | `{"port":N,"secret":"…"}` (:209-213) | identical, port = lane HTTP server, secret = per-run RandomHex(16) | ✅ proven in proof run (page boot: "Node info found") |
| QUERY_FULLSCREEN_STATE | constant `{"fullscreen":false,"hdractive":false,"borderlessMode":null}`, probe logged (:215-225) | identical; WinFullscreen probe ported (share_win.cpp) | ✅ |
| QUERY_OSC_DISPLAY_IS_DESKTOP_MODE | `"true"` (:227-232) | identical | ✅ |
| QUERY_OSC_SET_DISPLAY_RECTS | parse displayRects ([x,y,w,h] or {x,y,w|h,h|h}), store, `"true"` (:234-242) | identical parser (OscProtocol.ParseDisplayRects port) | ✅ |
| QUERY_OSC_SET_PAINTING | enablePainting → `"true"` (:244-248) | identical | ✅ |
| QUERY_OSC_SET_EXPERIMENTAL | `"true"` (:250-251) | identical — used as the synthetic proof round-trip command | ✅ PROOF_ECHO_VERIFIED |
| QUERY_OSC_REGISTER_CLOSE_EVENT | persistent → no immediate response; push later via RequestCloseFromPage (:253-257) | identical: callback kept (router persistent semantics), PushCloseFromHost() | ✅ implemented, push unused in proof |
| QUERY_WIN_OPEN_OSC / QUERY_WIN_CLOSE_OSC | events + `"true"` (:259-265) | identical; host window Show/Hide | ✅ |
| QUERY_READ/WRITE_SHARED_STORAGE | SharedStorageStore JSON file `Data\osc-shared-storage.json`, missing → `""` | identical file+semantics (share_storage.cpp) | ✅ |
| QUERY_LOAD_STRING_TABLE | ack `"true"` (:275-277) | identical | ✅ proven organically by the page boot |
| QUERY_WIN_COPY_TO_CLIPBOARD | Clipboard.SetText → `"true"` (:279-288) | identical (CF_UNICODETEXT) | ✅ |
| QUERY_HTTPSERVER_START | fail -1 `oauth_not_implemented` (:290-293) | identical | ✅ |
| QUERY_BROWSE_DIRECTORY | picker wired → true/false; unwired → fail -1 `folder_picker_unavailable` (:295-332) | parity with the UNWIRED engine state (fail fast, deterministic no-hang); native IFileDialog picker = later integration round | ⚠ documented deviation |
| QUERY_OSC_DROP_URL | logged ack `"true"` (:334-344) | identical | ✅ |
| QUERY_WIN_KB_MESSAGE | logged ack `"true"` (:346-355) | identical | ✅ |
| QUERY_TIME_INFO / QUERY_SYSTEM_INFO | deliberately NOT implemented (page never calls them; CefQueryBridge.vb:19-29) | same: not implemented | ✅ parity preserved |
| (unknown) | fail -1 `not_implemented` (:357-359) | identical | ✅ |
| (malformed) | fail -3 `bad request json` / `request not object` / `no command` (:189-199) | identical | ✅ |

`__PROOF_*` commands (`__PROOF_DOM`, `__PROOF_ECHO`, `__PROOF_OPEN_UI`) are
CEF-lane-only proof observers — namespaced so they cannot collide with the
production `QUERY_*` namespace. They are the CEF equivalent of the
engine's `window.__poc*`/postMessage observation path.

## Build flags (evidence)

Mirrors the distribution's own `cmake/cef_variables.cmake` (Windows block):
`/MT /Gy /GR- /W4 /Zi`, defines `WINVER=0x0601 _WIN32_WINNT=0x601 NOMINMAX
WIN32_LEAN_AND_MEAN _HAS_EXCEPTIONS=0 UNICODE _UNICODE` (+`NDEBUG` in
Release), standard libs `comctl32 rpcrt4 shlwapi ws2_32`. Documented
deviations: `/WX` dropped (newer MSVC emits new warnings in 2019-era
wrapper code), no `cef_sandbox.lib` (`no_sandbox=1`).

## Measured runtime findings (first runs, kept as evidence)

1. `resources_dir_path` must point at the CEF ROOT (where `cef.pak` +
   `locales\` sit beside NVIDIA Share.exe — GFE 3.28 layout). Pointing it
   at `Resources\` crashed `CefInitialize` ("Could not load cef.pak" →
   segfault).
2. A hidden parent window stalled the GPU-process spawn + first
   navigation by ~100s (Win11 26340 + Chromium 73). Proof runs show the
   window (`--hidden` suppresses).
3. `CloseBrowser(true)` alone never completes for a `SetAsChild` browser:
   the app must destroy its own top-level window (CEF close contract,
   `cef_life_span_handler.h` examples) — `DoClose` now destroys the host
   window; `OnBeforeClose` → `CefQuitMessageLoop` follows.
4. The OSC page builds backend URLs against `localhost:<port from
   QUERY_WIN_NODE_INFO>` regardless of page origin — navigation uses
   `127.0.0.1` (localhost resolution stalled the first navigations), so
   backend XHRs are CORS-blocked in the standalone CEF host and services
   degrade (the documented degradation path). Backend integration is a
   later round.
