# MISSION 3: Gallery CEF-IPC + full cefQuery enumeration (READ-ONLY recon)

Paste this whole card into ZCode. Work dir:
C:\My Project\NVIDIA-Shadowplay-gfe

## Context

We are rebuilding NVIDIA ShadowPlay's overlay stack. Our host already
implements the CEF message-router host side (CefQueryBridge.vb,
window.cefQuery polyfill injected before vendor.js runs) answering these
commands:

    QUERY_WIN_NODE_INFO            (boot handshake: {port, secret})
    QUERY_FULLSCREEN_STATE
    QUERY_OSC_DISPLAY_IS_DESKTOP_MODE
    QUERY_OSC_SET_DISPLAY_RECTS
    QUERY_OSC_SET_PAINTING
    QUERY_OSC_SET_EXPERIMENTAL
    QUERY_OSC_REGISTER_CLOSE_EVENT
    QUERY_WIN_OPEN_OSC
    QUERY_WIN_CLOSE_OSC
    QUERY_READ_SHARED_STORAGE
    QUERY_WRITE_SHARED_STORAGE
    QUERY_LOAD_STRING_TABLE
    QUERY_WIN_COPY_TO_CLIPBOARD
    QUERY_HTTPSERVER_START

Phase 5 = gallery handoff. Earlier assumptions said gallery browse would
run over HTTP (/Gallery/v.1.0/*). But the app.js evidence shows folder
browsing flows through CEF IPC — a query named QUERY_WIN_DIR_INFO — which
we do NOT implement yet, and the page may send other queries we have not
seen. YOUR job: pin down the production gallery data flow and enumerate
the FULL cefQuery command set so our host can answer everything.

Sources (all READ-ONLY):

    C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\WebView\osc\
      (app.js 1.6MB, vendor.js 1.4MB — minified; regex + slices only)
    C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\NvOverlay\
      (NVIDIA Share.exe, WebViewHook\ — strings/imports only, DO NOT run)
    C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\NvBackend\api\
      NvGalleryAPI.js (19 routes)

## HARD CONSTRAINTS (violating any = mission failure)

- READ-ONLY everywhere except docs\: never modify/move/rename/delete any
  file under C:\Program Files\NVIDIA Corporation\... or A:\NV OSC Backup\...
- No executing NVIDIA binaries, no installs, no service changes.
  Static analysis only (search, strings, hex dump of small ranges).
- You may ONLY create/modify files inside
  C:\My Project\NVIDIA-Shadowplay-gfe\docs\
- Cite evidence as <file> + charOffset / unique anchor string for every
  claim.

## Extract (sections A-D) -> docs\OSC-GALLERY-IPC-EXTRACTION.md

### A) QUERY_WIN_DIR_INFO — the gallery browse IPC

- Who ANSWERS it in production: the CEF host (NVIDIA Share.exe),
  WebViewHook injected DLL, or the node Web Helper? Evidence: strings/
  imports in the binaries, or handler registration in vendor.js.
- FULL wire shape: request JSON fields (path? filter? page? sort?), and
  the response entry shape for drives / folders / files — exact field
  names (name, path, size, modified, type flags...). Show 2+ call sites
  in app.js with anchors and what the gallery controller does with the
  result.
- Pagination/refresh semantics if visible.
- How this relates to the Gallery HTTP routes (NvGalleryAPI.js 19 routes;
  page calls EnumerateDrives, GetFolderListing, IsDirectoryWritable,
  Recent/8): which flow uses IPC vs HTTP, for what.

### B) Gallery HTTP routes still used

For each Gallery route the page actually calls (method + path + payload +
response shape + what UI state it feeds). Flag any NvGalleryAPI.js route
the page NEVER calls (backend-only). Include the thumbnail pipeline if
visible (who generates thumbs, where cached).

### C) Playback flow

How the page OPENS/plays a recorded MP4 or screenshot: file:// URL?
custom scheme? CEF query? external player handoff? Exact mechanism with
anchors, including any path-sanitization/security check the page applies
before handing a path out.

### D) FULL cefQuery command enumeration

Enumerate EVERY query command name the page can send (grep the crimson
cefService wrapper in vendor.js + all app.js call sites — command names
are the `request` JSON's discriminator or a field like `cmd`/`type` —
match how our known 14 above are shaped). Compare against our
implemented list from the Context section and output a diff table:

    command | implemented? | request shape | response shape | used by | anchor

For each NOT-implemented command, give the full wire shape. Include
persistent-query push semantics if any command uses them.

## Deliverable + commit

- Write docs\OSC-GALLERY-IPC-EXTRACTION.md (sections A-D above).
- Update docs\OSC-REQUIREMENTS-INVENTORY.md: append "## 7. gallery IPC
  extraction (ZCode mission 3)" with corrections/confirmations only.
- git add the two docs files; git commit -m "osc: gallery CEF-IPC
  extraction (ZCode mission 3) - QUERY_WIN_DIR_INFO + cefQuery enumeration"
- Do NOT push, do NOT touch anything else. Leave the worktree clean.
