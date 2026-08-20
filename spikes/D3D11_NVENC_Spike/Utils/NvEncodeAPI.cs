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
    public const uint NV_ENC_SUCCESS = 0;
    public const uint NV_ENC_ERR_NO_ENCODE_DEVICE = 1;
    public const uint NV_ENC_ERR_UNSUPPORTED_DEVICE = 2;
    public const uint NV_ENC_ERR_INVALID_ENCODERDEVICE = 3;
    public const uint NV_ENC_ERR_INVALID_DEVICE = 4;
    public const uint NV_ENC_ERR_DEVICE_NOT_EXIST = 5;
    public const uint NV_ENC_ERR_INVALID_PTR = 6;
    public const uint NV_ENC_ERR_INVALID_EVENT = 7;
    public const uint NV_ENC_ERR_INVALID_PARAM = 8;
    public const uint NV_ENC_ERR_INVALID_CALL = 9;
    public const uint NV_ENC_ERR_OUT_OF_MEMORY = 10;
    public const uint NV_ENC_ERR_ENCODER_NOT_INITIALIZED = 11;
    public const uint NV_ENC_ERR_UNSUPPORTED_PARAM = 12;
    public const uint NV_ENC_ERR_LOCK_BUSY = 13;
    // CRITICAL FIX (glm4-phase6-version-macro-fix):
    //   Our code previously had these codes OFF BY ONE starting at position 14,
    //   with wrong names like "NOT_ENOUGH_INTRA_REFRESH_CARDS" and "GENERIC"
    //   placed at slots that actually hold "NOT_ENOUGH_INPUT_DATA" and
    //   "INVALID_VERSION" in NVIDIA's real enum. Status 20 was mislabeled as
    //   RESOURCE_NOT_MAPPED but is actually RESOURCE_NOT_REGISTERED.
    //
    //   Correct enum from NVIDIA nvEncodeAPI.h (SDK 11.1+):
    //     14 = NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA
    //     15 = NV_ENC_ERR_INVALID_VERSION
    //     16 = NV_ENC_ERR_MAP_FAILED
    //     17 = NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY
    //     18 = NV_ENC_ERR_UNIMPLEMENTED
    //     19 = NV_ENC_ERR_RESOURCE_REGISTER_FAILED
    //     20 = NV_ENC_ERR_RESOURCE_NOT_REGISTERED  ← previously mislabeled as NOT_MAPPED
    //     21 = NV_ENC_ERR_RESOURCE_NOT_MAPPED
    //     22 = NV_ENC_ERR_DEVICE_NOT_EXIST (alias)
    public const uint NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA = 14;
    public const uint NV_ENC_ERR_INVALID_VERSION = 15;
    public const uint NV_ENC_ERR_MAP_FAILED = 16;
    public const uint NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY = 17;
    public const uint NV_ENC_ERR_UNIMPLEMENTED = 18;
    public const uint NV_ENC_ERR_RESOURCE_REGISTER_FAILED = 19;
    public const uint NV_ENC_ERR_RESOURCE_NOT_REGISTERED = 20;
    public const uint NV_ENC_ERR_RESOURCE_NOT_MAPPED = 21;
    // Old GENERIC slot was 15 in the previous (wrong) mapping. NVIDIA's actual
    // enum has GENERIC much later (~30+). For backward compatibility with any
    // legacy code still referencing it, leave an alias but mark deprecated.
    [Obsolete("Use specific NV_ENC_ERR_* codes. NVIDIA's actual enum has GENERIC later; status 15 is INVALID_VERSION.")]
    public const uint NV_ENC_ERR_GENERIC = 99;

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
    public const uint NV_ENC_DEVICE_DIRECTX = 0x00;  // was 0x01 (wrong — that's CUDA)
    public const uint NV_ENC_DEVICE_CUDA = 0x01;     // was 0x02

    // === Resource types ===
    public const uint NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX = 0x00;
    public const uint NV_ENC_INPUT_RESOURCE_TYPE_CUDADEVICEPTR = 0x01;

    // === Buffer formats (from nvEncodeAPI.h SDK 13.1) ===
    // Note: ARGB and ABGR values differ from what I previously had —
    // ARGB is 0x01000000 (not 0x00100000), ABGR is 0x10000000 (not 0x00800000).
    public const uint NV_ENC_BUFFER_FORMAT_UNDEFINED = 0x00000000;
    public const uint NV_ENC_BUFFER_FORMAT_NV12 = 0x00000001;
    public const uint NV_ENC_BUFFER_FORMAT_YV12 = 0x00000010;
    public const uint NV_ENC_BUFFER_FORMAT_IYUV = 0x00000100;
    public const uint NV_ENC_BUFFER_FORMAT_YUV444 = 0x00001000;
    public const uint NV_ENC_BUFFER_FORMAT_YUV420_10BIT = 0x00010000;
    public const uint NV_ENC_BUFFER_FORMAT_YUV444_10BIT = 0x00100000;
    public const uint NV_ENC_BUFFER_FORMAT_ARGB = 0x01000000;     // was 0x00100000 (wrong)
    public const uint NV_ENC_BUFFER_FORMAT_ARGB10 = 0x02000000;   // was 0x00200000 (wrong)
    public const uint NV_ENC_BUFFER_FORMAT_AYUV = 0x04000000;     // was 0x00400000 (wrong)
    public const uint NV_ENC_BUFFER_FORMAT_ABGR = 0x10000000;     // was 0x00800000 (wrong)
    public const uint NV_ENC_BUFFER_FORMAT_ABGR10 = 0x20000000;   // was 0x01000000 (wrong)

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

    // === Preset GUIDs (from nvEncodeAPI.h SDK 11) ===
    public static readonly Guid NV_ENC_PRESET_DEFAULT_GUID =
        new(0xb2dfb705, 0x4ebd, 0x4c49, 0x9b, 0x5f, 0x24, 0xa7, 0x77, 0xd3, 0xe5, 0x87);
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
    /// Computes the struct version field per NVENC's convention.
    ///
    /// CRITICAL EMPIRICAL FINDING (glm4-phase6-version-macro-fix-revert):
    ///   I previously thought NVIDIA's macro was:
    ///     NVENCAPI_STRUCT_VERSION(ver) = ver | (NVENCAPI_VERSION << 16) | (0x7 << 28)
    ///   But OWNER's nvEncodeAPI64.dll EMPIRICALLY REJECTS this form —
    ///   NvEncOpenEncodeSessionEx returns NV_ENC_ERR_INVALID_VERSION (status 15)
    ///   when called with version field = 0x700D0001 (the documented form).
    ///
    ///   OWNER's DLL ACCEPTS the SWAPPED form:
    ///     NVENCAPI_VERSION | (ver << 16) | (0x7 << 28)
    ///   producing version field = 0x7001000D for NVENCAPI_VERSION=0x0D, ver=1.
    ///
    ///   This was confirmed by the Phase 4 prior PASS:
    ///     "Function table version: 0x7001000D" → OpenEncodeSession PASS
    ///   vs. Phase 6 after the macro swap:
    ///     "Function table version: 0x700D0001" → OpenEncodeSession FAIL
    ///
    ///   Possible explanations:
    ///     1. OWNER's DLL is built against an older SDK (pre-13.1) where the
    ///        macro had NVENCAPI_VERSION in the low bits.
    ///     2. The public SDK 13.1 header documentation is wrong.
    ///     3. The DLL's FileDescription "Version 11.0" indicates SDK 11.0 was
    ///        compiled with a different macro form than SDK 13.1 docs show.
    ///
    ///   Empirically, the SWAPPED form is what this specific DLL expects.
    ///   REVERTING to the swapped form.
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

    // === Struct version constants ===
    //
    // OWNER's nvEncodeAPI64.dll has FileDescription "NVIDIA Video Encoder
    // API, Version 11.0" — meaning the DLL is built against SDK 11 layout,
    // even though the driver reports max supported API = 13.0.
    //
    // SDK 11 struct versions differ from SDK 13:
    //   SDK 11:  NV_ENCODE_API_FUNCTION_LIST_VER = NVENCAPI_STRUCT_VERSION(1)
    //            NV_ENC_REGISTER_RESOURCE_VER    = NVENCAPI_STRUCT_VERSION(3)
    //   SDK 13:  NV_ENCODE_API_FUNCTION_LIST_VER = NVENCAPI_STRUCT_VERSION(2)
    //            NV_ENC_REGISTER_RESOURCE_VER    = NVENCAPI_STRUCT_VERSION(5)
    //
    // OpenEncodeSessionExParams is ver=1 in both SDKs.
    //
    // We use SDK 11 versions because that's what the DLL expects.
    public const uint NV_ENCODE_API_FUNCTION_LIST_VER_STRUCT = 1;       // SDK 11
    public const uint NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER_STRUCT = 1;
    public const uint NV_ENC_REGISTER_RESOURCE_VER_STRUCT = 3;          // SDK 11

    // Phase 6 struct version constants (SDK 11)
    public const uint NV_ENC_CREATE_BITSTREAM_BUFFER_VER_STRUCT = 1;    // SDK 11
    public const uint NV_ENC_MAP_INPUT_RESOURCE_VER_STRUCT = 4;         // SDK 11
    public const uint NV_ENC_PIC_PARAMS_VER_STRUCT = 1;                // SDK 11
    public const uint NV_ENC_LOCK_BITSTREAM_VER_STRUCT = 1;             // SDK 11

    public static uint NV_ENC_CREATE_BITSTREAM_BUFFER_VER =>
        MakeStructVersion(NV_ENC_CREATE_BITSTREAM_BUFFER_VER_STRUCT);
    public static uint NV_ENC_MAP_INPUT_RESOURCE_VER =>
        MakeStructVersion(NV_ENC_MAP_INPUT_RESOURCE_VER_STRUCT);
    public static uint NV_ENC_PIC_PARAMS_VER =>
        MakeStructVersion(NV_ENC_PIC_PARAMS_VER_STRUCT);
    public static uint NV_ENC_LOCK_BITSTREAM_VER =>
        MakeStructVersion(NV_ENC_LOCK_BITSTREAM_VER_STRUCT);

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
        public uint deviceType;             // NV_ENC_DEVICE_TYPE enum
        public IntPtr device;              // void*
        public IntPtr reserved;            // void*
        public uint apiVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 253)]
        public uint[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public IntPtr[] reserved2;
    }

    //
    // NV_ENC_REGISTER_RESOURCE — from nvEncodeAPI.h SDK 11:
    //
    // SDK 11 layout is DIFFERENT from SDK 13:
    //   - No bufferUsage field
    //   - No pInputFencePoint field
    //   - No chromaOffset[2] / chromaOffsetIn[2] fields
    //   - reserved1 size is 248 (not 244)
    //   - reserved2 size is 62 (not 61)
    //
    // typedef struct _NV_ENC_REGISTER_RESOURCE {
    //     uint32_t                    version;             // offset 0
    //     NV_ENC_INPUT_RESOURCE_TYPE  resourceType;        // offset 4
    //     uint32_t                    width;               // offset 8
    //     uint32_t                    height;              // offset 12
    //     uint32_t                    pitch;               // offset 16
    //     uint32_t                    subResourceIndex;    // offset 20
    //     void*                       resourceToRegister;  // offset 24
    //     NV_ENC_REGISTERED_PTR       registeredResource;  // offset 32 (void*)
    //     NV_ENC_BUFFER_FORMAT        bufferFormat;        // offset 40 (enum = int32)
    //     uint32_t                    reserved1[248];      // offset 44 - 1035
    //     void*                       reserved2[62];      // offset 1036 - 1532
    // }
    // Total size: 1532 bytes
    //
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_REGISTER_RESOURCE
    {
        public uint version;
        public uint resourceType;           // NV_ENC_INPUT_RESOURCE_TYPE enum
        public uint width;
        public uint height;
        public uint pitch;
        public uint subResourceIndex;      // 0 for non-array textures
        public IntPtr resourceToRegister;  // void*
        public IntPtr registeredResource;  // void* (OUT)
        public uint bufferFormat;           // NV_ENC_BUFFER_FORMAT enum
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 248)]
        public uint[] reserved1;           // SDK 11: 248 (was 244 in SDK 13)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 62)]
        public IntPtr[] reserved2;          // SDK 11: 62 (was 61 in SDK 13)
    }

    // ════════════════════════════════════════════════════════════════
    // Phase 6 structs — SDK 11 layouts
    // ════════════════════════════════════════════════════════════════

    // NV_ENC_CREATE_BITSTREAM_BUFFER — from nvEncodeAPI.h SDK 11:
    //   typedef struct _NV_ENC_CREATE_BITSTREAM_BUFFER {
    //       uint32_t  version;            // offset 0
    //       uint32_t  size;               // offset 4 (0 = default)
    //       uint32_t  memoryHeap;         // offset 8 (reserved, set to 0)
    //       void*     bitstreamBuffer;    // offset 16 (8-byte aligned, OUT)
    //       void*     reserved1;          // offset 24
    //       void*     reserved2;          // offset 32
    //       uint32_t  reserved3[226];     // offset 40
    //       void*     reserved4[64];      // offset 944
    //   }
    // Total size: ~1456 bytes
    //
    // NOTE: The exact reserved array sizes must match SDK 11's header.
    // SDK 11 uses reserved3[226] + reserved4[64] per the NVIDIA sample code.
    // If this struct size is wrong, NvEncCreateBitstreamBuffer will fail
    // with NV_ENC_ERR_INVALID_PARAM.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_CREATE_BITSTREAM_BUFFER
    {
        public uint version;
        public uint size;
        public uint memoryHeap;
        public uint _padding;              // explicit 4-byte pad for 8-byte alignment of bitstreamBuffer
        public IntPtr bitstreamBuffer;    // OUT — handle to bitstream buffer
        public IntPtr reserved1;
        public IntPtr reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 226)]
        public uint[] reserved3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public IntPtr[] reserved4;
    }

    // NV_ENC_MAP_INPUT_RESOURCE — from nvEncodeAPI.h SDK 11:
    //
    // typedef struct _NV_ENC_MAP_INPUT_RESOURCE {
    //     uint32_t              version;             // offset 0
    //     uint32_t              subResourceIndex;   // offset 4
    //     NV_ENC_REGISTERED_PTR inputResource;      // offset 8  (void*, 8 bytes)
    //     NV_ENC_INPUT_PTR      mappedInputResource; // offset 16 (void*, 8 bytes, OUT)
    //     void*                 reserved1[246];     // offset 24 (void*[246], 1968 bytes)
    //     void*                 reserved2[63];       // offset 1992 (void*[63], 504 bytes)
    // }
    // Total: 24 + 1968 + 504 = 2496 bytes
    //
    // CRITICAL FIX (glm4-phase6-map-struct-fix):
    //   Previous version had 3 bugs:
    //     1. _padding field between inputResource and mappedInputResource — UNNECESSARY,
    //        because inputResource@8 ends at offset 16 which is already 8-byte aligned.
    //        The _padding pushed mappedInputResource to offset 20, but NVENC writes its
    //        output to offset 16 → C# would read garbage even if MapInputResource succeeded.
    //     2. reserved1 declared as uint[246] (4 bytes/elem, 984 bytes total) — WRONG.
    //        NVIDIA SDK 11 nvEncodeAPI.h declares it as void*[246] (8 bytes/elem, 1968 bytes).
    //     3. reserved2 declared as IntPtr[59] (472 bytes) — WRONG size.
    //        NVIDIA SDK 11 declares it as void*[63] (504 bytes).
    //   Previous size: 1484 bytes (Marshal.SizeOf reported 1484 in run output).
    //   NVIDIA expected: 2496 bytes — discrepancy of 1012 bytes.
    //   This caused NvEncMapInputResource to return NV_ENC_ERR_RESOURCE_NOT_MAPPED (20).
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_MAP_INPUT_RESOURCE
    {
        public uint version;                // offset 0
        public uint subResourceIndex;        // offset 4
        public IntPtr inputResource;         // offset 8  — NV_ENC_REGISTERED_PTR (from RegisterResource)
        public IntPtr mappedInputResource;   // offset 16 — OUT, NV_ENC_INPUT_PTR (for EncodePicture)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 246)]
        public IntPtr[] reserved1;           // offset 24 — void*[246]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public IntPtr[] reserved2;           // offset 1992 — void*[63]
    }

    // NV_ENC_PIC_PARAMS — from nvEncodeAPI.h SDK 11:
    //
    // This is the most complex struct. In SDK 11 it contains:
    //   version, inputWidth, inputHeight, inputPitch, encodePicFlags,
    //   frameIdx, inputTimeStamp, inputDuration, codecPicHints (struct with bitfields),
    //   outputBitstream (void*), inputBuffer (void* — NV_ENC_INPUT_PTR),
    //   bufferFmt (enum), picParamsRC, reserved1[244], reserved2[63]
    //
    // The codecPicHints struct uses C bitfields which don't map cleanly to C#.
    // We use a single uint32 to hold the bitfield area (same approach as
    // NV_ENC_INITIALIZE_PARAMS.bitFields).
    //
    // Layout (SDK 11, x64, Pack=1):
    //   version               offset 0    (uint32, 4 bytes)
    //   inputWidth             offset 4    (uint32, 4 bytes)
    //   inputHeight            offset 8    (uint32, 4 bytes)
    //   inputPitch             offset 12  (uint32, 4 bytes)
    //   encodePicFlags         offset 16  (uint32, 4 bytes — 0 = normal encode)
    //   frameIdx               offset 20  (uint32, 4 bytes)
    //   inputTimeStamp         offset 24  (uint64, 8 bytes)
    //   inputDuration          offset 32  (uint64, 8 bytes)
    //   codecPicHints_bitfield offset 40  (uint32, 4 bytes — C bitfields packed)
    //   _padding1              offset 44  (uint32, explicit pad for 8-byte alignment)
    //   outputBitstream        offset 48  (void*, 8 bytes — NV_ENC_OUTPUT_PTR)
    //   inputBuffer            offset 56  (void*, 8 bytes — NV_ENC_INPUT_PTR)
    //   bufferFmt              offset 64  (int32, enum NV_ENC_BUFFER_FORMAT)
    //   picStruct              offset 68  (int32, enum)
    //   picType                offset 72  (uint32 — OUT)
    //   _padding2              offset 76  (uint32, explicit pad)
    //   reserved1[244]         offset 80  (uint32[244])
    //   reserved2[63]          offset 1056 (void*[63])
    // Total: 80 + 244*4 + 63*8 = 80 + 976 + 504 = 1560 bytes
    //
    // IMPORTANT: The exact size must match what NVENC expects for SDK 11.
    // If size is wrong, NvEncEncodePicture returns NV_ENC_ERR_INVALID_PARAM.
    // The reserved array sizes (244, 63) are derived from the SDK 11 header
    // by subtracting the known field sizes from the total struct size.
    // These MUST be verified at runtime by checking Marshal.SizeOf matches
    // what NVENC expects (if it returns NV_ENC_ERR_INVALID_PARAM, the size
    // is wrong).
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_PIC_PARAMS
    {
        public uint version;
        public uint inputWidth;
        public uint inputHeight;
        public uint inputPitch;
        public uint encodePicFlags;
        public uint frameIdx;
        public ulong inputTimeStamp;
        public ulong inputDuration;
        public uint codecPicHints_bitfield;
        public uint _padding1;
        public IntPtr outputBitstream;       // NV_ENC_OUTPUT_PTR (bitstream buffer handle)
        public IntPtr inputBuffer;           // NV_ENC_INPUT_PTR (mapped input resource)
        public uint bufferFmt;               // NV_ENC_BUFFER_FORMAT enum
        public uint picStruct;               // NV_ENC_PIC_STRUCT enum
        public uint picType;                // OUT
        public uint _padding2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 244)]
        public uint[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public IntPtr[] reserved2;
    }

    // NV_ENC_LOCK_BITSTREAM — from nvEncodeAPI.h SDK 11:
    //
    // Layout (SDK 11, x64, Pack=1):
    //   version               offset 0    (uint32)
    //   doNotWait             offset 4    (uint32 — 0 = blocking)
    //   _padding1             offset 8    (uint32 — pad for 8-byte alignment)
    //   ltrFrameIdx            offset 12   (uint32)
    //   reserved1              offset 16   (uint32)
    //   _padding2              offset 20   (uint32 — pad for 8-byte alignment)
    //   outputBitstream        offset 24   (void* — NV_ENC_OUTPUT_PTR, 8 bytes)
    //   sliceType              offset 32   (uint32 — OUT)
    //   picType                offset 36   (uint32 — OUT)
    //   picIdx                 offset 40   (uint32 — OUT)
    //   _padding3              offset 44   (uint32 — pad)
    //   bitstreamBufferPtr     offset 48   (void* — OUT, pointer to encoded data)
    //   bitstreamSizeInBytes   offset 56   (uint32 — OUT)
    //   _padding4              offset 60   (uint32 — pad)
    //   frameIdx               offset 64   (uint32 — OUT)
    //   inputTimeStamp         offset 72   (uint64 — OUT, echoes input)
    //   inputDuration          offset 80   (uint64 — OUT)
    //   reserved2[221]         offset 88   (uint32[221])
    //   reserved3[63]          offset 972  (void*[63])
    // Total: 88 + 221*4 + 63*8 = 88 + 884 + 504 = 1476 bytes
    //
    // NOTE: The reserved array sizes MUST match SDK 11. If wrong,
    // NvEncLockBitstream returns NV_ENC_ERR_INVALID_PARAM.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_LOCK_BITSTREAM
    {
        public uint version;
        public uint doNotWait;
        public uint ltrFrameIdx;
        public uint reserved1;
        public IntPtr outputBitstream;       // NV_ENC_OUTPUT_PTR (bitstream buffer handle)
        public uint sliceType;               // OUT
        public uint picType;                 // OUT
        public uint picIdx;                 // OUT
        public uint _padding;
        public IntPtr bitstreamBufferPtr;    // OUT — pointer to encoded H.264 data
        public uint bitstreamSizeInBytes;    // OUT — size of encoded data
        public uint _padding2;
        public uint frameIdx;               // OUT
        public uint _padding3;
        public ulong inputTimeStamp;         // OUT — echoes EncodePicture timestamp
        public ulong inputDuration;          // OUT
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 221)]
        public uint[] reserved2;
        public uint _padding4;              // explicit pad for 8-byte alignment of reserved3
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public IntPtr[] reserved3;
    }

    // ════════════════════════════════════════════════════════════════
    // Phase 6 delegates
    // ════════════════════════════════════════════════════════════════

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncCreateBitstreamBufferDelegate(
        IntPtr encoder, ref NV_ENC_CREATE_BITSTREAM_BUFFER createParams);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncDestroyBitstreamBufferDelegate(
        IntPtr encoder, IntPtr bitstreamBuffer);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncMapInputResourceDelegate(
        IntPtr encoder, ref NV_ENC_MAP_INPUT_RESOURCE mapParams);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncUnmapInputResourceDelegate(
        IntPtr encoder, IntPtr mappedInputResource);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncEncodePictureDelegate(
        IntPtr encoder, ref NV_ENC_PIC_PARAMS picParams);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncLockBitstreamDelegate(
        IntPtr encoder, ref NV_ENC_LOCK_BITSTREAM lockParams);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncUnlockBitstreamDelegate(
        IntPtr encoder, IntPtr bitstreamBuffer);
    //
    // CRITICAL: This struct previously used LayoutKind.Explicit with
    // [MarshalAs] arrays, which causes TypeLoadException in .NET because
    // [MarshalAs(UnmanagedType.ByValArray)] is NOT supported inside
    // LayoutKind.Explicit structs.
    //
    // Fix: Use LayoutKind.Sequential with Pack=1 + explicit padding fields
    // for 8-byte alignment of pointer fields (privData, encodeConfig,
    // reserved2). The NVENC header has NO pragma pack, so the C compiler
    // uses default x64 alignment (8 bytes for pointers).
    //
    // Field offsets (x64, matching C default alignment):
    //   version               offset 0     (uint32, 4 bytes)
    //   encodeGUID             offset 4     (GUID, 16 bytes)
    //   presetGUID             offset 20    (GUID, 16 bytes)
    //   encodeWidth            offset 36    (uint32, 4 bytes)
    //   encodeHeight           offset 40    (uint32, 4 bytes)
    //   darWidth               offset 44    (uint32, 4 bytes)
    //   darHeight              offset 48    (uint32, 4 bytes)
    //   frameRateNum           offset 52    (uint32, 4 bytes)
    //   frameRateDen           offset 56    (uint32, 4 bytes)
    //   enableEncodeAsync      offset 60    (uint32, 4 bytes)
    //   enablePTD              offset 64    (uint32, 4 bytes)
    //   bitFields              offset 68    (uint32, 4 bytes — 5 bit-fields + 27 reserved)
    //   privDataSize           offset 72    (uint32, 4 bytes)
    //   _padding1             offset 76    (uint32, 4 bytes — explicit padding for 8-byte alignment)
    //   privData               offset 80    (void*, 8 bytes — 8-byte aligned ✓)
    //   encodeConfig           offset 88    (NV_ENC_CONFIG*, 8 bytes — 8-byte aligned ✓)
    //   maxEncodeWidth         offset 96    (uint32, 4 bytes)
    //   maxEncodeHeight        offset 100   (uint32, 4 bytes)
    //   maxMEHintCountsPerBlockL0  offset 104  (uint32, 4 bytes)
    //   maxMEHintCountsPerBlockL1  offset 108  (uint32, 4 bytes)
    //   reserved[289]          offset 112   (uint32[289], 1156 bytes, ends at 1268)
    //   _padding2             offset 1268  (uint32, 4 bytes — explicit padding for 8-byte alignment)
    //   reserved2[64]          offset 1272  (void*[64], 512 bytes, ends at 1784)
    // Total size: 1784 bytes
    //
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENC_INITIALIZE_PARAMS
    {
        public uint version;
        public Guid encodeGUID;
        public Guid presetGUID;
        public uint encodeWidth;
        public uint encodeHeight;
        public uint darWidth;
        public uint darHeight;
        public uint frameRateNum;
        public uint frameRateDen;
        public uint enableEncodeAsync;
        public uint enablePTD;
        public uint bitFields;
        public uint privDataSize;
        public uint _padding1;           // explicit 4-byte padding for 8-byte alignment of privData
        public IntPtr privData;          // void* — offset 80 (8-byte aligned)
        public IntPtr encodeConfig;      // NV_ENC_CONFIG* — offset 88 (8-byte aligned)
        public uint maxEncodeWidth;
        public uint maxEncodeHeight;
        public uint maxMEHintCountsPerBlockL0;
        public uint maxMEHintCountsPerBlockL1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 289)]
        public uint[] reserved;
        public uint _padding2;           // explicit 4-byte padding for 8-byte alignment of reserved2
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public IntPtr[] reserved2;
    }

    //
    // NV_ENCODE_API_FUNCTION_LIST — from nvEncodeAPI.h SDK 11:
    //
    // SDK 11 has 38 function pointers + reserved2[281].
    // SDK 13 has 43 function pointers (adds nvEncGetLastErrorString,
    // nvEncSetIOCudaStreams, nvEncGetEncodePresetConfigEx,
    // nvEncGetSequenceParamEx, nvEncRestoreEncoderState,
    // nvEncLookaheadPicture) + reserved2[275].
    //
    // OWNER's DLL is SDK 11 — using the SDK 13 layout caused function
    // pointer offset mismatches: nvEncRegisterResource was at offset 248
    // in our struct but NVENC wrote it at offset 248 in SDK 11 layout,
    // so we read a different function pointer (returns Success/no error).
    //
    // SDK 11 field order (after version+reserved):
    //   1. nvEncOpenEncodeSession
    //   2. nvEncGetEncodeGUIDCount
    //   3. nvEncGetEncodeProfileGUIDCount
    //   4. nvEncGetEncodeProfileGUIDs
    //   5. nvEncGetEncodeGUIDs
    //   6. nvEncGetInputFormatCount
    //   7. nvEncGetInputFormats
    //   8. nvEncGetEncodeCaps
    //   9. nvEncGetEncodePresetCount
    //  10. nvEncGetEncodePresetGUIDs
    //  11. nvEncGetEncodePresetConfig
    //  12. nvEncInitializeEncoder
    //  13. nvEncCreateInputBuffer
    //  14. nvEncDestroyInputBuffer
    //  15. nvEncCreateBitstreamBuffer
    //  16. nvEncDestroyBitstreamBuffer
    //  17. nvEncEncodePicture
    //  18. nvEncLockBitstream
    //  19. nvEncUnlockBitstream
    //  20. nvEncLockInputBuffer
    //  21. nvEncUnlockInputBuffer
    //  22. nvEncGetEncodeStats
    //  23. nvEncGetSequenceParams
    //  24. nvEncRegisterAsyncEvent
    //  25. nvEncUnregisterAsyncEvent
    //  26. nvEncMapInputResource
    //  27. nvEncUnmapInputResource
    //  28. nvEncDestroyEncoder
    //  29. nvEncInvalidateRefFrames
    //  30. nvEncOpenEncodeSessionEx
    //  31. nvEncRegisterResource
    //  32. nvEncUnregisterResource
    //  33. nvEncReconfigureEncoder
    //  34. reserved1 (void*)
    //  35. nvEncCreateMVBuffer
    //  36. nvEncDestroyMVBuffer
    //  37. nvEncRunMotionEstimationOnly
    //  38. reserved2[281]
    //
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NV_ENCODE_API_FUNCTION_LIST
    {
        public uint version;                            // offset 0
        public uint reserved;                           // offset 4
        public IntPtr nvEncOpenEncodeSession;           // offset 8
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
        public IntPtr nvEncOpenEncodeSessionEx;         // offset 240  ★ Ex
        public IntPtr nvEncRegisterResource;            // offset 248  ★ target
        public IntPtr nvEncUnregisterResource;          // offset 256
        public IntPtr nvEncReconfigureEncoder;          // offset 264
        public IntPtr reserved1;                        // offset 272
        public IntPtr nvEncCreateMVBuffer;              // offset 280
        public IntPtr nvEncDestroyMVBuffer;             // offset 288
        public IntPtr nvEncRunMotionEstimationOnly;     // offset 296
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 281)]
        public IntPtr[] reserved2;                      // offset 304 - 2544   SDK 11: 281
    }
    // Total size: 8 + 37*8 + 281*8 = 8 + 296 + 2248 = 2552 bytes

    // === Delegates for function table entries we use ===
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncOpenEncodeSessionExDelegate(
        ref NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS sessionParams,
        out IntPtr encoder);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncGetEncodeGUIDCountDelegate(
        IntPtr encoder, out int count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncGetEncodeGUIDsDelegate(
        IntPtr encoder, [Out] Guid[] guidArray, int arraySize, out int actualCount);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncGetInputFormatCountDelegate(
        IntPtr encoder, Guid encodeGUID, out int count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncGetInputFormatsDelegate(
        IntPtr encoder, Guid encodeGUID, [Out] uint[] formatArray, int arraySize, out int actualCount);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncRegisterResourceDelegate(
        IntPtr encoder, ref NV_ENC_REGISTER_RESOURCE registerParams);

    // NVENC DOES have nvEncUnregisterResource (offset 256) — my earlier
    // "use nvEncUnmapInputResource instead" was wrong. nvEncUnmapInputResource
    // (offset 216) is for unmapping a *mapped* resource, not for unregistering.
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncUnregisterResourceDelegate(
        IntPtr encoder, IntPtr registeredResource);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncDestroyEncoderDelegate(IntPtr encoder);

    // NvEncInitializeEncoder — must be called BEFORE NvEncRegisterResource.
    // Configures codec, preset, frame rate, dimensions, etc.
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NvEncInitializeEncoderDelegate(
        IntPtr encoder, ref NV_ENC_INITIALIZE_PARAMS initParams);

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
    public static extern uint NvEncodeAPICreateInstance(ref NV_ENCODE_API_FUNCTION_LIST functionList);

    [DllImport("nvEncodeAPI64.dll", CallingConvention = CallingConvention.StdCall,
               SetLastError = false, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern uint NvEncodeAPIGetMaxSupportedVersion(out uint version);

    public static string NvencStatusToString(uint status)
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
            NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA => "NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA",
            NV_ENC_ERR_INVALID_VERSION => "NV_ENC_ERR_INVALID_VERSION",
            NV_ENC_ERR_MAP_FAILED => "NV_ENC_ERR_MAP_FAILED",
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
    // NOTE: SDK 11 does NOT have nvEncGetLastErrorString in the function table.
    // It was added in SDK 12+. We removed it from the struct, so we can't call it.

    // Phase 6 delegates
    public NvEncodeAPI.NvEncCreateBitstreamBufferDelegate? CreateBitstreamBuffer { get; private set; }
    public NvEncodeAPI.NvEncDestroyBitstreamBufferDelegate? DestroyBitstreamBuffer { get; private set; }
    public NvEncodeAPI.NvEncMapInputResourceDelegate? MapInputResource { get; private set; }
    public NvEncodeAPI.NvEncUnmapInputResourceDelegate? UnmapInputResource { get; private set; }
    public NvEncodeAPI.NvEncEncodePictureDelegate? EncodePicture { get; private set; }
    public NvEncodeAPI.NvEncLockBitstreamDelegate? LockBitstream { get; private set; }
    public NvEncodeAPI.NvEncUnlockBitstreamDelegate? UnlockBitstream { get; private set; }

    public uint MaxSupportedApiVersion { get; private set; }

    /// <summary>
    /// Loads the NVENC function table. Returns true on success.
    /// </summary>
    public bool TryLoad()
    {
        try
        {
            // Step 1: query max supported version
            uint status = NvEncodeAPI.NvEncodeAPIGetMaxSupportedVersion(out uint ver);
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
            int fnListSize = System.Runtime.InteropServices.Marshal.SizeOf<NvEncodeAPI.NV_ENCODE_API_FUNCTION_LIST>(); // Marshal.SizeOf returns int, not uint — this is correct
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
            InitializeEncoder = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncInitializeEncoderDelegate>(
                _fnList.nvEncInitializeEncoder);
            // SDK 11 does NOT have nvEncGetLastErrorString — skip marshalling it.

            // Phase 6: marshal the encode pipeline function pointers
            CreateBitstreamBuffer = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncCreateBitstreamBufferDelegate>(
                _fnList.nvEncCreateBitstreamBuffer);
            DestroyBitstreamBuffer = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncDestroyBitstreamBufferDelegate>(
                _fnList.nvEncDestroyBitstreamBuffer);
            MapInputResource = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncMapInputResourceDelegate>(
                _fnList.nvEncMapInputResource);
            UnmapInputResource = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncUnmapInputResourceDelegate>(
                _fnList.nvEncUnmapInputResource);
            EncodePicture = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncEncodePictureDelegate>(
                _fnList.nvEncEncodePicture);
            LockBitstream = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncLockBitstreamDelegate>(
                _fnList.nvEncLockBitstream);
            UnlockBitstream = Marshal.GetDelegateForFunctionPointer<NvEncodeAPI.NvEncUnlockBitstreamDelegate>(
                _fnList.nvEncUnlockBitstream);

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
