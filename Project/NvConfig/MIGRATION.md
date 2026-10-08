# Build Tree contract

`Project` is the source tree. `Scripts\build-dev.ps1` stages a non-destructive
runtime tree at `Build\NVIDIA ShadowPlay`; do not use `-Clean` for ordinary
builds because the output contains runtime-created settings and NvNode state.

## Runtime ownership

| Runtime | Build location | Source |
|---|---|---|
| NvContainer controller | `NvContainer\NvContainer.exe` | `Project\NvContainer\NvContainer` |
| NVIDIA Web Helper + `NvNode.exe` + NodeAPI + OSC frontend | `Overlay OSC\NVIDIA NodeAPI\` | `Project\Overlay OSC\NVIDIA NodeAPI` |
| NVIDIA OSC native host + CEF runtime | `Overlay OSC\NVIDIA OSC\` | `Project\Overlay OSC\NVIDIA OSC` and the CEF SDK |
| NvNode state and service configuration | `NvConfig\` | `Project\NvConfig` |
| Overlay enable/engine mode | `NvConfig\config.json` | Preserved if already present; seeded by the build script if absent |

The build script migrates an existing `Config\config.json` into
`NvConfig\config.json` when the new location is absent, then removes only the
old config file. The `Config\` directory remains available for capture-engine
settings; it is not the owner of the shared overlay switches.

NvContainer resolves the build root from its startup directory and loads
`NvConfig\nvcontainer.json` beneath that root. The service and OSC JSON paths
are relative to the same root. The OSC host independently walks from its own
executable location to the build root before loading its config and assets.

The managed Web Helper host is built from the Project host source and staged
beside the Project NodeAPI. `NvBackend` remains the separate API-hub owner; it
is not the Project NodeAPI runtime. This mirrors the Project layout and avoids
overwriting the pre-existing root `NvNode\` runtime. `NVIDIA OSC.exe` is
launched from `Overlay OSC\NVIDIA OSC` and serves the frontend staged under
`Overlay OSC\NVIDIA NodeAPI\osc`.

## Build prerequisites

Set `CEF_ROOT` to the CEF 73 SDK directory before building or staging. The
active Project tree always includes the native OSC host and shared CEF runtime;
there is no no-CEF solution-filter fallback. Visual Studio Build Tools with
the C++ workload and MSBuild are also required unless `-NoBuild` is used.

`Project\Launcher.Cef\deploy-launcher.ps1` is a compatibility entry point
that forwards to `Scripts\build-dev.ps1`, so both commands stage the same
complete Project-owned Build Tree. `-Dest` selects another output root,
`-NoBuild` stages existing Project outputs, and `-Clean` explicitly removes
the selected output root before staging.
