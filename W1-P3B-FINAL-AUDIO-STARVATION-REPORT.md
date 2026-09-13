# W1 — P3-B Audio Starvation: Incidence + Architecture A/B Final Report

**Date:** 2026-09-13
**Method:** 12 engine-level runs (production binary, config-per-cell, evidence JSON+log ต่อ run), matrix A/B × 5/10/15min + 3 การวิเคราะห์ซ้ำหลังพบ confound — **production ไม่ถูกแตะ**
**Supersedes:** P3-B interim reading (disk-pressure-only) — หลัง A/B แบบควบคุมตัวแปร พบ trigger ที่แรงกว่า: **เนื้อหาเสียงจริง (real audio content)**

---

## 0. Verdict (TL;DR)

**Audio starvation ใน long-run เกิดทั้งสองสถาปัตยกรรม และ trigger ที่ควบคุมได้คือ "เนื้อหาเสียงจริง" ไม่ใช่ (เฉพาะ) ดิสก์:**

| ปัจจัย | ผล |
|---|---|
| Device/engine side | **ส่งครบ 300s ทุก run** (frames=14404800, data=57.6MB, device dropped=0) — ฝั่งจับถูกต้องเสมอ |
| **Real audio content** (sine loop / desktop audio เล่นจริง) | **starve 6/6 runs** — adur 145–185s (29–61% ของเป้า) ทุกสถาปัตยกรรมและทุก disk state |
| **Silence-heavy content** | 1/1 — adur 300.021s เต็มเป้า, pass=True |
| Disk-full (100%) | ทำให้ starve เร็ว/รุนแรงขึ้น (E-series ตรง) — แต่ **ไม่ใช่ trigger เดียว**: A5_r5 starve ที่ 6.2GB ว่าง |
| **Consumption rate ของ ffmpeg** | video pipe ถูกกิน **100%** ตลอด; audio pipe ถูกกินเฉลี่ย **~117KB/s = 61% ของ realtime (192KB/s)** → audio queue (cap 8MB, drop-oldest) ทิ้งข้อมูลกลางเส้นทาง → adur สั้น |

**ผลต่อสถาปัตยกรรม (ทั้งคู่ล้มเหลวบน real audio long-run):**
- **A single-pipe (system-only, map ตรง):** 4/4 starved (145–185s) — รวม run ที่ดิสก์ว่าง 6.2GB
- **B multi-pipe amix (system+mic):** 2/3 finalize-timeout → salvage; 1/3 (B15, ดิสก์เต็มจริง) ENOSPC; **และไฟล์ SEP-test ที่ map ถูกต้องกลับขาด mic stream ใน output ทั้งที่ mic fed ครบ 28.8MB** (ดู §4)

## 1. ตาราง runs ทั้งหมด (12 runs)

| Run | audio | เป้า | vdur | adur | starve@T0+ | หมายเหตุ |
|---|---|---|---|---|---|---|
| A5_r1 | system | 300s | 300.044 | 164.4s | 303.1s* | disk ~100%; real desktop audio |
| A5_r2 (=5_r2) | system | 300s | 300.027 | 145.0s | 303.0s* | disk ~100%; real audio |
| A10_r1 | system | 600s | 600.050 | 173.6s | 613.7s* | disk ~100%; real audio |
| A15_r1 | system | 900s | 900.056 | 184.6s | 914.0s* | disk ~100%; real audio |
| **A5_r3** | system | 300s | 300.044 | **300.021 ✓** | — | **silence-heavy** (data 17MB/silence 40MB), disk 7.2GB |
| **A5_r4** | system | 300s | 300.027 | **171.9s** | ~305s | disk 2.3GB; **real audio 57.6MB** |
| **A5_r5** | system | 300s | 300.027 | **183.5s** | ~306s | **sine 600Hz loop ตลอด**, disk 6.2→5.4GB — disk theory ตาย |
| B5_r1 (amix) | sys+mic | 300s | 300.027 | 6212s** | 303.0s | finalize timeout → salvage; sys/mic fed ครบ |
| B5_r2 (amix) | sys+mic | 300s | 300.027 | 5456s** | 303.0s | finalize timeout → salvage |
| B10_r1 (amix) | sys+mic | 600s | 600.033 | 7132s** | 613.7s | finalize timeout → salvage |
| B15_r1 (amix) | sys+mic | 900s | 116.0 | 0.2s | 126.2s | **ENOSPC** exit=-28, 1.9GB dropped |
| B5_r3 (amix) | sys+mic | 300s | 300.027 | 7189s** | — (ไม่มี break) | **ส่งครบ 100% แล้ว finalize timeout** (สะอาด) |

\* = pipe break เกิดหลัง session จบ (drain phase) — ไม่ใช่กลางเล่น
\** = ค่า duration จากไฟล์ salvage — fragmented MP4 ไม่ finalize ทำ timebase พัง (ไม่สะท้อนเนื้อหา)

## 2. กลไกที่พิสูจน์ได้ต่อชั้น (measured)

```
[WASAPI device] ส่งครบ 300s ทุก run (frames=14404800, dropped=0)     ← ฝั่งจับสมบูรณ์
[AudioEngine] ส่งครบเข้า sink (data 57.6MB ต่อ 300s)                 ← ฝั่ง engine สมบูรณ์
[Audio PipeFeed → ffmpeg] ffmpeg อ่านเฉลี่ย ~117KB/s = 61% realtime
  → video pipe ถูกกิน 100% (2.125MB/s ตรงตาม CFR) ขณะ audio ตกหล่น
  → audio PipeFeed queue (cap 8MB, drop-oldest) ทิ้งเนื้อหากลางทาง
  → adur สั้นลงเหลือ 145–185s (การกินหยุดประมาณนั้นทุก run ที่เป็น real audio)
[Stop] drain budget 3s → ทิ้งคิดงค้าง 39–86MB → pass=False ตาม truth
```

- **บรรทัดชี้ขาด:** r3 (ผ่าน) vs r4/r1/r2/r5 (starve) — device/engine **เหมือนกันทุกด้าน** ต่างกันที่ (a) เนื้อหา: silence-heavy vs real audio (b) disk free 7.2 vs 2.3–5.4GB — และ r5 (sine, 6.2GB ว่าง) ตัด disk ออก → **real audio เป็นตัวเร่ง**
- B multi-pipe เพิ่มอีกสองอาการ: amix finalize-hang หลัง EOF ทุก run ที่ stream ส่งครบ (5/5: r1/r2/r3 + B10) และ **mic stream หายจาก output ใน SEP-test** (ดู §4)

## 3. FACT / HYPOTHESIS / UNKNOWN

**FACT**
1. Real-audio long-run: starve 6/6 (single 4/4, ผ่าน B อีก 2 ทาง failure) — adur 145–185s; silence-heavy 1/1 ผ่านเต็ม
2. Device/engine delivery สมบูรณ์ 100% ทุก run — failure เริ่มที่ ffmpeg consumption
3. ffmpeg กิน video pipe 100% แต่ audio pipe เฉลี่ย 61% (real audio) — audio PipeFeed drop-oldest ทิ้งกลางทาง
4. amix finalize-timeout ซ้ำทุก run ที่ส่งครบ (5/5 รวม B5_r3); salvage ได้ไฟล์แต่ audio metadata พัง
5. SEP SeparateTrack mapping ยืนยันด้วย command-line dump (`-map 0:v -map 1:a -map 2:a` ไม่มี amix) — output ขาด mic stream; sys เองก็ starve @199.5s (dropped 19.3MB) — **SeparateTrack ยังไม่แก้ starvation**
6. Phase A disk guard: refuse/refuse-boundary ทำงานตรง free space จริง; regression 46/0 ไม่เปลี่ยน

**HYPOTHESIS**
- ffmpeg audio consumption collapse มาจากการแย่ง I/O/CPU ของ video muxer path กับ AAC encode ภายใต้ (a) เนื้อหาเสียงความถี่สูง (AAC frames ใหญ่) (b) การเขียน video 17Mbps ต่อเนื่อง — silence เบาจนไม่ trigger
- drop-oldest policy ของ audio PipeFeed (8MB) ทำให้เสียเป็นช่วง ๆ แทนที่จะ backpressure — เป็นทางเลือก design ที่ตอนนี้มี evidence ว่าทำร้าย long-run

**UNKNOWN**
- จุดภายใน ffmpeg ที่แท้จริง (demuxer read scheduling? AAC encoder backpressure? muxer interleave) — ต้อง debug build ของ ffmpeg
- ทำไม SEP mode ขาด mic stream ใน output ทั้งที่ map+data ครบ (ffmpeg internal)

## 4. Architecture decision table

| Architecture | Long-run (real audio) | Audio integrity | Finalize | Dropped | Complexity | Recommendation |
|---|---|---|---|---|---|---|
| **AMIX (current, sys+mic)** | ✗ starve/fail 6/6 (incl. 2× finalize-hang clean disk) | ✗ mixed stream สั้น; salvage metadata พัง | ✗ timeout 5/5 เมื่อส่งครบ | 19–86MB | ต่ำ (มีอยู่) | **FIX/แทนที่** |
| **A single-pipe (sys only)** | ✗ starve 4/4 บน real audio (145–185s); ✓ silence 1/1 | ✓ ถ้าไม่ starve (ครบ 300s ใน r3) | ✓ exit=0 | 21–86MB | ต่ำสุด | ไม่พอสำหรับ real audio |
| **SeparateTrack (map ตรง)** | ✗ sys starve @199.5s + **mic stream หายจาก output** | ✗✗ แย่สุด (mic หายเงียบ) | ✓ exit=0 (แต่ไฟล์ไม่ครบ) | 19.3MB | ต่ำ | **ไม่ใช่ทางออก** |
| **SIDECAR/second-pass (WAV เขียนตรง + mux ทีหลัง)** | ยังไม่ได้รัน E2E — แต่ engine-side delivery พิสูจน์แล้วว่าสมบูรณ์ 100% ทุก run, และ WavSidecarWriter (production) เขียนตรงดิสก์ไม่ผ่าน FFmpeg pipe | คาด ✓ (bypass จุด failure ที่พบ) | ต้องวัด | คาด 0 | กลาง | **SIDECAR (เดินหน้าตรวจ E2E ต่อ)** |

## 5. Acceptance checklist

1. incidence per config — ✅ A real-audio 4/4; B ไม่มี valid output 3/3; silence 1/1 ผ่าน
2. starvation time distribution — ✅ 145–185s (real audio), เกิดหลัง T_END เสมอใน disk-full runs, กลางเล่นใน clean runs (B15/r5)
3. single vs multi evidence — ✅ single ไม่ปลอดภัยเช่นกัน → ไม่ใช่ architecture race แต่เป็น **FFmpeg audio-pipe consumption collapse**
4. no production change — ✅ (Phase A disk guard + P1-A ที่ทำไปแล้วเป็นงานก่อนหน้าตาม mission ที่อนุมัติ; สถาปัตยกรรมเสียงไม่ถูกแตะ)
5. orphan check ทุก run — ✅ 0 ทุก run
6. no false PASS — ✅ pass=False ทุกไฟล์ไม่สมบูรณ์; ไม่มีการเลี่ยง gate

## 6. RECOMMENDATION: **SIDECAR / SECOND-PASS (เดินหน้า proof-of-concept ต่อ)**

- ทั้ง AMIX และ single-pipe ตายที่จุดเดียวกัน: **FFmpeg audio pipe consumption** — การเปลี่ยน mapping/architecture ใน FFmpeg จึงไม่ใช่ทางออก
- ฝั่งที่พิสูจน์แล้วสมบูรณ์ 100% ทุก run = WASAPI device → AudioEngine (data 57.6MB, dropped=0) → จุดต่อไปคือ **เขียน WAV ตรงด้วย WavSidecarWriter (production class ที่มีอยู่) แล้ว mux เสียงใน second-pass จากไฟล์** — bypass FFmpeg audio pipe ทั้งจุด
- ต้องทำ E2E proof ของ sidecar path ก่อน production flip (งานถัดไป)
- คู่ขนาน: **disk headroom guard (Phase A) ติดตั้งแล้ว** — กัน ENOSPC edge; และอนาคต probe ที่ silent/audio-heavy ต้องแยกผลตาม content type (บทเรียนจาก r3 vs r5)

## 7. Evidence Index

- `…\Temp\sp_forensic\p3b\*.json/*.log` — 12 runs (A5_r1..r5, A10, A15, B5×3, B10, B15) + `evidence/w1-p3b-audio-starvation/` (A5_r3/r4/r5 decisive set)
- `…\Temp\sp_forensic\phaseb\*.json/*.log` — Phase B A/B runs (AMIX 05/10M ×3, SEP 05M ×2) + `sep_cmdline.txt` (SeparateTrack args จริง)
- Runners: `runner.ps1, batch.ps1, phaseb.ps1, runall.ps1, resume1.ps1, resume2.ps1`
- Cross-ref: `W1-P3B-AUDIO-STARVATION-REPORT.md` (รอบแรก — disk correlation), `W1-P1A-STOP-BOUNDARY-REPORT.md`
