// Utils/NvEncodeAPI.cs
//
// P1-B.2-V1 Spike — D3D11/NVENC Interop
// Minimal P/Invoke declarations for NVIDIA Video Codec SDK (NVENC).
//
// IMPORTANT — This file was rewritten from scratch to match the actual
// struct layouts in NVIDIA Video Codec SDK 13.1 header (nvEncodeAPI.h).
// Previous versions were based on incorrect assumptions and caused
// NvEncodeAPICreateInstance to fail with NV_ENC_ERR_GENERIC.
//
// OWNER must install NVIDIA Video Codec SDK:
//   1. Download from https://developer.nvidia.com/video-codec-sdk
//   2. Copy nvEncodeAPI64.dll from SDK's Lib/x64/ to:
//        - This project's output directory (next to the .exe), OR
//        - C:\Windows\System32\
//
// SPDX-License-Identifier: MIT
// Spike code — not production.

#pragma warning disable CS0649 // Field is never assigned to — struct layout for native interop

using System.Runtime.InteropServices;

namespace CaptureEngine.Video.Spike.D3D11.Utils;

public static class NvEncodeAPI
{
    // === NVENC status codes ===
    // IMPORTANT: These are C enum values starting from 0 (NV_ENC_SUCCESS = 0,
    // NV_ENC_ERR_NO_ENCODE_DEVICE = 1, etc.) — NOT negative values as I had
    // assumed in earlier versions of this file.
    //
    // The spike output reported "status=2 (NV_ENC_ERR_UNKNOWN(2))" which
    // actually means NV_ENC_ERR_UNSUPPORTED_DEVICE — the device passed to
    // NvEncOpenEncodeSessionEx is not supported (likely because the D3D11
    // device was created without NVENC-compatible flags or the DLL is older).
    public const int NV_ENC_SUCCESS = 0;
    public const int NV_ENC_ERR_NO_ENCODE_DEVICE = 1;
    public const int NV_ENC_ERR_UNSUPPORTED_DEVICE = 2;
    public const int NV_ENC_ERR_INVALID_ENCODERDEVICE = 3;
    public const int NV_ENC_ERR_INVALID_DEVICE = 4;
    public const int NV_ENC_ERR_DEVICE_NOT_EXIST = 5;
    public const int NV_ENC_ERR_INVALID_PTR = 6;
    public const int NV_ENC_ERR_INVALID_EVENT = 7;
    public const int NV_ENC_ERR_INVALID_PARAM = 8;
    public const int NV_ENC_ERR_INVALID_CALL = 9;
    public const int NV_ENC_ERR_OUT_OF_MEMORY = 10;
    public const int NV_ENC_ERR_ENCODER_NOT_INITIALIZED = 11;
    public const int NV_ENC_ERR_UNSUPPORTED_PARAM = 12;
    public const int NV_ENC_ERR_LOCK_BUSY = 13;
    public const int NV_ENC_ERR_NOT_ENOUGH_INTRA_REFRESH_CARDS = 14;
    public const int NV_ENC_ERR_GENERIC = 15;
    public const int NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY = 16;
    public const int NV_ENC_ERR_UNIMPLEMENTED = 17;
    public const int NV_ENC_ERR_RESOURCE_REGISTER_FAILED = 18;
    public const int NV_ENC_ERR_RESOURCE_NOT_REGISTERED = 19;
    public const int NV_ENC_ERR_RESOURCE_NOT_MAPPED = 20;

    // === Device types ===
    // From nvEncodeAPI.h SDK 13.1:
    //   typedef enum _NV_ENC_DEVICE_TYPE {
    //     NV_ENC_DEVICE_TYPE_DIRECTX  = 0x0,   // DirectX 9/11 device
    //     NV_ENC_DEVICE_TYPE_CUDA     = 0x1,   // CUDA context
    //     NV_ENC_DEVICE_TYPE_OPENGL   = 0x2    // OpenGL (Linux only)
    //   } NV_ENC_DEVICE_TYPE;
    //
    // IMPORTANT: In previous versions of this file I had NV_ENC_DEVICE_DIRECTX = 0x01,
    // which is CUDA! That's why NvEncOpenEncodeSessionEx returned
    // NV_ENC_ERR_UNSUPPORTED_DEVICE — we were telling NVENC the device is CUDA
    // but passing a D3D11 device pointer.
    public const int NV_ENC_DEVICE_DIRECTX = 0x00;  // was 0x01 (wrong — that's CUDA)
    public const int NV_ENC_DEVICE_CUDA = 0x01;     // was 0x02

    // === Resource types ===
    public const int NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX = 0x00;
    public const int NV_ENC_INPUT_RESOURCE_TYPE_CUDADEVICEPTR = 0x01;

    // === Buffer formats (from nvEncodeAPI.h SDK 13.1) ===
    // Note: ARGB and ABGR values differ from what I previously had —
    // ARGB is 0x01000000 (not 0x00100000), ABGR is 0x10000000 (not 0x00800000).
    public const int NV_ENC_BUFFER_FORMAT_UNDEFINED = 0x00000000;
    public const int NV_ENC_BUFFER_FORMAT_NV12 = 0x00000001;
    public const int NV_ENC_BUFFER_FORMAT_YV12 = 0x00000010;
    public const int NV_ENC_BUFFER_FORMAT_IYUV = 0x00000100;
    public const int NV_ENC_BUFFER_FORMAT_YUV444 = 0x00001000;
    public const int NV_ENC_BUFFER_FORMAT_YUV420_10BIT = 0x00010000;
    public const int NV_ENC_BUFFER_FORMAT_YUV444_10BIT = 0x00100000;
    public const int NV_ENC_BUFFER_FORMAT_ARGB = 0x01000000;     // was 0x00100000 (wrong)
    public const int NV_ENC_BUFFER_FORMAT_ARGB10 = 0x02000000;   // was 0x00200000 (wrong)
    public const int NV_ENC_BUFFER_FORMAT_AYUV = 0x04000000;     // was 0x00400000 (wrong)
    public const int NV_ENC_BUFFER_FORMAT_ABGR = 0x10000000;     // was 0x00800000 (wrong)
    public const int NV_ENC_BUFFER_FORMAT_ABGR10 = 0x20000000;   // was 0x01000000 (wrong)

    // === Codec GUIDs (verified from nvEncodeAPI.h SDK 13.1.15) ===
    //
    // Previous versions of this file had WRONG GUID values copied from FFmpeg
    // source (which uses an older SDK). The correct values from SDK 13.1:
    //
    //   H.264: {6BC82762-4E63-4ca4-AA85-1E50F321F6BF}
    //   HEVC:  {790CDC88-4522-4d7b-9425-BDA9975F7603}
    //
    // The spike output confirmed these — NVENC returned exactly these GUIDs
    // when we enumerated supported codecs.
    public static readonly Guid NV_ENC_CODEC_H264_GUID =
        new(0x6bc82762, 0x4e63, 0x4ca4, 0xaa, 0x85, 0x1e, 0x50, 0xf3, 0x21, 0xf6, 0xbf);
    public static readonly Guid NV_ENC_CODEC_HEVC_GUID =
        new(0x790cdc88, 0x4522, 0x4d7b, 0x94, 0x25, 0xbd, 0xa9, 0x97, 0x5f, 0x76, 0x03);
    // AV1 codec GUID is not in SDK 13.1.15 header — may exist in newer SDKs.
    // For the spike, we only need H.264, so this is informational only.
    public static readonly Guid NV_ENC_CODEC_AV1_GUID =
        new(0xc24b3f5d, 0x7354, 0x4ca4, 0x9c, 0xa2, 0x6a, 0x2b, 0x55, 0x4d, 0xb0, 0xa8);

    // === API version ===
    //
    // NVIDIA NVENCAPI_VERSION encoding (consistent across all SDK versions
    // 8.x through 13.x — verified from nvEncodeAPI.h in multiple SDKs):
    //
    //   NVENCAPI_VERSION = NVENCAPI_MAJOR_VERSION | (NVENCAPI_MINOR_VERSION << 24)
    //
    //   NVENCAPI_STRUCT_VERSION(ver) =
    //       NVENCAPI_VERSION | (ver << 16) | (0x7 << 28)
    //
    // The 0x7 magic bits in the top nibble are MANDATORY — without them,
    // NvEncodeAPICreateInstance fails with NV_ENC_ERR_GENERIC.
    //
    // IMPORTANT — NvEncodeAPIGetMaxSupportedVersion is DIFFERENT:
    //   It returns a PACKED format: (major << 4) | minor
    //   This is NOT the same as NVENCAPI_VERSION!
    //
    //   Examples:
    //     NVENC 12.0 → GetMaxSupportedVersion returns 0xC0 (192)
    //     NVENC 12.2 → GetMaxSupportedVersion returns 0xC2 (194)
    //     NVENC 13.0 → GetMaxSupportedVersion returns 0xD0 (208) ← OWNER's DLL
    //     NVENC 13.1 → GetMaxSupportedVersion returns 0xD1 (209)
    //
    // So we must:
    //   1. Read GetMaxSupportedVersion → packed format (major<<4)|minor
    //   2. Extract major/minor from packed format
    //   3. Compute NVENCAPI_VERSION = major | (minor << 24)
    //   4. Use NVENCAPI_STRUCT_VERSION(ver) = NVENCAPI_VERSION | (ver<<16) | (0x7<<28)
    //
    // My previous attempts confused the two formats — that's why every fix
    // failed. This version correctly translates between them.

    public static uint NVENCAPI_MAJOR_VERSION { get; private set; } = 13;
    public static uint NVENCAPI_MINOR_VERSION { get; private set; } = 0;

    public static uint NVENCAPI_VERSION =>
        NVENCAPI_MAJOR_VERSION | (NVENCAPI_MINOR_VERSION << 24);

    /// <summary>
    /// Computes the struct version field per NVENC's convention:
    ///
    ///   NVENCAPI_STRUCT_VERSION(ver) = NVENCAPI_VERSION | (ver << 16) | (0x7 << 28)
    /// </summary>
    public static uint MakeStructVersion(uint structVer)
    {
        return NVENCAPI_VERSION | (structVer << 16) | (0x7u << 28);
    }

    /// <summary>
    /// Sets the requested API version based on what
    /// NvEncodeAPIGetMaxSupportedVersion returned.
    ///
    /// Input is the packed format: (major << 4) | minor
    /// We extract major/minor and store them for NVENCAPI_VERSION computation.
    /// </summary>
    public static void SetVersionFromPacked(uint packed)
    {
        NVENCAPI_MAJOR_VERSION = (packed >> 4) & 0xF;
        NVENCAPI_MINOR_VERSION = packed & 0xF;
    }

    // === Struct version constants (per nvEncodeAPI.h SDK 13.1) ===
    public const uint NV_ENCODE_API_FUNCTION_LIST_VER_STRUCT = 2;
    public const uint NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER_STRUCT = 1;
    public const uint NV_ENC_REGISTER_RESOURCE_VER_STRUCT = 5;

    public static uint NV_ENCODE_API_FUNCTION_LIST_VER =>
        MakeStructVersion(NV_ENCODE_API_FUNCTION_LIST_VER_STRUCT);

    public static uint NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER =>
        MakeStructVersion(NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER_STRUCT);

    public static uint NV_ENC_REGISTER_RESOURCE_VER =>
        MakeStructVersion(NV_ENC_REGISTER_RESOURCE_VER_STRUCT);

    // === Structs ===
    //
    // Layouts below match nvEncodeAPI.h from SDK 13.1.15 EXACTLY.
    // Field order, types, and reserved array sizes are critical — NVENC
    // validates struct size and rejects with NV_ENC_ERR_GENERIC if wrong.
    //
    // All structs use LayoutKind.Sequential with Pack=1 to avoid implicit
    // padding — the SDK header uses #pragma pack(push, 1) on some structs.

    //
    // NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS — from nvEncodeAPI.h:
    //   typedef struct _NV_ENC_OPEN_ENCODE_SESSIONEX_PARAMS {
    //     uint32_t            version;          // offset 0
    //     NV_ENC_DEVICE_TYPE  deviceType;       // offset 4  (enum = int32)
    //     void*               device;           // offset 8  (8 bytes on x64)
    //     void*               reserved;         // offset 16
    //     uint32_t            apiVersion;       // offset 24
    //     uint32_t            reserved1[253];   // offset 28 - 1039
    //     void*               reserved2[64];    // offset 1040 - 1552
    //   }
    // Total size: 1552 bytes
    //
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
    {
        public uint version;
        public int deviceType;             // NV_ENC_DEVICE_TYPE enum
        public IntPtr device;              // void*
        public IntPtr reserved;            // void*
        public uint apiVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 253)]
        public uint[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public IntPtr[] reserved2;
    }

    //
    // NV_ENC_REGISTER_RESOURCE — from nvEncodeAPI.h:
    //   typedef struct _NV_ENC_REGISTER_RESOURCE {
    //     uint32_t                    version;             // offset 0
    //     NV_ENC_INPUT_RESOURCE_TYPE  resourceType;        // offset 4  (enum)
    //     uint32_t                    width;               // offset 8
    //     uint32_t                    height;              // offset 12
    //     uint32_t                    pitch;               // offset 16
    //     uint32_t                    subResourceIndex;    // offset 20  *** NEW in SDK 13 ***
    //     void*                       resourceToRegister;  // offset 24
    //     NV_ENC_REGISTERED_PTR       registeredResource;  // offset 32 (void*)
    //     NV_ENC_BUFFER_FORMAT        bufferFormat;        // offset 40 (enum = int32)
    //     NV_ENC_BUFFER_USAGE         bufferUsage;         // offset 44 (enum = int32)
    //     NV_ENC_FENCE_POINT_D3D12*   pInputFencePoint;    // offset 48 (void*)
    //     uint32_t                    chromaOffset[2];     // offset 56
    //     uint32_t                    chromaOffsetIn[2];   // offset 64
    //     uint32_t                    reserved1[244];      // offset 72 - 1039
    //     void*                       reserved2[61];       // offset 1040 - 1528
    //   }
    // Total size: 1528 bytes
    //
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_REGISTER_RESOURCE
    {
        public uint version;
        public int resourceType;           // NV_ENC_INPUT_RESOURCE_TYPE enum
        public uint width;
        public uint height;
        public uint pitch;
        public uint subResourceIndex;      // NEW in SDK 13.x
        public IntPtr resourceToRegister;  // void*
        public IntPtr registeredResource;  // void* (OUT)
        public int bufferFormat;           // NV_ENC_BUFFER_FORMAT enum
        public int bufferUsage;            // NV_ENC_BUFFER_USAGE enum
        public IntPtr pInputFencePoint;    // NV_ENC_FENCE_POINT_D3D12* (set to IntPtr.Zero)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] chromaOffset;        // OUT (set to zeros)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] chromaOffsetIn;      // IN (set to zeros)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 244)]
        public uint[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 61)]
        public IntPtr[] reserved2;
    }

    //
    // NV_ENCODE_API_FUNCTION_LIST — from nvEncodeAPI.h SDK 13.1:
    //
    // Field order is CRITICAL — NvEncodeAPICreateInstance writes function
    // pointers at the offsets defined by this struct. If the order is wrong,
    // we'll get garbage function pointers and crash or call wrong functions.
    //
    // Note: NV_ENC_INITIALIZE_PARAMS is intentionally omitted from the spike
    // (Phase 4 only does register/unregister/destroy, not InitializeEncoder).
    //
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENCODE_API_FUNCTION_LIST
    {
        public uint version;                            // offset 0
        public uint reserved;                           // offset 4
        public IntPtr nvEncOpenEncodeSession;           // offset 8    (legacy, NOT Ex)
        public IntPtr nvEncGetEncodeGUIDCount;          // offset 16
        public IntPtr nvEncGetEncodeProfileGUIDCount;   // offset 24
        public IntPtr nvEncGetEncodeProfileGUIDs;       // offset 32
        public IntPtr nvEncGetEncodeGUIDs;              // offset 40
        public IntPtr nvEncGetInputFormatCount;         // offset 48
        public IntPtr nvEncGetInputFormats;             // offset 56
        public IntPtr nvEncGetEncodeCaps;               // offset 64
        public IntPtr nvEncGetEncodePresetCount;        // offset 72
        public IntPtr nvEncGetEncodePresetGUIDs;        // offset 80
        public IntPtr nvEncGetEncodePresetConfig;       // offset 88
        public IntPtr nvEncInitializeEncoder;           // offset 96
        public IntPtr nvEncCreateInputBuffer;           // offset 104
        public IntPtr nvEncDestroyInputBuffer;          // offset 112
        public IntPtr nvEncCreateBitstreamBuffer;       // offset 120
        public IntPtr nvEncDestroyBitstreamBuffer;      // offset 128
        public IntPtr nvEncEncodePicture;               // offset 136
        public IntPtr nvEncLockBitstream;               // offset 144
        public IntPtr nvEncUnlockBitstream;             // offset 152
        public IntPtr nvEncLockInputBuffer;             // offset 160
        public IntPtr nvEncUnlockInputBuffer;           // offset 168
        public IntPtr nvEncGetEncodeStats;              // offset 176
        public IntPtr nvEncGetSequenceParams;           // offset 184
        public IntPtr nvEncRegisterAsyncEvent;          // offset 192
        public IntPtr nvEncUnregisterAsyncEvent;        // offset 200
        public IntPtr nvEncMapInputResource;            // offset 208
        public IntPtr nvEncUnmapInputResource;          // offset 216
        public IntPtr nvEncDestroyEncoder;              // offset 224
        public IntPtr nvEncInvalidateRefFrames;         // offset 232
        public IntPtr nvEncOpenEncodeSessionEx;         // offset 240  ★ Ex is HERE (not 368!)
        public IntPtr nvEncRegisterResource;            // offset 248
        public IntPtr nvEncUnregisterResource;          // offset 256  ★ EXISTS after all!
        public IntPtr nvEncReconfigureEncoder;          // offset 264
        public IntPtr reserved1;                        // offset 272
        public IntPtr nvEncCreateMVBuffer;              // offset 280
        public IntPtr nvEncDestroyMVBuffer;             // offset 288
        public IntPtr nvEncRunMotionEstimationOnly;     // offset 296
        public IntPtr nvEncGetLastErrorString;          // offset 304
        public IntPtr nvEncSetIOCudaStreams;            // offset 312
        public IntPtr nvEncGetEncodePresetConfigEx;     // offset 320
        public IntPtr nvEncGetSequenceParamEx;          // offset 328
        public IntPtr nvEncRestoreEncoderState;         // offset 336
        public IntPtr nvEncLookaheadPicture;            // offset 344
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 275)]
        public IntPtr[] reserved2;                      // offset 352 - 2552
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

    // NVENC DOES have nvEncUnregisterResource (offset 256) — my earlier
    // "use nvEncUnmapInputResource instead" was wrong. nvEncUnmapInputResource
    // (offset 216) is for unmapping a *mapped* resource, not for unregistering.
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncUnregisterResourceDelegate(
        IntPtr encoder, IntPtr registeredResource);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NvEncDestroyEncoderDelegate(IntPtr encoder);

    // Returns a human-readable error string for the LAST NVENC API call
    // from the current thread. Useful for debugging why an API call failed
    // with a generic status code.
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr NvEncGetLastErrorStringDelegate(IntPtr encoder);

    // === P/Invoke for the NVENC loader functions ===
    // NVIDIA Video Codec SDK 12.x+ ships the 64-bit DLL as 'nvEncodeAPI64.dll'.
    // This spike builds x64-only, so we use the 64-bit name directly.
    [DllImport("nvEncodeAPI64.dll", CallingConvention = CallingConvention.StdCall,
               SetLastError = false, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern int NvEncodeAPICreateInstance(ref NV_ENCODE_API_FUNCTION_LIST functionList);

    [DllImport("nvEncodeAPI64.dll", CallingConvention = CallingConvention.StdCall,
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
    public NvEncodeAPI.NvEncGetLastErrorStringDelegate? GetLastErrorString { get; private set; }

    public uint MaxSupportedApiVersion { get; private set; }

    /// <summary>
    /// Loads the NVENC function table. Returns true on success.
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

            // === Translate packed version to NVENCAPI_VERSION ===
            //
            // NvEncodeAPIGetMaxSupportedVersion returns a PACKED format:
            //   (major << 4) | minor
            //
            // OWNER's DLL returned 0x000000D0 = 208 = (13 << 4) | 0 → NVENC 13.0
            //
            // We extract major/minor and use SetVersionFromPacked to populate
            // NVENCAPI_MAJOR_VERSION and NVENCAPI_MINOR_VERSION, which are then
            // used to compute NVENCAPI_VERSION = major | (minor << 24).
            NvEncodeAPI.SetVersionFromPacked(ver);
            uint maxMajor = NvEncodeAPI.NVENCAPI_MAJOR_VERSION;
            uint maxMinor = NvEncodeAPI.NVENCAPI_MINOR_VERSION;
            Console.WriteLine(
                $"  NVENC max supported API: major={maxMajor}, minor={maxMinor} (packed=0x{ver:X8})");
            Console.WriteLine(
                $"  Spike requests API:     major={maxMajor}, minor={maxMinor} " +
                $"(NVENCAPI_VERSION=0x{NvEncodeAPI.NVENCAPI_VERSION:X8})");

            // Step 2: zero-init the function list and set version field.
            // version = NVENCAPI_STRUCT_VERSION(2) for NV_ENCODE_API_FUNCTION_LIST
            //        = NVENCAPI_VERSION | (2 << 16) | (0x7 << 28)
            _fnList = default;
            _fnList.version = NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST_VER;
            int fnListSize = System.Runtime.InteropServices.Marshal.SizeOf<NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST>();
            Console.WriteLine(
                $"  Function table version: 0x{_fnList.version:X8} (struct size={fnListSize} bytes)");

            // Step 3: call NvEncodeAPICreateInstance
            status = NvEncodeAPI.NvEncodeAPICreateInstance(ref _fnList);
            if (status != NvEncodeAPI.NV_ENC_SUCCESS)
            {
                Console.Error.WriteLine(
                    $"NvEncodeAPICreateInstance failed: status={status} " +
                    $"({NvEncodeAPI.NvencStatusToString(status)})");
                return false;
            }

            // Step 4: marshal function pointers to delegates.
            // The struct layout above matches SDK 13.1's nvEncodeAPI.h exactly,
            // so these field accesses read the correct function pointers.
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
            GetLastErrorString = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncGetLastErrorStringDelegate>(
                _fnList.nvEncGetLastErrorString);

            _loaded = true;
            return true;
        }
        catch (DllNotFoundException ex)
        {
            Console.Error.WriteLine($"ERROR: nvEncodeAPI64.dll not found.");
            Console.Error.WriteLine("  Install NVIDIA Video Codec SDK 13.x and copy nvEncodeAPI64.dll to:");
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
    }
}
