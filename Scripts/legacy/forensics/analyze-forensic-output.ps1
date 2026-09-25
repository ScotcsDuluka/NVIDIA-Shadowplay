# PowerShell script to analyze forensic output from console
$output = @"
[2026-09-11 16:56:20.211] [INFO] Validate CaptureSession: CFR TICK 1 SELECTION DEBUG:
[2026-09-11 16:56:20.211] [INFO] Validate   targetQpc100ns=903363081328
[2026-09-11 16:56:20.211] [INFO] Validate   timelineStartQpc100ns=903363081328
[2026-09-11 16:56:20.211] [INFO] Validate   nextTick=903363081328
[2026-09-11 16:56:20.211] [INFO] Validate   _timelineStartTicks=903363081328
[2026-09-11 16:56:20.211] [INFO] Validate   cfrTick=903363081335
[2026-09-11 16:56:20.211] [INFO] Validate   cfrQpc=903363081335
[2026-09-11 16:56:20.211] [INFO] Validate CaptureSession: CFR TICK 1 SELECTED FRAME:
[2026-09-11 16:56:20.211] [INFO] Validate   selectedSeq=1318
[2026-09-11 16:56:20.211] [INFO] Validate   selectedTs=903362269238
[2026-09-11 16:56:20.211] [INFO] Validate   selectedLag=812090ns
[2026-09-11 16:56:20.211] [INFO] Validate   selectedTick=903363082177
[2026-09-11 16:56:20.211] [INFO] Validate   selectedQpc=903363082178
[2026-09-11 16:56:20.211] [INFO] Validate CaptureSession: CFR TICK 1 SELECTED FRAME:
[2026-09-11 16:56:20.211] [INFO] Validate   selectedSeq=1319
[2026-09-11 16:56:20.211] [INFO] Validate   selectedTs=903362325955
[2026-09-11 16:56:20.211] [INFO] Validate   selectedLag=755373ns
[2026-09-11 16:56:20.211] [INFO] Validate   selectedTick=903363082363
[2026-09-11 16:56:20.211] [INFO] Validate   selectedQpc=903363082364
[2026-09-11 16:56:20.216] [INFO] Validate NvencEncoderBackend: keyframe at seq 3125 (picType=3)
[2026-09-11 16:56:20.216] [INFO] Validate CaptureSession: CFR TICK 1 MUXER FEED:
[2026-09-11 16:56:20.216] [INFO] Validate   muxFeedTick=903363129154
[2026-09-11 16:56:20.216] [INFO] Validate   muxFeedQpc=903363129154
[2026-11 16:56:20.216] [INFO] Validate   packet.Metadata.PresentationTimestampTicks=903362325955
[2026-09-11 16:56:20.216] [INFO] Validate   packet.Metadata.Sequence=3125
[2026-09-11 16:56:20.228] [INFO] Validate CaptureSession: CFR TICK 2 SELECTION DEBUG:
[2026-09-11 16:56:20.228] [INFO] Validate   targetQpc100ns=903363247995
[2026-09-11 16:56:20.228] [INFO] Validate   timelineStartQpc100ns=903363081328
[2026-09-11 16:56:20.228] [INFO] Validate   nextTick=903363247995
[2026-09-11 16:56:20.228] [INFO] Validate   _timelineStartTicks=903363081328
[2026-09-11 16:56:20.228] [INFO] Validate   cfrTick=903363248316
[2026-09-11 16:56:20.228] [INFO] Validate   cfrQpc=903363248316
[2026-09-11 16:56:20.231] [INFO] Validate CaptureSession: CFR TICK 2 MUXER FEED:
[2026-09-11 16:56:20.231] [INFO] Validate   muxFeedTick=903363280298
[2026-09-11 16:56:20.231] [INFO] Validate   muxFeedQpc=903363280298
[2026-09-11 16:56:20.231] [INFO] Validate   packet.Metadata.PresentationTimestampTicks=903362325955
[2026-09-11 16:56:20.231] [INFO] Validate   packet.Metadata.Sequence=3126
"@ -split "`n"

# Extract timing data
$timingData = @()

foreach ($line in $output) {
    if ($line -match "selectedTs=(\d+)") {
        $ts = $matches[1]
        $timingData += @{Type="selectedTs"; Value=$ts}
    }
    elseif ($line -match "selectedLag=(\d+)ns") {
        $lag = $matches[1]
        $timingData += @{Type="selectedLag"; Value=$lag}
    }
    elseif ($line -match "selectedTick=(\d+)") {
        $tick = $matches[1]
        $timingData += @{Type="selectedTick"; Value=$tick}
    }
    elseif ($line -match "muxFeedTick=(\d+)") {
        $mux = $matches[1]
        $timingData += @{Type="muxFeedTick"; Value=$mux}
    }
    elseif ($line -match "cfrTick=(\d+)") {
        $cfr = $matches[1]
        $timingData += @{Type="cfrTick"; Value=$cfr}
    }
    elseif ($line -match "_timelineStartTicks=(\d+)") {
        $timeline = $matches[1]
        $timingData += @{Type="timelineStart"; Value=$timeline}
    }
}

# Display results
Write-Host "Timing Analysis Results:"
Write-Host "======================"

foreach ($data in $timingData) {
    Write-Host "$($data.Type): $($data.Value)"
}

# Calculate time differences
$firstSelectedTs = $timingData | Where-Object {$_.Type -eq "selectedTs"} | Select-Object -First 1 | Select-Object -ExpandProperty Value
$firstSelectedTick = $timingData | Where-Object {$_.Type -eq "selectedTick"} | Select-Object -First 1 | Select-Object -ExpandProperty Value
$firstMuxFeedTick = $timingData | Where-Object {$_.Type -eq "muxFeedTick"} | Select-Object -First 1 | Select-Object -ExpandProperty Value
$firstCfrTick = $timingData | Where-Object {$_.Type -eq "cfrTick"} | Select-Object -First 1 | Select-Object -ExpandProperty Value
$timelineStart = $timingData | Where-Object {$_.Type -eq "timelineStart"} | Select-Object -First 1 | Select-Object -ExpandProperty Value

Write-Host "`nTime Differences (ns):"
Write-Host "======================"
Write-Host "SelectedTs to SelectedTick: $($firstSelectedTick - $firstSelectedTs)"
Write-Host "SelectedTick to MuxFeedTick: $($firstMuxFeedTick - $firstSelectedTick)"
Write-Host "SelectedTick to CfrTick: $($firstCfrTick - $firstSelectedTick)"
Write-Host "TimelineStart to SelectedTick: $($firstSelectedTick - $timelineStart)"