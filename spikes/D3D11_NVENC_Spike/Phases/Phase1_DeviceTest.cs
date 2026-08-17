// Phases/Phase1_DeviceTest.cs
//
// P1-B.2-V1 Spike — Phase 1: D3D11 Device Test
//
// Goal: Create a D3D11 device on the NVIDIA adapter and log identifying info.
// This proves we can identify and bind to a specific GPU before capture.
//
// Outputs to console (and to log file if --log flag is given):
//   D3D11_DEVICE_OK
//   Adapter=LUID(low,high)
//   GPU=NVIDIA GeForce ... (Vendor:0x10DE:0xXXXX)
//   FeatureLevel=11_1
//   DedicatedVideoMemory=NNNNMB
//
// Acceptance criteria for Phase 1:
//   - D3D11 device created on NVIDIA adapter (not WARP, not Intel/AMD)
//   - LUID recorded (will be compared with NVENC's expected LUID in Phase 4)
//   - Feature level >= 11_0 (required for DXGI Desktop Duplication API)
//
// SPDX-License-Identifier: MIT

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase1_DeviceTest
{
    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 1 — D3D11 Device Test");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        // --- Step 1: Enumerate DXGI adapters ---
        Console.WriteLine("[1.1] Enumerating DXGI adapters...");
        IDXGIFactory1? factory = null;
        try
        {
            DXGI.CreateDXGIFactory1(out factory).CheckError();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  FAIL: CreateDXGIFactory1 threw: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }

        var gpus = new List<GpuInfo>();
        int nvidiaIdx = -1;
        int adapterIdx = 0;
        while (factory!.EnumAdapters1((uint)adapterIdx, out IDXGIAdapter1 adapter).Success)
        {
            var info = GpuInfo.FromAdapter(adapterIdx, adapter);
            gpus.Add(info);
            Console.WriteLine($"  Adapter {info}");
            if (info.IsNvidia && nvidiaIdx < 0)
                nvidiaIdx = adapterIdx;
            adapter.Dispose();
            adapterIdx++;
        }

        if (gpus.Count == 0)
        {
            Console.Error.WriteLine("  FAIL: No DXGI adapters found.");
            return 1;
        }

        if (nvidiaIdx < 0)
        {
            Console.Error.WriteLine("  FAIL: No NVIDIA adapter found.");
            Console.Error.WriteLine("        This spike requires an NVIDIA GPU with NVENC support.");
            Console.Error.WriteLine("        Found adapters:");
            foreach (var g in gpus)
                Console.Error.WriteLine($"          - {g}");
            return 1;
        }

        var nvidiaGpu = gpus[nvidiaIdx];
        Console.WriteLine();
        Console.WriteLine($"[1.2] Selected NVIDIA adapter #{nvidiaIdx}: {nvidiaGpu.Description}");
        Console.WriteLine($"       VendorId:  0x{nvidiaGpu.VendorId:x4} ({nvidiaGpu.VendorName})");
        Console.WriteLine($"       DeviceId:  0x{nvidiaGpu.DeviceId:x4}");
        Console.WriteLine($"       LUID:      {nvidiaGpu.LuidString}");
        Console.WriteLine($"       Memory:    {nvidiaGpu.MemorySummary}");

        // --- Step 2: Create D3D11 device on the NVIDIA adapter ---
        Console.WriteLine();
        Console.WriteLine("[1.3] Creating D3D11 device on NVIDIA adapter...");

        IDXGIAdapter1 targetAdapter;
        factory.EnumAdapters1((uint)nvidiaIdx, out targetAdapter).CheckError();

        FeatureLevel[] requestedFeatureLevels =
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
        };

        ID3D11Device device;
        ID3D11DeviceContext context;
        try
        {
            // Fully-qualify D3D11 class to avoid ambiguity with our own
            // namespace CaptureEngine.Video.Spike.D3D11.
            //
            // Device creation flags:
            //   BgraSupport   — required for DXGI Desktop Duplication API
            //   VideoSupport  — required for NVENC interop (D3D11 device must
            //                   be created with this flag for NvEncRegisterResource
            //                   to accept textures from this device)
            //
            // Without VideoSupport, NvEncRegisterResource returns
            // NV_ENC_ERR_DEVICE_NOT_EXIST even though the texture is on the
            // same device as the encode session.
            Vortice.Direct3D11.D3D11.D3D11CreateDevice(
                targetAdapter,
                DriverType.Unknown,        // must be Unknown when specifying adapter
                DeviceCreationFlags.BgraSupport | DeviceCreationFlags.VideoSupport,
                requestedFeatureLevels,
                out device,
                out context).CheckError();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  FAIL: D3D11CreateDevice threw: {ex.GetType().Name}: {ex.Message}");
            targetAdapter.Dispose();
            factory.Dispose();
            return 1;
        }

        FeatureLevel achievedFeatureLevel = device.FeatureLevel;
        Console.WriteLine($"  Device created. Feature level: {achievedFeatureLevel}");
        Console.WriteLine($"  Device pointer: 0x{device.NativePointer.ToInt64():x16}");

        // Enable multithread protection on the device context.
        //
        // D3D11_CREATE_DEVICE_VIDEO_SUPPORT flag implies multithreaded usage
        // (NVENC may access the device from a different thread). Without
        // multithread protection enabled, the device context becomes
        // serialized and DXGI Desktop Duplication performance drops
        // dramatically — in OWNER's test, FPS went from ~100 to ~3 with
        // 2606 WAIT_TIMEOUT events.
        //
        // ID3D11Multithread::SetMultithreadProtected(TRUE) makes the device
        // context thread-safe, restoring capture performance.
        Vortice.Direct3D11.ID3D11Multithread multithread =
            context.QueryInterface<Vortice.Direct3D11.ID3D11Multithread>();
        multithread.SetMultithreadProtected(true);
        Console.WriteLine($"  Multithread protection: ENABLED (required for VideoSupport + Desktop Duplication)");

        // --- Step 3: Verify device adapter LUID matches the selected NVIDIA adapter ---
        Console.WriteLine();
        Console.WriteLine("[1.4] Verifying D3D11 device adapter LUID matches selected NVIDIA adapter...");

        // Query the device for IDXGIDevice, then GetParent to IDXGIAdapter
        IDXGIDevice? dxgiDevice = device.QueryInterface<IDXGIDevice>();
        IDXGIAdapter? deviceAdapter = dxgiDevice.GetParent<IDXGIAdapter>();
        var deviceAdapterDesc = deviceAdapter.Description;

        Console.WriteLine($"  Device's parent adapter: {deviceAdapterDesc.Description}");
        Console.WriteLine($"    VendorId:  0x{deviceAdapterDesc.VendorId:x4}");
        Console.WriteLine($"    DeviceId:  0x{deviceAdapterDesc.DeviceId:x4}");
        Console.WriteLine($"    LUID:      ({deviceAdapterDesc.Luid.LowPart:x8},{deviceAdapterDesc.Luid.HighPart:x8})");

        bool luidMatches = deviceAdapterDesc.Luid.LowPart == (ulong)nvidiaGpu.AdapterLuidLow
                        && deviceAdapterDesc.Luid.HighPart == nvidiaGpu.AdapterLuidHigh;

        if (!luidMatches)
        {
            Console.Error.WriteLine("  FAIL: D3D11 device's adapter LUID does NOT match selected NVIDIA adapter.");
            Console.Error.WriteLine("        This indicates the device was created on the wrong GPU.");
            deviceAdapter.Dispose();
            dxgiDevice.Dispose();
            device.Dispose();
            context.Dispose();
            targetAdapter.Dispose();
            factory.Dispose();
            return 1;
        }
        Console.WriteLine("  PASS: LUID matches.");

        // --- Step 4: Save device info for later phases ---
        // We stash the device + LUID into a shared context object so Phase 2/3/4 can reuse.
        SpikeSharedContext.Device = device;
        SpikeSharedContext.DeviceContext = context;
        SpikeSharedContext.Gpu = nvidiaGpu;
        SpikeSharedContext.Factory = factory;
        SpikeSharedContext.TargetAdapter = targetAdapter;

        // Don't dispose device/context — they're owned by SpikeSharedContext now.
        // But dxgiDevice and deviceAdapter are temporary.
        deviceAdapter.Dispose();
        dxgiDevice.Dispose();

        // --- Step 5: Print Phase 1 result summary ---
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 1 RESULT");
        Console.WriteLine("============================================================");
        Console.WriteLine($"  D3D11_DEVICE_OK");
        Console.WriteLine($"  Adapter=LUID({nvidiaGpu.AdapterLuidLow:x8},{nvidiaGpu.AdapterLuidHigh:x8})");
        Console.WriteLine($"  GPU={nvidiaGpu.Description}");
        Console.WriteLine($"  VendorId=0x{nvidiaGpu.VendorId:x4}");
        Console.WriteLine($"  DeviceId=0x{nvidiaGpu.DeviceId:x4}");
        Console.WriteLine($"  FeatureLevel={achievedFeatureLevel}");
        Console.WriteLine($"  DedicatedVideoMemory={nvidiaGpu.DedicatedVideoMemoryBytes / (1024 * 1024)}MB");
        Console.WriteLine($"  DevicePointer=0x{device.NativePointer.ToInt64():x16}");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        return 0;
    }
}

/// <summary>
/// Shared state across phases — D3D11 device and metadata established by Phase 1.
/// Declared `partial` so other phases can add their own fields without modifying
/// this file.
/// </summary>
public static partial class SpikeSharedContext
{
    public static ID3D11Device? Device { get; set; }
    public static ID3D11DeviceContext? DeviceContext { get; set; }
    public static GpuInfo? Gpu { get; set; }
    public static IDXGIFactory1? Factory { get; set; }
    public static IDXGIAdapter1? TargetAdapter { get; set; }

    public static void Cleanup()
    {
        StagingTexture?.Dispose();
        DeviceContext?.Dispose();
        Device?.Dispose();
        TargetAdapter?.Dispose();
        Factory?.Dispose();
    }
}
