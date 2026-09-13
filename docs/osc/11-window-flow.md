# 11 — Window Flow (Overlay Open / Close / Toggle / Fullscreen) — real code

Source: `unpacked/src/app/0012.oscDisplayService.js` (read in full; line refs
below are file lines). Companion facts: `vendor/0125…cefService` (QUERY_*
wrappers), host side `Overlay.Engine/OscHostForm.vb` + `CefQueryBridge.vb`.

## Internal flags (app/0012:104-113)

| Var | Meaning (from usage) |
|---|---|
| `P` | **menu open with input** (the main toggle state) |
| `R` | open-without-input counter (notifications/OSD hold the window open) |
| `z` | booting/opening guard ("need transition workaround" window) |
| `D` | pending `openOSC(!0)` after first navigation (`$stateChangeStart`) |
| `N` | painting-not-yet-allowed flag (starts `!0`) |
| `F` | desktop-mode flag (inverse of fullscreen; `null` until first probe) |
| `U` | borderlessMode (fullscreen-transition tracking) |
| `G` | "already on main-menu → go via base" workaround flag |

## Open — `openOSC(state?, params?)` (app/0012:125)

```
openOSC(e,t):
  setDisplayRects([])            // CLEAR rects first (host: no click zones yet)
  z || k()                       // k = getCaptureProcessInfo(4294967295) → appInFocus
    .then(() => { u.startWarm(); _(e,t) })
_ (openInternal):
  t default "main.main-menu"; if already there → G=!0, t="base" (transition workaround)
  P||(z=!0, P=!0, D=!0, trigger(OSC_STATE,"open"), push(OSC_HOTKEY_TOGGLE), startNavigation())
  E()                            // QUERY_FULLSCREEN_STATE
    .then(fs → F=!fs.fullscreen, trigger(HDR_ENABLED, fs.hdractive), trigger(DESKTOP_STATE,F))
  $state.go(t)
```

Then the router hooks FINISH the open (app/0012:188-197):

```
$stateChangeStart (leaving base):
  allowOSCPainting(!1)           // QUERY_OSC_SET_PAINTING off
  N=!1; if D → D=!1, s.openOSC(!0)      // QUERY_WIN_OPEN_OSC {enableInput:true}
            .then(→ notifyOverlayState(!0), endWarm())
$viewContentLoaded:
  G ? (G=!1, $state.go("main.main-menu"))
    : N || (allowOSCPainting(!0, z), N=!0, z=!1)
```

⇒ The REAL window-show command (`QUERY_WIN_OPEN_OSC`) fires when the router
LEAVES `base`, and painting is only allowed after the view has rendered.

## Close — `closeOSC()` (app/0012:129)

```
P && !z:
  appInFocus = {}; startPerf(closeOSC); P=!1
  $state.go("base")
  trigger(OSC_STATE,"closed"); trigger(PERF_OVERLAY_VISIBILITY_CHANGED)
  push(OSC_HOTKEY_TOGGLE); endNavigation()
  notifyOverlayState(!1)                    // POST …/NotifyOverlayState {open:!1,state}
  R>0 ? cefService.openOSC(!1) /* keep window, no input */
      : cefService.closeOSC()               // QUERY_WIN_CLOSE_OSC
```

`notifyOverlayState` payload: `{open, state}` where state = `"main"` |
`"permission"` (confirmation+PERMISSION) | `"highlightsSummary"` (gallery
upload with moments).

## Toggle — `T()` (app/0012:60)

```
z || (P ? closeOSC() : openOSC())
```

## Channel-driven behavior — `A(e)` on `/WindowState` (app/0012:75)

| windowMsg | Behavior |
|---|---|
| `dismiss` | `closeOSC()` |
| `fullscreenTransition` | if `F!==null && (R||P)`: re-probe `QUERY_FULLSCREEN_STATE`; on desktop↔fullscreen (or borderless change) → update `F`/`U`, trigger `DESKTOP_STATE`, `transitionDisplayFullscreen()`, and CLOSE (state change invalidates layout) |
| `overlayToggle` | `T()` |
| `showHotkeyMessage` | trigger `DISPLAY_HOTKEY` (toast layer shows it) |

Other registrations in `init()` (app/0012:180-197): `/DisplayOscPreferences`
→ `C` = `openOSC("main.preferences")`; `/DisplayOscState` → `O` =
preferences/gallery deep-open (`{state:"gallery", params}` → gallery upload
with file). Plus `QUERY_OSC_REGISTER_CLOSE_EVENT` (persistent) → push ⇒
`closeOSC()` (app/0012:90).

## Notification-mode open (no input)

```
openOSCForNotification(t):  t && setDisplayRects([]); R+=1; probe fullscreen
  P || ($state.go("base") if not there; cefService.openOSC(!1))   // window up, no input
closeOSCForNotification():  R−−; after 5 s timeout, if R==0 && !P → cefService.closeOSC()
openInDisplay(t) / closeForDisplay():  same R-counter pattern for OSD layers
```

⇒ The window is a SHARED resource with refcounting: menu (`P`) and
notification/OSD overlays (`R`) each keep it alive; whoever drops last sends
`QUERY_WIN_CLOSE_OSC`.

## Fullscreen transition — `transitionDisplayFullscreen()` (app/0012:151)

```
R<=0 && P<=0 || (closeOSC(); after 1 s → openOSC(P>0), trigger(OSD_SETTINGS_CHANGED))
```
Close-then-reopen (1 s) because display-mode changes invalidate rects/DPI.

## ReUI compliance notes (next/)

The new UI keeps the same outward semantics with a simpler internal model:
drawer open = rects reported + `QUERY_WIN_OPEN_OSC` ack'd; close = rects
cleared FIRST (click-through immediate) then `QUERY_WIN_CLOSE_OSC`; Escape =
`closeOsc()`; `/WindowState` handling identical (dismiss / overlayToggle /
fullscreenTransition / showHotkeyMessage). The R-counter machinery is not
needed in M1 because the host has no notification-hold or OSD layer yet —
documented here so M2 can adopt it without re-reverse.

## M1 host reality (facts)

- `QUERY_WIN_OPEN_OSC`/`QUERY_WIN_CLOSE_OSC` are wired to `ToggleOverlay` in
  the host; `WindowState.overlayToggle` push exists in the golden transcript;
  `NotifyOverlayState` REST hits the host catch-all (`{}`) — harmless.
- The host's own toggle (tray/hub) does NOT yet push
  `WindowState{overlayToggle}` (OscHostForm logs only) — page and host
  visibility can theoretically diverge; flagged as a host-side M2 item, not
  a page defect.
