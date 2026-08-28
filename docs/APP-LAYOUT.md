# App Layout — root-fixed "NVIDIA ShadowPlay" tree (2026-08-28)

OWNER's product tree. The assembler is **`scripts/layout.proj`**, invoked by
`build-all.ps1 -StageLayout` (or `dotnet msbuild scripts/layout.proj`).
Classic builds (`dotnet build`, VS F5) are **completely untouched** — they
still produce self-contained `bin\` outputs (the dev bin is additionally
auto-completed into a runnable family by `_DevLayoutComplete` in
`Directory.Build.targets`).

**-StageLayout ends by APPLYING the staged tree onto
`Overlay\bin\Release\net10.0-windows10.0.26100.0\` (robocopy /E, never
purge)** — per OWNER, the dev bin IS the main tree: after staging, that one
folder runs the whole family (flat dev exes AND the staged
Application/Services/Engine/... structure coexist; `AppLayout.Dir` and
`ExePath` resolve correctly from both). `dist\NVIDIA ShadowPlay` remains
the clean deployment artifact the apply step copies from.

```
NVIDIA ShadowPlay\                    <- dist\NVIDIA ShadowPlay (default)
├── NVIDIA Experience.exe/.dll/.deps.json/.runtimeconfig.json   (root app)
├── Application\            NVIDIA API.exe / NVIDIA Capture.exe / NVIDIA Notifier.exe
│                           (thin native hosts; embedded app path = ..\Services\<app>.dll)
├── Services\               the three services' NVIDIA <app>.dll + .deps.json + .runtimeconfig.json
├── Overlay\                NVIDIA ShadowPlay.exe/.dll/.deps.json/.runtimeconfig.json
├── Engine\                 CaptureEngine.dll, .Encoder, .Encoder.Nvenc,
│                           .FFmpegBackend, .Recording, .Video, .Video.Ddagrab,
│                           .Audio.Wasapi
├── Core\                   System.*, Microsoft.*, SharpGen.Runtime*, WinRT.Runtime
├── Audio\                  NAudio.*
├── Graphics\               Vortice.*
├── Libraries\              Newtonsoft.Json.dll
├── FFmpeg\                 ffmpeg/ffplay/ffprobe + av*-62/-11/-60/-6/-9 dlls
├── Languages\              <locale>.json + current.txt
├── Config\                 config.json (runtime), engine.json, audio.json,
│                           notifier_obs.json, overlay-config-path.txt
├── Logs\                   *.log (api-crash, notifier_obs, ui-engine, encoder-detect, ...)
├── Data\NVIDIA_Shadowplay_Data\   on / privacy / highlights\ / Live\ / mic\ / Record\ / Replay\
├── Resources\              nvgcshare.ttf
├── Runtimes\               rid-specific assets (runtimes\win\lib\net8.0\...)
├── Redist\                 64bit.runtime.exe
└── Flags\                  Ready / Use_Overlay / Dev / Engine.UI / Audio.UI  (sentinels)
```

Note: `Overlay.Engine.dll` in the OWNER sketch is **reserved** for a future
overlay-engine split — no such assembly exists today (the Overlay app is a
single assembly, `NVIDIA ShadowPlay`).

## How .NET is told where the DLLs are

Three mechanisms, zero magic config files:

1. **Split hosts (Application\ → Services\).** `CreateAppHost` embeds the
   app-binary path `..\Services\<app>.dll` into the host exe (verified in
   `scripts/apphost_test`: the path is embedded verbatim, the icon is
   copied from the managed dll, and hostfxr resolves the relative path
   against the HOST's own directory and normalizes `..`). The host reads
   `<dll>.deps.json` + `<dll>.runtimeconfig.json` from Services\.
   `_StageSplitAppHost` in `Directory.Build.targets` stamps these during
   the layout build (Windows only).

2. **Family-folder assembly probing.** Every app links
   `Common/AppLayout.vb` (VB, zero deps). Its
   `AssemblyLoadContext.Default.Resolving` handler probes, under the
   layout root: `Engine\`, `Core\`, `Audio\`, `Graphics\`, `Libraries\`,
   `Runtimes\win\lib\net8.0\`, then the exe dir (dev fallback). The host
   tries deps.json probing first (Services\), fails, and our handler
   supplies the assembly — this is the standard .NET extension point.

3. **Root resolution.** `AppLayout.Dir` = exe dir, except
   `Application\*`/`Overlay\*` walk one level up (env override:
   `NVIDIA_SHADOWPLAY_APP_ROOT`). Every file path in app code now derives
   from `AppLayout.Dir` (see mapping table) so all five apps agree on the
   same root regardless of which process asks. `AppLayout.Initialize()`
   runs from the VB `MyApplication.Startup` event (`Common/AppLayoutStartup.vb`)
   — before any form exists — and also sets process CWD to the root.

Dev-mode behaviour: running from a classic `bin\` folder, none of the
family folders exist, the resolver finds nothing (deps are local there),
and `AppLayout.Dir` == exe dir == the old behaviour. **No behaviour
change for developers.**

## Path mapping (what the code rewrite changed)

| Concern | Old (startup-path era) | New (root-fixed) |
|---|---|---|
| Sentinel flags (`Ready`, `Use_Overlay`, `Dev`, `Engine.UI`, `Audio.UI`) | `<exedir>\` | `<root>\Flags\` |
| Config json (`config.json`, `engine.json`, `audio.json`, `notifier_obs.json`) | `<exedir>\` | `<root>\Config\` |
| Logs (`api-crash.log`, `notifier_obs.log`, `ui-engine.log`, `encoder-detect.log`, `capture-engine.log`, `Logs\`) | `<exedir>\` | `<root>\Logs\` |
| Languages | `<exedir>\Languages` | `<root>\Languages` |
| Data (`NVIDIA_Shadowplay_Data`) | `<exedir>\` | `<root>\Data\NVIDIA_Shadowplay_Data` |
| Font (`nvgcshare.ttf`) | `<exedir>\` | `<root>\Resources\` (legacy app-dir fallback kept) |
| FFmpeg binaries | `<exedir>\API-Core\` | `<root>\FFmpeg\` (legacy fallbacks kept as candidates) |
| Service launches (`NVIDIA API/Capture/Notifier.exe`) | `<exedir>\<app>.exe` | `<root>\Application\<app>.exe` |
| Overlay launch (`NVIDIA ShadowPlay.exe`) | `<exedir>\` | `<root>\Overlay\NVIDIA ShadowPlay.exe` |
| Overlay config read (Engine side, `OverlayConfig.vb`) | exe dir + repo walk | `<root>\Config\` first, legacy fallbacks kept |
| `avatar.png` | `<exedir>\` | `<root>\` |

All rewrites keep the legacy location as a fallback candidate wherever a
wrong guess would be user-visible, per the project's "measure, don't
assume" style.

## Files / projects changed for the layout

- **New** `Common/AppLayout.vb`, `Common/AppLayoutStartup.vb` — linked
  (`<Compile Include="..\Common\...">`) into all five app projects.
- **New** `scripts/layout.proj` — the staging orchestrator (build → copy →
  manifest; hard-fails on Windows if a required file is missing, warns on
  Linux where the Windows apphosts cannot exist).
- **New** `Directory.Build.props` — `EnableWindowsTargeting=true` (lets
  Linux compile the -windows apps; harmless on Windows).
- `Directory.Build.targets` — layout-only targets (`_StageRuntimeLibs`,
  `_StageSplitAppHost`), both gated on `StageLayout=true` (set only by
  layout.proj) + `AppLayoutRoot`.
- `scripts/build-all.ps1` — `-StageLayout` switch.
- Path-site rewrites across API / App Experience / Notifier / Engine /
  Overlay (~40 sites; test projects and `Load.old` untouched).
- Post-rewrite full-code audit (2026-08-28) caught 3 late CWD-relative
  sites in `App Experience/Main.vb` — the `Use_Overlay` toggle READ at
  form load (writer had moved to `Flags\`, reader had not → toggle reset
  to OFF and the next tick deleted the real flag), the kill-restart
  DELETE (missed the flag → overlay survived the kill), and
  `kill_error.log` (now `Logs\`). Library layer is deliberately
  layout-agnostic: `AppLayout.vb` is linked only into the 5 app
  projects; libraries take explicit base-dir parameters instead.
- Latent-bug fixes found by first-ever Linux compile of the legacy apps:
  `NVIDIA API.vb` BC30068 (`p` loop-variable collision), `AppLayout.vb`
  BC42104 (VB case-insensitive self-shadowing of `ExeDir`).

## OWNER verification protocol (Windows)

1. `git pull`
2. `powershell -ExecutionPolicy Bypass -File scripts\build-all.ps1 -StageLayout`
   → expect `LAYOUT STAGED OK -> ...\dist\NVIDIA ShadowPlay` followed by
   `STAGED TREE APPLIED OK -> ...\Overlay\bin\Release\net10.0-windows10.0.26100.0`
3. Inspect the bin folder against the tree above (the staged subfolders now
   live INSIDE it); confirm `Application\NVIDIA API.exe` exists (apphost
   stamping is Windows-only) and has the ShadowPlay icon.
4. Run `Overlay\bin\Release\net10.0-windows10.0.26100.0\NVIDIA Experience.exe`
   → click its API launcher; then run `...\Application\NVIDIA API.exe`
   directly — the tray hub must come up (this proves the `..\Services\`
   host mechanism end-to-end).
5. Start a recording; confirm `Config\config.json` is picked up and
   `Logs\` receives `encoder-detect.log` / `ui-engine.log`.

If step 4's host ever fails to start (`The application to execute does not
exist` naming the wrong path), the fallback is to keep the apps' standard
hosts inside Services\ and turn Application\ entries into shortcuts — but
the mechanism is verified in `scripts/apphost_test` and should not be
needed.

## Runtime resolution summary (one line per mechanism)

- host: `Application\X.exe` → embedded `..\Services\X.dll` (+ deps/runtimeconfig from Services\)
- deps miss → `AssemblyLoadContext.Default.Resolving` → `Engine\Core\Audio\Graphics\Libraries\Runtimes` under root
- every file path → `AppLayout.Dir` (root), family subfolders per the table above
