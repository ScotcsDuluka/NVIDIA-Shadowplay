# =============================================================================
# forensic-frame-identity.ps1  —  M2/W1 TEMPORARY FORENSIC ARTIFACT (READ-ONLY)
# =============================================================================
# Frame identity mapping + CFR tick classification (FRESH / REUSED / STALE /
# LATE) reconstructed from existing forensic-instrumentation logs.
#
#   - FORENSIC ONLY. Standalone analyzer: reads logs, changes nothing, is NOT
#     part of the production pipeline.
#   - Identity model (derived from production code):
#       srcSeq    = DdagrabBackend._nextSequence (process-lifetime, NOT reset
#                   per Start) -> FrameDiagnostics.Sequence
#       srcStamp  = CaptureTimeTicks == PresentationTimestampTicks (same value)
#                   = LastPresentTime-derived 100ns QPC (fallback/clamp-able)
#       attempt   = FrameAcquisitionResult.AttemptTimeTicks (pre-acquire)
#       tick      = CFR presentation slot (CaptureSession.presentationTickCount)
#       pktSeq    = NVENC packet counter — NOT srcSeq (identity break #1)
#       container = ffmpeg feed-order PTS at fixed -framerate (identity break
#                   #2: srcStamp never reaches the MP4)
#     Link rule: pkt -> src via PTS equality with a SELECTED FRAME's selectedTs;
#     equal PTS on consecutive packets = REUSED run.
#
#   Classification rules (per CFR tick, from CaptureSession.vb CFR loop):
#       FRESH  : source frame selected this tick AND selectedLag <= tickInterval
#       STALE  : source frame selected this tick AND selectedLag > tickInterval
#                (backlog / warm-up frame -> temporal jump)
#       REUSED : no eligible source (isDuplicate) -> lastFrame re-encoded;
#                tail-fill padding also counts as REUSED (FramesDuplicated)
#       LATE   : tick executed after its wall deadline (telemetry lateTicks>Xms)
#       DISCARD: extra frames consumed-then-disposed inside one tick's While
#                loop (same-tick multi-select) — identity destroyed (jump)
#
# Usage:
#   .\forensic-frame-identity.ps1 -LogPath <session.log>
#   .\forensic-frame-identity.ps1 -Sample          # built-in validation trace
# =============================================================================

param(
    [string]$LogPath = "",
    [switch]$Sample
)

$ErrorActionPreference = "Stop"

$Freq = 10000000L

$SampleTrace = @"
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
[2026-09-11 16:56:20.216] [INFO] Validate   packet.Metadata.PresentationTimestampTicks=903362325955
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
[2026-09-11 16:56:20.9] [INFO] Validate DdagrabBackend: display refresh rate = 75Hz
[2026-09-11 16:56:20.9] [INFO] Validate [session] capture diagnostics: emitted=1500, pushed=1500, dropped=0, replaced=0, noFrame=8000, errors=0, accessLost=0, textures=1500/1500
[2026-09-11 16:56:20.9] [INFO] Validate [session] CFR telemetry: ticks=180, selectedSources=140, seqSkips=25, maxSelectedLag=81.209ms, maxSourceGap=16.7ms, avgEncode=1.8ms, maxEncode=7.9ms, lateTicks>16.667ms=2, maxTickLate=18.2ms
[2026-09-11 16:56:20.9] [INFO] Validate [session] CFR tail-fill: +40 frames (target 180)
"@

if ($Sample) { $lines = $SampleTrace -split "`n" }
elseif ($LogPath -ne "" -and (Test-Path $LogPath)) { $lines = Get-Content $LogPath }
else { Write-Host "Usage: -LogPath <file>  or  -Sample"; exit 1 }

# keys INCLUDE the trailing '=' — Get-Val appends only the value group
function Get-Val([string]$line, [string]$key) {
    if ($line -match ([regex]::Escape($key) + "(-?[\d.]+)")) { return [double]$matches[1] }
    return $null
}

# detail keys per block kind (case-SENSITIVE matching so e.g. telemetry's
# "maxSelectedLag=" never matches the "selectedLag=" detail key)
$DetailKeys = @{
    tick   = @( @('targetQpc100ns=',"Target"), @('nextTick=',"NextTick"), @('cfrTick=',"CfrTick") )
    sel    = @( @('selectedSeq=',"Seq"), @('selectedTs=',"Ts"), @('selectedLag=',"Lag100ns"), @('selectedTick=',"SelectedTick") )
    feed   = @( @('muxFeedTick=',"FeedTick"), @('packet.Metadata.PresentationTimestampTicks=',"PTS"), @('packet.Metadata.Sequence=',"PktSeq") )
    cap    = @( @('acquireQpc100ns=',"Acquire100ns"), @('frameQpc100ns=',"Frame100ns"), @('captureTick=',"CaptureTick"), @('captureQpc=',"CaptureQpc") )
    deq    = @( @('dequeueTick=',"DequeueTick"), @('frame.Diagnostics.CaptureTimeTicks=',"CaptureTimeTicks") )
    encin  = @( @('inputTick=',"InputTick"), @('frame.Diagnostics.PresentationTimestampTicks=',"PTS"), @('frame.Diagnostics.CaptureTimeTicks=',"CaptureTimeTicks") )
    encout = @( @('encodeOutputTick=',"OutputTick"), @('packet.PresentationTimestampTicks=',"PTS"), @('packet.IsKeyFrame=',"IsKey") )
}

$ticks = @{}          # tickNo -> tick object
$probes = @()
$captureBlocks = @()
$dequeueBlocks = @()
$encIn = @(); $encOut = @()
$telemetry = $null; $capDiag = $null; $tailFill = $null
$displayHz = $null

$curObj = $null

function Get-Tick([long]$no) {
    if (-not $script:ticks.ContainsKey($no)) {
        $script:ticks[$no] = New-Object PSObject -Property @{
            Kind="tick"; Tick=$no; Target=$null; NextTick=$null; CfrTick=$null
            Selections=@(); Feeds=@()
        }
    }
    return $script:ticks[$no]
}

foreach ($raw in $lines) {
    $l = $raw

    if ($l -match "DdagrabBackend: display refresh rate = (\d+)Hz") { $displayHz = [int]$matches[1]; continue }

    if ($l -match "QPC probe seq=(\d+)") {
        $probes += New-Object PSObject -Property @{
            Seq=[long]$matches[1]; Present=(Get-Val $l "sourcePresent="); Acquire=(Get-Val $l "acquireQpc=")
            DeltaMs=(Get-Val $l "deltaMs="); AccFrames=(Get-Val $l "accumulatedFrames=") }
        $curObj = $null; continue
    }

    # ── headers ──
    if ($l -match "FRAME (\d+) CAPTURE TIMING") {
        $curObj = New-Object PSObject -Property @{ Kind="cap"; Frame=[long]$matches[1]; Acquire100ns=$null; Frame100ns=$null; CaptureTick=$null; CaptureQpc=$null }
        $captureBlocks += $curObj; continue
    }
    if ($l -match "FRAME (\d+) QUEUE DEQUEUE") {
        $curObj = New-Object PSObject -Property @{ Kind="deq"; Frame=[long]$matches[1]; DequeueTick=$null; CaptureTimeTicks=$null }
        $dequeueBlocks += $curObj; continue
    }
    if ($l -match "FRAME (\d+) ENCODER INPUT") {
        $curObj = New-Object PSObject -Property @{ Kind="encin"; Frame=[long]$matches[1]; InputTick=$null; PTS=$null; CaptureTimeTicks=$null }
        $encIn += $curObj; continue
    }
    if ($l -match "FRAME (\d+) ENCODER OUTPUT") {
        $curObj = New-Object PSObject -Property @{ Kind="encout"; Frame=[long]$matches[1]; OutputTick=$null; PTS=$null; IsKey=$null }
        $encOut += $curObj; continue
    }
    if ($l -match "CFR TICK (\d+) SELECTION DEBUG") {
        $curObj = Get-Tick ([long]$matches[1]); continue
    }
    if ($l -match "CFR TICK (\d+) SELECTED FRAME") {
        $t = Get-Tick ([long]$matches[1])
        $curObj = New-Object PSObject -Property @{ Kind="sel"; Seq=$null; Ts=$null; Lag100ns=$null; SelectedTick=$null }
        $t.Selections += $curObj; continue
    }
    if ($l -match "CFR TICK (\d+) MUXER FEED") {
        $t = Get-Tick ([long]$matches[1])
        $curObj = New-Object PSObject -Property @{ Kind="feed"; PktSeq=$null; PTS=$null; FeedTick=$null }
        $t.Feeds += $curObj; continue
    }

    # ── detail lines ──
    if ($curObj -ne $null) {
        $filled = $false
        foreach ($pair in $DetailKeys[$curObj.Kind]) {
            if ($l -cmatch [regex]::Escape($pair[0])) {
                $v = Get-Val $l $pair[0]
                if ($pair[1] -eq "IsKey") { $curObj.$($pair[1]) = ($l -cmatch "True") } else { $curObj.$($pair[1]) = $v }
                $filled = $true
                break
            }
        }
        if ($filled) { continue }
    }

    # ── session summary lines ──
    if ($l -match "CFR telemetry:") {
        $intervalMs = $null; $lateCount = $null
        if ($l -match "lateTicks>([\d.]+)ms=(\d+)") { $intervalMs = [double]$matches[1]; $lateCount = [double]$matches[2] }
        $telemetry = New-Object PSObject -Property @{
            Ticks=(Get-Val $l "ticks="); Selected=(Get-Val $l "selectedSources="); SeqSkips=(Get-Val $l "seqSkips=")
            MaxSelLagMs=(Get-Val $l "maxSelectedLag="); MaxSrcGapMs=(Get-Val $l "maxSourceGap=")
            AvgEncodeMs=(Get-Val $l "avgEncode="); MaxEncodeMs=(Get-Val $l "maxEncode=")
            IntervalMs=$intervalMs; LateCount=$lateCount
            MaxTickLateMs=(Get-Val $l "maxTickLate=") }
        $curObj = $null; continue
    }
    if ($l -match "capture diagnostics: emitted=") {
        $capDiag = New-Object PSObject -Property @{
            Emitted=(Get-Val $l "emitted="); Pushed=(Get-Val $l "pushed="); Dropped=(Get-Val $l "dropped=")
            Replaced=(Get-Val $l "replaced="); NoFrame=(Get-Val $l "noFrame="); Errors=(Get-Val $l "errors=")
            AccessLost=(Get-Val $l "accessLost=") }
        $curObj = $null; continue
    }
    if ($l -match "CFR tail-fill: \+(\d+) frames \(target (\d+)\)") {
        $tailFill = New-Object PSObject -Property @{ Padded=[long]$matches[1]; Target=[long]$matches[2] }
        $curObj = $null; continue
    }
}

# ── infer fps / interval ─────────────────────────────────────────────────────
$fps = 0; $interval100ns = 0
if ($telemetry -ne $null -and $telemetry.IntervalMs -gt 0) {
    $interval100ns = [long][math]::Round($telemetry.IntervalMs * 10000.0)
    $fps = [int][math]::Round(10000000.0 / $interval100ns)
}
$tickKeys = @($ticks.Keys | Sort-Object)
if ($interval100ns -eq 0 -and $tickKeys.Count -ge 2) {
    $t1 = $ticks[$tickKeys[0]]; $t2 = $ticks[$tickKeys[1]]
    if ($t1.NextTick -and $t2.NextTick -and ($t2.NextTick -gt $t1.NextTick)) {
        $interval100ns = [long]($t2.NextTick - $t1.NextTick)
        $fps = [int][math]::Round(10000000.0 / $interval100ns)
    }
}

# ── per-tick classification ──────────────────────────────────────────────────
$lastFreshPts = $null
$rows = @()
foreach ($tk in $tickKeys) {
    $t = $ticks[$tk]
    $nSel = @($t.Selections).Count; $nFeed = @($t.Feeds).Count
    $cls = "?"; $srcSeq = "?"; $pts = $null; $lagMs = $null
    $discarded = [math]::Max(0, $nSel - 1)

    if ($nSel -gt 0) {
        $last = $t.Selections[$nSel - 1]      # emitted identity = LAST selected
        $srcSeq = $last.Seq; $pts = $last.Ts
        if ($last.Lag100ns -ne $null) { $lagMs = $last.Lag100ns / 10000.0 }   # 'ns' label lies: unit is 100ns
        if ($interval100ns -gt 0 -and $last.Lag100ns -ne $null -and $last.Lag100ns -gt $interval100ns) { $cls = "STALE" } else { $cls = "FRESH" }
        if ($pts -ne $null) { $lastFreshPts = $pts }
    } elseif ($nFeed -gt 0) {
        $f0 = $t.Feeds[0]; $pts = $f0.PTS
        if ($lastFreshPts -ne $null -and $pts -eq $lastFreshPts) { $cls = "REUSED" }
        elseif ($lastFreshPts -eq $null) { $cls = "REUSED?" } else { $cls = "PTS-MISMATCH" }
        $srcSeq = "<= last fresh"
    } elseif ($t.CfrTick -ne $null) { $cls = "EMPTY(encode gap)" }

    $lateMs = $null
    $ref = $null
    if ($nSel -gt 0) { $ref = $t.Selections[$nSel - 1].SelectedTick } else { $ref = $t.CfrTick }
    if ($ref -ne $null -and $t.Target -ne $null) { $lateMs = ($ref - $t.Target) / 10000.0 }

    $pktSeqs = (@($t.Feeds) | ForEach-Object { $_.PktSeq }) -join ","
    $rows += New-Object PSObject -Property @{
        Tick=$tk; Class=$cls; SrcSeq=$srcSeq; PTS=$pts; LagMs=$lagMs; LateMs=$lateMs; Discards=$discarded; PktSeqs=$pktSeqs }
}

# same-tick multi-select → discarded identities + source gap
$gaps = @()
foreach ($tk in $tickKeys) {
    $sels = @($ticks[$tk].Selections)
    for ($i = 1; $i -lt $sels.Count; $i++) {
        if ($sels[$i].Ts -ne $null -and $sels[$i-1].Ts -ne $null) {
            $gaps += New-Object PSObject -Property @{ Tick=$tk; From=$sels[$i-1].Seq; To=$sels[$i].Seq; GapMs=(($sels[$i].Ts - $sels[$i-1].Ts)/10000.0) }
        }
    }
}

# ── aggregate reconstruction ─────────────────────────────────────────────────
$agg = $null
if ($telemetry -ne $null) {
    $reusedMid = $telemetry.Ticks - $telemetry.Selected
    $reusedPct = 0.0
    if ($telemetry.Ticks -gt 0) { $reusedPct = [math]::Round(100.0 * $reusedMid / $telemetry.Ticks, 1) }
    $padded = 0; if ($tailFill) { $padded = $tailFill.Padded }
    $evicted = $null; $alost = $null; $nof = $null
    if ($capDiag) { $evicted = $capDiag.Replaced; $alost = $capDiag.AccessLost; $nof = $capDiag.NoFrame }
    $agg = New-Object PSObject -Property @{
        Fps=$fps; IntervalMs=$telemetry.IntervalMs
        Ticks=$telemetry.Ticks; FreshOrStale=$telemetry.Selected
        ReusedMidLoop=$reusedMid; ReusedPct=$reusedPct; TailFillPadded=$padded
        SeqSkips=$telemetry.SeqSkips; MaxSelLagMs=$telemetry.MaxSelLagMs; MaxSrcGapMs=$telemetry.MaxSrcGapMs
        LateTicks=$telemetry.LateCount; MaxTickLateMs=$telemetry.MaxTickLateMs; MaxEncodeMs=$telemetry.MaxEncodeMs
        DropOldestEvicted=$evicted; AccessLost=$alost; NoFrame=$nof }
}

# ── output ───────────────────────────────────────────────────────────────────
Write-Host "================ M2/W1 FRAME IDENTITY MAP (forensic-only) ================"
if ($displayHz) { Write-Host ("display refresh          : {0} Hz" -f $displayHz) }
$ivMs = "?"; if ($interval100ns -gt 0) { $ivMs = [math]::Round($interval100ns / 10000.0, 3) }
Write-Host ("inferred CFR             : {0} fps (tick interval {1} ms)" -f $fps, $ivMs)
Write-Host ""

if ($probes.Count -gt 0) {
    Write-Host "-- source layer: backend QPC probe (frame age at acquire, NOT cadence) --"
    foreach ($p in $probes) { Write-Host ("  srcSeq={0}  present->acquire delta={1}ms  accumulatedFrames={2}" -f $p.Seq, $p.DeltaMs, $p.AccFrames) }
}
if ($captureBlocks.Count -gt 0) {
    Write-Host "-- capture layer (identity birth: srcSeq + srcStamp) --"
    foreach ($b in $captureBlocks) {
        $d = "?"; if ($b.CaptureQpc -ne $null -and $b.CaptureTick -ne $null) { $d = $b.CaptureQpc - $b.CaptureTick }
        Write-Host ("  srcSeq={0}  frameStamp100ns={1}  captureTick={2}  (captureQpc-captureTick={3} ticks)" -f $b.Frame, $b.Frame100ns, $b.CaptureTick, $d)
    }
}
if ($dequeueBlocks.Count -gt 0) {
    foreach ($b in $dequeueBlocks) { Write-Host ("  dequeue srcSeq={0}  CaptureTimeTicks={1}" -f $b.Frame, $b.CaptureTimeTicks) }
}
if ($encIn.Count -gt 0 -or $encOut.Count -gt 0) {
    Write-Host "-- encoder layer (identity break: pktSeq != srcSeq; link = PTS) --"
    foreach ($b in $encIn)  { Write-Host ("  NVENC in  frame(srcSeq)={0}  PTS={1}  CaptureTimeTicks={2}" -f $b.Frame, $b.PTS, $b.CaptureTimeTicks) }
    foreach ($b in $encOut) { Write-Host ("  NVENC out packet PTS={0} key={1}" -f $b.PTS, $b.IsKey) }
}
Write-Host ""
Write-Host "-- per-tick classification (head ticks) --"
$rows | Format-Table Tick, Class, SrcSeq, PTS, LagMs, LateMs, Discards, PktSeqs -AutoSize | Out-String -Width 160 | Write-Host
if ($gaps.Count -gt 0) {
    Write-Host "-- same-tick multi-select (discarded identities = content jump) --"
    $gaps | Format-Table Tick, From, To, GapMs -AutoSize | Out-String -Width 120 | Write-Host
    foreach ($g in $gaps) {
        if ($displayHz -and $g.GapMs -gt 0 -and $g.GapMs -lt (1000.0 / $displayHz) * 0.5) {
            Write-Host ("  !! tick {0}: sourceGap {1}ms < half refresh ({2}ms) -> fallback/cursor-stamp suspect (mixed domain, W1 finding)" -f $g.Tick, ([math]::Round($g.GapMs,3)), ([math]::Round((1000.0 / $displayHz) * 0.5, 2)))
        }
    }
}
if ($agg) {
    Write-Host "-- session aggregate (CFR telemetry + capture diagnostics) --"
    $agg | Format-List Fps, IntervalMs, Ticks, FreshOrStale, ReusedMidLoop, ReusedPct, TailFillPadded, SeqSkips, MaxSelLagMs, MaxSrcGapMs, LateTicks, MaxTickLateMs, MaxEncodeMs, DropOldestEvicted, AccessLost, NoFrame | Out-String -Width 120 | Write-Host
    Write-Host "-- verdict heuristics --"
    if ($displayHz -and $fps -gt 0) {
        $expectedHold = [math]::Ceiling($displayHz / [double]$fps)
        Write-Host ("  expected REUSED run @ steady motion: ~{0} ticks (refresh {1}Hz / {2}fps)" -f ($expectedHold - 1), $displayHz, $fps)
    }
    if ($agg.MaxSelLagMs -gt $agg.IntervalMs) { Write-Host ("  STALE present: maxSelectedLag {0}ms > interval {1}ms (backlog/warm-up frames selected)" -f $agg.MaxSelLagMs, $agg.IntervalMs) } else { Write-Host "  STALE: none beyond one interval" }
    if ($agg.LateTicks -gt 0) { Write-Host ("  LATE present: {0} ticks late (max {1}ms) -> burst catch-up" -f $agg.LateTicks, $agg.MaxTickLateMs) } else { Write-Host "  LATE: none" }
    if ($agg.SeqSkips -gt 0 -or $agg.DropOldestEvicted -gt 0) { Write-Host ("  JUMP sources: seqSkips={0}  DropOldest evictions(replaced)={1}  accessLost={2}" -f $agg.SeqSkips, $agg.DropOldestEvicted, $agg.AccessLost) }
    if ($agg.ReusedPct -ge 95) { Write-Host ("  FREEZE suspect: REUSED {0}% of ticks" -f $agg.ReusedPct) }
    if ($agg.MaxEncodeMs -gt $agg.IntervalMs) { Write-Host ("  encode budget VIOLATED: maxEncode {0}ms > interval {1}ms (late/catch-up driver)" -f $agg.MaxEncodeMs, $agg.IntervalMs) }
}
Write-Host "=========================================================================="
