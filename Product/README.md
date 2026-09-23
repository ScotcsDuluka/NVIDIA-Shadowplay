# Product — the 4-instance Share tree (GFE coordinator pattern)

The real GFE host runs four `NVIDIA Share.exe` processes plus the Web
Helper service; this tree mirrors that shape with clearly-owned slots.
The Coordinator is the ONLY entry the autostart touches — everything
else is its children. Authoritative mapping: `docs/GFE-PROCESS-MAP.md`.

## Process model

```
HKCU Run "NvPortableShare"
      │
      ▼
Share.exe #1  Coordinator            (Product/Coordinator)
      │  spawns + monitors, passes --parent-pid to each child
      ├──────────────┬──────────────┬──────────────────┐
      ▼              ▼              ▼                  ▼
Share.exe #2   Share.exe #3   Share.exe #4      NVIDIA Notifier.exe
WinForm\       WebView\       Hook\             Notifier\
(Overlay       (Overlay.Engine (placeholder      (tray balloons,
 WinForms      --desktop,       hook slot —      polls :59001
 build)        WebView2 osc)    port :59004)     /Record/*)
```

## Build-source → slot mapping (what install.ps1 copies)

| Slot folder | Build source | Exe name at runtime |
|---|---|---|
| `Coordinator\` | `Product/Coordinator` (AssemblyName `Share`) | `Share.exe` |
| `WinForm\` | `Overlay` (AssemblyName `NVIDIA Share` — pre-rename builds land as `NVIDIA ShadowPlay.exe`, the Coordinator has a fallback for it) | `NVIDIA Share.exe` |
| `WebView\` | `Overlay.Engine` (AssemblyName `NVIDIA Share`), spawned with `--desktop` | `NVIDIA Share.exe` |
| `Hook\` | `Product/Hook` (AssemblyName `Share`) | `Share.exe` |
| `Notifier\` | `Product/Notifier` (AssemblyName `NVIDIA Notifier`) | `NVIDIA Notifier.exe` |

All four Share processes are the SAME exe name distinguished by folder —
exactly how the real host runs multiple `NVIDIA Share.exe` instances.

## Slot contracts

- **#1 Coordinator** — spawn once → child gets `--parent-pid <pid>`;
  children self-exit when it dies (no orphans). Respawn with exponential
  backoff 3s → 60s, reset after a stable 30s run (same constants as the
  proven `EngineProcessSupervisor`). Adopts already-running children
  instead of double-spawning — matched by **exe path**, not process name
  (all Share instances share a name). Child paths overridable via env
  (`SHARE_WINFORM_PATH` / `SHARE_WEBVIEW_PATH` / `SHARE_HOOK_PATH` /
  `SHARE_NOTIFIER_PATH`); health-polls backend :59001 + hook :59004.
- **#2 WinForm** — the WinForms overlay system (forms UI, hotkey
  ownership, engine supervisor). The legacy Overlay project verbatim;
  its `--parent-pid` flag is honored on startup.
- **#3 WebView** — `Overlay.Engine` in `--desktop` mode: WebView2 hosts
  the osc page (same-origin via OscControllerServer), transparent
  per-screen windows. The `--desktop` + `--parent-pid` contract is in
  `Overlay.Engine/Program.vb`.
- **#4 Hook** — placeholder BY DESIGN: tree shape, process naming and
  supervision are final NOW; the hook payload lands later without
  touching the Coordinator, autostart or install.ps1. Show port today:
  `GET :59004/hook/status` · `/hook/inject` (501 reserved) · `/hook/exit`.
- **Notifier** — own component (legacy stack ran it off the TCP :5000
  hub; here it polls the NEW API). Enable/Running edges → tray balloons;
  silent when the backend is down.

## Rename note (WinForm slot)

The GFE rule is "every Share instance is NVIDIA Share.exe". The legacy
WinForms project still ships AssemblyName `NVIDIA ShadowPlay` — rename
it to `NVIDIA Share` in `Overlay/NVIDIA Overlay.vbproj` on the build
machine; until then the Coordinator's pre-rename fallback keeps the
tree fully functional (`WinForm\NVIDIA ShadowPlay.exe` is adopted fine).
