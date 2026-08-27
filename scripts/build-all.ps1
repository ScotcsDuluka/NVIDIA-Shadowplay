# build-all.ps1 — Phase 12b whole-solution build (Windows)
#
# Follows docs/BUILD_PROTOCOL.md: CLEAN FIRST, then build, then optionally
# run every test suite. Stale DLLs are the #1 cause of false-positive bugs.
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\build-all.ps1              # build only
#   powershell -ExecutionPolicy Bypass -File scripts\build-all.ps1 -RunTests    # build + tests
#   powershell -ExecutionPolicy Bypass -File scripts\build-all.ps1 -Fast        # skip clean
#   powershell -ExecutionPolicy Bypass -File scripts\build-all.ps1 -StageLayout # ALSO stage the
#       # root-fixed "NVIDIA ShadowPlay" tree into dist\ (see docs/APP-LAYOUT.md):
#       # Application\ hosts -> Services\ dlls, Engine\ libs, FFmpeg\, Config\,
#       # Languages\, Data\, Flags\... Runtime assembly resolution is handled by
#       # Common/AppLayout.vb compiled into every app.

param(
    [switch]$RunTests,
    [switch]$Fast,
    [switch]$StageLayout
)

$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

Write-Host "=================================================="
Write-Host " Phase 12b — Build whole solution"
Write-Host " repo: $repo"
Write-Host "=================================================="

# ── 1. Clean (BUILD_PROTOCOL: prevents stale DLLs) ────────────────
if (-not $Fast) {
    Write-Host "`n>>> Killing app family (a running NVIDIA app locks DLLs and breaks clean)..." -ForegroundColor Cyan
    # ★ Root-cause lesson: HandleAppsSmart in the hub spawns/keeps the family
    # alive; any running instance (incl. Diag builds) locks bin DLLs →
    # 'Access denied' during Remove-Item. Kill FIRST, always, before clean.
    foreach ($n in @("NVIDIA ShadowPlay", "NVIDIA Capture", "NVIDIA API",
                     "NVIDIA Experience", "NVIDIA Notifier", "SPTest", "SPRename9", "ScratchHost",
                     "ffmpeg", "ffprobe", "ffplay")) {
        Get-Process -Name $n -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2

    Write-Host ">>> Cleaning bin/obj for all projects..." -ForegroundColor Cyan
    # Diag/scratch outputs from startup-crash investigation are one-shot
    # artifacts — delete them outright so clean never trips on them again.
    foreach ($d in @("Overlay\bin\DiagB1", "Overlay\bin\DiagB2", "Overlay\bin\DiagE")) {
        if (Test-Path (Join-Path $repo $d)) {
            Remove-Item -Recurse -Force (Join-Path $repo $d) -ErrorAction SilentlyContinue
        }
    }
    Get-ChildItem -Directory -Filter "CaptureEngine*" | ForEach-Object {
        foreach ($d in "bin", "obj") {
            $p = Join-Path $_.FullName $d
            if (Test-Path $p) { Remove-Item -Recurse -Force $p }
        }
    }
    foreach ($proj in "Engine", "Overlay", "API", "Notifier", "App Experience") {
        foreach ($d in "bin", "obj") {
            $p = Join-Path $repo "$proj\$d"
            if (Test-Path $p) { Remove-Item -Recurse -Force $p }
        }
    }
}

# ── 2. Build solution (now contains ALL projects) ─────────────────
Write-Host "`n>>> Building Overlay/NVIDIA Overlay.sln (Release)..." -ForegroundColor Cyan
dotnet build "Overlay\NVIDIA Overlay.sln" -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED" -ForegroundColor Red
    exit 1
}
Write-Host "BUILD OK" -ForegroundColor Green

# ── 3. Timestamp sanity (BUILD_PROTOCOL) ───────────────────────────
$dll = "Overlay\bin\Release\net8.0-windows10.0.26100.0\NVIDIA Capture.dll"
if (Test-Path $dll) {
    Write-Host "`n>>> Runtime DLL: $dll"
    Write-Host "    timestamp: $((Get-Item $dll).LastWriteTime)  (must be 'now')"
}

# ── 4. Tests ──────────────────────────────────────────────────────
if ($RunTests) {
    Write-Host "`n>>> Running test suites..." -ForegroundColor Cyan
    $suites = @(
        "CaptureEngine.Tests",
        "CaptureEngine.FFmpegTests",
        "CaptureEngine.FrameContractTests",
        "CaptureEngine.ConfigTests",
        "CaptureEngine.Encoder.Tests",
        "CaptureEngine.Video.Tests",
        # Phase 12b: SyncMath + sidecar + real-ffmpeg sync
        "CaptureEngine.Recording.Tests"
    )
    $failed = @()
    foreach ($s in $suites) {
        Write-Host "`n──── $s ────" -ForegroundColor Yellow
        dotnet run --project "$s\$s.vbproj" -c Release --no-build
        if ($LASTEXITCODE -ne 0) { $failed += $s }
    }
    if ($failed.Count -gt 0) {
        Write-Host "`nTEST SUITES FAILED: $($failed -join ', ')" -ForegroundColor Red
        exit 2
    }
    Write-Host "`nALL TEST SUITES PASSED" -ForegroundColor Green
}

Write-Host "`nDONE." -ForegroundColor Green

# ── 5. Stage the root-fixed layout (dist\NVIDIA ShadowPlay) ────────
if ($StageLayout) {
    Write-Host "`n>>> STAGING root-fixed layout..." -ForegroundColor Cyan
    dotnet msbuild "scripts\layout.proj" -p:Configuration=Release -nologo
    if ($LASTEXITCODE -ne 0) {
        Write-Host "LAYOUT STAGE FAILED" -ForegroundColor Red
        exit 3
    }
    Write-Host "LAYOUT STAGED OK -> $(Join-Path $repo 'dist\NVIDIA ShadowPlay')" -ForegroundColor Green
}

Write-Host "`nALL DONE." -ForegroundColor Green
