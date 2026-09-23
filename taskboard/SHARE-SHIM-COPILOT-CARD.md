# COPILOT TASK CARD - NvShim (root NVIDIA Share.exe dispatcher shim)

You are working in the repo `C:\My Project\NVIDIA-Shadowplay-gfe` (branch gfe-rebuild).
This is an ISOLATED new-project scaffold task. You create ONE new project and touch
almost nothing else. The lead agent (human+AI) reviews, verifies and commits.

## Context (read-only facts, do not modify)
- The live overlay family is FOUR separate VB.NET projects that all produce an exe
  named `NVIDIA Share.exe`, each in its own subdir of the deploy layout:
  Coordinator\, WinForm\, WebView\, Hook\ (they are separate assemblies by design).
- The owner target layout puts ONE `NVIDIA Share.exe` at the layout ROOT as a
  dispatcher shim: it reads `--role=<name>` and spawns the per-role exe in its subdir,
  then exits. Real supervision stays with Coordinator / NvContainer.exe - the shim is
  a launcher ONLY.
- Reference project for style/conventions: `NvContainer\` (VB.NET console,
  net10.0-windows10.0.26100.0, zero NuGet deps, JSON self-seed config, simple
  timestamped file log). Mirror its code style.

## Deliverable: new project `NvShim\`
Files (all NEW):
- `NvShim\NvShim.vbproj` - VB.NET console app, TargetFramework
  `net10.0-windows10.0.26100.0`, AssemblyName `NVIDIA Share`, RootNamespace NvShim,
  Option Strict On, ApplicationIcon `Assets\NVIDIA ShadowPlay.ico` (copy the icon
  from `NvContainer\Assets\NVIDIA ShadowPlay.ico` - do NOT modify the original),
  `AssemblyTitle/Never`-style metadata minimal, no package refs at all.
- `NvShim\Directory.Build.targets` - EMPTY project file
  (`<Project />`). Purpose: isolate the shim from the repo-root
  Directory.Build.targets sweeps that are keyed on AssemblyName 'NVIDIA Share'.
  This is REQUIRED, not optional.
- `NvShim\Program.vb` - entry point: parse args, resolve role, log, spawn, exit.
- `NvShim\ShimConfig.vb` - JSON config `Config\NvShim.json` next to the exe;
  self-seed defaults on first run (same pattern as NvContainer's ContainerConfig).
- `NvShim\ShimLog.vb` - append-only timestamped log `Logs\NvShim.log` next to exe
  (mirror ContainerLog.vb; create Logs\ if missing).

## Behavior spec (exact)
1. Arg parsing: accept `--role=<name>` (also `--role <name>`); if no `--role` given
   and a single bare positional arg exists, treat it as the role.
2. Default role table (self-seeded into Config\NvShim.json, editable):
   - `coordinator` -> `Coordinator\NVIDIA Share.exe`
   - `winform`     -> `WinForm\NVIDIA Share.exe`
   - `webview`     -> `WebView\NVIDIA Share.exe`
   - `hook`        -> `Hook\NVIDIA Share.exe`
   All paths relative to the shim exe's own directory. Config schema:
   `{ "roles": { "<name>": { "exe": "<relative path>", "args": "" } } }`
3. No args / `--help` / `-?`: print usage (role list + examples) to stdout, exit 0.
4. Known role: log "spawn role=<name> exe=<path>"; start via Process.Start with
   UseShellExecute=False, WorkingDirectory = the role exe's directory; do NOT wait
   for exit; exit 0.
5. Role exe missing: log the miss, print clear error to stderr, exit 3.
6. Unknown role: print known roles, exit 2.
7. Extra args: everything after the role is NOT forwarded in phase 1 (log a warning
   if extra args are present). Keep it dead simple.

## HARD CONSTRAINTS (violating any = task rejected)
- DO NOT modify any file outside `NvShim\` EXCEPT appending one project entry to the
  existing .sln (do not reorder or reformat existing sln lines).
- DO NOT touch Directory.Build.targets at repo root, the four family projects
  (Overlay / Overlay.Engine / Product\Coordinator / Product\Hook), NvContainer\,
  Engine\, deploy\.
- VB.NET only. Zero NuGet/package references. No async/await (mirror NvContainer's
  plain-thread style). No new documentation files.

## Verification you must run and paste output for
1. `dotnet build "NvShim\NvShim.vbproj" -c Release` -> 0 errors, 0 warnings.
2. Safe smoke tests (NEVER spawn the real family exes):
   - `NVIDIA Share.exe --help` (from NvShim bin output dir) -> usage text, exit 0
   - `NVIDIA Share.exe --role=nope` -> exit 2 + role list
   - Temporarily edit the SEEDED config in a COPY of the bin output (e.g. %TEMP%
     layout) so role `coordinator` points at `C:\Windows\System32\notepad.exe`;
     run `NVIDIA Share.exe --role=coordinator` there -> notepad starts, shim exits 0,
     Logs\NvShim.log shows the spawn line. (notepad is a SAFE stand-in; close it.)
3. Confirm `git status` shows ONLY: new `NvShim\` files + the one-line sln change.

## Report back
- Build output tail, smoke test exit codes, the seeded Config\NvShim.json content,
  and the full list of changed/new paths.
