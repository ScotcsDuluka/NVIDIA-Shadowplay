# CONFIG OWNERSHIP MATRIX — NVIDIA ShadowPlay

- Route: **PHASE 0 — CONFIG TRUTH** (rebaseline: CONFIG → VIDEO → UI LOGIC → ENGINE/API MODE → REAL VALIDATION)
- Anchored at commit: **`99a5dc0`** (PHASE 0 CONFIG TRUTH fix wave: CT-4 + FIX-1 + FIX-2)
- Owner: ScotcsDuluka · Matrix drafted by OWNER, evidence column verified against source by GLM/2
- Update rule: every status change in this file MUST ship with `path + method + line` evidence and a test (see §9)

---

## 1. The Law

> **One user setting → One canonical owner → One runtime mapping path.**

1. **`config.json` is the ONLY user-facing store.** Every value the user sets and
   expects to affect recording lives here. It is written atomically as a single
   file (`AppSettings.vb:1023` `Save()`, per-PID temp + `.bak` + rename, comment
   block `:856-861`) and it **WINS** everywhere: inside the Engine's
   `CaptureSettings.Load` the unified config.json is applied on top of
   engine.json and early-returns (`CaptureSettings.vb:91-160`, unified-WINS at
   `:101-116`).
2. **`engine.json` is for engine-internal implementation only** — backend
   specifics, diagnostics, migration/compatibility data. It must never be the
   place where a user expectation is stored while config.json also carries the
   same intent (that is exactly the duplicate-class this matrix kills).
3. **Configuration Truth Rule:** a config value is not "real" until **runtime +
   ffprobe** prove it. Greppable runtime echo + ffprobe of the produced file are
   the acceptance layers (CT-4 + `[RecordingEngine] effective config (fresh
   reload)` log line, `RecordingEngineHost.vb`, FIX-1).

## 2. Where things stand (anchored @ `99a5dc0`)

Phase 0 fix wave already landed:

- **FIX-1** — `RecordingEngineHost.HandleRecordingStart` reloads the effective
  config from disk before building `SessionConfig`
  (`CaptureSettings.Load` + `SyncWithOverlayConfig` + publish `_settings`, the
  exact legacy trio from `UI_Engine.vb:369-371`).
- **FIX-2** — `UI_Engine.HandleEngineConfigChanged` (`UI_Engine.vb:608-625`)
  reloads `_settings` for real on `engine_config_changed` (dispatched from
  `[Engine] Client.vb:179`).
- **CT-4** — deterministic stale-config-reload contract
  (`Engine.ConfigTruth.Tests`), proven FAIL pre-fix / PASS post-fix. The
  next-recording composition lives in
  `Engine/Engine/[API]/NextRecordingConfig.vb:56-98`.

Consequences for the matrix below: the **Audio group is ✅ fresh** (per-record).
The **encoder group is consumed but frozen at process init** (NVENC one-shot —
§3 "Runtime regime" column), and FPS/Resolution/CaptureMethod/PixelFormat are
**not consumed by the New Engine path at all** (display-driven).

### Runtime regimes (how to read the matrix)

| Regime | Meaning | Code path |
|---|---|---|
| **A · per-record (fresh)** | Value read from disk at every record start → new value applies to the NEXT recording | `NextRecordingConfig.BuildSessionConfig` → `SessionConfig` (FIX-1) |
| **B · init-time (frozen)** | Value read once at process init into `EngineStartupConfig`; changing it later does nothing until restart | `RecordingEngineHost.vb:60-106` snapshot → `RecordingEngine.vb:58,78-92` NVENC one-shot |
| **C · ignored** | New Engine path never reads it (display-driven / backend-selected / hardcoded) | `RecordingEngine.vb:71` (`DdagrabBackend(_logger)` no-args), `CaptureSession.vb:541-543` |

## 3. Matrix

Legend: ✅ keep · ⚠️ verify/duplicate exists · 🟡 acceptable for now · 🔴 must
change (wire / unify / move). "Source" = file that owns the key today.

### Recording (config.json `Recording` section — `AppSettings.vb:359-372`)

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `encoder` | config.json | ✅ New Engine | B (init) | config.json | ✅ Keep | `AppSettings.vb:361` → `OverlayConfig.vb:435` → `CaptureSettings.vb:26` → `RecordingEngineHost.vb:60-106` → `RecordingEngine.vb:80` |
| `encoder_now` | config.json | ❓ UI/runtime state | — | config.json | ⚠️ ตรวจว่าเป็น state หรือ config | write: `AppSettings.vb:363,783` · read: `:818` — transition mirror, confirm nothing runtime depends on it |
| `active_preset` | config.json | ⚠️ duplicate of `my_presets`+`current` selection | — | config.json | 🟡 Keep as product preset | `AppSettings.vb:365,784,819`; audit: dead in engine path |
| `current.fps` | config.json | ✅ WIRED (PHASE 1) — pacing + mux + NVENC frameRateNum | A→B | config.json | ✅ Wired @ PHASE 1 | `NextRecordingConfig.MapSessionConfig` (TargetFps) + `MapStartupConfig` (Fps); pacing `CaptureSession.vb` CFR loop; NVENC `NvEncParamBuilder.BuildInitializeParams`; tests V-CT1/1b/1c; pre-wiring bug: `CaptureSession.vb:489/:541` used display refresh |
| `current.bitrate` | config.json | ✅ WIRED (PHASE 1) — into NV_ENC_CONFIG.rcParams.averageBitRate | A→B | config.json | ✅ Wired @ PHASE 1 | loader kbps→bps `CaptureSettings.vb:230` → `MapStartupConfig` → `EncoderConfig.BitrateBps` → `NvEncParamBuilder.ApplyVideoSettings` (native struct); tests V-CT3/3b/5d; pre-wiring: encodeConfig=IntPtr.Zero |
| `current.encoder_preset` | config.json | ✅ WIRED (PHASE 1) — single mapper → p1-p7 GUID | A→B | config.json | ✅ Unified @ PHASE 1 | unified apply `OverlayConfig.ApplyUnifiedToCaptureSettings` (encoder_preset → NvencPreset, was DEAD) → `MapStartupConfig` via `ConfigMigrator.MapNvencPresetInteger` (single mapper) → `NvEncodeAPI.PresetGuidForKey`; tests V-CT4/4b/5f; engine.json `Preset` = compat fallback only |
| `current.use_native_resolution` | config.json | ✅ WIRED (PHASE 1) — native/custom branch | A→B | config.json | ✅ Wired @ PHASE 1 | `MapStartupConfig` → `EngineStartupConfig.ResolveEncodeDimensions` → `EncoderConfig.EncodeWidth/Height` → NVENC encodeWidth/Height (GPU scale, maxEncode=input); tests V-CT2/2b/2c |
| `current.width` | config.json | ✅ WIRED (PHASE 1) when native=false | A→B | config.json | ✅ Wired @ PHASE 1 | same path as use_native_resolution; oversized → loud ArgumentException (no silent desktop fallback) |
| `current.height` | config.json | ✅ WIRED (PHASE 1) when native=false | A→B | config.json | ✅ Wired @ PHASE 1 | same path as use_native_resolution |
| `replay_duration` | config.json | ❌ Replay not implemented | — | config.json | 🟡 Reserved | persisted `AppSettings.vb:368` |
| `my_presets.*` | config.json | ✅ UI preset data | — | config.json | ✅ Keep | `AppSettings.vb:351-357,369,794-847` |

### Video

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `CaptureMethod` | engine.json | ⚠️ requested→selected→actual logged (PHASE 1); only production backend = ddagrab | C | config.json | 🟡 Echo + gap recorded | unified apply maps `Recording.api_capture` → `CaptureMethod` (`OverlayConfig.vb:458-460`) → `MapStartupConfig.RequestedCaptureMethod` → evidence log `RecordingEngine.vb` (gfxcapture = recorded GAP, not silently accepted) |
| `PixelFormat` | engine.json | ❌ config nv12 NOT honored — runtime BGRA/ARGB (BLOCKER P1-PIXFMT, logged) | C | config.json | 🔴 BLOCKER recorded | engine.json `CaptureSettings.vb:59`; runtime chain BGRA8 (D3D11 capture) → NVENC `NV_ENC_BUFFER_FORMAT_ARGB` (`NvencResources.vb:107`); conversion layer (capture→NVENC input) not implemented in PHASE 1 — truth line logged, no fake pass |
| `FPS` | split | ✅ config.json is the single FPS owner (PHASE 1) | A→B | config.json | ✅ Wired @ PHASE 1 | config.json `AppSettings.vb:336` → unified apply → `MapSessionConfig.TargetFps` + `MapStartupConfig.Fps`; display refresh demoted to diagnostics; FPS→GOP mapping REMOVED (`RecordingEngineHost.vb:96-98` pre-wiring bug) |

### Encoder

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `Preset` | engine.json | ⚠️ compat fallback only (PHASE 1) — config `encoder_preset` wins via the single mapper | B | config.json | 🟡 Fallback (delete later per §6.6) | `CaptureSettings.vb:60` → `MapStartupConfig` fallback branch; config owner wired @ PHASE 1 (V-CT4) |
| `RateControl` | engine.json | ✅ WIRED (PHASE 1) — mode into NV_ENC_CONFIG.rcParams.rateControlMode | B | config.json | 🟡 Move later | `CaptureSettings.vb:61` → `RecordingEngine.vb` → `NvEncParamBuilder.ApplyVideoSettings`; **still read from engine.json — do NOT delete engine.json key yet** |

### Output

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `FileFormat` | engine.json | ❌ New path hardcodes `.mp4` | C | config.json | 🔴 Move/Map | engine.json `CaptureSettings.vb:62`; only legacy consumer `GenerateOutputFilename` `CaptureSettings.vb:469-471`; new path structurally MP4 (`LiveMuxSession.vb:18,306`) |

### Paths

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `FFmpegPath` | BOTH / split | ✅ runtime (fresh after FIX-1) | A | config.json → `Paths` | 🔴 engine copy should die | config.json `AppSettings.vb:59` (Paths); engine.json `CaptureSettings.vb:63`; re-resolve `OverlayConfig.ResetResolvedPath` + fresh reload `RecordingEngineHost.vb` (FIX-1) |

### Hotkeys

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `HotkeyStart` | engine.json | ⚠️ another system exists | — | config.json → `Hotkeys` | 🔴 Move | engine.json `CaptureSettings.vb:64,185,285`; live system reads config.json dict: `HotkeyService.vb:41,49` |
| `HotkeyStop` | engine.json | ⚠️ another system exists | — | config.json → `Hotkeys` | 🔴 Move | `CaptureSettings.vb:65,186,286`; `HotkeyService.vb:41,49` |
| `HotkeyToggle` | engine.json | ⚠️ another system exists | — | config.json → `Hotkeys` | 🔴 Move | `CaptureSettings.vb:66,187,287`; `HotkeyService.vb:41,49`; V2 migration twin `ConfigMigrator.vb:63-65,189` |

### Resolution

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `UseNativeResolution` | engine.json | split | C | config.json → `Recording.current` | 🔴 Duplicate | engine.json `CaptureSettings.vb` + config.json `AppSettings.vb:339` |
| `CustomWidth` | engine.json | ❌ New path ignored | C | config.json → `Recording.current.width` | 🔴 Unify | engine.json model vs `AppSettings.vb:340` |
| `CustomHeight` | engine.json | ❌ New path ignored | C | config.json → `Recording.current.height` | 🔴 Unify | engine.json model vs `AppSettings.vb:341` |

### Audio (config.json `Audio` — the POST-99a5dc0 reference group)

| Field | Current Source | Current Runtime Use | Regime | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|---|
| `SystemAudioEnabled` | config.json | ✅ SessionConfig | A | config.json | ✅ Keep | `AppSettings.vb` Audio section → `OverlayConfig.vb:435` → `NextRecordingConfig.vb:88` (fresh, FIX-1) |
| `MicEnabled` | config.json | ✅ SessionConfig | A | config.json | ✅ Keep | `NextRecordingConfig.vb:90` |
| `SystemAudioVolume` | config.json | ✅ SessionConfig | A | config.json | ✅ Keep | `NextRecordingConfig.vb:89` |
| `MicVolume` | config.json | ✅ SessionConfig | A | config.json | ✅ Keep | `NextRecordingConfig.vb:91` |
| `MicDeviceName` | config.json | ✅ mapped | A | config.json | ✅ Keep | `NextRecordingConfig.vb:93` |
| `MicDeviceId` | config.json | ✅ mapped | A | config.json | ✅ Keep | `NextRecordingConfig.vb:92` |
| `TrackMode` | config.json | ✅ mapped | A | config.json | ✅ Keep | `AppSettings.vb` → `CaptureSettings.AudioTrackModeEnum` → `NextRecordingConfig.vb:94-95` |
| `AudioClockMode` | config.json | ✅ mapped, **now fresh** | A | config.json | ✅ Keep | `NextRecordingConfig.vb:96`; stale before 99a5dc0 (CT-4) |

### UI / Privacy / Overlay / Notifications (config.json — already single-owner)

| Field | Current Source | Current Runtime Use | Proposed Owner | Status | Evidence |
|---|---|---|---|---|---|
| `Language` | config.json | ✅ | config.json | ✅ Keep | `AppSettings.vb` UI section (`ConfigFileDto:382`) |
| `Theme` | config.json | ✅ | config.json | ✅ Keep | `ConfigFileDto:382` |
| `UseWindowsSnip` | config.json | ✅ | config.json | ✅ Keep | UI section |
| `DesktopCaptureEnabled` | config.json | ✅ | config.json | ✅ Keep | Privacy section (`ConfigFileDto:384`) |
| `UseOverlayEnabled` | config.json | ✅ | config.json | ✅ Keep | Overlay section (`ConfigFileDto:385`) |
| `SlotCount` | config.json | ✅ | config.json | ✅ Keep | Notifications section (`ConfigFileDto:386`) |
| notification toggles | config.json | ✅ | config.json | ✅ Keep | Notifications section |

## 4. Supplementary rows (found during the evidence pass)

| Field | Current Source | Note | Status |
|---|---|---|---|
| `Recording.api_capture` | config.json | persisted (`AppSettings.vb:371`, "ddagrab / gfxcapture / GDIGrab / null") but New Engine backend-selects — same regime as CaptureMethod | 🔴 wire or park with CaptureMethod |
| config.json `Hotkeys` dict | config.json | already exists as a section (`ConfigFileDto:387`) and is the one the live HotkeyService reads — the engine.json Hotkey* keys are the DEAD side of the duplicate | 🔴 confirms move direction |
| aac `320k`/`48000` | hardcoded | `LiveMuxSession.vb:204-207` — not user-facing yet; when audio bitrate becomes a setting it MUST get a config.json owner first | 🟡 note for AUDIO phase |
| Priority precedence | engine.json | `CaptureSettings.Load` chain `:91-160` (unified WINS + early return `:101-116`) is THE load-order contract — never add a second apply-path | 📌 architecture invariant |

## 5. Target structures

```
config.json (single user-facing store; atomic single-file save)
│
├── Recording
│   ├── encoder
│   ├── active_preset
│   ├── current
│   │   ├── fps
│   │   ├── bitrate
│   │   ├── encoder_preset
│   │   ├── use_native_resolution
│   │   ├── width
│   │   └── height
│   ├── replay_duration
│   └── my_presets
│
├── Audio
├── Paths
├── Hotkeys
├── UI
├── Privacy
├── Overlay
└── Notifications
```

```
engine.json (engine-internal implementation only)
│
├── engine-internal options
├── backend-specific options
├── diagnostics
└── migration / compatibility data
```

## 6. Phase 0 walking order (do not reorder)

1. **Owner ของ field ชัด** — this matrix is the registry; every field has exactly one proposed owner.
2. **Mapping ชัด** — one documented mapping path per field (reference: `NextRecordingConfig.vb` for SessionConfig; `OverlayConfig.ApplyUnifiedToCaptureSettings:435` for the unified overlay).
3. **Runtime ใช้ owner เดียว** — kill parallel reads only after 1+2 hold for that field.
4. **Test** — every rewired field gets a deterministic contract test in `Engine.ConfigTruth.Tests` (pattern: CT-4, FAIL-first).
5. **Real record** — Configuration Truth Rule: runtime log echo + ffprobe on the OWNER's Windows machine.
6. **ค่อยลด/ลบ duplicate** — only after 1-5 are green for that field.

## 7. Do-not-rush rules (ห้ามทำแบบรีบ ๆ)

- **engine.json must NOT be deleted now.** `RateControl` / `Preset` (and the
  V2 migration block) are still genuinely read by the New Engine at init
  (`RecordingEngine.vb:86-87`, `ConfigMigrator.vb`) and config.json is not
  wired for every field yet. Removing the source before the wire exists =
  silent default fallback (see `RecordingEngine.vb:80-87` fallback chain).
- The **encoder group stays frozen per process** until Phase 4 designs a
  per-record NVENC re-init or accepts the restart contract explicitly. Do not
  "fix" it ad hoc inside the recording path (NVENC session one-shot is by
  design: `RecordingEngine.vb:58,78-92`).
- FPS / Resolution / CaptureMethod / PixelFormat are **display-driven by
  design today** (`RecordingEngine.vb:71`, `CaptureSession.vb:541-543`).
  Wiring them = changing capture semantics, not just config plumbing — belongs
  to VIDEO phase, gated by real-record evidence.
- Never introduce a SECOND apply-path on top of `CaptureSettings.Load`
  (unified-WINS chain `:91-160`). One load, one winner, one echo log.

## 8. Wire checklist (🔴 rows → ordered backlog)

1. `Paths.FFmpegPath` — make config.json the only source; engine copy dies (lowest risk: already fresh via FIX-1).
2. `Hotkeys` — delete engine.json keys after confirming HotkeyService is the only consumer (`HotkeyService.vb:41,49`) and `ConfigMigrator.vb:189` mapping is retired.
3. `current.encoder_preset` ⊕ engine.json `Preset` — unify (one key, one mapping).
4. `RateControl` — move to config.json (keep engine.json read as compat fallback during transition window, then delete per §6.6).
5. `CaptureMethod` / `PixelFormat` / `FileFormat` — move to config.json AND wire into New Engine (VIDEO-phase semantics).
6. `current.fps` — wire (VIDEO phase, display-refresh coupling documented above).
7. `current.bitrate` + resolution group — wire (Phase 4: per-record NVENC re-init design or explicit restart contract).
8. `encoder_now` — decide state-vs-config (likely: UI state only, never read by engine).

## 9. How to change this matrix

- Change a Status only with: (a) `path + method + line` evidence at the current
  HEAD, (b) a test that proves the new behavior (ConfigTruth pattern), (c) a
  real-record ffprobe confirmation when the field reaches the mux.
- Every wire/move lands as: owner change → mapping path → test → echo log →
  ffprobe → THEN duplicate deletion (in that order, separate commits).
