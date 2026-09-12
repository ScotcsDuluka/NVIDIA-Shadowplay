# W1 — `-fps_mode passthrough` Proof Report (headless Gallery harness, production path)

**Date:** 2026-09-12
**Branch:** `Engine-Rebuild-Stabilization` — **production source ไม่ถูกแตะแม้บรรทัดเดียว** (ผ่าน args-injection shim)
**Mission:** พิสูจน์ `-fps_mode passthrough` ใน `FfmpegDecodeWorker`: (1) stdout frames == showinfo frames (2) playback 240fps ต้องไม่ freeze — ก่อนแก้ production

---

## 0. Verdict (TL;DR)

| ข้ออ้าง | ผลพิสูจน์ |
|---|---|
| **(1) stdout frames == showinfo frames** | ✅ **PROVEN** — passthrough ให้ 1:1 ทุกไฟล์ (60/120/144/240); default(cfr) ให้ stdout > showinfo เสมอ |
| **(2) playback 240fps ไม่ freeze** | ❌ **REFUTED — passthrough เพียงอย่างเดียวไม่พอ** (ผ่าน production `PlaybackSession` จริง: presented = **0**, droppedLate = **961/961** — ยัง freeze) |
| ผลข้างเคียงดี | decode delivery **47.9 → 99.4 fps (2.08×)**, PTS-stall **10.4ms/เฟรม → 0.01ms**, ไม่มี 2s-stall |

**ตัวจริงที่ยังทำให้ freeze (structural, วัดค่าได้):** decode delivery ผ่าน BGRA-over-pipe (~99–120fps; swscale เดี่ยว ๆ ก็จำกัด 175fps) **< container 240fps** + late-drop policy (window ±6.25ms) ตัดทุกเฟรมตั้งแต่เฟรมที่ ~2 + **ไม่มีกลไก catch-up** → presented=0 ตลอดไฟล์ แม้ PTS จะสมบูรณ์แล้ว

**คำแนะนำต่อ board:** `-fps_mode passthrough` = **ถูกต้อง จำเป็น และไร้ความเสี่ยงต่อเนื้อหา** (เพียงแต่ยังไม่พอ) — แก้ได้เมื่ออนุมัติ พร้อมข้อ 2 ใน §4 ที่ต้องทำเพิ่มเพื่อ "240fps ไม่ freeze" จริง

---

## 1. วิธีพิสูจน์ (ไม่แตะ production)

**S-level (ffmpeg args ตรง):** นับ bytes บน stdout (÷7,056,000 = เฟรม) vs นับ `pts_time:` lines บน stderr

| ไฟล์ | default(cfr): stdout / showinfo | passthrough: stdout / showinfo |
|---|---|---|
| fps60-r1 | 242.0 / 241 | **241.0 / 241 ✓ MATCH** |
| fps120-r1 | 485.0 / 482 | **482.0 / 482 ✓ MATCH** |
| fps144-r1 | 580.0 / 577 | **577.0 / 577 ✓ MATCH** |
| fps240-r1 | 966.0 / 961 | **961.0 / 961 ✓ MATCH** |

→ default vsync ของ rawvideo muxer **สร้างเฟรมซ้ำหลัง filtergraph** (ไม่มี showinfo) ตาม drift ของ µs-quantized PTS; passthrough ตัดพฤติกรรมนี้ที่ต้นทาง

**B-level (replica ของ `VideoStdoutLoop`+`TakePtsTicks`+alloc/copy ทำงานจริง):**

| variant | fps | ms/frame | ptsWait avg | stalls |
|---|---|---|---|---|
| b2 = default + PTS sync | 54.8 | 18.26 | 10.41ms | **5 × ~2s** |
| **b2p = default + passthrough** | **120.7** | **8.29** | **0.01ms** | **0** |
| b3 = b2 + 7MB alloc/copy | 47.9 | 20.90 | 10.40ms | 5 × ~2s |
| **b3p = b3 + passthrough** | **99.4** | **10.06** | **0.00ms** | **0** |

**P-level (production `PlaybackSession` จริง — args injection ผ่าน shim):** harness ทำตัวเป็น `ffmpeg.exe` (รับ probe `-version` ให้ `FFmpegLocator.ProbeRuns` ผ่าน, แทรก `-fps_mode passthrough` ก่อน `-f rawvideo` เฉพาะ invocation ที่มี `-vf showinfo`, ส่งต่อ std handles ด้วย `CreateProcess + STARTF_USESTDHANDLES` แบบ zero-copy) → `PlaybackSession.Open/Play` ของ production รันต้นทางจริงทั้งหมด

| เคส | presented | droppedLate | Playing span (ไฟล์ 4.08s) |
|---|---|---|---|
| c2 default (อ้างอิงเดิม) | 0 | 966 | 19.9s |
| **p2 240fps + passthrough** | **0** | **961** | **15.4s** |
| p3 240fps + passthrough + hogs | 0 | 961 | 26.8s |
| p1 60fps + passthrough (ผ่าน shim) | 0 | 241 | 4.3s |

## 2. ทำไม (1) ถึงสำเร็จแต่ (2) ยังไม่ผ่าน — สลายเป็นตัวเลข

- หลังแก้: PTS สมบูรณ์ 1:1, worker ไม่มี 2s-stall, delivery ดีขึ้น 2× (99.4fps แบบรวม alloc/copy)
- แต่ 240fps ต้องการ delivery ≥ 240fps; เพดานจริง = **swscale yuv420→bgra ~5.7ms/เฟรม (175fps เพดานบน) + pipe ~3.8ms/เฟรม (~102-108fps วัดด้วย `| cat` และ .NET replica)** → ทำได้จริง ~99–120fps
- นาฬิกา (audio master) เดิน 1.00× wall = กิน PTS ที่ 4.17ms/เฟรม; เฟรมที่ k มาถึงตอน now ≈ k/99.4 → late ตั้งแต่ k≈2 เป็นต้นไป → late-policy drop **ทุก** เฟรม → presented=0 → จอค้าง
- ตรงกันข้ามที่ 60fps แบบเรียกตรง (c1): decode burst 120fps > 60fps → ทันกลับหลัง startup deficit → เล่นได้ 207/241 (เหลือ head-loss ~0.4s)
- p1 (60fps ผ่าน shim) freeze เพราะ shim เติม startup ~1s + ผ่าน pipe ชั้นเดียวทำ delivery ≈ file-rate (ไม่มี burst เหลือ) → deficit ไม่มีวันคืน — **เป็น artifact ของวิธีฉีด ไม่ใช่ของ passthrough** (production แก้ inline จะไม่มีต้นทุนนี้)

## 3. FACT / HYPOTHESIS / UNKNOWN

**FACT** — (1) 1:1 ทุกไฟล์ด้วย passthrough; 2× delivery; ศูนย์ stall; (2) ผ่าน production session จริง 240fps + passthrough ยัง presented=0/droppedLate=961 — freeze ยังอยู่; master clock เดินเกิน media end (15.4–26.8s)
**FACT** — เพดาน decode-delivery ปัจจุบัน: swscale 175fps, pipe ~102–108fps, รวม alloc/copy ~99fps — ทั้งหมด < 240fps
**HYPOTHESIS** — ทางเดียวที่ทำให้ 240fps "ไม่ freeze" ด้วยสถาปัตยกรรมปัจจุบันคือ late-policy แบบ degrade (starved → present เฟรมล่าสุด = slow-motion ไม่ใช่จอนิ่ง); ทางแก้ถาวรคือ decode ไม่ผ่าน BGRA pipe (D3D11VA + shared texture หรือ yuv420p+GPU convert)
**UNKNOWN** — ตัวเลขจริงบนเครื่องผู้ใช้ (CPU ช้ากว่าจะแย่กว่านี้); ผลของ `-fps_mode passthrough` ต่อไฟล์ non-native (HEVC ฯลฯ — อยู่หลัง format gate อยู่แล้ว)

## 4. สิ่งที่จะทำให้ "240fps ไม่ freeze" จริง (ตามลำดับ — ยังไม่ทำจนกว่าได้รับคำสั่ง)

1. **`-fps_mode passthrough` ใน `FfmpegDecodeWorker.Start`** (string เดียว) — จำเป็น: 1:1 frame↔PTS, จบ stall, delivery 2× — **พิสูจน์แล้วว่าไร้ผลข้างเคียงต่อเนื้อหา**
2. **Late-policy degrade mode ใน `PlaybackSession.RenderLoop`**: เมื่อ frame มาหลัง window แต่ queue ว่าง/starved → present เฟรมล่าสุด (นาฬิกายึด delivery) แทนการ drop จน presented=0 — เปลี่ยน "จอนิ่ง+เสียงเดิน" เป็น "วิดีโอเดินช้าลงตามกำลังเครื่อง" — ต้อง A/B ด้วย harness ชุดนี้ (เพิ่ม injection ได้ทันที)
3. **Master clock bound**: Playing ห้ามยืดเกิน media end (p3: 26.8s / c2: 19.9s สำหรับไฟล์ 4.08s)
4. **(ถ้าต้องการ 240fps จริง)** เปลี่ยน decode เป็น D3D11VA/shared-texture — ข้าม BGRA pipe ทั้งก้อน (เพดาน 9.5ms/เฟรม > 4.17ms budget แก้ไม่ได้ด้วย software path)

## 5. Evidence Index

- `evidence/w1-passthrough-proof/p{1,2,3}-log.txt + p{1,2,3}-poll.csv.gz` — timeline การเล่นจริงผ่าน production PlaybackSession (shim-injected passthrough)
- `evidence/w1-passthrough-proof/patch_pt.py / patch_shim3.py` — วิธีฉีดทั้งหมด (reproducible)
- ตัวเลข S/B: อยู่ในรายงานนี้ + `W2-DECODE-BOTTLENECK-REPORT.md` (evidence แถว S/B เดิม)
- Cross-ref: `W2-PLAYBACK-REPRO-REPORT.md` (baseline freeze), `W1-FPS-MATRIX-REPORT.md`
