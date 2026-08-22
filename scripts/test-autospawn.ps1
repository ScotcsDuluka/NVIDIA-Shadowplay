# test-autospawn.ps1 — EngineProcessSupervisor runtime validation (Windows)
#
# GPT spec (2026-08-23):
#   T0  Clean baseline (family processes gone, hub up)
#   T1  Engine absent  → Overlay starts → Capture.exe spawns → engine_ready → IPC works
#   T2  Engine running → Overlay keeps the SAME PID, no new instance
#   T3  Engine killed  → Supervisor detects → respawn per backoff → engine_ready again
#
# Lessons from run #1 (2026-08-23, FAIL):
#   - A stale pre-pull ShadowPlay survived cleanup (elevated? watchdog?) and the
#     Overlay is SingleInstance=true → the new launch FORWARDED to the old
#     no-supervisor instance and exited → no spawn, no engine_ready. Cleanup must
#     now (a) kill the whole app family, (b) WAIT until actually gone, (c) report
#     what it could not kill instead of silently continuing.
#   - T3 killed pseudo-process "Idle" because Get-EnginePid returned 0 → guarded.
#   - Supervisor diagnostics were Debug.WriteLine (invisible) → supervisor now
#     appends to %TEMP%\NVIDIA-Shadowplay-Supervisor.log; this script tails it.
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
$supLog  = Join-Path $env:TEMP "NVIDIA-Shadowplay-Supervisor.log"

# The whole app family — a surviving member can respawn or steal launches
$familyNames = @("NVIDIA ShadowPlay", "NVIDIA Capture", "NVIDIA API",
                 "NVIDIA Experience", "NVIDIA Notifier")

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

function Kill-Family {
    foreach ($n in $familyNames) {
        Get-Process -Name $n -ErrorAction SilentlyContinue | ForEach-Object {
            try   { $_ | Stop-Process -Force -ErrorAction Stop }
            catch { Write-Host ("  WARN cannot kill {0} pid {1}: {2}" -f $n, $_.Id, $_.Exception.Message) -ForegroundColor Yellow }
        }
    }
}

function Wait-FamilyGone([int]$TimeoutSec = 15) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $left = @()
        foreach ($n in $familyNames) {
            $left += @(Get-Process -Name $n -ErrorAction SilentlyContinue | ForEach-Object { "$n($($_.Id))" })
        }
        if ($left.Count -eq 0) { return $true }
        Start-Sleep -Milliseconds 500
    }
    return $false
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

<# Listen on the hub for a broadcast matching $Pattern (hub relays all lines). #>
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

# Fresh supervisor evidence for this run
try { Remove-Item $supLog -ErrorAction SilentlyContinue } catch { }

# ── T0a: clean baseline (kill family, WAIT until gone) ───────────
Write-Host "`n>>> Cleaning: closing app family (ShadowPlay/Capture/API/Experience/Notifier)..." -ForegroundColor Cyan
Kill-Family
$gone = Wait-FamilyGone 15
if (-not $gone) {
    Write-Host "  ERROR — processes still alive after 15s (elevated? watchdog?)" -ForegroundColor Red
    foreach ($n in $familyNames) {
        Get-Process -Name $n -ErrorAction SilentlyContinue |
            ForEach-Object { Write-Host ("    alive: {0} pid {1}" -f $n, $_.Id) -ForegroundColor Red }
    }
    Write-Host "  → close them manually (Task Manager) and re-run with -SkipBuild" -ForegroundColor Red
    exit 2
}
Check "T0 baseline: family clean" ($true)

# ── T0b: hub up (production: App Experience launcher owns it) ────
Write-Host "`n>>> Starting hub (NVIDIA API.exe)..." -ForegroundColor Cyan
Start-Process -FilePath $hubExe -WorkingDirectory $overlayBin
Check "T0 hub port 5000 ready" (Wait-Port 5000 20)

# ── T1: start ShadowPlay → engine spawns + engine_ready + IPC ────
Write-Host "`n>>> T1: starting ShadowPlay.exe (expect engine auto-spawn)..." -ForegroundColor Cyan
$spProc = Start-Process -FilePath $spExe -WorkingDirectory $overlayBin -PassThru
Start-Sleep -Seconds 4

if ($spProc.HasExited) {
    Write-Host ("  ERROR — new ShadowPlay exited immediately (pid {0})." -f $spProc.Id) -ForegroundColor Red
    Write-Host "  SingleInstance forwarding to a stale instance is the usual cause —" -ForegroundColor Red
    Write-Host "  but T0 says the family was clean. Check supervisor log below." -ForegroundColor Red
}
Check "T1 new ShadowPlay instance alive" (-not $spProc.HasExited)

$readyLine = Wait-HubBroadcast "engine_ready" 45
$t1pid = Get-EnginePid
Check "T1 engine spawned (pid $t1pid)" ($t1pid -ne 0)
Check "T1 engine_ready broadcast received via hub (IPC works)" ($null -ne $readyLine)
if ($readyLine) { Write-Host "      evidence: $readyLine" -ForegroundColor DarkGray }
Check "T1 exactly one engine" ((Count-Engine) -eq 1)

# ── T2: engine already running → same PID, no new instance ───────
Write-Host "`n>>> T2: supervisor must reuse the running engine (same PID)..." -ForegroundColor Cyan
$t2pidBefore = Get-EnginePid
Start-Sleep -Seconds 6
$t2pidAfter = Get-EnginePid
Check "T2 PID unchanged ($t2pidBefore -> $t2pidAfter)" ($t2pidBefore -ne 0 -and $t2pidBefore -eq $t2pidAfter)
Check "T2 still exactly one engine" ((Count-Engine) -eq 1)

# ── T3: kill engine → monitor respawns → engine_ready again ──────
Write-Host "`n>>> T3: killing engine (expect respawn per backoff + engine_ready)..." -ForegroundColor Cyan
$t3kill = Get-EnginePid
if ($t3kill -gt 0) {
    Get-Process -Id $t3kill -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "      killed engine pid $t3kill"
} else {
    Write-Host "      no engine PID to kill — T1/T2 already failed" -ForegroundColor Red
}

$readyLine3 = Wait-HubBroadcast "engine_ready" 45
$t3pid = Get-EnginePid
Check "T3 engine respawned with NEW pid ($t3kill -> $t3pid)" ($t3pid -ne 0 -and $t3pid -ne $t3kill)
Check "T3 engine_ready received again after respawn" ($null -ne $readyLine3)
if ($readyLine3) { Write-Host "      evidence: $readyLine3" -ForegroundColor DarkGray }
Check "T3 exactly one engine after respawn" ((Count-Engine) -eq 1)

# ── Supervisor evidence log ───────────────────────────────────────
if (Test-Path $supLog) {
    Write-Host "`n>>> Supervisor log ($supLog):" -ForegroundColor Cyan
    Get-Content $supLog | Select-Object -Last 30 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
} else {
    Write-Host "`n>>> Supervisor log MISSING ($supLog) — supervisor never ran!" -ForegroundColor Red
}

# ── Verdict ───────────────────────────────────────────────────────
Write-Host ""
Write-Host "=================================================="
Write-Host " AUTOSPAWN RESULT: $pass passed / $fail failed"
Write-Host "=================================================="

if (-not $KeepOpen) {
    Write-Host "`n>>> Cleaning up test processes..." -ForegroundColor Cyan
    Kill-Family
}

if ($fail -gt 0) { exit 1 } else { exit 0 }
