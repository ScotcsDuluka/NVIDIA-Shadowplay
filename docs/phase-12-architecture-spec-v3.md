# Phase 12 — Production RecordingEngine Architecture Spec (v3)

**Status:** Draft for OWNER review (v3 — audit-driven + contract-driven)
**Predecessors:**
- v1 spec: `docs/phase-12-architecture-spec.md` (commit `de214c2`) — speculative
- v2 spec: `docs/phase-12-architecture-spec-v2.md` (commit `a44cffe`) — audit-corrected, but missed `CaptureEngine.Encoder` contract existence
- Source audit: `docs/phase-12-source-audit.md` (commit `e892030`) — incomplete on encoder contract

**Target branch:** `Engine-Rebuild-Stabilization`
**OWNER decision:** Option E — implement `IEncoderBackend` contract + reuse existing infrastructure

---

## 0. Revision Summary (v2 → v3)

The audit (`e892030`) missed a critical finding: production **already has a full encoder contract** (`CaptureEngine.Encoder.IEncoderBackend` with 8-state lifecycle, `EncodedPacket`, `EncoderConfig`, `IEncoderDiagnostics`, `FakeEncoderBackend` reference implementation). v3 corrects v2's "greenfield RecordingEngine owns everything" approach with a contract-driven composition model.

| § | v2 (audit-corrected but missed encoder contract) | v3 (contract-driven, OWNER-approved) | Reason |
|---|---|---|---|
| Architecture | `RecordingEngine` owns D3D11 + DXGI + NVENC at process lifetime | `RecordingEngine` is **orchestrator only** — composes `IVideoCaptureBackend` + `IEncoderBackend` instances whose `Initialize()` happens once per process | `IEncoderBackend` contract has Initialize (process-lifetime) + Start/Stop (session) model already |
| New project | `CaptureEngine.Recording` (greenfield ownership layer) | `CaptureEngine.Encoder.Nvenc` (new) + `CaptureEngine.Recording` (orchestration only — minimal) | Match existing project boundaries |
| NVENC ownership | RecordingEngine owns NvencEncoder handle | `NvencEncoderBackend` implements `IEncoderBackend`; `Initialize()` opens NVENC session; `Dispose()` destroys | Contract designed for this |
| DXGI ownership | RecordingEngine owns DxgiOutputDuplication | `DdagrabBackend` (existing skeleton) extended with real DXGI duplication at `Initialize()` | Contract designed for this |
| JobObjectGuard | Write new in `CaptureEngine.Recording.Internal` | **REUSE** existing `Engine/Engine/[Infrastructure]/JobObjectGuard.vb` | Audit missed this file exists |
| AudioFileWriter | Replace stub `AudioSidecar.vb` with NAudio | **REUSE** existing `Engine/Engine/[Audio]/AudioFileWriter.vb` (37 KB, complete WASAPI implementation) | Audit said "NOT FOUND" — incorrect, file exists on Engine-Rebuild-Stabilization |
| TcpClientHelper | Copy into new IPC class | **REUSE** existing `Engine/Engine/[API]/TcpClientHelper.vb` + `[Engine] Client.vb` | Already wired into `NVIDIA Capture.vbproj` via source files |
| `NVIDIA Capture.vbproj` | New host project (Phase 12 creates) | **UPDATE EXISTING** `Engine/NVIDIA Capture.vbproj` — add ProjectReferences to `CaptureEngine.*` new projects | Project exists on branch, currently standalone legacy |
| Phase 12a scope | RecordingEngine + D3D11/DXGI/NVENC factories + CaptureSession + DTOs + AudioSidecar extension + Console driver | `NvencEncoderBackend` (implements IEncoderBackend) + `DdagrabBackend` real DXGI fill-in + `RecordingEngine` orchestrator + `CaptureSession` + Console driver | Smaller, contract-driven |
| Hard rule | ห้ามสร้าง `CaptureEngine.Recording` ถ้าซ้ำกับ contract ที่มี | Confirmed — `CaptureEngine.Recording` is orchestration only, no GPU/encoder code | OWNER request |

---

## 1. Objective

Build Phase 12 by **composing existing contracts** (`IVideoCaptureBackend`, `IEncoderBackend`, `MuxCoordinator`, `AudioFileWriter`, `JobObjectGuard`, `TcpClientHelper`) with **two new implementations** (`NvencEncoderBackend`, real DXGI fill-in for `DdagrabBackend`) and **one new orchestrator** (`RecordingEngine`). The architecture must:

1. **Use the existing contract lifecycle model** — `Initialize()` = process-lifetime (opens GPU resources), `Start()/Stop()` = per-session, `Dispose()` = process exit. This solves Phase 11 root causes #2 (DXGI) and #3 (NVENC) without inventing a new ownership model.

2. **Not duplicate existing code** — `JobObjectGuard`, `AudioFileWriter`, `TcpClientHelper`, `MuxCoordinator`, `FFmpegProcessHost`, `EngineConfigV2`, `EngineLogger` are all reused verbatim.

3. **Add only what's missing**:
   - `CaptureEngine.Encoder.Nvenc` — new project, implements `IEncoderBackend` with spike's NVENC ABI
   - Real DXGI fill-in for `DdagrabBackend` (currently skeleton emitting NoFrame)
   - `CaptureEngine.Recording` — new project, **orchestration only** (composes backends, no GPU resource ownership)
   - Update `NVIDIA Capture.vbproj` to reference new projects

4. **Preserve Foundation** — `CaptureEngine/Engine/CaptureEngine.vb` (frozen @ `82d792ab`) is untouched. The new `RecordingEngine` is a separate class in a separate namespace.

5. **Phase 13 testable** — same Test A/B/C matrix as Phase 11, expected to PASS because contracts guarantee Initialize-once / Start-Stop-many.

---

## 2. HARD RULES (v3 — final)

- ✅ No changes to NVENC struct layouts (spike's `NvEncodeAPI.cs` translated to VB verbatim).
- ✅ No manual WASAPI COM — **reuse `AudioFileWriter.vb`** which already handles WASAPI.
- ✅ No Foundation changes — `CaptureEngine.vb` @ `82d792ab` frozen.
- ✅ Phase 1-10 spike code stays in `spikes/` as evidence.
- ✅ Phase 12 code is **VB.NET** to match `CaptureEngine.*` convention.
- ✅ **REUSE existing contracts** — `IVideoCaptureBackend`, `IEncoderBackend`, `EncodedPacket`, `EncoderConfig`, `IEncoderDiagnostics`, `EngineConfigV2`, `EngineLogger`, `JobObjectGuard`, `AudioFileWriter`, `TcpClientHelper`, `MuxCoordinator`, `FFmpegProcessHost`, `FFmpegStderrParser`, `IFFmpegCommandBuilder`, `PipelineResolver`, `ConfigLoader`.
- 🆕 Phase 12 creates 3 things only:
  1. **`CaptureEngine.Encoder.Nvenc` project** — new project, implements `IEncoderBackend`
  2. **Real DXGI fill-in for `DdagrabBackend`** — extends existing skeleton (in `CaptureEngine.Video.Ddagrab`)
  3. **`CaptureEngine.Recording` project** — new project, **orchestration only** (composes backends per session)
- 🆕 Phase 12 updates 1 existing project:
  - `Engine/NVIDIA Capture.vbproj` — add ProjectReferences to `CaptureEngine.*` (currently has zero ProjectReferences)
- 🆕 Phase 12 adds 1 config field:
  - `Audio.Sync.AudioWarmupSec` in `EngineConfigV2.vb` (only missing field)

---

## 3. Architecture (v3 — contract composition)

```
┌─────────────────────────────────────────────────────────────────────┐
│  NVIDIA Capture.exe  (Engine/NVIDIA Capture.vbproj — EXISTING)      │
│                                                                     │
│  Currently: standalone legacy (WinForms + NAudio + Engine/Engine/) │
│  Phase 12: ADD ProjectReferences to new CaptureEngine.* projects    │
│                                                                     │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  Program / UI_Engine  (existing entry point — modified)       │ │
│  │    • Load EngineConfigV2 via ConfigLoader                      │ │
│  │    • Construct RecordingEngine (NEW orchestrator)              │ │
│  │    • Construct IpcClient (REUSE [Engine] Client.vb)            │ │
│  │    • Wire IPC commands → RecordingEngine methods              │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  RecordingEngine  (NEW — CaptureEngine.Recording project)     │ │
│  │  ORCHESTRATION ONLY — owns no GPU resources directly           │ │
│  │                                                                 │ │
│  │  PERSISTENT (created at process start, disposed at exit):     │ │
│  │   • IVideoCaptureBackend _capture  (DdagrabBackend instance)   │ │
│  │     └── Initialize() opens DXGI duplication ONCE              │ │
│  │   • IEncoderBackend _encoder  (NvencEncoderBackend instance)   │ │
│  │     └── Initialize() opens NVENC session ONCE                 │ │
│  │   • EngineLogger _logger  (REUSE Foundation)                   │ │
│  │   • EngineConfigV2 _config  (REUSE)                             │ │
│  │   • JobObjectGuard _jobGuard  (REUSE — wraps all FFmpeg)       │ │
│  │                                                                 │ │
│  │  PER-SESSION (StartSession creates, StopSession disposes):    │ │
│  │   • CaptureSession                                              │ │
│  │     ├─ AudioFileWriter (REUSE — WASAPI + temp .wav)            │ │
│  │     ├─ FFmpegProcessHost (REUSE — video pipe subprocess)       │ │
│  │     ├─ MuxCoordinator (REUSE — final mux)                      │ │
│  │     └─ SessionResult (NEW DTO)                                  │ │
│  │                                                                 │ │
│  │  LIFECYCLE:                                                     │ │
│  │   Process start:                                                │ │
│  │     _capture.Initialize()  → opens DXGI duplication             │ │
│  │     _encoder.Initialize()  → opens NVENC session                │ │
│  │   Per session:                                                  │ │
│  │     _capture.Start(sink)   → begins frame delivery              │ │
│  │     _encoder.Start()       → ready to encode                    │ │
│  │     [loop]: sink.Take() → _encoder.Encode(frame, packet)        │ │
│  │              → write packet to .tmp.h264                        │ │
│  │     _encoder.Stop()        → flush                              │ │
│  │     _capture.Stop()        → stop frame delivery                │ │
│  │     audio + mux (REUSE existing)                                │ │
│  │   Process exit:                                                 │ │
│  │     _capture.Dispose()     → releases DXGI duplication          │ │
│  │     _encoder.Dispose()     → destroys NVENC encoder            │ │
│  └────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  EXISTING CONTRACTS (REUSE — frozen)                                │
├─────────────────────────────────────────────────────────────────────┤
│  CaptureEngine.Video.IVideoCaptureBackend                          │
│    └── Initialize(ctx) / Start(sink) / Stop() / Dispose()            │
│                                                                     │
│  CaptureEngine.Encoder.IEncoderBackend                             │
│    └── Initialize(cfg) / Start() / Encode(frame, packet) /          │
│        Flush(sink) / Stop() / Dispose()                              │
│                                                                     │
│  CaptureEngine.Encoder.EncodedPacket  (caller-owned packet)         │
│  CaptureEngine.Encoder.EncoderConfig  (encoder config DTO)          │
│  CaptureEngine.Encoder.EncoderState   (8-state enum)                │
│  CaptureEngine.Encoder.IEncoderDiagnostics                          │
│                                                                     │
│  CaptureEngine.FFmpegBackend.FFmpegProcessHost  (subprocess mgmt)  │
│  CaptureEngine.FFmpegBackend.MuxCoordinator     (final mux)        │
│  CaptureEngine.FFmpegBackend.FFmpegStderrParser                    │
│                                                                     │
│  CaptureEngine.Configuration.EngineConfigV2                        │
│  CaptureEngine.Configuration.ConfigLoader                           │
│  CaptureEngine.Diagnostics.EngineLogger                             │
│  CaptureEngine.Pipeline.PipelineResolver                            │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  EXISTING IMPLEMENTATIONS (REUSE verbatim where applicable)         │
├─────────────────────────────────────────────────────────────────────┤
│  CaptureEngine.Video.Ddagrab.DdagrabBackend                        │
│    Currently skeleton (emits NoFrame forever)                       │
│    Phase 12: EXTEND with real DXGI Output Duplication              │
│                                                                     │
│  CaptureEngine.Encoder.Backends.Fake.FakeEncoderBackend             │
│    Reference impl — DO NOT use in production (test only)            │
│                                                                     │
│  Engine/Engine/[Infrastructure]/JobObjectGuard.vb                   │
│    Reuse verbatim — assigns FFmpeg subprocesses to Win32 Job Object │
│                                                                     │
│  Engine/Engine/[Audio]/AudioFileWriter.vb  (37 KB, complete WASAPI) │
│    Reuse verbatim — replaces AudioSidecar stub entirely             │
│                                                                     │
│  Engine/Engine/[API]/TcpClientHelper.vb                             │
│  Engine/Engine/[API]/[Engine] Client.vb                             │
│    Reuse verbatim — TCP/5000 IPC with engine_* command set          │
│                                                                     │
│  Engine/Engine/[Capture]/CaptureEngine.vb  (1020 lines, legacy)    │
│    Legacy orchestrator — Phase 12 does NOT modify this              │
│    (RecordingEngine in CaptureEngine.Recording supersedes it)       │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  NEW (Phase 12 creates)                                             │
├─────────────────────────────────────────────────────────────────────┤
│  CaptureEngine.Encoder.Nvenc/                   (NEW project)       │
│    ├── CaptureEngine.Encoder.Nvenc.vbproj                          │
│    │     ProjectReferences: CaptureEngine.Encoder, CaptureEngine,   │
│    │                            CaptureEngine.Video                  │
│    │     PackageReferences: Vortice.Windows (D3D11+DXGI)            │
│    ├── NvencEncoderBackend.vb                                       │
│    │     Implements IEncoderBackend                                  │
│    │     Initialize() → OpenEncodeSessionEx + InitializeEncoder     │
│    │     Start() → ready to accept Encode()                          │
│    │     Encode() → MapInputResource + EncodePicture + LockBitstream │
│    │     Stop() → flush + UnmapInputResource                         │
│    │     Dispose() → DestroyEncoder + UnregisterResource            │
│    └── Internal/                                                    │
│       ├── NvEncodeAPI.vb   (COPY spike's NvEncodeAPI.cs verbatim)   │
│       ├── NvEncFunctionTable.vb  (function pointer table + loader)  │
│       ├── NvencResources.vb  (bitstream buffer + registered texture) │
│       └── D3D11DeviceFactory.vb  (device creation — shared w/ capture)│
│                                                                     │
│  CaptureEngine.Video.Ddagrab/                   (EXISTING — extend) │
│    └── DdagrabBackend.vb                                            │
│       Phase 12: replace NoFrame worker with real AcquireNextFrame  │
│       Initialize() → DuplicateOutput (persistent, ONCE per process) │
│       Start() → spawn worker thread that calls AcquireNextFrame     │
│       Stop() → signal worker, join                                  │
│       Dispose() → release duplication                                │
│                                                                     │
│  CaptureEngine.Recording/                       (NEW project)        │
│    ├── CaptureEngine.Recording.vbproj                              │
│    │     ProjectReferences: CaptureEngine, CaptureEngine.Video,     │
│    │       CaptureEngine.Video.Ddagrab, CaptureEngine.Encoder,      │
│    │       CaptureEngine.Encoder.Nvenc, CaptureEngine.FFmpegBackend │
│    │     PackageReferences: (none — uses transitive deps)           │
│    ├── RecordingEngine.vb                                          │
│    │     Process-lifetime orchestrator (Initialize backends once)   │
│    ├── CaptureSession.vb                                           │
│    │     Per-session: composes borrowed backends + per-session      │
│    │     AudioFileWriter + FFmpegProcessHost + MuxCoordinator      │
│    ├── SessionConfig.vb  (DTO)                                     │
│    ├── SessionResult.vb  (DTO)                                     │
│    ├── EngineStatus.vb  (DTO)                                      │
│    └── EngineState.vb  (enum — distinct from Foundation's)         │
│                                                                     │
│  CaptureEngine.Recording.ConsoleDriver/        (NEW — test only)   │
│    ├── CaptureEngine.Recording.ConsoleDriver.vbproj                │
│    │     OutputType=Exe, references CaptureEngine.Recording         │
│    └── Program.vb                                                  │
│       Minimal CLI: Start → Stop → 2nd Start → Stop                  │
│       Validates lifecycle (Phase 12a minimum runtime gate)          │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 4. Ownership Contract (v3 — contract-driven)

| Resource | Owner | Lifetime | Disposal | Source |
|---|---|---|---|---|
| D3D11Device | `DdagrabBackend` (via Initialize) | process | DdagrabBackend.Dispose() | NEW (fill-in) — wraps spike Phase 1 logic |
| DxgiOutputDuplication | `DdagrabBackend` (via Initialize) | process | DdagrabBackend.Dispose() | NEW (fill-in) — wraps spike Phase 2 logic |
| NvencEncoder handle | `NvencEncoderBackend` (via Initialize) | process | NvencEncoderBackend.Dispose() | NEW — wraps spike Phase 4 logic |
| NvencBitstreamBuffer | `NvencEncoderBackend` | process | NvencEncoderBackend.Dispose() | NEW — wraps spike Phase 4 logic |
| NvencRegisteredResource | `NvencEncoderBackend` | process | NvencEncoderBackend.Dispose() | NEW — wraps spike Phase 4 logic |
| EncoderTexture (D3D11 BGRA8) | `NvencEncoderBackend` | process | NvencEncoderBackend.Dispose() | NEW — wraps spike Phase 4 logic |
| NvEncodeAPI.vb (P/Invoke) | static (Shared) | process | N/A | COPY verbatim from spike |
| CaptureSession | `RecordingEngine` | session | auto-disposed after Run() | NEW |
| AudioFileWriter | `CaptureSession` | session | CaptureSession.Dispose() via try/finally | REUSE `Engine/Engine/[Audio]/AudioFileWriter.vb` |
| FFmpegProcessHost (mux) | `CaptureSession` | session | CaptureSession.Dispose() via try/finally | REUSE `CaptureEngine.FFmpegBackend/FFmpegProcessHost.vb` |
| MuxCoordinator | `CaptureSession` | session | CaptureSession.Dispose() via try/finally | REUSE `CaptureEngine.FFmpegBackend/MuxCoordinator.vb` |
| JobObjectGuard | `RecordingEngine` | process | RecordingEngine.Dispose() | REUSE `Engine/Engine/[Infrastructure]/JobObjectGuard.vb` |
| Output MP4 file | filesystem | beyond session | N/A | N/A |
| temp .wav / .h264 / .m4a | filesystem | session-scoped | MuxCoordinator cleans up after successful mux (existing) | REUSE |
| EngineConfigV2 | Program.vb | process | N/A (loaded once) | REUSE `CaptureEngine/Configuration/EngineConfigV2.vb` + `ConfigLoader.vb` |
| EngineLogger | Program.vb | process | Program.vb finally block | REUSE `CaptureEngine/Diagnostics/EngineLogger.vb` |
| TcpClientHelper | IPC layer (existing `[Engine] Client.vb`) | process | IpcClient.Dispose() | REUSE `Engine/Engine/[API]/TcpClientHelper.vb` |

---

## 5. Public API (VB.NET — v3 contract-driven)

```vb
' ─── CaptureEngine.Recording.RecordingEngine ────────────────────────
Imports CaptureEngine.Video
Imports CaptureEngine.Encoder
Imports CaptureEngine.FFmpegBackend
Imports CaptureEngine.Configuration.Schema
Imports CaptureEngine.Diagnostics

Namespace CaptureEngine.Recording

    ''' <summary>
    ''' Process-lifetime orchestrator. Composes IVideoCaptureBackend +
    ''' IEncoderBackend instances whose Initialize() happens ONCE per process.
    '''
    ''' RecordingEngine itself owns NO GPU resources — backends do.
    ''' This class is responsible for:
    '''   - Constructing + Initializing the persistent backends (process start)
    '''   - Dispatching StartSession/StopSession to CaptureSession instances
    '''   - Disposing the persistent backends (process exit)
    ''' </summary>
    Public NotInheritable Class RecordingEngine
        Implements IDisposable

        Public Sub New(config As EngineConfigV2, logger As EngineLogger)

        ''' <summary>Constructs + Initializes persistent backends. Throws on failure.</summary>
        Public Sub Initialize()

        ''' <summary>Starts a new recording session. Throws if session already active.</summary>
        Public Function StartSession(sessionConfig As SessionConfig) As CaptureSession

        ''' <summary>Engine status — thread-safe, never throws.</summary>
        Public Function GetStatus() As EngineStatus

        Public Sub Dispose() Implements IDisposable.Dispose
    End Class

    ''' <summary>
    ''' Per-session resource owner. Borrows persistent backends from RecordingEngine.
    ''' Owns AudioFileWriter + FFmpegProcessHost + MuxCoordinator for session duration.
    ''' Dispose() is idempotent.
    ''' </summary>
    Public NotInheritable Class CaptureSession
        Implements IDisposable

        Public Function Run() As SessionResult

        Public Sub [Stop]()

        Public Sub Dispose() Implements IDisposable.Dispose
    End Class

    Public NotInheritable Class SessionConfig
        Public Property OutputPath As String = ""
        Public Property DurationSeconds As Integer = 30
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
        Idle
        Recording
        Stopping
        Faulted
        Disposed
    End Enum

    Public NotInheritable Class EngineStatus
        Public ReadOnly Property State As EngineState
        Public ReadOnly Property CurrentSessionId As String
        Public ReadOnly Property FramesEncodedThisSession As Long
        Public ReadOnly Property AudioSamplesThisSession As Long
        Public ReadOnly Property LastSessionResult As SessionResult
        Public ReadOnly Property UptimeSec As Double
    End Class

End Namespace
```

### IEncoderBackend implementation signature (existing contract — implement, don't change)

```vb
' CaptureEngine.Encoder.Nvenc.NvencEncoderBackend (NEW — implements existing contract)
Imports CaptureEngine.Video
Imports CaptureEngine.Encoder

Namespace CaptureEngine.Encoder.Nvenc

    Public NotInheritable Class NvencEncoderBackend
        Implements IEncoderBackend

        Public Sub Initialize(config As EncoderConfig) Implements IEncoderBackend.Initialize
            ' Opens NVENC session + creates bitstream buffer + registers texture.
            ' Called ONCE at RecordingEngine construction (process-lifetime).
        End Sub

        Public Sub Start() Implements IEncoderBackend.Start
            ' Transitions to Running. Per-session — recording start.
        End Sub

        Public Function Encode(frame As IVideoFrame, ByRef packet As EncodedPacket) As Boolean Implements IEncoderBackend.Encode
            ' MapInputResource + EncodePicture + LockBitstream + UnlockBitstream.
            ' Returns True when packet produced, False when backpressure/pipeline delay.
        End Function

        Public Function Flush(sink As Action(Of EncodedPacket)) As Integer Implements IEncoderBackend.Flush
            ' Drains in-flight frames.
        End Function

        Public Sub [Stop]() Implements IEncoderBackend.Stop
            ' Transitions to Stopped. Per-session — recording stop. Encoder stays open.
        End Sub

        Public ReadOnly Property CurrentState As EncoderState Implements IEncoderBackend.CurrentState
        Public ReadOnly Property Diagnostics As IEncoderDiagnostics Implements IEncoderBackend.Diagnostics

        Public Sub Dispose() Implements IDisposable.Dispose
            ' DestroyEncoder + UnregisterResource + DestroyBitstreamBuffer.
            ' Called ONCE at RecordingEngine disposal (process-lifetime).
        End Sub
    End Class

End Namespace
```

### IVideoCaptureBackend implementation extension (existing contract — fill-in)

```vb
' CaptureEngine.Video.Ddagrab.DdagrabBackend (EXISTING — extend, don't replace)
'
' Phase 12: replace the NoFrame worker with real DXGI Output Duplication.
' The class structure (state machine, Diagnostics, lifecycle) is already correct.

Public Sub Initialize(context As IVideoBackendContext) Implements IVideoCaptureBackend.Initialize
    ' EXISTING skeleton: state machine transition Created → Initialized
    ' NEW in Phase 12:
    '   - Create D3D11 device (or accept from context if shared)
    '   - Enumerate DXGI adapters/outputs
    '   - DuplicateOutput on primary output  ← persistent for process lifetime
    '   - Create staging texture
End Sub

Public Sub Start(sink As IVideoFrameSink) Implements IVideoCaptureBackend.Start
    ' EXISTING: state machine Initialized → Running, spawn worker thread
    ' NEW in Phase 12: worker calls AcquireNextFrame instead of emitting NoFrame
End Sub

Public Sub [Stop]() Implements IVideoCaptureBackend.Stop
    ' EXISTING: state machine Running → Stopping → Stopped, join worker
    ' NO CHANGE — already correct
End Sub

Public Sub Dispose() Implements IDisposable.Dispose
    ' EXISTING: state machine → Disposed
    ' NEW in Phase 12: release DXGI duplication + D3D11 device
End Sub
```

---

## 6. Lifecycle Flow (v3 — contract composition)

```
PROCESS START (NVIDIA Capture.exe)
    │
    ▼
Program.Main(args)  (existing — modified to add RecordingEngine)
    │
    ├─ Load EngineConfigV2 via ConfigLoader
    ├─ Construct EngineLogger (file sink → Logs/capture-engine.log)
    │
    ▼
engine = New RecordingEngine(config, logger)
engine.Initialize()
    │
    ├─ Create JobObjectGuard  (REUSE)
    ├─ Create DdagrabBackend instance
    │   └─ _capture.Initialize(context)
    │       ├─ Create D3D11 device (spike Phase 1 logic)
    │       ├─ Enumerate DXGI outputs
    │       └─ DuplicateOutput on primary output  ← persistent
    │           (Phase 11 root cause #2 SOLVED — only 1 duplication per process)
    │
    ├─ Create NvencEncoderBackend instance
    │   └─ _encoder.Initialize(encoderConfig)
    │       ├─ OpenEncodeSessionEx  ← persistent
    │       │   (Phase 11 root cause #3 SOLVED — only 1 NVENC session per process)
    │       ├─ InitializeEncoder
    │       ├─ CreateBitstreamBuffer
    │       └─ RegisterResource (encoder texture)
    │
    └─ State = Idle

[IPC loop — REUSE [Engine] Client.vb + TcpClientHelper.vb]
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_record_start:<outputPath>
    │       │
    │       ▼
    │     engine.StartSession(sessionConfig) → session
    │       │
    │       ├─ Create CaptureSession (borrows _capture + _encoder)
    │       ├─ Create AudioFileWriter (REUSE)
    │       ├─ Create FFmpegProcessHost (REUSE) — wraps in JobObjectGuard
    │       ├─ State = Recording
    │       │
    │       ▼
    │     session.Run()  (background thread)
    │       ├─ audioFileWriter.Start()  → WASAPI loopback → temp.wav
    │       ├─ ffmpegProcessHost.Start()  → video pipe (raw H.264 stdin)
    │       ├─ _capture.Start(sink)  ← begins frame delivery
    │       ├─ _encoder.Start()  ← ready to encode
    │       │
    │       │  ── Capture loop ──
    │       │  while not stop and elapsed < duration:
    │       │    frame = sink.Take()  ← from capture backend
    │       │    success = _encoder.Encode(frame, packet)
    │       │    if success: write packet.Payload to ffmpegProcessHost stdin
    │       │  ─────────────────────
    │       │
    │       ├─ _encoder.Stop()  → flush in-flight frames
    │       ├─ _capture.Stop()  → stop frame delivery
    │       ├─ ffmpegProcessHost.SendQuit() → close stdin (EOF)
    │       ├─ ffmpegProcessHost.WaitForExit()
    │       ├─ audioFileWriter.Stop()  → finalize temp.wav
    │       │
    │       ├─ MuxCoordinator.MuxAsync()  (REUSE):
    │       │   ├─ ffprobe video duration
    │       │   ├─ spawn mux FFmpeg: -i video -i system.wav -c:v copy -c:a aac -t <dur> output.mp4
    │       │   └─ cleanup temp files
    │       │
    │       ├─ Verify MP4 streams (ffmpeg -i info mode)
    │       ├─ Populate SessionResult
    │       └─ finally: Dispose per-session resources (idempotent)
    │       │
    │       ▼
    │     engine.State = Idle
    │     engine.LastSessionResult = result
    │     ipcClient.Send("engine_response:engine_record_start,success")
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_record_stop
    │       session.Stop()  → signals capture loop to exit
    │
    │  ◄── TCP: [Send] NVIDIA Engine|engine_get_status
    │       returns engine.GetStatus() as JSON
    │
    ▼
[On shutdown — Ctrl+C or process termination]
    │
    ├─ ipcClient.Dispose()
    ├─ engine.Dispose()
    │   ├─ If session active: session.Stop() + session.Dispose()
    │   ├─ _capture.Dispose()  → releases DXGI duplication + D3D11 device
    │   ├─ _encoder.Dispose()  → destroys NVENC encoder + bitstream + registered resource
    │   └─ _jobGuard.Dispose()  → kills any orphan FFmpeg (kill-on-job-close)
    │
    └─ Process exit (code 0)

─── Next session (no process restart) ───

    engine.StartSession(...) → reuses _capture + _encoder
      ├─ _capture.Start(sink)  ← was already Initialized, just starts frame delivery
      ├─ _encoder.Start()  ← was already Initialized, just transitions to Running
      └─ ... (rest of session as above)

  ↳ NO new DXGI duplication, NO new NVENC session — Phase 11 root causes solved
```

---

## 7. Failure Path Guarantees (v3 — same as v2, adapted)

### 7.1 Engine construction failure

If `RecordingEngine.Initialize()` throws partway through (e.g., NVENC load fails), the constructor must dispose any partially-created backends before re-throwing:

```vb
Public Sub Initialize()
    _state = EngineState.Initializing
    Try
        _capture = New DdagrabBackend()
        _capture.Initialize(_captureContext)
        Try
            _encoder = New NvencEncoderBackend()
            _encoder.Initialize(_encoderConfig)
        Catch ex As Exception
            _capture.Dispose()
            Throw
        End Try
    Catch ex As Exception
        _state = EngineState.Faulted
        Throw
    End Try
    _state = EngineState.Idle
End Sub
```

### 7.2 Session failure — try/finally at outermost scope

```vb
Public Function Run() As SessionResult
    Dim result As SessionResult = Nothing
    Try
        result = RunUnsafe()
    Catch ex As Exception
        _logger.Error(ex, "Session failed: " + ex.Message)
        result = SessionResult.Failure(_outputPath, _durationSec, ex.Message)
    Finally
        Dispose()  ' idempotent — safe to call even if never started
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

    If _currentSession IsNot Nothing Then
        _currentSession.Stop()
        _currentSession.Dispose()
    End If

    ' Dispose persistent backends (reverse construction order)
    _encoder?.Dispose()  ' destroys NVENC
    _capture?.Dispose()  ' releases DXGI + D3D11
    _jobGuard?.Dispose() ' kills orphan FFmpeg

    _state = EngineState.Disposed
End Sub
```

---

## 8. NEW §17 — Process & Crash Recovery Lifecycle (carried from v2)

(unchanged from v2 — the crash paths analysis is independent of whether
RecordingEngine owns GPU resources directly or via composed backends.
The key property — process death releases all OS resources — still holds.)

See v2 spec §17 for full content. v3 inherits:
- §17.1 Process ownership boundary (Capture.exe owns engine + logger + IPC)
- §17.2 Happy path (15 steps)
- §17.3 Capture.exe crash path (NVENC slot freed by driver ~1s)
- §17.4 API.exe crash path (recording continues, auto-reconnect)
- §17.5 ShadowPlay.exe crash path (Capture.exe continues to duration)
- §17.6 Orphan FFmpeg guard (JobObjectGuard mandatory — REUSE existing)

---

## 9. IPC Layer (v3 — REUSE existing verbatim)

**v1:** Named pipes (rejected).
**v2:** TCP/5000 reuse (correct).
**v3:** REUSE existing `Engine/Engine/[API]/TcpClientHelper.vb` + `[Engine] Client.vb` verbatim — these are already source files in `Engine/NVIDIA Capture.vbproj`, not even a separate project.

Phase 12 only needs to:
1. Wire the existing `[Engine] Client.vb` OnMessage handler to dispatch to `RecordingEngine.StartSession/StopSession/GetStatus` instead of legacy `CaptureEngine.vb`.
2. The command set (`engine_record_start`, `engine_record_stop`, `engine_get_status`) already exists — ShadowPlay.exe sends these commands already.

### JSON-in-value protocol (carried from v2)

Existing wire format `[Send] <App>|<cmd>:<value>` is kept. For `engine_get_status` and `engine_get_result`, the `<value>` field contains URL-encoded JSON.

---

## 10. FFmpeg Path Resolution (v3 — REUSE existing config)

**v2:** Reuse `EngineConfigV2.Runtime.FFmpegPath`.
**v3:** Same — no change. The only addition is a small validation helper.

```vb
' CaptureEngine.Recording.Internal.FfmpegPathResolver (NEW, small)
Public NotInheritable Class FfmpegPathResolver
    Public Shared Function Resolve(config As EngineConfigV2) As String
        Dim path As String = config.Runtime.FFmpegPath
        If String.IsNullOrWhiteSpace(path) Then
            ' Fallback: deployment-relative
            Dim exeDir As String = AppContext.BaseDirectory
            Dim candidates As String() = {
                Path.Combine(exeDir, "API-Core", "ffmpeg.exe"),
                Path.Combine(exeDir, "ffmpeg.exe")
            }
            For Each c As String In candidates
                If File.Exists(c) Then Return Path.GetFullPath(c)
            Next
            Throw New FileNotFoundException("ffmpeg.exe not found...")
        End If
        If Not File.Exists(path) Then
            Throw New FileNotFoundException($"FFmpeg not found at: {path}")
        End If
        Return path
    End Function
End Class
```

---

## 11. File Layout (v3)

### Existing structure (REUSE — do NOT modify unless noted)

```
CaptureEngine/                              ← Foundation (FROZEN)
├── Engine/CaptureEngine.vb                ← frozen @ 82d792ab
├── Engine/EngineState.vb                  ← frozen
├── Configuration/EngineConfigV2.vb        ← REUSE (only ADD AudioWarmupSec field)
├── Configuration/ConfigLoader.vb          ← REUSE
├── Configuration/ConfigMigrator.vb        ← REUSE
├── Configuration/ConfigValidator.vb        ← REUSE
├── Diagnostics/EngineLogger.vb            ← REUSE
├── FFmpeg/*                                ← REUSE
├── Pipeline/*                              ← REUSE
└── Backends/IVideoBackend.vb              ← REUSE

CaptureEngine.Video/                       ← Foundation video contract (FROZEN)
└── Contract/IVideoCaptureBackend.vb        ← frozen

CaptureEngine.Encoder/                     ← Foundation encoder contract (FROZEN)
├── Contract/IEncoderBackend.vb            ← frozen
├── Contract/EncodedPacket.vb              ← frozen
├── Contract/EncoderConfig.vb              ← frozen
├── Contract/EncoderState.vb               ← frozen
├── Contract/IEncoderDiagnostics.vb        ← frozen
└── Backends/Fake/FakeEncoderBackend.vb    ← reference impl (test only)

CaptureEngine.FFmpegBackend/              ← FFmpeg subprocess orchestrator (REUSE)
├── FFmpegPipelineBackend.vb               ← existing per-session orchestrator (do NOT modify)
├── FFmpegProcessHost.vb                  ← REUSE
├── FFmpegStderrParser.vb                 ← REUSE
├── AudioSidecar.vb                       ← EXISTING STUB — leave as-is (Phase 12 uses AudioFileWriter instead)
└── MuxCoordinator.vb                     ← REUSE

Engine/                                    ← Legacy Engine-Audio code (mostly frozen)
├── NVIDIA Capture.vbproj                 ← UPDATE — add ProjectReferences
├── ENGINE-REALTIME-ARCHITECTURE.md        ← existing architecture doc
├── Engine/[Infrastructure]/JobObjectGuard.vb   ← REUSE
├── Engine/[Audio]/AudioFileWriter.vb      ← REUSE (37 KB, complete WASAPI)
├── Engine/[API]/TcpClientHelper.vb        ← REUSE
├── Engine/[API]/[Engine] Client.vb       ← REUSE (wire to RecordingEngine)
├── Engine/[Capture]/CaptureEngine.vb    ← legacy (1 KB) — DO NOT use in Phase 12
├── Engine/[Capture]/CaptureSettings.vb   ← legacy — DO NOT use
├── Engine/[FFmpeg]/FFmpegArgumentBuilder.vb ← legacy — DO NOT use
└── Engine/[UI]/*                          ← WinForms UI — modify to construct RecordingEngine
```

### NEW Phase 12 structure

```
CaptureEngine.Encoder.Nvenc/              ← NEW PROJECT (Phase 12a)
├── CaptureEngine.Encoder.Nvenc.vbproj
│   ├── ProjectReference: CaptureEngine.Encoder
│   ├── ProjectReference: CaptureEngine.Video
│   ├── ProjectReference: CaptureEngine
│   └── PackageReference: Vortice.Windows 3.x (D3D11 + DXGI)
├── NvencEncoderBackend.vb                 ← implements IEncoderBackend
└── Internal/
    ├── NvEncodeAPI.vb                     ← COPY spike's NvEncodeAPI.cs verbatim (translated to VB)
    ├── NvEncFunctionTable.vb              ← function pointer table + loader
    ├── NvencResources.vb                  ← bitstream buffer + registered texture wrapper
    └── D3D11DeviceFactory.vb              ← D3D11 device creation (shared with DdagrabBackend)

CaptureEngine.Video.Ddagrab/               ← EXISTING PROJECT (Phase 12a — extend)
└── DdagrabBackend.vb                      ← FILL IN real DXGI capture (replace NoFrame worker)

CaptureEngine.Recording/                   ← NEW PROJECT (Phase 12a)
├── CaptureEngine.Recording.vbproj
│   ├── ProjectReference: CaptureEngine
│   ├── ProjectReference: CaptureEngine.Video
│   ├── ProjectReference: CaptureEngine.Video.Ddagrab
│   ├── ProjectReference: CaptureEngine.Encoder
│   ├── ProjectReference: CaptureEngine.Encoder.Nvenc
│   └── ProjectReference: CaptureEngine.FFmpegBackend
├── RecordingEngine.vb                     ← orchestrator (no GPU ownership)
├── CaptureSession.vb                      ← per-session owner (borrows backends)
├── SessionConfig.vb                       ← DTO
├── SessionResult.vb                       ← DTO
├── EngineStatus.vb                        ← DTO
├── EngineState.vb                         ← enum
└── Internal/
    └── FfmpegPathResolver.vb              ← small validation helper

CaptureEngine.Recording.ConsoleDriver/     ← NEW PROJECT (Phase 12a — test only)
├── CaptureEngine.Recording.ConsoleDriver.vbproj
│   ├── OutputType: Exe
│   └── ProjectReference: CaptureEngine.Recording
└── Program.vb                             ← minimal CLI for 2-session lifecycle test
```

### Solution file update

Add 2 new projects to existing `Overlay/NVIDIA Overlay.sln`:
- `..\CaptureEngine.Encoder.Nvenc\CaptureEngine.Encoder.Nvenc.vbproj`
- `..\CaptureEngine.Recording\CaptureEngine.Recording.vbproj`
- `..\CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj`

Existing `Engine/NVIDIA Capture.vbproj` is already in the .sln (GUID `{AF85BD81-55B6-44E8-BD61-81FDF22E0F98}`). Phase 12 just adds ProjectReferences inside it.

---

## 12. Phase 12a Definition of Done (v3)

### Minimum runtime gate (per OWNER spec)

```
Build ✅
30s recording ✅
Video ✅
Audio ✅
MP4 ✅
NVENC errors = 0 ✅
Cleanup ✅
Second session ✅  ← THIS IS THE LIFECYCLE CONTRACT GATE
```

If only one session works, 12a is NOT PASS.

### File-level checklist

- [ ] `CaptureEngine.Encoder.Nvenc/` project created with files in §11
- [ ] `NvEncodeAPI.vb` copied verbatim from spike (translated to VB syntax)
- [ ] `NvencEncoderBackend.vb` implements `IEncoderBackend` (Initialize/Start/Encode/Flush/Stop/Dispose)
- [ ] `DdagrabBackend.vb` extended with real DXGI Output Duplication (replace NoFrame worker)
- [ ] `CaptureEngine.Recording/` project created
- [ ] `RecordingEngine.vb` orchestrates backends (no direct GPU ownership)
- [ ] `CaptureSession.vb` per-session owner with try/finally Run
- [ ] DTOs: `SessionConfig.vb`, `SessionResult.vb`, `EngineStatus.vb`, `EngineState.vb`
- [ ] `FfmpegPathResolver.vb` reads `Runtime.FFmpegPath` from config
- [ ] `AudioWarmupSec` added to `EngineConfigV2.vb` Audio.Sync subsection
- [ ] `CaptureEngine.Recording.ConsoleDriver/` project created (Exe)
- [ ] `Program.vb` in ConsoleDriver runs 2 sessions back-to-back
- [ ] Add 3 new projects to `Overlay/NVIDIA Overlay.sln`
- [ ] Foundation `CaptureEngine.vb` @ `82d792ab` UNMODIFIED (verified via git diff)
- [ ] Spike files in `spikes/` UNMODIFIED (verified via git diff)
- [ ] Existing `Engine/Engine/[Infrastructure]/JobObjectGuard.vb` UNMODIFIED
- [ ] Existing `Engine/Engine/[Audio]/AudioFileWriter.vb` UNMODIFIED
- [ ] Existing `CaptureEngine.FFmpegBackend/*` UNMODIFIED (except possibly AudioSidecar.vb — but Phase 12 doesn't use it, leaves as-is)

### Phase 12b (deferred — adds IPC + host integration)

Not in 12a:
- Update `Engine/NVIDIA Capture.vbproj` to reference new projects
- Wire `[Engine] Client.vb` OnMessage to RecordingEngine
- Replace WinForms UI construction to use RecordingEngine instead of legacy CaptureEngine
- Crash path validation (taskkill /F → JobObjectGuard kills FFmpeg)

---

## 13. Open Questions (v3 — reduced)

v2 had 7 questions. v3 answers most via the audit + OWNER decision. Remaining:

1. **D3D11 device sharing between DdagrabBackend and NvencEncoderBackend** — both need a D3D11 device. Options:
   - (a) Each creates its own device (simpler, but cross-device texture sharing requires keyed mutex)
   - (b) DdagrabBackend creates the device; NvencEncoderBackend receives it via Initialize context
   - (c) RecordingEngine creates the device; passes to both via Initialize context
   - **Recommendation: (b)** — DdagrabBackend is the natural device owner (it captures frames). NvencEncoderBackend's `EncoderConfig` can carry a `Device` field. Spike Phase 10 had them sharing via SpikeSharedContext — same pattern.
   - **OWNER confirmation needed.**

2. **`DdagrabBackend` device creation** — should it create its own device (like spike Phase 1 does), or accept one from `IVideoBackendContext`? Existing `IVideoBackendContext` interface signature unknown without source read. **Recommendation:** Read `IVideoBackendContext.vb` and follow existing pattern.

3. **AudioFileWriter API** — does it match what `CaptureSession` needs (Start/Stop/Dispose, temp .wav path, QPC timestamps)? **Recommendation:** Read `AudioFileWriter.vb` source before writing CaptureSession. If API doesn't match, wrap it in a thin adapter inside `CaptureEngine.Recording.Internal`.

4. **EncodedPacket → FFmpeg stdin** — `IEncoderBackend.Encode()` returns `EncodedPacket`. The packet needs to be written to `FFmpegProcessHost`'s stdin (raw H.264 NAL bytes). Confirm `EncodedPacket.Payload` is `byte[]` or `Span<byte>` and is caller-owned (so CaptureSession can write + dispose). **Recommendation:** Read `EncodedPacket.vb` source.

These are all "read source first" questions, not design questions. Resolve during implementation.

---

## 14. Implementation Order (Phase 12a)

Per OWNER: "fetch source spike → port low-level → wire ownership layer → runtime test"

1. **Fetch + read all source files needed** (spike + production contracts)
   - Spike: NvEncodeAPI.cs, Phase1/2/4/10 .cs
   - Production: IEncoderBackend.vb, EncodedPacket.vb, EncoderConfig.vb, IVideoCaptureBackend.vb, IVideoBackendContext.vb, DdagrabBackend.vb, AudioFileWriter.vb, MuxCoordinator.vb, FFmpegProcessHost.vb, JobObjectGuard.vb, EngineConfigV2.vb, EngineLogger.vb

2. **Port low-level (NVENC + D3D11)**
   - Translate `NvEncodeAPI.cs` → `NvEncodeAPI.vb` (verbatim structs)
   - Translate `NvEncFunctionTable` (was inside NvEncodeAPI.cs in spike)
   - Create `D3D11DeviceFactory.vb` (from spike Phase 1)
   - Create `NvencResources.vb` (from spike Phase 4: bitstream + registered resource)
   - Create `NvencEncoderBackend.vb` (implements IEncoderBackend, wraps spike Phase 4 + 10 logic)

3. **Fill in DdagrabBackend**
   - Replace NoFrame worker with real AcquireNextFrame
   - Initialize() creates DXGI duplication (persistent)
   - Start/Stop/Dispose lifecycle (existing pattern is correct, just fill in worker body)

4. **Wire ownership layer**
   - Create `CaptureEngine.Recording` project
   - `RecordingEngine.vb` — constructs + Initialize backends, dispatches sessions
   - `CaptureSession.vb` — borrows backends, owns AudioFileWriter + FFmpeg + Mux
   - DTOs

5. **Runtime test**
   - `CaptureEngine.Recording.ConsoleDriver` — runs 2 sessions back-to-back
   - Validates: 30s recording, video stream, audio stream, MP4, NVENC errors=0, second session PASS
   - If 2nd session fails → Phase 12a NOT PASS (lifecycle contract broken)

---

## 15. Recommendation

Implement Phase 12a in this order:
1. Fetch all source files (spike + production contracts)
2. Port NVENC ABI + D3D11 device factory (lowest level, no deps)
3. Implement `NvencEncoderBackend` (depends on NVENC ABI + IEncoderBackend contract)
4. Fill in `DdagrabBackend` (depends on D3D11 + IVideoCaptureBackend contract)
5. Create `CaptureEngine.Recording` orchestrator (depends on backends)
6. Write `ConsoleDriver` (validates lifecycle)
7. Run minimum runtime gate (2 sessions, no rebuild GPU)

This order follows OWNER's directive: "fetch source spike → port low-level → wire ownership layer → runtime test".
