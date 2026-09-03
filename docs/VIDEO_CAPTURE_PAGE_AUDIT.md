# AUDIT — [5] Video Capture page (Overlay Settings) — heavy trace

- **Primary anchor:** `75917ef` (state audited line-by-line)
- **Re-anchored & re-verified:** `306bb30` — mid-audit, upstream landed the "Wave 2 UI honesty" commits (`2915801`, `1d201fb`, `a1b814c`, merges `0961729`/`306bb30`) which **fixed both CRITICAL findings of this audit** (§4) and one MEDIUM. Every finding below carries a `STATUS at 306bb30` line; citations are at `306bb30` numbering except §4 + Appendix A/B which are labeled `(75917ef numbering)`.
- **Target:** `Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/[index]/[Base]/[Main Menu]/[Settings]/[5] Video Capture.vb` (2,439 lines at HEAD) + its Designer (2,165 lines)
- **Method:** full file read; every config write/read site grepped and cross-checked; runtime behavior proven with an executed VB repro (Appendix A); engine-side consumption traced in `Engine/Engine/[Integration]/OverlayConfig.vb` and `[Engine] Client.vb`.
- **Scope guard:** doc-only. No code changed. Fix implementation belongs to a code-owning agent / OWNER.
- **Recent-change lens:** reworked *today* by `19088cd` ("Capture", +71 lines + DTO type change), `05af7d5` + `75917ef` ("UI", engine-mode semantics + API Capture Dropdown), then fixed/refined by Wave 2. The heaviest findings were introduced by `19088cd` and fixed by `a1b814c` a few hours later.

---

## 0. STATUS AT RE-ANCHOR `306bb30` (read this first)

| ID | Finding | Severity | STATUS at 306bb30 |
|---|---|---|---|
| C-1 | Overlay write path: `Save()` dies silently on String `api_capture` | CRITICAL | ✅ **FIXED** by `a1b814c` — DTO restored to `As String = Nothing` (`AppSettings.vb:371`) |
| C-2 | Engine read path: numeric `api_capture` breaks nested video parse | CRITICAL | ✅ **FIXED** by the same restore (files now serialize strings/null) — residual risk on legacy files, see §0.2 |
| H-1 | UI shows applied state the disk never received | HIGH | ✅ resolved with C-1 (silent-swallow policy still open → D-2) |
| H-2 | Engine diagnostics text "no UI writer; Q2 open" is stale | HIGH | 🔴 **OPEN** — `UI_Engine.vb:1374` still prints it |
| H-3 | One key (`api_capture`), two semantics (regime + FFmpeg filter) | HIGH | 🔴 **OPEN** — and now disk-persistent; `GetEngineModeKey()` :2397 unchanged |
| H-4 | FormClosing re-arms the debounced save instead of flushing | HIGH | 🔴 **OPEN** — :630-647 unchanged |
| M-1 | FPS cap schisma (UI 800 vs engine 240 + internal table conflicts) | MEDIUM | 🔴 **OPEN** — :18/:135 vs `CaptureSettings.vb:485` |
| M-2 | My-preset values not persisted at click time | MEDIUM | 🔴 **OPEN** |
| M-3 | Replay: ungated save + dead `BUFFER_DURATION` TCP message | MEDIUM | 🟡 **HALF-FIXED** — TCP send removed by W2-5 (`1d201fb`); the ungated `Save()` at :290 remains |
| M-4 | `PREWARM_FFMPEG` payload carries stale encoder name "60" | MEDIUM | 🔴 **OPEN** |
| M-5 | 5 permanently-disabled API placeholders + dead OBS card | MEDIUM | 🔴 **OPEN** (D-4) |
| M-6 | `_selectedApiKey` session shadow state diverges from disk | MEDIUM | 🟡 **LARGELY ADDRESSED** by Wave 2 per-mode caches (:777-778, :2335-2349); residual noted in M-6 |
| L-1..L-9 | Informational items | LOW | unchanged (L-3 dropped — its subject `BUFFER_DURATION` is gone) |

### 0.1 Why the CRITICALs were invisible to CI (FACT)

The green suites hand-write contract-shaped fixtures: `P3UIContractTests.vb:282` writes `"api_capture": "ddagrab"` (String) directly into a config.json fixture. No test exercised the real Overlay `Save()` → engine `Load()` round-trip (still true at 306bb30 — `git grep BuildRecordingDto -- *Tests*` is empty → R-4 remains open). Wave 2 added 8 W2-H pins (`Engine.ConfigTruth.Tests` 30/30) but none covers that seam.

### 0.2 Residual risk on installs that lived through the broken window (FACT)

Config files written between `19088cd` and `a1b814c` contain a **numeric** `"api_capture": 0` (the only value the broken Integer DTO could emit — Appendix A case C). On such files today:

- Overlay: `TryDeserializeConfigDto` (`AppSettings.vb:723-738`, strict String field) throws → returns Nothing → flat-shape apply / `.bak` recovery path runs (`TryReadBackupText`, `AppSettings.vb:742-752`); the next successful `Save()` rewrites the key as string/null and self-heals.
- Engine: `TryParseNestedRecording` (`OverlayConfig.vb:330-341`) throws on the same key → Nothing → flat fallback → FPS/bitrate/resolution/CaptureMethod read as **defaults** until the Overlay next saves.

Expected impact: one-time settings degradation/reset per affected install, self-healing on the first save — no crash. Worth a line in release notes.

---

## 1. Page identity and inventory

### 1.1 Numbered-page mapping (FACT)

The `[n]` prefix is the Overlay **Settings-page inventory** in `[Main Menu]/[Settings]/`:

| # | Page | Notes |
|---|------|-------|
| [0] | Settings - UI Main | container |
| [1] | Connect | |
| [2] | Overlay Hub | |
| [4] | Keyboard Shortcut | |
| **[5]** | **Video Capture** | this audit |
| [6] | Audio Capture | |
| [7] | Notifications | audited in `docs/NOTIFIER_SLOT_AUDIT.md` (commit `20c3d8d`) |
| [9] | Privacy Control | |

Numbers `[3]` and `[8]` do not exist in the tree (naming gap, informational).

### 1.2 Class / controls (FACT)

- Class `Base_RecordingsSet` (line 8). Engine-mode cards: `Engine_Mode1_Text = "FFmpeg Capture"` (Designer:479), `Engine_Mode2_Text = "Duluka Capture"`, `Engine_Mode3_Text = "OBS Capture"`. Capture-API row: `API_Box` Label, initial text `%API%` (Designer:33, :386).
- Timers: `Quality` — WinForms timer, **Interval = 200 ms** (Designer:1859), polls `Recording.Preset` and re-asserts lock/color state (`Quality_Tick`); `Recoed_IF` — timer `Enabled = True` (Designer:236, interval = default 100 ms), polls `Base.ReplayValue/RecordValue` to swap `Panel_SET` ↔ `captrueblock` during capture (`ALTZ_Tick`); `_saveSettingsTimer` — 300 ms debounced save (:1174); `_copyResetTimer` — 1500 ms clipboard-button feedback.
- UI→Engine TCP channels used by this page: `PREWARM_FFMPEG` (:561), `engine_config_changed` ("video") (:649, :2445). (The third historical channel `BUFFER_DURATION` was removed by W2-5.)
- Form is used through the **VB default-instance pattern** — e.g. `[1] Main Menu.vb:337-344` references `Base_RecordingsSet.FPS_BOX` directly — so the instance (and its field state) lives for the whole app session.

### 1.3 Config keys touched by this page (FACT, keyed to CONFIG_OWNERSHIP_MATRIX)

| Key (config.json `Recording.*`) | Write sites in this page | Immediate Save? |
|---|---|---|
| `encoder` + `encoder_now` | `ApplyEncoderSelection` (:821-822), `SaveSettingsNow` (:1193-1196), `EncoderMenuItem_Click` (:2200) | encoder click = yes |
| `active_preset` | preset clicks (:1273/:1281/:1289/:1302/:1323/:1331/:1339/:1347/:1355), `vdo_resetall_Click` (:728) | yes |
| `current.fps` | `SaveSettingsNow` (:1198) + My-preset mirroring (:1219-1232) | debounced 300 ms |
| `current.bitrate` | `SaveSettingsNow` (:1199) | debounced |
| `current.encoder_preset` | `SaveSettingsNow` (:1201-1203), `PMenuItem_Click` | debounced |
| `current.use_native_resolution/width/height` | `ApplyResolutionSelection` (:1100-1112), `SaveSettingsNow` (:1205-1216) | debounced |
| `replay_duration` | `TrackBar_Replaylast_MouseUp` (:289) | **yes, ungated** (M-3) |
| `my_presets.{low,medium,high}.*` | `SaveMyPresetValues` (:699-718), `SaveSettingsNow` (:1218-1232) | debounced |
| `paths.ffmpeg_path` | `LoadAPIRECORD` (:557) — UI writes a Paths key as a load side effect | rides next Save |
| **`api_capture`** | `ApiMenuItem_Click` (:2305, :2311), `ApplyEngineMode` (:2425, :2428) | yes — was the C-1 breakage, now fixed |

---

## 2. As-built data flow (at 306bb30)

### 2.1 Load pipeline (`LoadAPIRECORD`, :531-581)

`DetectNativeResolution` → read `_currentEncoderName` from `cmbEncoder.Text` **before** encoders are populated (:542-543) → `SetupTrackBar` → `AppSettings.DetectHardware()` if needed → `FindFFmpegPath` → **writes `Paths.FFmpegPath`** (:557) → `ClearEncoderAvailabilityCache` → `PREWARM_FFMPEG <path>|<encoder>` (:561, encoder name is the stale Designer text `"60"`, Designer:663 — M-4) → `PopulateEncoderDictionary` → `SelectSavedOrBestEncoder` → `LoadResolutionBox` → `LoadSettings` → `UpdateUIFromPreset` → `UpdateEngineModeUI` → `UpdateApiBoxDisplay` → `UpdateCommandPreview`.

### 2.2 Save pipeline

Two paths funnel into `AppSettings.Instance.Save()`:
1. **Debounced:** control change (editable preset only) → `SaveCurrentSettings` (:1173-1181) → 300 ms WinForms timer → `SaveSettingsNow` (:1191-1238) → `Save()`.
2. **Immediate:** preset clicks, replay MouseUp, encoder click, API/engine-mode clicks → `Save()` directly.

`FormClosing` (:630-655) disposes the debounce timer (:632-637), then calls `SaveCurrentSettings()` again (:644 — which **re-creates** the timer), then `Save()` (:647), then TCP `engine_config_changed "video"` (:649).

**Every one of these paths executes `AppSettings.Save()` → `BuildRecordingDto`.** At the 75917ef state, the single DTO defect (§4) therefore silenced the whole page; after `a1b814c` the DTO is String again and the pipeline works.

### 2.3 Preset system (FACT)

- Two groups: NVIDIA (`Low/Medium/High/Custom`, hardcoded values at :1249-1254) and My (`MyLow/MyMedium/MyHigh` editable + `Recommended/Maximum` locked).
- `IsEditablePreset()` = `Custom|MyLow|MyMedium|MyHigh` (:467-470). Gates: FPS menu (:2058-2060), P menu (:2205-2207), bitrate autosave (:1004-1006). Resolution menu gates on `Preset = "Custom"` only (:2099). Encoder menu and engine-mode/API menus are **never preset-gated** (deliberate global-surface pattern; see D-3).
- Locking is **visual-only** (`ApplyControlLockState` :330-346 strikes out text and changes cursor — it does not disable controls or detach handlers); only `TrackBar_BITRATE.Enabled` is a real disable (:1561/:1579). Handler-level self-gating covers FPS/P/Resolution; the replay trackbar is the one control with neither lock nor gate (M-3).
- `ApplyMy*Preset` mutates `Recording.FPS/Bitrate/EncoderPreset` in the model **without a Save** (e.g. :1380-1382) — the click handler saved only the preset *name* one line earlier (:1323). Values reach disk only via the next unrelated save (M-2).

### 2.4 Engine-mode radio + API Capture dropdown (FACT; introduced `19088cd`, reworked `05af7d5`+`75917ef`, refined by Wave 2)

- `GetEngineModeKey()` (:2397-2402): reads `Recording.APICapture`; `"ddagrab"` → mode `ddagrab`; **everything else** (`Nothing`, `"0"`, `"1"`, `"gdigrab"`, `"gfxcapture"`, garbage) → mode `ffmpeg`.
- `ApplyEngineMode("ffmpeg")` sets `APICapture = Nothing` (:2425, comment: legacy pipeline uses its canonical/default); `ApplyEngineMode("ddagrab")` sets `"ddagrab"` (:2428). Then `Save()` (:2438) + TCP `engine_config_changed` (:2445).
- API dropdown (:2254-2350) shows **two disjoint item sets** depending on mode:
  - ddagrab mode: 6 native-API items, only `dxgi_desktop_duplication` enabled, the other 5 are `Enabled = False` placeholders (:2261-2266).
  - ffmpeg mode: `ddagrab` / `gdigrab` / `gfxcapture`, all enabled (:2268-2270).
- `ApiMenuItem_Click` (:2296-2316): ffmpeg mode → writes the raw key into `APICapture` (:2305) + `Save()` (:2306); ddagrab mode → maps `dxgi_desktop_duplication` → writes `"ddagrab"` (:2311) + `Save()` (:2312).
- Shadow state, after Wave 2: `_selectedDulukaApiKey` / `_selectedFFmpegApiKey` (:777-778) cache the last selection **per mode**; `GetSelectedApiKey()` (:2335-2350) only trusts the ffmpeg cache when it matches the configured value — display/config divergence is now mostly closed (residual: the ddagrab-mode cache is still session-sticky and never re-hydrated).
- OBS card (`Engine_Mode3`) is present and visible but its click is a no-op Debug line (:2391-2395) and its colors are forced inactive everywhere (M-5).

---

## 3. Runtime boundary: who reads `api_capture`, and how (FACT)

| Side | Site | Declared type | Behavior |
|---|---|---|---|
| Overlay — in-memory model | `AppSettings.vb:50` (`Recording.APICapture`) | `String = Nothing` | UI reads/writes this |
| Overlay — file DTO | `AppSettings.vb:371` (`RecordingSectionDto.api_capture`) | `String = Nothing` (`19088cd` briefly made it `Integer = 1`; restored by `a1b814c`) | `BuildRecordingDto` assigns model → DTO; `ApplyRecordingDto` reverse |
| Engine — nested video parse | `OverlayConfig.vb:81` (`VideoConfig.api_capture`) | `String = Nothing` | `TryParseNestedRecording` :335 `Deserialize(Of VideoConfig)` with `_jsonOpts` (:283-288, **no NumberHandling**) |
| Engine — flat fallback | `OverlayConfig.vb:109` (`RecordingSettings.APICapture`) | `String = Nothing` | used only when nested parse returns Nothing; :497-498 `s.CaptureMethod = r.APICapture.ToLowerInvariant()` |
| Engine — capture settings | `CaptureSettings.vb:211-214` | `String` | `APICapture` → `CaptureMethod` if non-empty |
| Engine — diagnostics | `UI_Engine.vb:1370-1374` | `String` | prints requested API; text still says **"no UI writer; Q2 open"** — stale, H-2 |
| Contract tests | `P3UIContractTests.vb:282` | — | fixture hand-writes `"api_capture": "ddagrab"` (String) and asserts `CaptureMethod = "ddagrab"` |

The schema contract (per the test fixture, the doc comment on :370 "ddagrab, gfxcapture, gdigrab, or null (auto)", and the engine's String fields) says **`api_capture` is a nullable string** — which is again true at HEAD after `a1b814c`.

---

## 4. CRITICAL — the `api_capture` type schisma (HISTORICAL RECORD, `75917ef numbering`; FIXED by `a1b814c`)

> Everything in this section documents the state found at the primary anchor `75917ef`. It is preserved verbatim as evidence because (a) it explains the C-2 residual risk in §0.2, (b) it justifies R-4 (the missing round-trip test), and (c) the same failure pattern recurs any time a DTO type drifts — the swallow-and-continue save policy (D-2) turned it into a silent page-wide outage rather than a loud error.

### C-1 — Overlay write path: every Save silently died after any API/engine-mode selection

**FACT (at 75917ef).** `19088cd` changed the file DTO from `api_capture As String = Nothing` to `api_capture As Integer = 1` (diff in Appendix B) in the same commit that started writing strings from this page. With `Option Strict Off` the assignment `d.api_capture = Recording.APICapture` (`AppSettings.vb:800` at that anchor) compiled but performed a VB runtime narrowing conversion at save time:

- `APICapture = "ddagrab"` / `"gdigrab"` / `"gfxcapture"` → **`System.InvalidCastException`** (proven, Appendix A case A).
- `APICapture = Nothing` (FFmpeg mode) → converted to `0`, save proceeded, file got `"api_capture": 0` (case A2/C).
- `APICapture = "1"` (load round-trip artifact) → `1`, save proceeded (case A3).

The throw happened inside `BuildRecordingDto`, i.e. **before** `JsonSerializer.Serialize` and `WriteConfigFileAtomic`. `Save()` wraps everything in one blanket `Try/Catch` that only does `Debug.WriteLine`. Net effect at that anchor:

1. Clicking **Duluka Capture** or **any API item** left config.json untouched — the mode/API was shown as applied by the UI but never persisted.
2. The in-memory model still held `"ddagrab"` after the click, so **every subsequent `Save()` from anywhere in the process threw and wrote nothing** — preset/bitrate/FPS/resolution/replay changes all stopped persisting until the model was reset or the app restarted. No error surface: no dialog, no log file, only Debug output.
3. `FormClosing` hit the same throw inside its own `Try/Catch` → close-time persistence silently no-opped.
4. TCP `engine_config_changed` was still sent after the failed save — the engine reloaded the *old* file and concluded nothing changed.

**Blast radius (all Save sites in the page at 75917ef):** :290, :648, :729, :1233, :1272, :1280, :1288, :1301, :1322, :1330, :1338, :1346, :1354, :2199, :2304, :2308, :2426.

### C-2 — Engine read path: files the Overlay *could* write broke the nested video parse

**FACT (at 75917ef).** The only values the Overlay could persist were numeric (`0`/`1`, Appendix A case C). The engine parses the nested `Recording` section with `api_capture As String` and strict `_jsonOpts` (no `NumberHandling`):

- `"api_capture": 0` → **`System.Text.Json.JsonException`** ("The JSON value could not be converted to System.String", proven, Appendix A case B) inside `Deserialize(Of VideoConfig)` → caught → `TryParseNestedRecording` returned **Nothing**.
- The flat fallback matched only case-insensitive flat names (`encoder`, `active_preset`, `replay_duration` — `RecordingSettings` has no `current` sub-object), so `current.fps / bitrate / width / height / use_native_resolution` and `api_capture` **all fell back to defaults**.
- Consequence: for any config.json written by the Overlay in the broken window, the engine's effective video config silently degraded to defaults for FPS, bitrate, resolution and CaptureMethod — violating contract invariants **I-3** (single apply path) and **I-4** (session/config FPS authority) de facto, via JSON type drift at the file boundary. Encoder/preset/replay still flowed.
- The engine never crashed: the exception was caught, and the flat path tolerated the numeric key because `"api_capture"` does not match the flat `APICapture` property name (case-insensitive matching does not strip the underscore — proven, Appendix A case B3).

### Root cause statement (FACT)

`19088cd` re-typed the file DTO field to `Integer = 1` while the in-memory model, the engine DTO, the diagnostics and the contract tests all use String values like `"ddagrab"`. Before that commit the field was String end-to-end and merely lacked a UI writer; after it, the writer added the same day could not persist, and the reader that was working degraded. The doc comment ("ddagrab, gfxcapture, GDIGrab, or null (auto)") described the String semantics the Integer field could never express.

**Fix (verified at 306bb30):** `a1b814c` restored the field to `As String = Nothing` (`AppSettings.vb:371`, comment corrected to lowercase `gdigrab`) — exactly repair R-1/R-2 of this audit, executed upstream while the audit was in progress.

---

## 5. Findings — HIGH

### H-2 — `UI_Engine.vb:1374` diagnostics text is stale after today's commits
**STATUS at 306bb30: OPEN.**
**FACT.** The diagnostics panel prints `(config.json Recording.api_capture — no UI writer; Q2 open)`. Since `19088cd`/`05af7d5`/`75917ef`, the [5] page **is** a UI writer (2 write paths + mode radio, §1.3), and since `a1b814c` those writes actually persist. **IMPACT:** the operator panel misstates ownership; contract doc v2.0 (Q2) is behind the code. **RECOMMENDATION:** re-anchor `docs/CONFIG_RUNTIME_CONTRACT.md` Q2 with §4 of this audit (owner: GLM/2 per current task split); correct the diagnostics string when the engine UI is next touched.

### H-3 — One key, two semantics: `api_capture` drives both the engine-mode radio and the FFmpeg API dropdown
**STATUS at 306bb30: OPEN (now disk-persistent).**
**FACT.** `GetEngineModeKey()` (:2397-2402) derives the *regime* from the same value the API dropdown writes. In ffmpeg mode the menu offers `ddagrab` — an **FFmpeg filter name** — as an "API"; selecting it sets the model to `"ddagrab"` (:2305) and persists it (:2306), so the radio flips to Duluka on the next `UpdateEngineModeUI`/form reload even though the user only picked an FFmpeg capture filter. `gfxcapture` is additionally still accepted by `GetSelectedApiKey()` (:2347-2349) though no path can produce it after `05af7d5` (engine-mode mapper no longer recognizes it). **IMPACT:** modal state collision on one key. **RECOMMENDATION:** separate regime from capture-API (dedicated `engine_mode` key) — decision D-3.

### H-4 — FormClosing save/teardown ordering has a loss window
**STATUS at 306bb30: OPEN.**
**FACT.** `Base_RecordingsSet_FormClosing` disposes `_saveSettingsTimer` (:632-637), then calls `SaveCurrentSettings()` (:645) which **re-creates and starts** a 300 ms timer (:1172-1180). The pending `SaveSettingsNow` therefore fires after close (if the pump is alive) or never (app exits within 300 ms); in the latter case control-only edits (e.g. `FPS_BOX.Text` typed but not yet committed by any handler) are lost because :647 persists the model, not the controls. **IMPACT:** narrow, real, silent loss window. **RECOMMENDATION:** flush synchronously at close (call `SaveSettingsNow()` directly) instead of re-arming the debounce.

---

## 6. Findings — MEDIUM

### M-1 — FPS cap schisma inside the page and against the engine (pre-existing, still open)
**STATUS at 306bb30: OPEN.**
**FACT.** UI input caps: `MAX_FPS_GLOBAL = 800` (:18) and `FPS_LIMITS("1920x1080") = (1, 800, 60)` (:135) — with the page's own comment (:11-12) admitting Engine validates differently. Engine hard cap: `FPS must be between 1 and 240` (`CaptureSettings.vb:485`). Also internally inconsistent: `CalculateFPSLimitsFromPixels` yields max 144 for 1080p-class pixels (:431-436) and 120 for 3440x1440 (:429-430) while the dictionaries say 800 and 100 — unknown/custom resolutions get different limits than equal-size known ones. **IMPACT:** a user can set 800 FPS; engine clamps to 240. **OWNER DECISION REQUIRED (D-1)** — a policy choice (unify at 240? 800?), not a mechanical fix.

### M-2 — My-preset values are not persisted at click time
**STATUS at 306bb30: OPEN.**
**FACT.** `MyLow_TEXT_Click` saves only `Preset = "MyLow"` (:1323) *before* `ApplyMyLowPreset` writes the level's FPS/Bitrate/EncoderPreset into the model (:1380-1382) with no Save; `SetBitrateValue` deliberately suppresses the ValueChanged autosave (:976-978). Values reach disk only on the next unrelated save. Crash before that → `GetValueOrDefault` defaults return (:1370-1372). Same shape for MyMedium/MyHigh/Recommended/Maximum. **RECOMMENDATION:** save after apply.

### M-3 — Replay duration: ungated save (TCP half fixed by W2-5)
**STATUS at 306bb30: HALF-FIXED.**
**FACT.** The dead `BUFFER_DURATION` TCP send was removed by W2-5 (`1d201fb`) with a comment citing the same evidence ([Engine] dispatches `engine_*` only). Remaining: `TrackBar_Replaylast_MouseUp` (:287-295) writes `ReplayDuration` + `Save()` with **no `IsEditablePreset` gate**, and neither lock routine touches this control — so it is editable even in Recommended/Maximum ("ALL LOCKED", :1454/:1483). **RECOMMENDATION:** decide whether replay length is intentionally global (likely yes — then document it in the matrix) and gate the UI affordance accordingly.

### M-4 — `PREWARM_FFMPEG` payload carries a stale encoder name
**STATUS at 306bb30: OPEN.**
**FACT.** `LoadAPIRECORD` reads `_currentEncoderName` from `cmbEncoder.Text` (:542-543) *before* `PopulateEncoderDictionary`/`SelectSavedOrBestEncoder` run; the Designer default text is `"60"` (Designer:663). The prewarm at :561 therefore sends `"<path>|60"`. The engine parses out only the path and **discards the encoder part** (`[Engine] Client.vb:171-177` → `HandleEnginePrewarmFFmpeg(ffmpegPath)`), so today it is harmless but misleading on the wire. **RECOMMENDATION:** move the prewarm after encoder selection or drop the `|encoder` suffix.

### M-5 — Permanently-disabled advertising UI: 5 API placeholders + OBS card
**STATUS at 306bb30: OPEN.**
**FACT.** ddagrab mode shows 5 disabled items (`windows_graphics_capture`, `d3d11_native`, `window_capture`, `region_capture`, `native_game_capture`, :2263-2270) that no runtime implements; the OBS engine card is visible, colored permanently inactive, and its click only writes a Debug line (:2391-2395). **IMPACT:** users are shown options that cannot work. **OWNER DECISION REQUIRED (D-4):** hide until implemented, or keep as roadmap signage.

### M-6 — API display shadow state (largely addressed by Wave 2)
**STATUS at 306bb30: LARGELY ADDRESSED; residual noted.**
**FACT.** Wave 2 split `_selectedApiKey` into per-mode caches (`_selectedDulukaApiKey` / `_selectedFFmpegApiKey`, :777-778) and made `GetSelectedApiKey()` (:2335-2350) validate the ffmpeg cache (:2342-2346) against the configured value before trusting it — the display can no longer stick to a value config contradicts in ffmpeg mode. Residual: the ddagrab-mode cache (:2337-2338) is still session-sticky and never re-hydrated on load, and both caches are written even when the subsequent `Save()` fails. **RECOMMENDATION:** re-hydrate both caches from config in `LoadAPIRECORD` (same one-liner pattern as `UpdateApiBoxDisplay`).

---

## 7. Findings — LOW / informational (75917ef numbering; ±1-line drift under Wave 2)

- **L-1** — Triplicated dead comment block (:1161-1165 at 75917ef; still present ×3 at 306bb30): "GLM/6 unified config: legacy video.json writer/reader removed — config.json is the ONE file."
- **L-2** — `FindFFmpegPath` result is written into `Paths.FFmpegPath` (:557) — a UI page writing a Paths key as a load side effect (rides the next Save; not saved when ffmpeg is *not* found).
- **L-3** — ~~Dead `BUFFER_DURATION` TCP send~~ — **fixed by W2-5**; dropped from this revision.
- **L-4** — All new API-dropdown and engine-mode strings are hardcoded English (no `LangHelper`), while the rest of the page localizes — l10n gap for the next localization pass.
- **L-5** — Inline literal `Color.FromArgb(33, 35, 38)` duplicated in dropdown handlers vs the existing `COLOR_INACTIVE` constant; cosmetic drift.
- **L-6** — Encoder priority order `NVENC_HEVC > NVENC_H264 > NVENC_AV1 > QuickSync…` (:801): AV1 is tried *after* H.264 even when the GPU supports it; HEVC-first is a product choice worth confirming.
- **L-7** — `UpdateApiBoxDisplay` Case Else (:2331) renders the same label "ddagrab — Direct3D 11 Desktop Capture" for the FFmpeg filter `ddagrab` and for the native default — two different runtimes share one display string.
- **L-8** — The 2026-09-03 "defensive rewrite" commits (`05af7d5`/`75917ef`) inline explicit types (`Dim item As System.Windows.Forms.ToolStripMenuItem = New …`, `DirectCast(cms, ContextMenuStrip)`) with broken indentation — no behavior change beyond the listed ones.
- **L-9** — `CheckEncoderAvailability` runs ffmpeg with a 1500 ms timeout and a shared cache (:184-246); thread-safe via `SyncLock`; called in background via `EncoderService.VerifyAllInBackground`. Sound as-is.
- **L-10** — `WndProc` returns `HTTRANSPARENT` for all `WM_NCHITTEST` (:70-73): the form body is click-through by design (`a4262c7`), child controls still receive input.

---

## 8. Decision packages (no decision made by this audit)

### D-1 — FPS cap unification
- **FACT:** UI 800 @1080p (:18, :135); engine 240 (`CaptureSettings.vb:485`); internal UI tables disagree with the pixel-derived table (M-1).
- **OPTIONS:** (a) unify UI tables to 240; (b) raise engine to 800; (c) keep dual caps and clamp silently at the boundary.
- **IMPACT:** (a) removes false advertising, may annoy power users; (b) requires engine-side frame-pacing work (Video territory — GLM/1); (c) preserves status quo + user confusion.
- **RECOMMENDATION:** (a) + fix the internal table inconsistency; cheapest, aligns with contract I-4.
- **OWNER DECISION REQUIRED.**

### D-2 — Should `Save()` keep swallowing exceptions?
- **FACT:** the blanket catch in `AppSettings.Save()` is what turned the C-1 type bug into a *silent page-wide persistence outage* instead of a loud, immediate error. The atomic-write design itself is sound.
- **OPTIONS:** (a) keep swallow + add an error event/status surface; (b) rethrow a typed `ConfigSaveException` and let callers decide; (c) keep swallow but write a fallback marker file.
- **IMPACT:** (b) risks crash loops at UI sites without try/catch (e.g. :2438); (a)/(c) preserve resilience while killing silence.
- **RECOMMENDATION:** (a) — at minimum expose `LastSaveError` consumed by the overlay status/diagnostics.
- **OWNER DECISION REQUIRED.**

### D-3 — Separate *engine regime* from *capture API* on the config schema
- **FACT:** one key (`api_capture`) currently encodes regime (ddagrab vs ffmpeg) and, inside ffmpeg, the capture filter (H-3). Engine treats it as echo-only (`UI_Engine.vb:1377`: "non-ddagrab request = recorded GAP, never silently accepted").
- **OPTIONS:** (a) add `Recording.engine_mode` (ffmpeg|duluka) and demote `api_capture` to filter-only; (b) keep one key with namespaced values (`ffmpeg:gdigrab`); (c) status quo.
- **IMPACT:** (a) is a schema change → contract v3 + migration; (b) is uglier but additive; (c) leaves the collision live.
- **RECOMMENDATION:** (a), decided together with contract Q1/Q2 re-anchor.
- **OWNER DECISION REQUIRED.**

### D-4 — Placeholder UI policy
- **FACT:** 5 disabled native-API items + dead OBS card (M-5).
- **OPTIONS:** hide vs keep as roadmap.
- **RECOMMENDATION:** hide (disabled items in a dropdown still read as "coming soon" promises the engine cannot honor).
- **OWNER DECISION REQUIRED.**

## 9. Repair recommendations (for the code-owning agent; none executed here)

- **R-1 / R-2 (C-1/C-2):** ✅ **DONE upstream** — `a1b814c` restored `api_capture As String = Nothing` (`AppSettings.vb:371`). Optional belt-and-braces (not present at 306bb30): add `.NumberHandling = JsonNumberHandling.AllowReadingFromString` to `OverlayConfig._jsonOpts` so legacy numeric files parse instead of degrading to the flat path.
- **R-3 (H-2):** correct the stale diagnostics string at `UI_Engine.vb:1374`.
- **R-4 (test, still missing):** add a ConfigTruth round-trip test: build a `ConfigFileDto` via the real `BuildRecordingDto` with `APICapture = "ddagrab"`, serialize, deserialize on the engine side, assert `CaptureMethod = "ddagrab"` — this is the exact seam CI currently cannot see (`git grep BuildRecordingDto -- *Tests*` = empty at 306bb30). This single test would have caught C-1+C-2 on day one.
- **R-5:** H-4 flush-on-close; M-2 save-after-apply; M-4 prewarm ordering; M-6 cache re-hydration.

---

## Appendix A — Executed repro (evidence for C-1/C-2, run at the 75917ef state)

Mirror classes with the *exact* property declarations from both sides; stock `System.Text.Json` on .NET 10 (SDK 10.0.400), options copied verbatim from `OverlayConfig._jsonOpts`. Source kept at `scripts/api_capture_repro/` (outside the repo).

```
A)  String->Integer assignment THREW: System.InvalidCastException
    — Conversion from string "ddagrab" to type 'Integer' is not valid.        ← C-1
A2) Nothing->Integer assignment SUCCEEDED, value=0                             ← FFmpeg click writes 0
A3) "1"->Integer assignment SUCCEEDED, value=1                                 ← legacy round-trip only
B)  Number->String(snake) THREW: System.Text.Json.JsonException
    — The JSON value could not be converted to System.String.
    Path: $.api_capture                                                        ← C-2 (nested parse dies)
B2) String->String(snake) SUCCEEDED, api_capture=ddagrab                       ← contract type works
B3) Number->String(Pascal) SUCCEEDED, APICapture=<null>                        ← flat path skips silently
C)  Overlay writes to config.json: { "api_capture": 0 }                        ← the only writable shape
```

## Appendix B — The type change and its revert

Introduced (`19088cd`, 2026-09-03):
```diff
         ''' <summary>Capture API: ddagrab, gfxcapture, GDIGrab, or null (auto)</summary>
-        Public Property api_capture As String = Nothing
+        Public Property api_capture As Integer = 1
```
Reverted (`a1b814c`, same day — verified at `306bb30`):
```diff
-        ''' <summary>Capture API: ddagrab, gfxcapture, GDIGrab, or null (auto)</summary>
-        Public Property api_capture As Integer = 1
+        ''' <summary>Capture API: ddagrab, gfxcapture, gdigrab, or null (auto)</summary>
+        Public Property api_capture As String = Nothing
```
