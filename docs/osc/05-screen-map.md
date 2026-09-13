# 05 — Screen Map (Phase 6)

Derived from the ui-router registrations in app.js (`.state("name", {url})` —
38 states, all literals). `$urlRouterProvider.otherwise("/base")`.
`<div ui-view>` in index.html is the single mount; `nv-base` is the root
component (template `"<nv-base></nv-base>"`).

```
base (/base)                          ← boot resolves run here; <nv-base>
└── main (url "")                     ← shell
    ├── main.main-menu (/main-menu)   ← DEFAULT screen after open
    ├── main.error-dialog (/error-dialog)
    ├── main.confirmation (/confirmation)
    ├── main.preferences (/preferences)
    │   ├── .connect        (/preferences/connect)
    │   ├── .hangout        (/preferences/hangout)
    │   ├── .overlays      (/preferences/overlays)
    │   ├── .keyboard-shortcuts (/preferences/keyboard-shortcuts)
    │   ├── .recordings     (/preferences/recordings)
    │   │   └── .folder-browser (/preferences/recordings/folder-browser)
    │   ├── .stream         (/preferences/stream)
    │   ├── .mods           (/preferences/mods)
    │   ├── .broadcast      (/preferences/broadcast)
    │   ├── .privacy-control (/preferences/privacy-control)
    │   ├── .highlights     (/preferences/highlights)
    │   ├── .notifications  (/preferences/notifications)
    │   ├── .audio          (/preferences/audio)
    │   ├── .video          (/preferences/video)
    │   └── .perfsettings   (/preferences/perfsettings)
    ├── main.broadcast-menu (/broadcast-menu)
    ├── main.gallery (/gallery)
    │   ├── .files   (/gallery/files-menu)
    │   ├── .history (/gallery/history-menu)
    │   ├── .remove  (/gallery/remove-menu)
    │   └── .upload  (/gallery/upload-menu)
    ├── main.content (/content/content)
    ├── main.history (/content/history)
    ├── main.myrig   (/myrig)
    ├── main.microphone (/microphone)
    ├── main.oauth-menu (/oauth-menu)
    ├── main.coplay-invite (/stream-invite)
    ├── main.guest-controls (/guest-controls)
    ├── main.edge (/edge)
    ├── nvcamera (/nvcamera)      ← top-level (no main. prefix)
    ├── mods (/mods)              ← top-level
    └── octoolmenu (/octoolmenu)  ← top-level (OC tool)

OSD surfaces (not ui-router states — display-rect overlays):
  nvOsd → OsdShadowplayStatus / OsdPerfStats / OsdComments /
          OsdViewerCount / OsdWebcam (status indicators during capture)
```

## Programmatic navigation (non-hotkey entries)

| Trigger | Target | Evidence |
|---|---|---|
| overlay open (hotkey/host) | `main.main-menu` (or `base` if already there — transition workaround) | oscDisplayService `_open` |
| notification with `state:"preferences"` | `main.preferences` | `O(e)` handler |
| notification with `state:"gallery"` + `params.file` | `main.gallery.upload` | `O(e)` handler |
| notification with `state:"gallery"` | `main.gallery.files` | `O(e)` handler |
| broadcast start | `main.broadcast-menu` | broadcastService `L()` |

## ReUI screen coverage decision (detail in 07-boundary-matrix)

| Legacy screen | M1 host contract | ReUI |
|---|---|---|
| main-menu (record/replay core) | `/state`, `/Record/Enable` | **Home** (record hero + replay status) |
| preferences.recordings/video | `/Record/Settings`, `/RecordPaths` | **Settings** (read-only until host wires provider) |
| notifications | Notification channels | **Toast layer** |
| gallery/broadcast/coplay/nvcamera/mods/octool/oauth/edge | none | REMOVE (evidence in 07) |
| OSD indicators | display-rects + painting cmds exist | UNKNOWN (no M1 push source) |
