// SessionResult.cs — per-session aggregate struct
// SPDX-License-Identifier: MIT

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal struct SessionResult
{
    public int SessionIndex;
    public long FrameCount;
    public double WallElapsedSeconds;
    public long FirstSrt;
    public long LastSrt;
    public long FirstPts;
    public long LastPts;
    public long MinDelta;
    public long MaxDelta;
    public double AverageDelta;
    public long MedianDelta;
    public long P95Delta;
    public long P99Delta;
    public int EqualTimestampCount;
    public List<(long prevIdx, long currIdx, long srt)> EqualTimestampEvents;
    public int NegativeDeltaCount;
    public List<(long prevIdx, long currIdx, long prevSrt, long currSrt, long delta)> RegressionEvents;
    public int NegativePtsCount;
    public bool TimestampMonotonic;
    public bool PtsMonotonic;
    public string DisplayConfig;     // e.g. "1680x1050@DISPLAY1"
    public string LoadCondition;     // "idle" | "static-content" | "active-content" | "stress"
    public double AchievedFps;
}
