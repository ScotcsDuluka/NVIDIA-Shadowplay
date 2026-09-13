# 09 — Record Flow (Manual Record) — reconstructed from real code

Source of truth: `unpacked/src/app/0004.shadowPlayService.js` (service),
`0208.shadowPlayEndpoints.js` (REST), `0210.MainMenuController.js` (UI),
`0012.oscDisplayService.js` (window interplay). Snippets quoted verbatim from
the source-like tree.

## Chain at a glance

```
UI tile / hotkey
   → shadowPlayService.tryStartManualRecord / stopAndSaveManualRecord
      → shadowPlayService.startManualRecord → setManualRecording
         → POST /ShadowPlay/v.1.0/Record/Enable  {status:"true"|"false"}
   ← socket /ShadowPlay/v.1.0/Record/Enable {status}   (confirmation)
   ← socket /ShadowPlay/v.1.0/Notification {notification:"recordingsaved", result, file}
      → state update + toast
```

## 1. UI entry points (MainMenuController, app/0210)

| Tile state | Menu item | Handler |
|---|---|---|
| ManualRecord → NotRecording | "Start" (`l10n.start`) | `u.tryStartManualRecord(!1)` |
| ManualRecord → Recording | "Stop & save" (`l10n.stopAndSave`) | `u.stopAndSaveManualRecord()` |
| Hotkey (Alt+F9) | — | `J()` in app/0004: `isMRActive() ? stopAndSaveManualRecord() : tryStartManualRecord(!0)` |

## 2. Service layer (app/0004)

### Start — `tryStartManualRecord(closeOsc)` → `startManualRecord()`

Privacy-control gate FIRST (verbatim):

```js
we.tryStartManualRecord = function(e) {
  we.shouldWeAskUserToTurnOnPrivacyControl().then(function(t) {
    if (t === !1) return we.startManualRecord();
    var n = { title:"l10n.manualRecord", question:"l10n.captureRecording", ... 
              topAction: we.enableDTandStartManualRecord, closeOSC: !!e, ... };
    w.openOSC("main.confirmation", n);        // confirmation dialog state
  }, ...)
};
we.enableDTandStartManualRecord = function(e) {
  return we.setDesktopCaptureEnabled(!0).then(function(t) {
    we.startManualRecord(), e === !1 && o.go("main.main-menu");
  })
};
```

### Start — `startManualRecord()` — blocking checks then POST

```js
e.all([we.isBroadcastRecordSupported(), we.isBroadcastBlocking(), we.isCoplayBlocking()])
 .then(function(e){ var t=e[0], n=e[1]||we.isBroadcastStartActive, i=e[2];
   if (t && !n && !i) return we.setManualRecording(!0)
     .then(... "Manual Record started")
     .catch(→ "Manual Record cannot be started" → handleNodeError + telemetry error) });
```

Blocking gates (all REST-driven, `isRecordBlocking()` app/0004:580):
- `/Record/Concurrency/Broadcast` support flag; if not concurrent → IR
  enabled/active, MR active, or Highlights active each BLOCK with a
  `WARNING_*` toast.

### POST payload (app/0208, endpoint `x`)

```
POST {base}/ShadowPlay/v.1.0/Record/Enable      body: {status: "true"}
```
(GET variants: `h` = GET /Record/Enable → isMREnabled, `b` = GET /Record/Running
→ isMRActive. Header `X_LOCAL_SECURITY_COOKIE` rides every request.)

### Stop — `stopAndSaveManualRecord()`

```js
we.expectManualSave = !0;                    // flag consumed by save-notifier
we.setManualRecording(!1)                    // POST /Record/Enable {status:"false"}
```

## 3. Confirmation channels (app/0004 `ve()` + handlers)

| Channel | Handler | Behavior on payload |
|---|---|---|
| `/Record/Enable` | `ge` (recordNotifier) | `{status:true}` → toast `RECORD_STARTED`, `mainMenuData.manualRecordEnabled=!0`, trigger `STATUS_CHANGE_RECORD`; `{status:false}` → toast `RECORD_STOPPED`, flag off |
| `/Notification` | `pe` (processNotification) | `{notification:"recordingsaved", result, file}` → success: trigger `RECORDING_SAVED` + clear `expectManualSave`/`expectIRSave` (whoever set it), perf end; failure: error toast + capture-error telemetry |

⇒ **The UI never trusts the POST response alone** — recording state flips on
the socket confirmation, and "saved" is only real when the Notification
`recordingsaved` event arrives.

## 4. Resulting state (mainMenuData + STATUS_CHANGE_RECORD)

`ge`/`de` write `we.mainMenuData.{manualRecordEnabled, instantReplayEnabled,
instantReplayRunning}` then `eventAggregator.trigger(STATUS_CHANGE_RECORD)`;
the MainMenuController re-renders tile state (`NotRecording ⇄ Recording`,
highlight color, elapsed line).

Elapsed clock FACT (app/0004:710-716): `Ce`/`Oe` (IR/MR start-seconds) are
declared and read by `getIRRecordTime()/getMRRecordTime()/waitForIRStartToComplete()`
but **never assigned in this module** — in build 3.28.0.412 the in-service
elapsed clock is vestigial; the menu shows state strings, not a live timer.
(Do not invent a timer semantics when re-implementing; the new UI's live
elapsed counter is an M1-host-driven addition, see 08-host-contract.)

## 5. Overlay interplay during record

- Record start does NOT open/close the overlay window (works from the game
  via hotkey); if the menu is open, tiles update in place.
- `tryStartManualRecord(true)` (hotkey path with privacy prompt pending)
  opens the confirmation dialog via `openOSC("main.confirmation", …)`.
- Save success also fires `RECORDING_SAVED` → toast "Saved to Gallery".

## 6. ReUI contract checklist (per action)

| Action | Must send | Must react to |
|---|---|---|
| Start record | POST /Record/Enable `{status:"true"}` | Record/Enable `{status:true}` → REC state |
| Stop&save | POST /Record/Enable `{status:"false"}` | `{status:false}` + Notification `recordingsaved` → idle + toast |
| Privacy prompt | (GFE: confirmation state + `/DesktopCapture/Enable` POST) | M1 host has no privacy/desktop-capture contract → M1 ReUI skips the gate |
| Concurrency gate | GET /Record/Concurrency/Broadcast | M1: absent → treat as non-blocking (host catch-all `{}`) |
