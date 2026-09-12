# W2 — Decode-Worker Bottleneck Pinpoint Report: 988 fps → 48.6 fps

**Date:** 2026-09-12
**Branch:** `Engine-Rebuild-Stabilization` — production code ไม่ถูกแตะ (ทุกการวัดใช้ replica/args เดิม)
**Mission:** M1/W1 — หาคอขวดจริงใน `FfmpegDecodeWorker`: process stdout/stderr → frame extraction → BGRA conversion → copy → queue → synchronization

---

## 0. Verdict (TL;DR)

**คอขวดไม่ใช่จุดเดียว — เป็นบันได 5 ขั้นที่วัดค่าได้ครบ** โดยมี **2 ตัวหลัก**:

1. **🔴 muxer-level frame duplication ทำ showinfo-PTS ไม่ครบ → `TakePtsTicks` บล็อก 2s ต่อเฟรมที่ขาด** — ตัวการที่ทำให้ 55fps แทนที่จะเป็น ~100fps+ (5 เฟรม × 2s = **10 วินาทีต่อไฟล์ 4s**) และทำลายสัญญา 1:1 frame↔PTS ของ worker
2. **🟠 BGRA-over-pipe สถาปัตยกรรม**: 7.06MB/เฟรมผ่าน anonymous pipe = ขอบฟ้า ~102-108 fps พอดี ๆ (ไม่ว่า reader จะเร็วแค่ไหน) + worker alloc LOH 7MB + copy ต่อเฟรมอีก ~4ms

**ไม่ใช่ตัวการ:** FrameQueue (b4≈b3), showinfo parsing cost, RenderTickMs, stderr drain (thread แยกถูกต้องแล้ว)

---

## 1. บันไดคอขวด (ทุกขั้นวัดด้วย args เดียวกับ production)

| ขั้น | การวัด | fps | ms/frame | Δ ต่อเฟรม |
|---|---|---|---|---|
| 0 | `ffmpeg -f null` (decode เท่านั้น — เพดานที่ไม่เกี่ยวข้อง) | ~595–988* | 1.0–1.7 | — |
| 1 | **+ BGRA swscale** → `/dev/null` (S1: `-vf showinfo -f rawvideo -pix_fmt bgra pipe:1`) | 175.0 | 5.71 | +4.0ms |
| 1b | ย่อย: yuv420p แทน bgra = 309fps → **สเกล yuv420→bgra แพง ~2.5ms**; showinfo เอง ≈ 0.3ms | | | |
| 2 | **+ anonymous pipe จริงมีผู้อ่าน** (`pipe:1 \| cat > /dev/null` = 101.8; .NET replica อ่านแบบ worker = 108.3) | **~102–108** | 9.2–9.8 | **+3.5ms** |
| 3 | **+ TakePtsTicks** (Monitor sync กับ stderr thread — replica เป๊ะ) | **55.2** | 18.11 | **+8.9ms** ← ดู §2 |
| 4 | **+ alloc LOH 7MB + Array.Copy ต่อเฟรม** (replica ของ `OnCompleteFrame`) | **47.9** | 20.90 | +2.8ms (alloc+copy วัดรวม 3.97ms/เฟรม) |
| 5 | + FrameQueue(cap 4, consumer เร็ว) | 51.9 | 19.26 | ≈ 0 (b4≈b3) |
| — | **production จริง (c2):** 966 เฟรม / 19.9s | **48.6** | 20.6 | ✓ ตรง B3/B4 |

\* 988fps = ตัวเลขเดิมของผมด้วย `-f null`; วันนี้ได้ 595fps บนไฟล์เดิม (variance ระบบ ~2×) — ทั้งคู่เป็นเพดาน decode-lonly ไม่เกี่ยวกับเส้นทางจริง

**สรุปส่วนแบ่งเวลาต่อเฟรม (production):** BGRA swscale ~4.0ms + pipe ~3.5ms + alloc/copy ~4.0ms + **PTS-stall เฉลี่ย ~10.4ms** (รวมท้ายไฟล์) + อื่น ๆ ≈ 20.6ms = 48.6fps

---

## 2. ตัวการหลัก: showinfo ไม่ครบเพราะ **muxer duplicate เฟรมหลัง filtergraph**

หลักฐานชี้ขาด (นับจริงบน args จริง):

| โหมด | stdout frames (bytes ÷ 7,056,000) | showinfo lines (stderr) |
|---|---|---|
| default (production) | **966.0** | **961** |
| `-fps_mode passthrough` | **961.0** | **961** ✓ 1:1 |
| `-fps_mode cfr` | 966.0 | 961 |

- **default vsync ของ rawvideo muxer = CFR** → เมื่ออัตราจริงเฉลี่ยของไฟล์ (µs-quantized, avg ≈ 238.29 @240fps) ต่ำกว่าเป้า มัน**สร้างเฟรมซ้ำที่ระดับ muxer — หลัง filtergraph — จึงไม่มี showinfo line** (จำนวนสเกลกับ fps: 60fps +1, 120 +3, 144 +3, 240 +5 ต่อไฟล์ ~4s)
- Worker สมมติ 1:1 (`OnCompleteFrame` ↔ 1 showinfo line): เฟรมที่ 962–966 จึงไม่มี PTS มาเลย →
  `TakePtsTicks` รอ **ครบ deadline 2s ต่อเฟรม** (`Monitor.Wait(_ptsLock, ≤50ms)` วนจนครบ 2s → extrapolate) — log จาก replica:
  ```
  STALL frame=962 wait=2008ms  … frame=966 wait=2003ms  (stderrLines=961, ptsQ=0)
  frames with PTS already queued at completion: 961/966
  ```
- **ขณะ stdout thread บล็อก 2s ไม่อ่าน pipe → ffmpeg โดน backpressure → ทั้งสายการผลิตหยุด** → เฉลี่ยทับ = −10.4ms/เฟรม; และช่วง Playing ยืดออก (สอดคล้อง c2: 19.9s)

---

## 3. กลไกเชื่อมโยงกับอาการ (จาก phase-1 reproduction)

- sustained read (~102–108fps ceiling) < 240fps → ทุกเฟรมมาถึงเมื่อ PTS ตกหลังนาฬิกาเกิน window (±6.25ms) → late-drop 100% → **freeze** (c2: presented=0)
- PTS-stall บล็อก pipe → delivery rate ที่เห็นเฉลี่ย 48.6/s + Playing ยืดเป็น ~20s
- ที่ 60fps: ceiling ~100fps > 60 → ปกติเล่นได้ (c1); ใต้โหลด delivery ตกต่ำกว่า 60 → freeze (c6)

## 4. ทางแก้ตามลำดับความคุ้ม (HYPOTHESIS — รอคำสั่ง ยังไม่แก้)

1. **`-fps_mode passthrough` ใน video decode args** (แก้ที่ string เดียวใน `FfmpegDecodeWorker.Start`): จบ duplication → 1:1 frame↔PTS → จบ stall 10s; playback ใช้ PTS จริงของเฟรม (ถูกต้องกว่า CFR-duplicate สำหรับ playback domain)
2. **อย่าบล็อก pipe reader กับ PTS**: ลด deadline (2s → ~1-2 frame periods) หรือย้ายการจับคู่ PTS ออกจาก read loop (extrapolate ก่อน แก้ทีหลัง) — ทำให้ stall ไม่ย้อนกลับไปกด ffmpeg
3. **ลด alloc/copy**: buffer pool แทน `New 7MB` ต่อเฟรม (~−4ms/เฟรม)
4. **สถาปัตยกรรมระยะยาว**: BGRA-over-pipe จำกัดที่ ~105fps @1680×1050 โดยธรรมชาติ — ถ้าต้องการ >120fps playback ต้องเปลี่ยนช่องทาง (เช่น ส่ง h264 เข้า D3D11 decoder, หรือ shared texture) — ใหญ่ ไม่จำเป็นตอนนี้

## 5. FACT / HYPOTHESIS / UNKNOWN

**FACT** — ทุกตัวเลขใน §1 (S-series, B-series, passthrough/cfr, byte-count, stall log); default vsync ของ rawvideo muxer duplicate 5 เฟรม/966 บนไฟล์ 240fps และเฟรมซ้ำไม่มี showinfo; 5×2s = 10s stall; queue ไม่ใช่คอขวด; alloc+copy = 3.97ms/เฟรม
**FACT** — pipe ceiling ~102–108fps สำหรับ 7.06MB/เฟรม (2 วิธีวัดอิสระ: `| cat` และ .NET replica)
**HYPOTHESIS** — ลำดับทางแก้ §4 ตามความคุ้ม; `-fps_mode passthrough` จะยุบ stall ทั้งหมด (ต้องทดสอบยืนยันหลังได้รับอนุญาตแก้)
**UNKNOWN** — สมการภายใน ffmpeg ที่เลือกจุด duplicate (rate reconciliation ของ µs-quantized PTS); พฤติกรรมกับไฟล์อื่น (สรุปแนวโน้มแล้ว: +1/+3/+3/+5)

## 6. Evidence Index

- `evidence/w2-decode-bottleneck/b2_final.txt` — stall log + wait distribution จาก replica ตัวสุดท้าย
- `evidence/w2-decode-bottleneck/sinfo_passthrough.txt / sinfo_cfr.txt` — showinfo stderr ดิบ (961 lines ทั้งคู่)
- `evidence/w2-decode-bottleneck/patch_*.py` — สคริปต์ที่เพิ่ม BenchWorker/histogram/stall-log ให้ harness
- Harness พร้อม bench: `evidence/w2-playback-repro/Program.cs` (อัปเดตแล้วใน working dir ของ harness)
- Cross-ref: `W2-PLAYBACK-REPRO-REPORT.md` (ผล phase-1), `W2-FREEZE-JUMP-REPORT.md`
