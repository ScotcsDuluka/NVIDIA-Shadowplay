<#
═══════════════════════════════════════════════════════════════════════
  ShadowPlay Engine — NVENC Comprehensive Test Script
  Tests EVERY combination of NVIDIA encoder settings
  Output: CSV report + per-test log files
═══════════════════════════════════════════════════════════════════════

USAGE:
  .\test_nvenc.ps1
  .\test_nvenc.ps1 -FFmpegPath "C:\API-Core\ffmpeg.exe"
  .\test_nvenc.ps1 -Duration 5    (shorter tests, 5 sec each)
  .\test_nvenc.ps1 -Quick         (minimal test set)

OUTPUT:
  Results saved to: NVENC_TestResults.csv
  Individual logs:  Logs\test_*.log
#>

param(
    [string]$FFmpegPath = "",
    [int]$Duration = 8,
    [switch]$Quick
)

# ── Auto-find FFmpeg ──
if ($FFmpegPath -eq "") {
    $candidates = @(
        ".\API-Core\ffmpeg.exe",
        "..\API-Core\ffmpeg.exe",
        "C:\API-Core\ffmpeg.exe",
        "$env:ProgramFiles\ffmpeg\bin\ffmpeg.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { $FFmpegPath = $c; break }
    }
    if ($FFmpegPath -eq "") {
        Write-Host "[FATAL] ffmpeg.exe not found. Use -FFmpegPath parameter." -ForegroundColor Red
        exit 1
    }
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ShadowPlay NVENC Comprehensive Test" -ForegroundColor Cyan
Write-Host "  FFmpeg: $FFmpegPath" -ForegroundColor Cyan
Write-Host "  Duration per test: ${Duration}s" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Verify FFmpeg + NVENC
Write-Host "`n[CHECK] Verifying FFmpeg and NVENC support..." -ForegroundColor Yellow
$check = & $FFmpegPath -hide_banner -encoders 2>&1 | Select-String -Pattern "nvenc"
if (-not $check) {
    Write-Host "[FATAL] No NVENC encoders found! This machine may not have NVIDIA GPU or driver." -ForegroundColor Red
    exit 1
}
$nvencCount = ($check | Measure-Object).Count
Write-Host "[OK] Found $nvencCount NVENC encoder(s):" -ForegroundColor Green
$check | ForEach-Object { Write-Host "      $_" -ForegroundColor DarkGray }

# ── Directories ──
$testDir = "NVENC_Tests"
$logDir = "NVENC_Tests\Logs"
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# ── Results CSV ──
$csvPath = "$testDir\NVENC_TestResults.csv"
"TestID,Encoder,CaptureMethod,FPS,TargetBitrate,RateControl,Preset,Tune,Zerolatency,SpatialAQ,TemporalAQ,Lookahead,PixFmt,ExitCode,FileSizeKB,DurationSec,ActualBitrateKbps,ActualFPS,AvgSpeed,Status,Notes" | Out-File -FilePath $csvPath -Encoding UTF8

# ── Test Matrix ──
$testID = 0
$results = [System.Collections.ArrayList]::new()

function Run-Test {
    param(
        [string]$Encoder,
        [string]$CaptureMethod,
        [int]$FPS,
        [long]$TargetBitrate,
        [string]$RateControl,
        [string]$Preset,
        [string]$Tune,
        [int]$Zerolatency,
        [int]$SpatialAQ,
        [int]$TemporalAQ,
        [int]$Lookahead,
        [string]$PixFmt,
        [string]$ExtraArgs,
        [string]$Notes
    )

    $script:testID++
    $id = $script:testID
    $label = "NVENC-$($id.ToString('000'))"
    $outFile = "$testDir\${label}.mp4"
    $logFile = "$logDir\${label}.log"

    Write-Host ""
    Write-Host "━━━ [$label] $Encoder | $CaptureMethod | ${FPS}fps | $RateControl | $Preset | $Tune ━━━" -ForegroundColor DarkCyan

    # Build FFmpeg command
    $args = "-hide_banner -loglevel info"

    # Video input
    switch ($CaptureMethod) {
        "ddagrab"   { $args += " -f lavfi -i ""ddagrab=output_idx=0:framerate=$FPS""" }
        "gdigrab"   { $args += " -f gdigrab -framerate $FPS -i desktop" }
        "gfxcapture"{ $args += " -f lavfi -i ""gfxcapture=monitor_idx=0:max_framerate=$FPS""" }
    }

    # Video filter
    $vf = ""
    if ($CaptureMethod -eq "gfxcapture") {
        $vf = "fps=$FPS"
    }

    # For NVENC + ddagrab/gfxcapture: NO -pix_fmt (d3d11 direct)
    # For NVENC + gdigrab: can use -pix_fmt nv12 (software frames)
    if ($CaptureMethod -ne "gdigrab") {
        $usePixFmt = ""
    } else {
        $usePixFmt = " -pix_fmt $PixFmt"
    }

    if ($vf -ne "") {
        $args += " -vf ""$vf"""
    }

    # Encoder
    $args += " -c:v $Encoder"
    $args += " -preset $Preset"
    if ($Tune -ne "") { $args += " -tune $Tune" }

    # Rate control
    $bufSize = $TargetBitrate * 2
    switch ($RateControl) {
        "cbr"      { $args += " -b:v $TargetBitrate -rc cbr -bufsize $bufSize -maxrate $TargetBitrate" }
        "vbr"      { $args += " -b:v $TargetBitrate -rc vbr -bufsize $bufSize -maxrate $($TargetBitrate * 1.5)" }
        "constqp"  { $args += " -qp 20 -rc constqp" }
        "vbr_hq"   { $args += " -b:v $TargetBitrate -rc vbr -bufsize $bufSize -qmin 18 -qmax 25" }
    }

    # NVENC options
    $args += " -zerolatency $Zerolatency"
    $args += " -spatial-aq $SpatialAQ"
    $args += " -temporal-aq $TemporalAQ"
    if ($Lookahead -gt 0) { $args += " -rc-lookahead $Lookahead" }
    if ($ExtraArgs -ne "") { $args += " $ExtraArgs" }

    $args += $usePixFmt
    $args += " -t $Duration"
    $args += " -y ""$outFile"""

    # Run
    $cmd = "$FFmpegPath $args"
    Write-Host "  CMD: $cmd" -ForegroundColor DarkGray

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $proc = Start-Process -FilePath $FFmpegPath -ArgumentList $args `
        -NoNewWindow -PassThru -RedirectStandardError $logFile -Wait
    $sw.Stop()

    $exitCode = $proc.ExitCode
    $fileSize = 0
    if (Test-Path $outFile) { $fileSize = (Get-Item $outFile).Length / 1KB }

    # Parse log for actual bitrate and fps
    $logContent = Get-Content $logFile -Raw -ErrorAction SilentlyContinue
    $actualBitrate = 0
    $actualFPS = 0
    $speed = ""
    $frameCount = 0

    if ($logContent) {
        # Last line with "frame= ... Lsize="
        $lastLine = ($logContent -split "`n" | Where-Object { $_ -match "Lsize" }) | Select-Object -Last 1
        if ($lastLine -match "bitrate=([\d.]+)kbits/s") { $actualBitrate = [double]$Matches[1] }
        if ($lastLine -match "fps=(\d+)") { $actualFPS = [int]$Matches[1] }
        if ($lastLine -match "speed=([\d.]+x)") { $speed = $Matches[1] }
        if ($lastLine -match "frame=\s*(\d+)") { $frameCount = [int]$Matches[1] }
    }

    # Status
    $status = "PASS"
    $notesOut = $Notes
    if ($exitCode -ne 0) {
        $status = "FAIL"
        $notesOut = "ExitCode=$exitCode; $Notes"
        Write-Host "  ❌ FAIL (exit code $exitCode)" -ForegroundColor Red
    } elseif ($fileSize -eq 0) {
        $status = "EMPTY"
        $notesOut = "No output file; $Notes"
        Write-Host "  ⚠️  EMPTY (no output)" -ForegroundColor Yellow
    } else {
        Write-Host "  ✅ OK | ${frameCount} frames | $([math]::Round($actualBitrate))kbps avg | $actualFPS fps | speed=$speed" -ForegroundColor Green
    }

    # CSV row
    $row = "$label,$Encoder,$CaptureMethod,$FPS,$TargetBitrate,$RateControl,$Preset,$Tune,$Zerolatency,$SpatialAQ,$TemporalAQ,$Lookahead,$PixFmt,$exitCode,$([math]::Round($fileSize)),$($sw.Elapsed.TotalSeconds.ToString('F1')),$([math]::Round($actualBitrate)),$actualFPS,$speed,$status,""$notesOut"""
    Add-Content -Path $csvPath -Value $row -Encoding UTF8

    return @{ Status=$status; Bitrate=$actualBitrate; FPS=$actualFPS; FileSize=$fileSize }
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 1: h264_nvenc — Capture Method Comparison
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 1: h264_nvenc — Capture Method Comparison           ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$captures = @("ddagrab", "gfxcapture", "gdigrab")
foreach ($cap in $captures) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod $cap -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group1-CaptureCompare"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 2: h264_nvenc — Rate Control Modes
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 2: h264_nvenc — Rate Control Comparison              ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$rcModes = @("cbr", "vbr", "vbr_hq", "constqp")
foreach ($rc in $rcModes) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl $rc -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group2-RateControl"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 3: h264_nvenc — Preset Comparison (p1-p7)
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 3: h264_nvenc — Preset Comparison (p1 to p7)       ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$presets = @("p1", "p2", "p3", "p4", "p5", "p6", "p7")
foreach ($p in $presets) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset $p -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group3-PresetCompare"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 4: h264_nvenc — Tune Comparison
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 4: h264_nvenc — Tune Comparison                      ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$tunes = @("ll", "ull", "hq", "lossless")
foreach ($t in $tunes) {
    $tuneLabel = if ($t -eq "lossless") { "lossless" } else { $t }
    $extraArgs = if ($t -eq "lossless") { "-cq 0" } else { "" }
    $br = if ($t -eq "lossless") { 0 } else { 50000000 }
    $rc = if ($t -eq "lossless") { "constqp" } else { "cbr" }
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate $br `
        -RateControl $rc -Preset "p4" -Tune $tuneLabel -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -ExtraArgs $extraArgs -Notes "Group4-TuneCompare"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 5: h264_nvenc — AQ Options Toggle
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 5: h264_nvenc — AQ Options Matrix                   ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$aqCombos = @(
    @{ spatial=1; temporal=1 },
    @{ spatial=1; temporal=0 },
    @{ spatial=0; temporal=1 },
    @{ spatial=0; temporal=0 }
)
foreach ($aq in $aqCombos) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ $aq.spatial -TemporalAQ $aq.temporal -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group5-AQ_s$($aq.spatial)_t$($aq.temporal)"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 6: h264_nvenc — Zerolatency ON vs OFF
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 6: h264_nvenc — Zerolatency ON vs OFF                ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

0..1 | ForEach-Object {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency $_ -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group6-Zerolatency=$_"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 7: h264_nvenc — Lookahead Depth
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 7: h264_nvenc — Lookahead Depth                      ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$lookaheads = @(0, 8, 16, 32)
foreach ($la in $lookaheads) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "hq" -Zerolatency 0 -SpatialAQ 1 -TemporalAQ 1 -Lookahead $la `
        -PixFmt "nv12" -Notes "Group7-Lookahead=$la"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 8: h264_nvenc — Bitrate Tiers
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 8: h264_nvenc — Bitrate Tiers                        ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$bitrates = @(10000000, 25000000, 50000000, 80000000, 150000000)
$brLabels = @("10Mbps", "25Mbps", "50Mbps", "80Mbps", "150Mbps")
for ($i = 0; $i -lt $bitrates.Count; $i++) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate $bitrates[$i] `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group8-Bitrate_$($brLabels[$i])"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 9: h264_nvenc — FPS Comparison
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 9: h264_nvenc — FPS Comparison                      ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$fpsValues = @(30, 60, 120, 144)
foreach ($f in $fpsValues) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS $f -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group9-FPS=$f"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 10: h264_nvenc — Pixel Format (gdigrab only)
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 10: h264_nvenc — Pixel Format (gdigrab)              ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$pixFmts = @("nv12", "yuv420p", "yuv444p", "yuv420p10le")
foreach ($pf in $pixFmts) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "gdigrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt $pf -Notes "Group10-PixFmt_$pf"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 11: h264_nvenc — Psycho AQ & B-adapt
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 11: h264_nvenc — Psycho AQ & B-adapt                 ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

# Psycho AQ ON
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
    -RateControl "cbr" -Preset "p4" -Tune "hq" -Zerolatency 0 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 16 `
    -PixFmt "nv12" -ExtraArgs "-psycho-aq 1 -b_adapt 1" -Notes "Group11-PsychoAQ_Badapt_ON"

# Psycho AQ OFF
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
    -RateControl "cbr" -Preset "p4" -Tune "hq" -Zerolatency 0 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 16 `
    -PixFmt "nv12" -ExtraArgs "-psycho-aq 0 -b_adapt 0" -Notes "Group11-PsychoAQ_Badapt_OFF"

# ══════════════════════════════════════════════════════════════
# TEST GROUP 12: h264_nvenc — Non-monotonic DTS fix (vsync)
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 12: h264_nvenc — VSYNC modes (DTS fix)               ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$vsyncModes = @("auto", "cfr", "vfr", "passthrough")
foreach ($vs in $vsyncModes) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -ExtraArgs "-vsync $vs" -Notes "Group12-VSync_$vs"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 13: h264_nvenc — Custom Resolution (scale)
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 13: h264_nvenc — Custom Resolution (scale)           ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$scales = @("1920x1080", "1280x720", "854x480")
foreach ($s in $scales) {
    $w, $h = $s -split "x"
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -ExtraArgs "-vf scale=$w`:$h" -Notes "Group13-Scale_$s"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 14: hevc_nvenc — Same Core Tests
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 14: hevc_nvenc — Capture Method Comparison            ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

foreach ($cap in $captures) {
    Run-Test -Encoder "hevc_nvenc" -CaptureMethod $cap -FPS 60 -TargetBitrate 35000000 `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group14-HEVC_CaptureCompare"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 15: hevc_nvenc — Preset Comparison
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 15: hevc_nvenc — Preset Comparison                   ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

foreach ($p in $presets) {
    Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 35000000 `
        -RateControl "cbr" -Preset $p -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group15-HEVC_PresetCompare"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 16: hevc_nvenc — Rate Control
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 16: hevc_nvenc — Rate Control                        ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

foreach ($rc in $rcModes) {
    Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 35000000 `
        -RateControl $rc -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group16-HEVC_RateControl"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 17: hevc_nvenc — 10-bit (yuv420p10le)
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 17: hevc_nvenc — 10-bit vs 8-bit                      ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Run-Test -Encoder "hevc_nvenc" -CaptureMethod "gdigrab" -FPS 60 -TargetBitrate 35000000 `
    -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
    -PixFmt "nv12" -Notes "Group17-HEVC_8bit_nv12"

Run-Test -Encoder "hevc_nvenc" -CaptureMethod "gdigrab" -FPS 60 -TargetBitrate 35000000 `
    -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
    -PixFmt "yuv420p10le" -Notes "Group17-HEVC_10bit_p10le"

# ══════════════════════════════════════════════════════════════
# TEST GROUP 18: hevc_nvenc — Bitrate Tiers
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 18: hevc_nvenc — Bitrate Tiers                        ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

for ($i = 0; $i -lt $bitrates.Count; $i++) {
    Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate $bitrates[$i] `
        -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
        -PixFmt "nv12" -Notes "Group18-HEVC_Bitrate_$($brLabels[$i])"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 19: hevc_nvenc — Lookahead + Zerolatency OFF
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 19: hevc_nvenc — HQ mode (zerolatency OFF)           ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

foreach ($la in $lookaheads) {
    Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 35000000 `
        -RateControl "cbr" -Preset "p4" -Tune "hq" -Zerolatency 0 -SpatialAQ 1 -TemporalAQ 1 -Lookahead $la `
        -PixFmt "nv12" -Notes "Group19-HEVC_HQ_Lookahead=$la"
}

# ══════════════════════════════════════════════════════════════
# TEST GROUP 20: av1_nvenc — If Available
# ══════════════════════════════════════════════════════════════
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  GROUP 20: av1_nvenc — AV1 (if GPU supports)                 ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

# Check if av1_nvenc exists
$hasAV1 = $false
$allEncoders = & $FFmpegPath -hide_banner -encoders 2>&1 | Select-String -Pattern "av1_nvenc"
if ($allEncoders) {
    $hasAV1 = $true
    Write-Host "  [INFO] av1_nvenc detected, testing..." -ForegroundColor Green
}

if ($hasAV1) {
    foreach ($cap in @("ddagrab", "gfxcapture")) {
        Run-Test -Encoder "av1_nvenc" -CaptureMethod $cap -FPS 60 -TargetBitrate 50000000 `
            -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
            -PixFmt "nv12" -Notes "Group20-AV1_CaptureCompare"
    }
    foreach ($p in @("p4", "p6", "p7")) {
        Run-Test -Encoder "av1_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
            -RateControl "cbr" -Preset $p -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
            -PixFmt "nv12" -Notes "Group20-AV1_Preset_$p"
    }
    foreach ($rc in @("cbr", "vbr", "constqp")) {
        Run-Test -Encoder "av1_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 `
            -RateControl $rc -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 0 `
            -PixFmt "nv12" -Notes "Group20-AV1_RC_$rc"
    }
} else {
    Write-Host "  [SKIP] av1_nvenc not available on this GPU" -ForegroundColor DarkYellow
    "NVENC-020-av1_skip,av1_nvenc,skipped,0,0,skipped,skipped,skipped,0,0,0,0,skipped,0,0,0,0,0,,SKIP,av1_nvenc not available" | Out-File -FilePath $csvPath -Append -Encoding UTF8
}

# ══════════════════════════════════════════════════════════════
# FINAL SUMMARY
# ══════════════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  TEST COMPLETE — $testID tests executed" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Count results
$csvData = Import-Csv $csvPath
$passCount = ($csvData | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($csvData | Where-Object { $_.Status -eq "FAIL" }).Count
$emptyCount = ($csvData | Where-Object { $_.Status -eq "EMPTY" }).Count

Write-Host ""
Write-Host "  Results Summary:" -ForegroundColor White
Write-Host "    ✅ PASS:  $passCount" -ForegroundColor Green
Write-Host "    ❌ FAIL:  $failCount" -ForegroundColor Red
Write-Host "    ⚠️  EMPTY: $emptyCount" -ForegroundColor Yellow
Write-Host ""
Write-Host "  CSV Report: $csvPath" -ForegroundColor White
Write-Host "  Video files: $testDir\" -ForegroundColor White
Write-Host "  Per-test logs: $logDir\" -ForegroundColor White
Write-Host ""
Write-Host "  [TIP] Open $csvPath in Excel to analyze results" -ForegroundColor DarkGray
Write-Host "  [TIP] Sort by ActualBitrateKbps to find closest to target" -ForegroundColor DarkGray
Write-Host "  [TIP] Check Speed column — higher = faster encoding" -ForegroundColor DarkGray
