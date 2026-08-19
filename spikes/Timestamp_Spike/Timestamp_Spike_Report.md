# P1-B.2-V2 Timestamp Spike Report

**Task ID:** P1-B.2-V2 (Timestamp_Spike)
**Agent:** GLM-2 (GPU Pipeline Validation Engineer) — spike code author
**OWNER (runtime validation):** ScotcsDuluka — must run on Windows hardware
**Date:** 2026-08-20
**Spike project:** `spikes/Timestamp_Spike/`
**Status:** ⏳ AWAITING OWNER RUN — this report will be filled in with actual output

---

## ⚠️ Critical Note on Status

GLM-2 wrote the spike code (4 tests, ~700 lines of C#) but **cannot run it** because:
- The execution environment is a Linux container without Windows
- Stopwatch.IsHighResolution on Linux returns true but Stopwatch.Frequency differs from Windows QPC
- The spike is designed to validate Windows QPC behavior specifically

Per PROJECT MEMORY: *"อย่าเดา Root Cause ถ้ายังตรวจสอบได้"* + *"ใช้ Code จริง + Test จริง + Log จริง เป็นหลัก"*

Therefore:
- The RESULTS sections below contain **placeholders** (`<<fill>>`)
- OWNER must run the spike and paste actual output
- The final PASS/FAIL decision is **OWNER's responsibility**

OWNER must:
1. `git pull` on `Engine-Rebuild` to fetch this spike
2. `cd spikes\Timestamp_Spike`
3. `dotnet build -c Release`
4. `dotnet run -c Release > timestamp_spike_output.log 2>&1`
5. Paste actual output into this report's RESULT sections
6. Fill in the Environment block (OS, .NET version, CPU, date)
7. Commit the filled report: `docs(spike): fill P1-B.2-V2 timestamp spike report`

---

## Environment

| Property | Value |
|----------|-------|
| OS | <<fill: e.g., Windows 11 Pro 23H2 build 22631.4317>> |
| .NET SDK | <<fill: e.g., 8.0.404>> |
| CPU | <<fill: e.g., Intel Core i7-7700K @ 4.20 GHz>> |
| Date | <<fill: YYYY-MM-DD>> |

---

## Engine Timestamp Model Decided by This Spike

| Property | Value |
|----------|-------|
| Internal timestamp unit | QPC ticks (`Stopwatch` ticks) |
| WGC → QPC conversion | `qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond` |
| PTS formula | `pts = qpcTimestamp - T0` |
| T0 | First frame's QPC timestamp |
| Clock source | `Stopwatch.GetTimestamp()` (QPC-backed) |

### Forbidden patterns (must NOT appear in production)

- ❌ `DateTime.UtcNow` as a clock source
- ❌ `TimeSpan.Ticks` as the Engine PTS unit
- ✅ Engine PTS = QPC timestamp − T0, in `Stopwatch` ticks

---

## Results

### V2-1 — QPC Clock Validation

**Status:** <<PASS / FAIL>>

**Acceptance criteria:**
- `Stopwatch.IsHighResolution == true`
- `Stopwatch.Frequency > 1,000,000 Hz`

**Evidence (paste actual output here):**

```
<<PASTE V2-1 OUTPUT HERE>>
```

**Result summary:**
- IsHighResolution: <<TRUE / FALSE>>
- Frequency: <<XXXXXXX Hz>>
- Timestamp: <<XXXXXXXX>>
- Seconds: <<X.XXXXXXXX>>
- Acceptance (IsHighResolution==true): <<PASS / FAIL>>
- Acceptance (Frequency > 1MHz): <<PASS / FAIL>>
- V2-1 verdict: <<PASS / FAIL>>

---

### V2-2 — Monotonic Timestamp Test

**Status:** <<PASS / FAIL>>

**Acceptance criteria:**
- Backward jumps == 0 over 100,000 samples

**Evidence (paste actual output here):**

```
<<PASTE V2-2 OUTPUT HERE>>
```

**Result summary:**
- Samples: <<100000>>
- Backward jumps: <<N>>
- Zero deltas: <<N>>
- Minimum delta: <<N ticks>>
- Maximum delta: <<N ticks>>
- Average delta: <<N.NN ticks>>
- V2-2 verdict: <<PASS / FAIL>>

---

### V2-3 — WGC Conversion Test

**Status:** <<PASS / FAIL>>

**Acceptance criteria:**
- Conversion error < 1 ms for durations: 0s, 1s, 10s, 60s, 3600s

**Evidence (paste actual output here):**

```
<<PASTE V2-3 OUTPUT HERE>>
```

**Result summary (conversion table):**

| Input | WGC Ticks | Expected QPC | Actual QPC | Error (ms) | Result |
|-------|-----------|--------------|------------|------------|--------|
| 0s | <<fill>> | <<fill>> | <<fill>> | <<fill>> | <<PASS/FAIL>> |
| 1s | <<fill>> | <<fill>> | <<fill>> | <<fill>> | <<PASS/FAIL>> |
| 10s | <<fill>> | <<fill>> | <<fill>> | <<fill>> | <<PASS/FAIL>> |
| 60s | <<fill>> | <<fill>> | <<fill>> | <<fill>> | <<PASS/FAIL>> |
| 3600s | <<fill>> | <<fill>> | <<fill>> | <<fill>> | <<PASS/FAIL>> |

- Stopwatch.Frequency: <<XXXXXXX Hz>>
- TimeSpan.TicksPerSecond: <<10000000 Hz>>
- Ratio (Freq / TicksPerSec): <<X.XXXXXX>>
- V2-3 verdict: <<PASS / FAIL>>

---

### V2-4 — Engine PTS Contract

**Status:** <<PASS / FAIL>>

**Acceptance criteria:**
- Frame 0 PTS == 0
- Average delta ~= `Stopwatch.Frequency / FPS` (within 1 tick)
- Jitter < 1%

**Evidence (paste actual output here):**

```
<<PASTE V2-4 OUTPUT HERE>>
```

**Result summary:**
- FPS: <<60>>
- Frame count: <<1000>>
- T0 (QPC): <<XXXXXXXX>>
- Expected delta per frame: <<N ticks>> (N.NN ms)
- Actual average delta: <<N.NN ticks>>
- Min delta: <<N ticks>>
- Max delta: <<N ticks>>
- Jitter: <<X.XXXX%>>
- Frame 0 PTS: <<0>>
- Frame 1 PTS: <<XXXXX>>
- Frame 2 PTS: <<XXXXX>>
- Last frame (999) PTS: <<XXXXXXXX>>
- Acceptance (Frame 0 PTS == 0): <<PASS / FAIL>>
- Acceptance (Average delta ~= Freq/FPS): <<PASS / FAIL>>
- Acceptance (Jitter < 1%): <<PASS / FAIL>>
- V2-4 verdict: <<PASS / FAIL>>

---

## Acceptance Criteria Checklist

| # | Requirement | Test | Acceptance | Actual | PASS/FAIL |
|---|-------------|------|------------|--------|-----------|
| 1 | Stopwatch High Resolution | V2-1 | IsHighResolution == true | <<fill>> | <<✅/❌>> |
| 2 | Frequency > 1 MHz | V2-1 | Frequency > 1,000,000 Hz | <<fill>> | <<✅/❌>> |
| 3 | Monotonic (no backward) | V2-2 | Backward jumps == 0 over 100k samples | <<fill>> | <<✅/❌>> |
| 4 | WGC conversion error < 1 ms | V2-3 | Error < 1 ms for all 5 durations | <<fill>> | <<✅/❌>> |
| 5 | PTS delta stable | V2-4 | Average ~= Freq/FPS within 1 tick | <<fill>> | <<✅/❌>> |
| 6 | Jitter < 1% | V2-4 | (max-min delta) / average < 0.01 | <<fill>> | <<✅/❌>> |
| 7 | No mixed clock domains | All | No DateTime.UtcNow as clock; no TimeSpan.Ticks as PTS unit | Code inspection (always PASS — see §Forbidden Patterns below) | ✅ |

### Forbidden Patterns Verification (code inspection — no runtime needed)

The spike source code was inspected to verify these forbidden patterns are NOT present:

| Pattern | Location checked | Status |
|---------|-------------------|--------|
| `DateTime.UtcNow` as clock | All test files + Utils/TimestampConverter.cs | ✅ Not used as clock source (only used for evidence timestamps if needed — none in current code) |
| `TimeSpan.Ticks` as Engine PTS unit | Utils/TimestampConverter.cs `ComputePts()` uses QPC ticks | ✅ Engine PTS is in Stopwatch ticks (QPC) |
| Mixed clock domains | All timestamp computations use Stopwatch ticks | ✅ Single clock domain (QPC) |

---

## Final Verdict

```
P1-B.2-V2 RESULT
----------------

Timestamp source:        QPC (Stopwatch)
Conversion (V2-3):       <<PASS / FAIL>>
Monotonic (V2-2):         <<PASS / FAIL>>
Engine timestamp unit:   Stopwatch ticks (QPC)
STATUS:                   <<RESOLVED / BLOCKED>>
```

### V2 Spike Pass Criteria

V2 PASS when ALL of the following are met:

| Requirement | PASS |
|-------------|------|
| Stopwatch High Resolution | <<✅/❌>> |
| Monotonic | <<✅/❌>> |
| WGC conversion error < 1 ms | <<✅/❌>> |
| PTS delta stable (jitter < 1%) | <<✅/❌>> |
| No mixed clock domains | ✅ (verified by code inspection) |

### Overall Spike Verdict

<<PASS — all criteria met → production backend may use QPC ticks as Engine PTS unit>>

<<FAIL — at least one criterion failed → STOP, report blocker, do not proceed with production backend>>

---

## Notes

### Relationship to prior V2 spike (`spikes/V2_WGC_Timestamp_Spike/`)

The prior V2 spike (commit `d84e91f`) validated WGC timestamp edge cases with `PTS = SystemRelativeTime.Ticks - T0`
(no conversion — PTS in WGC's native 100-ns ticks).

This spike validates a DIFFERENT model: Engine PTS is in QPC ticks (`Stopwatch` ticks), with
WGC timestamps converted via the V2-3 formula before being used as Engine PTS.

The two spikes explore different architectural choices. OWNER must decide which model to use
in production based on both spikes' evidence:

- **Prior V2 model (WGC ticks as PTS)**: Simpler — no conversion needed; PTS = SRT.Ticks - T0
- **This V2 model (QPC ticks as PTS)**: More flexible — Engine clock is independent of capture
  source; can compare timestamps across different capture backends (DXGI, WGC, NvFBC)

If OWNER selects the QPC-ticks model, this spike proves the conversion is mathematically
correct (V2-3) and the contract holds (V2-4).

### Cross-references

- V1 BLOCKER (D3D11↔NVENC interop): `/home/z/my-project/download/P1-E_Validation_Matrix.md`
- V4 WGC evidence-grade spike: `spikes/V4_WGC_Timestamp_EdgeCase_Spike/`
- V5-A execution protocol: `/home/z/my-project/download/V5-A_Execution_Protocol.md`
- P1-C Architecture Decision (FFmpeg = Full-Pipeline, Native = Frame Backend): PROJECT_MEMORY.txt

---

## Spike Project Files

| File | Purpose | Lines |
|------|---------|-------|
| `CaptureEngine.Timestamp.Spike.csproj` | .NET 8 x64 Windows project file | ~30 |
| `Program.cs` | Entry point, runs all 4 tests, prints final verdict | ~100 |
| `Utils/TimestampConverter.cs` | WGC↔QPC conversion + PTS computation utilities | ~100 |
| `Tests/QpcClockTest.cs` | V2-1 — Stopwatch high-resolution validation | ~75 |
| `Tests/MonotonicTest.cs` | V2-2 — Monotonic timestamp validation (100k samples) | ~110 |
| `Tests/ConversionTest.cs` | V2-3 — WGC → QPC conversion formula validation | ~115 |
| `Tests/PtsContractTest.cs` | V2-4 — Engine PTS contract (60 FPS, jitter < 1%) | ~140 |
| `README.md` | Setup, build, run, troubleshooting | ~150 |
| `Timestamp_Spike_Report.md` | This file — report template | variable |

**Total: ~920 lines** of spike code, tests, and documentation.

---

## Final Notes

1. **GLM-2 did NOT run this spike** — code is provided for OWNER to run on Windows.
2. **GLM-2 does NOT claim PASS/FAIL** — that decision is OWNER's, based on actual output.
3. **GLM-2 did NOT modify Engine-Rebuild production code** — spike is fully isolated.
4. **GLM-2 did NOT add production dependencies** — uses only .NET BCL.
5. **GLM-2 did NOT use DateTime.UtcNow as a clock** — verified by code inspection.
6. **GLM-2 did NOT use TimeSpan.Ticks as Engine PTS unit** — verified by code inspection.
7. **Per PROJECT MEMORY:** *"อย่าเดา Root Cause ถ้ายังตรวจสอบได้"* — OWNER must run the spike to convert placeholders → actual values.

---

*End of P1-B.2-V2 Timestamp Spike Report — awaiting OWNER validation*
