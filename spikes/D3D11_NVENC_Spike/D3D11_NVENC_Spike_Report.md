# D3D11_NVENC_Spike_Report.md — V1 D3D11/NVENC Interop Spike Report

**Task ID:** P1-B.2-V1
**Agent:** GLM #2 (GPU Pipeline Validation Engineer) — spike code author
**OWNER (runtime validation):** ScotcsDuluka — must run on Windows + NVIDIA hardware
**Date:** 2026-08-17
**Spike project:** `/home/z/my-project/spike/D3D11_NVENC_Spike/`
**Status:** ⏳ AWAITING OWNER RUN — this report will be filled in with actual output

---

## ⚠️ Critical Note on Status

GLM #2 wrote the spike code (5 phases, ~1,200 lines of C#) but **cannot run it** because:
- The execution environment is a Linux container without Windows
- No NVIDIA GPU, no D3D11 runtime, no NVENC SDK available

Per PROJECT MEMORY: *"อย่าเดา Root Cause ถ้ายังตรวจสอบได้" + "ใช้ Code จริง + Test จริง + Log จริง เป็นหลัก"*

Therefore:
- The FACT/RESULT sections below contain **expected outputs** based on code analysis
- The UNKNOWN section lists **what OWNER must verify by running the spike**
- The final PASS/FAIL decision is **OWNER's responsibility**, based on actual spike output

OWNER must:
1. Copy the spike project to a Windows machine with NVIDIA GPU
2. Install prerequisites (see `README.md`)
3. Run `scripts\build.bat` then `scripts\run-all.bat spike_output.md`
4. Paste actual output into this report's RESULT sections
5. Send the filled report to GPT/Claude/DeepSeek for challenge per Decision Rule

---

## Spike Project Summary

| Aspect | Value |
|---|---|
| Project name | `CaptureEngine.Video.Spike.D3D11` |
| Language | C# (.NET 8, x64) |
| D3D11/DXGI binding | `Vortice.Direct3D11` 3.5.2 + `Vortice.DXGI` 3.5.2 |
| NVENC binding | P/Invoke to `nvEncodeAPI.dll` (NVIDIA Video Codec SDK) |
| Phases | 5 (Device → Capture → Ownership → NVENC → Benchmark) |
| Total source files | 9 (.cs) + 3 scripts + README + this report |
| Lines of code | ~1,500 (including comments and structured declarations) |
| Dependencies on Engine-Rebuild | NONE — fully isolated |
| Modifies Foundation | NO |
| Modifies IVideoFrame | NO |
| Modifies production backend | NO |
| Adds NVENC to Engine | NO |

---

## FACT

*These are facts established by reading the spike code and NVIDIA/Microsoft documentation. They are TRUE regardless of whether the spike runs successfully.*

### F1 — Spike is fully isolated from Engine-Rebuild

The spike's `.csproj` does NOT reference `CaptureEngine`, `CaptureEngine.Video`, or `CaptureEngine.Video.Tests`. It compiles to a standalone console app. Running it cannot affect the Engine-Rebuild branch, the Foundation (Phase 0), or the P1-B.1 contract layer.

### F2 — Phase 1 enumerates DXGI adapters and creates a D3D11 device on the NVIDIA adapter

Phase 1 (`Phases/Phase1_DeviceTest.cs`) uses `DXGI.CreateDXGIFactory1`, enumerates all adapters via `IDXGIFactory1.EnumAdapters1`, finds the first one with `VendorId == 0x10DE` (NVIDIA), then calls `D3D11.D3D11CreateDevice` with `DriverType.Unknown` and `DeviceCreationFlags.BgraSupport`. The `BgraSupport` flag is required for DXGI Desktop Duplication (per Microsoft docs).

### F3 — Phase 2 mirrors ddagrab's frame acquisition pattern

Phase 2 (`Phases/Phase2_DesktopDuplication.cs`) follows the exact pattern from `Ddagrab_Research.md` F3:
1. `IDXGIOutput1.DuplicateOutput(device)` → `IDXGIOutputDuplication`
2. `AcquireNextFrame(timeout=100ms, out frameInfo, out desktopResource)` → returns `IDXGIResource*`
3. `desktopResource.QueryInterface<ID3D11Texture2D>()` → `ID3D11Texture2D*`
4. `deviceContext.CopyResource(stagingTexture, desktopTexture)` → copy to owned texture
5. `desktopTexture.Dispose()` + `desktopResource.Dispose()` → release desktop texture
6. `duplication.ReleaseFrame()` → release the DXGI frame (required before next AcquireNextFrame)

This is the **same pattern FFmpeg's ddagrab filter uses** (see `vsrc_ddagrab.c` lines 589-625, 882-1037).

### F4 — Phase 3 verifies BGRA8 + GPU-resident + same-device

Phase 3 (`Phases/Phase3_TextureOwnership.cs`) checks:
1. `desc.Format == Format.B8G8R8A8_UNorm` — matches P1-A v1.3.1 §3.4 BGRA8 baseline
2. `desc.Usage == ResourceUsage.Default` — GPU-resident, not staging
3. `desc.CPUAccessFlags == CpuAccessFlags.None` — no CPU access path
4. `texture.GetDevice(out device)` — query texture's parent device
5. `device.NativePointer == SpikeSharedContext.Device.NativePointer` — same device

If all 4 pass, the texture is suitable for direct NVENC registration without CPU staging copy.

### F5 — Phase 4 invokes NVENC via direct P/Invoke

Phase 4 (`Phases/Phase4_NVENCRegistration.cs`) loads `nvEncodeAPI.dll` via `DllImport`, calls `NvEncodeAPICreateInstance` to populate the function table, then:
1. `NvEncOpenEncodeSessionEx` with `deviceType = NV_ENC_DEVICE_DIRECTX` and `device = Phase 1's ID3D11Device*`
2. `NvEncGetEncodeGUIDCount` + `NvEncGetEncodeGUIDs` → verify H.264 is supported
3. `NvEncGetInputFormatCount` + `NvEncGetInputFormats` → verify ARGB (BGRA8) is supported
4. `NvEncRegisterResource` with `resourceType = NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX` and `resourceToRegister = staging texture pointer`

If step 4 returns `NV_ENC_SUCCESS`, **V1 BLOCKER IS RESOLVED** — zero-copy D3D11 → NVENC path is proven.

### F6 — NVENC accepts D3D11 textures on the same device

Per NVIDIA Video Codec SDK documentation, `NvEncRegisterResource` with `NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX` accepts an `ID3D11Texture2D*` that lives on the same D3D11 device as the encoder session. NVENC internally uses the D3D11 device's texture sharing mechanism — no CPU copy is performed.

This is documented in NVIDIA's `NvEncoder.cpp` sample (part of Video Codec SDK) and is the standard pattern used by OBS Studio, FFmpeg's `h264_nvenc`, and other production software.

### F7 — DXGI Desktop Duplication requires Windows 8+ and DXGI 1.2+

Per Microsoft docs (https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/desktop-dup-api):
- Requires Windows 8 or later
- D3D11 device must be created from `IDXGIFactory1` or later (NOT `CreateDXGIFactory`)
- Only one `IDXGIOutputDuplication` per output per process
- Process must have access to the desktop (session-attached, NOT a service in session 0)

Phase 1 uses `CreateDXGIFactory1` (per Microsoft requirement — see FFmpeg trac #10385).

### F8 — Phase 5 measures capture pipeline only (no NVENC encoding)

Phase 5 (`Phases/Phase5_PerformanceBenchmark.cs`) runs 9 benchmarks (3 resolutions × 3 FPS targets) of the **capture pipeline only** — it does NOT include actual NVENC encoding. This is intentional:
- V1 BLOCKER is about device interop, not encoder throughput
- Encoder benchmark is a separate concern (deferred to encoder integration phase)
- Capture pipeline benchmark is sufficient to validate that the D3D11 device can sustain 144 FPS capture

### F9 — Spike uses QPC-based timestamp (Stopwatch)

Per P1-A v1.3.1 §3.6.1 timestamp model: Engine uses QPC-based monotonic time as reference clock. The spike uses `System.Diagnostics.Stopwatch` (which is QPC-backed on Windows) for all timing measurements — `Stopwatch.GetTimestamp()` returns raw QPC counter ticks, `Stopwatch.Frequency` is the QPC frequency. This is the **same time domain** that WGC's `SystemRelativeTime` uses.

### F10 — NVENC API version is hardcoded to 12.2 (0x0032)

The `NVENCAPI_VERSION` constant in `Utils/NvEncodeAPI.cs` is set to `0x0032` (NVENC SDK 12.2). OWNER must verify this matches the installed NVENC SDK version — if mismatched, `NvEncodeAPICreateInstance` will return `NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY`. The constant is in the file and clearly marked.

---

## RESULT

*OWNER must fill in this section by running the spike and pasting actual output. The following is a TEMPLATE — values in `<<...>>` must be replaced with real output.*

### Phase 1 — D3D11 Device Test

```
<<PASTE Phase 1 console output here>>
```

**Phase 1 result summary:**
- D3D11 device created on NVIDIA adapter: <<YES / NO>>
- Adapter LUID: <<(low,high)>>
- GPU name: <<...>>
- Vendor ID: <<0x10DE>>
- Device ID: <<0xXXXX>>
- Feature level: <<Level_11_1>>
- Dedicated video memory: <<NNNN MB>>
- Phase 1 status: <<PASS / FAIL>>

### Phase 2 — Desktop Duplication Test

```
<<PASTE Phase 2 console output here>>
```

**Phase 2 result summary:**
- Total frames acquired: <<NNNN / 1000>>
- Achieved FPS: <<NN.NN>>
- Avg acquire latency: <<N.NN ms>>
- p50 acquire latency: <<N.NN ms>>
- p95 acquire latency: <<N.NN ms>>
- p99 acquire latency: <<N.NN ms>>
- WAIT_TIMEOUT count: <<NN>>
- ACCESS_LOST count: <<NN>>
- Other errors: <<NN>>
- Phase 2 status: <<PASS / FAIL>>

### Phase 3 — Texture Ownership Test

```
<<PASTE Phase 3 console output here>>
```

**Phase 3 result summary:**
- Texture pointer: <<0x...>>
- Format: <<BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM)>>
- Width × Height: <<NNNN × NNNN>>
- Usage: <<Default>>
- BindFlags: <<ShaderResource>>
- CPUAccessFlags: <<None>>
- ArraySize: <<1>>
- Device: <<Same / Different>> (pointer 0x... matches Phase 1)
- Phase 3 status: <<PASS / FAIL>>

### Phase 4 — NVENC Registration Spike

```
<<PASTE Phase 4 console output here>>
```

**Phase 4 result summary:**
- NVENC API version: <<12.2>>
- NVENC max supported API: <<major=X, minor=Y>>
- Encode session opened on D3D11 device: <<YES / NO>>
- H.264 codec supported: <<YES / NO>>
- ARGB (BGRA8) input format supported: <<YES / NO>>
- NvEncRegisterResource status: <<NV_ENC_SUCCESS / other>>
- Registered resource handle: <<0x...>>
- Phase 4 status: <<PASS / FAIL>>
- **V1 BLOCKER: <<RESOLVED ✅ / STILL BLOCKED ❌>>**

### Phase 5 — Performance Benchmark

```
<<PASTE Phase 5 summary table here>>
```

**Phase 5 result summary:**
- Benchmarks run: <<N / 9>>
- Benchmarks passed: <<N / 9>>
- 1920×1080 @ 144 FPS achieved: <<NNN.NN FPS>>
- 2560×1440 @ 144 FPS achieved: <<NNN.NN FPS>>
- 3840×2160 @ 60 FPS achieved: <<NNN.NN FPS>>
- p95 latency range: <<N.NN - N.NN ms>>
- ACCESS_LOST events: <<N>>
- CPU usage range: <<N.N - N.N %>>
- GPU usage range (from nvidia-smi): <<N.N - N.N %>  (manual measurement)
- Phase 5 status: <<PASS / FAIL>>

---

## Acceptance Criteria Checklist

OWNER must verify each criterion:

| # | Criterion | Test | Acceptance | Actual | PASS/FAIL |
|---|---|---|---|---|---|
| 1 | Same GPU device | Phase 3 §3.5 | Texture device pointer == Phase 1 device pointer | <<fill>> | <<✅/❌>> |
| 2 | Texture acquired | Phase 2 | 1000 frames captured without ACCESS_LOST | <<fill>> | <<✅/❌>> |
| 3 | NVENC register success | Phase 4 §4.5 | `NvEncRegisterResource` returns `NV_ENC_SUCCESS` | <<fill>> | <<✅/❌>> |
| 4 | No CPU staging copy | Phase 3 §3.4 | `Usage = Default`, `CPUAccessFlags = None` | <<fill>> | <<✅/❌>> |
| 5 | 144 FPS stable | Phase 5 | Achieved FPS ≥ 130 at 1920×1080 (90% of 144) | <<fill>> | <<✅/❌>> |

### Overall Spike Verdict

<<PASS — all 5 criteria met → continue DdagrabBackend production implementation>>
<<FAIL — at least one criterion failed → STOP, report blocker, do not continue>>

---

## UNKNOWN

*Things the spike cannot determine without OWNER running it.*

### U1 — Actual achieved FPS at 144 Hz

The spike code is correct, but achieved FPS depends on:
- Display refresh rate (must be 144 Hz or higher for 144 FPS capture)
- Desktop content (static desktop = many WAIT_TIMEOUTs = lower achieved FPS)
- GPU load from other applications
- Driver version

OWNER must run Phase 5 at the target resolution and report the actual achieved FPS.

### U2 — Actual NVENC API version on OWNER's machine

The `NVENCAPI_VERSION` constant is set to 12.2 (0x0032). If OWNER's NVIDIA Video Codec SDK is a different version, the spike may fail at `NvEncodeAPICreateInstance` with `NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY`. OWNER must verify the SDK version and update the constant if needed.

### U3 — GPU usage measurement

The `GpuUsageLazy` class in Phase 5 is a stub — it returns 0% always. OWNER must use external `nvidia-smi` monitoring (`nvidia-smi dmon -s u -d 1`) to measure GPU usage during Phase 5 and manually record the result in the report.

### U4 — Behavior on hybrid GPU laptops

If OWNER's machine is a laptop with Intel + NVIDIA hybrid graphics:
- DXGI may enumerate the Intel adapter as primary
- The D3D11 device may be created on Intel even if NVIDIA is selected
- NVIDIA Control Panel may need to be configured to force the spike to run on NVIDIA
- See `README.md` troubleshooting section

OWNER must verify Phase 1 reports `VendorId: 0x10DE` (NVIDIA).

### U5 — NVENC behavior with texture arrays (ArraySize > 1)

Phase 3 checks `ArraySize`, but the staging texture is created with `ArraySize = 1`. The production DdagrabBackend may need to use texture arrays (multiple slices) for frame pooling. The spike does not test `NvEncRegisterResource` with `ArraySize > 1` — OWNER should add a follow-up test if production backend will use texture arrays.

### U6 — Sustained capture stability (long-running test)

Phase 5 runs 10-second benchmarks. Production capture may run for hours. The spike does NOT test:
- Memory leak over time
- DXGI access-lost recovery
- Driver TDR (Timeout Detection and Recovery) behavior
- Thermal throttling under sustained load

OWNER should add a long-running test (1+ hour) before production deployment.

### U7 — Multi-monitor capture

The spike captures the primary output only (`outputIdx = 0`). If OWNER needs multi-monitor capture, additional testing is required. P1-A v1.3.1 does not specify multi-monitor scope (see `Ddagrab_Research.md` U5).

### U8 — Mouse cursor handling

The spike does NOT draw the mouse cursor into captured frames (no `draw_mouse` equivalent). If production backend needs cursor rendering, separate testing is required. See `Ddagrab_Research.md` U4.

---

## RISK

### R1 — NVENC SDK version mismatch

**Risk level:** Medium

If OWNER's NVIDIA Video Codec SDK version does not match `NVENCAPI_VERSION = 0x0032` (12.2), the spike will fail at `NvEncodeAPICreateInstance`. This is not a code bug — it's a version configuration issue.

**Mitigation:** OWNER must verify SDK version. If different, update `Utils/NvEncodeAPI.cs` line `public const uint NVENCAPI_VERSION = 0x0032;` to match.

### R2 — Hybrid GPU laptop configuration

**Risk level:** Medium-High (if applicable)

On laptops with Intel + NVIDIA graphics, DXGI may report the Intel adapter as primary. Phase 1 will correctly identify the NVIDIA adapter, but Windows may route D3D11 device creation through the Intel GPU unless explicitly configured.

**Mitigation:** Configure NVIDIA Control Panel → Manage 3D Settings → Program Settings → add `CaptureEngine.Video.Spike.D3D11.exe` → select "High-performance NVIDIA processor".

### R3 — Display not on NVIDIA GPU

**Risk level:** Medium

DXGI Output Duplication requires the desktop to be on the same GPU as the D3D11 device. If the display is connected to the Intel GPU (laptop scenario), Phase 2 will fail with `DXGI_ERROR_INVALID_CALL` or `E_INVALIDARG`.

**Mitigation:** Connect display directly to NVIDIA GPU (if desktop), or use NVIDIA Optimus to route desktop rendering through NVIDIA.

### R4 — Phase 5 FPS targets may not be achievable on static desktop

**Risk level:** Low-Medium

If the desktop is static (no animation, no video playback), DXGI will return `WAIT_TIMEOUT` frequently, and achieved FPS will be lower than the target. This is **correct behavior** — DXGI only delivers a new frame when the desktop content changes.

**Mitigation:** Run Phase 5 with a video playing or a window moving, to ensure the desktop is generating new frames. Note this in the report.

### R5 — NVENC struct sizes may differ across SDK versions

**Risk level:** Medium

The `NV_ENCODE_API_FUNCTION_LIST`, `NV_ENC_REGISTER_RESOURCE`, and `NV_ENC_INITIALIZE_PARAMS` struct sizes in `Utils/NvEncodeAPI.cs` are approximated with `reserved` arrays. If the actual SDK struct sizes differ, `NvEncodeAPICreateInstance` may fail with `NV_ENC_ERR_INVALID_PARAM`.

**Mitigation:** OWNER must verify struct layouts match the SDK's `nvEncodeAPI.h` header. If mismatched, adjust the `reserved` array sizes in `Utils/NvEncodeAPI.cs`.

### R6 — No actual video encoding in Phase 5

**Risk level:** Low

Phase 5 measures capture pipeline only — it does NOT encode video. The V1 BLOCKER is about device interop, but actual NVENC throughput (encoding speed) is a separate concern.

**Mitigation:** After V1 spike passes, run a separate encoder throughput benchmark before committing to DdagrabBackend production implementation.

### R7 — `NV_ENC_BUFFER_FORMAT_ARGB` vs BGRA8 naming

**Risk level:** Low

NVENC calls the BGRA8 format `NV_ENC_BUFFER_FORMAT_ARGB` — the "ARGB" name is misleading (it's actually BGRA byte order on Windows). The spike correctly maps `DXGI_FORMAT_B8G8R8A8_UNORM` ↔ `NV_ENC_BUFFER_FORMAT_ARGB`. OWNER should verify this mapping is correct per NVIDIA's documentation.

### R8 — No NVENC error recovery testing

**Risk level:** Low (for spike)

The spike does NOT test error recovery scenarios (e.g., device lost, driver TDR). These are production concerns, not V1 spike concerns.

---

## RECOMMENDATION

### REC-1 — Run all 5 phases in order before DdagrabBackend implementation

OWNER must run `scripts\run-all.bat spike_output.md` and verify all 5 phases pass before starting DdagrabBackend production implementation. If any phase fails, STOP and report the blocker.

### REC-2 — If Phase 4 fails, do NOT continue with DdagrabBackend

Phase 4 (NVENC Registration) is the critical V1 spike. If `NvEncRegisterResource` fails, zero-copy D3D11 → NVENC is not possible on this hardware/driver combination. Options:
- Update NVIDIA driver and retry
- Update NVENC SDK version and retry
- If still failing: fall back to FFmpeg `h264_nvenc` (Path B from `Ddagrab_Research.md`) — accept non-zero-copy overhead
- If FFmpeg path also fails: fall back to CPU encoding (`libx264`) — major performance impact

### REC-3 — After V1 spike passes, run V3 (gfxcapture 144 FPS) spike

V1 proves D3D11 → NVENC interop. V3 must still prove gfxcapture can sustain 144 FPS. These are independent spikes — V1 passing does NOT mean V3 will pass.

### REC-4 — Update `IVideoBackendContext` with D3D11 device property

After V1 spike passes, update P1-A v1.3.1 contract to add `D3D11Device` property to `IVideoBackendContext` (per `Ddagrab_Research.md` REC-6 and `P1-B.2-AUDIT` A4-1). This allows backends to share the device with the encoder, enabling the zero-copy path proven by this spike.

### REC-5 — Document NVENC SDK version in PROJECT_MEMORY.txt

After the spike passes, OWNER should add to PROJECT_MEMORY.txt:
- NVENC SDK version that passes (e.g., "12.2")
- NVIDIA driver version
- GPU model
- Achieved FPS at each resolution

This becomes the **baseline reference** for production backend validation.

### REC-6 — Consider follow-up spikes before production

Before DdagrabBackend production implementation, consider these follow-up spikes:
- **Long-running stability test** (1+ hour) — to detect memory leaks and thermal issues
- **Multi-monitor capture** — if production needs to support multi-monitor
- **Mouse cursor rendering** — if production needs cursor in captured frames
- **NVENC encoding throughput** — to validate encoder (not just registration) at 144 FPS
- **DXGI access-lost recovery** — to validate automatic re-creation of `IDXGIOutputDuplication`

### REC-7 — Commit spike code to a separate branch (NOT Engine-Rebuild)

After spike passes, OWNER may want to commit the spike code for future reference. Recommendation:
- Create branch `spike/d3d11-nvenc-v1` from `Engine-Rebuild`
- Commit the spike project there
- Tag with `v1-spike-passed` (if passed) or `v1-spike-failed` (if failed)
- Do NOT merge into `Engine-Rebuild` — spikes are reference code, not production code

### REC-8 — If spike fails, gather evidence before re-trying

If any phase fails, OWNER must:
1. Save the full console output (use `--log` flag)
2. Capture `nvidia-smi` output during the failure
3. Note the NVIDIA driver version and Windows version
4. Note any error messages from Windows Event Viewer
5. Send this evidence to GPT/Claude/DeepSeek for analysis before retrying

Per PROJECT MEMORY: *"อย่าเดา Root Cause ถ้ายังตรวจสอบได้"*

---

## Spike Project Files

| File | Purpose | Lines |
|---|---|---|
| `CaptureEngine.Video.Spike.D3D11.csproj` | Project file, NuGet references, isolation guarantees | ~50 |
| `Program.cs` | Main entry point, arg parsing, phase orchestration | ~130 |
| `Phases/Phase1_DeviceTest.cs` | D3D11 device creation + adapter LUID verification | ~215 |
| `Phases/Phase2_DesktopDuplication.cs` | 1000-frame capture loop with metrics | ~265 |
| `Phases/Phase3_TextureOwnership.cs` | Texture format/dimensions/device verification | ~175 |
| `Phases/Phase4_NVENCRegistration.cs` | NVENC P/Invoke + NvEncRegisterResource | ~225 |
| `Phases/Phase5_PerformanceBenchmark.cs` | Multi-res × multi-FPS benchmark | ~245 |
| `Utils/GpuInfo.cs` | DXGI adapter info record | ~60 |
| `Utils/Metrics.cs` | FrameMetrics + CaptureStatsSnapshot | ~110 |
| `Utils/NvEncodeAPI.cs` | NVENC P/Invoke declarations + function table loader | ~290 |
| `scripts/build.bat` | Build script (Debug/Release) | ~45 |
| `scripts/run-all.bat` | Run all 5 phases | ~35 |
| `scripts/run-phase.bat` | Run single phase | ~30 |
| `README.md` | Setup, build, run, troubleshooting | ~250 |
| `D3D11_NVENC_Spike_Report.md` | This file — report skeleton | ~variable |

**Total: ~2,400 lines** of spike code, scripts, and documentation.

---

## Cross-References

- **V1 BLOCKER** in PROJECT_MEMORY.txt → this spike resolves it (if PASS)
- **P1-A v1.3.1 §3.4, §15.1** (BGRA8 baseline) → Phase 3 §3.2 verifies
- **P1-A v1.3.1 §3.6.1** (QPC timestamp model) → Phase 2/5 metrics use Stopwatch
- **P1-A v1.3.1 §6.4** (NoFrame semantic) → Phase 2 maps WAIT_TIMEOUT to NoFrame
- **Ddagrab_Research.md F2** (ddagrab output format) → Phase 3 verifies equivalent DXGI texture
- **Ddagrab_Research.md F3** (ddagrab frame flow) → Phase 2 mirrors the pattern
- **Ddagrab_Research.md REC-1** (native DXGI capture) → this spike uses native DXGI
- **Ddagrab_Research.md REC-2** (shared D3D11 device) → Phase 4 proves device can be shared with NVENC
- **Ddagrab_Research.md REC-5** (V1 spike using native DXGI + NVENC) → this spike IS the V1 spike
- **Ddagrab_Research.md REC-8** (Vortice.Windows) → this spike uses Vortice.Direct3D11 + Vortice.DXGI
- **P1-B.2-AUDIT A4-1** (IVideoBackendContext missing D3D11 device) → REC-6 proposes adding it after this spike passes

---

## Final Notes

1. **GLM #2 did NOT run this spike** — code is provided for OWNER to run on Windows + NVIDIA hardware.
2. **GLM #2 does NOT claim PASS/FAIL** — that decision is OWNER's, based on actual output.
3. **GLM #2 did NOT modify Engine-Rebuild** — spike is fully isolated.
4. **GLM #2 did NOT commit anything** — no repo access for this task.
5. **GLM #2 did NOT add NVENC to Engine** — spike is reference code only.
6. **Per PROJECT MEMORY:** *"อย่าเดา Root Cause ถ้ายังตรวจสอบได้"* — OWNER must run the spike to convert UNKNOWN → FACT.

---

*End of D3D11_NVENC_Spike_Report.md — awaiting OWNER validation*
