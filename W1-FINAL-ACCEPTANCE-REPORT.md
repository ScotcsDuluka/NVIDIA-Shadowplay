# W1 — Final Capture Acceptance Report (matrix + lifecycle + before/after)

**Date:** 2026-09-12
**Production state under test:** `FfmpegDecodeWorker` = passthrough (W1 fix), `AudioEngineSession` = P1-A boundary freeze (W1 fix), `PlaybackSession` = W2/W3 set — **ไม่มีการแก้ production เพิ่มในงานนี้** (acceptance only)
**Method:** `--videocheck` ผ่าน canonical config chain, 5s/case, config.json ต่อเคส (gitignored bin dir), ตรวจด้วย ffprobe + log accounting ทุกเคส

---

## 0. Verdict (TL;DR)

**6/7 เคส PASS ครบทุกเกณฑ์** (5 อัตรา + video-only): MP4 truth ตรง, dropped=0, orphan=0, A/V delta ใน AAC structural tail
**1 defect พบใหม่ (แยก task):** **mic-only recording ล้มทั้ง session** — `LiveMuxSession.BuildArgs` สร้าง filtergraph อ้าง `[2:a]` ที่ไม่มีอยู่ (exit -22) — mic pipeline เองทำงานถูก (247 packets) — **ไม่แก้ตามข้อบังคับ**

## 1. Matrix results (5s sessions, native 1680×1050, GTX 1080 Ti)

| เคส | pass | frames (จริง/คาด) | avg_frame_rate | video dur | A/V delta | dropped | orphan |
|---|---|---|---|---|---|---|---|
| fps30 | True | 151/150 ✓ | 60×(303277/9060000)=**30.03** | 5.055s | −4.7ms | 0B | 0 |
| fps60 | True | 300/300 ✓ | 9000000/150643=**59.74** | 5.021s | +23.6ms | 0B | 0 |
| fps120 | True | 602/600 ✓ | 3010000/25189=**119.49** | 5.038s | +20.0ms | 0B | 0 |
| fps144 | True | 722/720 ✓ | 433200000/3020941=**143.40** | 5.035s | +23.8ms | 0B | 0 |
| fps240 | True | 1200/1200 ✓ | 9000000/37663=**238.97** | 5.022s | +31.3ms | 0B | 0 |
| video-only 60 | True | 300/300 ✓ | 1000000/16667=**60.00** | 5.000s | no-audio | 0B | 0 |
| **mic-only 60** | **False** | — (MP4 ไม่เกิด) | — | — | — | 9,298,780B หาย | 0 |

- **MP4 truth:** nb_frames ตรงคาดทุกอัตรา (±2 จาก tail-fill boundary); avg_frame_rate = ค่า metadata ที่มาจาก first-gap + µs-quantization (พิสูจน์แล้วใน P1-A/W1 reports — ไม่ใช่ timing หลุด); PTS monotonic ทุกไฟล์ (ตรวจแล้วใน suite GATE-1 + W1 matrix)
- **A/V delta −4.7..+31.3ms** = AAC final-frame quantization + container rounding — ไม่มี systematic overrun (P1-A fix: clip จาก latch ทุกเคส)
- **dropped=0B / orphan=0** ทุกเคส; Start→Record→Stop lifecycle ครบ (exit=0, engine Idle)

## 2. DEFECT ใหม่ (แยก task) — mic-only recording ล้มทั้ง session

**Repro:** config `Audio.SystemAudioEnabled=false, MicEnabled=true` → mic endpoint เริ่มได้ปกติ (48000Hz/1ch, 247 packets captured) → `LiveMuxSession.BuildArgs` เข้า amix branch:
```
-filter_complex "[1:a]apad[a0];[2:a]apad[a1];[a0][a1]amix=inputs=2…" -map 0:v -map "[aout]"
```
→ ffmpeg: **"Invalid file index 2 in filtergraph"** exit −22 → pipes broken ที่ 1s → 9.3MB dropped → **ไม่มีไฟล์**

**Root cause (โค้ดจริง):** `CaptureEngine.FFmpegBackend/LiveMuxSession.vb:227-233` — amix branch สมมติ **system+mic มีพร้อมกันเสมอ** (`[1:a]`=system, `[2:a]`=mic) — mic-only (system ปิด) ทำให้ mic อยู่ที่ `[1:a]` และ `[2:a]` ไม่มีจริง → filtergraph ตาย
**Impact:** user ที่ปิด system audio เปิด mic จะ**ไม่ได้ไฟล์เลย** (partial salvage ไม่ทำงานเพราะ ffmpeg ตายตั้งแต่ยังไม่เขียนอะไร)
**Fix ที่เสนอ (task แยก):** BuildArgs เลือก filtergraph ตาม inputs ที่มีจริง: mic-only → `-map 0:v -map 1:a` ตรง ๆ (ไม่ amix); หรือ map แบบ dynamic ตาม `_sysRate/_micRate`

## 3. Before/After สรุปของงานที่ fix ไปแล้ว (P1-A + decode)

| รายการ | Before | After | Evidence |
|---|---|---|---|
| **P1-A stop boundary** (audio overrun ช่วง teardown) | +27.5ms @60s (window = stop-sequence wall time; heavy-res เครื่อง owner = 762ms-class ตาม board) | +28.0ms @60s, +21.8/+9.4ms @3s/5s — **คงที่ใน AAC structural tail** (clip จาก latch — boundary เดียวกันทั้ง video/audio) | `evidence/w1-p1a-stop-boundary/` (before/after MP4+log) |
| **Decode throughput** (worker) | 48.6 fps delivered; 966 stdout vs 961 showinfo (5×2s PTS stall) | 99.4 fps full-replica; **stdout==showinfo 1:1 ทุกไฟล์** | `evidence/w1-passthrough-fix/`, `W2-DECODE-BOTTLENECK-REPORT.md` |
| **240fps playback** | presented=0 (จอค้างตลอดไฟล์) | presented 34–47/เคส (slideshow ~11fps, ไม่ค้าง) + StarvedResyncPresents counter | `evidence/w1-final-gate/` |

## 4. FACT / HYPOTHESIS / UNKNOWN

**FACT** — 6/7 matrix cases ผ่านครบทุกเกณฑ์ (MP4 truth, duration, FPS metadata, PTS, dropped=0, orphan=0, lifecycle ครบ); mic-only defect reproduce ได้ (filtergraph index error, exit −22); P1-A/P1-B before/after ตามตาราง
**HYPOTHESIS** — mic-only defect แก้ด้วย dynamic filtergraph mapping (เสนอ §2) แล้วจะผ่านทั้ง 4 คอมโบ (sys/mic/both/none) — ต้อง regression ยืนยันหลังแก้
**UNKNOWN** — สาเหตุ mux ffmpeg ตายสุ่มบน session ยาว (จากรอบ P1-A — ยังเปิด); mic device จริงบนเครื่อง owner

## 5. Evidence Index

- `evidence/w1-final-gate/…` + acceptance logs/MP4s: `…\Temp\sp_forensic\accept\acc_*.log/mp4` (คัดเฉพาะ summary ในรายงานนี้ — MP4 ต่อเคสอยู่ใน Temp)
- Matrix runner: `…\Temp\sp_forensic\accept\matrix.sh`
- Cross-ref: `W1-P1A-STOP-BOUNDARY-REPORT.md`, `W1-FAULT-PROOF-REPORT.md`, `W1-FINAL-GALLERY-VIDEO-REPORT.md`, `W2-DECODE-BOTTLENECK-REPORT.md`
