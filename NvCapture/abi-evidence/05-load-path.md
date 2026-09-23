# 05 — Load / call path (nvsphelper64 → nvspcap and friends)

Question 5 of the task card. What is **proven** vs **hypothesis**, per hop.

## 5.1 Who references nvspcap by name [P]

String scan (bare filename references, min length 10) over the driver
package `DriverStore\FileRepository\nv_dispi.inf_amd64_*`:

| module | references |
|---|---|
| `nvd3dumx.dll` (D3D10/11 UMD, x64) | `nvspcap64.dll`, `nvspcaps.exe`, `nvspcaps64.exe`, `QueryShadowPlayInterface`, `QueryShadowPlayInterfaceVersionRange`, `QueryShadowPlayDdiShimStatus`, `QueryShadowPlayDdiShimInterface` |
| `nvd3dum.dll` (x86 UMD) | `nvspcap.dll`, `nvspcaps.exe` |
| `nvoglv64.dll` (OpenGL UMD) | `nvspcap64.dll`, `QueryShadowPlayDdiShimStatus`, `QueryShadowPlayDdiShimInterface` |
| `nvoglv32.dll` | `nvspcap.dll` |

**[P]** The **graphics user-mode drivers** are the components that know
`nvspcap(64).dll` and the `Query*` entry names (GetProcAddress-style name
strings). The DDI shim interface of doc 02 is consumed *by the UMD inside
the game/app process*.

**[O]** Negative evidence: `nvapi64.dll`, `nvwgf2umx.dll`, `nvlddmkm.sys`
contain **no** `nvspcap`/`DdiShim` strings; `nvsphelper64.exe` contains
**no** `nvspcap` string either (it only carries the ShadowPlay registry keys
`ShadowPlayCfg2/3` and imports `LoadLibraryExW`). `_nvspcaps64.dll` (the
ShadowPlay2 *server*) and `nvspapi64.dll` likewise show no name reference →
the service side reaches the shim via **ordinal** if at all (doc 03.1).

## 5.2 Which copy gets loaded [O]

`C:\Windows\System32\nvspcap64.dll` exists on this machine (driver-shipped;
mtime 2026-08-31, driver 32.0.15.8266) and exports the **same 3 names with
the same ordinals** as the GFE-folder copy, at different RVAs — i.e. the
contract, not the binary, is the stable thing. The UMDs load the shim by
bare name → loader search order resolves `System32` first; the GFE copy in
`...\ShadowPlay\` is the same contract deployed alongside GFE.
**[H]** which one wins in a given install (depends on how the UMD
constructs the path / DLL search path of the host process).

## 5.3 Process topology [O + H]

* Shim functions run **inside the app/game process** (that is where UMDs
  live) — consistent with nvspcap64.dll's MMF/log-string surface
  (`CaptureCore.log` under `%ProgramData%\NVIDIA Corporation\ShadowPlay\`,
  `CShadowPlayUserInputRedirections` window-message hooks). **[O]**
* `nvsphelper64.exe` is the per-user helper (registry config reader); the
  *service* half is `_nvspcaps64.dll` inside NVDisplay.Container (PDB path
  `shadowplay2\server\...`). **[O]**
* **[H]** the helper/service's exact role in the shim's in-process life
  (config push via the `CSPShareStateManager` MMFs, hotkey MMF) — the MMF
  plumbing is observed, the hand-off choreography is not, and our v1 does
  not participate in it (we expose no MMFs; see doc 06).

## 5.4 The proven call graph (v1 scope)

```
game process
  ├─ nvd3dumx.dll / nvoglv64.dll  (already loaded by D3D/OpenGL)
  │    └─ LoadLibrary-ish: "nvspcap64.dll"            [P: name string]
  │    └─ GetProcAddress("QueryShadowPlayDdiShimStatus")   [P: name string]
  │    └─ GetProcAddress("QueryShadowPlayDdiShimInterface")[P: name string]
  │         └─ VER_1..6 table  → Ddi* calls           [P: slot map, doc 02]
  │              └─ CreateSession → CShadowPlayProxyShim instance  [P]
  └─ (optional, ordinal 3) CreateShadowPlayProxyShimInterface  [O: no name refs]
       └─ proxy object → Shim* calls, Register/UnregisterCallback  [P: doc 03]
```

Our compatibility surface only has to satisfy the three proven entry points;
everything else in the reference binary is internal to it.
