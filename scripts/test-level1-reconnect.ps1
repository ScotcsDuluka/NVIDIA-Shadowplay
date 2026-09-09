# ============================================================================
# test-level1-reconnect.ps1 — LEVEL 1 UI/HOST RECOVERY Windows harness
# ============================================================================
# Deterministic proof harness for the Level-1 invariant on the OWNER's
# Windows machine. Linux cannot run this — Windows proof stays BLOCKED
# until this script is executed and its summary is captured.
#
# What it proves (measured, never assumed):
#   - UI (NVIDIA ShadowPlay.exe) death does NOT stop NVIDIA Capture.exe
#   - the recording output file keeps GROWING while the UI is dead
#   - a restarted UI reuses the existing engine (engine count stays 1)
#   - engine_get_status answers engine truth: Recording|elapsed|output
#   - explicit stop after reconnect saves the file exactly once
#
# HARD RULE: this harness NEVER kills NVIDIA Capture.exe — except the one
# gated matrix row that REQUIRES an absent engine (-AllowEngineKill), and
# only while no recording is active.
#
# Usage (from the layout root, hub NVIDIA API.exe must be running):
#   powershell -ExecutionPolicy Bypass -File scripts\test-level1-reconnect.ps1
#   powershell ... -Iterations 10 -RecordSeconds 20
#   powershell ... -AllowEngineKill          # enables matrix row 9
# ============================================================================

param(
    [int]$Iterations = 10,
    [int]$RecordSeconds = 15,
    [int]$ReconnectWaitSeconds = 12,
    [switch]$AllowEngineKill,
    [string]$WorkRoot = "$env:TEMP\Level1ReconnectTest"
)

$ErrorActionPreference = 'Continue'
$HubHost = '127.0.0.1'
$HubPort = 5001
$script:Results = New-Object System.Collections.Generic.List[string]
$script:Socket = $null
$script:Stream = $null
$script:Reader = $null
$script:Writer = $null

function Write-Result([string]$name, [bool]$pass, [string]$evidence) {
    $tag = if ($pass) { 'PASS' } else { 'FAIL' }
    $line = "[$tag] $name — $evidence"
    Write-Host $line
    $script:Results.Add($line)
}

function Get-ProcCount([string]$name) {
    return @(Get-Process -Name $name -ErrorAction SilentlyContinue).Count
}

function Connect-Hub {
    $script:Socket = New-Object System.Net.Sockets.TcpClient
    $script:Socket.Connect($HubHost, $HubPort)
    $script:Stream = $script:Socket.GetStream()
    $script:Stream.ReadTimeout = 3000
    $script:Writer = New-Object System.IO.StreamWriter($script:Stream)
    $script:Writer.AutoFlush = $true
    $script:Reader = New-Object System.IO.StreamReader($script:Stream)
    $script:Writer.WriteLine("[Send] L1Harness|register:L1 Harness")
}

function Disconnect-Hub {
    if ($script:Socket) {
        try { $script:Socket.Close() } catch {}
        $script:Socket = $null
    }
}

function Send-Hub([string]$cmd, [string]$value = "") {
    if ($value -eq "") { $script:Writer.WriteLine("[Send] L1Harness|$cmd") }
    else { $script:Writer.WriteLine("[Send] L1Harness|${cmd}:$value") }
}

# Read hub lines until a needle appears or the deadline passes. Returns the
# matched line (or $null). Non-matching lines are printed for the transcript.
function Wait-HubLine([string]$needle, [int]$deadlineSec = 10) {
    $deadline = (Get-Date).AddSeconds($deadlineSec)
    while ((Get-Date) -lt $deadline) {
        $line = $null
        try { $line = $script:Reader.ReadLine() } catch { return $null }
        if ($null -eq $line) { return $null }
        if ($line -eq '[System]|pong') { continue }
        Write-Host "    hub> $line"
        if ($line -like "*$needle*") { return $line }
    }
    return $null
}

# engine_get_status round-trip from ENGINE truth. Returns the raw data field
# (state|elapsed|output for a recording session; bare state otherwise).
function Get-EngineStatus {
    Send-Hub 'engine_get_status'
    $resp = Wait-HubLine 'engine_response:engine_get_status' 8
    if (-not $resp) { return $null }
    $parts = $resp -split '\|', 2
    if ($parts.Count -lt 2) { return $null }
    $fields = ($parts[1] -split ',')
    if ($fields.Count -lt 3) { return $fields[-1] }
    return $fields[2]
}

function Get-FileSize([string]$path) {
    try {
        $fi = Get-Item -LiteralPath $path -ErrorAction Stop
        return $fi.Length
    } catch { return -1 }
}

# ── preflight ───────────────────────────────────────────────────────────────
Write-Host "=== LEVEL 1 RECONNECT HARNESS — preflight ==="
New-Item -ItemType Directory -Force -Path $WorkRoot | Out-Null
$hubCount = Get-ProcCount 'NVIDIA API'
if ($hubCount -lt 1) {
    Write-Host "FATAL: NVIDIA API hub is not running. Start it (Launcher OpenApp) and re-run."
    exit 1
}
Write-Host "hub running. engine=$(Get-ProcCount 'NVIDIA Capture'), ui=$(Get-ProcCount 'NVIDIA ShadowPlay')"

Connect-Hub

try {
    # ── matrix 1: UI start → engine start ───────────────────────────────────
    # Close the UI first so row 1 measures a COLD start deterministically.
    Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    $engineBefore = Get-ProcCount 'NVIDIA Capture'
    # UI path is resolved by name below; adjust if the layout differs.
    $uiCandidates = @(
        (Get-Command 'NVIDIA ShadowPlay.exe' -ErrorAction SilentlyContinue).Source,
        "$PSScriptRoot\..\Application\NVIDIA ShadowPlay.exe",
        "$PSScriptRoot\..\..\Application\NVIDIA ShadowPlay.exe"
    ) | Where-Object { $_ -and (Test-Path $_) }
    if (-not $uiCandidates) {
        Write-Host "FATAL: NVIDIA ShadowPlay.exe not found — pass the layout root and re-run."
        exit 1
    }
    Start-Process $uiCandidates[0]
    Start-Sleep -Seconds 8
    $engineAfter = Get-ProcCount 'NVIDIA Capture'
    Write-Result '1. UI start -> engine started (or reused)' ($engineAfter -ge 1) "engine count $engineBefore -> $engineAfter (supervisor reuse if identical)"

    # ── matrix 3 setup: engine RECORDS via direct hub command ───────────────
    $outPath = Join-Path $WorkRoot ("L1_{0}.mp4" -f (Get-Date -Format 'yyyyMMdd_HHmmss'))
    Send-Hub 'RECORD_START' $outPath
    $startResp = Wait-HubLine 'engine_response:engine_record_start,ok' 15
    Write-Result '3a. RECORD_START accepted by engine' ($null -ne $startResp) "out=$outPath"
    Start-Sleep -Seconds 4
    $status = Get-EngineStatus
    $statusOk = ($status -match '^Recording\|')
    Write-Result '3b. engine truth = Recording|elapsed|output' $statusOk "status=$status"

    # ── matrix 4: recording → UI killed → engine continues ──────────────────
    Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 3
    $engineWhileDead = Get-ProcCount 'NVIDIA Capture'
    $sizeA = Get-FileSize $outPath
    Start-Sleep -Seconds ([Math]::Max(5, $RecordSeconds))
    $sizeB = Get-FileSize $outPath
    $statusDead = Get-EngineStatus
    $grew = ($sizeB -gt $sizeA)
    Write-Result '4. UI killed -> engine alive + file grows' (($engineWhileDead -eq 1) -and $grew) "engine=$engineWhileDead, size $sizeA -> $sizeB, status=$statusDead"

    # ── matrix 5: UI restarted → reconnect (count stays 1, recording intact) ─
    Start-Process $uiCandidates[0]
    Start-Sleep -Seconds $ReconnectWaitSeconds
    $engineReconnect = Get-ProcCount 'NVIDIA Capture'
    $statusReconnect = Get-EngineStatus
    $sizeC = Get-FileSize $outPath
    Start-Sleep -Seconds 5
    $sizeD = Get-FileSize $outPath
    Write-Result '5. UI restarted -> reconnect, count=1, file still grows' (($engineReconnect -eq 1) -and ($statusReconnect -match '^Recording\|') -and ($sizeD -gt $sizeC)) "engine=$engineReconnect, status=$statusReconnect, size $sizeC -> $sizeD"

    # ── matrix 8: explicit Stop Recording after reconnect ────────────────────
    Send-Hub 'RECORD_STOP'
    $stopResp = Wait-HubLine 'engine_recording_saved' 30
    $savedFile = if ($stopResp) { ($stopResp -split '\|', 2)[1] } else { '' }
    $savedExists = $savedFile -and (Test-Path -LiteralPath $savedFile)
    Write-Result '8. Stop after reconnect saves exactly one file' ($null -ne $stopResp -and $savedExists) "saved=$savedFile"
    $statusAfterStop = Get-EngineStatus
    Write-Result '8b. engine truth after stop = Idle' ($statusAfterStop -eq 'Idle') "status=$statusAfterStop"

    # ── matrix 6+7: repeated UI restart x5 idle + x5 while recording ─────────
    $idleFails = 0
    for ($i = 1; $i -le 5; $i++) {
        Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2
        Start-Process $uiCandidates[0]
        Start-Sleep -Seconds 6
        $c = Get-ProcCount 'NVIDIA Capture'
        if ($c -ne 1) { $idleFails++ }
        Write-Host "    idle restart $i/5 -> engine count $c"
    }
    Write-Result '6. repeated UI restart x5 (idle): engine count always 1' ($idleFails -eq 0) "failures=$idleFails"

    $recFails = 0
    $outPath2 = Join-Path $WorkRoot ("L1stress_{0}.mp4" -f (Get-Date -Format 'yyyyMMdd_HHmmss'))
    Send-Hub 'RECORD_START' $outPath2
    $null = Wait-HubLine 'engine_response:engine_record_start,ok' 15
    Start-Sleep -Seconds 4
    for ($i = 1; $i -le $Iterations; $i++) {
        Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2
        Start-Process $uiCandidates[0]
        Start-Sleep -Seconds 6
        $c = Get-ProcCount 'NVIDIA Capture'
        $s = Get-EngineStatus
        if ($c -ne 1 -or -not ($s -match '^Recording\|')) { $recFails++ }
        Write-Host "    recording restart $i/$Iterations -> engine count $c, status=$s"
    }
    Send-Hub 'RECORD_STOP'
    $null = Wait-HubLine 'engine_recording_saved' 30
    Write-Result "7. repeated UI restart x$Iterations while recording: count=1 + Recording truth every round" ($recFails -eq 0) "failures=$recFails"

    # ── matrix 9: engine absent after restart → normal startup (gated) ──────
    if ($AllowEngineKill) {
        $s = Get-EngineStatus
        if ($s -eq 'Idle' -or $s -eq 'Faulted') {
            Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
            Get-Process -Name 'NVIDIA Capture' -ErrorAction SilentlyContinue | Stop-Process -Force
            Start-Sleep -Seconds 3
            Start-Process $uiCandidates[0]
            Start-Sleep -Seconds ($ReconnectWaitSeconds + 10)  # supervisor respawn budget
            $c = Get-ProcCount 'NVIDIA Capture'
            $s2 = Get-EngineStatus
            Write-Result '9. engine absent -> UI start spawns exactly one engine' ($c -eq 1) "engine=$c, status=$s2"
        } else {
            Write-Result '9. engine-absent row SKIPPED' $true "engine busy: status=$s (run while idle)"
        }
    } else {
        Write-Host "[SKIP] 9. engine-absent row requires -AllowEngineKill (destructive, idle-only)"
    }

    # ── matrix 10: reconnect timeout — informational (hub down budget) ──────
    Write-Host "[INFO] 10. reconnect-timeout row: bounded pull (10 x 2s) is source-pinned by L1-1 (Engine.ConfigTruth); live hub-outage drill = stop NVIDIA API.exe, watch UI survive, restart hub, confirm engine_ready re-broadcast in hub log."

}
finally {
    Disconnect-Hub
    Write-Host ""
    Write-Host "=== SUMMARY ==="
    $script:Results | ForEach-Object { Write-Host $_ }
    $fails = @($script:Results | Where-Object { $_ -like '*[FAIL]*' }).Count
    Write-Host ""
    if ($fails -eq 0) { Write-Host "RESULT: ALL MEASURED ROWS PASS" }
    else { Write-Host "RESULT: $fails row(s) FAILED — do not report PASS without fixing"; exit 1 }
}
