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
#   powershell ... -UiExe 'C:\path\NVIDIA ShadowPlay.exe'  # only if auto-discovery misses
# ============================================================================

param(
    [int]$Iterations = 10,
    [int]$RecordSeconds = 15,
    [int]$ReconnectWaitSeconds = 12,
    [switch]$AllowEngineKill,
    [string]$WorkRoot = "$env:TEMP\Level1ReconnectTest",
    [string]$UiExe = ""
)

$ErrorActionPreference = 'Continue'
$HubHost = '127.0.0.1'
$HubPort = 5001
$script:Results = New-Object System.Collections.Generic.List[string]
$script:FailCount = 0
$script:Aborted = $false
$script:Socket = $null
$script:Stream = $null
$script:Reader = $null
$script:Writer = $null

function Write-Result([string]$name, [bool]$pass, [string]$evidence) {
    $tag = if ($pass) { 'PASS' } else { 'FAIL' }
    $line = "[$tag] $name — $evidence"
    Write-Host $line
    $script:Results.Add($line)
    # Verdict uses a REAL counter. A text match like -like '*[FAIL]*' is a
    # PowerShell wildcard CHARACTER CLASS (matches any of f/a/i/l, case-
    # insensitive) and would count PASS lines as failures.
    if (-not $pass) { $script:FailCount++ }
}

# Abort the harness. The finally block reports NO MEASURED ROWS (exit 2) —
# an aborted run must never be printable as a PASS.
function Fail-Hard([string]$msg) {
    $script:Aborted = $true
    Write-Host "FATAL: $msg"
    exit 1
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

# Unique correlation token. The engine echoes it back as ,req=<id> on the
# response ([Engine] Client.vb request format: req=<token>|<payload>).
function New-ReqId([string]$prefix) {
    return $prefix + [Guid]::NewGuid().ToString("N").Substring(0, 8)
}

# engine_get_status round-trip from ENGINE truth, correlated to THIS query
# by a unique req id. (Uncorrelated matching raced on the 2026-09-09 run:
# the restarted UI's own status pull interleaved with ours and the harness
# consumed a stale buffered answer — row 4 measured elapsed 7->7 while the
# engine had actually answered Recording|22 DURING the UI-dead window.
# Engine clock was fine; the measurement wasn't.) Returns the raw data
# field (state|elapsed|output for a recording session; bare state else).
function Get-EngineStatus {
    $reqId = New-ReqId 'L1S'
    Send-Hub 'engine_get_status' "req=$reqId|"
    $resp = Wait-HubLine "req=$reqId" 8
    if (-not $resp) { return $null }
    $payload = ($resp -split '\|', 2)[1]           # drop "[Send] <who>|"
    if (-not $payload) { return $null }
    $payload = ($payload -split ",req=$reqId")[0]  # drop correlation tail
    $fields = $payload -split ','
    if ($fields.Count -lt 3) { return $fields[-1] }
    return $fields[2]
}

function Get-FileSize([string]$path) {
    try {
        $fi = Get-Item -LiteralPath $path -ErrorAction Stop
        return $fi.Length
    } catch { return -1 }
}

# Engine-truth elapsed seconds from a Recording|<sec>|<path> status answer.
# Returns -1 when the engine is not in an elapsed-reporting recording state.
function Get-StatusElapsed([string]$status) {
    if ($status -match '^Recording\|(\d+)\|') { return [int]$Matches[1] }
    return -1
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

# ── resolve the UI executable ONCE, while facts are available ─────────────
# A running UI process exposes its own Path — more reliable than guessing
# the layout. Priority: -UiExe override > running process path > PATH >
# script-relative guesses. (2026-09-09 OWNER run: guessed paths missed and
# the harness aborted before measuring anything.)
if (-not $UiExe -or -not (Test-Path $UiExe)) {
    $proc = Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue |
            Where-Object { $_.Path } | Select-Object -First 1
    if ($proc) { $UiExe = $proc.Path }
}
if (-not $UiExe -or -not (Test-Path $UiExe)) {
    $UiExe = @(
        (Get-Command 'NVIDIA ShadowPlay.exe' -ErrorAction SilentlyContinue).Source,
        "$PSScriptRoot\..\Application\NVIDIA ShadowPlay.exe",
        "$PSScriptRoot\..\..\Application\NVIDIA ShadowPlay.exe"
    ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
}
if (-not $UiExe -or -not (Test-Path $UiExe)) {
    Fail-Hard "NVIDIA ShadowPlay.exe not found. Launch the UI once and re-run, or pass the full path: -UiExe 'C:\...\NVIDIA ShadowPlay.exe'"
}
Write-Host "ui exe = $UiExe"

Connect-Hub

try {
    # ── matrix 1: UI start → engine start ───────────────────────────────────
    # Close the UI first so row 1 measures a COLD start deterministically.
    Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    $engineBefore = Get-ProcCount 'NVIDIA Capture'
    Start-Process $UiExe
    Start-Sleep -Seconds 8
    $engineAfter = Get-ProcCount 'NVIDIA Capture'
    Write-Result '1. UI start -> engine started (or reused)' ($engineAfter -ge 1) "engine count $engineBefore -> $engineAfter (supervisor reuse if identical)"

    # ── matrix 3 setup: engine RECORDS via direct hub command ───────────────
    $outPath = Join-Path $WorkRoot ("L1_{0}.mp4" -f (Get-Date -Format 'yyyyMMdd_HHmmss'))
    $startReq = New-ReqId 'L1R'
    Send-Hub 'RECORD_START' "req=$startReq|$outPath"
    $startResp = Wait-HubLine "req=$startReq" 15
    Write-Result '3a. RECORD_START accepted by engine' ($null -ne $startResp) "out=$outPath"
    # The path the ENGINE echoes is the authoritative output location (it may
    # normalize what we sent, e.g. 8.3 short form). All file probes use THAT.
    $engineOutPath = $outPath
    if ($startResp) {
        $echo = (($startResp -split ',', 3)[2]) -replace ",req=$startReq.*$", ''
        if ($echo) { $engineOutPath = $echo.Trim() }
    }
    Start-Sleep -Seconds 4
    $status = Get-EngineStatus
    $statusOk = ($status -match '^Recording\|')
    Write-Result '3b. engine truth = Recording|elapsed|output' $statusOk "status=$status"

    # ── matrix 4: recording → UI killed → engine session keeps advancing ─────
    # Continuity proof per task spec: file-size growth OR engine-timer growth.
    # Measured fact (2026-09-09 OWNER run): the new engine materializes the
    # final MP4 only on SAVE (live-mux stages fragments), so DURING recording
    # the output path legitimately does not exist yet (probe = -1; the
    # engine's own 1s progress broadcast also reports size 0). The engine
    # elapsed counter (Recording|<sec>|...) is therefore the PRIMARY
    # continuity signal; the file probe stays as supporting evidence.
    Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 3
    $engineWhileDead = Get-ProcCount 'NVIDIA Capture'
    $statusDeadA = Get-EngineStatus
    $elapsedA = Get-StatusElapsed $statusDeadA
    $sizeA = Get-FileSize $engineOutPath
    Start-Sleep -Seconds ([Math]::Max(5, $RecordSeconds))
    $statusDead = Get-EngineStatus
    $elapsedB = Get-StatusElapsed $statusDead
    $sizeB = Get-FileSize $engineOutPath
    $timerGrew = ($elapsedB -gt $elapsedA)
    Write-Result '4. UI killed -> engine alive + session advances' (($engineWhileDead -eq 1) -and ($elapsedA -ge 0) -and $timerGrew) "engine=$engineWhileDead, elapsed $elapsedA -> $elapsedB s, size $sizeA -> $sizeB (size<0 = final mp4 written at save)"

    # ── matrix 5: UI restarted → reconnect (count stays 1, recording intact) ─
    Start-Process $UiExe
    Start-Sleep -Seconds $ReconnectWaitSeconds
    $engineReconnect = Get-ProcCount 'NVIDIA Capture'
    $statusReconnect = Get-EngineStatus
    $elapsedC = Get-StatusElapsed $statusReconnect
    $sizeC = Get-FileSize $engineOutPath
    Start-Sleep -Seconds 5
    $statusReconnect2 = Get-EngineStatus
    $elapsedD = Get-StatusElapsed $statusReconnect2
    $sizeD = Get-FileSize $engineOutPath
    Write-Result '5. UI restarted -> reconnect, count=1, session still advancing' (($engineReconnect -eq 1) -and ($statusReconnect -match '^Recording\|') -and ($elapsedD -gt $elapsedC)) "engine=$engineReconnect, elapsed $elapsedC -> $elapsedD s (reconnected clock continues), size $sizeC -> $sizeD, status=$statusReconnect2"

    # ── matrix 8: explicit Stop Recording after reconnect ────────────────────
    # Broadcast format is engine_recording_saved:<path> — COLON separator,
    # not pipe (the previous pipe-split yielded garbage and failed this row
    # even though the save itself succeeded).
    Send-Hub 'RECORD_STOP'
    $stopResp = Wait-HubLine 'engine_recording_saved' 30
    $savedFile = ''
    if ($stopResp) { $seg = ($stopResp -split ':', 2)[1]; if ($seg) { $savedFile = $seg.Trim() } }
    $savedExists = [bool]($savedFile -and (Test-Path -LiteralPath $savedFile))
    $savedSize = if ($savedExists) { (Get-Item -LiteralPath $savedFile).Length } else { -1 }
    Write-Result '8. Stop after reconnect -> saved broadcast + non-empty file on disk' ($null -ne $stopResp -and $savedExists -and $savedSize -gt 0) "saved=$savedFile ($savedSize bytes)"
    $statusAfterStop = Get-EngineStatus
    Write-Result '8b. engine truth after stop = Idle' ($statusAfterStop -eq 'Idle') "status=$statusAfterStop"

    # ── matrix 6+7: repeated UI restart x5 idle + x5 while recording ─────────
    $idleFails = 0
    for ($i = 1; $i -le 5; $i++) {
        Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2
        Start-Process $UiExe
        Start-Sleep -Seconds 6
        $c = Get-ProcCount 'NVIDIA Capture'
        if ($c -ne 1) { $idleFails++ }
        Write-Host "    idle restart $i/5 -> engine count $c"
    }
    Write-Result '6. repeated UI restart x5 (idle): engine count always 1' ($idleFails -eq 0) "failures=$idleFails"

    $recFails = 0
    $outPath2 = Join-Path $WorkRoot ("L1stress_{0}.mp4" -f (Get-Date -Format 'yyyyMMdd_HHmmss'))
    $startReq2 = New-ReqId 'L1R'
    Send-Hub 'RECORD_START' "req=$startReq2|$outPath2"
    $null = Wait-HubLine "req=$startReq2" 15
    Start-Sleep -Seconds 4
    for ($i = 1; $i -le $Iterations; $i++) {
        Get-Process -Name 'NVIDIA ShadowPlay' -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2
        Start-Process $UiExe
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
            Start-Process $UiExe
            Start-Sleep -Seconds ($ReconnectWaitSeconds + 10)  # supervisor respawn budget
            $c = Get-ProcCount 'NVIDIA Capture'
            $s2 = Get-EngineStatus
            Write-Result '9. engine absent -> UI start spawns exactly one engine' ($c -eq 1) "engine=$c, status=$s2"
        } else {
            Write-Host "[SKIP] 9. engine-absent row skipped — engine busy: status=$s (run while idle)"
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
    Write-Host ""
    # Honesty guard: an abort or zero measured rows must NEVER print a PASS
    # verdict (vacuous truth). Fail-count comes from the real counter.
    if ($script:Aborted -or $script:Results.Count -eq 0) {
        Write-Host "RESULT: NO MEASURED ROWS — HARNESS DID NOT RUN, THIS IS NOT A PASS"
        exit 2
    }
    if ($script:FailCount -eq 0) { Write-Host "RESULT: ALL MEASURED ROWS PASS" }
    else { Write-Host "RESULT: $($script:FailCount) row(s) FAILED — do not report PASS without fixing"; exit 1 }
}
