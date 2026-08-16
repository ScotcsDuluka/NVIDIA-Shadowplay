// Utils/NvEncodeAPI.cs
//
// P1-B.2-V1 Spike — D3D11/NVENC Interop
// Minimal P/Invoke declarations for NVIDIA Video Codec SDK (NVENC).
//
// This file contains ONLY the declarations needed for Phase 4 of the spike:
//   1. NvEncodeAPICreateInstance — load the NVENC function table
//   2. NvEncOpenEncodeSessionEx — open a session bound to a D3D11 device
//   3. NvEncGetEncodeGUIDCount / NvEncGetEncodeGUIDs — enumerate codecs
//   4. NvEncGetEncodeProfileGUIDCount / NvEncGetEncodeProfileGUIDs — profiles
//   5. NvEncGetInputFormatCount / NvEncGetInputFormats — input formats
//   6. NvEncInitializeEncoder — configure the encoder
//   7. NvEncRegisterResource — register a D3D11 texture as NVENC input
//   8. NvEncUnregisterResource — unregister
//   9. NvEncReconfigureEncoder — (optional) reconfigure
//  10. NvEncDestroyEncoder — tear down
//
// OWNER must install NVIDIA Video Codec SDK:
//   1. Download from https://developer.nvidia.com/video-codec-sdk
//   2. Copy nvEncodeAPI.dll from SDK's Lib/x64/ to:
//        - This project's output directory (next to the .exe), OR
//        - C:\Windows\System32\
//   3. The header nvEncodeAPI.h is NOT needed at compile time because we
//      declare the structures here in C#. But you may consult it for
//      reference.
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

#pragma warning disable CS0649 // Field is never assigned to — struct layout for native interop
#pragma warning disable CA1810 // Initialize static fields inline — false positive on inline arrays

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

    // === Structs (subset — only what we need) ===

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
    {
        public int version;          // NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER
        public int deviceType;       // NV_ENC_DEVICE_DIRECTX or NV_ENC_DEVICE_CUDA
        public IntPtr device;        // ID3D11Device* (must be cast to IntPtr)
        public IntPtr reserved;
        public IntPtr event;         // optional, can be IntPtr.Zero
        public void* fOldApi;        // backward-compat — set to null
        public uint apiVersion;      // NVENCAPI_VERSION
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENC_REGISTER_RESOURCE
    {
        public int version;          // NV_ENC_REGISTER_RESOURCE_VER
        public int resourceType;     // NV_ENC_INPUT_RESOURCE_TYPE_*
        public int width;
        public int height;
        public int pitch;            // 0 for D3D11 textures
        public IntPtr resourceToRegister;  // ID3D11Texture2D* (cast to IntPtr)
        public IntPtr registeredResource;  // OUT: opaque handle from NVENC
        public int bufferFormat;     // NV_ENC_BUFFER_FORMAT_*
        public int bufferUsage;
        public uint reserved2438[243]; // pad to match SDK struct size (we use uint array)
        public IntPtr p2PDeviceHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENC_INITIALIZE_PARAMS
    {
        public int version;
        public Guid encodeGUID;
        public Guid presetGUID;
        public int encodeWidth;
        public int encodeHeight;
        public int darWidth;
        public int darHeight;
        public int frameRateNum;
        public int frameRateDen;
        public int enableEncodeAsync;
        public int enablePTD;
        public int reportSliceOffsets;
        public int enableSubFrameWrite;
        public int enableExternalMEHints;
        public int enableMEOnlyMode;
        public int enableWeightedPrediction;
        public int enableOutputInVidmem;
        public int reservedBitFields;
        public IntPtr privData;
        public IntPtr encodeConfig;       // NV_ENC_PRESET_CONFIG*
        public int maxEncodeWidth;
        public int maxEncodeHeight;
        public int maxMEHintCountPerBlock;
        public IntPtr tunnelParams;       // NV_ENC_TUNNEL_PARAMS*
        public int MEHint;
        public uint reserved[277];
    }

    // === Function table — populated by NvEncodeAPICreateInstance ===
    [StructLayout(LayoutKind.Sequential)]
    public struct NV_ENCODE_API_FUNCTION_LIST
    {
        public int version;
        public uint reserved0;
        public IntPtr reserved1[50];   // we don't need other functions for the spike
        public IntPtr nvEncOpenEncodeSessionEx;
        public IntPtr nvEncGetEncodeGUIDCount;
        public IntPtr nvEncGetEncodeGUIDs;
        public IntPtr nvEncGetEncodeProfileGUIDCount;
        public IntPtr nvEncGetEncodeProfileGUIDs;
        public IntPtr nvEncGetInputFormatCount;
        public IntPtr nvEncGetInputFormats;
        public IntPtr nvEncInitializeEncoder;
        public IntPtr nvEncRegisterResource;
        public IntPtr nvEncUnregisterResource;
        public IntPtr nvEncDestroyEncoder;
        // NOTE: real struct has ~58 function pointers. For spike we only need
        // a subset, so we pad with reserved fields. The real NVENC SDK uses
        // this struct's `version` field to know which functions are present.
        public IntPtr reservedTail[46];
    }

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

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncInitializeEncoderDelegate(
        IntPtr encoder, ref NV_ENC_INITIALIZE_PARAMS initParams);

    // === P/Invoke for the NVENC loader function ===
    // nvEncodeAPI.dll exports NvEncodeAPICreateInstance — this is the ONLY
    // function exported directly; all other functions are obtained via the
    // function table.
    [DllImport("nvEncodeAPI.dll", CallingConvention = CallingConvention.StdCall,
               SetLastError = false, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern int NvEncodeAPICreateInstance(ref NV_ENCODE_API_FUNCTION_LIST functionList);

    [DllImport("nvEncodeAPI.dll", CallingConvention = CallingConvention.StdCall,
               SetLastError = false, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern int NvEncodeAPIGetMaxSupportedVersion(out uint version);

    // === Version helpers ===
    // NVENCAPI_VERSION: major << 4 | minor  (per NVENC SDK docs)
    // NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER: sizeof(struct) | (NVENCAPI_VERSION << 16)
    // For SDK 12.x, NVENCAPI_VERSION = 0x0030 (12.0) up to 0x0031 (12.1) etc.
    // We set version = 0 to let NVENC reject and tell us what it expects.
    // OWNER: set this to match your NVENC SDK version.

    public const uint NVENCAPI_VERSION = 0x0032; // 12.2 — change to match your SDK

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
    public NvEncodeAPI.NvEncInitializeEncoderDelegate? InitializeEncoder { get; private set; }

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

            // Step 2: zero-init the function list and set version field
            _fnList = default;
            // The version field encodes NVENCAPI_VERSION | sizeof(struct)
            // For SDK 12.x, this is typically 0x0030 | sizeof(NV_ENCODE_API_FUNCTION_LIST)
            // OWNER: may need to set this to match SDK version. We try 0 first and
            // let NVENC reject if it's wrong.
            _fnList.version = (int)(NvEncodeAPI.NVENCAPI_VERSION | 0x10000);

            // Step 3: call NvEncodeAPICreateInstance
            status = NvEncodeAPI.NvEncodeAPICreateInstance(ref _fnList);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine(
                    $"NvEncodeAPICreateInstance failed: status={status} " +
                    $"({NvEncodeAPI.NvencStatusToString(status)})");
                return false;
            }

            // Step 4: marshal function pointers to delegates
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
            InitializeEncoder = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncInitializeEncoderDelegate>(
                _fnList.nvEncInitializeEncoder);

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
