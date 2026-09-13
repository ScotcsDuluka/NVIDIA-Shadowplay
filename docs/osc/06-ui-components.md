# 06 — UI Component Map (Phase 7)

Names extracted from the bundles (registration literals survive minification).
Counts: 26 app services, 52 controllers, 73 directives, 113 constants.
Templates: the bundle registers `template:"…"`/`templateUrl` — GFE inlines
its templates in the bundle; **no separate .html templates ship** (verified:
no .html files in Overlay/osc besides index.html).

## Core services (app.js, all HIGH-literal)

| Service | Contract role | ReUI |
|---|---|---|
| `oscDisplayService` | window lifecycle: open/close/toggle, `setDisplayRects`, fullscreen transitions, WindowState channel, `QUERY_OSC_REGISTER_CLOSE_EVENT` | `bridge/native.js` + `services/displayRects.js` |
| `shadowPlayService` | record/replay/screenshot/broadcast logic + notification channel wiring + hotkey shortcut maps | `services/api.js` + `services/channels.js` |
| `socketService` | io connect with cookie query, emit/register bridge to eventAggregator | `bridge/socket.js` |
| `cefService` (vendor) | window.cefQuery wrapper | `bridge/cefQuery.js` |
| `oscNotificationService` | toast/notification presentation | `ui/components/toast.js` |
| `hotkeyService` / `keyboardService` | hotkey string display + monitoring toggle | not needed in M1 (host owns hotkeys) |
| `settingsService` | settings reads/writes | `ui/screens/settings.js` |
| `hardwareService` | system info (boot resolve) | skipped (M1 `{}` catch-all) |
| `errorDialogService` | fatal error surface | degraded panel |
| `telemetryService` | jsEvents batching | REMOVE (no server) |
| `piplConfigService` | config merge at boot | skipped (M1 `{}`) |
| others (broadcast/coplay/gallery/nvCamera/mods/quietMode2/osd/…) | per-feature | REMOVE in M1 scope |

## Controllers ↔ screens (52; the load-bearing ones)

| Controller | Screen/state | Notes |
|---|---|---|
| `BaseController` | base | boot orchestration |
| `MainMenuController` + `MainTopBarController` | main.main-menu | record/replay/broadcast tiles |
| `PreferencesMenuController` + `Preferences{Audio,Video,Recordings,Broadcast,Connect,…}Controller` | main.preferences.* | settings sections |
| `Gallery{Files,History,Remove,Upload}MenuController` | main.gallery.* | gallery |
| `FolderBrowserController` | recordings folder browser | uses `QUERY_WIN_DIR_INFO` |
| `ErrorDialogController`, `confirmationController` | error/confirm dialogs | |
| `OscNotifierController` | toasts | |
| `Osd*Controller` (5) | OSD overlays | status/perf/comments/viewers/webcam |
| `NvOauth*Controller`, `DestinationPickerController` | account/link flows | OAuth (M1 fails fast) |
| `ImageEditorController`, `VideoEditorController`, `VideoEditorGifController` | editors | upload flow |

## Directives (73; key ones)

`nvBase` (root), `nvMain`, `nvMainMenu`, `nvOscTopBar`, `nvOscTile`,
`nvOverlay`, `nvShadowplayStatus`, `nvProgressIndicator`, `nvSlider`,
`nvAccordion(+Pane)`, `nvConfirmation`, `nvErrorDialog`, `nvOscNotifier`,
`nvPreferences*` (15, one per section), `nvGallery*` (5), `nvOsd*` (5),
`nvVirtualGridList*` (3), `nvCameraMenu`, `nvOauth*` (2), `nvWebrtcChat`,
`modsMenu`, `octoolMenu`, `nvVideoEditor`, `nvVideoGifEditor`,
`nvImageEditor`, `settingsCustomize`, plus behavior directives
(`focus*`, `hoverFocus*`, `imageonload`, `nvFallbackSrc`, `nvFilter*`,
`nvChevron`, `getCustomizeWidth`).

## Styling facts (HIGH)

- No CSS files ship — styles are injected by webpack style loaders into the
  JS bundles (index.html has no `<link rel=stylesheet>`).
- ngMaterial theming + icon font (`nvgcshare.woff2`) + inline SVG icons.
- `ng-csp` is on (index.html) — no inline style/script evaluation reliance.

## ReUI component matrix (new UI, contract-preserving)

| New component | Replaces | Contract duties |
|---|---|---|
| `ui/shell.js` (drawer) | nvBase+nvMainMenu+TopBar | mounts interactive surfaces marked `data-osc-interactive` |
| `ui/screens/home.js` | MainMenuController tiles | REC state/elapsed/main action, replay badge |
| `ui/screens/settings.js` | PreferencesRecordings/Video | `/Record/Settings` + `/RecordPaths` read, honest unavailable state |
| `ui/components/toast.js` | OscNotifierController | bounded (5), 5 s life, kinds info/warn/error |
| `app/services/displayRects.js` | oscDisplayService.setDisplayRects | ResizeObserver + rAF-debounced rect reporting |
| `app/bridge/cefQuery.js` | cefService | exact response semantics (true/false/push/204) |
| `app/bridge/socket.js` | socket.io-client 2.5.0 | golden-transcript wire compliance (tested) |
| `app/services/l10n.js` | pascalprecht.translate | loads legacy `l10n/<lang>.json`, fallback dict |
