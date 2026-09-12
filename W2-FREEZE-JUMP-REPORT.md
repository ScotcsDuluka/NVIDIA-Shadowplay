# W2 — Runtime Freeze/Jump Reproduction Report (Native NVIDIA chain, 60/120/144/240)

**Date:** 2026-09-12
**Branch:** `Engine-Rebuild-Stabilization` (HEAD `cca2b2d8c2`) — production code/timing ไม่ถูกแตะ, ไม่มี commit
**Question:** อาการ freeze / jump / fast playback / stale visual เกิดที่ stage ไหน:
Ddagrab → timestamp → queue → CFR selection → duplicate/reuse → NVENC → mux หรือ playback/decode

---

## 0. Verdict (TL;DR)

**อาการ user-scale ไม่ reproduce ในไฟล์บันทึกผ่าน native chain เลย** (แม้แต่ไฟล์เดียวใน 8 runs) —
พิสูจน์ด้วย **burned-in clock**: app วาดสี 12 ค่า (300ms/สี) + progress bar (ความละเอียด ~0.25ms) บนจอ vsync-aligned
(p50 13.32ms = 75Hz) → อัดด้วย production chain → decode อ่าน clock กลับจาก**ทุกเฟรม** → เทียบ content-time กับ PTS-time

| อาการ | ผลการ reproduce ในไฟล์ | ข้อสรุป stage |
|---|---|---|
| fast playback | **ไม่เจอ** — slope 0.987–0.991 ทุกไฟล์ (เนื้อหาเล่น ~99% ของ real-time) | ไม่ใช่ pipeline |
| jump | **ไม่เจอ user-scale** — ใหญ่สุด 20–34ms = 1 vsync quantum (13.3ms) ต่อไฟล์ 0–2 ครั้ง | sub-perceptual |
| freeze/stale | **เจอแค่ช่วง head 69.9–79.7ms ครั้งเดียวต่อไฟล์** (เฟรม #0 คือ warm-up frame + start transient — mechanism จาก M1-W1) | capture/CFR head, ≤80ms, one-time |
| การซ่อน/ขยับ PTS | PTS สมบูรณ์: dup=0, neg=0, monotonic violations=0, P95 = คาบเฟรมเป๊ะ ทุกไฟล์ | mux สะอาด |

**กลุ่มต้องสงสัยที่เหลือ = ฝั่ง playback/decode** — code-reading พบ mechanism ตรงอาการ 2 ตัวใน `Gallery.Video/PlaybackSession.vb` RenderLoop (§3) พร้อมตัวนับยืนยัน (`DroppedLateFrames`) — ยังไม่รัน reproduction เพราะต้องเขียน harness (ต่อไปนี้ §5)

---

## 1. Method (measured, not inferred)

1. **Source-clock app** (`clockapp.ps1`, WPF): หน้าต่างที่ (0,0) 1300×320 (บังคับด้วย Win32 `SetWindowPos` หลังจากพบว่า Windows ขยับหน้าต่างเอง) — swatch สี solid เปลี่ยนทุก 300ms (12 สีหมุนวง) + bar กว้างตาม `(t mod 300ms)/300` + log ทุก render tick พร้อม QPC → **ground truth ของ content-time**
2. **Native production chain** เดียวกับ W1 (`--videocheck` → Ddagrab + NVENC + LiveMux), fps ผ่าน config chain, 4 วินาที × 2 runs × 60/120/144/240 = **8 recordings** ทั้งหมด EXIT=0, pass=True, config ทดสอบถูกลบหลังจบ
3. **Read-back**: ffmpeg decode crop → จำแนกสี swatch (12 palette) + หาขอบขวาของ bar (white-run scan) ต่อเฟรม → unwrap เป็น src-time → คำนวณ:
   - **slope** = Δcontent / ΔPTS (1.0 = เล่นตรงความเร็ว)
   - **jump** = content เดินหน้าเกิน PTS เกิน 20ms
   - **stall** = content นิ่ง ≥40ms ของ PTS time
   - PTS cadence ทุกเฟรม

## 2. Timing Table (8/8 runs)

| ไฟล์ | slope | jumps >20ms (max) | stalls >40ms (max @frame) | PTS first | PTS P95 | dup/neg/mono | NVENC err |
|---|---|---|---|---|---|---|---|
| fps60-r1  | 0.9907 | 0 | 0 | 38,000µs | 16,668µs | 0/0/0 | 0 |
| fps60-r2  | 0.9906 | 0 | 0 | 38,000µs | 16,668µs | 0/0/0 | 0 |
| fps120-r1 | 0.9886 | 1 (30.4ms) | 1 (79.7ms @f0) | 29,667µs | 8,334µs | 0/0/0 | 0 |
| fps120-r2 | 0.9875 | 0 | 1 (71.3ms @f0) | 29,667µs | 8,334µs | 0/0/0 | 0 |
| fps144-r1 | 0.9912 | 1 (34.3ms) | 1 (69.9ms @f0) | 28,278µs | 6,945µs | 0/0/0 | 0 |
| fps144-r2 | 0.9873 | 0 | 1 (69.9ms @f0) | 28,278µs | 6,945µs | 0/0/0 | 0 |
| fps240-r1 | 0.9869 | 2 (22.8ms) | 1 (71.3ms @f0) | 25,500µs | 4,168µs | 0/0/0 | 0 |
| fps240-r2 | 0.9867 | 2 (26.1ms) | 1 (71.3ms @f0) | 25,500µs | 4,168µs | 0/0/0 | 0 |

- **slope ≈ 0.99 ทุกอัตรา** — ส่วนต่าง ~1% คือ first-gap artifact ที่หัวไฟล์ (25.5–38ms จาก M1-W1/W1) บวก vsync quantization — **คงที่ deterministic ไม่ใช่อาการ**
- Runtime telemetry ทุก run: `dropped=0, replaced=0, noFrame=0, accessLost=0, seqSkips=0, lateTicks ≤25/961`, `maxSelectedLag = 32–41ms` (ขอบฟ้า freshness ของ CFR selection), captured ≈ 305 (จอ 75Hz — ฟิสิกส์ของเนื้อหา)
- Clock ฝั่งต้นทางแข็งแรงทุก run: step p50 = 13.32–13.33ms (75Hz เป๊ะ)

## 3. Stage-by-stage isolation (ต่อ mission ที่กำหนด)

| Stage | หลักฐาน | ผลตัดสิน |
|---|---|---|
| **Ddagrab** | emitted=pushed, dropped/replaced/noFrame/accessLost/errors = 0 ทุก run; unique frames ≈ อัตราอัปเดตจอ | **สะอาด** (ภายใต้เงื่อนไขทดสอบนี้) |
| **timestamp** | `CaptureTimeTicks` = DXGI LastPresentTime (QPC 100ns, monotonic, fallback acquire-QPC) — โดเมนเดียวกับ CFR target; ไม่มีทางเข้า MP4 (M1-W1) | **สะอาด** |
| **queue** | BoundedVideoFrameSink(16, DropOldest): dropped=0, seqSkips=0 ทุก run | **สะอาด** |
| **CFR selection** | maxSelectedLag 32–41ms = เนื้อหาที่แสดง "เก่า" ได้สูงสุด ~40ms (latency, ไม่ใช่ speed error); หัวไฟล์ stall ≤80ms จาก warm-up frame | **เจอ head-stale ≤80ms one-time** — อธิบาย "ภาพค้างช่วงเปิด" ที่เล็กมาก |
| **duplicate/reuse** | dup = re-encode P-frame ของเท็กซ์เจอร์ล่าสุด — slope 0.99 พิสูจน์ว่าเนื้อหาที่ซ้ำตรงกับจอจริง (ไม่มี texture เก่าค้าง) | **สะอาด** |
| **NVENC** | nvenc_errors=0, avg encode 2.5–3.2ms, sync 1-in-1-out — ไม่มี stall | **สะอาด** |
| **mux** | PTS CFR เป๊ะทุกเฟรม (P95 = คาบเฟรม), dup/neg/mono = 0, first-gap = structural artifact, audio dropped=0 | **สะอาด** (ยกเว้น first-gap ที่รู้จัก) |
| **playback/decode (reference)** | ffmpeg decode 583–988fps ≫ budget ทุกอัตรา; slope 0.987–0.991; ไม่มีอาการ | **สะอาดใน reference path** |
| **Gallery.Video player (code-reading เท่านั้น)** | §4 — พบ mechanism ตรงอาการ 2 ตัว | **HYPOTHESIS — ต้องรัน harness** |

## 4. ตัวสงสัยฝั่ง player (อ่านโค้ด — ยังไม่แก้)

`Gallery.Video/PlaybackSession.vb RenderLoop` + `PlaybackClock`:
- **Late-drop → "jump"**: เฟรมที่ `PTS < now − 1.5×interval` **ถูกทิ้ง** (`_droppedLate`) — render thread สะดุดครั้งเดียว (GC, sink.Present ช้า, tick พลาด) = เนื้อหาข้ามหลายเฟรมทันที
- **Starvation hold → "freeze/stale"**: queue จุ 3 เฟรม (~12.5ms @240fps); เมื่อ decode/อัปโหลด D3D11 ไม่ทัน `hold Is Nothing` → sleep → sink **ค้างภาพเดิม** ขณะ master clock (audio) เดินต่อ
- fps มาจาก `AvgFrameRate` ของ container (parse rational ถูกต้อง, fallback 30.0) — ไฟล์ของเรา parse ได้ 59.58/119.16/142.99/238.29 → **ไม่ใช่ต้นเรื่อง fast playback กับไฟล์ชุดนี้**
- decode throughput วัดจริง 583–988fps ≫ 240 — ตัว decode ล้วนไม่ใช่ bottleneck; ตัวที่ต้องวัดต่อคือ **decode+showinfo-parse+D3D11 upload ใน FfmpegDecodeWorker จริง**

## 5. Next experiment (ก่อนแก้โค้ดใด ๆ)

Headless harness (นอก repo, ใช้ assembly ที่ build แล้ว) เปิด `PlaybackSession` เล่น `fps240-r1.mp4` + fps60 เทียบ:
1. hook counters ทุก tick: `_droppedLate`, starvation spans (hold==Nothing ต่อเนื่อง), present intervals, master-clock source (audio vs wall)
2. เงื่อนไข A: เล่นปกติ; B: จำกัด CPU (จำลองเครื่องโหลดหนัก) เพื่อดัน late-drop/starvation ให้เกิดจริง
3. เกณฑ์ตัดสิน: ถ้า jump/freeze ปรากฏ**เฉพาะตอนเล่น** ทั้งที่ไฟล์ slope=0.99 → ปิดที่ player (แก้ได้โดยไม่แตะ production recording timing)

## 6. FACT / HYPOTHESIS / UNKNOWN

**FACT** — ไฟล์จาก native chain ที่ 60/120/144/240 มีเนื้อหา faithful (slope 0.987–0.991, jump ≤34ms, stall หัวไฟล์ ≤80ms one-time); PTS layer สมบูรณ์; ทุก stage ของ recording pipeline ผ่าน telemetry สะอาด; decode throughput เกิน budget 4×
**FACT** — freeze ที่ "ถูกต้อง" มีอยู่จริงเมื่อ**จอนิ่งจริง** (W1 telemetry: maxSourceGap ถึง 1,213ms ตอน desktop นิ่ง — ไฟล์เก็บความนิ่งนั้นตรงตามจริง ไม่ใช่บั๊ก)
**HYPOTHESIS** — อาการ freeze/jump ที่ user เห็นมาจาก Gallery RenderLoop (late-drop/starvation) หรือจาก master-clock (audio position) — มี mechanism + ตัวนับรองรับ ยังไม่ reproduce
**UNKNOWN** — พฤติกรรมภายใต้โหลด GPU/CPU จริง (เกม), DXGI AccessLost กลางไฟล์, และการเล่นผ่าน Gallery จริง (ต้องการ harness §5)

## Evidence Index

- `evidence/w2-freeze-jump/w2-analysis.jsonl` — ผลวัดรายไฟล์ (ทุก metric)
- `evidence/w2-freeze-jump/fps{60,120,144,240}-r{1,2}.mp4 + .log + -clock.csv` — 8 recordings พร้อม log ต้นทาง
- `evidence/w2-freeze-jump/clockapp.ps1 / run_w2.sh / analyze_w2.py` — เครื่องมือทั้งชุด (reproducible)
- `evidence/w2-freeze-jump/r1_frame_top.png / r2_frame_2.png` — ภาพยืนยัน geometry
- Cross-reference: `M1-W1-TIMING-CAUSALITY-REPORT.md` (PTS semantics, first-gap), `W1-FPS-MATRIX-REPORT.md` (matrix baseline)
