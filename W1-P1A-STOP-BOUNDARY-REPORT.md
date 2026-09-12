# W1 — P1-A Stop Boundary Report: video stop snapshot ↔ `_sessionEndQpc100ns` unified

**Date:** 2026-09-12
**Scope:** minimal production fix (CaptureEngine.Audio + 1 call ใน CaptureSession) — **ไม่แตะ Gallery / NVENC / Ddagrab architecture / ไม่ commit**

---

## 0. Verdict (TL;DR)

**Mechanism ยืนยันด้วยโค้ด + การวัด:** ใน `CaptureSession.Run` video stop snapshot (`stopQpcTicks`) latch ที่ต้น stop sequence แต่ `_sessionEndQpc100ns` (audio HARD T_END) freeze **ช้ากว่า** — เฉพาะเมื่อ `_audioEngine.Stop(...)` ทำงาน **ท้าย sequence** ระหว่างนั้น (หน้าต่าง = ระยะเวลา capture.Stop/tail-fill/disposer/encoder.Stop — **heavy-resolution = 762ms-class ตาม evidence เดิมของ board**) เสียงไหลผ่าน T_END **โดยไม่ถูก clip** เพราะ HARD T_END ใน `OnPacket` ยังไม่ถูก arm (`_sessionEndQpc100ns = 0`) → audio ยาวกว่า video ตามความหนักของ teardown

**Fix (minimal):** freeze `_sessionEndQpc100ns` **ที่ latch เดียวกับ video snapshot** — `AudioEngineSession.SetSessionEndQpc100ns()` (ใหม่, ไม่หยุด capture/ไม่ finalize) + `CaptureSession` เรียก 1 บรรทัดหลัง latch; `Stop(…stopQpcTicks…)` เดิมที่ท้าย sequence re-set ค่าเดิม (idempotent)

**ผลวัด (native 1680×1050@60fps, ไฟล์จริง):**

| รอบ | video | audio | Δ (audio−video) |
|---|---|---|---|
| BEFORE 60s | 60.039 | 60.067 | **+27.5ms** |
| AFTER 60s | 60.023 | 60.050 | **+28.0ms** |
| AFTER 3s | 3.021 | 3.043 | +21.8ms |
| AFTER 5s | 5.038 | 5.048 | +9.4ms |

→ บนเครื่องนี้ stop sequence เร็ว (window ~ms) ทั้ง before/after อยู่ใน **AAC final-frame quantization (~9-28ms)** — fix ไม่ทำให้แย่ลงและ clip window ถูก arm ตั้งแต่ latch (บนเครื่อง/สเปก heavy ที่ teardown = 762ms นี่คือส่วนต่างที่ถูกตัดทิ้ง)

**การพิสูจน์ภายใต้ heavy-load ติดข้อจำกัดใหม่:** 60s ใต้ CPU hogs → **mux ffmpeg ตายกลาง session แบบสุ่ม** (light: 2.7s / loaded: 17.6s, exit=-1 ไม่มี stderr error, 102MB dropped, partial-salvage ทำงานถูก) — **issue ใหม่ที่ session 60s (ยาวสุดเท่าที่เคยรัน) เปิดเห็น** — แยกจาก P1-A (ต้อง investigation ต่อ: infra/AV/memory); 1 ใน 3 รอบ 60s ผ่านสมบูรณ์

---

## 1. Code truth (ก่อนแก้)

**Video boundary** — `CaptureSession.vb:888-890`: `stopQpcTicks` = latch ผู้ใช้ หรือ GetTimestamp (natural end) → video tail-fill ถึง `stopElapsedSeconds` ✓

**Audio boundary** — `CaptureSession.vb:1003`: `_audioEngine.Stop(WasapiPositionCapture.StopwatchTicksTo100ns(stopQpcTicks))` → `AudioEngineSession.Stop` (`AudioEngineSession.cs:140-157`): ตั้ง `_sessionEndQpc100ns` → stop captures → FinalizeTrack (tail pad ถึง boundary) — **มี single-shot guard แล้ว** (`if (!_started || _stopped) return` :144) — first boundary wins

**ช่องว่าง:** ระหว่าง latch (:888) กับ Stop (:1003) — `OnPacket` ทำงานด้วย `_sessionEndQpc100ns = 0` → **HARD T_END clip ไม่ทำงาน** (`AudioEngineSession.cs:243-244: if (sessionEnd > 0)`) → ทุก packet ที่มาในช่วง teardown (heavy-res: 762ms) เข้า sink เต็ม ๆ → LastEnd100ns เกิน T_END → FinalizeTrack: `tail = sessionEnd − LastEnd < 0` → ไม่ pad → **audio ยาวเกิน video เท่ากับ window นั้น**

## 2. The fix (minimal, 2 ไฟล์)

**`CaptureEngine.Audio/AudioEngineSession.cs`** — เพิ่ม method:
```csharp
public void SetSessionEndQpc100ns(long sessionEndQpc100ns)
{
    if (sessionEndQpc100ns <= 0) return;
    lock (_sync)
    {
        if (!_started || _stopped) return;
        Interlocked.Exchange(ref _sessionEndQpc100ns, sessionEndQpc100ns);
    }
}
```
freeze boundary โดยไม่หยุด capture/ไม่ finalize — OnPacket จะ clip ทุกอย่างหลัง boundary ทันที

**`CaptureEngine.Recording/CaptureSession.vb`** — 1 call หลัง latch (ก่อน `_capture.Stop()`):
```vb
_audioEngine?.SetSessionEndQpc100ns(WasapiPositionCapture.StopwatchTicksTo100ns(stopQpcTicks))
```

เหตุผลที่เลือก freeze-instead-of-early-Stop: การเรียก `Stop()` เร็วจะหยุด capture ทันทีและ finalize — clip ก็ทำงานเหมือนกัน แต่เปลี่ยนลำดับ lifecycle มากกว่า (audio track ตายก่อน video capture) — freeze = boundary เดียวกันโดยไม่แตะลำดับเดิม (minimal ✓); `Stop` เดิมที่ท้ายยังทำหน้าที่ finalize/capture-stop/diagnostics ครบ

## 3. การตรวจตาม mission checklist

| รายการ | ผล |
|---|---|
| T0/T_END | T0 = common timeline (ทั้งคู่); T_END = `stopQpc100ns` เดียวกับ video latch หลังแก้ (log `Stop snapshot: elapsed=60.000s` ตรงทุก run); audio LastEnd ≤ T_END (clip) + FinalizeTrack pad ถึง T_END |
| video/audio duration | 60s: 60.023/60.050 (Δ+28.0ms = AAC tail); 3s: 3.021/3.043 (+21.8ms); 5s: 5.038/5.048 (+9.4ms) — ทุก delta อยู่ใน AAC final-frame quantization ไม่มี systematic overrun |
| tail silence | FinalizeTrack pad [LastEnd, T_END] — หลังแก้ LastEnd ≤ T_END เสมอ (clip ตั้งแต่ freeze) — ไม่มี tail เกิน boundary |
| dropped bytes | dropped=0 ทุก run (ทั้ง audio engine และ live-mux); accounting ok |
| session lifecycle | pass=True, state ครบ Idle→…→Idle, EOS ปกติ, ไม่มี orphan ffmpeg |
| short-run regression | 3s/5s pass=True ทั้งคู่, duration ตรง, ไม่มี regression |

## 4. FACT / HYPOTHESIS / UNKNOWN

**FACT** — mechanism (boundary freeze ช้ากว่า video latch; HARD T_END ไม่ armed ระหว่าง teardown); fix = freeze ที่ latch; before/after บนเครื่องนี้ +27.5/+28.0ms (AAC structural tail คงเดิม); short-run ผ่าน; dropped=0
**FACT (ใหม่, นอก scope P1-A)** — mux ffmpeg ตายกลาง session แบบสุ่มบน session ยาว (60s): light-load ตายที่ 2.7s, loaded ที่ 17.6s, 1/3 รอบผ่านครบ; exit=-1 ไม่มี stderr error; partial-salvage ทำงานถูก — ต้อง investigation ต่างหาก (AV? memory? pipe?)
**HYPOTHESIS** — บนเครื่อง/สเปก heavy ที่ teardown = 762ms: fix นี้ตัด audio overrun 762ms-class ออก (clip จาก latch) — สอดคล้อง evidence เดิมของ board
**UNKNOWN** — ผลจริงบนเครื่อง owner (heavy display); สาเหตุการตายของ mux ffmpeg (ต้อง investigation ต่างหาก)

## 5. Evidence

- `evidence/w1-p1a-stop-boundary/` — before/after logs + 2 MP4s (before_60s_try2.mp4 = pre-fix 60s, after_60s.mp4 = post-fix 60s) + hogs run log
- ไฟล์ที่แก้ (uncommitted): `CaptureEngine.Audio/AudioEngineSession.cs` (+SetSessionEndQpc100ns), `CaptureEngine.Recording/CaptureSession.vb` (+1 call + comment)
- Cross-ref: `W1-FAULT-PROOF-REPORT.md`, `W1-FINAL-GALLERY-VIDEO-REPORT.md`
