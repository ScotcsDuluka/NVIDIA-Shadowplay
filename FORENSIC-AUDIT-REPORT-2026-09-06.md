# FULL REPOSITORY FORENSIC REPORT — NVIDIA-Shadowplay

> **Audit type:** ULTRA-LEVEL static forensic audit (no code changes made by this audit)
> **Repository:** `C:\My Project\NVIDIA-Shadowplay`
> **Branch:** `Engine-Rebuild-Stabilization` (per HANDOFF.md:67; `git` not on PATH on this machine, direct git verification = UNKNOWN; `.git` directory present)
> **Canonical production version (per request):** `3.41.3590.61` — **not found anywhere on disk** (see §13/§14)
> **Audit window:** 2026-09-05 22:40 → 2026-09-06 (multi-agent scan + independent verification pass)
> **Scale:** 34 project files, ~86,000 source lines (VB.NET majority + C# audio/interop + 6 C# spike projects ≈ 36K lines)
>
> ⚠️ **VOLATILITY NOTICE:** the working tree was **modified during the audit window**. Evidence:
> (a) repo-root artifact directories (`-i`, `-vf`, `-y`, `cbr`, `cfr`, `7000000`, `hwdownload,format=bgra,format=nv12`, …) existed at 2026-09-05 22:42 (observed by direct listing) and were gone by the deep-scan pass;
> (b) `$null` (0 B), `_tmp_build.ps1`, and `scotcsduluka the best dev.zip` likewise disappeared;
> (c) `CaptureSession.vb` grew (1,072 → 1,196 → ~1,252 lines) and gained an `_audioEngine` dispose-hardening block (line 1159-1160) **while the audit was running**;
> (d) new projects `Engine.Concurrency.Tests\` and `Notifier.Obs.Resilience.Tests\` appeared on disk during the window (neither is in `Overlay\NVIDIA Overlay.sln`).
> Conclusion: a parallel engineering session is actively editing this repo. All findings below reflect disk state at verification time and cite file:line evidence; line numbers may drift with ongoing edits.

---

## 1. REPOSITORY MAP

### 1.1 Actual structure (as found — differs from the request's assumed Production/Tests/Tools layout)

| Directory | Role | Language | Notes |
|---|---|---|---|
| `API\` | **"NVIDIA API.exe"** — TCP hub, port 5000, pure broadcast relay | VB.NET WinExe | also runs 1 s supervisor for the app family |
| `Launcher\` | **"Launcher.exe"** — family launcher UI, spawns the hub | VB.NET WinExe | has its own `NVIDIA Shadowplay Helper.slnx` |
| `Notifier\` | **"NVIDIA Notifier.exe"** — toast system (3 slots), OBS WebSocket bridge | VB.NET WinExe | `Notifier-API.slnx` single-project solution |
| `Overlay\` | **"NVIDIA ShadowPlay.exe"** — main UI/overlay; ProjectReferences ALL other apps | VB.NET WinExe | hosts the only real `.sln` (23 projects); `ManualOverlayTest` variant project |
| `Engine\` | **"NVIDIA Capture.exe"** — engine host: NEW `RecordingEngineHost` + LEGACY `CaptureEngine` (2-process FFmpeg) | VB.NET WinExe | 19 files ≈ 10K lines; contains `UI_Engine` diagnostic console |
| `CaptureEngine\` | Session orchestration core, FFmpeg command builders V1/V2, Configuration stack (EngineConfigV2/ConfigLoader/…) | VB.NET lib | net10.0 |
| `CaptureEngine.Video` | Frame contract (`Contract\*` + legacy `Frames\*`), `BoundedVideoFrameSink`, factories | VB.NET lib | ships Fake backend in production dll |
| `CaptureEngine.Video.Ddagrab` | DXGI Desktop Duplication backend, `D3D11VideoFrame` | VB.NET lib | Vortice 3.6.2 |
| `CaptureEngine.Encoder` (+`.Nvenc`) | Encoder abstraction; native NVENC backend (`NvEncodeAPI`, function table, serializer) | VB.NET lib | ships Fake backend in production dll |
| `CaptureEngine.FFmpegBackend` | LiveMuxSession/PipeFeed, WavSidecar stack, AudioTap v2/v3 | VB.NET lib | most of it legacy-dormant (§4) |
| `CaptureEngine.Recording` | `CaptureSession`, `RecordingEngine`, `DeferredVideoFrameDisposer`, `AudioEngineMuxSink` | VB.NET lib | the "Duluka" state machine |
| `CaptureEngine.Audio` / `.Audio.Wasapi` | `AudioEngineSession` (C#), WASAPI position-aware capture (C#) | C# libs | Linux-testable design |
| `CaptureEngine.Recording.ConsoleDriver` | Phase-12b validation driver (diagnostic only) | VB.NET Exe | not in product payload |
| `Common\` | `AppLayout` (assembly resolver + cwd), `AppConfigShared`, `ObsConfig`, `AppLayoutStartup` | VB.NET | linked-compiled into every app |
| `NVIDIA Controls\` | `ToggleSwitch` — designer/Toolbox shim only | VB.NET lib | AnyCPU-only |
| Tests (8+2) | `CaptureEngine.Tests`, `.Video.Tests`, `.Encoder.Tests`, `.Recording.Tests`, `.FFmpegTests`, `.FrameContractTests`, `.ConfigTests`, `Engine.ConfigTruth.Tests`, + NEW: `Engine.Concurrency.Tests`, `Notifier.Obs.Resilience.Tests` | VB.NET/C# | custom harness runners (no xUnit); 4 test projects **link-compile production sources** |
| `spikes\` | 6 throwaway probes (V1–V5, Ddagrab_Nvenc integration) | C# | ancestors of production interop; stranded on net8.0 |
| `scripts\` | `build-all.ps1/.sh`, `layout.proj`, validation/diag scripts | PS1/proj | see §13 |
| `installer\` | `NVIDIA ShadowPlay.iss` (Inno Setup 6.7+) | iss | sources from **dev bin**, not dist (§14) |
| `docs\` | 29 markdown files (PHASE_PLAN, ARCHITECTURE, APP-LAYOUT, P13, P1-F NVENC series, audits) | md | some stale (§4) |
| `Web\` + `wrangler.jsonc` | Static marketing site → Cloudflare Worker assets deploy | html | no secrets; version drift vs product |
| `dist\`, `dist-installer\` | Staged product tree + built installer (122 MB) | — | gitignored, disk-only |
| `evidence\`, `feedback-logs\` | Test evidence (gitignored; **disk-only source code** `loopback-probe`), end-user logs (`i kill people\`) | — | §16 privacy note |
| `.github\workflows\` | **one workflow only**: `deploy-web.yml` (Web → gh-pages) | yml | no product CI |

### 1.2 Per-project matrix (condensed; full data in dependency agent report)

- **TFM census:** `net10.0-windows10.0.26100.0` ×8 (the 5 apps + NVIDIA Controls + ManualOverlayTest + Engine.Concurrency.Tests); plain `net10.0` ×13 (engine core + most tests); `net10.0-windows` ×6 (Recording, Encoder.Nvenc, Video.Ddagrab, Video.Tests, ConsoleDriver, loopback-probe); **net8.0\* ×6 (all 6 spikes + apphost_test)** — TFM drift between spikes and production they informed.
- **OutputType:** 6 WinExe (apps), 1 Exe (ConsoleDriver) with `UseWindowsForms=true` (needed because linked `CaptureSettings.vb` probes `Screen.PrimaryScreen`), 1 test Exe (Engine.Concurrency.Tests) **referencing a WinExe as a library** (`..\Engine\NVIDIA Capture.vbproj`), rest Libraries.
- **PackageReferences:** Newtonsoft.Json **13.0.4 everywhere it appears** (6 projects) — consistent; Vortice.Direct3D11/DXGI **3.6.2** everywhere — consistent; NAudio **2.3.0** production (Engine full family ×7, Recording `.Wasapi`) vs **2.2.1** in loopback-probe + D3D11 spike; SharpGen.Runtime only transitive via Vortice but **filename-swept by build targets**.
- **No root solution file.** `Overlay\NVIDIA Overlay.sln` (23 projects) omits: ManualOverlayTest, Engine.ConfigTruth.Tests, Engine.Concurrency.Tests, Notifier.Obs.Resilience.Tests, loopback-probe, apphost_test, spikes.
- **No production→test project reference anywhere** (clean directionality). Test→WinExe-as-library exists once (above).
- **Overlay ProjectReference chain:** Overlay → {API, Launcher, Engine, Notifier, Controls}; Launcher → Controls; Engine → CaptureEngine.Recording → {CaptureEngine, Video, Video.Ddagrab, Encoder, Encoder.Nvenc, FFmpegBackend, Audio, Audio.Wasapi}. **DAG confirmed, no cycles.**

---

## 2. DEPENDENCY GRAPH (with severity)

```
Launcher.exe ──> NVIDIA Controls
     │ (Process.Start)
     ▼
NVIDIA API.exe (hub :5000) ── supervises ──► Notifier.exe / NVIDIA ShadowPlay.exe / NVIDIA Capture.exe
     │ all four apps share (linked-compile): Common\AppLayout, AppConfigShared (, ObsConfig)
     ▼
NVIDIA ShadowPlay.exe (Overlay) ──ProjectReference──► API, Launcher, Engine, Notifier, Controls
     ▼
Engine\NVIDIA Capture.vbproj ──► CaptureEngine.Recording ──► {Video.Ddagrab, Encoder.Nvenc, FFmpegBackend, Audio, ...}
     ▼                                                ▼
  Vortice.Direct3D11/DXGI 3.6.2              NAudio.Wasapi 2.3.0
```

Findings (each with severity + evidence):

| # | Finding | Severity | Evidence |
|---|---|---|---|
| 2.1 | **No cycle, no production→test edge, no production→tooling edge** | INFO (clean) | dependency sweep of all 34 project files |
| 2.2 | **TFM drift**: apps on `net10.0-windows10.0.26100.0`, engine libs on plain `net10.0`/`net10.0-windows`, spikes stranded on net8.0; `hosttest.csproj` net8.0 | LOW | csproj census; `CaptureEngine.Video.Ddagrab.vbproj:5-7` documents the net8.0→net10 migration |
| 2.3 | **NAudio version skew** 2.3.0 (production) vs 2.2.1 (loopback-probe, D3D11 spike) | LOW | `loopback-probe.csproj:12` (documented: deployed NAudio.Wasapi.dll is trim-stripped, probe must use stock NuGet) |
| 2.4 | **Test exe → WinExe-as-library**: `Engine.Concurrency.Tests.vbproj:21 → ..\Engine\NVIDIA Capture.vbproj` | MEDIUM (fragile shape; Test not in sln) | vbproj line 21 |
| 2.5 | **Link-compile duplication instead of ProjectReference** (to keep Linux builds free of WinForms/D3D): ConsoleDriver + Engine.ConfigTruth.Tests link `CaptureSettings.vb`, `OverlayConfig.vb`, `NextRecordingConfig.vb`, `ConfigMigrator.vb`, `EngineConfigV2.vb`, `AppLayout.vb`; Recording.Tests links `DeferredVideoFrameDisposer.vb`; Encoder.Tests links `NvEncodeAPI.vb`+`NvEncParamBuilder.vb`; Notifier.Obs.Resilience.Tests links `ObsWebSocketClient.vb` | MEDIUM (silent divergence risk: tests can pass against a stale copy) | vbproj Compile Include lines |
| 2.6 | **Stale packages dir** `Overlay\packages\` (NAudio family, System.*.10.0.5) — no `packages.config`; all projects SDK-style | LOW (disk junk) | dir listing |
| 2.7 | **Unused-package risk**: NAudio full family (7 dlls) referenced by Engine although the legacy audio path now delegates to `CaptureEngine.Audio` — NAudio.WinForms/Asio/Midi have no visible call sites in Engine (POSSIBLY DYNAMIC via NAudio.Wasapi types) | LOW | Engine vbproj :47-53; grep found no direct NAudio.* usage beyond Wasapi capture classes |
| 2.8 | `System.Management 10.0.11` only in Overlay (SystemMonitor) — consistent; no duplicate package versions across production | INFO | Overlay vbproj :151 |

**Transitive surprises:** `SharpGen.Runtime` + `SharpGen.Runtime.COM` reach the product only via Vortice but are **explicitly classified by name** in `Directory.Build.targets:256-257,273,286` — a third SharpGen-named assembly would silently fall into `Core\` catch-all. `Microsoft.Windows.SDK.NET.dll`/`WinRT.Runtime` ride the `net10.0-windows10.0.26100.0` TFM and are swept to `Core\` — this is why plain `net10.0` engine libs cannot touch WinRT (by design).

---

## 3. RUNTIME DEPENDENCY GRAPH (from the real executables)

```
Launcher.exe (root)
 ├─ Application\NVIDIA API.exe   (apphost → ..\Services\NVIDIA API.dll)
 ├─ reads/writes Config\config.json, HKCU\...\Run (SetStartup), Logs\kill_error.log
 └─ Flags\Ready (delete)
NVIDIA API.exe (hub)
 ├─ TcpListener IPv6Any:5000 (IPv4 fallback), 12×5s bind retry, 32-client cap, 60s reaper
 ├─ spawns/keeps-alive (1s tick): Application\NVIDIA Notifier.exe, Overlay\NVIDIA ShadowPlay.exe,
 │                                Application\NVIDIA Capture.exe  (kills all when Overlay.UseOverlayEnabled=false)
 └─ Config\config.json (read-only)
NVIDIA Notifier.exe
 ├─ TcpClient to hub (DIVERGED pre-hardening copy), reads Config\notifier_obs.json (2s watcher)
 ├─ Config\config.json (Notifications gates, SlotCount, UI.Language), Logs\notifier_obs.log
 └─ ffprobe (replay duration) from product FFmpeg\
NVIDIA ShadowPlay.exe (Overlay)
 ├─ spawns Notifier.exe; EngineProcessSupervisor spawns/respawns NVIDIA Capture.exe (2s+backoff)
 ├─ TcpClient to hub (hardened copy) — RECORD_START/RECORD_STOP/REPLAY_*/PREWARM_FFMPEG/engine_get_status
 ├─ reads/writes Config\config.json (AppSettings owner), Config\notifier_obs.json (writer)
 ├─ Flags\Ready / Flags\Engine.UI / Flags\Audio.UI; Data\NVIDIA_Shadowplay_Data\* sentinels
 ├─ FFmpeg\ffmpeg.exe probes (EncoderService), ffplay, ms-screenclip, explorer, GitHub OAuth HttpListener
 └─ Resources\nvgcshare.ttf, Languages\*.json, Fonts
NVIDIA Capture.exe (Engine)
 ├─ TcpClient to hub (diverged copy + OnReconnected) — engine_record_* commands
 ├─ NEW engine: RecordingEngine → DdagrabBackend(DXGI) → NvencEncoderBackend(nvEncodeAPI64.dll, OS loader search)
 │            → LiveMuxSession → FFmpeg\ffmpeg.exe (named pipes) → frag-MP4 → remux
 ├─ LEGACY engine: CaptureEngine → FFmpeg subprocess (ddagrab/gdigrab/gfxcapture + QSV branch) → WAV sidecars → mux at stop
 └─ config via CaptureSettings (engine.json) + OverlayConfig (config.json); Paths.FFmpegPath REQUIRED valid (RECORD_START rejected otherwise)
```

**Payload-vs-runtime verification:**
- `dist\NVIDIA ShadowPlay\` (130 files) matches the APP-LAYOUT design: `Application\` 3 hosts, `Services\` 3 dll+runtimeconfig, `Engine\` 8 CaptureEngine dlls, `FFmpeg\` (ffmpeg/ffprobe/ffplay + 7 av/sw dlls), `Audio\`(7 NAudio), `Graphics\`(4 Vortice), `Core\`, `Libraries\`, `.NET Deployment\` (10 json), `Languages\` 27, `Redist\64bit.runtime.exe`, `Resources\nvgcshare.ttf`, `Data\NVIDIA_Shadowplay_Data\on`, `Runtimes\` (empty).
- **Runtime deps that exist in payload:** OK for hub/apps/engine chain. `nvEncodeAPI64.dll` is NOT in payload (by design — it comes from the NVIDIA driver, loader search). 
- **Runtime deps NOT guaranteed:** FFmpeg payload exists only on this machine (gitignored `Overlay\API-Core\` = 239 MB; see §13/§14 — a fresh clone cannot produce a working product; layout Manifest only *warns* on missing `FFmpeg\ffmpeg.exe`).
- **Stale-junk mismatch:** dev bin `Application\` still contains `NVIDIA API/Capture/Notifier.dll` (should be hosts-only); only `-StageLayout /MIR` purges it — contradicts "plain builds self-maintain the tree" (`build-all.ps1:219`).
- **Missing-but-referenced seed:** `NVIDIA_Shadowplay_Data\highlights\Effect.mp3` None item in `NVIDIA Overlay.vbproj:372-374` does not exist on disk; the `Data\NVIDIA_Shadowplay_Data\on` seed that layout Manifest **requires** is **excluded by the installer's `Excludes:`** (`NVIDIA ShadowPlay.iss:79`) → installed-first-run diverges from layout contract.

---

## 4. DEAD CODE FORENSICS

Reflection/dynamic audit first (this is what gates confidence): repo-wide grep found **zero** `Activator.CreateInstance`/`Type.GetType`/`Assembly.Load*`/`CallByName`/CodeDom in production code except: `WasapiDirectInterop.cs:143-144` (COM `GetTypeFromCLSID` — intentional) and 17× `Marshal.GetDelegateForFunctionPointer` in `NvEncFunctionTable.vb` (intentional NVENC binding). One **unused** `Imports System.Reflection.Emit` in `[1] Main Menu.vb:8`. → **unreferenced-symbol analysis is high-confidence** (no plugin/reflection activation surface exists).

### 4.1 Clusters (classification per request rules)

| Cluster | Contents | ~Lines | Class | Notes |
|---|---|---|---|---|
| A | **CaptureSession.vb `If False` legacy-audio blocks** (`:293-470` system sidecar + whole Device-clock branch, `:480` legacy mic, `:914` legacy finalize) — plus `BeginTimelinesOnce`, `SilenceKeepAlive`, `AudioTap`, `AudioTapDeviceClock`, `WavSidecarWriter`, `AudioWavSink` which are constructed **only** from those blocks or tests | ~150-250 in-file + ~1,900 collateral | **CONFIRMED UNUSED (production)** | consequence: `AudioClockMode` config accepted→logged→**ignored** (§11) |
| B | **CaptureEngine.FFmpegBackend legacy stack**: `FFmpegPipelineBackend` (495), `MuxCoordinator` (347), `AudioSidecar` (129, self-declared stub), `FFmpegProcessHost`+`StderrParser`+`SyncMath`+`AudioTimelineRepair` (~925) | ~1,900 | **LIKELY UNUSED** (constructed only by FFmpegTests + legacy paths; `LiveMuxSession` IS live) | needs type-level decision before removal |
| C | **Old frame contract** `CaptureEngine.Video\Frames\*` (2nd `IVideoFrame` in same assembly, different namespace) | 295 | **CONFIRMED UNUSED** (only FrameContractTests consumes) | two `IVideoFrame` types ship in one dll |
| D | **Fake backends in production dlls** (`FakeVideoCaptureBackend` ~518, `FakeEncoderBackend` ~487 + helpers; no `#If DEBUG` anywhere — 0 hits) | ~1,215 | **CONFIRMED reachable-only-from-tests**, but shipped in `CaptureEngine.Video.dll`/`CaptureEngine.Encoder.dll` | RecordingEngine hardcodes real backends; factories cannot select fakes (evidence: grep `New Fake…` = tests only) |
| E | **Test-only config/V2 stack inside production `CaptureEngine.dll`**: `ConfigLoader` (154), `ConfigValidator` (339), `PipelineResolver` (129), `PipelineConfig` (48), `EngineConfig` (41), `EngineConfigV2` (291), `FFmpegCommandBuilderV1` (255), `FFmpegCommandBuilderV2` (290) — minus `ConfigMigrator.MapNvencPresetInteger` which IS live via `NextRecordingConfig:157` | ~1,500 | **CONFIRMED UNUSED (runtime)** | but V1/V2 contain a latent QSV regression (§7, R14) |
| F | Dead members: `DdagrabBackend._context` (decl 94, assigned 215, never read), `_timestampFallbackCount` (written, never read), `NvEncFunctionTable` capability probes (GetEncodeGUIDCount/GetEncodeGUIDs/GetInputFormatCount/GetInputFormats — bound, never invoked), `NV_ENC_ERR_GENERIC` (obsolete, unused), `EncoderShutdownException` (documented, never thrown), `VideoBackendKind.GfxCapture` (no implementation; factory throws for it), `RecordingDTOs.UseSharedHandle` + `DdagrabBackend.UseSharedHandle` setter (**zero production callers** — only the spike sets it), `EncoderConfig.MaxInFlightFrames/.Cq/.MinrateBps/.FFmpegCodec` on the NVENC path, `FakeEncoderBackend._inFlight` (drained, never enqueued → Flush always 0 packets), legacy `CaptureEngine._audioWriter` (declared :71, read 486-497, **never assigned**), `LiveMuxSession.ConnectTimeoutMs` (declared :62, literal 30000 used at :567), `RecordingEngineState.Stopping` (never assigned), `AudioTrackDiagnostics.DroppedBytes` (never written — §8) | ~200 | **CONFIRMED UNUSED** | `UseSharedHandle` dead ⇒ production NVENC runs the "direct path" the code itself calls non-contractual (§8) |
| G | Dead UI: `Base_Empty` form (`[0.1] Empty.vb` — zero references), OBS `Engine_Mode3` radio (click only resets colors; forced COLOR_INACTIVE; never sets engine_mode), engine_replay_start/stop/save TCP handlers → `"not_implemented"`, Launcher `[APP] Client` dispatch (only `Case Else → Debug.WriteLine`), Highlights stub (`ShowNotifier("feature_not_ready")`), Debug_UI (debug-only by design) | ~500-700 | CONFIRMED/LIKELY UNUSED | |
| H | Fossil sentinels: `notifiermainoff` (**no writer anywhere**; reader `Loader.vb:988-1010` is slot-1-only), `notifier_main` (9 readers, writer commented out at `Sub_Misc.vb:55-66` → StackBaseY pinned 105, Y=205 branches dead), `notifier` state file (written `Loader.vb:993-997`, no reader), `Data\...\on` (no reader/writer; only staged), `not_save` timer (30 s, no Tick handler), `Overlay\Program.vb` manual `Global\NVIDIA_ShadowPlay_Overlay_SingleInstance` mutex (dead in production: startup object is `My.MyApplication`; live only in ManualOverlayTest) | — | **CONFIRMED UNUSED** | behavioral fossils documented in docs/NOTIFIER_SLOT_AUDIT.md (itself stale: describes FIFO queue that no longer exists) |
| I | Build/binary junk: stale net8.0 obj trees (API/Engine/Notifier/CaptureEngine), `Overlay\packages\`, dev-bin `Config\config.json.19108.tmp` + `.bak`, 7 `*.user` files, `.vs\`, missing-file vbproj None items (`Effect.mp3`, `_dxwebsetup.exe`, `_overlay.ttf`, `_runtime_8.0.8.exe`, `_readme.md`, `_icon.ttf`, `betanv.ttf`, `Languages\nvgcshare.ttf`), `wmplib` COMReference removed (comment Overlay vbproj:127) | — | CONFIRMED stale | |
| J | Docs: `NOTIFIER_SLOT_AUDIT.md` (stale FIFO model), `phase-12-architecture-spec{-v2,-v3}.md` (3 versions kept), `P13.5-REMOVAL-INVENTORY.md` (describes removal of knobs the code still carries) | — | LIKELY STALE | |
| K | Repo-root artifacts (transient during audit): ffmpeg-args-as-directories set (`-i`, `-vf`, `-y`, `-p`, `-g`, `-t`, `-rc`, `-preset`, `-fps_mode`, `-loglevel`, `-hide_banner`, `cbr`, `cfr`, `30`, `5`, `7000000`, `medium`, `h264_qsv`, `info`, `lavfi`, `hwdownload,format=bgra,format=nv12`), `$null`, `_tmp_build.ps1`, `scotcsduluka the best dev.zip` — **all absent at final verification** | — | **UNKNOWN origin, resolved** | token trail matches `nvenc_test.bat` args + `FPS=30/Bitrate=7000000` defaults in `feedback-logs\...\capture-engine.log:221`; consistent with an ffmpeg-invocation accident that treated args as paths at repo root; since removed |

**Total confirmed/likely dead in production assemblies: ≈ 7,500–8,500 lines** (excluding spikes ≈ 36,205 lines and duplication debt §5).

---

## 5. DUPLICATE IMPLEMENTATIONS

| Duplication | Copies | Status classification |
|---|---|---|
| **TcpClientHelper** | 4 copies: Launcher + Overlay (byte-identical MD5 `8767504F`, hardened: generation counter, reconnect gate, backoff), Notifier (pre-hardening: sync ctor connect `:36`, no gate → duplicate reconnect loops `:131-136,159-210`), Engine (pre-hardening + extra `OnReconnected` event) | Overlay/Launcher = **ACTIVE**; Notifier + Engine copies = **LEGACY-DUPLICATE (active in those apps)** — dedupable to 1 shared file (992 lines total) |
| **Encoder name tables** | 4-5 definitions of `NVENC_H264→h264_nvenc`-style maps: `OverlayConfig.MapEncoderToFfmpeg/MapEncoderToInternal/MapQsvPreset/MapNvencPreset` (`OverlayConfig.vb:577-662`), `ConfigMigrator.MapEncoderKeyToFfmpeg/MapNvencPresetInteger` (`:214-234`, **verbatim duplicate**), `EncoderService.GetFFmpegCodecName` (`:124-137`, **exact-case keys** vs ALL-CAPS elsewhere → case divergence hazard), `EncoderDetector._fallbackEncoders/_keepEncoders` (`:42-69`) | OverlayConfig = **ACTIVE** (legacy engine); ConfigMigrator map = **ACTIVE via NextRecordingConfig:157** (new engine) — two live copies of the same table that can drift |
| **FFmpeg argument builders** | 3: legacy `Engine\Engine\[FFmpeg]\FFmpegArgumentBuilder.vb` (ACTIVE — legacy regime), `CaptureEngine\FFmpeg\FFmpegCommandBuilderV1.vb` ("byte-identical replica" claim is **false for QSV**: reintroduces `-minrate/-maxrate/-bufsize` triplet + `-preset medium -look_ahead 1`, missing `-g/-fps_mode`; preserves NVIDIA `-bufsize` 1× bug explicitly, `:159-160`), `V2.vb` (config-schema driven, NVIDIA bufsize 2×, `-pix_fmt` added) | legacy = **ACTIVE**; V1 = **REFERENCE/DUPLICATE (test-only)**; V2 = **DUPLICATE (test-only)** — both latent-risk |
| **Two `CaptureEngine.vb`** | `Engine\Engine\[Capture]\CaptureEngine.vb` (1,197 lines — legacy FFmpeg orchestrator, ACTIVE fallback) vs `CaptureEngine\Engine\CaptureEngine.vb` (319 lines — Phase-0 lifecycle state machine, used by CaptureEngine.Tests only) | first = **ACTIVE**; second = **REFERENCE** |
| **Audio writers** | `AudioFileWriter.vb` (830, legacy engine — ACTIVE), `AudioTimelineWavSink`/`AudioWavSink` (C#, legacy sink — dormant), `AudioEngineSession`+`AudioEngineMuxSink` (live, both regimes), `WavSidecarWriter` (dead-block only) | mixed ACTIVE/DORMANT |
| **Config layers** | 15 config classes, ≈4,691 lines; `FFmpegPath` has 4 spellings; bitrate in **kbps (Overlay) vs bps (Engine)** converted in 2 places; `engine_mode`/`EngineMode`/`APICapture`/`CaptureMethod` 4 names; MyPresets ×3 shapes; `AudioClockMode` 4 hops into a dead branch | AppSettings+AppConfigShared = **ACTIVE (canonical)**; CaptureSettings = **ACTIVE (engine.json compat)**; EngineConfigV2/ConfigLoader = **UNKNOWN→test-only** |
| **Toggle switch** | `NVIDIA Controls\ToggleSwitch` (designer shim, used on Privacy page) vs Launcher `NvCustomControls.NvToggleButton` (~2,600-line family) | two diverged control families |
| **FONTS.vb / FontHelper** | Launcher vs API copies, drifted (Launcher adds Sleep(600)+Restart) | DUPLICATE, both ACTIVE |
| **Layout producers** | `scripts\layout.proj` vs `Directory.Build.targets _ProductTreeBin` — convergent-by-design but each has what the other lacks (Languages/notifier_obs.json explicit in layout.proj vs swept/seeded in targets; Audio/Graphics/Core folders only via per-project `_StageRuntimeLibs` or targets classification) | DUPLICATE by design, divergence-prone (§13) |

---

## 6. ARCHITECTURE BOUNDARY AUDIT

Intended direction: UI → Application → Engine → Infrastructure. Findings:

1. **UI → Engine inversion (accepted but real):** Overlay (UI) ProjectReferences the Engine *exe* and Notifier/API/Launcher exes to get their payloads staged — a packaging-driven reference, not a code dependency; but it makes the UI assembly the build-anchor ("Overlay builds LAST; everything copies into its bin"), coupling product shape to one WinExe. Documented in `Directory.Build.targets:29-33`. Severity: LOW (deliberate, but the "UI depends on everything" shape is the biggest architectural smell).
2. **Engine → WinForms:** project rule says "Engine ห้าม depend WinForms" — `CaptureEngine.*` libs honor it (plain net10.0). BUT `CaptureEngine.Recording.ConsoleDriver` (a *driver*) sets `UseWindowsForms=true` only because **linked production file** `CaptureSettings.vb` probes `Screen.PrimaryScreen` (`ConsoleDriver.vbproj:40-42`) — i.e., the Engine-adjacent config layer reaches into WinForms. Also `Engine.Concurrency.Tests` (WinForms Exe) references the engine WinExe. Severity: MEDIUM (boundary leak via linked source, violates the project's own rule in spirit).
3. **Test → production source link-compile** (§2.5): tests compile production `.vb` files directly — direction is fine, but it creates parallel-compile divergence (a fix in one copy doesn't propagate). Severity: MEDIUM.
4. **Production assembly contains test fakes** (§4-D) — the fake backend lives in the shipped `CaptureEngine.Video.dll`. Severity: MEDIUM (hygiene; no runtime selection path).
5. **No circular architectural coupling found** (DAG verified at project level; UI forms use events/TCP only toward the engine; engine never references Overlay/WinForms types directly — grep clean except item 2).
6. **Legacy→New leakage:** none found (legacy `CaptureEngine` uses `AudioEngineSession`/`AudioTimelineWavSink` from shared audio projects — shared infra, not new-engine leakage). New→Legacy leakage: `RecordingEngineHost` lives inside `UI_Engine` (partial class in the same form) — **the new-engine host is coupled to the legacy diagnostic UI form**, single-file (`UI_Engine.vb`, 1,535 lines) hosting both regimes. Severity: MEDIUM (maintainability).

---

## 7. THREADING & ASYNC FORENSICS

Every finding below was located with actual interleaving analysis; severity = realistic impact.

### 7.1 CRITICAL-adjacent / HIGH

| ID | Finding | Interleaving |
|---|---|---|
| T1 | **Notifier `TcpClientHelper` pre-hardening copy: no generation gate → two reconnect loops can run concurrently**, racing `_writer/_reader` | T0 socket read fails in ListenLoop → spawns ReconnectLoop#1; T1 `Connect()` also fails in ctor-path/send-path → spawns ReconnectLoop#2; T2 both loops assign `_writer = New TcpClient` alternately → duplicated messages, lost replies, leaked sockets. Evidence: `Notifier\...\TcpClientHelper.vb:60-63,131-136,159-210` |
| T2 | **Blocking `Invoke` from socket/WebSocket threads (Notifier)**: `Loader.vb:132-136` (`Me.Invoke(Sub() OnMessage(msg))` from WS thread) + `[Notifier] Client.vb:95-98` (`Invoke` in OnMessage) | T0 WS receive thread calls Me.Invoke; T1 UI thread is busy/modal → receive thread stalls → TCP/WS backpressure → heartbeat timeouts; contrast: Overlay/Launcher were already fixed to BeginInvoke ("blocking Invoke from the socket read thread stalled the reader", `[Overlay] Client.vb:44-56`) |
| T3 | **Legacy `CaptureEngine._state` plain field written/read across ≥4 thread contexts** (UI, Task.Run, Exited threadpool thread, stderr callback threads), no Volatile/Interlocked (`CaptureEngine.vb:61,1146-1149`) | T0 UI thread checks `_state <> Idle` (start guard :186-189); T1 stderr/Exited thread concurrently in `SetState`; T2 second start passes the same check before T0's `SetState(Recording)` lands (deep inside Task.Run :342) → double-start window. Narrow but real. |
| T4 | **`StopAudioWriter()` re-entrance race** between `OnExited` (:1039) and `StopRecordingAsync` (:500) — `If _audioEngine IsNot Nothing` check-then-act with no interlock (`:1122-1142`) | T0 OnExited begins Stop+dispose; T1 StopRecordingAsync passes the same Nothing-check; T2 both call `_audioEngine.Stop()/Dispose()` concurrently — AudioEngineSession.Stop is `_sync`-guarded so mostly benign, but dispose/null-out interleave can NRE. (Note: the one-shot `_stopCompleted`/`_muxCompleted` interlocks cover the mux, not the audio stop.) |
| T5 | **Synchronous `Me.Invoke` from background threads in `ReinitializeRecordingEngineFromConfig`** (`RecordingEngineHost.vb:139`) | T0 config-change Task.Run calls Me.Invoke; T1 UI thread busy → deadlock-class risk (everything else in this app uses BeginInvoke deliberately) |

### 7.2 MEDIUM (latent, guarded by construction today)

| ID | Finding | Why latent |
|---|---|---|
| T6 | `DeferredVideoFrameDisposer.Enqueue` check-then-enqueue without lock (`:31-39`): T1 passes `_stopping` check → T2 `CompleteAndWait` sets stopping, worker exits, `DrainSynchronously`, Dispose → T1 enqueues frame → **GPU frame leak** (D3D11VideoFrame has no finalizer) + `_wake.Set()` on disposed event | All production Enqueue call sites are the same thread that later calls CompleteAndWait (`CaptureSession.vb:689,717,816,820,846,881`) |
| T7 | `AudioEngineSession.Dispatch` iterates `t.Sinks` without lock (`:309-315`) while `AddSink` (`:83-103`) is `_sync`-guarded | Production adds sinks before Start; a post-Start AddSink would throw inside the capture loop (contained, track dies) |
| T8 | `RecordingEngineHost` publishes `_recordingEngine/_engineReady/_useNewEngine` unsynchronized across threads (`:100-101,108`) | reference-atomic; UI-marshaled in practice; consequence realized only as the misleading fallback log (§9) |
| T9 | `RecordingEngine` encoder swap outside `_sync` (`:258`) | safe only because Dispose waits `_sessionFinished` (30 s) — and on Dispose-timeout the gate contradicts (§8) |
| T10 | `PipeFeed.Feed` drop-oldest vs two producers (capture thread + `AudioEngineSession.Stop→FinalizeTrack→DispatchSilence` stop thread) — residual tail re-enqueue interleave can mis-order tail chunks (`LiveMuxSession.vb:530-544`) | stop-window only; bounded by 8 MB audio cap |
| T11 | Video Feed "fail hard" comment vs actual silent drop after 10 s block timeout (`:511-523`) — comment/code mismatch; session does not fail | corruption avoided (whole-packet drop), loss counted only in DroppedBytes |
| T12 | `LiveMuxSession.Stop()` reads `_proc.ExitCode` immediately after Kill on WaitForExit-timeout (`:335-339`) → can throw "process has not exited", caught by outer Catch → `FFmpegExitCode=0` + misleading error string | timeout path only |
| T13 | Fire-and-forget `Async Sub` hotkey handlers (`ToggleRecording/ToggleInstantReplay/SaveInstantReplay`, `Sub_Record.vb:133,229,309`) — guarded by 200 ms cooldown + interlocked-style flags, but an exception **before** the Try (e.g. `GetOutputDirectory`) crashes the app (async-void; Overlay has **no UnhandledException hook**, §12) | rare-path |

**Timer inventory (UI):** all `System.Windows.Forms.Timer` (no Threading.Timer/PeriodicTimer in app layer). Outliers: `Engine_UI`/`Audio_UI` at **1 ms** (1000 Hz `Process.GetProcessesByName` polls; `Main Menu.Designer.vb:2256/2260` — verified 2256 on disk), `Load_App` 50 ms, `GAMES_IN` 1 s enumerating **all** processes (`Sub_Misc.vb:186`), Launcher `IF_APP` at designer default **100 ms** (no Interval set, enabled in Designer `Main.Designer.vb:349-351`), Notifier heartbeat 100 ms. No timer-written config remains (the b697a30 class of bug is fixed — `Lang_Tick` explicitly stopped saving, `IF_APP_Tick` read-only with comment, `UpdateMicStatus` display-only).

---

## 8. RESOURCE LIFETIME AUDIT

| ID | Finding | Evidence | Severity |
|---|---|---|---|
| L1 | **Session-owned `_audioEngine` leak on exception path — FIXED DURING AUDIT WINDOW.** Core-agent initially read Finally without `_audioEngine` dispose; on-disk file now contains `Try : _audioEngine?.Stop(...) : Catch : End Try` + `Try : _audioEngine?.Dispose() : Catch : End Try` with "★ Hardening" comment | `CaptureSession.vb:1151-1162` (verified by direct read) | RESOLVED (verify in tests) |
| L2 | **Main live-mux ffmpeg is NOT under the JobObjectGuard.** `SessionConfig.OnProcessStarted` is invoked **once — for the verify ffprobe** only (`CaptureSession.vb:1112`, verified on disk); the live-mux ffmpeg (`LiveMuxSession.vb:168`), remux (:393) and probe (:1039) get no hook — despite host header claiming "one job object owns every child ffmpeg this host spawns" (`RecordingEngineHost.vb:52-54`) | host crash (or unhandled app death) orphans the recording ffmpeg; during normal stop only `Kill()` on timeout protects | **HIGH** |
| L3 | **NVENC Initialize generic-Catch leaks session+device**: Catch at `NvencEncoderBackend.vb:358-360` frees `encodeConfigPtr` only; `_nvenc`, `_deviceResult`, DestroyEncoder untouched; state stays `Created` so retry leaks again | OOM in AllocHGlobal/serializer throw after session open | **MEDIUM-HIGH** |
| L4 | **NVENC Encode-vs-Dispose race**: encode hot path native calls outside `_sync` (`:537-716`); Dispose sets `_disposed` under lock then tears down (`:840-866`); no in-flight tracking; `NvencResources._disposed` non-atomic (`:131`) | prevented only by caller discipline (CaptureSession stops before dispose) | MEDIUM |
| L5 | **Post-Fault encode spam**: after NVENC faults, subsequent `Encode` throws `InvalidOperationException` per frame; CaptureSession counts `NvencErrors` per frame and keeps looping to session end (`CaptureSession.vb:741-751`) | a mid-session driver fault degrades to exception-spam for the remainder instead of abort | MEDIUM |
| L6 | **D3D11VideoFrame: raw NT handle, no finalizer**; exactly-once `Interlocked.CompareExchange` guard present (`D3D11VideoFrame.vb:189-210`, verified); CloseHandle return ignored; correctness depends on every path disposing (acknowledged in code: `CaptureSession.vb:699`) | a leaked frame leaks an NT handle until process exit | MEDIUM (mitigated by DeferredVideoFrameDisposer) |
| L7 | **Production NVENC uses the "direct path" (cross-device texture use) the code itself labels non-contractual** — because `UseSharedHandle` is never set outside the spike (§4-F). Works when both devices pick the same NVIDIA adapter (forced by &H10DE filters) but driver-dependent by its own admission | `NvencEncoderBackend.vb:522-526` comment; `DdagrabBackend.vb:176-180` | HIGH (stability risk on driver updates) |
| L8 | **Ddagrab eternal retry**: `DuplicateOutput` failure (e.g. duplication-slot exhaustion / OOM) is logged and retried forever at 250 ms with no escalation/fault after N failures (`DdagrabBackend.vb:901-917`) | silent eternal retry loops | LOW-MEDIUM |
| L9 | **Ddagrab Dispose worker-join timeout path**: if Stop's 2 s join fails, `CleanupPersistentResources` disposes device/context/duplication while the worker may be mid-`CopyResource`/`ReleaseFrame` (swallowed COM exceptions = use-after-release) (`:519, 842-843, 861-867`) | rare (worker is cooperative) | MEDIUM |
| L10 | `LiveMuxSession.Dispose` (called from Finally on the exception path) sets events + disposes pipes + `_proc` **without drain and without killing ffmpeg** — pipe-break EOF usually stops ffmpeg, not guaranteed (`:407-414`) | combined with L2 | MEDIUM |
| L11 | `WasapiPositionCapture.Dispose` does not `Marshal.ReleaseComObject` — COM release deferred to GC; `Stop` joins 2 s then nulls fields while loop may live (RCW race window) (`WasapiPositionCapture.cs:329-344`) | acceptable session-lifetime; nondeterministic on engine reinit/rebuild path | LOW-MEDIUM |
| L12 | `RecordingEngine.Dispose` on 30 s timeout returns "backends left alive for a safe retry" — but `_disposeRequested` stays set, so any later StartSession throws ObjectDisposedException: comment contradicts gate (`:347-350` vs `:198`) | engine permanently unusable after a wedged session, requires app restart | MEDIUM |
| L13 | `AudioEngineMuxSink` silent uncounted drops: pending-cap discard (`:61-64`) and stale-packet skip (`:86`) increment no counter | audit hole feeding the Pass contract (below) | HIGH (accounting) |
| L14 | **`AudioTrackDiagnostics.DroppedBytes` is never written anywhere** (`AudioEngineTypes.cs:65` declared; `RebuildDiagnostics` `:343-373` never sets) → `result.AudioDroppedBytes` always 0 → the `Pass` requirement "sidecar dropped = 0" (`RecordingDTOs.vb:310-313`) is **vacuously true**; the mux counter (`MuxDroppedBytes`) is the only live leg | the 4-year "pass=True หลอก" class of bug partially persists in a new form | **HIGH** |

---

## 9. PROCESS LIFECYCLE FORENSICS

Chain: Launcher → API (hub) → {Notifier, ShadowPlay, Capture} → ffmpeg.

| ID | Finding | Evidence | Severity |
|---|---|---|---|
| P1 | **Dual supervisor for NVIDIA Capture.exe**: hub `HandleAppsSmart` every 1 s (`NVIDIA API.vb:146-155`, verified on disk) + Overlay `EngineProcessSupervisor.MonitorLoop` every 2 s+backoff (`EngineProcessSupervisor.vb:230-311`). Both key on `GetProcessesByName("NVIDIA Capture") = 0` → spawn. Engine single-instance is the **VB assembly-identity mutex** (`Engine\...\Application.Designer.vb:27`) | dev scenario: `dotnet NVIDIA Capture.dll` holds the mutex under process name "dotnet" → every spawned apphost exits ~2 s (exit code 0, not recorded anywhere) → **both loops respawn forever** (hub unconditionally; supervisor with backoff). No exit-code check or "died immediately" detection anywhere | **CRITICAL-adjacent (dev-only), HIGH** |
| P2 | Hub is a pure broadcast relay: command `Select Case` at `Server.vb:339-351` is empty no-ops; every line is rebroadcast to all clients except sender; 60 s silence reaper; 32-client cap; `pong` under `clientsLock` | not a bug, but any client can spoof any appName/command (§16) | INFO/SECURITY |
| P3 | Overlay's `EngineProcessSupervisor.Shutdown()` deliberately does NOT kill the engine on Overlay close (`[Overlay] Client.vb:30-42`) — engine outlives Overlay; hub keeps it alive while `UseOverlayEnabled=true` | by design; but combined with Launcher exit-button kill sequence (config write false → kill 4 process names, no WaitForExit) there is a window where the hub (killed last) may respawn a killed app | LOW |
| P4 | **Launcher exit-button kill is name-based and unordered** (`Main.vb:168-196`): kills Notifier/ShadowPlay/API/Capture with no WaitForExit; hub's 1 s tick may run once more in between and respawn one of the three before dying | stale processes appear "randomly" after exit | LOW-MEDIUM |
| P5 | FontHelper (Launcher copy) does Kill → `Thread.Sleep(600)` → `Application.Restart()` with hub also killing the same processes on its own tick — the known "font install bootloop" got one-shot guard `_fontInstallFailedOnce` (`NVIDIA API.vb:98-115`) | residual: API copy `FONTS.vb:69-79` kills without WaitForExit, empty Catch | LOW |
| P6 | Legacy engine OnExited with unreadable exit code (`proc Is Nothing → exitCode "?"`) fires **no branch** → state stays `Recording` forever (`CaptureEngine.vb:1010-1019,1041,1058`) | only if Process object already disposed/overwritten | LOW |
| P7 | Legacy stop: `WaitForExit(10000)` (timeout overload) without final parameterless `WaitForExit()` → async stderr handlers may not have drained the last lines when recording stops (`:452`) | last stats line can be missed; anchor chain slightly affected | LOW |
| P8 | `q`-quit via `StandardInput.Write("q\n")` — `StandardInput` never closed/disposed (`:441-442`) | minor handle lifetime | LOW |
| P9 | **Kill→WaitForExit ordering is consistently correct** in legacy engine (start-failure `:364-365`, stop-timeout `:462-463`, ForceStop `:810-811`, mux-timeout `:659-660`, ffprobe `:773-774`) and mux is job-object-tolerated with explicit reaping (`:640-647`) | positive finding | INFO |
| P10 | `HandleAppsSmart` disposes all returned Process objects (M4 fix, `:142-172`, verified) — handle-leak fix in place | positive | INFO |
| P11 | ffmpeg spawn `CreateNoWindow=true, RedirectStd*` everywhere; ffmpeg removed from hub supervised list with documented rationale (killed unrelated ffmpeg.exe machine-wide — fixed) (`:118-123`) | positive | INFO |

---

## 10. RECORDING STATE MACHINE

### 10.1 New engine (Duluka) — `RecordingEngine` + `CaptureSession`

- Real enum: `RecordingEngineState {Created, Initializing, Idle, Recording, Stopping, Faulted, Disposed}` (`RecordingDTOs.vb:15-23`) — owned by RecordingEngine. **`Stopping` is never assigned** (dead slot).
- `CaptureSession` itself is **not a state machine**: one blocking `Run()` with `_stopSignal`/`_stopRequestedTicks`/`_disposed` (idempotent `Stop()` via `Interlocked.CompareExchange` + `Volatile.Write`, `:1177-1180`).
- Guards: `StartSession` throws unless state=Idle (`RecordingEngine.vb:197-205`); re-checks `_disposeRequested/_disposed` after arming and self-stops the session (`:273-280`); `Stop()` no-ops when not Recording (`:310-315`).

Mental transition tests (actual code behavior):

| Scenario | Behavior | Verdict |
|---|---|---|
| Start + Start | host guard is check-then-act (`RecordingEngineHost.vb:237-240`, not atomic) but engine-level state guard throws `InvalidOperationException` → second Task faults | VALID (engine guard authoritative) |
| Stop + Stop | idempotent (`Interlocked` CAS) | VALID |
| Start during Stop | session runs an empty loop → near-zero failing result (deliberate) | VALID (wasteful but safe) |
| Stop + Start | state resets to Idle in session Finally (`:295-299`), next start OK | VALID |
| Start + Dispose | `_sessionFinished` initially signaled; Start after dispose throws ODE (`:198`) | VALID |
| Stop + Dispose | Dispose calls `session?.Stop()`, waits 30 s; **on timeout returns "safe retry" comment while `_disposeRequested` stays set → engine bricked** (L12) | **INVALID (contradiction)** |
| Restart during init | host `_engineReconfiguring` gates starts; rebuild queues `_rebuildPending` during recording | VALID (booleans unsynchronized, UI-marshaled) |
| Process exit during shutdown | crash-safe by design: frag-MP4 `+frag_keyframe+empty_moov` (`LiveMuxSession.vb:230`); remux fail → salvage rename of frag file (`:355-371`) | VALID |
| Failure mid-session (L2) | capture/encoder unwound in Finally (`:1141-1150`); `_audioEngine` now stopped/disposed on every path (`:1159-1160`) | VALID (as of audit-window fix) |

### 10.2 Legacy engine — `CaptureState {Idle, Detecting, Recording, Paused, Stopping, Muxing, HasError}`

- **`Paused` and `Detecting` are never entered** (no code path sets them) — dead states.
- Start guard `_state <> Idle` (:186-189) + UI-side `IsRecordingLifecycleActive` (Recording/Stopping/Muxing) guard (H1 fix) — double-start has a narrow TOCTOU window (T3).
- Stop sequence: guard (:391) → Stopping → `q` → WaitForExit(10 s) → Kill+2 s → `StopAudioWriter()` **unconditional** before mux (23ce979 fix confirmed at :500) → `AwaitOrRunMux` (Interlocked one-shot `_muxCompleted`) → honesty check `RecordingStopped` only if output file exists (:515-521).
- `HasAudioData` RIFF parsing confirmed (`AudioFileWriter.vb:782-809`) but **catch-all → False** (`:806-808`): a transient IO error silently reports "no audio" → video-only rename (data loss without error).
- Mux failure fallback confirmed: video-only rename, sidecars preserved (`:685-702`); if the fallback `File.Move` itself throws, outer Catch only logs and the missing-file honesty check then raises ErrorOccurred (stop reports failure — correct but noisy).

---

## 11. CONFIGURATION AUTHORITY FORENSICS

### 11.1 Authority matrix (key settings)

| Setting | Source(s) | Canonical authority | Readers | Writers |
|---|---|---|---|---|
| `Overlay.UseOverlayEnabled` | `Config\config.json` | **config.json** (single source; foreign-key guard `AppSettings.vb:1046-1052`; hub enforces 1 s) | API hub (every tick), Launcher tick, Overlay | **Launcher toggle only** (user action; `_toggleInitializing` gate) + kill-switch=false |
| All user settings (Recording, Audio, Notifications×27, Hotkeys, Language, …) | `Config\config.json` | **AppSettings (Overlay)** owner; atomic per-PID temp + .bak (`AppConfigShared.vb:153-164`) | Notifier (gates, slot count, language), hub | Overlay UI (≈15 call sites, incl. one **redundant double Save** in `SelectLang` `[0] Settings - UI Main.vb:124,132`) |
| engine.json | `Config\engine.json` | **compat tier** — read by CaptureSettings (config.json wins, early return `CaptureSettings.vb:99-146`); written by 2 declared compat writers: `SyncWithOverlayConfig` legacy branch (`UI_Engine.vb:545` — comment says "shadowplay-config.json", code writes engine.json) + `HandleEnginePrewarmFFmpeg` (:641); bare Catch swallows failures (`CaptureSettings.vb:194-195`) | Engine, NextRecordingConfig | Engine (2 sites) |
| `audio.json` | legacy tier | read fallback; written only by `AudioSettingsForm.SaveAudio` (`:127-128`) | CaptureSettings | AudioSettingsForm |
| `video.json` | migrated away | read once → renamed `.legacy` (`AppSettings.vb:482-489,570`) | — | — |
| `notifier_obs.json` | `Config\` | **ObsConfig** (read-modify-write so `forward` survives; mtime hot-reload 2 s watcher) | Notifier | Overlay `[7] Notifications.vb:437-472` (**password plaintext**, §16) |
| `engine_mode` | config.json nested `Recording.engine_mode` → flat `EngineMode` → `APICapture` inference | **THREE normalizers**: `AppSettings.NormalizeEngineMode` (:868), `OverlayConfig.GetEngineMode` (:438-464, catch→"ffmpeg"), `[5] Video Capture.vb:2403` backward-compat | Engine dispatch, RecordingEngineHost | Overlay Video Capture page (radio) |
| `Paths.FFmpegPath` | config.json + engine.json + auto-detect (`FFmpeg\ffmpeg.exe`/`API-Core\`) | **config.json**, auto-detect fallback; empty/missing → RECORD_START rejected "ffmpeg_not_found" (`UI_Engine.vb:413-417`) | Engine, RecordingEngineHost | Overlay settings; **observed absolute per-machine values in the wild** (`feedback-logs\...\ui-engine.log:193`: `C:\Duluka Corporation\NVIDIA Shadowplay\FFmpeg\ffmpeg.exe`) |
| `AudioClockMode` | config.json → mapped through 4 layers (`NextRecordingConfig.vb:102`) | **NO authority — accepted, logged, ignored** (sole consumer inside `If False`, `CaptureSession.vb:302-308`); de facto device-clock behavior regardless | — (mapped only) | Overlay Audio page |
| version | `Overlay\build.txt` (auto-increment per build) + vbproj `3.41.$(BuildNumber).61` | **Overlay exe FileVersion** (installer reads it, `NVIDIA ShadowPlay.iss:21`); **build.txt has no runtime reader**; other assemblies are 1.0.0.0 | installer, UpdateHelper (assembly version) | MSBuild IncrementBuild ×2 (Overlay + ManualOverlayTest — **shared counter, different schemes** `3.4.x.42`) |
| env/marker | `NVIDIA_SHADOWPLAY_APP_ROOT` (AppLayout.vb:77-95), `overlay-config-path.txt`, `Flags\*`, `Data\NVIDIA_Shadowplay_Data\*` (incl. fossils §4-H), HKCU Run key (SetStartup, `NVIDIA API.vb:81-94`) | documented | various | various |

### 11.2 Verdict

- The b697a30 class of bug (timer overwriting config) is **fixed and guarded** (foreign-key guard verified, `Lang_Tick` stop verified).
- Remaining authority violations: **(a)** `AudioClockMode` is a phantom setting (user-visible, no effect); **(b)** `engine_mode` has 3 normalizers + a silent catch→"ffmpeg" fallback that flips regime on config errors (`OverlayConfig.vb:460-463`); **(c)** engine.json remains a second writable config (2 writers, mislabeled comment); **(d)** version authority split across build.txt counter shared by 2 projects and 1.0.0.0 assembly defaults everywhere else.

---

## 12. ERROR HANDLING AUDIT

| Pattern | Volume/representative evidence | Production impact |
|---|---|---|
| **Global exception handlers exist ONLY in the hub** (`API\ApplicationEvents.vb:20-44` → api-crash.log; its own catch swallows) | Overlay/Notifier/Launcher have none (Launcher's is an empty partial `:26-28`) | **HIGH** — an unhandled exception (e.g. async-void hotkey path) kills the Overlay silently; hub will respawn it (masking the crash) |
| Empty/bare catches | systemic: Launcher hub-spawn `Main.vb:143-144` (empty — hub start failure invisible); `AppConfigShared` all read/write swallow (deliberate, documented); `TcpClientHelper` all copies → Debug.WriteLine only; legacy engine record path ~12 sites (`CaptureEngine.vb:327,367,534,661,668,773,781,818,918,986,1017,1139,1167`); `HasAudioData` catch-all→False (`:806-808`) → **silent audio loss**; `LiveMuxSession.PipeFeed` writer loop bare Catch (`:609`) — dead writer invisible except via dropped-bytes later; `SafeDelete` retry loop `Task.Delay(50).Wait()` ×5 **on UI thread** then silent give-up (`Loader.vb:958-969`) | the dangerous ones: HasAudioData, hub-spawn empty catch, writer-loop swallow |
| Silent fallbacks | `GetEngineMode` catch→"ffmpeg" (`OverlayConfig.vb:463`); `MapEncoderToFfmpeg` unknown→"h264_nvenc" (`:589`); `MapEncoderToInternal` unknown→"NVENC_H264" (`:615`); `ResolveFFmpegPath` last resort returns bare `"ffmpeg"` PATH string with only a log (`RecordingEngineHost.vb:219-220`); preset GUID unknown→p4; rate-control unknown→CBR | misconfiguration silently re-targets the encoder/regime |
| Failure reported as success | **Pass-contract vacuity (L14)**: sidecar-drop leg always 0 → `pass=True` can be true while audio was silently dropped by `AudioEngineMuxSink` (uncounted, L13). The mux leg (`MuxDroppedBytes`) is honest. Also `SessionResult` honesty improvements (9/05) verified in place | **HIGH** for diagnostics trust |
| Positive | honesty check on missing output file (§10.2); engine_response/errors propagated to overlay (`[Overlay] Client.vb:279-370`); `RecordingStopped` only when file exists; BackgroundLogger bounded queue with drop policy | — |

---

## 13. BUILD-SYSTEM FORENSICS

| ID | Finding | Evidence | Severity |
|---|---|---|---|
| B1 | **Version chaos (CRITICAL):** dist exe = **3.41.3595.61**, installer = **3.41.3591.61**, dev bin = **3.41.3593.61**, build.txt = 3593; canonical **3590 nowhere on disk**; sequence 3591→3595→(reset)→3593 implies hand-restored counter (documented practice: PROJECT_MEMORY.txt:2036). All non-Overlay assemblies are **1.0.0.0** (Launcher.exe, Services\*.dll, Engine dlls, verified via FileVersionInfo) | direct file reads during audit | **CRITICAL** |
| B2 | **build.txt double-increment**: `ManualOverlayTest.vbproj:55-70` defines its own `IncrementBuild` against the **same** `Overlay\build.txt` with a different version scheme `3.4.$(BuildNumber).42` — building the manual test mutates the production counter | vbproj | **MEDIUM-HIGH** |
| B3 | **FFmpeg payload machine-only**: `Overlay\API-Core\` = 239 MB (ffmpeg/ffprobe/ffplay + avcodec-62 97 MB etc.), gitignored (`Overlay/API-Core/`); `_Redist\` 60 MB gitignored. **A fresh clone cannot produce a working product or installer** — layout Manifest only *warns* on missing FFmpeg (`layout.proj:254-255`), the .iss has no alternate source | dir listings + .gitignore | **CRITICAL** (reproducibility/licensing: the repo does not contain its own shipping binaries) |
| B4 | **No product CI**: `.github\workflows\deploy-web.yml` only syncs `Web\`→gh-pages. `build-all.sh` claims "Linux CI" (line 2) but **no workflow invokes it**; none of the 7 test suites or validation drivers runs automatically anywhere | workflows dir | **HIGH** |
| B5 | Installer sources from **dev bin**, not dist (`NVIDIA ShadowPlay.iss:20`), while `-StageLayout` produces a curated dist — two different release payloads; dev bin currently carries stale `Application\*.dll` junk only `/MIR` would purge | .iss:20-23; disk | **HIGH** (§14) |
| B6 | Scripts: `build-all.ps1` healthy (distinct exit codes 1-4, kills app family before clean — documented DLL-lock reason); **BOM inconsistency**: `sync-verify.ps1` fixed (BOM verified) but `validate-phase12b.ps1`, `windows-phase1-video-validation.ps1`, `diag-recording.ps1`, `diag-startup.ps1`, `stress-startup.ps1` still BOM-less with non-ASCII content (PS 5.1 mojibake class) | file bytes | MEDIUM |
| B7 | `scripts\nvenc_test.bat` **broken as shipped**: line 5 hardcodes `...\net8.0-windows10.0.26100.0\api-core\ffmpeg.exe` — wrong TFM (net8.0), wrong case (`api-core`), machine-specific absolute path | bat | MEDIUM |
| B8 | Four separate FFmpeg-discovery strategies (`nvenc_test.bat`, `sync-verify.ps1 Find-Ffmpeg`, `validate-phase12b -Ffmpeg`, `windows-phase1-video-validation` candidate list) — none shared | scripts | LOW |
| B9 | `Directory.Build.targets` `_ProductTreeBin` vs `layout.proj` divergence (§5 last row) + `_DevLayoutComplete` only seeds Config/Data for the Overlay app — a lone Experience/other-app build intentionally keeps flat bin (documented MSB3030 constraint) | targets:127-144, 409-414 | LOW-MEDIUM (two layout producers) |
| B10 | `.gitignore` contradictions: `/evidence` ignored but contains **buildable source** (`loopback-probe\` csproj+Program.cs — disk-only knowledge); `*.zip` ignored and no zip exists; `$null` at root not ignored (would be swept by `git add .` — file since vanished); `Overlay/build.txt` deliberately tracked as "canonical build counter" | .gitignore | MEDIUM |
| B11 | Positive: `Directory.Build.props` minimal (EnableWindowsTargeting for Linux compile verification); vbproj IncrementBuild self-heals missing build.txt (=100); PostBuild deletes pdb/xml in Overlay | props/targets | INFO |

---

## 14. RELEASE / INSTALLER COUPLING

- **Version coupling:** `.iss:21` reads AppVersion from the built `Overlay\NVIDIA ShadowPlay.exe` (the only versioned component) — the installer is *correct by construction* but inherits B1's chaos; output name `NVIDIA-ShadowPlay-Setup-v{#AppVersion}.exe` → current artifact `v3.41.3591.61` **predates both dist (3595) and dev bin (3593)** — the shipped installer is 4 build-numbers stale.
- **Payload coupling:** installer packs the dev-bin tree with `Excludes: "Config\*,Logs\*,Flags\*,Data\NVIDIA_Shadowplay_Data\*"` + re-adds `Config\notifier_obs.json` (`.iss:77-81`). Consequences: (a) the `Data\...\on` seed required by layout Manifest is **not installed**; (b) `Config\*` exclusion is what *saves* end users from the dev machine's absolute `Paths.FFmpegPath` (the .iss itself comments "engine.json carries an absolute dev FFmpegPath") — the runtime then relies on auto-detect of `FFmpeg\ffmpeg.exe` (present in payload).
- **Uninstall:** deletes only `Logs` + `Flags`; Config/Data preserved (deliberate).
- **Prerequisites:** bundled `Redist\64bit.runtime.exe` (.NET 10 Desktop Runtime) silent install; admin required; per-user dirs granted modify.
- **Stale claims shipped:** `LICENSE.NOTICE` lists `libmp3lame.32/64.dll` — **neither exists anywhere on disk**; version string "3.3.2057.1-BETA" stale; Web site advertises three old version families (`Release-OBT-3`, `3.3.2057.1-BETA`, `3.35.2542.30-PRE`).
- No installer redesign performed (per instructions) — coupling reported only.

---

## 15. TEST COVERAGE GAP

Test projects are **custom-harness runners** (no xUnit/nunit), run manually via `build-all.ps1 -RunTests` (7 suites) or validation scripts. Baselines from project memory: ConfigTests 95/95 (incl. S1.26-S1.29 hwdownload regressions), ConfigTruth 30/30, Encoder.Tests 52/52 (V-CT5a-g), Recording.Tests 33/33+, FFmpegTests 57/57, FrameContract 8/8, Video.Tests 61/61 on Windows (43/61 on Linux — dxgi limitation).

| Component | Tests exist | Missing cases (gap) | Risk |
|---|---|---|---|
| BoundedVideoFrameSink | capacity/evict/replace/concurrent-dispose | — | LOW |
| Frame contract (new) | availability/ownership/monotonic/BGRA8 | — | LOW |
| DdagrabBackend | lifecycle ×18, deadlock regressions, replaceability | **E_OUTOFMEMORY / DuplicateOutput-failure escalation path (L8)** untested; vendor-filter (no-NVIDIA) path untested | MEDIUM |
| D3D11VideoFrame | **none directly** — dispose semantics tested only via mirror `FakeFrame` (`DisposerTests.vb:39`) | CloseHandle exactly-once on the real class; TOCTOU window | MEDIUM |
| NvencEncoderBackend | param builder struct sizes/GUID literals (V-CT5a-g); concurrency vs **fake** only | **`NvEncConfigSerializer.Serialize` bytes: ZERO tests** (CBR filler bit, union-discard L7 behavior); session-open/init-failure disposal (L3); Encode-vs-Dispose on real natives (L4) | **HIGH** |
| CaptureSession/RecordingEngine | RuntimeSyncTests (552 ln), DualTrackTests, DisposerTests, NEW Engine.Concurrency.Tests (M1/M2) | Start+Start concurrent (engine guard), Dispose-timeout-then-restart (L12), fault-then-continue encode spam (L5) | MEDIUM |
| LiveMuxSession/PipeFeed | FFmpegTests 57 | drain-budget worst case (2×budget per pipe), Feed-after-stop window (T11), ExitCode-after-Kill (T12) | MEDIUM |
| Legacy CaptureEngine | Engine.Concurrency.Tests (new, M1M2) | HasAudioData malformed-RIFF path; OnExited "?" strand (P6); QSV mux regression via legacy builder | MEDIUM |
| App layer (hub/supervisor/toasts) | Notifier.Obs.Resilience.Tests (new) | dual-supervisor respawn behavior (P1) — a script exists (`test-autospawn.ps1` v9) but not a suite; toast slot routing | MEDIUM |
| Config truth | ConfigTests 95 + ConfigTruth 30 (linked-source risk §2.5) | AudioClockMode phantom mapping (ignored setting) is "tested" as if live — tests encode the lie | MEDIUM |
| **Whole product** | — | **no CI runs any of the above** (B4) | **HIGH** |

No fake coverage found — the harness programs assert real behavior (orphan-ffmpeg checks, ffprobe asserts, crash-kill test in validate-phase12b).

---

## 16. SECURITY / ROBUSTNESS PASS (static only)

| # | Finding | Evidence | Severity |
|---|---|---|---|
| S1 | **Hub is an unauthenticated broadcast relay on `IPv6Any:5000`** (all interfaces, IPv4-mapped fallback) — any local process can connect, spoof any `appName|cmd`, and inject `RECORD_START`/toasts; heartbeats unauthenticated; commands are no-ops at hub but rebroadcast to all clients | `Server.vb:150-161,296-352` | **MEDIUM** (local-attack surface; recording start can be triggered by any local user process) |
| S2 | **OBS WebSocket password plaintext** in `Config\notifier_obs.json` (writer `ObsConfig.vb:23,80`) | local file, but synced/backed up freely | MEDIUM |
| S3 | `TCP messages split on "|"/":"` without length/auth — 64 KB line cap exists (`Server.vb:268-274`) | robustness OK-ish | LOW |
| S4 | **Path robustness**: output path comes from `RECORD_START <path>` payload (Overlay constructs it from user Gallery config) — no traversal concern locally, but Engine writes to arbitrary caller-provided path | `Sub_Record.vb:197` → `UI_Engine.vb:387` | LOW (local app) |
| S5 | **DLL search path**: `nvEncodeAPI64.dll` via plain DllImport = standard loader search (cwd is changed process-wide to product root by `AppLayout.Initialize` `:166` — **cwd is in the DLL search order**); payload folders are admin/owner-writable → a local attacker with write access to the product tree can plant a dll. Standard for per-app installs | `AppLayout.vb:166`, `NvEncodeAPI.vb:678-688` | LOW-MEDIUM |
| S6 | **Temp-file handling**: per-PID atomic config writes with `.bak` (good); leftover `config.json.19108.tmp` orphan observed in dev bin (crash between write and rename — self-limiting) | disk | LOW |
| S7 | **Untrusted config input**: JSON parsing everywhere is try/caught but `LoadConfig` returns Nothing on parse errors → callers fall back to defaults (silent). Settings NRE on corrupt notifier_obs.json was fixed (T13, 311de11) | `OverlayConfig.vb:292+` | LOW |
| S8 | **Privacy**: `feedback-logs\i kill people\` contains **end-user logs with absolute personal paths** (`C:\Users\xueglot\...`, `C:\Duluka Corporation\...`) — gitignored, but flagged for handling (PII in working tree); folder name is user-chosen, not scripted | dir contents | LOW/INFO |
| S9 | `Process.Start(downloadUrl)` shell-open of a Google Drive link (UpdateHelper `:30`) — no signature validation of updates | MEDIUM-LOW (update supply chain) |
| S10 | PAT rotation was already ordered in project memory (2026-09-02); no tokens found in tree by grep (Web/wrangler clean) | INFO |

---

## 17. TECHNICAL DEBT CLASSIFICATION (all material findings)

| Finding | Class |
|---|---|
| Version chaos B1 / build.txt double-writer B2 | **BUG** (release-process) |
| FFmpeg payload machine-only B3 | **RISK** (reproducibility) + **TECHNICAL DEBT** |
| No product CI B4 | **TECHNICAL DEBT** |
| Installer sources dev-bin + excludes required seed B5 | **RISK** |
| Dual supervisor respawn P1 | **BUG** (dev-machine behavior), **RISK** (production restart storms) |
| Fallback-log-false / engine_not_ready in ddagrab mode (§9 of legacy agent; verified dispatch) | **BUG** (diagnostics) + missing feature (real fallback) |
| engine_mode="ffmpeg" never reaches legacy engine when new engine healthy ([Engine] Client.vb:247 `_useNewEngine` wins) | **BUG/RISK** (regime selection contradiction — three selection sources disagree) |
| NvEncConfigSerializer union-discard (verified) | **RISK** (quality/config honesty) + **TECHNICAL DEBT** (no tests) |
| NVENC direct-path production (L7) | **RISK** (driver-dependent by code's own admission) |
| `UseSharedHandle` dead knob | **LEGACY** |
| AudioDroppedBytes vacuity L13/L14 | **BUG** (accounting honesty) |
| `_audioEngine` leak | **BUG — FIXED during audit window** (verify) |
| JobObject gap L2 | **RISK** (orphan ffmpeg on host crash) |
| NVENC Initialize leak L3 / Encode-Dispose race L4 / Fault-spam L5 | **BUG/RISK** |
| Legacy `_state` race T3 / StopAudioWriter reentrance T4 / Notifier Invoke T2 / Notifier TcpClientHelper T1 | **RISK** |
| 1 ms/100 ms UI timers | **CODE SMELL** (perf) |
| 4× TcpClientHelper, 4× encoder tables, V1/V2 builders, 15 config classes | **DUPLICATE** |
| Dead clusters A-K (≈8K lines) | **LEGACY / TECHNICAL DEBT** |
| Phantom config (`AudioClockMode`, `UseSharedHandle`, `Stopping` state, `Paused`/`Detecting` states, OBS radio, replay stubs) | **LEGACY** |
| Missing-file vbproj items, stale packages/obj, `.bak` residue patterns | **CODE SMELL** |
| `.gitignore` vs disk divergence (evidence source disk-only) | **RISK** (single-machine knowledge) |
| LICENSE.NOTICE false libmp3lame claims | **BUG** (legal accuracy) |
| BOM-less scripts with non-ASCII | **CODE SMELL** |
| Root artifact dirs (transient) | **UNKNOWN → resolved** |
| "falling back to legacy" being possible at all while `_useNewEngine` defaults True and dispatch ignores it | **FALSE POSITIVE** for the log line itself (it *does* have a fallback for ffmpeg-mode via `DispatchEngineCommand` :249-259) but **TRUE BUG** for ddagrab-mode (no fallback, error misattribution) |

---

## 18. SAFE FIX QUEUE

### SAFE TO FIX NOW (no runtime behavior change; evidence complete)
1. `NVIDIA Overlay.vbproj`/`ManualOverlayTest.vbproj`: remove the duplicate `IncrementBuild` from ManualOverlayTest or point it at its own counter file (stops production-counter corruption).
2. Delete dead members with zero-ref evidence: `DdagrabBackend._context`, `_timestampFallbackCount`, `FakeEncoderBackend._inFlight`, `LiveMuxSession.ConnectTimeoutMs` (or use it at :567), unused `Imports System.Reflection.Emit` (`[1] Main Menu.vb:8`), `not_save` timer.
3. Remove missing-file `None` items from `NVIDIA Overlay.vbproj` (Effect.mp3, _dxwebsetup.exe, _overlay.ttf, _runtime_8.0.8.exe, _readme.md, _icon.ttf, betanv.ttf, Languages\nvgcshare.ttf) — currently they only produce build noise/stale staging.
4. Add BOM to the 5 BOM-less scripts with non-ASCII content (same fix sync-verify got).
5. Fix `nvenc_test.bat` stale path (or delete the script).
6. Update `LICENSE.NOTICE` (remove libmp3lame entries, refresh version).
7. Delete `$null`-class root junk if it reappears; add `$null`/`*- Notebook` patterns to .gitignore.
8. Quote/comment fixes where comment contradicts code (V1 "byte-identical replica" claim; `LiveMuxSession` "fail hard" vs drop; RecordingEngine "safe retry" vs ObjectDisposedException gate; engine.json writer comment naming "shadowplay-config.json").

### FIX WITH TEST (small, behavior-affecting, need a harness run first)
9. L13/L14: make `AudioEngineMuxSink` pending-cap/stale-skip drops counted + write `AudioTrackDiagnostics.DroppedBytes` in `AudioEngineSession.RebuildDiagnostics` → Pass contract becomes honest. Add a drop-injection test.
10. T4: wrap `StopAudioWriter()` in an Interlocked one-shot (mirrors `_muxCompleted` pattern).
11. P1: hub + supervisor — skip respawn when the spawned process exits within ~5 s with exit code 0 (single-instance collapse), and add exit-code logging.
12. L12: on Dispose timeout, mark engine Faulted with a truthful message instead of "safe retry".
13. T2: Notifier `Me.Invoke` → `BeginInvoke` (3 sites), matching Overlay/Launcher.
14. Launcher hub-spawn empty catch (Main.vb:143-144) → log + user-visible fallback.

### NEEDS DESIGN DECISION (owner call — architecture)
15. Which regime owns RECORD_START: unify `_useNewEngine` + `GetEngineMode` + `NormalizeEngineMode` into ONE selector with documented fallback (the 3-source contradiction is the root of the misleading log + "ffmpeg mode unreachable" bug).
16. Implement-or-remove `AudioClockMode` (P13 device-clock is built but behind `If False`); same for `UseSharedHandle` (either set it in production per the encoder's own contract comment, or delete the knob).
17. Shared TcpClientHelper + shared encoder-name tables (single source of truth).
18. Consolidate fakes + V1/V2 builders + old frame contract out of production assemblies (`#If DEBUG` or move to test projects).
19. Version policy: single version authority (build.txt ownership, non-Overlay assembly versions).
20. FFmpeg payload strategy (vendored repo / LFS / download-on-build) — unblocks reproducible builds + CI.
21. Delete legacy clusters A/B/C/E (needs the type-level check in §4-B and owner sign-off per PROJECT_MEMORY rule 20).

### DO NOT TOUCH (protected / validated paths; per request + project memory)
- NVENC byte-level serializer layout & CBR filler bit ("validated ABI for this specific DLL" — `NvEncConfigSerializer.vb:24-30`).
- CFR timeline/monotonic nextTick + tail-fill (52c6d01 validated), `DeferredVideoFrameDisposer` design.
- `FFmpegPipelineBackend` first-frame anchor (production-validated; NVIDIA-machine only).
- LiveMux `RequestStopAndDrain` + Pass contract shape (2026-09-05 fix, runtime-validated 176 s).
- Legacy stop sequence ordering (23ce979) and HasAudioData RIFF parser (just fixed — fix the *catch-all*, not the parser).
- Capture 1/2/3, WASAPI interop (P13.1 rules in code comments), QPC math, installer design.

### FALSE POSITIVES (checked, NOT bugs)
- "No root .sln" — intentional; Overlay sln + per-app .slnx + scripts/build-all.ps1 build the solution directly.
- "Hub empty Select Case" — hub is deliberately a relay.
- Notifier toast slot units are not dead generations — they are the 3-slot system.
- `_ProductTreeBin` "deleting deps.json" — loader-fact-driven (runtimeconfig must stay, deps must move), documented with experiment evidence (`scripts/apphost_test`).
- Legacy `CaptureEngine.vb` is not dead — reachable fallback (see regime contradiction though).

---

## TOP 20 RISKS (Impact × Probability, descending; confidence per item)

| # | Risk | Impact | Prob. | Conf. |
|---|---|---|---|---|
| 1 | **FFmpeg/Redist payload (≈300 MB) exists only on this machine** (gitignored); fresh clone → product without FFmpeg; layout only warns; installer would ship broken | Release-blocking | High (any re-clone/CI/second machine) | HIGH |
| 2 | **Version chaos**: dist 3.41.3595.61 vs installer 3.41.3591.61 vs dev bin 3593; canonical 3590 absent; build.txt shared by 2 projects with different schemes; all other assemblies 1.0.0.0 | Release/traceability | High (already manifest on disk) | HIGH |
| 3 | **Dual supervisor + assembly-identity single-instance → infinite respawn of NVIDIA Capture.exe** (hub 1 s unconditional; no exit-code check) | Machine resource drain; dev paralysis; masked crashes | Medium (dev; rare in prod) | HIGH |
| 4 | **Regime-selection contradiction**: engine_mode="ddagrab" + new-engine init failure → every record fails `engine_not_ready` while log claims legacy fallback; engine_mode="ffmpeg" never reaches the legacy engine while new engine is healthy (3 conflicting selectors) | Recording impossible / wrong regime; misdiagnosis | Medium | HIGH |
| 5 | **No CI runs any test or validation** (only web sync workflow); all 9 suites + hardware matrices are manual | Regression risk on every change | High | HIGH |
| 6 | **Pass-contract vacuity**: `AudioTrackDiagnostics.DroppedBytes` never written + `AudioEngineMuxSink` uncounted drops → "dropped=0" leg of Pass is true by construction; silent audio loss class can recur undetected | Data loss w/ green status | Medium | HIGH |
| 7 | **Main recording ffmpeg not under JobObjectGuard** (hook wired to verify-probe only) — host crash orphans ffmpeg | Orphan process holding file/GPU | Medium | HIGH |
| 8 | **NVENC production uses direct cross-device texture path** its own code labels non-contractual (UseSharedHandle unreachable) | Driver-update fragility; corruption | Medium | HIGH |
| 9 | **NvEncConfigSerializer discards driver preset codec-union bytes** (hardcoded offsets; preset query outcome byte-irrelevant; entropyCodingMode=0); zero tests on serializer | Encode quality/compat drift | Medium | HIGH |
| 10 | **Overlay/Notifier/Launcher have no global exception handlers**; async-void hotkey path can kill the app silently; hub respawn masks it | Silent crash + phantom respawn | Medium | HIGH |
| 11 | **Notifier TCP stack is the pre-hardening variant** (sync ctor connect, no generation gate → duplicate reconnect loops) + **blocking Invoke from socket threads** | UI stalls, duplicated/lost toasts | Medium | HIGH |
| 12 | **TcpClientHelper.Send silent drop** for all non-recording commands (replay/save/open_overlay) — only record path guards hub-offline | Lost commands, silent feature failure | Medium | HIGH |
| 13 | **NVENC Initialize failure leaks session+device** (generic Catch frees pointer only) | GPU resource leak per failed init; retry compounds | Low-Medium | HIGH |
| 14 | **V1/V2 "replica" FFmpeg builders reintroduce the QSV rate-triplet** the legacy builder explicitly removed as producing broken files (plus `-look_ahead 1`, missing `-g/-fps_mode`) — dormant today (test-only) but one config-plumbing change away | Broken recordings if ever wired | Low | HIGH |
| 15 | **Installer coupling**: sources dev bin (not dist), excludes the `Data\...\on` seed required by layout Manifest, ships stale LICENSE.NOTICE claims; installed first-run state ≠ layout contract | Install divergence | Medium | HIGH |
| 16 | **Legacy `CaptureEngine` races**: `_state` cross-thread, `StopAudioWriter` reentrance (OnExited vs Stop), OnExited "?" strand leaving state=Recording | Stuck engine, double-stop | Low-Medium | MEDIUM |
| 17 | **AudioClockMode phantom config** — accepted, mapped, logged, ignored (consumer behind `If False`); tests/config docs encode the lie; P13.5 removal doc says it should be gone | Config dishonesty; user confusion | High | HIGH |
| 18 | **Dead-code mass ≈ 8K lines in production assemblies** (fakes without `#If DEBUG`, two IVideoFrame contracts, V2 config stack, dead legacy FFmpegBackend stack) — inflates payload, poisons analysis, risks accidental selection | Maintainability | High (certain) | HIGH |
| 19 | **Hub relay unauthenticated on all interfaces** + OBS password plaintext + cwd in DLL search order | Local attack surface | Low-Medium | HIGH |
| 20 | **1 ms UI timers (Engine_UI/Audio_UI at 1000 Hz) + 100 ms Launcher poll + GAMES_IN full-process enumeration** | CPU burn, GC pressure on low-end machines | High | HIGH |

*(Just outside the top 20: Ddagrab eternal-retry without escalation L8; Dispose-timeout engine bricking L12; D3D11VideoFrame TOCTOU L6; `.gitignore`/disk divergence B10.)*

---

# FINAL

**Overall Repository Health:**
🟡 **FUNCTIONAL BUT FRAGILE.** The recording pipeline itself (new NVENC path and legacy FFmpeg path) is in the best shape of its life — the recent M1/M2/23ce979-class hardening is real, verified in code, and the state machines are mostly guarded. The risk has moved *around* the pipeline: release/version management is in disarray (three different version numbers on disk simultaneously), the payload is unreproducible from the repo, no automated tests run anywhere, and a legacy/parallel-implementation accretion (~8K dead lines, 4× TCP stacks, 4× encoder tables, 15 config classes, two engines, two layout producers) makes every change higher-risk than it needs to be. The repo is a single machine away from being unbuildable.

**Highest Risk:**
Unreproducible product (FFmpeg/Redist payload gitignored, machine-only) combined with version chaos — a fresh clone or a new machine cannot produce the shipping artifact, and the current installer is 4 build-numbers stale vs dist.

**Most Valuable Fix:**
Unify the recording-regime selection into ONE authority (engine_mode) with an honest fallback — it removes the "engine_not_ready despite fallback log" failure mode (risk #4), makes the Intel-machine path deterministic, and is small enough to do with tests.

**Biggest Architectural Smell:**
The Overlay WinExe as build-anchor: the UI project references all four other *applications* purely to assemble the payload, and `UI_Engine.vb` hosts both the new-engine host and the legacy engine inside one 1,535-line partial-class form — engine regime, UI, and packaging are fused.

**Biggest Hidden Dependency:**
`Paths.FFmpegPath` / FFmpeg payload: config-dependent absolute paths in the wild (end-user logs show `C:\Duluka Corporation\...`), auto-detect only as fallback, 239 MB of gitignored shipping binaries, and the installer relying on a `Config\*` exclusion to avoid shipping the dev machine's absolute path.

**Most Suspicious Concurrency Path:**
The stop window of the legacy engine: `OnExited` (threadpool) racing `StopRecordingAsync` (Task.Run) through `StopAudioWriter()` with no interlock (both paths pass `If _audioEngine IsNot Nothing`), while `_state` is a plain field written from 4 thread contexts — the same window the 2026-09-05 audio-flush bug lived in.

**Largest Dead-Code Cluster:**
The dormant audio/legacy-FFmpegBackend stack: CaptureSession's `If False` blocks plus `AudioTap`/`AudioTapDeviceClock`/`WavSidecarWriter`/`SilenceKeepAlive`/`FFmpegPipelineBackend`/`MuxCoordinator`/`AudioSidecar` ≈ **3,800 lines** (plus ~1,215 lines of unguarded fakes and ~1,500 lines of test-only config/V2 stack inside production DLLs).

**Recommended Next 5 Actions:**
1. **Make the product reproducible:** commit or LFS-vendor the FFmpeg/Redist payload (or add a download-on-build step) + fix the layout Manifest to hard-fail without FFmpeg; pick one version authority (single build.txt writer; version the other assemblies).
2. **Turn on CI:** one GitHub Actions windows runner job calling `build-all.ps1 -RunTests` (the 7 suites already exist and are designed for it) + the `-StageLayout` manifest check; this alone de-risks half of this report.
3. **Unify engine-mode selection** (risk #4): one selector, one fallback rule, honest logging; add tests for init-failure in both regimes.
4. **Honest Pass accounting:** write `AudioTrackDiagnostics.DroppedBytes`, count `AudioEngineMuxSink` pending/skipped drops, add a drop-injection test; put the live-mux ffmpeg under the existing JobObjectGuard.
5. **Execute the SAFE-TO-FIX queue** (§18.1 — build counter split, dead members, missing-file project items, script BOMs, LICENSE.NOTICE), then schedule the duplicate-consolidation design decisions (TcpClientHelper, encoder tables, fakes-in-production) with the owner.

---
*Evidence base: 7 parallel forensic sweeps (dependency graph, app layer, legacy engine, capture core, video/encoder backends, build/installer, dead-code/duplicates) + independent verification pass by direct file reads (CaptureSession Finally block, RecordingEngineHost init-catch, UI_Engine dispatch, NVIDIA API supervisor, FFmpegCommandBuilderV1 QSV branch, NvEncConfigSerializer, legacy `_audioWriter` refs, FileVersionInfo of dist/dev-bin/installer, Designer timer intervals, root-dir listing). Line numbers are accurate as of the verification pass on 2026-09-06 and may drift due to concurrent edits observed during the audit.*
