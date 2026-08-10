<#
═══════════════════════════════════════════════════════════════════════
  ShadowPlay Engine — NVENC Test Script V2 (FIXED)
  Fixes from V1:
    - hevc_nvenc: temporal-aq forced OFF (not supported on many GPUs)
    - av1_nvenc: temporal-aq forced OFF
    - scale+ddagrab: hwdownload,format=nv12 before scale
    - gdigrab: tested at 30fps (more realistic)
    - QSV options: removed -rc cbr and -look_ahead
    - added -movflags +faststart for mp4
═══════════════════════════════════════════════════════════════════════

USAGE:
  .\test_nvenc_v2.ps1 -FFmpegPath "C:\API-Core\ffmpeg.exe"
  .\test_nvenc_v2.ps1 -FFmpegPath "C:\API-Core\ffmpeg.exe" -Duration 5
#>

param(
    [string]$FFmpegPath = "",
    [int]$Duration = 8
)

if ($FFmpegPath -eq "") {
    $candidates = @(".\API-Core\ffmpeg.exe", "..\API-Core\ffmpeg.exe", "C:\API-Core\ffmpeg.exe")
    foreach ($c in $candidates) { if (Test-Path $c) { $FFmpegPath = $c; break } }
    if ($FFmpegPath -eq "") { Write-Host "[FATAL] ffmpeg.exe not found" -ForegroundColor Red; exit 1 }
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ShadowPlay NVENC Test V2 (FIXED)" -ForegroundColor Cyan
Write-Host "  FFmpeg: $FFmpegPath | Duration: ${Duration}s" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

$testDir = "NVENC_Tests_V2"
$logDir = "NVENC_Tests_V2\Logs"
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$csvPath = "$testDir\NVENC_V2_Results.csv"
"TestID,Encoder,CaptureMethod,FPS,TargetBitrate,RateControl,Preset,Tune,Zerolatency,TemporalAQ,Lookahead,ExitCode,FileSizeKB,ActualBitrateKbps,Speed,Status,Notes" | Out-File -FilePath $csvPath -Encoding UTF8

$testID = 0

function Run-Test {
    param([string]$Encoder, [string]$CaptureMethod, [int]$FPS, [long]$TargetBitrate,
          [string]$RateControl, [string]$Preset, [string]$Tune, [int]$Zerolatency,
          [int]$SpatialAQ, [int]$TemporalAQ, [int]$Lookahead, [string]$ExtraArgs, [string]$Notes)

    $script:testID++
    $id = "V2-" + $script:testID.ToString("000")
    $outFile = "$testDir\${id}.mp4"
    $logFile = "$logDir\${id}.log"

    Write-Host "`n━━━ [$id] $Encoder | $CaptureMethod | ${FPS}fps | $Preset | $Tune | temporal-aq=$TemporalAQ ━━━" -ForegroundColor DarkCyan

    $args = "-hide_banner -loglevel info"
    $buf = $TargetBitrate * 2
    $isHwCap = ($CaptureMethod -eq "ddagrab" -or $CaptureMethod -eq "gfxcapture")

    # Video input
    switch ($CaptureMethod) {
        "ddagrab"    { $args += " -f lavfi -i ""ddagrab=output_idx=0:framerate=$FPS""" }
        "gdigrab"    { $args += " -f gdigrab -framerate $FPS -i desktop" }
        "gfxcapture" { $args += " -f lavfi -i ""gfxcapture=monitor_idx=0:max_framerate=$FPS""" }
    }

    # Video filter
    $vf = ""
    if ($CaptureMethod -eq "gfxcapture") { $vf = "fps=$FPS" }

    $args += " -c:v $Encoder -preset $Preset"

    if ($Tune -ne "") { $args += " -tune $Tune" }

    # Rate control
    if ($RateControl -eq "cbr") {
        $args += " -b:v $TargetBitrate -rc cbr -bufsize $buf -maxrate $TargetBitrate"
    } elseif ($RateControl -eq "constqp") {
        $args += " -qp 20 -rc constqp"
    } elseif ($RateControl -eq "vbr") {
        $args += " -b:v $TargetBitrate -rc vbr -bufsize $buf"
    }

    $args += " -zerolatency $Zerolatency -spatial-aq $SpatialAQ -temporal-aq $TemporalAQ"
    if ($Lookahead -gt 0) { $args += " -rc-lookahead $Lookahead" }

    # NO -pix_fmt for hw capture + hw encoder (d3d11 direct)
    # For gdigrab + NVENC: can add -pix_fmt nv12
    if (-not $isHwCap -and $TargetBitrate -gt 0) { $args += " -pix_fmt nv12" }

    if ($ExtraArgs -ne "") { $args += " $ExtraArgs" }

    $args += " -t $Duration -movflags +faststart"
    $args += " -y ""$outFile"""

    $cmd = "$FFmpegPath $args"
    Write-Host "  CMD: $cmd" -ForegroundColor DarkGray

    $proc = Start-Process -FilePath $FFmpegPath -ArgumentList $args -NoNewWindow -PassThru -RedirectStandardError $logFile -Wait
    $exitCode = $proc.ExitCode
    $fileSize = 0
    if (Test-Path $outFile) { $fileSize = (Get-Item $outFile).Length / 1KB }

    $logContent = Get-Content $logFile -Raw -ErrorAction SilentlyContinue
    $actualBR = 0; $speed = ""
    if ($logContent) {
        $last = ($logContent -split "`n" | Where-Object { $_ -match "Lsize" }) | Select-Object -Last 1
        if ($last -match "bitrate=([\d.]+)kbits/s") { $actualBR = [double]$Matches[1] }
        if ($last -match "speed=([\d.]+x)") { $speed = $Matches[1] }
    }

    $status = "PASS"
    $notesOut = $Notes
    if ($exitCode -ne 0) { $status = "FAIL"; $notesOut = "Exit=$exitCode; $Notes"; Write-Host "  ❌ FAIL" -ForegroundColor Red }
    elseif ($fileSize -eq 0) { $status = "EMPTY"; $notesOut = "Empty; $Notes"; Write-Host "  ⚠️  EMPTY" -ForegroundColor Yellow }
    else { Write-Host "  ✅ OK | $([math]::Round($actualBR))kbps | speed=$speed" -ForegroundColor Green }

    $row = "$id,$Encoder,$CaptureMethod,$FPS,$TargetBitrate,$RateControl,$Preset,$Tune,$Zerolatency,$TemporalAQ,$Lookahead,$exitCode,$([math]::Round($fileSize)),$([math]::Round($actualBR)),$speed,$status,""$notesOut"""
    Add-Content -Path $csvPath -Value $row -Encoding UTF8
}

# ════════════════════════════════════════════════════
# GROUP 1: h264_nvenc — Best Settings Confirmation
# ════════════════════════════════════════════════════
Write-Host "`n╔══ h264_nvenc — Best Settings Confirmation ══╗" -ForegroundColor Magenta

Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Notes "BEST-h264_ddagrab_60"
Run-Test -Encoder "h264_nvenc" -CaptureMethod "gfxcapture" -FPS 60 -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Notes "BEST-h264_gfxcapture_60"
Run-Test -Encoder "h264_nvenc" -CaptureMethod "gdigrab" -FPS 30 -TargetBitrate 35000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Notes "BEST-h264_gdigrab_30"

# ════════════════════════════════════════════════════
# GROUP 2: h264_nvenc — Quality Presets (matching UI)
# ════════════════════════════════════════════════════
Write-Host "`n╔══ h264_nvenc — Quality Presets ══╗" -ForegroundColor Magenta

# Low: 30fps, 10Mbps, p7
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 30 -TargetBitrate 10000000 -RateControl "cbr" -Preset "p7" -Tune "ll" -Zerolatency 1 -SpatialAQ 0 -TemporalAQ 0 -Notes "Quality=Low"
# Medium: 60fps, 35Mbps, p5
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 35000000 -RateControl "cbr" -Preset "p5" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Notes "Quality=Medium"
# High: 60fps, 50Mbps, p4
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Notes "Quality=High"
# Recommended: 60fps, 70Mbps, p4, lookahead 16
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 70000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 16 -Notes "Quality=Recommended"
# Maximum: 60fps, 150Mbps, p1, hq, lookahead 32
Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 150000000 -RateControl "cbr" -Preset "p1" -Tune "hq" -Zerolatency 0 -SpatialAQ 1 -TemporalAQ 1 -Lookahead 32 -Notes "Quality=Maximum"

# ════════════════════════════════════════════════════
# GROUP 3: hevc_nvenc — FIXED (temporal-aq=0)
# ════════════════════════════════════════════════════
Write-Host "`n╔══ hevc_nvenc — FIXED (temporal-aq=0) ══╗" -ForegroundColor Magenta

Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 35000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Notes "FIXED-hevc_ddagrab"
Run-Test -Encoder "hevc_nvenc" -CaptureMethod "gfxcapture" -FPS 60 -TargetBitrate 35000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Notes "FIXED-hevc_gfxcapture"
Run-Test -Encoder "hevc_nvenc" -CaptureMethod "gdigrab" -FPS 30 -TargetBitrate 35000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Notes "FIXED-hevc_gdigrab_30"

# hevc_nvenc quality presets
Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Lookahead 16 -Notes "FIXED-hevc_recommended"
Run-Test -Encoder "hevc_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 150000000 -RateControl "cbr" -Preset "p1" -Tune "ll" -Zerolatency 0 -SpatialAQ 1 -TemporalAQ 0 -Lookahead 32 -Notes "FIXED-hevc_maximum"

# ════════════════════════════════════════════════════
# GROUP 4: av1_nvenc — FIXED (temporal-aq=0)
# ════════════════════════════════════════════════════
Write-Host "`n╔══ av1_nvenc — FIXED (temporal-aq=0) ══╗" -ForegroundColor Magenta

$hasAV1 = ($(& $FFmpegPath -hide_banner -encoders 2>&1 | Select-String "av1_nvenc").Count -gt 0)
if ($hasAV1) {
    Run-Test -Encoder "av1_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Notes "FIXED-av1_ddagrab"
    Run-Test -Encoder "av1_nvenc" -CaptureMethod "gfxcapture" -FPS 60 -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 0 -Notes "FIXED-av1_gfxcapture"
} else {
    Write-Host "  [SKIP] av1_nvenc not available" -ForegroundColor DarkYellow
}

# ════════════════════════════════════════════════════
# GROUP 5: Bitrate Accuracy (CBR)
# ════════════════════════════════════════════════════
Write-Host "`n╔══ Bitrate Accuracy Tests ══╗" -ForegroundColor Magenta

$brs = @(@(10, 10000000), @(25, 25000000), @(50, 50000000), @(80, 80000000), @(100, 100000000), @(150, 150000000))
foreach ($b in $brs) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS 60 -TargetBitrate $b[1] -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Notes "Bitrate=$($b[0])Mbps"
}

# ════════════════════════════════════════════════════
# GROUP 6: FPS Tests
# ════════════════════════════════════════════════════
Write-Host "`n╔══ FPS Tests ══╗" -ForegroundColor Magenta

foreach ($f in @(30, 60)) {
    Run-Test -Encoder "h264_nvenc" -CaptureMethod "ddagrab" -FPS $f -TargetBitrate 50000000 -RateControl "cbr" -Preset "p4" -Tune "ll" -Zerolatency 1 -SpatialAQ 1 -TemporalAQ 1 -Notes "FPS=$f"
}

# ════════════════════════════════════════════════════
# SUMMARY
# ════════════════════════════════════════════════════

Write-Host "`n`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
$csv = Import-Csv $csvPath
$p = ($csv | Where-Object { $_.Status -eq "PASS" }).Count
$f = ($csv | Where-Object { $_.Status -eq "FAIL" }).Count
Write-Host "  DONE: $testID tests | PASS: $p | FAIL: $f" -ForegroundColor Cyan
Write-Host "  CSV: $csvPath" -ForegroundColor White
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
