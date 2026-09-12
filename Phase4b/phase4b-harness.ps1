# ============================================================================
# phase4b-harness.ps1 - M2-W2 Forensic Harness Engineer / Phase 4B A/B runs
# ============================================================================
# Mission: make Phase 4B (10ms timeline-delay ON/OFF A/B) reproducible.
#
# Per run (fresh recording, identical startup procedure, fixed duration/fps):
#   run -> collect -> verify MP4 -> verify runtime trace -> save config
#        -> deterministic evidence names
#
# Arms:
#   ON  = production build, timeline delay ENABLED  (CaptureSession.vb:279
#         `_timelineStartTicks = Stopwatch.GetTimestamp() + Stopwatch.Frequency \ 10`)
#   OFF = shadow build, timeline delay DISABLED. The one-line delay removal is
#         applied ONLY to a scratch copy of the source tree under %TEMP%
#         (phase4b-shadow). The production source file is never modified and
#         its SHA256 is verified unchanged before/after the whole harness.
#
# Evidence layout (deterministic):
#   evidence\phase4b\ON\run1..run3
#   evidence\phase4b\OFF\run1..run3
#   evidence\phase4b\harness-summary.json / harness-summary.md
#   evidence\phase4b\timeline-patch.txt
#   Per run: recording.mp4, runtime-trace.log, config.json, engine.json,
#            approot\ (isolated app config root), run-manifest.json,
#            mp4-verify.json, trace-verify.json
#
# Concurrency rules honored:
#   - Creates new files only (Phase4b\ + evidence\, which is gitignored).
#   - Does not rewrite any existing script (record-phase4b-*.bat at repo root
#     are STALE: they pass --duration/--output/--log/--evidence/--no-delay,
#     which ConsoleDriver Program.vb does not parse - left untouched).
#   - Does not modify production timing logic (shadow-copy patch only).
#   - Never commits.
# ============================================================================

param(
    [ValidateSet('ALL', 'ON', 'OFF')]
    [string]$Mode = 'ALL',
    [int]$RunsPerMode = 3,
    [int]$Seconds = 3,                     # identical for every run (Phase 4 A/B duration)
    [int]$Fps = 60,                        # target FPS, identical for every run
    [string]$EvidenceRoot = '',            # default: <repo>\evidence\phase4b
    [string]$FFmpeg = '',                  # default: <repo>\Overlay\API-Core\ffmpeg.exe
    [string]$ConsoleDriverExe = '',        # default: repo Release build (ON arm)
    [string]$ShadowDir = '',               # default: %TEMP%\phase4b-shadow
    [switch]$Force                         # wipe existing run dirs before re-running
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Paths / constants
# ---------------------------------------------------------------------------
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EvidenceRoot -or $EvidenceRoot -eq '') { $EvidenceRoot = Join-Path $repoRoot 'evidence\phase4b' }
if (-not $ShadowDir -or $ShadowDir -eq '')       { $ShadowDir = Join-Path ([System.IO.Path]::GetTempPath()) 'phase4b-shadow' }

$prodSessionVb = Join-Path $repoRoot 'CaptureEngine.Recording\CaptureSession.vb'
$prodDriverExe = Join-Path $repoRoot 'CaptureEngine.Recording.ConsoleDriver\bin\Release\net10.0-windows\CaptureEngine.Recording.ConsoleDriver.exe'
$prodDriverDll = Join-Path $repoRoot 'CaptureEngine.Recording\bin\Release\net10.0-windows\CaptureEngine.Recording.dll'

if (-not $FFmpeg -or $FFmpeg -eq '') {
    $candidate = Join-Path $repoRoot 'Overlay\API-Core\ffmpeg.exe'
    if (Test-Path $candidate) { $FFmpeg = $candidate } else { $FFmpeg = 'ffmpeg' }
}
$ffDir = Split-Path -Parent $FFmpeg
$ffprobe = Join-Path $ffDir 'ffprobe.exe'
if (-not (Test-Path $ffprobe)) { $ffprobe = 'ffprobe' }

# The EXACT production timeline-delay line (CaptureSession.vb:279). OFF arm
# removes the delay by deleting the additive term in the SHADOW COPY only.
$delayLineOld = '_timelineStartTicks = Stopwatch.GetTimestamp() + Stopwatch.Frequency \ 10'
$delayLineNew = '_timelineStartTicks = Stopwatch.GetTimestamp()'

# Project closure needed to build ConsoleDriver in the shadow tree.
$shadowProjects = @(
    'CaptureEngine',
    'CaptureEngine.Video',
    'CaptureEngine.Video.Ddagrab',
    'CaptureEngine.Encoder',
    'CaptureEngine.Encoder.Nvenc',
    'CaptureEngine.FFmpegBackend',
    'CaptureEngine.Audio',
    'CaptureEngine.Audio.Wasapi',
    'CaptureEngine.Recording',
    'CaptureEngine.Recording.ConsoleDriver',
    'Common'
)
# Linked Compile items from Engine\Engine (literal [ ] folder names).
$shadowLinkedEngineFiles = @(
    'Engine\Engine\[Capture]\CaptureSettings.vb',
    'Engine\Engine\[Integration]\OverlayConfig.vb',
    'Engine\Engine\[API]\NextRecordingConfig.vb'
)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function Get-Sha256([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Count-Ffmpeg {
    try { return @(Get-Process -Name ffmpeg -ErrorAction SilentlyContinue).Count } catch { return -1 }
}

function Invoke-LoggedProcess {
    param([string]$Exe, [string[]]$ArgList, [string]$LogPath, [hashtable]$Env = $null)

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $Exe
    $quoted = foreach ($a in $ArgList) { if ($a -match '\s') { '"' + $a + '"' } else { $a } }
    $psi.Arguments = ($quoted -join ' ')
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    $psi.CreateNoWindow = $true
    if ($Env) {
        foreach ($k in $Env.Keys) { [void]$psi.EnvironmentVariables.Remove($k); $psi.EnvironmentVariables.Add($k, [string]$Env[$k]) }
    }
    $proc = [System.Diagnostics.Process]::Start($psi)
    $outTask = $proc.StandardOutput.ReadToEndAsync()
    $errTask = $proc.StandardError.ReadToEndAsync()
    $proc.WaitForExit()
    $log = $outTask.Result + $errTask.Result
    $dir = Split-Path -Parent $LogPath
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [System.IO.File]::WriteAllText($LogPath, $log, [System.Text.Encoding]::UTF8)
    return @{ ExitCode = $proc.ExitCode; Log = $log }
}

function Write-Utf8([string]$Path, [string]$Text) {
    $dir = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($true)))
}

function Save-Json([string]$Path, $Object) {
    Write-Utf8 -Path $Path -Text (($Object | ConvertTo-Json -Depth 8) + "`r`n")
}

function New-HarnessConfigText {
    # Canonical, deterministic baseline config (production defaults, frozen).
    # Same composition as scripts\windows-phase1-video-validation.ps1 seeds:
    # unified config.json (OverlayConfig tier) + engine.json (CaptureSettings tier).
    param([int]$Fps, [int]$BitrateKbps, [int]$Preset, [int]$W, [int]$H)
    $unified = [ordered]@{
        Recording = [ordered]@{
            UseNativeResolution = $true;  Encoder = 'h264_nvenc'; EncoderNow = 'h264_nvenc'
            FPS = $Fps;                   Bitrate = $BitrateKbps; Width = $W; Height = $H
            Preset = 'CUSTOM';            EncoderPreset = $Preset; ReplayDuration = 60
            APICapture = 'ddagrab'
        }
        Audio = [ordered]@{
            SystemAudioEnabled = $true; MicEnabled = $false; SystemAudioVolume = 1.0
            MicVolume = 1.0; MicDeviceName = ''; MicDeviceId = ''; TrackMode = 0
            AudioClockMode = 'Legacy'
        }
        Paths = [ordered]@{}
    }
    $engine = [ordered]@{
        ConfigVersion = 3
        CaptureMethod = 'ddagrab'
        PixelFormat = 'nv12'
        Preset = ''
        RateControl = 'cbr'
        UseNativeResolution = $true
        CustomWidth = $W
        CustomHeight = $H
    }
    return @{
        ConfigJson = (($unified | ConvertTo-Json -Depth 6) + "`r`n")
        EngineJson = (($engine  | ConvertTo-Json -Depth 4) + "`r`n")
    }
}

function Assert-ProductionDriver {
    if (Test-Path -LiteralPath $prodDriverExe) { return }
    Write-Host "[harness] production driver missing - building (Release)..."
    $proj = Join-Path $repoRoot 'CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj'
    dotnet build $proj -c Release -v q --nologo
    if ($LASTEXITCODE -ne 0) { throw "production ConsoleDriver build FAILED (exit $LASTEXITCODE)" }
}

function Build-ShadowNoDelay {
    Write-Host "[harness] building shadow (timeline-delay OFF) tree at: $ShadowDir"
    if (Test-Path -LiteralPath $ShadowDir) {
        Remove-Item -LiteralPath $ShadowDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $ShadowDir | Out-Null

    # Root build files
    foreach ($f in @('Directory.Build.props', 'Directory.Build.targets')) {
        Copy-Item -LiteralPath (Join-Path $repoRoot $f) -Destination (Join-Path $ShadowDir $f) -Force
    }
    # Project closure (source only - bin/obj excluded)
    foreach ($p in $shadowProjects) {
        $src = Join-Path $repoRoot $p
        if (-not (Test-Path -LiteralPath $src)) { throw "shadow copy: missing project dir $p" }
        $dst = Join-Path $ShadowDir $p
        New-Item -ItemType Directory -Force -Path $dst | Out-Null
        robocopy $src $dst /E /XD bin obj /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -ge 8) { throw "robocopy failed for $p (code $LASTEXITCODE)" }
    }
    # Linked Engine sources (bracketed folder names -> -LiteralPath)
    foreach ($rel in $shadowLinkedEngineFiles) {
        $src = Join-Path $repoRoot $rel
        if (-not (Test-Path -LiteralPath $src)) { throw "shadow copy: missing linked source $rel" }
        $dst = Join-Path $ShadowDir $rel
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dst) | Out-Null
        Copy-Item -LiteralPath $src -Destination $dst -Force
    }

    # THE patch - applied to the shadow COPY only.
    $shadowSessionVb = Join-Path $ShadowDir 'CaptureEngine.Recording\CaptureSession.vb'
    $bytes = [System.IO.File]::ReadAllBytes($shadowSessionVb)
    $hasBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text = [System.IO.File]::ReadAllText($shadowSessionVb)
    $count = ([regex]::Matches($text, [regex]::Escape($delayLineOld))).Count
    if ($count -ne 1) { throw "shadow patch: expected exactly 1 delay line, found $count - aborting" }
    $patched = $text.Replace($delayLineOld, $delayLineNew)
    if (([regex]::Matches($patched, [regex]::Escape($delayLineOld))).Count -ne 0) { throw "shadow patch: old line still present" }
    if (([regex]::Matches($patched, [regex]::Escape($delayLineNew))).Count -ne 1) { throw "shadow patch: patched line count wrong" }
    $enc = New-Object System.Text.UTF8Encoding($hasBom)
    [System.IO.File]::WriteAllText($shadowSessionVb, $patched, $enc)

    $shadowProj = Join-Path $ShadowDir 'CaptureEngine.Recording.ConsoleDriver\CaptureEngine.Recording.ConsoleDriver.vbproj'
    $buildLog = Invoke-LoggedProcess -Exe 'dotnet' -ArgList @('build', $shadowProj, '-c', 'Release', '-v', 'q', '--nologo') -LogPath (Join-Path $EvidenceRoot 'shadow-build.log')
    if ($buildLog.ExitCode -ne 0) { throw "shadow build FAILED (exit $($buildLog.ExitCode)) - see evidence\phase4b\shadow-build.log" }

    $shadowDriverExe = Join-Path $ShadowDir 'CaptureEngine.Recording.ConsoleDriver\bin\Release\net10.0-windows\CaptureEngine.Recording.ConsoleDriver.exe'
    $shadowDriverDll = Join-Path $ShadowDir 'CaptureEngine.Recording\bin\Release\net10.0-windows\CaptureEngine.Recording.dll'
    if (-not (Test-Path -LiteralPath $shadowDriverExe)) { throw "shadow driver exe not found after build" }
    return @{
        Exe = $shadowDriverExe
        Dll = $shadowDriverDll
        RecordingDllSha = Get-Sha256 $shadowDriverDll
        PatchedFile = $shadowSessionVb
        Patch = @{ file = 'CaptureEngine.Recording\CaptureSession.vb (SHADOW COPY ONLY)'; old = $delayLineOld; new = $delayLineNew }
    }
}

function Verify-Mp4 {
    param([string]$Mp4Path, [int]$Seconds, [int]$Fps, [string]$LogPath)
    $result = @{ path = $Mp4Path; present = (Test-Path -LiteralPath $Mp4Path); verdict = 'FAIL'; facts = @{} }
    if (-not $result.present) {
        $result | Add-Member -NotePropertyName reason -NotePropertyValue 'recording.mp4 not produced'
        Save-Json $LogPath $result
        return $result
    }
    $json = & $ffprobe -v error -select_streams v:0 -show_entries stream=codec_name,width,height,avg_frame_rate -show_entries format=duration,size -of json $Mp4Path 2>$null
    $probe = $null
    try { $probe = ($json -join "`n") | ConvertFrom-Json } catch { }
    if ($null -eq $probe -or -not $probe.streams -or $probe.streams.Count -lt 1) {
        $result | Add-Member -NotePropertyName reason -NotePropertyValue 'ffprobe could not read video stream'
        Save-Json $LogPath $result
        return $result
    }
    $s = $probe.streams[0]
    $fpsVal = 0.0
    if ($s.avg_frame_rate -match '^(\d+)/(\d+)$') { $fpsVal = [math]::Round([double]$Matches[1] / [double]$Matches[2], 3) }
    $dur = 0.0; [void][double]::TryParse([string]$probe.format.duration, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$dur)
    $size = [long]$probe.format.size
    $fpsOk = ([math]::Abs($fpsVal - $Fps) -le 2.0)
    $durOk = ([math]::Abs($dur - $Seconds) -le 1.0)
    $checks = [ordered]@{
        codec_h264      = ($s.codec_name -eq 'h264')
        fps_within_tol  = $fpsOk
        duration_within_tol = $durOk
        size_nonzero    = ($size -gt 0)
    }
    $result.facts = [ordered]@{
        codec_name = [string]$s.codec_name; width = [int]$s.width; height = [int]$s.height
        avg_frame_rate = [string]$s.avg_frame_rate; duration_sec = $dur; size_bytes = $size
        checks = $checks
    }
    $allOk = $true; foreach ($v in $checks.Values) { if (-not $v) { $allOk = $false } }
    $result.verdict = if ($allOk) { 'PASS' } else { 'FAIL' }
    Save-Json $LogPath $result
    return $result
}

function Verify-Trace {
    param([string]$LogText, [string]$LogPath)
    $hasHeader = $LogText -match 'PHASE 1 VIDEO'
    $hasTimelineArmed = $LogText -match 'common timeline armed: T0=(\d+)'
    $hasCfrTick = $LogText -match 'CFR TICK 1 SELECTION DEBUG'
    $hasResult = $LogText -match 'pass:\s*(True|False)'
    $hasFatal = $LogText -match 'VIDEOCHECK FATAL|FATAL:'
    $hwBlocked = $LogText -match 'no NVIDIA adapter found'

    $t0 = $null; if ($LogText -match 'common timeline armed: T0=(\d+)') { $t0 = $Matches[1] }
    $tl = $null; if ($LogText -match 'timelineStartQpc100ns=(\d+)') { $tl = $Matches[1] }
    $fpsEcho = $null; if ($LogText -match "encoder='[^']*'\s+fps=(\d+)") { $fpsEcho = [int]$Matches[1] }
    $pass = $null; if ($LogText -match 'pass:\s*(True|False)') { $pass = ($Matches[1] -eq 'True') }
    $dur = $null; if ($LogText -match 'duration:\s*([0-9.]+)s') { $dur = [double]$Matches[1] }

    # A session that started must show the timeline markers; a session that
    # never started (hardware gate) is honestly recorded as NOT-RUN.
    $verdict = 'FAIL'
    if ($pass -eq $true -and $hasTimelineArmed -and $hasCfrTick) { $verdict = 'PASS' }
    elseif ($hwBlocked -or ($hasFatal -and -not $hasResult)) { $verdict = 'FAIL (session never started)' }

    $result = [ordered]@{
        verdict = $verdict
        markers = [ordered]@{
            videocheck_header = $hasHeader
            common_timeline_armed_T0 = $t0
            cfr_tick1_selection_debug = $hasCfrTick
            session_result_pass = $pass
            fatal_present = $hasFatal
            hardware_gate_no_nvidia_adapter = $hwBlocked
        }
        effective_startup_fps = $fpsEcho
        session_duration_sec = $dur
        timeline_start_qpc100ns = $tl
    }
    Save-Json $LogPath $result
    return $result
}

# ---------------------------------------------------------------------------
# Preflight
# ---------------------------------------------------------------------------
if (-not (Test-Path -LiteralPath $prodSessionVb)) { throw "production CaptureSession.vb not found: $prodSessionVb" }
$prodSessionVbShaBefore = Get-Sha256 $prodSessionVb
Assert-ProductionDriver
$prodDllSha = Get-Sha256 $prodDriverDll
if (-not $prodDllSha) { throw "production CaptureEngine.Recording.dll not found: $prodDriverDll" }

New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$hw = Get-CimInstance Win32_VideoController | Select-Object -ExpandProperty Name
$machineInfo = [ordered]@{
    machine = $env:COMPUTERNAME
    os = [string]$env:OS
    gpus = @($hw)
    dotnet = (dotnet --version 2>$null)
    ffmpeg = $FFmpeg
    ffmpeg_sha256 = Get-Sha256 $FFmpeg
}

$shadow = $null
if ($Mode -ne 'ON') { $shadow = Build-ShadowNoDelay }

$arms = @()
if ($Mode -eq 'ALL') { $arms = @('ON', 'OFF') } else { $arms = @($Mode) }

$results = New-Object System.Collections.Generic.List[object]

foreach ($arm in $arms) {
    if ($arm -eq 'ON') {
        $exe = $prodDriverExe; $dll = $prodDriverDll; $dllSha = $prodDllSha
        $patchInfo = $null
    } else {
        $exe = $shadow.Exe; $dll = $shadow.Dll; $dllSha = $shadow.RecordingDllSha
        $patchInfo = $shadow.Patch
    }
    $exeSha = Get-Sha256 $exe

    for ($i = 1; $i -le $RunsPerMode; $i++) {
        $runId = '{0}{1}' -f $arm, $i
        $runDir = Join-Path $EvidenceRoot (Join-Path $arm ('run{0}' -f $i))
        $mp4 = Join-Path $runDir 'recording.mp4'

        # Deterministic-name protection: refuse to clobber existing evidence.
        if ((Test-Path -LiteralPath $runDir) -and (Get-ChildItem -LiteralPath $runDir -Force | Select-Object -First 1)) {
            if ($Force) { Remove-Item -LiteralPath $runDir -Recurse -Force }
            else {
                Write-Host "[harness] $runId : evidence already exists - SKIPPED (use -Force to re-run)"
                $results.Add([pscustomobject]@{ runId = $runId; mode = $arm; verdict = 'SKIPPED'; reason = 'evidence exists; -Force not given' })
                continue
            }
        }
        New-Item -ItemType Directory -Force -Path $runDir | Out-Null

        $startedUtc = (Get-Date).ToUniversalTime().ToString('o')
        $orphBefore = Count-Ffmpeg

        # Save config (deterministic, identical for every run)
        $cfg = New-HarnessConfigText -Fps $Fps -BitrateKbps 20000 -Preset 4 -W 1920 -H 1080
        Write-Utf8 (Join-Path $runDir 'config.json') $cfg.ConfigJson
        Write-Utf8 (Join-Path $runDir 'engine.json') $cfg.EngineJson
        $appRoot = Join-Path $runDir 'approot'
        New-Item -ItemType Directory -Force -Path (Join-Path $appRoot 'Config') | Out-Null
        Write-Utf8 (Join-Path $appRoot 'Config\config.json') $cfg.ConfigJson

        # RUN - fresh process per run, isolated app root, fixed procedure
        $traceLog = Join-Path $runDir 'runtime-trace.log'
        $run = Invoke-LoggedProcess -Exe $exe -ArgList @(
            '--videocheck',
            '--config', (Join-Path $runDir 'engine.json'),
            '--out', $mp4,
            '--seconds', [string]$Seconds,
            '--ffmpeg', $FFmpeg
        ) -LogPath $traceLog -Env @{ NVIDIA_SHADOWPLAY_APP_ROOT = $appRoot }

        $endedUtc = (Get-Date).ToUniversalTime().ToString('o')
        $orphAfter = Count-Ffmpeg

        # COLLECT + VERIFY
        $mp4v = Verify-Mp4 -Mp4Path $mp4 -Seconds $Seconds -Fps $Fps -LogPath (Join-Path $runDir 'mp4-verify.json')
        $trv = Verify-Trace -LogText $run.Log -LogPath (Join-Path $runDir 'trace-verify.json')

        $modeVerified = ($dllSha -eq (Get-Sha256 $dll)) -and ($exeSha -eq (Get-Sha256 $exe))
        $prodIntact = (Get-Sha256 $prodSessionVb) -eq $prodSessionVbShaBefore

        $sessionOk = ($run.ExitCode -eq 0 -and $trv.markers.session_result_pass -eq $true)
        $verdict = 'PASS'
        $reasons = @()
        if (-not $sessionOk) {
            $verdict = 'FAIL'
            if ($trv.markers.hardware_gate_no_nvidia_adapter) { $reasons += 'MISSING PREREQUISITE: NVIDIA GPU (DdagrabBackend requires vendor 0x10DE; machine has Intel only)' }
            elseif ($run.ExitCode -ne 0) { $reasons += ("driver exit code {0}" -f $run.ExitCode) }
            else { $reasons += 'session result pass=False' }
        }
        if ($mp4v.verdict -ne 'PASS') { $verdict = 'FAIL'; $reasons += ('MP4 verify: ' + $mp4v.verdict) }
        if ($trv.verdict -notmatch '^PASS') { $verdict = 'FAIL'; $reasons += ('runtime trace verify: ' + $trv.verdict) }
        if (-not $modeVerified) { $verdict = 'FAIL'; $reasons += 'timeline mode binary hash mismatch' }
        if (-not $prodIntact) { $verdict = 'FAIL'; $reasons += 'PRODUCTION SOURCE CHANGED DURING RUN - harness integrity violation' }

        $manifest = [ordered]@{
            runId = $runId
            mode = $arm
            timelineMode = if ($arm -eq 'ON') { '10ms-timeline-delay-ENABLED (production baseline)' } else { '10ms-timeline-delay-DISABLED (shadow no-delay build)' }
            timelineModeVerifiedByBinaryHash = $modeVerified
            runIdentifier = ('phase4b/{0}/{1}/{2}' -f $arm, ("run{0}" -f $i), $runId)
            runDirectory = $runDir
            startedUtc = $startedUtc
            endedUtc = $endedUtc
            durationSecondsRequested = $Seconds
            targetFps = $Fps
            effectiveStartupFpsFromTrace = $trv.effective_startup_fps
            driverExecutable = $exe
            driverExecutableSha256 = $exeSha
            captureEngineRecordingDllSha256 = $dllSha
            timelinePatchAppliedToShadowOnly = $patchInfo
            productionCaptureSessionVbIntact = $prodIntact
            ffmpegPath = $FFmpeg
            orphanFfmpegBefore = $orphBefore
            orphanFfmpegAfter = $orphAfter
            exitCode = $run.ExitCode
            startStopResult = if ($trv.markers.session_result_pass -ne $null) { if ($trv.markers.session_result_pass) { 'PASS' } else { 'FAIL' } } else { 'NOT-RUN' }
            sessionDurationSec = $trv.session_duration_sec
            collected = @('recording.mp4', 'runtime-trace.log', 'config.json', 'engine.json', 'approot\Config\config.json', 'run-manifest.json', 'mp4-verify.json', 'trace-verify.json')
            mp4Verdict = $mp4v.verdict
            traceVerdict = ($trv.verdict -replace ' \(.*\)$', '')
            verdict = $verdict
            failureReasons = $reasons
        }
        Save-Json (Join-Path $runDir 'run-manifest.json') $manifest
        $results.Add([pscustomobject]$manifest)
        Write-Host ("[harness] {0}: {1}{2}" -f $runId, $verdict, $(if ($reasons.Count -gt 0) { ' - ' + ($reasons -join '; ') } else { '' }))
    }
}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
$prodSessionVbShaAfter = Get-Sha256 $prodSessionVb
$passCount = @($results | Where-Object { $_.verdict -eq 'PASS' }).Count
$failCount = @($results | Where-Object { $_.verdict -eq 'FAIL' }).Count
$skipCount = @($results | Where-Object { $_.verdict -eq 'SKIPPED' }).Count
$hwBlockedAny = @($results | Where-Object { $_.failureReasons -match 'NVIDIA GPU' }).Count -gt 0

$summaryJson = [ordered]@{
    harness = 'Phase4b M2-W2 forensic harness (timeline-delay ON/OFF A/B)'
    generatedUtc = (Get-Date).ToUniversalTime().ToString('o')
    invocation = ('powershell -NoProfile -ExecutionPolicy Bypass -File Phase4b\phase4b-harness.ps1 -Mode ALL -RunsPerMode 3 -Seconds ' + $Seconds + ' -Fps ' + $Fps)
    machine = $machineInfo
    runsRequested = $results.Count
    runsPassed = $passCount
    runsFailed = $failCount
    runsSkipped = $skipCount
    productionIntegrity = [ordered]@{
        captureSessionVbSha256Before = $prodSessionVbShaBefore
        captureSessionVbSha256After = $prodSessionVbShaAfter
        unchanged = ($prodSessionVbShaBefore -eq $prodSessionVbShaAfter)
    }
    shadowPatch = if ($shadow) { $shadow.Patch } else { $null }
    missingPrerequisites = @()
    runs = $results
}
if ($hwBlockedAny) {
    $summaryJson.missingPrerequisites += 'NVIDIA GPU machine required: DdagrabBackend initializes only on vendor 0x10DE and the session encoder is native NVENC. Run this harness on the GTX 1080 Ti machine for completing the 6 runs.'
}
Save-Json (Join-Path $EvidenceRoot 'harness-summary.json') $summaryJson

$md = New-Object System.Collections.Generic.List[string]
$md.Add('# Phase 4B harness summary (M2-W2)')
$md.Add('')
$md.Add('- Generated (UTC): ' + $summaryJson.generatedUtc)
$md.Add('- Production CaptureSession.vb unchanged: ' + $summaryJson.productionIntegrity.unchanged + " (sha256 $($summaryJson.productionIntegrity.captureSessionVbSha256After))")
$md.Add('- Target FPS: ' + $Fps + ' - Duration/run: ' + $Seconds + 's - Runs: ' + $results.Count + " (PASS $passCount / FAIL $failCount / SKIPPED $skipCount)")
if ($summaryJson.missingPrerequisites.Count -gt 0) {
    $md.Add('')
    $md.Add('## Missing prerequisites')
    foreach ($m in $summaryJson.missingPrerequisites) { $md.Add('- ' + $m) }
}
$md.Add('')
$md.Add('| Run | Mode | Start/Stop | MP4 | Trace | Verdict | Reason |')
$md.Add('|---|---|---|---|---|---|---|')
foreach ($r in $results) {
    $reason = ''
    if ($r.failureReasons -and $r.failureReasons.Count -gt 0) { $reason = ($r.failureReasons -join '; ') }
    $md.Add('| ' + $r.runId + ' | ' + $r.mode + ' | ' + $r.startStopResult + ' | ' + $r.mp4Verdict + ' | ' + $r.traceVerdict + ' | ' + $r.verdict + ' | ' + $reason + ' |')
}
Write-Utf8 (Join-Path $EvidenceRoot 'harness-summary.md') ($md -join "`r`n")

Write-Host ''
Write-Host ('[harness] DONE: PASS ' + $passCount + ' / FAIL ' + $failCount + ' / SKIPPED ' + $skipCount)
Write-Host ('[harness] summary: ' + (Join-Path $EvidenceRoot 'harness-summary.md'))
if ($failCount -gt 0 -or $skipCount -gt 0) { exit 1 } else { exit 0 }
