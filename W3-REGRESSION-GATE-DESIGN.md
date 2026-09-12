# W3 — REGRESSION GATE DESIGN (timing / FPS round)

> **STATUS 2026-09-12: IMPLEMENTED.** Suite = `Tester\test\Engine\TimingGate\Engine.TimingGate.Tests.vbproj`,
> registered in `scripts\build-all.ps1` `$suites`; canonical one-command runner = `scripts\run-recording.ps1` (§10).
> Latest gate result: `PASS=18 FAIL=1 BLOCKED=6` — พื้นที่ mission เขียวครบ ยกเว้น **240 FPS ยังแดง**
> จาก false-success race ที่เหลือหลัง fix M2/W1 (รายละเอียดและ root-cause ใน §6 — จงใจคงสถานะแดงไว้
> เป็นสัญญาณให้เจ้าของตามหลักการ "ห้าม xfail กลืนผล") §4-§6 เป็น design ตามที่ implement จริง

> 2026-09-11 · **ไม่แก้ production จากฝั่งงานนี้, ไม่ commit** (git ไม่มีบนเครื่องนี้; เอกสาร + artifacts ทั้งหมดเป็น untracked files
> — หมายเหตุ: มี workstream อื่น (M2/W1) แก้ `CaptureEngine.vb` เข้ามากลางงาน 2026-09-12 ซึ่ง gate ใช้ประเมินผลตามจริง)
> พื้นฐาน evidence: `test-recordings\fps-matrix\FPS-MATRIX-REPORT-M2W3.md` (M2-W3 matrix วัดจริง 2026-09-11)
> ตัวย่อ: T = target Δ = 1000/fps ms, tick = 1/15360 s ≈ 65.104 µs (MP4 timebase ที่ engine ใช้จริง)

---

## 1. Survey — test infrastructure ที่มีอยู่ (ของจริงทั้งหมด)

### 1.1 Suite registry ที่รันรวมกัน (`scripts\build-all.ps1 -RunTests`, `dotnet run -c Release` ต่อ suite)

| Suite | เนื้อหา | ผ่านล่าสุด (audit 2026-09-07, Machine B) |
|---|---|---|
| CaptureEngine.Tests (Core) | lifecycle state machine ของ engine ตัว foundation | 14/14 |
| CaptureEngine.FFmpegTests | LiveMuxSession, SyncMath, sidecar, MediaValidation (70) | 70/70 |
| CaptureEngine.FrameContractTests | frame ownership/availability contract | 8/8 |
| CaptureEngine.ConfigTests | ConfigLoader/Migrator/Validator | — |
| CaptureEngine.Encoder.Tests | NVENC param/fault (hardware-gated skip) | 64 pass / 5 skip |
| CaptureEngine.Video.Tests | Ddagrab lifecycle/ownership (hardware-gated) | 38 pass / 28 skip |
| CaptureEngine.Recording.Tests | RecordingEngine/CaptureSession units (43) | 43/43 |

Suite ที่อยู่นอก build-all (เจ้าของรันเองตาม audit): **Engine.ConfigTruth.Tests** (52; รันได้บน Linux, no hardware), **Engine.Concurrency.Tests** (20 pass / 7 skip × 10 รอบ), Gallery/Overlay/Notifier/Duluka/Hub-Boundary, NVIDIA.Soak.

**CI จริง = ไม่มี** สำหรับ engine (`.github\workflows\` มีแค่ `deploy-web.yml`) → gate ต้องพึ่ง runbook ต่อเครื่อง ไม่ใช่ GitHub Actions

### 1.2 รูปแบบ (conventions) ที่ gate ใหม่ต้องเดินตาม — พิสูจน์แล้วทั้งหมด

- **Runner**: console `TestRunner.RunTest(name, action)` + `Assert(cond,msg)`; exit 0 = ผ่าน, 1 = fail, 2 = setup fail (ConfigTruth/Concurrency ใช้รูปเดียวกัน)
- **Skip อย่างตรงไปตรงมา (C/4)**: `SkipException` → SKIP (ไม่ใช่ PASS, ไม่กลืน), ไม่กระทบ exit code; `HardwareGate.EnsureProbed()` ยิง DdagrabBackend จริง 1 ครั้ง/กระบวนการ → ไม่มี NVIDIA = M1-*/M2-* SKIP พร้อมหลักฐานจาก backend เอง
- **Environment split ใน assertion**: media-validity เช็คเฉพาะเมื่อ `HardwareGate.NvidiaAvailable` (H1-B/H2-C pattern) — environment ล้ม ≠ production bug
- **Helper-exe seam** (G2/F03): `CaptureSettings.FFmpegPath = Environment.ProcessPath` → ตัว suite เองเป็น fake ffmpeg ที่คุมได้ด้วย env `LMHLP_SLEEP/EXIT/SRC/COPYTO/MUX_EXIT/MUX_REAL/PROBE_REAL/PROBE_EXIT` → inject ความล้มแบบ deterministic โดยไม่ต้องมี GPU
- **MediaAssert** (F-02): file exists / size>0 / probe ผ่าน (ffmpeg -i) / Duration>0 / stream มี-ไม่มีตามคาด / scoped orphan check (Win32_Process CommandLine marker)
- **Sandbox hygiene**: temp dir prefix `engine-concurrency-tests-*`, stale sweep 6h, cleanup warning ไม่ flip verdict
- **Provenance banner**: Directory.Build.targets stamp `BuildUtc`+`SourceRevision` ให้ assembly `*Tests` ทุกตัว (กัน `--no-build` ของเก่าอ้างโค้ดปัจจุบัน)
- **Regression-first culture**: F03-B เขียนก่อนแล้ว FAIL กับโค้ดเก่าก่อน fix — gate มีสิทธิ์ติดสีแดงวันแรก ถ้ามันจับ bug จริง (§6)
- **ปัญหาที่ยืนยันแล้วใน W3 นี้**: `run-fps-matrix.bat`/`record-*.bat` ยิง flag ที่ ConsoleDriver ไม่รองรับ (ล้าสมัย) — ห้ามใช้เป็น gate

### 1.3 แหล่ง timing ที่วัดได้แล้ว (M2-W3, พร้อม baseline)

- `Tester\test\Engine\FpsMatrix\` — harness ยิง legacy engine จริง (`NVIDIA_Capture.CaptureEngine`) ต่อ 1 การอัด
- `test-recordings\fps-matrix\analyze-pts.ps1` — ffprobe `pts_time` ทุกเฟรม → ΔPTS ครบสถิติ (first/median/mean/P95/P99/min/max, dup, neg, monotonic, effective FPS)
- Baseline ที่วัดได้ (legacy+QSV, 5s×2 ต่อโหมด): กริด 30/60/120/144 **เป๊ะทุกค่า** (Δ=เป้าหมาย, dup=neg=0, eff FPS = เป้าหมาย .000); 240 = `h264_qsv` ปฏิเสธเปิดตัว (`Current frame rate is unsupported`) → exit −40 → ไฟล์ 0-byte **และ engine ประกาศสำเร็จหลอก** (stop จาก HasError คืน True, `RecordingStopped` ยิงตาม `File.Exists` ของไฟล์ขยะ)

### 1.4 Coverage map — 8 หัวข้อที่ gate ต้องครอบ

| # | หัวข้อ W3 | มีอยู่แล้วที่ไหน | ช่องว่าง → งาน W3 |
|---|---|---|---|
| 1 | FPS grid 30/60/120/144/240 | ❌ ไม่มี suite ไหน assert กริด PTS เลย (MediaAssert เช็คแค่ container/stream) | **W3-L2** grid gate ต่อโหมดต่อ lane |
| 2 | first-frame anomaly classification | ❌ (มีแต่ instrumentation plan ใน 38ms-Gap-Analysis) | **W3-L0-A3 + W3-L2** classifier ใน PtsAnalyzer |
| 3 | PTS monotonicity | ❌ | **W3-L0-A2 + W3-L2-A5** |
| 4 | frame count | ~ บาง (MediaValidation ใน FFmpegTests เช็ค nb_frames ของ media สังเคราะห์) | **W3-A3** count vs target fps |
| 5 | encoder failure | บางส่วน: NvencNativeFaultTests (NVENC, NVIDIA-gated), G2 (helper exit) | **W3-L1-B1 + W3-L2-Q6/N7** ความล้มแบบ encoder-open-reject ของจริง (QSV 240) + ทาง single-process |
| 6 | FFmpeg failure | ดีฝั่ง two-process (G2-A/B/C, F03-A) | **W3-L1-B1/B4** ฝั่ง single-process video-only + รูป exit −40 |
| 7 | false-success | ดีฝั่ง two-process (F03-B, no-audio-rename fix) | **W3-L1-B3 = KNOWN-RED**: Step-4 (`CaptureEngine.vb:567`) ประกาศ saved จาก `File.Exists` เท่านั้น → ไฟล์ 0-byte ผ่าน; ยืนยันแล้วด้วย QSV-240 จริง |
| 8 | restart / start-stop | H2-C (6 วงจร legacy), M2-STRESS (50 วงจร native), G3-B/D, Phase-12b Test C | **W3-A9**: restart **ข้าม FPS** (fps-switch) ทั้ง legacy + native (per-session NVENC rebuild), restart-after-fault |

---

## 2. สถาปัตยกรรม gate — 3 ชั้น + capability probe

```
Engine.TimingGate.Tests  (console suite ใหม่, net10.0-windows10.0.26100.0)
├─ L0  deterministic, ไม่ใช้ hardware/ffmpeg  — คณิตศาสตร์ analyzer + config contract   (<1 s)
├─ L1  engine จริง + helper-exe seam          — failure/restart/honesty ทุกเครื่อง      (~1 นาที)
└─ L2  capture+encode จริง, hardware-gated   — กริด PTS ต่อโหมด ต่อ lane              (~1.5–3 นาที)
     ├─ L2-NVENC: native RecordingEngine (Ddagrab+NVENC)  — NVIDIA-gated (HardwareGate)
     └─ L2-QSV:   legacy CaptureEngine ddagrab+qsv        — Intel/QSV-gated (QsvGate probe ใหม่)
```

**ทำไมแบ่งแบบนี้**: บทเรียนจาก C/4 และ M2-W3 — assertion เดียวที่ "จริงทุกเครื่อง" ไม่มีอยู่; สิ่งที่จริงได้คือ **คาดหวังต่อ (lane × mode) จาก probe** แล้ว assert สัญญา (contract) ของคาดหวังนั้นอย่างตรงไปตรงมา (SKIP ถ้าล่ม ไม่ปลอม PASS)

### 2.1 QsvGate — probe ใหม่ (เลียนแบบ HardwareGate, ไม่แตะ production)

- **`QsvGate.EnsureProbed()`**: `ffmpeg -v error -f lavfi -i testsrc=duration=0.2:size=320x240:rate=15 -c:v h264_qsv -f null -` → exit 0 = QSV ใช้ได้; ผล cache ต่อกระบวนการ
- **`QsvGate.ModeSupported(fps)`**: probe เดียวกันแต่ `rate=<fps>` (240 → `Current frame rate is unsupported` บน Intel UHD เครื่องนี้ — พิสูจน์แล้วใน M2-W3 isolation)
- ค่าคาดหวังต่อเซลล์ (lane × mode):

| lane \ mode | 30 | 60 | 120 | 144 | 240 |
|---|---|---|---|---|---|
| L2-NVENC (NVIDIA) | GRID | GRID | GRID | GRID | probe ตัดสิน: GRID หรือ HONEST_FAILURE |
| L2-QSV (Intel) | GRID | GRID | GRID | GRID | **HONEST_FAILURE** (baseline จริง 2/2 + isolation) |
| ไม่มี HW encoder เลย | SKIP | SKIP | SKIP | SKIP | SKIP |

- **สองสัญญา (contract) ที่ assert ได้ (และต้อง)**:
  - `GRID` → ไฟล์ต้องผ่าน assertion กริด PTS ทั้งหมด (§4)
  - `HONEST_FAILURE` → โหมดนั้นล้ม **อย่างตรงไปตรงมา**: error event ออกสาธารณะ, ไม่มี RecordingStopped หลอก, ไฟล์ทิ้งต้องไม่ถูกประกาศเป็น saved, engine กลับ Idle และใช้ต่อได้ (ปัจจุบัน QSV-240 ละเมิดข้อประกาศ = KNOWN-RED §6)

---

## 3. PtsAnalyzer / PtsAssert — แกนกลาง gate (port จาก analyze-pts.ps1 ให้เป็น VB ใน suite)

- **อินพุต**: `ffprobe -v error -select_streams v:0 -show_entries frame=pts_time -of csv=p=0 <file>` (ffprobe อยู่ใน API-Core เดียวกับ ffmpeg ที่ suite ใช้; ถ้าไม่เจอ → SkipException "analyzer unavailable") + ชื่อโหมด/เป้าหมาย
- **สถิติ (นิยามตายตัว เหมือน M2-W3)**: Δ[i]=pts[i+1]−pts[i]; first/median/mean/min/max/P95/P99 (interp), effFPS=(n−1)/(pts[n−1]−pts[0]); frames=n
- **Classifier ต่อไฟล์** (คืน class เดียวที่แย่ที่สุด + รายการเตือน):
  - `NO_FILE` / `UNDECODABLE` (probe ล้ม / frames<2)
  - `NEGATIVE_DELTA` (มี Δ < −1 µs)
  - `DUPLICATE_PTS` (มี |Δ| ≤ 1 µs)
  - `NON_MONOTONIC` = NEGATIVE หรือ DUPLICATE
  - `START_OFFSET` (pts[0] > 2 ticks — timeline ไม่เริ่ม 0)
  - `FIRST_GAP` (firstΔ > T + max(3 ticks, 12%·T) — เกณฑ์ implement จริง: ต่ำกว่า 1.5×T เพื่อให้
    first-gap ระดับ 38ms ถูกจับได้แม้ที่ 30fps โดย tick quantization ±1 ของ 144fps ไม่หลอก)
  - `RATE_DRIFT` (|medianΔ−T| > max(0.05×T, 1 tick) หรือ |effFPS−fps| > 2%)
  - `TIMEBASE_SAWTOOTH` (Δ สลับใน ±1 tick รอบ T — **คาดหวัง** เมื่อ 15360 mod fps ≠ 0 เช่น 144) — ไม่ใช่ความผิดพลาด (defect)
  - `GRID_OK` (ไม่เข้าข้ออื่น)
- L0 จะทดสอบ analyzer ด้วยเวกเตอร์สังเคราะห์ (กริดสมบูรณ์, มี dup, มี negative, sawtooth 144, first-gap 38ms-shape, start-offset) — ทำให้ math ของ gate **พิสูจน์ได้โดยไม่ต้องอัดจริง**

---

## 4. EXACT ASSERTIONS (สัญญา (contract) ที่ gate บังคับ — หมายเลขอ้างอิงได้)

ทุก assertion ระบุ: เงื่อนไข → ต้องจริง ไม่งั้น FAIL พร้อมข้อความที่มีหลักฐานในตัว (ค่าที่วัด + คาดหวัง + path ไฟล์)

### กลุ่ม A — กริด PTS (ใช้กับทุกไฟล์ผลลัพธ์ใน L2 และ L1-B4)

- **W3-A1 (มีผลลัพธ์จริง)**: `File.Exists(out) ∧ size>0 ∧ ffprobe เปิดได้ ∧ frames ≥ 2` — ไม่ถึง = FAIL `NO_FILE/UNDECODABLE` (ยกเว้นเซลล์ HONEST_FAILURE — ใช้กลุ่ม D แทน)
- **W3-A2 (monotonic + ไม่ซ้ำ)**: ∀i: Δ[i] > 1 µs — ครอบ `NEGATIVE_DELTA` (PTS ย้อน) และ `DUPLICATE_PTS` (PTS ซ้ำ) ในข้อเดียว; จำนวนทั้งสองต้อง = 0 (M2-W3 baseline: 0 ทุกโหมด)
- **W3-A3 (frame count)**: `ceil(0.7 × fps × dur) ≤ frames ≤ ceil(1.5 × fps × dur)` โดย dur = ความยาว container (s) — CFR ที่ถูกต้องให้ frames ≈ fps×dur (baseline: เยื้อง +0.3–0.7% จากจังหวะหยุด); ขอบล่างจับ "อัดแล้วเฟรมหาย", ขอบบนจับ "feed ช้า/เทียบเทียมซ้ำเกิน"
- **W3-A4 (cadence = เป้าหมาย)**: `|medianΔ − T| ≤ max(0.05×T, 1 tick)` ∧ `|meanΔ − T| ≤ max(0.05×T, 1 tick)` ∧ `|effFPS − fps| ≤ 0.02×fps` (baseline: คลาดเคลื่อน 0.0% ทุกโหมดที่ผ่าน)
- **W3-A5 (first-frame classification)**: class(firstΔ, pts[0]) ∈ {`GRID_OK`, `TIMEBASE_SAWTOOTH`} สำหรับโหมดที่คาดหวัง GRID; เจอ `FIRST_GAP`/`START_OFFSET` = FAIL **พร้อมทำเครื่องหมาย (tag)** ชื่อ class ในข้อความ (แยก diagnose ออกจากกัน — ห้ามรายงานรวมว่า "fps พัง") — หมายเหตุ: ค่า baseline ปัจจุบัน = GRID_OK ทุกโหมด; กรณี 38ms (first-gap) เมื่อแก้/เจอจริง ต้องโผล่เป็น FAIL ของข้อนี้ ไม่ใช่สรุปจากสถิติเฉลี่ย
- **W3-A6 (duration sanity)**: `0.7 × reqSec ≤ dur ≤ 1.5 × reqSec` (baseline: 5.17–5.22 สำหรับ req 5s)
- **W3-A7 (container truth)**: `avg_frame_rate` ของ stream = `fps/1` (หรือ ±2%) ∧ ไม่มี stream เสียงเมื่อสั่ง video-only (MediaAssert.AssertValidMp4 ใช้ต่อ)

### กลุ่ม B — encoder/FFmpeg failure (L1, helper-exe seam แบบ single-process video-only)

- **W3-B1 (encoder-open reject propagation)**: helper ตั้งค่าให้ตายก่อนเขียน output (เลียน exit −40 / "Could not open encoder") → ต้องได้: `ErrorOccurred` ขึ้น **เพียงครั้งเดียว** ∧ ข้อความระบุสาเหตุจาก ffmpeg ∧ State เข้า `HasError` (ถ้ายังอัดอยู่) หรือกลับ `Idle` ภายใน ≤ 5 s ∧ **ไม่มี `RecordingStopped`** ∧ orphan helper = 0 (scoped)
- **W3-B2 (stop-after-fault เป็นเอกภาพ (idempotent))**: เรียก `StopRecordingAsync()` หลังความล้ม (G2-F ฝั่ง single-process) → คืนได้ทั้ง True/False **แต่** ต้องไม่ยิง event ซ้ำ ∧ จบที่ `Idle` ∧ เรียกซ้ำอีกครั้งไม่เกิดอะไรเพิ่ม
- **W3-B3 (false-success: KNOWN-RED — ดู §6)**: helper เขียนไฟล์ 0-byte แล้วตาย → จบ stop: ถ้าไฟล์ไม่ผ่าน W3-A1 ต้อง **ไม่มี `RecordingStopped`** ∧ มี honesty error ≥ 1 (`"not saved"`-family) — **รู้ล่วงหน้าว่า FAIL กับโค้ดปัจจุบัน** (`CaptureEngine.vb:567` เช็คแค่ `File.Exists` เฉย ๆ)
- **W3-B4 (graceful path ยังถูกต้อง (green twin))**: helper เลียน 'q' ปกติ + stream จริง (delegate ไป ffmpeg จริง `LMHLP_*` ตาม F03 pattern, video-only) → `RecordingStopped` ขึ้นครั้งเดียว ∧ ไฟล์ผ่านกลุ่ม A ที่ fps ของ helper
- **W3-B5 (restart-after-fault)**: หลัง B1 → `StartRecordingAsync` ครั้งถัดไปต้องสำเร็จและได้ไฟล์ผ่านกลุ่ม A (เทียบเท่า M1-T* แต่ฝั่ง legacy single-process)

### กลุ่ม C — restart / start-stop ข้าม FPS

- **W3-C1 (legacy fps-switch)**: 3 วงจร: อัด 1.2s ที่ fps₁ → stop → **ทันที** อัดที่ fps₂ ≠ fps₁ (สลับ 15→30→15) → ไฟล์ทุกไฟล์ผ่านกลุ่ม A **ที่ fps ของมันเอง** ∧ State = Idle ระหว่างวงจร ∧ orphan กลับ baseline ทุกวงจร (scoped) ∧ engine ใช้ซ้ำได้จนจบ
- **W3-C2 (native fps-switch — NVIDIA-gated)**: `RecordingEngine.Initialize(fps=60)` → session A ที่ 30 → session B ที่ 120 (per-session NVENC rebuild path, `RecordingEngine.vb:229-300`) → ไฟล์ A กริด 30, ไฟล์ B กริด 120 (จับ bug คลาส "120 เฟรมถูกตีความเป็น 60") — ใช้ `--mismatch-fps` hook ของ ConsoleDriver เป็นรูปแบบอ้างอิง (แต่รันผ่าน engine ตรง ๆ ใน suite)
- **W3-C3 (native restart matrix — NVIDIA-gated)**: 5 × 3s ทันทีต่อกัน (Phase-12b Test C แบบ suite) + 1 วงจรที่ fps ต่างกัน → ทุกไฟล์ผ่านกลุ่ม A ∧ orphan = 0 ∧ ไม่มี state ค้างหลัง Dispose

### กลุ่ม D — HONEST_FAILURE (โหมดที่ encoder ปฏิเสธ, เช่น QSV-240 บนเครื่องนี้)

- **W3-D1**: การอัดจบแบบล้ม → `ErrorOccurred` ≥ 1 และข้อความอ้างถึง error ของ ffmpeg/encoder จริง (ไม่ใช่ error ปลอม ๆ ของ engine เอง)
- **W3-D2**: `RecordingStopped` **ต้องไม่ถูกยิง** สำหรับไฟล์ที่ W3-A1 ไม่ผ่าน (= ข้อนี้คือ W3-B3 ในบริบท hardware จริง — ตอนนี้ RED บนเครื่อง Intel)
- **W3-D3**: State ต้อง deterministic หลัง stop (Idle) ∧ engine ใช้ต่อได้ (เริ่มโหมดที่รองรับ เช่น 60 ได้ปกติ)
- **W3-D4**: ห้าม assert ว่า "ต้องล้ม" จากสมมติฐาน — คาดหวังมาจาก `QsvGate.ModeSupported(240)` เท่านั้น; ถ้าวันหนึ่ง driver เปลี่ยนแล้ว 240 ผ่าน → เซลล์เปลี่ยนเป็น GRID อัตโนมัติ (gate ไม่ขัดขวางการแก้ไข)

### กลุ่ม E — สุขภาพ suite (เดินตาม convention เดิม)

- orphan ffmpeg ระดับ suite (ก่อน/หลัง), sandbox sweep + cleanup-warning isolation, provenance banner, exit code 0/1/2, SKIP ไม่กระทบ exit

---

## 5. รายการเทสที่จะลงทะเบียน (ชื่อจริงใน suite ใหม่ `Engine.TimingGate.Tests`)

**L0 — TimingGateOff­line (ไม่ใช้ ffmpeg)**
1. `W3-L0-A1: PtsAnalyzer known-vectors — perfect grids at 30/60/120/144/240` 
2. `W3-L0-A2: monotonicity — dup/negative/non-increasing synthetic streams classified`
3. `W3-L0-A3: first-frame classes — GRID_OK / FIRST_GAP(38ms-shape) / START_OFFSET / TIMEBASE_SAWTOOTH(144)`
4. `W3-L0-A4: percentile math (P95/P99) + effFPS vs known vectors`
5. `W3-L0-A5: CaptureSettings.Validate fps bounds — 1..240 accepted, 241 rejected (contract ที่โหมด 240 พึ่งพา)`

**L1 — TimingGateLegacy (helper seam, ทุกเครื่องที่ build ได้)**
6. `W3-L1-B1: single-process unexpected ffmpeg exit (−40 shape) — error exactly once, deterministic terminal, no orphan`
7. `W3-L1-B2: stop-after-fault idempotency — no double terminal event, settles Idle`
8. `W3-L1-B3: FALSE-SUCCESS — 0-byte output must not be announced saved` **(KNOWN-RED — §6)**
9. `W3-L1-B4: valid helper stream → saved exactly once + full PTS grid at helper fps`
10. `W3-L1-B5: restart-after-fault → next recording valid`
11. `W3-L1-C1: fps-switch restarts 15→30→15 — each file on its own grid, engine reusable`

**L2-QSV — TimingGateRealQsv (QsvGate-gated; เครื่อง Intel นี้รันได้จริง)**
12. `W3-L2-Q1..Q4: real grid gate — ddagrab+qsv at 30/60/120/144, assertions A1–A7 per file (1 run ต่อโหมดใน gate; matrix M2-W3 คือ evidence เชิงลึก)`
13. `W3-L2-Q5: 240fps honest-failure — D1–D3 (QsvGate.ModeSupported ตัดสิน GRID หรือ HONEST_FAILURE)`

**L2-NVENC — TimingGateNative (HardwareGate-gated; เฉพาะเครื่อง GTX 1080 Ti)**
14. `W3-L2-N1..N5: native engine grid gate — RecordingEngine 30/60/120/144/240 (Initialize+StartSession ตรง, assertion A1–A7)`
15. `W3-L2-N6: per-session FPS rebuild authority — 60-init → 30-session → 120-session ตาม C2`
16. `W3-L2-N7: nvenc fault at session level → honest terminal (twin ของ NvencNativeFaultTests แต่วัดผ่าน SessionResult + ไฟล์)`

**การคืนค่า (Return contract)**: ต่อเทสพิมพ์ `PASS/FAIL/SKIP → เหตุผล` + แถว verdict ของไฟล์ที่เกี่ยว (class + ตัวเลขสำคัญ) — รูปแบบเดียวกับ matrix M2-W3 เพื่อให้ผล gate อ่านสะเทือนกับ report ได้ตรง ๆ

---

## 6. KNOWN-RED / BASELINE STATUS (อัปเดต 2026-09-12 หลัง fix M2/W1 เข้า production)

สถานการณ์ปัจจุบัน (evidence จาก gate run 2 รอบติด 2026-09-12):

| เทส | สถานะ | หลักฐาน |
|---|---|---|
| `W3-L1-B3` (helper seam, stop หลังความล้มเกิดขึ้นก่อนแล้ว) | **GREEN** | fix M2/W1 (`IsOutputFileValid` — exists ∧ size>0 — ใน Step-4 และ OnExited recovery, `CaptureEngine.vb:568/616/634/1235`) ครอบ path นี้: stop จาก state=HasError คืน `outputOk OrElse Not stopFromFailure` = False |
| `W3-L2-Q5b` (QSV-240 จริง) | **ยังแดง (deterministic 2/2)** | race ที่เหลือ: stderr error ยิงก่อน OnExited dispatch → stop เริ่มตอน state ยัง Recording → `stopFromFailure=False` ถูก capture ก่อนความล้มรู้ตัว → terminal event มาถึง *ระหว่าง* stop → `Return outputOk OrElse Not stopFromFailure` = **True** ทั้งที่ไฟล์ 0-byte (`CaptureEngine.vb:446/616`) — repro: `run-recording.ps1 -Fps 240 -ForceAttempt` หรือเทส `W3-L2-Q5b` |

ข้อสังเกต: fix M2/W1 ถูก apply โดย workstream อื่น (concurrent, ไฟล์ production ถูกแก้ 2026-09-12 01:46)
พร้อมถอด known-red registration ของ B3/Q5b ออกจากเทส — gate ยังจับ Q5b ติดต่อได้เลย ซึ่งคือหน้าที่ของมัน:
**gate ยังคง FAIL ในพื้นที่ 240 FPS จนกว่า mid-stop race จะถูกแก้** (ข้อเสนอแนะแก้เมื่อได้รับอนุญาตแตะ
production: capture ความล้มจาก terminal events ณ จุด return ไม่ใช่ capture ตอนเริ่ม stop — เช่น
`stopFromFailure` อ่านใหม่หลัง WaitForExit/Step-4, หรือ return `outputOk` เสมอเมื่อ session เคยรายงาน error)

หลักฐานเดิม (ก่อน fix): M2-W3 QSV-240 จริง 2/2 → `[result] OK` + exit 0 ทั้งที่ไฟล์เสีย; โค้ดเดิม
`CaptureEngine.vb` Step-4 เช็คแค่ `File.Exists`

---

## 7. Suite ที่ควรรัน — runbook

| โอกาส | ชุดที่รัน | รวมเวลาโดยประมาณ |
|---|---|---|
| **ทุกครั้งหลัง build** (สคริปต์เดิม) | `build-all.ps1 -RunTests` (7 suite เดิม) **+ เพิ่ม `Engine.TimingGate.Tests` ใน `$suites`** (L0+L1 ทำงานทุกเครื่อง; L2 SKIP ตาม probe อย่างซื่อสัตย์) + `Engine.ConfigTruth.Tests` (ควรเพิ่มเข้า list เดียวกัน — deterministic, no hardware) | ~3–5 นาที |
| **ก่อน merge งาน timing/FPS** (บนเครื่อง Intel นี้) | ข้างบนทั้งหมด **+ `Engine.Concurrency.Tests`** (H1/H2/F03/G2/G3) **+ L2-QSV ให้ทำงานเต็ม** (probe ผ่าน → กริด 30–144 + honest-failure 240) | ~6–8 นาที |
| **บนเครื่อง GTX 1080 Ti** (พิสูจน์ NVIDIA รอบใหม่) | ข้างบนทั้งหมด แต่ L2 วิ่ง lane NVENC (N1–N7) + M1/M2 ออกจาก SKIP + `NV-STRESS` + `validate-phase12b.ps1` | ~10–12 นาที |
| **Forensic เชิงลึกเมื่อ gate แดง** | `analyze-pts.ps1` + `FpsMatrixDriver` (M2-W3 tooling) — matrix 2 run/โหมด + isolation ด้วย ffmpeg ล้วน | ~5 นาที |

ข้อสังเกตการดำเนินการ (registration) เมื่อลงมือ implement (ไม่ใช่วันนี้): เพิ่มบรรทัดเดียวใน `$suites` ของ `scripts\build-all.ps1` = `"Tester\test\Engine\TimingGate\Engine.TimingGate.Tests.vbproj"` — script เป็น tooling ไม่ใช่ production timing code

---

## 8. ขอบเขตที่ gate นี้ไม่ครอบ (ตั้งใจไว้)

- ความแม่น A/V sync ms-level (Phase-13 single-clock) — อยู่นอก timing/FPS round นี้
- **สรุปสาเหตุรากที่แท้จริง 38ms** — gate แค่จัดประเภท FIRST_GAP และทำให้มัน FAIL ได้เมื่อเกิด; การสรุปสาเหตุรากที่แท้จริงยังต้องใช้ forensic instrumentation แยก (ห้ามสรุปจาก matrix/gate เพียงอย่างเดียว)
- Soak ระยะยาว (`NVIDIA.Soak`), replay buffer, ฝั่ง OBS — ไม่เกี่ยว
- ประสิทธิภาพ bitrate/CBR filler — M2-W3 พบ CBR ต่ำกว่า target บน static desktop (2.4MB/5s @20Mbps) แต่ไม่ใช่สัญญา (contract) timing → ไม่เข้า gate รอบนี้

## 9. การปฏิบัติตาม (compliance)

- ไม่มีการแก้ production code ใน W3 นี้ (design doc เดียว + ผล survey)
- ไม่มีการ commit (ไม่มี git บนเครื่อง; เอกสารอยู่ที่ repo root เป็น untracked file)
- ไฟล์ที่เกี่ยว: `W3-REGRESSION-GATE-DESIGN.md` (เอกสารฉบับนี้), หลักฐานอ้างอิงใน `test-recordings\fps-matrix\`

---

## 10. W3 — CANONICAL TEST RUNNER (implement แล้ว 2026-09-12, additive)

ปิด audit findings ของ M1/W3 (unknown args / silent-ignore / stale scripts / wrong binary / wrong ffmpeg)
ด้วย one-command entry point ใหม่ — **ไม่ลบ script เก่า, ไม่แตะ production**:

```
powershell -ExecutionPolicy Bypass -File scripts\run-recording.ps1 -Fps 60 [-Seconds N] [-Lane legacy|native]
           [-OutDir d] [-OutFile f] [-Encoder id] [-Force] [-ForceAttempt] [-NoBuild]
Exit codes: 0=PASS  1=FAIL  2=BLOCKED (environment)  3=usage/setup error
```

Chain ที่รับประกันต่อการรันหนึ่งครั้ง:

| ขั้น | การรับประกัน | audit finding ที่ปิด |
|---|---|---|
| build | สร้าง binary จาก source ทุกครั้ง (retry 1 ครั้งกัน MSBuild transient lock), `-NoBuild` = opt-out ชัดเจน | stale scripts |
| binary | path ตายตัว + SHA256 ทั้งจาก runner และจากตัว driver รายงานตัวเอง (ตรงกันจึงผ่าน) | wrong binary |
| ffmpeg/ffprobe | จาก product tree เท่านั้น + พิสูจน์รันได้ด้วย `-version` (กันรูปแบบไบนารีเสียตาม postmortem 2026-09-08) | wrong ffmpeg |
| args | strict ทุกชั้น: CmdletBinding ของ runner + driver/analyzer โต้แย้ง unknown/duplicate flag ด้วย exit 2 + `##RESULT## verdict ERROR-ARGS` | unknown args, silent-ignore |
| fps | `-Fps` บังคับ (1–240) + `QsvGate` probe ตัดสินก่อนอัด: โหมดไม่รองรับ = BLOCKED (ยกเว้น `-ForceAttempt` → จงใจอัดเพื่อเก็บ FAIL evidence) | — |
| output | default `test-recordings\canonical\rec-<lane>-<fps>-<ts>.mp4`, ปฏิเสธ overwrite ถ้าไม่ `-Force` | — |
| analyze | ใช้ PtsAnalyzer ตัวเดียวกับ gate ผ่าน `Engine.TimingGate.Tests --analyze` (ไม่มี drift) | — |
| result | `result.json` ข้างไฟล์ + บรรทัดสุดท้าย `##RUNRESULT## {json}` (schema canonical-run-v1: verdict/reason/binaries/ffmpeg+ffprobe versions/probe/runResult/analyze/timing/host) | — |

ผลทดสอบจริงบนเครื่องนี้ (2026-09-12): `-Fps 60` = **PASS** (GRID_OK 248 เฟรม eff 60.000, result.json เขียนแล้ว);
`-Fps 240` = **BLOCKED (exit 2)** ด้วยหลักฐาน QsvGate; `-Fps 240 -ForceAttempt` = **FAIL** พร้อมหลักฐาน
ไฟล์ 0-byte (analyzer FAIL + driver verdict FAILED); `-Lane native` = **BLOCKED** (no NVIDIA adapter);
`-Fpsx 60` = parameter binding error ทันที (ห้าม silent-ignore ทุกชั้น)

หมายเหตุ implementation: (a) ไฟล์ .ps1 เป็น ASCII ล้วน — กับดัก encoding BOM-less บน PowerShell 5.1
(เคยเจอที่ sync-verify.ps1); (b) `$ErrorActionPreference='Continue'` ใน runner — native stderr ที่ merge
ด้วย 2>&1 กลายเป็น ErrorRecord แล้ว 'Stop' จะฆ่าสคริปต์กลางทาง; runner ตรวจผลจริงด้วย exit code + marker เอง

Gate regression หลังเพิ่ม runner modes (2026-09-12, หลัง fix M2/W1 เข้า production โดย workstream อื่น):
`PASS=18 FAIL=1 BLOCKED=6` — FAIL เดียวคือ `W3-L2-Q5b` (false-success race ที่เหลือของ 240fps — ดู §6);
พื้นที่ mission อื่นครบเขียว รวม `W3-L1-B3` ที่เขียวจริงหลัง fix
