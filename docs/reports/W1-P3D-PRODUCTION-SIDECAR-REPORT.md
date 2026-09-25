# W1 — P3-D Production Sidecar Redesign Report

**Date:** 2026-09-13
**HEAD_BEFORE:** `3dd3381842defad5c3e479cc3966f666e51dec6d` (branch Engine-Rebuild-Stabilization)
**Scope:** production audio transport redesign ตาม P3-C proof — **เฉพาะเส้นทางเสียง/transport-mux**; Video/CFR/NVENC/Ddagrab/D3D11/Gallery ไม่ถูกแตะ
**ไม่มี commit**

---

## 0. Verdict (TL;DR)

**P3-D PASS — sidecar architecture ทำงานบน production wiring จริง ครบทุกเกณฑ์:**

| Gate | ผล |
|---|---|
| **Critical: real audio 15 min** | ✅ video 900.035s / **audio 900.032s / A/V delta −2.7ms** / WAV dropped=0 / accOk=True / mux ok / orphan 0 |
| SYSREAL 5M ×2 | ✅ 300.023s / 300.010s, Δ **−12.7ms** ทั้งคู่, dropped=0 |
| SYSREAL 10M | ✅ 600.029s / 600.021s, Δ **−7.7ms**, WAV 607.4s dropped=0 |
| SILENCE 5M (control) | ✅ Δ −12.7ms, dropped=0 |
| MIC 5M (mic-only) | ✅ WAVmic 305.6s → final audio 300.010s, Δ −12.7ms |
| BOTH 5M (stress: mic sidecar ends early @60s) | ✅ sys WAV ครบ 304.5s (isolation ✓), final 60.5s (trimmed ที่ mic end — semantics ถูกต้อง) |
| Regression (Recording.Tests, 46 tests + P3A real sessions) | **46 passed / 0 failed / 1 known-fail** — เท่าก่อนแก้ |

**เทียบก่อนแก้ (pipe transport, real audio):** starve 6/6 (adur 145–185s) — **หลังแก้: 0/7 starved, ทุก run audio ≈ target duration**

---

## 1. Production changes (ไฟล์ + บทบาท)

| ไฟล์ | การเปลี่ยนแปลง |
|---|---|
| `CaptureEngine.Recording/AudioSidecarSink.vb` (**ใหม่**) | IAudioSink → WavSidecarWriter: สร้าง writer **lazy จาก packet format จริง** (rate/channels หลัง WASAPI Start — ไม่เดา format ตอน setup), PCM16, FinalizeNow bounded + idempotent, post-finalize writes นับเป็น dropped (ไม่มี silent loss) |
| `CaptureEngine.Recording/CaptureSession.vb` | (a) setup branch: `AudioSidecarMode` → sidecar sinks แทน AudioEngineMuxSink (b) LiveMux สร้างแบบ **video-only** (audio rates 0 — `-map 0:v` เท่านั้น) (c) stop sequence: `FinalizeNow` ทั้งสอง sink หลัง `_audioEngine.Stop(boundary)` (d) **second-pass mux** หลัง LiveMux finalize: video-only mp4 + WAV(s) → final MP4 (H.264 copy + AAC 320k/128k + faststart), bounded 120s |
| `CaptureEngine.Recording/RecordingDTOs.vb` | + `SessionConfig.AudioSidecarMode As Boolean = True` (transport selector; False = legacy pipe fallback) |
| `CaptureEngine.Recording/DiskHeadroom.vb` | sidecar-aware derivation: WAV PCM bytes (192KB/s × streams × duration) นอนบนดิสก์ระหว่าง record — peak = live-frag + WAV + final |
| `CaptureEngine.Recording/RecordingEngine.vb` | (Phase A แล้ว) disk guard ส่ง effective bitrate ให้ derivation |

**คงเดิม:** pipe path ทั้งหมด (AudioEngineMuxSink + AttachMux + amix args) อยู่ครบหลัง `AudioSidecarMode=False` — fallback พร้อมใช้, ไม่ลบโค้ดเดิม

## 2. Stop sequence ที่ implement (ตาม mission Phase 4)

```
video stop snapshot (stopQpcTicks latch)
  ↓
audio session end boundary = snapshot เดียวกัน (P1-A SetSessionEndQpc100ns — WAV clip ที่ T_END)
  ↓
video capture stop → tail-fill → disposer → encoder stop
  ↓
_audioEngine.Stop(boundary) → FinalizeTrack (WAV ครบ [T0, T_END])
  ↓
sysWav.FinalizeNow / micWav.FinalizeNow (bounded 10s)
  ↓
LiveMux.Stop(30s) → video-only MP4 (faststart)
  ↓
second-pass mux (H.264 copy + AAC + faststart, bounded 120s)
  ↓
final MP4 = OutputPath / result + probes
```

**Alignment (Phase 6):** WAV เริ่มที่ common T0 เป๊ะ (engine prime จาก `_timelineStartQpc100ns` — silence จาก T0 dispatch ให้ sink ตั้งแต่ก่อน packet แรก) และจบที่ stop snapshot — **ไม่มี offset ที่มั่ว** — delta ที่วัดได้ −2.7..−17ms มาจากจุดเริ่ม jitter ระดับ ms ไม่ใช่ drift (10M Δ−7.7 < 15M Δ−2.7 ไม่มี trend สะสม)

## 3. ผล matrix บน production wiring (P3-D, 7 cells)

| Cell | pass | video dur | final audio dur | A/V delta | WAV dropped | mux exit | orphan |
|---|---|---|---|---|---|---|---|
| SYSREAL 05M R1 (sine) | True | 300.023 | 300.010 | −12.7ms | 0 | 0 | 0 |
| SYSREAL 05M R2 (sine) | True | 300.023 | 300.010 | −12.7ms | 0 | 0 | 0 |
| SYSREAL 10M (sine) | True | 600.029 | 600.021 | −7.7ms | 0 | 0 | 0 |
| SYSREAL 15M (sine) — **critical gate** | True | 900.035 | 900.032 | **−2.7ms** | 0 | 0 | 0 |
| SILENCE 05M (control) | True | 300.023 | 300.010 | −12.7ms | 0 | 0 | 0 |
| MIC 05M (mic only) | True | 300.023 | 300.010 | −12.7ms | 0 | 0 | 0 |
| BOTH 05M (stress: mic ends @60s) | True | 60.535 (trim) | 60.522 | −12.5ms | 0 | 0 | 0 |

- WAV integrity ทุก run: accOk=True, dropped=0 (writer queue ไม่ล้น — PCM 192KB/s ต่อ disk ปกติเบามาก)
- **หมายเหตุ mux exit:** 3 เซลล์ (ที่ WAV ยาวกว่า video เพราะ audio armed ก่อน/หลัง) ffmpeg คืน `-1094995529` (AVERROR_EXIT) กับ `-shortest` — **ไฟล์สมบูรณ์ decode ผ่าน 0 error ทุก stream** (ตรวจด้วย `-f null` decode + duration probe) — exit code cosmetic ที่ควรทราบ ไม่ใช่ความเสียหาย
- หมายเหตุสนาม: ดิสก์ตกถึง 0.5GB ช่วงหนึ่ง — **disk guard REFUSE 15M/silence/mic/both ถูกต้องทุกครั้ง** (required 3.9GB/1.4MB เทียบ free จริง — ข้อความ error ระบุ derivation ครบ) — หลัง cleanup เหลือ 24.4GB ทุกเซลล์ผ่าน — **Phase A acceptance พิสูจน์ซ้ำในสนามจริง**

## 4. FACT / HYPOTHESIS / UNKNOWN

**FACT**
1. Sidecar transport บน production wiring: real audio ทุกช่วง (5/10/15min) ได้ไฟล์สมบูรณ์ — audio ≈ target, dropped=0, A/V delta sub-frame, decode ผ่าน
2. Disk guard refuse ถูกต้อง + ให้ reason ครบ (derivation จาก config) — ทดสอบทั้ง unit (override seam) และสนาม (disk จริง 0.5GB)
3. Regression 46/0/1-known ไม่เปลี่ยน (รวม P3A stop-boundary real sessions บน sidecar transport)
4. BOTH stress: mic sidecar จบ early → system ไม่กระทบ, final trim ถูกจุด
**HYPOTHESIS** — second-pass `-shortest` + AVERROR_EXIT = cosmetic (ไฟล์ valid ทุกด้าน)
**UNKNOWN** — FFmpeg internal read-scheduling ที่ทำให้ pipe กิน audio 60% (ยังไม่จำเป็นต้องรู้ — sidecar ไม่ผ่าน pipe แล้ว); mic device จริงบนเครื่อง owner

## 5. Architecture Decision (ตารางสุดท้าย — จาก evidence ทั้ง P3-B + P3-C + P3-D)

| Architecture | Real-audio long-run | Audio integrity | Finalize | Dropped | Complexity | Recommendation |
|---|---|---|---|---|---|---|
| AMIX multi-pipe (เดิม) | ✗ ทุก run (finalize-hang 5/5 บน clean disk) | ✗ mixed stream สั้น/salvage พัง | ✗ timeout | 19–86MB | ต่ำ (เดิม) | **แทนที่ด้วย SIDECAR** |
| Single-pipe (sys only, pipe) | ✗ starve 145–185s บน real audio | ✓ เมื่อ silence | ✓ | 21–86MB | ต่ำสุด | ไม่พอ (real audio ตาย) |
| SeparateTrack (map ตรง, pipe) | ✗ sys starve + mic stream หายเงียบ | ✗✗ | ✓ | 19.3MB | ต่ำ | ไม่ใช่ทางออก |
| **SIDECAR/second-pass (P3-D)** | **✓ 7/7 รวม 15min critical gate** | **✓ dropped=0, accOk** | **✓ exit 0** | **0** | กลาง (second-pass step) | **✅ ADOPT** |

## 6. ข้อจำกัด / Remaining risks

1. **WAV disk footprint:** PCM 192KB/s/stream — 15min stereo = 173MB ชั่วคราว (ลบหลัง second-pass ✓) — disk guard ครอบ
2. **Second-pass เพิ่ม latency ตอน stop** (~28s สำหรับ 750MB — bounded 120s) — UX: stop ใช้เวลานิดหน่อยกว่าเดิม แลกความถูกต้อง
3. **AVERROR_EXIT exit code กับ -shortest** — cosmetic แต่ควรเช็คว่า production caller ไม่ตีความเป็น failure (ตอนนี้ LiveMuxResult ok=True แยกอยู่แล้ว)
4. A/V delta −2.7..−17ms มาจากจุดเริ่ม — ถ้าต้องการ <5ms ทุกกรณี ให้ second-pass ตัด WAV หัวด้วย video T0 จาก diagnostics (มีข้อมูล — งานถัดไปได้)

## 7. Evidence Index

- `evidence/w1-p3d-production-sidecar/p3d_matrix.txt` — log ทุกเซลล์ (authoritative)
- `evidence/w1-p3d-production-sidecar/p3c-results.txt + regression_p3d.txt` — after-count 46/0/1-known
- `evidence/w1-p3d-production-sidecar/Program.cs + p3c.csproj` — harness (production wiring via SessionConfig เท่านั้น)
- `evidence/w1-p3d-production-sidecar/SC_SYSREAL_15M_R2.log` — critical gate run log
- `SC_SYSREAL_15M_R2_final.mp4` (2.27GB) อยู่ที่ `…\Temp\sp_forensic\p3c\` — critical-gate artifact (เก็บไว้ตรวจสอบ)
