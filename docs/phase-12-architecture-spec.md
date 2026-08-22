# Phase 12 — Production RecordingEngine Architecture Spec

**Status:** Draft for OWNER review
**Predecessor:** Phase 11 (FAIL — see `Phase11_PostMortem.md`)
**Target location:** `CaptureEngine/` (existing repo folder)
**Process host:** `NVIDIA Capture.exe` (existing)

---

## 1. Objective

Build a **production-grade `RecordingEngine`** that:

1. Owns GPU resources (D3D11 device, DXGI duplication, NVENC encoder)
   for the **entire process lifetime** — not per session.
2. Exposes `StartSession()` / `StopSession()` that reuse the persistent
   infrastructure and only manage per-session resources (audio capture,
   FFmpeg process, output files).
3. Has **deterministic cleanup** via `try/finally` at the right scope,
   with explicit ownership contracts.
4. Resolves runtime dependencies (FFmpeg path, output directory) from
   a deployment contract, not from `PATH` guessing.
5. Can be tested by Phase 13 (lifecycle stress test) and **PASS** all
   11 scenarios that Phase 11 failed.

This is **not** a port of `Phase10_RealRecording.cs`. The spike proved
the low-level interop works; this phase builds the right **ownership
abstraction** on top of those proven pieces.

---

## 2. HARD RULES (carried over from spike + new)

- ✅ No changes to NVENC struct layouts (proven in spike Phase 4-9).
- ✅ No manual WASAPI COM (use NAudio loopback — proven in Phase 10).
- ✅ No Foundation changes.
- ✅ Phase 1-9 spike code stays in `spikes/D3D11_NVENC_Spike/` as evidence.
- ✅ Phase 10 spike code stays as evidence (do **not** port directly).
- 🆕 New code lives under `CaptureEngine/` (existing repo folder).
- 🆕 `NVIDIA Capture.exe` (in `Overlay/`) becomes the host that
  constructs `RecordingEngine` and exposes it via IPC.
- 🆕 Resource ownership is documented in this spec — every `IDisposable`
  has exactly one owner.

---

## 3. Architecture

```
┌──────────────────────────────────────────────────────────────┐
│  NVIDIA Capture.exe (process host)                          │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  RecordingEngine  (process-lifetime singleton)        │  │
│  │                                                        │  │
│  │  Owns (persistent, created at startup):               │  │
│  │    • D3D11Device                                       │  │
│  │    • DxgiOutputDuplication  (1 per output)             │  │
│  │    • NvencEncoder          (1 per GPU)                  │  │
│  │    • NvencBitstreamBuffer                              │  │
│  │    • NvencRegisteredResource                           │  │
│  │    • EncoderTexture (D3D11 BGRA8)                      │  │
│  │                                                        │  │
│  │  Per-session (created in StartSession):               │  │
│  │    • CaptureSession                                    │  │
│  │      ├─ WasapiLoopbackCapture (NAudio)                 │  │
│  │      ├─ WaveFileWriter       (NAudio)                  │  │
│  │      ├─ FileStream           (.tmp.h264 writer)        │  │
│  │      ├─ FFmpegProcess        (mux subprocess)          │  │
│  │      └─ SessionResult        (struct return)           │  │
│  │                                                        │  │
│  │  Stateless helpers:                                   │  │
│  │    • FfmpegPathResolver                                │  │
│  │    • SessionConfig          (DTO)                      │  │
│  │    • SessionResult          (DTO)                      │  │
│  │    • ILogger                (file: capture-engine.log) │  │
│  └────────────────────────────────────────────────────────┘  │
│                          │                                   │
│  ┌───────────────────────┴────────────────────────────────┐  │
│  │  IPC layer (Named Pipe or HTTP localhost)              │  │
│  │  Exposes: StartSession / StopSession / GetStatus       │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
                          ▲
                          │ IPC
                          ▼
        NVIDIA ShadowPlay.exe (orchestrator, existing)
```

---

## 4. Ownership Contract

Each resource below has **exactly one** owner. No shared mutable
state without an owner.

| Resource | Owner | Lifetime | Disposal mechanism |
|---|---|---|---|
| D3D11Device | `RecordingEngine` | process | `RecordingEngine.Dispose()` |
| DxgiOutputDuplication | `RecordingEngine` | process | `RecordingEngine.Dispose()` |
| NvencEncoder handle | `RecordingEngine` | process | `RecordingEngine.Dispose()` |
| NvencBitstreamBuffer | `RecordingEngine` | process | `RecordingEngine.Dispose()` |
| NvencRegisteredResource | `RecordingEngine` | process | `RecordingEngine.Dispose()` |
| EncoderTexture (D3D11) | `RecordingEngine` | process | `RecordingEngine.Dispose()` |
| WasapiLoopbackCapture | `CaptureSession` | session | `CaptureSession.Dispose()` via `try/finally` |
| WaveFileWriter | `CaptureSession` | session | `CaptureSession.Dispose()` via `try/finally` |
| FileStream (.tmp.h264) | `CaptureSession` | session | `CaptureSession.Dispose()` via `try/finally` |
| FFmpeg mux process | `CaptureSession` | session | `CaptureSession.Dispose()` via `try/finally` |
| Output MP4 file | filesystem | beyond session | N/A — kept as session result |
| temp .wav / .h264 / .m4a | filesystem | session-scoped | deleted by `CaptureSession` after `SessionResult` populated |

---

## 5. Public API (proposed)

```csharp
namespace CaptureEngine.Recording;

/// <summary>
/// Process-lifetime singleton. Owns GPU resources.
/// Constructed once at NVIDIA Capture.exe startup; disposed at shutdown.
/// </summary>
public sealed class RecordingEngine : IDisposable
{
    public RecordingEngine(RecordingEngineConfig config, ILogger logger);

    /// <summary>
    /// Starts a new recording session. Throws if a session is already running.
    /// </summary>
    public CaptureSession StartSession(SessionConfig sessionConfig);

    /// <summary>
    /// Engine status — never throws. Safe to call from any thread.
    /// </summary>
    public EngineStatus GetStatus();

    public void Dispose();
}

/// <summary>
/// Per-session resource owner. Reuses the parent engine's GPU resources.
/// Dispose() is idempotent and safe to call from finally blocks.
/// </summary>
public sealed class CaptureSession : IDisposable
{
    /// <summary>
    /// Blocks until the session duration elapses or Stop() is called.
    /// Returns the session result. Always runs cleanup via finally.
    /// </summary>
    public SessionResult Run();

    /// <summary>
    /// Signals early stop. Run() will return shortly after.
    /// </summary>
    public void Stop();

    public void Dispose();
}

public sealed class SessionConfig
{
    public string OutputPath { get; init; } = "";
    public int DurationSeconds { get; init; } = 30;
    public int VideoBitrateKbps { get; init; } = 30_000;
    public int AudioBitrateKbps { get; init; } = 192;
    public string? AudioSource { get; init; }  // null = default loopback
}

public sealed class SessionResult
{
    public string OutputPath { get; init; } = "";
    public int RequestedDurationSec { get; init; }
    public double ActualDurationSec { get; init; }
    public long FramesCaptured { get; init; }
    public long FramesEncoded { get; init; }
    public long Drops { get; init; }
    public long NvencErrors { get; init; }
    public long TotalVideoBytes { get; init; }
    public long AudioSamples { get; init; }
    public long AudioBytes { get; init; }
    public bool VideoStreamFound { get; init; }
    public bool AudioStreamFound { get; init; }
    public bool FileExists { get; init; }
    public long FileSize { get; init; }
    public string? ErrorMessage { get; init; }

    public bool Pass =>
        FramesEncoded > 0 &&
        NvencErrors == 0 &&
        FileExists &&
        FileSize > 0 &&
        VideoStreamFound &&
        AudioSamples > 0 &&
        AudioStreamFound;
}

public sealed class RecordingEngineConfig
{
    public string FfmpegPath { get; init; } = "";
    public string DefaultOutputDir { get; init; } = "";
    public int AudioWarmupSec { get; init; } = 2;
}

public enum EngineState { Idle, Recording, Disposed }
public sealed class EngineStatus
{
    public EngineState State { get; init; }
    public string? CurrentSessionId { get; init; }
    public long FramesEncodedThisSession { get; init; }
}
```

---

## 6. Lifecycle Flow

```
PROCESS START (NVIDIA Capture.exe)
    │
    ▼
RecordingEngine engine = new(config, logger);
    │     ├─ Phase 1 logic: enumerate DXGI, pick NVIDIA adapter
    │     ├─ Create D3D11 device (multithread protected)
    │     ├─ Phase 2 logic: DuplicateOutput on primary output
    │     ├─ Phase 4 logic: OpenEncodeSessionEx + InitializeEncoder
    │     ├─ Create bitstream buffer + register encoder texture
    │     └─ State = Idle
    │
    ▼
[IPC loop — listen for commands from NVIDIA ShadowPlay.exe]
    │
    │  ◄── StartSession(config) ──
    │       │
    │       ▼
    │     CaptureSession session = engine.StartSession(config);
    │       │  ├─ Validate engine state == Idle (else throw)
    │       │  ├─ State = Recording
    │       │  ├─ Open .tmp.h264 FileStream
    │       │  ├─ Spawn AudioCaptureLoop (NAudio)
    │       │  └─ (engine's GPU resources already exist)
    │       │
    │       ▼
    │     SessionResult result = session.Run();
    │       │  ├─ Capture loop: AcquireNextFrame → CopyResource →
    │       │  │   MapInputResource → EncodePicture → LockBitstream →
    │       │  │   write H.264 NAL to file → UnlockBitstream
    │       │  ├─ Stop signal (duration OR explicit Stop())
    │       │  ├─ Join audio thread
    │       │  ├─ FFmpeg mux (video + audio → MP4)
    │       │  ├─ Verify streams (ffmpeg -i info mode)
    │       │  └─ finally: dispose session resources
    │       │
    │       ▼
    │     session.Dispose();  // idempotent
    │     engine.State = Idle;
    │     return result;
    │  ──► SessionResult ──►
    │
    │  ◄── StopSession() ──  (if early stop requested)
    │       session.Stop();  // signals capture loop to exit
    │
    ▼
[On shutdown]
engine.Dispose();
    │  ├─ DestroyEncoder
    │  ├─ UnregisterResource
    │  ├─ DestroyBitstreamBuffer
    │  ├─ Dispose encoder texture
    │  ├─ Dispose DXGI duplication
    │  └─ Dispose D3D11 device
    │
    ▼
PROCESS EXIT
```

---

## 7. Failure Path Guarantees

The single most important property of this design: **the exception path
disposes the right resources at the right scope.**

### 7.1 Engine construction failure

If `RecordingEngine` constructor throws partway through (e.g. NVENC
load fails), the constructor itself must dispose any partially-created
resources before re-throwing. Pattern:

```csharp
public RecordingEngine(...)
{
    _device = CreateDevice();
    try
    {
        _duplication = CreateDuplication(_device);
        _encoder = OpenEncoder(_device);
        try
        {
            _bitstream = CreateBitstream(_encoder);
            _registeredResource = RegisterTexture(_encoder, _encoderTexture);
        }
        catch
        {
            _encoder.Dispose();
            throw;
        }
    }
    catch
    {
        _duplication.Dispose();
        _device.Dispose();
        throw;
    }
}
```

### 7.2 Session failure

`CaptureSession.Run()` uses a single outermost `try/finally` that
guarantees disposal of all per-session resources, even if recording
threw during the first frame:

```csharp
public SessionResult Run()
{
    SessionResult result;
    try
    {
        result = RunUnsafe();
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Session failed");
        result = SessionResult.Failure(ex.Message);
    }
    finally
    {
        // Idempotent — safe to call even if never started
        Dispose();
    }
    return result;
}
```

### 7.3 Engine disposal must be idempotent

```csharp
public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    // dispose in reverse construction order
    _registeredResource?.Dispose();
    _bitstream?.Dispose();
    _encoder?.Dispose();
    _encoderTexture?.Dispose();
    _duplication?.Dispose();
    _device?.Dispose();
}
```

---

## 8. FFmpeg Path Resolution

No more `PATH` walking or `"ffmpeg"` literal fallback.

```csharp
public sealed class FfmpegPathResolver
{
    public string Resolve(RecordingEngineConfig config)
    {
        // 1. Explicit config wins
        if (File.Exists(config.FfmpegPath))
            return config.FfmpegPath;

        // 2. Deployment-relative (relative to NVIDIA Capture.exe)
        string exeDir = AppContext.BaseDirectory;
        string[] candidates =
        {
            Path.Combine(exeDir, "API-Core", "ffmpeg.exe"),
            Path.Combine(exeDir, "ffmpeg.exe"),
            Path.Combine(exeDir, "..", "API-Core", "ffmpeg.exe"),
        };
        foreach (var c in candidates)
            if (File.Exists(c)) return Path.GetFullPath(c);

        throw new FileNotFoundException(
            "ffmpeg.exe not found. Set FfmpegPath in engine.json " +
            "or install at <app>/API-Core/ffmpeg.exe");
    }
}
```

The `engine.json` config (already exists in your deployment) gains a
`FfmpegPath` field. Default empty → resolver falls back to deployment-relative.

---

## 9. IPC Layer (initial proposal — needs OWNER confirmation)

`NVIDIA Capture.exe` becomes a long-running process that hosts the
`RecordingEngine`. `NVIDIA ShadowPlay.exe` (the orchestrator) communicates
via one of:

| Option | Pros | Cons |
|---|---|---|
| **Named pipes** (`\\.\pipe\nvidia-capture`) | Native Windows, fast, simple | Single-client only; harder to debug |
| **HTTP localhost** (`http://127.0.0.1:PORT/`) | Multi-client, debuggable via curl | Heavier; needs port management |
| **gRPC over named pipes** | Strongly-typed contracts | Overkill for 2-process system |

**Recommended: Named pipes** for v1 — single orchestrator client,
simple JSON protocol, low overhead.

Protocol sketch:
```json
→ {"cmd":"start","outputPath":"...","durationSec":30}
← {"ok":true,"sessionId":"..."}

→ {"cmd":"stop"}
← {"ok":true}

→ {"cmd":"status"}
← {"state":"recording","framesEncoded":1234,...}

→ {"cmd":"getResult","sessionId":"..."}
← {"ok":true,"result":{...}}
```

**Question for OWNER:** does NVIDIA ShadowPlay.exe already have an IPC
mechanism in place? If yes, reuse it. If no, named pipes is the proposal.

---

## 10. Logging

Replace spike's `Console.WriteLine` with structured logging to
`Logs/capture-engine.log` (already exists in your deployment tree).

Use `Microsoft.Extensions.Logging` (already a dependency of .NET 8):
- `LogInformation` for session start/stop, frame milestones
- `LogWarning` for non-fatal issues (drops, late frames)
- `LogError` for failures (NVENC errors, FFmpeg non-zero exit)
- `LogDebug` for per-frame trace (off by default)

Console output becomes optional — controlled by `--console` flag.
Default: silent, log to file only.

---

## 11. File Layout (proposed)

```
CaptureEngine/
├── RecordingEngine.cs             # The singleton
├── CaptureSession.cs              # Per-session owner
├── SessionConfig.cs               # DTO
├── SessionResult.cs               # DTO
├── RecordingEngineConfig.cs       # DTO (engine.json binding)
├── EngineStatus.cs                # DTO
├── EngineState.cs                 # enum
├── Internal/
│   ├── D3D11DeviceFactory.cs      # Wraps Phase 1 logic
│   ├── DxgiDuplicationFactory.cs  # Wraps Phase 2 logic
│   ├── NvencEncoderFactory.cs     # Wraps Phase 4 logic
│   ├── AudioCaptureLoop.cs        # Wraps NAudio loopback
│   ├── FfmpegMuxer.cs             # Wraps FFmpeg subprocess
│   ├── FfmpegPathResolver.cs     # Deployment-aware resolver
│   └── NvEncodeAPI.cs             # ← COPY from spike (proven struct layouts)
└── CaptureEngine.csproj           # net8.0-windows, library
```

`NVIDIA Capture.exe` (in `Overlay/`) references this library and exposes
it via IPC.

---

## 12. Test Plan (Phase 13 — placeholder)

Phase 13 will re-run the same Test A/B/C matrix as Phase 11, but against
the new `RecordingEngine` + IPC layer. Expected outcome:

| Test | Phase 11 | Phase 13 expected |
|---|---|---|
| A1-A3 (3 × 30s) | FAIL | PASS (engine reuses GPU; session only manages audio+FFmpeg) |
| B1-B3 (1s/5s/10s) | FAIL | PASS (early Stop() works; session.Dispose() cleans up) |
| C1-C5 (5 × immediate restart) | FAIL | PASS (encoder stays open; session restart is fast) |
| Memory growth | +6140% | < +20% (only per-session buffers cycle) |
| FFmpeg orphans | 0 (never started) | 0 (properally disposed) |

---

## 13. Open Questions for OWNER

Before implementation, please confirm:

1. **IPC choice** — Named pipes OK, or is there an existing IPC layer
   between `NVIDIA ShadowPlay.exe` and `NVIDIA Capture.exe` that we
   should reuse?

2. **`engine.json` schema** — what's the current format? Need to add
   `FfmpegPath`, `DefaultOutputDir`, `AudioWarmupSec` fields without
   breaking existing config.

3. **`CaptureEngine/` current contents** — is the folder empty/stub,
   or is there existing code that needs to coexist? Should I add a new
   subfolder `CaptureEngine/Recording/`?

4. **`NVIDIA Capture.exe` current state** — is it currently a stub
   (entry point only) or does it have logic we need to preserve?
   Can I refactor freely?

5. **NVENC struct layouts** — confirm I can copy
   `spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs` verbatim into
   `CaptureEngine/Internal/`. The HARD RULES say "no changes to proven
   NVENC struct layouts" — copying counts as no-change.

6. **Foundation** — confirm `CaptureEngine/` (or its subfolder) is
   not part of the protected Foundation. The Foundation likely refers
   to `Overlay/` runtime + existing `CaptureEngine/` content. Need
   confirmation before adding new files.

---

## 14. Out of Scope (deferred to later phases)

- Multi-output capture (currently 1 output, extendable later)
- HEVC / AV1 codec support (H.264 only for v1)
- Hardware-accelerated audio encoding (CPU AAC via FFmpeg is fine)
- Recording highlight / instant replay (separate feature)
- Streamed output (RTMP/SRT — separate phase)
- Multi-GPU selection (pick adapter 0 for now)

---

## 15. Definition of Done (Phase 12)

- [ ] `CaptureEngine/Recording/` folder exists with the files in §11
- [ ] `RecordingEngine` constructs and disposes cleanly (no leaks)
- [ ] `CaptureSession.Run()` returns `SessionResult` for normal 30s case
- [ ] `CaptureSession.Dispose()` is idempotent and safe in `finally`
- [ ] FFmpeg path resolved from deployment root, not PATH
- [ ] Structured logging to `Logs/capture-engine.log`
- [ ] `NVIDIA Capture.exe` constructs `RecordingEngine` at startup
- [ ] IPC endpoint responds to `status` command (start/stop can be Phase 12.5)
- [ ] Unit tests: construction, disposal, idempotent-dispose, path resolution

Phase 12 does NOT require:
- Running the full Phase 13 lifecycle stress test (that's Phase 13)
- IPC `start`/`stop` commands (can be deferred to Phase 12.5 if scope is too big)
- Console CLI for manual testing (Phase 13 will exercise via IPC)

---

## 16. Recommendation

Implement Phase 12 in two sub-phases:

- **Phase 12a** — `RecordingEngine` + `CaptureSession` library, exercised
  via a minimal console driver (similar to spike Phase 10 but using the
  new ownership model). Validates the architecture before IPC layer.

- **Phase 12b** — IPC layer + `NVIDIA Capture.exe` integration.

This separation reduces risk: 12a proves the resource ownership model
works without IPC complications; 12b adds the orchestration layer on top.
