# W1 — Capture Lifecycle Stress & Fault Proof Report (engine-level fault injection)

**Date:** 2026-09-12
**Method:** fault harness (`faultharness`, scratch นอก repo) ขับ **production `RecordingEngine`/`CaptureSession`/backends ตัวจริง** ผ่าน DLL; faults ฉีดด้วย (a) reflection เข้า seam/fault-path ที่ production มีอยู่แล้ว (C-2C worker crash, TransitionToFaulted) (b) process manipulation ภายนอก (NtSuspendProcess/NtResume/kill ต่อ mux ffmpeg ที่ระบุด้วย pid-diff + verify suspended) — **production source ไม่ถูกแต่งแม้บรรทัดเดียว**

---

## 0. Verdict (TL;DR)

| Fault injection | ผลลัพธ์ที่วัด | State truth | Orphan |
|---|---|---|---|
| **F1** encoder fault กลาง CFR | C/6 containment ทำงาน: loop abort ที่ 1.9s (จาก 6s), nvencErrors=3, unwound สะอาด | Idle → Disposed ✓ | 0 |
| **F2** capture worker crash (C-2C) | session รันครบ 6.00s (CFR dup ต่อ), log "worker terminated without Stop" → Stopped (self-heal); **follow-up session pass=True** บน engine เดิม | Idle ✓ | 0 |
| **F3** Stop-race ×15 (Stop ที่ 0/10/30/80/300ms ×3 reps) | ทุก rep: state Idle, ffmpeg 0; delay=0 → Stop-before-Start ไม่ cancel (honest semantics, session รันครบ 5.4s); 10–300ms → unwind 0.2–0.6s | Idle 15/15 ✓ | 0 |
| **F4** mux stall (suspended ffmpeg, verified) + Dispose | **Dispose timeout path ยิงจริง: 30.0s** → state=`Stopping` (honest, ไม่ปลอม Disposed) → `StartSession` ถูก reject → หลัง resume: mux finalize เอง (0 orphan) → second Dispose → Disposed ✓ | Stopping→Disposed ✓ | 0 |
| **F5** double-Initialize / concurrent StartSession / double-Dispose / Start-after-Dispose | throw InvalidOperationException / "state Recording" throw / no-throw ✓ / ObjectDisposedException ✓ | ครบ ✓ | 0 |

**สรุป:** ทุก fault path ที่ lifecycle forensic (W1-CAPTURE-LIFECYCLE-FORENSICS.md) audit จากโค้ด — **พฤติกรรมจริงตรงกับโค้ดทั้งหมด** ภายใต้การฉีดจริง: C/6 containment, stop-latch snapshot, honest Stopping/timeout-reject, self-heal หลัง worker crash, ownership old→new session, zero orphan

---

## 1. รายละเอียดต่อเคส (raw log ใน evidence)

### F1 — encoder fault กลาง CFR loop
- Injection: reflection → `RecordingEngine._encoder` → `TransitionToFaulted("W1 F1 injected encoder fault")` ที่ t≈2s ของ session 6s
- ผล: `nvencErrors=3` (C/6: ทุก tick หลัง fault throw ครั้งเดียวต่อ tick แล้ว abort), `frames=114`, session จบที่ 1.9s → stop sequence ปกติ → Idle → Disposed; ffmpeg orphan 0
- ข้อสังเกต (เล็ก): `SessionResult.ErrorMessage` **ว่าง** บนเส้นทางนี้ — fault ปรากฏเฉพาะ `NvencErrors` counter + log (observability gap เล็ก ไม่ใช่ correctness)

### F2 — capture worker crash กลาง session (C-2C seam)
- Injection: reflection → `DdagrabBackend.RequestWorkerCrashOnce()` ที่ t≈2s → worker throw ที่ top-of-iteration (เส้นทางเดียวกับ crash ธรรมชาติ)
- ผล: worker exit-tail transition `Running → Stopped` + Error log "worker terminated without Stop" (ตาม M2 design); session **รันครบ 6.00s** (CFR duplicate เฟรมสุดท้ายจนครบ — by design); engine Idle
- **F2b self-heal:** session ใหม่บน engine เดียว → pass=True, 121 frames (duplication recreate จริง) ✓

### F3 — Stop-race matrix
- Stop() ที่ 0ms: ถ้า session ยังไม่ผ่าน state=Recording → `Stop()` no-op → session รันเต็ม 5s (5.4s wall) — **เอกสาร semantics: Stop-before-Start ≠ cancel**
- Stop() ที่ 10–300ms: unwind 0.2–0.6s, state Idle, ffmpeg 0 — stop-latch snapshot ทำงาน (tail-fill ไม่ยืดเกินจุดกด)
- 15/15 reps: ไม่มี throw หลุด, ไม่มี orphan, ไม่มี state ปลอม

### F4 — mux stall + Dispose timeout + recovery (เคสหนักสุด)
- หา mux ด้วย pid-diff (ffmpeg ตัวใหม่หลัง session start) → `NtSuspendProcess` status=0x0 + **verify suspended=True ผ่าน ThreadState**
- `engine.Dispose()` ขณะ session Recording + mux ค้าง → **Dispose ใช้ 30.0s พอดี** (ตรง `_sessionFinished.Wait(30s)`) → warning "timed out waiting…" → state=`Stopping` → `StartSession` หลังจากนั้น = **ObjectDisposedException (reject ถาวรตาม design)**
- Recovery ตาม contract ในโค้ด: resume ffmpeg → mux finalize เอง (exit 0, frag promote) → session signal จบ → **second `Dispose()` ทำ cleanup ครบ → Disposed** → ffmpeg 0
- สรุป: timeout path + honest-reject + recovery — ทั้งสามพิสูจน์ด้วย evidence จริง

### F5 — double-invocation truth
- `Initialize` ซ้ำ → InvalidOperationException ✓
- concurrent `StartSession` ×2 → ตัวที่สอง throw "called from state Recording" ✓ (single-session enforced)
- `Dispose` ×2 → no-throw ✓
- `StartSession` หลัง Dispose → ObjectDisposedException ✓

## 2. FACT / HYPOTHESIS / UNKNOWN

**FACT** — ทุกบรรทัดใน §0/§1 (evidence: `evidence/w1-fault-proof/fault-results.txt, f4_run3.txt, f1/f2/f4.log` + harness source)
**FACT** — orphan ffmpeg = 0 ทุก checkpoint; ownership old→new สอดคล้อง M1-W2 battery (death-before-birth)
**HYPOTHESIS** — Dispose-timeout path กับ hardware-stall จริง (NVENC hang ระดับ driver) จะเจอเส้นทางเดียวกัน (suspend = สิ่งที่ใกล้ hardware-stall ที่สุดโดยไม่แก้ production)
**UNKNOWN** — พฤติกรรมเมื่อ stall เกิดที่ **capture worker** แบบค้าง (ไม่ crash) ระหว่าง session: CFR loop จะ dup ต่อจนครบ duration (session จบปกติ) — ตามโครงสร้างน่าจะไม่ค้าง engine แต่ไม่ได้ฉีดสด (thread-suspend ระดับ .NET ทำจากภายนอกไม่ได้)

## 3. ข้อสังเกตเชิงปรับปรุง (ไม่แก้ — บันทึกไว้)

1. F1: `SessionResult.ErrorMessage` ว่างบน encoder-fault containment path — fault มองเห็นเฉพาะ `NvencErrors` + log
2. Stop-before-Start (delay=0) ไม่ cancel session ที่กำลังจะเริ่ม — semantics ปัจจุบันโอเค แต่ควรมีเอกสารระบุ
3. Dispose-timeout ทำให้ engine ใช้ไม่ได้ถาวรจน restart process — by design; ควรมี telemetry ชัดฝั่ง host

## 4. Evidence Index

- `evidence/w1-fault-proof/fault-results.txt` — ผลทุกเคส (F1-F5)
- `evidence/w1-fault-proof/f4_run3.txt` — F4 timeout+recovery run (30.0s / verifiedSuspended=True)
- `evidence/w1-fault-proof/f1.log f2.log f4.log` — runtime logs (รวม "worker terminated without Stop", LiveMux ok=True)
- `evidence/w1-fault-proof/Program.cs + faultharness.csproj` — harness (reflection injection + NtSuspendProcess/Resume + pid-diff)
- Cross-ref: `W1-CAPTURE-LIFECYCLE-FORENSICS.md` (audit ต้นทาง), M1-W2 lifecycle battery
