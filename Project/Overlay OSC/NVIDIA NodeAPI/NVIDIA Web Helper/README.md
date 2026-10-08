# NvBackend — NVIDIA Web Helper.exe (production host)

The host project for the `NvBackend\` slot of the owner product tree
(`deploy/deploy-nvoverlay-layout.ps1` drawing 2026-09-24). Builds
`NVIDIA Web Helper.exe` (apphost) + `NVIDIA Web Helper.dll` (body) via
`<AssemblyName>NVIDIA Web Helper</AssemblyName>` — the production-name
precedent of `NVIDIA Share` / `NVIDIA Controls`.

## What it is (evidence, not assumption)

The REAL `NVIDIA Web Helper.exe` (GFE 3.28.0.412, `NvNode`) is an embedded
Node.js v11.13.0 win32-ia32 runtime with a HARDCODED entrypoint
(`<PFx86>\NVIDIA Corporation\NvNode\index.js`); it discards external argv
and cannot be repointed (proven by forensics: the Phase 1B launch contract,
`forensics/runtime-forensic.txt` in the NVIDIA-Shadowplay repo, and
`Backend/node-runtime/` as the immutable reference). It is an
NVIDIA-specific Node launcher, NOT a generic one — which is exactly why the
owner drawing reserves a NEW `NVIDIA Web Helper.exe` for our clean-room
tree: this project.

The parity backend itself lives in `Backend\` (express 4.19 + socket.io
2.5.1, 49/49 parity PASS) and is staged verbatim into the product tree's
`NvBackend\` — this host duplicates none of it.

## Host contract (the 9 lane goals)

| # | Goal | Where |
|---|------|-------|
| 1 | New Project / EXE | `NvBackend.vbproj` → `NVIDIA Web Helper.exe` |
| 2 | production name | `<AssemblyName>NVIDIA Web Helper</AssemblyName>` (exe + body dll) |
| 3 | installed owner-layout, not repo cwd | everything resolves from `AppContext.BaseDirectory` (the deployed `NvBackend\`) |
| 4 | backend on port 59001 | spawns `node index.js` with cwd = installed `NvBackend\`; port = env `NVSP_PORT` > `config.json` > 59001 (same resolution as `Backend\config.js`) |
| 5 | stdout/stderr/logging ชัด | console + `Logs\NVIDIA Web Helper.log` beside the exe; child lines tagged `[NODE]`/`[NODE!]` |
| 6 | duplicate-process guard | named mutex (`Global\NvBackend.WebHelperHost.<dirhash>`, `Local\` fallback) → exit 0; port guard: healthy backend already up → exit 0 idempotent; foreign port holder → exit 1, no doomed spawn |
| 7 | graceful shutdown | Ctrl+C (`e.Cancel` + grace window so node's own SIGINT handler runs) / `ProcessExit`; job-terminate fallback |
| 8 | health/proof check | polls `GET /Backend/v.1.0/health` up to 60s → `PROOF:` boot log line; child death before proof = explicit error exit |
| 9 | dependency resolution from real NvBackend\ | node resolution: `NVBACKEND_NODE_EXE` env > `NvBackend\node.exe` > PATH > `%ProgramFiles%\nodejs` > `%LocalAppData%\Programs\nodejs`; JS deps by standard node resolution from `NvBackend\node_modules` (no NODE_PATH injection) |

No-orphan guarantee: the child is assigned to a Windows job object with
`JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE` — the backend cannot outlive the host,
even on a hard kill or host crash. Safe, because the backend is crash-safe
by design (uncaughtException → keep serving; atomic tmp+rename store).

## Exit codes

```
0   ran and stopped cleanly / duplicate instance / backend already healthy
1   port held by a process that is not a healthy backend
2   node runtime not resolved
3   index.js missing (incomplete NvBackend tree)
4   node could not be started / exited before the health proof
5   boot window elapsed without health proof (child kept alive, supervised)
N   backend exited on its own with code N (propagated)
```

## Config contract

JSON + environment ONLY (launch contract §5 — the backend's argv is
discarded by the real Web Helper, so ours ignores it too; no registry).
Resolution order mirrors `Backend\config.js`: defaults < `config.json`
(`port`, `host`) < env (`NVSP_PORT`, `NVSP_HOST`). Health path:
`/Backend/v.1.0/health`.

## Staging

`deploy-nvoverlay-layout.ps1` stages `NvBackend\NVIDIA Web Helper.exe` +
`.dll` from this project's `bin\Release\net10.0-windows10.0.26100.0\`
alongside the Backend JS, plus a documentation copy of the dll into
`Services\` (per the owner drawing).

## Out of scope (by design)

- Restart policy: a crashed backend exits the host with the child's code;
  restart authority stays with the owner's supervisor lane (NvContainer
  boundary note in NvContainer.vbproj).
- Any parity surface: routes/socket/state belong to `Backend\`.
