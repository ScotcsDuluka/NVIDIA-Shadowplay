// Direct3D11Helper.cs — WGC interop helper
//
// Based on Microsoft's official Windows.UI.Composition-Win32-Samples.
// Uses CreateDirect3D11DeviceFromDXGIDevice from d3d11.dll (NOT CoCreateInstance).
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

namespace V3_WGC_Runtime_Timestamp_Spike;

internal static class Direct3D11Helper
{
    [DllImport("d3d11.dll", EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice",
        SetLastError = true, CharSet = CharSet.Unicode,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern uint CreateDirect3D11DeviceFromDXGIDevice(
        IntPtr dxgiDevice, out IntPtr graphicsDevice);

    /// <summary>
    /// Creates an IDirect3DDevice (WinRT) from an IDXGIDevice native pointer.
    /// Uses the d3d11.dll CreateDirect3D11DeviceFromDXGIDevice export directly.
    /// </summary>
    public static IDirect3DDevice CreateDirect3DDeviceFromDXGIDevice(IntPtr dxgiDevicePtr)
    {
        uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevicePtr, out IntPtr pUnknown);
        if (hr != 0)
            throw new InvalidOperationException(
                $"CreateDirect3D11DeviceFromDXGIDevice failed: 0x{hr:X8}");

        var device = Marshal.GetObjectForIUnknown(pUnknown) as IDirect3DDevice;
        Marshal.Release(pUnknown);
        return device
            ?? throw new InvalidOperationException("Failed to get IDirect3DDevice from IUnknown.");
    }
}
