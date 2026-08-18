// Direct3D11Helper.cs — WGC interop helper (reused from V3 pattern)
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal static class Direct3D11Helper
{
    [DllImport("d3d11.dll", EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice",
        SetLastError = true, CharSet = CharSet.Unicode,
        ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern uint CreateDirect3D11DeviceFromDXGIDevice(
        IntPtr dxgiDevice, out IntPtr graphicsDevice);

    public static IDirect3DDevice CreateDirect3DDeviceFromDXGIDevice(IntPtr dxgiDevicePtr)
    {
        uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevicePtr, out IntPtr pUnknown);
        if (hr != 0)
            throw new InvalidOperationException($"CreateDirect3D11DeviceFromDXGIDevice failed: 0x{hr:X8}");
        try
        {
            return WinRT.MarshalInspectable<IDirect3DDevice>.FromAbi(pUnknown);
        }
        finally
        {
            Marshal.Release(pUnknown);
        }
    }
}
