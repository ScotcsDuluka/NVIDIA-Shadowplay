# V3 WGC Runtime Timestamp Capture Spike

## Prerequisites
- Windows 10 1903+ (build 19041+)
- .NET 8 SDK (x64)
- NVIDIA GPU (for D3D11 device, though any D3D11 device works)
- Display connected and active

## Build & Run

```cmd
cd spikes\V3_WGC_Runtime_Timestamp_Spike
dotnet build -c Debug
dotnet run --no-build
```

Options:
- `dotnet run -- 60` — capture 60 seconds per session (default: 30)
- `dotnet run -- --no-restart` — skip session B restart test

## Output

The spike captures real `GraphicsCaptureFrame` instances and records
`SystemRelativeTime.Ticks` for each frame. It reports:

- Total frames captured
- Capture duration
- First/last timestamp (SystemRelativeTime.Ticks)
- First/last PTS (= Tn - T0)
- Min/max/average/median delta between consecutive timestamps
- Count of equal timestamps (delta == 0)
- Count of timestamp regressions (delta < 0)
- Count of negative PTS
- Timestamp and PTS monotonicity

Session A captures first, then session B (restart test) captures.
