# TcpClientHelper CONSOLIDATION DESIGN — one shared copy in `Common\`

- Route: **M3/W2 — TcpClientHelper consolidation design** (design-only; no production file is touched by this document)
- Companion input: M3/W2 static protocol audit (findings: 4 diverged copies, doc↔code drift on port/heartbeat/backoff)
- Owner: ScotcsDuluka · drafted by GLM/5.3-Flash (static analysis pass, 2026-09-12)
- Status: **DESIGN — awaiting owner sign-off. Contains no code changes.**

---

## 0. The design in one line

> **One canonical `Common\TcpClientHelper.vb`, linked (not copied) into the 4 client projects, semantics = the strictest union of the 4 today's copies, plus an `OnConnected` event that makes first-connect announcements deterministic — so the next M12/M7-class fix lands ONCE.**

This follows the pattern `Common\LoopbackGate.vb` already proved in this repo: *"as REAL production code shared by the Hub and the boundary regression tests (same source, zero copy drift)"* (`Common/LoopbackGate.vb:5-7`).

---

## 1. Evidence base (facts this design is built on, all re-traced 2026-09-12)

### 1.1 The four copies have drifted into four different fix-sets

| | Engine
`Engine\Engine\[API]\TcpClientHelper.vb` | Launcher
`Launcher\[Forms - Project Files]\[API]\TCP\TcpClientHelper.vb` | Overlay
`Overlay\[Forms Overlay - Project Files]\[API]\[Services]\TCP\TcpClientHelper.vb` | Notifier
`Notifier\[Forms Overlay - Project Files]\[API]\TCP\TcpClientHelper.vb` |
|---|---|---|---|---|
| size | 8,514 B | 10,282 B | 8,837 B | 7,454 B |
| reconnect gate (`_reconnectGate` CAS) | ❌ | ✅ | ✅ | ❌ |
| generation guard (`_generation`) | ❌ | ✅ | ✅ | ❌ |
| M12 backoff (sleep-first, first retry = 1s) | ✅ (:166-168) | ❌ (doubles first → 2s, :224-226) | ❌ (doubles first → 2s, :214-216) | ✅ (:164-168) |
| M7 dispose old cts/writer/reader | ✅ (:170-186) | ❌ | ❌ | ✅ (:170-183) |
| `OnReconnected` event | ✅ (v9 evidence fix) | ❌ | ✅ (L1) | ❌ |
| `ConnectAsync()` | ❌ | ✅ | ✅ | ❌ |
| ctor connects synchronously | ✅ (blocks UI thread) | ❌ | ❌ | ✅ (blocks UI thread) |
| publishes writer/reader under `_writeLock` | ❌ (swaps unlocked, :59-60) | ✅ | ✅ | ❌ (swaps unlocked) |
| dead field `_reconnectIntervalMs` | ✅ dead | ✅ dead | ✅ dead | ✅ dead |

Consequences already latent:
- Engine/Notifier copies can double-spawn `ReconnectLoop` and fight over `_client/_writer/_reader` — the exact failure the Launcher copy's own comment documents (`[APP] Client.vb`-adjacent `TcpClientHelper.vb:15-17`: *"without a gate two loops can fight … end up double-connecting"*).
- The first reconnect on Launcher/Overlay waits 2s, contradicting Engine's M12 comment (:165-167 calls the doubled-first behavior a bug) and the DULUKA contract's citation of "TcpClientHelper proven cadence — starts 1 s" (`docs/DULUKA-V0.1-IMPLEMENTATION-CONTRACT.md:317`).
- `phase-12-source-audit.md:116` still records *"identical 6198-byte copies"* — stale in both count and drift.

### 1.2 Wiring constraints discovered (they shape the migration)

| Fact | Evidence | Design impact |
|---|---|---|
| All 5 app vbprojs are SDK-style with **default compile globbing** (no `EnableDefaultCompileItems` override, no explicit `Compile Include` for the local copies) | absence checks across `NVIDIA Capture.vbproj`, `Launcher.vbproj`, `NVIDIA Overlay.vbproj`, `NVIDIA Notifier.vbproj` | A local copy **must be deleted**, not just unlinked — adding `..\Common\TcpClientHelper.vb` while the local file remains compiles the class twice (BC30179) |
| Common-file linking is the established pattern; Notifier/Launcher/Overlay/API all link `..\Common\*.vb` today | vbproj `Compile Include="..\Common\..."` lines (AppLayout/AppConfigShared/etc.) | `Common\TcpClientHelper.vb` needs one `<Compile Include>` line per client project — nothing else |
| Engine already tolerates a **not-yet-connected** helper: `StartHubClient` fires `BroadcastEngineReady()` from a one-shot 500ms timer with a comment admitting "if not connected yet … broadcast on the next successful register attempt" | `Engine\Engine\[UI]\UI_Engine.vb:164-180` | ctor-connect is **not load-bearing** for the Engine's first `engine_ready`; an `OnConnected` event replaces the timer deterministically |
| Notifier wires only `OnMessageReceived` right after ctor-connect | `Notifier\...\Loader.vb:38-40` | migration = add `tcp.ConnectAsync()` after `AddHandler`; no other Notifier change |
| No Tester test scans any `TcpClientHelper.vb` source (L1ReconnectTests / W2OverlayHonestyTests scan the *client* files only) | grep across `Tester\test\**` | moving the helper breaks no existing test |
| `Overlay\ManualOverlayTest.vbproj` includes no TCP sources (links only `AppLayout.vb`, explicitly removes `AppLayoutStartup.vb`) | `ManualOverlayTest.vbproj:417-423` | unaffected |
| Wire format on the hub side is untouched by this work | `API\...\Server.vb` parses `[Send] <app>\|<cmd>[:<value>]` | zero hub changes |

---

## 2. Canonical contract (`Common\TcpClientHelper.vb`)

### 2.1 Public surface (superset; same class name, no namespace — drop-in)

```vb
Public Class TcpClientHelper : Implements IDisposable

    ' ── construction ──
    Public Sub New(appName As String,
                   Optional host As String = "127.0.0.1",
                   Optional port As Integer = 5001,   ' NVIDIA API hub (Server.vb ApiPort = 5001 —
                                                      ' Duluka.Server owns 5000). The ctor default is
                                                      ' the client-side authority (Server.vb:13-14).
                   Optional autoReconnect As Boolean = True)
    ' ctor does NOT connect. Callers: construct → AddHandler(s) → ConnectAsync().

    Public Sub ConnectAsync()      ' background Connect(), generation-guarded (Launcher/Overlay shape)
    Public Sub Connect()           ' kept public for manual use; no caller in-tree after migration
    Public Sub Disconnect()
    Public Sub Dispose()

    ' ── messaging ──
    Public Sub Send(cmd As String, Optional value As String = "")      ' [Send] <appName>|<cmd>[:<value>]
    Public Sub SendLog(message As String)                              ' [Receive] <appName>|<message>
    Public ReadOnly Property IsConnected As Boolean

    ' ── events (fire on socket/worker threads — consumers MUST marshal to UI) ──
    Public Event OnMessageReceived(msg As String)
    Public Event OnDisconnected()
    Public Event OnReconnecting()      ' once per ReconnectLoop start (gated, not per attempt)
    Public Event OnConnected()         ' NEW semantic — see §2.2 (replaces OnReconnected)
End Class
```

### 2.2 Semantic decisions (locked here, flagged where owner input is cheap)

**D1 — `OnReconnected` → `OnConnected` (RECOMMENDED).**
Today's `OnReconnected` (Engine v9 / Overlay L1) fires only after a *re*-connect; first-connect announcement is patched around with a 500ms timer (Engine, `UI_Engine.vb:170-179`) or a bounded poll (Overlay, `StartBoundedStatusPull`). That split is exactly what produced "an engine that started BEFORE the hub stayed silently connected forever" — the bug the v9 fix itself documents.

Canonical: **`OnConnected` fires on EVERY successful connection — initial and reconnect.**
- Engine: `AddHandler tcp.OnConnected, AddressOf OnTcpConnected` where `OnTcpConnected` = today's `OnTcpReconnected` body (`BroadcastEngineReady` via `BeginUiInvoke`). The 500ms timer in `StartHubClient` is deleted (D2).
- Overlay: same rename; body (`_engineStatusPulled = False` + one `engine_get_status`) is correct for initial connect too — it duplicates at most one send against `StartBoundedStatusPull`'s first tick, which is idempotent.
- Notifier/Launcher: don't wire it; unaffected.

*Alternative (lower churn, not recommended):* keep the name `OnReconnected`, change semantics to "every successful connection". Rejected: the name would lie on the initial connect — this repo's own standard forbids state/naming that must be mentally re-interpreted ("UI guessed state is forbidden", `docs/CONFIG_RUNTIME_CONTRACT.md:29`).

**D2 — remove Engine's 500ms one-shot timer (RECOMMENDED).**
With D1 the timer is redundant and strictly worse: it fires once, too early when the hub is down, and never again. `OnConnected` covers both cases deterministically.

**D3 — behavioral loopback test now vs shape-test only (RECOMMENDED: both, behavioral test may slip a milestone).**
See §5.

### 2.3 Behavior = strictest union of today's four copies

| Mechanism | Adopted from | Notes |
|---|---|---|
| Single-flight reconnect gate (CAS `_reconnectGate` 0→1) | Launcher/Overlay | `Connect()` failure AND `ListenLoop` exit both try to spawn the loop; late arrivals return |
| Connection generation (`_generation`) on Connect/Disconnect/swap | Launcher/Overlay | stale Listen/Ping/Reconnect loops exit silently; only the current generation may mark disconnection or respawn |
| Publish/swap `_writer`/`_reader` under `_writeLock`; Send/SendLog/Ping write under the lock with null guard | Launcher/Overlay | Engine/Notifier copies swap unlocked — removed |
| M12 backoff: `Thread.Sleep(delay)` FIRST, then `delay = Min(delay*2, 30000)`; reset to 1000 on success | Engine/Notifier | makes the DULUKA contract's "starts 1 s" claim true for all consumers; Launcher/Overlay first retry 2s → 1s |
| M7: dispose old `_cts/_writer/_reader/_client` before replacing in ReconnectLoop | Engine/Notifier | stops CTS/Stream handle leaks across reconnect cycles |
| `ConnectAsync()` generation capture (`If gen = _generation Then Connect()`) | Launcher/Overlay | ctor no longer connects; UI thread never blocks in `TcpClient.Connect` |
| Ping every 10s; `[System]|pong` filtered in ListenLoop | all (identical) | unchanged |
| Wire frames `[Send] <appName>\|<cmd>[:<value>]` / `[Receive] <appName>\|<msg>` | all (identical) | hub parses unchanged |
| Defaults: host `127.0.0.1`, port **5001**, autoReconnect `True` | Server.vb:13-15 contract | all four copies already default 5001 — no consumer passes an explicit port |
| Drop `_reconnectIntervalMs` (dead in all copies) | — | — |

Constants to name (currently magic numbers): `PingIntervalMs = 10000`, `BackoffStartMs = 1000`, `BackoffMaxMs = 30000`, `DefaultHubPort = 5001`.

Threading contract to state in the file header: events fire on listener/reconnect threads; consumers marshal (Engine `BeginUiInvoke`, Overlay `BeginInvoke`, Notifier currently `Invoke` — Notifier's blocking marshal is a pre-existing item outside this design's scope).

Option Strict On-clean, dependency-free (BCL only) — same bar as `Common/AppLayout.vb` and `Common/LoopbackGate.vb`.

### 2.4 Known gaps intentionally NOT fixed here (kept out of scope)

1. The `|`-framing truncation bug (progress/status/PREWARM inner fields die at `parts(1)`) — separate M3/W2 work item; **sequence it AFTER this consolidation** so the fix lands once, in `Common\`.
2. Client-side read timeout / silent-dead-hub detection (client `ReadLine` blocks until TCP gives up; only the hub's 60s reaper and reconnect path recover). Noted as follow-up; changing it alters reconnect timing for all consumers — not a consolidation concern.
3. Notifier's blocking `Invoke` in `OnMessage`.
4. `ObsWebSocketClient` backoff (separate class, already 1s→30s, already request-correlated).

---

## 3. File-level change list (for the implementation milestone)

### 3.1 Add
- `Common\TcpClientHelper.vb` — canonical, per §2. Base = Overlay copy (most evolved), then graft M12 + M7 blocks verbatim from the Engine copy, apply D1/D2, drop dead field.

### 3.2 Delete (all four, in the SAME commit as 3.3 — atomic, else duplicate-class build break)
- `Engine\Engine\[API]\TcpClientHelper.vb`
- `Launcher\[Forms - Project Files]\[API]\TCP\TcpClientHelper.vb`
- `Overlay\[Forms Overlay - Project Files]\[API]\[Services]\TCP\TcpClientHelper.vb`
- `Notifier\[Forms Overlay - Project Files]\[API]\TCP\TcpClientHelper.vb`

### 3.3 Edit — one `<Compile Include>` line each
- `Engine\NVIDIA Capture.vbproj`
- `Launcher\Launcher.vbproj`
- `Overlay\NVIDIA Overlay.vbproj`
- `Notifier\NVIDIA Notifier.vbproj`
- NOT the API hub project (server side; it shares `LoopbackGate` already, has no client role).
- NOT `Overlay\ManualOverlayTest.vbproj` (verified: no TCP sources).

### 3.4 Edit — call sites (3 files)
| File | Change |
|---|---|
| `Engine\Engine\[API]\[Engine] Client.vb` | `StartTcpClient`: wire `OnConnected` → `OnTcpConnected` (renamed `OnTcpReconnected`, body unchanged); add `tcp.ConnectAsync()` at the end |
| `Engine\Engine\[UI]\UI_Engine.vb` | `StartHubClient` (:164-180): delete the 500ms timer block (D2), keep `StartTcpClient()` call |
| `Overlay\...\[Overlay] Client.vb` | rename handler `OnTcpReconnected` → `OnTcpConnected`; wire `OnConnected` instead of `OnReconnected` |
| `Notifier\...\Loader.vb` | after `AddHandler tcp.OnMessageReceived, ...` add `tcp.ConnectAsync()` |
| `Launcher\...\[APP] Client.vb` | no change (already construct → wire → `ConnectAsync()`) |

### 3.5 Doc updates (doc-only, same or follow-up commit)
- `docs/NOTIFIER_SLOT_AUDIT.md` §2.1: helper now shared from `Common\` (line anchors :77-96/:117-137/:159-210 become `Common/TcpClientHelper.vb:…`).
- `HANDOFF.md`: one line under validation state.
- `docs/phase-12-source-audit.md:116-120` + spec v2:644 "identical copies / consolidate" note: mark superseded-by pointer (these are anchored historical docs — add a one-line superseded note, don't rewrite).
- `Engine\Engine\[API]\[Engine] Client.vb:2-3` header comment "same shared TcpClientHelper" becomes literally true — no edit needed.

---

## 4. Behavior deltas per consumer (the "who feels what" list)

| Consumer | Delta | Risk assessment |
|---|---|---|
| Engine | connect becomes async; `engine_ready` announced via `OnConnected` (earlier and deterministic vs today's 500ms timer); gains gate+generation (kills the latent double-reconnect fight); loses nothing | LOW — the timer comment already designs for async connect; no code path requires `IsConnected` synchronously after ctor (`BroadcastEngineReady` self-checks) |
| Launcher | first reconnect 2s→1s; old sockets/CTS disposed; gains OnConnected availability (unused) | LOW |
| Overlay | first reconnect 2s→1s; M7 dispose; handler rename; one extra `engine_get_status` at initial connect (idempotent, deduped against bounded pull) | LOW |
| Notifier | connect becomes async (ctor no longer blocks UI thread); gains gate+generation | LOW — pre-existing race (toast broadcast arriving before `InitNotifications`) is unchanged in kind: listener already starts on `Task.Run` today before `InitNotifications` runs |
| Hub (API) | none | none — wire format and server untouched |

## 5. Validation plan

1. **Build tripwire:** `dotnet build "Overlay\NVIDIA Overlay.sln" -c Release` — duplicate-class (BC30179/BC30648) is the failure mode if any local copy survived; clean build proves §3.2/3.3 atomicity.
2. **Linux suites (repo pattern):** new shape tests below + existing `Engine.ConfigTruth.Tests` green.
3. **New `Tester\test\Engine\ConfigTruth\TcpClientHelperConsolidationTests.vb`** (source-shape scanning, `ExpectContains` style like L1ReconnectTests):
   - each of the 4 vbprojs contains exactly one `<Compile Include="..\Common\TcpClientHelper.vb"` and none contains its old local path;
   - the 4 old file paths do not exist on disk;
   - `Common\TcpClientHelper.vb` contains: `_reconnectGate` CAS, `_generation` compare in Listen/Ping loops, sleep-first backoff order, M7 dispose block, `OnConnected` raised at initial-connect AND reconnect sites, `port As Integer = 5001`, no `_reconnectIntervalMs`.
4. **Behavioral loopback test (D3, recommended; may follow one milestone):** in-process fake newline server on a random loopback port (Hub-Boundary precedent, `Tester\test\API\Hub-Boundary`): assert `OnConnected` fires exactly once on connect; `pong` line produces no `OnMessageReceived`; killing the server triggers `OnDisconnected` then `OnConnected` again within backoff bound; a second `ReconnectLoop` spawn does not double-connect (gate).
5. **OWNER Windows runtime pass** (APP-LAYOUT §OWNER verification protocol, plus HANDOFF §4 กฎเหล็ก single-instance rule before spawning apphosts): full tree up → hub client list shows all 4 names; kill hub → first reconnect observed at ~1s; hub restart → `engine_ready` re-broadcast + Overlay re-pull; one record start/stop → progress line moves (progress rendering itself stays broken until the framing fix — assert only that `engine_recording_progress` frames reach the hub log).

## 6. Commit slicing (repo's ordered-commit convention)

1. `Common\TcpClientHelper.vb` added (not yet referenced anywhere — inert).
2. Retarget 4 vbprojs + delete 4 local copies + 3 call-site edits — **one atomic commit** (any partial state breaks the build).
3. Shape tests + doc notes + (optional) behavioral test.

Net effect: −(237+270+265+217) + ~280 lines of helper code ≈ **−700 LOC** of permanently un-driftable surface, and every future protocol-client fix (including the framing fix) becomes a one-file change.
