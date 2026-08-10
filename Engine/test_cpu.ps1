<#
═══════════════════════════════════════════════════════════════════════
  ShadowPlay Engine - CPU Encoder Comprehensive Test Script
  Tests libx264 / libx265 / svtav1
  CPU specifics:
    - gdigrab: software capture, -pix_fmt always needed
    - ddagrab: hw capture, need hwdownload + format before encoding
    - Rate control: CRF (default) / CBR (-b:v)
    - libx264 presets: ultrafast..veryslow (10 levels)
    - libx265 presets: ultrafast..veryslow (10 levels)
    - svtav1 presets: 0..13 (speed preset, lower = slower/better)
    - 10-bit: libx265 and svtav1 support yuv420p10le
═══════════════════════════════════════════════════════════════════════

USAGE:
  .\test_cpu.ps1
  .\test_cpu.ps1 -FFmpegPath "C:\API-Core\ffmpeg.exe"
  .\test_cpu.ps1 -Duration 5    (shorter tests)
  .\test_cpu.ps1 -Quick         (minimal test set)

OUTPUT:
  Results: CPU_TestResults.csv
  Logs:    Logs\cpu_test_*.log
  Videos:  CPU_Tests\
#>

param(
    [string]$FFmpegPath = "",
    [int]$Duration = 8,
    [switch]$Quick
)

# -- Auto-find FFmpeg --
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

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  ShadowPlay CPU Encoder Comprehensive Test" -ForegroundColor Cyan
Write-Host "  FFmpeg: $FFmpegPath" -ForegroundColor Cyan
Write-Host "  Duration per test: ${Duration}s" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

# -- Directories --
$testDir = "CPU_Tests"
$logDir  = "Logs"
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# -- Verify FFmpeg + CPU encoders --
Write-Host ""
Write-Host "[CHECK] Verifying FFmpeg and CPU encoder support..." -ForegroundColor Yellow
$encodersOutput = & $FFmpegPath -hide_banner -encoders 2>&1
$hasX264  = ($encodersOutput | Select-String "libx264" | Measure-Object).Count
$hasX265  = ($encodersOutput | Select-String "libx265" | Measure-Object).Count
$hasSVT   = ($encodersOutput | Select-String "svt_av1" | Measure-Object).Count
$hasAV1E  = ($encodersOutput | Select-String "libaom-av1" | Measure-Object).Count

$x264Color = if ($hasX264 -gt 0) { "Green" } else { "Red" }
$x265Color = if ($hasX265 -gt 0) { "Green" } else { "Red" }
$svtColor  = if ($hasSVT  -gt 0) { "Green" } else { "Red" }
$av1eColor = if ($hasAV1E -gt 0) { "Green" } else { "Red" }

Write-Host ("  libx264:     " + $(if ($hasX264 -gt 0) { "FOUND" } else { "NOT FOUND" })) -ForegroundColor $x264Color
Write-Host ("  libx265:     " + $(if ($hasX265 -gt 0) { "FOUND" } else { "NOT FOUND" })) -ForegroundColor $x265Color
Write-Host ("  svt_av1:     " + $(if ($hasSVT  -gt 0) { "FOUND" } else { "NOT FOUND" })) -ForegroundColor $svtColor
Write-Host ("  libaom-av1:  " + $(if ($hasAV1E -gt 0) { "FOUND" } else { "NOT FOUND" })) -ForegroundColor $av1eColor

# Select primary AV1 encoder (prefer svtav1 over libaom)
if ($hasSVT -gt 0) {
    $av1Encoder = "svt_av1"
    Write-Host ""
    Write-Host "  [INFO] Using svt_av1 as primary AV1 encoder (faster)" -ForegroundColor Yellow
} elseif ($hasAV1E -gt 0) {
    $av1Encoder = "libaom-av1"
    Write-Host ""
    Write-Host "  [SKIP] libaom-av1 is too slow for real-time testing - skipping all AV1 tests" -ForegroundColor Yellow
    $av1Encoder = ""
} else {
    $av1Encoder = ""
}

# -- CSV header --
$csvFile = "CPU_TestResults.csv"
$csvHeader = "TestID,Group,Encoder,CaptureMethod,RateControl,Preset,CRF,Tune,TargetFPS,TargetKbps,PixFmt,Duration,Status,ExitCode,ActualFPS,ActualKbps,ActualWidth,ActualHeight,FileSizeKB,Command"
$csvHeader | Out-File -FilePath $csvFile -Encoding UTF8

# -- Run single test --
function Run-Test {
    param(
        [int]$TestId,
        [string]$Group,
        [string]$Encoder,
        [string]$CaptureMethod,
        [string]$RateControl,
        [string]$Preset,
        [string]$CRF,
        [string]$Tune,
        [int]$TargetFPS,
        [int]$TargetKbps,
        [string]$PixFmt,
        [string]$ScaleFilter,
        [string]$Notes
    )

    $testName = "CPU_{0:D3}_{1}_{2}" -f $TestId, $Encoder, $CaptureMethod
    $outFile  = Join-Path $testDir "$testName.mp4"
    $logFile  = Join-Path $logDir ("cpu_test_" + $TestId.ToString("000") + ".log")

    # -- Build FFmpeg args --
    $argList = @("-hide_banner", "-loglevel", "info")
    $fpsStr = $TargetFPS.ToString()

    # Video input
    switch ($CaptureMethod.ToLower()) {
        "ddagrab" {
            $argList += @("-f", "lavfi", "-i", "ddagrab=output_idx=0:framerate=$fpsStr")
        }
        "gdigrab" {
            $argList += @("-f", "gdigrab", "-framerate", $fpsStr, "-i", "desktop")
        }
        "gfxcapture" {
            $argList += @("-f", "lavfi", "-i", "gfxcapture=monitor_idx=0:max_framerate=$fpsStr")
        }
    }

    # Video filter
    $vf = ""
    switch ($CaptureMethod.ToLower()) {
        "ddagrab" {
            # d3d11 hw frames -> download for CPU encoding
            if ($ScaleFilter -ne "") {
                $vf = "hwdownload,format=nv12,$ScaleFilter"
            } else {
                $vf = "hwdownload,format=nv12"
            }
        }
        "gfxcapture" {
            # d3d11 VFR -> fps -> download for CPU encoding
            if ($ScaleFilter -ne "") {
                $vf = "fps=$fpsStr,hwdownload,format=nv12,$ScaleFilter"
            } else {
                $vf = "fps=$fpsStr,hwdownload,format=nv12"
            }
        }
        "gdigrab" {
            # Software frames, just scale if needed
            if ($ScaleFilter -ne "") {
                $vf = $ScaleFilter
            }
        }
    }

    if ($vf -ne "") {
        $argList += @("-vf", $vf)
    }

    # Encoder
    $argList += @("-c:v", $Encoder)

    # Preset
    if ($Preset -ne "") {
        $argList += @("-preset", $Preset)
    }

    # Rate control
    switch ($RateControl.ToLower()) {
        "crf" {
            # CRF mode — quality-based, no bitrate target
            $argList += @("-crf", $CRF)
        }
        "cbr" {
            # CBR mode — bitrate-based
            $br = $TargetKbps * 1000
            $bufSize = $br * 2
            $argList += @("-b:v", $br.ToString(), "-bufsize", $bufSize.ToString())
            # For x264/x265, also need -x264-params nal-hrd=cbr or similar
            # But basic CBR works with -b:v alone
        }
    }

    # Tune (x264/x265 only)
    if ($Tune -ne "") {
        $argList += @("-tune", $Tune)
    }

    # Pixel format
    if ($PixFmt -ne "") {
        $argList += @("-pix_fmt", $PixFmt)
    }

    # Duration limit
    $argList += @("-t", $Duration.ToString())

    # Output
    $argList += @("-movflags", "+faststart", "-y", $outFile)

    $argStr = $argList -join " "

    # Build label safely
    $rcLabel = if ($RateControl -eq "crf") { "CRF " + $CRF } else { $RateControl.ToUpper() + " " + $TargetKbps.ToString() + "K" }
    $label = "[CPU-" + $TestId.ToString("000") + "] " + $Encoder + " / " + $CaptureMethod + " / " + $rcLabel + " / " + $Preset + " / " + $TargetFPS.ToString() + "fps"
    Write-Host ""
    Write-Host "--- $label ---" -ForegroundColor White
    Write-Host "  CMD: $FFmpegPath $argStr" -ForegroundColor DarkGray

    # Run FFmpeg
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $proc = Start-Process -FilePath $FFmpegPath -ArgumentList $argList -NoNewWindow -Wait -PassThru -RedirectStandardError $logFile
    $sw.Stop()

    $exitCode = $proc.ExitCode
    $duration = [math]::Round($sw.Elapsed.TotalSeconds, 1)

    # Parse results
    $actualFPS = ""
    $actualKbps = ""
    $actualW = ""
    $actualH = ""
    $fileSizeKB = 0

    if (Test-Path $outFile) {
        $fi = Get-Item $outFile
        $fileSizeKB = [math]::Round($fi.Length / 1024)
    }

    # Probe output
    $probeFile = Join-Path $testDir ("probe_" + $TestId.ToString("000") + ".txt")
    & $FFmpegPath -hide_banner -i $outFile 2>&1 | Out-File -FilePath $probeFile -Encoding UTF8
    $probeContent = Get-Content $probeFile -Raw -ErrorAction SilentlyContinue

    if ($probeContent) {
        if ($probeContent -match "(\d+(?:\.\d+)?)\s*fps,") { $actualFPS = $Matches[1] }
        if ($probeContent -match " bitrate:\s*(\d+(?:\.\d+)?)\s*kb/s") { $actualKbps = $Matches[1] }
        if ($probeContent -match "(\d+)x(\d+)") { $actualW = $Matches[1]; $actualH = $Matches[2] }
    }

    # Status
    $status = "PASS"
    if ($exitCode -ne 0) {
        $status = "FAIL"
    } elseif (-not (Test-Path $outFile)) {
        $status = "NO_FILE"
    } elseif ($fileSizeKB -lt 10) {
        $status = "TOO_SMALL"
    }

    # Bitrate accuracy (for CBR only)
    $brAccuracy = ""
    if ($actualKbps -ne "" -and $RateControl -eq "cbr") {
        $ratio = [double]$actualKbps / $TargetKbps * 100
        $brAccuracy = "{0:N0}%" -f $ratio
    }

    # FPS accuracy
    $fpsAccuracy = ""
    if ($actualFPS -ne "") {
        $ratio = [double]$actualFPS / $TargetFPS * 100
        $fpsAccuracy = "{0:N0}%" -f $ratio
    }

    # Print result
    $color = if ($status -eq "PASS") {[ConsoleColor]::Green} else {[ConsoleColor]::Red}
    Write-Host ("  " + $status + " | exit=" + $exitCode + " | dur=" + $duration + "s | fps=" + $actualFPS + " (" + $fpsAccuracy + ") | br=" + $actualKbps + "kbps (" + $brAccuracy + ") | res=" + $actualW + "x" + $actualH + " | size=" + $fileSizeKB + "KB") -ForegroundColor $color

    if ($status -eq "FAIL") {
        $errLines = Get-Content $logFile -Tail 5 -ErrorAction SilentlyContinue
        if ($errLines) {
            foreach ($line in $errLines) {
                if ($line -match "error|not supported|invalid|failed") {
                    Write-Host "    ERROR: $line" -ForegroundColor Red
                }
            }
        }
    }

    # CSV row
    $csvRow = "$TestId,$Group,$Encoder,$CaptureMethod,$RateControl,$Preset,$CRF,$Tune,$TargetFPS,$TargetKbps,$PixFmt,$duration,$status,$exitCode,$actualFPS,$actualKbps,$actualW,$actualH,$fileSizeKB,$argStr"
    $csvRow | Out-File -FilePath $csvFile -Append -Encoding UTF8

    # Clean up
    if (Test-Path $outFile) { Remove-Item $outFile -Force -ErrorAction SilentlyContinue }
    if (Test-Path $probeFile) { Remove-Item $probeFile -Force -ErrorAction SilentlyContinue }

    return $status
}

# ============================================================
# TEST GROUPS
# ============================================================

$testCounter = 0
$totalPass = 0
$totalFail = 0

# -----------------------------------------------
# GROUP 1: libx264 - Capture Methods
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 1] libx264 - Capture Methods" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    foreach ($method in @("gdigrab", "ddagrab", "gfxcapture")) {
        $fps = if ($method -eq "gdigrab") { 30 } else { 60 }
        $pix = if ($method -eq "gdigrab") { "yuv420p" } else { "nv12" }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G1_X264_Capture" -Encoder "libx264" -CaptureMethod $method -RateControl "crf" -Preset "medium" -CRF "23" -TargetFPS $fps -TargetKbps 50000 -PixFmt $pix
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
} else {
    Write-Host "  [SKIP] libx264 not found" -ForegroundColor Yellow
}

# -----------------------------------------------
# GROUP 2: libx264 - CRF Quality Levels
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 2] libx264 - CRF Quality Levels" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    foreach ($crf in @(18, 20, 23, 26, 28, 30)) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G2_X264_CRF" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF $crf -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 3: libx264 - Presets (Speed vs Quality)
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 3] libx264 - Presets" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    $presets = @("ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow")
    foreach ($p in $presets) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G3_X264_Presets" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset $p -CRF "23" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 4: libx264 - CBR Mode
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 4] libx264 - CBR Mode" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    $tiers = @(@{Name="5M"; Kbps=5000}, @{Name="10M"; Kbps=10000}, @{Name="35M"; Kbps=35000}, @{Name="50M"; Kbps=50000})
    foreach ($tier in $tiers) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G4_X264_CBR" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "medium" -CRF "" -TargetFPS 30 -TargetKbps $tier.Kbps -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 5: libx264 - Tune Options
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 5] libx264 - Tune Options" -ForegroundColor Magenta
if ($hasX264 -gt 0 -and -not $Quick) {
    foreach ($tune in @("film", "animation", "game", "zerolatency", "fastdecode", "stillimage")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G5_X264_Tune" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "23" -Tune $tune -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 6: libx264 - Pixel Formats
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 6] libx264 - Pixel Formats" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    foreach ($pix in @("yuv420p", "nv12", "yuvj420p")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G6_X264_PixFmt" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "23" -TargetFPS 30 -TargetKbps 0 -PixFmt $pix
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    # 10-bit (x264 supports 8-bit only, expect FAIL or convert)
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G6_X264_PixFmt" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "23" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p10le"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 7: libx264 - FPS and Scale
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 7] libx264 - FPS & Scale" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    # 60fps via gdigrab
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G7_X264_FPS" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "23" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # Scale 1920x1080
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G7_X264_FPS" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "23" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p" -ScaleFilter "scale=1920:1080"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    # 1440p via ddagrab + hwdownload
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G7_X264_FPS" -Encoder "libx264" -CaptureMethod "ddagrab" -RateControl "crf" -Preset "medium" -CRF "23" -TargetFPS 30 -TargetKbps 0 -PixFmt "nv12" -ScaleFilter "scale=2560:1440"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 8: libx265 - Capture Methods
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 8] libx265 - Capture Methods" -ForegroundColor Magenta
if ($hasX265 -gt 0) {
    foreach ($method in @("gdigrab", "ddagrab")) {
        $fps = if ($method -eq "gdigrab") { 30 } else { 60 }
        $pix = if ($method -eq "gdigrab") { "yuv420p" } else { "nv12" }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G8_X265_Capture" -Encoder "libx265" -CaptureMethod $method -RateControl "crf" -Preset "medium" -CRF "28" -TargetFPS $fps -TargetKbps 0 -PixFmt $pix
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
} else {
    Write-Host "  [SKIP] libx265 not found" -ForegroundColor Yellow
}

# -----------------------------------------------
# GROUP 9: libx265 - CRF Quality Levels
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 9] libx265 - CRF Quality Levels" -ForegroundColor Magenta
if ($hasX265 -gt 0) {
    foreach ($crf in @(20, 23, 26, 28, 30, 35)) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G9_X265_CRF" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF $crf -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 10: libx265 - Presets
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 10] libx265 - Presets" -ForegroundColor Magenta
if ($hasX265 -gt 0) {
    $presets = @("ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow")
    if ($Quick) {
        $presets = @("ultrafast", "medium", "veryslow")
    }
    foreach ($p in $presets) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G10_X265_Presets" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "crf" -Preset $p -CRF "28" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 11: libx265 - CBR Mode
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 11] libx265 - CBR Mode" -ForegroundColor Magenta
if ($hasX265 -gt 0) {
    $tiers = @(@{Name="5M"; Kbps=5000}, @{Name="10M"; Kbps=10000}, @{Name="35M"; Kbps=35000}, @{Name="50M"; Kbps=50000})
    foreach ($tier in $tiers) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G11_X265_CBR" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "medium" -CRF "" -TargetFPS 30 -TargetKbps $tier.Kbps -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 12: libx265 - 10-bit Encoding
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 12] libx265 - 10-bit Encoding" -ForegroundColor Magenta
if ($hasX265 -gt 0 -and -not $Quick) {
    # 10-bit CRF
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G12_X265_10bit" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "28" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p10le"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # 10-bit CBR
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G12_X265_10bit" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "medium" -CRF "" -TargetFPS 30 -TargetKbps 35000 -PixFmt "yuv420p10le"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # 10-bit presets comparison
    foreach ($p in @("fast", "medium", "slow")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G12_X265_10bit" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "crf" -Preset $p -CRF "28" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p10le"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 13: libx265 - Tune Options
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 13] libx265 - Tune Options" -ForegroundColor Magenta
if ($hasX265 -gt 0 -and -not $Quick) {
    foreach ($tune in @("grain", "fastdecode", "zerolatency")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G13_X265_Tune" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "medium" -CRF "28" -Tune $tune -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 14: svt_av1 - Capture Methods
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 14] svt_av1 - Capture Methods" -ForegroundColor Magenta
if ($av1Encoder -ne "") {
    foreach ($method in @("gdigrab", "ddagrab")) {
        $fps = if ($method -eq "gdigrab") { 30 } else { 60 }
        $pix = if ($method -eq "gdigrab") { "yuv420p" } else { "nv12" }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G14_SVT_Capture" -Encoder $av1Encoder -CaptureMethod $method -RateControl "crf" -Preset "" -CRF "30" -TargetFPS $fps -TargetKbps 0 -PixFmt $pix
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
} else {
    Write-Host "  [SKIP] No AV1 encoder found (svt_av1 / libaom-av1)" -ForegroundColor Yellow
}

# -----------------------------------------------
# GROUP 15: svt_av1 - CRF Quality Levels
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 15] svt_av1 - CRF Quality Levels" -ForegroundColor Magenta
if ($av1Encoder -ne "") {
    foreach ($crf in @(25, 30, 35, 40, 45, 50)) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G15_SVT_CRF" -Encoder $av1Encoder -CaptureMethod "gdigrab" -RateControl "crf" -Preset "" -CRF $crf -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 16: svt_av1 - Speed Presets
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 16] svt_av1 - Speed Presets" -ForegroundColor Magenta
if ($av1Encoder -ne "") {
    $svtPresets = @("0", "4", "6", "8", "11", "13")
    if ($Quick) {
        $svtPresets = @("0", "6", "13")
    }
    foreach ($sp in $svtPresets) {
        $presetArg = ""
        $presetLabel = "svt-preset=$sp"
        if ($av1Encoder -eq "svt_av1") {
            $presetArg = "-preset $sp"
        }
        # For svtav1, preset is numeric speed (0=slowest/best, 13=fastest)
        # We pass it as the Preset parameter, Run-Test handles -preset flag
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G16_SVT_Presets" -Encoder $av1Encoder -CaptureMethod "gdigrab" -RateControl "crf" -Preset $sp -CRF "30" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 17: svt_av1 - CBR Mode
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 17] svt_av1 - CBR Mode" -ForegroundColor Magenta
if ($av1Encoder -ne "") {
    $tiers = @(@{Name="5M"; Kbps=5000}, @{Name="10M"; Kbps=10000}, @{Name="35M"; Kbps=35000})
    foreach ($tier in $tiers) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G17_SVT_CBR" -Encoder $av1Encoder -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "" -CRF "" -TargetFPS 30 -TargetKbps $tier.Kbps -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 18: svt_av1 - 10-bit
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 18] svt_av1 - 10-bit Encoding" -ForegroundColor Magenta
if ($av1Encoder -ne "" -and -not $Quick) {
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G18_SVT_10bit" -Encoder $av1Encoder -CaptureMethod "gdigrab" -RateControl "crf" -Preset "" -CRF "30" -TargetFPS 30 -TargetKbps 0 -PixFmt "yuv420p10le"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}

# -----------------------------------------------
# GROUP 19: libx264 - Game / Zero-latency (ShadowPlay typical)
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 19] libx264 - Game Capture Profiles" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    # zerolatency + ultrafast (lowest latency)
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G19_X264_Game" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "ultrafast" -CRF "23" -Tune "zerolatency" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # game tune
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G19_X264_Game" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "veryfast" -CRF "23" -Tune "zerolatency" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # fast + game
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G19_X264_Game" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "fast" -CRF "23" -Tune "zerolatency" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# -----------------------------------------------
# GROUP 20: Best Config Stress Test
# -----------------------------------------------
Write-Host ""
Write-Host "[GROUP 20] Best Config - Stress Tests" -ForegroundColor Magenta
if ($hasX264 -gt 0) {
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G20_Stress" -Encoder "libx264" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "ultrafast" -CRF "20" -Tune "zerolatency" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}
if ($hasX265 -gt 0) {
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G20_Stress" -Encoder "libx265" -CaptureMethod "gdigrab" -RateControl "crf" -Preset "fast" -CRF "26" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}
if ($av1Encoder -ne "") {
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G20_Stress" -Encoder $av1Encoder -CaptureMethod "gdigrab" -RateControl "crf" -Preset "8" -CRF "30" -TargetFPS 60 -TargetKbps 0 -PixFmt "yuv420p"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}

# ============================================================
# SUMMARY
# ============================================================
Write-Host ""
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ("  DONE: " + $testCounter.ToString() + " tests | PASS: " + $totalPass.ToString() + " | FAIL: " + $totalFail.ToString()) -ForegroundColor Cyan
Write-Host ("  CSV: " + $csvFile) -ForegroundColor Cyan
Write-Host ("  Logs: " + $logDir + "\") -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

# Show failed tests
if ($totalFail -gt 0) {
    Write-Host ""
    Write-Host "  FAILED TESTS:" -ForegroundColor Red
    $csvData = Import-Csv $csvFile
    $csvData | Where-Object {$_.Status -ne "PASS"} | ForEach-Object {
        $rcInfo = if ($_.RateControl -eq "crf") { "CRF=" + $_.CRF } else { $_.RateControl.ToUpper() + " " + $_.TargetKbps + "K" }
        Write-Host ("    [CPU-" + $_.TestID + "] " + $_.Encoder + " / " + $_.CaptureMethod + " / " + $rcInfo + " / " + $_.Preset + " - exit=" + $_.ExitCode) -ForegroundColor Red
    }
}

Write-Host ""
