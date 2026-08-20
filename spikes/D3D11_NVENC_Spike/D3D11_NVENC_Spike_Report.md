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

Log

Microsoft Windows [Version 10.0.26340.9212]
(c) Microsoft Corporation. All rights reserved.

C:\My Project\NVIDIA-Shadowplay>cd spike\D3D11_NVENC_Spike
The system cannot find the path specified.

C:\My Project\NVIDIA-Shadowplay>
C:\My Project\NVIDIA-Shadowplay>scripts\build.bat
'scripts\build.bat' is not recognized as an internal or external command,
operable program or batch file.

C:\My Project\NVIDIA-Shadowplay>
C:\My Project\NVIDIA-Shadowplay>scripts\run-all.bat spike_output.md
'scripts\run-all.bat' is not recognized as an internal or external command,
operable program or batch file.

C:\My Project\NVIDIA-Shadowplay>cd spikes\D3D11_NVENC_Spike

C:\My Project\NVIDIA-Shadowplay\spikes\D3D11_NVENC_Spike>scripts\build.bat

============================================
 Building CaptureEngine.Video.Spike.D3D11
 Configuration: Debug
============================================

Restore complete (0.7s)

Build succeeded in 1.3s
Restore complete (0.5s)
  CaptureEngine.Video.Spike.D3D11 net8.0-windows succeeded with 2 warning(s) (4.0s) → bin\x64\Debug\net8.0-windows\CaptureEngine.Video.Spike.D3D11.dll
    C:\My Project\NVIDIA-Shadowplay\spikes\D3D11_NVENC_Spike\Phases\Phase4_NVENCRegistration.cs(122,14): warning CS0219: The variable 'hevcSupported' is assigned but its value is never used
    C:\My Project\NVIDIA-Shadowplay\spikes\D3D11_NVENC_Spike\Phases\Phase4_NVENCRegistration.cs(123,14): warning CS0219: The variable 'av1Supported' is assigned but its value is never used

Build succeeded with 2 warning(s) in 5.3s

============================================
 Build SUCCESS
============================================
 Output: C:\My Project\NVIDIA-Shadowplay\spikes\D3D11_NVENC_Spike\scripts\\..\bin\x64\Debug\net8.0-windows\

 Next steps:
   1. Copy nvEncodeAPI.dll from NVIDIA Video Codec SDK's Lib\x64\ folder
      to the output directory above (next to CaptureEngine.Video.Spike.D3D11.exe)
   2. Run scripts\run-all.bat
============================================


C:\My Project\NVIDIA-Shadowplay\spikes\D3D11_NVENC_Spike>scripts\run-all.bat
============================================================
 CaptureEngine.Video.Spike.D3D11
 P1-B.2-V1 — D3D11/NVENC Interop Spike
 Branch: Engine-Rebuild (spike — does NOT modify repo)
 Foundation baseline: 82d792ab (untouched)
 Target commit: 39da0640 (untouched)
============================================================

============================================================
 Phase 1 — D3D11 Device Test
============================================================

[1.1] Enumerating DXGI adapters...
  Adapter [0] NVIDIA GeForce GTX 1080 Ti (NVIDIA:0x10de:0x1b06) LUID=(0000c78b,00000000) Video=11107MB Sys=0MB Shared=8109MB
  Adapter [1] NVIDIA GeForce GTX 1080 Ti (NVIDIA:0x10de:0x1b06) LUID=(000e9175,00000000) Video=11107MB Sys=0MB Shared=8109MB
  Adapter [2] NVIDIA GeForce GTX 1080 Ti (NVIDIA:0x10de:0x1b06) LUID=(00018f6a,00000000) Video=11107MB Sys=0MB Shared=8109MB
  Adapter [3] Microsoft Basic Render Driver (Microsoft (WARP):0x1414:0x008c) LUID=(0000dd97,00000000) Video=0MB Sys=0MB Shared=8109MB

[1.2] Selected NVIDIA adapter #0: NVIDIA GeForce GTX 1080 Ti
       VendorId:  0x10de (NVIDIA)
       DeviceId:  0x1b06
       LUID:      (0000c78b,00000000)
       Memory:    Video=11107MB Sys=0MB Shared=8109MB

[1.3] Creating D3D11 device on NVIDIA adapter...
  Device created. Feature level: Level_11_1
  Device pointer: 0x0000019200655030
  Multithread protection: ENABLED (required for VideoSupport + Desktop Duplication)

[1.4] Verifying D3D11 device adapter LUID matches selected NVIDIA adapter...
  Device's parent adapter: NVIDIA GeForce GTX 1080 Ti
    VendorId:  0x10de
    DeviceId:  0x1b06
    LUID:      (0000c78b,00000000)
  PASS: LUID matches.

============================================================
 Phase 1 RESULT
============================================================
  D3D11_DEVICE_OK
  Adapter=LUID(0000c78b,00000000)
  GPU=NVIDIA GeForce GTX 1080 Ti
  VendorId=0x10de
  DeviceId=0x1b06
  FeatureLevel=Level_11_1
  DedicatedVideoMemory=11107MB
  DevicePointer=0x0000019200655030
============================================================

============================================================
 Phase 2 — Desktop Duplication Test (1000 frames)
============================================================

[2.1] Enumerating outputs on NVIDIA adapter...
  Output 0: \\.\DISPLAY1  (1680x1050)

[2.2] Creating IDXGIOutputDuplication...
  DuplicateOutput created. ModeDescription: 1680x1050@75.02Hz
  Format: B8G8R8A8_UNorm
  Staging texture created: 1680x1050 BGRA8
  Staging texture pointer: 0x00000192008c5de0

[2.3] Capturing 1000 frames...
    [ 100/1000] FPS=101.4 avg=0.03ms p95=0.04ms WT=371486 AL=0
    [ 200/1000] FPS=113.7 avg=0.03ms p95=0.04ms WT=674422 AL=0
    [ 300/1000] FPS=93.4 avg=0.03ms p95=0.04ms WT=1211905 AL=0
    [ 400/1000] FPS=87.6 avg=0.03ms p95=0.03ms WT=1729970 AL=0
    [ 500/1000] FPS=98.4 avg=0.03ms p95=0.04ms WT=1913957 AL=0
    [ 600/1000] FPS=101.6 avg=0.03ms p95=0.04ms WT=2205060 AL=0
    [ 700/1000] FPS=98.3 avg=0.03ms p95=0.04ms WT=2651687 AL=0
    [ 800/1000] FPS=103.1 avg=0.03ms p95=0.04ms WT=2876800 AL=0
    [ 900/1000] FPS=106.7 avg=0.03ms p95=0.05ms WT=3070242 AL=0
    [1000/1000] FPS=109.8 avg=0.03ms p95=0.05ms WT=3332660 AL=0

============================================================
 Phase 2 RESULT
============================================================
--- Capture loop ---
  Total time:           9104.8 ms
  Frames acquired:      1000
  Frames dropped:       0
  Achieved FPS:         109.83
  WaitTimeout count:    3332660
  AccessLost count:     0
  Other errors:         0
  Acquire latency:
    min/avg/max:        0.007 / 0.030 / 2.330 ms
    p50/p95/p99:        0.022 / 0.050 / 0.150 ms
============================================================

  Phase 2: PASS

============================================================
 Phase 3 — Texture Ownership Test
============================================================

[3.1] Querying texture description...
  Texture pointer:   0x00000192008c5de0
  Texture type:      ID3D11Texture2D
  Width:             1680
  Height:            1050
  MipLevels:         1
  ArraySize:         1
  Format:            B8G8R8A8_UNorm
  SampleDescription: count=1, quality=0
  Usage:             Default
  BindFlags:         ShaderResource
  CPUAccessFlags:    None
  MiscFlags:         None

[3.2] Verifying format...
  PASS: Format is BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM).

[3.3] Verifying dimensions match desktop...
  Desktop:  1680x1050
  Texture:  1680x1050
  PASS: Dimensions match.

[3.4] Verifying resource usage...
  Usage = Default:           True
  Usage = Staging:           False
  BindFlags.ShaderResource:  True
  BindFlags.RenderTarget:    False
  CPUAccessFlags (any):      False
  PASS: Texture is GPU-resident (Default usage, no CPU access).

[3.5] Verifying texture's parent device matches Phase 1 device...
  Phase 1 device pointer: 0x0000019200655030
  Texture's parent device: 0x0000019200655030
  PASS: Texture lives on the same D3D11 device as Phase 1.

[3.6] Verifying ArraySize...
  ArraySize = 1 (1 = single texture, >1 = texture array)
  PASS: Single texture (ArraySize=1) — suitable for direct NVENC registration.

============================================================
 Phase 3 RESULT
============================================================
  Texture: ID3D11Texture2D @ 0x00000192008c5de0
  Format:  BGRA8 (DXGI_FORMAT_B8G8R8A8_UNORM)
  Dims:    1680x1050
  Device:  Same (0x0000019200655030)
  Usage:   Default
  BindFlags: ShaderResource
  CPUAccess: None (None = GPU-resident)
  ArraySize: 1
============================================================

  Phase 3: PASS — texture is GPU-resident BGRA8 on the same device as Phase 1.

============================================================
 Phase 4 — NVENC Registration Spike
============================================================

[4.1] Loading NVENC function table...
  NVENC max supported API: major=13, minor=0 (packed=0x000000D0)
  Spike requests API:     major=13, minor=0 (NVENCAPI_VERSION=0x0000000D)
  Function table version: 0x7001000D (struct size=2552 bytes)
  PASS: NVENC function table loaded.

[4.2] Opening NVENC encode session on D3D11 device...
  PASS: Encode session opened. Encoder handle: 0x0000019204eb5610

[4.3] Enumerating supported codecs...
  NVENC reports 2 supported codecs.
  Supported codecs:
    H.264: 6bc82762-4e63-4ca4-aa85-1e50f321f6bf
    HEVC: 790cdc88-4522-4d7b-9425-bda9975f7603
  PASS: H.264 codec is supported.

[4.4] Verifying ARGB (BGRA8) input format support for H.264...
  H.264 supports 9 input formats.
  H.264 input formats:
    [0] NV12
    [1] YV12
    [2] IYUV
    [3] YUV444
    [4] ARGB (BGRA8)
    [5] ABGR
    [6] AYUV
    [7] ARGB10
    [8] ABGR10
  PASS: ARGB (BGRA8) is supported — zero-copy path possible.

[4.4a] CLR struct layout diagnostic:
  NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS:
    Marshal.SizeOf = 1552 bytes
    version                                  offset    0  (UInt32)
    deviceType                               offset    4  (Int32)
    device                                   offset    8  (IntPtr)
    reserved                                 offset   16  (IntPtr)
    apiVersion                               offset   24  (UInt32)
    reserved1                                offset   28  (UInt32[])
    reserved2                                offset 1040  (IntPtr[])
  NV_ENC_REGISTER_RESOURCE:
    Marshal.SizeOf = 1532 bytes
    version                                  offset    0  (UInt32)
    resourceType                             offset    4  (Int32)
    width                                    offset    8  (UInt32)
    height                                   offset   12  (UInt32)
    pitch                                    offset   16  (UInt32)
    subResourceIndex                         offset   20  (UInt32)
    resourceToRegister                       offset   24  (IntPtr)
    registeredResource                       offset   32  (IntPtr)
    bufferFormat                             offset   40  (Int32)
    reserved1                                offset   44  (UInt32[])
    reserved2                                offset 1036  (IntPtr[])
  NV_ENC_INITIALIZE_PARAMS:
    Marshal.SizeOf = 1784 bytes
    version                                  offset    0  (UInt32)
    encodeGUID                               offset    4  (Guid)
    presetGUID                               offset   20  (Guid)
    encodeWidth                              offset   36  (UInt32)
    encodeHeight                             offset   40  (UInt32)
    darWidth                                 offset   44  (UInt32)
    darHeight                                offset   48  (UInt32)
    frameRateNum                             offset   52  (UInt32)
    frameRateDen                             offset   56  (UInt32)
    enableEncodeAsync                        offset   60  (UInt32)
    enablePTD                                offset   64  (UInt32)
    bitFields                                offset   68  (UInt32)
    privDataSize                             offset   72  (UInt32)
    _padding1                                offset   76  (UInt32)
    privData                                 offset   80  (IntPtr)
    encodeConfig                             offset   88  (IntPtr)
    maxEncodeWidth                           offset   96  (UInt32)
    maxEncodeHeight                          offset  100  (UInt32)
    maxMEHintCountsPerBlockL0                offset  104  (UInt32)
    maxMEHintCountsPerBlockL1                offset  108  (UInt32)
    reserved                                 offset  112  (UInt32[])
    _padding2                                offset 1268  (UInt32)
    reserved2                                offset 1272  (IntPtr[])
  NV_ENCODE_API_FUNCTION_LIST:
    Marshal.SizeOf = 2552 bytes
    version                                  offset    0  (UInt32)
    reserved                                 offset    4  (UInt32)
    nvEncOpenEncodeSession                   offset    8  (IntPtr)
    nvEncGetEncodeGUIDCount                  offset   16  (IntPtr)
    nvEncGetEncodeProfileGUIDCount           offset   24  (IntPtr)
    nvEncGetEncodeProfileGUIDs               offset   32  (IntPtr)
    nvEncGetEncodeGUIDs                      offset   40  (IntPtr)
    nvEncGetInputFormatCount                 offset   48  (IntPtr)
    nvEncGetInputFormats                     offset   56  (IntPtr)
    nvEncGetEncodeCaps                       offset   64  (IntPtr)
    nvEncGetEncodePresetCount                offset   72  (IntPtr)
    nvEncGetEncodePresetGUIDs                offset   80  (IntPtr)
    nvEncGetEncodePresetConfig               offset   88  (IntPtr)
    nvEncInitializeEncoder                   offset   96  (IntPtr)
    nvEncCreateInputBuffer                   offset  104  (IntPtr)
    nvEncDestroyInputBuffer                  offset  112  (IntPtr)
    nvEncCreateBitstreamBuffer               offset  120  (IntPtr)
    nvEncDestroyBitstreamBuffer              offset  128  (IntPtr)
    nvEncEncodePicture                       offset  136  (IntPtr)
    nvEncLockBitstream                       offset  144  (IntPtr)
    nvEncUnlockBitstream                     offset  152  (IntPtr)
    nvEncLockInputBuffer                     offset  160  (IntPtr)
    nvEncUnlockInputBuffer                   offset  168  (IntPtr)
    nvEncGetEncodeStats                      offset  176  (IntPtr)
    nvEncGetSequenceParams                   offset  184  (IntPtr)
    nvEncRegisterAsyncEvent                  offset  192  (IntPtr)
    nvEncUnregisterAsyncEvent                offset  200  (IntPtr)
    nvEncMapInputResource                    offset  208  (IntPtr)
    nvEncUnmapInputResource                  offset  216  (IntPtr)
    nvEncDestroyEncoder                      offset  224  (IntPtr)
    nvEncInvalidateRefFrames                 offset  232  (IntPtr)
    nvEncOpenEncodeSessionEx                 offset  240  (IntPtr)
    nvEncRegisterResource                    offset  248  (IntPtr)
    nvEncUnregisterResource                  offset  256  (IntPtr)
    nvEncReconfigureEncoder                  offset  264  (IntPtr)
    reserved1                                offset  272  (IntPtr)
    nvEncCreateMVBuffer                      offset  280  (IntPtr)
    nvEncDestroyMVBuffer                     offset  288  (IntPtr)
    nvEncRunMotionEstimationOnly             offset  296  (IntPtr)
    reserved2                                offset  304  (IntPtr[])
  NV_ENC_INITIALIZE_PARAMS_VER: 0xF005000D
  NV_ENC_REGISTER_RESOURCE_VER: 0x7003000D
  NV_ENCODE_API_FUNCTION_LIST_VER: 0x7001000D

[4.4b] Initializing encoder (required before RegisterResource)...
  PASS: Encoder initialized (1680x1050 @ 60fps, H.264, Default preset).

[4.5] Registering a fresh staging texture with NVENC...
  Creating fresh texture: 1680x1050 BGRA8 on device 0x0000019200655030
  Fresh texture pointer: 0x00000192008c4260
  Fresh texture's parent device: 0x0000019200655030
  Phase 1 device pointer:        0x0000019200655030
  PASS: Fresh texture is on the same D3D11 device.
  PASS: Texture registered with NVENC.
         Registered handle: 0x0000019204baba90
         Width:  1680
         Height: 1050
         Format: ARGB (BGRA8)
         Resource type: DirectX (D3D11Texture2D)

[4.6] Unregistering resource and destroying encoder...
  PASS: Resource unregistered.
  PASS: Fresh texture disposed.
  PASS: Encoder destroyed.

============================================================
 Phase 4 RESULT
============================================================
  NVENC loaded:          YES
  Encode session:        OPENED on D3D11 device
  H.264 codec:           SUPPORTED
  ARGB (BGRA8) format:   SUPPORTED
  Texture registered:    YES (NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX)
  GPU:                   NVIDIA GeForce GTX 1080 Ti
  Format:                ARGB (BGRA8)
============================================================

  V1 BLOCKER STATUS: ✅ RESOLVED — zero-copy path PROVEN

============================================================
 Phase 5 — Performance Benchmark
============================================================

[5.1] Determining current desktop resolution...
  Desktop resolution: 1680x1050
  NOTE: Spike cannot force desktop resolution change.
        Will run benchmarks matching current desktop resolution only.
  WARN: Current desktop (1680x1050) is not in target list.
        Running benchmark at current resolution with all 3 FPS targets.

[5.2] Benchmark: 1680x1050 @ 60 FPS, 10s duration
  Target:    1680x1050 @ 60 FPS
  Achieved:  101.52 FPS (1015 frames in 1593 iterations)
  Latency:   avg=6.032 ms, p50=5.554 ms, p95=13.641 ms, p99=15.253 ms
  Errors:    WT=578, AL=0, Other=0, Dropped=0
  CPU usage: 0.0%
  GPU usage: 0.0%
  NVENC:     Not benchmarked in Phase 5 (separate concern)

[5.2] Benchmark: 1680x1050 @ 120 FPS, 10s duration

Status: OWNER VALIDATED

Result:
PASS

GPU:
NVIDIA GeForce GTX 1080 Ti

Evidence:
NvEncRegisterResource returned NV_ENC_SUCCESS
D3D11 texture registered successfully
Same D3D11 device verified