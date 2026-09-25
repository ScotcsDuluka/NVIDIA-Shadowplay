# Repository Organization

This repository keeps build-sensitive project paths stable.

## Top-level rules

- API, Engine, Overlay, CaptureEngine.*, Launcher, Notifier, NvContainer, NvBackend, and NvCapture are source/owner projects. Do not move them casually because solution and project references depend on their paths.
- dist/ is generated deployment output.
- docs/reports/ contains historical forensic, timing, phase, and acceptance reports.
- docs/archive/ contains preserved ad-hoc or obsolete root artifacts that are retained for evidence.
- deploy/ contains deployment/layout scripts.
- taskboard/ contains worker/agent task cards.
- scripts/ contains maintained automation plus scripts/legacy/ for historical one-off harnesses that are no longer part of the normal workflow.
- spikes/ contains experimental source; spikes/legacy/ contains preserved one-off experiments that were previously at repository root.
- artifacts/ contains generated logs and other non-source outputs.
- config/ contains runtime/test configuration files.
- tools/ and test-recordings/ retain their existing specialist roles.
## Current ShadowPlay owner projects

The product-side architecture is documented in Logical Architecture/Layout.txt.

The intended owner tree is:

- Launcher
- NvContainer
- NvOverlay/WinForm
- NvOverlay/CEF
- NvCapture
- NvBackend
- NvAudio
- NvGraphics
- Runtime
- FFmpeg
- NvConfig
- Data
- Languages
- Resources
- .NET Deployment
- Logs

NvCapture/abi-evidence is retained as reverse-engineering evidence and is not generated deployment output.

## Cleanup policy

Generated files remain ignored rather than being mixed with source. Evidence is moved, not deleted, when its historical location is no longer appropriate.
