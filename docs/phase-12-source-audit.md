# Phase 12 — Production Architecture Source Audit

**Task:** phase12-audit (Explore subagent)
**Date:** 2026-08-22
**Repo:** `ScotcsDuluka/NVIDIA-Shadowplay`
**Method:** GitHub raw fetches (`raw.githubusercontent.com/ScotcsDuluka/NVIDIA-Shadowplay/<branch>/<path>`) + local working tree for Engine-Rebuild-Stabilization state. No local checkout of `Stable` exists.

**Branch reality discovered during the audit (IMPORTANT — task description is loose):**
- `Stable` branch: `CaptureEngine/` contains ONLY a 270-byte placeholder `CaptureEngine.vbproj` (net8.0, NAudio 2.3.0, no subdirs). The subdirs (Backends/Configuration/Diagnostics/Engine/FFmpeg/Pipeline) mentioned in the task description actually exist only on `Engine-Rebuild-Stabilization`.
- `Engine-Audio` branch: hosts the legacy `Engine/NVIDIA Capture.vbproj` that historically built `NVIDIA Capture.exe`. This project was REMOVED from `Stable`'s Overlay.sln.
- `Engine-Rebuild-Stabilization` branch: where the new CaptureEngine.* architecture lives (foundation + video contract + Ddagrab skeleton + FFmpeg backend + config V2 + pipeline resolver).

---

## 1. Find the project that builds NVIDIA Capture.exe

**NOT FOUND on Stable branch.** The Stable `Overlay/NVIDIA Overlay.sln` does NOT include any project whose `AssemblyName` or output is `NVIDIA Capture`.

The project that historically built `NVIDIA Capture.exe` lives on the **`Engine-Audio` branch** at:

- **Path:** `Engine/NVIDIA Capture.vbproj` (Engine-Audio branch only; HTTP 200, 4638 bytes)
- **TargetFramework:** `net8.0-windows10.0.26100.0`
- **OutputType:** `WinExe`
- **RootNamespace:** `NVIDIA_Capture`
- **StartupObject:** `NVIDIA_Capture.My.MyApplication`
- **ProjectReferences:** NONE (legacy standalone)
- **PackageReferences:** NAudio 2.3.0 + NAudio.Asio 2.3.0 + NAudio.Core 2.3.0 + NAudio.Wasapi 2.3.0 + NAudio.WinForms 2.3.0 + NAudio.WinMM 2.3.0 + Newtonsoft.Json 13.0.4 + System.Text.Json 8.0.5
- **Bundled binaries:** `API-Core\ffmpeg.exe`, `API-Core\ffprobe.exe` (CopyToOutputDirectory=Always)
- **Key source files referenced:**
  - `Engine/[API]/[Engine] Client.vb` (10731 bytes, TCP client — see §4)
  - `Engine/Engine/[UI]/AudioSettingsForm.vb` + `.Designer.vb` + `.resx`
  - `Engine/Engine/[UI]/UI_Engine.vb` + `.Designer.vb` + `.resx`
  - `Engine/Engine/[Capture]/CaptureEngine.vb` (52610 bytes, 1020 lines — legacy capture engine)
  - `Engine/Engine/[Capture]/CaptureSettings.vb` (24219 bytes — legacy config loader)
  - `Engine/Engine/EncoderDetector.vb` (24411 bytes — string-matching FFmpeg encoder resolver)

**Critical implication for Phase 12:** There is NO production source on `Stable` that builds `NVIDIA Capture.exe`. Any deployed `NVIDIA Capture.exe` in `Overlay/bin/` is a stale leftover from the Engine-Audio era. Phase 12 must CREATE a new host project (or resurrect `Engine/NVIDIA Capture.vbproj` from Engine-Audio — but that pulls in the entire legacy capture engine + WinForms UI, which conflicts with the new CaptureEngine architecture).

## 2. Find all .sln / .slnx files and list which projects each contains

Three solution files exist on `Stable`:

### `Overlay/NVIDIA Overlay.sln` (4608 bytes, master solution)
Projects (5):
- `Overlay/NVIDIA Overlay.vbproj` — GUID `{47D8F77D-...}`, OutputType=WinExe, AssemblyName=`NVIDIA ShadowPlay`, TargetFramework=`net8.0-windows10.0.26100.0`, RootNamespace=`NVIDIA_Share`, UseWindowsForms=True. Builds **`NVIDIA ShadowPlay.exe`** (the orchestrator). PackageReferences: Microsoft.AspNetCore.Components.WebView 10.0.5, NAudio.Core 2.3.0, NAudio.Wasapi 2.3.0, Newtonsoft.Json 13.0.4, System.CodeDom 10.0.5, System.Management 10.0.5. **ProjectReferences: API/NVIDIA API.vbproj, App Experience/NVIDIA Experience.vbproj, CaptureEngine/CaptureEngine.vbproj, Notifier/NVIDIA Notifier.vbproj.** Bundles `API-Core\ffmpeg.exe` + ffprobe.exe + ffplay.exe + 9 avcodec/format/util DLLs + 27 Languages/*.json + fonts + NVIDIA_Shadowplay_Data/.
- `..\API\NVIDIA API.vbproj` — GUID `{F7369117-...}`, OutputType=WinExe, RootNamespace=`NVIDIA_API`, no AssemblyName override (so assembly name = `NVIDIA API`). Builds **`NVIDIA API.exe`** (the TCP server host). No PackageReferences. No ProjectReferences. Plain WinForms.
- `..\App Experience\NVIDIA Experience.vbproj` — GUID `{A527F7B7-...}`, OutputType=WinExe, RootNamespace=`NVIDIA_Experience`. Builds **`NVIDIA Experience.exe`**. PackageReferences: naudio.winforms 2.3.0. Has `[Forms - Project Files]/[API]/TCP/[APP] Client.vb` + `[Forms - Project Files]/[API]/UI/ToggleSwitch.vb`.
- `..\Notifier\NVIDIA Notifier.vbproj` — GUID `{26F262C5-...}`, OutputType=WinExe, RootNamespace=`Notifier_API`. Builds **`NVIDIA Notifier.exe`**. PackageReferences: Newtonsoft.Json 13.0.4. Has `[Forms Overlay - Project Files]/[API]/TCP/[Notifier] Client.vb`.
- `..\CaptureEngine\CaptureEngine.vbproj` — GUID `{624D1015-...}`. **Stable version is a 270-byte placeholder** (net8.0, NAudio 2.3.0, no subdirs, no AssemblyName override). **NOT referenced by Overlay.vbproj's ProjectReference** despite being in the .sln — Overlay's ProjectReference is to a different `CaptureEngine.vbproj` (the same path). This is the leftover stub.

### `App Experience/NVIDIA Shadowplay Helper.slnx` (76 bytes)
```xml
<Solution>
  <Project Path="NVIDIA Shadowplay Helper.vbproj" />
</Solution>
```
Standalone single-project solution. **Note:** project file is `NVIDIA Shadowplay Helper.vbproj` — a DIFFERENT file than the `NVIDIA Experience.vbproj` referenced in the master .sln. Not probed (out of audit scope); likely a helper/secondary App Experience build.

### `Notifier/Notifier-API.slnx` (64 bytes)
```xml
<Solution>
  <Project Path="Notifier-API.vbproj" />
</Solution>
```
Standalone single-project solution. **Note:** project file is `Notifier-API.vbproj` — different from `NVIDIA Notifier.vbproj` in the master .sln. May be a slimmer version of the notifier (the audit reference to "Notifier-API" with RootNamespace=`Notifier_API` in the master .vbproj suggests they share a namespace but are separate projects).

**`Overlay/NVIDIA ShadowPlay.sln`** — NOT FOUND (HTTP 404). The task's hint at this path is wrong.
**`CaptureEngine.sln` / `CaptureEngine.All.sln`** — NOT FOUND on Stable (they exist only on Engine-Rebuild-Stabilization per the prior audit `Engine-Rebuild_Architecture_Build_Audit.md`).

## 3. Read the actual `engine.json` file

**NOT FOUND** in the repository on any branch (Stable, Engine-Audio, Engine-Rebuild, Engine-Rebuild-Stabilization). All probes returned HTTP 404.

The `.gitignore` confirms why: `**/bin/`, `**/obj/`, and `Overlay/NVIDIA_Shadowplay_Data/` are all git-ignored. The runtime `engine.json` exists only in build output / runtime data dirs.

### Source-of-truth schema (legacy — Engine-Audio)

Reconstructed from `Engine/Engine/[Capture]/CaptureSettings.vb` (Engine-Audio branch, lines 236-258, `LoadEngineSettings`):

```json
{
  "CaptureMethod": "ddagrab",
  "PixelFormat": "nv12",
  "RateControl": "cbr",
  "FileFormat": "mp4",
  "FFmpegPath": "",
  "HotkeyStart": "Control+Shift+F9",
  "HotkeyStop": "Control+Shift+F10"
}
```

Sibling files (also runtime-only, NOT in repo): `config.json` (general), `video.json` (encoder/FPS/bitrate), `audio.json` (system/mic/volume/track mode). `CaptureSettings.LoadAll()` walks `AppDomain.CurrentDomain.BaseDirectory` + parent dirs to find them. All loads use bare `Catch` blocks (silent fallback to defaults — see `Engine-Audio_Video_FFmpeg_Audit.md` RISK-4).

### Source-of-truth schema (new — Engine-Rebuild-Stabilization)

`CaptureEngine/Configuration/EngineConfigV2.vb` defines the V2 schema (file name: `engine-config.v2.json`, loaded by `CaptureEngine/Configuration/ConfigLoader.vb` from `AppContext.BaseDirectory`). Sections:
- `Video.Capture` { Method, OutputIndex, Framerate }
- `Video.Resolution` { Mode, Width, Height }
- `Video.Encoder` { Key, FFmpegCodec, Preset, Tune, Profile, RateControl, BitrateBps, MinrateBps, MaxrateBps, BufsizeBps, GopSize, FpsMode, SpatialAQ, TemporalAQ, LookAhead, ZeroLatency, Cq, PixelFormat }
- `Audio.System` / `Audio.Microphone` { Enabled, Volume, DeviceId, DeviceName }
- `Audio.Encoding` { Codec, BitrateBps, SampleRate, Channels, TrackMode }
- `Audio.Sync` { MaxOffsetSec, MinOffsetSec, AVSyncToleranceMs }
- `Output` { Container, Directory, FilenamePattern, Overwrite, FastStart, TempVideoSuffix, TempAudioSuffix, TempMicSuffix }
- `Runtime` { Hotkeys (8 hotkeys), FFmpegPath, FFprobePath, ProcessPriority, LogLevel, LogToFile, LogPath, DebugMode, AutoStartOverlay, ShutdownTimeout { FFmpegQuit=10000, MuxWait=60000, FFprobeWait=5000 } }
- `Experimental` { EnableZeroCopy, EnableD3D11Interop }

**Phase 12 implication:** The Phase 12 spec §13 Q2 asks "what's the current format? Need to add FfmpegPath, DefaultOutputDir, AudioWarmupSec fields without breaking existing config." Answer: `FFmpegPath` already exists at `Runtime.FFmpegPath`. `DefaultOutputDir` already exists as `Output.Directory`. `AudioWarmupSec` does NOT exist — must be added (likely to `Audio.Sync`).

## 4. Find existing IPC mechanisms

**Existing IPC: TCP sockets on `127.0.0.1:5000`.** NOT named pipes, NOT gRPC, NOT WCF.

### Server side — `API/[Forms - Project Files]/[API]/Server.vb` (10428 bytes, Stable)
`Partial Public Class API_RUN`. `TcpListener(IPAddress.Any, 5000)`. Multi-client broadcast pattern. Per-client `ClientInfo { Client, Writer, AppName, LastActivity, ConnectedAt }`. Methods: `StartServer()` (async accept loop), `HandleClientAsync(info)` (async read loop), `ProcessMessage(msg, info)` (parses `[Send] <AppName>|<cmd>:<value>`), `Broadcast(msg, senderInfo)` (echo to all other clients), `HeartbeatMonitor(token)` (kills clients inactive >30s), `Log(app, msg)`. Form lifecycle: `Server_Load` starts timer + listener task; `Server_FormClosing` cancels heartbeat, stops listener, closes all clients.

### Client side — `TcpClientHelper.vb` (6198 bytes, IDENTICAL copies in 2 projects on Stable)
- `App Experience/[Forms - Project Files]/[API]/TCP/TcpClientHelper.vb`
- `Notifier/[Forms Overlay - Project Files]/[API]/TCP/TcpClientHelper.vb`

`Public Class TcpClientHelper Implements IDisposable`. Constructor: `New(appName, host="127.0.0.1", port=5000, autoReconnect=True)`. Methods: `Connect()`, `Disconnect()`, `Send(cmd, value="")`, `SendLog(message)`. Events: `OnMessageReceived(msg)`, `OnDisconnected()`, `OnReconnecting()`. Loops: `ListenLoop` (read line, raise event), `PingLoop` (send `[Send] <appName>|ping` every 10s), `ReconnectLoop` (exponential backoff 1s→30s max). Wire format:
- Send: `[Send] <appName>|<cmd>` or `[Send] <appName>|<cmd>:<value>`
- Receive: `[Receive] <appName>|<message>`
- System: `[System]|pong`

### App Experience client — `App Experience/[Forms - Project Files]/[API]/TCP/[APP] Client.vb` (1187 bytes)
`Partial Public Class NVIDIA_Shadowplay_Helper`. Constructs `tcp = New TcpClientHelper("NVIDIA  APP")`. Waits for `NVIDIA API` process to start (`While Process.GetProcessesByName("NVIDIA API").Length = 0`). OnMessage parses `<cmd>:<value>` and dispatches (currently only `Case Else` Debug.WriteLine — no real handlers in this stub).

### Notifier client — `Notifier/[Forms Overlay - Project Files]/[API]/TCP/[Notifier] Client.vb` (8137 bytes)
`Partial Public Class Loader`. `tcp As TcpClientHelper`. OnMessage parses `[NVIDIA Overlay]|<key>`, looks up `NotificationData` from a hardcoded list of 35+ notification keys (`l10n.test`, `l10n.notificationManualRecordStarted`, etc.), calls `LangHelper.GetText(locKey, args)` for localization, then `UpdateNotifier(message, png, ico, color)` + `tcp.SendLog(message)`.

### Engine-side client (Engine-Audio branch only) — `Engine/Engine/[API]/[Engine] Client.vb` (10731 bytes)
`Partial Public Class UI_Engine`. `Public Shared tcp As TcpClientHelper` (named `"NVIDIA Engine"`). Sends `register:NVIDIA Engine` + `engine_ready` on connect. Listens for commands: `PREWARM_FFMPEG:<path>[|<encoder>]`, `engine_config_changed[:video|config]`, `RECORD_START:<outputPath>` (alias→`engine_record_start`), `RECORD_STOP`, `REPLAY_START:<seconds>`, `REPLAY_STOP`, `REPLAY_SAVE:<path>;<duration>`, `engine_get_status`, `engine_load_config`, `engine_set_encoder:<value>`. Sends `engine_response:<command>,<status>[,<data>][,req=<reqId>]`. Uses `BeginUiInvoke` (fire-and-forget) to avoid blocking the listener thread.

**Phase 12 implication:** The IPC pattern is alive and battle-tested on Stable. Phase 12's "NVIDIA Capture.exe → IPC → NVIDIA ShadowPlay.exe" plan should **REUSE** `TcpClientHelper.vb` verbatim and follow the same `[Send] <AppName>|<cmd>:<value>` wire format. The new host process should register as `"NVIDIA Engine"` (or similar) and reuse the legacy command set (`engine_record_start` / `engine_record_stop` / `engine_get_status`) to minimize Overlay-side changes. Named pipes (Phase 12 spec §9 proposal) is NOT necessary — TCP/5000 is already deployed.

## 5. Find existing Capture lifecycle / recording orchestration

### Foundation lifecycle (Engine-Rebuild-Stabilization, FROZEN @ 82d792ab)
- `CaptureEngine/Engine/EngineState.vb` (1198 bytes) — 8-state enum: `Created, Initializing, Stopped, Starting, Running, Stopping, Faulted, Disposed`.
- `CaptureEngine/Engine/CaptureEngine.vb` (14893 bytes) — `Public NotInheritable Class CaptureEngine Implements IDisposable`. State machine. `Public Sub Initialize(config As EngineConfig)`, `Public Sub Start()`, `Public Sub [Stop]()`, `Public Sub Dispose()`. Idempotent Start/Stop. Dispose routes through `StopPipeline` boundary when Running/Starting (testable via `Friend ReadOnly Property StopPipelineCallCount`). 14/14 tests pass. **FOUNDATION — Phase 12 HARD RULES say "No Foundation changes" — this class is NOT to be modified.**

### Foundation video contract (Engine-Rebuild-Stabilization)
- `CaptureEngine.Video/Contract/IVideoCaptureBackend.vb` (56 lines) — PUSH model: `Sub Initialize(context As IVideoBackendContext)`, `Sub Start(sink As IVideoFrameSink)`, `Sub [Stop]()`, `ReadOnly Property Diagnostics As IVideoBackendDiagnostics`. `Inherits IDisposable`. Sink-owned queue (BoundedVideoFrameSink).
- `CaptureEngine.Video.Ddagrab/DdagrabBackend.vb` (423 lines) — **SKELETON.** Full lifecycle state machine but worker thread always returns `NoFrame` (real DXGI Output Duplication is a TODO). Has TODO for: D3D11 device creation, AcquireNextFrame, BGRA8 validation, timestamp conversion.

### New backends contract (Engine-Rebuild-Stabilization, NOT Foundation)
- `CaptureEngine/Backends/IVideoBackend.vb` (8946 bytes) — PULL model: `ReadOnly Property CurrentState As VideoBackendState`, `Sub Start()`, `Sub [Stop]()`, `Function GetFrame() As VideoFrame`, `Function GetDiagnostics() As IReadOnlyDictionary(Of String, Long)`. `VideoBackendState` enum: `Created, Starting, Running, Stopping, Muxing, Stopped, Disposed, Faulted`. **NO concrete implementations exist** — the doc comment explicitly states "Until adapter is implemented: IVideoBackend is an empty contract (no concrete implementations)".

### FFmpeg pipeline orchestrator (Engine-Rebuild-Stabilization)
- `CaptureEngine.FFmpegBackend/FFmpegPipelineBackend.vb` (496 lines) — `Public NotInheritable Class FFmpegPipelineBackend Implements IVideoBackend, IDisposable`. Fire-and-forget (`GetFrame()` returns Nothing). Owns `FFmpegProcessHost`, `FFmpegStderrParser`, `AudioSidecar`, `MuxCoordinator` (lazy at Stop). Generation guard for stale callbacks. Lifecycle: Created→Starting→Running→Stopping→[Muxing]→Stopped→Disposed. Events: `StateChanged`, `RecordingStarted`, `RecordingStopped`, `ErrorOccurred`. **This is a PER-SESSION orchestrator, NOT a process-lifetime singleton.**

### Legacy Engine-Audio capture engine
- `Engine/Engine/[Capture]/CaptureEngine.vb` (Engine-Audio branch, 1020 lines) — `Partial Public Class CaptureEngine Implements IDisposable`. State enum: `Idle, Detecting, Recording, Paused, Stopping, Muxing, HasError`. `Public Async Function StartRecordingAsync(Optional overrideOutputPath) As Task(Of Boolean)`, `Public Async Function StopRecordingAsync() As Task(Of Boolean)`. Two-process mode: video FFmpeg + WASAPI AudioFileWriter + mux FFmpeg at stop. Events: `StateChanged`, `RecordingStarted`, `RecordingStopped`, `ErrorOccurred`, `FrameCaptured`, `ProgressUpdated`. Has `_jobGuard As JobObjectGuard` (kills child FFmpeg if parent dies).

### Phase 12 target classes
- `RecordingEngine` (process-lifetime singleton) — **NOT FOUND** in production code.
- `CaptureSession` (per-session owner) — **NOT FOUND** in production code.
- `ICaptureEngine` — NOT FOUND.
- `IRecordingSession` — NOT FOUND.
- `RecordingEngine`/`CaptureSession`/`SessionResult`/`SessionConfig`/`RecordingEngineConfig`/`EngineStatus` exist ONLY as proposed C# declarations in `download/Phase12_Architecture_Spec.md` — they are design-doc, not committed code.

## 6. Find existing FFmpeg integration

### FFmpeg command builder contract (Engine-Rebuild-Stabilization)
- `CaptureEngine/FFmpeg/IFFmpegCommandBuilder.vb` (2611 bytes) — `Public Interface IFFmpegCommandBuilder`. `Function Build(outputFile As String) As String`, `ReadOnly Property BuilderLabel As String`. Lives in `CaptureEngine` assembly so test projects can reference without WinForms deps.
- `CaptureEngine/FFmpeg/FFmpegCommandBuilderV1.vb` — wraps legacy `CaptureEngine.BuildFFmpegArguments` (byte-identical output for backward compat).
- `CaptureEngine/FFmpeg/FFmpegCommandBuilderV2.vb` — reads from `EngineConfigV2`.

### FFmpeg process host (Engine-Rebuild-Stabilization)
- `CaptureEngine.FFmpegBackend/FFmpegProcessHost.vb` (290 lines) — `Public NotInheritable Class FFmpegProcessHost Implements IDisposable`. Single FFmpeg process lifecycle: `Start()` (spawns `Process` with `ProcessStartInfo` — UseShellExecute=False, RedirectStandardOutput/Error/Input, CreateNoWindow=True), `SendQuit()` (writes `"q" & vbLf` to StandardInput), `WaitForExit(timeoutMs)` (returns False on timeout, NOT throw), `Kill()`. Properties: `FFmpegPath`, `Arguments`, `OutputPath`, `Generation` (incremented on each Start for stale-callback guard). Events: `Exited(generation, exitCode)`, `StderrLine(generation, line)`. NOT internally synchronized — caller serializes. **NEVER hold a lock across WaitForExit/Kill.**

### FFmpeg stderr parser (Engine-Rebuild-Stabilization)
- `CaptureEngine.FFmpegBackend/FFmpegStderrParser.vb` (10661 bytes) — parses FFmpeg stderr stats (`frame=`, `Lsize=`, `time=`, `dup=`, `drop=`, `speed=`).

### Mux coordinator (Engine-Rebuild-Stabilization)
- `CaptureEngine.FFmpegBackend/MuxCoordinator.vb` (332 lines) — `Public NotInheritable Class MuxCoordinator Implements IDisposable`. Steps: `ProbeVideoDuration()` (ffprobe temp video → seconds, returns 0.0 on failure), then spawn mux FFmpeg: `-i video.mp4 -i system.wav [-i mic.wav] -c:v copy -c:a aac -ss <offset> -t <duration> → final.mp4`. Cleans up temp files only on mux success. Resolves ffprobe path from FFmpeg dir + `API-Core` subfolder fallbacks. Per-track sync offsets (SystemOffsetSec, MicOffsetSec). Properties: `TempVideoPath`, `TempSystemWavPath`, `TempMicWavPath`, `OutputPath`, `HasSystemAudio`, `HasMicAudio`, `SystemVolume`, `MicVolume`, `SeparateTracks`.

### Pipeline resolver (Engine-Rebuild-Stabilization)
- `CaptureEngine/Pipeline/PipelineResolver.vb` (5450 bytes) — `Public NotInheritable Class PipelineResolver`. `Shared Function Resolve(cfg As EngineConfigV2) As PipelineConfig`. `Shared Function BuildFFmpegCommandBuilder(cfg) As IFFmpegCommandBuilder`. Video backend resolution: `"ffmpeg_ddagrab"` / `"ffmpeg_gdigrab"` / `"ffmpeg_gfxcapture"` / `"dxgi"` (future, throws `NotImplementedException`). Audio backend: `"wasapi_loopback"` or `"none"`.

### Legacy FFmpeg invocation (Engine-Audio)
- `Engine/Engine/[Capture]/CaptureEngine.vb` lines 207-237 — direct `Process.Start(_settings.FFmpegPath, args)`. Two-process mode controlled by `_useTwoProcess = _settings.SystemAudioCapture OrElse _settings.MicCapture`. Stop: send `"q" & vbLf` to stdin → `WaitForExit(10000)` → `Kill()` + `WaitForExit(2000)`.
- `Engine/Engine/[FFmpeg]/FFmpegArgumentBuilder.vb` (18065 bytes) — builds ddagrab/gdigrab/gfxcapture lavfi args, encoder selection ("nvenc"→NVIDIA, "qsv"→IntelQSV, "amf"→AMD, else→software), `-preset`, `-tune ll`, `-rc cbr`, `-b:v`, `-g <FPS>`, `-fps_mode cfr`, `-spatial-aq 1`, `-temporal-aq 1`.

### `IFFmpegRunner`
NOT FOUND. The closest abstraction is `IFFmpegCommandBuilder` (arg builder) + `FFmpegProcessHost` (subprocess manager). Phase 12's `FfmpegPathResolver` (deployment-aware) does NOT exist — current resolver is `_settings.FFmpegPath` with `FindFFmpegPath()` PATH-walking fallback (legacy) or `Runtime.FFmpegPath` from V2 config (new).

## 7. Find existing Audio abstraction

**No `IAudioCapture` interface exists** in production code (Stable or Engine-Rebuild-Stabilization).

- `CaptureEngine.FFmpegBackend/AudioSidecar.vb` (4955 bytes) — `Public NotInheritable Class AudioSidecar Implements IDisposable`. **STUB.** `Start()` and `Stop()` are no-ops. `HasAudioData` always returns False. Captures QPC timestamps (`SystemStartTicks`, `MicStartTicks`) on Start for future mux offset calc. Properties: `TempSystemWavPath`, `TempMicWavPath`, `SystemAudioEnabled`, `MicEnabled`. Doc comment: "TODO (future task): initialize NAudio WasapiLoopbackCapture + WaveFileWriter for each enabled track. Write to temp .wav files."
- Legacy Engine-Audio: `Engine/Engine/[Capture]/CaptureEngine.vb` references `_audioWriter As AudioFileWriter` (lines 91, 282-284, 406-409). The `AudioFileWriter.vb` source file was NOT FOUND on Engine-Audio branch via common-path probes (likely lives at a path I didn't guess — not critical since legacy is not being reused).
- **NAudio in production (Stable):**
  - `Overlay/NVIDIA Overlay.vbproj` — NAudio.Core 2.3.0 + NAudio.Wasapi 2.3.0
  - `App Experience/NVIDIA Experience.vbproj` — naudio.winforms 2.3.0
  - `CaptureEngine/CaptureEngine.vbproj` (Stable placeholder) — NAudio 2.3.0
- **NAudio in Engine-Rebuild-Stabilization new architecture:** NONE. `CaptureEngine.FFmpegBackend.vbproj` references only `CaptureEngine` (Foundation). No NAudio packages.
- `WasapiLoopbackCapture` in production code (outside spike): NOT FOUND.

## 8. Find existing Encoder abstraction

**No `IEncoder` / `IVideoEncoder` interface exists** in production code (Stable or Engine-Rebuild-Stabilization).

- **No `CaptureEngine.Encoder` project exists.**
- Encoder config (V2): `EngineConfigV2.Video.Encoder` (`EncoderSubSection`) — `Key` ("NVENC_H264", "NVENC_HEVC", "NVENC_AV1", "QuickSync_H264", "AMF_H264", "LibX264", "LibX265"), `FFmpegCodec` ("h264_nvenc", "hevc_nvenc", etc.), `Preset` (p1-p7), `Tune` ("ll"/"ull"/"lossless"), `Profile`, `RateControl` (cbr/vbr/cq), `BitrateBps`, `MinrateBps`, `MaxrateBps`, `BufsizeBps`, `GopSize`, `FpsMode`, `SpatialAQ`, `TemporalAQ`, `LookAhead`, `ZeroLatency`, `Cq`, `PixelFormat`. **Config-only — no encoder runtime abstraction.**
- **NVENC references outside the spike:** NOT FOUND in production code.
- **NVENC P/Invoke (`NvEncodeAPI.cs`):** Only in `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` (we're told not to explore the spike). NOT in production.
- **Hardware encoder detection (NVENC/QSV/AMF):**
  - Legacy `Engine/Engine/EncoderDetector.vb` (Engine-Audio, 24411 bytes) — does GPU detection via WMI/registry (returns string like "NVIDIA"/"Intel"/"AMD"). Does NOT invoke NVENC API.
  - Legacy `Engine/Engine/[FFmpeg]/FFmpegArgumentBuilder.vb` lines 349-360 — string matching on encoder name ("nvenc"→NVIDIA, "qsv"→IntelQSV, "amf"→AMD). This is FFmpeg CLI arg resolution, not a real encoder abstraction.

**Phase 12 implication:** The NVENC encoder owner must be CREATED from scratch. The spike's `NvEncodeAPI.cs` struct layouts can be copied verbatim per HARD RULES ("no changes to NVENC struct layouts").

## 9. Find existing Pipeline / orchestration

The `CaptureEngine/Pipeline/` directory (Engine-Rebuild-Stabilization) has:
- `PipelineConfig.vb` (49 lines) — snapshot DTO: `VideoBackend`, `Encoder`, `AudioBackend`, `OutputContainer`, `SourceConfig As EngineConfigV2`. ToString: `Pipeline[Video=..., Encoder=..., Audio=..., Container=...]`.
- `PipelineResolver.vb` (129 lines) — `Resolve(cfg) As PipelineConfig` + `BuildFFmpegCommandBuilder(cfg) As IFFmpegCommandBuilder`. Resolves video backend (`ffmpeg_ddagrab` / `ffmpeg_gdigrab` / `ffmpeg_gfxcapture` / `dxgi` [future, throws `NotImplementedException`]) + audio backend (`wasapi_loopback` / `none`) + encoder (from `cfg.Video.Encoder.FFmpegCodec`).

**Existing pipeline orchestrator (FFmpeg-based):** `CaptureEngine.FFmpegBackend/FFmpegPipelineBackend.vb` — composes `FFmpegProcessHost` + `FFmpegStderrParser` + `AudioSidecar` (stub) + `MuxCoordinator`. Per-session lifecycle (Created→Starting→Running→Stopping→[Muxing]→Stopped→Disposed). Generation guard. This orchestrator delegates ALL encoding to FFmpeg subprocess — it does NOT own GPU resources.

**Pipeline orchestrator that composes Backends + Encoders + Outputs in-process (Phase 12 style):** NOT FOUND. The existing `PipelineResolver` only resolves the FFmpeg command builder; it does NOT orchestrate a D3D11 + NVENC + WASAPI + FFmpeg-mux pipeline in-process. Phase 12's `RecordingEngine` (process-lifetime singleton owning D3D11 device + DXGI duplication + NVENC encoder) is a NEW component that does NOT exist anywhere in production code.

---

# Final Deliverable — Reuse / Extend / Replace Table

| Phase 12 Component | Existing Production Code | Action | Rationale |
|---|---|---|---|
| **RecordingEngine** (process-lifetime singleton) | `CaptureEngine/Engine/CaptureEngine.vb` (Foundation, frozen @ 82d792ab) — 8-state lifecycle, idempotent Start/Stop/Dispose, StopPipeline boundary. **NO GPU resource ownership.** | **REPLACE (separate class)** | Foundation is FROZEN (Phase 12 HARD RULES forbid modification). Foundation's `CaptureEngine` is lifecycle-only with no D3D11/NVENC ownership. Phase 12's `RecordingEngine` must be a NEW class (e.g. `CaptureEngine.Recording.RecordingEngine`) that OWNS D3D11Device + DXGI duplication + NVENC encoder for process lifetime. It can REUSE the Foundation's state-machine pattern (Created→Initializing→Stopped→...) and the StopPipeline boundary design — but as a new class, not by modifying Foundation. |
| **CaptureSession** (per-session owner) | NOT FOUND. Closest analog: `CaptureEngine.FFmpegBackend/FFmpegPipelineBackend.vb` — per-session lifecycle, owns FFmpegProcessHost+AudioSidecar+MuxCoordinator. But it delegates encoding to FFmpeg subprocess, not to a parent engine's NVENC. | **REPLACE** | FFmpegPipelineBackend delegates ALL encoding to FFmpeg subprocess. Phase 12's CaptureSession must reuse the parent engine's persistent NVENC encoder + DXGI duplication, only owning per-session WASAPI audio + FFmpeg mux subprocess + output files. The ownership pattern (try/finally with idempotent Dispose, generation guard for stale callbacks) is solid in FFmpegPipelineBackend and SHOULD be copied. But the resource set is fundamentally different. |
| **D3D11 device factory** | NOT FOUND in production. `DdagrabBackend.vb` has a TODO comment "Real D3D11 device creation per §5" but worker thread always returns NoFrame (skeleton). | **REPLACE (copy from spike)** | Production has no D3D11 device code outside the spike. Per HARD RULES, spike's `NvEncodeAPI.cs` struct layouts can be copied verbatim. The D3D11 device creation logic from `spikes/D3D11_NVENC_Spike/Phases/Phase1_D3D11Device.cs` (proven in P1-STABILIZATION audit: GTX 1080 Ti PASS) should be wrapped in `CaptureEngine.Recording.Internal.D3D11DeviceFactory`. |
| **DXGI duplication owner** | NOT FOUND in production. `DdagrabBackend.vb` TODO mentions "DuplicateOutput on primary output" but never implemented. Legacy Engine-Audio uses FFmpeg `ddagrab` lavfi filter (subprocess) — not DXGI duplication. | **REPLACE (copy from spike)** | Phase 11 PostMortem root cause #2: Windows constrains `IDXGIOutputDuplication` to ONE live instance per output per process. The spike's per-session `DuplicateOutput` call fails on 2nd session. Phase 12 must own DXGI duplication at the RecordingEngine (process-lifetime) level — sessions BORROW, never own. Copy spike's Phase 2 logic, wrap in `DxgiDuplicationFactory`, own at RecordingEngine. |
| **NVENC encoder owner** | NOT FOUND in production. No `IEncoder` interface, no `CaptureEngine.Encoder` project, no NVENC P/Invoke outside spike. | **REPLACE (copy from spike)** | Phase 11 PostMortem root cause #3: per-session `OpenEncodeSessionEx` exhausts NVENC session slots (error 21). Must be owned at RecordingEngine level. Copy spike's `NvEncodeAPI.cs` struct layouts verbatim + Phase 4-9 logic into `NvencEncoderFactory`. Wrap in `NvencEncoder` IDisposable owner. |
| **NAudio audio capture** | NOT FOUND as production abstraction. `CaptureEngine.FFmpegBackend/AudioSidecar.vb` is a STUB (Start/Stop are no-ops, HasAudioData=False, TODO: "initialize NAudio WasapiLoopbackCapture + WaveFileWriter"). Legacy `Engine-Audio` had `AudioFileWriter` (WASAPI loopback → temp.wav) but the source file was not located on the branch. Stable `Overlay/NVIDIA Overlay.vbproj` has NAudio.Core 2.3.0 + NAudio.Wasapi 2.3.0 PackageReferences. | **EXTEND AudioSidecar.vb** | The AudioSidecar class is the right architectural shape (per-session owner, temp .wav paths, per-track QPC timestamps, IDisposable) — just empty inside. Phase 12 should EXTEND it by replacing the stub TODOs with real `NAudio.WasapiLoopbackCapture` + `WaveFileWriter` initialization. The QPC timestamp capture (`SystemStartTicks`, `MicStartTicks`) is ALREADY in place — matches what mux needs. Add NAudio.Wasapi 2.3.0 PackageReference to `CaptureEngine.FFmpegBackend.vbproj` (currently has none). |
| **FFmpeg path resolver** | PARTIAL. Legacy `Engine-Audio CaptureSettings.FindFFmpegPath()` walks PATH (caused Phase 11 failure #1). V2 `EngineConfigV2.Runtime.FFmpegPath` exists as a config field but no `FfmpegPathResolver` class exists. `MuxCoordinator.ProbeVideoDuration()` has a partial fallback: looks for `ffprobe.exe` in FFmpeg dir + `API-Core` subfolder. | **REPLACE (new class)** | Phase 11 failure #1 root cause: spike walks PATH and falls back to literal `"ffmpeg"`. Phase 12 spec §8 proposes `FfmpegPathResolver` with deployment-relative candidate search (`<exeDir>/API-Core/ffmpeg.exe`, `<exeDir>/ffmpeg.exe`, `<exeDir>/../API-Core/ffmpeg.exe`). The MuxCoordinator's ffprobe resolution pattern is the right shape — extract it into a reusable `FfmpegPathResolver` and have both MuxCoordinator and the new RecordingEngine use it. |
| **FFmpeg mux subprocess** | FOUND — `CaptureEngine.FFmpegBackend/MuxCoordinator.vb` (332 lines). Full implementation: ffprobe duration probe + mux FFmpeg subprocess + per-track `-ss`/`adelay`/`apad` filters + `-t <duration>` + temp file cleanup. | **REUSE verbatim** | MuxCoordinator is complete, tested, and matches Phase 12's needs exactly. Phase 12's CaptureSession should compose a `MuxCoordinator` instance (lazy at Stop time, like FFmpegPipelineBackend does). The only change needed: wire `MuxCoordinator.FFmpegPath` through the new `FfmpegPathResolver` instead of relying on caller-supplied path. The temp-file-suffix fields (`Output.TempVideoSuffix` etc. in EngineConfigV2) already match MuxCoordinator's expectations. |
| **Session result DTO** | NOT FOUND as a class. Legacy `Engine-Audio CaptureEngine` exposes `LastAudioDiagnostics`, `LastFFmpegStatsLine`, `LastMuxSummary` as separate properties (post-recording analysis). FFmpegPipelineBackend has no result DTO. | **REPLACE (new DTO)** | Phase 12 spec §5 proposes `SessionResult` with structured fields (`FramesEncoded`, `AudioSamples`, `VideoStreamFound`, `AudioStreamFound`, `FileExists`, `FileSize`, `Pass` computed property). This is the right shape. The legacy `LastAudioDiagnostics` + `LastFFmpegStatsLine` + `LastMuxSummary` strings should be parsed into structured fields. New `SessionResult.vb` in `CaptureEngine.Recording` namespace. |
| **Engine config binding** | FOUND — `CaptureEngine/Configuration/EngineConfigV2.vb` (291 lines, comprehensive schema) + `ConfigLoader.vb` (154 lines, loads `engine-config.v2.json` from AppContext.BaseDirectory with case-insensitive System.Text.Json) + `ConfigMigrator.vb` + `ConfigValidator.vb` (Engine-Rebuild-Stabilization). | **REUSE + EXTEND** | EngineConfigV2 already has `Runtime.FFmpegPath`, `Runtime.FFprobePath`, `Output.Directory` (= Phase 12's `DefaultOutputDir`), `Runtime.ShutdownTimeout` (FFmpegQuit, MuxWait, FFprobeWait), `Audio.Sync` (offsets). Missing: `AudioWarmupSec` (Phase 12 spec §13 Q2) — add to `Audio.Sync` or new `Audio.Warmup` subsection. Also missing: a top-level binding to Phase 12's `RecordingEngineConfig` (spec §5) — but `RecordingEngineConfig` is mostly a subset of `EngineConfigV2.Runtime` + `EngineConfigV2.Output`. Recommendation: bind RecordingEngine constructor directly to `EngineConfigV2` (don't create a parallel config class), expose `RecordingEngineConfig` as a thin projection if needed for the spec's API. |
| **Logger** | FOUND — `CaptureEngine/Diagnostics/EngineLogger.vb` (Foundation, frozen). Synchronous, thread-safe (global lock), format `[yyyy-MM-dd HH:mm:ss.fff] [LEVEL] source message`. Levels: Debug/Info/Warning/Error. Default sink: Console.WriteLine. | **EXTEND (with file sink + MEL)** | Foundation logger is solid and USED by every Engine-Rebuild-Stabilization class. Phase 12 spec §10 wants structured logging to `Logs/capture-engine.log` via `Microsoft.Extensions.Logging`. The Foundation logger's `sink As Action(Of String)` constructor parameter already supports this — pass a file-writing sink instead of Console. Do NOT replace the Foundation logger; wrap it or add a `FileLoggerSink` helper. For richer structured logging (log scopes, event IDs), MEL can be added as a SEPARATE adapter in `CaptureEngine.Recording.Diagnostics` that wraps `EngineLogger` — Foundation stays untouched. |
| **IPC layer** | FOUND — `TcpClientHelper.vb` (6198 bytes, duplicated in App Experience + Notifier; same class on Engine-Audio branch's Engine project) + `API/[Forms - Project Files]/[API]/Server.vb` (10428 bytes, TcpListener on port 5000, multi-client broadcast). Battle-tested on Stable across 4 process types. | **REUSE verbatim** | TCP/5000 pattern is alive, deployed, and supports the exact command set Phase 12 needs (`engine_record_start`, `engine_record_stop`, `engine_get_status`, `engine_load_config`, `engine_set_encoder`, `PREWARM_FFMPEG`, `engine_config_changed`). Phase 12's new NVIDIA Capture.exe host should: (1) copy `TcpClientHelper.vb` into a shared location (e.g. `CaptureEngine.Ipc` new project, or duplicate into the new host project); (2) register as `"NVIDIA Engine"` to match the existing Overlay-side command set; (3) use the same `[Send] <AppName>|<cmd>:<value>` wire format. Phase 12 spec §9's proposed named-pipes is UNNECESSARY — TCP/5000 is already in production. The ONE thing missing from the existing IPC layer is a `start`/`stop`/`status` JSON protocol (Phase 12 spec §9 sketch) — current protocol is pipe-delimited key:value. Either keep the existing protocol (less code change in Overlay) or extend with a JSON sub-protocol for richer SessionResult payloads. |

---

## Brutal honesty summary

1. **Phase 12 is GREENFIELD for the GPU-resource ownership layer.** Production has ZERO D3D11 / NVENC code outside the spike. The DdagrabBackend skeleton is lifecycle-only (worker returns NoFrame). FFmpegPipelineBackend delegates ALL encoding to FFmpeg subprocess. The "process-lifetime singleton owning D3D11+NVENC" pattern does not exist anywhere — it must be built from scratch, reusing the spike's proven Phase 1-9 interop.

2. **Foundation is OFF-LIMITS.** `CaptureEngine/Engine/CaptureEngine.vb` (Foundation, frozen @ 82d792ab) is the lifecycle state machine. Phase 12's `RecordingEngine` is a NEW class that can borrow the pattern (Created→Initializing→Stopped→...→Disposed, StopPipeline boundary, idempotent Dispose routing through stop path) but must NOT modify Foundation. Put `RecordingEngine` in a new namespace like `CaptureEngine.Recording`.

3. **NVIDIA Capture.exe is NOT BUILT on Stable.** The `Engine/NVIDIA Capture.vbproj` project was REMOVED from Stable's Overlay.sln. It still exists on Engine-Audio branch but as the LEGACY capture engine (WinForms + NAudio suite + FFmpeg subprocess + legacy CaptureSettings.vb). Phase 12 must CREATE a new host project that produces `NVIDIA Capture.exe` — either a new project in the repo, or resurrect `Engine/NVIDIA Capture.vbproj` from Engine-Audio and STRIP it down to a thin host that constructs `RecordingEngine` + wires TCP. The first option is cleaner; the second drags in 1020 lines of legacy code that conflicts with the new architecture.

4. **engine.json does NOT exist in source.** It's a runtime-only file. The Phase 12 spec §13 Q2 ("what's the current format?") has a clear answer: the V2 schema is `engine-config.v2.json` (loaded by `CaptureEngine/Configuration/ConfigLoader.vb`). The legacy `engine.json` schema (7 fields, loaded by `Engine/Engine/[Capture]/CaptureSettings.vb` on Engine-Audio) is being phased out. Phase 12 should bind to `EngineConfigV2` directly.

5. **IPC is REUSE-VERBATIM.** The TCP/5000 pattern with `TcpClientHelper` is battle-tested across 4 process types on Stable. Phase 12 spec §9's named-pipes proposal is solving a problem that doesn't exist — the existing TCP layer already supports single-client orchestration with auto-reconnect and ping. New host should register as `"NVIDIA Engine"` and reuse the `engine_record_start` / `engine_record_stop` / `engine_get_status` command set so the Overlay side needs zero changes.

6. **MuxCoordinator is the gold standard.** It's the one piece of production code that is complete, tested, and matches Phase 12's needs verbatim. Reuse it directly — only wiring change is to route `FFmpegPath` through the new `FfmpegPathResolver` instead of caller-supplied.

7. **AudioSidecar is the right shape, empty inside.** Extend it: add `NAudio.Wasapi` PackageReference to `CaptureEngine.FFmpegBackend.vbproj`, replace the stub TODOs with real `WasapiLoopbackCapture` + `WaveFileWriter` init. The QPC timestamp capture is already in place — matches what MuxCoordinator needs.

8. **Config is REUSE-EXTEND.** `EngineConfigV2` + `ConfigLoader` are comprehensive. Add `AudioWarmupSec` field (the only missing Phase 12 spec field). Bind `RecordingEngine` constructor directly to `EngineConfigV2` — don't create a parallel `RecordingEngineConfig` class (the spec's proposal fragments the source of truth).

9. **Logger is EXTEND, not replace.** Foundation `EngineLogger` is solid and used everywhere. Add a file-writing sink via the existing `sink As Action(Of String)` constructor parameter. For richer structured logging, add a `Microsoft.Extensions.Logging` adapter in a new namespace — Foundation stays untouched.

10. **D3D11DeviceFactory / DxgiDuplicationFactory / NvencEncoderFactory / FfmpegPathResolver / SessionResult are NEW.** No production analog exists. Build them as `Internal/` helpers under `CaptureEngine.Recording` namespace per Phase 12 spec §11.
