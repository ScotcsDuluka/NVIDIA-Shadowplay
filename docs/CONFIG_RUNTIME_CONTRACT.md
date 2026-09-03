# CONFIG RUNTIME CONTRACT — NVIDIA ShadowPlay

- Route: **GLM/2 — CONFIG CONTRACT FINALIZATION v2.0** (doc-only; no production code touched, no schema change, no feature)
- Anchored at commit: **`6bd63e5`** — every `path · line` citation re-traced from real source at this commit by GLM/3. v1.0 was anchored at `cf47732`; the 30 commits `5b2a8a0..6bd63e5` changed cited files (`RecordingEngineHost.vb`, `RecordingEngine.vb`, `CaptureSession.vb`, `UI_Engine.vb`, `NextRecordingConfig.vb`, `AudioSettingsForm.vb`, `[5] Video Capture.vb`) and **landed new runtime facts this revision absorbs** (runtime rebuild seam, FPS authority resolution, rate-control normalization, strict CBR filler, PHASE 3 UI wave 1 writer removals; the final 3 commits `9e59fce`/`6db0341`/`6bd63e5` are a CaptureSession device-clock sync fix + SystemMonitor/Designer changes — only `CaptureSession.vb` line drift affects this doc, re-derived).
- Owner: ScotcsDuluka · v1.0 drafted by GLM/2 · v2.0 finalization pass by GLM/3 (every FACT re-verified, nothing re-decided)
- Method: `path · line` citations valid at the anchor commit. Missing things are reported `NO KEY` / `NOT IMPLEMENTED` — never invented. **No decision is made here**: §5 ships each open question as FACT → OPTIONS → IMPACT → RECOMMENDATION (non-binding) → **OWNER DECISION REQUIRED**.
- Companion docs (their roles are not re-decided here): `docs/CONFIG_OWNERSHIP_MATRIX.md` (ownership source of truth) · `docs/UI_CONFIG_ARCHITECTURE.md` v1.1 (UI consumer of this contract)

### Revisions
- **v2.0 (this)** — finalization pass re-anchored at `6bd63e5`. Absorbs, as FACT only: (1) **runtime rebuild seam** `ReinitializeRecordingEngineFromConfig` (`417ea64`+`23b16b9`) — B-regime fields now converge WITHOUT process restart; (2) **FPS authority resolution** — session/config FPS wins (V-CT1d), divergence = warning + rebuild requirement (`287b584` forced the opposite, superseded same day by `417ea64`); (3) **rate-control normalization** `ResolveRateControl` cbr/vbr/cq fail-closed → cbr + V-CT3c (`400b827`); (4) **strict CBR filler padding** on NVENC + evidence doc (`98cdffe`); (5) **pixel-format truth line rewritten** — nv12 = output/codec hint, output produces yuv420p (`897f02f`); (6) **PHASE 3 UI wave 1** (`7f43cb2`+`134bd90`): `UI_Engine.SaveSettings()` REMOVED, Engine WinForms demoted to diagnostic/operator, AudioSettingsForm triple-write killed, `[5] Video Capture` writes config.json only — §1 writer inventory and Q6 end-state updated accordingly. Q1–Q6 re-packaged; decision boxes still intentionally empty.
- **v1.0** — contract locked at `cf47732` (first drafted at `c5062ac`, rebased over GLM/1's validation-kit commit with zero source drift for cited files). Chain verified end-to-end from source; 6 open decisions (Q1–Q6) packaged for OWNER.

---

## 0. The contract in one line

> **One user setting → one canonical key in config.json → one mapper path → one runtime boundary — and every boundary is echoed honestly at runtime.**

The chain this document locks:

```text
config.json                     user intent — the ONLY user-facing store
   ↓  §2  canonical registry    which key owns which setting
CaptureSettings.Load            THE single apply-path (unified WINS)
   ↓  §3  effective config      three seams: per-record + engine-init + runtime-rebuild
runtime boundaries  §4          A per-record / B rebuildable / C echo-legacy
```

A setting is "real" only when runtime + ffprobe prove it (Configuration Truth Rule, matrix §1.3). Any UI that presents state the chain does not deliver is `UI guessed state` and is forbidden by the UI spec (§10) and by this contract.

---

## 1. Stores on disk (roles, writers, keys)

| STORE | ROLE | WRITER (evidence @ `6bd63e5`) | READ PATH |
|---|---|---|---|
| `config.json` | **ONLY user-facing store** (Law #1, matrix §1) | Overlay `AppSettings.Save()` `AppSettings.vb:1023` (atomic: sync-lock, foreign-key guard `:1026-1032`, temp+rename per matrix §1) · `[5] Video Capture.vb:645-646` (`AppSettings.Instance.Save()` — **video.json write gone**, comment :553-554 "legacy video.json is imported once by AppSettings migration at startup") · `AudioSettingsForm`-side writes: none (see `audio.json` row) · Launcher `AppConfigShared.WriteBool` (`Launcher/Main.vb:115-123`) · silent bulk import `SettingsExportImport.vb:135` | `CaptureSettings.Load` applies it ON TOP of engine.json (unified WINS) |
| `engine.json` | engine-internal knobs + compat/migration data (Law #2) | `CaptureSettings.Save()` `CaptureSettings.vb:170-196` — invoked by **two declared compat writers only**: `SyncWithOverlayConfig` LEGACY branch `UI_Engine.vb:520` (unified path early-returns at `:463-465` BEFORE any save) · PREWARM FFmpeg fallback `UI_Engine.vb:608` (only when existing path missing/invalid). **REMOVED in wave 1**: `UI_Engine.SaveSettings()` (was `:652-667` at v1.0 anchor; tombstone `UI_Engine.vb:690-696` "SaveSettings() REMOVED … second-writer divergence resolved") | loaded FIRST inside `Load`, then overridden by config.json |
| `video.json` | LEGACY import-once fallback | **NO UI writer remains** (`[5] Video Capture` writes config.json only); consulted only when config.json is unavailable (`CaptureSettings.vb:118-146`) | old installs only |
| `audio.json` | LEGACY fallback (old installs only) | `AudioSettingsForm.vb:127-128` — the form's SINGLE write (`SaveAudio`); its engine.json write and Overlay video.json shadow write were **REMOVED** (`:119-126` "triple-write divergence is resolved") | only when config.json is unavailable anywhere (`CaptureSettings.vb:118-146`) |
| `notifier_obs.json` | separate OBS-bridge store | `[7]:418-466` | `Common/ObsConfig.vb` — outside this contract's chain |

**config.json sections** (exhaustive, `ConfigFileDto` `AppSettings.vb:379-390`): `Recording` (`:359-372`), `Paths`, `UI`, `Audio`, `Privacy`, `Overlay`, `Notifications`, `Hotkeys`, `GitHubUser`, `GitHubTokenEncrypted`.
Consequences worth locking explicitly:

- **NO engine-selection key exists anywhere in config.json** (Q1).
- **NO rate-control key** and **NO pixel-format key** exist in config.json (Q3/Q4).
- `Recording` section exhaustive field list (`RecordingSectionDto` `:359-372`): `encoder`, `encoder_now`, `active_preset`, `current.{fps, bitrate, encoder_preset, use_native_resolution, width, height}` (`:335-342`), `replay_duration`, `my_presets`, `api_capture` (`:371`).
- Re-verified at `6bd63e5`: `AppSettings.vb`, `OverlayConfig.vb`, `CaptureSettings.vb` are byte-identical to the v1.0 anchor for every chain claim cited here — the drift landed entirely on the Engine/UI/runtime side.

**engine.json keys written today** (exhaustive, `CaptureSettings.Save()` `:176-191`): `ConfigVersion`, `CaptureMethod`, `PixelFormat`, `Preset`, `RateControl`, `FileFormat`, `FFmpegPath`, `HotkeyStart`, `HotkeyStop`, `HotkeyToggle`, `UseNativeResolution`, `CustomWidth`, `CustomHeight`. Already locked as facts: **no `OutputDirectory` and no `Encoder` key is serialized** (values edited in Engine UI are silently lost — UI spec §9), and the three `Hotkey*` keys are the dead side of the duplicate (`HotkeyService.vb:41,49` reads config.json `Hotkeys`). Wave-1 change: the NUMBER of call sites reaching `Save()` dropped from five to two (both declared compat writers) — the KEY inventory is unchanged (Q6 tracks deletion).

**Boundary rule (restated as contract):** the Overlay never writes `engine.json`/`video.json`/`audio.json`. Today this holds — all engine.json writers are Engine-side (both remaining call sites: `UI_Engine.vb:520`, `:608`), and the Overlay-side audio-form writes were removed with the triple-write kill.

---

## 2. Canonical field registry (the middle of the chain)

Ownership source of truth = matrix §3. This section locks the **mapping path** each canonical key takes — one path per field, no second apply-path ever.

### 2.1 THE apply-path invariant (I-1)

```text
CaptureSettings.Load (CaptureSettings.vb:94-164 — unchanged file, re-verified)
  1) engine.json first          :106-111   (engine-internal knobs)
  2) config.json WINS on top    :113-115   ApplyUnifiedToCaptureSettings + EARLY RETURN :114
  3) legacy video/audio.json    :118-146   only when (2) unavailable
```

> **I-1: There is exactly ONE composition order.** engine.json → unified config.json (WINS, early return) → legacy tier. `Never introduce a SECOND apply-path` (matrix §7). FIX-1, the per-record seam, AND the new runtime-rebuild seam all consume THIS chain — they change where it is invoked from, not its semantics.

### 2.2 The two mapper seams (I-2)

> **I-2: All mappings live in `NextRecordingConfig`** — `MapSessionConfig` (per-record, `:85-105`) and `MapStartupConfig` (engine-init, `:128-172`). Both are extracted verbatim so `Engine.ConfigTruth.Tests` executes the SAME composition on Linux (CT-4 / V-CT pattern). Host code must call them, never re-map inline (init `RecordingEngineHost.vb:89-90`; rebuild `:131`; per-record `:286-287`). (This is what PHASE 1 changed: the inline mappings moved into the testable seams.)

### 2.3 Registry — LOCKED rows (wired + tested; status must not regress)

| Canonical key (config.json) | Mapper path (single) | Effective runtime field | Regime | Test |
|---|---|---|---|---|
| `Recording.current.fps` | unified apply `OverlayConfig.vb:448` → `MapSessionConfig :93` (+ `MapStartupConfig :141-143`) | **Session FPS wins** (V-CT1d, `ResolveSessionTargetFps` `RecordingDTOs.vb:147`): CFR pacing + mux rate (`CaptureSession.vb:500-505`, fps-source echo `:505`); NVENC `FrameRateFps` at init (`RecordingEngine.vb:110`); divergence vs persistent session = warning `:217-218` + rebuild requirement | **B via rebuild** (see Q5) | V-CT1/1b/1c/**1d** |
| `Recording.current.bitrate` | `:449` (kbps→bps) → `MapStartupConfig :138-139` | `EncoderConfig.BitrateBps` → NVENC `averageBitRate` (`RecordingEngine.vb:103-106`) | B (rebuildable) | V-CT3/3b/5d |
| `Recording.current.encoder_preset` | `:455-457` → `MapStartupConfig :154-160` (`ConfigMigrator.MapNvencPresetInteger`, 1–7 → p1–p7, invalid → p4) | NVENC preset GUID; engine.json `Preset` = compat fallback ONLY (`:158-159`) | B (rebuildable) | V-CT4/4b/5f |
| `Recording.current.use_native_resolution` / `width` / `height` | `:458-462` → `MapStartupConfig :167-171` → `ResolveEncodeDimensions` | NVENC encode dims at init, GPU scale, oversized fails loudly (`RecordingEngine.vb:88-99` region — dims stamp `:111-112`) | B (rebuildable) | V-CT2/2b/2c |
| `Recording.encoder` | `MapEncoderToInternal` (`RecordingEngineHost.vb:93`, normalization echo `:95`, mapping `MapStartupConfig :131-137`) | `EncoderConfig.CodecKey` (`RecordingEngine.vb:102` region — codec echo `:146`); legacy `-c:v` via `MapEncoderToFfmpeg` | B (rebuildable) | host init |
| `Audio.*` (8 keys: system/mic enabled+volume, device id/name, track mode, clock mode) | unified apply `OverlayConfig.vb:495-515` → `MapSessionConfig :94-102` | `SessionConfig` audio fields, fresh every record | A | CT-4 + audio suites |
| `Paths.FFmpegPath` | `:517-519` (config wins when file exists) → FIX-1 fresh reload `RecordingEngineHost.vb:254,271` | ffmpeg resolution per record | A | FIX-1 suite |

### 2.4 Registry — TRANSITIONAL rows (owner decision or VIDEO-phase work pending)

| Field | Today | Why transitional | Decision |
|---|---|---|---|
| engine.json `RateControl` | ONLY source (`CaptureSettings.vb:61` → `MapStartupConfig :144` **via new `ResolveRateControl` normalization** `RecordingDTOs.vb:157-167`: cbr/vbr/cq only, unknown/blank fails closed → cbr) → `RecordingEngine.vb:108` | NO config.json key exists; matrix wire item #4; normalization + V-CT3c landed `400b827`, strict CBR filler + conformance evidence landed `98cdffe` | **Q3** |
| config.json `Recording.api_capture` | key exists (`:371`), mapped `OverlayConfig.vb:466-468`, but NO UI writer; New Engine echoes it as requested→selected→actual, non-ddagrab = recorded GAP (`RecordingEngine.vb:125-127`) | runtime still backend-hardcoded ddagrab (`:76`); native gfxcapture NOT IMPLEMENTED (`IVideoCaptureBackend.vb:8`) | **Q2** |
| engine.json `CaptureMethod` | legacy writer `UI_Engine.vb:660-664`; default `ddagrab` (`CaptureSettings.vb:58`); becomes effective when `api_capture` is null | twin of api_capture | **Q2/Q6** |
| engine.json `PixelFormat` | ONLY source (`:59`, default nv12) — **config NOT honored**: runtime BGRA8 → NVENC ARGB (`NvencResources.vb:107`), truth line `RecordingEngine.vb:134-142` (rewritten `897f02f`: nv12 = "output/codec pixel-format hint"; output produces yuv420p in MP4; non-nv12 request → warning "no conversion stage") | BLOCKER P1-PIXFMT — blocker unchanged, truth line now also documents the actual output format | **Q4** |
| engine.json `FileFormat` | legacy filename only (`GenerateOutputFilename`); New path is structurally MP4 (`LiveMuxSession.vb:18,111,210` — fragmented MP4 → +faststart remux `:315-329`) | legacy-only consumer | **Q6** |
| engine.json `HotkeyStart/Stop/Toggle` | DEAD (live system = config.json `Hotkeys` dict via `HotkeyService.vb:41,49`) | dead twin, deletion gated by evidence | **Q6** |
| engine.json `UseNativeResolution`/`CustomWidth`/`CustomHeight` | dead duplicates whenever config.json exists (unified WINS) | delete per matrix §8 | **Q6** |
| engine.json `FFmpegPath` | shadow copy; config wins when the file exists (`:517-519`); PREWARM writes it back only as compat fallback (`UI_Engine.vb:608`) | matrix wire item #1 | **Q6** |
| engine.json `Preset` | compat fallback only since PHASE 1 (`MapStartupConfig :158-159`) | delete after transition window | **Q3/Q6** |

---

## 3. Effective config (how the chain is consumed)

Three seams, all fed by the SAME `CaptureSettings.Load` chain (I-1):

### 3.1 Per-record seam (fresh — FIX-1)

`HandleRecordingStart` reloads effective config from disk on EVERY record start (`RecordingEngineHost.vb:254-256`): `CaptureSettings.Load(_configPath)` `:254` → `SyncWithOverlayConfig` `:255` → publish `_settings` `:256` → greppable echo `[RecordingEngine] effective config (fresh reload): …` `:262-266` → `NextRecordingConfig.MapSessionConfig` `:286-287`. FFmpegPath is re-resolved AFTER the reload so a fresh path is honored (`:271`).

**Consequence (locked):** everything in `MapSessionConfig` applies to the NEXT recording with no restart — audio group, FPS pacing, output path. This is regime A and it is already the reference behavior.

### 3.2 Engine-init seam (one-shot)

`InitializeRecordingEngine` (`RecordingEngineHost.vb:62-112`) snapshots once (`:70`), maps via `MapStartupConfig` (`:89-90`), and `RecordingEngine.Initialize(startup)` builds the persistent NVENC session (`RecordingEngine.vb:72-149`). `Initialize` is documented and enforced one-shot: "Called ONCE at process start. Subsequent calls throw" (`:50-55`, state guard throw `:69`); the encoder session is process-lifetime **between rebuilds** (see 3.3).

**Consequence (locked):** the B-registry rows apply at init. Changing them in config.json converges via the REBUILD seam (3.3) — the process no longer needs restarting, but between rebuilds the persistent NVENC session keeps its init values.

### 3.3 Runtime-rebuild seam (NEW — `417ea64` + `23b16b9`)

`ReinitializeRecordingEngineFromConfig` (`RecordingEngineHost.vb:118-152`) rebuilds the persistent engine from the current unified config.json WITHOUT process restart:

1. **Trigger**: `engine_config_changed` message with scope `video` / `config` / blank — handled at `UI_Engine.vb:629-663`, rebuild gate `:651-661`. Overlay senders: `[6] Audio Capture.vb:143` and `[5] Video Capture.vb:648`.
2. **Rebuild path**: fresh `CaptureSettings.Load` `:129` → `SyncWithOverlayConfig` `:130` → `MapStartupConfig` `:131` → NEW `RecordingEngine` initialized off-thread `:135-137` → swap on UI thread `:138-145` (previous runtime disposed only AFTER the replacement is live).
3. **Recording in progress** → `_rebuildPending = True` (`:121`), rebuild runs automatically after stop (`:334-336`).
4. **Record start during rebuild** → rejected with `engine_reconfiguring` (`:227-229`).
5. **Rebuild failure** → previous runtime is KEPT alive (error logged, `_engineReconfiguring` reset) — the engine never goes down because a rebuild failed.

> **I-3 (new invariant): the rebuild seam consumes the SAME chain.** It is `CaptureSettings.Load` + `SyncWithOverlayConfig` + `MapStartupConfig` — no second mapper, no second apply-path. Any future "apply now" mechanism MUST go through this seam, not re-map inline.

> **I-4 (new invariant): FPS resolution order.** Session/config FPS wins for CFR pacing (`ResolveSessionTargetFps`, V-CT1d); if the persistent NVENC session's init FPS differs, runtime logs the divergence warning (`RecordingEngine.vb:217-218` "runtime uses session/config FPS. Persistent NVENC must be rebuilt when FPS changes") and the rebuild seam converges the persistent session. The `287b584` interim behavior (force-override session FPS with the encoder FPS) was superseded by `417ea64` on the same day and is NOT the contract.

### 3.4 Acceptance echo layers (locked)

Every boundary must stay greppable — these lines are part of the contract, not logging garnish:

| Echo | Line | Proves |
|---|---|---|
| `[RecordingEngine] effective config (fresh reload)` | `RecordingEngineHost.vb:262-266` | per-record effective values |
| `[RecordingEngine] effective video config (startup)` | `RecordingEngine.vb:147` | init B-values + GOP independent of FPS |
| `capture method: requested → selected → actual` (+ GAP warning) | `:125-127` | honesty for C-regime capture |
| `pixel format: config='…' → runtime=BGRA8 → NVENC input=ARGB; output codec produces yuv420p in MP4` | `:140` (nv12) / `:142` (other, warning) | BLOCKER P1-PIXFMT truth |
| `fps source = config.json Recording.current.fps = N (display refresh … diagnostics-only)` | `CaptureSession.vb:505` | FPS single-owner |
| `session FPS X != persistent NVENC FPS Y; runtime uses session/config FPS…` | `RecordingEngine.vb:217-218` | I-4 divergence honesty |
| `rebuilding persistent runtime from fresh config.json...` / `runtime rebuilt: fps=…` | `RecordingEngineHost.vb:127` / `:144` | rebuild seam visibility |
| `config changed during recording — rebuild queued after stop` | `:122` | queued-rebuild honesty |
| `★ PIPELINE = LEGACY…` | `[Engine] Client.vb:258` | which pipeline actually ran |

---

## 4. Runtime boundaries (the regime table — locked)

| Regime | Meaning | Fields (today) | UI label required |
|---|---|---|---|
| **A · per-record (fresh)** | re-read from disk at every record start | `Audio.*` (8), FPS **pacing+mux** (session FPS wins, I-4), `Paths.FFmpegPath`, output path | "applies next recording" |
| **B · rebuildable (init-frozen between rebuilds)** | read once into the persistent NVENC session; converges via the rebuild seam (3.3) when `engine_config_changed` fires — no process restart | `Recording.encoder`, `current.bitrate`, `current.encoder_preset`, `current.use_native_resolution/width/height`, engine.json `RateControl`, GOP (engine default 60 — never from FPS, V-CT1 fix) | wording = Q5 decision (was "engine restart required"; the mechanism now exists to label "applies after automatic runtime rebuild") |
| **C · echo-only** | New Engine never consumes; logged honestly | capture method (requested→selected→actual), pixel format (BGRA/ARGB/yuv420p truth) | honest value + blocker note (P1-PIXFMT) |
| **LEGACY-only** | consumed by FFmpeg path only | `CaptureMethod` whitelist (`CaptureSettings.vb:493-496`), `FileFormat`, legacy pixel-format args, gdigrab/gfxcapture | "legacy pipeline only" |
| **RESERVED** | no runtime consumer | `replay_duration` (replay `not_implemented`, `UI_Engine.vb:552-560`) | disabled until wired |

**Engine selection itself is runtime-internal** (`_useNewEngine = True` at `RecordingEngineHost.vb:42`; on init failure `_useNewEngine = False` `:108` with stored reason `:105`; dispatch `[Engine] Client.vb:245-274`) — it has NO regime because it has NO canonical key (Q1). The rebuild seam does not change selection: it rebuilds the NEW engine; legacy fallback remains the init-failure resilience path.

**Validation boundary (flagged, not re-decided):** engine caps are `Validate()` FPS 1–240 / bitrate 1–200 Mbps / method whitelist (`CaptureSettings.vb:483-507`) while Overlay UI still allows FPS up to 800 and bitrate 500–150000 kbps (UI spec §11). Unifying the caps is a UI-spec acceptance item (#5), owned there — this contract only locks that `Validate()` is the runtime truth.

---

## 5. Open decisions Q1–Q6 (FACT → OPTIONS → IMPACT → RECOMMENDATION → OWNER DECISION REQUIRED)

> GLM does **not** decide here. Each RECOMMENDATION is a non-binding engineering view so the OWNER can decide fast; each decision box is intentionally left empty. Deciding any Q narrows the UI spec verdict (`WAIT FOR CONFIG`, UI spec v1.1) accordingly.

### Q1 — Engine selection: there is NO canonical key

**FACT**
- Runtime selection is hardcoded and automatic: `_useNewEngine = True` (`RecordingEngineHost.vb:42`); on init failure `_useNewEngine = False` (`:108`) with the failure reason stored (`:105`, `_engineInitFailReason`) and surfaced on legacy record starts (`:44-47` comment block); dispatch switch `[Engine] Client.vb:245-274`; legacy marker log `:258`.
- config.json has no key for it: the section list is exhaustive (`ConfigFileDto:379-390`) and `Recording` (`:359-372`) carries no engine-mode field. `NOT IMPLEMENTED as a user choice` (UI spec §2).
- The New Engine is structurally NVENC+ddagrab: `NvencEncoderBackend` constructed unconditionally (`RecordingEngine.vb:82`), backend hardcoded `DdagrabBackend` (`:76`).
- Re-verified at `6bd63e5`: the runtime-rebuild seam does NOT touch selection (it rebuilds the New engine in place); no commit in `5b2a8a0..6bd63e5` added an engine key.

**OPTIONS**
- (a) Keep implicit selection; expose read-only pipeline status in UI (what UI spec §13 already specs).
- (b) Add a config.json key (e.g. an `Engine` section or `Recording.pipeline`: `auto | new | legacy`), mapped through the single apply-path, consumed by the host before dispatch.
- (c) Engine-internal diagnostic override in engine.json (not user-facing; violates nothing but stays invisible to config.json).

**IMPACT**
- (b) unblocks the Overlay ENGINE section (UI spec §13, migration step §14-5) and gives non-NVENC encoder selection a real routing anchor (UI acceptance #9 depends on knowing which pipeline will run).
- Without a decision, UI must stay read-only on this topic — the current verdict's first blocker (UI spec v1.1 verdict item 1).
- (b) adds a schema key + wire + test burden (matrix §6 order) — it is a schema change, which is exactly why it is an OWNER decision, not a GLM one.

**RECOMMENDATION** (non-binding)
- (b) with default `auto` where `auto` == today's behavior (new engine unless init failed). Legacy escape on init failure should be preserved regardless of the key, because it is a resilience path, not a preference.

**OWNER DECISION REQUIRED**
- [ ] Add the key now, or keep implicit selection for the stabilization branch?
- [ ] If add: key name + section? allowed values? default? which process owns writing it (Overlay-only)?
- [ ] Is "force legacy" a supported user choice at all, or init-failure-only?

### Q2 — `Recording.api_capture`: key exists, no UI writer, runtime echoes instead of honoring

**FACT**
- Key exists with documented semantics: `api_capture` — "ddagrab, gfxcapture, GDIGrab, or null (auto)", default null (`AppSettings.vb:370-371`).
- No UI writer anywhere (model + legacy migration only, `AppSettings.LegacyVideo.vb:249`).
- Mapping: non-null → `CaptureMethod` (`OverlayConfig.vb:466-468`); null → engine.json `CaptureMethod` value stands (default `ddagrab` `CaptureSettings.vb:58`; legacy Engine UI writer `UI_Engine.vb:660-664` — read-only in the wave-1 diagnostic console).
- New Engine: requested→selected→actual echo (`RecordingEngine.vb:125-127`) — `ddagrab`/empty = selected; anything else = **recorded GAP warning**, runtime still ddagrab; native `GfxCaptureBackend` NOT IMPLEMENTED (`IVideoCaptureBackend.vb:8` "future").
- Legacy path: whitelist + builders honor it (`CaptureSettings.vb:493-496`, `FFmpegCommandBuilderV2.vb:79-96`).
- Re-verified: no change to api_capture semantics in `5b2a8a0..6bd63e5` (echo line numbers moved only).

**OPTIONS**
- (a) Wire an Overlay UI writer now (ENGINE section), disabled+ labeled "built-in ddagrab" when the New Engine runs; gfxcapture/gdigrab effective on legacy only.
- (b) Park the key (no writer) until native gfxcapture exists (VIDEO phase, GLM/1 territory).
- (c) Formalize `null` = "auto" in code (today it is implicit via mapping order) and document it as the only supported value for now.

**IMPACT**
- (a) gives the ENGINE section its second control on an existing key (UI spec §13); risk: users select gfxcapture and get an honest GAP echo on New Engine — acceptable only with the label.
- (b/c) leave UI showing read-only capture status; zero runtime risk.
- Any native-backend work here belongs to GLM/1's VIDEO phase — this contract only locks the config semantics.

**RECOMMENDATION** (non-binding)
- (c) first (document `null` = auto as the contract value), then (a) with the disabled+label treatment. No writer should promise gfxcapture on the New Engine while the GAP echo exists.

**OWNER DECISION REQUIRED**
- [ ] Is `null` = auto the formal contract value for stabilization?
- [ ] Add the UI writer now or park until native gfxcapture?
- [ ] If writer now: allowed values in UI (ddagrab only? + gdigrab legacy-only? + gfxcapture with GAP label?)
- [ ] What happens to engine.json `CaptureMethod` twin — retire the Engine-UI writer (UI spec §14-1) in the same window?

### Q3 — Preset / RateControl ownership

**FACT**
- **Preset: DONE (locked).** Canonical = config.json `current.encoder_preset` (1–7): unified apply `OverlayConfig.vb:455-457` → `MapStartupConfig :154-160` via the single mapper (`ConfigMigrator.MapNvencPresetInteger` → p1–p7, invalid → p4) → NVENC preset GUID. engine.json `Preset` is compat fallback ONLY (used when the config value is out of range, `:158-159`). Tests V-CT4/4b/5f. Matrix row upgraded to `✅ Unified @ PHASE 1`.
- **RateControl: NOT DONE as ownership — but hardened as runtime.** engine.json is the ONLY source (`CaptureSettings.vb:61`, default `cbr`); NO config.json key exists (`RecordingSectionDto:359-372` has none); mapped `MapStartupConfig :144` through **`ResolveRateControl`** (`RecordingDTOs.vb:157-167`, added `400b827`: accepts only `cbr|vbr|cq`, case/whitespace-normalized, unknown/blank **fails closed → cbr**) → `RecordingEngine.vb:108` → NVENC `rateControlMode` (V-CT5). New test **V-CT3c** (`VCTVideoWiringTests.vb`).
- **CBR is now production-proven**: strict filler padding enabled for CBR (`98cdffe` — `enableFillerDataInsertion`, bit 17, in `NvEncConfigSerializer.vb`) with real-record conformance evidence `evidence/cbr-conformance-20260902-filler.md` (CBR / 20 Mbps / VBV 2× verified).
- Both are regime B (rebuildable — 3.3).

**OPTIONS**
- (a) Add config.json `Recording.current.rate_control` + unified-apply mapping + ConfigTruth test; keep engine.json read as compat fallback during the transition window (mirror the preset pattern), then delete per matrix §6.6. The normalization + tests to clone already exist (preset precedent + `ResolveRateControl`).
- (b) Hardcode `cbr` and drop the knob until a UI wants it.
- (c) Leave the owner in engine.json permanently — contradicts Law #1; rejected by the matrix but listed for completeness.

**IMPACT**
- (a) closes matrix wire item #4 and makes RC the last encoder-group key to reach single-ownership; UI can expose RC later with the rebuild label (3.3) instead of "restart required".
- (b) simplifies the schema but silently removes an advanced knob (CBR/VBR/CQ) that NVENC already honors and conformance-proves — a product decision, not an engineering one.
- Deletion order matters: removing the engine.json key before the config wire exists = silent default fallback (matrix §7).

**RECOMMENDATION** (non-binding)
- (a), cloned from the preset precedent (it has tests, a mapper, and a fallback pattern already reviewed by the OWNER) — the new normalization makes (a) strictly safer than at v1.0: even out-of-contract values fail closed to the proven cbr.

**OWNER DECISION REQUIRED**
- [ ] Add `rate_control` to config.json now, defer, or hardcode?
- [ ] If add: default value (`cbr`)? allowed values (cbr/vbr/cq — the normalized set)? UI exposure timing?
- [ ] Transition window for the engine.json fallback — when may it be deleted (real-record ffprobe gate)?

### Q4 — PixelFormat: BLOCKER P1-PIXFMT (config not honored)

**FACT**
- engine.json `PixelFormat` is the only key (`CaptureSettings.vb:59`, default `nv12`); NO config.json key; no UI.
- Runtime truth (truth line REWRITTEN by `897f02f`, `RecordingEngine.vb:134-142`): capture texture is fixed by the Ddagrab backend (BGRA8) → NVENC input `NV_ENC_BUFFER_FORMAT_ARGB` (`NvencResources.vb:107`); a GPU conversion layer (BGRA→NV12) is NOT implemented; the requested `nv12` is documented as an **output/codec pixel-format hint** and the truth line now also states the actual output: `output codec produces yuv420p in MP4` (`:140`); any other requested value logs a warning "not a native capture texture format … no conversion stage is currently implemented" (`:142`). Recording continues BGRA/ARGB — honest, no fake pass.
- The legacy FFmpeg path DOES honor pixel format (builders) — the semantics are pipeline-split.
- Matrix row: 🔴 BLOCKER recorded (P1-PIXFMT); UI spec requires any PixelFormat control on New Engine to display "runtime BGRA/ARGB" (§5).

**OPTIONS**
- (a) Implement the BGRA→NV12 GPU conversion in the New Engine (VIDEO phase — GLM/1 territory), then move the key to config.json and honor it.
- (b) Move the key to config.json NOW with the blocker label (matrix wire item #5 direction), honoring it only on the legacy path until (a).
- (c) Park/retire the key for the New Engine: accept BGRA/ARGB capture → yuv420p output as the product default, keep nv12 meaning legacy-only.

**IMPACT**
- (a) changes capture semantics + perf + NVENC buffer management — explicitly gated by matrix §7 (real-record evidence), not a config-plumbing task.
- (b) fixes ownership but the New Engine would still not honor it — honest only with the blocker label.
- (c) is the smallest truth-preserving state; any UI showing a pixel-format choice without (a) is a labeled lie (UI spec §5). The `897f02f` truth line makes (c) cheaper to communicate: the honest default is now fully documented at runtime (BGRA8 → ARGB → yuv420p).

**RECOMMENDATION** (non-binding)
- (c) for the stabilization window + schedule (a) in the VIDEO phase; if the OWNER wants the key visible, (b) with the blocker label — never a fake pass.

**OWNER DECISION REQUIRED**
- [ ] Accept BGRA/ARGB capture → yuv420p output as the New Engine's product default for now, or require NV12 (schedule conversion)?
- [ ] If NV12 required: which phase owns the conversion layer (VIDEO/GLM/1) and what is the evidence gate?
- [ ] What does engine.json `PixelFormat` become meanwhile — legacy-only key, or retired from UI entirely?

### Q5 — Frozen fields: the rebuild contract (was: "what exactly restarts?")

**FACT (the frozen set, and the NEW mechanism that unfreezes it)**
- Frozen at engine init (regime B, persistent NVENC — `RecordingEngine.vb:72-149`): codec (`:146` echo), bitrate (`:103-106`), preset (`:109`), rate control (`:108`), encode dimensions (`:92-99` region, `:111-112`), GOP (engine default 60, `:83` — never derived from FPS since V-CT1).
- Fresh per record (regime A, `RecordingEngineHost.vb:254-287`): audio group, FPS pacing+mux (`CaptureSession.vb:500-505`), FFmpegPath, output path.
- **The restart contract CHANGED since v1.0.** A runtime-rebuild seam now exists (`ReinitializeRecordingEngineFromConfig`, `RecordingEngineHost.vb:118-152`, §3.3): B-fields converge from config.json WITHOUT process restart — automatic when idle, queued to after-stop during a recording, record starts rejected with `engine_reconfiguring` while rebuilding, previous runtime kept on rebuild failure.
- **The FPS span is RESOLVED, not open.** v1.0 documented an A→B span (pacing=mutable, NVENC rate=frozen). At `6bd63e5` the resolution order is I-4: session/config FPS always wins for pacing (V-CT1d `ResolveSessionTargetFps`); divergence from the persistent NVENC session is a logged warning (`RecordingEngine.vb:217-218`) and the rebuild seam converges the persistent session. The interim force-encoder-FPS behavior (`287b584`) was superseded by `417ea64` the same day.
- Rebuild trigger surface today = `engine_config_changed` scopes `video` / `config` / blank (`UI_Engine.vb:651-661`), sent by Overlay `[6] Audio Capture.vb:143` and `[5] Video Capture.vb:648`.

**OPTIONS**
- (a) Accept the rebuild contract explicitly as product behavior: B-fields get an "applies after automatic runtime rebuild" treatment (UI spec §15 item 4 wording updated from "engine restart required"); I-3/I-4 locked as invariants.
- (b) Design per-record NVENC re-init (Phase 4 scope — GLM/1 territory; NVENC session rebuild cost + risk) — largely subsumed by (a): the rebuild seam IS an idle-time re-init; the open residue is only "re-init DURING a recording", which today is queued, not concurrent.
- (c) Manual-restart-only (pre-`417ea64` world) — superseded in code; listed for completeness.

**IMPACT**
- (a) unlocks UI: `[5]` preset/bitrate/resolution controls may go live WITH labels (UI spec §14 step 7 requires this decision). The queue-during-recording and failure-keeps-previous semantics remove the two sharpest edges v1.0 worried about (killed in-flight state, overlap with supervisor logic).
- The FPS divergence warning is honest but noisy if a user changes FPS and records before the rebuild converges — labeling (a) should tell users the rebuild is automatic; if the OWNER wants stricter behavior (block recording until rebuilt), that is a new decision, not this contract's.
- The rebuild seam multiplies Initialize-path invocations: anything that assumed "Initialize once per process" (diagnostics, leak accounting) must be verified against rebuild — the one-shot guard inside `RecordingEngine.Initialize` still holds per INSTANCE (`:69`), which is what makes (a) safe.

**RECOMMENDATION** (non-binding)
- (a) now — it matches the code as it exists at the anchor commit (the contract documents reality; it does not grant it). Put any per-record-during-recording re-init on the Phase 4 backlog as a design study, not a commitment.

**OWNER DECISION REQUIRED**
- [ ] Accept the rebuild contract (a) as the stabilization-branch product behavior (automatic idle rebuild + queued after-stop rebuild)?
- [ ] Exact label/badge list for B-fields ("applies after automatic runtime rebuild" vs stricter wording)?
- [ ] Should a B-field change DURING a recording block the next recording until the queued rebuild completes (today: next record is allowed with the OLD persistent values + FPS divergence warning), or is the current queue-then-apply acceptable?

### Q6 — engine.json end-state: what remains?

**FACT**
- Keys today (exhaustive, `CaptureSettings.Save()` `:176-191`): `ConfigVersion`, `CaptureMethod`, `PixelFormat`, `Preset`, `RateControl`, `FileFormat`, `FFmpegPath`, `HotkeyStart/Stop/Toggle`, `UseNativeResolution`, `CustomWidth`, `CustomHeight`.
- **The writer inventory collapsed since v1.0 (PHASE 3 wave 1, `7f43cb2`+`134bd90`)**: `UI_Engine.SaveSettings()` REMOVED (tombstone `UI_Engine.vb:690-696`); AudioSettingsForm's engine.json + video.json writes REMOVED (single legacy `audio.json` write remains, `AudioSettingsForm.vb:119-128`); `[5] Video Capture` writes config.json only (`:645-646`). Remaining engine.json writers = TWO declared compat writers: `SyncWithOverlayConfig` legacy branch (`UI_Engine.vb:520`) and PREWARM FFmpeg fallback (`:608`). The "silent write-backs keep rewriting engine.json" concern from v1.0 is now bounded: unified path never saves (`:463-465` early return).
- Post-PHASE-1 roles: `Preset` = compat fallback only; `UseNativeResolution`/`CustomWidth`/`CustomHeight` = dead duplicates whenever config.json exists (unified WINS); `Hotkey*` = DEAD (live system reads config.json `Hotkeys`, `HotkeyService.vb:41,49`); `FFmpegPath` = shadow copy (config wins when the file exists, `OverlayConfig.vb:517-519`); `CaptureMethod` = legacy writer + auto default; `RateControl`/`PixelFormat`/`FileFormat` = still genuinely consumed (RC by New Engine init + rebuild; PF/FileFormat by the legacy path).
- Engine UI is now a diagnostic/operator console with read-only effective-value mirrors (`UI_Engine.vb:74-84`, `:106-111`) — it no longer offers competing write surfaces at all.
- Law #2 (matrix §1): engine.json must never store a user expectation that config.json also carries.

**OPTIONS (end-state definition)**
- (i) engine.json retains ONLY: engine-internal knobs not yet promoted (RateControl, PixelFormat, FileFormat until their decisions land), diagnostics, and migration/compatibility data (`ConfigVersion`, V2 migration block). Everything user-facing moves to config.json following matrix §8 order.
- (ii) Keep engine.json as a full parallel store (contradicts Law #1 — rejected by the matrix).
- (iii) Delete engine.json as soon as Q3/Q4 decide (premature — silent fallbacks, matrix §7).

**IMPACT**
- (i) gives a concrete deletion order: dead twins first (`Hotkey*`, resolution trio, `Preset` after the transition window), consumed-but-doomed keys after their Q3/Q4 decisions + evidence gates. Wave 1 already executed the WRITER half of (i) (no user-facing engine.json writer remains); the KEY half (deleting the dead keys from `Save()`) is what remains.
- Without a decided end-state, every future phase re-litigates which file wins — the contract's purpose is to end that.

**RECOMMENDATION** (non-binding)
- (i), with the deletion sequence tied to matrix §8 items and §6.6 evidence gates. Note the end-state is now >half-implemented: writers are consolidated; only the key deletions (and the two compat writers' retirement once config.json is guaranteed present) remain.

**OWNER DECISION REQUIRED**
- [ ] Approve end-state (i) as the definition of "engine.json remains what"?
- [ ] Approve the deletion order (dead twins → transition-window keys → decision-gated keys)?
- [ ] Keep `CaptureMethod`/`FileFormat` in engine.json while the legacy pipeline exists — yes/no?
- [ ] When may the two remaining compat writers (`UI_Engine.vb:520` legacy branch, `:608` PREWARM) be retired (gate: config.json guaranteed present on all install generations)?

---

## 6. What this contract locks vs leaves open

**LOCKED by v2.0** (change only via §7 amendment):
1. Chain shape: config.json → canonical registry → `CaptureSettings.Load` (unified WINS) → three seams (`NextRecordingConfig` + rebuild) → regimes.
2. Invariants I-1 (single apply-path, `CaptureSettings.vb:94-164`), I-2 (mappings live only in `NextRecordingConfig`, test-compiled on Linux), **I-3 (rebuild seam consumes the same chain — no second apply-path)**, **I-4 (FPS resolution order: session/config wins; divergence = warning + rebuild convergence; the `287b584` force-override is NOT the contract)**.
3. Store roles + exhaustive key inventories (§1), the Overlay-never-writes-engine.json boundary, and the **wave-1 writer consolidation** (SaveSettings removed; two declared compat writers remain).
4. Locked registry rows (§2.3) — FPS single-owner with V-CT1d resolution, preset single-mapper, bitrate/resolution/encoder/audio paths, FIX-1 freshness.
5. Regime table (§4) incl. the rebuildable B-regime and the echo acceptance layers (§3.4).
6. `Validate()` is the runtime truth for caps (FPS 1–240, bitrate 1–200 Mbps, method whitelist).
7. Rate-control normalization semantics (`cbr|vbr|cq`, fail-closed → cbr, V-CT3c) and the CBR strict-filler behavior + conformance evidence path (`98cdffe`).

**OPEN (blocked on OWNER):** Q1 engine-selection key · Q2 api_capture writer + null semantics · Q3 rate_control key · Q4 PixelFormat blocker disposition · Q5 rebuild-contract acceptance + labeling · Q6 engine.json end-state approval + compat-writer retirement gate.
**OPEN (owned elsewhere, cross-referenced):** validation-cap unification + volume range (UI spec §11/§15) · native gfxcapture + NV12 conversion + per-record-during-recording re-init (VIDEO/Phase 4, GLM/1).

## 7. Amendment protocol

1. Any status change to a LOCKED item requires: `path + method + line` evidence at the current HEAD + a ConfigTruth-pattern test (FAIL-first) + real-record ffprobe confirmation when the field reaches the mux (matrix §9 — same gate, no exceptions).
   - **Evidence path for the real-record layer:** the `--videocheck` kit (GLM/1 `cf47732`): `scripts/windows-phase1-video-validation.ps1` drives the canonical chain on the OWNER's Windows machine with ffprobe asserts; results ladder in `docs/PHASE1_VIDEO_VALIDATION_STATUS.md`. Linux suites prove the mapping; the kit produces the runtime/output truth — both layers feed any amendment. CBR-conformance evidence (`evidence/cbr-conformance-20260902-filler.md`) is the precedent for per-behavior evidence files.
2. Wire/move/delete lands as separate ordered commits: owner change → mapping path → test → echo log → ffprobe → then duplicate deletion (matrix §9).
3. This document never re-decides ownership (matrix's job) and never designs UI (UI spec's job) — it locks the runtime contract between them. Conflicts resolve in that order: matrix > contract > UI spec, then re-amend.
4. Every amendment bumps the Revisions block with the anchor commit and a one-line delta.

