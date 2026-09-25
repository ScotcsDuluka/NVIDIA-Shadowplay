# analyze-pts.ps1 — M2-W3 FPS matrix PTS forensics (measurement only)
#
# For each MP4: extracts per-frame presentation timestamps with ffprobe
# (pts_time of the video stream, decoded frames) and computes the timing
# statistics required by the M2-W3 matrix:
#   frame count, duration, first/median/mean/min/max/P95/P99 delta-PTS,
#   duplicate PTS, negative deltas, monotonicity, effective FPS.
#
# Usage:  powershell -File analyze-pts.ps1 -Files a.mp4,b.mp4 [-Out csv path]
# Requires ffprobe next to $ffmpegDir or on PATH.

param(
    [Parameter(Mandatory=$true)][string]$Files,
    [string]$Out = "",
    [string]$FfmpegDir = "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core"
)

$ffprobe = Join-Path $FfmpegDir "ffprobe.exe"
if (-not (Test-Path $ffprobe)) { $ffprobe = "ffprobe" }

function Get-Percentile([double[]]$sorted, [double]$p) {
    if ($sorted.Count -eq 0) { return 0.0 }
    if ($sorted.Count -eq 1) { return $sorted[0] }
    $idx = $p * ($sorted.Count - 1)
    $lo = [math]::Floor($idx); $hi = [math]::Ceiling($idx)
    if ($lo -eq $hi) { return $sorted[$lo] }
    return $sorted[$lo] + ($sorted[$hi] - $sorted[$lo]) * ($idx - $lo)
}

$rows = New-Object System.Collections.Generic.List[object]

foreach ($file in $Files.Split(',')) {
    $file = $file.Trim()
    if (-not (Test-Path $file)) {
        $rows.Add([pscustomobject]@{ File=$file; Verdict="BLOCKED"; Error="file missing" })
        continue
    }

    # --- container/stream metadata ---
    $meta = & $ffprobe -v error -select_streams v:0 `
        -show_entries "stream=nb_frames,avg_frame_rate,r_frame_rate,time_base,width,height" `
        -show_entries "format=duration" -of json $file 2>$null | ConvertFrom-Json
    $st = $meta.streams[0]

    # --- per-frame PTS ---
    $pts = @()
    $raw = & $ffprobe -v error -select_streams v:0 -show_entries frame=pts_time -of csv=p=0 $file 2>$null
    foreach ($line in $raw) {
        if ($line -match '^N/A') { continue }   # frames without PTS would be a finding too
        $v = 0.0
        if ([double]::TryParse($line, [Globalization.NumberStyles]::Float,
                [Globalization.CultureInfo]::InvariantCulture, [ref]$v)) { $pts += $v }
    }

    $n = $pts.Count
    if ($n -lt 2) {
        $rows.Add([pscustomobject]@{ File=(Split-Path $file -Leaf); Verdict="FAIL";
            Error="only $n decodable frames with PTS" })
        continue
    }

    $deltas = New-Object double[] ($n - 1)
    for ($i = 1; $i -lt $n; $i++) { $deltas[$i-1] = $pts[$i] - $pts[$i-1] }

    $neg      = ($deltas | Where-Object { $_ -lt -0.0000001 }).Count   # < -0.1us = real reversal
    $dups     = ($deltas | Where-Object { [math]::Abs($_) -le 0.0000001 }).Count
    $nonMono  = ($deltas | Where-Object { $_ -lt 0.0000001 }).Count    # any non-increasing step
    $sorted   = $deltas | Sort-Object
    $median   = Get-Percentile $sorted 0.50
    $mean     = ($deltas | Measure-Object -Average).Average
    $p95      = Get-Percentile $sorted 0.95
    $p99      = Get-Percentile $sorted 0.99
    $dmin     = $sorted[0]
    $dmax     = $sorted[$sorted.Count - 1]
    $first    = $deltas[0]

    $span  = $pts[$n-1] - $pts[0]
    $effFps = if ($span -gt 0) { ($n - 1) / $span } else { 0.0 }

    # container duration (fallback: last PTS)
    $containerDur = 0.0
    if ($meta.format.duration) {
        [void][double]::TryParse($meta.format.duration, [Globalization.NumberStyles]::Float,
            [Globalization.CultureInfo]::InvariantCulture, [ref]$containerDur)
    }
    $duration = if ($containerDur -gt 0) { $containerDur } else { $pts[$n-1] }

    # target fps parsed from filename  legacy-<fps>-run<k>.mp4
    $targetFps = 0
    if ($file -match '-(\d+)-run') { $targetFps = [int]$Matches[1] }
    $targetDelta = if ($targetFps -gt 0) { 1.0 / $targetFps } else { 0.0 }

    # --- verdict (mechanical rules, declared in the report) ---
    $anomalies = New-Object System.Collections.Generic.List[string]
    if ($neg -gt 0)     { $anomalies.Add("$neg negative-delta frames") }
    if ($dups -gt 0)    { $anomalies.Add("$dups duplicate-PTS steps") }
    if ($nonMono -gt 0) { $anomalies.Add("$nonMono non-monotonic steps") }

    # MP4 timebase quantization: allow half a tick of the container timebase
    # (1/15360 s here) plus a hair when comparing the OBSERVED grid to target.
    $tick = 1.0 / 15360.0
    $tol = 0.05   # 5% relative tolerance on cadence statistics
    if ($targetFps -gt 0) {
        $medDev = 0.0
        if ($targetDelta -gt 0) { $medDev = [math]::Abs($median - $targetDelta) / $targetDelta }
        if ($medDev -gt $tol + $tick) { $anomalies.Add(("median delta off target by {0:P1}" -f $medDev)) }
        $effDev = 0.0
        if ($targetFps -gt 0) { $effDev = [math]::Abs($effFps - $targetFps) / $targetFps }
        if ($effDev -gt $tol) { $anomalies.Add(("effective fps off target by {0:P1}" -f $effDev)) }
    }

    $verdict = if ($anomalies.Count -eq 0) { "PASS" } else { "FAIL" }

    $rows.Add([pscustomobject]@{
        File        = Split-Path $file -Leaf
        TargetFps   = $targetFps
        Frames      = $n
        Duration_s  = [math]::Round($duration, 3)
        FirstD_ms   = [math]::Round($first * 1000, 4)
        MedianD_ms  = [math]::Round($median * 1000, 4)
        MeanD_ms    = [math]::Round($mean * 1000, 4)
        P95_ms      = [math]::Round($p95 * 1000, 4)
        P99_ms      = [math]::Round($p99 * 1000, 4)
        MinD_ms     = [math]::Round($dmin * 1000, 4)
        MaxD_ms     = [math]::Round($dmax * 1000, 4)
        TargetD_ms  = [math]::Round($targetDelta * 1000, 4)
        EffFps      = [math]::Round($effFps, 3)
        DupPTS      = $dups
        NegD        = $neg
        NonMonotonic= $nonMono
        Monotonic   = $(if ($nonMono -eq 0) { "YES" } else { "NO" })
        AvgRateTag  = $st.avg_frame_rate
        NbFramesTag = $st.nb_frames
        Anomaly     = ($(if ($anomalies.Count -eq 0) { "-" } else { $anomalies -join "; " }))
        Verdict     = $verdict
    })
}

$rows | Format-Table -AutoSize | Out-String -Width 260 | Write-Output
if ($Out -ne "") {
    $rows | Export-Csv -NoTypeInformation -Encoding UTF8 $Out
    Write-Output "CSV written: $Out"
}
