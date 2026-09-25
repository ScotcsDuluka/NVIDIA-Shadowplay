// Direct3D11Helper.cs — WGC interop helper
//
// Based on Microsoft's official Windows.UI.Composition-Win32-Samples.
// Uses CreateDirect3D11DeviceFromDXGIDevice from d3d11.dll.
//
// IMPORTANT: In .NET 8 with CsWinRT, Marshal.GetObjectForIUnknown returns
// a raw __ComObject that CsWinRT cannot automatically wrap as a WinRT
// interface. We must use WinRT.MarshalInspectable<T>.FromAbi() instead.
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
    ///
    /// In .NET 8 with CsWinRT, Marshal.GetObjectForIUnknown returns a raw
    /// __ComObject that cannot be cast to WinRT interfaces. Instead, we
    /// use WinRT.MarshalInspectable<T>.FromAbi() which properly creates
    /// the CCW (COM Callable Wrapper) for the WinRT projection.
    /// </summary>
    public static IDirect3DDevice CreateDirect3DDeviceFromDXGIDevice(IntPtr dxgiDevicePtr)
    {
        uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevicePtr, out IntPtr pUnknown);
        if (hr != 0)
            throw new InvalidOperationException(
                $"CreateDirect3D11DeviceFromDXGIDevice failed: 0x{hr:X8}");

        try
        {
            // Use WinRT marshaler to create a proper WinRT projection object.
            // Marshal.GetObjectForIUnknown alone returns __ComObject which
            // CsWinRT cannot cast to IDirect3DDevice.
            var device = WinRT.MarshalInspectable<IDirect3DDevice>.FromAbi(pUnknown);
            return device;
        }
        finally
        {
            Marshal.Release(pUnknown);
        }
    }
}
