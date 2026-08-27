// WasapiDirectInterop.cs — P13.1 spike: zero-dependency WASAPI COM interop.
//
// WHY: NAudio's WasapiLoopbackCapture drops devicePosition/qpcPosition.
// This file declares just enough WASAPI to read them directly, so the spike
// can prove the DATA independent of any wrapper library. If the direct path
// works here, the P13.2 engine class can either keep this (~150 lines) or
// switch to NAudio's raw interface wrapper (see Program.cs --via naudio).
//
// All methods are declared void => runtime marshals HRESULT to COMException.
// That is intentional: a failed Initialize surfaces its AUDCLNT_E_* code
// directly in the exception message, which is exactly the evidence a spike wants.

using System;
using System.Runtime.InteropServices;

namespace V5_WASAPI_Position_Spike
{
    internal static class WasapiDirectInterop
    {
        // ── Constants ────────────────────────────────────────────────────
        public const int CLSCTX_ALL = 0x17;                    // INPROC_SERVER|INPROC_HANDLER|LOCAL_SERVER|REMOTE_SERVER
        public const int eRender = 0;                          // DATA_FLOW
        public const int eCapture = 1;
        public const int eConsole = 0;                         // ROLE
        public const int AUDCLNT_STREAMFLAGS_LOOPBACK = 0x00020000;

        public static readonly Guid CLSID_MMDeviceEnumerator =
            new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
        public static readonly Guid IID_IAudioClient =
            new Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2");
        public static readonly Guid IID_IAudioCaptureClient =
            new Guid("C8ADBD64-E71E-48A0-A4DE-185C395CD317");

        // ── IMMDeviceEnumerator ──────────────────────────────────────────
        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMMDeviceEnumerator
        {
            void EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr devices); // vtable slot 0 (unused)
            void GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice device);
            void GetDevice(IntPtr reserved);                                           // slot 2 (unused)
            void RegisterEndpointNotificationCallback(IntPtr callback);                // slot 3 (unused)
            void UnregisterEndpointNotificationCallback(IntPtr callback);              // slot 4 (unused)
        }

        // ── IMMDevice ────────────────────────────────────────────────────
        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMMDevice
        {
            void Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams,
                          [MarshalAs(UnmanagedType.IUnknown)] out object iface);
            void OpenPropertyStore(IntPtr reserved);                                   // slot 1 (unused)
            void GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
            void GetState(out int state);
        }

        // ── IAudioClient ─────────────────────────────────────────────────
        [ComImport]
        [Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioClient
        {
            // pFormat: pass the RAW pointer from GetMixFormat — do NOT round-
            // trip through a WAVEFORMATEX struct copy. The modern mix format
            // is WAVE_FORMAT_EXTENSIBLE (tag=65534, cbSize=22 => 40 bytes);
            // a struct-copy buffer would be 18 bytes => E_INVALIDARG.
            void Initialize(int shareMode, int streamFlags, long bufferDuration100ns,
                            long periodicity100ns, IntPtr pFormat, ref Guid sessionGuid);
            void GetBufferSize(out uint bufferFrames);
            void GetStreamLatency(out long latency100ns);
            void GetCurrentPadding(out uint paddingFrames);
            void IsFormatSupported(int shareMode, IntPtr pFormat, out IntPtr closestMatch);
            void GetMixFormat(out IntPtr ppFormat);
            void GetDevicePeriod(out long defaultPeriod100ns, out long minPeriod100ns);
            void Start();
            void Stop();
            void Reset();
            void SetEventHandle(IntPtr handle);
            void GetService(ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object service);
        }

        // ── IAudioCaptureClient ──────────────────────────────────────────
        [ComImport]
        [Guid("C8ADBD64-E71E-48A0-A4DE-185C395CD317")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioCaptureClient
        {
            // THE payload of this spike: hardware stamps per packet.
            // devicePosition : device sample-clock frames since stream start
            // qpcPosition    : performance counter when the device read that
            //                  position (unit verified empirically by the
            //                  spike — QPC ticks vs 100ns, see README)
            void GetBuffer(out IntPtr data, out int framesInPacket, out int flags,
                           out long devicePosition, out long qpcPosition);
            void ReleaseBuffer(int bytesWritten);   // capture side passes 0
            void GetNextPacketSize(out int numFramesInNextPacket);
        }

        // ── WAVEFORMATEX ─────────────────────────────────────────────────
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        /// <summary>Default device (render for loopback, capture for mic).</summary>
        public static IMMDevice GetDefaultDevice(int dataFlow)
        {
            var type = Type.GetTypeFromCLSID(CLSID_MMDeviceEnumerator);
            var enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(type);
            IMMDevice device;
            enumerator.GetDefaultAudioEndpoint(dataFlow, eConsole, out device);
            return device;
        }

        /// <summary>Activate IAudioClient on a device.</summary>
        public static IAudioClient ActivateAudioClient(IMMDevice device)
        {
            object raw;
            var iid = IID_IAudioClient;
            device.Activate(ref iid, CLSCTX_ALL, IntPtr.Zero, out raw);
            return (IAudioClient)raw;
        }

        /// <summary>Get the capture service from an initialized client.</summary>
        public static IAudioCaptureClient GetCaptureClient(IAudioClient client)
        {
            object raw;
            var iid = IID_IAudioCaptureClient;
            client.GetService(ref iid, out raw);
            return (IAudioCaptureClient)raw;
        }

        public static string FormatToString(WAVEFORMATEX f)
        {
            return f.nChannels + "ch " + f.nSamplesPerSec + "Hz " +
                   f.wBitsPerSample + "bit tag=" + f.wFormatTag +
                   " blockAlign=" + f.nBlockAlign;
        }
    }
}
