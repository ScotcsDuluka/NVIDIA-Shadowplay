# WebHelper patch set (standalone backend for non-NVIDIA machines)

Full NVIDIA Web Helper package is NOT in git (proprietary binaries) - the
complete working copy ships in NVIDIA-Host-Portable (payload).

Our files (in git):
- DulukaAPI.js       - drop-in NvAccountAPI replacement (Duluka accounts)
- DulukaCaptureAPI.js - state backend for the "Duluka Capture" osc page
- index.js           - NVIDIA index.js + standalone patches (proven 2026-09-19):
  stub NvBackendAPI (native crashes c0000005 without agent), skip Camera,
  skip GameStream init, skip ShadowPlayAPI wait, AccountAPI.catch,
  security off, registry port override. Deployment quirk: Web Helper.exe
  ignores argv - always runs hardcoded <PFx86>\NVIDIA Corporation\NvNode\index.js
