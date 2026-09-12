# W2 (Phase 2) — Playback Freeze/Jump Reproduction: Gallery.RenderLoop **CONFIRMED**

**Date:** 2026-09-12
**Branch:** `Engine-Rebuild-Stabilization` — production code ไม่ถูกแตะ, ไม่มี commit
**Mission:** M1/W1 — Playback Freeze/Jump Reproduction (headless harness + deterministic injections; ห้ามแก้ production ก่อนมี reproduction)

---

## 0. Verdict (TL;DR)

**อาการ freeze/jump จาก Gallery.RenderLoop: REPRODUCED แบบ deterministic แล้ว** — ใช้ headless harness ขับ
`PlaybackSession` (production `Gallery.Video.dll` ตัวจริง) เล่นไฟล์ native-chain จาก W2 พร้อม poller ความละเอียดสูง
จับ `FramesPresented / DroppedLateFrames / LastPresentedPtsTicks / PositionTicks / State` ต่อเหตุการณ์:

| เคส (injection ตาม mission) | presented | droppedLate | สภาพที่ผู้ใช้เห็น |
|---|---|---|---|
| c1 normal 60fps | 217 | 25 | เล่นได้ + **เนื้อหา ~0.4s แรกหาย** (23 เฟรมแรกถูก late-drop พร้อมกันที่ 439ms) |
| c2 **240fps** | **0** | **966** | **จอค้างสนิท** ทั้งไฟล์ (4.09s) ขณะเสียง/clock เดิน, Playing ยืดเป็น 19.9s |
| c3 240fps + tick 2ms | 0 | 966 | เหมือน c2 → **RenderTickMs ไม่ใช่ตัวการ** |
| c4 240fps + queue=1 | 0 | 966 | เหมือน c2 → **ความจุ queue ไม่ใช่ตัวการ** |
| c5 240fps + decode delay (hogs + BelowNormal) | 0 | 966 | freeze เหมือนกัน (injection ตาม mission ข้อ 3) |
| c6 60fps + โหลดหนัก | **0** | **242** | **freeze ที่ 60fps ด้วย** เมื่อ decode ตามไม่ทัน |
| c7 240fps + seek/pause/resume | 0 | 127 (gen=2) | เส้นทาง seek ก็ late-drop 100% |

- **fast playback: ไม่ reproduce** — presented-PTS speed = **1.010×** (ตรง first-gap artifact ของไฟล์) → RenderLoop ไม่ใช่ต้นเรื่อง fast playback (ต้องหา evidence ต่อที่เส้นทางอื่น)
- กลไกต้นตอ (วัดค่าได้ ไม่ใช่ guessing) = **late-frame policy × decode delivery throughput**:

```
FfmpegDecodeWorker ส่งเฟรมจริงเฉลี่ย ~48-49 fps (ขณะ ffmpeg decode ล้วนทำ 988 fps → ตัว worker pipeline เป็นคอขวด)
master clock (audio) เดิน 1.00× wall ตั้งแต่ Play()
late policy: PTS < now − 1.5×interval → DROP
ที่ 240fps: window = ±6.25ms แต่เฟรมมาถึงทุก ~20.6ms → เฟรมทุกตัว "เก่าเกิน" ตอน render loop เห็น
→ drop rate = delivery rate, presented = 0 → จอค้างตลอดไฟล์ ขณะเสียงเล่น
```

---

## 1. Harness (evidence-grade, ไม่แตะ production)

- ตำแหน่ง: `…\Temp\sp_forensic\pbharness\` (นอก repo) — C# console อ้าง production `Gallery.Video.dll` (+ Vortice/SharpGen/NAudio จาก packages เดียวกับที่ build) สร้าง **hidden HWND** เป็น swapchain target → D3D11 present path จริงทำงานโดยไม่มีหน้าต่างโชว์
- ขับ `PlaybackSession.Open → Play → (Seek/Pause/Resume) → EOF → Stop → Dispose` — ไม่มีการ bypass; ทุก decision เกิดใน `RenderLoop` ของ production
- Poller (thread priority Highest, loop ~0.5-1ms) เขียน CSV เมื่อค่าใดเปลี่ยน: `t, state, framesPresented, droppedLate, lastPresentedPts(100ns), positionTicks` + events (state transitions, EOS, FAULT)
- ปิดช่องการตีความ: `AppDomain.FirstChanceException` เพื่อจับ exception ที่โปรดักชันกลืน (ดู §4)

## 2. กลไกที่พิสูจน์ได้ (FACT)

1. **Late-policy ฆ่าทุกเฟรมเมื่อ delivery < fps**: ที่ 240fps ต้องส่ง 240 fps; worker ส่งได้ ~48.6 fps (วัดจากอัตรา drop เมื่อทุกเฟรม late) → ทุกเฟรมมาถึงเมื่อ PTS ตกหลัง clock ≥ ~20 intervals → `DroppedLateFrames = 966/966`, `FramesPresented = 0`, `LastPresentedPtsTicks = −1` (ไม่เคย present แม้แต่เฟรมเดียว)
2. **ไม่ใช่ปัญหา render-loop polling หรือ queue**: เปลี่ยน `RenderTickMs 8→2` (c3) และ `VideoQueueCapacity 3→1` (c4) — ผลเหมือนเดิมทุกตัวเลข
3. **ใช่ปัญหา decode-delivery ภายใต้โหลด**: c5 (hogs + BelowNormal — ffmpeg child ได้ priority ต่ำลงด้วย) และ c6 (60fps + hogs) — freeze เหมือนกัน
4. **Head content loss ~0.4s ที่ 60fps**: c1 ทิ้ง 23 เฟรมแรก (PTS 0..384ms) พร้อมกันที่ 439ms → วิดีโอเริ่มจาก content-time ~0.4s — audio master clock เริ่มเดินก่อน decode ส่งเฟรมแรกเสมอ (startup latency) — นี่คือ "เปิดคลิปแล้วภาพกระโดดข้ามช่วงต้น"
5. **Master clock ไม่มีขอบ media-end**: Playing span 19.9–25.1s บนไฟล์ 4.08s (c2–c5) — clock (audio endpoint) เดินต่อเรื่อย ๆ ขณะ decode ยังส่งไม่ครบ → "เสียงเล่นนานผิดปกติ + ภาพค้าง" พร้อมกัน
6. **Presented speed = 1.010×** (c1) — ไม่มี fast playback จาก RenderLoop

## 3. แผนผังอาการ → กลไก (คำตอบของ mission)

| อาการผู้ใช้ | Reproduced? | กลไกใน RenderLoop |
|---|---|---|
| freeze | ✅ (c2–c6: presented=0 ทั้งไฟล์) | late-drop policy × decode delivery < fps |
| stale | ✅ (same; sink ค้างภาพเดิม ขณะ clock เดิน) | starvation/late-drop — sink ไม่เคยได้เฟรมใหม่ |
| jump | ✅ บางส่วน (c1: head loss ~0.4s + drop กระจาย) | startup race + late-drop บางส่วนเมื่อ delivery ≈ fps |
| fast playback | ❌ ไม่พบใน harness นี้ (speed 1.010×) | — (เส้นทางอื่น; ยังไม่มี evidence) |

## 4. ผลพลอยได้ (production labeling bugs — บันทึกไว้ ยังไม่แก้)

ระหว่างสร้าง harness พบว่า fault message ของ session **ป้ายผิด** เมื่อ dependency โหลดไม่ได้ (จับได้ด้วย FirstChanceException):
1. `AudioRenderer.TryCreate` โยน `FileNotFoundException: NAudio.Wasapi …` (JIT-time type-load — internal catch ของมันช่วยไม่ได้) → `OpenCore` generic catch → **fault kind = `FileLocked`** ทั้งที่ไฟล์ไม่ได้ถูกล็อก
2. ก่อนหน้า (SharpGen.Runtime ขาดจาก probing path): fault = **`RendererUnavailable` "D3D11 renderer creation failed (no GPU/desktop?)"** — gate ปัดเป็น "ไม่มี GPU/desktop" ทั้งที่ GPU/desktop ปกติ (พิสูจน์ด้วย probe ชุดเดียวกัน: device+swapchain+texture สร้างได้ครบ)
   → ถ้าผู้ใช้เจอ fault 2 แบบนี้ ต้องตีความว่า "dependency โหลดไม่ครบ" ก่อนเชื่อข้อความ

## 5. FACT / HYPOTHESIS / UNKNOWN

**FACT** — reproduction ครบตามตาราง §0; กลไก §2.1–2.6 วัดค่าได้; RenderTickMs/queue-capacity ตัดออกจากสมการได้ด้วย A/B
**FACT** — เกณฑ์ "มีสิทธิ์แต่ Gallery production" ของ board เปิดแล้ว (reproduction สำเร็จ) — **แต่ยังไม่แก้ตามข้อบังคับ รอคำสั่ง**
**HYPOTHESIS** — ทางแก้ที่ถูกจุดคือพุ่งที่ **decode delivery throughput ของ FfmpegDecodeWorker** (คอขวด ~49fps ขณะ ffmpeg ล้วนทำ 988fps — น่าจะอยู่ที่ showinfo/stdout plumbing หรือ BGRA copy) และ/หรือ late-window แบบ adaptive; ยังไม่ได้พิสูจน์ทีละตัว
**HYPOTHESIS** — fast playback มาจากเส้นทางอื่น (avg_frame_rate ของไฟล์แปลกประเภท / audio-rate mismatch) — ไม่ใช่ RenderLoop
**UNKNOWN** — ขอบเขต delivery throughput จริงของ worker (ต้อง instrument ต่อ), พฤติกรรมบนเครื่องจริงที่มี GPU contention, และไฟล์ non-native (HEVC/QSV) ที่ถูก format-gate กันอยู่

## 6. Evidence Index

- `evidence/w2-playback-repro/case-summary.json` — สรุปทุกเคส + mechanism + labeling findings
- `evidence/w2-playback-repro/c{1..7}-poll.csv.gz + c{1..7}-log.txt` — timeline ดิบทุกเคส
- `evidence/w2-playback-repro/Program.cs + pbharness.csproj` — harness ต้นฉบับ (reproducible)
- Cross-ref: `W2-FREEZE-JUMP-REPORT.md` (ฝั่งไฟล์สะอาด), `W1-FPS-MATRIX-REPORT.md`, `M1-W1-TIMING-CAUSALITY-REPORT.md`
