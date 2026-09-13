# 12 — REAL SYSTEM MAP (installed GFE 3.28.0.412)

Everything below was read from an ACTUAL installation on this machine
(GFE 3.28.0.412, installed 2026-09-13). FACT unless marked otherwise.

## Components (install root `C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\`)

| Piece | Path | Role |
|---|---|---|
| osc web app | `osc\` — **66 files, byte-identical to `Overlay\osc`** (diff 0) | the overlay UI (confirms our copy IS the real production osc) |
| osc host | `NVIDIA Share.exe` (×3 processes when running) + `cef\` | CEF host; loads `osc/index.html` |
| host launch config | `NVIDIA Share.json` | switches: `nv-osc=true`, `nv-url-relative=osc/index.html`, `nv-node-app=NvNode\nvnodejslauncher.exe`, `nv-node-data=NvNode\nodejs.json`, `nv-plugin-folder-relative=./cef/share;./cef/common` |
| Node controller | `NvNode\nvnodejslauncher.exe` + `NvNode\nodejs.json` (launcher runs a bundled node; no standalone node.exe in PF) | the local REST+socket controller the osc page talks to |
| controller listener | `NVIDIA Web Helper.exe` (service) — LISTENING 127.0.0.1:59588 | local API surface (port is dynamic; page learns it via `QUERY_WIN_NODE_INFO` → {port, secret}) |
| PIPL config | `C:\ProgramData\NVIDIA Corporation\NvNode\piplConfig.json` | **runtime endpoint configuration — see Duluka section** |
| main app UI | `www\` (Roboto fonts, assets, app.js/vendor.js — the DESKTOP app, NOT osc) | separate surface, not needed by the overlay |

## PIPL config = the account/telemetry injection point (KEY FINDING)

`piplConfig.json` (live file, ProgramData) carries the EXACT structure the
osc page merges at runtime via socket channel `/PiplConfig/v.1.0/update`
(`PiplConfigService` → merges into OSC_CONFIG):

```json
{ "data": { "daysToExpire": 1, "isConnectEnabled": true,
  "configData": {
    "jarvis":   { "server": "https://accounts.nvgs.nvidia.com" },
    "gfwsl":    { "server": "https://gfwsl.geforce.com/" },
    "aem":      { "server": "https://www.nvidia.com/" },
    "vrs":      { "server": "https://www.nvidia.com" },
    "jsEvents": { "server": "https://events.gfe.nvidia.com" },
    "nvTelemetry": { "eventsServer": "...", "feedbackServer": "...", "feedbackAttachmentServer": "..." },
    "redirect": { "server": "..." } } },
  "expiryTime": 1789373819161 }
```

Page-side consumption (app.js): `PIPL_CONFIG_UPDATED` →
`jsEvents.server`, `gfwsl.server`, `jarvis.server` are swapped at runtime.

⇒ **Duluka Account integration = serve a piplConfig with
`jarvis.server` pointing at the Duluka auth server and telemetry keys
empty.** No osc UI change required; the host (our OscControllerServer)
already pushes `/PiplConfig/v.1.0/update` (currently `{}`).

## Confirmed equivalences with our Overlay.Engine

| Real system | Our implementation | Parity |
|---|---|---|
| NVIDIA Share.exe CEF host loads osc/index.html | OscHostForm + WebView2 loads same page via localhost | ✓ (same UI bytes) |
| Node controller: REST + socket.io (EIO3) | OscControllerServer (REST + engine.io v3/socket.io v2, golden-verified) | ✓ protocol |
| `QUERY_WIN_NODE_INFO` {port,secret} handshake | CefQueryBridge → controller port/secret | ✓ |
| piplConfig push | OscControllerServer `/PiplConfig/v.1.0/update` (now `{}`; will carry Duluka config) | ✓ channel |

## Differences that remain (FACT)

- Real host renders osc via CEF **off-screen + driver compositing** (in-game
  overlay); ours renders via WebView2 desktop window (borderless-game/
  desktop coverage). Exclusive-fullscreen in-game display stays
  NVIDIA-driver-only.
- Real account features (login, uploads, broadcasts) call
  `jarvis`/`gfwsl` cloud endpoints — to be replaced by Duluka Account
  (see 13-duluka-account.md).
