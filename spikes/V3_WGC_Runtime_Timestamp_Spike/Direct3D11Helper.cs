// Direct3D11Helper.cs — WGC interop helper
//
// Provides CreateDirect3DDeviceFromDXGIDevice to convert ID3D11Device → IDirect3DDevice
// for Windows.Graphics.Capture interop.
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

namespace V3_WGC_Runtime_Timestamp_Spike;

/// <summary>
/// Helper class for creating WinRT IDirect3DDevice from DXGI device.
/// </summary>
internal static class Direct3D11Helper
{
    [ComImport]
    [Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    private interface ICreateDirect3D11DeviceFromDXGIDevice
    {
        IntPtr CreateFromDxgiDevice([In] IntPtr dxgiDevice, [Out, MarshalAs(UnmanagedType.IUnknown)] out object graphicsDevice);
    }

    [ComImport]
    [Guid("1F69D7A3-9B6D-463C-9B5A-5CE8D3E8B2B7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    private interface ICreateDirect3D11DeviceFromDXGIDeviceFactory
    {
        ICreateDirect3D11DeviceFromDXGIDevice CreateInstance();
    }

    /// <summary>
    /// Creates an IDirect3DDevice from an IDXGIDevice native pointer.
    /// </summary>
    public static IDirect3DDevice CreateDirect3DDeviceFromDXGIDevice(IntPtr dxgiDevicePtr)
    {
        // Get the factory for creating Direct3D devices from DXGI devices
        var factoryClassId = new Guid("1F69D7A3-9B6D-463C-9B5A-5CE8D3E8B2B7");

        var iid = typeof(ICreateDirect3D11DeviceFromDXGIDeviceFactory).GUID;
        int hr = CoCreateInstance(ref factoryClassId, IntPtr.Zero, 1, ref iid, out object factoryObj);
        if (hr != 0)
            throw new InvalidOperationException($"CoCreateInstance failed: 0x{hr:X8}");

        var factory = (ICreateDirect3D11DeviceFromDXGIDeviceFactory)factoryObj;
        var creator = factory.CreateInstance();

        creator.CreateFromDxgiDevice(dxgiDevicePtr, out object d3dDeviceObj);
        return (IDirect3DDevice)d3dDeviceObj;
    }

    [DllImport("ole32.dll", PreserveSig = false)]
    [return: MarshalAs(UnmanagedType.IUnknown)]
    private static extern int CoCreateInstance(
        ref Guid clsid,
        [MarshalAs(UnmanagedType.IUnknown)] object? punkOuter,
        uint clsctx,
        ref Guid iid,
        out object ppv);
}
