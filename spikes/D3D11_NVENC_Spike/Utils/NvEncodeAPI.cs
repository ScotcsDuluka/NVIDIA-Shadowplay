// Utils/NvEncodeAPI.cs
//
// P1-B.2-V1 Spike — D3D11/NVENC Interop
// Minimal P/Invoke declarations for NVIDIA Video Codec SDK (NVENC).
//
// This file contains ONLY the declarations needed for Phase 4 of the spike:
//   1. NvEncodeAPICreateInstance — load the NVENC function table
//   2. NvEncOpenEncodeSessionEx — open a session bound to a D3D11 device
//   3. NvEncGetEncodeGUIDCount / NvEncGetEncodeGUIDs — enumerate codecs
//   4. NvEncGetInputFormatCount / NvEncGetInputFormats — input formats
//   5. NvEncRegisterResource — register a D3D11 texture as NVENC input
//   6. NvEncUnregisterResource — unregister
//   7. NvEncDestroyEncoder — tear down
//
// OWNER must install NVIDIA Video Codec SDK:
//   1. Download from https://developer.nvidia.com/video-codec-sdk
//   2. Copy nvEncodeAPI.dll from SDK's Lib/x64/ to:
//        - This project's output directory (next to the .exe), OR
//        - C:\Windows\System32\
//
// IMPORTANT — Struct layout notes:
//   The struct layouts below match NVIDIA Video Codec SDK 12.2.
//   If you use a different SDK version, you may need to adjust:
//     - NVENCAPI_VERSION constant
//     - Field offsets in NV_ENCODE_API_FUNCTION_LIST (function pointer order)
//     - Reserved array sizes in NV_ENC_REGISTER_RESOURCE
//   Consult nvEncodeAPI.h in your SDK for the authoritative definition.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

#pragma warning disable CS0649 // Field is never assigned to — struct layout for native interop

using System.Runtime.InteropServices;

namespace CaptureEngine.Video.Spike.D3D11.Utils;

public static class NvEncodeAPI
{
    // === NVENC status codes ===
    public const int NV_ENC_SUCCESS = 0;
    public const int NV_ENC_ERR_NO_ENCODE_DEVICE = -1;
    public const int NV_ENC_ERR_UNSUPPORTED_DEVICE = -2;
    public const int NV_ENC_ERR_INVALID_ENCODERDEVICE = -3;
    public const int NV_ENC_ERR_INVALID_DEVICE = -4;
    public const int NV_ENC_ERR_DEVICE_NOT_EXIST = -5;
    public const int NV_ENC_ERR_INVALID_PTR = -6;
    public const int NV_ENC_ERR_INVALID_EVENT = -7;
    public const int NV_ENC_ERR_INVALID_PARAM = -8;
    public const int NV_ENC_ERR_INVALID_CALL = -9;
    public const int NV_ENC_ERR_OUT_OF_MEMORY = -10;
    public const int NV_ENC_ERR_ENCODER_NOT_INITIALIZED = -11;
    public const int NV_ENC_ERR_UNSUPPORTED_PARAM = -12;
    public const int NV_ENC_ERR_LOCK_BUSY = -13;
    public const int NV_ENC_ERR_NOT_ENOUGH_INTRA_REFRESH_CARDS = -14;
    public const int NV_ENC_ERR_GENERIC = -15;
    public const int NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY = -16;
    public const int NV_ENC_ERR_UNIMPLEMENTED = -17;
    public const int NV_ENC_ERR_RESOURCE_REGISTER_FAILED = -18;
    public const int NV_ENC_ERR_RESOURCE_NOT_REGISTERED = -19;
    public const int NV_ENC_ERR_RESOURCE_NOT_MAPPED = -20;

    // === Device types ===
    public const int NV_ENC_DEVICE_DIRECTX = 0x01;
    public const int NV_ENC_DEVICE_CUDA = 0x02;

    // === Resource types ===
    public const int NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX = 0x00;
    public const int NV_ENC_INPUT_RESOURCE_TYPE_CUDADEVICEPTR = 0x01;

    // === Buffer formats (subset) ===
    public const int NV_ENC_BUFFER_FORMAT_UNDEFINED = 0x00000000;
    public const int NV_ENC_BUFFER_FORMAT_NV12 = 0x00000001;
    public const int NV_ENC_BUFFER_FORMAT_YV12 = 0x00000010;
    public const int NV_ENC_BUFFER_FORMAT_IYUV = 0x00000100;
    public const int NV_ENC_BUFFER_FORMAT_YUV444 = 0x00001000;
    public const int NV_ENC_BUFFER_FORMAT_ARGB = 0x00100000;
    public const int NV_ENC_BUFFER_FORMAT_ARGB10 = 0x00200000;
    public const int NV_ENC_BUFFER_FORMAT_AYUV = 0x00400000;
    public const int NV_ENC_BUFFER_FORMAT_ABGR = 0x00800000;
    public const int NV_ENC_BUFFER_FORMAT_ABGR10 = 0x01000000;

    // === Codec GUIDs (predefined by NVIDIA) ===
    public static readonly Guid NV_ENC_CODEC_H264_GUID =
        new(0x6bc82762, 0x4e63, 0x4ca4, 0xaa, 0x85, 0x1a, 0x4d, 0x8c, 0x39, 0x44, 0x0c);
    public static readonly Guid NV_ENC_CODEC_HEVC_GUID =
        new(0x790cdc88, 0x4522, 0x4ce7, 0x9c, 0x87, 0x14, 0x2b, 0x4c, 0x4c, 0x4a, 0xbc);
    public static readonly Guid NV_ENC_CODEC_AV1_GUID =
        new(0xc24b3f5d, 0x7354, 0x4ca4, 0x9c, 0xa2, 0x6a, 0x2b, 0x55, 0x4d, 0xb0, 0xa8);

    // === API version ===
    // NVENCAPI_VERSION = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION
    // For SDK 12.2: (12 << 4) | 2 = 0xC0 | 0x02 = 0xC2
    // Change this to match your NVENC SDK version.
    public const uint NVENCAPI_VERSION = 0x00C2; // SDK 12.2

    /// <summary>
    /// Computes the struct version field per NVENC's convention:
    ///   version = sizeof(struct) | (NVENCAPI_VERSION << 16)
    /// </summary>
    public static uint MakeStructVersion<T>() where T : struct
    {
        int structSize = Marshal.SizeOf<T>();
        return (uint)structSize | (NVENCAPI_VERSION << 16);
    }

    // === Structs ===
    //
    // NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS — minimal layout for the spike.
    // Field order MUST match nvEncodeAPI.h.
    // NOTE: 'event' is a C# keyword — escaped as '@event'.
    //
    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
    {
        public uint version;             // offset 0
        public int deviceType;           // offset 4  (NV_ENC_DEVICE_TYPE enum)
        public IntPtr device;            // offset 8  (void* — 8 bytes on x64)
        public IntPtr reserved;          // offset 16 (void*)
        public IntPtr @event;            // offset 24 (void* — 'event' is C# keyword, escape with @)
        public IntPtr inputParams;       // offset 32 (void* — NV_ENC_OPEN_ENCODE_SESSION_INPUT_PARAMS*)
        public uint apiVersion;          // offset 40
        // NOTE: real SDK struct has more reserved fields. For the spike, this
        // minimal layout should be accepted by NVENC because the version field
        // encodes our struct size. If NVENC returns NV_ENC_ERR_INVALID_PARAM,
        // OWNER must consult nvEncodeAPI.h and add the missing reserved fields.
    }
    // Expected size: 44 bytes + 4 padding = 48 bytes

    //
    // NV_ENC_REGISTER_RESOURCE — for NvEncRegisterResource.
    // Matches nvEncodeAPI.h layout for SDK 12.x.
    //
    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENC_REGISTER_RESOURCE
    {
        public uint version;                 // offset 0
        public int resourceType;             // offset 4  (NV_ENC_INPUT_RESOURCE_TYPE enum)
        public int width;                    // offset 8
        public int height;                   // offset 12
        public int pitch;                    // offset 16
        // 4 bytes padding to align IntPtr to 8-byte boundary
        public IntPtr resourceToRegister;    // offset 24 (void* — ID3D11Texture2D*)
        public IntPtr registeredResource;    // offset 32 (void* — OUT, opaque handle from NVENC)
        public int bufferFormat;             // offset 40 (NV_ENC_BUFFER_FORMAT enum)
        public int bufferUsage;              // offset 44
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 243)]
        public uint[] reserved2438;          // offset 48 — 243 × 4 = 972 bytes of reserved padding
        // padding to align next IntPtr
        public IntPtr p2PDeviceHandle;       // offset ~1024 (void*)
    }
    // Expected size: ~1032 bytes (matches SDK 12.x)

    //
    // NV_ENCODE_API_FUNCTION_LIST — function pointer table.
    // Populated by NvEncodeAPICreateInstance.
    //
    // CRITICAL: Field order MUST match nvEncodeAPI.h exactly, because
    // NvEncodeAPICreateInstance writes function pointers at these offsets.
    //
    // Layout below matches NVIDIA Video Codec SDK 12.x.
    // If using a different SDK version, consult nvEncodeAPI.h for the
    // correct function pointer order.
    //
    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENCODE_API_FUNCTION_LIST
    {
        public uint version;                            // offset 0
        public uint reserved;                           // offset 4
        public IntPtr nvEncOpenEncodeSession;           // offset 8    (NOT Ex — legacy)
        public IntPtr nvEncGetEncodeGUIDCount;          // offset 16
        public IntPtr nvEncGetEncodeGUIDs;              // offset 24
        public IntPtr nvEncGetEncodeProfileGUIDCount;   // offset 32
        public IntPtr nvEncGetEncodeProfileGUIDs;       // offset 40
        public IntPtr nvEncGetInputFormatCount;         // offset 48
        public IntPtr nvEncGetInputFormats;             // offset 56
        public IntPtr nvEncGetEncodeCaps;               // offset 64
        public IntPtr nvEncGetEncodePresetCount;        // offset 72
        public IntPtr nvEncGetEncodePresetGUIDs;        // offset 80
        public IntPtr nvEncGetEncodePresetConfig;       // offset 88
        public IntPtr nvEncGetEncodePresetConfigEx;     // offset 96
        public IntPtr nvEncInitializeEncoder;           // offset 104
        public IntPtr nvEncRegisterResource;            // offset 112
        public IntPtr nvEncRegisterResourceEx;          // offset 120  (12.0+)
        public IntPtr nvEncMapInputResource;            // offset 128
        public IntPtr nvEncUnmapInputResource;          // offset 136
        public IntPtr nvEncDestroyEncoder;              // offset 144
        public IntPtr nvEncInvalidateRefFrames;         // offset 152
        public IntPtr nvEncEncodePicture;               // offset 160
        public IntPtr nvEncLockBitstream;               // offset 168
        public IntPtr nvEncUnlockBitstream;             // offset 176
        public IntPtr nvEncLockInputBuffer;             // offset 184
        public IntPtr nvEncUnlockInputBuffer;           // offset 192
        public IntPtr nvEncGetEncodeStats;              // offset 200
        public IntPtr nvEncGetSequenceParams;           // offset 208
        public IntPtr nvEncEventNotify;                 // offset 216
        public IntPtr nvEncRegisterAsyncEvent;          // offset 224
        public IntPtr nvEncUnregisterAsyncEvent;        // offset 232
        public IntPtr nvEncGetLastErrorString;          // offset 240
        public IntPtr reserved1;                        // offset 248
        public IntPtr nvEncSetIOCudaStreams;            // offset 256
        public IntPtr nvEncGetSequenceParamEx;          // offset 264
        public IntPtr nvEncLookaheadPicture;            // offset 272
        public IntPtr nvEncGetEncodeBufferGCCount;      // offset 280
        public IntPtr reserved2;                        // offset 288
        public IntPtr reserved3;                        // offset 296
        public IntPtr nvEncReconfigureEncoder;          // offset 304
        public IntPtr reserved4;                        // offset 312
        public IntPtr reserved5;                        // offset 320
        public IntPtr reserved6;                        // offset 328
        public IntPtr reserved7;                        // offset 336
        public IntPtr reserved8;                        // offset 344
        public IntPtr reserved9;                        // offset 352
        public IntPtr reserved10;                       // offset 360
        public IntPtr nvEncOpenEncodeSessionEx;         // offset 368  ★ Ex is here
        public IntPtr reserved11;                       // offset 376
        public IntPtr reserved12;                       // offset 384
        public IntPtr reserved13;                       // offset 392
        public IntPtr reserved14;                       // offset 400
        public IntPtr reserved15;                       // offset 408
        public IntPtr reserved16;                       // offset 416
        public IntPtr reserved17;                       // offset 424
        public IntPtr reserved18;                       // offset 432
        public IntPtr reserved19;                       // offset 440
        public IntPtr reserved20;                       // offset 448
        public IntPtr reserved21;                       // offset 456
        public IntPtr reserved22;                       // offset 464
        public IntPtr reserved23;                       // offset 472
    }
    // Expected size: 480 bytes (8 + 59 × 8) — matches SDK 12.x

    // === Delegates for function table entries we use ===
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncOpenEncodeSessionExDelegate(
        ref NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS sessionParams,
        out IntPtr encoder);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncGetEncodeGUIDCountDelegate(
        IntPtr encoder, out int count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncGetEncodeGUIDsDelegate(
        IntPtr encoder, [Out] Guid[] guidArray, int arraySize, out int actualCount);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncGetInputFormatCountDelegate(
        IntPtr encoder, Guid encodeGUID, out int count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncGetInputFormatsDelegate(
        IntPtr encoder, Guid encodeGUID, [Out] int[] formatArray, int arraySize, out int actualCount);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncRegisterResourceDelegate(
        IntPtr encoder, ref NV_ENC_REGISTER_RESOURCE registerParams);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncUnregisterResourceDelegate(
        IntPtr encoder, IntPtr registeredResource);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncDestroyEncoderDelegate(IntPtr encoder);

    // === P/Invoke for the NVENC loader functions ===
    // nvEncodeAPI.dll exports only these two functions directly; all other
    // functions are obtained via the function table.
    [DllImport("nvEncodeAPI.dll", CallingConvention = CallingConvention.StdCall,
               SetLastError = false, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern int NvEncodeAPICreateInstance(ref NV_ENCODE_API_FUNCTION_LIST functionList);

    [DllImport("nvEncodeAPI.dll", CallingConvention = CallingConvention.StdCall,
               SetLastError = false, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern int NvEncodeAPIGetMaxSupportedVersion(out uint version);

    public static string NvencStatusToString(int status)
    {
        return status switch
        {
            NV_ENC_SUCCESS => "NV_ENC_SUCCESS",
            NV_ENC_ERR_NO_ENCODE_DEVICE => "NV_ENC_ERR_NO_ENCODE_DEVICE",
            NV_ENC_ERR_UNSUPPORTED_DEVICE => "NV_ENC_ERR_UNSUPPORTED_DEVICE",
            NV_ENC_ERR_INVALID_ENCODERDEVICE => "NV_ENC_ERR_INVALID_ENCODERDEVICE",
            NV_ENC_ERR_INVALID_DEVICE => "NV_ENC_ERR_INVALID_DEVICE",
            NV_ENC_ERR_DEVICE_NOT_EXIST => "NV_ENC_ERR_DEVICE_NOT_EXIST",
            NV_ENC_ERR_INVALID_PTR => "NV_ENC_ERR_INVALID_PTR",
            NV_ENC_ERR_INVALID_EVENT => "NV_ENC_ERR_INVALID_EVENT",
            NV_ENC_ERR_INVALID_PARAM => "NV_ENC_ERR_INVALID_PARAM",
            NV_ENC_ERR_INVALID_CALL => "NV_ENC_ERR_INVALID_CALL",
            NV_ENC_ERR_OUT_OF_MEMORY => "NV_ENC_ERR_OUT_OF_MEMORY",
            NV_ENC_ERR_ENCODER_NOT_INITIALIZED => "NV_ENC_ERR_ENCODER_NOT_INITIALIZED",
            NV_ENC_ERR_UNSUPPORTED_PARAM => "NV_ENC_ERR_UNSUPPORTED_PARAM",
            NV_ENC_ERR_LOCK_BUSY => "NV_ENC_ERR_LOCK_BUSY",
            NV_ENC_ERR_GENERIC => "NV_ENC_ERR_GENERIC",
            NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY => "NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY",
            NV_ENC_ERR_UNIMPLEMENTED => "NV_ENC_ERR_UNIMPLEMENTED",
            NV_ENC_ERR_RESOURCE_REGISTER_FAILED => "NV_ENC_ERR_RESOURCE_REGISTER_FAILED",
            NV_ENC_ERR_RESOURCE_NOT_REGISTERED => "NV_ENC_ERR_RESOURCE_NOT_REGISTERED",
            NV_ENC_ERR_RESOURCE_NOT_MAPPED => "NV_ENC_ERR_RESOURCE_NOT_MAPPED",
            _ => $"NV_ENC_ERR_UNKNOWN({status})"
        };
    }
}

/// <summary>
/// High-level wrapper around the NVENC P/Invoke declarations.
/// Handles function table loading and exposes only what the spike needs.
/// </summary>
public sealed class NvEncFunctionTable : IDisposable
{
    private NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST _fnList;
    private bool _loaded;

    public NvEncodeAPI.NvEncOpenEncodeSessionExDelegate? OpenEncodeSessionEx { get; private set; }
    public NvEncodeAPI.NvEncGetEncodeGUIDCountDelegate? GetEncodeGUIDCount { get; private set; }
    public NvEncodeAPI.NvEncGetEncodeGUIDsDelegate? GetEncodeGUIDs { get; private set; }
    public NvEncodeAPI.NvEncGetInputFormatCountDelegate? GetInputFormatCount { get; private set; }
    public NvEncodeAPI.NvEncGetInputFormatsDelegate? GetInputFormats { get; private set; }
    public NvEncodeAPI.NvEncRegisterResourceDelegate? RegisterResource { get; private set; }
    public NvEncodeAPI.NvEncUnregisterResourceDelegate? UnregisterResource { get; private set; }
    public NvEncodeAPI.NvEncDestroyEncoderDelegate? DestroyEncoder { get; private set; }

    public uint MaxSupportedApiVersion { get; private set; }

    /// <summary>
    /// Loads the NVENC function table. Returns true on success.
    /// Throws DllNotFoundException if nvEncodeAPI.dll is not on the path.
    /// </summary>
    public bool TryLoad()
    {
        try
        {
            // Step 1: query max supported version
            int status = NvEncodeAPI.NvEncodeAPIGetMaxSupportedVersion(out uint ver);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine(
                    $"NvEncodeAPIGetMaxSupportedVersion failed: status={status}");
                return false;
            }
            MaxSupportedApiVersion = ver;
            Console.WriteLine(
                $"  NVENC max supported API: major={(ver >> 4) & 0xF}, minor={ver & 0xF}");
            Console.WriteLine(
                $"  Spike requests API:     major={(NvEncodeAPI.NVENCAPI_VERSION >> 4) & 0xF}, minor={NvEncodeAPI.NVENCAPI_VERSION & 0xF}");

            // Step 2: zero-init the function list and set version field.
            // version = sizeof(struct) | (NVENCAPI_VERSION << 16)
            _fnList = default;
            _fnList.version = NvEncodeAPI.MakeStructVersion<NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST>();
            Console.WriteLine($"  Function table version: 0x{_fnList.version:X8} (size={Marshal.SizeOf<NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST>()} bytes)");

            // Step 3: call NvEncodeAPICreateInstance
            status = NvEncodeAPI.NvEncodeAPICreateInstance(ref _fnList);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine(
                    $"NvEncodeAPICreateInstance failed: status={status} " +
                    $"({NvEncodeAPI.NvencStatusToString(status)})");
                Console.Error.WriteLine("  Possible causes:");
                Console.Error.WriteLine("    - NVENCAPI_VERSION in NvEncodeAPI.cs does not match installed SDK");
                Console.Error.WriteLine("    - Struct size mismatch (check NV_ENCODE_API_FUNCTION_LIST layout)");
                Console.Error.WriteLine("    - Driver version too old");
                return false;
            }

            // Step 4: marshal function pointers to delegates.
            // CRITICAL: these field accesses assume the struct layout matches
            // the NVENC SDK's function pointer order. If you get
            // NullReferenceException or AccessViolationException here, the
            // struct layout is wrong — consult nvEncodeAPI.h.
            OpenEncodeSessionEx = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncOpenEncodeSessionExDelegate>(
                _fnList.nvEncOpenEncodeSessionEx);
            GetEncodeGUIDCount = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncGetEncodeGUIDCountDelegate>(
                _fnList.nvEncGetEncodeGUIDCount);
            GetEncodeGUIDs = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncGetEncodeGUIDsDelegate>(
                _fnList.nvEncGetEncodeGUIDs);
            GetInputFormatCount = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncGetInputFormatCountDelegate>(
                _fnList.nvEncGetInputFormatCount);
            GetInputFormats = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncGetInputFormatsDelegate>(
                _fnList.nvEncGetInputFormats);
            RegisterResource = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncRegisterResourceDelegate>(
                _fnList.nvEncRegisterResource);
            UnregisterResource = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncUnregisterResourceDelegate>(
                _fnList.nvEncUnregisterResource);
            DestroyEncoder = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncDestroyEncoderDelegate>(
                _fnList.nvEncDestroyEncoder);

            _loaded = true;
            return true;
        }
        catch (DllNotFoundException ex)
        {
            Console.Error.WriteLine($"ERROR: nvEncodeAPI.dll not found.");
            Console.Error.WriteLine("  Install NVIDIA Video Codec SDK and copy nvEncodeAPI.dll to:");
            Console.Error.WriteLine("    - This project's bin/x64/Debug/net8.0-windows/ directory, OR");
            Console.Error.WriteLine("    - C:\\Windows\\System32\\");
            Console.Error.WriteLine($"  Exception: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR loading NVENC: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (!_loaded) return;
        _loaded = false;
        // Function pointers don't need to be freed — they're pointers into nvEncodeAPI.dll
    }
}
