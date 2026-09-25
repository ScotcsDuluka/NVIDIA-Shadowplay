# diag-recording.ps1 — deep-dive a recorded MP4 + the engine log that made it
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\diag-recording.ps1 "C:\path\to\recording.mp4"
#
# Prints:
#   - video stream: duration, frame count, avg fps, codec
#   - audio stream: duration, sample rate, channels
#   - A/V duration delta (the smoking gun for pacing bugs)
#   - first 3 packet PTS of each stream (audio should start ≈ 0)
#   - the engine's last-session log lines (wrap fps, offset, dup counts)
# Paste EVERYTHING back to GLM/5.

param([Parameter(Mandatory=$true)][string]$Mp4)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
$ffprobe = Join-Path $repo "Overlay\API-Core\ffprobe.exe"
if (-not (Test-Path $ffprobe)) { $ffprobe = "ffprobe" }
if (-not (Test-Path $Mp4)) { Write-Host "File not found: $Mp4" -ForegroundColor Red; exit 1 }

function Probe([string]$Args) {
    & $ffprobe -v error $Args 2>$null
}

Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " FILE: $Mp4"
Write-Host " size: $((Get-Item $Mp4).Length.ToString('N0')) bytes"
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan

# ── container overview ─────────────────────────────────────────────
Write-Host "`n── streams ──" -ForegroundColor Cyan
Probe "-show_entries stream=index,codec_type,codec_name,duration,avg_frame_rate,r_frame_rate,nb_frames,width,height,sample_rate,channels -of default=noprint_wrappers=1 ""$Mp4"""

# ── per-stream durations (precise) ─────────────────────────────────
$vDur = Probe "-select_streams v:0 -show_entries stream=duration -of csv=p=0 ""$Mp4"""
$aDur = Probe "-select_streams a:0 -show_entries stream=duration -of csv=p=0 ""$Mp4"""
$vFps = Probe "-select_streams v:0 -show_entries stream=avg_frame_rate -of csv=p=0 ""$Mp4"""
$vFrames = Probe "-select_streams v:0 -show_entries stream=nb_frames -of csv=p=0 ""$Mp4"""

Write-Host "`n── measurement ──" -ForegroundColor Cyan
Write-Host ("  video duration : {0}" -f $vDur)
Write-Host ("  audio duration : {0}" -f $aDur)
Write-Host ("  video avg fps  : {0}   (frames: {1})" -f $vFps, $vFrames)

if ($vDur -and $aDur) {
    $delta = ([double]$vDur - [double]$aDur) * 1000.0
    Write-Host ("  A/V duration delta : {0:N0} ms   ({1})" -f $delta, $(If ($delta -gt 0) { "video LONGER than audio" } ElseIf ($delta -lt 0) { "audio LONGER than video" } Else { "equal" }))
    Write-Host "  (|delta| < ~200ms is normal mux clamp; large delta = pacing bug)" -ForegroundColor DarkGray
}

# ── first/last packet timestamps ───────────────────────────────────
Write-Host "`n── first packets (pts_time) ──" -ForegroundColor Cyan
$vPts = Probe "-select_streams v:0 -read_intervals %+3 -show_entries packet=pts_time -of csv=p=0 ""$Mp4""" | Select-Object -First 4
$aPts = Probe "-select_streams a:0 -read_intervals %+1 -show_entries packet=pts_time -of csv=p=0 ""$Mp4""" | Select-Object -First 4
Write-Host ("  video: {0}" -f ($vPts -join ", "))
Write-Host ("  audio: {0}" -f ($aPts -join ", "))

# ── the engine log that produced this file ─────────────────────────
$engineLog = Join-Path $repo "OverlaybinRelease
et10.0-windows10.0.26100.0\Logs\ui-engine.log"
Write-Host "`n── engine log: last session's key lines ──" -ForegroundColor Cyan
if (Test-Path $engineLog) {
    $lines = Get-Content $engineLog -ErrorAction SilentlyContinue
    $start = -1
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        if ($lines[$i] -match "Starting audio sidecar") { $start = $i; break }
    }
    if ($start -ge 0) {
        $lines | Select-Object -Skip $start |
            Where-Object { $_ -match "session|Sync|Wrapping|Result|CFR|fps|dup|Audio|Mic|offset|Mux" } |
            Select-Object -First 40 | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow }
    } else {
        Write-Host "  (no 'Starting audio sidecar' found in log)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  (engine log not found at $engineLog)" -ForegroundColor Yellow
}

Write-Host "`n>>> Paste this ENTIRE output back to GLM/5." -ForegroundColor Green
