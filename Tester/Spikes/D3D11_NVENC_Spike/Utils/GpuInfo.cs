// Utils/GpuInfo.cs
//
// P1-B.2-V1 Spike — D3D11/NVENC Interop
// Phase 1 helper: enumerate GPU adapters and log identifying info.
//
// SPDX-License-Identifier: MIT
// This is spike code — not production, not part of Engine-Rebuild contract.

using Vortice.DXGI;

namespace CaptureEngine.Video.Spike.D3D11.Utils;

/// <summary>
/// Information about a DXGI adapter, used to verify which GPU the D3D11
/// device was created on. Critical for V1 (cross-device interop) validation.
/// </summary>
public sealed record GpuInfo(
    int AdapterIndex,
    string Description,
    uint VendorId,
    uint DeviceId,
    long AdapterLuidLow,
    int AdapterLuidHigh,
    ulong DedicatedVideoMemoryBytes,
    ulong DedicatedSystemMemoryBytes,
    ulong SharedSystemMemoryBytes)
{
    /// <summary>
    /// LUID (Locally Unique Identifier) of the adapter — matches the LUID
    /// used by NVENC's NV_ENC_OPEN_ENCODE_SESSION_INPUT_PARAMS::device
    /// (when deviceType = NV_ENC_DEVICE_DIRECTX).
    /// </summary>
    public string LuidString => $"({AdapterLuidLow:x8},{AdapterLuidHigh:x8})";

    public string VendorName => VendorId switch
    {
        0x10DE => "NVIDIA",
        0x1002 => "AMD",
        0x8086 => "Intel",
        0x1414 => "Microsoft (WARP)",
        _      => $"Unknown(0x{VendorId:x4})"
    };

    public bool IsNvidia => VendorId == 0x10DE;

    public string MemorySummary =>
        $"Video={DedicatedVideoMemoryBytes / (1024 * 1024)}MB " +
        $"Sys={DedicatedSystemMemoryBytes / (1024 * 1024)}MB " +
        $"Shared={SharedSystemMemoryBytes / (1024 * 1024)}MB";

    public static GpuInfo FromAdapter(int index, IDXGIAdapter1 adapter)
    {
        var desc = adapter.Description1;
        return new GpuInfo(
            AdapterIndex: index,
            Description: desc.Description,
            VendorId: desc.VendorId,
            DeviceId: desc.DeviceId,
            AdapterLuidLow: desc.Luid.LowPart,
            AdapterLuidHigh: desc.Luid.HighPart,
            DedicatedVideoMemoryBytes: desc.DedicatedVideoMemory,
            DedicatedSystemMemoryBytes: desc.DedicatedSystemMemory,
            SharedSystemMemoryBytes: desc.SharedSystemMemory);
    }

    public override string ToString() =>
        $"[{AdapterIndex}] {Description} ({VendorName}:0x{VendorId:x4}:0x{DeviceId:x4}) " +
        $"LUID={LuidString} {MemorySummary}";
}
