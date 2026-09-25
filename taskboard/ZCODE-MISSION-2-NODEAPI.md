# MISSION 2: NvBackend api\ node-side deep-extraction (READ-ONLY recon)

Paste this whole card into ZCode. Work dir:
C:\My Project\NVIDIA-Shadowplay-gfe

## Context

We are rebuilding NVIDIA ShadowPlay's overlay stack. Our backend
(Overlay.Engine\OscControllerServer.vb) already serves the osc WebView
HTTP surface: record/IR/broadcast edges, picker-parity quality cluster
(Resolutions/Framerates/BitRates), settings round-trips — all verified
against the page-side wire shapes in docs\OSC-APPJS-EXTRACTION.md.

Two big gaps remain and BOTH need the NODE side as the authority:

1. SOCKET.IO: the page listens on 27 io events (list below). app.js only
   proves the event NAMES — the actual emit payloads and the code paths
   that trigger them live in the node bridge sources.
2. MISSING ROUTE FAMILIES: 12 route groups the page calls that our
   backend does not serve yet. app.js gave us request payloads from the
   client side, but the AUTHORITATIVE response construction (exact
   field names, envelope, status codes) lives in the node files.

The node bridge sources are here (READ-ONLY):

    C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\NvBackend\api\
      NvShadowPlayAPI.js   (~91KB, RegisterExpressEndpoints ~2700 lines,
                            ShadowPlay + socket emission)
      NvBackendAPI.js      (63 routes)
      NvGalleryAPI.js      (19 routes)
      NvAccountAPI.js      (8 routes)
      NvAbHubAPI.js        (4 routes)
      NvCameraAPI.js       (47 routes)

plus node_modules\ next to it if you need helper/express behavior.

Minified JS — use regex search + string slices, NEVER dump whole files
into context.

## HARD CONSTRAINTS (violating any = mission failure)

- READ-ONLY everywhere except docs\: never modify/move/rename/delete any
  file under C:\Program Files\NVIDIA Corporation\... or A:\NV OSC Backup\...
- No executing NVIDIA binaries, no installs, no service changes.
- You may ONLY create/modify files inside
  C:\My Project\NVIDIA-Shadowplay-gfe\docs\
- Cite evidence as <file> + charOffset or unique anchor string for every
  claim (e.g. "NvShadowPlayAPI.js @124808").

## Extract (sections A-D) -> docs\OSC-NODE-API-EXTRACTION.md

### A) SOCKET EMIT TRIGGER MAP (priority)

For EACH of the 27 page listeners below (names = literal route paths,
confirmed by app.js — see docs\OSC-APPJS-EXTRACTION.md §B), find in the
node sources WHERE io.emit (or equivalent) is called, WHAT triggers it
(native callback registration like SetGeneralNotificationCallback /
SetHotkeyCallback / SetOscCaptureStateChange / SetBroadcastSessionNotificationCallback,
a state poll, an HTTP edge side-effect...), and the EXACT payload JSON
(field names + types + one realistic example value):

    /ShadowPlay/v.1.0/Record/Enable
    /ShadowPlay/v.1.0/InstantReplay/Enable
    /ShadowPlay/v.1.0/InstantReplay/Started
    /ShadowPlay/v.1.0/InstantReplay/Save
    /ShadowPlay/v.1.0/InstantReplay/Upload
    /ShadowPlay/v.1.0/Notification
    /ShadowPlay/v.1.0/DisplayOscNotification
    /ShadowPlay/v.1.0/Hotkey
    /ShadowPlay/v.1.0/WindowState
    /ShadowPlay/v.1.0/DisplayOscPreferences
    /ShadowPlay/v.1.0/DisplayOscState
    /ShadowPlay/v.1.0/Broadcast/Enable
    /ShadowPlay/v.1.0/Broadcast/Pause
    /ShadowPlay/v.1.0/Broadcast/SessionEvent
    /GameShare/v.1.0/SessionUpdate
    /GameShare/v.1.0/CreateSession
    /NvCamera/v.1.0/Notifications
    /SDK/v.1.0/Notification
    /QuietMode2/v.1.0/state
    /QuietMode2/v.1.0/support
    /Account/v.1.0/UserToken
    /Account/v.1.0/PrivacySettings
    /abHubAPI/v.0.1/Message
    /abHubAPI/v.0.1/Status
    /PiplConfig/v.1.0/update
    /Settings/v.1.0/Language
    /gfeupdate/autoGFEDownload/autoGFEbeta

Priority order: Record/Enable, InstantReplay/* (5), Hotkey, Notification,
WindowState, DisplayOsc* (3), Broadcast/* (3) first — these drive the
main HUD; the rest in a second pass. If a payload is built dynamically,
show the construction code path (anchor) and enumerate every field that
can appear.

Also: does the node side validate the socket handshake query
X_LOCAL_SECURITY_COOKIE, and does it emit anything on connect
(welcome/state-sync burst)? We need to reproduce that too.

### B) MISSING-FAMILY WIRE SHAPES

For each route below: method + path + request payload fields
(names/types/required) + response JSON construction (exact field names,
envelope) + persistence side-effect (which file/key/store it writes):

    1.  /SDK/v.1.0/*  — the 11 Highlights routes (incl. Highlights/Customize
        twins /ShadowPlay/v.1.0/Highlights/{Customize,Session})
    2.  /QuietMode2/v.1.0/* (3 routes)
    3.  /GameShare/v.1.0/* (5 routes — note PUT/DELETE verbs,
        ConfigureControllerMapping)
    4.  /Nis2/v.1.0/* (2) and /DeepDVC/v.1.0/* (2)
    5.  /Feedback/v.0.1 and /HardwareInformation/v.0.1
    6.  /PiplConfig/v.1.0/data
    7.  /Settings/v.1.0/Language GET+POST and
        /gfeupdate/autoGFEDownload/autoGFEbeta GET
    8.  /Account/v.1.0/UserToken GET+POST, /PrivacySettings GET+POST
    9.  /abHubAPI/v.0.1/* (4 routes)
    10. POST /ShadowPlay/v.1.0/InstantReplay/Upload
    11. /ShadowPlay/v.1.0/Hotkey/Monitor GET+POST and
        /ShadowPlay/v.1.0/Hotkey/DynamicToggle POST
    12. NvCamera family: full shapes for /Capture/GetResolutions and
        /Notifications; for the other ~37 one-line purpose each is enough
        (the osc page itself only calls a handful).

For the ShadowPlay family routes we ALREADY serve (record/IR/audio/webcam/
overlay/settings), do NOT re-extract unless you find a response field we
could not know from app.js — focus on the gaps above.

### C) SETTINGS PERSISTENCE MAP

docs\OSC-APPJS-EXTRACTION.md §C.4 lists 16 persistence endpoint groups
(language, beta flag, record folders, audio/mic/PTT, volumes/tracks,
webcam, custom overlays, desktop capture, highlights budget, per-game
highlights, whisper, Nis2, DeepDVC, broadcast provider/ingest/title/
viewport, portal login token, GDPR consent). For each: WHERE does the
node side actually persist it — nodejs\config.json field path? a Services\
dll call? registry? %ProgramData% file? — with field names + defaults as
found. This decides where OUR backend stores the same settings.

### D) ERROR + ENVELOPE SHAPES

How the node side answers errors: status codes used, error body shape,
and whether success responses share an envelope (e.g. {status, data} vs
raw object). Include 2-3 concrete error examples with anchors. Also note
any route that answers 204/no-body.

## Deliverable + commit

- Write docs\OSC-NODE-API-EXTRACTION.md (sections A-D above).
- Update docs\OSC-REQUIREMENTS-INVENTORY.md: append "## 6. node-side
  extraction (ZCode mission 2)" with corrections/confirmations only —
  do not rewrite existing sections.
- git add the two docs files; git commit -m "osc: node-side API extraction
  (ZCode mission 2) - socket emit map + missing-family wire shapes"
- Do NOT push, do NOT touch anything else. Leave the worktree clean.
