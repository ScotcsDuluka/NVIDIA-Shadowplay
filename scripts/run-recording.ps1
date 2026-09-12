<#
.SYNOPSIS
  W3 canonical recording runner - ONE command for a known, machine-readable recording.

  known binary  -> built fresh from pinned source (SHA256 recorded)
  known config  -> the driver's canonical settings, echoed in the result
  known FPS     -> strict --fps, capability-probed before any attempt
  known output  -> explicit absolute path, never overwritten without -Force
  known ffprobe -> pinned beside the verified ffmpeg, proven with -version
  result        -> result.json + one ##RUNRESULT## {json} line + exit code

  Exit codes:  0 = PASS   1 = FAIL   2 = BLOCKED (environment)   3 = usage/setup error

  Additive by design: no legacy script is modified or removed, no production
  code is touched. The M1/W3 audit findings this closes:
    unknown args    -> PowerShell strict binding + strict driver/gate parsers:
                       an unknown flag can never reach a binary silently
    silent-ignore   -> the driver and the analyzer reject unknown/duplicate
                       arguments with exit 2 and a machine-readable verdict
    stale scripts   -> this runner is the new entry point; it builds from
                       source by default (-NoBuild is explicit opt-out)
    wrong binary    -> every binary is resolved from pinned paths and hashed;
                       the driver also self-reports its SHA256 (cross-checked)
    wrong ffmpeg    -> ffmpeg/ffprobe are resolved from the product tree and
                       PROVEN runnable (-version) before anything records

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts\run-recording.ps1 -Fps 60
  powershell -ExecutionPolicy Bypass -File scripts\run-recording.ps1 -Fps 240                # BLOCKED if unsupported
  powershell -ExecutionPolicy Bypass -File scripts\run-recording.ps1 -Fps 240 -ForceAttempt  # run anyway -> FAIL with evidence
  powershell -ExecutionPolicy Bypass -File scripts\run-recording.ps1 -Fps 60 -Lane native    # NVIDIA-gated lane

  NOTE: this file is deliberately ASCII-only (PowerShell 5.1 reads BOM-less
  files as ANSI - the same encoding trap scripts/sync-verify.ps1 hit).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidateRange(1, 240)][int]$Fps,
    [ValidateRange(1, 600)][int]$Seconds = 5,
    [ValidateSet('legacy', 'native')][string]$Lane = 'legacy',
    [string]$OutDir = '',
    [string]$OutFile = '',
    [string]$Encoder = '',           # legacy lane only; empty = driver default (h264_qsv)
    [switch]$Force,                  # allow overwriting an existing output file
    [switch]$ForceAttempt,           # record even when the capability probe says the mode is unsupported
    [switch]$NoBuild                 # skip the pre-flight build (stale-binary risk is yours)
)

Set-StrictMode -Version Latest
# PS 5.1: native stderr merged via 2>&1 becomes an ErrorRecord; under 'Stop'
# that ABORTS the script mid-run (observed on the analyze step). This runner
# validates outcomes explicitly (exit codes + ##MARKER## lines), so 'Continue'
# is the correct posture; real failures still surface as FAIL verdicts.
$ErrorActionPreference = 'Continue'
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$utcStart = [DateTime]::UtcNow.ToString('o')

# -------------------------- helpers --------------------------
function Get-Sha256([string]$path) {
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant()
}

function Get-ToolVersion([string]$exe) {
    $line = (& $exe -version 2>&1 | Select-Object -First 1)
    return ([string]$line).Trim()
}

function Get-MarkerJson([string[]]$outputLines, [string]$marker) {
    $line = $outputLines | Where-Object { $_ -like "$marker*" } | Select-Object -First 1
    if (-not $line) { return $null }
    return (($line -replace "^$([regex]::Escape($marker))\s*", '') | ConvertFrom-Json)
}

function Finish([string]$verdict, [int]$code, $result) {
    $result.verdict = $verdict
    $result.timing.wallSec = [math]::Round($sw.Elapsed.TotalSeconds, 2)
    $result.timing.utcEnd = [DateTime]::UtcNow.ToString('o')
    if ($result.outFile -and (Test-Path -LiteralPath $result.outFile)) {
        $jsonPath = "$($result.outFile).result.json"
        $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8
        $result.resultJson = $jsonPath
    }
    $compact = ($result | ConvertTo-Json -Depth 8 -Compress)
    Write-Output "##RUNRESULT## $compact"
    Write-Output "GATE-STYLE VERDICT: $verdict (exit $code)"
    exit $code
}

function HardExit([string]$verdict, [int]$code, [string]$reason) {
    $mini = [ordered]@{ schema = 'canonical-run-v1'; verdict = $verdict; reason = $reason; utcStart = $utcStart }
    Write-Output ("##RUNRESULT## " + ($mini | ConvertTo-Json -Compress))
    Write-Output "GATE-STYLE VERDICT: $verdict (exit $code) - $reason"
    exit $code
}

# -------------------------- 1. layout --------------------------
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tfm  = 'net10.0-windows10.0.26100.0'

$driverProj  = Join-Path $root 'Tester\test\Engine\FpsMatrix\FpsMatrixDriver.vbproj'
$gateProj    = Join-Path $root 'Tester\test\Engine\TimingGate\Engine.TimingGate.Tests.vbproj'
$cdProj      = Join-Path $root 'CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj'
$driverExe   = Join-Path $root "Tester\test\Engine\FpsMatrix\bin\Debug\$tfm\FpsMatrixDriver.exe"
$gateExe     = Join-Path $root "Tester\test\Engine\TimingGate\bin\Debug\$tfm\Engine.TimingGate.Tests.exe"
$cdExe       = Join-Path $root 'CaptureEngine.Recording.ConsoleDriver\bin\Debug\net10.0-windows\CaptureEngine.Recording.ConsoleDriver.exe'

Write-Output "=== W3 canonical recording runner ==="
Write-Output " repo   : $root"
Write-Output " lane   : $Lane   fps: $Fps   seconds: $Seconds"

# -------------------------- 2. build known binaries from source --------------------------
if (-not $NoBuild) {
    Write-Output " build  : restoring known-binary state from source (use -NoBuild to skip)"
    $projects = @($driverProj, $gateProj)
    if ($Lane -eq 'native') { $projects += $cdProj }
    foreach ($proj in $projects) {
        $buildOk = $false
        $lastBuildLog = ''
        # two sequential builds share transitive outputs (Engine project);
        # a transient MSBuild file lock is possible - retry once with proof.
        for ($attempt = 1; $attempt -le 2 -and -not $buildOk; $attempt++) {
            if ($attempt -gt 1) { Start-Sleep -Seconds 3 }
            $buildLog = & dotnet build $proj -c Debug --nologo -v q 2>&1
            $lastBuildLog = ($buildLog | Out-String)
            if ($LASTEXITCODE -eq 0) { $buildOk = $true }
        }
        if (-not $buildOk) {
            $tail = ($lastBuildLog -split "`r?`n" | Where-Object { $_ -match '\S' } | Select-Object -Last 5) -join ' | '
            HardExit 'FAIL' 3 "build failed: $proj -> $tail"
        }
    }
    Write-Output " build  : OK"
} else {
    Write-Output " build  : SKIPPED (-NoBuild) - binaries must already exist"
}

# -------------------------- 3. known binary (pinned + hashed) --------------------------
$binaries = [ordered]@{}
$needed = @($gateExe)
if ($Lane -eq 'legacy') { $needed += $driverExe } else { $needed += $cdExe }
foreach ($exe in $needed) {
    if (-not (Test-Path -LiteralPath $exe)) {
        HardExit 'FAIL' 3 "known binary missing: $exe (build did not produce it)"
    }
}
$binaries.gate = [ordered]@{ path = $gateExe; sha256 = Get-Sha256 $gateExe }
if ($Lane -eq 'legacy') {
    $binaries.driver = [ordered]@{ path = $driverExe; sha256 = Get-Sha256 $driverExe }
} else {
    $binaries.consoleDriver = [ordered]@{ path = $cdExe; sha256 = Get-Sha256 $cdExe }
}
if ($Lane -eq 'legacy') {
    Write-Output " binary : $($binaries.driver.path)  sha256=$($binaries.driver.sha256)"
} else {
    Write-Output " binary : $($binaries.consoleDriver.path)  sha256=$($binaries.consoleDriver.sha256)"
}

# -------------------------- 4. known ffmpeg/ffprobe (proven runnable) --------------------------
$ffCandidates = @(
    (Join-Path $root 'Overlay\API-Core\ffmpeg.exe'),
    (Join-Path $root 'Overlay\bin\Release\net10.0-windows10.0.26100.0\FFmpeg\ffmpeg.exe')
)
$ffmpeg = $ffCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $ffmpeg) { HardExit 'BLOCKED' 2 'ffmpeg.exe not found in the product tree (Overlay API-Core | Overlay bin FFmpeg)' }
$ffprobe = Join-Path (Split-Path $ffmpeg -Parent) 'ffprobe.exe'
if (-not (Test-Path -LiteralPath $ffprobe)) { HardExit 'BLOCKED' 2 "ffprobe.exe not found next to $ffmpeg" }
try {
    $ffVersion = Get-ToolVersion $ffmpeg
    $fpVersion = Get-ToolVersion $ffprobe
} catch {
    HardExit 'BLOCKED' 2 "ffmpeg/ffprobe present but not runnable (-version failed): $($_.Exception.Message)"
}
if (-not $ffVersion -or -not $fpVersion) { HardExit 'BLOCKED' 2 'ffmpeg/ffprobe -version returned nothing (wrong/corrupt binary - the 2026-09-08 postmortem shape)' }
Write-Output " ffmpeg : $ffmpeg"
Write-Output "          $ffVersion"

# -------------------------- 5. capability probe (lane + mode) --------------------------
$probeOut = & $gateExe --probe --ffmpeg $ffmpeg 2>&1
$probe = Get-MarkerJson $probeOut '##PROBE##'
if (-not $probe) { HardExit 'FAIL' 3 'capability probe produced no ##PROBE## line (gate binary mismatch?)' }
Write-Output " probe  : qsv=$($probe.qsvAvailable) native=$($probe.nativeAvailable) modeSupported.$Fps=$($probe.modeSupported."$Fps")"

if ($Lane -eq 'legacy' -and -not $probe.qsvAvailable) {
    HardExit 'BLOCKED' 2 "legacy lane unavailable on this machine (see probe result)"
}
if ($Lane -eq 'native' -and -not $probe.nativeAvailable) {
    HardExit 'BLOCKED' 2 "native lane unavailable on this machine: $($probe.nativeReason)"
}
if ($Lane -eq 'legacy' -and -not $probe.modeSupported."$Fps" -and -not $ForceAttempt) {
    HardExit 'BLOCKED' 2 "target ${Fps}fps is not supported by this machine's encoder runtime (QsvGate probe). Use -ForceAttempt to record anyway and capture the failure as evidence."
}

# -------------------------- 6. known output (never overwrite silently) --------------------------
if (-not $OutDir) { $OutDir = Join-Path $root 'test-recordings\canonical' }
if (-not (Test-Path -LiteralPath $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
if (-not $OutFile) { $OutFile = "rec-$Lane-$($Fps)fps-$(Get-Date -Format 'yyyyMMdd-HHmmss').mp4" }
$outPath = if ([IO.Path]::IsPathRooted($OutFile)) { $OutFile } else { Join-Path $OutDir $OutFile }
if (Test-Path -LiteralPath $outPath) {
    if (-not $Force) { HardExit 'FAIL' 3 "output already exists (refusing overwrite without -Force): $outPath" }
    Remove-Item -LiteralPath $outPath -Force
}
Write-Output " output : $outPath"

# -------------------------- 7. record --------------------------
$runResult = $null
$runExit = -1

if ($Lane -eq 'legacy') {
    $runArgs = @('--fps', "$Fps", '--seconds', "$Seconds", '--out', $outPath, '--ffmpeg', $ffmpeg)
    if ($Encoder) { $runArgs += @('--encoder', $Encoder) }
    Write-Output " record : $($binaries.driver.path) $($runArgs -join ' ')"
    $runOutput = & $driverExe @runArgs 2>&1
    $runExit = $LASTEXITCODE
    $runOutput | ForEach-Object { Write-Output "   | $_" }
    $runResult = Get-MarkerJson $runOutput '##RESULT##'
    if (-not $runResult) { HardExit 'FAIL' 1 "driver produced no ##RESULT## line (unknown/old binary? exit=$runExit)" }
    # cross-check: the driver must be the binary we hashed
    if ($runResult.binary.sha256 -ne $binaries.driver.sha256) {
        HardExit 'FAIL' 1 "binary identity mismatch: driver self-reported sha $($runResult.binary.sha256) but runner hashed $($binaries.driver.sha256)"
    }
} else {
    # native lane: the production ConsoleDriver --videocheck path, with the
    # documented per-session FPS hook (--mismatch-fps: startup 60 -> session $Fps).
    Write-Output " record : $($binaries.consoleDriver.path) --videocheck --out `"$outPath`" --seconds $Seconds --mismatch-fps $Fps"
    $runOutput = & $cdExe --videocheck --out $outPath --seconds $Seconds --mismatch-fps $Fps 2>&1
    $runExit = $LASTEXITCODE
    $runOutput | ForEach-Object { Write-Output "   | $_" }
    $fileSize = -1
    if (Test-Path -LiteralPath $outPath) { $fileSize = (Get-Item -LiteralPath $outPath).Length }
    $fileExistsBool = (Test-Path -LiteralPath $outPath)
    $runResult = [pscustomobject]@{
        schema  = 'consoledriver-videocheck-v1'
        verdict = $(if ($runExit -eq 0) { 'RECORDED' } else { 'FAILED' })
        binary  = [pscustomobject]@{ path = $binaries.consoleDriver.path; sha256 = $binaries.consoleDriver.sha256 }
        settings = [pscustomobject]@{ lane = 'native'; startupFps = 60; sessionFps = $Fps; fpsSource = '--mismatch-fps (per-session NVENC rebuild)'; configChain = 'NextRecordingConfig.LoadEffectiveSettings -> MapStartupConfig -> Initialize -> StartSession' }
        engine  = [pscustomobject]@{ exitCode = $runExit }
        file    = [pscustomobject]@{ exists = $fileExistsBool; size = $fileSize }
    }
}

# -------------------------- 8. analyze (same analyzer as the gate) --------------------------
$analyzeJson = $null
$analyzeVerdict = 'FAIL'
$analyzeReason = ''
if (Test-Path -LiteralPath $outPath) {
    $analyzeOut = & $gateExe --analyze $outPath --fps $Fps --seconds $Seconds 2>&1
    $analyzeExit = $LASTEXITCODE
    $analyzeOut | ForEach-Object { Write-Output "   | $_" }
    $analyzeJson = Get-MarkerJson $analyzeOut '##PTSRESULT##'
    if ($analyzeJson) { $analyzeVerdict = $analyzeJson.verdict } else { $analyzeReason = "no ##PTSRESULT## line (exit=$analyzeExit)" }
} else {
    $analyzeReason = 'no output file to analyze'
}

# -------------------------- 9. verdict --------------------------
$engineErrors = @()
if ($Lane -eq 'legacy' -and $runResult.engine.errors) { $engineErrors = @($runResult.engine.errors) }
$driverOk = ($runResult.verdict -eq 'RECORDED') -and ($engineErrors.Count -eq 0)

$finalVerdict = 'FAIL'
$reason = ''
if ($analyzeVerdict -eq 'PASS' -and $driverOk) {
    $finalVerdict = 'PASS'
} elseif ($analyzeVerdict -eq 'PASS' -and -not $driverOk) {
    $reason = 'media valid but engine reported errors/failed stop'
} else {
    $aReason = ''
    if ($analyzeJson -and $analyzeJson.reason) { $aReason = $analyzeJson.reason }
    $reason = "analyzer=$analyzeVerdict $aReason $analyzeReason driverVerdict=$($runResult.verdict)".Trim()
}

$result = [ordered]@{
    schema   = 'canonical-run-v1'
    verdict  = $finalVerdict
    reason   = $reason
    lane     = $Lane
    run      = [ordered]@{ fps = $Fps; seconds = $Seconds; forceAttempt = [bool]$ForceAttempt }
    outFile  = $outPath
    binaries = $binaries
    ffmpeg   = [ordered]@{ path = $ffmpeg; version = $ffVersion }
    ffprobe  = [ordered]@{ path = $ffprobe; version = $fpVersion }
    probe    = $probe
    runResult = $runResult
    analyze  = $analyzeJson
    timing   = [ordered]@{ utcStart = $utcStart; wallSec = 0 }
    host     = [ordered]@{ machine = $env:COMPUTERNAME; user = $env:USERNAME; psVersion = $PSVersionTable.PSVersion.ToString() }
    resultJson = ''
}

if ($finalVerdict -eq 'PASS') { Finish 'PASS' 0 $result }
Finish 'FAIL' 1 $result
