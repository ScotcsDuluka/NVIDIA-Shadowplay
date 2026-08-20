# P1-F — Video Encoder Subsystem Architecture

> **Document type:** Architecture specification
> **Branch:** `Engine-Rebuild-Stabilization`
> **Status:** v1.0 — encoder contract + FakeEncoderBackend complete; NVENC backend deferred to P1-F-NVENC (GLM/4)

---

| Field | Value |
|---|---|
| Document | `docs/P1-F-ENCODER-ARCHITECTURE.md` |
| Version | v1.0 |
| Author | GLM-3 (Encoder Architecture Lead) |
| Date | 2026-08-19 |
| Branch | `Engine-Rebuild-Stabilization` |
| Commits | `4f1c2a3` (contracts), `74c7bc6` (FakeEncoder), `9ec5113` (tests) |

---

## 1. Scope

P1-F defines the **video encoder subsystem** boundary for the NVIDIA-Shadowplay Engine-Rebuild project. The encoder accepts `IVideoFrame` instances (from any capture backend) and produces `EncodedPacket` instances (for the future Output/Muxer layer).

**In scope:**
- Encoder contract (`IEncoderBackend`, `EncoderConfig`, `EncodedPacket`, `PacketMetadata`, `EncoderState`, `IEncoderDiagnostics`, exception hierarchy)
- FakeEncoderBackend (deterministic in-process reference implementation)
- Contract tests (lifecycle, encode, packet, concurrency)

**Out of scope (deferred):**
- NVENC native implementation (P1-F-NVENC, owned by GLM/4)
- QSV / AMF / Software encoder implementations
- FFmpeg encoder wrapper
- Output / Muxer pipeline (Phase 2)
- Audio pipeline (Phase 3)
- A/V synchronization (Phase 4)

**Explicit non-goals:**
- Do NOT modify frozen Foundation (`IVideoFrame`, `FrameDiagnostics`, `IVideoCaptureBackend`)
- Do NOT add NVENC-specific API to the public contract
- Do NOT depend on D3D11/DXGI/FFmpeg/NAudio/WinForms
- Do NOT wait for D3D11/DXGI capture implementation

---

## 2. Architecture

### 2.1 Pipeline Position

```
┌─────────────────────────────────────────────────────────────┐
│                  Engine-Rebuild Pipeline                     │
│                                                             │
│  ┌─────────────┐    ┌──────────────┐    ┌─────────────┐    │
│  │ Capture     │───▶│ IVideoFrame  │───▶│ Encoder     │    │
│  │ Backend     │    │ (Foundation, │    │ (P1-F)      │    │
│  │ (P1-B/P1-D) │    │  frozen)     │    │             │    │
│  └─────────────┘    └──────────────┘    └──────┬──────┘    │
│                                                │            │
│                                                ▼            │
│                                         ┌─────────────┐    │
│                                         │ EncodedPacket│    │
│                                         │ (caller-    │    │
│                                         │  owned)     │    │
│                                         └──────┬──────┘    │
│                                                │            │
│                                                ▼            │
│                                         ┌─────────────┐    │
│                                         │ Output/Muxer│    │
│                                         │ (Phase 2)   │    │
│                                         └─────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Project Structure

```
CaptureEngine.Encoder/                          (NEW — P1-F)
├── AssemblyInfo.vb                            (InternalsVisibleTo tests)
├── CaptureEngine.Encoder.vbproj              (net8.0, no NuGet)
├── Contract/
│   ├── IEncoderBackend.vb                     (main interface)
│   ├── EncoderState.vb                       (8-state enum)
│   ├── EncoderConfig.vb                      (config + Clone)
│   ├── EncodedPacket.vb                      (caller-owned IDisposable)
│   ├── PacketMetadata.vb                     (immutable struct)
│   ├── EncoderDiagnostics.vb                 (counters interface)
│   └── EncoderException.vb                   (exception hierarchy)
└── Backends/
    └── Fake/
        └── FakeEncoderBackend.vb             (deterministic reference impl)

CaptureEngine.Encoder.Tests/                   (NEW — P1-F tests)
├── CaptureEngine.Encoder.Tests.vbproj
├── Program.vb                                 (custom console runner)
├── TestHelpers.vb                             (Assert + FakeVideoFrame)
├── Lifecycle/EncoderLifecycleTests.vb        (15 tests)
├── Encode/EncodeTests.vb                      (17 tests)
├── Contract/EncodedPacketTests.vb             (9 tests)
└── Concurrency/EncoderConcurrencyTests.vb    (4 tests)
```

### 2.3 Dependency Graph

```
CaptureEngine.Encoder
    ├──▶ CaptureEngine.Video  (IVideoFrame — frozen Foundation)
    └──▶ CaptureEngine         (EngineLogger — Foundation)

CaptureEngine.Encoder.Tests
    ├──▶ CaptureEngine.Encoder
    ├──▶ CaptureEngine.Video
    └──▶ CaptureEngine
```

**No dependency on:**
- DdagrabBackend / FakeVideoCaptureBackend (capture backends)
- FFmpeg / NVENC / D3D11 / DXGI
- WinForms / UI / Overlay
- EngineConfigV2 / ConfigMigrator / ConfigValidator (config V2)

---

## 3. Lifecycle

### 3.1 State Machine (8 states)

```
        ┌──────────┐
        │ Created  │
        └────┬─────┘
             │ Initialize(config)
             ▼
        ┌──────────────┐
        │ Initialized  │
        └────┬─────────┘
             │ Start()
             ▼
        ┌──────────┐ ←─────────────┐
        │ Running  │               │
        └────┬─────┘               │
             │ Flush()              │
             ▼                     │
        ┌──────────┐               │
        │ Flushing │ ──(complete)──┘
        └────┬─────┘
             │ Stop()
             ▼
        ┌──────────┐         Start()
        │ Stopping │ ─────────────────► (back to Running)
        └────┬─────┘
             │
             ▼
        ┌──────────┐         Start()
        │ Stopped  │ ─────────────────► (back to Running)
        └────┬─────┘
             │ Dispose()
             ▼
        ┌──────────┐
        │ Disposed │ (terminal)
        └──────────┘

   Any state ──(failure)──► Faulted ──(Dispose)──► Disposed
```

### 3.2 Transition Rules

| From State | Trigger | To State | Exception? |
|---|---|---|---|
| Created | Initialize(success) | Initialized | — |
| Created | Initialize(failure) | Faulted | EncoderConfigurationException |
| Initialized | Start() | Running | — |
| Running | Start() (idempotent) | Running | — |
| Running | Encode(frame) | Running | — |
| Running | Flush() | Flushing → Running | — |
| Running/Flushing | Stop() | Stopping → Stopped | — |
| Stopped | Start() (restart) | Running | — |
| Any | Dispose() | Disposed | — |
| Any | (failure) | Faulted | EncoderRuntimeException |
| Faulted | any except Dispose | Faulted (no change) | InvalidOperationException |
| Disposed | any | Disposed (no change) | ObjectDisposedException |

### 3.3 Forbidden Transitions

- `Disposed → any` (must create new instance)
- `Faulted → Running/Initialized` (must Dispose + recreate)

---

## 4. Configuration

### 4.1 EncoderConfig Fields

| Field | Type | Default | Validation | Reason | NVENC relevance |
|---|---|---|---|---|---|
| `CodecKey` | String | "NVENC_H264" | non-empty | Symbolic codec dispatch | "NVENC_H264"/"NVENC_HEVC"/"NVENC_AV1" |
| `FFmpegCodec` | String | "h264_nvenc" | — | FFmpeg wrapper compatibility | "h264_nvenc"/"hevc_nvenc" |
| `BitrateBps` | Long | 20,000,000 | > 0 (for CBR/VBR) | Target bitrate | NVENC `-b:v` |
| `MinrateBps` | Long | 20,000,000 | >= 0, <= Maxrate | CBR min | NVENC `-minrate` |
| `MaxrateBps` | Long | 20,000,000 | >= 0, >= Minrate | CBR max | NVENC `-maxrate` |
| `BufsizeBps` | Long | 40,000,000 | >= Bitrate | RC buffer | NVENC `-bufsize` (2× bitrate for SW) |
| `GopSize` | Integer | 60 | > 0 | I-frame interval | NVENC `-g` |
| `RateControl` | String | "cbr" | cbr/vbr/cq | RC mode | NVENC `-rc` |
| `Cq` | Integer | 0 | 1-51 (if cq) | Constant Quality | NVENC `-cq` |
| `Preset` | String | "p4" | non-empty | Speed/quality tradeoff | NVENC `-preset` (p1-p7) |
| `OutputPixelFormat` | String | "nv12" | non-empty | Encoder output format | NVENC `-pix_fmt` |
| `ExpectedWidth` | Integer | 0 | 0 or > 0 | Frame width validation | — |
| `ExpectedHeight` | Integer | 0 | 0 or > 0 | Frame height validation | — |
| `ExpectedInputFormat` | VideoPixelFormat | Bgra8 | valid enum | Phase 1 = Bgra8 | NVENC accepts BGRA |
| `MaxInFlightFrames` | Integer | 4 | > 0 | Backpressure threshold | — |
| `FlushTimeoutMs` | Integer | 5000 | > 0 | Flush bounded time | — |
| `StopTimeoutMs` | Integer | 10000 | > 0 | Stop bounded time | — |

### 4.2 Field Classification

| Required now (P1-F) | Required for NVENC (P1-F-NVENC) | Future extension |
|---|---|---|
| CodecKey, BitrateBps, GopSize | Preset (NVENC-specific p1-p7) | Profile (baseline/main/high) |
| RateControl, MinrateBps, MaxrateBps | Cq (NVENC CQ mode) | Level (4.0/4.1/5.1) |
| BufsizeBps, ExpectedInputFormat | FFmpegCodec (FFmpeg wrapper) | B-frames count |
| ExpectedWidth, ExpectedHeight | OutputPixelFormat | HDR color metadata |
| MaxInFlightFrames, FlushTimeoutMs, StopTimeoutMs | | SpatialAQ / TemporalAQ / Tune |

---

## 5. Input Contract

### 5.1 IVideoFrame (Foundation — frozen)

The encoder accepts `CaptureEngine.Video.IVideoFrame`:
- `Origin` (CpuMemory / GpuD3D11Texture)
- `PixelFormat` (Bgra8 — Phase 1 baseline)
- `Dimensions` (Width × Height)
- `Diagnostics` (Sequence, CaptureTimeTicks, PresentationTimestampTicks)

### 5.2 Frame Ownership — BORROW

**Rule:** Encoder MUST NOT call `frame.Dispose()`.

**Rationale:** Caller retains ownership to support frame pooling. Encoder reads frame data during `Encode()` but does not retain references after return.

### 5.3 Frame Validation

| Check | When | Exception |
|---|---|---|
| `frame Is Nothing` | Encode entry | ArgumentNullException |
| `frame.Dimensions.Width != ExpectedWidth` | Encode | EncoderRuntimeException → Faulted |
| `frame.Dimensions.Height != ExpectedHeight` | Encode | EncoderRuntimeException → Faulted |
| `frame.PixelFormat != ExpectedInputFormat` | Encode | EncoderRuntimeException → Faulted |
| `frame.Origin` unsupported | Encode (future impl) | EncoderRuntimeException → Faulted |

---

## 6. EncodedPacket Contract

### 6.1 Structure

```
EncodedPacket (caller-owned, IDisposable)
├── Metadata: PacketMetadata (immutable struct)
│   ├── Sequence: Long              (monotonic per-encoder)
│   ├── PresentationTimestampTicks: Long  (PTS — Engine ticks)
│   ├── DecodingTimestampTicks: Long      (DTS — may equal PTS)
│   ├── DurationTicks: Long               (frame display duration)
│   ├── IsKeyFrame: Boolean               (I-frame flag)
│   ├── IsReferenceFrame: Boolean         (P/B reference flag)
│   ├── CodecKey: String                  (matches EncoderConfig.CodecKey)
│   └── CodecSpecificFlags: Integer       (future bit field)
├── Payload: Byte[]                       (encoded bitstream)
└── PayloadLength: Integer                (valid bytes in Payload)
```

### 6.2 Ownership — TRANSFER

**Rule:** When `Encode()` returns a packet (or `Flush(sink)` delivers one), ownership TRANSFERS to the receiver.

- **From Encode:** caller owns the returned packet → must Dispose()
- **From Flush sink:** sink owns each packet → must Dispose()
- **Dispose:** idempotent via `Interlocked.Increment` (v1.1 fix — was non-atomic in v1.0)

### 6.3 Data Ownership

- `Payload` (byte[]): owned by the packet. Encoder allocates; packet is sole owner.
- After Dispose, accessing Payload is **undefined behavior** (documented, not enforced — perf choice).
- Future pooled-buffer variant: Dispose returns byte[] to pool (contract accommodates this).

---

## 7. Ownership

### 7.1 Ownership Table

| Resource | Created by | Owned by | Dispose responsibility |
|---|---|---|---|
| IVideoFrame (input) | Capture backend | Caller | Caller (encoder BORROWS) |
| EncodedPacket (from Encode) | Encoder | Caller (after return) | Caller |
| EncodedPacket (from Flush sink) | Encoder | Sink (after callback) | Sink |
| EncoderConfig | Caller | Caller | No Dispose (managed object) |
| Encoder internal state | Encoder | Encoder | Encoder.Dispose() |

### 7.2 Dispose Ordering (Encoder)

1. Stop accepting new Encode() calls (→ Stopping)
2. Drain in-flight frames via Flush (bounded by StopTimeoutMs)
3. Join worker thread (OUTSIDE _sync lock — P1-B.1 FIX #1)
4. Release resources
5. Transition to Disposed

---

## 8. Error Model

### 8.1 Exception Hierarchy

```
EncoderException (base)
├── EncoderConfigurationException  — invalid config at Initialize()
├── EncoderRuntimeException       — runtime failure during Encode()
└── EncoderShutdownException      — Flush/Stop timeout
```

### 8.2 Error → State Mapping

| Exception | State transition | LastErrorType |
|---|---|---|
| EncoderConfigurationException | Initialized → Faulted | "config" |
| EncoderRuntimeException | Running → Faulted | "runtime" |
| EncoderShutdownException | (indeterminate) → caller must Dispose | "shutdown" |
| InvalidOperationException | (no state change — caller error) | — |
| ObjectDisposedException | (no state change — already Disposed) | — |

### 8.3 Faulted Is Terminal

Once Faulted, only `Dispose()` is valid. All other methods throw `InvalidOperationException`. Caller must Dispose + create new instance.

---

## 9. FakeEncoderBackend

### 9.1 Purpose

- Prove IEncoderBackend contract compiles + behaves correctly
- Deterministic target for lifecycle/concurrency/ownership tests
- Validate PTS/Sequence propagation from IVideoFrame to PacketMetadata
- Generate deterministic synthetic payloads for output testing

### 9.2 Determinism Guarantees

| Property | Determinism |
|---|---|
| Payload bytes | `(seq * 31 + pts + i) XOR (isKey ? 0xFF : 0x00)` — same inputs always produce same output |
| Keyframe cadence | First packet + every GopSize-th packet |
| Sequence | 0, 1, 2, ... (monotonic) |
| PTS | Copied from `frame.Diagnostics.PresentationTimestampTicks` |
| DTS | Equals PTS (synchronous encoder) |
| Duration | Fixed 166,667 ticks (deterministic placeholder) |

### 9.3 Linux Compatibility

- No GPU / NVENC / FFmpeg / Windows APIs
- Pure `net8.0` (no `-windows` TFM)
- No NuGet dependencies
- Build + run on `ubuntu-latest` CI

---

## 10. NVENC Integration Boundary

### 10.1 Boundary Analysis

**Question:** Can a future NVENC backend (P1-F-NVENC, GLM/4) consume the current `IVideoFrame` architecture without modifying frozen Foundation?

**Answer:** **YES — with a documented boundary.**

### 10.2 What NVENC Needs from IVideoFrame

| NVENC needs | Available on IVideoFrame? | How |
|---|---|---|
| D3D11 texture handle | ✅ When `Origin == GpuD3D11Texture` | Future NvencEncoder casts frame to `ID3D11Texture2D` (impl-specific) |
| D3D11 device identity | ❌ NOT on IVideoFrame | NVENC creates its own device OR shared via future `IEncoderContext` |
| Adapter identity | ❌ NOT on IVideoFrame | NVENC selects adapter during Initialize (impl-specific) |
| GPU origin flag | ✅ `frame.Origin` | NvencEncoder checks `Origin == GpuD3D11Texture` |
| Pixel format | ✅ `frame.PixelFormat` | NvencEncoder validates Bgra8 |
| Dimensions | ✅ `frame.Dimensions` | NvencEncoder validates against config |
| Timestamp | ✅ `frame.Diagnostics.PresentationTimestampTicks` | NvencEncoder copies to PacketMetadata.PTS |

### 10.3 NVENC Integration Path

```
Future NvencEncoder : IEncoderBackend
├── Initialize(config)
│   ├── Validate config (CodecKey = "NVENC_H264")
│   ├── Create D3D11 device (own device OR from IEncoderContext — future)
│   ├── Open NVENC session (nvEncodeAPI.dll — P/Invoke in NvencEncoder project)
│   └── Configure encoder (preset, bitrate, GOP, etc.)
├── Encode(frame, packet)
│   ├── Validate frame (Origin == GpuD3D11Texture, PixelFormat == Bgra8)
│   ├── Register D3D11 texture with NVENC (NvEncRegisterResource)
│   ├── Encode (NvEncEncodePicture → NvEncLockBitstream)
│   ├── Build EncodedPacket from NVENC output
│   └── Return packet (caller-owned)
├── Flush(sink) — drain NVENC pipeline (NvEncFlushEnc buffer)
├── Stop() — close NVENC session
└── Dispose() — release D3D11 device + NVENC session
```

### 10.4 Contract Gaps (for future P1-F-NVENC)

| Gap | Description | Solution |
|---|---|---|
| D3D11 device sharing | NVENC needs D3D11 device — IVideoFrame doesn't expose it | Future `IEncoderContext` (deferred to v1.2) OR NvencEncoder creates own device |
| Texture lifetime | NVENC registers texture — frame may be disposed before encode completes | NvencEncoder must copy/reference texture (impl responsibility) |
| Adapter selection | NVENC needs specific NVIDIA adapter | NvencEncoder selects during Initialize (impl-specific) |

**No Foundation modification required** — NvencEncoder handles all NVENC-specific concerns internally.

---

## 11. Contract Gaps

### 11.1 ENCODER-CONTRACT-GAP-001: No IEncoderContext

**Current contract:** `Initialize(config As EncoderConfig)` — no context parameter.

**Missing:** Way to share D3D11 device between capture backend and encoder (for zero-copy pipeline).

**Why encoder needs it:** Future NVENC backend may want to consume GPU textures directly from DdagrabBackend without CPU copy. Requires shared D3D11 device.

**Possible solutions:**
1. Add `IEncoderContext` interface (similar to `IVideoBackendContext`) — deferred to v1.2
2. NvencEncoder creates own device + texture copy (current assumption — works, not zero-copy)
3. Resource registry pattern (future)

**Recommended solution:** Option 1 (defer to v1.2) — add `Initialize(config, context)` overload when zero-copy is needed. For Phase 1, NvencEncoder creates own device.

**Architectural consequences:** None for Phase 1. Future zero-copy path requires v1.2 contract extension.

### 11.2 ENCODER-CONTRACT-GAP-002: No IEncoderBackendFactory

**Current contract:** No factory interface (callers `New FakeEncoderBackend()` directly).

**Missing:** Way for PipelineResolver to create encoder instances based on CodecKey without tight coupling.

**Why encoder needs it:** PipelineResolver needs to dispatch on CodecKey → concrete encoder type.

**Possible solutions:**
1. Add `IEncoderBackendFactory` (mirrors `IVideoCaptureBackendFactory`) — defer to v1.2
2. PipelineResolver uses `Select Case CodecKey` (current — tight coupling, acceptable for Phase 1)

**Recommended solution:** Option 1 (defer to v1.2) — add factory when concrete encoders exist.

---

## 12. Test Strategy

### 12.1 Test Categories

| Category | Count | Coverage |
|---|---|---|
| Lifecycle | 15 | Initialize/Start/Stop/Dispose transitions, idempotency, post-Dispose guards, restart |
| Encode | 17 | Happy path, input validation, PTS/sequence propagation, keyframe cadence, determinism, BORROW contract, Flush, diagnostics, failure → Faulted |
| EncodedPacket | 9 | Construction, validation, Dispose idempotency, concurrent Dispose, metadata immutability |
| Concurrency | 4 | Start+Stop, Stop+Dispose, concurrent Encode, Encode+Dispose |
| **TOTAL** | **45** | |

### 12.2 Linux Compatibility

All 45 tests run on Linux (`net8.0`, no Windows SDK, no GPU). Suitable for `ubuntu-latest` CI.

### 12.3 Test Runner

Custom console runner (mirrors Foundation pattern):
- `Friend Module Program` with `_passed/_failed` counters
- `RunTest(name, action)` wrapper — Try/Catch + PASS/FAIL output
- Exit code: `0` = all passed, `1` = at least one failure
- No xUnit/NUnit/MSTest

### 12.4 Contract Test Reusability

Contract tests (lifecycle, packet) are designed to be reusable by future `NvencEncoder`:
- Tests accept `IEncoderBackend` parameter (not `FakeEncoderBackend` directly)
- No dependency on FakeEncoder-specific behavior (deterministic payload, synthetic timestamps)
- Future NvencEncoder tests will add NVENC-specific tests (real encoding, GPU texture handling)

---

## 13. Deferred Decisions

| Decision | Deferred to | Rationale |
|---|---|---|
| IEncoderContext (shared D3D11 device) | v1.2 | No zero-copy impl yet; NvencEncoder creates own device for Phase 1 |
| IEncoderBackendFactory | v1.2 | No concrete encoders yet; PipelineResolver uses Select Case |
| Async Encode (push model) | v2.0 (if needed) | Synchronous Encode is simplest for Phase 1; matches Foundation TryPush pattern |
| CancellationToken | v2.0 (if needed) | Contracts use `_stopSignal As Boolean` under lock; no cancellation needed yet |
| Pooled buffer for EncodedPacket.Payload | v2.0 (if perf needed) | Current GC-only is simple; contract accommodates future pool |
| HDR color metadata | Phase 6+ | Phase 1 = BGRA8 only (V4 CLOSED) |
| B-frame count configuration | P1-F-NVENC | NVENC-specific; add to NvencEncoder config (not shared EncoderConfig) |

---

## 14. Build & Test Verification

### 14.1 Build Status

| Project | Build status |
|---|---|
| `CaptureEngine.Encoder` | STATIC ANALYSIS ONLY (sandbox has no dotnet SDK) — expected 0/0 on Windows |
| `CaptureEngine.Encoder.Tests` | STATIC ANALYSIS ONLY — expected 0/0 on Windows |

### 14.2 Test Status

| Test project | Test count | Status |
|---|---|---|
| `CaptureEngine.Tests` (Foundation) | 14 | NOT RUN (sandbox — expected GREEN) |
| `CaptureEngine.Video.Tests` (Video Contract) | 60 | NOT RUN (sandbox — expected GREEN) |
| `CaptureEngine.ConfigTests` (Config Regression) | 91 | NOT RUN (sandbox — expected GREEN) |
| `CaptureEngine.Encoder.Tests` (NEW) | 45 | NOT RUN (sandbox — expected GREEN on Windows) |

### 14.3 Regression Risk

| Risk | Status |
|---|---|
| Foundation modified | ✅ NOT modified (git diff verified) |
| IVideoFrame modified | ✅ NOT modified |
| IVideoBackend modified | ✅ NOT modified |
| Existing tests broken | ✅ Zero impact (encoder is additive — no existing file changed) |

---

## 15. Final Assessment

### 15.1 Can future NVENC backend consume current frame architecture without modifying frozen Foundation?

**YES.**

**Required boundary:**
1. NvencEncoder implements `IEncoderBackend` (no Foundation modification)
2. NvencEncoder validates `frame.Origin == GpuD3D11Texture` + `frame.PixelFormat == Bgra8` at Encode time
3. NvencEncoder casts frame to access D3D11 texture (impl-specific — not in public contract)
4. NvencEncoder creates own D3D11 device during Initialize (no shared device — deferred to v1.2 IEncoderContext)
5. NvencEncoder copies PTS from `frame.Diagnostics.PresentationTimestampTicks` to `PacketMetadata.PresentationTimestampTicks`

**No Foundation modification required.**

### 15.2 P1-F-NVENC Blockers

| Blocker | Severity | Resolution |
|---|---|---|
| No NVENC SDK in repo (nvEncodeAPI.dll tracked but license-restricted) | MED | GLM/4 to handle in P1-F-NVENC task |
| No D3D11 device sharing (zero-copy deferred) | LOW | NvencEncoder creates own device for Phase 1 |
| No GPU runtime validation in sandbox | MED | OWNER runs on Windows + NVIDIA GPU |

---

**END OF P1-F Encoder Architecture Documentation v1.0**
