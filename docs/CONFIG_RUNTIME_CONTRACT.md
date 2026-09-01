# CONFIG RUNTIME CONTRACT — NVIDIA ShadowPlay

- Route: **GLM/2 — CONFIG CONTRACT FINALIZATION** (doc-only; no production code touched, no schema change, no feature)
- Anchored at commit: **`cf47732`** (every cited source file is unchanged since `06667e9`; `7fc11ba`/`c5062ac` are docs-only; `cf47732` adds GLM/1's Windows validation kit + `docs/PHASE1_VIDEO_VALIDATION_STATUS.md` — cited in §7)
- Owner: ScotcsDuluka · contract drafted + every FACT re-traced from real source by GLM/2
- Method: `path · line` citations valid at the anchor commit. Missing things are reported `NO KEY` / `NOT IMPLEMENTED` — never invented. **No decision is made here**: §5 ships each open question as FACT → OPTIONS → IMPACT → RECOMMENDATION (non-binding) → **OWNER DECISION REQUIRED**.
- Companion docs (their roles are not re-decided here): `docs/CONFIG_OWNERSHIP_MATRIX.md` (ownership source of truth) · `docs/UI_CONFIG_ARCHITECTURE.md` v1.1 (UI consumer of this contract)

### Revisions
- **v1.0 (this)** — contract locked at `cf47732` (first drafted at `c5062ac`, rebased over GLM/1's validation-kit commit with zero source drift for cited files). Chain verified end-to-end from source; 6 open decisions (Q1–Q6) packaged for OWNER.

---

## 0. The contract in one line

> **One user setting → one canonical key in config.json → one mapper path → one runtime boundary — and every boundary is echoed honestly at runtime.**

The chain this document locks:

```text
config.json                     user intent — the ONLY user-facing store
   ↓  §2  canonical registry    which key owns which setting
CaptureSettings.Load            THE single apply-path (unified WINS)
   ↓  §3  effective config      two seams: per-record + engine-init
runtime boundaries  §4          A per-record / B init-frozen / C echo-legacy
```

A setting is "real" only when runtime + ffprobe prove it (Configuration Truth Rule, matrix §1.3). Any UI that presents state the chain does not deliver is `UI guessed state` and is forbidden by the UI spec (§10) and by this contract.

---

## 1. Stores on disk (roles, writers, keys)

| STORE | ROLE | WRITER (evidence) | READ PATH |
|---|---|---|---|
| `config.json` | **ONLY user-facing store** (Law #1, matrix §1) | Overlay `AppSettings.Save()` `AppSettings.vb:1023` (atomic: sync-lock, foreign-key guard `:1026-1032`, temp+rename per matrix §1) · Launcher `AppConfigShared.WriteBool` (`Launcher/Main.vb:115-123`) · silent bulk import `SettingsExportImport.vb:135` | `CaptureSettings.Load` applies it ON TOP of engine.json (unified WINS) |
| `engine.json` | engine-internal knobs + compat/migration data (Law #2) | Engine-side only: `CaptureSettings.Save()` `CaptureSettings.vb:170-196` · `UI_Engine.SaveSettings:652-667` (`:666`) · silent write-backs `UI_Engine.vb:493` (per record start) and `:578-581` (PREWARM FFmpeg) | loaded FIRST inside `Load`, then overridden by config.json |
| `video.json` / `audio.json` | LEGACY fallback (old installs only) | `AudioSettingsForm.vb:131-158` (video.json — slated for removal, UI spec §14 step 1) | only when config.json is unavailable anywhere (`CaptureSettings.vb:118-146`) |
| `notifier_obs.json` | separate OBS-bridge store | `[7]:418-466` | `Common/ObsConfig.vb` — outside this contract's chain |

**config.json sections** (exhaustive, `ConfigFileDto` `AppSettings.vb:379-390`): `Recording` (`:359-372`), `Paths`, `UI`, `Audio`, `Privacy`, `Overlay`, `Notifications`, `Hotkeys`, `GitHubUser`, `GitHubTokenEncrypted`.
Consequences worth locking explicitly:

- **NO engine-selection key exists anywhere in config.json** (Q1).
- **NO rate-control key** and **NO pixel-format key** exist in config.json (Q3/Q4).
- `Recording` section exhaustive field list (`RecordingSectionDto` `:359-372`): `encoder`, `encoder_now`, `active_preset`, `current.{fps, bitrate, encoder_preset, use_native_resolution, width, height}` (`:335-342`), `replay_duration`, `my_presets`, `api_capture` (`:371`).

**engine.json keys written today** (exhaustive, `CaptureSettings.Save()` `:176-191`): `ConfigVersion`, `CaptureMethod`, `PixelFormat`, `Preset`, `RateControl`, `FileFormat`, `FFmpegPath`, `HotkeyStart`, `HotkeyStop`, `HotkeyToggle`, `UseNativeResolution`, `CustomWidth`, `CustomHeight`. Already locked as facts: **no `OutputDirectory` and no `Encoder` key is serialized** (values edited in Engine UI are silently lost — UI spec §9), and the three `Hotkey*` keys are the dead side of the duplicate (`HotkeyService.vb:41,49` reads config.json `Hotkeys`).

**Boundary rule (restated as contract):** the Overlay never writes `engine.json`/`video.json`/`audio.json`. Today this holds — all engine.json writers are Engine-side (`UI_Engine.vb:493,581,666`, `AudioSettingsForm.vb:122`).

---

## 2. Canonical field registry (the middle of the chain)

Ownership source of truth = matrix §3. This section locks the **mapping path** each canonical key takes — one path per field, no second apply-path ever.

### 2.1 THE apply-path invariant (I-1)

```text
CaptureSettings.Load (CaptureSettings.vb:94-164)
  1) engine.json first          :106-111   (engine-internal knobs)
  2) config.json WINS on top    :113-115   ApplyUnifiedToCaptureSettings + EARLY RETURN :114
  3) legacy video/audio.json    :118-146   only when (2) unavailable
```

> **I-1: There is exactly ONE composition order.** engine.json → unified config.json (WINS, early return) → legacy tier. `Never introduce a SECOND apply-path` (matrix §7). FIX-1 and the per-record seam both consume THIS chain — they change where it is invoked from, not its semantics.

### 2.2 The two mapper seams (I-2)

> **I-2: All mappings live in `NextRecordingConfig`** — `MapSessionConfig` (per-record, `:85-105`) and `MapStartupConfig` (engine-init, `:128-182`). Both are extracted verbatim so `Engine.ConfigTruth.Tests` executes the SAME composition on Linux (CT-4 / V-CT pattern). Host code must call them, never re-map inline. (This is what PHASE 1 changed: the inline mappings moved into the testable seams — `RecordingEngineHost.vb:78-88`, `:227-236`.)

### 2.3 Registry — LOCKED rows (wired + tested; status must not regress)

| Canonical key (config.json) | Mapper path (single) | Effective runtime field | Regime | Test |
|---|---|---|---|---|
| `Recording.current.fps` | unified apply `OverlayConfig.vb:448` → `MapSessionConfig :93` (+ `MapStartupConfig :142`) | `SessionConfig.TargetFps` → CFR pacing + mux rate (`CaptureSession.vb:495-500`); NVENC `FrameRateFps` at init (`RecordingEngine.vb:110`) | **A→B span** (see Q5) | V-CT1/1b/1c |
| `Recording.current.bitrate` | `:449` (kbps→bps) → `MapStartupConfig :139` | `EncoderConfig.BitrateBps` → NVENC `averageBitRate` (`RecordingEngine.vb:103-106`) | B | V-CT3/3b/5d |
| `Recording.current.encoder_preset` | `:455-457` → `MapStartupConfig :156-162` (`ConfigMigrator.MapNvencPresetInteger`, 1–7 → p1–p7, invalid → p4) | NVENC preset GUID; engine.json `Preset` = compat fallback ONLY (`:160-161`) | B | V-CT4/4b/5f |
| `Recording.current.use_native_resolution` / `width` / `height` | `:458-462` → `MapStartupConfig :169-173` → `ResolveEncodeDimensions` | NVENC encode dims at init, GPU scale, oversized fails loudly (`RecordingEngine.vb:88-99`) | B | V-CT2/2b/2c |
| `Recording.encoder` | `MapEncoderToInternal` (`RecordingEngineHost.vb:91`, normalization `MapStartupConfig :136`) | `EncoderConfig.CodecKey` (`RecordingEngine.vb:102`); legacy `-c:v` via `MapEncoderToFfmpeg` | B | host init |
| `Audio.*` (8 keys: system/mic enabled+volume, device id/name, track mode, clock mode) | unified apply `OverlayConfig.vb:495-515` → `MapSessionConfig :94-102` | `SessionConfig` audio fields, fresh every record | A | CT-4 + audio suites |
| `Paths.FFmpegPath` | `:517-519` (config wins when file exists) → FIX-1 fresh reload `RecordingEngineHost.vb:203,220` | ffmpeg resolution per record | A | FIX-1 suite |

### 2.4 Registry — TRANSITIONAL rows (owner decision or VIDEO-phase work pending)

| Field | Today | Why transitional | Decision |
|---|---|---|---|
| engine.json `RateControl` | ONLY source (`CaptureSettings.vb:61` → `MapStartupConfig :144-146` → `RecordingEngine.vb:108`) | NO config.json key exists; matrix wire item #4 | **Q3** |
| config.json `Recording.api_capture` | key exists (`:371`), mapped `OverlayConfig.vb:466-468`, but NO UI writer; New Engine echoes it as requested→selected→actual, non-ddagrab = recorded GAP (`RecordingEngine.vb:123-128`) | runtime still backend-hardcoded ddagrab (`:76`); native gfxcapture NOT IMPLEMENTED (`IVideoCaptureBackend.vb:8`) | **Q2** |
| engine.json `CaptureMethod` | legacy writer `UI_Engine.vb:660-664`; default `ddagrab` (`CaptureSettings.vb:58`); becomes effective when `api_capture` is null | twin of api_capture | **Q2/Q6** |
| engine.json `PixelFormat` | ONLY source (`:59`, default nv12) — **config NOT honored**: runtime BGRA8 → NVENC ARGB (`NvencResources.vb:107`), truth line `RecordingEngine.vb:130-135` | BLOCKER P1-PIXFMT | **Q4** |
| engine.json `FileFormat` | legacy filename only (`GenerateOutputFilename`); New path is structurally MP4 (`LiveMuxSession.vb:18,306`) | legacy-only consumer | **Q6** |
| engine.json `HotkeyStart/Stop/Toggle` | DEAD (live system = config.json `Hotkeys` dict via `HotkeyService.vb:41,49`) | dead twin, deletion gated by evidence | **Q6** |
| engine.json `UseNativeResolution`/`CustomWidth`/`CustomHeight` | dead duplicates whenever config.json exists (unified WINS) | delete per matrix §8 | **Q6** |
| engine.json `FFmpegPath` | shadow copy; config wins when the file exists (`:517-519`) | matrix wire item #1 | **Q6** |
| engine.json `Preset` | compat fallback only since PHASE 1 (`MapStartupConfig :160-161`) | delete after transition window | **Q3/Q6** |

---

## 3. Effective config (how the chain is consumed)

Two seams, both fed by the SAME `CaptureSettings.Load` chain (I-1):

### 3.1 Per-record seam (fresh — FIX-1)

`HandleRecordingStart` reloads effective config from disk on EVERY record start (`RecordingEngineHost.vb:191-215`): `CaptureSettings.Load(_configPath)` `:203` → `SyncWithOverlayConfig` `:204` → publish `_settings` `:205` → greppable echo `[RecordingEngine] effective config (fresh reload): …` `:211-215` → `NextRecordingConfig.MapSessionConfig` `:235-236`. FFmpegPath is re-resolved AFTER the reload so a fresh path is honored (`:217-225`).

**Consequence (locked):** everything in `MapSessionConfig` applies to the NEXT recording with no restart — audio group, FPS pacing, output path. This is regime A and it is already the reference behavior.

### 3.2 Engine-init seam (one-shot)

`InitializeRecordingEngine` (`RecordingEngineHost.vb:60-110`) snapshots once (`:68`), maps via `MapStartupConfig` (`:87-88`), and `RecordingEngine.Initialize(startup)` builds the persistent NVENC session (`RecordingEngine.vb:63-142`). `Initialize` is documented and enforced one-shot: "Called ONCE at process start. Subsequent calls throw" (`:50-55`, state guard `:68-69`); the encoder session is process-lifetime (`:57-61`).

**Consequence (locked):** the B-registry rows apply at init only. Changing them in config.json takes effect at the next ENGINE PROCESS start, not the next recording — unless and until the OWNER decides otherwise (Q5).

### 3.3 Acceptance echo layers (locked)

Every boundary must stay greppable — these lines are part of the contract, not logging garnish:

| Echo | Line | Proves |
|---|---|---|
| `[RecordingEngine] effective config (fresh reload)` | `RecordingEngineHost.vb:211-215` | per-record effective values |
| `[RecordingEngine] effective video config (startup)` | `RecordingEngine.vb:139` | init B-values + GOP independent of FPS |
| `capture method: requested → selected → actual` (+ GAP warning) | `:123-128` | honesty for C-regime capture |
| `pixel format: config='…' → runtime=BGRA8 → NVENC input=ARGB` | `:130-135` | BLOCKER P1-PIXFMT truth |
| `fps source = config.json Recording.current.fps = N (display refresh … diagnostics-only)` | `CaptureSession.vb:500` | FPS single-owner |
| `★ PIPELINE = LEGACY…` | `[Engine] Client.vb:258` | which pipeline actually ran |

---

## 4. Runtime boundaries (the regime table — locked)

| Regime | Meaning | Fields (today) | UI label required |
|---|---|---|---|
| **A · per-record (fresh)** | re-read from disk at every record start | `Audio.*` (8), `current.fps` (pacing+mux), `Paths.FFmpegPath`, output path | "applies next recording" |
| **B · init-time (frozen)** | read once into the one-shot NVENC session | `Recording.encoder`, `current.bitrate`, `current.encoder_preset`, `current.use_native_resolution/width/height`, engine.json `RateControl`, GOP (engine default 60 — never from FPS, V-CT1 fix) | **"engine restart required"** |
| **A→B span** | mapped at BOTH seams | `current.fps` (init `MapStartupConfig :142` → `FrameRateFps :110`; per-record pacing/mux `:495-500`) | see Q5 divergence note |
| **C · echo-only** | New Engine never consumes; logged honestly | capture method (requested→selected→actual), pixel format (BGRA/ARGB truth) | honest value + blocker note (P1-PIXFMT) |
| **LEGACY-only** | consumed by FFmpeg path only | `CaptureMethod` whitelist (`CaptureSettings.vb:493-496`), `FileFormat`, legacy pixel-format args, gdigrab/gfxcapture | "legacy pipeline only" |
| **RESERVED** | no runtime consumer | `replay_duration` (replay `not_implemented`, `UI_Engine.vb:524-534`) | disabled until wired |

**Engine selection itself is runtime-internal** (`_useNewEngine = True` at `RecordingEngineHost.vb:42`; on init failure `_useNewEngine = False` `:106` with stored reason `:103`; dispatch `[Engine] Client.vb:247-274`) — it has NO regime because it has NO canonical key (Q1).

**Validation boundary (flagged, not re-decided):** engine caps are `Validate()` FPS 1–240 / bitrate 1–200 Mbps / method whitelist (`CaptureSettings.vb:483-507`) while Overlay UI still allows FPS up to 800 and bitrate 500–150000 kbps (UI spec §11). Unifying the caps is a UI-spec acceptance item (#5), owned there — this contract only locks that `Validate()` is the runtime truth.

---

## 5. Open decisions Q1–Q6 (FACT → OPTIONS → IMPACT → RECOMMENDATION → OWNER DECISION REQUIRED)

> GLM does **not** decide here. Each RECOMMENDATION is a non-binding engineering view so the OWNER can decide fast; each decision box is intentionally left empty. Deciding any Q narrows the UI spec verdict (`WAIT FOR CONFIG`, UI spec v1.1) accordingly.

### Q1 — Engine selection: there is NO canonical key

**FACT**
- Runtime selection is hardcoded and automatic: `_useNewEngine = True` (`RecordingEngineHost.vb:42`); on init failure `_useNewEngine = False` (`:106`) with the failure reason stored (`:103`) and surfaced on legacy record starts (`:44-47` comment block); dispatch switch `[Engine] Client.vb:247-274`; legacy marker log `:258`.
- config.json has no key for it: the section list is exhaustive (`ConfigFileDto:379-390`) and `Recording` (`:359-372`) carries no engine-mode field. `NOT IMPLEMENTED as a user choice` (UI spec §2).
- The New Engine is structurally NVENC+ddagrab: `NvencEncoderBackend` constructed unconditionally (`RecordingEngine.vb:82`), backend hardcoded `DdagrabBackend` (`:76`).

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
- Mapping: non-null → `CaptureMethod` (`OverlayConfig.vb:466-468`); null → engine.json `CaptureMethod` value stands (default `ddagrab` `CaptureSettings.vb:58`; Engine UI writer `UI_Engine.vb:660-664`).
- New Engine: requested→selected→actual echo (`RecordingEngine.vb:123-128`) — `ddagrab`/empty = selected; anything else = **recorded GAP warning**, runtime still ddagrab; native `GfxCaptureBackend` NOT IMPLEMENTED (`IVideoCaptureBackend.vb:8` "future").
- Legacy path: whitelist + builders honor it (`CaptureSettings.vb:493-496`, `FFmpegCommandBuilderV2.vb:79-96`).

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
- **Preset: DONE (locked).** Canonical = config.json `current.encoder_preset` (1–7): unified apply `OverlayConfig.vb:455-457` → `MapStartupConfig :156-162` via the single mapper (`ConfigMigrator.MapNvencPresetInteger` → p1–p7, invalid → p4) → NVENC preset GUID. engine.json `Preset` is compat fallback ONLY (used when the config value is out of range, `:160-161`). Tests V-CT4/4b/5f. Matrix row upgraded to `✅ Unified @ PHASE 1`.
- **RateControl: NOT DONE.** engine.json is the ONLY source (`CaptureSettings.vb:61`, default `cbr`); NO config.json key exists (`RecordingSectionDto:359-372` has none); mapped `MapStartupConfig :144-146` → `EncoderConfig.RateControl` (`RecordingEngine.vb:108`) → NVENC `rateControlMode` (V-CT5). No UI anywhere.
- Both are regime B (init-frozen).

**OPTIONS**
- (a) Add config.json `Recording.current.rate_control` + unified-apply mapping + ConfigTruth test; keep engine.json read as compat fallback during the transition window (mirror the preset pattern), then delete per matrix §6.6.
- (b) Hardcode `cbr` and drop the knob until a UI wants it.
- (c) Leave the owner in engine.json permanently — contradicts Law #1; rejected by the matrix but listed for completeness.

**IMPACT**
- (a) closes matrix wire item #4 and makes RC the last encoder-group key to reach single-ownership; UI can expose RC later with the same restart-required label as preset/bitrate.
- (b) simplifies the schema but silently removes an advanced knob (CBR/VBR) that NVENC already honors — a product decision, not an engineering one.
- Deletion order matters: removing the engine.json key before the config wire exists = silent default fallback (matrix §7).

**RECOMMENDATION** (non-binding)
- (a), cloned from the preset precedent (it has tests, a mapper, and a fallback pattern already reviewed by the OWNER).

**OWNER DECISION REQUIRED**
- [ ] Add `rate_control` to config.json now, defer, or hardcode?
- [ ] If add: default value (`cbr`)? allowed values (cbr/vbr/…)? UI exposure timing?
- [ ] Transition window for the engine.json fallback — when may it be deleted (real-record ffprobe gate)?

### Q4 — PixelFormat: BLOCKER P1-PIXFMT (config not honored)

**FACT**
- engine.json `PixelFormat` is the only key (`CaptureSettings.vb:59`, default `nv12`); NO config.json key; no UI.
- Runtime truth: capture is BGRA8 (D3D11) → NVENC input `NV_ENC_BUFFER_FORMAT_ARGB` (`NvencResources.vb:107`); a GPU conversion layer (BGRA→NV12) is NOT implemented; the truth line reports the config value honestly and recording continues BGRA/ARGB (`RecordingEngine.vb:130-135`).
- The legacy FFmpeg path DOES honor pixel format (builders) — the semantics are pipeline-split.
- Matrix row: 🔴 BLOCKER recorded (P1-PIXFMT); UI spec requires any PixelFormat control on New Engine to display "runtime BGRA/ARGB" (§5).

**OPTIONS**
- (a) Implement the BGRA→NV12 GPU conversion in the New Engine (VIDEO phase — GLM/1 territory), then move the key to config.json and honor it.
- (b) Move the key to config.json NOW with the blocker label (matrix wire item #5 direction), honoring it only on the legacy path until (a).
- (c) Park/retire the key for the New Engine: accept BGRA/ARGB as the product default, keep nv12 meaning legacy-only.

**IMPACT**
- (a) changes capture semantics + perf + NVENC buffer management — explicitly gated by matrix §7 (real-record evidence), not a config-plumbing task.
- (b) fixes ownership but the New Engine would still not honor it — honest only with the blocker label.
- (c) is the smallest truth-preserving state; any UI showing a pixel-format choice without (a) is a labeled lie (UI spec §5).

**RECOMMENDATION** (non-binding)
- (c) for the stabilization window + schedule (a) in the VIDEO phase; if the OWNER wants the key visible, (b) with the blocker label — never a fake pass.

**OWNER DECISION REQUIRED**
- [ ] Accept BGRA/ARGB as the New Engine's product default for now, or require NV12 (schedule conversion)?
- [ ] If NV12 required: which phase owns the conversion layer (VIDEO/GLM/1) and what is the evidence gate?
- [ ] What does engine.json `PixelFormat` become meanwhile — legacy-only key, or retired from UI entirely?

### Q5 — Process-frozen fields: what exactly restarts?

**FACT (the frozen set)**
- Frozen at engine init (regime B, one-shot NVENC — `RecordingEngine.vb:50-61`, `:82-116`): codec (`:102`), bitrate (`:103-106`), preset (`:109`), rate control (`:108`), encode dimensions (`:91-99`, `:113-114`), GOP (engine default 60, `:83` — never derived from FPS since V-CT1).
- Fresh per record (regime A, `RecordingEngineHost.vb:191-236`): audio group, FPS pacing+mux (`CaptureSession.vb:495-500`), FFmpegPath, output path.
- **FPS spans both regimes**: mapped at init (`MapStartupConfig :142` → `FrameRateFps :110`) AND per-record (`MapSessionConfig :93` → pacing/mux). If config FPS changes after engine start, pacing/mux use the NEW value while the NVENC session keeps the INIT value until restart; the container's declared rate is the per-record value (authoritative for the mux, `CaptureSession.vb:490-492`).
- Restart contract today = engine process restart; there is also a resilience fallback (init failure → legacy, `:106`) whose restart semantics differ.

**OPTIONS**
- (a) Accept the restart contract explicitly: B-fields get an "engine restart required" badge (UI spec §15 item 4); document the FPS span in the same badge.
- (b) Design per-record NVENC re-init (Phase 4 scope — GLM/1 territory; NVENC session rebuild cost + risk).
- (c) Auto-restart the engine process when a B-field changes (lifecycle risk: kills in-flight state, overlaps supervisor logic).

**IMPACT**
- (a) unlocks UI: `[5]` preset/bitrate/resolution controls may go live WITH labels (UI spec §14 step 7 requires this decision).
- (b) is a genuine feature (re-init design) — out of this contract's scope, but the contract must state which option is chosen so UI labeling is not guessed.
- The FPS span is currently benign only because pacing/mux (per-record) own the container; if (b) ever lands, the init mapping must be revisited to avoid divergence.

**RECOMMENDATION** (non-binding)
- (a) now — zero runtime risk, matches the one-shot design; put (b) on the Phase 4 backlog as a design study, not a commitment.

**OWNER DECISION REQUIRED**
- [ ] Accept the restart contract (a) as the stabilization-branch product behavior?
- [ ] Exact badge list (which B-fields are labeled)?
- [ ] Is the FPS mid-process span (pacing=new, NVENC rate=init until restart) acceptable product behavior, or must FPS also be labeled restart-required?

### Q6 — engine.json end-state: what remains?

**FACT**
- Keys today (exhaustive, `CaptureSettings.Save()` `:176-191`): `ConfigVersion`, `CaptureMethod`, `PixelFormat`, `Preset`, `RateControl`, `FileFormat`, `FFmpegPath`, `HotkeyStart/Stop/Toggle`, `UseNativeResolution`, `CustomWidth`, `CustomHeight`.
- Post-PHASE-1 roles: `Preset` = compat fallback only; `UseNativeResolution`/`CustomWidth`/`CustomHeight` = dead duplicates whenever config.json exists (unified WINS); `Hotkey*` = DEAD (live system reads config.json `Hotkeys`, `HotkeyService.vb:41,49`); `FFmpegPath` = shadow copy (config wins when the file exists, `OverlayConfig.vb:517-519`); `CaptureMethod` = legacy writer + auto default; `RateControl`/`PixelFormat`/`FileFormat` = still genuinely consumed (RC by New Engine init; PF/FileFormat by the legacy path).
- Silent write-backs keep rewriting engine.json (`UI_Engine.vb:493` per record start, `:578-581` PREWARM) — engine.json is NOT frozen; it is a compat surface.
- Law #2 (matrix §1): engine.json must never store a user expectation that config.json also carries.

**OPTIONS (end-state definition)**
- (i) engine.json retains ONLY: engine-internal knobs not yet promoted (RateControl, PixelFormat, FileFormat until their decisions land), diagnostics, and migration/compatibility data (`ConfigVersion`, V2 migration block). Everything user-facing moves to config.json following matrix §8 order.
- (ii) Keep engine.json as a full parallel store (contradicts Law #1 — rejected by the matrix).
- (iii) Delete engine.json as soon as Q3/Q4 decide (premature — silent fallbacks, matrix §7).

**IMPACT**
- (i) gives a concrete deletion order: dead twins first (`Hotkey*`, resolution trio, `Preset` after the transition window), consumed-but-doomed keys after their Q3/Q4 decisions + evidence gates. It also caps the silent write-backs' blast radius: rewriting compat data is harmless; rewriting user expectations is the duplicate-class bug the matrix kills.
- Without a decided end-state, every future phase re-litigates which file wins — the contract's purpose is to end that.

**RECOMMENDATION** (non-binding)
- (i), with the deletion sequence tied to matrix §8 items and §6.6 evidence gates; write-backs (`:493`, `:578-581`) are retired together with the keys they preserve (UI spec §14 step 1 covers the Engine-UI writer).

**OWNER DECISION REQUIRED**
- [ ] Approve end-state (i) as the definition of "engine.json remains what"?
- [ ] Approve the deletion order (dead twins → transition-window keys → decision-gated keys)?
- [ ] Keep `CaptureMethod`/`FileFormat` in engine.json while the legacy pipeline exists — yes/no?

---

## 6. What this contract locks vs leaves open

**LOCKED by v1.0** (change only via §7 amendment):
1. Chain shape: config.json → canonical registry → `CaptureSettings.Load` (unified WINS) → two seams (`NextRecordingConfig`) → regimes.
2. Invariants I-1 (single apply-path, `CaptureSettings.vb:94-164`) and I-2 (mappings live only in `NextRecordingConfig`, test-compiled on Linux).
3. Store roles + exhaustive key inventories (§1) and the Overlay-never-writes-engine.json boundary.
4. Locked registry rows (§2.3) — FPS single-owner, preset single-mapper, bitrate/resolution/encoder/audio paths, FIX-1 freshness.
5. Regime table (§4) incl. the A→B FPS span and the echo acceptance layers (§3.3).
6. `Validate()` is the runtime truth for caps (FPS 1–240, bitrate 1–200 Mbps, method whitelist).

**OPEN (blocked on OWNER):** Q1 engine-selection key · Q2 api_capture writer + null semantics · Q3 rate_control key · Q4 PixelFormat blocker disposition · Q5 restart contract · Q6 engine.json end-state.
**OPEN (owned elsewhere, cross-referenced):** validation-cap unification + volume range (UI spec §11/§15) · native gfxcapture + NV12 conversion + per-record re-init (VIDEO/Phase 4, GLM/1).

## 7. Amendment protocol

1. Any status change to a LOCKED item requires: `path + method + line` evidence at the current HEAD + a ConfigTruth-pattern test (FAIL-first) + real-record ffprobe confirmation when the field reaches the mux (matrix §9 — same gate, no exceptions).
   - **Evidence path for the real-record layer:** the `--videocheck` kit (GLM/1 `cf47732`): `scripts/windows-phase1-video-validation.ps1` drives the canonical chain on the OWNER's Windows machine with ffprobe asserts; results ladder in `docs/PHASE1_VIDEO_VALIDATION_STATUS.md`. Linux suites prove the mapping; the kit produces the runtime/output truth — both layers feed any amendment.
2. Wire/move/delete lands as separate ordered commits: owner change → mapping path → test → echo log → ffprobe → then duplicate deletion (matrix §9).
3. This document never re-decides ownership (matrix's job) and never designs UI (UI spec's job) — it locks the runtime contract between them. Conflicts resolve in that order: matrix > contract > UI spec, then re-amend.
4. Every amendment bumps the Revisions block with the anchor commit and a one-line delta.

