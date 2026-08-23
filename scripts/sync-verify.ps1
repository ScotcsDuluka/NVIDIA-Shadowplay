# sync-verify.ps1 — TRUE end-to-end A/V sync measurement (ms-level)
#
# WHY: SessionResult.SystemOffsetSec reports the offset we MEASURED and
# compensated — it is not proof the final file is aligned. The only honest
# number is a physical reference event captured by the whole pipeline:
# a screen flash (video path) + a beep (audio path) emitted at the SAME
# instant, recorded through ShadowPlay, then measured in the OUTPUT file.
#
#   flash onset  : video path → D3D11 → NVENC → MP4
#   beep onset   : audio path → WASAPI → sidecar → mux → MP4
#   sync error   = beep_time - flash_time  (per event)
#                  > 0 → audio leads, < 0 → audio lags
#
# Usage:
#   1) Generate the reference video (once):
#        powershell -File scripts\sync-verify.ps1 -MakeReference
#   2) Play sync-reference.mp4 FULLSCREEN in a player; record ~30s with
#      ShadowPlay (normal recording, audio on).
#   3) Measure the recording:
#        powershell -File scripts\sync-verify.ps1 -Measure "path\to\recording.mp4"
#
# Interpretation:
#   |mean| <= 17ms  (1 frame @60fps) → frame-accurate; at the practical floor
#   |mean|  17-45ms                    → audible-visible threshold territory
#   |mean| > 45ms                      → real misalignment; investigate
#   A CONSISTENT mean (low spread) = systematic bias → fixable with one
#   calibrated constant if OWNER approves. High spread = jitter → engine issue.

param(
    [switch]$MakeReference,
    [string]$Measure,
    [int]$Seconds = 30,
    [int]$Interval = 3
)

$ErrorActionPreference = "Continue"

function Find-Ffmpeg {
    $c = @("Overlay\API-Core\ffmpeg.exe", "$env:TEMP\ffmpeg.exe")
    $repo = Split-Path -Parent $PSScriptRoot
    foreach ($p in @((Join-Path $repo "Overlay\API-Core\ffmpeg.exe"))) {
        if (Test-Path $p) { return $p }
    }
    $inPath = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if ($inPath) { return $inPath.Source }
    Write-Host "ffmpeg not found (Overlay\API-Core\ffmpeg.exe or PATH)" -ForegroundColor Red
    exit 1
}

$ffmpeg = Find-Ffmpeg

# ── Mode 1: generate the reference video ──────────────────────────
if ($MakeReference) {
    $out = Join-Path (Get-Location) "sync-reference.mp4"
    Write-Host ">>> Generating $out ($Seconds s, flash+beep every $Interval s)..." -ForegroundColor Cyan
    & $ffmpeg -y -hide_banner -loglevel error `
        -f lavfi -i "color=black:size=1280x720:rate=60:duration=$Seconds" `
        -f lavfi -i "sine=frequency=1000:sample_rate=48000:duration=$Seconds" `
        -filter_complex "[0:v]drawbox=x=0:y=0:w=iw:h=ih:color=white:t=fill:enable='lt(mod(t,$Interval),0.1)'[v];[1:a]volume=0:enable='gte(mod(t,$Interval),0.2)'[a]" `
        -map "[v]" -map "[a]" -c:v libx264 -preset fast -pix_fmt yuv420p -c:a aac -shortest $out
    if (Test-Path $out) {
        Write-Host "DONE: $out" -ForegroundColor Green
        Write-Host "Play it FULLSCREEN, record ~$Seconds seconds with ShadowPlay, then run:" -ForegroundColor Cyan
        Write-Host "  powershell -File scripts\sync-verify.ps1 -Measure <recording.mp4>" -ForegroundColor Cyan
    } else { Write-Host "Generation FAILED" -ForegroundColor Red; exit 1 }
    exit 0
}

# ── Mode 2: measure a recording ───────────────────────────────────
if ($Measure -eq "") {
    Write-Host "Usage: -MakeReference  |  -Measure <recorded.mp4>" -ForegroundColor Yellow
    exit 1
}
if (-not (Test-Path $Measure)) { Write-Host "File not found: $Measure" -ForegroundColor Red; exit 1 }

Write-Host ">>> Analyzing $Measure ..." -ForegroundColor Cyan

# Flash onsets: big scene changes (black→white = scene score near 1.0).
$showinfo = & $ffmpeg -hide_banner -i $Measure -vf "select='gt(scene,0.35)',showinfo" -f null - 2>&1 | Out-String
$flashTimes = @()
foreach ($m in [regex]::Matches($showinfo, "pts_time:(\d+\.?\d*)")) {
    $flashTimes += [double]$m.Groups[1].Value
}

# Beep onsets: silence→sound transitions (silence_end = beep start).
$silence = & $ffmpeg -hide_banner -i $Measure -af "silencedetect=noise=-35dB:d=0.1" -f null - 2>&1 | Out-String
$beepTimes = @()
foreach ($m in [regex]::Matches($silence, "silence_end:\s*(\d+\.?\d*)")) {
    $beepTimes += [double]$m.Groups[1].Value
}
# EOF flush: drop a silence_end that coincides with the file end.
$durOut = & $ffmpeg -hide_banner -i $Measure -f null - 2>&1 | Out-String
$durMatch = [regex]::Match($durOut, "time=(\d+\.?\d*)")
if ($durMatch.Success -and $beepTimes.Count -gt 0) {
    $dur = [double]$durMatch.Groups[1].Value
    $beepTimes = @($beepTimes | Where-Object { [Math]::Abs($_ - $dur) -gt 0.5 })
}

Write-Host ("  flash events (video): {0}" -f $flashTimes.Count)
Write-Host ("  beep  events (audio): {0}" -f $beepTimes.Count)

if ($flashTimes.Count -eq 0 -or $beepTimes.Count -eq 0) {
    Write-Host "Not enough reference events detected." -ForegroundColor Red
    Write-Host "  (Did the recording contain the fullscreen flash+beep video? Was audio enabled?)" -ForegroundColor Yellow
    exit 2
}

# Pair each beep with the NEAREST flash (within 0.5s window).
$deltas = @()
Write-Host ""
Write-Host "  event   flash(s)    beep(s)    delta(ms)   audio"
Write-Host "  ─────   ─────────   ────────   ─────────   ─────"
foreach ($b in $beepTimes) {
    $best = $null; $bestDiff = 999.0
    foreach ($f in $flashTimes) {
        $d = [Math]::Abs($b - $f)
        if ($d -lt $bestDiff) { $bestDiff = $d; $best = $f }
    }
    if ($best -ne $null -and $bestDiff -le 0.5) {
        $deltaMs = ($b - $best) * 1000.0
        $deltas += $deltaMs
        $note = If ($deltaMs -gt 0) { "LEADS video" } elseif ($deltaMs -lt 0) { "LAGS video" } else { "aligned" }
        Write-Host ("  {0,4}    {1,9:F3}  {2,8:F3}  {3,9:F1}   {4}" -f $deltas.Count, $best, $b, $deltaMs, $note)
    }
}

if ($deltas.Count -eq 0) {
    Write-Host "No flash/beep pairs matched within 0.5s." -ForegroundColor Red
    exit 2
}

$mean = ($deltas | Measure-Object -Average).Average
$min = ($deltas | Measure-Object -Minimum).Minimum
$max = ($deltas | Measure-Object -Maximum).Maximum
$spread = $max - $min

Write-Host ""
Write-Host "══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host (" SYNC ERROR (audio relative to video), {0} events" -f $deltas.Count)
Write-Host ("   mean : {0,8:F1} ms   {1}" -f $mean, $(If ($mean -gt 0) { "audio leads" } ElseIf ($mean -lt 0) { "audio lags" } Else { "aligned" }))
Write-Host ("   min  : {0,8:F1} ms" -f $min)
Write-Host ("   max  : {0,8:F1} ms" -f $max)
Write-Host ("   spread: {0,7:F1} ms   (low = consistent bias, high = jitter)" -f $spread)
Write-Host "══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
if ([Math]::Abs($mean) -le 17) {
    Write-Host " VERDICT: within 1 frame @60fps — frame-accurate (practical floor)." -ForegroundColor Green
} elseif ([Math]::Abs($mean) -le 45) {
    Write-Host " VERDICT: small but above 1 frame — consistent mean could be calibrated (ask GLM/5)." -ForegroundColor Yellow
} else {
    Write-Host " VERDICT: audible/visible territory — report the numbers above to GLM/5." -ForegroundColor Red
}
