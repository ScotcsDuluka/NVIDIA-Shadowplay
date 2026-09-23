# MISSION: osc app.js deep-extraction (READ-ONLY recon)

Paste this whole card into ZCode. Work dir:
C:\My Project\NVIDIA-Shadowplay-gfe

## Context

We are rebuilding NVIDIA ShadowPlay's overlay stack. The real osc WebView
frontend is an Angular app whose main bundle is:

    C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\WebView\osc\app.js
    (1.6 MB, minified, single line — use regex search + string slices,
     NEVER dump the whole file into context)

A first inventory (docs\OSC-REQUIREMENTS-INVENTORY.md, written from the
node bridge sources) lists the Express routes the backend serves and the
keys in osc\config.js. YOUR job is the PAGE side: what app.js actually
CALLS, SHOWS, and STORES.

## HARD CONSTRAINTS (violating any = mission failure)

- READ-ONLY everywhere except docs\: never modify/move/rename/delete any
  file under C:\Program Files\NVIDIA Corporation\... or A:\NV OSC Backup\...
- No executing NVIDIA binaries, no installs, no service changes.
- You may ONLY create/modify files inside
  C:\My Project\NVIDIA-Shadowplay-gfe\docs\
- Cite evidence as app.js <charOffset or unique anchor string> for every claim.

## Extract (sections A-D)

A) CLIENT CALLS: every HTTP call app.js makes — method + URL template +
   which OSC_CONFIG field it references + what it does with the response.
   Group by feature (record, instant replay, broadcast, gallery, hotkeys,
   audio, webcam, highlights, perfmon/OSD, connectivity portals).
   Flag any route the page calls that is MISSING from
   docs\OSC-REQUIREMENTS-INVENTORY.md section 2.

B) SOCKET.IO CONSUMERS: every io.on / socket event name the page listens
   for, and which UI state it drives (the backend io.emit names must match
   these exactly).

C) SETTINGS STATE: the page-side settings model — field names, types,
   allowed values (resolution/fps/bitrate pickers incl. the
   BitRates/:quality/:resolution + GetSupported/GetCustomize flows),
   persistence calls (which endpoints store what).

D) L10N KEY MAP: from l10n\en-US.json + app.js usage, list the l10n keys
   that identify user-visible settings screens (helps us name our own UI).

## Output (exactly 2 things, nothing else)

1. docs\OSC-APPJS-EXTRACTION.md — sections A-D with evidence anchors.
2. Append a section "## 5. app.js findings (ZCode)" to
   docs\OSC-REQUIREMENTS-INVENTORY.md summarizing: routes the page calls
   that were missing from v1, socket events consumed, and any new keys.

When done, report: counts per section + confirmation the READ-ONLY
constraint held.
