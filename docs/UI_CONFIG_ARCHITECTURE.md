# UI CONFIG ARCHITECTURE — NVIDIA ShadowPlay

- Route: **PHASE 3 — UI CONFIG ARCHITECTURE & SPEC** (read-only audit + design spec; no production code touched)
- Anchored at commit: **`06667e9`** (v1.1 — re-anchored after PHASE 1 video wiring `867aae3..06667e9`; v1.0 audited at `ab89372`, parent `99a5dc0` PHASE 0 CONFIG TRUTH fix wave)
- Owner: ScotcsDuluka · drafted by GLM/3
- Companion doc (source of truth for field ownership): **`docs/CONFIG_OWNERSHIP_MATRIX.md`** — this document does not re-decide ownership; it maps the UI layer onto that matrix and specifies the target UI.
- Method: every finding below was traced from real source. Format: `path · method/control · line`. Where the repo does not contain something, it is reported `NOT IMPLEMENTED` / `UNKNOWN` — never invented.

### Revisions
- **v1.0** — audited at `ab89372`. All line citations valid at that commit.
- **v1.1 (this)** — re-anchored at **`06667e9`** after PHASE 1 video wiring landed (`867aae3..06667e9`, V-CT1–5). Updated with fresh source evidence: FPS per-record `NextRecordingConfig.vb:93` → `CaptureSession.vb:495-500`; bitrate/preset/RC/engine-init `NextRecordingConfig.vb:139,156-159` → `RecordingEngine.vb:103-112`; resolution dims `RecordingEngine.vb:88-99`; preset unified mapper NOW EXISTS `OverlayConfig.vb:455-456`; capture-method echo `RecordingEngine.vb:123-127` (gfxcapture = recorded GAP); pixel-format BLOCKER truth line `RecordingEngine.vb:130-135`; backend still hardcoded `RecordingEngine.vb:76`; NVENC backend still unconditional `:82`; validation caps unchanged (`CaptureSettings.Validate()` FPS 1–240, bitrate 1–200 Mbps). Files NOT touched by PHASE 1 (UI_Engine.vb, AppSettings.vb, CaptureSettings.vb, all Overlay forms) keep valid v1.0 citations.

---

## 1. Current UI Inventory

### 1.1 Overlay app — `Overlay/NVIDIA Overlay.vbproj`

Root folder for forms: `Overlay/[Forms Overlay - Project Files]/[UI OVERLAY]/`

| FULL PATH | FORM / CLASS | KEY CONTROLS | CURRENT PURPOSE |
|---|---|---|---|
| `Overlay/.../[index]/[Base]/[Main Menu]/[1] Main Menu.vb` | `Base` (main overlay form) | keyboard hook (`KeyboardHookProc:230`), `Menu_Record_Sttings_text:916`, `Menu_Replay_Sttings_text:920`, `LoadFilePath:773` | Main overlay shell: hotkey event routing, record/replay menu, opens Engine UI via `Engine_UI_Tick` marker-file timer (`:1063`), audio UI via `Audio_UI_Tick` (`:1084`) |
| `Overlay/.../[Main Menu]/[1] Sub_Record.vb` | `Partial Base` | `ToggleRecording:132`, `ToggleInstantReplay:209`, `SaveInstantReplay:290`, `GetOutputDirectory:94` | Record/Replay command sender (TCP `RECORD_START/STOP`, `REPLAY_START/STOP/SAVE`), optimistic local state `_isRecordingLocal:68` |
| `Overlay/.../[Main Menu]/[1] Sub_Mouse.vb` | `Partial Base` | `MIC_ICO` click toggle (`Mic_Click:250`), `LoadMicState:243`, gallery helpers (`:279+`) | Overlay quick mic on/off toggle (writes `Audio.MicEnabled` + `Save()`), icon-glyph state machine |
| `Overlay/.../[Main Menu]/[1] Sub_Misc.vb` | `Partial Base` | `UpdateMicStatus:156`, `save_sc_Click:213` (folder dialog), `PrivacyOpen:247` | Mic-state sync from icon glyph, Gallery path picker (`Paths.GalleryPath`), privacy panel navigation |
| `Overlay/.../[Main Menu]/[1] Sub_Hotkey.vb` | `Partial Base` | `Run_OpenShare:8` … `Run_ManualRecordToggle:74`, `TestNotifier:91` | Hotkey event → action routing only (no config) |
| `Overlay/.../[Main Menu]/[Gallery]/[1] Main.vb` | `Base_Gallery` | `txtFilePath` → `AppSettings.Instance.Paths.GalleryPath = txtFilePath.Text` (`:62`) | Gallery view + gallery path editor (2nd writer of the same key) |
| `Overlay/.../[Main Menu]/[Settings]/[0] Settings - UI Main.vb` | `Base_Settings` | `ToggleUseWindowsSnip:78`, `SW_lang`/`SelectLang:111` | Settings home + General page (Windows Snip, UI.Language); navigation list for all pages |
| `Overlay/.../[Settings]/[1] Connect.vb` | `Base_Connect` | GitHub OAuth PKCE flow (`Github_Text_Click:142`, `ProcessCallback:269`, `Logout:477`) | GitHub account login → `GitHubUser` section |
| `Overlay/.../[Settings]/[2] Overlay Hub.vb` | `Base_Overlay_Hub` | nav shell only (`action_fn_Click:48`) | Layout shell; **no config controls** |
| `Overlay/.../[Settings]/[4] Keyboard Shortcut.vb` | `Base_KeySet` | per-action labels `lbl_<ActionKey>` (`InitKeyLabels:63`), capture flow (`Base_KeySet_KeyDown:129`), `Reset_Click:197` | Hotkey capture UI → `config.json` `Hotkeys` dict via `HotkeyService` |
| `Overlay/.../[Settings]/[5] Video Capture.vb` | `Base_RecordingsSet` | `FPS_BOX:2043`, `Resolution_BOX:2082`, `cmbEncoder`, `P_BOX` preset system, `TrackBar_BITRATE`, `TrackBar_Replaylast:273`, `prearg` command preview (`UpdateCommandPreview:1919`), `vdo_resetall:719` | THE video settings page: FPS, resolution, bitrate, encoder, NVENC preset, replay duration, My/Recommended/Maximum presets → `config.json` `Recording` |
| `Overlay/.../[Settings]/[6] Audio Capture.vb` | `Base_AudioSet` | `chkSystem/chkMic`, `trkSystemVol/trkMicVol` (0–150%), `cboMic`, `radSingle/radSeparate`, `btnRefresh/btnApply/btnTest` | Audio settings page → `config.json` `Audio` + `engine_config_changed` broadcast (`:137`) |
| `Overlay/.../[Settings]/[7] Notifications.vb` | `Base_Notifications` | 24 toast toggles (`LoadNotificationToggles:59`), `ToggleToastSlot2/3` (`SyncToastSlotCount:388`), `ObsEnabledToggle/HOST_BOX/PORT_BOX/KEY_BOX` (`LoadObsSettings:418`) | Notification toggles + toast slot count → `Notifications.*`; OBS bridge → **separate file `Config/notifier_obs.json`** |
| `Overlay/.../[Settings]/[9] Privacy Control.vb` | `Base_Privacy_Control` | `TogglePrivacy` (`:50`), recording lock timer `IF_Use_Engine_Tick:57` | Desktop-capture consent → `Privacy.DesktopCaptureEnabled`; locked while recording |
| `Overlay/.../[index]/[Base]/[Game Filter]/[1] Settings UI Main.vb` | Game Filter pages | — | Game Filter UI; **writes nothing to config.json** (zero `AppSettings` writes found in `[Game Filter]/`) |

Overlay services (non-visual): `Overlay/[Forms Overlay - Project Files]/[API]/[Services]/`
| FILE | ROLE | EVIDENCE |
|---|---|---|
| `AppSettings.vb` | config.json model + atomic Save | `Save():1023`, `WriteConfigFileAtomic:863-872`, nested DTO `ConfigFileDto:379-390` |
| `AppSettings.LegacyVideo.vb` | one-time video.json migration | `Recording.APICapture = video.APICapture:249` |
| `HotkeyService.vb` | hotkey registry + OS registration | `AllHotkeys:14-29`, read/write dict `:40-53`, `RegisterAll:74` |
| `EncoderService.vb` | ffmpeg encoder availability probe | `CheckAvailability:56`, `GetFFmpegCodecName:124-134` |
| `EngineProcessSupervisor.vb` | engine availability = process existence | `EnsureEngineRunning:94`, `Process.GetProcessesByName:126,262` |
| `SettingsExportImport.vb` | portable settings export/import | `ExportWithDialog:65`, `ImportWithDialog:135` |
| `TCP/[Overlay] Client.vb` | TCP client + engine command dispatch | `engine_ready→PREWARM_FFMPEG:102`, state reconcile `:114-161`, `DispatchEngineCommand:257-275` |

### 1.2 Engine app — `Engine/NVIDIA Capture.vbproj`

| FULL PATH | FORM / CLASS | KEY CONTROLS | CURRENT PURPOSE |
|---|---|---|---|
| `Engine/Engine/[UI]/UI_Engine.vb` | `UI_Engine` (+ partials `RecordingEngineHost.vb`, `[Engine] Client.vb`) | `nudFPS` (1–240, editable `Designer:290-294`), `nudBitrate`, `chkNativeRes:Designer:243-252`, `cboResolution`, `cboCaptureMethod` (3 items `Designer:186`), `cboEncoder` (9 items incl. `libsvtav1` `Designer:216`), `nudReplayDuration`, `chkSysAudio/chkMic`, `trkSysVol/trkMicVol`, `txtOutputDir`, `txtFFmpegPath`, `btnRecord/btnStop/btnStressTest`, `lblRecState/lblRecBitrate/lblRecSize/lblRecFrames`, `lblConfigSource` | Mixed-role window: 2s config mirror (`RefreshOverlayConfigUI:173`), **editable engine.json config controls** (`LoadSettings:633`, `SaveSettings:652`), record/stop operator buttons (`OnRecordClick:775`), 10-scenario stress test (`OnStressTestClick:833`), live status panel (`OnEngineStateChanged:964`, `OnEngineProgress:1105`) |
| `Engine/Engine/[UI]/AudioSettingsForm.vb` | `AudioSettingsForm` | `chkSystem/chkMic`, `trkSystemVol/trkMicVol` (0–150%), `cboMic`, `radSingle/radSeparate` | Engine-side audio settings — **writes 3 files on Apply** (`SaveToSettings:94-129`): `audio.json` (`:117-118`), `engine.json` (`:121-122`), Overlay legacy `video.json` (`SaveToOverlayVideoJson:131-158`) |
| `Engine/Engine/[Capture]/CaptureSettings.vb` | model (not a form) | — | engine.json/video.json/audio.json model + unified-WINS load chain (`Load:94-164`, WINS `:105-116`), `Save(engine.json):170-196`, `SaveAudio(audio.json):301-319`, `Validate():483-507` |
| `Engine/Engine/[Integration]/OverlayConfig.vb` | mirror module | — | Engine-side mirror of config.json (+ legacy video.json); `ApplyUnifiedToCaptureSettings:435-515`; encoder mapping `MapEncoderToFfmpeg:521` |
| `Engine/Engine/[API]/[Engine] Client.vb` | `Partial UI_Engine` | — | TCP dispatch; new/legacy pipeline switch at `DispatchEngineCommand:243-293` |
| `Engine/Engine/[API]/RecordingEngineHost.vb` | `Partial UI_Engine` | — | New-engine host; `_useNewEngine:42`; startup snapshot `:78-105`; FIX-1 fresh reload `:214-216` |
| `Engine/Engine/[API]/NextRecordingConfig.vb` | `NextRecordingConfig` | — | The per-record config seam (`MapSessionConfig:80-99` — audio-only mapping) |
| `Engine/Engine/EncoderDetector.vb` | `EncoderDetector` | — | ffmpeg encoder/device detection for Engine dropdown |

### 1.3 API hub — `API/NVIDIA API.vbproj`

| FULL PATH | FORM / CLASS | CURRENT PURPOSE |
|---|---|---|
| `API/[Forms - Project Files]/[API]/Server.vb` | `Server` | TCP broadcast hub (register/ping/broadcast, `ProcessMessage:296-352`); carries all commands; owns no config |
| `API/[Forms - Project Files]/[UI Forms]/NVIDIA API.vb` | `NVIDIA API` | Hub form; reads `Overlay.UseOverlayEnabled` every second to keep-alive/kill the overlay stack (`:131-133`) |

### 1.4 Launcher — `Launcher/NVIDIA Experience.vbproj`

| FULL PATH | CONTROL | CURRENT PURPOSE |
|---|---|---|
| `Launcher/[Forms - Project Files]/[UI Forms]/Main.vb` | `Use_Overlay` toggle | OWNER of `Overlay.UseOverlayEnabled` — `AppConfigShared.WriteBool("Overlay","UseOverlayEnabled", …)` (`:115-123`), read `:75` |

---

## 2. Current Config Surfaces

What each group actually has today. "W" = UI writer exists, "M" = mirror/display only.

### Engine (engine selection / New-Legacy / capture mode)
- **Engine selector: does not exist in ANY UI.** Runtime selection is automatic: `RecordingEngineHost.vb:42` `_useNewEngine = True`; on init failure `:117` `_useNewEngine = False`; dispatch switch `[Engine] Client.vb:247-259`. `NOT IMPLEMENTED` as a user choice.
- **Capture mode (CaptureMethod)**: Engine WinForms only — `cboCaptureMethod` (`UI_Engine.vb:1163-1170` → `_settings.CaptureMethod`; persisted to engine.json by `SaveSettings:660-664`). Overlay `[5]` has **no** capture-method control; the config.json key `Recording.api_capture` has **no UI writer** anywhere (model only: `AppSettings.vb:50,371`; migration `AppSettings.LegacyVideo.vb:249`).
- **New/Legacy visibility**: Engine WinForms shows pipeline only via log line (`[Engine] Client.vb:258`) and status label; no explicit "engine mode" control.

### Capture (ddagrab / gfxcapture / native)
- `ddagrab`/`gdigrab`/`gfxcapture` are legacy-FFmpeg capture methods: whitelist `CaptureSettings.Validate():493-496`, builder `CaptureEngine/FFmpeg/FFmpegCommandBuilderV2.vb:79-96`, V1 `:94-110`, validator `CaptureEngine/Configuration/ConfigValidator.vb:57`.
- New Engine **hardcodes** native `DdagrabBackend`: `CaptureEngine.Recording/RecordingEngine.vb:76` (`New DdagrabBackend(_logger)`, no args). `VideoBackendKind` enum has `Ddagrab, GfxCapture` (`CaptureEngine.Video/Contract/VideoBackendKind.vb:11-12`) but **no native GfxCaptureBackend exists** — `IVideoCaptureBackend.vb:8` says "Implemented by DdagrabBackend, GfxCaptureBackend (future)". → **gfxcapture (native) = NOT IMPLEMENTED**. PHASE 1 adds an honesty echo: `requested → selected → actual` logged at `RecordingEngine.vb:123-127`; a non-ddagrab request is a **recorded GAP**, not silently accepted.
- "Duluka Capture" as a literal option: **NOT IMPLEMENTED** — the string exists nowhere in `.vb` source (only as the GitHub owner name `ScotcsDuluka` in docs/README). The real two pipelines in code are *New Engine (native D3D11+NVENC)* vs *Legacy Engine (FFmpeg subprocess)*.

### Video (FPS / Resolution / Bitrate / Encoder / Preset / Rate Control / Pixel Format)
- FPS: Overlay `FPS_BOX` (`[5]:2043`, caps `MIN/MAX_FPS_GLOBAL 1–800` `:17-18`, dynamic limits `UpdateFPSLimit:1007`) → `Recording.current.fps` (`SaveSettingsNow:1194`). Engine mirror-edit: `nudFPS` 1–240 (`Designer:292-293`). **PHASE 1: config FPS is the ONLY fps source** — per record `NextRecordingConfig.vb:93` → `CaptureSession.vb:495` (fallback 60 + loud warning `:498`); display refresh demoted to diagnostics (`:500`).
- Resolution: `Resolution_BOX` + `Native` key (`[5]:26, 2082, LoadResolutionBox:1040, ApplyResolutionSelection:1092-1126`) → `current.use_native_resolution/width/height`. **PHASE 1: WIRED at engine init** — `ResolveEncodeDimensions` → NVENC encode dims, GPU scale, oversized fails loudly (`RecordingEngine.vb:88-99`).
- Bitrate: `TrackBar_BITRATE` (`[5]:979-1004`, caps 500–150000 kbps `:13-14`) → `current.bitrate` (`:1195`). **PHASE 1: lands in NVENC `averageBitRate` at engine init** (`NextRecordingConfig.vb:139` → `RecordingEngine.vb:104-106`).
- Encoder: `cmbEncoder` population `[5]:734-767` (NVENC_H264/HEVC, NVENC_AV1 if supported `:743-745`, QuickSync_H264/HEVC, LibX264/265; **AMF commented out** `:753-757`); ffmpeg probe `EncoderService.CheckAvailability:56`.
- NVENC preset: preset buttons Low/Medium/High/Custom/MyLow/Medium/High/Recommended/Maximum (`[5]:1265-1354`) → `current.encoder_preset` (`:1198`); `PresetNameToIndex:2009`. **PHASE 1: the v1.0 mapper gap is CLOSED** — unified apply now maps `encoder_preset → NvencPreset` (`OverlayConfig.vb:455-456`) and a single mapper reaches the NVENC preset GUID (`NextRecordingConfig.vb:156-159`); engine.json `Preset` = compat fallback only.
- **Rate Control: no UI anywhere.** engine.json only (`CaptureSettings.vb:61`); PHASE 1 applies it into NVENC `rateControlMode` at init (`RecordingEngine.vb:110`). Matrix: move key to config.json later.
- **Pixel Format: no UI anywhere.** engine.json only (`CaptureSettings.vb:59`). **PHASE 1 truth: config nv12 NOT honored** — runtime is BGRA8 (D3D11) → NVENC ARGB; conversion layer NOT implemented (**BLOCKER P1-PIXFMT**), reported honestly in the truth line `RecordingEngine.vb:130-135`.

### Audio (System / Mic / Devices / Volume / Tracks / Clock)
- System+Mic+Volumes+Device+Tracks: Overlay `[6] Audio Capture` (writer) AND Engine `AudioSettingsForm` (writer) AND overlay quick mic toggle `MIC_ICO` (`Sub_Mouse:250`) — three surfaces.
- **AudioClockMode: no UI toggle in either app.** config.json only, hand-edited for P13 A/B (`AppSettings.vb:115-124`).
- Volume range mismatch: Overlay `[6]:54-55,115-116` clamps 0–150% while model doc says 0.0–1.0 (`AppSettings.vb:87-94`) — values >1.0 possible from UI.

### Output (Save path / Gallery / Format / FFmpeg)
- GalleryPath: two Overlay writers — `[1] Sub_Misc.vb:218` and `[Gallery]/[1] Main.vb:62`.
- SavePath: **no UI writer** (loaded `AppSettings.vb:932`; used as first choice by `GetOutputDirectory` `Sub_Record.vb:99`).
- FFmpegPath: Overlay `[5]` auto-detect + write (`FindFFmpegPath:602-619`, write `:558`); Engine `txtFFmpegPath` + `OnBrowseFFmpeg:1183-1218` → engine.json on `SaveSettings:658`.
- **FileFormat: no UI** (engine.json `CaptureSettings.vb:62`; legacy filename only `GenerateOutputFilename:469-472`; new path is structurally MP4 — `LiveMuxSession.vb:18,306` per matrix).

### Hotkeys
- Overlay `[4] Keyboard Shortcut` → `Hotkeys` dict (10 actions, `HotkeyService.AllHotkeys:17-28`) — the LIVE system (`HotkeyService.vb:40-53,74-86`).
- engine.json `HotkeyStart/Stop/Toggle` (`CaptureSettings.vb:64-66`) = DEAD twin (engine UI has `_hotkeyStartId/_hotkeyStopId` fields `UI_Engine.vb:62-63` but no registration call was found in `UI_Engine.vb`).

### Overlay / Notifications
- Overlay enable: Launcher `Use_Overlay` toggle (`Launcher/Main.vb:115-123`) — the only writer; Overlay Save() re-reads it as a foreign-key guard (`AppSettings.vb:1032`); API hub enforces it (`NVIDIA API.vb:131-133`).
- Notifications: 24 per-toast toggles (`[7]:59-170`) + `SlotCount` via `ToggleToastSlot2/3` (`[7]:379-403`, clamp 1–3 `AppSettings.vb:1017`).
- OBS bridge: `[7]:418-466` → separate store `Config/notifier_obs.json` (`Common/ObsConfig.vb`).

---

## 3. User-Facing vs Engine-Internal

Classification traced from real usage (not names). Legend: USER / INTERNAL / DIAGNOSTIC / LEGACY-ONLY / DEAD / UNKNOWN.

| Setting | Class | Evidence |
|---|---|---|
| `Recording.encoder` | **USER** | UI writer `[5]:817-818`; runtime: new engine init `RecordingEngineHost.vb:80-91`, legacy `OverlayConfig.vb:445` |
| `Recording.encoder_now` | **DIAGNOSTIC** (transition mirror) | written `[5]:818,1191`; engine ignores it at record time (matrix ⚠️; consumed only by `SelectSavedOrBestEncoder:787`) |
| `Recording.active_preset` | **USER** (preset selection) | `[5]` preset buttons; `ApplyUnified:456` → `ActivePreset` (display/legacy preset name) |
| `Recording.current.fps` | **USER, WIRED per record (PHASE 1)** | persisted `[5]:1194`; per-record `NextRecordingConfig.vb:93` → `CaptureSession.vb:495` (fallback 60 + warning `:498`); display refresh diagnostics-only `:500` |
| `Recording.current.bitrate` | **USER, engine-init contract — lands in NVENC struct (PHASE 1)** | `[5]:1195` → `NextRecordingConfig.vb:139` → `RecordingEngine.vb:104-106` (`averageBitRate`); not re-read per record |
| `Recording.current.encoder_preset` | **USER, WIRED via single mapper (PHASE 1)** | `[5]:1198` → unified apply `OverlayConfig.vb:455-456` (v1.0 gap CLOSED) → `NextRecordingConfig.vb:156-159` (`MapNvencPresetInteger`) → NVENC preset GUID; engine.json `Preset` = fallback only |
| `Recording.current.use_native_resolution` | **USER, WIRED at engine init (PHASE 1)** | `[5]:1201-1212` → `ResolveEncodeDimensions` → NVENC encode dims (`RecordingEngine.vb:88-99`); oversized fails loudly |
| `current.width` / `current.height` | **USER, custom-only (WIRED PHASE 1)** | GPU scale via `ResolveEncodeDimensions` (`RecordingEngine.vb:88-99`); oversized → loud error, no silent fallback |
| `Recording.replay_duration` | **USER, RESERVED** (replay NOT IMPLEMENTED at engine) | UI `[5]:289,593-596` + replay commands rejected `UI_Engine.vb:524-534` |
| `Recording.my_presets.*` | **USER** | `[5]:1215-1228`; model `AppSettings.vb:344-357` |
| `Recording.api_capture` | **LEGACY-ONLY writer + PHASE 1 echo** (persisted, still no UI writer) | model `AppSettings.vb:50,371`; mapped `OverlayConfig.vb:466-467` → `RequestedCaptureMethod` (`NextRecordingConfig.vb:178`); New Engine logs gap for non-ddagrab (`RecordingEngine.vb:123-127`); legacy consumes `Validate():493` |
| engine.json `CaptureMethod` | **LEGACY-ONLY writer + DIAGNOSTIC echo (PHASE 1)** | `UI_Engine.vb:1163-1170`, `SaveSettings:660-664`; New Engine: requested→selected→actual log only (`RecordingEngine.vb:123-127`), runtime still ddagrab |
| engine.json `PixelFormat` | **LEGACY-ONLY + BLOCKER P1-PIXFMT recorded (PHASE 1)** | `CaptureSettings.vb:59`; runtime BGRA8 → NVENC ARGB, nv12 conversion NOT implemented — truth line `RecordingEngine.vb:130-135` |
| engine.json `Preset` | **INTERNAL compat fallback** (config `encoder_preset` wins since PHASE 1) | `:60` → fallback branch `NextRecordingConfig.vb:156-159`; delete later per matrix §6.6 |
| engine.json `RateControl` | **INTERNAL (consumed, now lands in NVENC struct)** | `:61` → `RecordingEngine.vb:110` (`rateControlMode`); matrix: move to config.json later |
| engine.json `FileFormat` | **LEGACY-ONLY** | `:62`; only consumer `GenerateOutputFilename:469-472` |
| engine.json `FFmpegPath` | **INTERNAL copy** (config.json `Paths.FFmpegPath` is the owner) | `:63`; engine copy should die (matrix 🔴) |
| engine.json `HotkeyStart/Stop/Toggle` | **DEAD** | `:64-66`; live system reads config.json dict (`HotkeyService.vb:41,49`) |
| `Audio.*` (all 8 keys) | **USER** (canonical, regime A) | writers `[6]`, `Sub_Mouse:250`, `AudioSettingsForm`; runtime fresh per record `NextRecordingConfig.vb:94-102` |
| `Audio.AudioClockMode` | **INTERNAL/DIAGNOSTIC** (A/B knob, no UI) | `AppSettings.vb:115-124`; normalized `OverlayConfig.vb:499-501` |
| `Paths.GalleryPath` / `Paths.SavePath` | **USER** | writers `Sub_Misc:218`, `Gallery/Main:62`; consumed `Sub_Record.vb:94-126`, `OverlayConfig.vb:509-511` |
| `Paths.FFmpegPath` | **USER** | writer `[5]:558`; fresh after FIX-1 (`RecordingEngineHost.vb:231`) |
| `Hotkeys` dict | **USER** | `[4]` + `HotkeyService` |
| `UI.Language`, `UI.UseWindowsSnip` | **USER** | `[0]:78-81,111-149` |
| `UI.Theme` | **UNKNOWN** (model key, no writer UI found) | `AppSettings.vb:67` only |
| `Privacy.DesktopCaptureEnabled` | **USER** | `[9]:50-55` |
| `Overlay.UseOverlayEnabled` | **USER** (Launcher-owned) | `Launcher/Main.vb:115-123` |
| `Notifications.*` + `SlotCount` | **USER** | `[7]` |
| `engine.json` `OutputDirectory` key | **DEAD write** — `Save()` never serializes it but `LoadEngineSettings` reads it | read `CaptureSettings.vb:291`; key list in `Save():176-191` has no OutputDirectory |

---

## 4. Engine → Capture API Dependency (from the UI perspective)

Real graph at `ab89372` (no invented nodes):

```text
[UI: Overlay settings pages]                    [UI: Engine WinForms (UI_Engine)]
   config.json  (Recording/Audio/Paths/…)          engine.json (CaptureMethod/Preset/
        |                                           RateControl/FileFormat/FFmpegPath)
        |  PREWARM_FFMPEG / engine_config_changed       |
        v                                               v
   CaptureSettings.Load (CaptureSettings.vb:94-164)
     engine.json first → unified config.json WINS (:105-116) → legacy video/audio.json fallback
        |
        +--> [NEW ENGINE path]  _useNewEngine=True (RecordingEngineHost.vb:42)
        |      RecordingEngine.Initialize(EngineStartupConfig)   ← encoder group at INIT (:82-112)
        |      New DdagrabBackend(_logger)  ← capture API HARDCODED (RecordingEngine.vb:76)
        |      NvencEncoderBackend.Initialize(EncoderConfig)     ← codec/bitrate/preset/RC/GOP/dims (:82,88-112)
        |      StartSession(SessionConfig)  ← audio + TargetFps per record (NextRecordingConfig.vb:85-102)
        |      CaptureSession.Run()         ← targetFps = SessionConfig.TargetFps (CaptureSession.vb:495;
        |                                      display refresh = diagnostics-only :500; PHASE 1 V-CT1)
        |
        +--> [LEGACY path]  _useNewEngine=False (init failure) or btnRecord (UI_Engine.vb:775-807)
               New CaptureEngine(_settings) (UI_Engine.vb:396,787)
               FFmpegCommandBuilderV1/V2  ← honors CaptureMethod ddagrab/gdigrab/gfxcapture,
                                            FPS, scale (WxH), pixel format, preset, RC, FileFormat
```

### Parent → child control consequences (what must show/hide/enable)

| Parent selection | Child control effect | Reason (evidence) |
|---|---|---|
| Engine = New (native) | Capture API selector **meaningless** → disable + show "ddagrab (built-in)" | `RecordingEngine.vb:76` hardcodes backend; `VideoBackendKind.GfxCapture` has no native implementation (`IVideoCaptureBackend.vb:8`) |
| Engine = Legacy (FFmpeg) | Capture API selector **active**: ddagrab / gdigrab / gfxcapture | whitelist `CaptureSettings.vb:493-496`; builders `FFmpegCommandBuilderV2.vb:79-96` |
| Engine = New (native) | FPS / Resolution now **live** (PHASE 1); PixelFormat control would be a **labeled lie** until BLOCKER P1-PIXFMT is resolved — must show "runtime BGRA/ARGB" | `CaptureSession.vb:495`, `RecordingEngine.vb:88-99,130-135` |
| Engine = New | Bitrate / Encoder / NVENC preset / RC apply at **next engine restart** (init contract); FPS is the exception — **per record** | NVENC init `RecordingEngine.vb:82,103-112`; per-record FPS `NextRecordingConfig.vb:93` |
| Engine = Legacy | FPS / Resolution / Bitrate / Preset / PixelFormat / FileFormat all live | `FFmpegCommandBuilderV1/V2` read them per record |
| Engine init failed | Engine silently falls back to legacy + logs `★★ PIPELINE = LEGACY` | `[Engine] Client.vb:255-259`, `RecordingEngineHost.vb:112-118` |
| Encoder = non-NVENC (QuickSync/LibX) on New Engine | **Not supported by NVENC backend** — must route to legacy or be blocked | `RecordingEngine.vb:82` constructs `NvencEncoderBackend` unconditionally; `CaptureEngine.Encoder.Nvenc` is the only real backend (`CaptureEngine.Encoder/Backends/Fake/FakeEncoderBackend.vb` = test fake) |
| NVENC_AV1 | shown only when `AppSettings.SupportsNVENCAV1` | `[5]:743-745`, detection `AppSettings.HardwareDetection.vb` |
| `chkNativeRes` checked | resolution dropdown disabled | `UI_Engine.vb:1150-1152` (Engine side), `[5]:1092+` (Overlay side) |
| Mic OFF | mic device/volume/tracks controls irrelevant for the record | `SessionConfig.MicEnabled` gate `NextRecordingConfig.vb:90` |
| Privacy OFF | record/replay blocked, settings page navigates to privacy | `Sub_Record.vb:144-151,221-228`; lock `[9]:57-67` |
| Recording active | privacy/record-affecting controls must lock | `[9]:57-67` precedent |

---

## 5. Conditional UI Matrix

Target matrix for the settings UI. Values are grounded in what the repo supports today.

| PARENT | VALUE | CONTROL | VISIBLE | ENABLED | REQUIRED | DEFAULT | REASON |
|---|---|---|---|---|---|---|---|
| Engine | New (native) | Capture API group | yes | **disabled** ("ddagrab built-in"; request echo logged honestly `RecordingEngine.vb:123-127`) | — | ddagrab | `RecordingEngine.vb:76` |
| Engine | New (native) | FPS / Resolution | yes | FPS enabled (live per record), Resolution enabled (applies at engine init — label it) | yes | from config | `CaptureSession.vb:495`, `RecordingEngine.vb:88-99` (PHASE 1) |
| Engine | New (native) | Pixel Format | yes | enabled but flagged "BLOCKER P1-PIXFMT: runtime BGRA/ARGB, config not honored" | no | nv12 | `RecordingEngine.vb:130-135` |
| Engine | New (native) | Bitrate / Encoder / Preset | yes | enabled, labeled "engine restart required" | yes | NVENC_H264 / 20000 kbps / p4 | NVENC init `RecordingEngine.vb:82,103-112` (regime B) |
| Engine | Legacy (FFmpeg) | Capture API: ddagrab | yes | yes | yes | ddagrab | `ConfigValidator.vb:57`, `FFmpegCommandBuilderV2.vb:79` |
| Engine | Legacy (FFmpeg) | Capture API: gdigrab | yes | yes | no | — | `FFmpegCommandBuilderV2.vb:91-92` (CPU capture fallback) |
| Engine | Legacy (FFmpeg) | Capture API: gfxcapture | yes | yes | no | — | `FFmpegCommandBuilderV2.vb:95-96` (ffmpeg filter exists; **native** gfxcapture = NOT IMPLEMENTED) |
| Engine | Legacy (FFmpeg) | PixelFormat | yes | yes | no | nv12 | `CaptureSettings.vb:59`, V1/V2 builders |
| Resolution | Native | width/height inputs | yes | **disabled**, show detected native | — | native size | `[5]:1092-1098`, `DetectNativeResolution:349` |
| Resolution | Custom | width/height inputs | yes | enabled + validated (320–7680 × 240–4320) | yes | 1920×1080 | `[5]:21-24`, `IsValidResolution:444` |
| Audio | System OFF | system volume slider | yes | disabled | no | — | `SessionConfig` consumes flags+volume together (`NextRecordingConfig.vb:88-89`) |
| Audio | Mic OFF | mic device / volume / separate-track | yes | disabled | no | — | `:90-95` |
| Audio | Mic ON, no device found | mic dropdown | yes | enabled, "0 mic(s)" status + refresh | warn | default device | `[6]:82-93` |
| Encoder | NVENC H264 / HEVC / AV1 | preset list p1–p7 | yes | yes | yes | p4 (index 4) | `MapNvencPreset:582-593`, `[5]:2009` |
| Encoder | QuickSync_* / LibX* | NVENC preset selector | yes | disabled (or switches UI to legacy-only group) | no | — | New Engine has only `NvencEncoderBackend` (`RecordingEngine.vb:77`) |
| Encoder | AMF_* | encoder entry | **hidden** | — | — | — | commented out `[5]:753-757` (NOT IMPLEMENTED) |
| Notifications | Slot toggles | slot3 toggle | yes | enabled only when slot2 on | — | slot2 on, slot3 off | `[7]:388-395` |
| Privacy | OFF | record/replay actions | — | blocked + toast | — | — | `Sub_Record.vb:144-151` |
| Recording state | active | privacy toggle, capture-affecting settings | yes | disabled | — | — | `[9]:57-67` precedent |

---

## 6. Canonical Field Mapping (UI CONTROL → config key → mapper → runtime)

Uses `docs/CONFIG_OWNERSHIP_MATRIX.md` as ownership source of truth. No new keys proposed here.

| UI CONTROL (writer) | config.json key | MAPPER | EFFECTIVE RUNTIME FIELD | REGIME |
|---|---|---|---|---|
| `[5] FPS_BOX` → `SaveSettingsNow:1194` | `Recording.current.fps` | `ApplyUnifiedToCaptureSettings` `OverlayConfig.vb:448` + `MapSessionConfig` `NextRecordingConfig.vb:93` | `SessionConfig.TargetFps` → CFR pacing + NVENC frameRateNum — **per record, PHASE 1 (V-CT1)**; legacy ffmpeg framerate unchanged | A→B |
| `[5] TrackBar_BITRATE` → `:1195` | `Recording.current.bitrate` | `:449` (kbps→bps) → `MapStartupConfig` `NextRecordingConfig.vb:139` | `EncoderConfig.BitrateBps` → NVENC `averageBitRate` at engine init (**PHASE 1, V-CT3**); legacy `-b:v` | B |
| `[5]` preset buttons → `:1198` | `Recording.current.encoder_preset` | **mapper NOW EXISTS (PHASE 1, V-CT4)**: `OverlayConfig.vb:455-456` → `NvencPreset` → `NextRecordingConfig.vb:156-159` (`MapNvencPresetInteger`) → NVENC preset GUID | runtime consumes config value; engine.json `Preset` = fallback only | A→B |
| `[5] Resolution_BOX` → `:1201-1212` | `current.use_native_resolution` / `width` / `height` | `:450-454` → `MapStartupConfig` → `ResolveEncodeDimensions` | NVENC encode dims at engine init (**PHASE 1, V-CT2**, GPU scale `RecordingEngine.vb:88-99`); legacy ffmpeg scale unchanged | A→B |
| `[5] cmbEncoder` → `:817-818,1190-1191` | `Recording.encoder` (+`encoder_now`) | legacy: `MapEncoderToFfmpeg` `:445,521-535`; new: `MapEncoderToInternal` `RecordingEngineHost.vb:91` | `EngineStartupConfig.CodecKey` → `EncoderConfig.CodecKey` (`RecordingEngine.vb:102`); legacy ffmpeg `-c:v` | B |
| `[5] TrackBar_Replaylast` → `:289` | `Recording.replay_duration` | none | **no runtime consumer** (replay NOT IMPLEMENTED, `UI_Engine.vb:524-534`) | — |
| `[6] chkSystem/chkMic/trk*/cboMic/rad*` → `SaveToSettings:109-142` | `Audio.*` (8 keys) | `ApplyUnifiedToCaptureSettings` `:483-503` (incl. `AudioClockMode:511-512`) | `SessionConfig` audio fields `NextRecordingConfig.vb:94-102` | **A (fresh)** |
| `MIC_ICO` overlay toggle (`Sub_Mouse.vb:250-260`) | `Audio.MicEnabled` | same as above | same as above | A |
| `[4]` labels (`SaveBinding:176-182`) | `Hotkeys.<ActionKey>` (10 keys) | `HotkeyService.Get/Set` `:40-53` | `RegisterAll` OS hotkeys `:74-86` | LIVE |
| `[9] TogglePrivacy` → `:53-54` | `Privacy.DesktopCaptureEnabled` | `Sub_Record` guard `IsPrivacyEnabled` | record gate | LIVE |
| Launcher `Use_Overlay` (`Main.vb:115-123`) | `Overlay.UseOverlayEnabled` | `AppConfigShared` shared readers | API hub keep-alive (`NVIDIA API.vb:131-133`) | LIVE |
| `[7]` toggles/slots | `Notifications.*`, `SlotCount` | `AppConfigShared` at display time (single choke point, `AppSettings.vb:186-191`) | Notifier toasts | LIVE |
| `[5]` auto-detect (`:558`) | `Paths.FFmpegPath` | `ApplyUnified:506-508` + FIX-1 reload `RecordingEngineHost.vb:191-211` | `SessionConfig.FFmpegPath` / ffmpeg args | **A** |
| `Sub_Misc:218` / `Gallery/Main:62` | `Paths.GalleryPath` / `SavePath` | `ApplyUnified:509-511` | `SessionConfig` output dir / `GetOutputDirectory:94-126` | A |
| `UI_Engine cboCaptureMethod` → engine.json | engine.json `CaptureMethod` | `LoadEngineSettings:279` | **legacy only** (`Validate:493`, V1/V2 builders); New Engine: echo log only (`RecordingEngine.vb:123-127`) | LEGACY |
| `UI_Engine nudFPS/nudBitrate/chkNativeRes/txtOutputDir/txtFFmpegPath` → engine.json (`SaveSettings:652-667`) | engine.json keys | `LoadEngineSettings:273-295` | legacy path; New Engine: only FFmpegPath via fresh reload | LEGACY/B |

**Key gap status (v1.1): CLOSED by PHASE 1.** v1.0 flagged that `Recording.current.encoder_preset` had no unified mapper and the New Engine consumed its engine.json twin frozen at init. Since `06667e9` the config value flows through a single mapper path (`OverlayConfig.vb:455-456` → `NextRecordingConfig.vb:156-159`). Remaining init-contract caveat: bitrate/preset/RC/resolution apply at ENGINE INIT, not per record — the UI must still label them "engine restart required"; FPS is per-record since PHASE 1 (V-CT1).

---

## 7. Duplicate UI Surfaces

**Answer to Final Question 1: there are 4 app-level UI surfaces that can write settings, plus 1 separate file-backed page (OBS) and 2 silent write-back paths.** Concretely:

| SURFACE | FILES IT WRITES | EVIDENCE |
|---|---|---|
| S1 — Overlay Settings pages | `config.json` | `AppSettings.Save():1023` |
| S2 — Engine WinForms `UI_Engine` | `engine.json` (+ `_settings` in memory) | `SaveSettings:652-667`; write-back `SyncWithOverlayConfig:493`; `PREWARM` write `:578-581` |
| S3 — Engine `AudioSettingsForm` | `audio.json` + `engine.json` + legacy `video.json` | `SaveToSettings:94-129`, `SaveToOverlayVideoJson:131-158` |
| S4 — Launcher `Use_Overlay` | `config.json` (Overlay section) via shared writer | `Launcher/Main.vb:118,123` |
| S5 — Notifications → OBS page | `Config/notifier_obs.json` (separate store) | `[7]:418-466` |
| S6 — silent: `SyncWithOverlayConfig` | `engine.json` re-written from unified config on every record start | `UI_Engine.vb:493` |
| S7 — silent: `SettingsExportImport` | `config.json` (bulk import) | `ImportWithDialog:135` |

Duplicated settings (same logical setting, ≥2 writer surfaces):

| SETTING | SURFACE #1 | SURFACE #2 | SURFACE #3 | CANONICAL OWNER | RISK |
|---|---|---|---|---|---|
| FPS | `[5] FPS_BOX` → config.json | `UI_Engine nudFPS` → engine.json | — | config.json `current.fps` | two files disagree; New Engine uses neither (display refresh) |
| Bitrate | `[5] TrackBar_BITRATE` → config.json | `UI_Engine nudBitrate` → engine.json | — | config.json `current.bitrate` | engine.json copy goes stale |
| Native/Custom resolution | `[5] Resolution_BOX` → config.json | `UI_Engine chkNativeRes+cboResolution` → engine.json | — | config.json | same |
| CaptureMethod / Capture API | `UI_Engine cboCaptureMethod` → engine.json | (config.json `api_capture` — persisted but no UI writer) | — | config.json (per matrix 🔴 move) | user edits in Engine never reach New Engine (hardcoded ddagrab) |
| Mic on/off | `[6] chkMic` | `MIC_ICO` overlay toggle (`Sub_Mouse:250`) | `AudioSettingsForm chkMic` | config.json `Audio.MicEnabled` | 3 writers, one file wins |
| Mic state sync | `UpdateMicStatus` (`Sub_Misc:156-165`) **derives state from icon glyph text** and writes it back | `[6]` | — | config.json | icon text is the source of truth — classic UI-guessed-state bug |
| System/Mic volume | `[6]` sliders (0–150%) | `AudioSettingsForm` sliders (0–150%) | `UI_Engine` mirror sliders (0–100% display `:216-217`) | config.json `Audio.*Volume` | ranges differ across surfaces |
| FFmpegPath | `[5]` auto-detect → config.json | `UI_Engine OnBrowseFFmpeg` → engine.json | `AudioSettingsForm` (engine.json) | config.json `Paths.FFmpegPath` (matrix 🔴 engine copy dies) | engine copy shadows config when config missing |
| Gallery/Save path | `Sub_Misc save_sc` | `Gallery txtFilePath` | (`UI_Engine txtOutputDir` — engine.json, **not even persisted** `Save():176-191`) | config.json `Paths.*` | Engine output-dir edit silently lost |
| Hotkeys | `[4]` → config.json dict (LIVE) | engine.json `HotkeyStart/Stop/Toggle` (DEAD twin) | — | config.json `Hotkeys` | hand-editing engine.json hotkeys does nothing |
| Encoder preset | `[5]` → config.json `encoder_preset` | engine.json `Preset` (the one runtime consumes) | — | config.json (unify per matrix 🔴) | user change invisible to New Engine |
| Replay duration | `[5] TrackBar_Replaylast` | `UI_Engine nudReplayDuration` (display-only mirror `:209-211`) | — | config.json `replay_duration` | mirror misleads |

---

## 8. Overlay Responsibility (target)

What the Overlay SHOULD own, based on what it already owns end-to-end today:

| AREA | OVERLAY RESPONSIBILITY | RUNTIME / ENGINE RESPONSIBILITY (sent, not held) |
|---|---|---|
| USER SETTINGS | All user-facing settings edit → `config.json` single store (already true: `[0][4][5][6][7][9]` + paths + GitHub) | none — Engine only *reads* config.json via `CaptureSettings.Load` |
| RECORD | `ToggleRecording` command sender + optimistic UI + privacy gate (`Sub_Record.vb:132-198`) | actual capture via `engine_record_start/stop` → RecordingEngine/CaptureEngine |
| STOP | same command pair (`:161-166`) | stop + mux finalize |
| REPLAY | UI exists (`ToggleInstantReplay:209-284`, `SaveInstantReplay:290-343`) | **NOT IMPLEMENTED at engine** — `engine_replay_start/stop/save` all respond `not_implemented` (`UI_Engine.vb:524-534`); Overlay must NOT present replay as functional until wired |
| AUDIO | settings pages + quick mic toggle (S1) | per-record fresh audio via `SessionConfig` (already regime A) |
| VIDEO | settings pages (`[5]`) | legacy args / NVENC init |
| OUTPUT | gallery + path pickers; `GetOutputDirectory` builds the record path and sends it in `RECORD_START` (`:173-177`) | engine writes the file; new path is MP4-structured |
| NOTIFICATIONS | toggles + slot count (`[7]`) | Notifier displays; `AppConfigShared` is the single read choke point |
| ENGINE LIFECYCLE | spawns/keeps alive via `EngineProcessSupervisor` (`EnsureEngineRunning:94`) | engine process itself |

Boundary rule: **Overlay never writes `engine.json`** (today it doesn't — engine.json writers are all Engine-side: `UI_Engine.vb:493,581,666`, `AudioSettingsForm.vb:122`). Keep it that way.

---

## 9. Engine WinForms Responsibility (target)

Current role mix (evidence §1.2): config editor + operator + diagnostics + mirror. Per the ownership matrix, the Engine WinForms should be reduced to **STATUS + DIAGNOSTICS + OPERATOR CONTROL**:

**KEEP (operator/diagnostic):**
- Record/Stop buttons (`OnRecordClick:775`, `OnStopClick:809`) — operator override path.
- Status panel: state label, timer, frames, size, actual-vs-target bitrate (`OnEngineStateChanged:964-1022`, `OnEngineProgress:1105-1146`) — this is already the best "Actual" telemetry in the product.
- Pipeline visibility: which engine ran (today only a log line, `[Engine] Client.vb:258`) — promote to a visible label.
- Stress test matrix (`OnStressTestClick:833-960`) — diagnostic tool.
- Hub status (`UpdateHubStatusUI:335-351`), config source label (`:81,254-256`).

**REMOVE as writable config (convert to read-only mirror or move to Overlay):**
- `nudFPS`, `nudBitrate`, `chkNativeRes`, `cboResolution` — duplicate Overlay writers (§7); New Engine ignores FPS/resolution anyway (regime C).
- `cboCaptureMethod` — legacy-only writer for a key the New Engine never reads.
- `cboEncoder` — transient only: `OnEncoderChanged:1154-1161` sets `_settings.Encoder` but `SaveSettings:652-667` never persists Encoder (engine.json `Save():176-191` has no Encoder key), and the next `SyncWithOverlayConfig` overwrites it from config.json. As a control it lies.
- `txtOutputDir` — its value is **silently not persisted** (`CaptureSettings.Save():176-191` writes no `OutputDirectory` key; only the reader `LoadEngineSettings:291` exists). Worst kind of control: appears to save, does not.
- `ValidateFFmpegPath:1225-1229` — empty-bodied validation (both branches empty) — either implement or remove.

**KEEP as background host:** the Engine window also hosts the TCP client, RecordingEngine lifetime, job guard, and the hidden `AudioSettingsForm` marker system (`OPEN_UI_Tick:1252-1264`) — those are runtime responsibilities, not UI config.

---

## 10. Runtime State vs UI State

Design rule: `UI State → Effective Config → Engine State → Actual Runtime` must converge, and UI must never *invent* state. Violations found (all with evidence):

| # | VIOLATION | EVIDENCE | FIX DIRECTION (spec only) |
|---|---|---|---|
| 1 | Optimistic record state can disagree with engine | Overlay sets `_isRecordingLocal = True` before any ack (`Sub_Record.vb:183`); reconciled only via `engine_state_changed` (`[Overlay] Client.vb:114-161`) | keep optimistic UX but surface last `engine_response` state; treat engine state as truth |
| 2 | Replay UI promises, engine cannot deliver | Overlay full replay UI (`Sub_Record.vb:209-343`) vs `not_implemented` (`UI_Engine.vb:524-534`) | disable replay group until engine implements it |
| 3 | Mic state derived from an icon glyph | `Sub_Mouse.vb:250-260` toggles by comparing `MIC_ICO.Text` PUA glyphs; `Sub_Misc.vb:156-165` writes config from the glyph | replace glyph-comparison with a bool field |
| 4 | Engine availability = process existence only | `EngineProcessSupervisor.vb:126,262` (`GetProcessesByName`) | add `engine_get_status` handshake (command exists: `UI_Engine.vb:536-542`, `RecordingEngineHost.vb:322-334`) |
| 5 | Engine mirror controls are editable and write engine.json | `RefreshOverlayConfigUI:173-261` fills `nudFPS` etc.; editable per Designer (`:290-294`); `SaveSettings:652-667` persists | make mirrors read-only; config.json is the only writable store |
| 6 | `engine_get_status` returns only `Idle/Recording` on legacy path | `HandleEngineGetStatus:536-542` (no `Initializing/Faulted` distinction that `RecordingEngineHost.vb:325` has) | unify status reporting on the new-engine variant |
| 7 | Encoder dropdown ≠ what runtime uses | `cboEncoder` selection never persisted, overwritten by sync (§9) | display mapped runtime encoder instead of an editable dropdown |
| 8 | Two 2s file-poll refresh loops + one TCP reload feed the same mirror | `OnRefreshTick:286-330` (LastWriteTime poll), `HandleEngineConfigChanged:602-629` (FIX-2 reload) | keep TCP as the push channel; poll only as fallback |

---

## 11. Validation UX (spec — do NOT implement here)

Existing validation facts to build on:
- Overlay video caps: bitrate 500–150000 kbps, FPS 1–800 UI input caps (`[5]:13-19`); dynamic per-resolution limits `GetBitrateLimits:370-389`, `GetFPSLimits:401-420`; `ValidateBitrate:458`, `ValidateFPS:463`.
- Engine contract caps differ: FPS 1–240, bitrate 1–200 Mbps (`CaptureSettings.Validate():483-507`), engine mirror 1–240 (`Designer:292-293`).
- Hotkeys: modifier required + duplicate detection + mouse buttons rejected (`HotkeyService.vb:161-171`, `[4]:144-154`).
- Audio volume: UI 0–150% vs model doc 0.0–1.0 (`[6]:54-55,115-116` vs `AppSettings.vb:87-94`).

| FIELD | INVALID | WARNING | AUTO-FIX | BLOCK | REASON |
|---|---|---|---|---|---|
| FPS | >240 or <1 | 1–800 range accepted by UI but engine rejects >240 (`:484-486`) — UI and engine caps disagree | clamp to engine cap with notice | no | one number must not be "valid" in Overlay and rejected by Engine |
| Bitrate | <500 kbps or >150000 kbps (UI), engine <1 Mbps or >200 Mbps | recommend per-resolution min/max (`RecommendedMin/Max:31-40`) | clamp to resolution table | only when file size math is impossible | two caps exist today; unify on engine cap |
| Resolution | outside 320×240–7680×4320 | custom ≷ native monitor | snap to native (`DetectNativeResolution:349`) | no | `IsValidResolution:444` |
| Encoder capability | NVENC requested, no NVENC (detection failed) | QuickSync/LibX selected while New Engine runs | suggest legacy fallback + explicit user choice | **BLOCK New-Engine record for non-NVENC** until multi-vendor encoder exists | `RecordingEngine.vb:77` constructs NVENC unconditionally |
| Capture API capability | gfxcapture chosen on machines without the filter | — | fallback ddagrab + toast | no | `PipelineResolver.vb:104-118` precedent |
| Audio device | selected mic missing at record time | "0 mic(s) found" state (`[6]:92`) | fall back to default device + toast | record continues without mic | `Validate():503-505` already blocks with message |
| FFmpeg path | file missing | stale path in engine.json | `[5]` auto-detect (`:602-619`) + `PREWARM_FFMPEG` handshake | **BLOCK legacy record** (fail fast `UI_Engine.vb:379-383`) | existing behavior is correct — keep |
| Hotkey | no modifier, duplicate | conflict with OS | none | reject at capture (`[4]:144-153`) | existing |
| Slot count | out of 1–3 | — | clamp (`AppSettings.vb:1017`) | no | existing |

---

## 12. UI Diagnostic Panel ("effective runtime") — SPEC

Goal: open one panel and know **what the Engine is actually recording with right now**. Four columns per row: `Requested` (config.json) → `Effective` (post-mapper CaptureSettings/SessionConfig) → `Actual` (runtime telemetry) → `Output` (file truth via ffprobe).

Existing building blocks (reuse, don't reinvent):
- Requested↔Effective echo already exists in logs: `[RecordingEngine] effective config (fresh reload): …` (`RecordingEngineHost.vb:211`) and `RecordingEngineHost` FIX-1 publish.
- **PHASE 1 added three first-class truth lines** (the panel's first rows are already logged): capture-method `requested → selected → actual` with GAP marking (`RecordingEngine.vb:123-127`), pixel-format truth incl. BLOCKER P1-PIXFMT (`:130-135`), fps source line (`CaptureSession.vb:500`), plus resolution echo (`RecordingEngine.vb:101-105`).
- Actual telemetry exists: `OnEngineProgress` frames/size/actual-bitrate (`UI_Engine.vb:1105-1146`), NVENC init line, capture init line (`RecordingEngine.vb:79`).
- Output truth: ffprobe acceptance layer (matrix §3, Configuration Truth Rule) + `SessionResult` (`RecordingEngineHost.vb:290-293`).
- A legacy command preview already exists but is DEAD: `GET_FFMPEG_ARGS` sent by `[5]:1924` has **no engine handler** (`[Engine] Client.vb` ignores non-`engine_*` commands `:196`) — `prearg` always shows "engine not connected". The new panel should replace it.

Panel spec:

| ROW | REQUESTED (config.json) | EFFECTIVE (mapper output) | ACTUAL (runtime) | OUTPUT (file) |
|---|---|---|---|---|
| Engine | — | `_useNewEngine` + reason (`RecordingEngineHost.vb:47,117`) | pipeline marker per record (`[Engine] Client.vb:258`) | which mux wrote the file |
| Capture API | `api_capture` / engine.json `CaptureMethod` | resolved backend — PHASE 1 echo: requested→selected→actual (`RecordingEngine.vb:123-127`) | `DdagrabBackend initialized … @ Hz` (`RecordingEngine.vb:79`) | ffprobe codec/dimensions |
| FPS | `current.fps` | PHASE 1: config is the single owner — `SessionConfig.TargetFps` (`NextRecordingConfig.vb:93`) | CFR target `@ {targetFps}fps` (`CaptureSession.vb:500,549`) + measured fps | ffprobe r_frame_rate |
| Resolution | `current.use_native_resolution` + W×H | `ResolveEncodeDimensions` output (`RecordingEngine.vb:88-99`) | encoded size per frame | ffprobe width/height |
| Encoder | `Recording.encoder` | internal key after `MapEncoderToInternal` (`RecordingEngineHost.vb:91`) | NVENC init line (`RecordingEngine.vb:102-112`) | ffprobe codec_name |
| Pixel Format | engine.json `PixelFormat` | truth: config not honored (BLOCKER P1-PIXFMT, `RecordingEngine.vb:130-135`) | BGRA8 capture → ARGB NVENC input | ffprobe pix_fmt |
| Preset | `current.encoder_preset` | `MapNvencPreset` result (`OverlayConfig.vb:582-593`) | preset in init line | — |
| Bitrate | `current.bitrate` | bps after ×1000 (`:449`) | `Actual: X Mbps / target Y` (`UI_Engine.vb:1128-1133`) | ffprobe bit_rate |
| Audio | `Audio.*` | `SessionConfig` audio fields (`NextRecordingConfig.vb:94-102`) + clock mode | `effective config` echo (`RecordingEngineHost.vb:211`), `SessionResult` audio counters (`:291-293`) | ffprobe audio streams |
| Output | `Paths.GalleryPath/SavePath` | `SessionConfig.OutputPath` | file size live (`UI_Engine.vb:1112-1123`) | ffprobe duration/streams |

Placement: this panel belongs in the Engine WinForms (diagnostic role, §9) mirrored read-only into the Overlay's engine-status area; data transported over the existing TCP channel (`engine_recording_progress` precedent, `UI_Engine.vb:1139-1143`).

---

## 13. Target UI Structure

Logical hierarchy mapped to actual config/runtime nodes (every leaf names its owner file — no invented keys):

```text
SETTINGS (Overlay — S1, writes config.json only)
│
├─ ENGINE                         ← NEW SECTION (see §14 order)
│  ├─ Engine pipeline (read-only status: New native / Legacy FFmpeg / fallback reason)
│  │     runtime truth: RecordingEngineHost.vb:42,117 · [Engine] Client.vb:247-259
│  └─ Capture API (ddagrab | gdigrab | gfxcapture)
│        owner: config.json Recording.api_capture (key exists: AppSettings.vb:371;
│        mapped: OverlayConfig.vb:466-467 → legacy builders; New Engine: built-in ddagrab
│        RecordingEngine.vb:76 — control disabled there; non-ddagrab request = honest GAP echo
│        RecordingEngine.vb:123-127; no GfxCapture native backend)
│
├─ VIDEO                          ← page [5] (Base_RecordingsSet), owner of Recording.*
│  ├─ FPS                → Recording.current.fps            (WIRED PHASE 1 — per record ✅ V-CT1)
│  ├─ Resolution         → current.use_native_resolution/width/height (WIRED PHASE 1 — engine init, label it ✅ V-CT2)
│  ├─ Encoder            → Recording.encoder                (B — init contract)
│  ├─ Preset             → current.encoder_preset           (WIRED PHASE 1 — single mapper ✅ V-CT4)
│  ├─ Bitrate            → current.bitrate                  (WIRED PHASE 1 — engine init, label it V-CT3)
│  ├─ Rate Control       → engine.json RateControl (lands in NVENC struct); move to config.json per matrix 🟡
│  └─ Pixel Format       → engine.json PixelFormat; BLOCKER P1-PIXFMT — UI must show "runtime BGRA/ARGB" 🔴
│
├─ AUDIO                          ← page [6] (Base_AudioSet), owner of Audio.* (regime A ✅)
│  ├─ System Audio (+volume 0–100 spec; 150% UI overshoot fixed per §11)
│  ├─ Microphone (+device by MicDeviceId, volume)
│  ├─ Tracks      → Audio.TrackMode (NextRecordingConfig.vb:94-95)
│  └─ Clock       → Audio.AudioClockMode (diagnostic knob until P13.5; surfaced read-only)
│
├─ OUTPUT
│  ├─ Gallery   → Paths.GalleryPath (ONE picker only — kill the duplicate writers §7)
│  ├─ Format    → engine.json FileFormat today; MP4-only in New path (LiveMuxSession.vb:18,306)
│  └─ FFmpeg    → Paths.FFmpegPath (auto-detect + manual override)
│
├─ HOTKEYS                        ← page [4], owner config.json Hotkeys (10 actions, live)
│
├─ OVERLAY                        ← Launcher-owned toggle today (Launcher/Main.vb:115-123);
│  │                                Overlay shows it read-only with owner note
│  └─ OBS bridge        → notifier_obs.json editor (page [7] OBS block, keep)
│
└─ NOTIFICATIONS                  ← page [7], owner Notifications.* (24 toggles + SlotCount 1–3)
```

Engine WinForms (S2) keeps: status, diagnostics panel (§12), operator record/stop, stress test — **no writable settings controls** (§9).

---

## 14. Migration / Implementation Order

Ordered so that no step creates a second apply-path or breaks a working group (respects matrix §6/§7 do-not-rush rules):

1. **Freeze duplicates (UI-level, low risk)** — Engine WinForms mirrors become read-only; `cboEncoder`/`txtOutputDir`/`cboCaptureMethod` lose write paths; `AudioSettingsForm` stops writing `video.json`/`engine.json` once Overlay `[6]` is confirmed the sole writer. Evidence anchors: `UI_Engine.vb:652-667`, `AudioSettingsForm.vb:94-129`.
2. **Mic toggle state fix** — replace `MIC_ICO` glyph-comparison with a bool (`Sub_Mouse.vb:243-260`, `Sub_Misc.vb:156-165`).
3. **Paths unify** — `Paths.FFmpegPath` single owner (matrix wire item #1); remove GalleryPath double-writer (`Gallery/Main.vb:62` vs `Sub_Misc.vb:218`).
4. **Diagnostics panel (§12)** — buildable NOW: it only reads existing echoes/logs/progress events; also removes the dead `GET_FFMPEG_ARGS` feature (`[5]:1924`).
5. **ENGINE + Capture API section** — after Phase 0 lands a canonical key for engine selection (does NOT exist today — must be added by OWNER/Phase 0, not invented by UI) and `api_capture` is wired (matrix wire item #5). Until then show read-only pipeline status only.
6. **VIDEO-phase wiring** — ✅ FPS + resolution group WIRED (PHASE 1, V-CT1/2, verified `CaptureSession.vb:495`, `RecordingEngine.vb:88-99`); remaining: PixelFormat (BLOCKER P1-PIXFMT — needs GPU conversion layer), native gfxcapture backend, `api_capture` UI writer on config.json. UI labels: FPS "live per record"; resolution/bitrate/preset "engine restart required"; PixelFormat honest "runtime BGRA/ARGB".
7. **Encoder group** — bitrate/preset/RC land in native NVENC structs at INIT (V-CT3/4/5, verified `RecordingEngine.vb:102-112`); remaining: per-record NVENC re-init OR explicit restart contract + UI badge before `[5]` preset/encoder can promise next-recording effect.
8. **Replay** — UI stays disabled until engine implements `engine_replay_*` (today `not_implemented`, `UI_Engine.vb:524-534`); `replay_duration` stays "reserved".
9. **Cleanup** — delete engine.json dead twins (HotkeyStart/Stop/Toggle, PixelFormat if retired) only after 1–6 are green per matrix §6.6.

---

## 15. Acceptance Criteria

For this spec to be considered implemented (future phases, each with path+method+line evidence + test per matrix §9):

1. `config.json` is the only user-writable store: grep-level proof that no UI control writes `engine.json`/`video.json`/`audio.json` (today: `UI_Engine.vb:493,581,666`; `AudioSettingsForm.vb:117-128`).
2. One UI writer per setting (§7 duplicate table resolved); mic state no longer derived from icon text (`Sub_Mouse.vb:251-255`).
3. Engine WinForms shows Requested/Effective/Actual/Output with no writable config controls; pipeline (new/legacy + fallback reason) visible without reading logs.
4. Every regime-B control displays its regime (restart-required badge) and every BLOCKER-honoring control displays its blocker (PixelFormat) — no control promises an effect the runtime does not deliver (matrix §2 regimes).
5. Validation caps unified: Overlay UI caps == `CaptureSettings.Validate()` caps (FPS 240, bitrate 200 Mbps) with tests — caps themselves unchanged by PHASE 1 (verified at `06667e9`).
6. Volume UI range matches model contract (0–100% or model updated to 1.5) — one decision, documented.
7. ~~`Recording.current.encoder_preset` has a single mapper path into the runtime~~ ✅ **SATISFIED by PHASE 1** (`OverlayConfig.vb:455-456` → `NextRecordingConfig.vb:156-159`, tests V-CT4/4b/5f) — remaining: the preset control may go "live" only together with the restart-contract badge (item 4).
8. Replay UI disabled until `engine_replay_start` stops returning `not_implemented` (`UI_Engine.vb:524-526`).
9. Non-NVENC encoder selection either routes to legacy explicitly or is blocked with reason (never silently fails — `NvencEncoderBackend` still constructed unconditionally at `RecordingEngine.vb:82`).
10. All changes ship with ConfigTruth-style tests (CT pattern, FAIL-first) + real-record ffprobe confirmation (matrix §9).

---

## FINAL QUESTIONS — answered from source at `ab89372`

**1) ตอนนี้ UI มี config surface กี่แห่งที่สามารถแก้ setting เดียวกันได้?**
**4 app surfaces + 1 file-backed page + 2 silent write-backs.**
- S1 Overlay Settings (`config.json`), S2 Engine `UI_Engine` (`engine.json`), S3 Engine `AudioSettingsForm` (`audio.json`+`engine.json`+`video.json`), S4 Launcher toggle (`config.json` Overlay section), S5 OBS page (`notifier_obs.json`), S6 `SyncWithOverlayConfig` silent engine.json rewrite (`UI_Engine.vb:493`), S7 `SettingsExportImport` bulk import (`:135`).
- Settings editable from ≥2 surfaces today: FPS, Bitrate, Native/Custom resolution, CaptureMethod, Mic on/off (3 surfaces), System/Mic volume (3 surfaces, differing ranges), FFmpegPath (2 files), Gallery path (2 controls), Encoder preset (2 keys), Hotkeys (live + dead twin), Replay duration (writer + mirror). Full table §7.

**2) Engine selector + Capture API selector ควรอยู่ที่ UI ไหน?**
- ทั้งคู่อยู่ใน **Overlay Settings → section ENGINE ใหม่** (§13) เพราะ Overlay เป็นเจ้าของ config.json และเป็น surface เดียวที่ user ใช้ตั้งค่าอยู่แล้ว
- แต่ **Engine selector ยังสร้างไม่ได้จนกว่า Phase 0 จะมี canonical key** (config.json วันนี้ไม่มี key สำหรับ engine selection; runtime เลือกเองที่ `RecordingEngineHost.vb:42,117`) — ห้าม UI ไปเดา/invent key; ระหว่างนั้นแสดงสถานะ read-only
- **Capture API selector สร้างได้เลยบน key ที่มีอยู่** (`Recording.api_capture`, `AppSettings.vb:371`) — แต่ต้อง disable+label "built-in ddagrab" เมื่อ runtime ใช้ New Engine (`RecordingEngine.vb:71`), และ gfxcapture มีผลเฉพาะ legacy FFmpeg path
- Engine WinForms ไม่ควรเป็นที่ตั้งค่าทั้งสองอย่างนี้ (§9)

**3) Engine WinForms ควรเป็น configuration UI หรือ diagnostic/operator UI เป็นหลัก?**
**Diagnostic/operator UI เป็นหลัก** — เก็บ record/stop, status panel, stress test, hub status, diagnostics panel (§12); ถอน writable config ออกทั้งหมด (§9) เพราะ (a) มันเขียน engine.json ทับความหมายเดียวกับ config.json, (b) `cboEncoder` ไม่เคยถูก persist (`Save():176-191`), (c) `txtOutputDir` เขียนแล้วหาย (`Save():176-191` ไม่มี OutputDirectory), (d) `ValidateFFmpegPath:1225-1229` เป็น empty body — ปัจจุบันมันเป็น config surface ที่ "โกหก" ผู้ใช้

**4) หลัง Phase 0 config contract นิ่งแล้ว UI สามารถสร้างจาก canonical config เดียวได้หรือยัง?**
**ได้กลุ่มใหญ่ขึ้นมากแล้วตั้งแต่ PHASE 1 (v1.1) — เหลือ 3 จุดที่ยังรอ:**
- พร้อมตอนนี้ (regime A, wired): Audio ทั้งกลุ่ม (`NextRecordingConfig.vb:94-102`), Paths (FFmpegPath fresh หลัง FIX-1 `RecordingEngineHost.vb:191-211`), Hotkeys, Privacy, Overlay, Notifications, UI
- **WIRED แล้วตั้งแต่ PHASE 1 (V-CT1–5, ยืนยันที่ `06667e9`)**: FPS (per record `NextRecordingConfig.vb:93` → `CaptureSession.vb:495`), Resolution (engine init `RecordingEngine.vb:88-99`), Bitrate/Preset/RC (engine init `:102-112` + `NextRecordingConfig.vb:139,156-159`) — UI สร้างได้จาก canonical config เดียว แต่กลุ่ม init-contract ต้องติด label "engine restart required"
- ยังติด: **PixelFormat** (BLOCKER P1-PIXFMT — runtime BGRA/ARGB ไม่มี conversion layer `RecordingEngine.vb:130-135`), **gfxcapture native** (recorded GAP `:123-127`), `api_capture` ยังไม่มี UI writer บน config.json
- ยังไม่มี key เลย: **engine selection** — ต้องให้ Phase 0/OWNER ออกแบบก่อน (runtime ยังเลือกเองที่ `RecordingEngineHost.vb:42`)

## VERDICT

```text
WAIT FOR CONFIG  (v1.1 — เงื่อนไขแคบลงมากจาก v1.0)
```
เหตุผล (อัปเดตที่ `06667e9`): กลุ่มวิดีโอหลัก (FPS/Resolution/Bitrate/Preset/RC) wired เข้า runtime จริงแล้วตั้งแต่ PHASE 1 (V-CT1–5) — UI สร้างกลุ่มนี้จาก canonical config เดียวได้ (ติด label init-contract ให้ถูก) แต่ยังต้องรอ 3 อย่างก่อนสร้าง UI ฉบับเต็ม: (1) **engine selection ยังไม่มี canonical key** — runtime เลือกเอง (`RecordingEngineHost.vb:42`), (2) **PixelFormat BLOCKER P1-PIXFMT** (`RecordingEngine.vb:130-135`), (3) **UI validation caps ยังไม่ unify** (FPS 1–800 บน Overlay vs 1–240 `CaptureSettings.Validate()`) — การสร้างก่อนครบ = สร้าง "UI guessed state" ซ้ำขึ้นมาใหม่
เริ่มได้ทันทีโดยไม่ติด config: กลุ่ม A ทั้งหมด + กลุ่ม VIDEO ที่ wired + งานลด UI-guessed state ของ v1.0 (mirror ฝั่ง Engine read-only, diagnostics panel §12, mic-state fix, ลบ duplicate writers)



