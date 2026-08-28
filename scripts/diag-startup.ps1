# diag-startup.ps1 — ShadowPlay.exe startup-crash bisect (Windows)
#
# Evidence so far (runs #1-#6):
#   - exe exits -1 (apphost StatusHostFailure) BEFORE managed code
#   - COREHOST TRACE dies right after selecting framework #1
#     (Microsoft.NETCore.App 8.0.30) — framework #2 (WindowsDesktop.App)
#     resolution lines never appear (or stderr truncation hides them)
#   - 'dotnet NVIDIA ShadowPlay.dll' runs fine
#   - sibling NVIDIA API.exe (same bin, same TFM, same frameworks) runs fine
#   - clean build changed nothing; not Defender; not runtime-install
#
# This script bisects the difference in ONE run:
#   R  Reference trace from NVIDIA API.exe (known-good) → shows what a
#      healthy framework-resolution phase looks like on THIS machine
#   B0 Trivial fresh 'dotnet new winforms' apphost → machine vs project
#   B1 Overlay rebuilt WITHOUT app.manifest (-p:ApplicationManifest=)
#   B2 Overlay rebuilt with short AssemblyName (-p:AssemblyName=SPTest)
#      (no space in exe name, different SingleInstance mutex identity)
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\diag-startup.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\diag-startup.ps1 -SkipRef   (skip R)

param([switch]$SkipRef)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo
$overlayBin = Join-Path $repo "Overlay\bin\Release\net10.0-windows10.0.26100.0"
$spExe  = Join-Path $overlayBin "NVIDIA ShadowPlay.exe"
$apiExe = Join-Path $overlayBin "NVIDIA API.exe"

Write-Host "=================================================="
Write-Host " ShadowPlay.exe startup-crash bisect"
Write-Host "=================================================="
try { & dotnet --info 2>$null | Select-String "Version:" | Select-Object -First 1 | ForEach-Object { Write-Host " SDK $($_.Line.Trim())" } } catch { }

# clean slate (B1/B2 launches start the real app → supervisor may spawn engine)
foreach ($n in @("NVIDIA ShadowPlay", "NVIDIA Capture", "NVIDIA API", "SPTest", "ScratchHost")) {
    Get-Process -Name $n -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}

# ── helper: launch an exe with COREHOST_TRACE + piped stderr ──────
function Trace-Launch([string]$Exe, [int]$AliveSec = 8, [string]$Tag = "") {
    if ($Tag -eq "") { $Tag = [IO.Path]::GetFileNameWithoutExtension($Exe) }
    $err = Join-Path $env:TEMP ("diag-" + $Tag + "-err.txt")
    $out = Join-Path $env:TEMP ("diag-" + $Tag + "-out.txt")
    foreach ($f in @($err, $out)) { if (Test-Path $f) { Remove-Item $f -Force } }
    $env:COREHOST_TRACE = "1"
    try {
        $p = Start-Process -FilePath $Exe -WorkingDirectory (Split-Path $Exe) -PassThru `
            -RedirectStandardError $err -RedirectStandardOutput $out
        if ($p.WaitForExit($AliveSec * 1000)) {
            return @{ Alive = $false; Code = $p.ExitCode; Err = $err }
        }
        try { $p | Stop-Process -Force } catch { }
        return @{ Alive = $true;  Code = $null;  Err = $err }
    } finally {
        Remove-Item Env:COREHOST_TRACE -ErrorAction SilentlyContinue
    }
}

# ── R: reference trace from known-good API.exe ────────────────────
if (-not $SkipRef) {
    Write-Host "`n>>> R: reference trace from NVIDIA API.exe (known-good)..." -ForegroundColor Cyan
    $r = Trace-Launch $apiExe 8 "APIref"
    if ($r.Alive) { Write-Host "  [OK] API.exe alive (healthy reference captured)" -ForegroundColor Green }
    else { Write-Host ("  [??] API.exe exited (code {0}) — reference may be short" -f $r.Code) -ForegroundColor Yellow }
    Write-Host "  trace saved: $($r.Err)"
    Write-Host "  ── healthy trace (framework phase) ──" -ForegroundColor DarkGray
    Get-Content $r.Err -ErrorAction SilentlyContinue |
        Where-Object { $_ -match "framework|FX version|Resolving|hostpolicy|coreclr|Starting" } |
        Select-Object -First 25 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
}

# ── re-trace the BROKEN exe for a same-run comparison ─────────────
Write-Host "`n>>> X: trace the broken ShadowPlay.exe (same conditions)..." -ForegroundColor Cyan
$x = Trace-Launch $spExe 8 "X"
if ($x.Alive) { Write-Host "  [?!] ShadowPlay.exe ALIVE this time (flaky?!)" -ForegroundColor Yellow }
else { Write-Host ("  [OK] reproduced: exit code {0}" -f $x.Code) -ForegroundColor Yellow }
Write-Host "  trace saved: $($x.Err)"

# ── B0: trivial fresh winforms apphost (machine vs project) ───────
Write-Host "`n>>> B0: trivial 'dotnet new winforms' scratch app (machine-level sanity)..." -ForegroundColor Cyan
$b0Dir = Join-Path $env:TEMP "diag-b0-scratch"
try {
    if (Test-Path $b0Dir) { Remove-Item $b0Dir -Recurse -Force }
    New-Item -ItemType Directory -Path $b0Dir | Out-Null
    Push-Location $b0Dir
    & dotnet new winforms -n ScratchHost --force 2>&1 | Out-Null
    & dotnet build ScratchHost -c Release -v q --nologo 2>&1 | Out-Null
    $b0exe = Join-Path $b0Dir "ScratchHost\bin\Release\net10.0-windows\ScratchHost.exe"
    if (Test-Path $b0exe) {
        $b0 = Trace-Launch $b0exe 6 "B0"
        if ($b0.Alive) { Write-Host "  [PASS] trivial winforms apphost RUNS → machine OK, problem is Overlay-specific" -ForegroundColor Green }
        else { Write-Host ("  [FAIL] even a fresh trivial winforms apphost dies (exit {0}) → MACHINE-level issue" -f $b0.Code) -ForegroundColor Red }
        try { Get-Process -Name ScratchHost -ErrorAction SilentlyContinue | Stop-Process -Force } catch { }
    } else { Write-Host "  [SKIP] scratch build failed" -ForegroundColor Yellow }
} finally { Pop-Location }

# ── B1: Overlay WITHOUT app.manifest ──────────────────────────────
Write-Host "`n>>> B1: Overlay rebuilt WITHOUT app.manifest..." -ForegroundColor Cyan
$b1out = Join-Path $repo "Overlay\bin\DiagB1"
try {
    & dotnet build "Overlay\NVIDIA Overlay.vbproj" -c Release -v q --nologo `
        -p:ApplicationManifest= -p:OutputPath=bin\DiagB1\ 2>&1 |
        Select-String "error" | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    $b1exe = Join-Path $b1out "NVIDIA ShadowPlay.exe"
    if (Test-Path $b1exe) {
        # Guard: verify the override actually removed the manifest (it costs ~1-2KB)
        $sizeA = (Get-Item $spExe).Length
        $sizeB = (Get-Item $b1exe).Length
        if ([Math]::Abs($sizeA - $sizeB) -lt 200) {
            Write-Host "  [SKIP] B1 exe size == original ($sizeA vs $sizeB) — the -p:ApplicationManifest= override did NOT take; result would be misleading" -ForegroundColor Yellow
        } else {
            Write-Host ("  exe size: {0:N0} → {1:N0} (manifest removed)" -f $sizeA, $sizeB) -ForegroundColor DarkGray
            $b1 = Trace-Launch $b1exe 8 "B1"
            if ($b1.Alive) {
                Write-Host "  [PASS] NO-MANIFEST exe RUNS → manifest embedding is the culprit" -ForegroundColor Green
            } else {
                Write-Host ("  [FAIL] no-manifest exe still dies (exit {0}) → not the manifest" -f $b1.Code) -ForegroundColor Yellow
            }
        }
        try { Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue | Stop-Process -Force } catch { }
    } else { Write-Host "  [SKIP] B1 build produced no exe" -ForegroundColor Yellow }
} catch { Write-Host "  [SKIP] B1 build threw: $($_.Exception.Message)" -ForegroundColor Yellow }

# ── B2: Overlay with short AssemblyName (no space, fresh mutex) ───
Write-Host "`n>>> B2: Overlay rebuilt as 'SPTest' (short name, no space)..." -ForegroundColor Cyan
$b2out = Join-Path $repo "Overlay\bin\DiagB2"
try {
    & dotnet build "Overlay\NVIDIA Overlay.vbproj" -c Release -v q --nologo `
        -p:AssemblyName=SPTest -p:OutputPath=bin\DiagB2\ 2>&1 |
        Select-String "error" | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    $b2exe = Join-Path $b2out "SPTest.exe"
    if (Test-Path $b2exe) {
        $b2 = Trace-Launch $b2exe 8 "B2"
        if ($b2.Alive) {
            Write-Host "  [PASS] renamed exe RUNS → name/space/mutex-identity is the culprit" -ForegroundColor Green
        } else {
            Write-Host ("  [FAIL] renamed exe still dies (exit {0}) → not the name" -f $b2.Code) -ForegroundColor Yellow
        }
        try { Get-Process -Name SPTest -ErrorAction SilentlyContinue | Stop-Process -Force } catch { }
    } else { Write-Host "  [SKIP] B2 build produced no exe" -ForegroundColor Yellow }
} catch { Write-Host "  [SKIP] B2 build threw: $($_.Exception.Message)" -ForegroundColor Yellow }

# ── Summary ────────────────────────────────────────────────────────
foreach ($n in @("NVIDIA ShadowPlay", "NVIDIA Capture", "SPTest", "ScratchHost")) {
    Get-Process -Name $n -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}
Write-Host "`n=================================================="
Write-Host " Trace files (paste these back if asked):"
Write-Host "   broken : $($x.Err)"
if (-not $SkipRef) { Write-Host "   healthy: $($r.Err)" }
Write-Host "=================================================="
Write-Host " Cleanup hint: Overlay\bin\DiagB1 / DiagB2 can be deleted anytime."
