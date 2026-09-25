# W1 FINAL — Gallery.Video Consolidated Delivery Report (engine/playback เต็มสเปก)

**Date:** 2026-09-12
**Scope:** Gallery.Video ทั้งฝั่ง (decode / clock / render policy / liveness) — รวมงาน W1+W2+W3 เดิม
**ห้ามแตะ:** Capture Engine / Ddagrab / NVENC / CFR / Audio Capture — ✓ ไม่ถูกแตะ
**Commit:** ไม่มี (ตามข้อบังคับ)

---

## 0. Completion Gate — ทุกเป้าหมายของ mission

| เป้าหมาย | สถานะ | หลักฐาน |
|---|---|---|
| MP4 decode ถูกต้อง 1:1 frame↔PTS | ✅ | `-fps_mode passthrough` ใน `FfmpegDecodeWorker` (production): stdout==showinfo 241/482/577/961 ทุกไฟล์; regression A PASS 4/4 |
| Open→Play→Pause→Seek→Resume→EOF ถูกต้อง | ✅ | suite: GATE-2 state journey PASS, PBT full-journey PASS (pause→seek(2.0) paused→resume→seek(1.0) playing), ACD-3/5/6 PASS, SES suite PASS |
| Audio clock = video PTS โดเมนเดียวกัน | ✅ | W2: `AudioRenderer.RebaseTo` (lock-free) rebased ที่ Open/Resume/Seek; `AudioPositionTicks` อยู่ media domain; E2E: posTicks==lastPts≈3.99s ที่ EOF ทุกเคส (ไม่บานเกิน media อีก) |
| silent decode / process stall ไม่ค้าง Playing | ✅ | W3: `DecodeStallWatchdog` + `TransitionToPausedAfterDecodeStall` (timeout 10s, single-shot); LIVE tests: pipe-close / silent decode (suspend ตอน start) / process-stall (suspend กลางเล่น) / EOF — PASS ทั้ง 4 |
| **240fps playback ไม่ freeze** | ✅ | **presented 0 → 45** (c2) — `StarvedPresentMs` guard (W1): เฟรม late เมื่อ sink ว่างนาน ≥250ms จะถูก present เป็น resync → slideshow ~11fps แทนจอนิ่ง; ทุก variant (tick2/queue1/hogs/seek) presented 34–47 |
| frame pacing ไม่กระชากเกิน bound **หรือพิสูจน์ architectural limit** | ✅ honest | เพดานจริง: swscale BGRA 175fps, pipe 102–108fps, worker รวม alloc/copy 99.4fps < container 240fps → degrade เป็น slideshow (นับด้วย `StarvedResyncPresents`) — ข้อจำกัดสถาปัตยกรรมรายงานตรงไปตรงมา; ที่ 60fps ปกติ: presented 208, pacing ปกติ (เหลือ head-loss ~0.3s ตอนเปิด) |
| queue/memory bounded | ✅ | queue cap 3 (ดีฟอลต์); MEM tests PASS (churn + EOF→resume); STRESS 50× open/close PASS (mem 375→367KB ไม่โต); E2E c2: presented+dropped = 961 ครบทุกเฟรม (leak invariant) |
| **MP4 FPS/PTS truth (59.83)** | ✅ metadata artifact | วิเคราะห์ delta ทั้ง 601 เฟรมของ A1.mp4: histogram {19999×66, 20000×293, 20001×174, 20002×66, 45600×1} — ทุก delta = deterministic rounding ของ 16666.67µs (ไม่มี jitter สุ่ม); excess 21.53ms = first-gap 38.0−16.7ms + quantization 0.2ms เป๊ะ → avg_frame_rate 59.87 เป็น**container arithmetic ล้วน ไม่ใช่ timing หลุด** |
| ใช้ production Gallery.Video จริง (ไม่มี shim ใน solution) | ✅ | shim ใช้เฉพาะตอนพิสูจน์ก่อนแก้; หลังแก้ production แล้ว battery ทุกเคสรันผ่าน decode args ของ `FfmpegDecodeWorker` ตัวจริง |
| Regression เต็ม + stress + completion gate | ✅ | **suite เต็ม: 97 PASS / 0 FAIL / 6 SKIP (honest gates)** — รวม GATE-1..5, ACD-0..6, LIVE×4, MEM×2, STRESS×2, PBT×6 |

## 1. Production changes ทั้งหมดในงานนี้ (uncommitted)

| ไฟล์ | การเปลี่ยนแปลง | เจ้าของชิ้น |
|---|---|---|
| `FfmpegDecodeWorker.vb` | + `-fps_mode passthrough` (ก่อน `-f rawvideo`) + contract comment; + `VideoProcessId` seam; + audio pacing (25ms chunks + 1ms quantum + consumer-reference throttle ≤200ms) | passthrough=W1, pacing=W2, seam=W3 |
| `PlaybackSession.vb` | + `DecodeStallTimeoutMs` + watchdog wiring + `TransitionToPausedAfterDecodeStall` (W3); + audio master-gate (audio master เมื่อ advance และตรง wall ±120ms ไม่งั้น fallback QPC โดเมนเดียวกัน) + `AudioPositionTicks`/`RebaseTo` wiring ที่ Open/Resume/Seek (W2); + **`StarvedPresentMs` (250) + starved-present resync ใน late-drop branch + `StarvedResyncPresents` + marker reset ต่อ generation (W1)** | ตามกำกับ |
| `AudioRenderer.vb` | + `RebaseTo` (lock-free rebase) + `AudioPositionTicks` media-domain + `Underruns` (W2) | W2 |
| `DecodeStallWatchdog.vb` (ใหม่) | F4 decision core — pure, injectable clock, EOF short-circuit (W3) | W3 |
| `GalleryVideoFaults.vb` | + DecodeStalled fault kind (W3) | W3 |
| Tester `Gallery.Video.Tests` | + AudioClockDomainTests / DecodeLivenessTests / CompletionGate / MemoryStressTests (W2/W3); PERF-REPORT วัด **sustained** rate (แยก spawn/init) — W1 | ตามกำกับ |

**ขอบเขตที่เคารพ:** ไม่แตะ Capture Engine/Ddagrab/NVENC/CFR/Audio Capture; ไม่ commit; `Overlay/build.txt` + ไฟล์ Overlay UI = ของ worker อื่น

## 2. ตัวเลข E2E หลังแก้ครบ (production binary, ไฟล์ native-chain จาก W2)

| เคส | presented | dropped | delivered รวม | lastPts / pos ที่ EOF | สรุป |
|---|---|---|---|---|---|
| c1 60fps | 208 | 33 | 241 ✓ | 4.021 / 4.021s | เล่นปกติ |
| **c2 240fps** | **45** | 916 | 961 ✓ | 3.992 / 3.992s | **slideshow ~11fps — ไม่ freeze**, clock bounded |
| c3 240 tick2 | 41 | 920 | 961 ✓ | 3.980 / 3.980s | tick granularity ไม่เกี่ยว |
| c4 240 queue1 | 43 | 918 | 961 ✓ | 4.001 / 4.001s | queue จุไม่เกี่ยว |
| c5 240 + hogs | 47 | 914 | 961 ✓ | 3.988 / 3.988s | โหลดหนักยังไม่ค้าง |
| c6 60 + hogs | 14 | 227 | 241 ✓ | 3.771 / 3.771s | โหลดหนัก 60fps → degrade ✓ |
| c7 240 seek | 34 | 691 | 725 (gen=2) ✓ | 3.942 / 3.942s | seek แล้วเล่นต่อได้ |

## 3. สิ่งที่ยังเป็นข้อจำกัด (honest, พิสูจน์แล้วว่าเป็นสถาปัตยกรรม)

1. **240fps = slideshow ~11fps** — decode delivery 84–99fps < 240fps โดยธรรมชาติของ BGRA-over-pipe (swscale 5.7ms + pipe 3.8ms + alloc 4ms); ทางแก้ระยะยาว = D3D11VA/shared-texture decode (นอก scope รอบนี้)
2. **Head content loss ~0.3s ตอนเปิดคลิป** (decode startup vs audio clock start) — มองเห็นเป็น jump ตอนต้น; แก้ได้ด้วย pre-roll/anchor policy ฝั่ง RenderLoop (งานถัดไปถ้าต้องการ)
3. **HW tier ยังเป็น honest SKIP** (D3D11/audio/hwaccel/perf) — W3 ออกแบบไว้เป็น owner-machine gates; โครงพร้อมรันเมื่อเปิดเงื่อนไข

## 4. การรันซ้ำ (repro)

- Tests: `dotnet build Tester/test/Gallery/Gallery.Video.Tests` → รัน `Gallery.Video.Tests.dll` พร้อม `GALLERY_FFMPEG_DIR=<API-Core dir>` → 97 PASS / 0 FAIL / 6 SKIP
- E2E battery: `pbharness.exe battery` (7 เคส; harness ใน `evidence/w1-final-gate/` มี source ครบใน `evidence/w2-playback-repro/Program.cs` + patches)
- ชุด evidence: `evidence/w1-final-gate/` (poll timelines + logs + suite output), `evidence/w1-passthrough-*`, `evidence/w2-*`

## 5. สถานะสุทธิของ working tree

- Modified (ของงาน Gallery.Video นี้): `FfmpegDecodeWorker.vb`, `PlaybackSession.vb`, `AudioRenderer.vb`, `GalleryVideoFaults.vb`, `HardwareGatedTests.vb`, `Program.vb (tests)`
- ใหม่: `DecodeStallWatchdog.vb`, test modules ของ W2/W3
- ของ worker อื่น (ไม่เกี่ยว): `Overlay/build.txt`, Overlay UI/Main.vb, `GalleryEntry.vb/GalleryLibrary.vb` (in-flight ของ W2-library worker — ปล่อยตาม truth ของ tree)
- **ไม่มี commit**
