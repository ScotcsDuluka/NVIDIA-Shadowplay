# W1 — Production Fix Regression Report: `-fps_mode passthrough` in FfmpegDecodeWorker

**Date:** 2026-09-12
**Branch:** `Engine-Rebuild-Stabilization`
**Scope:** production `Gallery.Video/FfmpegDecodeWorker.vb` เติม `-fps_mode passthrough` ก่อน `-f rawvideo` (ตำแหน่ง output-option ที่ถูกต้อง) + contract comment; **ไม่แตะ** PlaybackSession (W2) / liveness (W3)
**Status:** production change APPLIED + regression PASS (ยังไม่ commit — รอคำสั่ง)

---

## 1. The change (git diff ตรงขอบเขต)

`Gallery.Video/FfmpegDecodeWorker.vb`:
- **1 functional line** — video decode args เดิม:
  `-i "<file>" -map 0:v:0 -vf showinfo -f rawvideo -pix_fmt bgra pipe:1`
  → ใหม่:
  `-i "<file>" -map 0:v:0 -vf showinfo -fps_mode passthrough -f rawvideo -pix_fmt bgra pipe:1`
- contract comment ที่ header อัปเดตให้ตรง (เหตุผล + ตัวเลขหลักฐาน)
- audio args / ทุกอย่างอื่น **ไม่เปลี่ยน** (ทราบ: `Overlay/build.txt` เป็นการแก้ของ worker อื่น ไม่เกี่ยวกับงานนี้)

## 2. Regression A — stdout/showinfo = 1:1 (args จริงของ production หลังแก้)

| ไฟล์ | stdout frames | showinfo lines | ผล |
|---|---|---|---|
| fps60-r1 | 241.0 | 241 | **1:1 PASS** |
| fps120-r1 | 482.0 | 482 | **1:1 PASS** |
| fps144-r1 | 577.0 | 577 | **1:1 PASS** |
| fps240-r1 | 961.0 | 961 | **1:1 PASS** (7.06 ms/frame ≈ 141 fps ffmpeg-side) |

(เทียบก่อนแก้: default(cfr) ให้ stdout 242/485/580/966 เฟรม vs showinfo 241/482/577/961 — เฟรมซ้ำไม่มี PTS)

## 3. Regression B — production playback battery (production DLL ใหม่, PlaybackSession จริง)

| เคส | presented | delivered (droppedLate) | Playing span | ผล |
|---|---|---|---|---|
| c1 60fps | 213 | 28 | ~4.3s | เล่นได้ ✓ (เทียบเดิม 207/25) |
| **c2 240fps** | 0 | **961 (ครบทุกเฟรม)** | **11.38s** (เดิม 19.9s → **−43%**) | **ไม่มี tail stall** ✓ |
| c3 240 tick2 | 0 | 961 | 11.48s | ✓ |
| c4 240 queue1 | 0 | 961 | 11.47s | ✓ |
| c5 240 + hogs | 0 | 961 | 14.44s | ✓ (decode ช้าตามโหลด — สมเหตุผล) |
| c6 60 + hogs | 0 | 241 | 4.92s (เดิม 6.75s) | ✓ |
| c7 240 seek | 0 | 131 (gen=2) | — | ✓ |

**ไม่มี ptsWait tail stall** — ตรวจ timeline ทุกเคส: flat-gap (dropped ค้าง ≥1.5s) = **NONE** ทั้ง c2/c5
(เทียบก่อนแก้: 5 flat-gaps ~2s ที่ 9.1/10.9/12.9/14.9/16.9s จาก 961→966 เฟรม mismatch)

**delivery ต่อเนื่องหลังแก้ ≈ 961/11.38s ≈ 84 fps** — ตรงสูตร B-replica (pipe 9.2ms + alloc/copy 4.0ms ≈ 13.2ms → 75-84fps) ✓

## 4. ขอบเขตที่เหลือ (ให้ W2/W3 — อยู่นอก scope งานนี้)

- **presented = 0 ที่ 240fps ยังค้าง** — นี่ไม่ใช่ decode แล้ว: decode ส่งครบ 961/961 ตามเวลา; ตัวการที่เหลือคือ late-drop policy (±6.25ms window) × delivery ~84fps < 240fps + master clock domain (Playing เดินเกิน media end: c2 11.4s / c5 14.4s บนไฟล์ 4.08s) = **W2 (audio clock domain) และ policy ฝั่ง RenderLoop**
- Head-loss ~0.3-0.4s ตอนเปิดคลิป (startup race) — ฝั่ง RenderLoop เช่นกัน

## 5. FACT / สรุป

**FACT** — stdout/showinfo 1:1 ทุกอัตราหลังแก้ (A); production worker ส่งครบ 961/961 ทุกเคสโดยไม่มี flat-gap (B); span 240fps ลด 43%; 60fps เล่นได้เท่าเดิม; EOS สะอาดทุกเคส; diff = 1 functional line + comment
**ข้อจำกัด** — presented=0 @240fps ยังไม่หาย (คาดหมายไว้ตั้งแต่ W1-PASSTHROUGH-PROOF: passthrough จำเป็นแต่ไม่พอ — ส่วนที่เหลือเป็นของ W2/W3)

## 6. Evidence

- `evidence/w1-passthrough-fix/c{1..7}-log.txt + c{1..7}-poll.csv.gz` — production battery timeline หลังแก้
- `git diff Gallery.Video/FfmpegDecodeWorker.vb` — the change (uncommitted)
- Cross-ref: `W1-PASSTHROUGH-PROOF-REPORT.md`, `W2-DECODE-BOTTLENECK-REPORT.md`, `W2-PLAYBACK-REPRO-REPORT.md`
