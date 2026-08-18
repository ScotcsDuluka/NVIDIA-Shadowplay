// Direct3D11Helper.cs — WGC interop helper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

namespace V3_WGC_Runtime_Timestamp_Spike;

internal static class Direct3D11Helper
{
    [ComImport]
    [Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ICreateDirect3D11DeviceFromDXGIDevice
    {
        [PreserveSig]
        int CreateFromDxgiDevice([In] IntPtr dxgiDevice, [Out, MarshalAs(UnmanagedType.IUnknown)] out object graphicsDevice);
    }

    [ComImport]
    [Guid("1F69D7A3-9B6D-463C-9B5A-5CE8D3E8B2B7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ICreateDirect3D11DeviceFromDXGIDeviceFactory
    {
        void CreateInstance([Out] out ICreateDirect3D11DeviceFromDXGIDevice ppInstance);
    }

    /// <summary>
    /// Creates an IDirect3DDevice from an IDXGIDevice native pointer.
    /// Uses CoCreateInstance with PreserveSig to avoid marshal errors.
    /// </summary>
    public static IDirect3DDevice CreateDirect3DDeviceFromDXGIDevice(IntPtr dxgiDevicePtr)
    {
        var factoryClassId = new Guid("1F69D7A3-9B6D-463C-9B5A-5CE8D3E8B2B7");
        var iid = typeof(ICreateDirect3D11DeviceFromDXGIDeviceFactory).GUID;

        int hr = CoCreateInstance(ref factoryClassId, IntPtr.Zero, 1, ref iid, out object factoryObj);
        if (hr != 0)
            throw new InvalidOperationException($"CoCreateInstance failed: 0x{hr:X8}");

        var factory = (ICreateDirect3D11DeviceFromDXGIDeviceFactory)factoryObj;
        factory.CreateInstance(out ICreateDirect3D11DeviceFromDXGIDevice creator);

        hr = creator.CreateFromDxgiDevice(dxgiDevicePtr, out object d3dDeviceObj);
        if (hr != 0)
            throw new InvalidOperationException($"CreateFromDxgiDevice failed: 0x{hr:X8}");

        return (IDirect3DDevice)d3dDeviceObj;
    }

    [DllImport("ole32.dll", PreserveSig = true)]
    private static extern int CoCreateInstance(
        ref Guid clsid,
        IntPtr punkOuter,
        uint clsctx,
        ref Guid iid,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
}
