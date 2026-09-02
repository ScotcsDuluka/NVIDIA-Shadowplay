# CBR Conformance Evidence — 2026-09-02

## FACT
- Machine: DESKTOP-DULUKA
- GPU: NVIDIA GeForce GTX 1080 Ti
- Display: \\.\DISPLAY1 1680x1050 @ 75Hz
- Production encoder config: H.264 / p4 / 60 FPS / CBR / 20,000,000 bps / GOP 60
- Native NVENC initialization: PASS; NV_ENC_CONFIG active size 3584 bytes
- RC parameters: rateControlMode=CBR (0x2), averageBitRate=20,000,000, maxBitRate=20,000,000, vbvBufferSize=40,000,000, vbvInitialDelay=40,000,000
- Contract tests: 52/52 PASS, including V-CT5c/V-CT5d CBR bitrate wiring
- Real production recording: Single.mp4, target 30s
- NVENC diagnostics: 1,703 encoded frames, 0 NVENC errors, 15.8 Mbps measured over 29.7s, configured 20.0 Mbps
- LiveMux: exit 0, video 58,849,603 B, audio 5,671,100 B, faststart=True, no orphan ffmpeg
- Session result: PASS; actual 30.02s; mux video duration 29.56s; A/V sync offset 0.203s; video+audio streams present
- FFprobe video stream: H.264 1680x1050, 1,703 frames, duration 28.405234s, reported bit_rate=16,574,298 bps

## INTERPRETATION
- The configured CBR parameters are reaching the native NV_ENC_CONFIG; there is no current evidence that 20 Mbps is silently replaced with VBR/CQ in the managed builder path.
- The measured encoded bitrate is materially below the configured 20 Mbps target on this 30s real desktop capture.
- This run does not close strict CBR conformance; it establishes a reproducible under-target observation while the end-to-end recording path remains healthy.

## UNKNOWN
- Whether the under-target average is expected NVENC CBR behavior for this content/session, or indicates a missing rate-control option/config detail, is not established by this run.
- A higher-complexity controlled-content run is still required before changing production RC semantics.

## NEXT VALIDATION
1. Run the same production path against controlled high-motion/high-entropy content for a longer window.
2. Compare NVENC measured bitrate and MP4 stream bitrate against the 20 Mbps target.
3. If still materially under target, inspect native RC mode variants and rate-control flags in an isolated spike before any production change.
