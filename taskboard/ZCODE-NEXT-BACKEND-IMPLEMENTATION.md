# ZCODE NEXT: Backend parity implementation — socket + missing routes + Gallery HTTP
Work dir:
C:\My Project\NVIDIA-Shadowplay-gfe

## Authority
Mission 2 commit: 57f57529dc
Mission 3 commit: 587b60d298
Read:
- docs\OSC-NODE-API-EXTRACTION.md
- docs\OSC-GALLERY-IPC-EXTRACTION.md
- docs\OSC-REQUIREMENTS-INVENTORY.md §6/§7

Our live clean-room backend is Backend\ (NOT NvBackend\). Do not invent a new tree.

## Lane scope
Implement the parity gaps proven by Mission 2/3:
A. socket event surface + trigger-compatible server emits
B. missing HTTP route families from node extraction
C. Gallery HTTP GetFolderListing flow used by osc
D. only the small cefQuery/gallery host changes explicitly proven necessary by §7

Do NOT touch:
- deploy/layout files
- AppLayout.vb
- NVIDIA API.vb
- Directory.Build.targets
- Overlay project/build files
- vendor.js/app.js
- any NVIDIA source/binary or A:\ backup

## A — Socket implementation
Use the exact event names/payloads/trigger semantics in
docs\OSC-NODE-API-EXTRACTION.md §A.

Current socket implementation:
Backend\socket.js
Backend\routes\shadowplay.js
Backend\index.js

Implement the proven event map without speculative events.
Preserve socket.io v2 / engine.io v3 compatibility.

Priority:
1. Record/Enable
2. InstantReplay/Enable, Started, Save, Upload semantics
3. Hotkey, Notification, WindowState
4. DisplayOscNotification, DisplayOscPreferences, DisplayOscState
5. Broadcast/Enable, Pause, SessionEvent
6. GameShare, NvCamera, SDK, QuietMode2, Account, abHub, PiplConfig,
   Settings/Language, gfeupdate only where §A proves a node emit.

Dead channels remain dead:
- POST /InstantReplay/Upload = no production route
- POST /Hotkey/DynamicToggle = no production route
- /abHubAPI/v.0.1/Message has no production emit
Do not resurrect them just for symmetry.

## B — Missing HTTP route families
Implement only the missing families documented in §B, using the exact
methods, paths, request fields, response shapes, status codes, and
persistence semantics extracted from production.

Prioritize families that the osc page actually exercises.
Keep existing route behavior unchanged unless §6 proves a correction.

Important correction:
- Settings language GET+POST is proven.
- /Hotkey/Monitor is the param route /Hotkey/:hk; do not create a fake
  parallel production route shape.
- Do not create /NvBackend/api; our source tree remains Backend\routes\.

Persistence may use our existing store layer where the production node
delegates to native state. Do not copy NVIDIA native persistence paths
verbatim unless required by the extracted wire contract.

## C — Gallery HTTP / playback
Mission 3 proved osc gallery listing is HTTP, not QUERY_WIN_DIR_INFO:
- POST /Gallery/v.1.2/GetFolderListing
- GET /EnumerateDrives
- POST /isDirectoryWritable
Use the exact request/response shapes in
docs\OSC-GALLERY-IPC-EXTRACTION.md §B.

Implement the page-used Gallery endpoints first.
Do not block this work on QUERY_WIN_DIR_INFO.
Do not implement guessed pagination/filter fields.

Playback is local-file HTML5 video according to §C. Do not invent a
media HTTP server or custom URL scheme.

If the current host already has a Gallery controller/server surface,
extend it minimally. Otherwise add the smallest route module consistent
with Backend\index.js registration pattern.

## D — Validation
Before completion:
- enumerate implemented routes and compare against §6/§7
- run targeted HTTP requests for every newly implemented route
- verify success/error status codes and exact JSON field names
- verify socket client can connect using socket.io v2 and receive each
  implemented channel with a representative payload
- verify existing OSC boot still works
- run the narrowest relevant test/build available

Do not claim parity for a route/event that was not exercised.

## Deliverable
Create implementation code + focused tests/docs only as required by the
existing project conventions.
Do not modify unrelated files.
Do NOT push.
Commit only your implementation files and tests/docs:
1. osc: implement node-derived socket event parity
2. osc: implement node-derived route + gallery HTTP parity

If a dependency or missing production source blocks exact implementation,
STOP that sub-part, record the concrete blocker, and continue independent
sub-parts.