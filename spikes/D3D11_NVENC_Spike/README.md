# CaptureEngine.Video.Spike.D3D11 — D3D11/NVENC Interop Spike

**Task ID:** P1-B.2-V1
**Purpose:** Validate V1 BLOCKER (cross-device D3D11/NVENC interoperability) before DdagrabBackend production implementation.
**Status:** SPIKE — not part of Engine-Rebuild branch, not committed, not deployed.

---

## ⚠️ Isolation Guarantees

This spike:

- ❌ Does **NOT** modify Foundation (Phase 0, frozen at `82d792ab`)
- ❌ Does **NOT** modify `IVideoFrame` interface
- ❌ Does **NOT** modify any production backend
- ❌ Does **NOT** add NVENC integration to the main Engine
- ❌ Does **NOT** reference `CaptureEngine` or `CaptureEngine.Video` projects
- ❌ Does **NOT** push commits to the repository

It is a standalone .NET 8 console application that proves the zero-copy D3D11 → NVENC path is feasible.

---

## Prerequisites

### Hardware
- **NVIDIA GPU** with NVENC support:
  - GeForce GTX 600 series or newer (Kepler+)
  - Quadro K500+ / Tesla K10+
  - See [NVIDIA NVENC Support Matrix](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)
- **Display** connected to the NVIDIA GPU (DXGI Output Duplication requires the desktop to be on the same GPU)

### Software
- **Windows 10 version 1903+** (build 18362+) — required for `IDXGIOutput5::DuplicateOutput1`
- **.NET 8 SDK (x64)** — https://dotnet.microsoft.com/download/dotnet/8.0
- **NVIDIA driver** latest version (NVENC API requires recent driver)
- **NVIDIA Video Codec SDK** — https://developer.nvidia.com/video-codec-sdk
  - Download requires free NVIDIA Developer Program membership
  - After download, extract `nvEncodeAPI.dll` from the SDK's `Lib\x64\` folder

### Runtime DLL placement
Copy `nvEncodeAPI.dll` to one of:
1. **Project output directory** (recommended): `bin\x64\Debug\net8.0-windows\nvEncodeAPI.dll`
2. **System directory**: `C:\Windows\System32\nvEncodeAPI.dll` (system-wide)

---

## Build

```cmd
cd CaptureEngine.Video.Spike.D3D11
scripts\build.bat
```

Or directly with dotnet:

```cmd
dotnet restore
dotnet build -c Debug -p:Platform=x64
```

Output goes to: `bin\x64\Debug\net8.0-windows\CaptureEngine.Video.Spike.D3D11.exe`

---

## Run

### Run all 5 phases (typical)

```cmd
scripts\run-all.bat spike_output.md
```

### Run a single phase

```cmd
scripts\run-phase.bat 1              # Phase 1: D3D11 Device Test
scripts\run-phase.bat 4 report.md    # Phase 4: NVENC Registration, tee to report.md
```

### Direct execution

```cmd
bin\x64\Debug\net8.0-windows\CaptureEngine.Video.Spike.D3D11.exe --log spike_output.md
bin\x64\Debug\net8.0-windows\CaptureEngine.Video.Spike.D3D11.exe phase4 --log phase4_output.md
```

---

## Phases

### Phase 1 — D3D11 Device Test
- Enumerate DXGI adapters, find NVIDIA adapter
- Create D3D11 device on NVIDIA adapter with BgraSupport flag
- Log: GPU name, Vendor ID, Device ID, LUID, Feature Level, Memory
- Verify device's adapter LUID matches selected adapter
- **Output:** `D3D11_DEVICE_OK` + adapter LUID

### Phase 2 — Desktop Duplication Test
- Use `IDXGIOutput1::DuplicateOutput` on Phase 1's D3D11 device
- Capture 1000 frames using `AcquireNextFrame`
- Copy desktop texture → staging texture (`CopyResource`)
- Measure: FPS, acquire latency (min/avg/max/p50/p95/p99), WAIT_TIMEOUT count, ACCESS_LOST count
- **Acceptance:** 1000 frames captured, FPS ≥ 60, no ACCESS_LOST, p95 latency < 10 ms

### Phase 3 — Texture Ownership Test
- Verify staging texture is `ID3D11Texture2D` (not CPU buffer)
- Verify format = BGRA8 (`DXGI_FORMAT_B8G8R8A8_UNORM`)
- Verify dimensions match desktop
- Verify Usage = Default (no CPU staging)
- Verify CPUAccessFlags = None (no CPU access — true GPU resident)
- Verify texture's parent device matches Phase 1's device (same GPU)
- **Output:** `Texture: ID3D11Texture2D, Format: BGRA8, Device: Same`

### Phase 4 — NVENC Registration Spike
- Load `nvEncodeAPI.dll` via P/Invoke
- Call `NvEncodeAPICreateInstance` to get function table
- Call `NvEncOpenEncodeSessionEx` with `NV_ENC_DEVICE_DIRECTX` and Phase 1's device
- Enumerate supported codecs (verify H.264 is supported)
- Enumerate supported input formats (verify ARGB/BGRA8 is supported)
- Call `NvEncRegisterResource` with the staging texture from Phase 2
- Call `NvEncUnregisterResource` and `NvEncDestroyEncoder`
- **Acceptance:** Registration succeeds → V1 BLOCKER resolved ✅

### Phase 5 — Performance Benchmark
- Run capture loop at 3 resolutions × 3 FPS targets:
  - 1920×1080 @ 60/120/144 FPS
  - 2560×1440 @ 60/120/144 FPS
  - 3840×2160 @ 60/120/144 FPS
- For each: 10-second capture loop, measure FPS/latency/CPU%/GPU%
- **Note:** Spike uses current desktop resolution — if not in target list, runs at current resolution × 3 FPS targets
- **Acceptance:** Achieved FPS ≥ 90% of target, p95 latency < 15 ms, no ACCESS_LOST

---

## Acceptance Criteria (V1 BLOCKER)

| Criterion | Test | Acceptance |
|---|---|---|
| Same GPU device | Phase 3 §3.5 | Texture device pointer == Phase 1 device pointer |
| Texture acquired | Phase 2 | 1000 frames captured without ACCESS_LOST |
| NVENC register success | Phase 4 §4.5 | `NvEncRegisterResource` returns `NV_ENC_SUCCESS` |
| No CPU staging copy | Phase 3 §3.4 | `Usage = Default`, `CPUAccessFlags = None` |
| 144 FPS stable | Phase 5 | Achieved FPS ≥ 130 at 1920×1080 (90% of 144) |

### PASS → Continue to DdagrabBackend production implementation
### FAIL → STOP, report blocker, do not continue

---

## Output Report

After running `scripts\run-all.bat spike_output.md`, the file `spike_output.md` will contain the full console output (tee'd from console). OWNER should:

1. Review the output and confirm all 5 phases passed
2. Copy/paste the key results into `D3D11_NVENC_Spike_Report.md` (skeleton provided)
3. Fill in the FACT/RESULT/UNKNOWN/RISK/RECOMMENDATION sections based on actual output
4. Send the report to GPT/Claude/DeepSeek for challenge per Decision Rule
5. OWNER makes the final PASS/FAIL decision

---

## Troubleshooting

### `DllNotFoundException: nvEncodeAPI.dll`
- Copy `nvEncodeAPI.dll` from NVIDIA Video Codec SDK's `Lib\x64\` to the spike's output directory
- Verify the DLL is the 64-bit version (the project is built x64-only)

### `NV_ENC_ERR_NO_ENCODE_DEVICE`
- The D3D11 device was not created on an NVIDIA GPU
- Check Phase 1 output — confirm `VendorId: 0x10DE` (NVIDIA)
- If running on a laptop with hybrid graphics (Intel + NVIDIA), force the spike to run on the NVIDIA GPU:
  - Right-click `CaptureEngine.Video.Spike.D3D11.exe` → "Run with graphics processor" → "High-performance NVIDIA processor"
  - Or set in NVIDIA Control Panel → Manage 3D Settings → Program Settings

### `NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY`
- NVENC SDK version mismatch with driver
- Update NVIDIA driver to the latest version
- Verify `NVENCAPI_VERSION` in `Utils/NvEncodeAPI.cs` matches your SDK version (currently set to 12.2 = 0x0032)

### `AccessLost` during Phase 2
- Display mode changed during capture (resolution, refresh rate, RDP, UAC, lock screen)
- Re-run the spike without changing display settings

### `IDXGIOutput1::DuplicateOutput` returns `E_INVALIDARG`
- D3D11 device was not created from `IDXGIFactory1` (see FFmpeg trac #10385)
- Phase 1 uses `CreateDXGIFactory1` — this should not happen
- If it does, check that the device's adapter is the same one Phase 1 selected

### Phase 5 GPU usage shows 0%
- `GpuUsageLazy` is a stub — NVML integration is not implemented in this spike
- Use external `nvidia-smi` monitoring: `nvidia-smi dmon -s u -d 1` in a separate window
- Manually record peak GPU% during Phase 5 and add to report

---

## Spike Project Structure

```
CaptureEngine.Video.Spike.D3D11/
├── CaptureEngine.Video.Spike.D3D11.csproj
├── Program.cs                                # Main entry point
├── Phases/
│   ├── Phase1_DeviceTest.cs                  # D3D11 device + adapter LUID
│   ├── Phase2_DesktopDuplication.cs          # 1000-frame capture + metrics
│   ├── Phase3_TextureOwnership.cs            # Format/device verification
│   ├── Phase4_NVENCRegistration.cs           # NvEncRegisterResource
│   └── Phase5_PerformanceBenchmark.cs        # Multi-res + multi-FPS benchmark
├── Utils/
│   ├── GpuInfo.cs                            # Adapter enumeration helper
│   ├── Metrics.cs                            # FrameMetrics + CaptureStatsSnapshot
│   └── NvEncodeAPI.cs                        # P/Invoke declarations for NVENC
├── scripts/
│   ├── build.bat                             # Build (Debug/Release)
│   ├── run-all.bat                           # Run all 5 phases
│   └── run-phase.bat                         # Run single phase
├── README.md                                 # This file
└── D3D11_NVENC_Spike_Report.md               # Report skeleton (OWNER fills)
```

---

## Notes

- This spike uses **Vortice.Windows** for D3D11/DXGI bindings (per `Ddagrab_Research.md` REC-8 — SharpDX is abandoned since 2019)
- NVENC is invoked via **P/Invoke to nvEncodeAPI.dll** directly (not through FFmpeg) to validate the lowest-level zero-copy path
- The spike does NOT include actual video encoding — Phase 4 only validates resource registration. Actual encoding benchmark is deferred until encoder integration phase
- Phase 5 GPU usage monitoring is a stub — use external `nvidia-smi` for accurate GPU% measurement
- All spike output is timestamped and tee'd to a log file when `--log` is specified

---

## Cross-References

- **P1-A v1.3.1 §3.4, §15.1** — BGRA8 baseline requirement → Phase 3 §3.2
- **P1-A v1.3.1 §3.6.1** — QPC-based timestamp model → Phase 2/5 metrics use Stopwatch (QPC-based)
- **P1-A v1.3.1 §6.4** — NoFrame semantic → Phase 2 maps `WAIT_TIMEOUT` to NoFrame
- **Ddagrab_Research.md F2** — ddagrab output format is `AV_PIX_FMT_D3D11` → Phase 3 verifies equivalent DXGI texture
- **Ddagrab_Research.md F3** — ddagrab frame flow → Phase 2 mirrors `AcquireNextFrame → QueryInterface → CopySubresourceRegion → ReleaseFrame`
- **Ddagrab_Research.md REC-1** — Use native DXGI for capture layer → this spike validates that path
- **Ddagrab_Research.md REC-2** — Shared D3D11 device across capture and encoder → Phase 4 proves device can be shared with NVENC
- **Ddagrab_Research.md REC-5** — V1 spike using native DXGI + NVENC → this spike IS the V1 spike
- **Ddagrab_Research.md REC-8** — Use Vortice.Windows → this spike uses Vortice.Direct3D11 + Vortice.DXGI
- **P1-B.2-AUDIT A4-1** — `IVideoBackendContext` missing D3D11 device → REC-6 proposes adding it; this spike proves the device can be shared

---

*End of README.md*
