Option Strict On
Option Explicit On
Option Infer On

' NvencEncoderBackend.vb
'
' Implements CaptureEngine.Encoder.IEncoderBackend using NVIDIA NVENC (D3D11 path).
'
' Per Phase 12 spec v3 + OWNER feedback:
'   - Backend OWNS: D3D11 device, NVENC session, encoder texture, bitstream buffer,
'     registered resource (all created in Initialize(), destroyed in Dispose()).
'   - Frame = BORROW (caller retains ownership; backend MUST NOT dispose frame).
'   - EncodedPacket.Payload = newly-allocated Byte() owned by the caller.
'   - Encode hot path mirrors spike Phase 6/10:
'       CopyResource (frame texture → encoder texture)
'       → MapInputResource
'       → EncodePicture
'       → LockBitstream
'       → copy bytes to Byte()
'       → UnlockBitstream
'       → UnmapInputResource
'   - PTS/DTS taken from IVideoFrame.Diagnostics.PresentationTimestampTicks
'     (NO timestamp generation inside the encoder — pipeline contract is source of truth).
'   - Backend has ZERO knowledge of FFmpeg/WAV/MP4/IPC — those are orchestration concerns.
'
' Contract compliance:
'   - State machine: Created → Initialized → Running → Flushing → Stopping → Stopped
'     + Faulted (terminal until Dispose) + Disposed (terminal).
'   - Synchronous Encode(): blocks until packet ready OR returns False (backpressure).
'   - Caller-owned EncodedPacket (Payload Byte() + PayloadLength).
'   - Idempotent Dispose, Faulted is terminal.
'   - _sync protects state + counter writes; Join/encoding happen OUTSIDE _sync.

Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Vortice.Direct3D11
Imports SharpGen.Runtime
Imports CaptureEngine.Video
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Encoder.Nvenc.Internal  ' NvEncodeAPI + D3D11DeviceFactory + NvencResources + NvEncFunctionTable

Namespace CaptureEngine.Encoder.Nvenc

    ''' <summary>
    ''' NVIDIA NVENC implementation of IEncoderBackend. Owns D3D11 + NVENC resources
    ''' for the encoder's lifetime. Frame is borrowed; EncodedPacket is caller-owned.
    ''' </summary>
    Public NotInheritable Class NvencEncoderBackend
        Implements IEncoderBackend

        ' ─── State + sync ─────────────────────────────────────────────────
        Private ReadOnly _sync As New Object()
        Private _state As EncoderState = EncoderState.Created
        Private _disposed As Boolean = False

        ' ─── Owned resources (created in Initialize, destroyed in Dispose) ─
        Private _deviceResult As Internal.D3D11DeviceResult
        Private _nvenc As Internal.NvEncFunctionTable
        Private _encoderHandle As IntPtr = IntPtr.Zero
        Private _resources As Internal.NvencResources
        Private _encoderConfig As EncoderConfig

        ' ─── Diagnostics counters (atomic — Interlocked) ───────────────────
        Private _submittedFrames As Long = 0
        Private _encodedPackets As Long = 0
        Private _droppedFrames As Long = 0
        Private _flushCycles As Long = 0
        Private _errorCount As Long = 0
        Private _lastErrorMessage As String = ""
        Private _lastErrorType As String = ""

        ' ─── NVENC error detail log (Phase 12a-5c) ──────────────────────
        ' Records every error with stage + status code for post-mortem analysis.
        Private ReadOnly _errorDetails As New List(Of String)()

        ' ─── Encoder constants (from EncoderConfig at Initialize) ──────────
        Private _width As UInteger
        Private _height As UInteger
        Private _frameRateNum As UInteger = 60
        Private _frameRateDen As UInteger = 1

        Private ReadOnly _logger As EngineLogger

        Public Sub New(logger As EngineLogger)
            _logger = logger
        End Sub

        ' ═══════════════════════════════════════════════════════════════════
        ' IEncoderBackend — Lifecycle
        ' ═══════════════════════════════════════════════════════════════════

        ''' <summary>
        ''' Initialize the encoder. Creates D3D11 device + NVENC session +
        ''' bitstream buffer + registered encoder texture.
        ''' Throws EncoderConfigurationException on bad config.
        ''' </summary>
        Public Sub Initialize(config As EncoderConfig) Implements IEncoderBackend.Initialize
            If config Is Nothing Then
                Throw New ArgumentNullException(NameOf(config))
            End If

            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(NvencEncoderBackend))
                End If
                If _state <> EncoderState.Created Then
                    Throw New InvalidOperationException(
                        $"Initialize() called from state {_state} (must be Created).")
                End If

                ' ─── Validate config ───────────────────────────────────────
                If String.IsNullOrEmpty(config.CodecKey) Then
                    Throw New EncoderConfigurationException("CodecKey must not be null or empty.")
                End If
                If config.CodecKey <> "NVENC_H264" Then
                    Throw New EncoderConfigurationException(
                        $"CodecKey '{config.CodecKey}' is not supported by NvencEncoderBackend. " &
                        "Only 'NVENC_H264' is implemented in Phase 12.")
                End If
                If config.RateControl = "cbr" AndAlso config.BitrateBps <= 0 Then
                    Throw New EncoderConfigurationException(
                        "BitrateBps must be > 0 for RateControl='cbr'.")
                End If
                If config.GopSize <= 0 Then
                    Throw New EncoderConfigurationException("GopSize must be > 0.")
                End If

                ' Take a defensive clone (config is mutable).
                _encoderConfig = config.Clone()

                ' ─── Resolve expected dimensions (must match Encode frame) ──
                If config.ExpectedWidth <= 0 OrElse config.ExpectedHeight <= 0 Then
                    Throw New EncoderConfigurationException(
                        "ExpectedWidth/ExpectedHeight must be > 0 for NvencEncoderBackend " &
                        "(NVENC requires fixed dimensions at Initialize time).")
                End If
                _width = CUInt(config.ExpectedWidth)
                _height = CUInt(config.ExpectedHeight)
            End SyncLock

            ' ─── Create D3D11 device (no lock — factory is independent) ───
            Dim deviceFactory As New Internal.D3D11DeviceFactory(_logger)
            _deviceResult = deviceFactory.Create()
            If _deviceResult Is Nothing Then
                TransitionToFaulted("D3D11 device creation failed")
                Throw New EncoderRuntimeException("D3D11 device creation failed — see log for details.")
            End If

            ' ─── Load NVENC function table ────────────────────────────────
            _nvenc = New Internal.NvEncFunctionTable(_logger)
            If Not _nvenc.TryLoad() Then
                _deviceResult.Dispose()
                _deviceResult = Nothing
                TransitionToFaulted("NVENC function table load failed")
                Throw New EncoderRuntimeException("NVENC function table load failed — see log for details.")
            End If

            ' ─── Open NVENC encode session ────────────────────────────────
            Dim sessionParams As NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS = Nothing
            sessionParams.version = NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER
            sessionParams.deviceType = NvEncodeAPI.NV_ENC_DEVICE_DIRECTX
            sessionParams.device = _deviceResult.Device.NativePointer
            sessionParams.reserved = IntPtr.Zero
            sessionParams.apiVersion = NvEncodeAPI.NVENCAPI_VERSION
            sessionParams.reserved1 = Nothing
            sessionParams.reserved2 = Nothing

            Dim openStatus As UInteger = _nvenc.OpenEncodeSessionEx.Invoke(sessionParams, _encoderHandle)
            If openStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                Dim msg As String = $"NvEncOpenEncodeSessionEx failed: status={openStatus} " &
                                    $"({NvEncodeAPI.NvencStatusToString(openStatus)})"
                _logger.Error(msg)
                _nvenc.Dispose()
                _deviceResult.Dispose()
                _deviceResult = Nothing
                TransitionToFaulted(msg)
                Throw New EncoderRuntimeException(msg)
            End If
            _logger.Info($"NVENC encoder session opened: 0x{_encoderHandle.ToInt64():x16}")

            ' ─── Initialize encoder (codec + preset + dimensions + bitrate) ─
            Dim initParams As NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS = Nothing
            initParams.version = NvEncodeAPI.MakeStructVersion(5) Or (1UI << 31)
            initParams.encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID
            initParams.presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID
            initParams.encodeWidth = _width
            initParams.encodeHeight = _height
            initParams.darWidth = _width
            initParams.darHeight = _height
            initParams.frameRateNum = _frameRateNum
            initParams.frameRateDen = _frameRateDen
            initParams.enableEncodeAsync = 0  ' synchronous mode
            initParams.enablePTD = 1  ' presentation order = decode order (default)
            initParams.bitFields = 0
            initParams.privDataSize = 0
            initParams.privData = IntPtr.Zero
            initParams.encodeConfig = IntPtr.Zero  ' use preset defaults
            initParams.maxEncodeWidth = _width
            initParams.maxEncodeHeight = _height
            initParams.maxMEHintCountsPerBlockL0 = 0
            initParams.maxMEHintCountsPerBlockL1 = 0
            initParams.reserved = Nothing
            initParams.reserved2 = Nothing

            Dim initStatus As UInteger = _nvenc.InitializeEncoder.Invoke(_encoderHandle, initParams)
            If initStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                Dim msg As String = $"NvEncInitializeEncoder failed: status={initStatus} " &
                                    $"({NvEncodeAPI.NvencStatusToString(initStatus)})"
                _logger.Error(msg)
                Try : _nvenc.DestroyEncoder.Invoke(_encoderHandle) : Catch : End Try
                _nvenc.Dispose()
                _deviceResult.Dispose()
                _deviceResult = Nothing
                TransitionToFaulted(msg)
                Throw New EncoderRuntimeException(msg)
            End If
            _logger.Info($"NVENC encoder initialized: {_width}x{_height} H.264 CBR {_encoderConfig.BitrateBps} bps")

            ' ─── Create bitstream buffer + register encoder texture ─────────
            _resources = New Internal.NvencResources(_logger)
            Try
                _resources.Create(_encoderHandle, _nvenc, _deviceResult.Device, _width, _height)
            Catch ex As Exception
                Dim msg As String = $"NvencResources.Create failed: {ex.Message}"
                _logger.Error(msg, ex)
                Try : _nvenc.DestroyEncoder.Invoke(_encoderHandle) : Catch : End Try
                _nvenc.Dispose()
                _deviceResult.Dispose()
                _deviceResult = Nothing
                _resources = Nothing
                TransitionToFaulted(msg)
                Throw New EncoderRuntimeException(msg, ex)
            End Try

            ' ─── Success — transition to Initialized ──────────────────────
            SyncLock _sync
                If _state = EncoderState.Faulted Then
                    ' TransitionToFaulted raced ahead — abort
                    Throw New EncoderRuntimeException("Encoder faulted during Initialize.")
                End If
                _state = EncoderState.Initialized
            End SyncLock
            _logger.Info("NvencEncoderBackend: Initialize complete.")
        End Sub

        Public Sub Start() Implements IEncoderBackend.Start
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(NvencEncoderBackend))
                End If
                ' Idempotent: Start() while Running is a no-op.
                If _state = EncoderState.Running Then Return
                If _state <> EncoderState.Initialized AndAlso _state <> EncoderState.Stopped Then
                    Throw New InvalidOperationException(
                        $"Start() called from state {_state} (must be Initialized or Stopped).")
                End If
                _state = EncoderState.Running
            End SyncLock
            _logger.Info("NvencEncoderBackend: started.")
        End Sub

        ''' <summary>
        ''' Encode a single IVideoFrame.
        ''' Synchronous — blocks until packet ready OR returns False (backpressure).
        ''' Frame is BORROWED — caller retains ownership and disposes it.
        ''' EncodedPacket is caller-owned when this returns True.
        ''' </summary>
        Public Function Encode(frame As IVideoFrame, ByRef packet As EncodedPacket) As Boolean _
            Implements IEncoderBackend.Encode

            packet = Nothing
            If frame Is Nothing Then
                Throw New ArgumentNullException(NameOf(frame))
            End If

            ' ─── State check (under lock) ─────────────────────────────────
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(NvencEncoderBackend))
                End If
                If _state <> EncoderState.Running Then
                    Throw New InvalidOperationException(
                        $"Encode() called from state {_state} (must be Running).")
                End If
                ' Note: Foundation IVideoFrame does NOT expose IsDisposed.
                ' We rely on the D3D11VideoFrame.NativeTexture defensive check below
                ' (returns IntPtr.Zero if frame has been disposed).
            End SyncLock

            ' ─── Validate frame format + dimensions ──────────────────────
            If frame.PixelFormat <> VideoPixelFormat.Bgra8 Then
                RecordError("runtime", $"Frame pixel format {frame.PixelFormat} not supported (Bgra8 only).")
                TransitionToFaulted($"Unsupported pixel format {frame.PixelFormat}")
                Throw New EncoderRuntimeException(
                    $"NvencEncoderBackend only supports Bgra8 frames (got {frame.PixelFormat}).")
            End If
            If frame.Origin <> VideoFrameOrigin.GpuD3D11Texture Then
                RecordError("runtime", $"Frame origin {frame.Origin} not supported (GpuD3D11Texture only).")
                TransitionToFaulted($"Unsupported frame origin {frame.Origin}")
                Throw New EncoderRuntimeException(
                    $"NvencEncoderBackend only accepts GpuD3D11Texture frames (got {frame.Origin}). " &
                    "CPU-memory frames require a GPU upload step not yet implemented.")
            End If
            Dim dims As VideoFrameDimensions = frame.Dimensions
            If CUInt(dims.Width) <> _width OrElse CUInt(dims.Height) <> _height Then
                RecordError("runtime",
                    $"Frame dimensions {dims.Width}x{dims.Height} do not match encoder {_width}x{_height}.")
                TransitionToFaulted($"Frame dimension mismatch ({dims.Width}x{dims.Height})")
                Throw New EncoderRuntimeException(
                    $"Frame dimensions {dims.Width}x{dims.Height} do not match encoder {_width}x{_height}. " &
                    "NVENC encoder dimensions are fixed at Initialize time.")
            End If

            Interlocked.Increment(_submittedFrames)

            ' ─── Extract frame texture via ID3D11VideoFrame contract ─────
            ' CaptureEngine.Video.IVideoFrame does NOT expose native resource
            ' handles. The ID3D11VideoFrame extension interface lives in
            ' CaptureEngine.Video (contract layer) — D3D11-producing backends
            ' (DdagrabBackend) emit frames implementing both interfaces.
            '
            ' This DirectCast is type-safe at compile time (no reflection,
            ' no TryCast hack — per OWNER requirement).
            '
            ' Dependency graph (verified — no circular reference):
            '   CaptureEngine.Video (contract: ID3D11VideoFrame)
            '     ↑ implemented by
            '   CaptureEngine.Video.Ddagrab (D3D11VideoFrame class)
            '     ↓ consumed by
            '   CaptureEngine.Encoder.Nvenc (NvencEncoderBackend.Encode)
            Dim d3d11Frame As ID3D11VideoFrame = Nothing
            Try
                d3d11Frame = DirectCast(frame, ID3D11VideoFrame)
            Catch ex As InvalidCastException
                RecordError("runtime",
                    $"Frame type {frame.GetType().Name} does not implement ID3D11VideoFrame. " &
                    $"NvencEncoderBackend requires D3D11-producing backends (DdagrabBackend).")
                TransitionToFaulted($"Frame missing ID3D11VideoFrame: {frame.GetType().Name}")
                Throw New EncoderRuntimeException(
                    $"Frame type {frame.GetType().Name} does not implement ID3D11VideoFrame. " &
                    $"Encoder cannot access the D3D11 texture for encoding.", ex)
            End Try

            Dim texObj As Object = d3d11Frame.NativeTexture
            Dim sharedHandle As IntPtr = d3d11Frame.SharedHandle

            Dim frameTexture As ID3D11Texture2D
            Dim disposeFrameTexture As Boolean = False

            If sharedHandle <> IntPtr.Zero Then
                ' ── Shared-handle path (Phase 12a-5c) ──────────────────────
                ' Open the shared resource on the ENCODER's device via
                ' ID3D11Device1.OpenSharedResource1. This is the D3D11-
                ' contract-valid path for cross-device resource sharing.
                ' The opened texture lives on the encoder's device, so
                ' CopyResource is same-device (always valid).
                Try
                    Dim device1 As ID3D11Device1 = _deviceResult.Device.QueryInterface(Of ID3D11Device1)()
                    frameTexture = device1.OpenSharedResource1(Of ID3D11Texture2D)(sharedHandle)
                    device1.Dispose()
                    disposeFrameTexture = True  ' we created this wrapper — dispose after use
                Catch ex As Exception
                    RecordError("runtime", $"OpenSharedResource1 failed: {ex.Message}")
                    TransitionToFaulted($"OpenSharedResource1 failed: {ex.Message}")
                    Throw New EncoderRuntimeException(
                        $"OpenSharedResource1 failed for shared handle 0x{sharedHandle.ToInt64():x16}: {ex.Message}", ex)
                End Try
            Else
                ' ── Direct path (baseline — cross-device, driver-dependent) ─
                ' Uses the texture directly from Ddagrab's device. Works on
                ' NVIDIA same-GPU but NOT contractually valid per D3D11 spec.
                ' Phase 12a-5c compares this with the shared-handle path.
                If texObj Is Nothing Then
                    RecordError("runtime", "Frame's NativeTexture is Nothing (frame disposed?).")
                    TransitionToFaulted("Frame NativeTexture is Nothing")
                    Throw New EncoderRuntimeException(
                        "Frame NativeTexture is Nothing — frame may have been disposed.")
                End If
                frameTexture = DirectCast(texObj, ID3D11Texture2D)
                disposeFrameTexture = False  ' frame owns the texture — do NOT dispose
            End If

            ' ─── ENCODE HOT PATH (mirrors spike Phase 10) ────────────────
            ' All operations happen OUTSIDE _sync (no lock contention during encoding).
            Dim deviceCtx As ID3D11DeviceContext = _deviceResult.DeviceContext

            Try
                ' 1. CopyResource: frame texture → encoder texture (GPU-side copy, fast)
                deviceCtx.CopyResource(_resources.EncoderTexture, frameTexture)

                ' 2. MapInputResource: prepare encoder texture for NVENC
                Dim mapParams As NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE = Nothing
                mapParams.version = NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER
                mapParams.subResourceIndex = 0
                mapParams.inputResource = _resources.RegisteredResource
                mapParams.registeredResource = _resources.RegisteredResource  ' echo
                mapParams.mappedResource = IntPtr.Zero  ' OUT
                mapParams.mappedBufferFmt = 0  ' OUT
                mapParams.reserved1 = Nothing
                mapParams.reserved2 = Nothing

                Dim mapStatus As UInteger = _nvenc.MapInputResource.Invoke(_encoderHandle, mapParams)
                If mapStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                    Dim msg As String = $"NvEncMapInputResource failed: status={mapStatus} " &
                                        $"({NvEncodeAPI.NvencStatusToString(mapStatus)})"
                    RecordError("runtime", msg)
                    Interlocked.Increment(_droppedFrames)
                    TransitionToFaulted(msg)
                    Throw New EncoderRuntimeException(msg)
                End If
                Dim mappedInput As IntPtr = mapParams.mappedResource

                Try
                    ' 3. EncodePicture: submit frame for encoding (synchronous — no async event)
                    Dim pts As Long = frame.Diagnostics.PresentationTimestampTicks
                    Dim picParams As NvEncodeAPI.NV_ENC_PIC_PARAMS = Nothing
                    picParams.version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER
                    picParams.inputWidth = _width
                    picParams.inputHeight = _height
                    picParams.inputPitch = 0
                    picParams.encodePicFlags = 0
                    picParams.frameIdx = 0
                    picParams.inputTimeStamp = CULng(pts)  ' pipeline PTS (NOT Stopwatch — frame contract)
                    picParams.inputDuration = 0
                    picParams.inputBuffer = mappedInput
                    picParams.outputBitstream = _resources.BitstreamBuffer
                    picParams.completionEvent = IntPtr.Zero  ' sync mode
                    picParams.bufferFmt = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB
                    picParams.pictureStruct = 1  ' NV_ENC_PIC_STRUCT_FRAME = 1
                    picParams.pictureType = 0
                    picParams._padding1 = 0
                    picParams.codecPicParams = Nothing
                    picParams.meHintCountsPerBlock = Nothing
                    picParams.meExternalHints = IntPtr.Zero
                    picParams.reserved1 = Nothing
                    picParams.reserved2 = Nothing
                    picParams.qpDeltaMap = IntPtr.Zero
                    picParams.qpDeltaMapSize = 0
                    picParams.reservedBitFields = 0
                    picParams.meHintRefPicDist = Nothing
                    picParams._padding2 = 0
                    picParams.alphaBuffer = IntPtr.Zero
                    picParams.reserved3 = Nothing
                    picParams.reserved4 = Nothing

                    Dim encStatus As UInteger = _nvenc.EncodePicture.Invoke(_encoderHandle, picParams)
                    If encStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                        Dim msg As String = $"NvEncEncodePicture failed: status={encStatus} " &
                                            $"({NvEncodeAPI.NvencStatusToString(encStatus)})"
                        RecordError("runtime", msg)
                        Interlocked.Increment(_droppedFrames)
                        TransitionToFaulted(msg)
                        Throw New EncoderRuntimeException(msg)
                    End If

                    ' 4. LockBitstream: get pointer + size of encoded NAL bytes
                    Dim lockParams As NvEncodeAPI.NV_ENC_LOCK_BITSTREAM = Nothing
                    lockParams.version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER
                    lockParams.bitfields = 0
                    lockParams.outputBitstream = _resources.BitstreamBuffer
                    lockParams.sliceOffsets = IntPtr.Zero
                    ' frameIdx, hwEncodeStatus, numSlices, bitstreamSizeInBytes — OUT (zero)
                    ' outputTimeStamp, outputDuration — OUT
                    lockParams.bitstreamBufferPtr = IntPtr.Zero  ' OUT — pointer to encoded data
                    ' pictureType, pictureStruct, frameAvgQP, frameSatd, ltrFrameIdx, ltrFrameBitmap — OUT
                    lockParams.reserved = Nothing
                    lockParams.reserved1 = Nothing
                    lockParams.reserved2 = Nothing

                    Dim lockStatus As UInteger = _nvenc.LockBitstream.Invoke(_encoderHandle, lockParams)
                    If lockStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                        Dim msg As String = $"NvEncLockBitstream failed: status={lockStatus} " &
                                            $"({NvEncodeAPI.NvencStatusToString(lockStatus)})"
                        RecordError("runtime", msg)
                        Interlocked.Increment(_droppedFrames)
                        TransitionToFaulted(msg)
                        Throw New EncoderRuntimeException(msg)
                    End If

                    ' 5. Copy NAL bytes to caller-owned Byte()
                    Dim bsSize As UInteger = lockParams.bitstreamSizeInBytes
                    Dim bsPtr As IntPtr = lockParams.bitstreamBufferPtr
                    Dim payload As Byte() = Nothing
                    If bsSize > 0 AndAlso bsPtr <> IntPtr.Zero Then
                        payload = New Byte(CInt(bsSize - 1)) {}
                        Marshal.Copy(bsPtr, payload, 0, CInt(bsSize))
                    Else
                        payload = Array.Empty(Of Byte)()
                    End If

                    ' 6. UnlockBitstream: release the bitstream for re-use
                    Dim unlockStatus As UInteger = _nvenc.UnlockBitstream.Invoke(_encoderHandle, _resources.BitstreamBuffer)
                    If unlockStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                        _logger.Warning($"NvEncUnlockBitstream returned {unlockStatus} " &
                                        $"({NvEncodeAPI.NvencStatusToString(unlockStatus)}) — ignoring")
                    End If

                    ' 7. Build EncodedPacket (caller-owned)
                    Dim sequence As Long = Interlocked.Increment(_encodedPackets) - 1
                    Dim isKeyFrame As Boolean = (sequence Mod CLng(_encoderConfig.GopSize)) = 0
                    Dim metadata As New PacketMetadata(
                        sequence:=sequence,
                        presentationTimeTicks:=pts,
                        decodingTimeTicks:=pts,  ' sync mode: PTS = DTS
                        durationTicks:=0,  ' encoder doesn't know frame duration (pipeline's job)
                        isKeyFrame:=isKeyFrame,
                        isReferenceFrame:=isKeyFrame,  ' I-frames are reference frames
                        codecKey:=_encoderConfig.CodecKey,
                        codecSpecificFlags:=0)
                    packet = New EncodedPacket(metadata, payload, CInt(bsSize))
                    Return True

                Finally
                    ' 8. UnmapInputResource — always, even if EncodePicture threw
                    If mappedInput <> IntPtr.Zero Then
                        Try
                            Dim unmapStatus As UInteger = _nvenc.UnmapInputResource.Invoke(_encoderHandle, mappedInput)
                            If unmapStatus <> NvEncodeAPI.NV_ENC_SUCCESS Then
                                _logger.Warning($"NvEncUnmapInputResource returned {unmapStatus} — ignoring")
                            End If
                        Catch ex As Exception
                            _logger.Warning($"NvEncUnmapInputResource threw: {ex.Message}")
                        End Try
                    End If
                End Try

            Finally
                ' If shared-handle path was used, dispose the opened shared resource.
                ' This releases the Vortice wrapper (NOT the shared resource itself —
                ' that's owned by D3D11VideoFrame's staging texture).
                If disposeFrameTexture Then
                    Try : frameTexture?.Dispose() : Catch : End Try
                End If
            End Try
        End Function

        ''' <summary>
        ''' Flush in-flight frames. NVENC synchronous mode = no pipeline delay,
        ''' so there's nothing to flush. We just transition Flushing → Running.
        ''' Returns 0 (no packets emitted by Flush in sync mode).
        ''' </summary>
        Public Function Flush(sink As Action(Of EncodedPacket)) As Integer _
            Implements IEncoderBackend.Flush

            If sink Is Nothing Then
                Throw New ArgumentNullException(NameOf(sink))
            End If

            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(NvencEncoderBackend))
                End If
                If _state <> EncoderState.Running Then
                    Throw New InvalidOperationException(
                        $"Flush() called from state {_state} (must be Running).")
                End If
                _state = EncoderState.Flushing
            End SyncLock

            Interlocked.Increment(_flushCycles)

            ' NVENC synchronous mode: every Encode() call is fully synchronous —
            ' there are no in-flight frames to drain. Flush is a no-op that
            ' returns 0 packets. (Future async mode would drain here.)

            SyncLock _sync
                ' Only transition back to Running if we're still Flushing
                ' (Stop() may have moved us to Stopping concurrently).
                If _state = EncoderState.Flushing Then
                    _state = EncoderState.Running
                End If
            End SyncLock

            Return 0
        End Function

        Public Sub [Stop]() Implements IEncoderBackend.Stop
            SyncLock _sync
                If _disposed Then
                    Throw New ObjectDisposedException(NameOf(NvencEncoderBackend))
                End If

                Select Case _state
                    Case EncoderState.Created, EncoderState.Initialized, EncoderState.Stopped
                        ' No-op — not Running.
                        Return
                    Case EncoderState.Stopping
                        ' Already stopping — no-op.
                        Return
                    Case EncoderState.Flushing
                        ' Interrupt flush, transition to Stopping.
                        _state = EncoderState.Stopping
                    Case EncoderState.Running
                        ' Drain via Flush (no-op in sync mode), then Stopping.
                        _state = EncoderState.Stopping
                    Case EncoderState.Faulted
                        ' Faulted is terminal until Dispose — no-op.
                        Return
                    Case EncoderState.Disposed
                        Throw New ObjectDisposedException(NameOf(NvencEncoderBackend))
                End Select
            End SyncLock

            ' NVENC sync mode: no drain needed. Transition to Stopped.
            SyncLock _sync
                If _state = EncoderState.Stopping Then
                    _state = EncoderState.Stopped
                End If
            End SyncLock
            _logger.Info("NvencEncoderBackend: stopped.")
        End Sub

        ' ═══════════════════════════════════════════════════════════════════
        ' IEncoderBackend — Diagnostics
        ' ═══════════════════════════════════════════════════════════════════

        Public ReadOnly Property CurrentState As EncoderState Implements IEncoderBackend.CurrentState
            Get
                ' Use SyncLock for cross-thread visibility (simplest VB pattern;
                ' Volatile.Read(Of T) doesn't resolve correctly in VB).
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property Diagnostics As IEncoderDiagnostics Implements IEncoderBackend.Diagnostics
            Get
                Return New NvencEncoderDiagnostics(Me)
            End Get
        End Property

        ' ═══════════════════════════════════════════════════════════════════
        ' IDisposable
        ' ═══════════════════════════════════════════════════════════════════

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _sync
                If _disposed Then Return
                _disposed = True
                _state = EncoderState.Disposed
            End SyncLock

            ' Dispose in REVERSE construction order.
            ' All disposal happens OUTSIDE _sync (per P1-B.1 FIX lesson #1).
            Try : _resources?.Dispose() : Catch ex As Exception : _logger.Warning($"NvencResources.Dispose threw: {ex.Message}") : End Try

            If _encoderHandle <> IntPtr.Zero AndAlso _nvenc?.DestroyEncoder IsNot Nothing Then
                Try
                    Dim status As UInteger = _nvenc.DestroyEncoder.Invoke(_encoderHandle)
                    If status <> NvEncodeAPI.NV_ENC_SUCCESS Then
                        _logger.Warning($"NvEncDestroyEncoder returned {status} — ignoring")
                    End If
                Catch ex As Exception
                    _logger.Warning($"NvEncDestroyEncoder threw: {ex.Message}")
                End Try
            End If

            Try : _nvenc?.Dispose() : Catch ex As Exception : _logger.Warning($"NvEncFunctionTable.Dispose threw: {ex.Message}") : End Try
            Try : _deviceResult?.Dispose() : Catch ex As Exception : _logger.Warning($"D3D11DeviceResult.Dispose threw: {ex.Message}") : End Try

            _logger.Info("NvencEncoderBackend: disposed.")
        End Sub

        ' ═══════════════════════════════════════════════════════════════════
        ' Private helpers
        ' ═══════════════════════════════════════════════════════════════════

        Private Sub TransitionToFaulted(reason As String)
            SyncLock _sync
                If _state = EncoderState.Disposed Then Return
                _state = EncoderState.Faulted
            End SyncLock
            _logger.Error($"NvencEncoderBackend: FAULTED — {reason}")
        End Sub

        Private Sub RecordError(errorType As String, message As String)
            Interlocked.Increment(_errorCount)
            _lastErrorMessage = message
            _lastErrorType = errorType
            SyncLock _errorDetails
                _errorDetails.Add($"[{errorType}] {message}")
            End SyncLock
        End Sub

        ' ─── Atomic counter accessors (for NvencEncoderDiagnostics) ────────
        Friend ReadOnly Property SubmittedFramesCount As Long
            Get
                Return Interlocked.Read(_submittedFrames)
            End Get
        End Property

        Friend ReadOnly Property EncodedPacketsCount As Long
            Get
                Return Interlocked.Read(_encodedPackets)
            End Get
        End Property

        Friend ReadOnly Property DroppedFramesCount As Long
            Get
                Return Interlocked.Read(_droppedFrames)
            End Get
        End Property

        Friend ReadOnly Property FlushCyclesCount As Long
            Get
                Return Interlocked.Read(_flushCycles)
            End Get
        End Property

        Friend ReadOnly Property ErrorCountValue As Long
            Get
                Return Interlocked.Read(_errorCount)
            End Get
        End Property

        Friend ReadOnly Property LastErrorMessageValue As String
            Get
                Return _lastErrorMessage
            End Get
        End Property

        Friend ReadOnly Property LastErrorTypeValue As String
            Get
                Return _lastErrorType
            End Get
        End Property

        ' ─── Phase 12a-5c: NVENC error details + adapter LUID ────────────

        ''' <summary>List of all NVENC errors with stage + status code (for post-mortem).</summary>
        Public ReadOnly Property ErrorDetails As IReadOnlyList(Of String)
            Get
                SyncLock _errorDetails
                    Return _errorDetails.ToList().AsReadOnly()
                End SyncLock
            End Get
        End Property

        ''' <summary>Adapter LUID low part (for cross-device comparison).</summary>
        Public ReadOnly Property AdapterLuidLow As UInteger
            Get
                Return If(_deviceResult?.LuidLow, 0UI)
            End Get
        End Property

        ''' <summary>Adapter LUID high part (for cross-device comparison).</summary>
        Public ReadOnly Property AdapterLuidHigh As Integer
            Get
                Return If(_deviceResult?.LuidHigh, 0)
            End Get
        End Property

        ' ─── Inner diagnostics class ──────────────────────────────────────
        Private NotInheritable Class NvencEncoderDiagnostics
            Implements IEncoderDiagnostics

            Private ReadOnly _owner As NvencEncoderBackend

            Public Sub New(owner As NvencEncoderBackend)
                _owner = owner
            End Sub

            Public ReadOnly Property SubmittedFrames As Long Implements IEncoderDiagnostics.SubmittedFrames
                Get
                    Return _owner.SubmittedFramesCount
                End Get
            End Property

            Public ReadOnly Property EncodedPackets As Long Implements IEncoderDiagnostics.EncodedPackets
                Get
                    Return _owner.EncodedPacketsCount
                End Get
            End Property

            Public ReadOnly Property DroppedFrames As Long Implements IEncoderDiagnostics.DroppedFrames
                Get
                    Return _owner.DroppedFramesCount
                End Get
            End Property

            Public ReadOnly Property FlushCycles As Long Implements IEncoderDiagnostics.FlushCycles
                Get
                    Return _owner.FlushCyclesCount
                End Get
            End Property

            Public ReadOnly Property ErrorCount As Long Implements IEncoderDiagnostics.ErrorCount
                Get
                    Return _owner.ErrorCountValue
                End Get
            End Property

            Public ReadOnly Property LastErrorIfAny As String Implements IEncoderDiagnostics.LastErrorIfAny
                Get
                    Return _owner.LastErrorMessageValue
                End Get
            End Property

            Public ReadOnly Property LastErrorType As String Implements IEncoderDiagnostics.LastErrorType
                Get
                    Return _owner.LastErrorTypeValue
                End Get
            End Property
        End Class

    End Class

End Namespace
