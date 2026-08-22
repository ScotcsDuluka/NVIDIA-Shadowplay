Option Strict On
Option Explicit On
Option Infer On

' NvEncodeAPI.vb
'
' P/Invoke declarations for NVIDIA Video Codec SDK (NVENC).
' VERBATIM translation of spikes/D3D11_NVENC_Spike/Utils/NvEncodeAPI.cs (C# → VB.NET).
' Struct layouts, constants, delegates, and DllImport declarations are byte-identical.
'
' Phase 12: moved from spike namespace (CaptureEngine.Video.Spike.D3D11.Utils)
' to production namespace (CaptureEngine.Encoder.Nvenc.Internal).
'
' SPDX-License-Identifier: MIT

Imports System.Runtime.InteropServices

Namespace CaptureEngine.Encoder.Nvenc.Internal

    Public NotInheritable Class NvEncodeAPI

        Private Sub New()
        End Sub

        ' === NVENC status codes ===
        Public Const NV_ENC_SUCCESS As UInteger = 0UI
        Public Const NV_ENC_ERR_NO_ENCODE_DEVICE As UInteger = 1UI
        Public Const NV_ENC_ERR_UNSUPPORTED_DEVICE As UInteger = 2UI
        Public Const NV_ENC_ERR_INVALID_ENCODERDEVICE As UInteger = 3UI
        Public Const NV_ENC_ERR_INVALID_DEVICE As UInteger = 4UI
        Public Const NV_ENC_ERR_DEVICE_NOT_EXIST As UInteger = 5UI
        Public Const NV_ENC_ERR_INVALID_PTR As UInteger = 6UI
        Public Const NV_ENC_ERR_INVALID_EVENT As UInteger = 7UI
        Public Const NV_ENC_ERR_INVALID_PARAM As UInteger = 8UI
        Public Const NV_ENC_ERR_INVALID_CALL As UInteger = 9UI
        Public Const NV_ENC_ERR_OUT_OF_MEMORY As UInteger = 10UI
        Public Const NV_ENC_ERR_ENCODER_NOT_INITIALIZED As UInteger = 11UI
        Public Const NV_ENC_ERR_UNSUPPORTED_PARAM As UInteger = 12UI
        Public Const NV_ENC_ERR_LOCK_BUSY As UInteger = 13UI
        Public Const NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA As UInteger = 14UI
        Public Const NV_ENC_ERR_INVALID_VERSION As UInteger = 15UI
        Public Const NV_ENC_ERR_MAP_FAILED As UInteger = 16UI
        Public Const NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY As UInteger = 17UI
        Public Const NV_ENC_ERR_UNIMPLEMENTED As UInteger = 18UI
        Public Const NV_ENC_ERR_RESOURCE_REGISTER_FAILED As UInteger = 19UI
        Public Const NV_ENC_ERR_RESOURCE_NOT_REGISTERED As UInteger = 20UI
        Public Const NV_ENC_ERR_RESOURCE_NOT_MAPPED As UInteger = 21UI
        <Obsolete("Use specific NV_ENC_ERR_* codes.")>
        Public Const NV_ENC_ERR_GENERIC As UInteger = 99UI

        ' === Device types ===
        Public Const NV_ENC_DEVICE_DIRECTX As UInteger = &H00UI
        Public Const NV_ENC_DEVICE_CUDA As UInteger = &H01UI

        ' === Resource types ===
        Public Const NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX As UInteger = &H00UI
        Public Const NV_ENC_INPUT_RESOURCE_TYPE_CUDADEVICEPTR As UInteger = &H01UI

        ' === Buffer formats ===
        Public Const NV_ENC_BUFFER_FORMAT_UNDEFINED As UInteger = &H00000000UI
        Public Const NV_ENC_BUFFER_FORMAT_NV12 As UInteger = &H00000001UI
        Public Const NV_ENC_BUFFER_FORMAT_YV12 As UInteger = &H00000010UI
        Public Const NV_ENC_BUFFER_FORMAT_IYUV As UInteger = &H00000100UI
        Public Const NV_ENC_BUFFER_FORMAT_YUV444 As UInteger = &H00001000UI
        Public Const NV_ENC_BUFFER_FORMAT_YUV420_10BIT As UInteger = &H00010000UI
        Public Const NV_ENC_BUFFER_FORMAT_YUV444_10BIT As UInteger = &H00100000UI
        Public Const NV_ENC_BUFFER_FORMAT_ARGB As UInteger = &H01000000UI
        Public Const NV_ENC_BUFFER_FORMAT_ARGB10 As UInteger = &H02000000UI
        Public Const NV_ENC_BUFFER_FORMAT_AYUV As UInteger = &H04000000UI
        Public Const NV_ENC_BUFFER_FORMAT_ABGR As UInteger = &H10000000UI
        Public Const NV_ENC_BUFFER_FORMAT_ABGR10 As UInteger = &H20000000UI

        ' === Encode pic flags (NV_ENC_PIC_FLAG_*) ===
        Public Const NV_ENC_PIC_FLAG_FORCEINTRA As UInteger = &H1UI
        Public Const NV_ENC_PIC_FLAG_FORCEIDR As UInteger = &H2UI
        Public Const NV_ENC_PIC_FLAG_OUTPUT_SPSPPS As UInteger = &H4UI
        Public Const NV_ENC_PIC_FLAG_EOS As UInteger = &H8UI

        ' === Codec GUIDs ===
        Public Shared ReadOnly NV_ENC_CODEC_H264_GUID As Guid =
            New Guid(&H6BC82762UI, &H4E63US, &H4CA4US, &HAA, &H85, &H1E, &H50, &HF3, &H21, &HF6, &HBF)
        Public Shared ReadOnly NV_ENC_CODEC_HEVC_GUID As Guid =
            New Guid(&H790CDC88UI, &H4522US, &H4D7BUS, &H94, &H25, &HBD, &HA9, &H97, &H5F, &H76, &H03)
        Public Shared ReadOnly NV_ENC_PRESET_DEFAULT_GUID As Guid =
            New Guid(&HB2DFB705UI, &H4EBDUS, &H4C49US, &H9B, &H5F, &H24, &HA7, &H77, &HD3, &HE5, &H87)
        Public Shared ReadOnly NV_ENC_CODEC_AV1_GUID As Guid =
            New Guid(&HC24B3F5DUI, &H7354US, &H4CA4US, &H9C, &HA2, &H6A, &H2B, &H55, &H4D, &HB0, &HA8)

        ' === API version ===
        Public Shared Property NVENCAPI_MAJOR_VERSION As UInteger = 13UI
        Public Shared Property NVENCAPI_MINOR_VERSION As UInteger = 0UI

        Public Shared ReadOnly Property NVENCAPI_VERSION As UInteger
            Get
                Return NVENCAPI_MAJOR_VERSION Or (NVENCAPI_MINOR_VERSION << 24)
            End Get
        End Property

        Public Shared Function MakeStructVersion(structVer As UInteger) As UInteger
            Return NVENCAPI_VERSION Or (structVer << 16) Or (&H7UI << 28)
        End Function

        Public Shared Sub SetVersionFromPacked(packed As UInteger)
            NVENCAPI_MAJOR_VERSION = (packed >> 4) And &HFUI
            NVENCAPI_MINOR_VERSION = packed And &HFUI
        End Sub

        ' === Struct version constants ===
        Public Const NV_ENCODE_API_FUNCTION_LIST_VER_STRUCT As UInteger = 1UI
        Public Const NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER_STRUCT As UInteger = 1UI
        Public Const NV_ENC_REGISTER_RESOURCE_VER_STRUCT As UInteger = 3UI
        Public Const NV_ENC_CREATE_BITSTREAM_BUFFER_VER_STRUCT As UInteger = 1UI
        Public Const NV_ENC_MAP_INPUT_RESOURCE_VER_STRUCT As UInteger = 4UI
        Public Const NV_ENC_PIC_PARAMS_VER_STRUCT As UInteger = 4UI
        Public Const NV_ENC_LOCK_BITSTREAM_VER_STRUCT As UInteger = 1UI

        Public Shared ReadOnly Property NV_ENC_CREATE_BITSTREAM_BUFFER_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENC_CREATE_BITSTREAM_BUFFER_VER_STRUCT)
            End Get
        End Property

        Public Shared ReadOnly Property NV_ENC_MAP_INPUT_RESOURCE_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENC_MAP_INPUT_RESOURCE_VER_STRUCT)
            End Get
        End Property

        Public Shared ReadOnly Property NV_ENC_PIC_PARAMS_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENC_PIC_PARAMS_VER_STRUCT) Or (1UI << 31)
            End Get
        End Property

        Public Shared ReadOnly Property NV_ENC_LOCK_BITSTREAM_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENC_LOCK_BITSTREAM_VER_STRUCT)
            End Get
        End Property

        Public Shared ReadOnly Property NV_ENCODE_API_FUNCTION_LIST_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENCODE_API_FUNCTION_LIST_VER_STRUCT)
            End Get
        End Property

        Public Shared ReadOnly Property NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER_STRUCT)
            End Get
        End Property

        Public Shared ReadOnly Property NV_ENC_REGISTER_RESOURCE_VER As UInteger
            Get
                Return MakeStructVersion(NV_ENC_REGISTER_RESOURCE_VER_STRUCT)
            End Get
        End Property

        ' === Structs ===

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
            Public version As UInteger
            Public deviceType As UInteger
            Public device As IntPtr
            Public reserved As IntPtr
            Public apiVersion As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=253)>
            Public reserved1 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
            Public reserved2 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_REGISTER_RESOURCE
            Public version As UInteger
            Public resourceType As UInteger
            Public width As UInteger
            Public height As UInteger
            Public pitch As UInteger
            Public subResourceIndex As UInteger
            Public resourceToRegister As IntPtr
            Public registeredResource As IntPtr
            Public bufferFormat As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=248)>
            Public reserved1 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=62)>
            Public reserved2 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_CREATE_BITSTREAM_BUFFER
            Public version As UInteger
            Public size As UInteger
            Public memoryHeap As UInteger
            Public _padding As UInteger
            Public bitstreamBuffer As IntPtr
            Public reserved1 As IntPtr
            Public reserved2 As IntPtr
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=226)>
            Public reserved3 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
            Public reserved4 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_MAP_INPUT_RESOURCE
            Public version As UInteger
            Public subResourceIndex As UInteger
            Public inputResource As IntPtr
            Public registeredResource As IntPtr
            Public mappedResource As IntPtr
            Public mappedBufferFmt As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=251)>
            Public reserved1 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=63)>
            Public reserved2 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_PIC_PARAMS
            Public version As UInteger
            Public inputWidth As UInteger
            Public inputHeight As UInteger
            Public inputPitch As UInteger
            Public encodePicFlags As UInteger
            Public frameIdx As UInteger
            Public inputTimeStamp As ULong
            Public inputDuration As ULong
            Public inputBuffer As IntPtr
            Public outputBitstream As IntPtr
            Public completionEvent As IntPtr
            Public bufferFmt As UInteger
            Public pictureStruct As UInteger
            Public pictureType As UInteger
            Public _padding1 As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=1536)>
            Public codecPicParams As Byte()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)>
            Public meHintCountsPerBlock As Byte()
            Public meExternalHints As IntPtr
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=6)>
            Public reserved1 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)>
            Public reserved2 As IntPtr()
            Public qpDeltaMap As IntPtr
            Public qpDeltaMapSize As UInteger
            Public reservedBitFields As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)>
            Public meHintRefPicDist As UShort()
            Public _padding2 As UInteger
            Public alphaBuffer As IntPtr
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=286)>
            Public reserved3 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=59)>
            Public reserved4 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_LOCK_BITSTREAM
            Public version As UInteger
            Public bitfields As UInteger
            Public outputBitstream As IntPtr
            Public sliceOffsets As IntPtr
            Public frameIdx As UInteger
            Public hwEncodeStatus As UInteger
            Public numSlices As UInteger
            Public bitstreamSizeInBytes As UInteger
            Public outputTimeStamp As ULong
            Public outputDuration As ULong
            Public bitstreamBufferPtr As IntPtr
            Public pictureType As UInteger
            Public pictureStruct As UInteger
            Public frameAvgQP As UInteger
            Public frameSatd As UInteger
            Public ltrFrameIdx As UInteger
            Public ltrFrameBitmap As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=13)>
            Public reserved As UInteger()
            Public intraMBCount As UInteger
            Public interMBCount As UInteger
            Public averageMVX As Integer
            Public averageMVY As Integer
            Public alphaLayerSizeInBytes As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=218)>
            Public reserved1 As UInteger()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
            Public reserved2 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENC_INITIALIZE_PARAMS
            Public version As UInteger
            Public encodeGUID As Guid
            Public presetGUID As Guid
            Public encodeWidth As UInteger
            Public encodeHeight As UInteger
            Public darWidth As UInteger
            Public darHeight As UInteger
            Public frameRateNum As UInteger
            Public frameRateDen As UInteger
            Public enableEncodeAsync As UInteger
            Public enablePTD As UInteger
            Public bitFields As UInteger
            Public privDataSize As UInteger
            Public _padding1 As UInteger
            Public privData As IntPtr
            Public encodeConfig As IntPtr
            Public maxEncodeWidth As UInteger
            Public maxEncodeHeight As UInteger
            Public maxMEHintCountsPerBlockL0 As UInteger
            Public maxMEHintCountsPerBlockL1 As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=289)>
            Public reserved As UInteger()
            Public _padding2 As UInteger
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
            Public reserved2 As IntPtr()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure NV_ENCODE_API_FUNCTION_LIST
            Public version As UInteger
            Public reserved As UInteger
            Public nvEncOpenEncodeSession As IntPtr
            Public nvEncGetEncodeGUIDCount As IntPtr
            Public nvEncGetEncodeProfileGUIDCount As IntPtr
            Public nvEncGetEncodeProfileGUIDs As IntPtr
            Public nvEncGetEncodeGUIDs As IntPtr
            Public nvEncGetInputFormatCount As IntPtr
            Public nvEncGetInputFormats As IntPtr
            Public nvEncGetEncodeCaps As IntPtr
            Public nvEncGetEncodePresetCount As IntPtr
            Public nvEncGetEncodePresetGUIDs As IntPtr
            Public nvEncGetEncodePresetConfig As IntPtr
            Public nvEncInitializeEncoder As IntPtr
            Public nvEncCreateInputBuffer As IntPtr
            Public nvEncDestroyInputBuffer As IntPtr
            Public nvEncCreateBitstreamBuffer As IntPtr
            Public nvEncDestroyBitstreamBuffer As IntPtr
            Public nvEncEncodePicture As IntPtr
            Public nvEncLockBitstream As IntPtr
            Public nvEncUnlockBitstream As IntPtr
            Public nvEncLockInputBuffer As IntPtr
            Public nvEncUnlockInputBuffer As IntPtr
            Public nvEncGetEncodeStats As IntPtr
            Public nvEncGetSequenceParams As IntPtr
            Public nvEncRegisterAsyncEvent As IntPtr
            Public nvEncUnregisterAsyncEvent As IntPtr
            Public nvEncMapInputResource As IntPtr
            Public nvEncUnmapInputResource As IntPtr
            Public nvEncDestroyEncoder As IntPtr
            Public nvEncInvalidateRefFrames As IntPtr
            Public nvEncOpenEncodeSessionEx As IntPtr
            Public nvEncRegisterResource As IntPtr
            Public nvEncUnregisterResource As IntPtr
            Public nvEncReconfigureEncoder As IntPtr
            Public reserved1 As IntPtr
            Public nvEncCreateMVBuffer As IntPtr
            Public nvEncDestroyMVBuffer As IntPtr
            Public nvEncRunMotionEstimationOnly As IntPtr
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=281)>
            Public reserved2 As IntPtr()
        End Structure

        ' === Delegates ===
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncOpenEncodeSessionExDelegate(
            ByRef sessionParams As NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS,
            ByRef encoder As IntPtr) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncGetEncodeGUIDCountDelegate(
            encoder As IntPtr, ByRef count As Integer) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncGetEncodeGUIDsDelegate(
            encoder As IntPtr,
            <Out> guidArray As Guid(), arraySize As Integer,
            ByRef actualCount As Integer) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncGetInputFormatCountDelegate(
            encoder As IntPtr, encodeGUID As Guid,
            ByRef count As Integer) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncGetInputFormatsDelegate(
            encoder As IntPtr, encodeGUID As Guid,
            <Out> formatArray As UInteger(), arraySize As Integer,
            ByRef actualCount As Integer) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncRegisterResourceDelegate(
            encoder As IntPtr,
            ByRef registerParams As NV_ENC_REGISTER_RESOURCE) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncUnregisterResourceDelegate(
            encoder As IntPtr, registeredResource As IntPtr) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncDestroyEncoderDelegate(
            encoder As IntPtr) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncInitializeEncoderDelegate(
            encoder As IntPtr,
            ByRef initParams As NV_ENC_INITIALIZE_PARAMS) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncCreateBitstreamBufferDelegate(
            encoder As IntPtr,
            ByRef createParams As NV_ENC_CREATE_BITSTREAM_BUFFER) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncDestroyBitstreamBufferDelegate(
            encoder As IntPtr, bitstreamBuffer As IntPtr) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncMapInputResourceDelegate(
            encoder As IntPtr,
            ByRef mapParams As NV_ENC_MAP_INPUT_RESOURCE) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncUnmapInputResourceDelegate(
            encoder As IntPtr, mappedInputResource As IntPtr) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncEncodePictureDelegate(
            encoder As IntPtr,
            ByRef picParams As NV_ENC_PIC_PARAMS) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncLockBitstreamDelegate(
            encoder As IntPtr,
            ByRef lockParams As NV_ENC_LOCK_BITSTREAM) As UInteger

        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function NvEncUnlockBitstreamDelegate(
            encoder As IntPtr, bitstreamBuffer As IntPtr) As UInteger

        ' === P/Invoke loader functions ===
        <DllImport("nvEncodeAPI64.dll", CallingConvention:=CallingConvention.StdCall,
                   SetLastError:=False, BestFitMapping:=False, ThrowOnUnmappableChar:=True)>
        Public Shared Function NvEncodeAPICreateInstance(
            ByRef functionList As NV_ENCODE_API_FUNCTION_LIST) As UInteger
        End Function

        <DllImport("nvEncodeAPI64.dll", CallingConvention:=CallingConvention.StdCall,
                   SetLastError:=False, BestFitMapping:=False, ThrowOnUnmappableChar:=True)>
        Public Shared Function NvEncodeAPIGetMaxSupportedVersion(
            ByRef version As UInteger) As UInteger
        End Function

        Public Shared Function NvencStatusToString(status As UInteger) As String
            Select Case status
                Case NV_ENC_SUCCESS : Return "NV_ENC_SUCCESS"
                Case NV_ENC_ERR_NO_ENCODE_DEVICE : Return "NV_ENC_ERR_NO_ENCODE_DEVICE"
                Case NV_ENC_ERR_UNSUPPORTED_DEVICE : Return "NV_ENC_ERR_UNSUPPORTED_DEVICE"
                Case NV_ENC_ERR_INVALID_ENCODERDEVICE : Return "NV_ENC_ERR_INVALID_ENCODERDEVICE"
                Case NV_ENC_ERR_INVALID_DEVICE : Return "NV_ENC_ERR_INVALID_DEVICE"
                Case NV_ENC_ERR_DEVICE_NOT_EXIST : Return "NV_ENC_ERR_DEVICE_NOT_EXIST"
                Case NV_ENC_ERR_INVALID_PTR : Return "NV_ENC_ERR_INVALID_PTR"
                Case NV_ENC_ERR_INVALID_EVENT : Return "NV_ENC_ERR_INVALID_EVENT"
                Case NV_ENC_ERR_INVALID_PARAM : Return "NV_ENC_ERR_INVALID_PARAM"
                Case NV_ENC_ERR_INVALID_CALL : Return "NV_ENC_ERR_INVALID_CALL"
                Case NV_ENC_ERR_OUT_OF_MEMORY : Return "NV_ENC_ERR_OUT_OF_MEMORY"
                Case NV_ENC_ERR_ENCODER_NOT_INITIALIZED : Return "NV_ENC_ERR_ENCODER_NOT_INITIALIZED"
                Case NV_ENC_ERR_UNSUPPORTED_PARAM : Return "NV_ENC_ERR_UNSUPPORTED_PARAM"
                Case NV_ENC_ERR_LOCK_BUSY : Return "NV_ENC_ERR_LOCK_BUSY"
                Case NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA : Return "NV_ENC_ERR_NOT_ENOUGH_INPUT_DATA"
                Case NV_ENC_ERR_INVALID_VERSION : Return "NV_ENC_ERR_INVALID_VERSION"
                Case NV_ENC_ERR_MAP_FAILED : Return "NV_ENC_ERR_MAP_FAILED"
                Case NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY : Return "NV_ENC_ERR_INCOMPATIBLE_CLIENT_KEY"
                Case NV_ENC_ERR_UNIMPLEMENTED : Return "NV_ENC_ERR_UNIMPLEMENTED"
                Case NV_ENC_ERR_RESOURCE_REGISTER_FAILED : Return "NV_ENC_ERR_RESOURCE_REGISTER_FAILED"
                Case NV_ENC_ERR_RESOURCE_NOT_REGISTERED : Return "NV_ENC_ERR_RESOURCE_NOT_REGISTERED"
                Case NV_ENC_ERR_RESOURCE_NOT_MAPPED : Return "NV_ENC_ERR_RESOURCE_NOT_MAPPED"
                Case Else : Return $"NV_ENC_ERR_UNKNOWN({status})"
            End Select
        End Function

    End Class

End Namespace
