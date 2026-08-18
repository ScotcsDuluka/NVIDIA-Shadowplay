// FrameRecord.cs — per-frame data struct
// SPDX-License-Identifier: MIT

namespace V4_WGC_Timestamp_EdgeCase_Spike;

internal struct FrameRecord
{
    public long FrameIndex;
    public long SystemRelativeTimeTicks;
    public long DeltaFromPreviousSrtTicks;   // long.MinValue sentinel if first frame
    public long Pts;                          // SrtTicks - T0
    public long DeltaFromPreviousPtsTicks;    // same sentinel rule
    public DateTime WallClockUtcCaptured;     // for correlating with load/session timing only, NOT used in PTS math
}
