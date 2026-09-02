# CBR Conformance Evidence — Filler Padding — 2026-09-02

## FACT
- Machine: DESKTOP-DULUKA
- GPU: NVIDIA GeForce GTX 1080 Ti
- Display: DISPLAY1 1680x1050 @ 75Hz
- Production config: H.264 / p4 / 60 FPS / CBR / 20,000,000 bps / GOP 60
- Native NVENC init: PASS; explicit NV_ENC_CONFIG = 3584 bytes
- RC config: rateControlMode=CBR (0x2), averageBitRate=20,000,000, maxBitRate=20,000,000, VBV=40,000,000
- H.264 config: enableFillerDataInsertion=1 via first bitfield word at NV_ENC_CONFIG offset 168, bit 17 (0x20000)
- Real production session: Single.mp4, requested 30s
- NVENC diagnostics: 1,769 encoded frames, 0 NVENC errors, 19.7 Mbps measured over 29.9s
- LiveMux: exit 0, video 73,708,339 B, audio 5,758,844 B, dropped 0 B, faststart=True
- Session: PASS; actual 30.03s; mux 29.99s; A/V sync offset 0.054s; video+audio streams present; orphan ffmpeg baseline 0 -> final 0; engine Idle
- FFprobe video: H.264 1680x1050, 1,769 frames, duration 29.483922s, avg_frame_rate=707600000/11793569, bit_rate=19,999,992 bps

## INTERPRETATION
- Enabling H.264 filler-data insertion changed the observed 20 Mbps CBR run from materially under target to a stream bitrate of 19,999,992 bps (within 8 bps of target) while preserving end-to-end recording correctness.
- This is strong evidence that the prior under-target result was normal CBR behavior without filler padding, rather than evidence of the configured RC mode being silently replaced by VBR/CQ.
- The production recording path remained healthy in the same run: NVENC errors=0, LiveMux exit=0, dropped video bytes=0, video/audio streams present, engine returned to Idle.

## UNKNOWN / LIMITATION
- This closes the strict-CBR 20 Mbps acceptance on this hardware/configuration with one 30s real-production run; it does not characterize every GPU driver/content combination.
- NVENC diagnostic bitrate (19.7 Mbps) and container stream bitrate (19,999,992 bps) use different measurement windows/definitions, so they should not be treated as contradictory measurements.

## CHANGE
- CaptureEngine.Encoder.Nvenc/Internal/NvEncConfigSerializer.vb now sets the H.264 enableFillerDataInsertion bit when RC mode is CBR. VBR leaves the bit clear.
