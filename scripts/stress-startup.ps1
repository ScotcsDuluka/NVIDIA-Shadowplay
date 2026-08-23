# stress-startup.ps1 — statistical isolation of the ShadowPlay.exe startup killer
#
# Run #7 (bisect) evidence:
#   - B1 (no-manifest exe): RAN → manifest is A suspect
#   - X (with-manifest exe): RAN TOO → the failure is FLAKY, single runs prove nothing
#   - Cross-referencing ALL 7 runs so far: every DEATH had the hub (NVIDIA API.exe)
#     running; every SURVIVAL of the real exe had the hub DOWN.
#     → TWO confounded variables: (manifest?) × (hub?)
#
# This script fires N launches per cell of the 2×2 matrix:
#   A: manifest exe + hub UP      B: no-manifest exe + hub UP
#   C: manifest exe + hub DOWN    D: no-manifest exe + hub DOWN
# …and prints the verdict logic. Plus zero-cost evidence up front:
#   - broad event-log scan (any provider mentioning ShadowPlay, 24h) — catches
#     SxS/manifest and WER events the narrow .NET-only filter missed
#   - the tail of the SUCCESSFUL with-manifest trace (diag-X-err.txt) for reference
#   - optional E: renamed build (space-in-name hypothesis), only if A has deaths
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\stress-startup.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\stress-startup.ps1 -Iterations 6

param([int]$Iterations = 10)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo
$overlayBin = Join-Path $repo "Overlay\bin\Release\net8.0-windows10.0.26100.0"
$spExe   = Join-Path $overlayBin "NVIDIA ShadowPlay.exe"
$hubExe  = Join-Path $overlayBin "NVIDIA API.exe"
$b1Dir   = Join-Path $repo "Overlay\bin\DiagB1"
$b1Exe   = Join-Path $b1Dir "NVIDIA ShadowPlay.exe"
if ($Iterations -lt 3) { $Iterations = 3 }

Write-Host "=================================================="
Write-Host " ShadowPlay.exe startup killer — 2x2 stress matrix"
Write-Host " $Iterations launches per cell"
Write-Host "=================================================="

foreach ($f in @($spExe, $hubExe)) {
    if (-not (Test-Path $f)) { Write-Host "Missing $f" -ForegroundColor Red; exit 1 }
}

# Rebuild B1 (no-manifest) if the bisect's output is gone
if (-not (Test-Path $b1Exe)) {
    Write-Host "`n>>> Rebuilding no-manifest variant (bin\DiagB1)..." -ForegroundColor Cyan
    & dotnet build "Overlay\NVIDIA Overlay.vbproj" -c Release -v q --nologo `
        -p:ApplicationManifest= -p:OutputPath=bin\DiagB1\ 2>&1 |
        Select-String "error" | Select-Object -First 3 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    if (-not (Test-Path $b1Exe)) { Write-Host "B1 rebuild failed — aborting" -ForegroundColor Red; exit 1 }
}

# ── helpers ────────────────────────────────────────────────────────
function Test-Port5000 {
    $c = New-Object System.Net.Sockets.TcpClient
    try { $c.Connect("127.0.0.1", 5000); $c.Close(); return $true } catch { return $false }
}

function Set-Hub([bool]$Up) {
    if ($Up) {
        if (-not (Test-Port5000)) {
            Start-Process -FilePath $hubExe -WorkingDirectory $overlayBin | Out-Null
            $deadline = (Get-Date).AddSeconds(20)
            while ((Get-Date) -lt $deadline -and -not (Test-Port5000)) { Start-Sleep -Milliseconds 400 }
        }
        if (Test-Port5000) { Write-Host "  [hub] UP (port 5000)" -ForegroundColor DarkGray }
        else { Write-Host "  [hub] FAILED TO START" -ForegroundColor Red; exit 2 }
    } else {
        Get-Process -Name "NVIDIA API" -ErrorAction SilentlyContinue | Stop-Process -Force
        $deadline = (Get-Date).AddSeconds(10)
        while ((Get-Date) -lt $deadline -and (Test-Port5000)) { Start-Sleep -Milliseconds 400 }
        if (Test-Port5000) { Write-Host "  [hub] port 5000 STILL BUSY — D config invalid!" -ForegroundColor Red }
        else { Write-Host "  [hub] DOWN (port free)" -ForegroundColor DarkGray }
    }
}

function Kill-TestApps {
    foreach ($n in @("NVIDIA ShadowPlay", "NVIDIA Capture", "SPRename9")) {
        Get-Process -Name $n -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

<# One cell: N plain launches (no redirect — faithful to double-click/T1),
   alive-check at 3.5s. On FIRST death, one relaunch with COREHOST_TRACE to
   capture evidence. Returns death count. #>
function Invoke-Cell([string]$Name, [string]$Exe, [int]$N) {
    Write-Host "`n>>> CELL $Name : $(Split-Path $Exe -Parent | Split-Path -Leaf)\$(Split-Path $Exe -Leaf) x$N" -ForegroundColor Cyan
    $deaths = 0; $firstDeathTraceTaken = $false
    for ($i = 1; $i -le $N; $i++) {
        Kill-TestApps
        Start-Sleep -Milliseconds 800
        $p = Start-Process -FilePath $Exe -WorkingDirectory (Split-Path $Exe) -PassThru
        Start-Sleep -Milliseconds 3500
        if ($p.HasExited) {
            $deaths++
            Write-Host ("    #{0}: DEAD (exit {1})" -f $i, $p.ExitCode) -ForegroundColor Red
            if (-not $firstDeathTraceTaken) {
                $firstDeathTraceTaken = $true
                Write-Host "    → capturing COREHOST trace of a relaunch..." -ForegroundColor DarkGray
                $err = Join-Path $env:TEMP "stress-$Name-trace.txt"
                $env:COREHOST_TRACE = "1"
                try {
                    $p2 = Start-Process -FilePath $Exe -WorkingDirectory (Split-Path $Exe) -PassThru `
                        -RedirectStandardError $err -RedirectStandardOutput (Join-Path $env:TEMP "stress-$Name-out.txt")
                    if ($p2.WaitForExit(6000)) {
                        Write-Host ("      relaunch DIED under trace (exit {0}) — tail:" -f $p2.ExitCode) -ForegroundColor DarkYellow
                        Get-Content $err -ErrorAction SilentlyContinue | Select-Object -Last 12 |
                            ForEach-Object { Write-Host "        $_" -ForegroundColor DarkYellow }
                    } else {
                        Write-Host "      relaunch SURVIVED under trace (flaky) — no trace of death this time" -ForegroundColor DarkGray
                        try { $p2 | Stop-Process -Force } catch { }
                    }
                } finally { Remove-Item Env:COREHOST_TRACE -ErrorAction SilentlyContinue }
            }
        } else {
            Write-Host ("    #{0}: alive" -f $i) -ForegroundColor Green
        }
    }
    Kill-TestApps
    return $deaths
}

# ── zero-cost evidence first ───────────────────────────────────────
Write-Host "`n>>> EVIDENCE: event log (24h) — ANY provider mentioning 'ShadowPlay'..." -ForegroundColor Cyan
try {
    $ev = Get-WinEvent -FilterHashtable @{LogName="Application"; StartTime=(Get-Date).AddDays(-1)} -ErrorAction SilentlyContinue |
        Where-Object { $_.Message -match "ShadowPlay" } | Select-Object -First 8
    $ev2 = Get-WinEvent -FilterHashtable @{LogName="System"; StartTime=(Get-Date).AddDays(-1)} -ErrorAction SilentlyContinue |
        Where-Object { $_.Message -match "ShadowPlay" } | Select-Object -First 5
    $all = @($ev) + @($ev2)
    if ($all.Count -gt 0) {
        foreach ($e in $all) {
            Write-Host ("  [{0}] {1} (id {2})" -f $e.TimeCreated, $e.ProviderName, $e.Id) -ForegroundColor Yellow
            ($e.Message -split "`r?`n") | Select-Object -First 3 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
        }
    } else { Write-Host "  (no events mention ShadowPlay in 24h)" -ForegroundColor DarkGray }
} catch { Write-Host "  (event query failed: $($_.Exception.Message))" -ForegroundColor DarkGray }

$xTrace = Join-Path $env:TEMP "diag-X-err.txt"
if (Test-Path $xTrace) {
    Write-Host "`n>>> EVIDENCE: successful with-manifest trace (diag X) — tail for reference:" -ForegroundColor Cyan
    Get-Content $xTrace | Select-Object -Last 10 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
}

# ── the matrix (hub UP cells first, then DOWN) ─────────────────────
Kill-TestApps
Set-Hub $true
$a = Invoke-Cell "A-manifest-hubUP"   $spExe $Iterations
$b = Invoke-Cell "B-nomanifest-hubUP" $b1Exe $Iterations
Set-Hub $false
$c = Invoke-Cell "C-manifest-hubDOWN"   $spExe $Iterations
$d = Invoke-Cell "D-nomanifest-hubDOWN" $b1Exe $Iterations

# ── optional E: renamed build (space hypothesis) if A has deaths ──
$eDeaths = -1
if ($a -gt 0) {
    Write-Host "`n>>> E: renamed build 'SPRename9' (no space, WITH manifest) x3 ..." -ForegroundColor Cyan
    try {
        & dotnet build "Overlay\NVIDIA Overlay.vbproj" -c Release -v q --nologo `
            -p:AssemblyName=SPRename9 -p:OutputPath=bin\DiagE\ -p:BaseIntermediateOutputPath=obj\DiagE\ 2>&1 |
            Select-String "error" | Select-Object -First 3 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        $eexe = Join-Path $repo "Overlay\bin\DiagE\SPRename9.exe"
        if (Test-Path $eexe) {
            Set-Hub $true
            $eDeaths = Invoke-Cell "E-renamed-hubUP" $eexe 3
            Set-Hub $false
        } else { Write-Host "  [SKIP] E build produced no exe" -ForegroundColor Yellow }
    } catch { Write-Host "  [SKIP] E threw: $($_.Exception.Message)" -ForegroundColor Yellow }
}

# ── verdict ────────────────────────────────────────────────────────
Write-Host "`n=================================================="
Write-Host " MATRIX RESULTS (deaths / $Iterations)"
Write-Host "=================================================="
Write-Host ("  A  manifest   + hub UP  : {0}/{1}" -f $a, $Iterations)
Write-Host ("  B  no-manifest + hub UP : {0}/{1}" -f $b, $Iterations)
Write-Host ("  C  manifest   + hub DOWN: {0}/{1}" -f $c, $Iterations)
Write-Host ("  D  no-manifest + hub DOWN:{0}/{1}" -f $d, $Iterations)
if ($eDeaths -ge 0) { Write-Host ("  E  renamed     + hub UP  : {0}/3" -f $eDeaths) }

Write-Host "`n  VERDICT LOGIC:"
if ($a -eq $Iterations -and $b -eq 0 -and $c -eq 0) {
    Write-Host "  → MANIFEST + HUB INTERACTION (dies only when BOTH present)" -ForegroundColor Magenta
} elseif ($a -eq $Iterations -and $b -eq 0 -and $c -eq $Iterations) {
    Write-Host "  → MANIFEST IS THE KILLER (dies whenever manifest present, hub irrelevant)" -ForegroundColor Magenta
} elseif ($a -eq $Iterations -and $b -eq $Iterations -and $c -eq 0) {
    Write-Host "  → HUB INTERACTION (dies whenever hub is up, manifest irrelevant)" -ForegroundColor Magenta
} elseif ($a -eq 0) {
    Write-Host "  → NOT REPRODUCIBLE in A anymore — flaky; report counts for interpretation" -ForegroundColor Yellow
} else {
    Write-Host "  → MIXED pattern — report the counts; partial repro" -ForegroundColor Yellow
}
if ($eDeaths -eq 0) { Write-Host "  → E alive: name/space NOT the killer" -ForegroundColor DarkGray }
if ($eDeaths -gt 0) { Write-Host "  → E died even renamed: name/space NOT the fix" -ForegroundColor DarkGray }

Write-Host "`n  Cleanup hint: Overlay\bin\DiagB1 / DiagE can be deleted anytime."
