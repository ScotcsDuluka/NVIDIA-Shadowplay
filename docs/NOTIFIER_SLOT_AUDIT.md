# NVIDIA Notifier, [7] Notifications & the Toast Slot System — Architecture Audit

> **Anchor:** branch `Engine-Rebuild-Stabilization` @ `448e16e` (merge of `cb904dd` + `4952766`)
> **Author:** GLM/3 (doc/architecture agent) — read-only audit, doc-only deliverable
> **Method:** every claim below was traced directly in source at the anchor commit. No
> PROJECT_MEMORY.txt content was used as evidence. Line refs are exact at the anchor.
> **Scope:** the `Notifier/` project (NVIDIA Notifier), the Overlay's Settings →
> Notifications page (`[7] Notifications.vb`), and the toast Slot system (OWNER specs
> T29.2 / T29.3 / T29.4 / T30 RaiseStack). This is an audit, not a feature spec.
> **Status:** findings are FACT + IMPACT + RECOMMENDATION. Nothing here is decided —
> items marked OWNER DECISION REQUIRED stay with the OWNER.

---

## 0. Verdict (TL;DR)

The Notifier is architecturally healthy: one display choke point (`OnMessage`), one
per-category gate (`NotificationAllowed` → `config.json → Notifications.*`), a
user-configurable 1–3 slot stack with FIFO queueing and same-group dedup, and an
OBS WebSocket bridge that funnels into the same choke point. The Slot system
matches the OWNER specs (T29.2 group model, T29.4 anti-spam, T30 stack reflow /
queue / topmost) and its config key (`Notifications.SlotCount`) is read live on
every toast.

Three real defects were found, all of the same class: **toast keys the Overlay
sends are missing from the Notifier's key registry**, so those toasts are silently
dropped before any gate, toggle, or slot routing can act. Two dead-toggle +
fossil-file clusters and a set of production test controls round out the findings.
No engine (Video/Audio/FFmpeg) code is involved anywhere in this subsystem.

| ID | Severity | One-liner |
|----|----------|-----------|
| F-1 | HIGH | `l10n.recording_error` never displays — 3 senders, key absent from registry |
| F-2 | HIGH | `l10n.extension_not_found` never displays — key absent from registry |
| F-3 | HIGH | `l10n.notificationErrorEngineUIInUse` never displays — key absent from registry |
| F-4 | MEDIUM | Test buttons (Button1/2/3) and toast-click → `Application.Restart()` ship in the production UI |
| F-5 | MEDIUM | Sentinel-file fossils: `notifier_main` never written (9 readers pin base Y=105), `notifier` written (never read), `notifiermainoff` deleted (never written), per-key trigger files deleted (never written) |
| F-6 | LOW | 12+ legacy registry entries with no sender (harmless dead weight) |
| F-7 | INFO | Language file re-read from disk on every toast (`LoadLanguage` per `OnMessage`) |
| F-8 | INFO | 300 ms toast throttle exists only on the OBS path; TCP path relies on group coalescing + queue (works, but worth knowing) |

---

## 1. Component map

```
NVIDIA Experience (Launcher)          NVIDIA API (hub)  ── TcpListener :5000
        │                                  │  broadcast relay (skips sender)
        │ watch Overlay.UseOverlayEnabled  │
        ▼                                  ▼
┌────────────────────────────┐   ┌──────────────────────────────┐
│ NVIDIA Overlay (WinForms)  │   │ NVIDIA Notifier (WinExe)     │
│  [1] Main Menu.vb          │   │  Loader.vb (form + router)   │
│   ShowNotifier(key) ───────┼──►│  [Notifier] Client.vb        │
│  SystemMonitor.vb          │   │   registry + gates + groups  │
│  UpdateHelper.vb           │   │  TcpClientHelper.vb          │
│  [7] Notifications.vb ─────┼─┐ │  ObsWebSocketClient/EventMap │
│   writes config.json       │ │ │  Units: Notifier/2/3         │
└────────────────────────────┘ │ │   + riders Notifier_Sub/2/3  │
        config.json (Notifications.*) │   + shadows Shadow/2/3  │
        notifier_obs.json ────────────┘ (2s hot-reload watcher) │
                                    └──────────────────────────────┘
```

- **Notifier project**: `Notifier/NVIDIA Notifier.vbproj` — `WinExe`, root
  namespace `Notifier_API`, `net10.0-windows10.0.26100.0`, WinForms +
  Newtonsoft.Json 13.0.4. Links four shared sources from `../Common/`:
  `AppLayout.vb`, `AppLayoutStartup.vb`, `AppConfigShared.vb`, `ObsConfig.vb`.
- **Shared config**: `Common/AppConfigShared.vb` header names the three external
  consumers (Launcher writes `Overlay.UseOverlayEnabled`; API hub reads it every
  second to keep-alive/kill the overlay stack — Notifier/ShadowPlay/Capture
  included; Notifier reads `UI.Language`).
- **Engine-side "Slot" is a different concept**: `Engine/Engine/[Integration]/
  OverlayConfig.vb:58` defines `VideoMyPresetSlot` (low/medium/high video preset
  slots). It has nothing to do with toast slots. Do not confuse the two when
  searching for "Slot".

## 2. Toast pipeline end-to-end

### 2.1 TCP path (primary)

1. **Send** — Overlay's single toast entry point is `ShowNotifier(message)` at
   `[1] Main Menu.vb:870-884`: `tcp.Send("l10n." & message)` (line 872). The
   trigger-file write below it is **commented out** (875-883) — see F-5.
   Callers: `Sub_Record.vb` (11 sites: 148, 159, 171, 192, 225, 242, 258, 278,
   297, 330, 334), `Sub_Mouse.vb` (9 sites: 116, 223, 273, 424, 482, 606, 655,
   665, 710), `Sub_Hotkey.vb` (4 sites: 45, 81, 93, 142), `Sub_Misc.vb`
   (189, 201), `Main Menu.vb` (417, 419, 824, 846, 859, 1078, 1099),
   `[0] Settings - UI Main.vb:146`, `SnipWithWindows.vb:40`,
   `UpdateHelper.vb` (32, 35, 42), `[Overlay] Client.vb` (234, 265),
   `SystemMonitor.vb` (91, 104, 112, 133, 167).
2. **Wire format** — `TcpClientHelper.Send` builds `[Send] {appName}|{cmd}`
   (`TcpClientHelper.vb:77-96`); the Overlay client is created with appName
   `"NVIDIA Overlay"` (`[Overlay] Client.vb:10`), the Notifier with
   `"NVIDIA Notifier"` (`Loader.vb:39`).
3. **Hub relay** — `API/[Forms - Project Files]/[API]/Server.vb` listens on
   port 5000 (`:134-202`; IPv6Any dual-stack with IPv4 fallback + 12×5 s bind
   retries, loopback-constrained by remote-endpoint posture, 32-client cap
   `:216-222`, 64 KB line cap `:268-273`). `Broadcast` (`:354-389`) relays every
   line to all clients **except the sender**. ping/pong (`:316-327`, C2 lock
   fix) and 60 s heartbeat reaper (`:394-429`) keep the mesh alive.
4. **Receive** — Notifier's `ListenLoop` (`TcpClientHelper.vb:117-137`) filters
   its own `[System]|pong` and raises `OnMessageReceived`. Auto-reconnect:
   1 s→30 s exponential backoff with M7 resource-dispose and M12 first-delay
   fixes (`:159-210`).

### 2.2 OBS bridge path (secondary, Notifier-local)

- **Config**: `Common/ObsConfig.vb` — `Config/notifier_obs.json`
  (`enabled/host/port/password` + `forward` filter block; schema mirrors the
  shipped `Notifier/notifier_obs.json`). Save is read-modify-write against the
  current file so unknown keys survive (`:65-92`); hot-reload detects changes by
  `LastWriteTimeUtc` (`:94-102`).
- **Watcher**: `Loader.vb:57-94` polls every 2 s (`ObsConfigPollMs`), starts /
  stops / re-endpoints the bridge live (`UpdateEndpoint`, `ObsWebSocketClient.vb:63-79`).
- **Client**: `ObsWebSocketClient.vb` — OBS WebSocket v5, 8 KB receive buffer,
  auto-reconnect with 1 s→30 s backoff, request/response correlation.
- **Event map**: `ObsEventMap.vb:20-93` maps `RecordStateChanged`
  (STARTED→`l10n.recording_started` green, STOPPED→`l10n.recording_saved`),
  `ReplayBufferStateChanged` (→`instant_replay_on`/`instant_replay_off`),
  `ReplayBufferSaved` (→`l10n.notificationInstantReplaySaved`, green),
  `ScreenshotSaved` (→`l10n.notificationScreenshotSavedToGallery`, green).
- **Entry**: `OnObsEvent` (`Loader.vb:112-140`) applies the per-type
  `ShouldForward` filter (ObsConfig), a global 300 ms toast throttle
  (`ShouldShowToast`, `:142-153`), then feeds the **same** `OnMessage` /
  `OnMessageWithArgs` as TCP. `ReplayBufferSaved` runs ffprobe on
  `savedReplayPath` for the real duration first (10 candidate paths,
  `ReadVideoDurationSeconds`, `:220-286`).

### 2.3 Display choke point

`OnMessage` (`[Notifier] Client.vb:91-148`) — every toast from both paths:
1. `LoadLanguage()` (re-reads the language JSON — F-7).
2. Parse `[...]|key` → registry lookup `notifications.FirstOrDefault(...)` —
   **unknown key → `Exit Sub` (fail-closed, Debug.WriteLine only, :112-115)**.
   This is where F-1/F-2/F-3 die.
3. Special case `notificationInstantReplaySaved` / `saved_last_15`: read the
   replay duration from `Data/NVIDIA_Shadowplay_Data/Replay/*.m|*.s`
   (`GetSavedReplayDuration`, `Loader.vb:955-974`) then `DeleteReplayFiles`
   (`:991-1004`) — cleanup runs even if the toast is later suppressed.
4. Legacy trigger-file cleanup `SafeDelete(Data/NVIDIA_Shadowplay_Data/<key>)`
   (`:134`, `:977-988`) — no-op today, see F-5.
5. **Gate**: `NotificationAllowed(key)` (`:198-202`) →
   `AppConfigShared.ReadBool("Notifications", category, True)`. Category mapping
   `NotificationCategory` (`:205-273`) covers 26 categories; **unmapped keys are
   fail-open** (always shown) by design, so a future key can never be silently
   swallowed by a stale mapping.
6. `ManageNotifierState()` (`Loader.vb:1007-1029`) — fossil file juggling, F-5.
7. `UpdateNotifier(...)` with the toast's GROUP → Slot routing (§3).
8. `tcp.SendLog(message)` (`[Notifier] Client.vb:147`) → `[Receive] NVIDIA
   Notifier|<text>` back through the hub (logging channel).

Registry: `InitNotifications` (`:32-89`) holds ~40 entries — legacy
`l10n.notificationXxx` keys plus snake_case keys aliased via
`localizationKey:=` (e.g. `l10n.recording_started` →
`l10n.notificationManualRecordStarted`). `LangHelper.GetText` returns the raw
key when a language file entry is missing (`LangHelper.vb:28-34`), and
substitutes `{{argN}}` placeholders (`:36-42`).

## 3. The Slot system (T29.2 / T29.3 / T30 RaiseStack) — as built

### 3.1 Config surface

- Canonical key: **`config.json → Notifications.SlotCount`** (int, 1–3,
  default 2) — `AppSettings.vb:227-233` (typed model, Overlay) and read on the
  Notifier side via `ConfiguredSlotCount()` (`Loader.vb:333-338`) with clamp
  1..3. **Re-read on every toast**, so Settings changes apply live, no restart.
- UI: Settings → Notifications "Use a second / third toast slot" toggles
  (`[7] Notifications.vb:359-403`), encoding OFF/OFF=1, 2ND=2, 2ND+3RD=3 with
  dependency enforcement (turning on 3 forces 2; killing 2 kills 3,
  `SyncToastSlotCount` `:388-395`).

### 3.2 Group model (T29.2)

`NotificationGroup` (`[Notifier] Client.vb:288-300`): toasts route by GROUP, not
raw key — a start/stop pair of one feature shares one group so its toasts update
each other in place instead of stacking:

| Group | Member categories |
|-------|-------------------|
| `recording` | RecordingStarted, RecordingSaved, RecordingError |
| `replay` | InstantReplayOn, InstantReplayOff, ReplayTurnOn, ReplayError, ReplaySaved |
| `<category>` | every other mapped key (own group = its config category) |
| `<raw key>` | unknown keys (group with itself, future-proof) |

### 3.3 Routing decision tree (`UpdateNotifier`, Loader.vb:511-616)

Pre-step: `UpdateSlotLiveness()` reconciles bookkeeping with live forms before
any decision (`:519-522`) — a closed slot leaves the stack order immediately,
never one heartbeat later.

1. **Group steadily showing on side slot** (slotCount ≥ 2/3, `green_stop`
   visible, same group) → Updater-UI dance **in that slot** (`:526-537`).
2. **NEW group while main busy** → first FREE configured side slot gets a fresh
   show (`AssignUnitY` bottom-of-stack rank + `RegisterActiveSlot`,
   `:552-564`); **every configured slot busy → T30 FIFO queue**
   (`EnqueueToast`, `:565-566`) — never dropped, never steals a slot.
3. **Group live on side slot but mid-flight**: mid-DANCE → dance on this slot
   (T30.4 "key repeat stays where it lives"); **mid-EXIT → queue, never paint a
   corpse** (T29.4, `:569-596`).
4. **Main slot**: mid-exit → queue; mid-DANCE → coalesce (ShowOnMain's
   BeginDance returns False → paint rider, latest wins); otherwise fresh show /
   replace dance (`:598-616`). Covers 1-slot mode entirely.

Queue (`:722-901`): FIFO `Queue(Of PendingToast)`, cap `MaxPendingToasts = 8`,
same-group dedup refreshes the pending copy in place, overflow drops the OLDEST
and logs. Drain: `TryDequeueIntoFreeSlot` runs each heartbeat, targets the
first idle configured slot (1→2→3), only when the unit is fully idle
(`SlotIsIdle` = not alive AND not `InTransition`, `:801-809`).

### 3.4 Stack manager — T30 RaiseStack (100 ms heartbeat, `Loader.vb:696-947`)

- `OnStackHeartbeat` (`:769-787`), wrapped in Try/Catch ("never let the
  heartbeat kill the app"): liveness → compact → dequeue → every 20th tick
  (~2 s) `RaiseVisibleStack`.
- `CompactStack` (`:857-877`): every active unit glides to
  `StackBaseY() + rank * StackPitchPx (100px)` over 250 ms; idempotent, skipped
  mid-dance/mid-exit, self-heals drift every tick.
- `StackBaseY` (`:750-758`): 205 if the `notifier_main` sentinel file exists,
  else 105 — **cached 2 s** (T30.6: the probe used to run per tick; FileExists
  latency stole UI time mid-reflow). See F-5: the sentinel is never written.
- `RaiseVisibleStack` → per-unit `RaiseUnit()` (`[2] Background.vb:671-717`):
  `BeginDeferWindowPos` atomic chain Shadow < BG < Rider onto HWND_TOPMOST with
  SWP_NOACTIVATE — topmost without focus steal (T30.1/T30.7).
- Explosion-proofing (OWNER "กัน Users ระเบิด แอป") as implemented: idempotent
  heartbeat, Try/Catch envelope, "never animate a corpse" guards in the
  animation engine, bounded queue with oldest-drop, T29.4 mid-transition guard.

### 3.5 Unit mechanics (the form trio)

`Notifier` = `[2] Background.vb` (836 lines); `Notifier2` = `[2] Background
2.vb` and `Notifier3` = `[2] Background 3.vb` are **830-line mirrors** (verified
member-by-member: `UnitTargetY`, `StartSlide/StartSlideY`, `BeginDance/
EndDance`, `InTransition/IsDancing`, `FadeInShadow`, `ReflowTo`, `RaiseUnit`).
Riders `Notifier_Sub/2/3` = `[1] String + Icon + Logo*.vb` (each exposes
`Reveal(bgLeft, bgTop, cardPanelLeft)` — `:84` / `:95` / `:95`); shadows =
`[3] Shadow*.vb`.

- **No-activate guarantee**: `WS_EX_NOACTIVATE` + `ShowWithoutActivation=True`
  (`[2] Background.vb:22-37`); z-order handled exclusively by `RaiseUnit`
  (T30.1 — no `TopMost` setter anywhere, it can activate).
- **Animation engine** (`:59-278`): single UI-thread frame pump (1 ms timer,
  6 ms frame gate), stopwatch-based cubic ease-out, per-control animation
  replace, disposed-target guard (T30.2/T30.5/T30.6 history in comments).
  `UnitClockRes` (`:810-836`) refcounts `timeBeginPeriod(1)` process-globally so
  one unit going idle can't degrade another's 1 ms ticks.
- **Single-clock rule** (T30.3 OWNER): X slides — only the CARD moves; the rider
  is a still window revealed when the card parks; the shadow lives only while
  steady. Y reflow — the FORM leads and carries rider + shadow in the same
  atomic `DeferWindowPos` batch (`ApplyUnitY`, `:541-554`).
- **Lifecycle**: fresh show = `Show()` → `Form1_Load` dance (`:312-382`: card
  slide-in, then black card, then `green_stop` + rider `Reveal` + shadow fade;
  `autoClose` set to 6000 ms by Form_Load — callers must NOT start it earlier,
  documented at `Loader.vb:421-426`). Replace dance = `BeginDance` (hides rider,
  kills shadow, T30.7 Hide-never-Close so the rider is reborn by `Reveal`)
  → card out 600 ms → swap content at turnaround → card in 300 ms → `EndDance`
  + `FadeInShadow`. Close = `SlideOutAll` (`:728-766`): shadow gone instantly,
  rider hidden, card out, then rider Close + unit Close.
- **Position**: `UnitTargetY` (rank-assigned, consumed once by Form_Load,
  `:15`, `:316-331`) or legacy 105/205 by sentinel.
- **Reflow re-entry guard** (T30.6): `_activeReflowTarget` prevents
  CompactStack from restarting an in-flight glide every 100 ms (`:629-646`).

### 3.6 Failure-mode notes (verified, not defects)

- Suppressed-by-gate toasts still clean their trigger files and replay files
  (cleanup precedes the gate, §2.3 step 3-4) — deliberate per the comment at
  `[Notifier] Client.vb:136-138`.
- A toast arriving while ALL slots are mid-exit is queued, then surfaced by the
  heartbeat within ~100 ms of a slot going idle — no silent drop.
- The 300 ms OBS throttle (`ToastThrottleMs`) is OBS-path-only; the TCP path has
  no time throttle but is absorbed by group dances + the bounded queue (F-8).

---

## 4. Settings surface — `[7] Notifications.vb` (Overlay)

`Base_Notifications` (`Overlay/.../[Settings]/[7] Notifications.vb`, 509 lines):

- **27 per-category toggles** (`LoadNotificationToggles` `:59-98`, handlers
  `:180-347`): each `ValueChanged` writes `AppSettings.Instance.Notifications.<X>`
  then `AppSettings.Instance.Save()` immediately. The category set is **1:1 with
  `NotificationCategory` on the Notifier side** (26 mapped categories + the
  recording trio split = 27 toggles; every toggle name exists in both places —
  the reverse is what fails, see F-1/2/3).
- **Enable/Disable all** (`SetAllNotifications` `:103-178`) flips every switch
  under the `_notiLoading` gate (no per-toggle save mid-loop) then saves once.
- **Toast slots** (moved here from General, `:359-403` — see §3.1).
- **OBS editor** (`:405-472`): this page only edits `notifier_obs.json` via the
  shared `ObsConfig` (Enabled toggle, HOST/PORT/KEY boxes with Leave-save and
  1–65535 port validation); the Notifier owns the actual connection and picks
  changes up within ~2 s. `LoadObsSettings` also carries the fix for the
  first-show NRE that used to block the whole Settings window (`:418-427`).
- **Caret-free boxes** (`:474-499`): cosmetic `HideCaret` plumbing only —
  editing, selection and Leave-save untouched.
- Model: `AppSettings.NotificationsSettingsClass` (`AppSettings.vb:192-234`) —
  all 27 booleans default `True`; `SlotCount` default 2.

The Notifier reads these keys through `AppConfigShared.ReadBool`
(`Common/AppConfigShared.vb:49-69`) — case-insensitive lookup (`:195-202`),
missing file/section/key/type → fallback `True` (fail-open), never creates the
file. Writes elsewhere use the per-PID tmp + `.bak` atomic swap (`:128-169`).

## 5. SystemMonitor → notifier keys (state after `6bd63e5`)

`Overlay/[API]/[Services]/SystemMonitor.vb`, wired at `[1] Main Menu.vb:458-467`
(500 ms after menu load; the commit message "Enable SystemMonitor" refers to
un-commenting this call — the class was previously dead code):

- **RAM bands** (`CheckRam` `:76-125`, fixed branch order): 100% →
  `ramwramcritical` once + every 10 s; 95–99% → **`ramwram95`** every 10 s (was
  `ramwramcritical` — severity overshoot that also made `l10n.ramwram95` a dead
  key); 80–94% → `ramwram` warn-once per band entry; <80% resets all latches.
- **CPU** (`:128-139`): ≥95% → `cpuwram` warn-once.
- **Disk** (`:142-185`): gated on `Base.RecordValue` (only while recording),
  `<10 GB` → `diskspacelow` every 10 s. `6bd63e5` fixes: folder paths are mapped
  to their drive root before `DriveInfo` (a plain `D:\Videos` used to throw per
  tick and be swallowed silently); the broken-path branch now logs **once** per
  entry into the failed state via `tcp.Send("[SystemMonitor] disk check
  skipped: ...")` (`:173-183`).
- All alerts funnel through `Base.ShowNotifier(...)` — i.e. the same registry +
  gate + slot system as everything else. `RamWarning95`/`RamCritical` toggles
  are live for these keys.

## 6. Findings

### F-1 🔴 `l10n.recording_error` never displays

- **FACT**: Overlay sends it from 3 sites — `[Overlay] Client.vb:234` and `:265`
  (engine error / engine_record_start failed), `[1] Sub_Record.vb:192`. The
  Notifier registry (`[Notifier] Client.vb:32-89`) contains
  `l10n.recording_started` (:79) and `l10n.recording_saved` (:80) but **no
  `l10n.recording_error` entry** (grep-verified). `OnMessage` drops unknown keys
  at `:112-115` — before `NotificationCategory` (which *does* map
  `recording_error → RecordingError`, `:214-215`) or the Settings toggle can act.
- **IMPACT**: the user never sees a recording-failure toast; the Overlay UI
  correctly reverts to idle, so the failure is only visible in logs. The
  `RecordingError` toggle on [7] Notifications is dead for the TCP path, and
  group `recording` never receives its error member.
- **RECOMMENDATION**: add
  `notifications.Add(New NotificationData("l10n.recording_error", "", False, <red>, localizationKey:="l10n.notificationErrorGeneral"))`
  (or a dedicated `l10n.notificationRecordingError` localization key — new
  string needed) next to the other snake_case entries. One-line code change;
  left to the OWNER/code owners.

### F-2 🔴 `l10n.extension_not_found` never displays

- **FACT**: sent at `[1] Sub_Mouse.vb:273`; absent from registry (only the
  category mapping exists, `[Notifier] Client.vb:256-257`).
- **IMPACT**: mic/extension-missing feedback silently lost; `ExtensionNotFound`
  toggle dead.
- **RECOMMENDATION**: same one-line registry fix, aliased to a suitable
  existing localization key until a dedicated string exists.

### F-3 🔴 `l10n.notificationErrorEngineUIInUse` never displays

- **FACT**: sent at `[1] Sub_Mouse.vb:665`; the registry registers
  `l10n.notificationErrorEngineNotRunning` (`:83`) but not
  `...EngineUIInUse`; only the category mapping exists (`:264-265`).
- **IMPACT**: the engine-busy feedback path is silent; `EngineUIInUse` toggle
  dead.
- **RECOMMENDATION**: same registry fix. Note the pattern: **all three defects
  are registry gaps, not gate gaps** — the fail-open design of
  `NotificationAllowed` never gets the chance to apply because the registry is
  fail-closed. If OWNER wants "unknown key always shows" semantics end-to-end,
  the registry itself must become fail-open (fallback NotificationData with the
  raw key) — that is a semantic change, OWNER DECISION REQUIRED.

### F-4 🟡 Production test controls

- **FACT**: `Loader.Designer.vb` places real buttons (Button1/2/3,
  `:25-59`) on the transparent Loader form; handlers at `Loader.vb:1032-1045`
  fire a real toast through `UpdateNotifier`, pop the raw rider, or start the
  IF_N slide. Additionally **any click on a visible toast card** restarts the
  whole Notifier process (`[2] Background.vb:772-780` →
  `Application.Restart()`; mirrors in 2/3).
- **IMPACT**: users can trigger demo toasts; clicking a toast kills and respawns
  the notifier (state loss is benign but the intent is unclear — Alt+Z hint
  toast becomes click-to-restart).
- **RECOMMENDATION**: gate buttons behind a Dev flag (`Flags/Dev` pattern
  already exists, `Main Menu.vb:469-474`); make toast-click behavior explicit
  (no-op vs restart). OWNER DECISION REQUIRED.

### F-5 🟡 Sentinel / trigger-file fossils

- **FACT**:
  - `notifier_main` — the only writer is **commented out**
    (`[1] Sub_Misc.vb:53-66`), yet 9 code paths read it: stack base
    (`Loader.vb:756`), unit Form_Load Y (`[2] Background.vb:325`, `2:340`,
    `3:340`), IF_N stop condition (`Background.vb:792`, `2:822`, `3:822`),
    shadow base (`[3] Shadow.vb:53`, `2:65`, `3:65`).
  - `notifier` — created/deleted by `ManageNotifierState`
    (`Loader.vb:1009-1018`) but **no reader exists** (grep-verified).
  - `notifiermainoff` — deleted by `ManageNotifierState` (`:1010, :1020-1027`)
    but **no writer exists** → the `Notifier.IF_N.Start()` there is dead.
  - Per-key trigger files `Data/NVIDIA_Shadowplay_Data/<key>` — deleted by
    `SafeDelete` (`[Notifier] Client.vb:134`) but no writer since the Overlay's
    `ShowNotifier` file-write was commented out (`Main Menu.vb:875-883`).
- **IMPACT**: `StackBaseY()` is pinned to 105 (2 s-cached probe of a file that
  can never exist) and every IF_N loop self-stops on its first tick; the
  Notifier performs per-toast filesystem writes/deletes that are no-ops. No
  user-visible malfunction — but every future reader of these sentinels inherits
  a lie.
- **RECOMMENDATION**: either resurrect the writers (if the 205 offset + notifier
  state handshake are still wanted) or delete the readers + `ManageNotifierState`
  file juggling in one cleanup commit. OWNER DECISION REQUIRED.

### F-6 🟡 Dead registry entries (harmless)

`l10n.irOn`, `notificationWarningGameRequired`,
`notificationWarningPhotographyNotAllowed`, `notificationCustomOverlayFileNotFound`,
`notifierOpen`, `notifierNotUsing`, `notificationAppClosed`,
`notificationSharedClose`, `foldererror`, `Capture_notuse`, `openLocation`,
`privacy`, plus `l10n.test` / `l10n.testarg` (test keys). Registered but never
sent by current code (grep-verified). They cost nothing but obscure the real
surface; prune opportunistically.

### F-7 🟢 Language file re-read per toast

`OnMessage` → `LoadLanguage()` → `LangHelper.LoadLang` re-reads and re-parses
the language JSON on **every** toast (`[Notifier] Client.vb:99`, `Loader.vb:292-299`).
Toast rates are low (throttled/coalesced), so this is fine in practice; if toasts
ever get chatty, cache with an mtime check — same pattern as `StackBaseY`'s 2 s
TTL (T30.6).

### F-8 🟢 Throttle asymmetry (informational)

`ShouldShowToast` (300 ms global) is applied **only** on the OBS path
(`Loader.vb:118`, `:126`). TCP-path bursts rely on T29.4 coalescing + the capped
queue instead. Behavior is correct (no overlap, no unbounded growth); noting it
so future tuning targets the right path.

### Positive verifications (no action)

- SlotCount clamped 1..3 on **both** ends (UI dependency logic + `ConfiguredSlotCount`).
- Queue is bounded (8) with oldest-drop — memory stays flat under spam.
- Rider/shadow are hidden-never-closed (T30.7) — the old ObjectDisposedException
  zombie/shadow-blink failure mode is structurally gone.
- `timeBeginPeriod` refcount prevents cross-unit tick degradation.
- Hub has real hardening: dual-stack bind + retry, client/line caps, locked
  ping/pong, 60 s reaper, broadcast-dead-client cleanup.
- `AppConfigShared` never rewrites from a cached model; atomic swap + `.bak`.

## 7. Contract cross-references (for CONFIG_RUNTIME_CONTRACT.md)

The Notifier is a **runtime boundary consumer** of `config.json` in the sense of
§contract v2.0 — it reads through `AppConfigShared` without owning the schema:

| Key | Writer | Reader | Boundary behavior |
|-----|--------|--------|-------------------|
| `Notifications.*` (27 toggles) | Overlay `[7] Notifications.vb` → `AppSettings.Save()` | Notifier `NotificationAllowed` per toast | live, fail-open (`True`) |
| `Notifications.SlotCount` | same | Notifier `ConfiguredSlotCount()` per toast | live, clamp 1..3, default 2 |
| `UI.Language` | Overlay settings | Notifier `LoadLanguage` per toast | live, fallback `en-US` |
| `notifier_obs.json` (file, Notifier-owned schema) | Overlay OBS editor page | Notifier watcher (2 s) | hot-reload, keep-previous on parse fail |

New invariant worth locking (I-N1, proposal): **`config.json → Notifications.*`
is the single suppression authority for toasts; the Notifier display choke point
is the only place that reads it.** Overlay-side senders do NOT pre-check the
toggles (verified — senders fire unconditionally), which keeps writer/reader
roles clean. Keep it that way.

## 8. Explicitly not decided here

Per the standing rule (no decisions on the OWNER's behalf): fixing F-1/F-2/F-3
(and whether to also make the registry fail-open), removing vs gating the test
buttons, toast-click behavior, and the F-5 fossil cleanup are all presented as
FACT / IMPACT / RECOMMENDATION only. The three HIGH findings are one-line
registry additions with no schema or contract impact — but they are code changes,
so they belong to a code-owning agent, not this doc checkpoint.
