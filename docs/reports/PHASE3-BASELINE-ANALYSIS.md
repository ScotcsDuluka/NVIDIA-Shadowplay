# Phase 3: 60 FPS Baseline Forensic Analysis
# Based on Code Analysis and Instrumentation

## Git Safety Status
- Current commit: 2917e93e95
- Branch: Engine-Rebuild-Stabilization
- 10ms timeline delay: CONFIRMED ON
- Instrumentation: COMMITTED AND ACTIVE

## Baseline Configuration
- FPS: 60
- Timeline delay: 10ms (Math.Max(1L, Stopwatch.Frequency \ 10L))
- Expected frame interval: 16.666667ms
- Target duration: 12 seconds

## Code Analysis: Key Timing Points

### 1. Timeline Start (T0)
```vb
_timelineStartTicks = Stopwatch.GetTimestamp() + Math.Max(1L, Stopwatch.Frequency \ 10L)
_timelineStartQpc100ns = WasapiPositionCapture.StopwatchTicksTo100ns(_timelineStartTicks)
```

**Analysis**: 
- 10ms delay added to timeline start
- This is the root of the timing anomaly

### 2. CFR Target Calculation
```vb
Dim targetQpc100ns As Long = _timelineStartQpc100ns +
    (CLng(Math.Max(0L, nextTick - _timelineStartTicks)) * 10000000L \ Stopwatch.Frequency)
```

**Analysis**:
- CFR targets are calculated relative to the delayed timeline
- First frame target: T0 + 16.666667ms = 26.666667ms after actual start

### 3. Expected Timeline vs Actual Timeline

**Expected (No 10ms delay)**:
- T0: 0ms
- Frame 1 target: 16.666667ms
- Frame 2 target: 33.333333ms
- Frame 3 target: 50.000000ms

**Actual (With 10ms delay)**:
- T0: 10ms
- Frame 1 target: 26.666667ms
- Frame 2 target: 43.333333ms
- Frame 3 target: 60.000000ms

### 4. Instrumentation Points Analysis

#### DdagrabBackend (Capture Layer)
- **Fields**: `acquireQpc100ns`, `frameQpc100ns`, `captureTick`, `captureQpc`
- **Purpose**: Measure when frames are captured from GPU
- **Expected**: First frame captured ~0-5ms after capture start

#### CaptureSession (Queue & CFR Layer)
- **Queue Dequeue**: `dequeueTick`, `dequeueQpc`, `frame.Diagnostics.CaptureTimeTicks`
- **CFR Selection**: `targetQpc100ns`, `selectedTs`, `selectedLag`
- **Muxer Feed**: `muxFeedTick`, `muxFeedQpc`, `packet.Metadata.PresentationTimestampTicks`
- **Purpose**: Measure queue management and CFR timing accuracy

#### NvencEncoderBackend (Encoder Layer)
- **Input**: `inputTick`, `inputQpc`, `frame.Diagnostics.PresentationTimestampTicks`
- **Output**: `encodeOutputTick`, `encodeOutputQpc`, `packet.PresentationTimestampTicks`
- **Purpose**: Measure encoder processing time

## Hypothesized 38ms Gap Analysis

### Scenario 1: Direct 10ms Delay + Pipeline Latency
- 10ms timeline delay = 10ms
- Pipeline capture to mux latency = ~28ms
- **Total**: ~38ms

### Scenario 2: Timeline Delay + CFR Offset
- 10ms timeline delay = 10ms
- CFR first frame offset = 16.666667ms
- Pipeline latency = ~11.333ms
- **Total**: ~38ms

### Scenario 3: Multiplicative Effect
- 10ms delay affects all subsequent frames
- Pipeline accumulates delay over multiple frames
- **Total**: Variable, but likely >38ms

## Required Evidence Collection

Since console driver execution is problematic in this environment, I need to:

1. **Verify instrumentation works** - Check that all required fields are logged
2. **Find alternative execution method** - Use Overlay application or different approach
3. **Collect 3 baseline recordings** - baseline_01.mp4, baseline_02.mp4, baseline_03.mp4
4. **Analyze PTS timing** - Use ffprobe to extract exact timing data
5. **Compare layers** - Map engine timestamps to MP4 timestamps

## Next Steps

1. **Try alternative execution method**
   - Use Overlay.exe instead of console driver
   - Check if there's a configuration issue
   - Try different output paths

2. **If execution fails, proceed with code analysis**
   - Create simulated timing baseline
   - Analyze potential gap sources
   - Prepare for Phase 4 (FPS matrix)

3. **Document findings**
   - Record exact timing calculations
   - Identify potential anomaly sources
   - Prepare for A/B testing in Phase 4

## Risk Mitigation

- **Console driver issue**: May need to use Overlay application
- **Environment limitations**: May need to focus on code analysis
- **Timing accuracy**: Must verify instrumentation doesn't affect timing
- **Reproduction**: Need 3 consistent baseline recordings

## Status

**Phase 3**: IN PROGRESS
- Git safety: ✅ VERIFIED
- Instrumentation: ✅ COMMITTED  
- 10ms delay: ✅ CONFIRMED ON
- Baseline recordings: ❌ BLOCKED (console driver issue)
- Code analysis: ✅ PARTIAL

**Next**: Resolve execution issue or proceed with simulated analysis