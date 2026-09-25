# V4 WGC Timestamp Edge-Case Spike

## Prerequisites
- Windows 10 1903+ (build 19041+)
- .NET 8 SDK (x64)
- NVIDIA GPU (for D3D11 device)
- Display connected and active

## Build & Run

```cmd
cd spikes\V4_WGC_Timestamp_EdgeCase_Spike
dotnet build -c Release
```

### Test 1 — Long Runtime (default 600s = 10 min)
```cmd
dotnet run -c Release -- --mode longrun --duration 600
```

### Test 4 — Session Recreation (10 × 15s sessions)
```cmd
dotnet run -c Release -- --mode sessions --count 10 --duration-per-session 15
```

### Test 5 — Stress (90s per condition)
```cmd
dotnet run -c Release -- --mode stress --condition static --duration 90
dotnet run -c Release -- --mode stress --condition active --duration 90 --load true
```

### All tests
```cmd
dotnet run -c Release -- --mode all
```

## Output

Evidence files are written to `bin/Release/net8.0-windows10.0.19041.0/evidence/`:
- `v4_session_{mode}_{N}_frames.csv` — raw per-frame data
- `v4_session_{mode}_{N}_summary.json` — per-session summary
- `v4_final_report.json` — final rollup

## Tests
1. Long Runtime — 10 min continuous capture
2. Duplicate Timestamps — analyzed from all sessions
3. Timestamp Regressions — analyzed from all sessions
4. Session Recreation — 10 back-to-back sessions
5. Runtime Stress — CPU load + static/active content
6. Large Gap Reporting — top 10 largest deltas
7. Resolution/Display Mode — NOT TESTED (no safe alt display)
8. Suspend/Resume — NOT TESTED (no safe automated mechanism)
