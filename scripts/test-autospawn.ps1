# test-autospawn.ps1 — EngineProcessSupervisor runtime validation (Windows)
#
# Validates the GPT→GLM/5 checkpoint: Auto-spawn NVIDIA Capture.exe.
# Scenario matrix:
#   T1  Engine absent  → start ShadowPlay.exe → engine SPAWNS
#   T2  Engine running → ShadowPlay already up → NO duplicate (reuse)
#   T3  Engine killed  → monitor respawns it (backoff 3s)
#
# NOTE: This test launches the real ShadowPlay UI (it is a user-facing app).
# Close any running "NVIDIA ShadowPlay" instances first, or pass -KeepOpen
# to skip cleanup at the end.
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\test-autospawn.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\test-autospawn.ps1 -SkipBuild

param(
    [switch]$SkipBuild,
    [switch]$KeepOpen
)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo
$overlayBin = Join-Path $repo "Overlay\bin\Release\net8.0-windows10.0.26100.0"

$spExe  = Join-Path $overlayBin "NVIDIA ShadowPlay.exe"
$capExe = Join-Path $overlayBin "NVIDIA Capture.exe"

if (-not $SkipBuild) {
    Write-Host ">>> Building solution..." -ForegroundColor Cyan
    & powershell -ExecutionPolicy Bypass -File "scripts\build-all.ps1" -Fast
    if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED" -ForegroundColor Red; exit 1 }
}

if (-not (Test-Path $spExe))  { Write-Host "Missing $spExe" -ForegroundColor Red; exit 1 }
if (-not (Test-Path $capExe)) { Write-Host "Missing $capExe (engine exe must sit beside ShadowPlay.exe)" -ForegroundColor Red; exit 1 }

function Count-Engine { @(Get-Process -Name "NVIDIA Capture" -ErrorAction SilentlyContinue).Count }
function Count-Overlay { @(Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue).Count }

$pass = 0; $fail = 0
function Check($name, $cond) {
    if ($cond) { Write-Host "  [PASS] $name" -ForegroundColor Green; $script:pass++ }
    else       { Write-Host "  [FAIL] $name" -ForegroundColor Red;   $script:fail++ }
}

Write-Host ""
Write-Host "=================================================="
Write-Host " Auto-spawn validation — EngineProcessSupervisor"
Write-Host "=================================================="

# ── Clean slate ───────────────────────────────────────────────────
Write-Host "`n>>> Cleaning: closing any running ShadowPlay / Capture..." -ForegroundColor Cyan
Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "NVIDIA Capture"    -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Check "T0 baseline: no engine, no overlay" ((Count-Engine -eq 0) -and (Count-Overlay -eq 0))

# ── T1: start ShadowPlay → engine spawns ─────────────────────────
Write-Host "`n>>> T1: starting ShadowPlay.exe (expect engine auto-spawn)..." -ForegroundColor Cyan
Start-Process -FilePath $spExe -WorkingDirectory $overlayBin
Start-Sleep -Seconds 12          # spawn + engine init (D3D11/NVENC) + hub connect
$engineCount = Count-Engine
Check "T1 engine spawned after Overlay start ($engineCount process)" ($engineCount -ge 1)
Check "T1 no duplicate engines ($engineCount <= 1)" ($engineCount -le 1)

# ── T2: engine already running → Overlay keeps exactly one ───────
Write-Host "`n>>> T2: supervisor must reuse the running engine (no duplicate)..." -ForegroundColor Cyan
Start-Sleep -Seconds 5
$engineCount2 = Count-Engine
Check "T2 still exactly one engine ($engineCount2)" ($engineCount2 -eq 1)

# ── T3: kill engine → monitor respawns ───────────────────────────
Write-Host "`n>>> T3: killing engine (expect respawn within ~10s)..." -ForegroundColor Cyan
Get-Process -Name "NVIDIA Capture" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 12          # backoff 3s + poll 2s + engine init margin
$engineCount3 = Count-Engine
Check "T3 engine respawned after crash ($engineCount3 process)" ($engineCount3 -ge 1)
Check "T3 no duplicate after respawn ($engineCount3 -le 1)" ($engineCount3 -le 1)

# ── Verdict ───────────────────────────────────────────────────────
Write-Host ""
Write-Host "=================================================="
Write-Host " AUTOSPAWN RESULT: $pass passed / $fail failed"
Write-Host "=================================================="

if (-not $KeepOpen) {
    Write-Host "`n>>> Cleaning up test processes..." -ForegroundColor Cyan
    Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process -Name "NVIDIA Capture"    -ErrorAction SilentlyContinue | Stop-Process -Force
}

if ($fail -gt 0) { exit 1 } else { exit 0 }
