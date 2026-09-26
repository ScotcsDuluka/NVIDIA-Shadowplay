# EVIDENCE.md — CEF lane (NvOverlay\CEF\, NVIDIA Share.exe)

Date: 2026-09-24 · Branch: gfe-rebuild · Lane owner: `Overlay.Cef\`

## 1. Evidence collected BEFORE ordering the build

| Question | Finding | Source |
|---|---|---|
| CEF / cefQuery / OSC | cefQuery bridge proven in `Overlay.Engine\CefQueryBridge.vb` (16 commands + malformed ladder + persistent close channel); wire docs `docs\osc\03-protocol-map.md`, `docs\osc\08-host-contract.md` | repo |
| Overlay.Engine | WebView2 host (`OscHostForm.vb`), OSC served over loopback HTTP by `OscControllerServer.vb` (ephemeral port, `http://localhost:<port>/index.html`) | repo |
| WebView2 remnants | exactly ONE PackageReference (`Overlay.Engine\NVIDIA Overlay Engine.vbproj:28`) + host code; no other project touches it. NOT used by this lane | repo sweep |
| OSC resources | pristine bundle `Overlay\osc\` (NVIDIA 2015-16), runtime copy `Overlay.Engine\osc\` — served as `Resources\osc\` in the CEF owner slot | repo |
| NVIDIA reference binaries | GFE 3.28.0.412 full GFExperience copy: `C:\My Project\GFE\GeForce_Experience_v3.28.0.412\GFExperience\` (+ `A:\NV OSC Backup\NVIDIA ShadowPlay\`) — `NVIDIA Share.exe` 3,347,496 B (73.3683.1933.5), `libcef.dll` 111,341,608 B (**73.0.0-HEAD.1933+gee4b49f+chromium-73.0.3683.75**), `cef.pak` 3,660,519 B, `locales\` 53 pak, `NVIDIA Share.json` (nv-osc=true, nv-url-relative=osc/index.html) | disk sweep |
| CEF runtime (modern) | NVIDIA App: `C:\Program Files\NVIDIA Corporation\NVIDIA App\CEF\` (libcef 128.4.13 nv-fork) — reference only | disk sweep |
| CEF SDK | NONE on disk → downloaded (see §2) | disk sweep + download |
| Toolchain | VS Community 2026 18.10.2 (`C:\Visual Studio`), MSVC 14.51.36231, MSBuild 18.10.1, Windows SDK 10.0.26100.0 (`D:\SDK`, registry KitsRoot10) | machine |
| Architecture lane | `Logical Architecture\Layout.txt` v2 lines 21-28 draws `NvOverlay\CEF\` (NVIDIA Share.exe + NVIDIA Share.dll + *.dll + Resources + locales + cef.pak); `Common\AppLayout.vb` probes the lane; deploy scripts reserve it ([PENDING] CEF host replacement) | repo |

## 2. CEF SDK (pinned, hash-verified)

- Build: **cef_binary_73.1.12+gee4b49f+chromium-73.0.3683.75_windows64_minimal.tar.bz2**
- Source: https://cef-builds.spotifycdn.com/ (index.json)
- Size: 118,864,453 B · **SHA1 `9166371d513cd3a86c052778a6086f9a39e8e5c0` = exact match** with the index
- Decisive fact: Spotify's 73.1.12 carries CEF commit **gee4b49f** — the SAME commit the real NVIDIA `libcef.dll` version string carries (73.0.0-HEAD.1933+**gee4b49f**+chromium-73.0.3683.75). Headers/wrapper are API-compatible with the production runtime family, and Chromium 73.0.3683.75 is the exact renderer of the OSC bundle's host era (GFE 3.28).
- Extracted at `C:\My Project\cef-sdk\cef73\` (outside the repo; hash recorded above + in `CEF-RUNTIME-MANIFEST.txt`).

## 3. What was built (all new, CEF lane only)

```
Overlay.Cef\
├── libcef_dll_wrapper.vcxproj      CEF wrapper static lib (globs SDK sources)
├── NVIDIA Share.vcxproj            host body DLL  → NVIDIA Share.dll (2,487,808 B)
├── NVIDIA Share Bootstrap.vcxproj  thin bootstrap → NVIDIA Share.exe (554,496 B)
├── NVIDIA Share.json               evidence-shaped config (nv-osc / nv-url-relative …)
├── src\ share_main / share_app / share_client / share_query_handler /
│        share_http_server / share_storage / share_config / share_proof /
│        share_win / share_script.h / share_json.h / bootstrap.cpp /
│        version.rc / app.manifest
├── deploy-cef-owner.ps1            stages the owner slot (below)
├── run-proof-of-life.ps1           PASS/FAIL driver (exit 0 = PASS)
├── PROTOCOL-PARITY.md              VB bridge ↔ C++ port mapping
├── proof\                          committed evidence copies
└── EVIDENCE.md                     this file
```

Process model (mirrors the owner drawing): `NVIDIA Share.exe` loads
`NVIDIA Share.dll` and calls its `NvShareCefMain` export →
`CefExecuteProcess` handles subprocess relaunches of the same exe
(measured: renderer + gpu-process spawned with `--type=…` and exited
cleanly) → browser process: config → loopback OSC HTTP server →
`CefInitialize` → hidden-or-visible host window → browser → message
router (`cefQuery`/`cefQueryCancel`) → proof pipeline.

## 4. Owner slot (staged, verified)

`dist\nvoverlay-layout\NVIDIA ShadowPlay\NvOverlay\CEF\`
(= master architecture `NvOverlay\CEF\`):

```
NVIDIA Share.exe          (v3.41.3792.61, "NVIDIA Share (CEF OSC host)")
NVIDIA Share.dll          (v3.41.3792.61)
libcef.dll                (73.1.12+gee4b49f+chromium-73.0.3683.75)
chrome_elf.dll, d3dcompiler_43/47.dll, libEGL.dll, libGLESv2.dll
natives_blob.bin, snapshot_blob.bin, v8_context_snapshot.bin
cef.pak, cef_100/200_percent.pak, cef_extensions.pak, devtools_resources.pak
icudtl.dat
locales\                  (53 pak)
Resources\osc\            (the real OSC web UI bundle)
NVIDIA Share.json
CEF-RUNTIME-MANIFEST.txt  (name|size|sha256 of every staged file)
```

## 5. Proof-of-life (measured, reproducible)

Command: `powershell -File Overlay.Cef\run-proof-of-life.ps1`

```
process exit code: 0  (wall 3166 ms)
  [ok] PROC_START         0.8 ms   NvShareCefMain entered
  [ok] HTTP_SERVER_UP     8.3 ms   http://127.0.0.1:50382/ serving ...\Resources\osc
  [ok] CEF_INIT_OK      184.2 ms   CefInitialize returned true
  [ok] BROWSER_CREATED  241.4 ms   browser object created
  [ok] PAGE_LOADED     1819.1 ms   url=http://127.0.0.1:50382/index.html#/base httpStatus=200
  [ok] PROOF_ECHO_VERIFIED 2097.9 ms  QUERY_OSC_SET_EXPERIMENTAL round-trip returned "true"
  [ok] ORGANIC_QUERY    1785.7 ms  first page-originated command=QUERY_LOAD_STRING_TABLE
  [ok] DOM_PROBE        2086.5 ms  {"angular":true,"baseCount":0,"bodyChildren":4,"hasCefQuery":true}
PASS: process start -> CEF init -> page load -> cefQuery round-trip verified
```

Corroborating runtime evidence (from `cef-debug.log` / page console of the
same runs):

- Real OSC bundle boots: `"OSC Build Info: {oscPackageVersion: 3.28.0.412, branch: rel_03_28, buildType: prod}"`
- Native cefQuery round-trips organically: `crimson.cef/cefService "LOAD STRING TABLE"`,
  `"Request NodeInfo" → "Node info found"` (QUERY_LOAD_STRING_TABLE +
  QUERY_WIN_NODE_INFO delivered and answered)
- `DOM_PROBE.hasCefQuery = true` (the NATIVE router injected
  window.cefQuery — no polyfill involved)
- Backend-less degradation works as designed: REST/socket.io paths → 503
  `backend_unavailable_in_cef_host`, page services degrade without hang

Known limitations (documented, not hidden):

- `QUERY_BROWSE_DIRECTORY` answers the unwired-engine parity failure
  (`folder_picker_unavailable`); native picker is a later round.
- The menu-open visual attempt (`__PROOF_OPEN_UI` →
  `oscDisplayService.openOSC()`) did not produce an after-open DOM probe:
  the unlock shim's `nvCameraService` dependency does not resolve without
  backend state. `proof\osc-page-screenshot.png` therefore shows the
  booted page shell (dark backdrop), not the open menu.
- Backend REST/socket.io proxying stays engine-lane territory; the CEF
  host serves the OSC bundle only (503 otherwise).

## 6. Non-goals respected

- No WebView2 anywhere in the lane (only `Overlay.Engine` keeps its own).
- No `WebView\` / `WebViewHook\` folders created.
- No changes to NvCapture / nvspcap / NvBackend / reference NVIDIA
  binaries (A:\, Program Files, GFE copy — read-only sweeps).
- No reset/clean/revert of anything; no push.
