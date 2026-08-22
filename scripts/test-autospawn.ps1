# test-autospawn.ps1 — EngineProcessSupervisor runtime validation (Windows)
#
# GPT spec (2026-08-23):
#   T1  Engine absent  → Overlay starts → Capture.exe spawns → engine_ready → IPC works
#   T2  Engine running → Overlay keeps the SAME PID, no new instance
#   T3  Engine killed  → Supervisor detects → respawn per backoff → engine_ready again
#
# Hub note: the TCP hub is "NVIDIA API.exe" (spawned in production by the
# App Experience launcher). This harness starts/stops it itself so the
# three cases are measured in isolation.
#
# engine_ready evidence: we join the hub as an extra TCP client and listen
# for the Engine's broadcast (the hub relays every line to all clients).
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\test-autospawn.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\test-autospawn.ps1 -SkipBuild -KeepOpen

param(
    [switch]$SkipBuild,
    [switch]$KeepOpen
)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo
$overlayBin = Join-Path $repo "Overlay\bin\Release\net8.0-windows10.0.26100.0"

$spExe   = Join-Path $overlayBin "NVIDIA ShadowPlay.exe"
$capExe  = Join-Path $overlayBin "NVIDIA Capture.exe"
$hubExe  = Join-Path $overlayBin "NVIDIA API.exe"

if (-not $SkipBuild) {
    Write-Host ">>> Building solution..." -ForegroundColor Cyan
    & powershell -ExecutionPolicy Bypass -File "scripts\build-all.ps1" -Fast
    if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED" -ForegroundColor Red; exit 1 }
}

foreach ($f in @($spExe, $capExe, $hubExe)) {
    if (-not (Test-Path $f)) { Write-Host "Missing $f" -ForegroundColor Red; exit 1 }
}

function Count-Engine  { @(Get-Process -Name "NVIDIA Capture"  -ErrorAction SilentlyContinue).Count }
function Count-Overlay { @(Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue).Count }
function Get-EnginePid {
    $p = Get-Process -Name "NVIDIA Capture" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($p) { return $p.Id } else { return 0 }
}

function Wait-Port([int]$Port, [int]$TimeoutSec = 20) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $c = New-Object System.Net.Sockets.TcpClient
        try {
            $c.Connect("127.0.0.1", $Port)
            $c.Close()
            return $true
        } catch { Start-Sleep -Milliseconds 500 }
    }
    return $false
}

<#
    Listen on the hub for a broadcast line matching $Pattern (e.g. engine_ready).
    Connects as an extra client; the hub relays every received line to all peers.
#>
function Wait-HubBroadcast([string]$Pattern, [int]$TimeoutSec = 45) {
    try {
        $client = New-Object System.Net.Sockets.TcpClient
        $client.Connect("127.0.0.1", 5000)
        $stream = $client.GetStream()
        $stream.ReadTimeout = 1000
        $reader = New-Object System.IO.StreamReader($stream)
        $deadline = (Get-Date).AddSeconds($TimeoutSec)
        while ((Get-Date) -lt $deadline) {
            $line = $null
            try { $line = $reader.ReadLine() } catch { }
            if ($null -ne $line -and $line -match $Pattern) {
                $client.Close()
                return $line
            }
        }
        $client.Close()
    } catch { }
    return $null
}

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
Write-Host "`n>>> Cleaning: closing ShadowPlay / Capture / hub..." -ForegroundColor Cyan
Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "NVIDIA Capture"    -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "NVIDIA API"        -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Check "T0 baseline: nothing running" ((Count-Engine -eq 0) -and (Count-Overlay -eq 0))

# ── Hub up first (in production the App Experience launcher owns this) ──
Write-Host "`n>>> Starting hub (NVIDIA API.exe)..." -ForegroundColor Cyan
Start-Process -FilePath $hubExe -WorkingDirectory $overlayBin
Check "T0 hub port 5000 ready" (Wait-Port 5000 20)

# ── T1: start ShadowPlay → engine spawns + engine_ready + IPC ─────
Write-Host "`n>>> T1: starting ShadowPlay.exe (expect engine auto-spawn)..." -ForegroundColor Cyan
Start-Process -FilePath $spExe -WorkingDirectory $overlayBin

$readyLine = Wait-HubBroadcast "engine_ready" 45
$t1pid = Get-EnginePid
Check "T1 engine spawned (pid $t1pid)" ($t1pid -ne 0)
Check "T1 engine_ready broadcast received via hub (IPC works)" ($null -ne $readyLine)
if ($readyLine) { Write-Host "      evidence: $readyLine" -ForegroundColor DarkGray }
Check "T1 exactly one engine" ((Count-Engine) -eq 1)

# ── T2: engine already running → same PID, no new instance ────────
Write-Host "`n>>> T2: supervisor must reuse the running engine (same PID)..." -ForegroundColor Cyan
$t2pidBefore = Get-EnginePid
Start-Sleep -Seconds 6
$t2pidAfter = Get-EnginePid
Check "T2 PID unchanged ($t2pidBefore → $t2pidAfter)" ($t2pidBefore -ne 0 -and $t2pidBefore -eq $t2pidAfter)
Check "T2 still exactly one engine" ((Count-Engine) -eq 1)

# ── T3: kill engine → monitor respawns → engine_ready again ───────
Write-Host "`n>>> T3: killing engine (expect respawn per backoff + engine_ready)..." -ForegroundColor Cyan
$t2pid = Get-EnginePid
Get-Process -Id $t2pid -ErrorAction SilentlyContinue | Stop-Process -Force

$readyLine3 = Wait-HubBroadcast "engine_ready" 45
$t3pid = Get-EnginePid
Check "T3 engine respawned with NEW pid ($t2pid → $t3pid)" ($t3pid -ne 0 -and $t3pid -ne $t2pid)
Check "T3 engine_ready received again after respawn" ($null -ne $readyLine3)
if ($readyLine3) { Write-Host "      evidence: $readyLine3" -ForegroundColor DarkGray }
Check "T3 exactly one engine after respawn" ((Count-Engine) -eq 1)

# ── Verdict ───────────────────────────────────────────────────────
Write-Host ""
Write-Host "=================================================="
Write-Host " AUTOSPAWN RESULT: $pass passed / $fail failed"
Write-Host "=================================================="

if (-not $KeepOpen) {
    Write-Host "`n>>> Cleaning up test processes..." -ForegroundColor Cyan
    Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process -Name "NVIDIA Capture"    -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process -Name "NVIDIA API"        -ErrorAction SilentlyContinue | Stop-Process -Force
}

if ($fail -gt 0) { exit 1 } else { exit 0 }
