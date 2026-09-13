# 10 — Instant Replay Flow — reconstructed from real code

Sources: `unpacked/src/app/0004.shadowPlayService.js`, `0208.shadowPlayEndpoints.js`,
`0210.MainMenuController.js`. Snippets verbatim.

## Chain at a glance

```
UI tile / hotkey (Alt+F10 toggle, Alt+Shift+F10? — see hotkeys note)
   → startInstantReplay / stopInstantReplay / saveInstantReplay
      → setInstantReplayRecording / saveInstantReplay endpoint
         → POST /ShadowPlay/v.1.0/InstantReplay/Enable {status}
         → POST /ShadowPlay/v.1.0/InstantReplay/Save
   ← /InstantReplay/Enable {status}        (enabled-state confirmation)
   ← /InstantReplay/Started {started|restarted}   (buffering state)
   ← /InstantReplay/Save    → toast w/ buffer length
   ← /Notification {notification:"recordingsaved", result, file}  (file reality)
```

## 1. UI entry points (MainMenuController, app/0210)

| Tile | Menu item | Handler |
|---|---|---|
| InstantReplay → NotRecording | "Start" (`l10n.instantReplayStart`) | `u.startInstantReplay()` |
| InstantReplay → Recording | "Stop" (`l10n.instantReplayStop`) | `u.stopInstantReplay()` |
| InstantReplay → Recording | "Save" (`l10n.save`, initially `enabled:!1`) | `u.saveInstantReplay()` |
| InstantReplay → Recording | "Upload" (Connect-only) | `u.uploadInstantReplay()` |
| Hotkey DVR toggle | — | `$()` in app/0004: `isIREnabled() ? stopInstantReplay() : startInstantReplay()` |

## 2. Start — `startInstantReplay()` (app/0004:679)

```js
e.all([we.isBroadcastRecordSupported(), we.isBroadcastBlocking(), we.isCoplayBlocking()])
 .then(function(e){ var t=e[0], n=e[1]||we.isBroadcastStartActive, i=e[2];
   if (t && !n && !i)
     return we.setInstantReplayRecording(!0)
       .then("Instant Replay Started")
       .catch("Instant Replay cannot be started" → handleNodeError) });
```

Same concurrency gate family as record (Broadcast support + blocking +
coplay). Desktop/fullscreen telemetry branch (`isInDesktopMode`) is
telemetry-only.

## 3. Endpoint mapping (app/0208 — verified export table)

| Service method | HTTP | Notes |
|---|---|---|
| `getIREnableStatus` (`l`) | GET `/ShadowPlay/v.1.0/InstantReplay/Enable` | → `isIREnabled()`; caches into `mainMenuData.instantReplayEnabled` |
| `getIRRunningStatus` (`s`) | GET `/ShadowPlay/v.1.0/InstantReplay/Running` | → `isIRActive()` |
| `setInstantReplayRecording` (`d`) | POST `/InstantReplay/Enable` body `{status:"true"|"false"}` | start/stop buffer |
| `saveInstantReplay` (`c`) | POST `/InstantReplay/Save` | no body |
| `getInstantReplayBufferLength` (`f`) | GET `/InstantReplay/BufferLength` | used by save toast |
| IR Settings | GET/POST `/InstantReplay/Settings` | `{replayLengthSeconds, quality[, resolution, framerate, bitrateBps]}` |

## 4. Stop — `stopInstantReplay()` (app/0004:695)

```js
we.setInstantReplayRecording(!1).then("Instant Replay Stopped").catch(handleNodeError)
```

## 5. Save — `saveInstantReplay()` (app/0004:702)

```js
we.expectIRSave = !0;                 // consumed by the recordingsaved notifier
r.saveInstantReplay().then(e → e.data.status)
  .catch("Instant Replay recording cannot be saved" → handleNodeError)
```

## 6. Confirmation channels (app/0004 `ve()` registration + handlers)

| Channel | Handler | Payload → behavior |
|---|---|---|
| `/InstantReplay/Enable` | `de` | `{status:true}` → toast `INSTANT_REPLAY_STARTED`, `instantReplayEnabled=!0`, `STATUS_CHANGE_RECORD`; `{status:false}` → `INSTANT_REPLAY_STOPPED`, flag off |
| `/InstantReplay/Started` | `ce` | `{started:true}` or `{restarted:true}` → `instantReplayRunning=!0`, trigger `IR_RECORDING_STATE_CHANGED` + `STATUS_CHANGE_RECORD`; `{started:false}` → running=!0→!1 |
| `/InstantReplay/Save` | `fe` → `ue` | GET BufferLength → toast `INSTANT_REPLAY_SAVED` with `Xm Ys` (uses `OSC_CONFIG.autoUploadMs>0 ? autoUploadMs/1000 : lengthSeconds`) |
| `/InstantReplay/Upload` | `me` | toast `INSTANT_REPLAY_SAVED_TO_GALLERY` |
| `/Notification` | `pe` | `{notification:"recordingsaved"}` → `RECORDING_SAVED` + clears `expectIRSave` |

⇒ Save is a **two-event story**: the POST ack means "requested"; the
`/InstantReplay/Save` channel event means "UI should toast"; the
`/Notification recordingsaved` event means "file exists".

## 7. Hotkey wiring (app/0004 `ve()` tail — eventAggregator)

```
A.DVR        → autoUploadMs>0 ? uploadInstantReplay : saveInstantReplay
A.DVR_TOGGLE → $()  (IR on/off toggle)
```
(`A` = HOTKEY_EVENTS: the host/backend pushes hotkey activations as
aggregator events; the page never registers hotkeys itself.)

## 8. ReUI contract checklist

| Action | Must send | Must react to |
|---|---|---|
| IR start | POST `/InstantReplay/Enable {status:"true"}` | `/InstantReplay/Enable {status:true}` + `/Started` → badge ON + buffering |
| IR stop | POST `/InstantReplay/Enable {status:"false"}` | `{status:false}` → OFF |
| IR save | POST `/InstantReplay/Save` | `/InstantReplay/Save` → toast (buffer length); `/Notification recordingsaved` → idle/saved |
| IR settings | GET/POST `/InstantReplay/Settings` | M1 host: `{}` → show unavailable (already implemented in `next/`) |

M1 reality check: the M1 host answers all IR endpoints `{}` and pushes none
of these channels — the ReUI therefore shows IR as **status-only + honest
"unavailable"** until OscEngineClient grows replay commands.
