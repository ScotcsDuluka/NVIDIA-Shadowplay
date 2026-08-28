# test-autospawn.ps1 v9 — CORRECTED after root cause found
#
# ROOT CAUSE (closed after 8 investigation runs):
#   The hub (NVIDIA API.exe) runs HandleAppsSmart() every 1 second:
#     Use_Overlay marker present → SPAWNS Notifier/ShadowPlay/Capture itself
#     Use_Overlay marker ABSENT   → KILLS all three every second (p.Kill)
#   The test bin had no marker (App Experience creates it in production) →
#   the hub murdered our test ShadowPlay.exe ~1s into host startup:
#   exit -1, no crash, no event log, trace truncated mid-deps-resolution.
#   This also means: IN PRODUCTION the hub is ALREADY the auto-spawner.
#   The Overlay-side EngineProcessSupervisor is effectively a fallback layer
#   (its respawn backoff 3s+ loses the race to the hub's 1s loop).
#
# Corrected test design (honest attribution):
#   PHASE S — Supervisor validation, hub DOWN (clean attribution: only the
#             Overlay's supervisor can spawn the engine here)
#   PHASE P — Production topology, hub UP + Use_Overlay marker created:
#             hub spawns everything; engine_ready via hub; kill → respawn
#             (whoever wins — hub or supervisor; supervisor log gives evidence)
#   Cleanup removes the marker (restores machine state).
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File scripts\test-autospawn.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\test-autospawn.ps1 -SkipBuild

param([switch]$SkipBuild)

$ErrorActionPreference = "Continue"
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo
$overlayBin = Join-Path $repo "Overlay\bin\Release\net10.0-windows10.0.26100.0"

$spExe   = Join-Path $overlayBin "NVIDIA ShadowPlay.exe"
$capExe  = Join-Path $overlayBin "NVIDIA Capture.exe"
$hubExe  = Join-Path $overlayBin "NVIDIA API.exe"
$marker  = Join-Path $overlayBin "Use_Overlay"
$supLog  = Join-Path $env:TEMP "NVIDIA-Shadowplay-Supervisor.log"

$familyNames = @("NVIDIA ShadowPlay", "NVIDIA Capture", "NVIDIA API",
                 "NVIDIA Experience", "NVIDIA Notifier")

if (-not $SkipBuild) {
    Write-Host ">>> Building solution (clean)..." -ForegroundColor Cyan
    & powershell -ExecutionPolicy Bypass -File "scripts\build-all.ps1"
    if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED" -ForegroundColor Red; exit 1 }
}

foreach ($f in @($spExe, $capExe, $hubExe)) {
    if (-not (Test-Path $f)) { Write-Host "Missing $f" -ForegroundColor Red; exit 1 }
}

function Count-Engine {
    @(Get-Process -Name "NVIDIA Capture" -ErrorAction SilentlyContinue).Count
}
function Get-EnginePid {
    $p = Get-Process -Name "NVIDIA Capture" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($p) { return $p.Id } else { return 0 }
}
function Get-OverlayPid {
    $p = Get-Process -Name "NVIDIA ShadowPlay" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($p) { return $p.Id } else { return 0 }
}

function Kill-Family {
    foreach ($n in $familyNames) {
        Get-Process -Name $n -ErrorAction SilentlyContinue | ForEach-Object {
            try   { $_ | Stop-Process -Force -ErrorAction Stop }
            catch { Write-Host ("  WARN cannot kill {0} pid {1}" -f $n, $_.Id) -ForegroundColor Yellow }
        }
    }
}

function Wait-Port([int]$Port, [int]$TimeoutSec = 20) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $c = New-Object System.Net.Sockets.TcpClient
        try { $c.Connect("127.0.0.1", $Port); $c.Close(); return $true }
        catch { Start-Sleep -Milliseconds 500 }
    }
    return $false
}

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
Write-Host " Auto-spawn validation v9 (post-root-cause)"
Write-Host " Phase S: supervisor only (hub DOWN)"
Write-Host " Phase P: production topology (hub UP + marker)"
Write-Host "=================================================="

# Fresh supervisor evidence for this run
try { Remove-Item $supLog -ErrorAction SilentlyContinue } catch { }

# ★ v9.2: the engine writes its own evidence log (BackgroundLogger flushes
# ~250ms) at {engine dir}\Logs\ui-engine.log. Capture its current line count
# so the report can show ONLY this run's lines — tells us exactly what the
# engine's TCP lifecycle did (start / reconnect / broadcast engine_ready).
$engineLog = Join-Path $overlayBin "Logs\ui-engine.log"
$engineLogLinesBefore = 0
if (Test-Path $engineLog) {
    $engineLogLinesBefore = @(Get-Content $engineLog -ErrorAction SilentlyContinue).Count
}

# ═════════ PHASE S — supervisor validation, hub DOWN ═════════
Write-Host "`n>>> S0: clean baseline (hub DOWN, family killed)..." -ForegroundColor Cyan
Remove-Item $marker -Force -ErrorAction SilentlyContinue   # no marker in phase S
Kill-Family
$leftover = @()
$clean = $false
$deadline = (Get-Date).AddSeconds(15)
while ((Get-Date) -lt $deadline) {
    $leftover = @()
    foreach ($n in $familyNames) {
        $leftover += @(Get-Process -Name $n -ErrorAction SilentlyContinue)
    }
    if ($leftover.Count -eq 0) { $clean = $true; break }
    Start-Sleep -Milliseconds 500
}
if (-not $clean) {
    foreach ($p in $leftover) {
        $pathx = "(path unavailable)"
        try { $pathx = $p.Path } catch { }
        Write-Host ("  still alive: {0} pid {1}  <-  {2}" -f $p.ProcessName, $p.Id, $pathx) -ForegroundColor Yellow
    }
    Write-Host "  (path tells us whether it's our bin or another deployment)" -ForegroundColor DarkGray
}
Check "S0 baseline: family clean" $clean

Write-Host "`n>>> S1: start ShadowPlay.exe (hub down) — supervisor must spawn the engine..." -ForegroundColor Cyan
Start-Process -FilePath $spExe -WorkingDirectory $overlayBin | Out-Null

$enginePid = 0
$deadline = (Get-Date).AddSeconds(25)
while ((Get-Date) -lt $deadline -and $enginePid -eq 0) {
    Start-Sleep -Milliseconds 500
    $enginePid = Get-EnginePid
}
Check "S1 Overlay alive (hub down — no watchdog kills it)" ((Get-OverlayPid) -ne 0)
Check "S1 engine spawned by SUPERVISOR (pid $enginePid)" ($enginePid -ne 0)

$supSpawned = $false
if (Test-Path $supLog) {
    $supSpawned = (Get-Content $supLog -Raw) -match "spawned engine pid"
}
Check "S1 supervisor log confirms spawn (attribution)" $supSpawned

Start-Sleep -Seconds 6
Check "S2 engine PID stable ($enginePid -> $(Get-EnginePid))" ((Get-EnginePid) -eq $enginePid)
Check "S2 exactly one engine" ((Count-Engine) -eq 1)

# ═════════ PHASE P — production topology, hub UP + marker ═════════
Write-Host "`n>>> P0: creating Use_Overlay marker + starting hub (production state)..." -ForegroundColor Cyan
"" | Out-File -FilePath $marker -Encoding ascii -NoNewline
Start-Process -FilePath $hubExe -WorkingDirectory $overlayBin | Out-Null
Check "P0 hub port 5000 ready" (Wait-Port 5000 20)

# ★ v9.1 race fix: attach the broadcast listener IMMEDIATELY after hub start.
# The old design connected the listener at P2 — but the engine (spawned in
# Phase S while the hub was DOWN) reconnects on its own backoff schedule
# (attempts at ~1/3/7/15/31s after ITS start). If its reconnect landed during
# P1's sleep window, engine_ready was broadcast and relayed BEFORE the P2
# listener ever joined — a hub is a live relay, not a message queue; late
# joiners never see earlier broadcasts. That was the P2 failure in run #9
# (10/11) — the test missed the message, the engine sent it.
# This job stays connected from hub-start through P4 and buffers matches.
Write-Host "  attaching broadcast listener (connected from hub start)..." -ForegroundColor DarkGray
$rxJob = Start-Job -ScriptBlock {
    param($port)
    try {
        $client = New-Object System.Net.Sockets.TcpClient("127.0.0.1", $port)
        $reader = New-Object System.IO.StreamReader($client.GetStream())
        while ($true) {
            $line = $reader.ReadLine()
            if ($null -eq $line) { break }
            if ($line -match "engine_ready") { Write-Output $line }
        }
    } catch { }
} -ArgumentList 5000

function Get-ReadyCount {
    @(Receive-Job -Job $rxJob -Keep 2>$null | Where-Object { $_ -match "engine_ready" }).Count
}
function Wait-NewReady([int]$BaselineCount, [int]$TimeoutSec) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $all = @(Receive-Job -Job $rxJob -Keep 2>$null | Where-Object { $_ -match "engine_ready" })
        if ($all.Count -gt $BaselineCount) { return $all[$all.Count - 1] }
        Start-Sleep -Milliseconds 500
    }
    return $null
}

Write-Host "`n>>> P1: hub watchdog should now keep the family alive..." -ForegroundColor Cyan
Start-Sleep -Seconds 5   # hub spawns missing family members within ~1s ticks
Check "P1 engine alive (hub-spawned or supervisor)" ((Get-EnginePid) -ne 0)

Write-Host "`n>>> P2: waiting for engine_ready (listener attached since hub start; 60s)..." -ForegroundColor Cyan
$ready1 = Wait-NewReady 0 60
Check "P2 engine_ready received (IPC works end-to-end)" ($null -ne $ready1)
if ($ready1) { Write-Host "      evidence: $ready1" -ForegroundColor DarkGray }

Write-Host "`n>>> P3: kill engine — expect respawn (hub 1s loop vs supervisor backoff)..." -ForegroundColor Cyan
$killPid = Get-EnginePid
if ($killPid -gt 0) {
    Get-Process -Id $killPid -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "      killed engine pid $killPid"
}
$beforeKillCount = Get-ReadyCount

$newPid = 0
$deadline = (Get-Date).AddSeconds(20)
while ((Get-Date) -lt $deadline -and $newPid -eq 0) {
    Start-Sleep -Milliseconds 500
    $newPid = Get-EnginePid
    if ($newPid -eq $killPid) { $newPid = 0 }   # not yet replaced
}
Check "P3 engine respawned with NEW pid ($killPid -> $newPid)" ($newPid -ne 0 -and $newPid -ne $killPid)

$ready2 = Wait-NewReady $beforeKillCount 60
Check "P4 engine_ready received again after respawn" ($null -ne $ready2)
if ($ready2) { Write-Host "      evidence: $ready2" -ForegroundColor DarkGray }

# Attribution evidence
Write-Host "`n>>> Supervisor log (attribution):" -ForegroundColor Cyan
if (Test-Path $supLog) {
    Get-Content $supLog | Select-Object -Last 15 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
} else {
    Write-Host "    (no supervisor log — supervisor never acted in this run)" -ForegroundColor DarkGray
}

# ★ v9.2: engine-side evidence — what did the engine's TCP actually do?
Write-Host "`n>>> Engine log (this run only — ui-engine.log):" -ForegroundColor Cyan
if (Test-Path $engineLog) {
    $newLines = @(Get-Content $engineLog | Select-Object -Skip $engineLogLinesBefore)
    if ($newLines.Count -eq 0) {
        Write-Host "    (no new engine log lines this run — engine log silent?)" -ForegroundColor Yellow
    } else {
        $tcpLines = @($newLines | Where-Object { $_ -match "TCP|reconnect|engine_ready|register|[Hh]ub|PREWARM" })
        Write-Host ("    {0} new lines total; TCP-lifecycle lines:" -f $newLines.Count) -ForegroundColor DarkGray
        if ($tcpLines.Count -gt 0) {
            $tcpLines | Select-Object -Last 30 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
        } else {
            Write-Host "    (none matched TCP/reconnect/engine_ready — showing last 10 lines for context)" -ForegroundColor Yellow
            $newLines | Select-Object -Last 10 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
        }
    }
} else {
    Write-Host "    (engine log not found at $engineLog)" -ForegroundColor Yellow
}

# ═════════ Cleanup (restore machine state) ═════════
Write-Host "`n>>> Cleanup: family down, hub down, marker removed..." -ForegroundColor Cyan
try { Stop-Job $rxJob -ErrorAction SilentlyContinue } catch { }
try { Remove-Job $rxJob -Force -ErrorAction SilentlyContinue } catch { }
Kill-Family
Start-Sleep -Seconds 1
Remove-Item $marker -Force -ErrorAction SilentlyContinue

# ═════════ Verdict ═════════
Write-Host ""
Write-Host "=================================================="
Write-Host " AUTOSPAWN RESULT: $pass passed / $fail failed"
Write-Host " Phase S = Overlay supervisor | Phase P = production topology"
Write-Host "=================================================="
if ($fail -gt 0) { exit 1 } else { exit 0 }
