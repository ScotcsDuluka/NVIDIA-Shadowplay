# W3/W1 — P3-B Audio Starvation Reproduction / Incidence Report

**Date:** 2026-09-13
**Method:** production ConsoleDriver + `AudioEngineSession`/`LiveMuxSession` ตัวจริง, config ต่อเซลล์, runner เก็บ evidence ต่อ run; **ไม่มีการแก้ production**; ทุก run มี orphan check
**Critical confound ที่ค้นพบระหว่างทาง:** ดิสก์ C: เต็ม 100% (เหลือ 485MB) ระหว่างเก็บ matrix → ทุก cell ก่อน cleanup ทำงานภายใต้ disk pressure → หลัง cleanup (7.2GB) รันยืนยันซ้ำ → **ตาราง incidence ต้องแบ่งตาม disk state**

---

## 1. ตารางผลรวม (7 runs, 35+ นาทีของการบันทึก)

| Cell | audio | target | vdur | adur | starve@T0+ | โหมด failure |
|---|---|---|---|---|---|---|
| A5_r1 (disk ~100%) | system | 300s | 300.044 | **164.4s** | 303.1s* | drain timeout 39.3MB ทิ้ง |
| A5_r2 (disk ~100%) | system | 300s | 300.027 | **145.0s** | 303.0s* | drain timeout 42.9MB |
| A10_r1 (disk ~100%) | system | 600s | 600.050 | **173.6s** | 613.7s* | drain timeout 86.4MB |
| A15_r1 (disk ~100%) | system | 900s | 900.056 | **184.6s** | 914.0s* | drain timeout 82.6MB |
| **A5_r3 (clean 7.2GB)** | system | 300s | 300.044 | **300.021** ✓ | — | **PASS สมบูรณ์** |
| B5_r1 (disk ~100%) | sys+mic | 300s | 300.027 | 6212s** | 303.0s | finalize timeout → salvage |
| B5_r2 (disk ~100%) | sys+mic | 300s | 300.027 | 5456s** | 303.0s | finalize timeout → salvage |
| B10_r1 (disk ~100%) | sys+mic | 600s | 600.033 | 7132s** | 613.7s | finalize timeout → salvage |
| B15_r1 (disk 100% เต็มจริง) | sys+mic | 900s | 116.0 | **0.2s** | 126.2s | **ENOSPC** ffmpeg exit=-28 "No space left on device" 1.9GB dropped |
| **B5_r3 (clean 7.2GB)** | sys+mic | 300s | 300.027 | 7189s** | — (ไม่มี pipe break) | **finalize timeout** (audio fed ครบ 57.6MB, mic 28.8MB, dropped=0) → salvage |

\* timestamp ที่ pipe write fail ครั้งแรก = **หลัง session จบเสมอ** (T_END + 3-14s — เกิดตอน drain ของ stop sequence ไม่ใช่กลางเล่น)
\** adur จากไฟล์ salvage — timebase ของ fragmented MP4 ที่ไม่ได้ finalize = metadata พัง ไม่สะท้อนเนื้อหา

## 2. Incidence (แบ่งตาม disk state — สิ่งที่ข้อมูลสนับสนุนจริง)

| คอนฟิก | ดิสก์ ~100% | ดิสก์ปกติ (7.2GB ว่าง) |
|---|---|---|
| **A single-pipe** | 4/4 audio สั้น 21–55% ของเป้า (drain-discards 39–86MB) | **0/1 — PASS เต็ม 300.021s** |
| **B multi-pipe** | 3/3 ไฟล์ salvage ที่ adur พัง (5456–7132s) + B15 ENOSPC | **0/1 valid — finalize timeout** (audio ส่งครบ 100% แต่ ffmpeg hang หลัง EOF) |

- **time-to-failure (A, disk-full):** 145–185s ของเสียงจริงก่อนหยุด — สอดคล้อง E-series เดิม (E1=0.405s, E2=176.5s, E3=162.4s — ตัวเลขเดิมอยู่ในแถวเดียวกัน)
- **single vs multi:** ภายใต้ดิสก์ปกติ **single-pipe ผ่าน, multi-pipe ล้มด้วย amix finalize-hang** — multi-pipe ไม่เคยให้ไฟล์ valid เลยในทุก run

## 3. FACT / HYPOTHESIS / UNKNOWN

**FACT**
1. Clean-disk A5: audio ครบ 300.021s (target 300s), pass=True, dropped=0, orphan=0 — single-pipe ไม่ starve เมื่อดิสก์ปกติ
2. ทุก cell ก่อน cleanup ทำงานขณะดิสก์เหลือ ≤485MB→100%; A-cells ตายแบบเดียวกันทั้ง 4: queue ค้าง 39–86MB (ffmpeg หยุดกิน audio pipe) → drain timeout 3s → ทิ้ง → adur 21–55%
3. B15: **ENOSPC จริงจาก ffmpeg** ("No space left on device", exit=-28, 1.9GB dropped)
4. B5_r3 (ดิสก์ปกติ): **ไม่มี pipe break, dropped=0, ส่งครบ** — แต่ `ffmpeg finalize timeout` (exit=-1) → partial salvage → audio duration metadata พัง (7189s) — **amix+framed-mp4 finalize hang เกิดซ้ำทุก run ของ multi-pipe (3/3)**
5. Orphan ffmpeg = 0 ทุก run; worker count ถูกต้อง; P3-A containment (bounded stop) ทำงาน — ไม่มี hang ถาวร

**HYPOTHESIS**
- กลไก A-starvation ใต้ disk pressure: ffmpeg เขียน fragmented MP4 ลงดิสก์เต็ม → write stall → หยุดดึง audio pipe → PipeFeed queue สะสม (producer ยัง throttle ตาม consumer ไม่ได้เพราะ consumer=ffmpeg ค้าง) → ตอน Stop งบ drain 3s ไม่พอ → ทิ้งท้าย
- กลไก B finalize-hang: `amix=inputs=2:duration=longest` บน input pipe ยาว + fragmented output — amix ไม่จบเร็วพอเมื่อ EOF มาจากสอง pipe พร้อมกัน (จะต้อง profile ต่อใน ffmpeg ถึงจุด hang — นอกเหนือ build นี้)
- E-series เดิม (176.5s/162.4s) น่าจะเป็น disk-pressure คลาสเดียวกัน (#1663 เป็น precedent เท่านั้น — ยังไม่ใช่ root cause ที่พิสูจน์)

**UNKNOWN**
- จุด hang ภายใน ffmpeg ที่แท้จริง (amix buffering vs muxer write) — ต้อง gdb/loglevel debug ของ ffmpeg
- incidence ที่แท้จริงบนดิสก์ว่างเพียงพอ: n=1 ต่อ clean cell (ต้องเก็บซ้ำ ≥3 ต่อ cell เพื่อ rate ที่มั่นใจ)
- พฤติกรรม SeparateTrack (ไม่ amix) — อยู่นอก scope ห้ามแตะ

## 4. Acceptance checklist

1. incidence per configuration — ✅ (แบ่ง disk state; หมายเหตุ n เล็ก)
2. distribution/range of starvation time — ✅ (A: 145–185s เสียงจริง; B: finalize-hang ไม่มี starvation แบบ A)
3. evidence single-pipe materially safer — ✅ (clean-disk: A pass / B fail; disk-full: ทั้งคู่ fail แต่ A เสียท้ายแบบ graceful, B ได้ไฟล์ metadata พัง)
4. no production change — ✅
5. orphan check ทุก run — ✅ (0 ทุก run)
6. no false PASS — ✅ (pass=False ทุก run ที่ไฟล์ไม่สมบูรณ์; A5_r3 pass=True เพราะไฟล์สมบูรณ์จริง)

## 5. RECOMMENDATION: **REDESIGN — ออกจาก amix multi-pipe mix**

เหตุผล: multi-pipe (system+mic → amix → fragmented MP4) **ล้มทุก run ด้วย finalize-hang** แม้ดิสก์ว่างและ stream ส่งครบ 100% — และตอนกลายเป็นไฟล์ salvage อัด audio-duration พัง (7189s) ไม่ใช่ไฟล์ที่ใช้ได้ → นี่เป็นความเสี่ยงเชิงสถาปัตยกรรมของ amix บน input แบบ pipe ไม่ใช่ bug ที่แก้ด้วย timeout
ทางที่ evidence ชี้: เส้นทาง **SeparateTrack** (mic เป็น audio stream ที่สองของ MP4 โดยไม่ amix — LiveMux มีโหมดนี้อยู่แล้ว) หรือ sidecar/second-pass สำหรับ mic
ก่อน redesign: ต้องแยก task **disk-headroom guard** (ปิด/เตือนก่อนดิสก์ < 2GB — เพราะทุก failure-mode ในรอบนี้ถูก trigger ด้วยดิสก์เต็ม) และยืนยันซ้ำ clean-disk incidence ด้วย n≥3/cell

## 6. Evidence Index

- `…\Temp\sp_forensic\p3b\*.json` — evidence ต่อ run (duration/dropped/orphan/exit)
- `…\Temp\sp_forensic\p3b\*.log` — full logs (LiveMux/pipe/drain/salvage/ENOSPC)
- `runner.ps1 / batch.ps1` — matrix runner
- ไฟล์ MP4 ต่อเซลล์ถูกลบเพื่อคืนดิสก์ (เก็บเฉพาะที่จำเป็น: before/after ของ P1-A ใน `evidence/`)
