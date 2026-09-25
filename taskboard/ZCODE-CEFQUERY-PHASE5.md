# ZCODE: CEFQuery Phase 5 — implement proven OSC-relevant gaps
Work dir:
C:\My Project\NVIDIA-Shadowplay-gfe

## Authority
Mission 3:
587b60d298
Read:
- docs\OSC-GALLERY-IPC-EXTRACTION.md
- Overlay.Engine\CefQueryBridge.vb
- Overlay.Engine\PROTOCOL-MATRIX.md

## Scope
Implement ONLY these 5 cefQuery commands proven OSC-relevant:
1. QUERY_BROWSE_DIRECTORY
2. QUERY_OSC_DROP_URL
3. QUERY_WIN_KB_MESSAGE
4. QUERY_TIME_INFO
5. QUERY_SYSTEM_INFO

Do not implement QUERY_WIN_DIR_INFO yet. Mission 3 proved osc gallery
directory listing uses HTTP Gallery endpoints; QUERY_WIN_DIR_INFO is not
called by osc page.

## 1. QUERY_BROWSE_DIRECTORY
Production shape:
request {name}
Purpose: native folder picker for Gallery "Open Location".
Source: docs\OSC-GALLERY-IPC-EXTRACTION.md §D.1.

Implement on the host side in Overlay.Engine\CefQueryBridge.vb.
Use native Windows folder picker APIs already available to the project;
do not invent a custom WebView UI.

Preserve the cefQuery wire contract already used by CefQueryBridge:
success response is the string expected by vendor.js.
Read the exact consumer/return semantics from the reference extraction
before coding. Handle cancel deterministically (no hang).

## 2. QUERY_OSC_DROP_URL
Production shape:
request {url, xpos, ypos}
Purpose: drag/drop transfer URL used by oscCreateDropUrl.

Trace the exact consumer/return semantics from vendor.js/app.js evidence
in docs\OSC-GALLERY-IPC-EXTRACTION.md before implementation.
Implement only the proven behavior; do not guess a file-download pipeline.

## 3. QUERY_WIN_KB_MESSAGE
Production shape:
request {keycode, keymodifier}
Purpose: host keyboard message.

Find the page-side call semantics in the reference bundle and mirror the
minimum required behavior. Must not alter global keyboard hooks or add a
new listener; respond through the existing CefQueryBridge only.

## 4. QUERY_TIME_INFO
Production shape:
request {type}
Purpose: host time information.
Find the exact response fields/units and supported type values in the
Mission-3 evidence/vendor wrapper. Return the same JSON string shape.
Use invariant/UTC-safe formatting where the evidence requires it.

## 5. QUERY_SYSTEM_INFO
No guessed shape allowed.
Recover exact request/response shape from Mission-3 extraction/vendor.js
(and app.js caller if present), then implement the smallest equivalent
host response. If it is not actually called by our osc page, document
that fact and leave implementation out rather than inventing behavior.

## Validation
- Add/extend focused Overlay.Engine tests for all implemented commands.
- Test success + malformed request + cancel/no-op cases where applicable.
- Verify unknown commands still return the existing not_implemented error.
- Build the narrow Overlay.Engine target only.
- Do not modify Backend, deploy/layout, vendor.js, app.js, or NVIDIA
  binaries/source.
- Do not touch A:\ backup or Program Files NVIDIA source tree.

## Commit
One commit only:
osc: implement OSC-relevant cefQuery phase 5 commands

Do NOT push.
