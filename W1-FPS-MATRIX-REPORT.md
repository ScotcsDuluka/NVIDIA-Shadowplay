# W1 — Native NVIDIA FPS Matrix Report (Ddagrab + NVENC, GTX 1080 Ti)

**Date:** 2026-09-11
**Branch:** `Engine-Rebuild-Stabilization` (HEAD `cca2b2d8c2`) — production code/timing ไม่ถูกแตะ, ไม่มี commit
**Question:** native Duluka/NVENC pipeline ทำ 30/60/120/144/240 FPS ได้จริงหรือไม่ — โดยเฉพาะ **240 FPS บน GTX 1080 Ti**

---

## 0. Verdict (TL;DR)

**FACT: 240 FPS ใช้งานได้จริงบน GTX 1080 Ti (driver 582.66) บน native pipeline นี้** — ทั้ง 3 runs ผ่านทุกเกณฑ์:

- MP4 valid CFR 240fps: `r_frame_rate=240/1`, **721 frames / 3s** (720 CFR ticks + 1 final-frame encode = ตาม design เป๊ะ)
- Cadence หลังเฟรมแรก: **P95 = P99 = 4,168 µs** (ideal 4,166.67 µs — เบี่ยงเป็น µs-quantization ของ ffmpeg เท่านั้น)
- **first ΔPTS = 25,500 µs** = structural artifact รู้จัก (สูตร M1-W1: 1 video frame @240 = 4,166 µs + AAC priming 21,333 µs) — **ไม่ใช่ VFR, ไม่ใช่ timing ของเอนจิน**
- monotonic violations = 0, duplicate PTS = 0, negative PTS = 0 (ทุก run ทุก fps)
- NVENC: `nvenc_errors=0`, avg encode **2.9–3.1 ms < 4.167 ms** interval — encoder ตามโหลดไหว, lateTicks 6.7–9.2% ถูก catch-up ดูดคืนจน cadence ใน MP4 ไม่เสียเลย
- audio dropped=0, mux dropped=0, decode errors=0, orphan ffmpeg 0, `pass=True` ทุก run

ข้อจำกัดเดียวที่วัดได้: จำนวน**เฟรมจริง (unique)** จาก desktop คือ ~55–62 fps (จอหลัก 75Hz + อัปเดตจริงของ desktop) → ที่ 240fps ~75% เป็น duplicate ที่ CFR re-encode ตาม design — เป็นขีดจำกัดทางฟิสิกส์ของ**เนื้อหาจอ** ไม่ใช่ความสามารถของ pipeline (motion จริง 240fps ต้องมาจากจอ/เนื้อหา 240Hz เท่านั้น)

---

## 1. Method (native path พิสูจน์แล้วว่าไม่ใช่ QSV/Intel)

- **Canonical chain:** `dotnet run --project CaptureEngine.Recording.ConsoleDriver -- --videocheck --config-chain` → `NextRecordingConfig.LoadEffectiveSettings → MapStartupConfig → RecordingEngine.Initialize → BuildSessionConfig → StartSession` — สายเดียวกับ production host
- **Native components:** capture = `DdagrabBackend` (DXGI Desktop Duplication + D3D11 staging), encode = `NvencEncoderBackend` (NVENC API ตรง, FORCEIDR+SPS/PPS เฟรมแรก, periodic IDR GOP=60), mux = `LiveMuxSession` (named pipes → ffmpeg `-c:v copy` + AAC) — **ไม่มี FFmpegBackend/QSV เข้าเลย** (echo ใน log: `encoder='NVENC_H264' fps=N bitrate=17000000 bps, rc=cbr, preset=p4, gop=60 (GOP independent of FPS)`)
- **Per-fps config:** เขียน `config.json` (`{"Recording":{"FPS":N}}`) ลง gitignored ConsoleDriver bin dir (OverlayConfig search path 2b — production config อื่นไม่ถูกแตะ) → **ลบทิ้งหลังจบ matrix** (ตรวจแล้ว)
- **Matrix:** 30/60/120/144/240 × 3 runs × 3 วินาที = **15 recordings** ทั้งหมด EXIT=0
- **Environment:** GTX 1080 Ti (582.66), จอหลัก 1680×1050@75Hz, capture native resolution, cbr 17 Mbps, p4, audio AAC 48k stereo เปิด
- **วัดผล:** ffprobe ทุกเฟรม (`pts_time`) ผ่าน Python (`analyze.py`) + decode integrity (`ffmpeg -v error -f null`) + telemetry จาก runtime logs

## 2. Timing Table (รวม 15 runs; Δ หน่วย µs)

| FPS | frames (r1/r2/r3) | r_frame_rate | first ΔPTS | สูตรทำนาย (1f + AAC) | Δ min | Δ max | P95 | P99 | dup PTS | neg PTS | mono viol | avg enc (ms) | nvenc err | pass |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 30  | 90 / 90 / 92 | 30/1 | 54,667 | 33,333+21,333=54,666 ✓ | 33,332 | 54,667 | 33,334 | 35,894 | 0 | 0 | 0 | 4.0–4.3 | 0 | True |
| 60  | 181 ×3 | 60/1 | 38,000 | 16,667+21,333=38,000 ✓ | 16,666 | 38,000 | 16,668 | 16,668 | 0 | 0 | 0 | 3.7–4.0 | 0 | True |
| 120 | 362 / 360 / 360 | 120/1 | 29,667 | 8,333+21,333=29,666 ✓ | 8,332 | 29,667 | 8,334 | 8,334 | 0 | 0 | 0 | 3.3–3.7 | 0 | True |
| 144 | 432 / 433 / 433 | 288/1* | 28,278 | 6,944+21,333=28,277 ✓ | 6,943 | 28,278 | 6,945 | 6,945 | 0 | 0 | 0 | 3.9–4.1 | 0 | True |
| **240** | **721 ×3** | **240/1** | **25,500** | **4,166+21,333=25,499 ✓** | 4,166 | 25,500 | **4,168** | **4,168** | 0 | 0 | 0 | 2.9–3.1 | 0 | True |

*144fps: movenc เลือก timebase ให้ `r_frame_rate=288/1` (1200000/144 ไม่ลงตัว) — cadence จริง 6,943–6,945 µs เทียบ ideal 6,944.4 µs; `avg_frame_rate ≈ 143.0`

องค์ประกอบอื่นของ 240fps (r1/r2/r3):

| ตัวชี้วัด | ค่า |
|---|---|
| CFR ticks / frames encoded | 720 / **721** (loop 720 + final-frame encode 1 — code path ที่รู้จัก) |
| lateTicks (>4.167ms) | 66 / 48 / 51 (6.7–9.2%) — catch-up ดูดคืนหมด, maxTickLate 16.6–21.5ms |
| captured (unique) / dup | 164–185 / 540–557 (~75% dup — ฟิสิกส์ของเนื้อหาจอ 75Hz) |
| maxSourceGap (DXGI จริง) | 21.9–244.4 ms — source VFR ตามธรรมชาติ แต่ MP4 CFR สมบูรณ์ |
| mux duration / audio dropped / mux dropped | 3.05–3.07s / 0B / 0B |
| decode errors (`-f null`) | 0 / 0 / 0 |

## 3. สิ่งที่พิสูจน์ได้ (FACT / HYPOTHESIS / UNKNOWN)

**FACT**
1. **240 FPS ใช้งานได้จริง** end-to-end บน GTX 1080 Ti: capture→queue→CFR(240)→NVENC→frag-mux→MP4 ครบ 721 เฟรม/3s, cadence CFR เป๊ะ (P95–P99 = 4,168 µs), monotonic 100%, ไม่มี duplicate/negative PTS, decode ผ่าน, audio ไม่หล่น
2. NVENC (Pascal) ตามโหลด 240fps ที่ native res ไหว: avg encode 2.9–3.1 ms ต่อ interval budget 4.167 ms; ไม่มี NvencErrors; lateTicks ถูก catch-up (สูงสุด 8 ticks/iteration) กลืนจน cadence ในไฟล์ไม่บิดเบี้ยว
3. first ΔPTS ทุก fps ตามสูตร structural เดียวกัน (1 H.264 frame + AAC priming 21,333 µs) — 5/5 อัตราตรงเป๊ะ → สรุปรวมกับ M1-W1: **first-gap เป็น constant ของ container ไม่ใช่ VFR ไม่ใช่ timing ของเอนจิน** (ที่ 240fps มันชัดกว่าชื่ออื่นเพราะคิดเป็น ~4.1 frame slots)
4. Unique-frame rate ของ capture จำกัดด้วยการอัปเดตของ desktop (~55–62 fps บนจอ 75Hz) — duplicate ที่เหลือเป็น CFR-by-design (ทางเดียวกับที่ real ShadowPlay/OBS ทำ)
5. Production timing/โค้ดไม่ถูกแตะ — fps ควบคุมผ่าน config chain เท่านั้น (native Duluka config truth), config ทดสอบอยู่ใน bin dir (gitignored) และถูกลบหลังจบ

**HYPOTHESIS**
- 240fps จะยังคงแม่นเมื่อเนื้อหาจอเคลื่อนไหวถี่ ๆ (จอ ≥144Hz เป็นต้น) — จากการที่ encode budget เหลือเฟือและ catch-up ทำงาน แต่ session ใน matrix นี้เนื้อหาส่วนใหญ่ duplicate
- `avg_frame_rate` ที่อ่านได้ต่ำกว่า nominal เล็กน้อย (238.3@240, 59.87@60) เป็นผลจาก first-gap artifact + µs quantization เท่านั้น

**UNKNOWN**
- พฤติกรรมยาว (เช่น 60 วินาที) ที่ 240fps ภายใต้โหลด GPU จริง (เกม) — นอกขอบเขต session 3s นี้
- บรรทัด ffmpeg ภายในที่ผูก priming เข้า trun แรก (เดิมจาก M1-W1)

## 4. Evidence Index

- `evidence/w1-fps-matrix/analysis.csv` — ตารางวัดทุก run (ทุก metric ข้างบน)
- `evidence/w1-fps-matrix/fps{30,60,120,144,240}-r{1..3}.log` — runtime telemetry ครบ 15 runs
- `evidence/w1-fps-matrix/fps240-r{1,2,3}.mp4` + ตัวแทน fps อื่น 1 ไฟล์ (MP4 ที่เหลืออยู่ใน TEMP ชั่วคราว)
- `evidence/w1-fps-matrix/analyze.py` — สคริปต์วัด (ffprobe frame-by-frame + stats)
- อ้างอิง cross-report: `M1-W1-TIMING-CAUSALITY-REPORT.md` (กำเนิด 38ms/first-gap และ PTS semantics)
