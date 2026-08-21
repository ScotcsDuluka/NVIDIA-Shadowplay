// Phases/Phase10_RealRecording.cs
//
// Phase 10: Real Recording Integration — Video + Audio → File
//
// Integrates proven components from Phases 1-9 into a real recording pipeline:
//   Video: DXGI Desktop Duplication → D3D11 GPU Texture → NVENC H.264 → raw NAL → FFmpeg stdin
//   Audio: WASAPI Loopback → PCM samples → temp WAV file
//   Mux:   FFmpeg subprocess combines video + audio → MP4 output file
//
// Architecture (independent pipelines, no cross-blocking):
//
//   Video Thread:                Audio Thread:              Main Thread:
//   ┌──────────────────┐        ┌──────────────────┐     ┌─────────────┐
//   │ DXGI Acquire     │        │ WASAPI Capture   │     │ Start       │
//   │   ↓              │        │   ↓              │     │   ↓         │
//   │ CopyResource     │        │ Write PCM to     │     │ Wait Stop   │
//   │   ↓              │        │   temp WAV file  │     │   ↓         │
//   │ NVENC Encode     │        │   ↓              │     │ Stop Audio  │
//   │   ↓              │        │ Wait Stop signal │     │   ↓         │
//   │ LockBitstream    │        │   ↓              │     │ Stop Video  │
//   │ Write NAL to     │        │ Close WAV file   │     │   ↓         │
//   │ FFmpeg stdin     │        └──────────────────┘     │ FFmpeg Mux  │
//   │   ↓              │                                  │   ↓         │
//   │ Unlock/Unmap     │                                  │ Finalize    │
//   │   ↓              │                                  └─────────────┘
//   │ Wait Stop signal │
//   │   ↓              │
//   │ Close FFmpeg     │
//   │ stdin (EOF)      │
//   └──────────────────┘
//
// FFmpeg subprocess is started at recording Start:
//   ffmpeg -y -f h264 -i pipe:0 -i audio.wav -c:v copy -c:a aac -shortest output.mp4
//
// The video thread writes raw H.264 NAL units to FFmpeg's stdin.
// The audio thread writes PCM to a WAV file.
// FFmpeg reads both inputs and muxes into a single MP4.
// When Stop is called, both threads close their outputs (stdin EOF + WAV close),
// and FFmpeg finalizes the container.
//
// Timestamps: all use QPC-derived 100-ns ticks (Option β).
//   - Video: Stopwatch.GetTimestamp() at AcquireNextFrame time
//   - Audio: Stopwatch.GetTimestamp() at WASAPI buffer capture time
//   - Both share the same QPC timebase (no conversion needed).
//
// SPDX-License-Identifier: MIT
// Spike code — not production. Phase 7 untouched.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Vortice.Direct3D11;
using Vortice.DXGI;
using CaptureEngine.Video.Spike.D3D11.Utils;

namespace CaptureEngine.Video.Spike.D3D11.Phases;

public static class Phase10_RealRecording
{
    [DllImport("ole32.dll")]
    private static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

    private const uint CLSCTX_ALL = 0x17;
    private const int AUDCLNT_STREAMFLAGS_LOOPBACK = 0x00020000;
    private const int AUDCLNT_SHAREMODE_SHARED = 0;
    private const int WAVE_FORMAT_PCM = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(ref Guid clsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid iid, out IntPtr ppv);

    private static readonly Guid CLSID_MMDeviceEnumerator = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
    private static readonly Guid IID_IMMDeviceEnumerator = new("A95664D2-9614-4F35-A746-DE8DB63617E6");
    private static readonly Guid IID_IAudioClient = new("1CB9AD4C-DBFA-4c32-B178-C2F568A703D2");
    private static readonly Guid IID_IAudioCaptureClient = new("C8ADBD64-E71E-48a0-A4DE-185C395CD317");

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int ImmDevice_GetDefaultAudioEndpoint(IntPtr thisPtr, int dataFlow, int role, out IntPtr endpoint);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int ImmDeviceActivator_Activate(IntPtr thisPtr, ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, out IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioClient_Initialize(IntPtr thisPtr, int shareMode, int streamFlags, long hnsBufferDuration, long hnsPeriodicity, IntPtr pFormat, ref Guid audioSessionGuid);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioClient_GetMixFormat(IntPtr thisPtr, out IntPtr ppFormat);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioClient_Start(IntPtr thisPtr);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioClient_Stop(IntPtr thisPtr);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioClient_GetService(IntPtr thisPtr, ref Guid iid, out IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioCapture_GetBuffer(IntPtr thisPtr, out IntPtr ppData, out uint pNumFrames, out int pdwFlags, out long pu64Position, out long pu64QPCPosition);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AudioCapture_ReleaseBuffer(IntPtr thisPtr, uint numFramesRead);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int QIDel(IntPtr thisPtr, ref Guid iid, out IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint AddRefDel(IntPtr thisPtr);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint RelDel(IntPtr thisPtr);


    private static string? s_outputPath;
    private static string? s_ffmpegPath;
    private static int s_durationSeconds = 30;

    public static int Run()
    {
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 10 — Real Recording Integration");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--output") s_outputPath = args[++i];
            else if (args[i] == "--ffmpeg") s_ffmpegPath = args[++i];
            else if (args[i] == "--duration" && int.TryParse(args[++i], out int d)) s_durationSeconds = d;
        }
        s_outputPath ??= "phase10_recording.mp4";
        s_ffmpegPath ??= FindFFmpeg() ?? "ffmpeg";

        Console.WriteLine($"  Output:     {s_outputPath}");
        Console.WriteLine($"  FFmpeg:     {s_ffmpegPath}");
        Console.WriteLine($"  Duration:   {s_durationSeconds}s");
        Console.WriteLine();

        if (SpikeSharedContext.Device == null || SpikeSharedContext.DuplicationDesc == null)
        {
            Console.WriteLine("  Phase 1-3 not yet run — auto-running...");
            int p1 = Phase1_DeviceTest.Run(); if (p1 != 0) { Console.Error.WriteLine("  FAIL: Phase 1."); return 1; }
            int p2 = Phase2_DesktopDuplication.Run(); if (p2 != 0) { Console.Error.WriteLine("  FAIL: Phase 2."); return 1; }
            int p3 = Phase3_TextureOwnership.Run(); if (p3 != 0) { Console.Error.WriteLine("  FAIL: Phase 3."); return 1; }
            Console.WriteLine();
        }

        var device = SpikeSharedContext.Device!;
        var duplDesc = SpikeSharedContext.DuplicationDesc!.Value;
        uint texW = duplDesc.ModeDescription.Width;
        uint texH = duplDesc.ModeDescription.Height;
        Console.WriteLine($"  Video: {texW}x{texH} H.264 NVENC");
        Console.WriteLine("  Audio: WASAPI Loopback → PCM → AAC (via FFmpeg)");
        Console.WriteLine();

        return RunRecording(device, texW, texH);
    }

    private static int RunRecording(ID3D11Device device, uint texW, uint texH)
    {
        string tempWav = Path.ChangeExtension(s_outputPath!, ".tmp.wav");
        string tempH264 = Path.ChangeExtension(s_outputPath!, ".tmp.h264");

        Console.WriteLine("[10.1] Setting up NVENC encoder...");
        using var nvenc = new NvEncFunctionTable();
        if (!nvenc.TryLoad()) { Console.Error.WriteLine("  FAIL: NVENC load."); return 1; }

        var sessionParams = new NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS
        {
            version = NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER,
            deviceType = NvEncodeAPI.NV_ENC_DEVICE_DIRECTX,
            device = device.NativePointer,
            reserved = IntPtr.Zero,
            apiVersion = NvEncodeAPI.NVENCAPI_VERSION,
            reserved1 = new uint[253],
            reserved2 = new IntPtr[64],
        };
        uint status = nvenc.OpenEncodeSessionEx!(ref sessionParams, out IntPtr encoder);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: OpenSession: {status}"); return 1; }

        var initParams = new NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS
        {
            version = NvEncodeAPI.MakeStructVersion(5) | (1u << 31),
            encodeGUID = NvEncodeAPI.NV_ENC_CODEC_H264_GUID,
            presetGUID = NvEncodeAPI.NV_ENC_PRESET_DEFAULT_GUID,
            encodeWidth = texW, encodeHeight = texH, darWidth = texW, darHeight = texH,
            frameRateNum = 30, frameRateDen = 1,
            enableEncodeAsync = 0, enablePTD = 1, bitFields = 0,
            privDataSize = 0, privData = IntPtr.Zero, encodeConfig = IntPtr.Zero,
            maxEncodeWidth = texW, maxEncodeHeight = texH,
            maxMEHintCountsPerBlockL0 = 0, maxMEHintCountsPerBlockL1 = 0,
            reserved = new uint[289], reserved2 = new IntPtr[64],
        };
        status = nvenc.InitializeEncoder!(encoder, ref initParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: Init: {status}"); return 1; }

        var bsParams = new NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER
        {
            version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER, size = 0, memoryHeap = 0, _padding = 0,
            bitstreamBuffer = IntPtr.Zero, reserved1 = IntPtr.Zero, reserved2 = IntPtr.Zero,
            reserved3 = new uint[226], reserved4 = new IntPtr[64],
        };
        status = nvenc.CreateBitstreamBuffer!(encoder, ref bsParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: BS: {status}"); return 1; }
        IntPtr bitstreamBuffer = bsParams.bitstreamBuffer;

        var texDesc = new Texture2DDescription
        {
            Width = texW, Height = texH, MipLevels = 1, ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm, SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default, BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CPUAccessFlags = CpuAccessFlags.None, MiscFlags = ResourceOptionFlags.None,
        };
        var encoderTexture = device.CreateTexture2D(texDesc);

        var regParams = new NvEncodeAPI.NV_ENC_REGISTER_RESOURCE
        {
            version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
            resourceType = NvEncodeAPI.NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX,
            width = texW, height = texH, pitch = 0, subResourceIndex = 0,
            resourceToRegister = encoderTexture.NativePointer, registeredResource = IntPtr.Zero,
            bufferFormat = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
            reserved1 = new uint[248], reserved2 = new IntPtr[62],
        };
        status = nvenc.RegisterResource!(encoder, ref regParams);
        if (status != NvEncodeAPI.NV_ENC_SUCCESS) { Console.Error.WriteLine($"  FAIL: Register: {status}"); return 1; }
        IntPtr registeredResource = regParams.registeredResource;
        Console.WriteLine("  PASS: NVENC ready.");
        Console.WriteLine();

        Console.WriteLine("[10.2] Creating DXGI Output Duplication...");
        IDXGIOutput? primaryOutput = null;
        int outIdx = 0;
        while (SpikeSharedContext.TargetAdapter!.EnumOutputs((uint)outIdx, out IDXGIOutput out_).Success)
        {
            if (outIdx == 0) primaryOutput = out_; else out_.Dispose();
            outIdx++;
        }
        var output1 = primaryOutput!.QueryInterface<IDXGIOutput1>();
        var duplication = output1.DuplicateOutput(device);
        output1.Dispose(); primaryOutput.Dispose();
        Console.WriteLine("  PASS: Duplication ready.");
        Console.WriteLine();

        Console.WriteLine("[10.3] Starting FFmpeg video pipe (raw H.264 → file)...");
        var videoFile = new FileStream(tempH264, FileMode.Create, FileAccess.Write);
        Console.WriteLine($"  Writing raw H.264 to: {tempH264}");
        Console.WriteLine();

        Console.WriteLine("[10.4] Starting WASAPI audio capture thread...");
        var audioCtx = new AudioContext();
        var audioThread = new Thread(() => AudioCaptureLoop(audioCtx, tempWav))
        {
            IsBackground = true,
            Name = "Phase10.Audio"
        };
        audioThread.Start();
        Console.WriteLine("  Audio thread started.");
        Console.WriteLine();

        Console.WriteLine($"[10.5] Recording for {s_durationSeconds}s...");
        Console.WriteLine();
        var deviceCtx = device.ImmediateContext;
        var sw = Stopwatch.StartNew();
        var duration = TimeSpan.FromSeconds(s_durationSeconds);

        long framesCaptured = 0, framesEncoded = 0, drops = 0, nvencErrors = 0;
        long totalVideoBytes = 0;
        long videoStartTicks = 0;

        try
        {
            while (sw.Elapsed < duration)
            {
                long frameStart = Stopwatch.GetTimestamp();
                if (videoStartTicks == 0) videoStartTicks = frameStart;

                var acq = duplication.AcquireNextFrame(100, out var frameInfo, out var dr);
                if (acq.Failure)
                {
                    if (acq.Code != Vortice.DXGI.ResultCode.WaitTimeout) drops++;
                    if (dr != null) dr.Dispose();
                    continue;
                }
                framesCaptured++;

                var dt = dr.QueryInterface<ID3D11Texture2D>();
                dr.Dispose();

                deviceCtx.CopyResource(encoderTexture, dt);
                duplication.ReleaseFrame();
                dt.Dispose();

                var mapParams = new NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE
                {
                    version = NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER,
                    subResourceIndex = 0, inputResource = registeredResource,
                    registeredResource = registeredResource, mappedResource = IntPtr.Zero,
                    mappedBufferFmt = 0, reserved1 = new uint[251], reserved2 = new IntPtr[63],
                };
                uint mapStatus = nvenc.MapInputResource!(encoder, ref mapParams);
                if (mapStatus != NvEncodeAPI.NV_ENC_SUCCESS) { nvencErrors++; drops++; continue; }
                IntPtr mappedInput = mapParams.mappedResource;

                try
                {
                    var picParams = new NvEncodeAPI.NV_ENC_PIC_PARAMS
                    {
                        version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
                        inputWidth = texW, inputHeight = texH, inputPitch = 0,
                        encodePicFlags = 0, frameIdx = 0,
                        inputTimeStamp = (ulong)(frameStart - videoStartTicks),
                        inputDuration = 0,
                        inputBuffer = mappedInput, outputBitstream = bitstreamBuffer,
                        completionEvent = IntPtr.Zero,
                        bufferFmt = NvEncodeAPI.NV_ENC_BUFFER_FORMAT_ARGB,
                        pictureStruct = 1, pictureType = 0, _padding1 = 0,
                        codecPicParams = new byte[1536],
                        meHintCountsPerBlock = new byte[32],
                        meExternalHints = IntPtr.Zero,
                        reserved1 = new uint[6], reserved2 = new IntPtr[2],
                        qpDeltaMap = IntPtr.Zero, qpDeltaMapSize = 0, reservedBitFields = 0,
                        meHintRefPicDist = new ushort[2], _padding2 = 0, alphaBuffer = IntPtr.Zero,
                        reserved3 = new uint[286], reserved4 = new IntPtr[59],
                    };
                    uint encStatus = nvenc.EncodePicture!(encoder, ref picParams);
                    if (encStatus != NvEncodeAPI.NV_ENC_SUCCESS) { nvencErrors++; drops++; continue; }

                    var lockParams = new NvEncodeAPI.NV_ENC_LOCK_BITSTREAM
                    {
                        version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER,
                        bitfields = 0, outputBitstream = bitstreamBuffer,
                        sliceOffsets = IntPtr.Zero, frameIdx = 0, hwEncodeStatus = 0,
                        numSlices = 0, bitstreamSizeInBytes = 0,
                        outputTimeStamp = 0, outputDuration = 0,
                        bitstreamBufferPtr = IntPtr.Zero,
                        pictureType = 0, pictureStruct = 0, frameAvgQP = 0, frameSatd = 0,
                        ltrFrameIdx = 0, ltrFrameBitmap = 0, reserved = new uint[13],
                        intraMBCount = 0, interMBCount = 0, averageMVX = 0, averageMVY = 0,
                        alphaLayerSizeInBytes = 0, reserved1 = new uint[218], reserved2 = new IntPtr[64],
                    };
                    uint lockStatus = nvenc.LockBitstream!(encoder, ref lockParams);
                    if (lockStatus != NvEncodeAPI.NV_ENC_SUCCESS) { nvencErrors++; drops++; continue; }

                    uint bsSize = lockParams.bitstreamSizeInBytes;
                    IntPtr bsPtr = lockParams.bitstreamBufferPtr;
                    if (bsSize > 0 && bsPtr != IntPtr.Zero)
                    {
                        byte[] buf = new byte[bsSize];
                        Marshal.Copy(bsPtr, buf, 0, (int)bsSize);
                        videoFile.Write(buf, 0, (int)bsSize);
                        totalVideoBytes += (int)bsSize;
                        framesEncoded++;
                    }

                    nvenc.UnlockBitstream!(encoder, bitstreamBuffer);
                }
                finally
                {
                    if (mappedInput != IntPtr.Zero)
                        try { nvenc.UnmapInputResource!(encoder, mappedInput); } catch { }
                }
            }
            sw.Stop();
        }
        finally
        {
            videoFile.Flush();
            videoFile.Dispose();
            Console.WriteLine();
            Console.WriteLine($"  Video capture complete: {framesEncoded} frames, {totalVideoBytes} bytes");
        }

        Console.WriteLine("[10.6] Stopping audio capture...");
        audioCtx.StopSignal = true;
        audioThread.Join(TimeSpan.FromSeconds(5));
        Console.WriteLine($"  Audio stopped. {audioCtx.TotalSamples} samples, {audioCtx.TotalBytes} bytes.");
        Console.WriteLine();

        Console.WriteLine("[10.7] Muxing video + audio → MP4...");
        string ffmpegArgs;
        if (audioCtx.TotalBytes > 0)
            ffmpegArgs = $"-y -f h264 -r 30 -i \"{tempH264}\" -i \"{tempWav}\" -c:v copy -c:a aac -b:a 192k -shortest \"{s_outputPath}\"";
        else
        {
            Console.WriteLine("  WARNING: Audio has 0 bytes — muxing video-only.");
            ffmpegArgs = $"-y -f h264 -r 30 -i \"{tempH264}\" -c:v copy \"{s_outputPath}\"";
        }
        Console.WriteLine($"  FFmpeg: {s_ffmpegPath} {ffmpegArgs}");
        var muxPsi = new ProcessStartInfo
        {
            FileName = s_ffmpegPath,
            Arguments = ffmpegArgs,
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        Process? muxProc = null;
        try { muxProc = Process.Start(muxPsi); }
        catch (Exception ex) { Console.Error.WriteLine($"  FAIL: Could not start FFmpeg: {ex.Message}"); return 1; }
        if (muxProc == null) { Console.Error.WriteLine("  FAIL: Could not start FFmpeg."); return 1; }
        string muxErr = muxProc.StandardError.ReadToEnd();
        muxProc.WaitForExit(30000);
        int muxRC = muxProc.ExitCode;
        Console.WriteLine($"  FFmpeg exit code: {muxRC}");
        if (muxRC != 0)
        {
            Console.Error.WriteLine($"  FFmpeg stderr (last 500 chars): {muxErr[^500..]}");
        }
        Console.WriteLine();

        Console.WriteLine("[10.8] Cleaning up NVENC...");
        try { if (registeredResource != IntPtr.Zero) nvenc.UnregisterResource!(encoder, registeredResource); } catch { }
        try { if (bitstreamBuffer != IntPtr.Zero) nvenc.DestroyBitstreamBuffer!(encoder, bitstreamBuffer); } catch { }
        try { encoderTexture.Dispose(); } catch { }
        try { nvenc.DestroyEncoder!(encoder); } catch { }
        try { duplication.Dispose(); } catch { }
        Console.WriteLine("  Cleanup done.");

        try { File.Delete(tempH264); } catch { }
        try { File.Delete(tempWav); } catch { }

        double elapsedSec = sw.Elapsed.TotalSeconds;
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" Phase 10 — Recording Report");
        Console.WriteLine("============================================================");
        Console.WriteLine($"  duration_seconds:        {elapsedSec:F3}");
        Console.WriteLine($"  frames_captured:         {framesCaptured}");
        Console.WriteLine($"  frames_encoded:          {framesEncoded}");
        Console.WriteLine($"  dropped_count:           {drops}");
        Console.WriteLine($"  nvenc_errors:            {nvencErrors}");
        Console.WriteLine($"  total_video_bytes:       {totalVideoBytes}");
        Console.WriteLine($"  audio_samples:           {audioCtx.TotalSamples}");
        Console.WriteLine($"  audio_bytes:             {audioCtx.TotalBytes}");
        Console.WriteLine($"  output_file:             {s_outputPath}");
        Console.WriteLine("  video_codec:             H.264 (NVENC)");
        Console.WriteLine("  audio_codec:             AAC (FFmpeg)");
        Console.WriteLine("  container:               MP4");
        FileInfo fileInfo = new(s_outputPath!);
        Console.WriteLine($"  file_size:               {fileInfo.Length} bytes ({fileInfo.Length / 1024.0 / 1024.0:F2} MB)");
        Console.WriteLine($"  file_exists:             {fileInfo.Exists}");
        Console.WriteLine("============================================================");

        if (framesEncoded > 0 && nvencErrors == 0 && fileInfo.Exists && fileInfo.Length > 0)
            Console.WriteLine("  Phase 10: PASS — recording produced.");
        else
            Console.WriteLine("  Phase 10: FAIL");
        Console.WriteLine();

        return (framesEncoded > 0 && fileInfo.Exists) ? 0 : 1;
    }

    private class AudioContext
    {
        public volatile bool StopSignal;
        public long TotalSamples;
        public long TotalBytes;
        public int SampleRate;
        public int Channels;
        public int BitsPerSample;
    }

    private static void AudioCaptureLoop(AudioContext ctx, string wavPath)
    {
        try
        {
            CoInitializeEx(IntPtr.Zero, 2);

            Guid clsid = CLSID_MMDeviceEnumerator;
            Guid iid = IID_IMMDeviceEnumerator;
            int hr = CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX_ALL, ref iid, out IntPtr pEnum);
            if (hr != 0) { Console.Error.WriteLine($"  Audio: CoCreateInstance failed: 0x{hr:X8}"); return; }

            try
            {
                var getDev = Marshal.GetDelegateForFunctionPointer<ImmDevice_GetDefaultAudioEndpoint>(
                    ComSlot(pEnum, 4));
                Console.WriteLine($"  Audio: pEnum = 0x{pEnum.ToInt64():x16}");
                Console.WriteLine($"  Audio: calling GetDefaultAudioEndpoint(eRender=0, eConsole=0)...");
                hr = getDev(pEnum, 0, 0, out IntPtr pEndpoint);
                Console.WriteLine($"  Audio: GetDefaultAudioEndpoint HRESULT = 0x{hr:X8}, pEndpoint = 0x{pEndpoint.ToInt64():x16}");
                if (hr != 0) { Console.Error.WriteLine($"  Audio: GetDefaultAudioEndpoint failed: 0x{hr:X8}"); return; }
                if (pEndpoint == IntPtr.Zero) { Console.Error.WriteLine($"  Audio: GetDefaultAudioEndpoint returned null endpoint"); return; }

                // Verify pEndpoint is a valid COM object by calling AddRef (slot 1) then Release (slot 2)
                var addRefFn = Marshal.GetDelegateForFunctionPointer<AddRefDel>(ComSlot(pEndpoint, 1));
                var relFn = Marshal.GetDelegateForFunctionPointer<RelDel>(ComSlot(pEndpoint, 2));
                uint refCount = addRefFn(pEndpoint);
                Console.WriteLine($"  Audio: IMMDevice AddRef OK (refCount={refCount})");
                // Don't release yet — the outer finally does it.

                    // Diagnostic: try QI for IAudioClient first to see if the object supports it at all
                    Guid qiIid = IID_IAudioClient;
                    var qiFn = Marshal.GetDelegateForFunctionPointer<QIDel>(ComSlot(pEndpoint, 0));
                    IntPtr qiResult;
                    int qiHr = qiFn(pEndpoint, ref qiIid, out qiResult);
                    Console.WriteLine($"  Audio: QI(IAudioClient) HRESULT=0x{qiHr:X8}, result=0x{qiResult.ToInt64():x16}");



                try
                {
                    Guid iidAudioClient = IID_IAudioClient;
                    var activate = Marshal.GetDelegateForFunctionPointer<ImmDeviceActivator_Activate>(
                        ComSlot(pEndpoint, 3));

                    // Allocate a zeroed PROPVARIANT (16 bytes on x64) — some drivers
                    // reject NULL pActivationParams. Passing a valid VT_EMPTY PROPVARIANT
                    // is the safe approach per MSDN.
                    IntPtr pPropVar = Marshal.AllocHGlobal(16); // sizeof(PROPVARIANT) on x64
                    for (int i = 0; i < 16; i++) Marshal.WriteByte(pPropVar, i, 0);

                    Console.WriteLine($"  Audio: pEndpoint = 0x{pEndpoint.ToInt64():x16}");
                    Console.WriteLine($"  Audio: IID_IAudioClient = {{{iidAudioClient}}}");
                    Console.WriteLine($"  Audio: CLSCTX = 0x{CLSCTX_ALL:X}");
                    Console.WriteLine($"  Audio: pPropVar = 0x{pPropVar.ToInt64():x16} (VT_EMPTY)");

                    hr = activate(pEndpoint, ref iidAudioClient, 1, pPropVar, out IntPtr pAudioClient);
                    Marshal.FreeHGlobal(pPropVar);

                    Console.WriteLine($"  Audio: Activate HRESULT = 0x{hr:X8}");
                    Console.WriteLine($"  Audio: pAudioClient = 0x{pAudioClient.ToInt64():x16}");

                    if (hr != 0)
                    {
                        string hrName = hr switch
                        {
                            unchecked((int)0x80004002) => "E_NOINTERFACE",
                            unchecked((int)0x80070057) => "E_INVALIDARG",
                            unchecked((int)0x80004005) => "E_FAIL",
                            unchecked((int)0x8007000E) => "E_OUTOFMEMORY",
                            _ => "UNKNOWN"
                        };
                        Console.Error.WriteLine($"  Audio: Activate failed: 0x{hr:X8} ({hrName})");
                        Console.Error.WriteLine($"  Audio: Diagnostic checklist:");
                        Console.Error.WriteLine($"    1. Is pEndpoint a valid IMMDevice? (should be non-zero)");
                        Console.Error.WriteLine($"    2. Is IID_IAudioClient correct? (1CB9AD4C-DBFA-4c32-B178-C2F568A703D2)");
                        Console.Error.WriteLine($"    3. Is CLSCTX_ALL correct? (0x17 = INPROC_SERVER|INPROC_HANDLER|LOCAL_SERVER)");
                        Console.Error.WriteLine($"    4. Is pPropVar valid? (should be VT_EMPTY PROPVARIANT)");
                        Console.Error.WriteLine($"    5. Was CoInitializeEx called on this thread? (should be COINIT_MULTITHREADED)");
                        Console.Error.WriteLine($"    6. Was GetDefaultAudioEndpoint successful? (HRESULT should be 0)");
                        Console.Error.WriteLine($"    7. Try calling pEndpoint->QueryInterface(IID_IAudioClient) to see if QI also fails");
                        return;
                    }

                    try
                    {
                        var getMix = Marshal.GetDelegateForFunctionPointer<AudioClient_GetMixFormat>(ComSlot(pAudioClient, 8));
                        hr = getMix(pAudioClient, out IntPtr pFormat);
                        if (hr != 0) { Console.Error.WriteLine($"  Audio: GetMixFormat failed: 0x{hr:X8}"); return; }

                        var wfx = Marshal.PtrToStructure<WAVEFORMATEX>(pFormat);
                        ctx.SampleRate = (int)wfx.nSamplesPerSec;
                        ctx.Channels = wfx.nChannels;
                        ctx.BitsPerSample = wfx.wBitsPerSample;
                        Console.WriteLine($"  Audio: {wfx.nChannels}ch {wfx.nSamplesPerSec}Hz {wfx.wBitsPerSample}bit (mix format)");

                        var init = Marshal.GetDelegateForFunctionPointer<AudioClient_Initialize>(ComSlot(pAudioClient, 3));
                        Guid audioSessionGuid = Guid.Empty;
                        hr = init(pAudioClient, AUDCLNT_SHAREMODE_SHARED, AUDCLNT_STREAMFLAGS_LOOPBACK,
                                 10000000, 0, pFormat, ref audioSessionGuid);
                        if (hr != 0) { Console.Error.WriteLine($"  Audio: Initialize failed: 0x{hr:X8}"); return; }

                        Guid iidCapture = IID_IAudioCaptureClient;
                        var getService = Marshal.GetDelegateForFunctionPointer<AudioClient_GetService>(ComSlot(pAudioClient, 14));
                        hr = getService(pAudioClient, ref iidCapture, out IntPtr pCapture);
                        if (hr != 0) { Console.Error.WriteLine($"  Audio: GetService failed: 0x{hr:X8}"); return; }

                        try
                        {
                            var start = Marshal.GetDelegateForFunctionPointer<AudioClient_Start>(ComSlot(pAudioClient, 10));
                            start(pAudioClient);

                            using var wav = new BinaryWriter(File.Create(wavPath));
                            WriteWavHeader(wav, ctx.SampleRate, ctx.Channels, ctx.BitsPerSample);

                            var getBuf = Marshal.GetDelegateForFunctionPointer<AudioCapture_GetBuffer>(ComSlot(pCapture, 3));
                            var relBuf = Marshal.GetDelegateForFunctionPointer<AudioCapture_ReleaseBuffer>(ComSlot(pCapture, 4));

                            while (!ctx.StopSignal)
                            {
                                Thread.Sleep(10);

                                hr = getBuf(pCapture, out IntPtr pData, out uint numFrames, out int flags,
                                            out long pos, out long qpcPos);
                                if (hr == 0 && numFrames > 0 && pData != IntPtr.Zero)
                                {
                                    int bytesToRead = checked((int)(numFrames * wfx.nBlockAlign));
                                    byte[] buf = new byte[bytesToRead];
                                    Marshal.Copy(pData, buf, 0, bytesToRead);
                                    wav.Write(buf, 0, bytesToRead);
                                    ctx.TotalSamples += numFrames;
                                    ctx.TotalBytes += bytesToRead;
                                    relBuf(pCapture, numFrames);
                                }
                            }

                            var stop = Marshal.GetDelegateForFunctionPointer<AudioClient_Stop>(ComSlot(pAudioClient, 11));
                            stop(pAudioClient);

                            wav.Flush();
                            wav.BaseStream.Seek(4, SeekOrigin.Begin);
                            wav.Write((int)(wav.BaseStream.Length - 8));
                            wav.BaseStream.Seek(40, SeekOrigin.Begin);
                            wav.Write((int)ctx.TotalBytes);
                            wav.Flush();
                        }
                        finally { Marshal.Release(pCapture); }
                    }
                    finally { Marshal.Release(pAudioClient); }
                }
                finally { Marshal.Release(pEndpoint); }
            }
            finally { Marshal.Release(pEnum); }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  Audio thread error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void WriteWavHeader(BinaryWriter w, int sampleRate, int channels, int bitsPerSample)
    {
        int blockAlign = channels * bitsPerSample / 8;
        int avgBytesPerSec = sampleRate * blockAlign;

        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(0);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)WAVE_FORMAT_PCM);
        w.Write((short)channels);
        w.Write(sampleRate);
        w.Write(avgBytesPerSec);
        w.Write((short)blockAlign);
        w.Write((short)bitsPerSample);
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(0);
    }

    private static IntPtr ComSlot(IntPtr obj, int slot)
    {
        IntPtr vtable = Marshal.ReadIntPtr(obj);
        return Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
    }

    private static string? FindFFmpeg()
    {
        string[] paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        foreach (var p in paths)
        {
            string exe = Path.Combine(p, "ffmpeg.exe");
            if (File.Exists(exe)) return exe;
            exe = Path.Combine(p, "ffmpeg");
            if (File.Exists(exe)) return exe;
        }
        return null;
    }
}
