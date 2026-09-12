# M1-W1 — Timing Causality Report: First-Frame 38ms Anomaly vs `timelineStartTicks`

**Date:** 2026-09-11
**Lead forensic investigator:** M1-W1 (Timing Causality)
**Branch:** `Engine-Rebuild-Stabilization` (HEAD `cca2b2d8c2`, clean tree — no production file touched)
**Question:** พิสูจน์ causality (ไม่ใช่ correlation) ว่า first-frame anomaly `10ms ON → ~38ms` / `10ms OFF → ~28ms` เกิดจาก `timelineStartTicks` หรือไม่

---

## 0. Verdict (TL;DR)

**`timelineStartTicks` ไม่ใช่สาเหตุ — ถูกหักล้างระดับ mechanism (REFUTED).**

First interval 38ms ใน MP4 เป็น **structural artifact ที่ deterministic ภายใน FFmpeg** ของขั้น fragmented-MP4 mux (`+frag_keyframe+empty_moov+default_base_moof`) เมื่อมี audio encoder (AAC/MP3) อยู่ด้วย:

```
ΔPTS(MP4 frame#0 → frame#1) = 1 H.264 frame (16,667 µs, SPS 60fps)
                            + audio-encoder priming quantum (AAC@48k = 1024/48000 = 21,333 µs)
                            = 38,000 µs = 0.038000 s  (ตรงกับที่วัดได้ทุกไฟล์)
```

ประเด็น "10ms ON/OFF" ทั้งก้อนตั้งอยู่บนข้อผิดพลาดเลขคณิต: HEAD มี reserve = `Stopwatch.Frequency \ 10` = **100ms** (ไม่ใช่ 10ms) และตัวแปรนี้**ไม่มีเส้นทางใด ๆ** เข้าสู่ MP4 PTS เพราะเอนจินส่งเฉพาะ H.264 bytes เข้า named pipe (`FeedVideo(payload)`) ไม่มี timestamp เดินทางไปถึง mux เลย

ตัวเลข `OFF → 28.000ms` ของ GLM **ผลิตไม่ได้เชิงโครงสร้าง**: ไม่มี audio-encoder quantum ใดที่ 48kHz ให้ 28.000ms (AAC@48k = 38.000ms เสมอ) — และที่ HEAD flag `--no-delay` ของ phase4b scripts เป็น no-op (Program.vb ไม่ parse flag นี้)

**ไม่ BLOCKED** — map runtime → MP4 frame index สำเร็จ (§3)

---

## 1. Causal Chain (ของจริงทั้งเส้นทาง)

### 1.1 Timestamp semantics ต่อจุด (อ่านจากโค้ด HEAD ทั้งหมด)

| จุด | ค่า | โดเมน | เดินทางถึง MP4? |
|---|---|---|---|
| `_timelineStartTicks` (`CaptureSession.vb:279`) = `GetTimestamp() + Frequency\10` | Stopwatch ticks (QPC 10MHz) — **reserve = 100ms** | processing-clock session origin | **ไม่** |
| `_timelineStartQpc100ns` | ค่าเดียวกันในหน่วย 100ns | processing | **ไม่** |
| `CaptureTimeTicks` / `PresentationTimestampTicks` (`DdagrabBackend.vb:833-861,910-930`) | DXGI `LastPresentTime` (QPC 100ns, fallback = acquire QPC) | media-ish source present time | **ไม่** (ใช้เลือกเฟรมใน CFR loop เท่านั้น) |
| NVENC `inputTimeStamp` (`NvencEncoderBackend.vb:641-667`) | echo ของ PTT ข้างบน | metadata | **ไม่** (Annex-B bitstream ไม่มี PTS; sync encode 1-in-1-out) |
| `muxFeedTick / dequeueTick / cfrTick / selectedTick` (forensic logs) | wallclock processing logs | processing | **ไม่** |
| `LiveMuxSession.BuildArgs` (`LiveMuxSession.vb:212`) = `-f h264 -framerate {fps} -i pipe` + `-c:v copy` | ไม่มี `-use_wallclock_as_timestamps` | — | **จุดกำเนิด MP4 PTS** |
| MP4 video PTS | สังเคราะห์โดย ffmpeg h264 demuxer: µs-quantized index-based (16,667 µs/เฟรม) | **media time** | คือตัวมันเอง |
| MP4 audio PTS | sample-index @ 48k (AAC) + elst priming | **media time** | คือตัวมันเอง |

**FACT:** เอนจินควบคุม "เฟรมอะไร/กี่เฟรม/เนื้อหาไหน" ที่ไหลเข้า pipe แต่**ควบคุม PTS ไม่ได้แม้แต่บิตเดียว** PTS ทั้งหมดเกิดฝั่ง ffmpeg

### 1.2 กำเนิด 38ms (พิสูจน์ด้วย bisection matrix)

| ทดสอบ | inputs | ผลลัพธ์ first ΔPTS | สรุป |
|---|---|---|---|
| re-mux ES จาก A1.mp4 (file) | video เดี่ยว | uniform 16.667ms | 38ms ไม่ได้มาจาก H.264 bytes |
| stdin pipe (instant feed) | video เดี่ยว | uniform 16.667ms | ไม่ใช่ pipe/arrival |
| replica first-byte delay 100ms, **ไม่มี audio** | video paced | uniform 16.667ms | **ไม่ใช่ arrival delay** |
| **video file + audio file + frag flags** (`t0`) | dual-input | **0.038000** | 38ms ไม่ต้องมี runtime/เอนจินเลย |
| audio 44.1kHz + frag (`tg`) | dual-input | **0.039887** | = 16,667 + 23,220µs (1024/44100) ✓ สูตร |
| MP3@48k + frag (`tm_mp3`) | dual-input | **0.039688** | = 16,667 + 23,021µs (LAME priming 1105 samples) ✓ สูตร generic |
| plain MP4 (ไม่มี frag flags) + AAC (`ti`) | dual-input | uniform 16.667ms | ต้องมี `+frag_keyframe+empty_moov` |
| MOV + PCM (`tj`) | dual-input | uniform 16.667ms | PCM (ไม่มี encoder priming) → uniform |

**FACT:** 38ms เกิด**เฉพาะ** `+frag_keyframe+empty_moov+default_base_moof` + audio-encoder (มี priming quantum) — ค่า = 1 H.264 frame + audio priming, deterministic เป๊ะทุกรัน (µs-quantized, rescale สลับ 20001/20000 ticks ใน timebase 1/1200000)

**UNKNOWN (ปิดตายไม่ได้ แต่ไม่กระทบ verdict):** บรรทัดภายใน ffmpeg (demux.c `compute_pkt_fields` / mov.c fragmented `trun` duration) ที่ผสม priming quantum ของ audio เข้า duration ของ video sample แรก — เป็นพฤติกรรมภายใน ffmpeg ซึ่งเรา reproduce และ quantified ได้ครบแล้ว

---

## 2. Frame Mapping (runtime sequence → MP4 index)

### 2.1 Ground truth runtime (fresh run ON1, HEAD binary, 23:03:49)

```
23:03:49.041  common timeline armed: T0 = 1123852379606 (Stopwatch ticks / 100ns)   ← reserve 100ms เริ่มนับ
23:03:49.141  CFR TICK 1: target=1123852379606 == timelineStartQpc100ns == nextTick == _timelineStartTicks
              cfrTick = T0+1.7µs (loop wake) — ไม่มีเฟรม (จอนิ่ง, DXGI ไม่ส่ง)
23:03:49.158  CFR TICK 2: target = T0+166,667 (100ns) = T0+16.667ms ✓
23:03:49.174  CFR TICK 3: target = T0+333,334 = T0+33.334ms ✓
23:03:49.240+ FRAME 0-3 captured (console output ทำให้จอเปลี่ยน → DXGI เริ่มส่ง)
23:03:49.243  packet แรก: source PTT = 1123853335432 = T0+95.58ms → FeedVideo
```

CFR telemetry ON1: `ticks=180, selectedSources=131, maxSourceGap=73.638ms, avgEncode=2.88ms, maxEncode=14.655ms`
→ **source เป็น VFR โดยธรรมชาติ (DXGI)** — แต่ MP4 ไม่รู้เรื่อง

### 2.2 ตาราง mapping (ON1)

| MP4 index | MP4 pts_time | MP4 Δ ก่อนหน้า | runtime source (PTT − T0) | source Δ | หมายเหตุ |
|---|---|---|---|---|---|
| #0 | 0.000000 | — | **+95.58ms** | — | packet แรกที่ FeedVideo (tick ~6, เฟรมแรกที่ DXGI ส่ง) |
| #1 | 0.038000 | **+38.000ms** | +117.37ms | 21.79ms | Δsource ≠ ΔMP4 → timeline แยกขาด |
| #2 | 0.054668 | +16.668ms | +141.83ms | 24.46ms | Δsource ≠ ΔMP4 |
| #3 | 0.071334 | +16.666ms | +141.83ms (dup) | 0ms | static screen → CFR re-encode เฟรมเดิม |
| #4 | 0.088002 | +16.668ms | +172.46ms | 30.62ms | |

**FACT:** MP4 frame #N = packet ที่ถูก FeedVideo เป็นอันดับที่ N+1 (FIFO, `-c copy`, ไม่มี B-frames) — จุดเชื่อมเดียวระหว่าง runtime กับ MP4 คือ "จำนวน/ลำดับ/เนื้อหา" ไม่ใช่เวลา ทุกจุดเวลา (capture ts, CFR target, muxFeedTick) หลุดไปที่ logs หมด

### 2.3 ΔPTS(frame0→frame1) ตามคำสั่ง A/B

| Run | ΔPTS(f0→f1) | ΔPTS(f1→f2) | หมายเหตุ |
|---|---|---|---|
| ON1 (fresh, HEAD) | **0.038000** | 0.016668 | |
| ON2 (fresh, HEAD) | **0.038000** | 0.016668 | |
| ON3 (fresh, HEAD) | **0.038000** | 0.016668 | |
| A1/A2/A3, B1s/B5s/B10s, C1-C5 (GLM-era artifacts, 17:07-17:08) | **0.038000** | 0.016668 | ทุกไฟล์เท่ากันเป๊ะ |
| T0/T-g/T-h/tm (ffmpeg-only, **ไม่มีเอนจิน**) | 0.038000 / 0.039887 / 0.038000 / 0.039688 | uniform | structural proof |

**ON − OFF:** ทำการทดลอง OFF ที่ถูกต้องไม่ได้เพราะ (a) แก้โค้ด production = ฝ่าฝืนข้อห้าม, (b) flag `--no-delay` ที่ GLM ใช้เป็น no-op ที่ HEAD (Program.vb ไม่ parse — FACT จากโค้ดและ git history) → "OFF แบบ GLM" คือ ON คนละชื่อ **การทำนาย (ความมั่นใจระดับ FACT จากสูตร): การทดลอง OFF ที่แท้จริงจะได้ 0.038000 เท่าเดิม** เพราะตัวแปรที่หายไป (`timelineStartTicks` reserve) ไม่มีเส้นทางเข้า PTS และ `OFF=28.000ms` ผลิตไม่ได้ด้วย audio quantum ใด ๆ ที่ 48kHz

---

## 3. Timing Table (รวมทุกโดเมน)

| ปริมาณ | ค่า | โดเมน |
|---|---|---|
| Reserve ใน `_timelineStartTicks` (HEAD) | `Frequency \ 10` = 1,000,000 ticks = **100ms** (พิสูจน์จาก ON1: armed log .041 → tick1 .141) | processing |
| CFR tick interval (60fps) | 166,667 (100ns) = 16.667ms | processing |
| First CFR tick target | = T0 เป๊ะ (media-time origin) | processing→media boundary |
| Source VFR gap จริง (ON1) | max 73.638ms, typical 21-31ms | source media-ish (DXGI LastPresentTime) |
| MP4 ΔPTS(f0→f1) | **38.000ms คงที่** (ทุก run, ทุก config เดียวกัน) | media (ffmpeg) |
| MP4 ΔPTS(f1→fN) | 16.667ms (µs-quantized สลับ 20001/20000) | media (ffmpeg) |
| องค์ประกอบ 38.000ms | 16,667µs (H.264 frame) + 21,333µs (AAC priming 1024@48k) | media (ffmpeg) |

---

## 4. Arithmetic Verification

1. `1/60 s = 16,666.67 µs → µs-quantized 16,667`
2. `1024/48000 s = 21,333.33 µs → 21,333`
3. `16,667 + 21,333 = 38,000 µs = 0.038000 s` — ตรงกับค่าที่วัดทุกไฟล์ (45,600 ticks @ 1/1200000)
4. ตรวจทางอ้อม: A1 duration `10.0382 s` = `0.038 + 599×16.6667ms + 16.6667ms` ✓; `avg_frame_rate = 3005000/50191 = 59.868 = 601/10.0382` ✓
5. ตรวจข้ามอัตรา: 44.1kHz → `16,667+23,220 = 39,887µs` = 0.039887 (วัดได้เป๊ะ) ✓; MP3@48k (LAME priming 1105 samples) → `16,667+23,021 = 39,688µs` = 0.039688 (วัดได้เป๊ะ) ✓

---

## 5. Contradictions (ในหลักฐานเดิมของ GLM)

1. **"10ms delay" เป็นข้อผิดพลาดเลขคณิตตั้งแต่ต้น:** `38ms-Gap-Analysis.md` เรียก `Stopwatch.Frequency \ 10` ว่า "~10ms" — ที่ QPF=10MHz คือ **100ms** (ยืนยัน runtime: armed→tick1 = 100ms พอดีบน wallclock ของ ON1)
2. `PHASE4-FINAL-REPORT.md` อ้างโค้ด `_timelineStartTicks = timelineStartQpc100ns + 100000` — **ไม่มีอยู่ใน repo** (grep ทั้ง repo: มี assignment เดียวที่ line 279)
3. แถว "Timeline Start Ticks: ON 910,568,635,398 / OFF 910,568,625,398 (Δ=10,000,000)": Δ 10,000,000 ticks @10MHz = **1.0 วินาที** (ไม่ใช่ 10ms) และสองค่านี้ห่างกันเพียง 1s แต่ log ที่อ้างห่างกัน 3.83s → ตัวเลขตั้งต้นสอดไม่เข้ากันเอง
4. Log excerpt "Baseline (ON)" ของ GLM ละเมิด invariant ของ HEAD: `targetQpc100ns < timelineStartQpc100ns` และ `nextTick ≠ _timelineStartTicks` ที่ **TICK 1** — เป็นไปไม่ได้ตามโค้ด (fresh ON1 ของเราแสดงรูปแบบที่ถูก: ทั้งสี่ค่าเท่ากันเป๊ะที่ tick 1)
5. Excerpt เดียวกันถูกนำไปแปะซ้ำทั้งในหมวด "No Delay Test" และหมวด "60 FPS Test" — recycled
6. phase4b scripts ส่ง `--duration/--output/--log/--evidence/--no-delay` ซึ่ง **Program.vb ไม่เคย parse** (git history ทั้งหมด) → การรันจริงคือ Phase-12b matrix เต็มรูปแบบ 9 เซสชัน config เดียวกัน, `--no-delay` = no-op
7. **ไม่มี artifact ของ phase4b (phase4b-on*/off* .mp4/.log) อยู่ใน repo เลย** — MP4 ที่เหลือใน bin (17:07-17:08) คือของ Phase-12b matrix ทั้งหมด ซึ่งทุกไฟล์ก็มี 0.038000 เหมือนกัน
8. "FPS matrix 38ms ทุกอัตรา" ของ GLM จริง ๆ สอดคล้องกับ **constant artifact** (สูตรของเรา) — ไม่ใช่หลักฐาน causal ของ timeline delay
9. CSV forensic (`test-recordings/timing-analysis.csv`, `forensic-timing-data.csv`) มีแค่ header ไม่มี data — ไม่เคยมีการ extract หลักฐานจริง

---

## 6. Final Verdict (จำแนกตามที่ mission กำหนด)

**FACT** (มีหลักฐาน/ทดซ้ำได้):
- ΔPTS(f0→f1) ใน MP4 ของ pipeline นี้ = **0.038000 s คงที่** — ON1-3 fresh, A1-A3/B*/C* เดิม, และ ffmpeg-only reproduction
- 38ms เกิดใน stage **fragmented-MP4 mux ของ ffmpeg** และค่าตามสูตร `1 H.264 frame + audio-encoder priming` (พิสูจน์ด้วยการเปลี่ยน audio rate/codec — ตรงเป๊ะทั้งสองจุด)
- ไม่เกี่ยวกับ arrival timing / wallclock / pipe (ทด delay 100ms, instant feed, file — uniform หรือตามสูตรเสมอ)
- เอนจินส่งเฉพาะ H.264 bytes เข้า mux; ไม่มี timestamp ใดของเอนจินเดินทางเข้า MP4
- Reserve จริงของ `timelineStartTicks` ที่ HEAD = **100ms** (ไม่ใช่ 10ms)
- `--no-delay` ที่ HEAD = no-op; ไม่มี artifact OFF จริงใน repo; ตารางของ GLM ขัดแย้งตัวเอง (§5)

**HYPOTHESIS** (สอดคล้องกับหลักฐาน แต่ยังไม่รัน OFF จริง):
- การทดลอง OFF ที่แท้จริง (แก้ reserve ใน build ทดลอง แล้วคืนค่า) จะได้ ΔPTS(f0→f1) = 0.038000 เท่าเดิม — ความมั่นใจสูงมากจาก mechanism (ไม่มี causal path) แต่ตามพิธีกรรม A/B ยังไม่ได้รัน
- ถ้าเปลี่ยน audio codec/rate (เช่น AAC@96k → 27,333µs) gap จะเปลี่ยนตามสูตร — ถ้าอยาก "กำจัด" 38ms ให้ทำที่ **args ของ LiveMux** (เช่น ไม่ใช้ frag ระหว่างพักหน้าจอแรก หรือ pre-roll audio) — แต่**ห้ามแตะตามข้อห้ามของ mission นี้** (เป็น production timing ของ mux)

**UNKNOWN:**
- บรรทัดภายใน ffmpeg ที่ merge priming quantum เข้า trun duration แรก (reproduce/quantify ครบแล้ว — ไม่กระทบ verdict)
- ต้นทางที่แท้จริงของตัวเลข "28ms" ในตาราง GLM (ผลิตซ้ำไม่ได้จาก pipeline นี้ — ไม่มี artifact รองรับ)

**คำตอบของ mission:** first-frame anomaly 38ms **ไม่ได้** เกิดจาก `timelineStartTicks` (causality ถูกหักล้างระดับ mechanism โดยการ reproduce พฤติกรรมเดียวกันบน ffmpeg-only ที่ไม่มี timelineStartTicks, ไม่มี capture, ไม่มี CFR, ไม่มี NVENC) และเป็น constant ของ container — **ไม่ใช่หลักฐาน VFR** (consistent กับ known-evidence เดิม)

---

## 7. Next Experiment (ถ้าต้องการปิด HYPOTHESIS ตามพิธีกรรม)

ต้องได้รับอนุมัติก่อน (แตะ build ทดลองชั่วคราว):
1. สำเนา `CaptureSession.vb` → เปลี่ยน line 279 เป็น `Stopwatch.GetTimestamp()` (no reserve) → build → รัน 3 เซสชัน (`--videocheck --seconds 3`) → probe ΔPTS(f0→f1) → **คืนไฟล์เดิม byte-for-byte** → `git status` ต้อง clean
2. **Prediction:** ทั้ง 3 run ได้ 0.038000 → ปิดเคส ON−OFF = 0 ระดับ FACT
3. (ทางเลือกที่ไม่แตะโค้ด) รัน `record-phase4b-off1.bat` ตามตัว เพื่อบันทึกว่ามันกลายเป็น Phase-12b matrix + ได้ไฟล์ที่ยังมี 0.038000 — เอกสารหลักฐานว่า OFF-แบบ-GLM เป็น no-op

## Evidence Index

- Fresh ON runs + logs: `evidence/m1-w1-causality/on{1,2,3}.mp4`, `on{1,2,3}.log`
- ffmpeg-only bisection: `t0_vfile_afile.frag.mp4` (38.000), `tg_441.frag.mp4` (39.887), `tm_mp3.frag.mp4` (39.688), `ti_plain.frag.mp4`/`tj.mov` (uniform), `replica.ps1`
- GLM-era artifacts ที่ probe แล้ว: `CaptureEngine.Recording.ConsoleDriver/bin/Debug/net10.0-windows/{A1,A2,A3,B1s,B5s,B10s,C1..C5}.mp4` (ทุกไฟล์ 0.038000)
- โค้ดอ้างอิง: `CaptureEngine.Recording/CaptureSession.vb:279,663-731,807-819`, `CaptureEngine.Video.Ddagrab/DdagrabBackend.vb:833-861,910-930`, `CaptureEngine.Encoder.Nvenc/NvencEncoderBackend.vb:641-673`, `CaptureEngine.FFmpegBackend/LiveMuxSession.vb:207-255`
