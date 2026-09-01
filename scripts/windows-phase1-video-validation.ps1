# ============================================================================
# windows-phase1-video-validation.ps1 — PHASE 1 VIDEO RUNTIME VALIDATION KIT
# ============================================================================
# Runs on the OWNER's Windows machine (NVIDIA GPU + NVENC driver required).
# Drives the REAL production pipeline through the CANONICAL config chain
# (the exact composition Engine.ConfigTruth.Tests executes on Linux):
#
#   config.json/video.json + engine.json
#     → NextRecordingConfig.LoadEffectiveSettings
#     → NextRecordingConfig.MapStartupConfig → RecordingEngine.Initialize
#     → NextRecordingConfig.BuildSessionConfig (fresh reload, CT-4)
#     → RecordingEngine.StartSession (real D3D11/ddagrab + NVENC + mux)
#     → ffprobe asserts on the produced MP4
#
# Scenarios (each writes its own temp config dir — user config untouched):
#   S1 FPS        current.fps=60                → avg_frame_rate ≈ 60 (NOT display refresh)
#   S2 NATIVE     use_native_resolution=true    → output dims == primary screen dims
#   S3 CUSTOM     native=false, 1280x720        → output dims == 1280x720
#   S4 BITRATE    current.bitrate=10200 (CBR)   → stream bit_rate within [0.6x, 1.4x]
#   S5 PRESET     current.encoder_preset=7      → driver echo "preset='p7'" (init echo "preset p7")
#   S6 ENCODER    Encoder=h264_nvenc            → ffprobe codec_name == h264
#   S7 GFXCAP     engine.json CaptureMethod=gfxcapture → documented GAP warning in log,
#                 recording proceeds on ddagrab (loud gap, never silent)
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\windows-phase1-video-validation.ps1
#   ... -FFmpeg "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe" -Seconds 8
#
# Evidence: evidence\phase1-video\report.md + per-scenario logs and MP4s.
# ============================================================================

param(
    [string]$FFmpeg = "",
    [string]$OutDir = "",
    [int]$Seconds = 8,
    [string]$Config = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutDir -or $OutDir -eq "") { $OutDir = Join-Path $repoRoot "evidence\phase1-video" }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# ── locate ffmpeg ───────────────────────────────────────────────────────────
if (-not $FFmpeg -or -not (Test-Path $FFmpeg)) {
    $candidates = @(
        (Join-Path $repoRoot "Overlay\API-Core\ffmpeg.exe"),
        "ffmpeg"
    )
    foreach ($c in $candidates) {
        $cmd = Get-Command $c -ErrorAction SilentlyContinue
        if ($cmd) { $FFmpeg = $cmd.Source; break }
        if (Test-Path $c) { $FFmpeg = $c; break }
    }
}
if (-not (Test-Path $FFmpeg) -and $FFmpeg -ne "ffmpeg") {
    Write-Error "ffmpeg.exe not found — pass -FFmpeg <path>"
    exit 2
}
Write-Host "FFmpeg: $FFmpeg"

# ── build the driver ────────────────────────────────────────────────────────
$driverProj = Join-Path $repoRoot "CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj"
dotnet build $driverProj -c $Config -v q --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "driver build FAILED"; exit 2 }
$driverExe = Join-Path $repoRoot "CaptureEngine.Recording.ConsoleDriver\bin\$Config\net10.0-windows\CaptureEngine.Recording.ConsoleDriver.exe"
if (-not (Test-Path $driverExe)) {
    $driverDll = Join-Path $repoRoot "CaptureEngine.Recording.ConsoleDriver\bin\$Config\net10.0-windows\CaptureEngine.Recording.ConsoleDriver.dll"
    if (Test-Path $driverDll) { $driverExe = "dotnet"; $driverArg = $driverDll } else { Write-Error "driver output not found"; exit 2 }
} else { $driverArg = $driverExe }

# ── primary screen dims (for S2 native assert) ──────────────────────────────
Add-Type -AssemblyName System.Windows.Forms
$screenW = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width
$screenH = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height
Write-Host "Primary screen: ${screenW}x${screenH}"

function Write-ScenarioConfig {
    param([string]$Dir, [int]$Fps, [int]$BitrateKbps, [int]$Preset,
          [bool]$Native, [int]$W, [int]$H, [string]$CaptureMethod = "ddagrab")
    New-Item -ItemType Directory -Force -Path $Dir | Out-Null
    # legacy-tier video.json — accepted by CaptureSettings.LoadVideoSettings
    # (camelCase "current" section = same field names as unified config.json
    # Recording.current.*) — the real loader, not a test double.
    $video = @{
        Encoder = "h264_nvenc"
        ActivePreset = "CUSTOM"
        Current = [ordered]@{
            fps = $Fps; bitrate = $BitrateKbps; encoder_preset = $Preset
            use_native_resolution = $Native; width = $W; height = $H
        }
    } | ConvertTo-Json -Depth 4
    Set-Content -Path (Join-Path $Dir "video.json") -Value $video -Encoding UTF8
    # engine.json — engine knobs only (CaptureSettings.Save shape)
    $engine = [ordered]@{
        ConfigVersion = 3
        CaptureMethod = $CaptureMethod
        PixelFormat = "nv12"
        Preset = ""
        RateControl = "cbr"
        UseNativeResolution = $Native
        CustomWidth = $W
        CustomHeight = $H
    } | ConvertTo-Json
    Set-Content -Path (Join-Path $Dir "engine.json") -Value $engine -Encoding UTF8
}

function Invoke-Scenario {
    param([string]$Name, [string]$CfgDir, [string[]]$ExtraArgs = @())
    $mp4 = Join-Path $OutDir "$Name.mp4"
    $log = Join-Path $OutDir "$Name.log"
    if (Test-Path $mp4) { Remove-Item $mp4 -Force }
    Write-Host ""
    Write-Host "=== $Name ==="
    & $driverArg $driverExe --videocheck --config $CfgDir --out $mp4 --seconds $Seconds --ffmpeg $FFmpeg @ExtraArgs 2>&1 | Tee-Object -FilePath $log
    return @{ Mp4 = $mp4; Log = $log; Exit = $LASTEXITCODE }
}

function Invoke-FFProbe {
    param([string]$Mp4)
    if (-not (Test-Path $Mp4)) { return $null }
    $json = & $FFmpeg.Replace("ffmpeg.exe", "ffprobe.exe") -v error -select_streams v:0 `
        -show_entries stream=codec_name,width,height,avg_frame_rate,bit_rate `
        -of json $Mp4 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $json) { return $null }
    return ($json | ConvertFrom-Json).streams[0]
}

$results = New-Object System.Collections.Generic.List[object]

# ═══ S1 FPS — config fps=60 must win over display refresh ═══════════════════
$d = Join-Path $OutDir "cfg_s1"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $true -W 1920 -H 1080
$r = Invoke-Scenario -Name "S1_fps60" -CfgDir $d
$s = Invoke-FFProbe -Mp4 $r.Mp4
$fpsNum = if ($s -and $s.avg_frame_rate -match '^(\d+)/(\d+)$') { [math]::Round([double]$Matches[1] / [double]$Matches[2], 2) } else { 0 }
$ok = ($s -ne $null) -and ([math]::Abs($fpsNum - 60) -le 2)
$results.Add([pscustomobject]@{ S="S1"; Field="FPS"; Expect="avg_frame_rate≈60 (config wins, not display)"; Got="$($s.avg_frame_rate) → $fpsNum fps"; Verdict=$(if($ok){"PASS"}else{"FAIL"}) })

# ═══ S2 NATIVE — encode at desktop resolution ═══════════════════════════════
$d = Join-Path $OutDir "cfg_s2"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $true -W 1920 -H 1080
$r = Invoke-Scenario -Name "S2_native" -CfgDir $d
$s = Invoke-FFProbe -Mp4 $r.Mp4
$ok = ($s -ne $null) -and ($s.width -eq $screenW) -and ($s.height -eq $screenH)
$results.Add([pscustomobject]@{ S="S2"; Field="Resolution(native)"; Expect="${screenW}x${screenH}"; Got="$($s.width)x$($s.height)"; Verdict=$(if($ok){"PASS"}else{"FAIL"}) })

# ═══ S3 CUSTOM — 1280x720 GPU downscale ═════════════════════════════════════
$d = Join-Path $OutDir "cfg_s3"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $false -W 1280 -H 720
$r = Invoke-Scenario -Name "S3_custom720" -CfgDir $d
$s = Invoke-FFProbe -Mp4 $r.Mp4
$ok = ($s -ne $null) -and ($s.width -eq 1280) -and ($s.height -eq 720)
$results.Add([pscustomobject]@{ S="S3"; Field="Resolution(custom)"; Expect="1280x720"; Got="$($s.width)x$($s.height)"; Verdict=$(if($ok){"PASS"}else{"FAIL"}) })

# ═══ S4 BITRATE — 10200 kbps CBR lands in the stream ════════════════════════
$d = Join-Path $OutDir "cfg_s4"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $true -W 1920 -H 1080
$r = Invoke-Scenario -Name "S4_bitrate" -CfgDir $d
$s = Invoke-FFProbe -Mp4 $r.Mp4
$br = if ($s -and $s.bit_rate) { [double]$s.bit_rate } else { 0 }
$ok = ($br -ge 0.6 * 10200000) -and ($br -le 1.4 * 10200000)
$results.Add([pscustomobject]@{ S="S4"; Field="Bitrate(CBR)"; Expect="10.2 Mbps ±40% (low-motion dips = WARN)"; Got="$([math]::Round($br/1e6,2)) Mbps"; Verdict=$(if($ok){"PASS"}else{"FAIL"}) })

# ═══ S5 PRESET — encoder_preset=7 maps to p7 (echo evidence) ════════════════
$d = Join-Path $OutDir "cfg_s5"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $true -W 1920 -H 1080
$r = Invoke-Scenario -Name "S5_preset7" -CfgDir $d
$logText = Get-Content $r.Log -Raw
$ok = $logText -match "preset='p7'" -and $logText -match "preset p7"
$results.Add([pscustomobject]@{ S="S5"; Field="Preset"; Expect="startup echo preset='p7' + init echo 'preset p7'"; Got=$(if($logText -match "preset='(p\d)'"){$Matches[1]}else{"not found"}); Verdict=$(if($ok){"PASS"}else{"FAIL"}) })

# ═══ S6 ENCODER — h264_nvenc produces H.264 ═════════════════════════════════
$d = Join-Path $OutDir "cfg_s6"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $true -W 1920 -H 1080
$r = Invoke-Scenario -Name "S6_encoder" -CfgDir $d
$s = Invoke-FFProbe -Mp4 $r.Mp4
$ok = ($s -ne $null) -and ($s.codec_name -eq "h264")
$results.Add([pscustomobject]@{ S="S6"; Field="Encoder"; Expect="codec_name=h264 (NVENC)"; Got=$s.codec_name; Verdict=$(if($ok){"PASS"}else{"FAIL"}) })

# ═══ S7 GFXCAP — requested gfxcapture → documented GAP, ddagrab continues ═══
$d = Join-Path $OutDir "cfg_s7"
Write-ScenarioConfig -Dir $d -Fps 60 -BitrateKbps 10200 -Preset 7 -Native $true -W 1920 -H 1080 -CaptureMethod "gfxcapture"
$r = Invoke-Scenario -Name "S7_gfxcapture" -CfgDir $d
$logText = Get-Content $r.Log -Raw
$gapWarn = $logText -match "requested='gfxcapture'.*GAP"
$ddagrab = $logText -match "DdagrabBackend"
$ok = $gapWarn -and $ddagrab
$results.Add([pscustomobject]@{ S="S7"; Field="CaptureMethod(gfxcapture)"; Expect="GAP warning in log + actual=DdagrabBackend (never silent)"; Got=$(if($gapWarn){"GAP warn present, ddagrab ran"}else{"GAP warn MISSING"}); Verdict=$(if($ok){"PASS (gap honestly recorded)"}else{"FAIL"}) })

# ═══ report ═════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "==================== PHASE 1 VIDEO VALIDATION ===================="
$results | Format-Table -AutoSize | Out-String | Write-Host
$failed = @($results | Where-Object { $_.Verdict -like "FAIL*" }).Count
$verdict = if ($failed -eq 0) { "PASS" } else { "FAIL ($failed scenarios)" }
Write-Host "VERDICT: $verdict"

$report = New-Object System.Collections.Generic.List[string]
$report.Add("# PHASE 1 VIDEO — Windows real-record validation evidence")
$report.Add("")
$report.Add("- **Date:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$report.Add("- **Machine:** $env:COMPUTERNAME · OS: $([System.Environment]::OSVersion.VersionString)")
$report.Add("- **FFmpeg:** `$FFmpeg` · **Screen:** ${screenW}x${screenH} · **Seconds/scenario:** $Seconds")
$report.Add("- **Driver:** ConsoleDriver --videocheck (canonical chain LoadEffectiveSettings → MapStartupConfig → Initialize → BuildSessionConfig → StartSession)")
$report.Add("")
$report.Add("| S | Field | Expect | Got | Verdict |")
$report.Add("|---|---|---|---|---|")
foreach ($x in $results) { $report.Add("| $($x.S) | $($x.Field) | $($x.Expect) | $($x.Got) | $($x.Verdict) |") }
$report.Add("")
$report.Add("**VERDICT: $verdict**")
$report.Add("")
$report.Add("Known gaps (honest, pre-documented — NOT validation failures):")
$report.Add("- PixelFormat: nv12 requested but runtime is BGRA8→ARGB (BLOCKER P1-PIXFMT, RecordingEngine.vb:135-138)")
$report.Add("- gfxcapture: not implemented — GAP warning + ddagrab continuation (RecordingEngine.vb:123-128)")
$report.Add("")
Set-Content -Path (Join-Path $OutDir "report.md") -Value ($report -join "`r`n") -Encoding UTF8
Write-Host "Evidence: $(Join-Path $OutDir 'report.md')"

exit $(if ($failed -eq 0) { 0 } else { 1 })
