# Phase 12 — Production RecordingEngine Architecture Spec (v2)

**Status:** Draft for OWNER review (revised from v1 based on source audit)
**Predecessors:**
- v1 spec: `docs/phase-12-architecture-spec.md` (commit `de214c2`)
- Source audit: `docs/phase-12-source-audit.md` (commit `e892030`)
- Phase 11 post-mortem: `docs/phase-11-postmortem.md` (commit `de214c2`)

**Target branch:** `Engine-Rebuild-Stabilization`
**Target location:** New namespace `CaptureEngine.Recording` under existing `CaptureEngine.*` project structure

---

## 0. Revision Summary (v1 → v2)

Every revision in v2 is **driven by the source audit** — no speculation.

| § | v1 (speculative) | v2 (audit-driven) | Reason |
|---|---|---|---|
| §9 IPC | Named pipes proposal | **REUSE TCP/5000** via `TcpClientHelper.vb` + `API/Server.vb` pattern | Audit §4: TCP/5000 is already deployed, battle-tested, with exact command set Phase 12 needs |
| §8 FFmpeg path | Deployment-relative walk | **REUSE `EngineConfigV2.Runtime.FFmpegPath`** | Audit §3: `Runtime.FFmpegPath` already exists in config V2 schema |
| §11 File layout | New `CaptureEngine/Recording/` subfolder | **Integrate into existing `CaptureEngine.*` structure** — new namespace, new `CaptureEngine.Recording` project | Audit §1+§9: Stable's `CaptureEngine/` is a 270-byte placeholder; the real architecture is the multi-project structure on `Engine-Rebuild-Stabilization`. New project follows existing pattern |
| §5 API | C# declarations in `CaptureEngine.Recording` namespace | **VB.NET** declarations matching existing `CaptureEngine.*` style | Audit §2: All `CaptureEngine.*` projects are VB.NET. Phase 12 follows the convention |
| §13 Q1 IPC | Open question | **ANSWERED: TCP/5000 reuse** | Audit §4 |
| §13 Q2 engine.json | Open question | **ANSWERED: `EngineConfigV2` via `ConfigLoader.vb`** | Audit §3 |
| §13 Q3 CaptureEngine/ | Open question | **ANSWERED: Multi-project `CaptureEngine.*` on `Engine-Rebuild-Stabilization`** | Audit §1+§9 |
| §13 Q4 NVIDIA Capture.exe | Open question | **ANSWERED: NOT BUILT on Stable. Phase 12 must CREATE new host project** | Audit §1 |
| §13 Q5 NVENC structs | Open question | **ANSWERED: Copy spike's `NvEncodeAPI.cs` verbatim per HARD RULES** | Audit §8 |
| §13 Q6 Foundation | Open question | **ANSWERED: `CaptureEngine.vb` @ 82d792ab is FROZEN. New code must NOT modify it** | Audit §5 |
| — | (none) | **NEW §17: Process + crash recovery lifecycle** (OWNER request) | OWNER email |
| — | (none) | **NEW §18: Ownership boundary diagram** (OWNER request) | OWNER email |

---

## 1. Objective

Build a **production-grade `RecordingEngine`** that owns GPU resources
for the entire process lifetime and exposes session-based recording via
TCP/5000 IPC. The architecture must:

1. **Own GPU resources (D3D11 device, DXGI duplication, NVENC encoder)
   for process lifetime** — not per session. (Phase 11 root cause #2 + #3)
2. **Compose existing production code** — `MuxCoordinator.vb`,
   `FFmpegProcessHost.vb`, `AudioSidecar.vb` (extended),
   `EngineConfigV2`, `EngineLogger`, `TcpClientHelper` — instead of
   rewriting them.
3. **Add ONLY what's missing** — D3D11/DXGI/NVENC ownership (greenfield
   in production), `RecordingEngine` class, `CaptureSession` class,
   `SessionResult` DTO, host process.
4. **Preserve Foundation** — `CaptureEngine/Engine/CaptureEngine.vb`
   (frozen @ 82d792ab) is lifecycle pattern only; `RecordingEngine`
   borrows the pattern, does not modify the class.
5. **Be testable by Phase 13** — same Test A/B/C matrix as Phase 11,
   expected to PASS because resources no longer leak between sessions.

---

## 2. HARD RULES (carried from spike + audit findings)

- ✅ No changes to NVENC struct layouts (spike's `NvEncodeAPI.cs`).
- ✅ No manual WASAPI COM (use NAudio loopback).
- ✅ No Foundation changes — `CaptureEngine.vb` @ `82d792ab` is frozen.
- ✅ Phase 1-9 spike code stays in `spikes/` as evidence (copy verbatim
     where needed, do not modify spike files).
- ✅ Phase 10 spike code stays as evidence (do NOT port directly — wrong
     ownership model per Phase 11 post-mortem).
- 🆕 Phase 12 code is **VB.NET** to match existing `CaptureEngine.*`
     convention (audit §2).
- 🆕 Phase 12 creates a **new project** `CaptureEngine.Recording` +
     a **new host project** for `NVIDIA Capture.exe` (audit §1: no
     existing host on Stable).
- 🆕 Phase 12 **reuses** `MuxCoordinator.vb`, `FFmpegProcessHost.vb`,
     `EngineConfigV2.vb`, `ConfigLoader.vb`, `EngineLogger.vb`,
     `TcpClientHelper.vb` **verbatim or with thin wrappers only**.
- 🆕 Phase 12 **extends** `AudioSidecar.vb` (stub → real NAudio
     implementation). This is the only existing file Phase 12 modifies.
- 🆕 Phase 12 **binds directly to `EngineConfigV2`** — does NOT create
     a parallel `RecordingEngineConfig` class (audit recommends
     single source of truth).

---

## 3. Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│  NVIDIA Capture.exe  (NEW host project — Phase 12 creates this)   │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │  Program.vb (entry point)                                    │ │
│  │    • Parse command line                                      │ │
│  │    • Load EngineConfigV2 via ConfigLoader                   │ │
│  │    • Construct RecordingEngine (process-lifetime singleton) │ │
│  │    • Construct IpcServer (TCP/5000, reuse pattern)           │ │
│  │    • Wire IPC commands → RecordingEngine methods            │ │
│  │    • Block until shutdown signal                             │ │
│  │    • Dispose engine + IPC                                    │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌─────────────────────────┬───────────────────────────────────┐  │
│  │  RecordingEngine        │  IpcServer (NEW thin wrapper)     │  │
│  │  (process-lifetime)     │    • REUSE TcpClientHelper.vb     │  │
│  │                          │    • REUSE API/Server.vb pattern │  │
│  │  PERSISTENT (owns):      │    • Wire format:                │  │
│  │   • D3D11Device          │      [Send] NVIDIA Engine|<cmd>  │  │
│  │   • DxgiOutputDuplication│    • Commands (existing set):   │  │
│  │   • NvencEncoder         │      engine_record_start:<path> │  │
│  │   • NvencBitstreamBuffer │      engine_record_stop          │  │
│  │   • NvencRegisteredRes   │      engine_get_status           │  │
│  │   • EncoderTexture       │      engine_get_result           │  │
│  │                          │      engine_load_config          │  │
│  │  PER-SESSION (creates):  │      PREWARM_FFMPEG:<path>       │  │
│  │   • CaptureSession       │    • Response:                   │  │
│  │     └─ AudioSidecar (extended)                             │  │
│  │     └─ FFmpegProcessHost (REUSE)                           │  │
│  │     └─ MuxCoordinator (REUSE)                              │  │
│  │     └─ SessionResult (NEW DTO)                             │  │
│  │                                                          │  │
│  │  STATELESS HELPERS (new):                                │  │
│  │   • D3D11DeviceFactory (copy from spike Phase 1)         │  │
│  │   • DxgiDuplicationFactory (copy from spike Phase 2)     │  │
│  │   • NvencEncoderFactory (copy from spike Phase 4-9)      │  │
│  │   • NvEncodeAPI.vb (copy spike's NvEncodeAPI.cs verbatim)│  │
│  └─────────────────────────┴───────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
                          ▲
                          │ TCP 127.0.0.1:5000
                          │ Wire: [Send] <App>|<cmd>:<value>
                          │
        ┌─────────────────┴────────────────┐
        │  NVIDIA API.exe (existing)        │
        │  └─ Server.vb (TcpListener)       │
        │      Forwards commands between    │
        │      ShadowPlay / Notifier /     │
        │      Engine / App Experience     │
        └──────────────────────────────────┘
                          ▲
                          │ TCP (broadcast)
                          │
        ┌─────────────────┴────────────────┐
        │  NVIDIA ShadowPlay.exe           │
        │  (orchestrator — UNCHANGED)      │
        │  Sends engine_record_start/stop  │
        │  to NVIDIA Engine via API.exe    │
        └──────────────────────────────────┘
```

---

## 4. Ownership Contract

Every resource has **exactly one owner**. This is the contract.

| Resource | Owner | Lifetime | Disposal | Source |
|---|---|---|---|---|
| D3D11Device | RecordingEngine | process | RecordingEngine.Dispose() | Copy from spike Phase 1 |
| DxgiOutputDuplication | RecordingEngine | process | RecordingEngine.Dispose() | Copy from spike Phase 2 |
| NvencEncoder handle | RecordingEngine | process | RecordingEngine.Dispose() | Copy from spike Phase 4 |
| NvencBitstreamBuffer | RecordingEngine | process | RecordingEngine.Dispose() | Copy from spike Phase 4 |
| NvencRegisteredResource | RecordingEngine | process | RecordingEngine.Dispose() | Copy from spike Phase 4 |
| EncoderTexture (D3D11 BGRA8) | RecordingEngine | process | RecordingEngine.Dispose() | Copy from spike Phase 4 |
| CaptureSession | RecordingEngine | session | auto-disposed after Run() returns | NEW |
| WasapiLoopbackCapture | CaptureSession (via AudioSidecar) | session | CaptureSession.Dispose() via try/finally | EXTEND AudioSidecar.vb |
| WaveFileWriter | CaptureSession (via AudioSidecar) | session | CaptureSession.Dispose() via try/finally | EXTEND AudioSidecar.vb |
| FFmpegProcessHost (mux) | CaptureSession | session | CaptureSession.Dispose() via try/finally | REUSE `CaptureEngine.FFmpegBackend/FFmpegProcessHost.vb` |
| MuxCoordinator | CaptureSession | session | CaptureSession.Dispose() via try/finally | REUSE `CaptureEngine.FFmpegBackend/MuxCoordinator.vb` |
| Output MP4 file | filesystem | beyond session | N/A | N/A |
| temp .wav / .h264 / .m4a | filesystem | session-scoped | MuxCoordinator cleans up after successful mux (existing behavior) | REUSE MuxCoordinator |
| EngineConfigV2 | Program.vb (entry point) | process | N/A (loaded once) | REUSE `CaptureEngine/Configuration/EngineConfigV2.vb` + `ConfigLoader.vb` |
| EngineLogger | Program.vb | process | Program.vb finally block | EXTEND Foundation `EngineLogger.vb` with file sink |
| TcpClientHelper | IpcServer | process | IpcServer.Dispose() | REUSE `TcpClientHelper.vb` |
| TcpListener | IpcServer | process | IpcServer.Dispose() | REUSE pattern from `API/Server.vb` |
| NvEncodeAPI.vb (P/Invoke) | static (shared) | process | N/A | COPY verbatim from spike `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` (HARD RULES allow) |

---

## 5. Public API (VB.NET, matches existing CaptureEngine.* convention)

```vb
Imports CaptureEngine.Recording
Imports CaptureEngine.Configuration

Namespace CaptureEngine.Recording

    ''' <summary>
    ''' Process-lifetime singleton. Owns GPU resources.
    ''' Constructed once at NVIDIA Capture.exe startup; disposed at shutdown.
    ''' Reuses Foundation CaptureEngine.vb lifecycle PATTERN but is a NEW class
    ''' (Foundation is FROZEN @ 82d792ab — must not modify).
    ''' </summary>
    Public NotInheritable Class RecordingEngine
        Implements IDisposable

        Public Sub New(config As EngineConfigV2, logger As EngineLogger)

        ''' <summary>
        ''' Starts a new recording session. Throws InvalidOperationException
        ''' if a session is already running.
        ''' </summary>
        Public Function StartSession(sessionConfig As SessionConfig) As CaptureSession

        ''' <summary>
        ''' Engine status — never throws. Safe from any thread.
        ''' </summary>
        Public Function GetStatus() As EngineStatus

        Public Sub Dispose() Implements IDisposable.Dispose
    End Class

    ''' <summary>
    ''' Per-session resource owner. Reuses parent engine's GPU resources.
    ''' Dispose() is idempotent and safe from finally blocks.
    ''' </summary>
    Public NotInheritable Class CaptureSession
        Implements IDisposable

        ''' <summary>
        ''' Blocks until duration elapses or Stop() is called.
        ''' Always runs cleanup via finally — returns SessionResult.
        ''' </summary>
        Public Function Run() As SessionResult

        ''' <summary>Signals early stop. Run() returns shortly after.</summary>
        Public Sub [Stop]()

        Public Sub Dispose() Implements IDisposable.Dispose
    End Class

    Public NotInheritable Class SessionConfig
        Public Property OutputPath As String = ""
        Public Property DurationSeconds As Integer = 30
        ' Other fields derived from EngineConfigV2 at StartSession time
    End Class

    Public NotInheritable Class SessionResult
        Public ReadOnly Property OutputPath As String
        Public ReadOnly Property RequestedDurationSec As Integer
        Public ReadOnly Property ActualDurationSec As Double
        Public ReadOnly Property FramesCaptured As Long
        Public ReadOnly Property FramesEncoded As Long
        Public ReadOnly Property Drops As Long
        Public ReadOnly Property NvencErrors As Long
        Public ReadOnly Property TotalVideoBytes As Long
        Public ReadOnly Property AudioSamples As Long
        Public ReadOnly Property AudioBytes As Long
        Public ReadOnly Property VideoStreamFound As Boolean
        Public ReadOnly Property AudioStreamFound As Boolean
        Public ReadOnly Property FileExists As Boolean
        Public ReadOnly Property FileSize As Long
        Public ReadOnly Property ErrorMessage As String

        Public ReadOnly Property Pass As Boolean
    End Class

    Public Enum EngineState
        Created
        Initializing
        Idle           ' ← new state (Foundation has "Stopped" instead)
        Recording
        Stopping
        Faulted
        Disposed
    End Enum

    Public NotInheritable Class EngineStatus
        Public ReadOnly Property State As EngineState
        Public ReadOnly Property CurrentSessionId As String  ' Nothing if Idle
        Public ReadOnly Property FramesEncodedThisSession As Long
        Public ReadOnly Property AudioSamplesThisSession As Long
        Public ReadOnly Property LastSessionResult As SessionResult  ' Nothing if no session yet
        Public ReadOnly Property UptimeSec As Double
    End Class

End Namespace
```

---

## 6. Lifecycle Flow — Happy Path

```
PROCESS START (NVIDIA Capture.exe)
    │
    ▼
Program.Main(args)
    │
    ├─ Parse --config <path>  (default: engine-config.v2.json)
    ├─ ConfigLoader.Load(configPath) → EngineConfigV2
    ├─ Construct EngineLogger with file sink → Logs/capture-engine.log
    │     (REUSE Foundation EngineLogger.vb; pass file-writing Action(Of String))
    │
    ▼
engine = New RecordingEngine(config, logger)
    │     ├─ State = Created
    │     ├─ State = Initializing
    │     ├─ D3D11DeviceFactory.Create(config) → _device
    │     │     (Copy from spike Phase 1: enumerate DXGI, pick NVIDIA adapter,
    │     │      create D3D11 device with multithread protection)
    │     ├─ DxgiDuplicationFactory.Create(_device) → _duplication
    │     │     (Copy from spike Phase 2: DuplicateOutput on primary output)
    │     ├─ NvencEncoderFactory.Create(_device) → _encoder
    │     │     (Copy from spike Phase 4: OpenEncodeSessionEx + InitializeEncoder)
    │     ├─ Create bitstream buffer + register encoder texture
    │     │     (Copy from spike Phase 4: CreateBitstreamBuffer + RegisterResource)
    │     ├─ State = Idle
    │     └─ Log "RecordingEngine ready"
    │
    ▼
ipcServer = New IpcServer(engine, config, logger)
    │     ├─ Construct TcpClientHelper(appName:="NVIDIA Engine",
    │     │     host:="127.0.0.1", port:=5000, autoReconnect:=True)
    │     ├─ Connect to API.exe (must be running — wait/retry)
    │     ├─ Send "engine_ready" broadcast
    │     └─ Start listener loop
    │
    ▼
[IPC loop — listen for commands from NVIDIA ShadowPlay via API.exe]
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_record_start:<outputPath>
    │       │
    │       ▼
    │     ipcServer.HandleCommand → engine.StartSession(sessionConfig)
    │       │  ├─ Validate engine.State == Idle (else throw)
    │       │  ├─ State = Recording
    │       │  ├─ Construct CaptureSession(sessionConfig, _device, _duplication,
    │       │  │     _encoder, _bitstreamBuffer, _registeredResource,
    │       │  │     _encoderTexture, logger)
    │       │  │     ├─ Create AudioSidecar (REUSE, extended with NAudio)
    │       │  │     ├─ Create FFmpegProcessHost (REUSE) — for video pipe
    │       │  │     └─ Reserve MuxCoordinator (lazy at Stop time)
    │       │  └─ Return session handle (async — recording runs in background)
    │       │
    │       ▼
    │     Session background thread:
    │       CaptureSession.Run()
    │       ├─ Audio warmup: sleep config.Audio.Sync.AudioWarmupSec (NEW field)
    │       ├─ AudioSidecar.Start()  → NAudio WasapiLoopbackCapture → temp.wav
    │       ├─ Capture loop (Copy from spike Phase 10 RunRecording):
    │       │     while not stop and elapsed < duration:
    │       │       _duplication.AcquireNextFrame (BORROW)
    │       │       deviceCtx.CopyResource(_encoderTexture, desktopTexture)
    │       │       _encoder.MapInputResource(encoder, registeredResource)
    │       │       _encoder.EncodePicture(encoder, picParams)
    │       │       _encoder.LockBitstream(encoder, lockParams)
    │       │       write NAL bytes to .tmp.h264 file
    │       │       _encoder.UnlockBitstream(encoder, bitstreamBuffer)
    │       │       _encoder.UnmapInputResource(encoder, mappedInput)
    │       │       duplication.ReleaseFrame (release borrow)
    │       ├─ Duration elapsed or Stop() called
    │       ├─ AudioSidecar.Stop()  → flush + dispose WAV writer
    │       ├─ Close .tmp.h264 FileStream
    │       ├─ MuxCoordinator.MuxAsync()  (REUSE):
    │       │     ├─ ffprobe video duration
    │       │     ├─ Spawn mux FFmpeg: -i video -i system.wav
    │       │     │     -c:v copy -c:a aac -t <duration> output.mp4
    │       │     └─ Cleanup temp files (only on success)
    │       ├─ Verify MP4 streams (ffmpeg -i info mode — Phase 10 pattern)
    │       ├─ Populate SessionResult
    │       └─ finally: Dispose all per-session resources (idempotent)
    │       │
    │       ▼
    │     engine.State = Idle
    │     engine.LastSessionResult = result
    │     ipcServer.Send("engine_response:engine_record_start,success,req=<id>")
    │  ──► [Receive] NVIDIA ShadowPlay|engine_response:engine_record_start,success
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_record_stop
    │       │
    │       ▼
    │     ipcServer.HandleCommand → session.Stop()
    │       (signals capture loop to exit; Run() returns shortly after)
    │  ──► [Receive] ... engine_response:engine_record_stop,success
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_get_status
    │  ──► [Receive] ... engine_response:engine_get_status,success,<JSON>
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_get_result
    │  ──► [Receive] ... engine_response:engine_get_result,success,<JSON SessionResult>
    │
    ▼
[On shutdown — Ctrl+C or process termination signal]
    │
    ├─ ipcServer.Dispose()
    │     ├─ Send "engine_shutdown"
    │     └─ TcpClientHelper.Dispose()
    │
    ├─ engine.Dispose()
    │     ├─ If Recording: session.Stop(); session.Dispose()
    │     ├─ State = Disposing
    │     ├─ NvencEncoderFactory.Destroy(_encoder)
    │     │     (UnregisterResource + DestroyBitstreamBuffer + DestroyEncoder)
    │     ├─ DxgiDuplicationFactory.Destroy(_duplication)
    │     ├─ D3D11DeviceFactory.Destroy(_device)
    │     └─ State = Disposed
    │
    └─ Process exit (code 0)
```

---

## 7. Failure Path Guarantees

### 7.1 Engine construction failure

If `RecordingEngine` constructor throws partway through (e.g. NVENC load
fails), the constructor must dispose any partially-created resources
before re-throwing. Pattern (VB.NET):

```vb
Public Sub New(config As EngineConfigV2, logger As EngineLogger)
    _config = config
    _logger = logger
    _state = EngineState.Initializing

    _device = D3D11DeviceFactory.Create(config)
    Try
        _duplication = DxgiDuplicationFactory.Create(_device)
        Try
            _encoder = NvencEncoderFactory.Create(_device)
            Try
                _bitstream = NvencEncoderFactory.CreateBitstream(_encoder)
                _registeredResource = NvencEncoderFactory.RegisterTexture(_encoder, _encoderTexture)
            Catch ex As Exception
                NvencEncoderFactory.Destroy(_encoder)
                Throw
            End Try
        Catch ex As Exception
            _duplication.Dispose()
            Throw
        End Try
    Catch ex As Exception
        _device.Dispose()
        Throw
    End Try

    _state = EngineState.Idle
End Sub
```

### 7.2 Session failure (any exception during Run)

`CaptureSession.Run()` uses a single outermost `try/finally`:

```vb
Public Function Run() As SessionResult
    Dim result As SessionResult = Nothing
    Try
        result = RunUnsafe()  ' actual recording logic
    Catch ex As Exception
        _logger.Error(ex, "Session failed: " + ex.Message)
        result = SessionResult.Failure(_outputPath, _durationSec, ex.Message)
    Finally
        ' Idempotent — safe to call even if never started
        Dispose()
        _engine.OnSessionEnded(Me)
    End Try
    Return result
End Function
```

### 7.3 Engine disposal is idempotent

```vb
Public Sub Dispose() Implements IDisposable.Dispose
    If _disposed Then Return
    _disposed = True
    _state = EngineState.Disposing

    ' Stop any active session first (synchronously, with timeout)
    If _currentSession IsNot Nothing Then
        _currentSession.Stop()
        _currentSession.Dispose()
    End If

    ' Dispose in reverse construction order
    NvencEncoderFactory.Destroy(_registeredResource)
    NvencEncoderFactory.Destroy(_bitstream)
    NvencEncoderFactory.Destroy(_encoder)
    _encoderTexture?.Dispose()
    _duplication?.Dispose()
    _device?.Dispose()

    _state = EngineState.Disposed
End Sub
```

---

## 8. NEW §17 — Process & Crash Recovery Lifecycle (OWNER request)

### 17.1 Process ownership boundary

```
┌─────────────────────────────────────────────────────────────┐
│  NVIDIA Capture.exe (Phase 12 NEW host)                     │
│  ─────────────────────────────────────────                  │
│  OWNS:                                                      │
│    • RecordingEngine instance (process-lifetime)            │
│    • EngineConfigV2 instance (loaded once at startup)       │
│    • EngineLogger instance (file sink → capture-engine.log) │
│    • IpcServer instance (TCP/5000 client to API.exe)        │
│  DOES NOT OWN:                                              │
│    • API.exe (separate process, must be running)            │
│    • ShadowPlay.exe (separate process, orchestrator)        │
│    • Output files (filesystem — outlive process)            │
└─────────────────────────────────────────────────────────────┘
```

### 17.2 Happy path

```
1. NVIDIA Capture.exe starts (via ShadowPlay.exe launcher or auto-start)
2. Loads EngineConfigV2 from engine-config.v2.json
3. Constructs RecordingEngine (creates GPU resources)
4. Connects to API.exe via TCP/5000, sends "engine_ready"
5. Idle — waiting for commands

6. ShadowPlay.exe sends "engine_record_start:<path>"
   (via API.exe broadcast to NVIDIA Engine)
7. Capture.exe starts CaptureSession (reuses GPU resources)
8. Recording... (frames encoded in real-time)
9. ShadowPlay.exe sends "engine_record_stop"
10. Capture.exe finalizes MP4, returns SessionResult
11. Idle again

12. Capture.exe receives shutdown signal (Ctrl+C / service stop)
13. Disposes RecordingEngine (releases GPU)
14. Disconnects from API.exe
15. Process exits (code 0)
```

### 17.3 Crash path — Capture.exe crashes

```
1. Capture.exe crashes (unhandled exception / process kill)
   │
   ▼
2. Resources die with process:
   ├─ D3D11 device handle → OS releases
   ├─ DXGI duplication → OS releases (Windows desktop session regains)
   ├─ NVENC encoder → driver releases session slot (within ~1s)
   ├─ NAudio capture → WASAPI releases
   ├─ FFmpeg subprocess → dies (orphan risk! need JobObject guard)
   └─ Output .mp4 → filesystem (may be partial / un-finalized)
   │
   ▼
3. API.exe detects disconnect:
   ├─ TcpClientHelper raises OnDisconnected event
   ├─ Server.vb marks "NVIDIA Engine" as inactive (heartbeat timeout 30s)
   └─ Server.vb broadcasts "engine_disconnect" to other clients
   │
   ▼
4. ShadowPlay.exe receives "engine_disconnect":
   ├─ If recording was in progress → mark session as FAILED
   ├─ Notify user via Notifier (overlay): "Recording failed — engine crashed"
   └─ Schedule engine restart (via auto-start mechanism)
   │
   ▼
5. Capture.exe restarts (via OS auto-restart or ShadowPlay launcher):
   ├─ Step 1-5 of happy path
   ├─ On "engine_ready", ShadowPlay.exe marks engine as recovered
   └─ User must manually retry recording (no auto-resume of in-progress recording)
```

**Key guarantee:** A crashed Capture.exe **cannot corrupt the next session**
because all resources die with the process. NVENC session slot is freed
by the driver (~1s after process exit). DXGI duplication is released.
The next Capture.exe start gets a clean slate.

### 17.4 Crash path — API.exe crashes

```
1. API.exe (TcpListener) crashes
   │
   ▼
2. Capture.exe's TcpClientHelper detects disconnect:
   ├─ Raises OnDisconnected
   ├─ Auto-reconnect loop kicks in (1s → 30s exponential backoff)
   └─ RecordingEngine continues running (does NOT stop recording)
   │
   ▼
3. API.exe restarts:
   ├─ TcpListener back up on port 5000
   ├─ Capture.exe reconnects, re-registers as "NVIDIA Engine"
   └─ engine_ready broadcast again
   │
   ▼
4. ShadowPlay.exe (also auto-reconnects to API.exe):
   ├─ Re-establishes command path
   └─ Can query engine_get_status to verify engine is still alive
```

**Key guarantee:** RecordingEngine continues recording during API.exe
outage. The IPC layer is fire-and-forget for status queries; only
`engine_record_start` / `engine_record_stop` require a live API.exe path.

### 17.5 Crash path — ShadowPlay.exe crashes

```
1. ShadowPlay.exe (orchestrator) crashes
   │
   ▼
2. Capture.exe behavior:
   ├─ Still connected to API.exe (TcpClientHelper auto-reconnect)
   ├─ If recording in progress → continues until duration elapses or
   │   explicit engine_record_stop arrives (it won't, so runs to duration)
   ├─ SessionResult captured internally
   └─ Idle state after session ends
   │
   ▼
3. ShadowPlay.exe restarts:
   ├─ Connects to API.exe
   ├─ Sends engine_get_status → sees engine is Idle (or Recording if still going)
   └─ Can query engine_get_result to retrieve last SessionResult
```

### 17.6 Orphan FFmpeg process guard

**Phase 11 lesson:** FFmpeg subprocesses can outlive the parent if not
guarded. Production code already has `JobObjectGuard` (Engine-Audio
legacy). Phase 12 must:

1. Wrap every `FFmpegProcessHost` instance in a **Windows Job Object**
   with `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`.
2. When Capture.exe dies (even via `taskkill /F`), the Job Object dies,
   which kills the FFmpeg subprocess.
3. This is a low-level Windows mechanism — not optional.

**Implementation note:** Copy the JobObjectGuard pattern from Engine-Audio
legacy `Engine/Engine/[Capture]/CaptureEngine.vb` (referenced as
`_jobGuard As JobObjectGuard`) into `CaptureEngine.Recording.Internal`.

---

## 9. IPC Layer (REVISED from v1)

**v1 (REJECTED):** Named pipes proposal.
**v2 (AUDIT-DRIVEN):** Reuse existing TCP/5000 pattern.

### Existing infrastructure (REUSE VERBATIM)

| Component | Path | Action |
|---|---|---|
| `TcpClientHelper.vb` | Duplicated in `App Experience/` and `Notifier/` (identical 6198 bytes) | Consolidate into a new shared location OR copy into new host project |
| `API/Server.vb` | `API/[Forms - Project Files]/[API]/Server.vb` (10428 bytes) | NOT modified — already deployed, handles routing |
| Wire format | `[Send] <AppName>|<cmd>:<value>` | KEEP — minimize Overlay-side changes |
| Existing command set | `engine_record_start:<path>`, `engine_record_stop`, `engine_get_status`, `engine_load_config`, `engine_set_encoder:<value>`, `PREWARM_FFMPEG:<path>`, `engine_config_changed` | KEEP — ShadowPlay.exe already sends these |

### New component: `CaptureEngine.Recording.Ipc.IpcServer`

Thin wrapper that:
1. Constructs `TcpClientHelper(appName:="NVIDIA Engine")` (or similar name).
2. Wires `OnMessageReceived` to a command dispatcher.
3. Dispatcher routes commands to `RecordingEngine` methods:
   - `engine_record_start:<path>` → `engine.StartSession(...)` (async, returns immediately)
   - `engine_record_stop` → `engine.CurrentSession.Stop()`
   - `engine_get_status` → returns `engine.GetStatus()` as JSON
   - `engine_get_result` → returns `engine.LastSessionResult` as JSON
   - `engine_load_config` → reload `EngineConfigV2` from disk (hot reload)
   - `PREWARM_FFMPEG:<path>` → no-op for now (or future pre-warming)
4. Sends `engine_response:<command>,<status>[,<data>][,req=<reqId>]` back.

### JSON sub-protocol for richer payloads

Existing wire format is pipe-delimited key:value. For `engine_get_status`
and `engine_get_result`, the `<value>` field will contain a JSON-encoded
payload (URL-encoded if needed to avoid pipe-character collisions).

Example:
```
[Send] NVIDIA Engine|engine_get_status
[Receive] NVIDIA Engine|engine_response:engine_get_status,success,{"state":"Recording","sessionId":"abc123","framesEncoded":1234,"audioSamples":56789}
```

**Decision required from OWNER:** keep existing pipe-delimited format with
JSON-in-value, OR introduce a new line-delimited JSON protocol? v2
recommends **keep existing** to minimize Overlay-side changes.

---

## 10. FFmpeg Path Resolution (REVISED from v1)

**v1 (REJECTED):** Deployment-relative walk (`<exeDir>/API-Core/ffmpeg.exe`).
**v2 (AUDIT-DRIVEN):** Reuse `EngineConfigV2.Runtime.FFmpegPath`.

### Existing config field

```vb
' EngineConfigV2.vb (existing, audit §3)
Public Class RuntimeSubSection
    Public Property FFmpegPath As String = ""       ' ← USE THIS
    Public Property FFprobePath As String = ""      ' ← USE THIS for MuxCoordinator
    Public Property ProcessPriority As String = "Normal"
    ' ...
End Class
```

### Validation helper (NEW, small)

```vb
Namespace CaptureEngine.Recording.Internal

    Public NotInheritable Class FfmpegPathResolver

        ''' <summary>
        ''' Resolves FFmpeg path from EngineConfigV2.Runtime.FFmpegPath.
        ''' Throws FileNotFoundException if not configured or file missing.
        ''' </summary>
        Public Shared Function Resolve(config As EngineConfigV2) As String
            Dim path As String = config.Runtime.FFmpegPath

            If String.IsNullOrWhiteSpace(path) Then
                ' Fallback: deployment-relative (covers dev scenarios where
                ' config is not yet populated)
                Dim exeDir As String = AppContext.BaseDirectory
                Dim candidates As String() = {
                    Path.Combine(exeDir, "API-Core", "ffmpeg.exe"),
                    Path.Combine(exeDir, "ffmpeg.exe")
                }
                For Each c As String In candidates
                    If File.Exists(c) Then Return Path.GetFullPath(c)
                Next
                Throw New FileNotFoundException(
                    "ffmpeg.exe not found. Configure Runtime.FFmpegPath in engine-config.v2.json " +
                    "or place ffmpeg.exe at <app>/API-Core/ffmpeg.exe")
            End If

            If Not File.Exists(path) Then
                Throw New FileNotFoundException(
                    $"FFmpeg not found at configured path: {path}")
            End If

            Return path
        End Function

    End Class

End Namespace
```

**Key change from v1:** v1 walked PATH and fell back to literal `"ffmpeg"`.
v2 reads `Runtime.FFmpegPath` first (config is source of truth), only
falls back to deployment-relative if config is empty. Throws if neither
works (no silent failures — Phase 11 lesson).

---

## 11. File Layout (REVISED from v1)

**v1 (REJECTED):** New `CaptureEngine/Recording/` subfolder inside existing
single-project `CaptureEngine/`.
**v2 (AUDIT-DRIVEN):** New project `CaptureEngine.Recording` following the
multi-project pattern on `Engine-Rebuild-Stabilization`.

### Existing project structure on `Engine-Rebuild-Stabilization` (audit §9)

```
CaptureEngine/                              ← Foundation project (frozen)
├── Engine/
│   ├── CaptureEngine.vb                   ← FROZEN @ 82d792ab
│   └── EngineState.vb
├── Configuration/
│   ├── EngineConfigV2.vb                   ← REUSE
│   ├── ConfigLoader.vb                     ← REUSE
│   ├── ConfigMigrator.vb                   ← REUSE
│   └── ConfigValidator.vb                  ← REUSE
├── Diagnostics/
│   └── EngineLogger.vb                     ← REUSE (extend with file sink)
├── FFmpeg/
│   ├── IFFmpegCommandBuilder.vb            ← REUSE
│   ├── FFmpegCommandBuilderV1.vb            ← REUSE
│   └── FFmpegCommandBuilderV2.vb            ← REUSE
├── Pipeline/
│   ├── PipelineConfig.vb                   ← REUSE
│   └── PipelineResolver.vb                 ← REUSE
└── Backends/
    └── IVideoBackend.vb                     ← exists, no concrete impl

CaptureEngine.Video/                        ← Foundation video contract
├── Contract/
│   ├── IVideoCaptureBackend.vb             ← FROZEN (PUSH model)
│   ├── IVideoFrameSink.vb                  ← FROZEN
│   └── IVideoBackendDiagnostics.vb          ← FROZEN
└── ...

CaptureEngine.Video.Ddagrab/                ← SKELETON backend
└── DdagrabBackend.vb                       ← skeleton (NoFrame forever)

CaptureEngine.FFmpegBackend/                ← FFmpeg subprocess orchestrator
├── FFmpegPipelineBackend.vb                ← REUSE pattern (per-session shape)
├── FFmpegProcessHost.vb                    ← REUSE VERBATIM
├── FFmpegStderrParser.vb                   ← REUSE VERBATIM
├── AudioSidecar.vb                         ← EXTEND (stub → real NAudio)
└── MuxCoordinator.vb                       ← REUSE VERBATIM
```

### NEW Phase 12 structure

```
CaptureEngine.Recording/                    ← NEW PROJECT (Phase 12)
├── CaptureEngine.Recording.vbproj          ← net8.0-windows, library
│                                            (references CaptureEngine, CaptureEngine.FFmpegBackend,
│                                             NAudio.Wasapi 2.3.0, Vortice.Windows 3.x)
├── RecordingEngine.vb                      ← process-lifetime singleton
├── CaptureSession.vb                       ← per-session owner
├── SessionConfig.vb                        ← DTO
├── SessionResult.vb                        ← DTO
├── EngineStatus.vb                         ← DTO
├── EngineState.vb                          ← enum (separate from Foundation's EngineState)
├── Ipc/
│   └── IpcServer.vb                        ← TCP/5000 wrapper (REUSE TcpClientHelper)
└── Internal/
    ├── D3D11DeviceFactory.vb               ← Copy from spike Phase 1
    ├── DxgiDuplicationFactory.vb           ← Copy from spike Phase 2
    ├── NvencEncoderFactory.vb               ← Copy from spike Phase 4 (OpenEncodeSessionEx + InitializeEncoder)
    ├── NvencEncoderResources.vb            ← Copy from spike Phase 4 (BitstreamBuffer + RegisterResource)
    ├── NvEncodeAPI.vb                      ← Copy spike's NvEncodeAPI.cs verbatim (HARD RULES allow)
    ├── CaptureLoop.vb                      ← Copy from spike Phase 10 RunRecording (capture+encode loop)
    ├── FfmpegPathResolver.vb               ← NEW (small, §10)
    ├── FileLoggerSink.vb                   ← NEW (small, writes to Logs/capture-engine.log)
    └── JobObjectGuard.vb                   ← Copy from Engine-Audio legacy (orphan FFmpeg protection)

CaptureEngine.Recording.Host/               ← NEW PROJECT (Phase 12) — produces NVIDIA Capture.exe
├── CaptureEngine.Recording.Host.vbproj     ← net8.0-windows, OutputType=WinExe
│                                            (references CaptureEngine.Recording, CaptureEngine)
├── Program.vb                              ← Entry point: load config, construct engine, wire IPC, block
├── My Project/
│   └── ApplicationEvents.vb               ← Shutdown handler
└── App.config                              ← (optional) runtime config
```

### Solution file update

Add both new projects to `Overlay/NVIDIA Overlay.sln` (existing master
solution). Do NOT modify existing projects' references — only add new ones.

---

## 12. Audio Implementation (EXTEND AudioSidecar.vb)

### Current state (audit §7)

```vb
' CaptureEngine.FFmpegBackend/AudioSidecar.vb (existing STUB)
Public NotInheritable Class AudioSidecar
    Implements IDisposable

    ' Properties: TempSystemWavPath, TempMicWavPath, SystemAudioEnabled, MicEnabled
    ' QPC timestamps: SystemStartTicks, MicStartTicks (already populated on Start)

    Public Sub Start()
        ' TODO (future task): initialize NAudio WasapiLoopbackCapture +
        ' WaveFileWriter for each enabled track. Write to temp .wav files.
    End Sub

    Public Sub [Stop]()
        ' TODO
    End Sub

    Public ReadOnly Property HasAudioData As Boolean
        Get
            Return False  ' always False (stub)
        End Get
    End Property
End Class
```

### Phase 12 extension

1. **Add PackageReference** to `CaptureEngine.FFmpegBackend.vbproj`:
   ```xml
   <PackageReference Include="NAudio.Wasapi" Version="2.3.0" />
   ```

2. **Replace stub TODOs** in `AudioSidecar.vb` with real implementation:
   - `Start()` creates `WasapiLoopbackCapture` + `WaveFileWriter` per
     enabled track (system + mic).
   - `DataAvailable` handler writes to WAV writer + tracks sample/byte
     counts (mirror spike Phase 10 `AudioCaptureLoop`).
   - `Stop()` signals stop, joins thread, flushes + disposes writer.
   - `HasAudioData` returns True when any track has `TotalSamples > 0`.
   - On exception: log + mark track as failed (do NOT rethrow — recording
     should continue with video-only if audio fails).

3. **Preserve existing QPC timestamp capture** — `SystemStartTicks` and
   `MicStartTicks` are already populated on Start. MuxCoordinator uses
   these for A/V sync offsets.

4. **Add `DurationSeconds` property** to `AudioSidecar` so the wait
   timeout in `Stop()` is parameterized (Phase 10 lesson: static field
   broke multi-session).

---

## 13. Encoder Implementation (NEW — greenfield)

### No existing abstraction (audit §8)

Production has zero NVENC code outside the spike. Phase 12 must create:

1. **`NvEncodeAPI.vb`** — copy spike's
   `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` **verbatim** (HARD RULES
   allow). Translate C# → VB.NET syntax only; struct layouts unchanged.

2. **`NvencEncoderFactory.vb`** — wraps `OpenEncodeSessionEx` +
   `InitializeEncoder`. Copy logic from spike Phase 4.

3. **`NvencEncoderResources.vb`** — wraps `CreateBitstreamBuffer` +
   `RegisterResource`. Copy logic from spike Phase 4.

4. **`NvencEncoderFactory.Destroy(...)`** — wraps `UnregisterResource` +
   `DestroyBitstreamBuffer` + `DestroyEncoder`. Mirror spike Phase 10
   cleanup pattern (try/catch each, log failures).

### No `IEncoder` interface (intentional)

The audit recommends AGAINST creating a generic `IEncoder` interface
at this stage. Reasons:
- Phase 12 supports only H.264 NVENC.
- An interface without multiple implementations is premature abstraction.
- If HEVC/AV1 support is added later (Phase 14+), an interface can be
  extracted at that time with full knowledge of the requirements.

**OWNER decision required:** confirm this pragmatic approach, OR
request an `IEncoder` interface upfront for future-proofing.

---

## 14. Config Changes (minimal)

### Add `AudioWarmupSec` to `EngineConfigV2`

```vb
' EngineConfigV2.vb (existing) — Audio.Sync subsection
Public Class AudioSyncSubSection
    Public Property MaxOffsetSec As Double = 0
    Public Property MinOffsetSec As Double = 0
    Public Property AVSyncToleranceMs As Integer = 50
    Public Property AudioWarmupSec As Integer = 2   ' ← NEW (Phase 12)
End Class
```

### That's it — no other config changes

The audit confirms `Runtime.FFmpegPath`, `Runtime.FFprobePath`,
`Output.Directory`, `Runtime.ShutdownTimeout` (FFmpegQuit/MuxWait/FFprobeWait),
`Audio.System` / `Audio.Microphone` all already exist. No new fields.

---

## 15. Test Plan (Phase 13 — placeholder, will be detailed later)

Phase 13 will re-run the same Test A/B/C matrix as Phase 11, but against
the new `RecordingEngine` via IPC. Expected outcome:

| Test | Phase 11 (spike) | Phase 13 expected (production) |
|---|---|---|
| A1-A3 (3 × 30s) | FAIL | PASS (engine reuses GPU; session only manages audio+FFmpeg) |
| B1-B3 (1s/5s/10s) | FAIL | PASS (early Stop() works; session.Dispose() cleans up) |
| C1-C5 (5 × immediate restart) | FAIL | PASS (encoder stays open; session restart is fast) |
| Memory growth | +6140% | < +20% (only per-session buffers cycle) |
| FFmpeg orphans | 0 (never started) | 0 (JobObjectGuard kills them on parent exit) |
| Crash recovery | N/A (spike) | Engine restart yields clean state (Phase 12 §17.3) |

---

## 16. Definition of Done (Phase 12)

Phase 12 = §1-§14 implemented. Phase 13 = run the test matrix.

### Phase 12a (library + console driver — no IPC)

- [ ] `CaptureEngine.Recording/` project created with files in §11
- [ ] `CaptureEngine.Recording.vbproj` references correct packages
- [ ] `RecordingEngine` constructs and disposes cleanly (no leaks in 100 cycles)
- [ ] `CaptureSession.Run()` returns `SessionResult` for normal 30s case
- [ ] `CaptureSession.Dispose()` is idempotent (calling twice does not throw)
- [ ] `AudioSidecar.vb` extended with real NAudio implementation
- [ ] `NvEncodeAPI.vb` copied verbatim from spike (translated to VB syntax)
- [ ] `FfmpegPathResolver` reads `Runtime.FFmpegPath` from config
- [ ] `JobObjectGuard` wraps every `FFmpegProcessHost` instance
- [ ] Structured logging to `Logs/capture-engine.log` via file sink
- [ ] Unit tests: construction, disposal, idempotent-dispose, path resolution
- [ ] Console driver validates one full 30s recording end-to-end

### Phase 12b (IPC layer + host process)

- [ ] `CaptureEngine.Recording.Host/` project created (produces NVIDIA Capture.exe)
- [ ] `IpcServer` wires `TcpClientHelper` to command dispatcher
- [ ] Process registers as `"NVIDIA Engine"` with API.exe
- [ ] `engine_record_start` / `engine_record_stop` / `engine_get_status` /
      `engine_get_result` commands work end-to-end via TCP/5000
- [ ] `Ctrl+C` triggers clean shutdown (Dispose engine → Disconnect IPC → Exit)
- [ ] Crash path: `taskkill /F` leaves no orphan FFmpeg (JobObjectGuard)

### NOT in Phase 12 (deferred)

- Multi-output capture
- HEVC / AV1 codec support
- Highlight / instant replay
- Streamed output (RTMP/SRT)
- Multi-GPU selection

---

## 17. Open Questions for OWNER (revised)

v1 had 6 questions. v2 answers all of them via the audit. Remaining
questions for OWNER:

1. **`IEncoder` interface upfront, or defer?** §13 recommends defer
   (only H.264 NVENC in Phase 12; extract interface when 2nd codec added).
   Confirm or override.

2. **JSON-in-value protocol, or new line-delimited JSON protocol?** §9
   recommends keep existing pipe-delimited format with JSON-encoded
   payloads in the value field (minimizes Overlay-side changes). Confirm
   or override.

3. **App name for new host:** `"NVIDIA Engine"` (matches legacy Engine-Audio
   `[Engine] Client.vb`), or new name like `"NVIDIA Capture"`? §9 suggests
   `"NVIDIA Engine"` to match existing Overlay-side command routing.

4. **Solution file update:** add new projects to `Overlay/NVIDIA Overlay.sln`
   (existing master)? Or create a separate `Recording.sln` for Phase 12?
   §11 recommends add to existing (single source of truth for build).

5. **`engine-config.v2.json` location:** keep at
   `Overlay/bin/Release/net8.0-windows10.0.26100.0/engine-config.v2.json`
   (current), or move to a per-process location since NVIDIA Capture.exe
   is a separate process? Recommend keep co-located (single config for
   the whole Overlay suite).

6. **Auto-restart of Capture.exe on crash:** handled by OS service config
   (out of scope), or by ShadowPlay.exe launcher logic (in scope for
   Phase 12)? §17.3 mentions "auto-restart mechanism" but doesn't
   specify owner. Recommend ShadowPlay.exe owns this — confirm.

7. **Phase 12a vs 12b split:** §16 proposes splitting Phase 12 into
   12a (library + console driver) and 12b (IPC + host). Confirm this
   split is OK, or request a single combined Phase 12.

---

## 18. Recommendation

Implement Phase 12a first (RecordingEngine + CaptureSession library,
exercised via minimal console driver). This validates the **ownership
model** is correct without IPC complications. Once 12a passes a single
end-to-end 30s recording with no leaks, proceed to 12b (IPC layer +
NVIDIA Capture.exe host).

This separation is critical: the Phase 11 failure was an ownership
model problem, not an IPC problem. Solve ownership first (12a), then
add orchestration (12b).
