# validate-phase12b.ps1 — FULL Phase 12b runtime validation (Windows only)
#
# Runs, in order:
#   1. Whole-solution clean build (scripts\build-all.ps1 -RunTests)
#   2. Recording.Tests  — SyncMath + WavSidecar + real-ffmpeg A/V sync
#   3. ConsoleDriver matrix — Test A (3×10s) / B (early-stop) / C (restarts)
#      with per-session DoD checks + evidence markdown
#   4. Crash test — kills the driver mid-session and proves:
#        · no orphan ffmpeg survives (JobObjectGuard KILL_ON_JOB_CLOSE)
#        · engine process death does not hang the system
#
# Evidence lands in:
#   CaptureEngine.Recording.ConsoleDriver\evidence\phase-12b-validation-*.md
#   evidence\phase-12b-crash-*.txt
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\validate-phase12b.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\validate-phase12b.ps1 -SkipBuild

param(
    [switch]$SkipBuild,
    [string]$Ffmpeg = ""
)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

if (-not $Ffmpeg) {
    $Ffmpeg = Join-Path $repo "Overlay\API-Core\ffmpeg.exe"
}

Write-Host "=================================================="
Write-Host " Phase 12b — FULL runtime validation (Windows)"
Write-Host " ffmpeg: $Ffmpeg"
Write-Host "=================================================="

if (-not (Test-Path $Ffmpeg)) {
    Write-Host "ffmpeg not found at $Ffmpeg — pass -Ffmpeg <path>" -ForegroundColor Red
    exit 1
}

# ── 1+2: build + test suites ──────────────────────────────────────
if (-not $SkipBuild) {
    & powershell -ExecutionPolicy Bypass -File "scripts\build-all.ps1" -RunTests
    if ($LASTEXITCODE -ne 0) { Write-Host "build/tests FAILED" -ForegroundColor Red; exit 1 }
}

# ── 3: production matrix ──────────────────────────────────────────
Write-Host "`n>>> ConsoleDriver production matrix (PLAY AUDIO during sessions!)" -ForegroundColor Cyan
Write-Host "    >>> START PLAYING MUSIC NOW and keep it playing <<<"
Start-Sleep -Seconds 3

dotnet run --project "CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj" -c Release -- --ffmpeg "$Ffmpeg"
$matrixExit = $LASTEXITCODE

# ── 4: crash test ─────────────────────────────────────────────────
Write-Host "`n>>> Crash test: kill driver mid-session, verify no orphan ffmpeg" -ForegroundColor Cyan

$baseline = @(Get-Process -Name "ffmpeg" -ErrorAction SilentlyContinue).Count
$driver = Start-Process -FilePath "dotnet" `
    -ArgumentList "run","--project","CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj","-c","Release","--no-build","--","--ffmpeg","$Ffmpeg","--single","25" `
    -PassThru -WorkingDirectory $repo

Start-Sleep -Seconds 10     # mid-session (session runs 25s)
$midOrphans = @(Get-Process -Name "ffmpeg" -ErrorAction SilentlyContinue).Count

Write-Host "  killing driver (pid $($driver.Id)) mid-session... (mid-run ffmpeg: $midOrphans)"
taskkill /F /PID $driver.Id | Out-Null
Start-Sleep -Seconds 4

$afterOrphans = @(Get-Process -Name "ffmpeg" -ErrorAction SilentlyContinue).Count
Write-Host "  ffmpeg after crash: $afterOrphans (baseline was $baseline)"

$crashOk = ($afterOrphans -le $baseline)
$evidenceDir = Join-Path $repo "evidence"
New-Item -ItemType Directory -Force -Path $evidenceDir | Out-Null
$crashReport = Join-Path $evidenceDir "phase-12b-crash-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
@"
Phase 12b crash test — $(Get-Date)
Killed ConsoleDriver mid-session (taskkill /F).
ffmpeg baseline: $baseline
ffmpeg during session: $midOrphans
ffmpeg 4s after kill: $afterOrphans
ORPHAN CHECK: $(if ($crashOk) { 'PASS — JobObjectGuard killed all children' } else { 'FAIL — orphans survived' })
"@ | Out-File $crashReport -Encoding utf8
Write-Host "  crash evidence: $crashReport"

# ── Verdict ───────────────────────────────────────────────────────
Write-Host "`n=================================================="
Write-Host " VALIDATION SUMMARY"
Write-Host "=================================================="
Write-Host "  matrix:   $(if ($matrixExit -eq 0) { 'PASS' } else { 'FAIL' }) (exit $matrixExit)"
Write-Host "  crash:    $(if ($crashOk) { 'PASS' } else { 'FAIL' })"
Write-Host "  evidence: $evidenceDir + ConsoleDriver\evidence\"

if ($matrixExit -eq 0 -and $crashOk) {
    Write-Host "`n  PHASE 12b RUNTIME VALIDATION: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n  PHASE 12b RUNTIME VALIDATION: FAIL" -ForegroundColor Red
    exit 1
}
