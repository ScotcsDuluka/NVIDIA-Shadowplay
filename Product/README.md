# Product — the 3-process Share tree (GFE coordinator pattern)

The real GFE host runs several `NVIDIA Share` processes; this tree mirrors
that shape with three clearly-owned slots. The Coordinator is the ONLY
entry the autostart touches — everything else is its children.

## Process model

```
HKCU Run "NvPortableShare"
      │
      ▼
Share.exe #1  Coordinator        (this folder — Product/Coordinator)
      │  spawns + monitors, passes --parent-pid to each child
      ├──────────────────────────────────────┐
      ▼                                      ▼
Share.exe #2  Desktop                Share.exe #3  Hook
(Overlay.Engine — existing build,    (Product/Hook — placeholder slot:
 AssemblyName "NVIDIA Share",         idle process reserved for the
 --desktop mode)                      future nvspcap-style hook payload)
```

## Product tree layout (what install.ps1 builds, Phase 4)

```
C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\
├── osc\                          ← frontend (served by the Backend)
├── Backend\                      ← Backend/ (node, port 59001)
├── Coordinator\Share.exe         ← #1 (HKCU Run → this)
├── Desktop\NVIDIA Share.exe      ← #2 (Overlay.Engine build output, verbatim)
└── Hook\Share.exe                ← #3
```

- **#2 is NOT renamed in its project** — its AssemblyName already IS
  `NVIDIA Share` (the plan's "rename entry" step is satisfied by the
  AssemblyName; install.ps1 just copies the build output verbatim into
  `Desktop\`). Its `--desktop` + `--parent-pid` flags are the documented
  supervisor contract in `Overlay.Engine/Program.vb`.
- **#3 is a placeholder by design**: the tree shape, process naming and
  supervision contract are final NOW; the hook payload lands later
  without touching the Coordinator, the autostart entry or install.ps1.

## Supervisor contract (Coordinator/Program.vb)

- spawn once → child gets `--parent-pid <coordinator pid>`; children
  self-exit when it dies (no orphans)
- respawn on death with exponential backoff 3s → 60s, reset after a
  stable 30s run (same constants as the proven `EngineProcessSupervisor`)
- adopt already-running children instead of double-spawning — matched by
  **exe path**, not process name (all three are named Share)
- backend health (`/Backend/v.1.0/health` on :59001) checked every cycle,
  status changes logged — informational only, the backend owns its own
  lifecycle

## Overrides (dev/testing)

| Env / arg | Meaning |
|---|---|
| `SHARE_DESKTOP_PATH` / `--desktop-path` | where #2 lives (default `..\Desktop\NVIDIA Share.exe`) |
| `SHARE_HOOK_PATH` / `--hook-path` | where #3 lives (default `..\Hook\Share.exe`) |
| `--backend-url` | health URL (default `http://127.0.0.1:59001/Backend/v.1.0/health`) |
| `--parent-pid` | exit when that pid dies (Coordinator and Hook both honor it) |

Logs: `<slot>\logs\coordinator.log`, `<slot>\logs\hook.log`,
fallback `%TEMP%\NVIDIA-Share-*.log`.

## Build

```
dotnet build "Product/Coordinator/Coordinator.vbproj" -c Release
dotnet build "Product/Hook/Hook.vbproj" -c Release
# #2: dotnet build "Overlay.Engine/NVIDIA Overlay Engine.vbproj" -c Release
```
