# analyze-phase4b.ps1 - Phase 4B Timing Causality analyzer.
#
# Parses a phase4b-driver.log (from Phase4bDriver) into per-run stage metrics,
# groups by (t0Mode, arm), and writes the causality tables + verdicts.
#
# Usage: powershell -File analyze-phase4b.ps1 -Log <path-to-phase4b-driver.log> -Out <report.md>
#
# All timing math uses logged Stopwatch tick deltas (ticks == 100ns units on
# QPC-10MHz machines). Wall-clock log lines are ms-resolution only.
# A run's window extends from its RUN START line to the NEXT RUN START line
# (the PROBE line is logged after RUN END, so it belongs to the ending run).

param(
    [Parameter(Mandatory = $true)][string]$Log,
    [string]$Out = ""
)

$ErrorActionPreference = "Stop"
$lines = Get-Content -LiteralPath $Log

$freq = [long]10000000
foreach ($line in $lines) {
    if ($line -match 'stopwatch frequency=(\d+)') { $freq = [long]$Matches[1]; break }
}

function TicksToMs([long]$t) { return $t * 1000.0 / $freq }
function Fmt([object]$v) { if ($null -eq $v) { return "-" } else { return [math]::Round([double]$v, 1) } }

$runs = New-Object System.Collections.ArrayList
$current = $null
foreach ($line in $lines) {
    if ($line -match '\[phase4b\] RUN (\w+?)-(on10|on|off)-(\d+) START preRunTicks=(\d+).*?delayTicks=(\d+)') {
        if ($null -ne $current) { [void]$runs.Add($current) }
        $current = @{}
        $current.label = $Matches[1]
        $current.arm = $Matches[2]
        $current.runNo = [int]$Matches[3]
        $current.preRunTicks = [long]$Matches[4]
        $current.delayTicks = [long]$Matches[5]
        $current.armCallTicks = [long]0
        $current.t0Ticks = [long]0
        $current.rearmCallTicks = [long]0
        $current.rearmTicks = [long]0
        $current.firstCaptureTick = [long]0
        $current.firstDequeueTick = [long]0
        $current.feedPresentationTick = -1
        $current.feedMinusT0Ms = $null
        $current.contentMinusT0Ms = $null
        $current.lagP50 = $null
        $current.lagP95 = $null
        $current.packetPts0 = [long]0
        $current.framePts0 = [long]0
        $current.pass = ""
        $current.framesEncoded = -1
        $current.firstPts = ""
        $current.startTime = ""
        $current.mp4Duration = ""
        $current.packets = -1
        continue
    }
    if ($null -eq $current) { continue }

    if ($line -match '\[session\] timeline arm call: ticks=(\d+)') { $current.armCallTicks = [long]$Matches[1]; continue }
    if ($line -match '\[session\] common timeline armed: T0=(\d+)') { if ($current.t0Ticks -eq 0) { $current.t0Ticks = [long]$Matches[1] }; continue }
    if ($line -match '\[session\] RE-ARM call: ticks=(\d+)') { $current.rearmCallTicks = [long]$Matches[1]; continue }
    if ($line -match '\[session\] common timeline RE-ARMED at loop entry: T0=(\d+)') { $current.rearmTicks = [long]$Matches[1]; $current.t0Ticks = [long]$Matches[1]; continue }

    if ($line -match 'FRAME 0 CAPTURE TIMING:') { continue }
    if ($line -match 'captureTick=(\d+)') { if ($current.firstCaptureTick -eq 0) { $current.firstCaptureTick = [long]$Matches[1] }; continue }
    if ($line -match 'dequeueTick=(\d+)') { if ($current.firstDequeueTick -eq 0) { $current.firstDequeueTick = [long]$Matches[1] }; continue }

    if ($line -match 'FIRST ENCODE FEED: presentationTick=(\d+) feedMinusT0Ms=([-0-9.eE+]+) contentMinusT0Ms=([-0-9.eE+]+)') {
        $current.feedPresentationTick = [int]$Matches[1]
        $current.feedMinusT0Ms = [double]$Matches[2]
        $current.contentMinusT0Ms = [double]$Matches[3]
        # lag stats ride on the SAME line - parse them here too (no continue)
        if ($line -match 'lagP50=([-0-9.eE+]+)ms, lagP95=([-0-9.eE+]+)ms') {
            $current.lagP50 = [double]$Matches[1]
            $current.lagP95 = [double]$Matches[2]
        }
        continue
    }
    if ($line -match 'lagP50=([-0-9.eE+]+)ms, lagP95=([-0-9.eE+]+)ms') {
        $current.lagP50 = [double]$Matches[1]
        $current.lagP95 = [double]$Matches[2]
        continue
    }

    if ($line -match 'packet\.PresentationTimestampTicks=(\d+)') { if ($current.packetPts0 -eq 0) { $current.packetPts0 = [long]$Matches[1] }; continue }
    if ($line -match 'frame\.Diagnostics\.PresentationTimestampTicks=(\d+)') { if ($current.framePts0 -eq 0) { $current.framePts0 = [long]$Matches[1] }; continue }

    if ($line -match 'Result: pass=(\w+), frames=(\d+)') { $current.pass = $Matches[1]; $current.framesEncoded = [int]$Matches[2]; continue }
    if ($line -match 'PROBE \S+: first_video_pts=(\S+)s start_time=(\S+)s duration=(\S+)s video_packets=(\d+)') {
        $current.firstPts = $Matches[1]
        $current.startTime = $Matches[2]
        $current.mp4Duration = $Matches[3]
        $current.packets = [int]$Matches[4]
        continue
    }
    # Close the run when the driver disposes the block's backends (this line
    # is logged AFTER the last PROBE and BEFORE the next block's warm-up
    # session, whose identical "[session]" lines would otherwise pollute the
    # last run of each block).
    if ($line -match ': disposed') { [void]$runs.Add($current); $current = $null; continue }
    # NOTE: no close-on-END; the next RUN START (or backend dispose, or end
    # of file) closes the run.
}
if ($null -ne $current) { [void]$runs.Add($current) }

if ($runs.Count -eq 0) { Write-Host "NO RUNS FOUND in $Log"; exit 1 }

# per-run derived metrics
foreach ($r in $runs) {
    $r.delayMs = TicksToMs ([long]$r.delayTicks)
    if ($r.rearmCallTicks -ne 0 -and $r.rearmTicks -ne 0) {
        # loop mode: the delay is armed at the RE-ARM call
        $r.armDelayActualMs = TicksToMs ($r.rearmTicks - $r.rearmCallTicks)
    } elseif ($r.t0Ticks -gt 0 -and $r.armCallTicks -gt 0) {
        $r.armDelayActualMs = TicksToMs ($r.t0Ticks - $r.armCallTicks)
    } else {
        $r.armDelayActualMs = $null
    }
    $r.setupMs = TicksToMs ($r.armCallTicks - $r.preRunTicks)
    if ($r.rearmTicks -ne 0) { $r.t0MinusArmMs = TicksToMs ($r.rearmTicks - $r.armCallTicks) } else { $r.t0MinusArmMs = $r.delayMs }
    $r.feedMinusArmMs = $null
    if ($null -ne $r.feedMinusT0Ms) { $r.feedMinusArmMs = $r.t0MinusArmMs + $r.feedMinusT0Ms }
    $r.feedMinusPreRunMs = $null
    if ($null -ne $r.feedMinusArmMs) { $r.feedMinusPreRunMs = $r.setupMs + $r.feedMinusArmMs }
    $r.contentMinusArmMs = $null
    if ($null -ne $r.contentMinusT0Ms) { $r.contentMinusArmMs = $r.t0MinusArmMs + $r.contentMinusT0Ms }
    $r.ptsIdentityOk = ($r.packetPts0 -eq $r.framePts0 -and $r.framePts0 -ne 0)
}

function MedianOf([object[]]$v) {
    $a = @($v | Where-Object { $null -ne $_ } | ForEach-Object { [double]$_ })
    if ($a.Count -eq 0) { return $null }
    $s = $a | Sort-Object
    $n = $s.Count
    if (($n % 2) -eq 1) { return $s[[int](($n - 1) / 2)] }
    return ($s[[int]($n / 2) - 1] + $s[[int]($n / 2)]) / 2.0
}

$rep = New-Object System.Text.StringBuilder
[void]$rep.AppendLine("# Phase 4B - Timing Causality Analysis")
[void]$rep.AppendLine("")
[void]$rep.AppendLine("- source log: ``$Log``")
[void]$rep.AppendLine("- stopwatch frequency: $freq Hz (ticks == 100ns units when freq=10MHz)")
[void]$rep.AppendLine("- runs analyzed: $($runs.Count)")
[void]$rep.AppendLine("")

[void]$rep.AppendLine("## Per-run stage metrics (ms)")
[void]$rep.AppendLine("")
[void]$rep.AppendLine("| run | delay | armDelayActual | content@feed | feed(T0+) | 1stFeed(pre-) | feedTick | lagP50 | lagP95 | PTS identity | MP4 pts/start/pkts | pass |")
[void]$rep.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|")
foreach ($r in ($runs | Sort-Object { "$($_.label)-$($_.arm)-{0:d2}" -f $_.runNo })) {
    $row = "| " + $r.label + "-" + $r.arm + $r.runNo + " | " + (Fmt $r.delayMs) + " | " + (Fmt $r.armDelayActualMs) + " | " + (Fmt $r.contentMinusArmMs) + " | " + (Fmt $r.feedMinusT0Ms) + " | **" + (Fmt $r.feedMinusPreRunMs) + "** | " + $r.feedPresentationTick + " | " + (Fmt $r.lagP50) + " | " + (Fmt $r.lagP95) + " | " + $r.ptsIdentityOk + " | " + $r.firstPts + "/" + $r.startTime + "/" + $r.packets + " | " + $r.pass + " |"
    [void]$rep.AppendLine($row)
}
[void]$rep.AppendLine("")

[void]$rep.AppendLine("## Group medians (1stFeed = first encoded packet handed to the mux, from preRun)")
[void]$rep.AppendLine("")
[void]$rep.AppendLine("| group | n | 1stFeed median (ms) | content@feed median | lagP50 | lagP95 | MP4 packets |")
[void]$rep.AppendLine("|---|---|---|---|---|---|---|")
$groups = $runs | Group-Object { $_.label + "/" + $_.arm }
foreach ($g in $groups) {
    $mFeed = MedianOf ($g.Group | ForEach-Object { $_.feedMinusPreRunMs })
    $mContent = MedianOf ($g.Group | ForEach-Object { $_.contentMinusArmMs })
    $mL50 = MedianOf ($g.Group | ForEach-Object { $_.lagP50 })
    $mL95 = MedianOf ($g.Group | ForEach-Object { $_.lagP95 })
    $pk = ($g.Group | ForEach-Object { $_.packets } | Sort-Object -Unique) -join "/"
    [void]$rep.AppendLine("| " + $g.Name + " | " + $g.Count + " | " + (Fmt $mFeed) + " | " + (Fmt $mContent) + " | " + (Fmt $mL50) + " | " + (Fmt $mL95) + " | " + $pk + " |")
}
[void]$rep.AppendLine("")

[void]$rep.AppendLine("## Delta first-feed between arms (same t0-mode, medians)")
[void]$rep.AppendLine("")
foreach ($mode in (($runs | ForEach-Object { $_.label } | Sort-Object -Unique))) {
    $m = @($runs | Where-Object { $_.label -eq $mode })
    $mon = MedianOf (@($m | Where-Object { $_.arm -eq 'on' }) | ForEach-Object { $_.feedMinusPreRunMs })
    $m10 = MedianOf (@($m | Where-Object { $_.arm -eq 'on10' }) | ForEach-Object { $_.feedMinusPreRunMs })
    $moff = MedianOf (@($m | Where-Object { $_.arm -eq 'off' }) | ForEach-Object { $_.feedMinusPreRunMs })
    if ($null -ne $mon -and $null -ne $moff) {
        [void]$rep.AppendLine("- delta(on-off) [" + $mode + "]: " + [math]::Round($mon - $moff, 1) + " ms  (on=" + (Fmt $mon) + ", off=" + (Fmt $moff) + ")")
    }
    if ($null -ne $m10 -and $null -ne $moff) {
        [void]$rep.AppendLine("- delta(on10-off) [" + $mode + "]: " + [math]::Round($m10 - $moff, 1) + " ms  (on10=" + (Fmt $m10) + ", off=" + (Fmt $moff) + ")")
    }
}
[void]$rep.AppendLine("")

[void]$rep.AppendLine("## Verdicts")
[void]$rep.AppendLine("")
$v1 = $true
foreach ($r in $runs) {
    if ($null -eq $r.armDelayActualMs -or ([math]::Abs($r.armDelayActualMs - $r.delayMs) -gt 2.0)) { $v1 = $false }
}
[void]$rep.AppendLine("- **V1 timeline arm delay exact** (T0-armCall == delay within 2ms in every run): " + $(if ($v1) { "PASS" } else { "FAIL" }))
$v2 = $true
foreach ($r in $runs) { if (-not $r.ptsIdentityOk) { $v2 = $false } }
[void]$rep.AppendLine("- **V2 Packet PTS identity** (packet.PTS == frame.PTS == capture time, every run): " + $(if ($v2) { "PASS" } else { "FAIL" }))
$v3 = $true
foreach ($r in $runs) { if ($r.firstPts -ne "0.000000" -or $r.startTime -ne "0.000000") { $v3 = $false } }
[void]$rep.AppendLine("- **V3 MP4 PTS** (first video PTS = 0 and start_time = 0 in every run): " + $(if ($v3) { "PASS" } else { "FAIL" }) + " - container PTS is frame-index-based; the timeline delay is invisible in the MP4 by construction")
$v4 = $true
foreach ($r in $runs) { if ($null -ne $r.lagP50 -and $r.lagP50 -gt 25.0) { $v4 = $false } }
[void]$rep.AppendLine("- **V4 CFR grid alignment** (median selection lag <= 25ms = 1 frame at 60fps): " + $(if ($v4) { "PASS" } else { "FAIL" }))
$v5 = $true
foreach ($r in $runs) { if ($r.pass -ne "True") { $v5 = $false } }
[void]$rep.AppendLine("- **V5 session pass** (mux ok, dropped=0, file valid, every run): " + $(if ($v5) { "PASS" } else { "FAIL" }))
[void]$rep.AppendLine("")

if ($Out -ne "") {
    $rep.ToString() | Set-Content -LiteralPath $Out -Encoding UTF8
    Write-Host "report written: $Out"
}
Write-Host $rep.ToString()
