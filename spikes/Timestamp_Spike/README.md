# P1-B.2-V2 Timestamp Spike

## Purpose

Validates the Engine-Rebuild timestamp model **before** production backend implementation.
Specifically:

- **V2-1**: Proves `Stopwatch` is backed by QPC (High Resolution Counter)
- **V2-2**: Proves QPC timestamps are monotonic (no backward jumps over 100,000 samples)
- **V2-3**: Proves the WGC `SystemRelativeTime` → QPC conversion formula is correct
- **V2-4**: Defines and validates the Engine PTS contract (60 FPS, jitter < 1%)

This is a **standalone spike** — does NOT modify Engine-Rebuild, Foundation, P1-B.1 Contract,
or any production code.

## Engine Timestamp Model Decided by This Spike

| Property | Value |
|----------|-------|
| Internal timestamp unit | QPC ticks (`Stopwatch` ticks) |
| WGC → QPC conversion | `qpc = wgcTicks * Stopwatch.Frequency / TimeSpan.TicksPerSecond` |
| PTS formula | `pts = qpcTimestamp - T0` |
| T0 | First frame's QPC timestamp |
| Clock source | `Stopwatch.GetTimestamp()` (QPC-backed) |

### Forbidden patterns

These patterns MUST NOT appear in production code:

- ❌ `DateTime.UtcNow` as a clock source — not monotonic, not high-resolution
- ❌ `TimeSpan.Ticks` as the Engine PTS unit — Engine PTS is QPC ticks
- ✅ Engine PTS = QPC timestamp − T0, in `Stopwatch` ticks

## Prerequisites

- Windows 10/11 (QPC behavior is Windows-specific)
- .NET 8 SDK (x64)
- Any CPU (the spike uses `PlatformTarget=x64` for consistency with the Engine-Rebuild target)

No NVIDIA GPU required — this spike validates the clock, not the GPU.

## Build & Run

```cmd
cd spikes\Timestamp_Spike
dotnet build -c Release
dotnet run -c Release
```

To capture output to a log file:

```cmd
dotnet run -c Release > timestamp_spike_output.log 2>&1
```

To run a single test:

```cmd
dotnet run -c Release -- --test V2-1
dotnet run -c Release -- --test V2-2
dotnet run -c Release -- --test V2-3
dotnet run -c Release -- --test V2-4
```

## Expected Output

```
============================================================
 P1-B.2-V2 Timestamp Spike
 Branch: Engine-Rebuild (spike — does NOT modify repo)
 Runtime: .NET 8, Windows, x64
============================================================

Engine timestamp model under validation:
  Internal unit: QPC ticks (Stopwatch ticks)
  WGC → QPC conversion: qpc = wgcTicks * Freq / TicksPerSecond
  PTS formula:          pts = qpcTimestamp - T0

============================================================
 V2-1 — QPC CLOCK TEST
============================================================

Stopwatch.IsHighResolution: True
Stopwatch.Frequency:        10000000 Hz
Timestamp:                  12345678901234
Seconds:                    1234567.89012340

Acceptance: IsHighResolution==true : PASS
Acceptance: Frequency > 1MHz       : PASS (10,000,000 Hz)

V2-1 Result: PASS

... (V2-2, V2-3, V2-4 output) ...

============================================================
 FINAL VERDICT
============================================================

Tests run:    4
Tests passed: 4
Tests failed: 0

P1-B.2-V2 RESULT
----------------
Timestamp source:        QPC (Stopwatch)
Conversion (V2-3):       PASS
Monotonic (V2-2):         PASS
Engine timestamp unit:   Stopwatch ticks (QPC)
STATUS:                   RESOLVED

All 4 tests passed. Engine timestamp model validated.
```

## Acceptance Criteria

| # | Criterion | Test | Acceptance |
|---|-----------|------|------------|
| 1 | Stopwatch high resolution | V2-1 | `IsHighResolution == true` AND `Frequency > 1,000,000` Hz |
| 2 | Monotonic | V2-2 | Backward jumps == 0 over 100,000 samples |
| 3 | WGC conversion error | V2-3 | Error < 1 ms for durations 0s, 1s, 10s, 60s, 3600s |
| 4 | PTS delta stable | V2-4 | Average delta ~= `Stopwatch.Frequency / FPS` (within 1 tick), jitter < 1% |
| 5 | No mixed clock domains | All | Engine uses QPC only; never DateTime.UtcNow; never TimeSpan.Ticks as PTS unit |

## Files

| File | Purpose |
|------|---------|
| `CaptureEngine.Timestamp.Spike.csproj` | .NET 8 x64 Windows project file |
| `Program.cs` | Entry point, runs all 4 tests, prints final verdict |
| `Utils/TimestampConverter.cs` | Conversion utilities (WGC ↔ QPC, PTS computation) |
| `Tests/QpcClockTest.cs` | V2-1 — Stopwatch high-resolution validation |
| `Tests/MonotonicTest.cs` | V2-2 — Monotonic timestamp validation (100k samples) |
| `Tests/ConversionTest.cs` | V2-3 — WGC → QPC conversion formula validation |
| `Tests/PtsContractTest.cs` | V2-4 — Engine PTS contract (60 FPS, jitter < 1%) |
| `Timestamp_Spike_Report.md` | Report template — OWNER fills with actual runtime output |
| `README.md` | This file |

## Relationship to Prior V2 Spike (`spikes/V2_WGC_Timestamp_Spike/`)

The prior V2 spike (commit `d84e91f`) validated the **WGC timestamp edge cases** (duplicate
timestamps, regressions, PTS zero-basing with `PTS = SystemRelativeTime.Ticks - T0`).

This spike (`spikes/Timestamp_Spike/`) validates the **Engine timestamp model itself** — specifically
the decision to use QPC ticks (`Stopwatch` ticks) as the Engine's internal timestamp unit, and the
conversion formula needed to translate WGC's 100-ns ticks into the Engine's QPC clock domain.

The two spikes explore different aspects of the timestamp architecture. The prior spike assumed
PTS was in WGC's native 100-ns ticks; this spike assumes PTS is in QPC ticks. OWNER must decide
which model to use in production based on both spikes' evidence.

## Cross-References

- Prior V2 spike: `spikes/V2_WGC_Timestamp_Spike/`
- V3 runtime spike: `spikes/V3_WGC_Runtime_Timestamp_Spike/`
- V4 evidence-grade spike: `spikes/V4_WGC_Timestamp_EdgeCase_Spike/`
- V5-A execution protocol: `/home/z/my-project/download/V5-A_Execution_Protocol.md`
- P1-E validation matrix: `/home/z/my-project/download/P1-E_Validation_Matrix.md`
- P1-E runtime gap report: `/home/z/my-project/download/P1-E_Runtime_Gap_Report.md`

## Isolation Guarantees

This spike:
- ✅ Does NOT reference `CaptureEngine` or `CaptureEngine.Video` projects
- ✅ Does NOT modify any production code
- ✅ Does NOT modify `Engine-Rebuild` branch (other than adding this spike)
- ✅ Does NOT modify P1-B.1 Contract
- ✅ Does NOT add production dependencies
- ✅ Uses only .NET BCL (`System.Diagnostics.Stopwatch`, `System.TimeSpan`)

## License

SPDX-License-Identifier: MIT
Spike code — not production.
