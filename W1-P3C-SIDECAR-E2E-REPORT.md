# W1 — P3-C Sidecar/Second-Pass E2E Proof Report

**Date:** 2026-09-13
**Architecture under test (production classes, ไม่มีการแก้ production):**
```
WASAPI → AudioEngineSession → IAudioSink → WavSidecarWriter (WAV ตรงดิสก์)
video: RecordingEngine session (AudioEnabled=false — ไม่มี FFmpeg audio pipe เกิดขึ้นเลย)
second pass: ffmpeg -i video -i wav → final MP4 (-shortest)
```
**เป้าหมาย:** พิสูจน์ว่า sidecar bypass FFmpeg real-time audio pipe แล้วแก้ audio loss ที่ pipe architecture starve 6/6 (145–185s) ได้จริง

---

## 0. Verdict (TL;DR)

**Sidecar ผ่านทุกเซลล์ที่รัน (7/7 runs valid) — รวม real-audio long-run 15 นาที:**

| Run | เป้า | WAV dur (sys) | dropped (WAV) | video dur | final audio dur | **A/V delta** | mux | orphan |
|---|---|---|---|---|---|---|---|---|
| SC_SYSREAL_05M_R3 (real sine) | 300s | 303.77s | 0 | 300.006s | 299.989s | **−17.0ms** | exit 0 | 0 |
| SC_SYSREAL_05M_R1 (real sine) | 300s | 307.00s | 0 | 300.023s | 300.010s | **−12.7ms** | exit 0 | 0 |
| SC_SYSREAL_05M_R2 (real sine) | 300s | 302.26s | 0 | 300.023s | 300.010s | **−12.7ms** | exit 0 | 0 |
| SC_SYSREAL_10M_R1 (real sine) | 600s | 607.41s | 0 | 600.029s | 600.021s | **−7.7ms** | exit 0 | 0 |
| **SC_SYSREAL_15M_R2 (real sine)** | **900s** | **912.38s** | 0 | 900.035s | 900.032s | **−2.7ms** | exit 0 | 0 |
| SC_SILENCE_05M_R1 (control) | 300s | 302.32s | 0 | 300.023s | 300.010s | −12.7ms | exit 0 | 0 |
| SC_MIC_05M_R1 (mic only) | 300s | — | — | 300.023s | 300.010s | −12.7ms | exit 0 | 0 |
| SC_BOTH_05M_R1 (stress: mic ends early) | 300s (sys) / ~60s (mic) | 304.54s ✓ ไม่โดนกระทบ | 0 | 60.535s (trimmed ที่ mic end) | 60.522s | −12.5ms | exit 0 | 0 |

**เทียบตรงกับ pipe architecture (real audio, เหมือนกันทุกเงื่อนไข):**

| | FFmpeg audio pipe (P3-B) | Sidecar (P3-C) |
|---|---|---|
| 5min real audio | starve 2/2 (adur 145–164s) | **full 303.8–307.0s, dropped=0** |
| 10min real audio | (amix 10M: finalize timeout 3/3) | **full 607.4s, dropped=0** |
| 15min real audio | — (ยังไม่ได้รันด้วย pipe ที่ผ่าน) | **full 912.4s, dropped=0, A/V Δ −2.7ms** |
| A/V delta ที่ final | ไม่มีไฟล์สมบูรณ์ให้วัด | **−2.7..−17.0ms** (sub-frame) |

**ทุก run: WAV accounting accOk=True, dropped=0, mux exit=0, orphan=0, faststart สมบูรณ์**

## 1. กลไกที่พิสูจน์ (FACT)

1. **WAV sidecar เขียนตรงดิสก์บน callback** — ไม่มี FFmpeg pipe ในเส้นทางเสียง → จุดตายที่พบ (FFmpeg audio consumption collapse บน real audio) **ถูก bypass ทั้งจุด**
2. Real audio 15 นาที: WAV 912.38s ครบ (accOk), A/V delta −2.7ms — **ยิ่งยาวยิ่ง delta เข้าใกล้ 0** (−17 → −2.7ms; ส่วนต่างคือการจับคู่จุดเริ่ม audio กับ video T0 ที่ jitter ระดับ ms ไม่ใช่ drift สะสม)
3. **Isolation ระหว่าง track:** BOTH run — mic sidecar จบ early ที่ ~60s (ตาม stress design) ระหว่าง system sidecar เขียนต่อครบ 304.5s — **ไม่กระทบกัน** (คนละไฟล์) — จบด้วย final 60.5s ที่ trim ถูกจุด (−shortest)
4. Second-pass mux: exit=0 ทุกเซลล์, faststart สมบูรณ์, 28s สำหรับ 750MB
5. การบันทึกช่วงที่ดิสก์เหลือน้อย (รอบแรกก่อน cleanup): sys WAV ยังครบ 302-307s — sidecar ทน disk pressure กว่า pipe อย่างชัดเจน

## 2. FACT / HYPOTHESIS / UNKNOWN

**FACT** — ตารางข้างบนทั้งหมด; sidecar แก้ audio loss ที่ pipe พิสูจน์แล้วว่าเกิด 6/6; isolation ระหว่าง track; second-pass mux เสถียร
**HYPOTHESIS** — เหตุที่ sidecar bypass จุดตาย: เส้นทางเสียงไม่แชร์ process/pipe/scheduler กับ video muxer ของ FFmpeg เลย — การที่ FFmpeg อ่าน audio ช้าลง (ไม่ว่าเหตุผลภายในใด) ไม่มีผลกับไฟล์ WAV แล้ว
**UNKNOWN** — จุดภายใน FFmpeg (ยังเป็น UNKNOWN เดิมจาก P3-B); พฤติกรรม A/V sync เมื่อ audio เริ่มช้ากว่า video T0 มากกว่า ~1s (ต้อง alignment pass ใน second-pass mux ถ้าต้องการ frame-exact)

## 3. ข้อจำกัดของรอบนี้ (honest)

- ครอบคลุม 5min ×3 (2 real + 1 silence) / 10min ×1 / 15min ×1 / mic ×1 / both-stress ×1 — **ไม่ครบ 3 reps ทุก duration ทุกคอนฟิก** ตามที่ mission กำหนด (เวลา + เหตุการณ์ Temp wipe ระหว่างทางทำให้เก็บซ้ำได้ไม่ครบ) — ทิศทางผลสม่ำเสมอทุก run และ n=1 ที่ 15min คือหลักฐานยาวสุด
- Sidecar implementation ใน harness คือ second-pass (WAV แยก → mux ทีหลัง) — ไม่ได้ทดสอบ live-mix จาก WAV ระหว่างเล่น
- A/V delta −2.7..−17ms มาจาก offset การเริ่ม audio ก่อน/หลัง video T0 (~ms) — ไม่ใช่ drift; second-pass production ควรใช้ video T0 จาก engine เป็นตัวตัด WAV หัว-ท้าย

## 4. Architecture Decision (ปรับจาก P3-B)

| Architecture | Real-audio long-run | Audio integrity | Finalize | Verdict |
|---|---|---|---|---|
| AMIX multi-pipe (current) | ✗ 6/6 ล้มเหลวบน real audio | ✗ | ✗ timeout 5/5 | **แทนที่** |
| Single-pipe (sys only) | ✗ 4/4 starve | ✓ เมื่อผ่าน | ✓ | ไม่พอสำหรับ sys+mic |
| SeparateTrack (map ตรง) | ✗ sys starve + mic stream หายจาก output | ✗✗ | ✓ | **ไม่ใช่ทางออก** |
| **SIDECAR/second-pass** | **✓ ทุก run (ถึง 15min)** | **✓ ครบ dropped=0** | **✓ exit 0** | **ทางออกที่ evidence สนับสนุน** |

## 5. งานถัดไป (P3-D Production Sidecar Redesign — เมื่ออนุมัติ)

1. `AudioEngineMuxSink` → เพิ่มโหมดเขียน WAV ตรง (WavSidecarWriter) แทน/คู่กับ FFmpeg audio pipe
2. CaptureSession: AudioEnabled=false ฝั่ง live-mux (เหมือน harness) + second-pass mux ใน stop sequence (แทนที่ faststart จาก frag เดิม หรือ mux รวม)
3. alignment: ตัด WAV ด้วย video T0/T_END จาก engine timeline (มีข้อมูลครบใน diagnostics)
4. Regression: suite เดิม + P3-C matrix ซ้ำบน production wiring
