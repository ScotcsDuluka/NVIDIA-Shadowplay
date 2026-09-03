# App Layout — root-fixed "NVIDIA ShadowPlay" tree (2026-08-28 rev 2)

OWNER's product tree. Two producers, ONE set of rules:

- **Plain build (Build ปกติ)** — `dotnet build` / VS F5 / build-all.ps1.
  `_ProductTreeBin` in `Directory.Build.targets` (runs inside the
  NVIDIA ShadowPlay build, which ProjectReferences all four other apps and
  therefore finishes LAST) sweeps `Overlay\bin\<cfg>\<tfm>\` into this tree
  automatically. A lone Experience build (VS startup project) keeps its own
  classic flat bin — the product tree lives in Overlay's bin.
- **`scripts/layout.proj`** (invoked by `build-all.ps1 -StageLayout`) —
  wipes and rebuilds `dist\NVIDIA ShadowPlay` from the project outputs,
  manifest-verifies it, then the script MIRRORS it onto the dev bin
  (robocopy /MIR) for a guaranteed deep clean of accumulated junk.

**-StageLayout's mirror EXCLUDES the four runtime-writable dirs and
preserves them: **Config\, Data\, Logs\, Flags\** — user/app settings
(config.json, engine.json, audio.json), data state, diagnostics and
session sentinels survive every stage. They are seeded by the plain
build's `_DevLayoutComplete`, so a fresh bin still gets their staged
defaults.

Per OWNER, the dev bin IS the main tree: after any build that folder is
the clean product tree — `Launcher.exe` at the root,
`NVIDIA ShadowPlay.exe` in Overlay\, hosts in Application\, family
folders (Engine\Core\Audio\Graphics\Libraries\Runtimes) for the
resolver. `dist\NVIDIA ShadowPlay` remains the clean deployment
artifact the mirror copies from.

```
NVIDIA ShadowPlay\                    <- dist\NVIDIA ShadowPlay (default)
├── Launcher.exe/.dll/.runtimeconfig.json              (root app)
├── .NET Deployment\        EVERY app's .deps.json + .runtimeconfig.json
│                           (10 files: Experience, ShadowPlay, API,
│                           Capture, Notifier × 2 — OWNER tree rev 2;
│                           replaces the ConfigApp\ experiment)
├── Application\            NVIDIA API.exe / NVIDIA Capture.exe / NVIDIA Notifier.exe
│                           (thin native hosts; embedded app path = ..\Services\<app>.dll)
├── Services\               the three services' NVIDIA <app>.dll + .runtimeconfig.json
│                           (deps.json moved to .NET Deployment\)
├── Overlay\                NVIDIA ShadowPlay.exe/.dll/.ico/.runtimeconfig.json
│                           (deps.json moved to .NET Deployment\)
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
├── Runtimes\               rid-specific assets (runtimes\win\lib\net10.0\...)
├── Redist\                 64bit.runtime.exe
└── Flags\                  Ready / Use_Overlay / Dev / Engine.UI / Audio.UI  (sentinels)
```

Note: `Overlay.Engine.dll` in the OWNER sketch is **reserved** for a future
overlay-engine split — no such assembly exists today (the Overlay app is a
single assembly, `NVIDIA ShadowPlay`).

## .NET Deployment\ vs the files that MUST stay beside the dlls

Verified by experiment (scripts/lab-netreloc.sh, Linux hostfxr — the same
hostpolicy code the Windows apphosts run):

- **runtimeconfig.json is NOT relocatable.** hostfxr hard-requires
  `<app>.runtimeconfig.json` beside the app dll BEFORE any managed code
  runs; missing = `Failed to run as a self-contained app` — fatal, and no
  in-app handler can intercept it. Therefore root\, Overlay\ and
  Services\ KEEP their five runtimeconfig.json files, AND .NET Deployment\
  carries a copy of each so the folder is the complete deployment picture
  (10 files). These are build-generated files — never hand-edited.
- **deps.json IS relocatable.** Without it hostpolicy falls back to
  app-dir probing, and AppLayout's `AssemblyLoadContext.Default.Resolving`
  handler (installed at `MyApplication.Startup`, before any form code)
  supplies every cross-folder dependency. All five deps.json therefore
  live ONLY in .NET Deployment\.

## How .NET is told where the DLLs are

Three mechanisms, zero magic config files:

1. **Split hosts (Application\ → Services\).** `CreateAppHost` embeds the
   app-binary path `..\Services\<app>.dll` into the host exe (verified in
   `scripts/apphost_test`: the path is embedded verbatim, the icon is
   copied from the managed dll, and hostfxr resolves the relative path
   against the HOST's own directory and normalizes `..`). The host reads
   `<dll>.runtimeconfig.json` from Services\ (loader hard requirement);
   there is no `<dll>.deps.json` beside it any more — see the section
   above. `_StageSplitAppHost` stamps these during the layout build, and
   `_ProductTreeBin` re-stamps them into bin\Application\ after every
   plain build (Windows only).

2. **Family-folder assembly probing.** Every app links
   `Common/AppLayout.vb` (VB, zero deps). Its
   `AssemblyLoadContext.Default.Resolving` handler probes, under the
   layout root: `Engine\`, `Core\`, `Audio\`, `Graphics\`, `Libraries\`,
   `Services\` (the Overlay app calls the NVIDIA API types in-process),
   `Runtimes\win\lib\net10.0\`, then the exe dir (dev fallback). Without
   deps.json the host's own probing stops at the app dir; our handler
   supplies every cross-folder assembly — this is the standard .NET
   extension point.

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
- Path-site rewrites across API / Launcher (Launcher.exe) / Notifier / Engine /
  Overlay (~40 sites; test projects and `Load.old` untouched).
- Post-rewrite full-code audit (2026-08-28) caught 3 late CWD-relative
  sites in `Launcher/Main.vb` — the `Use_Overlay` toggle READ at
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
2. **Build ปกติ** — `dotnet build "Overlay\NVIDIA Overlay.sln" -c Release`
   (or just build/F5 in VS): when it finishes,
   `Overlay\bin\Release\net10.0-windows10.0.26100.0\` IS the product tree
   — the sweep runs inside the NVIDIA ShadowPlay build (the last project).
   Expect the log line `product tree staged in bin -> ...`.
3. For a guaranteed deep clean (purges anything not in the tree), run
   `powershell -ExecutionPolicy Bypass -File scripts\build-all.ps1 -StageLayout`
   → expect `LAYOUT STAGED OK -> ...\dist\NVIDIA ShadowPlay` followed by
   `STAGED TREE APPLIED OK` and the full manifest-lite check (incl.
   Application\ hosts + the 10 `.NET Deployment\` json).
4. Confirm `Application\NVIDIA API.exe` exists (apphost stamping is
   Windows-only) and has the ShadowPlay icon.
5. Run `Overlay\bin\Release\net10.0-windows10.0.26100.0\Launcher.exe`
   → click its API launcher; then run `...\Application\NVIDIA API.exe`
   directly — the tray hub must come up (this proves the `..\Services\`
   host mechanism end-to-end WITHOUT deps.json in Services\ — the
   AppLayout resolver now supplies the cross-folder assemblies).
6. Start a recording; confirm `Config\config.json` is picked up and
   `Logs\` receives `encoder-detect.log` / `ui-engine.log`.

If step 4's host ever fails to start (`The application to execute does not
exist` naming the wrong path), the fallback is to keep the apps' standard
hosts inside Services\ and turn Application\ entries into shortcuts — but
the mechanism is verified in `scripts/apphost_test` and should not be
needed.

## Runtime resolution summary (one line per mechanism)

- host: `Application\X.exe` → embedded `..\Services\X.dll` (+ runtimeconfig from Services\; deps.json now in .NET Deployment\)
- deps miss → `AssemblyLoadContext.Default.Resolving` → `Engine\Core\Audio\Graphics\Libraries\Services\Runtimes` under root
- every file path → `AppLayout.Dir` (root), family subfolders per the table above
