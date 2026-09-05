# docs/ — Documentation Index

Quick map of what each document in this folder **is** and how current it is.
Evidence/forensics live in `/evidence` (local-only, git-ignored) — not here.

## Start here (CURRENT — maintained, reflect the live tree)

| File | What it is |
|---|---|
| `ARCHITECTURE.md` | Canonical architecture definition for the Engine-Rebuild branch |
| `APP-LAYOUT.md` | Root-fixed product tree assembled by `scripts/layout.proj` / `build-all.ps1 -StageLayout` |
| `BUILD_PROTOCOL.md` | Clean-build protocol followed by `scripts/build-all.ps1` |
| `CONFIG_RUNTIME_CONTRACT.md` | v2.0 config↔runtime contract (writer inventory, FPS/rate-control authority) |
| `CONFIG_OWNERSHIP_MATRIX.md` | Which config file owns which setting |
| `PHASE-13-SHADOWPLAY-CLOCK.md` | Audio clock analysis behind the current A/V alignment design |

## Reference (design specs & audit records of shipped phases — verify against code before relying)

- `P1-F-ENCODER-ARCHITECTURE.md`, `P1-F-NVENC-API-MAP.md`, `P1-F-NVENC-CALL-SEQUENCE.md`,
  `P1-F-NVENC-CONTRACT-GAP.md`, `P1-F-NVENC-IMPLEMENTATION-CHECKLIST.md`, `P1-F-NVENC-PHASE6-RESULT.md`
  — NVENC encoder phase design & API mapping.
- `phase-12-architecture-spec.md`, `-v2`, `-v3`, `phase-12-source-audit.md`,
  `phase-12-v3-source-audit.md`, `phase-12b-implementation-notes.md`
  — Phase 12 modular-engine split, spec evolution v1→v3 (v1/v2 superseded by v3).
- `P13.5-REMOVAL-INVENTORY.md` — what P13.5 removed and why.
- `NOTIFIER_SLOT_AUDIT.md` — notifier slot audit record.

## Historical (point-in-time snapshots — do not treat as current claims)

- `PHASE_PLAN.md` — "Engine-Audio Reset" plan from the aborted approach that assumed
  removing `CaptureEngine/` (see the banner inside the file).
- `PHASE0_BASELINE.md`, `PHASE1_VIDEO_VALIDATION_STATUS.md` — early validation baselines.
- `phase-11-postmortem.md` — Phase 11 postmortem.
- `STATUS-2026-08-28.md` — status snapshot dated 2026-08-28.

Rule of thumb: **`ARCHITECTURE.md` + `CONFIG_RUNTIME_CONTRACT.md` win** over any older
phase doc when they disagree; runtime logs win over everything (see `BUILD_PROTOCOL.md`).
