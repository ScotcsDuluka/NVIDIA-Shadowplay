# W1 — Capture Lifecycle Forensics Report (read-only audit)

**Date:** 2026-09-12
**Scope:** process start → engine Initialize → session creation → Start → Running → Stop → Cancel → Dispose — state machine, ownership, races/locks, failure paths, double-invocation, cleanup — **read-only, no production touched**
**Method:** full source read of `RecordingEngine.vb` (449L), `CaptureSession.vb` (1,361L), `DdagrabBackend.vb` (state/join paths), `NvencEncoderBackend.vb` (state machine), `LiveMuxSession.vb` (862L) + regression-coverage inventory in the repo

---

## 1. State machine map (4 layers, per source)

### 1.1 RecordingEngine (`RecordingEngine.vb:33,56-162,194-342,371-422`)
```
Created ──Initialize()──► Initializing ──ok──► Idle ⇄ Recording ──(dispose pending)──► Stopping ──► Disposed
                              │fail                                              │
                              ▼                                                  ▼
                           Faulted ◄────────────────────── any state (dispose timeout keeps Stopping+rejects)
```
- `Initialize` เฉพาะจาก `Created` (:71-73) → `Initializing` (:74); fail = `Faulted` + cleanup partial backends (:154-161)
- `StartSession` เฉพาะจาก `Idle` (:199-201) → `Recording` ใต้ `_sync` (:202-204); Finally publish `Idle` **หรือ `Stopping` ถ้า `_disposeRequested`** (:334-337 — honest state, ไม่ปลอม Idle)
- `Stop()` ทำงานเฉพาะขณะ `Recording` (:349-354); `Dispose()` valid ทุก state, idempotent (:376-380)

### 1.2 DdagrabBackend (`DdagrabBackend.vb:434-564`)
```
Created → Initialized → Starting → Running → Stopping → Stopped ; Faulted (self-heal ได้ใน worker)
```
- `Start`: Starting/Running → **warn+ignore** (idempotent); Initialized/Stopped → Starting; state อื่น throw (:436-446)
- Start = recreate duplication ก่อน (session-start frame fix :466-477) → commit Running + spawn worker **ใต้ lock เดียว** (C-1: throw กลางทาง = rollback Faulted, ไม่มี orphan worker :479-490)
- `Stop`: Running → Stopping → set stopSignal → **Join 2s**; timeout = **ค้าง Stopping อย่างซื่อสัตย์** (M2 fix :541-550 — ไม่ประกาศ Stopped ปลอม เพราะ worker generation ยังจับ duplication อยู่; Start ถูก reject จาก Stopping)
- `Dispose`: **ไม่ Join ขณะถือ _sync** (:567-568 — worker ต้องใช้ _sync ทุก iteration = deadlock ถ้าไม่งั้น)

### 1.3 NvencEncoderBackend (`NvencEncoderBackend.vb:28-31,163,451-462,854-893`)
```
Created → Initializing → Initialized → Running → Stopping → Stopped ; Faulted (terminal จนกว่า Dispose) ; Disposed
```
- `Initialize` fail ทุกจุด (D3D11/NVENC load/open) → `Faulted` (:225,234,256,379,413)
- `Start`: Running → **no-op** (:457); Initialized/Stopped → Running (:462)
- `Stop`: Running/Flushing → Stopping (sync mode ไม่ต้อง drain) → Stopped; **Faulted = terminal no-op** (:885-887); Disposed → throw

### 1.4 CaptureSession + LiveMuxSession (`CaptureSession.vb:224-341,1338-1354`; `LiveMuxSession.vb:153-205,337-459`)
- `CaptureSession.Stop()` = latch `_stopRequestedTicks` (Interlocked.CompareExchange — **stop snapshot ก่อน drain/tail-fill**: timeline จบตามเวลาที่ user กด ไม่ใช่เวลา cleanup เสร็จ) + `_stopSignal` volatile (:1338-1341)
- `CaptureSession.Dispose` **ไม่แตะ `_capture`/`_encoder`** (กฎ ownership: engine เป็นเจ้าของ — :1353 comment)
- `Run()` Finally unwind ทุกเส้นทาง: captureRunning/encoderRunning flags (idempotent stop — success path ไม่ re-stop), audio engine stop+dispose, `_liveMux.Dispose`, frame retirement (`CompleteAndWait`), sink/events dispose (:1240-1282)
- `LiveMuxSession`: Start → pipes+listeners → spawn ffmpeg → StartWriter; Stop = drain (bounded) → pipe EOF → WaitForExit → faststart remux → salvage ถ้า remux พัง (:387-406); RequestStopAndDrain fold residual เข้า DroppedBytes ครบ

## 2. Ownership table

| ทรัพยากร | เจ้าของ | เกิด | ตาย |
|---|---|---|---|
| `_capture` (D3D11+DXGI duplication) | **RecordingEngine** (persistent, ใช้ซ้ำข้าม session) | `Initialize` (:79-81) | `Dispose` (:418) |
| `_encoder` (D3D11+NVENC session) | RecordingEngine (ยกเว้น FPS-rebuild swap) | `Initialize` (:85-119) | `Dispose` (:417) |
| `CaptureSession` | สร้างใหม่ต่อ session ใต้ `_sync` (:308) | StartSession | session unwound → `_currentSession=Nothing` (reference-equality :315-317); Dispose ก็ clear (:402) |
| sink/audio engine/live-mux/frame-disposer | **CaptureSession** (ยืม backends) | Run() | Finally ของ Run ทุกเส้นทาง (:1240-1282) |
| decode worker thread / duplication generation | DdagrabBackend | Start (ใต้ lock) | Stop join 2s / worker self-exit |
| ffmpeg process (mux) | LiveMuxSession | Start | Stop(drain→EOF) / Dispose kill (:455-457) |

## 3. Race/lock analysis (ทุกจุดที่มี guard จริง)

| Race window | Guard | ไฟล์:บรรทัด |
|---|---|---|
| Double `StartSession` (concurrent) | state check `<> Idle → throw` ใต้ `_sync` | RecordingEngine.vb:199-201 |
| Double `Initialize` | state check `<> Created → throw` | :71-73 |
| `Dispose` ระหว่าง Recording | `_disposeRequested` gate; session ใหม่ที่สร้างหลัง request → `Stop()` ทันที (`startStop`) | :304-312 |
| FPS-rebuild swap vs `Dispose` | re-check `_disposeRequested` **ใต้ `_sync`** ก่อน swap; แพ้ race = dispose rebuilt encoder ทิ้ง | :276-295 |
| `Dispose` vs backend-ref read | อ่าน refs ใต้ `_sync` ก่อน dispose | :413-416 |
| Session vs `_currentSession` clear | `ReferenceEquals` check | :315-317 |
| Dispose timeout 30s | honest reject: state ค้าง + `_disposeRequested` → StartSession rejected ตลอดไป (comment ยอมรับเอง "Saying safe retry here was a lie") | :386-394 |
| Ddagrab Start throw mid-way | commit Running+spawn ใต้ lock เดียว → Running(work) หรือ Faulted(no worker) เท่านั้น | DdagrabBackend.vb:479-490 |
| Ddagrab Stop join-timeout | ค้าง Stopping (Start rejected) — ไม่ spawn worker ซ้ำบน shared GPU state (M2 fix) | :541-550 |
| Ddagrab Dispose-vs-worker deadlock | ไม่ Join ขณะถือ `_sync` | :567-568 |
| CaptureSession Stop-latch vs tail-fill | latch ที่ Stop() เข้า (snapshot ก่อน) — tail-fill ใช้ snapshot เป็นจุดจบ timeline | CaptureSession.vb:888-890,1338-1341 |
| Encoder Faulted กลาง CFR loop | C/6 containment: faulted → abort loop → stop sequence ปกติ (ทุก encode ต่อจากนั้น throw เพียงครั้งเดียว) | CaptureSession.vb:824-836 |
| DXGI AccessLost | self-heal recreate (one live duplication per output per process) | DdagrabBackend.vb:812-819 |
| Session-start frame | recreate duplication ตอน Start → เฟรมแรกทันที (guard 'เสียงมาก่อนภาพ') | :466-477 |

## 4. Failure paths / double-invocation (ตามโค้ด)

- **Double Start (session):** throw InvalidOperationException — session ไม่ถูกสร้างซ้ำ
- **Double Stop (engine/session):** engine `Stop()` no-op เมื่อไม่ Recording; `CaptureSession.Stop()` latch = idempotent (CompareExchange)
- **Stop → Start ใหม่:** Idle → StartSession ได้ (backends persistent, NVENC re-arm FORCEIDR ต่อ session — NvencEncoderBackend.vb:649-655)
- **Cancel (Stop กลาง session):** stop snapshot ที่ latch → drain → tail-fill ถึง snapshot เท่านั้น → finally → Idle
- **Dispose กลาง session:** stop → join 30s → dispose session → dispose backends; timeout = ปฏิเสธถาวร (ไม่มี fake-Idle)
- **Initialize fail:** Faulted + partial cleanup → ต้อง process restart (ไม่มี retry จาก Faulted)
- **Exception หลัง Start กลาง Run:** Finally unwind ทุกทรัพยากร session-owned; engine กลับ Idle

## 5. FACT / HYPOTHESIS / UNKNOWN

**FACT** (ทุกข้อมี file:line ข้างต้น)
1. State machine 4 ชั้น enforce กติกาครบ: single-session, Initialize-once, Start idempotent, Faulted terminal, honest Stopping/Stopping-reject
2. Death-before-birth โครงสร้าง: StartSession เป็น synchronous บน caller thread → session ใหม่เกิดได้หลังเก่าจบเสมอ; Dispose join ก่อน dispose backends
3. FPS-rebuild dispose-race guard มีจริงใน code (re-check ใต้ _sync + dispose loser)
4. Stop-latch snapshot ทำให้ cancel ตามเวลาผู้ใช้ (ไม่ยืด timeline)
5. NVENC re-arm ต่อ session (FORCEIDR+SPS/PPS ที่เฟรมแรกของ session) — กันอาการ "session เก่าไม่ตายทำ Start ถูก skip"
6. Ddagrab Stop แบบ M2: join-timeout = ค้าง Stopping อย่างซื่อสัตย์ (กัน second worker บน shared GPU state)
7. Dispose timeout = ปฏิเสธถาวร (state truth ไม่ปลอม)

**HYPOTHESIS**
- Unwind ~0.4s (วัดโดย M1-W2 runtime battery) ทำให้โอกาสชน Dispose-timeout 30s ต่ำมาก — แต่ถ้าเกิด (ffmpeg hang ตอน finalize) ผลคือ engine ใช้ไม่ได้จน restart process: **ตาม design** ไม่ใช่ bug
- Texture ค้าง 2-8 ตัว ณ capture-stop = retained frames (pendingFrame/lastFrame/sink) ที่ disposer drain รับหมด — ไม่ใช่ leak
- Exception กลาง CFR loop (throw จาก encoder/mux) ครอบด้วย C/6 containment + Finally — ยังไม่เคย fault-injection สด (ไม่มีช่อง trigger จากภายนอก)

**UNKNOWN**
- NVENC/capture hardware failure จริง (driver busy, AccessLost จาก mode change ระหว่าง session) — counters = 0 ทุก run ที่วัด; self-heal path audit แล้วแต่ไม่เคย reproduce สด
- Stop() ระหว่าง head catch-up burst / ก่อนเฟรมแรก — ช่วงเวลาแคบ ต้อง fault injection
- หลาย engine instance ต่อ process — นอกขอบเขต production (สร้างเดียว)

## 6. Regression coverage ที่มีอยู่จริง

| ชั้น | ไฟล์/สคริปต์ | ครอบอะไร | สถานะ |
|---|---|---|---|
| Runtime battery (M1-W2, 2026-09-12) | ภายนอก repo (Temp\sp-lc) — report ใน board | T1 Start-before-Init, T3 cancel กลางทาง (0.4s unwind, stop-snapshot 2.91s), T4 restart+double-Start, T5 double-Stop, T6 normal หลัง abnormal, T7 Dispose กลาง session, T8 Start-after-Dispose; death-before-birth gap +188/191/218ms; FORCEIDR re-arm 4/4; orphan 0 | ผ่านทั้งหมด (artifacts ไม่อยู่ใน repo) |
| ConsoleDriver matrix | `ConsoleDriver/Program.vb` (A=3×10s normal, B=1/5/10s early-stop, C=5×3s restart) + evidence `bin/.../evidence/phase-12b-validation-*.md` (run จริง 0910-0911) | ปกติ/early-stop/restart/idempotency/orphan | ใช้งานได้จริง (W3 audit: รันผ่าน --videocheck/--single path เท่านั้น — root .bat scripts เป็น silent-ignore) |
| Crash test | `scripts/validate-phase12b.ps1` (kill driver mid-session → no orphan ffmpeg) + `scripts/build-all.ps1` | process-kill recovery | มีอยู่จริง รันได้ |
| Config/chain tests | `Tester/test/Engine/ConfigTruth/*` (CT4/H1/VCT) | config→session wiring (V-CT1/2), H1 path-injection gate | ใช้งานได้จริง |
| Recording unit tests | `Tester/test/CaptureEngine/Recording/*` (Disposer/DualTrack/MuxSinkAccounting/RuntimeSync/AudioTimeline) + `Encoder/Lifecycle/EncoderLifecycleTests,NvencLifecycleTests` + `FFmpeg/LiveMuxSessionTests` | disposer invariant, audio timeline, mux accounting, **encoder lifecycle states, mux job ownership** | ใช้งานได้จริง |
| **ช่องว่างที่ยังไม่มี** | — | (a) engine-level double-Start/Dispose-timeout unit test ใน repo (runtime battery อยู่นอก repo), (b) fault-injection กลาง CFR loop, (c) Stop ระหว่าง catch-up burst | UNKNOWN → ถ้าต้องการปิด ต้องเพิ่ม test seam (อนุมัติก่อน) |

## 7. บทสรุป

Lifecycle ของ Capture Engine มี state machine ที่ enforce กติกาครบทุกชั้นและมี guard จริงทุก race window ที่ audit — **ไม่พบ race ที่ไม่มี guard หรือ cleanup path ที่ขาด** ในขอบเขตที่อ่านได้ ช่องว่างเดียวคือความครอบคลุมของ regression ใน repo (engine-level battery อยู่นอก repo + ไม่มี fault-injection seam) — เป็นเรื่อง coverage ไม่ใช่ความถูกต้องของโค้ด
