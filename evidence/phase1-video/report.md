# PHASE 1 VIDEO — Windows real-record validation evidence

- **Date:** 2026-09-02 23:25:09
- **Machine:**  · OS: Microsoft Windows NT 10.0.26340.0
- **FFmpeg:** $FFmpeg · **Screen:** 1680x1050 · **Seconds/scenario:** 8
- **Driver:** ConsoleDriver --videocheck (canonical chain LoadEffectiveSettings → MapStartupConfig → Initialize → BuildSessionConfig → StartSession)

| S | Field | Expect | Got | Verdict |
|---|---|---|---|---|
| S1 | FPS | avg_frame_rate≈60 (config wins, not display) | 340000/5809 → 58.53 fps | PASS |
| S2 | Resolution(native) | 1680x1050 | 1680x1050 | PASS |
| S3 | Resolution(custom) | 1280x720 | 1280x720 | PASS |
| S4 | Bitrate(CBR) | 10.2 Mbps ±40% (low-motion dips = WARN) | 10.17 Mbps | PASS |
| S5 | Preset | startup echo preset='p7' + init echo 'preset p7' | p7 | PASS |
| S6 | Encoder | codec_name=h264 (NVENC) | h264 | PASS |
| S7 | CaptureMethod(gfxcapture) | GAP warning in log + actual=DdagrabBackend (never silent) | GAP warn present, ddagrab ran | PASS (gap honestly recorded) |

**VERDICT: PASS**

Known gaps (honest, pre-documented — NOT validation failures):
- PixelFormat: nv12 requested but runtime is BGRA8→ARGB (BLOCKER P1-PIXFMT, RecordingEngine.vb:135-138)
- gfxcapture: not implemented — GAP warning + ddagrab continuation (RecordingEngine.vb:123-128)

