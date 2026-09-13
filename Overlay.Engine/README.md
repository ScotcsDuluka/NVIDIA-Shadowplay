# NVIDIA Overlay Engine (osc host)

The second overlay host: a transparent per-screen window hosting NVIDIA
GFE's `osc` web app (Angular, bundled in `osc\`) inside **WebView2**,
driven over the same loopback TCP hub as the Forms overlay.

- **Runs in parallel** with `NVIDIA ShadowPlay.exe` (Forms overlay).
  Registers NO global hotkeys — toggling = hub `open_overlay`, the tray
  icon (double-click = toggle, menu = Exit), or the page itself.
- **No changes** to the Forms overlay's behavior; hotkey ownership stays
  with it (`UseOverlayEnabled=false` → the hub kills the old stack but
  never touches this engine).

## Run (dev)

```
dotnet build "Overlay.Engine/NVIDIA Overlay Engine.vbproj" -c Debug
Overlay.Engine\bin\Debug\net10.0-windows10.0.26100.0\NVIDIA Overlay Engine.exe
```

Requires the hub (`NVIDIA API.exe`) for TCP; the controller server and
the osc UI work without it. Toggle from any hub client:

```
node Tester\test\Overlay\Overlay.OscEngine.Tests\tools\open-overlay.js
```

Exit: tray icon → Exit (a recording session is never touched — the
engine process is owned by its own supervisor, same contract as the
Forms overlay).

## Config knobs

| Env | Default | Meaning |
|---|---|---|
| `OSCENGINE_PING_INTERVAL` | 25000 | engine.io pingInterval (tests use 1500) |
| `OSCENGINE_DEVTOOLS` | off | `=1` enables WebView2 DevTools |
| `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` | — | e.g. `--remote-debugging-port=9223` for the CDP tools |

`osc/config.js` — three flags flipped from the GFE defaults for M1
(Ansel/Photo/FreeStyle are out of M1 scope and their init path contains a
bundle bug — `.catch(function(){ ... error })` referencing an undefined
`error`): `nvCamera:false`, `anselLite:false`, `mods:false`.
Everything else is untouched GFE config.

## Layout / protocol docs

- `PROTOCOL-MATRIX.md` — the ground-truth contract (hub TCP line format,
  pipe rule, engine.io/socket.io framing incl. the binary polling mode,
  REST shapes) with `file:line` evidence for every claim.
- `Tester/test/Overlay/Overlay.OscEngine.Tests/` — 19-test suite; the
  wire codec is verified against `golden/golden-transcript.json` captured
  from the REAL `engine.io-client@3.5.4` + `socket.io-client@2.5.0`
  (`tools/` holds the harness + CDP/e2e scripts used during bring-up).

## M0 status (foundation — the overlay is now day-to-day usable)

- **Alt+Z toggle** ✅ — global hotkey registered by the engine itself
  (`Hotkeys.ToggleOverlay` from config.json, default Alt+Z). First-come-
  first-served: if the Forms overlay owns the combo, the engine logs and
  stays on tray/hub toggles. Debounced 500 ms (key auto-repeat fired
  WM_HOTKEY every ~30ms and machine-gunned the menu with the old 150ms).
- **Page-driven state** ✅ — the page's `QUERY_WIN_OPEN_OSC/CLOSE_OSC`
  are idempotent state setters (no push-back): pushing `overlayToggle`
  in response created an open/close ping-pong loop that crashed the
  host (measured 02:51). External triggers toggle; page confirmations set.
- **Fullscreen probe must lie about OUR window** ✅ — `QUERY_FULLSCREEN_
  STATE` counted our own just-Activated overlay (and any maximized app —
  its invisible borders overshoot the monitor) as a "fullscreen game",
  and the page dismissed its menu ~0.5s after every open. Fixed: exclude
  our pid + require absence of WS_CAPTION. Verified: menu stays open
  30s+ until the user closes it.
- **Dark backdrop, every open** ✅ — the osc page paints NO backdrop at all
  (real GFE's CEF host paints the dim layer). Ours is a host-owned
  `#oscengine-backdrop` div toggled with the overlay state via
  `html.oscengine-open` — survives reopen cycles (verified).
- **Window never hides, never moves** ✅ — parking off-screen made Chromium
  suspend the renderer and the recreated surface lost alpha (white
  second-open). Closed = transparent OSD chrome + full click-through.
- **BUILD GATE lesson** — MSBuild silently skipped compilation for ~1h of
  edits (stale dll, 0 errors reported) while a syntax error sat in
  OscHostForm.vb. Never trust "0 errors" without checking the dll
  timestamp; comments with `→` inside VB multi-line object initializers
  break the lexer (BC30985).

## M1 status (accepted / pending)

| # | Gate | Status |
|---|---|---|
| 1 | osc renders for real | ✅ (menu: Instant Replay / Record / Gallery tiles on screen) |
| 2 | cefQuery boot handshake | ✅ QUERY_WIN_NODE_INFO etc. |
| 3 | controller server | ✅ static + REST + auth cookie |
| 4 | socket.io/engine.io compat | ✅ golden tests 19/19 + live binary mode |
| 5 | TCP RECORD_START/STOP end-to-end | ⏳ protocol-exact lines unit-tested; needs the engine stack running for the live pass |
| 6 | hotkey ownership | ✅ none registered |
| 7 | Forms overlay untouched | ✅ |
| 8 | no WebView2 orphans on exit | ✅ (incl. abrupt host kill) |
| 9 | build 0 errors | ✅ |

M2 (next): Instant Replay wiring, notification parity, settings pages,
`_PtOsc` staging verification in a full Release build, hotkey arbitration
review.
