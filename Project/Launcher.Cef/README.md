# Launcher.Cef — CEF launcher lane (root Launcher.exe + NvOverlay\Cef\Launcher.dll)

UI style (owner call 2026-09-25 "ขอแนว Web Github"): mirrors the
github.io site design language — sharp 90° corners everywhere, corner
brackets + runway + sheen on the SERVICES banner (OBT3-banner grammar),
GeForce-Bold-Alt display font + Inter, animated gradient subtitle,
feature-card hover grammar, green-on-black palette from
`Web Github\CSS\index\style.css` (#76B900 / #0A0A0A / border white 10%).

The NEW Launcher replaces the WinForm `Launcher.exe` (VB,
`Project\Launcher.exe\`) with a native CEF host and a GFE-styled web UI.
Layout follows the owner's "ลงที่เดียวกับ osc" call:

```
<NVIDIA ShadowPlay>\                     (product root)
├── Launcher.exe                         ← thin bootstrap (root slot)
└── NvOverlay\Cef\
    ├── Launcher.dll                     ← host body (THIS lane's build)
    ├── NVIDIA Share.exe / .dll          ← osc lane (unchanged)
    ├── libcef.dll + cef.pak + locales\  ← ONE CEF runtime, shared
    └── Resources\launcher\              ← the launcher web UI bundle
```

The bootstrap resolves `NvOverlay\Cef\Launcher.dll` by ABSOLUTE path (CWD
independent), `SetDllDirectoryW`'s the shared CEF slot so the body's
libcef.dll import resolves there, and calls the `NvLauncherCefMain`
export — which implements the whole CEF process split (subprocess
relaunches of the root Launcher.exe re-enter the same export). The pinned
CEF 73 runtime of the osc lane is reused; no second copy on disk.

## Build + stage

```
powershell -File Project\Launcher.Cef\deploy-launcher.ps1            # build + stage
powershell -File Project\Launcher.Cef\deploy-launcher.ps1 -NoBuild   # stage bin only
powershell -File Project\Launcher.Cef\deploy-launcher.ps1 -Dest <path>
```

MSBuild path + CEF SDK default to the repo convention
(`C:\Visual Studio\MSBuild`, `C:\My Project\cef-sdk\cef73`).

## UI ↔ host contract

Page origin: loopback HTTP (ephemeral port) serving `Resources\launcher`
(production model parity: never file://). RPC = the osc page contract
`window.cefQuery` / `window.cefQueryCancel`, command namespace
`LAUNCHER_*` (no overlap with the osc `QUERY_*` namespace):

| command | payload | effect |
|---|---|---|
| `LAUNCHER_GET_STATE` | — | state snapshot JSON |
| `LAUNCHER_SET_OVERLAY` | `value:bool` | config `Overlay.UseOverlayEnabled` (user toggle only) |
| `LAUNCHER_SET_ENGINE_OVERLAY` | `value:bool` | **Overlay Mode (WINFORM ⟷ CEF)** — config `Overlay.EngineOverlayMode`; CEF=true brings the chain up, CEF=false ALSO stops `NvOverlay\Cef\NVIDIA Share.exe` (path-deduped) so the switch is real |
| `LAUNCHER_OPEN_OVERLAY` | — | hub frame `[Send] NVIDIA  APP\|open_overlay` → :5001 |
| `LAUNCHER_OPEN_OBT3` | — | opens the OBT3 page (legacy banner link) |
| `LAUNCHER_DRAG` | — | borderless-window drag (WM_NCLBUTTONDOWN/HTCAPTION) |
| `LAUNCHER_MINIMIZE` | — | SW_MINIMIZE |
| `LAUNCHER_CLOSE` | — | close the window; processes keep running |
| `LAUNCHER_EXIT_ALL` | — | `UseOverlayEnabled=false` + kill family + close (old "Installer Mode") |

State pushes: the supervisor polls every 1s (Main.vb `IF_APP` parity) and
pushes through the augmentation dispatcher
`window.__LauncherState(<json>)` (`launcher_script.h`); the page also
pulls `LAUNCHER_GET_STATE` every 2s as a fallback.

## Supervision contract (port of Main.vb)

- Base chain ALWAYS: `NvContainer\NvContainer.exe` + root
  `NVIDIA Backend.exe` started if missing (owner call 2026-09-25).
- ENGINE OVERLAY chain (toggle ON, or config ON at startup):
  NvContainer → `NvBackend\NVIDIA Web Helper.exe` → wait :59001 (≤10s) →
  `NvOverlay\Cef\NVIDIA Share.exe --backend-port 59001`, deduped by exe
  PATH.
- 1s status: NVIDIA Backend / NVIDIA ShadowPlay (+`Flags\Ready` →
  OVERLAY API "LOADING") / NVIDIA Notifier; stale `Flags\Ready` removed
  when the notifier lane is down.
- Config writes are read-modify-write on the CURRENT file
  (`launcher_json.h` order-preserving serializer; atomic tmp + `.bak`
  swap) — the AppConfigShared.vb contract.
- EXIT ALL kill list: Notifier, ShadowPlay, nvsphelper64, NvContainer,
  NVIDIA Backend, NVIDIA Capture.

## Switches (smoke/proof runs)

`--no-supervise` (no process starts) · `--self-exit-ms=N` (watchdog) ·
`--hidden` · `--width=W --height=H` · `--ui-root=<dir>` · `--url=<url>`.
Log: `<root>\Logs\launcher-cef.log` (+ CEF debug log beside it).

## Smoke evidence (2026-09-25)

- 3× Launcher.exe (browser + subprocesses), page `httpStatus=200`,
  `BRIDGE LIVE`, state dots rendered, watchdog close clean.
- LIVE owner interaction: ENGINE OVERLAY ON → full chain start
  (NvContainer, Web Helper, NVIDIA Share.exe --backend-port 59001);
  toggle writes correct; EXIT ALL killed the family and closed cleanly.
- Pixel check of the captured window: titlebar/footer `#1A1B1D`, cards
  `#1E2023`, NVIDIA eye `#76B900` — theme renders as designed.
- Family-name parity fixes proven in the supervised run: the hub starts
  as `NvBackend\NvBackend.exe` (staged name; legacy "NVIDIA Backend"
  kept as candidate + status alias + kill-list entry), Toolhelp32
  process matching accepts the `.exe` suffix (`adopt running: NvContainer`
  — no duplicate/startup loop).

## CEF 73 CSS constraints (learned the hard way)

The pinned CEF 73 = Chromium 73 (2019). The page must avoid:
- **flexbox `gap`** (Chrome 84+) — every flex row spaces children with
  `> * + * { margin-left: ... }` instead; grid `gap` is fine (66+).
- **`inset` shorthand** (Chrome 87+) — use top/right/bottom/left.
- `clamp()/min()/max()`, `:is()/:where()`, `aspect-ratio` — not used.
Also: JS must not overwrite className strings that drifted from the
stylesheet (the lane chips lost `.lane-chip` when render() still wrote
`.meta-chip` — the run-together "ENGINE LANECEF LANEAPI HUB" bug).

## Known notes

- EXIT ALL (owner-pressed during live testing) rewrote
  `NvConfig\config.json` `Overlay.UseOverlayEnabled=false` — the
  documented RadioButton2 contract. Re-enable via the OVERLAY ENABLED
  toggle.
- The WinForm launcher project (`Project\Launcher.exe\`) is untouched —
  rollback = restage its bin\ output to the root (or run
  `Scripts\build-dev.ps1` with the Launcher.Cef bin removed).
