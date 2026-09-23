# Copilot Task Card — NvContainer next phase

> การ์ดนี้สำหรับวางให้ GitHub Copilot (agent mode) ทำงานใน repo
> `C:\My Project\NVIDIA-Shadowplay-gfe` — งานที่แยกไฟล์ใหม่ล้วน ไม่แตะไฟล์ proven

## MISSION

Extend the `NvContainer\` project (VB.NET, net10.0-windows, zero NuGet deps).
NvContainer is the root control-plane authority that OWNS `nvsphelper64.exe`
(the capture/engine worker). Phase-1 skeleton is DONE and smoke-tested:
TCP authority on configurable port, cookie auth, spawn/stop/start/restart,
crash backoff, file+console log.

## HARD GUARDRAILS (violation = revert)

1. ONLY touch files inside `NvContainer\` (and `Assets\` if truly needed).
2. NEVER modify: `Engine\`, `Overlay\`, `Directory.Build.targets`,
   `Directory.Build.props`, `scripts\layout.proj`, `scripts\build-all.ps1`.
   Those encode the OBT3-style product tree and are byte-verified.
3. VB.NET only. No NuGet package additions (System.Text.Json / BCL only).
4. Keep the log style: `ContainerLog.Log("...")` single-line messages.
5. No autostart of NvContainer itself (scheduled-task ownership is a
   human decision — CAPTURE-REST-PLAN non-goal).

## TASK 1 — HTTP health probe per worker

- `WorkerSpec`: add optional `healthUrl` ("" = disabled).
- `WorkerProcess.MonitorLoop`: when Running and healthUrl non-empty,
  GET it every 10s using `System.Net.Http.HttpClient` (BCL).
  - 2 consecutive failures => treat as crashed: Kill + restart path.
  - success resets the failure counter.
- Log lines: `worker 'X' health probe failed (1/2)`, `worker 'X' unhealthy — restart`.

## TASK 2 — Adopt-already-running process (`adopt` command)

- AuthorityServer: new cmd `"adopt"` with `"pid": <int>`.
- `Supervisor.Adopt(name, pid)`: wrap an existing process id into a
  WorkerProcess (Process.GetProcessById), state Running, monitor watches it.
- Use case: ownership switch — NvContainer adopts the engine that a
  scheduled task started, without killing it first.

## TASK 3 — Uptime + last-exit info in status

- WorkerProcess.Status(): add `startedAtUtc` (ISO-8601) and
  `lastExitCode` (null while Running).
- Keep JSON camelCase as-is.

## ACCEPTANCE

- `dotnet build NvContainer\NvContainer.vbproj -c Release` = 0 error 0 warning.
- Re-run the smoke pattern: status shows new fields; bad cookie still rejected.
- Do NOT commit. Report changed-file list back.
