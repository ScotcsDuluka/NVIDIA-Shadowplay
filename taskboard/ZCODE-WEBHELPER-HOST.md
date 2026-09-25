# ZCODE: NvBackend host — NVIDIA Web Helper.exe (owner decision 1)

Work dir: C:\My Project\NVIDIA-Shadowplay-gfe
Branch: gfe-rebuild
Date: 2026-09-24

## Scope

Resolve the owner drawing's PENDING slot `NvBackend\NVIDIA Web Helper.exe`
(owner decision 1): a production host EXE for the staged Backend\ tree.

- New project `NvBackend\NvBackend.vbproj` (VB.NET, net10.0-windows10.0.26100.0,
  console, zero-dep) → `<AssemblyName>NVIDIA Web Helper</AssemblyName>` produces
  `NVIDIA Web Helper.exe` (apphost) + `NVIDIA Web Helper.dll` (body).
- Registered in Overlay\NVIDIA Overlay.sln under the "Integration Hosts" folder
  (NvContainer/Hook precedent).
- deploy-shadowplay-layout.ps1: canonical staging reserves the pending
  `NvBackend\NVIDIA Web Helper.exe/.dll` slot. The historical
  `deploy/legacy/deploy-nvoverlay-layout.ps1` is preserved for evidence.

## Research conclusion (evidence, not assumption)

The REAL `NVIDIA Web Helper.exe` (GFE 3.28.0.412, NvNode) is an embedded
Node.js v11.13.0 win32-ia32 runtime with a HARDCODED entrypoint
(`<PFx86>\NVIDIA Corporation\NvNode\index.js`); external argv is discarded,
execArgv=[], no respawn, module.paths from the entry location. Proven by the
Phase 1B launch contract (forensics/run1-4 + runtime-forensic.txt in the
NVIDIA-Shadowplay repo) + live statics here:

- PE32 i386, 29,446,696 bytes; nodejs.nvi package "NVIDIA NodeJS" 3.28.0.412
  (x86), node_modules.7z bundled, launcher nvnodejslauncher.exe.
- Live probe: the deployed NvNode copy currently serves 127.0.0.1:59001
  (owner's duluka index.js); its liveness is OPEN `/health` = 200 while
  `/Backend/v.1.0/health` = 401 (auth-gated) — the host guard recognizes both
  real liveness shapes.
- Consequence: a renamed/copy of the real exe cannot host `NvBackend\index.js`
  (hardcoded path) → a NEW host EXE is required. It is NOT a generic Node
  launcher and NOT a Win32 service wrapper — evidence before assumptions.

## Host contract (9 lane goals → implementation)

1. New Project / EXE — `NvBackend.vbproj` builds clean (0 warnings).
2. production name — AssemblyName = "NVIDIA Web Helper" (NVIDIA Share /
   NVIDIA Controls precedent); exe+body dll.
3. installed owner-layout — everything resolves from AppContext.BaseDirectory;
   proof runs executed with foreign cwd (C:\, C:\Users\..., C:\Windows\Temp).
4. backend on port 59001 — spawns `node index.js` cwd=installed NvBackend\;
   port = NVSP_PORT env > config.json > 59001 (same resolution as
   Backend\config.js; launch contract: JSON+env only, no registry, argv ignored).
5. stdout/stderr/logging — console + `Logs\NVIDIA Web Helper.log` beside the
   exe; child lines tagged [NODE]/[NODE!].
6. duplicate-process guard — named mutex `Global\NvBackend.WebHelperHost.<fnv1a(dir)>`
   (Local\ fallback); port guard: healthy backend up → idempotent exit 0;
   foreign holder → exit 1, no doomed spawn.
7. graceful shutdown — Ctrl+C (e.Cancel, grace window 8s so node's SIGINT
   handler can run on the shared console) / ProcessExit; job-terminate fallback.
8. health/proof — polls GET /Backend/v.1.0/health ≤60s → `PROOF:` log line;
   child death before proof → explicit error exit; boot window elapsed →
   warn + keep supervising.
9. dependency resolution from real NvBackend\ — node: NVBACKEND_NODE_EXE env >
   NvBackend\node.exe > PATH > %ProgramFiles%\nodejs > %LocalAppData%\Programs\nodejs
   (install.ps1 candidate list); JS deps by standard node_modules resolution
   beside index.js (no NODE_PATH injection).

No-orphan guarantee: child assigned to a Windows job object
(KILL_ON_JOB_CLOSE) — the backend cannot outlive the host even on a hard
kill. Exit codes: 0 clean/idempotent, 1 foreign port holder, 2 no node,
3 no index.js, 4 spawn/pre-proof failure, 5 boot window elapsed, else
child code propagated.

## Proof transcript (2026-09-24, staged tree
dist\nvoverlay-layout\NVIDIA ShadowPlay\NvBackend\)

A. Idempotent guard vs the REAL backend (59001, run from C:\):
   `backend already healthy at http://127.0.0.1:59001/Backend/v.1.0/health —
   nothing to do (idempotent)` → EXIT=0
B. Full boot on NVSP_PORT=59011 from the staged layout:
   `PROOF: backend healthy at http://127.0.0.1:59011/Backend/v.1.0/health
   (node pid 11656, boot 10.3s)`; curl 200; [NODE] relay works.
C. Duplicate guard: second instance while B ran → `duplicate guard: another
   NVIDIA Web Helper host from this directory is already running
   (Global\NvBackend.WebHelperHost.597b5fbc)` → EXIT=0, no second backend.
D. Graceful shutdown ×4 (03:52:58 / 03:57:38 / 04:00:28 / 04:04:44):
   CTRL_BREAK delivered to the host console → `NVIDIA Web Helper stopped
   cleanly`, port freed, node reaped every time. Exit code of the graceful
   path = 0, captured by a console-detached parent (Start-Process +
   WaitForExit). Idempotent path exit code also 0.
E. No-orphan: taskkill /F on the host (pid 13876) while node 32044 listened
   on 59015 → node reaped by the job object instantly, port freed.

## Ops notes

- Console signal nuance: pid-targeted GenerateConsoleCtrlEvent only reaches
  group leaders; a console BROADCAST (pid 0) after AttachConsole reaches
  host+child. Interactive Ctrl+C in the host console reaches both natively —
  node runs its own SIGINT shutdown during the host's grace window.
- msys/Git-Bash background runs may report 0xC000013A for the WRAPPER shell
  when console events fly; clean-parent measurement (Start-Process) is the
  authority: host exit 0.
- Backend WMI hardware probe noise (PowerShell stderr from
  hardwareProbe.js) appears as [NODE] lines — faithful relay of the child,
  pre-existing Backend behavior.
- The Backend boots API-only when the staged tree has no ..\osc (expected
  in this layout; CEF/osc staging is a separate lane).

## Not done (by design)

- Restart policy: host exits with the child's code; restart authority stays
  with the owner's supervisor lane (NvContainer boundary note).
- No parity surface here; Backend\ remains the only backend tree.
- Historical deploy-nvoverlay-layout.ps1 is preserved under deploy\legacy\;
  current owner-tree staging uses deploy\deploy-shadowplay-layout.ps1.
