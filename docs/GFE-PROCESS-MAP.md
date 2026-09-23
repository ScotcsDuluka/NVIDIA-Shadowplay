# GFE Process Map — NVIDIA Web Helper system, our structure

The authoritative mapping between the REAL NVIDIA GeForce Experience
process stack and our rebuild. One row per process, one owner per slot.
This is the contract the product tree, the installer and the Coordinator
all follow (updated 2026-09-23, supersedes the 3-process model in
git history).

## The five-family model

The old stack had five apps (API / Launcher / Notifier / Overlay / Engine)
talking private TCP (:5000 hub, :5001 engine channel). The GFE-shaped
rebuild keeps all five ROLES but gives them GFE identities:

| Role (ours) | GFE identity (real host) | Our component | Product tree slot | Wire | Status |
|---|---|---|---|---|---|
| API | `NVIDIA Web Helper.exe` (NvNode service) | `Backend/` — Node.js REST+socket.io | `Backend\` | loopback **:59001** (registry `Global\NvNode`) | **LIVE** — deployed step 8, Phase 1B golden 77/77 |
| Engine | `nvsphelper64.exe` (ShadowPlay helper / capture) | `Engine/` — AssemblyName `nvsphelper64` | `Engine\` | REST client → :59001 (`/Record/Enable`, `/Record/Running`, `/RecordPaths`) | REST client + docs done; capture wiring = CaptureEngine branches |
| Overlay #1 | `NVIDIA Share.exe` (controller instance) | `Product/Coordinator` — AssemblyName `NVIDIA Share` | `Coordinator\NVIDIA Share.exe` | spawns #2 #3 #4 + Notifier; health-polls :59001 + :59004 | **this commit** |
| Overlay #2 | `NVIDIA Share.exe` (WinForms overlay) | `Overlay/` — AssemblyName `NVIDIA Share` (was `NVIDIA ShadowPlay`) | `WinForm\NVIDIA Share.exe` | WinForms UI, own hotkey service, supervises engine | renamed this commit |
| Overlay #3 | `NVIDIA Share.exe` (WebView desktop overlay) | `Overlay.Engine` run `--desktop` — AssemblyName `NVIDIA Share` | `WebView\NVIDIA Share.exe` | WebView2 hosts osc, same-origin via OscControllerServer | adopted (was the "Desktop" slot) |
| Overlay #4 | `NVIDIA Share.exe` (WebView hook / inject) | `Product/Hook` — AssemblyName `NVIDIA Share` | `Hook\NVIDIA Share.exe` | **show port 127.0.0.1:59004** (`/hook/status`, `/hook/inject`, `/hook/exit`) | placeholder — payload pending, port live "for show" |
| Notifier | (GFE: notifications come from Share) | `Product/Notifier` — AssemblyName `NVIDIA Notifier` | `Notifier\NVIDIA Notifier.exe` | polls :59001 `/Record/Running` + `/Record/Enable` → tray balloons | **this commit** |
| Launcher | (GFE: service + Run keys) | `deploy/install.ps1` + `deploy/hotkey-listener.ps1` + HKCU Run | — | autostart chain | Phase 4, updated for the 4-instance tree |

## Share instance contract (the "NVIDIA Share.exe 1..4" rule)

All four Share processes are the SAME exe name, distinguished by folder
and arguments — exactly how the real host runs multiple `NVIDIA Share.exe`
instances:

1. `Coordinator\NVIDIA Share.exe` — instance 1, the controller. Spawns and
   supervises 2/3/4 (+ the Notifier support child), adopt-by-path, respawn
   backoff 3s→60s, exits when its own parent dies.
2. `WinForm\NVIDIA Share.exe` — instance 2, the WinForm overlay system
   (Forms UI, hotkey ownership, engine supervisor).
3. `WebView\NVIDIA Share.exe --desktop` — instance 3, the normal WebView
   desktop overlay (WebView2 osc host; transparent per-screen window).
4. `Hook\NVIDIA Share.exe` — instance 4, the WebView hook / inject slot.
   Payload NOT implemented yet; the slot exists and opens the show port
   so the surface is visible and testable today:

```
GET http://127.0.0.1:59004/              -> text banner (instance 4/4)
GET http://127.0.0.1:59004/hook/status   -> JSON state (mode, pid, uptime, inject status)
GET http://127.0.0.1:59004/hook/inject   -> 501 JSON (not implemented — reserved)
GET http://127.0.0.1:59004/hook/exit     -> graceful exit (operator handle)
```

## Wire map (ports)

| Port | Owner | Purpose |
|---|---|---|
| 59001 | Backend (`NVIDIA Web Helper` role) | REST + static osc + socket.io EIO=3 |
| 59004 | Share #4 Hook | show port (status/inject/exit) |
| 5115 | Duluka.Server (PC) | jarvis providers backend (separate system) |
| 5000 / 5001 | LEGACY API hub / engine TCP | superseded — do not use in new components |

## Legacy stack (superseded, kept for reference)

`API/` (NVIDIA API.exe hub :5000), `Launcher/` (Launcher.exe), `Notifier/`
(NVIDIA Notifier.exe, TCP-driven), the Overlay's private TCP client — the
forensic audit (FORENSIC-AUDIT-REPORT-2026-09-06.md) documents that graph.
New components must NOT depend on the legacy hub; the REST :59001 surface
is the only shared channel going forward.

## Autostart chain (install.ps1)

```
HKCU Run "NvPortableBackend"  -> node.exe Backend\index.js          (API first)
HKCU Run "NvPortableShare"    -> Coordinator\NVIDIA Share.exe       (spawns 2/3/4 + Notifier)
HKCU Run "NvPortableHotkey"   -> hotkey-listener.ps1                (Alt+Z wire-through)
```

Rollback: `C:\ProgramData\NVIDIA-Shadowplay-Portable\UNINSTALL.cmd`.
