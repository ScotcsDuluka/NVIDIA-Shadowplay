# 04 — State Model (Phase 5)

Only states actually observed in the artifacts. Each entry: where it lives in
the legacy app, how it changes, and what preserves it in the ReUI
(`Overlay/osc/next/app/state/appState.js`).

## Connection / phase

| State | Legacy source | Transition | ReUI field |
|---|---|---|---|
| booting | `base` resolve chain (localNodeInfo → localizedConfig → hardwareInfo) | resolve failure ⇒ error-dialog/degraded | `phase: boot → ready \| degraded \| error` |
| connected | socket CONNECT → eventAggregator | socket DISCONNECT/ERROR → eventAggregator | `connection: connecting → online → offline` (socket.io auto-reconnect 1→5 s backoff) |
| degraded | services reject on missing cefQuery/REST | per-feature, non-fatal | boot to degraded panel + retry (never blank) |

## Recording (manual record)

| State | Legacy source | Evidence |
|---|---|---|
| Idle / Recording / Stopping | engine states `Idle, Recording, Stopping, HasError` reconciled from `engine_state_changed` (`[Overlay] Client.vb:233-252` pattern; M1 host: `OscProtocol.ShouldShowRecording`) | `Stopping` KEEPS the previous visible state (reconcile rule) |
| elapsed clock | `W(e,t)` formatter — `mm:ss` for record (`mr` = `hh:mm:ss`), floor-seconds from epoch start | shadowPlayService literal |
| pending-confirmation | M1: POST `/Record/Enable` returns `{}`; truth arrives via `/state` poll (1 Hz while active) | host `OnRecordEnableRequested` |
| blocking conditions | `isRecordBlocking` (broadcast/coplay/HDR concurrency checks) | shadowPlayService methods |

ReUI: `record: {running, pending, expected, elapsedSec}` — REC indicator +
blink rules: green steady idle, red blink recording, amber blink pending.

## Instant Replay

| State | Legacy source | Evidence |
|---|---|---|
| enabled/disabled | `isIREnabled` / `/InstantReplay/Enable {status}` | endpoints literal |
| buffering/ready | `isIRActive`, channel `/InstantReplay/Started` | service methods |
| saving | `saveInstantReplay` → `/InstantReplay/Save` POST; save confirmation via `/InstantReplay/Save` channel | service + channel |
| duration | `getIRRecordTime`, `/InstantReplay/BufferLength`, settings `replayLengthSeconds` | endpoints |
| M1 reality | **no replay contract on the M1 host** (OscEngineClient has record commands only); `/state.instantReplay` is always false | OscEngineClient.vb (no Replay*) |

ReUI: `replay: {enabled}` — status-only in M1; controls disabled with an
explicit "not available from the host yet" hint (no dead buttons).

## Display / window

| State | Legacy source | Evidence |
|---|---|---|
| open/closed | `oscDisplayService` `P` flag; open ⇒ `OSC_STATE "open"` event, `startNavigation(OSC_NAVIGATION_TIME)` | service literal |
| desktop vs fullscreen | `QUERY_FULLSCREEN_STATE` → `F = !fullscreen` → `DESKTOP_STATE` event; borderlessMode tracked | service literal |
| displayRects | cleared on open, panel bounds while open, cleared on close | `setDisplayRects` logic |
| close request | persistent `QUERY_OSC_REGISTER_CLOSE_EVENT` push → `closeOSC()` | service literal |
| windowMsg | `/WindowState` channel: `dismiss` → close; `fullscreenTransition` → re-probe + close on change; `overlayToggle` → toggle; `showHotkeyMessage` → DISPLAY_HOTKEY event | `A(e)` handler |

## Settings / language / notifications

| State | Source | Notes |
|---|---|---|
| record settings | GET `/Record/Settings` → `{quality, resolution, framerate, bitrateBps}` (fields from POST data shapes) | M1 host: `{}` until provider wired |
| save path | GET `/RecordPaths` → `{savePath}` (M1) | |
| language | GET `/Language` (M1) / `/Settings/v.1.0/Language`; push `/Settings/v.1.0/Language`; 28 tables in `l10n/` | `QUERY_LOAD_STRING_TABLE` ack'd by M1 host |
| toasts | bounded queue (ReUI: max 5, 5 s life) from Notification/DisplayOscNotification/SDK channels | legacy `oscNotificationService` |
| shared storage | `QUERY_READ/WRITE_SHARED_STORAGE` keyed by `path` | host `SharedStorageStore` |

## Error surfaces (Phase 15 requirements derived from these)

- cefQuery failure → `{errorCode, errorMessage}` — 204 = cancelled (do NOT
  surface as error).
- REST failure → HTTP status + `{}` body possible — boot-time resolves must
  never reject on unknown endpoints (host answers `{}`).
- engine errors → `engine_response:engine_record_start,error[,reason]` →
  osc state → idle + reason toast (M1 host: `RecordFailed` event, only
  logged).
- Session expiry → jarvis `401 → "sessionExpired"` (legacy; not in M1).

**Never display**: raw exceptions, secrets (`secret`, security cookie),
stack traces (PHASE 16).
