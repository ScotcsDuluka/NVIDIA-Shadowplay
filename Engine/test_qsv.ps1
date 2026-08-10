<#
═══════════════════════════════════════════════════════════════════════
  ShadowPlay Engine — Intel QSV Comprehensive Test Script
  Tests h264_qsv / hevc_qsv / av1_qsv
  QSV specifics:
    - ddagrab/gfxcapture d3d11 → hwmap=derive_device=qsv
    - Rate control: cbr / vbr / icq / cqp (NOT constqp)
    - Presets: veryslow / slow / medium / fast / faster
    - Look-ahead: -look_ahead 1 -look_ahead_depth N
    - No temporal-aq / spatial-aq / psycho-aq (NVENC only)
═══════════════════════════════════════════════════════════════════════

USAGE:
  .\test_qsv.ps1
  .\test_qsv.ps1 -FFmpegPath "C:\API-Core\ffmpeg.exe"
  .\test_qsv.ps1 -Duration 5    (shorter tests)
  .\test_qsv.ps1 -Quick         (minimal test set)

OUTPUT:
  Results: QSV_TestResults.csv
  Logs:    Logs\qsv_test_*.log
  Videos:  QSV_Tests\
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
Write-Host "  ShadowPlay Intel QSV Comprehensive Test" -ForegroundColor Cyan
Write-Host "  FFmpeg: $FFmpegPath" -ForegroundColor Cyan
Write-Host "  Duration per test: ${Duration}s" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# ── Directories ──
$testDir = "QSV_Tests"
$logDir  = "Logs"
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# ── Verify FFmpeg + QSV encoders ──
Write-Host "`n[CHECK] Verifying FFmpeg and QSV support..." -ForegroundColor Yellow
$encodersOutput = & $FFmpegPath -hide_banner -encoders 2>&1
$hasH264 = $encodersOutput | Select-String "h264_qsv" | Measure-Object | Select-Object -ExpandProperty Count
$hasHEVC = $encodersOutput | Select-String "hevc_qsv" | Measure-Object | Select-Object -ExpandProperty Count
$hasAV1  = $encodersOutput | Select-String "av1_qsv"  | Measure-Object | Select-Object -ExpandProperty Count

Write-Host "  h264_qsv: $(if ($hasH264 -gt 0) {'FOUND'} else {'NOT FOUND'})" -ForegroundColor $(if ($hasH264) {[ConsoleColor]::Green} else {[ConsoleColor]::Red})
Write-Host "  hevc_qsv: $(if ($hasHEVC -gt 0) {'FOUND'} else {'NOT FOUND'})" -ForegroundColor $(if ($hasHEVC) {[ConsoleColor]::Green} else {[ConsoleColor]::Red})
Write-Host "  av1_qsv:  $(if ($hasAV1  -gt 0) {'FOUND'} else {'NOT FOUND'})" -ForegroundColor $(if ($hasAV1)  {[ConsoleColor]::Green} else {[ConsoleColor]::Red})

# ── CSV header ──
$csvFile = "QSV_TestResults.csv"
$csvHeader = "TestID,Group,Encoder,CaptureMethod,RateControl,Preset,Tune,ExtraOptions,TargetFPS,TargetKbps,PixFmt,Duration,Status,ExitCode,ActualFPS,ActualKbps,ActualWidth,ActualHeight,FileSizeKB,Command"
$csvHeader | Out-File -FilePath $csvFile -Encoding UTF8

# ── Run single test ──
function Run-Test {
    param(
        [int]$TestId,
        [string]$Group,
        [string]$Encoder,
        [string]$CaptureMethod,
        [string]$RateControl,
        [string]$Preset,
        [string]$Tune,
        [string]$ExtraOptions,
        [int]$TargetFPS,
        [int]$TargetKbps,
        [string]$PixFmt,
        [string]$ScaleFilter,
        [string]$Notes
    )

    $testName = "QSV_{0:D3}_{1}_{2}" -f $TestId, $Encoder, $CaptureMethod
    $outFile  = Join-Path $testDir "$testName.mp4"
    $logFile  = Join-Path $logDir "qsv_test_$($TestId.ToString('000')).log"

    # ── Build FFmpeg args ──
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

    # Video filter — QSV needs hwmap from d3d11
    $vf = ""
    switch ($CaptureMethod.ToLower()) {
        "ddagrab" {
            # d3d11 → QSV via hwmap
            if ($ScaleFilter -ne "") {
                # Scale: hwdownload first, then scale, then upload to QSV
                $vf = "hwdownload,format=nv12,$ScaleFilter,hwupload=extra_hw_frames=64"
            } else {
                $vf = "hwmap=derive_device=qsv"
            }
        }
        "gfxcapture" {
            # d3d11 VFR → fps → QSV
            if ($ScaleFilter -ne "") {
                $vf = "fps=$fpsStr,hwdownload,format=nv12,$ScaleFilter,hwupload=extra_hw_frames=64"
            } else {
                $vf = "fps=$fpsStr,hwmap=derive_device=qsv"
            }
        }
        "gdigrab" {
            # Software frames → upload to QSV
            if ($ScaleFilter -ne "") {
                $vf = "$ScaleFilter,hwupload=extra_hw_frames=64"
            } else {
                $vf = "hwupload=extra_hw_frames=64"
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
    $br = $TargetKbps * 1000  # kbps → bps
    $bufSize = $br * 2

    switch ($RateControl.ToLower()) {
        "cbr" {
            $argList += @("-rc", "cbr", "-b:v", $br.ToString(), "-bufsize", $bufSize.ToString())
        }
        "vbr" {
            $argList += @("-rc", "vbr", "-b:v", $br.ToString(), "-bufsize", $bufSize.ToString())
        }
        "icq" {
            # ICQ = Intelligent Constant Quality — bitrate is ignored, quality level matters
            $argList += @("-rc", "icq", "-icq", "28")
        }
        "cqp" {
            $argList += @("-rc", "cqp", "-qp", "28")
        }
        "la_icq" {
            # Look-ahead ICQ
            $argList += @("-rc", "la_icq", "-icq", "28", "-look_ahead", "1", "-look_ahead_depth", "30")
        }
        "la" {
            # Look-ahead CBR
            $argList += @("-rc", "la", "-b:v", $br.ToString(), "-bufsize", $bufSize.ToString(), "-look_ahead", "1", "-look_ahead_depth", "30")
        }
    }

    # Extra options
    if ($ExtraOptions -ne "") {
        $extraParts = $ExtraOptions -split "\s+"
        foreach ($part in $extraParts) {
            if ($part -ne "") { $argList += $part }
        }
    }

    # Pixel format — skip for hw capture (QSV handles via hwmap/hwupload)
    $isHwCapture = ($CaptureMethod.ToLower() -eq "ddagrab" -or $CaptureMethod.ToLower() -eq "gfxcapture")
    if ($PixFmt -ne "" -and -not $isHwCapture) {
        $argList += @("-pix_fmt", $PixFmt)
    }

    # Output
    $argList += @("-movflags", "+faststart", "-y", $outFile)

    $argStr = $argList -join " "

    Write-Host ""
    Write-Host "━━━ [QSV-$($TestId.ToString('000'))] $Encoder | $CaptureMethod | $RateControl | $Preset | ${TargetFPS}fps | ${TargetKbps}Kbps ━━━" -ForegroundColor White
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
    $probeFile = Join-Path $testDir "probe_$($TestId.ToString('000')).txt"
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

    # Bitrate accuracy
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
    Write-Host "  $($status) | exit=$exitCode | dur=${duration}s | fps=$actualFPS ($fpsAccuracy) | br=${actualKbps}kbps ($brAccuracy) | res=${actualW}x${actualH} | size=${fileSizeKB}KB" -ForegroundColor $color

    if ($status -eq "FAIL") {
        # Show error from log
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
    $csvRow = "$TestId,$Group,$Encoder,$CaptureMethod,$RateControl,$Preset,$Tune,$ExtraOptions,$TargetFPS,$TargetKbps,$PixFmt,$duration,$status,$exitCode,$actualFPS,$actualKbps,$actualW,$actualH,$fileSizeKB,$argStr"
    $csvRow | Out-File -FilePath $csvFile -Append -Encoding UTF8

    # Clean up
    if (Test-Path $outFile) { Remove-Item $outFile -Force -ErrorAction SilentlyContinue }
    if (Test-Path $probeFile) { Remove-Item $probeFile -Force -ErrorAction SilentlyContinue }

    return $status
}

# ═══════════════════════════════════════════════════════════════
# TEST GROUPS
# ═══════════════════════════════════════════════════════════════

$testCounter = 0
$totalPass = 0
$totalFail = 0

# ─────────────────────────────────────────────────────────
# GROUP 1: h264_qsv — Capture Method Comparison
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ h264_qsv — Capture Methods ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0) {
    foreach ($method in @("ddagrab", "gfxcapture", "gdigrab")) {
        $fps = if ($method -eq "gdigrab") { 30 } else { 60 }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G1_Capture" -Encoder "h264_qsv" -CaptureMethod $method -RateControl "cbr" -Preset "medium" -TargetFPS $fps -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
} else {
    Write-Host "  [SKIP] h264_qsv not found" -ForegroundColor Yellow
}

# ─────────────────────────────────────────────────────────
# GROUP 2: h264_qsv — Rate Control
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ h264_qsv — Rate Control ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0) {
    foreach ($rc in @("cbr", "vbr", "icq", "cqp")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G2_RateControl" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl $rc -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    # Look-ahead variants
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G2_RateControl" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "la" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G2_RateControl" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "la_icq" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 3: h264_qsv — Presets
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ h264_qsv — Presets ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0) {
    foreach ($p in @("veryslow", "slow", "medium", "fast", "faster")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G3_Presets" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset $p -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 4: h264_qsv — Look-ahead
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ h264_qsv — Look-ahead ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0 -and -not $Quick) {
    foreach ($depth in @(15, 30, 50)) {
        $testCounter++
        $extra = "-look_ahead 1 -look_ahead_depth $depth"
        $result = Run-Test -TestId $testCounter -Group "G4_Lookahead" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -ExtraOptions $extra -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 5: h264_qsv — Bitrate Tiers
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ h264_qsv — Bitrate Tiers ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0) {
    $tiers = @(@{Name="10M"; Kbps=10000}, @{Name="35M"; Kbps=35000}, @{Name="50M"; Kbps=50000}, @{Name="100M"; Kbps=100000})
    foreach ($tier in $tiers) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G5_Bitrate" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps $tier.Kbps -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 6: h264_qsv — FPS & Scale
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ h264_qsv — FPS & Scale ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0) {
    # 30fps
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G6_FPS_Scale" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 30 -TargetKbps 50000 -PixFmt "nv12"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # Scale 1920x1080
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G6_FPS_Scale" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12" -ScaleFilter "scale=1920:1080"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 7: hevc_qsv — Capture Methods
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ hevc_qsv — Capture Methods ══╗" -ForegroundColor Magenta
if ($hasHEVC -gt 0) {
    foreach ($method in @("ddagrab", "gdigrab")) {
        $fps = if ($method -eq "gdigrab") { 30 } else { 60 }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G7_HevcCapture" -Encoder "hevc_qsv" -CaptureMethod $method -RateControl "cbr" -Preset "medium" -TargetFPS $fps -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G7_HevcCapture" -Encoder "hevc_qsv" -CaptureMethod "gfxcapture" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
} else {
    Write-Host "  [SKIP] hevc_qsv not found" -ForegroundColor Yellow
}

# ─────────────────────────────────────────────────────────
# GROUP 8: hevc_qsv — Rate Control
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ hevc_qsv — Rate Control ══╗" -ForegroundColor Magenta
if ($hasHEVC -gt 0) {
    foreach ($rc in @("cbr", "vbr", "icq", "cqp")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G8_HevcRC" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl $rc -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    if (-not $Quick) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G8_HevcRC" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl "la" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G8_HevcRC" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl "la_icq" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 9: hevc_qsv — Presets
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ hevc_qsv — Presets ══╗" -ForegroundColor Magenta
if ($hasHEVC -gt 0) {
    foreach ($p in @("veryslow", "slow", "medium", "fast", "faster")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G9_HevcPresets" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset $p -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 10: hevc_qsv — Bitrate Tiers
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ hevc_qsv — Bitrate Tiers ══╗" -ForegroundColor Magenta
if ($hasHEVC -gt 0) {
    $tiers = @(@{Name="10M"; Kbps=10000}, @{Name="35M"; Kbps=35000}, @{Name="50M"; Kbps=50000}, @{Name="100M"; Kbps=100000})
    foreach ($tier in $tiers) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G10_HevcBitrate" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps $tier.Kbps -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 11: hevc_qsv — 10-bit + Scale
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ hevc_qsv — 10-bit & Scale ══╗" -ForegroundColor Magenta
if ($hasHEVC -gt 0 -and -not $Quick) {
    # 10-bit via gdigrab (software frames)
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G11_Hevc10bit" -Encoder "hevc_qsv" -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "medium" -TargetFPS 30 -TargetKbps 50000 -PixFmt "yuv420p10le"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    # Scale 1920x1080
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G11_Hevc10bit" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12" -ScaleFilter "scale=1920:1080"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}

# ─────────────────────────────────────────────────────────
# GROUP 12: av1_qsv — if supported
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ av1_qsv — Tests ══╗" -ForegroundColor Magenta
if ($hasAV1 -gt 0) {
    foreach ($method in @("ddagrab", "gdigrab")) {
        $fps = if ($method -eq "gdigrab") { 30 } else { 60 }
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G12_Av1" -Encoder "av1_qsv" -CaptureMethod $method -RateControl "cbr" -Preset "medium" -TargetFPS $fps -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    # Rate control
    foreach ($rc in @("cbr", "icq", "cqp")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G12_Av1" -Encoder "av1_qsv" -CaptureMethod "ddagrab" -RateControl $rc -Preset "medium" -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    # Presets
    foreach ($p in @("slow", "medium", "fast")) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G12_Av1" -Encoder "av1_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset $p -TargetFPS 60 -TargetKbps 50000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    # Bitrate tiers
    foreach ($tier in @(@{Name="10M"; Kbps=10000}, @{Name="50M"; Kbps=50000})) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G12_Av1" -Encoder "av1_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps $tier.Kbps -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
} else {
    Write-Host "  [SKIP] av1_qsv not found — skipping all AV1 tests" -ForegroundColor Yellow
}

# ─────────────────────────────────────────────────────────
# GROUP 13: gdigrab — software path tests
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ gdigrab — Software Path ══╗" -ForegroundColor Magenta
if (-not $Quick) {
    if ($hasH264 -gt 0) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G13_Gdi" -Encoder "h264_qsv" -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "medium" -TargetFPS 30 -TargetKbps 35000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
    if ($hasHEVC -gt 0) {
        $testCounter++
        $result = Run-Test -TestId $testCounter -Group "G13_Gdi" -Encoder "hevc_qsv" -CaptureMethod "gdigrab" -RateControl "cbr" -Preset "medium" -TargetFPS 30 -TargetKbps 35000 -PixFmt "nv12"
        if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
    }
}

# ─────────────────────────────────────────────────────────
# GROUP 14: Best Config Stress Test
# ─────────────────────────────────────────────────────────
Write-Host "`n╔══ Best Config — Stress Tests ══╗" -ForegroundColor Magenta
if ($hasH264 -gt 0) {
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G14_Best" -Encoder "h264_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps 100000 -PixFmt "nv12"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}
if ($hasHEVC -gt 0) {
    $testCounter++
    $result = Run-Test -TestId $testCounter -Group "G14_Best" -Encoder "hevc_qsv" -CaptureMethod "ddagrab" -RateControl "cbr" -Preset "medium" -TargetFPS 60 -TargetKbps 100000 -PixFmt "nv12"
    if ($result -eq "PASS") { $totalPass++ } else { $totalFail++ }
}

# ═══════════════════════════════════════════════════════════
# SUMMARY
# ═══════════════════════════════════════════════════════════
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DONE: $testCounter tests | PASS: $totalPass | FAIL: $totalFail" -ForegroundColor Cyan
Write-Host "  CSV: $csvFile" -ForegroundColor Cyan
Write-Host "  Logs: $logDir\" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Show failed tests
if ($totalFail -gt 0) {
    Write-Host "`n  FAILED TESTS:" -ForegroundColor Red
    $csvData = Import-Csv $csvFile
    $csvData | Where-Object {$_.Status -ne "PASS"} | ForEach-Object {
        Write-Host "    [QSV-$($_.TestID)] $($_.Encoder) | $($_.CaptureMethod) | $($_.RateControl) | $($_.Preset) — exit=$($_.ExitCode)" -ForegroundColor Red
    }
}

Write-Host ""
